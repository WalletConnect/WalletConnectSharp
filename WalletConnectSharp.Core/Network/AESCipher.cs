using System.Security.Cryptography;
using System.Text;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Utils;

namespace WalletConnectSharp.Core.Network;
public class AESCipher : ICipher
{
    public async Task<EncryptedPayload> EncryptWithKey(byte[] key, string message, Encoding encoding = null)
    {
        if (encoding == null)
            encoding = Encoding.UTF8;

        byte[] data = encoding.GetBytes(message);

        //Encrypt with AES/CBC/PKCS7Padding
        using (MemoryStream ms = new MemoryStream())
        {
            using (AesManaged ciphor = new AesManaged())
            {
                ciphor.Mode = CipherMode.CBC;
                ciphor.Padding = PaddingMode.PKCS7;
                ciphor.KeySize = 256;

                byte[] iv = ciphor.IV;

                using (CryptoStream cs = new CryptoStream(ms, ciphor.CreateEncryptor(key, iv),
                    CryptoStreamMode.Write))
                {
                    await cs.WriteAsync(data, 0, data.Length);
                }

                byte[] encryptedContent = ms.ToArray();

                using (HMACSHA256 hmac = new HMACSHA256(key))
                {
                    hmac.Initialize();

                    byte[] toSign = new byte[iv.Length + encryptedContent.Length];

                    //copy our 2 array into one
                    Buffer.BlockCopy(encryptedContent, 0, toSign, 0, encryptedContent.Length);
                    Buffer.BlockCopy(iv, 0, toSign, encryptedContent.Length, iv.Length);

                    byte[] signature = hmac.ComputeHash(toSign);

                    string ivHex = iv.ToHex();
                    string dataHex = encryptedContent.ToHex();
                    string hmacHex = signature.ToHex();

                    return new EncryptedPayload()
                    {
                        data = dataHex,
                        hmac = hmacHex,
                        iv = ivHex
                    };
                }
            }
        }
    }

    public async Task<string> DecryptWithKey(byte[] key, EncryptedPayload encryptedData, Encoding encoding = null)
    {
        if (encoding == null)
            encoding = Encoding.UTF8;

        byte[] rawData = encryptedData.data.HexToByteArray();
        byte[] iv = encryptedData.iv.HexToByteArray();
        byte[] hmacReceived = encryptedData.hmac.HexToByteArray();

        using (HMACSHA256 hmac = new HMACSHA256(key))
        {
            hmac.Initialize();

            byte[] toSign = new byte[iv.Length + rawData.Length];

            //copy our 2 array into one
            Buffer.BlockCopy(rawData, 0, toSign, 0, rawData.Length);
            Buffer.BlockCopy(iv, 0, toSign, rawData.Length, iv.Length);

            byte[] signature = hmac.ComputeHash(toSign);

            if (!signature.SequenceEqual(hmacReceived))
                throw new InvalidDataException("HMAC Provided does not match expected"); //Ignore
        }

        using (AesManaged cryptor = new AesManaged())
        {
            cryptor.Mode = CipherMode.CBC;
            cryptor.Padding = PaddingMode.PKCS7;
            cryptor.KeySize = 256;

            cryptor.IV = iv;
            cryptor.Key = key;

            ICryptoTransform decryptor = cryptor.CreateDecryptor(cryptor.Key, cryptor.IV);

            using (MemoryStream ms = new MemoryStream(rawData))
            {
                using (MemoryStream sink = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        int read = 0;
                        byte[] buffer = new byte[1024];
                        do
                        {
                            read = await cs.ReadAsync(buffer, 0, buffer.Length);

                            if (read > 0)
                                await sink.WriteAsync(buffer, 0, read);
                        } while (read > 0);

                        await cs.FlushAsync();

                        return encoding.GetString(sink.ToArray());
                    }
                }
            }
        }
    }
}
