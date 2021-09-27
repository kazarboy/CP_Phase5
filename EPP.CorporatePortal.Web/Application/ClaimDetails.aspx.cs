using EPP.CorporatePortal.Common;
using EPP.CorporatePortal.DAL.Service;
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

namespace EPP.CorporatePortal.Application
{
    public partial class ClaimDetails : System.Web.UI.Page
    {
        private AuditTrail auditTrailService = new AuditTrail();
        protected void Page_Load(object sender, EventArgs e)
        {

            string userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;
            string isowner = ((CorporatePortalSite)this.Master)._UserIdentityModel.IsOwner;
            try
            {
                var newCorpId = Request.QueryString["CorpId"] ?? "0";

                //temporary disable
                //if (newCorpId != "" && newCorpId != "0")
                //{
                //    hdnCorpId.Value = Utility.EncodeAndDecryptCorpId(newCorpId);
                //}
                //else
                //{
                //    var bizRegNo = new StoredProcService(userName).GetCorporateByUserName(userName);
                //    hdnCorpId.Value = Utility.EncodeAndDecryptCorpId(bizRegNo);
                //}

                var memberClaimsId = Request.QueryString["MemberClaimsId"] ?? "0";
                if (memberClaimsId != "" && memberClaimsId != "0")
                {
                    hdnMemberClaimsId.Value = Utility.EncodeAndDecryptCorpId(memberClaimsId);
                }

                var accessPermission = Rights_Enum.ManageClaim;
                HiddenField hdnPermission = (HiddenField)Page.Master.FindControl("hdnPermission");
                hdnPermission.Value = Enum.GetName(typeof(Rights_Enum), accessPermission);

                HiddenField hdnHomePage = (HiddenField)Page.Master.FindControl("hdnHomePage");
                hdnHomePage.Value = ConfigurationManager.AppSettings["RouteURL"] + Utility.GetHomePageByRole(userName);

                LoadAllList(userName);
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Page_Load: " + ex.Message, "ClaimDetails");
                throw;
            }
        }
        private void LoadAllList(string userName)
        {
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadAllList: Started", "ClaimDetails");

            try
            {
                var service = new StoredProcService(userName);

                var dtMemberClaims = service.GetClaimByMemberClaimsID(Convert.ToInt32(hdnMemberClaimsId.Value));

                if (dtMemberClaims.Rows.Count > 0)
                {
                    //To change/map out CGLS Status to Portal Status
                    dtMemberClaims.Rows[0]["ClaimStatus"] = CommonEntities.ClaimProcessStatus(dtMemberClaims.Rows[0]["ClaimStatus"].ToString(), dtMemberClaims.Rows[0]["CGLSRemark"].ToString(), userName);                   

                    spanInsuredName.InnerText = dtMemberClaims.Rows[0]["MemberName"].ToString();
                    spanInsuredIDNo.InnerText = string.IsNullOrEmpty(dtMemberClaims.Rows[0]["ICNo"].ToString()) ? dtMemberClaims.Rows[0]["IDNo"].ToString() : dtMemberClaims.Rows[0]["ICNo"].ToString();

                    var dtBenefit = service.GetBenefitByBenefitCode(dtMemberClaims.Rows[0]["BenefitCode"].ToString());

                    if (dtBenefit.Rows.Count > 0)
                        spanInsuredClaimType.InnerText = dtBenefit.Rows[0]["BenefitDescription"].ToString();

                    //Change labeling accordingly
                    ClaimTypeLabelLogic(dtMemberClaims.Rows[0]["BenefitCode"].ToString());

                    DataTable dtCauseofEvent = new DataTable();
                    if (dtMemberClaims.Rows[0]["BenefitCode"].ToString() == "CI")
                        dtCauseofEvent = service.GetEBParameter("CICategory");
                    else
                        dtCauseofEvent = service.GetEBParameter("CauseOfDeath");

                    var causeCode = string.Empty;

                    if (dtCauseofEvent.Rows.Count > 0)
                        causeCode = dtCauseofEvent.AsEnumerable().Where(w => w["Value"].ToString() == dtMemberClaims.Rows[0]["EventCauseOri"].ToString()).Select(s => s["Description"].ToString()).FirstOrDefault();

                    spanInsuredCauseOfEvent.InnerText = causeCode ?? dtMemberClaims.Rows[0]["EventCause"].ToString();
                    spanInsuredDateOfEvent.InnerText = Convert.ToDateTime(dtMemberClaims.Rows[0]["EventDate"]).ToString("dd MMMM yyyy");


                    spanClaimSubmissionDate.InnerText = Convert.ToDateTime(dtMemberClaims.Rows[0]["SubmissionDate"]).ToString("dd MMMM yyyy");
                    spanClaimPortalClaimNo.InnerText = string.IsNullOrEmpty(dtMemberClaims.Rows[0]["PortalClaimNo"].ToString()) ? "-" : dtMemberClaims.Rows[0]["PortalClaimNo"].ToString();
                    spanClaimCGLSClaimNo.InnerText = string.IsNullOrEmpty(dtMemberClaims.Rows[0]["CGLSClaimNo"].ToString()) ? "-" : dtMemberClaims.Rows[0]["CGLSClaimNo"].ToString();
                    spanClaimStatus.InnerText = string.IsNullOrEmpty(dtMemberClaims.Rows[0]["ClaimStatus"].ToString()) ? "-" : dtMemberClaims.Rows[0]["ClaimStatus"].ToString();

                    if (dtMemberClaims.Rows[0]["ClaimStatus"].ToString() == Common.Constants.Application.PortalStatus.Approved)
                    {
                        divClaimPaidDate.InnerText = "Approved Date";
                        spanClaimPaidDate.InnerText = !string.IsNullOrEmpty(dtMemberClaims.Rows[0]["ClaimStatusDate"].ToString()) ? Convert.ToDateTime(dtMemberClaims.Rows[0]["ClaimStatusDate"]).ToString("dd MMMM yyyy") : "-";
                        divClaimPaidAmount.InnerText = "Approved Amount";
                        spanClaimPaidAmount.InnerText = string.IsNullOrEmpty(dtMemberClaims.Rows[0]["PaymentAmount"].ToString()) ? "-" : String.Format("{0:n}", Convert.ToDecimal(dtMemberClaims.Rows[0]["PaymentAmount"]));
                        divClaimRejectReason.Style["display"] = "none";
                        divClaimRejectReason2.Style["display"] = "none";
                    }
                    else if (dtMemberClaims.Rows[0]["ClaimStatus"].ToString() == Common.Constants.Application.PortalStatus.Rejected)
                    {
                        divClaimPaidDate.InnerText = "Rejected Date";
                        spanClaimPaidDate.InnerText = !string.IsNullOrEmpty(dtMemberClaims.Rows[0]["ClaimStatusDate"].ToString()) ? Convert.ToDateTime(dtMemberClaims.Rows[0]["ClaimStatusDate"]).ToString("dd MMMM yyyy") : "-";
                        divClaimPaidAmount.Style["display"] = "none";
                        divClaimPaidAmount2.Style["display"] = "none";
                        divClaimRejectReason.Style.Remove("display");
                        divClaimRejectReason.InnerText = "Rejected Reasons";
                        divClaimRejectReason2.Style.Remove("display");
                        spanClaimRejectReason.InnerText = string.IsNullOrEmpty(dtMemberClaims.Rows[0]["CGLSRemark"].ToString()) ? "-" : dtMemberClaims.Rows[0]["CGLSRemark"].ToString();
                    }
                    else if (dtMemberClaims.Rows[0]["ClaimStatus"].ToString() == Common.Constants.Application.PortalStatus.PendingRequirement)
                    {
                        divClaimPaidDate.InnerText = "Status Date";
                        spanClaimPaidDate.InnerText = !string.IsNullOrEmpty(dtMemberClaims.Rows[0]["ClaimStatusDate"].ToString()) ? Convert.ToDateTime(dtMemberClaims.Rows[0]["ClaimStatusDate"]).ToString("dd MMMM yyyy") : "-";
                        divClaimPaidAmount.Style["display"] = "none";
                        divClaimPaidAmount2.Style["display"] = "none";
                        divClaimRejectReason.Style.Remove("display");
                        divClaimRejectReason.InnerText = "Pending Reasons";
                        divClaimRejectReason2.Style.Remove("display");
                        spanClaimRejectReason.InnerText = string.IsNullOrEmpty(dtMemberClaims.Rows[0]["CGLSRemark"].ToString()) ? "-" : dtMemberClaims.Rows[0]["CGLSRemark"].ToString();
                    }
                    else if (dtMemberClaims.Rows[0]["ClaimStatus"].ToString() == Common.Constants.Application.PortalStatus.Paid)
                    {
                        divClaimPaidDate.InnerText = "Paid Date";
                        spanClaimPaidDate.InnerText = !string.IsNullOrEmpty(dtMemberClaims.Rows[0]["PaymentDate"].ToString()) ? Convert.ToDateTime(dtMemberClaims.Rows[0]["PaymentDate"]).ToString("dd MMMM yyyy") : "-";
                        divClaimPaidAmount.InnerText = "Paid Amount";
                        spanClaimPaidAmount.InnerText = string.IsNullOrEmpty(dtMemberClaims.Rows[0]["PaymentAmount"].ToString()) ? "-" : String.Format("{0:n}", Convert.ToDecimal(dtMemberClaims.Rows[0]["PaymentAmount"]));
                        divClaimRejectReason.Style["display"] = "none";
                        divClaimRejectReason2.Style["display"] = "none";
                    }
                    else
                    {
                        divClaimPaidDate.InnerText = "Status Date";
                        spanClaimPaidDate.InnerText = !string.IsNullOrEmpty(dtMemberClaims.Rows[0]["ClaimStatusDate"].ToString()) ? Convert.ToDateTime(dtMemberClaims.Rows[0]["ClaimStatusDate"]).ToString("dd MMMM yyyy") : "-";
                        divClaimPaidAmount.InnerText = "Payment Amount";
                        spanClaimPaidAmount.InnerText = string.IsNullOrEmpty(dtMemberClaims.Rows[0]["PaymentAmount"].ToString()) ? "-" : String.Format("{0:n}", Convert.ToDecimal(dtMemberClaims.Rows[0]["PaymentAmount"]));
                        divClaimRejectReason.Style["display"] = "none";
                        divClaimRejectReason2.Style["display"] = "none";
                    }

                    spanClaimLetter.InnerText = "-";


                    var dtPolicy = service.GetPolicyDetails(dtMemberClaims.Rows[0]["PolicyId"].ToString(), hdnCorpId.Value);
                    if (dtPolicy.Rows.Count > 0)
                    {
                        spanPolicyName.InnerText = dtPolicy.Rows[0]["ProductName"].ToString();
                        spanPolicyNo.InnerText = dtPolicy.Rows[0]["ContractNo"].ToString();
                    }


                    var dtUser = service.GetUserByUserId(Convert.ToInt32(dtMemberClaims.Rows[0]["SubmitterId"]));
                    if (dtUser.Rows.Count > 0)
                    {
                        spanSubmitterName.InnerText = string.IsNullOrEmpty(dtUser.Rows[0]["FullName"].ToString()) ? "-" : dtUser.Rows[0]["FullName"].ToString();
                        spanSubmitterContactNo.InnerText = string.IsNullOrEmpty(Utility.EncodeAndDecryptCorpId(dtUser.Rows[0]["MobilePhone"].ToString())) ? "-" : Utility.EncodeAndDecryptCorpId(dtUser.Rows[0]["MobilePhone"].ToString());
                        spanSubmitterEmailAddr.InnerText = string.IsNullOrEmpty(dtUser.Rows[0]["EmailAddress"].ToString()) ? "-" : dtUser.Rows[0]["EmailAddress"].ToString();
                        spanSubmitterCompanyName.InnerText = string.IsNullOrEmpty(dtUser.Rows[0]["CoName"].ToString()) ? "-" : dtUser.Rows[0]["CoName"].ToString();
                        spanSubmitterBizRegNo.InnerText = string.IsNullOrEmpty(dtUser.Rows[0]["CoSourceId"].ToString()) ? "-" : dtUser.Rows[0]["CoSourceId"].ToString();
                    }

                    var dtDocument = service.GetClaimsMemberDocumentByMemberClaimsId(Convert.ToInt32(dtMemberClaims.Rows[0]["Id"]));
                    if (dtDocument.Rows.Count > 0)
                    {
                        rptDocList.DataSource = dtDocument;
                        rptDocList.DataBind();

                        ProcessDocRepeater(userName);
                    }
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in LoadAllList: " + ex.Message, "ClaimDetails");
            }

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadAllList: Finished", "ClaimDetails");
        }
        private void ClaimTypeLabelLogic(string benefitCode)
        {
            if (benefitCode == "DA" || benefitCode == "DN" || benefitCode == "DTH" || benefitCode == "FEM")
            {
                divCauseOfEventLabel.InnerText = "Cause of death";
                divDateOfEventLabel.InnerText = "Date of death";
            }
            else
            {
                divCauseOfEventLabel.InnerText = "Cause of event";
                divDateOfEventLabel.InnerText = "Date of event";
            }
        }
        private void ProcessDocRepeater(string userName)
        {
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "ProcessDocRepeater: Started", "ClaimDetails");

            try
            {
                var service = new StoredProcService(userName);

                for (int intI = 0; intI <= rptDocList.Items.Count - 1; intI++)
                {
                    HiddenField hdnDocIdCurr = (HiddenField)rptDocList.Items[intI].FindControl("hdnDocIDCurr");
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
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in ProcessDocRepeater: " + ex.Message, "ClaimDetails");
            }

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "ProcessDocRepeater: Finished", "ClaimDetails");
        }
        protected void Back(object sender, EventArgs e)
        {
            var userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;

            try
            {              

                Response.Redirect(ResolveUrl("~/Application/ClaimListing.aspx?&CorpId=" + hdnCorpId.Value));
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Back: " + ex.Message, "ClaimDetails");
            }
        }
    }
}