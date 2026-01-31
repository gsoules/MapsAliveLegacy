<%@ Control
	Language="C#"
	AutoEventWireup="true"
	CodeFile="ImportReportTable.ascx.cs"
	Inherits="Controls_ImportReportTable"
	ClassName="ImportReportTable"
%>
<style type="text/css">
.reportTrace
{
	display:none;
}
</style>
<asp:Panel ID="ReportPanel" runat="server" CssClass="textNormal" Visible="false">
	<div style="margin-top:16px;margin-bottom:8px;" class="optionsSectionTitle">
		<asp:Label ID="ReportTitle" runat="server"/>
	</div>
	<asp:Label ID="ReportBody" runat="server" />
</asp:Panel>

