<%@ Page Language="C#" MasterPageFile="~/CorporatePortalSite.Master" Title="Create User" AutoEventWireup="true" CodeBehind="AddUsers.aspx.cs" Inherits="EPP.CorporatePortal.Admin.AddUsers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
     <div class="page-heading ">

                <div class="row-wrap">
                    <h1 class="page-title bold"><%=Resources.Resource.CreateUsers %></h1>

                    <div class="btn-wrapper">
                        <div class="btn btn-primary" data-toggle="modal" data-target="#add-user" style="cursor:pointer">Submit New Users Listing</div>
                    </div>
                </div>

                <!-- popup window -->

                <div class="modal fade show" id="add-user">
                    <div class="modal-dialog modal-dialog-centered" role="document">
                        <div class="modal-content">
                            <div class="modal-header">

                                <div>
                                    <h4 class="modal-title bold">Add New Users listing</h4>
                                    <p class="modal-caption-p">Upload an excel file for adding new users</p>
                                </div>

                            </div>
                            <div class="modal-body">
                                <label class="modal-label">Upload your Excel file</label>
                                <div class="fileUpload btn-file btn width100">
                                    <span>+ Upload now</span>
                                  
                                    <asp:FileUpload ID="ctlFileUpload" runat="server" accept=".xls,.xlst,.xlsx" CssClass="uploadlogo"/>
                                </div>
                            </div>

                            <div class="modal-footer">
                                <button type="button" id="btnCancelUpload" class="btn btn-secondary no-outline"
                                    data-dismiss="modal">Cancel</button>
                                
                                <asp:Button Text="Submit" ID="btnUpload" OnClientClick="this.disabled = true;" OnClick="Upload" UseSubmitBehavior="false" runat="server" CssClass="btn btn-primary-grey" />
                            </div>
                        </div>
                    </div>
                </div>

          

      </div>
      <div class="tab-menu-wrapper">
            <button class="btn-tab tablink btn active" onclick="openTab(event,'new-users')">New Users</button>
       </div>

    <!---------------------- TAB : Submission & Invoice----------------->

            <div id="new-users" class="tab">

                <div class="clear-float"></div>

                <div class="table-responsive">
                    <table class="table table-product" style="min-width:1056px;">
                        <thead>
                            <tr>
                                <th scope="col">Excel File Name</th>
                                <th scope="col">Uploaded Date</th>
                                <th scope="col">Uploaded By</th>
                                <th scope="col">Approved By</th>
                                <th scope="col">Submission Status</th>
                                <th scope="col"></th>
                                <th scope="col"></th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptFileUploadHistory" runat="server">
                                <ItemTemplate>
                                    <tr>
                                <td scope="col"><a href='<%# System.Configuration.ConfigurationManager.AppSettings["RouteURL"].ToString() %>/Application/DownloadFile.ashx?id=<%# Eval("Id").ToString() %>&type=<%# EPP.CorporatePortal.Common.Enums.FileDownloadType.CreateUser.ToString() %>' target="_blank"><%# Eval("FileName") %></i></a></td>
                                <td scope="col"><%# Eval("UploadedDateTime", "{0:dd/MM/yyyy hh:mm:ss tt}") %> </td>
                                <td scope="col"><%# Eval("UploadedBy") %> </td>
                                <td scope="col"><%# Eval("ApprovedBy") %> </td>
                                <td scope="col"><%# Eval("Status") %> </td>
                                <td scope="col" <%# Eval("Status").ToString() == "Completed" || Eval("Status").ToString() == "Error" || Eval("UploadedBy").ToString() == hdnUsername.Value ? "style='opacity: 0.5;pointer-events: none;'" : "" %>><asp:Button Text="Approve" ID="btnApprove" CommandArgument='<%#Eval("Id")%>' CommandName="Approve" OnClientClick="this.disabled = true;" OnClick="Approval" UseSubmitBehavior="false" runat="server" CssClass="btn btn-primary-grey" /></td>
                                <td scope="col" class='download' onmouseover='view("<%# Eval("Id") %>")' onmouseout='hide("<%# Eval("Id") %>")'>Download
                                    <i class='fa fa-chevron-down btn-download'></i>
                                    <div class='download-popup <%# Eval("Id") %>' style='display: none;''>
                                    <ul>
                                    <li>
                                    <p>Exceptional Report</p>
                                    <ul>
                                        <li <%# string.IsNullOrEmpty(Eval("ExceptionFile").ToString()) ? "style='opacity: 0.5;pointer-events: none;'" : "" %>>Download in Excel format <a href='<%# System.Configuration.ConfigurationManager.AppSettings["RouteURL"].ToString() %>/Application/DownloadFile.ashx?id=<%# Eval("Id").ToString() %>&type=<%# EPP.CorporatePortal.Common.Enums.FileDownloadType.CreateUserException.ToString() %>' target="_blank"><i class='fa fa-chevron-right btn-download'></i></a></li>
                                    </ul>
                                    </li>
                                    </ul>
                                    </div>
                                </td>                                
                            </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:HiddenField ID="hdnUsername" value="" runat="server"/>
                        </tbody>
                    </table>
                </div>


            </div>
    <!------------------------ TAB :Find Member--------------------------->

            <div id="find-member" class="tab" style="display:none">
                <h2>Tab2</h2>
                <p>This is Tab 2</p>
            </div>
    <script type="text/javascript">
        function openTab(evt, tab) {

                var i, tablinks;
                var x = document.getElementsByClassName("tab");
                for (i = 0; i < x.length; i++) {
                    x[i].style.display = "none";
                }

                tablinks = document.getElementsByClassName("tablink");
                for (i = 0; i < x.length; i++) {
                    tablinks[i].className = tablinks[i].className.replace(" active", "");
                }

                $("#" + tab).show();
                evt.currentTarget.className += " active";

            }
        function view(popup) {
            $("div.download-popup." + popup).show()
        }

        function hide(popup) {
            $("div.download-popup." + popup).hide()
        }
        
    </script>

    <script type="text/javascript"  src="<%= ResolveUrl("~/Style/assets/vendor/jquery-3.5.1.js") %>"></script>
    <script type="text/javascript"  src="<%= ResolveUrl("~/Style/assets/vendor/equal-height.js") %>"></script>
    <script type="text/javascript"  src="<%= ResolveUrl("~/Style/index.js") %>"></script>
    <script type="text/javascript"  src="<%= ResolveUrl("~/Style/assets/vendor/popper.min.js") %>"></script>
    <script type="text/javascript"  src="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.js") %>"></script>
</asp:Content>