// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;

/*
	To add a new Advice Type:
	1.  Add an enum for the advice to AdviceType.
	2.  Add the advice to adviceTable in the AdviceSet constructor.
	3.  Add a class for the advice to AdviceSet.
	4.  If the advice uses a new solution:
		- Add an enum to PageSolution or SlideSolution.
		- Add a case to Advice.EmitPageSolution or Advice.EmitSlideSolution.
	5.  Add the advice to the TourAdvisor.AnalyzeTour.
*/

// The Tour Advisor will show advice in the REVERSE of the order listed here.
public enum AdviceType
{
	RenameSlide,
	RenameMap,
	RenameDataSheet,
	RenameGallery,
	UploadLargerSlidePhoto,
	DisableMapZoom,
	EnableMapZoom,
	PlaceMarkersOnMap,
	PlaceMarkerOnMapForSlide,
	AddSlide,
	UploadSlidePhoto,
	UploadMapImage,
	IframeInPopup
}

public class AdviceSet
{
	private TourPage tourPage;
	private Hashtable adviceTable;
	private int level1Count;
	private int level2Count;
	private int maxMessagesPerAdviceSet;
	private int skippedMessageCount;

	public AdviceSet(TourPage tourPage, int maxMessagesPerAdviceSet)
	{
		this.tourPage = tourPage;
		this.maxMessagesPerAdviceSet = maxMessagesPerAdviceSet;
		adviceTable = new Hashtable();

		adviceTable.Add(AdviceType.AddSlide, new AddSlide(this));
		adviceTable.Add(AdviceType.EnableMapZoom, new EnableMapZoom(this));
		adviceTable.Add(AdviceType.DisableMapZoom, new DisableMapZoom(this));
		adviceTable.Add(AdviceType.PlaceMarkerOnMapForSlide, new PlaceMarkerOnMapForSlide(this));
		adviceTable.Add(AdviceType.PlaceMarkersOnMap, new PlaceMarkersOnMap(this));
		adviceTable.Add(AdviceType.RenameMap, new RenameMap(this, tourPage));
		adviceTable.Add(AdviceType.RenameDataSheet, new RenameDataSheet(this, tourPage));
		adviceTable.Add(AdviceType.RenameGallery, new RenameGallery(this, tourPage));
		adviceTable.Add(AdviceType.RenameSlide, new RenameSlide(this));
		adviceTable.Add(AdviceType.UploadMapImage, new UploadMapImage(this, tourPage));
		adviceTable.Add(AdviceType.UploadSlidePhoto, new UploadSlidePhoto(this, tourPage));
		adviceTable.Add(AdviceType.UploadLargerSlidePhoto, new UploadLargerSlidePhoto(this));
		adviceTable.Add(AdviceType.IframeInPopup, new IframeInPopup(this));
	}

	public void AdviceAdded(int level)
	{
		if (level == 1)
			level1Count++;
		else if (level == 2)
			level2Count++;
	}

	public void AdviceNotAdded()
	{
		skippedMessageCount++;
	}

	public int Count
	{
		get { return level1Count + level2Count; }
	}

	public int Level1Count
	{
		get { return level1Count; }
	}

	public int Level2Count
	{
		get { return level2Count; }
	}

	public int MaxMessagesPerAdviceSet
	{
		get { return maxMessagesPerAdviceSet; }
	}

	public int SkippedMessageCount
	{
		get { return skippedMessageCount; }
	}

	public Advice GetAdvice(AdviceType type)
	{
		Advice advice = (Advice)adviceTable[type];
		Debug.Assert(advice != null, "The advice table does not contain an entry for " + type);
		return advice;
	}

	public void EmitHtml(StringBuilder html, string title, int tourBuilderActionId)
	{
		if (Count == 0)
			return;

		foreach (DictionaryEntry dictionaryEntry in adviceTable)
		{
			Advice advice = (Advice)dictionaryEntry.Value;
			advice.EmitAdviceAndSolutions(html, tourBuilderActionId);
		}
	}

	public TourPage TourPage
	{
		get { return tourPage; }
	}

	#region ===== Advice Subclasses============================================

	private class AddSlide : Advice
	{
		public AddSlide(AdviceSet adviceSet)
			: base(adviceSet)
		{
			SetSeverity(2);
			SetTitle("Add hotspot for this map");

			int slideCount = adviceSet.TourPage.TourViews.Count;
			string info;
			if (slideCount == 0)
				info = "it has no hotspots";
			else if (slideCount == 1)
				info = "it has just 1 hotspot";
			else
				info = string.Format("it has only {0} hotspots", slideCount);
			
			SetAdditionalInfo(info);
			
			AddSolution(PageSolution.AddSlide);
		}
	}

