// Copyright (C) 2007 AvantLogic Corporation
var webSiteUrl;

function maButtonIsDisabled(button)
{
	return button.className == "buttonDisabled";
}

function maDropCookie(name, value, days)
{
	var date = new Date();
	date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
	var cookie = name + "=" + value + ";expires=" + date.toGMTString() + ";path=/";
	document.cookie = cookie;
}

function maOnKeyPress(e) 
{ 
	var key = window.event ? window.event.keyCode : e.which;
	var enterPressed = key == 13;
	if (enterPressed)
	{
		// Disable the Enter key so that it won't cause the page to post.
		// A page can allow Enter by simply declaring a var named allowEnter;
		if (typeof allowEnterKeyToPost  != "undefined")
			return true;
		else
			return false;
	}
	return true;
}

function maOnRollover(over, button, name)
{
	if (maButtonIsDisabled(button))
		return;
	var state = over ? 2 : 1;
	button.src = '../Images/' + name + state + ".gif";
}

function maPt(e)
{
	var left = 0;
	var top = 0;
	while (e) {
		left += e.offsetLeft;
		top += e.offsetTop;
		e = e.offsetParent;
	}
	var pt = new Object();
	pt.x = left;
	pt.y = top;
	return pt;
}

function maQuickHelpShow(p, content, x, y, info)
{
	var id = info ? 'QuickInfo' : 'QuickHelp';
	var e = document.getElementById(id);
	if (!e)
		return;
	if (x === 0)
		x = 10;
	if (y === 0)
		y = 20;
	e.style.visibility = 'visible';
	var pt = maPt(p);
	var width = info ? 296 : 250;
	var left = pt.x + x;
	var top = pt.y + y;
	if (left > 790)
	{
		left = left - width - 28;
		if (info)
			top += 20;
	}
	e.style.left = left + 'px';
	e.style.top = top + 'px';
	e.style.width = width + 'px';
	e.innerHTML = content;
}

function maQuickHelpHide(info)
{
	var id = info ? 'QuickInfo' : 'QuickHelp';
	var e = document.getElementById(id);
	if (e)
	    e.style.visibility = 'hidden';
}

function maQuickPreviewHide()
{
	var e = document.getElementById('QuickPreview');
	if (e)
	    e.style.visibility = 'hidden';
}

function maQuickImageShow(e, src, x, y, w, h)
{
	maQuickPreviewShow(e, '<img src="' + src + '" />', x, y, w, h);
}

function maQuickIframeShow(e, src, x, y, w, h)
{
	maQuickPreviewShow(e, '<iframe src="' + src + '" frameborder="0" scrolling="no" width="' + w + 'px" height="' + h + 'px" />', x, y, w, h);
}

function maQuickPreviewShow(e, content, x, y, w, h)
{
	if (x === 0)
		x = 20;
	if (y === 0)
		y = 16;
	var ePopup = document.getElementById('QuickPreview');
	if (ePopup)
	{
		ePopup.style.visibility = 'visible';
		var pt = maPt(e);
		ePopup.style.left=(pt.x + x) + 'px';
		ePopup.style.top=(pt.y + y) + 'px';
		if (w !== 0)
		{
			ePopup.style.width=w + 'px';
			ePopup.style.height=h + 'px';
		}
		ePopup.innerHTML= content;
	}
	return false;
}

function maReadyMapsCollapseSiblings(node)
{
	var parent = node.get_parent();            
	var siblings = parent.get_nodes();
	var siblingsCount = siblings.get_count();

	for (var nodeIndex = 0; nodeIndex < siblingsCount; nodeIndex++)
	{
		var siblingNode = siblings.getNode(nodeIndex);

		if (siblingNode != node && siblingNode.get_expanded())
		{
			siblingNode.collapse();
			return;
		}
	}
}

function maReadyMapsNodeTitle(node)
{
	return node.get_parent().get_text() + " " + node.get_text();
}

function maReadyMapsOnNodeExpanding(sender, args)
{
	var node = args.get_node();
	if (node.get_nodes().get_count())
	{
		maReadyMapsCollapseSiblings(node);
	}
}

function maReadyMapsOnMouseOver(sender, args)
{
	var node = args.get_node();
	var e = node.get_textElement(); 
	var category = node.get_category();
	
	var filePath = node.get_value();
	
	if (category == "map" && filePath !== null)
	{
		// Change the file name from foo.swf to foo_.jpg.
		filePath = filePath.substr(0, filePath.length - 4) + "_.jpg";
		maQuickImageShow(e, 'Thumbnail.ashx?type=file&amp;dim=125&amp;file=' + filePath, 24, 24, 0, 0);
	}
	else
	{
		var description = node.get_attributes().getAttribute("Description");
		if (description)
		{
			maQuickHelpShow(e, description, 24, 24, 0);
		}
	}
}

function maReadyMapsOnMouseOut(sender, args)
{
	maQuickPreviewHide();
	maQuickHelpHide();
}

function maToggleFaq(question, id)
{
	var s = document.getElementById(id).style;
	var show = s.display == "none";
	s.display = show ? "block" : "none";
	question.className = show ? "faqQuestionExpanded" : "faqQuestion";
}

function maToggleFaqSeeAlso(question, id)
{
	var s = document.getElementById(id).style;
	var show = s.display == "none";
	s.display = show ? "block" : "none";
	question.className = show ? "faqQuestionExpandedSeeAlso" : "faqQuestionSeeAlso";
}

function maTransferToMenuItem(item)
{
	maTransferToPage(item.Value);
}

function maTransferToPage(url)
{
	if (typeof url == "undefined")
		return;
	window.location = webSiteUrl + url.substr(1);
}

function maWindow(u)
{
    var url = webSiteUrl + u.substr(1);
    maWindowSized(url, 550, 350, "wnd");
}

function maVideoWindow(u)
{
    var url = webSiteUrl + u.substr(1);
    maWindowSized(url, 950, 700, "wnd");
}

function maWindowSized(url, w, h, n)
{
	var width = w;
	var height = h;
	var left = parseInt((screen.availWidth / 2) - (width / 2),10);
	var top = parseInt((screen.availHeight / 2) - (height / 2),10);
	var childWindow = window.open(url, n,
		"resizable" +
		",scrollbars" +
		",height=" + height +
		",width=" + width +
		",top=" + top +
		",left=" + left +
		",screenX=" + left +
		",screenY=" + top);
	if (childWindow === null)
		alert("A popup blocker prevented the requested window from opening.");
	else
		childWindow.focus();
}
