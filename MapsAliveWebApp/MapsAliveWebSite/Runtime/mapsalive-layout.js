// Copyright (C) 2009-2022 AvantLogic Corporation: https://www.mapsalive.com

export { MapsAliveLayout };

    import { MapsAliveRuntime as Runtime__$$ } from "./mapsalive-runtime.js";

class MapsAliveLayout
{
	constructor(page)
	{
		//console.log(`Layout__$$::constructor`);

		this.page = page;
		this.tour = page.tour;

		this.MOBILE_PANEL = 'panel';
		this.MOBILE_HIDDEN = 'hidden';
		this.MOBILE_PORTRAIT_PARTIAL = 'portrait-partial';
		this.MOBILE_LANDSCAPE_PARTIAL = 'landscape-partial';
		this.MOBILE_PORTRAIT_FULL = 'portrait-full';
		this.MOBILE_LANDSCAPE_FULL = 'landscape-full';
		this.MOBILE_NONE = 'none';

		this.NAV_MAP_LEFT = 2;
		this.NAV_MAP_RIGHT = 3;
		this.NAV_TITLE_BAR = 4;
		this.NAV_V3_TOP_MENU = 5;
		this.NAV_BANNER_LEFT = 10;
		this.NAV_BANNER_CENTER = 11;
		this.NAV_BANNER_RIGHT = 12;
		this.NAV_ABOVE_LEFT = 13;
		this.NAV_ABOVE_CENTER = 14;
		this.NAV_ABOVE_RIGHT = 15;

		this.responsiveTourScale = 1.0;
		this.navButton = null;
		this.navPanel = null;
		this.navButtonParent = null;
		this.helpPanelPageNumber = 0;
		this.helpPanelIsUsed = false;
		this.usingResponsiveStackedLayout = false;
		this.usingMobileLayout = false;
		this.responsiveLayoutId = '';
		this.scaledTourW = this.tour.width;
		this.scaledLayoutSpacingRow = 0;
		this.scaledLayoutSpacingColumn = 0;
		this.scaledMediaAreaW = null;
		this.scaledMediaAreaH = null;
		this.bannerAndTitleHeight = 0;
		this.fontSizes = null;
		this.containerSize = { w: 0, h: 0 };
		this.containerSizePrevious = { w: 0, h: 0 };
		this.containerSizeChanged = false;
		this.contentPanel = null;
		this.contentPanelAppearance = this.MOBILE_NONE;
		this.contentPanelIsHidden = true;
		this.contentPanelTabHeight = 34;
		this.contentPanelSize = Runtime__$$.createSizeObject(0, 0);
		this.currentImageSize = null;
		this.mapSize = { w: 0, h: 0 };
		this.popupElements = { mediaArea: null, textArea: null, title: null, text: null };
		this.tiledElements = { contentArea: null, mediaArea: null, textArea: null, title: null, text: null };

		this.bindEventHandlers();
		this.enableMobileLayout();
		this.identifyFullWidthHeightLayouts();
		this.getHtmlElements();
		this.getHtmlElementHeights();
		this.setPageTitle(this.page.pageTitle.length > 0 ? this.page.pageTitle : this.tour.name);

		this.createTourElements();

		this.detectIfGrabberBarNeeded();
	}

	addMapToLayout()
	{
		if (this.page.isDataSheet)
		{
			this.mapElement = null;
			return;
		}

		this.mapElement = document.createElement("div");
		this.mapElement.id = this.tour.uniqueId('Map');
		this.mapElement.className = 'maMap';
		this.mapElement.style.height = "auto";

		this.layoutElement.appendChild(this.mapElement);

		if (this.tour.isTouchDevice)
		{
			// Unlike the other touchstart listeners which follow the best practice of being passive, this one must
			// not be passive using { passive: true } because doing so will cause this JavaScript error to occur:
			// "Unable to preventDefault inside passive event listener." Since calling preventDefault is required
			// per the comments in onTouchMapElement, this is a compromise that must be made.
			this.mapElement.addEventListener('touchstart', this.onTouchMapElement, false);
			this.mapElement.addEventListener('touchend', this.onTouchMapElement, false);
		}
	}

	appendMediaAreaToContentArea()
	{
		this.contentArea.appendChild(this.tiledElements.mediaArea);
	}

	appendTextAreaToContentArea()
	{
		this.contentArea.appendChild(this.tiledElements.textArea);
	}

	bindEventHandlers() 
	{
		// The binding of the click handlers must occur before it is used by createElementsForLayout();
        this.onClickContentPanelTab = this.onClickContentPanelTab.bind(this);
        this.onClickContentPanelCloseX = this.onClickContentPanelCloseX.bind(this);
        this.onClickNavButton = this.onClickNavButton.bind(this);
		this.onClickMenuItem = this.onClickMenuItem.bind(this);
		this.onClickHelpButton = this.onClickHelpButton.bind(this);
        this.onTouchMapElement = this.onTouchMapElement.bind(this);
		this.onClickHelpPanelCloseX = this.onClickHelpPanelCloseX.bind(this);
    }

	calculateContainerSize()
	{
		// Get the size in CSS pixels of the visible screen, that is, what you can see. This is the starting point
		// for determining the container size which may be further reduced by containg elements that are not as
		// wide or tall as the client size.
		this.calculateDeviceSize();
		const deviceSize = this.tour.deviceSize;

		// Get the tour's immediate container element.
		let containerElement = this.tourElement.parentElement;

		let calculatedW;
		if (this.tour.isFlexMapTour)
		{
			// The width of a flex map container is the user-specified with or the client width.
			calculatedW = this.tour.getTourSettingInt('flex-width', deviceSize.w);
			let maxWidth = this.tour.getTourSettingInt('flex-max-width', calculatedW);
			if (maxWidth > 0 && calculatedW > maxWidth)
				calculatedW = maxWidth;
		}
		else
		{
			// The width of a classic tour container is the smaller of the tour width and client width.
			calculatedW = Math.min(this.tour.width, deviceSize.w);
		}

		// Determine what the tour's height should be. For a Classic tour, the height is the height set in the
		// Tour Builder.For a flex map tour, the tour will grow to fill the container height, provided that the map is
		// tall enough.Thus the tour's footer, if it has one, will be at the bottom of the container when the map is
		// tall, or at the bottom of a shorter map. When in Tour Preview, the bottom of a Flex Map is allowed to extend
		// offscreen below, so that the tour scale does not have to change to fit the smaller available height.
		let tourH;
		let flexMaxWidth = this.tour.getTourSettingInt('flex-max-width');
		let isFullWidthFlexMap = false;
		if (this.tour.isFlexMapTour)
		{
			// Determine if a flag or the data-flex-width-full option is set to show the map
			// using the browser's full width even if the tour's parent container is narrower.
			isFullWidthFlexMap = isNaN(flexMaxWidth) || flexMaxWidth > 0 ? false : true;

			let nonLayoutHeight = this.calculateScaledNonLayoutHeight(true);
			tourH = Math.min(this.map.mapHeight + nonLayoutHeight, deviceSize.h);

			if (this.tour.preview)
			{
				// Adjust the tour height to account for the height of the Tour Preview header, plus the Code Snippets and
				// Tour Advisor panels, so that the bottom of the flex map does not extend below the bottom of the screen.
				// This is in keeping with how flex maps grow or shrink, to fit the available space.
				let previewTop = document.getElementsByClassName('tourPreviewTop');
				let previewTopH = this.getComputedHeight(previewTop[0]);
				let areaBelowTour = deviceSize.h - (tourH + previewTopH);
				if (areaBelowTour < 0)
					tourH += areaBelowTour;
			}
		}
		else if (this.usingResponsiveStackedLayout)
		{
			// Make the tour height be the taller of its rendered height or twice the map height.
			// This seems to be a good compromise between using the tour height and the available height.
			// The tour height may be too short when first switching to the stacked layout and the device
			// height may be too tall when the available space is very narrow. The ideal solution would
			// be to somehow determine the actual height needed, but this solution is simple and this is
			// not an important enough scenario to invest in a more sophisticated solution for.
			tourH = Math.round(this.tourElement.getBoundingClientRect().height);
			tourH = Math.max(tourH, this.mapSize.h * 2);
		}
		else
		{
			// Use the height set on the Advanced Tour Layout page in the Tour Builder.
			tourH = this.tour.height;
		}

		// Get the flex-height option value if it's set. If flex-min-width is also set, scale the
		// flex-height value in proportion to how much the map is scaled when less than min width.
		let flexHeight = 0;
		if (this.tour.isFlexMapTour)
		{
			flexHeight = this.tour.getTourSettingInt('flex-height', 0);
			if (flexHeight > 0)
			{
				if (flexHeight > deviceSize.h)
					flexHeight = deviceSize.h;

				let flexMinWidth = this.tour.getTourSettingInt('flex-min-width', calculatedW);
				if (calculatedW < flexMinWidth)
				{
					let scale = calculatedW / flexMinWidth;
					flexHeight = Math.round(flexHeight * scale);
				}
			}
		}

		// Determine the initital container height before looking for parent elements having a shorter height.
		// The mobile layout uses the entire device height except for when data-flex-height is set.
		let calculatedH;
		if (this.usingMobileLayout && flexHeight === 0)
		{
			calculatedH = deviceSize.h;
		}
		else
		{
			if (this.tour.isFlexMapTour)
				calculatedH = flexHeight === 0 ? deviceSize.h : flexHeight;
			else
				calculatedH = Math.min(tourH, deviceSize.h);
		}

		// Walk up the chain of the tour's container elements to determine if any have a computed width or max-height
		// style that constrain the size of the tour. The logic is looking for width, not max-width, and for max-height,
		// not height, because width will be used to set the tour's width, but max-height will only be used to determine
		// if the tour's height must be constained to that height, but the tour's height may be shorter. Thus width is a
		// hard limit and max-height is a soft-limit, and if not found, the tour's height will have no limit.
		//
		// Note that if a parent container has its height syle set to a value less than the tour's height, the tour will
		// overflow the container. To prevent this from happening, the user should either not set a parent conainer height,
		// (they should use max-height instead) or they should set the tour's max-height to not exceed the container height.
		// This issues exists because this code cannot determine if a container element's height value was specified using
		// CSS or if it was computed by the browser. The getComputedStyle function simply returns the computed height and
		// examination of the parent element's style object will only return the value of an inline height. A computed height
		// will be incorrect when the tour first loads because it may only be the height of the portion of the tour above
		// the layout area (the banner, menu, title bar). On a resize, it may be the height of the tour before the reize.
		// SInce there's no straightforward way to tell if the height value is usable, this logic does not rely on height. 

		while (containerElement !== null)
		{
			// Get the container's computed width and max-height. The user can speciy these in px, vh, vmin, vmax.
			// Use of em is not recommended because it can be side-affected by the tour's responsive setting of em.
			// The % unit must not be used because its value is based on the actual size of the container, but the
			// container's size is based on the size of the tour which is rendered after this method is called.
			// Regardless of the unit type, the computed value is always in px. Use parseInt to strip 'px' from the
			// value. If the value is 'none', 'unset', or 'auto', parseInt will return NaN. 
			let computedContainerStyle = getComputedStyle(containerElement); 

			// Compare the container's width to the width calculated so far and keep the smaller of the two. 
			let computedWidth = parseInt(computedContainerStyle.width, 10);
			if (isNaN(computedWidth))
				computedWidth = calculatedW;

			if (this.tour.isStandalone)
			{
				// When a vertical scroll bar is present in a standalone tour, count its width as part of the computed width to
				// deal with the situation where the presence of the vertical scroll bar sometimes alternates (though not visibly)
				// while narrowing the browser window which causes the width calculation to be inconsistent when being checked
				// repeatedly as the window resizes.Without this adjustment, the map jitters when making the window narrower.
				let verticalScrollBarWidth = document.body.scrollWidth - document.body.clientWidth;
				computedWidth += verticalScrollBarWidth;
			}

			// Reduce the width by the parent container width.
			if (computedWidth < calculatedW && !isFullWidthFlexMap)
				calculatedW = computedWidth;

			// Compare the container's max-height to the height calculated so far and keep the smaller of the two.
			let computedMaxHeight = parseInt(computedContainerStyle.maxHeight, 10);
			if (!isNaN(computedMaxHeight) && computedMaxHeight <= calculatedH)
				calculatedH = computedMaxHeight;

			containerElement = containerElement.parentElement;
		}

		// Copy the previously calculated container size before updating the size.
		this.containerSizePrevious = Object.assign({}, this.containerSize);

		this.containerSize.w = Math.round(calculatedW);
		this.containerSize.h = Math.round(calculatedH);

		this.containerSizeChanged = this.containerSize.w !== this.containerSizePrevious.w || this.containerSize.h !== this.containerSizePrevious.h;

		//console.log(`Layout::calculateContainerSize ${this.containerSize.w}x${this.containerSize.h}`);
	}

	calculateDeviceSize()
	{
		// This is the nth attempt to accurately and consistently detect the device size. Previous attempts
		// were based on the thinking that it was not necessary to detect when the browser toolbars come and go,
		// but a resize must occur to prevent the bottom toolbars from appearing and hiding the bottom of the
		// mobile layout content panel, and to prevent a large gap under the mobile content content panel when
		// the tool bars go away. There was also some other odd behavior having to do with determination of the
		// image size as toolbars came and went.

		// This current attempt to detect the device size is based on trial and error testing on multiple
		// devices. On some, window.innerHeight accurately returns the current height (shorter when toolbars
		// are present and taller when they are hidden), but on others, it returns a number that is sometimes
		// much larger than the actual height; however, on those devices, document.documentElement.clientHeight
		// seems to return the correct result. The same goes for window.innerWidth.

		let w = window.innerWidth <= window.outerWidth ? window.innerWidth : document.documentElement.clientWidth;
		let h = window.innerHeight <= window.outerHeight ? window.innerHeight : document.documentElement.clientHeight;

		// Determine if the vertical scrollbar is visible, and if so, subtract it from the device width.
		let scrollW = this.usingMobileLayout ? 0 : w - document.documentElement.clientWidth;
		w -= scrollW;

		//console.log(`\nLayout::calculateDeviceSize ${w}x${h} ScrollW:${scrollW} Inner:${window.innerWidth}x${window.innerHeight} Outer:${window.outerWidth}x${window.outerHeight} Client:${document.documentElement.clientWidth}x${document.documentElement.clientHeight}`);

		this.tour.deviceSize = { w: w, h: h };
	}

	calculateHtmlBoundingRect(html, width = 0)
	{
		// Calculate the dimensions of the text area by rendering it inside a temporary
		// <div> tag. Use inline-grid so that the size is only as large as the contents.
		let e = document.createElement("div");
		e.style.display = "inline-grid";
		e.style.visibility = "hidden";

		// Restrict the width only if the caller provided a width in order to
		// cause the text to reflow vertically to fit within the constrained width.
		if (width > 0)
			e.style.width = width + 'px';

		// Append the temporary <div> to the tour, insert the HTML, and calculate its dimmensions.
		// The HTML's classes and inline styles will be applied and so the result should be accurate.
		this.tourElement.appendChild(e);
		e.innerHTML = html;
		let rect = e.getBoundingClientRect();

		// Handle the case where the caller-provided a width but the text is narrower, that is, it
		// does not have to reflow. In that case, the resulting width in the bounding rect will be
		// the caller width. Do the calculation again to get the actual, narrower width.
		if (width > 0)
		{
			e.style.width = "unset";
			let w = e.getBoundingClientRect().width;
			if (w < rect.width)
				rect.width = w;
		}

		e.remove();
		return rect;
	}

	calculateLayoutAreaPercentages()
	{
		this.calculateLayoutMapAreaPercentages();
		this.calculateLayoutMediaAreaPercentages();
	}

