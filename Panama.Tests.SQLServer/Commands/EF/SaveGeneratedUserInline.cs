
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Tests.SQLServer.Models;
using System.Data;
using System.Threading.Tasks;
using System.Transactions;

namespace Panama.Tests.SQLServer.Commands.EF
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
                Created = System.DateTime.Now
            };

            using (var connection = new SqlConnection(_config.GetConnectionString("MSSQL")))
            {
                if (connection.State == ConnectionState.Closed)
                    await connection.OpenAsync().ConfigureAwait(false);

                using var command = new SqlCommand($@"

                    INSERT INTO [dbo].[User]
                               ([ID]
                               ,[FirstName]
                               ,[LastName]
                               ,[Email]
                               ,[Created])
                         VALUES
                               (@Id
                               ,@FirstName
                               ,@LastName
                               ,@Email
                               ,@Created)
                    
                    SELECT SCOPE_IDENTITY()"

                , connection);

                command.Parameters.Add(new SqlParameter {
                    ParameterName = "@Id",
                    DbType = DbType.String,
                    Value = user.ID.ToString(),
                });
                command.Parameters.Add(new SqlParameter {
                    ParameterName = "@FirstName",
                    DbType = DbType.String,
                    Value = user.FirstName,
                });
                command.Parameters.Add(new SqlParameter {
                    ParameterName = "@LastName",
                    DbType = DbType.String,
                    Value = user.LastName,
                });
                command.Parameters.Add(new SqlParameter {
                    ParameterName = "@Email",
                    DbType = DbType.String,
                    Value = user.Created,
                });
                
                command.Parameters.Add(new SqlParameter {
                    ParameterName = "@Created",
                    DbType = DbType.DateTime,
                    Value = System.DateTime.Now,
                });

                var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

                connection.Close();

                var id = result.ToInt();

                context.Data.Add(user);
            }
        }
    }
}
