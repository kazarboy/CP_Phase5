<%@ Page Language="C#" Title="Corporate Portal" MasterPageFile="~/CorporatePortalSite.Master" AutoEventWireup="true" CodeBehind="UserGuide.aspx.cs" Inherits="EPP.CorporatePortal.Application.UserGuide" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <div class="page-heading ">
        <div class="row-wrap">
            <h1 class="page-title bold"><%=Resources.Resource.UserGuide %></h1>
        </div>
    </div>
    <asp:HiddenField ClientIDMode="Static" ID="hdnCorpId" Value="" runat="server" />
    <div class="page-content">
        <%--<iframe
        src="../Attachments/Documents/CP_Guide.pdf"
        scrolling="auto"
        height="500px"
        width="100%"
        ></iframe>--%>

        <embed src="<%= UserGuideFrame %>" type="application/pdf" height="500px" width="100%" id="pdfcontainer"></embed>
    </div>

    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/jquery-3.5.1.js") %>"></script>
    <script type="text/javascript"  src="<%= ResolveUrl("~/Style/index.js")%>"></script>
    <script type="text/javascript"  src="<%= ResolveUrl("~/Style/assets/vendor/popper.min.js") %>"></script>
   
</asp:Content>
