// Copyright (C) 2009-2022 AvantLogic Corporation: https://www.mapsalive.com

import { MapsAliveRuntime as Runtime__$$ } from './mapsalive-runtime.js';

export { MapsAliveMarker };
export { MapsAliveMarkerStyle };
export { MapsAliveMarkerStyleProperties };

class MapsAliveMarker
{
	constructor(
		map,
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
		shapeCoords)
	{
		this.map = map;
		this.markerId = parseInt(markerId, 10);
		this.viewId = parseInt(viewId, 10);
		this.markerType = markerType;
		this.styleId = styleId;
		this.globalMarkerStyle = this.map.getMarkerStyle(this.styleId);
		this.privateMarkerStyle = null;
		this.normalSymbolId = normalSymbolId;
		this.selectedSymbolId = selectedSymbolId;
		this.shapeType = shapeType;
		this.shapeW = shapeW;
		this.shapeH = shapeH;
		this.extra = { w: 0, h: 0 };
		this.normalSymbolW = normalSymbolW;
		this.normalSymbolH = normalSymbolH;
		this.selectedSymbolW = selectedSymbolW;
		this.selectedSymbolH = selectedSymbolH;
		this.symbolLocationX = symbolLocationX;
		this.symbolLocationY = symbolLocationY;
		this.centerOffsetX = centerOffsetX;
		this.centerOffsetY = centerOffsetY;
		this.zoomThreshold = zoomThreshold;
		this.flags = flags;
		this.isDisabled = (flags & 0x00000001) !== 0;
		this.isHidden = (flags & 0x00000002) !== 0;
		this.isStatic = (flags & 0x00000004) !== 0;
		this.isRoute = (flags & 0x00000008) !== 0;
		this.isLocked = (flags & 0x00000010) !== 0;
		this.markerZooms = (flags & 0x00000020) !== 0 || this.map.page.isGallery;
		this.isShapeOnly = (flags & 0x00000040) !== 0;

		// Invert the isNotAnchored flag so we can refer to it as the less confusing isAnchored.
		// Markers are anchored by default which means that are attached to the map such that when
		// a zoomed map is panned, the marker pans with it. A user can set a marker to not be anchored
		// so that it stays in the same location on the map canvas regardless of how the map is panned.
		this.isAnchored = (flags & 0x00000080) === 0;

		this.isBound = (flags & 0x00000100) !== 0;

		// The showsContentOnlyInTooltip flag is set when either of the following conditions is true:
		// - Condition 1:
		//		the tour uses popups AND
		//		the view has no content AND
		//		the option is set to show a tooltip when a view has no content
		//		the popup is not fixed always visible
		// - Condition 2:
		//		the option for Show Content When is set to Never
		this._showsContentOnlyInTooltip = (flags & 0x00000400) !== 0;

		this.appearsSelected = false;
		this.tooltip = tooltip;
		this.clickAction = clickAction;
		this.clickActionTarget = clickActionTarget;
		this.mouseoverAction = mouseoverAction;
		this.mouseoverActionTarget = mouseoverActionTarget;
		this.mouseoutAction = mouseoutAction;
		this.mouseoutActionTarget = mouseoutActionTarget;
		this.touchPerformsClickAction = touchPerformsClickAction;
		this.showContentEvent = showContentEvent;
		this.routeDefined = false;
		this.blinkAlpha = 1.0;
		this.blinkCount = 0;
		this.blinkLimit = 0;
		this.blinkDirection = 0;
		this.visited = false;

		// An anchored marker's percentages are the relative distances of the marker's center from the left
		// and top edges of the unscaled map. pctX is the percentage of the map width and pctY is the percentage
		// of the map height. Unless the map is square, the percentage for an x value will be different than
		// than the percentage for the same y value distance. For example, if the map is 800 x 1000 pixels,
		// the pctX and pctY values for a marker centered at 100, 100 will be 0.125 and 0.100.
		this.pctX = pctX;
		this.pctY = pctY;

		if (this.isAnchored)
		{
			// Convert the percentages of an anchored marker so that its x,y coordinates are its center in
			// pixels on the unscaled map. 
			this.x = Math.round(this.map.mapW_actual * this.pctX);
			this.y = Math.round(this.map.mapH_actual * this.pctY);
		}
		else
		{
			// The x, y coordinates of a non-anchored marker are set dynamically each time the marker is drawn
			// so that its position is relative to the canvas, not the map. For now, set them to zero.
			this.x = 0;
			this.y = 0;
		}

		this.rotationDegrees = rotationDegrees;
		this.rotationRadians = rotationDegrees * Math.PI / 180;
		this.shapeCoords = shapeCoords;
		this.isPseudoMarker = false;

		this.blinkIntervalId = 0;

		this.bounds_ = null;

		this.handles = new Handles(this);
		this.isBeingEdited = false;

		this.svgImg = null;

		// Constants.
		this.MARKER_TYPE_SYMBOL = 1;
		this.MARKER_TYPE_SHAPE = 2;
		this.MARKER_TYPE_SYMBOL_AND_SHAPE = 3;
		this.MARKER_TYPE_TEXT = 4;
		this.MARKER_TYPE_PHOTO = 5;
		this.SHAPE_TYPE_NONE = 0;
		this.SHAPE_TYPE_CIRCLE = 1;
		this.SHAPE_TYPE_RECTANGLE = 2;
		this.SHAPE_TYPE_POLYGON = 3;
		this.SHAPE_TYPE_LINE = 4;
		this.SHAPE_TYPE_HYBRID = 5;

		// Bind event handlers so that when they are called, 'this' will be set to this MapsAliveMarker object.
		this.blinkMarker = this.blinkMarker.bind(this);

		this.createSymbolMarkerImages(markerType, normalSymbolId, selectedSymbolId, viewId);
	}

	addLineMarkerCoordsToRoute(marker, lastPoint, markers, coords)
	{
		// Get the current marker's start and end points.
		let markerPoints = this.getLineMarkerStartAndEndPoints(marker);
        let markerStartPoint = markerPoints[0];
        let markerEndPoint = markerPoints[1];

		// Handle the case where the current marker is the first line marker in the route. Since there is no last
		// marker, and therefore no last point in the route, look ahead to determine whether the current marker's
		// start point or its end point is furthest from the next marker's start or end point. Whichever one of the
		// current marker's points is further from the next marker will be used to initialize lastPoint.
		if (lastPoint === null)
		{
	 		// Get the next marker's start and end points.
			let nextMarker = markers[1];
            let nextMarkerPoints = this.getLineMarkerStartAndEndPoints(nextMarker);
            let nextMarkerStartPoint = nextMarkerPoints[0];
            let nextMarkerEndPoint = nextMarkerPoints[1];

			// Get the distances between the current and next marker points to determine which two points are closest.
			let distance1 = this.getDistanceBetweenPoints(markerStartPoint, nextMarkerStartPoint);
            let distance2 = this.getDistanceBetweenPoints(markerStartPoint, nextMarkerEndPoint);
            let distance3 = this.getDistanceBetweenPoints(markerEndPoint, nextMarkerStartPoint);
            let distance4 = this.getDistanceBetweenPoints(markerEndPoint, nextMarkerEndPoint);
            let minDistance = Math.min(distance1, distance2, distance3, distance4);

			// Set lastPoint to be the opposite end of the line's closest point. This will cause the current
			// marker to get drawn starting at lastPoint in the direction that gets closest to the next marker.
			if (distance1 === minDistance || distance2 == minDistance)
                lastPoint = markerEndPoint;
            else
                lastPoint = markerStartPoint;
        }

        // Determine whether the order of the current marker's coordinates needs to be reversed.
        let distanceFromLastPointToLineStart = this.getDistanceBetweenPoints(lastPoint, markerStartPoint);
		let distanceFromLastPointToLineEnd = this.getDistanceBetweenPoints(lastPoint, markerEndPoint);
		let reverseCoords = distanceFromLastPointToLineEnd < distanceFromLastPointToLineStart;

		// When the current marker's points need to be drawn in the opposite direction, reverse the marker's coords.
		let lineCoords = marker.shapeCoordsToShapeCoordsArray(marker.shapeCoords);
		if (reverseCoords)
		{
            let temp = [];
            for (let i = lineCoords.length - 2; i >= 0; i -= 2) {
                temp.push(lineCoords[i]);
                temp.push(lineCoords[i + 1]);
            }
            lineCoords = temp;
        }

		// Determine how much to adjust each coord to position the marker to the correct location on the screen.
		let bounds = marker.getBounds();
		let lineWidth = marker.markerStyle.lineWidth;
        let cornerX = marker.x - bounds.halfW_actual + 1 + (lineWidth / 2);
		let cornerY = marker.y - bounds.halfH_actual + (lineWidth / 2);

		// Add the current marker's coords to the coords for the route.
		for (let i = 0; i < lineCoords.length; i += 2)
		{
            let x = lineCoords[i];
            let y = lineCoords[i + 1];
            coords.push(cornerX + x);
            coords.push(cornerY + y);
        }
    }

