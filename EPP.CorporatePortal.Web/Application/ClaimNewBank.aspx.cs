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
    public partial class ClaimNewBank : System.Web.UI.Page
    {
        private AuditTrail auditTrailService = new AuditTrail();
        private UserIdentityModel _UserIdentityModel = new UserIdentityModel();
        private ClaimSubmissionModel _ClaimSubmissionModel = new ClaimSubmissionModel();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session[ConfigurationManager.AppSettings["SessionVariableName"]] != null)
            {
                _UserIdentityModel = (UserIdentityModel)Session[ConfigurationManager.AppSettings["SessionVariableName"]];
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "UserIdentityModel loaded", "ClaimNewBank");
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
                CommonEntities.ClaimProcessSteps(2, Common.Enums.ClaimStepsStatus.complete.ToString(), false, "", this, userName);
                CommonEntities.ClaimProcessSteps(3, Common.Enums.ClaimStepsStatus.complete.ToString(), true, ResolveUrl("~/Application/ClaimNewPolicy.aspx?&CorpId=" + newCorpId + "&UCorpId=" + newUCorpId + "&Return=1"), this, userName);
                CommonEntities.ClaimProcessSteps(4, Common.Enums.ClaimStepsStatus.active.ToString(), false, "", this, userName);
                CommonEntities.ClaimProcessSteps(5, Common.Enums.ClaimStepsStatus.disabled.ToString(), false, "", this, userName);
                CommonEntities.ClaimProcessSteps(6, Common.Enums.ClaimStepsStatus.disabled.ToString(), false, "", this, userName);

                if (Session["ClaimSubmissionModel"] != null)
                {
                    _ClaimSubmissionModel = (ClaimSubmissionModel)Session["ClaimSubmissionModel"];
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "ClaimSubmissionModel loaded", "ClaimNewBank");
                }

                if (!Page.IsPostBack)
                {
                    LoadClaimBankList(hdnCorpId.Value, _ClaimSubmissionModel.MemberID, userName, hdnUCorpId.Value);
                    LoadBankList(userName);

                    //If return from another step, to reinitialize session values
                    if (returnIndicator != "0")
                        ReinitializeSessionValue();
                }
                    
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Page_Load: " + ex.Message, "ClaimNewBank");
            }
        }
        private void ReinitializeSessionValue()
        {
            var userName = _UserIdentityModel.PrincipalName;

            _ClaimSubmissionModel = CommonEntities.ReInitClaimsBankSession(userName);
            //_ClaimSubmissionModel = CommonEntities.ReInitClaimsDocumentSession(userName);
        }
        private void LoadClaimBankList(string CorpId, string memberId, string userName, string UCorpId)
        {
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadClaimBankList MemberId : " + memberId, "ClaimNewBank");

            try
            {
                var service = new StoredProcService(userName);

                List<ClaimSubmissionPolicy> listPolicy = _ClaimSubmissionModel.PolicyList;

                DataTable dt = new DataTable();

                HiddenField hdnCorpId = (HiddenField)Page.Master.FindControl("hdnCorpId");

                foreach (var policy in listPolicy)
                {
                    var dtPolicy = service.GetPolicyDetails(policy.PolicyID, CorpId, UCorpId);

                    dt.Merge(dtPolicy);
                }

                DataView dtView = new DataView(dt);
                DataTable dtDistinct = dtView.ToTable(true, "Name", "BankBranchName", "BankAccountNo", "ProductName", "ContractNo", "CorporateSourceId", "PolicySourceId", "InsurableInterest");

                if (dtDistinct.Rows.Count > 0)
                    dt = dtDistinct;

                //Add new column for hidden bank acc no field
                dt.Columns.Add("HiddenBankAccountNo", typeof(String));

                //Only show last 4 digit of acc no
                foreach (DataRow dr in dt.Rows)
                {
                    // get column values for BankAccountNumber
                    string value = dr["BankAccountNo"].ToString();

                    dr["HiddenBankAccountNo"] = Utility.Encrypt(value);

                    dr["BankAccountNo"] = Last4DigitOnly(value);
                }

                //Separate into different layout according to the InsuredInterest condition
                var dt1 = dt.AsEnumerable().Where(w => w["InsurableInterest"].ToString() == "With");
                var dt2 = dt.AsEnumerable().Where(w => w["InsurableInterest"].ToString() == "Without" || string.IsNullOrEmpty(w["InsurableInterest"].ToString()));

                var dt1Source = new DataTable();
                var dt2Source = new DataTable();

                if (dt1.Any())
                    dt1Source = dt1.CopyToDataTable();
                if (dt2.Any())
                    dt2Source = dt2.CopyToDataTable();

                ////Temp for testing
                //var dt1Source = dt;
                //var dt2Source = dt;

                rptClaimBankList.DataSource = dt1Source;
                rptClaimBankList.DataBind();
                rptClaimBankList2.DataSource = dt2Source;
                rptClaimBankList2.DataBind();

                //Hides if accno empty
                ProcessBankAccountRepeater(rptClaimBankList, userName);
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in LoadClaimBankList: " + ex.Message, "ClaimNewBank");
            }
        }
        private string Last4DigitOnly(string value)
        {
            string result = value;
            try
            {
                if (!string.IsNullOrEmpty(value) && value.Length > 4)
                {
                    // set asterisk to hide first n - 4 digits
                    string asterisks = new string('*', value.Length - 4);

                    // pick last 4 digits for showing
                    string last = value.Substring(value.Length - 4, 4);

                    // combine both asterisk mask and last digits
                    result = asterisks + last;
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in Last4DigitOnly: " + ex.Message, "ClaimNewBank");
            }

            return result;
        }
        protected void AddBank(object sender, EventArgs e)
        {
            try
            {
                var hdnPolicyTmp = hdnPolicySourceIdTmp.Value;
                var hdnAddNewBankRpt = hdnAddNewBankRptType.Value;

                Repeater rpt = null;

                if (hdnAddNewBankRpt == "Nominee")
                    rpt = rptClaimBankList2;
                else
                    rpt = rptClaimBankList;

                for (int intI = 0; intI <= rpt.Items.Count - 1; intI++)
                {
                    HiddenField hdnPolicyIdCurr = (HiddenField)rpt.Items[intI].FindControl("hdnPolicySourceIdCurr");

                    if (hdnPolicyIdCurr.Value == hdnPolicyTmp)
                    {
                        var trAdd = (HtmlTableRow)rpt.Items[intI].FindControl("trAdd");
                        var divNew = (HtmlGenericControl)rpt.Items[intI].FindControl("divNomineeNew");
                        var divAddButton = (HtmlGenericControl)rpt.Items[intI].FindControl("divNomineeAddButton");
                        var spanAddName = (HtmlGenericControl)rpt.Items[intI].FindControl("spanAddName");
                        var spanAddBankBranchName = (HtmlGenericControl)rpt.Items[intI].FindControl("spanAddBankBranchName");
                        var spanBankAccountNo = (HtmlGenericControl)rpt.Items[intI].FindControl("spanAddBankAccountNo");
                        var spanAddBankROC = (HtmlGenericControl)rpt.Items[intI].FindControl("spanAddBankROC");
                        HiddenField hdnAddBankBranchCode = (HiddenField)rpt.Items[intI].FindControl("hdnAddBankBranchCode");
                        HiddenField hdnAddBankAccountNo = (HiddenField)rpt.Items[intI].FindControl("hdnAddBankAccountNo");
                        HiddenField hdnAddBankROC = (HiddenField)rpt.Items[intI].FindControl("hdnAddBankROC");

                        spanAddName.InnerText = newBankName.Value;
                        spanAddBankBranchName.InnerText = newBankBranchDDL.SelectedItem.Text;
                        hdnAddBankBranchCode.Value = newBankBranchDDL.SelectedValue;
                        hdnAddBankAccountNo.Value = Utility.Encrypt(newBankAccNo.Value);
                        spanBankAccountNo.InnerText = Last4DigitOnly(newBankAccNo.Value);
                        hdnAddBankROC.Value = newBankROC.Value;
                        spanAddBankROC.InnerText = newBankROC.Value;

                        //Cater for both repeater cases
                        if (trAdd != null)
                            trAdd.Style.Remove("display");
                        if (divNew != null)
                            divNew.Style.Remove("display");
                        if (divAddButton != null)
                            divAddButton.Style["display"] = "none";

                        if (hdnAddNewBankRpt == "Nominee")
                        {
                            HiddenField hdnNomineeIDType = (HiddenField)rpt.Items[intI].FindControl("hdnNomineeIDType");
                            HiddenField hdnNomineeIDNo = (HiddenField)rpt.Items[intI].FindControl("hdnNomineeIDNo");
                            HiddenField hdnNomineeContactNo = (HiddenField)rpt.Items[intI].FindControl("hdnNomineeContactNo");
                            HiddenField hdnNomineeEmailAddress = (HiddenField)rpt.Items[intI].FindControl("hdnNomineeEmailAddress");

                            //Company doesnt have these fields
                            hdnNomineeIDType.Value = string.Empty;
                            hdnNomineeIDNo.Value = string.Empty;
                            hdnNomineeContactNo.Value = string.Empty;
                            hdnNomineeEmailAddress.Value = string.Empty;

                            labelNomineeUpdateBank.Style.Remove("display");

                            var thBankROC = (HtmlTableCell)rpt.Items[intI].FindControl("thBankROC");
                            var tdBankROC = (HtmlTableCell)rpt.Items[intI].FindControl("tdBankROC");
                            var tdAddColspan = (HtmlTableCell)rpt.Items[intI].FindControl("tdAddColspan");

                            if (thBankROC != null && tdBankROC != null && tdAddColspan != null)
                            {
                                thBankROC.Style.Remove("display");
                                tdBankROC.Style.Remove("display");
                                tdAddColspan.Attributes.Remove("colspan");
                                tdAddColspan.Attributes.Add("colspan", "5");
                            }
                        }

                        //Blank out fields
                        newBankName.Value = string.Empty;
                        newBankBranchDDL.SelectedIndex = 0;
                        newBankAccNo.Value = string.Empty;
                        newBankROC.Value = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in AddBank: " + ex.Message, "ClaimNewBank");
            }
        }
        protected void AddNominee(object sender, EventArgs e)
        {
            try
            {
                var hdnPolicyTmp = hdnPolicySourceIdTmp.Value;

                for (int intI = 0; intI <= rptClaimBankList2.Items.Count - 1; intI++)
                {
                    HiddenField hdnPolicyIdCurr = (HiddenField)rptClaimBankList2.Items[intI].FindControl("hdnPolicySourceIdCurr");

                    if (hdnPolicyIdCurr.Value == hdnPolicyTmp)
                    {
                        var divNew = (HtmlGenericControl)rptClaimBankList2.Items[intI].FindControl("divNomineeNew");
                        var divAddButton = (HtmlGenericControl)rptClaimBankList2.Items[intI].FindControl("divNomineeAddButton");
                        var spanAddName = (HtmlGenericControl)rptClaimBankList2.Items[intI].FindControl("spanAddName");
                        var spanAddBankBranchName = (HtmlGenericControl)rptClaimBankList2.Items[intI].FindControl("spanAddBankBranchName");
                        var spanBankAccountNo = (HtmlGenericControl)rptClaimBankList2.Items[intI].FindControl("spanAddBankAccountNo");
                        var spanAddBankROC = (HtmlGenericControl)rptClaimBankList2.Items[intI].FindControl("spanAddBankROC");
                        HiddenField hdnAddBankBranchCode = (HiddenField)rptClaimBankList2.Items[intI].FindControl("hdnAddBankBranchCode");
                        HiddenField hdnBankAccountNo = (HiddenField)rptClaimBankList2.Items[intI].FindControl("hdnAddBankAccountNo");
                        HiddenField hdnNomineeIDType = (HiddenField)rptClaimBankList2.Items[intI].FindControl("hdnNomineeIDType");
                        HiddenField hdnNomineeIDNo = (HiddenField)rptClaimBankList2.Items[intI].FindControl("hdnNomineeIDNo");
                        HiddenField hdnNomineeContactNo = (HiddenField)rptClaimBankList2.Items[intI].FindControl("hdnNomineeContactNo");
                        HiddenField hdnNomineeEmailAddress = (HiddenField)rptClaimBankList2.Items[intI].FindControl("hdnNomineeEmailAddress");
                        HiddenField hdnAddBankROC = (HiddenField)rptClaimBankList2.Items[intI].FindControl("hdnAddBankROC");

                        if (nomineeIDType.Value == "NRIC" && nomineeIDNo.Value.Length != 12)
                        {
                            Utility.RegisterStartupScriptHandling(this, "Error", "alert('IC No must be 12 digit number without dash');", true, true, _UserIdentityModel.PrincipalName);
                            return;
                        }

                        spanAddName.InnerText = nomineeName.Value;
                        spanAddBankBranchName.InnerText = nomineeBankBranchDDL.SelectedItem.Text;
                        hdnAddBankBranchCode.Value = nomineeBankBranchDDL.SelectedValue;
                        hdnBankAccountNo.Value = Utility.Encrypt(nomineeBankAccNo.Value);
                        spanBankAccountNo.InnerText = Last4DigitOnly(nomineeBankAccNo.Value);

                        hdnNomineeIDType.Value = nomineeIDType.Value;
                        hdnNomineeIDNo.Value = nomineeIDNo.Value;
                        hdnNomineeContactNo.Value = nomineeContact.Value;
                        hdnNomineeEmailAddress.Value = nomineeEmail.Value;

                        divNew.Style.Remove("display");
                        divAddButton.Style["display"] = "none";

                        //Nominees doesn't have BankROC
                        if (spanAddBankROC != null)
                            spanAddBankROC.InnerText = string.Empty;
                        if (hdnAddBankROC != null)
                            hdnAddBankROC.Value = string.Empty;

                        //Hide Bank ROC column
                        var thBankROC = (HtmlTableCell)rptClaimBankList2.Items[intI].FindControl("thBankROC");
                        var tdBankROC = (HtmlTableCell)rptClaimBankList2.Items[intI].FindControl("tdBankROC");
                        var tdAddColspan = (HtmlTableCell)rptClaimBankList2.Items[intI].FindControl("tdAddColspan");
                        thBankROC.Style["display"] = "none";
                        tdBankROC.Style["display"] = "none";
                        tdAddColspan.Attributes.Remove("colspan");
                        tdAddColspan.Attributes.Add("colspan", "4");

                        //Blank out fields
                        nomineeName.Value = string.Empty;
                        nomineeBankBranchDDL.SelectedIndex = 0;
                        nomineeBankAccNo.Value = string.Empty;
                        nomineeIDNo.Value = string.Empty;
                        nomineeContact.Value = string.Empty;
                        nomineeEmail.Value = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in AddNominee: " + ex.Message, "ClaimNewBank");
            }
        }
        private void LoadBankList(string userName)
        {
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadBankList: Started", "ClaimNewBank");

            try
            {
                var service = new StoredProcService(userName);

                DataTable dt = new DataTable();

                dt = service.GetEBParameter("ClaimsBankList");

                if (dt.Rows.Count > 0)
                {
                    newBankBranchDDL.Items.Clear();
                    nomineeBankBranchDDL.Items.Clear();
                    foreach (DataRow dr in dt.Rows)
                    {
                        newBankBranchDDL.Items.Add(new ListItem(dr["Description"].ToString(), dr["Value"].ToString()));
                        nomineeBankBranchDDL.Items.Add(new ListItem(dr["Description"].ToString(), dr["Value"].ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in LoadBankList: " + ex.Message, "ClaimNewBank");
            }

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadBankList: Finished", "ClaimNewBank");
        }
        protected void Continue(object sender, EventArgs e)
        {
            try
            {
                var radioChecked = true;
                var userName = _UserIdentityModel.PrincipalName;

                HtmlInputCheckBox rdo = new HtmlInputCheckBox();
                HtmlInputCheckBox rdo2 = new HtmlInputCheckBox();
                for (int intI = 0; intI <= rptClaimBankList.Items.Count - 1; intI++)
                {
                    if (radioChecked == false)
                        break;

                    rdo = (HtmlInputCheckBox)rptClaimBankList.Items[intI].FindControl("checkmarkCheckbox");
                    rdo2 = (HtmlInputCheckBox)rptClaimBankList.Items[intI].FindControl("checkmarkCheckboxAdd");

                    if ((rdo != null && !rdo.Checked) && (rdo2 != null && !rdo2.Checked))
                    {
                        radioChecked = false;
                    }
                }

                for (int intI = 0; intI <= rptClaimBankList2.Items.Count - 1; intI++)
                {
                    if (radioChecked == false)
                        break;

                    rdo2 = (HtmlInputCheckBox)rptClaimBankList2.Items[intI].FindControl("checkmarkCheckboxAdd");

                    if (rdo2 != null && !rdo2.Checked)
                    {
                        radioChecked = false;
                    }
                }

                //Check if all related fields keyed in
                if (!radioChecked)
                {
                    Utility.RegisterStartupScriptHandling(this, "Error", "alert('No bank account selected');", true, true, _UserIdentityModel.PrincipalName);
                    return;
                }

                List<ClaimSubmissionPolicy> listPolicy = _ClaimSubmissionModel.PolicyList;

                for (int intI = 0; intI <= listPolicy.Count - 1; intI++)
                {
                    var policytmp = listPolicy[intI];

                    var policyList1 = ProcessBankDetailsRepeater(rptClaimBankList, policytmp, userName);
                    var policyList2 = ProcessBankDetailsRepeater(rptClaimBankList2, policytmp, userName);

                    //listPolicy[intI] = !string.IsNullOrEmpty(policyList1.PolicyID) ? policyList1 : policyList2;
                }

                //foreach (var policy in listPolicy)
                //{
                //    var policyList1 = ProcessBankDetailsRepeater(rptClaimBankList, policy, userName);
                //    var policyList2 = ProcessBankDetailsRepeater(rptClaimBankList2, policy, userName);

                //    //Replace the value back into the list
                //    if (_ClaimSubmissionModel.PolicyList.Any(a => a.PolicyID == policy.PolicyID))
                //    {
                //        var policyItem = _ClaimSubmissionModel.PolicyList.Where(w => w.PolicyID == policy.PolicyID).First();
                //        var index = _ClaimSubmissionModel.PolicyList.IndexOf(policyItem);

                //        if (index != -1)
                //            _ClaimSubmissionModel.PolicyList[index] = !string.IsNullOrEmpty(policyList1.PolicyID) ? policyList1 : policyList2;
                //    }
                //}

                Session["ClaimSubmissionModel"] = _ClaimSubmissionModel;

                HiddenField hdnCorpId = (HiddenField)Page.Master.FindControl("hdnCorpId");
                var corpID = Utility.Encrypt(hdnCorpId.Value);
                HiddenField hdnUCorpId = (HiddenField)Page.Master.FindControl("hdnUCorpId");
                var UCorpId = Utility.Encrypt(hdnUCorpId.Value);

                Response.Redirect(ResolveUrl("~/Application/ClaimNewDocument.aspx?&CorpId=" + corpID + "&UCorpId=" + UCorpId));
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in Continue: " + ex.Message, "ClaimNewBank");
            }
            finally
            {
                hdnContinue.Value = "0";
            }
        }
        private ClaimSubmissionPolicy ProcessBankDetailsRepeater(Repeater rpt, ClaimSubmissionPolicy policy, string author)
        {
            ClaimSubmissionPolicy retPolicy = new ClaimSubmissionPolicy();

            try
            {
                for (int intI = 0; intI <= rpt.Items.Count - 1; intI++)
                {
                    HiddenField hdnPolicyIdCurr = (HiddenField)rpt.Items[intI].FindControl("hdnPolicySourceIdCurr");

                    if (hdnPolicyIdCurr.Value == policy.PolicyID)
                    {
                        retPolicy = policy;

                        var rdoClaimBankList = (HtmlInputCheckBox)rpt.Items[intI].FindControl("checkmarkCheckbox");
                        var rdoClaimBankListAdd = (HtmlInputCheckBox)rpt.Items[intI].FindControl("checkmarkCheckboxAdd");

                        if (rdoClaimBankList != null && rdoClaimBankList.Checked)
                        {
                            var spanName = (HtmlGenericControl)rpt.Items[intI].FindControl("spanName");
                            var spanBankBranchName = (HtmlGenericControl)rpt.Items[intI].FindControl("spanBankBranchName");
                            var spanBankROC = (HtmlGenericControl)rpt.Items[intI].FindControl("spanBankROC");
                            HiddenField hdnBankAccountNo = (HiddenField)rpt.Items[intI].FindControl("hdnBankAccountNo");
                            HiddenField hdnAddBankBranchCode = (HiddenField)rpt.Items[intI].FindControl("hdnAddBankBranchCode");

                            retPolicy.AccountHolderName = spanName.InnerText;
                            retPolicy.BankName = spanBankBranchName.InnerText;
                            retPolicy.BankCode = hdnAddBankBranchCode.Value;
                            retPolicy.BankAccountNo = Utility.EncodeAndDecryptCorpId(hdnBankAccountNo.Value);
                            retPolicy.BankROC = spanBankROC.InnerText;
                        }
                        else
                        {
                            var spanName = (HtmlGenericControl)rpt.Items[intI].FindControl("spanAddName");
                            var spanBankBranchName = (HtmlGenericControl)rpt.Items[intI].FindControl("spanAddBankBranchName");
                            HiddenField hdnBankAccountNo = (HiddenField)rpt.Items[intI].FindControl("hdnAddBankAccountNo");
                            HiddenField hdnAddBankBranchCode = (HiddenField)rpt.Items[intI].FindControl("hdnAddBankBranchCode");

                            retPolicy.AccountHolderName = spanName.InnerText;
                            retPolicy.BankName = spanBankBranchName.InnerText;
                            retPolicy.BankCode = hdnAddBankBranchCode.Value;
                            retPolicy.BankAccountNo = Utility.EncodeAndDecryptCorpId(hdnBankAccountNo.Value);

                            //For repeaters with hidden bank roc
                            HiddenField hdnAddBankROC = (HiddenField)rpt.Items[intI].FindControl("hdnAddBankROC");
                            if (hdnAddBankROC != null)
                                retPolicy.BankROC = hdnAddBankROC.Value;

                            //For repeaters with hidden Nominee items
                            HiddenField hdnNomineeIDType = (HiddenField)rpt.Items[intI].FindControl("hdnNomineeIDType");
                            if (hdnNomineeIDType != null)
                                retPolicy.IDType = hdnNomineeIDType.Value;
                            HiddenField hdnNomineeIDNo = (HiddenField)rpt.Items[intI].FindControl("hdnNomineeIDNo");
                            if (hdnNomineeIDNo != null)
                                retPolicy.IDNo = hdnNomineeIDNo.Value;
                            HiddenField hdnNomineeContactNo = (HiddenField)rpt.Items[intI].FindControl("hdnNomineeContactNo");
                            if (hdnNomineeContactNo != null)
                                retPolicy.ContactNo = hdnNomineeContactNo.Value;
                            HiddenField hdnNomineeEmailAddress = (HiddenField)rpt.Items[intI].FindControl("hdnNomineeEmailAddress");
                            if (hdnNomineeEmailAddress != null)
                                retPolicy.EmailAddress = hdnNomineeEmailAddress.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in ProcessBankDetailsRepeater: " + ex.Message, "ClaimNewBank");
            }

            return retPolicy;
        }
        private void ProcessBankAccountRepeater(Repeater rpt, string author)
        {
            try
            {
                for (int intI = 0; intI <= rpt.Items.Count - 1; intI++)
                {
                    var trBankAccount = (HtmlTableRow)rpt.Items[intI].FindControl("trBankAccount");
                    var spanBankAccountNo = (HtmlGenericControl)rpt.Items[intI].FindControl("spanBankAccountNo");

                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "ProcessBankAccountRepeater: BankAccountNo= " + (spanBankAccountNo != null ? spanBankAccountNo.InnerText : "invalid obj"), "ClaimNewBank");

                    //Hide if account no is null or empty
                    if (spanBankAccountNo != null && string.IsNullOrEmpty(spanBankAccountNo.InnerText))
                        trBankAccount.Style["display"] = "none";
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, author, "Error in ProcessBankAccountRepeater: " + ex.Message, "ClaimNewBank");
            }
        }
        protected void rptClaimBankList2_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            try
            {
                var radioOne = (HtmlInputRadioButton)e.Item.FindControl("radioone");
                var radioTwo = (HtmlInputRadioButton)e.Item.FindControl("radiotwo");

                var labelRadioOne = (HtmlGenericControl)e.Item.FindControl("labelRadioOne");
                var labelRadioTwo = (HtmlGenericControl)e.Item.FindControl("labelRadioTwo");

                if (labelRadioOne != null && radioOne != null)
                {
                    labelRadioOne.Attributes.Remove("for");
                    labelRadioOne.Attributes.Add("for", radioOne.ClientID);
                }
                if (labelRadioTwo != null && radioTwo != null)
                {
                    labelRadioTwo.Attributes.Remove("for");
                    labelRadioTwo.Attributes.Add("for", radioTwo.ClientID);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in rptClaimBankList2_ItemDataBound: " + ex.Message, "ClaimNewBank");
            }
        }
    }
}