// Copyright (C) 2009-2022 AvantLogic Corporation: https://www.mapsalive.com

import { MapsAliveRuntime as Runtime__$$ } from './mapsalive-runtime.js';

export { MapsAlivePopup };

class MapsAlivePopup
{
	constructor(page)
	{
		//console.log(`Popup__$$::constructor`);

		this.page = page;
		this.tour = page.tour;

		this.STATE_CLOSED = 1;
		this.STATE_OPEN = 2;
		this.STATE_PINNED = 3;
		this.popupState = this.STATE_CLOSED;

		this.availableArea = { top: null, right: null, bottom: null, left: null };

		this.waitingToShowPopup = false;
		this.waitingPopupIsPinned = false;

		// Bind event handlers so that when they are called, 'this' will be set to this MapsAlivePopup object.
		this.onClickPopupX = this.onClickPopupX.bind(this);
		this.onPopupMouseOut = this.onPopupMouseOut.bind(this);
		this.onPopupMouseOver = this.onPopupMouseOver.bind(this);
		this.changePopupVisibility = this.changePopupVisibility.bind(this);
 
		// Constants
		this.LOCATION_MARKER_CENTER = 1;
		this.LOCATION_MARKER_EDGE = 2;
		this.LOCATION_POINTER = 3;
		this.DELAY_BEFORE_SHOWING_POPUP = 1;
		this.DELAY_BEFORE_CLOSING_POPUP = 2;
		this.ARROW_BASE_MIN = 20;
		this.ARROW_BASE_MAX = 48;
		this.ARROW_SIZE_MIN = 20;
		this.ARROW_PERCENT_MIN = 0.30;
		this.ARROW_TYPE_NONE = 0;
		this.ARROW_TYPE_SMALL = 1;
		this.ARROW_TYPE_LARGE = 2;
		this.POPUP_MARGIN = 4;

		// Other properties will be set by MapsAliveRuntime. It will then call this object's propertiesHaveBeenSet() method.
	}

	attachPopupListeners(e)
	{
		if (!this.requiresEventListeners)
			return;

		//console.log(`Popup::attachPopupListeners ${e.id}`);
		e.addEventListener('mouseover', this.onPopupMouseOver, false);
		e.addEventListener('mouseout', this.onPopupMouseOut, false);
	}

	calculateArrowSizeShrinkAmount(offScreenAmount)
	{
		// Return the amount of arrow size reduction that is needed to allow a partially off screen
		// popup to fit on screen, or if that's not possible, return the maximum arrow size reduction
		// that's acceptable without making the arrow smaller than the minimum allowed size.
		const minArrowSize = this.ARROW_SIZE_MIN * .75;
		return Math.min(this.arrowSize - minArrowSize, offScreenAmount);
	}

	calculateArrowTipGap(markerBounds)
	{
		// This method determines how far the tip of the popup arrow should be from the center
		// of the marker or from the mouse. If the popup does not use an arrow, it's the gap
		// between the edge of the popup and the marker center or mouse location.

		let gapX = 0;
		let gapY = 0;

		if (this.popupLocationOption === this.LOCATION_MARKER_EDGE)
		{
			if (this.arrowType === this.ARROW_TYPE_NONE)
			{
				gapX = -2;
				gapX = -2;
			}
			else
			{
				gapX = markerBounds.w_screen >= 16 ? -8 : -2;
				gapY = markerBounds.h_screen >= 16 ? -8 : -2;
			}

			// Shift the offset from the marker's center to its edge.
			gapX += this.map.convertMapToScreen(markerBounds.halfW_actual);
			gapY += this.map.convertMapToScreen(markerBounds.halfH_actual);
		}
		else if (this.popupLocationOption == this.LOCATION_POINTER && this.allowMouseOverPopup)
		{
			gapX = -1;
			gapY = -1;
		}

		if (!this.allowMouseOverPopup)
		{
			// Make the gap wide enough that the mouse won't easily get onto the popup even if
			// moved very quickly.If it does get onto the popup, the popup will close, but open
			// again immediately on the next mousemove event.This not noticable when it happens
			// occaisionally, but if the gap is too small, the rapid open and close will cause
			// the popup to flicker.The user can add an additional marker offset to make the gap
			// wider, but they can't use a negative offset to make it narrower than these values.
			gapX += 8;
			gapY += 8;
		}

		// Add the marker offset that the user specified (it could be negative or positive).
		gapX += this.markerOffset;
		gapY += this.markerOffset;

		return { x: gapX, y: gapY };
	}

	calculateAvailableArea(side)
	{
		// The method determines the size of the rectangular areas that are above, below,
		// to the right, and to the left of the point where the popup is anchored to its marker.

		let rect = new Object();

		switch (side)
		{
			case "top":
				rect.x = 0;
				rect.y = 0;
				rect.w = this.viewPort.w;
				rect.h = this.markerAnchorPoint.y;
				break;

			case "bottom":
				rect.x = 0;
				rect.y = this.markerAnchorPoint.y;
				rect.w = this.viewPort.w;
				rect.h = Math.max(this.viewPort.h - this.markerAnchorPoint.y, 0);
				break;

			case "right":
				rect.x = this.markerAnchorPoint.x;
				rect.y = 0;
				rect.w = Math.max(this.viewPort.w - this.markerAnchorPoint.x, 0);
				rect.h = this.viewPort.h;
				break;

			case "left":
				rect.x = 0;
				rect.y = 0;
				rect.w = this.markerAnchorPoint.x;
				rect.h = this.viewPort.h;
				break;

			default:
				break;
		}

		return rect;
	}

	calculateClipping(areaAvailableW, areaAvailableH)
	{
		let clippedW = areaAvailableW < this.panelSize.w ? this.panelSize.w - areaAvailableW : 0;
		let clippedH = areaAvailableH < this.panelSize.h ? this.panelSize.h - areaAvailableH : 0;

		let clippedPopupW = this.panelSize.w - clippedW;
		let clippedPopupH = this.panelSize.h - clippedH;

		let amount = (this.panelSize.w * this.panelSize.h) - (clippedPopupW * clippedPopupH);

		return {
			amount: amount,
			available: { w: clippedPopupW, h: clippedPopupH }
		};
	}

	calculateClippingForSide(side)
	{
		let clipping = 0;

		let minArrowSize = this.arrowType === this.ARROW_TYPE_NONE ? 0 : this.ARROW_SIZE_MIN;

		switch (side)
		{
			case 'top':
				clipping = this.calculateClipping(this.availableArea.top.w, this.availableArea.top.h - minArrowSize - this.arrowTipGap.y);
				break;

			case 'right':
				clipping = this.calculateClipping(this.availableArea.right.w - minArrowSize - this.arrowTipGap.x, this.availableArea.right.h);
				break;

			case 'bottom':
				clipping = this.calculateClipping(this.availableArea.bottom.w, this.availableArea.bottom.h - minArrowSize - this.arrowTipGap.y);
				break;

			case 'left':
				clipping = this.calculateClipping(this.availableArea.left.w - minArrowSize - this.arrowTipGap.x, this.availableArea.left.h);
				break;
		}

		//console.log(`EXAMINED ${side} ${clipping.amount}`)

		return clipping;
	}

	calculateMarkerAnchorPoint(pointer, markerBounds)
	{
		// Determines where the popup should be anchored to the marker. By default a marker is
		// anchored at its center, but the anchor point might need to be adjusted based on the
		// user-chosen popup location option and where the marker appears in the in the viewport.

		// Set the default anchor point to be the center of the marker.
		let anchorPoint = { x: markerBounds.centerX_screen, y: markerBounds.centerY_screen };

		// Handle the easy case first. The marker anchored is at the mouse or touch position.
		if (this.popupLocationOption === this.LOCATION_POINTER)
		{
			if (pointer === null)
			{
				// The popup normally appears at the pointer location, but this method is
				// getting called now to position the popup near its marker, but without
				// a pointer location.
				this.popupLocationOption = this.LOCATION_MARKER_CENTER;
				//console.log(`NO POINTER LOCATION`);
			}
			else
			{
				anchorPoint.x = pointer.x;
				anchorPoint.y = pointer.y;
				return anchorPoint;
			}
		}

		// Determine if the anchor point is off the canvas or very near the edge. This can
		// happen on a zoomable map with large markers like the states or counties where part
		// of the shape is visible on the canvas, but the actual center is off the canvas.
		// In this case, shift the anchor point onto the canvas near the edge.
		if (anchorPoint.x < this.POPUP_MARGIN)
			anchorPoint.x = this.POPUP_MARGIN;
		else if (anchorPoint.x > this.map.canvasW - this.POPUP_MARGIN)
			anchorPoint.x = this.map.canvasW - this.POPUP_MARGIN;
		if (anchorPoint.y < this.POPUP_MARGIN)
			anchorPoint.y = this.POPUP_MARGIN;
		else if (anchorPoint.y > this.map.canvasH - this.POPUP_MARGIN)
			anchorPoint.y = this.map.canvasH - this.POPUP_MARGIN;

		// Determine if the marker center is partially off screen due to scrolling. This can
		// happen with both zoomable and non-zoomable map if the browser window is scrolled
		// up or down, or left or right, such that part of the map is outside the view port
		// (off screen). In this case, shift the anchor point to the viewport's inside edge.
		if (this.popupLocationOption === this.LOCATION_MARKER_CENTER)
		{
			let offScreenX = (anchorPoint.x + this.mapLocation.x) * -1;
			if (offScreenX > 0)
			{
				// When the vertical center of the marker is off screen left, shift the anchor point right.
				anchorPoint.x += offScreenX + this.POPUP_MARGIN;
			}
			else
			{
				// When the vertical center of the marker is off screen right, shift the anchor point left.
				offScreenX = anchorPoint.x + this.mapLocation.x - this.viewPort.w;
				if (offScreenX > 0)
					anchorPoint.x -= offScreenX + this.POPUP_MARGIN;
			}

			let offScreenY = (anchorPoint.y + this.mapLocation.y) * -1;
			if (offScreenY > 0)
			{
				// When the horizontal center of the marker is off screen top, shift the anchor point down.
				anchorPoint.y += offScreenY + this.POPUP_MARGIN;
			}
			else
			{
				// When the horizontal center of the marker is off screen bottom, shift the anchor point up.
				offScreenY = anchorPoint.y + this.mapLocation.y - this.viewPort.h;
				if (offScreenY > 0)
					anchorPoint.y -= offScreenY + this.POPUP_MARGIN;
			}
		}

		return anchorPoint;
	}

