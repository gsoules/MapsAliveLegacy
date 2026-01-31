// Copyright (C) 2009-2022 AvantLogic Corporation: https://www.mapsalive.com

export { MapsAliveApi };

import { MapsAliveLiveData as LiveData__$$ } from './mapsalive-livedata.js';
import { MapsAliveMedia as Media__$$ } from './mapsalive-media.js';
import { MapsAliveRuntime as Runtime__$$ } from './mapsalive-runtime.js';

class MapsAliveApi
{
	constructor(tour)
	{
		//console.log(`Api__$$::contructor`);

		this.tour = tour;
		this._media = new Media__$$(this.tour);
		this._liveData = new LiveData__$$(this.tour);
		this.apiFunctionName = "";
		this.reportJavaScriptErrors = false;
		this.waitForLiveData = false;
		this.waitForLiveDataMessage = "";

		// Create an empty object that the user can use to store their own data.
		this._global = {};

		// Provide access to V3 API functions using the V3 naming convention e.g. window.mapsalive.playSound.
		window.mapsalive = this;

		this.onScreenMapLayer = null;
		this.onScreenMarkerLayer = null;
		this.onScreenHitLayer = null;
	}

	//====================================================================================================
	// Public V3 methods
	//====================================================================================================

	changeMarkerNormalShapeAppearance(hotspotIdList, lineColor, lineAlpha, fillColor, fillAlpha, effects)
	{
		this.changeMarkerShapeAppearance(false, hotspotIdList, lineColor, lineAlpha, fillColor, fillAlpha, effects);
	}

	changeMarkerSelectedShapeAppearance(hotspotIdList, lineColor, lineAlpha, fillColor, fillAlpha, effects)
	{
		this.changeMarkerShapeAppearance(true, hotspotIdList, lineColor, lineAlpha, fillColor, fillAlpha, effects);
	}

	flushLiveDataCache()
	{
		this.liveData.flushCache()
	}

	getCurrentHotspot()
	{
		if (this.page.isDataSheet)
			return null;

		let viewId = this.map.selectedMarkerViewId;
		let marker = this.map.getMarker(viewId, false);
		if (marker === null)
			return null;
		let view = this.page.getView(viewId);
		let plainText = view.plainText;
		if (plainText.length === 0)
			plainText = marker.tooltip;

		// Create and return a temporary object. We do this instead of returning the internal hotspot
		// so that we can change the internal representation if necessary without breaking the API.
		let o = new Object();
		o.id = view.hotspotId;
		o.title = view.title;
		o.htmlText = view.htmlText;
		o.plainText = plainText;
		o.embedText = view.embedText ? view.embedText : "";
		o.imageSrc = view.imageSrc ? view.imageSrc : "";
		return o;
	}

	getCurrentPage()
	{
		let o = new Object();
		let page = this.page;
		o.id = page.mapId;
		o.name = page.pageName;
		o.title = page.pageTitle;
		return o;
	}

	mapIsHtml5()
	{
		return true;
	}

	playSound(name, url)
	{
		this.media.playAudio(url);
	}

	restoreMarkerNormalShapeAppearance(hotspotIdList)
	{
		this.restoreMarkerShapeAppearance(false, hotspotIdList);
	}

	restoreMarkerSelectedShapeAppearance(hotspotIdList)
	{
		this.restoreMarkerShapeAppearance(true, hotspotIdList);
	}

	setMarkerAppearanceNormal(hotspotIdList)
	{
		this.showMarkerSelected(false, hotspotIdList);
	}

	setMarkerAppearanceSelected(hotspotIdList)
	{
		this.showMarkerSelected(true, hotspotIdList);
	}

	stopSound()
	{
		this.media.pauseAudio();
	}

	//====================================================================================================
	// Public V4 properties
	//====================================================================================================

	get currentHotspot()
	{
		return this.getCurrentHotspot();
	}

	get currentPage()
	{
		return this.getCurrentPage();
	}

	get global()
	{
		return this._global;
	}

	set global(value)
	{
		this._global = value;
	}

	get hotspots()
	{
		let hotspots = [];

		for (const viewId in this.tour.currentPage.views)
		{
			let view = this.page.getView(viewId);
			let hotspotProperties = this.createHotspotProperties(view);
			hotspots.push(hotspotProperties);
		}

		return hotspots;
	}

	get instance()
	{
		return this.tour.instanceName;
	}

	get isSmallMobileDevice()
	{
		return this.tour.isSmallMobileDevice;
	}

