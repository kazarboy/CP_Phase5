using EPP.CorporatePortal.DAL.Entity;
using EPP.CorporatePortal.DAL.Service;
using EPP.CorporatePortal.DAL.Model;
using Newtonsoft.Json;
using System;
using System.Web;
using System.Web.UI;
using EPP.CorporatePortal.Models;
using System.Configuration;
using System.Web.Security;
using System.Net;

namespace EPP.CorporatePortal
{
    public partial class Login : System.Web.UI.Page
    {
        //public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected void Page_Load(object sender, EventArgs e)
        {
            Page.DataBind();
        }  

        protected void BtnLogin_Click(object sender, EventArgs e)
        {
            string RetOTP = string.Empty;
            var blnContinue = false;
            double passDayLeft = 0;
            
            var userName = txtUserName.Text;
            var appCode = CommonService.GetSystemConfigValue("AppCode");
            var storedProcServ = new StoredProcService(userName);
            try
             {
                 var lastPasswordChangeDate = new DateTime();

                 //To cater for invalid login credentials attempts. Limit users to certain login attemps amount. Also to cater for Inactive or deleted accounts
                 var userDetails = storedProcServ.GetUserDetails(userName);
                 if (userDetails.Rows.Count > 0)
                 {
                     //Login Attempt counts check
                     var loginAttemptCount = Convert.ToInt32(userDetails.Rows[0]["LoginAttempt"] != DBNull.Value ? userDetails.Rows[0]["LoginAttempt"] : 0);
                     var lastAttemptDate = Convert.ToDateTime(userDetails.Rows[0]["LastAttemptDate"] != DBNull.Value ? userDetails.Rows[0]["LastAttemptDate"] : null);

                     var maxLoginCount = Convert.ToInt32(CommonService.GetSystemConfigValue("MaxLoginCount"));
                     var loginAttemptWaitTime = Convert.ToDouble(CommonService.GetSystemConfigValue("LoginAttemptWaitTime"));

                     if (loginAttemptCount >= maxLoginCount)
                     {
                         //Updated logic so that in the case of loginAttemptWaitTime being set to 99999, will straight away disable login without checking on time. In general 99999 = disable account
                         if (loginAttemptWaitTime == 99999)
                         {
                             spanLoginStatus.InnerHtml = "Max login attempt reached. Account has been disabled.";
                             spanLoginStatus.Style["display"] = "block";
                             return;
                         }
                         else if ((DateTime.Now - lastAttemptDate).TotalMinutes < loginAttemptWaitTime)
                         {
                             spanLoginStatus.InnerHtml = "Max login attempt reached. Please try again later.";
                             spanLoginStatus.Style["display"] = "block";
                             return;
                         }                        
                     }
                 }

                 var businessEntityID = CommonService.GetSystemConfigValue("BusinessEntityID");
                 //Assigns username session variable first
                 Session[appCode + "Username"] = txtUserName.Text;
                 var uuid = HttpContext.Current.Session.SessionID;
                 new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Generating login body {username: " + txtUserName.Text + ", appcode: " + appCode + ", businessid: " + businessEntityID + ", uuid: " + uuid + "}", "BtnLogin_Click");
                 var accReq = new LoginTokenRequest() { AccountCode = txtUserName.Text, Password = txtPassword.Text, AppCode = appCode, BusinessEntityID = Convert.ToInt32(businessEntityID), UUiD = uuid };
                 var body = JsonConvert.SerializeObject(accReq, Formatting.Indented);

                 string token = CommonService.EncryptString(body);
                 var loginTokenResponse = new LoginService().Authenticate(token, txtUserName.Text);
                 if (loginTokenResponse.Valid)
                 {
                     new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Checking account inactivity", "Login");
                     //Additional validation checking
                     if (userDetails.Rows.Count > 0)
                     {
                         var lastLoginDate = Convert.ToDateTime(userDetails.Rows[0]["LastLoginDate"] != DBNull.Value ? userDetails.Rows[0]["LastLoginDate"] : null);
                         //Dormant account check
                         var isDeleteDays = Convert.ToDouble(CommonService.GetSystemConfigValue("DeleteAccountDays"));
                         var isDelete = Convert.ToBoolean(userDetails.Rows[0]["IsDelete"] != DBNull.Value ? userDetails.Rows[0]["IsDelete"] : 0);
                         if (isDelete || (lastLoginDate != DateTime.MinValue && (DateTime.Now - lastLoginDate).TotalDays > isDeleteDays))
                         {
                             if (!isDelete)
                                 CommonEntities.DeleteUser(userName, loginTokenResponse.TokenID, userName, this);

                             spanLoginStatus.InnerHtml = "Account has been in dormant for more than " + isDeleteDays + " Days inactivity.";
                             spanLoginStatus.Style["display"] = "block";
                             return;
                         }

                         //Inactive account check
                         var isActiveDays = Convert.ToDouble(CommonService.GetSystemConfigValue("InactiveAccountDays"));
                         var isActive = Convert.ToBoolean(userDetails.Rows[0]["IsActive"] != DBNull.Value ? userDetails.Rows[0]["IsActive"] : 0);
                         if (!isActive || (lastLoginDate != DateTime.MinValue && (DateTime.Now - lastLoginDate).TotalDays > isActiveDays))
                         {
                             if (isActive)
                                 CommonEntities.StatusChangeUser(userName, Convert.ToInt32(Common.Enums.AgentHubUserStatus.Disable), loginTokenResponse.TokenID, userName, this, false);

                             spanLoginStatus.InnerHtml = "Account has been inactive for more than " + isActiveDays + " Days inactivity.";
                             spanLoginStatus.Style["display"] = "block";
                             return;
                         }

                         //Assign for later PasswordChange check
                         lastPasswordChangeDate = Convert.ToDateTime(userDetails.Rows[0]["LastPasswordChangeDate"] != DBNull.Value ? userDetails.Rows[0]["LastPasswordChangeDate"] : null);

                         //Save session variable for last login date for current session before being stamped with new date
                         Session[appCode + "LastLoginDate"] = Convert.ToDateTime(userDetails.Rows[0]["LastLoginDate"] != DBNull.Value ? userDetails.Rows[0]["LastLoginDate"] : null).ToString("dd/MM/yyyy hh:mm tt");
                     }

                     //Last password change check
                     var passwordChangeDays = Convert.ToDouble(CommonService.GetSystemConfigValue("PasswordChangeDays"));
                     if (lastPasswordChangeDate != DateTime.MinValue)
                     {
                         var passTotalDays = Math.Round((DateTime.Now - lastPasswordChangeDate).TotalDays);
                         passDayLeft = passwordChangeDays - passTotalDays;
                         Session[appCode + "PasswordDaysLeft"] = passDayLeft;
                         if (passTotalDays >= passwordChangeDays)
                         {
                             new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Password not changed for more than " + passwordChangeDays + ". Redirecting to Password Reset page", "Login");
                             Response.Redirect(ConfigurationManager.AppSettings["RouteURL"] + "/Shared/ChangePassword.aspx");
                         }
                     }

                     //Only start giving authentication after all validations done.
                     //Save to session variable if valid/authenticated
                     Session[appCode + "Token"] = loginTokenResponse.TokenID;

                     var userData = loginTokenResponse.ResponseStatusEntity.UserData;
                     try
                     {
                         //var userIdentity = new LoginService().CreateIdentity(txtUserName.Text, userData);
                         UserIdentityModel userIdentityModel = new UserIdentityModel();
                         userIdentityModel = new LoginService().CreateSession(txtUserName.Text, userData);
                         Session[ConfigurationManager.AppSettings["SessionVariableName"]] = userIdentityModel;

                         if(userIdentityModel.Role == Common.Enums.UserRole.Admin.ToString() || userIdentityModel.Role == Common.Enums.UserRole.UIDAdmin.ToString())
                         {
                             string ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                             new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Get IP " + ipAddress, "Login");
                             if (string.IsNullOrEmpty(ipAddress))

                             {
                                 ipAddress = Request.ServerVariables["REMOTE_ADDR"];
                             }

                             if (!CheckIsInternalIP(ipAddress.Split(',')[0]))
                             {
                                 new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Admin login from external network.", "Login");
                                 spanLoginStatus.InnerHtml = "You are not allowed to access EPP outside of Maybank/Etiqa network. Please contact Maybank Helpdesk for further assistance.";
                                 spanLoginStatus.Style["display"] = "block";
                                 return;
                             }
                         }
                         //HttpContext.Current.GetOwinContext().Authentication.SignIn(userIdentity);
                         FormsAuthentication.SetAuthCookie(userIdentityModel.AccountCode, false);
                         new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Forms Auth Cookie Name: " + FormsAuthentication.FormsCookieName + ", Path: " + FormsAuthentication.FormsCookiePath + ", Domain: " + FormsAuthentication.CookieDomain, "Login");
                     }
                     catch (ArgumentOutOfRangeException ex)
                     {
                         spanLoginStatus.InnerHtml = "User does not have any Corporate assigned";
                         spanLoginStatus.Style["display"] = "block";

                         new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error setting up authentication: " + ex.Message, "Login");
                         return;
                     }
                     catch (Exception ex)
                     {
                         spanLoginStatus.InnerHtml = ex.Message;
                         spanLoginStatus.Style["display"] = "block";

                         new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error setting up authentication: " + ex.Message, "Login");

                         throw;
                     }
                     spanLoginStatus.InnerHtml = loginTokenResponse.ResponseStatusEntity.StatusDescription;
                     spanLoginStatus.Style["display"] = "block";
                     blnContinue = true;

                     new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Reinitializing login attempts", "Login");
                     //To cater for invalid login credentials attempts. Zero-rised since now successful
                     storedProcServ.UpdateLastLoginAttempt(userName, true);
                 }
                 else
                 {
                     spanLoginStatus.InnerHtml = loginTokenResponse.ResponseStatusEntity.StatusDescription;
                     spanLoginStatus.Style["display"] = "block";

                     new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Login not valid: " + loginTokenResponse.ResponseStatusEntity.StatusDescription, "BtnLogin_Click");

                     //To cater for Change Password for 1st time logins.
                     if (loginTokenResponse.ResponseStatusEntity.StatusDescription == "Change Password Needed.")
                     {
                         new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Redirecting to Password Reset page", "Login");
                         Response.Redirect(ConfigurationManager.AppSettings["RouteURL"] + "/Shared/ChangePassword.aspx");
                     }

                     //Remove Username Session variable if not valid
                     Session[appCode + "Username"] = null;

                     //To cater for invalid login credentials attempts.
                     if (loginTokenResponse.ResponseStatusEntity.StatusDescription == "Not authorized." || loginTokenResponse.ResponseStatusEntity.StatusDescription == "Unauthorized Access - Invalid User")
                         storedProcServ.UpdateLastLoginAttempt(userName, false);
                 }
             }
             catch (Exception ex)
             {
                 spanLoginStatus.InnerHtml = ex.Message;
                 spanLoginStatus.Style["display"] = "block";

                 new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in BtnLogin_Click: " + ex.Message, "Login");
             }


            #region Testing purpose since agentdatahub down
            /*UserIdentityModel userIdentityModel = new UserIdentityModel();
            UserData identityModel = new UserData();


            identityModel.AccountStatus = "true";
            identityModel.FullName = userName;
            identityModel.AccountCode = "";
            identityModel.EmailAddress = "";
            identityModel.MobilePhone = "";
            identityModel.OfficePhone = "";
            identityModel.Address1 = "";
            identityModel.Address2 = "";
            identityModel.Address3 = "";
            identityModel.AddressCity = "";
            identityModel.AddressState = "";
            identityModel.AddressCountry = "";
            identityModel.AddressPostcode = "";
            identityModel.BranchCode = "";
            identityModel.BusinessRegistrationNo = "20076K";
            identityModel.AgentCodes = "";

            userIdentityModel = new LoginService().CreateSession(txtUserName.Text, identityModel);
            Session[ConfigurationManager.AppSettings["SessionVariableName"]] = userIdentityModel;
            FormsAuthentication.SetAuthCookie(txtUserName.Text, false);
            Session[appCode + "Username"] = txtUserName.Text;
            Session[appCode + "Token"] = "asdasda";
            blnContinue = true;*/
            #endregion


            if (blnContinue)
            { 
                try
                {
                    Utility.SetOTP(txtUserName.Text.ToString(), ref RetOTP);
                    new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Successfully Send OTP and Direct to OTP Page", "BtnLogin_Click");
                    // Utility.SendEmailOTP(userName, RetOTP);
                    var check = ConfigurationManager.AppSettings["RouteURL"] + "2FA.aspx";
                    Response.Redirect(ConfigurationManager.AppSettings["RouteURL"] + "/2FA.aspx",false);
                }
                catch (Exception ex)
                {
                    new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Generate_OTP: " + ex.Message, "Login");
                    spanLoginStatus.InnerHtml = "Failed Generate OTP. Please Contact System Administrator";
                    spanLoginStatus.Style["display"] = "block";
                    Utility.Logout(userName, Session[appCode + "Token"].ToString());
                    return;
                }
            }
        }