	calculatePopupLocation()
	{
		this.adjustY = 0;

		// Determine the location of the popup's upper left corner at its best location.
		switch (this.bestSide.side)
		{
			case "left":
			case "right":
				this.calculatePopupLocationLeftOrRight();
				break;

			case "top":
			case "bottom":
				this.calculatePopupLocationUpOrDown();
				break;
		}
	}

	calculatePopupLocationAdjustY()
	{
		const minAnchorOffsetTop = 32;
		const minAnchorOffsetBottom = 48;

		this.popupAnchorPoint.y = this.markerAnchorPoint.y;

		// Make sure the anchor point is not so close to the corner that the arrow protrudes from the corner
		// radius. The arrow won't be pointing right at the marker but its tip will get adjusted later.
		if (this.popupAnchorPoint.y < minAnchorOffsetTop)
		{
			// The anchor point is too close to the top edge of the popup.
			this.adjustY = minAnchorOffsetTop - this.popupAnchorPoint.y;
			this.popupAnchorPoint.y = minAnchorOffsetTop;
		}
		else if (this.popupAnchorPoint.y > this.viewPort.h - minAnchorOffsetBottom)
		{
			// The anchor point is too close to the bottom edge of the popup.
			this.adjustY = (this.viewPort.h - minAnchorOffsetBottom) - this.popupAnchorPoint.y;
			this.popupAnchorPoint.y = this.viewPort.h - minAnchorOffsetBottom;
		}
	}

	calculatePopupLocationLeftOrRight()
	{
		//console.log(`Popup::calculatePopupLocationLeftOrRight '${this.bestSide.side}'`);

		// Recalculate the popup's display height now that it has been scaled and its position determined.
		// This is necessary to account for the popup needing more height if scaling it made it narrower
		// which would cause its text to grow taller.
		let rect = this.page.layout.calculateHtmlBoundingRect(this.page.layout.popupPanel.innerHTML, this.displaySize.w);
		if (rect.height > this.displaySize.h)
			this.displaySize.h = Math.min(rect.height, this.popupMaxH);

		this.popupAnchorPoint = { x: 0, y: 0 };
		this.popupCorner = { x: 0, y: 0 };

		// The popup needs to be positioned to the left or right of the marker. Ideally it will be centered
		// vertically next to the marker.However, if centering would clip the popup on the top or bottom,
		// the popup needs to be shifted up or down to prevent clipping and the popup anchor point
		// needs to be shifted so that it stays lined up horizontally with the marker's anchor point.

		let popupHeightMidPoint = this.displaySize.h / 2;

		// Detemine the popup's vertical position. 
		if (this.displaySize.h > this.viewPort.h)
		{
			//console.log('NOT ENOUGH HEIGHT');
			// There's not enough height to fit the entire popup. Clip it on the bottom.
			this.popupCorner.y = this.POPUP_MARGIN;

			this.calculatePopupLocationAdjustY();
			console.log(`VERTICAL (too tall) ${this.adjustY} : ${ this.popupAnchorPoint.y } : ${ window.scrollY }`);
		}
		else
		{
			// Determine if the popup will fit vertically centered next to its marker.
			let availableTop = this.markerAnchorPoint.y - popupHeightMidPoint - this.POPUP_MARGIN;
			let availableBottom = this.viewPort.h - this.markerAnchorPoint.y - popupHeightMidPoint;

			// Shorten the available space a little to prevent the bottom of the popup from being too near the bottom of
			// the screen. The POPUP_MARGIN provides enough visual gap based on the calculated height of the popup, but
			// when setPopupFinalHeight is called to set the popup's max-height based on the rendered height, some extra
			// height may be needed to prevent a vertical scroll bar from appearing.
			availableBottom -= 16;

			if (availableTop >= 0 && availableBottom >= 0)
			{
				// Center the popup next to its marker;
				this.popupCorner.y = this.markerAnchorPoint.y - popupHeightMidPoint;
			}
			else
			{
				// Shift the popup up or down so that it fits.
				if (availableTop >= 0)
				{
					//console.log('ROOM ON TOP');
					this.popupCorner.y = this.markerAnchorPoint.y - popupHeightMidPoint + availableBottom;
				}
				else if (availableBottom >= 0)
				{
					//console.log('ROOM ON BOTTOM');
					this.popupCorner.y = this.markerAnchorPoint.y - popupHeightMidPoint - availableTop;
				}
				else
				{
					// Center the popup within the vertical space available.
					this.popupCorner.y = this.viewPort.h / 2 - this.displaySize.h / 2;
				}
			}

			this.calculatePopupLocationAdjustY();
			//console.log(`VERTICAL (fits) ${this.adjustY} : ${this.popupAnchorPoint.y} : ${window.scrollY}`);
		}

		// Determine the popup's horizontal position.
		let extraW = this.arrowSize + this.arrowTipGap.x;
		if (this.bestSide.side === "left")
		{
			// Calculate the location of the left edge of the popup panel.
			this.popupCorner.x = this.markerAnchorPoint.x - this.displaySize.w - extraW + 1;
			this.popupAnchorPoint.x = this.markerAnchorPoint.x - extraW;
		}
		else if (this.bestSide.side === "right")
		{
			this.popupCorner.x = this.markerAnchorPoint.x + extraW - 1;
			this.popupAnchorPoint.x = this.markerAnchorPoint.x + this.arrowTipGap.x;
		}

		// Adjust the horizontal position if part of the popup is off screen to the left or right.
		// This is done by shrkinking the size of the arrow to allow more room for the popup.
		// Note that the popup is considered off screen if there is not enough room for its margin.
		if (this.arrowType !== this.ARROW_TYPE_NONE)
		{
			let offScreenX;
			if (this.bestSide.side === "left")
			{
				// Move the popup right by the amount that the arrow can be shrunk.
				offScreenX = this.popupCorner.x * -1 + this.POPUP_MARGIN;
				if (offScreenX > 0)
				{
					let shift = this.calculateArrowSizeShrinkAmount(offScreenX);
					this.arrowSize -= shift;
					this.popupCorner.x += shift;
					this.popupAnchorPoint.x = this.markerAnchorPoint.x - this.arrowSize - this.arrowTipGap.x;
				}
			}
			else if (this.bestSide.side === "right")
			{
				// Move the popup left by the amount that the arrow can be shrunk.
				offScreenX = this.popupCorner.x + this.displaySize.w + this.POPUP_MARGIN - this.viewPort.w;
				if (offScreenX > 0)
				{
					let shift = this.calculateArrowSizeShrinkAmount(offScreenX);
					this.arrowSize -= shift;
					this.popupCorner.x -= shift;
				}
			}
		}
	}