	get liveData()
	{
		return this._liveData;
	}

	get media()
	{
		return this._media;
	}

	get pages()
	{
		let pages = [];
		for (const page of this.tour.pages)
		{
			let pageProperties = this.createPageProperties(page);
			pages.push(pageProperties);
		}
		return pages;
	}

	//====================================================================================================
	// Public V4 methods
	//====================================================================================================

	changeMarkerShapeAppearance(selected, hotspotIdList, lineColor, lineAlpha, fillColor, fillAlpha, effects, draw = true)
	{
		let viewIdList = this.page.createViewIdListFromHostpotIdList(hotspotIdList);
		this.map.changeMarkerShapeAppearance(selected, viewIdList, lineColor, lineAlpha, fillColor, fillAlpha, effects, draw);
	}

	changeMarkerStyleAppearance(selected, styleId, lineColor, lineAlpha, fillColor, fillAlpha, effects, draw = true)
	{
		this.map.changeMarkerStyleAppearance(selected, styleId, lineColor, lineAlpha, fillColor, fillAlpha, effects, draw);
	}

	disableMarkerDrawing(disable)
	{
		this.map.disableMarkerDrawing(disable);
	}

	drawAllMarkers()
	{
		this.map.flushMarkerLayerCache();
		this.map.drawAllMarkers();
	}

	getElementByUniqueId(id)
	{
		return document.getElementById(this.tour.uniqueId(id));
	}

	goToPage(pageSpecifier)
	{
		//console.log(`API: goToPage "${pageSpecifier}" from "${this.tour.currentPage.mapId}"`);

		// Don't attempt to leave the current page while it is still loading.
		if (!this.tour.currentPage.pageLoaded)
			return;

		let pageNumber = this.tour.getPageNumber(pageSpecifier);
		if (pageNumber === 0)
			return;
		this.tour.goToPage(pageNumber);
	}

	reportErrors(report)
	{
		this.reportJavaScriptErrors = report === true;
	}

	restoreMarkerShapeAppearance(selected, hotspotIdList, draw = true)
	{
		let viewIdList = this.page.createViewIdListFromHostpotIdList(hotspotIdList);
		this.map.restoreMarkerShapeAppearance(selected, viewIdList, draw);
	}

	restoreMarkerStyleAppearance(styleId, selected, draw = true)
	{
		this.map.restoreMarkerStyleAppearance(styleId, selected, draw);
	}

	routeIsDefined(routeId)
	{
		if (typeof this.page.routesTable === "undefined")
			return false;
		let routeRecord = this.page.routesTable[routeId];
		if (!routeRecord)
			return false;
		return true;
	}

	setHotspotUsesLiveData(hotspotIdList, uses)
	{
		let viewIdList = this.page.createViewIdListFromHostpotIdList(hotspotIdList);
		this.page.setHotspotUsesLiveData(viewIdList, uses);
	}

	setHotspotHtml(hotspotId, html)
	{
		//console.log(`API::setHotspotHtml ${hotspotId}`);
		let view = this.page.getViewByHotspotId(hotspotId);
		if (view === null)
			return;
		view.htmlText = html;
	}

	setHotspotTitle(hotspotId, title)
	{
		let view = this.page.getViewByHotspotId(hotspotId);
		if (view === null)
			return;

		let titleChanged = view.title !== title;
		view.title = title;

		// When a hotspot's title changes, it's entry in the nav panel is no longer valid.
		// Tell the Layout object to remove and create a new panel before showing it again.
		if (titleChanged)
			this.page.layout.invalidateNavPanel();
	}

	setMapPan(deltaX_map, deltaY_map)
	{
		this.apiFunctionName = "setMapPan";
		if (this.apiMissingArg(deltaX_map, "deltaX_map"))
			return;
		if (this.apiMissingArg(deltaY_map, "deltaY_map"))
			return;

		// Interpret positive values to mean down or to the right and negative values as up or left.
		this.map.panMap(-deltaX_map, -deltaY_map);
	}

	setTourSetting(name, value)
	{
		//console.log(`API::setTourSetting "${name}" "${value}"`);
		this.tour.setTourSetting(name, value);
	}

	showHelpPanel(pageSpecifier)
	{
		this.page.layout.showHelpPanelForPage(pageSpecifier);
	}

	showMarkerSelected(selected, hotspotIdList)
	{
		let viewIdList = this.page.createViewIdListFromHostpotIdList(hotspotIdList);
		this.map.setMarkerAppearanceAsNormalOrSelected(viewIdList, selected);
	}

