using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EPP.CorporatePortal.Common;
using EPP.CorporatePortal.DAL.Model;
using EPP.CorporatePortal.DAL.Service;
using EPP.CorporatePortal.Models;

namespace EPP.CorporatePortal.Admin
{
    public partial class AuditLog : System.Web.UI.Page
    {
        //This property will contain the current page number 
        public int PageNumber
        {
            get
            {
                if (ViewState["PageNumber"] != null)
                {
                    return Convert.ToInt32(ViewState["PageNumber"]);
                }
                else
                {
                    return 0;
                }
            }
            set { ViewState["PageNumber"] = value; }
        }
        private AuditTrail auditTrailService = new AuditTrail();
        private UserIdentityModel _UserIdentityModel = new UserIdentityModel();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session[ConfigurationManager.AppSettings["SessionVariableName"]] != null)
            {
                _UserIdentityModel = (UserIdentityModel)Session[ConfigurationManager.AppSettings["SessionVariableName"]];
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, _UserIdentityModel.PrincipalName, "Logout", "Logout");
            }

            var userName = _UserIdentityModel.PrincipalName;

            var newCorpId = Request.QueryString["CorpId"] ?? "0";

            try
            {
                hdnCorpId.Value = Utility.EncodeAndDecryptCorpId(newCorpId);

                var accessPermission = Rights_Enum.ManageAdminTasks;
                HiddenField hdnPermission = (HiddenField)Page.Master.FindControl("hdnPermission");
                hdnPermission.Value = Enum.GetName(typeof(Rights_Enum), accessPermission);
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Page_Load: " + ex.Message, "AuditLog");
            }
        }
        protected void BtnSearch_Click(object sender, EventArgs e)
        {
            var userName = _UserIdentityModel.PrincipalName;

            var searchString = txtSearch.Value;
            var searchType = selectSearchType.Value;

            var searchStartDate = txtStartDate.Value;
            var searchEndDate = txtEndDate.Value;

            if (string.IsNullOrEmpty(searchString) || string.IsNullOrEmpty(searchType) || string.IsNullOrEmpty(searchStartDate) || string.IsNullOrEmpty(searchEndDate))
            {
                Utility.RegisterStartupScriptHandling(this, "Error", "alert('Please make sure all fields are keyed in');", true, true, userName);
                return;
            }

            LoadAuditLog(userName, searchString, searchType, Convert.ToDateTime(searchStartDate), Convert.ToDateTime(searchEndDate));
        }
        private void LoadAuditLog(string userName, string searchString, string searchType, DateTime searchStartDate, DateTime searchEndDate)
        {
            var service = new StoredProcService(userName);

            //load menu based on Audit Log
            DataTable dt = service.GetAuditLog(userName, searchString, searchType, searchStartDate, searchEndDate);

            rptAuditLog.DataSource = dt;
            rptAuditLog.DataBind();

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadAuditLog: Loaded", "AuditLog");
        }
        protected void DdlFilterValues_OnChange(object sender, EventArgs e)
        {
            var searchString = txtSearch.Value;
        }
        protected void rptPaging_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            PageNumber = Convert.ToInt32(e.CommandArgument) - 1;
        }
        protected void GenerateReport(object sender, EventArgs e)
        {          
            var userName = _UserIdentityModel.PrincipalName;

            var service = new StoredProcService(userName);

            var searchString = txtSearch.Value;
            var searchType = selectSearchType.Value;

            var searchStartDate = txtStartDate.Value;
            var searchEndDate = txtEndDate.Value;

            var fileformat = selectGenerateReportFormat.Value;

            if (string.IsNullOrEmpty(searchString) || string.IsNullOrEmpty(searchType) || string.IsNullOrEmpty(searchStartDate) || string.IsNullOrEmpty(searchEndDate))
            {
                Utility.RegisterStartupScriptHandling(this, "Error", "alert('Please make sure all fields are keyed in');", true, true, userName);
                return;
            }

            try
            {
                //load menu based on Audit Log
                DataTable dt = service.GetAuditLog(userName, searchString, searchType, Convert.ToDateTime(searchStartDate), Convert.ToDateTime(searchEndDate));

                if (fileformat == "CSV")
                {
                    var fileBinary = ExportDataSetToBytes(dt);

                    //Utility.RegisterStartupScriptHandling(this, "Success", "window.open(url,'_blank');", true, true, userName);

                    System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                    response.ClearContent();
                    response.Clear();
                    response.ContentType = "text/plain";
                    response.AddHeader("Content-Disposition", "attachment; filename=AuditLog_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".csv");
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
                    response.AddHeader("Content-Disposition", "attachment; filename=AuditLog_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".pdf");
                    response.BinaryWrite(fileBinary);
                    response.Flush();
                    response.End();

                }
            }
            catch (Exception errMsg)
            {
                //In case got any uncaptured errors.
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, errMsg.ToString(), "AuditLog"); ;
            }

            btnGenerateReport.Enabled = true;
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
                builder.Append("table{border: 1px solid black;table-layout: fixed;width: 770px;border-collapse: collapse;font-size: x-small;}");
                builder.Append("td { page-break-inside: avoid; }");
                builder.Append("body { max-width: 770px;}");
                builder.Append("</style>");
                builder.Append("</head>");
                builder.Append("<body>");
                builder.Append("<table>");
                builder.Append("<tr>");
                builder.Append("<th style='Width: 80px;'>LASTLOGINDATE</th>");
                builder.Append("<th style='Width: 80px;'>USERNAME</th>");
                builder.Append("<th style='Width: 80px;'>FULLNAME</th>");
                builder.Append("<th style='Width: 80px;'>ROC</th>");
                builder.Append("<th style='Width: 80px;'>CORPNAME</th>");
                builder.Append("<th style='Width: 240px;'>DESCRIPTION</th>");
                builder.Append("<th style='Width: 80px;'>MENU</th>");
                builder.Append("<th style='Width: 50px;'>TYPE</th>");
                builder.Append("</tr>");

                foreach (DataRow dr in dt.Rows)
                {
                    builder.Append("<tr align='left' valign='top'>");
                    foreach (DataColumn dc in dt.Columns)
                    {

                        var checkcolumn = dc.ColumnName.ToString();
                        var checkvalue = dr[dc.ColumnName].ToString();

                        if (dc.ColumnName.ToString() != "id")
                        {
                            builder.Append("<td style='word-wrap: break-word;'>");
                            builder.Append(dr[dc.ColumnName].ToString());
                            builder.Append("</td>");
                        }
                    }
                    builder.Append("</tr>");
                }

                builder.Append("</table>");
                builder.Append("</body>");
                builder.Append("</html>");


                MemoryStream ms = new System.IO.MemoryStream();
                StreamWriter wr = new StreamWriter(ms);
                wr.Write(builder.ToString());

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
                        //wr.Write(Convert.ToString(data.Rows[i][j]) + ",");
                        if (data.Columns[j].ToString().ToUpper() == "DESCRIPTION")
                        {
                            var strDesc = Convert.ToString(data.Rows[i][j]);

                            wr.Write("\"" + strDesc + "\"" + ",");
                        }
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
        /// <summary>
        /// Turn a string into a CSV cell output
        /// </summary>
        /// <param name="value">String to output</param>
        /// <returns>The CSV cell formatted string</returns>
        private string ConvertToCsvCell(string value)
        {
            var mustQuote = value.Any(x => x == ',' || x == '\"' || x == '\r' || x == '\n');

            if (!mustQuote)
            {
                return value;
            }

            value = value.Replace("\"", "\"\"");

            return string.Format("\"{0}\"", value);
        }
    }
}