	calculatePopupLocationUpOrDown()
	{
		//console.log(`Popup::calculatePopupLocationUpOrDown '${this.bestSide.side}'`);

		this.popupAnchorPoint = { x: 0, y: 0 };
		this.popupCorner = { x: 0, y: 0 };

		// The popup needs to be positioned above or below the marker. Ideally it will be centered
		// horizontally below the marker. However, if centering would clip the popup on either side,
		// the popup needs to be shifted left or right to prevent clipping and the popup anchor point
		// point needs to be shifted so that it stays lined up vertically with the marker's anchor point.

		let popupWidthMidPoint = this.displaySize.w / 2;
		const minCornerOffset = 12;

		// Detemine the popup's horizontal position. 
		if (this.displaySize.w > this.viewPort.w)
		{
			//console.log('NOT ENOUGH WIDTH');
			// There's not enough width to fit the entire popup. Clip it on the right.
			this.popupCorner.x = this.POPUP_MARGIN * 2;
			this.popupAnchorPoint.x = this.markerAnchorPoint.x;
		}
		else
		{
			// Determine if the popup will fit horizontally centered above or below its marker.
			let availableLeft = this.markerAnchorPoint.x - popupWidthMidPoint - this.POPUP_MARGIN;
			let availableRight = this.viewPort.w - this.markerAnchorPoint.x - popupWidthMidPoint - this.POPUP_MARGIN;

			if (availableLeft >= 0 && availableRight >= 0)
			{
				// Center the popup beneath its marker.
				this.popupCorner.x = this.markerAnchorPoint.x - popupWidthMidPoint;
			}
			else
			{
				// Shift the popup left or right so that it fits.
				if (availableLeft >= 0)
				{
					//console.log('ROOM ON LEFT');
					this.popupCorner.x = this.markerAnchorPoint.x - popupWidthMidPoint + availableRight;
				}
				else if (availableRight >= 0)
				{
					//console.log('ROOM ON RIGHT');
					this.popupCorner.x = this.markerAnchorPoint.x - popupWidthMidPoint - availableLeft;
				}
				else
				{
					// Center the popup within the horizontal space available.
					this.popupCorner.x = this.viewPort.w / 2 - this.displaySize.w / 2;
				}
			}

			this.popupAnchorPoint.x = this.markerAnchorPoint.x;
			if (this.popupAnchorPoint.x < minCornerOffset)
				this.popupAnchorPoint.x = minCornerOffset;
			else if (this.popupAnchorPoint.x > this.viewPort.w - minCornerOffset)
				this.popupAnchorPoint.x = this.viewPort.w - minCornerOffset;
		}

		// Determine the popup's vertical position.
		let extraH = this.arrowSize + this.arrowTipGap.y;
		if (this.bestSide.side === "top")
		{
			// Calculate the location of the top edge of the popup panel.
			this.popupCorner.y = this.markerAnchorPoint.y - this.displaySize.h - extraH + 1;
			this.popupAnchorPoint.y = this.markerAnchorPoint.y - extraH;
		}
		else if (this.bestSide.side === "bottom")
		{
			this.popupCorner.y = this.markerAnchorPoint.y + extraH - 1;
			this.popupAnchorPoint.y = this.markerAnchorPoint.y + this.arrowTipGap.y;
		}

		// Adjust the veritcal position if part of the popup is off screen to the top or bottom.
		// This is done by shrkinking the size of the arrow to allow more room for the popup.
		// Note that the popup is considered off screen if there is not enough room for its margin.
		if (this.arrowType !== this.ARROW_TYPE_NONE)
		{
			let offScreenY;
			if (this.bestSide.side === "top")
			{
				// Move the popup down by the amount that the arrow can be shrunk.
				offScreenY = this.popupCorner.y * -1 + this.POPUP_MARGIN;
				if (offScreenY > 0)
				{
					let shift = this.calculateArrowSizeShrinkAmount(offScreenY);
					this.arrowSize -= shift;
					this.popupCorner.y += shift;
					this.popupAnchorPoint.y = this.markerAnchorPoint.y - this.arrowSize - this.arrowTipGap.y;
				}
			}
			else if (this.bestSide.side === "bottom")
			{
				// Move the popup up by the amount that the arrow can be shrunk.
				offScreenY = this.popupCorner.y + this.displaySize.h + this.POPUP_MARGIN - this.viewPort.h;
				if (offScreenY > 0)
				{
					let shift = this.calculateArrowSizeShrinkAmount(offScreenY);
					this.arrowSize -= shift;
					this.popupCorner.y -= shift;
				}
			}
		}
	}

	calculatePopupMediaSize()
	{
		let popupMediaSize = Runtime__$$.createSizeObject();

		if (this.page.layout.hasMediaArea)
		{
			this.page.layout.mediaAreaElement.style.display = this.page.currentView.mediaW === 0 ? 'none' : 'block';

			if (this.page.currentView.hasImage)
			{
				// Determine the size of the popup's image by scaling the original image to fit within the popup media area.
				// The values for scaledMediaAreaW/H are the popup's actual media size since popup scaling does not occur unless
				// the popup is too big to fit in the available space, but that adjustment is made after this method is called.
				// In V4, the original image can be larger than is needed for the popup so that there is more image to work
				// with if the tour is displayed on a mobile device and the mobile layout has a larger media area than the popup.
				let originalImageSize = Runtime__$$.createSizeObject(this.page.currentView.mediaW, this.page.currentView.mediaH);
				let popupMediaAreaSize = Runtime__$$.createSizeObject(this.page.layout.scaledMediaAreaW, this.page.layout.scaledMediaAreaH);
				popupMediaSize = Runtime__$$.scaledImageSize(originalImageSize, popupMediaAreaSize);
			}
			else if (this.page.currentView.hasEmbeddedMedia)
			{
				// Use the embedded media's size without attempting to scale it to fit the popup's media area.
				popupMediaSize = { w: this.page.currentView.mediaW, h: this.page.currentView.mediaH };
			}
		}

		return popupMediaSize;
	}

	calculatePopupSizeBasedOnContent()
	{
		// Determine if the popup's arrow needs to be hidden. When the arrow is supposed to point to either the edge
		// of the marker or the center of the marker, and the marker is a hybrid marker, don't show the arrow because
		// in most cases the arrow will end up pointing to some place where none of the hybrids component shapes are.
		this.arrowType = this.defaultArrowType;
		if (this.popupLocationOption === this.LOCATION_MARKER_EDGE || this.popupLocationOption == this.LOCATION_MARKER_CENTER)
		{
			let marker = this.page.map.getMarker(this.page.currentView.viewId);
			if (marker.shapeType === marker.SHAPE_TYPE_HYBRID)
				this.arrowType = this.ARROW_TYPE_NONE;
		}

		// Make sure that the font sizes the the content are for a full size popup. If the popup
		// had previously been shown at a reduced size, the font sizes will have been reduced as well.
		// NOTE: Fonts are not currently being scaled because it makes the text hard to read.
		this.page.layout.restoreFontSizes();

		// Initialize the popup panel size to be empty with a default font size of 1em.
		this.panelSize = { w: 0, h: 0, fontSize: 1 };

		// Calculate how much content area is being taken up by the margins.
		let marginsV = this.page.layoutMarginLeft + this.page.layoutMarginRight;
		let marginsH = this.page.layoutMarginTop + this.page.layoutMarginBottom;

		// Determine the media size.
		let popupMediaSize = this.calculatePopupMediaSize();

		// Places to store calculated content dimensions.
		let hasTextContent = this.page.layoutId !== "HII" && this.page.currentView.htmlText.length > 0;
		let textAreaRect = { width: 0, height: 0 };
		let textHtml = hasTextContent ? this.page.layout.textElement.outerHTML : "";
		let contentWidth = 0;
		let contentHeight = 0;

		switch (this.page.layoutId)
		{
			case "HII":
				// Media only layouts are simple. The size is the media dimensions plus margins and border all around.
				contentWidth = popupMediaSize.w;
				contentHeight = popupMediaSize.h;
				break;

			case "HTT":
				textAreaRect = this.page.layout.calculateHtmlBoundingRect(textHtml, this.popupMaxW - marginsV);
				contentWidth = textAreaRect.width;
				contentHeight = textAreaRect.height;
				break;

			case "HIITT":
			case "HTTII":
				if (hasTextContent)
				{
					let maxTextW = popupMediaSize.h > 0 ? popupMediaSize.w : this.popupMaxW - marginsV;
					textAreaRect = this.page.layout.calculateHtmlBoundingRect(textHtml, maxTextW);
				}

				// When there's no media, the popup width is the text width. When there is media, the popup width is
				// the media width instead of the text width so that there won't be empty space next to the image.
				// However, if the image is narrow, the text will be narrow too. In that case, the user can use the
				// popup's Min Size option to set a width that works well for the text.
				contentWidth = popupMediaSize.w === 0 ? textAreaRect.width : popupMediaSize.w;

				contentHeight = popupMediaSize.h + textAreaRect.height + this.page.layoutSpacingH;

				// Add horizontal spacing between the media and text. If no spacing is needed, set it to zero to replace
				// spacing that might be there from the previous popup. Use ems so the space will scale when the popup scales.
				if (popupMediaSize.h > 0)
				{
					let spacingH = hasTextContent ? this.page.layout.pxToEm(this.page.layoutSpacingH) : 0;
					if (this.page.layoutId === "HTTII")
						this.page.layout.mediaAreaElement.style.marginTop = spacingH;
					else if (this.page.layoutId === "HIITT")
						this.page.layout.mediaAreaElement.style.marginBottom = spacingH;
				}
				break;

			default:
				break;
		}

		// Calculate and the panel size based on the content, popup title, margins, and the border.
		const panelW = contentWidth + marginsV + this.borderWidth * 2;
		let panelH = contentHeight + marginsH + this.borderWidth * 2;

		if (this.page.layout.popupElements.title)
		{
			let popupTitleStyle = getComputedStyle(this.page.layout.popupElements.title);
			panelH += parseInt(popupTitleStyle.height, 10);
		}

		// Set the calculated panel size. This is an estimate of the dimensions of the panel unscaled.
		// It will be used to determine where the panel will best fit when it is scaled. Since scaling
		// makes the panel narrower, but its text content does not scale, the text height will actually
		// increase as the panel gets narrower. This is handled by setting the panel element's max-height
		// so that the text area won't get taller but the text will scroll.
		this.setPopupPanelSize(panelW, panelH);

		// Set the popup's max-height so that subsequent examinations of its browser-determined bounds
		// will be based on that height, not on the calculated panel height.
		this.page.layout.popupPanel.style.maxHeight = this.panelSize.h + 'px';

		// Use auto for the text height to let the brower determine the actual height after the popup
		// is scaled since it's virtually impossible to accurately calculate the rendered text height.
		if (hasTextContent)
			this.page.layout.textElement.style.height = 'auto';
	}

