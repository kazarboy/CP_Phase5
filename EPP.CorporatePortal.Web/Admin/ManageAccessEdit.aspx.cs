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
    public partial class ManageAccessEdit : System.Web.UI.Page
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
                
                txtUName.Text = userName;
                lblUsername.Text = userName;
                txtFullName.Text = fullName;
                txtICNo.Text = icNo;
                txtPhoneNo.Text = mobilePhone;
                txtEmailAddress.Text = emailAddress;

                genderF.Checked = gender == "F" ? true : false;
                genderM.Checked = gender == "M" ? true : false;
            }

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadUserList: Finished", "ManageAccessEdit");
        }
        protected void ModifyUser(object sender, EventArgs e)
        {
            var author = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;
            var appCode = CommonService.GetSystemConfigValue("AppCode");
            var businessEntityID = CommonService.GetSystemConfigValue("BusinessEntityID");
            var loginPageUrl = CommonService.GetSystemConfigValue("LoginPageUrl");

            var loginToken = Session[appCode + "Token"];

            var service = new StoredProcService(author);

            //load menu
            var username = hdnUsername.Value;
            DataTable dt = service.GetUserDetails(username);
            if (dt.Rows.Count > 0)
            {
                //Get User Details                
                var userName = dt.Rows[0]["UserName"].ToString();
                var emailAddress = dt.Rows[0]["EmailAddress"].ToString();
                var fullName = dt.Rows[0]["FullName"].ToString();
                var gender = dt.Rows[0]["Gender"].ToString();
                var icNo = Utility.EncodeAndDecryptCorpId(dt.Rows[0]["ICNo"].ToString());
                var mobilePhone = Utility.EncodeAndDecryptCorpId(dt.Rows[0]["MobilePhone"].ToString());

                try
                {
                    new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "Processing username [" + userName + "]", "ModifyUser_Click");
                    //Update User Details
                    var encPhoneNo = Utility.Encrypt(txtPhoneNo.Text);
                    new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "Processing username [" + userName + "]: Old MobilePhone: " + dt.Rows[0]["MobilePhone"].ToString() + ", New MobilePhone: " + encPhoneNo + ", Old EmailAddress: " + emailAddress + ", New EmailAddress: " + txtEmailAddress.Text, "ModifyUser_Click");
                    service.UpdateUserDetails(userName, txtEmailAddress.Text, encPhoneNo);
                    new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "Processing username [" + userName + "]: updated user details", "ModifyUser_Click");

                    var userReq = new ModifyUserTokenRequest() { Username = userName, OldEmailAddress = emailAddress, OldMobilePhone = mobilePhone, NewEmailAddress = txtEmailAddress.Text, NewMobilePhone = txtPhoneNo.Text };
                    var body = JsonConvert.SerializeObject(userReq, Formatting.Indented);

                    string token = CommonService.EncryptString(body);
                    var modifyTokenResponse = new LoginService().ModifyUser(token, author, loginToken.ToString());
                    if (modifyTokenResponse.Valid)
                    {
                        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, author, "Modify User successfully to Agent Portal Hub", "ModifyUser_Click");
                    }
                    else
                    {
                        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, author, "Modify User not valid: " + modifyTokenResponse.ResponseStatusEntity.StatusDescription, "ModifyUser_Click");

                        //TODO: undo user changes
                    }

                    Utility.RegisterStartupScriptHandling(this, "Success", "alert('User modified successfully');", true, true, author);
                }
                catch (Exception ex)
                {
                    //In case got any uncaptured errors.
                    //UploadLogging("Error: Error in modifying user", "Error in modify user function: " + ex.Message, author, true);                    
                }
            }
        }
    }
}