<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="ReportAccountStatistics.aspx.cs"
	Inherits="Admin_ReportAccountStatistics"
	Title="Customers"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ContentPlaceHolderID="ContentBody" Runat="Server">
	<asp:Label ID="Statistics" runat="server" />
</asp:Content>