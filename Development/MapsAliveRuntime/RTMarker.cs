// Copyright (C) 2003-2005 AvantLogic Corporation
using System;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Diagnostics;

namespace AvantLogic.MapsAlive.Runtime
{
	public class RTMarker:RTElement
	{
		protected Hashtable rtMarkerInstances = new Hashtable();
		protected Hashtable rtTooltips = new Hashtable();
		protected RTSymbol normalSymbol;
		protected RTSymbol rolloverSymbol;
		protected RTSymbol selectedSymbol;
		protected RTShape normalShape;
		protected RTShape rolloverShape;
		protected RTShape selectedShape;
		protected RTShape hotspotShape;
		protected SortedList rtEvents = new SortedList();
		protected Point anchorDelta;
		protected int definitionId;
		protected int layerId;
		protected int rotation;
		protected int stacking;
		protected string absoluteFileLocation;
		protected string relativeFileLocation;
		protected int zoomThreshold;
		protected double pctX;
		protected double pctY;

		public RTMarker(int id):this(id, id)
		{
			// Default stacking is equivalent to id
		}

		public RTMarker(int id, int stacking):base()
		{
			this.id = id;
			Stacking = stacking;
		}

		public RTMarker(int id, string absoluteFileLocation, string relativeFileLocation):this(id, id, absoluteFileLocation, relativeFileLocation)
		{
		}

		public RTMarker(int id, int stacking, string absoluteFileLocation, string relativeFileLocation):this(id, stacking)
		{
			this.absoluteFileLocation = absoluteFileLocation;
			this.relativeFileLocation = relativeFileLocation;
		}

		public override string Name
		{
			get { return name; }
			set { name = value.Replace(" ","").ToLower(); }
		}

		public Point AnchorDelta
		{
			get { return anchorDelta; }
			set { anchorDelta = value; }
		}

		public double PctX
		{
			get { return pctX; }
			set { pctX = value; }
		}

		public double PctY
		{
			get { return pctY; }
			set { pctY = value; }
		}

		public virtual int Rotation
		{
			get { return rotation; }
			set { rotation = value; }
		}

		public virtual int ZoomThreshold
		{
			get { return zoomThreshold; }
			set { zoomThreshold = value; }
		}

		public Hashtable RTTooltips 
		{
			get { return rtTooltips; }
		}

		public Hashtable RTMarkerInstances 
		{
			get { return rtMarkerInstances; }
		}

		public RTSymbol NormalSymbol
		{
			get { return normalSymbol; }
			set { normalSymbol = value; }
		}

		public RTSymbol RolloverSymbol
		{
			get { return rolloverSymbol; }
			set { rolloverSymbol = value; }
		}

		public RTSymbol SelectedSymbol
		{
			get { return selectedSymbol; }
			set { selectedSymbol = value; }
		}

		public RTShape NormalShape
		{
			get { return normalShape; }
			set { normalShape = value; }
		}

		public RTShape RolloverShape
		{
			get { return rolloverShape; }
			set { rolloverShape = value; }
		}

		public RTShape SelectedShape
		{
			get { return selectedShape; }
			set { selectedShape = value; }
		}

		public RTShape HotspotShape
		{
			get { return hotspotShape; }
			set { hotspotShape = value; }
		}
		
		public int LayerId
		{
			get { return layerId; }
			set { layerId = value; }
		}		
	
		public int DefinitionId
		{
			get { return definitionId; }
			set { definitionId = value; }
		}	

		public int Stacking 
		{
			get { return stacking; }
			set 
			{
				Debug.Assert(value > 0, "Stacking index must be > 0");
				stacking = value; 
			}
		}
		
		public SortedList RTEvents 
		{
			get { return rtEvents; }
		}

		public string AbsoluteFileLocation
		{
			get { return absoluteFileLocation; }
		}
	
		public string RelativeFileLocation
		{
			get { return relativeFileLocation; }
		}
	}
}
