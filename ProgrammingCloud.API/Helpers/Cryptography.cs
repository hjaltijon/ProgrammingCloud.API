using Microsoft.Extensions.Options;
using ProgrammingCloud.API.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingCloud.API.Helpers
{

    public class Cryptography
    {
        private readonly AppSettings _settings;
        private readonly string _hmacsecret;
        public Cryptography(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
            _hmacsecret = _settings.HmacSecret;
        }
        


        private const int SaltSize = 15;

        private const int HashSize = 64;

        private const int Iterations = 10000;

        public string GenerateSalt()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt;
                rng.GetBytes(salt = new byte[SaltSize]);
                return Convert.ToBase64String(salt);
            }
        }

        public string ComputeHash(string password, string salt)
        {
            return ComputeHash(password, salt, Iterations);
        }
        private string ComputeHash(string password, string salt, int iterations)
        {
            var saltBytes = Convert.FromBase64String(salt);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA512))
            {
                var hash = pbkdf2.GetBytes(HashSize);
                // Convert to base64
                var base64Hash = Convert.ToBase64String(hash);

                // Format hash with extra information
                return base64Hash;
            }
        }




        public string ComputeSHA512Hash(string text)
        {
            byte[] key = Convert.FromBase64String(_hmacsecret);
            using (HMACSHA512 hash = new HMACSHA512(key))
            {
                string hashedText =
                    Convert.ToBase64String(hash.ComputeHash(Convert.FromBase64String(text)));

                return hashedText;
            }
        }

        public string GenerateRandomString() //21 bytes is plenty
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] randombBytes;
                rng.GetBytes(randombBytes = new byte[21]);
                return Convert.ToBase64String(randombBytes);
            }
        }



    }
}