	calculateLayoutMapAreaPercentages()
	{
		// This method calculates what percentage of the layout width and height the map area
		// occupies in the user-chosen layout and splitter positions.

		let spacingRow = this.mapIsFullHeight ? 0 : this.page.layoutSpacingH;
		let spacingColumn = this.mapIsFullWidth ? 0 : this.page.layoutSpacingV;

		let innerLayoutHeight = this.tour.layoutAreaH - this.layoutPadding.top - spacingRow - this.layoutPadding.bottom;
		let innerLayoutWidth = this.tour.layoutAreaW - this.layoutPadding.left - spacingColumn - this.layoutPadding.right;

		this.mapAreaPercentW = this.map.mapAreaW / innerLayoutWidth;
		this.mapAreaPercentH = this.map.mapAreaH / innerLayoutHeight;
	}

	calculateLayoutMediaAreaPercentages()
	{
		// This method calculates what percentage of the layout width and height the media area
		// occupies in the user-chosen layout and splitter positions.

		let spacingH = this.mediaAreaIsFullHeight ? 0 : this.page.layoutSpacingH;
		let spacingV = this.mediaAreaIsFullWidth ? 0 : this.page.layoutSpacingV;

		let layoutAreaW = this.page.hasPopup ? this.page.popup.layoutAreaW : this.tour.layoutAreaW;
		let layoutAreaH = this.page.hasPopup ? this.page.popup.layoutAreaH : this.tour.layoutAreaH;
		let innerLayoutW = layoutAreaW - this.page.layoutMarginLeft - spacingV - this.page.layoutMarginRight;
		let innerLayoutH = layoutAreaH - this.page.layoutMarginTop - spacingH - this.page.layoutMarginBottom;

		this.mediaAreaPercentW = this.page.mediaAreaW / innerLayoutW;
		this.mediaAreaPercentH = this.page.mediaAreaH / innerLayoutH;
	}

	calculateLayoutPadding()
	{
		if (this.usingMobileLayout)
		{
			// Use no padding on small mobile devices..
			this.layoutPadding = { top: 0, right: 0, bottom: 0, left: 0 };
		}
		else
		{
			// Get the padding values for the layout. For popup layouts, use the user-specified map margins
			// for the padding because the user-specified layout margins are for the layout of the popup.
			// For tiled layouts, use the user-specified layout margins for the layout padding.
			this.layoutPadding = {
				top: this.page.hasPopup ? this.map.mapMarginTop : this.page.layoutMarginTop,
				right: this.page.hasPopup ? this.map.mapMarginRight : this.page.layoutMarginRight,
				bottom: this.page.hasPopup ? this.map.mapMarginBottom : this.page.layoutMarginBottom,
				left: this.page.hasPopup ? this.map.mapMarginLeft : this.page.layoutMarginLeft
			};
		}
	}

	calculateMapSize() 
	{
		if (this.usingResponsiveStackedLayout)
		{
			let layoutSize = Runtime__$$.createSizeObject(this.scaledInnerLayoutW, this.scaledInnerLayoutH);
			let scaledMapSize = Runtime__$$.scaledImageSize(this.map.mapSize_actual, layoutSize);
			this.mapSize.w = scaledMapSize.w;
			this.mapSize.h = scaledMapSize.h;
			return;
		}

		// Determine what percentage of the layout height the map occupies.
		let percentH;
		if (this.usingMobileLayout)
			percentH = 1.0;
		else
			percentH = this.mapAreaPercentH;

		// Determine what percentage of the layout width the map occupies.
		let percentW;
		if (this.usingResponsiveLayout)
			percentW = 1.0;
		else
			percentW = this.mapAreaPercentW;

		// Translate the percentages into pixels.
		let spacingRow = this.mapIsFullHeight ? 0 : this.scaledLayoutSpacingRow;
		let spacingColumn = this.mapIsFullWidth ? 0 : this.scaledLayoutSpacingColumn;
		this.mapSize.w = Math.round((this.scaledInnerLayoutW - spacingColumn) * percentW);
		this.mapSize.h = Math.round((this.scaledInnerLayoutH - spacingRow) * percentH);

		console.log(`Layout::calculateMapSize Map:${this.mapSize.w}x${this.mapSize.h} Container:${this.containerSize.w}x${this.containerSize.h}`);
	}

	calculateMediaAreaSize()
	{
		if (this.tour.editMode)
			return;

		// Determine what percentage of the layout height the media area occupies.
		let percentH;
		if (this.usingResponsiveLayout)
			percentH = 1.0;
		else
			percentH = this.mediaAreaPercentH;

		// Determine what percentage of the layout width the media area occupies.
		let percentW;
		if (this.usingResponsiveLayout)
			percentW = 1.0;
		else
			percentW = this.mediaAreaPercentW;

		let spacingRow = this.mediaAreaIsFullHeight ? 0 : this.scaledLayoutSpacingRow;
		let spacingColumn = this.mediaAreaIsFullWidth ? 0 : this.scaledLayoutSpacingColumn;

		let innerLayoutW;
		let innerLayoutH;
		if (this.page.hasPopup)
		{
			innerLayoutW = this.page.popup.layoutAreaW - this.page.layoutMarginLeft - spacingColumn - this.page.layoutMarginRight;
			innerLayoutH = this.page.popup.layoutAreaH - this.page.layoutMarginTop - spacingRow - this.page.layoutMarginBottom;
		}
		else
		{
			innerLayoutW = this.scaledInnerLayoutW;
			innerLayoutH = this.scaledInnerLayoutH;
		}

		// Translate the percentages into pixels.
		this.scaledMediaAreaW = Math.round((innerLayoutW - spacingColumn) * percentW);
		this.scaledMediaAreaH = Math.round((innerLayoutH - spacingRow) * percentH);

		//console.log(`Layout::calculateMediaAreaSize ${this.scaledMediaAreaW}x${this.scaledMediaAreaH}`);
	}

	calculateNavPanelSize() 
	{
		this.navPanelSize = Runtime__$$.createSizeObject(300, 150);
		if (this.tour.hasNavPanel) 
		{
			const NAV_BUTTON_OFFSET = 40;
			this.navPanelSize.w = Math.min(this.tour.directory.contentWidth, this.containerSize.w - NAV_BUTTON_OFFSET);
			this.navPanelSize.h = this.containerSize.h - 8;
		}
	}

	calculateScaledNonLayoutHeight(useScaledBannerHeight = true)
	{
		// For flex map tours, all of these heights except for the banner, will be unscaled
		// because the tour scale is always 1.0. The banner image scales to fit its container.

		let scaledBannerHeight = this.bannerElement ? this.tour.bannerImgScaledHeight : 0;
		let scaledTitleBarHeight = this.getComputedHeight(this.titleBarElement)
		let scaledHeaderStripeHeight = this.getComputedHeight(this.headerStripeElement);
		let scaledFooterStripeHeight = this.getComputedHeight(this.footerStripeElement);
		let scaledFooterHeight = this.getComputedHeight(this.footerElement);

		this.bannerAndTitleHeight = scaledBannerHeight + scaledTitleBarHeight;

		let bannerHeight = useScaledBannerHeight ? scaledBannerHeight : this.tour.bannerHeight;

		let nonLayoutHeight = 
			bannerHeight +
			scaledTitleBarHeight +
			scaledHeaderStripeHeight +
			scaledFooterStripeHeight +
			scaledFooterHeight;

		return nonLayoutHeight;
	}

	calculateScaledOuterLayoutHeight()
	{
		let scaledNonLayoutHeight = this.calculateScaledNonLayoutHeight();

		let height;
		if (this.usingResponsiveLayout) 
		{
			height = this.containerSize.h - scaledNonLayoutHeight;
		}
		else
		{
			if (this.tour.isFlexMapTour)
			{
				// Flex map tour's don't scale per se because there is no specific tour size. However, the map scales,
				// and the banner scales, but not the title bar, stripes or footer. The height of the container that
				// was derived by calculateContainerSize is based on all non-scaled nonlayout elements, but since the
				// banner can scale, the delta between its unscaled and scaled heights must be worked into the height
				// calculation for a flex map tour. If no delta were factored in, as the browser gets narrower, and the
				// banner gets shorter, blank space, which is about the height of the delta, appears below the tour.
				// For some reason, use of the full delta sometimes causes a vertical scroll bar to appear, but using
				// 90% seems to prevent the scroll bars and eliminates the blank space.
				let unscaledNonLayoutHeight = this.calculateScaledNonLayoutHeight(false);
				let bannerDelta = unscaledNonLayoutHeight - scaledNonLayoutHeight;
				height = this.containerSize.h - unscaledNonLayoutHeight + Math.round(bannerDelta * 0.9);
				if (height > this.map.mapSize_actual.h)
					height = this.map.mapSize_actual.h;
			}
			else
			{
				// Scale the tour height of a classic tour in proportion to the tour scale.
				let containerH = this.convertToResponsiveValue(this.tour.height);
				height = containerH - scaledNonLayoutHeight;
			}
		}

		return height;
	}

	changeContentPanelAppearance(newAppearance, visible = true) 
	{
		// The content panel is what is referred to in the User Guide as the slideout.
		// The term slideout came into use after this code was developed.

		if (newAppearance !== this.contentPanelAppearance)
		{
			// Hide the panel while switching its appearance. The MOBILE_HIDDEN class must be used
			// in conjunction with one of the visible classes because the transition from visible to
			// hidden must begin with the top and left values defined by the current class.
			this.contentPanel.classList.add(this.MOBILE_HIDDEN);

			// Add the class for the new appearance and remove the class for the old appearance.
			// It does not matter that the two classes have conflicting properties (e.g. width, height,
			// left, top) because the MOBILE_HIDDEN class is emitted below the visible class in the
			// style sheet and as such it overrides the properties of both classes.
			this.contentPanel.classList.add(newAppearance);
			this.contentPanel.classList.remove(this.contentPanelAppearance);
		}

		// Make the new panel visible.
		if (visible)
		{
			this.contentPanel.classList.remove(this.MOBILE_HIDDEN);
			this.contentPanelIsHidden = false;
		}

		//console.log(`Layout::changeContentPanelAppearance '${this.contentPanel.classList.value}'`);

		this.contentPanelAppearance = newAppearance;
		this.chooseContentPanelExpander();
	}

	chooseActiveLayout()
	{
		// Don't change the tour scale or layout when responsiveness is disabled.
		if (this.tour.disableResponsive)
			return;

		if (this.tour.isFlexMapTour)
		{
			this.scaledTourW = Math.min(this.containerSize.w, this.page.map.mapW_actual); 
		}
		else
		{
			// When the tour is too wide at 100%, scale it to fit the container element's width.
			if (this.tour.width > this.containerSize.w)
				this.responsiveTourScale = (this.containerSize.w / this.tour.width);
			else
				this.responsiveTourScale = 1.0;

			this.scaledTourW = this.convertToResponsiveValue(this.tour.width);
		}

		// Determine if a responsive layout is required. Popup tours always use the responsive
		// mobile layout on small mobile devices, but they never use the responsive stacked layout.
		if (this.usingMobileLayout)
		{
			this.usingResponsiveStackedLayout = false;
		}
		else if (!this.page.hasPopup)
		{
			// Use the responsive stacked layout when the tour needs to be scaled below a certain threshold.
			let thresholdPercent = this.tour.getTourSettingInt("stacked-layout", 50)
			if (thresholdPercent)
			{
				const thresholdScale = thresholdPercent / 100;
				this.usingResponsiveStackedLayout = this.responsiveTourScale < thresholdScale;
			}
		}

		if (this.usingMobileLayout)
		{
			if (this.containerOrientationIsPortrait)
			{
				this.responsiveLayoutId = 'R-PORTRAIT';
				this.showExtraLayoutElements(true, true, false, false);
			}
			else
			{
				this.responsiveLayoutId = 'R-LANDSCAPE';
				let showBanner = this.navButtonLocationIsBanner || this.tour.isEmbedded;
				this.showExtraLayoutElements(showBanner, this.tour.hasTitleBar, false, false);
			}
		}
		else
		{
			if (this.usingResponsiveStackedLayout)
				this.responsiveLayoutId = this.page.layoutId.includes('IT') ? 'HMIT' : 'HMTI'; 
			else
				this.responsiveLayoutId =  '';
			this.showExtraLayoutElements(true, true, true, true);
		}
	}

	chooseContentPanelExpander()
	{
		let showExpand = this.contentPanelAppearance === this.MOBILE_PORTRAIT_PARTIAL || this.contentPanelAppearance === this.MOBILE_LANDSCAPE_PARTIAL;
		let graphic = showExpand ? 'contentExpand' : 'contentContract';

		this.panelExpanderDisabled = false;
		if (showExpand)
		{
			this.panelExpanderDisabled =
				this.contentPanelAppearance === this.MOBILE_PORTRAIT_PARTIAL && this.disablePartialPortraitExpander ||
				this.contentPanelAppearance === this.MOBILE_LANDSCAPE_PARTIAL;
		}

		// Show the disabled portrait panel expander as faded. Hide it for landscape panel expansion which is not currently supported. 
		// Show the disabled portrait panel expander as faded. Hide it for landscape panel expansion which is not currently supported. 
		let disabledOpacity = this.contentPanelAppearance === this.MOBILE_PORTRAIT_PARTIAL ? 0.2 : 0;

		this.contentPanelExpander.style.opacity = this.panelExpanderDisabled ? disabledOpacity : 1.0;
		this.contentPanelExpander.src = this.tour.graphics[graphic].src;
	}

	cleanCss(css)
	{
		// Remove tabs and extra new lines, and replace multiple spaces with one space.
		css = css.replace(/\t/g, '');
		css = css.replace(/\n\n/g, '\n');
		css = css.replace(/  +/g, ' ');
		return css;
	}

	closeHelpPanel()
	{
		let helpPanel = Runtime__$$.getElement("HelpPanel", this.tour);
		if (helpPanel)
		{
			helpPanel.style.visibility = 'hidden';
			helpPanel.style.left = 0;

			// Destroy the help panel's content in case it contains video or audio that is playing.
			let helpContent = Runtime__$$.getElement("HelpContent", this.tour);
			if (helpContent)
				helpContent.innerHTML = "";
		}
	}

	closeNavPanel()
	{
		this.closeHelpPanel();

		if (this.navPanel === null)
			return;

		if (this.tour.hasNavPanel && this.tour.directory.staysOpen)
			return;

		this.navPanel.classList.remove('show');
	}

