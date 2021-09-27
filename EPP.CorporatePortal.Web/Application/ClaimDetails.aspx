<%@ Page Title="Claim Details" Language="C#" MasterPageFile="~/CorporatePortalSite.Master" AutoEventWireup="true" CodeBehind="ClaimDetails.aspx.cs" Inherits="EPP.CorporatePortal.Application.ClaimDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/claim.css") %>" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-heading ">
        <div class="row-wrap">
            <h1 class="page-title bold">Claims</h1>
        </div>
    </div>
    <asp:HiddenField ClientIDMode="Static" ID="hdnCorpId" Value="" runat="server" />
    <asp:HiddenField ClientIDMode="Static" ID="hdnMemberClaimsId" Value="" runat="server" />
    <div class="page-content">
        <div class="tab">

            <br>

            <asp:Button Text="< Back" ID="btnBack" OnClientClick="this.disabled = true;" OnClick="Back" UseSubmitBehavior="false" runat="server" CssClass="btnBack" />

            <div class="row rowDetails">

                <div class="col-md-12">
                    <h1 class="page-title bold">Claims Details</h1>
                </div>
                <div class="col-md-6">
                    <div class="row">

                        <div class="col-md-6 p-gr">
                            Insured Name / Member Person
                        </div>
                        <div class="col-md-6 p-dr">
                            <span id="spanInsuredName" runat= "server"></span>
                        </div>

                        <div class="col-md-6 p-gr">
                            ID No
                        </div>
                        <div class="col-md-6 p-dr">
                            <span id="spanInsuredIDNo" runat= "server"></span>
                        </div>

                        <div class="col-md-6 p-gr">
                            Claim Type
                        </div>
                        <div class="col-md-6 p-dr">
                            <span id="spanInsuredClaimType" runat= "server"></span>
                        </div>

                        <div id="divCauseOfEventLabel" class="col-md-6 p-gr" runat= "server">
                            Cause of Event
                        </div>
                        <div class="col-md-6 p-dr">
                            <span id="spanInsuredCauseOfEvent" runat= "server"></span>
                        </div>

                        <div id="divDateOfEventLabel" class="col-md-6 p-gr" runat= "server">
                            Date of Event
                        </div>
                        <div class="col-md-6 p-dr">
                            <span id="spanInsuredDateOfEvent" runat= "server"></span>
                        </div>

                    </div>
                </div>

                <div class="col-md-6 gr">

                    <div class="row ">
                        <div class="col-md-6 p-gr">
                            Submission Date
                        </div>
                        <div class="col-md-6 p-dr">
                            <span id="spanClaimSubmissionDate" runat= "server"></span>
                        </div>

                        <div class="col-md-6 p-gr">
                            Portal Claim No
                        </div>
                        <div class="col-md-6 p-dr">
                            <span id="spanClaimPortalClaimNo" runat= "server"></span>
                        </div>

                        <div class="col-md-6 p-gr">
                            CGLS Claim No
                        </div>
                        <div class="col-md-6 p-dr">
                            <span id="spanClaimCGLSClaimNo" runat= "server"></span>
                        </div>

                        <div class="col-md-6 p-gr">
                            Claim Status
                        </div>
                        <div class="col-md-6 p-dr">
                            <span id="spanClaimStatus" runat= "server"></span>
                        </div>

                        <div id="divClaimPaidDate" runat= "server" class="col-md-6 p-gr">Claim Paid Date</div>
                        <div class="col-md-6 p-dr">
                            <span id="spanClaimPaidDate" runat= "server"></span>
                        </div>

                        <div id="divClaimPaidAmount" runat= "server" class="col-md-6 p-gr">Paid Amount</div>
                        <div id="divClaimPaidAmount2" runat= "server" class="col-md-6 p-dr">
                            <span id="spanClaimPaidAmount" runat= "server"></span>
                        </div>

                        <div id="divClaimRejectReason" runat= "server" class="col-md-6 p-gr">Rejected Reasons</div>
                        <div id="divClaimRejectReason2" runat= "server" class="col-md-6 p-dr">
                            <span id="spanClaimRejectReason" runat= "server"></span>
                        </div>

                        <div class="col-md-6 p-gr">
                            Claim Letter
                        </div>
                        <div class="col-md-6 p-dr">
                            <span id="spanClaimLetter" runat= "server"></span>
                        </div>

                    </div>

                </div>

            </div>

            <div class="row">

                <div class="col-md-6 ">

                    <div class="row">

                        <div class="col-md-12">
                            <h1 class="page-title bold">Policy / Contract Details</h1>
                        </div>

                        <div class="col-md-6 p-gr">
                            Policy/ Contract Name
                        </div>
                        <div class="col-md-6 p-dr">
                            <span id="spanPolicyName" runat= "server"></span>
                        </div>

                        <div class="col-md-6 p-gr">
                            Policy/ Contract No
                        </div>
                        <div class="col-md-6 p-dr">
                            <span id="spanPolicyNo" runat= "server"></span>
                        </div>

                    </div>

                </div>

                <div class="col-md-6 ">

                    <div class="row">

                        <div class="col-md-12">
                            <h1 class="page-title bold">Submitter Details</h1>
                        </div>

                        <div class="col-md-6 p-gr">
                            Claimant Name
                        </div>
                        <div class="col-md-6 p-dr">
                            <span id="spanSubmitterName" runat= "server"></span>
                        </div>
                        <div class="col-md-6 p-gr">
                            Contact No
                        </div>
                        <div class="col-md-6 p-dr">
                            <span id="spanSubmitterContactNo" runat= "server"></span>
                        </div>

                        <div class="col-md-6 p-gr">
                            Email Address
                        </div>
                        <div class="col-md-6 p-dr">
                            <span id="spanSubmitterEmailAddr" runat= "server"></span>
                        </div>

                        <div class="col-md-6 p-gr">
                            Company Name
                        </div>
                        <div class="col-md-6 p-dr">
                            <span id="spanSubmitterCompanyName" runat= "server"></span>
                        </div>

                        <div class="col-md-6 p-gr">
                            Company Registration No
                        </div>
                        <div class="col-md-6 p-dr">
                            <span id="spanSubmitterBizRegNo" runat= "server"></span>
                        </div>

                    </div>

                </div>

            </div>

            <div class="sectionUpload">

                <h1 class="page-title bold">Uploaded Documents</h1>

                <asp:Repeater ID="rptDocList" runat="server">
                    <ItemTemplate>
                        <div class="row rowUploaded">
                            <asp:HiddenField ID="hdnDocIDCurr" Value='<%# Eval("DocumentId") %>' runat="server" ClientIDMode="Static" />
                            <div class="col-md-2">
                                <img class="fileThumbnail" src="<%= ResolveUrl("~/Style/assets/img/fileThumbnail.png") %>">
                            </div>
                            <div class="col-md-8">
                                <span id="spanDocumentName" runat="server"></span>
                                <p class="sub"><%# Eval("UploadedDocumentName") %></p>
                            </div>

                            <div class="col-md-2">
                                <a href='<%= ResolveUrl("~/Application/DownloadFile.ashx") %>?id=<%# Eval("Id").ToString() %>&type=<%# EPP.CorporatePortal.Common.Enums.FileDownloadType.ClaimsDocuments.ToString() %>' target="_blank" class="view">Download</a>
                            </div>

                        </div>

                        <hr class="divider">
                    </ItemTemplate>
                </asp:Repeater>

            </div>
        </div>
    </div>

    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/jquery-3.5.1.js") %>"></script>
    <script type="text/javascript"  src="<%= ResolveUrl("~/Style/assets/vendor/equal-height.js") %>"></script>
    <script type="text/javascript"  src="<%= ResolveUrl("~/Style/index.js")%>"></script>
    <script type="text/javascript"  src="<%= ResolveUrl("~/Style/assets/vendor/popper.min.js") %>"></script>
    <script type="text/javascript"  src="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.js") %>"></script>
</asp:Content>
