<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="ResetPassword.aspx.cs" Inherits="EPP.CorporatePortal.Shared.ResetPassword" %>

<!DOCTYPE html>
<html lang="en" data-textdirection="ltr" class="loading">

<head>
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=0, minimal-ui">
    <meta name="description" content="Etiqa Partner Portal for Etiqa General Insurance and Takaful">
    <meta name="keywords"
        content="etiqa insurance and takaful, etiqa, insurance, general insurance, takaful, agent insurance, etiqa partner portal">
    <title>EB Portal</title>
    <meta name="apple-mobile-web-app-capable" content="yes">
    <meta name="apple-touch-fullscreen" content="yes">
    <meta name="apple-mobile-web-app-status-bar-style" content="default">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/fontawesome/css/font-awesome.min.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/style.css") %>">
</head>

<body>

    <!-- <div class=""> -->

    <form id="ResetPasswordForm" runat="server">
        <div class="row wrapper">

            <div class="login-bg">

                <img src="<%= ResolveUrl("~/Style/assets/img/logo-white.png") %>" class="logo">
            </div>

            <div class="login-section">

                <div class="login-section-inner-wrapper">

                    <img src="<%= ResolveUrl("~/Style/assets/img/logo.png") %>" class=" logo-mobile">

                    <h3 class="page-title">Can’t remember your password?</h3>
                    <p class="caption">Don't worry, let us help you.</p>

                    <h5 class="bold margin">Enter username to find your account</h5>

                    <span runat="server" id="spanResetStatus" class="error-login" style="display: none;">Sorry, we couldn’t find your account with that information. Try
                    entering it again.</span>

                    <div class="form-group form-gap">
                        <asp:TextBox runat="server" ID="TxtUsername" class="form-control" placeholder="abc@email.com" required="required"></asp:TextBox>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <asp:Button runat="server" OnClick="BtnResetPassword_Click" CssClass="btn btn-warning btn-login" Text="Email me a temporary password" />
                            <div style="text-align: center;"><a class="btn btn-outline-info btn-login" href="<%= ResolveUrl("~/Shared/Logout.aspx") %>">Back to login</a></div>
                        </div>
                    </div>

                    <p class="bold margin">Important Note</p>

                    <p class="foot-note">
                        Use of this system is restricted to individuals and activities authorized by the management of the
                    Etiqa Insurance & Takaful. Unauthorized use may result in the appropriate disciplinary action and/or
                    legal prosecution.               
                    </p>
                </div>
            </div>
        </div>
    </form>

    <!-- </div> -->


</body>




<script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/jquery-3.5.1.js") %>" ></script>
</html>