<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="BannerOptions.aspx.cs"
	Inherits="Members_BannerOptions"
	Trace="false"
	TraceMode="SortByCategory"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<asp:Panel ID="RemoveBannerPanel" runat="server" style="margin-top:8px;">
		<asp:Label ID="NavButtonInBannerMessage" runat="server" Visible="false">This tour's nav button is located in the banner. To remove the banner, change the nav button location and return to this screen.</asp:Label>
        <asp:LinkButton ID="ButtonRemove" runat="server" Enabled="true" Text="Remove banner" OnClientClick="maOnEventRemoveBannerImage();return false;" CausesValidation="false" CssClass="pageJavaScriptControl" />
		<AvantLogic:QuickHelpTitle ID="RemoveBanner" runat="server" Span="true" />
	</asp:Panel>

	<AvantLogic:QuickHelpTitle ID="BannerImage" runat="server" Title="<%$ Resources:Text, BannerImageLabel %>" />
	
	<input type="file" runat="server" id="FileInput" style="height:22px;" onkeypress="return maStopEvent(event);"/>
	<asp:Button ID="ButtonUpload" runat="server" Enabled="true" Text="<%$ Resources:Text, ButtonUpload %>" OnClientClick="maOnEventUploadFile();return false;" CausesValidation="false" Height="22px" />

	<telerik:RadProgressManager id="RadProgressManager" runat="server" />
	<telerik:RadProgressArea id="ProgressArea" runat="server" />

	<asp:Panel ID="FileInfoPanel" runat="server" style="margin-top:4px;margin-bottom:16px;">
		<table class="finePrintHelp" style="margin-bottom:8px;">
			<tr><td align="right">File name:</td><td><asp:Label ID="FileName" runat="server" CssClass="textUploadFileName"/></td></tr>
			<tr><td align="right">Banner area size:</td><td><asp:Label ID="ImageSizeScaled" runat="server"/></td></tr>
			<tr><td align="right">Original image size:</td><td><asp:Label ID="ImageSizeActual" runat="server"/></td></tr>
		</table>
		<asp:Image ID="ImageElement" runat="server" Visible="false" />
	</asp:Panel>

	<asp:Panel ID="OptionsPanel" runat="server">
		<AvantLogic:QuickHelpTitle ID="BannerUrlTitle" runat="server" Title="<%$ Resources:Text, BannerTitleLabel %>" />
		<asp:TextBox ID="BannerUrlTitleTextBox" runat="server" Width="570"></asp:TextBox>
		
		<AvantLogic:QuickHelpTitle ID="BannerUrl" runat="server" Title="<%$ Resources:Text, BannerUrlLabel %>" />
		<asp:TextBox ID="BannerUrlTextBox" Width="570" runat="server"></asp:TextBox>

		<div class="checkboxOptionFirst">
			<asp:CheckBox ID="BannerUrlOpensWindowCheckBox" runat="server" />
			<AvantLogic:QuickHelpTitle ID="BannerUrlOpensWindow" runat="server" Title="<%$ Resources:Text, BannerUrlOpensWindowLabel %>" Span="true"  />
		</div>
	</asp:Panel>
	
	<asp:Panel ID="LayoutPreviewImagePanel" runat="server">
		<div class="optionsSectionTitle">Preview</div>

		<div class="finePrintHelp" style="margin-bottom:12px;">
		This preview is an approximation. For more accuracy, use Tour Preview.
		</div>

		<asp:Panel runat="server" style="margin-top:8px;">
			<asp:Image ID="LayoutPreviewImage" runat="server" />
		</asp:Panel>
	</asp:Panel>
</asp:Content>
