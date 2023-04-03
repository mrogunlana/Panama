using Microsoft.EntityFrameworkCore.Storage;
using MySqlConnector;
using System.Data;
using System.Data.Common;

namespace Panama.Canal.MySQL.Extensions
{
    internal static class TransactionExtensions
    {
        internal static DbConnection? GetConnection(this object transaction)
        {
            if (transaction == null)
                return null;
            if (transaction is not DbTransaction)
                return null;
            
            return ((DbTransaction)transaction)?.Connection;
        }
    }
}
