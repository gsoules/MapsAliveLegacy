<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="Users.aspx.cs"
	Inherits="Admin_Users"
	Title="Customers"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentBody" Runat="Server">
	<script type="text/javascript">
		var allowEnterKeyToPost = true;
		function SetSelected(id)
		{
			document.getElementById("<%= Filter.ClientID %>").select();
		}
	</script>
	
	
	<asp:Panel ID="StopImpersonationPanel" runat="server" style="margin-top:0px;margin-bottom:8px;">
		<asp:LinkButton ID="StopImpersonationButton" runat="server" OnClick="OnStopImpersonation" />
	</asp:Panel>
	
	<table style="margin-bottom:4px;" class="textNormal">
		<tr>
			<td align="right">Search:</td>
			<td>
				<asp:TextBox ID="Filter" runat="server" />
				<asp:Button ID="FindButton" runat="server" Text="Find" />
				&nbsp;<asp:Label ID="Results" runat="server"/>
			</td>
		</tr>
	</table>
	
	<div style="margin-bottom:8px;font-size:11px;color:#777;">
		To see all users, enter *. To search by tour, enter # followed by the tour number.
	</div>
	
	<asp:GridView
		ID="GridView"
		runat="server"
		CellPadding="6"
		ForeColor="#333333"
		GridLines="None"
		CssClass="textNormal"
		OnSelectedIndexChanged="OnSelectRow"
		AllowSorting="False"
		AutoGenerateColumns="False"
		>
		<Columns>
			<asp:CommandField ShowSelectButton="True" />
			<asp:BoundField DataField="AccountId" HeaderText="Account #" ReadOnly="True" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
			<asp:BoundField DataField="UserName" HeaderText="Email" ReadOnly="True" />
			<asp:BoundField DataField="ContactName" HeaderText="Contact Name" ReadOnly="True" />
			<asp:BoundField DataField="SlideLimit" HeaderText="Hotspots" ReadOnly="True" />
			<asp:BoundField DataField="Days" HeaderText="Days" ReadOnly="True" />
		</Columns>
		<FooterStyle BackColor="#777777" Font-Bold="True" ForeColor="White" />
		<RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
		<EditRowStyle BackColor="#999999" />
		<SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
		<PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
		<HeaderStyle BackColor="#777777" Font-Bold="True" ForeColor="White" />
		<AlternatingRowStyle BackColor="White" ForeColor="#284775" />
	</asp:GridView>
</asp:Content>


