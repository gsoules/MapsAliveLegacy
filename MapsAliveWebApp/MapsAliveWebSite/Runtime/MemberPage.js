// Copyright (C) 2006-2021 AvantLogic Corporation

var actionId;
var actionIdEditView;
var contentChanged = false;
var contentChangedForPreview = false;
var formId;
var formContentId;
var deniedRequest;
var inLoad = true;
var inPostBack = false;
var inTransfer = false;
var masterBodyId;
var monitoredText;
var monitoredTextForPreview;
var monitoredTextBox;
var monitoredTextBoxForPreview;
var originalStatusMsg;
var originalStatusClass;
var panelState;
var saveButtonSrc;
var telerikComboBoxObserverList = null;
var tourViewIdToLocate;
var showingResourceGridView = true;
let statusBox = null;
let showingDialog = false;

window.onload = function()
{
	maOnLoad();
	vex.defaultOptions.overlayClosesOnClick = false;
	statusBox = document.getElementById('StatusBox')
	if (statusBox)
		statusBox.addEventListener('mousedown', maStatusBoxMouseDown)
};

function maStatusBoxDrag(e)
{
	let style = window.getComputedStyle(statusBox)
	statusBox.style.left = parseInt(style.left) + e.movementX + 'px'
	let top = parseInt(style.top) + e.movementY;
	if (top < 0)
		top = 0;
	statusBox.style.top = top + 'px'
}

function maStatusBoxMouseUp()
{
	document.removeEventListener('mousemove', maStatusBoxDrag)
	document.removeEventListener('mouseup', maStatusBoxMouseUp)
}

function maStatusBoxMouseDown()
{
	document.addEventListener('mousemove', maStatusBoxDrag)
	document.addEventListener('mouseup', maStatusBoxMouseUp)
}

function maHighlightTourNavigator(highlight)
{
	let panel = document.getElementsByClassName('tourNavigatorPanel');
	if (panel.length)
	{
		let e = panel[0];
		if (highlight)
			e.classList.add('highlight');
		else
			e.classList.remove('highlight');
	}
}

function maContinueButtonClicked(buttonElement, messageElementId)
{
	buttonElement.style.display = 'none';
	document.getElementById(messageElementId).style.display = 'block';
}

function maChangeDetected()
{
	if (contentChanged)
		return;
	contentChanged = true;
	
	var saveButton = document.getElementById("SaveButton");
	if (saveButton)
	{
		saveButton.className = "enabled";
		saveButton.src = saveButtonSrc;
	}
	var undoButton = document.getElementById("UndoButton");
	if (undoButton)
	{
		undoButton.className = "enabled";
		undoButton.src = undoButtonSrc;
	}

	if (typeof maChangeDetectedHelper !== "undefined")
		maChangeDetectedHelper();
}

function maChangeDetectedForPreview()
{
	if (contentChangedForPreview)
		return;
	contentChangedForPreview = true;
	var previewMessage = document.getElementById("PreviewMessage");
	if (previewMessage)
	{
		previewMessage.className = "previewMessage";
		previewMessage.innerHTML = "Click Save to update preview";
	}
	maChangeDetected();
}

function sleep(ms)
{
	return new Promise(resolve => setTimeout(resolve, ms));
}

function maFormatDialogMsg(text)
{
	// Convert format characters to HTML.
	let s = text;

	// Replace [@ ... @] with <span class='confirmTarget'> ... </span>
	s = s.replace(/\[@/g, `<span class='confirmTarget'>`);
	s = s.replace(/@]/g, '</span>');

	// Replace [* ... *] with <div class='confirmWarning'> ... </div>
	s = s.replace(/\[\*/g, `<div class='confirmWarning'>`);
	s = s.replace(/\*]/g, '</div>');

	// Replace [ ... ] with <p> ... </p>
	s = s.replace(/\[/g, '<p>');
	s = s.replace(/]/g, '</p>');
	return s;
}

function maAlert(msg, buttonText = 'OK')
{
	vex.dialog.buttons.YES.text = buttonText;
	vex.dialog.alert({ unsafeMessage: maFormatDialogMsg(msg) });
}