	configureLayoutAreas(resize)
	{
		this.calculateMediaAreaSize();

		//console.log(`Layout::configureLayoutAreas: ${this.page.layoutId} [${this.responsiveLayoutId}] ${this.responsiveTourScale.toFixed(2)}`);

		this.setTextAreaFontSizes();

		let layoutId = this.responsiveLayoutId !== '' ? this.responsiveLayoutId : this.page.layoutId;

		// Remove any content styling leftover from the previous layout so that it won't affect the current layout.
		this.contentPanel.style.removeProperty('height');
		this.contentPanel.style.removeProperty('width');
		this.contentPanel.style.removeProperty('overflow');
		this.contentPanel.style.margin = 0;

		this.contentArea.style.removeProperty('height');
		this.contentArea.style.removeProperty('flex-direction');
		this.contentArea.style.removeProperty('margin');
		this.contentArea.style.removeProperty('overflow');

		this.contentPanel.classList.remove(
			this.MOBILE_PANEL,
			this.MOBILE_HIDDEN,
			this.MOBILE_PORTRAIT_FULL,
			this.MOBILE_PORTRAIT_PARTIAL,
			this.MOBILE_LANDSCAPE_FULL,
			this.MOBILE_LANDSCAPE_PARTIAL);

		if (this.tiledElements.textArea)
		{
			this.tiledElements.textArea.style.removeProperty('height');
			this.tiledElements.textArea.style.removeProperty('overflow');
		}

		this.layoutElement.classList.remove('maStacked');

		// Layouts #3, #4, #9 - 24, #26, #28 from V3 are deprecated in MapsAlive V4.

		switch (layoutId)
		{
			// Layout #1
			case "HMMIT":
				this.setFlexDirection('column', 'row');
				this.setFlexGaps('top', 'right');
				this.setTextAreaHeight();
				break;

			// Layout #2
			case "HMMTI":
				this.setFlexDirection('column', 'row');
				this.setFlexGaps('top', 'left');
				this.setTextAreaScrollable();

				// Keep the image on the right so it won't move left toward the text.
				this.contentArea.style.justifyContent = 'space-between';
				break;

			// Layout #5
			case "VMMIT":
				this.setFlexDirection('row', 'column');
				this.setFlexGaps('left', 'bottom');
				this.setTextAreaScrollable();
				break;

			// Layout #6
			case "VMMTI":
				this.setFlexDirection('row', 'column');
				this.setFlexGaps('left', 'top');
				this.setTextAreaScrollable();
				break;

			// Layout #7
			case "VITMM":
				this.setFlexDirection('row-reverse', 'column');
				this.setFlexGaps('right', 'bottom');
				this.setTextAreaScrollable();
				break;

			// Layout #8
			case "VTIMM":
				this.setFlexDirection('row-reverse', 'column');
				this.setFlexGaps('right', 'top');
				this.setTextAreaScrollable();
				break;

			// Layout #25
			case "HMMII":
				this.setFlexDirection('column', 'row');
				this.setFlexGaps('top');
				this.setContentHeightForRow();
				break;

			// Layout #27
			case "HMMTT":
				this.setFlexDirection('column', 'row');
				this.setFlexGaps('top');
				this.setTextAreaHeight();;
				break;

			// Layout #29
			case "VMMTT":
				this.setFlexDirection('row', 'column');
				this.setFlexGaps('left');
				this.setContentHeightForColumn();
				break;

			// Layout #30
			case "VTTMM":
				this.setFlexDirection('row-reverse', 'column');
				this.setFlexGaps('right');
				this.setContentHeightForColumn();
				break;

			// Layout #31
			case "VMMII":
				this.setFlexDirection('row', 'column');
				this.setFlexGaps('left');
				this.setContentHeightForColumn();
				break;

			// Layout #32
			case "VIIMM":
				this.setFlexDirection('row-reverse', 'column');
				this.setFlexGaps('right');
				this.setContentHeightForColumn();
				break;

			// Layout #33
			case "HIITT":
				this.setFlexDirection('column', 'column');
				this.setFlexGaps('', 'bottom');
				this.setContentHeight(this.scaledInnerLayoutH);
				this.setOverflowForContentArea();
				break;

			// Layout #34
			case "HTTII":
				this.setFlexDirection('column', 'column');
				this.setFlexGaps('', 'top');
				this.setContentHeight(this.scaledInnerLayoutH);
				this.setOverflowForContentArea();
				break;

			// Layout #35, #36 (Data Sheets only)
			case "VIITT":
			case "VTTII":
				this.setFlexDirection('column', 'row');
				this.setFlexGaps('', layoutId === "VIITT" ? 'right' : 'left');

				// Set the media area and text area widths to match the splitter percentages for these two layouts.
				if (this.page.hasPopup)
				{
					let maxW = this.page.popup.maxW - this.page.popup.borderWidth * 2;
					let percent = Math.round((this.page.mediaAreaW / maxW) * 100);
					this.page.layout.mediaAreaElement.style.width = `${percent}%`;
					this.page.layout.textAreaElement.style.width = `${100 - percent}%`;
				}

				this.setTextAreaScrollable();
				break;

			// Layout #37
			case "HMM":
				this.setFlexDirection('column');
				break;

			// Layout #38
			case "HII":
				this.setFlexDirection('column');
				this.setContentHeightForColumn();
				break;

			// Layout #39
			case "HTT":
				this.setFlexDirection('column');
				this.setContentHeightForColumn();
				break;

			// Layout #40
			case "HMIT":
				this.setFlexDirection('column', 'column');
				this.setFlexGaps('top', 'bottom');
				this.layoutElement.classList.add('maStacked');
				break;

			// Layout #41
			case "HMTI":
				this.setFlexDirection('column', 'column');
				this.setFlexGaps('top', 'top');
				this.layoutElement.classList.add('maStacked');
				break;

			// Layouts for mobile devices. Make the panel visible when resizing the tour. When
			// resize is false, this method is being called when the page is first displayed. In
			// that case, the content panel needs to get configured, but not shown.
			case "R-PORTRAIT":
			case "R-LANDSCAPE":
				let visible = resize === true && this.map.markerIsSelected;
				this.configureMobileContentPanel(layoutId, visible)
				break;

			default:
				Runtime__$$.assert(false, `No case for layout '${layoutId}'`);
				break;
		}
	}

	configureMobileContentPanel(layoutId, visible)
	{
		this.setOverflowForContentArea();
		this.contentPanel.classList.add(this.MOBILE_PANEL);

		let appearance;

		switch (layoutId)
		{
			case "R-PORTRAIT":
				appearance = this.page.isDataSheet ? this.MOBILE_PORTRAIT_FULL : this.MOBILE_PORTRAIT_PARTIAL;
				break;

			case "R-LANDSCAPE":
				appearance = this.page.isDataSheet ? this.MOBILE_LANDSCAPE_FULL : this.MOBILE_LANDSCAPE_PARTIAL;
				break;
		}

		// Set the visibility class.
		this.contentPanelAppearance = this.MOBILE_NONE;

		this.changeContentPanelAppearance(appearance, visible);
	}

	get containerOrientationIsLandscape()
	{
		return !this.containerOrientationIsPortrait;
	}

	get containerOrientationIsPortrait()
	{
		return this.containerSize.w <= this.containerSize.h;
	}

	convertToResponsivePx(value, minValue)
	{
		let newValue = this.convertToResponsiveValue(value);
		if (newValue < minValue)
			newValue = minValue;
		return newValue + 'px'
	}

	convertToResponsiveValue(value, round = true)
	{
		let scaledValue = this.responsiveTourScale * value;
		return round ? Math.round(scaledValue) : scaledValue;
	}

	createCss(id, css)
	{
		let elementId = this.tour.uniqueId(id);

		// Remove the previously created style element if it exists.
		let styleElement = document.getElementById(elementId);
		if (styleElement)
			styleElement.remove();

		// Create a new style element and insert it into the DOM.
		styleElement = document.createElement('style');
		styleElement.id = elementId;

		styleElement.innerHTML = this.cleanCss(css);
		this.tour.insertElementIntoDomAfterScriptElementforTour(styleElement);
	}

	createCssForContentPanel(imageSize, resize = false)
	{
		if (resize)
		{
			// Update the styles for the current image to fit the resized container.
			imageSize = this.currentImageSize;
			if (imageSize === null)
				return;
		}
		else
		{
			this.currentImageSize = imageSize;
		}

		//console.log(`Layout::createCssForContentPanel ${imageSize.w}x${imageSize.h}`);

		let contentPanelId = this.tour.uniqueId('ContentPanel');
		let contentPanelTabId = this.tour.uniqueId('ContentPanelTab');
		let contentAreaId = this.tour.uniqueId('ContentArea');
		let mediaAreaId = this.tour.uniqueId('HotspotMediaArea');
		let textAreaId = this.tour.uniqueId('HotspotTextArea');
		let hotspotTitleId = this.tour.uniqueId('HotspotTitle');
		let boxShadowForTab = '#ccccccbc 0px 0px 6px';
		let boxShadowForPanelEdges = '#aaa 0px 2px 6px';
		let borderRadiusForTab = '4px 4px 0 0';
		let contentPanelTransition = 'all 0.35s ease-out';
		let css = '';

		// Calculate the sizes of the full and partial panel sizes for both portrait and landscape.
		let sizes = this.createMobilePanelSizes(imageSize);
		this.chooseContentPanelExpander();

		// Create the CSS for all combinations of mobile layouts: for hidden, full portrait,
		// partial portrait, full landscape, and partial landscape. The CSS is specific to each
		// image and thus needs to be destroyed and recreated every time an image displays.

		// ContentPanel CSS.
		css += `
		#${contentPanelId}.${this.MOBILE_PANEL} {
			position:absolute;
			opacity:1.0;
			transition:${contentPanelTransition};
			z-index:1;
			background-color:rgba(128,128,128,0.65);
			box-shadow:${boxShadowForPanelEdges};
		}

		#${contentPanelId}.${this.MOBILE_PORTRAIT_PARTIAL} {
			width:${sizes.portraitPartial.w}px;
			height:${sizes.portraitPartial.h}px; 
			top:${sizes.portraitPartial.top}px;
			left:${sizes.portraitPartial.left}px;
			box-shadow: 0px 2px 8px 0px #ccc;
		}

		#${contentPanelId}.${this.MOBILE_PORTRAIT_FULL} {
			width:${sizes.portraitFull.w}px;
			height:${sizes.portraitFull.h}px; 
			top:${sizes.portraitFull.top}px;
			left:${sizes.portraitFull.left}px;
			box-shadow: 0px 2px 8px 0px #ccc;
		}

		#${contentPanelId}.${this.MOBILE_LANDSCAPE_PARTIAL} {
			width:${sizes.landscapePartial.w}px;
			height:${sizes.landscapePartial.h}px; 
			top:${sizes.landscapePartial.top}px;
			left:${sizes.landscapePartial.left}px;
		}
		
		#${ contentPanelId }.${ this.MOBILE_LANDSCAPE_FULL } {
			width: ${ sizes.landscapeFull.w } px;
			height: ${ sizes.landscapeFull.h } px;
			top: ${ sizes.landscapeFull.top } px;
			left: ${ sizes.landscapeFull.left } px;
		}
		`;

		// ContentPanelTab CSS.
		css += `
		.${this.MOBILE_PANEL} #${contentPanelTabId} {
			display:none;
		}

		.${this.MOBILE_PANEL} #${contentPanelTabId} {
			display:flex;
			flex-direction:row;
			align-items:center;
			visibility:visible;
			opacity:1.0;
			border-bottom:solid 1px #ccc;
			box-shadow:${boxShadowForTab};
			border-radius:${borderRadiusForTab};
			transition:${contentPanelTransition};
		}

		.${this.MOBILE_HIDDEN} #${contentPanelTabId} {
			visibility:hidden;
			opacity:0;
		}
		`;

		// CSS for a mobile datasheet appears below and overrides non-datasheet CSS.
		if (this.page.isDataSheet)
		{
			css += `
			.${this.MOBILE_PORTRAIT_FULL} #${contentPanelTabId},
			.${this.MOBILE_LANDSCAPE_FULL} #${contentPanelTabId} {
				display:none;
			}
			`;
		}

		// ContentArea CSS
		css += `
		.${this.MOBILE_PANEL} #${contentAreaId} {
			flex-direction:column;
			background-color:#fff;
		}

		.${this.MOBILE_LANDSCAPE_PARTIAL} #${contentAreaId},
		.${this.MOBILE_LANDSCAPE_FULL} #${contentAreaId} {
			border-bottom: solid 2px #777;
		}

		.${this.MOBILE_LANDSCAPE_FULL} #${contentAreaId} {
			flex-direction:row;
		}
		`;

		// CSS for a mobile datasheet appears below and overrides non-datasheet CSS.
		if (this.page.isDataSheet)
		{
			css += `
			.${this.MOBILE_LANDSCAPE_FULL} #${contentAreaId} {
				box-shadow:none;
				border-right:solid 1px #ccc;
			}
			`;
		}

		// HotspotImage CSS.
		css += `
		.${this.MOBILE_PANEL} #${mediaAreaId} img {
			transition:${contentPanelTransition};
		}

		.${this.MOBILE_PORTRAIT_FULL} #${mediaAreaId} img {
			width:${sizes.portraitFull.imageSize.w}px;
			height:${sizes.portraitFull.imageSize.h}px; 
		}

		.${this.MOBILE_PORTRAIT_PARTIAL} #${mediaAreaId} img {
			width:${sizes.portraitPartial.imageSize.w}px;
			height:${sizes.portraitPartial.imageSize.h}px;
		}

		.${this.MOBILE_LANDSCAPE_FULL} #${mediaAreaId} img {
			width:${sizes.landscapeFull.imageSize.w}px;
			height:${sizes.landscapeFull.imageSize.h}px; 
		}

		.${this.MOBILE_LANDSCAPE_PARTIAL} #${mediaAreaId} img {
			width:${sizes.landscapePartial.imageSize.w}px;
			height:${sizes.landscapePartial.imageSize.h}px; 
		}
		`;

		// HotspotTextArea CSS
		css += `
		#${textAreaId} {
			padding:6px 6px 12px 6px;
			background-color:${this.tour.hotspotTextBackgroundColor};
		}

		.${this.MOBILE_LANDSCAPE_FULL} #${textAreaId} {
			overflow:auto;
		}

		#${hotspotTitleId} {
			display:none !important;
		}
		`;

		// CSS for a hidden appears below and overrides CSS for visible panels.
		css += `
		#${contentPanelId}.${this.MOBILE_HIDDEN} {
			opacity:0;
			height: 0;
		}

		#${contentPanelId}.${this.MOBILE_HIDDEN}.${this.MOBILE_PORTRAIT_FULL},
		#${contentPanelId}.${this.MOBILE_HIDDEN}.${this.MOBILE_PORTRAIT_PARTIAL} {
			top:${sizes.portraitContainer.h}px;
			left:0px;
			width:${sizes.portraitPartial.w}px;
		}

		#${contentPanelId}.${this.MOBILE_HIDDEN}.${this.MOBILE_LANDSCAPE_FULL} {
			top:${sizes.landscapeContainer.h}px;
			left:${sizes.landscapeFull.left}px;
			width:${sizes.landscapePartial.w}px;
		}

		#${contentPanelId}.${this.MOBILE_HIDDEN}.${this.MOBILE_LANDSCAPE_PARTIAL} {
			top:${sizes.landscapeContainer.h}px;
			left:${sizes.landscapePartial.left}px;
		}
		`;

		this.createCss('ContentStyle', css);
	}

	createCssForLayout()
	{
		// Create the page-specific CSS.
		let css = '';

		if (this.page.hasPopup)
		{
			let bs = this.popup.boxShadow;
			let boxShadow = bs > 0 ? `${bs}px ${bs}px  10px rgba(0,0,0,0.3)` : 'unset';

			css += `
			#${this.tour.uniqueId('PopupPanel')} {
				background-color:${this.popup.backgroundColor};
				border:solid ${this.popup.borderWidth}px;
				border-color:${this.popup.borderColor} ${this.popup.borderColor} ${this.popup.borderColor} ${this.popup.borderColor};
				border-radius:${this.popup.borderRadius}px;
				box-shadow:${boxShadow};
			}

			#${this.tour.uniqueId('PopupImage')} {
				border-radius:${this.popup.imageCornerRadius}px;
			}

			#${this.tour.uniqueId('PopupTitle')} {
				color:${this.popup.titleColor};
				background-color:${this.popup.backgroundColor};
			}

			#${this.tour.uniqueId('PopupText')} {
				color:${this.popup.textColor};
				background-color:${this.popup.backgroundColor};
			}
			`;
		}
		else
		{
			css += `
			#${this.tour.uniqueId('HotspotTitle')} {
				color:${this.tour.hotspotTitleColor};
				background-color:${this.tour.hotspotTitleBackgroundColor};
			}

			#${this.tour.uniqueId('HotspotText')} {
				color:${this.tour.hotspotTextColor};
				background-color:${this.tour.hotspotTextBackgroundColor};
			}
			`;
		}

		css += `
		#${this.tour.uniqueId('ContentPanelTab')} {
			display:none;
		}
		`
		this.createCss('PageStyle', css);
	}

	createCssForNavPanel(left, top, alignment)
	{
		let navPanelId = this.tour.uniqueId('NavPanel');
		let directoryPanelId = this.tour.uniqueId('DirPanel');

		// Choose how the menu panel will close based on how it is aligned in the tour.
		// For left alignment, the panel opens and its left edge and widens to the right.
		// For right alignment, it opens at its right edge and widens to the left. For
		// center alignment, it opens halfway between its left and right edges and widens
		// in both directions. Set the left property of the closed menu panel to make the
		// transition from closed to open and vice versa work properly..
		let closedLeft = left;
		if (alignment === 'R')
			closedLeft += this.navPanelSize.w;
		else if (alignment === 'C')
			closedLeft += Math.round(this.navPanelSize.w / 2);

		let css = '';
		css += `
		#${navPanelId} {
			left: ${closedLeft}px;
			top: ${top}px;
			max-height: ${this.navPanelMaxHeight}px;
		}

		#${navPanelId}.show {
			left: ${left}px;
			width: ${this.navPanelSize.w}px;
		}

		#${navPanelId} .maMenuItem {
			font-size: ${this.usingMobileLayout ? 16 : 14}px;
		}

		#${directoryPanelId} {
		}

		#${directoryPanelId} .maDirLevel1 {
			font-size: ${this.usingMobileLayout ? 16 : 14}px;
		}

		#${directoryPanelId} .maDirLevelCount {
			font-size: ${this.usingMobileLayout ? 12 : 10}px;
		}

		#${directoryPanelId} .maDirEntry {
			font-size: ${this.usingMobileLayout ? 16 : 12}px;
		}
		`
		this.createCss('MenuStyle', css);
	}

