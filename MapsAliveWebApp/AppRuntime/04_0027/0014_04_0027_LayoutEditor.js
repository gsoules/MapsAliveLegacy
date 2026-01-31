var minSlider = 36;
var sliderH;
var sliderV;
var sliderCoord;
var sliderCoordValue;
var sliderCoordMeaning;
var sliderControl;
var offsetSliderH;
var offsetSliderV;
var sliderPosition;
var splitterBar;
var viewSpacingH;
var viewSpacingV;
var viewWidth;
var viewHeight;
var viewMarginsV;
var viewMarginsH;

function initSlider()
{
	sliderH = maGetElementByPageId('SliderHorizontal');
    attachEventListener(sliderH, "mousedown", mousedownSliderH, false);
	sliderV = maGetElementByPageId('SliderVertical');
    attachEventListener(sliderV, "mousedown", mousedownSliderV, false);
    return true;
}

function mousedownSliderH(event)
{
	mousedownSlider(event, true);
}

function mousedownSliderV(event)
{
	mousedownSlider(event, false);
}

function mousedownSlider(event, horizontal)
{
	if (typeof event == "undefined")
	{
		event = window.event;
	}

	var target = getEventTarget(event);

	document.currentSlider = target;
	document.currentSliderIsHorizontal = horizontal;
	sliderPosition = -1;

	if (horizontal)
	{
		target.originPosition = event.clientX;
		attachEventListener(document, "mousemove", mousemoveSliderHorizontal, false);
	}
	else
	{
		target.originPosition = event.clientY;
		attachEventListener(document, "mousemove", mousemoveSliderVertical, false);
	}
	attachEventListener(document, "mouseup", mouseupSlider, false);

	stopDefaultAction(event);
	
	sliderCoord = document.getElementById("SliderCoord");
	sliderCoordValue = document.getElementById("SliderCoordValue");
	sliderCoordMeaning = document.getElementById("SliderCoordMeaning");
	sliderControl = maGetElementByPageId("SliderControl");
	sliderOffsetH = maGetElementByPageId("SliderHorizontalTrack").offsetLeft;
	sliderOffsetV = maGetElementByPageId("SliderVerticalTrack").offsetTop;
	splitterBar = document.getElementById("SplitterBar");

	return true;
}

function mousemoveSliderHorizontal(event)
{
	if (typeof event == "undefined")
		event = window.event;

	let slider = document.currentSlider;
	let currentPosition = slider.currentPosition;

	if (isNaN(currentPosition))
		currentPosition = parseInt(maGetElementByPageId('LeftWidth').value,10);

	currentPosition += event.clientX - slider.originPosition;

	splitterBar.style.width = "1px";
	splitterBar.style.height = fullHeight + "px";

	let left = sliderOffsetH + sliderH.offsetLeft;
	let top = sliderH.offsetTop;

	if (currentPosition < minSlider)
		currentPosition = minSlider;
	else if (currentPosition > (slider.parentNode.offsetWidth - slider.offsetWidth - minSlider))
		currentPosition = slider.parentNode.offsetWidth - slider.offsetWidth - minSlider;
	else
		slider.originPosition = event.clientX;

	sliderPosition = currentPosition;
	slider.style.left = sliderPosition + "px";

	let layoutInnerWidth = viewWidth - viewMarginsV;
	let halfSpacingV = viewSpacingV / 2;
	let splitterPositionV = invertV ? layoutInnerWidth - sliderPosition - halfSpacingV : sliderPosition + halfSpacingV;
	let percentWidth = splitterPositionV / (layoutInnerWidth - viewSpacingV);

	reportSliderPosition(percentWidth, meaningV, left + 16, top + 24, left + halfSpacingV, top + 18);

	sliderPosition = invertV ? sliderPosition - halfSpacingV : splitterPositionV;

	slider.currentPosition = currentPosition;
	stopDefaultAction(event);
	return true;
}

function mousemoveSliderVertical(event)
{
	if (typeof event == "undefined")
		event = window.event;

	let slider = document.currentSlider;
	let currentPosition = slider.currentPosition;

	if (isNaN(currentPosition))
		currentPosition = parseInt(maGetElementByPageId('TopHeight').value,10);

	currentPosition += event.clientY - slider.originPosition;
	
	splitterBar.style.width = fullWidth + "px";
	splitterBar.style.height = 1 + "px";

	let left = sliderV.offsetLeft;
	let top = sliderOffsetV + sliderV.offsetTop;

	if (currentPosition < minSlider)
		currentPosition = minSlider;
	else if (currentPosition > (slider.parentNode.offsetHeight - slider.offsetHeight - minSlider))
		currentPosition = slider.parentNode.offsetHeight - slider.offsetHeight - minSlider;
	else
		slider.originPosition = event.clientY;

	sliderPosition = currentPosition;
	slider.style.top = sliderPosition + "px";

	let layoutInnerHeight = viewHeight - viewMarginsH;
	let halfSpacingH = viewSpacingH / 2;
	let splitterPositionH = invertH ? layoutInnerHeight - sliderPosition - halfSpacingH : sliderPosition + halfSpacingH;
	let percentHeight = splitterPositionH / (layoutInnerHeight - viewSpacingH);

	reportSliderPosition(percentHeight, meaningH, left + 28, top - 34, left + 20, top + halfSpacingH);

	sliderPosition = invertH ? sliderPosition - halfSpacingH : splitterPositionH;

	slider.currentPosition = currentPosition;
	stopDefaultAction(event);
	return true;
}

