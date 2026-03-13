// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Drawing;
using System.Text;

public class TourAdvisor
{
	private enum MissingElement
	{
		None,
		Page,
		Map,
		Slide,
		Photo
	}

	private int level1AdviceCount;
	private int level2AdviceCount;
	private Hashtable tourAdviceSets = new Hashtable();
	private Tour tour;

	public TourAdvisor(Tour tour)
	{
		this.tour = tour;
	}

	#region ===== Public ============================================================

	public MemberPageActionId DefaultActionId
	{
		get
		{
			MemberPageActionId actionId;

			if (tour.TourPages.Count >= 1 && !TourMeetsMinimumRequirements)
			{
				TourView firstView = tour.FirstPage.FirstTourView;
				if (firstView != null && !firstView.HasImage)
					actionId = MemberPageActionId.EditHotspotContent;
				else
					actionId = MemberPageActionId.UploadMap;
			}
			else
				actionId = MemberPageActionId.TourManager;

			return actionId;
		}
	}

	public int Level1AdviceCount
	{
		get { return level1AdviceCount; }
	}

	public int Level2AdviceCount
	{
		get { return level2AdviceCount; }
	}

	public string EmitAdviceAndSolutionsHtml(int tourBuilderActionId, int maxMessagesPerAdviceSet)
	{
		level1AdviceCount = 0;
		level2AdviceCount = 0;
		StringBuilder html = new StringBuilder();

		// Create an advice set for each page of the tour.
		AnalyzeTour(maxMessagesPerAdviceSet);

		// Create HTML from each advice set that got created.
		foreach (TourPage tourPage in tour.TourPages)
		{
			// Get the advice set for this page.
			AdviceSet adviceSet = (AdviceSet)tourAdviceSets[tourPage.Id];

			if (adviceSet.Count > 0)
			{
				// Emit the advice HTML rows this page.
				string note = string.Empty;
				if (adviceSet.SkippedMessageCount > 0)
					note = string.Format("<span class='maNote'> ({0} advice messages not shown)</span>", adviceSet.SkippedMessageCount);
				html.Append(string.Format("<tr><td colspan='2' class='maAdvisorPageSection'>{0}{1}</td>", tourPage.Name, note));
				adviceSet.EmitHtml(html, tourPage.Name, tourBuilderActionId);
				html.Append("</td></tr>");
				level1AdviceCount += adviceSet.Level1Count;
				level2AdviceCount += adviceSet.Level2Count;
			}
		}

		// Free up the advice sets.
		tourAdviceSets.Clear();

		// Wrap the page HTML rows in a table.
		if (level1AdviceCount > 0 || level2AdviceCount > 0)
		{
			html.Insert(0, "<table class=\"assistLink\" >");
			html.Append("</table>");
		}

		// Return the html.  If no advice is needed, an empty string is returned.
		return html.ToString();
	}

	public string PreviewScript(int actionId, int selectedTourPageNumber)
	{
		// Construct the Javascript to be executed when a user clicks the Tour Preview button.
		string message;
		string script;
		const string prefix = "This tour is not ready for preview because ";
		TourPage firstPage = tour.FirstPage;

		switch (IdentifyMissingElement())
		{
			case MissingElement.Map:
				message = string.Format("{0}<span class='alertTarget'>{1}</span> has no map or hotspots yet.", prefix, firstPage.Name);
                string instructions;
                if (actionId == (int)MemberPageActionId.UploadMap)
                    instructions = "You can upload a map now";
                else
                    instructions = "To upload a map, choose <b>Map > Choose Map Image</b> from the menu.";
				script = ConstructAlertScript(string.Format("{0}<br/><br/>{1}", message, instructions));
				break;
		
			default:
				script = string.Format("maOnEventSaveAndTransfer('/Members/TourPreview.aspx?aid={0}&page={2}&rev={3}');", actionId, tour.Id, selectedTourPageNumber, App.Revision);
				break;
		}

		return script;
	}

	public bool TourMeetsMinimumRequirements
	{
		get { return IdentifyMissingElement() == MissingElement.None; }
	}
	#endregion


	#region ===== Private ===========================================================

	private void AnalyzeTour(int maxMessagesPerAdviceSet)
	{
		foreach (TourPage tourPage in tour.TourPages)
		{
			// Create an advice set for this page.
			AdviceSet adviceSet = new AdviceSet(tourPage, maxMessagesPerAdviceSet);
			tourAdviceSets.Add(tourPage.Id, adviceSet);

			bool layoutHasMapArea = tourPage.ActiveSlideLayout.HasMapArea || tourPage.SlidesPopup;

			AnalyzePageName(tourPage, adviceSet);
			AnalyzeMapImage(tourPage, adviceSet, layoutHasMapArea);
			AnalyzeSlideCount(tourPage, adviceSet);
			
			bool advisedToPlaceMarkers = AnalyzeMapMarkerCount(tourPage, adviceSet, layoutHasMapArea);

			foreach (TourView tourView in tourPage.TourViews)
			{
				// Don't report missing slide markers if we already determined that they
				// don't have the minimum recommended number of markers.
				if (!advisedToPlaceMarkers)
					AnalyzeSlideMarker(tourView, adviceSet, layoutHasMapArea);
				
				AnalyzeSlideImage(tourView, adviceSet);
				AnalyzeSlideName(tourView, adviceSet);
			}
		}
	}

	private bool UsingDefaultName(string name, string prefix)
	{
		// This method is looking for names that begin with the prefix and end in a single digit.
		// It identifies default names like "Map 1" or "Hotspot 7" but it ignores "Map A".

		if (name.StartsWith(prefix + " "))
		{
			if (name.Length == prefix.Length + 2)
			{
				char last = name[name.Length - 1];
				return Char.IsDigit(last);
			}
		}
		return false;
	}

