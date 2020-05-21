using Panama.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Panama.Core.Sql
{
    public interface IQueryAsync
    {
        Task<List<T>> GetAsync<T>(string sql, object parameters);
        Task<List<T>> GetAsync<T>(string connection, string sql, object parameters);
        Task InsertAsync<T>(T obj) where T : class;
        Task InsertAsync<T>(string connection, T obj) where T : class;
        Task UpdateAsync<T>(T obj) where T : class;
        Task UpdateAsync<T>(string connection, T obj) where T : class;
        Task SaveAsync<T>(T obj, object parameters) where T : class, IModel;
        Task SaveAsync<T>(string connection, T obj, object parameters) where T : class, IModel;
        Task<bool> ExistAsync<T>(string sql, object parameters) where T : class, IModel;
        Task<bool> ExistAsync<T>(string connection, string sql, object parameters) where T : class, IModel;
        Task<T> GetSingleAsync<T>(string sql, object parameters);
        Task<T> GetSingleAsync<T>(string connection, string sql, object parameters);
        Task DeleteAsync<T>(T obj) where T : class, IModel;
        Task DeleteAsync<T>(string connection, T obj) where T : class, IModel;
        Task ExecuteAsync(string sql, object parameters);
        Task ExecuteAsync(string connection, string sql, object parameters);
        Task<T> ExecuteScalarAsync<T>(string sql, object parameters);
        Task<T> ExecuteScalarAsync<T>(string connection, string sql, object parameters);
        Task InsertBatchAsync<T>(List<T> models, int batch = 0) where T : class, IModel;
        Task InsertBatchAsync<T>(string connection, List<T> models, int batch = 0) where T : class, IModel;
    }
}
