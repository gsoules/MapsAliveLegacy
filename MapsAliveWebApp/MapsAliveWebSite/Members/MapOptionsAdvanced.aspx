<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="MapOptionsAdvanced.aspx.cs"
	Inherits="Members_MapOptionsAdvanced"
	ValidateRequest="false"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<script language="javascript">                
        tinymce.init({
            selector: `.HtmlEditor`,
            content_css: '../Styles/Editor.css',
            placeholder: 'Enter help text here',
            min_height: 200,
            max_width: 1600,
            resize: 'both',
            removed_menuitems: 'newdocument visualaid',
            plugins: 'code paste autoresize link lists charmap media image charmap hr',
            paste_enable_default_filters: true,
            forced_root_block: false,
            fontsize_formats: "8px 9px 10px 11px 12px 13px 14px 15px 16px 18px 20px 22px 24px 28px 32px 36px",
            branding: false,
            autoresize_bottom_margin: 8,
            menubar: 'format insert edit',
            toolbar: 'bold italic underline | fontselect fontsizeselect | forecolor backcolor | link unlink | bullist numlist | code',
            setup: function (editor)
            {
                editor.on('Change', function (e)
                {
                    maChangeDetected();
                });
            }
        });
	function maAllowEnter(e)
	{
		if (window.event)
			window.event.cancelBubble = true;
		else
			e.stopPropagation();
		return true;
	}
	function maShowInstructionsText(showInstructionsTextCheckBox)
	{
		maChangeDetected();
		document.getElementById(formContentId + "InstructionsTextPanel").style.display = showInstructionsTextCheckBox.checked ? "block" : "none";	
	}
	function maEnableSlideListInstructions(slideListCheckBox)
	{
		maChangeDetected();
		document.getElementById('<%=ShowSlideListOptionsPanel.ClientID%>').style.display=slideListCheckBox.checked?'block':'none';
	}
    function maTooltipStyleChanged()
    {
        maTourResourceSelectionChanged("<%= TooltipStyleComboBox.ClientID %>", "<%= EditTooltipStyleControl.ClientID %>", "EditTooltipStyle.aspx");
    }
    </script> 
	 
	
	<table cellpadding="0" cellspacing="0">
		<tr>
			<td>
				<AvantLogic:QuickHelpTitle ID="MapUrl" runat="server" Title="URL" TopMargin="0px" />
				<asp:Label ID="CodeSnippets" runat="server" CssClass="textNormal" />
			</td>
			<td style="padding-left:32px;">
				<AvantLogic:QuickHelpTitle ID="MapBackgroundColor" runat="server" Title="Map Background Color" />
				<AvantLogic:ColorSwatch Id="MapBackgroundColorSwatch" runat="server" />
			</td>
		</tr>
		<tr>
			<td>
				<AvantLogic:QuickHelpTitle ID="TourPageTitle" runat="server" Title="<%$ Resources:Text, PageTitleLabel %>" />
				<asp:TextBox ID="TourTitleTextBox" runat="server" Width="280px"></asp:TextBox>
			</td>
			<td style="padding-left:32px;">
				<AvantLogic:QuickHelpTitle ID="PageId" runat="server" Title="Map Id" />
				<asp:TextBox ID="PageIdTextBox" Width="100" runat="server"></asp:TextBox>
				<asp:Label ID="PageIdError" runat="server" CssClass="textErrorMessage" />
			</td>
		</tr>
	</table>

    <asp:Panel ID="TooltipPanel" runat="server">
        <div style="margin-top:16px;margin-bottom:2px;">
		    <AvantLogic:QuickHelpTitle ID="PageTooltipStyle" runat="server" Title="Tooltip Style" Span="true" />
	    </div>
	    <table cellpadding="0" cellspacing="0">
		    <tr>
			    <td>
				    <uc:TourResourceComboBox Id="TooltipStyleComboBox" runat="server" />
			    </td>
			    <td style="padding-left:4px;">
				    <AvantLogic:MemberPageActionButton Subtle="true" ID="EditTooltipStyleControl" runat="server" />
			    </td>
		    </tr>
	    </table>
    </asp:Panel>
	
	<div class="optionsSectionTitle">Hotspot Content Options</div>
	<div class="checkboxOption" style="margin-top:8px;">
		<asp:CheckBox ID="ShowTitleCheckBox" runat="server" />
		<AvantLogic:QuickHelpTitle ID="ShowSlideTitle" runat="server" Title="<%$ Resources:Text, ShowSlideTitleLabel %>" Span="true" />
	</div>

	<asp:Panel ID="MapZoomPanel" runat="server">
		<div class="optionsSectionTitle">MapZoom Options</div>
		<asp:Panel ID="MapZoomDisabledPanel" runat="server" class="textHelp" Visible="false">
			MapZoom is turned off. To turn it on, go to the Map Setup screen.
		</asp:Panel>
		<asp:Panel ID="MapZoomOptionsPanel" runat="server">
			<div id="MapZoomIsOnMessage" runat="server" class="textHelp">MapZoom is turned on. To turn it off, go to the Map Setup screen.</div>
			<div style="margin-top:16px;margin-bottom:16px;">
				<asp:CheckBox ID="MapZoomEnabledCheckbox" runat="server" />
				<AvantLogic:QuickHelpTitle ID="MapZoomEnabled" runat="server" Title="<%$ Resources:Text, MapZoomEnabledLabel %>" Span="true" />
			</div>
			<table cellpadding="0" cellspacing="0">
				<tr>
					<td>
						<table cellpadding="0" cellspacing="0">
							<tr>
								<td>
									<AvantLogic:QuickHelpTitle ID="MapZoomInsetLocation" runat="server" Title="<%$ Resources:Text, MapZoomInsetLocationLabel %>" TopMargin="0px" />
									<asp:DropDownList ID="MapZoomInsetLocationDropDown" runat="server">
										<asp:ListItem Text="No Inset" Value="0" Selected="True" />
										<asp:ListItem Text="Upper Right" Value="2" />
										<asp:ListItem Text="Lower Right" Value="3" />
										<asp:ListItem Text="Lower Left" Value="4" />
									</asp:DropDownList>
								</td>
								<td style="padding-left:16px;">
									<AvantLogic:QuickHelpTitle ID="MapZoomInsetSize" runat="server" Title="<%$ Resources:Text, MapZoomInsetSizeLabel %>" TopMargin="0px" />
									<asp:TextBox ID="MapZoomInsetSizeTextBox" runat="server" Width="30px" /><span class="unit">px</span>
									<asp:Label ID="MapZoomInsetSizeError" runat="server" CssClass="textErrorMessage" />
								</td>
							</tr>
						</table>
                        <div style="margin-top:16px;">
                            <AvantLogic:QuickHelpTitle ID="MapImageSharpening" runat="server" Title="Map Image Sharpening" TopMargin="0px" />
							<asp:DropDownList ID="MapImageSharpeningDropDownList" runat="server">
								<asp:ListItem Text="None" Value="0" Selected="True" />
								<asp:ListItem Text="Soft" Value="1" />
								<asp:ListItem Text="Sharp" Value="2" />
								<asp:ListItem Text="Balanced" Value="3" />
							</asp:DropDownList>
                        </div>
					</td>
					</td>
					<td valign="top" style="padding-left:48px;">
						<table>
							<AvantLogic:ColorSwatch Id="ControlOffColorSwatch" Label="Zoom Controls Color" QuickHelpTitle="ControlOffColor" Row="true" runat="server" />
							<AvantLogic:ColorSwatch Id="MapInsetColorSwatch" Label="Inset Highlight Color" QuickHelpTitle="MapInsetColor" Row="true" runat="server" TopMargin="0px" />
						</table>			
					</td>
				</tr>
			</table>
		</asp:Panel>
	</asp:Panel>

	<div class="optionsSectionTitle">Help Options</div>
	<asp:Panel ID="InstructionsCheckbox" runat="server" CssClass="checkboxOption">
		<asp:CheckBox ID="ShowInstructionsCheckBox" runat="server" />
		<AvantLogic:QuickHelpTitle ID="ShowInstructions" runat="server" Title="<%$ Resources:Text, ShowInstructionsLabel %>" Span="true" />
	</asp:Panel>
			
	<asp:Panel ID="InstructionsTextPanel" runat="server" style="margin-top:8px;">
		<table cellpadding="0" cellspacing="0">
			<tr>
				<td valign="top">
					<table style="vertical-align:top;">
						<tr>
							<td>
								<AvantLogic:QuickHelpTitle ID="InstructionsWidth" runat="server" Title="<%$ Resources:Text, InstructionsWidthLabel %>" TopMargin="0px" />	
								<asp:TextBox ID="InstructionsWidthTextBox" runat="server" Width="30px" /><span class="unit">px</span>
								<asp:Label ID="InstructionsWidthError" runat="server" CssClass="textErrorMessage" />
							</td>
						</tr>
						<tr>
							<td>
								<AvantLogic:QuickHelpTitle ID="InstructionsTextColor" runat="server" Title="<%$ Resources:Text, InstructionsTextColorLabel %>" />
								<AvantLogic:ColorSwatch Id="InstructionsTextColorSwatch" runat="server" />
							</td>
						</tr>
						<tr>
							<td>
								<AvantLogic:QuickHelpTitle ID="InstructionsBackgroundColor" runat="server" Title="<%$ Resources:Text, InstructionsBackgroundColorLabel %>" />
								<AvantLogic:ColorSwatch Id="InstructionsBackgroundColorSwatch" runat="server" />
							</td>	                        
						</tr>
						<tr>
							<td>
								<AvantLogic:QuickHelpTitle ID="InstructionsTitle" runat="server" Title="<%$ Resources:Text, InstructionsTitleLabel %>" />
								<asp:TextBox ID="InstructionsTitleTextBox" runat="server" Width="350" />
							</td>
						</tr>
					</table>
				</td>
			</tr>
		</table>
        <div style="margin-left:2px;">
		    <AvantLogic:QuickHelpTitle ID="InstructionsText" runat="server" Title="<%$ Resources:Text, InstructionsTextLabel %>" />
		    <asp:TextBox ID="InstructionsTextBox" runat="server" onkeypress="return maAllowEnter(event);" Rows="12" Width="712" TextMode="MultiLine" />
        </div>
	</asp:Panel>
	
	<div class="optionsSectionTitle">Marker Options</div>
	<table cellpadding="0" cellspacing="0">
		<tr>
			<td>
				<%-- Selected Marker Blink option --%>
				<AvantLogic:QuickHelpTitle ID="MarkerBlink" runat="server" Title="<%$ Resources:Text, MarkerBlinkLabel %>" TopMargin="0px" />
				<asp:TextBox ID="SelectedMarkerBlinkTextBox" runat="server" Width="30px" /><span class="unit">times</span>
				<asp:Label ID="MarkerBlinkError" runat="server" CssClass="textErrorMessage" />
			</td>
			<td style="padding-left:24px;">
				<%-- Visited Marker Opacity option --%>
				<AvantLogic:QuickHelpTitle ID="VisitedMarkerOpacity" runat="server" Title="<%$ Resources:Text, VisitedMarkerOpacityLabel %>" TopMargin="0px" />
				<asp:TextBox ID="VisitedMarkerOpacityTextBox" runat="server" Width="30px" /><span class="unit">%</span>
				<asp:Label ID="VisitedMarkerOpacityError" runat="server" CssClass="textErrorMessage" />
			</td>
		</tr>
	</table>
	<div class="checkboxOptionFirst">
		<asp:CheckBox ID="MarkersZoomCheckBox" runat="server" CssClass="checkboxOption" />
		<AvantLogic:QuickHelpTitle ID="MarkerZoomType" runat="server" Title="Markers Zoom" Span="true" />
	</div>

	<div class="optionsSectionTitle">Navigation Options</div>

	<asp:Panel ID="ShowSlideNamesInMenuPanel" runat="server" visible="false">
	    <div class="checkboxOption">
		    <asp:CheckBox ID="ShowSlideNamesInMenuCheckBox" runat="server" CssClass="checkboxOption" />
		    <AvantLogic:QuickHelpTitle ID="ShowSlideNamesInMenu" runat="server" Title="<%$ Resources:Text, ShowSlideTitlesInMenuLabel %>" Span="true" />
	    </div>
    </asp:Panel>
	
	<div class="checkboxOption">
		<asp:CheckBox ID="ShowSlideListCheckBox" runat="server" CssClass="checkboxOption" />
		<AvantLogic:QuickHelpTitle ID="ShowSlideList" runat="server" Title="<%$ Resources:Text, ShowSlideListLabel %>" Span="true" />
	</div>
	
	<asp:Panel ID="ShowSlideListOptionsPanel" runat="server" style="margin-left:20px;margin-bottom:8px;">
		<table>
			<tr>
				<td>
					<div style="padding:4px;">
						<AvantLogic:QuickHelpTitle ID="ShowPageHotspotsTitle" runat="server" Title="Instructions" TopMargin="0px" />
						<asp:TextBox ID="SlideListInstructionsTextBox" runat="server" Width="300" />
						<asp:Label ID="SlideListInstructionsError" runat="server" CssClass="textErrorMessage" />
					</div>
				</td>
			</tr>
		</table>
	</asp:Panel>

	<div class="checkboxOption" style="margin-bottom:4px;">
		<asp:CheckBox ID="ExcludeFromNavigationCheckbox" runat="server" />
		<AvantLogic:QuickHelpTitle ID="ExcludeFromNavigation" runat="server" Title="Exclude From Navigation" Span="true" />
	</div>
	
	<div class="optionsSectionTitle">Slide Show Options</div>
	<div class="checkboxOption">
		<asp:CheckBox ID="RunSlideShowCheckBox" runat="server" />
		<AvantLogic:QuickHelpTitle ID="RunSlideShow" runat="server" Title="<%$ Resources:Text, RunSlideShowLabel %>" Span="true" OffsetY="-254" />
	</div>
	
	<div class="unit" style="margin-top:4px;">
		Show hotspot for
		<asp:DropDownList ID="SlideShowDropDown" runat="server">
			<asp:ListItem Text="1" Value="1" />
			<asp:ListItem Text="2" Value="2" />
			<asp:ListItem Text="3" Value="3" />
			<asp:ListItem Text="4" Value="4" />
			<asp:ListItem Text="5" Value="5" />
			<asp:ListItem Text="6" Value="6" />
			<asp:ListItem Text="7" Value="7" />
			<asp:ListItem Text="8" Value="8" />
			<asp:ListItem Text="9" Value="9" />
			<asp:ListItem Text="10" Value="10" />
			<asp:ListItem Text="11" Value="11" />
			<asp:ListItem Text="12" Value="12" />
			<asp:ListItem Text="13" Value="13" />
			<asp:ListItem Text="14" Value="14" />
			<asp:ListItem Text="15" Value="15" />
			<asp:ListItem Text="16" Value="16" />
			<asp:ListItem Text="17" Value="17" />
			<asp:ListItem Text="18" Value="18" />
			<asp:ListItem Text="19" Value="19" />
			<asp:ListItem Text="20" Value="20" />
		</asp:DropDownList>
		<span class="unit">seconds</span>
	</div>

	<asp:Panel ID="HotspotListPanel" runat="server" style="margin-top:16px;">
		<AvantLogic:QuickHelpTitle ID="HotspotOrder" runat="server" Title="Hotspot Order" TopMargin="0px" />
		<telerik:RadListBox
			runat="server"
			ID="HotspotOrderListBox"
			Width="300px"
			OnClientReordered="maChangeDetected"
			EnableDragAndDrop="true"
			>
		</telerik:RadListBox>
	</asp:Panel>
	
	<asp:Panel ID="RouteTestingPanel" runat="server">
		<div class="optionsSectionTitle">Route Testing</div>
		<AvantLogic:QuickHelpTitle ID="RoutesTestId" runat="server" Title="Show Routes List" TopMargin="0px" OffsetY="-230" />
		<asp:CheckBox ID="RouteTestCheckBox" runat="server" />
	</asp:Panel>
</asp:Content>