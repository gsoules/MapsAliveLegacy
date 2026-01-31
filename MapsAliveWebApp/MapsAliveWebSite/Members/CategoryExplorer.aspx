<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="CategoryExplorer.aspx.cs"
	Inherits="Public_CategoryExplorer"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<div id="FilterWarningPanel" runat="server" Visible="false" class="noticeMessage" />
	<AvantLogic:TourResourceSelector ID="CategorySelector" runat="server" />
</asp:Content>

