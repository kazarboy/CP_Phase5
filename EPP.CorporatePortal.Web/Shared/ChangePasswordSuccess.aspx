<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="ChangePasswordSuccess.aspx.cs" Inherits="EPP.CorporatePortal.Shared.ChangePasswordSuccess" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=0, minimal-ui" />
    <meta name="description" content="Etiqa Partner Portal for Etiqa General Insurance and Takaful" />
    <meta name="keywords"
        content="etiqa insurance and takaful, etiqa, insurance, general insurance, takaful, agent insurance, etiqa partner portal" />
    <title>Corporate Portal</title>
    <meta name="apple-mobile-web-app-capable" content="yes" />
    <meta name="apple-touch-fullscreen" content="yes" />
    <meta name="apple-mobile-web-app-status-bar-style" content="default" />
    <link href="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.css") %>" rel="stylesheet" type="text/css"/>
    <link rel="icon" type="/image/png" href="<%= ResolveUrl("/Style/assets/img/logo.png") %>" />
    <link href="<%= ResolveUrl("~/Style/fontawesome/css/font-awesome.min.css") %>" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/style.css") %>" />
</head>
<body>


    <form id="form1" runat="server">
        <div class="row wrapper">
            <div class="login-bg">
                <img src="<%= ResolveUrl("~/Style/assets/img/logo-white.png") %>" class="logo" />
            </div>
            <div class="login-section">
                <div class="login-section-inner-wrapper">

                    <img src="<%= ResolveUrl("~/Style/assets/img/logo.png") %>" class="logo-mobile" />

                    <div class="success-icon">
                        <img src="<%= ResolveUrl("~/Style/assets/icon/tick.svg") %>" style="width: 100%;" />
                    </div>

                    <h3 class="page-title pw-updated">Password updated</h3>
                    <p class="center">you will be redirected in 3 seconds..</p>


                </div>
            </div>
        </div>
    </form>


</body>

<script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/jquery-3.5.1.js") %>"></script>

</html>
