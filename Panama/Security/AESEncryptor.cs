﻿using Microsoft.Extensions.Configuration;
using Panama.Security.Interfaces;
using Panama.Security.Resolvers;
using System.Security.Cryptography;
using System.Text;

namespace Panama.Security
{
    public class AESEncryptor : IStringEncryptor
    {
        private string _secret;
        private string _salt;
        private string _vector;
        private int _iterations = 3;
        private int _keySize = 256;
        private string _hash = "SHA1";


        public StringEncryptorResolverKey Key { get { return StringEncryptorResolverKey.AES; } }

        public AESEncryptor(IConfiguration configuration)
        {
            _secret = configuration.GetValue<string>("Panama:Security:AES:Secret") ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENCRYPTION_SECRET") ?? string.Empty;
            _salt = configuration.GetValue<string>("Panama:Security:AES:Salt") ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENCRYPTION_SALT") ?? string.Empty;
            _vector = configuration.GetValue<string>("Panama:Security:AES:Vector") ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENCRYPTION_VECTOR") ?? string.Empty;
        }

        public string ToString(string value)
        {
            return ToString<AesManaged>(value);
        }

        public string ToString<T>(string value)
                where T : SymmetricAlgorithm, new()
        {
            byte[] vectorBytes = Convert.FromBase64String(_vector);
            byte[] saltBytes = Convert.FromBase64String(_salt);
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);

            byte[] encrypted;
            using (T cipher = new T())
            {
                PasswordDeriveBytes _passwordBytes =
                    new PasswordDeriveBytes(Convert.FromBase64String(_secret), saltBytes, _hash, _iterations);
                byte[] keyBytes = _passwordBytes.GetBytes(_keySize / 8);

                cipher.Mode = CipherMode.CBC;

                using (ICryptoTransform encryptor = cipher.CreateEncryptor(keyBytes, vectorBytes))
                {
                    using (MemoryStream to = new MemoryStream())
                    {
                        using (CryptoStream writer = new CryptoStream(to, encryptor, CryptoStreamMode.Write))
                        {
                            writer.Write(valueBytes, 0, valueBytes.Length);
                            writer.FlushFinalBlock();
                            encrypted = to.ToArray();
                        }
                    }
                }
                cipher.Clear();
            }
            return Convert.ToBase64String(encrypted);
        }

        public string FromString(string encrypted)
        {
            return FromString<AesManaged>(encrypted);
        }

        public string FromString<T>(string encrypted) where T : SymmetricAlgorithm, new()
        {
            byte[] vectorBytes = Convert.FromBase64String(_vector);
            byte[] saltBytes = Convert.FromBase64String(_salt);
            byte[] valueBytes = Convert.FromBase64String(encrypted);

            byte[] decrypted;
            int decryptedByteCount = 0;

            using (T cipher = new T())
            {
                PasswordDeriveBytes _passwordBytes = new PasswordDeriveBytes(Convert.FromBase64String(_secret), saltBytes, _hash, _iterations);
                byte[] keyBytes = _passwordBytes.GetBytes(_keySize / 8);

                cipher.Mode = CipherMode.CBC;

                using (ICryptoTransform decryptor = cipher.CreateDecryptor(keyBytes, vectorBytes))
                using (MemoryStream from = new MemoryStream(valueBytes))
                using (CryptoStream reader = new CryptoStream(from, decryptor, CryptoStreamMode.Read))
                {
                    decrypted = new byte[valueBytes.Length];
                    decryptedByteCount = reader.Read(decrypted, 0, decrypted.Length);
                }

                cipher.Clear();
            }
            return Encoding.UTF8.GetString(decrypted, 0, decryptedByteCount);
        }
    }
}