	showNavPanel()
	{
		this.page.layout.onClickNavButton();
	}

	//====================================================================================================
	// Public V3 and V4 methods
	//====================================================================================================

	closePopup()
	{
		this.map.deselectMarkerAndClosePopup();
	}

	createOffScreenMap(width, height)
	{
		// Save the current map and marker layers so they can be restored when the off-screen canvas is destroyed.
		this.onScreenMapLayer = this.map.mapLayer;
		this.onScreenMarkerLayer = this.map.markerLayer;
		this.onScreenHitLayer = this.map.hitLayer;

		// Create new map, marker, and hit layer canvas elements. The hit layer is needed for blending.
		let offScreenMapLayer = document.createElement("canvas");
		offScreenMapLayer.width = width;
		offScreenMapLayer.height = height;

		let offScreenMarkerLayer = document.createElement("canvas");
		offScreenMarkerLayer.width = width;
		offScreenMarkerLayer.height = height;

		let offScreenHitLayer = document.createElement("canvas");
		offScreenHitLayer.width = width;
		offScreenHitLayer.height = height;

		// Fool the runtime into thinking that the offscreen canvas layers are the actual map and marker layers.
		this.assignCanvasLayers(offScreenMapLayer, offScreenMarkerLayer, offScreenHitLayer);
	}

	destroyOffScreenMap()
	{
		// Restore the actual map and marker layers that were saved by createOffScreenMap;
		this.assignCanvasLayers(this.onScreenMapLayer, this.onScreenMarkerLayer, this.onScreenHitLayer);

		this.onScreenMapLayer = null;
		this.onScreenMarkerLayer = null;
		this.onScreenHitLayer = null;
	}

	drawRoute(hotspotId, routeId, lineWidth, lineColor, lineAlpha, effects)
	{
		this.apiFunctionName = "drawRoute";

		if (this.apiTourNotLoaded())
			return;

		// Verify that required parameters were passed.
		if (this.apiMissingArg(hotspotId, "hotspotId"))
			return;

		if (this.apiMissingArg(routeId, "route"))
			return;

		if (hotspotId.length > 0)
		{
			let view = this.page.getViewByHotspotId(hotspotId);
			if (view === null)
			{
				this.apiError(`"${hotspotId}" is not a hotspot belonging to this map.`);
				return;
			}

			let marker = this.map.getMarker(view.viewId, false);
			if (!marker.isRoute)
			{
				this.apiError(`Hotspot "${hotspotId}" is not a route hotspot."`);
				return;
			}
		}
		else
		{
			this.apiError(`The hotspotId parmeter is empty.`);
			return;
		}

		// Determine if the 2nd arg is a route Id or a comma-separated list of hotspot Ids.
		let routeIsList = routeId.indexOf(',') >= 0;

		if (!routeIsList)
		{
			// Verify that the route Id is valid.
			if (typeof this.page.routesTable === "undefined")
			{
				this.apiError("This map does not have routes.");
				return;
			}
			let routeRecord = this.page.routesTable[routeId];
			if (!routeRecord)
			{
				this.apiError("This map does not have route '" + routeId + "'.");
				return;
			}
		}

		// Draw the route.
		this.map.drawRoute(hotspotId, routeId, lineWidth, lineColor, lineAlpha, effects);
	}

	drawRoutes(routes)
	{
		this.map.setRoutes(routes);
		this.map.initializeDrawState();
		this.map.drawMap();
	}

