using Panama.Core.Security.Interfaces;
using Panama.Core.Security.Resolvers;
using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Panama.Core.Security
{
    public class Base64Encryptor : IStringEncryptor
    {
        public ResolverKey Key { get { return ResolverKey.Base64; } }

        public string FromString(string encrypted)
        {
            var bytes = Convert.FromBase64String(encrypted);

            return Encoding.UTF8.GetString(bytes);
        }

        public string ToString(string value)
        {
            var text = Encoding.UTF8.GetBytes(value);

            return Convert.ToBase64String(text);
        }
    }
}
