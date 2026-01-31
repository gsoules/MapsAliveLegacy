// Copyright (C) 2009-2022 AvantLogic Corporation: https://www.mapsalive.com

export { MapsAliveMap };

import './hammer.min.js';
import { MapsAliveGraphics as Graphics__$$ } from './mapsalive-graphics.js';
import { MapsAliveMarker } from './mapsalive-marker.js';
import { MapsAliveMarker as Marker__$$} from './mapsalive-marker.js ';
import { MapsAliveMarkerStyle as MarkerStyle__$$ } from './mapsalive-marker.js ';
import { MapsAliveMarkerStyleProperties as MarkerStyleProperties__$$ } from './mapsalive-marker.js ';
import { MapsAliveRuntime as Runtime__$$ } from './mapsalive-runtime.js';

class MapsAliveMap
{
	constructor(page, mapData, symbols)
	{
		//console.log(`Map__$$::contructor ${page.pageName}`);

		this.page = page;

		for (const property in mapData)
			this[property] = mapData[property];

		this.symbols = symbols;
		this.tour = page.tour;
		this.graphics = null;

		this.markers = null;
		this.defaultMarker = null;
		this.savedMarkers = null;
		this.editingHybrid = false;
		this.routes = [];

		// Only get/set via the currentMapScale method.
		this.mapScale_ = 0;

		this.mapId = this.tour.uniqueId('Map');
		this.mapImage100 = null;
		this.mapImage50 = null;
		this.mapImage25 = null;
		this.useFullSizeImage = false;
		this.mapInsetImage = null;
		this.mapInsetRect = null;
		this.mapInsetZoomRect = null;
		this.mapInsetScale = 0;
		this.drawingMap = false;
		this.drawState = null;
		this.allowOverZoom = false;

		this.panningMap = false;
		this.panPointerStartX = 0;
		this.panPointerStartY = 0;
		this.panMapStartX = 0;
		this.panMapStartY = 0;
		this.panNewX_map = 0;
		this.panNewY_map = 0;
		this.lastPanEventType = '';
		this.editorIsHandlingPanEvents = false;
		this.autoPanCurve = null;
		this.panX_map = 0;
		this.panY_map = 0;
		this.markerViewIdToSelectAfterAutoPan = 0;
		this.disableAutoPan = false;
		this.zoomLimit = 100;

		this.drawMapDelayTimerId = 0;
		this.zoomMouseWheelDelayTimerId = 0;

		this.autoPanningMap = false;
		this.stopAutoPanningMap = false;
		this.zoomingMap = false;
		this.zoomingMapInOut = false;
		this.zoomPercent = 0;
		this.pinchingMap = false;
		this.lastPinchDistance = 0;
		this.currentPinchDistance = 0;
		this.mousePointer = { x: 0, y: 0 };
		this.panCenterLocation = { x_map: -1, y_map: -1, x_screen: -1, y_screen: -1 };

		this.editLayer = null;
		this.editLayerContext = null;
		this.handlesLayer = null;
		this.handlesLayerContext = null;
		this.hitLayer = null;
		this.hitLayerContext = null;
		this.insetLayer = null;
		this.insetLayerContext = null;
		this.mapLayer = null;
		this.mapLayerContext = null;
		this.markerLayer = null;
		this.markerLayerContext = null;
		this.markerLayerCache = null;
		this.slowGraphicsAllowed = true;

		this._disableMarkerDrawing = false;

		this.statusArea = null;
		this.tooltipElement = null;

		// Constants
		this.ACTION_NONE = 0;
		this.ACTION_GOTO_PAGE = 1;
		this.ACTION_LINK_TO_URL = 2;
		this.ACTION_LINK_TO_URL_IN_NEW_WINDOW = 5;
		this.ACTION_CALL_JAVASCRIPT = 3;
		this.SHOW_CONTENT_ON_MOUSEOVER = 0;
		this.SHOW_CONTENT_ON_CLICK = 1;
		this.SHOW_CONTENT_NEVER = 2;
		this.POINTER_STATE_START = 1;
		this.POINTER_STATE_MAP = 2;
		this.POINTER_STATE_NEW_MARKER = 3;
		this.POINTER_STATE_SAME_MARKER = 4;
		this.POINTER_STATE_DIFFERENT_MARKER = 5;
		this.ZOOM_STATE_UNLOCKED = 1;
		this.ZOOM_STATE_LOCKED = 2;
		this.MAP_IMAGE_SHARPENING_NONE = 0;
		this.MAP_IMAGE_SHARPENING_SOFT = 1;
		this.MAP_IMAGE_SHARPENING_SHARP = 2;
		this.MAP_IMAGE_SHARPENING_BALANCED = 3;
		this.OVER_ZOOM_COLOR = '#ff0000';
		this.NORMAL_ZOOM_COLOR = '#ffffff'

		// The control must be a multiple of four to be drawn correctly at different sizes.
		this.ZOOM_CONTROL_SIZE = this.tour.isTouchDeviceWithoutMouse && screen.width < 450 ? 24 : 24;

		// Initialize the rect used by getPointerLocationOnMap to null (it will also get set to null on
		// a resize or scroll event). Null tells getPointerLocationOnMap to get the current bounds.
		this.mapElementBoundingClientRect = null;

		// Bind event handlers so that when they are called, 'this' will be set to this MapsAliveMap object.
		this.bindEventHandlers();
	}

	allowSlowGraphics(allow)
	{
		//console.log(`Map::allowSlowGraphics ${allow}`);

		// This method allow or disallows slow graphics operations to occur, namely marker blending which is
		// very compute intensive. Disabling blending improves performance when zooming or panning. On maps
		// like 50 USA states or Texas counties that can have dozens or even a hundred or more shape markers
		// that all use the blend effect, literally billions of operations can be required to draw them all
		// blended on each animation frame. That makes panning and zooming very sluggish even on the fastest
		// devices. By temporarily disabling these expensive operations, performance is drastically improved
		// at the cost of the map markers not showing their blended appearance during the zoom or pan, but
		// the appearance is restored as soon as the zoom or pan stops.

		let disableBlending = this.tour.getTourSettingBool("disable-blending") === true || this.tour.editMode;
		if (disableBlending)
		{
			if (allow)
			{
				this.slowGraphicsAllowed = true;

				// Force the map to draw again using marker blending.
				this.initializeDrawState();
				this.drawMap();
				if (this.tour.editMode)
					this.editor.redrawSelectedMarkers();
			}
			else
			{
				this.slowGraphicsAllowed = false;
			}
		}

		// Ignore the request if the option is set to always draw markers.
		let disableDrawing = this.tour.getTourSettingBool("disable-drawing") === true;
		if (disableDrawing)
		{
			if (allow)
			{
				this.disableMarkerDrawing(false);
				this.flushMarkerLayerCache();
			}
			else
			{
				this.disableMarkerDrawing(true, true);
			}
		}
	}

	bindEventHandlers()
	{
		this.onTapMap = this.onTapMap.bind(this);

		this.onPanMap = this.onPanMap.bind(this);
		this.panMapOnEachAnimationFrame = this.panMapOnEachAnimationFrame.bind(this);
		this.panMapAutomaticallyOnEachAnimationFrame = this.panMapAutomaticallyOnEachAnimationFrame.bind(this);

		this.onTapZoomInOutButton = this.onTapZoomInOutButton.bind(this);
		this.onZoomInOutButtonMouseOver = this.onZoomInOutButtonMouseOver.bind(this);
		this.zoomMapOnEachAnimationFrame = this.zoomMapOnEachAnimationFrame.bind(this);

		this.onPinchMap = this.onPinchMap.bind(this);

		this.onMapMouseMove = this.onMapMouseMove.bind(this);
		this.onMapClick = this.onMapClick.bind(this);
		this.onMapDoubleClick = this.onMapDoubleClick.bind(this);
		this.onMapMouseOut = this.onMapMouseOut.bind(this);
		this.onMouseWheel = this.onMouseWheel.bind(this);
		this.onMouseWheelTimeout = this.onMouseWheelTimeout.bind(this);
		this.onMapInsetMouseMove = this.onMapInsetMouseMove.bind(this);

		this.onRawInputZoomInOutButton = this.onRawInputZoomInOutButton.bind(this);
		this.onRawInputMap = this.onRawInputMap.bind(this);

		this.drawMapImageHighQuality = this.drawMapImageHighQuality.bind(this);
		this.drawMap = this.drawMap.bind(this);

		this.drawAllMarkers = this.drawAllMarkers.bind(this);
		this.showNextSlide = this.showNextSlide.bind(this);

		this.movePopupWithMouse = this.movePopupWithMouse.bind(this);
		this.onKeyDown = this.onKeyDown.bind(this);
		this.onTourBuilderMouseUp = this.onTourBuilderMouseUp.bind(this);
		this.onResize = this.onResize.bind(this);
		this.onScroll = this.onScroll.bind(this);

		this.showPopupAfterDelay = this.showPopupAfterDelay.bind(this);
		this.deselectMarkerAndClosePopupAfterDelay = this.deselectMarkerAndClosePopupAfterDelay.bind(this);
	}

	blinkMarker(viewId)
	{
		//console.log(`Map::blinkMarker ${viewId}`);

		let marker = this.getMarker(viewId);

		let pulsesPerCycle = 5;
		let cycle = marker.blinkCount % pulsesPerCycle;

		if (cycle === 0)
		{
			// Change the cycle direction from increasing to descreasing or vice-versa.
			marker.blinkDirection *= -1;

			if (marker.blinkDirection === -1 && marker.blinkCount / (pulsesPerCycle * 2) >= marker.blinkLimit - 1)
			{
				//console.log("TURN OFF BLINK " + marker.blinkCount + "," + marker.blinkLimit + "," + marker.viewId);
				marker.setBlink(0);
				return;
			}
		}

		// Create a two-cycle pattern of 0123443210.
		if (marker.blinkDirection === -1)
			cycle = pulsesPerCycle - cycle - 1;

		// Set the alpha value within the cycle such that the marker "blinks" like an incandescent  
		// lamp.  It does not go on/off instantly -- there is a slight fade between fully on and fully off.
		let alpha;
		if (cycle === 0)
			alpha = 10;
		else if (cycle === 1)
			alpha = 20;
		else if (cycle === 2)
			alpha = 80;
		else
			alpha = 100;

		//console.log("blink = " + cycle + " : " + alpha);

		marker.setBlinkAlpha(alpha / 100);
		marker.blinkCount++;
	}

	calculateMapInsetScale()
	{
		let longEdge = this.mapW_actual > this.mapH_actual ? this.mapW_actual : this.mapH_actual;
		return this.mapInsetSize / longEdge;
	}

	calculatePanXLimit() 
	{
        let leftMostPanX_map = 0;
        let zoomedMapWidth_screen = this.convertMapToScreen(this.mapW_actual);

        // Determine how far left the map can be panned.
		if (zoomedMapWidth_screen > this.canvasW) 
		{
            // Calculate how much of the map extends left or right of the canvas.
            // That amount scaled up to the map size is far the map can be panned left.
            let extraWidth = zoomedMapWidth_screen - this.canvasW;
            leftMostPanX_map = this.convertScreenToMap(-extraWidth);
        }
        return leftMostPanX_map;
    }

	calculatePanYLimit()
	{
        let topMostPanY_map = 0;
        let zoomedMapHeight_screen = this.convertMapToScreen(this.mapH_actual);

        // Determine how far up the map can be panned.
		if (zoomedMapHeight_screen > this.canvasH) 
		{
            // Calculate how much of the map extends above or below the canvas.
            // That amount scaled up to the map size is how far the map can be panned up.
            let extraHeight = zoomedMapHeight_screen - this.canvasH;
            topMostPanY_map = this.convertScreenToMap(-extraHeight);
        }
        return topMostPanY_map;
    }

	calculateZoomedOutMapScale()
	{
		// Determine the scale needed to make the map fit within the current canvas area fully zoomed out.
		// Calculate based on the shorter side of the canvas so that the entire map will be visible.
		let canvasSize = Runtime__$$.createSizeObject(this.canvasW, this.canvasH);
		let zoomedOutMapSize = Runtime__$$.scaledImageSize(this.mapSize_actual, canvasSize);

		let scale;
		if (this.canvasW > this.canvasH)
			scale = zoomedOutMapSize.w / this.mapW_actual;
		else
			scale = zoomedOutMapSize.h / this.mapH_actual

		this.zoomedOutMapScale = Runtime__$$.roundFloat(scale);

		//console.log(`Map::calculateZoomedOutMapScale ${this.zoomedOutMapScale}`);
	}
	
	calculateZoomedToFillCanvasMapScale()
	{
		// Determine the scale needed to make the map fill the entire canvas area provided the map image is large enough
		let canvasAspectRatio = this.canvasW / this.canvasH;
		let mapAspectRatio = this.mapW_actual / this.mapH_actual;
		let widthScale = this.canvasW / this.mapW_actual;
		let heightScale = this.canvasH / this.mapH_actual;

		function validScale(scale)
		{
			// Make sure the scale is not more than 100% which can happen if the map image is narrower/shorter than the canvas.
			return Math.min(scale, 1);
		}

		return validScale(mapAspectRatio >= canvasAspectRatio ? heightScale : widthScale);
	}

	changeMarkerShapeAppearance(selected, viewIdList, lineColorNumber, lineColorOpacity, fillColorNumber, fillColorOpacity, effectsDefinition, draw = true)
	{
		// Get all the markers to be changed.
		let markers = this.createMarkerArrayFromMarkerIdList(viewIdList);

		for (const marker of markers)
		{
			// Create a new marker style properties object containing the changes.
			let markerStyle = marker.markerStyle;
 			let properties = this.mergeMarkerStyleProperties(markerStyle, selected, lineColorNumber, fillColorNumber, lineColorOpacity, fillColorOpacity, effectsDefinition);

			// Apply the changes to the marker.
			marker.changeMarkerShapeAppearance(selected, properties);
		}

		// Redraw all the changed markers.
		if (draw)
			this.graphics.drawIntersectingMarkers(markers, selected);
	}

	changeMarkerStyleAppearance(selected, styleId, lineColorNumber, lineColorOpacity, fillColorNumber, fillColorOpacity, effectsDefinition, draw = true)
	{
		// Make sure the passed-in style is valid.
		let markerStyle = this.getMarkerStyle(styleId);
		if (markerStyle === null)
			return;

		// Create a new marker style properties object containing the changes.
		let properties = this.mergeMarkerStyleProperties(markerStyle, selected, lineColorNumber, fillColorNumber, lineColorOpacity, fillColorOpacity, effectsDefinition);

		// Apply the changes to the marker style and redraw all the markers that use the style.
		markerStyle.setChangedProperties(selected, properties);

		if (draw)
		{
			let markers = this.findMarkersThatUseStyle(styleId);
			this.graphics.drawIntersectingMarkers(markers, selected);
		}
	}

	changeMarkerStackingOrder(viewId, above)
	{
		let reorderedMarkers = [];
		const below = !above;

		let markerBeingMoved = this.getMarker(viewId);

		// Put the marker first in the array so that it will be drawn first, below all other markers.
		if (below)
			reorderedMarkers.push(markerBeingMoved)

		// Copy all the markers from the old array to the new except for the one being moved.
		for (const marker of this.markers)
		{
			if (marker.viewId === viewId)
				continue;
			reorderedMarkers.push(marker)
		}

		// Put the marker last in the array so that it will be drawn last, above all other markers.
		if (above)
			reorderedMarkers.push(markerBeingMoved);

		this.markers = reorderedMarkers;
		this.reportMarkerCoords();
	}

	clearDrawMapDelayTimer()
	{
		if (this.drawMapDelayTimerId === 0)
			return;

		//console.log(`Map::clearDrawMapDelayTimer`);
		clearTimeout(this.drawMapDelayTimerId);
		this.drawMapDelayTimerId = 0;
	}

	clearPopupDelayTimer()
	{
		if (this.popupDelayTimerId === 0)
			return;

		// Clear the timer used for both the delay before showing a popup, and the delay
		// before deselecting a marker. Since the two options are mutually exclusive, only 
		// one timer Id is needed for whichever of the two kinds of delay is in use.

		//console.log(`Map::clearPopupDelayTimer`);
		clearTimeout(this.popupDelayTimerId);
		this.popupDelayTimerId = 0;
	}

	convertMapFocusPercentToScale()
	{
		if (this.mapFocusPercent === 0)
			return this.zoomedOutMapScale;

		if (this.mapFocusPercent === -1)
		{
			if (this.tour.editMode)
				return this.currentMapScale;
			else
				return this.calculateZoomedToFillCanvasMapScale();
		}

		let scale = this.mapFocusPercent / 100;
		if (this.tour.isFlexMapTour && !this.tour.editMode)
		{
			// Check if the current scale is sufficient to fill the canvas, and if not, increase it.
			let scaledMapW = this.convertMapToScreen(this.mapW_actual);
			let scaledMapH = this.convertMapToScreen(this.mapH_actual);
			if (scaledMapW < this.canvasW || scaledMapH < this.canvasH)
				scale = this.calculateZoomedToFillCanvasMapScale();
		}
		return scale;
	}

	convertMapToScreen(value_map)
	{
		// This method converts the position of a pixel on the screen to the corresponding
		// pixel on the map based on the current scale of the map. The examples below are 
		// for a 1200px wide map that is scaled to 75% on a 900px wide map canvas.
		//
		// value_map	_screen
		//	  0			  0
		//	100			 75
		//	600			450
		//  900			900

		return Math.round(value_map * this.currentMapScale);
	}

	convertScreenToMap(value_screen)
	{
		// This method converts the position of a pixel on map to the corresponding pixel
		// on the screen based on current the scale of the map. The examples below are
		// for a 1200px wide map that is scaled to 75% on a 900px wide map canvas.
		//
		// value_screen	_map
		//	  0			   0
		//	100			 133
		//	600			 800
		//  900			1200

		return Math.round(value_screen / this.currentMapScale);
	}

	convertScreenToMapDouble(value_screen)
	{
		let value_map = value_screen / this.currentMapScale;
		return value_map;
	}

	createCanvasLayer(id)
	{
		//console.log(`Map::createCanvasLayer ${id}`);

		let layer = document.createElement("canvas");
		layer.id = id;
		layer.width = this.tour.editMode ? this.canvasW : 0;
		layer.height = this.tour.editMode ? this.canvasH : 0;
		layer.style.backgroundColor = "transparent";
		layer.style.position = "absolute";
		layer.style.userSelect = "none";
		return layer;
	}

	createCanvasLayerForMapInset() 
	{
		this.insetLayer = this.createCanvasLayer(this.tour.uniqueId('InsetLayer'));
		this.insetLayerContext = this.getCanvasContext(this.insetLayer);
		this.setMapInsetRectSize({ resize: false });
	}

	createDefaultMarker()
	{
		let marker = new MapsAliveMarker(
			this,	// map,
			0,		// markerId
			0,		// viewId
			2,		// markerType is MARKER_TYPE_SHAPE
			0,		// styleId
			0,		// normalSymbolId,
			0,		// selectedSymbolId
			0,		// pctX
			0,		// pctY
			5,		// shapeType is SHAPE_TYPE_HYBRID
			0,		// shapeW
			0,		// shapeH
			0,		// normalSymbolW
			0,		// normalSymbolH
			0,		// selectedSymbolW
			0,		// selectedSymbolH
			0,		// symbolLocationX
			0,		// symbolLocationY
			0,		// centerOffsetX
			0,		// centerOffsetY
			0,		// rotationDegrees
			0,		// zoomThreshold
			0,		// flags
			"",		// tooltip
			0,		// clickAction
			"",		// clickActionTarget
			0,		// mouseoverAction
			"",		// mouseoverActionTarget
			0,		// mouseoutAction
			"",		// mouseoutActionTarget
			false,	// touchPerformsClickAction
			false,	// showContentEvent
			"",		// shapeCoords
		);

		marker.globalMarkerStyle = this.getMarkerStyle(0);
		marker.privateMarkerStyle = null;
		marker.markerZooms = true;
		marker.routeDefined = false;

		this.defaultMarker = marker;
	}

	createHybridMarkerFromPseudoMarkers(hybridMarker)
	{
		// This method converts a set of pseudo markers back into the hybrid marker passed to the method.
		//console.log(`HYBRID START: ${hybridMarker.x},${hybridMarker.y} :: ${hybridMarker.shapeW}x${hybridMarker.shapeW}`);

		// Determine the upper-left and lower-right corners of the bounding area than encloses all the pseudo markers.
		let x1 = Number.MAX_SAFE_INTEGER;
		let y1 = Number.MAX_SAFE_INTEGER;
		let x2 = Number.MIN_SAFE_INTEGER;
		let y2 = Number.MIN_SAFE_INTEGER;

		for (let pseudoMarker of this.markers)
		{
			let pseudoMarkerBounds = pseudoMarker.getBounds();
			x1 = Math.min(pseudoMarkerBounds.cornerX_map, x1);
			y1 = Math.min(pseudoMarkerBounds.cornerY_map, y1);
			x2 = Math.max(pseudoMarkerBounds.cornerX_map + pseudoMarkerBounds.w_actual - 1, x2);
			y2 = Math.max(pseudoMarkerBounds.cornerY_map + pseudoMarkerBounds.h_actual - 1, y2);
		}

		// Shift the bounding box to account for panning.
		x1 -= this.panX_map;
		y1 -= this.panY_map;
		x2 -= this.panX_map;
		y2 -= this.panY_map;

		// Determine the size of the hybrid marker's bounding box.
		let hybridW = x2 - x1 + 1;
		let hybridH = y2 - y1 + 1;

		// Construct the hybrid marker's shape coords which are a "-1,T,"-separated list where T is the shape type.
		let hybridShapeCoords = "";
		for (let marker of this.markers)
		{
			let coords = marker.shapeCoordsArray;

			// Adjust the pseudo marker's coords to be zero-based relative to the hybrid marker's bounding box.
			switch (marker.shapeType)
			{
				case marker.SHAPE_TYPE_CIRCLE:
					coords[0] = marker.x - x1;
					coords[1] = marker.y - y1;
					break;

				case marker.SHAPE_TYPE_RECTANGLE:
					let halfW = Math.round(marker.shapeW / 2);
					let halfH = Math.round(marker.shapeH / 2);
					coords[0] = marker.x - x1 - halfW;
					coords[1] = marker.y - y1 - halfH;
					coords[2] = coords[0] + marker.shapeW - 1;
					coords[3] = coords[1] + marker.shapeH - 1;
					break;

				case marker.SHAPE_TYPE_POLYGON:
				case marker.SHAPE_TYPE_LINE:
					let b = marker.getBounds();
					let deltaX = b.centerX_map - x1 - Math.round(marker.shapeW / 2);
					let deltaY = b.centerY_map - y1 - Math.round(marker.shapeH / 2);
					for (let i = 0; i < coords.length; i += 2)
					{
						coords[i] = coords[i] + deltaX - this.panX_map;
						coords[i + 1] = coords[i + 1] + deltaY - this.panY_map;
					}
					break;
			}

			// Append the shape coords for this shape to the hybrid's shape coords, separated by "-1,T,".
			let updatedCoords = coords.join(",");
			if (hybridShapeCoords.length > 0)
				hybridShapeCoords += ",";
			hybridShapeCoords += `-1,${marker.shapeType},${updatedCoords}`;
		}

		// Assign the hybrid shape's coords and size to its marker.
		hybridMarker.shapeCoords = hybridShapeCoords;
		hybridMarker.shapeW = hybridW;
		hybridMarker.shapeH = hybridH;
		hybridMarker.updateBounds();

		// Calculate and set the hybrid marker's center. When the width or height is an odd number, the
		// center x and/or y value will contain a half pixel from having divided the odd dimension by 2.
		// Discard the half pixel when the dimension is odd and round it up when the dimension is even.
		let centerX = x1 + hybridW / 2;
		let centerY = y1 + hybridH / 2;
		if (hybridW % 2 === 0)
			centerX = Math.floor(centerX);
		else
			centerX = Math.ceil(centerX);
		if (hybridH % 2 === 0)
			centerY = Math.floor(centerY);
		else
			centerY = Math.ceil(centerY);

		this.editor.updateMarkerPosition(hybridMarker, centerX, centerY);

		//console.log(`HYBRID END: ${hybridMarker.x},${hybridMarker.y} :: ${hybridMarker.shapeW}x${hybridMarker.shapeH} ${hybridMarker.shapeCoords}`);
	}

