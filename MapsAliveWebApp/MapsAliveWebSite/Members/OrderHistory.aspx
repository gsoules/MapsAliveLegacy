<%@
	Page Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="OrderHistory.aspx.cs"
	Inherits="Members_OrderHistory"
	Title="MapsAlive Account"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentBody" Runat="Server">
	<div class="textNormal">
		<asp:GridView
			ID="OrderTable"
			runat="server"
			CellPadding="4"
			ForeColor="#333333"
			GridLines="None"
			AutoGenerateColumns="False"
			OnSelectedIndexChanged="OnSelectRow"
			>
			<FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
			<RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
			<EditRowStyle BackColor="#999999" />
			<SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
			<PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
			<HeaderStyle BackColor="#808080" Font-Bold="True" ForeColor="White" />
			<AlternatingRowStyle BackColor="White" ForeColor="#284775" />
			<Columns> <%--Note that HtmlEncode must be set to false for DataFormatString to work.--%>
				<asp:BoundField DataField="OrderId" HeaderText="Order#" ReadOnly="True" SortExpression="OrderId" HtmlEncode="false" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" HeaderStyle-Width="50px" />
				<asp:CommandField ButtonType="Link" SelectText="Receipt" ShowSelectButton="True" HeaderStyle-Width="60px" ItemStyle-HorizontalAlign="Center" />
				<asp:BoundField DataField="PurchaseDate" HeaderText="Date" ReadOnly="True" SortExpression="PurchaseDate" HtmlEncode="false" DataFormatString="{0:MM/dd/yyyy}" HeaderStyle-Width="80px" />
				<asp:BoundField DataField="PlanDescription" HeaderText="Description" ReadOnly="True" SortExpression="PlanDescription" DataFormatString="{0:0,0}" HtmlEncode="false" HeaderStyle-Width="408px" />
				<asp:BoundField DataField="PurchasePrice" HeaderText="Payment" DataFormatString="{0:$###0.00}" HtmlEncode="false" HeaderStyle-Width="80px" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
			</Columns>
		</asp:GridView>
	</div>
</asp:Content>

