<%@ Page Language="C#" MasterPageFile="~/ClaimSubmission.Master" AutoEventWireup="true" CodeBehind="ClaimNewBank.aspx.cs" Inherits="EPP.CorporatePortal.Application.ClaimNewBank" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/fontawesome/css/font-awesome.min.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/dashboard.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/claim.css") %>">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="container">

        <h1 class="page-title container bold ">Bank details</h1>


        <div class="container">

            <div class="row">
                <div class="col-md-12">
                    <p class="label">Please verify the bank account details for claim payment</p>
                </div>
            </div>

            <asp:HiddenField ID="hdnPolicySourceIdTmp" Value="" runat="server" ClientIDMode="Static" />
            <asp:Repeater ID="rptClaimBankList" runat="server">
                <ItemTemplate>
                    <div class="row">

                        <div class="col-md-3 divHeading">
                            <div class="divHeadingItem">Product Name</div>
                            <div class="divHeadingItem"><%# Eval("ProductName") %></div>
                        </div>

                        <div class="col-md-3 divHeading">
                            <div class="divHeadingItem">Policy/Contract No</div>
                            <div class="divHeadingItem"><%# Eval("ContractNo") %></div>
                        </div>
                    </div>


                    <div class="table-responsive">

                        <table class="table tableResult tableColWidthEqual">
                            <thead>
                                <tr>
                                    <th scope="col">Account Holder Name</th>
                                    <th scope="col">Bank Name</th>
                                    <th scope="col">Account No</th>
                                    <th scope="col">Bank ROC</th>
                                    <th scope="col"></th>
                                </tr>
                            </thead>

                            <tbody>
                                <asp:HiddenField ID="hdnBankAccountNo" Value='<%# Eval("HiddenBankAccountNo") %>' runat="server" />
                                <tr id="trBankAccount" class="groupedRow" runat="server">
                                    <td colspan="5">
                                        <table class="innerTable" width="100%">
                                            <tr class="trBankCheckbox" style="cursor: pointer;">
                                                <td><span id="spanName" runat="server" class="policyLabel"><%# Eval("Name") %></span></td>
                                                <td><span id="spanBankBranchName" runat="server" class="policyLabel"><%# Eval("BankBranchName") %></span></td>                                                
                                                <td><span id="spanBankAccountNo" runat="server" class="policyLabel"><%# Eval("BankAccountNo") %></span></td>
                                                <td><span id="spanBankROC" runat="server" class="policyLabel"><%# Eval("CorporateSourceId") %></span></td>
                                                <td>
                                                    <label class="containerCC">
                                                        <input type="checkbox" class='<%# string.Format("bankAcc{0} bankAcc", Eval("PolicySourceId")) %>' id="checkmarkCheckbox"
                                                            onchange='<%# string.Format("bankSelected(this,\"{0}\")", Eval("PolicySourceId")) %>' runat="server">
                                                        <span class="checkmark"></span>
                                                    </label>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <%--Additional Bank Account--%>
                                <asp:HiddenField ID="hdnAddBankAccountNo" Value="" runat="server" />
                                <asp:HiddenField ID="hdnAddBankROC" Value="" runat="server" />
                                <asp:HiddenField ID="hdnAddBankBranchCode" Value="" runat="server" />
                                <tr id="trAdd" class="groupedRow" style="display: none" runat="server">
                                    <td colspan="5">
                                        <table class="innerTable" width="100%">
                                            <tr class="trBankCheckbox" style="cursor: pointer;">
                                                <td><span id="spanAddName" runat="server" class="policyLabel"></span></td>                                                
                                                <td><span id="spanAddBankBranchName" runat="server" class="policyLabel"></span></td>                                                
                                                <td><span id="spanAddBankAccountNo" runat="server" class="policyLabel"></span></td>
                                                <td><span id="spanAddBankROC" runat="server" class="policyLabel"></span></td>
                                                <td>
                                                    <label class="containerCC">
                                                        <input type="checkbox" class='<%# string.Format("bankAcc{0} bankAcc", Eval("PolicySourceId")) %>' id="checkmarkCheckboxAdd"
                                                            onchange='<%# string.Format("bankSelected(this,\"{0}\")", Eval("PolicySourceId")) %>' runat="server">
                                                        <span class="checkmark"></span>
                                                    </label>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                    <br>

                    <asp:HiddenField ID="hdnPolicySourceIdCurr" Value='<%# Eval("PolicySourceId") %>' runat="server" />
                    <p class="label labelGreen" onclick="setPolicyId('<%# Eval("PolicySourceId") %>');" data-toggle="modal" data-target="#addNew">
                        <a href="#">+ Update New Bank Account
                        </a>
                    </p>

                    <br>
                    <hr>
                </ItemTemplate>
            </asp:Repeater>

            <asp:Repeater ID="rptClaimBankList2" runat="server" OnItemDataBound="rptClaimBankList2_ItemDataBound">
                <ItemTemplate>
                    <%--2nd item--%>
                    <div class="row">
                        <div class="col-md-3 divHeading">
                            <div class="divHeadingItem">Product Name</div>
                            <div class="divHeadingItem"><%# Eval("ProductName") %></div>
                        </div>

                        <div class="col-md-4 divHeading">
                            <div class="divHeadingItem">Polic/Contract No</div>
                            <div class="divHeadingItem"><%# Eval("ContractNo") %></div>
                        </div>

                        <div class="col-md-6">
                            <div class="claimTypeOpt">
                                <div class="switch-field v3">
                                    <input type="radio" id="radioone" name='<%# string.Format("switch-one{0}", Eval("PolicySourceId")) %>' value="yes" checked="" onclick="handleClick(this);" runat="server">
                                    <label id="labelRadioOne" for="radioone" runat="server">Pay to Member/Nominee</label>
                                    <input type="radio" id="radiotwo" name='<%# string.Format("switch-one{0}", Eval("PolicySourceId")) %>' value="no" onclick="handleClick(this);" runat="server">
                                    <label id="labelRadioTwo" for="radiotwo" runat="server">Pay to Company</label>
                                </div>
                            </div>
                        </div>

                        <asp:HiddenField ID="hdnPolicySourceIdCurr" Value='<%# Eval("PolicySourceId") %>' runat="server" />
                        <div id="divNomineeAddButton" class="col-md-12" runat="server">
                            <div id="divNomineeAdd" class="addDetailsSect" onclick="setPolicyIdNominee('<%# Eval("PolicySourceId") %>');" data-toggle="modal" data-target="#nomineeBankDetails">
                                Key in Member/Nominee bank details
                            </div>
                        </div>
                        <div id="divNomineeNew" class="table-responsive" style="display: none" runat="server">
                            <table class="table tableResult tableColWidthEqual">
                                <thead>
                                    <tr>
                                        <th scope="col">Account Holder Name</th>
                                        <th scope="col">Bank Name</th>
                                        <th scope="col">Account No</th>
                                        <th id="thBankROC" runat="server" scope="col">Bank ROC</th>
                                        <th scope="col"></th>
                                    </tr>
                                </thead>

                                <tbody>
                                    <%--Additional Bank Account--%>
                                    <asp:HiddenField ID="hdnAddBankROC" Value="" runat="server" />
                                    <asp:HiddenField ID="hdnAddBankAccountNo" Value="" runat="server" />
                                    <asp:HiddenField ID="hdnNomineeIDType" Value="" runat="server" />
                                    <asp:HiddenField ID="hdnNomineeIDNo" Value="" runat="server" />
                                    <asp:HiddenField ID="hdnNomineeContactNo" Value="" runat="server" />
                                    <asp:HiddenField ID="hdnNomineeEmailAddress" Value="" runat="server" />
                                    <asp:HiddenField ID="hdnAddBankBranchCode" Value="" runat="server" />
                                    <tr class="groupedRow" runat="server">
                                        <td id="tdAddColspan" colspan="5" runat="server">
                                            <table class="innerTable" width="100%">
                                                <tr class="trBankCheckbox" style="cursor: pointer;">
                                                    <td><span id="spanAddName" runat="server" class="policyLabel"></span></td>                                                    
                                                    <td><span id="spanAddBankBranchName" runat="server" class="policyLabel"></span></td>                                                    
                                                    <td><span id="spanAddBankAccountNo" runat="server" class="policyLabel"></span></td>
                                                    <td id="tdBankROC" runat="server"><span id="spanAddBankROC" runat="server" class="policyLabel"></span></td>
                                                    <td>
                                                        <label class="containerCC">
                                                            <input type="checkbox" class='<%# string.Format("bankAcc{0} bankAcc", Eval("PolicySourceId")) %>' id="checkmarkCheckboxAdd"
                                                                onchange='<%# string.Format("bankSelected(this,\"{0}\")", Eval("PolicySourceId")) %>' runat="server">
                                                            <span class="checkmark"></span>
                                                        </label>
                                                    </td>
                                                </tr>
                                            </table>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>   
                    </div>
                </ItemTemplate>
            </asp:Repeater>
            <br />
            <p style="display: none" id="labelNomineeUpdateBank" class="label labelGreen" onclick='<%# string.Format("setPolicyId(\"{0}\")", Eval("PolicySourceId")) %>' data-toggle="modal" data-target="#nomineeBankDetails" runat="server">
                    <a href="#">+ Update New Bank Account
                </a>
            </p>
        </div>
    </div>

    <!---------------Popup add new nominee bank details ----->

    <div class="modal fade" id="nomineeBankDetails">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="exampleModalLabel">Key In Member/Nominee Bank Details</h5>

                </div>
                <div class="modal-body">

                    <br>
                    <div class="row  row-input">
                        <div class="col-md-12">
                            <label>Account Holder Name</label>
                            <input id="nomineeName" type="text" class="form-control" runat="server">
                        </div>

                        <div class="col-md-6">
                            <label>ID Type</label>
                            <div class="custom-dropdown">
                                <select id="nomineeIDType" runat="server" clientidmode="Static">
                                    <option value="NRIC">NRIC</option>
                                    <option value="OtherIDNo">Other ID No</option>
                                </select>
                            </div>
                        </div>

                        <div class="col-md-6">
                            <label>ID Number</label>
                            <input id="nomineeIDNo" type="number" class="form-control" runat="server">
                        </div>

                        <div class="col-md-12">
                            <label>Bank Name</label>
                            <div class="custom-dropdown">
                                <asp:DropDownList runat="server" ID="nomineeBankBranchDDL" ClientIDMode="Static">
                                    <asp:ListItem Selected="true" Value="Maybank">Maybank</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>


                        <div class="col-md-12">
                            <label>Bank Acc No.</label>
                            <input id="nomineeBankAccNo" type="number" class="form-control" runat="server">
                        </div>

                        <div class="col-md-6">
                            <label>Contact No (Optional)</label>
                            <input id="nomineeContact" type="number" class="form-control" runat="server">
                        </div>

                        <div class="col-md-6">
                            <label>Email Address (Optional)</label>
                            <input id="nomineeEmail" type="email" class="form-control" runat="server">
                        </div>

                    </div>


                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary v2" data-dismiss="modal">Cancel</button>
                    <button class="btn btn-primary v2" type="button" onserverclick="AddNominee" runat="server">Done</button>
                    <%--<button type="button" class="btn btn-primary v2">Done</button>--%>
                </div>
            </div>
        </div>
    </div>

    <!-------------- Popup WIndow Update Bank Acc ----------->

    <div class="modal fade" id="addNew">
        <asp:HiddenField ID="hdnAddNewBankRptType" Value="" runat="server" />
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="exampleModalLabel">Add New Bank Details</h5>

                </div>
                <div class="modal-body">

                    <br>
                    <div class="row  row-input">
                        <div class="col-md-12">
                            <label>Account Holder Name</label>
                            <input id="newBankName" type="text" class="form-control" runat="server">
                        </div>

                        <div class="col-md-12">
                            <label>Bank Name</label>
                            <div class="custom-dropdown">
                                <asp:DropDownList runat="server" ID="newBankBranchDDL" ClientIDMode="Static">
                                    <asp:ListItem Selected="true" Value="Maybank">Maybank</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>

                        <div class="col-md-12">
                            <label>Bank Account No.</label>
                            <input id="newBankAccNo" type="number" class="form-control" runat="server">
                        </div>

                        <div class="col-md-12">
                            <label>ROC No. (Company Registration No.)</label>
                            <input id="newBankROC" type="text" class="form-control" runat="server">
                        </div>
                    </div>


                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary v2" data-dismiss="modal">Cancel</button>
                    <button class="btn btn-primary v2" type="button" onserverclick="AddBank" runat="server">Add</button>
                    <%--<button type="button" class="btn btn-primary v2">Add</button>--%>
                </div>
            </div>
        </div>
    </div>


    <!------------- End Popup Window Update Bank Acc--------------->

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
            highlightAll();

            $('tr.trBankCheckbox').click(function (event) {
                if (event.target.type !== 'checkbox') {
                    $(':checkbox', this).trigger('click');
                }
            });

            $('.checkmark').on('click', function (e) {
                e.stopImmediatePropagation()
            });
        });
        function handleClick(myRadio) {

            if (myRadio.value == "yes") {
                $('#divNomineeAdd').attr('data-target', '#nomineeBankDetails');
                $('#<%= labelNomineeUpdateBank.ClientID %>').attr('data-target', '#nomineeBankDetails');
            }
            else {
                $('#divNomineeAdd').attr('data-target', '#addNew');
                $('#<%= labelNomineeUpdateBank.ClientID %>').attr('data-target', '#addNew');
            }
        }
        function CheckDouble() {
            var submit = parseInt($("#<%= hdnContinue.ClientID %>").val());
            ++submit;
            $("#<%= hdnContinue.ClientID %>").val(submit);

            if (submit > 1) {
                return false;
            }
        }
        function bankSelected(event, ClassString) {

            check(event, ClassString);
        }
        function setPolicyId(PolicyId) {
            $("#<%= hdnPolicySourceIdTmp.ClientID %>").val(PolicyId);
        }
        function setPolicyIdNominee(PolicyId) {
            $("#<%= hdnPolicySourceIdTmp.ClientID %>").val(PolicyId);
            $("#<%= hdnAddNewBankRptType.ClientID %>").val("Nominee");
        }
        function check(input, ClassString) {

            var checkboxes = document.getElementsByClassName("bankAcc" + ClassString);

            var checkedCheck = input.checked;

            for (var i = 0; i < checkboxes.length; i++) {
                //uncheck all
                if (checkboxes[i].checked == true) {
                    checkboxes[i].checked = false;
                }
                highlight(checkboxes[i]);
            }

            input.checked = checkedCheck;
            //highlight(input);
            highlightAll();
        }
        function highlight(event) {

            if ($("#" + event.id).is(":checked")) {
                $("#" + event.id).parentsUntil("tbody").addClass("activeRow")
                $("td").removeClass("activeRow")
                $("label").removeClass("activeRow")
            } else {
                $("#" + event.id).parentsUntil("tbody").removeClass("activeRow")
            }
        }
        function highlightAll() {
            $('.bankAcc').each(function () {
                if ($(this).is(":checked")) {
                    $(this).parentsUntil("tbody").addClass("activeRow")
                    $("td").removeClass("activeRow")
                    $("label").removeClass("activeRow")
                } else {
                    $(this).parentsUntil("tbody").removeClass("activeRow")
                }
            });
        }
    </script>
</asp:Content>
