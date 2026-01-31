window.onload = function ()
{
	maOnLoad();
	vex.defaultOptions.overlayClosesOnClick = false;
};

function maRunStalePublishedTour(msg, url)
{
	// This method calls Vex directly because maConfirmAndExecuteScript is not available from TourPreview.js.
	vex.dialog.buttons.YES.text = 'RUN CHANGED TOUR';
	vex.dialog.confirm({
		unsafeMessage: msg,
		callback: function (clickedOk)
		{
			if (clickedOk)
				window.open(url, "_blank");
		}
	});
}

function maReturnToTourBuilder(page)
{
	window.location=page;
}

function maBuild(page)
{
	var e = document.getElementById('Tasks');
	e.innerHTML = "Building tour...";
	window.location=page;
}

function maAlert(tourId, msg, buttonText = 'OK')
{
	maCloseV4Popup(tourId);

	// This method calls Vex directly because maConfirmAndExecuteScript is not available from TourPreview.js.
	vex.dialog.buttons.YES.text = buttonText;
	vex.dialog.alert({ unsafeMessage: msg });
}

function maConfirmPublish(tourId, name, url, aid, tbaid)
{
	maCloseV4Popup(tourId);
	let msg = `<p>Publish this tour on the internet?</p><p>The tour URL will be: <b>${url}</b></p>`;

	// This method calls Vex directly because maConfirmAndExecuteScript is not available from TourPreview.js.
	vex.dialog.buttons.YES.text = 'PUBLISH';
	vex.dialog.confirm({
		unsafeMessage: msg,
		callback: function (clickedOk)
		{
			if (clickedOk)
			{
				var e = document.getElementById('StatusMessage');
				e.innerHTML = 'Publishing ' + name + " (please wait)...";
				e.style.color = "red";
				e.style.fontWeight = "bold";
				window.location = 'PerformAction.ashx?aid=' + aid + '&tbaid=' + tbaid;
			}
		}
	});
}

function maCloseV4Popup(tourId)
{
	// Close any open popup for a V4 tour.
	let v3 = typeof maClient !== 'undefined';
	if (!v3)
	{
		let api = window.MapsAlive.getApi(tourId);
		if (api.page.popup)
			api.closePopup();
	}
}

function maPublish(name, url1, url2)
{
	window.location='';
}

function maOnLoad()
{
	window.addEventListener('resize', maOnResize);
	maOnResize();
}

function maOnResize()
{
	let headerControlsElement = document.getElementById('PublishPanel');
	let pageWidth = Math.max(965, document.body.clientWidth);
	let controlsWidth = headerControlsElement.offsetWidth;
	let offset = pageWidth - controlsWidth;
	headerControlsElement.style.left = `${offset}px`;
	headerControlsElement.style.visibility = 'visible';
}

function maOnRollover(over, button, name)
{
	var state = over ? 2 : 1;
	button.src = '../Images/' + name + state + ".gif";
}

function maShowTasks(adviceCount, showTitle, hideTitle)
{
	var s = document.getElementById('TasksPanel').style;
	var show = s.display === "none";
	s.display = show ? "block" : "none";
	var e = document.getElementById('ShowAdvisorOption');
	e.innerHTML = show ? hideTitle : showTitle;
	if (adviceCount === 0)
		document.getElementById("AdviceHeader").style.display = 'none';
	maResizeTourPreview();
}

function maShowSnippets(published, showTitle, hideTitle, tourId)
{
	if (published)
	{
		var s = document.getElementById('SnippetsPanel').style;
		var show = s.display != "block";
		s.display = show ? "block" : "none";
		var e = document.getElementById('ShowSnippetsOption');
		e.innerHTML = show ? hideTitle : showTitle;
		maResizeTourPreview();
	}
	else
	{
		maAlert(tourId, "<p>You must publish this tour to see its code snippets.</p><p>You can publish this tour now by clicking <b>Publish</b>.</p>");
	}
}

function maResizeTourPreview()
{
	// This function gets called whenever the Tour Advisor or Code Snippets panel is shown or hidden,
	// which causes the tour to move up or town in the window, which changes the map x,y coordinates
	// relative to the window. Simulate a resize event to adjust the mouse X,Y values.
	if (typeof maClient !== 'undefined')
	{
		// Only V3 defines maClient so therefore this tour is V3. Send a pseudo resize event to the mapviewer object.
		maClient.map.onResize();
	}
	else
	{
		// The tour is V4. Send a pseudo resize event to both the map and tour objects.
		let tour = window.MapsAlive.firstTourOnPage;
		let map = tour.currentPage.map;
		tour.onResizeTour();
		map.onResize();
	}
}

function maShowIframe(src, w, h)
{
	var o = document.getElementById('ShowIframeOption');
	var e = document.getElementById("IframeDiv");
	var t = document.getElementById("IframeTag");
	if (t)
	{
		e.removeChild(t);
		o.innerHTML = "Show me";
	}
	else
	{
		var i = document.createElement("iframe");
		i.id = "IframeTag";
		i.src = src;
		i.width = w;
		i.height = h;
		i.frameBorder = "0";
		i.scrolling = "no";
		i.style.marginTop = "8px";
		e.appendChild(i);
		o.innerHTML = "Hide iframe";
	}
}