	createHybridPseudoMarker(shapeType, coords, markerId, hybridMarker)
	{
		// This method converts the data extracted for one of a hybrid marker's shapes into an actual marker.
		//console.log(`Map::createHybridPseudoMarker ${hybridMarker.x},${hybridMarker.y} :: ${hybridMarker.shapeW}x${hybridMarker.shapeH} ${hybridMarker.shapeCoords}`);

		const markerType = hybridMarker.MARKER_TYPE_SHAPE;
		let shapeSize = { w: 0, h: 0 };
		let cornerX = 0;
		let cornerY = 0;
		let shapeCoords;

		// Calculate the shape's upper-left corner within the bounding box that contains the entire hybrid marker.
		switch (shapeType)
		{
			case hybridMarker.SHAPE_TYPE_CIRCLE:
				cornerX = coords[0];
				cornerY = coords[1];
				let radius = coords[2];
				cornerX -= radius;
				cornerY -= radius;
				shapeSize.w = radius * 2;
				shapeSize.h = shapeSize.w;
				shapeCoords = coords.join(",");
				break;

			case hybridMarker.SHAPE_TYPE_RECTANGLE:
				cornerX = coords[0];
				cornerY = coords[1];
				let x2 = coords[2] + 1;
				let y2 = coords[3] + 1;
				shapeSize.w = x2 - cornerX;
				shapeSize.h = y2 - cornerY;;
				shapeCoords = coords.join(",");
				break;

			case hybridMarker.SHAPE_TYPE_POLYGON:
			case hybridMarker.SHAPE_TYPE_LINE:
				// Normalize the coords so that they are zero-based within their own bounding-box. The delta x and y values
				// are the shape point's distance from the upper-left corner of the containing hybrid marker's bounding box.
				let normalizedCoords = this.editor.normalizeCoords(coords);
				shapeSize = normalizedCoords.size;
				cornerX = normalizedCoords.deltaX;
				cornerY = normalizedCoords.deltaY;
				shapeCoords = normalizedCoords.coords.join(",");
				break;
		}

		// Construct the marker.
		let pseudoMarker = new MapsAliveMarker(
			this,					// map,
			markerId,				// markerId
			markerId,				// viewId (use same value as markerId)
			markerType,				// markerType
			hybridMarker.styleId,	// styleId
			0,						// normalSymbolId,
			0,						// selectedSymbolId
			0,						// pctX
			0,						// pctY
			shapeType,				// shapeType
			shapeSize.w,			// shapeW
			shapeSize.h,			// shapeH
			0,						// normalSymbolW
			0,						// normalSymbolH
			0,						// selectedSymbolW
			0,						// selectedSymbolH
			0,						// symbolLocationX
			0,						// symbolLocationY
			0,						// centerOffsetX
			0,						// centerOffsetY
			0,						// rotationDegrees
			0,						// zoomThreshold
			0,						// flags
			"",						// tooltip
			0,						// clickAction
			"",						// clickActionTarget
			0,						// mouseoverAction
			"",						// mouseoverActionTarget
			0,						// mouseoutAction
			"",						// mouseoutActionTarget
			false,					// touchPerformsClickAction
			false,					// showContentEvent
			shapeCoords,			// shapeCoords
		);

		pseudoMarker.markerZooms = hybridMarker.markerZooms;
		pseudoMarker.isPseudoMarker = true;

		// Calcuate the center of the pseudo marker based on its size and relative position within the hybrid marker.
		let hybridMarkerBounds = hybridMarker.getBounds();
		let pseudoMarkerBounds = pseudoMarker.getBounds();
		let centerX = hybridMarkerBounds.cornerX_map + cornerX + pseudoMarkerBounds.halfW_actual;
		let centerY = hybridMarkerBounds.cornerY_map + cornerY + pseudoMarkerBounds.halfH_actual;

		// Adjust the center by the pan amount.
		centerX -= this.panX_map;
		centerY -= this.panY_map;

		// Set the pseudo marker object's center x and y values which will also calculate its pctX and pctY values.
		this.editor.updateMarkerPosition(pseudoMarker, centerX, centerY);

		return pseudoMarker;
	}

	createHybridPseudoMarkersFromHybrid(hybridMarker)
	{
		// Convert the hybrids shape coords into an array of strings that each end with a comma
		// (except for the last shape). The first result will be empty because the hybrid coords
		// begin with "-1" and so the split will produce an array that starts with an empty string.
		let shapes = hybridMarker.shapeCoords.split("-1,");

		let pseudoMarkers = [];
		let pseudoMarkerId = 0;

		// Extract the shape's type and its coords.
		for (let shapeCoords of shapes)
		{
			// Skip past the empty string.
			if (shapeCoords.length === 0)
				continue;

			// Strip off the trailing comma.
			if (shapeCoords.endsWith(','))
				shapeCoords = shapeCoords.substring(0, shapeCoords.length - 1);

			// The shape type will now be the first 1-character integer in the string.
			let shapeType = parseInt(shapeCoords.substring(0, 1), 10);

			// Strip off the type to get the actual shape coords.
			shapeCoords = shapeCoords.substring(2);

			let coords = hybridMarker.shapeCoordsToShapeCoordsArray(shapeCoords);

			// Create a pseudo marker from the shape and add the marker to the list of markers
			// that will be used while editing the hybrid.
			pseudoMarkerId += 1;
			const pseudoMarker = this.createHybridPseudoMarker(shapeType, coords, pseudoMarkerId, hybridMarker);
			pseudoMarkers.push(pseudoMarker);
		}

		return pseudoMarkers;
	}

	createMapLayers(navButtonIsOnMapLeft)
	{
		//console.log(`Map::createMapLayers`);

		this.markerLayer = this.createCanvasLayer(this.tour.uniqueId('MarkerLayer'));
		this.markerLayerContext = this.getCanvasContext(this.markerLayer);

		this.mapLayer = this.createCanvasLayer(this.tour.uniqueId('MapLayer'));
		this.mapLayerContext = this.getCanvasContext(this.mapLayer);

		if (this.mapZoomEnabled)
			this.createZoomControls(navButtonIsOnMapLeft);

		if (this.mapInsetEnabled)
			this.createCanvasLayerForMapInset();

		if (this.tour.editMode)
		{
			this.editLayer = this.createCanvasLayer('EditLayer');
			this.editLayerContext = this.getCanvasContext(this.editLayer);
			this.handlesLayer = this.createCanvasLayer('HandlesLayer');
			this.handlesLayerContext = this.getCanvasContext(this.handlesLayer);

			// Prevent the edit and handles layers from being the target of mouse and touch events
			// so that those events will get sent to the underlying marker layer. Thes layers are
			// only used for drawing selected marker's outline and cross hairs, and a shapes handles.
			this.editLayer.style.pointerEvents = "none";
			this.handlesLayer.style.pointerEvents = "none";
		}

		// Create a place to display a status message while the map is loading. Style it here instead of
		// using XSLT CSS, because the status area also displays in the Map Editor which does not use that CSS,
		this.statusArea = document.createElement('div');
		this.statusArea.style.color = '#ccc';
		this.statusArea.style.position = 'fixed';
		this.statusArea.style.marginTop = '40px';
		this.statusArea.style.width = '400px';
		this.statusArea.style.textAlign = 'center';

		// Create an offscreen hit layer that will be used to detect if the pointer is over a marker.
		this.hitLayer = document.createElement('canvas');
		this.hitLayerContext = this.getCanvasContext(this.hitLayer);
		this.hitLayer.width = this.canvasW;
		this.hitLayer.height = this.canvasH;

		// Now that the canvas layers exist, create the graphics object that will do drawing for this map.
		this.graphics = new Graphics__$$(this.tour, this);
	}

	createMarkerArrayFromMarkerIdList(markerIdList)
	{
		// Make sure the list is a string. If a single numeric element was passed, convert it to a string.
		markerIdList = markerIdList + "";

		let markers = new Array();
		if (markerIdList.length > 0)
		{
			let marker;
			let index;

			if (markerIdList === "*")
			{
				for (index in this.markers)
				{
					marker = this.markers[index];
					markers.push(marker);
				}
			}
			else
			{
				let list = markerIdList.split(",");
				for (index in list)
				{
					let viewId = parseInt(list[index]);
					marker = this.getMarker(viewId, false);
					if (marker !== null)
						markers.push(marker);
				}
			}
		}
		return markers;
	}

	createMarkerLayerCache(force = false)
	{
		Runtime__$$.assert(this.markerLayerCacheEnabled, "Call to create cache when cache disabled");

		// Don't create the cache when a marker is selected because doing so would cause
		// the marker's selected appearance to get cached. 
		if (this.markerIsSelected)
		{
			//console.log(`Map::createMarkerLayerCache IGNORE REQUEST TO CREATE CACHE`);
			return;
		}

		//console.log(`Map::createMarkerLayerCache`);

		// Ignore the request when the marker layer has not yet been sized. This can happen when API
		// calls make requests e.g.to change the appearance of a marker, before the map has been sized. 
		if (this.markerLayer.width === 0 || this.markerLayer.height === 0)
			return;

		// Don't create the cache using a low resolution image. Doing so would create a fuzzy cache image.
		// However, if the caller says it's okay, force the cache to get created anyway.
		if (!force && this.drawState && this.drawState.resolution !== this.drawState.resolutionNeeded)
			return;

		//console.log(`Map::createMarkerLayerCache CREATE CACHE`);
        this.markerLayerCache = document.createElement('canvas');
        this.markerLayerCache.width = this.markerLayer.width;
        this.markerLayerCache.height = this.markerLayer.height;
		this.getCanvasContext(this.markerLayerCache).drawImage(this.markerLayer, 0, 0);
    }

	createMarkers()
	{
		//console.log(`Map::createMarkers`);

		this.allMarkerImagesLoaded = false;
		this.waitForMarkerImagesAttempts = 0;

		let markerTable = this.page.markerInstanceTable;
		this.markers = [];

		for (let index = 0; index < markerTable.length; index++)
		{
			let data = markerTable[index].split(',');
			let markerId = data[31];
			let viewId = data[0];
			let flags = parseInt(data[20], 10);

			let pctX = parseFloat(data[5]);
			let pctY = parseFloat(data[6]);

			if (this.ignoreMarker(pctX, pctY, flags))
			{
				//console.log(`Map::createMarkers ${viewId} IGNORED`);
				continue;
			}

			let markerType = parseInt(data[1], 10);
			let styleId = parseInt(data[2], 10);
			let normalSymbolId = parseInt(data[3], 10);
			let selectedSymbolId = parseInt(data[4], 10);
			let shapeType = parseInt(data[7], 10);
			let shapeW = parseInt(data[8], 10);
			let shapeH = parseInt(data[9], 10);
			let normalSymbolW = parseInt(data[10], 10);
			let normalSymbolH = parseInt(data[11], 10);
			let selectedSymbolW = parseInt(data[12], 10);
			let selectedSymbolH = parseInt(data[13], 10);
			let symbolLocationX = parseInt(data[14], 10);
			let symbolLocationY = parseInt(data[15], 10);
			let centerOffsetX = parseInt(data[16], 10);
			let centerOffsetY = parseInt(data[17], 10);
			let rotationDegrees = parseInt(data[18], 10);
			let zoomThreshold = parseInt(data[19], 10);
			let tooltip = this.page.lookupString(data[21]);
			let clickAction = parseInt(data[22], 10);
			let clickActionTarget = this.page.lookupString(data[23]);
			let mouseoverAction = parseInt(data[24], 10);
			let mouseoverActionTarget = this.page.lookupString(data[25]);
			let mouseoutAction = parseInt(data[26], 10);
			let mouseoutActionTarget = this.page.lookupString(data[27]);
			let touchPerformsClickAction = parseInt(data[28], 10);
			let showContentEvent = parseInt(data[29], 10);
			let shapeCoords = this.page.lookupString(data[30]);

			let marker = new Marker__$$(
				this,
				markerId,
				viewId,
				markerType,
				styleId,
				normalSymbolId,
				selectedSymbolId,
				pctX,
				pctY,
				shapeType,
				shapeW,
				shapeH,
				normalSymbolW,
				normalSymbolH,
				selectedSymbolW,
				selectedSymbolH,
				symbolLocationX,
				symbolLocationY,
				centerOffsetX,
				centerOffsetY,
				rotationDegrees,
				zoomThreshold,
				flags,
				tooltip,
				clickAction,
				clickActionTarget,
				mouseoverAction,
				mouseoverActionTarget,
				mouseoutAction,
				mouseoutActionTarget,
				touchPerformsClickAction,
				showContentEvent,
				shapeCoords);

			// Assign the marker instance to the ordered list of marker instances.
			// They are in stacking order from bottom to top.
			this.markers.push(marker);
		}

		this.createDefaultMarker();
	}

	createMarkerStyles()
	{
		// Create a table of MapsAliveMarkerStyle objects from the table of raw data entries provided by the
		// Tour Builder. Example raw entry: "358859,1,#669044,#9c5f0c,#76abff,#3465a4,99,100,99,100,17,17".
		// Parse each entry into string or integer values to create a MapsAliveMarkerStyleProperties object.

		this.markerStyles = [];
		let markerStyleTable = this.page.markerStyleTable;

		for (const entry of markerStyleTable)
		{
			let data = entry.split(',');
			let id = parseInt(data[0], 10);
			let lineWidth = parseInt(data[1], 10);

			let normalProperties = new MarkerStyleProperties__$$(
				this,
				data[2],
				parseInt(data[6], 10),
				data[3],
				parseInt(data[7], 10),
				this.page.lookupString(data[10])
			);

			let selectedProperties = new MarkerStyleProperties__$$(
				this,
				data[4],
				parseInt(data[8], 10),
				data[5],
				parseInt(data[9], 10),
				this.page.lookupString(data[11])
			);

			let markerStyle = new MarkerStyle__$$(id, lineWidth, normalProperties, selectedProperties);
			this.markerStyles.push(markerStyle);
		}
	}

	createTooltipElement()
	{
		// This method gets called each time a map is loaded. Since all maps share the same tooltip element,
		// it creates the element when the first map loads. Then it applies the tooltip styles for the first
		// map. When a new map loads, it applies the new map's tooltip styles to the existing tooltip element.

		let id = this.tour.uniqueId('Tooltip');
		let e = document.getElementById(id);
		let style;

		// Create the tooltip element if it does not already exists.
		if (e === null)
		{
			e = document.createElement("div");
			e.id = id;
			e.className = "maTooltip";
			style = e.style;
			style.zIndex = 10000;
			style.visibility = "hidden";
			style.position = "absolute";
			document.body.appendChild(e);
		}

		// Assign the same element to each map for easy access e.g. to set the tooltip text.
		this.tooltipElement = e;

		// Apply the tooltip styles for the map that just loaded.
		let tooltip = this.page.tooltip;
		style = e.style;
		style.border = tooltip.border;
		style.color = tooltip.color;
		style.backgroundColor = tooltip.bgColor;
		style.fontSize = tooltip.fontSize;
		style.fontFamily = tooltip.fontFamily
		style.padding = tooltip.padding
		style.fontWeight = tooltip.fontWeight
		style.fontStyle = tooltip.fontStyle
		style.textDecoration = tooltip.textDecoration
		if (tooltip.maxWidth > 0)
			style.maxWidth = tooltip.maxWidth;
	}

	createZoomControls(navButtonIsOnMapLeft)
	{
		const controlSize = '22px';
		const tour = this.tour;

		// Inline these styles instead of using CSS from the XSLT because the Map Editor also
		// uses these controls, but does not use the CSS. This way a change made here will
		// apply in both a tour and in the Map Editor.
		function setControlStyle(style, top, color)
		{
			style.position = 'absolute';
			style.cursor = 'pointer';
			style.border = 'solid 1px #000';
			style.userSelect = 'none';
			style.backgroundColor = color;
			style.width = controlSize;
			style.height = controlSize;
			style.top = top + 'px';
			style.left = '6px';
			style.backgroundSize = 'contain';
			style.visibility = 'hidden';
		}

		function createControl(id, graphicName, top, color)
		{
			let control = document.createElement('div');
			control.id = tour.uniqueId(id);
			setControlStyle(control.style, top, color);
			control.style.backgroundImage = `url(${tour.graphics[graphicName].src})`;
			return control;
		}

		const editModeColor = this.currentMapScale > 1.0 ? this.OVER_ZOOM_COLOR : this.NORMAL_ZOOM_COLOR;
		const zoomInColor = this.tour.editMode ? editModeColor : this.zoomInOutControlColor;
		let top = this.tour.hasNavPanel && !this.tour.navButtonHidden && navButtonIsOnMapLeft ? this.ZOOM_CONTROL_SIZE + 12 : 8;
		this.zoomInControl = createControl('ZoomInControl', 'zoomIn', top + 6, zoomInColor);
		this.zoomOutControl = createControl('ZoomOutControl', 'zoomOut', top + this.ZOOM_CONTROL_SIZE + 12, this.NORMAL_ZOOM_COLOR);
	}

	ctrlKey(event)
	{
		// Treat the Mac Command key (also the Windows key) and the Ctrl key as the same.
		return event && (event.metaKey || event.ctrlKey);
	}

	get currentMapScale()
	{
		return this.mapScale_;
	}

	set currentMapScale(value)
	{
		if (value === this.mapScale_)
			return;
		//console.log(`Map::currentMapScale ${this.mapScale_} >>> ${value}`);
		this.flushMarkerLayerCache();
		this.mapScale_ = value;
	}

	get delayBeforeDeselectingMarker()
	{
		return this.page.hasPopup && this.popup.delayType === this.popup.DELAY_BEFORE_CLOSING_POPUP && this.popup.delay > 0;
	}

	get delayBeforeDeselectingMarkerTimerRunning()
	{
		return this.popupDelayTimerId > 0 && this.popup.delayType === this.popup.DELAY_BEFORE_CLOSING_POPUP;
	}

	get delayBeforeShowingPopup()
	{
		return this.page.hasPopup && this.popup.delayType === this.popup.DELAY_BEFORE_SHOWING_POPUP && this.popup.delay > 0;
	}

	get delayBeforeShowingPopupTimerRunning()
	{
		return this.popupDelayTimerId > 0 && this.popup.delayType === this.popup.DELAY_BEFORE_SHOWING_POPUP;
	}

	deselectMarker()
	{
		// This method deselected the currently selected marker.

		if (!this.markerIsSelected || this.page.isDataSheet)
			return;

		//console.log(`Map::deselectMarker ${this.selectedMarkerViewId}`);

		this.page.stopWaitingForViewContentToLoad(false);

		let marker = this.getMarker(this.selectedMarkerViewId);

		marker.visited = true;
		marker.appearsSelected = false;

		if (this.page.hasPopup || this.page.layout.usingMobileLayout)
		{
			this.executeMouseOutAction(this.selectedMarker);

			// For popup tours, a mouseout deselects the marker and thus the hotspot changes to not selected.
			// For tiled tours, a marker is always selected and so mousing off of a marker does not change
			// which hotspot is selected.
			this.tour.api.callbackHotspotChanged(null);
		}

		this.selectedMarker = null;

		if (!marker.isStatic)
		{
			// When the map uses the Visited Marker Opacity feature, flush the marker cache because this marker's
			// deselected appearance is going to change from 100% opacity to partial opacity, but the cached
			// marker layer shows the marker at 100% opacity. If the cache doesn't get flushed now, the opacity
			// change won't take effect until the next time all markers get redrawn e.g. when the user pans the map.
			if (this.visitedMarkerAlpha < 1.0)
				this.flushMarkerLayerCache();

			this.drawMarkerAndNeighbors(marker, false);
		}
	}

	deselectMarkerAndClosePopup()
	{
		this.deselectMarker();
		this.page.closePopup();
	}

	deselectMarkerAndClosePopupAfterDelay()
	{
		//console.log('AFTER TIMER FIRED');
		this.popupDelayTimerId = 0;

		// Handle the case where the pointer moved off of the popup and onto the popup's marker.
		// This can happen when the popup location is center of marker or if the popup or its
		// callout overlaps the marker because it has a negative popup offset. In this situation
		// do nothing, otherwise, the popup will close and then open again as soon as the next 
		// mouse-move event occurs over the marker. However, if the user moves the mouse off of
		// the popup and then stops moving it during the delay period, before this method has been,
		// called, after the delay expires and this this methods gets called, the popup will close
		// and stay closed but as soon as the mouse moves again, the popup will open again. To
		// avoid the resulting flicker, detect the situation and don't close the popup.
		if (this.markerUnderPointer && this.markerUnderPointer.viewId === this.selectedMarker.viewId)
			return;

		this.deselectMarkerAndClosePopup();
	}

	detectPointerIsOverMarker(marker, pointer)
	{
		if (marker === null)
			return false;

		//console.log(`MapsAliveMarker::detectPointerIsOverMarker Pointer is over hit test area for ${marker.viewId}`);
		if (marker.boundsContainPoint(pointer))
		{
			// Determine if the pointer is over the marker by drawing the marker's shape to an off-screen
			// canvas and then reading back from that canvas the pixel under the pointer. If the pixel is
			// is visible we have a hit.
			let ctx = this.hitLayerContext;
			ctx.clearRect(0, 0, this.canvasW, this.canvasH);
			this.graphics.drawShape(marker, ctx, false);

			let imageData;
			try
			{
				imageData = ctx.getImageData(pointer.x, pointer.y, 1, 1);
			}
			catch (error)
			{
				console.log("ERROR " + error);
				console.log(">>> " + pointer.x + "," + pointer.y);
				return false;
			}

			if (imageData.data[3] > 0)
			{
				//console.log("Map::detectPointerIsOverMarker " + marker.viewId + ': ' + pointer.x + "," + pointer.y);
				// There is a pixel under the pointer on the hit layer.
				return true;
			}

			// The pointer is within the marker's hit bounds, but not over the marker itself.
			return false;
		}
		else
		{
			// The mosue is not within the marker's hit bounds.
			return false;
		}
	}

	disableMarkerDrawing(disable, eraseMarkers = false)
	{
		this._disableMarkerDrawing = disable;
		if (disable)
		{
			if (eraseMarkers)
				this.eraseMarkerLayer()
		}
		else
		{
			this.flushMarkerLayerCache();
			this.drawAllMarkers();
		}
	}

	drawAllMarkers(useCache = false)
	{
		if (this.markerDrawingIsDisabled)
			return;

		if (this.waitingForMarkerImagesToLoad())
			return;

		//console.log(`Map::drawAllMarkers START ${Date.now()} ms`);

		let time = Date.now();

		if (useCache && this.markerLayerCacheEnabled && !this.markerLayerCacheIsStale)
		{
			this.drawCachedMarkerLayer();
		}
		else
		{
			//console.log("NOT USING CACHE");
			this.eraseMarkerLayer()

			for (let index in this.markers)
			{
				let marker = this.markers[index];
				this.graphics.drawMarker(marker, marker.viewId === this.selectedMarkerViewId);
			}
		}

		if (this.markerLayerCacheEnabled && this.markerLayerCacheIsStale)
			this.createMarkerLayerCache();

		this.drawRoutes();

		//console.log(`Map::drawAllMarkers END ${Date.now() - time}ms`);
	}

