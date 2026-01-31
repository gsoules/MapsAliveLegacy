<%@
	Page Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="LastReport.aspx.cs"
	Inherits="Members_LastReport"
	EnableViewState="false"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<asp:Panel ID="HideTracePanel" runat="server">
		<style type="text/css">
		.reportTrace
		{
			display:none;
		}
		</style>
	</asp:Panel>
	<div id="Report" runat="server" class="textNormal" style="margin-bottom:12px;"/>
	
	<asp:CheckBox ID="ShowTraceCheckBox" runat="server" class="textNormal" AutoPostBack="true" />
	<AvantLogic:QuickHelpTitle ID="ShowReportTrace" runat="server" Span="true" Title="Show Trace" />
</asp:Content>

