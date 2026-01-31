<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="UploadMap.aspx.cs"
	Inherits="Members_UploadMap"
%>

<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<script language="javascript">
	var mapImageWidth;
	var mapImageHeight;
	function maUploadMapOnLoad()
	{
		var e = document.getElementById("maSampleImage");
		if (e) e.style.visibility = "visible";
	}

	window.addEventListener('load', maUploadMapOnLoad, false);

	function onNodeClicking(sender, args)
    {
		var node = args.get_node();
		node.toggle();
		if (node.get_nodes().get_count())
			maReadyMapsCollapseSiblings(node);
		
		var value = node.get_value();
		if (value === null)
		{
            args.set_cancel(true);
            return;
		}

        // This code no longer calls window.confirm to ask the user if they want to choose the map they
        // just clicked. Instead, the map gets chosen and the page posts to the server. Attempts to replace
        // window.confirm with the much nicer maAwaitConfirm were unsuccessful due to the fact that the server-side
        // OnUploadReadyMap method needs access to the RadTreeNode in order to extract the file path for the
        // chosen ReadyMap. See the comments in maAwaitConfirm to better understand the issues with Telerik controls.
        // If it turns out that the confirm is necessary, uncomment the code below.  

        /*
        if (!confirm("Press OK to choose " + node.get_parent().get_text() + " " + node.get_text()))
            args.set_cancel(true);
        */
    }
    function maWaitForRefresh()
    {
		maShowBusyMsg("Please wait...", "memberPageControlsStatusMessage", true);
		var e = document.getElementById("<%= UploadFilePanel.ClientID %>");
		if (!e)
			e = document.getElementById("<%= ReadyMapsPanel.ClientID %>");
		if (e)
			e.style.display = "none"
    }
    </script> 
	
	<asp:Panel ID="ChoicesPanel" runat="server" style="margin-bottom:16px;" class="textNormal">
        <AvantLogic:QuickHelpTitle ID="ChooseMapChoice" runat="server" Title="Where is your map image?" TopMargin="0px" />
		
		<div style="margin-top:4px;">
			<asp:RadioButton ID="RadioButtonUploadFile" runat="server" AutoPostBack="true" GroupName="Option" Text="My map is in a file located on my computer" onclick="maWaitForRefresh();" />
		</div>
		
		<div style="margin-top: 4px;">
			<asp:RadioButton ID="RadioButtonReadyMap" runat="server" AutoPostBack="true" GroupName="Option" Text="I want to use a Ready Map" onclick="maWaitForRefresh();" />
		</div>
	</asp:Panel>

	<asp:Panel ID="UploadFilePanel" runat="server" Visible="false">
		<asp:Panel ID="UploadFilePanelSectionTitle" runat="server" class="optionsSectionTitle">Upload an Image File&nbsp;<AvantLogic:QuickHelpTitle ID="UploadMapImage" runat="server" Span="true"/></asp:Panel>
		<div runat="server" class="textHelp">Browse for the file containing the image you want to upload. Then click the Load button.</div>
		<input type="file" runat="server" id="FileInput" style="height:22px;" onkeypress="return maStopEvent(event);"/>
		<asp:Button ID="ButtonUpload" runat="server" Enabled="true" Text="<%$ Resources:Text, ButtonUpload %>" OnClientClick="maOnEventUploadFile();return false;" CausesValidation="false" Height="22px" />
	</asp:Panel>
	
	<asp:Panel ID="ReadyMapsPanel" runat="server" Visible="false">
		<div class="optionsSectionTitle">
			Choose a Ready Map
			<AvantLogic:QuickHelpTitle ID="ImportReadyMap" runat="server" Span="true"/>
		</div>
		<div runat="server" class="textNormal" style="margin-bottom:8px;">
			Navigate the map folders below by clicking the triangles. <b>Click a map name to choose it.</b>
			<div style="margin-top:4px;font-weight:bold;">
				<asp:Label ID="HelpMessage" runat="server" Text="Choose the map you want by clicking its name." />
			</div>
		</div>
		<telerik:RadTreeView ID="ReadyMapsTree"	runat="server" OnNodeClick="OnUploadReadyMap" />
	</asp:Panel>
	
	<asp:Panel ID="ReadyMapsMessagePanel" runat="server" class="memberPageSpecialNotice" style="margin-top:8px;" Visible="false">
		<asp:Label ID="ReadyMapsMessage" runat="server" Text="Label" />
	</asp:Panel>
	
	<telerik:RadProgressManager id="RadProgressManager" runat="server" />
	<telerik:RadProgressArea id="ProgressArea" runat="server" />

	<%-- Map Image --%>
	<asp:Panel ID="ImagePreviewPanel" runat="server">
		<div class="optionsSectionTitle"><asp:Label ID="PreviewLabel" runat="server" Text="Label">Map Preview</asp:Label></div>
		<asp:Panel ID="ImageDiv" runat="server" CssClass="uploadedImage">
			<asp:Panel ID="MapPreviewPanel" runat="server">
				<asp:Image ID="MapPreviewImage" runat="server" />
			</asp:Panel>
			<asp:Label ID="FileName" runat="server" CssClass="textUploadFileName"></asp:Label>
			<table class="finePrintHelp" style="margin-top:12px;" cellpadding="0" cellspacing="0">
				<tr ID="ScaleSizeMessage" runat="server">
					<td align="right" style="padding-bottom:4px;">Scaled size:&nbsp;</td><td><asp:Label ID="ImageSizeScaled" runat="server"/></td>
				</tr>
				<tr>
					<td align="right">Uploaded size:&nbsp;</td><td><asp:Label ID="ImageSizeActual" runat="server"/></td>
				</tr>
			</table>
			
			<asp:Panel ID="RemoveMapPanel" runat="server" class="finePrint" style="margin-top:8px;">
				<asp:LinkButton ID="ButtonRemove" runat="server" Enabled="true" Text="Remove map image" OnClientClick="maOnEventRemoveMapImage();return false;" CausesValidation="false" />
			</asp:Panel>
		</asp:Panel>
	</asp:Panel>
	
</asp:Content>