        private Boolean CheckIsInternalIP(string IpAddress)
        {
            if (IpAddress == "::1") return true;

            new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, "user", "Check Internal IP " + IpAddress, "Login");
            byte[] ip = IPAddress.Parse(IpAddress).GetAddressBytes();
            new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, "user", "Check IP Complete " + ip.ToString(), "Login");
            switch (ip[0])
            {
                case 10:
                case 127:

                    return true;

                case 172:

                    return ip[1] >= 16 && ip[1] < 32;

                case 192:

                    return ip[1] == 168;

                case 202:

                    return ip[1] == 162;

                default:

                    return false;
            }
        }

        //public void Logout(string username)
        //{
        //    UserIdentityModel _UserIdentityModel = new UserIdentityModel();

        //    if (Session[ConfigurationManager.AppSettings["SessionVariableName"]] != null)
        //    {
        //        _UserIdentityModel = (UserIdentityModel)Session[ConfigurationManager.AppSettings["SessionVariableName"]];
        //        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "Logout", "Logout");
        //    }

        //    var appCode = CommonService.GetSystemConfigValue("AppCode");
        //    //Logout from AgentPortalHub 
        //    var loginToken = Session[appCode + "Token"];
        //    var loginUserName = username; //Session[appCode + "UserName"];
        //    if (loginToken != null && loginUserName != null)
        //    {
        //        var loginTokenResponse = new LoginService().LogoutToken(loginToken.ToString(), loginUserName.ToString());
        //        if (!loginTokenResponse.Valid)
        //            new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "Logout AgentPortalHub returned not valid", "Logout");
        //    }

        //    FormsAuthentication.SignOut();
        //    Session.Abandon();
        //}
    }
}