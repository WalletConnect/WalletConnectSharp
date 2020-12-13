using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WalletConnectSharp.Models;
using WalletConnectSharp.Utils;

namespace WalletConnectSharp.Network
{
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
                    ciphor.BlockSize = 128;

                    byte[] iv = ciphor.IV;

                    using (CryptoStream cs = new CryptoStream(ms, ciphor.CreateEncryptor(key, iv),
                        CryptoStreamMode.Write))
                    {
                        await cs.WriteAsync(data, 0, data.Length);
                    }

                    byte[] encryptedContent = ms.ToArray();

                    using (HMACSHA256 hmac = new HMACSHA256(key))
                    {
                        byte[] toSign = new byte[iv.Length + encryptedContent.Length];
                        
                        //copy our 2 array into one
                        System.Buffer.BlockCopy(iv, 0, toSign, 0, iv.Length);
                        System.Buffer.BlockCopy(encryptedContent, 0, toSign, iv.Length, encryptedContent.Length);

                        byte[] signature = hmac.ComputeHash(toSign);
                        
                        string ivHex = BitConverter.ToString(iv).Replace("-", "");
                        string dataHex = BitConverter.ToString(encryptedContent).Replace("-", "");
                        string hmacHex = BitConverter.ToString(signature).Replace("-", "");

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

        public Task<string> DecryptWithKey(byte[] key, EncryptedPayload encryptedData, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            
            byte[] rawData = encryptedData.data.FromHex();
            byte[] iv = encryptedData.iv.FromHex();
            byte[] hmac = encryptedData.hmac.FromHex();


            using (AesManaged cryptor = new AesManaged())
            {
                cryptor.Mode = CipherMode.CBC;
                cryptor.Padding = PaddingMode.PKCS7;
                cryptor.KeySize = 256;
                cryptor.BlockSize = 128;

                cryptor.IV = iv;
                cryptor.Key = key;

                ICryptoTransform decryptor = cryptor.CreateDecryptor(cryptor.Key, cryptor.IV);

                using (MemoryStream ms = new MemoryStream(rawData))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs, encoding))
                        {
                            return sr.ReadToEndAsync();
                        }
                    }
                }
            }
        }
    }
}