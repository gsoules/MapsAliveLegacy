<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="StyleExplorer.aspx.cs"
	Inherits="Public_StyleExplorer"
	Title="MapsAlive Tours"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<AvantLogic:TourResourceSelector ID="StyleSelector" runat="server" />
</asp:Content>

