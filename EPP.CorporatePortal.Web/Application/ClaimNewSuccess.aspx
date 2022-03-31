<%@ Page Language="C#" MasterPageFile="~/ClaimSubmission.Master" AutoEventWireup="true" CodeBehind="ClaimNewSuccess.aspx.cs" Inherits="EPP.CorporatePortal.Application.ClaimNewSuccess" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/fontawesome/css/font-awesome.min.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/dashboard.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/claim.css") %>">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <asp:HiddenField ID="hdnNotifyEmbedded" Value="" runat="server" ClientIDMode="Static" />
    <div class="submitComplete">
        <div class="iconWrap">
            <img src="<%= ResolveUrl("~/Style/assets/icon/icon-success.svg") %>">
        </div>
        <h1 class="page-title bold container center">Claim Submission Complete</h1>
        <p class="sub v2 center">We will keep you updated on the claim status</p>
    </div>

    <hr>

    <div class="container">

        <h1 class="page-title bold container">Summary & Confirm</h1>

        <h3 class="headingGreen">Claims Details</h3>

        <div class="row rowSummary">

            <div class="col-md-4">
                Insured Name / Covered Person
            </div>
            <div class="col-md-5">
                <span id="spanClaimName" runat="server"></span>
            </div>

            <div class="col-md-4">
                ID No.
            </div>
            <div class="col-md-8">
                <span id="spanClaimIDNo" runat="server"></span>
            </div>

            <div class="col-md-4">
                Claim Type
            </div>
            <div class="col-md-8">
                <span id="spanClaimType" runat="server"></span>
            </div>

            <div class="col-md-4">
                Cause of Death/ Diagnosis
            </div>
            <div class="col-md-8">
                <span id="spanClaimCause" runat="server"></span>
            </div>

            <div class="col-md-4">
                Date of Death
            </div>
            <div class="col-md-8">
                <span id="spanClaimDateofEvent" runat="server"></span>
            </div>

        </div>
    </div>

    <hr class="divider">

    <div class="container">
        <h3 class="headingGreen">Policy/Contract Details</h3>

        <asp:Repeater ID="rptPolicyBankList" runat="server">
            <ItemTemplate>
                <div class="row rowSummary">
                    <asp:HiddenField ID="hdnPolicySourceIdCurr" Value='<%# Eval("PolicyID") %>' runat="server" ClientIDMode="Static" />
                    <div class="col-md-4">
                        Policy / Contract Name
                    </div>
                    <div class="col-md-8">
                        <span id="spanPolicyName" runat="server"></span>
                    </div>

                    <div class="col-md-4">
                        Policy / Contract No
                    </div>
                    <div class="col-md-8">
                        <span id="spanPolicyNo" runat="server"></span>
                        <a href='<%= ResolveUrl("~/Application/DownloadFile.ashx") %>?id=<%# Eval("MemberClaimID").ToString() %>&type=<%# EPP.CorporatePortal.Common.Enums.FileDownloadType.ClaimsEForm.ToString() %>&UCorpId=<%# Eval("UCorpId").ToString() %>' target="_blank" class="btn btnDownload">Download the claim form</a>
                        <%--<a href="#">
                                <button class="btn btnDownload">Download the claim form</button></a>--%>
                    </div>
                </div>

                <h4 class="headingBlack">Bank Details</h4>

                <div class="row rowSummary">

                    <div class="col-md-4">
                        Bank Name
                    </div>
                    <div class="col-md-8">
                        <%# Eval("BankName") %>
                    </div>

                    <div class="col-md-4">
                        Account Holder Name
                    </div>
                    <div class="col-md-8">
                        <%# Eval("AccountHolderName") %>
                    </div>

                    <div class="col-md-4">
                        Account No.
                    </div>
                    <div class="col-md-8">
                        <%# Eval("BankAccountNo") %>
                    </div>

                    <div id="divBankROC" class="col-md-4" runat="server">ROC No.</div>
                    <div id="divBankROCData" class="col-md-8" runat="server"><%# Eval("BankROC") %></div>

                    <div id="divIDType" class="col-md-4" runat="server">Nominee ID Type</div>
                    <div id="divIDTypeData" class="col-md-8" runat="server"><%# Eval("IDType") %></div>

                    <div id="divIDNo" class="col-md-4" runat="server">Nominee ID No.</div>
                    <div id="divIDNoData" class="col-md-8" runat="server"><%# Eval("IDNo") %></div>

                    <div id="divContactNo" class="col-md-4" runat="server">Nominee Contact No</div>
                    <div id="divContactNoData" class="col-md-8" runat="server"><%# Eval("ContactNo") %></div>

                    <div id="divEmailAddress" class="col-md-4" runat="server">Nominee Email Address</div>
                    <div id="divEmailAddressData" class="col-md-8" runat="server"><%# Eval("EmailAddress") %></div>
                </div>

                <hr class="divider">
            </ItemTemplate>
        </asp:Repeater>

        <div class="container">
            <h3 class="headingGreen">Submitter Details</h3>

            <div class="row rowSummary">

                <div class="col-md-4">
                    Claimant Name
                </div>
                <div class="col-md-8">
                    <span id="spanSubmitterName" runat="server"></span>
                </div>

                <div class="col-md-4">
                    Contact No
                </div>
                <div class="col-md-8">
                    <span id="spanSubmitterContactNo" runat="server"></span>
                </div>

                <div class="col-md-4">
                    Email Address
                </div>
                <div class="col-md-8">
                    <span id="spanSubmitterEmail" runat="server"></span>
                </div>

                <div class="col-md-4">
                    Company Name
                </div>
                <div class="col-md-8">
                    <span id="spanSubmitterCompany" runat="server"></span>
                </div>

                <div class="col-md-4">
                    Company Registration No
                </div>
                <div class="col-md-8">
                    <span id="spanSubmitterROC" runat="server"></span>
                </div>

            </div>
        </div>

        <hr class="divider">

        <div class="container">
            <h3 class="headingGreen">Uploaded Documents</h3>

            <asp:Repeater ID="rptDocList" runat="server">
                <ItemTemplate>
                    <div class="row rowSummary">
                        <asp:HiddenField ID="hdnDocIdCurr" Value='<%# Eval("DocumentId") %>' runat="server" ClientIDMode="Static" />
                        <div class="col-md-2">
                            <img class="fileThumbnail" src="<%= ResolveUrl("~/Style/assets/img/fileThumbnail.png") %>">
                        </div>
                        <div class="col-md-10">
                            <span id="spanDocumentName" runat="server"></span>
                            <p class="sub"><%# Eval("FileName") %></p>
                        </div>
                    </div>

                    <hr class="divider">
                </ItemTemplate>
            </asp:Repeater>
        </div>




    </div>

    <div style="clear:both; height: 60px;"></div>
    <div class="footerNewClaim">
        <asp:HiddenField ClientIDMode="Static" ID="hdnContinue" Value="0" runat="server" />
        <asp:LinkButton Text="Back to Home" ID="linkContinue" OnClientClick="return CheckDouble();" OnClick="Continue" runat="server" />
    </div>

    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/jquery-3.5.1.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/equal-height.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/claim.js")%>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/popper.min.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.js") %>"></script>

    <script>
        $(document).ready(function () {

            var embeddedCheck = $("#<%= hdnNotifyEmbedded.ClientID %>").val();

            if (embeddedCheck == "True") {
                alert('Death and Funeral Benefits exists. Please make sure to submit for both.');
            }
        });

        function CheckDouble() {
            var submit = parseInt($("#<%= hdnContinue.ClientID %>").val());
            ++submit;
            $("#<%= hdnContinue.ClientID %>").val(submit);

            if (submit > 1) {
                return false;
            }
        }
    </script>
</asp:Content>
