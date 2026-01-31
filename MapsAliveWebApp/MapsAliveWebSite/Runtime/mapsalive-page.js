// Copyright (C) 2009-2022 AvantLogic Corporation: https://www.mapsalive.com

export { MapsAlivePage };

import { MapsAliveLayout as Layout__$$ } from './mapsalive-layout.js';
import { MapsAliveMap as Map__$$ } from './mapsalive-map.js';
import { MapsAliveRuntime as Runtime__$$ } from './mapsalive-runtime.js';
import { MapsAliveView as View__$$ } from './mapsalive-view.js';

class MapsAlivePage
{
	constructor(tour)
	{
		//console.log(`Page__$$::contructor`);

		this.tour = tour;
		this.map = null;
		this.pageLoaded = false;
		this.popup = null;

		this.hasDisabledPopop = false;

		this.layout = null;
		this.currentView = null;
		this.views = null;

		this.hotspotSelectedFromDirectory = 0;

		this.waitingForMapImageToLoad = false;
		this.waitDuration = 0;
		this.waitStart = 0;
		this.waitAttempts = 0;

		this.waitingForContentTimerId = 0;
		this.offlineIndicatorTimerId = 0;

		// Bind event handlers so that when they are called, 'this' will be set to this MapsAlivePopup object.
		this.onLoadNextViewImage = this.onLoadNextViewImage.bind(this);
		this.waitForPageToLoad = this.waitForPageToLoad.bind(this);
		this.stopShowingWaitIndicator = this.stopShowingWaitIndicator.bind(this);
		this.showViewForSelectedMarker = this.showViewForSelectedMarker.bind(this);

		// Other properties will be set by MapsAliveRuntime. It will then call this object's propertiesHaveBeenSet() method.
	}

	addMap(mapData, symbols)
	{
		this.map = new Map__$$(this, mapData, symbols);
	}

	closePopup()
	{
		if (this.tour.editMode)
			return;

		// This method is provided for convenience so that it can be called without
		// concern for whether the page has a popup or content panel that is showing.
		if (this.hasPopup && this.popup.isShowing)
			this.popup.hidePopup();
		else if (!this.layout.contentPanelIsHidden)
			this.layout.hideContentPanel();
	};

	createViewIdListFromRouteHotspotIdList(hotspotIdList)
	{
		// Make sure the list is a string in case the parameter is a single integer value.
		hotspotIdList += "";

		let viewIdList = "";
		if (hotspotIdList.length > 0)
		{
			let sections = hotspotIdList.split(";");
			for (let i in sections)
			{
				if (viewIdList.length > 0)
					viewIdList += ";";
				let section = Runtime__$$.trim(sections[i]);

				viewIdList += this.createViewIdListFromHostpotIdList(section);
			}
		}
		return viewIdList;
	}

	createViewIdListFromHostpotIdList(hotspotIdList)
	{
		// Make sure the list is a string in case the parameter is a single integer value.
		hotspotIdList += "";

		if (hotspotIdList === "*")
			return hotspotIdList;

		let viewIdList = "";
		if (hotspotIdList.length > 0)
		{
			let viewIds = hotspotIdList.split(",");
			for (let i in viewIds)
			{
				if (viewIdList.length > 0)
					viewIdList += ",";
				let hotspotId = Runtime__$$.trim(viewIds[i]);

				let view = this.getViewByHotspotId(hotspotId);
				if (view)
					viewIdList += view.viewId;
				else
					/**/console.warn(`Hotspot "${hotspotId}" does not exist`);
			}
		}
		return viewIdList;
	}

	displayPage()
	{
		let previousViewId = this.map.selectedMarkerViewId;
		//console.log(`Page::displayPage ${previousViewId}`);

		this.map.initializeDrawState();

		// Set the size of the tour to fit the viewport. This also draws the map at the pan/zoom position chosen
		// and locked in the Map Editor. If the user did not lock the map, it displays zoomed all the way out.
		this.layout.setTourSize({ resize: false });

		// Show the default content. If the map is locked, it won't be repositioned to show the marker for
		// the default view. This is intentional so that the map comes up in the locked position as expected. 
		// To avoid confusion, the user should lock the map in a position where the marker is visible.
		if (this.runSlideShow)
			this.map.startSlideShow();
		else if (this.isDataSheet)
			this.showDataSheetContent();
		else
			this.showDefaultView(previousViewId);

		// Show the directory and instructions
		if (this.tour.hasNavPanel && this.tour.hasDirectory && this.tour.directory.staysOpen)
			this.layout.toggleShowNavPanel();

		this.tour.api.callbackPageLoaded();
	}

