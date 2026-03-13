// Copyright (C) 2006-2009 AvantLogic Corporation
using System;
using System.Drawing;

namespace AvantLogic.MapsAlive.Engine
{
	public class BaseMarkerAppearance
	{
		protected BaseShape shape;
		protected BaseSymbol symbol;

		public BaseMarkerAppearance()
		{
			this.symbol = null;
			this.shape = null;
		}

		public BaseMarkerAppearance(BaseSymbol symbol, BaseShape shape)
		{
			this.symbol = symbol;
			this.shape = shape;
		}

		public BaseShape BaseShape
		{
			get { return shape; }
			set { shape = value; }
		}
	
		public bool Defined
		{
			get { return symbol != null || shape != null; }
		}

		public BaseSymbol BaseSymbol
		{
			get { return symbol; }
			set { symbol = value; }
		}
		
		public Bitmap SymbolBitmap
		{
			get { return symbol == null ? null : symbol.Bitmap; }
		}

		public Rectangle Bounds()
		{
			// Returns a rectangle that is the union of the symbol rectangle and shape rectangle.
			Rectangle shapeBounds = Rectangle.Empty;
			Rectangle symbolBounds = Rectangle.Empty;

			if (shape != null)
			{
				shapeBounds = shape.Rectangle;
			}

			if (symbol != null)
			{
				Size symbolSize = symbol.Bitmap.Size;
				symbolBounds = new Rectangle(new Point(0, 0), symbolSize);
			}
			
			// Get the height and width of each rectangle.
			int symbolWidth = symbolBounds.Width;
			int symbolHeight = symbolBounds.Height;
			int shapeWidth = shapeBounds.Width;
			int shapeHeight = shapeBounds.Height;
			
			Rectangle bounds = new Rectangle(0, 0, Math.Max(symbolWidth, shapeWidth), Math.Max(symbolHeight, shapeHeight));
			return bounds;
		}
	}
}