	blinkMarker(viewId)
	{
		// This pass-through method is called on an interval timer.
		this.map.blinkMarker(viewId);
	}

	boundsContainPoint(pointer)
	{
		// Determine if the pointer is within the hit rectangle.
		let b = this.getBounds();
		let inBounds =
			pointer.x >= b.hitX1_screen &&
			pointer.x <  b.hitX2_screen &&
			pointer.y >= b.hitY1_screen &&
			pointer.y < b.hitY2_screen;
		return inBounds;
	}

	boundsIntersectsWithMarker(otherMarker)
	{
		//console.log(`Marker::boundsIntersectsWithMarker ${this.viewId} : ${otherMarker.viewId}`);

		let b1 = this.getBounds();
		let b2 = otherMarker.getBounds();

		if (b1.hitX1_screen + b1.hitW_screen < b2.hitX1_screen)
		{
			// b1 is to the of left of b2.
			return false;
		}
		else if (b1.hitX1_screen > b2.hitX1_screen + b2.hitW_screen)
		{
			// b1 is to the right of b2.
			return false;
		}
		else if (b1.hitY1_screen + b1.hitH_screen < b2.hitY1_screen)
		{
			// b1 is above b2.
			return false;
		}
		else if (b1.hitY1_screen > b2.hitY1_screen + b2.hitH_screen)
		{
			// b1 is below b2.
			return false;
		}

		return true;
	}

	calculateBounds()
	{
		// This method calculates both the dimensions and positions of this marker. It creates
		// an object having a variety of properties that provide all other code with bounds
		// information without having to do any other calculations. The bounds object is
		// cached by virtue of being attached to the marker for as long as the information is
		// valid. It becomes invalid if the marker's position one the map changes and when the
		// map is panned or zoomed. In those cases, the bounds object is removed and a new one
		// is created when needed.

		let b = new Object();

		this.calculateBoundsDimensions(b);
		this.calculateBoundsPosition(b);

		// Determine if the marker is visible.
		b.isVisibleOnCanvas = this.calculateIsVisibleOnCanvas(b, false);
		b.isEntirelyVisibleOnCanvas = this.calculateIsVisibleOnCanvas(b, true);
		b.isVisibleAtCurrentZoomLevel = this.calculateIsVisibleAtCurrentZoomLevel();

		//console.log(`MapsAliveMap::calculateBounds ${this.viewId}`);

		this.bounds_ = b;
	}

	calculateBoundsDimensions(b)
	{
		// Increase the size of the marker's shape to accommodate line thickness and effects.
		this.extra = this.calculateBoundsExtra();
		b.w_actual = this.shapeW + this.extra.w;
		b.h_actual = this.shapeH + this.extra.h;

		// Calculate half the width and height.
		b.halfW_actual = Math.round(b.w_actual / 2);
		b.halfH_actual = Math.round(b.h_actual / 2);

		// Determine the dimensions of the rectangle that will be used for hit testing when
		// checking to see if the pointer is over a marker.
		if (this.rotationRadians !== 0)
		{
			// Use trignometry to calculate the dimensions of a rectangle that will fully
			// encompass the rotated marker. The enclosing rectangle is not rotated.
			let theta = this.rotationRadians;
			let w = b.w_actual;
			let h = b.h_actual;
			b.hitW_screen = Math.round(w * Math.abs(Math.cos(theta)) + h * Math.abs(Math.sin(theta)));
			b.hitH_screen = Math.round(w * Math.abs(Math.sin(theta)) + h * Math.abs(Math.cos(theta)));
		}
		else
		{
			// Use the unrotated marker dimensions for the size of the hit rectangle.
			b.hitW_screen = b.w_actual;
			b.hitH_screen = b.h_actual;
		}

		// Set the marker's dimensions based on whether the marker zooms or is bound.
		if (this.markerZooms || this.isBound)
		{
			// Scale the marker's screen dimensions to the appropriate map scale.
			b.w_screen = this.map.convertMapToScreen(b.w_actual);
			b.h_screen = this.map.convertMapToScreen(b.h_actual);

			// Scale the hit test area to the map's scale.
			b.hitW_screen = this.map.convertMapToScreen(b.hitW_screen);
			b.hitH_screen = this.map.convertMapToScreen(b.hitH_screen);
		}
		else
		{
			// Use the marker's actual dimensions as the screen dimensions.
			b.w_screen = b.w_actual;
			b.h_screen = b.h_actual;
		}

		// Increase the hit bounds of small markers to make them easier to touch.
		if (this.map.tour.enlargeHitTestArea)
		{
			if (b.hitW_screen < this.map.minHitTargetSize)
				b.hitW_screen += this.map.minHitTargetSize - b.hitW_screen;
			if (b.hitH_screen < this.map.minHitTargetSize)
				b.hitH_screen += this.map.minHitTargetSize - b.hitH_screen;
		}

		// The marker's map and screen dimensions are always the same. When the marker zooms, the
		// dimensions are scaled to the map. When it doesn't zoom, they are its actual dimensions.
		// Both the _map and _screen properties are provided, even though the same, for clarity
		// so that code can use one or the other when working with other _map or _screen values.
		b.w_map = b.w_screen;
		b.h_map = b.h_screen;

		//console.log(`Marker::calculateBoundsDimensions ${this.viewId} ${this.extra.w}x${this.extra.h} ${b.hitW_screen}x${b.hitH_screen}`);
	}

	calculateBoundsExtra()
	{
		// This method derives how much extra size must be added to a marker's bounds to
		// account for shape, line thickness, and effects that spill beyond the marker's edges.
		// A marker's bounds are only allowed to grow and never shrink. This simplifies the logic
		// that redraws markers when their appearance changes. A redraw first erases a marker and
		// then redraws its new appearance. If the bounds of the new appearance were smaller than
		// the old appearance, the edges of the old appearance would not get erased leaving crud
		// around the marker. Rather than adding to the already complex drawing logic to handle
		// this case, and so, a marker's bounds, once enlarged, are never allowed to get smaller.
		// This should have minimal negative impact because typically, API calls toggle a marker's
		// appearance and so maintaing the largest bounds is probably no worse than making the
		// drawing logic handle bounds that gets smaller.

		// Initially assume there is no extra.
		let extra = { w: 0, h: 0 };

		// Don't include extra for a hybrid marker because the calculations won't work right since a
		// hybrid is a combination of other shapes. Also, increasing the size of a hybrid marker
		// breaks the logic for hybrid marker editing which must deconstruct a hybrid into pseudo
		// markers, allow the user to edit the pseudo markers, and then recombine the pseudo markers
		// back into the same hybrid marker. A side effect of not adding extra to hybrids could cause
		// crud to appear as described above and so users should avoid using hybrids where that happens.
		if (this.shapeIsHybrid)
			return extra;

		// Don't include extra for a pseudo marker or a marker being edited because the extra
		// would cause the marker's edit handles to be drawn outside of the actual shape bounds. 
		if (this.isPseudoMarker || this.isBeingEdited)
			return extra;

        // Get the size of the marker's style effects like drop shadow and/or glow. These are calculated
		// by the MapsAliveMarkerStyleEffects::translateDefinition method and recorded as part of the
		// marker style properties. Use the largest width or height of the normal and selected appearances.
        let normalExtraSize = this.markerStyle.normalProperties.effects.extraSize;
        let selectedExtraSize = this.markerStyle.selectedProperties.effects.extraSize;
        extra.w += Math.max(normalExtraSize.w, selectedExtraSize.w);
        extra.h += Math.max(normalExtraSize.h, selectedExtraSize.h);

        // Account for the fact that some markers have very thick lines which make them larger.
        // Lines are drawn on the center of the edges, so only add half the thickness.
        let lineWidth = this.lineWidth();
		if (lineWidth >= 2) 
		{
            extra.w += lineWidth;
            extra.h += lineWidth;
		}

		// Add a little extra to circles because without it the bounds are a bit too small which
		// results in the top, right, bottom, and left sides not geting fully erase when redrawn.
		if (this.shapeIsCircle)
		{
			extra.w += 4;
			extra.h += 4;
		}

		// Add a fudge factor for polygon, line, and hybrid shapes.
		let extraExtra = this.calculateBoundsExtraForIrregularShapes();
		extra.w += extraExtra;
		extra.h += extraExtra;

		return { w: Math.max(extra.w, this.extra.w), h: Math.max(extra.h, this.extra.h)};
    }

