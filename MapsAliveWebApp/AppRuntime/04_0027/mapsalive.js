// Copyright (C) 2009-2022 AvantLogic Corporation: https://www.mapsalive.com

function maGetQueryStringArg(arg) { 
	var pairs = document.location.search.substring(1).split("&");
	for (i = 0; i < pairs.length; i++)
	{
		var pair = pairs[i];
		var eq = pair.indexOf('=');
		if (pair.substring(0, eq).toLowerCase() == arg.toLowerCase())
			return pair.substring(eq + 1);
	}
	return "";
}


//--// OBFUSCATE

if (typeof (console) === 'undefined')
{
	console = function() {};
	console.log = function(s) {};
}

window.onerror = function(errorMessage, fileName, lineNumber)
{
	if (maClient.preview)
	{
		// While in Tour Preview, report JavaScript errors. Return true to keep the error from bubbling up to the browser.
		alert("MapsAlive detected a JavaScript error:\n\n" + errorMessage + "\n\nVerify that your Custom HTML JavaScript is correct.\n\n" + fileName + " : Line " + lineNumber);
		return true;
    }
    else
    {
		// In published tours, return false to let errors bubble up so the browser can report them.
		return false;
    }
};

function maUaIs(ua) 
{
	var answer = navigator.userAgent.toLowerCase().indexOf(ua);
	console.log(ua + " = " + answer);
	return answer != -1;
}

var mapsalive = new Object();
var maClient = new Object();

maClient.runtimeVersion = 116;

// These value must be kept in sync with TourPageXmlWriter.WritePopupAttributes.
maClient.popupLocationMarkerCenter = 1;
maClient.popupLocationMarkerEdge = 2;
maClient.popupLocationMouse = 3;
maClient.popupLocationFixedPopup = 4;
maClient.popupLocationFixedAlwaysVisible = 5;

maClient.opera = navigator.userAgent.search(/Opera/) != -1;
maClient.ie = navigator.appVersion.indexOf("MSIE") != -1 && !maClient.opera ? 1 : 0;
maClient.ie6 = maClient.ie && (navigator.appVersion.indexOf("IE 6.") != -1);
maClient.ie9 = maClient.ie && (navigator.appVersion.indexOf("IE 9.") != -1);
maClient.ie10 = maClient.ie && (navigator.appVersion.indexOf("IE 10.") != -1);
maClient.macintosh = maUaIs("macintosh");
maClient.firefox = maUaIs("firefox");
maClient.android = maUaIs("android");
maClient.chrome = maUaIs("chrome");
maClient.mobile = maUaIs("mobile") || maClient.android;
maClient.macFirefox = maClient.macintosh && maClient.firefox;
maClient.safariLt3 = false;
maClient.iOS = maUaIs("ipad") || maUaIs("iphone") || maUaIs("ipod");
maClient.windows = maUaIs("windows nt");
maClient.windowsTablet = maClient.windows && maUaIs("touch");
maClient.viewId = 0;
maClient.tid = 1;
maClient.path = "";
maClient.preview = false;
maClient.page = false;
maClient.unbranded = false;
maClient.pinNextPopup = false;
maClient.dirPreviewSlide = null;
maClient.liveDataErrorSettings = null;
maClient.map = null;
maClient.mapLoaded = false;
maClient.ready = false;
maClient.waitForClientReadyCount = 0;
maClient.restoredViewId = 0;
maClient.mouseX = -1;
maClient.mouseY = -1;
maClient.mouseIsOverPopup = false;
maClient.mouseDragStarted = false;
maClient.enableRefresh = false;
maClient.clickedCloseX = false;
maClient.slideShowIsRunning = false;
maClient.soundManagerReady = false;
maClient.hotspotOnQueryString = false;
maClient.mapIsHtml5 = false;
maClient.mapIsFlash = false;
maClient.mapIsEditor = false;

mapsalive.Api = "";


//-------------------------//
// MapsAlive API Functions //
//-------------------------//

mapsalive.ApiError = function(msg)
{
	if (maClient.preview)
	{
		alert("MapsAlive API Error\n\n" + mapsalive.Api + "\n\n" + msg);
	}
};

mapsalive.ApiMissingArg = function(arg, name)
{
	if (typeof arg == "undefined")
	{
		mapsalive.ApiError("A required parameter is missing: " + name);
		return true;
	}
	return false;
};

mapsalive.ApiSoundManagerMissing = function()
{
	if (typeof soundManager == "undefined")
	{
		mapsalive.ApiError("SoundManager has not been included with this tour.\nFor help, see the MapsAlive User Guide for the JavaScript API.");
		return true;
	}
	else if (!maClient.soundManagerReady)
	{
		console.log("SoundManager called before it is ready");
		return true;
	}
	return false;
};

mapsalive.ApiMapNotLoaded = function()
{
	if (!maClient.mapLoaded)
	{
		mapsalive.ApiError("An API call to the map was made before the map finished loading. To avoid this error, don't call map functions before maOnMapLoaded has been called.");
		return true;
	}
	return false;
};

mapsalive.map = function()
{
	return maClient.map;
};

mapsalive.mapIsHtml5 = function()
{
	return maClient.mapIsHtml5;
};

mapsalive.mapIsTouchDevice = function()
{
	return maClient.isTouchDevice;
};

mapsalive.getQueryStringArg = function(arg)
{
	return maGetQueryStringArg(arg);
};

mapsalive.changeMarkerNormalShapeAppearance = function(slideList, lineColor, lineAlpha, fillColor, fillAlpha, effects)
{
	var viewIdList = maClient.createViewListFromSlideList(slideList);
	effects = maClient.convertEffects(effects);
	maClient.map.changeMarkerShapeAppearance(viewIdList, false, lineColor, lineAlpha, fillColor, fillAlpha, effects);
};

mapsalive.changeMarkerSelectedShapeAppearance = function(slideList, lineColor, lineAlpha, fillColor, fillAlpha, effects)
{
	var viewIdList = maClient.createViewListFromSlideList(slideList);
	effects = maClient.convertEffects(effects);
	maClient.map.changeMarkerShapeAppearance(viewIdList, true, lineColor, lineAlpha, fillColor, fillAlpha, effects);
};

mapsalive.closePopup = function()
{
	maClosePopup();
};

mapsalive.drawRoute = function(hotspotId, route, lineWidth, lineColor, lineAlpha, effects)
{
	mapsalive.Api = "drawRoute";
		
	if (mapsalive.ApiMapNotLoaded())
		return;
	
	// Verify that required parameters were passed.
	if (mapsalive.ApiMissingArg(hotspotId, "hotspotId"))
		return;
	if (mapsalive.ApiMissingArg(route, "route"))
		return;
	var hotspot = maClient.getSlideBySlideId(hotspotId);
	if (!hotspot)
	{
		mapsalive.ApiError(hotspotId + " is not a hotspot on this map.");
		return;
	}
	
	// Determine if the 2nd arg is a route Id or a comma-separated list of hotspot Ids.
	var routeIsList = route.indexOf(',') >= 0;
	
	// Provide default values for missing parameters.
	if (typeof lineWidth == "undefined")
		lineWidth = 3;
	if (typeof lineColor == "undefined")
		lineColor = 0xcc0000;
	if (typeof lineAlpha == "undefined")
		lineAlpha = 80;
	if (typeof effects == "undefined")
		effects = "shadow";
	effects = maClient.convertEffects(effects);

	var viewIdList;
	
	if (routeIsList)
	{
		// Construct the route from the passed-in list of hotspot Ids.
		viewIdList = maClient.createViewListFromRouteSlideList(route);
	}
	else
	{
		// Verify that the route Id is valid.
		if (typeof routesTable == "undefined")
		{
			mapsalive.ApiError("This map does not have routes.");
			return;
		}
		var routeRecord = routesTable[route];
		if (!routeRecord)
		{
			mapsalive.ApiError("This map does not have route '" + route + "'.");
			return;
		}
		
		// Get the route's view Id list from the table.
		viewIdList = routeRecord.route;
	}
	
	// Draw the route through the list of hotspots.
	var ok = maClient.map.drawRouteThroughMarkers(hotspot.viewId, lineWidth, lineColor, lineAlpha, viewIdList, effects);
	
	// The Flash drawing functions return false if hotspotId is not a marker or the marker is not a route.
	if (!ok)
		mapsalive.ApiError("Hotspot " + hotspotId + " is not a route.");
};

mapsalive.drawTestRoute = function(hotspotId, routeId)
{
	mapsalive.drawRoute(hotspotId, routeId, 1, 0x0000ff, 100, "glow,0xffffff,100,5,5");
};

mapsalive.flushLiveDataCache = function()
{
	for (var viewId in maClient.slides)
	{
		var slide = maClient.getSlide(viewId);
		slide.liveDataUpdateTime = 0;
	}
};

mapsalive.getCurrentHotspot = function()
{
	var hotspot = maClient.getSlide(maTour.marker.viewId);
	if (!hotspot)
		return null;
	
	// Create and return a temporary object. We do this instead of returning out internal hotspot
	// so that we can change the internal representation if necessary without breaking the API.
	var o = new Object();
	o.id = hotspot.slideId;
	o.title = hotspot.title;
	o.htmlText = hotspot.htmlText;
	o.plainText = hotspot.plainText;
	o.embedText = hotspot.embedText ? hotspot.embedText : "";
	o.imageSrc = hotspot.imageSrc ? hotspot.imageSrc : "";
	return o;
};

mapsalive.getCurrentPage = function()
{
	var o = new Object();
	o.id = maTour.mapId;
	return o;
};

mapsalive.getHotspotIdsForCategory = function(codes, and)
{
	console.log("getHotspotIdsForCategory " + codes + " : " + and);
	
	var codeList = codes.split(",");
	
	// Determine if we should get all categories. When there is nothing to AND, we get everything
	// by acting as though we are ORing all the categories. It works like this because the categories
	// in the code list act as filters when ANDing. If there are no filters, you get everything.
	// In the case of OR, you start with nothing and the code list controls what gets included.
	var getAll = and && maTrim(codes).length === 0;
	
	var result = new Array();
	
	// Loop over each category in the code list.
	for (var codeListIndex = 0; codeListIndex < codeList.length; codeListIndex++)
	{
		var listCode = maTrim(codeList[codeListIndex]);
		
		// Loop over each category in the category table.
		for (var categoryTableIndex = 0; categoryTableIndex < maTour.categoryTable.length; categoryTableIndex++)
		{
			// Get the next category row in the category table.
			var row = maTour.categoryTable[categoryTableIndex];
			var tableRowCode = row[0];
			
			if (!getAll && tableRowCode != listCode.toLowerCase())
			{
				// This row's category is not in the code list. Ignore it unless getting all categories.
				continue;
			}
				
			if (and && !getAll)
			{
				// Create an array of all the hotspot Ids in the current category table row.
				var rowHotspotIds = row.slice(1);
				
				if (codeListIndex === 0)
				{
					// This is the first category in the code list. Add all of its ids to the result.
					result = rowHotspotIds;
				}
				else
				{
					// AND the current result with this row to get a new result.
					result = maClient.andArray(result, rowHotspotIds);
				}
				continue;
			}
			else
			{
				// Loop over each hotspot Id in the current row. We start at 1 because 0 is the row's code.
				var hotspotId;
				for (var rowIndex = 1; rowIndex < row.length; rowIndex++)
				{
					hotspotId = row[rowIndex];
					if (!maClient.arrayContains(result, hotspotId))
					{
						// The id does not yet exist in the result set. Add it to the set.
						result.push(hotspotId);
					}
				}
			}
		}
	} 
	
	if (result.length === 0)
		return "";
	else if (result.length == 1)
		return result[0];
	else
		return result.join();
};

maClient.andArray = function(array1, array2)
{
	var result = new Array();
	
	for (var i in array1)
	{
		var item1 = array1[i];
		for (var j in array2)
		{
			var item2 = array2[j];
			if (item1.toLowerCase() == item2.toLowerCase())
				result.push(item1);
		}
	}
	
	return result;
};

maClient.arrayContains = function(array, item)
{
	for (var i in array)
	{
		if (array[i] == item)
			return true;
	}
	return false;
};

mapsalive.hideDirectory = function()
{
	if (maTour.hasDirectory)
		maClient.dirShow(false);
};

mapsalive.playSound = function(name, url)
{
	mapsalive.Api = "playSound";
	
	if (mapsalive.ApiSoundManagerMissing())
		return;
	
	// Verify that required parameters were passed.
	if (mapsalive.ApiMissingArg(name, "name"))
		return;
	if (mapsalive.ApiMissingArg(url, "url"))
		return;
		
	var sound = soundManager.getSoundById(name);
	var toggle = sound && sound.playState == 1;
	
	if (toggle)
	{
		soundManager.togglePause(name);
		return;
	}
	
	// Stop playing any sound that is currently playing.
	soundManager.stopAll();
			
	if (!sound)
	{
		if (url.length === 0)
			return;
		
		sound = soundManager.createSound(name, url);
	}

	if (sound)
	{
		sound.play();
	}
	else if (maClient.preview)
	{
		var error = "MapsAlive could not play sound '" + name + "'.";
		if (url)
			error += "\n\n" + url;
		alert(error);
	}
};

mapsalive.positionMapToShowMarker = function(hotspotId)
{
	console.log("positionMapToShowMarker " + hotspotId);
	
	mapsalive.Api = "positionMapToShowMarker";
		
	if (mapsalive.ApiMapNotLoaded())
		return;
	
	if (mapsalive.ApiMissingArg(hotspotId, "hotspotId"))
		return;
	
	var viewId = maClient.getViewIdBySlideId(hotspotId);
	if (!viewId)
	{
		mapsalive.ApiError(hotspotId + " is not a hotspot on this map.");
		return;
	}

	if (maClient.map.positionMapToShowMarker(viewId))
		maClosePopup();	
};

mapsalive.restoreMarkerNormalShapeAppearance = function(slideList)
{
	var viewIdList = maClient.createViewListFromSlideList(slideList);
	maClient.map.restoreMarkerShapeAppearance(viewIdList, false);
};

mapsalive.restoreMarkerSelectedShapeAppearance = function(slideList)
{
	var viewIdList = maClient.createViewListFromSlideList(slideList);
	maClient.map.restoreMarkerShapeAppearance(viewIdList, true);
};

mapsalive.setMapZoomInOut = function(delta)
{
	mapsalive.Api = "setMapZoomInOut";
	if (mapsalive.ApiMissingArg(delta, "delta"))
		return;
	if (maClient.map.setMapZoomInOut(delta))
		maClosePopup();
};

mapsalive.setMapZoomLevel = function(level)
{
	mapsalive.Api = "setMapZoomLevel";
	if (mapsalive.ApiMissingArg(level, "level"))
		return;
	if (maClient.map.setMapZoomLevel(level))
		maClosePopup();
};

mapsalive.setMapPan = function(deltaX, deltaY)
{
	mapsalive.Api = "setMapPan";
	if (mapsalive.ApiMissingArg(deltaX, "deltaX"))
		return;
	if (mapsalive.ApiMissingArg(deltaY, "deltaY"))
		return;

	if (maClient.mapIsFlash)
		alert("The setMapPan API is only supported for HTML5.\n\nThis map is rendered with Flash.");
	else
		maClient.map.setMapPan(deltaX, deltaY);
};

mapsalive.setMarkerAppearanceNormal = function(slideList)
{
	var viewIdList = maClient.createViewListFromSlideList(slideList);
	maClient.map.setMarkerAppearanceNormal(viewIdList);
};

mapsalive.setMarkerAppearanceSelected = function(slideList)
{
	var viewIdList = maClient.createViewListFromSlideList(slideList);
	maClient.map.setMarkerAppearanceSelected(viewIdList);
};

mapsalive.setMarkerBlink = function(slideList, blinkCount)
{
	var viewIdList = maClient.createViewListFromSlideList(slideList);
	maClient.map.setMarkerListBlink(viewIdList, blinkCount);
};

mapsalive.setMarkerDisabled = function(slideList, isDisabled)
{
	var viewIdList = maClient.createViewListFromSlideList(slideList);
	maClient.map.setMarkerListDisabled(viewIdList, isDisabled);
};

mapsalive.setMarkerHidden = function(slideList, isHidden)
{
	var viewIdList = maClient.createViewListFromSlideList(slideList);
	maClient.map.setMarkerListHidden(viewIdList, isHidden);
};

mapsalive.setMarkerOnTop = function(slideId)
{
	var slide = maClient.getSlideBySlideId(slideId);
	if (slide)
	{
		maClient.map.setMarkerOnTop(slide.viewId);
	}
};

mapsalive.setMarkerSelected = function(hotspotId)
{
	console.log("setMarkerSelected %s", hotspotId);
	
	mapsalive.Api = "setMarkerSelected";
		
	if (mapsalive.ApiMapNotLoaded())
		return;
	
	if (mapsalive.ApiMissingArg(hotspotId, "hotspotId"))
		return;
	
	var viewId = maClient.getViewIdBySlideId(hotspotId);
	if (!viewId)
	{
		mapsalive.ApiError(hotspotId + " is not a hotspot on this map.");
		return;
	}
	
	// Call showSlide as though the the hotspot had been selected from the directory.
	// If using popups, the true parameter will cause the popup to be pinned, and to
	// get positioned near the marker even if popups normally appear at the mouse location.
	maClient.showSlide(viewId, true);
};

mapsalive.setMarkerStatic = function(slideList, isStatic)
{
	var viewIdList = maClient.createViewListFromSlideList(slideList);
	maClient.map.setMarkerListStatic(viewIdList, isStatic);
};

mapsalive.setTourTitle = function(text)
{
	var e = document.getElementById("maPageTitleText");
	if (e)
		e.innerHTML = text;
};

mapsalive.stopSound = function()
{
	mapsalive.Api = "stopSound";
	
	if (mapsalive.ApiSoundManagerMissing())
		return;
	
	soundManager.stopAll();
};

//-----------------------------------//
// Functions called from Flash       //
//-----------------------------------//

maClient.flashConsoleLog = function(message)
{
	// This function is used for debugging. It gives us a way to trace flash via the console.
	console.log("FLASH: %s", message);
};

maClient.flashEditHotspotContent = function(viewId)
{
	console.log("flashEditHotspotContent");
	maEditSlide(viewId);
};

maClient.flashExecuteJavaScript = function(script)
{
	// This function is called to execute the JavaScript for mouse actions (click, over, out).
	console.log("flashExecuteJavaScript " + script);

	try
	{
		maClient.hideTooltip();
		eval(script);
	}
	catch (error)
	{
		if (maClient.preview)
		{
			alert('MapsAlive detected an error in the following\n' +
				'JavaScript that was provided for this hotspot:\n\n' +
				script + '\n\n' + error.message);
		}
	}
};

maClient.flashGalleryMarkerSelected = function(viewId)
{
	console.log("flashGalleryMarkerSelected %s", viewId );
	maGalleryMarkerSelected(viewId);
};

maClient.flashGoToPage = function(url)
{
	console.log("flashGoToPage %s", url);

	if (url === null)
		return;

	// Correct bug where uppercase P is used in Page.htm. Need to use lower case p everywhere for Apache servers.
	url = url.toLowerCase();
	
	maClient.goToPage(url, null, null);
};

maClient.flashHideMarkers = function(idList)
{
	console.log("flashHideMarkers %s", idList );
	var viewId = idList.split(",");
	for (var i = 0; i < viewId.length; i++)
		maShowMarkerThumbAsHidden(viewId[i]);
};

maClient.flashHidePopup = function()
{
	console.log("flashHidePopup");
	
	if (!maClient.ready)
		return;
	
	maClient.hidePopup(0);
};

