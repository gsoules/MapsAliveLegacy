// Copyright (C) 2009-2022 AvantLogic Corporation: https://www.mapsalive.com

function maLiveDataSetCustomError(message, color, backgroundColor, showDetails)
{
	// Call this function to override the default appearance of error messages.
	maClient.liveDataErrorSettings = new maLiveDataErrorSettings(message, color, backgroundColor, showDetails);
}

function maLiveDataRequest(requestType, responseType, id, hotspot, url, xhr, cachePeriodSeconds)
{
	// This class holds the information needed to make a request.
	this.requestType = requestType;
	this.responseType = responseType;
	this.id = id;
	this.slide = hotspot;
	this.url = url;
	this.xhr = xhr;
	this.cachePeriodSeconds = cachePeriodSeconds;
}

function maLiveDataSendRequest(cachePeriodSeconds, url)
{
	// This function is for backward compatibility with earlier versions of livedata.js.
	// The original libray used POST so we preserve that here in case anyone is depending on it.
	var parameters = maLiveDataParseParameters(arguments, 2);
	maSendLiveDataRequest("hotspot", "xml", "", cachePeriodSeconds, url, true, parameters);
}

function maLiveDataRequestHotspotXml(cachePeriodSeconds, url)
{
	// Ask the server to return XML to be used as a hotspot's content.
	var parameters = maLiveDataParseParameters(arguments, 2);
	maSendLiveDataRequest("hotspot", "xml", "", cachePeriodSeconds, url, false, parameters);
}

function maLiveDataRequestHotspotHtml(cachePeriodSeconds, url)
{
	// Ask the server to return HTML to be used as a hotspot's content.
	var parameters = maLiveDataParseParameters(arguments, 2);
	maSendLiveDataRequest("hotspot", "text", "", cachePeriodSeconds, url, false, parameters);
}

function maLiveDataRequestXml(requestId, url)
{
	// Ask the server to return XML that user JavaScript is requesting.
	var parameters = maLiveDataParseParameters(arguments, 2);
	maSendLiveDataRequest("server", "xml", requestId, 0, url, false, parameters);
}

function maLiveDataRequestHtml(requestId, url)
{
	// Ask the server to return HTML that user JavaScript is requesting.
	var parameters = maLiveDataParseParameters(arguments, 2);
	maSendLiveDataRequest("server", "text", requestId, 0, url, false, parameters);
}

function maSendLiveDataRequest(requestType, responseType, requestId, cachePeriodSeconds, url, usePost, parameters)
{
	// This is a private method and is subject to change. Users should never call this method directly.
	var hotspot = null;
	
	if (requestType == "hotspot")
	{
		// Get the hotspot that this request is for. If the user's mouse if hovering over a directory
		// entry, we know the request is for the entry's preview image. Otherwise we assume it is
		// for the current hotspot.
		hotspot = maClient.dirPreviewSlide === null ? maClient.slide : maClient.dirPreviewSlide;
		requestId = hotspot.slideId;
		
		// The cache period must be specified as the first parameter as a value in seconds.
		// This value is set on a hotspot-by-hotspot basis so that volatile data can be updated
		// more frequently than data that does not change very often.
		cachePeriodSeconds = parseInt(cachePeriodSeconds, 10);
		if (isNaN(cachePeriodSeconds))
		{
			maClient.reportLiveDataError(hotspot, null, "A cache-period in seconds was not provided as the first parameter to maLiveDataSendRequest().");
			return;
		}
		
		// Set the hotspot's cache period in milleseconds. A cache period of zero means
		// only call the server once so set the ms value to the largest number.
		hotspot.liveDataCachePeriodMs = cachePeriodSeconds === 0 ? Number.MAX_VALUE : cachePeriodSeconds * 1000;
			
		// See if the hotspot was updated from the server within the cache period. If so, ignore this request.
		// If the update time is zero, this hotspot has never gotten data from the server.
		if (hotspot.liveDataUpdateTime !== 0)
		{
			var ms = (new Date()).getTime();
			if (ms - hotspot.liveDataUpdateTime < hotspot.liveDataCachePeriodMs)
				return;
		}
	}
	else if (requestType == "server")
	{
		if (typeof maOnLiveDataResponse == "undefined")
		{
			alert("Calls to maLiveDataRequestXml and maLiveDataRequestHtml will be ignored because no handler is defined for maOnLiveDataResponse.");
			return;
		}
	}

	// Create the actual HTTP request object.
	var xhr = maCreateLiveDataXhr();
	if (xhr === null)
	{
		// This should never happen, but just in case...
		maReportLiveDataError(requestType, requestId, hotspot, "Live Data could not create an HTTP request.");
		return;
	}
	
	// Create our request object that remembers what the request is for.
	var request = new maLiveDataRequest(requestType, responseType, requestId, hotspot, url, xhr, cachePeriodSeconds);
	
	// Determine what method to use for the request.
	var method;
	if (usePost)
	{
		// When doing a POST, parameters are passed inside the request object.
		method = "POST";
	}
	else
	{
		method = "GET";
		url += "?";
			
		// When doing a GET, parameters have to be on the query string and be null in the request object.
		if (parameters.length > 0)
			url += parameters + "&";
		parameters = null;
		
		// Append the time to prevent the browser from returning cached results for this request.
		url += "request=" + new Date().getTime();
	}
	
	// Make the actual request. Note that the sequence of statements of below is important.
	// In particular, the open call should precede setting the onreadystatechange property.
	xhr.open(method, url);
	xhr.onreadystatechange = function() { maLiveDataResponseHandler(request);};
	
	if (usePost)
	{
		// Tell the server that the data being sent is name-value pairs.
		xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
	}
	
	xhr.send(parameters);
}

