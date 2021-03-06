<%@ Page Language="C#" MasterPageFile="~/CorporatePortalSite.Master" Title="Member Listing" AutoEventWireup="true" CodeBehind="MemberListing.aspx.cs" Inherits="EPP.CorporatePortal.Application.MemberListing" %>
<%@ Import namespace="EPP.CorporatePortal.Common" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
     <div class="page-heading ">

                <div class="row-wrap">
                    <%--  --%>
                    <h1 class="page-title bold"><%=Resources.Resource.MemberListing %></h1>
                     <asp:HiddenField ID="hdnPolicyChecker" ClientIDMode="Static" value="" runat="server"/>

                    <div class="btn-wrapper">
                           <%--<div class="btn btn-primary tooltip" onclick="DLTemplate" style="cursor:pointer;margin-left:10px;"><i class="fa fa-download" style="font-size:24px;"></i><span class="tooltiptext">Download Latest Template</span></div>--%>
                               <a class="btn btn-primary tooltip" style="cursor:pointer;margin-left:10px;" href="<%= DLTemplate %>" download><i class="fa fa-download" style="font-size:24px;"></i><span class="tooltiptext">Download Latest Template</span></a>
                         </div>
                    <div class="btn-wrapper">
                            <div class="btn btn-primary" onclick="MemberListModal()" style="cursor:pointer">Submit New
                            Member/ Endorsement Listing</div>
                    </div>
                </div>

                <!-- popup window -->

                <div class="modal fade show" id="update-member">
                    <div class="modal-dialog modal-dialog-centered" role="document">
                        <div class="modal-content">
                            <div class="modal-header">

                                <div>
                                    <h4 class="modal-title bold">Update member listing</h4>
                                    <p class="modal-caption-p">Upload an excel file for update new member or endorsement
                                        request</p>
                                </div>

                            </div>
                            <div class="modal-body">

                                <label class="modal-label">For Policy</label>
                                <div class="dropdown-wrapper dropdown-member">

                                    <span class="custom-dropdown full-width">
                                        <select runat="server" id="ddlProduct">
                                             
                                        </select>
                                    </span>

                                </div>

                                <label class="modal-label">Upload your Excel file</label>
                                <div class="fileUpload btn-file btn width100">
                                    <span>+ Upload now</span>
                                  
                                    <asp:FileUpload ID="ctlFileUpload" runat="server" accept=".xls,.xlst,.xlsx" CssClass="uploadlogo"/>
                                </div>

                            </div>

                            <div class="modal-footer">
                              <%--  <button type="button" id="btnCancelUpload" class="btn btn-secondary no-outline"
                                    data-dismiss="#update-member">Cancel</button>--%>
                                    <button type="button" id="btnCancelUpload" class="btn btn-secondary no-outline"
                                    onclick ="$('#update-member').modal('hide');">Cancel</button>
                                
                                <%--<asp:Button Text="Submit" ID="btnUpload" OnClientClick="this.disabled = true;document.getElementById('btnCancelUpload').setAttribute('disabled','disabled');" OnClick="Upload" UseSubmitBehavior="false" runat="server" CssClass="btn btn-primary-grey" />--%>
                                <asp:Button Text="Submit" ID="btnUpload" OnClientClick="this.disabled = true;" OnClick="Upload" UseSubmitBehavior="false" runat="server" CssClass="btn btn-primary-grey" />
                            </div>
                        </div>
                    </div>
                </div>

         <!--Notification-->
         <div class="modal fade show" id="Notification">
                    <div class="modal-dialog modal-dialog-centered" role="document">
                        <div class="modal-content">
                            <div class="modal-header">

                                <div>
                                    <h4 class="modal-title bold">Notification</h4>
                                </div>

                            </div>
                            <div class="modal-body">
                                <p>Please Upload GHI/GWH Endorsement Before Uploading The OPC/OPW Endorsement.</p>
                            </div>

                            <div class="modal-footer">
                                    <button type="button" id="btnClose" class="btn btn-primary no-outline"
                                    onclick ="$('#Notification').modal('hide');">Proceed</button>          
                            </div>
                        </div>
                    </div>
                </div>
          

      </div>
      <div class="tab-menu-wrapper">
            <button class="btn-tab tablink btn active" onclick="openTab(event,'submission-invoice')">Submission &
            Invoice</button>
            <%--<button class="btn-tab tablink btn" onclick="openTab(event,'find-member')">Find Member</button>
            <button class="btn-tab tablink btn"
            onclick="openTab(event,'underwriting-deferment-member')">Underwriting Deferment Member</button>--%>
       </div>

    <!---------------------- TAB : Submission & Invoice----------------->

            <div id="submission-invoice" class="tab">

                <%--<div class="dropdown-wrapper float-left dropdown-product">
                    <label>Filter by product</label>

                    <span class="custom-dropdown">
                        <select>
                            <option value="group-term-takaful">Group Term Takaful</option>
                        </select>
                    </span>

                </div>

                <div class="dropdown-wrapper float-right dropdown-product">
                    <label>Sort by</label>

                    <span class="custom-dropdown">
                        <select>
                            <option value="latest">Latest</option>
                        </select>
                    </span>

                </div>--%>

                <div class="clear-float"></div>

                <div class="table-responsive">
                    <table class="table table-product" style="min-width:1056px;">
                        <thead>
                            <tr>
                                <%--<th scope="col">Policy</th>--%>
                                <th scope="col">Excel File Name</th>
                                <th scope="col">Uploaded Date</th>
                                <th scope="col">Submission Status</th>
                                <th scope="col">Invoice No.</th>
                                <th scope="col">Invoice Date</th>
                                <th scope="col">Billing Amount</th>
                                <th scope="col">Invoice Status</th>
                                <th scope="col"></th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptFileUploadHistory" runat="server">
                                <ItemTemplate>
                                    <tr>
                               <%-- <td scope="col"><%# Eval("PolicyId") %> </td>--%>                                        
                                <td scope="col"><a href='DownloadFile.ashx?id=<%# Eval("Id").ToString() %>&type=<%# EPP.CorporatePortal.Common.Enums.FileDownloadType.UploadFile.ToString() %>' target="_blank"><%# Eval("FileName") %></i></a></td>
                                <%--<td scope="col"><a href="javascript:DownloadOriginalFile('<%# Eval("FileName") %>');"><%# Eval("FileName") %></a></td>--%>
                                <td scope="col"><%# Eval("UploadedDateTime", "{0:dd/MM/yyyy hh:mm:ss tt}") %> </td>
                                <td scope="col"><%# Eval("Status") %> </td>
                                <td scope="col"><%# Eval("InvoiceNo") %> </td>
                                <td scope="col"><%# Eval("InvoiceDate", "{0:dd/MM/yyyy hh:mm:ss tt}") %> </td>
                                <td scope="col"><%# Eval("ContributionAmt") %> </td>
                                <td scope="col"><%# Eval("InvoiceStatus") %> </td>
                                <td scope="col" class='download' onmouseover='view("<%# Eval("Id") %>")' onmouseout='hide("<%# Eval("Id") %>")'>Download
                                    <i class='fa fa-chevron-down btn-download'></i>
                                    <div class='download-popup <%# Eval("Id") %>' style='display: none;''>
                                    <ul>
                                    <li>
                                    <p>Invoice</p>
                                    <ul>
                                        <li <%# string.IsNullOrEmpty(Eval("InvoiceFile").ToString()) ? "style='opacity: 0.5;pointer-events: none;'" : "" %>>Download in PDF format <a href='DownloadFile.ashx?id=<%# Eval("Id").ToString() %>&type=<%# EPP.CorporatePortal.Common.Enums.FileDownloadType.Invoice.ToString() %>' target="_blank"><i class='fa fa-chevron-right btn-download'></i></a></li>
                                        <%--<li <%# string.IsNullOrEmpty(Eval("InvoiceLink").ToString()) ? "style='opacity: 0.5;pointer-events: none;'" : "" %>>Download in PDF format <asp:LinkButton ID="LinkButtonDownloadInvoice" runat="server" CommandName='<%# EPP.CorporatePortal.Common.Enums.FileDownloadType.Invoice.ToString() %>' CommandArgument='<%# Eval("Id").ToString() %>' OnCommand="DownloadFile"><i class='fa fa-chevron-right btn-download'></i></asp:LinkButton></li>--%>
                                    <%--<%# !string.IsNullOrEmpty(Eval("InvoiceLink").ToString()) ? "<li>Download in PDF format <a href='" + Eval("InvoiceLink") + "'><i class='fa fa-chevron-right btn-download'></i></a></li>" : "<li style='opacity: 0.5;pointer-events: none;'>Download in PDF format <i class='fa fa-chevron-right btn-download'></i></li>" %>--%>
                                    </ul>
                                    </li>
                                    <li>
                                    <p>Exceptional Report</p>
                                    <ul>
                                        <li <%# string.IsNullOrEmpty(Eval("ExceptionFile").ToString()) ? "style='opacity: 0.5;pointer-events: none;'" : "" %>>Download in Excel format <a href='DownloadFile.ashx?id=<%# Eval("Id").ToString() %>&type=<%# EPP.CorporatePortal.Common.Enums.FileDownloadType.Exception.ToString() %>' target="_blank"><i class='fa fa-chevron-right btn-download'></i></a></li>
                                        <%--<li <%# string.IsNullOrEmpty(EPP.CorporatePortal.Models.Utility.ExceptionLinksHandling(Eval("ExceptionLink").ToString(), Eval("CGLSExceptionLink").ToString(), Eval("RPAExceptionLink").ToString())) ? "style='opacity: 0.5;pointer-events: none;'" : "" %>>Download in Excel format <asp:LinkButton ID="LinkButtonDownloadException" runat="server" CommandName='<%# EPP.CorporatePortal.Common.Enums.FileDownloadType.Exception.ToString() %>' CommandArgument='<%# Eval("Id").ToString() %>' OnCommand="DownloadFile"><i class='fa fa-chevron-right btn-download'></i></asp:LinkButton></li>--%>
                                    <%--<%# ExceptionLinksHandling(Eval("ExceptionLink").ToString(), Eval("CGLSExceptionLink").ToString(), Eval("RPAExceptionLink").ToString()) %>--%>
                                    <%--<%# !string.IsNullOrEmpty(Eval("ExceptionLink").ToString()) ? "<li>Download in Excel format <a href='" + Eval("ExceptionLink") + "'><i class='fa fa-chevron-right btn-download'></i></a></li>" : "<li style='opacity: 0.5;pointer-events: none;'>Download in Excel format <i class='fa fa-chevron-right btn-download'></i></li>" %>--%>
                                    </ul>
                                    </li>
                                    <%--<li>
                                    <p>Uploaded Member Listing</p>
                                    <ul>
                                    <li>Download in Excel format <i class='fa fa-chevron-right btn-download'></i></li>
                                    </ul>
                                    </li>--%>
                                    </ul>
                                    </div>
                                </td>
                            </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:HiddenField  ClientIDMode="Static" ID="hdnROC" value="" runat="server"/>
                        </tbody>
                    </table>
                </div>


            </div>
    <!------------------------ TAB :Find Member--------------------------->

            <div id="find-member" class="tab" style="display:none">
                <h2>Paris</h2>
                <p>Paris is the capital of France.</p>
            </div>


            <!-------------- TAB : Underwriting Deferment Member ----------------->


            <div id="underwriting-deferment-member" class="tab" style="display:none">
                <h2>Tokyo</h2>
                <p>Tokyo is the capital of Japan.</p>
            </div>
    
    <script type="application/javascript">
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

        function MemberListModal() {
            $("#update-member").modal('show');

            if (document.getElementById("hdnPolicyChecker").value == 'True') 
            {    
                $("#Notification").modal('show');
            }
            
        }

</script>
<script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/jquery-3.5.1.js") %>"></script>
<script type="text/javascript"  src="<%= ResolveUrl("~/Style/assets/vendor/equal-height.js") %>"></script>
<script type="text/javascript"  src="<%= ResolveUrl("~/Style/index.js")%>"></script>
<script type="text/javascript"  src="<%= ResolveUrl("~/Style/assets/vendor/popper.min.js") %>"></script>
<script type="text/javascript"  src="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.js") %>"></script>
</asp:Content>
