using MySqlConnector;
using System.Data.Common;

namespace Panama.Canal.MySQL.Extensions
{
    internal static class TransactionExtensions
    {
        internal static MySqlConnection? GetConnection(this object transaction)
        {
            if (transaction == null)
                return null;
            if (transaction is not DbTransaction)
                return null;

            var result = transaction as DbTransaction;
            
            return (MySqlConnection)result?.Connection!;
        }
    }
}
