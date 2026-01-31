// Copyright (C) 2009-2022 AvantLogic Corporation: https://www.mapsalive.com

export { MapsAliveGraphics };

import { MapsAliveMarkerStyleProperties as MarkerStyleProperties__$$ } from './mapsalive-marker.js ';
import { MapsAliveRuntime as Runtime__$$ } from './mapsalive-runtime.js';

class MapsAliveGraphics
{
	constructor(tour, map)
	{
		this.tour = tour;
		this.map = map;

		this.ctx = new Object();
		this.ctx.markerLayer = map.markerLayerContext;
		this.ctx.hitLayer = map.hitLayerContext;
		this.ctx.mapLayer = map.mapLayerContext;
	}

	blendOnto(marker, mode)
	{
		// This method was adapted by AvantLogic based on code from http://github.com/Phrogz/context-blender
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

		// The above copyright notice and this permission notice shall be included in
		// all copies or substantial portions of the Software.

		// Ignore the blend request if slow graphics operations are disabled such as when zooming or panning.
		if (!this.map.slowGraphicsAllowed)
			return;

		mode = mode.toLowerCase();

		// Normal doesn't do anything and we don't currently support invert.
		if (mode.length === 0 || mode == "normal" || mode == "invert")
			return;

		// Get the shape's hit bounds which completly encompass the shape even if it is rotated.
		let bounds = marker.getBounds();
		let x = bounds.hitX1_screen;
		let y = bounds.hitY1_screen;
		let w = bounds.hitW_screen;
		let h = bounds.hitH_screen;

		// Clip the size of the shape if partially off the map to prevent an image data error.
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

		if (x + w > this.map.canvasW)
			w -= x + w - this.map.canvasW;
		if (y + h > this.map.canvasH)
			h -= y + h - this.map.canvasH;
		if (w <= 0 || h <= 0)
			return;

		// Erase the hit layer and draw the marker's shape on it. We'll use this layer to tell which
		// pixels are part of the shape and which are not so that we only blend pixels within the shape.
		this.ctx.hitLayer.clearRect(0, 0, this.map.canvasW, this.map.canvasH);
		this.drawShape(marker, this.ctx.hitLayer, false);

		// Read the image data for the exact same region on each of the hit, map, and marker layers.
		let hitLayerImageData;
		let mapLayerImageData;
		let markerLayerImageData;
		try
		{
			hitLayerImageData = this.ctx.hitLayer.getImageData(x, y, w, h);
			mapLayerImageData = this.ctx.mapLayer.getImageData(x, y, w, h);
			markerLayerImageData = this.ctx.markerLayer.getImageData(x, y, w, h);
		}
		catch (error)
		{
			console.log(`ERROR blendOnto: ${error.message}`);
			console.log("== " + x + "," + y + " : " + w + "x" + h);
			return;
		}

		// Get the array of pixels for each layer.
		let hit = hitLayerImageData.data;
		let src = mapLayerImageData.data;
		let dst = markerLayerImageData.data;

		// Get the total number of pixels. Each contains four bytes (RGBA), the last being the alpha channel.
		let pixelCount = dst.length;

		// console.log(`Graphics::blendOnto ${marker.viewId} ${w}x${h} Pixels:${pixelCount}`);

		for (let px = 0; px < pixelCount; px += 4)
		{
			if (hit[px + 3] === 0)
			{
				// The pixel that was drawn on the hit test layer is not part of the marker's shape.
				continue;
			}

			// Calculate the blended alpha value.
			let sA = src[px + 3] / 255;
			let dA = dst[px + 3] / 255;
			let dA2 = (sA + dA - sA * dA);
			dst[px + 3] = dA2 * 255;

			// Get the source and destination RGB values.
			let sRA = src[px] / 255 * sA;
			let dRA = dst[px] / 255 * dA;
			let sGA = src[px + 1] / 255 * sA;
			let dGA = dst[px + 1] / 255 * dA;
			let sBA = src[px + 2] / 255 * sA;
			let dBA = dst[px + 2] / 255 * dA;

			let demultiply = 255 / dA2;

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
		this.ctx.markerLayer.putImageData(markerLayerImageData, x, y);
	};

	convertHexColorToRgb(hex)
	{
		//console.log("HEX " + hex);
		let r = 0;
		let g = 0;
		let b = 0;
		if (hex.length === 8)
		{
			if (hex.substr(0, 2) === "0x")
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
		let color = new Object();
		color.r = r;
		color.g = g;
		color.b = b;
		return color;
	}

	convertIntegerColorToCss(integerColor)
	{
		if ((integerColor + " ").substr(0, 1) === "#")
		{
			// The integer value was passed in as a CSS color like "#aabbcc".
			return integerColor;
		}

		let hex = this.convertIntegerColorToHex(integerColor);
		return "#" + hex.substr(2);
	}

	convertIntegerColorToHex(integerColor)
	{
		if ((integerColor + "  ").substr(0, 2).toLowerCase() === "0x")
		{
			// The integer value was passed in as a string like "0xaabbcc".
			return integerColor;
		}

		// Make sure the value is an integer (it might be an integer passed as a string).
		integerColor = parseInt(integerColor, 10);

		let hex = integerColor.toString(16);
		let pad = "00000";
		if (hex.length < 6)
		{
			hex = pad.substring(0, 6 - hex.length) + hex;
		}
		return "0x" + hex;
	}

	createRgbaFromHexColor(hexColor, alpha)
	{
		let c = this.convertHexColorToRgb(hexColor);
		return "rgba(" + c.r + "," + c.g + "," + c.b + "," + alpha + ")";
	}

	createRgbaFromIntegerColor(integerColor, alpha)
	{
		let hexColor = this.convertIntegerColorToHex(integerColor);
		return this.createRgbaFromHexColor(hexColor, alpha);
	}

	drawBoundsForDebugging(marker, color, pattern = [])
	{
		let b = marker.getBounds();

		let ctx = this.ctx.markerLayer;
		ctx.save();

		let x = b.hitX1_screen;
		let y = b.hitY1_screen;
		let w = b.hitW_screen;
		let h = b.hitH_screen;

		ctx.globalAlpha = 1.0;
		ctx.setLineDash(pattern);
		ctx.strokeStyle = color;
		let offset = 0.5;
		ctx.strokeRect(x + offset, y + offset, w, h);

		ctx.restore();
	}

	drawCircle(marker, x, y, radius, ctx, fillAlpha, lineAlpha, lineWidth)
	{
		if (this.enlargeHitTestArea && ctx === this.ctx.hitLayer)
		{
			// We are hit testing on a small symbol or symbol+shape marker.
			// Draw the shape a little larger to make it easier to touch.
			let delta = this.map.minHitTargetSize - (radius * 2);
			if (delta > 0)
			{
				let pad = this.map.convertScreenToMap(delta / 2);
				radius += pad;

				// Draw the enlarged hit area for debugging purposes by calling this method
				// recursively, but passing the marker layer context instead of the hit layer context.
				if (this.tour.flagShowSmallMarkers)
					this.drawCircle(marker, x, y, radius, this.ctx.markerLayer, 0.1, 0.2);
			}
		}

		ctx.beginPath();
		ctx.arc(x, y, radius, 0, Math.PI * 2, true);
		ctx.closePath();

		if (lineWidth > 0 && ctx !== this.ctx.hitLayer)
		{
			// Erase the line.
			ctx.globalCompositeOperation = "destination-out";
			ctx.globalAlpha = 1.0;
			ctx.stroke();
		}

		if (ctx !== this.ctx.hitLayer)
		{
			// Erase the circle
			ctx.globalCompositeOperation = "destination-out";
			ctx.globalAlpha = 1.0;
			ctx.fill();
		}

		// Draw the circle.
		ctx.globalCompositeOperation = "source-over";
		ctx.globalAlpha = fillAlpha;
		ctx.fill();

		// Draw the line.
		if (lineWidth > 0)
		{
			ctx.globalCompositeOperation = "source-over";
			ctx.globalAlpha = lineAlpha;
			ctx.stroke();
		}
	}

	drawHandles(marker)
	{
		if (!marker.isBeingEdited)
			return;

		if (marker.shapeIsHybrid)
			return;

		//console.log(`Graphics::drawHandles ${marker.markerId}`);

		this.map.editor.eraseHandlesLayer();
		let ctx = this.map.handlesLayerContext;

		ctx.strokeStyle = marker.handles.lineColorBetweenHandles;

		let handleList = marker.handles.getHandleList();

		// Draw a line between the handles of a polygon or line.
		if (marker.shapeHasPoints)
		{
			ctx.globalAlpha = 0.50;
			ctx.beginPath();
			let firstHandle = true;

			//console.log(`Graphics::drawHandles ${marker.markerId} ${marker.shapeCoords}`);

			for (const handle of handleList)
			{
				let x = handle.x;
				let y = handle.y;

				if (firstHandle)
					ctx.moveTo(x, y);
				else
					ctx.lineTo(x, y);
				firstHandle = false;
			}

			if (marker.shapeIsPolygon)
				ctx.closePath();

			ctx.lineWidth = 1;
			ctx.stroke();
		}

		// Draw the handles.
		ctx.lineWidth = 1;
		for (const handle of handleList)
		{
			// Shift the point by a half pixel to get a crisp line.
			let hx = handle.xh - 0.5;
			let hy = handle.yh - 0.5;
			const w = handle.handles.HANDLE_SIDE;
			const h = handle.handles.HANDLE_SIDE;

			ctx.globalAlpha = marker.handles.handleBeingDragged ? 0 : handle.handleColor.fillAlpha;
			ctx.fillStyle = handle.handleColor.fill;
			ctx.fillRect(hx, hy, w, h);

			ctx.globalAlpha = 1.0;
			ctx.strokeStyle = handle.handleColor.line;
			ctx.strokeRect(hx, hy, w, h);
		}

		if (marker.handles.handleBeingDragged)
			return;

		// Draw the break points.
		marker.handles.setBreakPoints();
		for (const breakPoint of marker.handles.breakPoints)
		{
			const color = breakPoint.breakPointColor(breakPoint);

			const radius = 4;

			let x = breakPoint.x;
			let y = breakPoint.y;

			ctx.beginPath();
			ctx.arc(x, y, radius, 0, Math.PI * 2, true);
			ctx.closePath();
			ctx.globalAlpha = color.fillAlpha;
			ctx.fillStyle = color.fill;
			ctx.fill();

			ctx.globalAlpha = 1.0;
			ctx.strokeStyle = color.line;
			ctx.stroke();
		}
	}

	drawHybridShape(ctx, hybridMarker, fillAlpha, lineAlpha, lineWidth)
	{
		let coords = hybridMarker.shapeCoordsArray;

		//if (ctx !== null && ctx !== this.ctx.hitLayer) console.log("drawHybridShape: " + coord);

		let i = 0;
		while (i < coords.length)
		{
			// The first pair of the coordinates indicates what type of shape to draw.
			// A first value of -1 means section start. The second value has the shape type.
			if (coords[i] !== -1)
			{
				Runtime__$$.assert(false, `Unexpected hybrid shape coords at ${i}=${coords[i]}`);
				return;
			}

			let shapeType = coords[i + 1];
			i += 2;

			switch (shapeType)
			{
				case hybridMarker.SHAPE_TYPE_CIRCLE:
					i += this.drawHybridShapeCircle(ctx, hybridMarker, coords, i, fillAlpha, lineAlpha, lineWidth);
					break;

				case hybridMarker.SHAPE_TYPE_RECTANGLE:
					i += this.drawHybridShapeRectangle(ctx, hybridMarker, coords, i, fillAlpha, lineAlpha, lineWidth);
					break;

				case hybridMarker.SHAPE_TYPE_POLYGON:
				case hybridMarker.SHAPE_TYPE_LINE:
					i += this.drawHybridShapePolygonOrLine(ctx, hybridMarker, i, shapeType, fillAlpha, lineAlpha, lineWidth);
					break;
			}
		}
	}

	drawHybridShapeCircle(ctx, hybridMarker, coords, i, fillAlpha, lineAlpha, lineWidth)
	{
		//console.log("drawHybridShapeCircle: " + coord);

		let x = coords[i];
		let y = coords[i + 1];
		let r = coords[i + 2];

		let length = 4;
		if (i + 3 >= coords.length || coords[i + 3] == "-1")
		{
			// These are old V2 circle coords where the unused 4th value is missing and
			// the position has to be moved right and down to the center of the circle.
			length = 3;
			x += r;
			y += r;
		}

		// Shift to the upper left corner of the rectangle that encloses the circle.
		x -= hybridMarker.halfShapeW;
		y -= hybridMarker.halfShapeH;

		this.drawCircle(hybridMarker, x, y, r, ctx, fillAlpha, lineAlpha, lineWidth);

		// Return the number of coords consumed by this method. The value is
		// 4 instead of 3 because there is an unused value after the radius.
		return length;
	}

	drawHybridShapePolygonOrLine(ctx, hybridMaker, i, shapeType, fillAlpha, lineAlpha, lineWidth)
	{
		let isPolygon = shapeType === hybridMaker.SHAPE_TYPE_POLYGON;
		let length = this.drawPoints(ctx, hybridMaker, i, fillAlpha, lineAlpha, isPolygon, lineWidth);
		return length;
    }

	drawHybridShapeRectangle(ctx, hybridMarker, coords, i, fillAlpha, lineAlpha, lineWidth)
	{
		// When drawing a rectangle using vector graphics, you would draw from the origin to the width,
		// not width - 1 as you would with a bitmap. Our rectangle coordinates are non-vector so we have
		// to add 1 to the width and height. 

        let x1 = coords[i];
        let y1 = coords[i + 1];
        let x2 = coords[i + 2] + 1;
		let y2 = coords[i + 3] + 1;

        // Handle lines with an odd number of pixels. When drawing rectangles, we want odd
        // pixels drawn outside the rectangle and even pixels inside. To accomplish this
        // we have to increase the width by a pixel to counteract the fact that Flash
        // drew odd pixels to the left of vertical lines and above horizontal lines.
        // For a rectangle that means that the odd pixes are drawn outside on the left
        // side and inside on the right side (outside on the top and inside on the bottom).
        // That's very confusing an unintuitive. It also means that when you add a 1 pixel
        // border to a 100x100 rectangle it ends up being 101x101. Our adjustment here
        // makes the rectangle be 102x102 as you would expect.
		if (lineWidth % 2 === 1)
		{
            x2++;
            y2++;
        }

        let w = x2 - x1;
        let h = y2 - y1;

        // Shift to the upper left corner of the rectangle.
        let x = x1 - hybridMarker.halfShapeW;
        let y = y1 - hybridMarker.halfShapeH;

		this.drawRectangle(hybridMarker, x, y, w, h, ctx, fillAlpha, lineAlpha, lineWidth);

		// Return the number of coords consumed by this method.
		return 4;
	}

	drawIntersectingMarkers(markers, selected, useCache = false)
	{
		if (this.map.markerDrawingIsDisabled)
			return;

		// This method erases and then redraws each of the passed-in markers and every marker that
		// intersects those markers. Markers are redrawn bottom to top according to their stacking
		// order. This method is used whenever a marker's appearance has changed.

		let time = Date.now();

		// Determine if any of the intersecting markers cannot be drawn onto the cached marker layer
		// because the selected appearance of one or more of the markers can only be drawn onto a clean
		// canvas in order to prevent bits of the cached normal appearance from being visible.
		let markerSetCannotUseCache = selected && this.markerSetCannotUseCache(markers)

		if (useCache && markerSetCannotUseCache)
			useCache = false;

		// Override the cache parameter passed by the caller if caching is not enabled,
		// for example when in the Map Editor or when debugging with caching disabled.
		if (useCache && !this.map.markerLayerCacheEnabled)
			useCache = false;

		if (useCache)
		{
			// Callers of this method set useCache to true when they know that the appearance
			// of the map's markers have not changed since the marker layer was last cached.
			this.map.drawAllMarkers(true);
		}
		else if (this.map.markerLayerCacheEnabled && !markerSetCannotUseCache)
		{
			// The caller assumes that the cache is stale. Flush the cache and then recreate it
			// by virture of calling drawAllMakers. This is NOT done if markerSetCannotUseCache
			// is true, because in that case, it's not necessary to draw all the markers again.
			// Instead, because useCache is false, execution skips the code below and instead
			// performs the call to eraseBoundingAreaOfIntersectingMarkers logic which only erases
			// the area of the marker layer canvas that contains all the markers in the set,
			// followed by logic to only draw markers that are in the set.
			this.map.flushMarkerLayerCache();
			this.map.drawAllMarkers(true);
			useCache = true;
		}

		// Draw an outline around the selected marker when in the map editor.
		if (this.tour.editMode)
		{
			// When called for the Map Editor, only one marker is passed in the markers array.
			let marker = markers[0];
			if (selected && !marker.isRoute)
				this.map.editor.drawOutlineForMarker(marker);
		}

		// Find all the other markers that intersect with the passed-in markers.
		this.intersectingMarkers = [];

		if (!useCache)
			this.eraseBoundingAreaOfIntersectingMarkers(markers, time);

		// Loop over all the markers and draw the ones that are intersecting which includes the
		// passed-in marker. The markers get drawn in their stacking order from bottom to top.
		for (const marker of this.map.markers)
		{
			// Ignore a non-intersecting marker.
			if (!useCache && !this.intersectingMarkers.includes(marker))
				continue;

			// Determine if this marker should be drawn as selected.
			let selected = (marker.viewId === this.map.selectedMarkerViewId && !marker.isStatic) || marker.appearsSelected;

			if (!useCache || selected)
				this.drawMarker(marker, selected);
		}

		//console.log(`Graphics::drawIntersectingMarkers Count:${this.intersectingMarkers.length} Time:${Math.round(Date.now() - time)}ms Selected:${selected}`);
	}

	drawMarker(marker, selected)
	{
		// Don't draw a hidden marker unless in the Map Editor.
		if (marker.isHidden && !this.tour.editMode)
			return;

		// Don't draw a marker that is hidden at the current zoom level or is off the canvas.
		if (!marker.isRoute && (!marker.isVisibleOnCanvas || !marker.isVisibleAtCurrentZoomLevel))
		{
			//let view = this.tour.currentPage.getView(marker.viewId);
			//console.log(`NOT VISIBLE ${marker.viewId} ${marker.x},${marker.y} selected:${selected}`);
			return;
		}

		// Don't draw the marker for a route that has not been defined yet.
		if (marker.isRoute && !marker.routeDefined)
			return;

		if (this.tour.flagShowMarkerBounds)
			this.drawBoundsForDebugging(marker, 'cyan', [7, 3, 3]);

		if (marker.isStatic)
			selected = false;

		if (marker.appearsSelected)
			selected = true;

		// See if the map editor has requested that unselected markers appear as selected and vice versa.
		if (this.map.showMarkerAppearanceToggled)
			selected = !selected;

		// Every marker has a shape except for symbol markers.
		let hasShape = !marker.typeIsSymbol;

		// Every marker has a symbol except for shape markers.
		let hasSymbol = !marker.typeIsShape;

		if (hasShape)
		{
			if (marker.shapeIsSvg)
			{
				this.drawSvgShape(marker, this.ctx.markerLayer, selected);
			}
			else
			{
				this.drawShape(marker, this.ctx.markerLayer, selected);
				this.blendOnto(marker, marker.markerStyle.getProperties(selected).effects.blendMode);
			}
		}

		if (hasSymbol)
			this.drawSymbol(marker, this.ctx.markerLayer, selected);

		//console.log(`Graphics::drawMarker ${selected} [${marker.isHidden}] ${marker.viewId} ${marker.x},${marker.y} ${marker.pctX},${marker.pctY}`);
	}

	drawPoints(ctx, marker, index, fillAlpha, lineAlpha, isPolygon, lineWidth)
	{
		// Calculate the offset without rounding so that the drawing code can work with fractional pixels.
		let x = -(marker.shapeW / 2);
		let y = -(marker.shapeH / 2);

		let length = 0;
		let coords = marker.shapeCoordsArray;

		ctx.beginPath();

		for (let i = index; i < coords.length; i += 2)
		{
			let cx = coords[i];
			let cy = coords[i + 1];
			//if (ctx !== this.ctx.hitLayer) console.log("== " + i + " : " + cx + "," + cy);

			if (cx === -1)
				break;

			length += 2;

			cx += x;
			cy += y;

			if (i === 0)
				ctx.moveTo(cx, cy);
			else
				ctx.lineTo(cx, cy);
		}

		if (lineWidth > 0 && ctx !== this.ctx.hitLayer)
		{
			// Erase the line.
			ctx.globalCompositeOperation = "destination-out";
			ctx.globalAlpha = 1.0;
			ctx.stroke();
		}


		if (isPolygon)
		{
			// Connect the last point to the first point.
			ctx.closePath();

			// Erase the fill area to avoid the problem where a semi-transparent polygon
			// gets darker and darker because the new fill is being drawn on top of the old.
			if (ctx !== this.ctx.hitLayer && !marker.isHidden && fillAlpha > 0 && fillAlpha < 1.0)
			{
				ctx.globalCompositeOperation = "destination-out";
				ctx.globalAlpha = 1.0;
				ctx.fill();
			}

			// Draw the fill area.
			ctx.globalCompositeOperation = "source-over";
			ctx.globalAlpha = fillAlpha;
			ctx.fill();
		}

		if (lineWidth > 0)
		{
			// Draw the line.
			ctx.globalCompositeOperation = "source-over";
			ctx.globalAlpha = lineAlpha;
			ctx.stroke();
		}

		return length;
	}

	drawRectangle(marker, x, y, w, h, ctx, fillAlpha, lineAlpha, lineWidth)
	{
		if (this.enlargeHitTestArea && ctx === this.ctx.hitLayer)
		{
			// We are hit testing on a small symbol or symbol+shape marker.
			// Draw the shape a little larger to make it easier to touch.
			let deltaW = this.map.minHitTargetSize - this.map.convertMapToScreen(w);
			if (deltaW > 0) 
			{
				let padW = this.map.convertScreenToMap(deltaW);
				w += padW;
				x -= Math.round(padW / 2);
			}
			let deltaH = this.map.minHitTargetSize - this.map.convertMapToScreen(h);
			if (deltaH > 0)
			{
				let padH = this.map.convertScreenToMap(deltaH);
				h += padH;
				y -= Math.round(padH / 2);
			}

			// Draw the enlarged hit area for debugging purposes by calling this method
			// recursively, but passing the marker layer context instead of the hit layer context.
			if (deltaW + deltaH > 0 && this.tour.flagShowSmallMarkers)
				this.drawRectangle(marker, x, y, w, h, this.ctx.markerLayer, 0.1, 0.2);
		}

		// Erase the line
		if (lineWidth > 0 && ctx !== this.ctx.hitLayer)
		{
			// Erase the line.
			ctx.globalCompositeOperation = "destination-out";
			ctx.globalAlpha = 1.0;
			ctx.strokeRect(x, y, w, h);
		}

		// Erase the rectangle.
		if (ctx !== this.ctx.hitLayer)
		{
			ctx.globalCompositeOperation = "destination-out";
			ctx.globalAlpha = 1.0;
			ctx.fillRect(x, y, w, h);
		}

		// Draw the rectangle.
		ctx.globalCompositeOperation = "source-over";
		ctx.globalAlpha = fillAlpha;
		ctx.fillRect(x, y, w, h);

		if (lineWidth > 0)
		{
			// Draw the line.
			ctx.globalCompositeOperation = "source-over";
			ctx.globalAlpha = lineAlpha;
			ctx.strokeRect(x, y, w, h);
		}
	}

	drawRouteThroughMarkers(routeId, viewId, lineWidth, lineColor, lineAlpha, viewIdList, effects)
	{
		if (viewIdList === "0")
		{
			// Erase all the routes and redraw the map.
			for (let marker of this.map.markers)
				marker.defineRoute("");
			this.map.drawMap();
			return false;
		}

		// Determine if drawing a route using a route hotspot, or drawing a set of routes.
		let drawingRouteSet = viewId === 0;

		// When there's no viewId, use the default marker as the route hotspot.
		let marker = drawingRouteSet ? this.map.defaultMarker : this.map.getMarker(viewId);

		console.log(`Graphics::drawRouteThroughMarkers ${routeId} ${viewId}`);

		let markerStyle = marker.markerStyle;
		markerStyle.lineWidth = lineWidth;

		if (marker.routeDefined && !drawingRouteSet)
		{
			// Erase the previous route drawn for this marker in case its definition has changed.
			markerStyle.baseProperties.normal.lineColorOpacity = 0;
			this.drawIntersectingMarkers([marker], false);
		}

		marker.defineRoute(viewIdList);

		let lineColorCss = this.convertIntegerColorToCss(lineColor);

		// Create and set custom properties for the line color, line alpha, and effects.
		let normalProperties = new MarkerStyleProperties__$$(this.map, '#000000', 0, lineColorCss, lineAlpha, effects);
		let selectedProperties = new MarkerStyleProperties__$$(this.map, '#000000', 0, lineColorCss, lineAlpha, effects);
		markerStyle.setBasePropertiesNormal(normalProperties);
		markerStyle.setBasePropertiesSelected(selectedProperties);

		// Force the routes's bounds to get calculated.
		marker.updateBounds();

		// When drawing a set of routes, just draw one route after another without concern for
		// overlapping markers. When drawing a route using a route hotspot, make sure that the
		// neighboring markers are redrawn as well.
		if (drawingRouteSet)
			this.drawMarker(marker, true);
		else
			this.drawIntersectingMarkers([marker], true);

		if (this.tour.flagShowMarkerBounds)
			this.drawBoundsForDebugging(marker, 'red', [7, 3, 3]);

		return true;
	}

	drawShape(marker, ctx, selected)
	{
		Runtime__$$.assert(ctx !== null, 'ctx is null');

		this.enlargeHitTestArea = ctx === this.ctx.hitLayer && this.okToEnlargeHitTestArea(marker);
		let debugging = this.enlargeHitTestArea && this.tour.flagShowSmallMarkers;

		this.setDrawingSurface(ctx, marker);

		if (debugging)
			this.setDrawingSurface(this.ctx.markerLayer, marker);

		this.fillAlpha = 1.0;
		this.lineAlpha = 1.0;

		let x = -marker.halfShapeW - marker.centerOffsetX;
		let y = -marker.halfShapeH - marker.centerOffsetY;

		let properties = marker.markerStyle.getProperties(selected);
		let effects = properties.effects;

		this.lineWidth = marker.lineWidth();

		// Make the hit area for line shapes thicker so that line markers are easier to select.
		if (this.lineWidth < 8 && marker.shapeIsLine && ctx === this.ctx.hitLayer)
			this.lineWidth = 8;

		ctx.lineWidth = this.lineWidth;
		ctx.setLineDash(properties.effects.lineDash);

		// Draw the marker's effects.
		if (ctx === this.ctx.markerLayer)
		{
			this.setEffects(ctx, properties, effects, marker);
			//console.log(`Graphics::drawMarkerShape '${ctx.canvas.id}' ${marker.viewId} ${selected} ${ctx.fillStyle}`);
		}

		// Draw the marker's shape and line.
		switch (marker.shapeType)
		{
			case marker.SHAPE_TYPE_CIRCLE:
				let radius = Math.round(marker.shapeW / 2);
				this.drawCircle(marker, x + radius, y + radius, radius, ctx, this.fillAlpha, this.lineAlpha, this.lineWidth);
				break;

			case marker.SHAPE_TYPE_POLYGON:
			case marker.SHAPE_TYPE_LINE:
				ctx.lineJoin = "bevel";
				this.drawPoints(ctx, marker, 0, this.fillAlpha, this.lineAlpha, marker.shapeIsPolygon, this.lineWidth);
				break;

			case marker.SHAPE_TYPE_HYBRID:
				ctx.lineJoin = "bevel";
				this.drawHybridShape(ctx, marker, this.fillAlpha, this.lineAlpha, this.lineWidth);
				break;

			case marker.SHAPE_TYPE_RECTANGLE:
				this.drawRectangle(marker, x, y, marker.shapeW, marker.shapeH, ctx, this.fillAlpha, this.lineAlpha, this.lineWidth);
				break;

			default:
				// Treat a symbol marker like a rectangle shape for the purpose of hit testing.
				if (marker.typeIsSymbol && ctx === this.ctx.hitLayer)
					this.drawRectangle(marker, x, y, marker.shapeW, marker.shapeH, ctx, this.fillAlpha, this.lineAlpha, this.lineWidth);
				break;
		}

		if (ctx !== this.ctx.hitLayer)
			this.drawHandles(marker);

		this.restoreDrawingSurface(ctx);

		if (debugging)
			this.restoreDrawingSurface(this.ctx.markerLayer);
	}

	drawSymbol(marker, ctx, selected, img = null)
	{
		//console.log(`Graphics::drawSymbol ${marker.viewId} ${selected}`);

		if (img === null) ////// SVG
			img = this.getSymbolImage(marker, selected);
		if (img === null)
			return;

		this.setDrawingSurface(ctx, marker);

		let x;
		let y;

		let symbolW = selected ? marker.selectedSymbolW : marker.normalSymbolW;
		let symbolH = selected ? marker.selectedSymbolH : marker.normalSymbolH;

		switch (marker.markerType)
		{
			case marker.MARKER_TYPE_SYMBOL:
				x = -Math.round(symbolW / 2);
				y = -Math.round(symbolH / 2);
				break;

			case marker.MARKER_TYPE_SYMBOL_AND_SHAPE:
				let symbolLocationX = marker.symbolLocationX;
				let symbolLocationY = marker.symbolLocationY;

				x = -Math.round(symbolW / 2);
				y = -Math.round(symbolH / 2);

				if (symbolLocationX >= 0 || symbolLocationY >= 0)
				{
					// The symbol is not centered in the shape.
					x = x - marker.halfShapeW + symbolLocationX;
					y = y - marker.halfShapeH + symbolLocationY;
				}
				break;

			default:
				x = -marker.halfShapeW;
				y = -marker.halfShapeH;

				let lineWidth = marker.lineWidth();
				if (lineWidth > 1)
				{
					// Adjust the position of the symbol within its shape.
					lineWidth = Math.round((lineWidth - 1) / 2);
					x += lineWidth;
					y += lineWidth;
				}
		}

		x -= marker.centerOffsetX;
		y -= marker.centerOffsetY;

		// Set the symbol's opacity.
		let globalAlpha = 1.0;
		if (marker.blinkAlpha < 1.0)
			globalAlpha = marker.blinkAlpha;
		else if (marker.visited && this.map.visitedMarkerAlpha < 1.0)
			globalAlpha = this.map.visitedMarkerAlpha;

		// See if the Map Editor has requested that selected markers be shown transparent.
		// In the Map Editor, hidden markers are always shown transparent.
		if ((this.map.showMarkerTransparencyToggled && marker.appearsSelected) || marker.isHidden)
			globalAlpha /= 2;

		ctx.globalAlpha = globalAlpha;

		// Draw the symbol.
		ctx.drawImage(img, x, y);

		this.restoreDrawingSurface(ctx);
	}

	////// SVG
	drawSvgShape(marker, ctx, selected)
	{
		if (marker.svgImg !== null)
		{
			this.drawSvgShapeHandler(marker, ctx, selected, marker.svgImg);
			return true;
		}

		let img = new Image();

		img.onload = function ()
		{
			let img = this;
			let request = img.svgRequest;
			request.marker.svgImg = img;
			request.graphics.drawSvgShapeHandler(request.marker, request.ctx, request.selected);
		}

		// Assign to the img object a request object containing the request data.
		// Eventually this should be done as part of a preloading process so that an SVG
		// shape can be drawn immediately without having to wait for its onload event to fire.
		// That would also save having to recreate the img every time. If the shapes style
		// was changed via the API, the data would have to be regenerated. So would it be
		// better/possible to await for the event to fire?
		img.svgRequest = { graphics: this, marker, ctx, selected };

		let encodedSvg = encodeURIComponent(marker.shapeCoords);
		img.src = `data:image/svg+xml;charset=utf-8,${encodedSvg}`;

		return true;
	}

	////// SVG
	drawSvgShapeHandler(marker, ctx, selected)
	{
		this.drawSymbol(marker, ctx, selected, marker.svgImg);
		this.blendOnto(marker, marker.markerStyle.getProperties(selected).effects.blendMode);
	}

	eraseBoundingAreaOfIntersectingMarkers(markers, time)
	{
		for (const marker of markers)
			this.findIntersectingMarkers(marker);

		for (let marker of this.intersectingMarkers)
			this.eraseMarker(marker);
	}

	eraseMarker(marker)
	{
		// This method erases the marker's hit area which is large enough
		// to contain both the marker itself and the area outside the marker
		// that might be drawn with a glow or shadow effect.

		let b = marker.getBounds();
		let x = b.hitX1_screen;
		let y = b.hitY1_screen;
		let w = b.hitX2_screen - x;
		let h = b.hitY2_screen - y;
		let ctx = this.ctx.markerLayer;

		ctx.clearRect(x, y, w, h);

		if (this.tour.flagShowMarkerClippingRect)
		{
			ctx.clearRect(x, y, w, h);
			ctx.fillStyle = "cyan";
			ctx.globalAlpha = 0.25;
			ctx.fillRect(x, y, w, h);
		}
	}

	findIntersectingMarkers(marker)
	{
		// This method calls itself recursively. It unwinds when every marker that's
		// visible on the map has been examine to determine if it intersects with the
		// passed-in marker.

		//console.log(`Graphics::findIntersectingMarkers ${marker.viewId}`);

		if (this.intersectingMarkers.includes(marker))
			return;

		// Add the passed-in marker to the list of intersecting markers.
		this.intersectingMarkers.push(marker);

		for (const otherMarker of this.map.markers)
		{
			// Ignore markers that have already been identified as intersecting.
			if (this.intersectingMarkers.includes(otherMarker))
				continue;

			// Ignore hidden markers except for routes and when using the Map Editor.
			if (otherMarker.isHidden && !otherMarker.isRoute && !this.tour.editMode)
				continue;

			// Ignore markers that are not visible on the canvas. Treat routes as always visble.
			if (!otherMarker.isRoute && (!otherMarker.isVisibleOnCanvas || !otherMarker.isVisibleAtCurrentZoomLevel))
				continue;

			// Find the markers that intersect with this other marker.
			if (marker.boundsIntersectsWithMarker(otherMarker))
				this.findIntersectingMarkers(otherMarker);
		}
	}

	getSymbolImage(marker, selected)
	{
		let hasImage = selected ? marker.hasImageS : marker.hasImageN;

		if (!hasImage)
			return null;

		let loaded = selected ? marker.imgLoadedS : marker.imgLoadedN;

		if (!loaded)
			return null;

		let img = selected ? marker.imgS : marker.imgN;

		return img;
	}

	markerSetCannotUseCache(markers)
	{
		for (const marker of markers)
		{
			// Return true if any of the markers is a symbol because if the selected symbol does 
			// not completely cover the normal symbol, part of the cached normal symbol will
			// appear beneath the selected symbol.
			if (marker.typeIsSymbol)
				return true;
		}
		return false;
	}

	okToEnlargeHitTestArea(marker)
	{
		return !marker.isHidden && (this.tour.enlargeHitTestArea || this.tour.flagShowSmallMarkers);
	}

	restoreDrawingSurface(ctx)
	{
		ctx.restore();
	}

	setDrawingSurface(ctx, marker)
	{
		Runtime__$$.assert(ctx !== null);

		ctx.save();

		// Set the location of a non-anchored marker relative to the canvas, not the map.
		if (!marker.isAnchored)
		{
			marker.x = Math.round(this.map.canvasW * marker.pctX);
			marker.y = Math.round(this.map.canvasH * marker.pctY);
			marker.updateBounds();
		}

		let x = marker.x;
		let	y = marker.y;

		// Adjust the marker's position and scaling.
		if (marker.isAnchored)
		{
			// Shift the location of an anchored marker to adjust for panning.
			x += this.map.panX_map;
			y += this.map.panY_map;

			if (!marker.markerZooms)
			{
				// For an anchored non-zoomable marker, scale its map location to screen coordinates.
				x = this.map.convertMapToScreen(x);
				y = this.map.convertMapToScreen(y);
			}
		}
		else if (marker.markerZooms)
		{
			// For a non-anchored zoomable marker, use its map coordinates as absolute screen coordinates.
			x = this.map.convertScreenToMap(x);
			y = this.map.convertScreenToMap(y);
		}

		// Scale the canvas so that the zoomable marker will be drawn at the correct size.
		if (marker.markerZooms || marker.isBound)
		{
			let s;
			if (marker.isBound)
				s = this.map.currentMapScale;
			else
				s = this.map.currentMapScale;
			ctx.scale(s, s);
		}

		// Shift right and down by a half pixel to avoid antialiasing when drawing rectangle borders
		// that have an odd width. The antialiasing makes the line look thick and faded, e.g. instead
		// of a crisp single pixel red line, the line looks like a 2 pixel rose colored line.
		if (marker.shapeIsRectangle && marker.lineWidth() % 2 !== 0)
		{
			x += 0.5;
			y += 0.5;
		}

		// Shift the drawing origin to be the center of the marker on the canvas.
		ctx.translate(x, y);

		// Rotate the canvas so that a rotated marker will be drawn correctly.
		if (marker.rotationRadians !== 0)
			ctx.rotate(marker.rotationRadians);
	}

	setEffects(ctx, properties, effects, marker)
	{
		ctx.fillStyle = properties.fillColor;
		ctx.strokeStyle = properties.lineColor;

		this.fillAlpha = properties.fillColorOpacity / 100;
		this.lineAlpha = properties.lineColorOpacity / 100;

		if (!marker.isBeingEdited && !marker.isBlinking)
		{
			// Apply the marker's glow and/or shadow effects. Don't show these while editing a marker or
			// while it is blinking because they will bleed outside of the shape and will get darker and
			// darker. This occurs because on each edit or blink, the shape's line and fill get erased and
			// redrawn, but the glow/shadow is not erased and so it gets redrawn on top of itself. Also,
			// glow and shadow are expensive and so not drawing them should improve performance.
			if (effects.glow)
			{
				ctx.shadowBlur = effects.glow.shadowBlur;
				ctx.shadowColor = effects.glow.shadowColor;
			}

			if (effects.shadow)
			{
				ctx.shadowOffsetX = effects.shadow.shadowOffsetX;
				ctx.shadowOffsetY = effects.shadow.shadowOffsetY;
				ctx.shadowBlur = effects.shadow.shadowBlur;
				ctx.shadowColor = effects.shadow.shadowColor;
			}
		}

		// Scale down the blur so that a zoomed out marker's blur is not as great as one that is zoomed in.
		// This is neccesary so that the blur stays within the marker's bounds which are also calculated
		// based on a scaled down value.
		if (marker.markerZooms && !marker.isBeingEdited)
			ctx.shadowBlur = this.map.convertMapToScreen(ctx.shadowBlur);

		// Apply the marker's line and fill alpha.
		if (marker.blinkAlpha < 1.0)
		{
			this.fillAlpha *= marker.blinkAlpha;
			this.lineAlpha *= marker.blinkAlpha;
		}
		else if (marker.visited && this.map.visitedMarkerAlpha < 1.0)
		{
			this.fillAlpha *= this.map.visitedMarkerAlpha;
			this.lineAlpha *= this.map.visitedMarkerAlpha;
		}

		// See if the Map Editor has requested that selected markers be shown transparent.
		// In the Map Editor, hidden markers are always shown transparent.
		if ((!marker.isBeingEdited && this.map.showMarkerTransparencyToggled && marker.appearsSelected) || marker.isHidden)
		{
			this.lineAlpha /= 3;
			this.fillAlpha /= 3;
		}

		// Always use cyan when editing a marker.
		if (marker.isBeingEdited)
		{
			ctx.fillStyle = 'cyan';
			this.fillAlpha = 0.5;
			ctx.strokeStyle = 'cyan';
			ctx.lineAlpha = 1.0;
		}

		ctx.globalAlpha = this.fillAlpha;
	}
}