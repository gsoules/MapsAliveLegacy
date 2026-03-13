// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

// These values are known in the DB -- do not change.
public enum SpecialOfferId
{
	None = 0,
	FreeSmartDraw = 1
}

public class SpecialOffer
{
	public SpecialOffer()
	{
	}

	public static SpecialOfferId GetSpecialOfferId(Account account, decimal orderAmount)
	{
		SpecialOfferId id = SpecialOfferId.None;

		return id;
	}

	public static string Instructions(SpecialOfferId specialOfferId)
	{
		return "";
	}
}