	drawCachedMarkerLayer()
	{
		Runtime__$$.assert(this.markerLayerCache !== null, "drawCachedMarkerLayer: cache is null");
		Runtime__$$.assert(this.markerLayerCacheEnabled, "drawCachedMarkerLayer: cache is disabled");

		//console.log(`Map::drawCachedMarkerLayer`);
		this.eraseMarkerLayer()
		this.markerLayerContext.drawImage(this.markerLayerCache, 0, 0);
	}

	drawControlsForMapInset()
	{
		if (!this.mapInsetEnabled)
			return;

		// Hide the inset when the map can't zoom because the browser window is bigger than the full size map.
		// Hide the inset if the map is so narrow that the inset covers more than half the map.
		let hideInset = this.zoomingDisabled || this.mapInsetSize > Math.round(this.canvasW * 0.50);
		this.insetLayer.style.display = hideInset ? "none" : "block";
		if (hideInset)
			return;

		//console.log(`Map::drawControlsForMapInset`);

		let ctx = this.insetLayerContext;

		// Erase the control.
		ctx.clearRect(0, 0, this.mapInsetRect.w, this.mapInsetRect.h);

		// Draw the inset border. Shift x and y by half a pixel so that the border doesn't get anti-aliased.
		let x = 0.5;
		let y = 0.5;
		ctx.drawImage(this.mapInsetImage, x, y);
		ctx.lineWidth = 1.0;
		ctx.strokeStyle = "#777777";
		ctx.strokeRect(x, y, this.mapInsetRect.w - 1, this.mapInsetRect.h - 1);

		// Calculate the offset of the zoom region within the map inset.
		x += Math.round(-this.panX_map * this.mapInsetScale);
		y += Math.round(-this.panY_map * this.mapInsetScale);

		// Calculate the size of the zoom region which can be wider or taller than the map inset.
		// That's because the zoom region's aspect ratio is the same as the canvas's aspect ratio
		// whereas the inset's aspect ratio is the same as the map's apect ratio. Note also that the
		// zoom region's scale is the reciprocal of the map's scale. That's what makes the zoom
		// region get smaller as the map scale gets larger (approaches 100%) and vice-versa.
		let zoomRegionScale = (1 / this.currentMapScale) * this.mapInsetScale;
		let zoomRegionW = Math.round(this.canvasW * zoomRegionScale);
		let zoomRegionH = Math.round(this.canvasH * zoomRegionScale);

		// Clip away the right or bottom part of the zoom region that overflows the inset area.
		zoomRegionW -= Math.max(0, x + zoomRegionW - this.mapInsetRect.w);
		zoomRegionH -= Math.max(0, y + zoomRegionH - this.mapInsetRect.h);

		// Record the current zoom region for use in hit testing to see if the pointer is over it.
		this.mapInsetZoomRect = { x: x, y: y, w: zoomRegionW, h: zoomRegionH }

		// Draw the zoom region.
		ctx.lineWidth = 1.0;
		ctx.globalAlpha = 0.05;
		ctx.fillStyle = "#000";
		ctx.fillRect(x, y, zoomRegionW, zoomRegionH);
		ctx.globalAlpha = 1.0;
		ctx.strokeStyle = this.mapInsetColor;
		ctx.strokeRect(x, y, zoomRegionW, zoomRegionH);
	}

	drawControlsForZooming()
	{
		if (!this.mapZoomEnabled)
			return;

		this.drawControlForZooming(this.zoomInControl);
		this.drawControlForZooming(this.zoomOutControl);
	}

	drawControlForZooming(control)
	{
		let zoomIn = control === this.zoomInControl;

		let disableZoomIn = this.zoomedAllTheWayIn;
		let disableZoomOut = this.zoomedAllTheWayOut;

		let opacity = 1.0;

		if (disableZoomIn && disableZoomOut)
			opacity = 0;
		else if (zoomIn && disableZoomIn || !zoomIn && disableZoomOut)
			opacity = 0.3;

		control.style.opacity = opacity;
		control.style.visibility = opacity > 0 ? 'visible' : 'hidden';
	}

	drawHybridMarkerOutline()
	{
		if (!this.tour.editMode)
			return;

		if (!this.editor.hybridEditingEnabled)
			return;

		this.editor.redrawSelectedMarkers();
		this.editor.drawOutlineForMarker(this.editor.hybridMarkerBeingEdited);
	}

	drawMap()
	{
		// This method quickly draws the map and then sets a timer to draw it again at higher quality after a delay period
		// has elapsed since this method was last called. If this method is called again before the timer fires, clear the
		// timer, draw the map, and then set the timer again. This approach provides high performance when zooming and
		// panning the map, and defers the slower higher quality drawing until after panning or zooming stops. On slow
		// devices that don't have a lot of CPU power, like the Galaxy A01 phone, Fire 8 tablet, or Acer Chromebook Spin 311,
		// it can a full second to resample and sharpen the map image which is far too long when zooming or panning. 
		const delayMs = 200;

		if (this.drawingMap)
			return;

		// Ignore a request to draw the map while a marker is dropping onto the map.
		if (this.tour.editMode && this.editor.animatingMarker)
			return;

		this.drawingMap = true;

		this.clearDrawMapDelayTimer();

		// Draw the map image. The method will return false if nothing has changed since the last draw.
		let draw = this.drawMapImage();

		if (draw)
		{
			if (this.drawState.resolution === this.drawState.resolutionNeeded)
			{
				// Set the timer for drawing the higher quality map image.
				this.drawMapDelayTimerId = setTimeout(this.drawMapImageHighQuality, delayMs);
			}
			else
			{
				// The map had to be drawn using a lower resolution image because the 100% resolution
				// image had not yet loaded. Set the timer to attempt to draw the map again at 100%.
				//console.log(`Map::drawMap Waiting for 100% resolution image to load`);
				this.drawMapDelayTimerId = setTimeout(this.drawMap, delayMs);
			}

			this.drawAllMarkers();
			this.drawMapControls();
		}

		this.drawingMap = false;
	}

	drawMapImage()
	{
		// This method quickly draws the map image. Though it performs a number of calculations, the results are
		// used to optimize drawing performance, and are use later when drawing the image again at higher quality.

		let resolution;
		let resolutionNeeded;
		let mapImage;
		let mapPixels;
		let canvasMask;
		let pan;
		let canvasToMapPixelRatio = 1 / this.currentMapScale;
		let hasDrawState = this.drawState !== null;

		function calculateCanvasMaskSize(_this)
		{
			// Determine the dimensions of the visible portion of the scaled map on the canvas. If the map is zoomed in far enough,
			// the canvas mask will have same dimensions as the canvas. If the map is only partially zoomed in and the, canvas and
			// map aspect ratios are different, the width or height of the canvas mask may be narrower or shorter than the canvas.

			let mapScaleChanged;
			let scaledMapW;
			let scaledMapH;

			_this.calculateZoomedOutMapScale();

			if (_this.page.isGallery)
			{
				_this.currentMapScale = _this.zoomedOutMapScale;
				mapScaleChanged = true;
			}
			else
			{
				// Use the current scale. The scale should only ever be zero when going
				// to the Map Editor for the first time after uploading a map image.
				if (_this.currentMapScale === 0)
				{
					Runtime__$$.assert(_this.tour.editMode, "Current map scale is 0");
					_this.currentMapScale = _this.zoomedOutMapScale;
					mapScaleChanged = true;
				}
				else
				{
					mapScaleChanged = hasDrawState && _this.drawState.scale !== _this.currentMapScale;
				}
			}

			let newCurrentMapScale = _this.currentMapScale;
			let canvasGotNarrower = false;
			let canvasGotWider = false;

			// Determine if the user made the desktop canvas narrower or wider since the previous draw.
			if (hasDrawState && !_this.page.layout.usingMobileLayout)
			{
				canvasGotNarrower = _this.canvasW < _this.drawState.canvas.w;
				canvasGotWider = _this.canvasW > _this.drawState.canvas.w;
			}

			// Determine if, after the previous draw, the map extended across the full width of the canvas with no gap on
			// the right side as though the right edge of the map was "glued" to the right edge of the canvas. However, if
			// the map was all the way zoomed out on the previous draw, don't make the edge sticky because the zoomed out
			// state is maintained until the user explicitly zooms the map in.
			let canvasRightEdgeIsSticky = hasDrawState && !_this.drawState.zoomedAllTheWayOut && _this.drawState.canvas.w === _this.drawState.canvasMask.w;

			// When the map's right edge is "glued" to right right side of the canvas, and the canvas has gotten wider or
			// narrower since the previous draw, zoom the map in or out so that its right edge stays glued to the right
			// side of the canvas. This "sticky" drawing is not done when there is a gap between the map and the right
			// side of the canvas, as can be the case when the map is all the way zoomed out, because doing so would
			// cause the map to instantly grow to fill the canvas width and "snap" to the right edge of the canvas.
			if (canvasRightEdgeIsSticky && (canvasGotWider || canvasGotNarrower))
			{
				// Calculate the percentage increase or decrease in the canvas width since the previous draw.
				let delta = _this.canvasW / _this.drawState.canvas.w;

				// Increase or decrease the map scale to match the change in the canvas width.
				newCurrentMapScale = delta * _this.currentMapScale;
				newCurrentMapScale = Runtime__$$.roundFloat(newCurrentMapScale);

				//console.log(`Map::calculateCanvasMaskSize WIDTH CHANGED ${_this.currentMapScale} >>> ${newCurrentMapScale} `)

				// Prevent the scale from going past 100% in the case where the canvas got wider, but the maps was already at 100%
				if (canvasGotWider && newCurrentMapScale > 1.0)
					newCurrentMapScale = 1.0;

				if (newCurrentMapScale !== _this.currentMapScale)
					mapScaleChanged = true;
			}
			else if (hasDrawState &&  !_this.zoomingMap && _this.drawState.zoomedAllTheWayOut)
			{
				// The canvas edge is not sticky and the map was zoomed all the way out on the previous draw, so maintain
				// the zoomed out state when the user widens or narrows the browser window, but not when they are zooming.
				newCurrentMapScale = _this.zoomedOutMapScale;
				mapScaleChanged = true;
			}

			// Convert the map dimensions to screen dimensions.
			scaledMapW = _this.convertMapToScreen(_this.mapW_actual);
			scaledMapH = _this.convertMapToScreen(_this.mapH_actual);

			// Update the scaled map dimensions to match the new map scale.
			if (mapScaleChanged)
			{
				//console.log(`Map::calculateCanvasMaskSize SCALE CHANGED ${mapScaleChanged} ${_this.currentMapScale} >>> ${newCurrentMapScale} `)
				_this.currentMapScale = newCurrentMapScale;
				canvasToMapPixelRatio = 1 / _this.currentMapScale;

				// Convert the map dimensions to screen dimensions again since the map scale just changed.
				scaledMapW = _this.convertMapToScreen(_this.mapW_actual);
				scaledMapH = _this.convertMapToScreen(_this.mapH_actual);
			}

			// Create the canvas mask to specify the portion of the scaled map that is visible within the canvas.
			// Since the visible area may be narrower and/or shorter than the canvas, use the smaller values.
			canvasMask = { w: Math.min(_this.canvasW, scaledMapW), h: Math.min(_this.canvasH, scaledMapH) };

			//console.log(`Map::calculateCanvasMaskSize ${canvasMask.w}x${canvasMask.h} ${_this.currentMapScale}`);
		}

		function chooseMapImageResolution(_this)
		{
			// Determine which of the map images generated by the Tour Builder has enough resolution to display
			// the visible portion of the map at the current zoom level. The lowest resolution image is used when
			// the map is zoomed out and the highest resolution when it is zoomed in.
			const resolution25 = 0.25;
			const resolution50 = 0.50;
			const resolution100 = 1.00;

			if (canvasToMapPixelRatio >= 1 / resolution25 && !_this.useFullSizeImage)
			{
				resolution = resolution25;
				resolutionNeeded = resolution;
				mapImage = _this.mapImage25;
			}
			else if (canvasToMapPixelRatio >= 1 / resolution50 && !_this.useFullSizeImage)
			{
				resolution = resolution50;
				resolutionNeeded = resolution;
				mapImage = _this.mapImage50;
			}
			else
			{
				// Determine if the 100% resolution image has been loaded and is ready to use.
				_this.mapImage100Loaded = _this.mapImage100 !== null && _this.mapImage100.complete && _this.mapImage100.naturalHeight !== 0;

				if (_this.mapImage100Loaded)
				{
					resolution = 1.0;
					resolutionNeeded = resolution;
					mapImage = _this.mapImage100;
				}
				else
				{
					// The 100% resolution image is not available. If it has not been loaded, load it.
					if (_this.mapImage100 === null)
						_this.mapImage100 = _this.loadMapImage(100);

					// Use the 50% image for now.
					resolution = resolution50;
					resolutionNeeded = resolution100;
					mapImage = _this.mapImage50;
				}
			}

			pan = { x: Math.round(-_this.panX_map * resolution), y: Math.round(-_this.panY_map * resolution) };
		}

		function calculateMapPixels()
		{
			// Calculate how many pixels are needed at the chosen resolution. 
			let canvasMaskAspectRatio = canvasMask.w / canvasMask.h;

			let w, h;
			if (canvasMaskAspectRatio >= 1.0)
			{
				h = canvasMask.h * canvasToMapPixelRatio;
				w = h * canvasMaskAspectRatio;
			}
			else
			{
				w = canvasMask.w * canvasToMapPixelRatio;
				h = w / canvasMaskAspectRatio;
			}

			// Round to integers for best drawing performance.
			mapPixels = { w: Math.round(w * resolution), h: Math.round(h * resolution) };
		}

		function clearCanvas(_this, canvasMask)
		{
			// Clear part or all of the canvas by filling it with the map area background color.

			_this.mapLayerContext.fillStyle = _this.tour.enableV3Compatibility ? '#ffffff' : _this.tour.mapAreaBackgroundColor;

			if (_this.page.layout.usingMobileLayout)
			{
				// Clear the entire mobile canvas in case it had been over-panned to show a marker that would otherwise
				// get covered by its content panel. Rather than figure out what part of the canvas is made empty by the
				// over-pan, just clear the entire canvas. This also works correctly when over-panning has not occurred.
				_this.mapLayerContext.clearRect(0, 0, _this.canvasW, _this.canvasH);
				_this.mapLayerContext.fillRect(0, 0, _this.canvasW, _this.canvasH);
			}
			else
			{
				// Clear only the the right or bottom portion of the canvas that won't get drawn upon so that no pixels are
				// leftover from the previous draw. This is necessary when the map is being zoomed out and becoming smaller
				// and thus covering less of the canvas than for the previous draw.

				if (canvasMask.w < _this.canvasW)
					_this.mapLayerContext.fillRect(canvasMask.w, 0, _this.canvasW - canvasMask.w, _this.canvasH);

				if (canvasMask.h < _this.canvasH)
					_this.mapLayerContext.fillRect(0, canvasMask.h, _this.canvasW, _this.canvasH - canvasMask.h);
			}
		}

		function ignoreDrawRequest(_this)
		{
			if (_this.tour.editMode)
				return false;

			let state = _this.drawState;

			return hasDrawState &&
				state.resolution === state.resolutionNeeded &&
				state.scale === Runtime__$$.roundFloat(_this.currentMapScale) &&
				state.pan.x === pan.x && state.pan.y === pan.y &&
				state.canvasMask.w === canvasMask.w &&
				state.canvasMask.h === canvasMask.h &&
				state.mapSize.w === _this.page.layout.mapSize.w &&
				state.mapSize.h === _this.page.layout.mapSize.h;
		}

		function saveDrawState(_this)
		{
			//console.log(`Map::saveDrawState ${_this.page.layout.mapSize.w}x${_this.page.layout.mapSize.h}`);

			_this.drawState = {
				mapImage,
				scale: Runtime__$$.roundFloat(_this.currentMapScale),
				pan,
				resolution,
				resolutionNeeded,
				mapPixels: Object.assign({}, mapPixels),
				canvas: { w: _this.canvasW, h: _this.canvasH },
				canvasMask: Object.assign({}, canvasMask),
				mapSize: Object.assign({}, _this.page.layout.mapSize),
				zoomedAllTheWayOut: _this.zoomedAllTheWayOut
			};
		}

		calculateCanvasMaskSize(this);
		chooseMapImageResolution(this);
		calculateMapPixels();

		if (ignoreDrawRequest(this))
		{
			//console.log(`Map::drawMapImage IGNORE DRAW REQUEST`);
			return false;
		}

		clearCanvas(this, canvasMask);
		saveDrawState(this);

		// Quickly draw the map image onto the canvas. The drawImage method will automatically scale the larger image
		// to fit the smaller canvas. Because the map image with the closest resolution is used, and because the 50% and
		// 25% image were created by the Tour Builder using high quality scaling, even this quick draw produces a very
		// good result and, based on timings on various devices, seems to take less than 1ms even on slow devices.
		try
		{
			this.mapLayerContext.drawImage(mapImage, pan.x, pan.y, mapPixels.w, mapPixels.h, 0, 0, canvasMask.w, canvasMask.h);
		}
		catch (error)
		{
			/**/console.log("ERROR " + error);
			let message = "An error occurred while trying to draw the map image. This can happen if someone recently changed this interactive tour.\n\n";
			message += "Press OK to run the updated tour.\n\nIf this error persists, press Cancel and please report the problem.";
			if (confirm(message))
				location.reload();
			return false;
		}

		//console.log(`Map::drawMapImage ${this.currentMapScale} ${resolution * 100}% Mask:${canvasMask.w}x${canvasMask.h} Canvas:${this.canvasW}x${this.canvasH} Map:${mapPixels.w}x${mapPixels.h}  Pan:${pan.x} ${pan.y}`);

		return true;
	}

	drawMapImageHighQuality()
	{
		this.drawMapDelayTimerId = 0;

		if (this.drawingMap)
			return;

		this.drawingMap = true;

		let mapImage = this.drawState.mapImage;
		let resolution = this.resolution;
		let mapPixels = this.drawState.mapPixels;
		let pan = this.drawState.pan;
		let canvasMask = this.drawState.canvasMask;

		let sharpenTime = 0;
		let resampleTime = 0;

		// Update this code later to use user-specified options.
		let performResample = !this.tour.editMode && !this.tour.flagDisableMapResample && (this.mapImageSharpening === this.MAP_IMAGE_SHARPENING_SOFT || this.mapImageSharpening === this.MAP_IMAGE_SHARPENING_BALANCED);
		let performSharpen = !this.tour.editMode && !this.tour.flagDisableMapSharpen && (this.mapImageSharpening === this.MAP_IMAGE_SHARPENING_SHARP || this.mapImageSharpening === this.MAP_IMAGE_SHARPENING_BALANCED);

		if (performResample)
		{
			// Draw the map image onto an off-screen canvas where its data can be accessed by the resampling logic.
			let resamplingCanvas = document.createElement('canvas');
			let resamplingCanvasCtx = this.getCanvasContext(resamplingCanvas);
			resamplingCanvas.width = mapPixels.w;
			resamplingCanvas.height = mapPixels.h;
			resamplingCanvasCtx.drawImage(mapImage, pan.x, pan.y, mapPixels.w, mapPixels.h, 0, 0, mapPixels.w, mapPixels.h);

			// Resample the off-screen map image down to the dimensions of the virtual canvas. Depending on the map
			// and canvas aspect ratios, either the width or the height of the virtaul canvas may be shorter than the
			// width or height of the actual canvas. The resample method will then reduce the dimensions of the off-screen
			// canvas to the smaller image size. Resampling is done to address the problem where when a large map image
			// is reduced in size, it can get a "crunchy" appearance that looks awful.
			resampleTime = Date.now();
			this.resampleImage(resamplingCanvas, canvasMask.w, canvasMask.h);
			resampleTime = Math.round(Date.now() - resampleTime);

			// Copy the smaller resampled image from the off-screen canvas onto the actual map canvas.
			this.mapLayerContext.drawImage(resamplingCanvas, 0, 0);
		}

		// Slightly sharpen the map when using the lowest resolution (smallest) map image.
		let sharpenAmount = 0.1;
		if (performSharpen)
		{
			if (resolution === 0.5)
				sharpenAmount = 0.2;
			else if (resolution === 0.25)
				sharpenAmount = 0.3;

			// Create an off-screen canvas to use for sharpening.
			let sharpeningCanvas = document.createElement('canvas');
			let sharpeningCanvasCtx = this.getCanvasContext(sharpeningCanvas);
			sharpeningCanvas.width = canvasMask.w;
			sharpeningCanvas.height = canvasMask.h;

			// Copy the unsharpened map image from the map canvas to an off-screen canva and perform the sharpening.
			sharpeningCanvasCtx.drawImage(this.mapLayer, 0, 0);
			sharpenTime = Date.now();
			this.sharpenImage(sharpeningCanvasCtx, canvasMask.w, canvasMask.h, sharpenAmount);
			sharpenTime = Math.round(Date.now() - sharpenTime);

			// Copy the sharpened image onto the canvas except for 2 pixels on the edges to work around
			// a bug in the sharpening code that leaves them very light or white.
			this.mapLayerContext.drawImage(sharpeningCanvas, 2, 2, canvasMask.w - 4, canvasMask.h-4, 2, 2, canvasMask.w-4, canvasMask.h-4);
		}

		//console.log(`Map::drawMapImageHighQuality resample:${resampleTime}ms sharpen:${sharpenTime}ms ${sharpenAmount}`);

		this.drawingMap = false;
	}

	drawMapControls()
	{
		this.drawControlsForZooming();
		this.drawControlsForMapInset();
	}

	drawMarkerAndNeighbors(marker, selected, useCache = true)
	{
		//console.log(`Map::drawMarkerAndNeighbors ${marker.viewId}`);
		this.graphics.drawIntersectingMarkers([marker], selected, useCache);
	}

	drawMarkerOnly(marker, selected)
	{
		this.graphics.drawMarker(marker, selected);
	}

	drawRoute(hotspotId, routeId, lineWidth, lineColor, lineAlpha, effects)
	{
		let view = this.page.getViewByHotspotId(hotspotId);
		let viewId = view ? view.viewId : 0;

		if (!(typeof routeId === 'string'))
			return false;

		// Provide default values for missing parameters.
		if (typeof lineWidth === "undefined")
			lineWidth = 3;
		if (typeof lineColor === "undefined")
			lineColor = 0xcc0000;
		if (typeof lineAlpha === "undefined")
			lineAlpha = 80;
		if (typeof effects === "undefined")
			effects = "";

		// Determine if the 2nd arg is a route Id or a comma-separated list of hotspot Ids.
		let routeIsList = routeId.indexOf(',') >= 0;
		let viewIdList;

		if (routeIsList)
		{
			// Construct the route from the passed-in list of hotspot Ids.
			viewIdList = this.page.createViewIdListFromRouteHotspotIdList(routeId);
		}
		else
		{
			if (typeof this.page.routesTable === "undefined")
				return false;
			let routeRecord = this.page.routesTable[routeId];
			if (!routeRecord)
				return false;

			// Get the route's view Id list from the table.
			viewIdList = routeRecord.route;
		}

		// Draw the route through the list of hotspots.
		return this.graphics.drawRouteThroughMarkers(routeId, viewId, lineWidth, lineColor, lineAlpha, viewIdList, effects);
	}

