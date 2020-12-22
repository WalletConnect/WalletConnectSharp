using System;

namespace WalletConnectSharp.Utils
{
    public static class Hex
    {
        public static string ToHex(this byte[] data, bool includeDashes = false)
        {
            var b = BitConverter.ToString(data);
            return includeDashes ? b : b.Replace("-", "");
        }
        
        public static byte[] FromHex(this string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}