	private class DisableMapZoom : Advice
	{
		public DisableMapZoom(AdviceSet adviceSet)
			: base(adviceSet)
		{
			SetSeverity(1);
			SetTitle("Turn MapZoom off");
			SetAdditionalInfo("the map image is not very large");
			AddSolution(PageSolution.DisableMapZoom);
		}
	}

	private class EnableMapZoom : Advice
	{
		public EnableMapZoom(AdviceSet adviceSet)
			: base(adviceSet)
		{
			SetSeverity(1);
			SetTitle("Turn MapZoom on");
			SetAdditionalInfo("the map image is large enough for zooming");
			AddSolution(PageSolution.EnableMapZoom);
		}
	}

	private class PlaceMarkerOnMapForSlide : Advice
	{
		public PlaceMarkerOnMapForSlide(AdviceSet adviceSet)
			: base(adviceSet)
		{
			SetSeverity(2);
			SetTitle("Place a marker on the map for this hotspot", "Place markers on the map for these hotspots");
			AddSolution(SlideSolution.GoToMap);
		}
	}

	private class PlaceMarkersOnMap : Advice
	{
		public PlaceMarkersOnMap(AdviceSet adviceSet)
			: base(adviceSet)
		{
			SetSeverity(2);
			SetTitle("Place markers on map");

			int markersOnMap = adviceSet.TourPage.MarkersOnMap;
			string info;
			if (markersOnMap == 0)
				info = "it has no markers";
			else if (markersOnMap == 1)
				info = "it has just 1 marker";
			else
				info = string.Format("it has only {0} markers", markersOnMap);

			SetAdditionalInfo(info);

			AddSolution(PageSolution.GoToMap);
		}
	}

	private class RenameMap : Advice
	{
		public RenameMap(AdviceSet adviceSet, TourPage tourPage)
			: base(adviceSet)
		{
			SetSeverity(1);
			SetTitle("Give this map a meaningful name");
			AddSolution(PageSolution.RenameMap);
		}
	}

	private class RenameGallery : Advice
	{
		public RenameGallery(AdviceSet adviceSet, TourPage tourPage)
			: base(adviceSet)
		{
			SetSeverity(1);
			SetTitle("Give this gallery a meaningful name");
			AddSolution(PageSolution.RenameGallery);
		}
	}

	private class RenameDataSheet : Advice
	{
		public RenameDataSheet(AdviceSet adviceSet, TourPage tourPage)
			: base(adviceSet)
		{
			SetSeverity(1);
			SetTitle("Give this data sheet a meaningful name");
			AddSolution(SlideSolution.RenameDataSheet);
		}
	}

	private class RenameSlide : Advice
	{
		public RenameSlide(AdviceSet adviceSet)
			: base(adviceSet)
		{
			SetSeverity(1);
			SetTitle("Give this hotspot a meaningful name", "Give these hotspots meaningful names");
			AddSolution(SlideSolution.RenameSlide);
		}
	}

	private class UploadMapImage : Advice
	{
		public UploadMapImage(AdviceSet adviceSet, TourPage tourPage)
			: base(adviceSet)
		{
			SetSeverity(2);
			SetTitle("Choose a map image for this page");
			AddSolution(PageSolution.UploadMapImage);
		}
	}

	private class UploadLargerSlidePhoto : Advice
	{
		public UploadLargerSlidePhoto(AdviceSet adviceSet)
			: base(adviceSet)
		{
			SetSeverity(1);
			SetTitle("Upload a larger photo for this hotspot", "Upload larger photos for these hotspots");
			SetAdditionalInfo("there is empty space to the right and below the image");
			AddSolution(SlideSolution.UploadSlidePhoto);
		}
	}

	private class UploadSlidePhoto : Advice
	{
		public UploadSlidePhoto(AdviceSet adviceSet, TourPage tourPage)
			: base(adviceSet)
		{
			SetSeverity(2);

			if (tourPage.IsDataSheet)
			{
				SetTitle("Upload a photo for this data sheet", string.Empty);
				AddSolution(SlideSolution.UploadSlidePhoto);
			}
			else
			{
				SetTitle("Upload a photo for this hotspot", "Upload photos for these hotspots");
				AddSolution(SlideSolution.UploadSlidePhoto);
			}
		}
	}

	private class IframeInPopup : Advice
	{
		public IframeInPopup(AdviceSet adviceSet)
			: base(adviceSet)
		{
			SetSeverity(2);
			string single = "this popup hotspot";
			string plural = "these popup hotspots";
			string warning = "<span style='color:red;'>Warning:</span> The iframe in {0} may not display correctly with Firefox on Mac OS X"; 
			SetTitle(string.Format(warning, single), string.Format(warning, plural));
		}
	}
	#endregion
}