	calculateBoundsExtraForIrregularShapes()
	{
		// This method applies to irregular shaped zoomable markers that use blur and need still
		// larger bounds than regular shaped markers like rectangles and circles. Jutting points,
		// like the southern tip of Texas, that protrude from an irregular shape tend to spill a
		// little more. Rather than use a constant value for this fudge factor which would cause
		// the relative value to have a lesser effect when the map is scalled down than when full
		// size, it takes the map scale into consideration.

		// Always add at least 2 pixels of extra bounds for all shapes. This is enough to deal
		// with bounds being ever so slightly too small because the math to calculate them
		// doesn't always exactly match what the canvas drawing logic does.
		let extra = 2;

		if (!this.markerZooms)
			return extra;

		let usesBlur = this.markerStyle.normalProperties.effects.hasBlur || this.markerStyle.selectedProperties.effects.hasBlur;
		if (!usesBlur)
			return extra;

		if (!this.shapeHasPoints)
			return extra;

		// This calculation is based on trial and error. The numbers here are arbitrary and can be
		// adjusted up or down if new cases reveal that they are not good enough. By using the
		// inverse of the map scale and subtracting 1, the base value is zero when the map's scale
		// is 1.0 and gets larger the more the map is scaled down, e.g. the base is 3 at 0.25 scale.
		// The larger base adjusts for the fact that when the map is scaled down, the bounding
		// box of zoomable markers gets scaled down as well and so this formula causes the box to
		// scale a little less to better allow for the blur, which is also scaled down, but doesn't
		// seem to do so in a linear fashion which is why this code exists.
		let base = (1 / this.map.zoomedOutMapScale) - 1;
		extra = base * 25;
		if (extra > 200)
			extra = 200;
		else if (extra < 20)
			extra = 20;

		return extra;
	}

	calculateBoundsPosition(b)
	{
		// Get the anchor point of the marker on the unscaled map. It is usually the center of the marker, but
		// if the marker is anchored off-center, the point is shifted by this.centerOffsetX and this.centerOffsetY
		// which are offsets from the center of the marker. When the marker is centered, both offsets are zero.
		b.anchorX_map = this.x;
		b.anchorY_map = this.y;

		if (this.isAnchored)
		{
			// Adjust the coordinates for panning;
			b.anchorX_map += this.map.panX_map;
			b.anchorY_map += this.map.panY_map;
		}

		// Get the center point of the marker on the unscaled map.
		if (this.markerZooms)
		{
			b.centerX_map = b.anchorX_map - this.centerOffsetX;
			b.centerY_map = b.anchorY_map - this.centerOffsetY;
		}
		else
		{
			// Scale the offsets to match the map's scale.
			b.centerX_map = b.anchorX_map - this.map.convertScreenToMap(this.centerOffsetX);
			b.centerY_map = b.anchorY_map - this.map.convertScreenToMap(this.centerOffsetY);
		}

		// Get the upper left corner of the marker.
		if (this.markerZooms)
		{
			b.cornerX_map = b.centerX_map - b.halfW_actual;
			b.cornerY_map = b.centerY_map - b.halfH_actual;
		}
		else
		{
			// Scale the half width to match the map's scale.
			b.cornerX_map = b.centerX_map - this.map.convertScreenToMap(b.halfW_actual);
			b.cornerY_map = b.centerY_map - this.map.convertScreenToMap(b.halfH_actual);
		}

		// Get the screen coordinates.
		if (this.isAnchored)
		{
			// Get the anchor point.
			b.anchorX_screen = this.map.convertMapToScreen(b.anchorX_map);
			b.anchorY_screen = this.map.convertMapToScreen(b.anchorY_map);

			// Get the center point of the marker which is the same as the anchor if the anchor is not off-center.
			b.centerX_screen = this.map.convertMapToScreen(b.centerX_map);
			b.centerY_screen = this.map.convertMapToScreen(b.centerY_map);

			// Get the upper left corner of the marker.
			b.cornerX_screen = this.map.convertMapToScreen(b.cornerX_map);
			b.cornerY_screen = this.map.convertMapToScreen(b.cornerY_map);
		}
		else
		{
			b.anchorX_screen = b.anchorX_map;
			b.anchorY_screen = b.anchorY_map;
			b.centerX_screen = b.centerX_map;
			b.centerY_screen = b.centerY_map;

			// Since the map coords for a non anchored marker are actually its screen coords,
			// the calculation for the upper left corner is different than for an anchored marker.
			if (this.markerZooms)
			{
				b.cornerX_screen = b.centerX_screen - this.map.convertMapToScreen(b.halfW_actual);
				b.cornerY_screen = b.centerY_screen - this.map.convertMapToScreen(b.halfH_actual);
			}
			else
			{
				b.cornerX_screen = b.centerX_screen - b.halfW_actual;
				b.cornerY_screen = b.centerY_screen - b.halfH_actual;
			}
		}

		// Get the bounds of the hit area rectangle which will be larger than the bounds for a rotated marker.
		this.calculateBoundsPositionHit(b);
	}

	calculateBoundsPositionHit(b)
	{
		// This method calculates the position of a marker's hit bounds taking into consideration the marker's
		// anchor point and its rotation. The marker and hit bounds are the same when a marker is not rotated.
		// The center point of the marker bounds and the center point of the hit bounds are the same, whether
		// or not the marker is rotated if the marker's anchor point is the same as its center point. Those
		// are the easy cases. The tricky situtation is when the anchor point is off-center and the marker is
		// rotated. As a marker with an off-center anchor rotates, the dimensions of the hit bounds grow or
		// shrink as necessary to contain the rotated marker. As the marker rotates around its own anchor point,
		// the center of the hit bounds rotate around the marker staying centered on the marker's center point.
		// The result is that the hit area both encompasses the rotated marker and is centered over the rotated
		// marker making it possible to do hit detection at any rotation. The logic that follows handles all
		// cases, but the comments describe the case when the marker is both anchored off-center and rotated.

		// The centerOffsetX and centerOffsetY values are the distances from the marker's center point to its
		// anchor point. Invert the values to get the center point of the circle that the marker's center point
		// rotates around. In other words, treat the anchor point as the center of the Unit Circle used by trig
		// funtions that calculate angles, distances, and positions.
		let offsetX = this.centerOffsetX * -1;
		let offsetY = this.centerOffsetY * -1;

		// The offset values are also the lengths of the opposite and adjacent sides of a right triangle located
		// between the marker's anchor point and center point. The opposite side is on the X axis. The adjacent
		// side is on the Y axis. Scale the lengths of the sides to the map's scale. If the marker is bound,
		// use the map area scale since the marker's scale is bound to the original map's scale.
		if (this.markerZooms)
		{
			offsetX = this.map.convertMapToScreen(offsetX, this.isBound);
			offsetY = this.map.convertMapToScreen(offsetY, this.isBound);
		}

		// Use the Pythagorean theorem to compute the length of the hypotenuse of the triangle which is the the
		// distance from the marker's anchor point to its center point.
		this.centerOffsetDistance = Math.hypot(offsetX, offsetY);

		// Get the angle in the plane between the positive X axis of the Unit Circle and the ray from the anchor
		// point to the center point. See the https://vectorjs.org/examples/unit-circle/ for a demonstration and
		// code for working with the Unit Circle. Use of atan2 here comes from the getAngle function in that code.
		let angle = Math.atan2(offsetY, offsetX);

		// Adjust the angle to be the proper value in the Unit Circle so that the ray points from the anchor point
		// to the center point. This is necessary to deal with the fact that trig math must consider which of the
		// four quadrants of the Unit Circle a value belongs in depending on whether Y is positve or negative.
		angle = offsetY <= 0 ? Math.abs(angle) : Math.PI * 2 - angle;

		// Subtract the rotation of the marker so that the angle itself is not further rotated.
		angle -= this.rotationRadians;

		// Get the end point of the vector from the anchor point to the center point at the angle. This end point
		// is what the hit bounds must be centered on in order to both contain the rotated marker and be centered
		// over the marker's center point.
		b.hitCenterX = Math.round(b.anchorX_screen + this.centerOffsetDistance * Math.cos(angle));
		b.hitCenterY = Math.round(b.anchorY_screen + this.centerOffsetDistance * -(Math.sin(angle)));

		// Use the hit bounds center point to get its upper left and lower right corners. These will be used
		// by the boundsContainPoint method to determine if the pointer if over the hit bounds.
		b.hitX1_screen = b.hitCenterX - Math.round(b.hitW_screen / 2);
		b.hitY1_screen = b.hitCenterY - Math.round(b.hitH_screen / 2);
		b.hitX2_screen = b.hitX1_screen + b.hitW_screen;
		b.hitY2_screen = b.hitY1_screen + b.hitH_screen;
	}

