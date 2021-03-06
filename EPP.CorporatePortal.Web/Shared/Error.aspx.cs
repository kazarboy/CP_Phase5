using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EPP.CorporatePortal.Shared
{
    public partial class Error : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var errorMsg = Request.QueryString["Error"];
            if (errorMsg != null)
            {
                spanError.InnerHtml = Server.HtmlEncode(errorMsg);
            }
        }
    }
}