	getLiveData(view, pointer)
	{
		console.log(`Page::getLiveData ${view.hotspotId} : ${view.title} ${view.viewId}`);

		// Attach the current pointer position to the view so that we can get it back after the
		// asyncronous request for live data responds. It will be needed for popup positioning.
		view.pointer = pointer;

		let script = view.messengerFunction;
		if (this.tour.preview && this.tour.flagDisableTourPreviewLiveData)
		{
			let message = "Live Data has been disabled while in Tour Preview.";
			this.reportLiveDataError(view, null, message, script);
			return;
		}

		let event = this.tour.api.createEvent();
		event.action = "livedata";
		event.hotspot = this.tour.api.createHotspotProperties(view);
		view.liveDataRequestPending = true;

		// Determine whether the user has provided their own request function (formerly referred to
		// as a messenger function) or if they are using the default function onEventRequestLiveData.
		// An error in the function will be handled by Tour::onError.
		if (script.length === 0)
		{
			let eventName = "RequestLiveData";
			let fcn = this.tour.runtime.getCallbackFunction('V4', this.tour, eventName);
			if (fcn)
				fcn(event);
			else
				this.reportLiveDataError(view, null, "The onEventRequestLiveData function is missing", "");
		}
		else
		{
			// Call the function the user provided.
			window.MapsAlive.callbackEvent = event;
			window.Function(script)();
			window.MapsAlive.callbackEvent = null;
		}
	}

	getView(viewId)
	{
		let view = this.views[viewId];
		if (typeof view === "undefined")
			view = null;
		return view;
	}

	getViewByHotspotId(hotspotId)
	{
		// In case the hotspot Id came in via the query string, restore
		// any escaped characters such as %20 for blank spaces.
		hotspotId = unescape(hotspotId);

		for (let viewId in this.views)
		{
			let view = this.getView(viewId);
			if (view.hotspotId.toLowerCase() === hotspotId.toLowerCase())
			{
				// Make sure the view belongs to the current page.
				if (view.pageNumber !== this.pageNumber)
					continue;

				return view;
			}
		}
		return null;
	}

	getWaitDuration()
	{
		return Date.now() - this.waitStart;
	}

	hidePage()
	{
		// This method is called when another page is about to be displayed e.g. when the user clicks
		// another page in the tour menue.

		// Perform any housekeeping that needs to be done before leaving the page. Check for the existence
		// of objects like layout before using them since this method could be called while the page is
		// still loading, before those objects have been created.

		this.closePopup();
		this.map.hideTooltip();

		if (this.layout)
			this.layout.removeLayoutElements();

		this.map.pauseSlideShow();
	}

	hideViewContent(viewId)
	{
		// Show the view's text which also erases any previous Live Data content;
		if (this.layout.hasTextArea)
			this.setViewText(viewId);

		// Erase the media area.
		if (this.layout.hasMediaArea)
			this.layout.mediaAreaElement.innerHTML = '';
	}

	loadViewContent(viewId)
	{
		//console.log(`Page::loadViewContent ${viewId}`);

		this.currentView = this.getView(viewId);

		let imageSize = { w: this.currentView.mediaW, h: this.currentView.mediaH };

		// Reset the text even if the marker did not change because we always clear the text
		// after closing a popup in case it contains a macro for video that might still be playing. 
		this.setViewText(viewId);

		// Call setViewMedia even if the marker has not changed in order to restore the view image's
		// src to the view's image instead of the blank image that gets set when a view is hidden.
		if (this.layout.hasMediaArea)
			this.setViewMedia(viewId, imageSize);

		// Create the styles needed to properly display the content. For mobile layouts, the CSS is created
		// dynamically to specify image dimensions and other styles for the portrait and landscape content panels
		if (this.layout.usingMobileLayout)
			this.layout.createCssForContentPanel(imageSize);

		// Reset scrolling so the new content won't be scrolled if the user had scrolled the previous content.
		this.layout.resetContentScrollTop();
	}

	loadNextViewImage()
	{
		// This method is called repeatedly on a timer until all images have been preloaded.
		// It loads one image and then clears a flag indicating that it's not okay to load
		// the next image until the current image has loaded. This scheme lets all view
		// images get preloaded, but allows another image-load request to be honored if it
		// occurs before the entire preload is complete. For example, a user may mouse over
		// a marker with a popup view before the preload is complete. If that view's image
		// has not preloaded, a request will be made via this.viewImageSrc to load it
		// immediately and the request will be sent to the server in between the currently
		// preloaded image and the next. The effect is like a high priority interrupt.
		//
		// Note that if preloading needed to be turned off for some reason (e.g. a user option)
		// it could be done by simply not setting the timer to call this method. In that case,
		// every image would be loaded on demand.

		if (this.tour.okayToPreloadNextImage)
		{
			this.tour.okayToPreloadNextImage = false;

			let view = this.viewsImageList[this.viewImageListIndex];

			this.viewImageListIndex++;

			if (this.viewImageListIndex >= this.viewsImageList.length)
				clearInterval(this.preloadIntervalId);

			//console.log(`Page::loadNextViewImage ${this.pageName}::${view.title}: ${view.imageSrc}`)

			if (view.imageSrc)
				view.image.src = view.imageSrc;
			else
				this.tour.okayToPreloadNextImage = true;
		}
	}