	calculateIsVisibleAtCurrentZoomLevel()
	{
		if (this.map.tour.editMode)
			return true;

		if (this.isHidden)
			return false;

		// Determine if the map is zoomed in or out enough to see this marker.
		let isVisible = true;
		let zoomLevelPercent = Math.round(this.map.currentMapScale * 100);

		if (this.zoomThreshold < 0)
		{
			// A negative threshold means the marker is visble at or below that zoom percentage.
			isVisible = zoomLevelPercent <= -this.zoomThreshold;
		}
		else if (this.zoomThreshold > 0)
		{
			// A positive threshold means the marker is visble above that zoom percentage.
			isVisible = zoomLevelPercent > this.zoomThreshold;
		}

		return isVisible;
	};

	calculateIsVisibleOnCanvas(bounds, entireMarkerMustBeVisible)
	{
		// Non anchored markers are always positioned to be visible on the canvas.
		if (!this.isAnchored)
			return true;

		let x1 = bounds.cornerX_screen;
		let y1 = bounds.cornerY_screen;
		let x2 = x1 + bounds.w_screen;
		let y2 = y1 + bounds.h_screen;

		let visible;
		if (entireMarkerMustBeVisible || this.isRoute)
		{
			// Determine the entire marker is visible on the canvas.
			visible = x1 > 0 && x2 <= this.map.canvasW && y1 > 0 && y2 <= this.map.canvasH;
		}
		else
		{
			// Determine if at least part of the marker is visible on the canvas
			visible = x2 > 0 && x1 <= this.map.canvasW && y2 > 0 && y1 <= this.map.canvasH;
		}

		return visible;
	}

	changeMarkerShapeAppearance(selected, properties)
	{
		//console.log(`Marker::changeMarkerShapeAppearance ${selected} ${this.viewId}`);

		if (this.privateMarkerStyle === null)
		{
			// This marker has no private marker style. Create one using the passed-in properties.
			let normalProperties = selected ? null : properties;
			let selectedProperties = selected ? properties : null;

			this.privateMarkerStyle = new MapsAliveMarkerStyle(
				this.globalMarkerStyle.id,
				this.globalMarkerStyle.lineWidth,
				normalProperties,
				selectedProperties,
				this.globalMarkerStyle);
		}
		else
		{
			// Update this marker's private marker style with the passed-in properties.
			this.privateMarkerStyle.setBaseProperties(selected, properties);
		}

		// Force the marker bounds to be recalculated since the new properties could cause them to grow.
		this.updateBounds();
	}

	changeMarkerStyleAppearance(selected, properties)
	{
		console.log(`MapsAliveMarker::changeMarkerStyleAppearance ${selected} ${this.viewId}`);

		this.markerStyle.setChangedProperties(selected, properties);

		// Force the marker bounds to be recalculated since the new properties could cause them to grow.
		this.updateBounds();
	}

	createSymbolMarkerImages(markerType, normalSymbolId, selectedSymbolId, viewId)
	{
		let me = this;
		let symbolType;
		let symbolIdN = 0;
		let symbolIdS = 0;

		if (markerType === this.MARKER_TYPE_SYMBOL || markerType === this.MARKER_TYPE_SYMBOL_AND_SHAPE)
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
		else if (markerType === this.MARKER_TYPE_TEXT || markerType === this.MARKER_TYPE_PHOTO)
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
			this.imgN.onload = function () { me.imgLoadedN = true; };
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
			this.imgS.onload = function () { me.imgLoadedS = true; };
			this.imgS.src = this.getSymbolDataUri(symbolIdS, symbolType, "S");
		}