async function maAwaitConfirm(msg, buttonText = 'OK', escCloses = true)
{
	// IMPORTANT: Callers of this method must be aync methods and they must use await.
	// For example: await maAwaitConfirm('message goes here').

	// This method replaces the Tour Builder's use of the windows confirm dialog which cannot
	// be customized and looks and behaves differently on different browsers. This method does
	// not however work in the few places where a Telerik control asks for user confirmation.
	// Two places are 1) the warning when switching between Photo and Multimedia on the Edit
	// Hotspot Content page and 2) the warning when choosing "Delete Unused <resource name>"
	// from the Library menu. A third place is the Telerik tree control used for choosing a
	// Ready Map. For some reason, when this method is provided as the callback to be called
	// when an event has occured (e.g. Ready Map selected) the Telerik controls do not respond
	// to the asyncronous waiting achieved here like they do with the windows confirm function's
	// blocking wait. The Telerik controls post to the server before the user has responded to
	// this confirm. For these reasons, the Telerik controls do not callback to maAwaitConfirm.

	// Initialize the wait flag to zero to mean that the user has not made a choice yet.
	// When they choose OK, set the flag to 1. When they choose Cancel, set the flag to -1.
	let choice = 0;

	showingDialog = true;

	// Set the OK button text. The cancel button is always CANCEL.
	vex.dialog.buttons.YES.text = buttonText;

	// Specify whether the Esc key closes the dialog. Normally it should
	// unless use of the Esc key is what caused the dialog to be displayed.
	vex.defaultOptions.escapeButtonCloses = escCloses;

	// Display the confirm dialog. The Vex dialog will call the callback function when the user
	// clicks one of the two buttons. The clickedOk parameter returns true if they clicked OK
	// and false if they clicked Cancel. The call to display the dialog is asynchronous and
	// returns immediately. Control then falls through to the wait-loop below.
	vex.dialog.confirm({
		unsafeMessage: maFormatDialogMsg(msg),
		callback: function (clickedOk)
		{
			choice = clickedOk ? 1 : -1;
		}
	});

	// The dialog is being displayed. Wait until the user has clicked one of the buttons.
	// The loop will execute and prevent this method from returning until a choice is made.
	// As such, this method behaves like a synchronous function, even though it is async.
	while (choice === 0)
	{
		console.log('Sleeping...');
		await sleep(500);
	}

	vex.defaultOptions.escapeButtonCloses = true;

	// The user clicked a button. Return true for OK and false for Cancel.
	console.log(`maAwaitConfirm DONE ${choice}`);
	showingDialog = false;
	return choice === 1;
}

async function maAwaitDialog(msg, formHtml, buttonText = 'OK')
{
	// See comments for maAwaitConfirm.

	let choice = 0;
	let formData = null;

	showingDialog = true;

	vex.dialog.buttons.YES.text = buttonText;

	vex.dialog.open({
		unsafeMessage: maFormatDialogMsg(msg),
		input: formHtml,
		callback: function (data)
		{
			choice = data ? 1 : -1;
			formData = data;
		}
	});

	setTimeout(maSetVexDialogFocus, 0);

	while (choice === 0)
	{
		console.log('Sleeping...');
		await sleep(500);
	}

	console.log(`maAwaitDialog DONE ${choice}`);
	showingDialog = false;
	return formData;
}

function maSetVexDialogFocus()
{
	// Set the dialog's focus to be the "OK" button. This is done for two reasons.
	// The first is to remove the focus from the first element in the dialog so that it is not drawn as focused.
	// In the case of a radio button, a dark outline is draw around the normal outline of the first button. The
	// second is to cause the primary button's pulsating animation to work like on Vex alert and confirm dialogs.
	let elements = document.getElementsByClassName("vex-dialog-button-primary");
	if (elements.length > 0)
		elements[0].focus();
}

function maDoPostBack(target, arg)
{
	if (maDenyRequest("Please wait...", target + ":" + arg, false))
		return;

	inPostBack = true;
	__doPostBack(target, arg);
}

