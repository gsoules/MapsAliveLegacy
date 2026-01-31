// Copyright (C) 2009-2022 AvantLogic Corporation: https://www.mapsalive.com

export { MapsAliveTour };

import { MapsAliveRuntime as Runtime__$$ } from './mapsalive-runtime.js';
import { MapsAliveApi as Api__$$ } from './mapsalive-api.js';

class MapsAliveTour
{
	constructor(runtime)
	{
		console.log(`Tour__$$::contructor`);

		this.runtime = runtime;

		this.currentPage = null;
		this.directory = null;
		this.path = null;
		this.pages = [];

		// Set the instance Id to 1 until it's determined whether there are multiple instances of this tour.
		this.instanceId = 1;
		this.instanceName = "";

		this.hasCustomJs = false;
		this.insertingCustomJs = false;

		this.viewIdOfHotspotOnQueryString = 0;
		this.preview = false;
		this.reportError = true;
		this.bannerImg = null;
		this.bannerImgScaledHeight = 0;

		this.tourSettingNames = null;
		this.tourSettings = {};
		this.defineTourSettings();
		this.setTourSetting("disable-blending", true);

		this.setOptionFlags();

		// Create the API objects for this tour.
		this.api = new Api__$$(this);

		// Bind 'this' to event handlers.
		this.onTourMouseMove = this.onTourMouseMove.bind(this);
		this.onResizeWindow = this.onResizeWindow.bind(this);
		this.onResizeTour = this.onResizeTour.bind(this);
		this.onClick = this.onClick.bind(this);
		this.onError = this.onError.bind(this);
		this.selectMarkerAfterPageLoaded = this.selectMarkerAfterPageLoaded.bind(this);

		// Other properties will be set by MapsAliveRuntime. It will then call this object's propertiesHaveBeenSet() method.
	}

	addPage(page)
	{
		//console.log(`Tour::addPage ${page.pageName} to ${this.tourId}`);

		// Load the page's views immediately so that they will be available for building the directory.
		page.loadViewTable();

		this.pages.push(page);
	}

	get bannerImageHasLoaded()
	{
		if (this.editMode)
			return true;

		if (!this.hasBanner || this.hideBanner)
			return true;

		if (this.bannerImg && this.bannerImg.complete)
		{
			let loaded = this.bannerImg.height === this.bannerHeight;
			console.log(`Tour::bannerImageHasLoaded ${loaded}`);
			return loaded;
		}

		console.log(`Tour::bannerImageHasLoaded WAITING`);
		return false;
	}

	camelCase(input)
	{
		// Convert e.g. 'my-foo-bar' to 'myFooBar'.
		return input.toLowerCase().replace(/-(.)/g, function (match, group1)
		{
			return group1.toUpperCase();
		});
	}

	defineTourSettings()
	{
		this.tourSettingNames = [
			"disable-blending",
			"disable-caching",
			"disable-drawing",
			"flex-height",
			"flex-max-width",
			"flex-min-width",
			"flex-width",
			"instance",
			"mobile-mode",
			"page",
			"small-mobile",
			"stacked-layout"
		];
	}

	determineNewPage(allowOverride, pageNumber)
	{
        let newPage;

		if (allowOverride && !this.editMode)
		{
			let useQueryStringPageNumber = this.preview || this.isStandalone;
			let overridePageNumber;

			if (useQueryStringPageNumber)
			{
				// See if there's a 'page' arg in the query string.
				overridePageNumber = parseInt(Runtime__$$.getQueryStringArg('page'), 10);
				if (isNaN(overridePageNumber)) {
					// There's no query string `page` number.
					useQueryStringPageNumber = false;
				}
			}

			if (!useQueryStringPageNumber)
			{
				// Try to get the page number from the tour element's `data-page` attribute.
				overridePageNumber = this.getTourSettingInt('page');
			}

            newPage = isNaN(overridePageNumber) ? this.getPage(pageNumber) : this.getPage(overridePageNumber);
        }
		else
		{
            // Override is not allowed. Use the passed in page number.
            newPage = this.getPage(pageNumber);
        }

		if (!newPage)
		{
            // If the override page number is invalid, use the tour's default page.
            newPage = this.getPage(this.firstPageNumber);
        }

		if (this.currentPage && newPage.pageLoaded)
		{
			// The new page is already loaded which means it has been previously displayed.
			// Set the current view to what it was when the new page was last displayed.
			this.currentPage.currentView = newPage.getView(newPage.map.selectedMarkerViewId);
		}

        return newPage;
    }

