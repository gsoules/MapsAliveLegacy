<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="PopupBehavior.aspx.cs"
	Inherits="Members_PopupBehavior"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<script language="javascript">
	var selectedLocation = 0;               
	function maLocationChanged(init, location)
	{
		if (!init)
			maChangeDetected();
		
		selectedLocation = location;
		
		var fixedLocationPanelStyle = document.getElementById("<%= FixedLocationPanel.ClientID %>").style;
		var offsetPanelStyle = document.getElementById("<%= OffsetPanel.ClientID %>").style;
		
		var isFixedLocation = location >= 6;
		fixedLocationPanelStyle.display = isFixedLocation ? "block" : "none";
 		offsetPanelStyle.display = !isFixedLocation ? "block" : "none";
 		
 		var fixedAlwaysVisible = location == 7;
		document.getElementById("<%= OptionsPanel.ClientID %>").style.display = !fixedAlwaysVisible ? "block" : "none";
		document.getElementById("<%= DelayPanel.ClientID %>").style.display = !fixedAlwaysVisible ? "block" : "none";
		document.getElementById("<%= HelpPanel.ClientID %>").style.display = fixedAlwaysVisible ? "block" : "none";
		
		var pinChecked = document.getElementById("<%= PinOnClickCheckBox.ClientID %>").checked;
        var pinMessagePanel = document.getElementById("<%= PinMessagePanel.ClientID %>");
        if (pinMessagePanel)
		    pinMessagePanel.style.display = pinChecked && (location == 3 || location == 5)  ? "block" : "none";
	}
	function maOptionsChanged()
	{
		maChangeDetected();
		maLocationChanged(false, selectedLocation);
	}
	</script>
	
    <div class="optionsSectionTitleFirst">
		Popup Location
		<AvantLogic:QuickHelpTitle ID="PopupLocationOption" runat="server" Span="true"/>
	</div>
	
	<div class="textNormal" style="margin-top:0px;margin-bottom:16px;color:#333;">
		<div style="margin-top:2px;">
			<asp:RadioButton ID="RadioButton1" runat="server" GroupName="Location" OnClick="maLocationChanged(false, 1);" Text="Center of marker. Allow mouse onto popup." />
		</div>
		<div style="margin-top:2px;">
			<asp:RadioButton ID="RadioButton2" runat="server" GroupName="Location" OnClick="maLocationChanged(false, 1);" Text="Next to marker. Allow mouse onto popup." />
		</div>
		<div style="margin-top:2px;">
			<asp:RadioButton ID="RadioButton3" runat="server" GroupName="Location" OnClick="maLocationChanged(false, 3);" Text="Next to marker. Don't allow mouse onto popup." />
		</div>
		<div style="margin-top:2px;">
			<asp:RadioButton ID="RadioButton4" runat="server" GroupName="Location" OnClick="maLocationChanged(false, 4);" Text="Mouse cursor. Allow mouse onto popup." />
		</div>
		<div style="margin-top:2px;">
			<asp:RadioButton ID="RadioButton5" runat="server" GroupName="Location" OnClick="maLocationChanged(false, 5);" Text="Mouse cursor. Popup follows mouse while over marker." />
		</div>
		<div style="margin-top:2px;">
			<asp:RadioButton ID="RadioButton6" runat="server" GroupName="Location" OnClick="maLocationChanged(false, 6);" Text="Fixed location. Visible while mouse is over marker." />
		</div>
		<div style="margin-top:2px;">
			<asp:RadioButton ID="RadioButton7" runat="server" GroupName="Location" OnClick="maLocationChanged(false, 7);" Text="Fixed location. Always visible." />
		</div>
	</div>
	
	<asp:Panel ID="FixedLocationPanel" runat="server" >
		<AvantLogic:QuickHelpTitle ID="PopupFixedLocation" runat="server" Title="Fixed Location" />
		<table>
			<tr>
				<td>
					<asp:Label runat="server" CssClass="controlLabel" Text="<%$ Resources:Text, PopupSlideXLabel %>"></asp:Label>
				</td>
				<td>
					<asp:TextBox ID="PopupSlideX" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
					<asp:Label ID="PopupSlideXError" runat="server" CssClass="textErrorMessage" />
				</td>
				<td style="padding-left:8px;">
					<asp:Label runat="server" CssClass="controlLabel" Text="<%$ Resources:Text, PopupSlideYLabel %>"></asp:Label>
				</td>
				<td>
					<asp:TextBox ID="PopupSlideY" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
					<asp:Label ID="PopupSlideYError" runat="server" CssClass="textErrorMessage" />
				</td>
			</tr>
		</table>
	</asp:Panel>
	
	<asp:Panel ID="OffsetPanel" runat="server">
        <AvantLogic:QuickHelpTitle ID="PopupsShowArrow" runat="server" Title="Arrow" Visible="false" />
        <AvantLogic:QuickHelpTitle ID="PopupsCallout" runat="server" Title="Callout" Visible="false" />
        <asp:DropDownList ID="ArrowDropDown" runat="server"></asp:DropDownList>

        <AvantLogic:QuickHelpTitle ID="PopupOffset" runat="server" Title="Popup Offset" OffsetY="-200" OffsetX="20" />
		<table>
			<tr>
				<td>
					<asp:TextBox ID="PopupMarkerOffset" runat="server" Width="30px" /><span class="unit">px</span>
					<asp:Label ID="PopupMarkerOffsetError" runat="server" CssClass="textErrorMessage" />
				</td>
			</tr>
		</table>
	</asp:Panel>
	
	<AvantLogic:QuickHelpTitle ID="PopupBestSideSequence" runat="server" Title="Positioning Sequence" OffsetY="-200" OffsetX="20" />
	<asp:DropDownList ID="BestSideSequenceDropDown" runat="server">
		<asp:ListItem Text="Right, Left, Below, Above" Value="0" />
		<asp:ListItem Text="Right, Left, Above, Below" Value="1" />
		<asp:ListItem Text="Right, Below, Above, Left" Value="2" />
		<asp:ListItem Text="Right, Below, Left, Above" Value="3" />
		<asp:ListItem Text="Left, Right, Above, Below" Value="4" />
		<asp:ListItem Text="Left, Right, Below, Above" Value="5" />
		<asp:ListItem Text="Left, Above, Right, Below" Value="6" />
		<asp:ListItem Text="Left, Above, Below, Right" Value="7" />
		<asp:ListItem Text="Below, Above, Left, Right" Value="8" />
		<asp:ListItem Text="Below, Above, Right, Left" Value="9" />
		<asp:ListItem Text="Below, Left, Above, Right" Value="10" />
		<asp:ListItem Text="Below, Left, Right, Above" Value="11" />
		<asp:ListItem Text="Above, Below, Left, Right" Value="12" />
		<asp:ListItem Text="Above, Below, Right, Left" Value="13" />
		<asp:ListItem Text="Above, Right, Below, Left" Value="14" />
		<asp:ListItem Text="Above, Right, Left, Below" Value="15" />
		<asp:ListItem Text="Outside Best Fit (100%)" Value="24" />
		<asp:ListItem Text="Outside Loose Fit (75%)" Value="25" />
		<asp:ListItem Text="Outside Tight Fit (50%)" Value="26" />
		<asp:ListItem Text="Above, Below" Value="20" />
		<asp:ListItem Text="Below, Above" Value="21" />
		<asp:ListItem Text="Right, Left" Value="22" />
		<asp:ListItem Text="Left, Right" Value="23" />
		<asp:ListItem Text="Above" Value="16" />
		<asp:ListItem Text="Below" Value="17" />
		<asp:ListItem Text="Right" Value="18" />
		<asp:ListItem Text="Left" Value="19" />
	</asp:DropDownList>

	<div class="optionsSectionTitle">Options</div>
	<asp:Panel ID="OptionsPanel" runat="server">
		<div>
			<asp:CheckBox ID="ShowTooltipCheckBox" runat="server" />
			<AvantLogic:QuickHelpTitle ID="PopupShowTooltip" runat="server" Span="true" Title="Show Tooltip When No Hotspot Content" TopMargin="0px" OffsetY="-100" OffsetX="20" />
		</div>
		<div style="margin-top:4px;">
			<asp:CheckBox ID="PinOnClickCheckBox" runat="server" />
			<AvantLogic:QuickHelpTitle ID="PopupPinOnClick" runat="server" Span="true" Title="Pin Popup When Marker Clicked" TopMargin="0px" OffsetY="-100" OffsetX="20" />
			<asp:Panel ID="PinMessagePanel" runat="server" style="margin-top:12px;">
				<AvantLogic:QuickHelpTitle ID="PinMessageOption" runat="server" Title="Pin Message"/>
				<asp:TextBox ID="PinMessage" runat="server" Width="300px" />
			</asp:Panel>
		</div>
	</asp:Panel>
	
	<asp:Panel ID="HelpPanel" runat="server" class="textHelp" style="margin-top:0px;">
		<asp:Label ID="HelpPanelText" runat="server" />
	</asp:Panel>
	
	<asp:Panel ID="DelayPanel" runat="server">
		<div class="optionsSectionTitle">
			Delay
			<AvantLogic:QuickHelpTitle ID="PopupDelayOptions" runat="server" Span="true" OffsetY="-160" OffsetX="20" />
		</div>
		<table>
			<tr>
				<td>
					<asp:DropDownList ID="DelayDropDown" runat="server">
						<asp:ListItem Text="None" Value="0" />
						<asp:ListItem Text="Delay before showing popup" Value="1" />
						<asp:ListItem Text="Close popup after delay" Value="2" />
					</asp:DropDownList>
				</td>
				<td>
					<asp:TextBox ID="PopupDelay" runat="server" Width="30px"></asp:TextBox><span class="unit">ms</span>
					<asp:Label ID="PopupDelayError" runat="server" CssClass="textErrorMessage" />
				</td>
			</tr>
		</table>
	</asp:Panel>

</asp:Content>

