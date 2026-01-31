// Copyright (C) 2009-2022 AvantLogic Corporation: https://www.mapsalive.com

export { MapsAliveLiveData };

class MapsAliveLiveData
{
	constructor(tour)
	{
		//console.log(`LiveData__$$::contructor`);

		this.tour = tour;

		this.errorSettings = new MapsAliveLiveDataErrorSettings("Data could not be obtained from the server.", "red", "#eee", true, false);

		this.addV3Compatibility();
	}

	//====================================================================================================
	// Public methods for API callers.
	//====================================================================================================

	//====================================================================================================
	// Private methods.
	//====================================================================================================

	addV3Compatibility()
	{
		// Bind 'this' to methods called using V3 names.
		this.requestHotspotHtml = this.requestHotspotHtml.bind(this);
		this.requestHotspotXml = this.requestHotspotXml.bind(this);
		this.requestHtml = this.requestHtml.bind(this);
		this.requestXml = this.requestXml.bind(this);
		this.setCustomError = this.setCustomError.bind(this);

		window.maLiveDataRequestHotspotHtml = this.requestHotspotHtml;
		window.maLiveDataRequestHotspotXml = this.requestHotspotXml;
		window.maLiveDataRequestHtml = this.requestHtml;
		window.maLiveDataRequestXml = this.requestXml;
		window.maLiveDataSetCustomError = this.setCustomError;
	}

	// ================================================================
	// Public methods
	// ================================================================

	flushCache(hotspotId = 0)
	{
		if (hotspotId === 0)
		{
			for (let viewId in this.tour.currentPage.views)
			{
				let view = this.tour.currentPage.getView(viewId);
				view.liveDataUpdateTime = 0;
			}
		}
		else
		{
			let view = this.tour.currentPage.getViewByHotspotId(hotspotId);
			view.liveDataUpdateTime = 0;
		}
	}

	fulfillRequest(hotspotId, data, cachePeriodSeconds)
	{
		let view = this.tour.currentPage.getViewByHotspotId(hotspotId);
		if (view === null)
			return;
		cachePeriodSeconds = parseInt(cachePeriodSeconds, 10);
		if (isNaN(cachePeriodSeconds))
		{
			this.tour.currentPage.reportLiveDataError(view, null, "A cache-period in seconds was not provided to liveData.fulfillRequest().");
			return;
		}
		view.liveDataCachePeriodMs = cachePeriodSeconds * 1000;

		view.htmlText = data;
		this.tour.currentPage.updateViewWithLiveData(view);
	}

	requestData(responseType, requestId, url)
	{
		this.sendRequest("data", responseType, requestId, url, this.argPairs(arguments, 3), 0);
	}

	requestHotspot(responseType, cachePeriodSeconds, url)
	{
		this.sendRequest("hotspot", responseType, "", url, this.argPairs(arguments, 3), cachePeriodSeconds);
	}

	setCustomError(message, color, backgroundColor, showDetails)
	{
		// Call this function to override the default appearance of error messages.
		this.errorSettings = new MapsAliveLiveDataErrorSettings(message, color, backgroundColor, showDetails, true);
	}

	// ================================================================
	// The remaining public methods are only here for V3 compatibility.
	// They don't call their V4 equivalents because they use argPairs
	// to obtain optional name/value pairs.
	// ================================================================

	// maLiveDataRequestHotspotHtml
	requestHotspotHtml(cachePeriodSeconds, url)
	{
		this.sendRequest("hotspot", "html", "", url, this.argPairs(arguments, 2), cachePeriodSeconds);
	}

	// maLiveDataRequestHotspotXml
	requestHotspotXml(cachePeriodSeconds, url)
	{
		this.sendRequest("hotspot", "xml", "", url, this.argPairs(arguments, 2), cachePeriodSeconds);
	}

	// maLiveDataRequestHtml
	requestHtml(requestId, url)
	{
		this.sendRequest("data", "html", requestId, url, this.argPairs(arguments, 2), 0);
	}

	// maLiveDataRequestXml
	requestXml(requestId, url)
	{
		this.sendRequest("data", "xml", requestId, url, this.argPairs(arguments, 2), 0);
	}

	// ================================================================
	// Private methods
	// ================================================================

	argPairs(args, index)
	{
		// Return an array containing just the argument pairs passed to a Live Data request.
		// This method is needed because the V3 and V4 request args positions are different. 
		return Array.from(args).slice(index);
	}