	displayPage(pageNumber, allowOverride = true)
	{
		if (this.currentPage)
		{
			// Do nothing if the new page is the current page.
			if (pageNumber === this.currentPage.pageNumber)
				return;

			// Remove the class that indicates the current page number.
			if (!this.editMode)
				this.tourElement.classList.remove(`maPage-${this.currentPage.pageNumber}`);

			this.currentPage.hidePage();
		}

		this.currentPage = this.determineNewPage(allowOverride, pageNumber);
		//console.log(`Tour::displayPage ${this.currentPage.pageName}`);

		// Add a class that indicates the current page number so that it's possible
		// to write CSS to refer to a tour or layout element for a specific page.
		if (!this.editMode)
			this.tourElement.classList.add(`maPage-${this.currentPage.pageNumber}`);

		this.updatePageArgInQueryString();

		// Let the user JavaScript know if the page is being displayed for the first time.
		if (!this.currentPage.pageLoaded)
			this.api.callbackPageLoading();

		// Start an asyncronous load of the page assets, namely the map image. This used to take longer when
		// the map was composed of individual map tiles that had to each be loaded, but a large map image can
		// take time too. The call to waitForPageToLoad returns immediately because it only initiates loading
		// of the map image, but does not wait for the image to load.
		this.currentPage.waitForPageToLoad();
	}

	elementHasNoId(e)
	{
		return e && (typeof e.id === 'undefined' || e.id.length === 0);
	}

	getDeviceOrientation()
	{
		// This method is not currently being used. It was tested and works with Safari on iOS
		// but gets an error on Safari on Big Sur because window.screen.orientation does not
		// exist there. If this method is needed in the future, test it thoroughly.
		//
		// The window.orientation method is deprecated but is supported by iOS and all mobile
		// browsers as of 1/2022. The window.screen object is not supported by iOS, so first
		// check for window.orientation in case the tour is on iOS, otherise use window.screen.
		if (window.hasOwnProperty('orientation'))
			return window.orientation;
		return window.screen.orientation.angle;
	}

	getElementByUniqueId(id)
	{
		return document.getElementById(this.uniqueId(id));
	}

	getPage(pageNumber)
	{
		for (const page of this.pages)
		{
			if (page.pageNumber === pageNumber)
				return page;
		}
		return null;
	}

	getPageNumber(pageSpecifier)
	{
		let pageNumber = parseInt(pageSpecifier, 10);

		if (isNaN(pageNumber))
		{
			if (typeof pageSpecifier === "string")
			{
				for (const page of this.pages)
				{
					// page.mapId is used for maps, galleries, and data sheets.
					if (pageSpecifier === page.mapId)
						return page.pageNumber;
				}
			}
			return 0;
		}

		if (pageNumber < 0)
			pageNumber = 0;

		return pageNumber;
	}

	getTourSetting(name, defaultValue = null)
	{
		if (this.editMode)
			return "";

		// Check if the setting was set in any of the following ways:
		// -	data-* attribute in HTML embed code
		// -	query string
		// -	api.setTourSetting method
		//
		// The first setting found takes precedence over any other. This allows an embed or query
		// string setting to apply instead of one set in the tour's JavaScript. Thus, the JavaScript
		// setting will only be used when there's no data-* attribute or query string setting.

		let camelCaseName = this.camelCase(name);
		if (camelCaseName in this.tourElement.dataset)
			return this.tourElement.dataset[camelCaseName];

		let value = Runtime__$$.getQueryStringArg(name);
		if (value)
			return value;

		if (name in this.tourSettings)
			return this.tourSettings[name];

		return defaultValue;
	}

	getTourSettingBool(name)
	{
		let value = this.getTourSetting(name);
		if (typeof value === 'boolean')
			return value;
		if (typeof value === 'string')
			value = value.toLowerCase();
		if (value === 'true')
			return true;
		if (value === 'false')
			return false;
		return null;
	}

	getTourSettingInt(name, defaultValue = null)
	{
		let value = parseInt(this.getTourSetting(name), 10);
		if (isNaN(value) && defaultValue !== null)
			value = defaultValue;
		return value;
	}

	getViewFromAnyPage(viewId)
	{
		for (const page of this.pages)
		{
			let view = page.getView(viewId);
			if (view)
				return view;
		}
		Runtime__$$.assert(view !== null, `No view found on any page for '${viewId}'`);
		return null;
	}

	goToPage(pageNumber)
	{
		this.displayPage(pageNumber, false);
	}

