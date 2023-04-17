using Panama.Security.Interfaces;
using Panama.Security.Resolvers;
using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Panama.Security
{
    public class SHA256CryptoServiceEncryptor : IStringEncryptor
    {
        public StringEncryptorResolverKey Key { get { return StringEncryptorResolverKey.SHA256; } }

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
