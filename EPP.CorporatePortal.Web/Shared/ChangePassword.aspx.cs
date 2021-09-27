using EPP.CorporatePortal.DAL.Entity;
using EPP.CorporatePortal.DAL.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EPP.CorporatePortal.Shared
{
    public partial class ChangePassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var userName = "";

            try
            {
                var appCode = CommonService.GetSystemConfigValue("AppCode");
                userName = Session[appCode + "Username"].ToString();
            }
            catch
            {
                userName = "";
            }

            //Redirect to logout if No userID session
            if (string.IsNullOrEmpty(userName))
                Response.Redirect(ConfigurationManager.AppSettings["RouteURL"] + "/Shared/Logout.aspx", false);
            else
            {
                var usernameCheck = userName.Contains("@") ? userName.Substring(0, userName.LastIndexOf('@')) : userName; //Only take username without trailing email. e.g "username" instead of "username@email.com"
                hdnUsername.Value = usernameCheck;
            }
        }
        protected void BtnResetPassword_Click(object sender, EventArgs e)
        {
            var blnContinue = false;

            if (Page.IsValid)
            {
                try
                {
                    var appCode = CommonService.GetSystemConfigValue("AppCode");
                    var businessEntityID = CommonService.GetSystemConfigValue("BusinessEntityID");
                    var userName = Session[appCode + "Username"].ToString();                    

                    var password = TxtCurrPassword.Text;
                    var newPassword = TxtNewPassword.Text;
                    var confirmPassword = TxtConfirmNewPassword.Text;

                    if (!newPassword.Equals(confirmPassword)) //Checks if both new password and confirm new password are the same.
                    {
                        spanResetStatus.InnerHtml = "Password doesn't match!";
                        spanResetStatus.Style["display"] = "block";
                        return;
                    }

                    //2nd validation check in case javascript error/bypassed
                    var usernameCheck = userName.Contains("@") ? userName.Substring(0, userName.LastIndexOf('@')) : userName; //Only take username without trailing email. e.g "username" instead of "username@email.com"
                    var expression = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[~`!@#$%^&*(){}+=-])[A-Za-z\d~`!@#$%^&*(){}+=-]{8,}$";
                    Regex regex = new Regex(expression);
                    Match match = regex.Match(newPassword);
                    if (!match.Success || newPassword.Contains(usernameCheck)) //Now checks if contains username as well
                    {
                        spanResetStatus.InnerHtml = "New Password not complex";
                        spanResetStatus.Style["display"] = "block";
                        return;
                    }                    

                    var storedProcServ = new StoredProcService(userName);

                    //Check Password History
                    var userPasswordHashed = Get_HASH_SHA512(newPassword);
                    var dtPasswordHist = storedProcServ.GetUserPasswordHistory(userName);
                    if (dtPasswordHist.Rows.Count > 0)
                    {
                        var passwordHashesList = dtPasswordHist.AsEnumerable().Select(s => s["Hash"].ToString()).ToList();

                        if (passwordHashesList.Contains(userPasswordHashed))
                        {
                            spanResetStatus.InnerHtml = "New Password has been previously used";
                            spanResetStatus.Style["display"] = "block";
                            return;
                        }
                    }

                    new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Generating change password body {username: " + userName + ", appcode: " + appCode + ", businessid: " + businessEntityID + "}", "BtnResetPassword_Click");
                    var accReq = new ChangePasswordTokenRequest() { AccountCode = userName, Password = password, AppCode = appCode, BusinessEntityID = Convert.ToInt32(businessEntityID), NewPassword = newPassword };
                    var body = JsonConvert.SerializeObject(accReq, Formatting.Indented);

                    string token = CommonService.EncryptString(body);
                    var resetTokenResponse = new LoginService().UpdatePassword(token, userName);
                    if (resetTokenResponse.Valid)
                    {
                        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Change Password successfully to Agent Portal Hub", "BtnResetPassword_Click");
                        //Save the last time user changed password
                        storedProcServ.UpdateLastPasswordChangeDate(userName);

                        //Password History logic
                        var noOfPasswordHistory = Convert.ToInt32(CommonService.GetSystemConfigValue("NoOfPasswordHistory"));
                        var dtPasswordHistCount = dtPasswordHist.Rows.Count;
                        //Delete old records leaving only requiured amount of records
                        if (dtPasswordHistCount >= noOfPasswordHistory)
                        {
                            var countToDelete = (dtPasswordHistCount + 1) - noOfPasswordHistory;
                            new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Removing old Password History records. Count = " + countToDelete.ToString(), "BtnResetPassword_Click");
                            storedProcServ.RemoveOldUserPasswordHistory(userName, countToDelete);
                        }
                        //Save password history
                        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Saving New Password History", "BtnResetPassword_Click");
                        storedProcServ.InsUserPasswordHistory(userName, userPasswordHashed);

                        blnContinue = true;
                    }
                    else
                    {
                        spanResetStatus.InnerHtml = resetTokenResponse.ResponseStatusEntity.StatusDescription;
                        spanResetStatus.Style["display"] = "block";

                        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Change Password not valid: " + resetTokenResponse.ResponseStatusEntity.StatusDescription, "BtnResetPassword_Click");
                    }
                }
                catch (Exception ex)
                {
                    var appCode = CommonService.GetSystemConfigValue("AppCode");
                    var userName = Session[appCode + "Username"].ToString();

                    spanResetStatus.InnerHtml = "Error occured";
                    spanResetStatus.Style["display"] = "block";

                    new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in BtnResetPassword_Click: " + ex.Message, "ResetPassword");
                }
                if (blnContinue)
                {
                    //User will need to relogin using the new login credentials
                    Response.Redirect(ConfigurationManager.AppSettings["RouteURL"] + "/Shared/ChangePasswordSuccess.aspx", false);
                }
            }
            else
            {
                spanResetStatus.InnerHtml = "Please make sure all fields are filled in correctly";
                spanResetStatus.Style["display"] = "block";
            }

        }
        public static string Get_HASH_SHA512(string password)
        {
            try
            {
                //Plain Text in Byte
                byte[] plainTextBytes = Encoding.UTF8.GetBytes(password);

                HashAlgorithm hash = new SHA512Managed();
                byte[] hashBytes = hash.ComputeHash(plainTextBytes);

                return Convert.ToBase64String(hashBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool CompareHashValue(string password, string OldHASHValue)
        {
            try
            {
                string expectedHashString = Get_HASH_SHA512(password);

                return (OldHASHValue == expectedHashString);
            }
            catch
            {
                return false;
            }
        }
    }
}