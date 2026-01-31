// Copyright (C) 2009-2022 AvantLogic Corporation: https://www.mapsalive.com

export { MapsAliveDirectory, MapsAliveDirectoryEntry, MapsAliveDirectoryLevel };

	import { MapsAliveRuntime as Runtime__$$ } from './mapsalive-runtime.js';

class MapsAliveDirectory
{
	constructor(tour)
	{
		//console.log(`Directory__$$::constructor`);

		this.tour = tour;

		this.onClearSearchResults = this.onClearSearchResults.bind(this);
		this.onClickGroupHeader = this.onClickGroupHeader.bind(this);
		this.onClickHotspotEntry = this.onClickHotspotEntry.bind(this);
		this.onClickPageEntry = this.onClickPageEntry.bind(this);
		this.onHidePreview = this.onHidePreview.bind(this);
		this.onSearchChanged = this.onSearchChanged.bind(this);
		this.onShowPreview = this.onShowPreview.bind(this);
		this.onToggleOrder = this.onToggleOrder.bind(this);
		this.showPreviewPanel = this.showPreviewPanel.bind(this);

		this.DIR_MODE_ALPHA = 1;
		this.DIR_MODE_GROUP = 2;

		this.firstUse = true;
		this.collapsed = new Array();
		this.searchPattern = "";
		this.hotspotCount = 0;

		this.liveDataPreviewIntervalId = 0;
		this.liveDataPreviewViewId = 0;
	}

	createDirectoryPanel()
	{
		// This method creates the directory panel and the data that goes into it. Normally
		// the directory panel is used for directory entries, but if the directory feature
		// is turned off and the show hotspots for page in menu feature is turned on, the panel
		// is used to show the hotspots. That's why the code often checks for tour.hasDirectory.

		this.dirMode = this.showGroupSort ? this.DIR_MODE_GROUP : this.DIR_MODE_ALPHA;

		// Create the alphabetically sorted directory.
		this.hotspotCount = this.createOrderAlphabetically();

		if (this.hotspotCount === 0)
			return;

		if (this.tour.hasDirectory && (this.showSearch || this.showGroupSort))
		{
			this.dirHeader = document.createElement("div");
			this.page.layout.navPanel.appendChild(this.dirHeader);
			this.dirHeader.className = 'maDirHeader';
		}

		if (this.showAllPages || !this.page.isDataSheet)
		{
			this.dirPanel = document.createElement("div");
			this.dirPanel.id = this.tour.uniqueId('DirPanel');
			this.dirPanel.className = 'maDirPanel';
			this.page.layout.navPanel.appendChild(this.dirPanel);

			if (!(this.showSearch || this.showGroupSort))
				this.dirPanel.classList.add('maNoDirHeader');
		}

		if (this.tour.hasDirectory)
			this.createSortSearchPanel();

		if (this.showImagePreview || this.showTextPreview)
		{
			this.dirPreviewPanel = document.createElement("div");
			this.dirPreviewPanel.id = this.tour.uniqueId("DirPreview");
			this.dirPreviewPanel.className = "maDirPreview";
			this.page.layout.layoutElement.appendChild(this.dirPreviewPanel);
		}

		// Create the sort by group directory.
		if (this.tour.hasDirectory && this.showGroupSort)
			this.createOrderByGroup();

		// Set the search text if we came here from another page that had search text.
		let find = Runtime__$$.getQueryStringArg("find");
		if (find.length !== 0)
			this.dirSearchBox.value = find;

		if (this.tour.hasDirectory)
			this.search();

		if (this.tour.hasDirectory && this.showSearch)
			this.dirSearchBox.addEventListener("keyup", this.onSearchChanged);

		this.dirShowing = false;
		this.tour.showingPreview = false;

		if (this.tour.hasDirectory && this.staysOpen)
			this.showEntries(true)

		this.firstUse = false;

		// When the directory is on the left side of the page, display the preview on the right.
		this.previewOnRight = this.page.layout.navButtonLocationIsLeft;
	}