function reportSliderPosition(percent, meaning, tooltipLeft, tooltipTop, splitterLeft, splitterTop, invert)
{
	sliderCoord.style.visibility = "visible";
	sliderCoord.style.left = tooltipLeft + "px";
	sliderCoord.style.top = (tooltipTop + sliderControl.offsetTop) + "px";
	sliderCoordMeaning.innerHTML = meaning + ': ';
	sliderCoordValue.innerHTML = (percent * 100).toFixed(1) + "%";
	
	splitterBar.style.visibility = "visible";
	splitterBar.style.opacity = 0.5;
	splitterBar.style.left = splitterLeft + "px";
	splitterBar.style.top = (splitterTop + sliderControl.offsetTop) + "px";
}

function mouseupSlider()
{
	if (document.currentSliderIsHorizontal)
	{
		detachEventListener(document, "mousemove", mousemoveSliderHorizontal, false);
		if (sliderPosition >= 0)
			maMoveSliderHorizontal(sliderPosition, true);
	}
	else
	{
		detachEventListener(document, "mousemove", mousemoveSliderVertical, false);
		if (sliderPosition >= 0)
			maMoveSliderVertical(sliderPosition, true);
	}
	
	detachEventListener(document, "mouseup", mouseupSlider, false);

	return true;
}

function attachEventListener(target, eventType, functionRef, capture)
{
  if (typeof target.addEventListener != "undefined")
  {
    target.addEventListener(eventType, functionRef, capture);
  }
  else if (typeof target.attachEvent != "undefined")
  {
    target.attachEvent("on" + eventType, functionRef);
  }
  else
  {
    eventType = "on" + eventType;

    if (typeof target[eventType] == "function")
    {
      var oldListener = target[eventType];

      target[eventType] = function()
      {
        oldListener();

        return functionRef();
      };
    }
    else
    {
      target[eventType] = functionRef;
    }
  }

  return true;
}

function detachEventListener(target, eventType, functionRef, capture)
{
  if (typeof target.removeEventListener != "undefined")
  {
    target.removeEventListener(eventType, functionRef, capture);
  }
  else if (typeof target.detachEvent != "undefined")
  {
    target.detachEvent("on" + eventType, functionRef);
  }
  else
  {
    target["on" + eventType] = null;
  }

  return true;
}

function getEventTarget(event)
{
  var targetElement = null;

  if (typeof event.target != "undefined")
  {
    targetElement = event.target;
  }
  else
  {
    targetElement = event.srcElement;
  }

  while (targetElement.nodeType == 3 && targetElement.parentNode !== null)
  {
    targetElement = targetElement.parentNode;
  }

  return targetElement;
}

function stopDefaultAction(event)
{
  event.returnValue = false;

  if (typeof event.preventDefault != "undefined")
  {
    event.preventDefault();
  }

  return true;
}

function maSetFlagMovedH(moved)
{
	var e = maGetElementByPageId('SplitterLockedCheckBoxH');
	if (e)
		e.checked = moved;
}

function maSetFlagMovedV(moved)
{
	var e = maGetElementByPageId('SplitterLockedCheckBoxV');
	if (e)
		e.checked = moved;
}

function maOnSlideLayoutClicked(warning, patternId)
{
	// Store the pattern Id for the selected thumbnail in the page's hidden PatternId field.
	let e = maGetElementByPageId("PatternId");
	e.value = patternId;

	// Post the page back to the server which can then retrieve the value of PatternId.
	var target = `/Members/TemplateChoices.aspx`;
	maChangeDetected();
	if (warning.length > 0)
	{
		if (confirm(warning))
			maOnEventSaveAndTransfer(target);
	}
	else
	{
		maOnEventSaveAndTransfer(target);
	}
}

function maMoveSliderHorizontal(distance, done)
{
	let leftWidth = maGetElementByPageId('LeftWidth');
	leftWidth.value = Math.round(distance);
	if (done)
	{
		maSetFlagMovedV(true);
		maUpdatePreview();
	}
}

function maMoveSliderVertical(distance, done)
{
	let topHeight = maGetElementByPageId('TopHeight');
	topHeight.value = Math.round(distance);
	if (done)
	{
		maSetFlagMovedH(true);
		maUpdatePreview();
	}
}

function maMoveSplitterPositionH(position)
{
	var topHeight = maGetElementByPageId('TopHeight');
	topHeight.value = Math.round(position);
	var sliderV = maGetElementByPageId('SliderVertical');
	sliderV.style.top = position + "px";
	sliderV.currentPosition = position;
}

function maMoveSplitterPositionV(position)
{
	var leftWidth = maGetElementByPageId('LeftWidth');
	leftWidth.value = Math.round(position);
	var sliderH = maGetElementByPageId('SliderHorizontal');
	sliderH.style.left = position + "px";
	sliderH.currentPosition = position;
}

function maNudgeSliderHorizontal(amount)
{
	let leftWidth = maGetElementByPageId('LeftWidth');
	let leftWidthValue = parseInt(leftWidth.value, 10);
	leftWidthValue += amount;
	if (leftWidthValue >= 16 && leftWidthValue <= (viewWidth - viewSpacingV - viewMarginsV - 16))
	{
		maMoveSplitterPositionV(leftWidthValue);
		maSetFlagMovedV(true);
		maUpdatePreview();
	}
}

function maNudgeSliderVertical(amount)
{
	var topHeight = maGetElementByPageId('TopHeight');
	var topHeightValue = parseInt(topHeight.value,10);
	topHeightValue += amount;
	if (topHeightValue >= 16 && topHeightValue <= (viewHeight - viewSpacingH - viewMarginsH - 16))
	{
		maMoveSplitterPositionH(topHeightValue);
		maSetFlagMovedH(true);
		maUpdatePreview();
	}
}
