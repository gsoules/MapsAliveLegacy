<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="TourLayoutAdvanced.aspx.cs"
	Inherits="Members_TourLayoutAdvanced"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<script type="text/javascript" language="javascript">                
	function maColorSchemeChanged()
	{
		maChangeDetectedForPreview();
		maTourResourceSelectionChanged("<%= ColorSchemeComboBox.ClientID %>", "<%= EditColorSchemeControl.ClientID %>", "EditColorScheme.aspx");
	}
	</script> 	
	
	<div class="optionsSectionTitleFirst">Tour Size (<asp:Label ID="TourSizeHelp" runat="server" class="finePrint" />)</div>
	
   	<asp:Panel ID="DimensionsPanel" runat="server" Visible="false">
	    <table class="optionsTable">
		    <tr>
			    <td style="width:60px;">
				    <AvantLogic:QuickHelpTitle ID="LayoutTourWidth" runat="server" Title="Width" TopMargin="0px" />
				    <asp:TextBox ID="TourWidth" runat="server" Width="30px" />
				    <asp:Label ID="TourHeightError" runat="server" CssClass="textErrorMessage" />
			    </td>
			    <td style="">
				    <AvantLogic:QuickHelpTitle ID="LayoutTourHeight" runat="server" Title="Height" TopMargin="0px" />
				    <asp:TextBox ID="TourHeight" runat="server" Width="30px" />
				    <asp:Label ID="TourWidthError" runat="server" CssClass="textErrorMessage" />
			    </td>
		    </tr>
	    </table>
	</asp:Panel>

	<asp:Panel ID="DimensionsPanelV3" runat="server" Visible="false">
        <table cellpadding="0" cellspacing="0">
		    <tr>
			    <td valign="top">
				    <table>
					    <tr>
						    <td>
							    <asp:DropDownList ID="PageWidthDropDownList" runat="server" Width="140" CssClass="dropDownList" />
							    <asp:Label ID="PageWidthError" runat="server" CssClass="textErrorMessage" />
						    </td>
						    <td>
							    <asp:TextBox ID="PageWidth" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
							    <AvantLogic:QuickHelpTitle ID="LayoutPageWidth" runat="server" Span="True" />
						    </td>
						    <td style="padding-left:30px;">
							    <asp:DropDownList ID="PageHeightDropDownList" runat="server" Width="130" CssClass="dropDownList" />
							    <asp:Label ID="PageHeightError" runat="server" CssClass="textErrorMessage"/>
						    </td>
						    <td>
							    <asp:TextBox ID="PageHeight" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
							    <AvantLogic:QuickHelpTitle ID="LayoutPageHeight" runat="server" Span="True" />
						    </td>
					    </tr>
				    </table>
			    </td>
		    </tr>
	    </table>
	</asp:Panel>

	<asp:Panel ID="MenuOptions" runat="server" Visible="false">
	    <div class="optionsSectionTitle">Navigation Options</div>
        <asp:Label ID="NoNavOptionsExplanation" runat="server" Visible="false" CssClass="textNormal"></asp:Label>
	    <table id="NavOptionsTable" runat="server" class="optionsTable" style="width:500px;">
		    <tr>
 				<td valign="top">
					<AvantLogic:QuickHelpTitle ID="NavButtonLocation" runat="server" Title="Nav Button Location" TopMargin="0px" />
					<asp:DropDownList ID="LocationDropDownList" runat="server" style="margin-top:4px;"></asp:DropDownList>
				</td>
                <td>
		            <AvantLogic:QuickHelpTitle ID="NavButtonOffsets" runat="server" Title="Button Offsets" TopMargin="0px" />
		            <table style="padding:0px;">
			            <tr>
				            <td>
					            <asp:Label runat="server" CssClass="controlLabel" Text="X" />
				            </td>
				            <td>
					            <asp:TextBox ID="CustomLocationX" runat="server" Width="24px"></asp:TextBox><span class="unit">px</span>
					            <asp:Label ID="CustomLocationXError" runat="server" CssClass="textErrorMessage" />
				            </td>
				            <td style="padding-left:8px;">
					            <asp:Label runat="server" CssClass="controlLabel" Text="Y" />
				            </td>
				            <td>
					            <asp:TextBox ID="CustomLocationY" runat="server" Width="24px"></asp:TextBox><span class="unit">px</span>
					            <asp:Label ID="CustomLocationYError" runat="server" CssClass="textErrorMessage" />
				            </td>
			            </tr>
		            </table>
                </td>
			    <td>
				    <div class="checkboxOption">
					    <asp:CheckBox ID="HideNavButtonCheckBox" runat="server" />
					    <AvantLogic:QuickHelpTitle ID="HideNavButton" runat="server" Title="<%$ Resources:Text, LayoutNoMenuLabel %>" Span="true" />
				    </div>
                </td>
           </tr>
	    </table>
	</asp:Panel>

	<asp:Panel ID="MenuOptionsV3" runat="server" Visible="false">
	    <div class="optionsSectionTitle">Menu Options</div>
	    <table class="optionsTable">
		    <tr>
			    <td style="width:120px;">
				    <%-- Menu Style option --%>
				    <AvantLogic:QuickHelpTitle ID="MenuStyle" runat="server" Title="<%$ Resources:Text, LayoutMenuStyleLabel %>" TopMargin="0px" />
				    <asp:DropDownList ID="MenuStyleDropDownList" runat="server" />
			    </td>
			    <td style="width:150px;">
				    <%-- Menu Location option --%>
				    <AvantLogic:QuickHelpTitle ID="Navigation" runat="server" Title="<%$ Resources:Text, LayoutNavigationLabel %>" TopMargin="0px" />
				    <asp:DropDownList ID="NavigationDropDownList" runat="server" />
			    </td>
			    <td style="width:135px;">
				    <%-- Menu Width option --%>
				    <AvantLogic:QuickHelpTitle ID="MenuWidth" runat="server" Title="<%$ Resources:Text, LayoutMenuWidthLabel %>" TopMargin="0px" />
				    <asp:TextBox ID="MenuWidthTextBox" runat="server" Width="30px" />
				    <asp:Label ID="MenuWidthError" runat="server" CssClass="textErrorMessage" />
			    </td>
			    <td style="">
				    <%-- Menu Scrolls option --%>
				    <div class="checkboxOptionFirst">
					    <asp:CheckBox ID="MenuScrollsCheckBox" runat="server" />
					    <AvantLogic:QuickHelpTitle ID="MenuScrolls" runat="server" Title="<%$ Resources:Text, LayoutMenuScrollsLabel %>" TopMargin="0px" Span="true" />
				    </div>
			    </td>
		    </tr>
	    </table>
	</asp:Panel>

	<div class="optionsSectionTitle"">Layout Options</div>

	<div id="TitleAndStripeOptions" runat="server" class="checkboxOption">
		<asp:CheckBox ID="ShowTitleBarCheckBox" runat="server" />
		<AvantLogic:QuickHelpTitle ID="PageTitle" runat="server" Title="Title Bar" Span="true" />
		
		<asp:CheckBox ID="ShowHeaderStripeCheckBox" runat="server" style="margin-left:30px;" />
		<AvantLogic:QuickHelpTitle ID="ShowHeaderStripe" runat="server" Span="true" Title="Header Stripe" />
		
		<asp:CheckBox ID="ShowFooterStripeCheckBox" runat="server" style="margin-left:30px;" />
		<AvantLogic:QuickHelpTitle ID="ShowFooterStripe" runat="server" Span="true" Title="Footer Stripe" />
		
	</div>
		
	<table class="optionsTable" style="margin-top:16px;">
		<tr>
			<td width="260px;>
				<AvantLogic:QuickHelpTitle ID="ColorSchemeOption" runat="server" Title="Color Scheme" Span="true" />
				<table cellpadding="0" cellspacing="0">
					<tr>
						<td>
							<uc:TourResourceComboBox Id="ColorSchemeComboBox" runat="server" />
						</td>
						<td style="padding-left:4px;">
							<AvantLogic:MemberPageActionButton Subtle="true" ID="EditColorSchemeControl" runat="server" />
						</td>
					</tr>
				</table>
			</td>
			<td width="150px;" style="padding-left:24px;">
				<AvantLogic:QuickHelpTitle ID="BodyBackgroundColor" runat="server" Title="Background Color" TopMargin="0px" />
				<AvantLogic:ColorSwatch Id="BackgroundColorSwatch" runat="server" />
			</td>
			<td id="BodyMarginOption" runat="server">
				<AvantLogic:QuickHelpTitle ID="BodyMargin" runat="server" Title="<%$ Resources:Text, BodyMarginLabel %>" TopMargin="0px" />
				<asp:TextBox ID="BodyMarginTextBox" runat="server" Width="30px" /><span class="unit">px</span>
				<asp:Label ID="BodyMarginError" runat="server" CssClass="textErrorMessage" />
			</td>
            <td id="LeftAlignedOption" runat="server">
		        <asp:CheckBox ID="LeftAlignedCheckBox" runat="server" style="margin-left:21px;" />
		        <AvantLogic:QuickHelpTitle ID="LeftAlignedInBrowser" runat="server" Title="Left Aligned" Span="true" />
            </td>
		</tr>
	</table>
		
	<%-- Custom Footer --%>
	<div id="FooterOption" runat="server" style="margin-left:4px;">
		<AvantLogic:QuickHelpTitle ID="CustomFooter" runat="server" Title="<%$ Resources:Text, CustomerFooterLabel %>" OffsetX="20" OffsetY="-350"/>
		<asp:TextBox ID="CustomFooterText" runat="server" Width="550px"></asp:TextBox>
		<div style="margin-bottom:8px;" class="unit"><i>Example: </i>Created with~MapsAlive~https://www.mapsalive.com <i>displays as </i>Created with <a href="https://www.mapsalive.com">MapsAlive</a></div>
	</div>

	<asp:Panel ID="CurrentPagePanel" runat="server">
		
        <asp:Panel ID="LayoutOptionsPanel" runat="server">
            <div class="optionsSectionTitle"">Layout Actions</div>

		    <table>
			    <tr>
				    <td>
					    <AvantLogic:MemberPageActionButton ID="RunAutoLayoutControl" runat="server" />
					    <AvantLogic:QuickHelpTitle ID="RunAutoLayout" runat="server" Span="true" OffsetX="20" OffsetY="-300" />
				    </td>
				    <td style="padding-left:12px;">
					    <AvantLogic:MemberPageActionButton ID="RestoreLayoutControl" runat="server"/>
					    <AvantLogic:QuickHelpTitle ID="RestoreLayout" runat="server" Span="true" OffsetX="20" OffsetY="-156" />
				    </td>
				    <td style="padding-left:12px;">
					    <AvantLogic:MemberPageActionButton ID="ToggleAutoLayoutControl" runat="server" />
					    <AvantLogic:QuickHelpTitle ID="DisableAutoLayout" runat="server" Span="true" />
					    <AvantLogic:QuickHelpTitle ID="EnableAutoLayout" runat="server" Span="true" />
				    </td>
			    </tr>
		    </table>
    	</asp:Panel>

		<div id="PreviewMessage" class="optionsSectionTitle">Preview</div>

		<asp:Panel ID="LayoutPreviewImagePanel" runat="server" style="margin-top:8px;">
			<div class="finePrintHelp" style="margin-bottom:12px;">
				This preview is an approximation. For more accuracy, use Tour Preview.
			</div>
			
			<asp:Image ID="PagePreviewImage" runat="server" />
		</asp:Panel>
	</asp:Panel>
</asp:Content>