	createEntryForGroup(parentLevel, groupId, depth, levelId, pageNumber)
	{
		// Create the level's outer <div>.
		let levelDiv = document.createElement("div");
		levelDiv.className = "maDirLevel" + depth;

		let groupName = this.page.lookupString(groupId);

		let groupHeader = document.createElement("div");
		groupHeader.className = 'maDirGroupHeader';
		levelDiv.appendChild(groupHeader);

		groupHeader.dataset.levelId = levelId;
		groupHeader.dataset.depth = depth;
		groupHeader.addEventListener('click', this.onClickGroupHeader, false);

		let expanderDiv = document.createElement("img");
		expanderDiv.className = "maDirExpander";
		groupHeader.appendChild(expanderDiv);

		let grouptDiv = document.createElement("div");
		grouptDiv.className = "maDirGroup";
		grouptDiv.innerHTML = groupName;
		groupHeader.appendChild(grouptDiv);

		// Create a <span> to contain the level's count.
		let countSpan = document.createElement("span");
		countSpan.className = "maDirLevelCount";
		groupHeader.appendChild(countSpan);

		// Create the level's inner div that will contain sub-levels or entries.
		let contentDiv = document.createElement("div");
		contentDiv.id = this.tour.uniqueId("Group" + levelId);
		contentDiv.className = "maDirGroupContent";

		// Determine if this level should be expanded or collapsed. The default is that
		// level 1 is collapsed and level 2 is expanded. However, if a level's Id was
		// passed as part of the collapse state of a calling page, then use the opposite state.
		let collapsed;
		if (depth === 1 && this.openExpanded && this.firstUse)
		{
			// The directory is being created for the first time and open-expanded was requested. If auto-collapse
			// was also requested, then show only the current page expanded, otherwise, show all pages expanded.
			if (this.autoCollapse)
				collapsed = pageNumber !== this.page.pageNumber;
			else
				collapsed = false;
		}
		else if (this.firstUse)
		{
			collapsed = depth === 1;
		}
		else
		{
			collapsed = this.collapsed[levelId];
		}

		// Don't show the directory expanded for a different page than the current page.
		if (!collapsed && pageNumber !== this.page.pageNumber)
			collapsed = true;

		this.collapsed[levelId] = collapsed;
		contentDiv.style.display = collapsed ? 'none' : 'block';
		levelDiv.appendChild(contentDiv);
		expanderDiv.src = this.getExpanderIcon(collapsed);

		// Attach the outer div to its parent div.
		parentLevel.contentDiv.appendChild(levelDiv);

		// Create a new level object and make it a child of its parent level object.
		let level = new MapsAliveDirectoryLevel(parentLevel, levelId, depth, levelDiv, contentDiv, countSpan);
		parentLevel.content.push(level);

		return level;
	};

	createEntryForHotspot(parentLevel, view, depth)
	{
		// Create the entry's <div>.
		let div = document.createElement("div");
		div.className = "maDirEntry";

		//console.log(`Directory::createEntryForHotspot ${depth}`);

		// Set the width of the entry div so that the text-overflow:ellipsis CSS will work.
		// The math to compute the indent is an approximation and does not take into account
		// whether there's a vertical scroll bar which makes the available width narrower.
		let indent = depth === 0 ? 36 : depth * 32;
		div.style.width = this.page.layout.navPanelSize.w - indent - 16 + 'px';

		let paddingLeft = this.showGroupSort ? 20 : 0;
		div.style.paddingLeft = paddingLeft + 'px';

		div.dataset.viewId = view.viewId;
		div.dataset.levelId = parentLevel.id;

		div.addEventListener('click', this.onClickHotspotEntry, false);

		div.innerHTML = view.title;

		if (this.tour.hasDirectory && (this.showImagePreview || this.showTextPreview) && !this.page.layout.usingMobileLayout)
		{
			div.addEventListener('mouseover', this.onShowPreview, false);
			div.addEventListener('mouseout', this.onHidePreview, false);
		}

		// Attach the div to it parent div.
		parentLevel.contentDiv.appendChild(div);

		// Create a new entry object and make it a child of its parent level object.
		let entry = new MapsAliveDirectoryEntry(parentLevel, div, view);
		parentLevel.content.push(entry);
	};

