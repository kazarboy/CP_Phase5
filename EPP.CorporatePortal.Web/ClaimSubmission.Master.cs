using EPP.CorporatePortal.Common;
using EPP.CorporatePortal.DAL.Model;
using EPP.CorporatePortal.DAL.Service;
using EPP.CorporatePortal.Helper;
using EPP.CorporatePortal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using static EPP.CorporatePortal.Common.Enums;

namespace EPP.CorporatePortal
{
    public partial class ClaimSubmission : System.Web.UI.MasterPage
    {
        private AuditTrail auditTrailService = new AuditTrail();
        private const string AntiXsrfTokenKey = "__EPPCorporatePortalEQToken";
        private const string AntiXsrfUserNameKey = "__EPPCorporatePortalEQUserName";
        private string _antiXsrfTokenValue;
        public UserIdentityModel _UserIdentityModel = new UserIdentityModel();
        protected void Page_Init(object sender, EventArgs e)
        {
            //First, check for the existence of the Anti-XSS cookie
            var requestCookie = Request.Cookies[AntiXsrfTokenKey];

            if (Session[ConfigurationManager.AppSettings["SessionVariableName"]] != null)
            {
                _UserIdentityModel = (UserIdentityModel)Session[ConfigurationManager.AppSettings["SessionVariableName"]];
            }
            else
            {
                Response.Redirect(ConfigurationManager.AppSettings["RouteURL"] + "/shared/logout.aspx");
            }
            //If the CSRF cookie is found, parse the token from the cookie.
            //Then, set the global page variable and view state user
            //key. The global variable will be used to validate that it matches 
            //in the view state form field in the Page.PreLoad method.
            if (requestCookie != null
                && Guid.TryParse(requestCookie.Value, out Guid requestCookieGuidValue))
            {
                //Set the global token variable so the cookie value can be
                //validated against the value in the view state form field in
                //the Page.PreLoad method.
                _antiXsrfTokenValue = requestCookie.Value;

                //Set the view state user key, which will be validated by the
                //framework during each request
                Page.ViewStateUserKey = _antiXsrfTokenValue;
            }
            //If the CSRF cookie is not found, then this is a new session.
            else
            {
                //Generate a new Anti-XSRF token
                _antiXsrfTokenValue = Guid.NewGuid().ToString("N");

                //Set the view state user key, which will be validated by the
                //framework during each request
                Page.ViewStateUserKey = _antiXsrfTokenValue;

                //Create the non-persistent CSRF cookie
                var responseCookie = new HttpCookie(AntiXsrfTokenKey)
                {
                    //Set the HttpOnly property to prevent the cookie from
                    //being accessed by client side script
                    HttpOnly = true,

                    //Add the Anti-XSRF token to the cookie value
                    Value = _antiXsrfTokenValue
                };

                //If we are using SSL, the cookie should be set to secure to
                //prevent it from being sent over HTTP connections
                if (FormsAuthentication.RequireSSL &&
                    Request.IsSecureConnection)
                {
                    responseCookie.Secure = true;
                }

                //Add the CSRF cookie to the response
                Response.Cookies.Set(responseCookie);
            }

            Page.PreLoad += Master_Page_PreLoad;
        }

