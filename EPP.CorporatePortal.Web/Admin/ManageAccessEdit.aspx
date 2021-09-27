<%@ Page Title="" Language="C#" MasterPageFile="~/CorporatePortalSite.Master" AutoEventWireup="true" CodeBehind="ManageAccessEdit.aspx.cs" Inherits="EPP.CorporatePortal.Admin.ManageAccessEdit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/userAccess.css") %>" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-heading-2">
        <asp:HiddenField ID="hdnUsername" Value="" runat="server" />
        <div class="backBtn">
            <a href="ManageAccess.aspx?Username=<%= hdnUsername.Value %>">< Back to User
            </a>
        </div>

        <div class="row-wrap">
            <h1 class="page-title bold">Edit user: <asp:Label runat="server" ID="lblUsername" /></h1>
        </div>

    </div>

    <div class="content-mw tab rmPad">

        <div class="row">


            <div class="col-md-7">
                <div class="contentBox ">

                    <h3 class="formTitle">User details</h3>
                    <hr class="line">

                    <div class="form-group">
                        <div class="row">
                            <div class="col-md-12">
                                        <label class="label" for="txtUName">Username</label>
                                        <asp:TextBox runat="server" ID="txtUName" class="form-control fc" disabled />
                                    </div>
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="row">
                            <div class="col-md-12">
                                <label class="label" for="txtFullName">Fullname</label>
                                <asp:TextBox runat="server" ID="txtFullName" class="form-control fc" disabled />
                            </div>
                        </div>
                    </div>

                    <div class="form-group">
                        <div class="row">
                            <div class="col-md-8">
                                <label class="label" for="txtICNo">Identification no.</label>
                                <asp:TextBox runat="server" ID="txtICNo" class="form-control fc" disabled />
                            </div>
                        </div>
                    </div>

                    <div class="form-group">
                        <div class="row">
                            <div class="col-md-8">
                                <label class="label" for="txtPhoneNo">Mobile no.</label>
                                <asp:TextBox runat="server" ID="txtPhoneNo" class="form-form-control fc " placeholder="0123456789" required="required"></asp:TextBox>
                                <asp:RegularExpressionValidator ID="RegularExpressionValidator2"
                                    ControlToValidate="txtPhoneNo" runat="server"
                                    ErrorMessage="Only Numbers allowed"
                                    ValidationExpression="\d+">
                                </asp:RegularExpressionValidator>
                            </div>
                        </div>
                    </div>

                    <div class="form-group">
                        <div class="row">
                            <div class="col-md-8">
                                <label class="label" for="txtEmailAddress">Email Address</label>
                                <asp:TextBox runat="server" ID="txtEmailAddress" class="form-form-control fc " placeholder="abc@email.com" required="required"></asp:TextBox>
                            </div>
                        </div>
                    </div>

                    <div class="form-group">
                        <div class="row">
                            <div class="col-md-8">
                                <label class="label" for="mail">Gender</label>
                                <div class="switch-field">
                                    <input runat="server" type="radio" id="genderF" name="switch-one" value="F" disabled />
                                    <label for="radio-one">Female</label>
                                    <input runat="server" type="radio" id="genderM" name="switch-one" value="M" disabled />
                                    <label for="radio-two">Male</label>
                                </div>
                            </div>
                        </div>
                    </div>

                </div>
            </div>


        </div>

        <div class="row rowAction">
            <div class="col-md-2">
                <button type="button" class="btn btn-secondary" onclick="location.replace('ManageAccess.aspx?Username=<%= hdnUsername.Value %>')" >Back</button>
            </div>
            <div class="col-md-5">
                <asp:Button runat="server" OnClick="ModifyUser" CssClass="btn btn-primary" Text="Save update" />
            </div>
        </div>

    </div>

    <div class="modal" tabindex="-1" role="dialog" id="popupUpdate">
        <div class="modal-dialog modal-dialog-centered" role="document">
            <div class="modal-content">
                <div class="iconSucces"></div>

                <h3 class="userConfirm">Info has been updated.</h3>


            </div>
        </div>
    </div>

    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/jquery-3.5.1.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/equal-height.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/index.js")%>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/popper.min.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.js") %>"></script>
</asp:Content>
