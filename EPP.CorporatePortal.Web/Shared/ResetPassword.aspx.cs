using EPP.CorporatePortal.DAL.Entity;
using EPP.CorporatePortal.DAL.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EPP.CorporatePortal.Shared
{
    public partial class ResetPassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //In case some logics are needed later on
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
                    var userName = TxtUsername.Text;                 

                    var storedProcServ = new StoredProcService(userName);

                    //Checks if email address exists in db and gets the username
                    var userData = storedProcServ.GetUserDetails(userName);
                    if (userData.Rows.Count > 0)
                    {
                        var emailAddress = userData.Rows[0]["EmailAddress"].ToString();

                        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Generating reset password body {username: " + userName + ", password: ''" + ", appcode: " + appCode + ", businessid: " + businessEntityID + ", emailAddress: " + emailAddress + "}", "BtnResetPassword_Click");
                        var accReq = new ResetPasswordTokenRequest() { AccountCode = userName, Password = "", AppCode = appCode, BusinessEntityID = Convert.ToInt32(businessEntityID), EmailAddress = emailAddress };
                        var body = JsonConvert.SerializeObject(accReq, Formatting.Indented);

                        string token = CommonService.EncryptString(body);
                        var resetTokenResponse = new LoginService().ResetPassword(token, userName);
                        if (resetTokenResponse.Valid)
                        {
                            new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Reset Password successfully to Agent Portal Hub", "BtnResetPassword_Click");
                            //Save the last time user changed password
                            storedProcServ.UpdateLastPasswordChangeDate(userName);

                            blnContinue = true;
                        }
                        else
                        {
                            new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Reset Password not valid: " + resetTokenResponse.ResponseStatusEntity.StatusDescription, "BtnResetPassword_Click");

                            spanResetStatus.Style["display"] = "block";
                            return;                            
                        }
                    }
                    else
                    {
                        spanResetStatus.Style["display"] = "block";
                        return;
                    }
                    
                }
                catch (Exception ex)
                {
                    var userName = TxtUsername.Text;

                    //spanResetStatus.InnerHtml = "Error occured";
                    //spanResetStatus.Style["display"] = "block";

                    new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in BtnResetPassword_Click: " + ex.Message, "ResetPassword");
                }
                if (blnContinue)
                {
                    //User will need to relogin using the new login credentials
                    Response.Redirect(ConfigurationManager.AppSettings["RouteURL"] + "/Shared/ResetPasswordSuccess.aspx", false);
                }
            }
            else
            {
                //spanResetStatus.InnerHtml = "Please make sure all fields are filled in correctly";
                //spanResetStatus.Style["display"] = "block";
            }

        }
    }
}