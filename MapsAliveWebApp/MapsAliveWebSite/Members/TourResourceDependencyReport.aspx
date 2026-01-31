<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="TourResourceDependencyReport.aspx.cs"
	Inherits="Public_TourResourceDependencyReport"
	Title="MapsAlive Tours"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<div class="textNormal" style="margin-bottom:16px;">
		<asp:Label ID="ReportTitle" runat="server" ></asp:Label>
	</div>
	<asp:Label ID="ReportData" runat="server" ></asp:Label>
</asp:Content>

