<%@ Page Language="C#" Title="Manage RoleRights" AutoEventWireup="true" CodeBehind="ManageRoleRights.aspx.cs" Inherits="EPP.CorporatePortal.Admin.ManageRoleRights" %>
<%
    
    var Authorization = new  EPP.CorporatePortal.Service.UserService();
     var roles = Authorization.GetAllRoles();
    var rights = Authorization.GetAllRights().OrderBy(r => r.Name);
    
    
    %>
<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body> 
    <form  method="post" action="AdminActions.aspx"  >
     <input type="hidden" name="hidFormName" value="AssignRoleRights"/>
    <table style="width:100%" border="1">
        <tr>
            <th>#</th>
            <th>Rights/Role</th>
            <%
                var roleId = new int();
                foreach (var r in Authorization.GetAllRoles().OrderBy(r => r.Name))
                {
                    roleId = r.Id;
            %>

                <th title="<%=r.Description %>"><%=r.Name %></th>
            <%} %>
            
        </tr>
        <%
            var rowCount1 = 0;
            var rowNumber = 1;
        %>
        <%
            foreach(var right in Authorization.GetAllRights().OrderBy(r => r.Name))
            { %>
                <tr>
                <th><%=rowNumber%></th>

                <td><%=right.Description.ToString()%> </td>

                <%
                    var roleRights = Authorization.GetRoleRightsUsingRightID(right.Id).OrderBy(rr1 => rr1.RoleName);
                    foreach (var roleRight in roleRights)
                    {
                        var chked =   roleRight.IsChecked ? true : false;
                        var chkedStr = chked == true ? "Checked" : "";
                        var name = String.Concat("chkRoleRights", ".", rowCount1);
                        var title = roleRight.RightsName + "for " + roleRight.RoleDescription;
                %>
                <th title="<%=title%>">                      
                    <input type="checkbox" name="<%=name%>" value="true" <%=chkedStr %> />
                    <input type="hidden" name="hidRightsID" value="<%=roleRight.RightID%>" <%=chkedStr %> />
                    <input type="hidden" name="hidRightsName" value="<%=roleRight.RightsName %>" <%=chkedStr %> />
                    <input type="hidden" name="hidRoleID" value="<%=roleRight.RoleID %>" />
                    <input type="hidden" name="hidRoleName" value="<%=roleRight.RoleName %>" />
                    <input type="hidden" name="hidRoleChecked" value="true" <%=roleRight.IsChecked %> />
                </th>
                   
        <% rowCount1++;} %>
        <%rowNumber= rowNumber+1;} %>      
        </tr>          
         
    </table>
    <br />
    <input type="submit" value="Update" class="button-full button-emerald-green-show" name="UPDATEROLERIGHTS" />
    <input type="submit" value="Cancel" class="button-full button-emerald-green-show" name="UPDATEROLERIGHTSCancel" />
    </form>
</body>
</html>
