<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="CanvasMarginsAndSpacing.aspx.cs"
	Inherits="Members_CanvasMarginsAndSpacing"
%>
<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<uc:MarginsAndSpacing ID="MarginsAndSpacingControl" runat="server" />
	
	<div id="PreviewMessage" class="optionsSectionTitle">Preview</div>
	
	<div class="finePrintHelp" style="margin-bottom:12px;">
	This preview is an approximation. For more accuracy, use Tour Preview.
	</div>

	<asp:Panel runat="server" style="margin-top:8px;">
		<asp:Image ID="LayoutPreviewImage" runat="server" />
	</asp:Panel>
</asp:Content>

