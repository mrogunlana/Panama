using Panama.Core.Entities;
using System;

namespace Panama.Core.Tests.Models
{
    public class User : IModel
    {
        public User()
        {
            if (Modified == DateTime.MinValue)
                Modified = DateTime.Now;
        }
        public int _ID { get; set; }
        public Guid ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public bool Enabled { get; set; }
        public bool KeepAlive { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}
