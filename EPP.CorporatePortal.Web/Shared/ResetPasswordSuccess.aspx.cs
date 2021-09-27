using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EPP.CorporatePortal.Shared
{
    public partial class ResetPasswordSuccess : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.AddHeader("REFRESH", "3;URL=" + ConfigurationManager.AppSettings["RouteURL"] + "/Shared/Logout.aspx");
        }
    }
}