		else
		{
			this.hasImageS = false;
		}
	}

	defineRoute(markerIdList)
	{
		this.markerIdList = markerIdList;

		if (this.markerIdList.length === 0)
		{
			this.routeDefined = false;
			return;
		}

		// Make sure the list is a string. If a single numeric element was passed, convert it to a string.
		this.markerIdList = this.markerIdList + "";
		let sections = this.markerIdList.split(";");
		let coords = new Array();

		for (let i in sections)
		{
			// Prepend a section start indicator to each section after the first one.
			coords.push(-1);
			coords.push(4);

			// Create an array of marker Ids for each semicolon-separated section.
			let markers = new Array();
			if (this.markerIdList.length > 0)
			{
				let list = sections[i].split(",");
				for (let j in list)
				{
					// Get the marker for this Id. Ignore route markers since we don't draw through them.
					let viewId = parseInt(list[j], 10);
					let m = this.map.getMarker(viewId);
					if (m !== null && !m.isRoute)
					{
						markers.push(m);
					}
				}
			}

			// Create an x,y pair from each marker in the section.
			let lastPoint = null;
			let lastIndex = 0;
			for (let k in markers)
			{
				let marker = markers[k];
				if (marker.shapeType == marker.SHAPE_TYPE_LINE)
				{
					if (marker.rotationDegrees !== 0)
					{
						/**/console.warn(`Cannot draw rotated line marker ${marker.markerId} in a route`);
						continue;
					}
					this.addLineMarkerCoordsToRoute(marker, lastPoint, markers, coords);
				}
				else
				{
					coords.push(marker.x);
					coords.push(marker.y);
				}

				lastIndex = coords.length - 1; 
				lastPoint = [coords[lastIndex - 1], coords[lastIndex]];

			}
		}

		// Determine the bounding box and center point for this route.
		let minX = Number.MAX_VALUE;
		let maxX = Number.MIN_VALUE;
		let minY = Number.MAX_VALUE;
		let maxY = Number.MIN_VALUE;

		for (let i = 0; i < coords.length; i += 2)
		{
			if (coords[i] === -1)
				continue;
			let x = coords[i];
			let y = coords[i + 1];
			minX = Math.min(x, minX);
			minY = Math.min(y, minY);
			maxX = Math.max(x, maxX);
			maxY = Math.max(y, maxY);
		}

		this.shapeW = maxX - minX + 1;
		this.shapeH = maxY - minY + 1;
		this.x = minX + (this.shapeW / 2);
		this.y = minY + (this.shapeH / 2);

		// Shift the coordinates so that the upper left corner of the route is at 0,0.
		for (let index = 0; index < coords.length; index += 2)
		{
			if (coords[index] === -1)
				continue;
			coords[index] -= (minX + 0.5);
			coords[index + 1] -= (minY + 0.5);
		}

		// Convert the coords array to a comma separated list.
		let points = coords.join(",");
		this.shapeCoords = points;

		this.routeDefined = true;
	}

	getBounds()
	{
		if (this.bounds_ === null)
			this.calculateBounds();

		//console.log(`MapsAliveMarker::getBounds ${this.markerId} ${this.shapeType} ${this.bounds_.centerX_map},${this.bounds_.centerY_map} ${this.bounds_.w_actual}x${this.bounds_.h_actual}`);

		// Return a copy of the bounds so that the caller cannot modify the original.
		return Object.assign({}, this.bounds_);
	}

	getDistanceBetweenPoints(point1, point2)
	{
		// The distance between two points is the hypotenuse of the right triangle they form.
		let base = Math.abs(point1[0] - point2[0]);
		let height = Math.abs(point1[1] - point2[1]);
		let distance = Math.hypot(base, height);
		return distance;
	}

	getLineMarkerStartAndEndPoints(marker)
	{
		let bounds = marker.getBounds();
		let cornerX = marker.x - bounds.halfW_actual;
		let cornerY = marker.y - bounds.halfH_actual;
		let lineCoords = marker.shapeCoordsToShapeCoordsArray(marker.shapeCoords);

		// Get the line's start and end coords.
		let lastIndex = lineCoords.length - 1;
		let startCoord = [lineCoords[0], lineCoords[1]];
		let endCoord = [lineCoords[lastIndex - 1], lineCoords[lastIndex]];

		// Convert the start and end coords into the start and end points.
		let startPoint = [cornerX + startCoord[0], cornerY + startCoord[1]];
		let endPoint = [cornerX + endCoord[0], cornerY + endCoord[1]];

		return [startPoint, endPoint];
	}

	getSymbolDataUri(symbolId, symbolType, symbolState)
	{
		// Get the symbol image data objects that were imported from the symbols js file.
		for (const symbol of this.map.symbols)
		{
			if (symbol.id === symbolType + symbolId + symbolState)
			{
				return "data:image/png;base64," + symbol.data;
			}
		}
		return null;
	};

	get halfShapeW()
	{
		return Math.round(this.shapeW / 2);
	};

	get halfShapeH()
	{
		return Math.round(this.shapeH / 2);
	};

	get hasPrivateMarkerStyle()
	{
		return this.privateMarkerStyle !== null;
	}

	get isBlinking()
	{
		return this.blinkIntervalId !== 0;
	}

	get isEntirelyVisibleOnCanvas()
	{
		return this.getBounds().isEntirelyVisibleOnCanvas;
	}

	get isNotOnMap()
	{
		return this.x < 0 && this.y < 0;
	}

	get isShapeMarker()
	{
		return this.markerType !== this.MARKER_TYPE_SYMBOL;
	}

	get isVisibleAtCurrentZoomLevel()
	{
		return this.getBounds().isVisibleAtCurrentZoomLevel;
	}

	get isVisibleOnCanvas()
	{
		return this.getBounds().isVisibleOnCanvas;
	}

	lineWidth()
	{
		return this.markerStyle === null ? 0 : this.markerStyle.lineWidth;
	};

	get markerStyle()
	{
		return this.privateMarkerStyle === null ? this.globalMarkerStyle : this.privateMarkerStyle;
	}

	setBlink(howManyTimes)
	{
		// Be very careful when changing this code because blinking is an expensive operation.
		// Unecessary blinking can make the map very sluggish when mousing over markers.

		// Ignore this call when the map does not blink its markers.
		if (this.map.blinkCount === 0)
			return;

		// Ignore this call when the call is to stop blinking a marker that is not blinking.
		if (!this.isBlinking && howManyTimes === 0)
			return;

		//console.log(`Marker setBlink ${howManyTimes}`);

		// Clear the interval because this method was called while the marker was blinking.
		// Since setInterval returns a new Id each time it's called, we need to make sure the
		// old interval is clear before its Id is replaced with the new Id.
		clearInterval(this.blinkIntervalId);

		if (howManyTimes > 0)
		{
			this.blinkIntervalId = setInterval(this.blinkMarker, 75, this.viewId);
			this.blinkDirection = -1;
			this.blinkLimit = howManyTimes;
		}
		else
		{
			this.setBlinkAlpha(1.0);
			this.blinkIntervalId = 0;

			// When blinking stops, the marker will have been last drawn above neighboring markers.
			// If it's not the selected marker, draw it in it's normal stacking position.
			let selected = this.viewId == this.map.selectedMarkerViewId;
			if (!selected)
				this.map.drawMarkerAndNeighbors(this, false);
		}

		this.blinkCount = 0;
	};

	setBlinkAlpha(alpha)
	{
		this.blinkAlpha = alpha;

		// When blinking a symbol, it's necessary to redraw the marker and intersecting markers because
		// there is no other way to safely erase and redraw a symbol marker without erasing its bounding
		// box which could erase parts of adjacent markers. Thus, blinking a symbol can cause many or
		// all markers to get redrawn. A shape marker's shape can be erased without erasing its entire
		// bounding box and so its neighbors don't have to be redrawn.
		if (this.typeIsSymbol)
			this.map.drawMarkerAndNeighbors(this, true);
		else
			this.map.drawMarkerOnly(this, true);
	};

	get shapeCoordsArray()
	{
		return this.shapeCoordsToShapeCoordsArray(this.shapeCoords);
	}

	set shapeCoordsArray(coords)
	{
		// Verify that every coord is an integer (not a string, and not a fraction).
		for (const value of coords)
			Runtime__$$.assert(parseInt(Number(value)) === value, `Coord ${value} in marker ${this.markerId} is not an integer`);

		this.shapeCoords = coords.join(",");
	}

	shapeCoordsToShapeCoordsArray(shapeCoords)
	{
		let coords = shapeCoords.split(",");
		for (const i in coords)
			coords[i] = parseInt(coords[i], 10);
		return coords;
	}

	get shapeHasPoints()
	{
		return this.shapeIsPolygon || this.shapeIsLine || this.shapeIsHybrid;
	}

	get shapeIsCircle()
	{
		return this.shapeType === this.SHAPE_TYPE_CIRCLE;
	}

	get shapeIsHybrid()
	{
		return this.shapeType === this.SHAPE_TYPE_HYBRID;
	}

	get shapeIsLine()
	{
		return this.shapeType === this.SHAPE_TYPE_LINE;
	}

	get shapeIsNone()
	{
		return this.shapeType === this.SHAPE_TYPE_NONE;
	}

	get shapeIsPolygon()
	{
		return this.shapeType === this.SHAPE_TYPE_POLYGON;
	}

	get shapeIsRectangle()
	{
		return this.shapeType === this.SHAPE_TYPE_RECTANGLE;
	}

	////// SVG
	get shapeIsSvg()
	{
		return this.shapeType === this.SHAPE_TYPE_POLYGON && this.shapeCoords.startsWith('<svg');
	}

	get showsContentOnlyInTooltip()
	{
		// This property is used to override the Tour Builder's setting for the marker's DoesNotShowContent flag.
		// The Tour Builder set the flag to true if the the marker's hotspot had no content when the tour was built;
		// However, if the tour's JavaScript calls the api.setHotspotText method to provide content dynamically, the
		// flag's setting needs to be reevaluated.
		const never = 2;
		if (this._showsContentOnlyInTooltip && this.showContentEvent !== never)
		{
			let view = this.map.page.getView(this.viewId);
			if (view.htmlText.length > 0)
				return false;
		}
		return this._showsContentOnlyInTooltip;
	}

	set showsContentOnlyInTooltip(shows)
	{
		this._showsContentOnlyInTooltip = shows;
	}

	get typeIsPhoto()
	{
		return this.markerType === this.MARKER_TYPE_PHOTO;
	}

	get typeIsShape()
	{
		return this.markerType === this.MARKER_TYPE_SHAPE;
	}

	get typeIsSymbol()
	{
		return this.markerType === this.MARKER_TYPE_SYMBOL;
	}

	get typeIsSymbolAndShape()
	{
		return this.markerType === this.MARKER_TYPE_SYMBOL_AND_SHAPE;
	}

	get typeIsText()
	{
		return this.markerType === this.MARKER_TYPE_TEXT;
	}

	updateBounds()
	{
		// This method is called when the bounds have changed because the user panned or
		// zoomed the map, or they moved the marker in the Map Editor. Rather than update
		// the bounds, simply delete the old bounds which will force them to get recalculated
		// the next time their getBounds method is called. This optimization prevents recalculation
		// over and over of the bounds for off-canvas markers while the map is zooming or panning.
		//console.log(`MapsAliveMarker::updateBounds`);
		this.bounds_ = null;
	}
}