	loadViewTable()
	{
		//console.log(`Page::loadViewTable`);

		let stringTable = this.stringTable;

		// Fixup string table entries.
		for (let stringIndex = 0; stringIndex < stringTable.length; stringIndex++)
		{
			// Restore HTML characters.
			let text = unescape(stringTable[stringIndex]);

			// Fixup view images that are embedded in the text by replacing ':' with the
			// client path. The ':' originates in TourPageXmlWriter when it expands [image] macros.
			text = text.replace(/src=":/g, 'src="' + this.tour.path);

			// Update the entry.
			stringTable[stringIndex] = text;
		}

		// Create a div for the sole purpose of converting HTML to plain text.
		let div = document.createElement("div");

		// Create a list of pointers to views that have images that will need to be loaded.
		this.viewsImageList = new Array();
		this.viewImageListIndex = 0;

		// Construct a table of MapsAliveView objects.
		this.views = new Object();
		let viewTable = this.viewTable;
		for (let viewIndex = 0; viewIndex < viewTable.length; viewIndex++)
		{
			let data = viewTable[viewIndex].split(',');
			let viewId = parseInt(data[0], 10);
			let pageNumber = parseInt(data[1], 10);
			let hotspotId = this.lookupString(data[2]);
			let titleText = this.lookupString(data[3]);
			let htmlText = this.lookupString(data[4]);
			let plainText = "";
			let imageSrc = this.lookupString(data[5]);
			let mediaW = parseInt(data[6], 10);
			let mediaH = parseInt(data[7], 10);
			let mediaType = parseInt(data[8], 10);
			let embedText = this.lookupString(data[9]);
			let popupOverrideW = parseInt(data[10], 10);
			let popupOverrideH = parseInt(data[11], 10);
			let usesLiveData = parseInt(data[12], 10) === 1;
			let messengerFunction = this.lookupString(data[13]);
			let dirPreviewImageUrl = this.lookupString(data[14]);
			let dirPreviewText = this.lookupString(data[15]);
			let onMap = parseInt(data[16], 10);

			// Get the view's HTML and convert it to plain text.
			// Save both in the view: HTML for displaying the view and plain text for searching.
			div.innerHTML = htmlText;
			plainText = div.innerText;

			let view = new View__$$(
				this,
				viewId,
				pageNumber,
				hotspotId,
				titleText,
				htmlText,
				plainText,
				imageSrc,
				mediaW,
				mediaH,
				mediaType,
				embedText,
				popupOverrideW,
				popupOverrideH,
				usesLiveData,
				messengerFunction,
				dirPreviewImageUrl,
				dirPreviewText,
				onMap);

			this.views[viewId] = view;

			if (mediaW > 0)
			{
				this.viewsImageList.push(view);
			}
		}

		this.currentView = this.getView(this.firstViewId);
	}

	lookupString(index)
	{
		if (index === '-1')
			return '';

		return this.stringTable[index];
	}

	needToRequestLiveData(view)
	{
		if (view.liveDataRequestPending)
			return false;

		// Check if an error occurred during the most recent request for Live Data.
		if (this.currentView.liveDataErrorOccurred)
		{
			view.liveDataErrorOccurred = false;
			return false;
		}

		// Check if Live Data for this view has ever been requested.
		if (view.liveDataUpdateTime === 0 && !(this.tour.preview && this.tour.flagDisableTourPreviewLiveData))
			return true;

		// Check if the data has been requested, but no data has ever been received.
		if (view.htmlText.length === 0)
			return true;

		// Check if there is cached data that has not gone stale.
		if (!view.liveDataCachePeriodHasExpired())
			return false;

		// There is data, but the cache period has expired.
		return true;
	}

	onLoadNextViewImage()
	{
		this.loadNextViewImage();
	}

	onViewImageLoaded(viewId)
	{
		//console.log(`Page::onViewImageLoaded ${viewId}`);

		if (this.tour.busy.waitingForImage(viewId))
			this.tour.busy.stopWaitingForImage(viewId);
	}

	get usingTiledLayout()
	{
		return !this.hasPopup && !this.layout.usingMobileLayout;
	}

	preLoadViewImages()
	{
		//console.log(`Page::preLoadViewImages`);

		if (this.tour.flagDisablePreloadImages)
			return;

		if (this.tour.isMobileDevice && !this.tour.preloadImagesOnMobile)
			return;

		if (this.viewsImageList.length === 0)
			return;

		// Start preloading view images.
		this.tour.okayToPreloadNextImage = true;
		this.preloadIntervalId = setInterval(this.onLoadNextViewImage, 0);
	}

	propertiesHaveBeenSet()
	{
		// This method is called after MapsAliveRuntime has contructed this object and
		// set its properties with data sent from the Tour Builder in JavaScript files.

		if (this.isDataSheet || this.hasPopup)
			return;

		// Convert the page to an info page when its layout has no map.
		if (Layout__$$.layoutHasNoMap(this.layoutId))
			this.isDataSheet = true;
	}

	reportLiveDataError(view, request, message, script)
	{
		let liveData = this.tour.api.liveData;
		let settings = liveData.errorSettings;
		if (settings.isCustomError)
			message = settings.message;

		let title = "Live Data request failed";
		let html = "<div style='font-weight:bold;margin-left:3px;'>" + title + "</div>";

		if (settings.showDetail)
		{
			let reason = message;

			html += "<hr/>";
			html += "<table>";
			html += "<tr><td valign='top' style='font-weight:bold;'>Reason:</td><td>" + reason + "</td></tr>";

			if (request !== null)
			{
				html += "<tr><td valign='top' style='font-weight:bold;'>Hotspot Title:</td><td>" + request.view.title + "</td></tr>";
				html += "<tr><td valign='top' style='font-weight:bold;'>Hotspot Id:</td><td>" + request.view.hotspotId + "</td></tr>";
				html += "<tr><td valign='top' style='font-weight:bold;'>Server URL:</td><td>" + request.url + "</td></tr>";
				html += "<tr><td valign='top' style='font-weight:bold;'>Parameters:</td><td>" + request.parameters + "</td></tr>";
				let plural = request.cachePeriodSeconds !== 1 ? "s" : "";
				html += "<tr><td valign='top' style='font-weight:bold;'>Cache Period:</td><td>" + request.cachePeriodSeconds + " second" + plural + "</td></tr>";
			}

			if (typeof script !== "undefined")
			{
				html += "<tr><td valign='top' style='font-weight:bold;'>JavaScript:</td><td>" + script + "</td></tr>";
			}

			html += "</table>";
		}
		else
		{
			html = message;
		}

		html = "<div style='overflow:hidden;font-family:sans-serif;font-size:11px;border:solid 1px gray;padding:4px;" +
			"color:" + settings.color + ";background-color:" + settings.backgroundColor + ";'>" + html + "</div>";
		html = unescape(html);

		view.htmlText = html;

		view.liveDataErrorOccurred = true;
		this.updateViewWithLiveData(view, view.liveDataErrorOccurred);

		// Reset the time so that the error won't get cached.
		view.liveDataUpdateTime = 0;
	}

	setHotspotUsesLiveData(viewIdList, uses)
	{
		if (viewIdList === "*")
		{
			for (let viewId in this.views)
			{
				let view = this.getView(viewId);
				view.setViewUsesLiveData(uses);
			}
		}
		else
		{
			let list = viewIdList.split(",");
			for (const index in list)
			{
				let viewId = parseInt(list[index]);
				let view = this.getView(viewId);
				view.setViewUsesLiveData(uses);
			}
		}
	}

	setSrcForViewImage()
	{
		if (!this.currentView.hasImage)
			return;

		// Set the image src if not already set. This will be the case when either images are not being preloaded,
		// or if preloading is occuring, but the user selected this view before the preloaded got to it.
		if (this.currentView.image.src.length === 0)
			this.currentView.image.src = this.currentView.imageSrc;
	}

	setViewMedia(viewId, imageSize)
	{
		//console.log(`Page::setViewMedia ${viewId}`);

		if (this.currentView.mediaW === 0 || this.currentView.mediaH === 0)
		{
			this.layout.mediaAreaElement.innerHTML = '';
			return;
		}

		// Ensure that embedded media will have no border since a border would
		// make the media element wider and taller than the media area element. 
		if (this.viewMediaTypeIsEmbedded)
		{
			this.layout.mediaAreaElement.innerHTML = this.getView(viewId).embedText;
			let element = this.layout.mediaAreaElement.firstElementChild;
			if (element)
			{
				if (element.nodeName === "IFRAME")
					element.frameBorder = 0;
				element.style.border = "";
			}

			// Prevent embedded media from overflowing the media area.
			this.layout.mediaAreaElement.style.overflow = 'hidden';
			return;
		}

		this.layout.mediaAreaElement.style.overflow = 'unset';

		// Remove the previous image.
		this.layout.mediaAreaElement.innerHTML = ""

		// Set the dimension styles for non-mobile layouts. For mobile layouts, createCssForContentPanel will get called
		// to set the styles. Because each view's image can have different dimensions than the images for other views,
		// this logic must be executed each time a view's image is displayed.
		let img = this.currentView.image;
		if (!this.layout.usingMobileLayout)
		{
			if (this.hasPopup)
			{
				img.style.maxWidth = '100%';
			}
			else
			{
				let scaledMediaAreaSize = Runtime__$$.createSizeObject(this.layout.scaledMediaAreaW, this.layout.scaledMediaAreaH);
				let scaledImageSize = Runtime__$$.scaledImageSize(imageSize, scaledMediaAreaSize);
				img.style.width = `${scaledImageSize.w}px`;
				img.style.height = `${scaledImageSize.h}px`;
			}
		}

		this.layout.mediaAreaElement.appendChild(img);
	}

	setViewText(viewId)
	{
		function setText(page, viewId, element, isTitle)
		{
			if (!element)
				return;

			let view = page.getView(viewId);
			let text = isTitle ? view.title : view.htmlText;
			let textIsEmpty = false;

			if (!isTitle)
				textIsEmpty = text.length === 0;

			element.style.display = textIsEmpty ? 'none' : 'block';
			element.innerHTML = textIsEmpty ? "" : text;
		}

		if (this.showViewTitle)
			setText(this, viewId, this.layout.titleElement, true);

		if (!this.layout.hasTextArea)
			return;

		setText(this, viewId, this.layout.textElement, false);

		this.layout.setTextAreaFontSizes();
	}

	showDataSheetContent()
	{
		function setImgSrc(img, src, size)
		{
			if (src === '')
				img.removeAttribute('src');
			else
				img.src = src;

			img.style.width = size;
			img.style.height = size;
		}

		// An Info page has one implicit hotspot, its first view, but no marker. Show the hotspot's content.
		this.currentView = this.getView(this.firstViewId);

		if (this.currentView.hasImage)
		{
			// Make sure the image is loaded. It won't be if the user is viewing this data sheet
			// for the first time on a mobile device where image preloading does not occur.
			const offlineImageSrc = this.tour.graphics["offline"].src;
			if (navigator.onLine)
			{
				// If the device was previously offline but is online now, remove the offline indicator.
				if (this.currentView.image.src === offlineImageSrc)
					setImgSrc(this.currentView.image, '', '100%');

				// Set the image src. For a data sheet there is no loading indicator while waiting.
				this.setSrcForViewImage();
			}
			else
			{
				// Show the offline icon in place of the image.
				const imageWasPreviouslyLoaded = this.currentView.imageLoaded && this.currentView.image.src !== offlineImageSrc;
				if (!imageWasPreviouslyLoaded)
					setImgSrc(this.currentView.image, offlineImageSrc, '32px');
			}
		}

		this.loadViewContent(this.firstViewId);
		this.layout.makeContentVisibleOnMobileDevice(this.firstViewId);
	}

	showDefaultView(previousViewId = 0)
	{
		// Determine if the query string contains the Id of a hotspot that should be shown when the tour loads.
		let hotspotIdOnQueryString = Runtime__$$.getQueryStringArg("hotspot");

		// Don't show a selected marker or its view when a page first displays on mobile.
		if (this.layout.usingMobileLayout && this.hotspotSelectedFromDirectory === 0 && hotspotIdOnQueryString === '')
		{
			this.map.deselectMarker()
			return;
		}

		// Don't show a popup for a hotspot that was not selected from the directory and is not on the query string.
		if (this.hasPopup && this.hotspotSelectedFromDirectory === 0 && hotspotIdOnQueryString === '')
			return;

		// Get the default view that the user chose in the Tour Builder.
		let defaultViewId = this.firstViewId;

		// Override the default view if the user just chose a hotspot in the directory.
		if (this.hotspotSelectedFromDirectory !== 0)
			defaultViewId = this.hotspotSelectedFromDirectory;

		if (previousViewId)
		{
			// This page was previously displayed. Use the last view that was showing as the default.
			let marker = this.map.getMarker(previousViewId);

			if (marker.showContentEvent === this.map.SHOW_CONTENT_ON_MOUSEOVER && !marker.showsContentOnlyInTooltip)
				defaultViewId = previousViewId;
		}
		else
		{
			// When there's a hotspot Id on the query string, show its view instead of the default view.
			if (hotspotIdOnQueryString)
			{
				// See if the id matches an actual view on this map. Replace occurences of '+' that occur
				// in the URL for spaces with actual spaces.
				let id = hotspotIdOnQueryString.replace(/\+/g, ' ');
				let view = this.getViewByHotspotId(id);
				if (view)
					defaultViewId = view.viewId;
				else
					hotspotIdOnQueryString = '';
			}
		}

		// Make sure the marker for the default view is on the map.
		defaultViewId = this.validateDefaultViewId(defaultViewId);
		if (defaultViewId === 0)
			return;

		// Show the view's marker as selected.
		this.map.selectMarker(defaultViewId, null);

		if (this.hasPopup && (this.hotspotSelectedFromDirectory || hotspotIdOnQueryString))
		{
			const pin = true;
			this.popup.showPopup(pin);
		}

		if (this.hotspotSelectedFromDirectory || hotspotIdOnQueryString)
			this.map.selectedMarker.setBlink(this.map.blinkCount);

		this.hotspotSelectedFromDirectory = 0;
	}

	showView(viewId, pointer)
	{
		//console.log(`Page::showView ${viewId}`);

		// Hide the directory before showing the view.
		if (this.tour.hasNavPanel && this.tour.dirShowing && !this.tour.directory.staysOpen)
			this.tour.directory.dirShow(false);

		this.loadViewContent(viewId);

		// Show the popup if it had been kept hidden while waiting for content to load.
		if (this.hasPopup && this.popup.waitingToShowPopup)
			this.popup.showWaitingPopup();

		this.layout.makeContentVisibleOnMobileDevice(viewId);

		if (this.hasPopup)
			this.popup.movePopupToLocation(pointer);
	}

	showViewForSelectedMarker(pointer, isCallback = false)
	{
		let viewId = this.map.selectedMarkerViewId;

		//console.log(`Page::showViewForSelectedMarker ${this.map.selectedMarkerViewId} Attempt:${this.waitAttempts}`);

		// Handle the case where the marker got deselected in between attempts to wait for its content to load.
		if (isCallback && viewId === 0)
		{
			this.stopWaitingForViewContentToLoad(false);
			return;
		}

		Runtime__$$.assert(viewId !== 0, 'selectedMarkerViewId IS ZERO');

		this.currentView = this.getView(viewId);

		// Initiate loading of the view's content if not already loaded. If the call returns false,
		// the view's image and/or Live Data has not loaded yet and so we'll have to wait for it. 
		if (this.viewContentIsLoaded(viewId, pointer))
		{
			this.showView(viewId, pointer);
			return;
		}

		// The content is not yet loaded. When using a tiled layout, erase the current image and any
		// Live Data content so that content for the previosly selected marker is not showing while
		// content for the newly selected marker is loading. If the view shows a title and/or any
		// non-Live Data text, it will get displayed immediately so the user at least sees something.
		if (this.usingTiledLayout)
			this.hideViewContent(viewId);

		// Determine if waiting should continue, in which case, waitForContentToLoad will make an asynchronous
		// callback to this showViewForSelectedMarker method with the expectation that eventually the call to
		// viewContentIsLoaded will return true and the view will get shown. However, if the content has not
		// loaded after repeated wait cycles, waitForContentToLoad will return false and execution will fall
		// through to the code below that cancels waiting.
		if (this.waitForContentToLoad(pointer))
			return;

		// The max number of attempts has been reached.
		this.showViewForSelectedMarkerAbort();
	}

	showViewForSelectedMarkerAbort()
	{
        this.tour.busy.stopWaiting();

        // Clear the image src so the browser will try to load the image again next time the user selects the view's marker. 
        if (this.currentView.hasImage)
            this.currentView.image.removeAttribute('src');

        if (this.currentView.usesLiveData)
            this.currentView.liveDataRequestPending = false;

        if (this.hasPopup)
			this.popup.stopWaitingToShowPopup();

		if (this.layout.usingMobileLayout)
		{
			this.map.deselectMarker();
			this.layout.hideContentPanel();
		}
    }

	stopShowingWaitIndicator()
	{
		this.tour.busy.hideWaitIndicator();
	}

	stopWaitingForViewContentToLoad(deselectMarker)
	{
		// This method is used for popup and mobile maps, but not for tiled layouts. It gets called when a view's
		// image has successfully loaded, when Lived Data has loaded, and when waiting is being cancelled because
		// the user has done something, like mousing off the the view's marker, such that there's no need to keep
		// waiting. When cancelling, this code makes sure that the selected marker gets deselected in case that's
		// not done by the caller chain. The most notable case where deselection is necessary is when the user
		// mouses over a marker and then clicks the marker to pin the popup. On the mouseover, waiting starts with
		// the intention of displaying the popup unpinned once the image loads. When the click occurs, the first
		// wait gets cancelled, and a new wait beging that will show the popup pinned once the image loads.

		//console.log(`Page::stopWaitingForViewContentToLoad deselectMarker:${deselectMarker}`);

		this.tour.busy.stopWaiting();

		clearTimeout(this.waitingForContentTimerId);
		this.waitingForContentTimerId = 0;

		this.waitAttempts = 0;

		if (deselectMarker && (this.hasPopup || this.layout.usingMobileLayout))
			this.map.deselectMarker();

		if (this.hasPopup)
			this.popup.stopWaitingToShowPopup();
	}

	startLoadingPage() 
	{
		//console.log(`Page::startLoadingPage`);

		// Initial the map object's state.
		if (!this.isDataSheet)
			this.map.initializeMapState();

		// Create the page's layout. This call must occur after the map state is initialized.
		this.layout = new Layout__$$(this);

		// Show the user that something is happening. This call must occur after the layout is created.
		this.pageKind = this.isGallery ? 'gallery' : 'map';

		if (navigator.onLine)
		{
			this.map.showLoadingMessage(`Loading ${this.pageKind} for ${this.pageName}`);
		}
		else
		{
			this.map.showLoadingMessage(`No internet connection`, true);
			return;
		}

		if (!this.isDataSheet)
		{
			// Initiate asyncronous loading of the map images.
			this.map.loadMapImages();

			// Create the map's marker styles and instances.
			this.map.createMarkerStyles();
			this.map.createMarkers();
		}

		// Load other page assets while the map image is loading.
		if (!this.tour.editMode)
			this.preLoadViewImages();
	}

	userStoppedSlideShow()
	{
		// Prevent the slide show from starting again automatically if this page is reloaded.
		this.runSlideShow = false;
	}

	updateViewWithLiveData(view, liveDataErrorOccurred = false)
	{
		console.log(`Page::updateViewWithLiveData for ${view.hotspotId} : ${view.title} : ${view.viewId}`);

		view.liveDataUpdateTime = (new Date()).getTime();
		view.liveDataRequestPending = false;
		view.liveDataErrorOccurred = liveDataErrorOccurred;

		// Hide the waiting indicator except in Tour Preview where this method only
		// gets called to report that Live Data calls are not made during Tour Preview.
		this.tour.busy.stopWaitingForData(view.viewId);

		// Determine if the view is the current view. If it's not, either the response to the view's Live Data
		// request came after the user selected another view, or the pointer is over a directory entry and the
		// request is to get the preview image and text.
		if (!this.tour.showingPreview && (this.map.selectedMarkerViewId === 0 || this.currentView.viewId !== view.viewId))
			return;

		// Set the view's text area unless no Live Data text was returned. Prior to calling the server,
		// we put the loading indicator HTML into the text area. If it's still there, we know that no
		// text came back so we set the view's text to blank in order to erase the loading indicator.
		if (this.currentView.htmlText === this.tour.loadingGraphic() && this.layout.textElement)
			this.currentView.htmlText = "";

		// Insert the HTML returned from the Live Data request into the view's text area.
		this.updateViewWithLiveDataText(view);
	}

	updateViewWithLiveDataText(view)
	{
		//console.log(`Page::updateViewWithLiveDataText ${view.hotspotId}`);

		this.setViewText(view.viewId);

		if (this.hasPopup)
		{
			// Recover the position of the pointer that was attached to the view at the time the
			// live data request was made. Then remove the pointer position which is no longer needed.
			let pointer = view.pointer;
			if (pointer === null)
			{
				// This should never happen, but it does sometimes, perhaps because a second
				// response to the same request is being received. On the first response, this
				// code would have set view.pointer to null.
				console.log(`Page::updateViewWithLiveDataText ${view.hotspotId} Pointer IS NULL`);
				return;
			}
			view.pointer = null;

			this.popup.movePopupToLocation(pointer);
		}
	}

	validateDefaultViewId(viewId)
	{
		let defaultViewId = viewId;

		// The default view Id should not be zero unless map has no hotspots.
		if (defaultViewId === 0)
			return 0;

		// The default viewId is set using the the First Hotspot on the Map Setup page. Verify
		// that the marker for the default is on the map. Normally it would be, but if the user
		// removed that marker from the map, or never placed it, the view Id would be invalid.
		let marker = this.map.getMarker(defaultViewId, false);
		if (marker === null)
		{
			// Choose another marker as the default. If there isn't one, it's
			// because the user has not placed any of the page's hotspots on the map.
			marker = this.map.getFirstMarker();
			if (marker === null)
				return 0;
			defaultViewId = marker.viewId;
		}

		return defaultViewId;
	}

	viewContentIsLoaded(viewId, pointer)
	{
		//console.log(`Page::viewContentIsLoaded ${viewId}`);

		let loaded = true;
		let startWaitingToShowPopup = false;

		if (this.currentView.hasImage && !this.currentView.imageLoaded)
		{
			if (!this.tour.busy.waitingForImage(viewId))
			{
				this.tour.busy.startWaitingForImage(viewId);
				this.setSrcForViewImage();
				this.waitAttempts = 0;
				if (this.hasPopup)
					startWaitingToShowPopup = true;
			}
			loaded = false;
		}

		if (this.currentView.usesLiveData)
		{
			if (this.needToRequestLiveData(this.currentView))
			{
				this.getLiveData(this.currentView, pointer);
				loaded = false;
			}
			else if (this.currentView.liveDataRequestPending)
				loaded = false;

			if (!loaded && !this.tour.busy.waitingForData(viewId))
			{
				if (this.hasPopup)
					startWaitingToShowPopup = true;
				this.tour.busy.startWaitingForData(viewId);
			}
		}

		if (!loaded)
		{
			if (startWaitingToShowPopup)
				this.popup.startWaitingToShowPopup();
			return false;
		}

		if (this.tour.busy.waiting)
			this.tour.busy.stopWaiting();

		return true;
	}

	viewImageSrc(view)
	{
		if (view.imageLoaded)
		{
			// The image was preloaded and is ready to show.
			return view.image.src;
		}
		else
		{
			// The image has not loaded yet into the view object's image object.
			// Request that it be fetched right now from its URL. 
			return view.imageSrc;
		}
	}

	get viewMediaTypeIsEmbedded()
	{
		return this.currentView.mediaType === 1;
	}

	waitForContentToLoad(pointer)
	{
		//console.log(`Page::waitForContentToLoad`);

		// This method implements an asynchronous wait "loop" by calling back
		// to showViewForSelectedMarker repeatedly until the content has loaded.
		let MAX_ATTEMPTS = navigator.onLine ? 200 : 50;

		if (this.waitAttempts >= MAX_ATTEMPTS)
			return false;

		this.waitAttempts += 1;

		// Start with short callback delay, but after a while, wait a little longer on subsequent tries.
		const delayMS = this.waitAttempts < 20 ? 50 : 100;
		const isCallback = true;
		this.waitingForContentTimerId = setTimeout(this.showViewForSelectedMarker, delayMS, pointer, isCallback);

		return true;
	}

	waitForPageToLoad()
	{
		//console.log(`Page::waitForPageToLoad ${this.pageName} ${Date.now()} ${this.pageLoaded} ${this.waitDuration}`);

		// This method returns to its caller immediately after initiating asynchronous logic to load
		// the map image. Nothing in the caller chain of this method should depend on the page being
		// loaded when this call returns. As such, the caller should call this method only after it's
		// done everything it needs to do other than to wait for the user to interact with the tour.

		const WAIT_PERIOD_MS = 50;
		const WAIT_ATTEMPTS_LIMIT = 500;

		if (this.pageLoaded)
		{
			// When this method is called and pageLoaded is already true, it means this page was
			// previously loaded and that the user has chosen to see it again. Recreate the page's
			// layout elements which gets destoyed whenever the users leaves a page to go to another.
			// All other page assets remain from when the page originally loaded.
			this.layout = new Layout__$$(this);
		}
		else
		{
			if (this.map.mapImagesHaveLoaded() && this.tour.bannerImageHasLoaded && !this.tour.api.waitForLiveData)
			{
				// The asyncronous load has completed. Stop waiting.
				this.waitingForMapImageToLoad = false;
			}
			else
			{
				// The page has not finished loading.
				if (!this.waitingForMapImageToLoad)
				{
					// This is the first call to wait for the page to load. Initiate the load.
					this.waitDuration = 0;
					this.waitAttempts = 0;
					this.waitStart = Date.now();
					this.waitingForMapImageToLoad = true;

					this.startLoadingPage();
					if (!navigator.onLine)
						return;
				}

				if (this.map.mapImagesHaveLoaded() && this.tour.api.waitForLiveData)
				{
					let message = this.tour.api.waitForLiveDataMessage;
					if (message.length === 0)
						message = `${this.pageName} is waiting for Live Data`;
					this.map.showLoadingMessage(message, true);
				}

				// Check if the maximum number of wait attempts has been exceeded.
				if (this.waitAttempts >= WAIT_ATTEMPTS_LIMIT)
				{
					this.waitingForMapImageToLoad = false;
					this.map.showLoadingMessage(`Could not load ${this.pageKind} for ${this.pageName} after ${this.getWaitDuration()} ms`, true)
					return;
				}

				if (this.isDataSheet && this.tour.bannerImageHasLoaded)
				{
					// An info page has no map image, so don't wait for it to load.
					this.waitingForMapImageToLoad = false;
				}
				else
				{
					// Set a timer to call this method again after one wait period has elapsed.
					this.waitDuration = this.getWaitDuration();
					this.waitAttempts += 1;
					//console.log(`Page::waitForPageToLoad ${this.pageName} WAITING Elaspsed time: ${this.getWaitDuration()} ms`);
					setTimeout(this.waitForPageToLoad, WAIT_PERIOD_MS);

					// Avoid flicker by making the "loading" message visible only after some time has passed.
					if (this.waitAttempts === 10)
						this.map.statusArea.style.visibility = 'visible';

					// Return immediately so that flow of control cannot get past this point until the map image has loaded.
					return;
				}
			}
		}

		// The page is now completely loaded.
		console.log(`Page::waitForPageToLoad ${this.pageName} FINISHED in <= ${this.getWaitDuration()} ms`);

		// Delete the status area element from the DOM since it's no longer needed.
		this.map.statusArea.remove();
		this.map.statusArea = null;

		if (this.tour.editMode)
		{
			// Draw the map and tell map.aspx that the map has loaded. The Map Editor will take over from here.
			this.map.drawMap();
			maOnPageLoaded();
		}
		else
		{
			this.displayPage();
		}

		this.map.listenForEvents();

		this.pageLoaded = true;
	}
}
