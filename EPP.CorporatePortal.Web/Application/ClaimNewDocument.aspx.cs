using EPP.CorporatePortal.Common;
using EPP.CorporatePortal.DAL.Model;
using EPP.CorporatePortal.DAL.Service;
using EPP.CorporatePortal.Helper;
using EPP.CorporatePortal.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
    public partial class ClaimNewDocument : System.Web.UI.Page
    {
        private AuditTrail auditTrailService = new AuditTrail();
        private UserIdentityModel _UserIdentityModel = new UserIdentityModel();
        private ClaimSubmissionModel _ClaimSubmissionModel = new ClaimSubmissionModel();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session[ConfigurationManager.AppSettings["SessionVariableName"]] != null)
            {
                _UserIdentityModel = (UserIdentityModel)Session[ConfigurationManager.AppSettings["SessionVariableName"]];
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "UserIdentityModel loaded", "ClaimNewDocument");
            }

            if (Session["ClaimSubmissionModel"] != null)
            {
                _ClaimSubmissionModel = (ClaimSubmissionModel)Session["ClaimSubmissionModel"];
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "ClaimSubmissionModel loaded", "ClaimNewDocument");
            }

            var newCorpId = Request.QueryString["CorpId"] ?? "0";
            var newUCorpId = Request.QueryString["UCorpId"] ?? "0";

            var returnIndicator = Request.QueryString["Return"] ?? "0";

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
                    CommonEntities.ClaimProcessSteps(4, Common.Enums.ClaimStepsStatus.complete.ToString(), true, ResolveUrl("~/Application/ClaimNewBank.aspx?&CorpId=" + newCorpId + "&UCorpId=" + newUCorpId + "&Return=1"), this, userName);
                    CommonEntities.ClaimProcessSteps(5, Common.Enums.ClaimStepsStatus.active.ToString(), false, "", this, userName);
                    CommonEntities.ClaimProcessSteps(6, Common.Enums.ClaimStepsStatus.disabled.ToString(), false, "", this, userName);
                    
                    //If return from another step, to reinitialize session values
                    //if (returnIndicator != "0")
                    //    ReinitializeSessionValue();

                    //Load claims                
                    LoadReqDocList(hdnCorpId.Value, _ClaimSubmissionModel.MemberID, userName);
                }
                catch (Exception ex)
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Page_Load: " + ex.Message, "ClaimNewDocument");
                }
            }
        }
        private void ReinitializeSessionValue()
        {
            var userName = _UserIdentityModel.PrincipalName;

            //_ClaimSubmissionModel = CommonEntities.ReInitClaimsDocumentSession(userName);
        }
        private void LoadReqDocList(string CorpId, string memberId, string userName)
        {
            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadReqDocList: Started", "ClaimNewDocument");

            try
            {
                var maxFileSize = Convert.ToInt32(CommonService.GetSystemConfigValue("ClaimMaxUploadSize"));

                if (maxFileSize > 0)
                    pPDFLimitLabel.InnerText = "Only PDF format is allowed and only one PDF file for each document. The file size cannot exceed " + ((maxFileSize / 1024f) / 1024f).ToString("0.0") + " MB in total.";

                var service = new StoredProcService(userName);

                var docBenefitCode = (_ClaimSubmissionModel.BenefitCode ?? "") + (_ClaimSubmissionModel.CauseOfEvent ?? "");

                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadReqDocList: BenefitCode = " + docBenefitCode, "ClaimNewDocument");

                DataTable dt = new DataTable();

                dt = service.GetRequiredDocList(docBenefitCode);

                //Separate into different layout according to the various conditions
                var dt1 = dt.AsEnumerable().Where(w => (bool)w["IsMandatoryIndicator"] == true && (bool)w["DeathOccurOversea"] == false);
                var dt2 = dt.AsEnumerable().Where(w => (bool)w["IsMandatoryIndicator"] == false && (bool)w["DeathOccurOversea"] == false);
                var dt3 = dt.AsEnumerable().Where(w => (bool)w["IsMandatoryIndicator"] == true && (bool)w["DeathOccurOversea"] == true);
                var dt4 = dt.AsEnumerable().Where(w => (bool)w["IsMandatoryIndicator"] == false && (bool)w["DeathOccurOversea"] == true);

                var dt1Source = new DataTable();
                var dt2Source = new DataTable();
                var dt3Source = new DataTable();
                var dt4Source = new DataTable();

                if (dt1.Any())
                {
                    divMandatoryDocList.Style.Remove("display");
                    dt1Source = dt1.CopyToDataTable();
                }
                else
                    divMandatoryDocList.Style["display"] = "none";
                if (dt2.Any())
                {
                    divSupportingDocList.Style.Remove("display");
                    dt2Source = dt2.CopyToDataTable();
                }
                else
                    divSupportingDocList.Style["display"] = "none";
                if (dt3.Any())
                {
                    divOverseasMandatoryDocList.Style.Remove("display");
                    dt3Source = dt3.CopyToDataTable();
                }
                else
                {
                    divOverseasMandatoryDocList.Style["display"] = "none";
                    tableOverseasMandatoryDocList.Style["display"] = "none";
                }
                if (dt4.Any())
                {
                    divOverseasSupportingDocList.Style.Remove("display");
                    dt4Source = dt4.CopyToDataTable();
                }
                else
                {
                    divOverseasSupportingDocList.Style["display"] = "none";
                    tableOverseasSupportingDocList.Style["display"] = "none";
                }
                //Overseas button
                if (dt3.Any() || dt4.Any())
                    divOverseasButton.Style.Remove("display");
                else
                    divOverseasButton.Style["display"] = "none";

                rptMandatoryDocList.DataSource = dt1Source;
                rptMandatoryDocList.DataBind();
                rptSupportingDocList.DataSource = dt2Source;
                rptSupportingDocList.DataBind();
                rptOverseasMandatoryDocList.DataSource = dt3Source;
                rptOverseasMandatoryDocList.DataBind();
                rptOverseasSupportingDocList.DataSource = dt4Source;
                rptOverseasSupportingDocList.DataBind();

                ProcessImagePreview(userName);
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in LoadReqDocList: " + ex.Message, "ClaimNewDocument");
            }

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadReqDocList: Finished", "ClaimNewDocument");
        }
        protected void Upload(object sender, EventArgs e)
        {
            var userName = _UserIdentityModel.PrincipalName;

            Button btn = (Button)sender;

            if (btn.CommandName == "UploadFile")
            {
                var inputId = hdnDocSourceIdTmp.Value;

                ProcessUpload(inputId, userName);

                ////Check on each repeater and proces the necessary items
                //ProcessUploadRepeater(rptMandatoryDocList, inputId, userName);
                //ProcessUploadRepeater(rptSupportingDocList, inputId, userName);
                //ProcessUploadRepeater(rptOverseasMandatoryDocList, inputId, userName);
                //ProcessUploadRepeater(rptOverseasSupportingDocList, inputId, userName);
            }            
        }
        private void ProcessImagePreview(string userName)
        {
            List<ClaimSubmissionDocument> listDoc = _ClaimSubmissionModel.DocumentList;

            if (listDoc.Any())
            {
                ProcessImagePreviewRepeater(rptMandatoryDocList, listDoc, userName);
                ProcessImagePreviewRepeater(rptSupportingDocList, listDoc, userName);
                ProcessImagePreviewRepeater(rptOverseasMandatoryDocList, listDoc, userName);
                ProcessImagePreviewRepeater(rptOverseasSupportingDocList, listDoc, userName);
            }

            if (rptOverseasMandatoryDocList.Items.Count > 0)
                divOverseasMandatoryDocList.Style.Remove("display");

            if (rptOverseasSupportingDocList.Items.Count > 0)
                divOverseasSupportingDocList.Style.Remove("display");

            if (_ClaimSubmissionModel.DeathOverseas == true)
                deathOccurOversea.Checked = true;
        }
        private void ProcessImagePreviewRepeater(Repeater rpt, List<ClaimSubmissionDocument> listDoc, string author)
        {
            try
            {
                for (int intI = 0; intI <= rpt.Items.Count - 1; intI++)
                {
                    HiddenField hdnDocIdCurr = (HiddenField)rpt.Items[intI].FindControl("hdnDocSourceIdCurr");
                    HtmlGenericControl divGalleryImg = (HtmlGenericControl)rpt.Items[intI].FindControl("divGalleryImg");

                    if (hdnDocIdCurr != null && listDoc.Where(w => w.DocumentId == Convert.ToInt32(hdnDocIdCurr.Value)).Any())
                    {
                        HtmlGenericControl pFileName = (HtmlGenericControl)rpt.Items[intI].FindControl("pFileName");

                        var doc = listDoc.Where(w => w.DocumentId == Convert.ToInt32(hdnDocIdCurr.Value)).First();

                        divGalleryImg.Style.Remove("display");
                        pFileName.InnerText = doc.FileName;
                    }
                    else
                    {
                        divGalleryImg.Style["display"] = "none";
                    }
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in ProcessImagePreviewRepeater: " + ex.Message, "ClaimNewDocument");
            }
        }
        private void ProcessImageRemoveRepeater(Repeater rpt, string docId, string author)
        {
            try
            {
                for (int intI = 0; intI <= rpt.Items.Count - 1; intI++)
                {
                    HiddenField hdnDocIdCurr = (HiddenField)rpt.Items[intI].FindControl("hdnDocSourceIdCurr");
                    HtmlGenericControl divGalleryImg = (HtmlGenericControl)rpt.Items[intI].FindControl("divGalleryImg");

                    if (hdnDocIdCurr != null && hdnDocIdCurr.Value == docId)
                    {
                        HtmlGenericControl pFileName = (HtmlGenericControl)rpt.Items[intI].FindControl("pFileName");

                        divGalleryImg.Style["display"] = "none";
                        pFileName.InnerText = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in ProcessImageRemoveRepeater: " + ex.Message, "ClaimNewDocument");
            }
        }
        private void ProcessImageRemoveSession(string docId, string author)
        {
            try
            {
                _ClaimSubmissionModel.DocumentList.RemoveAll(x => x.DocumentId == Convert.ToInt32(docId));

                Session["ClaimSubmissionModel"] = _ClaimSubmissionModel;
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in ProcessImageRemoveSession: " + ex.Message, "ClaimNewDocument");
            }
        }
        protected void RemoveImg(object sender, EventArgs e)
        {
            var userName = _UserIdentityModel.PrincipalName;

            LinkButton btn = (LinkButton)sender;
            var docId = btn.CommandArgument;

            ProcessImageRemoveRepeater(rptMandatoryDocList, docId, userName);
            ProcessImageRemoveRepeater(rptSupportingDocList, docId, userName);
            ProcessImageRemoveRepeater(rptOverseasMandatoryDocList, docId, userName);
            ProcessImageRemoveRepeater(rptOverseasSupportingDocList, docId, userName);

            ProcessImageRemoveSession(docId, userName);
        }
        private void ProcessOnchangeRepeater(Repeater rpt, string author)
        {
            try
            {
                for (int intI = 0; intI <= rpt.Items.Count - 1; intI++)
                {
                    HiddenField hdnDocIdCurr = (HiddenField)rpt.Items[intI].FindControl("hdnDocSourceIdCurr");

                    FileUpload fileUploadControl = (FileUpload)rpt.Items[intI].FindControl("FileUploadDoc");

                    fileUploadControl.Attributes["onchange"] = string.Format("previewImage(this, 'div.gallery-{0}')", hdnDocIdCurr.Value);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in ProcessOnchangeRepeater: " + ex.Message, "ClaimNewDocument");
            }
        }
        private void ProcessUpload(string docId, string author)
        {
            try
            {
                byte[] uploadedFileBinary;
                var maxFileSize = Convert.ToInt32(CommonService.GetSystemConfigValue("ClaimMaxUploadSize"));

                FileInfo fi = new FileInfo(ctlFileUpload.PostedFile.FileName);
                if (fi.Extension != ".pdf")
                {
                    Utility.RegisterStartupScriptHandling(this, "Error", "alert('Only .pdf file allowed');", true, true, author);
                    return;
                }

                //var totalUploadedFileSize = ProcessUploadTotalFileSize(author);

                uploadedFileBinary = ctlFileUpload.FileBytes;

                //var oriTotalUploadedFileSize = totalUploadedFileSize;

                //totalUploadedFileSize += uploadedFileBinary.Length;

                if (uploadedFileBinary.Length > maxFileSize)
                {
                    Utility.RegisterStartupScriptHandling(this, "Error", "alert('Total uploaded size exceeded limit: " + ((maxFileSize / 1024f) / 1024f).ToString("0.0") + " MB.');", true, true, author);
                    return;
                }

                ClaimSubmissionDocument subDocument = new ClaimSubmissionDocument();
                subDocument.DocumentId = Convert.ToInt32(docId);
                subDocument.FileName = fi.Name;
                subDocument.FileBytes = uploadedFileBinary;

                //To replace object if exists
                if (_ClaimSubmissionModel.DocumentList.Any(a => a.DocumentId == Convert.ToInt32(docId)))
                {
                    var docItem = _ClaimSubmissionModel.DocumentList.Where(w => w.DocumentId == Convert.ToInt32(docId)).First();
                    var index = _ClaimSubmissionModel.DocumentList.IndexOf(docItem);

                    if (index != -1)
                        _ClaimSubmissionModel.DocumentList[index] = subDocument;
                } //else add new
                else
                    _ClaimSubmissionModel.DocumentList.Add(subDocument);

                Session["ClaimSubmissionModel"] = _ClaimSubmissionModel;

                ProcessImagePreview(author);
            }
            catch (Exception ex)
            {
                //keep on showing the popup and show the error
                Utility.RegisterStartupScriptHandling(this, "Error", "alert('Error reading pdf data');", true, true, author);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in ProcessUpload: " + ex.Message, "ClaimNewDocument");
                return;
            }
        }
        private int ProcessUploadTotalFileSize(string userName)
        {
            int res = 0;
            try
            {
                var docList = _ClaimSubmissionModel.DocumentList;
                var totalSize = 0;

                foreach (var doc in docList)
                {
                    totalSize += doc.FileBytes.Length;
                }

                res = totalSize;
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in ProcessUploadFileSize: " + ex.Message, "ClaimNewDocument");
                throw;
            }

            return res;
        }
        private void ProcessUploadRepeater (Repeater rpt, string rptId, string author)
        {
            for (int intI = 0; intI <= rpt.Items.Count - 1; intI++)
            {
                HiddenField hdnDocIdCurr = (HiddenField)rpt.Items[intI].FindControl("hdnDocSourceIdCurr");

                if (hdnDocIdCurr.Value == rptId)
                {
                    FileUpload fileUploadControl = (FileUpload)rpt.Items[intI].FindControl("FileUploadDoc");
                    if (fileUploadControl != null)
                    {
                        try
                        {
                            byte[] uploadedFileBinary;

                            FileInfo fi = new FileInfo(fileUploadControl.PostedFile.FileName);
                            if (fi.Extension != ".pdf")
                            {
                                Utility.RegisterStartupScriptHandling(this, "Error", "alert('Only .pdf file allowed');", true, true, author);
                                return;
                            }

                            uploadedFileBinary = fileUploadControl.FileBytes;

                            ClaimSubmissionDocument subDocument = new ClaimSubmissionDocument();
                            subDocument.DocumentId = Convert.ToInt32(rptId);
                            subDocument.FileBytes = uploadedFileBinary;

                            //To replace object if exists
                            if (_ClaimSubmissionModel.DocumentList.Any(a => a.DocumentId == Convert.ToInt32(rptId)))
                            {
                                var docItem = _ClaimSubmissionModel.DocumentList.Where(w => w.DocumentId == Convert.ToInt32(rptId)).First();
                                var index = _ClaimSubmissionModel.DocumentList.IndexOf(docItem);

                                if (index != -1)
                                    _ClaimSubmissionModel.DocumentList[index] = subDocument;
                            } //else add new
                            else
                                _ClaimSubmissionModel.DocumentList.Add(subDocument);
                        }
                        catch (Exception ex)
                        {
                            //keep on showing the popup and show the error
                            Utility.RegisterStartupScriptHandling(this, "Error", "alert('Error reading pdf data');", true, true, author);
                            return;
                        }
                    }
                }                
            }
        }
        private bool ProcessCheckMandatoryRepeater(Repeater rpt, string author)
        {
            var boolRes = true;
            try
            {
                for (int intI = 0; intI <= rpt.Items.Count - 1; intI++)
                {
                    HiddenField hdnDocIdCurr = (HiddenField)rpt.Items[intI].FindControl("hdnDocSourceIdCurr");
                    HtmlGenericControl pFileName = (HtmlGenericControl)rpt.Items[intI].FindControl("pFileName");

                    if (pFileName != null && string.IsNullOrEmpty(pFileName.InnerText))
                    {
                        boolRes = false;
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "DocumentID " + hdnDocIdCurr.Value + " has no file attached.", "ClaimNewDocument");
                    }
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, author, "Error in ProcessCheckMandatoryRepeater: " + ex.Message, "ClaimNewDocument");
            }
            return boolRes;
        }
        protected void Continue(object sender, EventArgs e)
        {
            try
            {
                var userName = _UserIdentityModel.PrincipalName;
                var service = new StoredProcService(_UserIdentityModel.PrincipalName);
                var attachmentCheck = false;

                //Validations
                attachmentCheck = ProcessCheckMandatoryRepeater(rptMandatoryDocList, userName);
                attachmentCheck = ProcessCheckMandatoryRepeater(rptMandatoryDocList, userName);

                if (attachmentCheck == false)
                {
                    Utility.RegisterStartupScriptHandling(this, "Error", "alert('Please upload any mandatory documents');", true, true, _UserIdentityModel.PrincipalName);
                    return;
                }

                Session["ClaimSubmissionModel"] = _ClaimSubmissionModel;

                HiddenField hdnCorpId = (HiddenField)Page.Master.FindControl("hdnCorpId");
                var corpID = Utility.Encrypt(hdnCorpId.Value);
                HiddenField hdnUCorpId = (HiddenField)Page.Master.FindControl("hdnUCorpId");
                var UCorpId = Utility.Encrypt(hdnUCorpId.Value);

                Response.Redirect(ResolveUrl("~/Application/ClaimNewConfirm.aspx?&CorpId=" + corpID + "&UCorpId=" + UCorpId));
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, _UserIdentityModel.PrincipalName, "Error in Continue: " + ex.Message, "ClaimNewDocument");
            }
            finally
            {
                hdnContinue.Value = "0";
            }
        }
    }
}