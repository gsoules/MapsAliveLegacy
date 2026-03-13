// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public struct SlideLayoutSpacing
{
	public int H;
	public int V;

	public SlideLayoutSpacing(SlideLayoutSpacing spacing)
	{
		H = spacing.H;
		V = spacing.V;
	}

	public SlideLayoutSpacing(int h, int v)
	{
		H = h;
		V = v;
	}

	public static bool operator ==(SlideLayoutSpacing s1, SlideLayoutSpacing s2)
	{
		return s1.H == s2.H && s1.V == s2.V;
	}

	public static bool operator !=(SlideLayoutSpacing s1, SlideLayoutSpacing s2)
	{
		return !(s1 == s2);
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