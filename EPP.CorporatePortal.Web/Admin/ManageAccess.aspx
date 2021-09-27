<%@ Page Title="Manage Access" Language="C#" MasterPageFile="~/CorporatePortalSite.Master" AutoEventWireup="true" CodeBehind="ManageAccess.aspx.cs" Inherits="EPP.CorporatePortal.Admin.ManageAccess" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/userAccess.css") %>" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="modal fade" id="divAddUser">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="exampleModalLabel">Proceed to Create User page?</h5>

                    </div>

                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary v2" data-dismiss="modal">Cancel</button>
                        <button type="button" class="btn btn-primary v2" onserverclick="BtnAddUser_Click" runat="server">Proceed</button>

                    </div>
                </div>
            </div>
        </div>
    
    <div class="page-heading-2">
        <asp:HiddenField ID="hdnUsername" Value="" runat="server" />
        <asp:HiddenField ID="hdnButtonHandler" Value="" runat="server" />
        <div class="row-wrap">
            <h1 class="page-title bold">User Access Management</h1>
        </div>

    </div>

    <div class="content-mw tab">
        <div class="row">
            <div class="col-md-3">
                <asp:LinkButton ID="btnAllStatus" CssClass="btn btnCount" runat="server" OnClick="BtnAllStatus_Click">
                    <div class="countStat">All Status</div>
                    <div class="countStatDetails">
                        <span>
                            <asp:Label runat="server" ID="lblAllStatusCnt" /></span>
                        <span>
                            <img class="goto" src="<%= ResolveUrl("~/Style/assets/icon/icon-goto.svg") %>"></span>
                    </div>
                </asp:LinkButton>
            </div>
            <div class="col-md-3">
                <asp:LinkButton ID="btnActiveStatus" CssClass="btn btnCount" runat="server" OnClick="BtnActiveStatus_Click">
                    <!--Please add css class greenActive when this page is active-->
                    <div class="countStat">Active</div>
                    <div class="countStatDetails">
                        <span>
                            <asp:Label runat="server" ID="lblActiveCnt" /></span>
                        <span>
                            <img class="goto" src="<%= ResolveUrl("~/Style/assets/icon/icon-goto.svg") %>"></span>
                    </div>
                </asp:LinkButton>
            </div>
            <div class="col-md-3 ">
                <asp:LinkButton ID="btnSuspendedStatus" CssClass="btn btnCount" runat="server" OnClick="BtnSuspendedStatus_Click">
                    <!--Please add css class redActive when this page is active-->
                    <div class="countStat">Suspended</div>
                    <div class="countStatDetails">
                        <span><asp:Label runat="server" ID="lblSuspendedCnt" /></span>
                        <span>
                            <img class="goto" src="<%= ResolveUrl("~/Style/assets/icon/icon-goto.svg") %>"></span>
                    </div>
                </asp:LinkButton>
            </div>
        </div>

        <div class="row">
            <div class="col-md-6">
                <asp:TextBox runat="server" ID="txtSearchString" class="form-control fc" placeholder="Search by username, roc or contract id" required="required"></asp:TextBox>
            </div>
            <div class="col-md-1"></div>
            <div class="col-md-1">
                <asp:Button runat="server" OnClick="BtnSearch_Click" CssClass="btn btnAdd" Text="Search" />
            </div>
            <div class="col-md-4">
                <asp:LinkButton ID="BtnAddUser" CssClass="btn btnAdd" runat="server" OnClientClick="return false;" data-toggle="modal" data-target="#divAddUser"><span class="iconAdd"></span><span>Add a user</span></asp:LinkButton>
            </div>
        </div>

        <div class="row">
            <div class="col-md-12">
                <!-- <table id="userList" class="table tableUser"></table> -->

                <table class="table tableUser">
                    <thead>
                        <tr>
                            <th scope="col">User</th>
                            <th scope="col">Company Name</th>
                            <th scope="col">Last Login</th>
                            <th scope="col">Status</th>
                            <th scope="col">Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptUserList" runat="server">
                            <ItemTemplate>
                                <tr onmouseover='hideOther(<%# Eval("Id") %>)'>
                                    <td>
                                        <div class='userName'>
                                            <%# Eval("UserName") %><br>
                                            <span><%# Eval("FullName") %></span>
                                        </div>
                                    </td>
                                    <td>
                                        <span><%# Eval("CorporateSourceId") %></span>
                                    </td>
                                    <td>
                                        <span><%# Eval("LastLoginDate") %></span>
                                    </td>
                                    <td>
                                        <div class='userSuspended'><%# Eval("Status") %></div>
                                    </td>
                                    <td>
                                        <div class='threeDots' onmouseover='view(<%# Eval("Id") %>)'></div>
                                        <ul class='actionList' id="list<%# Eval("Id") %>">
                                            <li onclick='action("edit","<%# Eval("UserName") %>")'>Edit</li>
                                            <li onclick='action("delete","<%# Eval("UserName") %>")'>Delete</li>
                                            <li onclick='action("view","<%# Eval("UserName") %>")'>View</li>
                                            <li onclick='action(<%# Eval("Status").ToString() == "Active" ? "&quot;suspend&quot;" : "&quot;reactivate&quot;" %>,"<%# Eval("UserName") %>")'><%# Eval("Status").ToString() == "Active" ? "Suspend" : "Reactivate" %></li>
                                        </ul>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>


            </div>
        </div>
    </div>

    <!-- Popup Reactivate or Suspend -->

    <div class="modal" tabindex="-1" role="dialog" id="popupAction">
        <div class="modal-dialog modal-dialog-centered" role="document">
            <div class="modal-content">
                <div class="iconNotice"></div>

                <h3 class="userConfirm"><span id="actionTaken"></span>user: <span id="userName"></span>?</h3>
                <p id="subCapt" class="sub center"></p>

                <div class="modal-footer modalAction">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal">Not now</button>
                    <asp:Button runat="server" OnClick="ModalButtonHandler" CssClass="btn btn-primary" Text="Yes" />
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">

        function action(action, userName) {

            $("#<%= hdnUsername.ClientID %>").val(userName);
            $("#userName").html(userName);

            if (action == "reactivate") {
                $('#popupAction').modal('show');
                $("#subCapt").html("Reactivating user account to allow login.")
                $("#actionTaken").html("Reactivate")
                $("#<%= hdnButtonHandler.ClientID %>").val("Reactivate");
            }

            if (action == "suspend") {
                $('#popupAction').modal('show');
                $("#subCapt").html("")
                $("#actionTaken").html("Suspend")
                $("#<%= hdnButtonHandler.ClientID %>").val("Suspend");
            }

            if (action == "delete") {
                $('#popupAction').modal('show');
                $("#subCapt").html("Hello. Cannot be undone, all the records will be deleted.")
                $("#actionTaken").html("Delete")
                $("#<%= hdnButtonHandler.ClientID %>").val("Delete");
            }

            if (action == "view") {
                //redirect to view user
                window.location.replace('ManageAccessView.aspx' + "?Username=" + userName)
            }

            if (action == "edit") {
                //redirect to edit user
                window.location.replace('ManageAccessEdit.aspx' + "?Username=" + userName)
            }
        }

        function view(id) {
            $(".actionList").hide()
            $("#list" + id).show()
        }

        function hideOther(id) {
            $(".actionList:not(#list" + id + ")").hide()
        }
    </script>

    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/jquery-3.5.1.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/equal-height.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/index.js")%>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/popper.min.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.js") %>"></script>
</asp:Content>
