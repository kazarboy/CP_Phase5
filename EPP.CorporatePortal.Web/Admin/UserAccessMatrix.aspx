<%@ Page Title="User Access Matrix" Language="C#" MasterPageFile="~/CorporatePortalSite.Master" AutoEventWireup="true" CodeBehind="UserAccessMatrix.aspx.cs" Inherits="EPP.CorporatePortal.Admin.UserAccessMatrix" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/userAccess.css") %>" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">    
    <div class="page-heading-2">
        <asp:HiddenField ID="hdnUsername" Value="" runat="server" />
        <asp:HiddenField ID="hdnButtonHandler" Value="" runat="server" />
        <div class="row-wrap">
            <h1 class="page-title bold">User Access Matrix</h1>

            <div class="btn-wrapper">
               <!-- 20210726 - added UAM Report Generation-->
                <button type="button" class="btn btn-primary" onserverclick="GenerateUAMReport" style="cursor:pointer;margin-left:10px;" runat="server">Download UAM Report</button>
            </div>
            <div class="btn-wrapper">
                <button type="button" class="btn btn-primary" data-toggle="modal" data-target="#generate-report" style="cursor:pointer">Download Report Listing</button>
            </div>
        </div>

        <!-- popup window -->

        <div class="modal fade show" id="generate-report">
            <div class="modal-dialog modal-dialog-centered" role="document">
                <div class="modal-content">
                    <div class="modal-header">

                        <div>
                            <h4 class="modal-title bold">Generate Report</h4>
                            <p class="modal-caption-p">Generate report according to output format chosen</p>
                        </div>

                    </div>
                    <div class="modal-body">
                        <label class="modal-label">Output Format</label>
                        <div class="dropdown-wrapper dropdown-member">

                            <span class="custom-dropdown full-width">
                                <select id="selectGenerateReportFormat" runat="server" clientidmode="Static">
                                    <option value="CSV">CSV</option>
                                    <option value="PDF">PDF</option>
                                </select>
                            </span>

                        </div>
                    </div>

                    <div class="modal-footer">
                        <button type="button" id="btnCancelGenerate" class="btn btn-secondary no-outline"
                            data-dismiss="modal">Cancel</button>
                        <asp:Button Text="Generate" ID="btnGenerateReport" OnClick="GenerateReport" UseSubmitBehavior="false" runat="server" CssClass="btn btn-primary-grey" data-dismiss="modal"/>
                    </div>
                </div>
            </div>
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
                <asp:TextBox runat="server" ID="txtSearchString" class="form-control fc" placeholder="Search by username, roc or contract id"></asp:TextBox>
            </div>
            <div class="col-md-1"></div>
            <div class="col-md-1">
                <asp:Button runat="server" OnClick="BtnSearch_Click" CssClass="btn btnAdd" Text="Search" />
            </div>
            <div class="col-md-4">
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
                            <th scope="col">Role</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptUserList" runat="server">
                            <ItemTemplate>
                                <tr>
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
                                        <span><%# Eval("RoleName") %></span>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>


            </div>
        </div>
    </div>

    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/jquery-3.5.1.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/equal-height.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/index.js")%>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/popper.min.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.js") %>"></script>
</asp:Content>
