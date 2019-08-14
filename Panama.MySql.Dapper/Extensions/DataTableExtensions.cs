using Panama.Core.Entities;
using Panama.Core.IoC;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Panama.MySql.Dapper
{
    public static class DataTableExtensions
    {
        /// <summary>
        /// Converts a DataTable to a IModel list. 
        /// </summary>
        /// <typeparam name="T">IModel</typeparam>
        /// <param name="data">DataTable</param>
        /// <returns>List<T></returns>
        public static List<T> ToList<T>(this DataTable data) where T : class, IModel
        {
            var result = new List<T>();
            if (data == null)
                return result;

            var type = typeof(T);
            var properties = type.GetProperties();
            if (properties.Count() == 0)
                return result;

            var columns = data.Columns.Cast<DataColumn>().ToList();
            foreach (DataRow row in data.Rows)
            {
                T model = ServiceLocator.Current.Resolve<T>();
                foreach (PropertyInfo property in properties)
                {
                    DataColumn column = columns.Find(col => col.ColumnName == property.Name);
                    if (column == null)
                        continue;

                    property.SetValue(model, row[column]);
                }
                result.Add(model);
            }
            return result;
        }
    }
}