	createElementsForContentPanel()
	{
        this.contentPanel = document.createElement("div");
        this.contentPanel.id = this.tour.uniqueId('ContentPanel');
        this.contentPanel.style.display = 'flex';
		this.contentPanel.style.flexDirection = 'column';
        this.layoutElement.appendChild(this.contentPanel);

        this.createElementsForContentPanelTab();

        this.contentArea = document.createElement("div");
        this.contentArea.id = this.tour.uniqueId('ContentArea');
		this.contentArea.className = 'maContentArea';
		this.contentArea.style.display = 'flex';
		this.contentArea.style.backgroundColor = this.tourElement.style.backgroundColor;
        this.contentPanel.appendChild(this.contentArea);
    }

	createElementsForContentPanelTab()
	{
		if (!this.usingMobileLayout)
		{
			this.contentPanelTab = null;
			return;
		}

		this.contentPanelTab = document.createElement("div");
        this.contentPanelTab.id = this.tour.uniqueId('ContentPanelTab');
        const contentPanelTabTopBottomBorder = 2;
        this.contentPanelTab.style.minHeight = this.contentPanelTabHeight - contentPanelTabTopBottomBorder + 'px';
        this.contentPanelTab.style.backgroundColor = '#fff';
        this.contentPanelTab.style.padding = '0 12px';
        this.contentPanel.appendChild(this.contentPanelTab);

        this.contentPanelExpander = document.createElement("img");
        this.contentPanelExpander.id = this.tour.uniqueId('ContentPanelExpander');
        this.contentPanelExpander.src = this.tour.graphics['contentExpand'].src;
        this.contentPanelExpander.style.width = '20px';
        this.contentPanelExpander.style.height = '20px';
        this.contentPanelExpander.style.cursor = 'pointer';
        this.contentPanelTab.appendChild(this.contentPanelExpander);

        this.contentPanelTitle = document.createElement("div");
        this.contentPanelTitle.id = this.tour.uniqueId('ContentPanelTitle');
        this.contentPanelTitle.style.color = '#000';
        this.contentPanelTitle.style.fontFamily = 'Arial, Helvetica, Verdana, Sans-Serif';
        this.contentPanelTitle.style.fontWeight = 'bold';
        this.contentPanelTitle.style.fontSize = '14px';
        this.contentPanelTitle.style.flexGrow = 1;
        this.contentPanelTitle.style.whiteSpace = 'nowrap';
        this.contentPanelTitle.style.overflow = 'hidden';
        this.contentPanelTitle.style.textOverflow = 'ellipsis';
        this.contentPanelTitle.style.textAlign = 'center';
        this.contentPanelTitle.style.paddingLeft = '4px';
        this.contentPanelTab.appendChild(this.contentPanelTitle);

        this.contentPanelCloseX = document.createElement("img");
        this.contentPanelCloseX.id = this.tour.uniqueId('ContentPanelCloseX');
		this.contentPanelCloseX.src = this.tour.graphics['mobileCloseX'].src;
        this.contentPanelCloseX.style.width = '24px';
        this.contentPanelCloseX.style.height = '24px';
        this.contentPanelCloseX.style.cursor = 'pointer';
        this.contentPanelTab.appendChild(this.contentPanelCloseX);

		this.contentPanelTab.addEventListener('click', this.onClickContentPanelTab);
		this.contentPanelCloseX.addEventListener('click', this.onClickContentPanelCloseX);
    }

	createElementsForLayout()
	{
		//console.log(`Layout::createElementsForLayout`);

		// Disable popups when on a small mobile device..
		if (this.page.hasPopup && this.usingMobileLayout)
			this.page.hasPopup = false;

		this.popupElement = null;

		// Get or create the map element depending on whether or not its being used for the Map Editor.
		if (this.tour.editMode)
		{
			// The element already exists because it gets created by the Map.aspx page.
			this.mapElement = document.getElementById('maMap');

			this.createElementsForMap();
			return;
		}

		// Dynamically create a style sheet for page-specific styles.
		this.createCssForLayout();

		this.setLayoutStyles();
		this.addMapToLayout();
		this.createElementsForMap();

		if (this.page.hasPopup)
			this.createElementsForPopupLayout();
		else
			this.createElementsForTiledLayout();
	}

	createElementsForMap() 
	{
		if (this.page.isDataSheet)
			return;

		// The map layer is on the bottom.
		this.mapElement.appendChild(this.map.mapLayer);

		// The marker layer lies on top of the map
		this.mapElement.appendChild(this.map.markerLayer);

		// The edit and handles layers are above the marker layer. The handles layer is
		// above the edit layer so that handles will be drawn on top of the cross hairs.
		if (this.tour.editMode)
		{
			this.mapElement.appendChild(this.map.editLayer);
			this.mapElement.appendChild(this.map.handlesLayer);
		}

		if (this.map.mapInsetEnabled && this.map.mapZoomEnabled)
			this.mapElement.appendChild(this.map.insetLayer);

		// Add the zoom control buttons after all the canvas layers.
		if (this.map.mapZoomEnabled)
		{
			this.mapElement.appendChild(this.map.zoomInControl);
			this.mapElement.appendChild(this.map.zoomOutControl);
		}

		// The status area is on top of all the layers. It's sole purpose is to
		// show the loading message, but will get deleted once the map has loaded.
		this.mapElement.appendChild(this.map.statusArea);

		this.map.createTooltipElement();
	}
	
	createElementsForMenu(pageNumber = 0)
	{
		for (const page of this.tour.pages)
		{
			if (page.showHelp)
				this.helpPanelIsUsed = true;
		}

		for (const page of this.tour.pages)
		{
			if (page.excludeFromNavigation)
				continue;

			// When only creating a menu entry for a specific page, ignore the other pages.
			if (pageNumber !== 0 && page.pageNumber !== pageNumber)
				continue;

			let isCurrentPage = page.pageNumber === this.page.pageNumber;

			let menuItem = document.createElement("div");
			menuItem.className = 'maMenuItem';
			if (isCurrentPage)
				menuItem.classList.add('maSelected');
			this.menuPanel.appendChild(menuItem);

			let menuItemRow = document.createElement("div");
			menuItemRow.className = 'maMenuItemRow';

			let menuItemText = document.createElement("div");
			menuItemText.className = 'maMenuItemText';
			let pageName = page.pageTitle.length > 0 ? page.pageTitle : page.pageName;
			menuItemText.innerHTML = pageName;

			menuItemText.dataset.pageNumber = page.pageNumber;
			menuItemText.addEventListener('click', this.onClickMenuItem, false);

			if (this.helpPanelIsUsed)
			{
				let menuItemHelp = document.createElement("div");
				menuItemHelp.style.width = "24px";
				menuItemHelp.style.height = "16px";
				if (page.showHelp && (page.help.title || page.help.text))
				{
					let helpIcon = document.createElement("img");
					helpIcon.className = "maHelpButton"
					helpIcon.src = this.tour.graphics['helpButton'].src;
					helpIcon.alt = "Help button";
					menuItemHelp.appendChild(helpIcon);

					// Assign the page's help properties to the element's dataset
					// so they will be availble when the user taps the help icon.
					menuItemHelp.dataset.helpTitle = page.help.title;
					menuItemHelp.dataset.helpText = page.help.text;
					menuItemHelp.dataset.helpWidth = page.help.width;
					menuItemHelp.dataset.helpBackgroundColor = page.help.bgColor;
					menuItemHelp.dataset.helpColor = page.help.color;
					menuItemHelp.dataset.helpPageNumber = page.pageNumber;
					menuItemHelp.addEventListener('click', this.onClickHelpButton, false);
				}
				menuItemRow.appendChild(menuItemHelp);
			}

			menuItemRow.appendChild(menuItemText);
			menuItem.appendChild(menuItemRow);
		}
	}

	createElementsForNavigation(resize)
	{
		if (this.tour.editMode)
			return;

		if (!this.tour.hasNavPanel)
			return;

		if (resize)
			return;

		this.createElementsForNavButton();
		this.createElementsForNavPanel();
	}

	createElementsForNavButton()
	{
		this.navButton = document.createElement('img');
		this.navButton.id = this.tour.uniqueId('NavButton');
		this.navButton.className = "maNavButton";
		this.navButton.alt = "Navigation button";
        this.navButton.src = this.tour.graphics['navButton'].src;
        this.navButton.style.position = 'absolute';
        this.navButton.style.width = '22px';
		this.navButton.style.height = '22px';

        // Append the menu button to its containing elmement.
		if (this.tour.navButtonLocation === this.NAV_TITLE_BAR && this.titleBarElement !== null)
			this.navButtonParent = this.titleBarElement;
		else if ((this.tour.navButtonLocation === this.NAV_MAP_LEFT || this.tour.navButtonLocation === this.NAV_MAP_RIGHT) && this.mapElement !== null)
			this.navButtonParent = this.mapElement;
		else
			this.navButtonParent = this.tourElement;
		this.navButtonParent.appendChild(this.navButton);

		// When the menu button is hidden, make it invisible, but the button element must still
		// be created because its position is used to determine where the menu panel drops down.
		if (this.tour.navButtonHidden)
		{
			this.navButton.style.visibility = 'hidden';
		}
		else
		{
			this.navButton.style.cursor = 'pointer';
			this.navButton.style.backgroundColor = '#fff';
			this.navButton.style.border = 'solid 1px #000';
	        this.navButton.addEventListener('click', this.onClickNavButton);
		}
	}

	createElementsForNavPanel()
	{
		if (this.navPanel !== null)
			this.tourElement.removeChild(this.navPanel);

		// If the panel already exists, remove it and create it again. This happens when
		// user JavaScript calls api.setHotspotTitle which, in turn, invalidates the panel
		// contents and causes it to get removed.
		let navPanelId = this.tour.uniqueId('NavPanel');
		let navPanel = document.getElementById(navPanelId);
		if (navPanel)
			navPanel.remove();

		this.navPanel = document.createElement('nav');
		this.navPanel.id = navPanelId;

		this.tourElement.appendChild(this.navPanel);

		if (!this.tour.hasNavPanel)
			return;

		// Create the menu if the tour is showing its menu or if it has a directory.
		if (!this.tour.hideMenu || this.tour.hasDirectory)
		{
			this.menuPanel = document.createElement("div");
			this.menuPanel.className = 'maMenuPanel';
			this.navPanel.appendChild(this.menuPanel);

			// Handle the case where the menu is hidden, but the directory is showing,
			// by emitting a single menu item for the current page.
			let pageNumber = 0;
			if (this.tour.hideMenu && this.tour.hasDirectory)
				pageNumber = this.page.pageNumber;
			this.createElementsForMenu(pageNumber);
		}

		this.calculateNavPanelSize();

		// Create the directory.
		if (this.tour.hasDirectory)
			this.tour.directory.createDirectoryPanel();
	}

	createElementsForPopupArrow(position, zIndex)
	{
		let xmlns = "http://www.w3.org/2000/svg";

		// Create an SVG element to hold the SVG polyline. Its size and
		// location get set dynamically each time a popup is displayed.
		this.popupArrow = document.createElementNS(xmlns, "svg");
		this.popupArrow.setAttribute('id', this.tour.uniqueId('PopupArrow'));
		this.popupArrow.setAttribute('class', 'maPopupArrow');

		// Create an SVG polyline element that will be used to draw the arrow.
		this.popupArrowPolyline = document.createElementNS(xmlns, "polyline");
		this.popupArrowPolyline.setAttribute('id', this.tour.uniqueId('PopupArrowLine'));

		// Create a gradient for the polyline so that the arrow fades away from the popup.
		this.createGradientForPopupArrow(xmlns);
		this.popupArrowPolyline.setAttribute('fill', `url(#${this.tour.uniqueId('ArrowGradient')}`);

		// Append the polyline to the arrow and append the arrow to the popup.
		this.popupArrow.appendChild(this.popupArrowPolyline);
		this.popupElement.appendChild(this.popupArrow);

		// Style the arrow.
		let style = this.popupArrow.style;
		style.position = position;
		style.visibility = 'hidden';
		style.zIndex = zIndex + 1;
		style.position = position;
		style.top = '0px';
		style.left = '0px';
		style.stroke = this.popup.borderColor;
		style['stroke-width'] = this.popup.getArrowLineWidth();
		style['stroke-linejoin'] = 'round';
		style['stroke-opacity'] = 1.0;
	}

	createElementsForPopupCloseX(position, zIndex)
	{
		// The outside of the graphic is large to make it easy to touch.
		// The inside, that is the actual X, is much smaller.
		this.popupCloseXSizeOuter = 44;
		this.popupCloseXSizeInner = 18;

		let e = document.createElement("img");
		e.id = this.tour.uniqueId('PopupCloseX');
		e.className = 'maPopupCloseX';
		e.onclick = this.popup.onClickPopupX;
		e.style.display = "none";
		e.src = this.tour.graphics["popupCloseX"].src;
		e.style.width = this.popupCloseXSizeOuter + "px";
		e.style.height = this.popupCloseXSizeOuter + "px";
		e.style.zIndex = zIndex + 2;
		e.style.position = position;
		e.style.cursor = "pointer";
		this.popupCloseX = e;
		this.popupElement.appendChild(this.popupCloseX);
	}

	createElementsForPopupContent()
	{
		switch (this.page.layoutId)
		{
			case "HIITT":
				this.createElementsForPopupContentTitle();
				this.createElementsForPopupContentMedia();
				this.createElementsForPopupContentText();
				break;

			case "HTTII":
				this.createElementsForPopupContentTitle();
				this.createElementsForPopupContentText();
				this.createElementsForPopupContentMedia();
				break;

			case "HII":
				this.createElementsForPopupContentTitle();
				this.createElementsForPopupContentMedia();
				break;

			case "HTT":
				this.createElementsForPopupContentTitle();
				this.createElementsForPopupContentText();
				break;

			default:
				Runtime__$$.assert(false, `No case for popup layout '${this.page.layoutId}'`);
				break;
		}
	}

	createElementsForPopupContentMedia()
	{
        this.popupElements.mediaArea = document.createElement("div");
        this.popupElements.mediaArea.id = this.tour.uniqueId('PopupMediaArea');
        this.popupElements.mediaArea.className = 'maPopupMediaArea';
        this.popupPanel.appendChild(this.popupElements.mediaArea);
    }

	createElementsForPopupContentText()
	{
        this.popupElements.text = document.createElement("div");
        this.popupElements.text.id = this.tour.uniqueId('PopupText');
        this.popupElements.text.className = 'maPopupText';
        this.popupPanel.appendChild(this.popupElements.text);
    }

	createElementsForPopupContentTitle()
	{
		if (!this.page.showViewTitle)
			return;

		this.popupElements.title = document.createElement("div");
        this.popupElements.title.id = this.tour.uniqueId('PopupTitle');
        this.popupElements.title.className = 'maPopupTitle';
		this.popupPanel.appendChild(this.popupElements.title);

		// Determine if the title needs some left and/or top padding. If the user has CSS for either,
		// don't override it, otherwise, adjust the left/and or top to ensure the minimum padding.
		const minPaddingLeft = 4;
		const minPaddingTop = 4;

		// Get the actual styling. If there's no user CSS, left and top will be 0.
		let titleStyle = getComputedStyle(this.popupElements.title);
		let userPaddingLeft = parseInt(titleStyle.paddingLeft, 10);
		let userPaddingTop = parseInt(titleStyle.paddingTop, 10);

		// Make the adjustments based on how much "padding" is already provided by the layout margins.
		if (this.page.layoutMarginLeft < minPaddingLeft && userPaddingLeft === 0)
			this.popupElements.title.style.paddingLeft = Math.max(this.page.layoutMarginLeft, minPaddingLeft - this.page.layoutMarginLeft) + 'px';
		if (this.page.layoutMarginTop < minPaddingTop && userPaddingTop === 0)
			this.popupElements.title.style.paddingTop = Math.max(this.page.layoutMarginTop, minPaddingTop - this.page.layoutMarginTop) + 'px';
    }