	drawRoutes()
	{
		for (const route of this.routes)
			this.drawRoute("", route.id, route.lineWidth, route.lineColor, route.lineAlpha, route.effects);
	}

	enterHybridEditingMode(enter, hybridMarker)
	{
		//console.log(`Map::enterHybridEditingMode ${enter}`);

		// Hybrid logic throughout the runtime depends on this.editingHybrid being true from the moment
		// editing mode is entered until the map is redrawn and edting mode is exited. Be extremely
		// careful and test thoroughly when making any modifications to this code and its call sequence.

		if (enter)
		{
			// Enter hybrid editing mode.
			this.editingHybrid = true;

			// Save the map's markers and completely replace them with pseudo markers for the hybrid's shapes.
			this.savedMarkers = this.markers;
			this.markers = this.createHybridPseudoMarkersFromHybrid(hybridMarker);

			// Discard the map's markers and draw the hybrid pseudo markers.
			this.flushMarkerLayerCache();
			this.drawMap();
		}
		else
		{
			// Restore the map's markers from before hybrid editing mode was entered.
			this.createHybridMarkerFromPseudoMarkers(hybridMarker);
			this.markers = this.savedMarkers;
			this.savedMarkers = null;

			// Discard the pseudo markers and draw the map's markers.
			this.flushMarkerLayerCache();
			this.drawMap();

			// Exit hybrid editing mode.
			this.editingHybrid = false;
		}
	}

	eraseMarkerLayer()
	{
		this.markerLayerContext.clearRect(0, 0, this.canvasW, this.canvasH);
	}

	executeClickAction(marker)
	{
		let clickActionTarget = marker.clickActionTarget;

		if (marker.clickActionTarget.length === 0)
			return;

		console.log(`Map::executeClickAction ${marker.viewId}`);

		if (marker.clickAction === this.ACTION_CALL_JAVASCRIPT)
			this.executeJavaScript('click', marker, clickActionTarget);
	}

	executeClickActionToLeavePage(marker)
	{
		// This method returns true if the action will transfer away from the current page.

		let clickActionTarget = marker.clickActionTarget;

		if (marker.clickActionTarget.length === 0)
			return false;

		if (marker.clickAction === this.ACTION_LINK_TO_URL)
		{
			// Prefix the URL with '0' to indicate open the link in the same window.
			this.tour.linkToUrl('0' + clickActionTarget);
			return true;
		}

		if (marker.clickAction === this.ACTION_LINK_TO_URL_IN_NEW_WINDOW)
		{
			// Prefix the URL with '1' to indicate open the link in a new window.
			this.tour.linkToUrl('1' + clickActionTarget);
			return true;
		}

		if (marker.clickAction === this.ACTION_GOTO_PAGE)
		{
			// Hide the tooltip so it won't still be showing on the new page.
			this.hideTooltip();

			// Extract the page number from the target which is the page URL e.g. page1.htm.
			let pageNumber = clickActionTarget.replace('page', '');
			pageNumber = pageNumber.replace('.htm', '');

			this.tour.goToPage(parseInt(pageNumber, 10));
			return true;
		}

		console.log(`Map::executeClickActionToLeavePage ${marker.viewId} FALSE`);

		return false;
	}

	executeMouseOutAction(marker)
	{
		Runtime__$$.assert(marker !== null, 'marker must not be null');
		let script = marker.mouseoutAction === this.ACTION_CALL_JAVASCRIPT ? marker.mouseoutActionTarget : '';
		//console.log(`Map::executeMouseOUTAction  ${marker.viewId} '${script}'`);
		if (script.length > 0)
			this.executeJavaScript('mouseout', marker, script);
	}

	executeMouseOverAction(marker)
	{
		let script = marker.mouseoverAction === this.ACTION_CALL_JAVASCRIPT ? marker.mouseoverActionTarget : '';
		//console.log(`Map::executeMouseOVERAction ${marker.viewId} '${script}'`);
		if (script.length > 0)
			this.executeJavaScript('mouseover', marker, script);
	}

	executeJavaScript(action, marker, script)
	{
		//console.log(`Map::executeJavaScript ${script}`);
		let view = this.tour.currentPage.getView(marker.viewId);
		let event = this.tour.api.createEvent();
		event.action = action;
		event.hotspot = this.tour.api.createHotspotProperties(view);
		window.MapsAlive.callbackEvent = event;
		window.Function(script)();
		window.MapsAlive.callbackEvent = null;
	}

	findMarkersThatUseStyle(styleId)
	{
		let markers = [];
		for (const marker of this.markers)
			if (styleId === marker.styleId)
				markers.push(marker);

		return markers;
	}

	flushMarkerLayerCache()
	{
		if (!this.markerLayerCacheEnabled)
			return;

		// Ignore a request to flush the cache while a marker is dropping onto the map.
		// If it got flushed, it could get created with the marker partially dropped.
		if (this.tour.editMode && this.editor && this.editor.animatingMarker)
			return;

		//console.log(`Map::flushMarkerLayerCache FLUSH CACHE`);
		this.markerLayerCache = null;
	}

	getCanvasContext(canvas)
	{
		return canvas.getContext('2d', { willReadFrequently: true });
	}

	getFirstMarker()
	{
		if (this.markers.length > 0)
			return this.markers[0];
		return null;
	}

	getMarker(viewId, markerMustExist = true)
	{
		// console.log(`Map::getMarker ${viewId}`);

		if (this.markers && this.markers.length > 0)
		{
			for (const marker of this.markers)
			{
				if (marker.viewId === viewId)
					return marker;
			}
		}

		if (markerMustExist)
		{
			// The viewId should be valid except in some special cases such as in the Gallery Editor
			// when the user selects a hotspot from the Hotspot Order list and that hotspot is not
			// in the Gallery, or when a tiled layout map's first hotspot (chosen on the Map Setup)
			// page is not on the map. In those cases, the caller passes false for the must exist
			// flag. In all other cases the viewId is expected to be valid.
			Runtime__$$.assert(false, `Map::getMarker NO MARKER FOUND FOR ${viewId}`);
		}

		return null;
	}

	getMarkerStyle(styleId)
	{
		for (const markerStyle of this.markerStyles)
			if (styleId === markerStyle.id)
				return markerStyle;

		// This marker has no style. Return the default style.
		let normalProperties = new MarkerStyleProperties__$$(this, '#000000', 0, '#000000', 100, '');
		let selectedProperties = new MarkerStyleProperties__$$(this, '#000000', 0, '#000000', 100, '');
		return new MarkerStyle__$$(0, 0, normalProperties, selectedProperties);
	}

	getMarkerUnderPointer(pointer)
	{
		// This method returns the view Id of the marker under the pointer. If there are overlapping
		// markers at the pointer location, it returns the top level marker unless the Shift key is
		// down, in which case it returns the second level marker. There is no way to go any deeper.
		let findSecondLevelMarker = window.event && window.event.shiftKey;
		let topLevelMarker = null;

		// See if the pointer is over the selected marker in the Map Editor. Even if that marker is not
		// a top level, it's probably the one the user is interested in. However, if they are looking
		// for the second level marker, don't give precedence to the selected marker because the user
		// might be looking for the marker beneath the selected marker.
		if (this.tour.editMode && !findSecondLevelMarker)
		{
			let primaryViewId = this.editor.primarySelectedMarkerViewId;
			if (primaryViewId !== 0)
			{
				let marker = this.getMarker(primaryViewId);
				if (this.detectPointerIsOverMarker(marker, pointer))
					return marker;
			}
		}

		// Loop over all marker instances in reverse order so markers are tested from highest to lowest level.
		let count = this.markers.length;
		for (let index = count - 1; index >= 0; index--)
		{
			// Get the next marker in the stacking order.
			let marker = this.markers[index];

			// Ignore markers that are not visible.
			if (!marker.isVisibleOnCanvas || !marker.isVisibleAtCurrentZoomLevel)
				continue;

			if (this.detectPointerIsOverMarker(marker, pointer))
			{
				if (findSecondLevelMarker && topLevelMarker === null)
				{
					// The top level marker has not previoulsy been found and therefore, this is the top level
					// marker. However, we are looking for the second level marker so keep looking deeper.
					topLevelMarker = marker;
				}
				else
				{
					// This is either the top or second level marker that we are looking for.
					return marker;
				}
			}
		}

		if (findSecondLevelMarker && topLevelMarker !== null)
		{
			// The loop above did not find a second level marker, so return the top level marker.
			return topLevelMarker;
		}

		//console.log("Map::getMarkerUnderPointer " + "NONE" + ': ' + pointer.x + "," + pointer.y);
		return null;
	}

	getPointerLocationOnMap(pointerX, pointerY)
	{
		// This method adjusts the raw coordinates of the pointer location within
		// the browser to coordinates relative to the upper left corner of the map.

		if (this.mapElementBoundingClientRect === null)
			this.mapElementBoundingClientRect = this.page.layout.mapElement.getBoundingClientRect();

		let x = pointerX - this.mapElementBoundingClientRect.left;
		let y = pointerY - this.mapElementBoundingClientRect.top;

		x = Math.round(x);
		y = Math.round(y);

		this.mousePointer.x = x;
		this.mousePointer.y = y;

		if (this.tour.flagTrackPointer)
		{
			// Draw a red square at the pointer location.
			this.markerLayerContext.fillStyle = "#ff0000";
			this.markerLayerContext.fillRect(x - 0.5, y - 0.5, 1, 1);
			/**/console.log(`POINTER: ${x},${y} : ${this.convertScreenToMap(x)},${this.convertScreenToMap(y)}`);
		}

		//console.log(`Map::getPointerLocationOnMap ${pointerX},${pointerY} >>> ${x},${y}`);

		return { x: x, y: y };
	}

	handlePointerMovedOffMarker()
	{
		//console.log(`Map::handlePointerMovedOffMarker ${this.selectedMarkerViewId}`);

		if (!this.page.hasPopup)
			return;

		if (this.delayBeforeDeselectingMarker)
		{
			// Set a timer to wait before deselecting the marker. If the timer is already set, then the
			// user must be mousing on and off the same marker. Reset the timer to the full delay period.
			if (this.popupDelayTimerId !== 0)
				this.clearPopupDelayTimer();
			this.popupDelayTimerId = setTimeout(this.deselectMarkerAndClosePopupAfterDelay, this.popup.delay);
		}
		else
		{
			// Deselect the marker immediately, but first clear a timer that is waiting to show the popup.
			// This happens when the user mouses onto a marker and then off of it during the delay period.
			if (this.delayBeforeShowingPopupTimerRunning)
				this.clearPopupDelayTimer();

			this.deselectMarkerAndClosePopup();
		}
	}

	handlePointerStateChange(pointer)
	{
		// Get the marker that is currently under the pointer.
		let previousMarkerUnderPointer = this.markerUnderPointer;
		this.markerUnderPointer = this.getMarkerUnderPointer(pointer);

		//console.log(`Map::handlePointerStateChange ${pointer.x} ${pointer.y} Over marker:${this.pointerIsOverMarker} State:${this.pointerState} Previous:${previousMarkerUnderPointer}`);

		// Let the Map Editor handle his event.
		if (this.tour.editMode)
		{
			this.editor.handleEventPointerOverMapEditor(this.markerUnderPointerViewId, pointer);
			return;
		}

		if (this.pointerIsOverMarker)
		{
			if (this.markerUnderPointer.isDisabled)
			{
				// Treat a disabled marker as though it was not there.
				this.markerUnderPointer = null;
			}
			else if (this.markerUnderPointer.showContentEvent === this.SHOW_CONTENT_ON_CLICK)
			{
				// Ignore a show-on-click marker when the mouse moves over it.
				this.showTooltip(this.markerUnderPointer, pointer);
				this.markerUnderPointer = null;
			}
		}
		else
		{
			this.hideTooltip();
		}

		// Set the cursor to the pointing finger when the mouse is over a marker.
		this.markerLayer.style.cursor = this.pointerIsOverMarker ? "pointer" : "auto";

		// For mobile layouts there's nothing else to do after setting the cursor.
		if (this.page.layout.usingMobileLayout)
			return;

		// Assign convenience variables to make the code below easier to read.
		let pointerIsOverSelectedMarker = this.pointerIsOverMarker && this.markerUnderPointerViewId === this.selectedMarkerViewId;
		let pointerIsOverDifferentMarker = this.pointerIsOverMarker && this.markerUnderPointerViewId !== this.selectedMarkerViewId;

		// Ignore the pointer when showing a pinned popup. Revert to the Start state
		// since the current state may no longer be accurate after the popup gets closed.
		if (this.page.hasPopup && this.popup.isPinned)
		{
			this.pointerState = this.POINTER_STATE_START;
			return;
		}

		let oldState = this.pointerState;

		switch (this.pointerState)
		{
			case this.POINTER_STATE_START:
				if (this.pointerIsOverMarker)
				{
					this.pointerState = this.POINTER_STATE_NEW_MARKER;
					this.pointerMovedFromMapToMarker(pointer);
				}
				else
				{
					this.pointerState = this.POINTER_STATE_MAP;
					this.pointerMovedOverMap();
				}
				break;

			case this.POINTER_STATE_MAP:
				if (this.pointerIsOverMarker)
				{
					this.pointerState = this.POINTER_STATE_NEW_MARKER;
					this.pointerMovedFromMapToMarker(pointer);
				}
				break;

			case this.POINTER_STATE_NEW_MARKER:
			case this.POINTER_STATE_DIFFERENT_MARKER:
				if (pointerIsOverSelectedMarker)
				{
					this.pointerState = this.POINTER_STATE_SAME_MARKER;
					this.pointerMovedOverSelectedMarker(pointer);
				}
				else if (pointerIsOverDifferentMarker)
				{
					this.pointerState = this.POINTER_STATE_DIFFERENT_MARKER;
					this.pointerMovedFromMarkerToMarker(pointer);
				}
				else
				{
					this.pointerState = this.POINTER_STATE_MAP;
					this.pointerMovedFromMarkerToMap(previousMarkerUnderPointer);
				}
				break;

			case this.POINTER_STATE_SAME_MARKER:
				if (pointerIsOverDifferentMarker)
				{
					this.pointerState = this.POINTER_STATE_DIFFERENT_MARKER;
					this.pointerMovedFromMarkerToMarker(pointer);
				}
				else if (!this.pointerIsOverMarker)
				{
					this.pointerState = this.POINTER_STATE_MAP;
					this.pointerMovedFromMarkerToMap(previousMarkerUnderPointer);
				}
				break;
		}

		if (oldState === this.pointerState)
			return;

		//console.log(`Map::handlePointerStateChange ${oldState} >>> ${this.pointerState}`);
	}

	handleTapMarker(viewId, pointer, pointerType)
	{
		//console.log(`Map::handleTapMarker ${viewId} ${pointerType}`);

		this.stopCurrentOperation();

		// When the map is tapped, but not on a marker, deselect the currently selected marker.
		if (!this.pointerIsOverMarker)
		{
			if (this.page.hasPopup)
				this.deselectMarkerAndClosePopup();
			else if (this.page.layout.usingMobileLayout)
				this.page.layout.hideContentPanel();
			return;
		}

		let marker = this.getMarker(viewId);

		// Handle the case on mobile where a marker that displays only a tooltip is tapped.
		// Since it won't display a content panel to replace the current content panel
		// (if one is showing) the tap of this marker needs to close the content panel.
		if (this.page.layout.usingMobileLayout)
		{
			let view = this.page.getView(viewId);
			let markerDoesNotShowContentPanel = marker.showsContentOnlyInTooltip || view.hasNoContentForLayout();
			if (markerDoesNotShowContentPanel)
				this.page.layout.hideContentPanel();
		}

		if (marker.isDisabled)
			return;

		// Execute the marker's click action that goes to another page.
		let leavePage = this.executeClickActionToLeavePage(marker);
		if (leavePage)
			return;

		if (this.page.hasPopup && !marker.showsContentOnlyInTooltip)
		{
			// Determine whether the marker's popup should be shown or hidden.

			// First detect the case where the user just touched a marker whose popup was pinned in reponse
			// to the previous touch. A second touch acts as a toggle to deselect the marker and close the
			// popup. This is different behavior than when clicking a pinned marker with the mouse.
			// Clicking a pinned marker with the mouse unpins its popup, but does not deselect the marker
			// or close the popup. That's because the mouse remains over the marker and so if the popup
			// got closed, it would open again immediately when the mouse moved.
			let userTouchedSelectedMarker = pointerType === 'touch' && this.markerUnderPointerViewId === this.selectedMarkerViewId;
			if (userTouchedSelectedMarker && this.popup.isPinned)
			{
				this.deselectMarkerAndClosePopup();
				return;
			}

			// When a marker is touched, pin its popup whether or not the Pin When Marker Clicked option is set.
			let pinPopup = this.popup.pinOnClick || pointerType === 'touch';

			// Determine if a new popup needs to be shown or if the exsting popup needs to be pinned or unpinned.
			if (pinPopup || marker.showContentEvent === this.SHOW_CONTENT_ON_CLICK)
			{
				let pin;
				if (viewId === this.selectedMarkerViewId)
				{
					// The user tapped the selected marker. If it's pinned, unpin it, otherwise pin it if pinnable.
					pin = this.popup.isPinned ? false : pinPopup;
				}
				else
				{
					// The user tapped a marker that is not currently selected. Pin it if pinnable.
					this.deselectMarker();
					this.selectMarker(marker.viewId, pointer);
					pin = pinPopup
				}

				this.popup.showPopup(pin);
			}
			else if (this.page.popup.popupState === this.page.popup.STATE_PINNED)
			{
				this.deselectMarker();
				this.selectMarkerAndShowPopup({ viewId: viewId, pointer: pointer });
			}
		}
		else
		{
			this.selectMarker(marker.viewId, pointer);
		}

		// Execute the marker's click action that does not go to another page.
		// This code is at the end here so that the clicked marker is selected and its hotspot Id
		// can be retrieved by the click action's JavaScript using the API's currentHotspot property.
		let isTouch = this.page.layout.usingMobileLayout || pointerType === "touch";
		if (isTouch && !marker.touchPerformsClickAction)
			this.executeMouseOverAction(marker);
		else
			this.executeClickAction(marker);
	}

	hideTooltip()
	{
		//console.log(`Map::hideTooltip`);

		if (!this.showingTooltip)
			return;

		this.tooltipElement.classList.remove('maWaiting')
		this.tooltipElement.style.visibility = "hidden";
		this.showingTooltip = false;
	}

	ignoreMarker(pctX, pctY, flags)
	{
		// Ignore markers that are off the map except for route markers or when in the Map Editor.
		// This is an optimzation for tours so that processing is not spent creating and searching
		// marker instances for markers that are not on the map. All markers are needed in the Map
		// Editor so that the user can place off-map markers onto the map. Route markers are special
		// in that they represent a route through waypoint markers, but don't appear on the map

		if (pctX >= 0 && pctY >= 0)
			return false;

		if (this.tour.editMode)
			return false;

		const isRoute = (flags & 0x00000008) !== 0;
		if (isRoute)
			return false;

		return true;
	}

	ignorePanEvent(event)
	{
		// Detect and handle what appears to be a bug in hammer.js where following a 'pinchend' event
		// hammer fires a 'pan' event and then a 'panend' event without having first fired 'panstart'.
		// The problem only happens on touch devices because those are the only ones where you can pinch.
		// The underlying raw event that seems to trigger the problem is 'pointerup' which is the event
		// that causes hammer to fire a final 'pan' and 'panend' when a pan gesture finishes. Since we
		// don't know if there are other similar instances of this problem, this method verifies that the
		// current event is acceptable based on the previous event.
		// The only valid event sequence is: '' > 'panstart' > 'pan'+ > 'panend'
		let last = this.lastPanEventType;
		if (event.type === 'panstart' && last !== '' ||
			event.type === 'pan' && !(last === 'pan' || last === 'panstart') ||
			event.type === 'panend' && last !== 'pan')
		{
			console.log(`Map::ignorePanEvent '${event.type}' cannot follow '${last}'`);
			this.stopPanning();
			return true;
		}

		// Ignore a pan event when there is more than one finger on the screen. This deals with the case
		// where the user starts panning with one finger, then puts down a second finger and then either
		// lifts the second finger or lifts the first finger. In both cases, the pointer location associated
		// with the pan event changes from one finger to the other which causes the pan to jump by the
		// distance between the two fingers. In this scenario, cancel panning so that subsequent pan event
		// will get ignored by the code above which ignores a 'pan' without a 'panstart'.
		let numberOfTouchPoints = event.pointers.length;
		if (numberOfTouchPoints !== 1)
		{
			console.log(`Map::ignorePanEvent ${event.type} ${numberOfTouchPoints} touch points`);
			this.stopPanning();
			return true;
		}

		// Ignore a pan event and instead close the mobile content panel if it's showing. If the map had been
		// over-panned in order to make a marker visible that would otherwise have been covered by the panel,
		// the call to hideContentPanel will cause the map to get repositioned to a valid panning position.
		if (this.page.layout.usingMobileLayout && !this.page.layout.contentPanelIsHidden)
		{
			this.page.layout.hideContentPanel();
			return true;
		}

		// Rememember the current event.
		this.lastPanEventType = event.type;

		return false;
	}

	ignoreTapEvent(event)
	{
		if (event.type === 'press')
		{
			if (this.tour.editMode)
			{
				// Ignore a 'press' event when in the Map Editor unless the ctrl key is being pressed.
				// Normally, press signals the start of a drag, but if the ctrl key is down, treat it
				// as a tap to let the user ctrl-click markers to add or remove them from the set of
				// selected markers. Without this special handling, the initial press works to add a
				// marker to the selection, but the subsequent 'pressup' tap deslects it.
				if (event.srcEvent.ctrlKey)
					return true;
			}
			else
			{
				// Ignore a 'press' event when not in the Map Editor. When the 'pressup' fires, accept
				// the event. This way, a press/pressup sequence gets treated the same as a single tap.
				// If we don't ignore the press, it gets treated as a tap and then the pressup gets
				// treated as a second tap. Two consecutive taps on a marker using a pinnable popup
				// causes it the popup to get pinned on the press and then unpinned on the pressup.
				return true;
			}
		}

		return false;
	}

	initializeDrawState()
	{
		this.drawState = null;
		this.clearDrawMapDelayTimer();
	}

	initializeMapScale()
	{
		if (this.tour.editMode)
		{
			// Restore the map's zoom percent and pan position to what they were when last viewed in the Map Editor.
			this.currentMapScale = Runtime__$$.roundFloat(this.mapEditorPercent / 100);
			this.updatePanXandPanY(this.mapEditorPanX, this.mapEditorPanY)
		}
		else if (!this.mapZoomEnabled)
		{
			this.currentMapScale = 0;
			this.mapFocusPercent = 0;
			this.updatePanXandPanY(0, 0);
		}
		else
		{
			// Set the map scale, but defer setting the pan distances until the canvas dimensions are known. That
			// happens when Layout::setTourSize is called which in turns calls Map::positionAndZoomMapToFocusedState.

			// See if the map is focused zoomed out (mapFocusPercent is zero or -1). Otherwise, use the focused percentage.
			if (this.mapFocusPercent <= 0)
				this.currentMapScale = 0;
			else
				this.currentMapScale = Runtime__$$.roundFloat(this.mapFocusPercent / 100);
		}
	}