	changePopupVisibility({ show, pin = false })
	{
		let oldState = this.popupState;
		let hide = !show;

		if (show)
		{
			// Always pin the popup on a touch-only device so that the popup close X shows.
			if (this.tour.isTouchDeviceWithoutMouse && !this.map.slideShowRunning)
				pin = true;

			this.setPopupState({ open: true, pinned: pin });

			this.attachPopupListeners(this.page.layout.popupPanel);
			if (this.arrowType != this.ARROW_TYPE_NONE)
				this.attachPopupListeners(this.page.layout.popupArrow);
		}
		else
		{
			this.setPopupState({ open: false, pinned: false });

			this.removePopupListeners(this.page.layout.popupPanel);
			if (this.arrowType != this.ARROW_TYPE_NONE)
				this.removePopupListeners(this.page.layout.popupArrow);
		}

		Runtime__$$.assert(this.popupState !== oldState, (`Same popup state requested ${this.popupState}`));

		//console.log(`Popup::changePopupVisibility ${show} ${pin}`);

		let visibility = show ? "visible" : "hidden";
		this.page.layout.popupPanel.style.visibility = visibility;

		this.page.layout.popupCloseX.style.display = this.popupState === this.STATE_PINNED ? "block" : "none";

		if (this.arrowType !== this.ARROW_TYPE_NONE)
			this.page.layout.popupArrow.style.visibility = visibility;

		if (this.page.layout.mediaAreaElement)
		{
			this.page.layout.mediaAreaElement.style.visibility = visibility;

			if (hide)
			{
				// Replace the image with nothing and make it invisible. The idea
				// is that if the next image to be displayed is not loaded immediately, 
				// nothing will showPopup in the meantime instead of the previous image.
				this.page.layout.mediaAreaElement.innerHTML = "";
			}
		}

		if (hide && oldState === this.STATE_PINNED)
			this.tour.api.callbackPopupClosed(this.page.currentView);
	}

	chooseBestResizeScale(panelSize, available)
	{
		// Determine the scale that the popup must be reduced by to fit the available space.
		let scale1 = available.w / panelSize.w;
		let scale2 = available.h / panelSize.h;
		let scale;

		// Use the larger scale if the popup will fit within the container, otherwise, use the smaller scale.
		let largerScale = Math.max(scale1, scale2);
		let smallerScale = Math.min(scale1, scale2);
		let w = Math.round(largerScale * panelSize.w);
		let h = Math.round(largerScale * panelSize.h);
		if (w <= available.w && h <= available.h)
			scale = largerScale;
		else
			scale = smallerScale;

		// Handle the rare case where the scale is negative because the popup calculation came up
		// with a negative width because the viewport is so smamll there's no room for the popup.
		if (scale < 0)
			scale = 0.0;

		return scale;
	}

	chooseBestSide()
	{
		//console.log(`Popup::chooseBestSide`);

		const OUTSIDE_1 = 24;
		const OUTSIDE_2 = 25;
		const OUTSIDE_3 = 26;
		let usingOutsideSequence = this.bestSideSequence >= OUTSIDE_1 && this.bestSideSequence <= OUTSIDE_3;
		let minScale = 1.0;

		if (usingOutsideSequence)
		{
			if (this.bestSideSequence === OUTSIDE_2)
				minScale = 0.75;
			else if (this.bestSideSequence === OUTSIDE_3)
				minScale = 0.50;
		}

		let sideSequence;

		switch (this.bestSideSequence)
		{
			case 0: sideSequence = "right,left,bottom,top"; break;
			case 1: sideSequence = "right,left,top,bottom"; break;
			case 2: sideSequence = "right,bottom,top,left"; break;
			case 3: sideSequence = "right,bottom,left,top"; break;
			case 4: sideSequence = "left,right,top,bottom"; break;
			case 5: sideSequence = "left,right,bottom,top"; break;
			case 6: sideSequence = "left,top,right,bottom"; break;
			case 7: sideSequence = "left,top,bottom,right"; break;
			case 8: sideSequence = "bottom,top,left,right"; break;
			case 9: sideSequence = "bottom,top,right,left"; break;
			case 10: sideSequence = "bottom,left,top,right"; break;
			case 11: sideSequence = "bottom,left,right,top"; break;
			case 12: sideSequence = "top,bottom,left,right"; break;
			case 13: sideSequence = "top,bottom,right,left"; break;
			case 14: sideSequence = "top,right,bottom,left"; break;
			case 15: sideSequence = "top,right,left,bottom"; break;
			case 16: sideSequence = "top"; break;
			case 17: sideSequence = "bottom"; break;
			case 18: sideSequence = "right"; break;
			case 19: sideSequence = "left"; break;
			case 20: sideSequence = "top,bottom"; break;
			case 21: sideSequence = "bottom,top"; break;
			case 22: sideSequence = "right,left"; break;
			case 23: sideSequence = "left,right"; break;
			case OUTSIDE_1:
			case OUTSIDE_2:
			case OUTSIDE_3:
				sideSequence = this.chooseOutsideSequence();
				break;
			default: break;
		}

		// Determine which side has the least amount of clipped area by comparing the amount of
		// clipping for each side one at a time and comparing it to the amount for the previous
		// side. To get started, use the largest interior as the least clip amount found so far. 
		let sideClipping;
		let sides = sideSequence.split(',');

		// Use the first choice as a default to handle the rare case where no side
		// is chosen because the viewport is so small there's no room for the popup.
		let bestSide = sides[0];

		let bestScaleSoFar = 0.0;

		for (let side of sides)
		{
			let thisSideClipping = this.calculateClippingForSide(side);
			let thisSideScale = this.chooseBestResizeScale(this.panelSize, thisSideClipping.available);
			//console.log(`SCALE ${side} ${thisSideScale}`);

			if (thisSideClipping.amount === 0)
			{
				// This side has no clipping so choose it. There may be other sides with no clipping, but the user prefers
				// this side to any others. Return the scale as 100% instead of returning thisSideScale because the latter
				// will have been slightly reduced by chooseBestResizeScale to allow room around the popup if near an edge.
				bestSide = side;
				sideClipping = thisSideClipping;
				bestScaleSoFar = 1.0;
				break;
			}

			if (thisSideScale > bestScaleSoFar || thisSideScale >= minScale)
			{
				// This side has some clipping, but less than any side examined so far. Select it as the best for now.
				bestSide = side;
				sideClipping = thisSideClipping;
				bestScaleSoFar = thisSideScale;

				// When using the outside sequence, use this side if its scale meets the minimum requirement.
				if (usingOutsideSequence && thisSideScale >= minScale)
					break;
			}
		}

		//console.log(`CHOSE ${bestSide} ${bestScaleSoFar} PREFER ${sideSequence}`);

		// Reduce the scale just a little to leave some extra room around the popup. This also helps adjust for
		// the fact that it's virtually impossible to precisely determine the scaled popup dimensions and have
		// them match the actual dimensions rendered by the browser.
		if (bestScaleSoFar < 1.0)
			bestScaleSoFar *= 0.95;

		this.bestSide = { side: bestSide, scale:bestScaleSoFar, clipping: sideClipping };
	}