	createElementsForPopupLayout()
	{
		// Make a popup appear over the directory and help. The directory z-index is 4000. Help is 5000.
		let zIndex = 6000;

		// Position popups fixed relative to the upper left corner of the viewport.
		let position = 'fixed';

		this.createElementsForPopupPanel(position, zIndex);
		this.createElementsForPopupArrow(position, zIndex);
		this.createElementsForPopupCloseX(position, zIndex);
		this.createElementsForPopupContent();

		this.setPaddingStyleEm(this.popupPanel, this.page.layoutMarginTop, this.page.layoutMarginRight, this.page.layoutMarginBottom, this.page.layoutMarginLeft);

		// Set the popup panel to use box sizing so that getting and setting the panel's
		// width and height will include the panel's border and margins.
		this.popupPanel.style.boxSizing = 'border-box';

		// Create elements needed for a tiled layout to be used if the tour size gets smaller
		// and the responsive logic switches from a popup layout to a tiled layout. Initially
		// these element will be hidden and only shown if the switch occurs.
		this.createElementsForTiledLayout(true);
	}

	createElementsForPopupPanel(position, zIndex)
	{
		// Create the popup container within which all of the popup parts will be contained.
		// Insert it into the DOM as the first child of the tour element so that it will
		// move as the tour moves if the browser window is resized.
		this.popupElement = document.createElement("div");
		this.popupElement.id = this.tour.uniqueId('Popup');
		this.popupElement.className = 'maPopup';
		this.tourElement.insertBefore(this.popupElement, this.tourElement.firstChild);

		// Create the popup panel.
		let e = document.createElement("div");
		e.id = this.tour.uniqueId('PopupPanel');
		e.className = 'maPopupPanel';
		e.style.visibility = "hidden";
		e.style.zIndex = zIndex;
		e.style.position = position;
		e.style.display = 'flex';
		e.style.flexDirection = 'column';
		e.style.justifyContent = 'start';

		this.popupPanelWidth = this.popup.maxW;

		e.style.top = "0px";
		e.style.left = "0px";

		this.popupPanel = e;
		this.popupElement.appendChild(this.popupPanel);
	}

	createElementsForTiledLayout(primaryLayoutIsPopup = false)
	{
		//console.log(`Layout::createElementsForTiledLayout`);

		this.layoutElement.style.display = "flex";

		let hasMap = this.page.layoutId.indexOf('M') !== -1;
		let hasMedia = this.page.layoutId.indexOf('I') !== -1;
		let hasText = this.page.layoutId.indexOf('T') !== -1;

		if (hasMap && !primaryLayoutIsPopup)
		{
			this.mapElement.style.overflow = "hidden";
			this.mapElement.style.flexShrink = 0;
		}

		this.createElementsForContentPanel();

		if (hasMedia)
		{
			this.tiledElements.mediaArea = document.createElement("div");
			this.tiledElements.mediaArea.id = this.tour.uniqueId('HotspotMediaArea');
			this.tiledElements.mediaArea.className = 'maHotspotMediaArea';

			// Prevent the image from getting squished when it's side-by-side with a text area. The
			// problem only happens sometimes which makes it seem like a flex bug, but this prevents it.
			this.tiledElements.mediaArea.style.flexShrink = 0;

			// Prevent a horizontal scrollbar from ever apearing in the media area. One should only
			// appear in a responsive stacked layout where there's a lot of content and a tall image.
			// In that case, a vertical scoller appears for the content leaving less width for the image
			// which causes a horizontal scoller to appear. In that case, clip the image on the right.

			// This solution is disabled because it creates some kind of conflict with Flex whereby the
			// HotspotMediaArea width becomes fractional and then displays very narrow. At the moment,
			// the phenomenon is not occurring, so perhaps this comment and code can be removed.
			// this.tiledElements.mediaArea.style.overflowX = 'clip';
		}

		if (hasText)
		{
			this.tiledElements.textArea = document.createElement("div");
			this.tiledElements.textArea.id = this.tour.uniqueId('HotspotTextArea');
			this.tiledElements.textArea.className = 'maHotspotTextArea';

			this.tiledElements.title = document.createElement("div");
			this.tiledElements.title.id = this.tour.uniqueId('HotspotTitle');
			this.tiledElements.title.className = 'maHotspotTitle';
			this.tiledElements.textArea.appendChild(this.tiledElements.title);

			this.tiledElements.text = document.createElement("div");
			this.tiledElements.text.id = this.tour.uniqueId('HotspotText');
			this.tiledElements.text.className = 'maHotspotText';
			this.tiledElements.textArea.appendChild(this.tiledElements.text);
		}

		// Emit the order of the media and text area elements based on their position in the layout's
		// content area (image above vs below text, or image left of or to the right of text.) Though
		// flexbox provides the column-reverse and row-reverse directions, using those to set the
		// order versus doing it here has an affect on how scroll bars work when the combined image
		// and text height are too tall in layouts where one is over the other. When using flex to
		// reverse the direction, the vertical scroll bar initially appears scrolled all the way down
		// rather than all the way up which means you have to scroll up to see the top of the content.
		// It's as though using reverse turns everything upside down including the scoll bar. The
		// problem is avoided by setting the correct order here and avoiding use of flex reverse for
		// the content area (it's okay to use it for the layout area because it doesn't use scroll bars.
		if (this.page.layoutId.includes('IT'))
		{
			this.appendMediaAreaToContentArea();
			this.appendTextAreaToContentArea();
		}
		else if (this.page.layoutId.includes('TI'))
		{
			this.appendTextAreaToContentArea();
			this.appendMediaAreaToContentArea();
		}
		else
		{
			// These layouts have text or an image (or neither), but not both.
			if (hasMedia)
				this.appendMediaAreaToContentArea();
			if (hasText)
				this.appendTextAreaToContentArea();
		}
	}

	createGrabberBar()
	{
		this.grabberBar = document.createElement('div');
		this.grabberBar.id = this.tour.uniqueId('Grabber');
		this.mapElement.appendChild(this.grabberBar);
	}

	createGradientForPopupArrow(xlmns)
	{
		// Create an SVG defs element and append it to the popup arrow.
		let defsElement = document.createElementNS(xlmns, 'defs');
		this.popupArrow.appendChild(defsElement);

		// Create an SVG linear gradient element and append it to the defs element.
		this.popupArrowGradient = document.createElementNS(xlmns, 'linearGradient');
		this.popupArrowGradient.id = this.tour.uniqueId('ArrowGradient');
		defsElement.appendChild(this.popupArrowGradient);

		// Set the fill attribute of the popup arrow polyline to use the gradient.
		this.popupArrowPolyline.setAttribute('fill', `url(#${this.popupArrowGradient.id})`);

		// Get the popup panel's background color instead of using this.popup.backgroundColor
		// because the user may have changed the color and/or its opacity, using CSS. This
		// way the arrow color and opacity will match the background.
		let popupPanelStyle = getComputedStyle(this.tour.getElementByUniqueId("PopupPanel"));
		let backgroundColor = popupPanelStyle.backgroundColor;

		// Create an array of stop data that will define the color, opacity, and
		// position to use on the gradient.
		let stops = [
			{
				"color": backgroundColor,
				"offset": "0%"
			},
			{
				"color": '#ffffff',
				'opacity': '30%',
				"offset": "100%"
			}
		];

		// Create stop elements from the stop data and append them to the gradient.
		for (let stop of stops)
		{
			let stopElement = document.createElementNS(xlmns, 'stop');
			stopElement.setAttribute('offset', stop.offset);
			stopElement.setAttribute('stop-color', stop.color);
			stopElement.setAttribute('stop-opacity', stop.opacity);
			this.popupArrowGradient.appendChild(stopElement);
		}
	}

	createHelpPanel()
	{
		let helpPanel = document.createElement("div");
		helpPanel.id = this.tour.uniqueId("HelpPanel");
		helpPanel.class = 'maHelpPanel';
		helpPanel.style.visibility = "hidden";
		helpPanel.style.position = "absolute";
		helpPanel.style.left = 0;
		helpPanel.style.maxHeight = "100%";
		helpPanel.style.overflow = "auto";

		this.page.layout.tourElement.appendChild(helpPanel);

		let titleBar = document.createElement("div");
		titleBar.style.display = "flex";
		titleBar.style.justifyContent = "space-between";
		titleBar.style.alignItems = "center";
		helpPanel.appendChild(titleBar);

		let title = document.createElement("div");
		title.id = this.tour.uniqueId('HelpTitle');
		title.className = 'maHelpTitle';
		title.style.paddingTop = "4px";
		titleBar.appendChild(title);

		let closeX = document.createElement("img");
		closeX.id = this.tour.uniqueId('HelpCloseX');
		closeX.alt = "Close help button";
		closeX.src = this.tour.graphics["mobileCloseX"].src;
		closeX.style.width = "20px";
		closeX.style.height = "20px";
		closeX.style.cursor = "pointer";
		closeX.addEventListener("click", this.onClickHelpPanelCloseX, false);
		titleBar.appendChild(closeX);

		let content = document.createElement("div");
		content.id = this.tour.uniqueId('HelpContent');
		content.className = 'maHelpContent';
		helpPanel.appendChild(content);

		return helpPanel;
	}

	createMobilePanelSizes(imageSize) 
	{
		let portraitContainer = Runtime__$$.createSizeObject();
		let landscapeContainer = Runtime__$$.createSizeObject();

		// Create a portrait and landscape version of the container so that the size logic
		/// doesn't need to be concerned with what the device's current orientation is.
		let isPortraitOrientation = this.containerOrientationIsPortrait;
		if (isPortraitOrientation)
		{
			portraitContainer.w = this.containerSize.w;
			portraitContainer.h = this.containerSize.h;
			landscapeContainer.w = this.containerSize.h;
			landscapeContainer.h = this.containerSize.w;
		}
		else
		{
			portraitContainer.w = this.containerSize.h;
			portraitContainer.h = this.containerSize.w;
			landscapeContainer.w = this.containerSize.w;
			landscapeContainer.h = this.containerSize.h;
		}

		// Determine if there's any text and how much vertical space it needs.
		let hasText = this.tiledElements.text !== null && this.tiledElements.text.innerHTML.length > 0;
		let textAreaRect = { width: 0, height: 0 };
		if (hasText)
		{
			// Make sure the text area is displaying, otherwise the bounding rect height will be zero.
			this.tiledElements.textArea.style.display = 'block';

			// Get the size of the text area displayed at the width of the device in portrait orientation
			// which will usually mean about half the width in landscape orientation. This is not very 
			// accurate but should be good enough most of the time.
			const textAreaLeftRightPadding = 6 + 6;
			textAreaRect = this.calculateHtmlBoundingRect(this.textAreaElement.innerHTML, portraitContainer.w - textAreaLeftRightPadding);
		}
		else if (this.tiledElements.text !== null)
		{
			this.tiledElements.textArea.style.display = 'none';
		}

		let portrait = this.createMobilePanelSizesPortrait(portraitContainer, imageSize, hasText, textAreaRect);
		let portraitPartial = portrait.portraitPartial;
		let portraitFull = portrait.portraitFull;

		let landscape = this.createMobilePanelSizesLandscape(landscapeContainer, imageSize, hasText);
		let landscapePartial = landscape.landscapePartial;
		let landscapeFull = landscape.landscapePartial;

		this.contentPanelSize.w = isPortraitOrientation ? portraitPartial.w : landscapePartial.w;
		this.contentPanelSize.h = isPortraitOrientation ? portraitPartial.h : landscapePartial.h;

		return { portraitPartial, portraitFull, portraitContainer, landscapePartial, landscapeFull, landscapeContainer };
	}

	createMobilePanelSizesLandscape(landscapeContainer, imageSize, hasText)
	{
		// For landscape orientation, let the image use less than half of the right side of the device. Allow the
		// height of the image to be around 3 / 4 of the device height. The goal is to show the entire image.

		let tabHeight = this.page.isDataSheet ? 0 : this.contentPanelTabHeight;

        // PARTIAL LANDSCAPE PANEL
		// There is no longer a full landscape panel because for hotspot content it proved to be unattractive and/or
		// confusing most of the time. It used to be used for data sheets, but now the partial panel is used at full
		// width. Though there is only one panel, the partial and full panel naming convention is still used to stay
		// consistent with the partial and full panel naming convention used for portrait orientation.

		// Calculate the max width and height of the partial panel. When there's a title bar and/or banner, let the,
		// panel come close to it, but when there isn't, keep it down by a little so the close X is not so close 
		// to the browser toolbar that the toolbar is too easily touched my mistake. Use almost half of the available
		// width. Note that when the map auto-pans to get out of the way of a marker on the right side of the map, the
		// marker will get positioned centered horizontally. Thus using a width above 48% could cover the marker.
		const bottomShadowGap = 3;
		const rightSideGap = 3;
		let topOffset = this.bannerAndTitleHeight > 0 ? 2 : 8;
		let landscapePartialMaxPanelW = Math.round(landscapeContainer.w * 0.45) - rightSideGap;
		let landscapePartialMaxPanelH = landscapeContainer.h - topOffset - this.bannerAndTitleHeight - bottomShadowGap;
		let landscapePartialMaxContentH = landscapePartialMaxPanelH - tabHeight;

        // Determine how much of the partial panel height to use for the media area. Use 90% of the available height.
        let landscapePartialMaxImageH = hasText ? Math.round(landscapePartialMaxContentH * 0.9) : landscapePartialMaxContentH;

        // Calculate the size of the image when it is scaled to fit within the partial panel media area.
		let landscapePartialMediaAreaSize = Runtime__$$.createSizeObject(landscapePartialMaxPanelW, landscapePartialMaxImageH);
		let landscapePartialScaledImageSize = Runtime__$$.scaledImageSize(imageSize, landscapePartialMediaAreaSize);
		let h = this.page.map.markerIsSelected ? landscapePartialMaxPanelH : 0;

        // Configure the partial landscape panel.
		let landscapePartial = {
			w: this.page.isDataSheet ? landscapeContainer.w : landscapePartialMaxPanelW,
			h: h,
			left: landscapeContainer.w - landscapePartialMaxPanelW - rightSideGap,
			top: topOffset,
            imageSize: landscapePartialScaledImageSize,
        };

		return { landscapePartial };
    }