maClient.flashLinkToUrl = function(url)
{
	console.log("flashLinkToUrl '%s'", url);

	if (!maClient.ready)
		return;

	if (typeof url == "undefined" || url === null)
	{
		return;
	}

	try
	{
		var openInNewWindow = url.substring(0, 1) == "1";
		url = url.substring(1);
		
		// Make sure the url starts with http or with a relative reference i.e. '..'.
		if (url.substring(0, 1) != "." && !url.match(/^https?:\/\//i))
		{
        		url = 'http://' + url;
        	}
		
		
		if (openInNewWindow)
		{
			var wnd = window.open(url, "_blank");
			if (wnd === null || typeof wnd == "undefined")
			{
				if (confirm("A popup blocker prevented the following page from being opened in a new window:\n\n" + url + "\n\nPress OK to open the page in the current window."))
					window.location = url;
			}
		}
		else
			window.location = url;
	}
	catch (error)
	{
		alert('MapsAlive detected an error while trying to link to this URL:\n\n' +
			url + '\n\n' + error.message +
			'\n\nPlease report this problem to the author\n' +
			'of this tour so that they can correct it.');
	}
};

maClient.flashMapLoaded = function(mapViewerVersion, restoredViewId)
{
	maClient.runtimeVersion = "v" + maClient.runtimeVersion + ":" + mapViewerVersion;
	console.log("flashMapLoaded %s %s", maClient.runtimeVersion, restoredViewId);
	
	maClient.restoredViewId = restoredViewId;
	
	maClient.waitForClientReady();
};

maClient.flashMarkerCoordsChanged = function(coords)
{
	console.log("flashMarkerCoordsChanged %s", coords);
	var markerCoords = maGetElementByPageId("MarkerCoords");
	if (markerCoords)
	{
		markerCoords.value = coords;
		maChangeDetected();
	}
};

maClient.flashMarkerDeselected = function()
{
	console.log("flashMarkerDeselected");
	maDeselectSelectedMarkerThumb();
};

maClient.flashMarkerMouseOver = function(tooltip, x, y)
{
	//console.log("flashMarkerMouseOver '" + tooltip + "' " + x + "," + y);
	
	if (!maClient.ready)
		return;

	// Don't show a tooltip if the text is all blank. Users can type a space
	// for the tooltip text if they don't want anything to display.
	if (maTrim(tooltip).length === 0)
		return;
	
	maClient.showTooltip(parseInt(x,10), parseInt(y,10), tooltip);
};

maClient.flashMarkerMouseOut = function()
{
	//console.log("flashMarkerMouseOut");

	if (!maClient.ready)
	{
		return;
	}

	maClient.clearBeforeDelay();
	maClient.hideTooltip();

	if (!maClient.usesHidablePopup)
	{
		return;
	}

	if (maClient.popupIsPinned)
	{
		//console.log("== popupIsPinned");
		return;
	}

	if (maClient.showingPopup)
	{
		maClient.hidePopup(maClient.popupAfterDelay());
	}

	if (typeof maOnHotspotChanged != "undefined")
		maOnHotspotChanged(0);
};

maClient.flashMarkerSelected = function(viewId)
{
	console.log("flashMarkerSelected %s", viewId);
	
	if (!maClient.ready)
		return;
	
	maShowSelectedMarkerThumb(viewId);
	maShowMarkerInSlideList(viewId);
};

maClient.flashMouseDragStarted = function()
{
	console.log("flashMouseDragStarted");
	if (maClient.showingPopup && maTour.usesDynamicPopup)
	{
		maClient.mouseDragStarted = true;
		maClient.hidePopup(0);
		maClient.mouseDragStarted = false;
	}
	
	maClient.hideTooltip();
};

maClient.flashMouseMoved = function(x, y)
{
	// This is only called when necessary for debugging.
	console.log("flashMouseMoved %d, %d", x, y);
};

maClient.flashPinnableMarkerClicked = function()
{
	console.log("flashPinnableMarkerClicked");
	
	if (!maClient.ready)
		return;
	
	// If the marker's popup is showing, pin it. Normally the popup is showing when this
	// function is called because the mouse had to go over it to click it; however, if the
	// user click-dragged on a pinnable popup, the drag would have closed the popup and
	// there would be nothing to pin.
	if (maClient.showingPopup)
	{
		maClient.pinPopup();
	}
};

maClient.flashSlideShowIsRunning = function(running)
{
	console.log("flashSlideShowIsRunning %s", running);
	maClient.slideShowIsRunning = running;
};

maClient.flashStageSizeWarning = function(scale)
{
	console.log("flashStageSizeWarning %d%", scale);
	if (maClient.stageSizeWarning)
	{
		if (scale == 100)
		{
			maClient.stageSizeWarning.style.visibility='hidden';
			return;
		}
	}
	else
	{
		if (scale == 100)
			return;
			
		// Create a div to use for the warning message. We put the styling here
		// because no style sheet is available for both the map editor and the
		// published tour (this message can appear for both).
		maClient.stageSizeWarning = document.createElement("div");
		maClient.stageSizeWarning.setAttribute("id", "maBrowserZoomWarning");
		var map = document.getElementById('maMap');
		if (!map)
			return;
		map.appendChild(maClient.stageSizeWarning);
		var s = maClient.stageSizeWarning.style;
		s.left = '30px';
		s.top = '30px';
		s.zIndex = '5000';
		s.visibility = 'hidden';
		s.position = 'absolute';
		s.border = 'solid 2px #000000';
		s.color = '#000000';
		s.backgroundColor = '#fafac6';
		s.fontSize = '16px';
		s.fontFamily = "Verdana, Arial, Helvetica, Sans-Serif";
		s.padding = '6px';
		s.textAlign = 'center';
	}

	maClient.showStageSizeWarning(scale);
};

maClient.showStageSizeWarning = function(scale)
{
	var msg = "<b>Warning</b><br/><br/>Your browser is zoomed to " + scale + "%.<br/><br/>This map works best at 100% and might<br/>not behave correctly at other zoom levels.";
	msg += "<br/><br/><span style='cursor:pointer;' onclick='maClient.stageSizeWarning.style.visibility=\"hidden\";'>[Close]</span>";
	maClient.stageSizeWarning.innerHTML = msg;
	maClient.stageSizeWarning.style.visibility = "visible";
};

maClient.flashViewChanged = function(viewId, x, y, w, h, mouseX, mouseY, isDirEntry)
{
	// console.log("flashViewChanged " + viewId + " " + w + "x" + h + " : " + x + "," + y + " : " + mouseX + "," + mouseY + " " + isDirEntry);

	if (!maClient.ready)
		return;

	if (maClient.isTouchDevice)
	{
		maClient.mouseX = mouseX;
		maClient.mouseY = mouseY;
	}

	maClient.hideTooltip();

	if (maClient.popupIsPinned && (maTour.marker.viewId == viewId || viewId == "0"))
	{
		// If viewId is not zero, the user clicked the marker for the popup that is pinned.
		// If zero, the user clicked another marker that does not display a view when clicked.
		if (viewId == "0")
		{
			maClosePopup();
		}
		else
		{
			console.log("unpin popup");
			maClient.popupIsPinned = false;
			maClient.popupSetControlPanelState(true, false);
		}
		return;
	}

	maClient.dirEntryClicked = isDirEntry;
	if (maTour.hasDirectory && maClient.dirShowing && !maTour.dir.staysOpen)
	{
		if (maClient.dirMouseIsOver && !maClient.dirEntryClicked && !maClient.isTouchDevice)
		{
			// Ignore the view change if the mouse moved over a marker that was
			// under the directory panel (occurs on Mac browsers).
			return;
		}
		else
		{
			// Hide the directory before showing the view.
			maClient.dirShow(false);
		}
	}

	if (maTour.usesPopup)
	{
		// If waiting to close the popup, stop waiting.
		maClient.clearAfterDelay();

		// If the moused just moved over the marker for the popup that is already
		// showing, then just leave the popup showing.
		if (maClient.showingPopup && viewId == maTour.marker.viewId)
		{
			return;
		}
	}

	// Remember information about the marker that was passed from Flash.
	maTour.marker.viewId = parseInt(viewId, 10);
	maTour.marker.x = x;
	maTour.marker.y = y;
	maTour.marker.w = w;
	maTour.marker.h = h;

	// Determine if we should process the view change immediately, or delay awhile.
	if (!maClient.dirEntryClicked && !maClient.showingPopup && (maTour.usesPopup && maTour.popup.delayType == 1 && maTour.popup.delay !== 0))
	{
		// Delay Before (delayType 1) is set with a non-zero delay. Don't change slides until the delay expires.
		// Note that before setting the delay, we have to make sure that a previous delay is not still pending.
		// If we don't clear it, the original timer will fire along with the one we are setting now and we'll
		// end up getting two calls to maClient.slideChanged.
		maClient.clearBeforeDelay();
		maClient.popupDelayBeforeIntervalId = setTimeout("maClient.slideChanged(true);", maTour.popup.delay);
	}
	else
	{
		if (maClient.dirEntryClicked || maClient.isTouchDevice)
		{
			// When the user clicks a directory entry, pin the popup for the selected hotspot.
			maClient.pinNextPopup = true;
		}
		else
		{
			if (maClient.popupIsPinned)
			{
				// The popup is currently pinned, but since the next popup is not supposed to
				// be pinned, we need to change the state to unpinned. This can happen when the
				// user clicks an entry in the directory (causes the popup to appear pinned) and
				// then while the popup is pinned, clicks on another marker. The new marker's
				// popup should appear unpinned.
				maClient.popupSetControlPanelState(false, false);
			}
		}

		var mouseIsOverMarker = !maClient.dirEntryClicked && mouseX > 0 & mouseY > 0;
		maClient.slideChanged(mouseIsOverMarker);
	}

	if (typeof maOnHotspotChanged != "undefined")
		maOnHotspotChanged(maClient.slide.slideId);
};

maClient.flashZoomStateChanged = function(percent, x, y)
{
	console.log("flashZoomStateChanged %s, %d, %d", percent, x, y);
	maGetElementByPageId("ZoomState").value = percent + "," + x + "," + y;
	maChangeDetected();
};

//-----------------------------------//
// Private functions                 //
//-----------------------------------//

maClient.convertEffects = function(effects)
{
	var s = effects.toLowerCase();
	s = s.replace(/blend/g, "-1,1");
	s = s.replace(/innerglow/g, "-1,2");
	s = s.replace(/glow/g, "-1,3");
	s = s.replace(/shadow/g, "-1,4");
	
	// Allow a semicolon to be used to separate effects.
	s = s.replace(/;/g, ",");
	return s;
};

maClient.swfPath = function()
{
	return maTour.editMode ? maTour.appRuntimeUrl : maClient.path;
};

function maAttachEventListener(target, eventType, functionRef)
{
	if (typeof target.addEventListener != "undefined")
		target.addEventListener(eventType, functionRef, false);
	else
		target.attachEvent("on" + eventType, functionRef);
	return true; 
}

maClient.elementHasNoId = function(e)
{
	return e && (typeof e.id == 'undefined' || e.id.length === 0);
};

maClient.Flash = function()
{
};

maClient.Flash.prototype.addParam = function(name, value)
{
	this.params[name] = value;
};

maClient.Flash.prototype.addFlashVar = function(name, value)
{
	this.flashVars[name] = value;
};

maClient.flashPresent = function()
{
	return maClient.getPlayerVersion().major >= 8;
};

maClient.showRenderErrorMessage = function()
{
    var msg1 = maTour.mobileDeviceFeaturesEnabled ? "<b>This map needs Flash or HTML5</b><hr/>Your browser does not support HTML5.<br/><br/>" :
	    "<b>Adobe Flash is required for map editing.<br/>The tours you create do not need Flash.<br/>They will run in any browser.</b><hr/>";
    var msg2 = "The upcoming version of MapsAlive (due out later in 2020) will not require Flash.<br/><br/>For now, please read about<br/><a href='Announcements.aspx'>how to enable Flash in your browser</a>.";
	var e = document.createElement("div");
	e.innerHTML = msg1 + msg2;
	var s = e.style;
	s.position = "absolute";
	s.zIndex = 1000;
	s.left = "30px";
	s.top = "30px";
	s.backgroundColor = "#f3f3f3";
	s.border = "1px solid #777777";
	s.color = "#000055";
	s.fontSize = "11px";
	s.fontFamily = "arial,helvetica,sans-serif";
	s.padding = "8px";
	s.textAlign = "center";
	s.width = "230px";
	var map = document.getElementById('maMap');
	if (!map)
		return;
	map.appendChild(e);
	var d = document.createElement("div");
	d.style.padding = "12px 0px 4px 0px";
	d.style.textAlign = "center";
	e.appendChild(d);
	// var a = document.createElement("a");
	// a.href = "http://macromedia.com/shockwave/download/download.cgi?P1_Prod_Version=ShockwaveFlash";
	// a.target = "_blank";
	// d.appendChild(a);
	// var i = document.createElement("img");
	// i.src = maTour.editMode ? "../Runtime/get_flash_player.gif" : maClient.path + "0020_" + maTour.version + ".gif";
	// i.style.border = 0;
	// a.appendChild(i);
};

maClient.Flash.prototype.getParamTags = function()
{
	var paramTags = "";
	for (var key in this.params)
		paramTags += '<param name="' + key + '" value="' + this.params[key] + '" />';
	return paramTags;
};

maClient.Flash.prototype.getFlashVars = function()
{
	var pair = new Array();
	for (var key in this.flashVars)
		pair.push(key + "=" + this.flashVars[key]);
	return pair.join("&");
};

maClient.Flash.prototype.emitMapHtml = function(editorUsage)
{
	maClient.mapIsHtml5 = true;
	var html = "";
	html += '<canvas id="maStatusLayer" style="position:absolute;z-index:0;-webkit-user-select:none;" width="' + maTour.stageW + '" height="' + maTour.stageH + '"></canvas>';
	html += '<canvas id="maMapLayer" style="position:absolute;z-index:1;-webkit-user-select:none;" width="' + maTour.stageW + '" height="' + maTour.stageH + '"></canvas>';
	html += '<canvas id="maMarkerLayer" style="position:absolute;z-index:2;-webkit-user-select:none;-webkit-tap-highlight-color:rgba(0,0,0,0);" width="' + maTour.stageW + '" height="' + maTour.stageH + '"><div style="font-family:sans-serif;width:200px;padding:12px;color:#333;">This interactive map requires HTML5.You can view it with these browsers: Safari, Firefox, IE9, Chrome, or Opera.</div></canvas>';
	document.write(html);
};

function maCaptureMousePosition(event)
{
	maClient.captureMousePosition(event);
}

maClient.captureMousePosition = function(event)
{
	if (!maClient.ready || !maClient.mapLoaded)
		return;

	if (maClient.isTouchDevice)
		return;

	if (typeof event == "undefined")
		event = window.event;

	var scrollingPosition = maClient.getScrollingPosition();

	if (typeof event.pageX != "undefined" && typeof event.x != "undefined")
	{
		maClient.mouseX = event.pageX;
		maClient.mouseY = event.pageY;
	}
	else
	{
		maClient.mouseX = event.clientX + scrollingPosition[0];
		maClient.mouseY = event.clientY + scrollingPosition[1];
	}

	// Move the preview panel as the mouse moves.
	if (maClient.showingPreview)
		maClient.dirMovePreviewPanel();

	if (maClient.mapIsHtml5 && !maClient.mobile)
	{
		// Prevent a popup or tooltip from following the mouse if the cursor moves
		// in such a way that the map does not know that the mouse has moved off of a marker.
		// This can happen, for example, if the mouse quickly moves off the map, or
		// goes over another element (like another app window) that overlaps the map.
		if (!maClient.map.getMouseLocation() || maClient.map.getMarkerUnderMouse() === 0)
		{
			if (!maClient.mouseIsOverPopup)
			{
				maClient.flashMarkerMouseOut();
			}
			return;
		}
	}

	if (maClient.showingTooltip)
		maClient.moveTooltip();

	var movePopupWithMouse =
		maClient.showingPopup &&
		maTour.usesDynamicPopup &&
		maTour.popup.location == maClient.popupLocationMouse &&
		!maTour.popup.allowMouseover &&
		!maClient.popupIsPinned;

	//console.log("captureMousePosition " + " " + movePopupWithMouse + " " + maClient.mouseX + " " + maClient.mouseY);

	if (movePopupWithMouse)
	{
		// Move the popup to the current mouse location.
		maClient.setPopupLocation(true);
	}
};

maClient.showPage = function(pageId, slideId)
{
	var page = maClient.getPageByPageId(pageId);
	if (page)
	{
		var url = "page" + page.pageNumber + ".htm";
		var slide = maClient.getSlideBySlideId(slideId);
		var viewId = slide ? slide.viewId : null;
			
		if (page.pageNumber != maTour.pageNumber)
			maClient.updateLevelState();
		
		maClient.goToPage(url, viewId, maClient.levelState);
		
		return true;
	}
	else
	{
		return false;
	}
};

maClient.goToPage = function(url, viewId, dirCollapseState)
{
	var pn = url.substr(4, url.indexOf('.') - 4);
	
	if (maClient.preview)
	{
		var aid = maGetQueryStringArg("aid");
		var e = document.getElementById('TasksPanel');
		var sa = e && e.style.display == 'block';
		e = document.getElementById('SnippetsPanel');
		var ss = e && e.style.display == 'block';
		var tourId = maGetQueryStringArg("tourId");
		var accountId = maGetQueryStringArg("accountId");
		var tid = tourId.length > 0 ? "&tourId=" + tourId + "&accountId=" + accountId : "";
		var mid = viewId === null ? "" : "&hotspot=" + maClient.getSlide(viewId).slideId;
		var find = "";
		var cs = "";
		if (maTour.hasDirectory)
		{
			if (dirCollapseState === null)
			{
				maClient.updateLevelState();
				dirCollapseState = maClient.levelState;
			}
			cs = "&cs=" + dirCollapseState;
			
			find = maClient.dirSearchText();
			if (find.length > 0)
				find = "&find=" + find;
		}
		window.location = 'TourPreview.aspx?aid=' + aid + '&page=' + pn + '&sa=' + (sa ? '1' : '0') + '&ss=' + (ss ? '1' : '0') + tid + mid + cs + find;
	}
	else
	{
		if (maClient.page)
		{
			if (maClient.unbranded)
				url = url.substring(0, url.length - 4) + '_.htm';
			window.location = maClient.constructGoToPageUrl(url, viewId, dirCollapseState);
		}
		else
		{
			if (typeof maClient.hostPageUrl == 'undefined' || maClient.hostPageUrl.indexOf("#") == -1)
			{
				alert("This web page has not yet been configured to allow this interactive map to display another page. To learn about embedding a multi-page tour, see the MapsAlive User Guide for Integrating Interactive Maps with Web Pages.");
			}
			else
			{
				url = maClient.hostPageUrl.replace(/#/, pn);
				window.location = maClient.constructGoToPageUrl(url, viewId, dirCollapseState);
			}
		}
	}
};

maClient.constructGoToPageUrl = function(url, viewId, dirCollapseState)
{
	var mid = "";
	var cs = "";
	var find = "";
	var prefix = "";
	
	if (viewId)
	{
		prefix = url.indexOf("?") == -1 ? "?" : "&";
		mid = prefix + "hotspot=" + maClient.getSlide(viewId).slideId;
	}
	
	if (maTour.hasDirectory)
	{
		if (dirCollapseState === null || typeof dirCollapseState == "undefined")
		{
			maClient.updateLevelState();
			dirCollapseState = maClient.levelState;
		}
		
		if (dirCollapseState.length > 0)
		{
			prefix = prefix.length === 0 && url.indexOf("?") == -1 ? "?" : "&";
			cs = prefix + "cs=" + dirCollapseState;
		}
		
		find = maClient.dirSearchText();
		if (find.length > 0)
		{
			prefix = prefix.length === 0 && url.indexOf("?") == -1 ? "?" : "&";
			find = prefix + "find=" + find;
		}
	}
	
	return url + mid + cs + find;
};

maClient.getSlideBySlideId = function(slideId)
{
	// In case the slideId came in via the query string, restore
	// any escaped characters such as %20 for blank spaces.
	slideId = unescape(slideId);
	
	for (var viewId in maClient.slides)
	{
		var slide = maClient.getSlide(viewId);
		if (slide.slideId.toLowerCase() == slideId.toLowerCase())
		{
			// Make sure the slide belongs to the current page.
			if (slide.pageNumber != maTour.pageNumber)
				continue;
			
			return slide;
		}
	}
	return null;
};

maClient.getViewIdBySlideId = function(slideId)
{
	// In case the slideId came in via the query string, restore
	// any escaped characters such as %20 for blank spaces.
	slideId = unescape(slideId);
	
	for (var viewId in maClient.slides)
	{
		var slide = maClient.getSlide(viewId);
		if (slide.slideId.toLowerCase() == slideId.toLowerCase())
		{
			// Make sure the slide belongs to the current page.
			if (slide.pageNumber != maTour.pageNumber)
				continue;

			return viewId;
		}	
	}
	return null;
};

maClient.slideChanged = function(mouseIsOverMarker)
{
	//console.log("slideChanged " + mouseIsOverMarker);

	maClient.changeSlide(mouseIsOverMarker && (!maClient.pinNextPopup || maClient.isTouchDevice));
	
	if (maTour.usesPopup)
	{
		maClient.showPopup(true);
		
		if (maClient.pinNextPopup)
		{
			maClient.pinPopup();
		}
	}
};

maClient.getPlayerVersion = function()
{
	var playerVersion = new maClient.PlayerVersion([0,0,0]);
	if (navigator.plugins && navigator.mimeTypes.length)
	{
		var plugin = navigator.plugins["Shockwave Flash"];
		if (plugin && plugin.description)
		{
			playerVersion = new maClient.PlayerVersion(plugin.description.replace(/([a-zA-Z]|\s)+/, "").replace(/(\s+r|\s+b[0-9]+)/, ".").split("."));
		}
	}
	else
	{
		// do minor version lookup in IE, but avoid fp6 crashing issues
		// see http://blog.deconcept.com/2006/01/11/getvariable-setvariable-crash-internet-explorer-flash-6/
		var axo = null;
		try
		{
			axo = new ActiveXObject("ShockwaveFlash.ShockwaveFlash.7");
		}
		catch(e)
		{
			try
			{
				axo = new ActiveXObject("ShockwaveFlash.ShockwaveFlash.6");
				playerVersion = new maClient.PlayerVersion([6,0,21]);
				axo.AllowScriptAccess = "always"; // throws if player version < 6.0.47
			}
			catch(e)
			{
				if (playerVersion.major == 6)
				{
					return playerVersion;
				}
			}
			try
			{
				axo = new ActiveXObject("ShockwaveFlash.ShockwaveFlash");
			}
			catch(e)
			{}
		}
		if (axo !== null)
		{
			playerVersion = new maClient.PlayerVersion(axo.GetVariable("$version").split(" ")[1].split(","));
		}
	}
	return playerVersion;
};

maClient.PlayerVersion = function(arrVersion)
{
	this.major = arrVersion[0] !== null ? parseInt(arrVersion[0],10) : 0;
	this.minor = arrVersion[1] !== null ? parseInt(arrVersion[1],10) : 0;
	this.rev = arrVersion[2] !== null ? parseInt(arrVersion[2],10) : 0;
};

function maTrim(string)
{
	if (string.length === 0)
		return string;
	while (string.substr(0, 1) == " ")
		string = string.substring(1, string.length);
	while (string.substr(string.length - 1, 1) == " ")
		string = string.substring(0, string.length - 1);
	return string;
}


maClient.createViewListFromRouteSlideList = function(slideList)
{
	// Make sure the list is a string in case the parameter is a single integer value.
	slideList += "";

	var viewIdList = "";
	if (slideList.length > 0)
	{
		var sections = slideList.split(";");
		for (var i in sections)
		{
			if (viewIdList.length > 0)
				viewIdList += ";";
			var section = maTrim(sections[i]);
			
			viewIdList += maClient.createViewListFromSlideList(section);
		}
	}
	return viewIdList;
};

maClient.createViewListFromSlideList = function(slideList)
{
	// Make sure the list is a string in case the parameter is a single integer value.
	slideList += "";

	if (slideList == "*")
		return slideList;
	
	var viewIdList = "";
	if (slideList.length > 0)
	{
		var slideIds = slideList.split(",");
		for (var i in slideIds)
		{
			if (viewIdList.length > 0)
				viewIdList += ",";
			var slideId = maTrim(slideIds[i]);
			
			var slide = maClient.getSlideBySlideId(slideId);
			if (slide)
				viewIdList += slide.viewId;
		}
	}
	return viewIdList;
};

maClient.getPageByPageNumber = function(pageNumber)
{
	return maClient.pages[pageNumber];
};

maClient.getPageByPageId = function(pageId)
{
	for (var pageNumber in maClient.pages)
	{
		var page = maClient.getPageByPageNumber(pageNumber);
		if (page.pageId.toLowerCase() == pageId.toLowerCase())
			return page;
	}
	return null;	
};

maClient.getSlide = function(viewId)
{
	var slide = maClient.slides[viewId];
	if (typeof slide == "undefined")
		slide = null;
	return slide;
};

maClient.getPageBySlideViewId = function(viewId)
{
	return maClient.getPageByPageNumber(maClient.getSlide(viewId).pageNumber);
};

maClient.lookupString = function(index)
{
	return maTour.stringTable[index];
};

maClient.waitForClientReady = function()
{
	console.log("waitForClientReady (start)");

	if (!maClient.mapIsHtml5)
	{
		maClient.mapLoaded = true;
		maClient.map = document.getElementById(maTour.editMode ? "fsmapEditor" : "fsmapViewer");
	}

	clearTimeout(maClient.waitForClientReadyIntervalId);
	if (!maClient.ready)
	{
		// The map is loaded, but this client is not ready yet. This is only likely to happen
		// with a tour that contains hundreds of slides where it could take awhile to load the
		// slide table. Wait a little while and try again, but give up after 5 seconds.
		maClient.waitForClientReadyCount++;
		if (maClient.waitForClientReadyCount > 50)
			return;

		console.log(">>> waitForClientReadyCount " + maClient.waitForClientReadyCount);
		maClient.waitForClientReadyIntervalId = setTimeout("maClient.waitForClientReady();", 100);
		return;
	}

	console.log("Client is ready");

	if (maClient.mapIsHtml5)
	{
		try
		{
			maClient.map = new maClient.Html5();
		}
		catch (error)
		{
			alert("MapsAlive could not load its HTML5 JavaScript libary. This can happen if the <script> tag for mapviewer.js is missing.");
		}
		try
		{
			maClient.map.loadMap();
		}
		catch (error)
		{
			// This should only happen if the users specifies html5=1 on the query string and the browser does not support html5.
			return;
		}
	}

	// See if there's a hotspot Id on the query string.
	var slideId = maGetQueryStringArg("hotspot");

	if (slideId.length === 0)
	{
		// Also check for the deprecated arg "slide" for compatibility with MapsAlive v2.
		slideId = maGetQueryStringArg("slide");
	}

	if (slideId.length === 0)
	{
		// Also check for the deprecated arg "marker" for compatibility with MapsAlive v2.
		var viewId = maGetQueryStringArg("marker");
		if (viewId.length)
		{
			slideId = maClient.getSlide(viewId).slideId;
		}
	}

	if (slideId.length !== 0)
	{
		maClient.hotspotOnQueryString = true;
	}

	if (slideId.length === 0 && maClient.restoredViewId !== 0)
	{
		// The tour's state was restored by Flash. We check this last so that the query string takes precedence.
		slideId = maClient.getSlide(maClient.restoredViewId).slideId;
	}

	var slide = null;
	maClient.viewIdToShowWhenMapLoaded = 0;

	if (slideId.length !== 0)
	{
		// See if the id matches an actual slide on this map.
		slide = maClient.getSlideBySlideId(slideId);
	}

	if (slide !== null)
	{
		// Remember the hotspot that was specified on the query string or restored by Flash.
		// The second parameter to showSlide is isDir, which is a misnomer. We set it false
		// for a restored hotspot so that it's marker does not blink when the page reloads.
		maClient.viewIdToShowWhenMapLoaded = slide.viewId;
	}
	else if (maTour.selectedViewId && !maClient.usesHidablePopup)
	{
		if (maTour.usesFixedPopup)
		{
			// Make the popup visible. It will stay visible since it's not hidable.
			maClient.showPopup(true);
		}

		// Display the default first slide and set its marker to the selected state.
		maTour.marker.viewId = maTour.selectedViewId;
		maClient.changeSlide(false);

		if (!maTour.infoPage && maClient.map)
		{
			// Show the first slide's marker as selected.
			maClient.map.setMarkerSelectedState(maTour.selectedViewId);
		}
	}

	if (!maClient.mapIsHtml5)
	{
		// We make this call now for Flash, but mapviewer.js calls it when it's ready.
		maClient.onMapLoaded();

		if (maClient.opera)
			maClient.showOperaWarning();
	}

	// For debugging show the Flash map viewer version. For this code to work, the page must
	// have custom HTML (Top, Absolute, or Bottom) containing: <div id="maRuntimeVersion" style="height:20px;"></div>
	var v = document.getElementById("maRuntimeVersion");
	if (v)
		v.innerHTML = maClient.runtimeVersion;

	console.log("waitForClientReady (finish)");
};

maClient.onMapLoaded = function()
{
	maClient.mapLoaded = true;

	if (maClient.viewIdToShowWhenMapLoaded !== 0)
	{
		maClient.showSlide(maClient.viewIdToShowWhenMapLoaded, maClient.restoredViewId === 0);
	}

	if (maTour.hasDirectory && maTour.dir.staysOpen)
	{
		// We waited until the map loaded to show the directory because dirShow messages the map.
		maClient.dirShow(true);
	}

	if (typeof maOnMapLoaded != "undefined")
		maOnMapLoaded();
};

maClient.showOperaWarning = function()
{
	var map = document.getElementById('maMap');
	if (!map)
		return;
	maClient.operaWarning = document.createElement("div");
	map.appendChild(maClient.operaWarning);
	var s = maClient.operaWarning.style;
	s.left = '20px';
	s.top = '20px';
	s.zIndex = '6000';
	s.position = 'absolute';
	s.border = 'solid 2px #000000';
	s.color = '#000000';
	s.opacity = 0.75;
	s.backgroundColor = '#fafac6';
	s.fontSize = '10px';
	s.fontFamily = "Verdana, Arial, Helvetica, Sans-Serif";
	s.padding = '4px';
	s.textAlign = 'center';
	s.visibility = 'visible';
	s.width = '160px';

	var msg = "<b>Opera Browser Warning</b><br/><br/>Some features of this interactive map might not be supported by Opera.";
	msg += "<br/><br/><span style='cursor:pointer;' onclick='maClient.operaWarning.style.visibility=\"hidden\";'>[Close]</span>";
	maClient.operaWarning.innerHTML = msg;
};

maClient.showSlide = function(viewId, isDirEntry)
{
	console.log("showSlide " + viewId + " " + isDirEntry);

	if (viewId == -1)
		return;

	if (typeof mapViewer == "undefined" && maClient.map === null)
	{
		// Page has slide, but no map.
		maTour.marker.viewId = parseInt(viewId, 10);
		maTour.marker.x = 0;
		maTour.marker.y = 0;
		maTour.marker.w = 0;
		maTour.marker.h = 0;
		maClient.changeSlide(false);
	}
	else
	{
		if (maTour.usesDynamicPopup)
		{
			// When this method is called repeatedly with the same Id for a popup,
			// toggle the popup (hide/show) on each call.
			if (maClient.viewId == viewId && maClient.popupPanel.style.visibility == "visible")
			{
				maClosePopup();
				return;
			}

			maClient.pinNextPopup = maTour.popup.pinOnClick || maTour.popup.allowMouseover;
		}

		if (maTour.usesPopup)
			maClient.hidePopup(0);

		if (maTour.hasDirectory)
		{
			// The directory would normally get closed when the new view is shown, but if the map
			// does not show a view the directory stays open so we close it now.
			maClient.dirShow(false);
		}

		maClient.map.selectMarkerAndShowView(viewId, typeof isDirEntry == 'undefined' ? 0 : isDirEntry);
	}
};

maClient.showMarker = function(viewId, show)
{
	// This method is called when the mouse moves over a hotspot name in the left menu.
	// Ignore the call if the page that has hotspots, but no map.
	if (typeof mapViewer == "undefined")
		return;
		
	if (!maClient.mapLoaded)
		return;

	maClient.map.showMarkerSelected(viewId, show);
};

maClient.setText = function(e, index)
{
	if (!e)
		return;
	var text;
	if (index == 1)
		text = maClient.getSlide(maClient.viewId).title;
	else
		text = maClient.getSlide(maClient.viewId).htmlText;
	try
	{
		var noText;
		if (index == 1)
		{
			if (text.length > 0 && text.substring(0,1) == '_')
				text = text.substring(1);
			noText = false;
		}
		else if (index == 2)
		{
			noText = text.length === 0;
		}
		e.style.display = noText ? 'none' : 'block';
		e.innerHTML = noText ? "" : text;
	}
	catch(error)
	{
		e.innerHTML='The browser reported an error: ' + error.message;
	}
};

maClient.setSlideSelector = function()
{
	// If this page has a dropdown list, keep the selected item in the list in sync with the selected view.
	var dropdownList = document.getElementById('maHotspotDropdown');
	if (dropdownList)
	{
		for (var option = 0; option < dropdownList.options.length; option++)
		{
			if (dropdownList.options[option].value == maClient.viewId)
			{
				dropdownList.options[option].selected = true;
				dropdownList.selectedIndex = dropdownList[option].index;
				break;
			}
		}
	}
};

maClient.setLeftMenuItem = function()
{
	// If this page is showing slide titles in the left menu, deselect the currently selected
	// slide title and select the slide title corresponding to the the selected slide.
	var slideTitlesInMenu = document.getElementById('maHotspotNamesInMenu');
	if (slideTitlesInMenu)
	{
		var newSelectedViewId = document.getElementById("ma" + maClient.viewId);
		if (newSelectedViewId)
		{
			var oldSelectedViewId = document.getElementById("ma" + maTour.selectedViewId);
			if (oldSelectedViewId) 
				oldSelectedViewId.className = '';
			newSelectedViewId.className = "currentSlide";
			
			// save the new selected slide for next time
			maTour.selectedViewId = maClient.viewId;
		}
	}
};

maClient.getTextHeight = function(textWidth)
{
	// Handle the case where overly large margins create a negative text width.
	if (textWidth < 0)
		textWidth = 0;
	
	// Get the text height for this width by forcing the offset height to be calculated.
	maClient.textArea.style.width = textWidth + "px";
	maClient.textArea.style.height = "auto";
	return maClient.textArea.offsetHeight;
};

maClient.setActualPopupHeight = function(h)
{
	// Make sure the height is at least ha high as the min.
	h = Math.max(h, maClient.popupMinH());
	
	// Make sure the height does not exceed the max.
	maTour.popup.actualH = Math.min(h, maClient.popupMaxH());
	
	maClient.popupPanel.style.height = maTour.popup.actualH + "px";
};

maClient.setActualPopupWidth = function(w)
{
	// Make sure the width is at least as wide as the min.
	w = Math.max(w, maClient.popupMinW());
	
	// Make sure the width does not exceed the max.
	maTour.popup.actualW = Math.min(w, maClient.popupMaxW());
	
	maClient.popupPanel.style.width = maTour.popup.actualW + "px";
};

maClient.tourLayoutIs = function(list)
{
	var item = list.split(",");
	for (var i = 0; i < item.length; i++)
	{
		if (maTour.layout == item[i])
			return true;
	}
	return false;
};

maClient.setFixedLayoutAreaSizes = function()
{
	// This method adjusts the width and/or height of the text and/or media sections
	// for layouts where the text or media size or position changes depending on the
	// content of the slide currently being displayed.
	
	switch (maTour.layout)
	{
		case "HMMIT":
		case "HITMM":
		case "VIITT":
			// In these layouts the text area is absolute positioned to the right of the
			// media and may need to move toward the left and extend its width so there is
			// no gap between the right edge of the media and the left edge of the text.
			
			var spacingV = 0;
			if (maClient.tourLayoutIs("HMMIT,HITMM"))
			{
				// When there's no image, count the vertical spacer that would normally
				// appear between the image and text as part of the text area's width.
				if (maClient.slide.mediaW === 0)
					spacingV = maTour.layoutSpacingV;	
			}
			
			// Let the media occupy its full width.
			maClient.mediaArea.style.width = maClient.slide.mediaW + "px";
			
			// Make the text area width the normal width plus any space that the media is not using.
			maClient.textArea.style.width = maTour.textAreaW + maClient.slide.mediaDeltaW + spacingV + "px";
			
			// Move the text to the left so that its left edge is next to the media's right edge.
			maClient.textArea.style.left = maClient.textAreaLeft - maClient.slide.mediaDeltaW - spacingV + "px";
			break;
		
		case "HMMTI":
		case "HTIMM":
		case "VTTII":
			// In these layouts the media is absolute positioned to the far right of the text/media area.
			
			// Position the media so that it is all the way to the right.
			var mediaArea = document.getElementById("maAbsBlock");
			mediaArea.style.width = maClient.slide.mediaW + "px";
			var imageLeft = maTour.layoutMarginLeft + maTour.textAreaW + maClient.slide.mediaDeltaW + maTour.layoutSpacingV;
			mediaArea.style.left = imageLeft + "px";
			
			// Adjust the text area to fill the space from the tour's left margin to the media's left edge.
			maClient.textArea.style.width = (imageLeft - maTour.layoutSpacingV - maTour.layoutMarginLeft) + "px";
			break;
			
		case "VMMTI":
		case "VTIMM":
			// In these layouts the text appears above the media and the media floats up to meet
			// the bottom of the text.
			
			// Force the text area offsetHeight to be recalculated for the width.
			maClient.textArea.style.height = "auto";
			
			// If the text area height is taller than the text, shorten the text area height so that
			// the top of the media will be below the text without a gap except for the spacing.
			var textH = maClient.calculateTextAreaHeight();
			if (maClient.textArea.offsetHeight >= textH)
				maClient.textArea.style.height = textH + "px";
				
			if (maClient.slideMediaIsEmbed())
			{
				// When the media is embedded, set the media area height to the height available.
				var mediaH = maTour.canvasH - maTour.layoutMarginTop - maClient.textArea.offsetHeight - maTour.layoutSpacingH - maTour.layoutMarginBottom;
				maClient.mediaArea.style.height = mediaH + "px";
			}
			else
			{
				// Clear the height that might have been set if the previous hotspot was embedded media.
				maClient.mediaArea.style.height = "auto";
			}
			break;
			
		case "HTTII":
			// In this layout, the text height is fixed, even though it means letting there be
			// empty space below the text and above the image. Otherwise the image would float
			// up and leave space below the image and the bottom of the tour.
			maClient.textArea.style.height = maClient.calculateTextAreaHeight() + "px";
			break;
			
		case "VMMIT":
		case "VITMM":
		case "HIITT":
			// In these layouts the media appears above the text and the text floats up to meet
			// the bottom edge of the media.
			if (maClient.slideMediaIsEmbed())
			{
				// When the media is embedded, set the media area height to the media's height.
				maClient.mediaArea.style.height = maClient.slide.mediaH + "px";
			}
			else
			{
				// Clear the height that might have been set if the previous hotspot was embedded media.
				// Note that we used to set the height to "auto" but when we changed to the simpler DOCTYPE
				// for HTML5, it caused there to be an extra 4 pixel gap between the image and text. We
				// avoid that problem by setting the height to the image height.
				maClient.mediaArea.style.height = maClient.slide.mediaH + "px";
			}
		
			// Introduce a space between the media and text. If there is no media, hide it.
			var noTopImage = maClient.slide.mediaH === 0;
			maClient.textArea.style.marginTop = (noTopImage ? 0 : maTour.layoutSpacingH) + "px";
			maClient.mediaArea.style.display = noTopImage ? "none" : "block";
			maClient.textArea.style.height = maClient.calculateTextAreaHeight() + "px";
			break;
			
		default:
			break;
	}
};

maClient.calculateTextAreaHeight = function()
{
	// The text area height is all the vertical space not occupied by the media.
	var noTopImage = maClient.slide.mediaH === 0;
	var spacingH = noTopImage ? 0 : maTour.layoutSpacingH;
	var textH = maTour.canvasH - maTour.layoutMarginTop - maClient.slide.mediaH - spacingH - maTour.layoutMarginBottom;
	
	// Check for negative height due to unusually large margins.
	if (textH < 0)
		textH = 0;
	
	return textH;
};

maClient.slideImageSrc = function(slide)
{
	if (slide.imageLoaded)
	{
		// The image was preloaded and is ready to show.
		return slide.image.src;
	}
	else
	{
		// The image has not loaded yet into the slide object's image object.
		// Request that it be fetched right now from its URL. 
		return slide.imageSrc;
	}
};

maClient.slideMediaIsEmbed = function()
{
	return maClient.slide.mediaType == 1;
};

maClient.setSlideMedia = function()
{
	var showMedia = maClient.slide.mediaW > 0 && maClient.slide.mediaH > 0;

	if (showMedia)
	{
		if (maClient.slideMediaIsEmbed())
		{
			maClient.mediaArea.innerHTML = maClient.getSlide(maClient.viewId).embedText;
		}
		else
		{
			var mediaW = maClient.slide.mediaW + "px";
			var mediaH = maClient.slide.mediaH + "px";
			var src = maClient.slideImageSrc(maClient.slide);
			maClient.mediaArea.innerHTML =
				"<img id='" + "maHotspotImage" +
				"' src='" + src +
				"' style='" + "width:" + mediaW + ";height:" + mediaH +
				"' />";
		}
	}
	else
	{
		maClient.mediaArea.innerHTML = "";
	}
};

maClient.setLiveData = function(slide)
{
	// console.log("setLiveData for " + slide.slideId + " : " + slide.title + " : " + slide.viewId);

	slide.liveDataUpdateTime = (new Date()).getTime();	
	slide.liveDataRequestPending = false;
	
	// Determine if this slide being passed in is the current slide. If it's not, either the response to the
	// slide's Live Data request came after the user selected another slide, or the mouse is over a directory
	// entry. Note that we compare using the viewId instead of the slideId because only the viewId is unique
	// within the tour.
	if (maClient.viewId === 0 || maClient.slide.viewId != slide.viewId)
		return;

	// Set the slide's text area unless no Live Data text was returned. Prior to calling the server,
	// we put the loading indicator HTML into the text area. If it's still there, we know that no
	// text came back so we set the slide's text to blank in order to erase the loading indicator.
	if (maClient.slide.htmlText == maClient.loadingGraphic() && maClient.slideText)
		maClient.slide.htmlText = "";
	
	maClient.setLiveDataText(slide);
};

maClient.setLiveDataText = function(slide)
{
	console.log("setLiveDataText for hotspot %s", slide.slideId);
	
	maClient.setSlideText();
	
	if (maTour.usesPopup)
	{
		maClient.setPopupSize();
		maClient.setPopupLocation(false);
	}
	else
	{
		maClient.setFixedLayoutAreaSizes();
	}
};

maClient.getLiveData = function(slide)
{
	// console.log("getLiveData for " + slide.slideId + " : " + slide.title + " : " + slide.viewId);
	var message;
	if (maClient.preview)
	{
		message = "Live Data for this slide cannot be requested from your server while in Tour Preview. This is normal behavior.";
		maClient.reportLiveDataError(slide, null, message, script);
	}
	else
	{
		var script = slide.messengerFunction;
		if (script.length === 0)
		{
			message = "This Live Data slide is missing its messenger function.";
			maClient.reportLiveDataError(slide, null, message, "<i>Missing</i>");
		}
		else
		{
			try
			{
				eval(script);
			}
			catch (error)
			{
				maClient.reportLiveDataError(slide, null, error.message, script);
			}
		}
	}
};

function maLiveDataErrorSettings(message, color, backgroundColor, showDetail)
{
	// This class holds the information needed to report an error.
	this.message = message;
	this.color = color;
	this.backgroundColor = backgroundColor;
	this.showDetail = showDetail;
}

maClient.reportLiveDataError = function(slide, request, message, script)
{
	var settings = maClient.liveDataErrorSettings;
	
	if (settings === null)
	{
		if (maClient.preview)
			settings = new maLiveDataErrorSettings(message, "green", "#eee", false);
		else
			settings = new maLiveDataErrorSettings("Data could not be obtained from the server.", "red", "#eee", true);
	}

	var html = "<div>" + settings.message + "</div>";
	
	if (settings.showDetail)
	{
		var reason = message;
		if (script !== null && !maClient.preview)
			reason = "An error occurred while requesting Live Data.";
		
		html += "<hr/>";
		html += "<table>";
		html += "<tr><td valign='top' style='font-weight:bold;'>Reason:</td><td>" + reason + "</td></tr>";
		
		if (request !== null)
		{
			html += "<tr><td valign='top' style='font-weight:bold;'>Slide Title:</td><td>" + request.slide.title + "</td></tr>";
			html += "<tr><td valign='top' style='font-weight:bold;'>Slide Id:</td><td>" + request.slide.slideId + "</td></tr>";
			html += "<tr><td valign='top' style='font-weight:bold;'>Server URL:</td><td>" + request.url + "</td></tr>";
			html += "<tr><td valign='top' style='font-weight:bold;'>Parameters:</td><td>" + request.parameters + "</td></tr>";
			var plural = request.cachePeriodSeconds !== 1 ? "s" : "";
			html += "<tr><td valign='top' style='font-weight:bold;'>Cache Period:</td><td>" + request.cachePeriodSeconds + " second" + plural + "</td></tr>";
		}
		
		if (typeof script != "undefined")
		{
			html += "<tr><td valign='top' style='font-weight:bold;'>JavaScript:</td><td>" + script + "</td></tr>";
		}

		if (!maClient.preview)
			html += "<tr><td valign='top' style='font-weight:bold;'>Error:</td><td>" + message + "</td></tr>";
		
		html += "</table>";
	}
	
	// If there's no text area, the error will have to go in the media area. Set it up so that the error will
	// be scrollable if the error is taller than the area. Adjust the height to allow for the space taken up
	// by the horizontal scroller.
	var overflow = maClient.textArea ? "" : "height:" + (maClient.slide.mediaH - 12) + "px;overflow:scroll;";
	
	html = "<div style='" + overflow + "font-family:sans-serif;font-size:11px;border:solid 1px gray;padding:4px;" +
		"color:" + settings.color + ";background-color:" + settings.backgroundColor + ";'>" + html + "</div>";
		
	html = unescape(html);
	
	// Put the error in the text area if available, otherwise in the media area.
	if (maClient.textArea)
		slide.htmlText = html;
	else
		slide.media = html;
	
	maClient.setLiveData(slide);
	
	// Reset the time so that the error won't get cached.
	slide.liveDataUpdateTime = 0;
};

maClient.setSlideText = function()
{
	if (maTour.showSlideTitle)
		maClient.setText(maClient.slideTitle, 1);
	
	maClient.setText(maClient.slideText, 2);
};

maClient.changeSlide = function(mouseIsOverMarker)
{
	//console.log("changeSlide " + mouseIsOverMarker);
	
	maClient.viewId = maTour.marker.viewId;
	maClient.slide = maClient.getSlide(maClient.viewId);
	
	if (maClient.slide === null)
	{
		// This can happen when a gallery loads if the First Hotspot does not fit in the gallery.
		return;
	}
	
	// Reset the text even if the marker did not change because we always clear the text
	// after closing a popup in case it contains a macro for video that might still be playing. 
	maClient.setSlideText();
		
	if (maTour.usesPopup)
	{
		maClient.setPopupSize();
		maClient.setPopupLocation(mouseIsOverMarker);
	}
	else
	{
		maClient.setFixedLayoutAreaSizes();
	}
	
	maClient.setSlideSelector();
	maClient.setLeftMenuItem();
	
	// We call setSlideMedia even if the marker has not changed in order
	// to restore the slide image's src to the slide's image instead of 
	// the blank image that gets set when a slide is hidden.
	if (maClient.hasMedia)
		maClient.setSlideMedia();
		
	if (maClient.slide.usesLiveData)
	{
		// If the slide's text is null, show the loading graphic while waiting for live data.
		// Once we get a response from the server, we won't show the graphic again. The old text
		// will stay visible until the new live data text arrives. In most cases, there won't
		// be any new text and so by not loading the graphic again, we avoid flicker.
		if (maClient.slideText && maClient.slide.htmlText.length === 0 && maClient.slide.liveDataUpdateTime === 0)
		{
			maClient.slide.htmlText = maClient.loadingGraphic();
			maClient.slideText.style.display = "block";
			maClient.setLiveDataText(maClient.slide);
		}

		maClient.getLiveData(maClient.slide);
	}
};

maClient.loadingGraphic = function()
{
	return "<img width='15' height='15' src='" + maClient.graphics["loading"].src + "'/>";
};

maClient.getScrollingPosition = function()
{
	var position = [0, 0];

	if (typeof window.pageYOffset != 'undefined')
	{
		position = [window.pageXOffset,	window.pageYOffset];
	}

	else if (typeof document.documentElement.scrollTop != 'undefined' && (document.documentElement.scrollTop > 0 || document.documentElement.scrollLeft > 0))
	{
		position = [document.documentElement.scrollLeft, document.documentElement.scrollTop];
	}

	else if (typeof document.body.scrollTop != 'undefined')
	{
		position = [document.body.scrollLeft, document.body.scrollTop];
	}

	return position;
};

maClient.getViewportSize = function()
{
	var size = [0, 0];

	if (typeof window.innerWidth != 'undefined')
	{
		size = [window.innerWidth,	window.innerHeight];
	}
	else if (typeof document.documentElement != 'undefined'	&& typeof document.documentElement.clientWidth != 'undefined' && document.documentElement.clientWidth !== 0)
	{
		size = [document.documentElement.clientWidth, document.documentElement.clientHeight];
	}
	else
	{
		size = [document.getElementsByTagName('body')[0].clientWidth, document.getElementsByTagName('body')[0].clientHeight];
	}

	// Reduce the size in case scroll bars are present. Accurately detecting their presence and size with
	// all browsers would be very difficult. Instead we simply reduce the viewport size by a set amount.
	size[0] -= 16;
	size[1] -= 16;
	
	return size;
};

maClient.getRect = function(side, size, scrollPosition)
{
	var rect = new Object();
	var absX = parseInt(maTour.marker.absX,10);
	var absY = parseInt(maTour.marker.absY,10);
	var scrollW = scrollPosition[0];
	var scrollH = scrollPosition[1];
	var w = size[0];
	var h = size[1];
	
	switch (side)
	{
		case "top":
			rect.x = scrollW;
			rect.y = scrollH;
			rect.w = w;
			rect.h = absY - scrollH;
			break;

		case "bottom":
			rect.x = scrollW;
			rect.y = scrollH + absY;
			rect.w = w;
			rect.h = h - (absY - scrollH);
			break;
			
		case "right":
			rect.x = scrollW + absX;
			rect.y = scrollH;
			rect.w = w - (absX - scrollW);
			rect.h = h;
			break;
	
		case "left":
			rect.x = scrollW;
			rect.y = scrollH;
			rect.w = absX - scrollW;
			rect.h = h;
			break;
		
		default:
			break;
	}
	
	// If the rectangle is offscreen, the width or height will be negative. Change it to zero.
	if (rect.w < 0)
		rect.w = 0;
	if (rect.h < 0)
		rect.h = 0;
	
	var e = document.getElementById("maHotspotDropdown");
	if (e)
	{
		var ddH = 24;
		var ddW = e.offsetWidth;
		var ddPt = maClient.tagLocation(null, "maHotspotDropdown");
		var ddBottom = ddPt.y + ddH;
		var ddLeft = ddPt.x + scrollW;
		var ddRight = ddLeft + ddW;
		var ddLeftInRect = ddLeft >= rect.x && ddLeft <= rect.x + rect.w;
		var ddRightInRect = ddRight >= rect.x && ddRight <= rect.x + rect.w;
		var ddBottomInRect = ddBottom >= rect.y;
		if (ddBottomInRect && (ddLeftInRect || ddRightInRect))
		{
			rect.h = absY - ddBottom;
			rect.y = ddBottom;
		}
	}
	
	return rect;
};

maClient.clippedArea = function(popupW, popupH, areaW, areaH)
{
	var deltaW = Math.max(popupW - areaW, 0);
	var deltaH =  Math.max(popupH - areaH, 0);
	var clippedV = 0;
	var clippedH = 0;
	if (deltaW > 0)
		clippedV = deltaW * popupH;
	if (deltaH > 0)
		clippedH = deltaH * (popupW - deltaW);
	return clippedV + clippedH;
};

maClient.tagLocation = function(outerTagId, innerTagId)
{
	var e = document.getElementById(innerTagId);
	var left = 0;
	var top = 0;
	while (e && (outerTagId === null || e.id != outerTagId))
	{
		left += e.offsetLeft;
		top += e.offsetTop;
		e = e.offsetParent;
	}
	var pt = new Object();
	pt.x = left;
	pt.y = top;
	return pt;
};

maAttachEventListener(window, "load", maEndLoad);
maAttachEventListener(document, "mousemove", maCaptureMousePosition);

if (maClient.firefox)
{
    maAttachEventListener(window, "scroll", maScrollEvent);
    maClient.scrollToggle = 0;
}

if (typeof soundManager != "undefined")
{
	soundManager.onready(function(status)
	{
		maClient.soundManagerReady = status.success;
		if (typeof maOnSoundManagerReady != "undefined")
			maOnSoundManagerReady();
	});
}

function maScrollEvent()
{
	if (maTour.editMode)
		return;
	clearInterval(maClient.scrollEventIntervalId);
	maClient.scrollEventIntervalId = setTimeout("maClient.scrollStopped()", 500);
}

maClient.scrollStopped = function()
{
	// This code forces Firefox to correctly recognize a mouse-over on a map marker after
	// the page has been scrolled. There may be other, lower-impact ways to do this, but
	// for now, we resize the Flash stage by a pixel each time the user stops scrolling. 
	console.log("scrollStopped");
	var map = maClient.map;
	if (map)
		map.width = maTour.stageW + (maClient.scrollToggle === 0 ? 1 : 0);
	maClient.scrollToggle = maClient.scrollToggle === 0 ? 1 : 0;
};

function maEndLoad()
{
	console.log("maEndLoad");
	
	var maMap = document.getElementById('maMap');

	if (maMap && !maClient.mapIsFlash && !maClient.mapIsHtml5)
	{
		// The browser does not support Flash or HTML5.
		maClient.showRenderErrorMessage();
	}

	if (maTour.editMode)
	{
		maClient.ready = true;
		return;
	}
	
	maClient.usesHidablePopup = maTour.usesDynamicPopup || (maTour.usesFixedPopup && maTour.popup.location == maClient.popupLocationFixedPopup) ? 1 : 0;

	// Note that we used to test for 'createTouch' but it returned false on Android.
	// See http://modernizr.github.com/Modernizr/touch.html for touch tests on various devices.
	maClient.isTouchDevice = 'ontouchend' in document;

	// Special case handling for Chrome and Firefox on Windows.
	if (maClient.isTouchDevice && (maClient.chrome || maClient.firefox))
	{
		// Chrome and Firefox are reporting that they are running on a touch device. Is it a tablet or just a touch screen?
		if (maClient.windows && !maClient.windowsTablet)
		{
			// Chrome or Firefox is running on a Windows touch screen, but it's not a tablet. To allow the map
			// to respond to mouse moves and clicks we need to not treat the screen as a touch device.
			// The user will be able to touch the screen, but touches get translated to mouse events.
			maClient.isTouchDevice = false;
		}
	}
	
	maClient.useTouchCss = maClient.isTouchDevice || maTour.useTouchUiOnDeskop;

	if (maClient.isTouchDevice && maTour.usesPopup)
	{
		// Delays don't make sense on touch devices.
		maTour.popup.delay = 0;
	}
	
	maClient.loadGraphics();
	maClient.loadPageTable();
	maClient.loadSlideTable();
	maClient.preLoadSlideImages();
	maClient.initClientState();
	maClient.initLayouts();
	maClient.dirInit();
	maClient.ready = true;

	if (maTour.infoPage || !maMap || maClient.mapIsHtml5)
	{
		maClient.waitForClientReady();
	}
}

maClient.loadGif = function(id, name)
{
	maClient.loadImg(id, name, "gif");
};

maClient.loadImg = function(id, name, ext)
{
	var image = new Image();
	image.src = maClient.path + "00" + id + "_" + maTour.version + "." + ext;
	maClient.graphics[name] = image;
};

maClient.loadGraphics = function()
{
	maClient.graphics = new Object();
	
	maClient.loadGif(21, "loading");
	maClient.loadGif(22, "loading2");
	maClient.loadImg(29, "closeInstructionsX", "png");
	
	if (maTour.usesPopup)
	{
		maClient.loadGif(24, "closeX");
		maClient.loadGif(25, "pinUp");
		maClient.loadGif(26, "pinDown");
		maClient.loadGif(27, "pinAnimated");
		maClient.loadImg(28, "closeTouchX", "png");
		
		if (maTour.popup.arrowType !== 0)
		{
			maClient.loadGif(30, "left1");
			maClient.loadGif(31, "left2");
			maClient.loadGif(32, "right1");
			maClient.loadGif(33, "right2");
			maClient.loadGif(34, "up1");
			maClient.loadGif(35, "up2");
			maClient.loadGif(36, "down1");
			maClient.loadGif(37, "down2");
		}
	}
	
	if (maTour.hasDirectory)
	{
		maClient.loadImg(50, "sortAlpha", "png");
		maClient.loadImg(51, "sortGroup", "png");
	}
};

maClient.initClientState = function()
{
	maClient.showingTooltip = false;
	maClient.showingPopup = false;
	maClient.popupIsPinned = false;
	maClient.maTour = document.getElementById('maTour');
	maClient.slide = maClient.getSlide(maTour.selectedViewId);
};

maClient.initLayouts = function()
{
	if (maTour.usesPopup)
		maClient.initPopupLayouts();
	else
		maClient.initFixedLayouts();
};

maClient.preLoadSlideImages = function()
{
	if (maClient.slidesImageList.length > 0 && (!maClient.mobile || maTour.enableImagePreloading))
	{
		// Start preloading slide images. By default we don't do this on mobile devices unless requested.
		maClient.okayToPreload = true;
		maClient.preloadIntervalId = setInterval(maClient.loadNextSlideImage, 10);
	}
};

maClient.loadPageTable = function()
{
	// Construct a table of Page objects. Note that we use pageNumber as the table
	// index because it allows us to create a relationship from the pageNumber property
	// of the Slide objects in the Slide table. It would be more natural to use the
	// pageId as the index, but since it is a string it would occupy more space in each
	// Slide entry.
	var pages = new Object();
	for (var pageIndex = 0; pageIndex < maTour.pageTable.length; pageIndex++)
	{
		var data = maTour.pageTable[pageIndex].split(',');
		var pageId = maClient.lookupString(data[0]);
		var pageNumber = data[1];
		
		var page = new maPage(
			pageId,
			pageNumber);
			
		pages[pageNumber] = page;
	}
	maClient.pages = pages;
};

maClient.loadSlideTable = function()
{
	// Fixup string table entries.
	for (var stringIndex = 0; stringIndex < maTour.stringTable.length; stringIndex++)
	{
		// Restore HTML characters.
		var text = unescape(maTour.stringTable[stringIndex]);
		
		// Fixup slide images that are embedded in the text by replacing ':' with the
		// client path. The ':' originates in TourPageXmlWriter when it expands [image] macros.
		text = text.replace(/src=":/g, 'src="' + maClient.path);
		
		// Update the entry.
		maTour.stringTable[stringIndex] = text;
	}

	// Create a div for the sole purpose of converting HTML to plain text.
	var div = document.createElement("div");
	
	// Create a list of pointers to slides that have images that will need to be loaded.
	maClient.slidesImageList = new Array();
	maClient.slideListIndex = 0;
	
	// Construct a table of Slide objects.
	var slides = new Object();
	for (var slideIndex = 0; slideIndex < maTour.slideTable.length; slideIndex++)
	{
		var data = maTour.slideTable[slideIndex].split(',');
		var viewId = data[0];
		var pageNumber = data[1];
		var slideId = maClient.lookupString(data[2]);
		var titleText = maClient.lookupString(data[3]);
		var htmlText = maClient.lookupString(data[4]);
		var plainText = "";
		var imageSrc = maClient.lookupString(data[5]);
		var mediaW = parseInt(data[6], 10);
		var mediaH = parseInt(data[7], 10);
		var mediaType = parseInt(data[8], 10);
		var embedText = maClient.lookupString(data[9]);
		var slideW = parseInt(data[10], 10);
		var slideH = parseInt(data[11], 10);
		var usesLiveData = parseInt(data[12], 10) == 1;
		var messengerFunction = maClient.lookupString(data[13]);
		var dirPreviewImageUrl = maClient.lookupString(data[14]);
		var dirPreviewText = maClient.lookupString(data[15]);
		
		if (maTour.hasDirectory)
		{
			// Get the slide's HTML and convert it to plain text.
			// Save both in the slide: HTML for displaying the slide and plain text for searching.
			div.innerHTML = htmlText;
			plainText = maClient.getInnerText(div);
		}
		
		var slide = new maSlide(
			viewId,
			pageNumber,
			slideId,
			titleText,
			htmlText,
			plainText,
			imageSrc,
			mediaW,
			mediaH,
			mediaType,
			embedText,
			slideW,
			slideH,
			usesLiveData,
			messengerFunction,
			dirPreviewImageUrl,
			dirPreviewText);
			
		slides[viewId] = slide;
		
		if (mediaW > 0)
		{
			maClient.slidesImageList.push(slide);
		}
	}
	
	maClient.slides = slides;
};

// The 4 methods that follow provide a slides max and min size. Normally these come from the page
// values that apply to all slides on the page; however, if the dimensions for an individual slide
// have been overridden, then those dimensions are used for both the max and min size.
maClient.popupMaxH = function()
{
	return maClient.slide && maClient.slide.slideH > 0 ? maClient.slide.slideH : maTour.popup.maxH;
};

maClient.popupMaxW = function()
{
	return maClient.slide && maClient.slide.slideW > 0 ? maClient.slide.slideW : maTour.popup.maxW;
};

maClient.popupMinH = function()
{
	return maClient.slide && maClient.slide.slideH > 0 ? maClient.slide.slideH : maTour.popup.minH;
};

maClient.popupMinW = function()
{
	return maClient.slide && maClient.slide.slideW > 0 ? maClient.slide.slideW : maTour.popup.minW;
};

maClient.popupTextOnlyW = function()
{
	return maClient.slide && maClient.slide.slideW > 0 ? maClient.slide.slideW : maTour.popup.textOnlyW;
};

maClient.getInnerText = function(e)
{
	// For IE use innerText.
	var plainText = e.innerText;
	
	// For Firefox and Safari 3 use textContent.
	if (typeof plainText == 'undefined')
		plainText = e.textContent;
	
	if (typeof plainText == 'undefined' || (plainText.length === 0 && maClient.safariLt3))
	{
		// Browser Fix #5.
		// If we can't get the inner text, do very basic cleanup on the HTML.
		plainText = e.innerHTML.replace(/<[^>]*>/g,'');
	}
		
	return plainText;
};

maClient.deriveAltColor = function(hexColor)
{
	// Returns a color that is different in the low order byte by just 1.
	// The difference in color is imperceptible to the eye. 
	var hex = '0123456789abcdefe';
	var c = hexColor.substr(6,1);
	var i = hex.indexOf(c.toLowerCase());
	return hexColor.substr(0,6) + hex.substr(i + 1,1);
};

maClient.MapViewer = function()
{
	maClient.mapIsEditor = false;
	this.emitMapHtml();
};

maClient.MapEditor = function(editorUsage)
{
	maClient.mapIsEditor = true;
	this.emitMapHtml(editorUsage);
};

maClient.MapViewer.prototype = maClient.Flash.prototype;
maClient.MapEditor.prototype = maClient.Flash.prototype;

maClient.showTooltip = function(x, y, tooltip)
{
	//console.log("showTooltip '" + tooltip + "' " + x + "," + y);

	if (tooltip.length === 0)
		return;

	if (!maClient.tooltip)
	{
		maClient.tooltip = document.createElement("div");
		maClient.tooltip.setAttribute("id", "maTooltip");
		document.body.appendChild(maClient.tooltip);
	}

	maClient.tooltip.innerHTML = tooltip;
	maClient.tooltip.style.visibility = "visible";
	maClient.showingTooltip = true;

	if (maClient.slideShowIsRunning || maClient.mobile)
	{
		// When the slide show is running, we need to put the tooltip at the marker
		// location instead of the mouse location, otherwise the tooltip would follow
		// the mouse no matter where it was on the screen.
		var mapLocation = maClient.tagLocation(null, "maMap");
		x += mapLocation.x;
		y += mapLocation.y;
		var s = maClient.tooltip.style;
		s.left = x + 'px';
		s.top = y + 'px';

		if (maClient.mobile)
		{
			if (typeof maClient.hideTooltipIntervalId != "undefined")
				clearInterval(maClient.hideTooltipIntervalId);
			maClient.hideTooltipIntervalId = setTimeout("maClient.hideTooltip();", 2000);
		}
	}
	else
	{
		// Show the tooltip at the mouse location.
		maClient.moveTooltip();
	}
};

maClient.hideTooltip = function()
{
	// console.log("hideTooltip");
	
	if (maClient.showingTooltip)
	{
		maClient.tooltip.style.visibility= "hidden";
		maClient.showingTooltip = false;
	}
};

maClient.moveTooltip = function()
{
	//console.log("moveTooltip");
	
	if (maClient.showingPopup && maTour.popup.location != maClient.popupLocationFixedAlwaysVisible)
	{
		// Normally this would never happen, but it could as a result of the user called mapsalive.setMarkerSelected.
		maClient.hideTooltip();
		return;
	}
	
	if (maClient.slideShowIsRunning)
	{
		// The user moved the mouse, but we ignore this call to
		// prevent the tooltip from getting attached to the mouse cursor.
		return;
	}
		
	var x = (parseInt(maClient.mouseX, 10) + 8);
	var y = (parseInt(maClient.mouseY, 10) - 12);

	if (maClient.preview)
	{
		// Adjustment for V4 Tour Preview which is centered in the browser instead of left-aligned as it was in V3.
		// When the browser is wider than the Tour Builder, the blank space between the left edge of the preview panel
		// and the left edge of the browser must be subtracted from mouse X. A left scroll adjustment is needed too.
		var previewPanelElement = document.getElementById('PreviewPanelV3');
		x -= previewPanelElement.getBoundingClientRect().left + maClient.getScrollingPosition()[0];
	}

	var s = maClient.tooltip.style;
	s.left = x + 'px';
	s.top = y + 'px';
};

function maOnImageError()
{
	maClient.okayToPreload = true;
}

function maOnImageLoad()
{
	// This event fired for a specific slide. Mark the slide as having its image loaded.
	this.slide.imageLoaded = true;
	
	// Allow the next slide's image to load.
	maClient.okayToPreload = true;
}

maClient.loadNextSlideImage = function()
{
	// This method is called repeatedly on a timer until all images have been preloaded.
	// It loads one image and then clears a flag indicating that it's not okay to load
	// the next image until the current image has loaded. This scheme lets all slide
	// images get preloaded, but allows another image-load request to be honored if it
	// occurs before the entire preload is complete. For example, Flash will request that
	// the map image load, or a user may mouse over a marker with a popup slide before
	// the preload is complete. If that slide's image has not preloaded, a request will
	// be made via maClient.slideImageSrc to load it immediately and the request will be
	// sent to the server in between the currently preloaded image and the next. The
	// effect is like a high priority interrupt.
	//
	// Note that if preloading needed to be turned off for some reason (e.g. a user option)
	// it could be done by simply not setting the timer to call this method. In that case,
	// every image would be loaded on demand.

	if (maClient.okayToPreload)
	{
		maClient.okayToPreload = false;

		var slide = maClient.slidesImageList[maClient.slideListIndex];

		maClient.slideListIndex++;

		if (maClient.slideListIndex >= maClient.slidesImageList.length)
			clearInterval(maClient.preloadIntervalId);

		if (slide.imageSrc)
			slide.image.src = slide.imageSrc;
	}
};

// Class constructor.
function maPage(pageId, pageNumber)
{
	this.pageId = pageId;
	this.pageNumber = pageNumber;
}

// Class constructor.
function maSlide(
	viewId,
	pageNumber,
	slideId,
	title,
	htmlText,
	plainText,
	imageSrc,
	mediaW,
	mediaH,
	mediaType,
	embedText,
	slideW,
	slideH,
	usesLiveData,
	messengerFunction,
	dirPreviewImageUrl,
	dirPreviewText)
{
	this.viewId = viewId;
	this.pageNumber = pageNumber;
	this.slideId = slideId;
	this.title = title;
	this.htmlText = htmlText;
	this.plainText = plainText;

	this.imageSrc = imageSrc && imageSrc.length > 0 ? maClient.path + imageSrc : null;
	this.mediaW = mediaW;
	this.mediaH = mediaH;
	
	// Determine the amount of width that the image is not using within the available area.
	this.mediaDeltaW = Math.max(0, maTour.mediaAreaW - mediaW);
		
	this.imageLoaded = false;
	
	// Create an image object for this slide. Give the image a pointer (image.slide) back
	// to this slide and also set up load and error handlers.
	if (mediaW > 0)
	{
		this.image = new Image();
		this.image.slide = this;
		this.image.onload = maOnImageLoad;
		this.image.onerror = maOnImageError;
	}
	
	this.mediaType = mediaType;
	
	this.embedText = embedText;
	
	this.slideW = slideW;
	this.slideH = slideH;
	
	this.inSearchResults = true;
	this.searchStart = -1;
	this.searchLength = 0;
	
	this.usesLiveData = usesLiveData;
	this.messengerFunction = messengerFunction;
	this.liveDataUpdateTime = 0;
	this.liveDataCachePeriodMs = 0;
	this.liveDataRequestPending = false;
	
	this.dirPreviewImageUrl = dirPreviewImageUrl;
	this.dirPreviewText = dirPreviewText;
	
	// The media property only gets set after a Live Data response from the server.
	this.media = null;
}

maClient.runtimeImage = function(id, ext)
{
	return maClient.path + "00" + id + "_" + maTour.version + "." + ext;
};


//--// HAS POPUP

function maClosePopup()
{
	//console.log("maClosePopup");
	if (maClient.popupPanel && maTour.popup.location != maClient.popupLocationFixedAlwaysVisible)
	{
		maClient.hidePopup(0);
	}

	if (!maClient.mapIsFlash)
	{
		maClient.map.refreshMap('maClosePopup');
	}
}

function maClosePopupX()
{
	console.log("maClosePopupX");
	maClient.clickedCloseX = true;
	maClosePopup();
}

maClient.attachPopupListeners = function(e)
{
	if (maClient.isTouchDevice)
	{
		return;
	}
	
	if (maTour.popup.location == maClient.popupLocationFixedAlwaysVisible)
	{
		return;
	}

	if (maTour.popup.location == maClient.popupLocationMouse && !maTour.popup.allowMouseover)
	{
		// The popup follows the mouse and therefore the mouse can never go onto the popup.
		return;
	}

	maAttachEventListener(e, "mouseover", maPopupMouseOver);
	maAttachEventListener(e, "mouseout", maPopupMouseOut);
};

maClient.initFixedLayouts = function()
{
	maClient.mediaArea = document.getElementById("maHotspotMediaArea");
	maClient.textArea = document.getElementById("maTextArea");
	maClient.slideTitle = document.getElementById("maHotspotTitle");
	maClient.slideText = document.getElementById("maHotspotText");

	maClient.hasMedia = maClient.mediaArea !== null;
	
	if (maClient.hasMedia)
	{
		// Always clip media that is too big in fixed layouts.
		maClient.mediaArea.style.overflow = "hidden";

		if (maClient.mobile)
		{
			// Set the background to display the image loading graphic.
			maClient.mediaArea.style.backgroundImage = "url(" + maClient.graphics["loading2"].src + ")";
			maClient.mediaArea.style.backgroundRepeat = "no-repeat";
			maClient.mediaArea.style.backgroundPosition = "center center";
		}
	}
	
	if (maClient.textArea)
	{
		// Always show scroll bars for overflowing text in fixed layouts.
		maClient.textArea.style.overflow = "auto";
		
		if (maClient.textArea.style.position == "absolute")
		{
			// Remember the default left edge of the absolute positioned text area.
			// We will use this as a reference point when adjusting the position for "IT"
			// layouts where the text floats to the left to meet the media's right edge.
			var leftPx = maClient.textArea.style.left;
			maClient.textAreaLeft = parseInt(leftPx.substr(0, leftPx.length - 2),10);
		}
	}
	
	// Set the width and/or height of the text and/or media when that dimension will
	// never change regardless of content. Since this method is only called once when
	// the page loads its saves having this work be done every time a slide is shown.
	switch (maTour.layout)
	{
		case "HMMIT":
		case "HITMM":
		case "HMMTI":
		case "HTIMM":
		case "VIITT":
		case "VTTII":
			// These layouts have side-by-side text and media (media and text).
			// The height of both is always the same regardless of content.
			maClient.setFixedTextAndMediaHeight();
			break;
			
		case "HIITT":
		case "HTTII":
		case "VMMIT":
		case "VMMTI":
		case "VTIMM":
		case "VITMM":
			// These layouts have media over text or text over media.
			// The width of both is always the same regardless of content.
			maClient.setFixedTextAndMediaWidth();
			break;
			
		case "HII":
		case "HMMII":
		case "HIIMM":
		case "VMMII":
		case "VIIMM":
			// These layouts have media that always has the same width and height.
			maClient.setFixedMediaWidthAndHeight();
			break;
			
		case "HTT":
		case "HMMTT":
		case "HTTMM":
		case "VMMTT":
		case "VTTMM":
			// These layous have text that always has the same width and height.
			maClient.setFixedTextWidthAndHeight();
			break;
			
		case "HIIMT":
		case "HIITM":
		case "HMTII":
		case "HTMII":
		case "VIIMT":
		case "VIITM":
		case "VMTII":
		case "VTMII":
		case "HTTMI":
		case "HTTIM":
		case "HMITT":
		case "HIMTT":
		case "VTTMI":
		case "VTTIM":
		case "VMITT":
		case "VIMTT":
			// These layouts have both media and text that is always the same width
			// and height because either the media or the text shares the same half
			// of the layout (upper/lower half or left/right half) with the map.
			maClient.setFixedTextWidthAndHeight();
			maClient.setFixedMediaWidthAndHeight();
			break;
			
		case "HMM":
			// The map-only layout have no media or text.
			break;
			
		default:
			break;
	}
};

maClient.setFixedTextWidthAndHeight = function()
{
	maClient.textArea.style.width = maTour.textAreaW + "px";
	maClient.textArea.style.height = maTour.textAreaH + "px";
};

maClient.setFixedMediaWidthAndHeight = function()
{
	maClient.mediaArea.style.width = maTour.mediaAreaW + "px";
	maClient.mediaArea.style.height = maTour.mediaAreaH + "px";
};

maClient.setFixedTextAndMediaWidth = function()
{
	maClient.textArea.style.width = maTour.textAreaW + "px";
	maClient.mediaArea.style.width = maTour.mediaAreaW + "px";
};

maClient.setFixedTextAndMediaHeight = function()
{
	maClient.textArea.style.height = maTour.textAreaH + "px";
	maClient.mediaArea.style.height = maTour.mediaAreaH + "px";
};

maClient.initPopupLayouts = function()
{
	// Set a lower z-index for fixed slides so that the directory will appear on
	// top of them. Otherwise we want the popup to appear over the directory.
	// The directory z-index is 4000.
	var zindex = maTour.usesFixedPopup ? 3000 : 5000;

	if (maTour.usesFixedPopup)
	{
		maTour.popup.arrowType = 0;
	}

	if (maTour.popup.arrowType !== 0)
	{
		e = document.createElement("img");
		e.setAttribute("id", "maArrow");
		e.style.visibility = "hidden";
		e.style.zIndex = zindex + 1;
		e.style.position = "absolute";
		e.style.top = "0px";
		e.style.left = "0px";
		document.body.appendChild(e);
		maClient.arrowPanel = e;

		// Set up mouse listeners for the popup panel arrow.
		if (maTour.usesDynamicPopup)
		{
			maClient.attachPopupListeners(maClient.arrowPanel);
			if (maTour.popup.pinOnClick)
				maAttachEventListener(maClient.arrowPanel, "click", maPopupClickPin);
		}
	}

	maTour.popup.backgroundColorAlt = maClient.deriveAltColor(maTour.popup.backgroundColor);

	e = document.createElement("div");
	e.setAttribute("id", "maPopup");
	e.style.visibility = "hidden";
	e.style.zIndex = zindex;
	e.style.position = "absolute";
	e.style.backgroundColor = maTour.popup.backgroundColor;
	e.style.borderTop = maTour.popup.borderTop;
	e.style.borderRight = maTour.popup.borderRight;
	e.style.borderBottom = maTour.popup.borderBottom;
	e.style.borderLeft = maTour.popup.borderLeft;

	maClient.popupPanelWidth = maClient.popupMaxW();

	if (maTour.usesFixedPopup)
	{
		e.style.top = maTour.popup.top + "px";
		e.style.left = maTour.popup.left + "px";
		maClient.maTour.appendChild(e);
	}
	else
	{
		e.style.top = "0px";
		e.style.left = "0px";
		document.body.appendChild(e);
	}
	maClient.popupPanel = e;

	// Set up mouse listeners for the popup panel.
	maClient.attachPopupListeners(maClient.popupPanel);

	// Create the control panel. Note that we set the font size small to avoid a problem with
	// IE6 where the default size forces the panel to be 20px high even though it has no text.
	maClient.popupControlPanelH = 16;
	e = document.createElement("div");
	e.id = "maPopupControlPanel";
	e.style.height = maClient.popupControlPanelH + "px";
	e.style.zIndex = zindex;
	e.style.position = "absolute";
	e.style.backgroundColor = maTour.popup.borderColor;
	e.style.fontSize = "6px";
	maClient.popupControlPanel = e;
	document.body.appendChild(maClient.popupControlPanel);

	maTour.popup.borderColorAlt = maClient.deriveAltColor(maTour.popup.borderColor);

	// Create the Close X panel.
	e = document.createElement("img");
	e.onclick = maClosePopupX;
	e.style.display = "none";
	if (maClient.useTouchCss)
	{
		maClient.popupControlCloseButtonH = 44;
		e.src = maClient.graphics["closeTouchX"].src;
		e.style.width = maClient.popupControlCloseButtonH + "px";
		e.style.height = maClient.popupControlCloseButtonH + "px";
	}
	else
	{
		maClient.popupControlCloseButtonH = 12;
		e.src = maClient.graphics["closeX"].src;
		e.style.width = maClient.popupControlCloseButtonH + "px";
		e.style.height = maClient.popupControlCloseButtonH + "px";
	}
	e.style.zIndex = zindex + 2;
	e.style.position = "absolute";
	e.style.cursor = "pointer";
	document.body.appendChild(e);
	maClient.popupControlPanelCloseX = e;

	// Create the Pin panel.
	if (maTour.popup.pinOnClick)
	{
		e = document.createElement("img");
		e.id = "maPopupPin";
		e.onclick = maPopupClickPin;
		e.style.display = "none";
		e.style.width = "14px";
		e.style.height = "14px";
		e.style.zIndex = zindex + 2;
		e.style.position = "absolute";
		e.style.cursor = "pointer";
		document.body.appendChild(e);
		maClient.popupControlPanelPin = e;

		// Create the Pin message panel.
		e = document.createElement("div");
		e.id = "maPopupPinMsg";
		e.style.display = "none";
		e.style.color = "#ffffff";
		e.innerHTML = maTour.popup.pinMsg;
		e.style.fontFamily = "arial,helvetica,sans-serif";
		e.style.fontSize = "9px";
		e.style.overflow = "hidden";
		e.style.height = "14px";
		e.style.zIndex = zindex + 1;
		e.style.position = "absolute";
		e.style.cursor = "pointer";
		document.body.appendChild(e);
		maClient.popupControlPanelPinMsg = e;
	}

	// Set up mouse listeners.
	maClient.attachPopupListeners(maClient.popupControlPanel);

	if (maTour.popup.pinOnClick)
	{
		maClient.attachPopupListeners(maClient.popupControlPanelPin);
		maClient.attachPopupListeners(maClient.popupControlPanelPinMsg);
	}

	var mT = maTour.layoutMarginTop + "px ";
	var mR = maTour.layoutMarginRight + "px ";
	var mB = maTour.layoutMarginBottom + "px ";
	var mL = maTour.layoutMarginLeft + "px ";
	var mH = maTour.layoutSpacingH + "px ";
	var mV = maTour.layoutSpacingV + "px ";
	var mNone = "0px ";

	if (maClient.tourLayoutIs("HTT,HMM"))
	{
		maClient.hasMedia = false;
	}
	else
	{
		maClient.hasMedia = true;
		maClient.mediaArea = document.createElement("div");
		maClient.mediaArea.setAttribute("id", "maHotspotMediaArea");
		maClient.mediaArea.style.margin = mT + mR + mNone + mL;
		maClient.mediaArea.style.overflow = "hidden";

		if (maClient.mobile)
		{
			// Set the background to display the image loading graphic.
			maClient.mediaArea.style.backgroundImage = "url(" + maClient.graphics["loading2"].src + ")";
			maClient.mediaArea.style.backgroundRepeat = "no-repeat";
			maClient.mediaArea.style.backgroundPosition = "center center";
		}
	}

	if (!maClient.tourLayoutIs("HII,HMM"))
	{
		maClient.textArea = document.createElement("div");
		maClient.textArea.setAttribute("id", "maTextArea");

		if (maTour.showSlideTitle)
		{
			maClient.slideTitle = document.createElement("div");
			maClient.slideTitle.setAttribute("id", "maHotspotTitle");
			maClient.textArea.appendChild(maClient.slideTitle);
		}

		maClient.slideText = document.createElement("div");
		maClient.slideText.setAttribute("id", "maHotspotText");
		maClient.slideText.style.marginTop = (maTour.showSlideTitle ? 4 : 0) + "px";
		maClient.textArea.appendChild(maClient.slideText);
	}

	switch (maTour.layout)
	{
		case "HIITT":
			maClient.mediaArea.style.margin = mT + mR + mH + mL;
			maClient.textArea.style.margin = "0px " + mR + mB + mL;
			maClient.popupPanel.appendChild(maClient.mediaArea);
			maClient.popupPanel.appendChild(maClient.textArea);
			maClient.popupPanel.style.width = maClient.popupPanelWidth + "px";
			break;

		case "HTTII":
			maClient.mediaArea.style.margin = mH + mR + mB + mL;
			maClient.textArea.style.margin = mT + mR + "0px " + mL;
			maClient.popupPanel.appendChild(maClient.textArea);
			maClient.popupPanel.appendChild(maClient.mediaArea);
			maClient.popupPanel.style.width = maClient.popupPanelWidth + "px";
			break;

		case "VIITT":
		case "VTTII":
			var table = document.createElement("table");
			var tbody = document.createElement("tbody");
			table.setAttribute("cellPadding", "0");
			table.setAttribute("cellSpacing", "0");

			var row = document.createElement("tr");
			var cell1 = document.createElement("td");
			var cell2 = document.createElement("td");
			cell1.style.verticalAlign = "top";
			cell2.style.verticalAlign = "top";
			row.appendChild(cell1);
			row.appendChild(cell2);

			tbody.appendChild(row);
			table.appendChild(tbody);
			maClient.popupPanel.appendChild(table);

			if (maTour.layout == "VIITT")
			{
				maClient.textArea.style.margin = mT + mR + mB + mNone;
				cell2.style.textAlign = "left";
				maClient.mediaArea.style.margin = mT + mV + mB + mL;
				cell1.appendChild(maClient.mediaArea);
				cell2.appendChild(maClient.textArea);
			}
			else
			{
				maClient.textArea.style.margin = mT + mNone + mB + mL;
				cell1.style.textAlign = "left";
				maClient.mediaArea.style.margin = mT + mR + mB + mV;
				cell1.appendChild(maClient.textArea);
				cell2.appendChild(maClient.mediaArea);
			}
			break;

		case "HII":
			maClient.mediaArea.style.margin = mT + mR + mB + mL;
			maClient.popupPanel.appendChild(maClient.mediaArea);
			break;

		case "HTT":
			maClient.textArea.style.margin = mT + mR + mB + mL;
			maClient.popupPanel.appendChild(maClient.textArea);
			maClient.popupPanel.style.width = maClient.popupPanelWidth + "px";
			break;

		default:
			break;
	}
};

maClient.clearAfterDelay = function()
{
	if (typeof maClient.popupDelayAfterIntervalId != "undefined")
		clearInterval(maClient.popupDelayAfterIntervalId);
};

maClient.clearBeforeDelay = function()
{
	if (typeof maClient.popupDelayBeforeIntervalId != "undefined")
	{
		//console.log("clearBeforeDelay " + maClient.popupDelayBeforeIntervalId);
		clearInterval(maClient.popupDelayBeforeIntervalId);
	}
};

maClient.hidePopup = function(delay)
{
	//console.log("hidePopup " + delay);
	
	maClient.clearAfterDelay();
	
	if (delay > 0)
	{
		maClient.popupDelayAfterIntervalId = setTimeout("maClient.showPopup(false);", delay);
	}
	else
	{
		var notifyApi = maClient.popupIsPinned;
		
		maClient.showPopup(false);
		
		if (notifyApi)
		{
			if (typeof maOnHotspotChanged != "undefined")
				maOnHotspotChanged(0);
			if (typeof maOnPopupClosed != "undefined")
				maOnPopupClosed(maClient.slide.slideId);
		}
	}
};

maClient.pinPopup = function()
{
	console.log("pinPopup");
	maClient.popupIsPinned = true;
	maClient.popupSetControlPanelState(true, true);
};

maClient.getPopupLocation = function(mouseIsOverMarker)
{
	// Get the preferred popup location for this map.
	var popupLocation = maTour.popup.location;

	// Determine if the popup should appear at the mouse location. Do that when that's the
	// popup preferred location and when the request to display the popup came as a result
	// of mousing over the marker. In other cases, for example, when a hotspot is clicked in
	// the directory or when when a hotspot Id is specified on the query string, the popup
	// needs to be displayed, but the mouse is not over the marker and so we have to display
	// the popup near the marker instead of at the mouse. If the slide show is running, we
	// don't display the popup at the mouse.
	if ((popupLocation == maClient.popupLocationMouse && !mouseIsOverMarker) || maClient.slideShowIsRunning)
	{
		popupLocation = maClient.popupLocationMarkerEdge;
	}

	var markerX = parseInt(maTour.marker.x, 10);
	var markerY = parseInt(maTour.marker.y, 10);
	var markerW = parseInt(maTour.marker.w, 10);
	var markerH = parseInt(maTour.marker.h, 10);

	var scrollPosition = maClient.getScrollingPosition();
	var scrollX = scrollPosition[0];
	var scrollY = scrollPosition[1];

	var viewPortSize = maClient.getViewportSize();
	var viewPortW = viewPortSize[0];
	var viewPortH = viewPortSize[1];

	// Get the location of the map.
	var mapLocation = maClient.tagLocation(null, "maMap");

	// Determine if the marker is partially scrolled off the bottom or right side of the browser.
	// If so, adjust the size to only what is visible so that the popup won't appear offscreen.
	// Note that the view port size does not include the scroll bars making the calculation not
	// quite right, but it's close enough. It seems that detecting the presence of scroll bars in
	// all browsers is unreliable, so we live without the accuracy.

	// Get the right edge of the marker and determine if it's outside the viewport.
	var x_ = markerX + (markerW / 2);
	var offscreenX = mapLocation.x + x_ - viewPortW - scrollX;
	if (offscreenX > 0 && markerW - offscreenX > 0)
	{
		// The marker is partially off the screen, but part is still visible.
		// Reduce the marker width and adjust X to be at the center of the remaining width.
		markerW -= offscreenX;
		markerX -= offscreenX / 2;
	}

	// Get the bottom edge of the marker and determine if it's outside the viewport.
	var y_ = markerY + (markerH / 2);
	var offscreenY = mapLocation.y + y_ - viewPortH - scrollY;
	if (offscreenY > 0 && markerH - offscreenY > 0)
	{
		// The marker is partially off the screen, but part is still visible.
		// Reduce the marker height and ajust Y to be at the middle of the remaining height.
		markerH -= offscreenY;
		markerY -= offscreenY / 2;
	}

	//console.log("getPopupLocation %s %d %d %dx%d : %dx%d", mouseIsOverMarker, markerX, markerY, markerW, markerH, viewPortSize[0], viewPortSize[1]);

	/*
	var debugOutline = document.getElementById("maCustomHtmlAbsolute");
	if (debugOutline)
	{
	debugOutline.style.left = markerX - (markerW / 2) + "px";
	debugOutline.style.top = markerY - (markerH / 2) + "px";
	debugOutline.style.width = markerW + "px";
	debugOutline.style.height = markerH + "px";
	}
	*/

	// Determine how much of an offset to allow for the popup's arrow.
	var arrowSize = 0;
	if (maTour.popup.arrowType === 0)
		arrowSize = 0;
	else if (maTour.popup.arrowType == 1)
		arrowSize = 18;
	else if (maTour.popup.arrowType == 2)
		arrowSize = 36;

	var arrowOffset = arrowSize > 0 ? arrowSize - maTour.popup.borderWidth + 1 : 0;

	// Determine how much to overlap the popup onto the marker or how much of a gap to leave.
	var locationOffsetX = 0;
	var locationOffsetY = 0;

	if (popupLocation == maClient.popupLocationMarkerEdge)
	{
		if (maTour.popup.allowMouseover)
		{
			if (maTour.popup.arrowType === 0)
			{
				locationOffsetX = -2;
				locationOffsetX = -2;
			}
			else
			{
				locationOffsetX = markerW >= 16 ? -8 : -2;
				locationOffsetY = markerH >= 16 ? -8 : -2;
			}
		}
		else
		{
			locationOffsetX = 4;
			locationOffsetY = 4;
		}
	}
	if (popupLocation == maClient.popupLocationMouse)
	{
		locationOffsetX = maTour.popup.allowMouseover ? -1 : 4;
		locationOffsetY = locationOffsetX;
	}
	else if (popupLocation == maClient.popupLocationMarkerCenter)
	{
		locationOffsetX = (markerW / 2) * -1;
		locationOffsetY = (markerH / 2) * -1;
	}

	// Add any offset that the user specified (it could be negative or positive).
	locationOffsetX += maTour.popup.markerOffset;
	locationOffsetY += maTour.popup.markerOffset;

	var popupOffsetX = locationOffsetX + arrowOffset;
	var popupOffsetY = locationOffsetY + arrowOffset;

	//console.log("popupOffset %d, %d, %d, %dx%d", popupOffsetX, popupOffsetY, maTour.popup.borderWidth, markerW, markerH);

	if (popupLocation == maClient.popupLocationMouse)
	{
		maTour.marker.absX = maClient.mouseX;
		maTour.marker.absY = maClient.mouseY - 4;

		if (maClient.preview)
		{
			// Adjustment for V4 Tour Preview which is centered in the browser instead of left-aligned as it was in V3.
			// When the browser is wider than the Tour Builder, the blank space between the left edge of the preview panel
			// and the left edge of the browser must be subtracted from mouse X. A left scroll adjustment is needed too.
			var previewPanelElement = document.getElementById('PreviewPanelV3');
			maTour.marker.absX -= previewPanelElement.getBoundingClientRect().left + maClient.getScrollingPosition()[0];
		}
	}
	else
	{
		// Get the absolute location of the marker's center relative to the map. We have to calculate
		// it every time in case the user changed the window size and caused the map to move relative
		// to the upper left corner of the browser window.
		maTour.marker.absX = Number(mapLocation.x) + Number(markerX);
		maTour.marker.absY = Number(mapLocation.y) + Number(markerY);

		// Shift the offset to be relative to the center.
		popupOffsetX += markerW / 2;
		popupOffsetY += markerH / 2;
	}

	maTour.marker.rectTop = maClient.getRect("top", viewPortSize, scrollPosition);
	maTour.marker.rectBottom = maClient.getRect("bottom", viewPortSize, scrollPosition);
	maTour.marker.rectRight = maClient.getRect("right", viewPortSize, scrollPosition);
	maTour.marker.rectLeft = maClient.getRect("left", viewPortSize, scrollPosition);

	//console.log("RECT %d, %d, %d : %dx%d %dx%d %dx%d %dx%d", viewPortSize, scrollPosition[0], scrollPosition[1], maTour.marker.rectTop.w, maTour.marker.rectTop.h, maTour.marker.rectRight.w, maTour.marker.rectRight.h, maTour.marker.rectBottom.w, maTour.marker.rectBottom.h, maTour.marker.rectLeft.w, maTour.marker.rectLeft.h);

	var borders = maTour.popup.borderWidth * 2;
	var popupW = maTour.popup.actualW + borders;
	var popupH = maTour.popup.actualH + borders;
	var clippedArea = new Object();
	clippedArea.least = 2147483647;
	var sideList;

	switch (maTour.popup.bestSideSequence)
	{
		case 0: sideList = "right,left,bottom,top"; break;
		case 1: sideList = "right,left,top,bottom"; break;
		case 2: sideList = "right,bottom,top,left"; break;
		case 3: sideList = "right,bottom,left,top"; break;
		case 4: sideList = "left,right,top,bottom"; break;
		case 5: sideList = "left,right,bottom,top"; break;
		case 6: sideList = "left,top,right,bottom"; break;
		case 7: sideList = "left,top,bottom,right"; break;
		case 8: sideList = "bottom,top,left,right"; break;
		case 9: sideList = "bottom,top,right,left"; break;
		case 10: sideList = "bottom,left,top,right"; break;
		case 11: sideList = "bottom,left,right,top"; break;
		case 12: sideList = "top,bottom,left,right"; break;
		case 13: sideList = "top,bottom,right,left"; break;
		case 14: sideList = "top,right,bottom,left"; break;
		case 15: sideList = "top,right,left,bottom"; break;
		case 16: sideList = "top"; break;
		case 17: sideList = "bottom"; break;
		case 18: sideList = "right"; break;
		case 19: sideList = "left"; break;
		default: break;
	}

	// console.log(sideList);

	var bestSide = "";
	var sides = sideList.split(",");
	for (var i = 0; i < sides.length; i++)
	{
		var sideToTry = sides[i];
		bestSide = maClient.chooseBestSide(bestSide, sideToTry, clippedArea, popupW, popupH, popupOffsetX, popupOffsetY);
	}

	//console.log("==CLIPPED " + bestSide + " " + clippedArea.least + " " + clippedArea.left + " " + clippedArea.right + " " + clippedArea.top + " " + clippedArea.bottom);

	if (maClient.isTouchDevice && maTour.entirePopupVisible && clippedArea.left > 0 && clippedArea.right > 0 && clippedArea.top > 0 && clippedArea.bottom > 0)
	{
		// The popup won't fit without being clipped on one or more sides. Display it centered over the map with no arrow.
		pt = new Object();
		pt.x = mapLocation.x + ((maTour.stageW - popupW) / 2);
		pt.y = mapLocation.y + ((maTour.stageH - popupH) / 2);

		if (maTour.popup.arrowType !== 0)
		{
			maClient.arrowPanel.style.display = "none";
		}
	}
	else
	{
		switch (bestSide)
		{
			case "left":
			case "right":
				pt = maClient.positionPopupLeftOrRight(bestSide, popupOffsetX, arrowSize);
				break;

			case "top":
			case "bottom":
				pt = maClient.positionPopupUpOrDown(bestSide, popupOffsetY, arrowSize);
				break;

			default:
				break;
		}
	}

	return pt;
};

maClient.chooseBestSide = function(bestSide, sideToTry, clippedArea, popupW, popupH, popupOffsetX, popupOffsetY)
{
	// console.log("chooseBestSide %s vs %s : %s", bestSide, sideToTry, maClient.hotspotOnQueryString);
	
	switch (sideToTry)
	{
		case "top":
			clippedArea.top = maClient.clippedArea(popupW, popupH, maTour.marker.rectTop.w, maTour.marker.rectTop.h - popupOffsetY);
			if (clippedArea.top < clippedArea.least)
			{
				if (clippedArea.top === 0 || !document.getElementById("maHotspotDropdown"))
				{
					if (!maClient.hotspotOnQueryString || clippedArea.top === 0)
					{
						clippedArea.least = clippedArea.top;
						bestSide = "top";
					}
				}
			}
			break;
			
		case "right":
			clippedArea.right = maClient.clippedArea(popupW, popupH, (maTour.marker.rectRight.w - popupOffsetX), maTour.marker.rectRight.h);
			if (clippedArea.right < clippedArea.least)
			{
				if (!maClient.hotspotOnQueryString || clippedArea.right === 0)
				{
					clippedArea.least = clippedArea.right;
					bestSide = "right";
				}
			}
			break;
		
		case "bottom":
			clippedArea.bottom = maClient.clippedArea(popupW, popupH, maTour.marker.rectBottom.w, maTour.marker.rectBottom.h - popupOffsetY);
			if (clippedArea.bottom < clippedArea.least)
			{
				clippedArea.least = clippedArea.bottom;
				bestSide = "bottom";
			}
			break;
			
		case "left":
			clippedArea.left = maClient.clippedArea(popupW, popupH, (maTour.marker.rectLeft.w - popupOffsetX), maTour.marker.rectLeft.h);
			if (clippedArea.left < clippedArea.least)
			{
				clippedArea.least = clippedArea.left;
				bestSide = "left";
			}
			break;
			
		default:
			break;
	}
	
	return bestSide;
};

maClient.popupAfterDelay = function()
{
	var delay = 0;
	if (maTour.popup.delayType == 2 && maTour.popup.delay !== 0)
	{
		// Delay After (type 2) is set with a non-zero delay. Don't hide the popup until the delay expires.
		delay = maTour.popup.delay;
	}
	return delay;
};

function maPopupMouseOut(event)
{
	console.log("maPopupMouseOut %d %d", maClient.popupIsPinned, maTour.popup.allowMouseover);

	// The method is called on a mouseout event. If the element being moused out of
	// is related to the popup panel, we ignore it. Otherwise we close the panel.
	var related = typeof event.relatedTarget != 'undefined' ? event.relatedTarget : event.toElement;
	
	// Some related elements have no Id and are nested inside a related element.
	// Walk up the node chain until we find an element with an Id or hit the top.
	var elementHasNoId = maClient.elementHasNoId(related);
	while (related && maClient.elementHasNoId(related))
	{
		related = related.parentNode;
		elementHasNoId = maClient.elementHasNoId(related);
	}
	
	//console.log("maPopupMouseOut" + (related && related.id ? ">>>" + related.id : "???"));

	// Hide the popup unless there is a related element. Note that the "VideoPlayback" related item
	// was added specifically to deal with the case where a google video is causing a popup to close
	// because we get a mouseout on the embed tag. It appears that the id on google.com videos (as
	// opposed to youtube videos) is always "VideoPlayback" so hopefully it will handle all of them
	// as long as the user does not change the id when they paste the code into MapsAlive.
	if (!related || elementHasNoId || !maClient.stringStartsWith(related.id, "maArrow,maTextArea,maHotspotTitle,maHotspotText,maHotspotImage,maHotspotMediaArea,maPopup,maPopupControlPanel,maPopupPin,maPopupPinMsg,VideoPlayback"))
	{
		if (maClient.popupIsPinned)
		{
			console.log("== IGNORE pinned");
			maClient.mouseIsOverPopup = false;
			maClient.setFlashPopupState();
			return;
		}
		
		// The element that was moused onto is not related to the popup.
		maClient.mouseIsOverPopup = false;
		maClient.setFlashPopupState();
		maClient.hidePopup(maClient.popupAfterDelay());
	}
	else
	{
		console.log("== IGNORE related");
	}
}

function maPopupMouseOver(event)
{
	console.log("maPopupMouseOver " + maTour.popup.location);

	if (maClient.mouseIsOverPopup || maClient.isTouchDevice)
	{
		console.log("== IGNORE " + maClient.mouseIsOverPopup + " " + maClient.isTouchDevice);
		return;
	}

	maClient.mouseIsOverPopup = true;
	maClient.setFlashPopupState();
	
	if (maClient.popupIsPinned)
	{
		console.log("== IGNORE popupIsPinned");
		return;
	}
	
	if (maTour.popup.allowMouseover)
	{
		maClient.clearAfterDelay();
	}
	else
	{
		maClient.hidePopup(0);
	}
}

function maPopupClickPin(event)
{
	console.log("maPopupClickPin " + maClient.popupIsPinned + " " + event.pageX + " " + event.pageY);
	
	if (maClient.popupIsPinned)
	{
		// Clicking the pin is the same as clicking the close X.
		maClosePopupX();
	}
	else
	{
		maClient.pinPopup();
	}
}

maClient.stringStartsWith = function(s, list)
{
	var item = list.split(",");
	for (var i = 0; i < item.length; i++)
	{
		if (s.indexOf(item[i]) === 0)
			return true;
	}
	return false;
};

maClient.positionPopupLeftOrRight = function(side, popupOffsetX, arrowSize)
{
	//console.log("positionPopupLeftOrRight %s %s %s", side, popupOffsetX, maTour.marker.absX);

	var arrow = side == "right" ? "left" : "right";
	var arrow1 = arrow + "1";
	var arrow2 = arrow + "2";
	var arrowType = arrow1;
	var border = parseInt(maTour.popup.borderWidth, 10);
	var middle = Math.floor(maTour.popup.actualH / 2);
	var yOffset = middle + border;
	var borderTweak = border > 0 ? 1 : 0;
	var topH = side == "right" ? maTour.marker.rectRight.h : maTour.marker.rectLeft.h;

	if (middle < arrowSize)
	{
		yOffset = border - borderTweak;
	}
	else if (yOffset > maTour.marker.rectTop.h || maTour.popup.actualH > topH)
	{
		yOffset -= yOffset - topH + 1;
		if (yOffset < border - 1)
			yOffset = border - (border > 0 ? 1 : 0);
		if (yOffset > maTour.popup.actualH - arrowSize)
			yOffset = maTour.popup.actualH - arrowSize;
	}
	else if (yOffset > maTour.marker.rectBottom.h)
	{
		arrowType = arrow2;
		yOffset += yOffset - maTour.marker.rectBottom.h + 1;
		if (yOffset > maTour.popup.actualH)
			yOffset = maTour.popup.actualH + border + (border > 0 ? 1 : 0);
	}

	var popupX;
	var popupY;
	var arrowX;
	var arrowY;

	popupY = maTour.marker.absY - yOffset;

	if (side == "right")
	{
		popupX = maTour.marker.absX + popupOffsetX;
		arrowX = popupX - arrowSize + border;
		if (popupY < maTour.marker.rectRight.y)
			popupY = maTour.marker.rectRight.y;
	}
	else
	{
		popupX = maTour.marker.absX - (maTour.popup.actualW + (border * 2) + popupOffsetX);
		arrowX = popupX + maTour.popup.actualW + border;
		if (popupY < maTour.marker.rectLeft.y)
			popupY = maTour.marker.rectLeft.y;
	}

	// Make sure the top of the popup and its close button are below the top edge of the browser window.
	var minPopupY = maClient.popupControlPanelH;
	if (maClient.useTouchCss)
	{
		minPopupY += maClient.popupControlCloseButtonH / 2;
	}
	if (popupY < minPopupY && (maClient.dirEntryClicked || maTour.popup.pinOnClick || maClient.isTouchDevice))
	{
		popupY = minPopupY;
	}

	arrowY = maTour.marker.absY - (arrowType == arrow1 ? 0 : arrowSize);

	maClient.setArrowPanel(maClient.arrowPanel, arrowType, arrowSize, arrowX, arrowY);

	var pt = new Object();
	pt.x = popupX;
	pt.y = popupY;
	return pt;
};

maClient.positionPopupUpOrDown = function(side, popupOffsetY, arrowSize)
{
	//console.log("positionPopupUpOrDown %s %s %s", side, popupOffsetY, arrowSize);
	
	var arrow = side == "bottom" ? "up" : "down";
	var arrow1 = arrow + "1";
	var arrow2 = arrow + "2";
	var arrowType = arrow1;           
	var border = parseInt(maTour.popup.borderWidth, 10);
	var middle = Math.floor(maTour.popup.actualW / 2);
	var xOffset = middle + border;
	var borderTweak = border > 0 ? 1 : 0;

	if (middle < arrowSize)
	{
		xOffset = border - borderTweak;
	}
	else if (xOffset > maTour.marker.rectLeft.w || maTour.popup.actualW > maTour.marker.rectBottom.w)
	{
		xOffset -= xOffset - maTour.marker.rectLeft.w + 1;
		if (xOffset < border - 1)
			xOffset = border - borderTweak;
	}
	else if (xOffset > maTour.marker.rectRight.w)
	{
		arrowType = arrow2;         
		xOffset += xOffset - maTour.marker.rectRight.w + 1;
		if (xOffset > maTour.popup.actualW)
			xOffset = maTour.popup.actualW + border + borderTweak;
	}

	var popupX;
	var popupY;
	var arrowX;
	var arrowY;

	if (side == "bottom")
	{
		popupY = maTour.marker.absY + popupOffsetY + (maTour.popup.allowMouseover ? -4 : 4);
		arrowY = popupY + border - arrowSize;
		if ((maTour.popup.pinOnClick || maClient.pinNextPopup) && !maClient.isTouchDevice)
		{
			popupY += maClient.popupControlPanelH;
			arrowY += 1;
		}
	}
	else
	{
		popupY = maTour.marker.absY - (maTour.popup.actualH + (border * 2) + popupOffsetY);

		if (popupY < maClient.popupControlPanelH && (maClient.dirEntryClicked || maTour.popup.pinOnClick) && maTour.popup.allowMouseover)
		{
			// The top of the popup would be above the browser top. Move it down so that the top shows.
			popupY = maClient.popupControlPanelH;
		}
		
		arrowY = popupY + maTour.popup.actualH + border;
	}

	popupX = maTour.marker.absX - xOffset;
	arrowX = maTour.marker.absX - (arrowType == arrow1 ? 0 : arrowSize);
	maClient.setArrowPanel(maClient.arrowPanel, arrowType, arrowSize, arrowX, arrowY);

	var pt = new Object();
	pt.x = popupX;
	pt.y = popupY;
	return pt;
};

maClient.setArrowPanel = function(panel, type, size, x, y)
{
	if (maTour.popup.arrowType === 0)
		return;

	var s = panel.style;
	s.display = "none";
	s.width = size + "px";
	s.height = size + "px";
	panel.src = maClient.graphics[type].src;
	s.left = x + 'px';
	s.top = y + 'px';
	s.display = "block";
};

maClient.popupSetControlPanelState = function(show, pin)
{
	//console.log("popupSetControlPanelState " + show + " " + pin);

	if (pin && maTour.popup.location == maClient.popupLocationFixedAlwaysVisible)
	{
		// Ignore pin requests for fixed popups that are always visible. It's cleaner
		// to do this than to detect this case is all the places that request pinning.
		pin = false;
		maClient.pinNextPopup = false;
	}

	var pinnable = pin || maClient.pinNextPopup || maTour.popup.pinOnClick || maClient.popupIsPinned;

	maClient.popupIsPinned = show && pin && !maClient.isTouchDevice;
	maClient.pinNextPopup = false || maClient.isTouchDevice;

	var showAndPinnable = show && pinnable && !maClient.isTouchDevice;
	maClient.popupControlPanel.style.width = maTour.popup.actualW + (maTour.popup.borderWidth * 2) + "px";
	maClient.popupControlPanel.style.display = showAndPinnable ? "block" : "none";

	maClient.popupPanel.className = showAndPinnable ? "maPopupPinned" : "maPopup";
	
	if (maTour.popup.pinOnClick)
	{
		if (showAndPinnable)
		{
			var pinUp = maTour.popup.allowMouseover || !maTour.popup.pinOnClick ? "pinUp" : "pinAnimated";
			maClient.popupControlPanelPin.src = maClient.graphics[pin ? "pinDown" : pinUp].src;
		}

		maClient.popupControlPanelPin.style.display = showAndPinnable ? "block" : "none";
		maClient.popupControlPanelPinMsg.style.display = show && pinnable && !pin && !maTour.popup.allowMouseover && maTour.popup.pinOnClick ? "block" : "none";
	}

	maClient.popupControlPanelCloseX.style.display = show && pinnable && pin ? "block" : "none";

	maClient.setFlashPopupState();
};

maClient.setFlashPopupState = function()
{
	if (!maClient.mapLoaded)
		return;

	//console.log("setFlashPopupState " + maTour.marker.viewId + " " + maClient.showingPopup + " " + maClient.popupIsPinned + " " + maClient.mouseIsOverPopup);
	maClient.map.setPopupState(maTour.marker.viewId, maClient.showingPopup, maClient.popupIsPinned, maClient.mouseIsOverPopup);
};

maClient.setPopupLocation = function(mouseIsOverMarker)
{
	//console.log("setPopupLocation %s", mouseIsOverMarker);

	var x;
	var y;
	var l;
	var s;

	if (maTour.usesDynamicPopup)
	{
		l = maClient.getPopupLocation(mouseIsOverMarker);
		x = l.x;
		y = l.y;
		s = maClient.popupPanel.style;
		s.top = y + 'px';
		s.left = x + 'px';
	}
	else if (maTour.usesFixedPopup)
	{
		l = maClient.tagLocation(null, "maPopup");
		x = l.x;
		y = l.y;
	}

	if (maTour.usesPopup)
	{
		var bw = maTour.popup.borderWidth;
		var top = maClient.popupControlPanelH - bw - 1;

		s = maClient.popupControlPanel.style;
		s.top = (y - top) + 'px';
		s.left = x + 'px';

		s = maClient.popupControlPanelCloseX.style;

		if (maClient.useTouchCss)
		{
			var buttonOffset = maClient.popupControlCloseButtonH / 2;
			s.top = (y - top - buttonOffset + maClient.popupControlPanelH) + 'px';
			s.left = (x + maTour.popup.actualW - buttonOffset + bw) + 'px';
		}
		else
		{
			s.top = (y - top + 2) + 'px';
			s.left = (x + maTour.popup.actualW - 14 + bw) + 'px';
		}

		if (maTour.popup.pinOnClick)
		{
			s = maClient.popupControlPanelPin.style;
			s.top = (y - top) + 'px';
			s.left = (x + bw) + 'px';

			s = maClient.popupControlPanelPinMsg.style;
			s.top = (y - top + 2) + 'px';
			s.left = (x + bw + 16) + 'px';
		}
	}
};

maClient.setPopupSize = function()
{
	var marginsV = parseInt(maTour.layoutMarginLeft,10) + parseInt(maTour.layoutMarginRight,10);
	var marginsH = parseInt(maTour.layoutMarginTop,10) + parseInt(maTour.layoutMarginBottom,10);
	var contentHeight;
	var popupWidth;
	var textAreaHeight;
	var textHeight;
	var textWidth;
	var isTextOnly;
	
	if (maClient.hasMedia)
		maClient.mediaArea.style.display = maClient.slide.mediaW ? "block" : "none";

	switch (maTour.layout)
	{
		case "HII":
			// Media only layouts are simple: the size is the media dimensions plus margins all around. 
			maClient.setActualPopupWidth(maClient.slide.mediaW + marginsV);
			maClient.setActualPopupHeight(maClient.slide.mediaH + marginsH);
			maClient.mediaArea.style.width = maClient.slide.mediaW + "px";
			maClient.mediaArea.style.height = maClient.slide.mediaH + "px";
			break;

		case "HTT":
		case "HIITT":
		case "HTTII":
			var nonTextHeight;
			
			// Determine if this slide only contains text.
			isTextOnly = maTour.layout == "HTT" || maClient.slide.mediaW === 0;
			
			// Normally we set the width to the media width plus left and right margins, but if
			// the slide is text-only, we use the width the user has specified for text-only slides.
			popupWidth = isTextOnly ? maClient.popupTextOnlyW() : maClient.slide.mediaW + marginsV;
			maClient.setActualPopupWidth(popupWidth);
			
			// Now that we have picked a width for the popup, we can use it for the text. Once text has a
			// width, we can get the DOM to tell us its height at that width. Note that the slide title
			// is part of the text.
			textWidth = maTour.popup.actualW - marginsV;
			textHeight = maClient.getTextHeight(textWidth);
			
			// Determine if spacing is needed between the media and text. It's needed when both exist.
			var spacingH = maClient.slide.mediaH > 0 && textHeight > 0 ? maTour.layoutSpacingH : 0;
			var marginH = spacingH + "px";
			
			if (maTour.layout == "HTTII")
			{
				if (maClient.slide.mediaH > 0)
				{
					// Put the spacing above the image.
					maClient.mediaArea.style.marginTop = marginH;
				}
			}
			else if (maTour.layout == "HIITT")
			{
				if (maClient.slide.mediaH > 0)
				{
					// Put the spacing below the image.
					maClient.mediaArea.style.marginBottom = marginH;
				}
				
				// If there's no media, add a margin to the top of the text area.
				// that would normally be the top margin of the media area.
				maClient.textArea.style.marginTop = (maClient.slide.mediaH > 0 ? 0 : maTour.layoutMarginTop) + "px";
			}
		
			nonTextHeight = maClient.slide.mediaH + spacingH;
			contentHeight =	nonTextHeight + textHeight + marginsH;
			textAreaHeight = Math.max(0,  maClient.popupMaxH() - nonTextHeight - marginsH);
			break;
		
		case "VIITT":
		case "VTTII":
			// Determine if this slide only contains text.
			isTextOnly = maClient.slide.mediaW === 0;
			
			// Normally we set the width to the max width minus any space not being used by the media.
			// We do it this way because in a vertical popup layout the text width does grow and shrink
			// as does the text height in a horizontal popup layout. In other words, in the vertical layout,
			// the text width is constant and the width of the popup various according to the media width.
			popupWidth = isTextOnly ? maClient.popupTextOnlyW() : maClient.popupMaxW() - maClient.slide.mediaDeltaW;
			maClient.setActualPopupWidth(popupWidth);
			
			if (maTour.layout == "VIITT")
			{
				// If there's no media, add a margin to the left of the text area
				// that would normally be the left margin of the mdeia area.
				var marginL = maClient.slide.mediaW > 0 ? 0 : maTour.layoutMarginLeft;
				maClient.textArea.style.marginLeft = marginL + "px";
			}
			
			// Determine if spacing is needed between the media and text.
			var spacingV = maClient.slide.mediaW > 0 ? parseInt(maTour.layoutSpacingV,10) : 0;
			
			// Calculate the text width and then from that, let the DOM tell us the text height.
			textWidth = maTour.popup.actualW - maClient.slide.mediaW - spacingV - marginsV;
			textHeight = maClient.getTextHeight(textWidth);
			
			contentHeight = Math.max(maClient.slide.mediaH, textHeight) + marginsH;
			textAreaHeight = Math.min(maTour.textAreaH, maClient.popupMaxH() - marginsH);
			break;

		default:
			break;
	}
	
	if (maTour.layout != "HII")
	{
		// Determine if the content fills the popup height. If not, there is extra height.	
		var extraHeight = maClient.popupMaxH() - contentHeight;
			
		if (extraHeight >= 0)
		{
			// Reduce the height of the popup to fit the content and don't show scroll bars in the text area.
			maClient.setActualPopupHeight(Math.max(maClient.popupMaxH() - extraHeight, maClient.popupMinH()));
			maClient.textArea.style.height = textHeight + "px";
			maClient.textArea.style.overflow = "hidden";
		}
		else
		{
			// Set the height of the popup to the max. Show scroll bars in the text area if the user can get
			// to them because the popup persists or is pinnaable, otherwise hide them.
			maClient.setActualPopupHeight(maClient.popupMaxH());
			maClient.textArea.style.height = textAreaHeight + "px";
			maClient.textArea.style.overflow = maTour.popup.allowMouseover || maTour.popup.pinOnClick ? "auto" : "hidden";
		}
		
		if (maClient.hasMedia)
		{
			maClient.mediaArea.style.width = maClient.slide.mediaW + "px";
			maClient.mediaArea.style.height = maClient.slide.mediaH + "px";
		}
		
		if (maClient.popupControlPanelPinMsg && maTour.popup.actualW > 32)
		{
			// Only show the pin message if the popup is wide enough.
			maClient.popupControlPanelPinMsg.style.width = (maTour.popup.actualW - 32) + "px";
		}
	}
};

maClient.showPopup = function(show)
{
	//console.log("showPopup " + show);

	if (show)
	{
		maClient.popupStartRefresh();
	}
	else
	{
		if (maClient.clickedCloseX)
		{
			// The user clicked the X to close the popup. The flag tells us to
			// close the popup and not test for the marker being under the mouse.
			maClient.clickedCloseX = false;
		}
		//else if (!maClient.mouseDragStarted && maTour.popup.allowMouseover)
		//{
		// To prevent the popup from flickering off and then back on, ignore the request to hide
		// a popup if the user moused off of the popup and back onto the marker. Unfortunately we
		// cannot reliably determine if the mouse is over its marker. If the popup extended beyond
		// the bounds of the map and the user moused off the popup out of map bounds, Flash will
		// not get a mouse event and if asked, will report that the mouse is still over the marker
		// since that's the last place it was before the mouse moved onto the popup. And there's no
		// way to tell in JavaScript if the mouse is over the marker. We know the marker's bounding
		// rectangle, but if the marker is a polygon, we can't tell if the mouse is within the shape.
		// So unless we come up with a solution, we live with some flicker.
		//}

		maClient.popupStopRefresh();

		var dropdownList = document.getElementById('maHotspotDropdown');
		if (dropdownList)
			dropdownList.selectedIndex = 0;

		var slideTitlesInMenu = document.getElementById('maHotspotNamesInMenu');
		if (slideTitlesInMenu)
		{
			var oldSelectedViewId = document.getElementById("ma" + maTour.selectedViewId);
			if (oldSelectedViewId)
				oldSelectedViewId.className = '';
			maTour.selectedViewId = '';
		}

		maClient.mouseIsOverPopup = false;

		// Deal with the case where a tooltip was displayed instead of a popup that had no content.
		maClient.hideTooltip();
	}

	var visibility = show ? "visible" : "hidden";

	maClient.popupPanel.style.visibility = visibility;

	if (maTour.popup.arrowType !== 0)
		maClient.arrowPanel.style.visibility = visibility;

	if (maClient.mediaArea)
	{
		maClient.mediaArea.style.visibility = visibility;

		if (!show)
		{
			// Replace the image with nothing and make it invisible. The idea
			// is that if the next image to be displayed is not loaded immediately, 
			// nothing will show in the meantime instead of the previous image.
			maClient.mediaArea.innerHTML = "";
		}
	}

	if (maClient.slideText && !show)
	{
		// Erase the text in case it contains a macro for video that might still be playing.
		// If we don't do this, you'll still hear the audio on some browsers even though
		// the popup is no longer visible.
		maClient.slideText.innerHTML = "";
	}

	var pin = maClient.pinNextPopup || maClient.popupIsPinned;

	maClient.showingPopup = show;
	maClient.popupSetControlPanelState(show, pin);
};

maClient.popupStartRefresh = function()
{
	if (maClient.enableRefresh)
	{
		// During the early days of MapsAlive when the current browser versions were IE 6, Netscape, FF 1, and Safari 2,
		// we had problems, especially on the Mac, where a popup would display incorrectly when it appeared on top of the
		// map. Parts of the popup would become transparent as though it were disintegrating. The problem seemed to be
		// a conflict between Flash and JavaScript and how they refreshed their screen regions. To prevent it, we added
		// refresh logic that would force JavaScript to redraw the popup repeatedly. The newer browsers (IE7+, FF 3+,
		// Safari 3+) no longer exhibit this behavior and so this logic is disabled, but we are keeping it for now.
		maClient.refreshCycle = maClient.macintosh ? 60 : 5;
		maClient.popupStopRefresh();
		maClient.refreshIntervalId = setInterval(maClient.popupRefresh, maClient.macintosh ? 50 : 200);
	}
};

maClient.popupStopRefresh = function()
{
	if (typeof maClient.refreshIntervalId != "undefined")
		clearInterval(maClient.refreshIntervalId);
};

maClient.popupRefresh = function()
{
	console.log("popupRefresh");
	maClient.refreshCycle--;
	maClient.popupPanel.style.backgroundColor = maClient.refreshCycle % 2 === 0 ? maTour.popup.backgroundColor : maTour.popup.backgroundColorAlt;
	
	if (maClient.macintosh && maTour.popup.arrowType !== 0)
		maClient.arrowPanel.style.color = maClient.refreshCycle % 2 === 0 ? maTour.popup.backgroundColor : maTour.popup.backgroundColorAlt;
	
	if (maClient.macFirefox && maClient.popupIsPinned)
		maClient.popupControlPanel.style.backgroundColor = maClient.refreshCycle % 2 === 0 ? maTour.popup.borderColor : maTour.popup.borderColorAlt;
	
	if (maClient.refreshCycle === 0)
	{
		maClient.popupStopRefresh();
		if (maClient.macintosh)
			maClient.refreshIntervalId = setInterval(maClient.popupRefresh, 500);
	}
};

//--// HAS DIRECTORY

function maDirClose(event)
{
	if (!maClient.okToCloseDirectoryPanel)
	{
		maClient.okToCloseDirectoryPanel = true;
		return;
	}

	// The method is called on a mouseout event. If the element being moused out to is related
	// to the directory panel, we ignore it. Otherwise we close the panel.
	var related = typeof event.relatedTarget != 'undefined' ? event.relatedTarget : event.toElement;
	
	// Some related elements have no Id and are nested inside a related element.
	// Walk up the node chain until we find an element with an Id or hit the top.
	var elementHasNoId = maClient.elementHasNoId(related);
	while (related && maClient.elementHasNoId(related))
	{
		related = related.parentNode;
		elementHasNoId = maClient.elementHasNoId(related);
	}
		
	// Hide the directory unless there is a related element that begins with "maDir".
	if (!related || elementHasNoId || related.id.indexOf("maDir") !== 0)
	{
		maClient.dirShow(false);
	}
}

maClient.dirCreateEntry = function(parentLevel, slide, isDataSheet, depth)
{
	// Create the entry's <div>.
	var div = document.createElement("div");
	div.id = "maDirEntry" + slide.viewId + "_" + parentLevel.id;
	if (isDataSheet)
		div.className = depth === 0 ? "maDirLevel1" : "maDirLevel2";
	else
		div.className = maClient.useTouchCss ? "maDirEntryTouch" : "maDirEntry";

	if ((maTour.dir.showImagePreview || maTour.dir.showTextPreview) && !maClient.isTouchDevice)
	{
		div.onmouseover = new Function("maClient.dirShowPreview(" + slide.viewId + ");");
		div.onmouseout = new Function("maClient.dirHidePreview();");
	}
	
	// Create the entry's <a> inside the <div>
	var title = document.createElement("a");
	title.href = "javascript:maClient.dirShowEntry(" + slide.viewId + ");";
	title.innerHTML = slide.title;
	div.appendChild(title);
	
	if (maClient.macFirefox)
	{
		// Browser Fix #2.
		// Leave some room between the div's right edge and the scroll bar so that
		// we'll have an area the user can mouse into where the preview will disappear
		// and the scroll bars will reappear. See Browser Fix #1.
		div.style.width = maTour.dir.contentWidth - 40 + "px";
	}
	
	// Attach the div to it parent div.
	parentLevel.contentDiv.appendChild(div);
	
	// Create a new entry object and make it a child of its parent level object.
	var entry = new maDirEntry(parentLevel, div, slide);
	parentLevel.content.push(entry);
};

maClient.dirCreateLevel = function(parentLevel, titleId, depth, levelId, collapseState)
{
	// Create the level's outer <div>.
	var outerDiv = document.createElement("div");
	outerDiv.id = "maDirLevel" + levelId;
	outerDiv.className = (maClient.useTouchCss ? "maDirLevelTouch" : "maDirLevel") + depth;
	
	// Create the level's <a> inside the outer <div>.
	var title = document.createElement("a");
	title.href = "javascript:maClient.dirToggleLevelExpansion(" + levelId + "," + depth + ");";
	title.innerHTML = maClient.lookupString(titleId);
	outerDiv.appendChild(title);
	
	// Create a <span> to contain the level's count.
	var countSpan = document.createElement("span");
	countSpan.id = "maDirLevelCount" + levelId;
	countSpan.className = maClient.useTouchCss ? "maDirLevelCountTouch" : "maDirLevelCount";
	outerDiv.appendChild(countSpan);
	
	// Create the level's inner div that will contain sub-levels or entries.
	var contentDiv = document.createElement("div");
	contentDiv.id = "maDirLevel" + levelId + "Content";
	contentDiv.className = "maDirLevelContent";
	
	// Determine if this level should be expanded or collapsed. The default is that
	// level 1 is collapsed and level 2 is expanded. However, if a level's Id was
	// passed as part of the collapse state of a calling page, then use the opposite state.
	var collapse;
	if (depth == 1 && maTour.dir.openExpanded && collapseState.length === 0)
	{
		// We are creating the directory for the first time and open-expanded was requested.
		collapse = false;
	}
	else
	{
		collapse = depth == 1;
		if (collapseState.length > 0)
		{
			// Start at index 1 because index 0 is the mode (sort by alpha or sort by group).
			for (var i = 1; i < collapseState.length; i++)
			{
				if (collapseState[i] == levelId)
				{
					// Get the opposite state.
					collapse = !collapse;
					break;
				}
			}
		}
	}
	contentDiv.style.display = collapse ? "none" : "block";
	outerDiv.appendChild(contentDiv);
	
	// Attach the outer div to its parent div.
	parentLevel.contentDiv.appendChild(outerDiv);
	
	// Create a new level object and make it a child of its parent level object.
	var level = new maDirLevel(parentLevel, levelId, depth, outerDiv, contentDiv, countSpan);
	parentLevel.content.push(level);
	
	return level;
};

maClient.dirCreateOrderAlpha = function()
{
	maClient.dirBodyAlphabetic = new maDirLevel(null, 0, 0, null, document.createElement("div"), null);
	
	console.log("dirCreateOrderAlpha");
	
	// Create an array of slides so that we can sort the maClient.slides object which is not an array.
	var hotspots = new Array();	
	for (var viewId in maClient.slides)
	{
		var slide = maClient.getSlide(viewId);
		if (slide.title.length >= 1 && slide.title.substr(0, 1) != "_")
		{
			hotspots.push(slide);
		}
	}
	
	// Sort the slides alphabetically by title. Note that we used to count on maClient.slides having
	// the titles already be in order because that's how they come from the tour builder. But then we
	// discovered that the Chrome browser stored them in viewId order internally which meant that
	// titles were not necessarily in alpha order. So now we go to the extra trouble to sort here.
	hotspots.sort(maClient.sortHotspotsByTitle);

	for (var hotspot in hotspots)
	{
		maClient.dirCreateEntry(maClient.dirBodyAlphabetic, hotspots[hotspot], false, 0);
	}
};

maClient.sortHotspotsByTitle = function(a, b)
{
	// This function is used to sort slides by their title when passed to Array.sort().
	var titleA = a.title.toLowerCase();
	var titleB = b.title.toLowerCase();
	if (titleA < titleB)
		return -1;
	if (titleA > titleB)
		return 1;
	return 0;
};

maClient.dirCreateOrderGroup = function(collapseState)
{
	maClient.dirBodyLevels = new maDirLevel(null, 0, 0, null, document.createElement("div"), null);
	
	// The first element of the table contains the depth of slide entries.
	// The remaining elements are kind/id pairs.
	// A negative depth means the entry is a Data Sheet.
	var entryDepth = maTour.dir.table[0];
	
	var levelId = 0;
	var parentLevel = new Array();
	parentLevel[0] = maClient.dirBodyLevels;
	var i = 1;
	
	while (i < maTour.dir.table.length)
	{
		var depth = maTour.dir.table[i];
		var isDataSheet = depth < 0;
		
		if (depth == entryDepth || isDataSheet)
		{
			var viewId = maTour.dir.table[i + 1];
			var slide = maClient.getSlide(viewId);
			// Slide titles that start with "_" are excluded from the directory.
			if (slide.title.length >= 1 && slide.title.substr(0, 1) != "_")
			{
				if (isDataSheet)
					 depth *= -1;
				depth = depth - 1;
				maClient.dirCreateEntry(parentLevel[depth], slide, isDataSheet, depth);
			}
		}
		else
		{
			levelId++;
			var titleId = maTour.dir.table[i + 1];
			parentLevel[depth] = maClient.dirCreateLevel(parentLevel[depth - 1], titleId, depth, levelId, collapseState);
		}
		
		i += 2;
	}
};

maClient.dirCreateTitleBar = function()
{
	if (maTour.dir.titleBarWidth > 0)
	{
		var padding = 6;
		maClient.dirPanel.style.width = maTour.dir.titleBarWidth - padding + "px";
	}
	
	var bar = document.getElementById("maDirTitleBar");
	
	var title = document.createElement("td");
	title.className = maClient.useTouchCss ? "maDirTitleTouch" : "maDirTitle";
	title.innerHTML = maTour.dir.textTitle;
	bar.appendChild(title);
	
	if (maTour.dir.showGroupSort)
	{
		var sortTd = document.createElement("td");
		var sortImg = document.createElement("img");
		sortTd.appendChild(sortImg);
		sortTd.style.paddingLeft = "4px";
		sortTd.style.textAlign = "right";
		sortImg.style.width = "16px";
		sortImg.style.height = "17px";
		sortImg.title = maTour.dir.textAlphaSortTooltip;
		sortImg.id = "maDirToggle";
		sortImg.src = maClient.dirMode == 2 ? maClient.graphics["sortAlpha"].src : maClient.graphics["sortGroup"].src;
		sortTd.onclick = new Function("maClient.dirToggleOrder();");
		bar.appendChild(sortTd);
	}
	
	if (maTour.dir.showSearch)
	{
		var search = document.createElement("td");
		search.className = maClient.useTouchCss ? "maDirSearchLabelTouch" : "maDirSearchLabel";
		search.innerHTML = maTour.dir.textSearchLabel;
		bar.appendChild(search);
		
		var searchBoxTd = document.createElement("td");
		var searchBox = document.createElement("input");
		searchBoxTd.appendChild(searchBox);
		searchBox.className = maClient.useTouchCss ? "maDirSearchBoxTouch" : "maDirSearchBox";
		searchBox.type = "text";
		bar.appendChild(searchBoxTd);
		maClient.dirSearchBox = searchBox;
	}
};


// Class constructor.
function maDirEntry(parentLevel, div, slide)
{
	this.div = div;
	this.parentLevel = parentLevel;
	this.slide = slide;
}

maClient.dirValidLocation = function(locationId, titleElement, topMenuElement)
{
	if (locationId == 4)
	{
		if (titleElement)
			return 4;
		else
			locationId = 5;
	}
	
	if (locationId == 5)
	{
		if (topMenuElement)
			return 5;
		else if (titleElement)
			return 4;
	}

	if (locationId != 1)
		locationId = document.getElementById("maMap") ? 3 : 1;
	
	return locationId;
};

maClient.dirSetLocation = function()
{
	var x = 0;
	var y = 0;
	var containerElement;
	var containerLocation;
	
	var locationId = maTour.dir.location;
	var mapElement = document.getElementById("maMap");
	var titleElement = document.getElementById("maPageTitle");
	var topMenuElement = document.getElementById("maTopMenu");
	
	// Determine if the location is valid and if not, change it to one that is.
	if (locationId == 2 || locationId == 3)
	{
		if (!mapElement)
			locationId = maClient.dirValidLocation(4, titleElement, topMenuElement);
	}
	else
	{
		locationId = maClient.dirValidLocation(locationId, titleElement, topMenuElement);
	}
	
	switch (locationId)
	{
		// Custom
		case 1:
		{
			break;
		}
		
		// MapLeft (2) or MapRight (3).
		case 2:
		case 3:
		{
			// Note that we used to get and save the map location in initClientState,
			// but that is too early. In a layout where the text area is over the map
			// area, the text area height had not been set yet and so the map's y offset
			// was wrong. So now we get the location dynamically.
			containerLocation = maClient.tagLocation("maTour", "maMap");
			y = containerLocation.y;
			x = containerLocation.x;
			if (locationId == 3)
				x += maTour.stageW - maClient.dirPanel.offsetWidth;
			break;
		}
			
		// TitleBar (4) or TopMenu (5).
		case 4:
		case 5:
		{
			containerElement = locationId == 4 ? titleElement : topMenuElement;
			if (containerElement)
			{
				containerLocation = maClient.tagLocation("maTour", containerElement.id);
				var topOffset = locationId == 4 ? 3 : 2;
				y = containerLocation.y + topOffset;
				x = containerLocation.x + containerElement.offsetWidth - maClient.dirPanel.offsetWidth - 4;
			}
			break;
		}
			
		default:
			break;	
	}
	
	x += maTour.dir.locationX;
	y += maTour.dir.locationY;
	
	maClient.dirPanel.style.left = x + 'px';
	maClient.dirPanel.style.top = y + 'px';  
	
	var bodyPanelStyle = maClient.dirBodyPanel.style;
	var bodyPanelDelta = 0;
	if (maTour.dir.alignContentRight)
		bodyPanelDelta = maClient.dirPanel.offsetWidth - maTour.dir.contentWidth - 2;
	bodyPanelStyle.left = (x + bodyPanelDelta) + 'px';
	bodyPanelStyle.top = (y + maClient.dirPanel.offsetHeight - 1) + 'px';
	
	maClient.dirPanel.style.visibility = "visible";
};

maClient.dirInit = function()
{
	if (!maTour.hasDirectory)
		return;

	console.log("dirInit");

	// If we got to this page from a directory on another page in this tour, the
	// cs query string arg will contain the collapse state of the directory.
	var collapseState = new Array();
	var cs = maGetQueryStringArg("cs");
	if (cs.length === 0)
	{
		maClient.dirMode = maTour.dir.showGroupSort ? 2 : 1;
	}
	else
	{
		collapseState = cs.split(",");

		// The first element of the state array is the mode.
		maClient.dirMode = collapseState[0];
	}

	maClient.dirPanel = document.getElementById("maDir");
	maClient.dirBodyPanel = document.getElementById("maDirBody");
	maClient.dirContentPanel = document.getElementById("maDirContent");
	maClient.dirStatusPanel = document.getElementById("maDirStatusLine");

	maClient.dirPanel.className = maClient.useTouchCss ? "maDirTouch" : "maDir";
	maClient.dirStatusPanel.className = maClient.useTouchCss ? "maDirStatusLineTouch" : "maDirStatusLine";
	maClient.dirContentPanel.className = maClient.useTouchCss ? "maDirContentTouch" : "maDirContent";

	maClient.dirBodyPanel.className = maClient.useTouchCss ? "maDirBodyTouch" : "maDirBody";

	if (maTour.dir.showImagePreview || maTour.dir.showTextPreview)
	{
		var previewDiv = document.createElement("div");
		previewDiv.className = "maDirPreview";
		document.body.appendChild(previewDiv);
		maClient.dirPreviewPanel = previewDiv;
	}

	maClient.dirCreateTitleBar();

	// Create the alphabetically sorted directory.
	maClient.dirCreateOrderAlpha();

	// Create the sort by group directory.
	if (maTour.dir.showGroupSort)
		maClient.dirCreateOrderGroup(collapseState);

	// Set the search text if we came here from another page that had search text.
	var find = maGetQueryStringArg("find");
	if (find.length !== 0)
		maClient.dirSearchBox.value = find;

	maClient.dirSearch();

	maClient.dirSetLocation();

	maAttachEventListener(maClient.dirPanel, "mouseout", maDirClose);
	maAttachEventListener(maClient.dirBodyPanel, "mouseout", maDirClose);

	if (maTour.dir.showSearch)
		maAttachEventListener(maClient.dirSearchBox, "keyup", maDirSearchChanged);

	if (maClient.macFirefox || maClient.safariLt3)
	{
		// Browser Fix #4.
		// The parts or all of the directory disappear because the browser is not playing
		// nicely with Flash. Keep refreshing the directory so that it stays visible.
		maClient.dirStartRefresh();

		if (maClient.macFirefox)
		{
			// Browser Fix #9.
			// Choose a color nearly identical to the title bar color so that you won't
			// notice the refresh which alternates between two colors.
			maTour.dir.backgroundColorAlt = maClient.deriveAltColor(maTour.dir.backgroundColor);
			maTour.dir.titleBarColorAlt = maClient.deriveAltColor(maTour.dir.titleBarColor);
		}
	}

	maClient.dirShowing = false;
	maClient.showingPreview = false;
	maClient.levelState = null;
};

// Class constructor.
function maDirLevel(parentLevel, id, depth, outerDiv, contentDiv, countSpan)
{
	this.id = id;
	this.depth = depth;
	this.parentLevel = parentLevel;
	this.outerDiv = outerDiv;
	this.contentDiv = contentDiv;
	this.countSpan = countSpan;
	this.content = new Array();
	this.resultCount = 0;
}

maClient.dirBodyMouseOver = function()
{
	maClient.dirMouseIsOver = true;
};

maClient.dirMouseOver = function()
{
	console.log("dirMouseOver");
	maClient.dirMouseIsOver = true;
	maClient.dirShow(true);
};

maClient.dirSearchText = function()
{
	if (maTour.dir.showSearch)
		return maClient.dirSearchBox.value;
	else
		return "";
};

maClient.dirSearch = function()
{
	if (maTour.dir.showImagePreview || maTour.dir.showTextPreview)
		maClient.dirHidePreview();
	
	var pattern = maClient.dirSearchText().toLowerCase();
	var clear = pattern.length <= 1;
	var patternLength = pattern.length;
	var resultCount = 0;
	
	for (var viewId in maClient.slides)
	{
		var slide = maClient.getSlide(viewId);
		
		slide.searchStart = "";
		slide.searchLength = 0;
		
		if (clear)
		{
			slide.inSearchResults = true;
			resultCount++;
			continue;
		}
		
		slide.inSearchResults = false;
		slide.searchLength = patternLength;
		var index = 0;
		var offset = 0;
		
		// First search the slide's title.
		if (slide.title.toLowerCase().indexOf(pattern) >= 0)
		{
			slide.inSearchResults = true;
			resultCount++;
		}
		
		if (slide.plainText.length === 0)
			continue;
		
		// Then search the slide's text
		var searchText = slide.plainText.toLowerCase();
		
		while (offset != -1)
		{
			var text = searchText.substr(index);
			offset = text.indexOf(pattern);
			if (offset >= 0)
			{
				slide.inSearchResults = true;
				if (slide.searchStart.length > 0)
					slide.searchStart += ",";
				slide.searchStart += index + offset;
				index += offset + patternLength;
				if (index >= searchText.length)
					offset = -1;
				resultCount++;
			}
		}
	}
	
	if (maTour.dir.showGroupSort)
		maClient.dirShowSearchResults(maClient.dirBodyLevels, clear);
	
	maClient.dirShowSearchResults(maClient.dirBodyAlphabetic, false);

	maClient.dirSetStatusMessage(maClient.dirSearchText());
};

function maDirSearchChanged(event)
{
	if (maClient.safariLt3)
	{
		alert("Search requires Safari 3 or greater");
		return;
	}
	maClient.dirSearchChanged();
}

maClient.dirSearchChanged = function()
{
	console.log("dirSearchChanged");

	if (!maClient.dirShowing)
		maClient.dirShow(true);
	
	maClient.dirSearch();
};

maClient.dirSearchClear = function()
{
	maClient.dirSearchBox.value = "";
	maClient.dirSearch();
};

maClient.dirSetStatusMessage = function()
{
	var msg;
	var resultCount;
	var searchString = maTour.dir.showSearch ? maClient.dirSearchText() : "";
	
	if (maClient.dirMode == 1)
		resultCount = maClient.dirBodyAlphabetic.resultCount;
	else
		resultCount = maClient.dirBodyLevels.resultCount;
	
	if (searchString.length >= 2)
	{
		var className = maClient.useTouchCss ? "maDirEntrySearchResultTouch" : "maDirEntrySearchResult";
		msg = maTour.dir.textSearchResultsMessage + " <span class='" + className + "'>" + searchString + "</span> : <b>" + resultCount + "</b>";
		msg += "<b><a style='margin-left:16px;' href='javascript:maClient.dirSearchClear();' title='Erase the Search box'>" + maTour.dir.textClearButtonLabel + "</a></b>";
	}
	else
	{
		msg = maTour.dir.textNoSearchMessage + " : " + resultCount;
	}
	
	maClient.dirStatusPanel.innerHTML = msg;
};

maClient.dirShowSearchResults = function(level, clear)
{
	// Loop over all the levels exposing/hiding ones with/without entries.
	var resultCount = 0;
	for (var index = 0; index < level.content.length; index++)
	{
		var o = level.content[index];
		if (o instanceof maDirEntry)
		{
			var inSearchResults = o.slide.inSearchResults;
			o.div.style.display = inSearchResults ? "block" : "none";
			if (inSearchResults)
				resultCount++;
		}
		else
		{
			resultCount += maClient.dirShowSearchResults(o, clear);
		}
	}
	if (level.outerDiv !== null)
	{
		if (level.depth == 1)
			level.countSpan.innerHTML = "&nbsp;(" + resultCount + ")";
		level.outerDiv.style.display = resultCount > 0 ? "block" : "none";
		
		var div = maClient.dirLevelContentDiv(level.id);
		if (div)
			div.style.display = clear && level.depth == 1 ? 'none' : 'block';
	}
		
	level.resultCount = resultCount;
	return resultCount;
};

maClient.dirAdjustHeight = function(count)
{
	// All the supported browsers except IE 6 support a max height style, but for
	// IE 6 we have to do the calculations manually.
	if (!maClient.ie6)
		return;
		
	var contentHeight = maClient.dirContentPanel.firstChild.offsetHeight;
	if (contentHeight === 0)
	{
		// The content has not finished loading yet. Give it a little time and try again.
		if (typeof count == 'undefined')
			count = 1;
		else if (count >= 10)
			return;
		count++;
		setTimeout("maClient.dirAdjustHeight(" + count + ");", 1);
	}
	else
	{
		var h;
		if (contentHeight > maTour.dir.maxHeight)
			h = maTour.dir.maxHeight;
		else
			h = contentHeight;
		maClient.dirContentPanel.style.height = h + "px";
	}
};

maClient.closeDirIfMouseIsNotOverIt = function()
{
	var dirIsShowing = maClient.dirBodyPanel.style.display == "block";
	var closeDir = dirIsShowing && !maClient.mouseIsOverDir;
	console.log("closeDirIfMouseIsNotOverIt %s %s", dirIsShowing, closeDir);
	if (closeDir)
	{
		// This logic is here only until we figure out why the directory sometimes gets stuck
		// open (see OnTime 186). This function gets called on a timer after the dir has been
		// opened. When it gets called, if the dir is still open, but the mouse is not over it,
		// we force it closed.
		maClient.dirShow(false);
	}
};

maClient.mouseIsOverDir = function()
{
	var tourLocation = maClient.tagLocation(null, "maTour");
	var x = maClient.mouseX - tourLocation.x;
	var y = maClient.mouseY - tourLocation.y;
	
	var e = maClient.dirPanel;
	var mouseIsOverDirHeader = 
		x >= e.offsetLeft && x <= e.offsetLeft + e.offsetWidth &&
		y >= e.offsetTop && y <= e.offsetTop + e.offsetHeight;
		
	e = maClient.dirBodyPanel;
	var mouseIsOverDirBody = 
		x >= e.offsetLeft && x <= e.offsetLeft + e.offsetWidth &&
		y >= e.offsetTop && y <= e.offsetTop + e.offsetHeight;
	
	return mouseIsOverDirHeader || mouseIsOverDirBody;
};

maClient.dirShow = function(show)
{
	console.log("dirShow %s %s", show, maClient.mapLoaded);
	
	if (!maClient.mapLoaded)
	{
		// Since the directory messages the map we don't want it open until the map is loaded.
		console.log("== IGNORE map not loaded yet");
		return;
	}

	if (show)
	{
		if (!maTour.dir.staysOpen)
		{
			// Close the popup when showing the directory unless the directory is always open.
			// If we close the popup in that case, the initial display of the directory after the map 
			// loads would close a popup that was opened because its hotspot Id was on the query string.		
			maClosePopup();
		}
		
		var dirBodyContent = maClient.dirContentPanel.firstChild;
		
		if (dirBodyContent)
			maClient.dirContentPanel.removeChild(dirBodyContent);
		
		if (maClient.dirMode == 1)
		{
			maClient.dirContentPanel.appendChild(maClient.dirBodyAlphabetic.contentDiv);
		}
		else
		{
			maClient.dirContentPanel.appendChild(maClient.dirBodyLevels.contentDiv);
		}
		
		maClient.dirAdjustHeight();		
		maClient.dirSetStatusMessage();
		
		// This logic is temporary until we figure out why the directory sometimes gets stuck open.
		clearInterval(maClient.dirCloseDirIntervalId);
		maClient.dirCloseDirIntervalId = setTimeout("maClient.closeDirIfMouseIsNotOverIt();", 2000);
		// End temporary logic. 
	}
	else
	{
		maClient.dirMouseIsOver = false;
	}
		
	maClient.dirBodyPanel.style.display = show || maTour.dir.staysOpen ? "block" : "none";
	maClient.dirShowing = show || maTour.dir.staysOpen;
	
	if (maClient.map)
	{
		// Tell Flash the the directory is open or closed, but only if this layout has a map.
		// Note that is the directory stays open, we have to tell Flash it's closed, otherwise
		// it will ignore marker mouseovers.
		maClient.map.setDirectoryState(show && !maTour.dir.staysOpen);
	}
};

maClient.updateLevelState = function()
{
	maClient.levelState = maClient.dirMode.toString();
	maClient.dirSaveCollapseState(maClient.dirBodyLevels);
};

maClient.dirShowEntry = function(viewId)
{
	maClient.dirHidePreview();

	// Determine if the viewId is on this page or on another page.
	var pageNumber = maClient.getPageBySlideViewId(viewId).pageNumber;
	if (pageNumber != maTour.pageNumber)
	{
		maClient.updateLevelState();
		maClient.goToPage('page' + pageNumber + '.htm', viewId, maClient.levelState);
	}
	else
	{
		if (typeof maOnDirectoryEntryClick != "undefined")
		{
			var slide = maClient.getSlide(viewId);
			maOnDirectoryEntryClick(slide.slideId);
		}

		maClient.showSlide(viewId, true);
	}
};

maClient.dirMovePreviewPanel = function()
{
	var border = 2;
	var offset;
	if (maTour.dir.previewOnRight)
	{
		offset = 16;
	}
	else
	{
		offset = maClient.showingPreviewImageOnly ? maTour.dir.previewImageWidth + border : maTour.dir.previewWidth;
		offset = -(offset + 32);
	}

	var x = maClient.mouseX;
	if (maClient.preview)
	{
		// Adjustment for V4 Tour Preview which is centered in the browser instead of left-aligned as it was in V3.
		// When the browser is wider than the Tour Builder, the blank space between the left edge of the preview panel
		// and the left edge of the browser must be subtracted from mouse X. A left scroll adjustment is needed too.
		var previewPanelElement = document.getElementById('PreviewPanelV3');
		x -= previewPanelElement.getBoundingClientRect().left + maClient.getScrollingPosition()[0];
	}

	maClient.dirPreviewPanel.style.left = x + offset + "px";
	maClient.dirPreviewPanel.style.top = maClient.mouseY - 8 + "px";
	
	// NOTE: One of the reasons we put the preview on the left is because on Mac Firefox,
	// if the preview is on the right and the dir content panel has a vertical scroll bar,
	// the preview will appear over the panel (correct) but under the scroll bar. This was
	// last seen on FF 2.0.0.12 on Leopard. If we ever want to add an option that allows the
	// preview to appear to the right of the mouse, this problem will have to be faced.
};

maClient.dirHidePreview = function()
{
	var previewDiv = maClient.dirPreviewPanel;
	
	if (!previewDiv)
		return;
		
	clearInterval(maClient.dirPreviewIntervalId);

	maClient.dirShowPreviewPanel(false);
	maClient.dirPreviewSlide = null;
	maClient.showingPreview = false;
};

maClient.highlightSearchText = function(slide)
{
	var searchText = slide.dirPreviewText;
	if (searchText.length === 0)
		searchText = slide.plainText;
	var searchStart = slide.searchStart.split(',');
	var lastStart = 0;
	var html = "";

	if (slide.searchLength >= 2 && slide.searchStart.length > 0)
	{
		for (var index = 0; index < searchStart.length; index++)
		{
			var start = parseInt(searchStart[index], 10);
			var found = searchText.substr(start, slide.searchLength);
			if (found.length > 0)
			{
				var className = maClient.useTouchCss ? "maDirEntrySearchResultTouch" : "maDirEntrySearchResult";
				html += 
					searchText.substring(lastStart, start) + 
					"<span class='" + className + "'>" + found + "</span>";
			}
			lastStart = start + slide.searchLength;
		}
	}

	html += searchText.substring(lastStart);
	
	return html;
};

maClient.dirShowPreview = function(viewId)
{
	var previewDiv = maClient.dirPreviewPanel;
	
	if (!previewDiv)
		return;

	var slide = maClient.getSlide(viewId);
	
	maClient.dirPreviewSlide = slide;
	
	if (typeof maOnDirectoryEntryMouseover != "undefined")
		maOnDirectoryEntryMouseover(slide.slideId);

	if (slide.usesLiveData && slide.liveDataUpdateTime === 0)
	{
		// This slide uses live data, but live data has not yet been called for it.
		// Make the call and then try again in a short while to see if the data is 
		// there. Repeat until we have it.
		clearInterval(maClient.dirPreviewIntervalId);
		maClient.dirPreviewIntervalId = setTimeout("maClient.dirShowPreview(" + viewId + ");", 250);
		
		if (!slide.liveDataRequestPending)
		{
			slide.liveDataRequestPending = true;
			maClient.getLiveData(slide);
		}
		return;
	}
	
	// Determine if there is preview text.
	var html = maClient.highlightSearchText(slide);
	var noText = html.length === 0 || !maTour.dir.showTextPreview;
	var innerHtml = "";
	
	// Determine if there is a preview image. An image URL of "0" means don't show a preview for
	// this slide. We use the suppress feature to keep from displaying a preview for data sheets.
	var suppressPreview = slide.dirPreviewImageUrl == "0";
	var noImage = slide.mediaW === 0 || slide.imageSrc === null || suppressPreview;
	
	if (noImage && !suppressPreview && slide.dirPreviewImageUrl.length)
	{
		// There's no MapsAlive image to show, but the user has provided an image URL via Live Data.
		noImage = false;
	}

	if (maTour.dir.showImagePreview && !noImage)
	{
		// Determine the preview image width. If using an image URL that the user provided, we don't know
		// the image size, so use the preview width that the user set for the directory. Otherwse, compare
		// the media width and the preview width and use whichever is smaller.
		var w;
		if (slide.dirPreviewImageUrl.length)
			w = maTour.dir.previewImageWidth;
		else
		w = slide.mediaW < maTour.dir.previewImageWidth ? slide.mediaW : maTour.dir.previewImageWidth;
		
		var margin = noText ? "" : "margin-left:4px;";
		var width = "width:" + w + "px;";
		var imaSrc = slide.dirPreviewImageUrl.length ? slide.dirPreviewImageUrl : maClient.slideImageSrc(slide);
		var imgTag = "<img src='" + imaSrc + "' style='" + width + margin + "' class='maDirPreviewImage'/>";
		innerHtml += imgTag;
	}

	maClient.showingPreviewImageOnly = false;

	if (noText)
	{
		if (maTour.dir.showImagePreview && !noImage)
			maClient.showingPreviewImageOnly = true;
	}
	else
	{
		innerHtml += html;
	}

	if (innerHtml.length === 0)
	{
		previewDiv.style.visibility = "hidden";
	}
	else
	{
		maClient.dirMovePreviewPanel();
		previewDiv.style.padding = noText ? "0px" : "4px";
		previewDiv.innerHTML = innerHtml;
		var border = 2;
		previewDiv.style.width = (noText ? maTour.dir.previewImageWidth + border : maTour.dir.previewWidth) + "px";

		// Wait just a little while before showing the preview to avoid flicker when
		// the user is rapidly mousing over the directory entries.
		maClient.dirPreviewIntervalId = setTimeout("maClient.dirShowPreviewPanel(true);", 250);
	}
	
	maClient.showingPreview = true;
};

maClient.dirShowPreviewPanel = function(show)
{
	maClient.dirPreviewPanel.style.visibility = show ? "visible" : "hidden";
};

maClient.dirLevelContentDiv = function(levelId)
{
	var divId = "maDirLevel" + levelId + "Content";
	return document.getElementById(divId);
};

maClient.dirSaveCollapseState = function(level)
{
	if (typeof level == "undefined")
		return;
	
	for (var index = 0; index < level.content.length; index++)
	{
		var o = level.content[index];
		if (o instanceof maDirEntry)
		{
			continue;
		}
		else
		{
			// Add level 1 items if they are expanded and level 2 items if they are collapsed.
			// The idea is to only include ones whose expand/collapse state are opposite of
			// what they'll usually be (level 1 is usually collapsed and level 2 is usually
			// expanded). This keeps the query arg short.
			var expanded = o.contentDiv.style.display == "block";
			var include = o.depth == 1 ? expanded : !expanded;
			if (include && o.resultCount)
				maClient.levelState += "," + o.id;
			maClient.dirSaveCollapseState(o);
		}
	}
};

maClient.dirToggleLevelExpansion = function(levelId, depth)
{
	var div = maClient.dirLevelContentDiv(levelId);
	var show = div.style.display == 'none';
	div.style.display = show ? 'block' : 'none';
	
	// Only toggle when clicking a level 1 item.
	if (depth > 1)
		return;
	
	if (maTour.dir.autoCollapse)
	{
		// Loop over all the level 1 levels hiding all but one.
		var levels = maClient.dirBodyLevels.content;
		for (var index = 0; index < levels.length; index++)
		{
			var level = levels[index];
			if (level.depth > 1)
				continue;
			
			// Ignore Data Sheets that are attached at level 1 (they have no contentDiv)
			// because they don't need to be collapsed. 
			if (typeof level.contentDiv == "undefined")
				continue;
			
			if (level.contentDiv.id != div.id)
				level.contentDiv.style.display = 'none';
		}
	}

	maClient.dirAdjustHeight();		
		
	// Ignore the next mouseout event to deal with the case where collapsing a category
	// causes the panel to shorten so much that its bottom is above the current cursor
	// location. This is the only time it is possible to move the mouse around the map
	// with the directory expanded, but if we don't do this, the user clicks a new
	// category and the panel closes immediately.
	maClient.okToCloseDirectoryPanel = false;
};

maClient.dirToggleOrder = function()
{
	console.log("dirToggleOrder");
	
	var e = document.getElementById("maDirToggle");
	if (maClient.dirMode == 1)
	{
		e.title = maTour.dir.textAlphaSortTooltip;
		e.src = maClient.graphics["sortAlpha"].src;
		maClient.dirMode = 2;
	}
	else
	{
		e.title = maTour.dir.textGroupSortTooltip;
		e.src = maClient.graphics["sortGroup"].src;
		maClient.dirMode = 1;
	}
	maClient.dirShow(true);
};


maClient.dirStartRefresh = function()
{
	if (maClient.enableRefresh)
	{
		// See comment for popupStartRefresh.
		maClient.refreshDirCycle = maClient.macintosh ? 60 : 5;
		maClient.dirStopRefresh();
		maClient.refreshDirIntervalId = setInterval(maClient.refreshDir, 50);
	}
};

maClient.dirStopRefresh = function()
{
	if (typeof maClient.refreshDirIntervalId != "undefined")
		clearInterval(maClient.refreshDirIntervalId);
};

maClient.refreshDir = function()
{
	maClient.refreshDirCycle--;
	console.log("refreshDir");
	
	if (maClient.dirShowing)
		maClient.dirBodyPanel.style.backgroundColor = maClient.refreshDirCycle % 2 === 0 ? maTour.dir.backgroundColor : maTour.dir.backgroundColorAlt;
	
	maClient.dirPanel.style.backgroundColor = maClient.refreshDirCycle % 2 === 0 ? maTour.dir.titleBarColor : maTour.dir.titleBarColorAlt;
	
	if (maClient.refreshDirCycle === 0)
	{
		//maClient.dirStopRefresh();
		
		// Browser Fix #3.
		// Parts or all of the popup disappear because the browser is not playing
		// nicely with Flash. Keep refreshing the popop so that it stays visible.
		maClient.refreshDirIntervalId = setInterval(maClient.refreshDir, maClient.macFirefox ? 50 : 100);
	}
};

