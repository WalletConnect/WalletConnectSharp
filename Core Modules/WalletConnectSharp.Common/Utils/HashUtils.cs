using System.Security.Cryptography;
using System.Text;

namespace WalletConnectSharp.Common.Utils
{
    public static class HashUtils
    {
        public static string HashMessage(string message)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(message));

                return bytes.ToHex();
            }
        }
    }
}