
using EPP.CorporatePortal.Common;
using EPP.CorporatePortal.DAL.EDMX;
using EPP.CorporatePortal.DAL.Service;
using EPP.CorporatePortal.DAL.Model;
using EPP.CorporatePortal.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static EPP.CorporatePortal.Common.Enums;
using EPP.CorporatePortal.Models;
using System.Web.Security;
using System.Configuration;

namespace EPP.CorporatePortal
{
    public partial class CorporatePortalSite : System.Web.UI.MasterPage
    {
        private const string AntiXsrfTokenKey = "__EPPCorporatePortalEQToken";
        private const string AntiXsrfUserNameKey = "__EPPCorporatePortalEQUserName";
        private string _antiXsrfTokenValue;
        public UserIdentityModel _UserIdentityModel = new UserIdentityModel();
        protected void Page_Init(object sender, EventArgs e)
        {
            //First, check for the existence of the Anti-XSS cookie
            var requestCookie = Request.Cookies[AntiXsrfTokenKey];

            if(Session[ConfigurationManager.AppSettings["SessionVariableName"]] != null)
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

            //Password Change Reminder
            var passDayLeft = Convert.ToInt32(Session[appCode + "PasswordDaysLeft"]);
            if (passDayLeft != 0 && passDayLeft <= 7)
            {
                string scriptText = "if (confirm(\"Password expiring in " + passDayLeft.ToString() + " Days. Would you like to reset password?\") == true) {window.location.href=\"" + ResolveUrl("~/Shared/ChangePassword.aspx") + "\";};";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "Message", scriptText, true);
                //Set it to null since should just show message on login only.
                Session[appCode + "PasswordDaysLeft"] = null;
            }

            //var identity = (System.Security.Claims.ClaimsIdentity)Context.User.Identity;
            //var fullName = identity.Claims.Where(c => c.Type == ClaimTypes.Name ).Select(c => c.Value).SingleOrDefault();
            var fullName = _UserIdentityModel.Name;
            //var parentCorporate = identity.Claims.Where(c => c.Type == "ParentCorporate").Select(c => c.Value).SingleOrDefault();
            var parentCorporate = _UserIdentityModel.ParentCorporate;
            //var parentBizRegNo = identity.Claims.Where(c => c.Type == "ParentBizRegNo").Select(c => c.Value).SingleOrDefault(); 
            var parentBizRegNo = _UserIdentityModel.ParentBizRegNo;
            //var userName = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).SingleOrDefault();
            var userName = _UserIdentityModel.PrincipalName;
            //var bizRegNo = identity.Claims.Where(c => c.Type == "BusinessRegistrationNo").Select(c => c.Value).SingleOrDefault();
            var bizRegNo = _UserIdentityModel.BusinessRegistrationNo;
            txtName.InnerHtml = Server.HtmlEncode(fullName);

            var newCorpId = Utility.EncodeAndDecryptCorpId( Request.QueryString["CorpId"])  ?? "0";
            var newUCorpId = Utility.EncodeAndDecryptCorpId(Request.QueryString["UCorpId"]) ?? "0";

            var storedProcServ = new StoredProcService(userName);
            var uid = storedProcServ.GetCorporateUId(parentBizRegNo, parentCorporate);
            var UCorpId = uid.Rows[0]["Id"].ToString();

            _UserIdentityModel.UCorpId = UCorpId;


            if (newCorpId != "0" && newUCorpId != "0")
            {         
                var corporate = storedProcServ.GetCorporateById(newCorpId, newUCorpId);
                if (corporate.Rows.Count > 0)
                {
                    var serv = new LoginService();
                    //serv.UpdateClaim(corporate.Rows[0]["Name"].ToString());
                    _UserIdentityModel = serv.UpdateSession(loginUsername.ToString(), corporate.Rows[0]["Name"].ToString(), _UserIdentityModel);
                    lblAccName.Text = corporate.Rows[0]["Name"].ToString();
                }
                else
                {
                    lblAccName.Text =parentCorporate;
                }

            }

            hdnParentCorpId.Value = Utility.Encrypt(parentBizRegNo);
            hdnUCorpId.Value = Utility.Encrypt(UCorpId);

