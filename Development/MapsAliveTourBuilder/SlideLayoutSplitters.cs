// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public struct SlideLayoutSplitters
{
	public int H;
	public int V;
	public bool LockedH;
	public bool LockedV;

	public SlideLayoutSplitters(SlideLayoutSplitters Splitters)
	{
		H = Splitters.H;
		V = Splitters.V;
		LockedH = Splitters.LockedH;
		LockedV = Splitters.LockedV;
	}

	public SlideLayoutSplitters(int h, int v)
	{
		H = h;
		V = v;
		LockedH = false;
		LockedV = false;
	}

	public SlideLayoutSplitters(int h, int v, bool lockedH, bool lockedV)
	{
		H = h;
		V = v;
		LockedH = lockedH;
		LockedV = lockedV;
	}

	public static bool operator ==(SlideLayoutSplitters s1, SlideLayoutSplitters s2)
	{
		return
			s1.H == s2.H &&
			s1.V == s2.V &&
			s1.LockedH == s2.LockedH &&
			s1.LockedV == s2.LockedV;
	}

	public static bool operator !=(SlideLayoutSplitters s1, SlideLayoutSplitters s2)
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