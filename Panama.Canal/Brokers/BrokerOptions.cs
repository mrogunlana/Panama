﻿using Panama.Canal.Brokers.Interfaces;

namespace Panama.Canal.Brokers
{
    public class BrokerOptions : IBrokerOptions
    {
        public bool Default { get; set; } = true;
        public string Exchange { get; set; } = "panama.default.router";
    }
}