	initializeMapState()
	{
		//console.log(`Map::initializeMapState ${this.page.pageName}`);

		this.selectedMarker = null;
		this.markerUnderPointer = null;
		this.pointerState = this.POINTER_STATE_START;

		this.showingTooltip = false;
		this.movingPopupWithMouse = false;
		this.movingPopupMouse = null;

		if (this.page.isGallery)
		{
			this.mapZoomEnabled = false;
			this.mapInsetLocation = 0;
		}

		if (this.tour.editMode)
			this.mapZoomEnabled = true;

		this.popupDelayTimerId = 0;
		this.slideShowTimerId = 0;
		this.insetDragX_screen = 0;
		this.insetDragY_screen = 0;
		this.minHitTargetSize = 44;
		this.blinkCycle = 0;
		this.visitedMarkerAlpha = this.visitedMarkerAlpha / 100;
		this.slideShowRunning = false;
		this.nextMarkerIndex = 0;
		this.mapZoomDelta = 0;
		this.zoomedOutMapScale = 0;
		this.mapW_actual = this.mapWidth;
		this.mapH_actual = this.mapHeight;
		this.mapSize_actual = Runtime__$$.createSizeObject(this.mapW_actual, this.mapH_actual);

		this.mapInsetScale = this.calculateMapInsetScale();

		// Set the canvas dimensions only if using the Map Editor, otherwise they are set when the page layout is sized.
		if (this.tour.editMode)
		{
			this.setCanvasSize({ w: this.mapAreaW, h: this.mapAreaH });
			this.calculateZoomedOutMapScale();
		}

		this.initializeMapScale();

		if (this.page.isDataSheet)
			return;

		// Use separate Id's for local storage when in Tour Preview or not so that
		// a published tour does not use the same state as a one in Tour Preview.
		this.localStorageId = "tour" + (this.tour.preview ? "_." : ".");

		// Initialize flags that can be set by the map editor.
		this.showMarkerAppearanceToggled = false;
		this.showMarkerTransparencyToggled = false;

		// Debug options.
		this.selectOnClick = false;
		this.mapIsSvg = false;

		// Create the Map Editor.
		if (this.tour.editMode)
		{
			try
			{
				this.editor = new MapEditor(this);
			}
			catch (error)
			{
				// This error occurs with desktop Safari 12.1.2, but works with 14.0.3.
				// Rather than attempt to determine which versions of which browsers cannot
				// instantiate the Map Editor, just catch the exception and report the error.
				console.trace(`MapEditor::initializeMapState ${error.message}`);
				debugger;
				alert('This brower does not support the MapsAlive map editor.\n\nPlease use the latest version of Chrome, Microsoft Edge, Firefox, or Safari.');
				window.location = 'TourManager.aspx';
			}
		}
	}

	get inMapEditorWithNonZoomableV3Map()
	{
		// In V3 a zoom level of zero means not zoomable.
		return this.tour.enableV3Compatibility && this.tour.editMode && this.mapZoomLevelV3 === 0;
	}

	listenForEvents()
	{
		//console.log(`Map::listenForEvents ${this.page.pageName}`)

		this.listenForEventsOnMap();
		this.listenForEventsOnMapInset();
		this.listenForEventsOnZoomInOutButtons();

		// Listen for touch events on the nav button when its on the map.
		if (this.page.layout.navButtonIsOnMap)
			this.page.layout.navButton.addEventListener('touchstart', this.page.layout.onClickNavButton, { passive: true });

		// Detect if the user has released the mouse button anywhere, including outside the
		// brower. See the comments for onTourBuilderMouseUp for more information.
		window.addEventListener("mouseup", this.onTourBuilderMouseUp, false);

		// Listen for special keys like the arrow keys.
		document.addEventListener("keydown", this.onKeyDown);

		// Listen for events that affect the position of the map within the viewport.
		window.addEventListener("resize", this.onResize);
		document.addEventListener('scroll', this.onScroll);
	}

	listenForEventsOnMap()
	{
		//console.log(`Map::listenForEventsOnMap`)

		this.markerLayerEventManager = new Hammer(this.markerLayer);

		// Listen for tap and press (tap and hold) events. Set a short time period to detect
		// press so that touching a marker for a moment has the same effect has tapping, but
		// without lifting your finger. Note that setting the time too short (e.g. 10) would
		// make press fire before tap when tap is what you want.
		this.markerLayerEventManager.get('press').set({ time: 100 });
		this.markerLayerEventManager.on("tap press pressup", this.onTapMap);

		if (this.mapZoomEnabled)
		{
			// Listen for pan events which are also used for dragging in the Map Editor. Set the
			// pan threshold to 1 so that pan is detected immediately. With the default value of
			// 10, it wasn't possible to pan the crosshairs because the mouse was off of them
			// before the pan event fired.
			this.markerLayerEventManager.get('pan').set({ direction: Hammer.DIRECTION_ALL, threshold: 1 });
			this.markerLayerEventManager.on('pan panstart panend', this.onPanMap);

			// Listen for pinch events.
			this.markerLayerEventManager.get('pinch').set({ enable: true });
			this.markerLayerEventManager.on('pinchstart pinch pinchend', this.onPinchMap);

			if (this.tour.isMouseDevice && !this.inMapEditorWithNonZoomableV3Map && !this.page.isGallery)
			{
				//console.log(`Map::listenForEventsOnMap wheel`);
				this.markerLayer.addEventListener('wheel', this.onMouseWheel, { passive: false });
			}
		}

		// Show raw events in the console only for debugging purposes.
		if (this.tour.flagShowDeviceEvents)
			this.markerLayerEventManager.on("hammer.input", this.onRawInputMap);

		// Listen for mouse events on any device that has a mouse.
		if (this.tour.isMouseDevice)
		{
			this.markerLayer.addEventListener('mousemove', this.onMapMouseMove, false);
			this.markerLayer.addEventListener('mouseout', this.onMapMouseOut, false);
			this.markerLayer.addEventListener('click', this.onMapClick, false);
			this.markerLayer.addEventListener('dblclick', this.onMapDoubleClick, false);
		}

		// Prevent the context menu from comming up following a pressup event.
		this.markerLayer.addEventListener('contextmenu', e => { e.preventDefault(); });
	}

	listenForEventsOnMapInset()
	{
		if (this.insetLayer === null)
			return;

		this.insetLayerEventManager = new Hammer(this.insetLayer);

		// Listen for pan events. Set the pan threshold very low so that pan is detected immediately.
		this.insetLayerEventManager.get('pan').set({ direction: Hammer.DIRECTION_ALL, threshold: 1 });
		this.insetLayerEventManager.on('panstart pan panend', this.onPanMap);

		if (!this.tour.isTouchDevice)
		{
			// Listen for mouse move events.
			this.insetLayer.addEventListener('mousemove', this.onMapInsetMouseMove, false);
		}
	}

	listenForEventsOnZoomInOutButtons()
	{
		if (!this.mapZoomEnabled)
			return;

		//console.log(`Map::listenForEventsOnZoomInOutButtons`);

		this.zoomInControl.addEventListener('mousedown', this.onTapZoomInOutButton, false);
		this.zoomInControl.addEventListener('mouseup', this.onTapZoomInOutButton, false);
		this.zoomInControl.addEventListener('touchstart', this.onTapZoomInOutButton, { passive: true });
		this.zoomInControl.addEventListener('touchend', this.onTapZoomInOutButton, false);

		this.zoomOutControl.addEventListener('mousedown', this.onTapZoomInOutButton, false);
		this.zoomOutControl.addEventListener('mouseup', this.onTapZoomInOutButton, false);
		this.zoomOutControl.addEventListener('touchstart', this.onTapZoomInOutButton, { passive: true });
		this.zoomOutControl.addEventListener('touchend', this.onTapZoomInOutButton, false);

		// Listen for mouseover and mouseout events when in the Map Editor.
		if (this.tour.editMode && !this.tour.isTouchDeviceWithoutMouse)
		{
			this.zoomInControl.addEventListener('mouseover', this.onZoomInOutButtonMouseOver, false);
			this.zoomInControl.addEventListener('mouseout', this.onZoomInOutButtonMouseOver, false);
			this.zoomOutControl.addEventListener('mouseover', this.onZoomInOutButtonMouseOver, false);
			this.zoomOutControl.addEventListener('mouseout', this.onZoomInOutButtonMouseOver, false);
		}

		// Prevent the context menu from comming up following a pressup event.
		this.zoomInControl.addEventListener('contextmenu', e => { e.preventDefault(); });
		this.zoomOutControl.addEventListener('contextmenu', e => { e.preventDefault(); });

	}

	loadMapImage(resolution)
	{
		let image = new Image();

		// Allow cross-origin downloading to prevent the error "canvas has been tainted by cross-origin data"
		// from occurring when getImageData() gets called to resample or sharpen the image while in Tour Preview.
		// The problem occurs because Tour Preview runs on www.mapsalive.com whereas the original image comes
		// from tour.mapsalive.com. The error does not occur when running the actual tour from tour.mapsalive.com.
		image.crossOrigin = "Anonymous";

		// Initiate asynchronous loading of the file
		let fileName = this.page.isGallery ? this.mapFileName : this.mapFileName.replace('.jpg', `_${resolution}.jpg`);
		image.src = this.mapImagePath + fileName + '?v=' + this.tour.buildId;

		return image;
	}

	loadMapImages()
	{
		this.useFullSizeImage = this.page.isGallery || this.inMapEditorWithNonZoomableV3Map;

		// Load the 25% and 50% resolution map images.
		if (!this.useFullSizeImage)
		{
			this.mapImage25 = this.loadMapImage(25);
			this.mapImage50 = this.loadMapImage(50);
		}

		if (this.useFullSizeImage)
		{
			// Ideally the full size image would also be loaded when the map will need that image when it first displays.
			// However, the logic to calculate the device, container, and map sizes, and which resolution image will be
			// needed the first time the map is drawn, does not occur until later. The Tour Builder cannot figure it out
			// ahead of time because the needed resolution is dependent on the device size. As such there will be some
			// situations where the full resolution map will be needed and could have been loaded ahead of time, but
			// instead will get loaded on demand. One solution would be to provide a user option to preload the full
			// size image, but it's not clear whether that is really needed.
			this.mapImage100 = this.loadMapImage(100);
		}
		else
		{
			// Defer loading of the 100% resolution image until it's actually needed.
			this.mapImage100 = null;
			this.mapImage100Loaded = false;
		}

		this.loadMapInsetImage();
	}

	loadMapInsetImage()
	{
		if (!this.mapInsetEnabled)
			return;

        this.mapInsetImage = new Image();
		this.mapInsetImage.src = this.mapImagePath + '_' + this.mapFileName + '?v=' + this.tour.buildId;
	}

	mapCanBeOverZoomed(event)
	{
		return this.zoomedAllTheWayIn && this.tour.editMode && event.shiftKey;
	}

	get mapImagePath()
	{
		return this.tour.editMode ? this.tour.tourFolderUrl + this.tour.tourId + '_/' : this.tour.path;
	}

	mapImagesHaveLoaded()
	{
		if (this.useFullSizeImage)
		{
			if (this.mapImage100 === null)
				return false;

			if (!(this.mapImage100.complete))
				return false;
		}
		else
		{
			if (this.mapImage25 === null || this.mapImage50 === null)
				return false;

			if (!(this.mapImage25.complete && this.mapImage50.complete))
				return false;
		}

		//console.log(`Map::mapImagesHaveLoaded`);

		return true;
	}

	get mapInsetEnabled()
	{
		return this.mapInsetLocation !== 0 && this.mapZoomEnabled && !this.page.isGallery && !this.inMapEditorWithNonZoomableV3Map;
	}

	mapZoomPanStateChanged()
	{
		//console.log(`Map::mapZoomPanStateChanged`);

		// Update the bounding area for each marker.
		for (let marker of this.markers)
			marker.updateBounds();

		if (this.tour.editMode)
			this.editor.mapZoomStateChanged();
	}

	get markerDrawingIsDisabled()
	{
		return this._disableMarkerDrawing;
	}

	get markerIsSelected()
	{
		return this.selectedMarker !== null;
	}

	get markerLayerCacheEnabled()
	{
		let disableCaching = this.tour.getTourSettingBool("disable-caching") === true;
		return !disableCaching;
	}

	get markerLayerCacheIsStale()
	{
		if (this.markerLayerCache === null)
			return true;

		if (this.markerLayerCache.width !== this.markerLayer.width || this.markerLayerCache.height !== this.markerLayer.height)
		{
			this.flushMarkerLayerCache();
			return true;
		}

		//console.log(`Map::markerLayerCacheIsStale NOT STALE`);
		return false;
	}

	get markerUnderPointerViewId()
	{
		return this.markerUnderPointer === null ? 0 : this.markerUnderPointer.viewId;
	}

	mergeMarkerStyleProperties(markerStyle, selected, lineColorNumber, fillColorNumber, lineColorOpacity, fillColorOpacity, effectsDefinition)
	{
		// This method creates a marker style properties object by merging the passed-in values
		// with the existing marker style properties. A null parameter value means don't change
		// that property. A non-null value means changes the existing property.

		let properties = markerStyle.getProperties(selected);

		// Replace any null values with the marker style's values.
		let lineColor = lineColorNumber === null ? properties.lineColor : this.graphics.convertIntegerColorToCss(lineColorNumber);
		let fillColor = fillColorNumber === null ? properties.fillColor : this.graphics.convertIntegerColorToCss(fillColorNumber);

		if (lineColorOpacity === null)
			lineColorOpacity = properties.lineColorOpacity;

		if (fillColorOpacity === null)
			fillColorOpacity = properties.fillColorOpacity;

		if (effectsDefinition === null)
			effectsDefinition = properties.effects.definition;

		// Create the changed properties.
		let changedProperties = new MarkerStyleProperties__$$(this, fillColor, fillColorOpacity, lineColor, lineColorOpacity, effectsDefinition);

		return changedProperties;
	}

	mouseIsOverInsetZoomRegion(mouseX, mouseY)
	{
		// Determine if the mouse is over the map zoom inset. The mouseX and mouseY values must come from
		// event.pageX and event.pageY in order to compare with the rect returned from elementlocation.
		let insetLayerRect = Runtime__$$.elementLocation(this.insetLayer);
		let x = mouseX - insetLayerRect.x;
		let y = mouseY - insetLayerRect.y;
		let rect = this.mapInsetZoomRect;
		return x >= rect.x && x < rect.x + rect.w && y >= rect.y && y < rect.y + rect.h;
	}

	movePopupWithMouse()
	{
		// Make sure the popup is still showing and wasn't closed since the last animation frame.
		if (this.popup.isShowing)
			this.page.popup.movePopupToLocation(this.movingPopupMouse);

		this.movingPopupWithMouse = false;
		this.movingPopupMouse = null;
	}

	movePopupWithMouseOnNextAnimationFrame(mouse)
	{
		// This method makes a request to asynchronously update the popup position during the next
		// animation frame. This method returns immediately if the popup is currently being moved
		// so as to not burden the JavaScript runtime with unnessary drawing requests in between
		// screen repaints. This is okay since this method will get called again to handle the next
		// popup move at the current mouse location.

		if (this.movingPopupWithMouse)
			return;

		//console.log(`Map::movePopupWithMouseOnNextAnimationFrame ${this.popup.popupState}`);

		this.movingPopupWithMouse = true;
		this.movingPopupMouse = mouse;

		requestAnimationFrame(this.movePopupWithMouse);
	}

	moveTooltipToCoordinates(pointer)
	{
		//console.log(`Map::moveTooltipToCoordinates ${pointer.x},${pointer.y}`);

		// Verify that the mapElement still exists. This is necessesary to handle the case where someone click/drags
		// over a marker that both shows a tooltip and has a click handler to go to another page in the tour e.g.
		// a "go upstairs" marker in a house tour. While mouse-move events are rapidly firing as the page is changing,
		// the current mapElement is set to null before being set to the map for the new page. During that brief period
		// this method can be called in which case it simply returns.
		if (this.page.layout.mapElement === null)
			return;

		// Get the offset of the preview panel centered within the browser and use it to adjust the tooltip location.
		let offsetX = 0;
		if (this.tour.preview)
			offsetX = Runtime__$$.elementLocation(document.getElementById('PreviewPanel')).x;

		// Get the offset of the map within the browser.
		let rect = this.page.layout.mapElement.getBoundingClientRect();

		// Move the tooltip to the adjusted location relative to the pointer position.
		let s = this.tooltipElement.style;

		// Set the tooltip's offset from the cursor such that on a desktop browser the cursor
		// can't get onto the tooltip if the user mouses very quickly. This is to avoid having
		// the marker flicker from being deselected/selected as the mouse goes over/off the tooltip.
		let yOffset = this.page.layout.usingMobileLayout ? -12 : 12;

		// Prevent the tooltip from becoming too narrow if very close to the right edge of the tour.
		const minTooltipW = 160;
		if (pointer.x + minTooltipW > this.tour.deviceSize.w)
			pointer.x = this.tour.deviceSize.w - minTooltipW;

		s.left = (pointer.x + rect.left + window.scrollX + 16 - offsetX) + 'px';
		s.top = (pointer.y + rect.top + window.scrollY + yOffset) + 'px';
	}

	moveTooltipToMouseLocation(mouse)
	{
		//console.log(`Map::moveTooltipToMouseLocation ${mouse.x},${mouse.y}`);

		if (this.slideShowRunning)
			return;

		this.moveTooltipToCoordinates(mouse);
	}

	onKeyDown(event)
	{
		if (this.tour.editMode)
		{
			this.editor.handleEventKeyDown(event)
		}
		else
		{
			if (event.key === 'Escape')
				this.page.closePopup();
		}
	}

	onMapClick(event)
	{
		if (this.tour.editMode)
			this.editor.handleEventClick(event)
	}

	onMapDoubleClick(event)
	{
		if (this.tour.editMode)
			this.editor.handleEventDoubleClick(event)
	}

	onMapInsetMouseMove(event)
	{
		//console.log(`Map::onInsetMouseMove`);

		// Ignore mouse moves over the inset when it's not showing.
		if (this.zoomingDisabled)
			return;

		// Ignore mouse moves over the inset while panning the inset.
		if (this.panningMap)
			return;

		let mouseIsOverInsetZoomRegion = this.mouseIsOverInsetZoomRegion(event.pageX, event.pageY);
		this.insetLayer.style.cursor = mouseIsOverInsetZoomRegion ? 'pointer' : 'auto';

		if (this.tour.editMode)
			this.editor.showZoomPanInformation();
	}

	onMapMouseMove(event)
	{
		// The mouse is over the map. Don't let this event also be handled by the tour's mouse handler.
		event.stopPropagation();

		// Protect against a move-move event that occurs while changing from one page to another.
		if (this.page.layout.mapElement === null)
			return;

		// Ignore mouse moves while panning.
		if (this.panningMap)
			return;

		//console.log(`Map::onMapMouseMove`);

		// Record the most recent mouse location. Normally the mouse location is passed along from
		// here, or is obtained again on other events like pan and tap, but this saved location is
		// used when showing a popup after a delay. In that case, the delay timer fires, but since
		// that's not a mouse event, this save position is used to position the popup at the mouse.
		this.mouse = this.getPointerLocationOnMap(event.clientX, event.clientY);

		this.handlePointerStateChange(this.mouse);

		// For mobile layouts there's nothing else to do after the state change.
		if (this.page.layout.usingMobileLayout)
			return;

		if (this.tour.editMode)
			return;

		// Move the tooltip.
		if (this.showingTooltip)
			this.moveTooltipToMouseLocation(this.mouse);

		// Move the popup to the current mouse location.
		if (this.page.hasPopup && this.popup.isShowing && this.popup.followsMouse)
			this.movePopupWithMouseOnNextAnimationFrame(this.mouse);
	}

	onMapMouseOut(event)
	{
		if (this.page.layout.usingMobileLayout)
			return;

		if (this.page.hasPopup)
		{
			// Ignore a mouse move from the map onto a popup. The popup's mouseover handler will take care of it.
			if (event.relatedTarget !== null && this.popup.isPopupElement(event.relatedTarget))
			{
				//console.log(`Map::onMapMouseOut relatedTarget:${event.relatedTarget.id}`);
				return;
			}

			// Ignore the pointer when showing a pinned popup. Revert to the Start state
			// since the current state may no longer be accurate after the popup gets closed.
			if (this.popup.isPinned)
			{
				this.resetMapPointerState();
				return;
			}
		}

		// Handle the case where the mouse moves off the map while over a marker that is at the edge
		// of the map, or at the edge of the viewport. In these cases, the pointerMovedFromMarkerToMap
		// method won't not get called because they are triggered by a mousemove, so call it here, but
		// first hide the tooltip if one is displaying since the mouse is off the map.
		this.hideTooltip();
		if (this.pointerIsOverMarker)
		{
			//console.log(`Map::onMapMouseOut pointer moved off the map from a marker`);
			this.pointerMovedFromMarkerToMap(this.markerUnderPointer);
		}
	}

	onMouseWheel(event)
	{
		if (this.zoomingDisabled)
			return;

		this.stopCurrentOperation();

		// Prevent the mouse wheel from scrolling the page when the cursor is over the map.
		event.preventDefault();

		// When the mouse wheel moves while waiting for the previous wheel movement to time out,
		// cancel the timer and start zooming again. This is simpler than extending the timer, but
		// having to check to see if the wheel direction changed from zoom in to out or vice versa.
		if (this.zoomMouseWheelDelayTimerId !== 0)
		{
			clearTimeout(this.zoomMouseWheelDelayTimerId);
			this.onMouseWheelTimeout(event)
		}

		// Get the zoom direction. 
		let delta = event.deltaY;

		if (this.mapCanBeOverZoomed(event))
			this.editor.actionAllowOverZoom();

		// Check if the map is zoomed all the way in the mouse wheel direction.
		if (delta >= 0 && this.zoomedAllTheWayOut || delta < 0 && this.zoomedAllTheWayIn)
			return;

		// Start zooming on each animation frame, but stop if the user stops moving the wheel.
		let zoomIn = delta < 0;
		this.zoomPercent = zoomIn ? 1 : -1;
		this.startZooming(this.mousePointer);
		this.allowSlowGraphics(false);
		this.zoomMapOnEachAnimationFrame();
		this.zoomMouseWheelDelayTimerId = setTimeout(this.onMouseWheelTimeout, 100);

		//console.log(`Map::onMouseWheel ${delta} In:${this.zoomedAllTheWayIn} Out:${this.zoomedAllTheWayOut}`);
	}

	onMouseWheelTimeout(event)
	{
		//console.log(`Map::onMouseWheelTimeout`);
		this.zoomMouseWheelDelayTimerId = 0;
		this.stopZooming();
	}

