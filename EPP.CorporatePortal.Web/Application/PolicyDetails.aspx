<%@ Page Language="C#" Title="Corporate Portal"  MasterPageFile="~/CorporatePortalSite.Master" AutoEventWireup="true" CodeBehind="PolicyDetails.aspx.cs" Inherits="EPP.CorporatePortal.Application.PolicyDetails" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
 
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" Runat="Server">
    <asp:HiddenField   ClientIDMode="Static" ID="hdnPolicyId" value="" runat="server"/>
    <asp:HiddenField   ClientIDMode="Static" ID="hdnCorpId" value="" runat="server"/>
    <asp:HiddenField   ClientIDMode="Static" ID="hdnUCorpId" value="" runat="server"/>

                    <%--List of policies--%>
          <%--<asp:Repeater ID="rptPolicies" runat="server"> 
               <ItemTemplate>
                    <div class="statusCard mtop28 card-takaful" id="divPolicy_<%# Eval("SourceId") %>">
                        <div class="row">
                            <div class="col-10">
                                <h5 class="statusText1 insurance"><%# Eval("ProductName") %></h5>
                                <h5 class="statusText2"><%# Eval("ContractNo") %></h5>
                            </div>
                            <div class="col-2 arrowStatus">
                         
                                <span class="bold"><a href="PolicyDetails.aspx?PolicyId=<%# Encrypt(Eval("SourceId").ToString())%>&CorpId=<%= Encrypt(hdnCorpId.Value) %>">&gt;</a>  </span>
                            </div>
                        </div>
                    </div> 
               </ItemTemplate>
          </asp:Repeater> --%>
                    <%--End of List of policies--%>


     <div class="page-heading ">
                    
           <div class="row-wrap">


                <h1 class="page-title bold" id="hTitle" runat="server">
	                <span class="btn btn-takaful" id="spanBtn" runat="server">Group</span><br />
	                <asp:Label ID="lblTitle" runat="server"></asp:Label>
	
	            </h1>
                            
                            
                            
                </div>
            </div>
                    
            <div class="tab-menu-wrapper">
                        <button class="btn-tab tablink btn active" onclick="openTab(event,'policy-details');return false;" runat="server" id="tabPolicyDtails">
                        Policy Details</button>

                        <button class="btn-tab tablink btn" onclick="openTab(event,'coverage-and-benefits');return false;">
                        Coverage and Benefits</button>

                        <%--<button class="btn-tab tablink btn" onclick="openTab(event,'invoices-and-payment');return false;">Invoices &
                            Payment</button>

                        <button class="btn-tab tablink btn" onclick="openTab(event,'claim');return false;">Claims</button>--%>

                        <button class="btn-tab tablink btn"
                            onclick="openTab(event,'insured-group-subsidiaries');return false;" id="btnTabInsured" runat="server">Insured Group/ Subsidiaries</button>

                        <%--<button class="btn-tab tablink btn" onclick="openTab(event,'agent');return false;">Agent</button>

                        <button class="btn-tab tablink btn" onclick="openTab(event,'downloads');return false;">Downloads</button>--%>
                    </div>
                    <div id="policy-details" class="tab" style="display:block">
                        <h2 class="tab-title bold" id="hPolicyDetail" runat="server">Policy Details</h2>


                        <div class="details-wrapper">

                            <div class="row text-left pbtmResponsive">
                                <div class="col-lg-3 col-sm-3  pbtm4">
                                	<span class="policyLabel">Corporate Name</span>
                                </div>
                                
                                <div class="col-lg-9 col-sm-9 ">
                                	<span id="spanCorporateName" runat= "server"  class="policyInput"> </span>
                               	</div>

                                <%--<div class="col-lg-3 col-sm-3  pbtm4"><span class="policyLabel">ID Type</span></div>
                                <div class="col-lg-9 col-sm-9 "><span class="policyInput">Company Reg No</span></div>--%>

                                <div class="col-lg-3 col-sm-3  pbtm4">
                                	<span class="policyLabel">Business Registration No</span>
                                </div>
                                <div class="col-lg-9 col-sm-9 ">
                                	<span id="spanIdNo" runat= "server"  class="policyInput" > </span>
                                </div>

                                <div class="col-lg-3 col-sm-3  pbtm4">
                                	<span class="policyLabel">Correspondence Address</span>
                                </div>
                                <div class="col-lg-9 col-sm-9 ">
                                	<span id="spanAddress1" runat= "server"  class="policyLabel"> </span>
                                </div>
                                <div class="col-lg-3 col-sm-3  pbtm4">
                                	<span class="policyLabel"></span>
                                </div>
                                <div class="col-lg-9 col-sm-9 ">
                                	<span id="spanAddress2" runat= "server"  class="policyLabel"> </span>
                                </div>

                                <div class="col-lg-3 col-sm-3  pbtm4">
                                	<span class="policyLabel"></span>
                                </div>
                                
                                <div class="col-lg-9 col-sm-9 ">
                                	<span id="spanAddress3" runat= "server" class="policyLabel" > </span>
                                </div>

                                <div class="col-lg-3 col-sm-3  pbtm4">
                                	<span class="policyLabel"></span>
                                </div>
                                
                                <div class="col-lg-9 col-sm-9 ">
                                	<span id="spanAddress4" runat= "server" class="policyLabel" > </span>
                                </div>

                               <%-- <div class="col-lg-3 col-sm-3  pbtm4">
                                	<span class="policyLabel"></span>
                                </div>--%>
                                
                                <%--<div class="col-lg-9 col-sm-9 ">
                                	<span id="spanAddress5" runat= "server" class="policyLabel" > </span>
                                </div>--%>

                                <%--<div class="col-lg-3 col-sm-3  pbtm4">
                                	<span class="policyLabel"></span>
                                </div>
                                <div class="col-lg-9 col-sm-9 ">
                                	<span id="spanAddress6" runat= "server" class="policyLabel" > </span>
                                </div>--%>

                                <div class="col-lg-3 col-sm-3  pbtm4"><span class="policyLabel">Covered From</span></div>
                                <div class="col-lg-9 col-sm-9 "><span id="spanCoveredFrom" runat= "server" class="policyInput"> </span></div>

                                <div class="col-lg-3 col-sm-3  pbtm4"><span class="policyLabel">Covered To</span></div>
                                <div class="col-lg-6 col-sm-3"><span  id="spanCoveredTo" runat= "server" class="policyInput"> </span></div>

                                <div class="col-lg-3 col-sm-6 ">
                                	<span class="link"><%--<a href="#">Request for
                                            Extension</a>--%></span>
                                </div>

                                <div class="col-lg-3 col-sm-3  pbtm4"><span class="policyLabel">Renewal Due</span></div>
                                <div class="col-lg-6 col-sm-3"><span id="spanRenewal" runat= "server"  class="policyInput"> </span></div>
                                <div class="col-lg-3 col-sm-6 ">
                                	<span class="link"><%--<a href="#">Renew Now</a>--%></span>
                                </div>

                                <div id="divPolicyProductLabel" runat= "server" class="col-lg-3 col-sm-3  pbtm4"><span id="spanPolicyProduct1Label" runat= "server" class="policyLabel"> </span></div>
                                <div id="divPolicyProduct" runat= "server" class="col-lg-9 col-sm-9"><span id="spanPolicyProduct1" runat= "server"  class="policyInput"></span></div>

                                <div id="divPolicyProduct2Label" runat= "server" class="col-lg-3 col-sm-3  pbtm4"><span id="spanPolicyProduct2Label" runat= "server" class="policyLabel"> </span></div>
                                <div id="divPolicyProduct2" runat= "server" class="col-lg-9 col-sm-9"><span id="spanPolicyProduct2" runat= "server"  class="policyInput"></span></div>

                                <%--<div class="col-lg-3 col-sm-3  pbtm4"><span class="policyLabel">Third Party Administrator</span></div>
                                <div class="col-lg-9 col-sm-9 "><span id="spanThirdPartyAdministrator" runat= "server" class="policyInput"> </span></div>--%>

                                <div class="col-lg-3 col-sm-3  pbtm4"><span class="policyLabel">Bank Detail</span></div>
                                <div class="col-lg-9 col-sm-9 "><span id="spanBankDetail" runat= "server"  class="policyInput"></span></div>

                                <div class="col-lg-3 col-sm-3  pbtm4"><span class="policyLabel">Account Manager(Etiqa)</span></div>
                                <div class="col-lg-9 col-sm-9 "><span id="spanAccountManager" runat= "server"  class="policyInput"></span></div>
             

                            </div>
                        </div>

                    </div>
                    <div id="coverage-and-benefits" class="tab" style="display:none">

                                    <div class="table-responsive">
                                        <table class="table table-no-border table-responsive">
                                            
                                            <tbody>
                                                <tr>
                                                   
                                                    <td style="border:none"><a runat="server" id ="aSOB" download>Click here to Download</a></td>
                                                    <td style="border:none"></td>
                                                </tr> 
                                            </tbody>
                                        
                                        </table>
                                    </div>


                                </div>
                    
                    <div id="insured-group-subsidiaries" class="tab" style="display:none">
                                    <h2 class="tab-title bold" id="h2Insured" runat="server">Insured Group Subsidiaries</h2>

                                    <div class="table-responsive">
                                        <table class="table table-semi-border ">
                                            <thead>
                                                <tr>
                                                    <th scope="col">#</th>
                                                    <th scope="col">Company Name</th>
                                                    <th scope="col" id="thTotalInsured" runat="server">Total Insured Number</th>
                                                    <th scope="col"></th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <asp:Repeater runat="server" ID="rptInsuredGroupSubsidaries">
                                                <ItemTemplate>
                                                <tr>
                                                    <td><%# Container.ItemIndex + 1 %></td>
                                                    <td><%#Eval("Company Name") %> [ <%#Eval("Status") %>]</td>
                                                    <td align="center"><%#Eval("Total Insured Number") %></td>
                                                    <%--<td><a href="#" class="link">View Member Listing</a></td>--%>
                                                </tr> 
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                                 
                                            </tbody>

                                          
                                        </table>
                                    </div>

                                </div>
             

    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/jquery-3.5.1.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/equal-height.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/index.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/popper.min.js") %>"></script>
    <script type="text/javascript" src="<%= ResolveUrl("~/Style/assets/vendor/bootstrap.min.js") %>"></script>

    <script type="text/javascript">  
    function openTab(evt, tab) {
         
        var i, tablinks;
        var x = document.getElementsByClassName("tab");
        for (i = 0; i < x.length; i++) {
            x[i].style.display = "none";
        }
        tablinks = document.getElementsByClassName("tablink");
        for (i = 0; i < x.length; i++) {
            tablinks[i].className = tablinks[i].className.replace(" active", "");
        }

        $("#" + tab).show();
        evt.currentTarget.className += " active";
        

        }

        function SetActivePolicy() {
            var activePolicy =document.getElementById('MainContent_hdnPolicyId').value; 
            
            var theDiv = document.getElementById('divPolicy_' + activePolicy);
 
            theDiv.className="statusCard mtop28 active card-insurance";
        }
        SetActivePolicy();
</script>
</asp:Content>
