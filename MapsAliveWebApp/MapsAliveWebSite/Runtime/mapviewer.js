// Copyright (C) 2009-2022 AvantLogic Corporation: https://www.mapsalive.com

if (typeof (console) === 'undefined')
{
	console = function() { };
	console.log = function(s) { };
}

//--// OBFUSCATE

// ----------------------------------------------------------------------------

// Constants
var maMarkerType_Symbol = 1;
var maMarkerType_Shape = 2;
var maMarkerType_SymbolAndShape = 3;
var maMarkerType_Text = 4;
var maMarkerType_Photo = 5;
var maShapeType_None = 0;
var maShapeType_Circle = 1;
var maShapeType_Rectangle = 2;
var maShapeType_Polygon = 3;
var maShapeType_Line = 4;
var maShapeType_Hybrid = 5;

var maActionNone = 0;
var maActionGotoPage = 1;
var maActionLinkToUrl = 2;
var maActionCallJavascript = 3;
var maActionLinkToUrlNewWindow = 5;

var maShowContentOnMouseover = 0;
var maShowContentOnClick = 1;
var maShowContentNever = 2;

maClient.runtimeVersionHtml5 = 13;

// Class Constructors
// ----------------------------------------------------------------------------

maClient.Html5 = function()
{
};

function maMarkerInstance(
	map,
	viewId,
	markerType,
	markerStyleId,
	normalSymbolId,
	selectedSymbolId,
	pctX,
	pctY,
	x,
	y,
	shapeType,
	shapeW,
	shapeH,
	normalSymbolW,
	normalSymbolH,
	selectedSymbolW,
	selectedSymbolH,
	symbolLocationX,
	symbolLocationY,
	anchorX,
	anchorY,
	rotation,
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
	shapeCoords)
{
	this.map = map;
	this.viewId = viewId;
	this.markerType = markerType;
	this.markerStyleId = markerStyleId;
	this.blendMode = "";
	this.customMarkerStyle = null;
	this.normalSymbolId = normalSymbolId;
	this.selectedSymbolId = selectedSymbolId;
	this.shapeType = shapeType;
	this.shapeW = shapeW;
	this.shapeH = shapeH;
	this.normalSymbolW = normalSymbolW;
	this.normalSymbolH = normalSymbolH;
	this.selectedSymbolW = selectedSymbolW;
	this.selectedSymbolH = selectedSymbolH;
	this.symbolLocationX = symbolLocationX;
	this.symbolLocationY = symbolLocationY;
	this.anchorX = anchorX;
	this.anchorY = anchorY;
	this.zoomThreshold = zoomThreshold;
	this.flags = flags;
	this.isDisabled = (flags & 0x00000001) !== 0;
	this.isHidden = (flags & 0x00000002)!== 0;
	this.isStatic = (flags & 0x00000004)!== 0;
	this.isRoute = (flags & 0x00000008)!== 0;
	this.markerZooms = (flags & 0x00000020)!== 0;
	this.isShapeOnly = (flags & 0x00000040) !== 0;
	this.isNotAnchored = (flags & 0x00000080) !== 0;
	this.isBound = (flags & 0x00000100) !== 0;
	this.doesNotShowContent = (flags & 0x00000400) !== 0;
	this.appearsSelected = false;
	this.tooltip = tooltip;
	this.clickAction = clickAction;
	this.clickActionTarget = clickActionTarget;
	this.mouseoverAction = mouseoverAction;
	this.mouseoverActionTarget = mouseoverActionTarget;
	this.mouseoutAction = mouseoutAction;
	this.mouseoutActionTarget = mouseoutActionTarget;
	this.touchPerformsClickAction = touchPerformsClickAction;
	this.mouseIsOverMarker = false;
	this.showContentEvent = showContentEvent;
	this.routeDefinition = null;
	this.blinkAlpha = 1.0;
	this.blinkCount = 0;
	this.blinkLimit = 0;
	this.blinkDirection = 0;
	this.visited = false;
	this.cornerX = 0;
	this.cornerY = 0;
	this.drawnMaxWidth = 0;
	this.drawnMaxHeight = 0;
	this.isVisibleAtCurrentZoomLevel = false;

	if (this.isNotAnchored)
	{
		// Make x and y to be relative to the viewport instead of to the map.
		this.x = Math.round(this.map.viewPort.w * pctX);
		this.y = Math.round(this.map.viewPort.h * pctY);
	}
	else
	{
		this.x = x;
		this.y = y;
	}
	
	// Convert from degrees to radians.
	this.rotation = rotation * Math.PI / 180;
	if (this.rotation !== 0)
	{
		// Calculate a bounding box that will contain the rotated marker. This could
		// be optimized to produce a better fit, but this code works for now.
		var hypotenuse = Math.round(Math.sqrt((shapeW * shapeW) + (shapeH * shapeH)));
		this.maxWidth = hypotenuse;
		this.maxHeight = hypotenuse;

		if (anchorX !== 0 || anchorY !== 0)
		{
			// The marker is not only rotated, but its anchor point is not at the center.
			// Make yet another gross adjustment to help ensure that the bounding box will
			// cover the entire marker.
			this.maxWidth *= 3;
			this.maxHeight *= 3;
		}
	}
	else
	{
		this.maxWidth = shapeW;
		this.maxHeight = shapeH;
	}

	// Add an extra pixel because without it we sometimes see a sliver of a marker leftover after a redraw.
	this.maxWidth += 2;
	this.maxHeight += 2;

	//console.log("maMarkerInstance " + this.maxWidth + "x" + this.maxHeight);

	this.shapeCoords = shapeCoords;

	// The padding accounts for the marker's drop shadow, blur, and border thickness.
	this.paddingNormal = new Object();
	this.paddingSelected = new Object();
	this.paddingNormal.x = 0;
	this.paddingNormal.y = 0;
	this.paddingNormal.w = 0;
	this.paddingNormal.h = 0;
	this.paddingSelected.x = 0;
	this.paddingSelected.y = 0;
	this.paddingSelected.w = 0;
	this.paddingSelected.h = 0;

	var me = this;

	var symbolType;
	var symbolIdN = 0;
	var symbolIdS = 0;

	if (markerType == maMarkerType_Symbol || markerType == maMarkerType_SymbolAndShape)
	{
		// Symbol or Symbol+Shape marker.
		symbolType = "S";
		if (normalSymbolId !== 0)
		{
			symbolIdN = normalSymbolId;
		}

		if (selectedSymbolId !== 0)
		{
			symbolIdS = selectedSymbolId;
		}
	}
	else if (markerType == maMarkerType_Text || markerType == maMarkerType_Photo)
	{
		// Photo or Text marker.
		symbolType = "H";
		symbolIdN = viewId;
		symbolIdS = viewId;
	}

	if (symbolIdN !== 0)
	{
		this.hasImageN = true;
		this.imgLoadedN = false;
		this.imgN = new Image();
		this.imgN.onload = function() { me.imgLoadedN = true; };
		this.imgN.src = this.getSymbolDataUri(symbolIdN, symbolType, "N");
	}
	else
	{
		this.hasImageN = false;
	}

	if (symbolIdS !== 0)
	{
		this.hasImageS = true;
		this.imgLoadedS = false;
		this.imgS = new Image();
		this.imgS.onload = function() { me.imgLoadedS = true; };
		this.imgS.src = this.getSymbolDataUri(symbolIdS, symbolType, "S");
	}
	else
	{
		this.hasImageS = false;
	}
}

maMarkerInstance.prototype.getSymbolDataUri = function(symbolId, symbolType, symbolState)
{
	// Get the array of marker image data objects. Note that the markerImages() function is defined
	// in the dynamically loaded symbols data JavaScript file -- it's not here in mapviewer.js.
	var markerImages = this.map.markerImages();
	
	for (var index in markerImages)
	{
		var o = markerImages[index];
		if (o.id == symbolType + symbolId + symbolState)
		{
			return "data:image/png;base64," + o.data;
		}
	}
	return null;
};

maMarkerInstance.prototype.anchorScaledX = function()
{
	var x = this.anchorX;
	if (this.markerZooms)
	{
		x = Math.round(x * this.map.currentMapScale);
	}
	return x;
};

maMarkerInstance.prototype.anchorScaledY = function()
{
	var y = this.anchorY;
	if (this.markerZooms)
	{
		y = Math.round(y * this.map.currentMapScale);
	}
	return y;
};

maMarkerInstance.prototype.blendOnto = function()
{
	// This method was developed by AvantLogic based on code from http://github.com/Phrogz/context-blender
	//
	// Context Blender JavaScript Library
	// 
	// Copyright © 2010 Gavin Kistner
	// 
	// Permission is hereby granted, free of charge, to any person obtaining a copy
	// of this software and associated documentation files (the "Software"), to deal
	// in the Software without restriction, including without limitation the rights
	// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	// copies of the Software, and to permit persons to whom the Software is
	// furnished to do so, subject to the following conditions:
	// 
	// The above copyright notice and this permission notice shall be included in
	// all copies or substantial portions of the Software.

	var mode = this.blendMode.toLowerCase();

	// See comment on disableBlendingOnChrome flag.
	if (maClient.disableBlendingOnChrome)
		return;
	
	if (maTour.disableBlendEffect && maClient.mobile)
	{
		// The user asked us to disable blending for mobile.
		// Blending simply doesn't look right on Android with the AppleWebKit browser.
		return;
	}
	
	if (mode.length === 0 || mode == "normal" || mode == "invert")
	{
		// Normal doesn't do anything and we don't currently support invert.
		return;
	}

	// Get the shape's location and size on the map.
	var x = this.cornerX;
	var y = this.cornerY;
	var w = this.drawnMaxWidth;
	var h = this.drawnMaxHeight;

	// Increase the shape's size by half the border width.
	var lineWidth = this.lineWidth();
	w += lineWidth;
	h += lineWidth;
	x -= lineWidth / 2;
	y -= lineWidth / 2;

	// Clip the size of the shape if it is partially off the map.
	// If we don't do this we'll get an image data error.
	if (x < 0)
	{
		w += x;
		x = 0;
	}
	if (y < 0)
	{
		h += y;
		y = 0;
	}

	if (x + w > this.map.viewPort.w)
	{
		w -= x + w - this.map.viewPort.w;
	}
	if (y + h > this.map.viewPort.h)
	{
		h -= y + h - this.map.viewPort.h;
	}

	//console.log("blendOnto " + this.viewId + " : " + w + "x" + h);

	// Erase the hit layer and draw the marker's shape on it. We'll use this layer to tell which
	// pixels are part of the shape and which are not so that we only blend pixels within the shape.
	this.map.hitLayerContext.clearRect(0, 0, this.map.viewPort.w, this.map.viewPort.h);
	this.map.drawMarkerShape(this, this.map.hitLayerContext, false);

	// Read the image data for the exact same region on each of the hit, map, and marker layers.
	var hitLayerImageData;
	var mapLayerImageData;
	var markerLayerImageData;
	try
	{
		hitLayerImageData = this.map.hitLayerContext.getImageData(x, y, w, h);
		mapLayerImageData = this.map.mapLayerContext.getImageData(x, y, w, h);
		markerLayerImageData = this.map.markerLayerContext.getImageData(x, y, w, h);
	}
	catch (error)
	{
		console.log("ERROR blendOnto");
		console.log("== viewPort: " + this.map.viewPort.x1 + "," + this.map.viewPort.x2 + "," + this.map.viewPort.y1 + "," + this.map.viewPort.y2);
		console.log("== " + x + "," + y + " : " + w + "x" + h);
		return;
	}

	// Get the array of pixels for each layer.
	var hit = hitLayerImageData.data;
	var src = mapLayerImageData.data;
	var dst = markerLayerImageData.data;

	// Variables that keep track of which pixel in the arrays corresponds to which pixel in the shape.
	var x_ = x;
	var y_ = y;

	// Get the clipping area that blending will be restricted to. The clip area comes into play when
	// redrawing the markers that intersect a marker that is changing from its normal to selected
	// appearance or vice versa. The blend mode is applied to all of the pixels of that marker but
	// is only applied to those pixels of intersecting markers that are within the clipping region.
	// This is not just an optimization. If we didn't honor the clipping area, intersecting marker
	// pixels that are outside of the clipping area would get blended over and over again creating
	// weird effects. Intersecting marker pixels inside the clipping area only get blended once,
	// because the entire clipping area is erased before redrawing begins.
	var clip = this.map.clipBounds;

	// Get the total number of pixels. Each contains four bytes (RGBA), the last being the alpha channel.
	var pixelCount = dst.length;

	for (var px = 0; px < pixelCount; px += 4)
	{
		var skip = false;

		if (hit[px + 3] === 0)
		{
			// The pixel that was drawn on the hit test layer is not part of the marker's shape.
			skip = true;
		}
		else if (clip !== null && (x_ < clip.x || x_ >= clip.x + clip.w || y_ < clip.y || y_ >= clip.y + clip.h))
		{
			// The pixel is outside the clipping area.
			skip = true;
		}

		// Keep track of which pixel maps to which x,y coordinate.
		x_ += 1;
		if (x_ - x >= w)
		{
			// Move to the next row.
			x_ = x;
			y_ += 1;
		}

		if (skip)
		{
			// Don't change the destination pixel.
			continue;
		}

		// Calculate the blended alpha value.
		var sA = src[px + 3] / 255;
		var dA = dst[px + 3] / 255;
		var dA2 = (sA + dA - sA * dA);
		dst[px + 3] = dA2 * 255;

		// Get the source and destination RGB values.
		var sRA = src[px] / 255 * sA;
		var dRA = dst[px] / 255 * dA;
		var sGA = src[px + 1] / 255 * sA;
		var dGA = dst[px + 1] / 255 * dA;
		var sBA = src[px + 2] / 255 * sA;
		var dBA = dst[px + 2] / 255 * dA;

		var demultiply = 255 / dA2;

		switch (mode)
		{
			case 'darken':
				dst[px] = (sRA > dRA ? dRA : sRA) * demultiply;
				dst[px + 1] = (sGA > dGA ? dGA : sGA) * demultiply;
				dst[px + 2] = (sBA > dBA ? dBA : sBA) * demultiply;
				break;

			case 'difference':
				dst[px] = (sRA + dRA - 2 * Math.min(sRA * dA, dRA * sA)) * demultiply;
				dst[px + 1] = (sGA + dGA - 2 * Math.min(sGA * dA, dGA * sA)) * demultiply;
				dst[px + 2] = (sBA + dBA - 2 * Math.min(sBA * dA, dBA * sA)) * demultiply;
				break;

			case 'lighten':
				dst[px] = (sRA < dRA ? dRA : sRA) * demultiply;
				dst[px + 1] = (sGA < dGA ? dGA : sGA) * demultiply;
				dst[px + 2] = (sBA < dBA ? dBA : sBA) * demultiply;
				break;

			case 'multiply':
				dst[px] = (sRA * dRA + sRA * (1 - dA) + dRA * (1 - sA)) * demultiply;
				dst[px + 1] = (sGA * dGA + sGA * (1 - dA) + dGA * (1 - sA)) * demultiply;
				dst[px + 2] = (sBA * dBA + sBA * (1 - dA) + dBA * (1 - sA)) * demultiply;
				break;

			case 'overlay': // was hardlight in context_blender.js but this matches Flash overlay mode.
				dst[px] = (sRA <= 0.5) ? (2 * dst[px] * sRA / dA) : 255 - (2 - 2 * sRA / sA) * (255 - dst[px]);
				dst[px + 1] = (sGA <= 0.5) ? (2 * dst[px + 1] * sGA / dA) : 255 - (2 - 2 * sGA / sA) * (255 - dst[px + 1]);
				dst[px + 2] = (sBA <= 0.5) ? (2 * dst[px + 2] * sBA / dA) : 255 - (2 - 2 * sBA / sA) * (255 - dst[px + 2]);
				break;

			case 'hardlight': // was overlay in context_blender.js but this matches Flash hardlight mode.
				dst[px] = (dRA <= 0.5) ? (2 * src[px] * dRA / dA) : 255 - (2 - 2 * dRA / dA) * (255 - src[px]);
				dst[px + 1] = (dGA <= 0.5) ? (2 * src[px + 1] * dGA / dA) : 255 - (2 - 2 * dGA / dA) * (255 - src[px + 1]);
				dst[px + 2] = (dBA <= 0.5) ? (2 * src[px + 2] * dBA / dA) : 255 - (2 - 2 * dBA / dA) * (255 - src[px + 2]);
				break;

			case 'screen':
				dst[px] = (sRA + dRA - sRA * dRA) * demultiply;
				dst[px + 1] = (sGA + dGA - sGA * dGA) * demultiply;
				dst[px + 2] = (sBA + dBA - sBA * dBA) * demultiply;
				break;

			default:
				break;
		}
	}

	// Write the blended destination pixels back to the marker layer. Any pixels
	// that were not blended will be written back using their original values.
	this.map.markerLayerContext.putImageData(markerLayerImageData, x, y);
};

