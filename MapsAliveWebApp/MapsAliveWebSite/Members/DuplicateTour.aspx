<%@
	Page Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="DuplicateTour.aspx.cs"
	Inherits="Members_DuplicateTour"
	Title=""
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" runat="Server">
	<style type="text/css">
	.riTextBox
	{
		padding-left:3px !important;
		font-weight:bold !important;
	}
	</style>
	<script type="text/javascript">
	function okToDuplicateTour()
    {
        maConfirmAndPostBack("Press DUPLICATE to make a copy of the current tour.", "EventOnDuplicate", "DUPLICATE");
		return false;
	}
    </script>
	
	<div class="textNormal" style="margin-bottom:8px;">
		<asp:Button ID="DuplicateButton" runat="server" style="margin-top:8px;" OnClientClick="return okToDuplicateTour();" Text="Duplicate" />
		
		<div class="checkboxOption" style="margin-top:20px;">
			<asp:CheckBox ID="ExcludeHotspotsCheckbox" runat="server" />
			<AvantLogic:QuickHelpTitle ID="ExcludeHotspotsWhenDuplicateTour" runat="server" Title="Don't Duplicate Hotspots" Span="true" />
		</div>
	</div>
				
	<telerik:RadProgressManager ID="RadProgressManager" runat="server" />
	<telerik:RadProgressArea ID="ProgressArea" runat="server" />
	
	<uc:ImportReportTable Id="ImportReportTable" runat="server" />
</asp:Content>

