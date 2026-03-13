// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public struct SlideLayoutMargin
{
	public int Top;
	public int Right;
	public int Bottom;
	public int Left;

	public SlideLayoutMargin(SlideLayoutMargin margin)
	{
		Top = margin.Top;
		Right = margin.Right;
		Bottom = margin.Bottom;
		Left = margin.Left;
	}

	public SlideLayoutMargin(int top, int right, int bottom, int left)
	{
		Top = top;
		Right = right;
		Bottom = bottom;
		Left = left;
	}

	public static bool operator ==(SlideLayoutMargin m1, SlideLayoutMargin m2)
	{
		return m1.Top == m2.Top && m1.Right == m2.Right && m1.Bottom == m2.Bottom && m1.Left == m2.Left;
	}

	public static bool operator !=(SlideLayoutMargin m1, SlideLayoutMargin m2)
	{
		return !(m1 == m2);
	}

	public override bool Equals(object o)
	{
		return true;
	}

	public override int GetHashCode()
	{
		return 0;
	}
}