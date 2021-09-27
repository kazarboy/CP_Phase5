using EPP.CorporatePortal.Common;
using EPP.CorporatePortal.DAL.Entity;
using EPP.CorporatePortal.DAL.Service;
using EPP.CorporatePortal.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EPP.CorporatePortal.Admin
{
    public partial class ManageAccess : System.Web.UI.Page
    {
        private AuditTrail auditTrailService = new AuditTrail();
        protected void Page_Load(object sender, EventArgs e)
        {
            var userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;

            try
            {
                var accessPermission = Rights_Enum.ManageAdminTasks;
                HiddenField hdnPermission = (HiddenField)Page.Master.FindControl("hdnPermission");
                hdnPermission.Value = Enum.GetName(typeof(Rights_Enum), accessPermission);

                if (!Page.IsPostBack)
                {
                    //In case back button from other pages
                    var uName = Request.QueryString["Username"];
                    if (!string.IsNullOrEmpty(uName))
                    {
                        txtSearchString.Text = uName;
                        LoadUserList(userName);
                    }
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Page_Load: " + ex.Message, "ManageAccess");
            }
        }
        protected void BtnSearch_Click(object sender, EventArgs e)
        {
            var userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;

            LoadUserList(userName);
        }
        protected void BtnAddUser_Click(object sender, EventArgs e)
        {
            var userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;

            Response.Redirect(ResolveUrl("~/Admin/AddUsers.aspx"));
        }
        protected void BtnAllStatus_Click(object sender, EventArgs e)
        {
            var userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;

            LoadUserList(userName);
        }
        protected void BtnActiveStatus_Click(object sender, EventArgs e)
        {
            var userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;

            LoadUserList(userName, "Active");
        }
        protected void BtnSuspendedStatus_Click(object sender, EventArgs e)
        {
            var userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;

            LoadUserList(userName, "Suspended");
        }
        protected void ModalButtonHandler(object sender, EventArgs e)
        {
            var author = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;
            var appCode = CommonService.GetSystemConfigValue("AppCode");
            var businessEntityID = CommonService.GetSystemConfigValue("BusinessEntityID");
            var loginPageUrl = CommonService.GetSystemConfigValue("LoginPageUrl");

            var loginToken = Session[appCode + "Token"]. ToString();

            var service = new StoredProcService(author);

            var username = hdnUsername.Value;
            var buttonHandler = hdnButtonHandler.Value;

            try
            {
                if (buttonHandler == "Delete")
                    DeleteUser();
                else if (buttonHandler == "Reactivate")
                    CommonEntities.StatusChangeUser(username, Convert.ToInt32(Common.Enums.AgentHubUserStatus.Active), loginToken, author, this);
                else if (buttonHandler == "Suspend")
                    CommonEntities.StatusChangeUser(username, Convert.ToInt32(Common.Enums.AgentHubUserStatus.Disable), loginToken, author, this);

                //Reload user listing
                LoadUserList(author);
            }
            catch
            {

            }

        }
        private void LoadUserList(string userName)
        {
            LoadUserList(userName, "");
        }
        private void LoadUserList(string userName, string statusFilter)
        {
            //var user = HttpContext.Current.User as ClaimsPrincipal;
            //var identity = user.Identity as ClaimsIdentity;
            //var bizRegNo = identity.Claims.Where(c => c.Type == "BusinessRegistrationNo").Select(c => c.Value).SingleOrDefault();
            var bizRegNo = ((CorporatePortalSite)this.Master)._UserIdentityModel.BusinessRegistrationNo;
            var role = ((CorporatePortalSite)this.Master)._UserIdentityModel.Role;

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadUserList: Started", "ManageAccess");

            var service = new StoredProcService(userName);

            //load menu
            DataTable dt = service.GetUserAccessList(txtSearchString.Text, role);

            //Set up status counts
            lblAllStatusCnt.Text = dt.Rows.Count.ToString();
            lblActiveCnt.Text = dt.AsEnumerable().Where(row => row["Status"].ToString().Equals("Active")).Count().ToString();
            lblSuspendedCnt.Text = dt.AsEnumerable().Where(row => row["Status"].ToString().Equals("Suspended")).Count().ToString();

            //Filter according to status if chosen
            if (!String.IsNullOrEmpty(statusFilter))
            {
                var dtFiltered = dt.AsEnumerable().Where(row => row["Status"].ToString().Equals(statusFilter));
                if (dtFiltered.Any())
                    dt = dtFiltered.CopyToDataTable();
                else
                    dt = new DataTable();

                btnAllStatus.CssClass = "btn btnCount";
                btnActiveStatus.CssClass = statusFilter == "Active" ? "btn btnCount greenActive" : "btn btnCount";
                btnSuspendedStatus.CssClass = statusFilter == "Suspended" ? "btn btnCount redActive" : "btn btnCount";
            }
            else
            {
                btnAllStatus.CssClass = "btn btnCount yellowActive";
                btnActiveStatus.CssClass = "btn btnCount";
                btnSuspendedStatus.CssClass = "btn btnCount";
            }

            rptUserList.DataSource = dt;
            rptUserList.DataBind();

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadUserList: Finished", "ManageAccess");
        }
        protected void AddUser(object sender, EventArgs e)
        {
            var author = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;
            var appCode = CommonService.GetSystemConfigValue("AppCode");
            var businessEntityID = CommonService.GetSystemConfigValue("BusinessEntityID");
            var loginPageUrl = CommonService.GetSystemConfigValue("LoginPageUrl");

            var loginToken = Session[appCode + "Token"];

            var service = new StoredProcService(author);

            Button btn = (Button)sender;
            var fileID = Convert.ToInt32(btn.CommandArgument);

            try
            {
                var userName = "";
                var emailAddress = "";
                var fullName = "";
                var gender = "";
                var icNo = "";
                var mobilePhone = "";
                //TODO: BusinessEntityID should be an input
                var businessRegNo = "";

                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "Processing username [" + userName + "]", "AddUser_Click");
                var userReq = new CreateUserTokenRequest() { BusinessEntityID = Convert.ToInt32(businessEntityID), AppCode = appCode, EmailAddress = emailAddress, FullName = fullName, Gender = gender, ICNo = icNo, MobilePhone = mobilePhone, Username = userName, Url = loginPageUrl };
                var body = JsonConvert.SerializeObject(userReq, Formatting.Indented);

                string token = CommonService.EncryptString(body);
                var createTokenResponse = new LoginService().CreateUser(token, author, loginToken.ToString());
                if (createTokenResponse.Valid)
                {
                    //service.ProcessCreateUser(userName, fullName, businessRegNo, false);
                    new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, author, "Create User successfully to Agent Portal Hub", "AddUser_Click");
                }
                else
                {
                    new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, author, "Create User not valid: " + createTokenResponse.ResponseStatusEntity.StatusDescription, "AddUser_Click");
                }

                Utility.RegisterStartupScriptHandling(this, "Success", "alert('User created successfully');", true, true, author);
            }
            catch (Exception ex)
            {
                //In case got any uncaptured errors.
                UploadLogging("Error: Error in creating user", "Error in create user function: " + ex.Message, author, true);
                btn.Enabled = true;
            }
            finally
            {
                //To cater for all cases to re-enable the upload button.
                btn.Enabled = true;
            }
        }
        private void DeleteUser()
        {
            var author = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;
            var appCode = CommonService.GetSystemConfigValue("AppCode");

            var loginToken = Session[appCode + "Token"].ToString();

            var service = new StoredProcService(author);

            var username = hdnUsername.Value;

            //Calling common function for deleting user
            CommonEntities.DeleteUser(username, loginToken, author, this);
        }
        private void UploadLogging(string userMsg, string errMsg, string userName, bool auditLogging)
        {
            if (!string.IsNullOrEmpty(userMsg))
                ScriptManager.RegisterStartupScript(this, this.GetType(), "Error", "$('#update-member').modal('toggle');$('.uploadlogo').parent().children('span').html('" + userMsg + "');", true);

            if (!string.IsNullOrEmpty(errMsg) && auditLogging)
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, errMsg, "AddUser");
        }
    }
}