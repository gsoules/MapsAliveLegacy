// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Drawing;

public struct SlideLayoutAreaSizes
{
	public Size Map;
	public Size Image;
	public Size Text;

	public SlideLayoutAreaSizes(Size map, Size image, Size text)
	{
		Map = map;
		Image = image;
		Text = text;
	}
}