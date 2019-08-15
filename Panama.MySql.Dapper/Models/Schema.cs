using Panama.Core.Entities;
using System;

namespace Panama.Core.MySql.Dapper.Models
{
    public class Schema : IModel
    {
        public string Name { get; set; }
        public string ColumnName { get; set; }
        public Type Type { get; set; }
        public object Size { get; set; }
        public string DataType { get; set; }
    }
}