	chooseOutsideSequence()
	{
		let quadrant;

		// Calculate the dimensions of the visible portion of the map within the canvas. If the map is scaled such
		// that it is narrower and/or shorter than the canvas dimensions, use the scaled maps dimensions so that
		// determination of the marker's quadrant is based only on the visible map, ignoring empty canvas area.
		let scaledMapW = this.map.convertMapToScreen(this.map.mapW_actual);
		let scaledMapH = this.map.convertMapToScreen(this.map.mapH_actual);
		let w = Math.min(this.map.canvasW, scaledMapW);
		let h = Math.min(this.map.canvasH, scaledMapH);

		// Determine which of the four quadrants of the visible map contains the marker.
		// The quadrants are upper-left:1, uppr-right:2, lower-left:3, lower-right:4.
		let x = this.markerAnchorPoint.x - this.mapLocation.left;
		let y = this.markerAnchorPoint.y - this.mapLocation.top;
		if (y < h * 0.5)
			quadrant = x < w * 0.5 ? 1 : 2;
		else
			quadrant = x < w * 0.5 ? 3 : 4;

		// Determine whether the marker is positioned in the left or right side of its quadrant. For quadrants 1 and 3,
		// the marker is in the left half if its position is less than 25% from the left edge of the visible map. For
		// quardrants 2 and 4, the marker is in the the right half if its position is greater than 75% from the left
		// edge of the map. Choose the sequence based on the quadrant and the marker's position within the quadrant.
		let sequence;
		switch (quadrant)
		{
			case 1:
				sequence = x < w * 0.25 ? "left,top,right,bottom" : "top,left,right,bottom";
				break;
			case 2:
				sequence = x > w * 0.75 ? "right,top,left,bottom" : "top,right,left,bottom";
				break;
			case 3:
				sequence = x < w * 0.25 ? "left,bottom,top,right" : "bottom,left,top,right";
				break;
			case 4:
				sequence = x > w * 0.75 ? "right,bottom,top,left" : "bottom,right,top,left";
				break;
		}

		console.log(`Popup::chooseOutsideSequence ${quadrant} : ${sequence}`)

		return sequence;
	}

	choosePopupSide(pointer)
	{
		// This method determines where the popup will appear on the screen.

		// Get the bounds of the marker on the screen relative to the upper left corner of the map.
		let markerBounds = this.map.selectedMarker.getBounds();

		// Determine where the popup should be anchored to the marker.
		this.markerAnchorPoint = this.calculateMarkerAnchorPoint(pointer, markerBounds, this.viewPort);

		// Adjust the anchor point to take into account the map's location on the screen.
		this.markerAnchorPoint.x += this.mapLocation.x;
		this.markerAnchorPoint.y += this.mapLocation.y;

		// Determine the available space above, right, bottom, and left of the marker.
		this.availableArea.top = this.calculateAvailableArea("top", this.viewPort);
		this.availableArea.right = this.calculateAvailableArea("right", this.viewPort);
		this.availableArea.bottom = this.calculateAvailableArea("bottom", this.viewPort);
		this.availableArea.left = this.calculateAvailableArea("left", this.viewPort);

		this.arrowSize = this.getArrowSize();
		this.arrowTipGap = this.calculateArrowTipGap(markerBounds);
		this.arrowLineWidth = this.getArrowLineWidth();

		//console.log(`VIEWPORT/SCROLL ${this.viewPort.w}x${this.viewPort.h}, ${window.scrollX},${window.scrollY}`);
		//console.log(`SIDES Top:${this.availableArea.top.w}x${this.availableArea.top.h} Right:${this.availableArea.right.w}x${this.availableArea.right.h} Bottom:${this.availableArea.bottom.w}x${this.availableArea.bottom.h} Left:${this.availableArea.left.w}x${this.availableArea.left.h}`);
		//console.log(`NEED ${this.panelSize.w}x${this.panelSize.h} GAP ${this.arrowTipGap.x},${this.arrowTipGap.y} FROM ${this.markerAnchorPoint.x},${this.markerAnchorPoint.y}`);
	}