	goToPageView(pageNumber, viewId)
	{
		// This method is called from the directory when the user selects a hotspot that's on a
		// different page than the current page. Go to the new page making sure that the slide
		// show won't start running, and select the hotspot as though the user tapped its marker.
		let newPage = this.getPage(pageNumber);
		newPage.runSlideShow = false;
		newPage.hotspotSelectedFromDirectory = viewId;
		this.goToPage(pageNumber);

		// Select the view's marker but only after the page has loaded.
		setTimeout(this.selectMarkerAfterPageLoaded, 0, viewId);
	}

	insertCustomJsIntoDom(js)
	{
		//console.log(`Tour__$$::insertCustomJsIntoDom`);
		this.insertingCustomJs = true;

		// Creates a <script> element and set its inner HTML to be the JavaScript.
		let scriptElementForCustomJs = document.createElement('script');
		scriptElementForCustomJs.id = `ma-${this.instanceId}-${this.tourId}-CustomJs`;
		scriptElementForCustomJs.innerHTML = js;

		// Insert the <script> element into the DOM.
		this.insertElementIntoDomAfterScriptElementforTour(scriptElementForCustomJs);

		this.insertingCustomJs = false;
	}

	insertElementIntoDomAfterScriptElementforTour(elementToInsert)
	{
		let scriptElementForTour = document.getElementById(this.uniqueScriptElementId());
		if (!scriptElementForTour)
		{
			alert(`No <script id="${this.uniqueScriptElementId()}" found`);
			return;
		}

		// Insert the element into the DOM right after the <script> element that
		// is used to embed the tour. Since there is no JavaScript insertAfter function,
		// use insertBefore with nextSibling to achieive the same effect.
		scriptElementForTour.parentNode.insertBefore(elementToInsert, scriptElementForTour.nextSibling);
	}

	insertTourCssIntoDom(css)
	{
		//console.log(`Tour::insertTourCssIntoDom`);

		// Replace Ids in the CSS to use the current instance Id.
		if (this.instanceId > 1)
			css = this.updateInstanceIdCss(css);

		// Creates a <style> element and set its inner HTML to be the CSS.
		let styleElement = document.createElement('style');
		styleElement.id = `ma-${this.instanceId}-${this.tourId}-Style`;
		styleElement.innerHTML = css;

		// Insert the <style> element into the DOM.
		this.insertElementIntoDomAfterScriptElementforTour(styleElement);
	}

	insertTourHtmlIntoDom(html)
	{
		//console.log(`Tour::insertTourHtmlIntoDom`);

		// Update the default intance Id of 1 with this tour's instance Id.
		if (this.instanceId > 1)
			html = this.updateInstanceIdHtml(html);

		// Insert the tour's HTML into the DOM.
		this.tourElement.innerHTML = html;
	}

	interceptConsoleMessages(id)
	{
		console.log(`Tour::interceptConsoleMessages (see on screen console for messages)`);

		// Replace the system console function with our own.
		/**/console.log = function (message)
		{
			const maxLines = 10;
			const endMark = ` [${id}]`;

			// Get the <div> for our own console or create one if it does not yet exist.
			let e = document.getElementById('maConsole');
			if (e === null)
			{
				e = document.createElement('div');
				e.id = 'maConsole';
				let s = e.style;
				s.left = '4px';
				s.top = '5vh';//`calc(100vh - 300px)`;
				s.height = `auto`;
				s.width = '300px'
				s.position = 'fixed';
				s.overflow = 'scroll';
				s.zIndex = '10000';
				s.padding = '6px 6px 8px 8px'
				s.fontSize = '11px';
				s.fontFamily = 'sans-serif';
				s.backgroundColor = '#000';
				s.color = '#000';
				s.backgroundColor = '#fff';
				s.opacity = '0.8';
				document.body.insertAdjacentElement('afterbegin', e);
			}

			// Get the contents of the console and break it into line after first removing the end mark.
			let text = e.innerHTML;
			text = text.replace(endMark, '');
			let lines = text.split('<br>');
			let start = lines.length > maxLines ? 1 : 0;
			let html = '';

			// Emit the most recent lines and ignore older ones.
			for (let i = start; i < lines.length; i++)
			{
				let line = lines[i];
				if (line.length === 0)
					continue;
				html += `${line}<br>`;
				if (i >= maxLines)
					break;
			}

			// Write the lines to the console followed by the end mark so that the
			// developer knows they are seeing the most recent last console message.
			e.innerHTML = `${html}${message}${endMark}`;
		}
	}

	get isClassicTour()
	{
		return !this.isFlexMapTour;
	}

	get isEmbedded()
	{
		return !this.isStandalone;
	}

