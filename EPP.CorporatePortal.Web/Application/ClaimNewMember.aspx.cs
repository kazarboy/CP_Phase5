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
    public partial class ClaimNewMember : System.Web.UI.Page
    {
        private AuditTrail auditTrailService = new AuditTrail();
        private UserIdentityModel _UserIdentityModel = new UserIdentityModel();
        ClaimSubmissionModel _ClaimSubmissionModel = new ClaimSubmissionModel();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session[ConfigurationManager.AppSettings["SessionVariableName"]] != null)
            {
                _UserIdentityModel = (UserIdentityModel)Session[ConfigurationManager.AppSettings["SessionVariableName"]];
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "UserIdentityModel loaded", "ClaimNewMember");
            }

            if (Session["ClaimSubmissionModel"] != null)
            {
                _ClaimSubmissionModel = (ClaimSubmissionModel)Session["ClaimSubmissionModel"];
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "ClaimSubmissionModel loaded", "ClaimNewMember");
            }

            var userName = _UserIdentityModel.PrincipalName;

            var newCorpId = Request.QueryString["CorpId"] ?? "0";
            var returnIndicator = Request.QueryString["Return"] ?? "0";

            try
            {
                HiddenField hdnCorpId = (HiddenField)Page.Master.FindControl("hdnCorpId");
                hdnCorpId.Value = Utility.EncodeAndDecryptCorpId(newCorpId);

                var exitURL = ResolveUrl("~/Application/ClaimListing.aspx?&CorpId=" + newCorpId);
                HiddenField hdnExitURL = (HiddenField)Page.Master.FindControl("hdnExitURL");
                hdnExitURL.Value = exitURL;

                var accessPermission = Rights_Enum.ManageMember;
                HiddenField hdnPermission = (HiddenField)Page.Master.FindControl("hdnPermission");
                hdnPermission.Value = Enum.GetName(typeof(Rights_Enum), accessPermission);

                //Step Processing
                CommonEntities.ClaimProcessSteps(1, Common.Enums.ClaimStepsStatus.active.ToString(), false, "", this, userName);
                CommonEntities.ClaimProcessSteps(2, Common.Enums.ClaimStepsStatus.disabled.ToString(), false, "", this, userName);
                CommonEntities.ClaimProcessSteps(3, Common.Enums.ClaimStepsStatus.disabled.ToString(), false, "", this, userName);
                CommonEntities.ClaimProcessSteps(4, Common.Enums.ClaimStepsStatus.disabled.ToString(), false, "", this, userName);
                CommonEntities.ClaimProcessSteps(5, Common.Enums.ClaimStepsStatus.disabled.ToString(), false, "", this, userName);
                CommonEntities.ClaimProcessSteps(6, Common.Enums.ClaimStepsStatus.disabled.ToString(), false, "", this, userName);

                if (!Page.IsPostBack)
                {
                    //If return from another step, to reinitialize session values
                    if (returnIndicator != "0")
                        ReinitializeSessionValue();
                }                
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Page_Load: " + ex.Message, "ClaimNewMember");
            }
        }
        private void ReinitializeSessionValue()
        {
            var userName = _UserIdentityModel.PrincipalName;

            _ClaimSubmissionModel = CommonEntities.ReInitClaimsMemberSession(userName);
            _ClaimSubmissionModel = CommonEntities.ReInitClaimsClaimSession(userName);
            _ClaimSubmissionModel = CommonEntities.ReInitClaimsPolicySession(userName);
            _ClaimSubmissionModel = CommonEntities.ReInitClaimsBankSession(userName);
            //_ClaimSubmissionModel = CommonEntities.ReInitClaimsDocumentSession(userName);
        }
        protected void Search(object sender, EventArgs e)
        {
            var userName = _UserIdentityModel.PrincipalName;

            LoadMembersList(txtSearchString.Value, selectSearchType.Value, userName);
        }
        private void Repopulate()
        {
            ClaimSubmissionModel _ClaimSubmissionModel = new ClaimSubmissionModel();

            if (Session["ClaimSubmissionModel"] != null)
            {
                _ClaimSubmissionModel = (ClaimSubmissionModel)Session["ClaimSubmissionModel"];
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "ClaimSubmissionModel loaded", "ClaimNewMember");

                HtmlInputCheckBox rdo = new HtmlInputCheckBox();
                for (int intI = 0; intI <= rptClaimMemberList.Items.Count - 1; intI++)
                {
                    rdo = (HtmlInputCheckBox)rptClaimMemberList.Items[intI].FindControl("checkmarkCheckbox");

                    if (rdo.Value == _ClaimSubmissionModel.MemberID)
                        rdo.Checked = true;
                }

                txtMemberIDNo.Text = _ClaimSubmissionModel.MemberIDNoKeyIn;
                selectKeyInMemberIDType.Value = _ClaimSubmissionModel.MemberIDTypeKeyIn;
                txtMemberName.Text = _ClaimSubmissionModel.MemberNameKeyIn;
            }
        }
        private void LoadMembersList(string searchString, string searchType, string userName)
        {
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadMembersList searchType: " + searchType + " searchString: " + searchString, "ClaimNewMember");

            var service = new StoredProcService(userName);

            try
            {
                HiddenField hdnCorpId = (HiddenField)Page.Master.FindControl("hdnCorpId");
                var corpID = Utility.Encrypt(hdnCorpId.Value);

                //load menu
                DataTable dt = service.GetClaimsMemberList(searchString, searchType);
                //To filter out only showing those which valid for logged in PIC
                if (dt.Rows.Count > 0)
                {
                    //Filter FileUpload lists to only those user has access to. PIC/Owner only
                    var policies = CommonEntities.LoadPolicies(corpID, _UserIdentityModel.PrincipalName, _UserIdentityModel.IsOwner);
                    var policyList = policies.AsEnumerable().Select(s => s["ContractNo"].ToString()).Distinct().ToList();
                    var dtFiltered = dt.AsEnumerable().Where(w => policyList.Contains(w["ContractNo"].ToString()));

                    if (dtFiltered.Any())
                        dt = dtFiltered.CopyToDataTable();
                    else
                        dt = new DataTable();

                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Member Search Count(Filtered): " + dt.Rows.Count, "ClaimNewMember");
                }

                rptClaimMemberList.DataSource = dt;
                rptClaimMemberList.DataBind();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in LoadMembersList: " + ex.Message, "ClaimNewMember");
            }
        }
        protected void Continue(object sender, EventArgs e)
        {
            var service = new StoredProcService(_UserIdentityModel.PrincipalName);

            try
            {
                //Validations
                DataTable dt = service.GetClaimsMemberList(txtMemberIDNo.Text, selectKeyInMemberIDType.Value);
                //To filter out only showing those which valid for logged in PIC
                if (dt.Rows.Count > 0)
                {
                    txtMemberName.Text = "";
                    txtMemberIDNo.Text = "";

                    Utility.RegisterStartupScriptHandling(this, "Error", "alert('Member already exist. Please use the search function');", true, true, _UserIdentityModel.PrincipalName);
                    return;
                }

                var radioChecked = false;                

                if (Session["ClaimSubmissionModel"] != null)
                {
                    _ClaimSubmissionModel = (ClaimSubmissionModel)Session["ClaimSubmissionModel"];
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "ClaimSubmissionModel loaded", "ClaimNewMember");
                }

                HtmlInputCheckBox rdo = new HtmlInputCheckBox();
                HiddenField hdnMemberTerminationDate = new HiddenField();
                HtmlTableCell tdCorpName = new HtmlTableCell();
                for (int intI = 0; intI <= rptClaimMemberList.Items.Count - 1; intI++)
                {
                    rdo = (HtmlInputCheckBox)rptClaimMemberList.Items[intI].FindControl("checkmarkCheckbox");

                    hdnMemberTerminationDate = (HiddenField)rptClaimMemberList.Items[intI].FindControl("hdnMemberTerminationDate");

                    tdCorpName = (HtmlTableCell)rptClaimMemberList.Items[intI].FindControl("tdCorpName");

                    if (rdo.Checked)
                    {
                        _ClaimSubmissionModel.MemberID = rdo.Value;
                        try
                        {
                            if (hdnMemberTerminationDate != null && !string.IsNullOrEmpty(hdnMemberTerminationDate.Value))
                                _ClaimSubmissionModel.MemberTerminationDate = Convert.ToDateTime(hdnMemberTerminationDate.Value);
                        }
                        catch (Exception ex)
                        {
                            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "hdnMemberTerminationDate error || value: " + hdnMemberTerminationDate.Value + " error: " + ex, "ClaimNewMember");
                        }
                        radioChecked = true;

                        if (tdCorpName != null && !string.IsNullOrEmpty(tdCorpName.InnerText))
                            _ClaimSubmissionModel.MemberCompanyName = tdCorpName.InnerText;
                    }
                }

                //Check if all related fields keyed in
                if (!radioChecked && (string.IsNullOrEmpty(txtMemberIDNo.Text) || string.IsNullOrEmpty(txtMemberName.Text)))
                {
                    Utility.RegisterStartupScriptHandling(this, "Error", "alert('No Member selected');", true, true, _UserIdentityModel.PrincipalName);
                    return;
                }

                if (radioChecked && (!string.IsNullOrEmpty(txtMemberIDNo.Text) || !string.IsNullOrEmpty(txtMemberName.Text)))
                {
                    Utility.RegisterStartupScriptHandling(this, "Error", "alert('Please select only 1 member');", true, true, _UserIdentityModel.PrincipalName);
                    return;
                }

                if (!radioChecked && selectKeyInMemberIDType.Value == "NRIC" && txtMemberIDNo.Text.Length != 12)
                {
                    Utility.RegisterStartupScriptHandling(this, "Error", "alert('IC No must be 12 digit number without dash');", true, true, _UserIdentityModel.PrincipalName);
                    return;
                }

                _ClaimSubmissionModel.MemberIDNoKeyIn = txtMemberIDNo.Text;
                _ClaimSubmissionModel.MemberIDTypeKeyIn = selectKeyInMemberIDType.Value;
                _ClaimSubmissionModel.MemberNameKeyIn = txtMemberName.Text;

                Session["ClaimSubmissionModel"] = _ClaimSubmissionModel;

                HiddenField hdnCorpId = (HiddenField)Page.Master.FindControl("hdnCorpId");
                var corpID = Utility.Encrypt(hdnCorpId.Value);

                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, _UserIdentityModel.PrincipalName, string.Format("Member Selected: [exist={0}||icno={1}]", radioChecked.ToString(), !string.IsNullOrEmpty(_ClaimSubmissionModel.MemberID) ? _ClaimSubmissionModel.MemberID : _ClaimSubmissionModel.MemberIDNoKeyIn), "ClaimNewMember");

                Response.Redirect(ResolveUrl("~/Application/ClaimNewClaim.aspx?&CorpId=" + corpID));
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in Continue: " + ex.Message, "ClaimNewMember");
            }
            finally
            {
                hdnContinue.Value = "0";
            }
        }
    }
}