using System;
using System.Globalization;
using EPP.CorporatePortal.Helper;
using System.IO;
using EPP.CorporatePortal.DAL.Service;
using System.Linq;
using System.Web.UI;
using System.Collections.Generic;
using OfficeOpenXml;
using DocumentFormat.OpenXml.Packaging;
using System.Text.RegularExpressions;
using EPP.CorporatePortal.DAL.Model;
using System.Text;
using OpenXmlPowerTools;
using PdfSharp.Pdf;
using PdfSharp;
using PdfSharp.Drawing;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using PdfSharp.Pdf.IO;
using System.Net.Mail;
using System.Configuration;
using System.Web;
using System.Web.Security;

namespace EPP.CorporatePortal.Models
{
    public static class Utility
    {
        public static string TitleCase(string value)
        {
            TextInfo ti = CultureInfo.CurrentCulture.TextInfo;

            return ti.ToTitleCase(value.ToLower());
            
        }
        public static string Encrypt(string str)
        {
            if (!IsEncrypted(str))
            {
                return EncryptionHelper.AESEncryptStringData(str);
            }
            return str;
        }
        public static string Decrypt(string str)
        {
            if (IsEncrypted(str, out string decryptedKey))
            {
                return decryptedKey;
            }
            return str;
        }
        public static bool IsEncrypted(string corpId)
        {
            //Call function, but only returning the bool result.
            return IsEncrypted(corpId, out string tmpKey);
        }
        public static bool IsEncrypted(string corpId, out string decryptedKey)
        {
            decryptedKey = "";
            bool returnValue = false;
            try
            {
                decryptedKey = EncryptionHelper.AESDecryptStringData(corpId);
                returnValue = true;
            }
            catch
            {
                returnValue = false;
            }
            return returnValue;
        }
        public static string EncodeAndDecryptCorpId(string input)
        {
            var returnValue = "";
            if (!string.IsNullOrEmpty(input))
            {
                input = input.Replace(" ", "+");
                input = Decrypt(input);
                returnValue = input;
            }
            return returnValue;
        }

        public static string GetHomePageByRole(string userName)
        {
            var roles = new RolesService().GetUserRolesObject(userName);
            return roles.FirstOrDefault().HomePageUrl;
        }

        public static void CopyRemoteFiles(string localPath, string remotePath, string userName)
        {
            var wildcard = "";
            CopyRemoteFiles(localPath, remotePath, userName, wildcard);
        }

        public static void SetOTP(string userName, ref string RetOTP)
        {
            Random r = new Random();
            int RandNum = r.Next(0, 1000000);
            string OTP = RandNum.ToString("000000");
            RetOTP = OTP;

            new UserService().SetUserOTP(userName,OTP);        
        }

        public static string RetrieveOTP(string userName)
        {
            string OTP = new UserService().GetOTP(userName);
            return OTP;
        }

        public static DateTime RetrieveGenOTP(string userName)
        {
            DateTime GenOTP = new UserService().GetGenOTP(userName);
            return GenOTP;
        }

        public static void SendEmailOTP(string userName,string OTP)
        {
            
            string SenderEmail = ConfigurationManager.AppSettings["fromEmailAddress"];
            string subject = ConfigurationManager.AppSettings["emailSubject"];
            string Bcc = ConfigurationManager.AppSettings["emailBCC"];
            string host = ConfigurationManager.AppSettings["smtpHost"];
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["smtpPort"]);

            string htmlString = string.Empty;
            //string Comp = string.Empty;

            try
            {
                var data = new UserService().GetEmailOTP(userName);
                //var Compdata = new UserService().GetComp(userName);

                htmlString = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("Email_Template.html"));
                htmlString = htmlString.Replace("#OTPNumber#", OTP);
                htmlString = htmlString.Replace("#Name#", data.Item2);

                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress(SenderEmail);
                message.To.Add(new MailAddress(data.Item1));
                if (!string.IsNullOrEmpty(Bcc))
                {
                    message.Bcc.Add(new MailAddress(Bcc));
                }           
                message.Subject = subject;
                message.IsBodyHtml = true; //to make message body as html  
                message.Body = htmlString;

