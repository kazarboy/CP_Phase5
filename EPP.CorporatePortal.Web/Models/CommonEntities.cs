using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using EPP.CorporatePortal.DAL.Entity;
using EPP.CorporatePortal.DAL.Model;
//using System.Security.Claims;
using EPP.CorporatePortal.DAL.Service;
using Newtonsoft.Json;

namespace EPP.CorporatePortal.Models
{
    public static class CommonEntities
    {
        public static DataTable LoadPolicies(string corporateId, string userName, string isOwnerClaim, string UcorpId = null)
        {
            var decryptedCorpId = Utility.EncodeAndDecryptCorpId(corporateId);
            var decryptedUCorpId = Utility.EncodeAndDecryptCorpId(UcorpId);
            //var userName = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).FirstOrDefault();
            //var isOwnerClaim = identity.Claims.Where(c => c.Type == "IsOwner").Select(c => c.Value).FirstOrDefault();
            var isOwner = Convert.ToBoolean(isOwnerClaim);
            var policies = new DataTable();
            var service = new StoredProcService(userName);
            if (isOwner)//Get all policies for the corporate
            {
                policies = service.GetPolicyByCoporateId(decryptedCorpId, decryptedUCorpId);
            }
            else //get only owned policies
            {
                //Need to add  UcorpId******
                policies = service.GetPolicyByOwnership(decryptedCorpId, userName);
            }
            //Encrypt the PolicyID  

            foreach (DataRow dr in policies.Rows)
            {
                string policyId = dr["SourceId"].ToString().Trim();
                dr["SourceId"] = Utility.Encrypt(policyId);
            }

            new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Retreiving Policies for Corporate:" + decryptedCorpId + " UID:"+ decryptedUCorpId, "DBEntities");
            return policies;
        }
        public static void DeleteUser(string username, string loginToken, string author, Page currPage)
        {
            var appCode = CommonService.GetSystemConfigValue("AppCode");
            var businessEntityID = CommonService.GetSystemConfigValue("BusinessEntityID");

            var service = new StoredProcService(author);
            try
            {
                //Get existing user details to compare value
                var userDetails = service.GetUserDetails(username);
                if (userDetails.Rows.Count > 0)
                {
                    var emailAddress = userDetails.Rows[0]["EmailAddress"].ToString();

                    //Get user details
                    new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "Processing username deletion [" + username + "]", "DeleteUser_Click");
                    var userReq = new DeleteUserTokenRequest() { EmailAddress = emailAddress, Username = username };
                    var body = JsonConvert.SerializeObject(userReq, Formatting.Indented);

                    string token = CommonService.EncryptString(body);
                    var deleteTokenResponse = new LoginService().DeleteUser(token, author, loginToken);
                    if (deleteTokenResponse.Valid)
                    {
                        service.UpdateUserIsDelete(username, true);
                        service.UpdateUserIsActive(username, false);
                        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, author, "Delete User successfully to Agent Portal Hub", "DeleteUser_Click");
                        Utility.RegisterStartupScriptHandling(currPage, "Success", "alert('User deleted successfully');", true, true, author);
                    }
                    else
                    {
                        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, author, "Delete User not valid: " + deleteTokenResponse.ResponseStatusEntity.StatusDescription, "DeleteUser_Click");
                        Utility.RegisterStartupScriptHandling(currPage, "Success", "alert('Error deleting user');", true, true, author);
                    }
                }
            }
            catch (Exception ex)
            {
                //In case got any uncaptured errors.
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, author, "Error in DeleteUser: " + ex.Message, "DeleteUser_Click");
                Utility.RegisterStartupScriptHandling(currPage, "Error", "alert('Error deleting user');", true, true, author);
            }
        }
        public static void StatusChangeUser(string username, int status, string loginToken, string author, Page currPage)
        {
            //Default alertMsg to true
            StatusChangeUser(username, status, loginToken, author, currPage, true);
        }
        public static void StatusChangeUser(string username, int status, string loginToken, string author, Page currPage, bool alertMsg)
        {
            var appCode = CommonService.GetSystemConfigValue("AppCode");
            var businessEntityID = CommonService.GetSystemConfigValue("BusinessEntityID");

            var service = new StoredProcService(author);
            try
            {
                //Get existing user details to compare value
                var userDetails = service.GetUserDetails(username);
                if (userDetails.Rows.Count > 0)
                {
                    //Get user details
                    var emailAddress = userDetails.Rows[0]["EmailAddress"];

                    new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "Processing username status change [" + username + "]", "ReactivateUser_Click");
                    //Status = 0 - Active, 2 - Dormant, 10 - Disable 
                    var userReq = new StatusChangeUserTokenRequest() { EmailAddress = emailAddress.ToString(), Username = username, AppCode = appCode, UpdatedBy = author, Status = status.ToString() };
                    var body = JsonConvert.SerializeObject(userReq, Formatting.Indented);

                    string token = CommonService.EncryptString(body);
                    var statusChangeTokenResponse = new LoginService().StatusChangeUser(token, author, loginToken.ToString());
                    if (statusChangeTokenResponse.Valid)
                    {
                        var statusActivity = "";

                        if (status == 0)
                        {
                            service.UpdateUserIsActive(username, true);
                            statusActivity = "reactivated";
                        }
                        else
                        {
                            service.UpdateUserIsActive(username, false);
                            statusActivity = "suspended";
                        }
                        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, author, "Reactivate User successfully to Agent Portal Hub", "ReactivateUser_Click");

                        if (alertMsg)
                            Utility.RegisterStartupScriptHandling(currPage, "Success", "alert('User " + statusActivity + " successfully');", true, true, author);
                    }
                    else
                    {
                        var statusActivity = "";

                        if (status == 0)
                            statusActivity = "reactivating";
                        else
                            statusActivity = "suspending";

                        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, author, "Reactivate User not valid: " + statusChangeTokenResponse.ResponseStatusEntity.StatusDescription, "ReactivateUser_Click");
                        Utility.RegisterStartupScriptHandling(currPage, "Success", "alert('Error " + statusActivity + " user');", true, true, author);
                    }
                }
            }
            catch (Exception ex)
            {
                var statusActivity = "";

                if (status == 0)
                    statusActivity = "reactivating";
                else
                    statusActivity = "suspending";

                //In case got any uncaptured errors.
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, author, "Error in DeleteUser: " + ex.Message, "DeleteUser_Click");
                Utility.RegisterStartupScriptHandling(currPage, "Error", "alert('Error " + statusActivity + " user');", true, true, author);
            }
        }
        public static ClaimSubmissionModel ReInitClaimsMemberSession(string userName)
        {
            var _ClaimSubmissionModel = new ClaimSubmissionModel();

            new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Reinitializing Claims Session Variables", "ReInitClaimsMemberSession");
            try
            {
                if (System.Web.HttpContext.Current.Session["ClaimSubmissionModel"] != null)
                {
                    _ClaimSubmissionModel = (ClaimSubmissionModel)System.Web.HttpContext.Current.Session["ClaimSubmissionModel"];

                    _ClaimSubmissionModel.MemberID = string.Empty;
                    _ClaimSubmissionModel.MemberIDNoKeyIn = string.Empty;
                    _ClaimSubmissionModel.MemberIDTypeKeyIn = string.Empty;
                    _ClaimSubmissionModel.MemberNameKeyIn = string.Empty;
                    _ClaimSubmissionModel.MemberTerminationDate = new DateTime();

                    System.Web.HttpContext.Current.Session["ClaimSubmissionModel"] = _ClaimSubmissionModel;
                }
            }
            catch (Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in ReInitClaimsMemberSession: " + ex.Message, "ReInitClaimsMemberSession");
            }

            return _ClaimSubmissionModel;
        }
        public static ClaimSubmissionModel ReInitClaimsClaimSession(string userName)
        {
            var _ClaimSubmissionModel = new ClaimSubmissionModel();

            new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Reinitializing Claims Session Variables", "ReinititializeClaimsClaimSession");
            try
            {
                if (System.Web.HttpContext.Current.Session["ClaimSubmissionModel"] != null)
                {
                    _ClaimSubmissionModel = (ClaimSubmissionModel)System.Web.HttpContext.Current.Session["ClaimSubmissionModel"];

                    _ClaimSubmissionModel.BenefitCode = string.Empty;
                    _ClaimSubmissionModel.BenefitCodeOri = string.Empty;
                    _ClaimSubmissionModel.CauseOfEvent = string.Empty;
                    _ClaimSubmissionModel.DayOfEvent = string.Empty;
                    _ClaimSubmissionModel.YearOfEvent = string.Empty;
                    _ClaimSubmissionModel.MonthOfEvent = string.Empty;

                    System.Web.HttpContext.Current.Session["ClaimSubmissionModel"] = _ClaimSubmissionModel;
                }
            }
            catch (Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in ReinititializeClaimsClaimSession: " + ex.Message, "ReinititializeClaimsClaimSession");
            }

            return _ClaimSubmissionModel;
        }
        public static ClaimSubmissionModel ReInitClaimsPolicySession(string userName)
        {
            var _ClaimSubmissionModel = new ClaimSubmissionModel();

            new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Reinitializing Claims Session Variables", "ReInitClaimsPolicySession");
            try
            {
                if (System.Web.HttpContext.Current.Session["ClaimSubmissionModel"] != null)
                {
                    _ClaimSubmissionModel = (ClaimSubmissionModel)System.Web.HttpContext.Current.Session["ClaimSubmissionModel"];

                    _ClaimSubmissionModel.PolicyList = new List<ClaimSubmissionPolicy>();

                    System.Web.HttpContext.Current.Session["ClaimSubmissionModel"] = _ClaimSubmissionModel;
                }
            }
            catch (Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in ReInitClaimsPolicySession: " + ex.Message, "ReInitClaimsPolicySession");
            }

            return _ClaimSubmissionModel;
        }
        public static ClaimSubmissionModel ReInitClaimsBankSession(string userName)
        {
            var _ClaimSubmissionModel = new ClaimSubmissionModel();

            new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Reinitializing Claims Session Variables", "ReInitClaimsBankSession");
            try
            {
                if (System.Web.HttpContext.Current.Session["ClaimSubmissionModel"] != null)
                {
                    _ClaimSubmissionModel = (ClaimSubmissionModel)System.Web.HttpContext.Current.Session["ClaimSubmissionModel"];

                    List<ClaimSubmissionPolicy> listPolicy = _ClaimSubmissionModel.PolicyList;

                    for (int intI = 0; intI <= listPolicy.Count - 1; intI++)
                    {
                        var policy = listPolicy[intI];

                        policy.AccountHolderName = string.Empty;
                        policy.BankName = string.Empty;
                        policy.BankAccountNo = string.Empty;
                        policy.BankROC = string.Empty;
                    }

                    _ClaimSubmissionModel.PolicyList = listPolicy;

                    System.Web.HttpContext.Current.Session["ClaimSubmissionModel"] = _ClaimSubmissionModel;
                }
            }
            catch (Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in ReInitClaimsBankSession: " + ex.Message, "ReInitClaimsBankSession");
            }

            return _ClaimSubmissionModel;
        }
        public static ClaimSubmissionModel ReInitClaimsDocumentSession(string userName)
        {
            var _ClaimSubmissionModel = new ClaimSubmissionModel();

            new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Reinitializing Claims Session Variables", "ReInitClaimsDocumentSession");
            try
            {
                if (System.Web.HttpContext.Current.Session["ClaimSubmissionModel"] != null)
                {
                    _ClaimSubmissionModel = (ClaimSubmissionModel)System.Web.HttpContext.Current.Session["ClaimSubmissionModel"];

                    _ClaimSubmissionModel.DocumentList = new List<ClaimSubmissionDocument>();

                    System.Web.HttpContext.Current.Session["ClaimSubmissionModel"] = _ClaimSubmissionModel;
                }
            }
            catch (Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in ReInitClaimsDocumentSession: " + ex.Message, "ReInitClaimsDocumentSession");
            }

            return _ClaimSubmissionModel;
        }
        public static void ClaimProcessSteps(int step, string status, bool activateURL, string url, Page currPage, string author)
        {
            try
            {
                HiddenField hdnStep = (HiddenField)currPage.Master.FindControl("hdnStep" + step.ToString());
                hdnStep.Value = url;

                HtmlGenericControl divStep = (HtmlGenericControl)currPage.Master.FindControl("divStep" + step.ToString());
                divStep.Attributes.Add("class", "col-xs-3 bs-wizard-step " + status);

                if (activateURL)
                {
                    HtmlAnchor aStep = (HtmlAnchor)currPage.Master.FindControl("aStep" + step.ToString());
                    aStep.Attributes.Add("onClick", "return confirm('Are you sure you want to go back to previous step?');");
                }
                else
                {
                    HtmlAnchor aStep = (HtmlAnchor)currPage.Master.FindControl("aStep" + step.ToString());
                    aStep.Attributes.Add("onClick", "return false;");
                }
            }
            catch (Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, author, "Error in ClaimProcessSteps: " + ex.Message, "ClaimProcessSteps");
            }
        }
        public static string ClaimProcessStatus(string cglsStatus, string cglsRemarks, string author)
        {
            string ret = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(cglsStatus))
                    ret = Common.Constants.Application.PortalStatus.NewSubmission;
                else if (cglsStatus == Common.Constants.Application.CGLSStatus.InProgress && string.IsNullOrEmpty(cglsRemarks))
                    ret = Common.Constants.Application.PortalStatus.InProgress;
                else if (cglsStatus == Common.Constants.Application.CGLSStatus.InProgress && !string.IsNullOrEmpty(cglsRemarks))
                    ret = Common.Constants.Application.PortalStatus.PendingRequirement;
                else if (cglsStatus == Common.Constants.Application.CGLSStatus.PendingApproval)
                    ret = Common.Constants.Application.PortalStatus.PendingApproval;
                else if (cglsStatus == Common.Constants.Application.CGLSStatus.Approved || cglsStatus == Common.Constants.Application.CGLSStatus.FullIntimationSentToFin || cglsStatus == Common.Constants.Application.CGLSStatus.IntimationSentToFin)
                    ret = Common.Constants.Application.PortalStatus.Approved;
                else if (cglsStatus == Common.Constants.Application.CGLSStatus.FullyPaid || cglsStatus == Common.Constants.Application.CGLSStatus.Paid || cglsStatus == Common.Constants.Application.CGLSStatus.PaidHoldPayment || cglsStatus == Common.Constants.Application.CGLSStatus.PaidNoPayment || cglsStatus == Common.Constants.Application.CGLSStatus.PaidStopPayment)
                    ret = Common.Constants.Application.PortalStatus.Paid;
                else if (cglsStatus == Common.Constants.Application.CGLSStatus.Rejected)
                    ret = Common.Constants.Application.PortalStatus.Rejected;
                else if (cglsStatus == Common.Constants.Application.CGLSStatus.Closed)
                    ret = Common.Constants.Application.PortalStatus.Closed;
                else
                    ret = cglsStatus;
            }
            catch (Exception ex)
            {
                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, author, "Error in ClaimProcessSteps: " + ex.Message, "ClaimProcessStatus");

                //Default to same status if error
                ret = cglsStatus;
            }

            return ret;
        }
    }
}