	getHotspotIdsForCategory(codes, and = false)
	{
		//console.log("getHotspotIdsForCategory " + codes + " : " + and);

		let codeList = codes.split(",");

		// Determine if we should get all categories. When there is nothing to AND, we get everything
		// by acting as though we are ORing all the categories. It works like this because the categories
		// in the code list act as filters when ANDing. If there are no filters, you get everything.
		// In the case of OR, you start with nothing and the code list controls what gets included.
		let getAll = and && Runtime__$$.trim(codes).length === 0;

		let hotspotIds = [];
		let categoryTable = this.page.categoryTable;

		// Loop over each category in the code list.
		for (let codeListIndex = 0; codeListIndex < codeList.length; codeListIndex++)
		{
			let listCode = Runtime__$$.trim(codeList[codeListIndex]);

			// Loop over each category in the category table.
			for (let categoryTableIndex = 0; categoryTableIndex < categoryTable.length; categoryTableIndex++)
			{
				// Get the next category row in the category table.
				let row = categoryTable[categoryTableIndex];
				let tableRowCode = row[0];

				if (!getAll && tableRowCode !== listCode.toLowerCase())
				{
					// This row's category is not in the code list. Ignore it unless getting all categories.
					continue;
				}

				if (and && !getAll)
				{
					// Create an array of all the hotspot Ids in the current category table row.
					let rowHotspotIds = row.slice(1);

					if (codeListIndex === 0)
					{
						// This is the first category in the code list. Add all of its ids to the result.
						hotspotIds = rowHotspotIds;
					}
					else
					{
						// AND the current result with this row to get a new result.
						hotspotIds = this.andArray(hotspotIds, rowHotspotIds);
					}
					continue;
				}
				else
				{
					// Loop over each hotspot Id in the current row. We start at 1 because 0 is the row's code.
					let hotspotId;
					for (let rowIndex = 1; rowIndex < row.length; rowIndex++)
					{
						hotspotId = row[rowIndex];
						if (!hotspotIds.includes(hotspotId))
						{
							// The id does not yet exist in the result set. Add it to the set.
							hotspotIds.push(hotspotId);
						}
					}
				}
			}
		}

		if (hotspotIds.length === 0)
			return "";
		else if (hotspotIds.length === 1)
			return hotspotIds[0];
		else
			return hotspotIds.join();
	}

	getQueryStringArg(arg)
	{
		return Runtime__$$.getQueryStringArg(arg);
	}

	mapIsTouchDevice()
	{
		return this.tour.isTouchDevice;
	}

	mapToDataUrl()
	{
		let canvas = document.createElement("canvas");
		canvas.width = this.map.canvasW;
		canvas.height = this.map.canvasH;
		let ctx = this.map.getCanvasContext(canvas);
		ctx.drawImage(this.map.mapLayer, 0, 0);
		ctx.drawImage(this.map.markerLayer, 0, 0);
		return canvas.toDataURL();
	}

	positionMapToShowMarker(hotspotId)
	{
		//console.log("Api::positionMapToShowMarker " + hotspotId);

		this.apiFunctionName = "positionMapToShowMarker";

		if (this.apiTourNotLoaded())
			return;

		if (this.apiMissingArg(hotspotId, "hotspotId"))
			return;

		let view = this.page.getViewByHotspotId(hotspotId);
		if (view === null)
		{
			this.apiError(hotspotId + " is not a hotspot on this map.");
			return;
		}

		// Treat the request as though it came from the directory.
		this.map.selectMarkerChosenFromDirectory(view.viewId);
	}

	resizeTour()
	{
		this.tour.onResizeTour(1);
	}

	runSlideShow(interval)
	{
		this.apiFunctionName = "runSlideShow";
		if (this.apiMissingArg(interval, "interval"))
			return;

		if (interval <= 0)
		{
			this.map.stopSlideShow();
		}
		else
		{
			this.page.slideShowInterval = interval;
			this.map.startSlideShow();
		}
	}

	setMapZoomInOut(delta)
	{
		this.apiFunctionName = "setMapZoomInOut";
		if (this.apiMissingArg(delta, "delta"))
			return;
		if (this.map.zoomMap(delta))
			this.closePopup();
	}

	setMapZoomLevel(level)
	{
		this.apiFunctionName = "setMapZoomLevel";
		if (this.apiMissingArg(level, "level"))
			return;

		this.map.initializeDrawState();

		let percent = level;
		let zoomPercent = Math.max(level, percent);
		this.map.setMapZoomLevel(zoomPercent);
	}

	setMarkerBlink(hotspotIdList, blinkCount)
	{
		let viewIdList = this.page.createViewIdListFromHostpotIdList(hotspotIdList);
		this.map.setMarkerListBlink(viewIdList, blinkCount);
	}

	setMarkerDisabled(hotspotIdList, isDisabled)
	{
		let viewIdList = this.page.createViewIdListFromHostpotIdList(hotspotIdList);
		this.map.setMarkerListDisabled(viewIdList, isDisabled);
	}

	setMarkerHidden(hotspotIdList, isHidden)
	{
		let viewIdList = this.page.createViewIdListFromHostpotIdList(hotspotIdList);
		this.map.setMarkerListHidden(viewIdList, isHidden);
	}

	setMarkerOnTop(hotspotId)
	{
		let view = this.page.getViewByHotspotId(hotspotId);
		if (view)
			this.map.setMarkerOnTop(view.viewId);
	}