        protected void Master_Page_PreLoad(object sender, EventArgs e)
        {
            //During the initial page load, add the Anti-XSRF token and user
            //name to the ViewState
            if (!IsPostBack)
            {
                //Set Anti-XSRF token
                ViewState[AntiXsrfTokenKey] = Page.ViewStateUserKey;

                //If a user name is assigned, set the user name
                ViewState[AntiXsrfUserNameKey] =
                       _UserIdentityModel.Name ?? String.Empty;
            }
            //During all subsequent post backs to the page, the token value from
            //the cookie should be validated against the token in the view state
            //form field. Additionally user name should be compared to the
            //authenticated users name
            else
            {
                //Validate the Anti-XSRF token
                if ((string)ViewState[AntiXsrfTokenKey] != _antiXsrfTokenValue
                    || (string)ViewState[AntiXsrfUserNameKey] !=
                         (_UserIdentityModel.Name ?? String.Empty))
                {
                    throw new InvalidOperationException("Validation of " +
                                        "Anti-XSRF token failed.");
                }
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            var appCode = CommonService.GetSystemConfigValue("AppCode");

            //Check with AgentPortalHub again for authentication 
            var loginToken = Session[appCode + "Token"];
            var loginUsername = Session[appCode + "Username"];

            if (loginToken != null && loginUsername != null)
            {
                var loginTokenResponse = new LoginService().ValidateToken(loginToken.ToString(), loginUsername.ToString());
                if (!loginTokenResponse.Valid)
                    ScriptManager.RegisterStartupScript(this, GetType(), "Error", "alert('Session timeout. Kindly login again');location.href='" + ResolveUrl("~/Shared/Logout.aspx") + "'", true);
            }
            else
                ScriptManager.RegisterStartupScript(this, GetType(), "Error", "alert('Session timeout. Kindly login again');location.href='" + ResolveUrl("~/Shared/UnAuthorized.aspx") + "'", true);

            var accessPermission = hdnPermission.Value;
            if (!CheckPageAccess(accessPermission))
            {
                Response.Redirect(ConfigurationManager.AppSettings["RouteURL"] + "/Shared/UnAuthorized.aspx");
            }
        }
        private bool CheckPageAccess(string right)
        {
            var retValue = false;
            if (!String.IsNullOrEmpty(right))
            {
                var rightEnum = (Rights_Enum)Enum.Parse(typeof(Rights_Enum), right.ToString());
                List<Rights_Enum> RequestedPermissions = new List<Rights_Enum>
            {
                rightEnum
            };
                var userName = _UserIdentityModel.PrincipalName;
                var usrPermissions = GetUserPermission(userName);
                if (AuthorizationHelper.HasPermission(usrPermissions, RequestedPermissions, ComparisonType.All))
                {
                    retValue = true;
                }
            }
            else
            {
                retValue = false;
            }
            return retValue;
        }
        protected IList<Rights_Enum> GetUserPermission(string userName)
        {
            var list = new List<Rights_Enum>();

            var roles = new RolesService().GetUserRoles(userName);

            foreach (var role in roles)
            {
                var rights = new UserService().GetRoleRightsEnumList(role);
                list.AddRange(rights);
            }
            return list;
        }
        protected void Exit(object sender, EventArgs e)
        {
            var exitURL = hdnExitURL.Value;

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, _UserIdentityModel.PrincipalName, "Exiting back to: " + exitURL, "MemberListing");

            Response.Redirect(exitURL);
        }
        protected void Step1_OnClick(object sender, EventArgs e)
        {
            var clickURL = hdnStep1.Value;

            if (!string.IsNullOrEmpty(clickURL))
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, _UserIdentityModel.PrincipalName, "Returning back to: " + clickURL, "MemberListing");

                Response.Redirect(clickURL);
            }
        }
        protected void Step2_OnClick(object sender, EventArgs e)
        {
            var clickURL = hdnStep2.Value;

            if (!string.IsNullOrEmpty(clickURL))
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, _UserIdentityModel.PrincipalName, "Returning back to: " + clickURL, "MemberListing");

                Response.Redirect(clickURL);
            }
        }
        protected void Step3_OnClick(object sender, EventArgs e)
        {
            var clickURL = hdnStep3.Value;

            if (!string.IsNullOrEmpty(clickURL))
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, _UserIdentityModel.PrincipalName, "Returning back to: " + clickURL, "MemberListing");

                Response.Redirect(clickURL);
            }
        }
        protected void Step4_OnClick(object sender, EventArgs e)
        {
            var clickURL = hdnStep4.Value;

            if (!string.IsNullOrEmpty(clickURL))
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, _UserIdentityModel.PrincipalName, "Returning back to: " + clickURL, "MemberListing");

                Response.Redirect(clickURL);
            }
        }
        protected void Step5_OnClick(object sender, EventArgs e)
        {
            var clickURL = hdnStep5.Value;

            if (!string.IsNullOrEmpty(clickURL))
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, _UserIdentityModel.PrincipalName, "Returning back to: " + clickURL, "MemberListing");

                Response.Redirect(clickURL);
            }
        }
        protected void Step6_OnClick(object sender, EventArgs e)
        {
            var clickURL = hdnStep6.Value;

            if (!string.IsNullOrEmpty(clickURL))
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, _UserIdentityModel.PrincipalName, "Returning back to: " + clickURL, "MemberListing");

                Response.Redirect(clickURL);
            }
        }
    }
}