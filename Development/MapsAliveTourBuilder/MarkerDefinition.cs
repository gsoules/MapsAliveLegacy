// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Drawing;
using AvantLogic.MapsAlive.Engine;

public class MarkerDefinition
{
	private Point anchorDelta;
	private Rectangle bounds;
	private BaseMarkerDefinition baseMarkerDefinition;

	public MarkerDefinition(int definitionId, string name, BaseMarkerRuleSet markerRuleSet)
	{
		baseMarkerDefinition = new BaseMarkerDefinition(definitionId, name, markerRuleSet);
	}

	public Point AnchorDelta
	{
		get { return anchorDelta; }
		set { anchorDelta = value; }
	}

	public BaseMarkerDefinition Base
	{
		get { return baseMarkerDefinition; }
	}

	public Rectangle Bounds
	{
		get
		{
			if (bounds == Rectangle.Empty)
			{
				Rectangle boundsNormal = Base.BaseNormalAppearance.Bounds();
				Rectangle boundsSelected = Base.BaseSelectedAppearance.Bounds();
				bounds = Rectangle.Union(boundsNormal, boundsSelected);
			}

			return bounds;
		}
	}

	public static MarkerDefinition CreateMarkerDefinitionForRoute()
	{
		// Create an invisible empty rectangle to be used as the shape for route markers.
		// This was done so only the route appeared when a route marker was drawn in Flash.
		// See if this logic can now be eliminated.

		MarkerDefinition definition;

		BaseShape shape = new BaseShape(AvantLogic.MapsAlive.Engine.ShapeType.Rectangle);
		shape.Id = 0;
		shape.FillColorOpacity = 0;
		shape.LineColorOpacity = 0;
		shape.LineWidth = 0;
		shape.Coords = "0,0,0,0";
		shape.Rectangle = new Rectangle();

		definition = new MarkerDefinition(0, string.Empty, null);
		definition.Base.BaseNormalAppearance = new BaseMarkerAppearance(null, shape);
		definition.Base.BaseSelectedAppearance = new BaseMarkerAppearance(null, shape);

		return definition;
	}

	public int LineWidth
	{
		get
		{
			BaseShape shape = Base.NormalAppearance.BaseShape;
			if (shape == null)
				return 0;
			else
				return shape.LineWidth;
		}
	}

	public Size SizeWithBorder
	{
		get
		{
			Size size = Bounds.Size;
			int border = (LineWidth + 1) / 2;
			size.Width += border * 2;
			size.Height += border * 2;
			return size;
		}
	}
}

