﻿using Panama.Security.Interfaces;
using System;
using System.Configuration;
using System.IO;
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
        
        public AESEncryptor()
        {
            _secret = ConfigurationManager.AppSettings["Secret"];
            _salt = ConfigurationManager.AppSettings["Salt"];
            _vector = ConfigurationManager.AppSettings["Vector"];
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
