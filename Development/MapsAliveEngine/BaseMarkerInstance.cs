// Copyright (C) 2006 AvantLogic Corporation
using System;
using System.Collections.Generic;
using System.Text;

namespace AvantLogic.MapsAlive.Engine
{
	public class BaseMarkerInstance
	{
		protected BaseMarkerRuleSet baseActionRuleSet;
		protected BaseMarkerRuleSet baseJavascriptRuleSet;
		protected BaseMarker baseMarker;
		protected int targetPageId;
		protected string targetPageName;
		protected int targetViewId;
		protected string tooltip;
		protected bool isBound;
		protected bool isDisabled;
		protected bool isHidden;
		protected bool isLocked;
		protected bool isShapeOnly;
		protected bool isNotAnchored;
		protected bool isStatic;
		protected bool isRoute;
		protected bool markerZooms;
		protected bool doesNotShowContent;

		public BaseMarkerInstance()
		{
		}

		public BaseMarkerInstance(BaseMarker baseMarker, BaseMarkerRuleSet baseActionRuleSet, BaseMarkerRuleSet baseJavascriptRuleSet)
		{
			this.baseMarker = baseMarker;
			this.baseActionRuleSet = baseActionRuleSet;
			this.baseJavascriptRuleSet = baseJavascriptRuleSet;
		}

		public BaseMarker BaseMarker
		{
			get { return baseMarker; }
		}

		public virtual BaseMarkerRuleSet BaseActionMarkerRuleSet
		{
			get
			{
				if (baseActionRuleSet != null)
					return baseActionRuleSet;
				else
					return BaseMarkerDefinitionRuleSet;
			}
		}

		public virtual BaseMarkerRuleSet BaseJavascriptMarkerRuleSet
		{
			get { return baseJavascriptRuleSet; }
		}
		
		public virtual BaseMarkerRuleSet BaseMarkerDefinitionRuleSet
		{
			get { return baseMarker.BaseMarkerDefinition.BaseMarkerRuleSet; }
		}

		public bool DoesNotShowContent
		{
			get { return doesNotShowContent; }
			set { doesNotShowContent = value; }
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

		public bool IsNotAnchored
		{
			get { return isNotAnchored; }
			set { isNotAnchored = value; }
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

		public virtual string MapTagId
		{
			get { return string.Empty; }
		}

		public bool MarkerZooms
		{
			get { return markerZooms; }
			set { markerZooms = value; }
		}

		public virtual int TargetPageId
		{
			get { return targetPageId; }
			set { targetPageId = value; }
		}

		public virtual string TargetPageName
		{
			get { return targetPageName; }
			set { targetPageName = value; }
		}

		public virtual int TargetViewId
		{
			get { return targetViewId; }
			set { targetViewId = value; }
		}

		public virtual void SetTooltip(int themeId, string text)
		{
			tooltip = text;
		}

		public virtual string TooltipForTheme(int themeId)
		{
			return tooltip;
		}
	}
}
