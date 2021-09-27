using EPP.CorporatePortal.Common;
using EPP.CorporatePortal.DAL.Service;
using System;
using System.Globalization;
using System.Threading;
using System.Web.UI;

namespace EPP.CorporatePortal.Application
{
    public partial class ChangeLanguage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var chosenLang = Request.QueryString["lang"];
            if (chosenLang == null)
            {
                chosenLang = CommonService.GetAppSettingValue("DefaultLanguage");
            }
            var usedCulture = CultureInfo.GetCultureInfo(chosenLang);
            Thread.CurrentThread.CurrentCulture = usedCulture;
            Thread.CurrentThread.CurrentUICulture = usedCulture;
            CultureInfo.DefaultThreadCurrentCulture = usedCulture;
            CultureInfo.DefaultThreadCurrentUICulture = usedCulture;
            Page.DataBind();

            var refererlUrl = Request.UrlReferrer;
            Response.Redirect(refererlUrl.LocalPath);
            Response.End();
        }
    }
}