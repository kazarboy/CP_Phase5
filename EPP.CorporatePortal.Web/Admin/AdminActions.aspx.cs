using EPP.CorporatePortal.DAL.Service;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace EPP.CorporatePortal.Admin
{
    public partial class AdminActions : System.Web.UI.Page
    {
        public UserService Authorization { get; set; }
        public RolesService RolesService { get; set; }
        protected void Page_Load(object sender, EventArgs e)
        {
            NameValueCollection userdata;
            userdata = Request.Form;
            var formName = userdata["hidFormName"];
            if(formName == "AsignUserToRole")
            {
                AsignUserToRole(userdata);
                Response.Redirect(ConfigurationManager.AppSettings["RouteURL"] + "ManageRoles.aspx");               
            } 
            else if(formName== "CreateDeleteRole") {
                CREATEDELETEROLE(userdata);
                Response.Redirect(ConfigurationManager.AppSettings["RouteURL"] + "ManageRoles.aspx");
            }
            else if (formName == "AssignRoleRights")
            {
                AssignRoleRights(userdata);
                Response.Redirect(ConfigurationManager.AppSettings["RouteURL"] + "ManageRoleRights.aspx");
            }
            
        }

        protected void CREATEDELETEROLE(NameValueCollection userdata)
        {
            var roleName = userdata["textRoleName"];
            var homePageUrl = userdata["textHomePageUrl"];
            if (userdata["CREATEDELETEROLE"] == "CREATE")
            {
                //Common.PostActivityLogs("New Role Created :" + Role.RoleName, profile.UserName, DateTime.UtcNow);
                RolesService.CreateRole(roleName, homePageUrl);
            }
            else if (userdata["CREATEDELETEROLE"] == "DELETE")
            {
                //Common.PostActivityLogs("Role Deleted :" + Role.RoleName, profile.UserName, DateTime.UtcNow);
                RolesService.DeleteRole(roleName);
            }
        }

        protected void AsignUserToRole(NameValueCollection userdata)
        {
            if (userdata != null)
            {
                var selectedChkboxes = userdata.AllKeys.Where(k => k.Contains("chkUserRole"));
                StringBuilder selectedIndexes = new StringBuilder();
                foreach (var selectedChkbox in selectedChkboxes)
                {
                    var index = selectedChkbox.Split('.')[1];
                    selectedIndexes.Append(index + ",");
                }

                var roleName = userdata["hidRoleName"];
                var users = userdata["hidUserName"];
                var listRoleName = roleName.Split(',');
                var listUserName = users.Split(',');
                var roleCount = listRoleName.Count();

                var listisChecked = selectedIndexes.ToString().Split(',');
                if (userdata["UPDATEUSERFROMROLE"] == "Update")
                {

                    for (var i = 0; i < roleCount; i++)
                    {
                        var isCheckedLocal = false;
                        var user = listUserName[i];
                        var role = listRoleName[i];
                        if (listisChecked.ToList().Contains(i.ToString()))
                        {
                            isCheckedLocal = true;
                        }
                        if (isCheckedLocal)
                        {
                            var response = RolesService.AssignUserToRole(user, role);
                            if (response)
                            {
                                //Common.PostActivityLogs("Assigning Role :" + role + " to User : " + user, profile.UserName, DateTime.UtcNow);
                            }
                        }
                        else
                        {

                            var response = RolesService.DeleteUserToRole(user, role);
                            if (response)
                            {
                                //Common.PostActivityLogs("Removing Role :" + role + " from User : " + user, profile.UserName, DateTime.UtcNow);
                            }
                        }
                    }
                }
            }
        }

        public void AssignRoleRights(NameValueCollection userdata)
        { 
            var selectedChkboxes = userdata.AllKeys.Where(k => k.Contains("chkRoleRights"));
            StringBuilder selectedIndexes = new StringBuilder();
            foreach (var selectedChkbox in selectedChkboxes)
            {
                var index = selectedChkbox.Split('.')[1];
                selectedIndexes.Append(index + ",");
            }


            var roleIds = userdata["hidRoleID"].ToString();
            var listRoleIds = roleIds.Split(',');

            var rightsIds = userdata["hidRightsID"];
            var listRightsIds = rightsIds.Split(',');

            var roleName = userdata["hidRoleName"];
            var listRoleName = roleName.Split(',');


            var rightsName = userdata["hidRightsName"];
            var listRightsName = rightsName.Split(',');


            var listisChecked = selectedIndexes.ToString().Split(',');

            var count = listRoleIds.Count();

            if (userdata["UPDATEROLERIGHTS"] == "Update")
            {

                for (var i = 0; i < count; i++)
                {
                    var isCheckedLocal = false;

                    if (listisChecked.ToList().Contains(i.ToString()))
                    {
                        isCheckedLocal = true;
                    }
                    if (isCheckedLocal)
                    {

                        var response = Authorization.AssignRightsToRole( Convert.ToInt32(listRoleIds[i]), Convert.ToInt32(listRightsIds[i]));
                        if (response)
                        {
                            //Common.PostActivityLogs("Assigning Right :" + listRightsName[i] + " to Role : " + listRoleName[i], profile.UserName, DateTime.UtcNow);
                        }
                    }
                    else
                    {

                        var response = Authorization.DeleteRightsToRole(Convert.ToInt32(listRoleIds[i]), Convert.ToInt32(listRightsIds[i]));
                        if (response)
                        {
                           // Common.PostActivityLogs("Removing Right :" + listRightsName[i] + " from Role : " + listRoleName[i], profile.UserName, DateTime.UtcNow);
                        }
                    }
                }
            }
        }
    }
}  
