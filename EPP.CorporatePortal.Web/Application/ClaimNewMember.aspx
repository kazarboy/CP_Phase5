<%@ Page Language="C#" MasterPageFile="~/ClaimSubmission.Master" AutoEventWireup="true" CodeBehind="ClaimNewMember.aspx.cs" Inherits="EPP.CorporatePortal.Application.ClaimNewMember" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/fontawesome/css/font-awesome.min.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/dashboard.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/claim.css") %>">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="container">
        <h1 class="page-title bold container">Choose a member</h1>

                <div class="flex row-input  container">

                    <label>Claim for </label>
                    <label class="containerCC">
                        <input type="radio" checked="checked" name="radio">
                        <span class="checkmark"></span>
                        <span>Employee</span>
                    </label>

                    <label class="containerCC ">
                        <div class="disabledInput"></div>
                        <input type="radio" name="radio" disabled>
                        <span class="checkmark"></span>
                        <span>Employee's Dependant</span>
                        <div class="comingSoon">Coming Soon</div>
                    </label>

                </div>

                <div class="flex row-input  container">
                    <label>Search by</label>

                    <div class="input-group mb-3">
                        <div class="input-group-prepend">
                            <span class="custom-dropdown adjHeight">
                                <select id="selectSearchType" runat="server" clientidmode="Static">
                                    <option value="NRIC">NRIC</option>
                                    <option value="OtherIDNo">Other ID No</option>
                                </select>
                            </span>
                        </div>
                        <input type="text" class="form-control adjHeight no-border-right" runat="server" id="txtSearchString">
                        <div class="input-group-append">
                            <button class="btn search-icon " type="button" onserverclick="Search" runat="server"></button>
                        </div>
                    </div>
                </div>

                <p class="foot-note center" onclick="showExtraField()">
                    <a href="#">Or key in ID details</a>
                </p>
    </div>

    <hr>

            <!-------------- Extra input field for dependant? ----------------->
            <div id="extraField" class="container extraField">

                <div class="row rowExtraField">

                    <div class="col-md-12">
                        <h4 class="formTitle">Provide us the Insured/Member details</h4>
                    </div>



                    <div class="col-md-7">
                        <div class="form-group">
                            <label for="txtMemberName">Insured/Member Name</label>
                            <asp:TextBox runat="server" ID="txtMemberName" class="form-control" placeholder="Insured/Member Name"></asp:TextBox>
                        </div>
                    </div>

                    <div class="col-md-7">
                        <label>ID Type</label>
                        <div class="custom-dropdown">
                            <select id="selectKeyInMemberIDType" runat="server" clientidmode="Static">
                                <option value="NRIC">NRIC</option>
                                <option value="OtherIDNo">Other ID No</option>
                            </select>
                        </div>
                    </div>

                    <div class="col-md-7">
                        <div class="form-group">
                            <label for="txtMemberIDNo">ID No</label>
                            <asp:TextBox runat="server" ID="txtMemberIDNo" class="form-control" placeholder="ID No"></asp:TextBox>
                            <%--<asp:CustomValidator runat="server" id="validatorMemberIDNo" controltovalidate="txtMemberIDNo" onservervalidate="validatorMemberIDNo" errormessage="IC No must be 12 digit number without dash" />--%>
                        </div>
                    </div>
                </div>
            </div>

            <div id="resultSection" class="container">

                <br>
                <p>Search result:</p>

                <div id="searchResult" class="table-responsive">
                    <table class="table tableResult">
                        <thead>
                            <tr>
                                <th scope="col"></th>
                                <th scope="col">Name</th>
                                <th scope="col">Identify ID No</th>
                                <th scope="col">Company/ Subsidiary</th>
                                <th scope="col"></th>
                            </tr>
                        </thead>

                        <tbody>
                            <asp:Repeater ID="rptClaimMemberList" runat="server">
                                <ItemTemplate>
                                    <asp:HiddenField ID="hdnMemberTerminationDate" value='<%# Eval("TerminationDate") %>' runat="server"/>
                                    <tr class="groupedRow">
                                        <td colspan="5">
                                            <table class="innerTable" width="100%">
                                                <tr class="trMemberCheckbox" style="cursor: pointer;">                                                    
                                                    <td>
                                                        <div class="nameInitial"><%# Eval("MemberName").ToString()[0] %></div>
                                                    </td>
                                                    <td><%# Eval("MemberName") %><br>
                                                        <span class="userType"><%# Eval("RelationToInsured") %></span></td>
                                                    <td><%# Eval("ICNo") %></td>
                                                    <td id ="tdCorpName" runat="server"><%# Eval("CorpName") %></td>
                                                    <td>
                                                        <label class="containerCC">
                                                            <input type="checkbox" name="member" id="checkmarkCheckbox" value='<%# Eval("SourceId") %>' onchange="chooseMember(this)" runat="server">
                                                            <span class="checkmark"></span>
                                                        </label>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>

            </div>

    <div style="clear:both; height: 60px;"></div>
    <div class="footerNewClaim">
        <asp:HiddenField ClientIDMode="Static" ID="hdnContinue" Value="0" runat="server" />
        <asp:LinkButton Text="Continue" ID="linkContinue" OnClientClick="return CheckDouble();" OnClick="Continue" runat="server" />
        <%--<a id="linkContinue" href="#" runat="server" onserverclick="Continue">Continue</a>--%>
    </div>

    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/jquery-3.5.1.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/equal-height.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/claim.js")%>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/popper.min.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.js") %>"></script>

    <script>
        var isExtraFieldOpen = false;

        $(document).ready(function () {
            $('tr.trMemberCheckbox').click(function (event) {
                if (event.target.type !== 'checkbox') {
                    $(':checkbox', this).trigger('click');
                }
            });

            $('.checkmark').on('click', function (e) {
                e.stopImmediatePropagation()
            });
        });

        function CheckDouble() {
            var submit = parseInt($("#<%= hdnContinue.ClientID %>").val());
            ++submit;
            $("#<%= hdnContinue.ClientID %>").val(submit);

            if (submit > 1) {
                return false;
            }
        }

        function showExtraField() {

            isExtraFieldOpen = !isExtraFieldOpen

            if (isExtraFieldOpen) {
                $("#extraField").show()
            } else {
                $("#extraField").hide()
                $("#txtMemberName").val("")
                $("#txtMemberIDNo").val("")
            }

        }

        function chooseMember(event) {
            if ($("#" + event.id).is(":checked")) {
                $("#" + event.id).parentsUntil("tbody").addClass("activeRow")
                $("td").removeClass("activeRow")
                $("label").removeClass("activeRow")
            } else {
                $("#" + event.id).parentsUntil("tbody").removeClass("activeRow")
            }


        }
    </script>
</asp:Content>
