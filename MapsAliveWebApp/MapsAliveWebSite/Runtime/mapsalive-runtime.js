// Copyright (C) 2009-2022 AvantLogic Corporation: https://www.mapsalive.com

export { MapsAliveRuntime };

import { MapsAliveDirectory as Directory__$$ } from './mapsalive-directory.js';
import { MapsAlivePage as Page__$$ } from './mapsalive-page.js';
import { MapsAlivePopup as Popup__$$ } from './mapsalive-popup.js';
import { MapsAliveTour as Tour__$$ } from './mapsalive-tour.js';

class MapsAliveRuntime
{
	constructor(tourId)
	{
		console.log(`Runtime__$$::contructor for tour ${tourId}`);
		this.tourId = tourId;
		this.runtimeTourAlias = "";

		// Keep track of all the tours in the containing web page that have the same tour Id. Each of these
		// tour instances share this runtime instance. Most of the time there will be only one tour in a
		// containing page, and when there is more than one, the tours will usually be different. However,
		// the runtime needs to be able to handle multiple instances of the same tour.
		this.tours = [];
	}

	addTour(tour)
	{
		this.tours.push(tour);
	}

	static assert(condition, description)
	{
		if (condition)
			return true;
		console.trace(`>>> ASSERT FAILED: ${description}`);
		debugger;
		return false;
	}

	browserSupportsMapsAlive()
	{
		let message = "Your browser does not support MapsAlive. "
		message += "Please switch to the latest version of Chrome, Microsoft Edge, Firefox, or Safari."
	
		// Test if the browser supports CSS Grid. If so, it will support Flex. 
		if (!('grid' in document.body.style))
		{
			alert(message);
			return false;
		}

		let ua = navigator.userAgent;
		if (ua.indexOf("MSIE ") > -1 || ua.indexOf("Trident/") > -1)
		{
			alert(message);
			return false;
		}

		return true; 
	}

	static closePopups(tour)
	{
		// Close any popup that is open for another tour. This ensures that when multiple tours
		// are embedded in the same page, only one popup can be open at a time. This prevents the
		// various problems that can manisfest themselves if multiple overlapping popups are open.
		// In particular, with more than one popup, there is the issue of z-index stacks and the
		// fact that each popup can have three elements (panel, close-X, and callout arrow) that
		// can end up stacking such that parts of one popup are above or below parts of the other.
		// To allow two popups open at the same time, there would need to be a way to ensure that
		// all of the parts of the most recently opened popup were all above the other popup.
		for (const runtime of window.MapsAlive._runtimes)
		{
			for (const otherTour of runtime.tours)
			{
				if (otherTour.tourId === tour.tourId && otherTour.instanceId === tour.instanceId)
					continue;
				otherTour.currentPage.closePopup();
			}
		}
	}

	createDirectory(tour, pagesData, directoryProperties)
	{
		// In V4, the menu and directory are both contained in the nav panel whereas in V3, they were separate.
		// The logic for the directory is in the Directory class, and the logic for the menu is in the Layout

		let allPagesExcludedFromNavigation = true;
		let showHelpPanel = false;

		// Set the Directory properities that come from the Tour Builder.
		tour.directory = new Directory__$$(tour);
		for (const property in directoryProperties)
			tour.directory[property] = directoryProperties[property];

		for (const data of pagesData)
		{
			let page = data.page;
			if (!page.excludeFromNavigation)
				allPagesExcludedFromNavigation = false;

			// If at least one page shows help, then the help panel can be shown.
			if (page.showHelp)
				showHelpPanel = true;
		}

		if (allPagesExcludedFromNavigation)
			tour.hasNavPanel = false;
		else if (tour.hasDirectory)
			tour.hasNavPanel = true;
		else if (tour.hideMenu)
			tour.hasNavPanel = false;
		else
			tour.hasNavPanel = pagesData.length >= 2 || showHelpPanel;
	}

	static createSizeObject(w = 0, h = 0)
	{
		return { w, h };
	}