async function maConfirmAndPostBack(msg, eventName, buttonText = 'OK')
{
	// This method displays the confirm dialog and does not return until the user has clicked OK,
	// or Cancel, at which point, it does the post back if they clicked OK.
	let okay = await maAwaitConfirm(msg, buttonText);
	if (okay)
		maDoPostBack(eventName, '');
}

function maConfirmAndExecuteScript(msg, script, buttonText = 'OK')
{
	//console.log(`maConfirmAndExecuteScript "${msg}" : ${script}`);

	// This method displays the confirm dialog and returns immediately without waiting for
	// the user to click OK or Cancel. If and when the user clicks OK, it executes the script.
	vex.dialog.buttons.YES.text = buttonText;
	vex.dialog.confirm({
		unsafeMessage: maFormatDialogMsg(msg),
		callback: function (clickedOk)
		{
			if (clickedOk)
				window.Function(script)();
		}
	});
}

function maRunStalePublishedTour(msg, url)
{
	maConfirmAndExecuteScript(msg, `window.open("${url}", "_blank");`);
}

function maDenyRequest(msg, request, okWhenLoading)
{
	if (inTransfer || inPostBack || (inLoad && !okWhenLoading))
	{
		deniedRequest = request;
		msg = "";
		if (inTransfer)
			msg += "Transferring...";
		else if (inPostBack)
			msg += "Please wait...";
		else if (inLoad)
			msg += "Loading...";
		else
			msg += "...";
		return maShowBusyMsg(msg, "memberPageControlsStatusMessageRed", true);
	}
	else
	{
		if (request.indexOf("TourPreview.aspx") != -1)
			maShowBuildingPreviewMsg();
		else
			maShowBusyMsg(msg, "memberPageControlsStatusMessage", false);
		return false;
	}
}

function maShowMarkerThumbAsUnselected(viewId)
{
	//console.log(`maShowMarkerThumbAsUnselected ${viewId}`);

	if (maUndefined(viewId) || viewId === 0)
		return;
		
	var oldThumb = document.getElementById('thumb' + viewId);
	if (oldThumb)
		oldThumb.className = "markerThumbUnselected";
	
	var oldName = document.getElementById('name' + viewId);
	if (oldName)
		oldName.className = "markerThumbNameUnselected";
}

function maTourResourceSelectionChanged(comboBoxId, editControlId, editPage)
{
	maChangeDetected();
	var combo = $find(comboBoxId + "_TourResourceComboBox");
	var id = combo.get_value();
	var text = combo.get_text();
	var script = "maOnEventSaveAndTransfer('/Members/" + editPage + "?id=" + id + "');";
	var className = "pageActionControl";
	var classNameOver = "pageActionControlOver";
	var control = document.getElementById(editControlId).firstChild;
	control.className = className;
	control.onmouseover = new Function("this.className='" + classNameOver + "'");
	control.onmouseout = new Function("this.className='" + className + "';");
	control.onclick = new Function(script);
}

function maGetElementByPageId(name)
{
	return document.getElementById(formContentId + name);
}

function maHandleActionExplicit(aid, targetPage)
{
	actionId = aid;
	maHandleAction(targetPage);
}

function maHandleAction(targetPage)
{
	if (targetPage.substr(0,19) == "TourPageEditor.ashx")
	{
		if (!actionIdIsMapAction)
			actionId = actionIdLastPageAction;
		targetPage += "&aid=" + actionId;
	}
	else if (targetPage.substr(0,19) == "TourViewEditor.ashx")
	{
		if (!actionIdIsViewAction)
			actionId = actionIdLastViewAction;
		targetPage += "&aid=" + actionId;
	}
	maOnEventSaveAndTransfer("/Members/" + targetPage);
}

// Some callers of these methods (e.g. PageThumbs.cs) construct the names dynamically, so 
// searching for these names won't find all callers. In other words, don't delete these.
function maHandleDeleteMap(pageId, name)
{
	maHandleDelete("Map", pageId, `[@${name}@] map and all of its hotspots`, "map");
}

