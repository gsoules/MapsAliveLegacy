<%@ Page 
    Language="C#" 
    MasterPageFile="~/Masters/MemberPage.master" 
    AutoEventWireup="true" 
    CodeFile="Announcements.aspx.cs" 
    Inherits="Members_Announcements" 
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<asp:Panel ID="OptionsPanel" runat="server" CssClass="textLarge" style="margin-top:24px;margin-bottom:24px;">
		<asp:CheckBox ID="DontShowCheckBox" runat="server" AutoPostBack="true" OnCheckedChanged="OnDontShowChanged" Text="&nbsp;&nbsp;Don't show this screen when I login until there's a new announcement." />
	</asp:Panel>
	<div style="margin-bottom:24px">
		<div style="margin-top:18px;margin-bottom:0px;">
			<asp:Label ID="Announcement" runat="server" />
		</div>
	</div>
	<asp:Button Text="Close" runat="server" Enabled="true" OnClientClick="maSafeTransfer('/Members/TourManager.aspx');return false;" CausesValidation="false" Height="22px" />
    <span style="margin-top:8px;" class="textNormal">&nbsp;&nbsp;To see this announcement again, choose <b>Account > Announcements</b> from the menu.</span>
</asp:Content>