	createMobilePanelSizesPortrait(portraitContainer, imageSize, hasText, textAreaRect)
	{
		// For portrait orientation, let the image use the full panel width as long as the height does
		// not cause the bottom of the image to get cut off. The goal is to show a scaled verson of the
		// entire image and a little bit of text. When there is no text, make the image as big as possible
		// without cutting any off at the bottom.

		const isDataSheet = this.page.isDataSheet;
		let tabHeight = isDataSheet ? 0 : this.contentPanelTabHeight;

		// Calculate the max height of the partial and full panels.
		let partialMaxPanelH = Math.round(0.50 * portraitContainer.h) - this.bannerAndTitleHeight;
		let partialMaxContentH = partialMaxPanelH - tabHeight;
		let fullMaxPanelH = portraitContainer.h - this.bannerAndTitleHeight;
		let fullMaxContentH = fullMaxPanelH - tabHeight;

        // PARTIAL PORTRAIT PANEL
        // Determine how much of the partial panel height to use for the media area.
        let partialMaxImageH = hasText ? Math.round(partialMaxContentH * 0.8) : partialMaxContentH;

        // Calculate the size of the image when it is scaled to fit within the partial panel media area.
		let partialMediaAreaSize = Runtime__$$.createSizeObject(portraitContainer.w, partialMaxImageH);
		let partialScaledImageSize = Runtime__$$.scaledImageSize(imageSize, partialMediaAreaSize);

        // Determine the adjusted height of the partial panel based on whether the content height exceeds the max height.
		const textAreaTopBottomPadding = 18;
		let partialActualContentH = partialScaledImageSize.h + textAreaRect.height + textAreaTopBottomPadding;
        let partialAdjustedContentH = partialActualContentH < partialMaxContentH ? partialActualContentH : partialMaxContentH;
		let partialAdjustedPanelH = Math.ceil(tabHeight + partialAdjustedContentH);

		// Allow for a small gap between the top of the panel and the bottom of the map plus a small shadow area at the bottom.
		const topGap = 2;
		const bottomShadow = 3;

		// Adjust the top if necessary so there's only a small gap between the bottom of the map and the top of the partial panel.
		let maxTop = isDataSheet ? topGap : this.map.convertMapToScreen(this.map.mapH_actual) + topGap;
		let top = portraitContainer.h - this.bannerAndTitleHeight - partialAdjustedPanelH;
		let h = 0;
		if (this.page.map.markerIsSelected)
		{
			h = partialAdjustedPanelH;
			if (top > maxTop)
			{
				h = h + (top - maxTop) - bottomShadow;
				top = maxTop;
			}
			else
			{
				h -= bottomShadow;
			}
		}

        // Configure the partial portrait panel.
        let portraitPartial = {
            w: portraitContainer.w,
			h: h,
            left: 0,
			top: top,
            imageSize: partialScaledImageSize,
        };

        // FULL PORTRAIT PANEL
        // Calculate the size of the image when it is scaled to fit within the full panel media area.
		let fullMediaAreaSize = Runtime__$$.createSizeObject(portraitContainer.w, fullMaxContentH);
		let fullScaledImageSize = Runtime__$$.scaledImageSize(imageSize, fullMediaAreaSize);

        // Determine the adjusted height of the full panel based on whether the content height exceeds the max height.
		let fullActualContentH = fullScaledImageSize.h + textAreaRect.height + textAreaTopBottomPadding;
		let fullAdjustedContentH = fullActualContentH < fullMaxContentH ? fullActualContentH : fullMaxContentH;
		let fullAdjustedPanelH = Math.ceil(tabHeight + fullAdjustedContentH);

		// Adjust the top if necessary so there's no gap between the bottom of the map and the top of the full panel.
		top = isDataSheet ? 0 : portraitContainer.h - this.bannerAndTitleHeight - fullAdjustedPanelH;
		h = 0;
		if (this.page.map.markerIsSelected || isDataSheet)
		{
			h = fullAdjustedPanelH;
			if (top > maxTop)
			{
				h = h + (top - maxTop) - bottomShadow;
				top = maxTop;
			}
			else
			{
				h -= bottomShadow;
			}
		}

        // Configure the full portrait panel.
        let portraitFull = {
            w: portraitContainer.w,
			h: h,
            left: 0,
			top: top,
            imageSize: fullScaledImageSize
		};

		// Determine whether the panel should be expandable. Galleries always display the full panel.
		let partialToFullPercentage = portraitPartial.h / portraitFull.h;
		if (partialToFullPercentage >= 0.8 || this.page.isGallery)
		{
			// When the partial height is nearly as tall as the full height, use the full height.
			portraitPartial = Object.assign({}, portraitFull);
			this.disablePartialPortraitExpander = true;
		}
		else
		{
			// Disable the partial panel expander when all the content is already showing. Allow the comparison
			// to be off by a few pixels to account for rounding errors such that the full panel is virtually
			// identical to the partial panel.
			const aFewPixels = 2;
			this.disablePartialPortraitExpander = fullActualContentH - aFewPixels <= partialMaxContentH;
		}

		return { portraitPartial, portraitFull };
    }

	createTourElements()
	{
        this.validateNavButtonLocation();
		this.map.createMapLayers(this.tour.navButtonLocation === this.NAV_MAP_LEFT);
        this.calculateLayoutPadding();
        this.calculateLayoutAreaPercentages();
        this.createElementsForLayout();
    }

	detectIfGrabberBarNeeded() 
	{
        this.showGrabberBar = false;
		if (this.tour.isTouchDeviceWithoutMouse && !this.page.isDataSheet)
		{
            this.showGrabberBar = true;
            this.createGrabberBar();
        }
    }

	emToPx(em)
	{
		return em * 16;
	}

	enableMobileLayout() 
	{
		if (this.tour.editMode)
			return;

		// Determine whether to use the mobile or desktop layout.
		this.usingMobileLayout = this.tour.isStandalone && this.tour.isSmallMobileDevice;

		// Check if the user has chosen to explicitly use the mobile layout or the desktop layout.
		let mobileSetting = this.tour.getTourSetting('mobile-mode');
		if (mobileSetting)
		{
			// Treat any value other than true or false as auto which means don't override the default mode.
			mobileSetting = mobileSetting.toLowerCase();
			if (mobileSetting === 'always')
				this.usingMobileLayout = true;
			else if (mobileSetting === 'never')
				this.usingMobileLayout = false;
			else if (mobileSetting === 'auto')
				this.usingMobileLayout = this.tour.isSmallMobileDevice;
		}

		// Don't allow the nav panel to stay open when using mobile layout because there usually isn't enough room.
		if (this.usingMobileLayout && this.tour.hasNavPanel)
			this.tour.directory.staysOpen = false;
	}

	getComputedHeight(element)
	{
		let height = 0;
		if (element)
		{
			height = Math.round(parseFloat(getComputedStyle(element).height, 10));
			if (isNaN(height))
				height = 0;
		}

		return height;
	}

	getHtmlElementHeights() 
	{
        this.bannerHeight = this.getComputedHeight(this.bannerElement);
        this.titleBarHeight = this.getComputedHeight(this.titleBarElement);
        this.headerStripeHeight = this.getComputedHeight(this.headerStripeElement);
        this.footerStripeHeight = this.getComputedHeight(this.footerStripeElement);
        this.footerHeight = this.getComputedHeight(this.footerElement);
    }

	getHtmlElements()
	{
        this.tourElement = document.getElementById(this.tour.tourElementId);
        this.layoutElement = this.tour.getElementByUniqueId('Layout');
        this.bannerElement = this.tour.getElementByUniqueId('Banner');
        this.headerStripeElement = this.tour.getElementByUniqueId('HeaderStripe');
        this.titleBarElement = this.tour.getElementByUniqueId('TitleBar');
        this.titleTextElement = this.tour.getElementByUniqueId('TitleText');
        this.footerStripeElement = this.tour.getElementByUniqueId('FooterStripe');
        this.footerElement = this.tour.getElementByUniqueId('Footer');
    }

	getScaledLayoutPadding()
	{
		let cs = getComputedStyle(this.layoutElement);
		let padding = {
			left: parseInt(cs.getPropertyValue('padding-left'), 10),
			right: parseInt(cs.getPropertyValue('padding-right'), 10),
			top: parseInt(cs.getPropertyValue('padding-top'), 10),
			bottom: parseInt(cs.getPropertyValue('padding-bottom'), 10)
		};
		return padding;
	}

	get hasMediaArea()
	{
		return this.mediaAreaElement !== null;
	}

	get hasTextArea()
	{
		return this.tiledElements.textArea !== null;
	}

	hideContentPanel() 
	{
		//console.log(`Layout::hideContentPanel`);

		if (!this.usingMobileLayout)
			return;

		if (this.page.isDataSheet)
			return;

		// Hide the tooltip to handle the case where the selected marker shows a tooltip instead
		// of a content panel. In that case, a request to hide the panel wil hide the tooltip.
		this.map.hideTooltip();

		if (this.contentPanelIsHidden)
			return;

		this.contentPanel.classList.add(this.MOBILE_HIDDEN);
		this.contentPanelIsHidden = true;
		this.currentImageSize = null;

		let viewId = this.map.selectedMarkerViewId;
		this.map.deselectMarker();

		// Restore the map to a valid pan position in case it had been over-panned to make a marker visible
		// instead of letting it get covered by the mobile content panel.
		if (this.usingMobileLayout)
			this.map.positionMapToMakeMarkerVisibleOnCanvas(viewId, false);

		this.map.drawMap();
	}

	identifyFullWidthHeightLayouts() 
	{
        this.fullHeightMapLayouts = ['VMMIT', 'VMMTI', 'VITMM', 'VTIMM', 'VMMTT', 'VTTMM', 'VMMII', 'VIIMM', 'HMM'];
        this.fullWidthMapLayouts = ['HMMIT', 'HMMTI', 'HMMII', 'HMMTT', 'HMIT', 'HMTI', 'HMM'];
        this.fullHeightMediaLayouts = ['VMMII', 'VIIMM', 'HII'];
        this.fullWidthMediaLayouts = ['HMIT', 'HMTI', 'HII', 'HIITT', 'HTTII', 'HII'];
	}

	invalidateNavPanel()
	{
		this.navPanel = null;
	}

	static layoutHasNoMap(layoutId)
	{
		let nonMapLayouts = ["HIITT", "HTTII", "HII", "HTT"];
		return nonMapLayouts.includes(layoutId);
	}

	get layoutHasMap()
	{
		return this.page.map && !this.page.isDataSheet;
	}

	makeContentVisibleOnMobileDevice(viewId)
	{
		//console.log(`Layout::makeContentVisibleOnMobileDevice ${viewId}`);

		if (!this.usingMobileLayout)
			return;

		// Don't show the content panel for a hotspot that displays no content.
		if (!(this.hasMediaArea || this.hasTextArea))
			return;

		if (viewId === 0)
		{
			this.hideContentPanel();
			return;
		}

		if (this.tour.hasNavPanel)
			this.closeNavPanel();

		let appearance;
		if (this.page.isDataSheet)
			appearance = this.containerOrientationIsPortrait ? this.MOBILE_PORTRAIT_FULL : this.MOBILE_LANDSCAPE_FULL;
		else
			appearance = this.containerOrientationIsPortrait ? this.MOBILE_PORTRAIT_PARTIAL : this.MOBILE_LANDSCAPE_PARTIAL;
		this.changeContentPanelAppearance(appearance);

		if (!(this.page.isDataSheet || this.page.isGallery))
			this.map.positionMapToMakeMarkerVisibleOnCanvas(viewId);

		let title = this.page.isDataSheet ? this.page.pageName : this.page.getView(viewId).title;
		this.contentPanelTitle.innerHTML = title;
	}

	get map()
	{
		return this.page.map;
	}

	get mapIsFullHeight()
	{
		return this.page.hasPopup || this.usingMobileLayout || this.fullHeightMapLayouts.includes(this.page.layoutId);
	}

	get mapIsFullWidth()
	{
		return this.page.hasPopup || this.usingMobileLayout || this.fullWidthMapLayouts.includes(this.page.layoutId);
	}

	get mediaAreaElement()
	{
		return this.page.hasPopup ? this.popupElements.mediaArea : this.tiledElements.mediaArea;
	}

	get mediaAreaIsFullHeight()
	{
		return this.fullHeightMediaLayouts.includes(this.page.layoutId);
	}

	get mediaAreaIsFullWidth()
	{
		return this.fullWidthMediaLayouts.includes(this.page.layoutId);
	}

	get navButtonLocationIsAbove()
	{
		return this.tour.navButtonLocation === this.NAV_ABOVE_LEFT ||
			this.tour.navButtonLocation === this.NAV_ABOVE_CENTER ||
			this.tour.navButtonLocation === this.NAV_ABOVE_RIGHT;
	}

	get navButtonLocationIsBanner()
	{
		return this.tour.navButtonLocation === this.NAV_BANNER_LEFT ||
			this.tour.navButtonLocation === this.NAV_BANNER_CENTER ||
			this.tour.navButtonLocation === this.NAV_BANNER_RIGHT;
	}

	get navButtonLocationIsLeft()
	{
		return this.tour.navButtonLocation === this.NAV_MAP_LEFT ||
			this.tour.navButtonLocation === this.NAV_BANNER_LEFT ||
			this.tour.navButtonLocation === this.NAV_ABOVE_LEFT;
	}

	get navPanelIsShowing()
	{
		return this.navPanel.classList.contains('show');
	}

	onClickHelpPanelCloseX()
	{
		this.closeHelpPanel();
	}

	onClickContentPanelCloseX(event)
	{
		event.stopPropagation();
		this.hideContentPanel()
	}

	onClickContentPanelTab()
	{
		if (this.panelExpanderDisabled || this.page.isDataSheet)
			return;

		let visibility = this.MOBILE_HIDDEN;

		if (this.containerOrientationIsPortrait)
		{
			if (this.contentPanelAppearance === this.MOBILE_HIDDEN)
				visibility = this.MOBILE_PORTRAIT_PARTIAL;
			else if (this.contentPanelAppearance === this.MOBILE_PORTRAIT_PARTIAL)
				visibility = this.MOBILE_PORTRAIT_FULL;
			else if (this.contentPanelAppearance === this.MOBILE_PORTRAIT_FULL)
				visibility = this.MOBILE_PORTRAIT_PARTIAL;
		}
		else
		{
			if (this.contentPanelAppearance === this.MOBILE_HIDDEN)
				visibility = this.MOBILE_LANDSCAPE_PARTIAL;
			else if (this.contentPanelAppearance === this.MOBILE_LANDSCAPE_PARTIAL)
				visibility = this.MOBILE_LANDSCAPE_FULL;
			else if (this.contentPanelAppearance === this.MOBILE_LANDSCAPE_FULL)
				visibility = this.MOBILE_LANDSCAPE_PARTIAL;
		}

		this.changeContentPanelAppearance(visibility)
	}

	onClickHelpButton(event)
	{
		event.stopPropagation();
		let dataset = event.currentTarget.dataset;
		this.showHelpPanel(dataset);
	}

	onClickNavButton()
	{
		this.page.map.stopCurrentOperation();

		this.closeHelpPanel();

		// Make sure the panel still exists. It won't if user JavaScript called api.setHotspotTitle
		// which, in turn, would invalidate the panel contents and cause the panel to get removed.
		if (this.navPanel === null)
			this.createElementsForNavPanel();

		this.toggleShowNavPanel();

		if (this.tour.hasDirectory)
			this.tour.directory.showEntries(true)
	}

	onClickMenuItem(event)
	{
		event.stopPropagation();

		let pageNumber = parseInt(event.currentTarget.dataset.pageNumber, 10);
		this.tour.goToPage(pageNumber);
	}

	onTouchMapElement(event)
	{
		// The purpose of this method is to intercept and ignore touch events on the map element
		// (not the map canvas) for the sole purpose of preventing iOS from selecting the map
		// element and putting a blue selection area over it, when the user puts their finger on
		// the map and holds it there for awhile. Since the help panel is a child of the
		// map element, touches on its close X will cause this method to be called and so those
		// calls are passed along to the close X click handler.

		//console.log(`Layout::onTouchMap ${event.type}`);

		if (event.target.id === this.tour.uniqueId('HelpCloseX'))
			this.map.onClickCloseHelp();

		// Allow the user to touch and drag the grabber bar.
		if (event.target === this.grabberBar)
		{
			if (event.type === 'touchstart')
				this.grabberBar.classList.add('maSelected');
			else
				this.grabberBar.classList.remove('maSelected');
			return;
		}

		// Don't let the event bubble up.
		event.preventDefault();
	}

	pixels(dimension)
	{
		return dimension + 'px';
	}

	get popup()
	{
		return this.page.popup;
	}

	positionGrabberBar()
	{
		if (!this.showGrabberBar)
			return;

		let barH = Math.round(this.mapSize.h * 0.50);
		let barW = 12;
		this.grabberBar.style.height = barH + 'px';
		this.grabberBar.style.width = barW + 'px';
		this.grabberBar.style.left = Math.round(this.mapSize.w - (barW * 1.75)) + 'px';
		this.grabberBar.style.top = Math.round(this.mapSize.h * 0.25) + 'px';
	}

	pxToEm(px)
	{
		return (px / 16).toFixed(3) + 'em';
	}

