<%@ Page Title="Audit Log" Language="C#" MasterPageFile="~/CorporatePortalSite.Master" AutoEventWireup="true" CodeBehind="AuditLog.aspx.cs" Inherits="EPP.CorporatePortal.Admin.AuditLog" %>
<%@ Import namespace="EPP.CorporatePortal.Models" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/claim.css") %>" />
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/assets/vendor/bootstrap-datepicker.min.css") %>" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField   ClientIDMode="Static" ID="hdnCorpId" value="" runat="server"/>
    <div class="page-heading ">

        <div class="row-wrap">
            <h1 class="page-title bold"><%=Resources.Resource.AuditTrail %></h1>

            <div class="btn-wrapper">
                <%--<div class="btn btn-secondary"  style="cursor:pointer">Download QC Listing</div>--%>
                <div class="btn btn-primary" data-toggle="modal" data-target="#generate-report" style="cursor:pointer">Generate Report</div>
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

    <!---------------------- TAB :  All ----------------->

    <div class="row row-filter">
        <div class="col-lg-5 col-md-5">
            <div class="input-group mb-5">
                <div class="input-group-prepend">
                    <span class="custom-dropdown adjHeight">
                        <select id="selectSearchType" runat="server" clientidmode="Static">
                            <option value="Username">Username</option>
                            <option value="EmailAdress">Email Address</option>
                            <option value="ROC">ROC</option>
                        </select>
                    </span>
                </div>
                <input type="text" class="form-control adjHeight no-border-right" runat="server" id="txtSearch" />
                <div class="input-group-append">
                    <button class="btn search-icon " type="button" onserverclick="BtnSearch_Click" runat="server"></button>
                </div>
            </div>
            <%--<div class="input-group mb-3">
                <input id="txtSearch" type="text" class="form-control  search-box" placeholder="Search by Name / ID No." runat="server">
                <div class="input-group-append">
                    <button id="btnSearch" class="btn search-icon" type="button" onserverclick="BtnSearch_Click" runat="server"></button>
                </div>
            </div> --%>           
        </div>
        <div class="col-lg-2 col-md-2">
        </div>
        <div class="col-lg-3 col-md-3">
            <div class="input-daterange input-group" id="datepicker">
                <input id="txtStartDate" type="text" class="input-sm form-control" name="start" placeholder="Start Date" runat="server" />
                 <div class="input-group-prepend"><span class="input-group-text">to</span></div>
                <input id="txtEndDate" type="text" class="input-sm form-control" name="end" placeholder="End Date" runat="server" />
            </div>
        </div>
        <%--<div class="col-lg-1 col-md-1">
            <div class="float-right">
                <input id="txtStartDate" type="text" class="form-control" runat="server" />
            </div>
        </div>
        <div class="col-lg-1 col-md-1">
            <div class="float-right">
                <input id="txtEndDate" type="text" class="form-control" runat="server" />
            </div>
        </div>--%>
        <div class=" col-lg-6 col-md-6">
            <%--<div class="dropdown-wrapper float-right  dropdown-product">
                <label>Filter by product</label>

                <div class="dropdown-wrapper float-right dropdown-product">

                    <span class="custom-dropdown">
                        <asp:DropDownList runat="server" ID="ddlFilterType">
                            <asp:ListItem Selected="true" Value="">Claim Type</asp:ListItem>
                        </asp:DropDownList>
                    </span>

                </div>

                <div class="dropdown-wrapper float-right dropdown-product">
                    <asp:HiddenField ClientIDMode="Static" ID="hdnFilterValues" Value="" runat="server" />
                    <span class="custom-dropdown">
                        <asp:DropDownList runat="server" ID="ddlFilterValues" AutoPostBack="True" OnSelectedIndexChanged="DdlFilterValues_OnChange">
                            <asp:ListItem Selected="true" Value="">All</asp:ListItem>
                        </asp:DropDownList>
                    </span>
                </div>


            </div>--%>


        </div>




    </div>

    <div id="claim-all" class="tab" style="max-width: 100%;">

        <div class="clear-float"></div>

        <div class="table-product-wrapper table-responsive">
            <table class="table table-product" style="table-layout: fixed;max-width: 100%">
                <thead>
                    <tr>

                        <th scope="col">Log Date</th>
                        <th scope="col">Username</th>
                        <th scope="col">Name</th>
                        <th scope="col">ROC</th>
                        <th scope="col">Company</th>
                        <th scope="col" style="width: 20%;">Description</th>
                        <th scope="col">Menu</th>
                        <th scope="col">Type</th>

                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater ID="rptAuditLog" runat="server">
                            <ItemTemplate>
                                <tr onmouseover='hideOther(<%# Eval("Id") %>)'>
                                    <td style="word-break:break-all;"><%# !string.IsNullOrEmpty(Eval("Date").ToString()) ? Convert.ToDateTime(Eval("Date")).ToString("dd/MM/yyyy hh:mm tt") : string.Empty %></td>
                                    <td style="word-break:break-all;"><%# Eval("Users") %></td>
                                    <td style="word-break:break-all;"><%# Eval("FullName") %></td>
                                    <td style="word-break:break-all;"><%# Eval("ROC") %></td>
                                    <td style="word-break:break-all;"><%# Eval("ROCName") %></td>
                                    <td style="width: 20%;"><div style="overflow: auto; height: auto; width: 100%;"><%# Eval("Description") %></div></td>
                                    <td><%# Eval("Menu") %></td>                          
                                    <td><%# Eval("Type") %></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                </tbody>
            </table>
        </div>

        <div style="overflow: hidden;">

            <asp:Repeater ID="rptPaging" runat="server" OnItemCommand="rptPaging_ItemCommand">
                <ItemTemplate>
                    <asp:LinkButton ID="btnPage"
                        Style="padding: 8px; margin: 2px; font: 8pt tahoma;"
                        CommandName="Page" CommandArgument="<%# Container.DataItem %>"
                        runat="server" ForeColor="Black" Font-Bold="True">
                            <%# Container.DataItem %>
                    </asp:LinkButton>
                </ItemTemplate>
            </asp:Repeater>

        </div>


    </div>
    
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/jquery-3.5.1.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/equal-height.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/index.js")%>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/popper.min.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/bootstrap-datepicker.min.js") %>"></script>

    <script type="text/javascript">
        $(document).ready(function () {
            $('#<%= txtStartDate.ClientID %>').datepicker({
                format: "dd/MM/yyyy"
            });

            $('#<%= txtEndDate.ClientID %>').datepicker({
                format: "dd/MM/yyyy"
            });

<%--

           // set default dates
            var start = new Date();
            // set end date to max one year period:
            var end = new Date(new Date().setYear(start.getFullYear()+1));

            $('#<%= txtStartDate.ClientID %>').datepicker({
                startDate : start,
                endDate   : end
            // update "toDate" defaults whenever "fromDate" changes
            }).on('changeDate', function(){
                // set the "toDate" start to not be later than "fromDate" ends:
                $('#<%= txtEndDate.ClientID %>').datepicker('setStartDate', new Date($(this).val()));
            }); 

            $('#<%= txtEndDate.ClientID %>').datepicker({
                startDate : start,
                endDate   : end
            // update "fromDate" defaults whenever "toDate" changes
            }).on('changeDate', function(){
                // set the "fromDate" end to not be later than "toDate" starts:
                $('#<%= txtStartDate.ClientID %>').datepicker('setEndDate', new Date($(this).val()));
            });--%>
        });        
    </script>
</asp:Content>
