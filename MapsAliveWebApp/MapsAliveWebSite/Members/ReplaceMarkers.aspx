<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="ReplaceMarkers.aspx.cs"
	Inherits="Members_ReplaceMarkers"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<script type="text/javascript">
	function maOnReplace(target)
	{
		var replacements = document.getElementsByTagName("input");
		var list = maOnCheckListGetCheckedItems(replacements);
		document.getElementById("<%= ReplacementList.ClientID %>").value = list;
	}
	function maCheckAll(all)
	{
		var replacements = document.getElementsByTagName("input");
		maOnCheckListCheckAll(all, replacements);
	}
	</script>

	<AvantLogic:QuickHelpTitle ID="MarkerReplacementNew" runat="server" Title="New Marker" TopMargin="0px" />
	<table cellpadding="0" cellspacing="0">
		<tr>
			<td>
				<uc:TourResourceComboBox Id="MarkerComboBox" runat="server" />
			</td>
			<td style="padding-left:16px;">
				<asp:Button ID="ButtonReplace" runat="server" OnClick="OnReplace" Text="Replace" />
			</td>
		</tr>
	</table>
	
	<div style="margin-top:16px;margin-bottom:8px;">
		<AvantLogic:QuickHelpTitle ID="MarkerReplacementOld" runat="server" Title="Current Markers" Span="true" />
		<AvantLogic:MemberPageActionButton ID="CheckAllControl" Title="Check All" runat="server" />
		<AvantLogic:MemberPageActionButton ID="UncheckAllControl" Title="Uncheck All" runat="server" />
	</div>
	
	<AvantLogic:CheckList ID="ReplaceList" runat="server" />

	<div id="NoListPanel" runat="server" class="finePrintHelp" />
	
	<asp:HiddenField ID="ReplacementList" runat="server"  Value="" />
</asp:Content>