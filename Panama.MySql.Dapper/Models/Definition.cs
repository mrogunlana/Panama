using DapperExtensions;
using Panama.Core.Entities;
using System.Collections.Generic;
using System.Threading;

namespace Panama.Core.MySql.Dapper.Models
{
    public class Definition : IModel
    {
        public Definition()
        {
            if (Dictionary == null)
                Dictionary = new Dictionary<string, object>();
        }
        public string Connection { get; set; }
        public string Sql { get; set; }
        public object Parameters { get; set; }
        public int? CommandTimeout { get; set; }
        public CancellationToken Token { get; set; }
        public IPredicate Predicate { get; set; }
        public IDictionary<string, object> Dictionary { get; set; }
    }
}
