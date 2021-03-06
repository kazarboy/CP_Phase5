using EPP.CorporatePortal.Common;
using EPP.CorporatePortal.DAL.Entity;
using EPP.CorporatePortal.DAL.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
//using System.Security.Claims;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using static EPP.CorporatePortal.Common.Enums;
using DataTable = System.Data.DataTable;
using EPP.CorporatePortal.Models;
using System.Net;
using System.Configuration;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;
using EPP.CorporatePortal.Helper;

namespace EPP.CorporatePortal.Application
{
    public partial class MemberListing : System.Web.UI.Page
    {
        BaseService db = new BaseService();
        private AuditTrail auditTrailService = new AuditTrail();
        public string CorporateId { get; set; }
        public string UCorporateId { get; set; }
        public string DLTemplate { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var corpId = Request.QueryString["CorpId"] ?? "0";
            var UCorpId = Request.QueryString["UCorpId"] ?? "0";

            //var user = HttpContext.Current.User as ClaimsPrincipal;
            //var identity = user.Identity as ClaimsIdentity;
            //var userName = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).SingleOrDefault();
            var userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;
            //var bizRegNo = identity.Claims.Where(c => c.Type == "BusinessRegistrationNo").Select(c => c.Value).SingleOrDefault();
            var bizRegNo = ((CorporatePortalSite)this.Master)._UserIdentityModel.BusinessRegistrationNo;
            var parentCorporate = ((CorporatePortalSite)this.Master)._UserIdentityModel.ParentCorporate;

            try
            {
                CorporateId = Utility.EncodeAndDecryptCorpId(corpId);
                UCorporateId = Utility.EncodeAndDecryptCorpId(UCorpId);

                if (corpId == null && UCorpId == null)
                {
                    var service = new CorporateService();
                    CorporateId = service.GetParentId(bizRegNo);

                    var storedProcServ = new StoredProcService(userName);
                    var uid = storedProcServ.GetCorporateUId(bizRegNo, parentCorporate);
                    UCorporateId = uid.Rows[0]["Id"].ToString();
                }


                hdnROC.Value = bizRegNo;

                var accessPermission = Rights_Enum.ManageMember;
                HiddenField hdnPermission = (HiddenField)Page.Master.FindControl("hdnPermission");
                hdnPermission.Value = Enum.GetName(typeof(Rights_Enum), accessPermission);

                if (!Page.IsPostBack)
                {
                    LoadPolicyCombo(CorporateId, UCorporateId, out bool PolicyChecker);

                    //Added 20210405
                    hdnPolicyChecker.Value = PolicyChecker.ToString();
                }

                LoadUploadHistory(userName, CorporateId);

                DLTemplate = db.dbEntities.SystemConfigs.Where(x => x.Setting == "DLTemplate").Select(x => x.Value).Single().ToString();

            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Page_Load: " + ex.Message, "MemberListing");
            }
        }
        private string ConstructDirectory(string baseUploadFilesPath, string ROC, string contractNo, string version)
        {
            var directory = Server.MapPath(baseUploadFilesPath + ROC + Path.AltDirectorySeparatorChar + contractNo + Path.AltDirectorySeparatorChar + version + Path.AltDirectorySeparatorChar);
            return directory;
        }
        protected void Upload(object sender, EventArgs e)
        {
            var fileIdentifier = ddlProduct.Value;
            var productCode = fileIdentifier.Split('_')[0];
            var contractNo = fileIdentifier.Split('_')[1];
            var version = fileIdentifier.Split('_')[2];
            var accNo = fileIdentifier.Split('_')[3];
            //var identity = (System.Security.Claims.ClaimsIdentity)Context.User.Identity;
            var recordCount = 0;
            var indicatorContinue = false; //To prevent unnecessary writes to FileUpload table.

            //var author = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).FirstOrDefault();
            var author = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;
            //string ROC = identity.Claims.Where(c => c.Type == "BusinessRegistrationNo").Select(c => c.Value).SingleOrDefault();
            string ROC = ((CorporatePortalSite)this.Master)._UserIdentityModel.BusinessRegistrationNo;

            string baseUploadFilesPath = ConfigurationManager.AppSettings["BaseUploadFilesPath"];
            string directory = ConstructDirectory(baseUploadFilesPath, ROC, contractNo, version);

            try
            {
                var systemEnvironment = CommonService.GetSystemConfigValue("Environment");

                if (ctlFileUpload.HasFile)
                {
                    byte[] uploadedFileBinary;

                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "File Upload: Started for " + ctlFileUpload.PostedFile.FileName, "MemberListing");

                    //Upload and save the file
                    //var localPath = string.Empty;
                    //var CSVFilename = Path.GetFileNameWithoutExtension(ctlFileUpload.PostedFile.FileName) + ".csv";
                    var localPath = directory + Path.GetFileName(ctlFileUpload.PostedFile.FileName);
                    var remoteFileName = ConstructFileName(productCode, contractNo, version, accNo);
                    var remotePath = CommonService.GetSystemConfigValue("DestinationPath") + remoteFileName;
                    var localGenPath = Server.MapPath(baseUploadFilesPath) + remoteFileName;

                    var service = new StoredProcService(author);
                    var fileUploadExist = service.IsFileUploadExist(ctlFileUpload.FileName, ROC, contractNo, Convert.ToInt32(version));
                    if (fileUploadExist)
                    {
                        Utility.RegisterStartupScriptHandling(this, "Error", "alert('File exists in system. Kindly rename file name and upload again');", true, true, author);
                        btnUpload.Enabled = true;
                        return;
                    }

                    if (!Directory.Exists(Server.MapPath(baseUploadFilesPath)))
                    {
                        Directory.CreateDirectory(Server.MapPath(baseUploadFilesPath));
                    }

                    try
                    {
                        FileInfo fi = new FileInfo(ctlFileUpload.PostedFile.FileName);
                        if (fi.Extension != ".xlsx")
                        {
                            Utility.RegisterStartupScriptHandling(this, "Error", "alert('Only .xlsx file allowed');", true, true, author);
                            btnUpload.Enabled = true;
                            return;
                        }

                        uploadedFileBinary = ctlFileUpload.FileBytes;

                        //save in local first if not Production environment
                        if (systemEnvironment != "Production")
                        {
                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }
                            ctlFileUpload.SaveAs(localPath);
                        }

                    }
                    catch (Exception ex)
                    {
                        //keep on showing the popup and show the error
                        UploadLogging("Error: Error in uploading file", "Error in file Upload Control: " + ex.Message, author, true);
                        btnUpload.Enabled = true;
                        return;
                    }

                    //Check and cater for rows with possible script injection characters
                    uploadedFileBinary = Utility.FileBinaryCheckInjectionChar(uploadedFileBinary, author, out bool injectionCheckResult);
                    if (!injectionCheckResult)
                    {
                        File.Delete(localPath);
                        Utility.RegisterStartupScriptHandling(this, "Error", "alert('Error reading excel data');", true, true, author);
                        btnUpload.Enabled = true;
                        return;
                    }

                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "File Upload: Get Excel Data", "MemberListing");
                    var originalData = GetOriginalByteDataOpenXML(uploadedFileBinary, author, out bool oriExcelResults); //Reads binary instead
                    if (!oriExcelResults)
                    {
                        File.Delete(localPath);
                        Utility.RegisterStartupScriptHandling(this, "Error", "alert('Error reading excel data');", true, true, author);
                        btnUpload.Enabled = true;
                        return;
                    }