class MapsAliveMarkerStyle
{
	constructor(id, lineWidth, normalProperties, selectedProperties, globalMarkerStyle = null)
	{
		this.id = id;
		this.lineWidth = lineWidth;
		this.baseProperties = { normal: normalProperties, selected: selectedProperties };
		this.changedProperties = { normal: null, selected: null };
		this.globalMarkerStyle = globalMarkerStyle;
	}

	get isPrivateMarkerStyle()
	{
		return this.globalMarkerStyle !== null;
	}

	getProperties(selected)
	{
		if (selected)
		{
			if (this.isPrivateMarkerStyle)
			{
				// Return this private marker style's base selected properties if it has them. Otherwise,
				// call this method recursively to return the global marker style's selected properties.
				if (this.baseProperties !== null && this.baseProperties.selected !== null)
					return this.baseProperties.selected;
				else
					return this.globalMarkerStyle.getProperties(selected);
			}
			else
			{
				// Return this base marker style's changed selected properties if it has them. Otherwise,
				// return its base selected properties.
				if (this.changedProperties.selected !== null)
					return this.changedProperties.selected
				else
					return this.baseProperties.selected;
			}
		}
		else
		{
			if (this.isPrivateMarkerStyle)
			{
				// Return this private marker style's base normal properties if it has them. Otherwise,
				// call this method recursively to return the global marker style's normal properties.
				if (this.baseProperties !== null && this.baseProperties.normal !== null)
					return this.baseProperties.normal;
				else
					return this.globalMarkerStyle.getProperties(selected);
			}
			else
			{
				// Return this base marker style's changed normal properties if it has them. Otherwise,
				// return its base normal properties.
				if (this.changedProperties.normal !== null)
					return this.changedProperties.normal
				else
					return this.baseProperties.normal;
			}
		}
	}

	get normalProperties()
	{
		return this.getProperties(false);
	}

	get selectedProperties()
	{
		return this.getProperties(true);
	}

	setBaseProperties(selected, properties)
	{
		if (selected)
			this.baseProperties.selected = properties;
		else
			this.baseProperties.normal = properties;
	}

	setBasePropertiesNormal(properties)
	{
		this.setBaseProperties(false, properties);
	}

	setBasePropertiesSelected(properties)
	{
		this.setBaseProperties(true, properties);
	}

	setChangedProperties(selected, properties)
	{
		// This method is used to modify the changed properites of a global marker style. The changes
		// affect all markers that use the style. This method cannot be used with a private marker
		// style because a private style's base properties are the changes to the global marker style
		// for just one marker. Thus it's not meaningful to have changes to those changes and so the
		// changed properties for a private style are always set to null.
		Runtime__$$.assert(!this.isPrivateMarkerStyle, 'Cannot set changed properties of private marker style');

		// When properties is passed as null, this call restores the global marker style's base
		// properties by virtue of removing the changed properties. Otherwise, the calls sets the
		// passed properties as the changed properties.
		if (selected)
			this.changedProperties.selected = properties;
		else
			this.changedProperties.normal = properties;
	}
}

class MapsAliveMarkerStyleEffects
{
	constructor(map, markerStyleProperties, definition)
	{
		this.map = map;
		this.definition = definition;
		this.markerStyleProperties = markerStyleProperties;
		this.blendMode = '';
		this.glow = null;
		this.shadow = null;
		this.lineDash = [];
		this.extraSize = { w: 0, h: 0 };

		this.translateDefinition();
	}

	getEffectOptions(values, index)
	{
		// Return the options for a specific effect e.g. for the shadow effect, its distance, angle, and blur.
		let options = new Array();
		while (index < values.length)
		{
			let value = values[index];
			if (value === "-1")
				break;
			options.push(value);
			index++;
		}

		return options;
	}

	getOptionAlpha(options, index, defaultValue)
	{
		let alpha = this.getOptionNumber(options, index, defaultValue);
		if (alpha < 0)
			alpha = 0;
		if (alpha > 100)
			alpha = 100;

		// Increase the shadow to be more like the result we got with Flash.
		alpha = Math.min(100, Math.round(alpha * 1.3));

		alpha /= 100;
		return alpha;
	}

	getOptionBlur(options, index, defaultValue)
	{
		// HTML5 does not support directional blur so we use the average of blur x and blur y.
		let blurX = this.getOptionNumber(options, index, defaultValue);
		let blurY = this.getOptionNumber(options, index + 1, defaultValue);
		let blur = Math.round((blurX + blurY) / 2);

		return blur;
	}

	getOptionColor(options, index, defaultValue, alpha)
	{
		let rawColor = this.getOptionText(options, index, defaultValue);
		let hexColor = this.map.graphics.convertIntegerColorToHex(rawColor);
		return this.map.graphics.createRgbaFromHexColor(hexColor, alpha);
	}

	getOptionNumber(options, index, defaultValue)
	{
		let n = parseInt(this.getOptionText(options, index, defaultValue), 10);
		if (isNaN(n))
			n = defaultValue;
		return n;
	}

	getOptionText(options, index, defaultValue)
	{
		return options.length > index ? options[index] : defaultValue;
	}

	getValidBlendMode(mode)
	{
		mode = mode.toLowerCase();
		if (mode === "darken" ||
			mode === "difference" ||
			mode === "hardlight" ||
			mode === "invert" ||
			mode === "lighten" ||
			mode === "multiply" ||
			mode === "normal" ||
			mode === "overlay" ||
			mode === "screen")
			return mode;
		else
			return "multiply";
	}

	get hasBlur()
	{
		return this.glow !== null || this.shadow !== null;
	}

	setSizeOfEffects(w, h)
	{
		// Record how much extra space is occupied by the marker's shadow and/or glow effects which
		// blur the edges of a marker. w and h are the blur distances, but trial and error testing
		// reveals that they need to be doubled to adequately increase the drawing bounds of a
		// marker to fully enclose the blur.
		w *= 2;
		h *= 2;

		// Since this method can be called multiple times for the same marker (when the marker has
		// multipe effects e.g. glow + shadow) record the largest size of any effect.

		this.extraSize.w = Math.max(w, this.extraSize.w);
		this.extraSize.h = Math.max(h, this.extraSize.h);
	}

	translateDefinition()
	{
		// Translate the effects definition text into the properties of this MapsAliveMarkerStyleEffects object.
		//console.log(`Marker::translateDefinition '${this.definition}'`)

		if (this.definition.length === 0)
			return;

		let definitions = this.definition.split(",");
		let i = 0;

		// Loop over and translate each of the marker's effect definitions.
		while (i < definitions.length)
		{
			if (definitions[i] !== "-1")
			{
				// The user specified an invalid definition.
				return;
			}

			// Get the effect type and handle the case where the user separated parameters with spaces
			// instead of commas by gettomg just the text prior to the first space and ignoring the rest.
			let effectType = definitions[i + 1];
			effectType = effectType.split(" ")[0];
			effectType = parseInt(effectType);

			//console.log("Draw Effect " + i + " " + effectType);

			// Skip past -1 and the effect Id;
			i += 2;

			switch (effectType)
			{
				case this.markerStyleProperties.EFFECT_BLEND:
					i = this.translateDefinitionForBlendEffect(definitions, i);
					break;

				case this.markerStyleProperties.EFFECT_INNER_GLOW:
				case this.markerStyleProperties.EFFECT_OUTER_GLOW:
					i = this.translateDeinitionForGlowEffect(definitions, i);
					break;

				case this.markerStyleProperties.EFFECT_DROP_SHADOW:
					i = this.translateDefinitionForDropShadowEffect(definitions, i);
					break;

				case this.markerStyleProperties.EFFECT_LINE_DASH:
					i = this.translateDefinitionForLineDashEffect(definitions, i);
					break;
			}
		}
	}

	translateDefinitionForBlendEffect(definitions, i) 
	{
        let options = this.getEffectOptions(definitions, i);
        i += options.length;
        this.blendMode = this.getValidBlendMode(this.getOptionText(options, 0, "multiply"));
        return i;
    }

