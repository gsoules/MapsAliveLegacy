<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="Resources.aspx.cs"
	Inherits="Public_Resources"
	Title="MapsAlive Tours"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<asp:Label ID="ResourceLandingPageContent" runat="server" />
</asp:Content>

