<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="ImportMarkerShapes.aspx.cs"
	Inherits="Members_ImportMarkerShapes"
	ValidateRequest="false"
	Title=""
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
    <script language="javascript">                
    function onNodeClicking(sender, args)
    {
		var node = args.get_node();
		var value = node.get_value();
		if (value == null)
			node.toggle();
		if (value === null || !confirm("Press OK to import " + node.get_parent().get_text() + " " + node.get_text()))
			args.set_cancel(true);
    }
    </script> 
	<asp:Panel ID="AllPanels" runat="server" class="textNormal">
		<asp:Panel ID="ChoicesPanel" runat="server" style="margin-bottom:12px;">
			<AvantLogic:QuickHelpTitle ID="ImportShapesChoice" runat="server" Title="What would you like to import?" TopMargin="0px" />
			
			<div style="margin-top:4px;">
				<asp:RadioButton ID="RadioButtonImportFile" runat="server" AutoPostBack="true" GroupName="Option" Text="A file from my computer" />
			</div>
			
			<div style="margin-top: 4px;">
				<asp:RadioButton ID="RadioButtonImportReadyMap" runat="server" AutoPostBack="true" GroupName="Option" Text="Ready Map shapes" />
			</div>
		</asp:Panel>

		<asp:Panel ID="MarkerStyleSectionTitlePanel" runat="server" class="optionsSectionTitle">Style for New Markers</asp:Panel>
		<AvantLogic:QuickHelpTitle ID="ImportHtmlMarker" runat="server" Title="Marker Style" TopMargin="4px" />
		<uc:TourResourceComboBox Id="MarkerStyleSelector" runat="server" />
		
		<asp:Panel ID="ImportFilePanel" runat="server" class="textNormal">
			<asp:Panel ID="ImportFilePanelSectionTitle" runat="server" class="optionsSectionTitle">Import Shapes from a File</asp:Panel>
			<uc:UploadFile Id="UploadFileControl" runat="server" />
		</asp:Panel>

		<asp:Panel ID="ReadyMapsPanel" runat="server">
			<div class="optionsSectionTitle">Choose Shapes for <asp:Label ID="MapDescription" runat="server"/>
				<AvantLogic:QuickHelpTitle ID="ImportReadyMapShapes" runat="server" Span="true"/>
			</div>
			<div runat="server" class="textNormal" style="margin-bottom:8px;">
				The shapes available for this map are highlighted below in yellow.
				<div style="margin-top:4px;"><b>Click the shapes you want to import.</b></div>
			</div>
			<telerik:RadTreeView ID="ReadyMapsTree"	runat="server" OnNodeClick="OnUploadReadyMapShapes" />
			<telerik:RadProgressManager id="RadProgressManager" runat="server" />
			<telerik:RadProgressArea id="ProgressArea" runat="server" />
		</asp:Panel>

		<asp:Panel ID="FileImportOptionsPanel" runat="server">
			<div class="optionsSectionTitle">Import Options</div>
		
			<div class="checkboxOption">
				<asp:CheckBox ID="TreatPolygonsAsLinesCheckBox" runat="server" />
				<AvantLogic:QuickHelpTitle ID="TreatPolygonsAsLines" runat="server" Title="<%$ Resources:Text, TreatPolygonsAsLines %>" Span="true" />
			</div>
		
			<div class="checkboxOption">
				<asp:CheckBox ID="UseTitleForTooltipCheckBox" runat="server" Checked="true" />
				<AvantLogic:QuickHelpTitle ID="UseTitleForTooltip" runat="server" Span="true" Title="<%$ Resources:Text, UseTitleAsTooltipLabel %>" />
			</div>
		
			<div style="margin-left:0px;margin-top:12px;font-size:10px;">
				<AvantLogic:QuickHelpTitle ID="HrefAttributeOption" runat="server" Span="true" Title="<%$ Resources:Text, HrefAttributeOptionLabel %>" />

				<div style="margin-top:2px;">
					<asp:RadioButton ID="IgnoreHref" runat="server" Text="Ignore Href attribute" GroupName="Href" />
				</div>
				<div style="margin-top: 2px;">
					<asp:RadioButton ID="LinkToUrl" runat="server" Text="Set click action to link to href URL" GroupName="Href" />
				</div>
				<div style="margin-top: 2px;">
					<asp:RadioButton ID="LinkToUrlInNewWindow" runat="server" Text="Set click action to link to href URL in new window" GroupName="Href" />
				</div>
			</div>
		</asp:Panel>
		
		<uc:ImportReportTable Id="ImportReportTable" runat="server" />
	</asp:Panel>
</asp:Content>

