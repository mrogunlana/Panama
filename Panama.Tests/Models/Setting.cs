using Panama.Interfaces;
using System;

namespace Panama.Tests.Models
{
    public class Setting : IModel
    {
        public Setting()
        {
            if (Created == DateTime.MinValue)
                Created = DateTime.Now;
        }
        public int _ID { get; set; }
        public Guid ID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime Created { get; set; }
    }
}