maMarkerInstance.prototype.computeMarkerLocationValues = function()
{
	// These variables are used to cache the value of expensive computations. We know they are
	// expensive from using the Firebug profiler to see where time is spent. This function gets
	// called when we know the values may have gotten stale. The functions that do the computing
	// end with an underscore to remind us not to call them directly.
	this.drawnMaxWidth = this.drawnMaxWidth_();
	this.drawnMaxHeight = this.drawnMaxHeight_();
	this.cornerX = this.cornerX_();
	this.cornerY = this.cornerY_();
	this.isVisibleAtCurrentZoomLevel = this.isVisibleAtCurrentZoomLevel_();
};

maMarkerInstance.prototype.centerX = function()
{
	var x = this.x;

	if (this.isNotAnchored)
	{
		x -= this.map.mapPanX;
	}
	else
	{
		x = Math.round(this.map.currentMapScale * x);
	}
	return x;
};

maMarkerInstance.prototype.centerY = function()
{
	var y = this.y;

	if (this.isNotAnchored)
	{
		y -= this.map.mapPanY;
	}
	else
	{
		y = Math.round(this.map.currentMapScale * y);
	}
	return y;
};

maMarkerInstance.prototype.cornerX_ = function()
{
	return this.centerX() - Math.round(this.drawnMaxWidth / 2) - this.anchorScaledX() - this.map.viewPort.x1;
};

maMarkerInstance.prototype.cornerY_ = function()
{
	return this.centerY() - Math.round(this.drawnMaxHeight / 2) - this.anchorScaledY() - this.map.viewPort.y1;
};

maMarkerInstance.prototype.offsetX = function()
{
	return -Math.round(this.shapeW / 2);
};

maMarkerInstance.prototype.offsetY = function()
{
	return -Math.round(this.shapeH / 2);
};

maMarkerInstance.prototype.drawnWidth = function()
{
	var w = this.shapeW;
	if (this.markerZooms)
		w = Math.round(this.map.currentMapScale * w);
	return w;
};

maMarkerInstance.prototype.drawnHeight = function()
{
	var h = this.shapeH;
	if (this.markerZooms)
		h = Math.round(this.map.currentMapScale * h);
	return h;
};

maMarkerInstance.prototype.drawnMaxWidth_ = function()
{
	var w = this.maxWidth;
	if (this.markerZooms)
		w = Math.round(this.map.currentMapScale * w);
	return w;
};

maMarkerInstance.prototype.drawnMaxHeight_ = function()
{
	var h = this.maxHeight;
	if (this.markerZooms)
		h = Math.round(this.map.currentMapScale * h);
	return h;
};

maMarkerInstance.prototype.isVisibleAtCurrentZoomLevel_ = function()
{
	if (this.isHidden)
		return false;
		
	// Determine if the map is zoomed in or out enough to see this marker.
	var isVisible = true;
	var zoomLevelPercent = Math.round(this.map.currentMapScale * 100);

	if (this.zoomThreshold < 0)
	{
		// A negative threshold means the marker is visble at or below that zoom percentage.
		isVisible = zoomLevelPercent <= -this.zoomThreshold;
	}
	else if (this.zoomThreshold > 0)
	{
		// A positive threshold means the marker is visble at or above that zoom percentage.
		isVisible = zoomLevelPercent >= this.zoomThreshold;
	}

	return isVisible;
};

maMarkerInstance.prototype.lineWidth = function()
{
	var markerStyle = this.markerStyle();
	if (markerStyle === null)
	{
		return 0;
	}
	else
	{
		var lineWidth = markerStyle.lineWidth;
		if ((maClient.ie9 || maClient.ie10) && lineWidth % 2 == 1 && this.blendMode.length !== 0)
		{
			// Bump the line width to an even number to avoid an IE9 and IE10 bug that makes
			// shapes wiggle around on mouseover if the line width is odd and blend mode is on.
			lineWidth++;
		}
		return lineWidth;
	}
};

maMarkerInstance.prototype.markerStyle = function()
{
	var markerStyle;
	if (this.customMarkerStyle === null)
	{
		markerStyle = this.map.markerStyles[this.markerStyleId];
		if (typeof markerStyle == "undefined")
			markerStyle = null;
	}
	else
	{
		markerStyle = this.customMarkerStyle;
	}
	return markerStyle;
};

maMarkerInstance.prototype.setBlink = function(howManyTimes)
{
	if (typeof this.blinkIntervalId != "undefined")
	{
		clearInterval(this.blinkIntervalId);
	}

	if (howManyTimes > 0)
	{
		// Clear the interval in case this marker was clicked again while it
		// was still blinking.  Since setInterval returns a new Id each time
		// it's called, we need to make sure the old interval is clear before
		// its Id is replaced with the new Id.
		this.blinkIntervalId = setInterval("maClient.map.blinkMarker(" + this.viewId + ");", 50);
		//console.log("setBlink:setInterval " + this.blinkIntervalId + " for " + this.viewId);
		this.blinkDirection = -1;
		this.blinkLimit = howManyTimes;
	}
	else
	{
		this.setBlinkAlpha(1.0);
	}

	this.blinkCount = 0;
};

maMarkerInstance.prototype.setBlinkAlpha = function(alpha)
{
	this.blinkAlpha = alpha;
	this.map.redrawMarker(this, this.viewId == this.map.selectedViewId);
    maClient.map.refreshMap('setBlinkAlpha');
};

function maMarkerStyle(
	id,
	lineWidth,
	normalFillColor,
	normalLineColor,
	selectedFillColor,
	selectedLineColor,
	normalFillColorOpacity,
	normalLineColorOpacity,
	selectedFillColorOpacity,
	selectedLineColorOpacity,
	normalShapeEffects,
	selectedShapeEffects)
{
	this.id = id;
	this.lineWidth = lineWidth;
	this.normalFillColor = normalFillColor;
	this.normalLineColor = normalLineColor;
	this.selectedFillColor = selectedFillColor;
	this.selectedLineColor = selectedLineColor;
	this.normalFillColorOpacity = normalFillColorOpacity;
	this.normalLineColorOpacity = normalLineColorOpacity;
	this.selectedFillColorOpacity = selectedFillColorOpacity;
	this.selectedLineColorOpacity = selectedLineColorOpacity;
	this.normalShapeEffects = normalShapeEffects;
	this.selectedShapeEffects = selectedShapeEffects;
}

maMarkerStyle.prototype.cloneMarkerStyle = function()
{
	var markerStyle = new maMarkerStyle(
			this.id,
			this.lineWidth,
			this.normalFillColor,
			this.normalLineColor,
			this.selectedFillColor,
			this.selectedLineColor,
			this.normalFillColorOpacity,
			this.normalLineColorOpacity,
			this.selectedFillColorOpacity,
			this.selectedLineColorOpacity,
			this.normalShapeEffects,
			this.selectedShapeEffects);

	return markerStyle;
};

function maRouteDefinition(markerInstance, markerIdList)
{
	this.markerInstance = markerInstance;
	this.markerIdList = markerIdList;

	var minX = Number.MAX_VALUE;
	var maxX = Number.MIN_VALUE;
	var minY = Number.MAX_VALUE;
	var maxY = Number.MIN_VALUE;
	
	// Make sure the list is a string. If a single numeric element was passed, convert it to a string.
	markerIdList = markerIdList + "";
	var sections = markerIdList.split(";");
	var coords = new Array();

	for (var i in sections)
	{
		// Prepend a section start indicator to each section after the first one.
		coords.push(-1);
		coords.push(4);

		// Create an array of marker Ids for each semicolon-separated section.
		var markers = new Array();
		if (markerIdList.length > 0)
		{
			var list = sections[i].split(",");
			for (var j in list)
			{
				// Get the marker for this Id. Ignore route markers since we don't draw through them.
				var m = this.markerInstance.map.markerInstance(list[j]);
				if (m !== null && !m.isRoute)
				{
					markers.push(m);
				}
			}
		}

		// Create an x,y pair from each marker in the section.
		for (var k in markers)
		{
			var marker = markers[k];
			coords.push(marker.x);
			coords.push(marker.y);

			if (marker.x < minX)
				minX = marker.x;
			if (marker.x > maxX)
				maxX = marker.x;
			if (marker.y < minY)
				minY = marker.y;
			if (marker.y > maxY)
				maxY = marker.y;
		}
	}
	
	// Determine the bounding box and center point for this route.
	this.markerInstance.shapeW = maxX - minX;
	this.markerInstance.maxWidth = this.markerInstance.shapeW;
	this.markerInstance.shapeH = maxY - minY;
	this.markerInstance.maxHeight = this.markerInstance.shapeH;
	this.markerInstance.x = minX + (markerInstance.maxWidth / 2);
	this.markerInstance.y = minY + (markerInstance.maxHeight / 2);

	// Shift the coordinates so that the upper left corner of the route is at 0,0.
	for (var index = 0; index < coords.length; index += 2)
	{
		if (coords[index] == -1)
			continue;
		coords[index] -= (minX + 0.5);
		coords[index + 1] -= (minY + 0.5);
	}

	// Convert the coords array to a comma separated list.
	var points = coords.join(",");
	markerInstance.shapeCoords = points;
}


// Event Handlers
// ----------------------------------------------------------------------------
maClient.Html5.prototype.onClick = function(event)
{
	// Simulate a touch event in case the map is being displayed on a touch screen monitor
	// (as opposed to a touch device). On a touch screen we might not get a mouseover event
	// and if not, the marker won't get selected before being clicked. If using a mouse,
	// this causes an extra touch event to get handled, but it will be treated as though the
	// mouse moved over and stayed on the same marker.
	maClient.map.onTouch(event);

	var originalViewId = maClient.map.selectedViewId;
	var clickedViewId = maClient.map.getMarkerUnderMouse();

	console.log("onClick " + originalViewId + " " + clickedViewId);

	if (clickedViewId === 0)
	{
		return;
	}

	var markerInstance = maClient.map.markerInstance(clickedViewId);
	if (markerInstance === null)
	{
		// This can happen if the first hotspot is not on the map.
		return;
	}

	if (markerInstance.isDisabled)
	{
		return;
	}

	if (markerInstance.clickActionTarget.length > 0)
	{
		maClient.map.executeClickAction(markerInstance);
	}

	if (markerInstance.showContentEvent == maShowContentOnClick)
	{
		maClient.map.switchView(markerInstance, false);
	}

	if (maTour.usesPopup && (maClient.popupIsPinned || maTour.popup.pinOnClick))
	{
		var pinnedPopupWasClicked = maClient.popupIsPinned && originalViewId == clickedViewId;

		maClient.map.deselectMarker();
		maClient.map.switchView(markerInstance, false);

		if (!pinnedPopupWasClicked && maTour.popup.pinOnClick)
		{
			maClient.flashPinnableMarkerClicked();
		}
	}
};

maClient.Html5.prototype.onMapTileImageDataLoaded = function()
{
	console.log("onMapTileImageDataLoaded");
	maClient.map.mapTileImageDataLoaded = true;
};

maClient.Html5.prototype.onMouseDown = function(event)
{
	if (!maClient.map.mapIsZoomed())
		return;

	maClient.map.touchStartX = event.pageX;
	maClient.map.touchStartY = event.pageY;
	maClient.map.touchDeltaX = 0;
	maClient.map.touchDeltaY = 0;

	maClient.map.dragging = true;

	// On Safari and Chrome the cursor is the iBeam instead. We don't know why.
	maClient.map.markerLayer.style.cursor = 'move';
};

maClient.Html5.prototype.onMouseUp = function(event)
{
	// This handler is called for mouseout also. If the mouse leaves the map, we end panning.
	// If we don't and the user mouses down and then drags outside of the map, then mouses up
	// and moves back over the map, we would still be panning because the map would not have
	// gotten the mouseup event.

	maClient.map.dragging = false;

	// On Opera the cursor just stays whatever is was. We also tried setting it to an arrow here and that didn't help.
	maClient.map.markerLayer.style.cursor = 'auto';
};

maClient.Html5.prototype.onResize = function(event)
{
	maClient.map.mapLocationInBrowser = maClient.tagLocation(null, "maMarkerLayer");
};

maClient.Html5.prototype.onSymbolImageDataLoaded = function()
{
	console.log("onSymbolImageDataLoaded");
	maClient.map.symbolImageDataLoaded = true;
};

maClient.Html5.prototype.onTouch = function(event)
{
	var map = maClient.map;
	var js;

	if (!maClient.mapLoaded)
	{
		console.log(">>> Map not loaded");
		return;
	}
	
	if (map.dragging)
	{
		var deltaX = event.pageX - maClient.map.touchStartX;
		var deltaY = event.pageY - maClient.map.touchStartY;

		maClient.map.touchStartX = event.pageX;
		maClient.map.touchStartY = event.pageY;
		maClient.map.touchDeltaX = deltaX;
		maClient.map.touchDeltaY = deltaY;
		maClient.map.handlePanEvent();
		return;
	}

	if (maClient.isTouchDevice)
	{
		// Any touch stops the slide show.
		map.stopSlideShow();
	}

	if (!maClient.isTouchDevice && maClient.popupIsPinned)
	{
		// Ignore mouse overs when popup is pinned.
		return;
	}

	if (!map.getMouseLocation())
	{
		// Ignore mouse events that are off the canvas.
		// console.log("Mouse is not over the canvas");
		return;
	}

	var viewId = map.getMarkerUnderMouse();

	if (map.mapCanZoom)
	{
		// Prevent a double tap from zooming the entire browser window in or out.
		event.preventDefault();
	}

	if (map.ignoreTouch)
	{
		console.log("IGNORE onTouch during delay");
		return;
	}

	if (viewId === 0 && map.selectedViewId === 0)
	{
		// The mouse is not over a marker and no marker is currently selected.
		// This is just a mouse move or touch over the map so we ignore it.
		//console.log("== not over a marker");
		map.markerLayer.style.cursor = "auto";
		return;
	}

	//console.log("onTouch " + map.selectedViewId + " " + viewId + " " + map.mouseX + "," + map.mouseY);

	var selectNewMarker = false;
	var markerInstance;

	if (viewId === 0)
	{
		// This option is not currently available to users, but we might want to add it so
		// that you don't have to move your finger to the X in order to close the popup.
		var closePopupWhenClickOnMap = false;

		//console.log("== moused off of a marker");

		if (maTour.usesPopup)
		{
			if (closePopupWhenClickOnMap || !maClient.isTouchDevice)
			{
				// Deselect the currently selected marker.
				map.markerLayer.style.cursor = "auto";
				map.deselectMarker();
				maClient.flashMarkerMouseOut();
			}
		}
		else if (!maClient.isTouchDevice && map.mouseIsOverMarker)
		{
			// Execute the mouse out action.
			markerInstance = map.markerInstance(map.selectedViewId);
			js = markerInstance.mouseoutAction == maActionCallJavascript ? markerInstance.mouseoutActionTarget : "";
			if (js.length > 0)
			{
				console.log("== perform mouse out action");
				map.mouseIsOverMarker = false;
				maClient.flashExecuteJavaScript(js);
			}
		}
	}
	else
	{
		map.stopSlideShow();

		markerInstance = map.markerInstance(viewId);

		if (!maClient.isTouchDevice && markerInstance.showContentEvent == maShowContentOnClick)
		{
			// This is a desktop browser and the marker shows its content on click.
			// Ignore this mouseover, but show the marker's tooltip if it has one.
			map.showTooltip(markerInstance);
			return;
		}

		if (map.selectedViewId === 0)
		{
			// Select a marker when no marker is selected.
			// console.log("== moused onto a marker " + viewId);
			map.mouseIsOverMarker = true;
			selectNewMarker = true;
			if (!maClient.isTouchDevice && markerInstance.showContentEvent != maShowContentOnMouseover)
			{
				// The user moused onto a marker that does not show its content on mouseover. Because we are
				// on a desktop browser, the marker won't get selected so we show its tooltip. On a touch
				// device, the marker will get selected because content is always shown on touch
				map.showTooltip(markerInstance);
			}
		}
		else if (map.selectedViewId !== viewId)
		{
			// Mouse off of one marker onto another.
			// console.log("== from one marker onto another " + map.selectedViewId + " : " + viewId);
			map.deselectMarker();
			map.mouseIsOverMarker = true;
			selectNewMarker = true;

			// Close a pinned popup now in case the new marker only displays a tooltip.
			if (this.popupIsShowing)
			{
				maClosePopup();
			}
		}
		else
		{
			// console.log("== staying on same marker " + viewId);
			markerInstance.setBlink(0);

			if (!map.mouseIsOverMarker)
			{
				js = markerInstance.mouseoverAction == maActionCallJavascript ? markerInstance.mouseoverActionTarget : "";
				if (js.length > 0)
				{
					console.log("== perform mouse over action");
					maClient.flashExecuteJavaScript(js);
				}
			}

			map.mouseIsOverMarker = true;

			if (markerInstance.clickActionTarget.length > 0 && markerInstance.touchPerformsClickAction && maClient.isTouchDevice)
			{
				maClient.map.executeClickAction(markerInstance);
			}
			else if (markerInstance.doesNotShowContent || !maTour.usesDynamicPopup)
			{
				// Make the tooltip appear again if the user touches or mouses over the selected marker.
				map.showTooltip(markerInstance);
			}
		}
	}

	map.markerLayer.style.cursor = viewId === 0 ? "auto" : "pointer";

	if (selectNewMarker && !markerInstance.isDisabled)
	{
		markerInstance.setBlink(0);
		map.switchView(markerInstance, false);
	}

	maClient.map.refreshMap('onTouch');
};

