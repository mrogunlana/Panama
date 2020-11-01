using Panama.Core.Entities;
using Panama.Core.MySql.Dapper.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Panama.Core.MySql.Dapper.Interfaces
{
    public interface IDefinable
    {
        List<T> Get<T>(Definition definition);
        T GetSingle<T>(Definition definition);
        void Insert<T>(T obj, Definition definition) where T : class;
        void Update<T>(T obj, Definition definition) where T : class;
        void Save<T>(T obj, Definition definition) where T : class, IModel;
        bool Exist<T>(Definition definition) where T : class, IModel;
        void Delete<T>(T obj, Definition definition) where T : class, IModel;
        void Execute(Definition definition);
        T ExecuteScalar<T>(Definition definition);
    }
}
