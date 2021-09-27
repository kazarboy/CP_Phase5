
using EPP.CorporatePortal.Common;
using EPP.CorporatePortal.DAL.Service;
using System;
using System.Data;
using System.Linq;
//using System.Security.Claims;
using System.Web.UI;
using System.Web.UI.WebControls;
using EPP.CorporatePortal.Models;
using System.Configuration;

namespace EPP.CorporatePortal.Application
{

    public partial class Policy : System.Web.UI.Page
    {
        private AuditTrail auditTrailService = new AuditTrail();
        protected void Page_Load(object sender, EventArgs e)
        {
            //var identity = (System.Security.Claims.ClaimsIdentity)Context.User.Identity;
            //var userName = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).SingleOrDefault();
            string userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;
            string isowner = ((CorporatePortalSite)this.Master)._UserIdentityModel.IsOwner;
            var parentCorporate = ((CorporatePortalSite)this.Master)._UserIdentityModel.ParentCorporate;
            var parentBizRegNo = ((CorporatePortalSite)this.Master)._UserIdentityModel.ParentBizRegNo;
            try
            {
                var newCorpId = Request.QueryString["CorpId"] ?? "0";
                var newUCorpId = Request.QueryString["UCorpId"] ?? "0";

                if (newCorpId != "" && newCorpId != "0" && newUCorpId != "" && newUCorpId != "0")
                {
                    hdnCorpId.Value = Utility.EncodeAndDecryptCorpId(newCorpId);
                    hdnUCorpId.Value = Utility.EncodeAndDecryptCorpId(newUCorpId);

                    var policies = CommonEntities.LoadPolicies(newCorpId, userName, isowner,newUCorpId);

                    rptPolicies.DataSource = policies;
                    rptPolicies.DataBind();
                }
                else
                {
                    var storedProcServ = new StoredProcService(userName);
                    var uid = storedProcServ.GetCorporateUId(parentBizRegNo, parentCorporate);
                    var UCorpId = uid.Rows[0]["Id"].ToString();

                    //var bizRegNo = new StoredProcService(userName).GetCorporateByUserName(userName);
                    var CorpValue = new StoredProcService(userName).GetCorporateByUserName(userName,UCorpId);
                    hdnCorpId.Value = Utility.EncodeAndDecryptCorpId(CorpValue.Rows[0]["SourceId"].ToString());
                    hdnUCorpId.Value = Utility.EncodeAndDecryptCorpId(CorpValue.Rows[0]["Id"].ToString());

                    string RetUCorpId = CorpValue.Rows[0]["Id"].ToString();
                    string RetbizRegNo = CorpValue.Rows[0]["SourceId"].ToString();

                    var policies = CommonEntities.LoadPolicies(RetbizRegNo, userName, isowner, UCorpId);

                    rptPolicies.DataSource = policies;
                    rptPolicies.DataBind();
                }                
                SetPageDetails();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Page_Load: " + ex.Message, "Policy");
                throw;
            }
        }

        private void SetPageDetails()
        {
            var accessPermission = Rights_Enum.ManagePolicy;
            HiddenField hdnPermission = (HiddenField)Page.Master.FindControl("hdnPermission");
            hdnPermission.Value = Enum.GetName(typeof(Rights_Enum), accessPermission);

            //var identity = (System.Security.Claims.ClaimsIdentity)Context.User.Identity;
            //var userName = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).SingleOrDefault();
            string userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;
            HiddenField hdnHomePage = (HiddenField)Page.Master.FindControl("hdnHomePage");
            hdnHomePage.Value = ConfigurationManager.AppSettings["RouteURL"] + Utility.GetHomePageByRole(userName);
            
        }

        protected static string GetStatus(object dataItem)
        {
            string value = DataBinder.Eval(dataItem,"Status").ToString();
            if (value.ToLower()=="true")            
                return Resources.Resource.Active;            
            else            
                return Resources.Resource.Inactive;
        }
        protected void rptPolicies_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            // If the Repeater contains no data.
            if (sender is Repeater rptPoliciesObj && rptPoliciesObj.Items.Count < 1 && e.Item.ItemType == ListItemType.Footer)
            {
                // Show the Error Label (if no data is present).
                if (e.Item.FindControl("lblEmptyMsg") is Label lblEmptyMsg)
                {
                    lblEmptyMsg.Visible = true;
                }
            }


        }


       
    }
}