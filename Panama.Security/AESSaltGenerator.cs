﻿using Panama.Security.Interfaces;
using System;
using System.Configuration;
using System.Security.Cryptography;

namespace Panama.Security
{
    public class AESSaltGenerator : ISaltGenerator
    {
        private string _secret;
        private string _salt;
        private string _vector;

        public AESSaltGenerator()
        {
            _secret = ConfigurationManager.AppSettings["Secret"];
            _salt = ConfigurationManager.AppSettings["Salt"];
            _vector = ConfigurationManager.AppSettings["Vector"];
        }

        public string Salt()
        {
            var random = new RNGCryptoServiceProvider();

            // Maximum length of salt
            int max_length = 32;

            // Empty salt array
            byte[] salt = new byte[max_length];

            // Build the random bytes
            random.GetNonZeroBytes(salt);

            // Return the string encoded salt
            return Convert.ToBase64String(salt);
        }

        public string Vector()
        {
            var block = 0;
            using (var cipher = new AesManaged())
                block = cipher.BlockSize;

            var vector = string.Empty;
            using (var rfc = new Rfc2898DeriveBytes(Convert.FromBase64String(_secret), Convert.FromBase64String(_salt), 3))
                vector = Convert.ToBase64String(rfc.GetBytes(block / 8));

            return vector;
        }
    }
}