	drawPopupArrow()
	{
		if (this.arrowType === this.ARROW_TYPE_NONE)
			return;

		//console.log(`Popup::drawPopupArrow ${this.bestSide.side} ${this.popupAnchorPoint.x},${this.popupAnchorPoint.y} ${this.markerAnchorPoint.x},${this.markerAnchorPoint.y}`)

		// This method determines a) the shape of the arrow's triangle and b) positions the arrow
		// along the popup's side to keep the arrow pointing at its marker. The arrow is a polyline
		// that is shaped like a little house with two walls and a roof. The roof is the two angled
		// lines that meet at the tip of the arrow which points to its marker. The length of the walls
		// is just enough to so that the arrow can cut through the popup panel's border and get drawn
		// next to the interior of the popup at a right angle so that the popup background color looks
		// like it flows into the arrow. Without the wall extension, a small corner of the angled roof
		// lines is visible just inside the popup.

		// Most of the logic of this method deals solely with the triangle that is formed between the
		// two roof lines and an imaginary base. The triangle's median is an invisible line segment with
		// one end at the triangle's vertex (arrow tip) and the other at the midpoint of the base. The
		// end points of the median are the marker's anchor point and the popup's anchor point.

		// When there is plenty of room in the viewport for the popup, the popup's anchor point is at
		// the exact center of the popup's side (50% of the distance) and the arrow shape is an isoceles
		// triangle with the center of its base on the popup's anchor point and the the arrow tip at the
		// marker's anchor point. This shape is used as long as the popup is centered on the popup's edge.
		//
		// When there's not enough room to center the popup with its marker, the popup hits a viewport
		// edge and so the arrow must shift along the popup's edge to stay lined up with the marker. In
		// the cases where the marker's anchor point is near or at the edge of the view port, and thus the
		// popup's anchor point is near or at the corner of the popup, the arrow's shape must change from
		// isoceles to a right triangle, otherwise part of its base would extend beyond the popup and go
		// off screen. When the arrow is a right triangle, its adjacent side is also the median segment
		// connecting the popup and marker anchor points.
		//
		// Rather than waiting until the arrow is near the edge of the popup to switch from an isoceles
		// triangle to a right triangle, and then changing its shape, this logic gradually morphs the
		// triangle from isosoles to scalene, eventually turning it into a right triangle. This looks
		// better and avoids a sudden change in the arrow's shape. To do this, the logic first determines
		// where the arrow is positioned on the popup edge, and then if the arrow is not at the center,
		// it determines the percentage distance from the center to the corner. As the percentage
		// increases from 0% to 100% the shapes changes from isoceles to right triangle by shifting
		// the arrow tip within the rectangle that contains the triangle, and by shifting the rectangle
		// to keep the tip lined up with the marker's anchor point.

		// Initially set the length of the base of the triangle to half the arrow size. Then adjust to
		// make sure the base is not too narrow or too wide. 
		let base = this.arrowSize / 2;
		if (base < this.ARROW_BASE_MIN)
			base = this.ARROW_BASE_MIN;
		else if (base > this.ARROW_BASE_MAX)
			base = this.ARROW_BASE_MAX;
		let arrowPointsLeftOrRight = this.bestSide.side === 'left' || this.bestSide.side === 'right';

		// Make sure that the base is not longer than half the side of a very small popup.
		base = arrowPointsLeftOrRight ? Math.min(base, this.displaySize.h * .3) : Math.min(base, this.displaySize.w * .3);

		// The house-shaped polygon has four sides connected by five points. Regardless of the orientation
		// and direction of the arrow, the two walls are formed by the lines between pt1 & pt2, and between
		// pt4 & pt5. The roof lines are between pt2 & pt3, and between pt3 & pt4. The house is attached to
		// a side of the popup at pt1 and pt5. When drawing lines, the line thickness must be taken into 
		// account because half the thickness is drawn on each side of the vector between the line's start
		// and end points. Calculations in the logic below use this half-thickness as an offset where needed.
		let drawingOffset = this.arrowLineWidth / 2;

		// The extension value is the length of the sides of the house-shape. A length of just one pixel
		// seems to be enough to prevent a corner of the angled lines from protruding into the popup pannel.
		let extension = 1;

		// The width and height are the dimensions of the rectangle that contains the polyline that will
		// be drawn from pt1 through pt5.
		let width = arrowPointsLeftOrRight ? this.arrowSize + extension : base + this.arrowLineWidth;
		let height = arrowPointsLeftOrRight ? base + this.arrowLineWidth : this.arrowSize + extension;

		// The cases below set pt1 - pt5 and tipX and tipY for each of the four possible arrows orientations
		// and directions (left, right, up, down). This way, the drawing code at the end only needs to
		// draw a line throught the points without knowing which kind of arrow it's drawing.
		let pt1, pt2, pt3, pt4, pt5;
		let tipX, tipY;
		let left, top;
		let shift = 0;
		let percent, halfPercent, halfWay;
		let gradient;

		// Get the location and size of the popup panel.
		let popupPanelBounds = this.page.layout.popupPanel.getBoundingClientRect();
		if (popupPanelBounds.height < this.popupMinH)
			popupPanelBounds.height = this.popupMinH;

		// Define the sides, gradient, and position of the arrow's triangle without the extensions
		// which get added later after the triangle shape and position have been determined's.
		switch (this.bestSide.side)
		{
			case 'left':  // Right arrow '>'
			case 'right': // Left arrow '<'
				// The popup is to the right or left of its marker. When the popup is on the right, its,
				// arrow points to the left. When the popup is on the left, its arrow points to the right.

				// Set the initial location of the arrow's tip.
				tipX = this.arrowSize
				tipY = (base + this.arrowLineWidth) / 2;

				// Get the popup's anchor point percentage distance along the popup's vertical edge.
				percent = (this.popupAnchorPoint.y - this.popupCorner.y) / this.displaySize.h;

				// Determine how much to shift the arrow tip to the bottom or top.
				if (percent === 0.5)
				{
					// The arrow is a horizontal isoceles triange pointing from the middle of the popup panel.
					shift = 0;
				}
				else
				{
					// The arrow is a scalene triangle. Calculate how far up or down its tip must shift
					// to keep it in line with its marker. The amount of shift ranges from nearly zero
					// to 100% of the tip distance depending on the percentage distance of the
					// arrow from the center of the popup toward the popup's top or bottom corner.
					if (percent < 0.5)
					{
						halfPercent = 1.0 - (percent * 2);
						shift = -tipY * halfPercent;
					}
					else
					{
						halfWay = this.displaySize.h / 2;
						halfPercent = (this.popupAnchorPoint.y - this.popupCorner.y - halfWay) / halfWay;
						shift = tipY * halfPercent;
					}
				}

				// Adjust the arrow tip up or down using the tip's vertical position in the arrow polyline.
				tipY += shift;
				if (tipY < drawingOffset)
					tipY = drawingOffset;

				tipY -= this.adjustY;

				console.log(`TIP ${tipX},${tipY}`);

				// Adjust the arrow vertically by the amount that the tip shifted downward.
				top = this.popupAnchorPoint.y - Math.max(tipY, 0);

				// Further adjust the arrow position if necessary to prevent its top or bottom
				// edge from going beyond the bottom or top edge of the popup panel.
				let bottomEdge = this.popupCorner.y + this.displaySize.h - (this.arrowLineWidth / 2);
				let tooFarDown = top + base - bottomEdge;
				if (tooFarDown > 0)
				{
					top -= tooFarDown;
				}
				else
				{
					let topEdge = this.arrowLineWidth * 1.5;
					if (top < topEdge)
						top = topEdge;
				}

				// Determine if the popup panel was moved down so it's not so close to the top which makes its arrow point
				// upward such that the tip is at the top of the arrow's SVG container. To draw the arrow that way requires
				// that all the drawing points, except the tip, get adjusted down. Because of the upward or downward slant,
				// the base needs to be a little wider so that the arrow doesn't look took skinny.
				const baseAdjust = 12;
				let adjustV = 0;
				if (this.adjustY > 0)
				{
					tipY = 0;
					adjustV = this.adjustY;
					base += baseAdjust;
				}
				else if (this.adjustY < -baseAdjust)
				{
					base += Math.min(-this.adjustY - baseAdjust, baseAdjust);
				}

				// Define the points and gradient that form the house-shaped polyline.
				// Position the arrow to cut through the popup panel's left or right border.
				switch (this.bestSide.side)
				{
					case 'left': // Right arrow '>'
						pt1 = [0, drawingOffset + adjustV];
						pt2 = [extension, drawingOffset + adjustV];
						pt3 = [tipX + extension, tipY];
						pt4 = [extension, base + adjustV];
						pt5 = [0, base + adjustV];
						left = popupPanelBounds.left + popupPanelBounds.width - this.borderWidth;
						gradient = { x1: 0, y1: 0, x2: 1, y2: 0 };
						break;

					case 'right': // Left arrow '<'
						pt1 = [this.arrowSize + extension, drawingOffset + adjustV];
						pt2 = [this.arrowSize, drawingOffset + adjustV];
						pt3 = [drawingOffset, tipY];
						pt4 = [this.arrowSize, base + adjustV];
						pt5 = [this.arrowSize + extension, base + adjustV];
						left = popupPanelBounds.left - width + this.borderWidth;
						gradient = { x1: 1, y1: 0, x2: 0, y2: 0 };
						break;
				}

				break;

			case 'top':    // Down arrow 'V'
			case 'bottom': // Up arrow '^'
				// The popup is above or below its marker. When the popup is on the top, its arrow
				// points down. When the popup is on the bottom, its arrow points up.

				// Set the initial location of the arrow's tip.
				tipX = (base + this.arrowLineWidth) / 2;
				tipY = this.arrowSize

				// Get the popup's anchor point percentage distance along the popup's horizontal edge.
				percent = (this.popupAnchorPoint.x - this.popupCorner.x) / this.displaySize.w;

				// Determine how much to shift the arrow tip to the left or right.
				if (percent === 0.5)
				{
					// The arrow is a vertical isoceles triange pointing from the middle of the popup panel.
					shift = 0;
				}
				else
				{
					// The arrow is a scalene triangle. Calculate how far left or right its tip must shift
					// to keep it in line with its marker. The amount of shift ranges from nearly zero
					// to 100% of the tip distance depending on the percentage distance of the
					// arrow from the center of the popup toward the popup's left or right corner.
					if (percent < 0.5)
					{
						halfPercent = 1.0 - (percent * 2);
						shift = -tipX * halfPercent;
					}
					else
					{
						halfWay = this.displaySize.w / 2;
						halfPercent = (this.popupAnchorPoint.x - this.popupCorner.x - halfWay) / halfWay;
						shift = tipX * halfPercent;
					}
				}

				// Adjust the arrow tip left or right using the tip's horizonatl position in the arrow polyline.
				tipX += shift;
				if (tipX < drawingOffset)
					tipX = drawingOffset;

				// Adjust the arrow horizontally by the amount that the tip shifted.
				left = this.popupAnchorPoint.x - tipX;

				// Further adjust the arrow position if necessary to prevent its right or left
				// side from going beyond the right or left edge of the popup panel.
				let rightEdge = this.popupCorner.x + this.displaySize.w - (this.arrowLineWidth / 2);
				let tooFarRight = left + base - rightEdge;
				if (tooFarRight > 0)
				{
					left -= tooFarRight;
				}
				else
				{
					let leftEdge = this.arrowLineWidth * 1.5;
					if (left < leftEdge)
						left = leftEdge;
				}

				// Define the points and gradient that form the house-shaped polyline.
				// Position the arrow to cut through the popup panel's top or bottom border.
				switch (this.bestSide.side)
				{
					case 'top': // Down arrow 'V'
						pt1 = [drawingOffset, 0];
						pt2 = [drawingOffset, extension];
						pt3 = [tipX, tipY + extension];
						pt4 = [base, extension];
						pt5 = [base, 0];
						top = popupPanelBounds.top + popupPanelBounds.height - this.borderWidth;
						gradient = { x1: 0, y1: 0, x2: 0, y2: 1 };
						break;

					case 'bottom': // Up arrow '^'
						pt1 = [drawingOffset, this.arrowSize + extension];
						pt2 = [drawingOffset, this.arrowSize];
						pt3 = [tipX, 0];
						pt4 = [base, this.arrowSize];
						pt5 = [base, this.arrowSize + extension];
						top = popupPanelBounds.top - height + this.borderWidth;
						gradient = { x1: 0, y1: 1, x2: 0, y2: 0 };
						break;
				}
				break;
		}

		// Detemine if the top of the arrow's SVG container needs to get moved up or down to stay in
		// alignment with the popup panel. A positive panel adjustment means that the panel was moved
		// down. A negative adustment means it was moved up. Adjust the top accordingly.
		top -= this.adjustY;

		height += Math.abs(this.adjustY) * 2;

		// Update the popup arrow's SVG container style with the arrow's position and dimensions.
		let style = this.page.layout.popupArrow.style;
		style.left = left + 'px';
		style.top = top + 'px';
		style.width = width + 'px';
		style.height = height + 'px';

		// Erase the previously drawn arrow.
		this.page.layout.popupArrowPolyline.points.clear();

		// Create an SVG point element for each point and add them to the arrow polyline.
		let points = [pt1, pt2, pt3, pt4, pt5];
		for (let point of points)
		{
			let svgPoint = this.page.layout.popupArrow.createSVGPoint();
			svgPoint.x = point[0];
			svgPoint.y = point[1];
			this.page.layout.popupArrowPolyline.points.appendItem(svgPoint);
		}

		// Set the direction of the gradient so that it is dark against
		// the popup edge and fades away toward the arrow's tip.
		this.page.layout.popupArrowGradient.setAttribute('x1', gradient.x1);
		this.page.layout.popupArrowGradient.setAttribute('y1', gradient.y1);
		this.page.layout.popupArrowGradient.setAttribute('x2', gradient.x2);
		this.page.layout.popupArrowGradient.setAttribute('y2', gradient.y2);
	}

	get followsMouse()
	{
		return this.location === this.LOCATION_POINTER && !this.allowMouseOverPopup && this.isShowing && !this.isPinned;
	}

	getArrowLineWidth()
	{
		return this.borderWidth;
	}

	getArrowSize()
	{
		// These values are known in PopupBehavior.aspx. If you change them here, also change them there.
		switch (this.arrowType)
		{
			case this.ARROW_TYPE_NONE:
				return 0;
			case this.ARROW_TYPE_SMALL:
				return 20;
			case this.ARROW_TYPE_LARGE:
				return 32;
			default:
				return this.arrowType;
		}
	}

