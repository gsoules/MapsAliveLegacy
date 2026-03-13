// Copyright (C) 2003-2008 AvantLogic Corporation
using System;
using System.Collections;
using System.Drawing;

namespace AvantLogic.MapsAlive.Runtime
{
	public class RTShape : RTElement
	{
		public const int CIRCLE = 1;
		public const int RECTANGLE = 2;
		public const int POLYGON = 3;
		public const int LINE = 4;
		public const int HYBRID = 5;
		
		protected int type;
		protected string coordinates;
		protected string effects;
		protected Color fillColor = new Color();
		protected Color lineColor = new Color();
		protected int fillColorOpacity;
		protected int lineColorOpacity;
		protected int lineWidth;
		protected int lineStyle;

		#region ===== Constructors ====================================================

		public RTShape()
		{
			this.Name = "empty_shape";
		}

		#endregion

		#region ===== Accessors ====================================================

		public int Type
		{
			get { return type; }
			set { type = value; }
		}

		public string Coordinates
		{
			get { return coordinates; }
			set { coordinates = value; }
		}

		public string Effects
		{
			get { return effects; }
			set { effects = value; }
		}

		public Color FillColor
		{
			get { return fillColor; }
			set { fillColor = value; }
		}

		public int FillColorOpacity
		{
			get { return fillColorOpacity; }
			set { fillColorOpacity = value; }
		}

		public Color LineColor
		{
			get { return lineColor; }
			set { lineColor = value; }
		}

		public int LineColorOpacity
		{
			get { return lineColorOpacity; }
			set { lineColorOpacity = value; }
		}

		public int LineWidth
		{
			get { return lineWidth; }
			set { lineWidth = value; }
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