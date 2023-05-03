using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Canal.Tests.MySQL.Models;
using System.Data;
using System.Threading.Tasks;

namespace Panama.Canal.Tests.MySQL.Commands.EF
{
    public class SaveGeneratedUserInline : ICommand
    {
        private readonly IConfiguration _config;

        public SaveGeneratedUserInline(IConfiguration config)
        {
            _config = config;
        }
        public async Task Execute(IContext context)
        {
            var user = new User {
                ID = System.Guid.NewGuid().ToString(),
                Email = $"test.{new System.Random().Next()}",
                FirstName = $"first.{new System.Random().Next()}",
                LastName = $"last.{new System.Random().Next()}",
                Created = System.DateTime.UtcNow
            };

            using (var connection = new MySqlConnection(_config.GetConnectionString("MYSQL")))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

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
                    Value = System.DateTime.UtcNow,
                });

                var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

                connection.Close();

                var id = result.ToInt();

                context.Data.Add(user);
            }
        }
    }
}