	setMarkerSelected(hotspotId)
	{
		//console.log("Api::setMarkerSelected %s", hotspotId);

		this.apiFunctionName = "setMarkerSelected";

		if (this.apiTourNotLoaded())
			return;

		if (this.apiMissingArg(hotspotId, "hotspotId"))
			return;

		let view = this.page.getViewByHotspotId(hotspotId);
		if (view === null)
		{
			this.apiError(hotspotId + " is not a hotspot on this map.");
			return;
		}

		// Show the marker's view as though the hotspot had been selected from the directory.
		this.map.selectMarkerAndShowPopup({ viewId: view.viewId, pin: true });
	}

	setMarkerStatic(hotspotIdList, isStatic)
	{
		let viewIdList = this.page.createViewIdListFromHostpotIdList(hotspotIdList);
		this.map.setMarkerListStatic(viewIdList, isStatic);
	}

	setTourTitle(text)
	{
		let e = document.getElementById(this.tour.uniqueId('TitleText'));
		if (e)
			e.innerHTML = text;
	}

	zoomMapToShowMarker(hotspotId, disableAutoPan, zoomLimit, padding)
	{
		console.log("Api::zoomMapToShowMarker " + hotspotId);

		this.apiFunctionName = "zoomMapToShowMarker";

		if (this.apiTourNotLoaded())
			return;

		if (this.apiMissingArg(hotspotId, "hotspotId"))
			return;

		let view = this.page.getViewByHotspotId(hotspotId);
		if (view === null)
		{
			this.apiError(hotspotId + " is not a hotspot on this map.");
			return;
		}

		this.map.disableAutoPan = disableAutoPan === true;

		padding = parseInt(padding, 10);
		if (isNaN(padding))
			padding = 1;

		zoomLimit = parseInt(zoomLimit);
		if (isNaN(zoomLimit) || zoomLimit < 100)
			zoomLimit = 100;
		this.map.zoomLimit = zoomLimit;

		// Determine how much to zoom the map so that the marker fills the visible map area.
		let marker = this.map.getMarker(view.viewId);
		let scaleW = marker.shapeW / this.map.canvasW;
		let scaleH = marker.shapeH / this.map.canvasH;
		let scale = Math.max(scaleW, scaleH);
		let percent = (1 / scale) * 100;

		// Reduce the percentage to provide some space around the marker so it's not touching the edges
		// of the map. Increase the padding value so that it's a little larger when the map is zoomed in
		// than when it's zoomed out.
		padding = padding + (padding * (percent / 100));
		percent -= padding;

		// Zoom the map to the percentage and then pan so that the marker is in the center.
		this.map.initializeDrawState();
		this.map.setMapZoomLevel(percent);
		this.positionMapToShowMarker(hotspotId);
	}

	waitingForLiveData(waiting, message = "")
	{
		this.waitForLiveData = waiting;
		this.waitForLiveDataMessage = waiting ? message : "";
	}

	//====================================================================================================
	// Private methods
	//====================================================================================================

	andArray(array1, array2)
	{
		let result = new Array();

		for (let i in array1)
		{
			let item1 = array1[i];
			for (let j in array2)
			{
				let item2 = array2[j];
				if (item1.toLowerCase() === item2.toLowerCase())
					result.push(item1);
			}
		}

		return result;
	}

	apiError(message)
	{
		if (this.tour.preview)
		{
			alert("MapsAlive API Error\n\n" + this.apiFunctionName + "\n\n" + message);
		}
	}

	apiMissingArg(arg, name)
	{
		if (typeof arg === "undefined")
		{
			this.apiError("A required parameter is missing: " + name);
			return true;
		}
		return false;
	}

	apiTourNotLoaded()
	{
		if (this.tour)
			return false;

		this.apiError("An API call was made before the tour finished loading. To avoid this error, don't call map functions before maOnTourLoaded has been called.");
		return true;
	}

	assignCanvasLayers(mapLayer, markerLayer, hitLayer)
	{
		this.map.mapLayer = mapLayer;
		this.map.mapLayerContext = this.map.getCanvasContext(mapLayer);
		this.map.graphics.ctx.mapLayer = this.map.mapLayerContext;

		this.map.markerLayer = markerLayer;
		this.map.markerLayerContext = this.map.getCanvasContext(markerLayer);
		this.map.graphics.ctx.markerLayer = this.map.markerLayerContext;

		this.map.hitLayer = hitLayer;
		this.map.hitLayerContext = this.map.getCanvasContext(hitLayer);
		this.map.graphics.ctx.hitLayer = this.map.hitLayerContext;

		this.map.setCanvasSize({ w: mapLayer.width, h: mapLayer.height });
		this.map.mapZoomPanStateChanged();

		this.map.initializeDrawState();
		this.map.flushMarkerLayerCache();
	}

