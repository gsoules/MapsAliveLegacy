<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="Map.aspx.cs"
	Inherits="Members_Map"
	Title="Untitled Page"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="App_Code\ServerControls\MapControls.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>
<%@ Register Assembly="App_Code\ServerControls\MarkerThumbs.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>
<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<script type="text/javascript">
	var originalMarkerZoomLimit = 0;
    var ignoreMarkerZoomLimitChange = false;
    var tourFolderPath = "<% =PathTourFolder %>/";
    let defaultMarkerId = <% =DefaultMarkerId %>;
    let defaultMarkerStyleId = <% =DefaultMarkerStyleId %>;
    let defaultMarkerImage = "<% =DefaultMarkerImage %>";
    let appRuntimeFolder = "<% =AppRuntimeFolder %>";
    let markerNameSuffix = "<% =MarkerCount %>";
    let newHotspotViewId = <% =NewHotspotViewId %>;
    let isPlusOrProPlan = <% =IsPlusOrProPlan %>;
    let selectingMarkerFromList = false;
    let defaultCoords = <% =DefaultCoords %>;
    let newShapeSideLength = <% =NewShapeSideLength %>;
    let hotspotData = <% =HotspotData %>;
    let hotspotLimitReached = <% =HotspotLimitReached %>;
    let markerData = <% =MarkerData %>;
    let markerStyleData = <% =MarkerStyleData %>;

    function hotspotChoiceChanged(radioButton)
    {
        let hotspotFields = document.getElementById("hotspotFields");
        let selectList = document.getElementById("hotspotSelectList");
        let newHotspot = radioButton.value === "new";
        hotspotFields.style.display = newHotspot ? "block" : "none";
        selectList.style.display = newHotspot ? "none" : "block";
    }
    function markerSelected()
    {
        let list = document.getElementById("markerSelect");
        let option = markerData.filter(obj => { return obj.id === parseInt(list.value, 10) });
        let imageId = option[0].image;
        let src = imageId === "1" ? defaultMarkerImage : `${appRuntimeFolder}Markers/${imageId}.png`;
        document.getElementById("selectedMarkerImage").src = src;
    }
    function markerStyleSelected()
    {
        let list = document.getElementById("markerStyleSelect");
        let option = markerStyleData.filter(obj => { return obj.id === parseInt(list.value, 10) });
        document.getElementById("selectedMarkerStyleImage").src = `${appRuntimeFolder}MarkerStyles/${option[0].image}.png`;
    }
	function maSaveWarning()
	{
        let msg = "<p>Hello " + contactName + ".</p><p>You have not saved your changes for a while and your MapsAlive session may expire soon.</p><p>To avoid losing any work, press SAVE to save changes and extend your browser session.</p><p>To continue without saving, press CANCEL."; 
        if (contentChanged)
        {
            let dialogShowing = document.getElementsByClassName('vex-dialog-form');
            if (dialogShowing.length == 0)
                maConfirmAndExecuteScript(msg, "maOnEventSave();", "SAVE");
        }
        setTimeout(maSaveWarning, saveTimeout);
	}
    function maDrawRoute(viewIdList)
    {
        let tour = MapsAlive.firstTourOnPage;
        let route = { id: viewIdList, lineWidth: 3, lineColor: "#ff0000", lineAlpha: 100, effects: "" };
        tour.api.drawRoutes([route]);
    }
    function maOnPageLoaded()
	{
        if (maUndefined(tourViewIdToLocate) && newHotspotViewId  !== 0)
            tourViewIdToLocate = newHotspotViewId;

        if (!maUndefined(tourViewIdToLocate))
            maSelectMarkerThumb(tourViewIdToLocate);
	}

    function maSelectMarkerThumb(markerId)
	{
		if (markerId === 0)
            return;
        if (window.MapEditor.hybridEditingEnabled || window.MapEditor.editingMarker)
        {
            maShowMarkerInHotspotList(0);
            window.MapEditor.hotspotSelectedWhileEditing();
            return;
        }
        if (maShowMarkerThumbAsSelected(markerId))
		{
            selectingMarkerFromList = true;
            window.MapEditor.setMarkerSelected(markerId);
            selectingMarkerFromList = false;
            maShowMarkerInHotspotList(markerId);
            window.MapEditor.updateControls();
		}
	}
    function maShowMarkerInHotspotList(markerId)
	{
        var e = maGetElementByPageId("SlideNameDropDownList");
		if (e)
		{
			for (var option = 0; e.options.length; option++)
			{
				if (e.options[option].value == markerId)
				{
					e.options[option].selected = true;
					e.selectedIndex = e[option].index;
					break;
				}
			} 
		}
	}
	function maShowMarkerThumbAsSelected(markerId)
	{
		// Ignore click on "Choose a slide"
		if (markerId === 0)
			return false;
		
		var ok = true;
		
		maShowSelectedMarkerThumbName(markerId);
			
		var newThumb = document.getElementById('thumb' + markerId);
		if (newThumb)
		{
			newThumb.className = "markerThumbSelected";
				
			var e = document.getElementById('noMarker' + markerId);
			if (e)
			{
				if (ok)
				{
					e.alt = '';
					e.src = '../Images/SlideMarker1.gif';
					e.id = 'hasMarker' + markerId;
				}
			}
			return ok;
		}
		else
		{
			return false;
		}
	}
	function maShowMarkerThumbAsHidden(markerId)
	{
		// Ignore click on "Choose a slide"
		if (markerId === 0)
			return;
		
        maShowMarkerThumbAsUnselected(markerId);

		var e = document.getElementById('hasMarker' + markerId);
		if (e)
		{
			e.src = '../Images/SlideMarker0.gif';
			e.id = 'noMarker' + markerId;
		}
	}
	function maShowSelectedMarkerThumbName(markerId)
	{
		// Ignore click on "Choose a slide"
		if (markerId === 0)
			return;
		
		var newName = document.getElementById('name' + markerId);
		
		if (newName)
		{
			newName.className = "markerThumbNameSelected";
		}
     }
    function maMarkersOnMapChanged(count, total)
    {
        console.log("maMarkersOnMapChanged " + count);
        let panel = document.getElementsByClassName('statusBoxLastAction');
        if (panel.length === 0)
            return;
        let e = panel[0];
        if (count === 0)
        {
            e.innerHTML = `None of the markers are on the map`;
        }
        else if (count === total)
        {
            let howMany = count === 2 ? "Both" : `All ${count}`;
            let html = `<div>${howMany} markers are on the map.</div><div class="statusBoxDone1">Press the Tour Preview button<br>to try using this tour.</div>`;
            html += `<div class="statusBoxDone2">You have completed the step-by-step instructions for this tour.</div>`;
            e.innerHTML = html;
            document.getElementById('statusBoxInstructions').style.display = "none";
        }
        else
        {
            e.innerHTML = `${count} of ${total} markers are on the map`;
        }
    }
    </script>
	<% =StyleDefinitions %>
	<asp:Panel ID="MapPanel" runat="server">
		<asp:DropDownList ID="SlideNameDropDownList" runat="server"/>
		<AvantLogic:QuickHelpTitle ID="TourPageMap" runat="server" TopMargin="0px" Span="true" />
		
		<asp:DropDownList ID="RoutesDropDownList" runat="server" Visible="false" style="margin-left:16px;"/>
		<AvantLogic:QuickHelpTitle ID="RoutesListExplain" runat="server" Visible="false" TopMargin="0px" Span="true" />

		<asp:Panel ID="MapArea" style="margin-top:4px;border-top:solid 1px #ccc" runat="server">
			<div id="maMap" style="width:100%;height:100%;position:relative">
		</asp:Panel>

		<div id="mapControlsPanel">
			<AvantLogic:MapControls ID="mapControls" runat="server" />
			<div id="mapControlsMessageArea">&nbsp;</div>
		</div>

		<div class="optionsSectionTitle">Hotspot Thumbnails <AvantLogic:QuickHelpTitle ID="TourPageSlides" runat="server" Span="true" OffsetX="20" OffsetY="-300" /></div>

		<AvantLogic:MarkerThumbs ID="MarkerThumbs" runat="server" ColumnsPerRow="6"	/>

		<asp:HiddenField ID="MarkerCoords" runat="server" Value="" />
		<asp:HiddenField ID="ZoomState" runat="server" Value="" />
		<asp:HiddenField ID="DeletedHotspots" runat="server" Value="" />
		<asp:HiddenField ID="ReplacedMarkers" runat="server" Value="" />
		<asp:HiddenField ID="ReplacedMarkerStyles" runat="server" Value="" />
		<asp:HiddenField ID="EditedMarkers" runat="server" Value="" />
		<asp:HiddenField ID="NewHotspot" runat="server" Value="" />
		<asp:HiddenField ID="NewShapeMarker" runat="server" Value="" />
		<asp:HiddenField ID="LastShape" runat="server" Value="" />
		<asp:HiddenField ID="LastHotspotChoice" runat="server" Value="" />
		<asp:HiddenField ID="HybridConvert" runat="server" Value="" />
	</asp:Panel>
	
	<asp:Panel ID="NoMapPanel" runat="server"  Visible="false" CssClass="textNormal">
	<br />
	This layout does not use a map. To switch to a layout with a map choose<br /><b>Layout > Template Choices</b> from the menu.
	</asp:Panel>
</asp:Content>

