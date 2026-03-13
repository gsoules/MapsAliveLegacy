// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.IO;
using System.Web;

public class ImporterForHotspots : Importer
{
	protected int importedSlideCount;
	protected int maxNewSlidesAllowed;

	public ImporterForHotspots(Tour tour, TourPage tourPage, Stream stream, string reportTitle)
		: base(tour, tourPage, stream, reportTitle)
	{
		maxNewSlidesAllowed = -1;
	}

	protected TourView CreateSlide(string slideId, string title, int markerId)
	{
		return CreateSlide(slideId, title, markerId, tour.SelectedTourPage);
	}

	protected TourView CreateSlide(string slideId, string title, int markerId, TourPage tourPage)
	{
		TourView tourView = TourView.ImportSlide(slideId, title, markerId, tourPage);
		importedSlideCount++;
		return tourView;
	}

	protected TourView GetTourViewBySlideId(TourPage tourPage, string slideId, string title)
	{
		foreach (TourView tourView in tourPage.TourViews)
		{
			if (slideId != string.Empty)
			{
				if (tourView.SlideId.ToLower() == slideId.Trim().ToLower())
					return tourView;
			}
			else
			{
				if (tourView.Title.ToLower() == title)
					return tourView;
			}
		}
		return null;
	}
}