	callback(eventName, arg)
	{
		//console.log(`API::callback ${eventName}`);

		let fcnV4 = this.tour.runtime.getCallbackFunction('V4', this.tour, eventName);
		if (fcnV4)
		{
			this.callbackToV4Function(eventName, fcnV4, arg);
			return true;
		}

		let fcnV3 = this.tour.runtime.getCallbackFunction('V3', this.tour, eventName);
		if (fcnV3)
		{
			this.callbackToV3Function(eventName, fcnV3, arg);
			return true;
		}

		return false;
	}

	//
	// When adding a new callback function, also add it to Runtime::createUniqueCallbackFunctionNames.
	//

	callbackDirectoryEntryClicked(view)
	{
		let properties = this.createHotspotProperties(view);
		this.callback('DirectoryEntryClicked', properties);
	}

	callbackHotspotChanged(view)
	{
		let properties = this.createHotspotProperties(view);
		this.callback('HotspotChanged', properties);
	}

	callbackLiveDataResponse(request, id, data, xml, count, total)
	{
		let error = count === 0;
		let properties = { "requestId" : request.id, "responseType" : request.responseType, id, data, xml, count, total, error };
		this.callback('LiveDataResponse', properties);
	}

	callbackPageLoaded()
	{
		//console.log(`API::callbackPageLoaded`);

		let called = this.callback('PageLoaded');
		if (called)
			return;

		// Call the V4 callback.
		this.callback('MapLoaded');
	}

	callbackPageLoading()
	{
		//console.log(`API::callbackPageLoading`);
		this.callback('PageLoading');
	}

	callbackPopupClosed(view)
	{
		let properties = this.createHotspotProperties(view);
		this.callback('PopupClosed', properties);
	}

	callbackTourLoaded()
	{
		//console.log(`API::callbackTourLoaded`);
		this.callback('TourLoaded');
	}

	callbackToV3Function(eventName, fcn, properties)
	{
		switch (eventName)
		{
			case 'DirectoryEntryClick':
				fcn(properties.id);
				break;

			case 'HotspotChanged':
				fcn(properties.id);
				break;

			case 'LiveDataResponse':
				// In V3 a dataId value of 0 means error, but only if the request type was XML.
				let id = properties.responseType === "xml" ? properties.id : "";
				fcn(properties.requestId, id, properties.data, properties.xml);
				break;

			case 'MapLoaded':
				fcn();
				break;

			case 'PopupClosed':
				fcn(properties.id);
				break;
		}
	}

	callbackToV4Function(eventName, fcn, properties)
	{
		let event = this.createEvent();

		switch (eventName)
		{
			case 'DirectoryEntryClicked':
				event.hotspot = properties;
				break;

			case 'HotspotChanged':
				event.hotspot = properties;
				break;

			case "LiveDataResponse":
				event.response = properties;
				break;

			case 'PageLoading':
			case 'PageLoaded':
				break;

			case 'PopupClosed':
				event.hotspot = properties;
				break;

			case 'TourLoaded':
				break;

			default:
				Runtime__$$.assert(false, `API:: Unsupported V4 callback`);
				return;
		}

		fcn(event);
	}

	createEvent()
	{
		let event = {
			api: this,
			tourNumber: this.tour.tourId,
			tourName: this.tour.name,
			instance: this.tour.instanceName,
			instanceId: this.tour.instanceId
		};

		let page = this.tour.currentPage;
		event.page = page ?  this.createPageProperties(page) : null;

		return event;
	}

	createHotspotProperties(view)
	{
		return { id: view === null ? 0 : view.hotspotId, title: view === null ? "" : view.title };
	}

	createPageProperties(page)
	{
		return {
			id: page.mapId,
			name: page.pageName,
			number: page.pageNumber,
			title: page.pageTitle
		};
	}

	drawTestRoute(hotspotId, routeId)
	{
		this.drawRoute(hotspotId, routeId, 1, 0x0000ff, 100, "glow,0xffffff,100,5,5");
	}

	get map()
	{
		return this.tour.currentPage.map;
	}

	get page()
	{
		return this.tour.currentPage;
	}
}