	createTourInstance(instanceId, instanceElement, tourProperties, directoryProperties, pagesData, html = null, css = null, js = null)
	{
		console.log(`Runtime::createTourInstance ${tourProperties.tourId}`);

		let tour = new Tour__$$(this);

		for (const property in tourProperties)
			tour[property] = tourProperties[property];

		tour.tourElement = instanceElement;
		tour.propertiesHaveBeenSet();
		tour.instanceId = instanceId;
		this.createTourInstanceIdentifier(tour);

		if (tour.editMode)
		{
			tour.path = tourFolderPath;
		}
		else
		{
			this.createDirectory(tour, pagesData, directoryProperties);

			instanceElement.id = tour.uniqueId('Tour');

			// Add the generic maTour class to this instance.
			instanceElement.classList.add('maTour');

			// Determine if the tour is standalone or embedded. A standalone tour is one created by MapsAlive that can
			// be run standalone from tour.mapsalive.com (a tour inside a <main> tag), or it's an embedded tour having
			// the <body> tag as its parent element as is the case for a tour that is copy-pasted from the code-snippets.
			tour.isStandalone =
				tour.tourElement.parentElement.nodeName === 'MAIN' &&
				tour.tourElement.parentElement.parentElement === document.body;
			if (!tour.isStandalone)
				tour.isStandalone = tour.tourElement.parentElement === document.body;

			// Replace any occurrences of the TOUR_INSTANCE macro with this tour's instance name.
			html = html.replaceAll("TOUR_INSTANCE", tour.instanceName);

			// Insert the tour's CSS and HTML into the DOM.
			tour.loadTour(html, css);
		}

		tour.loadTourGraphics();

		this.createTourInstancePages(pagesData, tour);

		// Add this tour to the global MapsAlive object's tour instances.
		this.addTour(tour);

		this.createTourInstanceJavaScript(tour, js);

		tour.api.callbackTourLoaded();

		this.displayFirstPage(tour, tourProperties);
	}

	createTourInstanceIdentifier(tour)
	{
		let instanceName = tour.getTourSetting("instance", "");

		if (instanceName)
		{
			let api = window.MapsAlive.getApi(instanceName);
			if (api)
			{
				let message = `Tour #${tour.tourId} has two or more instances using the same identifier "${instanceName}".`;
				MapsAliveRuntime.reportEmbeddingError(tour.tourId, message);
				instanceName = "";
			}
		}

		if (instanceName && !MapsAliveRuntime.validateInstanceIdentifier("instance identifier", instanceName))
			instanceName = "";

		if (instanceName === "")
			instanceName = `_${tour.tourId}_${tour.instanceId}_`;

		tour.instanceName = instanceName.toLowerCase();
	}

	createTourInstanceJavaScript(tour, js)
	{
		if (tour.editMode)
			return;

		// Custom JavaScript into the DOM. Insert it only once if there are multiple instances of the same tour.
		tour.hasCustomJs = js !== null;
		if (tour.hasCustomJs && this.tours.length === 1)
		{
			// Append the tour number to the end of callback function names to make them unique.
			js = this.fixupCallbackFunctionNames(js, tour);

			tour.insertCustomJsIntoDom(js);
		}
	}

	createTourInstancePages(pagesData, tour)
	{
		// Create the tour's pages and set their properties with data generated by the Tour Builder.
		// If in edit mode, there will be just the page for the map being edited.
		for (const pageData of pagesData)
		{
			let page = new Page__$$(tour);
			for (const property in pageData.page)
				page[property] = pageData.page[property];
			page.propertiesHaveBeenSet();

			if (page.hasPopup)
			{
				page.popup = new Popup__$$(page);
				for (const property in pageData.popup)
					page.popup[property] = pageData.popup[property];
				page.popup.propertiesHaveBeenSet();
			}

			else
				page.popup = null;

			// Create the map with its map image and marker symbol images.
			page.addMap(pageData.map, pageData.symbols);

			// Attach the page to the tour.
			tour.addPage(page);
		}
	}

	static decodeHtml(html)
	{
		// Decode a string containing HTML entities into HTML e.g. "One&lt;br/&gt;Two" into "One<br/>Two".
		let element = document.createElement("textarea");
		element.innerHTML = html;
		return element.value;
	}

	defaultTourAlias(tour)
	{
		return `Tour${tour.tourId}`;
	}

	displayFirstPage(tour, tourProperties)
	{
		// Determine which of the tour's pages should be displayed.
		let pageNumber;
		if (tour.editMode)
		{
			pageNumber = tour.pages[0].pageNumber;
		}

		else
		{
			let pageSetting = tour.getTourSetting("page");
			pageNumber = tour.getPageNumber(pageSetting);
			if (pageNumber === 0)
				pageNumber = tourProperties.firstPageNumber;
		}

		// Display the page. When in edit mode, this displays the map in the Map Editor.
		tour.displayPage(pageNumber, true);
	}