	translateDefinitionForDropShadowEffect(definitions, i)
	{
        let options = this.getEffectOptions(definitions, i);
        i += options.length;

        let distance = this.getOptionNumber(options, 0, 4);

        // Increase the distance to be more like the result we got with Flash.
        distance *= 2;

        let angle = this.getOptionNumber(options, 1, 45);
        let alpha = this.getOptionAlpha(options, 3, 40);
        let colorRgba = this.getOptionColor(options, 2, "0x000000", alpha);
        let blur = this.getOptionBlur(options, 4, 10);

        // Convert the angle to horizontal and vertical distances. Note that we have to do this
        // because the Flash version of this logic used a single distance plus an angle whereas in
        // HTML5 there's x and y distances and no angle.
        let dh;
        let dv;

        if (angle < 0 || angle > 359)
            angle = 0;

        if (angle <= 90) {
            dh = (90 - angle) / 90;
            dv = angle / 90;
        }
        else if (angle <= 180) {
            angle -= 90;
            dh = (angle / 90) * -1;
            dv = (90 - angle) / 90;
        }
        else if (angle <= 270) {
            angle -= 180;
            dh = ((90 - angle) / 90) * -1;
            dv = (angle / 90) * -1;
        }

        else {
            angle -= 270;
            dh = angle / 90;
            dv = ((90 - angle) / 90) * -1;
        }

        // Right now dh and dv are percentages. Convert them to pixel distances.
        dh = Math.round(dh * distance);
        dv = Math.round(dv * distance);

        // Record the drop shadow values.
        this.shadow = new Object();
        this.shadow.shadowOffsetX = dh;
        this.shadow.shadowOffsetY = dv;
        this.shadow.shadowBlur = blur;
        this.shadow.shadowColor = colorRgba;

        // Calculate the markers bounds need to include its shadow  blur. Add a little
		// extra to the calculated blur because without it the bounds are sometimes too
		// small which can result in the shape not geting fully erased when redrawn.
		blur += 4;
        dh += dh < 0 ? -blur : blur;
        dv += dv < 0 ? -blur : blur;

        this.setSizeOfEffects(Math.abs(dh), Math.abs(dv));
        return i;
    }

	translateDeinitionForGlowEffect(definitions, i) 
	{
        let options = this.getEffectOptions(definitions, i);
        i += options.length;

        let alpha = this.getOptionAlpha(options, 1, 50);

        // The marker's line color is used as the default glow color.
        let defaultGlowColor = "0x" + this.markerStyleProperties.lineColor.substr(1);
        let colorRgba = this.getOptionColor(options, 0, defaultGlowColor, alpha);

        // Record the glow values.
        let blur = this.getOptionBlur(options, 2, 10);
        this.glow = new Object();
        this.glow.shadowBlur = blur;
        this.glow.shadowColor = colorRgba;

        this.setSizeOfEffects(blur, blur);
        return i;
    }

	translateDefinitionForLineDashEffect(definitions, i) 
	{
		let options = this.getEffectOptions(definitions, i);
		i += options.length;

		let pattern = [];
		for (const option of options)
		{
			let segment = parseInt(option, 10);
			if (isNaN(segment) || segment < 1)
				segment = 1;
			pattern.push(segment);
		}
		this.lineDash = pattern;

		return i;
	}
}

class MapsAliveMarkerStyleProperties
{
	constructor(map, fillColor, fillColorOpacity, lineColor, lineColorOpacity, effectsUserDefinition)
	{
		this.map = map;

		this.fillColor = fillColor;
		this.lineColor = lineColor;
		this.fillColorOpacity = fillColorOpacity;
		this.lineColorOpacity = lineColorOpacity;

		// Constants.
		this.EFFECT_BLEND = 1;
		this.EFFECT_INNER_GLOW = 2;
		this.EFFECT_OUTER_GLOW = 3;
		this.EFFECT_DROP_SHADOW = 4;
		this.EFFECT_LINE_DASH = 5;

		let definition = this.convertEffectsUserDefinition(effectsUserDefinition.replace(/ /g, ''));
		this.effects = new MapsAliveMarkerStyleEffects(this.map, this, definition);
	}

	convertEffectsUserDefinition(effects)
	{
		if (effects.length === 0)
			return '';

		// Convert the friendly text that user's provide to specify effects into codes used internally.
		let s = effects.toLowerCase();
		s = s.replace(/blend/g, `-1,${this.EFFECT_BLEND}`);
		s = s.replace(/innerglow/g, `-1,${this.EFFECT_INNER_GLOW}`);
		s = s.replace(/glow/g, `-1,${this.EFFECT_OUTER_GLOW}`);
		s = s.replace(/shadow/g, `-1,${this.EFFECT_DROP_SHADOW}`);
		s = s.replace(/linedash/g, `-1,${this.EFFECT_LINE_DASH}`);

		// Allow a semicolon to be used to separate effects.
		s = s.replace(/;/g, ",");
		return s;
	}
}

class Handle
{
	constructor(handles, index, x, y, xh, yh, coord, prev = -1, next = -1)
	{
		this.isBreakPoint = false;
		this.handles = handles;
		this.index = index;
		this.x = x;
		this.y = y;
		this.xh = xh;
		this.yh = yh;
		this.coord = coord;
		this.prev = prev;
		this.next = next;

		this.COLOR_RED = '#ff0000';
		this.COLOR_WHITE = '#ffffff';
		this.COLOR_BLUE = '#009de0';
		this.COLOR_GREEN = '#00cc00';
		this.COLOR_BLACK = '#000000';
	}

	breakPointColor(breakPoint)
	{
		const handle = this.handles.handleUnderPointer;
		let color = { fill: "", line: "", alpha: 1.0 };

		if (handle && handle.isBreakPoint && handle.index === breakPoint.index)
		{
			color.fill = this.COLOR_RED;
			color.line = this.COLOR_BLACK;
			color.fillAlpha = 0.75;
		}
		else
		{
			color.fill = this.COLOR_WHITE;
			color.line = this.COLOR_BLUE;
			color.fillAlpha = 1.0;
		}

		return color;
	}

	get handleColor()
	{
		let color = { fill: "", line: "", fillAlpha: 1.0 };

		const handle = this.handles.handleUnderPointer;

		const handleIsUnderPointer = handle !== null && this.index === handle.index && !handle.isBreakPoint;

		// Colors for polygons and lines.
		if (this.handles.selected.length >= 1)
		{
			if (this.handles.marker.shapeHasPoints)
			{
				if (this.handles.handleBeingDragged && this.index === this.handles.handleBeingDragged.index)
				{
					// Color of a handle that is being dragged.
					color.fill = this.COLOR_RED;
					color.line = this.COLOR_BLACK;
					color.fillAlpha = 0.5;
					return color;
				}

				if (this.index === this.handles.selected[0])
				{
					// Color of the first selected handle.
					color.fill = this.COLOR_RED;
					color.line = this.COLOR_BLACK;
					color.fillAlpha = handleIsUnderPointer ? 0.5 : 1.0;
					return color;
				}

				if (this.handles.selected.includes(this.index))
				{
					// Color of other selected handles.
					color.fill = this.COLOR_GREEN;
					color.line = this.COLOR_BLACK;
					color.fillAlpha = handleIsUnderPointer ? 0.5 : 1.0;
					return color;
				}
			}
			else if (this.index === this.handles.selected[0])
			{
				// Color of the only selected handle for a circle or rectangle.
				color.fill = this.COLOR_RED;
				color.line = this.COLOR_BLACK;
				color.fillAlpha = 0.5;
				return color;
			}
		}

		if (handleIsUnderPointer)
		{
			// Color of an unselected handle that is under the pointer.
			color.fill = this.handles.selectedHandleCount === 0 ? this.COLOR_RED : this.COLOR_GREEN;
			color.line = this.COLOR_BLUE;
			color.fillAlpha = 0.75;
			return color;
		}

		// Color of an unselected handle that is not under the pointer.
		color.fill = this.COLOR_WHITE;
		color.line = this.COLOR_BLUE;
		color.fillAlpha = 1.0;

		return color;
	}
}

class Handles
{
	constructor(marker)
	{
		this.marker = marker;
		this.initialize();

		this.HANDLE_SIDE = 9;
		this.HANDLE_OFFSET = 4;
	}