                    var maxHeaderCount = Convert.ToInt32(CommonService.GetSystemConfigValue("MaxHeaderCount"));
                    if (originalData.Columns.Count > maxHeaderCount)
                    {
                        File.Delete(localPath);
                        Utility.RegisterStartupScriptHandling(this, "Error", "alert('Number of column in upload file should not exceed " + maxHeaderCount + "');", true, true, author);
                        btnUpload.Enabled = true;
                        return;
                    }

                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "File Upload: Replace Header Data", "MemberListing");
                    var mandatoryHeadersTable = GetFileUploadMandatoryColumnsTable(author, productCode);
                    Dictionary<string, string> mandatoryColumnPairs = new Dictionary<string, string>();
                    var rpaTable = PopulateRPAExcelData(originalData, productCode, author, out bool populateRPAResults, ref mandatoryColumnPairs);
                    //Check and adds any missing headers
                    var checkHeaderColumns = CheckColumnsExistInHeaders(mandatoryHeadersTable, ref rpaTable);
                    if (!checkHeaderColumns)
                    {
                        File.Delete(localPath);
                        Utility.RegisterStartupScriptHandling(this, "Error", "alert('No mandatory headers found');", true, true, author);
                        btnUpload.Enabled = true;
                        return;
                    }

                    try
                    {
                        //Checks if any duplicated headers exists. using mandatoryColumnPairs
                        var checkMandatoryColumnsDupe = mandatoryColumnPairs.GroupBy(x => x.Value).Where(x => x.Count() > 1).SelectMany(i => i).ToDictionary(t => t.Key, t => t.Value);
                        if (checkMandatoryColumnsDupe.Any())
                        {
                            File.Delete(localPath);
                            var alertMessage = String.Format("alert('Duplicated mandatory headers: {0}{2}{1}');", checkMandatoryColumnsDupe.Count, string.Join(", ", checkMandatoryColumnsDupe.Keys.ToArray()), @"\n");
                            Utility.RegisterStartupScriptHandling(this, "Error", alertMessage, true, true, author);
                            btnUpload.Enabled = true;
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        File.Delete(localPath);
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, author, "Error in checkMandatoryColumnsDupe: " + ex.Message, "MemberListing");
                        throw;
                    }

                    try
                    {
                        //Checks if any duplicated NRIC ICNo
                        //Only checks if the mapped column exists
                        if (rpaTable.Columns.Contains("*Member/Dependant NRIC"))
                        {
                            var icDupeCount = rpaTable.AsEnumerable().Where(w1 => !string.IsNullOrEmpty(w1["*Member/Dependant NRIC"].ToString())).Select(s => s["*Member/Dependant NRIC"]).GroupBy(g => g).Where(w2 => w2.Count() > 1).Select(y => y.Key).ToList();
                            if (icDupeCount.Any())
                            {
                                File.Delete(localPath);
                                var alertMessage = String.Format("alert('Duplicated Member/Dependant NRIC: {0}{2}{1}');", icDupeCount.Count, string.Join(", ", icDupeCount), @"\n");
                                Utility.RegisterStartupScriptHandling(this, "Error", alertMessage, true, true, author);
                                btnUpload.Enabled = true;
                                return;
                            }
                        }

                        //Checks if any duplicated ODIC ICNo
                        //Only checks if the mapped column exists
                        if (rpaTable.Columns.Contains("*Member/Dependant ODIC"))
                        {
                            var icDupeCount = rpaTable.AsEnumerable().Where(w1 => !string.IsNullOrEmpty(w1["*Member/Dependant ODIC"].ToString())).Select(s => s["*Member/Dependant ODIC"]).GroupBy(g => g).Where(w2 => w2.Count() > 1).Select(y => y.Key).ToList();
                            if (icDupeCount.Any())
                            {
                                File.Delete(localPath);
                                var alertMessage = String.Format("alert('Duplicated Member/Dependant ODIC: {0}{2}{1}');", icDupeCount.Count, string.Join(", ", icDupeCount), @"\n");
                                Utility.RegisterStartupScriptHandling(this, "Error", alertMessage, true, true, author);
                                btnUpload.Enabled = true;
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        File.Delete(localPath);
                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, author, "Error in checkICNoDupe: " + ex.Message, "MemberListing");
                        throw;
                    }

                    //try
                    //{
                    //    //Checks if there is any null value, if yes then prompt error
                    //    if (rpaTable.Columns.Contains("*Relationship With Member"))
                    //    {
                    //        var NoCount = rpaTable.AsEnumerable().Where(w1 => !string.IsNullOrEmpty(w1["*Relationship With Member"].ToString())).Select(s => s["*Relationship With Member"]).ToList();

                    //        var NullCount = rpaTable.AsEnumerable().Where(w1 => string.IsNullOrEmpty(w1["*Relationship With Member"].ToString())).Select(s => s["*Relationship With Member"]).ToList();
                    //        if (NullCount.Any())
                    //        {
                    //            File.Delete(localPath);
                    //            var alertMessage = String.Format("alert('*MANDATORY FIELD RELATIONSHIP WITH MEMBER: {0}{2}{1}');", NullCount.Count, string.Join(", ", NullCount), @"\n");
                    //            Utility.RegisterStartupScriptHandling(this, "Error", alertMessage, true, true, author);
                    //            btnUpload.Enabled = true;
                    //            return;
                    //        }
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    File.Delete(localPath);
                    //    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, author, "Error in checkRelationshipMandatory: " + ex.Message, "MemberListing");
                    //    throw;
                    //}

                    try
                    {
                        //Get Record Counts of data given to RPA. To be saved in database and used later for validation.
                        recordCount = rpaTable.AsEnumerable().Where(w => !string.IsNullOrEmpty(w["*Member/Dependant NRIC"].ToString()) || !string.IsNullOrEmpty(w["*Member/Dependant ODIC"].ToString())).Select(s => !string.IsNullOrEmpty(s["*Member/Dependant NRIC"].ToString()) ? s["*Member/Dependant NRIC"] : s["*Member/Dependant ODIC"]).Distinct().Count();

                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "File Upload: Export to csv file format", "MemberListing");

                        var remoteFileBinary = ExportDataSetToBytes(rpaTable, mandatoryHeadersTable);

                        if (systemEnvironment != "Production") //Keep a physical copy if not Production
                        {
                            using (var fileStream = File.Create(localGenPath))
                            {
                                Stream stream = new MemoryStream(remoteFileBinary);
                                stream.Seek(0, SeekOrigin.Begin);
                                stream.CopyTo(fileStream);
                            }
                        }

                        auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "File Upload: Copy csv to remote " + remotePath, "MemberListing");