	createOrderAlphabetically()
	{
		let div = document.createElement("div");
		this.dirBodyAlphabetic = new MapsAliveDirectoryLevel(null, 0, 0, null, div, null);

		//console.log("dirCreateOrderAlpha");

		// Create an array of views so that we can sort the views object which is not an array.
		let views = new Array();

		for (const page of this.tour.pages)
		{
			let isCurrentPage = page.pageNumber === this.page.pageNumber;

			if (page.excludeFromNavigation)
			{
				continue;
			}
			else if (this.tour.hasDirectory)
			{
				if (!this.showAllPages && !isCurrentPage)
					continue;
			}

			for (let viewId in page.views)
			{
				let view = page.getView(viewId);

				// Don't show an entry for hotspot that is not on the map.
				if (!view.onMap)
					continue;

				if (!view.excludedFromDirectory)
					views.push(view);
			}
		}

		// Sort the views alphabetically by title.
		views.sort(this.tour.sortHotspotsByTitle);

		// Create an entry for each hotspot.
		for (let viewIndex in views)
		{
			let view = views[viewIndex];
			this.createEntryForHotspot(this.dirBodyAlphabetic, view, 0);
		}

		return views.length;
	};

	createOrderByGroup()
	{
		let div = document.createElement("div");
		this.dirBodyLevels = new MapsAliveDirectoryLevel(null, 0, 0, null, div, null);

		// The first value is the depth of the view entries. The depth is 2 when grouping by
		// page or category. It's 3 when grouping by page then catetory, or by category then page.
		let entryDepth = this.table[0];
		let i = 1;

		let levelId = 0;
		let parentLevel = new Array();
		parentLevel[0] = this.dirBodyLevels;

		while (i < this.table.length)
		{
			let depth = this.table[i];
			let pageNumber = this.table[i + 1];
			let isDataSheet = depth < 0;

			if (depth === entryDepth)
			{
				if (this.showAllPages || pageNumber === this.page.pageNumber)
				{
					let viewId = this.table[i + 2];
					let view = this.tour.getViewFromAnyPage(viewId);

					if (!view.excludedFromDirectory)
					{
						depth = depth - 1;
						this.createEntryForHotspot(parentLevel[depth], view, depth);
					}
				}
			}
			else if (!isDataSheet)
			{
				levelId++;
				let groupId = this.table[i + 2];
				parentLevel[depth] = this.createEntryForGroup(parentLevel[depth - 1], groupId, depth, levelId, pageNumber);
			}

			i += 3;
		}
	};

	createSortSearchPanel()
	{
		if (!(this.showGroupSort || this.showSearch))
			return;

		let sortSearchDiv = document.createElement("div");
		sortSearchDiv.className = 'maDirHeaderSortSearch';
		sortSearchDiv.style.display = 'flex';
		sortSearchDiv.style.flexDirection = 'row';
		sortSearchDiv.style.justifyContent = 'space-between';
		sortSearchDiv.style.alignItems = 'center';
		this.dirHeader.appendChild(sortSearchDiv);

		if (this.showGroupSort)
		{
			this.sortToggle = document.createElement("div");
			this.sortToggle.className = "maDirToggle";
			this.sortToggle.innerHTML = this.dirMode === this.DIR_MODE_GROUP ? this.textAlphaSort : this.textGroupSort;
			sortSearchDiv.appendChild(this.sortToggle);
			this.sortToggle.addEventListener("click", this.onToggleOrder);
		}

		if (this.showSearch)
		{
			let searchBoxDiv = document.createElement("div");
			searchBoxDiv.className = 'maDirSearchContainer';
			sortSearchDiv.appendChild(searchBoxDiv);

			let searchImg = document.createElement("img");
			searchImg.className = 'maDirSearchIcon';
			searchImg.alt = "Search icon";
			searchImg.src = this.tour.graphics["dirSearch"].src;
			searchBoxDiv.appendChild(searchImg);

			let searchBoxInput = document.createElement("input");
			searchBoxInput.className = "maDirSearchBox";
			searchBoxInput.type = "text";
			searchBoxInput.placeholder = this.textSearchLabel;
			searchBoxInput.value = this.searchPattern;
			searchBoxDiv.appendChild(searchBoxInput);

			let searchBoxX = document.createElement("img");
			searchBoxX.className = 'maDirSearchX';
			searchBoxX.alt = "Clear search text";
			searchBoxX.src = this.tour.graphics["mobileCloseX"].src;
			searchBoxDiv.appendChild(searchBoxX);
			searchBoxX.addEventListener("click", this.onClearSearchResults);

			this.dirSearchBox = searchBoxInput;
		}
	};

	getDirLevelContentElement(levelId)
	{
		let divId = this.tour.uniqueId("Group" + levelId);
		return document.getElementById(divId);
	};

