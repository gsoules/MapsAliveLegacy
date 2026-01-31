<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="DeleteHotspots.aspx.cs"
	Inherits="Members_DeleteHotspots"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<script type="text/javascript">
	function maOnDelete()
	{
		var deletions = document.getElementsByTagName("input");
		var list = maOnCheckListGetCheckedItems(deletions);
		if (list.length === 0)
		{
			maAlert("You have not selected any hotspots to delete.");
			return false;
		}
		var howMany = list.split(",").length;
        var what = howMany + " hotspot" + (howMany > 1 ? "s" : "");
        let msg = "Are you sure you want to delete " + what + "?[*WARNING: This action cannot be undone.*]";
        let script = `document.getElementById("<%= DeletionList.ClientID %>").value = `;
        script += `"${list}";`;
        script += "maDoPostBack('EventOnDelete', '');";
        maConfirmAndExecuteScript(msg, script, "DELETE");
        return false;
	}
	function maCheckAll(all)
	{
		var deletions = document.getElementsByTagName("input");
		maOnCheckListCheckAll(all, deletions);
	}
    </script>

	<table>
		<tr>
			<td><asp:Button ID="ButtonDelete" runat="server" Text="Delete" /></td>
			<td style="padding-left:8px;font-weight:bold;color:#cc9936;">WARNING: Deletions are permanent</td>
		</tr>
	</table>
	
	<div style="margin-top:16px;margin-bottom:8px;">
		<AvantLogic:MemberPageActionButton ID="CheckAllControl" Title="Check All" runat="server" />
		<AvantLogic:MemberPageActionButton ID="UncheckAllControl" Title="Uncheck All" runat="server" />
	</div>
	
	<AvantLogic:CheckList ID="DeleteList" runat="server" />

	<div id="NoListPanel" runat="server" class="finePrintHelp" />
	
	<asp:HiddenField ID="DeletionList" runat="server"  Value="" />
</asp:Content>