using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UmbrellaProject.Services
{
    class RSA2048
    {
        public static string publicKey = "<RSAKeyValue><Modulus>ourfNiz5z3+NKD+dXQ9JJ2WoNeZQPt+VK9Qqfl5QNY0deO2gwDiAw1w1S6xYmad1qyA0iJk6Z+c/lrOYAf5jDpPwfsGQ/4SP8GHN7x099uOrqW9qRJPVYW23lGCllyUN5aGE1Xz+i3ErWIv6sob/s5o5C+WrIcpPCzr80DxRjSt0G4UPFQmwt4pW+F1pDDPkKPXVDw/WWdkKHPPF3cr5LfZfDHmY36Zk9zG83/iCo54Gd7Iluy1MGiuTO1uoLDZlSYK7biBnqwDEUomgWuX2LqX8W3RfOehFB/SOvsRW7TfalRSH5zcaaUiChd/gaP9d+A4UyMz+3VAXC3LmTyeKNQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        public static void GenerateKeys(out string PublicKey, out string PrivateKey)
        {
            using (RSACryptoServiceProvider RSA2048 = new RSACryptoServiceProvider(2048))
            {
                RSA2048.PersistKeyInCsp = false;
                PublicKey = RSA2048.ToXmlString(false);
                PrivateKey = RSA2048.ToXmlString(true);
            }
        }

        public static string Encrypt(string PublicKey, string plain)
        {
            using (RSACryptoServiceProvider RSA2048 = new RSACryptoServiceProvider(2048))
            {
                RSA2048.PersistKeyInCsp = false;
                RSA2048.FromXmlString(PublicKey);
                return Convert.ToBase64String(RSA2048.Encrypt(Encoding.UTF8.GetBytes(plain), false));
            }
        }

        public static string Decrypt(string PrivateKey, string cipher)
        {
            using (RSACryptoServiceProvider RSA2048 = new RSACryptoServiceProvider(2048))
            {
                RSA2048.PersistKeyInCsp = false;
                RSA2048.FromXmlString(PrivateKey);
                return Encoding.UTF8.GetString(RSA2048.Decrypt(Convert.FromBase64String(cipher), false));
            }
        }
    }
}
