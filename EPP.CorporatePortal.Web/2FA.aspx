<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="2FA.aspx.cs" Inherits="EPP.CorporatePortal._2FA" %>

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

                    <h5 class="bold margin" ><%#Resources.Resource.FACode %></h5>
                    <span runat="server" id="spanFACodeLoginStatus" class="error-login" style="display:none;"><%#Resources.Resource.FACodeErrorMsg%></span>
                    <span runat="server" id="span1" class="error-login" style="display:none;"><%#Resources.Resource.FACodeTimeOutErrorMsg%></span>
                    <span runat="server" id="span4" class="error-login" style="display:none;"><%#Resources.Resource.FACodeWrongOTPMsg%></span>

                    <div class="form-group form-gap">
                       <asp:TextBox runat="server" ID="txtFACode"  class="form-control" TextMode="Password"></asp:TextBox>
                   </div>
                    <asp:HiddenField  ClientIDMode="Static" ID="hdnUserName" value="" runat="server"/>
          
                    <%--<div class="row form-gap">--%>
                        <span runat="server" id="span2" class="error-login"><%#Resources.Resource.FACodesend%> </span>
                        <span runat="server" id="span3" class="error-login" style="display:none;"><%#Resources.Resource.FACodeResend%></span><span class="error-login" runat="server" id="FaCodesend"></span><span class="error-login" runat="server" id="Span5"> This code only valid for 2 minutes.</span>
                         <asp:LinkButton  runat="server" OnClick="BtnResend_Click" Text="Resend" Font-Underline="True" Font-Bold="True" ForeColor="blue"/>
                    <%--</div>--%>

                    <div class="row form-gap">
                         <div class=" col-md-4">
                            <asp:Button runat="server" OnClick="BtnSubmit_Click" CssClass="btn btn-warning btn-login" Text="SUBMIT"/>
                        </div> 
                        <div class="offset-md-4 col-md-4">
                            <asp:Button runat="server" OnClick="BtnCancel_Click" CssClass="btn btn-warning btn-login" Text="CANCEL"/>
                        </div>        
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