	get isSmallMobileDevice()
	{
		let smallMobileHeight = this.getTourSettingInt('small-mobile', 1000);
		return this.isMobileDevice && window.screen.height < smallMobileHeight;
	}

	linkToUrl(url)
	{
		console.log("Tour::linkToUrl '%s'", url);

		if (typeof url === "undefined" || url === null)
		{
			return;
		}

		try
		{
			let openInNewWindow = url.substring(0, 1) === "1";
			url = url.substring(1);

			// Make sure the url starts with http or with a relative reference i.e. '..'.
			if (url.substring(0, 1) !== "." && !url.match(/^https?:\/\//i))
			{
				url = 'https://' + url;
			}

			if (openInNewWindow)
			{
				let wnd = window.open(url, "_blank");
				if (wnd === null || typeof wnd === "undefined")
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
	}

	loadGif(id, name)
	{
		this.loadImg(id, name, "gif");
	}

	loadingGraphic()
	{
		return "<img width='32px' height='32px' src='" + this.graphics["loading"].src + "'/>";
	}

	loadImg(id, name, ext)
	{
		//console.log(`Tour::loadImg ${this.path}`);
		let image = new Image();
		let version = this.version;
		image.src = this.path + "00" + id + "_" + version + "." + ext;
		this.graphics[name] = image;
	}

	loadTour(html, css)
	{
		// This method is only called for actual tours. It is not called when this.editMode is true.

		//console.log(`Tour::loadTour`);

		this.insertTourCssIntoDom(css);
		this.insertTourHtmlIntoDom(html);

		// Determine if in Tour Preview by checking window.maTourPreviewPath which is set true by TourPreview.aspx.
		this.preview = typeof window.maTourPreviewPath !== 'undefined';

		if (this.preview)
		{
			this.path = window.maTourPreviewPath;
		}
		else
		{
			// Get the path to the tour folder that contains the tour. It's the folder containing the loader script.
			let scriptElement = document.getElementById(this.uniqueScriptElementId());
			let src = scriptElement.src;
			this.path = src.substring(0, src.lastIndexOf("/")) + '/';
		}

		this.tourElementId = this.uniqueId('Tour');

		this.viewId = 0;
		this.tid = 1;
		this.dirPreviewView = null;
		this.okayToPreloadNextImage = true;

		// Create the banner img element and set its src so that the image starts loading.
		// The tour won't finish loading until the banner image load has completed.
		if (this.hasBanner && !this.hideBanner)
		{
			let bannerElement = document.getElementById(this.uniqueId('Banner'));
			if (bannerElement)
			{
				this.bannerImg = bannerElement.querySelector('img');
				this.bannerImg.src = `${this.path}${this.bannerImageSrc}`;
			}
		}

		// Listen for events. Resive events only fire on the window object.
		window.addEventListener('resize', this.onResizeWindow, false);
		window.addEventListener('click', this.onClick, false);
		window.addEventListener('error', this.onError, false);

		if (!this.isTouchDevice)
		{
			this.tourElement.addEventListener('mousemove', this.onTourMouseMove, false);
		}
	}

	loadTourGraphics()
	{
		this.graphics = new Object();

		this.loadImg(95, "zoomIn", "svg");
		this.loadImg(96, "zoomOut", "svg");

		if (this.editMode)
			return;

		this.loadGif(21, "loading");
		this.loadImg(29, "closeHelpX", "png");
		this.loadImg(82, "popupCloseX", "png");
		this.loadImg(87, "contentExpand", "svg");
		this.loadImg(88, "contentContract", "svg");
		this.loadImg(89, "navButton", "svg");
		this.loadImg(90, "mobileCloseX", "svg");
		this.loadImg(91, "dirContract", "svg");
		this.loadImg(92, "dirExpand", "svg");
		this.loadImg(93, "dirSearch", "svg");
		this.loadImg(94, "currentPage", "svg");
		this.loadImg(97, "offline", "svg");
		this.loadImg(98, "helpButton", "svg");

		// Now that the graphics sources are available, it's okay to create
		// the Busy class that displays the loading and offline indicators.
		this.busy = new MapsAliveBusy(this);
	}

	onClick(event)
	{
		// Ignore a click or tap on the map since it will have been handled by MapsAliveMap.
		if (event.srcElement.id === this.uniqueId('MarkerLayer'))
			return;

		// Currently nothing happens when clicking outside of the map. For a while there was
		// logic to close a popup, but that conflicted with use of the API's setMarkerSelected
		// function if it was called in response to clicking an off-map element like a button
		// in order to select a marker and show its popup. The popup was shown and then
		// immediately closed by this handler.
	}

	onError(event)
	{
		if (this.reportError && (this.api.reportJavaScriptErrors || this.preview))
		{
			this.reportError = false;

			let explanation = "";
			if (this.hasCustomJs)
			{
				if (this.insertingCustomJs)
				{
					let line = "";
					if (event.lineno)
						line = ` near line ${event.lineno}`;
					explanation = `\n\nThe error is in this tour's Custom JavaScript${line}.`;
				}
				else
				{
					explanation = `\n\nThe error could be due to a problem in this tour's Custom JavaScript`;
				}
			}
			console.error(event.error.stack);
			alert(`MapsAlive detected a JavaScript error:\n\n${event.error.message}${explanation}\n\nClick OK, then see the developer console for details.`);
		}
	}

	onResizeTour(count)
	{
		console.log(`Tour::onResizeTour [${count}]`);

		// Ignore the event if the user has touched inside the directory search box (text input element).
		// This is necesary on Android devices which trigger a resize event when the soft keyboard appears.
		if (document.activeElement.type === 'text')
			return;

		this.currentPage.map.stopCurrentOperation();

		// Ensure that the tour is positioned at the top of the screen, especially after the device has been rotated.
		// Most browsers do this automatically but not Safari on iOS 14 (works okay on 15). On Safari, often after
		// a rotate, the page is partially scrolled up which, without the code below, puts the top of the tour above
		// the top of the screen and under the toolbar if one is showing.
		if (this.isSmallMobileDevice && this.isStandalone)
			window.scrollTo(0, 0);

		// Peform the resize.
		this.currentPage.layout.setTourSize({ resize: true });

		// Ensure that if the resize occurred because the device was rotated, that the pan position is valid for the
		// new orientation. This handles the case where before the rotation the device is in landscape mode and the
		// map is overpanned to show a slideout, then when the device is rotated, the overpan position is not valid
		// for portrait orientation.
		this.currentPage.map.validateAndUpdatePanXandPanY(0, 0);
		this.currentPage.map.drawMap();

		console.log(`Tour::onResizeTour [${count}] DONE ScrollY:${window.scrollY} Size:${this.deviceSize.h}`);
	}

	onResizeWindow()
	{
		//console.log(`Tour::onResizeWindow`);

		if (!navigator.onLine)
			return;

		this.onResizeTour(1);

		if (!this.isMobileDevice || this.flagDisableResizeDelay)
			return;

		// Perform the resize again to address an issue with Safari on iOS 14 where sometimes rotating from landscape
		// to portrait causes the top toolbar to show minified and sometimes show full height. In the minified case,
		// the tour is sometimes getting the shorter device height when it should be getting the taller height. To
		// workaround this, performing the resize again seems to get the correct height. Note that the cound arg
		// passed to onResizeTour is there only for debugging purposes.
		setTimeout(this.onResizeTour, 500, 2);
	}

	onTourMouseMove(event)
	{
		// This handler is called when the mouse is over any portion of the tour except
		// the map. Mouse moves over the map are handled by MapsAliveMap::onMapMouseMove.

		//console.log(`Tour::onTourMouseMove ${event.type}`);

		// Move the preview panel as the mouse moves.
		if (this.showingPreview)
		{
			let mouse = { x: event.clientX, y: event.clientY};
			this.directory.movePreviewPanel(mouse);
		}
	}

	propertiesHaveBeenSet()
	{
		// This method is called after MapsAliveRuntime has contructed this object and
		// set its properties with data sent from the Tour Builder in JavaScript files.

		this.isAndroid = this.userAgentIs("android");
		this.isMobileDevice = this.userAgentIs("mobi") || this.isAndroid;
		this.isTouchDevice = 'ontouchstart' in window || navigator.maxTouchPoints > 1;
		this.isMouseDevice = matchMedia('(pointer:fine)').matches;
		this.isTouchDeviceWithoutMouse = this.isTouchDevice && !this.isMouseDevice;

		if (this.flagInterceptConsole)
			this.interceptConsoleMessages(this.buildId);

		console.log(`isTouchDevice:${this.isTouchDevice} isMobileDevice:${this.isMobileDevice} isMouseDevice:${this.isMouseDevice}`);

		this.useTouchCss = this.isTouchDevice || this.useTouchUiOnDeskop;

		// Support unbranded tours by hiding the banner when requested.
		this.hideBanner = this.canAppearUnbranded && this.queryStringFlags.has('nobanner');

		if (this.enlargeHitTestArea || this.flagShowSmallMarkers)
		{
			// The user has chosen the option to enlarge the hit test area.
			// Disable it when in edit mode and when not using a touch device.
			if ((this.editMode || !this.isTouchDevice) && !this.flagShowSmallMarkers)
				this.enlargeHitTestArea = false;
		}
	
		this.disableResponsive = this.flagToggleResponsive;
		this.v4 = !this.enableV3Compatibility;
	}

	selectMarkerAfterPageLoaded(viewId)
	{
		console.log(`Tour::selectMarkerAfterPageLoaded ${viewId} ${this.currentPage.pageLoaded}`);

		// This method only gets called from goToPageView which only gets called when the user selects a
		// hotspot that's on a different page than the current page. Before that hotspot's marker can be
		// selected, the page's map has to load so that it can be auto-panned, if necessary, to make the
		// marker visible. This method gets called repeatedly on a timer until the page has loaded.
		if (this.currentPage.pageLoaded)
		{
			this.currentPage.map.selectMarkerChosenFromDirectory(viewId);
			return;
		}
		setTimeout(this.selectMarkerAfterPageLoaded, 100, viewId);
	}

	setBannerStyle()
	{
		if (this.bannerImg === null)
			return;

		// Set the banner image to its scaled height. Setting the max height ensures that the browser will
		// allocate just enough vertical space for the image. If instead the width were set to 100%, the
		// full height image would first render and then the space would be collapsed vertically to the
		// correct height for the image's aspect ratio, but you would see the jump in image size.
		let ratio = this.bannerHeight / this.width;
		this.bannerImgScaledHeight = Math.ceil(ratio * this.currentPage.layout.scaledTourW);
		this.bannerImg.style.maxHeight = this.bannerImgScaledHeight + 'px';
	}

	setOptionFlag(id, enabled)
	{
		// Use the query string to enable a disabled flag or disable an enabled flag.
		let onQueryString = this.queryStringFlags.has(id);

		if (onQueryString)
		{
			// When the flag is on the query string and set to 0 or 1, use the setting.
			let flag = Runtime__$$.getQueryStringArg(id);
			if (flag === '0')
				return false;
			else if (flag === '1')
				return true;
		}

		// Either the flag is not on the query string, or it's there, but is not set to 0 or 1.
		if (enabled)
			return !onQueryString
		else
			return onQueryString;
	}

	setOptionFlags()
	{
		// One or more option flags can be enabled or disabled via the browser by adding it as a query
		// string parameter.  Specifying a flag by itself, e.g. &7, or setting it with a value of 1 e.g.
		// &7=1, sets the flag to true, but giving it a value of 0, e.g. &7=0 sets it to false. Thus by
		// using values of 0 or 1 you can override flags that the user set via the API. During development,
		// it's ok to enable a flag by setting its enable parameter to true, but turn it off for production.

		// Get the flags that are specfied on the query string.
		this.queryStringFlags = new URLSearchParams(window.location.search);

		// These flags are used for debugging and do not have corresponding MapsAlive.Api options.
		// The flag names are just digits to make them easier to type on mobile device keyboards.
		this.flagTrackPointer = this.setOptionFlag('1', false);
		this.flagShowMarkerBounds = this.setOptionFlag('2', false);
		this.flagShowMarkerClippingRect = this.setOptionFlag('3', false);
		this.flagDisableMapResample = this.setOptionFlag('4', false);
		this.flagDisableMapSharpen = this.setOptionFlag('5', false);
		this.flagDisableResizeDelay = this.setOptionFlag('6', false);
		this.flagInterceptConsole = this.setOptionFlag('7', false);
		this.flagToggleResponsive = this.setOptionFlag('8', false);
		this.flagDisableTourPreviewLiveData = this.setOptionFlag('9', false);
		this.flagShowDeviceEvents = this.setOptionFlag('10', false);
		this.flagShowMarkerViewId = this.setOptionFlag('11', false);
		this.flagDisableOverpanOptimization = this.setOptionFlag('12', false);
		this.flagDisablePreloadImages = this.setOptionFlag('13', false);
		this.flagShowCrosshairsForMarkerBeingEdited = this.setOptionFlag('14', false);
		this.flagShowSmallMarkers = this.setOptionFlag('15', false);
	}

	setTourSetting(name, value)
	{
		let settingName = name.toLowerCase();
		if (!this.tourSettingNames.includes(settingName))
		{
			alert(`"${name}" is not a supported Tour Setting`);
			return;
		}

		// Save the setting.
		this.tourSettings[settingName] = value;
	}

	showMarker(viewId, show)
	{
		if (this.currentPage.hasPopup && this.currentPage.popup.isPinned)
			return;

		this.currentPage.map.stopSlideShow();

		this.currentPage.closePopup();
		this.currentPage.map.showMarkerSelected(viewId, show);
	}

	sortHotspotsByTitle(a, b)
	{
		// This function is used to sort views by their title when passed to Array.sort().
		let titleA = a.title.toLowerCase();
		let titleB = b.title.toLowerCase();
		if (titleA < titleB)
			return -1;
		if (titleA > titleB)
			return 1;
		return 0;
	}

	// Eventually replace all uses of this method with the simpler MapsAliveRuntime.elementLocation method.
	tagLocation(outerTagId, innerTagId)
	{
		let e = document.getElementById(innerTagId);
		let left = 0;
		let top = 0;
		while (e && (outerTagId === null || e.id !== outerTagId))
		{
			left += e.offsetLeft;
			top += e.offsetTop;
			e = e.offsetParent;
		}
		let pt = new Object();
		pt.x = left;
		pt.y = top;
		return pt;
	}

	uniqueId(id = '')
	{
		// Return an Id that is unique to the tou.
		let prefix = `ma-${this.instanceId}-${this.tourId}`;
		if (id)
			return `${prefix}-${id}`;
		else
			return prefix;
	}

	uniqueScriptElementId()
	{
		return `ma-${this.tourId}`;
	}

	updateInstanceIdCss(text)
	{
		return text.replace(/#ma-1-/g, `#ma-${this.instanceId}-`);
	}

	updateInstanceIdHtml(text)
	{
		return text.replace(/id="ma-1-/g, `id="ma-${this.instanceId}-`);
	}

	updatePageArgInQueryString()
	{
		// Ignore this request if the tour is embedded. Since a tour in Tour Preview is actually
		// an embedded tour, the test needs to allow this method to work when in tour Preview.
		if (!this.preview && this.isEmbedded)
			return;

		let url = window.location.pathname;
		let args = new URLSearchParams(window.location.search);

		args.set('page', this.currentPage.pageNumber);

		if (args.toString().length)
			url += `?${ args }`;
		history.replaceState(null, null, url);
	}

	uniqueTourInstanceId()
	{
		return `${this.instanceId}-${this.tourId}`;
	}

	userAgentIs(ua) 
	{
		let answer = navigator.userAgent.toLowerCase().indexOf(ua);
		return answer !== -1;
	}
}

class MapsAliveBusy
{
	constructor(tour)
	{
		this.tour = tour;

		this.createWaitIndicatorElement();
		this.initializeWaitState();

		this.WAIT_IMAGE = 1;
		this.WAIT_DATA = 2;

		this.onShowWaitIndicator = this.onShowWaitIndicator.bind(this);
	}

	cancelTimer()
	{
		clearTimeout(this.timerId);
		this.timerId = 0;
	}

	createWaitIndicatorElement()
	{
		this.waitIndicatorElement = document.createElement("div");
		this.waitIndicatorElement.id = this.tour.uniqueId('WaitIndicator');
		this.waitIndicatorElement.className = "maWaitIndicator";
		this.waitIndicatorElement.style.visibility = "hidden";

		this.imgWait = document.createElement('img');
		this.imgWait.id = this.tour.uniqueId('maWait');
		this.imgWait.src = this.tour.graphics['loading'].src;
		this.imgWait.style.display = 'none';
		this.waitIndicatorElement.appendChild(this.imgWait);

		this.imgOffline = document.createElement('img');
		this.imgOffline.src = this.tour.graphics['offline'].src;
		this.imgOffline.id = this.tour.uniqueId('maOffline');
		this.imgOffline.style.display = 'none';
		this.waitIndicatorElement.appendChild(this.imgOffline);

		document.body.appendChild(this.waitIndicatorElement);
	}

	hideWaitIndicator()
	{
		//console.log(`Busy::hideWaitIndicator`);

		if (!this.showingWaitIndicator)
			return;

		// This method normally gets called on a timer, but it can also be called directly, so if a
		// timer is set, cancel it so that the indicator won't appear again when the timer expires.
		this.cancelTimer();

		this.waitIndicatorElement.style.visibility = "hidden";
		this.imgWait.style.display = 'none';
		this.imgOffline.style.display = 'none';
		this.showingWaitIndicator = false;
	}

	initializeWaitState()
	{
		this.viewId = 0;
		this.waitingFor = { image: false, data: false };
		this.timerId = 0;
		this.showingWaitIndicator = false;
	}

	moveWaitIndicatorToCoordinates(pointer)
	{
		//console.log(`Busy::moveWaitIndicatorToCoordinates ${pointer.x},${pointer.y}`);

		// Get the offset of the preview panel centered within the browser and use it to adjust the tooltip location.
		let offsetX = 0;
		if (this.tour.preview)
			offsetX = Runtime__$$.elementLocation(document.getElementById('PreviewPanel')).x;

		// Get the offset of the map within the browser.
		let rect = this.tour.currentPage.layout.mapElement.getBoundingClientRect();

		// Move the indicator to the adjusted location relative to the pointer position.
		let s = this.waitIndicatorElement.style;
		s.left = Math.round(pointer.x + rect.left + window.scrollX + 8 - offsetX) + 'px';
		s.top = Math.round(pointer.y + rect.top + window.scrollY - 12) + 'px';
	}

	onShowWaitIndicator()
	{
		// Ignore this call if waiting has stopped. This can happen if the call was
		// made on a timer and waiting stopped before the timer was cancelled.
		if (!this.waiting)
			return;

		console.log(`Busy::onShowWaitIndicator ${this.viewId}`);

		this.showingWaitIndicator = true;
		this.cancelTimer();

		let marker = this.tour.currentPage.map.getMarker(this.viewId);
		let bounds = marker.getBounds();
		let offsetX = 16;
		let offsetY = 16;
		this.moveWaitIndicatorToCoordinates({ x: bounds.centerX_screen - offsetX, y: bounds.cornerY_screen - offsetY });

		if (navigator.onLine)
			this.imgWait.style.display = 'block';
		else
			this.imgOffline.style.display = 'block';

		this.waitIndicatorElement.style.visibility = "visible";
	}

	setWaitState(what, waiting)
	{
		if (what === this.WAIT_IMAGE)
			this.waitingFor.image = waiting;
		else if (what === this.WAIT_DATA)
			this.waitingFor.data = waiting;
	}

	showWaitIndicator()
	{
		// Ignore this request if the indicator is already showing;
		if (this.showingWaitIndicator || this.timerId != 0)
			return;

		// Delay just a bit before showing the indicator for the situation where the content
		// being waited on becomes instantly available. This way the user won't see the icon
		// appear and then immediately dissappear.
		this.timerId = setTimeout(this.onShowWaitIndicator, 100);
	}

	startWaitingFor(what, viewId)
	{
		this.viewId = viewId;
		this.setWaitState(what, true);
		//console.log(`Busy::startWaitingFor ${this.what(what)} ${this.waitStatus}`);
		this.showWaitIndicator();
	}

	startWaitingForData(viewId)
	{
		this.startWaitingFor(this.WAIT_DATA, viewId);
	}

	startWaitingForImage(viewId)
	{
		this.startWaitingFor(this.WAIT_IMAGE, viewId);
	}

	stopWaiting()
	{
		//console.log(`Busy::stopWaiting ${this.waitStatus}`);
		this.hideWaitIndicator();
		this.initializeWaitState();
	}

	stopWaitingFor(what, viewId)
	{
		console.log(`Busy::stopWaitingFor ${this.what(what)} ${this.waitStatus}`);

		// Ignore the request if no longer waiting for the view. This can happen if, for example,
		// the user mouses off a marker for which Live Data has been requested, and then when the
		// data arrives, this methods gets called to stop waiting for it.
		if (!this.waiting || viewId !== this.viewId)
			return;

		this.setWaitState(what, false);

		if (!this.waiting)
		{
			this.viewId = 0;
			this.hideWaitIndicator();
		}
	}

	stopWaitingForData(viewId)
	{
		this.stopWaitingFor(this.WAIT_DATA, viewId);
	}

	stopWaitingForImage(viewId)
	{
		this.stopWaitingFor(this.WAIT_IMAGE, viewId);
	}

	get waiting()
	{
		return this.waitingFor.image || this.waitingFor.data;
	}

	waitingForData(viewId)
	{
		return viewId === this.viewId && this.waitingFor.data;
	}

	waitingForImage(viewId)
	{
		return viewId === this.viewId && this.waitingFor.image;
	}

	get waitStatus()
	{
		return `view:${this.viewId} image:${this.waitingFor.image} data:${this.waitingFor.data}`
	}

	what(what)
	{
		if (what === this.WAIT_IMAGE)
			return 'IMAGE';
		else if (what === this.WAIT_DATA)
			return 'DATA';

		return `Unexpected what: ${what}`;
	}
}
