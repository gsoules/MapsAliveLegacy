// Copyright (C) 2009-2022 AvantLogic Corporation: https://www.mapsalive.com

export { MapsAliveView };

class MapsAliveView
{
	constructor(
		page,
		viewId,
		pageNumber,
		hotspotId,
		title,
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
		onMap)
{
		//console.log(`View__$$::contructor`);

		this.page = page;
		this.viewId = viewId;
		this.pageNumber = pageNumber;
		this.hotspotId = hotspotId;

		// Titles that start with an underscore are excluded from the directory.
		// WHen there's an underscore, remove it and set the excluded flag.
		if (title.length > 0 && title.substring(0, 1) === '_')
		{
			this.excludedFromDirectory = true;
			this.title = title.substring(1);
		}
		else
		{
			this.title = title;
			this.excludedFromDirectory = false;
		}

		// When using Live Data, ignore any text that the user provided for the view.
		this.htmlText = usesLiveData ? "" : htmlText;

		this.plainText = plainText;

		this.imageSrc = imageSrc && imageSrc.length > 0 ? this.page.tour.path + imageSrc : null;
		this.mediaW = mediaW;
		this.mediaH = mediaH;

		// Determine the amount of width that the image is not using within the available area.
		this.mediaDeltaW = Math.max(0, this.page.mediaAreaW - mediaW);

		this.imageLoaded = false;

		this.hasImage = this.imageSrc !== null && mediaW > 0;

		if (this.hasImage)
		{
			this.hasImage = true;
			this.image = new Image();
			this.image.id = this.page.hasPopup ? this.page.tour.uniqueId('PopupImage') : this.page.tour.uniqueId('HotspotImage');
			this.image.className = this.page.hasPopup ? 'maPopupImage' : 'maHotspotImage';
			this.image.alt = 'Hotspot image';

			// Set up the load and error handlers.
			this.image.onload = this.onImageLoad;
			this.image.onerror = this.onImageError;

			// Set vertical-align to 'middle' (or almost anything except 'baseline') to prevent browsers
			// from leaving space below the image as is done for elements that need to account for text
			// that has descenders like the letters j, p, g.
			this.image.style.verticalAlign = 'middle';

			// Give the image a pointer back to this view.
			this.image.view = this;
		}

		this.mediaType = mediaType;

		this.embedText = embedText;
		this.hasEmbeddedMedia = embedText.length > 0;

		this.popupOverrideW = popupOverrideW;
		this.popupOverrideH = popupOverrideH;

		this.inSearchResults = true;
		this.searchStart = -1;
		this.searchLength = 0;

		this.usesLiveData = usesLiveData;
		this.messengerFunction = messengerFunction;
		this.liveDataUpdateTime = 0;
		this.liveDataCachePeriodMs = 0;
		this.liveDataRequestPending = false;
		this.liveDataErrorOccurred = false;
		this.pointer = null;

		this.dirPreviewImageUrl = dirPreviewImageUrl;
		this.dirPreviewText = dirPreviewText;

		this.onMap = onMap === 1;

		// The media property only gets set after a Live Data response from the server.
		this.media = null;
	}

	hasNoContentForLayout()
	{
		let layoutId = this.page.layoutId;

		if (layoutId === 'HTT' && this.htmlText.length === 0 && !this.usesLiveData)
			return true;

		if (layoutId === 'HII' && !this.hasImage)
			return true;

		return this.htmlText.length === 0 && !this.usesLiveData && !this.hasImage;
	}

	liveDataCachePeriodHasExpired()
	{
		// The data is considered expired if there is no data.
		if (this.htmlText.length === 0)
			return true;

		// When in Tour Preview, the data will be an error message.
		if (this.page.tour.preview && this.page.tour.flagDisableTourPreviewLiveData)
			return false;

		// When the cache period is zero, the data is only fetched once during the life of the tour.
		if (this.liveDataCachePeriodMs === 0)
			return false;

		let timeNowMs = (new Date()).getTime();
		let cacheAgeMs = timeNowMs - this.liveDataUpdateTime;
		let expired = cacheAgeMs > this.liveDataCachePeriodMs;
		console.log(`View::liveDataCachePeriodHasExpired ${expired}`);
		return expired;
	}

	onImageError()
	{
		this.view.page.tour.okayToPreloadNextImage = true;
	}

	onImageLoad()
	{
		// NOTE: "this" is an img object. It has a "view" property that is the image's MapsAliveView object. 
		//console.log(`View::onImageLoad ${this.view.viewId} ${this.view.imageSrc}`);
		this.view.imageLoaded = true;

		// Allow the next view's image to load.
		this.view.page.tour.okayToPreloadNextImage = true;

		this.view.page.onViewImageLoaded(this.view.viewId);
	}

	setViewUsesLiveData(uses)
	{
		this.usesLiveData = uses;
		if (uses)
		{
			let marker = this.page.map.getMarker(this.viewId);
			marker.showsContentOnlyInTooltip = false;
		}
	}
}
