using System.Security.Cryptography;

namespace WalletConnectSharp.Auth.Internals
{
    public class CryptoUtils
    {
        private static readonly char[] ALPHANUMERIC =
            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
        
        public static string GenerateNonce()
        {
            int bits = 96;
            int charsetLength = ALPHANUMERIC.Length;
            int length = (int)Math.Ceiling(bits / (Math.Log10(charsetLength) / Math.Log(2)));
            
            string @out = "";
            int maxByte = 256 - (256 % charsetLength);

            using (var rng = RandomNumberGenerator.Create())
            {
                while (length > 0)
                {
                    byte[] buf = new byte[(int)Math.Ceiling(length * 256.0 / maxByte)];

                    rng.GetBytes(buf);

                    for (int i = 0; i < buf.Length && length > 0; i++)
                    {
                        var randomByte = buf[i];
                        if (randomByte < maxByte)
                        {
                            @out += ALPHANUMERIC[randomByte % charsetLength];
                            length--;
                        }
                    }
                }
            }

            return @out;
        }
    }
}
