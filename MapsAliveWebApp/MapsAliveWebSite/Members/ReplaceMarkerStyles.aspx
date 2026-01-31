<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="ReplaceMarkerStyles.aspx.cs"
	Inherits="Members_ReplaceMarkerStyles"
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

	<AvantLogic:QuickHelpTitle ID="MarkerStyleReplacementNew" runat="server" Title="New Marker Style" TopMargin="0px" />
	<table cellpadding="0" cellspacing="0">
		<tr>
			<td>
				<uc:TourResourceComboBox Id="MarkerStyleComboBox" runat="server" />
			</td>
			<td style="padding-left:16px;">
				<asp:Button ID="ButtonReplace" runat="server" OnClick="OnReplace" Text="Replace" />
			</td>
		</tr>
	</table>
	
	<div style="margin-top:16px;margin-bottom:8px;">
		<AvantLogic:QuickHelpTitle ID="MarkerStyleReplacementOld" runat="server" Title="Current Marker Styles" Span="true" />
		<AvantLogic:MemberPageActionButton ID="CheckAllControl" Title="Check All" runat="server" />
		<AvantLogic:MemberPageActionButton ID="UncheckAllControl" Title="Uncheck All" runat="server" />
	</div>
	
	<AvantLogic:CheckList ID="ReplaceList" runat="server" />

	<div id="NoListPanel" runat="server" class="finePrintHelp" />
	
	<asp:HiddenField ID="ReplacementList" runat="server"  Value="" />
</asp:Content>