	addBreakPoint(index, prev, next)
	{
		// Calculate this breakpoint's location as halfway on the line between its prev and next handles.
		let x = Math.round((prev.x + next.x) / 2);
		let y = Math.round((prev.y + next.y) / 2);

		let xh = x;
		let yh = y;

		// Calculate the coord for this breakpoint in case it gets converted to a handle.
		let coordX = Math.round((prev.coord.x + next.coord.x) / 2)
		let coordY = Math.round((prev.coord.y + next.coord.y) / 2)
		let coord = { x: coordX, y: coordY };

		// Create the breakpoint which is actually a Handle object with its breakpoint flag set.
		let breakPoint = new Handle(this, index, x, y, xh, yh, coord, prev, next);
		breakPoint.isBreakPoint = true;

		this.breakPoints.push(breakPoint);
	}

	addHandle(index, x, xOffset, y, yOffset, coord = null)
	{
		// Index is the position of this handle within the shape's set of handle where 0 is the
		// first handle, 1 is the second and so on. The x and y values are a point on the shape.
		// The offset values indicate how to shift the handle square so that its center is at or
		// near the point. For polygons the handle is centered on the point, but for rectancles
		// the handle is shifed outward from the corner.
		let xh = x + xOffset;
		let yh = y + yOffset;

		let handle = new Handle(this, index, x, y, xh, yh, coord);
		this.handles.push(handle);
		this.fixupPrevNext()

		return handle;
	}

	adjacentHandles(handle)
	{
		let index = handle.index;
		let prev, next;

		let last = this.count - 1;

		if (index === 0)
		{
			prev = this.handles[last];
			next = this.handles[1];
		}
		else if (index === last)
		{
			prev = this.handles[last - 1];
			next = this.handles[0];
		}
		else
		{
			prev = this.handles[index - 1];
			next = this.handles[index + 1];
		}

		return { prev, next };
	}

	get count()
	{
		return this.handles.length;
	}

	createHandlesForCircle(bounds, offset)
	{
		let x = bounds.centerX_screen;
		let y = bounds.centerY_screen;

		let radius = Math.round(this.marker.shapeW / 2);
		if (this.marker.markerZooms)
			radius = this.marker.map.convertMapToScreen(radius);

		this.addHandle(0, x - radius, -offset, y, -offset);
		this.addHandle(1, x, -offset, y - radius, -offset);
		this.addHandle(2, x + radius, -offset, y, -offset);
		this.addHandle(3, x, -offset, y + radius, -offset);
	}

	createHandlesForPolygon(bounds, offset)
	{
		let coords = this.marker.shapeCoordsArray;

		let index = 0;

		for (let i = 0; i < coords.length; i += 2)
		{
			let x = coords[i];
			let y = coords[i + 1];
			let coord = { x, y };

			if (x === -1)
				break;

			if (this.marker.markerZooms)
			{
				x = this.marker.map.convertMapToScreen(x);
				y = this.marker.map.convertMapToScreen(y);
			}

			x += bounds.centerX_screen;
			y += bounds.centerY_screen;

			let w = Math.round(this.marker.shapeW / 2);
			if (this.marker.markerZooms)
				w = this.marker.map.convertMapToScreen(w);
			x -= w;

			let h = Math.round(this.marker.shapeH / 2);
			if (this.marker.markerZooms)
				h = this.marker.map.convertMapToScreen(h);
			y -= h;

			this.addHandle(index, x, -offset, y, -offset, coord);
			index += 1;
		}
	}

	createHandlesForRectangle(bounds, side, offset)
	{
		let x = bounds.cornerX_screen;
		let y = bounds.cornerY_screen;
		let w = bounds.w_screen;
		let h = bounds.h_screen;

		offset -= 2;

		this.addHandle(0, x, -offset, y, -offset);
		this.addHandle(1, x + w, -side + offset, y, -offset);
		this.addHandle(2, x + w, -side + offset, y + h, -side + offset);
		this.addHandle(3, x, -offset, y + h, -side + offset);
	}

	deleteHandle()
	{
		if (!this.handles.selectedHandleCount === 1)
			return;

		let coords = this.marker.shapeCoordsArray;
		let index = this.selectedHandle.index;
		this.setSelectedHandle(null);

		coords.splice(index * 2, 2);
		this.handles.splice(index, 1);

		index = 0
		for (let handle of this.handles)
		{
			handle.index = index;
			index += 1;
		}

		this.fixupPrevNext()

		this.marker.shapeCoordsArray = coords;
	}

	fixupPrevNext(handle)
	{
		// Fixup the previous and next values for all handles
		for (handle of this.handles)
		{
			let adjacent = this.adjacentHandles(handle);
			handle.prev = adjacent.prev;
			handle.next = adjacent.next;
		}
	}

	getHandleForCoord(x, y)
	{
		for (const handle of this.handles)
		{
			if (handle.coord && handle.coord.x === x && handle.coord.y === y)
				return handle;
		}
		return null;
	}

	getHandleList()
	{
		// Don't create handles if they already exists.
		if (this.count > 0)
			return this.handles;

		this.invalidateHandles();

		const bounds = this.marker.getBounds();

		switch (this.marker.shapeType)
		{
			case this.marker.SHAPE_TYPE_CIRCLE:
				this.createHandlesForCircle(bounds, this.HANDLE_OFFSET);
				break;

			case this.marker.SHAPE_TYPE_POLYGON:
			case this.marker.SHAPE_TYPE_LINE:
			case this.marker.SHAPE_TYPE_HYBRID:
				this.createHandlesForPolygon(bounds, this.HANDLE_OFFSET);
				break;

			case this.marker.SHAPE_TYPE_RECTANGLE:
				this.createHandlesForRectangle(bounds, this.HANDLE_SIDE, this.HANDLE_OFFSET);
				break;
		}

		return this.handles;
	}

	insertHandle(pointer, point)
	{
		let index = this.count;
		let handle = this.addHandle(index, pointer.x, -this.HANDLE_OFFSET, pointer.y, -this.HANDLE_OFFSET, point);
		return handle;
	}

	initialize()
	{
		this.invalidateHandles();
		this.handleBeingDragged = null;
		this.handleUnderPointer = null;
		this.selected = [];
	}

	invalidateHandles()
	{
		this.handles = [];
		this.breakPoints = [];
	}

	get lineColorBetweenHandles()
	{
		return '#009de0';
	}

	get selectedHandle()
	{
		return this.selected.length >= 1 ? this.handles[this.selected[0]] : null;
	}

	get selectedHandleCount()
	{
		return this.selected.length;
	}

	get selectedHandleIndex()
	{
		return this.selected.length >= 1 ? this.selected[0] : -1;
	}

	setAdjacentHandleSelected(prev)
	{
		let adjacent = this.adjacentHandles(this.selectedHandle);
		this.setOnlySelectedHandle(prev ? adjacent.prev : adjacent.next);
	}

	setBreakPoints()
	{
		if (!this.marker.shapeHasPoints)
			return;

		let handle = this.selectedHandle;

		if (handle === null)
			return;

		this.breakPoints = [];

		if (this.marker.shapeIsLine)
		{
			// Only set one breakpoint for the a lines first and last handles.
			if (handle.index === 0)
			{
				this.addBreakPoint(1, handle, handle.next);
				return;
			}
			else if (handle.index === this.count - 1)
			{
				this.addBreakPoint(0, handle, handle.prev);
				return;
			}
		}

		this.addBreakPoint(0, handle, handle.prev);
		this.addBreakPoint(1, handle, handle.next);
	}

	setOnlySelectedHandle(handle)
	{
		// Set the handle as the shape's only selected handle.
		this.selected = [];
		this.selected.push(handle.index);
	}

	setSelectedHandle(handle, deselectSelectedHandle = false)
	{
		// A null handle means to deselect all selected handles.
		if (handle === null)
		{
			this.selected = [];
			return;
		}

		let handleIndex = handle.index;

		// When no handles are selected, make this the selected handle.
		if (this.selected.length === 0)
		{
			this.selected.push(handleIndex);
			return;
		}

		// Determine if the handle is already selected and if yes, whether to deselect it.
		for (const index of this.selected)
		{
			if (index === handleIndex)
			{
				if (deselectSelectedHandle)
				{
					let i = this.selected.indexOf(index);
					this.selected.splice(i, 1);
				}
				return;
			}
		}

		// Add the handle to the list of selected handles.
		this.selected.push(handleIndex);
	}
}