	onPanMap(event)
	{
		this.stopCurrentOperation();

		// Ignore panning on a map that is zoomed all the way out because the map cannot be panned.
		if (this.zoomedAllTheWayOut && !this.tour.editMode)
		{
			// Determine if there is a marker under the pointer and if so, treat this pan as a tap.
			// This is necessary for the Apple Pencil which tends to generate many pointermove events
			// even when just quickly tapping a marker.
			this.onTapMap(event);
			return;
		}

		if (this.ignorePanEvent(event))
			return;

		let pointer = this.getPointerLocationOnMap(event.center.x, event.center.y)
		//console.log(`Map::onPanMap ${event.type} ${pointer.x},${pointer.y} ${event.pointers.length}`);

		// Determine whether the event is for the map or for the map inset.
		let eventTargetIsMap = event.target.id === this.tour.uniqueId('MarkerLayer');

		// Give the Map Editor a chance to handle the event, but only if not already panning the map.
		if (!this.panningMap && this.tour.editMode && eventTargetIsMap)
		{
			// Simulate a mousemove event so that the Map Editor will detect if the point
			// of touch at the start of the pan is over a marker or over crosshairs.
			if (this.tour.isTouchDevice)
				this.handlePointerStateChange(pointer);

			// See if this pan request is to drag a marker or crosshairs. If the editor handles
			// the request, we're done, otherwise fall through and start panning the map.
			if (this.editor.handleEventMapEditorPan(event.type, pointer))
				return;
		}

		let style = eventTargetIsMap ? this.markerLayer.style : this.insetLayer.style;

		switch (event.type)
		{
			case 'panstart':
				// Record the pointer location and pan position so we'll know how much to pan on subsequent 'pan' events.
				this.panPointerStartX = pointer.x;
				this.panPointerStartY = pointer.y;
				this.panMapStartX = this.panX_map;
				this.panMapStartY = this.panY_map;

				// Initialy set the new pan position to the current position.
				this.panNewX_map = this.panMapStartX;
				this.panNewY_map = this.panMapStartY;

				// Initiate panning.
				this.panningMap = true;
				this.allowSlowGraphics(false);
				this.panMapOnEachAnimationFrame();
				break;

			case 'pan':
				// Show the double-arrow cross cursor while panning.
				style.cursor = 'move';

				// Determine how far the pointer has moved since the pan started.
				let pointerDeltaX = pointer.x - this.panPointerStartX;
				let pointerDeltaY = pointer.y - this.panPointerStartY;

				// Convert the pointer movement distance to map pixels.
				let mapDeltaX;
				let mapDeltaY;
				if (eventTargetIsMap)
				{
					mapDeltaX = this.convertScreenToMap(pointerDeltaX);
					mapDeltaY = this.convertScreenToMap(pointerDeltaY);
				}
				else
				{
					// Scale-up pixels on the inset to what they represent on the map.
					mapDeltaX = pointerDeltaX / this.mapInsetScale * -1;
					mapDeltaY = pointerDeltaY / this.mapInsetScale * -1;
				}

				// Calculate the new pan position based on the original position plus the distance panned away since
				// panning started. The panMapOnEachAnimationFrame method will do the actual pan to this new position.
				this.panNewX_map = Math.round(this.panMapStartX + mapDeltaX);
				this.panNewY_map = Math.round(this.panMapStartY + mapDeltaY);
				break;

			case 'panend':
				style.cursor = this.pointerIsOverMarker ? "pointer" : "auto";
				this.stopPanning();

				// See comment at the top regarding the Apple Pencil.
				this.onTapMap(event);
				break;

			default:
				console.log(`Map::onPanMap UNEXPECTED PAN TYPE ${event.type}`);
				this.stopPanning();
		}
	}

	onPinchMap(event)
	{
		function distanceBetweenTouchPoints(_this)
		{
			// Calculate the distance between the two touch points by getting
			// the hypotenuse of the right triangle that is formed between them.
			let numberOfTouchPoints = event.pointers.length;
			if (numberOfTouchPoints !== 2)
			{
				console.log(`Map::onPinchMap ${event.type} Expected 2 touch points, but found ${numberOfTouchPoints}`);
				return 0;
			}
			let x1 = event.pointers[0].pageX;
			let y1 = event.pointers[0].pageY;
			let x2 = event.pointers[1].pageX;
			let y2 = event.pointers[1].pageY;
			let a = Math.abs(y2 - y1);
			let b = Math.abs(x2 - x1);

			return Math.hypot(a, b);
		}

		function pointBetweenTouchPoints()
		{
			// Get the point that is in the middle of the two touch points.
			let pointer1 = { x: Math.round(event.pointers[0].pageX), y: Math.round(event.pointers[0].pageY) };
			let pointer2 = { x: Math.round(event.pointers[1].pageX), y: Math.round(event.pointers[1].pageY) };
			return { x: Math.round((pointer1.x + pointer2.x) / 2), y: Math.round((pointer1.y + pointer2.y) / 2) };
		}

		if (this.page.isGallery)
			return;

		this.stopCurrentOperation();

		console.log(`Map::onPinchMap ${event.type}`);

		switch (event.type)
		{
			case 'pinchstart':
				this.startZooming(pointBetweenTouchPoints());
				this.pinchingMap = true;
				this.zoomPercent = 0;
				this.lastPinchDistance = distanceBetweenTouchPoints(this);
				this.currentPinchDistance = this.lastPinchDistance;

				// Initiate pinching.
				this.allowSlowGraphics(false);
				this.zoomMapOnEachAnimationFrame();
				break;

			case 'pinch':
				// Get the delta between the current distance and the distance when this method was last
				// called and add it to the current distance. It will be used by zoomMapOnEachAnimationFrame
				// to dermine how much to zoom the map in or out. If the user is pinching quickly and this
				// method gets called in between animation frames, intermediate distances will be ignored
				// and the accumlated distance will be used in the next frame. After the zoom is performed,
				// the current distance gets reset to zero
				let delta = distanceBetweenTouchPoints(this) - this.currentPinchDistance;
				this.currentPinchDistance += delta;
				break;

			case 'pinchend':
				this.stopZooming();
				break;

			default:
				console.log(`Map::onPinchMap UNEXPECTED EVENT ${event.type}`);
		}
	}

	onRawInputMap(event)
	{
		// This method is enabled/disabled by flagShowDeviceEvents, so don't comment out console.log.
		let pointer = event.pointers[0];
		/**/console.log(`Map::onRawInputMap ${pointer.pointerType} ${pointer.type}`);
	}

	onRawInputZoomInOutButton(event)
	{
		let eventType = event.pointers[0].type;
		//console.log(`Map::onRawInputZoomInOutButton ${eventType}`);

		// Handle the case where the user is pressing one of the zoom in/out buttons and then
		// slides their finger off the button, or moves the mouse off the button. In this case
		// there will be no pressup event, so on a pointermove event, stop zooming.
		if (this.s !== 0 && eventType === 'pointermove')
			this.stopZooming();
	}

	onResize(event)
	{
		//console.log(`Map::onResize`);
		this.mapElementBoundingClientRect = null;
	}

	onScroll(event)
	{
		this.mapElementBoundingClientRect = null;

		if (this.page.layout.usingMobileLayout)
			return;

		// Close the popup for a non-mobile layout because there is no logic to reposition a popup
		// when the page scrolls. For mobile layouts, ignore the scoll event because there are instances
		// where the event fires for some reason during the process of displaying the content panel,
		// in which case it must not be closed while it is opening. This happens on the iPhone on a
		// classic tour if you push up on the title bar. which scrolls the page, and then you touch
		// a marker which displays the content panel. Why the scroll event fires while the content panel
		// is displaying is a mystery, but be sure to test this issue if this code ever needs to be modifed.
		this.page.closePopup();
	}

	onSymbolImageDataLoaded()
	{
		console.log("onSymbolImageDataLoaded");
		this.symbolImageDataLoaded = true;
	}

	onTapMap(event)
	{
		//console.log(`Map::onMapTap ${event.type} ${event.pointerType}`);

		this.stopCurrentOperation();

		if (this.ignoreTapEvent(event))
			return;

		// Determine if a marker was tapped, or just the map.
		let pointer = this.getPointerLocationOnMap(event.center.x, event.center.y);
		this.markerUnderPointer = this.getMarkerUnderPointer(pointer);

		// Let the Map Editor handles the tap.
		if (this.tour.editMode)
		{
			this.editor.handleEventMapEditorTap(this.markerUnderPointerViewId, pointer);
			return;
		}

		// The user clicked or touched a marker.
		this.handleTapMarker(this.markerUnderPointerViewId, pointer, event.pointerType);
	}

	onTapZoomInOutButton(event)
	{
		//console.log(`Map::onTapZoomInOutButton ${event.type}`);

		this.stopCurrentOperation();

		let zoomIn = event.currentTarget.id === this.tour.uniqueId('ZoomInControl');

		if (this.mapCanBeOverZoomed(event))
			this.editor.actionAllowOverZoom();

		if (zoomIn && this.zoomedAllTheWayIn || !zoomIn && this.zoomedAllTheWayOut)
		{
			this.stopZooming();
			return;
		}

		this.zoomPercent = zoomIn ? 1 : -1;

		switch (event.type)
		{
			case 'mousedown':
			case 'touchstart':
				if (!this.zoomingMap)
				{
					// Initiate continuous zooming which continues until a pressup event. Because the user is
					// pressing the zoom controls, their mouse or finger is not over a point on the map, use
					// the center of the canvas as the point to zoom from.
					let canvasCenterPoint = { x: Math.round(this.canvasW / 2), y: Math.round(this.canvasH / 2) };
					this.startZooming(canvasCenterPoint);
					this.allowSlowGraphics(false);
					this.zoomMapOnEachAnimationFrame();

					// Only zoom once when the ctrl key is pressed.
					if (event.ctrlKey && !this.allowOverZoom)
						this.stopZooming();
				}
				break;

			case 'mouseup':
			case 'touchend':
				this.stopZooming();
				break;

			default:
				console.log(`Map::onTapZoomInOutButton UNEPXECTED EVENT ${event.type}`)
		}
	}

	onTourBuilderMouseUp(event)
	{
		// The user has released the mouse button somewhere, possibly on the map, or in the Tour Builder,

		// Ignore any mouseup events on the markerlayer since those are handled by the map.
		if (event.target === this.markerLayer)
			return;

		// Clear the dragging flag in the Map Editor. This handles the case where the user is dragging
		// over the map (dragging the map itself, or dragging a marker or crosshairs in the Map Editor), but then
		// drags off the map and releases the mouse button. If the flags are not cleared, and the user mouses back
		// over the map, dragging would resume even though the mouse button is not down. This mechanism is also
		// what allows the user to drag off and then back onto the map, while still holding down the mouse button,
		// and continue draggging. Both of these are very common cases.
		if (this.tour.editMode)
			this.editor.stopDragging();
	}

	onZoomInOutButtonMouseOver(event)
	{
		//console.log(`Map::onZoomInOutButtonMouseOver ${event.type}`);

		if (this.tour.editMode)
			this.editor.showZoomPanInformation();
	}

	panMap(deltaX_map, deltaY_map)
	{
		if (this.tour.editMode && this.editor.panningIsDisabled)
			return;

		// This method pans the map by an X and/or Y distance in pixels on the unscaled map. Ignore
		// the call when there's nothing to do as happens when the user stops moving the mouse or
		// their finger while panning, but requestAnimationFrame is continuing to call this method.
		if (deltaX_map === 0 && deltaY_map === 0)
			return;

		//console.log(`Map::panMap ${deltaX_map},${deltaY_map}`);

		this.validateAndUpdatePanXandPanY(deltaX_map, deltaY_map);
		this.drawMap();

		// Tell the Map Editor so it knows to redraw selected marker outlines and crosshairs in their new location
		if (this.tour.editMode)
			this.editor.mapWasPanned();
	}

	panMapAutomaticallyOnEachAnimationFrame()
	{
		function finalizeAutoPanning(_this)
		{
			console.log(`>>> finalizeAutoPanning ${_this.stopAutoPanningMap ? 'COMPLETED' : 'STOPPED'} ${_this.panX_map},${_this.panY_map}`);
			_this.autoPanningMap = false;
			_this.stopAutoPanningMap = false;
			_this.markerViewIdToSelectAfterAutoPan = 0;
			_this.flushMarkerLayerCache();
			_this.mapZoomPanStateChanged();
			_this.initializeDrawState();
			_this.allowSlowGraphics(true);
		}

		// Check to see if auto-panning has been stopped. This happens when the user attempts
		// to perform an operation such as selecting a marker or zooming the map during auto-panning.
		if (this.stopAutoPanningMap)
		{
			finalizeAutoPanning(this);
			this.drawMap();

			// When done panning a mobile map while waiting for content to load, show the waiting indicator
			// at the panned position of the marker that is being waited on so the user can see it until
			// the content loads and the mobile content panel displays.
			if (this.page.layout.usingMobileLayout && this.tour.busy.waiting)
				this.tour.busy.showWaitIndicator();

			return;
		}

		// Tell the pan curve object to calculate the next position when newPanX_map and newPanX_map are.
		// called. If the previous interation was the last, the object's completed flag will get set to true.
		this.autoPanCurve.iterate();

		// Check to see if auto-panning has completed.
		if (this.autoPanCurve.completed)
		{
			// Save the marker to be selected because finalizeAutoPanning sets it to zero.
			let markerViewIdToSelect = this.markerViewIdToSelectAfterAutoPan;

			finalizeAutoPanning(this);

			if (this.tour.editMode)
			{
				// Tell the Map Editor to select the marker and show it in the crosshairs.
				this.editor.setMarkerSelected(markerViewIdToSelect);
			}
			else
			{
				// For non-mobile layouts, select and blink the marker that the auto-pan was performed for.
				// The pan occurs first so that when the marker is selected, its popup will get displayed
				// at the marker's new position. For mobile layouts, the marker is selected and its content
				// panel displayed before panning begins so that the pannng logic knows if and how far to
				// over-pan the map so that the marker will not be covered by the content panel.
				if (markerViewIdToSelect !== 0 && !this.page.layout.usingMobileLayout)
					this.selectMarkerBlinkingAndShowPopup(markerViewIdToSelect);
			}
			return;
		}

		// Calculate the limits on how far the map can be panned left or up.
		let leftMostPanX_map = this.calculatePanXLimit();
		let topMostPayY_map = this.calculatePanYLimit();
		let isPortraitOrientation = this.page.layout.containerOrientationIsPortrait;
		if (isPortraitOrientation)
			topMostPayY_map -= this.convertScreenToMap(this.page.layout.contentPanelSize.h);
		else
			leftMostPanX_map -= this.convertScreenToMap(this.page.layout.contentPanelSize.w);

		// Get the new pan position.
		let newPanX_map = this.disableAutoPan ? this.autoPanCurve.endX : this.autoPanCurve.newPanX_map;
		let newPanY_map = this.disableAutoPan ? this.autoPanCurve.endY : this.autoPanCurve.newPanY_map;

		//console.log(`Map::panMapAutomaticallyOnEachAnimationFrame ${this.panX_map},${this.panY_map} ${newPanX_map},${newPanY_map}`);

		// Adjust the new pan position if necessary and update the map's pan settings.
		if (newPanX_map > 0)
			newPanX_map = 0;
		else if (newPanX_map < leftMostPanX_map)
			newPanX_map = leftMostPanX_map;
		this.panX_map = newPanX_map;
		
		if (newPanY_map > 0)
			newPanY_map = 0;
		else if (newPanY_map < topMostPayY_map)
			newPanY_map = topMostPayY_map;
		this.panY_map = newPanY_map;

		// Force the bounds and visibility of each marker to get recalculated in the new pan position.
		this.mapZoomPanStateChanged();

		this.drawMap();

		if (this.disableAutoPan)
			this.panMapAutomaticallyOnEachAnimationFrame();
		else
			requestAnimationFrame(this.panMapAutomaticallyOnEachAnimationFrame);
	}

	panMapOnEachAnimationFrame()
	{
		if (!this.panningMap)
			return;

		// Calculate the panning deltas based on the map's position when this method was last called
		// and the new position it should be moved to based on the current pointer position. Pan the map
		// by that amount and then request that this method be called again on the next animation frame.
		// This approach updates the map's position as often as possible based on the device's refresh
		// ability (usually 60 frames per second or about every 16ms) regardless of how fast the pointer
		// or the user's finger is moving. Any panning that occurs in between frames is ignored and
		// made up for on the next frame.

		let deltaX_map = this.panNewX_map - this.panX_map;
		let deltaY_map = this.panNewY_map - this.panY_map;
		this.panMap(deltaX_map, deltaY_map);

		requestAnimationFrame(this.panMapOnEachAnimationFrame);
	}

	pauseSlideShow()
	{
		// Prevent the next slide from showing, but allow the slide show to resume
		// where it left off if the user leaves and then comes back to this page.
		this.slideShowRunning = false;
	}

	get pointerIsOverMarker()
	{
		return this.markerUnderPointer !== null;
	}

	pointerMovedFromMapToMarker(pointer)
	{
		//console.log(`Map::pointerMovedFromMapToMarker ${this.markerUnderPointerViewId}`);
		this.selectMarkerUnderPointer(pointer);
	}

	pointerMovedFromMarkerToMap(previousMarkerUnderPointer)
	{
		//console.log(`Map::pointerMovedFromMarkerToMap ${previousMarkerUnderPointer.viewId}`);

		this.hideTooltip();

		if (this.page.hasPopup)
		{
			// Clear a timer that is waiting to select the marker. This happens when the
			// user mouses onto a marker and then off of it during the delay period.
			if (this.delayBeforeShowingPopupTimerRunning)
				this.clearPopupDelayTimer();

			if (this.markerIsSelected)
				this.handlePointerMovedOffMarker();

			return;
		}
		else
		{
			// When the pointer has just gone off of a marker, perform that marker's mouseout action.
			if (previousMarkerUnderPointer !== null && !this.tour.editMode)
				this.executeMouseOutAction(this.selectedMarker);
		}
	}

	pointerMovedFromMarkerToMarker(pointer)
	{
		//console.log(`Map::pointerMovedFromMarkerToMarker : pointer moved from marker ${this.selectedMarkerViewId} to ${this.markerUnderPointer.viewId}`);

		// Clear a timer that is waiting to select the marker that was just moved off of.
		if (this.page.hasPopup && this.delayBeforeShowingPopupTimerRunning)
			this.clearPopupDelayTimer();

		this.deselectMarkerAndClosePopup();
		this.selectMarkerUnderPointer(pointer);
	}

	pointerMovedOverMap()
	{
		//console.log(`Map::pointerMovedOverMap`);
	}

	pointerMovedOverSelectedMarker(pointer)
	{
		//console.log(`Map::pointerMovedOverSelectedMarker`);

		// Ignore this call for popup tours because no action is taken while mousing over the same marker.
		if (this.page.hasPopup)
			return;

		// For a tiled tour, this method is called when the user mouses off of and then back onto the
		// selected marker. The marker's mouseover action needs to be performed, and its tooltip needs to be
		// shown again, but the API for hotspot changed does not get called since the hotspot didn't change.
		this.showTooltip(this.selectedMarker, pointer);
		this.executeMouseOverAction(this.selectedMarker);
	}

	get popup()
	{
		return this.page.popup;
	}

	positionAndZoomMapToInitialFocus()
	{
		// Determine the map's initial zoom level.
		this.currentMapScale = this.convertMapFocusPercentToScale();

		// Calculate how the map needs to be panned to honor its focus point at that zoom level.
		let canvasW_map = this.convertScreenToMap(this.canvasW);
		let canvasH_map = this.convertScreenToMap(this.canvasH);
		let focusedPanX = -(this.mapFocusX - Math.round(canvasW_map / 2));
		let focusedPanY = -(this.mapFocusY - Math.round(canvasH_map / 2));
		this.updatePanXandPanY(focusedPanX, focusedPanY);

		// Verify the the pan settings are valid for the canvas and adjust if necessary. This needs
		// to be done because the user chooses the map's center while in the Map Editor which has a
		// size and aspect ratio that will almost always be different than the size and aspect ratio
		// of the map when displayed in a tour, especially on mobile devices. As such, the pan values
		// calculated above could cause the map to over-pan if not validated and adjusted.
		this.validateAndUpdatePanXandPanY(0, 0);

		// Since this method is called to set the map to its intial focused state, reset the map's draw state.
		this.initializeDrawState();
	}

