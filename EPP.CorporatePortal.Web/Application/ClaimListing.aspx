<%@ Page Title="Claim Listing" Language="C#" MasterPageFile="~/CorporatePortalSite.Master" AutoEventWireup="true" CodeBehind="ClaimListing.aspx.cs" Inherits="EPP.CorporatePortal.Application.ClaimListing" %>
<%@ Import namespace="EPP.CorporatePortal.Models" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/claim.css") %>" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:HiddenField   ClientIDMode="Static" ID="hdnCorpId" value="" runat="server"/>
    <div class="page-heading ">

        <div class="row-wrap">

            <h1 class="page-title bold">Claims</h1>

            <div class="btn-wrapper">

                <a href="<%# ResolveUrl("~/Application/ClaimNewMember.aspx?&CorpId=" + Utility.Encrypt(hdnCorpId.Value)) %>" class="btn btn-primary">+ Submit New Claims
                </a>

                <a href="#" class="btn btn-noborder" data-toggle="modal" data-target="#downloadReport">Download
                            Report</a>

                <a href="#" class="btn btn-noborder">Document Checklist
                </a>
            </div>


        </div>


    </div>

    <div class="tab-menu-wrapper">
        <asp:Button Text="All" ID="btnAllStatus" OnClick="BtnAllStatus_Click" UseSubmitBehavior="false" runat="server" CssClass="btn-tab tablink btn active" />
        <asp:Button Text="In Progress ()" ID="btnInProgressStatus" OnClick="BtnInProgressStatus_Click" UseSubmitBehavior="false" runat="server" CssClass="btn-tab tablink btn" />
        <asp:Button Text="Pending ()" ID="btnPendingStatus" OnClick="BtnPendingStatus_Click" UseSubmitBehavior="false" runat="server" CssClass="btn-tab tablink btn" />
        <asp:Button Text="Approved ()" ID="btnApprovedStatus" OnClick="BtnApprovedStatus_Click" UseSubmitBehavior="false" runat="server" CssClass="btn-tab tablink btn" />
        <asp:Button Text="Fully Paid ()" ID="btnFullyPaidStatus" OnClick="BtnFullyPaidStatus_Click" UseSubmitBehavior="false" runat="server" CssClass="btn-tab tablink btn" />
        <asp:Button Text="Rejected ()" ID="btnRejectedStatus" OnClick="BtnRejectedStatus_Click" UseSubmitBehavior="false" runat="server" CssClass="btn-tab tablink btn" />
        <%--<button class="btn-tab tablink btn active" onclick="openTab(event,'claim-all')">All</button>
        <button class="btn-tab tablink btn" onclick="openTab(event,'claim-in-progress')">
            In Progress
                    (4)</button>
        <button class="btn-tab tablink btn" onclick="openTab(event,'claim-pending')">Pending (1)</button>
        <button class="btn-tab tablink btn" onclick="openTab(event,'claim-approved')">Approved (12)</button>
        <button class="btn-tab tablink btn" onclick="openTab(event,'claim-fully-paid')">Fully Paid(12)</button>
        <button class="btn-tab tablink btn" onclick="openTab(event,'claim-rejected')">Rejected(1)</button>--%>
    </div>

    <!---------------------- TAB :  All ----------------->

    <div class="row row-filter">
        <div class="col-lg-3 col-md-12">
            <div class="input-group mb-3">
                <input id="txtSearch" type="text" class="form-control  search-box" placeholder="Search by Name / ID No." runat="server">
                <div class="input-group-append">
                    <button id="btnSearch" class="btn search-icon" type="button" onserverclick="BtnSearch_Click" runat="server"></button>
                </div>
            </div>
        </div>

        <div class=" col-lg-9 col-md-12">
            <div class="dropdown-wrapper float-right  dropdown-product">
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


            </div>


        </div>




    </div>

    <div id="claim-all" class="tab" style="max-width: 100%;">

        <div class="clear-float"></div>

        <div class="table-product-wrapper table-responsive">
            <table class="table table-product" style="max-width: 100%">
                <thead>
                    <tr>

                        <th scope="col">Submission Date</th>
                        <th scope="col">Insured/Member Name</th>
                        <th scope="col">ID No.</th>
                        <th scope="col" style="width: 7%">Claim Type</th>
                        <th scope="col">Policy/Contract No.</th>
                        <th scope="col" style="width: 7%">Portal Claim No.</th>
                        <th scope="col" style="width: 7%">CGLS Claim No</th>
                        <th scope="col">Claim Status</th>
                        <th scope="col">Claim Status Date</th>
                        <th scope="col">Appeal</th>
                        <th scope="col"></th>

                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater ID="rptClaimsList" runat="server">
                            <ItemTemplate>
                                <tr onmouseover='hideOther(<%# Eval("Id") %>)'>
                                    <td><%# !string.IsNullOrEmpty(Eval("SubmissionDate").ToString()) ? Convert.ToDateTime(Eval("SubmissionDate")).ToString("dd/MM/yyyy hh:mm tt") : string.Empty %></td>
                                    <td><%# Eval("MemberName") %></td>
                                    <td><%# Eval("DisplayIDNo") %></td>
                                    <td><%# Eval("BenefitDescription") %></td>
                                    <td><%# Eval("ContractNo") %></td>
                                    <td style="word-break: break-all;"><%# Eval("PortalClaimNo") %></td>
                                    <td><%# Eval("CGLSClaimNo") %></td>
                                    <td><%# Eval("ClaimStatus") %></td>
                                    <td><%# !string.IsNullOrEmpty(Eval("ClaimStatusDate").ToString()) ? Convert.ToDateTime(Eval("ClaimStatusDate")).ToString("dd/MM/yyyy hh:mm tt") : "-" %></td>
                                    <td><%# Convert.ToBoolean(Eval("IsAppeal").ToString()) ? "Yes" : "No" %></td>
                                    <td class="actionList" id="colAction<%# Eval("Id") %>">
                                        <div class="threeDots" onmouseover="view(<%# Eval("Id") %>)"></div>
                                        <ul id="action<%# Eval("Id") %>" style="display: none;">
                                            <li><a href='<%= ResolveUrl("~/Application/DownloadFile.ashx") %>?id=<%# Eval("Id").ToString() %>&type=<%# EPP.CorporatePortal.Common.Enums.FileDownloadType.ClaimsEForm.ToString() %>' target="_blank">Download Claim Form</a></li>
                                            <li><a href='<%= ResolveUrl("~/Application/ClaimDetails.aspx?&CorpId=" + Utility.Encrypt(hdnCorpId.Value)) %>&MemberClaimsId=<%# Utility.Encrypt(Eval("Id").ToString()) %>'>View Claim Details</a></li>
                                            <%--<li>Remove This Claim</li>--%>
                                        </ul>
                                    </td>
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

    <%--<!------- ------ TAB : In Progress ------------->

    <div id="claim-in-progress" class="tab" style="display: none">
        Load data for claim in progress here
    </div>

    <!-------------- TAB : Pending ----------------->

    <div id="claim-pending" class="tab" style="display: none">
        Load data for claim pending here
    </div>

    <!-------------- TAB : Approved ----------------->

    <div id="claim-approved" class="tab" style="display: none">
        Load data for approved claim here
    </div>

    <!-------------- TAB : Fully Paid ----------------->

    <div id="claim-fully-paid" class="tab" style="display: none">
        Load data for fully paid claim here
    </div>

    <!-------------- TAB : Rejected ----------------->

    <div id="claim-rejected" class="tab" style="display: none">
        Load data for rejected claim here
    </div>--%>

    <div class="modal fade" id="downloadReport">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="exampleModalLabel">Download Claim Report</h5>

                </div>
                <div class="modal-body">

                    <br>
                    <div class="row  row-input">

                        <div class="col-md-12">
                            <label>Claim Type</label>
                            <div class="custom-dropdown">
                                <select>
                                    <option value="">All</option>
                                </select>
                            </div>
                        </div>

                        <div class="col-md-12">
                            <div class="row">
                                <div class="col-md-6">
                                    <label>From</label>
                                    <div class="custom-dropdown">
                                        <select>
                                            <option value="">2019</option>
                                        </select>
                                    </div>
                                </div>

                                <div class="col-md-6">
                                    <label>To</label>
                                    <div class="custom-dropdown">
                                        <select>
                                            <option value="">2020</option>
                                        </select>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="col-md-12">
                            <label>Filter Report By</label>
                            <div class="switch-field v2">
                                <input type="radio" id="radio-one" name="switch-one" value="yes" checked="">
                                <label for="radio-one">Year</label>
                                <input type="radio" id="radio-two" name="switch-one" value="no">
                                <label for="radio-two">Months</label>
                                <input type="radio" id="radio-two" name="switch-one" value="no">
                                <label for="radio-two">Policy No</label>
                            </div>
                        </div>

                        <div class="col-md-12">
                            <label>Claim Status</label>
                            <div class="custom-dropdown">
                                <select>
                                    <option value="">All</option>
                                </select>
                            </div>
                        </div>

                    </div>


                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary v2" data-dismiss="modal">Cancel</button>
                    <button type="button" class="btn btn-primary v2">Add</button>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
    function openTab(evt, tab) {

        var i, tablinks;
        //var x = document.getElementsByClassName("tab");
        //for (i = 0; i < x.length; i++) {
        //    x[i].style.display = "none";
        //}

        tablinks = document.getElementsByClassName("tablink");
        for (i = 0; i < x.length; i++) {
            tablinks[i].className = tablinks[i].className.replace(" active", "");
        }

        //$("#" + tab).show();
        evt.currentTarget.className += " active";

    }

    function view(id) {
        $("td.actionList > ul").hide()
        $("#action" + id).show()
    }

    function hideOther(id) {
        $("td.actionList > ul:not(#action" + id + ")").hide()
    }

    //$(document).ready(function () {

    //    var data = [
    //        {
    //            "date": "29 April 2019",
    //            "insuredName": "Fatin Nabila binti Azahari",
    //            "idNo": "940730097654",
    //            "claimType": "Death and Funeral Expenses",
    //            "policyNo": "EFT319371084",
    //            "portalClaimNo" : "EBP_200TGT",
    //            "cglsClaimNo" : "-",
    //            "claimStatus" : "New Submission",
    //            "claimStatusDate": "30 July 2010",
    //            "appeal": true,     
    //        },{
    //            "date": "29 April 2019",
    //            "insuredName": "Fatin Nabila binti Azahari",
    //            "idNo": "940730097654",
    //            "claimType": "Death and Funeral Expenses",
    //            "policyNo": "EFT319371084",
    //            "portalClaimNo" : "EBP_200TGT",
    //            "cglsClaimNo" : "-",
    //            "claimStatus" : "Rejected",
    //            "claimStatusDate": "30 July 2010",
    //            "appeal": true,     
    //        },{
    //            "date": "29 April 2019",
    //            "insuredName": "Fatin Nabila binti Azahari",
    //            "idNo": "940730097654",
    //            "claimType": "Death and Funeral Expenses",
    //            "policyNo": "EFT319371084",
    //            "portalClaimNo" : "EBP_200TGT",
    //            "cglsClaimNo" : "-",
    //            "claimStatus" : "Pending",
    //            "claimStatusDate": "30 July 2010",
    //            "appeal": true,     
    //        },

    //        {
    //            "date": "29 April 2019",
    //            "insuredName": "Fatin Nabila binti Azahari",
    //            "idNo": "940730097654",
    //            "claimType": "Death and Funeral Expenses",
    //            "policyNo": "EFT319371084",
    //            "portalClaimNo" : "EBP_200TGT",
    //            "cglsClaimNo" : "-",
    //            "claimStatus" : "Approved",
    //            "claimStatusDate": "30 July 2010",
    //            "appeal": true,     
    //        },

    //        {
    //            "date": "29 April 2019",
    //            "insuredName": "Fatin Nabila binti Azahari",
    //            "idNo": "940730097654",
    //            "claimType": "Death and Funeral Expenses",
    //            "policyNo": "EFT319371084",
    //            "portalClaimNo" : "EBP_200TGT",
    //            "cglsClaimNo" : "-",
    //            "claimStatus" : "In Progress",
    //            "claimStatusDate": "30 July 2010",
    //            "appeal": true,     
    //        },
           
    //    ];



    //    for (var i = 0; i < data.length; i++) {

    //        $("table.table-product > tbody").append("" +
    //            "<tr  onmouseover='hideOther(" + i + ")'>" +
    //            "<td>" + data[i].date + "</td>" +
    //            "<td>" + data[i].insuredName + "</td>" +
    //            "<td>" + data[i].idNo + "</td>" +
    //            "<td>" + data[i].claimType + "</td>" +
    //            "<td>" + data[i].policyNo + "</td>" +
    //            "<td>" + data[i].portalClaimNo + "</td>" +
    //            "<td>" + data[i].cglsClaimNo + "</td>" +
    //            "<td>" + data[i].claimStatus + "</td>" +
    //            "<td>" + data[i].claimStatusDate + "</td>" +
    //            "<td>" + isAppeal(data[i].appeal) + "</td>" +
    //            "<td class='actionList' id='colAction" + i + "'><div class='threeDots' onmouseover='view(" +
    //            i + ")'></div></td>" +
    //            "</tr>");

    //        //Check with BA what would be the action
    //        if (data[i].claimStatus.toLowerCase() == "new submission") {
    //            $("#colAction" + i).append("" +
    //                "<ul id='action" + i + "'>" +
    //                "<li><a href='#'>View Claim Form</a></li>" + // Link to PDF?
    //                "<li><a href='claim-details.html'>View Claim Details</a></li>" +
    //                "<li>Remove This Claim</li>" +
    //                "</ul>")
    //        }


    //        if (data[i].claimStatus.toLowerCase() == "in progress") {
    //            $("#colAction" + i).append("" +
    //                "<ul id='action" + i + "'>" +
    //                "<li><a href='#'>View Claim Form</a></li>" + // Link to PDF?
    //                "<li><a href='claim-details.html'>View Claim Details</a></li>" +
    //                "<li>Remove This Claim</li>" +
    //                "</ul>")
    //        }

    //        if (data[i].claimStatus.toLowerCase() == "pending") {

    //            $("#colAction" + i).append("" +
    //                "<ul id='action" + i + "'>" +
    //                "<li><a href='#'>View Claim Form</a></li>" + // Link to PDF?
    //                "<li><a href='claim-details.html'>View Claim Details</a></li>" +
    //                "</ul>")

    //            $("table.table-product > tbody").append("" +
    //                "<tr class='additionalRow'>" +
    //                "<td colspan='11'>" +
    //                "<div class='pending'>" +
    //                "<span class='float-left'>There are few more document required</span>" +
    //                "<span class='float-right'><a href='#'>Upload Now ></a></span>" +
    //                "</div>" +
    //                "</td>" +
    //                "</tr>")
    //        }

    //        if (data[i].claimStatus.toLowerCase() == "approved") {
    //            $("#colAction" + i).append("" +
    //                "<ul id='action" + i + "'>" +
    //                "<li><a href='#'>View Claim Form</a></li>" + // Link to PDF?
    //                "<li><a href='claim-details.html'>View Claim Details</a></li>" +
    //                "<li>View Claim Letter</li>" +
    //                "<li>View Settlement Letter</li>" +
    //                "</ul>")
    //        }

    //        if (data[i].claimStatus.toLowerCase() == "fully paid") {
    //            $("#colAction" + i).append("" +
    //                "<ul id='action" + i + "'>" +
    //                "<li><a href='#'>View Claim Form</a></li>" + // Link to PDF?
    //                "<li><a href='claim-details.html'>View Claim Details</a></li>" +
    //                "<li>View Claim Letter</li>" +
    //                "<li>View Settlement Letter</li>" +
    //                "</ul>")
    //        }

    //        if (data[i].claimStatus.toLowerCase() == "rejected") {
    //            $("#colAction" + i).append("" +
    //                "<ul id='action" + i + "'>" +
    //                "<li><a href='appeal-document.html'>Appeal This Claim</a></li>" +
    //                "<li><a href='claim-details.html'>View Claim Details</a></li>" +
    //                "<li>View Declined Letter</li>" +
    //                "</ul>")

    //            $("table.table-product > tbody").append("" +
    //                "<tr class='additionalRow'>" +
    //                "<td colspan='11'>" +
    //                "<div class='rejected'>" +
    //                "<span class='float-left'>Due to reason of pre-existing condition, this claim is rejected </span>" +
    //                "<span class='float-right'><a href='#'>View Declined Letter ></a></span>" +
    //                "</div>" +
    //                "</td>" +
    //                "</tr>")
    //        }

    //    }


    //});

    function isAppeal(status){

        let statusString ;

        !status ? statusString = "-" : statusString = "Yes";

        return statusString

    }
</script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/jquery-3.5.1.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/equal-height.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/index.js")%>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/popper.min.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.js") %>"></script>
</asp:Content>