	static elementLocation(element)
	{
		// Get the upper left corner of the element relative to the canvas.
		let rect = element.getBoundingClientRect();
		let pt = new Object();
		pt.x = Math.round(rect.left + window.scrollX);
		pt.y = Math.round(rect.top + window.scrollY);
		return pt;
	}

	fixupCallbackFunctionNames(js, tour)
	{
		let names = [
			'DirectoryEntryClicked',
			'HotspotChanged',
			'LiveDataResponse',
			'RequestLiveData',
			'PageLoaded',
			'PageLoading',
			'PopupClosed',
			'TourLoaded'
		];

		for (const name of names)
			js = js.replaceAll(`onEvent${name}`, `onEvent${name}_${tour.tourId}`);

		// Insert the tour number into the getApi method call;
		js = js.replaceAll(`getApi()`, `getApi(${tour.tourId})`);

		return js;
	}

	getCallbackFunction(version, tour, eventName)
	{
		let fcn;

		if (version == 'V4')
		{
			// See if there is a callback function that has the tour's id e.g. onEventPageLoaded_12345.
			fcn = `onEvent${eventName}_${tour.tourId}`;
			if (typeof window[fcn] === 'function')
			{
				return window[fcn];
			}
			else
			{
				// See if there is a callback function without the tour's id e.g. onEventPageLoaded.
				fcn = `onEvent${eventName}`;
				if (typeof window[fcn] === 'function')
					return window[fcn];
			}
		}
		else if (version == 'V3')
		{
			// See if there is a callback function with the V3 name e.g. maOnPopupClosed.
			fcn = `maOn${eventName}`;
			if (typeof window[fcn] === 'function')
				return window[fcn];
		}

		return null;
	}

	static getElement(id, tour)
	{
		return document.getElementById(tour.uniqueId(id));
	}

	static getQueryStringArg(arg)
	{
		let pairs = document.location.search.substring(1).split("&");
		for (let i = 0; i < pairs.length; i++)
		{
			let pair = pairs[i];
			let eq = pair.indexOf('=');
			if (pair.substring(0, eq).toLowerCase() === arg.toLowerCase())
				return pair.substring(eq + 1);
		}
		return '';
	}

	static isValidInstanceIdentifier(identifier)
	{
		if (!(typeof identifier === 'string'))
			return false;
		return identifier.match(/^[a-zA-Z0-9_\-]*$/) !== null;
	}

