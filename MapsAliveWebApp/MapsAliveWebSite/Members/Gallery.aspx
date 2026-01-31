<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="Gallery.aspx.cs"
	Inherits="Members_Gallery"
	ValidateRequest="false"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="App_Code\ServerControls\MapControls.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>
<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<script type="text/javascript">
	var markerClicked = false;
    var tourFolderPath = "<% =PathTourFolder %>/";
	function maGalleryMarkerSelected(viewId)
	{
		markerClicked = true;
		var listbox = $find("<%= HotspotOrderListBox.ClientID %>");
		var items = listbox.get_items();
		items.forEach(function(item){if(maGetViewId(item.get_value())==viewId)item.set_selected(true);});
	}
	function maSelectedHotspotChanged(sender, e)
	{
		if (markerClicked)
		{
			markerClicked = false;
			return;
		}
		var value = e.get_item().get_value();
		var viewId = maGetViewId(value);
        if (window.MapEditor.setMarkerSelected(viewId))
            window.MapEditor.updateControls();
	}
	function maEdit(hotspot, actionId)
	{
		var listbox = $find("<%= HotspotOrderListBox.ClientID %>");
		var item = listbox.get_selectedItem();
		if (item)
		{
			var value = item.get_value();
			var url;
			if (hotspot == 1)
				url = "TourViewEditor.ashx?vid=" + maGetViewId(value) + "&aid=" + actionId;
			else
				url = "EditMarker.aspx?id=" + maGetMarkerId(value);
			maOnEventSaveAndTransfer("/Members/" + url);
		}
		else
		{
			maAlert("No marker is selected.");
		}
	}
	function maGetViewId(value)
	{
		return parseInt(value.split(',')[0], 10);
	}
	function maGetMarkerId(value)
	{
		return value.split(',')[1];
	}
	function maChangeDetectedHelper()
	{
		var e = document.getElementById("StatusBox");
		if (e)
			e.style.display = "none";
	}
    function maOnPageLoaded()
    {
    }
    function maShowMarkerThumbAsSelected(markerId) 
	{
        // Ignore click on "Choose a slide"
        if (markerId === 0)
            return false;

        var ok = true;

        maShowMarkerThumbAsUnselected(markerId);
        maShowSelectedMarkerThumbName(markerId);

        var newThumb = document.getElementById('thumb' + markerId);
        if (newThumb) {
            newThumb.className = "markerThumbSelected";

            var e = document.getElementById('noMarker' + markerId);
            if (e) {
                if (ok) {
                    e.alt = '';
                    e.src = '../Images/SlideMarker1.gif';
                    e.id = 'hasMarker' + markerId;
                }
            }
            return ok;
        }
        else {
            return false;
        }
    }
    function maShowSelectedMarkerThumbName(markerId) {
        // Ignore click on "Choose a slide"
        if (markerId === 0)
            return;

        var newName = document.getElementById('name' + markerId);

        if (newName) {
            newName.className = "markerThumbNameSelected";
        }
    }
    function maShowMarkerInHotspotList(markerId) {
        var e = maGetElementByPageId("SlideNameDropDownList");
        if (e) {
            for (var option = 0; e.options.length; option++) {
                if (e.options[option].value == markerId) {
                    e.options[option].selected = true;
                    e.selectedIndex = e[option].index;
                    break;
                }
            }
        }
    }
    </script>
	<style type="text/css">
		.RadListBox .rlbItem
		{
			background-color:#fcfcfc;
			padding-top:0px;
			padding-bottom:0px;
		}
	</style>
	<% =StyleDefinitions %>
	
	<table cellpadding="0" cellspacing="0" style="margin-bottom:16px;">
		<tr>
			<td>
				<AvantLogic:QuickHelpTitle ID="GalleryBackgroundColor" runat="server" Title="Background Color" TopMargin="0px" />
				<AvantLogic:ColorSwatch Id="GalleryBackgroundColorSwatch" runat="server" ForPreview="true" />
			</td>
			<td style="padding-left:12px;">
				<AvantLogic:QuickHelpTitle ID="GalleryBackgroundImage" runat="server" Title="Background Image" TopMargin="0px" />
				<asp:DropDownList runat="server" ID="BackgroundImageDropDownList" />
			</td>
			<td style="padding-left:12px;">
				<AvantLogic:QuickHelpTitle ID="GalleryAlignV" runat="server" Title="Vertical Align" TopMargin="0px" />
				<asp:DropDownList runat="server" ID="AlignVDropDownList" />
			</td>
			<td style="padding-left:12px;">
				<AvantLogic:QuickHelpTitle ID="GalleryAlignH" runat="server" Title="Horizontal Align" TopMargin="0px" />
				<asp:DropDownList runat="server" ID="AlignHDropDownList" />
			</td>
		</tr>
	</table>

	<AvantLogic:QuickHelpTitle ID="GalleryMarginsAndSpacing" runat="server" Title="Margins and Spacing" TopMargin="0px" />

	<div style="padding-left:12px;">
		<table>
			<tr>
				<td align="Right">
					<asp:Label runat="server" CssClass="controlLabelNested" Text="Top Margin"></asp:Label>
				</td>
				<td>
					<asp:TextBox ID="MarginTop" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
					<asp:Label ID="MarginTopError" runat="server" CssClass="textErrorMessage" />
				</td>
				
				<td align="Right" style="padding-left:16px;">
					<asp:Label runat="server" CssClass="controlLabelNested" Text="Left Margin"></asp:Label>
				</td>
				<td>
					<asp:TextBox ID="MarginLeft" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
					<asp:Label ID="MarginLeftError" runat="server" CssClass="textErrorMessage" />
				</td>
				
				<td align="Right" style="padding-left:16px;">
					<asp:Label runat="server" CssClass="controlLabelNested" Text="Row Spacing"></asp:Label>
				</td>
				<td>
					<asp:TextBox ID="RowSpacing" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
					<asp:Label ID="RowSpacingError" runat="server" CssClass="textErrorMessage" />
				</td>
				
				<td align="Right" style="padding-left:16px;">
					<asp:Label ID="Label4" runat="server" CssClass="controlLabelNested" Text="Column Spacing"></asp:Label>
				</td>
				<td>
					<asp:TextBox ID="ColumnSpacing" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
					<asp:Label ID="ColumnSpacingError" runat="server" CssClass="textErrorMessage" />
				</td>
			</tr>
		</table>
	</div>

	<table cellpadding="0" cellspacing="0" style="margin-top:16px;">
		<tr>
			<td>
				<div class="checkboxOption">
					<asp:CheckBox ID="AutoSpacingRowCheckBox" runat="server" />
					<AvantLogic:QuickHelpTitle ID="GalleryAutoSpacingRow" runat="server" Title="Auto Row Spacing" Span="true" />
				</div>
			</td>
			<td style="padding-left:20px;">
				<div class="checkboxOption">
					<asp:CheckBox ID="UseFixedRowHeightCheckbox" runat="server" />
					<AvantLogic:QuickHelpTitle ID="GalleryUseFixedRowHeight" runat="server" Title="Fixed Row Height" Span="true" />
				</div>
			</td>
			<td id="EditHotspotOption" runat="server" style="padding-left:20px;">
				<AvantLogic:MemberPageActionButton ID="EditHotspotControl" Title="Edit Hotspot Content" runat="server" />
				<AvantLogic:QuickHelpTitle ID="GalleryEditHotspot" runat="server" Span="true" />
			</td>
		</tr>
		<tr>
			<td>
				<div class="checkboxOption">
					<asp:CheckBox ID="AutoSpacingColumnCheckBox" runat="server" />
					<AvantLogic:QuickHelpTitle ID="GalleryAutoSpacingColumn" runat="server" Title="Auto Column Spacing" Span="true" />
				</div>
			</td>
			<td style="padding-left:20px;">
				<div class="checkboxOption">
					<asp:CheckBox ID="UseFixedColumnWidthCheckbox" runat="server" />
					<AvantLogic:QuickHelpTitle ID="GalleryUseFixedColumnWidth" runat="server" Title="Fixed Column Width" Span="true" />
				</div>
			</td>
			<td id="EditMarkerOption" runat="server" style="padding-left:20px;">
				<AvantLogic:MemberPageActionButton ID="EditMarkerControl" Title="Edit Marker" runat="server" />
				<AvantLogic:QuickHelpTitle ID="GalleryEditMarker" runat="server" Span="true" />
			</td>
		</tr>
	</table>
	
	<div id="PreviewMessage" class="optionsSectionTitlePlain" style="width:180px;">Preview</div>
	
	<asp:Panel ID="MapArea" style="margin-top:4px;" runat="server">
		<div id="maMap" style="width:100%;height:100%;"></div>
	</asp:Panel>
	
	<div id="mapControlsPanel">
		<AvantLogic:MapControls ID="mapControls" runat="server" />
		<div id="mapControlsMessageArea">&nbsp;</div>
	</div>
	
	<br />
	<asp:Label ID="GallerySize" runat="server" CssClass="textHelp" />
	<asp:Panel ID="HotspotListPanel" runat="server" style="margin-bottom:16px;">
		<AvantLogic:QuickHelpTitle ID="HotspotOrderGallery" runat="server" Title="Hotspot Order" TopMargin="4px" />
		<telerik:RadListBox
			runat="server"
			ID="HotspotOrderListBox"
			Width="400px"
			OnClientReordered="maChangeDetectedForPreview"
			OnClientSelectedIndexChanged="maSelectedHotspotChanged"
			EnableDragAndDrop="true"
			>
		</telerik:RadListBox>
	</asp:Panel>
	
	<asp:HiddenField ID="MarkerCoords" runat="server" Value="" />
</asp:Content>