maClient.Html5.prototype.onGestureStart = function(event)
{
	//console.log("onGestureStart");
	
	maClient.map.gestureStarted = true;
};

maClient.Html5.prototype.onGestureEnd = function(event)
{
	//console.log("onGestureEnd");

	if (maClient.map.mapZoomDelta !== 0)
	{
		maClient.map.setMapZoomInOut(maClient.map.mapZoomDelta);
		maClient.map.startTouchDelay();
	}

	maClient.map.gestureChanged = false;
};

maClient.Html5.prototype.onGestureChange = function(event)
{
	if (maClient.map.mapCanZoom)
	{
		event.preventDefault();
	}

	if (maClient.map.gestureChanged)
	{
		//console.log("IGNORE onGestureChange");
		return;
	}
	else
	{
		//console.log("onGestureChange");
		maClient.map.gestureChanged = true;

		if (event.scale < 1.0)
		{
			maClient.map.mapZoomDelta = -25;
		}
		else if (event.scale > 1.0)
		{
			maClient.map.mapZoomDelta = 25;
		}
	}
};

maClient.Html5.prototype.onTouchStart = function(event)
{
	//console.log("onTouchStart " + event.touches.length);
	maClient.map.touchStartX = event.touches[0].pageX;
	maClient.map.touchStartY = event.touches[0].pageY;
	maClient.map.touchDeltaX = 0;
	maClient.map.touchDeltaY = 0;
};

maClient.Html5.prototype.onTouchMove = function(event)
{
	if (maClient.map.mapCanZoom)
	{
		event.preventDefault();
	}

	if (maClient.map.gestureStarted || !maClient.map.mapIsZoomed())
	{
		//console.log("IGNORE onTouchMove");
		return;
	}

	var deltaX = event.targetTouches[0].pageX - maClient.map.touchStartX;
	var deltaY = event.targetTouches[0].pageY - maClient.map.touchStartY;

	maClient.map.touchStartX = event.targetTouches[0].pageX;
	maClient.map.touchStartY = event.targetTouches[0].pageY;

	if (maTour.disableSmoothPanning)
	{
		maClient.map.touchDeltaX += deltaX;
		maClient.map.touchDeltaY += deltaY;
	}
	else
	{
		maClient.map.touchDeltaX = deltaX;
		maClient.map.touchDeltaY = deltaY;
		maClient.map.handlePanEvent();
	}
};

maClient.Html5.prototype.onTouchEnd = function(event)
{
	//console.log("onTouchEnd " + event.touches.length);

	maClient.map.gestureStarted = event.touches.length === 0 ? false : true;

	if (maTour.disableSmoothPanning)
	{
		maClient.map.handlePanEvent();
	}
};

maClient.Html5.prototype.onTouchCancel = function(event)
{
	console.log("onTouchCancel " + event.touches.length);
};


// Class Methods
// ----------------------------------------------------------------------------

maClient.Html5.prototype.changeMarkerShapeAppearance = function(viewIdList, selected, lineColor, lineAlpha, fillColor, fillAlpha, effects)
{
	console.log("changeMarkerShapeAppearance " + viewIdList + " : " + selected + " " + this.convertIntegerColorToHex(fillColor));

	var markers = this.createMarkerArrayFromMarkerIdList(viewIdList);

	var fillColorCss = this.convertIntegerColorToCss(fillColor);
	var lineColorCss = this.convertIntegerColorToCss(lineColor);

	// Loop over all the markers and for each one, see if its in the view Id list.
	// If it is, then redraw it. This algorithm causes all the markers in the list to be
	// redrawn in their correct stacking order.

	for (index in this.markerInstances)
	{
		markerInstance = this.markerInstances[index];

		var redraw = false;

		for (var i in markers)
		{
			var m = markers[i];

			if (m.viewId == markerInstance.viewId)
			{
				var markerStyle;
				if (m.customMarkerStyle === null)
				{
					// This marker instance does not have a custom marker style yet so create one.
					markerStyle = m.markerStyle().cloneMarkerStyle();
					m.customMarkerStyle = markerStyle;
				}
				else
				{
					// Get the marker's existing custom marker style.
					markerStyle = m.customMarkerStyle;
				}

				// Update the custom marker style with the requested appearance.
				if (selected)
				{
					markerStyle.selectedFillColor = fillColorCss;
					markerStyle.selectedFillColorOpacity = fillAlpha;
					markerStyle.selectedLineColor = lineColorCss;
					markerStyle.selectedLineColorOpacity = lineAlpha;
					markerStyle.selectedShapeEffects = effects;
				}
				else
				{
					markerStyle.normalFillColor = fillColorCss;
					markerStyle.normalFillColorOpacity = fillAlpha;
					markerStyle.normalLineColor = lineColorCss;
					markerStyle.normalLineColorOpacity = lineAlpha;
					markerStyle.normalShapeEffects = effects;
				}

				redraw = true;
				break;
			}
		}

		if (redraw)
		{
			var drawSelected = (markerInstance.viewId == this.selectedViewId && !markerInstance.isStatic) || markerInstance.appearsSelected;
			this.redrawMarker(markerInstance, drawSelected);
		}
	}

    maClient.map.refreshMap('changeMarkerShapeAppearance');
};

maClient.Html5.prototype.convertHexColorToRgb = function(hex)
{
	//console.log("HEX " + hex);
	var r = 0;
	var g = 0;
	var b = 0;
	if (hex.length == 8)
	{
		if (hex.substr(0, 2) == "0x")
		{
			r = parseInt(hex.substr(2, 2), 16);
			if (isNaN(r))
				r = 0;
			g = parseInt(hex.substr(4, 2), 16);
			if (isNaN(g))
				g = 0;
			b = parseInt(hex.substr(6, 2), 16);
			if (isNaN(b))
				b = 0;
		}
	}
	var color = new Object();
	color.r = r;
	color.g = g;
	color.b = b;
	return color;
};

maClient.Html5.prototype.convertIntegerColorToHex = function(integerColor)
{
	if ((integerColor + "  ").substr(0, 2).toLowerCase() == "0x")
	{
		// The integer value was passed in as a string like "0xaabbcc".
		return integerColor;
	}
	
	// Make sure the value is an integer (it might be an integer passed as a string).
	integerColor = parseInt(integerColor, 10);

	var hex = integerColor.toString(16);
	var pad = "00000";
	if (hex.length < 6)
	{
		hex = pad.substring(0, 6 - hex.length) + hex;
	}
	return "0x" + hex;
};

maClient.Html5.prototype.convertIntegerColorToCss = function(integerColor)
{
	if ((integerColor + " ").substr(0, 1) == "#")
	{
		// The integer value was passed in as a CSS color like "#aabbcc".
		return integerColor;
	}
	
	var hex = this.convertIntegerColorToHex(integerColor);
	return "#" + hex.substr(2);
};

maClient.Html5.prototype.createMarkerArrayFromMarkerIdList = function(markerIdList)
{
	// Make sure the list is a string. If a single numeric element was passed, convert it to a string.
	markerIdList = markerIdList + "";

	var markers = new Array();
	if (markerIdList.length > 0)
	{
		var markerInstance;
		var index;

		if (markerIdList == "*")
		{
			for (index in this.markerInstances)
			{
				markerInstance = this.markerInstances[index];
				markers.push(markerInstance);
			}
		}
		else
		{
			var list = markerIdList.split(",");
			for (index in list)
			{
				markerInstance = this.markerInstance(list[index]);
				if (markerInstance !== null)
				{
					markers.push(markerInstance);
				}
			}
		}
	}
	return markers;
};

