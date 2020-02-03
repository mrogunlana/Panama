using Panama.Core.Entities;
using System.Collections.Generic;

namespace Panama.Core.Sql
{
    public interface IQueryAsync
    {
        List<T> GetAsync<T>(string sql, object parameters);
        void InsertAsync<T>(T obj) where T : class;
        void UpdateAsync<T>(T obj) where T : class;
        void SaveAsync<T>(T obj, object parameters) where T : class, IModel;
        bool ExistAsync<T>(string sql, object parameters) where T : class, IModel;
        T GetSingleAsync<T>(string sql, object parameters);
        void DeleteAsync<T>(T obj) where T : class, IModel;
        void ExecuteAsync(string sql, object parameters);
        T ExecuteScalarAsync<T>(string sql, object parameters);
        void InsertBatchAsync<T>(List<T> models, int batch = 0) where T : class, IModel;
    }
}
