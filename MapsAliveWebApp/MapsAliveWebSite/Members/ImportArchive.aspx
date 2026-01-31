<%@
Page Language="C#"
MasterPageFile="~/Masters/MemberPage.master"
AutoEventWireup="true"
CodeFile="ImportArchive.aspx.cs"
Inherits="Members_ImportArchive"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<asp:Panel ID="AllPanels" runat="server" class="textNormal">
		<asp:Panel ID="ChoicesPanel" runat="server" style="margin-bottom:16px;">
			<AvantLogic:QuickHelpTitle ID="ChooseImportArchiveChoice" runat="server" Title="Where is your archive file?" TopMargin="0px" />
			
			<div style="margin-top:4px;">
				<asp:RadioButton ID="RadioButtonUploadFile" runat="server" AutoPostBack="true" GroupName="Option" Text="In a file on my computer" Checked="true" />
			</div>
			
			<div style="margin-top: 4px;">
				<asp:RadioButton ID="RadioButtonRemoteUrl" runat="server" AutoPostBack="true" GroupName="Option" Text="At a URL on the web" />
			</div>
		</asp:Panel>

		<asp:Panel ID="UploadFilePanel" runat="server">
			<div class="optionsSectionTitle">Upload an Archive File</div>
			<div runat="server" class="textHelp">Browse for the file containing the archive file you want to upload. Then click the Import button.</div>
			<input type="file" runat="server" id="FileInput" style="height:22px;" onkeypress="return maStopEvent(event);"/>
			<asp:Button ID="ButtonUpload" runat="server" Enabled="true" Text="Import" OnClientClick="maOnEventUploadFile();return false;" CausesValidation="false" Height="22px" />
			<div>
				<asp:Label ID="FileName" runat="server" style="color:Green;font-size:9px;" />
			</div>
		</asp:Panel>

		<asp:Panel ID="RemoteUrlPanel" runat="server">
			<div class="optionsSectionTitle">Get Archive from a URL</div>
			<div runat="server" class="textHelp">Type in the URL for the location of a remote archive. Then click the Import button.</div>
			<asp:TextBox ID="RemoteImportTextBox" runat="server" Width="570" style="margin-bottom:8px;"/>
			<asp:Button ID="RemoteImportButton" runat="server" Text="Import" />
		</asp:Panel>
	
		<telerik:RadProgressManager id="RadProgressManager" runat="server" />
		<telerik:RadProgressArea id="ProgressArea" runat="server" />
		
		<uc:ImportReportTable Id="ImportReportTable" runat="server" />
	</asp:Panel>
</asp:Content>

