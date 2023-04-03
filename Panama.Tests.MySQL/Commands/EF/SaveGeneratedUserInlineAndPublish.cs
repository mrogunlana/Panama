using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Panama.Canal.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Tests.MySQL.Models;
using System.Data;
using System.Threading.Tasks;

namespace Panama.Tests.MySQL.Commands.EF
{
    public class SaveGeneratedUserInlineAndPublish : ICommand
    {
        private readonly IConfiguration _config;
        private readonly IGenericChannelFactory _factory;

        public SaveGeneratedUserInlineAndPublish(
              IConfiguration config
            , IGenericChannelFactory factory)
        {
            _config = config;
            _factory = factory;
        }
        public async Task Execute(IContext context)
        {
            var user = new User {
                ID = System.Guid.NewGuid().ToString(),
                Email = $"test.{new System.Random().Next()}",
                FirstName = $"first.{new System.Random().Next()}",
                LastName = $"last.{new System.Random().Next()}",
                Created = System.DateTime.Now
            };

            using (var connection = new MySqlConnection(_config.GetConnectionString("MYSQL")))
            using (var channel = _factory.CreateChannel<IDbConnection, IDbTransaction>(connection, context.Token))
            {
                using var command = new MySqlCommand($@"

                    INSERT INTO `devdb`.`User`
                    (`Id`,
                    `FirstName`,
                    `LastName`,
                    `Email`,
                    `Created`)
                    VALUES
                    (@Id,
                     @FirstName,
                     @LastName,
                     @Email,
                     @Created);

                     SELECT LAST_INSERT_ID();"

                , connection);

                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Id",
                    DbType = DbType.String,
                    Value = user.ID.ToString(),
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@FirstName",
                    DbType = DbType.String,
                    Value = user.FirstName,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@LastName",
                    DbType = DbType.String,
                    Value = user.LastName,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Email",
                    DbType = DbType.String,
                    Value = user.Created,
                });
                command.Parameters.Add(new MySqlParameter {
                    ParameterName = "@Created",
                    DbType = DbType.DateTime,
                    Value = System.DateTime.Now,
                });

                var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

                var id = result.ToInt();

                user._ID = id;

                await channel.Post(
                    name: "foo.event",
                    group: "foo",
                    data: user,
                    ack: "foo.event.success",
                    nack: "foo.event.failed").ConfigureAwait(false);

                await channel.Commit();
                
                connection.Close();

                context.Data.Add(user);
            }
        }
    }
}