function maHandleDeleteGallery(pageId, name)
{
	maHandleDelete("Gallery", pageId, `[@${name}@] gallery and all of its photos`, "gallery");
}

function maHandleDeleteDataSheet(pageId, name)
{
	maHandleDelete("DataSheet", pageId, `[@${name}@] data sheet`, "data sheet");
}

function maHandleDeleteTour(tourId, name)
{
	maHandleDelete("Tour", tourId, `[@${name}@] tour [@#${tourId}@] and all of its maps and hotspots`, "tour");
}

function maHandleDelete(kind, id, targetName, description)
{
	var msg1 = targetName + " will be permanently deleted.<div class='confirmFirst'>You will be asked one more time to confirm.</div>";
	var msg2 = "Are you sure you want to delete " + targetName + "?</span>[*<span class='confirmSevereWarning'>WARNING:</span> Deleting this " + description + " cannot be undone.*]";

	vex.dialog.buttons.YES.text = 'CONFIRM';
	vex.dialog.confirm({
		unsafeMessage: maFormatDialogMsg(msg1),
		callback: function (clickedOk)
		{
			if (clickedOk)
			{
				vex.dialog.buttons.YES.text = 'DELETE';
				vex.dialog.confirm({
					unsafeMessage: maFormatDialogMsg(msg2),
					callback: function (clickedOkAgain)
					{
						if (clickedOkAgain)
							maDoPostBack("EventDelete", "Delete" + kind + "," + id);
					}
				});
			}
		}
	});
}

function maHandleDeleteEvent(eventName, targetType, targetName)
{
	let kind = targetType === undefined ? "it" : "this " + targetType;
	var msg = "[[@" + targetName + "@] will be permanently deleted.]<div class='confirmAreYouSure'>Are you sure you want to delete " + kind + "?</div>[*This action cannot be undone.*].";
	maConfirmAndExecuteScript(msg, `maDoPostBack("EventDelete", "${eventName}");`, 'DELETE');
}

function maIsDeleteEvent(eventName)
{
	return (
		eventName == "DeleteTour" ||
		eventName == "DeleteDataSheet" ||
		eventName == "DeleteGallery" ||
		eventName == "DeleteMap" ||
		eventName == "DeleteHotspot" ||
		eventName == "DeleteCategory" ||
		eventName == "DeleteMarker" ||
		eventName == "DeleteFontStyle" ||
		eventName == "DeleteMarkerStyle" ||
		eventName == "DeleteTooltipStyle" ||
		eventName == "DeleteTourStyle" ||
		eventName == "DeleteSymbol");
}

function maHandleExportEvent(actionId)
{
	window.location='PerformAction.ashx?aid=' + actionId;
}

function maIsNumericKey(e) 
{ 
	var key = window.event ? window.event.keyCode : e.which;
	if (window.event && window.event.cancelBubble !== null)
		window.event.cancelBubble = true;
	else 
		e.stopPropagation();

	return key >= 48 && key <= 57;
}

function maOnCheckListCheckAll(all, items)
{
	for (var i = 0; i < items.length; i++)
	{
		var checkbox = items[i];
		if (checkbox.type != "checkbox")
			continue;
		checkbox.checked = all;
	}
}

function maOnCheckListGetCheckedItems(items)
{
	var list = "";
	for (var i = 0; i < items.length; i++)
	{
		var checkbox = items[i];
		if (checkbox.type != "checkbox")
			continue;
		if (!checkbox.checked)
			continue;
		if (list.length > 0)
			list += ",";
		list += checkbox.value;
	}
	return list;
}

function maOnClickTourNavigator(value)
{
	var type = value.substr(0,1);
	var id = value.substr(1);
	
	if (type == "t")
		targetPage = "TourManager.aspx";
	else if (type == "p")
		targetPage = "TourPageEditor.ashx?pid=" + id;
	else if (type == "s")
		targetPage = "TourViewEditor.ashx?vid=" + id;
	else
	{
		maAlert('bad target');
		return; 
	}

	maHandleAction(targetPage);
}

