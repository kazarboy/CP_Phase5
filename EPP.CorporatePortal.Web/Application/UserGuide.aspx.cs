using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EPP.CorporatePortal.Common;
using EPP.CorporatePortal.DAL.Service;
using EPP.CorporatePortal.Models;

namespace EPP.CorporatePortal.Application
{
    public partial class UserGuide : System.Web.UI.Page
    {
        BaseService db = new BaseService();
        private AuditTrail auditTrailService = new AuditTrail();

        public string UserGuideFrame { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            string userName = ((CorporatePortalSite)this.Master)._UserIdentityModel.PrincipalName;
            string isowner = ((CorporatePortalSite)this.Master)._UserIdentityModel.IsOwner;

            var newCorpId = Request.QueryString["CorpId"] ?? "0";

            try
            {
                hdnCorpId.Value = Utility.EncodeAndDecryptCorpId(newCorpId);

                var accessPermission = Rights_Enum.ManageClaim;
                //var accessPermission = accessEnum(userName);
                HiddenField hdnPermission = (HiddenField)Page.Master.FindControl("hdnPermission");
                hdnPermission.Value = Enum.GetName(typeof(Rights_Enum), accessPermission);

                UserGuideFrame = db.dbEntities.SystemConfigs.Where(x => x.Setting == "UserGuide").Select(x => x.Value).Single().ToString();
            }
            catch (Exception ex)
            {
                auditTrailService.LogAuditTrail(DateTime.Now, Common.Enums.AuditType.Error, userName, "Error in Page_Load: " + ex.Message, "UserGuide");
                throw;
            }
        }

        //public Enumerable accessEnum(string Getusername)
        //{
        //    var getuserID = db.dbEntities.user
        //    var getuseraccess =
        //}

    }
}