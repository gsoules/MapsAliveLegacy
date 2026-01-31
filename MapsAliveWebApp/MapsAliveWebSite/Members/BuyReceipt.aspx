<%@ Page 
    Language="C#" 
    MasterPageFile="~/Masters/MemberPage.master" 
    AutoEventWireup="true" 
    CodeFile="BuyReceipt.aspx.cs" 
    Inherits="Members_BuyReceipt" 
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentBody" Runat="Server">
	<div class="textNormal">
		<asp:Panel ID="CreateFirstTourPanel" runat="server" style="margin-bottom:16px;" Visible="false">
			<span style="color:#105aa5;font-weight:bold;font-size:16px;">To create your first tour, choose <a href="https://www.mapsalive.com/Members/Welcome.aspx">Account > Welcome</a> from the menu.</span>
		</asp:Panel>
		
		<asp:Panel ID="SpecialOfferPanel" runat="server" Visible="false" style="margin-bottom:16px;padding:8px;border:solid 1px gray;">
			<asp:Label ID="SpecialOfferMessage" runat="server" />
		</asp:Panel>
		
        <table cellpadding="0" cellspacing="0" style="border:solid 1px #cccccc;width:500px;">
            <tr>
                <td>
                    <div class="formTitleBar">
						<asp:Label ID="LeftBoxLabel" runat="server" Text="<%$ Resources:Text, BuyReceiptOrderInformation %>" />
                    </div>
                </td>
            </tr>
            <tr>
				<td>
					<table cellspacing="4px" style="margin-left:8px;">
						<tr>
							<td width="110">Account Number:</td>
							<td><asp:Label ID="AccountNumber" runat="server" /></td>
						</tr>	
						<tr>
							<td width="110">Order Number:</td>
							<td><asp:Label ID="OrderNumber" runat="server" /></td>
						</tr>	
						<tr>
							<td>Order Date:</td>
							<td><asp:Label ID="OrderDate" runat="server"></asp:Label></td>
						</tr>
						<tr>
							<td>Purchase:</td>
							<td><asp:Label ID="Purchase" runat="server" /></td>
						</tr>
						<tr>
							<td width="110">Price:</td>
							<td><asp:Label ID="Price" runat="server" /></td>
						</tr>
					</table>
				</td>
            </tr>
        </table>
		<asp:Panel ID="PaymentPanel" runat="server" style="margin-top:16px;">
			<table cellpadding="0" cellspacing="0" style="border:solid 1px #cccccc;width:500px;">
				<tr>
					<td>
						<div class="formTitleBar">
							<asp:Label ID="Label1" runat="server" Text="<%$ Resources:Text, BuyReceiptPaymentInformation %>" />
						</div>
					</td>
				</tr>
				<tr>
					<td>
						<table cellspacing="4px" style="margin-left:8px;">
							<tr>
								<td width="110">Payment Method:</td>
								<td><asp:Label ID="PaymentMethod" runat="server" /></td>
							</tr>
							<tr>
							</tr>
							<tr>
								<td valign="top">Billed to:</td>
								<td>
									<div><asp:Label ID="Who" runat="server" /></div>
									<div><asp:Label ID="Company" runat="server" /></div>
									<div><asp:Label ID="Address1" runat="server" /></div>
									<div><asp:Label ID="Address2" runat="server" /></div>
									<div><asp:Label ID="CityStateZip" runat="server" /></div>
									<div><asp:Label ID="Country" runat="server" /></div>
								</td>
							</tr>
						</table>
					</td>
				</tr>
			</table>
		</asp:Panel>
 		
 		<asp:Panel ID="CreditBalancePanel" runat="server" Visible="false" style="margin-top:16px;">
			<table cellpadding="0" cellspacing="0" style="border:solid 1px #cccccc;width:500px;">
				<tr>
					<td>
						<div class="formTitleBar">
							<asp:Label ID="Label2" runat="server" Text="<%$ Resources:Text, BuyReceiptPaymentInformation %>" />
						</div>
					</td>
				</tr>
				<tr>
					<td style="padding:8px;">
						<asp:Label ID="CreditBalanceMessage" runat="server"/>
					</td>
				</tr>
			</table>
		</asp:Panel>
       
        <div style="width:500px;">
			<asp:Panel ID="PaymentMessagePanel" runat="server" style="margin-top:20px;">
				<asp:Label runat="server" Text="<%$ Resources:Text, BuyReceiptTotal1 %>" />
				<b><asp:Label ID="Charge" runat="server" /></b>
				<asp:Label runat="server" Text="<%$ Resources:Text, BuyReceiptTotal2 %>" />
				<br />
				<asp:Label runat="server" Text="<%$ Resources:Text, BuyReceiptTotal3 %>" />
			</asp:Panel>
	        	
			<div style="margin-top:12px;">
				<asp:Label ID="Label3" runat="server" Text="<%$ Resources:Text, BuyReceiptReceiptMessage %>" />
				<asp:Label ID="Email" runat="server" />.
			</div>

			<div style="margin-top:12px;">
				<asp:Label runat="server" Text="<%$ Resources:Text, BuyReceiptQuestions %>" />
				<asp:HyperLink ID="EmailLink" runat="server">support@mapsalive.com</asp:HyperLink>.
			</div>
			
			<div class="miniHeadline" style="margin-top:12px;text-align:center;">
				<asp:Label runat="server" Text="<%$ Resources:Text, BuyReceiptThankYou %>" />
			</div>
        </div>
        
    </div>
</asp:Content>

