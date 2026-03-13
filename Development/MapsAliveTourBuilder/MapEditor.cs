// Copyright (C) 2006 AvantLogic Corporation
using System;
using System.Drawing;

public class MapEditor
{
	public enum Usage
	{
		MapEditor = 1,
		GalleryEditor = 2
	}

	public static int CalculateMapSizeEnglargement(TourPage tourPage, int targetPercent)
	{
		int factor = CalculateMapScaleFactor(tourPage, targetPercent);
		if (factor < 100)
		{
			// The map has to be scaled down to fit in the map area at the targetPercent
			// and therefore it is already at least as big as desired and so we use it at 100%.
			factor = 100;
		}
		return factor;
	}

	public static int CalculateMapSizeReduction(TourPage tourPage, int targetPercent)
	{
		int factor = CalculateMapScaleFactor(tourPage, targetPercent);
		return factor;
	}

	private static int CalculateMapScaleFactor(TourPage tourPage, int targetPercent)
	{
		double targetWidth = ((double)tourPage.ScaledMapSize.Width * ((double)targetPercent / 100.0));
		int factor = (int)Math.Round((targetWidth / (double)tourPage.MapImage.Size.Width) * 100);
		return factor;
	}
}