	createXhr()
	{
		// Create the actual HTTP request object.
		let xhr = null;
		if (window.XMLHttpRequest)
			xhr = new XMLHttpRequest();
		else if (window.ActiveXObject)
			xhr = new ActiveXObject("Microsoft.XMLHTTP");
		return xhr;
	}

	getXmlValue(xml, tagName)
	{
		// Return the data for an XML element returned from the server.
		// If the element is not found or any kind of exception occurs, return null.
		let data = null;
		try
		{
			let nodes = xml.getElementsByTagName(tagName);
			if (nodes.length > 0)
				data = nodes[0].firstChild.data;
		}
		catch (error)
		{
		}
		return data;
	}

	handleResponse(request)
	{
		//console.log(`LiveData::handleResponse readyState:${request.xhr.readyState} status:${request.xhr.status}`);

		// Get the hotspot that the request was made for.
		let view = request.requestType === "hotspot" ? request.view : null;

		try
		{
			if (request.xhr.status === 200)
			{
				if (request.responseType === "json")
					this.handleResponseJson(request, view);
				else if (request.responseType === "xml")
					this.handleResponseXml(request, view);
				else if (request.responseType === "html" || request.responseType === "*")
					this.handleResponseAny(request, view);
				return;
			}

			// Something was wrong with the request. Report an error.
			this.handleResponseError(request, view);
		}
		catch (error)
		{
			let message = `MapsAlive detected an error while interpreting ${request.responseType} data returned in a Live Data response.`;
			message += `\n\nError: "${ error.message }"`;
			message += `\n\nVerify that the data returned from the server is correct and that its format is ${request.responseType}.`;
			if (request.requestType !== "hotspot")
				message += ` Also check that your JavaScript is using the data correctly.`;
			message += `\n\nSee the developer console for a stack trace.`;
			console.error(error.stack);
			this.reportError(request, request, view, message);
		}
	}

	handleResponseAny(request, view)
	{
		let text = request.xhr.responseText;
		if (request.requestType === "hotspot")
		{
			view.htmlText = text;
			this.tour.currentPage.updateViewWithLiveData(view);
		}
		else if (request.requestType === "data")
		{
			let api = window.MapsAlive.getApi(request.instanceName);
			api.callbackLiveDataResponse(request, 0, text, null, 1, 1);
		}
	}

	handleResponseError(request, view)
	{
		let message;
		if (request.xhr.status === 0 && request.xhr.statusText.length === 0)
		{
			message = "The browser did not report why the failure occurred. ";
			message += "Check the JavaScript developer console to see if the cause is reported there. ";
			message += "This can happen when Live Data makes a Cross-Origin Resource Sharing (CORS) request to a server that does not send an Access-Control-Allow-Origin header. ";
			message += "It can also happen if a CORS URL does not exist which triggers a CORS error instead of a 404 error.";
			message += "\n\nTo learn more, see the Server Communication section in the MapsAlive User Guide.";
		}
		else
		{
			message = "Error " + request.xhr.status;
			if (request.xhr.statusText.length > 0)
				message += " - " + request.xhr.statusText;
			else if (request.xhr.status === 404)
				message += ` - The requested URL "${request.url}" was not found`;
			else if (request.xhr.responseText.length > 0)
				message += " - " + request.xhr.responseText;
		}

		this.reportError(request, view, message);
	}

	handleResponseHotspot(html, view)
	{
		// Update the hotspot's HTML only if HTML was provided. If the hotspot's HTML
		// has never been set, set it to a single space. This way the logic that tests
		// that Live Data was received will know there was a response, even if empty. 
		if (html)
			view.htmlText = unescape(html);
		else if (view.htmlText.length === 0)
			view.htmlText = " ";

		this.tour.currentPage.updateViewWithLiveData(view);
	}

	handleResponseHotspotServerError(errorMessage, view, request)
	{
		if (errorMessage && errorMessage.trim())
		{
			view.htmlText = unescape(errorMessage);
			this.tour.currentPage.reportLiveDataError(view, request, errorMessage);
			return true;
		}

		return false;
	}

	handleResponseJson(request, view)
	{
		let json;
		try
		{
			json = JSON.parse(request.xhr.responseText);
		}
		catch (e)
		{
			let error = `Unable to parse Live Data response as JSON: ${e.message}`;
			console.error(error);
			if (request.requestType === "hotspot")
			{
				// The server responded with invalid JSON. Turn it into good JSON as though
				// a server-side error occured and the server reported it within valid JSON.
				json = JSON.parse(`{"error":"${error}"}`);
			}
			else
			{
				let api = window.MapsAlive.getApi(request.instanceName);
				api.callbackLiveDataResponse(request, 0, error, null, 0, 0);
				return;
			}
		}

		if (request.requestType === "hotspot")
			this.handleResponseJsonHotspot(json, view, request);
		else if (request.requestType === "data")
			this.handleResponseJsonData(json, request);
	}

