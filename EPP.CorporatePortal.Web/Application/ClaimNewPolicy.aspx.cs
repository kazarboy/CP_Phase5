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
    public partial class ClaimNewPolicy : System.Web.UI.Page
    {
        private AuditTrail auditTrailService = new AuditTrail();
        private UserIdentityModel _UserIdentityModel = new UserIdentityModel();
        private ClaimSubmissionModel _ClaimSubmissionModel = new ClaimSubmissionModel();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session[ConfigurationManager.AppSettings["SessionVariableName"]] != null)
            {
                _UserIdentityModel = (UserIdentityModel)Session[ConfigurationManager.AppSettings["SessionVariableName"]];
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "UserIdentityModel loaded", "ClaimNewPolicy");
            }

            var userName = _UserIdentityModel.PrincipalName;

            var newCorpId = Request.QueryString["CorpId"] ?? "0";
            var newUCorpId = Request.QueryString["UCorpId"] ?? "0";

            var returnIndicator = Request.QueryString["Return"] ?? "0";

            try
            {
                HiddenField hdnCorpId = (HiddenField)Page.Master.FindControl("hdnCorpId");
                HiddenField hdnUCorpId = (HiddenField)Page.Master.FindControl("hdnUCorpId");
                hdnCorpId.Value = Utility.EncodeAndDecryptCorpId(newCorpId);
                hdnUCorpId.Value = Utility.EncodeAndDecryptCorpId(newUCorpId);

                var exitURL = ResolveUrl("~/Application/ClaimListing.aspx?&CorpId=" + newCorpId + "&UCorpId=" + newUCorpId);
                HiddenField hdnExitURL = (HiddenField)Page.Master.FindControl("hdnExitURL");
                hdnExitURL.Value = exitURL;

                var accessPermission = Rights_Enum.ManageMember;
                HiddenField hdnPermission = (HiddenField)Page.Master.FindControl("hdnPermission");
                hdnPermission.Value = Enum.GetName(typeof(Rights_Enum), accessPermission);

                //Step Processing
                CommonEntities.ClaimProcessSteps(1, Common.Enums.ClaimStepsStatus.complete.ToString(), false, "", this, userName);
                CommonEntities.ClaimProcessSteps(2, Common.Enums.ClaimStepsStatus.complete.ToString(), true, ResolveUrl("~/Application/ClaimNewClaim.aspx?&CorpId=" + newCorpId + "&UCorpId=" + newUCorpId + "&Return=1"), this, userName);
                CommonEntities.ClaimProcessSteps(3, Common.Enums.ClaimStepsStatus.active.ToString(), false, "", this, userName);
                CommonEntities.ClaimProcessSteps(4, Common.Enums.ClaimStepsStatus.disabled.ToString(), false, "", this, userName);
                CommonEntities.ClaimProcessSteps(5, Common.Enums.ClaimStepsStatus.disabled.ToString(), false, "", this, userName);
                CommonEntities.ClaimProcessSteps(6, Common.Enums.ClaimStepsStatus.disabled.ToString(), false, "", this, userName);

                //HiddenField hdnStep2 = (HiddenField)Page.Master.FindControl("hdnStep2");
                //hdnStep2.Value = ResolveUrl("~/Application/ClaimNewClaim.aspx?&CorpId=" + newCorpId + "&Return=1");
                //HtmlAnchor aStep2 = (HtmlAnchor)Page.Master.FindControl("aStep2");
                //aStep2.Attributes.Add("onClientClick", "return confirm('Are you sure you want to go back to previous step?');");

                if (Session["ClaimSubmissionModel"] != null)
                {
                    _ClaimSubmissionModel = (ClaimSubmissionModel)Session["ClaimSubmissionModel"];
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "ClaimSubmissionModel loaded", "ClaimNewPolicy");
                }

                //Load claims
                if (!Page.IsPostBack)
                {
                    //Load claims
                    LoadClaimPolicyList(hdnCorpId.Value, _ClaimSubmissionModel.MemberID, userName);

                    //If return from another step, to reinitialize session values
                    if (returnIndicator != "0")
                        ReinitializeSessionValue();
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Page_Load: " + ex.Message, "ClaimNewPolicy");
            }
        }
        private void ReinitializeSessionValue()
        {
            var userName = _UserIdentityModel.PrincipalName;

            _ClaimSubmissionModel = CommonEntities.ReInitClaimsPolicySession(userName);
            _ClaimSubmissionModel = CommonEntities.ReInitClaimsBankSession(userName);
            //_ClaimSubmissionModel = CommonEntities.ReInitClaimsDocumentSession(userName);
        }
        private void LoadClaimPolicyList(string CorpId, string memberId, string userName)
        {
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadClaimPolicyList MemberId: " + memberId, "ClaimNewPolicy");

            try
            {
                var service = new StoredProcService(userName);

                //load menu
                if (string.IsNullOrEmpty(memberId)) //Show all headcounts for PIC
                {
                    DataTable dt = service.GetBenefitsPolicyLevel(CorpId);

                    DataView dtView = new DataView(dt);
                    DataTable dtDistinct = dtView.ToTable(true, "PolicyId", "Entity", "ProductName", "ContractNo", "DisplayCode", "PeriodOfCoverStart", "PeriodOfCoverEnd");

                    if (dtDistinct.Rows.Count > 0)
                        dt = dtDistinct;

                    //Filter to only those with the BenefitCode selected by user
                    var dtFiltered = dt.AsEnumerable().Where(w => w["DisplayCode"].ToString() == _ClaimSubmissionModel.BenefitCode);

                    if (dtFiltered.Any())
                        dt = dtFiltered.CopyToDataTable();
                    else
                        dt = new DataTable();

                    //Distinct again
                    DataView dtView2 = new DataView(dt);
                    DataTable dtDistinct2 = dtView2.ToTable(true, "PolicyId", "Entity", "ProductName", "ContractNo", "PeriodOfCoverStart", "PeriodOfCoverEnd");

                    if (dtDistinct2.Rows.Count > 0)
                        dt = dtDistinct2;

                    rptClaimPolicyList.DataSource = dt;
                    rptClaimPolicyList.DataBind();

                    if (dt.Rows.Count == 0)
                    {
                        Utility.RegisterStartupScriptHandling(this, "Error", "alert('Kindly contact your servicing agent for more info on claims submission for this member');", true, true, _UserIdentityModel.PrincipalName);
                        return;
                    }

                    try
                    {
                        var dateEvent = new DateTime(Convert.ToInt32(_ClaimSubmissionModel.YearOfEvent), Convert.ToInt32(_ClaimSubmissionModel.MonthOfEvent), Convert.ToInt32(_ClaimSubmissionModel.DayOfEvent));

                        var eventDateCheck = dt.AsEnumerable().Where(w => dateEvent >= (DateTime)w["PeriodOfCoverStart"] && dateEvent <= (DateTime)w["PeriodOfCoverEnd"]).Any();

                        if (!eventDateCheck)
                        {
                            Utility.RegisterStartupScriptHandling(this, "Error", "alert('Event date falls outside of all policy period. Kindly contact your servicing agent for more info on claims submission for this member');", true, true, _UserIdentityModel.PrincipalName);
                            dt = new DataTable();

                            rptClaimPolicyList.DataSource = dt;
                            rptClaimPolicyList.DataBind();

                            return;
                        }
                    }
                    catch
                    {
                        Utility.RegisterStartupScriptHandling(this, "Error", "alert('Invalid event date');", true, true, _UserIdentityModel.PrincipalName);
                        dt = new DataTable();

                        rptClaimPolicyList.DataSource = dt;
                        rptClaimPolicyList.DataBind();

                        return;
                    }
                }
                else //Show all headcounts for PIC as well as chosen member's
                {
                    DataTable dt = service.GetBenefitsMemberLevel(memberId, CorpId);

                    DataView dtView = new DataView(dt);
                    DataTable dtDistinct = dtView.ToTable(true, "PolicyId", "Entity", "ProductName", "ContractNo", "DisplayCode", "PeriodOfCoverStart", "PeriodOfCoverEnd");

                    if (dtDistinct.Rows.Count > 0)
                        dt = dtDistinct;

                    //Filter to only those with the BenefitCode selected by user
                    var dtFiltered = dt.AsEnumerable().Where(w => w["DisplayCode"].ToString() == _ClaimSubmissionModel.BenefitCode);

                    if (dtFiltered.Any())
                        dt = dtFiltered.CopyToDataTable();
                    else
                        dt = new DataTable();

                    //Distinct again
                    DataView dtView2 = new DataView(dt);
                    DataTable dtDistinct2 = dtView2.ToTable(true, "PolicyId", "Entity", "ProductName", "ContractNo", "PeriodOfCoverStart", "PeriodOfCoverEnd");

                    if (dtDistinct2.Rows.Count > 0)
                        dt = dtDistinct2;

                    rptClaimPolicyList.DataSource = dt;
                    rptClaimPolicyList.DataBind();

                    if (dt.Rows.Count == 0)
                    {
                        Utility.RegisterStartupScriptHandling(this, "Error", "alert('Kindly contact your servicing agent for more info on claims submission for this member');", true, true, _UserIdentityModel.PrincipalName);
                        return;
                    }

                    try
                    {
                        var dateEvent = new DateTime(Convert.ToInt32(_ClaimSubmissionModel.YearOfEvent), Convert.ToInt32(_ClaimSubmissionModel.MonthOfEvent), Convert.ToInt32(_ClaimSubmissionModel.DayOfEvent));

                        var eventDateCheck = dt.AsEnumerable().Where(w => dateEvent >= (DateTime)w["PeriodOfCoverStart"] && dateEvent <= (DateTime)w["PeriodOfCoverEnd"]).Any();

                        if (!eventDateCheck)
                        {
                            Utility.RegisterStartupScriptHandling(this, "Error", "alert('Event date falls outside of all policy period');", true, true, _UserIdentityModel.PrincipalName);
                            dt = new DataTable();

                            rptClaimPolicyList.DataSource = dt;
                            rptClaimPolicyList.DataBind();

                            return;
                        }
                    }
                    catch
                    {
                        Utility.RegisterStartupScriptHandling(this, "Error", "alert('Invalid event date');", true, true, _UserIdentityModel.PrincipalName);
                        dt = new DataTable();

                        rptClaimPolicyList.DataSource = dt;
                        rptClaimPolicyList.DataBind();

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in LoadClaimPolicyList: " + ex.Message, "ClaimNewPolicy");
            }
        }
        protected void Continue(object sender, EventArgs e)
        {
            try
            {
                var radioChecked = false;
                System.Text.StringBuilder sbPolicySelected = new System.Text.StringBuilder();
                ClaimSubmissionModel _ClaimSubmissionModel = new ClaimSubmissionModel();

                if (Session["ClaimSubmissionModel"] != null)
                {
                    _ClaimSubmissionModel = (ClaimSubmissionModel)Session["ClaimSubmissionModel"];
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "ClaimSubmissionModel loaded", "ClaimNewPolicy");
                }

                HtmlInputCheckBox rdo = new HtmlInputCheckBox();
                for (int intI = 0; intI <= rptClaimPolicyList.Items.Count - 1; intI++)
                {
                    rdo = (HtmlInputCheckBox)rptClaimPolicyList.Items[intI].FindControl("checboxPolicy");

                    if (rdo.Checked)
                    {
                        ClaimSubmissionPolicy subPolicy = new ClaimSubmissionPolicy();
                        subPolicy.PolicyID = rdo.Value;
                        _ClaimSubmissionModel.PolicyList.Add(subPolicy);

                        sbPolicySelected.Append(rdo.Value + ", ");

                        radioChecked = true;
                    }
                }

                //Check if all related fields keyed in
                if (!radioChecked)
                {
                    Utility.RegisterStartupScriptHandling(this, "Error", "alert('No policy selected');", true, true, _UserIdentityModel.PrincipalName);
                    return;
                }

                Session["ClaimSubmissionModel"] = _ClaimSubmissionModel;

                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, _UserIdentityModel.PrincipalName, string.Format("Policy Selected: {0}", sbPolicySelected.ToString()), "ClaimNewPolicy");

                HiddenField hdnCorpId = (HiddenField)Page.Master.FindControl("hdnCorpId");
                var corpID = Utility.Encrypt(hdnCorpId.Value);
                HiddenField hdnUCorpId = (HiddenField)Page.Master.FindControl("hdnUCorpId");
                var UCorpId = Utility.Encrypt(hdnUCorpId.Value);

                Response.Redirect(ResolveUrl("~/Application/ClaimNewBank.aspx?&CorpId=" + corpID + "&UCorpId=" + UCorpId));
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in Continue: " + ex.Message, "ClaimNewPolicy");
            }
            finally
            {
                hdnContinue.Value = "0";
            }
        }
    }
}