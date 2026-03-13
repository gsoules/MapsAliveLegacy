// Copyright (C) 2006 AvantLogic Corporation
using System;
using System.Drawing;

namespace AvantLogic.MapsAlive.Engine
{
	// Do not change the numeric values of these enumerations -- they are known in the XSL and JavaScript.
	public enum MarkerEventType
	{
		None = 0,
		Click = 1,
		MouseEnter = 3,
		MouseExit = 4
	}

	public class BaseMarker
	{
		protected BaseMarkerDefinition baseMarkerDefinition;
		protected BaseMarkerInstance baseMarkerInstance;
		protected int id;
		protected BaseLayer baseLayer;
		protected BaseMap baseMap;
		protected Point anchorDelta;
		protected Point location;
		protected string name;
		protected double pctX;
		protected double pctY;
		protected int rotation;
		protected int zIndex;
		protected int zoomThreshold;

		public BaseMarker()
		{
		}

		public BaseMarker(int id, int layerId)
		{
			this.id = id;
		}

		public virtual BaseLayer BaseLayer
		{
			get { return baseLayer; }
			set { baseLayer = value; }
		}

		public virtual BaseMarkerDefinition BaseMarkerDefinition
		{
			get { return baseMarkerDefinition; }
			set { baseMarkerDefinition = value; }
		}

		public bool InstanceHasActions
		{
			// Determine if this marker's instance has it's own actions.
			// If not, the marker's definition's actions apply.
			get { return baseMarkerInstance != null && baseMarkerInstance.BaseActionMarkerRuleSet != null; }
		}

		public virtual Point AnchorDelta
		{
			get { return anchorDelta; }
			set { anchorDelta = value; }
		}

		public virtual int Id
		{
			get { return id; }
			set { id = value; }
		}

		public virtual Point Location
		{
			get { return location; }
			set { location = value; }
		}

		public BaseMarkerInstance MarkerInstance
		{
			get { return baseMarkerInstance; }
		}

		public virtual string Name
		{
			get { return name; }
			set { name = value; }
		}

		public virtual double PctX
		{
			get { return pctX; }
			set { pctX = value; }
		}

		public virtual double PctY
		{
			get { return pctY; }
			set { pctY = value; }
		}

		public virtual int Rotation
		{
			get { return rotation; }
			set { rotation = value; }
		}

		public int ZIndex
		{
			get { return zIndex; }
			set { zIndex = value; }
		}

		public virtual int ZoomThreshold
		{
			get { return zoomThreshold; }
			set { zoomThreshold = value; }
		}

		public void AddBaseMarkerInstance(BaseMarkerRuleSet baseActionRuleSet, BaseMarkerRuleSet baseJavascriptRuleSet)
		{
			baseMarkerInstance = new BaseMarkerInstance(this, baseActionRuleSet, baseJavascriptRuleSet);
		}

		public virtual BaseMarkerInstance MarkerInstanceForPageOrAncestors(BasePage basePage)
		{
			return baseMarkerInstance;
		}

		public virtual string ToolTipForTheme(int themeId)
		{
			return "tooltip text from BaseMarker";
		}
	}
}
