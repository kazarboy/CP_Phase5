<%@ Page Language="C#" Title="Manage Roles" MasterPageFile="~/CorporatePortalSite.Master" AutoEventWireup="true" CodeBehind="ManageRoles.aspx.cs" Inherits="EPP.CorporatePortal.Admin.ManageRoles" %>

 
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
    <%
    
    var UserService = new  EPP.CorporatePortal.DAL.Service.UserService();
    var RoleService = new  EPP.CorporatePortal.DAL.Service.RolesService();
    var users = UserService.GetAllUsers();%>
    <form id="frmMain" method="post"  action="AdminActions.aspx"  > 
        <input type="hidden" name="hidFormName" value="CreateDeleteRole"/>
        <div class="row">
            <div class="col-lg-3">
                <div class="text-box-container form-group">
                    <label for="textRoleName">Role Name</label>
                   <input type="text" id="textRoleName" runat="server" name="textRoleName" />
                    <label for="textHomePageUrl"> Home Page Url</label>
                   <input type="text" id="textHomePageUrl" runat="server" name="textHomePageUrl" />
                </div>
            </div>
            <div class="row">
                <div class="col-lg-4"></div>
                <div class="col-lg-4">
                    <input type="submit" value="CREATE" class="button-full button-emerald-green-show" name="CREATEDELETEROLE" />
                    <input type="submit" value="DELETE" class="button-full button-emerald-green-show" name="CREATEDELETEROLE" />
                </div>
            </div>
        </div>
    </form>
    <hr />
    <br />
    <form  method="post" action="AdminActions.aspx"  >
     <input type="hidden" name="hidFormName" value="AsignUserToRole"/>
    <table style="width:100%" border="1">
        <tr>
            <th>#</th>
            <th>Users/Role</th>
            <%
                foreach (var r in RoleService.GetAllRoles())
                { %>

                <th title="<%=r.Description %>"><%=r.Name %></th>
            <%} %>
            
        </tr>
        <%
            var rowCount1 = 0;
            var rowNumber = 1;
        %>
        <%
            foreach(var u in users)
            { %>
                <tr>
                <th><%=rowNumber%></th>

                <td><%=u.UserName.ToString()%> </td>

                <%
                    foreach (var r in RoleService.GetAllRoles())
                    {
                        var inRole =   RoleService.IsUserInRole(u.UserName.ToString(), r.Name);
                        var inRoleStr = inRole == true ? "Checked" : "";
                        var name = String.Concat("chkUserRole", ".", rowCount1);
                %>
                <th title="<%=r.Description %>">                      
                    <input type="checkbox" name="<%=name%>" value="true" <%=inRoleStr %> />
                    <input type="hidden" name="hidRoleName" value="<%=r.Name %>" <%=inRoleStr %> />
                    <input type="hidden" name="hidUserName" value="<%=u.UserName %>" <%=inRoleStr %> />
                    <input type="hidden" name="hidUserRoleChecked" value="true" <%=inRoleStr %> />
                </th>
                   
        <% rowCount1++;} %>
        <%rowNumber= rowNumber+1;} %>      
        </tr>          
         
    </table>
    <br />
    <input type="submit" value="Update" class="button-full button-emerald-green-show" name="UPDATEUSERFROMROLE" />
    <input type="submit" value="Cancel" class="button-full button-emerald-green-show" name="UPDATEUSERFROMROLECancel" />
    </form>
</asp:Content>