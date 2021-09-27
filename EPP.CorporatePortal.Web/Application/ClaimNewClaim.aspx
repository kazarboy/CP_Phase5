<%@ Page Language="C#" MasterPageFile="~/ClaimSubmission.Master" AutoEventWireup="true" CodeBehind="ClaimNewClaim.aspx.cs" Inherits="EPP.CorporatePortal.Application.ClaimNewClaim" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/fontawesome/css/font-awesome.min.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/dashboard.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/claim.css") %>">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <h1 class="page-title bold container">Fill up claim details</h1>

    <div class="container">

        <div class="btn-group v2">
            <asp:HiddenField ClientIDMode="Static" ID="hdnSelectedType" Value="" runat="server" />
            <asp:HiddenField ClientIDMode="Static" ID="hdnSelectedOriType" Value="" runat="server" />
            <asp:HiddenField ClientIDMode="Static" ID="hdnMemberTerminationDate" Value="" runat="server" />
            <asp:HiddenField ClientIDMode="Static" ID="hdnContainsDTH" Value="" runat="server" />
            <asp:Button ID="btnChangeSelection" OnClick="ClaimListChangeLogic" Style="display: none" runat="server"></asp:Button>
            <button type="button" class="btn btn-dropdrown dropdown-toggle" data-toggle="dropdown"
                aria-haspopup="true" aria-expanded="false">
                <span id="spanSelectedType" runat="server">Death and Funeral Expenses</span>
                <span id="spanSelectedTooltip" runat="server" data-toggle="tooltip" data-placement="top"
                    title="Death and Funeral Expenses" class="customTooltip">i</span>
            </button>

            <ul class="dropdown-menu">
                <asp:Repeater ID="rptClaimTypeList" runat="server">
                    <ItemTemplate>
                        <li class="dropdown-item" onclick="changeSelection('<%# Eval("DisplayName") %>', '<%# Eval("DisplayCode") %>', '<%# Eval("DisplayCode") %>')">
                            <span><%# Eval("DisplayName") %></span>
                            <span data-toggle="tooltip" data-placement="top" title="<%# Eval("DisplayName") %>"
                                class="customTooltip">i</span>
                        </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </div>

        <div id="divCICategory" class="btn-group v2" style="display: none" runat="server">
            <asp:HiddenField ClientIDMode="Static" ID="hdnClaimTypeOptdiv3" Value="" runat="server" />
            <asp:HiddenField ClientIDMode="Static" ID="hdnSelectedCICategory" Value="" runat="server" />
            <p id="claimTypeOpt2LabelCI" runat="server" class="claimTypeOptLabel">Category</p>
            <button type="button" class="btn btn-dropdrown dropdown-toggle" data-toggle="dropdown"
                aria-haspopup="true" aria-expanded="false">
                <span id="spanSelectedTypeCI" runat="server">Select CI Category</span>
                <span id="spanSelectedTooltipCI" runat="server" data-toggle="tooltip" data-placement="top"
                    title="Death and Funeral Expenses" class="customTooltip">i</span>
            </button>

            <ul class="dropdown-menu">
                <asp:Repeater ID="rptClaimTypeListCI" runat="server">
                    <ItemTemplate>
                        <li class="dropdown-item" onclick="changeSelectionCI('<%# Eval("Description") %>', '<%# Eval("Value") %>')">
                            <span><%# Eval("Description") %></span>
                            <span data-toggle="tooltip" data-placement="top" title="<%# Eval("Description") %>"
                                class="customTooltip">i</span>
                        </li>
                    </ItemTemplate>
                </asp:Repeater>
            </ul>
        </div>


        <!-- Below extra field only shown upon selection of certain claim type. defualt to hide. Check with BA the extra field for
                each claim type-->

        <div class="row">
            <div id="claimTypeOptdiv" runat="server" class="col-md-6">
                <asp:HiddenField ClientIDMode="Static" ID="hdnClaimTypeOptdiv" Value="" runat="server" />
                <div class="claimTypeOpt">
                    <p id="claimTypeOptLabel" runat="server" class="claimTypeOptLabel">Cause of death</p>
                    <div class="switch-field">
                        <input type="radio" id="radioOne" ClientIDMode="Static" name="switch-one" value="A" checked runat="server" />
                        <label for="radioOne">Accidental</label>
                        <input type="radio" id="radioTwo" ClientIDMode="Static" name="switch-one" value="N" runat="server" />
                        <label for="radioTwo">Other</label>
                    </div>
                </div>
            </div>

            <div id="claimTypeOpt3div" runat="server" class="col-md-12">
                <p id="claimTypeOpt3Label" runat="server" class="claimTypeOptLabel">Cause Description (Optional)</p>
                <input type="text" class="form-control adjHeight" runat="server" id="txtCauseDesc">
            </div>

            <div id="claimTypeOpt2div" runat="server" class="col-md-12">
                <div class="claimTypeOpt">
                    <asp:HiddenField ClientIDMode="Static" ID="hdnClaimTypeOpt2div" Value="" runat="server" />
                    <label id="claimTypeOpt2Label" class="claimTypeOptLabel" runat="server">Date of death</label>
                    <div class="flex dateWrapper">
                        <span style="min-width: 9%" class="custom-dropdown">
                            <asp:DropDownList runat="server" ID="dropdownDD" ClientIDMode="Static">
                                <asp:ListItem Selected="true" Value="">DD</asp:ListItem>
                            </asp:DropDownList>
                            <%--<select id="selectDD" runat="server">
                                        <option value="">DD</option>
                                    </select>--%>
                        </span>
                        <span style="min-width: 9%" class="custom-dropdown ">
                            <asp:DropDownList runat="server" ID="dropdownMM" ClientIDMode="Static">
                                <asp:ListItem Selected="true" Value="">MM</asp:ListItem>
                            </asp:DropDownList>
                            <%--<select id="selectMM" runat="server">
                                        <option value="">MM</option>
                                    </select>--%>
                        </span>
                        <span style="min-width: 9%" class="custom-dropdown ">
                            <asp:DropDownList runat="server" ID="dropdownYYYY" ClientIDMode="Static">
                                <asp:ListItem Selected="true" Value="">YYYY</asp:ListItem>
                            </asp:DropDownList>
                            <%--<select id="selectYYYY" runat="server">
                                        <option value="">YYYY</option>
                                    </select>--%>
                        </span>
                    </div>
                </div>
            </div>
        </div>

        <!-- End Extra Field -->
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

    <script type="text/javascript">
        function CheckDouble() {
            var submit = parseInt($("#<%= hdnContinue.ClientID %>").val());
            ++submit;
            $("#<%= hdnContinue.ClientID %>").val(submit);

            if (submit > 1) {
                return false;
            }
        }

        function changeSelection(selection, selectVal, selectOriVal) {
            $("#<%= hdnSelectedType.ClientID %>").val(selectVal);
        $("#<%= hdnSelectedOriType.ClientID %>").val(selectOriVal);
        $("#<%= spanSelectedType.ClientID %>").html(selection)
        $("#<%= spanSelectedTooltip.ClientID %>").prop("title", selection)

        document.getElementById('<%= btnChangeSelection.ClientID %>').click();
        }

        function changeSelectionCI(selection, selectVal) {
            $("#<%= hdnSelectedCICategory.ClientID %>").val(selectVal);
            $("#<%= spanSelectedTypeCI.ClientID %>").html(selection)
            $("#<%= spanSelectedTooltipCI.ClientID %>").prop("title", selection)
        }
    </script>
</asp:Content>
