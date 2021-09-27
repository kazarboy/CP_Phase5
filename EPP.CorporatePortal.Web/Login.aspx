<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="EPP.CorporatePortal.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8"/>
    <meta http-equiv="X-UA-Compatible" content="IE=edge"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=0, minimal-ui"/>
    <meta name="description" content="Etiqa Partner Portal for Etiqa General Insurance and Takaful"/>
    <meta name="keywords"
        content="etiqa insurance and takaful, etiqa, insurance, general insurance, takaful, agent insurance, etiqa partner portal"/>
    <title>Corporate Portal</title>
    <meta name="apple-mobile-web-app-capable" content="yes"/>
    <meta name="apple-touch-fullscreen" content="yes"/>
    <meta name="apple-mobile-web-app-status-bar-style" content="default"/>
    <link href="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.css") %>" rel="stylesheet" type="text/css"/>
    <link rel="icon" type="image/png"  href="<%= ResolveUrl("~/Style/assets/img/logo.png") %>" />
    <link href="<%= ResolveUrl("~/Style/fontawesome/css/font-awesome.min.css") %>" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/style.css") %>"/>
</head>
<body>
    <form id="form1" runat="server">
        <div class="row wrapper">
            <div class="login-bg">                        
                <img src="<%= ResolveUrl("~/Style/assets/img/logo-white.png") %>" class="logo" />
            </div>
            <div class="login-section">
                <div class="login-section-inner-wrapper">                    
                    <img src="<%= ResolveUrl("~/Style/assets/img/logo.png") %>" class="logo-mobile"/>
                 
                   <%-- <span class="languageOption" ><%#Resources.Resource.ChooseLanguage %>: <a  href="/Application/ChangeLanguage.aspx?lang=en-US" title="<%#Resources.Resource.English %>"><%#Resources.Resource.English %></a>  | <a  href="/Application/ChangeLanguage.aspx?lang=ms-MY" title="<%#Resources.Resource.Malay %>"><%#Resources.Resource.Malay %></a></span>
                  --%>
                    <h3 class="page-title"><%#Resources.Resource.LoginPageGreeting %><span class="higlight"> Corporate Portal</span></h3>
                    <p class="caption"><%#Resources.Resource.LoginGreeting %></p>

                    <h5 class="bold margin" ><%#Resources.Resource.LoginToYourAccount %></h5>
                    <span runat="server" id="spanLoginStatus" class="error-login" style="display:none;"><%#Resources.Resource.LoginErrorMsg%></span>

                    <div class="form-group form-gap">
                       <asp:TextBox runat="server" ID="txtUserName" ToolTip="<%#Resources.Resource.UserName %>" class="form-control" placeholder="<%#Resources.Resource.UserName %>" required="required" oninvalid="<%#Resources.Resource.SetCustomValidityUserName %>" oninput="this.setCustomValidity('')" AutoCompleteType="Disabled"></asp:TextBox>
                    </div>
                     <div class="form-group">
                       <asp:TextBox runat="server" TextMode="Password" ID="txtPassword" class="form-control" ToolTip="<%#Resources.Resource.Password %>" placeholder="<%#Resources.Resource.Password %>"  required="required" oninvalid="<%#Resources.Resource.SetCustomValidityPassword %>" oninput="this.setCustomValidity('')"></asp:TextBox>
                    </div>
                    <%--<p class="forgot-username"><a runat="server" href="ForgotPassword.aspx"><asp:Label runat="server" Text="<%#Resources.Resource.ForgotPasswordText %>"></asp:Label></a></p>--%>
                    <div class="row">
                        <div class="col-md-6">
                            <asp:Button runat="server" OnClick="BtnLogin_Click" CssClass="btn btn-warning btn-login" ToolTip="<%#Resources.Resource.Login %>" Text="<%#Resources.Resource.Login %>" />
                        </div>                        
                    </div>
                    <div class="form-group form-gap">
                        <a href="<%# ResolveUrl("~/Shared/ResetPassword.aspx") %>" title="Forgot Password">Forgot Password</a>
                    </div>
                    <p class="bold margin"><%#Resources.Resource.ImportantNoteTitle %></p>
                    <p class="foot-note">
                        <%#Resources.Resource.ImportantNote %>
                    </p>
                </div>
            </div>
        <//div>
    </form> 
</body>
</html>


