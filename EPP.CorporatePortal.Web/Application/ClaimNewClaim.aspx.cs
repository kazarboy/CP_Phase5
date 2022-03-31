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
    public partial class ClaimNewClaim : System.Web.UI.Page
    {
        private AuditTrail auditTrailService = new AuditTrail();
        private UserIdentityModel _UserIdentityModel = new UserIdentityModel();
        ClaimSubmissionModel _ClaimSubmissionModel = new ClaimSubmissionModel();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session[ConfigurationManager.AppSettings["SessionVariableName"]] != null)
            {
                _UserIdentityModel = (UserIdentityModel)Session[ConfigurationManager.AppSettings["SessionVariableName"]];
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "UserIdentityModel loaded", "ClaimNewClaim");
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
                CommonEntities.ClaimProcessSteps(1, Common.Enums.ClaimStepsStatus.complete.ToString(), true, ResolveUrl("~/Application/ClaimNewMember.aspx?&CorpId=" + newCorpId + "&UCorpId=" + newUCorpId + "&Return=1"), this, userName);
                CommonEntities.ClaimProcessSteps(2, Common.Enums.ClaimStepsStatus.active.ToString(), false, "", this, userName);
                CommonEntities.ClaimProcessSteps(3, Common.Enums.ClaimStepsStatus.disabled.ToString(), false, "", this, userName);
                CommonEntities.ClaimProcessSteps(4, Common.Enums.ClaimStepsStatus.disabled.ToString(), false, "", this, userName);
                CommonEntities.ClaimProcessSteps(5, Common.Enums.ClaimStepsStatus.disabled.ToString(), false, "", this, userName);
                CommonEntities.ClaimProcessSteps(6, Common.Enums.ClaimStepsStatus.disabled.ToString(), false, "", this, userName);

                if (Session["ClaimSubmissionModel"] != null)
                {
                    _ClaimSubmissionModel = (ClaimSubmissionModel)Session["ClaimSubmissionModel"];
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "ClaimSubmissionModel loaded", "ClaimNewMember");
                }

                //Load claims
                if (!Page.IsPostBack)
                {
                    PopulateDatesDropdown();
                    LoadClaimTypeList(hdnCorpId.Value, _ClaimSubmissionModel.MemberID, userName);
                    LoadCICategoryList(userName);

                    //For member termination date validation
                    HiddenField hdnMemberTerminationDate = (HiddenField)Page.Master.FindControl("MainContent").FindControl("hdnMemberTerminationDate");
                    if (hdnMemberTerminationDate != null)
                        hdnMemberTerminationDate.Value = _ClaimSubmissionModel.MemberTerminationDate.ToString();

                    //If return from another step, to reinitialize session values
                    if (returnIndicator != "0")
                        ReinitializeSessionValue();
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Page_Load: " + ex.Message, "ClaimNewClaim");
            }
        }
        private void ReinitializeSessionValue()
        {
            var userName = _UserIdentityModel.PrincipalName;

            _ClaimSubmissionModel = CommonEntities.ReInitClaimsClaimSession(userName);
            _ClaimSubmissionModel = CommonEntities.ReInitClaimsPolicySession(userName);
            _ClaimSubmissionModel = CommonEntities.ReInitClaimsBankSession(userName);
            //_ClaimSubmissionModel = CommonEntities.ReInitClaimsDocumentSession(userName);
        }
        protected void ClaimListChangeLogic(object sender, EventArgs e)
        {
            var service = new StoredProcService(_UserIdentityModel.PrincipalName);

            HiddenField hdnCorpId = (HiddenField)Page.Master.FindControl("hdnCorpId");
            LoadClaimTypeList(hdnCorpId.Value, _ClaimSubmissionModel.MemberID, _UserIdentityModel.PrincipalName);
        }
        private void ClaimListChangeLogic(DataTable dt)
        {
            //Check if BenefitGrouping exists
            if (dt.AsEnumerable().Where(w => w["GroupCode"].ToString() == hdnSelectedType.Value && !string.IsNullOrEmpty(w["GroupCode"].ToString())).Any())
            {
                //Indicate whether show or not for validation
                hdnClaimTypeOptdiv.Value = hdnSelectedType.Value;
                hdnClaimTypeOpt2div.Value = hdnSelectedType.Value;
                hdnClaimTypeOptdiv3.Value = string.Empty;

                if (hdnSelectedType.Value == "D" || hdnSelectedType.Value == "FEM")
                {
                    claimTypeOptdiv.Style["display"] = "block";
                    claimTypeOptLabel.InnerText = "Cause of death";

                    claimTypeOpt2div.Style["display"] = "block";
                    claimTypeOpt2Label.InnerText = "Date of death";
                }                
                else
                {
                    claimTypeOptdiv.Style["display"] = "block";
                    claimTypeOptLabel.InnerText = "Cause of event";

                    claimTypeOpt2div.Style["display"] = "block";
                    claimTypeOpt2Label.InnerText = "Date of event";
                }

                //claimTypeOpt3div.Style["display"] = "block";
                claimTypeOpt3div.Style["display"] = "none";

                divCICategory.Style["display"] = "none";
            }
            else
            {
                if (hdnSelectedType.Value == "CI")
                {
                    //Indicate whether show or not for validation
                    hdnClaimTypeOptdiv.Value = string.Empty;
                    hdnClaimTypeOpt2div.Value = hdnSelectedType.Value;
                    hdnClaimTypeOptdiv3.Value = hdnSelectedType.Value;

                    claimTypeOptdiv.Style["display"] = "none";
                    claimTypeOpt3div.Style["display"] = "none";

                    claimTypeOpt2div.Style["display"] = "block";
                    claimTypeOpt2Label.InnerText = "Date of event";

                    divCICategory.Style["display"] = "block";
                }
                else
                {
                    //Indicate whether show or not for validation
                    hdnClaimTypeOptdiv.Value = string.Empty;
                    hdnClaimTypeOpt2div.Value = hdnSelectedType.Value;
                    hdnClaimTypeOptdiv3.Value = string.Empty;

                    claimTypeOptdiv.Style["display"] = "none";
                    claimTypeOpt3div.Style["display"] = "none";

                    claimTypeOpt2div.Style["display"] = "block";
                    claimTypeOpt2Label.InnerText = "Date of event";

                    divCICategory.Style["display"] = "none";
                }
            }
        }
        private void LoadClaimTypeList(string selectedType, string CorpId)
        {
            LoadClaimTypeList(selectedType, CorpId, "");
        }
        private void LoadClaimTypeList(string CorpId, string memberId, string userName)
        {
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadClaimTypeList Existing MemberId: " + memberId, "ClaimNewClaim");

            try
            {
                var service = new StoredProcService(userName);

                var selectedType = hdnSelectedType.Value;

                //load menu
                DataTable dt = new DataTable();

                if (string.IsNullOrEmpty(memberId)) //Show all headcounts for PIC
                    dt = service.GetBenefitsPolicyLevel(CorpId);
                else //Show all headcounts for PIC as well as chosen member's
                    dt = service.GetBenefitsMemberLevel(memberId, CorpId);

                var containsDTH = dt.AsEnumerable().Where(w => w["BenefitCode"].ToString() == "DTH").Any();
                if (containsDTH)
                {
                    hdnContainsDTH.Value = "DTH";
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "List contains DTH Claim Type", "ClaimNewClaim");
                }

                var containsPPD= dt.AsEnumerable().Where(w => w["BenefitCode"].ToString() == "PPD").Any();
                if (containsPPD)
                {
                    hdnContainsDTH.Value = "PPD";
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "List contains PPD Claim Type", "ClaimNewClaim");
                }

                var containsPTD = dt.AsEnumerable().Where(w => w["BenefitCode"].ToString() == "PTD").Any();
                if (containsPTD)
                {
                    hdnContainsDTH.Value = "PTD";
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "List contains PTD Claim Type", "ClaimNewClaim");
                }

                try
                {
                    //Check if contains both Funeral and Death Benefits
                    var listNonEmbeddedFuneral = service.GetEBParameter("NonEmbeddedFuneral").AsEnumerable().Select(s => s["Value"].ToString()).ToList();
                    var listNonEmbeddedDeath = service.GetEBParameter("NonEmbeddedDeath").AsEnumerable().Select(s => s["Value"].ToString()).ToList();
                    if (dt.AsEnumerable().Where(w => listNonEmbeddedDeath.Contains(w["BenefitCode"].ToString())).Any() && dt.AsEnumerable().Where(w => listNonEmbeddedFuneral.Contains(w["BenefitCode"].ToString())).Any())
                    {
                        _ClaimSubmissionModel.NonEmbeddedDeathFuneral = true;
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Death and Funeral Benefits exists", "ClaimNewClaim");
                    }
                    else
                        _ClaimSubmissionModel.NonEmbeddedDeathFuneral = false;

                    Session["ClaimSubmissionModel"] = _ClaimSubmissionModel;
                }
                catch (Exception ex)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in LoadClaimTypeList (NonEmbeddedFuneralCheck): " + ex.Message, "ClaimNewClaim");
                }

                DataView dtView = new DataView(dt);
                DataTable dtDistinct = dtView.ToTable(true, "DisplayCode", "DisplayName");

                DataTable dtBenefitGroup = dtView.ToTable(true, "GroupCode", "GroupName");

                if (dtDistinct.Rows.Count > 0)
                    dt = dtDistinct;

                rptClaimTypeList.DataSource = dt;
                rptClaimTypeList.DataBind();

                if (dt.Rows.Count == 0)
                {
                    spanSelectedType.InnerText = "Select claim type";
                    spanSelectedTooltip.Attributes.Remove("title");
                    hdnSelectedType.Value = "";

                    claimTypeOptdiv.Style["display"] = "none";

                    claimTypeOpt3div.Style["display"] = "none";

                    claimTypeOpt2div.Style["display"] = "none";

                    divCICategory.Style["display"] = "none";

                    Utility.RegisterStartupScriptHandling(this, "Error", "alert('Kindly contact your servicing agent for more info on claims submission for this member');", true, true, _UserIdentityModel.PrincipalName);
                    return;
                }

                if (!string.IsNullOrEmpty(selectedType) && dt.AsEnumerable().Where(w => w["DisplayCode"].ToString() == selectedType).Any())
                {
                    var dtFiltered = dt.AsEnumerable().Where(w => w["DisplayCode"].ToString() != selectedType);

                    var selectedTypeDesc = dt.AsEnumerable().Where(w => w["DisplayCode"].ToString() == selectedType).Select(s => s["DisplayName"].ToString()).FirstOrDefault();

                    if (dtFiltered.Any())
                        dt = dtFiltered.CopyToDataTable();

                    spanSelectedType.InnerText = selectedTypeDesc;
                    spanSelectedTooltip.Attributes.Remove("title");
                    spanSelectedTooltip.Attributes.Add("title", selectedTypeDesc);
                }
                else
                {
                    var selectedTypeDesc = dt.AsEnumerable().Where(w => w["DisplayCode"].ToString() == selectedType).Select(s => s["DisplayName"].ToString()).FirstOrDefault();

                    spanSelectedType.InnerText = "Select claim type";
                    spanSelectedTooltip.Attributes.Remove("title");
                    hdnSelectedType.Value = "";
                }

                ClaimListChangeLogic(dtBenefitGroup);
                ClaimListChangeLogic(dtBenefitGroup);
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in LoadClaimTypeList: " + ex.Message, "ClaimNewClaim");
            }
        }
        private void LoadCICategoryList(string userName)
        {
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadCICategoryList: Started", "ClaimNewClaim");

            try
            {
                var service = new StoredProcService(userName);

                var selectedType = hdnSelectedCICategory.Value;

                //load menu
                DataTable dt = new DataTable();

                dt = service.GetEBParameter("CICategory");

                rptClaimTypeListCI.DataSource = dt;
                rptClaimTypeListCI.DataBind();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in LoadCICategoryList: " + ex.Message, "ClaimNewClaim");
            }

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadCICategoryList: Finished", "ClaimNewClaim");
        }
        private void PopulateDatesDropdown()
        {
            //YYYY
            for (int i = (DateTime.Now.Year) - 2; i != (DateTime.Now.Year) + 1; i++)
            {
                dropdownYYYY.Items.Add(i.ToString());
            }
            //DD
            for (int i = 1; i <= 31; i++)
            {
                dropdownDD.Items.Add(i.ToString());
            }
            //MM
            for (int i = 1; i <= 12; i++)
            {
                dropdownMM.Items.Add(i.ToString());
            }
        }
        protected void Continue(object sender, EventArgs e)
        {
            try
            {

                ClaimSubmissionModel _ClaimSubmissionModel = new ClaimSubmissionModel();

                if (Session["ClaimSubmissionModel"] != null)
                {
                    _ClaimSubmissionModel = (ClaimSubmissionModel)Session["ClaimSubmissionModel"];
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "ClaimSubmissionModel loaded", "ClaimNewMember");
                }

                HiddenField hdnCorpId = (HiddenField)Page.Master.FindControl("hdnCorpId");

                HtmlInputRadioButton radioOne = (HtmlInputRadioButton)Page.Master.FindControl("MainContent").FindControl("radioOne");
                HtmlInputRadioButton radioTwo = (HtmlInputRadioButton)Page.Master.FindControl("MainContent").FindControl("radioTwo");

                _ClaimSubmissionModel.BenefitCode = hdnSelectedType.Value;                

                if (!string.IsNullOrEmpty(hdnClaimTypeOptdiv.Value))
                    _ClaimSubmissionModel.CauseOfEvent = radioOne.Checked ? radioOne.Value : radioTwo.Checked ? radioTwo.Value : "";
                if (!string.IsNullOrEmpty(hdnClaimTypeOptdiv3.Value))
                    _ClaimSubmissionModel.CauseOfEvent = hdnSelectedCICategory.Value ?? "";

                _ClaimSubmissionModel.DayOfEvent = dropdownDD.Text;
                _ClaimSubmissionModel.YearOfEvent = dropdownYYYY.Text;
                _ClaimSubmissionModel.MonthOfEvent = dropdownMM.Text;

                //_ClaimSubmissionModel.BenefitCodeOri = hdnSelectedOriType.Value;

                var service = new StoredProcService(_UserIdentityModel.PrincipalName);

                List<string> benefitCodeAllcauseslistShortened = new List<string>(new string[] { "PPD", "PTD", "TPD", "TTD" });                

                if (_ClaimSubmissionModel.BenefitCode == "D" && !string.IsNullOrEmpty(hdnContainsDTH.Value) && hdnContainsDTH.Value == "DTH") //If DTH, will be DTH
                    _ClaimSubmissionModel.BenefitCodeOri = "DTH";
                else if (benefitCodeAllcauseslistShortened.Contains(_ClaimSubmissionModel.BenefitCode)) //If other All Causes
                {
                    DataTable dtAllCauses = new DataTable();

                    if (string.IsNullOrEmpty(_ClaimSubmissionModel.MemberID)) //Show all headcounts for PIC
                        dtAllCauses = service.GetBenefitsPolicyLevel(hdnCorpId.Value);
                    else //Show all headcounts for PIC as well as chosen member's
                        dtAllCauses = service.GetBenefitsMemberLevel(_ClaimSubmissionModel.MemberID, hdnCorpId.Value);

                    var containsAllCauses = dtAllCauses.AsEnumerable().Where(w => w["BenefitCode"].ToString() == _ClaimSubmissionModel.BenefitCode).Any();
                    if (containsAllCauses) //Only front portion if all causes
                        _ClaimSubmissionModel.BenefitCodeOri = _ClaimSubmissionModel.BenefitCode;
                    else
                        _ClaimSubmissionModel.BenefitCodeOri = _ClaimSubmissionModel.BenefitCode + _ClaimSubmissionModel.CauseOfEvent;
                } //Others
                else
                    _ClaimSubmissionModel.BenefitCodeOri = _ClaimSubmissionModel.BenefitCode == "CI" ? _ClaimSubmissionModel.BenefitCode : _ClaimSubmissionModel.BenefitCode + _ClaimSubmissionModel.CauseOfEvent;

                //Check if all related fields keyed in
                if (string.IsNullOrEmpty(hdnSelectedType.Value) || string.IsNullOrEmpty(dropdownDD.Text) || string.IsNullOrEmpty(dropdownMM.Text) || string.IsNullOrEmpty(dropdownYYYY.Text)
                    || (!string.IsNullOrEmpty(hdnClaimTypeOptdiv3.Value) && string.IsNullOrEmpty(hdnSelectedCICategory.Value)))
                {
                    Utility.RegisterStartupScriptHandling(this, "Error", "alert('Please fill out all fields');", true, true, _UserIdentityModel.PrincipalName);
                    return;
                }
                var date = new DateTime();
                try
                {
                    date = new DateTime(Convert.ToInt32(dropdownYYYY.Text), Convert.ToInt32(dropdownMM.Text), Convert.ToInt32(dropdownDD.Text));
                }
                catch
                {
                    Utility.RegisterStartupScriptHandling(this, "Error", "alert('Invalid date of event');", true, true, _UserIdentityModel.PrincipalName);
                    return;
                }

                HiddenField hdnMemberTerminationDate = (HiddenField)Page.Master.FindControl("MainContent").FindControl("hdnMemberTerminationDate");
                if (hdnMemberTerminationDate != null && !string.IsNullOrEmpty(hdnMemberTerminationDate.Value) && hdnMemberTerminationDate.Value != "1/1/0001 12:00:00 AM")
                {
                    var memberTerminationDate = DateTime.Parse(hdnMemberTerminationDate.Value);
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "Member Termination Date: " + hdnMemberTerminationDate.Value, "ClaimNewClaim");
                    if (memberTerminationDate.Date < date.Date && memberTerminationDate.Date != DateTime.MinValue.Date)
                    {
                        Utility.RegisterStartupScriptHandling(this, "Error", "alert('Date of Event is after Member Termination Date: " + memberTerminationDate.Date.ToString("dd/MM/yyyy") + "');", true, true, _UserIdentityModel.PrincipalName);
                        return;
                    }
                }

                _ClaimSubmissionModel.EventDescription = txtCauseDesc.Value;

                var dtAccidentMapping = service.GetEBParameter("ClaimsAccidentMap");

                if (dtAccidentMapping.Rows.Count > 0)
                {
                    var accidentMapped = dtAccidentMapping.AsEnumerable().Where(w => w["Value"].ToString() == _ClaimSubmissionModel.BenefitCodeOri).Select(s => s["Description"].ToString()).FirstOrDefault();
                    if (!string.IsNullOrEmpty(accidentMapped))
                        _ClaimSubmissionModel.Accident = accidentMapped;
                }

                try
                {
                    //Check if contains both Funeral and Death Benefits and benefit code chosen is either
                    var listNonEmbeddedFuneral = service.GetEBParameter("NonEmbeddedFuneral").AsEnumerable().Select(s => s["Value"].ToString()).ToList();
                    var listNonEmbeddedDeath = service.GetEBParameter("NonEmbeddedDeath").AsEnumerable().Select(s => s["Value"].ToString()).ToList();
                    if ((listNonEmbeddedDeath.Contains(_ClaimSubmissionModel.BenefitCodeOri) || listNonEmbeddedFuneral.Contains(_ClaimSubmissionModel.BenefitCodeOri)) && _ClaimSubmissionModel.NonEmbeddedDeathFuneral)
                    {
                        _ClaimSubmissionModel.NonEmbeddedDeathFuneral = true;
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, _UserIdentityModel.PrincipalName, "Death and Funeral Benefits exists and chosen", "ClaimNewClaim");
                    }
                    else
                        _ClaimSubmissionModel.NonEmbeddedDeathFuneral = false;
                }
                catch (Exception ex)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in Continue (NonEmbeddedFuneralCheck): " + ex.Message, "ClaimNewClaim");
                }

                Session["ClaimSubmissionModel"] = _ClaimSubmissionModel;

                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, _UserIdentityModel.PrincipalName, string.Format("Claim Selected: [BenefitCode={0}||OriBenefitCode={1}]", _ClaimSubmissionModel.BenefitCode, _ClaimSubmissionModel.BenefitCodeOri), "ClaimNewClaim");

                var corpID = Utility.Encrypt(hdnCorpId.Value);
                HiddenField hdnUCorpId = (HiddenField)Page.Master.FindControl("hdnUCorpId");
                var UCorpId = Utility.Encrypt(hdnUCorpId.Value);

                Response.Redirect(ResolveUrl("~/Application/ClaimNewPolicy.aspx?&CorpId=" + corpID + "&UCorpId=" + UCorpId));
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in Continue: " + ex.Message, "ClaimNewClaim");
            }
            finally
            {
                hdnContinue.Value = "0";
            }
        }
    }
}