	private void AnalyzePageName(TourPage tourPage, AdviceSet adviceSet)
	{
		if (UsingDefaultName(tourPage.Name, "Map"))
			adviceSet.GetAdvice(AdviceType.RenameMap).Add();
		else if (UsingDefaultName(tourPage.Name, "Gallery"))
			adviceSet.GetAdvice(AdviceType.RenameGallery).Add();
	}

	private void AnalyzeSlideName(TourView tourView, AdviceSet adviceSet)
	{
		if (UsingDefaultName(tourView.Title, "Hotspot"))
			adviceSet.GetAdvice(AdviceType.RenameSlide).AddAffectedSlide(tourView);
		else if (UsingDefaultName(tourView.Title, "Data Sheet"))
			adviceSet.GetAdvice(AdviceType.RenameDataSheet).AddAffectedSlide(tourView);
	}

	private void AnalyzeMapImage(TourPage tourPage, AdviceSet adviceSet, bool layoutHasMapArea)
	{
		if (!layoutHasMapArea || tourPage.IsGallery)
			return;

		if (tourPage.MapImage.HasFile)
		{
			if (tour.V3CompatibilityEnabled)
            {
                bool recomendMapZoom = tourPage.QualifiesForMapZoom(tourPage.MapAreaSize);
			    if (recomendMapZoom && !tourPage.MapCanZoom)
				    adviceSet.GetAdvice(AdviceType.EnableMapZoom).Add();
			    else if (!recomendMapZoom && tourPage.MapCanZoom)
				    adviceSet.GetAdvice(AdviceType.DisableMapZoom).Add();
            }
		}
		else
		{
			adviceSet.GetAdvice(AdviceType.UploadMapImage).Add();
		}
	}

	private bool AnalyzeMapMarkerCount(TourPage tourPage, AdviceSet adviceSet, bool layoutHasMapArea)
	{
		if (tourPage.IsDataSheet || !layoutHasMapArea)
			return false;

		// Don't report missing markers for a page that has no map. Let them add the map first.
		if (!tourPage.MapImage.HasFile)
			return false;

		// Don't report missing markers when there are not at least two slides.
		if (tourPage.TourViews.Count < 2)
			return false;

		if (tourPage.MarkersOnMap < 2)
		{
			adviceSet.GetAdvice(AdviceType.PlaceMarkersOnMap).Add();
			return true;
		}

		return false;
	}

	private void AnalyzeSlideCount(TourPage tourPage, AdviceSet adviceSet)
	{
		if (!tourPage.IsDataSheet && tourPage.TourViews.Count < 2)
			adviceSet.GetAdvice(AdviceType.AddSlide).Add();
	}

	private void AnalyzeSlideImage(TourView tourView, AdviceSet adviceSet)
	{
		bool layoutHasImageArea = tourView.TourPage.ActiveSlideLayout.HasImageArea;
		if (tourView.MediaType == SlideMediaType.Photo && layoutHasImageArea && !tourView.HasImage && tourView.ShowContentEvent == ShowContentEvent.OnMouseover)
			adviceSet.GetAdvice(AdviceType.UploadSlidePhoto).AddAffectedSlide(tourView);
		else if (tourView.MediaType == SlideMediaType.Embed && tourView.TourPage.SlidesPopup)
		{
			if (tourView.EmbedText.Trim().ToLower().StartsWith("<iframe"))
			{
				adviceSet.GetAdvice(AdviceType.IframeInPopup).AddAffectedSlide(tourView);
			}
		}
	}

	private void AnalyzeSlideMarker(TourView tourView, AdviceSet adviceSet, bool layoutHasMapArea)
	{
		if (tourView.MarkerHasBeenPlacedOnMap)
			return;

		// Don't report a missing marker for a slide that has no photo.  Let them add the photo first.
		if (!tourView.HasImage)
			return;

		TourPage tourPage = tourView.TourPage;
		if (tourPage.IsDataSheet || !layoutHasMapArea)
			return;

		adviceSet.GetAdvice(AdviceType.PlaceMarkerOnMapForSlide).AddAffectedSlide(tourView);
	}

	private MissingElement IdentifyMissingElement()
	{
		if (tour.TourPages.Count == 0)
			return MissingElement.Page;

		TourPage firstPage = tour.FirstPage;

		if (firstPage == null)
		{
			// This should never happen except during development when working on export/import.
			// Sometimes we quit in the middle of an operation and create a corrupt tour.
			System.Diagnostics.Debug.Fail("FirstPage is null");
			return MissingElement.None;
		}

		// If the tour has or once had more than 1 page, don't flag missing elements.
		// We do this to keep the logic simple. The primary reason for this logic is to handle
		// the common case of a new user creating their first tour with the default page and
		// trying to preview it before it is ready. Handling mult-page tours gets tricky
		// because the logic to tell the user that the first page has a missing element while
		// they are currently on the second page and then automatically take them back to the
		// first page to add the element is more trouble than its worth.
		if (firstPage.PageNumber > 1 || tour.TourPages.Count > 1)
			return MissingElement.None;

		if (!firstPage.MapImage.HasFile && firstPage.TourViews.Count == 0)
			return MissingElement.Map;

		return MissingElement.None;
	}

	private string ConstructAlertScript(string message)
	{
		return string.Format("maAlert('{0}');",
			Utility.JavascriptSingleQuotedString(message));
	}
	#endregion
}
