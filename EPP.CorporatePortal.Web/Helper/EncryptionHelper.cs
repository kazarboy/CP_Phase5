using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using EPP.CorporatePortal.DAL.Service;
using System.Web;

namespace EPP.CorporatePortal.Helper
{
    public static class EncryptionHelper
    {
        /// <summary>
        /// Encrypts given string
        /// </summary>
        /// <param name="byteData"></param>
        /// <returns>Encrypted bytes</returns>
        public static string AESEncryptStringData(string str)
        {
            byte[] resultArray;

            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(str);

            resultArray = AESEncryptByteData(toEncryptArray);

            return HttpServerUtility.UrlTokenEncode(resultArray);
        }

        /// <summary>
        /// Decrypts given string
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns>Decrypted bytes</returns>
        public static string AESDecryptStringData(string str)
        {
            byte[] resultArray;

            byte[] toEncryptArray = HttpServerUtility.UrlTokenDecode(str);

            resultArray = AESDecryptByteData(toEncryptArray);

            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        /// <summary>
        /// Encrypts given byte string
        /// </summary>
        /// <param name="byteData"></param>
        /// <returns>Encrypted bytes</returns>
        public static byte[] AESEncryptByteData(byte[] byteData)
        {
            byte[] key = Encoding.ASCII.GetBytes(CommonService.GetSystemConfigValue("FileEncryptionKey"));
            byte[] iv = Encoding.ASCII.GetBytes(CommonService.GetSystemConfigValue("FileEncryptionIV"));


            byte[] encrypted;
            // Create a new AesManaged.    
            using (AesManaged aes = new AesManaged())
            {
                aes.Mode = CipherMode.CBC;
                aes.KeySize = 256;
                aes.Padding = PaddingMode.PKCS7;
                // Create encryptor    
                ICryptoTransform encryptor = aes.CreateEncryptor(key, iv);
                
                // Create MemoryStream    
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create crypto stream using the CryptoStream class. This class is the key to encryption    
                    // and encrypts and decrypts data from any given stream. In this case, we will pass a memory stream    
                    // to encrypt    
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(byteData, 0, byteData.Length);
                        cs.FlushFinalBlock();

                        encrypted = ms.ToArray();
                    }
                }
            }

            return encrypted;
        }

        /// <summary>
        /// Decrypts given byte string
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns>Decrypted bytes</returns>
        public static byte[] AESDecryptByteData(byte[] byteData)
        {
            byte[] key = Encoding.ASCII.GetBytes(CommonService.GetSystemConfigValue("FileEncryptionKey"));
            byte[] iv = Encoding.ASCII.GetBytes(CommonService.GetSystemConfigValue("FileEncryptionIV"));

            byte[] decrypted;

            // Create AesManaged    
            using (AesManaged aes = new AesManaged())
            {
                aes.Mode = CipherMode.CBC;
                aes.KeySize = 256;
                aes.Padding = PaddingMode.PKCS7;

                // Create a decryptor    
                ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);
                // Create the streams used for decryption.    
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create crypto stream    
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(byteData, 0, byteData.Length);
                        cs.FlushFinalBlock();

                        decrypted = ms.ToArray();
                    }
                }
            }
            return decrypted;
        }
    }
}
