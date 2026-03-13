// Copyright (C) 2003-2009 AvantLogic Corporation
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace AvantLogic.MapsAlive.Engine
{
	public enum ShapeType
	{
		Circle = 1,
		Rectangle = 2,
		Polygon = 3,
		Line = 4,
		Hybrid = 5
	}

	public class BaseShape
	{
		public BaseShape(BaseSymbol symbol)
		{
			Id = -1;
			Name = "";
			ShapeType = ShapeType.Rectangle;
			Size size = symbol.BaseFileDescriptor == null ? symbol.Bitmap.Size : symbol.BaseFileDescriptor.Size;
		}

		public BaseShape(ShapeType shapeType)
		{
			Id = -1;
			Name = "";
			ShapeType = shapeType;
		}

		public string Coords { get; set; }
		public string Effects { get; set; }
		public Color FillColor { get; set; }
		public int Id { get; set; }
		public Color LineColor { get; set; }
		public int LineWidth { get; set; }
		public string Name { get; set; }
		public Rectangle Rectangle { get; set; }
		public ShapeType ShapeType { get; set; }
		public Point SymbolLocation { get; set; }

		public int FillColorOpacity
		{
			get { return AlphaToOpacity(FillColor.A); }
			set { FillColor = Color.FromArgb(OpacityToAlpha(value), FillColor); }
		}

		public int LineColorOpacity
		{
			get { return AlphaToOpacity(LineColor.A); }
			set { LineColor = Color.FromArgb(OpacityToAlpha(value), LineColor); }
		}

		private int AlphaToOpacity(int alpha)
		{
			// Convert the a color's alpha value to a percentage.
			return (int)Math.Ceiling(alpha / 256.0 * 100.0);
		}

		private int OpacityToAlpha(int opacity)
		{
			// Convert an opacity percentage to an alpha value.
			return (int)((opacity * 255) / 100);
		}
	}
}
