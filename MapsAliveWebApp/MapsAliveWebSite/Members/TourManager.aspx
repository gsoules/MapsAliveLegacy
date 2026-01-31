<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="TourManager.aspx.cs"
	Inherits="Members_TourManager"
	EnableViewState="true"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>
<%@ Register Assembly="App_Code\ServerControls\PageThumbs.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
    <script type="text/javascript" language="javascript">                
    function maOnShowFirstChanged()
    {
        // Hide the Tour Preview button because the page has to reload to apply the change. If the user
        // clicks the button before the page loads again, the previewed tour won't reflect the change.
        document.getElementById('MemberPageControlsHeaderRight').style.visibility = 'hidden';
    }
    function maConvertToV4(element)
    {
        if (!element.checked)
        {
            let message = "[*WARNING*][Unchecking the Enable V3 Compatibility option will convert a V3 tour to V4 ";
            message += "and it might not be possible to convert back to V3. The V4 tour may look and/or behave differently.]";
            message += "[Before continuing, <a href='https://www.mapsalive.com/docs/about-v3/#converting-from-v3-to-v4' target='_blank'>Learn about converting from V3 to V4</a>.]"
            maAlert(message);
        }
        maChangeDetected();
    }
    </script> 

	<div class="tourStatistics">
		<table cellpadding="0" cellspacing="4">
			<tr>
				<td class="tourStatisticLabel">Name:</td>
				<td class="tourStatisticValue"><asp:Label ID="TourName" runat="server"/></td>
				<td class="tourStatisticLabel">Compiled:</td>
				<td class="tourStatisticValue"><asp:Label ID="Compiled" runat="server"/></td>
			</tr>
			<tr>
				<td class="tourStatisticLabel">Tour #:</td>
				<td class="tourStatisticValue"><asp:Label ID="TourId" runat="server"/></td>
				<td class="tourStatisticLabel">Downloaded:</td>
				<td class="tourStatisticValue"><asp:Label ID="DateDownloaded" runat="server"/></td>
			</tr>
			<tr>
				<td class="tourStatisticLabel">Created:</td>
				<td class="tourStatisticValue"><asp:Label ID="DateCreated" runat="server"/></td>
				<td class="tourStatisticLabel">Archived:</td>
				<td class="tourStatisticValue"><asp:Label ID="DateArchived" runat="server"/></td>
			</tr>
			<tr>
				<td class="tourStatisticLabel">Published:</td>
				<td class="tourStatisticValue"><asp:Label ID="DatePublished" runat="server"/></td>
				<td class="tourStatisticLabel">URL:</td>
				<td class="tourStatisticValue"><asp:HyperLink ID="HyperLinkTour" runat="server" /></td>
			</tr>
		</table>
	</div>
		
	<div>
		<asp:Panel ID="PagesPanel" runat="server">
			<div class="optionsSectionTitle">Pages in this Tour <AvantLogic:QuickHelpTitle ID="TourQuickView" runat="server" Span="true" /></div>
		
			<%-- Page thumbnails --%>
			<AvantLogic:PageThumbs ID="PageThumbs" runat="server" ColumnsPerRow="5" Dimension="124" />

			<asp:Panel ID="ConfigurationPanel" runat="server" style="margin-top:-12px;margin-bottom:20px;">
				<%-- Configuration --%>
				<div class="optionsSectionTitle" style="margin-top:0px;">Sequence Options</div>
				
				<table cellpadding="0" cellspacing="0">
					<tr>
						<td>
							<AvantLogic:QuickHelpTitle ID="MenuOrder" runat="server" Title="Menu and Directory Order" TopMargin="0px" />
							<telerik:RadListBox
								runat="server"
								ID="MenuOrderListBox"
								Width="300px"
								OnClientReordered="maChangeDetected"
								>
							</telerik:RadListBox>
						</td>
						<td valign="top" style="padding-left:24px;">
							<AvantLogic:QuickHelpTitle ID="FirstPage" runat="server" Title="Show First" TopMargin="0px" />
							<telerik:RadComboBox 
								id="FirstPageComboBox" 
								Runat="server"
								AutoPostBack="true"
								OnSelectedIndexChanged="OnChangeFirstPage"
                                OnClientSelectedIndexChanged="maOnShowFirstChanged"
								Skin="Default"
								> 
								<ExpandAnimation Type="None" />
								<CollapseAnimation Type="None" />
							</telerik:RadComboBox>
						</td>
					</tr>
				</table>
			</asp:Panel>
		</asp:Panel>
		
		<div class="optionsSectionTitle">Browser Options</div>
		<%-- Browser Title --%>
		<div style="margin-top:16px;">
			<AvantLogic:QuickHelpTitle ID="BrowserTitle" runat="server" Title="<%$ Resources:Text, BrowserTitleLabel %>" TopMargin="0px" />
			<asp:TextBox ID="BrowserTitleText" runat="server" Width="550px"></asp:TextBox>
		</div>

		<div class="optionsSectionTitle">Display and Usability Options</div>
		
		<%-- Show Zoom Control on Mobile --%>
		<div class="checkboxOption">
			<asp:CheckBox ID="ShowZoomControlCheckBox" runat="server" Enabled="false" />
			<AvantLogic:QuickHelpTitle ID="ShowZoomControl" runat="server" Title="Show Zoom Control on Touch Devices" Span="true" OffsetY="-100" />
		</div>

		<%-- Enlarge hit test area for symbol markers --%>
		<div class="checkboxOption">
			<asp:CheckBox ID="EnlargeHitTestAreaCheckBox" runat="server" Enabled="false" />
			<AvantLogic:QuickHelpTitle ID="EnlargeHitTestArea" runat="server" Title="Make Small Markers Easier To Touch" Span="true" OffsetY="-100" />
		</div>
		
		<%-- Touch Screen --%>
		<div class="checkboxOption">
			<asp:CheckBox ID="UseTouchUiOnDesktopCheckBox" runat="server" Enabled="false" />
			<AvantLogic:QuickHelpTitle ID="UseTouchScreenUi" runat="server" Title="Use Touch User Interface for Desktop Browser" Span="true" OffsetY="-150" />
		</div>
		
		<%-- Enable Image Preloading --%>
		<div class="checkboxOption">
			<asp:CheckBox ID="EnableImagePreloadingCheckBox" runat="server" Enabled="false" />
			<AvantLogic:QuickHelpTitle ID="EnableImagePreloading" runat="server" Title="Enable Image Preloading on Mobile Browsers" Span="true" OffsetY="-225" />
		</div>
		
		<%-- Select Hotspot On Touch Option --%>
		<div class="checkboxOption">
			<asp:CheckBox ID="SelectsOnTouchStartCheckBox" runat="server" Enabled="false" />
			<AvantLogic:QuickHelpTitle ID="SelectsOnTouchStart" runat="server" Title="Select Hotspot On Touch Start" Span="true" OffsetY="-100" />
		</div>
		
		<%-- Disable Smooth Panning --%>
		<div class="checkboxOption">
			<asp:CheckBox ID="DisableSmoothPanningCheckBox" runat="server" Enabled="false" />
			<AvantLogic:QuickHelpTitle ID="DisableSmoothPanning" runat="server" Title="Disable Smooth Panning" Span="true" OffsetY="-100" />
		</div>
		
		<%-- Disable Blend Effect --%>
		<div class="checkboxOption">
			<asp:CheckBox ID="DisableBlendEffectCheckBox" runat="server" Enabled="false" />
			<AvantLogic:QuickHelpTitle ID="DisableBlendEffect" runat="server" Title="Disable Blend Effect on Mobile Browsers" Span="true" OffsetY="-225" />
		</div>
		
		<%-- Make Entire Popup Visible --%>
		<div class="checkboxOption">
			<asp:CheckBox ID="EntirePopupVisibleCheckBox" runat="server" Enabled="false" />
			<AvantLogic:QuickHelpTitle ID="EntirePopupVisible" runat="server" Title="Make Entire Popup Visible" Span="true" OffsetY="-100" />
		</div>
		
		<%-- Web App Capable --%>
		<div class="checkboxOption">
			<asp:CheckBox ID="WebAppCapableCheckBox" runat="server" Enabled="false" />
			<AvantLogic:QuickHelpTitle ID="WebAppCapable" runat="server" Title="Make Tour Web App Capable" Span="true" OffsetY="-150" />
		</div>
		<%-- Unbranded option --%>
		<div class="checkboxOption">
			<asp:CheckBox ID="AllowUnbrandedCheckBox" runat="server" />
			<AvantLogic:QuickHelpTitle ID="AllowUnbranded" runat="server" Title="<%$ Resources:Text, AllowUnbrandedTitleLabel %>" Span="true" OffsetY="-224" OffsetX="20" />
		</div>
		
		<%-- Private option --%>
		<div class="checkboxOption">
			<asp:CheckBox ID="PrivateCheckBox" runat="server" />
			<AvantLogic:QuickHelpTitle ID="PrivateTour" runat="server" Span="true" Title="Private" OffsetY="-184" OffsetX="20" />
		</div>
		
		<div class="optionsSectionTitle">Advanced Options</div>

		<%-- Enable V3 Compatibility option --%>
		<div class="checkboxOption">
			<asp:CheckBox ID="EnableV3CompatibilityCheckBox" runat="server" Enabled="false" />
			<AvantLogic:QuickHelpTitle ID="EnableV3Compatibility" runat="server" Title="Enable V3 Compatibility" Span="true" OffsetY="-100" />
		</div>

		<%-- Disable Map Editor keyboard shortcuts --%>
		<div class="checkboxOption">
			<asp:CheckBox ID="DisableKeyboardShortcutsCheckBox" runat="server" />
			<AvantLogic:QuickHelpTitle ID="DisableKeyboardShortcuts" runat="server" Title="Disable Keyboard Shortcuts" Span="true" OffsetY="-100" />
		</div>
		
		<%-- Use Sound Manager --%>
		<div class="checkboxOption">
			<asp:CheckBox ID="UseSoundManagerCheckBox" runat="server" />
			<AvantLogic:QuickHelpTitle ID="UseSoundManager" runat="server" Title="Enable SoundManager" Span="true" OffsetY="-120" OffsetX="20" />
		</div>
		
		<%-- Special Actions --%>
		<div class="optionsSectionTitle" >Special Actions</div>
		<table style="margin-bottom:16px;">
			<tr>
				<td>
					<AvantLogic:MemberPageActionButton ID="RebuildControl" Title="Rebuild" runat="server" />
					<AvantLogic:QuickHelpTitle ID="TourManagerRebuild" runat="server" Span="true" OffsetY="-118" OffsetX="20" />
				</td>
				<td style="padding-left:12px;">
					<asp:Panel ID="RenumberPanel" runat="server">
						<AvantLogic:MemberPageActionButton ID="RenumberControl" Title="Renumber" runat="server" />
						<AvantLogic:QuickHelpTitle ID="TourManagerRenumber" runat="server" Span="true" OffsetY="-274" OffsetX="20" />
					</asp:Panel>
				</td>
				<td style="padding-left:12px;">
					<asp:Panel ID="CopyToSamplesPanel" runat="server" Visible="false">
						<AvantLogic:MemberPageActionButton ID="CopyToSamplesControl" Title="Copy to Samples" runat="server" />
					</asp:Panel>
				</td>
			</tr>
		</table>
	</div>
</asp:Content>

