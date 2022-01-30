using CsvHelper.Configuration.Attributes;
using Panama.Core.Entities;
using System;

namespace Panama.Core.Tests.Models
{
    public class Csv : IModel
    {
        [Ignore]
        public long _ID { get; set; }

        [Name("seq")]
        public long ID { get; set; }

        [Name("first")]
        public string First { get; set; }

        [Name("last")]
        public string Last { get; set; }

        [Name("age")]
        public int Age { get; set; }

        [Name("street")]
        public string Street { get; set; }

        [Name("city")]
        public string City { get; set; }

        [Name("state")]
        public string State { get; set; }

        [Name("zip")]
        public string Zip { get; set; }

        [Name("dollar")]
        public string Dollar { get; set; }

        [Name("pick")]
        public string Pick { get; set; }

        [Name("date")]
        public DateTime Date { get; set; }

    }
}
