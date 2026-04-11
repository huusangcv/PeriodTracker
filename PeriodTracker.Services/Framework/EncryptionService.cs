using System.Security.Cryptography;
using System.Text;

namespace PeriodTracker.Services.Framework
{
    public interface IEncryptionService
    {
        string CreatePasswordHash(string password, string salt, string passwordFormat = "SHA1");
        string CreateSalt(int saltLength);
        bool IsValidPassword(string sqlPassword, string inputPassword, string salt);
    }

    public class EncryptionService : IEncryptionService
    {
        public string CreateSalt(int saltLength)
        {
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                byte[] array = new byte[saltLength];
                randomNumberGenerator.GetBytes(array);
                return Convert.ToBase64String(array);
            }
        }
        public virtual string CreatePasswordHash(string password, string saltkey, string passwordFormat = "SHA1")
        {
            if (string.IsNullOrEmpty(passwordFormat))
            {
                passwordFormat = "SHA1";
            }
            string s = password + saltkey;
            HashAlgorithm hashAlgorithm = passwordFormat switch
            {
                "SHA1" => SHA1.Create(),
                "SHA256" => SHA256.Create(),
                "SHA384" => SHA384.Create(),
                "SHA512" => SHA512.Create(),
                "MD5" => MD5.Create(),
                _ => throw new ArgumentException("Unrecognized hash name")
            };
            using (hashAlgorithm)
            {
                byte[] hashBytes = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(s));
                return Convert.ToHexString(hashBytes);
            }
        }
        public bool IsValidPassword(string sqlPassword, string inputPassword, string salt) => CreatePasswordHash(inputPassword, salt).Equals(sqlPassword);
    }
}