                smtp.Port = port;
                smtp.Host = host;  
                smtp.EnableSsl = false;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);

                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Successully Send OTP via Email", "Login_OTPEmail");

            }
            catch(Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error Sending Email: " + ex.Message, "Login_OTPEmail");
            }
        }

        public static void Logout(string username,string token)
        {
            UserIdentityModel _UserIdentityModel = new UserIdentityModel();

            if (System.Web.HttpContext.Current.Session[ConfigurationManager.AppSettings["SessionVariableName"]] != null)
            {
                _UserIdentityModel = (UserIdentityModel)System.Web.HttpContext.Current.Session[ConfigurationManager.AppSettings["SessionVariableName"]];
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "Logout", "Logout");
            }

            var appCode = CommonService.GetSystemConfigValue("AppCode");
            //Logout from AgentPortalHub 
            var loginToken = token;  //System.Web.HttpContext.Current.Session[appCode + "Token"];
            var loginUserName = username; //Session[appCode + "UserName"];
            if (loginToken != null && loginUserName != null)
            {
                var loginTokenResponse = new LoginService().LogoutToken(loginToken.ToString(), loginUserName.ToString());
                if (!loginTokenResponse.Valid)
                    new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "Logout AgentPortalHub returned not valid", "Logout");
            }

            FormsAuthentication.SignOut();
            System.Web.HttpContext.Current.Session.Abandon();
        }

        public static void CopyRemoteFiles(string localPath, string remotePath, string userName, string wildcard)
        {
            try
            {
                //Copy Exception Files and Delete from Remote
                using (NetworkShareAccesser.Access(Path.GetDirectoryName(remotePath), CommonService.GetSystemConfigValue("RemoteAccessUsername"), CommonService.GetSystemConfigValue("RemoteAccessPassword")))
                {
                    if (!Directory.Exists(localPath))
                    {
                        Directory.CreateDirectory(localPath);
                    }

                    //Loop through all subfolders in the directory path
                    var subDirectoryList = Directory.GetDirectories(remotePath);
                    foreach (var subDirectory in subDirectoryList)
                    {
                        //Creates the Subdirectory in local if does not exists
                        var subDirectoryLocal = subDirectory.Replace(Directory.GetParent(subDirectory).FullName, localPath);
                        if (!Directory.Exists(subDirectoryLocal))
                        {
                            Directory.CreateDirectory(subDirectoryLocal);
                        }

                        //Lopp thorugh files in Subdirectory
                        var fileList = !string.IsNullOrEmpty(wildcard) ? Directory.GetFiles(subDirectory, wildcard) : Directory.GetFiles(subDirectory);
                        foreach (var srcPath in fileList)
                        {
                            //Do nothing if file has been copied over.
                            if (!File.Exists(localPath + Path.GetFileName(srcPath)))
                            {
                                File.Copy(srcPath, srcPath.Replace(subDirectory, subDirectoryLocal), true);
                                //File.Delete(srcPath);
                                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, Path.GetFileName(srcPath) + " has been copied to " + subDirectoryLocal, "FileProcessing");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in CopyRemoteFiles: " + ex.Message, "FileProcessing");
            }
        }

        public static void CopyRemoteFile(string localPath, string remotePath, string userName)
        {
            try
            {
                //Copy Exception Files and Delete from Remote
                using (NetworkShareAccesser.Access(Path.GetDirectoryName(remotePath), CommonService.GetSystemConfigValue("RemoteAccessUsername"), CommonService.GetSystemConfigValue("RemoteAccessPassword")))
                {
                    if (!Directory.Exists(localPath))
                    {
                        Directory.CreateDirectory(localPath);
                    }

                    if (File.Exists(remotePath) && !File.Exists(localPath + Path.GetFileName(remotePath)))
                    {
                        //Do nothing if file has been copied over.
                        File.Copy(remotePath, remotePath.Replace(Path.GetDirectoryName(remotePath), localPath), true);
                        File.Delete(remotePath);
                        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, Path.GetFileName(remotePath) + " has been copied to " + localPath, "FileProcessing");
                    }
                }
            }
            catch (Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in CopyRemoteFile: " + ex.Message, "FileProcessing");
            }
        }

        public static void DeleteRemoteFile(string remotePath, string userName)
        {
            try
            {
                //Copy Exception Files and Delete from Remote
                using (NetworkShareAccesser.Access(Path.GetDirectoryName(remotePath), CommonService.GetSystemConfigValue("RemoteAccessUsername"), CommonService.GetSystemConfigValue("RemoteAccessPassword")))
                {
                    if (File.Exists(remotePath))
                    {
                        File.Delete(remotePath);
                        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, Path.GetFileName(remotePath) + " has been deleted", "FileProcessing");
                    }
                }
            }
            catch (Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in CopyRemoteFile: " + ex.Message, "FileProcessing");
            }
        }

        public static void DeleteRemoteFolder(string remotePath, string userName)
        {
            try
            {
                //Copy Exception Files and Delete from Remote
                using (NetworkShareAccesser.Access(Path.GetDirectoryName(remotePath), CommonService.GetSystemConfigValue("RemoteAccessUsername"), CommonService.GetSystemConfigValue("RemoteAccessPassword")))
                {
                    if (Directory.Exists(remotePath))
                    {
                        Directory.Delete(remotePath, true);
                        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Directory deleted: " + Path.GetFileName(remotePath), "FileProcessing");
                    }
                }
            }
            catch (Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in CopyRemoteFile: " + ex.Message, "FileProcessing");
            }
        }

        public static byte[] CopyRemoteFileBytes(string remotePath, string userName)
        {
            try
            {
                //Copy Bytes of file
                //using (NetworkShareAccesser.Access(Path.GetDirectoryName(remotePath), CommonService.GetSystemConfigValue("RemoteAccessUsername"), CommonService.GetSystemConfigValue("RemoteAccessPassword")))
                //{
                //    if (File.Exists(remotePath))
                //    {
                //        var fileBytes = File.ReadAllBytes(remotePath);
                //        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Get binary data for: " + Path.GetFileName(remotePath), "FileProcessing");

                //        return fileBytes;
                //    }
                //    else
                //    {
                //        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "File not exists for: " + Path.GetFileName(remotePath), "FileProcessing");
                //        return Array.Empty<byte>();
                //    }
                //}            

                //string name = remotePath.Split('\\').Last();
                //string path = remotePath.Substring(0, remotePath.Count() - name.Count());
                //var file = System.IO.Directory.GetFiles(path,name).ToString();
                //var fileBytes1 = File.ReadAllBytes(file);
                var fileBytes = File.ReadAllBytes(remotePath);
                return fileBytes;
            }
            catch (Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in CopyRemoteFileBytes: " + ex.Message, "FileProcessing");

                return Array.Empty<byte>();
            }            
        }

        public static void SendRemoteFile(string localPath, string remotePath, string userName, out bool result)
        {
            result = true;
            try
            {
                using (NetworkShareAccesser.Access(Path.GetDirectoryName(remotePath), CommonService.GetSystemConfigValue("RemoteAccessUsername"), CommonService.GetSystemConfigValue("RemoteAccessPassword")))
                {
                    File.Copy(localPath, remotePath, true);
                }
            }
            catch (Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in SendRemoteFile: " + ex.Message, "FileProcessing");
                result = false;
            }
        }

        public static void SendRemoteFileBytes(byte[] fileBytes, string remotePath, string remoteUsername, string remotePassword, string userName, out bool result)
        {
            result = true;
            try
            {
                using (NetworkShareAccesser.Access(Path.GetDirectoryName(remotePath), remoteUsername, remotePassword))
                {
                    using (var fileStream = File.Create(remotePath))
                    {
                        Stream stream = new MemoryStream(fileBytes);
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in SendRemoteFile: " + ex.Message, "FileProcessing");
                result = false;
            }
        }
        public static void SendRemoteFileBytesAndCreateFolder(byte[] fileBytes, string remotePath, string remoteCreateFolder, string remoteUsername, string remotePassword, string userName, out bool result)
        {
            result = true;
            try
            {
                using (NetworkShareAccesser.Access(Path.GetDirectoryName(remotePath), remoteUsername, remotePassword))
                {
                    if (!Directory.Exists(Path.GetDirectoryName(remotePath) + Path.AltDirectorySeparatorChar + remoteCreateFolder + Path.AltDirectorySeparatorChar))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(remotePath) + Path.AltDirectorySeparatorChar + remoteCreateFolder + Path.AltDirectorySeparatorChar);
                    }                    

                    using (var fileStream = File.Create(Path.GetDirectoryName(remotePath) + Path.AltDirectorySeparatorChar + remoteCreateFolder + Path.AltDirectorySeparatorChar + Path.GetFileName(remotePath)))
                    {
                        Stream stream = new MemoryStream(fileBytes);
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in SendRemoteFile: " + ex.Message, "FileProcessing");
                result = false;
            }
        }

        public static void SendRemoteFileBytes(byte[] fileBytes, string remotePath, string userName, out bool result)
        {
            result = true;
            var remoteUsername = CommonService.GetSystemConfigValue("RemoteAccessUsername");
            var remotePassword = CommonService.GetSystemConfigValue("RemoteAccessPassword");

            SendRemoteFileBytes(fileBytes, remotePath, remoteUsername, remotePassword, userName, out bool resultSendRemote);

            result = resultSendRemote;
        }

        public static string FormatCSVDataTypes(string data, string dataType)
        {
            string response = "";

            switch(dataType.ToUpper())
            {
                case "STRING":
                    response = "=" + "\"" + data + "\"";
                    break;
                case "DECIMAL":
                    if (Decimal.TryParse(data, out decimal value))
                        response = value.ToString("0.##");
                    else
                        response = data;                  
                    break;
                default:
                    response = data;
                    break;
            }
            return response;
        }

        public static string GetRelativePathtoFile(string filePath)
        {
            var returnPath = "";

            var sitePath = System.Web.HttpContext.Current.Server.MapPath("~");
            returnPath = filePath.Replace(sitePath, "~/").Replace("\\", "/");

            return returnPath;
        }

        public static string ExceptionLinksHandling(string combinedLink, string cglsLink, string rpaLink)
        {
            var response = "";

            //link priority Combined > CGLS > RPA. To cater for cases where only 1 exception received.
            var exceptionLink = !string.IsNullOrEmpty(combinedLink) ? combinedLink : !string.IsNullOrEmpty(cglsLink) ? cglsLink : rpaLink;

            response = exceptionLink;

            return response;
        }

        public static void RegisterStartupScriptHandling(Page currPage, string key, string script, bool addScriptTags, bool logMessage, string userName)
        {
            ScriptManager.RegisterStartupScript(currPage, currPage.GetType(), key, script, addScriptTags);

            if (logMessage)
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "RegisterStartupScriptHandling: " + script, currPage.Title);
        }

        public static bool CheckFileExistsUseWildcard(string path, string fileName, bool isRemotePath)
        {
            List<string> filePath = new List<string>();
            bool result = CheckFileExistsUseWildcard(path, fileName, ref filePath, isRemotePath);

            return result;
        }

        public static bool CheckFileExistsUseWildcard(string path, string fileName, ref List<string> filePath, bool isRemotePath)
        {
            bool result = false;

            try
            {
                if (isRemotePath)
                {
                    using (NetworkShareAccesser.Access(Path.GetDirectoryName(path), CommonService.GetSystemConfigValue("RemoteAccessUsername"), CommonService.GetSystemConfigValue("RemoteAccessPassword")))
                    {
                        var files = System.IO.Directory.GetFiles(path, fileName, System.IO.SearchOption.TopDirectoryOnly).ToList();
                        if (files.Count > 0)
                        {
                            filePath = files;

                            result = true;
                        }
                    }
                }
                else
                {
                    var files = System.IO.Directory.GetFiles(path, fileName, System.IO.SearchOption.TopDirectoryOnly).ToList();
                    if (files.Count > 0)
                    {
                        filePath = files;

                        result = true;
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                //To do nothing if directory doesn't exists. File is assumed not exists and returned false.
            }

            return result;
        }
        public static byte[] SearchAndReplaceEFTBHtml(string docPath, ClaimSubmissionDocEFTB objEFTB, string userName)
        {
            byte[] retByte;

            var docHtml = File.ReadAllText(docPath);

            docHtml = StringReplace(docHtml, "<<ContractNo>>", objEFTB.ContractNo, userName);
            docHtml = StringReplace(docHtml, "<<ContractHolderName>>", objEFTB.ContractHolderName, userName);
            docHtml = StringReplace(docHtml, "<<SubsidiaryName>>", objEFTB.SubsidiaryName, userName);
            docHtml = StringReplace(docHtml, "<<LifeAssuredName>>", objEFTB.LifeAssuredName, userName);
            docHtml = StringReplace(docHtml, "<<LifeAssuredID>>", objEFTB.LifeAssuredID, userName);
            docHtml = StringReplace(docHtml, "<<SubmitterName>>", objEFTB.SubmitterName, userName);
            docHtml = StringReplace(docHtml, "<<SubmitterContactNo>>", objEFTB.SubmitterContactNo, userName);
            docHtml = StringReplace(docHtml, "<<SubmitterEmail>>", objEFTB.SubmitterEmail, userName);            
            docHtml = StringReplace(docHtml, "<<BankBranchName>>", objEFTB.BankBranchName, userName);            
            docHtml = StringReplace(docHtml, "<<ClaimType>>", objEFTB.ClaimType, userName);
            docHtml = StringReplace(docHtml, "<<DateOfEvent>>", objEFTB.DateOfEvent, userName);
            docHtml = StringReplace(docHtml, "<<CauseOfEvent>>", objEFTB.CauseOfEvent, userName);
            docHtml = StringReplace(docHtml, "<<UploadedDocList>>", objEFTB.UploadedDocList, userName);
            docHtml = StringReplace(docHtml, "<<SigClaimantName>>", objEFTB.SigClaimantName, userName);
            docHtml = StringReplace(docHtml, "<<SigSubmissionDate>>", objEFTB.SigSubmissionDate, userName);
            docHtml = StringReplace(docHtml, "<<PortalClaimNo>>", objEFTB.PortalClaimNo, userName);
            docHtml = StringReplace(docHtml, "<<EtiqaLogo>>", objEFTB.EtiqaLogo, userName);
            docHtml = StringReplace(docHtml, "<<EtiqaFooter>>", objEFTB.EtiqaFooter, userName);

            var sb = new StringBuilder();
            if (string.IsNullOrEmpty(objEFTB.AccountHolderNRIC)) //Not Nominees
            {
                //AccountHolder Name                
                sb.AppendLine("<tr class=\"pt-000002\">");
                sb.AppendLine("<td rowspan=\"4\" class=\"pt-000014\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">BANK DETAILS OF CLAIMANT</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("<td class=\"pt-000015\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">NAME ACCOUNT HOLDER : " + objEFTB.AccountHolderName + "</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");

                //Name of bank
                sb.AppendLine("<tr class=\"pt-000002\">");
                sb.AppendLine("<td class=\"pt-000015\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">NAME OF BANK  : " + objEFTB.BankName + "</span>");                
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");

                //Account No
                sb.AppendLine("<tr class=\"pt-000002\">");
                sb.AppendLine("<td class=\"pt-000015\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">ACCOUNT NUMBER : " + objEFTB.BankAccNo + "</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");

                //Company Reg No
                sb.AppendLine("<tr class=\"pt-000002\">");
                sb.AppendLine("<td class=\"pt-000015\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">COMPANY REGISTRATION NUMBER : " + objEFTB.BankROC + "</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">(NOT APPLICABLE FOR INDIVIDUAL CLAIMANT)</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");
            }
            else //Nominees
            {
                //Name of bank
                sb.AppendLine("<tr class=\"pt-000002\">");
                sb.AppendLine("<td rowspan=\"4\" class=\"pt-000014\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">BANK DETAILS OF CLAIMANT</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("<td class=\"pt-000015\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">NAME OF BANK  : " + objEFTB.BankName + "</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");

                //Account No
                sb.AppendLine("<tr class=\"pt-000002\">");
                sb.AppendLine("<td class=\"pt-000015\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">ACCOUNT NUMBER : " + objEFTB.BankAccNo + "</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");

                //AccountHolder Name
                sb.AppendLine("<tr class=\"pt-000002\">");
                sb.AppendLine("<td class=\"pt-000015\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">NAME ACCOUNT HOLDER : " + objEFTB.AccountHolderName + "</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");

                //AccountHolder NRIC
                sb.AppendLine("<tr class=\"pt-000002\">");
                sb.AppendLine("<td class=\"pt-000015\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">NRIC/ID ACCOUNT HOLDER : " + objEFTB.AccountHolderNRIC + "</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");
            }

            docHtml = StringReplace(docHtml, "<<BankDetails>>", sb.ToString(), userName);

            retByte = PdfSharpConvert(docHtml);

            return retByte;
        }
        public static byte[] SearchAndReplaceELIBHtml(string docPath, ClaimSubmissionDocEFTB objELIB, string userName)
        {
            byte[] retByte;

            var docHtml = File.ReadAllText(docPath);

            docHtml = StringReplace(docHtml, "<<ContractNo>>", objELIB.ContractNo, userName);
            docHtml = StringReplace(docHtml, "<<ContractHolderName>>", objELIB.ContractHolderName, userName);
            docHtml = StringReplace(docHtml, "<<SubsidiaryName>>", objELIB.SubsidiaryName, userName);
            docHtml = StringReplace(docHtml, "<<LifeAssuredName>>", objELIB.LifeAssuredName, userName);
            docHtml = StringReplace(docHtml, "<<LifeAssuredID>>", objELIB.LifeAssuredID, userName);
            docHtml = StringReplace(docHtml, "<<SubmitterName>>", objELIB.SubmitterName, userName);
            docHtml = StringReplace(docHtml, "<<SubmitterContactNo>>", objELIB.SubmitterContactNo, userName);
            docHtml = StringReplace(docHtml, "<<SubmitterEmail>>", objELIB.SubmitterEmail, userName);
            docHtml = StringReplace(docHtml, "<<BankBranchName>>", objELIB.BankBranchName, userName);
            docHtml = StringReplace(docHtml, "<<ClaimType>>", objELIB.ClaimType, userName);
            docHtml = StringReplace(docHtml, "<<DateOfEvent>>", objELIB.DateOfEvent, userName);
            docHtml = StringReplace(docHtml, "<<CauseOfEvent>>", objELIB.CauseOfEvent, userName);
            docHtml = StringReplace(docHtml, "<<UploadedDocList>>", objELIB.UploadedDocList, userName);
            docHtml = StringReplace(docHtml, "<<SigClaimantName>>", objELIB.SigClaimantName, userName);
            docHtml = StringReplace(docHtml, "<<SigSubmissionDate>>", objELIB.SigSubmissionDate, userName);
            docHtml = StringReplace(docHtml, "<<PortalClaimNo>>", objELIB.PortalClaimNo, userName);
            docHtml = StringReplace(docHtml, "<<EtiqaLogo>>", objELIB.EtiqaLogo, userName);
            docHtml = StringReplace(docHtml, "<<EtiqaFooter>>", objELIB.EtiqaFooter, userName);

            var sb = new StringBuilder();
            if (string.IsNullOrEmpty(objELIB.AccountHolderNRIC)) //Not Nominees
            {
                //AccountHolder Name
                sb.AppendLine("<tr class=\"pt-000002\">");
                sb.AppendLine("<td rowspan=\"4\" class=\"pt-000014\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">BANK DETAILS OF CLAIMANT</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("<td class=\"pt-000015\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">NAME ACCOUNT HOLDER : " + objELIB.AccountHolderName + "</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");
                                
                //Name of bank
                sb.AppendLine("<tr class=\"pt-000002\">");
                sb.AppendLine("<td class=\"pt-000015\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">NAME OF BANK  : " + objELIB.BankName + "</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");

                //Account No
                sb.AppendLine("<tr class=\"pt-000002\">");
                sb.AppendLine("<td class=\"pt-000015\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">ACCOUNT NUMBER : " + objELIB.BankAccNo + "</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");

                //Company Reg No
                sb.AppendLine("<tr class=\"pt-000002\">");
                sb.AppendLine("<td class=\"pt-000015\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">COMPANY REGISTRATION NUMBER : " + objELIB.BankROC + "</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">(NOT APPLICABLE FOR INDIVIDUAL CLAIMANT)</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");
            }
            else //Nominees
            {
                //Name of bank
                sb.AppendLine("<tr class=\"pt-000002\">");
                sb.AppendLine("<td rowspan=\"4\" class=\"pt-000014\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">BANK DETAILS OF CLAIMANT</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("<td class=\"pt-000015\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">NAME OF BANK  : " + objELIB.BankName + "</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");

                //Account No
                sb.AppendLine("<tr class=\"pt-000002\">");
                sb.AppendLine("<td class=\"pt-000015\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">ACCOUNT NUMBER : " + objELIB.BankAccNo + "</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");

                //AccountHolder Name
                sb.AppendLine("<tr class=\"pt-000002\">");
                sb.AppendLine("<td class=\"pt-000015\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">NAME ACCOUNT HOLDER : " + objELIB.AccountHolderName + "</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");

                //AccountHolder NRIC
                sb.AppendLine("<tr class=\"pt-000002\">");
                sb.AppendLine("<td class=\"pt-000015\">");
                sb.AppendLine("<p dir=\"ltr\" class=\"pt-Normal-000005\">");
                sb.AppendLine("<span lang=\"en-MY\" class=\"pt-DefaultParagraphFont\">NRIC/ID ACCOUNT HOLDER : " + objELIB.AccountHolderNRIC + "</span>");
                sb.AppendLine("</p>");
                sb.AppendLine("</td>");
                sb.AppendLine("</tr>");
            }

            docHtml = StringReplace(docHtml, "<<BankDetails>>", sb.ToString(), userName);

            retByte = PdfSharpConvert(docHtml);

            return retByte;
        }
        public static byte[] SearchAndReplaceEFTB(string docPath, ClaimSubmissionDocEFTB objEFLB, string userName)
        {
            byte[] retByte;

            var docBytes = File.ReadAllBytes(docPath);

            retByte = SearchAndReplaceEFTB(docBytes, objEFLB, userName);

            return retByte;
        }
        public static byte[] SearchAndReplaceEFTB(byte[] fileBytes, ClaimSubmissionDocEFTB objEFTB, string userName)
        {
            byte[] retByte;

            MemoryStream stream = new MemoryStream(fileBytes);
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(stream, true))
            {
                string docText = null;
                using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
                {
                    docText = sr.ReadToEnd();
                }

                docText = StringReplace(docText, "ContractNo", objEFTB.ContractNo, userName);
                docText = StringReplace(docText, "ContractHolderName", objEFTB.ContractHolderName, userName);
                docText = StringReplace(docText, "SubsidiaryName", objEFTB.SubsidiaryName, userName);
                docText = StringReplace(docText, "LifeAssuredName", objEFTB.LifeAssuredName, userName);
                docText = StringReplace(docText, "LifeAssuredID", objEFTB.LifeAssuredID, userName);
                docText = StringReplace(docText, "SubmitterName", objEFTB.SubmitterName, userName);
                docText = StringReplace(docText, "SubmitterContactNo", objEFTB.ContractNo, userName);
                docText = StringReplace(docText, "SubmitterEmail", objEFTB.SubmitterEmail, userName);
                docText = StringReplace(docText, "BankName", objEFTB.BankName, userName);
                docText = StringReplace(docText, "BankAccNo", objEFTB.BankAccNo, userName);
                docText = StringReplace(docText, "BankBranchName", objEFTB.BankBranchName, userName);
                docText = StringReplace(docText, "BankROC", objEFTB.BankROC, userName);
                docText = StringReplace(docText, "ClaimType", objEFTB.ClaimType, userName);
                docText = StringReplace(docText, "DateOfEvent", objEFTB.DateOfEvent, userName);
                docText = StringReplace(docText, "CauseOfEvent", objEFTB.CauseOfEvent, userName);
                docText = StringReplace(docText, "UploadedDocList", objEFTB.UploadedDocList, userName);
                docText = StringReplace(docText, "SigClaimantName", objEFTB.SigClaimantName, userName);
                docText = StringReplace(docText, "SigSubmissionDate", objEFTB.SigSubmissionDate, userName);
                docText = StringReplace(docText, "PortalClaimNo", objEFTB.PortalClaimNo, userName);

                byte[] byteArray = Encoding.UTF8.GetBytes(docText);
                MemoryStream ms = new MemoryStream(byteArray);
                wordDoc.MainDocumentPart.FeedData(ms);
            }

            retByte = stream.ToArray();

            return retByte;
        }
        public static byte[] SearchAndReplaceELIB(string docPath, ClaimSubmissionDocEFTB objEFLB, string userName)
        {
            byte[] retByte;

            var docBytes = File.ReadAllBytes(docPath);

            retByte = SearchAndReplaceELIB(docBytes, objEFLB, userName);

            return retByte;
        }
        public static byte[] SearchAndReplaceELIB(byte[] fileBytes, ClaimSubmissionDocEFTB objEFLB, string userName)
        {
            byte[] retByte;

            MemoryStream stream = new MemoryStream(fileBytes);
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(stream, true))
            {
                string docText = null;
                using (StreamReader sr = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
                {
                    docText = sr.ReadToEnd();
                }

                docText = StringReplace(docText, "ContractNo", objEFLB.ContractNo, userName);
                docText = StringReplace(docText, "ContractHolderName", objEFLB.ContractHolderName, userName);
                docText = StringReplace(docText, "SubsidiaryName", objEFLB.SubsidiaryName, userName);
                docText = StringReplace(docText, "LifeAssuredName", objEFLB.LifeAssuredName, userName);
                docText = StringReplace(docText, "LifeAssuredID", objEFLB.LifeAssuredID, userName);
                docText = StringReplace(docText, "SubmitterName", objEFLB.SubmitterName, userName);
                docText = StringReplace(docText, "SubmitterContactNo", objEFLB.ContractNo, userName);
                docText = StringReplace(docText, "SubmitterEmail", objEFLB.SubmitterEmail, userName);
                docText = StringReplace(docText, "BankName", objEFLB.BankName, userName);
                docText = StringReplace(docText, "BankAccNo", objEFLB.BankAccNo, userName);
                docText = StringReplace(docText, "BankBranchName", objEFLB.BankBranchName, userName);
                docText = StringReplace(docText, "BankROC", objEFLB.BankROC, userName);
                docText = StringReplace(docText, "ClaimType", objEFLB.ClaimType, userName);
                docText = StringReplace(docText, "DateOfEvent", objEFLB.DateOfEvent, userName);
                docText = StringReplace(docText, "CauseOfEvent", objEFLB.CauseOfEvent, userName);
                docText = StringReplace(docText, "UploadedDocList", objEFLB.UploadedDocList, userName);
                docText = StringReplace(docText, "SigClaimantName", objEFLB.SigClaimantName, userName);
                docText = StringReplace(docText, "SigSubmissionDate", objEFLB.SigSubmissionDate, userName);
                docText = StringReplace(docText, "PortalClaimNo", objEFLB.PortalClaimNo, userName);

                //using (MemoryStream ms = new MemoryStream())
                //{
                //    using (StreamWriter sw = new StreamWriter(ms))
                //    {
                //        sw.Write(docText);
                //    }
                //    ms.Seek(0, SeekOrigin.Begin);
                //    wordDoc.MainDocumentPart.FeedData(ms);
                //}
                byte[] byteArray = Encoding.UTF8.GetBytes(docText);
                MemoryStream ms = new MemoryStream(byteArray);
                wordDoc.MainDocumentPart.FeedData(ms);
            }

            retByte = stream.ToArray();

            return retByte;
        }
        private static string StringReplace(string document, string toFind, string toReplace, string userName)
        {
            string ret = document;

            try
            {
                //Replace with empty string if null
                toReplace = toReplace ?? string.Empty;

                Regex regexText = new Regex(toFind);
                if (regexText != null)
                    ret = regexText.Replace(document, toReplace);
            }
            catch (Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in StringReplace: " + ex.Message, "SearchAndReplace");
            }

            return ret;
        }
        public static byte[] PdfDocumentToBytes(PdfDocument pdfDoc, string userName)
        {
            try
            {
                MemoryStream stream = new MemoryStream();
                pdfDoc.Save(stream, false);
                byte[] bytes = stream.ToArray();

                return bytes;
            }
            catch (Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in PdfDocumentToBytes: " + ex.Message, "SearchAndReplace");

                Byte[] array = new Byte[64];
                Array.Clear(array, 0, array.Length);
                return array;
            }
        }
        public static byte[] OpenXMLBytesToPDF(byte[] docBytes, string userName)
        {
            byte[] retByte;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(docBytes, 0, docBytes.Length);
                using (WordprocessingDocument doc = WordprocessingDocument.Open(memoryStream, true))
                {
                    HtmlConverterSettings settings = new HtmlConverterSettings()
                    {
                        PageTitle = "Claims e-Form"
                    };
                    System.Xml.Linq.XElement html = HtmlConverter.ConvertToHtml(doc, settings);

                    //retByte = Encoding.Default.GetBytes(html.ToStringNewLineOnAttributes());

                    var htmlString = html.ToString();

                    //Replace css formatting to cater for pdfsharp's compatibility
                    htmlString = StringReplace(htmlString, "background: ", "background-color: ", userName);

                    retByte = PdfSharpConvert(htmlString);
                }
            }

            return retByte;
        }
        public static Byte[] PdfSharpConvert(String html)
        {
            Byte[] res = null;

            using (MemoryStream ms = new MemoryStream())
            {
                PdfGenerateConfig config = new PdfGenerateConfig()
                {
                    MarginBottom = 20,
                    MarginLeft = 20,
                    MarginRight = 20,
                    MarginTop = 20,
                    PageSize = PageSize.A3
                };

                PdfDocument pdf = TheArtOfDev.HtmlRenderer.PdfSharp.PdfGenerator.GeneratePdf(html, config);
                pdf.Save(ms);
                res = ms.ToArray();
            }

            return res;
        }

        public static Byte[] PdfSharpConvertA4(String html)
        {
            Byte[] res = null;

            PdfGenerateConfig config = new PdfGenerateConfig()
            {
                MarginBottom = 20,
                MarginLeft = 20,
                MarginRight = 20,
                MarginTop = 20,
                PageSize = PageSize.A4,
                PageOrientation = PageOrientation.Landscape,

            };
            
            using (MemoryStream ms = new MemoryStream())
            {
                PdfDocument pdf = TheArtOfDev.HtmlRenderer.PdfSharp.PdfGenerator.GeneratePdf(html, config);
                pdf.Save(ms);
                res = ms.ToArray();
             }

             return res;
        }

        public static Byte[] PdfSharpConvertA4(String html, string doctype = null)
        {
            Byte[] res = null;
            PdfGenerateConfig config;

            if (doctype == "UAM")
            {
                config = new PdfGenerateConfig()
                {
                    PageSize = PageSize.A3,
                    PageOrientation = PageOrientation.Landscape,

                };
            }
            else
            {
                config = new PdfGenerateConfig()
                {
                    MarginBottom = 20,
                    MarginLeft = 20,
                    MarginRight = 20,
                    MarginTop = 20,
                    PageSize = PageSize.A3,
                    PageOrientation = PageOrientation.Landscape,

                };
            }

            using (MemoryStream ms = new MemoryStream())
            {
                var pdf = TheArtOfDev.HtmlRenderer.PdfSharp.PdfGenerator.GeneratePdf(html, config);
                pdf.Save(ms);
                res = ms.ToArray();
            }

            return res;
        }

        public static Byte[] PDFCopyPages(byte[] fileByteFrom, byte[] fileByteTo)
        {
            Byte[] res = null;

            PdfDocument from = PdfReader.Open(new MemoryStream(fileByteFrom), PdfDocumentOpenMode.Import);
            PdfDocument to = PdfReader.Open(new MemoryStream(fileByteTo), PdfDocumentOpenMode.Import);

            for (int i = 0; i < from.PageCount; i++)
            {
                to.AddPage(from.Pages[i]);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                to.Save(ms);

                res = ms.ToArray();
            }

            return res;
        }
        public static byte[] GetWordFilBytes(string document)
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(document, true))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    wordDoc.MainDocumentPart.GetStream().CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
        public static byte[] FileBinaryCheckInjectionChar(byte[] fileBinary, string userName, out bool boolResult)
        {
            boolResult = false;
            try
            {
                byte[] retByte;
                List<string> scriptInjectionChar = new List<string>(new string[] { "=", "<", "+", "-", "@" });

                //load the uploaded file into the memorystream
                using (MemoryStream stream = new MemoryStream(fileBinary))
                {
                    using (ExcelPackage excelPackage = new ExcelPackage(stream))
                    {
                        ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets.First();
                        var start = workSheet.Dimension.Start;
                        var end = workSheet.Dimension.End;
                        for (int row = start.Row; row <= end.Row; row++)
                        { // Row by row...
                            for (int col = start.Column; col <= end.Column; col++)
                            { // ... Cell by cell...
                              //Checks if starts with any char possible for script injection
                                if (workSheet.Cells[row, col].Value != null && scriptInjectionChar.Any(a => workSheet.Cells[row, col].Value.ToString().StartsWith(a)))
                                {
                                    //Append ' to make it as a string
                                    workSheet.Cells[row, col].Value = "'" + workSheet.Cells[row, col].Value.ToString();
                                }
                            }
                        }
                        retByte = excelPackage.GetAsByteArray();
                    }
                }

                boolResult = true;
                return retByte;
            }
            catch (Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in FileBinaryCheckInjectionChar: " + ex.Message, "FileProcessing");

                Byte[] array = new Byte[64];
                Array.Clear(array, 0, array.Length);
                return array;
            }
        }
    }
}