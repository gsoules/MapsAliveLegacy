// Copyright (C) 2003-2007 AvantLogic Corporation
using System;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Diagnostics;

namespace AvantLogic.MapsAlive.Runtime
{
	public class RTMarkerInstance
	{
		private enum Flags
		{
			IsDisabled		= 0x00000001,
			IsHidden		= 0x00000002,
			IsStatic		= 0x00000004,
			IsRoute			= 0x00000008,
			IsLocked		= 0x00000010,
			MarkerZooms		= 0x00000020,
			IsShapeOnly		= 0x00000040,
			IsNotAnchored	= 0x00000080,
			IsBound			= 0x00000100,
		}

		protected SortedList rtEvents = new SortedList();
		protected int targetPageId;
		protected int targetViewId;
		protected string tooltip;
		protected bool isBound;
		protected bool isDisabled;
		protected bool isHidden;
		protected bool isLocked;
		protected bool isNotAnchored;
		protected bool isShapeOnly;
		protected bool isStatic;
		protected bool isRoute;
		protected bool markerZooms;
		protected bool doesNotShowContent;

		public RTMarkerInstance()
		{
		}

		public bool DoesNotShowContent
		{
			get { return doesNotShowContent; }
			set { doesNotShowContent = value; }
		}

		public int FlagBits
		{
			get
			{
				Flags bits = 0;
				
				if (isBound)
					bits |= Flags.IsBound;
				if (isDisabled)
					bits |= Flags.IsDisabled;
				if (isHidden)
					bits |= Flags.IsHidden;
				if (isRoute)
					bits |= Flags.IsRoute;
				if (isStatic)
					bits |= Flags.IsStatic;
				if (isShapeOnly)
					bits |= Flags.IsShapeOnly;
				if (isLocked)
					bits |= Flags.IsLocked;
				if (markerZooms)
					bits |= Flags.MarkerZooms;
				if (isNotAnchored)
					bits |= Flags.IsNotAnchored;
				
				return (int)bits;
			}
		}

		public bool IsBound
		{
			get { return isBound; }
			set { isBound = value; }
		}

		public bool IsDisabled
		{
			get { return isDisabled; }
			set { isDisabled = value; }
		}

		public bool IsNotAnchored
		{
			get { return isNotAnchored; }
			set { isNotAnchored = value; }
		}

		public bool IsHidden
		{
			get { return isHidden; }
			set { isHidden = value; }
		}

		public bool IsLocked
		{
			get { return isLocked; }
			set { isLocked = value; }
		}

		public bool IsRoute
		{
			get { return isRoute; }
			set { isRoute = value; }
		}

		public bool IsShapeOnly
		{
			get { return isShapeOnly; }
			set { isShapeOnly = value; }
		}

		public bool IsStatic
		{
			get { return isStatic; }
			set { isStatic = value; }
		}

		public bool MarkerZooms
		{
			get { return markerZooms; }
			set { markerZooms = value; }
		}

		public SortedList RTEvents 
		{
			get { return rtEvents; }
		}

		public int TargetPageId
		{
			get { return targetPageId; }
			set { targetPageId = value; }
		}		

		public int TargetViewId
		{
			get { return targetViewId; }
			set { targetViewId = value; }
		}

		public void SetTooltip(int themeId, string text)
		{
			tooltip = text;
		}

		public string Tooltip(int themeId)
		{
			return tooltip;
		}
	}
}