maClient.Html5.prototype.createMarkerInstances = function()
{
	this.waitForMarkerImagesAttempts = 0;
	this.markerInstances = new Array(maTour.markerInstanceTable.length);

	for (var index = 0; index < maTour.markerInstanceTable.length; index++)
	{
		var data = maTour.markerInstanceTable[index].split(',');
		var viewId = data[0];
		var markerType = parseInt(data[1], 10);
		var markerStyleId = parseInt(data[2], 10);
		var normalSymbolId = parseInt(data[3], 10);
		var selectedSymbolId = parseInt(data[4], 10);
		var pctX = parseFloat(data[5]);
		var pctY = parseFloat(data[6]);
		var shapeType = parseInt(data[7], 10);
		var shapeW = parseInt(data[8], 10);
		var shapeH = parseInt(data[9], 10);
		var normalSymbolW = parseInt(data[10], 10);
		var normalSymbolH = parseInt(data[11], 10);
		var selectedSymbolW = parseInt(data[12], 10);
		var selectedSymbolH = parseInt(data[13], 10);
		var symbolLocationX = parseInt(data[14], 10);
		var symbolLocationY = parseInt(data[15], 10);
		var anchorX = parseInt(data[16], 10);
		var anchorY = parseInt(data[17], 10);
		var rotation = parseInt(data[18], 10);
		var zoomThreshold = parseInt(data[19], 10);
		var flags = parseInt(data[20], 10);
		var tooltip = maClient.lookupString(data[21]);
		var clickAction = parseInt(data[22], 10);
		var clickActionTarget = maClient.lookupString(data[23]);
		var mouseoverAction = parseInt(data[24], 10);
		var mouseoverActionTarget = maClient.lookupString(data[25]);
		var mouseoutAction = parseInt(data[26], 10);
		var mouseoutActionTarget = maClient.lookupString(data[27]);
		var touchPerformsClickAction = parseInt(data[28], 10);
		var showContentEvent = parseInt(data[29], 10);
		var shapeCoords = maClient.lookupString(data[30]);

		var x = Math.round(this.mapWidth * pctX);
		var y = Math.round(this.mapHeight * pctY);

		var markerInstance = new maMarkerInstance(
			this,
			viewId,
			markerType,
			markerStyleId,
			normalSymbolId,
			selectedSymbolId,
			pctX,
			pctY,
			x,
			y,
			shapeType,
			shapeW,
			shapeH,
			normalSymbolW,
			normalSymbolH,
			selectedSymbolW,
			selectedSymbolH,
			symbolLocationX,
			symbolLocationY,
			anchorX,
			anchorY,
			rotation,
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
		this.markerInstances[index] = markerInstance;

		// Get the marker's effects in order to establish it's initial clipping bounds.
		var markerStyle = markerInstance.markerStyle();
		if (markerStyle !== null)
		{
			this.getEffects(markerInstance, markerStyle.normalShapeEffects, null, false);
			this.getEffects(markerInstance, markerStyle.selectedShapeEffects, null, true);
		}
	}
};

maClient.Html5.prototype.blinkMarker = function(viewId)
{
	var markerInstance = this.markerInstance(viewId);

	var pulsesPerCycle = 5;
	var cycle = markerInstance.blinkCount % pulsesPerCycle;

	if (cycle === 0)
	{
		// Change the cycle direction from increasing to descreasing or vice-versa.
		markerInstance.blinkDirection *= -1;

		if (markerInstance.blinkDirection == -1 && markerInstance.blinkCount / (pulsesPerCycle * 2) >= markerInstance.blinkLimit - 1)
		{
			//console.log("TURN OFF BLINK " + markerInstance.blinkCount + "," + markerInstance.blinkLimit + "," + markerInstance.viewId);
			markerInstance.setBlink(0);
			return;
		}
	}

	// Create a two-cycle pattern of 0123443210.
	if (markerInstance.blinkDirection == -1)
		cycle = pulsesPerCycle - cycle - 1;

	// Set the alpha value within the cycle such that the marker "blinks" like an incandescent  
	// lamp.  It does not go on/off instantly -- there is a slight fade between fully on and fully off.
	var alpha;
	if (cycle === 0)
		alpha = 10;
	else if (cycle == 1)
		alpha = 20;
	else if (cycle == 2)
		alpha = 80;
	else
		alpha = 100;

	//console.log("blink = " + cycle + " : " + alpha);

	markerInstance.setBlinkAlpha(alpha / 100);
	markerInstance.blinkCount++;
};

maClient.Html5.prototype.createMarkerStyles = function()
{
	this.markerStyles = new Object();

	for (index = 0; index < maTour.markerStyleTable.length; index++)
	{
		var data = maTour.markerStyleTable[index].split(',');
		var id = parseInt(data[0], 10);
		var lineWidth = parseInt(data[1], 10);
		var normalFillColor = data[2];
		var normalLineColor = data[3];
		var selectedFillColor = data[4];
		var selectedLineColor = data[5];
		var normalFillColorOpacity = parseInt(data[6], 10);
		var normalLineColorOpacity = parseInt(data[7], 10);
		var selectedFillColorOpacity = parseInt(data[8], 10);
		var selectedLineColorOpacity = parseInt(data[9], 10);
		var normalShapeEffects = maClient.convertEffects(maClient.lookupString(data[10]));
		var selectedShapeEffects = maClient.convertEffects(maClient.lookupString(data[11]));
		
		// See comment on disableBlendingOnChrome flag.
		if (maClient.disableBlendingOnChrome)
		{
			// To counteract the impact of no blending, reduce the opacity to ensure that
			// you can still see through any marker, especially ones that are opaque.
			normalFillColorOpacity = parseInt(normalFillColorOpacity / 2);
			normalLineColorOpacity = parseInt(normalLineColorOpacity / 2);
			selectedFillColorOpacity = parseInt(selectedFillColorOpacity / 2);
			selectedLineColorOpacity = parseInt(selectedLineColorOpacity / 2);
		}

		var markerStyle = new maMarkerStyle(
			id,
			lineWidth,
			normalFillColor,
			normalLineColor,
			selectedFillColor,
			selectedLineColor,
			normalFillColorOpacity,
			normalLineColorOpacity,
			selectedFillColorOpacity,
			selectedLineColorOpacity,
			normalShapeEffects,
			selectedShapeEffects);

		this.markerStyles[id] = markerStyle;
	}
};

maClient.Html5.prototype.createRgbaFromHexColor = function(hexColor, alpha)
{
	var c = this.convertHexColorToRgb(hexColor);
	return "rgba(" + c.r + "," + c.g + "," + c.b + "," + alpha + ")";
};

maClient.Html5.prototype.createRgbaFromIntegerColor = function(integerColor, alpha)
{
	var hexColor = this.convertIntegerColorToHex(integerColor);
	return this.createRgbaFromHexColor(hexColor, alpha);
};

maClient.Html5.prototype.deselectMarker = function()
{
	if (this.selectedViewId === 0)
		return;

	//console.log("deselectMarker");

	var markerInstance = this.markerInstance(this.selectedViewId);

	if (markerInstance === null)
	{
		// This can happen if the marker for the tour's first tour view is not on the map.
		// The missing marker is the selected view Id, but it has not marker instance.
		return;
	}

	markerInstance.setBlink(0);
	markerInstance.visited = true;

	var js = markerInstance.mouseoutAction == maActionCallJavascript ? markerInstance.mouseoutActionTarget : "";
	if (js.length > 0)
	{
		maClient.flashExecuteJavaScript(js);
	}

	markerInstance.appearsSelected = false;

	if (!markerInstance.isStatic)
	{
		this.redrawMarker(markerInstance, false);
	}

	this.selectedViewId = 0;
};

maClient.Html5.prototype.drawAllMarkers = function()
{
	var viewId;
	var markerInstance;

	if (!this.allMarkerImagesLoaded && this.waitForMarkerImagesAttempts < 100)
	{
		this.waitForMarkerImagesAttempts++;
		//console.log("MARKER IMAGE ATTEMPT " + this.waitForMarkerImagesAttempts);

		// Determine if all the marker images are loaded. If not, try again after a short delay.
		for (index in this.markerInstances)
		{
			markerInstance = this.markerInstances[index];

			// Note that a marker's image is considered loaded if it has no image.
			var loadedN = !markerInstance.hasImageN || markerInstance.imgLoadedN;
			var loadedS = !markerInstance.hasImageS || markerInstance.imgLoadedS;

			if (!loadedN || !loadedS)
			{
				setTimeout("maClient.map.drawAllMarkers();", 50);
				return;
			}
		}
		this.allMarkerImagesLoaded = true;
	}

	// console.log("drawAllMarkers");

	// All the images are loaded. Draw the markers.
	this.markerLayerContext.clearRect(0, 0, this.viewPort.w, this.viewPort.h);

	for (index in this.markerInstances)
	{
		markerInstance = this.markerInstances[index];
		markerInstance.computeMarkerLocationValues();
		this.drawMarker(markerInstance, markerInstance.viewId == this.selectedViewId);
	}
};

maClient.Html5.prototype.drawCircle = function(markerInstance, x, y, radius, ctx, fillAlpha, lineAlpha)
{
	//console.log("drawCircle " + markerInstance.viewId + " " + x + "," + y);
	ctx.beginPath();
	ctx.arc(x, y, radius, 0, Math.PI * 2, true);
	ctx.closePath();
	this.markerLayerContext.globalAlpha = fillAlpha;
	ctx.fill();
	this.markerLayerContext.globalAlpha = lineAlpha;
	ctx.stroke();
};

maClient.Html5.prototype.drawHybridShape = function(markerInstance, ctx, fillAlpha, lineAlpha, lineWidth)
{
	var coord = markerInstance.shapeCoords.split(",");

//	if (ctx !== null && ctx !== this.hitLayerContext)
//	{
//		console.log("drawHybridShape: " + coord);
//	}

	var i = 0;
	while (i < coord.length)
	{
		// The first pair of the coordinates indicates what type of shape to draw.
		// An x value of -1 means section start. The y value has the shape type.
		if (coord[i] == "-1")
		{
			var shapeType = coord[i + 1];

			//console.log("Draw Section " + i + " " + shapeType);

			var isCircle = shapeType == "1";
			var isRectangle = shapeType == "2";
			var isPolygon = shapeType == "3";
			var isLine = shapeType == "4";
			var drawFill = isRectangle || isCircle || isPolygon;

			i += 2;

			if (isCircle)
			{
				var x = parseInt(coord[i], 10);
				var y = parseInt(coord[i + 1], 10);
				var r = parseInt(coord[i + 2], 10);

				// Move past the center point, radius, and unused "y" value after the radius.
				if (i + 3 >= coord.length || coord[i + 3] == "-1")
				{
					// These are old V2 circle coords where the dummy 4th value is missing.
					// Determine if the circle is for a circle shape or is a circle within a hybrid
					// shape. If it's a circle there will be a total of 5 coords: -1,1,x,y,r.
					if (coord.length > 5)
					{
						// This is a circle within a hybrid shape. Adjust the center point for V3.
						var offset = lineWidth / 2;
						x += r + offset;
						y += r + offset;
					}
					i += 3;
				}
				else
				{
					// These are the expected coords.
					i += 4;
				}

				x += markerInstance.offsetX();
				y += markerInstance.offsetY();
				this.drawCircle(markerInstance, x, y, r, ctx, fillAlpha, lineAlpha);
				//console.log("drawCircleShape " + x + "," + y + " " + r);
			}
			else if (isRectangle)
			{
				// Note that when drawing a rectangle using vector graphics, you need to draw
				// from the origin to the width, not width -1 as you would with a bitmap.
				// Our rectangle coordinates are non-vector so we have to add 1 to the width and height. 
				var x1 = parseInt(coord[i], 10);
				var y1 = parseInt(coord[i + 1], 10);
				var x2 = parseInt(coord[i + 2], 10) + 1;
				var y2 = parseInt(coord[i + 3], 10) + 1;


				if (lineWidth % 2 == 1)
				{
					// The line has an odd number of pixels. When drawing rectangles, we want odd
					// pixels drawn outside the rectangle and even pixels inside. To accomplish this
					// we have to increase the width by a pixel to counteract the fact that Flash
					// draws odd pixels to the left of vertical lines and above horizontal lines.
					// For a rectangle that means that the odd pixes are drawn outside on the left
					// side and inside on the right side (outside on the top and inside on the bottom).
					// That's very confusing an unintuitive. It also means that when you add a 1 pixel
					// border to a 100x100 rectangle it ends up being 101x101. Our adjustment here
					// makes the rectangle be 102x102 as you would expect.
					x2++;
					y2++;
				}

				i += 4;

				//console.log("drawRectangle " + x1 + "," + y1 + " " + x2 + "," + y2);
				var w = x2 - x1;
				var h = y2 - y1;
				x = markerInstance.offsetX() + x1;
				y = markerInstance.offsetY() + y1;

				this.drawRectangle(markerInstance, x, y, w, h, ctx, fillAlpha, lineAlpha);
			}
			else if (isPolygon || isLine)
			{
				var length = this.drawPoints(markerInstance, i, ctx, fillAlpha, lineAlpha, isPolygon);
				i += length;
			}
		}
		else
		{
			console.log(">>> Unexpected coords at " + i + " = " + coord[i]);
			return;
		}
	}
};

maClient.Html5.prototype.drawLoadingMessage = function(msg)
{
	if (msg == this.loadingMessage)
	{
		// Don't spend execution time if the message has not changed.
		return;
	}

	this.loadingMessage = msg;

	// Erase any previous message.
	this.statusLayerContext.clearRect(0, 0, this.viewPort.w, this.viewPort.h);

	if (msg === null)
	{
		// Null means use the default message.
		msg = "Loading " + maTour.pageName;
	}

	if (msg.length === 0)
	{
		// Empty string means erase the message.
		return;
	}

	// Set the appearanace of the message.
	var pts = 16;
	this.statusLayerContext.font = pts + "px sans-serif";
	this.statusLayerContext.fillStyle = "#cccccc";

	// Horizontally center the text and place it near the top of the map.
	var textWidth = this.statusLayerContext.measureText(msg).width;
	var msgX = Math.round((this.viewPort.w / 2) - (textWidth / 2));
	var msgY = 100;

	// Draw the message.
	this.statusLayerContext.fillText(msg, msgX, msgY);
};

maClient.Html5.prototype.drawMap = function()
{
	// console.log("drawMap " + this.mapPanX + "," + this.mapPanY);

	// Get the map level object for the current zoom level.
	var mapLevel = this.mapLevels[this.mapIsZoomed() ? 1 : 0];

	var w = 256;
	var h = 256;
	for (var row = 1; row <= mapLevel.lastRow; row++)
	{
		for (var column = 1; column <= mapLevel.lastColumn; column++)
		{
			index = (((row - 1) * mapLevel.lastColumn) + column) - 1;
			tile = mapLevel.tiles[index];
			
			// Determine where the tile should be drawn.
			var x = w * (column - 1);
			var y = h * (row - 1);
			x -= this.viewPort.x1;
			y -= this.viewPort.y1;
			
			if (x + w < 0 || x > this.viewPort.w || y + h < 0 || y > this.viewPort.h)
			{
				// No part of this tile is visible in the view port so skip it.
				continue;
			}

			// Draw the entire tile. Note that tiles around the left and top edges can have
			// negative x and you offsets and tiles around the right and bottom edges can
			// extend beyond the bounds of the canvas. We tried creating a clipping region in
			// these cases to see if it would improve performance, but we didn't see any
			// difference. I expect that the hardware is smart enough to know not to draw
			// pixels that are off the canvas.
			this.mapLayerContext.drawImage(tile.image, x, y);
			//console.log("DRAW TILE AT " + x + "," + y);
		}
	}

	if (this.mapIsZoomed())
	{
		this.drawMapInset();
	}

	this.drawAllMarkers();
	this.showZoomControls();

	// Erase the loading message;
	this.drawLoadingMessage("");
};

maClient.Html5.prototype.drawMapInset = function()
{
	if (maTour.mapInsetLocation === 0)
		return;

	var x;
	var y;
	var NAV_BORDER_WIDTH = 2;

	// Determine the map's longest dimension (width or height).
	var longDimension = maTour.mapWidth > maTour.mapHeight ? maTour.mapWidth : maTour.mapHeight;

	// Calculate the scale of the inset based on the long dimension.
	var mapInsetScale = maTour.mapInsetSize / longDimension;

	// Calculate the size of the map inset.
	var mapInsetW = Math.round(maTour.mapWidth * mapInsetScale);
	var mapInsetH = Math.round(maTour.mapHeight * mapInsetScale);

	// Calculate the location of the inset.
	switch (maTour.mapInsetLocation)
	{
		case 2:
			// Upper right
			x = this.viewPort.w - (mapInsetW + (NAV_BORDER_WIDTH * 2));
			y = 4;
			break;

		case 3:
			// Lower right
			x = this.viewPort.w - (mapInsetW + (NAV_BORDER_WIDTH * 2));
			y = this.viewPort.h - (mapInsetH + (NAV_BORDER_WIDTH * 2));
			break;

		case 4:
			// Lower left
			x = 4;
			y = this.viewPort.h - (mapInsetH + (NAV_BORDER_WIDTH * 2));
			break;

		default:
			break;
	}

	// Draw the inset. We shift x and y by half a pixel so that the border doesn't get anti-aliased.
	this.mapLayerContext.save();
	this.mapLayerContext.drawImage(this.mapInsetImage, x, y);
	this.mapLayerContext.lineWidth = 1.0;
	this.mapLayerContext.strokeStyle = "#777777";
	this.mapLayerContext.strokeRect(x - 0.5, y - 0.5, mapInsetW, mapInsetH);
	this.mapLayerContext.restore();

	// Calculate the offset and size of the zoom region.
	x += Math.round(-this.mapPanX * mapInsetScale) + 2;
	y += Math.round(-this.mapPanY * mapInsetScale) + 2;
	var zoomRegionW = Math.round(this.viewPort.w * mapInsetScale) - 4;
	var zoomRegionH = Math.round(this.viewPort.h * mapInsetScale) - 4;

	// Draw the zoom region.
	this.mapLayerContext.save();
	this.mapLayerContext.lineWidth = 1.0;
	this.mapLayerContext.globalAlpha = 0.50;
	this.mapLayerContext.fillStyle = maTour.mapInsetColor;
	this.mapLayerContext.fillRect(x, y, zoomRegionW, zoomRegionH);
	this.mapLayerContext.globalAlpha = 1.0;
	this.mapLayerContext.strokeStyle = "#777777";
	this.mapLayerContext.strokeRect(x - 0.5, y - 0.5, zoomRegionW, zoomRegionH);
	this.mapLayerContext.restore();
};

maClient.Html5.prototype.drawMarker = function(markerInstance, selected)
{
	if (!markerInstance.isVisibleAtCurrentZoomLevel)
	{
		return;
	}

	if (markerInstance.isRoute && markerInstance.routeDefinition === null)
	{
		// This marker is a route that has not been defined yet so ignore it.
		return;
	}

	var x = markerInstance.cornerX;
	var y = markerInstance.cornerY;
	var w = markerInstance.drawnMaxWidth;
	var h = markerInstance.drawnMaxHeight;
	if (x + w < 0 || x > this.viewPort.w || y + h < 0 || y > this.viewPort.h)
	{
		// No part of the marker is visible in the view port.
		//console.log("Ignore marker outside of viewport " + markerInstance.viewId);
		return;
	}

	if (markerInstance.isStatic)
	{
		selected = false;
	}

	if (markerInstance.appearsSelected)
	{
		selected = true;
	}

	if (markerInstance.markerType != maMarkerType_Symbol)
	{
		// Every marker has a shape except for symbol markers.
		this.drawMarkerShape(markerInstance, this.markerLayerContext, selected);
	}

	if (markerInstance.markerType != maMarkerType_Shape)
	{
		// Every marker has a symbol except for shape markers.
		this.drawMarkerSymbol(markerInstance, this.markerLayerContext, selected);
	}
};

maClient.Html5.prototype.drawMarkerShape = function(markerInstance, ctx, selected)
{
	//	if (ctx !== null && ctx != this.hitLayerContext)
	//	{
	//		console.log("drawMarkerShape " + markerInstance.viewId + " " + selected);
	//	}

	this.setDrawingSurface(markerInstance);

	// Get this marker's style.
	var markerStyle = markerInstance.customMarkerStyle;
	if (markerStyle === null)
	{
		markerStyle = markerInstance.markerStyle();
	}

	var fillAlpha = 1.0;
	
	var lineAlpha = 1.0;
	var lineWidth = markerInstance.lineWidth();
	if (ctx)
	{
		ctx.lineWidth = lineWidth;
	}

	var enlargeHitTestArea = ctx == this.hitLayerContext && (this.hitTestPaddingW > 0 || this.hitTestPaddingH > 0);

	var x = markerInstance.offsetX();
	var y = markerInstance.offsetY();

	x -= markerInstance.anchorX;
	y -= markerInstance.anchorY;

	if (this.debugDrawing && ctx === null)
	{
		// Draw the shape as a solid color.
		ctx = this.markerLayerContext;
		ctx.fillStyle = "cyan";
		fillAlpha = 0.1;
		ctx.strokeStyle = "yellow";
		lineAlpha = 1.1;
	}
	else if (ctx != this.hitLayerContext)
	{
		// Set the border and fill colors.
		this.markerLayerContext.fillStyle = selected ? markerStyle.selectedFillColor : markerStyle.normalFillColor;
		this.markerLayerContext.strokeStyle = selected ? markerStyle.selectedLineColor : markerStyle.normalLineColor;

		fillAlpha = (selected ? markerStyle.selectedFillColorOpacity : markerStyle.normalFillColorOpacity) / 100;
		lineAlpha = (selected ? markerStyle.selectedLineColorOpacity : markerStyle.normalLineColorOpacity) / 100;

		if (fillAlpha === 0.0 && lineAlpha === 0.0 && markerInstance.markerType != maMarkerType_Symbol && markerInstance.markerType != maMarkerType_SymbolAndShape)
		{
			// This marker is completely invisible so we can save a lot of time by not drawing and blending it.
			this.restoreDrawingSurface();
			return;
		}

		if (lineWidth === 0)
		{
			// On some browsers except Firefox, a border gets drawn even though the line width is zero.
			// To counteract this we set the line attributes the sames as for the file.
			lineAlpha = fillAlpha;
			this.markerLayerContext.strokeStyle = selected ? markerStyle.selectedFillColor : markerStyle.normalFillColor;
		}

		var effects = selected ? markerStyle.selectedShapeEffects : markerStyle.normalShapeEffects;
		this.getEffects(markerInstance, effects, ctx, selected);

		if (markerInstance.blinkAlpha < 1.0)
		{
			fillAlpha *= markerInstance.blinkAlpha;
			lineAlpha *= markerInstance.blinkAlpha;
		}
		else if (markerInstance.visited && this.visitedMarkerAlpha < 1.0)
		{
			fillAlpha *= this.visitedMarkerAlpha;
			lineAlpha *= this.visitedMarkerAlpha;
		}

		this.markerLayerContext.globalAlpha = fillAlpha;
	}

	if (markerInstance.shapeType == maShapeType_Circle)
	{
		var radius = Math.round(markerInstance.shapeW / 2);

		if (enlargeHitTestArea)
		{
			radius = (markerInstance.shapeW + this.hitTestPaddingW) / 2;
			x -= this.hitTestPaddingW / 2;
			y -= this.hitTestPaddingH / 2;
		}

		this.drawCircle(markerInstance, x + radius, y + radius, radius, ctx, fillAlpha, lineAlpha);
	}
	else if (markerInstance.shapeType == maShapeType_Polygon || markerInstance.shapeType == maShapeType_Line)
	{
		this.markerLayerContext.lineJoin = "bevel";
		this.drawPoints(markerInstance, 0, ctx, fillAlpha, lineAlpha, markerInstance.shapeType == maShapeType_Polygon);
	}
	else if (markerInstance.shapeType == maShapeType_Hybrid)
	{
		this.markerLayerContext.lineJoin = "bevel";
		this.drawHybridShape(markerInstance, ctx, fillAlpha, lineAlpha, lineWidth);
	}
	else if (markerInstance.shapeType == maShapeType_Rectangle || (markerInstance.markerType == maMarkerType_Symbol && ctx == this.hitLayerContext))
	{
		var w = markerInstance.shapeW;
		var h = markerInstance.shapeH;

		if (enlargeHitTestArea)
		{
			// We are hit testing on a small symbol or symbol+shape marker.
			// Draw the shape a little larger to make it easier to touch.
			w += this.hitTestPaddingW;
			h += this.hitTestPaddingH;
			x -= this.hitTestPaddingW / 2;
			y -= this.hitTestPaddingH / 2;
		}

		this.drawRectangle(markerInstance, x, y, w, h, ctx, fillAlpha, lineAlpha);
	}

	this.restoreDrawingSurface();

	// Apply the blend mode. Note that we must do this after restoring the drawing surface because
	// the call to blendOnto is also going to set and restore the drawing surface.
	if (ctx != this.hitLayerContext)
	{
		markerInstance.blendOnto();
	}
};

maClient.Html5.prototype.drawMarkerSymbol = function(markerInstance, ctx, selected)
{
	var hasImage = selected ? markerInstance.hasImageS : markerInstance.hasImageN;

	//console.log("drawMarkerSymbol " + markerInstance.viewId + " " + selected + " " + hasImage);

	if (!hasImage)
		return;

	this.setDrawingSurface(markerInstance);

	var img;
	var loaded;
	if (selected)
	{
		img = markerInstance.imgS;
		loaded = markerInstance.imgLoadedS;
	}
	else
	{
		img = markerInstance.imgN;
		loaded = markerInstance.imgLoadedN;
	}

	if (loaded)
	{
		var x;
		var y;

		var symbolW = selected ? markerInstance.selectedSymbolW : markerInstance.normalSymbolW;
		var symbolH = selected ? markerInstance.selectedSymbolH : markerInstance.normalSymbolH;

		if (markerInstance.markerType == maMarkerType_SymbolAndShape)
		{
			var symbolLocationX = markerInstance.symbolLocationX;
			var symbolLocationY = markerInstance.symbolLocationY;

			x = -Math.round(symbolW / 2);
			y = -Math.round(symbolH / 2);

			if (symbolLocationX >= 0 || symbolLocationY >= 0)
			{
				// The symbol is not centered in the shape.
				x = markerInstance.offsetX() + x + symbolLocationX;
				y = markerInstance.offsetY() + y + symbolLocationY;
			}
		}
		else if (markerInstance.markerType == maMarkerType_Symbol)
		{
			x = -Math.round(symbolW / 2);
			y = -Math.round(symbolH / 2);
		}
		else
		{
			x = markerInstance.offsetX();
			y = markerInstance.offsetY();

			var lineWidth = markerInstance.lineWidth();
			if (lineWidth > 1)
			{
				// Adjust the position of the symbol within its shape.
				lineWidth = Math.round((lineWidth - 1) / 2);
				x += lineWidth;
				y += lineWidth;
			}
		}

		x -= markerInstance.anchorX;
		y -= markerInstance.anchorY;

		// Set the symbol's opacity.
		var globalAlpha = 1.0;
		if (markerInstance.blinkAlpha < 1.0)
		{
			globalAlpha = markerInstance.blinkAlpha;
		}
		else if (markerInstance.visited && this.visitedMarkerAlpha < 1.0)
		{
			globalAlpha = this.visitedMarkerAlpha;
		}
		ctx.globalAlpha = globalAlpha;

		// Draw the symbol.
		ctx.drawImage(img, x, y);
	}

	this.restoreDrawingSurface();
};

maClient.Html5.prototype.drawPoints = function(markerInstance, index, ctx, fillAlpha, lineAlpha, isPolygon)
{
	var x;
	var y;

	// Calculate the offset without rounding so that the drawing code can work with fractional pixels.
	x = -(markerInstance.shapeW / 2);
	y = -(markerInstance.shapeH / 2);

	var length = 0;
	var coords = markerInstance.shapeCoords.split(",");

	ctx.beginPath();

	for (var i = index; i < coords.length; i += 2)
	{
		var cx = parseInt(coords[i], 10);
		var cy = parseInt(coords[i + 1], 10);
		//console.log("== " + i + " : " + cx + "," + cy);

		if (cx == -1)
		{
			break;
		}

		length += 2;

		cx += x;
		cy += y;

		if (i === 0)
		{
			ctx.moveTo(cx, cy);
		}
		else
		{
			ctx.lineTo(cx, cy);
		}
	}

	if (isPolygon)
	{
		ctx.globalAlpha = fillAlpha;
		ctx.closePath();
		ctx.fill();
	}

	ctx.globalAlpha = lineAlpha;
	ctx.stroke();

	return length;
};

maClient.Html5.prototype.drawRectangle = function(markerInstance, x, y, w, h, ctx, fillAlpha, lineAlpha)
{
	this.markerLayerContext.globalAlpha = fillAlpha;
	ctx.fillRect(x, y, w, h);
	this.markerLayerContext.globalAlpha = lineAlpha;
	ctx.strokeRect(x, y, w, h);
};

maClient.Html5.prototype.drawRouteThroughMarkers = function(viewId, lineWidth, lineColor, lineAlpha, markerIdList, effects)
{
	var lineColorCss = this.convertIntegerColorToCss(lineColor);

	console.log("drawRouteThroughMarkers " + viewId + " : " + markerIdList + " " + lineColorCss);

	var markerInstance = this.markerInstance(viewId);
	if (markerInstance === null)
		return false;

	if (markerInstance.routeDefinition !== null)
	{
		// Erase the previous route.
		markerInstance.customMarkerStyle.normalLineColorOpacity = 0;
		this.redrawMarker(markerInstance, false);
	}

	// The line width does not get scaled so make the number larger to compensate for the map scale.
	//lineWidth /= this.currentMapScale;

	markerInstance.routeDefinition = new maRouteDefinition(markerInstance, markerIdList);

	markerInstance.customMarkerStyle = new maMarkerStyle(
			0,
			lineWidth,
			"#000000",
			lineColorCss,
			"#000000",
			lineColorCss,
			0,
			lineAlpha,
			0,
			lineAlpha,
			effects,
			effects);

	markerInstance.computeMarkerLocationValues();
	this.redrawMarker(markerInstance, false);

    maClient.map.refreshMap('drawRouteThroughMarkers');

    return true;
};

maClient.Html5.prototype.executeClickAction = function(markerInstance)
{
	console.log("executeClickAction");

	var clickActionTarget = markerInstance.clickActionTarget;
	if (markerInstance.clickActionTarget.length > 0)
	{
		if (markerInstance.clickAction == maActionLinkToUrl || markerInstance.clickAction == maActionLinkToUrlNewWindow)
		{
			clickActionTarget = (markerInstance.clickAction == maActionLinkToUrl ? "0" : "1") + clickActionTarget;
			maClient.flashLinkToUrl(clickActionTarget);
		}
		else if (markerInstance.clickAction == maActionGotoPage)
		{
			maClient.flashGoToPage(clickActionTarget);
		}
		else if (markerInstance.clickAction == maActionCallJavascript)
		{
			maClient.flashExecuteJavaScript(clickActionTarget);
		}
	}
};

maClient.Html5.prototype.getBoundingArea = function(markerInstance)
{
	var bounds = new Object();

	var n = markerInstance.paddingNormal;
	var s = markerInstance.paddingSelected;

	var maxW = Math.max(n.w, s.w);
	var maxH = Math.max(n.h, s.h);

	if (markerInstance.markerType != maMarkerType_Symbol)
	{
		var lineWidth = markerInstance.lineWidth();

		if (lineWidth > 0)
		{
			// Increase the line width because polygons with thick lines and very angular shapes have pointy
			// corners with tips that go outside the calculated bounding area. This adjustment helps ensure
			// that the entire shape with its border will be contained in the bounding box.
			lineWidth *= 2;

			// Increase the area to accommodate the half thickness of the border that is outside the shape,
			// but only if greater than the area occuppied by the shape's glow or shadow.
			maxW = Math.max(maxW, lineWidth);
			maxH = Math.max(maxH, lineWidth);
		}
	}

	if (markerInstance.markerType == maMarkerType_SymbolAndShape && (markerInstance.symbolLocationX >= 0 || markerInstance.symbolLocationY >= 0))
	{
		// The symbol in this Shape+Symbol marker is not centered. Increase the shape size to account
		// for the possibility of part of the symbol protruding outside of the shape. This might not
		// be enough, but it's what we are using for now until we find a case where it does not work.
		maxW += markerInstance.shapeW;
		maxH += markerInstance.shapeH;
	}

	bounds.w = markerInstance.maxWidth + maxW;
	bounds.h = markerInstance.maxHeight + maxH;

	if (markerInstance.markerZooms)
	{
		bounds.w = Math.round(bounds.w * this.currentMapScale);
		bounds.h = Math.round(bounds.h * this.currentMapScale);
	}

	bounds.x = markerInstance.x;
	bounds.y = markerInstance.y;

	if (!markerInstance.isNotAnchored)
	{
		bounds.x = Math.round(bounds.x * this.currentMapScale);
		bounds.y = Math.round(bounds.y * this.currentMapScale);
	}

	bounds.x -= Math.round(bounds.w / 2);
	bounds.y -= Math.round(bounds.h / 2);

	bounds.x -= markerInstance.anchorScaledX();
	bounds.y -= markerInstance.anchorScaledY();

	bounds.x -= this.viewPort.x1;
	bounds.y -= this.viewPort.y1;

	return bounds;
};
		
maClient.Html5.prototype.getEffectOptions = function(values, index)
{
	var options = new Array();
	
	while (index < values.length)
	{
		var value = values[index];
		if (value == "-1")
			break;
		options.push(value);
		index++;
	}

	return options;
};

maClient.Html5.prototype.getEffects = function(markerInstance, effects, ctx, selected)
{
	if (effects.length === 0)
	{
		return;
	}

	//console.log("getEffects " + markerInstance.viewId);

	var filters = new Array();
	var options;
	var values = effects.split(",");
	var i = 0;

	while (i < values.length)
	{
		if (values[i] == "-1")
		{
			var effectType = values[i + 1];

			// Handle the case where the user separated parameters with spaces instead of commas.
			// Get just the text prior to the first space and ignore the rest.
			effectType = effectType.split(" ")[0];

			//console.log("Draw Effect " + markerInstance.viewId + " " + i + " " + effectType);

			var isBlend = effectType == "1";
			var isInnerGlow = effectType == "2";
			var isOuterGlow = effectType == "3";
			var isDropShadow = effectType == "4";

			var blur;
			var colorRgba;
			var alpha;

			// Skip past -1 and the effect Id;
			i += 2;

			if (isBlend)
			{
				options = this.getEffectOptions(values, i);
				i += options.length;
				markerInstance.blendMode = this.getValidBlendMode(this.getOptionText(options, 0, "multiply"));
			}
			else if (isInnerGlow || isOuterGlow)
			{
				options = this.getEffectOptions(values, i);
				i += options.length;

				alpha = this.getOptionAlpha(options, 1, 50);

				// The marker's line color is used as the default glow color.
				var markerStyle = markerInstance.markerStyle();
				if (markerStyle === null)
					markerStyle = markerInstance.customerMarkerStyle;
				var defaultGlowColor = selected ? markerStyle.selectedLineColor : markerStyle.normalLineColor;
				defaultGlowColor = "0x" + defaultGlowColor.substr(1);
				colorRgba = this.getOptionColor(options, 0, defaultGlowColor, alpha);

				// HTML5 does not support directional blur so we use the average of blur x and blur y.
				blur = this.getOptionBlur(options, 2, 10);

				// Apply the glow.
				if (ctx !== null)
				{
					ctx.shadowBlur = blur;
					ctx.shadowColor = colorRgba;
				}

				// Set the markers bounds that include its glow. Increase the blur amount to account
				// for the fact that the drawn blur on some browsers is larger than the value.
				blur += 8;
				var deltaXandY = Math.round(blur / 2) * -1;
				this.setMarkerClipBounds(markerInstance, selected, deltaXandY, deltaXandY, blur, blur);
			}
			else if (isDropShadow)
			{
				options = this.getEffectOptions(values, i);
				i += options.length;

				var distance = this.getOptionNumber(options, 0, 4);

				// Increase the distance to be more like the result we get with Flash.
				distance *= 2;

				var angle = this.getOptionNumber(options, 1, 45);
				alpha = this.getOptionAlpha(options, 3, 40);
				colorRgba = this.getOptionColor(options, 2, "0x000000", alpha);
				blur = this.getOptionBlur(options, 4, 10);

				//console.log("dropShadow " + distance + "," + angle + "," + colorRgba + "," + alpha + "," + blur);

				// Convert the angle to horizontal and vertical distances. Note that we have to do this
				// because the Flash version of this logic uses a single distance plus an angle whereas in
				// HTML5 there's x and y distances and no angle.
				var dh;
				var dv;

				if (angle < 0 || angle > 359)
					angle = 0;

				if (angle <= 90)
				{
					dh = (90 - angle) / 90;
					dv = angle / 90;
				}
				else if (angle <= 180)
				{
					angle -= 90;
					dh = (angle / 90) * -1;
					dv = (90 - angle) / 90;
				}
				else if (angle <= 270)
				{
					angle -= 180;
					dh = ((90 - angle) / 90) * -1;
					dv = (angle / 90) * -1;
				}
				else
				{
					angle -= 270;
					dh = angle / 90;
					dv = ((90 - angle) / 90) * -1;
				}

				// Right now dh and dv are percentages. Convert them to pixel distances.
				dh = Math.round(dh * distance);
				dv = Math.round(dv * distance);

				// Apply the drop shadow.
				if (ctx !== null)
				{
					ctx.shadowOffsetX = dh;
					ctx.shadowOffsetY = dv;
					ctx.shadowBlur = blur;
					ctx.shadowColor = colorRgba;
				}

				// Set the markers bounds that include its shadow and blur. If we don't
				// do this we get a little bit of clipping on the outer edges.
				dh += dh < 0 ? -blur : blur;
				dv += dv < 0 ? -blur : blur;

				if (markerInstance.shapeType == maShapeType_Polygon || markerInstance.shapeType == maShapeType_Line || markerInstance.shapeType == maShapeType_Hybrid)
				{
					// For irregular shapes we have to create a larger bounding box. How much larger depends on
					// how much the map is scaled. Since the shadow distance remains constant regardless of the
					// scale, the relative shadow size is much greater on scaled down maps. So we come up with a
					// fudge factor that is greater when the map is scaled more.
					var extra = (1 / this.mapScale) - 1;
					extra *= 25;
					if (extra > 200)
						extra = 200;
					else if (extra < 20)
						extra = 20;
					dh += extra;
					dv += extra;
				}

				this.setMarkerClipBounds(markerInstance, selected, dh < 0 ? dh : 0, dv < 0 ? dv : 0, Math.abs(dh), Math.abs(dv));
			}
		}
		else
		{
			console.log(">>> Unexpected effect value at " + i + " = " + values[i]);
			return;
		}
	}
};

maClient.Html5.prototype.getMarkerUnderMouse = function()
{
	var count = this.markerInstances.length;

	// Loop over the marker instances in reverse order so that we test markers that are
	// higher in the stacking order than those lower.
	for (var index = count - 1; index >= 0; index--)
	{
		var viewIdUnderMouse = 0;
		var markerInstance = this.markerInstances[index];

		if (!markerInstance.isVisibleAtCurrentZoomLevel)
			continue;

		var x = markerInstance.cornerX;
		var y = markerInstance.cornerY;
		var w = markerInstance.drawnMaxWidth;
		var h = markerInstance.drawnMaxHeight;

		//console.log("getMarkerUnderMouse " + this.mouseX + "," + this.mouseY + " : " + x + "," + y + " : " + w + "x" + h);

		if (maTour.enlargeHitTestArea && maClient.isTouchDevice)
		{
			// Increase the size of the hit area for small markers to make them easier to touch.
			if (w < this.minHitTargetSize || h < this.minHitTargetSize)
			{
				if (w < this.minHitTargetSize)
				{
					this.hitTestPaddingW = this.minHitTargetSize - w;
				}
				if (h < this.minHitTargetSize)
				{
					this.hitTestPaddingH = this.minHitTargetSize - h;
				}

				w += this.hitTestPaddingW;
				h += this.hitTestPaddingH;
				x -= this.hitTestPaddingW / 2;
				y -= this.hitTestPaddingH / 2;
			}
		}

		// Determine if the mouse is over the rectangle that contains the marker.
		var mouseIsInsideMarkerBounds = false;
		if (this.mouseX >= x &&
			this.mouseX < x + w &&
			this.mouseY >= y &&
			this.mouseY < y + h)
		{
			mouseIsInsideMarkerBounds = true;

			if (this.debugDrawing || this.debugShowTouch)
			{
				this.markerLayerContext.save();
				this.markerLayerContext.strokeStyle = "red";
				this.markerLayerContext.strokeRect(x - 0.5, y - 0.5, w, h);
				this.markerLayerContext.restore();
			}
		}

		if (mouseIsInsideMarkerBounds)
		{
			// Determine if the mouse is over the marker by drawing the marker's shape to an off-screen
			// canvas and then reading back from that canvas the pixel under the mouse. If the pixel is
			// is visible we have a hit.
			var ctx = this.hitLayerContext;
			ctx.clearRect(0, 0, this.viewPort.w, this.viewPort.h);
			this.drawMarkerShape(markerInstance, ctx, false);

			if (this.debugDrawing)
			{
				this.drawMarkerShape(markerInstance, null, false);
			}

			var imageData;
			try
			{
				imageData = ctx.getImageData(this.mouseX, this.mouseY, 1, 1);
			}
			catch (error)
			{
				console.log("ERROR " + error);
				console.log("== viewPort: " + this.viewPort.x1 + "," + this.viewPort.x2 + "," + this.viewPort.y1 + "," + this.viewPort.y2);
				console.log(">>> " + this.mouseX + "," + this.mouseY + " : " + x + "," + y + " : " + w + "x" + h);
				return 0;
			}

			if (imageData.data[3] > 0)
			{
				viewIdUnderMouse = markerInstance.viewId;
			}
		}

		// Reset the hit test override values used for small symbols.
		this.hitTestPaddingW = 0;
		this.hitTestPaddingH = 0;

		if (viewIdUnderMouse !== 0)
		{
			return viewIdUnderMouse;
		}
	}

	return 0;
};

maClient.Html5.prototype.getMouseLocation = function()
{
	var x;
	var y;

	if (maClient.isTouchDevice)
	{
		if (maTour.selectsOnTouchStart)
		{
			x = event.touches[0].pageX;
			y = event.touches[0].pageY;
		}
		else
		{
			// The touch just ended. Get the location of the mouse when the touch started.
			x = this.touchStartX;
			y = this.touchStartY;
		}
	}
	else
	{
		x = maClient.mouseX;
		y = maClient.mouseY;
	}

	x -= this.mapLocationInBrowser.x;
	y -= this.mapLocationInBrowser.y;

	if (maClient.preview)
	{
		// Adjustment for V4 Tour Preview which is centered in the browser instead of left-aligned as it was in V3.
		// When the browser is wider than the Tour Builder, the blank space between the left edge of the preview panel
		// and the left edge of the browser must be subtracted from mouse X. A left scroll adjustment is needed too.
		var previewPanelElement = document.getElementById('PreviewPanelV3');
		x -= previewPanelElement.getBoundingClientRect().left + maClient.getScrollingPosition()[0];
	}

	if (x < 0 || y < 0 || x >= this.viewPort.w || y >= this.viewPort.h)
	{
		//console.log(">>> Mouse is not on map %s,%s", x, y);
		return false;
	}
	else
	{
		if (this.debugHitTesting || this.debugShowTouch)
		{
			// Draw a red square.
			this.markerLayerContext.fillStyle = "#ff0000";
			this.markerLayerContext.fillRect(x - 2, y - 2, 4, 4);
		}

		this.mouseX = Math.round(x);
		this.mouseY = Math.round(y);

		return true;
	}
};

maClient.Html5.prototype.getOptionAlpha = function(options, index, defaultValue)
{
	var alpha = this.getOptionNumber(options, index, defaultValue);
	if (alpha < 0)
		alpha = 0;
	if (alpha > 100)
		alpha = 100;

	// Increase the shadow to be more like the result we get with Flash.
	alpha = Math.min(100, Math.round(alpha * 1.3));
	
	alpha /= 100;
	return alpha;
};

maClient.Html5.prototype.getOptionBlur = function(options, index, defaultValue)
{
	// HTML5 does not support directional blur so we use the average of blur x and blur y.
	var blurX = this.getOptionNumber(options, index, defaultValue);
	var blurY = this.getOptionNumber(options, index + 1, defaultValue);
	var blur = Math.round((blurX + blurY) / 2);

	return blur;
};

maClient.Html5.prototype.getOptionColor = function(options, index, defaultValue, alpha)
{
	var rawColor = this.getOptionText(options, index, defaultValue);
	var hexColor = this.convertIntegerColorToHex(rawColor);
	return this.createRgbaFromHexColor(hexColor, alpha);
};

maClient.Html5.prototype.getOptionNumber = function(options, index, defaultValue)
{
	var n = parseInt(this.getOptionText(options, index, defaultValue), 10);
	if (isNaN(n))
		n = defaultValue;
	return n;
};

maClient.Html5.prototype.getOptionText = function(options, index, defaultValue)
{
	return options.length > index ? options[index] : defaultValue;
};

maClient.Html5.prototype.getValidBlendMode = function(mode)
{
	mode = mode.toLowerCase();
	if (mode == "darken" ||
		mode == "difference" ||
		mode == "hardlight" ||
		mode == "invert" ||
		mode == "lighten" ||
		mode == "multiply" ||
		mode == "normal" ||
		mode == "overlay" ||
		mode == "screen")
		return mode;
	else
		return "multiply";
};

maClient.Html5.prototype.handlePanEvent = function()
{
	if (this.touchDeltaX !== 0 || this.touchDeltaY !== 0)
	{
		// console.log("handlePanEvent " + this.touchDeltaX + "," + this.touchDeltaY);
		this.setMapPan(this.touchDeltaX, this.touchDeltaY);
		this.startTouchDelay();
	}
};

maClient.Html5.prototype.initEventHandlers = function()
{
	var handler;

	if (maClient.isTouchDevice)
	{
		if (maTour.selectsOnTouchStart)
		{
			handler = 'touchstart';
		}
		else
		{
			handler = 'touchend';
		}

		this.markerLayer.addEventListener("touchstart", maClient.map.onTouchStart, false);
		this.markerLayer.addEventListener("touchend", maClient.map.onTouchEnd, false);
		this.markerLayer.addEventListener("touchcancel", maClient.map.onTouchCancel, false);

		if (this.mapCanZoom)
		{
			this.markerLayer.addEventListener("touchmove", maClient.map.onTouchMove, false);
			this.markerLayer.addEventListener("gesturechange", maClient.map.onGestureChange, false);
			this.markerLayer.addEventListener("gesturestart", maClient.map.onGestureStart, false);
			this.markerLayer.addEventListener("gestureend", maClient.map.onGestureEnd, false);
		}
	}
	else
	{
		handler = 'mousemove';
		this.markerLayer.addEventListener('click', maClient.map.onClick, false);

		// Detect if the browser size changes so that we can adjust the mouse location
		// on the map relative to the upper left corner of the browser window.
		window.addEventListener('resize', maClient.map.onResize, false);
		
		this.markerLayer.addEventListener("mousedown", maClient.map.onMouseDown, false);
		this.markerLayer.addEventListener("mouseup", maClient.map.onMouseUp, false);
		this.markerLayer.addEventListener("mouseout", maClient.map.onMouseUp, false);
	}

	this.markerLayer.addEventListener(handler, maClient.map.onTouch, false);
};

maClient.Html5.prototype.hideInstructions = function()
{
	document.getElementById('maInstructions').style.visibility = 'hidden';
	document.getElementById('maInstructionsCloseX').style.visibility = 'hidden';

	// Rememeber that the user explicitly closed the instructions.
	localStorage[this.localStorageId + "userClosedHelpWindow"] = "1";
	var currentTime = (new Date()).getTime();
	localStorage[this.localStorageId + "timeLastShown"] = currentTime;
};

maClient.Html5.prototype.loadMap = function()
{
	console.log("Html5.LoadMap v" + maClient.runtimeVersion + ":" + maClient.runtimeVersionHtml5);

	// Debug options.
	this.selectOnClick = false;
	this.mapIsSvg = false;
	this.debugClipping = false;
	this.debugDrawing = false;
	this.debugHitTesting = false;
	this.debugShowTouch = maGetQueryStringArg("showtouch") == "1";

	// Work around a regression in Chrome v77 whereby a) blending causes the shape to be filled in with garbage
	// instead of its proper blended fill color and b) when panning a zoomed-in map, marker shapes draw on top
	// of each other in repeating patterns. Both problems can be eliminated by disabling blending, or they can be
	// worked around by redrawing the entire map every time a marker is redrawn. However, the map redraw solution
	// is unacceptably slow on maps with a lot of shape markers (e.g. a trade show floorplan). The default
	// solution for Chrome is map redraw, but if that's too slow, the user can choose the disable blending option
	// from the Tour Manager and accept that the appearance on Chrome will be different than on other browsers.
	maClient.disableBlendingOnChrome = maClient.chrome && maTour.disableBlendEffect; 

	// Initialization.
	this.selectedViewId = 0;
	this.minHitTargetSize = 44;
	this.hitTestPaddingW = 0;
	this.hitTestPaddingH = 0;
	this.ignoreTouch = false;
	this.mapPanX = parseInt(maTour.mapPanX, 10);
	this.mapPanY = parseInt(maTour.mapPanY, 10);
	this.savedMapPanX = 0;
	this.savedMapPanY = 0;
	this.touchStartX = -1;
	this.touchStartY = -1;
	this.touchDeltaX = 0;
	this.touchDeltaY = 0;
	this.clipBounds = null;
	this.mouseX = 0;
	this.mouseY = 0;
	this.popupIsShowing = false;
	this.blinkCycle = 0;
	this.visitedMarkerAlpha = parseInt(maTour.visitedMarkerAlpha, 10) / 100;
	this.slideShowRunning = false;
	this.nextSlideShowIndex = 0;
	this.loadingMessage = "";
	this.dragging = false;

	// Use separate Id's for local storage when in Tour Preview or not so that
	// a published tour does not use the same state as a one in Tour Preview.
	this.localStorageId = "tour" + (maClient.preview ? "_." : ".");

	this.viewPort = new Object();
	this.viewPort.w = maTour.stageW;
	this.viewPort.h = maTour.stageH;
	this.setViewPortPosition();

	this.mapWidth = maTour.mapWidth;
	this.mapHeight = maTour.mapHeight;
	this.mapScale = parseFloat(maTour.mapAreaScale);
	this.mapWidthScaled = Math.round(this.mapWidth * this.mapScale);
	this.mapHeightScaled = Math.round(this.mapHeight * this.mapScale);
	this.mapInsetImageLoaded = true;

	this.mapZoomDelta = 0;
	this.minMaxZoom = 1.0;
	this.maxMapZoom = this.mapWidthScaled >= this.viewPort.w ? this.mapWidth / this.viewPort.w : this.mapHeight / this.viewPort.h;
	this.showZoomControl = maTour.mapShowZoomControl && (!maClient.iOS || maTour.showZoomControlOnIOs);

	// Initialize the layers.
	this.statusLayer = document.getElementById("maStatusLayer");
	this.statusLayerContext = this.statusLayer.getContext('2d');
	this.drawLoadingMessage(null);

	this.mapLayer = document.getElementById("maMapLayer");
	this.mapLayerContext = this.mapLayer.getContext('2d');
	this.mapLayer.width = this.viewPort.w;
	this.mapLayer.height = this.viewPort.h;
	this.mapLayer.style.backgroundColor = "transparent";

	this.markerLayer = document.getElementById("maMarkerLayer");
	this.markerLayerContext = this.markerLayer.getContext("2d");
	this.markerLayer.width = this.viewPort.w;
	this.markerLayer.height = this.viewPort.h;
	this.markerLayer.style.backgroundColor = "transparent";

	this.hitLayer = document.createElement('canvas');
	this.hitLayerContext = this.hitLayer.getContext('2d');
	this.hitLayer.width = this.viewPort.w;
	this.hitLayer.height = this.viewPort.h;

	// Locate the map's upper left corner of the map relative to the browser window.
	this.mapLocationInBrowser = maClient.tagLocation(null, "maMarkerLayer");

	// Determine if the map will initialy be displayed zoomed in or zoomed out.
	this.mapCanZoom = maTour.mapZoomLevel > 0;
	this.mapZoom = maTour.mapZoomLevel == 1 || !this.mapCanZoom ? this.minMaxZoom : this.maxMapZoom;
	this.setCurrentMapScale();

	// Initialize each zoom level.
	this.mapLevels = new Array(this.mapCanZoom ? 2 : 1);
	this.mapTileImagesLoaded = false;
	this.waitForMapTileImagesAttempts = 0;

	this.loadImageData();
	this.loadImageObjects();
	this.initEventHandlers();
};

maClient.Html5.prototype.loadImageData = function()
{
	// Initialize flags and a counter to let us monitor when the image data for the map tiles
	// and marker symbols have downloaded. We used to do this dynamically by inserting script
	// tags, but now we statically load the files in the page html file. We made the change to
	// work around a bug in Safari on iOS9 where it was taking 30+ seconds to determine that
	// the files had downloaded.
	this.waitForImageDataLoadAttempts = 0;
	this.mapTileImageDataLoaded = false;
	this.symbolImageDataLoaded = false;
};

maClient.Html5.prototype.loadImageObjects = function()
{
	var delayPeriod = 50;

	// Check to see if the image data for the map tiles and marker symbols has downloaded. The two DataLoaded
	// functions are defined in those data files. If the functions are undefined, the files have not downloaded
	// yet. When we used to dynamically download the files, we used onLoad handlers and waited for them to fire.
	// One flaw in this logic is that if the downloads have not occurred after 50*1200ms (60 seconds), the code
	// falls through and executes without data which means that the map and/or symbols won't be visible.
	maClient.map.mapTileImageDataLoaded = typeof (maClient.Html5.prototype.markerImages) !== 'undefined';
	maClient.map.symbolImageDataLoaded = typeof (maClient.Html5.prototype.mapTiles) !== 'undefined';

	if ((!this.mapTileImageDataLoaded || !this.symbolImageDataLoaded) && this.waitForImageDataLoadAttempts < 1200)
	{
		this.waitForImageDataLoadAttempts++;
		setTimeout("maClient.map.loadImageObjects();", delayPeriod);
		return;
	}

	console.log("Image data loaded in " + (delayPeriod * this.waitForImageDataLoadAttempts) + " ms");

	// The image data has been loaded. Now set the src of each map tile image object to its data URI.
	
	// Load the zoomed out map tiles.
	this.loadMapTiles(1);
	
	if (this.mapCanZoom)
	{
		if (maTour.mapInsetLocation !== 0)
		{
			// Load the map inset image.
			this.mapInsetImageLoaded = false;
			this.mapInsetImage = new Image();
			this.mapInsetImage.onload = maOnMapInsetImageLoad;
			this.mapInsetImage.src = maClient.path + maTour.mapInsetFileName + '?v=' + maTour.buildId;
		}

		// Load the zoomed in map tiles.
		this.loadMapTiles(2);
	}

	// Create the markers styles and instances.
	this.createMarkerStyles();
	this.createMarkerInstances();

	// Wait until the map image objects are loaded and then draw the map.
	this.drawLoadingMessage(null);
	this.waitForMapTileImagesToLoad();
};

maClient.Html5.prototype.loadMapTiles = function(level)
{
	// Create an object that will track all the information about this levels.
	var mapLevel = new Object();
	this.mapLevels[level - 1] = mapLevel;

	// Determine how many rows and columns of tiles this level has. The more zoomed
	// in the level, the more tiles it will have.
	var tileWidth = 256;
	var tileHeight = 256;

	var mapWidth = level == 1 ? this.mapWidthScaled : this.mapWidth;
	var mapHeight = level == 1 ? this.mapHeightScaled : this.mapHeight;

	mapLevel.lastColumn = Math.round(mapWidth / tileWidth);
	mapLevel.lastColumnWidth = mapWidth - (mapLevel.lastColumn * tileWidth);
	if (mapLevel.lastColumnWidth > 0)
		mapLevel.lastColumn++;
	else
		mapLevel.lastColumnWidth = tileWidth;

	mapLevel.lastRow = Math.round(mapHeight / tileHeight);
	mapLevel.lastRowHeight = mapHeight - (mapLevel.lastRow * tileHeight);
	if (mapLevel.lastRowHeight > 0)
		mapLevel.lastRow++;
	else
		mapLevel.lastRowHeight = tileHeight;

	if (mapLevel.lastRow < 0)
	{
		// This would normally never happen, but it can if the map has a negative height because the tour
		// height is fixed and has a too-tall banner. The problem should be caught in the banner logic,
		// but for now we just keep an exception from occurring.
		mapLevel.lastRow = 0;
	}

	// Create an array to hold a tile object for each tile belonging to this level.
	var tile;
	var index = 0;
	var tileCount = mapLevel.lastRow * mapLevel.lastColumn;
	mapLevel.tiles = new Array(tileCount);

	for (var row = 1; row <= mapLevel.lastRow; row++)
	{
		for (var column = 1; column <= mapLevel.lastColumn; column++)
		{
			index = (((row - 1) * mapLevel.lastColumn) + column) - 1;

			// Create a tile object to hold information about a tile, including its image.
			tile = new Object();
			tile.id = index;
			tile.level = level;
			tile.loaded = false;

			// Create an image object to hold the tile's image.
			tile.image = new Image();

			// Add a property to the image that points back to its tile.
			// It will be used in the image's onload handler which we assign here.
			tile.image.tile = tile;
			tile.image.onload = maOnTileImageLoad;

			// Add the tile object to this level's array of tiles.
			mapLevel.tiles[index] = tile;
		}
	}

	// Get the array of map tile image data. Note that the mapTiles() function is defined
	// in the dynamically loaded map image data JavaScript file -- it's not here in mapviewer.js.
	var mapTiles = this.mapTiles();

	// Assign the image src for each tile.
	for (index = 0; index < tileCount; index++)
	{
		tile = mapLevel.tiles[index];
		tile.image.src = "data:image/jpg;base64," + mapTiles[level - 1][index];
	}
};

function maOnTileImageLoad()
{
	// This event fired for a specific tile. Mark the tile as having its image loaded.
	// Note that 'this' is the image object which has a tile property that we added in
	// loadMapTiles. The tile property point back to the tile that this image belongs
	// to and allows us to access the tile's loaded flag.
	this.tile.loaded = true;
	//console.log('LOADED: ' + this.tile.level + " : " + this.tile.id);
}

function maOnMapInsetImageLoad()
{
	maClient.map.mapInsetImageLoaded = true;
	console.log("maOnMapInsetImageLoad");
}

maClient.Html5.prototype.mapIsZoomed = function()
{
	return this.mapCanZoom && this.mapZoom > this.minMaxZoom;
};

maClient.Html5.prototype.markerInstance = function(viewId)
{
	for (var index in this.markerInstances)
	{
		var markerInstance = this.markerInstances[index];
		if (markerInstance.viewId == viewId)
			return markerInstance;
	}
	console.log(">>> NO MARKER INSTANCE FOUND FOR " + viewId);
	return null;
};

maClient.Html5.prototype.markerIsInClippingRegion = function(markerInstance, clipX, clipY, clipW, clipH)
{
	//console.log("markerIsInClippingRegion " + markerInstance.viewId);
	
	var bounds = this.getBoundingArea(markerInstance);
	var x = bounds.x;
	var y = bounds.y;
	var w = bounds.w;
	var h = bounds.h;

	if (x + w < clipX)
	{
		// The marker is to the of left of the clipping region.
		return false;
	}
	else if (x > clipX + clipW)
	{
		// The marker is to the right of the clipping region.
		return false;
	}
	else if (y + h < clipY)
	{
		// The marker is above the clipping region.
		return false;
	}
	else if (y > clipY + clipH)
	{
		// The marker is below the clipping region.
		return false;
	}

	return true;
};

maClient.Html5.prototype.needToShowHelp = function()
{
	if (!maTour.showInstructions)
		return false;

	// Determine whether to show the instructions box by reading from HTML5 local storage.
	var buildId = localStorage[this.localStorageId + "buildId"];

	// Read the build Id from local storage.  If it does not match the tour builder build Id,
	// it is either undefined which mean this is the first time the user has ever visited this tour,
	// or it has changed, which means it's the first visit since the last build of this tour.
	var firstVisit = buildId != maTour.buildId;
	var timeLastShown = firstVisit ? 0 : localStorage[this.localStorageId + "timeLastShown"];

	// Determine how much time has passed since a map in this tour was previously loaded.
	var currentTime = (new Date()).getTime();
	var elapsedTime = currentTime - timeLastShown;

	// We show the Help window if any of the following are true:
	// 1 - This is the user's first visit.
	// 2 - The user has never explicity closed the Help window.
	// 3 - Too many minutes have elapsed since the previous visit.
	var HELP_TIMEOUT = 1000 * 60 * 5; // 5 minutes;

	// Determine if we need to show the Help window.
	var show = firstVisit || localStorage[this.localStorageId + "userClosedHelpWindow"] != "1" || elapsedTime > HELP_TIMEOUT;

	// Write the state info to the user's computer.
	localStorage[this.localStorageId + "buildId"] = maTour.buildId;
	if (show)
	{
		localStorage[this.localStorageId + "userClosedHelpWindow"] = "0";
	}
	localStorage[this.localStorageId + "timeLastShown"] = currentTime;
	localStorage[this.localStorageId + "buildId"] = maTour.buildId;

	return show;
};

maClient.Html5.prototype.positionMapToShowMarker = function(viewId)
{
	console.log("positionMapToShowMarker " + viewId);

	if (!this.mapIsZoomed())
		return false;

	var markerInstance = this.markerInstance(viewId);
	if (markerInstance === null)
		return false;

	// Determine if the marker is already visible.
	var x = this.mapPanX + markerInstance.x;
	var y = this.mapPanY + markerInstance.y;

	// The distance from the edge considered to be not visible.
	var pad = 16;

	var visible = x > pad && x < this.viewPort.w - pad && y > pad && y < this.viewPort.h - pad;
	if (visible)
	{
		return false;
	}
	else
	{
		var positioned = this.setMapPosition(markerInstance.x, markerInstance.y);
		this.drawMap();
		return positioned;
	}
};

maClient.Html5.prototype.redrawMarker = function(markerInstance, selected)
{
	if (markerInstance === null)
	{
		// This can happen if the map's first hotspot is not placed on the map. The logic thinks
		// its the selected hotspot and tries to redraw it when you touch another hotspot after
		// the map first loads. This should really be addressed in the Tour Builder, but this
		// logic prevents a JavaScript error from occurring.
		return;
	}

	//console.log("redrawMarker " + markerInstance.viewId);

	// Get the area covered by the marker and its shadow or blur.
	var bounds = this.getBoundingArea(markerInstance);
	var x = bounds.x;
	var y = bounds.y;
	var w = bounds.w;
	var h = bounds.h;

	if (markerInstance.isNotAnchored)
	{
		x -= this.mapPanX;
		y -= this.mapPanY;
	}

	this.markerLayerContext.save();

	// Draw a rectangular path around the marker area.
	this.markerLayerContext.beginPath();
	this.markerLayerContext.moveTo(x, y);
	this.markerLayerContext.lineTo(x + w, y);
	this.markerLayerContext.lineTo(x + w, y + h);
	this.markerLayerContext.lineTo(x, y + h);
	this.markerLayerContext.lineTo(x, y);

	// Set the marker bounds as the clipping region. Drawing will be limited to this area in
	// order to minimize the performance cost of the redraw. Skip on Chrome unless blending 
	// is disabled (see comment on disableBlendingOnChrome flag).
	if (!maClient.chrome || maClient.disableBlendingOnChrome)
		this.markerLayerContext.clip();

	// Erase the clipping region. When debugging we fill the area with color instead.
	if (this.debugClipping && (selected || markerInstance.isStatic))
	{
		this.markerLayerContext.fillStyle = "yellow";
		this.markerLayerContext.fill();
	}
	else
	{
		this.markerLayerContext.clearRect(x, y, w, h);
	}

	var drawSelectedMarkerOnTop = false;

	// Redraw every marker that intersects the clipping region, including the one requested.
	// By drawing in the order of the marker instances array we preserve the stacking order.
	for (var index in this.markerInstances)
	{
		var m = this.markerInstances[index];

		if (m.viewId == markerInstance.viewId)
		{
			// Draw the requested marker.
			if (!this.isHidden && (!drawSelectedMarkerOnTop || !selected))
			{
				this.drawMarker(m, selected);
			}
		}
		else
		{
			if (this.markerIsInClippingRegion(m, x, y, w, h))
			{
				// Draw an intersecting marker.
				//console.log("REDRAW intersecting " + m.viewId);
				var drawSelected = (m.viewId == this.selectedViewId && !m.isStatic) || m.appearsSelected;
				this.clipBounds = bounds;
				this.drawMarker(m, drawSelected);
				this.clipBounds = null;
			}
		}
	}

	if (drawSelectedMarkerOnTop && selected)
	{
		console.log("ON TOP");
		this.drawMarker(markerInstance, true);
	}

	// Restore original clipping region whic is the full canvas area.
	this.markerLayerContext.restore();
	
	// See comment on disableBlendingOnChrome flag.
	if (maClient.chrome && !maClient.disableBlendingOnChrome)
	{
		// console.log("drawMap on Chrome " + markerInstance.viewId);
		this.drawMap();
	}
};

maClient.Html5.prototype.refreshMap = function(callerName)
{
    if (maClient.iOS)
    {
        // Work around problem introduced in iOS 11 where a marker's backgrounds appears black after it gets selected or deselected.
        console.log('>>> refreshMap called by: ' + callerName);
        maClient.map.drawMap();
    }
};

maClient.Html5.prototype.restoreDrawingSurface = function()
{
	//console.log("restoreDrawingSurface");
	this.markerLayerContext.restore();
	this.hitLayerContext.restore();
};

maClient.Html5.prototype.restoreMarkerShapeAppearance = function(viewIdList, selected)
{
	console.log("restoreMarkerShapeAppearance " + viewIdList);

	var markers = this.createMarkerArrayFromMarkerIdList(viewIdList);

	for (var index in markers)
	{
		var markerInstance = markers[index];
		
		// Get the custom style for this marker.
		var custom = markerInstance.customMarkerStyle;
		if (custom === null)
		{
			// There's nothing to restore;
			continue;
		}
		
		// Get the original style for this marker.
		var original = this.markerStyles[markerInstance.markerStyleId];
		if (!original)
		{
			// This marker does not have a style. It could be a route.
			continue;
		}

		// Set the normal or selected attributes of the custom style back to the original style.
		// Note that once a marker has a custom style it always uses it even if both the normal
		// and selected appearances get set back to the original.
		if (selected)
		{
			custom.selectedFillColor = original.selectedFillColor;
			custom.selectedFillColorOpacity = original.selectedFillColorOpacity;
			custom.selectedLineColor = original.selectedLineColor;
			custom.selectedLineColorOpacity = original.selectedLineColorOpacity;
			custom.selectedShapeEffects = original.selectedShapeEffects;
		}
		else
		{
			custom.normalFillColor = original.normalFillColor;
			custom.normalFillColorOpacity = original.normalFillColorOpacity;
			custom.normalLineColor = original.normalLineColor;
			custom.normalLineColorOpacity = original.normalLineColorOpacity;
			custom.normalShapeEffects = original.normalShapeEffects;
		}
		
		// Show the original appearance.
		this.redrawMarker(markerInstance, markerInstance.viewId == this.selectedViewId);
	}

    maClient.map.refreshMap('restoreMarkerShapeAppearance');
};

maClient.Html5.prototype.runSlideShow = function()
{
	if (!maTour.runSlideShow)
		return;

	// Get the index of the first hotspot.
	for (index in this.markerInstances)
	{
		var markerInstance = this.markerInstances[index];
		if (markerInstance.viewId == maTour.selectedViewId)
		{
			this.nextSlideShowIndex = index;
			break;
		}
	}

	this.slideShowRunning = true;
	maClient.flashSlideShowIsRunning(true);
	this.showNextSlide();
	this.slideShowIntervalId = setInterval("maClient.map.showNextSlide();", parseInt(maTour.slideShowInterval, 10));
};

maClient.Html5.prototype.selectMarkerAndShowView = function(viewId, isDirEntry, dontBlink)
{
	console.log("selectMarkerAndShowView " + viewId + " " + isDirEntry);

	this.deselectMarker();

	markerInstance = this.markerInstance(viewId);

	// Make sure the marker is visible in the viewport.
	this.positionMapToShowMarker(markerInstance.viewId);

	if (isDirEntry)
	{
		if (!dontBlink)
		{
			markerInstance.setBlink(parseInt(maTour.blinkCount, 10));
		}

		this.stopSlideShow();
	}

	// Select the new marker.
	this.switchView(markerInstance, isDirEntry);

    maClient.map.refreshMap('selectMarkerAndShowView');
};

maClient.Html5.prototype.setCurrentMapScale = function()
{
	// Compute this just one for the zoom level because the value is accessed often.
	// We used to call a function to get the value, but it chewed up a lot of cycles.
	this.currentMapScale = this.mapZoom == this.maxMapZoom ? 1.0 : this.mapZoom * this.mapScale;
};

maClient.Html5.prototype.setDirectoryState = function(showing)
{
	console.log("setDirectoryState " + showing);
};

maClient.Html5.prototype.setDrawingSurface = function(markerInstance)
{
	//console.log("setDrawingSurface");

	this.markerLayerContext.save();
	this.hitLayerContext.save();

	var x = markerInstance.x;
	var y = markerInstance.y;

	if (!markerInstance.isNotAnchored)
	{
		// Shift the marker's location to where it appears within the viewport.
		x += this.mapPanX;
		y += this.mapPanY;
	}

	if (markerInstance.markerZooms)
	{
		// Scale the canvas so that the marker will be drawn at the correct scale.
		var s = this.currentMapScale;
		this.markerLayerContext.scale(s, s);
		this.hitLayerContext.scale(s, s);
	}
	else if (!markerInstance.isNotAnchored)
	{
		x = Math.round(x * this.currentMapScale);
		y = Math.round(y * this.currentMapScale);
	}

	if (markerInstance.isNotAnchored)
	{
		if (markerInstance.markerZooms)
		{
			// Adjust the marker's position to be unscaled.
			x = x / this.currentMapScale;
			y = y / this.currentMapScale;
		}
	}

	if (markerInstance.markerType == maMarkerType_Shape)
	{
		// Adjusting by a half pixel for polygons and rectangles seems to give us the best results.
		// We don't do it for any marker that has a bitmap because it can cause unwanted anti-aliasing.
		x += 0.5;
		y += 0.5;
	}

	// Shift the drawing origin to be the center of the marker within the viewport.
	//console.log("TRANSLATE " + markerInstance.viewId + " " + x + "," + y + " : " + markerInstance.rotation);
	this.markerLayerContext.translate(x, y);
	this.hitLayerContext.translate(x, y);

	if (markerInstance.rotation !== 0)
	{
		// Rotate the canvas so that the marker will be drawn rotated.
		this.markerLayerContext.rotate(markerInstance.rotation);
		this.hitLayerContext.rotate(markerInstance.rotation);
	}
};

maClient.Html5.prototype.setMapPan = function(deltaX, deltaY)
{
	this.shiftMapPanPosition(deltaX, deltaY);
	this.drawMap();
};

maClient.Html5.prototype.shiftMapPanPosition = function(deltaX, deltaY)
{
	// console.log("shiftMapPanPosition " + deltaX + "," + deltaY);

	if (!this.mapIsZoomed())
	{
		this.mapPanX = 0;
		this.mapPanY = 0;
		this.setViewPortPosition();
		return;
	}

	var mapWasPanned = false;

	var currentMapWidth = Math.round(this.mapWidth * this.currentMapScale);
	var currentMapHeight = Math.round(this.mapHeight * this.currentMapScale);

	// Pan horizontal.
	var maxPanRight;
	var okToPan = true;

	if (this.viewPort.w > currentMapWidth)
		maxPanRight = 0;
	else
		maxPanRight = -(currentMapWidth - this.viewPort.w);

	var requestedPanX = this.mapPanX + deltaX;

	if (requestedPanX > 0)
	{
		// Don't allow left edge of map to pan right away from left edge of viewport.
		this.mapPanX = 0;
	}
	else if (requestedPanX < maxPanRight)
	{
		// Don't allow right edge of map to pan left away from right edge of viewport.
		this.mapPanX = maxPanRight;
	}
	else
	{
		if (this.mapPanX == requestedPanX && deltaX !== 0)
		{
			// There's nothing to do. When deltaX is zero, this is an initialization call. 
			okToPan = false;
		}
		else
		{
			this.mapPanX = requestedPanX;
		}
	}

	if (okToPan)
	{
		mapWasPanned = true;
	}

	// Pan vertical.
	var maxPanBottom;
	okToPan = true;

	if (this.viewPort.h > currentMapHeight)
		maxPanBottom = 0;
	else
		maxPanBottom = -(currentMapHeight - this.viewPort.h);

	var requestedPanY = this.mapPanY + deltaY;

	if (requestedPanY > 0)
	{
		// Don't allow top edge of map to pan down away from top edge of viewport.
		this.mapPanY = 0;
	}
	else if (requestedPanY < maxPanBottom)
	{
		// Don't allow bottom edge of map to pan up from bottom edge of viewport.
		this.mapPanY = maxPanBottom;
	}
	else
	{
		if (this.mapPanY == requestedPanY && deltaY !== 0)
		{
			// There's nothing to do. When deltaY is zero, this is an initialization call.
			okToPan = false;
		}

		this.mapPanY = requestedPanY;
	}

	if (okToPan)
	{
		mapWasPanned = true;
	}

	if (mapWasPanned && (deltaX !== 0 || deltaY !== 0))
	{
		if (this.popupIsShowing)
		{
			this.deselectMarker();
			maClosePopup();
		}
	}

	this.setViewPortPosition();
};

maClient.Html5.prototype.setMapPosition = function(centerX, centerY)
{
	if (this.mapIsZoomed())
	{
		console.log("setMapPosition " + centerX + "," + centerY);

		var oldMapPanX = this.mapPanX;
		var oldMapPanY = this.mapPanY;

		// Attempt to position the center point in the center of the viewPort.
		// Round the x and y values so that map tiles will always get drawn on whole pixel
		// boundaries. If we don't do this, some browsers leave hairline gap between tiles.
		this.mapPanX = Math.round(-centerX + (this.viewPort.w / 2));
		this.mapPanY = Math.round(-centerY + (this.viewPort.h / 2));
		
		this.shiftMapPanPosition(0, 0);

		return oldMapPanX != this.mapPanX || oldMapPanY != this.mapPanY;
	}
	else
	{
		return false;
	}
};

maClient.Html5.prototype.setMapZoomInOut = function(delta)
{
	if (!this.mapCanZoom)
		return;

	var lastSelectedViewId = this.selectedViewId;
	console.log("setMapZoomInOut " + delta + " " + lastSelectedViewId);

	if (this.popupIsShowing)
	{
		maClosePopup();
	}

	if (delta > 0 && this.mapZoom == this.maxMapZoom)
		return;

	if (delta < 0 && this.mapZoom == this.minMaxZoom)
		return;

	if (delta > 0)
	{
		this.mapZoom = this.maxMapZoom;
	}
	else
	{
		// Remember the map's last pan position so that it can be restored to that
		// same position if the user zooms back in.
		this.mapZoom = this.minMaxZoom;
		this.savedMapPanX = this.mapPanX;
		this.savedMapPanY = this.mapPanY;
		this.mapPanX = 0;
		this.mapPanY = 0;
		this.setViewPortPosition();
	}

	this.setCurrentMapScale();

	if (lastSelectedViewId !== 0 && this.mapIsZoomed())
	{
		// If we just got asked to redraw the map zoomed in and a marker is selected, position the
		// map so that the selected marker is visible. Pass true for isDir to prevent the hotspot's
		// click handler from executing, but also indicate not to blink the marker.
		this.selectMarkerAndShowView(lastSelectedViewId, true, true);
	}

	// Clear the map layer since the tiles of the new zoom level might not cover the entire canvas.
	// We used to clear the map from drawMap, but it's an expensive operation and does not need to
	// be done when the map is panned, only when zoomed.
	this.mapLayerContext.clearRect(0, 0, this.viewPort.w, this.viewPort.h);
	
	this.drawMap();
};

maClient.Html5.prototype.setMapZoomLevel = function(level)
{
	var midLevel = parseInt(maTour.mapZoomMidLevel, 10);
	console.log("setMapZoomLevel " + level + " " + midLevel);
	this.setMapZoomInOut(level < midLevel ? -1 : 1);
};

maClient.Html5.prototype.setMarkerAppearanceAsNormalOrSelected = function(viewIdList, selected)
{
	console.log("setMarkerAppearanceAsNormalOrSelected " + viewIdList);

	var markers = this.createMarkerArrayFromMarkerIdList(viewIdList);

	// Loop over all the markers and for each one, see if its in the view Id list.
	// If it is, flag it as appearing selected or normal depending on the request
	// and then redraw it. This algorithm causes all the markers in the list to be
	// redrawn in their correct stacking order with the proper ones showing as selected.
	// The flag ensures that if a marker is redrawn again elsewhere it will still appear
	// selected. The flag only gets cleared here (if selected == false) or when the
	// marker gets deselected e.g. when you mouse over it and off of it again.

	for (index in this.markerInstances)
	{
		markerInstance = this.markerInstances[index];

		var redraw = false;

		for (var i in markers)
		{
			var m = markers[i];
			
			if (m.viewId == markerInstance.viewId)
			{
				markerInstance.appearsSelected = selected;
				redraw = true;
				break;
			}
		}

		if (redraw)
		{
			this.redrawMarker(markerInstance, false);
		}
	}
};

maClient.Html5.prototype.setMarkerAppearanceNormal = function(viewIdList)
{
	console.log("setMarkerAppearanceNormal " + viewIdList);
	this.setMarkerAppearanceAsNormalOrSelected(viewIdList, false);
};

maClient.Html5.prototype.setMarkerAppearanceSelected = function(viewIdList)
{
	console.log("setMarkerAppearanceSelected " + viewIdList);
	this.setMarkerAppearanceAsNormalOrSelected(viewIdList, true);
};

maClient.Html5.prototype.setMarkerListBlink = function(viewIdList, blinkCount)
{
	console.log("setMarkerListBlink " + viewIdList + " : " + blinkCount);

	var markers = this.createMarkerArrayFromMarkerIdList(viewIdList);

	for (var index in markers)
	{
		var markerInstance = markers[index];
		markerInstance.setBlink(blinkCount);
	}
};

maClient.Html5.prototype.setMarkerClipBounds = function(markerInstance, selected, deltaX, deltaY, deltaW, deltaH)
{
	//console.log("setMarkerClipBounds " + markerInstance.viewId + " " + deltaX + " " + deltaY + " " + deltaW + " " + deltaH);
	// Set the marker's padding so we know how much extra space is occupied by a shadow and/or glow.
	// Negative delta x or y is the amount that the shadow or blur appears to the left or above the marker.
	// Delta w and h are the total increase in width and height caused by the shadow or glow. When the 
	// padding is applied to the marker's shape, the resulting bounds tell us how much of the canvas has
	// to be redrawn whenever this marker is selected or deselected. Any other markers that intersect this
	// clipping area have to redrawn too.
	var padding = selected ? markerInstance.paddingSelected : markerInstance.paddingNormal;
	padding.x = deltaX;
	padding.y = deltaY;
	padding.w = deltaW;
	padding.h = deltaH;
};

maClient.Html5.prototype.setMarkerListDisabled = function(viewIdList, isDisabled)
{
	var markers = this.createMarkerArrayFromMarkerIdList(viewIdList);

	for (var index in markers)
	{
		var markerInstance = markers[index];
		markerInstance.isDisabled = isDisabled === true;
		console.log("setMarkerListDisabled" + " : " + markerInstance.viewId + " " + isDisabled);
	}
};

maClient.Html5.prototype.setMarkerListHidden = function(viewIdList, isHidden)
{
	console.log("setMarkerListHidden " + viewIdList + " : " + isHidden);
	var markers = this.createMarkerArrayFromMarkerIdList(viewIdList);

	for (var index in markers)
	{
		var markerInstance = markers[index];
		if (markerInstance.isHidden != isHidden)
		{
			markerInstance.isHidden = isHidden === true;

			// Recompute the cached isVisibleAtCurrentZoomLevel flag.
			markerInstance.isVisibleAtCurrentZoomLevel = markerInstance.isVisibleAtCurrentZoomLevel_();

			this.redrawMarker(markerInstance, markerInstance.viewId == this.selectedViewId);
		}
	}

    maClient.map.refreshMap('setMarkerListHidden');
};

maClient.Html5.prototype.setMarkerListStatic = function(viewIdList, isStatic)
{
	var markers = this.createMarkerArrayFromMarkerIdList(viewIdList);

	for (var index in markers)
	{
		var markerInstance = markers[index];
		markerInstance.isStatic = isStatic === true;
		console.log("setMarkerListStatic" + " : " + markerInstance.viewId + " " + isStatic);
	}
};

maClient.Html5.prototype.setMarkerOnTop = function(viewId)
{
	console.log("setMarkerOnTop " + viewId);

	// Get the position of this marker in the marker array.
	var markerInstance = null;
	var position = -1;
	for (var index in this.markerInstances)
	{
		markerInstance = this.markerInstances[index];
		if (markerInstance.viewId == viewId)
		{
			position = index;
			break;
		}
	}

	if (position != -1)
	{
		// Remove the marker from its current position...
		this.markerInstances.splice(position, 1);

		// And put it at the end of the array so that it will be on top of all other markers.
		this.markerInstances.push(markerInstance);
	}

	this.redrawMarker(markerInstance, markerInstance.viewId == this.selectedViewId);
};

maClient.Html5.prototype.setMarkerSelectedState = function(viewId)
{
	console.log("setMarkerSelectedState " + viewId);
	this.selectedViewId = viewId;
};

maClient.Html5.prototype.setPopupState = function(viewId, showing, pinned, mouseIsOver)
{
	//console.log("setPopupState " + viewId + " " + this.popupIsShowing + " : " + showing + " " + pinned + " " + mouseIsOver);
	this.popupIsShowing = showing;
	if (!showing && this.selectedViewId !== 0)
		this.deselectMarker();
};

maClient.Html5.prototype.setViewPortPosition = function()
{
	this.viewPort.x1 = -this.mapPanX;
	this.viewPort.x2 = this.viewPort.x1 + this.viewPort.w - 1;
	this.viewPort.y1 = -this.mapPanY;
	this.viewPort.y2 = this.viewPort.y1 + this.viewPort.h - 1;
};

maClient.Html5.prototype.showInstructions = function()
{
	if (!this.needToShowHelp())
		return;

	var zoomControlOffset = this.showZoomControl ? 32 : 0;

	var instructions = document.createElement("div");
	instructions.setAttribute("id", "maInstructions");
	instructions.setAttribute("class", "maInstructions");
	instructions.style.position = "absolute";
	instructions.style.zIndex = 5000;
	instructions.style.top = "0px";
	instructions.style.left = 8 + zoomControlOffset + "px";
	instructions.style.backgroundColor = maTour.instructions.bgColor;
	instructions.style.color = maTour.instructions.color;
	instructions.style.width = maTour.instructions.width - 12 + "px";
	instructions.onclick = new Function("maClient.map.hideInstructions();");

	var closeX = document.createElement("img");
	closeX.setAttribute("id", "maInstructionsCloseX");
	closeX.style.position = "absolute";
	closeX.style.zIndex = 5000;
	closeX.style.top = "4px";
	closeX.style.left = zoomControlOffset + maTour.instructions.width - 6 + "px";
	closeX.src = maClient.graphics["closeInstructionsX"].src;
	closeX.style.width = "12px";
	closeX.style.height = "12px";
	closeX.onclick = new Function("maClient.map.hideInstructions();");

	var maMap = document.getElementById("maMap");
	maMap.appendChild(instructions);
	maMap.appendChild(closeX);

	var text = maTour.instructions.text;
	var index = text.indexOf("###");
	if (index != -1)
	{
		if (maClient.isTouchDevice)
		{
			// Three hash marks mean the start of instructions for touch devices.
			text = text.substr(index + 3);
		}
		else
		{
			text = text.substr(0, index);
		}
	}

	instructions.innerHTML = "<div class='maInstructionsTitle'>" + maTour.instructions.title + "</div>" + text;
};

maClient.Html5.prototype.showMarkerSelected = function(viewId, show)
{
	console.log("showMarkerSelected " + viewId + " " + show);
	if (show)
		this.setMarkerAppearanceSelected(viewId);
	else
		this.setMarkerAppearanceNormal(viewId);
};

maClient.Html5.prototype.showNextSlide = function()
{
	console.log("SS " + this.nextSlideShowIndex);
	var markerInstance = this.markerInstances[this.nextSlideShowIndex];
	this.nextSlideShowIndex++;
	if (this.nextSlideShowIndex >= this.markerInstances.length)
	{
		this.nextSlideShowIndex = 0;
	}
	this.selectMarkerAndShowView(markerInstance.viewId, false);
};

maClient.Html5.prototype.showTooltip = function(markerInstance)
{
	if (markerInstance.tooltip.length > 0)
	{
		var x = markerInstance.centerX() + this.mapPanX - markerInstance.anchorScaledX();
		var y = markerInstance.centerY() + this.mapPanY - markerInstance.anchorScaledY();
		maClient.flashMarkerMouseOver(markerInstance.tooltip, x, y);
	}
};

maClient.Html5.prototype.showZoomControls = function()
{
	if (!this.showZoomControl)
		return;

	var zoomControl = document.getElementById("maZoomControl");
	if (!zoomControl)
	{
		zoomControl = document.createElement('canvas');
		zoomControl.setAttribute("id", "maZoomControl");
		zoomControl.setAttribute("class", "maZoomControl");
		zoomControl.width = 44;
		zoomControl.height = 44;

		var maMap = document.getElementById("maMap");
		maMap.appendChild(zoomControl);
	}

	var ctx = zoomControl.getContext('2d');
	ctx.save();
	ctx.clearRect(0, 0, 44, 44);

	// Draw the background color.
	ctx.fillStyle = maTour.mapZoomControlColor;
	ctx.fillRect(7.5, 7.5, 28, 28);

	// Draw the border.
	ctx.lineWidth = 1.0;
	ctx.strokeStyle = "#777777";
	ctx.strokeRect(7.5, 7.5, 28, 28);

	// Draw the horizontal line for minus.
	ctx.lineWidth = 2.0;
	ctx.strokeStyle = "#555555";
	ctx.beginPath();
	ctx.moveTo(12, 21);
	ctx.lineTo(30, 21);

	if (this.mapIsZoomed())
	{
		zoomControl.onclick = new Function("maClient.map.setMapZoomInOut(-10);");
	}
	else
	{
		// Draw the vertical line for plus.
		ctx.moveTo(21, 12);
		ctx.lineTo(21, 30);
		zoomControl.onclick = new Function("maClient.map.setMapZoomInOut(10);");
	}

	ctx.stroke();

	ctx.restore();
};

maClient.Html5.prototype.startTouchDelay = function()
{
	// This method sets a delay period during which touch events should be ignored.
	// It is used following a zoom or pan gesture to prevent the gesture's touch
	// events from being interpreted as the user intending to touch a hotspot.
	// Without this delay, a hotspot can get selected at the position on the map where
	// the user spread their fingers to zoom the map in.
	//
	// The timeout of 500ms seems to work well on iPad 1. A value of 250 was too short.
	// Prior to using this delay we tried using flags to track when a pan or zoom event
	// occurred and to ignore subsequent touches, but the event sequence was not always
	// the same and so the logic didn't always work. It was also affected by whether or
	// not the selectsOnTouchStart option was set.
	if (typeof this.ignoreTouchIntervalId != "undefined")
	{
		clearInterval(this.ignoreTouchIntervalId);
	}
	
	maClient.map.ignoreTouch = true;
	ignoreTouchIntervalId = setTimeout("maClient.map.ignoreTouch = false;", 500);
};

maClient.Html5.prototype.stopSlideShow = function()
{
	if (this.slideShowRunning)
	{
		clearInterval(this.slideShowIntervalId);
		this.slideShowRunning = false;
		maClient.flashSlideShowIsRunning(false);
	}
};

maClient.Html5.prototype.switchView = function(markerInstance, isDirEntry)
{
	//console.log("switchView");

	if (markerInstance === null)
	{
		// This can happen if the marker for the tour's first tour view is not on the map
		// and someone chooses the marker from the directory.
		return;
	}

	var transferringToAnotherPage = false;
	var performClickAction = false;

	if (!isDirEntry && !this.slideShowRunning)
	{
		// Perform this marker's mouse action if it has one.
		if (markerInstance.clickActionTarget.length > 0 && markerInstance.touchPerformsClickAction && maClient.isTouchDevice)
		{
			transferringToAnotherPage = markerInstance.clickAction != maActionCallJavascript;
			performClickAction = true;
			this.executeClickAction(markerInstance);
		}
		else
		{
			var mouseoverActionTarget = markerInstance.mouseoverActionTarget;
			if (mouseoverActionTarget.length > 0 && markerInstance.mouseoverAction == maActionCallJavascript)
			{
				maClient.flashExecuteJavaScript(mouseoverActionTarget);
			}
		}
	}

	var x = markerInstance.centerX() + this.mapPanX - markerInstance.anchorScaledX();
	var y = markerInstance.centerY() + this.mapPanY - markerInstance.anchorScaledY();

	var w = markerInstance.drawnWidth();
	var h = markerInstance.drawnHeight();

	var mouseX = this.mouseX + this.mapLocationInBrowser.x;
	var mouseY = this.mouseY + this.mapLocationInBrowser.y;

	if (!markerInstance.doesNotShowContent && !transferringToAnotherPage)
	{
		maClient.flashViewChanged(markerInstance.viewId, x, y, w, h, mouseX, mouseY, isDirEntry);
	}

	this.selectedViewId = markerInstance.viewId;

	if (!markerInstance.isStatic)
	{
		this.redrawMarker(markerInstance, true);
	}

	if (!isDirEntry && !performClickAction && (markerInstance.doesNotShowContent || !maClient.usesHidablePopup))
	{
		// It's okay for the new hotspot to display a tooltip because it either
		// does not display any content or it displays content in a fixed popup.
		if (this.popupIsShowing)
		{
			// The previous hotspot's popup is showing so close it.
			maClosePopup();
		}
		this.showTooltip(markerInstance);
	}
};

maClient.Html5.prototype.waitForMapTileImagesToLoad = function()
{
	console.log("waitForMapTileImagesToLoad " + this.waitForMapTileImagesAttempts);

	// The data for the map image tiles has been loaded and the map has finished
	// initializing, but we need to wait until all the map image objects have loaded.

	var delayPeriod = 50;
	var wait = false;

	for (var level = 0; level < this.mapLevels.length; level++)
	{
		var mapLevel = this.mapLevels[level];

		// Wait for up to 10 seconds for all the image objects to load. Since the image data
		// file has already been loaded from the server, the objects should load quickly.
		if (this.waitForMapTileImagesAttempts < 200)
		{
			// See if every image has loaded. Quit as soon as we find one that's not loaded yet.
			for (var index = 0; index < mapLevel.tiles.length; index++)
			{
				var tile = mapLevel.tiles[index];
				if (!tile.loaded || !this.mapInsetImageLoaded)
				{
					wait = true;
					break;
				}
			}

			if (wait)
			{
				this.waitForMapTileImagesAttempts++;
				break;
			}
		}
	}

	if (wait)
	{
		setTimeout("maClient.map.waitForMapTileImagesToLoad();", delayPeriod);
		return;
	}
	else
	{
		this.mapTileImagesLoaded = true;
		console.log("Map tile image objects loaded in " + (delayPeriod * this.waitForMapTileImagesAttempts) + " ms");
	}

	if (!this.mapTileImagesLoaded)
	{
		// This should not happen, but if it does at least the user can tell us this message.
		this.drawLoadingMessage("Could not load map image");
		return;
	}

	this.drawMap();

	this.showInstructions();
	this.runSlideShow();

	maClient.onMapLoaded();
};
