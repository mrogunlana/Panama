using Panama.Core.Entities;
using System.Collections.Generic;

namespace Panama.Core.Sql
{
    public interface IQuery
    {
        List<T> Get<T>(string sql, object parameters);
        List<T> Get<T>(string connection, string sql, object parameters);
        void Insert<T>(T obj) where T : class;
        void Insert<T>(string connection, T obj) where T : class;
        void Update<T>(T obj) where T : class;
        void Update<T>(string connection, T obj) where T : class;
        void Save<T>(T obj, object parameters) where T : class, IModel;
        void Save<T>(string connection, T obj, object parameters) where T : class, IModel;
        bool Exist<T>(string sql, object parameters) where T : class, IModel;
        bool Exist<T>(string connection, string sql, object parameters) where T : class, IModel;
        T GetSingle<T>(string sql, object parameters);
        T GetSingle<T>(string connection, string sql, object parameters);
        void Delete<T>(T obj) where T : class, IModel;
        void Delete<T>(string connection, T obj) where T : class, IModel;
        void Execute(string sql, object parameters);
        void Execute(string connection, string sql, object parameters);
        T ExecuteScalar<T>(string sql, object parameters);
        T ExecuteScalar<T>(string connection, string sql, object parameters);
        void InsertBatch<T>(List<T> models, int batch = 0) where T : class, IModel;
        void InsertBatch<T>(string connection, List<T> models, int batch = 0) where T : class, IModel;
    }
}
