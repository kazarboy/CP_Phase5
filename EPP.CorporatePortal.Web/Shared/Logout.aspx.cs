using EPP.CorporatePortal.DAL.Service;
using System;
using System.Linq;
//using System.Security.Claims;
using System.Web;
using System.Web.Security;
using EPP.CorporatePortal.DAL.Model;
using System.Configuration;

namespace EPP.CorporatePortal
{
    public partial class Logout : System.Web.UI.Page
    {
        private AuditTrail auditTrailService = new AuditTrail();
        protected void Page_Load(object sender, EventArgs e)
        {
            //var identity = (System.Security.Claims.ClaimsIdentity)Context.User.Identity;
            //var userName = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).FirstOrDefault();
            UserIdentityModel _UserIdentityModel = new UserIdentityModel();

            if (Session[ConfigurationManager.AppSettings["SessionVariableName"]] != null)
            {
                _UserIdentityModel = (UserIdentityModel)Session[ConfigurationManager.AppSettings["SessionVariableName"]];
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "Logout", "Logout");
            }

            var appCode = CommonService.GetSystemConfigValue("AppCode");
            //Logout from AgentPortalHub 
            var loginToken = Session[appCode + "Token"];
            var loginUsername = Session[appCode + "Username"];
            if (loginToken != null && loginUsername != null)
            {
                var loginTokenResponse = new LoginService().LogoutToken(loginToken.ToString(), loginUsername.ToString());
                if (!loginTokenResponse.Valid)
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "Logout AgentPortalHub returned not valid", "Logout");
            }

            //var ctx = Request.GetOwinContext();
            //var authenticationManager = ctx.Authentication;
            //authenticationManager.SignOut();

            FormsAuthentication.SignOut();
            Session.Abandon();
            //FormsAuthentication.RedirectToLoginPage();
            Response.Redirect(ConfigurationManager.AppSettings["RouteURL"] + "/Login.aspx");
            

        }
    }
}