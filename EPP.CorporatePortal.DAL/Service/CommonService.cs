using EPP.CorporatePortal.Common;
using EPP.CorporatePortal.DAL.EDMX;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EPP.CorporatePortal.DAL.Service
{
    public static class CommonService
    {

        /// <summary>
        /// Encrypts given text
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns>Encrypted string</returns>
        public static string EncryptString(string plainText)
        {
            byte[] key = Encoding.ASCII.GetBytes(GetAppSettingValue("JWTKey"));
            byte[] iv = Encoding.ASCII.GetBytes(GetAppSettingValue("JWTVi"));


            byte[] encrypted;
            // Create a new AesManaged.    
            using (AesManaged aes = new AesManaged())
            {
                aes.Mode = CipherMode.CBC;
                aes.KeySize = 256;
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
                        // Create StreamWriter and write data to a stream    
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(plainText);
                        encrypted = ms.ToArray();
                    }
                }
            }

            return System.Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Decrypts given text
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns>Decrypted string</returns>
        public static string Decrypt(string plainText)
        {
            
            byte[] key = Encoding.ASCII.GetBytes(GetAppSettingValue("JWTKey"));
            byte[] iv = Encoding.ASCII.GetBytes(GetAppSettingValue("JWTVi"));

            string returnText = null;

            // Create AesManaged    
            using (AesManaged aes = new AesManaged())
            {
                aes.Mode = CipherMode.CBC;
                aes.KeySize = 256;
                
                // Create a decryptor    
                ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);
                // Create the streams used for decryption.    
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(plainText)))
                {
                    CryptoStream cs = null;
                    try
                    {
                        cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                        using (StreamReader reader = new StreamReader(cs))
                        {
                            returnText = reader.ReadToEnd();
                        }
                    }
                    finally
                    {
                        if (cs != null)
                            cs.Dispose();
                    }
                }
            }
            return returnText;
        }

        /// <summary>
        /// Gets the app.setting value from web.config
        /// </summary>
        /// <param name="key"></param>
        /// <returns>String value</returns>
        public static string GetAppSettingValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// Gets condig value from SystemConfig table in Database
        /// </summary>
        /// <param name="key"></param>
        /// <returns>String value</returns>
        public static string GetSystemConfigValue(string key)
        {
            var dbEntity = new EPPCorporatePortalEntities();
            var value = dbEntity.SystemConfigs.Where(s => s.Setting == key);
            if (value != null)
            {
                return value.FirstOrDefault().Value;
            }
            return String.Empty;
        }

        public static Rights_Enum accessEnum(string Getusername)
        {
            var dbEntity = new EPPCorporatePortalEntities();
            var getuserID = dbEntity.Users.Where(x => x.UserName == Getusername).Select(x => x.Id).Single();
            int getuserroleid = dbEntity.UserRoles.Where(y => y.UserId == getuserID).Select(y => y.RoleId).Single();

            var result = getuserroleid == 1 ? Rights_Enum.ManageAdminTasks
                : Rights_Enum.ManageClaim;

            return result;
        }
    }
}
