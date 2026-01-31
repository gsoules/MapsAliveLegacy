<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="ImportHotspotContent.aspx.cs"
	Inherits="Members_ImportHotspotContent"
	Title=""
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<asp:Panel ID="AllPanels" runat="server" class="textNormal">
		<asp:Panel ID="ChoicesPanel" runat="server" style="margin-bottom:16px;">
			<AvantLogic:QuickHelpTitle ID="ChooseImportContentChoice" runat="server" Title="Where is your content file?" TopMargin="0px" />
			
			<div style="margin-top:4px;">
				<asp:RadioButton ID="RadioButtonUploadFile" runat="server" AutoPostBack="true" GroupName="Option" Text="In a file on my computer" Checked="true" />
			</div>
			
			<div style="margin-top: 4px;">
				<asp:RadioButton ID="RadioButtonRemoteUrl" runat="server" AutoPostBack="true" GroupName="Option" Text="At a URL on the web" />
			</div>
		</asp:Panel>

		<asp:Panel ID="UploadFilePanel" runat="server">
			<div class="optionsSectionTitle">Upload a Content File</div>
			<div id="Div1" runat="server" class="textHelp">Browse for the file containing the content you want to upload. Then click the Import button.</div>
			<input type="file" runat="server" id="FileInput" style="height:22px;" onkeypress="return maStopEvent(event);"/>
			<asp:Button ID="ButtonUpload" runat="server" Enabled="true" Text="Import" OnClientClick="maOnEventUploadFile();return false;" CausesValidation="false" Height="22px" />
			<div>
				<asp:Label ID="FileName" runat="server" style="color:Green;font-size:9px;" />
			</div>
		</asp:Panel>
		
		<asp:Panel ID="RemoteUrlPanel" runat="server">
			<div class="optionsSectionTitle">Get Content from a URL</div>
			<div runat="server" class="textHelp">Type in the URL for the location of your remote content. Then click the Import button.<br />To learn about using remote import, see the <i>MapsAlive User Guide for Content Management</i>.</div>
			<asp:TextBox ID="RemoteImportTextBox" runat="server" Width="570"/>
			<asp:Button ID="RemoteImportButton" runat="server" Text="Import" />
		</asp:Panel>
	
		<telerik:RadProgressManager id="RadProgressManager" runat="server" />
		<telerik:RadProgressArea id="ProgressArea" runat="server" />

		<asp:Panel ID="OptionsPanel" runat="server">
			<div class="optionsSectionTitle">Import Options</div>
			<AvantLogic:QuickHelpTitle ID="ImportedSlideMarker" runat="server" Title="Default marker" TopMargin="4px" />
			<uc:TourResourceComboBox Id="MarkerComboBox" runat="server" />

            <div class="checkboxOption" style="margin-top:16px;">
	        	<asp:CheckBox ID="ImportMarkerLocations" runat="server" Checked="false" />
		        <AvantLogic:QuickHelpTitle ID="ImportMarkerLocation" runat="server" Title="Place markers on map" Span="true" />
        	</div>
		</asp:Panel>
		
		<uc:ImportReportTable Id="ImportReportTable" runat="server" />
	</asp:Panel>
</asp:Content>

