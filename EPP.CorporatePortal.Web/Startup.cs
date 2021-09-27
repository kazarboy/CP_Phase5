using System;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;

[assembly: OwinStartup(typeof(EPP.CorporatePortal.Startup))]

namespace EPP.CorporatePortal
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
          
            SetCookieAuthenticationAsDefault(app);
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
        }
        private void SetCookieAuthenticationAsDefault(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                ExpireTimeSpan = TimeSpan.FromMinutes(60),
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Login.aspx")});
        }
    }
}