function maOnEventBuild(pages, tourId, pageNumber)
{
	if (pages === 0)
	{
		maAlert("This tour has no pages yet.\n\nA tour must have at least one page before you can build it.");
		return;
	}
	var url = "/Members/TourPreview.aspx?build=1&aid=" + actionId + "&page=" + pageNumber;
	if (contentChanged)
		maDoPostBack("EventSaveAndBuild", url);
	else
		maSafeTransfer(url);
}

function maOnEventTabSelected(sender, args)
{
	var tab = args.get_tab();
	var id = tab.get_value();
	var targetPage = tab.get_attributes().getAttribute("TargetPage");
	maOnEventSaveAndTransfer('/Members/' + targetPage);
}

function maConfirmDeleteUnusedResource(msg, targetPage)
{
	maConfirmAndExecuteScript(msg, `maHandleAction('${targetPage}');`, 'DELETE');
}

function maOnEventMenuItemClicking(sender, args)
{
	var item = args.get_item();
	var id = item.get_value();
	var attributes = item.get_attributes();
	var confirmMsg = attributes.getAttribute("ConfirmMsg");
	var alertMsg = attributes.getAttribute("AlertMsg");
	var cancel = false;

	if (id === "_DeleteUnused")
	{
		// Make the user confirm that they want to delete an unused resource like font styles.
		// Do this by cancelling the menu click so that the page won't post. Then set a short
		// timer to display the the confirm dialog, which if the user clicks OK, will delete
		// the resource. The timer is necessary so that this method can return to its caller
		// before the confirm dialog gets displayed.
		args.set_cancel(true);
		var targetPage = attributes.getAttribute("TargetPage");
		setTimeout(maConfirmDeleteUnusedResource, 10, confirmMsg, targetPage);
		return;
	}

	if (confirmMsg != undefined && confirmMsg.length > 0)
	{ 
		var targetPage = attributes.getAttribute("TargetPage");
		if (!confirm(confirmMsg))
			cancel = true;
	}
	else if (alertMsg != undefined && alertMsg.length > 0)
	{
		maAlert(alertMsg);
		cancel = true;
	}
	
	if (cancel)
		args.set_cancel(true);
}

function maOnEventMenuItemClicked(sender, args)
{
	var item = args.get_item();
	var id = item.get_value();
	var attributes = item.get_attributes();
	
	if (maIsDeleteEvent(id))
	{
		var targetType = attributes.getAttribute("TargetType");
		var targetName = attributes.getAttribute("TargetName");
		var targetId = attributes.getAttribute("TargetId");
		
		if (id == "DeleteMap")
			maHandleDeleteMap(targetId, targetName);
		else if (id == "DeleteGallery")
			maHandleDeleteGallery(targetId, targetName);
		else if (id == "DeleteDataSheet")
			maHandleDeleteDataSheet(targetId, targetName);
		else if (id == "DeleteTour")
			maHandleDeleteTour(targetId, targetName);
		else
			maHandleDeleteEvent(id, targetType, targetName);
			
		return;
	}
	else if (id == "Save")
	{
		maOnEventSave();
		return;
	}
	else if (id == "Undo")
	{
		maOnEventUndo();
		return;
	}
	
	var targetPage = attributes.getAttribute("TargetPage");
	if (targetPage == undefined || targetPage.length === 0)
		return;

	// Collapse the menu dropdown so that it's not visible while the page loads.
	item.close();
	
	maHandleAction(targetPage);
}

function maConfirmChangePageMode(msg, newValue)
{
	maConfirmAndExecuteScript(msg, `maDoPostBack("EventChangePageMode", '${newValue}');`, 'SWITCH');
}

function maOnEventChangePageMode(sender, eventArgs)
{
	var oldValue = sender.get_selectedItem().get_value();
	var oldText = sender.get_selectedItem().get_text();
	var newValue = eventArgs.get_item().get_value();
	var newText = eventArgs.get_item().get_text();
	
	var newValueText;
	if (oldValue == "0")
		newValueText = "photo";
	else if (oldValue == "1")
		newValueText = "multimedia HTML";
	var msg = `[You are about to switch from [@${oldText}@] to [@${newText}@].][*This hotspot's ${newValueText} will be removed.*]`;

	eventArgs.set_cancel(true);
	setTimeout(maConfirmChangePageMode, 10, msg, newValue);
}

