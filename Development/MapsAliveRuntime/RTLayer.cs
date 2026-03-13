// Copyright (C) 2003-2005 AvantLogic Corporation
using System;
using System.IO;
using System.Collections;
using System.Diagnostics;

namespace AvantLogic.MapsAlive.Runtime
{
	public class RTLayer:RTElement
	{
		protected ArrayList rtMarkers = new ArrayList();
		protected Hashtable backgroundImages = new Hashtable();
		protected bool visibleOnLoad = true;
		protected int stacking;

		#region ===== Constructors ====================================================

		public RTLayer(int id):base()
		{
			this.id = id;

			// Default stacking is equivalent to id
			Stacking = this.id;
		}

		public RTLayer(int id, int stacking):base()
		{
			this.id = id;
			Stacking = stacking;
		}

		#endregion

		#region ===== Accessors ====================================================
	
		public ArrayList RTMarkers
		{
			get { return rtMarkers; }
		}		

		public bool VisibleOnLoad
		{
			get { return visibleOnLoad; }
			set { visibleOnLoad = value; }
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

		#endregion
	
		#region ===== Private ====================================================
		#endregion

		#region ===== Protected ====================================================
		#endregion

		#region ===== Public ====================================================

		public Hashtable BackgroundImages
		{
			get { return backgroundImages; }
		}

		#endregion

	}
}