	redrawMap(resize)
	{
		if (resize)
		{
			// Redraw the map at its new size and scale, but at its current panning position.
			let newScale = this.map.currentMapScale * 100;
			this.map.zoomMap(newScale, false, true);
		}

		else
		{
			// This is the initial call to draw the map when the page first loads.
			this.map.drawMap();
		}
	}

	removeLayoutElements()
	{
		if (this.mapElement)
		{
			this.layoutElement.removeChild(this.mapElement);
			this.mapElement = null;
		}

		if (this.navButton)
		{
			this.navButtonParent.removeChild(this.navButton);
			this.navButton = null;
		}

		if (this.navPanel)
		{
			this.tourElement.removeChild(this.navPanel);
			this.navPanel = null
		}

		if (this.tour.directory.dirPreviewPanel)
		{
			this.layoutElement.removeChild(this.tour.directory.dirPreviewPanel);
			this.tour.directory.dirPreviewPanel = null;
		}

		this.layoutElement.removeChild(this.contentPanel);
		this.contentPanel = null;

		if (this.popupElement)
		{
			this.popupElement.parentNode.removeChild(this.popupElement);
			this.popupElement = null;
		}
	}

	resetContentScrollTop()
	{
		if (this.usingMobileLayout)
		{
			if (this.contentArea)
				this.contentArea.scrollTop = 0;
		}
		else
		{
			if (this.textAreaElement)
				this.textAreaElement.scrollTop = 0;
		}
	}

	restoreFontSizes()
	{
		// This method provides support for the setTextAreaFontSizes method. The first time it
		// is called, it saves the hotspot's font sizes. On subsequent calls it checks to see
		// if the font sizes have been reduced and if so, restores them to the saved sizes.

		// Remember the original font sizes for the popup title and text. This only gets done the first time
		// the popup is shown because these values are the same for every popup regardless of its content.
		if (this.fontSizes === null)
		{
			let titleElement = this.titleElement;
			let titleFontSize = titleElement ? getComputedStyle(titleElement).fontSize : '';
			let textElement = this.textElement;
			let textFontSize = textElement ? getComputedStyle(textElement).fontSize : '';;
			this.fontSizes = { title: titleFontSize, text: textFontSize, reduced: false };
			return;
		}

		let viewId = this.page.isDataSheet ? this.page.firstViewId : this.map.selectedMarkerViewId;
		//console.log(`restoreFontSizes ${viewId}`);

		if (viewId === 0)
			return;

		// When the popup's font sizes have been previously reduced, restore them to their original
		// sizes. This is necessary so that they will be correct when the content needs to be displayed
		// at its full size, but also so that when the content will be displayed at a reduced size, the
		// original sizes will be used to calcuate the scaled-down sizes. If we didn't restore the
		// original sizes, the scaling would be applied to already scaled values causing the font
		// size to just keep getting smaller.
		if (this.fontSizes.reduced)
		{
			// Restore the original title and text font sizes.
			if (this.fontSizes.title)
				this.titleElement.style.fontSize = this.fontSizes.title;
			if (this.fontSizes.text)
				this.textElement.style.fontSize = this.fontSizes.text;
			this.fontSizes.reduced = false;

			// Reload the view text to force the browser to recompute unreduced font sizes
			// for any styled HTML elements that the user provided for this marker's hotspot text.
			this.page.setViewText(viewId);
		}
	}

	scaleFontSize(scale, originalSize, minSize, governor)
	{
		// Reduce the font to coincide with the passed-in scale, but add-in a constant to act as a
		// governor that limits how soon and how quickly the size gets reduced. Without the constant,
		// the font gets too small too quickly when it's not really necessary.

		// Scale the font with the governor amount added in.
		const candidateSize = scale * (originalSize + governor);

		// Make sure the result is not less than the minimum size allowed.
		let reducedSize = Math.max(candidateSize, minSize);

		// Now test that the result is not larger than the original. This is where the governing
		// comes in. The responsive scale needs to get small enough before a reduction even kicks
		// in, and then the contant amount is always added-in to maintain a reasonable size until
		// the minimum size is reached.
		if (reducedSize > originalSize)
			reducedSize = originalSize;

		return Runtime__$$.roundFloat(reducedSize, 3);
	}

	setBodyStyle()
	{
		if (this.tour.preview || this.tour.isEmbedded)
			return;

		// Save space by removing the body margin on small devices and scaling it otherwise.
        let margin = 0;
        if (!this.usingMobileLayout)
            margin = this.convertToResponsiveValue(this.tour.bodyMargin) + 'px';
		document.body.style.margin = margin;

		// Prevent scroll bars from every appearing on a flex map. There is no circumstance where they should be
		// visible because flex maps constantly adjust to whatever container they are in and as such, there is
		// nothing to overflow. However, sometimes browsers will quickly display and then hide them, and occasionally
		// display and not hide them even though they are not needed. Setting overlow prevents the scroll bars from
		// ever displaying. The one exception is a standalone tour on a small mobile, specifically Safari on the iPhone,
		// where in landscape orientation the browser's toolbar can drop down on top of the tour. In that case the
		// tour needs to be scrollable so that the user can drag on the map's grabber bar to scroll the page down to
		// expose the part that's under the toolbar. If overflow were set to hidden, dragging the grabber would not
		// work because the page would not be scollable.
		if (this.tour.isFlexMapTour && !this.tour.isSmallMobileDevice)
			document.body.style.overflow = 'hidden';
    }

	setCanvasCtxSize(ctx)
	{
		// Set a new canvas size, but only if the size has changed, because setting the size of a canvas,
		// even to the same size, clears the canvas and its state. The canvas must not be cleared in this
		// way because the map drawing code is optimized to not clear the canvas when the map does not need
		// to be redrawn e.g. if the user is making their browser wider than the map image. When the map
		// does get drawn, the map drawing logic will clear the canvas as necessary.
		if (ctx.canvas.width === this.map.canvasW && ctx.canvas.height === this.map.canvasH)
			return;

		ctx.canvas.width = this.map.canvasW;
		ctx.canvas.height = this.map.canvasH;
	}

	setContentHeight(height)
	{
		this.contentArea.style.height = height + 'px';
	}

	setContentHeightForColumn()
	{
		// This method sets the content area height when the content is in a column to the left or right of the map.
		this.setContentHeight(this.scaledInnerLayoutH);
		this.setOverflowForContentArea();
	}

	setContentHeightForRow()
	{
		// This method sets the content area height when the content is in a row above or below the map.
		let height = this.scaledInnerLayoutH - this.map.canvasH - this.scaledLayoutSpacingRow;
		this.setContentHeight(height);
	}

	setFlexDirection(layoutDirection, contentDirection = '')
	{
		this.layoutElement.style.flexDirection = layoutDirection;

		if (contentDirection !== '')
			this.contentArea.style.flexDirection = contentDirection;
	}

	setFlexGaps(gap1 = '', gap2 = '')
	{
		// As of April 2021 not all browsers were supporting flex gap so margin is used instead.

		function setGap(layout, style, gap)
		{
			let rowGap = layout.scaledLayoutSpacingRow + 'px';
			let columnGap = layout.scaledLayoutSpacingColumn + 'px';

			style.margin = '0 0 0 0';

			switch (gap)
			{
				case 'top':
					style.marginTop = rowGap;
					break;

				case 'right':
					style.marginRight = columnGap;
					break;

				case 'bottom':
					style.marginBottom = rowGap;
					break;

				case 'left':
					style.marginLeft = columnGap;
					break;
			}

		}

		// Gap 1 is either the content column or the content row.
		if (gap1)
			setGap(this, this.contentArea.style, gap1);

		// Gap 2 is always the media area.
		if (gap2 && this.tiledElements.mediaArea !== null)
			setGap(this, this.tiledElements.mediaArea.style, gap2);
	}

	setLayoutPaddingSize()
	{
		if (!this.usingMobileLayout)
		{
			// Adjust the padding to the current scale, but not for mobile since it's already minimal.
			let top = this.convertToResponsiveValue(this.layoutPadding.top);
			let right = this.convertToResponsiveValue(this.layoutPadding.right);
			let bottom = this.convertToResponsiveValue(this.layoutPadding.bottom);
			let left = this.convertToResponsiveValue(this.layoutPadding.left);
			this.setPaddingStyle(this.layoutElement, top, right, bottom, left);
		}
	}

	setLayoutStyles()
	{
		// Apply the padding to the layout element's style.
		this.setPaddingStyle(this.layoutElement, this.layoutPadding.top, this.layoutPadding.right, this.layoutPadding.bottom, this.layoutPadding.left);

		// Calculate the unscaled layout area dimensions
		this.layoutAreaW = this.tour.layoutAreaW - (this.layoutPadding.left + this.layoutPadding.right);
		this.layoutAreaH = this.tour.layoutAreaH - (this.layoutPadding.top + this.layoutPadding.bottom);

		// To center a standalone tour in the browser, set its parent <main> to use margin auto.
		if (this.tour.centeredInBrowser && this.tour.isStandalone)
			this.tourElement.parentElement.style.margin = 'auto';
	}

	setMapSize(resize)
	{
		if (!this.layoutHasMap)
			return;

		this.calculateMapSize();

		let sizeChanged = this.mapSize.w !== this.map.canvasW || this.mapSize.h !== this.map.canvasH;

		console.log(`Layout::setMapSize ${this.page.layoutId} ${this.responsiveLayoutId} ${this.mapSize.w}x${this.mapSize.h} ${this.map.currentMapScale.toFixed(3)}%`);

		if (sizeChanged || !resize)
		{
			this.map.setCanvasSize(this.mapSize);
			this.map.calculateZoomedOutMapScale();

			this.setCanvasCtxSize(this.map.mapLayerContext);
			this.setCanvasCtxSize(this.map.markerLayerContext);
			this.setCanvasCtxSize(this.map.hitLayerContext);

			// Shift the map inset to its proper position at the new map size.
			this.map.setMapInsetRectSize({ resize: true });

			this.map.mapZoomPanStateChanged();
		}

		this.mapElement.style.width = this.map.canvasW + "px";
		this.mapElement.style.height = this.map.canvasH + "px";
	}

	setNavButtonPosition()
	{
		if (!this.tour.hasNavPanel)
			return;

		const MARGIN = 6;
		const ICON_SIDE = 24;
		const ICON_SIDE_MIDDLE = ICON_SIDE / 2;
		
		let alignment = 'R';
		let navButtonLeft;
		let navButtonTop;
		let navLocation = this.tour.navButtonLocation;
		let tourBounds = this.tourElement.getBoundingClientRect();

		if (navLocation === this.NAV_MAP_LEFT)
		{
			navButtonLeft = MARGIN + this.tour.navButtonLocationX;
			navButtonTop = MARGIN;
			alignment = 'L';
		}
		else if (navLocation === this.NAV_MAP_RIGHT)
		{
			navButtonLeft = this.mapSize.w - ICON_SIDE - MARGIN + this.tour.navButtonLocationX;
			navButtonTop = (MARGIN * 2) + 2;
			alignment = 'R';
		}
		else if (navLocation === this.NAV_TITLE_BAR)
		{
			navButtonLeft = tourBounds.width - ICON_SIDE - MARGIN;
			if (this.tour.navButtonLocationX < 0)
				navButtonLeft += this.tour.navButtonLocationX;
			navButtonTop = MARGIN - 3;
		}
		else
		{
			switch (navLocation)
			{
				case this.NAV_BANNER_LEFT:
				case this.NAV_ABOVE_LEFT:
					navButtonLeft = this.tour.navButtonLocationX;
					alignment = 'L';
					break;

				case this.NAV_ABOVE_CENTER:
				case this.NAV_BANNER_CENTER:
					navButtonLeft = Math.round(this.scaledTourW / 2 - ICON_SIDE_MIDDLE + this.tour.navButtonLocationX);
					alignment = 'C';
					break;

				case this.NAV_ABOVE_RIGHT:
				case this.NAV_BANNER_RIGHT:
					navButtonLeft = this.scaledTourW - ICON_SIDE + this.tour.navButtonLocationX;
					break;
			}

			switch (navLocation)
			{
				case this.NAV_BANNER_LEFT:
				case this.NAV_BANNER_CENTER:
				case this.NAV_BANNER_RIGHT:
					navButtonTop = this.tour.navButtonLocationY; 
					if (navButtonTop < 0)
						navButtonTop = 0;

					if (this.bannerElement !== null)
					{
						// Calculate the scaled banner height instead of getting its computed height because the
						// banner image may not have been loaded yet the first time this method is called, in
						// which case the computed height will be zero.
						let ratio = this.tour.bannerHeight / this.tour.width;
						let scaledBannerH = ratio * this.scaledTourW;

						// Determine the menu top position in pixels based on the user-specified y offset.
						let scaledDistanceFromBannerTopToNavButtonTop = scaledBannerH / this.tour.bannerHeight;
						navButtonTop = Math.round(scaledDistanceFromBannerTopToNavButtonTop * (navButtonTop + ICON_SIDE_MIDDLE)) - ICON_SIDE_MIDDLE;

						// Prevent the top of the icon from going above the top of the banner.
						if (navButtonTop < 0)
							navButtonTop = 0;

						// Prevent the bottom of the icon from going below the bottom of the banner.
						let safetyMargin = 4;
						if (navButtonTop + ICON_SIDE + safetyMargin > scaledBannerH)
							navButtonTop = scaledBannerH - ICON_SIDE - safetyMargin;
					}
					break;

				case this.NAV_ABOVE_LEFT:
				case this.NAV_ABOVE_CENTER:
				case this.NAV_ABOVE_RIGHT:
					navButtonTop = -ICON_SIDE;
					navButtonTop += this.tour.navButtonLocationY;
					break;
			}
		}

		this.navButton.style.left = navButtonLeft + 'px';
		this.navButton.style.top = navButtonTop + 'px';

		// When the nav button is located above the tour, and the tour is standalone or in tour preview,
		// there's no room above the tour for the button. Add margin above the tour to make room so that
		// the user can get an accurate idea of where the button will appear when the tour is embedded.
		if (this.navButtonLocationIsAbove && (this.tour.preview || this.tour.isStandalone))
			this.tourElement.style.marginTop = ICON_SIDE - this.tour.navButtonLocationY + 4 + 'px';

		let navButtonRect = this.navButton.getBoundingClientRect();

		let tourRect = this.tourElement.getBoundingClientRect();

		let navButtonLeftOffset = navButtonRect.left - tourRect.left;
		let navButtonTopOffset = navButtonRect.top - tourRect.top;

		this.calculateNavPanelSize();

		const SPACER = 4;
		let navPanelLeft;
		let navPanelTop;

		if (this.usingMobileLayout)
		{
			if (navLocation === this.NAV_ABOVE_CENTER || navLocation === this.NAV_BANNER_CENTER)
				navPanelTop = navButtonTopOffset;
			else
				navPanelTop = SPACER;
		}
		else
		{
			navPanelTop = navButtonTopOffset;
		}

		switch (navLocation)
		{
			case this.NAV_BANNER_LEFT:
			case this.NAV_ABOVE_LEFT:
			case this.NAV_MAP_LEFT:
				navPanelLeft = navButtonLeftOffset + ICON_SIDE + SPACER;
				break;

			case this.NAV_ABOVE_CENTER:
			case this.NAV_BANNER_CENTER:
				navPanelLeft = Math.round(navButtonLeftOffset + ICON_SIDE_MIDDLE - this.navPanelSize.w / 2);
				navPanelTop += ICON_SIDE + SPACER;
				break;

			case this.NAV_ABOVE_RIGHT:
			case this.NAV_BANNER_RIGHT:
			case this.NAV_MAP_RIGHT:
			case this.NAV_TITLE_BAR:
				navPanelLeft = navButtonLeftOffset - SPACER - this.navPanelSize.w;
				break;
		}

		// Calculate the maximum height of the nav panel. On mobile, it's all
		// the height available but on desktop its the max height set by the user.
		this.navPanelMaxHeight = this.usingMobileLayout ? this.containerSize.h - 8 : this.tour.directory.maxHeight;

		// Dynamically set the panel's styles that change when the panel position changes.
		this.createCssForNavPanel(navPanelLeft, navPanelTop, alignment);
	}