	getExpanderIcon(collapsed)
	{
		return collapsed ? this.tour.graphics["dirExpand"].src : this.tour.graphics["dirContract"].src
	}

	getSearchText()
	{
		if (this.showSearch)
			return this.dirSearchBox.value;
		else
			return "";
	};

	highlightSearchText(view)
	{
		let searchText = view.dirPreviewText;
		if (searchText.length === 0)
			searchText = view.plainText;
		let searchStart = view.searchStart.split(',');
		let lastStart = 0;
		let html = "";

		if (view.searchLength >= 2 && view.searchStart.length > 0)
		{
			for (let index = 0; index < searchStart.length; index++)
			{
				let start = parseInt(searchStart[index], 10);
				let found = searchText.substr(start, view.searchLength);
				if (found.length > 0)
				{
					let className = "maDirSearchHitHighlight";
					html +=
						searchText.substring(lastStart, start) +
						"<span class='" + className + "'>" + found + "</span>";
				}
				lastStart = start + view.searchLength;
			}
		}

		html += searchText.substring(lastStart);

		return html;
	};

	movePreviewPanel(mouse)
	{
		let panelRect = this.page.layout.layoutElement.getBoundingClientRect();
		mouse.x -= panelRect.left;
		mouse.y -= panelRect.top;

		let border = 2;
		let offset;
		if (this.previewOnRight)
		{
			offset = 40;
		}
		else
		{
			offset = this.tour.showingPreviewImageOnly ? this.previewWidth + border : this.previewWidth;
			offset = -(offset + 32);
		}
		this.dirPreviewPanel.style.left = mouse.x + offset + "px";
		this.dirPreviewPanel.style.top = mouse.y - 8 + "px";
	};

	onClearSearchResults()
	{
		this.dirSearchBox.value = "";
		this.search();
	};

	onClickHotspotEntry(event)
	{
		event.stopPropagation();

		this.page.layout.closeNavPanel();

		let viewId = parseInt(event.target.dataset.viewId, 10);
		this.showEntry(viewId);
	}

	onClickGroupHeader(event)
	{
		let target = event.currentTarget;
		let levelId = parseInt(target.dataset.levelId, 10);
		let depth = parseInt(target.dataset.depth, 10);

		let div = this.getDirLevelContentElement(levelId);

		// Toggle the collapse state.
		let collapsed = this.collapsed[levelId];
		collapsed = !collapsed;
		this.collapsed[levelId] = collapsed;
		div.style.display = collapsed ? 'none' : 'block';
		let expander = target.querySelector('.maDirExpander');
		expander.src = this.getExpanderIcon(collapsed);

		// Only toggle when clicking a level 1 item.
		if (depth > 1)
			return;

		if (this.autoCollapse)
		{
			// Loop over all the level 1 levels hiding all but one.
			let levels = this.dirBodyLevels.content;
			for (let index = 0; index < levels.length; index++)
			{
				let level = levels[index];
				if (level.depth > 1)
					continue;

				if (level.contentDiv.id !== div.id)
				{
					level.contentDiv.style.display = 'none';
					this.collapsed[level.id] = true;
					let expander = level.outerDiv.querySelector('.maDirExpander');
					if (expander && expander !== event.target)
						expander.src = this.getExpanderIcon(true);
				}
			}
		}

		// Ignore the next mouseout event to deal with the case where collapsing a category
		// causes the panel to shorten so much that its bottom is above the current cursor
		// location. This is the only time it is possible to move the mouse around the map
		// with the directory expanded, but if we don't do this, the user clicks a new
		// category and the panel closes immediately.
		this.tour.okToCloseDirectoryPanel = false;
	};

	onClickPageEntry(event)
	{
		event.stopPropagation();

		let pageNumber = parseInt(event.target.dataset.pageNumber, 10);
		this.tour.goToPage(pageNumber);
	}

	onHidePreview()
	{
		let previewDiv = this.dirPreviewPanel;

		if (!previewDiv)
			return;

		clearInterval(this.liveDataPreviewIntervalId);
		this.liveDataPreviewViewId = 0;

		this.showPreviewPanel(false);
		this.tour.dirPreviewView = null;
		this.tour.showingPreview = false;
	};

	onSearchChanged()
	{
		if (!this.dirShowing)
			this.showEntries(true);

		this.search();
	};