	getPopupLocationOption()
	{
		// Get the preferred popup location for this map.
		let popupLocationOption = this.location;

		// Change the location from pointer to the marker's edge when there is no pointer.
		// This happens when displaying a popup while running a slide show or in response to 
		// a request from the directory. If the location is LOCATION_MARKER_CENTER, that
		// will get used, otherwise default to the marker's edge.
		if (!this.map.pointerIsOverMarker && popupLocationOption === this.LOCATION_POINTER)
			popupLocationOption = this.LOCATION_MARKER_EDGE;

		return popupLocationOption;
	}

	hidePopup()
	{
		//console.log(`Popup::hidePopup ${this.popupState}`);

		if (this.popupState === this.STATE_CLOSED)
			return;

		this.changePopupVisibility({ show: false });
	}

	get isPinned()
	{
		return this.popupState === this.STATE_PINNED;
	}

	isPopupElement(element)
	{
		// Determine if the element is part of a popup by checking if it has "Popup" as a prefix.
		if (element === null)
			return false;

		let popupElementPrefix = this.tour.uniqueId(`Popup`);
		//console.log(`Popup::isPopupElement ${popupElementPrefix} ${element.id}`);
		if (Runtime__$$.stringStartsWith(element.id, popupElementPrefix))
			return true;

		return false;
	}

	get isShowing()
	{
		return this.popupState !== this.STATE_CLOSED;
	}

	get layoutHasMediaArea()
	{
		// All the layouts have media except HTT.
		return this.page.layoutId !== "HTT";
	}

	get map()
	{
		return this.page.map;
	}

	movePopupToLocation(pointer)
	{
		// The value of pointer can be null when the request is to position the popup near
		// its marker as is the case when the user chooses a hotspot from the directory.

		if (this.page.currentView.hasNoContentForLayout())
			return;

		// Get the offset of the upper left corner of the map relative to the upper left corner of the viewport.
		let rect = this.page.layout.mapElement.getBoundingClientRect();
		this.mapLocation = { x: rect.left, y: rect.top, left: rect.left, top: rect.top };

		// Use a shorter name for the device size which is the visible area not including the toolbars on mobile.
		this.viewPort = this.tour.deviceSize;

		// Determine the popup's size and position.
		this.displaySize = null;
		this.popupLocationOption = this.getPopupLocationOption();
		this.calculatePopupSizeBasedOnContent();
		this.choosePopupSide(pointer);
		this.resizePopupToFitAvailableSpace();
		this.setPopupDisplaySize();
		this.calculatePopupLocation();
		this.setPopupSizeAndLocation({ x: this.popupCorner.x, y: this.popupCorner.y });

		this.setPopupFinalHeight();
	}

	onClickPopupX(event)
	{
		//console.log("onClickPopupX");
		this.map.deselectMarker();
		this.hidePopup();
	}

	onPopupMouseOut(event)
	{
		//console.log(`Popup::onPopupMouseOut ${this.isPinned}, ${this.allowMouseOverPopup}`);

		Runtime__$$.assert(this.map.markerIsSelected, `A popup mouseout event occurred when no marker is selected`);

		// Ignore a mouseout from the main popup panel onto the popup's arrow, media, or text elements.
		if (this.isPopupElement(event.relatedTarget))
			return;

		// Ignore a mouseout when the related target is contained within the popup panel. This handles the
		// case where an image, iframe, anchor tag, or other element is embedded with the popup's text area.
		if (event.relatedTarget && event.relatedTarget.offsetParent === this.page.layout.popupPanel)
			return;

		// Ignore a mouseout from a pinned popup.
		if (this.isPinned)
			return;

		// The element that was moused onto is not related to the popup. The user might have moused onto
		// the map, or back onto the marker. In either case, deselect the marker. If they moused back
		// onto the marker, the marker will get selected again immediately on the next mousemove event.
		this.map.handlePointerMovedOffMarker();
	}

	onPopupMouseOver(event)
	{
		//console.log(`Popup::onPopupMouseOver ${this.map.selectedMarkerViewId}`);

		if (this.isPinned)
			return;

		// Handle the case where the mouse has gotten onto a popup that does not allow the mouse to be
		// over it. Normally the popup moves out of the way as the mouse moves toward it. However, when
		// there isn't enough room for the popup anywere else, it doesn't move. This can also happen if
		// the mouse move very quickly and gets over the popup before it has moved. It can also happen
		// when a hotspot is chosen from the hotspot dropdown and the popup appears under the mouse. In
		// all cases, close the popup.In the case where the popup didn't move out of the way quickly
		// enough, the next mouse move will cause the popup to show again.
		if (!this.allowMouseOverPopup)
		{
			this.map.deselectMarkerAndClosePopup();
			this.map.resetMapPointerState();
			return;
		}

		// Clear the timer, if one has been set, to deselect the marker and close the popup shortly after
		// the mouse moves off the marker. If while the timer is running, the user mouses onto the the popup,
		// the timer needs to be cleared so that the popup doesn't go away.
		if (this.map.delayBeforeDeselectingMarkerTimerRunning)
			this.map.clearPopupDelayTimer();
	}

	// The 4 methods that follow provide a view max and min size. Normally these come from the page
	// values that apply to all views on the page; however, if the dimensions for an individual view
	// have been overridden, then those dimensions are used instead of the page values.
	get popupMaxH()
	{
		return this.page.currentView.popupOverrideH > 0 ? this.page.currentView.popupOverrideH : this.maxH;
	}

	get popupMaxW()
	{
		return this.page.currentView.popupOverrideW > 0 ? this.page.currentView.popupOverrideW : this.maxW;
	}

	get popupMinH()
	{
		let minH = Math.min(this.minH, this.page.layout.containerSize.h);
		return this.page.currentView.popupOverrideH > 0 ? this.page.currentView.popupOverrideH : minH;
	}

	get popupMinW()
	{
		let minW = Math.min(this.minW, this.page.layout.containerSize.w);
		return this.page.currentView.popupOverrideW > 0 ? this.page.currentView.popupOverrideW : minW;
	}

	propertiesHaveBeenSet()
	{
		// This method is called after MapsAliveRuntime has contructed this object and
		// set its properties with data sent from the Tour Builder in JavaScript files.

		if (this.tour.isTouchDeviceWithoutMouse)
		{
			// Delays don't make sense on touch devices.
			this.delay = 0;
		}

		this.fontSizes = null;

		this.layoutAreaW = this.maxW;
		this.layoutAreaH = this.maxH;

		// The max dimensions take into account popup margins (actually padding), but not the border.
		this.maxW += this.borderWidth * 2;
		this.maxH += this.borderWidth * 2;

		this.defaultArrowType = this.arrowType;
	}

	removePopupListeners(e)
	{
		if (!this.requiresEventListeners)
			return;

		//console.log(`Popup::removePopupListeners ${e.id}`);
		e.removeEventListener('mouseover', this.onPopupMouseOver, false);
		e.removeEventListener('mouseout', this.onPopupMouseOut, false);
	}

	get requiresEventListeners()
	{
		// Event listeners are needed to detect when the mouse has gone over a popup.
		return this.tour.isMouseDevice;
	}

	resizePopupToFitAvailableSpace()
	{
		//console.log(`Popup::resizePopupToFitAvailableSpace`);

		if (this.tour.disableResponsive)
			return;

		// Don't resize the popup if it contains and embeded iframe of video tag.
		if (this.page.currentView.hasEmbeddedMedia)
			return;

		// Determine the best side to display the popup relative to the marker based on user preference
		// and available space. The best side is one where the popup fits and matches the user's preference.
		this.chooseBestSide();
		let scale = this.bestSide.scale;

		// When there's enough room for the popup at full size, no reduction is necessary.
		if (scale === 1.0)
			return;

		// Scale the popup.
		let w = Math.round(scale * this.panelSize.w);
		let h = Math.round(scale * this.panelSize.h);

		// Special case the text-only layout to retain the maximum text height.
		if (this.page.layoutId === 'HTT')
			h = Math.min(this.panelSize.h, this.bestSide.clipping.available.h);

		// Adjust the dimensions up if the reduced size is less than the minimum.
		let minTextAreaWidth = 120;
		if (w < minTextAreaWidth)
		{
			let recipricol = minTextAreaWidth / w;
			w *= recipricol;
			h *= recipricol;
		}

		// Reduce the panel size.
		this.panelSize.w = Math.round(w);
		this.panelSize.h = Math.round(h);

		// Resize the text for layouts that have a text area.
		const minFontSizePx = 9;
		this.page.layout.setTextAreaFontSizes(scale, minFontSizePx);

		// Scale the popup panel's font size which is not used for fonts, but rather for the
		// popup's margins and spacing values which are in ems. By scaling this value, those
		// other values scale automaticaly because they are child elements of the popup panel.
		this.panelSize.fontSize = Runtime__$$.roundFloat(scale, 3);
	}