function maOnEventImportSlides(selector)
{
	maSafeTransfer("/Members/ImportSlides.aspx?id=" + selector.value);
}

function maOnEventRebuild(tourId, pageNumber, renumber)
{
	var renumberArg = renumber === 1 ? "&renumber=1" : "";
	var url = "/Members/TourPreview.aspx?rebuild=1&aid=" + actionId + "&page=" + pageNumber + renumberArg;
	if (contentChanged)
		maDoPostBack("EventSaveAndBuild", url);
	else
		maSafeTransfer(url);
}

function maOnEventSave()
{
	maDoPostBack("EventSave", "");
}

function maOnEventSaveButton(button)
{
	if (maButtonIsDisabled(button))
		return;
	maOnEventSave();
}

function maOnEventSaveAndTransfer(url)
{
	url = url.replace(/&amp;/, "&");
	url = maAttachPostId(url);
	
	if (contentChanged)
		maDoPostBack("EventSaveAndTransfer", url);
	else
		maSafeTransfer(url);
}

function maAttachPostId(url)
{
	var actions = new Array("AddTourView", "CreateMarker");
	if (maStringContains(url, actions))
	{
		var postId = document.getElementById(formId + "PostId");
		var delimeter = url.indexOf('?') == -1 ? '?' : '&';
		url += delimeter + "post=" + postId.value;
	}
	return url;
}

function maStringContains(s, list)
{
	for (var name in list)
	{
		if (s.indexOf(list[name]) != -1)
			return true;
	}
	return false;
}

function maOnEventUndo()
{
	maDoPostBack("EventUndo", "");
}

function maOnEventUndoButton(button)
{
	if (maButtonIsDisabled(button))
		return;
	maOnEventUndo();
}

function maOnEventUploadFile()
{
	maDoPostBack("EventUploadFile", contentChanged ? 1 : 0);
}

function maOnEventRemoteImport(url)
{
	maDoPostBack("EventRemoteImport", url);
}

function maOnEventRemoveBannerImage()
{
	var msg = "[Remove the banner for this tour?][Note that you don't have to remove the old banner to choose a new one. Just browse for another image and then press Load.]";
	maConfirmAndExecuteScript(msg, `maDoPostBack("EventRemoveImage", contentChanged ? 1 : 0);`, "REMOVE");
		
}

function maOnEventRemoveSlideImage()
{
	var msg = "[Remove the photo for this hotspot?][Note that you don't have to remove the old photo to choose a new one. Just browse for another image and then press Load.]";
	maConfirmAndExecuteScript(msg, `maDoPostBack("EventRemoveImage", contentChanged ? 1 : 0);`, "REMOVE");
}

function maOnEventRemoveMapImage()
{
	var msg = "[Remove the map image for this page?][Note that you don't have to remove the old map to choose a new one. Just browse for another image and then press Load.]";
	maConfirmAndExecuteScript(msg, `maDoPostBack("EventRemoveImage", contentChanged ? 1 : 0);`, "REMOVE");
}

function maOnEventUploadSampleImage(id)
{
	maDoPostBack("EventUploadSampleImage", id);
}

function maOnLoad()
{
	window.addEventListener('resize', maOnResize);
	maOnResize();

	inLoad = false;
	if (!maUndefined(originalStatusMsg))
		maShowBusyMsg(originalStatusMsg, originalStatusClass, false);
}

