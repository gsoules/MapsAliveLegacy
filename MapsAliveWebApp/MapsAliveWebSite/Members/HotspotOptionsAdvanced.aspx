<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="HotspotOptionsAdvanced.aspx.cs"
	Inherits="Members_HotspotOptionsAdvanced"
	ValidateRequest="false"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<script language="javascript">   
	function maOnUseLiveDataClicked()
	{
		maChangeDetected();
		var show = document.getElementById("<%= LiveDataCheckBox.ClientID %>").checked;
		document.getElementById("<%= MessengerFunctionPanel.ClientID %>").style.display = show ? "block" : "none";
	}
	</script>
	 
    <asp:Label ID="HotspotTitle" runat="server" CssClass="textLarge" style="font-weight:bold;"></asp:Label>

	<AvantLogic:QuickHelpTitle ID="SlideContentCodeSnippets" runat="server" Title="URL" />
	<asp:Label ID="CodeSnippets" runat="server" CssClass="textNormal" />
	
	<div style="margin-top:12px;">
		<asp:CheckBox ID="ExcludeCheckBox" runat="server" />
		<AvantLogic:QuickHelpTitle ID="ExcludeFromDirectory" runat="server" Span="true" Title="Exclude From Directory" />
	</div>

	<asp:Panel ID="DimensionsPanel" runat="server" style="margin-top:16px;">
		<AvantLogic:QuickHelpTitle ID="PopupSizeOverride" runat="server" Title="Popup Size Override" TopMargin="4px" />
		<table>
			<tr>
				<td>
					<asp:Label runat="server" CssClass="controlLabel" Text="Width" />
				</td>
				<td>
					<asp:TextBox ID="OverrideWidthTextBox" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
					<asp:Label ID="OverrideWidthError" runat="server" CssClass="textErrorMessage" />
				</td>
				<td style="padding-left:8px;">
					<asp:Label runat="server" CssClass="controlLabel" Text="Height" />
				</td>
				<td>
					<asp:TextBox ID="OverrideHeightTextBox" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
					<asp:Label ID="OverrideHeightError" runat="server" CssClass="textErrorMessage" />
				</td>
			</tr>
		</table>
	</asp:Panel>
					
	<asp:Panel ID="MarkerAttirbutesPanel" runat="server">
		<div class="optionsSectionTitle">Marker Attributes</div>
		<div>
			<asp:CheckBox ID="MarkerNotAnchoredCheckBox" runat="server" />
			<AvantLogic:QuickHelpTitle ID="MarkerNotAnchored" runat="server" Span="true" Title="Is Not Anchored To Map" />
		</div>
		
		<div>
			<asp:CheckBox ID="MarkerLockedCheckBox" runat="server" />
			<AvantLogic:QuickHelpTitle ID="MarkerLocked" runat="server" Span="true" Title="Is Locked" />
		</div>
		
		<div>
			<asp:CheckBox ID="MarkerHiddenCheckBox" runat="server" />
			<AvantLogic:QuickHelpTitle ID="MarkerHidden" runat="server" Span="true" Title="Is Hidden" />
		</div>
		
		<div>
			<asp:CheckBox ID="MarkerDisabledCheckBox" runat="server" />
			<AvantLogic:QuickHelpTitle ID="MarkerDisabled" runat="server" Span="true" Title="Is Disabled" />
		</div>
		
		<div>
			<asp:CheckBox ID="MarkerStaticCheckBox" runat="server" />
			<AvantLogic:QuickHelpTitle ID="MarkerStatic" runat="server" Span="true" Title="Is Static" />
		</div>
		
		<div>
			<asp:CheckBox ID="MarkerRouteCheckBox" runat="server" />
			<AvantLogic:QuickHelpTitle ID="MarkerRoute" runat="server" Span="true" Title="Is Route" />
		</div>

		<div class="optionsSectionTitle">Marker Zooming</div>
		<div>
			<AvantLogic:QuickHelpTitle ID="MarkerZoomTypeOverride" runat="server" Title="Marker Zooms" TopMargin="0" />
			<asp:DropDownList ID="MarkerZoomTypeDropDownList" runat="server" />
		</div>
		
		<div>
			<AvantLogic:QuickHelpTitle ID="MarkerZoomThreshold" runat="server" Title="Zoom Visibility Threshold" />
			<asp:TextBox ID="MarkerZoomThresholdTextBox" runat="server" Width="30px" /><span class="unit">%</span>
			<asp:Label ID="MarkerZoomThresholdError" runat="server" CssClass="textErrorMessage" />
		</div>
	</asp:Panel>
	
	<asp:Panel ID="LiveDataSectionPanel" runat="server" class="optionsSectionTitle" style="margin-top:16px;">Live Data Options</asp:Panel>
	<asp:Panel ID="LiveDataPanel" runat="server" style="margin-bottom:12px;">
		<asp:CheckBox ID="LiveDataCheckBox" runat="server" />
		<AvantLogic:QuickHelpTitle ID="LiveDataUse" runat="server" Span="true" Title="Uses Live Data" />
		<asp:Panel ID="MessengerFunctionPanel" runat="server" style="display:none;">
			<AvantLogic:QuickHelpTitle ID="LiveDataMessengerFunction" runat="server" Title="Request Function" />
			<asp:TextBox ID="LiveDataTextBox" Width="700" runat="server" style="margin-top:4px;font-family:Courier New;font-size:13px;" />
		</asp:Panel>	
	</asp:Panel>
	
	<asp:Panel ID="CategoryPanel" runat="server">
		<div class="optionsSectionTitle">Categories <AvantLogic:QuickHelpTitle ID="Categories" runat="server" Span="true" OffsetX="20" OffsetY="-230" /></div>
		<table class="optionsTable" cellpadding="0" cellspacing="0">
			<tr>
				<td colspan="2">
					<div id="CategoryFilterWarningPanel" runat="server" Visible="false" class="noticeMessage" style="margin-top:2px;" />
				</td>
			</tr>
			<tr>
				<td>
					<asp:Panel ID="CategoriesPanel" runat="server" class="textNormal" xstyle="background-color:#e7f0f6;">
						<asp:CheckBoxList ID="CheckBoxList" runat="server" RepeatColumns="1" CellSpacing="0" CellPadding="0" />
					</asp:Panel>
					<asp:Panel ID="NoCategoriesPanel" runat="server" class="finePrintHelp" >
						<asp:Label ID="NoCategoriesMessage" runat="server" />
					</asp:Panel>
				</td>
			</tr>
		</table>
	</asp:Panel>
				
</asp:Content>

