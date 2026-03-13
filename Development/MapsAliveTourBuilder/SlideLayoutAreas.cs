// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Drawing;

public enum SlideLayoutAreasType
{
	Vertical_LeftTall_RightSplit,
	Vertical_LeftSplit_RightTall,
	Vertical_LeftTall_RightTall,
	Horizontal_TopWide_BottomSplit,
	Horizontal_TopSplit_BottomWide,
	Horizontal_TopWide_BottomWide,
	SingleArea
}

public class SlideLayoutAreas
{
	private Rectangle _topLeft;
	private Rectangle _topRight;
	private Rectangle _bottomLeft;
	private Rectangle _bottomRight;
	private SlideLayoutAreasType slideLayoutAreasType;

	// Note: if you change this value, also change it in LayoutEditor.js
	private const int minSplitter = 36;

	public SlideLayoutAreas(SlideLayoutAreasType slideLayoutAreasType, SlideLayout slideLayout)
	{
		this.slideLayoutAreasType = slideLayoutAreasType;

		int spacingH = slideLayout.Spacing.H;
		int spacingV = slideLayout.Spacing.V;

		int marginLeft = slideLayout.Margin.Left;
		int marginRight = slideLayout.Margin.Right;
		int marginTop = slideLayout.Margin.Top;
		int marginBottom = slideLayout.Margin.Bottom;

		Size innerSize = slideLayout.InnerSize;

		// Declare shortcut names to make the code easier to follow.
		int innerW = innerSize.Width;
		int innerH = innerSize.Height;
		int splitterH = slideLayout.Splitters.H;
		int splitterV = slideLayout.Splitters.V;

		// Determine how much height the slide needs in order for the horizontal splitter to
		// be positioned such that there is at least the minimum height above and below it.
		bool enoughRoomToAdjustSplitterH = innerH > minSplitter * 2 + spacingH;
		bool enoughRoomToAdjustSplitterV = innerW > minSplitter * 2 + spacingV;

		// Determine the middle spliter position to use if there is not enough room to adjust it.
		int middleH = (innerH - spacingH) / 2;
		int middleV = (innerW - spacingV) / 2;

		// Adjust the horizontal splitter.
		if (!slideLayout.Splitters.LockedH)
		{
			if (splitterH < minSplitter)
			{
				// The horizontal splitter is too high. Lower it to the min height.
				splitterH = enoughRoomToAdjustSplitterH ? minSplitter : middleH;
			}
			else if (splitterH > innerH - minSplitter - spacingH)
			{
				// The horizontal splitter is too low, Raise to the min height from the bottom.
				splitterH = enoughRoomToAdjustSplitterH ? innerH - minSplitter - spacingH : middleH;
			}
		}

		// Adjust the vertical splitter.
		if (!slideLayout.Splitters.LockedV)
		{
			if (splitterV < minSplitter)
			{
				// The vertical splitter is too far left. Move it to the min distance from the left.
				splitterV = enoughRoomToAdjustSplitterV ? minSplitter : middleV;
			}
			else if (splitterV > innerW - minSplitter - spacingV)
			{
				// The vertical splitter is too far right. Move it to the min distance from the right.
				splitterV = enoughRoomToAdjustSplitterV ? innerW - minSplitter - spacingV : middleV;
			}
		}

		// Create new splitters with the adjusted positions.
		slideLayout.Splitters = new SlideLayoutSplitters(splitterH, splitterV, slideLayout.Splitters.LockedH, slideLayout.Splitters.LockedV);

		// Use the position of the slitters to determine the size of the layout's areas.
		int rightW = innerW - splitterV - spacingV;
		int bottomH = innerH - splitterH - spacingH;
		int leftX = marginLeft;
		int rightX = marginLeft + splitterV + spacingV;
		int topY = marginTop;
		int bottomY = marginTop + splitterH + spacingH;

		switch (slideLayoutAreasType)
		{
			case SlideLayoutAreasType.Vertical_LeftTall_RightSplit:
				Left = new Rectangle(leftX, topY, splitterV, innerH);
				TopRight = new Rectangle(rightX, topY, rightW, splitterH);
				BottomRight = new Rectangle(rightX, bottomY, rightW, bottomH);
				break;

			case SlideLayoutAreasType.Vertical_LeftSplit_RightTall:
				TopLeft = new Rectangle(leftX, topY, splitterV, splitterH);
				BottomLeft = new Rectangle(leftX, bottomY, splitterV, bottomH);
				Right = new Rectangle(rightX, topY, rightW, innerH);
				break;

			case SlideLayoutAreasType.Vertical_LeftTall_RightTall:
				Left = new Rectangle(leftX, topY, splitterV, innerH);
				Right = new Rectangle(rightX, topY, rightW, innerH);
				break;

			case SlideLayoutAreasType.Horizontal_TopWide_BottomSplit:
				Top = new Rectangle(leftX, topY, innerW, splitterH);
				BottomLeft = new Rectangle(leftX, bottomY, splitterV, bottomH);
				BottomRight = new Rectangle(rightX, bottomY, rightW, bottomH);
				break;

			case SlideLayoutAreasType.Horizontal_TopSplit_BottomWide:
				TopLeft = new Rectangle(leftX, topY, splitterV, splitterH);
				TopRight = new Rectangle(rightX, topY, rightW, splitterH);
				Bottom = new Rectangle(leftX, bottomY, innerW, bottomH);
				break;

			case SlideLayoutAreasType.Horizontal_TopWide_BottomWide:
				Top = new Rectangle(leftX, topY, innerW, splitterH);
				Bottom = new Rectangle(leftX, bottomY, innerW, bottomH);
				break;

			case SlideLayoutAreasType.SingleArea:
				Top = new Rectangle(leftX, topY, innerW, innerH);
				break;

			default:
				break;
		}
	}

	public Rectangle Bottom
	{
		get { return _bottomLeft; }
		set { _bottomLeft = value; }
	}

	public Rectangle BottomLeft
	{
		get { return _bottomLeft; }
		set { _bottomLeft = value; }
	}

	public Rectangle BottomRight
	{
		get { return _bottomRight; }
		set { _bottomRight = value; }
	}

	public Rectangle Empty
	{
		get { return Rectangle.Empty; }
	}

	public Rectangle Left
	{
		get { return _topLeft; }
		set { _topLeft = value; }
	}

	public static int MinSplitter
	{
		get { return minSplitter; }
	}

	public Rectangle Right
	{
		get { return _topRight; }
		set { _topRight = value; }
	}

	public Rectangle Top
	{
		get { return _topLeft; }
		set { _topLeft = value; }
	}

	public Rectangle TopLeft
	{
		get { return _topLeft; }
		set { _topLeft = value; }
	}

	public Rectangle TopRight
	{
		get { return _topRight; }
		set { _topRight = value; }
	}
}