	positionMapToMakeLocationVisibleOnCanvas(location, marker = null, selectMarkerAfterPositioning = true)
	{
		let locationIsMarker = marker != null;
		let locationIsVisible;
		let isPortraitOrientation = this.page.layout.containerOrientationIsPortrait;
		let leftMostPanX_map = this.calculatePanXLimit();
		let topMostPanY_map = this.calculatePanYLimit();
		let newPan;
		let mapIsOverPanned;
		let allowOverPan;
		let canvasW_map;
		let canvasH_map;

		function determineIfOverPanAllowed(_this)
		{
			// Over-panning is only allowed for mobile layouts.
			if (!_this.page.layout.usingMobileLayout)
				return false;

			// When no marker will be selected, there's no need to overpan.
			if (!selectMarkerAfterPositioning)
				return false;

			// When no marker is selected, there's no need to overpan.
			if (_this.selectedMarkerViewId === 0 && marker === null)
				return false;

			// When the selected marker is already visible, but no content panel is showing, there's no need to over-pan.
			if (marker !== null && marker.showsContentOnlyInTooltip && locationIsVisible)
				return false;

			// The content panel is displayed for the selected marker in a mobile layout. If the marker is not
			// already visible above or to the left of the content panel, over-panning will be required.
			return true;
		}

		function ignorePositioningRequest(_this)
		{
			// Test for all the cases where the positioning request should be ignored.

			// The pan center location has not been set as is the case when the tour is being resized.
			// The negative values come from panCenterLocation when it is not set to a location.
			if (!locationIsMarker && location.x < 0 && location.y < 0)
				return true;

			// Panning cannot occur when a desktop map is zoomed all the way out. A mobile map
			// might still require that the zoomed out map be over-panned to ensure that the
			// content panel does not cover the selected marker.
			if (_this.zoomedAllTheWayOut && !_this.page.layout.usingMobileLayout)
				return true;

			return false;
		}

		function positioningNeeded(_this)
		{
			// Positioning is always required when the marker is not visible.
			if (!locationIsVisible)
				return true;

			// Always do positioning when auto pan is disabled so that the map gets panned to the
			// best position even if the marker is already visible off to the side.
			if (_this.disableAutoPan)
				return true;

			// When zooming, the request is to reposition a visible marker to keep it in the best location.
			if (_this.zoomingMapInOut)
				return true;

			// Positioning may be required for a mobile layout when the content panel has been closed.
			if (mapIsOverPanned)
				return true;

			// When the marker is visible above or to the left of the mobile content panel, there's no need to over-pan.
			if (allowOverPan)
			{
				if (isPortraitOrientation)
				{
					let markerY_canvas = _this.convertMapToScreen(marker.y + _this.panY_map);
					let availableH_canvas = _this.canvasH - _this.page.layout.contentPanelSize.h;
					if (markerY_canvas < availableH_canvas)
						return false;
				}
				else
				{
					let markerX_canvas = _this.convertMapToScreen(marker.x + _this.panX_map);
					let availableW_canvas = _this.canvasW - _this.page.layout.contentPanelSize.w;
					if (markerX_canvas < availableW_canvas)
						return false;
				}
				return true;
			}

			// No positioning is needed since the marker is visible and none of the above conditions are true.
			return false;
		}

		function calculatePanPositionForMarker(_this)
		{
			newPan = { x: 0, y: 0 };

			// Start with pan offsets that will put the marker near the center of the canvas (the ideal position).
			// For mobile in portrait orientation, put y up into the top third of the canvas where it will never
			// be obscured by a partial content panel. An exception is when the route is a marker which needs to
			// be centered in case it is as large as the map area at the current zoom level.
			const xOffset = 2;
			const yOffset = allowOverPan && isPortraitOrientation && !marker.isRoute ? 3 : 2;

			let canvasCenterW_map = Math.round(canvasW_map / xOffset);
			let canvasCenterH_map = Math.round(canvasH_map / yOffset);
			newPan.x = Math.min(0, -location.x + canvasCenterW_map);
			newPan.y = Math.min(0, -location.y + canvasCenterH_map);

			if (allowOverPan)
			{
				// The ideal position calculated above is the starting position for the over-panned position.
				// Now determine the ideal vertical xor horizontal over-pan that is needed.
				validateOverPanPosition(_this);
			}
			else
			{
				// Over-panning is not allowed so choose an ideal, but valid pan position. This is always the case for
				// non-mobile layouts, but this logic also gets called to restore an over-panned mobile map to a
				// non-over-panned position when the user closes the content panel. If the map had been over panned to
				// keep the marker from being covered by the panel, this logic will pan it again to eliminate the over-pan.

				// If the offsets would over-pan the map, adjust them to their valid left and top limits. This happens
				// when the marker is too close to the right or bottom edge of the map to display it at the canvas center.
				if (newPan.x < leftMostPanX_map)
					newPan.x = leftMostPanX_map;
				if (newPan.y < topMostPanY_map)
					newPan.y = topMostPanY_map;
			};
		}

		function validateOverPanPosition(_this) 
		{
			if (_this.tour.flagDisableOverpanOptimization)
				return;

			// This method ensures that there will never be a gap between the bottom of the map and the top of the content panel
			// in portrait orientation, or between the right edge of the map and the left edge of the content panel is landscape
			// orientation. These cases can arise when calculatePanPositionForMarker sets the marker position to be near the
			// center of the available area, but the marker is near the bottom or right edge of the map such that placing the marker
			// near the center would over-pan the map too much and leave a gap. To address this, the logic below adjusts the x
			// or y pan amount to eliminat the gap.

			if (isPortraitOrientation)
			{
				// Determine how much vertical space is available for the map after subtracting the banner and title height.
				let mapAreaH_screen = _this.page.layout.containerSize.h - _this.page.layout.calculateScaledNonLayoutHeight(true);

				// Calculate the proposed location of the bottom of the map.
				let bottomEdgeOfMap_screen = _this.convertMapToScreen(_this.mapH_actual) + _this.convertMapToScreen(newPan.y)

				// Determine if there is a gap between the bottom of the map and the top of the content panel
				let gap_screen = mapAreaH_screen - _this.page.layout.contentPanelSize.h - bottomEdgeOfMap_screen;

				// Close the gap.
				if (gap_screen > 0)
					newPan.y += _this.convertScreenToMap(gap_screen);
			}
			else
			{
				// Determine how much horizontal space is available for the map. In landscape orientation, it's the entire container width.
				let mapAreaW_screen = _this.page.layout.containerSize.w;

				// Calculate the proposed location of the right side of the map.
				let rightEdgeOfMap_screen = _this.convertMapToScreen(_this.mapW_actual) + _this.convertMapToScreen(newPan.x);

				// Determine if there is a gap between the right side of the map and the left side of the content panel.
				let gap_screen = mapAreaW_screen - _this.page.layout.contentPanelSize.w - rightEdgeOfMap_screen;

				// Close the gap.
				if (gap_screen > 0)
					newPan.x += _this.convertScreenToMap(gap_screen);
			}
		}

		function calculatePanPositionForZooming(_this)
		{
			// Calculate the percentage distance the pan center location is from the upper left corner
			// of the canvas. Remember the distance as the basis for calculating the pan increment.
			let oldPct = { x: _this.panCenterLocation.x_screen / _this.canvasW, y: _this.panCenterLocation.y_screen / _this.canvasH };
			let newPct = Object.assign({}, oldPct);

			// Adjust the current pan position to put the location closer to the center of the canvas. The shift needs
			// to occur gradually on each zoom increment, otherwise the map will jump as it zooms. The logic tests to
			// see how far the location is from the center and shift it toward the center just a little bit.
			const maxShift = 0.01;
			const center = 0.50;

			if (oldPct.x < center - maxShift)
				newPct.x = oldPct.x + maxShift;
			else if (oldPct.x > center + maxShift)
				newPct.x = oldPct.x - maxShift;

			if (oldPct.y < center - maxShift)
				newPct.y = oldPct.y + maxShift;
			else if (oldPct.y > center + maxShift)
				newPct.y = newPct.y - maxShift;

			// Pan the map based on the percentages determined above.
			let x = Math.round(-location.x + (newPct.x * canvasW_map));
			let y = Math.round(-location.y + (newPct.y * canvasH_map));
			newPan = { x, y };
		}

		// Make a first determination as to whether is positioning is needed.
		if (ignorePositioningRequest(this))
			return false;

		// The location to position to will always be visible unless its a marker. A marker that is entirely or
		// partially off the canvas is considered to be not visible in which case the map will be positioned to
		// show the marker in an easily visible location.
		if (locationIsMarker)
			locationIsVisible = marker.isEntirelyVisibleOnCanvas;
		else
			locationIsVisible = true;

		// Set flags that are used for positioning on mobile devices where overpanning is sometimes necessary.
		mapIsOverPanned = this.panX_map < leftMostPanX_map || this.panY_map < topMostPanY_map;
		allowOverPan = determineIfOverPanAllowed(this);

		// Make a second determination as to whether positioning is needed.
		if (!positioningNeeded(this))
			return false;

		//console.log(`Map::positionMapToMakeLocationVisibleOnCanvas ${location.x}x${location.y} ${locationIsMarker} ${selectMarkerAfterPositioning}`);

		// Get the canvas dimensions in full size map image pixels.
		canvasW_map = this.convertScreenToMap(this.canvasW);
		canvasH_map = this.convertScreenToMap(this.canvasH);

		// Determine how the map should be positioned to make the marker visible. The logic for mobile layouts ensures
		// that the marker will appear above the content panel. For non-mobile layouts, when the location is a marker,
		// the pan position will get set to the marker's location. Otherwise, the positioning is being done in conjunction
		// with zooming in which case the call to positionMapToMakeLocationVisibleOnCanvas only positions incrementally.
		if (locationIsMarker)
			calculatePanPositionForMarker(this);
		else
			calculatePanPositionForZooming(this);

		// If the above calculations did not change the current pan position, there's nothing left to do.
		if (newPan.x === this.panX_map && newPan.y === this.panY_map)
			return false;

		// The pan position has changed.
		if (locationIsMarker)
		{
			// Auto-pan from the current position to the new position, redrawing the map on each pan animation frame.
			let viewId = selectMarkerAfterPositioning ? marker.viewId : 0;
			this.startAutoPanning(newPan.x, newPan.y, viewId);
		}
		else
		{
			// Update the pan settings for the current zoom level. The zooming logic will redraw the map.
			this.updatePanXandPanY(newPan.x, newPan.y);
			this.validateAndUpdatePanXandPanY(0, 0);
		}

		return true;
	}

	positionMapToMakeMarkerVisibleOnCanvas(viewId, selectMarkerAfterPositioning = true)
	{
		//console.log(`Map::positionMapToMakeMarkerVisibleOnCanvas ${viewId} ${selectMarkerAfterPositioning}`);

		if (viewId === 0)
			return false;

		let marker = this.getMarker(viewId);
		if (marker === null)
			return false;

		let markerLocation = { x: marker.x, y: marker.y };
		return this.positionMapToMakeLocationVisibleOnCanvas(markerLocation, marker, selectMarkerAfterPositioning);
	}

	reportMarkerCoords()
	{
		let coords = "";
		let c;
		let markers;

		if (this.editor.hybridEditingEnabled)
		{
			// When editing a hybrid marker, only record the coords for the hybrid. The coords change as the user
			// adds, resizes, or moves shapes within the hybrid. These kinds of actions changes the size of the
			// hybrid's bounding box which in turn changes the marker's center.
			markers = [this.editor.hybridMarkerBeingEdited];
		}
		else
		{
			// Report the coords for all the markers.
			markers = this.markers;
		}

		for (const marker of markers)
		{
			let locked = marker.isLocked ? 1 : 0;
			c = marker.viewId + "," + marker.pctX + "," + marker.pctY + "," + marker.rotationDegrees + "," + locked + ";";
			coords += c;
		}

		if (coords.length > 0)
		{
			// Remove trailing ";"
			coords = coords.substring(0, coords.length - 1);
		}

		let markerCoords = maGetElementByPageId("MarkerCoords");
		if (markerCoords)
		{
			// Store the changed coords in the hidden MarkerCoords field that will get posted back to the server.
			markerCoords.value = coords;
			maChangeDetected();
		}

		//console.log(`Map::reportMarkerCoords ${coords}`);
	}

	// This method is a copy of the https://github.com/viliusle/Hermite-resize hermite.js resample_single()
	// function which uses a single core for processing. Hermite also has a function that supports workers and
	// will use multiple cores, and makes a callback when complete. It seems to perform better on devices that
	// don't have a lot of CPU power like the Galaxy A01 phone and the Fire 8 tablet, but tests on iPhone 7
	// iPhone 12, and iPad Pro show that it actually performs worse than the single threaded version. We're
	// using the single core function because a) we can easily inline the code here instead of importing the
	// Hermite JS file and b) it avoids introducing multithreading and possible problems that might introduce.
	resampleImage(canvas, width, height)
	{
		var width_source = canvas.width;
		var height_source = canvas.height;
		width = Math.round(width);
		height = Math.round(height);

		var ratio_w = width_source / width;
		var ratio_h = height_source / height;
		var ratio_w_half = Math.ceil(ratio_w / 2);
		var ratio_h_half = Math.ceil(ratio_h / 2);

		var ctx = this.getCanvasContext(canvas);
		var img = ctx.getImageData(0, 0, width_source, height_source);
		var img2 = ctx.createImageData(width, height);
		var data = img.data;
		var data2 = img2.data;

		for (var j = 0; j < height; j++)
		{
			for (var i = 0; i < width; i++)
			{
				var x2 = (i + j * width) * 4;
				var weight = 0;
				var weights = 0;
				var weights_alpha = 0;
				var gx_r = 0;
				var gx_g = 0;
				var gx_b = 0;
				var gx_a = 0;
				var center_y = j * ratio_h;

				var xx_start = Math.floor(i * ratio_w);
				var xx_stop = Math.ceil((i + 1) * ratio_w);
				var yy_start = Math.floor(j * ratio_h);
				var yy_stop = Math.ceil((j + 1) * ratio_h);
				xx_stop = Math.min(xx_stop, width_source);
				yy_stop = Math.min(yy_stop, height_source);

				for (var yy = yy_start; yy < yy_stop; yy++)
				{
					var dy = Math.abs(center_y - yy) / ratio_h_half;
					var center_x = i * ratio_w;
					var w0 = dy * dy; //pre-calc part of w
					for (var xx = xx_start; xx < xx_stop; xx++)
					{
						var dx = Math.abs(center_x - xx) / ratio_w_half;
						var w = Math.sqrt(w0 + dx * dx);
						if (w >= 1)
						{
							//pixel too far
							continue;
						}
						//hermite filter
						weight = 2 * w * w * w - 3 * w * w + 1;
						var pos_x = 4 * (xx + yy * width_source);
						//alpha
						gx_a += weight * data[pos_x + 3];
						weights_alpha += weight;
						//colors
						if (data[pos_x + 3] < 255)
							weight = weight * data[pos_x + 3] / 250;
						gx_r += weight * data[pos_x];
						gx_g += weight * data[pos_x + 1];
						gx_b += weight * data[pos_x + 2];
						weights += weight;
					}
				}
				data2[x2] = gx_r / weights;
				data2[x2 + 1] = gx_g / weights;
				data2[x2 + 2] = gx_b / weights;
				data2[x2 + 3] = gx_a / weights_alpha;
			}
		}

		// Clear and resize the canvas.
		canvas.width = width;
		canvas.height = height;

		// Draw the downsized image onto the canvas.
		ctx.putImageData(img2, 0, 0);
	};

	resetMapPointerState()
	{
		// This method gets called when the mouse moves off the map or off of a popup
		// and this invalidates the current state. Setting the state back to the Start
		// state forces the correct state to be determined as soon as the mouse if over
		// the map again.
		this.pointerState = this.POINTER_STATE_START;
	}

	restoreMarkerShapeAppearance(selected, viewIdList, draw = true)
	{
		console.log("restoreMarkerShapeAppearance " + viewIdList);

		let markers = this.createMarkerArrayFromMarkerIdList(viewIdList);

		for (let marker of markers)
		{
			if (!marker.hasPrivateMarkerStyle)
				continue;

			// Restore the marker's selected or normal appearance by virtue of removing
			// its private marker style's selected or normal appearance.
			marker.privateMarkerStyle.setBaseProperties(selected, null);
		}

		if (draw)
			this.graphics.drawIntersectingMarkers(markers, selected);
	}

	restoreMarkerStyleAppearance(selected, styleId, draw = true)
	{
		let markerStyle = this.getMarkerStyle(styleId);
		if (markerStyle === null)
			return;

		markerStyle.setChangedProperties(selected, null);

		if (draw)
		{
			let markers = this.findMarkersThatUseStyle(styleId);
			this.graphics.drawIntersectingMarkers(markers, selected);
		}
	}

	selectMarker(viewId, pointer)
	{
		//console.log(`Map::selectMarker ${viewId}`);

		let marker = this.getMarker(viewId);
		Runtime__$$.assert(marker !== null, `No marker found for ${viewId}`);

		// When a marker is selected, stop it or any other markers from blinking.
		this.stopBlinking();

		// Deselect the previously selected marker before selecting a different marker. This
		// method can be called to reselect the same marker, but only when a delayed popup needs
		// to be shown. In that case, the reselect causes the marker's content to get loaded.
		if (marker.viewId !== this.selectedMarkerViewId)
		{
			this.deselectMarker();

			// Close the popup if there is one, but don't close the content panel on a mobile device
			// in order to avoid the bounce that can occur when hiding the panel for one marker and
			// then immediately showing it again for another. There's no problem if the content for
			// both markers is already loaded, but when the new marker's image has to get loaded, even
			// if it loads immediately, the slight delay between hiding and showing makes it look like
			// the panel slide partially down and then bounced back up again.
			if (this.page.hasPopup)
				this.page.closePopup();

			this.selectedMarker = marker;
			this.tour.api.callbackHotspotChanged(this.page.getView(viewId));
		}

		// Show the marker's view unless it has none (only has a tooltip) or the user chose to never show it.
		let okayToShowView = !marker.showsContentOnlyInTooltip && marker.showContentEvent !== this.SHOW_CONTENT_NEVER;
		if (okayToShowView && !this.delayBeforeShowingPopupTimerRunning)
			this.page.showViewForSelectedMarker(pointer);

		// Draw the marker unless the map is zooming in which case all the markers will get redrawn at the new zoom level.
		if (!marker.isStatic && !this.zoomingMap)
			this.drawMarkerAndNeighbors(marker, true);

		// Hide the tooltip for the previously selected marker.
		this.hideTooltip();

		this.showTooltipForSelectedMarker(pointer, marker);
	}

	selectMarkerAndShowPopup({ viewId, pointer = null, pin = false})
	{
		//console.log(`Map::selectMarkerAndShowPopup ${viewId}`);

		this.selectMarker(viewId, pointer);

		if (this.selectedMarker.showsContentOnlyInTooltip)
			return;

		if (this.page.hasPopup)
			this.popup.showPopup(pin);
	}

	selectMarkerBlinkingAndShowPopup(viewId)
	{
		let marker = this.getMarker(viewId);
		if (marker.isRoute)
			return;
		console.log(`Map::selectMarkerBlinkingAndShowPopup ${viewId}`);
		this.selectMarkerAndShowPopup({ viewId: viewId, pin: true });
		marker.setBlink(this.blinkCount);
	}

	selectMarkerChosenFromDirectory(viewId)
	{
		let positionedMap = this.positionMapToMakeMarkerVisibleOnCanvas(viewId);

		// When using a non-mobile layout, the auto-panning used to position the map, causes the marker to
		// get selected. In that case, this method can return. But if the map did not get positioned because
		// the marker was already visible, or when using a mobile layout, select the marker.
		if (positionedMap && !this.page.layout.usingMobileLayout)
			return;

		this.selectMarkerBlinkingAndShowPopup(viewId);
	}

	selectMarkerUnderPointer(pointer) 
	{
		this.stopCurrentOperation();

		if (this.delayBeforeShowingPopup) 
		{
			// Set a timer to wait before showing the popup. If the timer is already set, then the
			// user must be mousing over the same marker, so just wait for the previous timer to fire.
			if (this.popupDelayTimerId !== 0)
				return;
			if (!this.markerUnderPointer.showsContentOnlyInTooltip)
				this.popupDelayTimerId = setTimeout(this.showPopupAfterDelay, this.popup.delay, this.markerUnderPointer.viewId, pointer);

			// Select the marker immediately now that the timer is running. The marker will get selected,
			// but its content won't load unless the timer expires and the popup appears. This way if the
			// user is quickly mousing from one marker to the next, processing won't be wasted on popups 
			// never show. This is especially important if the content is coming from Live Data,
			this.selectMarker(this.markerUnderPointer.viewId, pointer);
		}

		else
		{
			// Select the marker and show its popup immediately, but first clear a timer that is waiting
			// to deselect the marker. This happens when the user mouses off a marker and back onto a
			// marker during the delay period.
			if (this.delayBeforeDeselectingMarkerTimerRunning)
				this.clearPopupDelayTimer();

			this.selectMarkerAndShowPopup({ viewId: this.markerUnderPointer.viewId, pointer: pointer });
		}

		// Perform the selected marker's mouseover action.
		this.executeMouseOverAction(this.selectedMarker);
	}

	get selectedMarkerViewId()
	{
		return this.selectedMarker ? this.selectedMarker.viewId : 0;
	}

	setCanvasSize(mapAreaSize)
	{
		this.canvasW = Math.min(mapAreaSize.w, this.mapW_actual);
		this.canvasH = Math.min(mapAreaSize.h, this.mapH_actual);

		//console.log(`Map::setCanvasSize ${this.canvasW} x ${this.canvasH}`);
	}

	setMapInsetRectSize({ resize })
	{
		if (!this.mapInsetEnabled)
			return;

		//console.log(`Map::setMapInsetRectSize ${resize}`);

		// Calculate the map inset's scale relative to the full size map image.
		this.mapInsetScale = this.calculateMapInsetScale(this.mapW_actual, this.mapH_actual);

		// Calculate the size of the map inset.
		let w = this.mapW_actual * this.mapInsetScale;
		let h = this.mapH_actual * this.mapInsetScale;
		let rect = { w: Math.round(w), h: Math.round(h) };

		// Add a margin around the inset.
		const INSET_MARGIN = 3;

		// Calculate the location of the inset.
		switch (this.mapInsetLocation)
		{
			case 2:
				// Upper right
				rect.x = this.canvasW - (rect.w + (INSET_MARGIN * 2));
				rect.y = INSET_MARGIN * 2;

				// Shift the inset to the left if the nav button is on its right.
				if (this.page.layout && this.tour.navButtonLocation === this.page.layout.NAV_MAP_RIGHT)
					rect.x -= 28;
				break;

			case 3:
				// Lower right
				rect.x = this.canvasW - (rect.w + (INSET_MARGIN * 2));
				rect.y = this.canvasH - (rect.h + (INSET_MARGIN * 2));
				break;

			case 4:
				// Lower left
				rect.x = INSET_MARGIN * 2;
				rect.y = this.canvasH - (rect.h + (INSET_MARGIN * 2));
				break;

			default:
				break;
		}

		this.mapInsetRect = rect;

		// Set a new canvas size, but only if the size has changed, because setting the size of a canvas,
		// even to the same size, clears the canvas and its state.
		if (this.insetLayer.width !== this.mapInsetRect.w || this.insetLayer.height !== this.mapInsetRect.h)
		{
			this.insetLayer.width = this.mapInsetRect.w;
			this.insetLayer.height = this.mapInsetRect.h;
		}

		this.insetLayer.style.left = this.mapInsetRect.x + 'px';
		this.insetLayer.style.top = this.mapInsetRect.y + 'px';
	}

	setMapZoomLevel(percent)
	{
		this.zoomMap(percent, false);
	}

	setMarkerAppearanceAsNormalOrSelected(viewIdList, selected)
	{
		console.log("setMarkerAppearanceAsNormalOrSelected " + viewIdList);

		let markersList = this.createMarkerArrayFromMarkerIdList(viewIdList);

		// Loop over all the markers and for each one, see if it's in the view Id list.
		// If it is, flag it as appearing selected or normal depending on the request
		// and then redraw it. This algorithm causes all the markers in the list to be
		// redrawn in their correct stacking order with the proper ones showing as selected.
		// The flag ensures that if a marker is redrawn again elsewhere it will still appear
		// selected. The flag only gets cleared here (if selected === false) or when the
		// marker gets deselected e.g. when you mouse over it and off of it again.

		for (const marker of this.markers)
		{
			let redraw = false;

			for (const m of markersList)
			{
				if (m.viewId === marker.viewId)
				{
					marker.appearsSelected = selected;
					redraw = true;
					break;
				}
			}

			if (redraw)
				this.drawMarkerAndNeighbors(marker, false);
		}

		this.drawAllMarkers();
	}

	setMarkerAppearanceToggled(toggled)
	{
		this.showMarkerAppearanceToggled = toggled;
	}

	setMarkerListBlink(viewIdList, blinkCount)
	{
		console.log("setMarkerListBlink " + viewIdList + " : " + blinkCount);

		let markers = this.createMarkerArrayFromMarkerIdList(viewIdList);

		for (let index in markers)
		{
			let marker = markers[index];
			marker.setBlink(blinkCount);
		}
	}

	setMarkerListDisabled(viewIdList, isDisabled)
	{
		let markers = this.createMarkerArrayFromMarkerIdList(viewIdList);

		for (let index in markers)
		{
			let marker = markers[index];
			marker.isDisabled = isDisabled === true;
			console.log("setMarkerListDisabled" + " : " + marker.viewId + " " + isDisabled);
		}
	}

	setMarkerListHidden(viewIdList, isHidden)
	{
		console.log("setMarkerListHidden " + viewIdList + " : " + isHidden);
		let markers = this.createMarkerArrayFromMarkerIdList(viewIdList);
		let drewRoute = false;

		for (let index in markers)
		{
			let marker = markers[index];
			if (marker.isHidden !== isHidden)
			{
				marker.isHidden = isHidden === true;

				// Force the bounds to be recalculated because the bound contain a flag indicating if
				// the marker is hidden at the current zoom level. Changing whether the marker is
				// hidden affects how that flag should be set.
				marker.updateBounds();

				this.drawMarkerAndNeighbors(marker, marker.viewId === this.selectedMarkerViewId);

				if (marker.isRoute)
					drewRoute = true;
			}
		}

		// Flush the marker cache if a route was drawn because the appearance of routes is often brief,
		// and routes are usually drawn and hidden dynamically by JavaScript. If cached, they can end
		// up remaining visible unless the cache is explicitly flushed as is done here.
		if (drewRoute)
			this.flushMarkerLayerCache();
	}

	setMarkerListStatic(viewIdList, isStatic)
	{
		let markers = this.createMarkerArrayFromMarkerIdList(viewIdList);

		for (let index in markers)
		{
			let marker = markers[index];
			marker.isStatic = isStatic === true;
			console.log("setMarkerListStatic" + " : " + marker.viewId + " " + isStatic);
		}
	}

	setMarkerOnTop(viewId)
	{
		console.log("setMarkerOnTop " + viewId);

		// Get the position of this marker in the marker array.
		let marker = null;
		let position = -1;
		for (let index in this.markers)
		{
			marker = this.markers[index];
			if (marker.viewId === viewId)
			{
				position = index;
				break;
			}
		}

		if (position !== -1)
		{
			// Remove the marker from its current position...
			this.markers.splice(position, 1);

			// And put it at the end of the array so that it will be on top of all other markers.
			this.markers.push(marker);
		}

		this.drawMarkerAndNeighbors(marker, marker.viewId === this.selectedMarkerViewId);
	}

	setMarkerTransparencyToggled(toggled)
	{
		this.showMarkerTransparencyToggled = toggled;
	}

