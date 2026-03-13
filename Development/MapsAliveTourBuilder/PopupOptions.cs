// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Drawing;
using System.Diagnostics;

// These values are known in the DB -- do not change.
public enum PopupArrowType
{
	None = 0,
	Small = 1,
	Large = 2,
	Callout50 = 50,
	Callout75 = 75,
	Callout100 = 100,
	Callout125 = 125,
	Callout150 = 150,
	Callout175 = 175,
	Callout200 = 200,
	Callout225 = 225,
	Callout250 = 250,
	Callout275 = 275,
	Callout300 = 300
}

// These values are known in the DB -- do not change.
public enum PopupDelayType
{
	None = 0,
	Before = 1,
	After = 2
}

// These values are known in the DB -- do not change.
public enum PopupLocation
{
	MarkerCenter = 0,
	MarkerEdgeInside = 2,
	MarkerEdgeOutside = 3,
	Mouse = 4,
	MouseFollow = 5,
	Fixed = 6,
	FixedAlwaysVisible = 7
}

public class PopupOptions
{
	private PopupArrowType arrowType;
	private int borderWidth;
	private string borderColor;
	private string backgroundColor;
	private int bestSideSequence;
	private int delay;
	private PopupDelayType delayType;
	private int dropShadowDistance;
	private int imageCornerRadius;
	private PopupLocation location;
	private Point locationPoint;
	private Size minSize;
	private int markerOffset;
	private bool pinOnClick;
	private string pinMessage;
	private int popupCornerRadius;
	private bool showTooltipWhenNoContent;
	private string textColor;
	private int textOnlyWidth;
	private string titleTextColor;
    private Tour tour;
	private bool useColorSchemeColors;

	public PopupOptions(
		Tour tour,
        int bestSideSequence,
		int borderWidth,
		int popupCornerRadius,
		int imageCornerRadius,
		int dropShadowDistance,
		string borderColor,
		string backgroundColor,
		string textColor,
		string titleTextColor,
		PopupLocation location,
		Point locationPoint,
		Size minSize,
		PopupArrowType arrowType,
		bool showTooltipWhenNoContent,
		PopupDelayType delayType,
		int delay,
		bool pinOnClick,
		string pinMessage,
		int textOnlyWidth,
		bool useColorSchemeColors,
		int markerOffset)
	{
        this.tour = tour;
        this.bestSideSequence = bestSideSequence;
		this.borderWidth = borderWidth;
		this.popupCornerRadius = popupCornerRadius;
		this.imageCornerRadius = imageCornerRadius;
		this.dropShadowDistance = dropShadowDistance;
		this.borderColor = borderColor;
		this.backgroundColor = backgroundColor;
		this.textColor = textColor;
		this.titleTextColor = titleTextColor;
		this.location = location;
		this.locationPoint = locationPoint;
		this.minSize = minSize;
		this.arrowType = arrowType;
		this.showTooltipWhenNoContent = showTooltipWhenNoContent;
		this.delayType = delayType;
		this.delay = delay;
		this.pinOnClick = pinOnClick;
		this.pinMessage = pinMessage;
		this.textOnlyWidth = textOnlyWidth;
		this.useColorSchemeColors = useColorSchemeColors;
		this.markerOffset = markerOffset;
	}

	public PopupArrowType ArrowType
	{
		get { return arrowType; }
		set { arrowType = value; }
	}

	public string BackgroundColor
	{
		get { return backgroundColor; }
		set { backgroundColor = value; }
	}

	public int BestSideSequence
	{
		get { return bestSideSequence; }
		set { bestSideSequence = value; }
	}

	public string BorderColor
	{
		get { return borderColor; }
		set { borderColor = value; }
	}

	public int BorderWidth
	{
		get { return borderWidth; }
		set { borderWidth = value; }
	}

	public int Delay
	{
		get { return delay; }
		set { delay = value; }
	}

	public PopupDelayType DelayType
	{
		get { return delayType; }
		set { delayType = value; }
	}

	public int DropShadowDistance
	{
		get { return dropShadowDistance; }
		set { dropShadowDistance = value; }
	}

	public int ImageCornerRadius
	{
		get { return imageCornerRadius; }
		set { imageCornerRadius = value; }
	}

	public PopupLocation Location
	{
		get 
        {
            if (tour.V4 && (location == PopupLocation.Fixed || location == PopupLocation.FixedAlwaysVisible))
                return PopupLocation.MarkerEdgeInside;
            else
                return location;
        }
		set { location = value; }
	}

	public bool LocationAllowsMouseOntoPopup
	{
		get
		{
			return
				location == PopupLocation.MarkerCenter ||
				location == PopupLocation.MarkerEdgeInside ||
				location == PopupLocation.Mouse ||
				LocationIsFixed;
		}
	}

	public bool LocationIsFixed
	{
		get
        {
            if (tour.V4)
                return false;
            else
                return location == PopupLocation.Fixed || location == PopupLocation.FixedAlwaysVisible;
        }
	}

	public Point LocationPoint
	{
		get { return locationPoint; }
		set { locationPoint = value; }
	}

	public int MarkerOffset
	{
		get { return markerOffset; }
		set { markerOffset = value; }
	}

	public Size MinSize
	{
		get { return minSize; }
		set { minSize = value; }
	}

	public bool PinOnClick
	{
		get { return pinOnClick; }
		set { pinOnClick = value; }
	}

	public string PinMessage
	{
		get
		{
			if (pinMessage == string.Empty)
				pinMessage = MapsAliveTourBuilder.Text.DefaultPinPopupMessage;
			return pinMessage; 
		}
		set { pinMessage = value; }
	}

	public int PopupCornerRadius
	{
		get { return popupCornerRadius; }
		set { popupCornerRadius = value; }
	}

	public bool ShowTooltipWhenNoContent
	{
		get { return showTooltipWhenNoContent; }
		set { showTooltipWhenNoContent = value; }
	}

	public string TextColor
	{
		get { return textColor; }
		set { textColor = value; }
	}

	public int TextOnlyWidth
	{
		get { return textOnlyWidth; }
		set { textOnlyWidth = value; }
	}

	public string TitleTextColor
	{
		get { return titleTextColor; }
		set { titleTextColor = value; }
	}

	public bool UseColorSchemeColors
	{
		get { return useColorSchemeColors; }
		set { useColorSchemeColors = value; }
	}

	public void SetGalleyDefaults()
	{
		SetReadyMapDefaults();
		MarkerOffset = 48;
	}

	public void SetReadyMapDefaults()
	{
		DelayType = PopupDelayType.Before;
		Delay = 100;
		Location = PopupLocation.MouseFollow;
		showTooltipWhenNoContent = true;
		MinSize = new Size(12, 12);
	}
}