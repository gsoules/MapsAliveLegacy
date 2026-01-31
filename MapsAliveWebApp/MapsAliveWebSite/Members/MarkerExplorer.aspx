<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="MarkerExplorer.aspx.cs"
	Inherits="Public_MarkerExplorer"
	Title="MapsAlive Tours"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<asp:Panel ID="AccountMarkerSelectorPanel" runat="server"  style="margin-bottom:16px;">
		<div id="FilterWarningPanel" runat="server" Visible="false" class="noticeMessage" />
		<AvantLogic:TourResourceSelector ID="AccountMarkerSelector" runat="server" />
	</asp:Panel>
</asp:Content>

