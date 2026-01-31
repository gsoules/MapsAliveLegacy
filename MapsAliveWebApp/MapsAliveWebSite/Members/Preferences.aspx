<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="Preferences.aspx.cs"
	Inherits="Members_Preferences"
	Title=""
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
    <table>
        <tr>
            <td align="Right">
                <asp:Label runat="server" CssClass="controlLabel" Text="Tour Builder Title"></asp:Label>
            </td>
            <td>
                <asp:TextBox ID="SiteNameTextBox" runat="server" Width="260px" />
                <AvantLogic:QuickHelpTitle ID="SiteName" runat="server" Span="true" />
            </td>
        </tr>
    </table>
    
	<div class="checkboxOption">
		<asp:CheckBox ID="PreviewSlideContentCheckBox" runat="server" />
		<AvantLogic:QuickHelpTitle ID="ShowSlideContentInLayoutPreview" runat="server" Title="Show Hotspot Content in Layout Previews" Span="true" />
	</div>
    
	<div class="checkboxOption">
		<asp:CheckBox ID="ShowTourNavigatorExpandedCheckBox" runat="server" />
		<AvantLogic:QuickHelpTitle ID="ShowTourNavigatorExpanded" runat="server" Title="Show Tour Navigator Expanded" Span="true" />
	</div>
     
	<div class="checkboxOption">
		<asp:CheckBox ID="DisableTourAdvisorCheckBox" runat="server" />
		<AvantLogic:QuickHelpTitle ID="DisableTourAdvisor" runat="server" Title="Turn Off Tour Advisor" Span="true" />
	</div>
     
	<div class="checkboxOption">
		<asp:CheckBox ID="ShowStepByStepHelpCheckBox" runat="server" />
		<AvantLogic:QuickHelpTitle ID="ShowStepByStepHelpPreference" runat="server" Title="Show Instructions" Span="true" />
	</div>
   
   	<div class="optionsSectionTitle">Resource Defaults</div>
	
	<div style="margin-bottom:2px;">
		<AvantLogic:QuickHelpTitle ID="DefaultColorScheme" runat="server" Title="Default Color Scheme" TopMargin="0px" />
		<uc:TourResourceComboBox Id="ColorSchemeComboBox" runat="server" />
	</div>
	<div style="margin-bottom:2px;">
		<AvantLogic:QuickHelpTitle ID="DefaultTooltipStyle" runat="server" Title="Default Tooltip Style" />
		<uc:TourResourceComboBox Id="TooltipStyleComboBox" runat="server" />
	</div>
	<div style="margin-bottom:2px;">
		<AvantLogic:QuickHelpTitle ID="DefaultMarker" runat="server" Title="Default Marker" />
		<uc:TourResourceComboBox Id="MarkerComboBox" runat="server" />
	</div>
	<div style="margin-bottom:2px;">
		<AvantLogic:QuickHelpTitle ID="DefaultMarkerStyle" runat="server" Title="Default Marker Style" />
		<uc:TourResourceComboBox Id="MarkerStyleComboBox" runat="server" />
	</div>
	<div style="margin-bottom:2px;">
		<AvantLogic:QuickHelpTitle ID="DefaultFontStyle" runat="server" Title="Default Font Style" />
		<uc:TourResourceComboBox Id="FontStyleComboBox" runat="server" />
	</div>
	<div style="margin-bottom:2px;">
		<AvantLogic:QuickHelpTitle ID="DefaultSymbol" runat="server" Title="Default Symbol" />
		<uc:TourResourceComboBox Id="SymbolComboBox" runat="server" />
	</div>
	
	<div style="margin-top:16px;">
		<AvantLogic:MemberPageActionButton ID="ImportSystemResourceControl" Title="Import MapsAlive Resources" runat="server" />
		<AvantLogic:QuickHelpTitle ID="ImportSystemResources" runat="server" Span="true" />
	</div>
</asp:Content>