	setOverflowForContentArea()
	{
		// This method is called for layouts where the image and text are in the same column (image over text
		// or text over image). It provides vertical scroll bar that lets the user scroll the image and text
		// content together.
		this.contentArea.style.overflow = 'auto';
	}

	setOverflowForTextArea()
	{
		// This method is called for layouts where the image and text are side by side. It provides a vertical
		// scroll bar that lets the user scroll just the text.
		this.textAreaElement.style.overflow = 'auto';
	}

	setPaddingStyle(element, top, right, bottom, left)
	{
		element.style.padding = `${top}px ${right}px ${bottom}px ${left}px`;
	}

	setPaddingStyleEm(element, top, right, bottom, left)
	{
		element.style.padding = `${this.pxToEm(top)} ${this.pxToEm(right)} ${this.pxToEm(bottom)} ${this.pxToEm(left)}`;
	}

	setPageTitle(title)
	{
		if (this.titleTextElement)
			this.titleTextElement.innerHTML = title;
	}

	setScaledLayoutAreaSize()
	{
		// Get and set the layout element's height which includes its padding.
		let scaledOuterLayoutHeight = this.calculateScaledOuterLayoutHeight();
		this.layoutElement.style.height = scaledOuterLayoutHeight + 'px';

		// Subtract the top and bottom padding to get the usable layout area height.
		let layoutStyle = getComputedStyle(this.layoutElement);
		let layoutTopBottomPaddingHeight = parseInt(layoutStyle.paddingTop, 10) + parseInt(layoutStyle.paddingBottom, 10);

		this.scaledInnerLayoutH = scaledOuterLayoutHeight - layoutTopBottomPaddingHeight;
		let layoutW = this.scaledTourW;

		// Narrow the layout width by the amount of its left and right padding.
		let layoutPadding = this.getScaledLayoutPadding(this.layoutElement);
		layoutW -= (layoutPadding.left + layoutPadding.right);

		this.scaledInnerLayoutW = layoutW;

		this.scaledLayoutSpacingRow = Math.round(this.convertToResponsiveValue(this.page.layoutSpacingH));
		this.scaledLayoutSpacingColumn = Math.round(this.convertToResponsiveValue(this.page.layoutSpacingV));
	}

	setSizesOfNonLayoutElements()
	{
		//console.log(`Layout::setSizesOfNonLayoutElements`);

		// Set the tour and layout elements to the new width.
		this.tourElement.style.width = this.scaledTourW + 'px';
		this.layoutElement.style.width = this.scaledTourW + 'px';

		this.setLayoutPaddingSize();

		let w = this.scaledTourW + 'px';

		if (this.bannerElement)
			this.bannerElement.style.maxWidth = w;

		if (this.titleBarElement)
		{
			// Scale the title bar height unless the menu icon is located there.
			if (this.tour.navButtonLocation !== this.NAV_TITLE_BAR)
			{
				this.titleBarElement.style.width = w;
				this.titleBarElement.style.height = this.convertToResponsivePx(this.titleBarHeight, 24);
			}

			const MAX_FONT_SIZE_EM = 1.0;
			const MIN_FONT_SIZE_EM = 0.8;
			let reducedFontSizeEm = this.scaleFontSize(this.responsiveTourScale, MAX_FONT_SIZE_EM, MIN_FONT_SIZE_EM, 0.25);
			this.titleBarElement.style.fontSize = reducedFontSizeEm + 'em';
		}

		if (this.headerStripeElement)
		{
			this.headerStripeElement.style.width = w;
			this.headerStripeElement.style.height = this.convertToResponsiveValue(this.headerStripeHeight) + 'px';
		}

		// When setting the size of the footer stripe and/or footer, also set display to block. When the tour first
		// loads, display is set to none to avoid the screen flicker that would otherwise occur between the time
		// a flex map starts loading and when it finishes. THe flicker occurs because until the map loads, the stripe
		// and footer get rendered at the top of the tour (below the banner and title), but then jump down below the
		// map once it loads. The flicker is avoided by hiding these elements until the map is loaded.

		if (this.footerStripeElement)
		{
			this.footerStripeElement.style.width = w;
			this.footerStripeElement.style.height = this.convertToResponsiveValue(this.footerStripeHeight) + 'px';
		}

		if (this.footerElement)
		{
			this.footerElement.style.width = w;
			this.footerElement.style.height = this.convertToResponsivePx(this.footerHeight, 16);
		}
	}

	setTextAreaFontSizes(scale = 0, minFontSizePx = 9)
	{
		//console.log(`setTextAreaFontSizes ${this.responsiveTourScale}`);

		// This feature is currently disabled for all text because scaled description content
		// has provided to be unattractive and hard to read. Possibly enable it for scaling titles
		// or possibly allow it when the area available for the text is so small that showing
		// full size text doesn't make sense.
		return false;

		// This method is called to scale the text for both popups and the text area of
		// tiled layouts. When called for a popup, the scale is the scaled size of the
		// popup itself which has been made smaller to fit the available browser area.
		// The popup scale is independent of the tour scale. In fact, a popup can get
		// scaled when the responsive tour scale is 100%. When this method is called for
		// a tiled/layout, the scale is the responsive tour scale.

		if (this.textAreaElement === null)
			return;

		// Scale fonts to the responsive tour scale, but not on mobile. The mobile layout
		// can accommodate full size fonts which are needed for readability on small devices.
		if (scale === 0)
			scale = this.usingMobileLayout || this.usingResponsiveStackedLayout ? 1.0 : this.responsiveTourScale;

		this.restoreFontSizes();

		// Reduce the font size of each element in the text area. This includes the title,
		// and text elements, and also every child element that is in user-provided HTML.
		let fontSizePx;
		let elements = this.textAreaElement.querySelectorAll('*');
		for (const element of elements)
		{
			// Get the computed font size. Use parseFloat to strip off the 'px' at the end
			// while preserving any fractional parts of the size. Note that 'px' is always
			// used for computed styles because  that's what browsers use internally
			// regardless of the units (e.g. em or %) used inline or in CSS for the element.
			fontSizePx = parseFloat(getComputedStyle(element).fontSize, 10);

			let reducedFontSizePx = this.scaleFontSize(scale, fontSizePx, minFontSizePx, 4);
			element.style.fontSize = reducedFontSizePx + 'px';
		}

		this.fontSizes.reduced = true;
	}

	setTextAreaHeight()
	{
		let height = this.scaledInnerLayoutH - this.map.canvasH - this.scaledLayoutSpacingRow;
		this.tiledElements.textArea.style.height = height + 'px';
		this.tiledElements.textArea.style.overflow = 'auto';
	}

	setTextAreaScrollable()
	{
		// Override flex box behavior for the automatic minimum size of flex items which allows long
		// text to overflow its container even though overflow is set to auto on the HotspotTextAreaElement.
		// The code below causes a vertical scroll bar to appear in the text area.
		// See https://stackoverflow.com/questions/36247140/why-dont-flex-items-shrink-past-content-size
		this.contentPanel.style.overflow = 'hidden';
		this.contentArea.style.minHeight = 0;
		this.setOverflowForTextArea();
	}

	setTourSize({ resize, switchToStackedLayout = false })
	{
		// This method is called to set the initial tour size when a page first loads and is
		// subsequently called to resize the tour if the browser or tour container size change.

		if (resize && this.tour.disableResponsive)
			return;

		this.closeNavPanel();

		if (resize)
			this.hideContentPanel();

		this.setBodyStyle();

		// Set the max-width on the element that contains the tour. The setting constrains the width of a
		// Classic tour, and is needed for both classic and flex map tours to make horizontal centering
		// within the browser work in conjunction with margin auto. When in Tour Preview, don't set the max
		// width unless the tour is not centered in the browser. If it is not centered, then setting max-width
		// will prevent the from being centered within the centered Tour Preview page. The max-width must not
		// be set in an embedded tour because it would override the container element's max-width.
		let needMaxWidth = !this.tour.isEmbedded && (!this.tour.preview || !this.tour.centeredInBrowser);
		if (needMaxWidth)
		{
			let maxWidth;
			if (this.tour.isClassicTour)
				maxWidth = this.tour.width;
			else
				maxWidth = this.map.mapW_actual;
			this.tourElement.parentElement.style.maxWidth = maxWidth + 'px';
		}

		this.calculateContainerSize();

		// Create the initial CSS to style the content panel before it any content gets loaded into it.
		if (!resize && this.usingMobileLayout)
			this.createCssForContentPanel(Runtime__$$.createSizeObject());

		if (resize && !this.containerSizeChanged)
			return;
		
		this.createElementsForNavigation(resize);
		this.chooseActiveLayout();
		this.tour.setBannerStyle();
		this.setSizesOfNonLayoutElements();
		this.setScaledLayoutAreaSize();
		this.setMapSize(resize);

		// When displaying the map for the first time, position it to its locked center. However, if the map is
		// being reloaded on mobile because the device orientation changed, don't zoom and only pan if the pan
		// distances are invalid after the rotation.
		if (!resize && this.layoutHasMap && !this.page.isGallery)
			this.map.positionAndZoomMapToInitialFocus();

		this.positionGrabberBar();
		this.createCssForContentPanel(null, true);
		this.configureLayoutAreas(resize);

		if (this.layoutHasMap)
		{
			this.redrawMap(resize);

			// Refresh the content for a tiled desktop layout so that the image will resize to fit the new tour size.
			if (resize && !this.page.hasPopup && !this.usingMobileLayout)
				this.page.loadViewContent(this.page.currentView.viewId);
		}
		else if (this.page.isDataSheet)
		{
			this.page.showDataSheetContent();
		}

		this.setNavButtonPosition();

		// Handle the case where the tour loaded initially in a browser window that was so narrow as to require use of
		// the responsive stacked layout, but that was not determined until after the container size was calculated for
		// the first time and so the container height calculation used the tour height instead of the container height
		// which is needed for the stacked layout. To deal with this, do a recursive call to force the resize to occur
		// again, but with the layout set to stacked. Pass the switchToStackedLayout flag to keep from going into an
		// infinite loop. Note also that the resize parameter must remain set to false so that the effect is to size
		// the tour using the stacked layout, not to resize it.
		if (!resize && this.usingResponsiveStackedLayout && !switchToStackedLayout)
			this.setTourSize({ resize: false, switchToStackedLayout: true });

		//console.log(`Layout::setTourSize Width:${this.scaledTourW}px : ${this.responsiveTourScale * 100}%`);
	}

	showExtraLayouElement(show, elementId, display = 'block')
	{
		let element = this.tour.getElementByUniqueId(elementId)
		if (element)
			element.style.display = show ? display : 'none';
		return show ? element : null;
	}

	showExtraLayoutElements(showBanner, showTitleBar, showStripes, showFooter)
	{
		this.bannerElement = this.tour.hideBanner ? null : this.showExtraLayouElement(showBanner, 'Banner', 'flex');
		this.titleBarElement = this.showExtraLayouElement(showTitleBar, 'TitleBar', 'flex');
		this.headerStripeElement = this.showExtraLayouElement(showStripes, 'HeaderStripe');
		this.footerStripeElement = this.showExtraLayouElement(showStripes, 'FooterStripe');
		this.footerElement = this.showExtraLayouElement(showFooter, 'Footer', 'flex');
	}

	showHelpPanel(dataset)
	{
		let helpPanel = this.tour.getElementByUniqueId("HelpPanel");
		if (helpPanel === null)
			helpPanel = this.createHelpPanel();

		// Close the panel if the page button's help is currently showing.
		if (helpPanel.style.visibility === "visible" && parseInt(dataset.helpPageNumber, 10) === this.helpPanelPageNumber)
		{
			helpPanel.style.visibility = "hidden";
			helpPanel.style.left = 0;
			this.helpPanelPageNumber = 0;
			return;
		}

		// Adjust for the panel's left and right padding and border.
		let style = getComputedStyle(helpPanel);
		let borderWidth = parseInt(style.borderLeftWidth, 10) + parseInt(style.borderRightWidth, 10);
		let paddingWidth = parseInt(style.paddingLeft, 10) + parseInt(style.paddingRight, 10);
		let paddingAndBorder = borderWidth + paddingWidth;
		helpPanel.style.backgroundColor = dataset.helpBackgroundColor;
		helpPanel.style.color = dataset.helpColor;

		let helpTitleId = this.tour.uniqueId("HelpTitle");
		let helpTitle = document.getElementById(helpTitleId);
		helpTitle.innerHTML = dataset.helpTitle

		let helpContentId = this.tour.uniqueId("HelpContent");
		let helpContent = document.getElementById(helpContentId);

		// Decode the help's text which is encoded by the Tour Builder so that it
		// is stored in the page.js file pageProperties.help.text as HTML entities.
		helpContent.innerHTML = Runtime__$$.decodeHtml(dataset.helpText);

		// Get the width of the panel's content but don't let it exceed the panel's max width.
		let helpPanelRect = helpPanel.getBoundingClientRect();
		let helpPanelWidth = Math.min(helpPanelRect.width, parseInt(dataset.helpWidth, 10));

		// Make sure the panel is never the full width of the screen.
		const margin = 16;
		let layoutRect = this.layoutElement.getBoundingClientRect();
		if (helpPanelWidth >= layoutRect.width - margin)
			helpPanelWidth = layoutRect.width - margin;

		helpPanelWidth -= paddingAndBorder;
		helpPanel.style.maxWidth = helpPanelWidth + "px";

		// Horizontally center the panel based on the width of its content.
		let helpPanelLeft = Math.round((layoutRect.width / 2) - (helpPanelWidth / 2));
		helpPanelLeft -= Math.round(paddingAndBorder / 2);
		helpPanel.style.left = helpPanelLeft + "px";

		helpPanel.style.visibility = "visible";

		// Remember which page's help is showing.
		this.helpPanelPageNumber = parseInt(dataset.helpPageNumber, 10);
	}

	showHelpPanelForPage(pageSpecifier)
	{
		let pageNumber = this.tour.getPageNumber(pageSpecifier);
		if (pageNumber === 0)
			return;
		let page = this.tour.getPage(pageNumber);
		if (!page.showHelp)
			return;
		let help = page.help;
		let dataset = {
			helpTitle: help.title,
			helpText: help.text,
			helpPageNumber: pageNumber,
			helpWidth: help.width,
			helpBackgroundColor: help.bgColor,
			helpColor: help.color
		}
		this.showHelpPanel(dataset);
	}

	get textElement()
	{
		return this.page.hasPopup ? this.popupElements.text : this.tiledElements.text;
	}

	get textAreaElement()
	{
		return this.page.hasPopup ? this.popupElements.text : this.tiledElements.textArea;
	}

	get titleElement()
	{
		return this.page.hasPopup ? this.popupElements.title : this.tiledElements.title;
	}

	toggleShowNavPanel()
	{
		this.navPanel.classList.toggle('show');
	}

	get usingResponsiveLayout()
	{
		return this.usingMobileLayout || this.usingResponsiveStackedLayout;
	}

	validateNavButtonLocation()
	{
		// Handle conversion from V3.
		if (this.tour.navButtonLocation === this.NAV_V3_TOP_MENU)
			this.tour.navButtonLocation = this.tour.hasTitleBar ? this.NAV_TITLE_BAR : this.NAV_MAP_LEFT;

		// The Tour Builder now ensures that the nav button is not located in the banner, but
		// detect tours created during development that have an invalid nav button location. 
		if (this.navButtonLocationIsBanner && !this.tour.hasBanner)
			Runtime__$$.assert(false, "Nav button located in banner, but there is no banner");

		// When the nav button is located above the tour, but the tour is running standalone on
		// a mobile device where there is not room above, move it to the title bar or the map.
		if (this.usingMobileLayout && this.tour.isStandalone && this.navButtonLocationIsAbove)
			this.tour.navButtonLocation = this.titleBarElement ? this.NAV_TITLE_BAR : this.NAV_MAP_LEFT;

		this.navButtonIsOnMap = this.tour.hasNavPanel && (this.tour.navButtonLocation === this.NAV_MAP_LEFT || this.tour.navButtonLocation === this.NAV_MAP_RIGHT);
	}
}