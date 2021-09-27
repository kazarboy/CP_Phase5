using EPP.CorporatePortal.Common;
using EPP.CorporatePortal.DAL.Service;
using EPP.CorporatePortal.Models;
using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EPP.CorporatePortal.Application
{
    public partial class PolicyDetails : System.Web.UI.Page
    {
        private AuditTrail auditTrailService = new AuditTrail();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                var newCorpId = Request.QueryString["CorpId"] ?? "0";
                var newUCorpId = Request.QueryString["UCorpId"] ?? "0";

                newCorpId = Utility.EncodeAndDecryptCorpId(newCorpId);
                newUCorpId = Utility.EncodeAndDecryptCorpId(newUCorpId);

                if (newCorpId != "" && newCorpId != "0" && newUCorpId != "" && newUCorpId != "0")
                {
                    LoadPolicyDetails(newCorpId, ((CorporatePortalSite)this.Master)._UserIdentityModel.IsOwner, newUCorpId);
                    LoadInsuredGroupSubsidaries(newCorpId);
                }
                else
                {
                    //var identity = (System.Security.Claims.ClaimsIdentity)Context.User.Identity;
                    //var bizRegNo = identity.Claims.Where(c => c.Type == "BusinessRegistrationNo").Select(c => c.Value).SingleOrDefault();
                    //** Need to check this - Phase5
                    var bizRegNo = ((CorporatePortalSite)this.Master)._UserIdentityModel.BusinessRegistrationNo;
                    var corporateID = new CorporateService().GetParentId(bizRegNo);;

                    LoadPolicyDetails(corporateID, ((CorporatePortalSite)this.Master)._UserIdentityModel.IsOwner);
                    LoadInsuredGroupSubsidaries(corporateID);
                }
                SetPageDetails();
            }

        }

        private void LoadInsuredGroupSubsidaries(string corporateId)
        {
            var policyId = Request.QueryString["PolicyId"] ?? "0";
            policyId = Utility.EncodeAndDecryptCorpId(policyId.ToString().Trim());
            //var identity = (System.Security.Claims.ClaimsIdentity)Context.User.Identity;
            //var isOwnerClaim = identity.Claims.Where(c => c.Type == "IsOwner").Select(c => c.Value).FirstOrDefault();
            var isOwner = Convert.ToBoolean(((CorporatePortalSite)this.Master)._UserIdentityModel.IsOwner);
            //var userName = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).SingleOrDefault();
            var userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;
            var data = new StoredProcService(userName).GetInsuredGroupSubsidaries(corporateId, userName, policyId, isOwner);
          
            rptInsuredGroupSubsidaries.DataSource = data;
            rptInsuredGroupSubsidaries.DataBind();

            auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, userName, "Retrieved InsuredGroupSubsidaries. CorpID: " + corporateId + ", PolicyID: " + policyId + ", isOwner: " + isOwner.ToString(), "PolicyDetails");
        }
        private void SetPageDetails()
        {
            var accessPermission = Rights_Enum.ManagePolicy;
            HiddenField hdnPermission = (HiddenField)Page.Master.FindControl("hdnPermission");
            hdnPermission.Value = Enum.GetName(typeof(Rights_Enum), accessPermission);

            //var identity = (System.Security.Claims.ClaimsIdentity)Context.User.Identity;
            //var userName = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).SingleOrDefault();
            var userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;
            HiddenField hdnHomePage = (HiddenField)Page.Master.FindControl("hdnHomePage");
            hdnHomePage.Value = ConfigurationManager.AppSettings["RouteURL"] + Utility.GetHomePageByRole(userName);
        }

        private void LoadPolicyDetails(string corporateID, string owner, string UCorpId =null)
        {
            //var identity = (System.Security.Claims.ClaimsIdentity)Context.User.Identity;
            //var owner = identity.Claims.Where(c => c.Type == ClaimTypes.Upn).Select(c => c.Value).FirstOrDefault();
            var policyId = Request.QueryString["PolicyId"] ?? "0";
            policyId = Utility.EncodeAndDecryptCorpId(policyId.ToString().Trim());
            hdnPolicyId.Value = policyId;
            hdnCorpId.Value = corporateID;
            hdnUCorpId.Value = UCorpId;

            try
            {
                var policyProduct = new StoredProcService(owner).GetPolicyProduct(policyId);

                var productString = "<u>Basic</u><br />";
                StringBuilder product2String = new StringBuilder("<u>Supplementary</u><br />");

                //Main Product
                var policyProductMain = policyProduct.AsEnumerable().Where(w => w["Type"].ToString() == "Basic").Select(s => s["ProductCode"].ToString() + " - " + s["ProductName"].ToString()).FirstOrDefault();
                if (!string.IsNullOrEmpty(policyProductMain))
                {
                    productString += Server.HtmlEncode(policyProductMain);

                    spanPolicyProduct1.InnerHtml = productString;
                }

                //Sub Products
                var policyProductSupp = policyProduct.AsEnumerable().Where(w => w["Type"].ToString() == "Supplementary").Select(s => s["ProductCode"].ToString() + " - " + s["ProductName"].ToString()).ToList();
                if (policyProductSupp.Count > 0)
                {
                    var lastItem = policyProductSupp.Last();

                    foreach (string s in policyProductSupp)
                    {
                        var delimiter = s.Equals(lastItem) ? "" : ", ";
                        product2String.Append(Server.HtmlEncode(s) + delimiter);
                    }

                    spanPolicyProduct2.InnerHtml = product2String.ToString();
                }                               

                var checkProductMain = !string.IsNullOrEmpty(policyProductMain);
                var checkProductSupp = policyProductSupp.Count > 0;

                //To display, hide, put in labels where necessary
                if (!checkProductMain && !checkProductSupp) //Both Main and Supp empty
                {
                    divPolicyProductLabel.Style["display"] = "none";
                    divPolicyProduct.Style["display"] = "none";
                    divPolicyProduct2Label.Style["display"] = "none";
                    divPolicyProduct2.Style["display"] = "none";
                }
                else if (checkProductMain && !checkProductSupp) //Only Supp empty
                {
                    spanPolicyProduct1Label.InnerHtml = "Product Code";
                    divPolicyProduct2Label.Style["display"] = "none";
                    divPolicyProduct2.Style["display"] = "none";
                    divPolicyProduct.Style["margin-bottom"] = "0.5rem"; //Add spacing after
                }
                else if (!checkProductMain && checkProductSupp) //Only Main Empty
                {
                    spanPolicyProduct2Label.InnerHtml = "Product Code";                    
                    divPolicyProductLabel.Style["display"] = "none";
                    divPolicyProduct.Style["display"] = "none";
                    divPolicyProduct2.Style["margin-bottom"] = "0.5rem"; //Add spacing after
                }
                else //Normal case, all have values
                {
                    spanPolicyProduct1Label.InnerHtml = "Product Code";
                    divPolicyProduct2.Style["margin-bottom"] = "0.5rem"; //Add spacing after
                }
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "Error", "alert('Error in loading Policy Products');", true);
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, owner, "Error in LoadPolicyProducts: " + ex.Message, "PolicyDetails");
            }

            var policy = new StoredProcService(owner).GetPolicyDetails(policyId, corporateID, UCorpId);
            if (policy.Rows.Count > 0)
            {
                try
                {
                    string strEffectiveTo = "";
                    string strEffectiveFrom = "";
                    string strDueTo = "";

                    var dataRow = policy.Rows[0];

                    if (dataRow["EffectiveFrom"].ToString() != "")
                    {
                        strEffectiveFrom = Convert.ToDateTime(dataRow["EffectiveFrom"]).ToString("dd/MM/yyy");
                    }

                    if (dataRow["EffectiveFrom"].ToString() != "")
                    {
                        strEffectiveTo = Convert.ToDateTime(dataRow["EffectiveTo"]).ToString("dd/MM/yyy");
                        strDueTo = Convert.ToDateTime(dataRow["EffectiveTo"]).AddDays(1).ToString("dd/MM/yyy");
                    }

                    spanCorporateName.InnerHtml = Server.HtmlEncode(dataRow["Name"].ToString());
                    spanIdNo.InnerHtml = Server.HtmlEncode(dataRow["CorporateSourceId"].ToString());
                    spanBankDetail.InnerHtml = Server.HtmlEncode(dataRow["BankBranchName"].ToString() + " - " + dataRow["BankAccountNo"].ToString());
                    spanAccountManager.InnerHtml = Server.HtmlEncode(dataRow["MarketingStaff"].ToString());
                    spanCoveredFrom.InnerHtml = Server.HtmlEncode(strEffectiveFrom);
                    spanCoveredTo.InnerHtml = Server.HtmlEncode(strEffectiveTo);
                    spanRenewal.InnerHtml = Server.HtmlEncode(strDueTo);
                    spanAddress1.InnerHtml = Server.HtmlEncode(dataRow["AddressLine1"].ToString());
                    spanAddress2.InnerHtml = Server.HtmlEncode(dataRow["AddressLine2"].ToString());
                    spanAddress3.InnerHtml = Server.HtmlEncode(dataRow["AddressLine3"].ToString());
                    spanAddress4.InnerHtml = Server.HtmlEncode(dataRow["AddressLine4"].ToString());
                    lblTitle.Text = Utility.TitleCase(dataRow["ProductName"].ToString());

                    if (dataRow["Entity"].ToString() == "ETB")
                    {
                        tabPolicyDtails.InnerText = "Contract Details";
                        hPolicyDetail.InnerText = "Contract Details";
                        h2Insured.InnerText = "Covered Group/ Subsidiaries";
                        btnTabInsured.InnerText = "Covered Group/ Subsidiaries";
                        thTotalInsured.InnerText = "Total Covered Member";
                    }
                    else
                    {
                        tabPolicyDtails.InnerText = "Policy Details";
                        hPolicyDetail.InnerText = "Policy Details";
                        h2Insured.InnerText = "Insured Group/ Subsidiaries";
                        thTotalInsured.InnerText = "Total Insured Member";
                        spanBtn.Attributes.Remove("Class");
                        spanBtn.Attributes.Add("Class", "btn btn-insurance");
                    }

                    if (System.IO.File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "\\Attachments\\Download\\SOB\\" + dataRow["ContractNo"] + "_V" + dataRow["Version"] + ".pdf"))
                    {
                        aSOB.HRef = ConfigurationManager.AppSettings["RouteURL"] + "/Attachments/Download/SOB/" + dataRow["ContractNo"] + "_V" + dataRow["Version"] + ".pdf";
                    }
                    else
                    {
                        aSOB.Attributes.Remove("OnClick");
                        aSOB.InnerHtml = "No SOB Uploaded";
                    }
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Debug, owner, "Retreiving Policiy Details for Policy:" + policyId + ", CorpID: " + corporateID, "PolicyDetails");
                }
                catch (Exception ex)
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "Error", "alert('Error in loading Policy Details');", true);
                    auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, owner, "Error in LoadPolicyDetails: " + ex.Message, "PolicyDetails");
                }
            }
        }    
    }
}