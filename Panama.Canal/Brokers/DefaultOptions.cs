using Panama.Canal.Brokers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panama.Canal.Brokers
{
    public class DefaultOptions : IBrokerOptions
    {
        public bool Default { get; set; } = true;
        public string Exchange { get; set; } = "panama.default.router";
    }
}
