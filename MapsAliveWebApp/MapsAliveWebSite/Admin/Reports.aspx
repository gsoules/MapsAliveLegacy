<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="Reports.aspx.cs"
	Inherits="Admin_Reports"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="App_Code\ServerControls\AdminReport.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<div class="textNormal">
		Period: <asp:DropDownList ID="PeriodDropDownList" runat="server" AutoPostBack="true" />&nbsp;&nbsp;
		Report: <asp:DropDownList ID="ReportDropDownList" runat="server" AutoPostBack="true" />&nbsp;&nbsp;
		<asp:TextBox ID="SearchTextBox" runat="server" Width="150"></asp:TextBox>
		<asp:Button runat="server" Text="Run" />
		<br /><br />
		<AvantLogic:AdminReport ID="AdminReport" runat="server" />
	</div>
</asp:Content>


