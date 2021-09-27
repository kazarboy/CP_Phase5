<%@ Page Language="C#" Title="Corporate Portal" MasterPageFile="~/CorporatePortalSite.Master" AutoEventWireup="true" CodeBehind="Policy.aspx.cs" Inherits="EPP.CorporatePortal.Application.Policy" %>
<%@ Import namespace="EPP.CorporatePortal.Models" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
    <div class="page-heading ">
        <div class="row-wrap">
            <h1 class="page-title bold"><%=Resources.Resource.ContractPolicy %></h1>                                       
        </div>
    </div>
    <asp:HiddenField   ClientIDMode="Static" ID="hdnCorpId" value="" runat="server"/>
    <asp:HiddenField   ClientIDMode="Static" ID="hdnUCorpId" value="" runat="server"/>
    <div class="page-content">

        <%--<div class="dropdown-wrapper ">
            <label><%=Resources.Resource.ShowStatus %></label>

            <span class="custom-dropdown big">
                <select>
                    <option value="policy-status"><%=Resources.Resource.All %></option>

                </select>
            </span>

        </div>--%>

        <div class="row row-card">
                   
            <asp:Repeater ID="rptPolicies" runat="server" OnItemDataBound="rptPolicies_ItemDataBound" >
                <ItemTemplate>
                    <div class="card-col">
                        <div class="card text-center card-takaful">

                        <div class="card-header card-header-product">
                            <button class='<%#(Eval("Entity").ToString().ToUpper().Equals("ETB") ? "btn btn-takaful" :"btn btn-insurance")%>' id="btnType" runat="server"><%#(Eval("Entity").ToString().ToUpper().Equals("ETB") ? "Takaful" :"Insurance")%></button>
                            
                        </div>
                        <div class="card-body">
                            <h5 class="card-title card-title-product"><%# EPP.CorporatePortal.Models.Utility.TitleCase(Eval("ProductName").ToString()) %></h5>
                            <div class="detail">
                                <p><%#(Eval("Entity").ToString().ToUpper().Equals("ETB") ? Resources.Resource.ContractNo : Resources.Resource.PolicyNo)%></p>
                                <p><%# Eval("ContractNo") %></p>
                            </div>
                                <div class="detail">
                                <p><%#(Eval("Entity").ToString().ToUpper().Equals("ETB") ? Resources.Resource.ContractHolder : Resources.Resource.PolicyHolder)%></p>
                                <p><%# Eval("ContractHolderName") %></p>
                            </div>

                            <div class="detail">
                                <p><%#Resources.Resource.PeriodOfCover %></p>
                                <p> <%# Eval("PeriodOfCoverStart", "{0: dd/MM/yyyy}") %> <%#Resources.Resource.To %> <%# Eval("PeriodOfCoverEnd", "{0: dd/MM/yyyy}") %></p>
                            </div>

                            <div class="detail">
                                <p><%#Resources.Resource.Renewal %> </p>
                                <p><%# Eval("Renewal", "{0: dd/MM/yyyy}")%></p>
                                <%--<button class="btn btn-renew"><%#Resources.Resource.Renewal %></button>--%>
                            </div>

                            <div class="detail">
                                <p><%#Resources.Resource.Status %> </p>
                                <p><%# Eval("Status") %></p>
                                <%--<p>  <%# GetStatus(Container.DataItem) %></p>--%>
                            </div>

                        </div>
                        <div class="card-footer card-footer-product">
                            <%#Resources.Resource.ViewDetails %> 
                            <a href="PolicyDetails.aspx?PolicyId=<%# Eval("SourceId") %>&CorpId=<%=Utility.Encrypt(hdnCorpId.Value) %>&UcorpId=<%=Utility.Encrypt(hdnUCorpId.Value) %>" class="btn-detail-link">
                                <i class="fa fa-chevron-right btn-detail"></i>
                            </a>
                        </div>
                    
                                            
                    </div>
                  </div>    
                </ItemTemplate>
                <FooterTemplate>
                      <asp:Label ID="lblEmptyMsg" runat="server" CssClass="errMsg" Text="No policies found." Visible="false">
        </asp:Label>
                </FooterTemplate>
            </asp:Repeater> 
        </div>
    </div>


    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/jquery-3.5.1.js") %>"></script>
    <script type="text/javascript"  src="<%= ResolveUrl("~/Style/assets/vendor/equal-height.js") %>"></script>
    <script type="text/javascript"  src="<%= ResolveUrl("~/Style/index.js")%>"></script>
    <script type="text/javascript"  src="<%= ResolveUrl("~/Style/assets/vendor/popper.min.js") %>"></script>
    <script type="text/javascript"  src="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.js") %>"></script>
</asp:Content>