<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="EditColorScheme.aspx.cs"
	Inherits="Members_EditColorScheme"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<AvantLogic:QuickHelpTitle ID="TourStyleName" runat="server" Title="<%$ Resources:Text, ColorSchemeNameLabel %>" TopMargin="0px" Span="true" />
	<AvantLogic:MemberPageActionButton ID="ShowUsageControl" runat="server" />
	<br />
	<asp:TextBox ID="ColorSchemeNameTextBox" runat="server" Width="200" />
	<asp:Label ID="ColorSchemeNameError" runat="server" CssClass="textErrorMessage" />
	
	<div class="optionsSectionTitle">Text and Background Colors</div>
	<table>
		<tr>
			<AvantLogic:ColorSwatch Id="TitleTextColorSwatch" Label="Tour Title Text" LabelWidth="124" QuickHelpTitle="TourStyleTitleText" runat="server" Col="true" ForPreview="true" />
			<AvantLogic:ColorSwatch Id="SlideTitleTextColorSwatch" Label="Text Area Title" LabelWidth="124" QuickHelpTitle="TourStyleSlideTitleText" runat="server" Col="true" ForPreview="true" />
		</tr>
		<tr>
			<AvantLogic:ColorSwatch Id="TitleBackgroundColorSwatch" Label="Tour Title Background" QuickHelpTitle="TourStyleTitleBackground" runat="server" Col="true" ForPreview="true" />
			<AvantLogic:ColorSwatch Id="SlideTextColorSwatch" Label="Text Area Text" LabelWidth="124" QuickHelpTitle="TourStyleSlideText" runat="server" Col="true" ForPreview="true" />
		</tr>
		<tr>
			<AvantLogic:ColorSwatch Id="FooterLinkColorSwatch" Label="Footer Link Text" QuickHelpTitle="TourStyleFooterLink" runat="server" Col="true" ForPreview="true" />
			<AvantLogic:ColorSwatch Id="SlideBackgroundColorSwatch" Label="Text Area Background" QuickHelpTitle="TourStyleSlideBackground" runat="server" Col="true" ForPreview="true" />
		</tr>
	</table>

	<div class="optionsSectionTitle">Layout Colors</div>
	<table>
		<tr>
			<AvantLogic:ColorSwatch Id="StripeColorSwatch" Label="Stripe" QuickHelpTitle="TourStyleStripeColor" runat="server" Col="true" ForPreview="true" />
			<AvantLogic:ColorSwatch Id="StripeBorderColorSwatch" Label="Stripe Border" QuickHelpTitle="TourStyleStripeBorderColor" runat="server" Col="true" ForPreview="true" />
		</tr>
		<tr>
			<AvantLogic:ColorSwatch Id="MapAreaBackgroundColorSwatch" Label="Map Area Background" LabelWidth="124" QuickHelpTitle="TourStyleMapArea" runat="server" Col="true" ForPreview="true" />
			<AvantLogic:ColorSwatch Id="BannerBackgroundColorSwatch" Label="Banner Background" LabelWidth="124" QuickHelpTitle="TourStyleBanner" runat="server" Col="true" ForPreview="true" />
		</tr>
		<tr>
			<AvantLogic:ColorSwatch Id="LayoutAreaBackgroundColorSwatch" Label="Canvas Background" LabelWidth="124" QuickHelpTitle="TourStyleCanvas" runat="server" Col="true" ForPreview="true" />
			<td colspan="2" style="width:280px;"></td>
		</tr>
	</table>

	<div class="optionsSectionTitle">Menu Colors</div>
	<table>
		<tr>
			<AvantLogic:ColorSwatch Id="MenuNormalTextColorSwatch" Label="Normal Item Text" LabelWidth="124" QuickHelpTitle="TourStyleMenuNormalText" runat="server" Col="true" ForPreview="true" />
			<AvantLogic:ColorSwatch Id="MenuHoverTextColorSwatch" Label="Hover Item Text" QuickHelpTitle="TourStyleMenuHoverText" runat="server" Col="true" ForPreview="true" />
		</tr>
		<tr>
			<AvantLogic:ColorSwatch Id="MenuNormalBackgroundColorSwatch" Label="Normal Tab" QuickHelpTitle="TourStyleMenuNormalBackground" runat="server" Col="true" ForPreview="true" />
			<AvantLogic:ColorSwatch Id="MenuHoverBackgroundColorSwatch" Label="Hover Tab" QuickHelpTitle="TourStyleMenuHoverBackground" runat="server" Col="true" ForPreview="true" />
		</tr>
		<tr>
			<AvantLogic:ColorSwatch Id="MenuSelectedTextColorSwatch" Label="Selected Item Text" QuickHelpTitle="TourStyleMenuSelectedText" runat="server" Col="true" ForPreview="true" />
			<AvantLogic:ColorSwatch Id="MenuBackgroundColorSwatch" Label="Menu Background" LabelWidth="124" QuickHelpTitle="TourStyleMenuBackground" runat="server" Col="true" ForPreview="true" />
		</tr>
		<tr>
			<AvantLogic:ColorSwatch Id="MenuSelectedBackgroundColorSwatch" Label="Selected Tab" QuickHelpTitle="TourStyleMenuSelectedBackground" runat="server" Col="true" ForPreview="true" />
			<AvantLogic:ColorSwatch Id="MenuLineColorSwatch" Label="Navigation Accent" QuickHelpTitle="TourStyleMenuLine" runat="server" Col="true" ForPreview="true" />
		</tr>
	</table>

	<asp:Panel ID="LayoutPreviewImagePanel" runat="server" style="margin-top:8px;">
		<div id="PreviewMessage" class="optionsSectionTitle">Preview</div>
		
		<asp:Panel ID="PreviewMsgPanel" runat="server" class="colorSchemePreviewMessage">
			<asp:Label ID="PreviewMsg" runat="server" />
			<AvantLogic:MemberPageActionButton ID="SwitchColorSchemeControl" runat="server" />
		</asp:Panel>

		<div class="finePrintHelp" style="margin-bottom:12px;">
		This preview is an approximation. For more accuracy, use Tour Preview.
		</div>
		
		<asp:Image ID="PagePreviewImage" runat="server" />
	</asp:Panel>
</asp:Content>

