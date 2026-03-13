// Copyright (C) 2007 AvantLogic Corporation
using System;
using System.Drawing;

// IMPORTANT: This struct must be kept in sync with AvantLogic.MapsAlive.Engine.BaseRuntimeOptions.cs

namespace AvantLogic.MapsAlive.Runtime
{
	public struct RTOptions
	{
		public int BuildId;
		public string HelpText;
		public string HelpTitle;
		public string HelpFont;
		public int HelpFontSize;
		public int HelpWidth;
		public int HelpColor;
		public int HelpBgColor;
		public Size MapAreaSize;
		public bool MapCanZoom;
		public int MapInsetColor;
		public int MapInsetSize;
		public int MapInsetLocation;
		public double MapAreaScale;
		public Size MapSize;
		public int MapX;
		public int MapY;
		public double MapZoom;
		public int MapZoomLimit;
		public int MarkerZoomLimit;
		public int MouseOverDelay;
		public int PanZoomControlColorOff;
		public int PanZoomControlColorOn;
		public bool SaveStateChanges;
		public int SelectedMarkerBlink;
		public bool ShowSlideShow;
		public bool ShowHelp;
		public bool ShowPanZoomControls;
		public int SlideShowInterval;
		public int VisitedMarkerAlpha;
	}
}
