<%@ Page Title="Unauthorized Access" Language="C#" AutoEventWireup="true" CodeBehind="UnAuthorized.aspx.cs" Inherits="EPP.CorporatePortal.UnAuthorized" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h1>Unauthorized Access</h1>
            <a href="javascript:history.back()">Go Back</a>
            <a href="<%= ResolveUrl("~/Login.aspx") %>">Login</a>
        </div>
    </form>
</body>
</html>
