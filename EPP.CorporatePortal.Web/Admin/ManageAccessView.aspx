<%@ Page Title="" Language="C#" MasterPageFile="~/CorporatePortalSite.Master" AutoEventWireup="true" CodeBehind="ManageAccessView.aspx.cs" Inherits="EPP.CorporatePortal.Admin.ManageAccessView" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/userAccess.css") %>" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-heading-2">
        <asp:HiddenField ID="hdnUsername" Value="" runat="server" />
        <div class="backBtn">
            <a href="ManageAccess.aspx?Username=<%= hdnUsername.Value %>">< Back to User</a>
        </div>

        <div class="row-wrap">
            <h1 class="page-title bold">View user:
                <asp:Label runat="server" ID="lblUsername1" /></h1>
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
                                <label class="label" for="uName">Username</label>
                                <p id="pUsername" class="displayDetail" runat="server" />
                            </div>
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="row">
                            <div class="col-md-12">
                                <label class="label" for="formGroupExampleInput2">Fullname</label>
                                <p id="pFullName" class="displayDetail" runat="server" />
                            </div>
                        </div>
                    </div>

                    <div class="form-group">
                        <div class="row">
                            <div class="col-md-8">
                                <label class="label" for="formGroupExampleInput3">Identification no.</label>
                                <p id="pICNo" class="displayDetail" runat="server" />
                            </div>
                        </div>
                    </div>

                    <div class="form-group">
                        <div class="row">
                            <div class="col-md-12">
                                <label class="label" for="formGroupExampleInput5">Email Address</label>
                                <p id="pEmailAddress" class="displayDetail" runat="server" />
                            </div>

                        </div>
                    </div>

                    <div class="form-group">
                        <div class="row">
                            <div class="col-md-12">
                                <label class="label" for="formGroupExampleInput4">Mobile no.</label>
                                <p id="pMobileNo" class="displayDetail" runat="server" />
                            </div>

                        </div>
                    </div>

                    <div class="form-group">
                        <div class="row">
                            <div class="col-md-8">
                                <label class="label" for="mail">Gender</label>
                                <p id="pGender" class="displayDetail" runat="server" />
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
                <button type="button" class="btn btn-primary" onclick="location.replace('ManageAccessEdit.aspx?Username=<%= hdnUsername.Value %>')" >Edit user</button>
                <asp:Button runat="server" OnClick="DeleteUser" CssClass="btn btn-third" Text="Delete user" />
                <asp:Button runat="server" id="btnSuspendActive" OnClick="SuspendUser" CssClass="btn btn-third" Text="Suspend user" />
            </div>
        </div>

    </div>
</asp:Content>