function maOnResize()
{
	let headerControlsElement = document.getElementById('MemberPageControlsHeaderRight');
	let pageWidth = Math.max(965, document.body.clientWidth);
	let controlsWidth = headerControlsElement.offsetWidth;
	let offset = pageWidth - controlsWidth;
	headerControlsElement.style.marginLeft = `${offset}px`;
	headerControlsElement.style.visibility = 'visible';

	// Create a new set of combo box observers on every resize so that the dropdown list
	// will shift left or right as the window gets wider or narrower.
	maCreateTelerikComboBoxObservers();

	// When the page is showing step-by-step instructions, position the help
	// panel so that its right edge sticks to the right edge of the page.
	let statusBox = document.getElementById('StatusBox');
	if (statusBox)
		statusBox.style.left = pageWidth - 540 + 'px';
}

function maSafeTransfer(url)
{
	if (maDenyRequest("Transferring...", url, true))
		return;
	inTransfer = true;
	maTransferToPage(url);
}

function maShowBusyMsg(msg, msgClass, pleaseWait)
{
	var e = document.getElementById("MemberPageStatusMsg");		
	if (e)
	{
		if (pleaseWait && maUndefined(originalStatusMsg))
		{
			originalStatusMsg = e.innerHTML;
			originalStatusClass = e.className;
			e.onclick = maShowDeniedRequest;
		}
		e.className = msgClass;
		e.innerHTML = msg;
		return true;
	}
	return false;
}

function maShowBuildingPreviewMsg()
{
	var e = document.getElementById("MemberPageStatusMsg");		
	e.className = "memberPageControlsStatusMessage";
	e.innerHTML = "Building Tour Preview...";
}

function maShowDeniedRequest()
{
	maAlert(deniedRequest);
}

function maShowElement(show, id)
{
	e = document.getElementById(id);
	if (e)
		e.style.visibility = show ? 'visible' : 'hidden';
}

function maShowStillLoadingMsg(busy, msg)
{
	return maShowBusyMsg(msg, "memberPageControlsStatusMessage", true);
}

function maShowSliderElement(name, show)
{
	maGetElementByPageId(name).style.display = show ? "block" : "none";
}

function maShowStatusBox(show)
{
	maShowElement(show, "StatusBox");
	maShowElement(show, "StatusBoxTopArrow");
	maShowElement(show, "StatusBoxArrowUp");
	maShowElement(show, "StatusBoxArrowLineV");
	maShowElement(show, "StatusBoxArrowElbow");
	maShowElement(show, "StatusBoxArrowLineH");
	maShowElement(show, "StatusBoxArrowEnd");
	maShowElement(show, "maSampleImage");
}

function maShowThumbControls(show, markerId)
{
	var controls = document.getElementById('controls' + markerId);
	controls.className = show ? "thumbControlsSelected" : "thumbControlsUnselected";
}

function maStartMonitoringTextBoxChanges(textBox)
{
	if (contentChanged)
		return;
	monitoredTextBox = textBox;
	monitoredText = monitoredTextBox.value;
}

function maStartMonitoringTextBoxChangesForPreview(textBox)
{
	if (contentChangedForPreview)
		return;
	monitoredTextBoxForPreview = textBox;
	monitoredTextForPreview = monitoredTextBoxForPreview.value;
}

function maStopEvent(e)
{
	e.cancelBubble = true;
	e.returnValue = false;
	if (e.stopPropagation)
	{
		e.stopPropagation();
		e.preventDefault();
	}
	return false;
}

function maTestForTextBoxChanges()
{
	if (contentChanged)
		return;
	if (monitoredText != monitoredTextBox.value)
		maChangeDetected();
}

function maTestForTextBoxChangesForPreview()
{
	if (contentChangedForPreview)
		return;
	if (monitoredTextForPreview != monitoredTextBoxForPreview.value)
		maChangeDetectedForPreview();
}

function maToggleReportDetail(detailId, iconId)
{
	var eDetail = document.getElementById(detailId);
	var eIcon = document.getElementById(iconId);
	var collapsed = eDetail.style.display == "none";
	if (collapsed)
	{
		eDetail.style.display = ""; // Use "" instead of "block" to make this work on a tbody tag in Firefox.
		eIcon.src = "../Images/ReportZoomOut.gif";
	}
	else
	{
		eDetail.style.display = "none";
		eIcon.src = "../Images/ReportZoomIn.gif";
	}
}