function maReportLiveDataError(requestType, requestId, hotspot, message)
{
	if (requestType == "hotspot")
		maClient.reportLiveDataError(hotspot, null, message);
	else
		maOnLiveDataResponse(requestId, 0, message);
}

function maCreateLiveDataXhr()
{
	// Create the actual HTTP request object.
	var xhr = null;
	if (window.XMLHttpRequest)
		xhr = new XMLHttpRequest();
	else if (window.ActiveXObject)
		xhr = new ActiveXObject("Microsoft.XMLHTTP");
	return xhr;
}

function maLiveDataResponseHandler(request)
{
	// Handle responses from the server. 
	if (request.xhr.readyState != 4)
		return;
		
	// Get the hotspot that the request was made for.
	var hotspot = request.requestType == "hotspot" ? request.slide : null;
		
	try
	{
		if (request.xhr.status == 200)
		{
			if (request.responseType == "xml")
			{
				// Get the XML from the response.
				var xml = request.xhr.responseXML;
				var data;
				
				if (request.requestType == "hotspot")
				{
					// Parse the <hotspot> element and update the hotspot from it.
					data = maLiveDataGetXmlValue(xml, 'serverError');
					if (data)
					{
						hotspot.htmlText = unescape(data);
						maClient.reportLiveDataError(hotspot, request, data);
						return;
					}

					data = maLiveDataGetXmlValue(xml, 'text');
					if (data)
						hotspot.htmlText = unescape(data);

					data = maLiveDataGetXmlValue(xml, 'previewImage');
					if (data)
						hotspot.dirPreviewImageUrl = data;

					data = maLiveDataGetXmlValue(xml, 'previewText');
					if (data)
						hotspot.dirPreviewText = unescape(data);

					// Tell mapsalive.js that this hotspot's Live Data has been updated.
					maClient.setLiveData(hotspot);
				}
				else
				{
					// Get each <data> element and supply a response for each.
					var nodes = xml.getElementsByTagName("data");
					for (i = 0; i < nodes.length; i++)
					{
						var node = nodes[i];
						var dataId = node.getAttribute("id");
						data = unescape(node.firstChild.data);
						maOnLiveDataResponse(request.id, dataId, data, node);
					}
				}
			}
			else
			{
				// Get the text from the response.
				var text = request.xhr.responseText;
				
				if (request.requestType == "hotspot")
				{
					hotspot.htmlText = text;
					maClient.setLiveData(hotspot);
				}
				else
				{
					maOnLiveDataResponse(request.id, "", text, null);
				}
			} 
		}
		else
		{
			// Something was wrong with the request. Report an error.
			var message = "Error " +  request.xhr.status + " - " + request.xhr.statusText;
			maReportLiveDataError(request.requestType, request.id, hotspot, message);
		}
	}
	catch (error)
	{
		// Handle a Firefox bug where it will sometimes throw an exception instead of a request error.
		maReportLiveDataError(request.requestType, request.id, hotspot, error.message);
	}
}

function maLiveDataGetXmlValue(xml, tagName)
{
	// Return the data for an XML element returned from the server.
	// If the element is not found or any kind of exception occurs, return null.
	var data = null;
	try
	{
		var nodes = xml.getElementsByTagName(tagName);
		if (nodes.length > 0)
			data = nodes[0].firstChild.data;
	}
	catch (error)
	{
	}
	return data;
}

function maLiveDataParseParameters(args, firstPairIndex)
{
	// Construct a name/value parameter list of the form "name1=value1&name2=value2" from a
	// variable length parameter list of the form "'name1', value1, 'name2', value2". Note
	// that there might be parameters before the first pair, for example the cache period
	// and the url, so the caller has to say where to start.
	var parameters = "";
	var argsCount = args.length;
	for (var arg = firstPairIndex; arg < argsCount; arg += 2)
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

