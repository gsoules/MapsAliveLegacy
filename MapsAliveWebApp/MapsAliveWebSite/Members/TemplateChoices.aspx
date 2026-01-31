<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="TemplateChoices.aspx.cs"
	Inherits="Members_TemplateChoices"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>
<%@ Register Assembly="App_Code\ServerControls\SlideThumbs.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<asp:Panel ID="OptionsPanel" runat="server" Visible="false" style="margin-bottom:16px;">
		<div style="padding-left:3px;margin-bottom:8px;">
			<asp:Label ID="OptionsPanelText" runat="server" CssClass="textNormal" style="color:#345a91;"/>
		</div>
		<AvantLogic:MemberPageActionButton ID="ToggleTemplateChoices" runat="server" />
	</asp:Panel>
	
	<AvantLogic:SlideThumbs
		ID="SlideThumbs"
		runat="server"
		ColumnsPerRow="4" />

	<asp:HiddenField ID="PatternId" runat="server" Value="0" />
</asp:Content>

