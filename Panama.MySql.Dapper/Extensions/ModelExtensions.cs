using DapperExtensions.Mapper;
using DapperExtensions.Sql;
using Panama.Core.Entities;
using Panama.Core.MySql.Dapper.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Panama.Core.MySql.Dapper
{
    public static class ModelExtensions
    {
        private static readonly ISqlGenerator _sql = new SqlGeneratorImpl(new DapperExtensions.DapperExtensionsConfiguration());

        /// <summary>
        /// Converts generic List/Enumerable of IModel to DataTable
        /// Reference: https://stackoverflow.com/questions/564366/convert-generic-list-enumerable-to-datatable
        /// </summary>
        /// <typeparam name="T">IModel</typeparam>
        /// <param name="data">Models</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable<T>(this IList<T> data) where T: class, IModel
        {
            var map = _sql?.Configuration?.GetMap<T>();
            if (map == null)
                throw new Exception($"Class Map for:{typeof(T).Name} could not be found.");
            
            var properties = TypeDescriptor.GetProperties(typeof(T)).Cast<PropertyDescriptor>();
            var parameters = map.Properties
                            .Where(x => x.Ignored == false)
                            .Where(x => x.IsReadOnly == false)
                            .Where(x => x.KeyType == KeyType.NotAKey);

            var schema = (from p in properties
                          join o in parameters on p.Name equals o.ColumnName
                          select new Schema() {
                              Name = p.Name,
                              ColumnName = o.ColumnName,
                              Type = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType
                          }).ToList();

            DataTable table = new DataTable();
            foreach (var property in schema)
                table.Columns.Add(property.Name, property.Type);

            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (var property in schema)
                    row[property.ColumnName] = properties
                        ?.FirstOrDefault(x => x.Name == property.Name)
                        ?.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        public static void SetKey<T>(this T model, object key) where T : class
        {
            var map = _sql?.Configuration?.GetMap<T>();
            if (map == null)
                throw new Exception($"Class Map for:{typeof(T).Name} could not be found.");

            var identity = map.Properties
                .Where(x => x.KeyType == KeyType.Identity)
                .FirstOrDefault();

            if (identity == null)
                return;

            var property = model.GetType().GetProperty(identity.PropertyInfo.Name, BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
                return;
            if (!property.CanWrite)
                return;

            property.SetValue(model, Convert.ChangeType(key, identity.PropertyInfo.PropertyType));
        }
    }
}