	static onWindowLoadEvent(tourProperties, directoryProperties, pagesData, css = null, html = null, js = null)
	{
		// This method creates the one and only instance of a MapsAlive object if it does not already
		// exist. It will exist if another tour has already been instantated on the container HTML page.
		// The MapsAlive object provides global access to all of the tours in a container page.
		// Usually there will only be one tour, but a container page can contain multiple tours,
		// including multiple instances of the same tour e.g. to show different pages of the tour at
		// the same time such as the first and second floor of a house tour.

		// Determine if the global MapsAlive object exists, and if not, create it.
		// In this code, _this is used to remind us that this is a static method that
		// is operating on an instance of itself pointed to by _this.
		if (typeof window.MapsAlive === 'undefined')
		{
			window.MapsAlive = new MapsAlive();
		}
		else
		{
			let ma = new MapsAlive();
			if (ma.version !== window.MapsAlive.version)
				/**/console.error(`The web page contains a tour built with an out of date version of MapsAlive`);
		}

		let _this = new MapsAliveRuntime(tourProperties.tourId);
		window.MapsAlive.addRuntime(_this);

		// Make sure the browser is adequate.
		if (!_this.browserSupportsMapsAlive())
			return;

		// If there's no HTML, this call is for the Map Editor which does not use the tour's HTML.
		tourProperties.editMode = html === null;

		if (tourProperties.editMode)
		{
			// Create a tour and set its properties with data generated by the Tour Builder.
			_this.createTourInstance(1, null, tourProperties, directoryProperties, pagesData);
		}
		else
		{
			// Determine how many instances there are for this tour. Each instance of the same tour
			// shares the same class name. Instances of other tours have a different class name.
			// The class name is on the <div> for the tour and matches the id of the <script> tag  
			// for the tour. Both tags are in the HTML for the page containing the tour. They are
			// emitted in the index.htm file created by the Tour Builder, or are hand-coded by a
			// user who is embedding a tour, or multiple tours, in an HTML container page.
			let tourSpecifier = `ma-${tourProperties.tourId}`;

			let scriptElements = document.querySelectorAll(`[id='${tourSpecifier}']`);
			if (scriptElements.length === 0)
			{
				let message = `Tour #${tourSpecifier} is missing a <script> tag having id="ma-${tourProperties.tourId}".`;
				MapsAliveRuntime.reportEmbeddingError(tourProperties.tourId,message);
				return;
			}
			else if (scriptElements.length > 1)
			{
				let message = `Found ${scriptElements.length} <script> tags with id="ma-${tourProperties.tourId}". There should only be 1.`;
				MapsAliveRuntime.reportEmbeddingError(tourProperties.tourId, message);
				return;
			}

			let classElements = document.getElementsByClassName(tourSpecifier);
			if (classElements.length === 0)
			{
				let message = `Tour #${tourSpecifier} is missing a <div> tag having class="ma-${tourProperties.tourId}".`;
				MapsAliveRuntime.reportEmbeddingError(tourProperties.tourId, message);
				return;
			}
			// Instantiate each of this tour's instances.
			let instanceId = 1;
			for (const instanceElement of classElements)
			{
				// Create a tour and set its properties with data generated by the Tour Builder.
				_this.createTourInstance(instanceId, instanceElement, tourProperties, directoryProperties, pagesData, html, css, js);
				instanceId += 1;
			}
		}
	}

	static reportEmbeddingError(tourId, message)
	{
		let error = `MapsAlive tour #${tourId} detected an embedding error.\n\n`;
		let help = `For help, see the "Embedding Errors" section in the MapsAlive User Guide.`;
		alert(`${error}${message}\n\n${help}`);
	}

	static roundFloat(value, precision = 5)
	{
		// Reduce the precision of a float to make it safe to compare numbers like ratios and scale factors that can
		// have values like 0.259983475626549 and will compare false to a another "identical" value that only differs
		// in a far decimal place. This addresses a problem where two numbers need to be compared, but due to the
		// difference in how each was calculated, and because of the nature of floating point math on binary data, the 
		// difference in their values is infinitesimal, but actually makes the values invalid for use in comparisons
		// where the goal is to determine if two scales or ratios are the. For example, to determine if the map's
		// current scale is the same as the minimum scale for a fully zoomed out map. This method should not be used
		// where the value's full precision is necessary in order to calulate another value that must be precise.

		// Convert the number to a string representation of the rounded value and then convert the string back to a number.
		return Number(value.toFixed(precision));
	}

	static scaledImageSize(imageSize, containerSize)
	{
		let containerWidth = containerSize.w;
		let containerHeight = containerSize.h;
		let imageWidth = imageSize.w;
		let imageHeight = imageSize.h;

		// Don't scale if the image already fits within the container.
		if (imageWidth <= containerWidth && imageHeight <= containerHeight)
			return imageSize;

		// Determine the scaling factor needed to reduce the image size
		// while still preserving its original aspect ratio.  The value we
		// use depends on which dimension (width or height) has to be reduced
		// the most in order to make the image fit within its container.
		let scalingFactor;
		let scalingFactorW = containerWidth / imageWidth;
		let scalingFactorH = containerHeight / imageHeight;
		if (scalingFactorH < scalingFactorW)
			scalingFactor = scalingFactorH;
		else
			scalingFactor = scalingFactorW;

		let scaledWidth;
		let scaledHeight;

		// Calculate the scaled image's dimensions.  If one of the dimensions will
		// be smaller than the container, we calculate it.  We could simply calculate
		// both dimensions, but rounding when converting from double to int can cause
		// the result to be off by a pixel.
		if (scalingFactorH === scalingFactorW)
		{
			scaledWidth = containerWidth;
			scaledHeight = containerHeight;
		}
		else if (scalingFactorH < scalingFactorW)
		{
			scaledWidth = Math.ceil(imageWidth * scalingFactor);
			scaledHeight = containerHeight;
		}
		else
		{
			scaledWidth = containerWidth;
			scaledHeight = Math.ceil(imageHeight * scalingFactor);
		}

		// Very wide or tall images that are scaled to a very small size
		// can lose a dimension. Make sure we always return a valid size.
		if (scaledWidth === 0)
			scaledWidth = 1;
		if (scaledHeight === 0)
			scaledHeight = 1;

		return { w: scaledWidth, h: scaledHeight };
	}

