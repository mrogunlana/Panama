using Panama.Interfaces;
using System;

namespace Panama.Tests.SQLServer.Models
{
    public class Setting : IModel
    {
        public Setting()
        {
            if (Created == DateTime.MinValue)
                Created = DateTime.UtcNow;
        }
        public int _ID { get; set; }
        public Guid ID { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }
        public DateTime Created { get; set; }
    }
}
