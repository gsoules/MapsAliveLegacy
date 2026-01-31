<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="EditHotspotContent.aspx.cs"
	Inherits="Members_EditHotspotContent"
	ValidateRequest="false"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<script language="javascript">   
    maShowBusyMsg("Loading hotspot content...", "memberPageControlsStatusMessage", true);

    tinymce.init({
        selector: `#${formContentId}HtmlEditor`,
        content_css: '../Styles/Editor.css',
        placeholder: 'Enter hotspot text here',
        min_height:200,
        removed_menuitems: 'newdocument visualaid',
        plugins: 'code paste autoresize link lists charmap media image charmap hr',
        paste_enable_default_filters: true,
        forced_root_block: false,
        fontsize_formats: "8px 9px 10px 11px 12px 13px 14px 15px 16px 18px 20px 22px 24px 28px 32px 36px",
        branding: false,
        resize: true,
        autoresize_bottom_margin: 8,
        menubar: 'format insert edit',
        toolbar: 'bold italic underline | fontselect fontsizeselect | forecolor backcolor | link unlink | bullist numlist | code',
        setup: function (editor)  {
            editor.on('Change', function (e) {
                maChangeDetected();
           });
        }
    });

	function maAllowEnter(e)
	{
		if (window.event)
			window.event.cancelBubble = true;
		else
			e.stopPropagation();
		return true;
	}
	function maOnLoadEditor(editor)
	{
		maEditor = editor;
    }
	function maOnClientSelectionChange(sender, args)
	{
        maChangeDetected();
    }
	function maShowImage(id)
	{
		var e = document.getElementById(id);
		if (e) e.style.visibility = "visible";
	}
	function maSlideContentOnLoad()
	{
		var e = document.getElementById("StatusBox");
		if (e && e.style.visibility != "hidden")
			maShowImage("maSampleImage");
		maShowImage(formContentId + "ImageElement");
		maShowImage(formContentId + "MapImageElement");
	}
	function maMarkerChanged()
	{
		maTourResourceSelectionChanged("<%= MarkerComboBox.ClientID %>", "<%= EditMarkerControl.ClientID %>", "EditMarker.aspx");
	}
	
	function getFileInputElementsOnPageWithValues() 
    { 
        var fileInputElements = []; 
        var inputElements = document.getElementsByTagName("input"); 
        for (var i = 0; i < inputElements.length; i++) 
        { 
            if (inputElements[i].type.toLowerCase() == "file" && inputElements[i].value) 
            { 
                fileInputElements[fileInputElements.length] = inputElements[i]; 
            } 
        } 
        return fileInputElements; 
    } 
 
    function onClientProgressUpdating(sender, args) 
    { 
        // Circumvent the problem described in OnTime task #1099.
        if (getFileInputElementsOnPageWithValues().length == 0) 
        { 
            args.set_cancel(true); 
        } 
    }
     
	window.addEventListener('load', maSlideContentOnLoad, false);
    </script> 
		
	<div style="position:relative;">
		<asp:Panel ID="DataSheetNamePanel" runat="server" Visible="false" style="margin-bottom:12px;">
			<AvantLogic:QuickHelpTitle ID="DataSheetName" runat="server" Title="Data Sheet Name" TopMargin="0px" />
			<asp:TextBox ID="DataSheetNameTextBox" runat="server"></asp:TextBox>
			<asp:Label ID="DataSheetNameError" runat="server" CssClass="textErrorMessage" />
		</asp:Panel>

		<table cellpadding="0" cellspacing="0" style="margin-top:12px;margin-bottom:16px;">
			<tr>
				<td>
					<AvantLogic:QuickHelpTitle ID="TourViewTitle" runat="server" Title="Title" TopMargin="0px" />
					<asp:TextBox ID="TourViewTitleTextBox" Width="300" runat="server"></asp:TextBox>
					<asp:Label ID="TourViewTitleError" runat="server" CssClass="textErrorMessage" />
				</td>
				<td style="padding-left:24px;">
					<AvantLogic:QuickHelpTitle ID="TourViewId" runat="server" Title="Hotspot&nbsp;Id" TopMargin="0px" />
					<asp:TextBox ID="SlideIdTextBox" Width="200" runat="server"></asp:TextBox>
					<asp:Label ID="SlideIdError" runat="server" CssClass="textErrorMessage" />
				</td>
			</tr>
		</table>

		<asp:Panel ID="MarkerPanel" runat="server" style="margin-top:12px;margin-bottom:4px;">
			<div style="margin-top:0px;margin-bottom:2px;">
				<AvantLogic:QuickHelpTitle ID="TourViewMarker" runat="server" Title="Marker" Span="true" />
			</div>
			
			<table cellpadding="0" cellspacing="0">
				<tr>
					<td>
						<uc:TourResourceComboBox Id="MarkerComboBox" runat="server" />
					</td>
					<td style="padding-left:4px;">
						<AvantLogic:MemberPageActionButton Subtle="true" ID="EditMarkerControl" runat="server" />
					</td>
					<td style="padding-left:4px;">
						<AvantLogic:MemberPageActionButton Subtle="true" ID="NewMarkerControl" runat="server" />
					</td>
				</tr>
			</table>
			
			<div id="MarkerFilterWarningPanel" runat="server" Visible="false" class="noticeMessage" style="margin-top:2px;width:558px;" />
		</asp:Panel>

		<div style="margin-top:16px;">
            <AvantLogic:MemberPageActionButton ID="LocateHotspotOnMapControl" runat="server" Visible="false" />
		    <AvantLogic:QuickHelpTitle ID="LocateHotspotOnMap" runat="server" Visible="false" Span="true" />
		</div>

		<asp:Panel ID="MapPanel" runat="server" style="position:absolute;left:540px;top:0px;">
			<table>
				<tr>
					<td valign="bottom">
						<asp:Image ID="MapImageElement" runat="server" style="padding:2px;border:solid 2px #e7f0f6;visibility:hidden;" />
					</td>
					<td valign="top" style="padding-left:8px;">
						<asp:Label runat="server" CssClass="controlLabel" Text="Map Preview" />
						<AvantLogic:QuickHelpTitle ID="SlideContentMapPreview" runat="server" TopMargin="0px" Span="true"/>
					</td>
				</tr>
			</table>
		</asp:Panel>
		
		<div style="margin-top:8px;">
			<table class="optionsTable" cellpadding="0" cellspacing="0">
				<tr>
					<td class="optionsSectionTitle" colspan="2">
						<asp:Label ID="MediaLabel" runat="server" />
						<AvantLogic:QuickHelpTitle ID="TourViewPhoto" runat="server" Span="true"/>
    					<AvantLogic:MemberPageActionButton ID="ToggleTextBoxHeightControl" runat="server" />
					</td>
				</tr>
				<tr>
					<td valign="top" style="width:100%;">
						<asp:Panel ID="PhotoPanel" runat="server">
							<asp:Panel ID="PhotoUploadPanel" runat="server" style="margin-top:12px;">
								<input type="file" runat="server" id="FileInput" style="height:22px;" onkeypress="return maStopEvent(event);"/>
								<asp:Button ID="ButtonUpload" runat="server" Enabled="true" Text="<%$ Resources:Text, ButtonUpload %>" OnClientClick="maOnEventUploadFile();return false;" CausesValidation="false" Height="22px" />

								<telerik:RadProgressManager id="RadProgressManager" runat="server" />
								<telerik:RadProgressArea id="ProgressArea" runat="server"
									OnClientProgressUpdating = "onClientProgressUpdating">
								</telerik:RadProgressArea>
								
								<%-- Photo --%>
								<asp:Panel ID="PhotoImagePanel" runat="server" CssClass="uploadedImage">
									<table cellpadding="0" cellspacing="0">
										<tr>
											<td valign="top">
												<asp:Image ID="ImageElement" runat="server" style="visibility:hidden;" />
											</td>
											<td valign="top" style="padding-left:8px;">
												<asp:Label ID="FileName" runat="server" CssClass="textUploadFileName"></asp:Label>
												<div class="finePrintHelp" style="margin-top:4px;">
													Display size: <asp:Label ID="ImageSizeScaled" runat="server"/>.&nbsp;&nbsp;
													Actual size: <asp:Label ID="ImageSizeActual" runat="server"/>.
												</div>
												<div class="finePrintHelp" style="margin-top:2px;">
													Move mouse over thumbnail to see larger image.<br />
												</div>
												<div class="finePrint" style="margin-top:2px;">
													<asp:LinkButton ID="ButtonRemove" runat="server" Enabled="true" Text="Remove Photo" OnClientClick="maOnEventRemoveSlideImage();return false;" CausesValidation="false" />
												</div>
											</td>
										</tr>
									</table>
								</asp:Panel>
							</asp:Panel>
						</asp:Panel>
						<asp:Panel ID="EmbedPanel" runat="server" style="margin-top:12px;">
							<div class="finePrintHelp" style="margin-bottom:4px;">
								<asp:Label ID="EmbedPanelInstructions" runat="server" /></div>
							<asp:TextBox ID="EmbedTextBox" runat="server" TextMode="MultiLine" Width="520" onkeypress="return maAllowEnter(event);" />
						</asp:Panel>
						<asp:Panel ID="MediaMessagePanel" runat="server" class="finePrintHelp" style="margin-top:4px;">
							<asp:Label ID="MediaMessage" runat="server"/>
						</asp:Panel>
					</td>
					<td valign="top">
						<asp:Panel ID="MediaSelectorPanel" runat="server" style="background-color:#ffffff;padding:6px;width:110px;">
							<div>
								<AvantLogic:QuickHelpTitle ID="MediaSelector" Title="Media" runat="server" OffsetX="-310" OffsetY="-80" TopMargin="0px" />
								<telerik:RadComboBox 
									id="MediaSelectorComboBox" 
									Runat="server"
									Width="110"
									AutoPostBack="False"
									Skin="Default"
									OnClientSelectedIndexChanging="maOnEventChangePageMode"
									>
									<ExpandAnimation Type="None" />
									<CollapseAnimation Type="None" />
									<Items>
										<telerik:RadComboBoxItem runat="server" Text="Photo" Value="0" />
										<telerik:RadComboBoxItem runat="server" Text="Multimedia" Value="1" />
									</Items>
								</telerik:RadComboBox>
							</div>
						</asp:Panel>
					</td>
				</tr>
			</table>
		</div>
		
		<asp:Panel ID="EmbedPreviewPanel" runat="server" style="margin-top:8px;margin-left:5px;border:solid 1px #e7f0f6;">&nbsp;</asp:panel>
		<asp:Panel ID="EmbedPreviewPanelError" runat="server" Visible="false" class="textNormal" style="margin:4px 0px 8px 5px;color:red">&nbsp;</asp:Panel>

		<div class="optionsSectionTitle">Text Editor <AvantLogic:QuickHelpTitle ID="TextEditor" runat="server" Span="true" OffsetX="20" OffsetY="-92" /></div>
		<table class="optionsTable" cellpadding="0" cellspacing="0">
            <tr>
				<td valign="top">
					<asp:Panel ID="TextPanel" runat="server" style="margin-top:12px;">
                    	<asp:TextBox ID="HtmlEditor" runat="server" />
					</asp:Panel>
				</td>
			</tr>
			<tr ID="NoTextMessage" runat="server" class="finePrintHelp">
				<td style="padding-top:4px;">
					This layout does not display text.
				</td>
			</tr>
		</table>
		
		<asp:Panel ID="TooltipSectionPanel" runat="server">
			<div class="optionsSectionTitle">Tooltip <AvantLogic:QuickHelpTitle ID="TourViewTooltip" runat="server" Span="true" OffsetX="20" OffsetY="-92" /></div>
			<table class="optionsTable" cellpadding="0" cellspacing="0">
				<tr>
					<td style="padding-top:6px;">
						<asp:Panel ID="NoTooltipPanel" runat="server" class="finePrintHelp" style="margin-bottom:4px;">
							<asp:Label ID="NoTooltipMessage" runat="server" />
						</asp:Panel>
						<asp:Panel ID="TooltipPanel" runat="server">
							<asp:TextBox ID="TourViewToolTipTextBox" runat="server" Width="500" />
						</asp:Panel>
					</td>
				</tr>
			</table>
		</asp:Panel>
	</div>
</asp:Content>

