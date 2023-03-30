using Panama.Interfaces;
using System;

namespace Panama.Tests.Models
{
    public class User : IModel
    {
        public User()
        {
            if (Created == DateTime.MinValue)
                Created = DateTime.Now;
        }
        public int _ID { get; set; }
        public Guid ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime Created { get; set; }
    }
}