function maToggleGroupNavigator(page, id)
{
	var s = document.getElementById(id).style;
	var show = s.display == "none";
	s.display = show ? "block" : "none";
	page.className = show ? "groupNavExpanded" : "groupNavCollapsed";
}

function maTogglePanelBar(bar, id)
{
	var e = document.getElementById(id);
	if (!e)
		return;
	var s = e.style;
	if (!s)
		return;
	var show = s.display == "none";
	s.display = show ? "block" : "none";
	e = document.getElementById(bar);
	if (!e)
		return;
	e.className = show ? "panelBarExpanded" : "panelBarCollapsed";
}

function maToggleResourceView()
{
	let resources = document.getElementById("Resources");

	if (showingResourceGridView)
		resources.classList.replace("resourceGrid", "resourceDetails");
	else
		resources.classList.replace("resourceDetails", "resourceGrid");

	showingResourceGridView = !showingResourceGridView;

	let e = document.getElementById("ResourceViewToggle");
	e.innerHTML = showingResourceGridView ? "Show Details View" : "Show Grid View";
}

function maUndefined(object)
{
	return typeof object == "undefined";
}

function maUpdatePreview()
{
	var e = maGetElementByPageId('LayoutPreviewImage');
	if (e === null)
		return;
	
	maChangeDetected();
	maDoPostBack("EventChangePageMode", 0);
}

function maCreateTelerikComboBoxObservers()
{
	// This function provides a workaround for a bug in the telerik:RadComboBox control whereby the
	// control's dropdown list does not take into account the left offset of the containing page
	// within the browser window. As a result, the dropdown appears to the right of the combo box
	// itself by the amount that the container page is shifted to the left. The workaround uses
	// MutationObserver objects that watch for changes to the control's style and then change the
	// dropdown's left offset by the appropriate amount.

	// Create a list of observers, one for each Telerik ComboBox dropdown element.
	telerikComboBoxObserverList = [];

	// Get the dropdown element for each Telerik combo box (each has class 'rcbSlide'). These elements
	// reside at the top of the page separated from the combo box itself.
	let dropdownElements = document.getElementsByClassName('rcbSlide');

	// Create an observer for each dropdown element.
	for (const dropdownElement of dropdownElements)
	{
		// Add a flag to the element indicating that it should be adjusted the next time it is displayed.
		dropdownElement.maNeedsAdjustment = true;

		// Create a MutationObserver that will be called every time the element's style changes.
		let observer = new MutationObserver(function ()
		{
			// The logic that follows gets executed whenever the element's style changes. The style change
			// we are interested in is the one where when the user clicks on the combo box, the Telerik
			// logic changes the dropdown element's display style from 'none' to 'block' and vice versa.

			if (dropdownElement.style.display === 'none')
			{
				// The dropdown is hidden. Flag that it needs to be adjusted when it gets displayed.
				dropdownElement.maNeedsAdjustment = true;
			}
			else if (dropdownElement.maNeedsAdjustment)
			{
				// The dropdown is showing. Adjust it, but only once while it is being displayed.
				// This prevents this observer from getting called if other style changes occur, and
				// most importantly, keeps it from getting called in response to it changing the style.
				dropdownElement.maNeedsAdjustment = false;

				// Determine how far the page is shifted to the right due to being centered in the browser window.
				let adjustment = document.getElementById('PageContent').getBoundingClientRect().left;

				// Get the dropdown's style.left value, convert it to an integer (which strips of the
				// trailing 'px'), shift it left by the adjustment amount.
				let adjustedLeft = parseInt(dropdownElement.style.left, 10) - adjustment;
				
				// Update the style. Doing so will trigger a call to this observer, but this code won't
				// get executed in response beause the maNeedsAdjustment flag it false.
				dropdownElement.style.left = `${adjustedLeft}px`;
			}
		});

		// Add the observer to the list of observers for this page and tell it to start watching for style changes.
		telerikComboBoxObserverList.push(observer);
		observer.observe(dropdownElement, { attributeFilter: ["style"] });
	}
}
