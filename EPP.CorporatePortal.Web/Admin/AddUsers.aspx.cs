using System;
using System.Data;
using System.IO;
using System.Linq;
//using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using EPP.CorporatePortal.Common;
using EPP.CorporatePortal.DAL.Entity;
using EPP.CorporatePortal.DAL.Service;
using EPP.CorporatePortal.Helper;
using EPP.CorporatePortal.Models;
using Newtonsoft.Json;

namespace EPP.CorporatePortal.Admin
{
    public partial class AddUsers : System.Web.UI.Page
    {
        private AuditTrail auditTrailService = new AuditTrail();
        protected void Page_Load(object sender, EventArgs e)
        {
            
            //var user = HttpContext.Current.User as ClaimsPrincipal;
            //var identity = user.Identity as ClaimsIdentity;
            //var userName = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).SingleOrDefault();
            var userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;
            
            new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "ok Add user 1", "add user");
            try
            {
                hdnUsername.Value = userName;

                var accessPermission = Rights_Enum.ManageAdminTasks;
                HiddenField hdnPermission = (HiddenField)Page.Master.FindControl("hdnPermission");
                hdnPermission.Value = Enum.GetName(typeof(Rights_Enum), accessPermission);
                
                LoadUsersHistory(userName);
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Page_Load: " + ex.Message, "AddUsers");
            }
        }
        private void LoadUsersHistory(string userName)
        {
            var role = ((CorporatePortalSite)this.Master)._UserIdentityModel.Role;

            var service = new StoredProcService(userName);

            //load menu based on user's role
            DataTable dt = service.GetUserUploadFileByUserName(role);

            rptFileUploadHistory.DataSource = dt;
            rptFileUploadHistory.DataBind();

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadUsersHistory: Loaded", "AddUsers");
        }
        protected void Upload(object sender, EventArgs e)
        {
            //var identity = (System.Security.Claims.ClaimsIdentity)Context.User.Identity;
            //var author = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).FirstOrDefault();
            var author = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;
            var role = ((CorporatePortalSite)this.Master)._UserIdentityModel.Role;

            try
            {
                if (ctlFileUpload.HasFile)
                {
                    byte[] uploadedFileBinary;
                    byte[] processedFileBinary;

                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "File Upload: Started for " + ctlFileUpload.PostedFile.FileName, "AddUsers");

                    var service = new StoredProcService(author);
                    var fileUploadExist = service.IsUserUploadFileExist(ctlFileUpload.FileName, author);
                    if (fileUploadExist)
                    {
                        Utility.RegisterStartupScriptHandling(this, "Error", "alert('File exists in system. Kindly rename file name and upload again');", true, true, author);
                        btnUpload.Enabled = true;
                        return;
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

                        //Process to cater for any possible script injection method
                        processedFileBinary = Utility.FileBinaryCheckInjectionChar(uploadedFileBinary, author, out bool injectionCheckResult);
                        if (!injectionCheckResult)
                        {
                            Utility.RegisterStartupScriptHandling(this, "Error", "alert('Error reading excel data');", true, true, author);
                            btnUpload.Enabled = true;
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        //keep on showing the popup and show the error
                        UploadLogging("Error: Error in uploading file", "Error in file Upload Control: " + ex.Message, author, true);
                        btnUpload.Enabled = true;
                        return;
                    }

                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "File Upload: Get Excel Data", "AddUsers");
                    var originalData = GetOriginalByteDataOpenXML(processedFileBinary, author, out bool oriExcelResults); //Reads binary instead
                    if (!oriExcelResults)
                    {
                        Utility.RegisterStartupScriptHandling(this, "Error", "alert('Error reading excel data');", true, true, author);
                        btnUpload.Enabled = true;
                        return;
                    }

                    //Filter in case any rows with empty username. To prevent any empty spaces from being read.
                    var originalDataRow = originalData.AsEnumerable().Where(w => !string.IsNullOrEmpty(w["Username"].ToString()));

                    if (originalDataRow.Any())
                        originalData = originalDataRow.CopyToDataTable();

                    var fileUploadId = 0;
                    try
                    {
                        //Save file binary to database
                        fileUploadId = service.InsUserUploadFile(ctlFileUpload.PostedFile.FileName, DateTime.Now, author, EncryptionHelper.AESEncryptByteData(processedFileBinary));

                        if (fileUploadId != 0)
                        {
                            //For UIDADmin, Business Reg No and Is Owner column is not required. System to auto add
                            if (role == Common.Enums.UserRole.UIDAdmin.ToString())
                            {
                                DataColumnCollection columnsoriginalData = originalData.Columns;
                                if (!columnsoriginalData.Contains("Business Reg No"))
                                    originalData.Columns.Add("Business Reg No");
                                if (!columnsoriginalData.Contains("Is Owner"))
                                    originalData.Columns.Add("Is Owner");

                                var defaultBizRegNo = CommonService.GetSystemConfigValue("AdminBizRegNo");
                                foreach (DataRow row in originalData.Rows)
                                {
                                    //Default Business Reg No for Admin users
                                    row["Business Reg No"] = defaultBizRegNo;
                                    //Default Is Owner for Admin users
                                    row["Is Owner"] = "Y";
                                }
                            }
                            //Loop through all the usernames
                            foreach (DataRow dr in originalData.Rows)
                            {
                                var status = "";
                                var exceptionMessage = "";

                                //Checks if the CorpID exists
                                if (service.IsCorporateExist(dr["Business Reg No"].ToString()))
                                {
                                    status = "Pending";
                                    exceptionMessage = "";
                                }
                                else
                                {
                                    status = "Error";
                                    exceptionMessage = "Business Reg No [" + dr["Business Reg No"].ToString() + "] does not exists";
                                }

                                //Inserts user record into database for processing
                                var userUploadId = service.InsUserUpload(dr["Username"].ToString(), dr["Full Name"].ToString(), dr["Email Address"].ToString(), Utility.Encrypt(dr["Mobile Phone"].ToString()), dr["Gender"].ToString(), Utility.Encrypt(dr["IC No"].ToString()), dr["Business Reg No"].ToString(), status, fileUploadId, dr["Is Owner"].ToString() == "Y", exceptionMessage);
                                if (userUploadId == 0)
                                {
                                    //Delete any existing/remaining data
                                    service.RemoveUserUploadFile(fileUploadId);

                                    UploadLogging("Error: Error in uploading file", "Error in file processing into database: InsUserUpload error responded with 0", author, true);
                                    btnUpload.Enabled = true;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            UploadLogging("Error: Error in uploading file", "Error in file processing into database: InsUserUploadFile error responded with 0", author, true);
                            btnUpload.Enabled = true;
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        //Delete any existing/remaining data
                        service.RemoveUserUploadFile(fileUploadId);

                        //keep on showing the popup and show the error
                        UploadLogging("Error: Error in uploading file", "Error in file processing into database: " + ex.Message, author, true);
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

                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "File Upload: Reload users history", "AddUsers");
                LoadUsersHistory(author);//Reload the history
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "File Upload: Finished", "AddUsers");
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, author, "File uploaded: " + ctlFileUpload.PostedFile.FileName, "AddUsers");
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

        protected void Approval(object sender, EventArgs e)
        {
            //var identity = (System.Security.Claims.ClaimsIdentity)Context.User.Identity;
            //var author = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).FirstOrDefault();
            var author = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;
            var appCode = CommonService.GetSystemConfigValue("AppCode");
            var businessEntityID = CommonService.GetSystemConfigValue("BusinessEntityID");
            var role = ((CorporatePortalSite)this.Master)._UserIdentityModel.Role;

            var loginPageUrl = CommonService.GetSystemConfigValue("LoginPageUrl");

            var loginToken = Session[appCode + "Token"];

            var service = new StoredProcService(author);

            Button btn = (Button)sender;
            var fileID = Convert.ToInt32(btn.CommandArgument);

            var dtFile = service.GetUserUploadFileById(fileID);

            //If file record doesn't exists or if file record is uploaded by the same user
            if (dtFile.Rows.Count == 0 || (dtFile.Rows.Count > 0 && dtFile.AsEnumerable().Where(row => row["UploadedBy"].ToString().Equals(author)).Select(s => s["UploadedBy"].ToString()).Any()))
            {
                Utility.RegisterStartupScriptHandling(this, "Error", "alert('User not authorised to approve');", true, true, author);
                return;
            }

            try
            {
                var dt = service.GetUserUploadByFileId(fileID);

                //Loop thorugh all users of file
                foreach (DataRow dr in dt.Rows)
                {
                    try
                    {
                        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, author, "Processing row id [" + dr["Id"].ToString() + "]", "BtnApprove_Click");
                        //Only do for those still in Processing status
                        if (dr["Status"].ToString() == "Pending")
                        {
                            var emailAddress = dr["EmailAddress"].ToString();
                            var fullName = dr["FullName"].ToString();
                            var gender = dr["Gender"].ToString();
                            var icNo = Utility.EncodeAndDecryptCorpId(dr["ICNo"].ToString());
                            var mobilePhone = Utility.EncodeAndDecryptCorpId(dr["MobilePhone"].ToString());
                            var userName = dr["UserName"].ToString();

                            var userReq = new CreateUserTokenRequest() { BusinessEntityID = Convert.ToInt32(businessEntityID), AppCode = appCode, EmailAddress = emailAddress, FullName = fullName, Gender = gender, ICNo = icNo, MobilePhone = mobilePhone, Username = userName, Url = loginPageUrl };
                            var body = JsonConvert.SerializeObject(userReq, Formatting.Indented);

                            string token = CommonService.EncryptString(body);
                            var createTokenResponse = new LoginService().CreateUser(token, author, loginToken.ToString());
                            if (createTokenResponse.Valid)
                            {
                                //Check based on roles. e.g. UIDAdmin can only add Admin roles. Admin can only add HR roles.
                                if (role == Common.Enums.UserRole.UIDAdmin.ToString())
                                    service.ProcessCreateUser(userName, fullName, dr["BusinessRegNo"].ToString(), Convert.ToBoolean(dr["IsOwner"]), emailAddress, Utility.Encrypt(mobilePhone), gender, Utility.Encrypt(icNo), Convert.ToInt32(Common.Enums.UserRole.Admin));
                                else //For others, current logic will only be adding for new HR users.
                                    service.ProcessCreateUser(userName, fullName, dr["BusinessRegNo"].ToString(), Convert.ToBoolean(dr["IsOwner"]), emailAddress, Utility.Encrypt(mobilePhone), gender, Utility.Encrypt(icNo), Convert.ToInt32(Common.Enums.UserRole.HR));

                                service.UpdateUserUploadStatus(Convert.ToInt32(dr["Id"]), "", "Completed");
                                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, author, "Create User successfully to Agent Portal Hub", "BtnApprove_Click");

                                dr["Status"] = "Completed";
                                dr["ExceptionMessage"] = "";
                            }
                            else
                            {
                                service.UpdateUserUploadStatus(Convert.ToInt32(dr["Id"]), createTokenResponse.ResponseStatusEntity.StatusDescription, "Error");

                                new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, author, "Create User not valid: " + createTokenResponse.ResponseStatusEntity.StatusDescription, "BtnApprove_Click");

                                dr["Status"] = "Error";
                                dr["ExceptionMessage"] = "Create User failed: " + createTokenResponse.ResponseStatusEntity.StatusDescription;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        new AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, author, "Error in file Approval row id [" + dr["Id"].ToString() + "] processing: " + ex, "BtnApprove_Click");
                    }
                }
                var exceptionDTRows = dt.AsEnumerable().Where(w => w["Status"].ToString() != "Completed");
                if (exceptionDTRows.Any())
                {
                    var exceptionDT = exceptionDTRows.CopyToDataTable();

                    exceptionDT.Columns.Remove("Id");
                    exceptionDT.Columns.Remove("FileId");
                    exceptionDT.Columns.Remove("IsOwner");

                    //Save data into file binary
                    var fileBinary = ExportDataSetToBytes(exceptionDT);

                    service.UpdateUserUploadFileExceptionStatus(fileID, EncryptionHelper.AESEncryptByteData(fileBinary), "Error", author);

                    Utility.RegisterStartupScriptHandling(this, "Success", "alert('File processed with errors');", true, true, author);
                }
                else
                {
                    service.UpdateUserUploadFileExceptionStatus(fileID, null, "Completed", author);

                    Utility.RegisterStartupScriptHandling(this, "Success", "alert('File processed successfully');", true, true, author);
                }                
            }
            catch (Exception ex)
            {
                //In case got any uncaptured errors.
                UploadLogging("Error: Error in approving file", "Error in file Approval function: " + ex.Message, author, true);
                btn.Enabled = true;
            }
            finally
            {
                LoadUsersHistory(author);//Reload the history
                //To cater for all cases to re-enable the upload button.
                btn.Enabled = true;
            }
        }

        private byte[] ExportDataSetToBytes(DataTable data)
        {
            byte[] retByte;
            MemoryStream stream = new MemoryStream();
            StreamWriter wr = new StreamWriter(stream);

            for (int i = 0; i < data.Columns.Count; i++)
            {
                var colName = data.Columns[i].ToString().ToUpper();

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
        private void UploadLogging(string userMsg, string errMsg, string userName, bool auditLogging)
        {
            if (!string.IsNullOrEmpty(userMsg))
                ScriptManager.RegisterStartupScript(this, this.GetType(), "Error", "$('#update-member').modal('toggle');$('.uploadlogo').parent().children('span').html('" + userMsg + "');", true);

            if (!string.IsNullOrEmpty(errMsg) && auditLogging)
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, errMsg, "AddUser");
        }
        public DataTable GetOriginalByteDataOpenXML(byte[] fileBytes, string userName, out bool results)
        {
            results = true;
            DataTable dt = new DataTable();
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
    }
}