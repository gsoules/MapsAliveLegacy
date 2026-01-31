<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="TourSetup.aspx.cs"
	Inherits="Members_TourSetup"
	Title="MapsAlive Tours"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<script type="text/javascript">
	function maColorSchemeChanged()
	{
		maChangeDetected();
		maTourResourceSelectionChanged("<%= ColorSchemeComboBox.ClientID %>", "<%= EditColorSchemeControl.ClientID %>", "EditColorScheme.aspx");
	}
    function maShowWebSitePageTypes()
    {
        maChangeDetected();
        let webSiteRadioButton = document.getElementById(formContentId + "RadioButtonTypeWebSite");
        document.getElementById(formContentId + "WebSiteOptionsPanel").style.display = webSiteRadioButton.checked ? "block" : "none";
    }
    </script>

	<AvantLogic:QuickHelpTitle ID="TourName" runat="server" Title="<%$ Resources:Text, TourNameLabel %>" TopMargin="0px" />
	<asp:TextBox ID="TourNameTextBox" runat="server" Width="200px"></asp:TextBox>
	<asp:Label ID="TourNameError" runat="server" CssClass="textErrorMessage" />
	
	<asp:Panel ID="V4RadioButtons" runat="server">
        <div class="textNormal" style="margin-top:20px;margin-bottom:16px;color:#333;">
            <AvantLogic:QuickHelpTitle ID="TourType" runat="server" Title="Tour Type" TopMargin="0px" />
		    <div style="margin-top:2px;">
			    <asp:RadioButton ID="RadioButtonTypeFlexMap" runat="server" GroupName="TourType" />
		    </div>
		    <div style="margin-top:2px;">
			    <asp:RadioButton ID="RadioButtonTypeWebSite" runat="server" GroupName="TourType" />
		    </div>
	    </div>
	</asp:Panel>

    <asp:Panel ID="WebSiteOptionsPanel" runat="server" Visible="false">
        <AvantLogic:QuickHelpTitle ID="TourTypePage" runat="server" Title="Page Type for Classic tour" TopMargin="0px" />
		<div style="margin-top:2px;">
			<asp:RadioButton ID="RadioButtonWebSiteMap" runat="server" GroupName="PageType" />
		</div>
		<div style="margin-top:2px;">
			<asp:RadioButton ID="RadioButtonWebSiteGallery" runat="server" GroupName="PageType" />
		</div>
		<div style="margin-top:2px;">
			<asp:RadioButton ID="RadioButtonWebSiteDataSheet" runat="server" GroupName="PageType" />
		</div>
    </asp:Panel>

    <div class="textLarge">
        <asp:Label ID="FlexMapDisqualifierList" runat="server" Visible="false"></asp:Label>
    </div>

	<div class="textNormal" style="margin-top:20px;">
		<asp:Panel ID="ColorSchemePanel" runat="server">
            <div style="margin-top:16px;">
			    <AvantLogic:QuickHelpTitle ID="ColorSchemeOptionNew" runat="server" Title="Color Scheme" Span="true" />
		    </div>
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
        </asp:Panel>
		
		<asp:Panel ID="ButtonPanel" style="margin-top:24px;" runat="server">
		    <asp:Button ID="ButtonContinue" Text="Create Tour" runat="server" OnClientClick="maContinueButtonClicked(this, 'WaitMessage');" OnClick="OnCreateTour" />
		    <div id="WaitMessage" style="display:none;">Creating a new tour. Please wait...</div>
		</asp:Panel>
 	</div>
</asp:Content>

