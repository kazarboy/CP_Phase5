using EPP.CorporatePortal.Common;
using System;
using System.Web.UI.WebControls;

namespace EPP.CorporatePortal.Admin
{
    public partial class ManageRoles : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SetPageDetails();
        }
        private void SetPageDetails()
        {
            var accessPermission = Rights_Enum.ManageAdminTasks;
            HiddenField hdnPermission = (HiddenField)Page.Master.FindControl("hdnPermission");
            hdnPermission.Value = Enum.GetName(typeof(Rights_Enum), accessPermission);

            

        }
    }
}
