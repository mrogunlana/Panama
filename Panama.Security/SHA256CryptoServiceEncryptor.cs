using Panama.Core.Security.Interfaces;
using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Panama.Core.Security
{
    public class SHA256CryptoServiceEncryptor : IStringEncryptor
    {
        public string FromString(string encrypted)
        {
            throw new NotImplementedException();
        }

        public string ToString(string value)
        {
            var algorithm = new SHA256CryptoServiceProvider();
            var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));

            return Convert.ToBase64String(hash);
        }
    }
}