	handleResponseJsonData(json, request)
	{
		let api = window.MapsAlive.getApi(request.instanceName);

		if (json.error)
		{
			api.callbackLiveDataResponse(request, 0, json.error, null, 0, 0);
			return;
		}

		// When the JSON does not contain an array of data objects, return the JSON.
		if (!Array.isArray(json))
		{
			let json = JSON.parse(request.xhr.responseText);
			api.callbackLiveDataResponse(request, 0, json, null, 1, 1);
			return;
		}

		// Call back to the response handler for each of the data objects.
		let index = 0;
		for (const data of json)
		{
			index += 1;
			api.callbackLiveDataResponse(request, data.id, data, null, index, json.length);
		}
	}

	handleResponseJsonHotspot(json, view, request)
	{
		let errorMessage = json.error;
		if (this.handleResponseHotspotServerError(errorMessage, view, request))
			return false;

		this.handleResponseHotspot(json.html, view);

		return true;
	}

	handleResponseXml(request, view)
	{
		// Get the XML from the response.
		let xml = request.xhr.responseXML;
		if (request.requestType === "hotspot")
			this.handleResponseXmlHotspot(xml, view, request);
		else if(request.requestType === "data")
			this.handleResponseXmlData(xml, request);
	}

	handleResponseXmlData(xml, request)
	{
		let api = window.MapsAlive.getApi(request.instanceName);

		// Report an error if the XML is an error.
		if (xml.firstChild.nodeName === "error")
		{
			api.callbackLiveDataResponse(request, 0, unescape(xml.firstChild.textContent), xml, 0, 0);
			return;
		}

		// Get each <data> element and supply a response for each.
		let nodes = xml.getElementsByTagName("data");

		// When the XML does not contain an <data> nodes, return the XML.
		if (nodes.length === 0)
		{
			let xml = request.xhr.responseXML;
			api.callbackLiveDataResponse(request, 0, xml, xml, 1, 1);
			return;
		}

		let i;
		for (i = 0; i < nodes.length; i++)
		{
			let data = "";
			let dataNode = nodes[i];
			let id = dataNode.getAttribute("id");

			if (dataNode.childElementCount === 0)
			{
				// The <data> node's content text.
				data = unescape(dataNode.textContent);
				dataNode = null;
			}

			api.callbackLiveDataResponse(request, id, data, dataNode, i + 1, nodes.length);
		}
	}

	handleResponseXmlHotspot(xml, view, request)
	{
		let errorMessage = this.getXmlValue(xml, 'error');

		// V3 compatibility.
		if (!errorMessage)
			errorMessage = this.getXmlValue(xml, 'serverError');

		if (this.handleResponseHotspotServerError(errorMessage, view, request))
			return false;

		let html = this.getXmlValue(xml, "html");

		// V3 compatibility.
		if (!html)
			html = this.getXmlValue(xml, "text");

		this.handleResponseHotspot(html, view);

		return true;
	}

	parseParameters(args)
	{
		// Construct a name/value parameter list of the form "name1=value1&name2=value2" from a
		// variable length parameter list of the form "'name1', value1, 'name2', value2". Note
		// that there might be parameters before the first pair, for example the cache period
		// and the url, so the caller has to say where to start.
		let parameters = "";
		let argsCount = args.length;
		for (let arg = 0; arg < argsCount; arg += 2)
		{
			// Make sure there are a pair of args left.
			if (arg + 2 > argsCount)
				break;

			if (parameters.length > 0)
				parameters += "&";

			parameters += args[arg] + "=" + args[arg + 1];
		}
		return parameters;
	}

	reportError(request, view, message)
	{
		/**/console.error(message);

		if (request.requestType === "hotspot")
		{
			this.tour.currentPage.reportLiveDataError(view, null, message);
		}
		else
		{
			this.tour.api.callbackLiveDataResponse(request, 0, message, null, 0, 0);
		}
	}

	responseTypeIsValid(requestType, responseType)
	{
		if (!(typeof responseType === 'string'))
			return false;

		if (requestType === "data" && responseType === "*")
			return true;

		return ['html', 'json', 'xml'].includes(responseType.toLowerCase());
	}

