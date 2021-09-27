using EPP.CorporatePortal.Common;
using EPP.CorporatePortal.DAL.Service;
using EPP.CorporatePortal.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EPP.CorporatePortal.Admin
{
    public partial class UserAccessMatrix : System.Web.UI.Page
    {
        private AuditTrail auditTrailService = new AuditTrail();
        protected void Page_Load(object sender, EventArgs e)
        {
            var userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;

            try
            {
                var accessPermission = Rights_Enum.ManageAdminTasks;
                HiddenField hdnPermission = (HiddenField)Page.Master.FindControl("hdnPermission");
                hdnPermission.Value = Enum.GetName(typeof(Rights_Enum), accessPermission);
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Page_Load: " + ex.Message, "ManageAccess");
            }
        }
        protected void BtnSearch_Click(object sender, EventArgs e)
        {
            var userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;

            LoadUserList(userName);
        }
        protected void BtnAllStatus_Click(object sender, EventArgs e)
        {
            var userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;

            LoadUserList(userName);
        }
        protected void BtnActiveStatus_Click(object sender, EventArgs e)
        {
            var userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;

            LoadUserList(userName, "Active");
        }
        protected void BtnSuspendedStatus_Click(object sender, EventArgs e)
        {
            var userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;

            LoadUserList(userName, "Suspended");
        }
        private void LoadUserList(string userName)
        {
            LoadUserList(userName, "");
        }
        private void LoadUserList(string userName, string statusFilter)
        {
            //var user = HttpContext.Current.User as ClaimsPrincipal;
            //var identity = user.Identity as ClaimsIdentity;
            //var bizRegNo = identity.Claims.Where(c => c.Type == "BusinessRegistrationNo").Select(c => c.Value).SingleOrDefault();
            var bizRegNo = ((CorporatePortalSite)this.Master)._UserIdentityModel.BusinessRegistrationNo;
            var role = ((CorporatePortalSite)this.Master)._UserIdentityModel.Role;

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadUserList: Started", "ManageAccess");

            var service = new StoredProcService(userName);

            //load menu
            DataTable dt = service.GetUserAccessMatrix(txtSearchString.Text);

            //Set up status counts
            lblAllStatusCnt.Text = dt.Rows.Count.ToString();
            lblActiveCnt.Text = dt.AsEnumerable().Where(row => row["Status"].ToString().Equals("Active")).Count().ToString();
            lblSuspendedCnt.Text = dt.AsEnumerable().Where(row => row["Status"].ToString().Equals("Suspended")).Count().ToString();

            //Filter according to status if chosen
            if (!String.IsNullOrEmpty(statusFilter))
            {
                var dtFiltered = dt.AsEnumerable().Where(row => row["Status"].ToString().Equals(statusFilter));
                if (dtFiltered.Any())
                    dt = dtFiltered.CopyToDataTable();
                else
                    dt = new DataTable();

                btnAllStatus.CssClass = "btn btnCount";
                btnActiveStatus.CssClass = statusFilter == "Active" ? "btn btnCount greenActive" : "btn btnCount";
                btnSuspendedStatus.CssClass = statusFilter == "Suspended" ? "btn btnCount redActive" : "btn btnCount";
            }
            else
            {
                btnAllStatus.CssClass = "btn btnCount yellowActive";
                btnActiveStatus.CssClass = "btn btnCount";
                btnSuspendedStatus.CssClass = "btn btnCount";
            }

            rptUserList.DataSource = dt;
            rptUserList.DataBind();

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadUserList: Finished", "ManageAccess");
        }
        protected void GenerateReport(object sender, EventArgs e)
        {
            var userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;

            var service = new StoredProcService(userName);

            var searchString = txtSearchString.Text;

            var fileformat = selectGenerateReportFormat.Value;

            //if (string.IsNullOrEmpty(searchString) )
            //{
            //    Utility.RegisterStartupScriptHandling(this, "Error", "alert('Please make sure all fields are keyed in');", true, true, userName);
            //    return;
            //}

            try
            {
                //load menu based on Audit Log
                DataTable dt = service.GetUserAccessMatrix(searchString);

                //Utility.RegisterStartupScriptHandling(this, "Success", "window.open(url,'_blank');", true, true, userName);

                btnGenerateReport.Enabled = true;

                if (fileformat == "CSV")
                {
                    var fileBinary = ExportDataSetToBytes(dt);

                    System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                    response.ClearContent();
                    response.Clear();
                    response.ContentType = "text/plain";
                    response.AddHeader("Content-Disposition", "attachment; filename=UAM_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".csv");
                    response.BinaryWrite(fileBinary);
                    response.Flush();
                    response.End();
                }
                else
                {
                    var fileBinary = ConvertDatatableHTML(dt);

                    System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                    response.ClearContent();
                    response.Clear();
                    response.ContentType = "application/pdf";
                    response.AddHeader("Content-Disposition", "attachment; filename=UAM_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".pdf");
                    response.BinaryWrite(fileBinary);
                    response.Flush();
                    response.End();
                }
            }
            catch (Exception errMsg)
            {
                //In case got any uncaptured errors.
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, errMsg.ToString(), "UserAccessMatrix"); ;
            }

        }

        private byte[] ConvertDatatableHTML(DataTable dt)
        {
            byte[] retByte;

            try
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("<html>");
                builder.Append("<title>");
                builder.Append("Page-");
                builder.Append(Guid.NewGuid());
                builder.Append("</title>");
                builder.Append("<style> th { border: 1px solid black}");
                builder.Append("td {border: 1px solid black; page-break-inside: avoid; word-break:break-all;}");
                builder.Append("table{border: 1px solid black;table-layout: fixed;width: 700px;border-collapse: collapse;font-size: x-small;}");
                builder.Append("td { page-break-inside: avoid; }");
                builder.Append("body { max-width: 700px;}");
                builder.Append("</style>");
                builder.Append("</head>");
                builder.Append("<body>");
                builder.Append("<table>");
                builder.Append("<tr>");
                builder.Append("<th style='Width: 30px;'>ID</th>");
                builder.Append("<th style='Width: 100px;'>USERNAME</th>");
                builder.Append("<th style='Width: 100px;'>FULLNAME</th>");
                builder.Append("<th style='Width: 100px;'>EMAIL</th>");
                builder.Append("<th style='Width: 90px;'>LAST LOGIN</th>");
                builder.Append("<th style='Width: 50px;'>STATUS</th>");
                builder.Append("<th style='Width: 80px;'>CORPORATESOURCEID</th>");
                builder.Append("<th style='Width: 50px;'>ROLE</th>");
                builder.Append("<th style='Width: 90px;'>CORPNAME</th>");
                builder.Append("</tr>");

                foreach (DataRow dr in dt.Rows)
                {
                    builder.Append("<tr align='left' valign='top'>");
                    builder.Append("<td style='word-wrap: break-word;'>");
                    builder.Append(dr["Id"].ToString());
                    builder.Append("</td>");
                    builder.Append("<td style='word-wrap: break-word;'>");
                    builder.Append(dr["UserName"].ToString());
                    builder.Append("</td>");
                    builder.Append("<td style='word-wrap: break-word;'>");
                    builder.Append(dr["FullName"].ToString());
                    builder.Append("</td>");
                    builder.Append("<td style='word-wrap: break-word;'>");
                    builder.Append(dr["EmailAddress"].ToString());
                    builder.Append("</td>");
                    builder.Append("<td style='word-wrap: break-word;'>");
                    builder.Append(dr["LastLoginDate"].ToString());
                    builder.Append("</td>");
                    builder.Append("<td style='word-wrap: break-word;'>");
                    builder.Append(dr["Status"].ToString());
                    builder.Append("</td>");
                    builder.Append("<td style='word-wrap: break-word;'>");
                    builder.Append(dr["CorporateSourceId"].ToString());
                    builder.Append("</td>");
                    builder.Append("<td style='word-wrap: break-word;'>");
                    builder.Append(dr["RoleName"].ToString());
                    builder.Append("</td>");
                    builder.Append("<td style='word-wrap: break-word;'>");
                    builder.Append(dr["CorpName"].ToString());
                    builder.Append("</td>");
                    builder.Append("</tr>");
                }

                builder.Append("</table>");
                builder.Append("</body>");
                builder.Append("</html>");

                retByte = Utility.PdfSharpConvertA4(builder.ToString());

                return retByte;
            }
            catch (Exception)
            {
                throw;
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
    }
}