	onShowPreview(event)
	{
		// Handle the case where the user has clicked a directory entry, and as the nav panel
		// is closing, a ghost mouseover occurs on one of the other entries which would cause
		// that entry's preview to display if the event were not ignored here.
		if (!this.page.layout.navPanelIsShowing)
			return;

		let previewDiv = this.dirPreviewPanel;

		if (!previewDiv)
			return;

		this.tour.showingPreview = true;

		let viewId;
		
		// When there's an event, the view Id is in the dataset. No event means that this
		// is a callback from the setTimeout call below to wait for Live Data results.
		if (event)
			viewId = parseInt(event.target.dataset.viewId, 10);
		else
			viewId = this.liveDataPreviewViewId;

		let view = this.tour.getViewFromAnyPage(viewId);

		// Don't show preview text for Live Data while the user is mousing across the directory entries because
		// fetching the data from the server is expensive and usually the mouse is not over an entry long enough
		// for the response to come back and get displayed. If showing Live Data in preview ever becomes necessary,
		// add logic that only makes the server request if the mouse hovers over the entry for a while.
		if (view.usesLiveData)
			return;

		this.tour.dirPreviewView = view;

		// Determine if there is preview text.
		let	html = this.highlightSearchText(view);

		let noText = html.length === 0 || !this.showTextPreview;
		let innerHtml = "";

		// Determine if there is a preview image. An image URL of "0" means don't show a preview for
		// this view. We use the suppress feature to keep from displaying a preview for data sheets.
		let suppressPreview = view.dirPreviewImageUrl === "0";
		let noImage = this.page.mediaW === 0 || view.imageSrc === null || suppressPreview;

		if (noImage && !suppressPreview && view.dirPreviewImageUrl.length)
		{
			// There's no MapsAlive image to show, but the user has provided an image URL via Live Data.
			noImage = false;
		}

		if (this.showImagePreview && !noImage)
		{
			// Determine the preview image width. If using an image URL that the user provided, we don't know
			// the image size, so use the preview width that the user set for the directory. Otherwse, compare
			// the media width and the preview width and use whichever is smaller.
			let w;
			if (view.dirPreviewImageUrl.length)
				w = Math.min(this.previewImageWidth, this.previewWidth);
			else
				w = view.mediaW < this.previewImageWidth ? view.mediaW : this.previewImageWidth;

			let width = "width:" + w + "px;";
			let imgSrc = view.dirPreviewImageUrl.length ? view.dirPreviewImageUrl : this.page.viewImageSrc(view);
			let imgTag = "<img src='" + imgSrc + "' style='" + width + "' class='maDirPreviewImage'/>";
			innerHtml += imgTag;
		}

		this.tour.showingPreviewImageOnly = false;

		if (noText)
		{
			if (this.showImagePreview && !noImage)
				this.tour.showingPreviewImageOnly = true;
		}
		else
		{
			innerHtml += `<div class='maDirPreviewText'><div>${html}</div></div>`;
		}

		if (innerHtml.length === 0)
		{
			previewDiv.style.visibility = "hidden";
		}
		else if (event)
		{
			let mouse = { x: event.clientX, y: event.clientY };
			this.movePreviewPanel(mouse);
			previewDiv.style.padding = 0;
			previewDiv.innerHTML = innerHtml;
			previewDiv.style.width = this.previewWidth + "px";

			this.showPreviewPanel(true);
		}
	};

	onToggleOrder()
	{
		if (this.dirMode === this.DIR_MODE_ALPHA)
		{
			this.sortToggle.innerHTML = this.textAlphaSort;
			this.dirMode = this.DIR_MODE_GROUP;
		}
		else
		{
			this.sortToggle.innerHTML = this.textGroupSort;
			this.dirMode = this.DIR_MODE_ALPHA;
		}

		this.showEntries(true);
	};

