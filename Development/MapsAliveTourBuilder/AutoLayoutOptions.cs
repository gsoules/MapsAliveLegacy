// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public class AutoLayoutOptions
{
	private TourSizeType widthType;
	private TourSizeType heightType;
	private bool okToChooseDifferentLayout;
	private bool okToAdjustSlideLayout;

	public AutoLayoutOptions()
	{
		okToChooseDifferentLayout = false;
		OkToAdjustSlideLayout = true;
		widthType = TourSizeType.Unknown;
		heightType = TourSizeType.Unknown;
	}

	public bool OkToAdjustSlideLayout
	{
		get { return okToAdjustSlideLayout; }
		set { okToAdjustSlideLayout = value; }
	}

	public bool OkToChooseDifferentLayout
	{
		get { return okToChooseDifferentLayout; }
		set { okToChooseDifferentLayout = value; }
	}

	public bool TourWidthLocked
	{
		get { return widthType == TourSizeType.Exact; }
	}

	public bool TourHeightLocked
	{
		get { return heightType == TourSizeType.Exact; }
	}

	public TourSizeType WidthType
	{
		get
		{
			System.Diagnostics.Debug.Assert(widthType != TourSizeType.Unknown, "Width type was not initialized");
			return widthType;
		}
		
		set { widthType = value; }
	}

	public TourSizeType HeightType
	{
		get
		{
			System.Diagnostics.Debug.Assert(heightType != TourSizeType.Unknown, "Height type was not initialized");
			return heightType;
		}
		set { heightType = value; }
	}
}