<%@ Page Language="C#" MasterPageFile="~/ClaimSubmission.Master" AutoEventWireup="true" CodeBehind="ClaimNewDocument.aspx.cs" Inherits="EPP.CorporatePortal.Application.ClaimNewDocument" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/fontawesome/css/font-awesome.min.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/dashboard.css") %>">
    <link rel="stylesheet" type="text/css" href="<%= ResolveUrl("~/Style/claim.css") %>">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="modal fade show" id="upload-file">
        <div class="modal-dialog modal-dialog-centered" role="document">
            <div class="modal-content">
                <div class="modal-header">

                    <div>
                        <h4 class="modal-title bold">Upload file</h4>
                        <%--<p class="modal-caption-p">Upload a pdf file for the related document</p>--%>
                    </div>

                </div>
                <div class="modal-body">
                    <label class="modal-label">Upload your PDF file</label>
                    <div class="fileUpload btn-file btn width100">
                        <span>+ Upload now</span>
                        <asp:HiddenField ID="hdnDocSourceIdTmp" Value='' runat="server" />
                        <asp:FileUpload ID="ctlFileUpload" runat="server" accept=".pdf" CssClass="uploadlogo" />
                    </div>

                </div>

                <div class="modal-footer">
                    <button type="button" id="btnCancelUpload" class="btn btn-secondary no-outline"
                        data-dismiss="modal">
                        Cancel</button>

                    <asp:Button Text="Submit" ID="btnUpload" OnClick="Upload" CommandName="UploadFile" UseSubmitBehavior="false" runat="server" CssClass="btn btn-primary-grey" />
                </div>
            </div>
        </div>
    </div>

    <h1 class="page-title bold container">Upload Documents</h1>

    <div class="container">
        <div class="row">
            <div class="col-md-12">
                <p id="pPDFLimitLabel" class="label" runat="server">Only PDF format is allowed and only one PDF file for each document. The file size cannot exceed XX MB in total.</p>
                <br />
            </div>
        </div>

        <div id="divMandatoryDocList" class="table-responsive" runat="server">

            <table class="table tableResult tableColWidth50">
                <thead>
                    <tr>
                        <th scope="col" colspan="2">
                            <span class="headingGreen">Mandatory Document</span>
                        </th>
                    </tr>
                </thead>

                <tbody>
                    <tr class="groupedRow">
                        <td colspan="2">
                            <asp:Repeater ID="rptMandatoryDocList" runat="server">
                                <ItemTemplate>
                                    <asp:HiddenField ID="hdnDocSourceIdCurr" Value='<%# Eval("DocumentId") %>' runat="server" />
                                    <table class="innerTable noBorder" width="100%">
                                        <tr>

                                            <td class="docName"><%# Eval("DocumentName") %></td>
                                            <td>
                                                <div class="upload-btn-wrapper">
                                                    <%--<label class="col-form-label btnUpload" for="btnUploadGallery">
                                                                    Upload
                                                                file</label>--%>
                                                    <div class="gallery-item-wrapper-0">
                                                        <div id="btnUploadGallery" class="btnUpload filestyles gallery-images" onclick='processUpload(<%# Eval("DocumentId") %>)' data-toggle="modal" data-target="#upload-file" style="cursor: pointer">Upload file</div>
                                                        <%--<asp:FileUpload ID="FileUploadDoc" name="images[]" class="filestyles gallery-images" onchange="Upload" CommandArgument='<%# Eval("DocumentId") %>' CommandName="UploadFile" ClientIDMode="Static" runat="server"/>--%>
                                                        <%--<input name="images[]" type="file" multiple
                                                                        class="filestyles gallery-images" id="photo-gallery-<%# Eval("DocumentId") %>" runat="server" />--%>
                                                    </div>
                                                </div>

                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="2" style="padding: 0px 15px">
                                                <div id="divGalleryImg" class="gallery-id row" style='display: none;' runat="server">
                                                    <div class="col-md-2">
                                                        <img class="fileThumbnail" src="<%# ResolveUrl("~/Style/assets/img/fileThumbnail.png") %>">
                                                        <br>
                                                        <p id="pFileName" class="float-left" runat="server"></p>
                                                        <asp:LinkButton ID="btnRemoveImg" CssClass="fa fa-trash-o float-right" aria-hidden="true" runat="server" CommandName="RemoveImg" CommandArgument='<%# Eval("DocumentId") %>' OnClick="RemoveImg"></asp:LinkButton>
                                                    </div>
                                                </div>
                                            </td>
                                        </tr>

                                    </table>
                                </ItemTemplate>
                            </asp:Repeater>
                        </td>
                    </tr>


                </tbody>
            </table>
        </div>

        <div id="divSupportingDocList" class="table-responsive" runat="server">

            <table class="table tableResult tableColWidth50">
                <thead>
                    <tr>
                        <th scope="col" colspan="2">
                            <span class="headingGreen">Supporting Document</span>
                        </th>

                    </tr>
                </thead>

                <tbody>
                    <tr class="groupedRow">
                        <td colspan="2">
                            <asp:Repeater ID="rptSupportingDocList" runat="server">
                                <ItemTemplate>
                                    <asp:HiddenField ID="hdnDocSourceIdCurr" Value='<%# Eval("DocumentId") %>' runat="server" />
                                    <table class="innerTable noBorder" width="100%">
                                        <tr>

                                            <td class="docName"><%# Eval("DocumentName") %></td>
                                            <td>
                                                <div class="upload-btn-wrapper">
                                                    <%--<label class="col-form-label btnUpload" for="btnUploadGallery">
                                                                    Upload
                                                                file</label>--%>
                                                    <div class="gallery-item-wrapper-0">
                                                        <div id="btnUploadGallery" class="btnUpload filestyles gallery-images" onclick='processUpload(<%# Eval("DocumentId") %>)' data-toggle="modal" data-target="#upload-file" style="cursor: pointer">Upload file</div>
                                                        <%--<asp:FileUpload ID="FileUploadDoc" name="images[]" class="filestyles gallery-images" onchange="Upload" CommandArgument='<%# Eval("DocumentId") %>' CommandName="UploadFile" ClientIDMode="Static" runat="server"/>--%>
                                                        <%--<input name="images[]" type="file" multiple
                                                                        class="filestyles gallery-images" id="photo-gallery-<%# Eval("DocumentId") %>" runat="server" />--%>
                                                    </div>
                                                </div>

                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="2" style="padding: 0px 15px">
                                                <div id="divGalleryImg" class="gallery-id row" style='display: none;' runat="server">
                                                    <div class="col-md-2">
                                                        <img class="fileThumbnail" src="<%# ResolveUrl("~/Style/assets/img/fileThumbnail.png") %>">
                                                        <br>
                                                        <p id="pFileName" class="float-left" runat="server"></p>
                                                        <asp:LinkButton ID="btnRemoveImg" CssClass="fa fa-trash-o float-right" aria-hidden="true" runat="server" CommandName="RemoveImg" CommandArgument='<%# Eval("DocumentId") %>' OnClick="RemoveImg"></asp:LinkButton>
                                                    </div>
                                                </div>
                                            </td>
                                        </tr>

                                    </table>
                                </ItemTemplate>
                            </asp:Repeater>
                        </td>
                    </tr>

                    <tr class="groupedRow">
                        <td colspan="2"></td>
                    </tr>


                </tbody>
            </table>
        </div>

        <div id="divOverseasButton" class="row" runat="server">
            <div class="col-md-12 ">
                <div class="checkedRow ">
                    <label class="containerCC v2" for="deathOccurOversea">
                        <input type="checkbox" name="radio" id="deathOccurOversea" onclick="showExtraField('deathOccurOversea')" ClientIDMode="Static" runat="server">
                        <span class="checkmark"></span>
                        <span>If death occured in overseas</span>
                    </label>
                </div>
            </div>

        </div>

        <br>

        <!------Additional Field -->

        <div id="divOverseasMandatoryDocList" class="table-responsive extraFieldDeath" runat="server">

            <table id="tableOverseasMandatoryDocList" class="table tableResult tableColWidth50" runat="server">
                <thead>
                    <tr>
                        <th scope="col" colspan="2">
                            <span class="headingGreen">Mandatory Document</span>
                        </th>
                    </tr>
                </thead>

                <tbody>
                    <tr class="groupedRow">
                        <td colspan="2">
                            <asp:Repeater ID="rptOverseasMandatoryDocList" runat="server">
                                <ItemTemplate>
                                    <asp:HiddenField ID="hdnDocSourceIdCurr" Value='<%# Eval("DocumentId") %>' runat="server" />
                                    <table class="innerTable noBorder" width="100%">
                                        <tr>

                                            <td class="docName"><%# Eval("DocumentName") %></td>
                                            <td>
                                                <div class="upload-btn-wrapper">
                                                    <%--<label class="col-form-label btnUpload" for="btnUploadGallery">
                                                                    Upload
                                                                file</label>--%>
                                                    <div class="gallery-item-wrapper-0">
                                                        <div id="btnUploadGallery" class="btnUpload filestyles gallery-images" onclick='processUpload(<%# Eval("DocumentId") %>)' data-toggle="modal" data-target="#upload-file" style="cursor: pointer">Upload file</div>
                                                        <%--<asp:FileUpload ID="FileUploadDoc" name="images[]" class="filestyles gallery-images" onchange="Upload" CommandArgument='<%# Eval("DocumentId") %>' CommandName="UploadFile" ClientIDMode="Static" runat="server"/>--%>
                                                        <%--<input name="images[]" type="file" multiple
                                                                        class="filestyles gallery-images" id="photo-gallery-<%# Eval("DocumentId") %>" runat="server" />--%>
                                                    </div>
                                                </div>

                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="2" style="padding: 0px 15px">
                                                <div id="divGalleryImg" class="gallery-id row" style='display: none;' runat="server">
                                                    <div class="col-md-2">
                                                        <img class="fileThumbnail" src="<%# ResolveUrl("~/Style/assets/img/fileThumbnail.png") %>">
                                                        <br>
                                                        <p id="pFileName" class="float-left" runat="server"></p>
                                                        <asp:LinkButton ID="btnRemoveImg" CssClass="fa fa-trash-o float-right" aria-hidden="true" runat="server" CommandName="RemoveImg" CommandArgument='<%# Eval("DocumentId") %>' OnClick="RemoveImg"></asp:LinkButton>
                                                    </div>
                                                </div>
                                            </td>
                                        </tr>

                                    </table>
                                </ItemTemplate>
                            </asp:Repeater>
                        </td>
                    </tr>


                </tbody>
            </table>
        </div>

        <div id="divOverseasSupportingDocList" class="table-responsive extraFieldDeath" runat="server">

            <table id="tableOverseasSupportingDocList" class="table tableResult tableColWidth50" runat="server">
                <thead>
                    <tr>
                        <th scope="col" colspan="2">
                            <span class="headingGreen">Supporting Document</span>
                        </th>

                    </tr>
                </thead>

                <tbody>
                    <tr class="groupedRow">
                        <td colspan="2">
                            <asp:Repeater ID="rptOverseasSupportingDocList" runat="server">
                                <ItemTemplate>
                                    <asp:HiddenField ID="hdnDocSourceIdCurr" Value='<%# Eval("DocumentId") %>' runat="server" />
                                    <table class="innerTable noBorder" width="100%">
                                        <tr>

                                            <td class="docName"><%# Eval("DocumentName") %></td>
                                            <td>
                                                <div class="upload-btn-wrapper">
                                                    <%--<label class="col-form-label btnUpload" for="btnUploadGallery">
                                                                    Upload
                                                                file</label>--%>
                                                    <div class="gallery-item-wrapper-0">
                                                        <div id="btnUploadGallery" class="btnUpload filestyles gallery-images" onclick='processUpload(<%# Eval("DocumentId") %>)' data-toggle="modal" data-target="#upload-file" style="cursor: pointer">Upload file</div>
                                                        <%--<asp:FileUpload ID="FileUploadDoc" name="images[]" class="filestyles gallery-images" onchange="Upload" CommandArgument='<%# Eval("DocumentId") %>' CommandName="UploadFile" ClientIDMode="Static" runat="server"/>--%>
                                                        <%--<input name="images[]" type="file" multiple
                                                                        class="filestyles gallery-images" id="photo-gallery-<%# Eval("DocumentId") %>" runat="server" />--%>
                                                    </div>
                                                </div>

                                            </td>
                                        </tr>

                                        <tr>
                                            <td colspan="2" style="padding: 0px 15px">
                                                <div id="divGalleryImg" class="gallery-id row" style='display: none;' runat="server">
                                                    <div class="col-md-2">
                                                        <img class="fileThumbnail" src="<%# ResolveUrl("~/Style/assets/img/fileThumbnail.png") %>">
                                                        <br>
                                                        <p id="pFileName" class="float-left" runat="server"></p>
                                                        <asp:LinkButton ID="btnRemoveImg" CssClass="fa fa-trash-o float-right" aria-hidden="true" runat="server" CommandName="RemoveImg" CommandArgument='<%# Eval("DocumentId") %>' OnClick="RemoveImg"></asp:LinkButton>
                                                    </div>
                                                </div>
                                            </td>
                                        </tr>

                                    </table>
                                </ItemTemplate>
                            </asp:Repeater>
                        </td>
                    </tr>


                </tbody>
            </table>
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
            showExtraField('deathOccurOversea');
        });

        function CheckDouble() {
            var submit = parseInt($("#<%= hdnContinue.ClientID %>").val());
            ++submit;
            $("#<%= hdnContinue.ClientID %>").val(submit);

            if (submit > 1) {
                return false;
            }
        }

        function imagesPreview() {
            var placeToInsertImagePreview = 'div.gallery-' + $("#<%= hdnDocSourceIdTmp.ClientID %>").val();
            var input = $('#<%= ctlFileUpload.ClientID %>')[0];

            if (input.files) {

                let path = "<%= ResolveUrl("~/Style/assets/img/fileThumbnail.png") %>"
                var filesAmount = input.files.length;

                let inputName = $("#" + input.id).val().split("\\").pop()

                for (i = 0; i < filesAmount; i++) {
                    var reader = new FileReader();

                    reader.onload = function (event) {
                        var htmlImage = '<div class="col-md-2">';
                        htmlImage = htmlImage + '<img  class="fileThumbnail" src="' + path + '" />';
                        htmlImage = htmlImage + '<br><p class="float-left">' + inputName +
                            '</p><i class="fa fa-trash-o float-right" aria-hidden="true" onclick="removeImage($(this))"></i>';
                        htmlImage = htmlImage + '</div>';
                        $($.parseHTML(htmlImage)).appendTo(placeToInsertImagePreview);
                    }

                    reader.readAsDataURL(input.files[i]);
                }
            }

        };

        function processUpload(targetID) {
            $("#<%= hdnDocSourceIdTmp.ClientID %>").val(targetID);
        }

        function removeImage(item) {
            item.parent().remove();
        }

        function showExtraField(id) {
            if ($('#' + id).is(":checked")) {
                $(".extraFieldDeath").show()
            }

            else {
                $(".extraFieldDeath").hide()
            }
        }
    </script>
</asp:Content>