	search()
	{
		if (this.showImagePreview || this.showTextPreview)
			this.onHidePreview();

		this.searchPattern = this.getSearchText().toLowerCase();
		let clear = this.searchPattern.length <= 1;
		let patternLength = this.searchPattern.length;

		// Not currently using the result count, but keeping for now from the V3 logic.
		let resultCount = 0;

		for (const page of this.tour.pages)
		{
			if (!this.showAllPages && page.pageNumber != this.page.pageNumber)
				continue;

			for (let viewId in page.views)
			{
				let view = page.getView(viewId);

				view.searchStart = "";
				view.searchLength = 0;

				if (clear)
				{
					view.inSearchResults = true;
					resultCount++;
					continue;
				}

				view.inSearchResults = false;
				view.searchLength = patternLength;
				let index = 0;
				let offset = 0;

				// First search the view's title.
				if (view.title.toLowerCase().indexOf(this.searchPattern) >= 0)
				{
					view.inSearchResults = true;
					resultCount++;
				}

				if (view.plainText.length === 0)
					continue;

				// Then search the view's text
				let searchText = view.plainText.toLowerCase();

				while (offset !== -1)
				{
					let text = searchText.substr(index);
					offset = text.indexOf(this.searchPattern);
					if (offset >= 0)
					{
						view.inSearchResults = true;
						if (view.searchStart.length > 0)
							view.searchStart += ",";
						view.searchStart += index + offset;
						index += offset + patternLength;
						if (index >= searchText.length)
							offset = -1;
						resultCount++;
					}
				}
			}
		}

		if (this.tour.hasDirectory && this.showGroupSort)
			this.showSearchResults(this.dirBodyLevels, clear);

		this.showSearchResults(this.dirBodyAlphabetic, false);
	};

	showEntries(show)
	{
		if (this.hotspotCount === 0)
			return;

		if (show)
		{
			if (!this.staysOpen)
			{
				// Close the popup when showing the directory unless the directory is always open.
				// If we close the popup in that case, the initial display of the directory after the map 
				// loads would close a popup that was opened because its hotspot Id was on the query string.		
				this.page.closePopup();
			}

			if (this.showAllPages || !this.page.isDataSheet)
			{
				let firstChild = this.dirPanel.firstChild;
				if (firstChild)
					this.dirPanel.removeChild(firstChild);

				if (this.dirMode === this.DIR_MODE_ALPHA)
					this.dirPanel.appendChild(this.dirBodyAlphabetic.contentDiv);
				else
					this.dirPanel.appendChild(this.dirBodyLevels.contentDiv);
			}
		}
		else
		{
			this.dirMouseIsOver = false;
		}

		this.dirPanel.style.display = show || this.staysOpen ? 'block' : 'none';
		this.dirShowing = show || this.staysOpen;
	};

	showEntry(viewId)
	{
		this.onHidePreview();

		let view = this.tour.getViewFromAnyPage(viewId);
		if (view)
		{
			this.tour.api.callbackDirectoryEntryClicked(view);

			// Determine if the view Id is on this page or on another page.
			if (view.pageNumber === this.page.pageNumber)
				this.page.map.selectMarkerChosenFromDirectory(viewId);
			else
				this.tour.goToPageView(view.pageNumber, viewId);
		}
		else
		{
			Runtime__$$.assert(false, `No view found for viewId '${viewId}'`);
		}
	};

	showPreviewPanel(show)
	{
		this.dirPreviewPanel.style.visibility = show ? "visible" : "hidden";
	};

	showSearchResults(level, clear)
	{
		// Loop over all the levels exposing/hiding ones with/without entries.
		let resultCount = 0;
		for (let index = 0; index < level.content.length; index++)
		{
			let o = level.content[index];
			if (o instanceof MapsAliveDirectoryEntry)
			{
				let inSearchResults = o.view.inSearchResults;
				o.div.style.display = inSearchResults ? 'block' : 'none';
				if (inSearchResults)
					resultCount++;
			}
			else
			{
				resultCount += this.showSearchResults(o, clear);
			}
		}
		if (level.outerDiv !== null)
		{
			level.countSpan.innerHTML = "&nbsp;(" + resultCount + ")";
			level.outerDiv.style.display = resultCount > 0 ? 'block' : 'none';

			let div = this.getDirLevelContentElement(level.id);
			if (div && clear && level.depth === 1)
				div.style.display = this.collapsed[level.id] ? 'none' : 'block';
		}

		level.resultCount = resultCount;
		return resultCount;
	};

	get page()
	{
		return this.tour.currentPage;
	}
}

class MapsAliveDirectoryEntry
{
	constructor(parentLevel, div, view)
	{
		this.div = div;
		this.parentLevel = parentLevel;
		this.view = view;
	}
}

class MapsAliveDirectoryLevel
{
	constructor(parentLevel, id, depth, outerDiv, contentDiv, countSpan)
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
}
