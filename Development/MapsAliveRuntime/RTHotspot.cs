// Copyright (C) 2003-2005 AvantLogic Corporation
using System;
using System.Collections;
using System.Drawing;

namespace AvantLogic.MapsAlive.Runtime
{
	public class RTHotspot:RTShape
	{
		protected SortedList rtEvents = new SortedList();

		#region ===== Constructors ====================================================

		public RTHotspot():base()
		{
			this.Name = "hotspot";
		}

		#endregion

		#region ===== Accessors ====================================================

		public SortedList RTEvents
		{
			get { return rtEvents; }
		}

		#endregion
	
		#region ===== Private ====================================================
		#endregion

		#region ===== Protected ====================================================
		#endregion

		#region ===== Public ====================================================
		#endregion
	}

}