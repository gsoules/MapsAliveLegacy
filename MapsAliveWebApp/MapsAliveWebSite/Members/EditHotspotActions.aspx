<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="EditHotspotActions.aspx.cs"
	Inherits="Members_EditHotspotActions"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">

	<script language="javascript">                
    function maOnClickActionSelected(e)
    {
		maChangeDetected();
		maShowClickActionTarget(e.value);
    }
	function maShowClickActionTarget(action)
	{
		var showPageList = false;
		var showUrl = false;
		var showJS = false;
		if (action == <%= (int)MarkerAction.GotoPage %>)
			showPageList = true;
		else if (action == <%= (int)MarkerAction.LinkToUrl %> || action == <%= (int)MarkerAction.LinkToUrlNewWindow %>)
			showUrl = true;
		else if (action == <%= (int)MarkerAction.CallJavascript %>)
			showJS = true;
		document.getElementById(formContentId + "PageSelectorPanel").style.display = showPageList ? "block" : "none";	
		document.getElementById(formContentId + "UrlPanel").style.display = showUrl ? "block" : "none";	
		document.getElementById(formContentId + "JSPanel1").style.display = showJS ? "block" : "none";	
	}
    function maOnRolloverActionSelected(e)
    {
		maChangeDetected();
		maShowRolloverActionTarget(e.value);
    }
 	function maShowRolloverActionTarget(action)
 	{
		var showJS = action == <%= (int)MarkerAction.CallJavascript %>;
		document.getElementById(formContentId + "JSPanel2").style.display = showJS ? "block" : "none";	
 	}
	function maOnRolloutActionSelected(e)
    {
		maChangeDetected();
		maShowRolloutActionTarget(e.value);
    }
 	function maShowRolloutActionTarget(action)
 	{
		var showJS = action == <%= (int)MarkerAction.CallJavascript %>;
		document.getElementById(formContentId + "JSPanel3").style.display = showJS ? "block" : "none";	
 	}
	function maOnPreviewRollover(over)
	{
		var img = document.getElementById(formContentId + "CurrentImage");
		img.src = "MarkerRenderer.ashx?state=" + (over ? "1" : "0") + "&id=" + selectedMarkerId + "&actual=0";
	}
	function maAllowEnter(e)
	{
		if (window.event)
			window.event.cancelBubble = true;
		else
			e.stopPropagation();
		return true;
	}
	</script> 
	
	<AvantLogic:QuickHelpTitle ID="HotspotActionsHotspotId" runat="server" Title="Hotspot Id" />
	<asp:Label ID="HotspotId" runat="server" CssClass="textNormal" />

	<asp:Panel ID="MarkerPreviewPanel" runat="server" style="margin-top:12px;">
		<AvantLogic:QuickHelpTitle ID="EditMarkerSlide" Title="Marker Preview" runat="server" />
		<img id="CurrentImage" runat="server" onmouseover="maOnPreviewRollover(true)" onmouseout="maOnPreviewRollover(false)" />
	</asp:Panel>
		
	<div class="optionsSectionTitle">Show Hotspot Options</div>
	
	<div style="margin-bottom:16px;font-size:10px;color:#666">
        <AvantLogic:QuickHelpTitle ID="SlideSelectOptions" runat="server" Title="<%$ Resources:Text, SelectSlideOptionsLabel %>" TopMargin="0px" />
		<div style="margin-top:4px;">
			<asp:RadioButton ID="RadioButtonOnMouseover" runat="server" GroupName="SlideSelect" Text="<%$ Resources:Text, SelectSlideOnRolloverLabel %>" />
		</div>
		<div style="margin-top: 4px;">
			<asp:RadioButton ID="RadioButtonOnClick" runat="server" GroupName="SlideSelect" Text="<%$ Resources:Text, SelectSlideOnClickLabel %>" />
		</div>
		<div style="margin-top: 4px;">
			<asp:RadioButton ID="RadioButtonNever" runat="server" GroupName="SlideSelect" Text="<%$ Resources:Text, SelectSlideNeverLabel %>" />
		</div>
	</div>

	<div class="optionsSectionTitle">Click Options</div>

	<div style="margin-right:16px;">
		<AvantLogic:QuickHelpTitle ID="TourViewClickAction" runat="server" Title="<%$ Resources:Text, ClickActionLabel %>" OffsetY="-150" />
		<asp:DropDownList runat="server" ID="ClickActionDropDownList" onChange="maOnClickActionSelected(this)" />
	</div>
	
	<asp:Panel ID="PageSelectorPanel" runat="server" style="display:none;">
		<AvantLogic:QuickHelpTitle ID="TourViewGotoPageAction" runat="server" Title="<%$ Resources:Text, MarkerGotoPageLabel %>" />
		<asp:DropDownList ID="PageDropDownList" runat="server" />
	</asp:Panel>
	
	<asp:Panel ID="UrlPanel" runat="server" style="display:none;">
		<div style="margin-right:16px;">
			<AvantLogic:QuickHelpTitle ID="TourViewLinkToUrlAction" runat="server" Title="<%$ Resources:Text, MarkerLinkToUrlLabel %>" />
			<asp:TextBox ID="UrlTextBox" runat="server" Width="570"></asp:TextBox>
		</div>
		<div class="checkboxOptionFirst">
			<asp:CheckBox ID="PopupCheckBox" runat="server" />
			<AvantLogic:QuickHelpTitle ID="TourViewLinkToUrlPopup" runat="server" Title="<%$ Resources:Text, MarkerLinkToUrlPopupLabel %>" Span="true"  />
		</div>
	</asp:Panel>
	
	<asp:Panel ID="JSPanel1" runat="server" style="display:none;">
		<AvantLogic:QuickHelpTitle ID="TourViewJavascriptAction" runat="server" Title="<%$ Resources:Text, MarkerJavascriptLabel %>" />
		<asp:TextBox ID="JS1TextBox" runat="server" TextMode="MultiLine" Rows="5" Width="700" onkeypress="return maAllowEnter(event);" />
	</asp:Panel>
	
	<div class="optionsSectionTitle">Mouseover Options</div>
	
	<AvantLogic:QuickHelpTitle ID="MouseoverAction" runat="server" Title="<%$ Resources:Text, RolloverActionLabel %>" />
	<asp:DropDownList runat="server" ID="RolloverActionDropDownList" onChange="maOnRolloverActionSelected(this)">
		<asp:ListItem Text="None" Value="0" Selected="True"></asp:ListItem>
		<asp:ListItem Text="JavaScript" Value="3"></asp:ListItem>
	</asp:DropDownList>
	
	<asp:Panel ID="JSPanel2" runat="server" style="display:none;">
		<AvantLogic:QuickHelpTitle ID="TourViewJavascriptRolloverAction" runat="server" Title="<%$ Resources:Text, MarkerJavascriptLabel %>" />
		<asp:TextBox ID="JS2TextBox" runat="server" TextMode="MultiLine" Rows="5" Width="700" onkeypress="return maAllowEnter(event);" />
	</asp:Panel>
		
	<div class="optionsSectionTitle">Mouseout Options</div>

	<AvantLogic:QuickHelpTitle ID="MouseoutAction" runat="server" Title="<%$ Resources:Text, RolloutActionLabel %>" OffsetY="-240" />
	<asp:DropDownList runat="server" ID="RolloutActionDropDownList" onChange="maOnRolloutActionSelected(this)">
		<asp:ListItem Text="None" Value="0" Selected="True"></asp:ListItem>
		<asp:ListItem Text="JavaScript" Value="3"></asp:ListItem>
	</asp:DropDownList>
	
	<asp:Panel ID="JSPanel3" runat="server" style="display:none;">
		<AvantLogic:QuickHelpTitle ID="TourViewJavascriptRolloutAction" runat="server" Title="<%$ Resources:Text, MarkerJavascriptLabel %>" />
		<asp:TextBox ID="JS3TextBox" runat="server" TextMode="MultiLine" Rows="5" Width="700" onkeypress="return maAllowEnter(event);" />
	</asp:Panel>
		
	<asp:Panel ID="TouchOptionsPanel" runat="server" visible="false">
		<div class="optionsSectionTitle">Touch Options</div>
		<div style="margin-bottom:16px;font-size:10px;color:#666">
			<AvantLogic:QuickHelpTitle ID="TouchAction" runat="server" Title="When a hotspot is touched:" TopMargin="0px" OffsetY="-180" />
			<div style="margin-top:4px;">
				<asp:RadioButton ID="RadioButtonTouchMouseOver" runat="server" GroupName="TouchOption" Text="Execute the Mouseover Action" />
			</div>
			<div style="margin-top: 4px;">
				<asp:RadioButton ID="RadioButtonTouchClick" runat="server" GroupName="TouchOption" Text="Execute the Click Action" />
			</div>
		</div>
	</asp:Panel>
</asp:Content>

