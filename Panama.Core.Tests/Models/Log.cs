using Panama.Core.Entities;
using System;

namespace Panama.Core.Tests.Models
{
    public class Log : IModel
    {
        public int _ID { get; set; }
        public string MachineName { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string CallSite { get; set; }
        public string Logger { get; set; }
        public DateTime Logged { get; set; }

    }
}
