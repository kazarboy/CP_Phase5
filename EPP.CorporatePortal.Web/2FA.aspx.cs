using EPP.CorporatePortal.DAL.Entity;
using EPP.CorporatePortal.DAL.Service;
using EPP.CorporatePortal.DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EPP.CorporatePortal.Models;
using System.Configuration;
using System.Web.Security;

namespace EPP.CorporatePortal
{
    public partial class _2FA : System.Web.UI.Page
    {
        private static string appCode = CommonService.GetSystemConfigValue("AppCode");
        private static string UserName = "";
        private AuditTrail auditTrailService = new AuditTrail();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                hdnUserName.Value = Session[appCode + "UserName"].ToString();
                FaCodesend.InnerHtml = " " + DateTime.Now.ToString() + " ";
            }
            Page.DataBind();
        }

        protected void BtnSubmit_Click(object sender, EventArgs e)
        {
            UserName = hdnUserName.Value;
            var appCode = CommonService.GetSystemConfigValue("AppCode");
            var storedProcServ = new StoredProcService(UserName);

            DateTime RecordTime = Utility.RetrieveGenOTP(UserName);
            // Retrieve from SystemConfig in seconds
            DateTime TimeOut = RecordTime.AddSeconds(Convert.ToInt32(CommonService.GetSystemConfigValue("OTPExiry"))); //RecordTime.AddSeconds(Utility.RetrieveExOTP("OTPExiry"));
            DateTime CurTime = DateTime.Now;

            try
            {
                if (CurTime > TimeOut)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, UserName, "OTP Expired", "Login_2FA");
                    span1.Style["display"] = "initial";
                    txtFACode.Text = "";
                }
                else
                {
                    string OTP = Utility.RetrieveOTP(UserName);
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, UserName, "Succesful Retrieve OTP", "Login_2FA");

                    if (OTP == txtFACode.Text.ToString())
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, UserName, "OTP Valid", "Login_2FA");
                        var homePage = Utility.GetHomePageByRole(UserName);
                        //update last login date                
                        storedProcServ.UpdateLastLoginDate(UserName);
                        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, UserName, "ok nLogin", "login");
                        Response.Write("OK User");
                        Response.Redirect(ConfigurationManager.AppSettings["RouteURL"] + homePage, false);
                    }
                    else
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, UserName, "OTP Not Valid", "Login_2FA");
                        span4.Style["display"] = "initial";
                    }
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, UserName, "2FA_Btn_Submit Error : " +ex.ToString(), "Login_2FA");
            }    
        }

        protected void BtnCancel_Click(object sender, EventArgs e)
        {
            UserIdentityModel _UserIdentityModel = new UserIdentityModel();

            if (Session[ConfigurationManager.AppSettings["SessionVariableName"]] != null)
            {
                _UserIdentityModel = (UserIdentityModel)Session[ConfigurationManager.AppSettings["SessionVariableName"]];
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "Logout", "Logout");
            }

            var appCode = CommonService.GetSystemConfigValue("AppCode");
            //Logout from AgentPortalHub 
            var loginToken = Session[appCode + "Token"];
            var loginUserName = Session[appCode + "UserName"];
            if (loginToken != null && loginUserName != null)
            {
                var loginTokenResponse = new LoginService().LogoutToken(loginToken.ToString(), loginUserName.ToString());
                if (!loginTokenResponse.Valid)
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "Logout AgentPortalHub returned not valid", "Logout");
            }

            FormsAuthentication.SignOut();
            Session.Abandon();
            Response.Redirect(ConfigurationManager.AppSettings["RouteURL"] + "/Login.aspx");
        }

        protected void BtnResend_Click(object sender, EventArgs e)
        {
            string RetOTP = string.Empty;
            UserName = Session[appCode + "UserName"].ToString();
            Utility.SetOTP(UserName, ref RetOTP);
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, UserName, "Successful Resend OTP", "Login_2FA");
            Utility.SendEmailOTP(UserName,RetOTP);
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, UserName, "Successfully ReSend OTP via Email", "Login_2FA");

            span1.Style["display"] = "none";
            span2.Style["display"] = "none";
            span4.Style["display"] = "none";

            span3.Style["display"] = "initial";
            FaCodesend.InnerHtml = " " + DateTime.Now.ToString() + " ";          
        }
    }
}