	setRoutes(routes)
	{
		if (!routes || routes === null || routes.length == 0 || !Array.isArray(routes))
		{
			this.routes = [];
			return;
		}

		// Set the routes to be drawn every time the map is drawn.
		this.routes = routes;
	}

	// This method is from https://gist.github.com/mikecao/65d9fc92dc7197cb8a7c.
	// The mix parameter is a sharpening factor in the range 0 to 1, e.g. 0.9 for very sharp edges
	sharpenImage(ctx, w, h, mix)
	{
		var x, sx, sy, r, g, b, a, dstOff, srcOff, wt, cx, cy, scy, scx,
			weights = [0, -1, 0, -1, 5, -1, 0, -1, 0],
			katet = Math.round(Math.sqrt(weights.length)),
			half = (katet * 0.5) | 0,
			dstData = ctx.createImageData(w, h),
			dstBuff = dstData.data,
			srcBuff = ctx.getImageData(0, 0, w, h).data,
			y = h;

		while (y--)
		{
			x = w;
			while (x--)
			{
				sy = y;
				sx = x;
				dstOff = (y * w + x) * 4;
				r = 0;
				g = 0;
				b = 0;
				a = 0;

				for (cy = 0; cy < katet; cy++)
				{
					for (cx = 0; cx < katet; cx++)
					{
						scy = sy + cy - half;
						scx = sx + cx - half;

						if (scy >= 0 && scy < h && scx >= 0 && scx < w)
						{
							srcOff = (scy * w + scx) * 4;
							wt = weights[cy * katet + cx];

							r += srcBuff[srcOff] * wt;
							g += srcBuff[srcOff + 1] * wt;
							b += srcBuff[srcOff + 2] * wt;
							a += srcBuff[srcOff + 3] * wt;
						}
					}
				}

				dstBuff[dstOff] = r * mix + srcBuff[dstOff] * (1 - mix);
				dstBuff[dstOff + 1] = g * mix + srcBuff[dstOff + 1] * (1 - mix);
				dstBuff[dstOff + 2] = b * mix + srcBuff[dstOff + 2] * (1 - mix);
				dstBuff[dstOff + 3] = srcBuff[dstOff + 3];
			}
		}
		ctx.putImageData(dstData, 0, 0);
	}

	showLoadingMessage(message, show = false)
	{
		this.statusArea.innerHTML = message;
		this.statusArea.style.visibility = show ? 'visible' : 'hidden';
	}

	showMarkerSelected(viewId, selected)
	{
		console.log("showMarkerSelected " + viewId + " " + selected);
		this.setMarkerAppearanceAsNormalOrSelected(viewId, selected);
	}

	showNextSlide()
	{
		if (!this.slideShowRunning)
			return;

		console.log(`Map::showNextSlide ${this.nextMarkerIndex}`);

		let lookingForNextSlide = true;
		let marker;

		while (lookingForNextSlide)
		{
			// Get the marker for the next slide.
			marker = this.markers[this.nextMarkerIndex];
			this.nextMarkerIndex++;
			if (this.nextMarkerIndex >= this.markers.length)
				this.nextMarkerIndex = 0;

			if (!(marker.isHidden || marker.isDisabled || marker.isRoute))
				lookingForNextSlide = false;
		}

		// Close the current popup, resposition the map, then show the popup for the next slide.
		// When a slide show is running, the map is auto-panned very quickly into it's new position.
		//If normal (not fast) panning was done, the new popup would open in pre-pan position and
		// then jump to the new position. That still happens but so quickly that you shouldn't notice.
		this.page.closePopup();
		this.positionMapToMakeMarkerVisibleOnCanvas(marker.viewId);
		this.selectMarkerAndShowPopup({ viewId: marker.viewId });

		// Set a timer to show the next slide. If the slide show gets stopped in the
		// meantime, this method will still get called when the timer fires, but it
		// won't do anything since the slideShowRunningFlag will be false.
		setTimeout(this.showNextSlide, this.page.slideShowInterval);
	}

	showPopupAfterDelay()
	{
		//console.log(`Map::showPopupAfterDelay ${this.selectedMarkerViewId}`);
		this.popupDelayTimerId = 0;

		// The marker is already selected, but its content did not get loaded because the delay timer was running.
		// Make sure the marker didn't already get deselected, then select it again to get its content loaded and
		// to move the popup to the current mouse location (for popups that follow the mouse).
		if (this.selectedMarkerViewId !== 0)
			this.selectMarkerAndShowPopup({ viewId: this.selectedMarkerViewId, pointer: this.mouse });
	}

	showTooltip(marker, mouse)
	{
		let tooltip = marker.tooltip;

		//console.log(`Tour::showTooltip '${tooltip}' for ${marker.viewId}`);

		if (Runtime__$$.trim(tooltip).length === 0)
		{
			// Users can type a space for the tooltip text if they don't want anything to display.
			return;
		}

		this.tooltipElement.innerHTML = tooltip;
		this.tooltipElement.style.visibility = "visible";
		this.showingTooltip = true;

		if (this.slideShowRunning)
		{
			// When the slideshow is running, use the marker's location as the tooltip mouse location.
			let bounds = marker.getBounds();
			let point = { x: bounds.centerX_screen, y: bounds.centerY_screen };
			this.moveTooltipToCoordinates(point);
		}
		else
		{
			this.moveTooltipToMouseLocation(mouse);
		}
	}

	showTooltipForSelectedMarker(pointer, marker)
	{
		// Ignore calls to this method when the pointer is not over the selected marker except during a slide show.
		if (pointer === null && !this.slideShowRunning)
			return;

		let view = this.page.getView(marker.viewId);
		let hasNoVisibleContent = view.hasNoContentForLayout();
		let okToShowTooltip = true;

		if (this.page.hasPopup)
			okToShowTooltip = marker.showsContentOnlyInTooltip || hasNoVisibleContent;
		else if (this.page.layout.usingMobileLayout)
			okToShowTooltip = marker.showsContentOnlyInTooltip || hasNoVisibleContent;

		if (okToShowTooltip)
			this.showTooltip(marker, pointer);
	}

	startAutoPanning(panX, panY, viewId)
	{
		// Don't auto-pan the map while in the Map Editor except when selecting the marker
		// from either the hotspots dropdown or by clicking one of the hotspot thumbnails.
		// The prevents the map from auto-panning while you are selecting a marker that is
		// partially off-screen which can be confusing/unexpected.
		if (this.tour.editMode && !selectingMarkerFromList)
			return;

		console.log(`Map::startAutoPanning FROM ${this.panX_map},${this.panY_map} TO ${panX},${panY}`)
		let fastPan = this.slideShowRunning;
		this.markerViewIdToSelectAfterAutoPan = viewId;
		this.autoPanCurve = new AutoPanCurve(this, panX, panY, fastPan);
		this.allowSlowGraphics(false);
		this.autoPanningMap = true;
		this.stopAutoPanningMap = false;
		this.panMapAutomaticallyOnEachAnimationFrame();
	}

	startSlideShow()
	{
		// Don't allow an API call to run the slide show on an info page.
		if (this.page.isDataSheet)
			return;

		// Get the index of the first hotspot. If nextMarkerIndex is already set, this map's page
		// was previously loaded and the slideshow was running. Pick up where it left off.
		if (this.nextMarkerIndex === 0)
		{
			for (const index in this.markers)
			{
				let marker = this.markers[index];
				if (marker.viewId === this.page.firstViewId)
				{
					this.nextMarkerIndex = index;
					break;
				}
			}
		}

		this.slideShowRunning = true;
		this.showNextSlide();
	}

	startZooming(pointer)
	{
		this.zoomingMap = true;

		// Record the current mouse or pinch location which is a distance
		// in screen pixels from the upper left corner of the map canvas.
		this.panCenterLocation.x_screen = pointer.x;
		this.panCenterLocation.y_screen = pointer.y;

		// Convert the screen location to its location on the full map image.
		let panX_screen = this.convertMapToScreen(this.panX_map);
		let panY_screen = this.convertMapToScreen(this.panY_map);
		this.panCenterLocation.x_map = this.convertScreenToMap(pointer.x - panX_screen);
		this.panCenterLocation.y_map = this.convertScreenToMap(pointer.y - panY_screen);

		// Hide the mobile content panel whenever zooming starts in case it was positioned up near the
		// bottom of the zoomed out map. If the user zooms in, which will cause the bottom of the map to
		// move down, the panel will need to be positioned lower the next time it is opened. If it doesn't
		// get closed here, the map will zoom behind the panel, possibly past its bottom.
		this.page.layout.hideContentPanel();

		//console.log(`Map::startZooming ${this.panCenterLocation.x_map}x${this.panCenterLocation.y_map}`);
	}

	stopBlinking()
	{
		for (const marker of this.markers)
			marker.setBlink(0);
	}

	stopCurrentOperation()
	{
		//console.log(`Map::stopCurrentOperation`);

		if (this.page.isDataSheet)
			return;

		if (this.autoPanningMap)
			this.stopAutoPanningMap = true;

		if (this.tour.editMode)
			return;

		this.stopSlideShow();

		this.page.stopWaitingForViewContentToLoad(true);
	}

	stopSlideShow()
	{
		if (this.slideShowRunning)
		{
			console.log(`Map::stopSlideShow`);
			this.slideShowRunning = false;
			this.hideTooltip();

			// Tell the page not to resume the slide show if the user leaves and comes back to this page.
			this.page.userStoppedSlideShow();
		}
	}

	stopPanning()
	{
		//console.log(`Map::stopPanning`);
		this.panningMap = false;
		this.lastPanEventType = '';

		this.allowSlowGraphics(true);
		this.flushMarkerLayerCache();

		this.drawHybridMarkerOutline();
	}

	stopZooming()
	{
		//console.log(`Map::stopZooming`);
		this.zoomingMap = false;
		this.zoomPercent = 0;

		// Set the pan center values to -1 to indicate that the center location is not set.
		this.panCenterLocation.x_map = -1;
		this.panCenterLocation.y_map = -1;
		this.panCenterLocation.x_screen = -1;
		this.panCenterLocation.y_screen = -1;

		this.pinchingMap = false;
		this.currentPinchDistance = 0;

		this.allowSlowGraphics(true);
		this.flushMarkerLayerCache();

		this.drawHybridMarkerOutline();
	}

	updatePanXandPanY(x, y)
	{
		//console.log(`Map::updatePanSettings ${this.panX_map},${this.panY_map} >>> ${x},${y}`);
		this.panX_map = x;
		this.panY_map = y;
		this.flushMarkerLayerCache();
	}

	validateAndUpdatePanXandPanY(deltaX_map, deltaY_map)
	{
		// This method changes the map's panning amount. When the delta values are non-zero, the method
		// attempts to pan the map left/right and/or up/down by those amounts, but limits the pan if the
		// distances would cause the map to shift too far. The  method is also called immediately after
		// the map has been zoomed out to determine if existing pan settings are acceptable, and if so,
		// to adjust them to prevent the zoom from causing the map to pan incorrectly.

		// When auto panning the map, assume that the pan position is valid, especially for a mobile
		// map that might be getting over-panned to what would normally be an invalid position.
		if (this.autoPanningMap)
			return;

		//console.log(`Map::validateAndUpdatePanXandPanY ${this.currentMapScale}  ${deltaX_map},${deltaY_map}`);

		let newPanX_map = this.panX_map;
		let newPanY_map = this.panY_map;

		// Determine if it's okay to pan left or right.
		let leftMostPanX_map = this.calculatePanXLimit();

		// Determine the new pan X.
		newPanX_map = this.panX_map + deltaX_map;

		if (newPanX_map > 0)
		{
			// The delta would cause the map to shift right too far such that it would leave
			// a gap between the left edge of the map and the left edge of the canvas.
			// Set the new X to be the left edge of the canvas.  
			newPanX_map = 0;
		}
		else if (newPanX_map < leftMostPanX_map)
		{
			// The pan left distance would leave a gap between right edge of map and the right edge of the canvas.
			newPanX_map = leftMostPanX_map;
		}

		// Determine if it's okay to pan up or down.
		let topMostPanY_map = this.calculatePanYLimit();

		// Determine the new pan Y.
		newPanY_map = this.panY_map + deltaY_map;

		if (newPanY_map > 0)
		{
			// The delta would cause the map to shift down too far such that it would leave
			// a gap between the top edge of the map and the top edge of the canvas.
			// Set the new Y to be the top edge of the canvas.  
			newPanY_map = 0;
		}
		else if (newPanY_map < topMostPanY_map)
		{
			// The pan up distance would leave a gap between the bottom edge of map and the bottom edge of the canvas.
			newPanY_map = topMostPanY_map;
		}

		if (this.page.hasPopup && this.popup.isShowing)
			this.deselectMarkerAndClosePopup();
		else
			this.hideTooltip();

		if (this.panX_map !== newPanX_map || this.panY_map !== newPanY_map)
		{
			this.updatePanXandPanY(newPanX_map, newPanY_map);
			this.mapZoomPanStateChanged();
		}
	}

	waitingForMarkerImagesToLoad()
	{
		if (!this.allMarkerImagesLoaded && this.waitForMarkerImagesAttempts < 100)
		{
			this.waitForMarkerImagesAttempts++;

			// Determine if all the marker images are loaded. If not, try again after a short delay.
			for (let index in this.markers)
			{
				let marker = this.markers[index];

				// Note that a marker's image is considered loaded if it has no image.
				let loadedN = !marker.hasImageN || marker.imgLoadedN;
				let loadedS = !marker.hasImageS || marker.imgLoadedS;

				if (!loadedN || !loadedS)
				{
					console.log(`Map::waitingForMarkerImagesToLoad : ${this.waitForMarkerImagesAttempts}`);
					let delayPeriod = 50;
					setTimeout(this.drawAllMarkers, delayPeriod);
					return true;
				}
			}
			this.allMarkerImagesLoaded = true;
		}

		return false;
	}

	get zoomedAllTheWayIn()
	{
		return this.currentMapScale >= 1.0 && !this.allowOverZoom;
	}

	get zoomedAllTheWayOut()
	{
		// Allow a small tolerance to prevent the zoom out control from appearing when the
		// map is virtually zoomed out, but not exactly due to the precise scaling arithmetic.
		let tolerance = this.tour.editMode ? 0 : 0.005;
		return this.currentMapScale <= this.zoomedOutMapScale + tolerance;
	}

	get zoomingDisabled()
	{
		// When the zoom all the way in and out values are the same, the map is not big enough to zoom.
		return this.zoomedAllTheWayIn && this.zoomedAllTheWayOut;
	}

	zoomMap(delta, deltaIsIncrement = true, validatePanForResize = false)
	{
		// This method can be called to zoom the map by an increment like 1% or to zoom the
		// map to an absolute zoom percentage. To do the latter, the caller specifies that
		// the delta is not an increment. The API::setMapZoomLevel method is such a caller.
		// The caller can also indicate that the map's current pan position needs to be
		// validated after the zoom. This is done automatically whenever the map zooms out
		// as explained in comments below, but is manually requested when the zoom is a result
		// of the map being resized because the tour size changed.

		//console.log(`Map::zoomMap ${delta} ${this.currentMapScale}`);

		this.zoomingMapInOut = true;
		let newMapScale;

		// Determine the new map scale.
		if (deltaIsIncrement)
		{
			// Make sure the delta is valid.
			let deltaAbs = Math.abs(delta);
			if (deltaAbs < 0.001 || deltaAbs > 100)
				delta = delta < 0 ? -1 : 1;

			// Increase the map scale by 1% times the delta.
			let percent = this.currentMapScale * 100;
			percent += delta;

			// Adjust the resulting percentage to be a multiple of 5, but only when incrementing by 5.
			// This deals with rounding issues and the case where when the map is all the way zoomed out, its
			// percentage won't be multiple of 5, but this logic bumps it to the next 5% on the first zoom-in.
			if (deltaAbs === 5)
			{
				while (Math.round(percent) % 5 !== 0)
					percent += delta < 0 ? 1 : -1;
			}

			// Calculate the new map scale, but don't apply it until it has been validated as acceptable.
			newMapScale = percent / 100;
		}
		else
		{
			// When the delta is not an increment, it's an absolute percentage.
			newMapScale = delta / 100;
			delta = 0;
		}

		// Don't allow a zoom that would make the map smaller than the map area or greater
		// than 100% unless over-zooming is enabled in which case allow zoom-in up to 1000%.
		if (newMapScale > 1.0)
		{
			if (this.zoomLimit > 100)
			{
				let scaleLimit = this.zoomLimit / 100;
				newMapScale = newMapScale > scaleLimit ? scaleLimit : newMapScale;
			}
			else if (this.allowOverZoom)
			{
				newMapScale = newMapScale > 10.0 ? 10.0 : newMapScale;
			}
			else
			{
				newMapScale = 1.0;
			}
		}
		else if (newMapScale < this.zoomedOutMapScale)
		{
			newMapScale = this.zoomedOutMapScale;
		}

		// Show the zoom-in control in red when the map is over-zoomed while in edit mode.
		if (this.tour.editMode)
			this.zoomInControl.style.backgroundColor = newMapScale > 1.0 ? this.OVER_ZOOM_COLOR : this.NORMAL_ZOOM_COLOR;

		// If the zoom level has not changed, the map is all the way zoomed in or out.
		// An exception is when this method is called to reize the map when the tour is
		// resized. In that case, currentMapScale gets updated to the new size before this
		// method has been called which makes it seem that the zoom level has not changed.
		if (newMapScale === this.currentMapScale && !validatePanForResize)
		{
			if (this.zoomingMap && !this.pinchingMap)
				this.stopZooming();
			this.zoomingMapInOut = false;
			return;
		}

		// Accept the new scale.
		newMapScale = Runtime__$$.roundFloat(newMapScale);
		this.currentMapScale = newMapScale;

		// Close the popup and deselected its marker. For a tiled layout, the marker stays selected.
		if (this.page.hasPopup)
			this.deselectMarkerAndClosePopup();

		// Validate and, if necessary, correct the pan settings to account for the zoom out.
		// This is done to address the case, for example, where the map is zoomed to 100% and
		// panned all the way to the bottom and all the way to the right to show its lower right
		// corner. Zooming out without adjusting the pan settings would cause the map to shrink
		// toward the upper left corner leaving empty space around the lower right corner.
		// Validation is also required for the same purpose if this method was called to change
		// the map zoom in response to the the tour being resized such that the map zooms out.
		if (delta < 0 || validatePanForResize)
			this.validateAndUpdatePanXandPanY(0, 0);

		if (this.markerIsSelected)
		{
			// Show the marker as selected at the new zoom level.
			this.selectMarker(this.selectedMarkerViewId, null);
		}

		// Position the map so that the mouse or touch location is at the pan center.
		this.positionMapToMakeLocationVisibleOnCanvas({ x: this.panCenterLocation.x_map, y: this.panCenterLocation.y_map });

		this.mapZoomPanStateChanged();

		this.drawMap();

		// Redraw the outline and crosshairs for any markers that are selected in the Map Editor.
		if (this.tour.editMode)
			this.editor.redrawSelectedMarkers();

		this.zoomingMapInOut = false;
	}

	zoomMapOnEachAnimationFrame()
	{
		if (!this.zoomingMap)
			return;

		function round(value, decimals)
		{
			return Number(Math.round(value + 'e' + decimals) + 'e-' + decimals);
		}

		let percent = 0;

		if (this.zoomPercent != 0)
		{
			// User is pressing one of the zoom in/out buttons.
			percent = this.zoomPercent;
		}
		else if (this.currentPinchDistance != 0)
		{
			// User is pinching in or out on the map. Get the pinch distance since the last zoom.
			let distance = this.currentPinchDistance - this.lastPinchDistance;
			this.lastPinchDistance = this.currentPinchDistance;

			// Derive a zoom percentage amount using the pinch distance and what feels natural.
			// The greater the pinch distance the higher the percentage and vice-versa. This
			// way a quick/large pinch zooms the map more than a slow/small pinch.
			let sensitivity = 0.1;
			percent = distance * sensitivity;
			percent = round(percent, 2);

			if (isNaN(percent))
			{
				// This should never happen, but just to play it safe, ignore anomalies that might
				// occur if the pinch detection logic or touch device delivered bad data.
				console.log(`NaN ${distance} ${this.currentPinchDistance} ${this.lastPinchDistance}`);
				percent = 0;
			}

			// Reset the pinch distance so that it won't get reused on the next frame.
			this.currentPinchDistance = 0;
		}

		if (percent < 0 && this.zoomedAllTheWayOut || percent > 0 && this.zoomedAllTheWayIn)
		{
			if (this.zoomedAllTheWayOut)
			{
				this.updatePanXandPanY(0, 0);
				this.zoomMap(this.zoomedOutMapScale, false);
			}
			this.stopZooming();
			return;
		}

		if (percent != 0)
		{
			// Adjust the percentange based on the map scale to create a smoother zooming effect. A 1% change in the
			// zoom level is much more noticable when the map is all way zoomed out than when it is partially zoomed
			// in so use a smaller percentage when zoomed out and a larger percentage when zoomed in.
			percent *= this.currentMapScale * 1.5;
			this.zoomMap(percent);
		}

		// When editing a hybrid marker, redraw it on each zoom.
		if (this.tour.editMode && this.editor.hybridEditingEnabled)
			this.editor.redrawHybrid();

		//console.log(`Map::zoomMapOnEachAnimationFrame ${percent}%`);

		requestAnimationFrame(this.zoomMapOnEachAnimationFrame);
	}
}

class AutoPanCurve
{
	// This class utilizes one of Robert Panner's easing functions (https://easings.net/)
	// to provide smooth auto panning from the current map position to a new map position.

	constructor(map, endX, endY, fastPan = false)
	{
		this.map = map;
		this.endX = endX;
		this.endY = endY;
		this.startX = this.map.panX_map;
		this.startY = this.map.panY_map;

		// Calculate the distance from the starting position to the ending position for each axis.
		this.changeInValueX = this.endX - this.startX;
		this.changeInValueY = this.endY - this.startY;

		if (fastPan || map.disableAutoPan)
		{
			// For fast panning, set the total number of iterations so that the pan should take between 0.1 and 0.2 seconds.
			// When auto panning is disabled, set the total iterations to do so that panning will stop after the first iteration.
			this.totalIterations = map.disableAutoPan ? 2 : 12;
		}
		else
		{
			// Set the total number of iterations so that the pan should take between 1 and 2 seconds depending on
			// the distance to be panned, based on a refresh rate of about 60 times per second in most web browsers.
			let maxDistance = Math.max(Math.abs(this.changeInValueX), Math.abs(this.changeInValueY));
			this.totalIterations = Math.round(0.05 * maxDistance);
			if (this.totalIterations < 60)
				this.totalIterations = 60;
			else if (this.totalIterations > 120)
				this.totalIterations = 120;
		}

		this.currentIteration = 0;
	}

	// The caller should stop calling requestAnimationFrame when this method returns true.
	get completed()
	{
		return this.currentIteration >= this.totalIterations;
	}

	easeInOutCubic(startValue, changeInValue)
	{
		let currentIteration = this.currentIteration;
		if ((currentIteration /= this.totalIterations / 2) < 1)
			return Math.round(changeInValue / 2 * Math.pow(currentIteration, 3) + startValue);
		let newValue = changeInValue / 2 * (Math.pow(currentIteration - 2, 3) + 2) + startValue;
		return Math.round(newValue);
	}

	// This method needs to be called on each auto pan animation frame.
	iterate()
	{
		this.currentIteration += 1;
	}

	get newPanX_map()
	{
		return this.easeInOutCubic(this.startX, this.changeInValueX);
	}

	get newPanY_map()
	{
		return this.easeInOutCubic(this.startY, this.changeInValueY);
	}
}
