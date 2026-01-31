<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="MapSetup.aspx.cs"
	Inherits="Members_MapSetup"
	Title="<%$ Resources:Text, PageTitlePage %>"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<script type="text/javascript">
	function maChooseLayout(layout)
	{
		maChangeDetected();			
		if (layout == 1)
			document.getElementById("<%= RadioButtonTiled.ClientID %>").checked = true;
		else if (layout == 2)
			document.getElementById("<%= RadioButtonPopup.ClientID %>").checked = true;
	}
	function maOnToggleOptions()
	{
		var e = document.getElementById("<%= RadioButtonSetup.ClientID %>");
		var showSetupOptions = e.checked;	
		document.getElementById("<%= CopyMapPanel.ClientID %>").style.display = !showSetupOptions ? "block" : "none";	
		document.getElementById("<%= SetupOptionsPanel.ClientID %>").style.display = showSetupOptions ? "block" : "none";	
	}
	</script>

	<AvantLogic:QuickHelpTitle ID="TourPageName" runat="server" Title="<%$ Resources:Text, PageNameLabel %>" TopMargin="0px" />
	<asp:TextBox ID="TourPageNameTextBox" runat="server"></asp:TextBox>
	<asp:Label ID="TourPageNameError" runat="server" CssClass="textErrorMessage" />
	
	<asp:Panel ID="NewMapOptionsPanel" style="margin-top:16px;margin-bottom:16px;" class="textNormal" runat="server">
		<AvantLogic:QuickHelpTitle ID="NewMapOptions" runat="server" Title="What settings do you want to use?" TopMargin="0px" />
		<div style="margin-top:4px;">
			<asp:RadioButton ID="RadioButtonSetup" runat="server" GroupName="Option" Text="Default settings" OnClick="maOnToggleOptions();" />
		</div>
		<div style="margin-top: 4px;">
			<asp:RadioButton ID="RadioButtonCopy" runat="server" GroupName="Option" OnClick="maOnToggleOptions();" />
		</div>
	</asp:Panel>

	<asp:Panel ID="CopyMapPanel" runat="server" style="display:none;margin-bottom:16px;">
		<AvantLogic:QuickHelpTitle ID="NewMapLikeThis" runat="server" Title="Copy settings from:" TopMargin="0px" />
		<asp:DropDownList ID="PageDropDownList" runat="server" style="margin-top:4px;" />
	</asp:Panel>
	
	<asp:Panel ID="SetupOptionsPanel" runat="server" class="textNormal" style="margin-top:16px;">
		<asp:Panel ID="LayoutOptionsPanel" runat="server">
            <AvantLogic:QuickHelpTitle ID="NewMapAppearance" runat="server" Title="Choose how content will appear" TopMargin="0px" />
		    <table>
			    <tr>
				    <td style="vertical-align:top;">
					    <div style="margin-top:2px;">
						    <asp:RadioButton ID="RadioButtonPopup" runat="server" GroupName="PageType" Text="Popup<span style='color:#999;'> (content pops up over map)</span>" />
					    </div>
					    <div style="margin-left:20px; margin-top: 12px;">
						    <asp:Image ID="ImagePopup" runat="server" onclick="maChooseLayout(2);" />
					    </div>
				    </td>
				    <td style="vertical-align:top;padding-left:20px;">
					    <div style="margin-top:2px;">
						    <asp:RadioButton ID="RadioButtonTiled" runat="server" GroupName="PageType" Text="Tiled<span style='color:#999;'> (content next to map)</span>" />
					    </div>
					    <div style="margin-left:20px; margin-top:12px;">
						    <asp:Image ID="ImageTiled" runat="server" onclick="maChooseLayout(1);" />
					    </div>
				    </td>
			    </tr>
		    </table>	
        </asp:Panel>

		<asp:Panel ID="MapZoomPanel" runat="server">
			<div class="checkboxOptionFirst" style="margin-top:8px;">
				<asp:CheckBox ID="MapCanZoomCheckBox" runat="server" />
				<AvantLogic:QuickHelpTitle ID="MapCanZoom" runat="server" Title="<%$ Resources:Text, MapCanZoomLabel %>" Span="true" />
			</div>
		</asp:Panel>
		
		<asp:Panel ID="FirstHotspotPanel" style="margin-top:16px;" runat="server">
			<AvantLogic:QuickHelpTitle ID="FirstHotspot" runat="server" Title="First Hotspot" />
			<telerik:RadComboBox 
				id="FirstHotspotComboBox" 
				Runat="server"
				AutoPostBack="false"
				OnClientSelectedIndexChanged="maChangeDetected"
				Skin="Default"
				Width="300"
				> 
				<ExpandAnimation Type="None" />
				<CollapseAnimation Type="None" />
			</telerik:RadComboBox>
		</asp:Panel>
	</asp:Panel>

	<asp:Panel ID="ButtonPanel" style="margin-top:24px;" runat="server">
		<asp:Button ID="ButtonContinue" Text="Create Map" runat="server" OnClientClick="maContinueButtonClicked(this, 'WaitMessage');" OnClick="OnAddMap" />
		<div id="WaitMessage" style="display:none;">Creating a map. Please wait...</div>
	</asp:Panel>
</asp:Content>