	setPopupCloseXPosition(topLeftCorner) 
	{
		const outer = this.page.layout.popupCloseXSizeOuter;
		const inner = this.page.layout.popupCloseXSizeInner;

		let top;
		let left;

		if (this.page.layout.popupElements.title)
		{
			// Compute the title's current style settings instead of using page.layoutMarginTop etc.
			// because those values will have been scaled if the popup is scaled.
			let titleStyle = getComputedStyle(this.page.layout.popupElements.title);
			let titleH = parseInt(titleStyle.height, 10);
			let popuPanelStyle = getComputedStyle(this.page.layout.popupPanel);
			let popupPaddingRight = parseInt(titleStyle.paddingRight, 10);
			if (popupPaddingRight === 0)
				popupPaddingRight = parseInt(popuPanelStyle.paddingRight, 10);
			let marginTopH = parseInt(popuPanelStyle.paddingTop, 10);

			// Place the X just inside the right side of the title area.
			top = topLeftCorner.y + this.borderWidth + marginTopH + Math.round(titleH / 2) - (outer / 2);
			const leftOffset = outer / 2 + inner / 2 + 6;
			left = topLeftCorner.x + this.displaySize.w - popupPaddingRight - this.borderWidth - leftOffset;
		}
		else
		{
			// When there's no title, place the X inside the border far enough that it won't cut into
			// rounted corners if the popup has them .The corner offset accounts for the fact that
			// there's a lot of empty space between the outer edge of the graphic and the actual X.
			const cornerOffset = 4;
			top = topLeftCorner.y + this.borderWidth - cornerOffset;
			left = topLeftCorner.x + this.displaySize.w - outer - this.borderWidth + cornerOffset;
		}

		// Shift the top up if the height of the popup is shorter than the close X graphic.
		if (this.displaySize.h < outer)
			top = topLeftCorner.y + this.displaySize.h / 2 - outer / 2;

		// Position the close X at the top right corner of the popup panel.
		let closeXStyle = this.page.layout.popupCloseX.style;
		closeXStyle.top = top + 'px';
		closeXStyle.left = left + 'px';
	}

	setPopupDisplaySize()
	{
		// Set the popup panel's width to the calculated width, but let the browser determine the height since
		// it has proved virtually impossible to accurately calculate the height when the popup is scaled.
		let style = this.page.layout.popupPanel.style;

		// Set the width to the calculated width.
		style.width = this.panelSize.w + 'px';
		style.fontSize = this.panelSize.fontSize + 'em';

		// Get the dimensions of the popup now that it's width has been set and the browser has perfomed
		// resizing. The displaySize dimensons will be use to position the popup and draw its arrow. The
		// bounds height may be taller than when the popup is rendered if the popup would be too tall to
		// fit. The popup's max-height will get set by setPopupFinalHeight after the popup has been
		// resize and moved to its display location.
		let bounds = this.page.layout.popupPanel.getBoundingClientRect();
		let h = Math.round(Math.max(bounds.height, this.popupMinH));
		this.displaySize = { w: Math.round(bounds.width), h: Math.min(h, this.popupMaxH) };
	}

	setPopupFinalHeight()
	{
		// The popup has been resized and moved to its display location.
		let popupPanelStyle = this.page.layout.popupPanel.style;
		let popupPanelBounds = this.page.layout.popupPanel.getBoundingClientRect();

		// Determine if its top is off the screen. This happens when a popup that contains a lot of text is scaled.
		// Scaling makes the popup narrower and because the text is not scaled, the text and thus the popup become
		// taller than the calculated dimensions et by setPopupDisplaySize. Because calculatePopupLocationUpOrDown
		// uses those display size dimensions to determine the popup's vertical position, when there's not enough
		// vertical, space, popupCorner.y becomes negative which puts the popup's top off screen. When this occurs,
		// shift the popup panel and close X down so the top of the panel is just below the top of the screen.
		let popupPanelTop = Math.round(popupPanelBounds.top);
		if (popupPanelTop < this.POPUP_MARGIN)
		{
			popupPanelTop = this.POPUP_MARGIN;
			popupPanelStyle.top = popupPanelTop + 'px';
			this.setPopupCloseXPosition({ x: popupPanelBounds.left, y: popupPanelTop });
		}

		// Calculate the height available for the popup as the distance from its top to near the bottom of the screen.
		const BOTTOM_PAD = 4;
		let availableHeight = this.viewPort.h - popupPanelTop - BOTTOM_PAD;

		// Reduce the available height if it is taller than the user-specified max height.
		availableHeight = Math.min(availableHeight, this.popupMaxH);

		// Reduce the available height if necessary to prevent the bottom of a top popup from going below the top of
		// its down-arrow. This can happen when the popup was vertically positioned based on its calculated height
		// but the rendered height is taller than calculated. This is not a problem with popups that are positioned
		// below, left, or right because their arrow's are not on the bottom. The ideal solution in this case would
		// be to move the top of the popup up so that its bottom would align with the top of the arrow, but the height
		// from popupPanelBounds is not always accurate. It is often too short, especially when the popup has been
		// scaled a lot. As such, the safe solution is to shorten the popup based on the distance between the top of
		// the panel and the top of the arrow.
		if (this.bestSide.side === 'top' && this.arrowType !== this.ARROW_TYPE_NONE)
		{
			let popupArrowBounds = this.page.layout.popupArrow.getBoundingClientRect();
			let popupArrowTop = Math.round(popupArrowBounds.top);
			availableHeight = popupArrowTop - popupPanelTop + this.borderWidth;
		}

		// For now assume that the available height should be set as the popup's max-height.
		let maxHeight = availableHeight + 'px';

		// Test for and handle the fringe case where a popup that contains an image is near the bottom
		// of the screen and the popup is scaled so much that due to the image's aspect ratio, the scaled
		// image overflows the bounds of the popup panel and cuts through its bottom border. In this rare
		// case, let the popup extend past the bottom of the screen by not setting max-height.
		const titleElement = this.page.layout.popupElements.title;
		const mediaElement = this.page.layout.popupElements.mediaArea;
		let titleH = titleElement ? parseInt(getComputedStyle(titleElement).height, 10) : 0;
		let mediaH = mediaElement ? parseInt(getComputedStyle(mediaElement).height, 10) : 0;
		if (titleH + mediaH > availableHeight)
			maxHeight = "unset";

		// At this point, the popup panel's max-height is set to the value that was used for calculating
		// its display size and position. However, due to the difference between the calculated height
		// and the browser-rendered height, the original max-height setting may be too short and if so,
		// using it could cause a vertical scroll bar to appear when not needed. Change the max height
		// to the available height calculated above to prevent an unneeded scroll bar. Note that if
		// max-height were not set, a vertical scroll bar would not appear even if the text won't fit.
		// Note also that the "text" of a popup could contain <img> tags and so the issues of height
		// are not restricted to actual text.
		popupPanelStyle.maxHeight = maxHeight;
		popupPanelStyle.minHeight = this.popupMin + 'px';
	}

	setPopupPanelSize(w, h)
	{
		// Make sure the dimensions are at least as large as the user-specified minimum.
		w = Math.max(w, this.popupMinW);
		h = Math.max(h, this.popupMinH);

		// Make sure the dimensions don't exceed the user-specified maximum.
		this.panelSize.w = Math.ceil(Math.min(w, this.popupMaxW));
		this.panelSize.h = Math.ceil(Math.min(h, this.popupMaxH));
	}

	setPopupSizeAndLocation(topLeftCorner)
	{
		let style = this.page.layout.popupPanel.style;
		style.left = topLeftCorner.x + 'px';
		style.top = topLeftCorner.y + 'px';

		// Position and draw the arrow.
		if (this.arrowType !== this.ARROW_TYPE_NONE)
			this.drawPopupArrow();

		this.setPopupCloseXPosition(topLeftCorner);
	}

	setPopupState({ open, pinned })
	{
		let oldState = this.popupState;

		if (open)
			this.popupState = pinned ? this.STATE_PINNED : this.STATE_OPEN;
		else
			this.popupState = this.STATE_CLOSED;

		Runtime__$$.assert(this.popupState !== oldState, ` New popup state is same as old state ${oldState}`);
	}

	showPopup(pin = false)
	{
		//console.log(`Popup::showPopup pin:${pin} WAITING:${this.waitingToShowPopup}`);

		if (this.page.currentView && this.page.currentView.hasNoContentForLayout())
			return;

		if (this.waitingToShowPopup)
		{
			this.waitingPopupIsPinned = pin;
			return;
		}

		if (this.isShowing && this.isPinned === pin)
			return;

		// Close any popup that might be open for another tour that's embedded in the same page as this tour.
		Runtime__$$.closePopups(this.tour);

		this.changePopupVisibility({ show: true, pin: pin });
	}

	showWaitingPopup()
	{
		//console.log(`Popup::showWaitingPopup pin:${this.waitingPopupIsPinned}`)
		this.waitingToShowPopup = false;
		this.showPopup(this.waitingPopupIsPinned);
	}

	startWaitingToShowPopup()
	{
		//console.log(`Popup::startWaitingToShowPopup`)
		this.waitingToShowPopup = true;
		this.waitingPopupIsPinned = false;
	}

	stopWaitingToShowPopup()
	{
		//console.log(`Popup::stopWaitingToShowPopup`)
		this.waitingToShowPopup = false;
		this.waitingPopupIsPinned = false;
	}
}