using EPP.CorporatePortal.DAL.Service;
using EPP.CorporatePortal.DAL.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using EPP.CorporatePortal.Models;
using System.Data;
using System.Threading;
using EPP.CorporatePortal.Helper;
using EPP.CorporatePortal.Common;
using static EPP.CorporatePortal.Common.Enums;
using System.Configuration;

namespace EPP.CorporatePortal.Application
{
    /// <summary>
    /// Summary description for DownloadFile
    /// </summary>
    public class DownloadFile : IHttpHandler, System.Web.SessionState.IReadOnlySessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            UserIdentityModel _UserIdentityModel = new UserIdentityModel();

            if (context.Session[ConfigurationManager.AppSettings["SessionVariableName"]] != null)
            {
                _UserIdentityModel = (UserIdentityModel)context.Session[ConfigurationManager.AppSettings["SessionVariableName"]];
                new DAL.Service.AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, "", "Get session: " + _UserIdentityModel.BusinessRegistrationNo, "DownloadFile");
            }

            System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;
            var id = request.QueryString["id"];
            var type = request.QueryString["type"];
            var UCorpId = request.QueryString["UCorpId"] ?? "0";

            var downloadFileString = "";
            //var identity = (System.Security.Claims.ClaimsIdentity)context.User.Identity;
            //var userName = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).FirstOrDefault();
            var userName = _UserIdentityModel.PrincipalName;
            //For checking user's company access rights to filw
            //var bizRegNo = identity.Claims.Where(c => c.Type == "BusinessRegistrationNo").Select(c => c.Value).SingleOrDefault();
            var bizRegNo = _UserIdentityModel.BusinessRegistrationNo;
            var storedProcService = new StoredProcService(userName);

            try
            {
                //If admin files
                if (type == Common.Enums.FileDownloadType.CreateUser.ToString() || type == Common.Enums.FileDownloadType.CreateUserException.ToString())
                {
                    var dt = storedProcService.GetUserUploadFileById(Convert.ToInt32(id));
                    //If record exists and if the user has access to admin pages.
                    if (dt.Rows.Count > 0 && CheckPageAccess(Rights_Enum.ManageAdminTasks.ToString(), context))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(dt.Rows[0]["FileName"].ToString());
                        var extension = "";
                        if (type == Common.Enums.FileDownloadType.CreateUser.ToString())
                        {
                            downloadFileString = "UserFile";
                            extension = ".xlsx";
                        }
                        else if (type == Common.Enums.FileDownloadType.CreateUserException.ToString())
                        {
                            downloadFileString = "ExceptionFile";
                            extension = ".csv";
                        }

                        var encFileBinary = (byte[])dt.Rows[0][downloadFileString];
                        var decFileBinary = EncryptionHelper.AESDecryptByteData(encFileBinary);

                        //Return as HTTP Response Stream to user instead of direct file link
                        if (decFileBinary != null)
                        {
                            System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                            response.ClearContent();
                            response.Clear();
                            response.ContentType = "text/plain";
                            response.AddHeader("Content-Disposition", "attachment; filename=" + fileName + "_" + type + extension);
                            response.BinaryWrite(decFileBinary);
                            response.Flush();
                            response.End();
                        }
                        else
                        {
                            new DAL.Service.AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "File doesn't exists: " + type, "DownloadFile");
                            System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                            response.ClearContent();
                            response.Clear();
                            response.Write("File does not exists");
                            response.Flush();
                            response.End();
                        }
                    }
                    else
                    {
                        new DAL.Service.AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "No authority to download", "DownloadFile");
                        System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                        response.ClearContent();
                        response.Clear();
                        response.Write("No authority to download");
                        response.Flush();
                        response.End();
                    }
                }
                else if (type == Common.Enums.FileDownloadType.ClaimsEForm.ToString()) //Claims eForm
                {
                    var dt = storedProcService.GetClaimByMemberClaimsID(Convert.ToInt32(id));
                    //If record exists and is the same corpid as requested. Check file's PolicySourceId with user's policy access list.
                    var policies = CommonEntities.LoadPolicies(bizRegNo, _UserIdentityModel.PrincipalName, _UserIdentityModel.IsOwner,UCorpId);
                    var policyList = policies.AsEnumerable().Select(s => Utility.Decrypt(s["SourceId"].ToString())).Distinct().ToList();

                    if (dt.Rows.Count > 0 && policyList.Contains(dt.Rows[0]["PolicyId"].ToString()))
                    {
                        var fileName = dt.Rows[0]["PortalClaimNo"].ToString() + ".pdf";

                        var encFileBinary = (byte[])dt.Rows[0]["Base64ApplicationForm"];
                        var decFileBinary = EncryptionHelper.AESDecryptByteData(encFileBinary);

                        //Return as HTTP Response Stream to user instead of direct file link
                        if (decFileBinary != null)
                        {
                            System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                            response.ClearContent();
                            response.Clear();
                            response.ContentType = "application/pdf";
                            response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
                            response.BinaryWrite(decFileBinary);
                            response.Flush();
                            response.End();
                        }
                        else
                        {
                            new DAL.Service.AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "File doesn't exists: " + type, "DownloadFile");
                            System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                            response.ClearContent();
                            response.Clear();
                            response.Write("File does not exists");
                            response.Flush();
                            response.End();
                        }
                    }
                    else
                    {
                        new DAL.Service.AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "No authority to download", "DownloadFile");
                        System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                        response.ClearContent();
                        response.Clear();
                        response.Write("No authority to download");
                        response.Flush();
                        response.End();
                    }
                }
                else if (type == Common.Enums.FileDownloadType.ClaimsDocuments.ToString()) //Claims eForm
                {
                    var dt = storedProcService.GetClaimsMemberDocumentByDocID(Convert.ToInt32(id));
                    //If record exists and is the same corpid as requested. Check file's PolicySourceId with user's policy access list.
                    var policies = CommonEntities.LoadPolicies(bizRegNo, _UserIdentityModel.PrincipalName, _UserIdentityModel.IsOwner,UCorpId);
                    var policyList = policies.AsEnumerable().Select(s => Utility.Decrypt(s["SourceId"].ToString())).Distinct().ToList();

                    if (dt.Rows.Count > 0 && policyList.Contains(dt.Rows[0]["PolicyId"].ToString()))
                    {
                        var fileName = dt.Rows[0]["UploadedDocumentName"].ToString();

                        var encFileBinary = (byte[])dt.Rows[0]["Base64Value"];
                        var decFileBinary = EncryptionHelper.AESDecryptByteData(encFileBinary);

                        //Return as HTTP Response Stream to user instead of direct file link
                        if (decFileBinary != null)
                        {
                            System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                            response.ClearContent();
                            response.Clear();
                            response.ContentType = "application/pdf";
                            response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
                            response.BinaryWrite(decFileBinary);
                            response.Flush();
                            response.End();
                        }
                        else
                        {
                            new DAL.Service.AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "File doesn't exists: " + type, "DownloadFile");
                            System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                            response.ClearContent();
                            response.Clear();
                            response.Write("File does not exists");
                            response.Flush();
                            response.End();
                        }
                    }
                    else
                    {
                        new DAL.Service.AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "No authority to download", "DownloadFile");
                        System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                        response.ClearContent();
                        response.Clear();
                        response.Write("No authority to download");
                        response.Flush();
                        response.End();
                    }
                }
                else //Other normal FileUploads
                {
                    var dt = storedProcService.GetFileUploadById(Convert.ToInt32(id));
                    //If record exists and is the same corpid as requested. Check file's PolicySourceId with user's policy access list.
                    var policies = CommonEntities.LoadPolicies(bizRegNo, _UserIdentityModel.PrincipalName, _UserIdentityModel.IsOwner, UCorpId);
                    var policyList = policies.AsEnumerable().Select(s => s["ContractNo"].ToString()).Distinct().ToList();

                    if (dt.Rows.Count > 0 && policyList.Contains(dt.Rows[0]["PolicySourceId"].ToString()))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(dt.Rows[0]["MappedFileName"].ToString());
                        var extension = "";
                        if (type == Common.Enums.FileDownloadType.Exception.ToString())
                        {
                            downloadFileString = "ExceptionFile";
                            extension = ".csv";
                        }
                        else if (type == Common.Enums.FileDownloadType.Invoice.ToString())
                        {
                            downloadFileString = "InvoiceFile";
                            extension = ".pdf";
                        }
                        else if (type == Common.Enums.FileDownloadType.UploadFile.ToString())
                        {
                            downloadFileString = "UploadedFile";
                            fileName = Path.GetFileNameWithoutExtension(dt.Rows[0]["FileName"].ToString());
                            extension = Path.GetExtension(dt.Rows[0]["FileName"].ToString());
                        }

                        var encFileBinary = (byte[])dt.Rows[0][downloadFileString];
                        var decFileBinary = EncryptionHelper.AESDecryptByteData(encFileBinary);

                        //Return as HTTP Response Stream to user instead of direct file link
                        if (decFileBinary != null)
                        {
                            System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                            response.ClearContent();
                            response.Clear();
                            response.ContentType = "text/plain";
                            response.AddHeader("Content-Disposition", "attachment; filename=" + fileName + (type == Common.Enums.FileDownloadType.UploadFile.ToString() ? "" : "_" + type) + extension);
                            response.BinaryWrite(decFileBinary);
                            response.Flush();
                            response.End();
                        }
                        else
                        {
                            new DAL.Service.AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "File doesn't exists: " + type, "DownloadFile");
                            System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                            response.ClearContent();
                            response.Clear();
                            response.Write("File does not exists");
                            response.Flush();
                            response.End();
                        }
                    }
                    else
                    {
                        new DAL.Service.AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "No authority to download", "DownloadFile");
                        System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                        response.ClearContent();
                        response.Clear();
                        response.Write("No authority to download");
                        response.Flush();
                        response.End();
                    }
                }
            }            
            catch (ThreadAbortException)
            {
                //Thread will always be aborted since response is ended
            }
            catch (Exception ex)
            {
                new DAL.Service.AuditTrail().LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in ProcessRequest: " + ex.Message, "DownloadFile");
                System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                response.ClearContent();
                response.Clear();
                response.Write("Error in downloading file");
                response.Flush();
                response.End();
            }
        }

        private bool CheckPageAccess(string right, HttpContext context)
        {
            UserIdentityModel _UserIdentityModel = new UserIdentityModel();

            if (context.Session[ConfigurationManager.AppSettings["SessionVariableName"]] != null)
            {
                _UserIdentityModel = (UserIdentityModel)context.Session[ConfigurationManager.AppSettings["SessionVariableName"]];
            }

            var retValue = false;
            if (!String.IsNullOrEmpty(right))
            {
                var rightEnum = (Rights_Enum)Enum.Parse(typeof(Rights_Enum), right.ToString());
                List<Rights_Enum> RequestedPermissions = new List<Rights_Enum>
            {
                rightEnum
            };
                //var identity = (System.Security.Claims.ClaimsIdentity)HttpContext.Current.User.Identity;
                //var userName = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).SingleOrDefault();
                var userName = _UserIdentityModel.PrincipalName;
                var usrPermissions = GetUserPermission(userName);
                if (AuthorizationHelper.HasPermission(usrPermissions, RequestedPermissions, ComparisonType.All))
                {
                    retValue = true;
                }
            }
            else
            {
                retValue = false;
            }
            return retValue;
        }

        protected IList<Rights_Enum> GetUserPermission(string userName)
        {
            var list = new List<Rights_Enum>();

            var roles = new RolesService().GetUserRoles(userName);

            foreach (var role in roles)
            {
                var rights = new UserService().GetRoleRightsEnumList(role);
                list.AddRange(rights);
            }
            return list;
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}