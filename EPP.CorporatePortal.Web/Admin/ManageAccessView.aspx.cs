using EPP.CorporatePortal.Common;
using EPP.CorporatePortal.DAL.Service;
using EPP.CorporatePortal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EPP.CorporatePortal.Admin
{
    public partial class ManageAccessView : System.Web.UI.Page
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

                var uName = Request.QueryString["Username"];
                hdnUsername.Value = uName;
                LoadUserDetails(uName);
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Page_Load: " + ex.Message, "ManageAccessEdit");
            }
        }
        private void LoadUserDetails(string userName)
        {
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadUserList: Started", "ManageAccessEdit");

            var service = new StoredProcService(userName);

            //load menu
            DataTable dt = service.GetUserDetails(userName);

            if (dt.Rows.Count > 0)
            {
                //Get User Details
                var emailAddress = dt.Rows[0]["EmailAddress"].ToString();
                var fullName = dt.Rows[0]["FullName"].ToString();
                var gender = dt.Rows[0]["Gender"].ToString();
                var icNo = Utility.EncodeAndDecryptCorpId(dt.Rows[0]["ICNo"].ToString());
                var mobilePhone = Utility.EncodeAndDecryptCorpId(dt.Rows[0]["MobilePhone"].ToString());

                //Change button actions as accordingly
                if (Convert.ToBoolean(dt.Rows[0]["IsActive"].ToString()))
                {
                    btnSuspendActive.Text = "Suspend User";
                    btnSuspendActive.Click += new EventHandler(SuspendUser);
                }
                else
                {
                    btnSuspendActive.Text = "Reactivate User";
                    btnSuspendActive.Click += new EventHandler(ReactivateUser);
                }

                lblUsername1.Text = userName;
                pUsername.InnerText = userName;
                pEmailAddress.InnerText = emailAddress;
                pFullName.InnerText = fullName;
                pICNo.InnerText = icNo;
                pMobileNo.InnerText = mobilePhone;
                pGender.InnerText = gender == "M" ? "Male" : "Female";                
            }

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadUserList: Finished", "ManageAccessEdit");
        }
        protected void DeleteUser(object sender, EventArgs e)
        {
            var author = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;
            var appCode = CommonService.GetSystemConfigValue("AppCode");

            var loginToken = Session[appCode + "Token"].ToString();

            var service = new StoredProcService(author);

            var username = hdnUsername.Value;

            //Calling common function for deleting user
            CommonEntities.DeleteUser(username, loginToken, author, this);
        }
        protected void SuspendUser(object sender, EventArgs e)
        {
            var author = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;
            var appCode = CommonService.GetSystemConfigValue("AppCode");

            var loginToken = Session[appCode + "Token"].ToString();

            var service = new StoredProcService(author);

            var username = hdnUsername.Value;

            //Calling common function for changing user status
            CommonEntities.StatusChangeUser(username, Convert.ToInt32(Common.Enums.AgentHubUserStatus.Disable.ToString()), loginToken, author, this);
        }
        protected void ReactivateUser(object sender, EventArgs e)
        {
            var author = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;
            var appCode = CommonService.GetSystemConfigValue("AppCode");

            var loginToken = Session[appCode + "Token"].ToString();

            var service = new StoredProcService(author);

            var username = hdnUsername.Value;

            //Calling common function for changing user status
            CommonEntities.StatusChangeUser(username, Convert.ToInt32(Common.Enums.AgentHubUserStatus.Active.ToString()), loginToken, author, this);
        }
    }
}