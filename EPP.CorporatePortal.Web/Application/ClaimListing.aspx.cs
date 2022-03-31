using EPP.CorporatePortal.Common;
using EPP.CorporatePortal.DAL.Model;
using EPP.CorporatePortal.DAL.Service;
using EPP.CorporatePortal.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EPP.CorporatePortal.Application
{
    public partial class ClaimListing : System.Web.UI.Page
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
            var newUCorpId = Request.QueryString["UCorpId"] ?? "0";

            try
            {
                hdnCorpId.Value = Utility.EncodeAndDecryptCorpId(newCorpId);
                hdnUCorpId.Value = Utility.EncodeAndDecryptCorpId(newUCorpId);

                var accessPermission = Rights_Enum.ManageClaim;
                HiddenField hdnPermission = (HiddenField)Page.Master.FindControl("hdnPermission");
                hdnPermission.Value = Enum.GetName(typeof(Rights_Enum), accessPermission);

                //Load claims
                LoadClaimsList(userName);

                //Create session model for claim submission usage
                CreateClaimModelSession();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Page_Load: " + ex.Message, "ClaimListing");
            }
        }
        protected void BtnAllStatus_Click(object sender, EventArgs e)
        {
            btnAllStatus.CssClass = "btn-tab tablink btn";
            btnInProgressStatus.CssClass = "btn-tab tablink btn";
            btnPendingStatus.CssClass = "btn-tab tablink btn";
            btnApprovedStatus.CssClass = "btn-tab tablink btn";
            btnFullyPaidStatus.CssClass = "btn-tab tablink btn";
            btnRejectedStatus.CssClass = "btn-tab tablink btn";

            btnAllStatus.CssClass = "btn-tab tablink btn active";

            txtSearch.Value = string.Empty;
            hdnFilterValues.Value = string.Empty;

            LoadClaimsList(_UserIdentityModel.PrincipalName);
        }
        protected void BtnInProgressStatus_Click(object sender, EventArgs e)
        {
            btnAllStatus.CssClass = "btn-tab tablink btn";
            btnInProgressStatus.CssClass = "btn-tab tablink btn";
            btnPendingStatus.CssClass = "btn-tab tablink btn";
            btnApprovedStatus.CssClass = "btn-tab tablink btn";
            btnFullyPaidStatus.CssClass = "btn-tab tablink btn";
            btnRejectedStatus.CssClass = "btn-tab tablink btn";

            btnInProgressStatus.CssClass = "btn-tab tablink btn active";

            txtSearch.Value = string.Empty;
            hdnFilterValues.Value = Common.Constants.Application.PortalStatus.InProgress;

            LoadClaimsList(_UserIdentityModel.PrincipalName, Common.Constants.Application.PortalStatus.InProgress);
        }
        protected void BtnPendingStatus_Click(object sender, EventArgs e)
        {
            btnAllStatus.CssClass = "btn-tab tablink btn";
            btnInProgressStatus.CssClass = "btn-tab tablink btn";
            btnPendingStatus.CssClass = "btn-tab tablink btn";
            btnApprovedStatus.CssClass = "btn-tab tablink btn";
            btnFullyPaidStatus.CssClass = "btn-tab tablink btn";
            btnRejectedStatus.CssClass = "btn-tab tablink btn";

            btnPendingStatus.CssClass = "btn-tab tablink btn active";

            txtSearch.Value = string.Empty;
            hdnFilterValues.Value = Common.Constants.Application.PortalStatus.PendingApproval;

            LoadClaimsList(_UserIdentityModel.PrincipalName, Common.Constants.Application.PortalStatus.PendingApproval);
        }
        protected void BtnApprovedStatus_Click(object sender, EventArgs e)
        {
            btnAllStatus.CssClass = "btn-tab tablink btn";
            btnInProgressStatus.CssClass = "btn-tab tablink btn";
            btnPendingStatus.CssClass = "btn-tab tablink btn";
            btnApprovedStatus.CssClass = "btn-tab tablink btn";
            btnFullyPaidStatus.CssClass = "btn-tab tablink btn";
            btnRejectedStatus.CssClass = "btn-tab tablink btn";

            btnApprovedStatus.CssClass = "btn-tab tablink btn active";

            txtSearch.Value = string.Empty;
            hdnFilterValues.Value = Common.Constants.Application.PortalStatus.Approved;

            LoadClaimsList(_UserIdentityModel.PrincipalName, Common.Constants.Application.PortalStatus.Approved);
        }
        protected void BtnFullyPaidStatus_Click(object sender, EventArgs e)
        {
            btnAllStatus.CssClass = "btn-tab tablink btn";
            btnInProgressStatus.CssClass = "btn-tab tablink btn";
            btnPendingStatus.CssClass = "btn-tab tablink btn";
            btnApprovedStatus.CssClass = "btn-tab tablink btn";
            btnFullyPaidStatus.CssClass = "btn-tab tablink btn";
            btnRejectedStatus.CssClass = "btn-tab tablink btn";

            btnFullyPaidStatus.CssClass = "btn-tab tablink btn active";

            txtSearch.Value = string.Empty;
            hdnFilterValues.Value = Common.Constants.Application.PortalStatus.Paid;

            LoadClaimsList(_UserIdentityModel.PrincipalName, Common.Constants.Application.PortalStatus.Paid);
        }
        protected void BtnRejectedStatus_Click(object sender, EventArgs e)
        {
            btnAllStatus.CssClass = "btn-tab tablink btn";
            btnInProgressStatus.CssClass = "btn-tab tablink btn";
            btnPendingStatus.CssClass = "btn-tab tablink btn";
            btnApprovedStatus.CssClass = "btn-tab tablink btn";
            btnFullyPaidStatus.CssClass = "btn-tab tablink btn";
            btnRejectedStatus.CssClass = "btn-tab tablink btn";

            btnRejectedStatus.CssClass = "btn-tab tablink btn active";

            txtSearch.Value = string.Empty;
            hdnFilterValues.Value = Common.Constants.Application.PortalStatus.Rejected;

            LoadClaimsList(_UserIdentityModel.PrincipalName, Common.Constants.Application.PortalStatus.Rejected);
        }
        protected void BtnSearch_Click(object sender, EventArgs e)
        {
            var searchString = txtSearch.Value;

            LoadClaimsList(_UserIdentityModel.PrincipalName, hdnFilterValues.Value, searchString, "", ddlFilterValues.SelectedValue);
        }
        protected void DdlFilterValues_OnChange(object sender, EventArgs e)
        {
            var searchString = txtSearch.Value;

            LoadClaimsList(_UserIdentityModel.PrincipalName, hdnFilterValues.Value, searchString, "", ddlFilterValues.SelectedValue);
        }
        public ClaimSubmissionModel CreateClaimModelSession()
        {
            ClaimSubmissionModel claimModel = new DAL.Model.ClaimSubmissionModel();

            Session["ClaimSubmissionModel"] = claimModel;

            return claimModel;
        }
        private void LoadClaimsList(string userName)
        {
            LoadClaimsList(userName, "", "", "", "");
        }
        private void LoadClaimsList(string userName, string filter)
        {
            LoadClaimsList(userName, filter, "", "", "");
        }
        private void LoadClaimsList(string userName, string statusFilter, string searchString, string filterType, string filterValue)
        {
            var bizRegNo = ((CorporatePortalSite)this.Master)._UserIdentityModel.BusinessRegistrationNo;
            var role = ((CorporatePortalSite)this.Master)._UserIdentityModel.Role;

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadClaimsList: Started", "ClaimListing");

            try
            {
                var service = new StoredProcService(userName);

                //load menu
                DataTable dt = service.GetClaimsList(userName);

                var corpID = Utility.Encrypt(hdnCorpId.Value);
                var UcorpID = Utility.Encrypt(hdnUCorpId.Value);

                //To change/map out CGLS Status to Portal Status
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        row["ClaimStatus"] = CommonEntities.ClaimProcessStatus(row["ClaimStatus"].ToString(), row["CGLSRemark"].ToString(), userName);
                    }
                }

                //To filter out only showing those which valid for logged in PIC
                if (dt.Rows.Count > 0)
                {
                    //Filter Claims lists to only those user has access to. PIC/Owner only
                    var policies = CommonEntities.LoadPolicies(corpID, _UserIdentityModel.PrincipalName, _UserIdentityModel.IsOwner,UcorpID);
                    var policyList = policies.AsEnumerable().Select(s => s["ContractNo"].ToString()).Distinct().ToList();
                    var dtFiltered = dt.AsEnumerable().Where(w => policyList.Contains(w["ContractNo"].ToString()));

                    if (dtFiltered.Any())
                        dt = dtFiltered.CopyToDataTable();
                    else
                        dt = new DataTable();
                }

                btnInProgressStatus.Text = "In Progress (" + dt.AsEnumerable().Where(row => row["ClaimStatus"].ToString().Equals("In Progress")).Count().ToString() + ")";
                btnPendingStatus.Text = "Pending (" + dt.AsEnumerable().Where(row => row["ClaimStatus"].ToString().Equals("Pending Approval")).Count().ToString() + ")";
                btnApprovedStatus.Text = "Approved (" + dt.AsEnumerable().Where(row => row["ClaimStatus"].ToString().Equals("Approved")).Count().ToString() + ")";
                btnFullyPaidStatus.Text = "Fully Paid (" + dt.AsEnumerable().Where(row => row["ClaimStatus"].ToString().Equals("Paid")).Count().ToString() + ")";
                btnRejectedStatus.Text = "Rejected (" + dt.AsEnumerable().Where(row => row["ClaimStatus"].ToString().Equals("Rejected")).Count().ToString() + ")";

                //Filter according to status if chosen
                if (!String.IsNullOrEmpty(statusFilter) && statusFilter != "All")
                {
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadClaimsList: Filtering using " + statusFilter, "ClaimListing");

                    var dtFiltered = dt.AsEnumerable().Where(row => row["ClaimStatus"].ToString().Equals(statusFilter));
                    if (dtFiltered.Any())
                        dt = dtFiltered.CopyToDataTable();
                    else
                        dt = new DataTable();

                    if (Page.IsPostBack)
                        LoadClaimTypeList(dt, userName, filterValue);
                }

                //Filter according to filter type
                if (!Page.IsPostBack)
                    LoadClaimTypeList(dt, userName, filterValue);
                if (!string.IsNullOrEmpty(filterValue) && filterValue != "All")
                {
                    var dtFiltered = dt.AsEnumerable().Where(row => row["BenefitDescription"].ToString().Equals(filterValue));
                    if (dtFiltered.Any())
                        dt = dtFiltered.CopyToDataTable();
                    else
                        dt = new DataTable();
                }

                //Filter according to search string if keyed in
                if (!String.IsNullOrEmpty(searchString))
                {
                    var dtFiltered = dt.AsEnumerable().Where(row => row["ICNo"].ToString().Equals(searchString, StringComparison.OrdinalIgnoreCase) || row["IDNo"].ToString().Equals(searchString, StringComparison.OrdinalIgnoreCase) || row["MemberName"].ToString().Equals(searchString, StringComparison.OrdinalIgnoreCase));
                    if (dtFiltered.Any())
                        dt = dtFiltered.CopyToDataTable();
                    else
                        dt = new DataTable();
                }

                var dtPaged = DataSourcePaging(dt, userName);

                rptClaimsList.DataSource = dtPaged;
                rptClaimsList.DataBind();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in LoadClaimsList: " + ex.Message, "ClaimListing");
            }

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "LoadClaimsList: Finished", "ClaimListing");
        }
        private void LoadClaimTypeList(DataTable dt, string userName, string selectedVal)
        {
            try
            {
                if (dt.Rows.Count > 0)
                {
                    DataView dtView = new DataView(dt);
                    DataTable dtDistinct = dtView.ToTable(true, "BenefitCode", "BenefitDescription");

                    if (dtDistinct.Rows.Count > 0)
                    {
                        ddlFilterValues.Items.Clear();
                        ddlFilterValues.Items.Add("All");
                        foreach (DataRow dr in dtDistinct.Rows)
                        {
                            ddlFilterValues.Items.Add(dr["BenefitDescription"].ToString());
                        }
                    }

                    if (!string.IsNullOrEmpty(selectedVal))
                        ddlFilterValues.Items.FindByValue(selectedVal).Selected = true;
                }
                else
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Info, userName, "Error in LoadClaimTypeList: Empty dt row", "ClaimListing");
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in LoadClaimTypeList: " + ex.Message, "ClaimListing");
            }
        }
        private PagedDataSource DataSourcePaging (DataTable dt, string userName)
        {
            var ret = new PagedDataSource();

            try
            {
                //Create the PagedDataSource that will be used in paging
                PagedDataSource pgitems = new PagedDataSource();
                pgitems.DataSource = dt.DefaultView;
                pgitems.AllowPaging = true;

                //Control page size from here 
                pgitems.PageSize = 20;
                pgitems.CurrentPageIndex = PageNumber;
                if (pgitems.PageCount > 1)
                {
                    rptPaging.Visible = true;
                    ArrayList pages = new ArrayList();
                    for (int i = 0; i <= pgitems.PageCount - 1; i++)
                    {
                        pages.Add((i + 1).ToString());
                    }
                    rptPaging.DataSource = pages;
                    rptPaging.DataBind();
                }
                else
                {
                    rptPaging.Visible = false;
                }

                ret = pgitems;
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in DataSourcePaging: " + ex.Message, "ClaimListing");
            }

            return ret;
        }
        //This method will fire when clicking on the page no link from the pager repeater
        protected void rptPaging_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            PageNumber = Convert.ToInt32(e.CommandArgument) - 1;
            LoadClaimsList(_UserIdentityModel.PrincipalName, hdnFilterValues.Value, txtSearch.Value, "", ddlFilterValues.SelectedValue);
        }
    }
}