                        Utility.SendRemoteFileBytes(remoteFileBinary, remotePath, author, out bool resultSendRemote);

                        if (!resultSendRemote)
                        {
                            File.Delete(localPath);
                            File.Delete(localGenPath);

                            //keep on showing the popup and show the error
                            UploadLogging("Error: Error in uploading file to network", "Error in sending to RPA folder", author, true);
                            btnUpload.Enabled = true;
                            return;
                        }
                        else
                        {
                            //convert from xlsx binary to csv binary
                            //uploadedFileBinary = ConvertToCSV(uploadedFileBinary, author, directory, CSVFilename);

                            indicatorContinue = true;
                        }

                        if (indicatorContinue) //Only insert if successfully sent file to remote server.
                        {
                            var fileUploadId = service.InsFileUpload(FileUploadType.MemberListing.ToString(), CorporateId, contractNo, ctlFileUpload.PostedFile.FileName, DateTime.Now, author, remoteFileName, localPath, remotePath, Convert.ToInt32(version), recordCount);
                            //change filename from xlsx to csv
                            //var fileUploadId = service.InsFileUpload(FileUploadType.MemberListing.ToString(), CorporateId, contractNo, CSVFilename, DateTime.Now, author, remoteFileName, localPath, remotePath, Convert.ToInt32(version), recordCount);

                            InsertFileUploadFallout(Convert.ToInt32(fileUploadId), "", author);

                            service.UpdateFileUploadBinary(Convert.ToInt32(fileUploadId), EncryptionHelper.AESEncryptByteData(uploadedFileBinary), Common.Enums.FileDownloadType.UploadFile.ToString());
                            service.UpdateFileUploadBinary(Convert.ToInt32(fileUploadId), EncryptionHelper.AESEncryptByteData(remoteFileBinary), Common.Enums.FileDownloadType.GeneratedFile.ToString());
                        } 
                    }
                    catch (Exception ex)
                    {
                        File.Delete(localPath);
                        File.Delete(localGenPath);

                        //keep on showing the popup and show the error
                        UploadLogging("Error: Error in uploading file", "Error in file Upload Excel generation: " + ex.Message, author, true);
                        btnUpload.Enabled = true;
                        return;
                    }
                }
                else
                {
                    //keep on showing the popup and show the error
                    UploadLogging("Error: No File Selected", "", author, false);
                    btnUpload.Enabled = true;
                    return;
                }

                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "File Upload: Reload upload history", "MemberListing");
                LoadUploadHistory(author, CorporateId);//Reload the history
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "File Upload: Finished", "MemberListing");
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, author, "File uploaded: " + ctlFileUpload.PostedFile.FileName, "MemberListing");
                Utility.RegisterStartupScriptHandling(this, "Success", "alert('File uploaded successfully');", true, true, author);
            }
            catch (Exception ex)
            {
                //In case got any uncaptured errors.
                UploadLogging("Error: Error in uploading file", "Error in file File Upload function: " + ex.Message, author, true);
                btnUpload.Enabled = true;
            }
            finally
            {
                //To cater for all cases to re-enable the upload button.
                btnUpload.Enabled = true;
            }
        }

        private void UploadLogging(string userMsg, string errMsg, string userName, bool auditLogging)
        {
            if (!string.IsNullOrEmpty(userMsg))
                ScriptManager.RegisterStartupScript(this, this.GetType(), "Error", "$('#update-member').modal('toggle');$('.uploadlogo').parent().children('span').html('" + userMsg + "');", true);

            if (!string.IsNullOrEmpty(errMsg) && auditLogging)
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, errMsg, "MemberListing");
        }

        private void LoadUploadHistory(string userName, string corpId)
        {
            //var user = HttpContext.Current.User as ClaimsPrincipal;
            //var identity = user.Identity as ClaimsIdentity;
            //var bizRegNo = identity.Claims.Where(c => c.Type == "BusinessRegistrationNo").Select(c => c.Value).SingleOrDefault();
            var bizRegNo = ((CorporatePortalSite)this.Master)._UserIdentityModel.BusinessRegistrationNo;

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadUploadHistory: Started", "MemberListing");

            var service = new StoredProcService(userName);

            //Directory Paths
            //Remote Paths
            var remoteRPAExceptionPath = CommonService.GetSystemConfigValue("DestinationPathRPA");
            var remoteCGLSExceptionPath = CommonService.GetSystemConfigValue("DestinationPathCGLS");
            var remoteInvoicePath = CommonService.GetSystemConfigValue("DestinationPathInv");
            //Local App Paths
            var localRPAExceptionPath = Server.MapPath(ConfigurationManager.AppSettings["BaseDownloadRPAExceptionPath"]);
            var localCGLSExceptionPath = Server.MapPath(ConfigurationManager.AppSettings["BaseDownloadCGLSExceptionPath"]);
            var localInvoicePath = Server.MapPath(ConfigurationManager.AppSettings["BaseDownloadInvoicePath"]);

            var systemEnvironment = CommonService.GetSystemConfigValue("Environment");

            //if (systemEnvironment != "Production")
            //{
            //    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "CopyRemoteFile: Started", "MemberListing");
            //    //Copy files from remote paths any new files
            //    Utility.CopyRemoteFiles(localRPAExceptionPath, remoteRPAExceptionPath, userName, "*_error*.*"); // _error1 and _error2. To be copied 1st since same extension as CGLS's
            //    Utility.CopyRemoteFiles(localCGLSExceptionPath, remoteCGLSExceptionPath, userName, "*.xlsx"); // .xlsx
            //    Utility.CopyRemoteFiles(localInvoicePath, remoteInvoicePath, userName, "*.pdf"); // .pdf
            //    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "CopyRemoteFile: Finished", "MemberListing");
            //}

            //load menu
            var decryptedCorpId = Utility.EncodeAndDecryptCorpId(corpId);
            DataTable dt = service.GetFileUpload(decryptedCorpId);

            //if (dt.Rows.Count > 0)
            //{
            //    //Filter FileUpload lists to only those user has access to. PIC/Owner only
            //    var policies = CommonEntities.LoadPolicies(bizRegNo, ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName, ((CorporatePortalSite)this.Master)._UserIdentityModel.IsOwner);
            //    var policyList = policies.AsEnumerable().Select(s => s["ContractNo"].ToString()).Distinct().ToList();
            //    var dtFiltered = dt.AsEnumerable().Where(w => policyList.Contains(w["PolicySourceId"].ToString()));

            //    if (dtFiltered.Any())
            //        dt = dtFiltered.CopyToDataTable();
            //    else
            //        dt = new DataTable();
            //}

            //Update UploadHistory data with new ExceptionLink column
            dt = UpdateUploadHistoryLinks(dt, remoteRPAExceptionPath, remoteCGLSExceptionPath, remoteInvoicePath, userName);

            rptFileUploadHistory.DataSource = dt;
            rptFileUploadHistory.DataBind();

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadUploadHistory: Finished", "MemberListing");
        }

        private DataTable UpdateUploadHistoryLinks(DataTable uploadHistory, string remoteRPAExceptionPath, string remoteCGLSExceptionPath, string remoteInvoicePath, string userName)
        {
            var service = new StoredProcService(userName);
            var updatedUploadHist = new DataTable();
            string tmpFileName = "", tmpRPAPath = "", tmpRPAPath2 = "", tmpCGLSPath = "", tmpInvoicePath = "";

            try
            {
                updatedUploadHist = uploadHistory;

                foreach (DataRow dr in updatedUploadHist.Rows)
                {
                    DataTable rpaData = new DataTable();
                    DataTable rpa2Data = new DataTable();
                    DataTable rpaFinalData = new DataTable();
                    DataTable rpaCombinedData = new DataTable();
                    DataTable cglsData = new DataTable();
                    DataTable exceptionCombinedData = new DataTable();

                    //General FileName and Path
                    tmpFileName = Path.GetFileName(dr["MappedFileName"].ToString());

                    //Only start checking if status is "Processing"
                    if (dr["Status"].ToString() == "Processing")
                    {
                        try
                        {
                            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Processing Remote files for File Upload ID: " + dr["Id"].ToString(), "MemberListing");

                            //Check both RPA, CGLS exception and Invoice files
                            List<string> listRPAPath = new List<string>();
                            List<string> listRPA2Path = new List<string>();
                            List<string> listCGLSPath = new List<string>();
                            List<string> listInvoicePath = new List<string>();
                            var pathRPACheck = remoteRPAExceptionPath + Path.GetFileNameWithoutExtension(tmpFileName) + Path.DirectorySeparatorChar;
                            var pathRPA2Check = remoteRPAExceptionPath + Path.GetFileNameWithoutExtension(tmpFileName) + Path.DirectorySeparatorChar;
                            var pathCGLSCheck = remoteCGLSExceptionPath + Path.GetFileNameWithoutExtension(tmpFileName) + Path.DirectorySeparatorChar;
                            var pathInvoiceCheck = remoteInvoicePath + Path.GetFileNameWithoutExtension(tmpFileName) + Path.DirectorySeparatorChar;
                            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Checking RPA1 Folder Path: " + pathRPACheck, "MemberListing");
                            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Checking RPA2 Folder Path: " + pathRPA2Check, "MemberListing");
                            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Checking CGLS Folder Path: " + pathCGLSCheck, "MemberListing");
                            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Checking Invoice Folder Path: " + pathInvoiceCheck, "MemberListing");
                            var rpaExists = Utility.CheckFileExistsUseWildcard(pathRPACheck, Path.GetFileNameWithoutExtension(tmpFileName) + "_error1.*", ref listRPAPath, true);
                            tmpRPAPath = rpaExists ? listRPAPath.FirstOrDefault() : tmpRPAPath; //Should be only 1 file, so only get the 1st in the list.
                            var rpa2Exists = Utility.CheckFileExistsUseWildcard(pathRPA2Check, Path.GetFileNameWithoutExtension(tmpFileName) + "_error2.*", ref listRPA2Path, true);
                            tmpRPAPath2 = rpa2Exists ? listRPA2Path.FirstOrDefault() : tmpRPAPath2; //Should be only 1 file, so only get the 1st in the list.
                            var cglsExists = Utility.CheckFileExistsUseWildcard(pathCGLSCheck, Path.GetFileNameWithoutExtension(tmpFileName) + ".xlsx", ref listCGLSPath, true);
                            tmpCGLSPath = cglsExists ? listCGLSPath.FirstOrDefault() : tmpCGLSPath; //Should be only 1 file, so only get the 1st in the list.                        
                            var invoiceExists = Utility.CheckFileExistsUseWildcard(pathInvoiceCheck, Path.GetFileNameWithoutExtension(tmpFileName) + ".pdf", ref listInvoicePath, true);
                            tmpInvoicePath = invoiceExists ? listInvoicePath.FirstOrDefault() : tmpInvoicePath; //Should be only 1 file, so only get the 1st in the list.

                            var totalOriCount = dr["RecordCount"] == DBNull.Value ? 0 : Convert.ToInt32(dr["RecordCount"]);
                            var totalRPACount = 0;
                            var totalCGLSCount = 0;

                            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Checking RPA1 File Path: " + tmpRPAPath, "MemberListing");
                            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Checking RPA2 File Path: " + tmpRPAPath2, "MemberListing");
                            //If RPA file exists
                            if (rpaExists || rpa2Exists)
                            {
                                //RPA1 processing
                                if (rpaExists)
                                {
                                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "RPA1 File exists: " + tmpRPAPath, "MemberListing");
                                    var fileBytes = Utility.CopyRemoteFileBytes(tmpRPAPath, userName);
                                    var extension = Path.GetExtension(tmpRPAPath);
                                    if (extension == ".csv")
                                        rpaData = GetOriginalCSVByteData(fileBytes, userName);
                                    else //.xls or .xlsx
                                        rpaData = GetOriginalByteDataOpenXML(fileBytes, userName);
                                }

                                //RPA2 processing
                                if (rpa2Exists)
                                {
                                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "RPA2 File exists: " + tmpRPAPath2, "MemberListing");
                                    var fileBytes = Utility.CopyRemoteFileBytes(tmpRPAPath2, userName);
                                    var extension = Path.GetExtension(tmpRPAPath2);
                                    if (extension == ".csv")
                                        rpa2Data = GetOriginalCSVByteData(fileBytes, userName);
                                    else //.xls or .xlsx
                                        rpa2Data = GetOriginalByteDataOpenXML(fileBytes, userName);
                                }

                                //To combine both RPA exception files if both exists
                                if (rpaExists && rpa2Exists)
                                {
                                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Combining RPA1 & RPA2 file", "MemberListing");
                                    rpaCombinedData = CombineDataTableFile(rpaData, rpa2Data, userName);
                                }

                                //In case only 1 file exists. Priority list: Combined > rpa2 > rpa1
                                rpaFinalData = rpaCombinedData.Rows.Count > 0 ? rpaCombinedData : rpa2Data.Rows.Count > 0 ? rpa2Data : rpaData;

                                totalRPACount = rpaFinalData.AsEnumerable().Where(w => !string.IsNullOrEmpty(w["*Member/Dependant NRIC"].ToString()) || !string.IsNullOrEmpty(w["*Member/Dependant ODIC"].ToString())).Select(s => !string.IsNullOrEmpty(s["*Member/Dependant NRIC"].ToString()) ? s["*Member/Dependant NRIC"] : s["*Member/Dependant ODIC"]).Distinct().Count();

                                //Check Count in file2
                                if (rpa2Exists)
                                {
                                    totalRPACount += rpaFinalData.AsEnumerable().Where(w => !string.IsNullOrEmpty(w["Member/Dependant NRIC"].ToString()) || !string.IsNullOrEmpty(w["Member/Dependant ODIC"].ToString())).Select(s => !string.IsNullOrEmpty(s["Member/Dependant NRIC"].ToString()) ? s["Member/Dependant NRIC"] : s["Member/Dependant ODIC"]).Distinct().Count();
                                }

                                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Total RPA Count: " + totalRPACount, "MemberListing");

                                //If all RPA records are errors.
                                if (totalOriCount == totalRPACount)
                                {
                                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "RPA Count same as Total Count", "MemberListing");

                                    //Save the binary data
                                    var fileBinary = ExportDataSetToBytes(rpaFinalData);
                                    var encFileBinary = EncryptionHelper.AESEncryptByteData(fileBinary);
                                    service.UpdateFileUploadBinary(Convert.ToInt32(dr["Id"].ToString()), encFileBinary, Common.Enums.FileDownloadType.Exception.ToString());
                                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "File Binary saved: " + Common.Enums.FileDownloadType.Exception.ToString(), "MemberListing");

                                    dr["ExceptionFile"] = encFileBinary;

                                    //Update Status to Completed.
                                    dr["Status"] = "Completed";
                                    SetProcessCompleted(dr, userName);

                                    //Delete all files since successful.
                                    if (!string.IsNullOrEmpty(tmpInvoicePath))
                                        Utility.DeleteRemoteFolder(Path.GetDirectoryName(tmpInvoicePath), userName);
                                    if (!string.IsNullOrEmpty(tmpRPAPath))
                                        Utility.DeleteRemoteFolder(Path.GetDirectoryName(tmpRPAPath), userName);
                                    if (!string.IsNullOrEmpty(tmpCGLSPath))
                                        Utility.DeleteRemoteFolder(Path.GetDirectoryName(tmpCGLSPath), userName);

                                    //Continue to next file checking in the loop
                                    continue;
                                }
                            }
                            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Checking CGLS File Path: " + tmpCGLSPath, "MemberListing");
                            //If CGLS file exists
                            if (cglsExists)
                            {
                                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "CGLS File exists: " + tmpCGLSPath, "MemberListing");
                                var fileBytes = Utility.CopyRemoteFileBytes(tmpCGLSPath, userName);
                                var extension = Path.GetExtension(tmpCGLSPath);
                                if (extension == ".csv")
                                    cglsData = GetOriginalCSVByteData(fileBytes, userName);
                                else //.xls or .xlsx
                                    cglsData = GetOriginalByteDataOpenXML(fileBytes, userName);

                                totalCGLSCount = cglsData.AsEnumerable().Select(s => s["Key Indicator"]).Distinct().Count();

                                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Total CGLS Count: " + totalCGLSCount, "MemberListing");
                            }
                            //If both RPA & CGLS exception file exists, to combine both
                            if ((rpaExists || rpa2Exists) && cglsExists)
                            {
                                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Combining RPA & CGLS file", "MemberListing");
                                exceptionCombinedData = CombineExceptionFile(rpaFinalData, cglsData, userName);
                            }
                            //In case only 1 exception file exists. Priority list: Combined > cgls > rpa
                            DataTable exceptionFinalData = exceptionCombinedData.Rows.Count > 0 ? exceptionCombinedData : cglsData.Rows.Count > 0 ? cglsData : rpaFinalData;
                            //If all RPA + CGLS records are errors. Waits until all RPA & CGLS processing done before checking count.
                            if (totalOriCount == (totalCGLSCount + totalRPACount))
                            {
                                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "RPA + CGLS Count same as Total Count", "MemberListing");

                                //Save the binary data
                                var fileBinary = ExportDataSetToBytes(exceptionFinalData);
                                var encFileBinary = EncryptionHelper.AESEncryptByteData(fileBinary);
                                service.UpdateFileUploadBinary(Convert.ToInt32(dr["Id"].ToString()), encFileBinary, Common.Enums.FileDownloadType.Exception.ToString());
                                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "File Binary saved: " + Common.Enums.FileDownloadType.Exception.ToString(), "MemberListing");

                                dr["ExceptionFile"] = encFileBinary;

                                //Update Status to Completed.
                                dr["Status"] = "Completed";
                                SetProcessCompleted(dr, userName);

                                //Delete all files since successful.
                                if (!string.IsNullOrEmpty(tmpInvoicePath))
                                    Utility.DeleteRemoteFolder(Path.GetDirectoryName(tmpInvoicePath), userName);
                                if (!string.IsNullOrEmpty(tmpRPAPath))
                                    Utility.DeleteRemoteFolder(Path.GetDirectoryName(tmpRPAPath), userName);
                                if (!string.IsNullOrEmpty(tmpCGLSPath))
                                    Utility.DeleteRemoteFolder(Path.GetDirectoryName(tmpCGLSPath), userName);

                                //Continue to next file checking in the loop
                                continue;
                            }
                            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Checking Invoice File Path: " + tmpInvoicePath, "MemberListing");
                            //If Invoice file exists
                            if (invoiceExists)
                            {
                                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Invoice File exists: " + tmpInvoicePath, "MemberListing");
                                var fileBytesInvoice = Utility.CopyRemoteFileBytes(tmpInvoicePath, userName);
                                var encBytesInvoice = EncryptionHelper.AESEncryptByteData(fileBytesInvoice);
                                //Save the binary data
                                service.UpdateFileUploadBinary(Convert.ToInt32(dr["Id"].ToString()), encBytesInvoice, Common.Enums.FileDownloadType.Invoice.ToString());
                                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "File Binary saved: " + Common.Enums.FileDownloadType.Invoice.ToString(), "MemberListing");

                                dr["InvoiceFile"] = encBytesInvoice;

                                var fileBytesException = ExportDataSetToBytes(exceptionFinalData);
                                var encBytesException = EncryptionHelper.AESEncryptByteData(fileBytesException);
                                //Save the binary data
                                service.UpdateFileUploadBinary(Convert.ToInt32(dr["Id"].ToString()), encBytesException, Common.Enums.FileDownloadType.Exception.ToString());
                                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "File Binary saved: " + Common.Enums.FileDownloadType.Exception.ToString(), "MemberListing");

                                dr["ExceptionFile"] = encBytesException;

                                //Update Status to Completed.
                                dr["Status"] = "Completed";
                                SetProcessCompleted(dr, userName);

                                //Delete all files since successful.
                                if (!string.IsNullOrEmpty(tmpInvoicePath))
                                    Utility.DeleteRemoteFolder(Path.GetDirectoryName(tmpInvoicePath), userName);
                                if (!string.IsNullOrEmpty(tmpRPAPath))
                                    Utility.DeleteRemoteFolder(Path.GetDirectoryName(tmpRPAPath), userName);
                                if (!string.IsNullOrEmpty(tmpCGLSPath))
                                    Utility.DeleteRemoteFolder(Path.GetDirectoryName(tmpCGLSPath), userName);
                            }
                        }
                        catch (Exception ex)
                        {
                            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in processing fileID [" + dr["Id"].ToString() + "] with error: " + ex.Message, "MemberListing");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in UpdateUploadHistoryLinks: " + ex.Message, "MemberListing");
            }

            return updatedUploadHist;
        }

        private void SetProcessCompleted(DataRow dr, string userName)
        {
            var service = new StoredProcService(userName);

            //Update all links to database.
            service.UpdateFileUploadLinks(Convert.ToInt32(dr["Id"]), dr["RPAExceptionLink"].ToString(), dr["CGLSExceptionLink"].ToString(), dr["InvoiceLink"].ToString(), dr["ExceptionLink"].ToString());

            //Update Status to Completed.
            service.UpdateFileUploadStatusByID(Convert.ToInt32(dr["Id"]), "Completed");

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Set Process Completed", "MemberListing");
        }

        private DataTable CombineExceptionFile(DataTable rpaData, DataTable cglsData, string userName)
        {
            DataTable returnDataTable = new DataTable();
            try
            {
                foreach (DataRow dr in cglsData.Rows)
                {
                    DataRow newDataRow = rpaData.NewRow();

                    newDataRow["*MEMBER/DEPENDANT NRIC"] = dr["Key Indicator"];
                    newDataRow[newDataRow.ItemArray.Length - 1] = dr["Message"]; //Error Message will always be the last column


                    rpaData.Rows.Add(newDataRow);//this will add the row at the end of the datatable                    
                }
                returnDataTable = rpaData;
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in CombineExceptionFile: " + ex.Message, "MemberListing");
            }
            return returnDataTable;
        }

        private DataTable CombineDataTableFile(DataTable dt1, DataTable dt2, string userName)
        {
            DataTable returnDataTable = new DataTable();
            try
            {

                dt1.Merge(dt2);

                returnDataTable = dt1;
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in CombineDataTableFile: " + ex.Message, "MemberListing");
            }
            return returnDataTable;
        }

        private void LoadPolicyCombo(string corporateId,string CorpId, out bool PolicyChecker)
        {
            PolicyChecker = false;

            //var identity = (System.Security.Claims.ClaimsIdentity)Context.User.Identity;
            DataTable policies = CommonEntities.LoadPolicies(corporateId, ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName, ((CorporatePortalSite)this.Master)._UserIdentityModel.IsOwner, CorpId);
            policies.Columns.Add("CustomField");
            foreach (DataRow row in policies.Rows)
            {
                row["CustomField"] = row["ProductCode"].ToString() + "_" + row["ContractNo"] + "_" + row["Version"] + "_" + row["AccountNo"];

                //Added 20210405
                if (row["ProductCode"].ToString() == "OPC" || row["ProductCode"].ToString() == "OPW")
                {
                    PolicyChecker = true;
                }
            }

            ddlProduct.DataValueField = "CustomField";
            ddlProduct.DataTextField = "ProductName";
            ddlProduct.DataSource = policies;
            ddlProduct.DataBind();

        }

        protected string GetStatus(object dataItem)
        {
            var retValue = String.Empty;
            string value = DataBinder.Eval(dataItem, "Status").ToString();
            string Id = DataBinder.Eval(dataItem, "Id").ToString();
            if (value == "True")
            {
                retValue = "Sucessful";
            }
            else
            {
                retValue = "Failed. See " + GenerateFalloutLink(Id);
            }
            return retValue;


        }

        protected string GenerateFalloutLink(string Id)
        {
            var retValue = String.Empty;

            retValue = "<a class='falloutError' href='~/Application/DisplayFallout.aspx?FileUploadID=" + Id + "'>Fallout</a>";

            return retValue;
        }


        private void InsertFileUploadFallout(int fileUploadId, string fallout, string userName)
        {
            new StoredProcService(userName).InsFileUploadFallout(fileUploadId, fallout);
        }
        public DataTable GetFileUploadMandatoryColumnsTable(string userName, string product)
        {
            var dt = new StoredProcService(userName).GetFileUploadMandatoryColumn(product);
            return dt;
        }
        public System.Data.DataTable GetFileUploadMappingColumnsTable(string userName)
        {
            var dt = new StoredProcService(userName).GetFileUploadMappingColumns();
            return dt;
        }
        public bool CheckColumnsExistInHeaders(DataTable mandatoryColumnsTable, ref DataTable inputTable)
        {
            bool returnValue = true; //Defaulted. Since only false if mandatory columns exists but not available in headers.
            if (mandatoryColumnsTable != null && mandatoryColumnsTable.Rows.Count > 0) //Only check if mandatory header data exists.
            {
                var mandatoryList = new List<string>();

                foreach (DataRow dr in mandatoryColumnsTable.Rows)
                {
                    var column = dr["MandatoryColumn"].ToString();
                    mandatoryList.Add(column.Trim());
                }

                var inputList = inputTable.Columns.Cast<DataColumn>()
                                 .Select(x => x.ColumnName.Trim())
                                 .ToList();

                //Check if there are no headers available in mandatory header list
                if (!inputList.Any(a => mandatoryList.Any(b => a == b)))
                {
                    returnValue = false;
                    return returnValue;
                }

                //Check difference to add in missing columns
                var exceptionList = mandatoryList.Except(inputList, StringComparer.OrdinalIgnoreCase).ToList();
                if (exceptionList.Count > 0)
                {
                    foreach (var checkColumn in exceptionList)
                    {
                        inputTable.Columns.Add(checkColumn, typeof(String));
                    }
                    returnValue = true;
                }
            }

            return returnValue;
        }

        public DataTable GetOriginalExcelData(string localPath, string userName)
        {
            var dt = GetOriginalExcelData(localPath, userName, out bool results);

            return dt;
        }

        public DataTable GetOriginalExcelData(string localPath, string userName, out bool results)
        {
            results = true;
            var dt = new DataTable();
            try
            {
                string strExcelConn = "Provider=Microsoft.ACE.OLEDB.12.0; Data Source="
            + localPath
            + "; Extended Properties='Excel 12.0;HDR=Yes'";
                OleDbConnection connExcel = new OleDbConnection(strExcelConn);
                OleDbCommand cmdExcel = new OleDbCommand();
                OleDbDataAdapter da = new OleDbDataAdapter();
                cmdExcel.Connection = connExcel;
                connExcel.Open();
                DataTable dtExcelSchema;
                dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                connExcel.Close();
                DataSet ds = new DataSet();
                string SheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                cmdExcel.CommandText = "SELECT * From [" + SheetName + "]";
                da.SelectCommand = cmdExcel;
                da.Fill(ds);
                dt = ds.Tables[0];
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in GetOriginalExcelData: " + ex.Message, "MemberListing");
                results = false;
            }
            return dt;
        }

        public DataTable SpreadSheetDocProcessing(SpreadsheetDocument doc)
        {
            var returnDt = new DataTable();

            var sheets = doc.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
            var id = sheets.First().Id.Value;
            var part = (WorksheetPart)doc.WorkbookPart.GetPartById(id);
            var sheet = part.Worksheet;
            var data = sheet.GetFirstChild<SheetData>();
            var rows = data.Descendants<Row>();
            int columns = -1;

            if (rows.Count() != 0)
            {
                var colCount = rows.First().Cast<Cell>().Count();
                if (columns > colCount || columns <= 0)
                    columns = colCount;

                //Header Column
                foreach (var cell in rows.First().Cast<Cell>().Take(columns))
                    returnDt.Columns.Add(GetValue(cell, doc));

                //Skips 1st row/header. Only reads remaining rows
                foreach (var row in rows.Skip(Convert.ToInt32(true)))
                {
                    DataRow tempRow = returnDt.NewRow();

                    for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                    {
                        Cell cell = row.Descendants<Cell>().ElementAt(i);
                        int? actualCellIndex = CellReferenceToIndex(cell); //New function to get the real index of the cell in case previous cell was empty.
                        if (actualCellIndex != null)
                            tempRow[Convert.ToInt32(actualCellIndex)] = GetValue(cell, doc);
                    }

                    returnDt.Rows.Add(tempRow);
                }
            }

            return returnDt;
        }

        public DataTable GetOriginalByteDataOpenXML(byte[] fileBytes, string userName)
        {
            var dt = GetOriginalByteDataOpenXML(fileBytes, userName, out bool results);

            return dt;
        }

        public DataTable GetOriginalByteDataOpenXML(byte[] fileBytes, string userName, out bool results)
        {
            results = true;
            var dt = new DataTable();
            try
            {
                Stream stream = new MemoryStream(fileBytes);
                //Open the Excel file in Read Mode using OpenXml.
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, false))
                {
                    dt = SpreadSheetDocProcessing(doc);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in GetOriginalExcelDataOpenXML: " + ex.Message, "MemberListing");
                results = false;
            }
            return dt;
        }

        public DataTable GetOriginalExcelDataOpenXML(string localPath, string userName)
        {
            var dt = GetOriginalExcelDataOpenXML(localPath, userName, out bool results);

            return dt;
        }

        public DataTable GetOriginalExcelDataOpenXML(string localPath, string userName, out bool results)
        {
            results = true;
            var dt = new DataTable();
            try
            {
                //Open the Excel file in Read Mode using OpenXml.
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(localPath, false))
                {
                    dt = SpreadSheetDocProcessing(doc);
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in GetOriginalExcelDataOpenXML: " + ex.Message, "MemberListing");
                results = false;
            }
            return dt;
        }
        private int? CellReferenceToIndex(Cell cell)
        {
            string reference = cell.CellReference.ToString().ToUpper();
            if (string.IsNullOrEmpty(reference))
            {
                return null;
            }

            //remove digits
            string columnReference = Regex.Replace(reference.ToUpper(), @"[\d]", string.Empty);

            int columnNumber = -1;
            int mulitplier = 1;

            //working from the end of the letters take the ASCII code less 64 (so A = 1, B =2...etc)
            //then multiply that number by our multiplier (which starts at 1)
            //multiply our multiplier by 26 as there are 26 letters
            foreach (char c in columnReference.ToCharArray().Reverse())
            {
                columnNumber += mulitplier * ((int)c - 64);

                mulitplier *= 26;
            }

            //this will match Excel's column index
            return columnNumber;
        }
        private string GetValue(Cell cell, SpreadsheetDocument document)
        {
            string result = string.Empty;
            try
            {
                if (cell != null && cell.ChildElements.Count != 0)
                {
                    var part = document.WorkbookPart.SharedStringTablePart;
                    if (cell.DataType != null && cell.DataType == CellValues.SharedString)
                        result = part.SharedStringTable.ChildElements[Int32.Parse(cell.CellValue.InnerText)].InnerText;
                    else
                        result = cell.CellValue.InnerText;
                }
            }
            catch (Exception)
            {
                result = string.Empty;
            }
            return result;
        }

        public DataTable GetOriginalCSVData(string localPath, string userName)
        {
            DataTable dt = new DataTable();
            try
            {
                using (StreamReader sr = new StreamReader(localPath))
                {
                    string[] headers = sr.ReadLine().Split(',');
                    foreach (string header in headers)
                    {
                        dt.Columns.Add(header);
                    }
                    while (!sr.EndOfStream)
                    {
                        string[] rows = sr.ReadLine().Split(',');
                        DataRow dr = dt.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            dr[i] = rows[i];
                        }
                        dt.Rows.Add(dr);
                    }

                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in GetOriginalCSVData: " + ex.Message, "MemberListing");
            }
            return dt;
        }

        public DataTable GetOriginalCSVByteData(byte[] fileBytes, string userName)
        {
            DataTable dt = new DataTable();
            try
            {
                MemoryStream stream = new MemoryStream(fileBytes);
                using (StreamReader sr = new StreamReader(stream))
                {
                    string[] headers = sr.ReadLine().Split(',');
                    foreach (string header in headers)
                    {
                        dt.Columns.Add(header);
                    }
                    while (!sr.EndOfStream)
                    {
                        string[] rows = sr.ReadLine().Split(',');
                        DataRow dr = dt.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            dr[i] = rows[i];
                        }
                        dt.Rows.Add(dr);
                    }

                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in GetOriginalCSVByteData: " + ex.Message, "MemberListing");
            }
            return dt;
        }

        public DataTable PopulateRPAExcelData(DataTable originalExcelData, string product, string userName, out bool results, ref Dictionary<string, string> mandatoryColumnsPairs)
        {
            DataTable rpaExcelData = new DataTable();
            results = false;//Defaulted to false in case no mappedcolumns exists.

            try
            {
                DataTable rpaMappingTable = GetFileUploadMappingColumnsTable(userName);
                var rpaMappingList = (from DataRow dr in rpaMappingTable.Rows
                                      select new RPAMappingClass()
                                      {
                                          Value = dr["Value"].ToString(),
                                          MappingValue = dr["Mappingvalue"].ToString()
                                      }).ToList();

                rpaExcelData = originalExcelData.Copy();
                var originalHeaders = originalExcelData.Columns;
                //get mapping
                foreach (DataColumn x in originalHeaders)
                {

                    var mappedColumn = rpaMappingList.Where(l => l.Value.ToUpper().Trim() == x.ColumnName.ToUpper().Trim());
                    if (mappedColumn.Any())
                    {
                        //add into mandatoryColumnsPairs dictionary for duplicated header checking later
                        mandatoryColumnsPairs.Add(rpaExcelData.Columns[x.ColumnName].ColumnName, mappedColumn.OrderBy(o => o.MappingValue).FirstOrDefault().MappingValue);
                        try
                        {
                            //Replace with mapped value
                            rpaExcelData.Columns[x.ColumnName].ColumnName = mappedColumn.FirstOrDefault().MappingValue;
                        }
                        catch (DuplicateNameException ex)
                        {
                            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "DuplicateNameException in PopulateRPAExcelData: " + ex.Message + "; appending header name.", "MemberListing");

                            //Replace with mapped value buit appended since duped
                            rpaExcelData.Columns[x.ColumnName].ColumnName = mappedColumn.FirstOrDefault().MappingValue + "_1";
                        }
                        results = true; //true if mapped columns exists.
                    }
                }
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in PopulateRPAExcelData: " + ex.Message, "MemberListing");
                throw;
            }

            return rpaExcelData;
        }

        private string ConstructFileName(string product, string contractNo, string version, string accNo)
        {
            var dateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            var fileName = product + "_EB_" + contractNo + "_" + version + "_" + accNo + "_" + dateTime + ".csv";
            return fileName;
        }

        private void ExportDataSetToExcel(string location, DataTable data)
        {
            //Call function without needing mandatoryColData
            ExportDataSetToExcel(location, data, new DataTable());
        }

        private void ExportDataSetToExcel(string location, DataTable data, DataTable mandatoryColData)
        {
            StreamWriter wr = new StreamWriter(location);
            Dictionary<int, string> columnTypeList = new Dictionary<int, string>();

            for (int i = 0; i < data.Columns.Count; i++)
            {
                var colName = data.Columns[i].ToString().ToUpper();

                if (mandatoryColData.Rows.Count > 0)
                {
                    //To prevent to be displayed as Scientific notation. Add in the DataType data into columnTypeList if colName exists in mandatoryColData "MandatoryColumn" field.
                    var columnCheck = mandatoryColData.AsEnumerable().Where(row => row["MandatoryColumn"].ToString().Trim().Equals(colName, StringComparison.OrdinalIgnoreCase)).Select(s => s["DataType"].ToString()).FirstOrDefault();
                    if (!string.IsNullOrEmpty(columnCheck))
                        columnTypeList.Add(i, columnCheck);
                }

                wr.Write(colName + ",");
            }

            wr.WriteLine();

            //write rows to excel file
            for (int i = 0; i < (data.Rows.Count); i++)
            {
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    if (data.Rows[i][j] != null)
                    {
                        //To prevent to be displayed as Scientific notation
                        if (columnTypeList.Count > 0 && columnTypeList.ContainsKey(j))
                            wr.Write(Utility.FormatCSVDataTypes(Convert.ToString(data.Rows[i][j]), columnTypeList[j]) + ",");
                        else
                            wr.Write(Convert.ToString(data.Rows[i][j]) + ",");
                    }
                    else
                    {
                        wr.Write(",");
                    }
                }
                //go to next line
                wr.WriteLine();
            }
            //close file
            wr.Close();
        }

        private byte[] ExportDataSetToBytes(DataTable data)
        {
            byte[] retByte;

            //Call function without needing mandatoryColData
            retByte = ExportDataSetToBytes(data, new DataTable());

            return retByte;
        }

        private byte[] ExportDataSetToBytes(DataTable data, DataTable mandatoryColData)
        {
            byte[] retByte;
            MemoryStream stream = new MemoryStream();
            StreamWriter wr = new StreamWriter(stream);
            Dictionary<int, string> columnTypeList = new Dictionary<int, string>();

            for (int i = 0; i < data.Columns.Count; i++)
            {
                var colName = data.Columns[i].ToString().ToUpper();

                if (mandatoryColData.Rows.Count > 0)
                {
                    //To prevent to be displayed as Scientific notation. Add in the DataType data into columnTypeList if colName exists in mandatoryColData "MandatoryColumn" field.
                    var columnCheck = mandatoryColData.AsEnumerable().Where(row => row["MandatoryColumn"].ToString().Trim().Equals(colName, StringComparison.OrdinalIgnoreCase)).Select(s => s["DataType"].ToString()).FirstOrDefault();
                    if (!string.IsNullOrEmpty(columnCheck))
                        columnTypeList.Add(i, columnCheck);
                }

                wr.Write(colName + ",");
            }

            wr.WriteLine();

            //write rows to excel file
            for (int i = 0; i < (data.Rows.Count); i++)
            {
                for (int j = 0; j < data.Columns.Count; j++)
                {
                    if (data.Rows[i][j] != null)
                    {
                        //To prevent to be displayed as Scientific notation
                        if (columnTypeList.Count > 0 && columnTypeList.ContainsKey(j))
                            wr.Write(Utility.FormatCSVDataTypes(Convert.ToString(data.Rows[i][j]), columnTypeList[j]) + ",");
                        else
                            wr.Write(Convert.ToString(data.Rows[i][j]) + ",");
                    }
                    else
                    {
                        wr.Write(",");
                    }
                }
                //go to next line
                wr.WriteLine();
            }
            //close file
            wr.Close();

            retByte = stream.ToArray();

            return retByte;
        }
    }
}