	static stringStartsWith(s, list)
	{
		let item = list.split(",");
		for (let i = 0; i < item.length; i++)
		{
			if (s.indexOf(item[i]) === 0)
				return true;
		}
		return false;
	}

	static trim(string)
	{
		if (string.length === 0)
			return string;
		while (string.substr(0, 1) === " ")
			string = string.substring(1, string.length);
		while (string.substr(string.length - 1, 1) === " ")
			string = string.substring(0, string.length - 1);
		return string;
	}

	static validateInstanceIdentifier(what, identifier)
	{
		if (MapsAliveRuntime.isValidInstanceIdentifier(identifier))
			return true;
		alert(`The MapsAlive ${what} "${identifier}" is not valid. It must contain only letters, numbers, hypen, or underscore.`);
		return false;
	}
}

// Only one MapsAlive object is created for a web page containing one or more tours. It keeps track of the
// MapsAliveRuntime objects for each of the tours in the page. If the same tour exists multiple times on the
// same page, there will be a separate MapsAliveTour object for each tour instance, but all the tour instances
// will share the same MapsAliveRuntime object instance. The runtime instance keeps track of its tour instances.
//
// Keep the code in this class to a bare minimum and avoid making any changes to its functionality or its
// interface unless absolutely necessary. Since all tours in the containing web page share the same MapsAlive
// object that was created from this source code for the first tour in the page, if the first tour's code is
// out of date or contains a bug, all of the other tours in the page will be using that old code. The only
// remdedy is to rebuild the first tour to ensure that the other tours are using the newest code.

class MapsAlive
{
	constructor()
	{
		// These properties begin with an underscore to indicate they are private
		// to this class and should never be directly accessed by another object.
		this._runtimes = [];
		this._callbackEvent = null;

		// Only bump the version if an incompatible change has been made to the MapsAlive object. Changing the
		// version will cause the runtime to detect if not all tours on the same web page are using the same version,
		// and if so, report an error in the developer console. Making a change should be avoided if at all possible
		this.version = 1;
	}

	//====================================================================================================
	// Public methods for API callers.
	//====================================================================================================

	getApi(tourSpecifier)
	{
		if (tourSpecifier === undefined || tourSpecifier === "")
		{
			let tour = this.firstTourOnPage;
			/**/console.warn(`getApi was called with no tour specified. Returned the first instance of "${tour.tourId}".`);
			return tour.api;
		}

		if (typeof tourSpecifier === 'string')
		{
			let instanceName = tourSpecifier.toLowerCase();

			for (const runtime of this._runtimes)
			{
				for (const tour of runtime.tours)
				{
					if (tour.instanceName === instanceName)
						return tour.api;
				}
			}
		}

		for (const runtime of this._runtimes)
		{
			if (runtime.tourId !== tourSpecifier)
				continue;
			let tour = runtime.tours[0];
			if (runtime.tours.length > 1)
				/**/console.warn(`getApi was called for tour ${tour.tourId} that has ${runtime.tours.length} instances. Returned the first instance "${tour.instanceName}".`);
			return tour.api;
		}

		return null;
	}

	getEvent()
	{
		return this._callbackEvent;
	}

	//====================================================================================================
	// Private methods.
	//====================================================================================================

	addRuntime(runtime)
	{
		this._runtimes.push(runtime);
	}

	get callbackEvent()
	{
		return this._callbackEvent;
	}

	set callbackEvent(event)
	{
		this._callbackEvent = event;
	}

	get firstTourOnPage()
	{
		return this._runtimes[0].tours[0];
	}

	handleLiveDataResponse(request)
	{
		// Ignore responses other than DONE such as HEADERS_RECEIVED and LOADING.
		const READY_STATE_DONE = 4;
		if (request.xhr.readyState !== READY_STATE_DONE)
			return;

		// Send the response to the Live Data object for the tour instance that made the request.
		let api = this.getApi(request.instanceName);
		api.liveData.handleResponse(request);
	}
}
