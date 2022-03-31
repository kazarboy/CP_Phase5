using EPP.CorporatePortal.Common;
using EPP.CorporatePortal.DAL.Model;
using EPP.CorporatePortal.DAL.Service;
using EPP.CorporatePortal.Helper;
using EPP.CorporatePortal.Models;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static EPP.CorporatePortal.Common.Enums;

namespace EPP.CorporatePortal.Application
{
    public partial class ClaimNewConfirm : System.Web.UI.Page
    {
        private AuditTrail auditTrailService = new AuditTrail();
        private UserIdentityModel _UserIdentityModel = new UserIdentityModel();
        private ClaimSubmissionModel _ClaimSubmissionModel = new ClaimSubmissionModel();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session[ConfigurationManager.AppSettings["SessionVariableName"]] != null)
            {
                _UserIdentityModel = (UserIdentityModel)Session[ConfigurationManager.AppSettings["SessionVariableName"]];
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "UserIdentityModel loaded", "ClaimNewConfirm");
            }

            if (Session["ClaimSubmissionModel"] != null)
            {
                _ClaimSubmissionModel = (ClaimSubmissionModel)Session["ClaimSubmissionModel"];
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "ClaimSubmissionModel loaded", "ClaimNewConfirm");
            }

            var newCorpId = Request.QueryString["CorpId"] ?? "0";
            var newUCorpId = Request.QueryString["UCorpId"] ?? "0";

            if (!Page.IsPostBack)
            {
                var userName = _UserIdentityModel.PrincipalName;                

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
                    CommonEntities.ClaimProcessSteps(3, Common.Enums.ClaimStepsStatus.complete.ToString(), false, "", this, userName);
                    CommonEntities.ClaimProcessSteps(4, Common.Enums.ClaimStepsStatus.complete.ToString(), false, "", this, userName);
                    CommonEntities.ClaimProcessSteps(5, Common.Enums.ClaimStepsStatus.complete.ToString(), true, ResolveUrl("~/Application/ClaimNewDocument.aspx?&CorpId=" + newCorpId + "&UCorpId=" + newUCorpId + "&Return=1"), this, userName);
                    CommonEntities.ClaimProcessSteps(6, Common.Enums.ClaimStepsStatus.active.ToString(), false, "", this, userName);

                    //Load claims                
                    LoadAllList(_UserIdentityModel.BusinessRegistrationNo, _ClaimSubmissionModel.MemberID, userName);
                }
                catch (Exception ex)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Page_Load: " + ex.Message, "ClaimNewConfirm");
                }
            }
        }
        private void LoadAllList(string CorpId, string memberId, string userName)
        {
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadAllList: Started", "ClaimNewConfirm");

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
                spanSubmitterROC.InnerText = _UserIdentityModel.ParentBizRegNo;//_UserIdentityModel.BusinessRegistrationNo;
                spanSubmitterCompany.InnerText = _UserIdentityModel.ParentCorporate;

                //TnC, based on the 1st policy's Entity type
                var policy = claimObject.PolicyList.FirstOrDefault();

                if (policy != null)
                {
                    var drPolicy = service.GetPolicyDetails(policy.PolicyID, _UserIdentityModel.BusinessRegistrationNo, _UserIdentityModel.UCorpId).AsEnumerable().FirstOrDefault();

                    if (drPolicy != null && drPolicy["Entity"] != null && drPolicy["Entity"].ToString() == "ETB")
                    {
                        trTncELIB.Style["display"] = "none";
                        trTncEFTB.Style.Remove("display");
                    }
                    else
                    {
                        trTncEFTB.Style["display"] = "none";
                        trTncELIB.Style.Remove("display");
                    }
                }

            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in LoadAllList: " + ex.Message, "ClaimNewConfirm");
            }

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadAllList: Finished", "ClaimNewConfirm");
        }
        private void ProcessPolicyBankRepeater(string userName)
        {
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "ProcessPolicyBankRepeater: Started", "ClaimNewConfirm");

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
                        var drPolicy = service.GetPolicyDetails(hdnPolicyIdCurr.Value, _UserIdentityModel.BusinessRegistrationNo, _UserIdentityModel.UCorpId).AsEnumerable().First();

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
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in ProcessPolicyBankRepeater: " + ex.Message, "ClaimNewConfirm");
            }

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "ProcessPolicyBankRepeater: Finished", "ClaimNewConfirm");
        }
        private void ProcessDocRepeater(string userName)
        {
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "ProcessDocRepeater: Started", "ClaimNewConfirm");

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
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in ProcessDocRepeater: " + ex.Message, "ClaimNewConfirm");
            }

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "ProcessDocRepeater: Finished", "ClaimNewConfirm");
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
        private byte[] GenerateEFormPDF(ClaimSubmissionModel claimObject, DataRow drMember, ClaimSubmissionPolicy policy, DataRow drMemberClaims, string userName)
        {
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "GenerateEFormPDF: Started", "ClaimNewConfirm");

            try
            {
                byte[] retByte;

                var service = new StoredProcService(userName);                

                var docList = claimObject.DocumentList;
                var docEFTB = new ClaimSubmissionDocEFTB();
                
                docEFTB.ContractHolderName = _UserIdentityModel.ParentCorporate;
                docEFTB.SubsidiaryName = !string.IsNullOrEmpty(_ClaimSubmissionModel.MemberCompanyName) ? _ClaimSubmissionModel.MemberCompanyName : _UserIdentityModel.ParentCorporate;
                if (drMember != null)
                {
                    docEFTB.LifeAssuredName = drMember["MemberName"].ToString();
                    docEFTB.LifeAssuredID = !string.IsNullOrEmpty(drMember["ICNo"].ToString()) ? drMember["ICNo"].ToString() : drMember["IDNo"].ToString();
                }
                else
                {
                    docEFTB.LifeAssuredName = claimObject.MemberNameKeyIn;
                    docEFTB.LifeAssuredID = claimObject.MemberIDNoKeyIn;
                }
                docEFTB.SubmitterName = _UserIdentityModel.Name;
                docEFTB.SubmitterContactNo = _UserIdentityModel.MobileNumber;
                docEFTB.SubmitterEmail = _UserIdentityModel.Email;
                docEFTB.BankName = policy.BankName;
                docEFTB.BankAccNo = policy.BankAccountNo;
                docEFTB.BankBranchName = policy.BankName;
                docEFTB.BankROC = policy.BankROC;

                docEFTB.AccountHolderName = policy.AccountHolderName;
                docEFTB.AccountHolderNRIC = policy.IDNo;

                DataTable dt = new DataTable();

                if (string.IsNullOrEmpty(claimObject.MemberID)) //Show all headcounts for PIC
                    dt = service.GetBenefitsPolicyLevel(_UserIdentityModel.BusinessRegistrationNo);
                else //Show all headcounts for PIC as well as chosen member's
                    dt = service.GetBenefitsMemberLevel(claimObject.MemberID, _UserIdentityModel.BusinessRegistrationNo);

                DataView dtView = new DataView(dt);
                DataTable dtDistinct = dtView.ToTable(true, "BenefitCode", "BenefitDescription");

                //Only CI has different listing category
                var sessionBenefitCode = _ClaimSubmissionModel.BenefitCodeOri;
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

                docEFTB.ClaimType = benefitCode ?? claimObject.BenefitCode;
                docEFTB.DateOfEvent = new DateTime(Convert.ToInt32(claimObject.YearOfEvent), Convert.ToInt32(claimObject.MonthOfEvent), Convert.ToInt32(claimObject.DayOfEvent)).ToString("dd MMMM yyyy");
                docEFTB.CauseOfEvent = causeCode ?? claimObject.CauseOfEvent;

                var sbDocList = new StringBuilder();
                foreach (var doc in docList)
                {
                    var docName = string.Empty;
                    var dtDoc = service.GetRequiredDocByDocID(doc.DocumentId);
                    if (dtDoc.Rows.Count > 0)
                    {
                        var drDoc = dtDoc.AsEnumerable().First();
                        docName = drDoc["DocumentName"].ToString();
                    }

                    sbDocList.AppendLine(doc.FileName + " (" + docName + ")");
                }

                docEFTB.UploadedDocList = sbDocList.ToString();

                var drPolicy = service.GetPolicyDetails(policy.PolicyID, _UserIdentityModel.BusinessRegistrationNo, _UserIdentityModel.UCorpId).AsEnumerable().FirstOrDefault();

                docEFTB.ContractNo = drPolicy["ContractNo"] != null ? drPolicy["ContractNo"].ToString() : string.Empty;

                docEFTB.SigClaimantName = _UserIdentityModel.Name;
                docEFTB.SigSubmissionDate = DateTime.Now.ToString("dd MMMM yyyy");
                docEFTB.PortalClaimNo = drMemberClaims["PortalClaimNo"].ToString();        
                
                if (drPolicy["Entity"] != null && drPolicy["Entity"].ToString() == "ETB")
                {
                    docEFTB.EtiqaLogo = Server.MapPath("~/Application/Download/Templates/Images/Etiqa-Logo_EFTB.png");
                    docEFTB.EtiqaFooter = Server.MapPath("~/Application/Download/Templates/Images/Etiqa-Footer_EFTB.png");

                    retByte = Utility.SearchAndReplaceEFTBHtml(Server.MapPath("~/Application/Download/Templates/EFTB_Template.html"), docEFTB, userName);
                }
                else //ELIB
                {
                    docEFTB.EtiqaLogo = Server.MapPath("~/Application/Download/Templates/Images/Etiqa-Logo_ELIB.png");
                    docEFTB.EtiqaFooter = Server.MapPath("~/Application/Download/Templates/Images/Etiqa-Footer_ELIB.png");

                    retByte = Utility.SearchAndReplaceELIBHtml(Server.MapPath("~/Application/Download/Templates/ELIB_Template.html"), docEFTB, userName);
                }

                //if (drPolicy["Entity"] != null && drPolicy["Entity"].ToString() == "ETB")
                //    retByte = Utility.SearchAndReplaceEFTB(Server.MapPath("~/Application/Download/Templates/E-Claim Auto Form_EFTB_Template.docx"), docEFTB, userName);
                //else //ELIB
                //    retByte = Utility.SearchAndReplaceELIB(Server.MapPath("~/Application/Download/Templates/E-Claim Auto Form_ELIB_Template.docx"), docEFTB, userName);

                //retByte = Utility.OpenXMLBytesToPDF(retByte, userName);

                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "GenerateEFormPDF: Finished", "ClaimNewConfirm");

                return retByte;
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in GenerateEFormPDF: " + ex.Message, "ClaimNewConfirm");
            }

            //return empty bytes if not success
            Byte[] array = new Byte[64];
            Array.Clear(array, 0, array.Length);
            return array;
        }
        private void GenerateFNAFile(string folderPath, string fileName, DataRow dr, string userName)
        {
            try
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Generating FNA File", "ClaimNewConfirm");

                var service = new StoredProcService(userName);

                var fnaFileName = Path.GetFileNameWithoutExtension(fileName) + ".FNA";

                byte[] bytes = null;
                using (var ms = new MemoryStream())
                {
                    TextWriter tw = new StreamWriter(ms);

                    var drPolicy = service.GetPolicyDetails(dr["PolicyId"].ToString(), _UserIdentityModel.BusinessRegistrationNo, _UserIdentityModel.UCorpId).AsEnumerable().FirstOrDefault();

                    var BussCat = string.Empty;
                    var PolicyNo = string.Empty;
                    var Entity = string.Empty;
                    if (drPolicy != null)
                    {
                        //BussCat = drPolicy["ContractNo"].ToString()[0] == 'T' ? "Takaful" : "Conventional";
                        BussCat = drPolicy["Entity"].ToString().ToUpper().Equals("ETB") ? "Takaful" : "Conventional";
                        PolicyNo = drPolicy["ContractNo"].ToString();
                        Entity = drPolicy["Entity"].ToString().ToUpper().Equals("ETB") ? "EFTB" : "ELIB";
                    }
                    var BussType = "Individual";
                    var ClaimIC = dr["ICNo"].ToString();
                    var ClaimNo = dr["PortalClaimNo"].ToString();
                    var CoName = _UserIdentityModel.ParentCorporate;
                    var ICetc = dr["IDNo"].ToString();
                    var ReferenceNo = dr["PortalClaimNo"].ToString();
                    var FileName = fileName;                    

                    var textToWrite = String.Format("446,447,448,449,451,||{0},{1},{2},{3},{4},{5},{6}||{7}||||N||{8}||{9}||", BussCat, BussType, ClaimIC, ClaimNo, CoName, ICetc, PolicyNo, Entity, ReferenceNo, FileName);

                    tw.Write(textToWrite);
                    tw.Flush();
                    ms.Position = 0;
                    bytes = ms.ToArray();
                }

                if (bytes != null)
                {
                    var eIWLocalPath = CommonService.GetSystemConfigValue("EIWLocalPath");
                    var eIWLocalSave = CommonService.GetSystemConfigValue("EIWLocalSave");
                    if (eIWLocalSave.ToUpper().Equals("TRUE"))
                    {                    
                        try
                        {
                            //Save to local for backup
                            File.WriteAllBytes(Server.MapPath(eIWLocalPath) + fnaFileName, bytes);
                        }
                        catch (Exception ex)
                        {
                            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in CombineUploadedFile (WriteAllBytes):" + ex.Message, "ClaimNewConfirm");
                        }
                    }

                    //Save to remote
                    var remoteUsername = CommonService.GetSystemConfigValue("EIWRemoteAccessUsername");
                    var remotePassword = CommonService.GetSystemConfigValue("EIWRemoteAccessPassword");

                    Utility.SendRemoteFileBytesAndCreateFolder(bytes, folderPath + fnaFileName, Path.GetFileNameWithoutExtension(fnaFileName), remoteUsername, remotePassword, userName, out bool resultSendRemote);
                    if (!resultSendRemote)
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "GenerateFNAFile: Error transferring file to remote", "ClaimNewConfirm");
                    }
                    else
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "GenerateFNAFile: Success transferring file to remote", "ClaimNewConfirm");
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in GenerateFNAFile: " + ex.Message, "ClaimNewConfirm");
            }

            //File.WriteAllText(filePath, "");
        }
        private string ConstructDirectory(string baseUploadFilesPath, string fileName)
        {
            var directory = Server.MapPath(baseUploadFilesPath + Path.AltDirectorySeparatorChar + fileName + Path.AltDirectorySeparatorChar);
            return directory;
        }
        private void CombineUploadedFile(byte[] eFormBytes, string folderPath, DataRow dr, string userName)
        {
            try
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Generating FNA File", "ClaimNewConfirm");

                var service = new StoredProcService(userName);

                var maxFileSize = Convert.ToInt32(CommonService.GetSystemConfigValue("ClaimMaxUploadSize"));

                var docList = _ClaimSubmissionModel.DocumentList;
                var totalSize = 0;

                List<PdfDocument> pdfDocumentList = new List<PdfDocument>();

                var outPdf = new PdfDocument();

                //Combine with eForm 1st
                MemoryStream eFormDocStream = new MemoryStream(eFormBytes);
                var eFormPDF = PdfSharp.Pdf.IO.PdfReader.Open(eFormDocStream, PdfDocumentOpenMode.Import);
                outPdf = CombinePages(eFormPDF, outPdf);

                var lastItem = docList.Last();
                foreach (var doc in docList)
                {
                    MemoryStream docStream = new MemoryStream(doc.FileBytes);
                    if (!((totalSize + doc.FileBytes.Length) > maxFileSize))
                    {
                        var pdf = PdfSharp.Pdf.IO.PdfReader.Open(docStream, PdfDocumentOpenMode.Import);
                        outPdf = CombinePages(pdf, outPdf);

                        //If last item and not exceed max file size limit
                        if (doc.Equals(lastItem))
                        {
                            pdfDocumentList.Add(outPdf);
                        }
                    }
                    else
                    {
                        //Split into multiple files if bigger than max size
                        pdfDocumentList.Add(outPdf);

                        //Reinitialize outPdf
                        outPdf = new PdfDocument();
                        var pdf = PdfSharp.Pdf.IO.PdfReader.Open(docStream, PdfDocumentOpenMode.Import);
                        outPdf = CombinePages(pdf, outPdf);

                        //If last item and not exceed max file size limit
                        if (doc.Equals(lastItem))
                        {
                            pdfDocumentList.Add(outPdf);
                        }
                    }
                    totalSize += doc.FileBytes.Length;
                }

                //Remote transfer files
                var fileCount = 1;
                foreach (var pdfDoc in pdfDocumentList)
                {
                    var tmpFileName = string.Empty;
                    if (fileCount == 1)
                        tmpFileName = dr["PortalClaimNo"].ToString() + ".pdf";
                    else
                        tmpFileName = dr["PortalClaimNo"].ToString() + "_" + fileCount.ToString() + ".pdf";

                    //Convert PdfDocument to Bytes
                    var fileBytes = Utility.PdfDocumentToBytes(pdfDoc, userName);

                    var eIWLocalPath = CommonService.GetSystemConfigValue("EIWLocalPath");
                    var eIWLocalSave = CommonService.GetSystemConfigValue("EIWLocalSave");
                    if (eIWLocalSave.ToUpper().Equals("TRUE"))
                    {
                        try
                        {
                            //Save to local for backup
                            File.WriteAllBytes(Server.MapPath(eIWLocalPath) + tmpFileName, fileBytes);
                        }
                        catch (Exception ex)
                        {
                            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in CombineUploadedFile (WriteAllBytes):" + ex.Message, "ClaimNewConfirm");
                        }
                    }

                    var remoteUsername = CommonService.GetSystemConfigValue("EIWRemoteAccessUsername");
                    var remotePassword = CommonService.GetSystemConfigValue("EIWRemoteAccessPassword");

                    Utility.SendRemoteFileBytesAndCreateFolder(fileBytes, folderPath + tmpFileName, Path.GetFileNameWithoutExtension(tmpFileName), remoteUsername, remotePassword, userName, out bool resultSendRemote);
                    if (!resultSendRemote)
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "CombineUploadedFile: Error transferring file", "ClaimNewConfirm");
                    }
                    else
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "CombineUploadedFile: Success transferring file", "ClaimNewConfirm");
                        //Generate FNA file for EIW
                        GenerateFNAFile(folderPath, tmpFileName, dr, userName);                        
                    }

                    fileCount++;
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in CombineUploadedFile: " + ex.Message, "ClaimNewConfirm");
            }
        }
        private PdfDocument CombinePages(PdfDocument from, PdfDocument to)
        {
            for (int i = 0; i < from.PageCount; i++)
            {
                to.AddPage(from.Pages[i]);
            }

            return to;
        }
        private bool Save(string userName)
        {
            bool ret = true;

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Save: Started", "ClaimNewConfirm");

            try
            {
                var service = new StoredProcService(userName);

                var claimObject = _ClaimSubmissionModel;

                DataRow drMember = null;
                if (!string.IsNullOrEmpty(claimObject.MemberID))
                {
                    var dtMember = service.GetClaimsMemberByMemberSourceID(claimObject.MemberID);
                    if (dtMember.Rows.Count > 0)
                    {
                        drMember = dtMember.AsEnumerable().FirstOrDefault();
                    }
                }

                DataRow drUser = null;
                var dtUser = service.GetUserByUsername(userName);
                if (dtUser.Rows.Count > 0)
                {
                    drUser = dtUser.AsEnumerable().FirstOrDefault();
                }

                ////Temp generate file bytes
                //var fileBytes = System.IO.File.ReadAllBytes("");
                //var encFileBytes = EncryptionHelper.AESEncryptByteData(eFormBytes);

                var folderPathEIW = CommonService.GetSystemConfigValue("EIWDestinationPath");
                var fileNameEIW = CommonService.GetSystemConfigValue("EIWFileName");

                var policyList = claimObject.PolicyList;
                var docList = claimObject.DocumentList;
                foreach (var policy in policyList)
                {                                    
                    var memberClaimsID = service.InsMemberClaims(claimObject, drMember, drUser, policy);

                    if (memberClaimsID == 0)
                    {
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Save: Error, memberClaimsID = 0", "ClaimNewConfirm");

                        ret = false;
                        return ret;                        
                    }

                    var dtMemberClaims = service.GetMemberClaimsByID(memberClaimsID);
                    if (dtMemberClaims.Rows.Count > 0)
                    {
                        var drMemberClaims = dtMemberClaims.AsEnumerable().FirstOrDefault();

                        //Generate e-application form for the specific policy
                        var eFormBytes = GenerateEFormPDF(claimObject, drMember, policy, drMemberClaims, userName);

                        //Save eForm to MemberClaims
                        service.UpdateMemberClaimsEFormByID(memberClaimsID, EncryptionHelper.AESEncryptByteData(eFormBytes));

                        //Combine and send uploaded files to EIW path
                        CombineUploadedFile(eFormBytes, folderPathEIW, drMemberClaims, userName);
                    }
                    else
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in PDF Generation and Combination: No MemberClaims record found", "ClaimNewConfirm");                    

                    foreach (var doc in docList)
                    {
                        service.InsMemberClaimsDocument(true, memberClaimsID, doc.DocumentId, doc.FileName, EncryptionHelper.AESEncryptByteData(doc.FileBytes), DateTime.Now);
                    }                    

                    //Save eForm in session for Success Page
                    policy.MemberClaimID = memberClaimsID;
                }

                _ClaimSubmissionModel.PolicyList = policyList;
                Session["ClaimSubmissionModel"] = _ClaimSubmissionModel;
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Save: " + ex.Message, "ClaimNewConfirm");
            }

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Save: Finished", "ClaimNewConfirm");

            return ret;
        }
        protected void Continue(object sender, EventArgs e)
        {
            try
            {
                var userName = _UserIdentityModel.PrincipalName;

                if (!tncChecbox.Checked)
                {
                    Utility.RegisterStartupScriptHandling(this, "Error", "alert('Please agree to the Terms & Conditions');", true, true, _UserIdentityModel.PrincipalName);
                    return;
                }

                //Save all object's data to database
                var saveSuccess = Save(userName);

                if (!saveSuccess)
                {
                    Utility.RegisterStartupScriptHandling(this, "Error", "alert('Error in saving claims details');", true, true, _UserIdentityModel.PrincipalName);
                    return;
                }

                Session["ClaimSubmissionModel"] = _ClaimSubmissionModel;

                HiddenField hdnCorpId = (HiddenField)Page.Master.FindControl("hdnCorpId");
                var corpID = Utility.Encrypt(hdnCorpId.Value);
                HiddenField hdnUCorpId = (HiddenField)Page.Master.FindControl("hdnUCorpId");
                var UCorpId = Utility.Encrypt(hdnUCorpId.Value);

                Response.Redirect(ResolveUrl("~/Application/ClaimNewSuccess.aspx?&CorpId=" + corpID + "&UCorpId=" + UCorpId));
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in Continue: " + ex.Message, "ClaimNewConfirm");
            }
            finally
            {
                hdnContinue.Value = "0";
            }
        }
    }
}