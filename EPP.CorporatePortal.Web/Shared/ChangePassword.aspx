<%@ Page Title="" Language="C#" AutoEventWireup="true" CodeBehind="ChangePassword.aspx.cs" Inherits="EPP.CorporatePortal.Shared.ChangePassword" %>

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
    <link rel="icon" type="/image/png" href="<%= ResolveUrl("~/Style/assets/img/logo.png") %>" />
    <link href="<%= ResolveUrl("~/Style/fontawesome/css/font-awesome.min.css") %>" rel="stylesheet" type="text/css" />
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/style.css") %>" />
</head>
<body>
    
    <form id="ResetPasswordForm" runat="server">
        <asp:HiddenField ID="hdnUsername" Value="" runat="server" />
        <asp:HiddenField ID="hdnPasswordTally" Value="False" runat="server" />
        <asp:HiddenField ID="hdnPasswordCheck" Value="" runat="server" />
        <div class="row wrapper">
            <div class="login-bg">
                <img src="<%= ResolveUrl("~/Style/assets/img/logo-white.png") %>" class="logo" />
            </div>
            <div class="login-section">
                <div class="login-section-inner-wrapper">
                    <img src="<%= ResolveUrl("~/Style/assets/img/logo.png") %>" class="logo-mobile" />
                    <h3 class="page-title">Your password has expired</h3>
                    <p class="caption" style="margin-top: 0px">Please reset your password below</p>

                    <br />
                    <span runat="server" id="spanResetStatus" class="error-login" style="display: none;"></span>
                    <label>Current Password</label>
                    <div class="input-group mb-3">

                        <asp:TextBox runat="server" TextMode="Password" ID="TxtCurrPassword" class="form-control" placeholder="Pas$word" required="required"></asp:TextBox>
                        <div class="input-group-append">
                            <button class="btn btn-outline-secondary" type="button" id="unhideCurrPw" onclick="UnhideCurrPw();">
                                <img src="<%= ResolveUrl("~/Style/assets/icon/eyelash.svg") %>" style="width: 20px;" />
                            </button>
                        </div>

                    </div>

                    <label>New Password</label>
                    <div class="input-group mb-3">

                        <asp:TextBox runat="server" TextMode="Password" ID="TxtNewPassword" class="form-control" placeholder="Pas$word" required="required" onkeyup="CheckPasswordStrength()"></asp:TextBox>
                        <div class="input-group-append">
                            <button class="btn btn-outline-secondary" type="button" id="unhideNewPw" onclick="UnhideNewPw()">
                                <img src="<%= ResolveUrl("~/Style/assets/icon/eyelash.svg") %>" style="width: 20px;" />
                            </button>
                        </div>

                    </div>

                    <label>Retype your new password</label>
                    <div class="input-group mb-3">

                        <asp:TextBox runat="server" TextMode="Password" ID="TxtConfirmNewPassword" class="form-control" placeholder="Pas$word" required="required" onblur="CheckIfPasswordTally()"></asp:TextBox>
                        <div class="input-group-append">
                            <button class="btn btn-outline-secondary" type="button"
                                id="unhideConfirmNewPw" onclick="UnhideConfirmNewPw()">
                                <img src="<%= ResolveUrl("~/Style/assets/icon/eyelash.svg") %>" style="width: 20px;" />
                            </button>
                        </div>

                    </div>

                    <span class="red pw-not-match">Password doesn't match!</span>

                    <label>Password must</label>

                    <!-- add class .req-checked to the list if requirement matched-->
                    <ul class="pw-requirement">
                        <li>not contain your username</li>
                        <li>contain one number</li>
                        <li>contain one uppercase alphabet</li>
                        <li>contain one lowercase alphabet</li>
                        <li>contain one special character: ~`!@#$%^&*(){}+=-</li>
                        <li>contain at least 8 characters</li>
                    </ul>
                    <div class="row">
                        <div class="col-md-12">
                            <div>
                                <asp:Button runat="server" OnClientClick="return CheckPasswordStatus();" OnClick="BtnResetPassword_Click" CssClass="btn btn-warning btn-login" Text="RESET MY PASSWORD" />
                            </div>
                            <br />
                            <div style="text-align: center;"><a href="<%= ResolveUrl("~/Shared/Logout.aspx") %>" onclick="return confirm('Are you sure you want to cancel?')">Cancel</a></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </form>


</body>

<script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/jquery-3.5.1.js") %>" ></script>
<script type="text/javascript">
    function CheckPasswordStrength() {
        var password = $("#TxtNewPassword").val();
        //Regular Expressions
        var regex = new Array();
        regex.push("[A-Z]"); //For Uppercase Alphabet
        regex.push("[a-z]"); //For Lowercase Alphabet
        regex.push("[0-9]"); //For Numeric Digits
        regex.push("[~`!@#$%^&*(){}+=-]"); //For Special Characters
        var passed = 0;
        //Validation for each Regular Expression. 4 checks
        for (var i = 0; i < regex.length; i++) {
            if ((new RegExp(regex[i])).test(password)) {
                switch (i) {
                    case 0:
                        $(".pw-requirement").children("li").eq(2).addClass("req-checked");
                        break;
                    case 1:
                        $(".pw-requirement").children("li").eq(3).addClass("req-checked");
                        break;
                    case 2:
                        $(".pw-requirement").children("li").eq(1).addClass("req-checked");
                        break;
                    case 3:
                        $(".pw-requirement").children("li").eq(4).addClass("req-checked");
                        break;
                }
                passed++;
            }
            else {
                switch (i) {
                    case 0:
                        $(".pw-requirement").children("li").eq(2).removeClass("req-checked");
                        break;
                    case 1:
                        $(".pw-requirement").children("li").eq(3).removeClass("req-checked");
                        break;
                    case 2:
                        $(".pw-requirement").children("li").eq(1).removeClass("req-checked");
                        break;
                    case 3:
                        $(".pw-requirement").children("li").eq(4).removeClass("req-checked");
                        break;
                }
            }
        }

        //Validation for Length of Password 1 checks
        if (password.length > 7) {
            $(".pw-requirement").children("li").eq(5).addClass("req-checked");
            passed++;
        }
        else {
            $(".pw-requirement").children("li").eq(5).removeClass("req-checked");
        }

        //Validation for username usage 1 checks
        if ($("#TxtNewPassword").val().indexOf($("#hdnUsername").val()) == -1) {
            $(".pw-requirement").children("li").eq(0).addClass("req-checked");
            passed++;
        }
        else {
            $(".pw-requirement").children("li").eq(0).removeClass("req-checked");
        }

        $("#hdnPasswordCheck").val(passed);
    }

    function CheckPasswordStatus() {

        if ($("#hdnPasswordCheck").val() < 6 || $("#hdnPasswordTally").val() == "False") {
            alert("Password not complex enough");
            return false;
        }
        else {
            return true;
        }

    }

    function CheckIfPasswordTally() {

        if ($("#TxtConfirmNewPassword").val() != $("#TxtNewPassword").val()) {
            $("span.pw-not-match").show();
            $("#hdnPasswordTally").val("False");
        }

        else {
            $("span.pw-not-match").hide();
            $("#hdnPasswordTally").val("True");
        }

    }

    function UnhideCurrPw() {

        var x = document.getElementById("TxtCurrPassword");

        if (x.type === "password") {
            x.type = "text";
            $("#unhideCurrPw > img").attr("src", "<%= ResolveUrl("~/Style/assets/icon/eyes.svg") %>");
        }

        else {
            x.type = "password";
            $("#unhideCurrPw > img").attr("src", "<%= ResolveUrl("../Style/assets/icon/eyelash.svg") %>");
        }

    }

    function UnhideConfirmNewPw() {

        var x = document.getElementById("TxtConfirmNewPassword");

        if (x.type === "password") {
            x.type = "text";
            $("#unhideConfirmNewPw > img").attr("src", "<%= ResolveUrl("../Style/assets/icon/eyes.svg") %>");
        }

        else {
            x.type = "password";
            $("#unhideConfirmNewPw > img").attr("src", "<%= ResolveUrl("../Style/assets/icon/eyelash.svg") %>");
        }
    }


    function UnhideNewPw() {

        var x = document.getElementById("TxtNewPassword");

        if (x.type === "password") {
            x.type = "text";
            $("#unhideNewPw > img").attr("src", "<%= ResolveUrl("../Style/assets/icon/eyes.svg") %>");
        }

        else {
            x.type = "password";
            $("#unhideNewPw > img").attr("src", "<%= ResolveUrl("../Style/assets/icon/eyelash.svg") %>");
        }

    }
</script>
</html>
