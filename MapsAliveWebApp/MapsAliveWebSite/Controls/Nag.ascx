<%@ Control
	Language="C#"
	AutoEventWireup="true"
	CodeFile="Nag.ascx.cs"
	Inherits="Controls_Nag"
%>

<div class="nagBox">
    <table cellpadding="0" cellspacing="0">
		<tr>
			<td><asp:Image ID="NagImage" runat="server" BorderWidth="0" CssClass="nagImage" ImageUrl="~/Images/timer.svg" /></td>
			<td class="nagMessage"><asp:Label ID="NagMessage" runat="server" Visible="false"/>It's time to renew. Choose <b>Renew Plan</b> in the Account menu.</td>
		</tr>
    </table>
</div>


