using EPP.CorporatePortal.Common;
using EPP.CorporatePortal.DAL.Model;
using EPP.CorporatePortal.DAL.Service;
using EPP.CorporatePortal.Helper;
using EPP.CorporatePortal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static EPP.CorporatePortal.Common.Enums;

namespace EPP.CorporatePortal.Application
{
    public partial class ClaimNewSuccess : System.Web.UI.Page
    {
        private AuditTrail auditTrailService = new AuditTrail();
        private UserIdentityModel _UserIdentityModel = new UserIdentityModel();
        private ClaimSubmissionModel _ClaimSubmissionModel = new ClaimSubmissionModel();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session[ConfigurationManager.AppSettings["SessionVariableName"]] != null)
            {
                _UserIdentityModel = (UserIdentityModel)Session[ConfigurationManager.AppSettings["SessionVariableName"]];
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "UserIdentityModel loaded", "ClaimNewSuccess");
            }

            if (!Page.IsPostBack)
            {
                var userName = _UserIdentityModel.PrincipalName;

                try
                {
                    var newCorpId = Request.QueryString["CorpId"] ?? "0";

                    HiddenField hdnCorpId = (HiddenField)Page.Master.FindControl("hdnCorpId");
                    hdnCorpId.Value = Utility.EncodeAndDecryptCorpId(newCorpId);

                    var exitURL = ResolveUrl("~/Application/ClaimListing.aspx?&CorpId=" + newCorpId);
                    HiddenField hdnExitURL = (HiddenField)Page.Master.FindControl("hdnExitURL");
                    hdnExitURL.Value = exitURL;

                    var accessPermission = Rights_Enum.ManageMember;
                    HiddenField hdnPermission = (HiddenField)Page.Master.FindControl("hdnPermission");
                    hdnPermission.Value = Enum.GetName(typeof(Rights_Enum), accessPermission);

                    //Step Processing
                    CommonEntities.ClaimProcessSteps(1, Common.Enums.ClaimStepsStatus.complete.ToString(), false, "", this, userName);
                    CommonEntities.ClaimProcessSteps(2, Common.Enums.ClaimStepsStatus.complete.ToString(), false, "", this, userName);
                    CommonEntities.ClaimProcessSteps(3, Common.Enums.ClaimStepsStatus.complete.ToString(), false, "", this, userName);
                    CommonEntities.ClaimProcessSteps(4, Common.Enums.ClaimStepsStatus.complete.ToString(), false, "", this, userName);
                    CommonEntities.ClaimProcessSteps(5, Common.Enums.ClaimStepsStatus.complete.ToString(), false, "", this, userName);
                    CommonEntities.ClaimProcessSteps(6, Common.Enums.ClaimStepsStatus.complete.ToString(), false, "", this, userName);

                    if (Session["ClaimSubmissionModel"] != null)
                    {
                        _ClaimSubmissionModel = (ClaimSubmissionModel)Session["ClaimSubmissionModel"];
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "ClaimSubmissionModel loaded", "ClaimNewSuccess");
                    }

                    //Load claims                
                    LoadAllList(_UserIdentityModel.BusinessRegistrationNo, _ClaimSubmissionModel.MemberID, userName);
                }
                catch (Exception ex)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Page_Load: " + ex.Message, "ClaimNewSuccess");
                }
            }
        }
        private void LoadAllList(string CorpId, string memberId, string userName)
        {
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadAllList: Started", "ClaimNewSuccess");

            try
            {
                var service = new StoredProcService(userName);

                var claimObject = _ClaimSubmissionModel;

                //Claim Details
                if (string.IsNullOrEmpty(claimObject.MemberID))
                {
                    spanClaimName.InnerText = claimObject.MemberNameKeyIn;
                    spanClaimIDNo.InnerText = claimObject.MemberIDNoKeyIn;
                }
                else
                {
                    DataTable dtMember = service.GetClaimsMemberByMemberSourceID(claimObject.MemberID);
                    if (dtMember.Rows.Count > 0)
                    {
                        var member = dtMember.AsEnumerable().First();
                        spanClaimName.InnerText = member["MemberName"].ToString();
                        spanClaimIDNo.InnerText = member["ICNo"].ToString() ?? member["IDNo"].ToString();
                    }
                }

                hdnNotifyEmbedded.Value = claimObject.NonEmbeddedDeathFuneral.ToString();

                DataTable dt = new DataTable();

                if (string.IsNullOrEmpty(claimObject.MemberID)) //Show all headcounts for PIC
                    dt = service.GetBenefitsPolicyLevel(_UserIdentityModel.BusinessRegistrationNo);
                else //Show all headcounts for PIC as well as chosen member's
                    dt = service.GetBenefitsMemberLevel(claimObject.MemberID, _UserIdentityModel.BusinessRegistrationNo);

                DataView dtView = new DataView(dt);
                DataTable dtDistinct = dtView.ToTable(true, "BenefitCode", "BenefitDescription");

                //Only CI has different listing category
                var sessionBenefitCode = claimObject.BenefitCodeOri;
                var benefitCode = string.Empty;

                if (dtDistinct.Rows.Count > 0)
                    benefitCode = dtDistinct.AsEnumerable().Where(w => w["BenefitCode"].ToString() == sessionBenefitCode).Select(s => s["BenefitDescription"].ToString()).FirstOrDefault();

                DataTable dtCauseofEvent = new DataTable();
                if (claimObject.BenefitCode == "CI")
                    dtCauseofEvent = service.GetEBParameter("CICategory");
                else
                    dtCauseofEvent = service.GetEBParameter("CauseOfDeath");

                var causeCode = string.Empty;

                if (dtCauseofEvent.Rows.Count > 0)
                    causeCode = dtCauseofEvent.AsEnumerable().Where(w => w["Value"].ToString() == claimObject.CauseOfEvent).Select(s => s["Description"].ToString()).FirstOrDefault();

                spanClaimCause.InnerText = causeCode ?? claimObject.CauseOfEvent;
                spanClaimType.InnerText = benefitCode ?? claimObject.BenefitCode;
                spanClaimDateofEvent.InnerText = new DateTime(Convert.ToInt32(claimObject.YearOfEvent), Convert.ToInt32(claimObject.MonthOfEvent), Convert.ToInt32(claimObject.DayOfEvent)).ToString("dd MMMM yyyy");

                //Policy Details
                rptPolicyBankList.DataSource = claimObject.PolicyList;
                rptPolicyBankList.DataBind();
                ProcessPolicyBankRepeater(userName);

                //Document Details
                rptDocList.DataSource = claimObject.DocumentList;
                rptDocList.DataBind();
                ProcessDocRepeater(userName);

                //Submitter Details
                spanSubmitterName.InnerText = _UserIdentityModel.Name;
                spanSubmitterEmail.InnerText = _UserIdentityModel.Email;
                spanSubmitterContactNo.InnerText = _UserIdentityModel.MobileNumber;
                spanSubmitterROC.InnerText = _UserIdentityModel.BusinessRegistrationNo;
                spanSubmitterCompany.InnerText = _UserIdentityModel.ParentCorporate;
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in LoadAllList: " + ex.Message, "ClaimNewSuccess");
            }

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadAllList: Finished", "ClaimNewSuccess");
        }
        private void ProcessPolicyBankRepeater(string userName)
        {
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "ProcessPolicyBankRepeater: Started", "ClaimNewSuccess");

            try
            {
                var service = new StoredProcService(userName);

                for (int intI = 0; intI <= rptPolicyBankList.Items.Count - 1; intI++)
                {
                    HiddenField hdnPolicyIdCurr = (HiddenField)rptPolicyBankList.Items[intI].FindControl("hdnPolicySourceIdCurr");
                    HtmlGenericControl spanPolicyName = (HtmlGenericControl)rptPolicyBankList.Items[intI].FindControl("spanPolicyName");
                    HtmlGenericControl spanPolicyNo = (HtmlGenericControl)rptPolicyBankList.Items[intI].FindControl("spanPolicyNo");

                    if (hdnPolicyIdCurr != null)
                    {
                        var drPolicy = service.GetPolicyDetails(hdnPolicyIdCurr.Value, _UserIdentityModel.BusinessRegistrationNo).AsEnumerable().First();

                        spanPolicyName.InnerText = drPolicy["ProductName"].ToString();
                        spanPolicyNo.InnerText = drPolicy["ContractNo"].ToString();
                    }

                    HtmlGenericControl divBankROC = (HtmlGenericControl)rptPolicyBankList.Items[intI].FindControl("divBankROC");
                    HtmlGenericControl divBankROCData = (HtmlGenericControl)rptPolicyBankList.Items[intI].FindControl("divBankROCData");
                    if (divBankROCData != null && divBankROC != null && string.IsNullOrEmpty(divBankROCData.InnerText))
                    {
                        divBankROC.Style["display"] = "none";
                        divBankROCData.Style["display"] = "none";
                    }

                    HtmlGenericControl divIDType = (HtmlGenericControl)rptPolicyBankList.Items[intI].FindControl("divIDType");
                    HtmlGenericControl divIDTypeData = (HtmlGenericControl)rptPolicyBankList.Items[intI].FindControl("divIDTypeData");
                    if (divIDType != null && divIDTypeData != null && string.IsNullOrEmpty(divIDTypeData.InnerText))
                    {
                        divIDType.Style["display"] = "none";
                        divIDTypeData.Style["display"] = "none";
                    }

                    HtmlGenericControl divIDNo = (HtmlGenericControl)rptPolicyBankList.Items[intI].FindControl("divIDNo");
                    HtmlGenericControl divIDNoData = (HtmlGenericControl)rptPolicyBankList.Items[intI].FindControl("divIDNoData");
                    if (divIDNo != null && divIDNoData != null && string.IsNullOrEmpty(divIDNoData.InnerText))
                    {
                        divIDNo.Style["display"] = "none";
                        divIDNoData.Style["display"] = "none";
                    }

                    HtmlGenericControl divContactNo = (HtmlGenericControl)rptPolicyBankList.Items[intI].FindControl("divContactNo");
                    HtmlGenericControl divContactNoData = (HtmlGenericControl)rptPolicyBankList.Items[intI].FindControl("divContactNoData");
                    if (divContactNo != null && divContactNoData != null && string.IsNullOrEmpty(divContactNoData.InnerText))
                    {
                        divContactNo.Style["display"] = "none";
                        divContactNoData.Style["display"] = "none";
                    }

                    HtmlGenericControl divEmailAddress = (HtmlGenericControl)rptPolicyBankList.Items[intI].FindControl("divEmailAddress");
                    HtmlGenericControl divEmailAddressData = (HtmlGenericControl)rptPolicyBankList.Items[intI].FindControl("divEmailAddressData");
                    if (divEmailAddress != null && divEmailAddressData != null && string.IsNullOrEmpty(divEmailAddressData.InnerText))
                    {
                        divEmailAddress.Style["display"] = "none";
                        divEmailAddressData.Style["display"] = "none";
                    }
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in ProcessPolicyBankRepeater: " + ex.Message, "ClaimNewSuccess");
            }

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "ProcessPolicyBankRepeater: Finished", "ClaimNewSuccess");
        }
        private void ProcessDocRepeater(string userName)
        {
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "ProcessDocRepeater: Started", "ClaimNewSuccess");

            try
            {
                var service = new StoredProcService(userName);

                for (int intI = 0; intI <= rptDocList.Items.Count - 1; intI++)
                {
                    HiddenField hdnDocIdCurr = (HiddenField)rptDocList.Items[intI].FindControl("hdnDocIdCurr");
                    HtmlGenericControl spanDocumentName = (HtmlGenericControl)rptDocList.Items[intI].FindControl("spanDocumentName");

                    if (hdnDocIdCurr != null)
                    {
                        var dtDoc = service.GetRequiredDocByDocID(Convert.ToInt32(hdnDocIdCurr.Value));

                        if (dtDoc.Rows.Count > 0)
                        {

                            var drDoc = dtDoc.AsEnumerable().First();

                            spanDocumentName.InnerText = drDoc["DocumentName"].ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in ProcessDocRepeater: " + ex.Message, "ClaimNewSuccess");
            }

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "ProcessDocRepeater: Finished", "ClaimNewSuccess");
        }
        private string Last4DigitOnly(string value)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(value) && value.Length > 4)
            {
                // set asterisk to hide first n - 4 digits
                string asterisks = new string('*', value.Length - 4);

                // pick last 4 digits for showing
                string last = value.Substring(value.Length - 4, 4);

                // combine both asterisk mask and last digits
                result = asterisks + last;
            }
            return result;
        }
        protected void Continue(object sender, EventArgs e)
        {
            try
            {
                HiddenField hdnCorpId = (HiddenField)Page.Master.FindControl("hdnCorpId");
                var corpID = Utility.Encrypt(hdnCorpId.Value);

                Response.Redirect(ResolveUrl("~/Application/ClaimListing.aspx?&CorpId=" + corpID));
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in Continue: " + ex.Message, "ClaimNewSuccess");
            }
            finally
            {
                hdnContinue.Value = "0";
            }
        }
    }
}