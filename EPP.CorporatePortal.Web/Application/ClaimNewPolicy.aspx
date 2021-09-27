<%@ Page Language="C#" MasterPageFile="~/ClaimSubmission.Master" AutoEventWireup="true" CodeBehind="ClaimNewPolicy.aspx.cs" Inherits="EPP.CorporatePortal.Application.ClaimNewPolicy" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/fontawesome/css/font-awesome.min.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/dashboard.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/claim.css") %>">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="container">
        <h1 class="page-title bold container">Choose policy/certificate(s)</h1>


        <div class="container">

            <div class="table-responsive">

                <table class="table tableResult">
                    <thead>
                        <tr>
                            <th scope="col"></th>
                            <th scope="col">Product Name</th>
                            <th scope="col">Policy/Certificate No</th>
                            <th scope="col"></th>
                        </tr>
                    </thead>

                    <tbody>
                        <asp:Repeater ID="rptClaimPolicyList" runat="server">
                            <ItemTemplate>
                                <tr class="groupedRow">
                                    <td colspan="4">
                                        <table class="innerTable" width="100%">
                                            <tr class="trPolicyCheckbox" style="cursor: pointer;">
                                                <td>
                                                    <div class="productInitial takaful"><%# Eval("Entity") %></div>
                                                </td>
                                                <td><%# Eval("ProductName") %></td>
                                                <td><%# Eval("ContractNo") %></td>
                                                <td>
                                                    <label class="containerCC">
                                                        <input type="checkbox" name="policy" id="checboxPolicy" value='<%# Eval("PolicyId") %>' onchange="product(this)" runat="server">
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
        $(document).ready(function () {
            $('tr.trPolicyCheckbox').click(function (event) {
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

        function product(event) {

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