            //To display user's lastlogin datetime.
            if (Session[appCode + "LastLoginDate"] != null)
            {
                lblLoggedInDateTime.Text = Session[appCode + "LastLoginDate"].ToString();
                lblLoggedInDateTime2.Text = Session[appCode + "LastLoginDate"].ToString();
            }

            var cu = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cu.Name);
            Page.DataBind();

            LoadSubsidaries(newCorpId, bizRegNo, parentCorporate, parentBizRegNo);

            LoadMenu(userName);
        }

        private void LoadMenu(string userName)
        {
            //load menu
            var menus = new  StoredProcService(userName).GetMenus(userName).ToList();//returns dataRow

            for (int i=0; i < menus.Count(); i++)
            {
                var checkmenuname = menus[i].Table.Rows[i];
                string check = checkmenuname["Name"].ToString();

                if (check == "UserGuide")
                {
                    var movemenu = menus[i];
                    menus.RemoveAt(i);
                    menus.Add(movemenu);
                    break;
                }
            }

            DataTable dt = menus.CopyToDataTable();
            //DataTable dt = menus.AsEnumerable().OrderBy(x=>x["Name"]).CopyToDataTable(); //copy to a DataTable
          
            rptMenu.DataSource = dt;
            rptMenu.DataBind();

        }
        private void LoadSubsidaries(string corporateId, string bizRegNo, string parentCorporate, string parentBizRegNo)
        {
            //var user = HttpContext.Current.User as ClaimsPrincipal;
            //var identity = user.Identity as ClaimsIdentity;
            //var userName = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).SingleOrDefault();
            var userName = _UserIdentityModel.PrincipalName;

            //load Subsidaries
            var service = new CorporateService();
            var storedProcService=new  StoredProcService(userName);
            var parentId = bizRegNo;
            if(bizRegNo != parentBizRegNo)
            {
                parentId = service.GetParentId(parentBizRegNo);
            }                
            var parentSubsidaries = storedProcService.GetCorporateSubsidaries(parentId);
           
            rptSubsidaries.DataSource = parentSubsidaries;
            rptSubsidaries.DataBind();
            var total = parentSubsidaries.Rows.Count;
            lblSubCount2.Text = total.ToString();

            var selectedCoporate = corporateId != "" && corporateId != "0" ? corporateId : parentId;
            var subsidaries = storedProcService.GetCorporateSubsidaries(selectedCoporate);
            var total2 = subsidaries.Rows.Count;
            lblSubCount.Text = total2.ToString();

            lblAccName2.Text = parentCorporate.ToString();
        }
       
        public string GetLocalizedString(string name)
        {
            var check = GetGlobalResourceObject("resource", name).ToString();
            return GetGlobalResourceObject("resource", name).ToString();
        }

        private bool CheckPageAccess(string right)
        {
            var retValue = false;
            if (! String.IsNullOrEmpty(right))
            {
                var rightEnum = (Rights_Enum)Enum.Parse(typeof(Rights_Enum), right.ToString());
                List<Rights_Enum> RequestedPermissions = new List<Rights_Enum>
            {
                rightEnum
            };
                //var identity = (System.Security.Claims.ClaimsIdentity)HttpContext.Current.User.Identity;
                //var userName = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).SingleOrDefault();
                var userName = _UserIdentityModel.PrincipalName;
                var usrPermissions = GetUserPermission(userName);
                if (AuthorizationHelper.HasPermission(usrPermissions, RequestedPermissions, ComparisonType.All))
                {
                    retValue= true;
                }
            }
            else
            {
                retValue= false;
            }
            return retValue;
        }

        protected IList<Rights_Enum> GetUserPermission(string userName)
        {
            var list = new List<Rights_Enum>();

            var roles = new  RolesService().GetUserRoles(userName);

            foreach (var role in roles)
            {
                var rights = new  UserService().GetRoleRightsEnumList(role);
                list.AddRange(rights);
            }
            return list;//new List<PermissionRule>();
        }
      
        public void btnSearchCorp_Click(object sender, EventArgs e)
        {
            var service = new CorporateService();
            //var identity = (System.Security.Claims.ClaimsIdentity)Context.User.Identity;
            //var parentBizRegNo = identity.Claims.Where(c => c.Type == "ParentBizRegNo").Select(c => c.Value).SingleOrDefault();
            var parentBizRegNo = _UserIdentityModel.ParentBizRegNo;
            //var userName = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).SingleOrDefault();
            var userName = _UserIdentityModel.PrincipalName;
            var parentId = service.GetParentId(parentBizRegNo);
            var items = new StoredProcService(userName).GetCorporateSubsidaries(parentId);
            var input = "";
            //var input = txtSearch.Value;
            items.CaseSensitive = false;
            var searchResult = items.Select(" Name like '%" + input + "%'");
            if (searchResult.Any())
            {
                rptSubsidaries.DataSource = searchResult.CopyToDataTable();
                rptSubsidaries.DataBind();
            }
            else
            {
                rptSubsidaries.DataSource = new DataTable();//dummy
                rptSubsidaries.DataBind();
            }
             

        }

        protected string GetTitleAndName(object dataItem)
        {
            string value = DataBinder.Eval(dataItem, "Name").ToString();
            return GetLocalizedString(value);
 
        }

        protected string GetActionPath(object dataItem)
        {
            string newCorpId = "0";
            string newUCorpId = "0";
            var param =  Request.QueryString["CorpId"]??"";
            var UCorpId = Request.QueryString["UCorpId"] ?? "";
            
            param = Utility.EncodeAndDecryptCorpId(param);
            UCorpId = Utility.EncodeAndDecryptCorpId(UCorpId);

            if (String.IsNullOrEmpty(param))
            {
                //var identity = (System.Security.Claims.ClaimsIdentity)Context.User.Identity;
                //string sInfo = identity.Claims.Where(c => c.Type == "UserCorporates").Select(c => c.Value).SingleOrDefault();
                string sInfo = _UserIdentityModel.UserCorporates;
                var objcorporate = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Corporate>>(sInfo);

                var corpId = objcorporate[0].SourceId.ToString();
                if (Utility.IsEncrypted(corpId))
                {
                    newCorpId = corpId;
                }
                else
                {
                    newCorpId = Utility.Encrypt(corpId);
                }
               
            }
            else
            {
                if (Utility.IsEncrypted(param))
                {
                    newCorpId = param;
                }
                else
                {
                    var encrypedParam = Utility.Encrypt(param);
                    newCorpId = encrypedParam;
                }
               
            }

            if (String.IsNullOrEmpty(UCorpId))
            {
                string sInfo = _UserIdentityModel.UserCorporates;
                var objcorporate = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Corporate>>(sInfo);
                var storedProcServ = new StoredProcService(_UserIdentityModel.PrincipalName);
                var uid = storedProcServ.GetCorporateUId(objcorporate[0].SourceId.ToString(), objcorporate[0].Name.ToString());
                newUCorpId = Utility.Encrypt(uid.Rows[0]["Id"].ToString());
            }
            else if (Utility.IsEncrypted(UCorpId))
            {
                newUCorpId = UCorpId;
            }
            else
            {
                var encrypedParam = Utility.Encrypt(UCorpId);
                newUCorpId = encrypedParam;
            }


            var retValue = String.Empty;
            string value = DataBinder.Eval(dataItem, "ActionMenuPath").ToString();
            if (newCorpId != "0" && newCorpId != "")
            {
                retValue = value + "?CorpId=" + newCorpId + "&UCorpId=" + newUCorpId;
            }
            else
            {
                retValue = value;
            }
            return retValue;

        }
        protected  string EncryptCorpId(object dataItem)
        {
            string sourceId = Convert.ToString(DataBinder.Eval(dataItem, "SourceId"));

            var returnValue = Utility.Encrypt(sourceId);
             
            
            return returnValue;

        }

        protected string EncryptUid(object dataItem)
        {
            string AccNo = Convert.ToString(DataBinder.Eval(dataItem, "Id"));

            var returnValue = Utility.Encrypt(AccNo);

            return returnValue;
        }
    }
    }