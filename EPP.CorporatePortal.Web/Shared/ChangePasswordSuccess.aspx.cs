using System;
using System.Configuration;

namespace EPP.CorporatePortal.Shared
{
    public partial class ChangePasswordSuccess : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.AddHeader("REFRESH", "3;URL=" + ConfigurationManager.AppSettings["RouteURL"] + "/Shared/Logout.aspx");
        }
    }
}