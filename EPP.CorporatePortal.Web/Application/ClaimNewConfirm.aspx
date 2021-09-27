<%@ Page Language="C#" MasterPageFile="~/ClaimSubmission.Master" AutoEventWireup="true" CodeBehind="ClaimNewConfirm.aspx.cs" Inherits="EPP.CorporatePortal.Application.ClaimNewConfirm" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/fontawesome/css/font-awesome.min.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/dashboard.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/claim.css") %>">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <h1 class="page-title bold container">Claim Summary Confirmation</h1>

    <div class="container">

        <h3 class="headingGreen">Claims Details</h3>

        <div class="row rowSummary">

            <div class="col-md-4">
                Insured Name / Covered Person
            </div>
            <div class="col-md-8">
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
                Cause of Death / Diagnosis
            </div>
            <div class="col-md-8">
                <span id="spanClaimCause" runat="server"></span>
            </div>

            <div class="col-md-4">
                Date of Death / Event
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

    </div>

    <hr class="divider">

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

        <div class="row">
            <div class="col-md-12 ">
                <div class="checkedRow ">
                    <label class="containerCC v2 " for="tncChecbox">
                        <input type="checkbox" name="radio" id="tncChecbox" ClientIDMode="Static" runat="server">
                        <span class="checkmark checkmarkRect"></span>
                        <span class="tnc">I agree with the <a href="#" data-toggle="modal" data-target="#viewTnC">Consent & Terms & Conditions</a></span>
                    </label>
                </div>
            </div>

        </div>

    </div>

    <div class="modal fade" id="viewTnC">
        <div class="modal-dialog modal-lg" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="exampleModalLabel">Terms & Conditions</h5>

                </div>
                <div class="modal-body">
                    <br>
                    <div>
                        <table>               
                                <tr id="trTncELIB" style="display:none" runat="server">
                                    <td colspan="2">
                                        <p>
                                            <span>I do solemnly and sincerely declare that I am the nominee/administrator/beneficiary for the Life Insurance benefit of the deceased and further declare as follows:-</span>
                                        </p>
                                        <p>
                                            <span>  <b>1.</b> That the foregoing answers and statements on the Deceased are complete and true to the best of my knowledge and belief, and that I have withheld no material facts from the Company.</span>
                                        </p>
                                        <p>
                                            <span>  <b>2.</b> That any difference, if any, in respect of the details contained in the enclosed supporting document and the information presented to Etiqa Life Insurance Berhad(Etiqa) in this form refers to the same person. I understand and agree that Etiqa has the sole discretion to reject this application if the information given is false or insufficient. </span>
                                        </p>
                                        <p>
                                            <span>  <b>3.</b> That the original certificate whether or not enclosed therein (if any), due to loss or mutilated, belongs to the deceased.</span>
                                        </p>
                                        <p>
                                            <span>  <b>4.</b> And I hereby authorize any medical practitioner, surgeon person, hospital, clinic and any other institution or organization to furnish Etiqa Life Insurance Berhad or its representative any information that may be required concerning my health conditions, for settlement of this claim. I agree that Etiqa Takaful Berhad or its representative may use or disclose any of the information collected or held to third parties such as reinsurers, medical examiner or medical consultant, claims investigator and etc. within or outside Malaysia for the purpose of processing the claim. I agree that a photocopy of this authorization shall be considered as effective and valid as original.</span>
                                        </p>
                                        <p>
                                            <span>  <b>5.</b> I, agree, consent and allow Etiqa Life Insurance Berhad (hereinafter called “Etiqa Insurancel”) to process my personal data (including sensitive personal data) (‘Personal Data’) with the intention of processing this Claim Form, in compliance with the provisions of the Personal Data Protection Act 2010.</span>
                                        </p>
                                        <p>
                                            <span>  <b>6.</b> I, understand and agree that any Personal Data collected or held by Etiqa Insurnace contained in this Claim Form may be held, used, processed and disclosed by Etiqa Insurance to individuals and/or organizations related to and associated with Etiqa Insurance or any selected third party (within or outside Malaysia, including medical institutions, solicitors, industry associations, regulators, statutory bodies and government authorities) for the purpose of processing this Claim Form and providing subsequent service related to it and to communicate with me for such purposes.</span>
                                        </p>
                                        <p>
                                            <span>  <b>7.</b> I agree that a copy of documents submitted shall be as valid as the original. I confirm that the information given on this online submission form is to the best of my knowledge and belief, true in every aspect. I understand that the making of a fraudulent claim by providing untrue information is a criminal offence likely to lead to prosecution.</span>
                                        </p>
                                    </td>
                                </tr>
                                <tr id="trTncEFTB" style="display:none" runat="server">
                                    <td colspan="2">
                                        <p>
                                            <span>I do solemnly and sincerely declare that I am the nominee/administrator/beneficiary for the Takaful benefit of the deceased and further declare as follows:-</span>
                                        </p>
                                        <p>
                                            <span>  <b>1.</b> That the foregoing answers and statements on the Deceased are complete and true to the best of my knowledge and belief, and that I have withheld no material facts from the Company.</span>
                                        </p>
                                        <p>
                                            <span>  <b>2.</b> That any difference, if any, in respect of the details contained in the enclosed supporting document and the information presented to Etiqa Family Takaful Berhad (Etiqa Takaful) in this form refers to the same person. I understand and agree that Etiqa Takaful has the sole discretion to reject this application if the information given is false or insufficient. </span>
                                        </p>
                                        <p>
                                            <span>  <b>3.</b> That the original certificate whether or not enclosed therein (if any), due to loss or mutilated, belongs to the deceased.</span>
                                        </p>
                                        <p>
                                            <span>  <b>4.</b> And I hereby authorize any medical practitioner, surgeon person, hospital, clinic and any other institution or organization to furnish Etiqa Famiy Takaful Berhad or its representative any information that may be required concerning my health conditions, for settlement of this claim. I agree that Etiqa Family Takaful Berhad or its representative may use or disclose any of the information collected or held to third parties such as reinsurers, medical examiner or medical consultant, claims investigator and etc. within or outside Malaysia for the purpose of processing the claim. I agree that a photocopy of this authorization shall be considered as effective and valid as original.</span>
                                        </p>
                                        <p>
                                            <span>  <b>5.</b> I, agree, consent and allow Etiqa Family Takaful Berhad (hereinafter called “Etiqa Takaful”) to process my personal data (including sensitive personal data) (‘Personal Data’) with the intention of processing this Claim Form, in compliance with the provisions of the Personal Data Protection Act 2010.</span>
                                        </p>
                                        <p>
                                            <span>  <b>6.</b> I, understand and agree that any Personal Data collected or held by Etiqa Takaful contained in this Claim Form may be held, used, processed and disclosed by Etiqa Takaful to individuals and/or organizations related to and associated with Etiqa Takaful or any selected third party (within or outside Malaysia, including medical institutions, solicitors, industry associations, regulators, statutory bodies and government authorities) for the purpose of processing this Claim Form and providing subsequent service related to it and to communicate with me for such purposes.</span>
                                        </p>
                                        <p>
                                            <span>  <b>7.</b> I agree that a copy of documents submitted shall be as valid as the original. I confirm that the information given on this online submission form is to the best of my knowledge and belief, true in every aspect. I understand that the making of a fraudulent claim by providing untrue information is a criminal offence likely to lead to prosecution.</span>
                                        </p>
                                    </td>
                                </tr>
                        </table>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary v2" data-dismiss="modal">Close</button>
                </div>
            </div>
        </div>
    </div>

    <div style="clear:both; height: 60px;"></div>
    <div class="footerNewClaim">
        <asp:HiddenField ClientIDMode="Static" ID="hdnContinue" Value="0" runat="server" />
        <asp:LinkButton Text="Continue" ID="linkContinue" OnClientClick="return CheckDouble();" OnClick="Continue" runat="server" />
    </div>

    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/jquery-3.5.1.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/equal-height.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/claim.js")%>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/popper.min.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.js") %>"></script>

    <script>
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