	sendRequest(requestType, responseType, requestId, url, args, cachePeriodSeconds)
	{
		if (this.tour.plan !== "Plus" && this.tour.plan !== "Pro")
		{
			console.error(`The MapsAlive Live Data feature is only supported with the Plus and Pro Plans.`);
			let view = this.tour.currentPage.currentView;
			if (view)
				this.tour.currentPage.reportLiveDataError(view, null, "The MapsAlive Live Data feature is only supported with the Plus and Pro Plans.");
			return;
		}

		// Ignore requests while in Tour Preview.
		if (this.tour.preview && this.tour.flagDisableTourPreviewLiveData)
			return;

		if (!this.responseTypeIsValid(requestType, responseType))
		{
			/**/console.error(`Live Data detected an invalid response type "${responseType}"`);
			return;
		}
		responseType = responseType.toLowerCase();

		let parameters = this.parseParameters(args);

		// This is a private method and is subject to change. Users should never call this method directly.
		let view = null;

		//console.log(`LiveData::sendRequest ${requestType}`);

		if (requestType === "hotspot")
		{
			view = this.tour.currentPage.currentView;

			// Make sure there's a view. There should be unless liveData.requestHotspot was not called from a request function.
			if (view === null)
				return;

			requestId = view.hotspotId;

			// The cache period must be specified as the first parameter as a value in seconds.
			// This value is set on a hotspot-by-hotspot basis so that volatile data can be updated
			// more frequently than data that does not change very often.
			cachePeriodSeconds = parseInt(cachePeriodSeconds, 10);
			if (isNaN(cachePeriodSeconds))
			{
				this.tour.currentPage.reportLiveDataError(view, null, "A cache-period in seconds was not provided to liveData.requestHotspotHtml().");
				return;
			}

			// Set the hotspot's cache period in milleseconds. A cache period of zero means
			// only call the server once so set the ms value to the largest number.
			view.liveDataCachePeriodMs = cachePeriodSeconds === 0 ? Number.MAX_VALUE : cachePeriodSeconds * 1000;
		}

		// Create a partial request object in case it's needed for error reporting.
		let request = { requestType, responseType, "id": requestId };

		if (!navigator.onLine)
		{
			this.reportError(request, view, "The browser is not connected to the internet.");
			return;
		}

		// Create the actual HTTP request object.
		let xhr = this.createXhr();
		if (xhr === null)
		{
			// This should never happen, but just in case...
			this.reportError(request, view, "Live Data could not create an HTTP request.");
			return;
		}

		// Create the actual request object that remembers what the request is for.
		request = new MapsAliveLiveDataRequest(this.tour, requestType, responseType, requestId, view, url, xhr, cachePeriodSeconds, parameters);

		// Determine what method to use for the request.
		url += "?";

		// When doing a GET, parameters have to be on the query string and be null in the request object.
		if (parameters.length > 0)
			url += parameters + "&";
		parameters = null;

		// Append the time to prevent the browser from returning cached results for this request.
		url += "_$_=" + new Date().getTime();

		// Create a new request.
		xhr.open("GET", url);

		// Set the function that will be called back when the response for the request is returned by the server.
		// The handleLiveDataResponse callback function will determine which Live Data object made the request
		// and then pass the response to that object's handleResponse method.
		xhr.onreadystatechange = function () { window.MapsAlive.handleLiveDataResponse(request); };

		try
		{
			xhr.send();
		}
		catch (error)
		{
			this.reportError(request, view, error.message);
		}
	}
}

class MapsAliveLiveDataErrorSettings
{
	// This class holds the information needed to report an error.

	constructor(message, color, backgroundColor, showDetail, isCustomError)
	{
		this.message = message;
		this.color = color;
		this.backgroundColor = backgroundColor;
		this.showDetail = showDetail;
		this.isCustomError = isCustomError;
	}
}

class MapsAliveLiveDataRequest
{
	// This class holds the information needed to make a request.

	constructor(tour, requestType, responseType, requestId, view, url, xhr, cachePeriodSeconds, parameters)
	{
		this.tourId = tour.tourId;
		this.instanceName = tour.instanceName;
		this.requestType = requestType;
		this.responseType = responseType;
		this.id = requestId;
		this.view = view;
		this.url = url;
		this.xhr = xhr;
		this.cachePeriodSeconds = cachePeriodSeconds;
		this.parameters = parameters;
	}
}

