// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Web;

public class AutoLayout
{
	private Tour tour;
	private TourPage tourPage;

	public AutoLayout(TourPage tourPage)
	{
		this.tourPage = tourPage;
		this.tour = tourPage.Tour;
	}

	private void HandleAutoLayoutRequest(NameValueCollection queryString, bool changedLayoutInSameFamily, bool changedLayoutInDifferentFamily)
	{
		string requestAdjust = queryString["adjust"];
		if (requestAdjust == null)
			return;

		TourSizeType widthType;
		TourSizeType heightType;

		switch (requestAdjust)
		{
			case "0":
				// Don't change the tour size.
				widthType = TourSizeType.Exact;
				heightType = TourSizeType.Exact;
				break;

			case "1":
				// Ok to change tour size for single page tours and when switching
				// to a new layout in a different group than the current layout.
				if (changedLayoutInDifferentFamily)
				{
					widthType = tour.WidthType;
					heightType = tour.HeightType;
				}
				else
				{
					widthType = tour.HasMoreThanOnePage || changedLayoutInSameFamily ? TourSizeType.Exact : tour.WidthType;
					heightType = tour.HasMoreThanOnePage || changedLayoutInSameFamily ? TourSizeType.Exact : tour.HeightType;
				}

				break;

			case "2":
				// User explicitly chose to run auto layout and accepted the/ warning that the page
				// size could change for all pages. Unlock the splitters since this is a user request
				// for autolayout and therefore we don't want to be restricted by existing locks.
				widthType = tour.WidthType;
				heightType = tour.HeightType;
				tourPage.UnlockSplitters();
				break;

			default:
				System.Diagnostics.Debug.Fail("Invalid autolayout request " + requestAdjust);
				return;
		}

		// Run auto layout on all the pages in the tour.
		AutoLayoutOptions autoLayoutOptions = new AutoLayoutOptions();
		autoLayoutOptions.WidthType = widthType;
		autoLayoutOptions.HeightType = heightType;
		tourPage.LayoutManager.PerformAutoLayout(autoLayoutOptions);
		tourPage.InvalidateThumbnail();
		tourPage.LayoutManager.PerformAutoLayoutOnOtherPages(tourPage);

		if (changedLayoutInSameFamily || changedLayoutInDifferentFamily)
		{
			// We got here from the Template Choices page when the user clicked on a new template.
			MemberPageActionId returnActionId = tourPage.SlidesPopup ? MemberPageActionId.PopupAppearance : MemberPageActionId.TourLayoutAdvanced;
			HttpContext.Current.Response.Redirect(MemberPageAction.ActionPageTarget(returnActionId));
		}
	}

	private void HandleDisableAutoLayoutRequest(NameValueCollection queryString)
	{
		string requestDisable = queryString["disable"];
		if (requestDisable == "1")
			tour.AutoLayoutEnabled = false;
		else if (requestDisable == "0")
			tour.AutoLayoutEnabled = true;
	}

	private void HandleNewTemplateChoiceRequest(NameValueCollection queryString, out bool changedLayoutInSameFamily, out bool changedLayoutInDifferentFamily)
	{
		changedLayoutInDifferentFamily = false;
		changedLayoutInSameFamily = false;
		
		int newPatternId = 0;
		
		if (int.TryParse(queryString["layout"], out newPatternId))
		{
			if (!Enum.IsDefined(typeof(SlideLayoutPattern), newPatternId))
				return;

			// A layout Id was on the query string which means we came here from the Template Choices page.
			SlideLayout slideLayout = tourPage.ActiveSlideLayout;
			SlideLayoutPattern newPattern = (SlideLayoutPattern)newPatternId;
			changedLayoutInSameFamily = tourPage.ActiveSlideLayout.Family == SlideLayout.GetFamily(newPattern);
			changedLayoutInDifferentFamily = !changedLayoutInSameFamily;

			// Determine the splitter positions and lock status for the new layout.
			int splitterH;
			int splitterV;
			bool lockedH;
			bool lockedV;

            if (tour.V3CompatibilityEnabled)
            {
                if (changedLayoutInSameFamily)
                {
                    // The new layout is in the same group as the old. Translate the splitters to the new layout.
                    SlideLayout.TranslateSplitters(tourPage.Tour, slideLayout, newPattern, out splitterH, out splitterV);
                    lockedH = slideLayout.Splitters.LockedH;
                    lockedV = slideLayout.Splitters.LockedV;
                }
                else
                {
                    // Unlock both splitters whenever the layout group changes since the
                    // meaning of splitters is different for different groups.
                    SlideLayout.TranslateSplitters(tourPage.Tour, slideLayout, newPattern, out splitterH, out splitterV);
                    splitterH = slideLayout.Splitters.H;
				    splitterV = slideLayout.Splitters.V;
				    lockedH = false;
				    lockedV = false;
			    }
            }
            else
            {
                SlideLayout.TranslateSplitters(tourPage.Tour, slideLayout, newPattern, out splitterH, out splitterV);
                lockedH = false;
                lockedV = false;
            }

            // Change the current layout's type to the new type.
            slideLayout.Pattern = newPattern;

			// Set up the splitters and thus the map, image, and text areas for the new layout type.
			SlideLayoutSplitters newSplitters = new SlideLayoutSplitters(splitterH, splitterV, lockedH, lockedV);
			tourPage.LayoutManager.SplittersChanged(newSplitters);

			tourPage.SetLayoutChanged();
		}
	}

	public void HandleQueryOptions(NameValueCollection queryString)
	{
		bool changedLayoutInSameFamily;
		bool changedLayoutInDifferentFamily;
		
		HandleNewTemplateChoiceRequest(queryString, out changedLayoutInSameFamily, out changedLayoutInDifferentFamily);

        if (tour.V3CompatibilityEnabled)
        {
		    HandleAutoLayoutRequest(queryString, changedLayoutInSameFamily, changedLayoutInDifferentFamily);
		    HandleRestoreLayoutRequest(queryString);
		    HandleDisableAutoLayoutRequest(queryString);
        }
	}

	private void HandleRestoreLayoutRequest(NameValueCollection queryString)
	{
		if (queryString["restore"] != "1")
			return;

		// Restore the current page.
		tourPage.RestoreLayout();

		// Restore the other pages by running auto layout on them.
		tourPage.LayoutManager.PerformAutoLayoutOnOtherPages(tourPage);
	}

	public static void HandleTogglePopupRequest(TourPage tourPage)
	{
		Tour tour = tourPage.Tour;

		tourPage.SlidesPopup = !tourPage.SlidesPopup;

		// The popup setting has changed. Save off the current map and image area sizes.
		Size oldScaledMapSize = tourPage.ScaledMapSize;
		Size oldImageAreaSize = tourPage.ActiveSlideLayout.ImageArea.Size;

		// Run auto layout.
		AutoLayoutOptions autoLayoutOptions = new AutoLayoutOptions();
		autoLayoutOptions.WidthType = tour.HasMoreThanOnePage ? TourSizeType.Exact : tour.WidthType;
		autoLayoutOptions.HeightType = tour.HasMoreThanOnePage ? TourSizeType.Exact : tour.HeightType;

		// Let auto layout choose the best layout when switching between popup/fixed the first time.
		// We know it's the first time if the layout being switched to has negative splitter values.
		bool firstTimeSwitchingToFixed = !tourPage.SlidesPopup && tourPage.LayoutAreaSlideLayout.Splitters.H == -1 && tourPage.LayoutAreaSlideLayout.Splitters.V == -1;
		bool firstTimeSwitchingToPopup = tourPage.SlidesPopup && tourPage.PopupSlideLayout.Splitters.H == -1 && tourPage.PopupSlideLayout.Splitters.V == -1;

		if (firstTimeSwitchingToFixed || firstTimeSwitchingToPopup)
		{
			autoLayoutOptions.OkToChooseDifferentLayout = true;
		}
		else if (!tourPage.SlidesPopup)
		{
			// When switching from popup to tiled layout, verify that the splitters are valid.
			// If the user had made the tour smaller while using the popup layout, the tiled
			// splitters could be out of range. In that case clear the locks so that auto layout
			// can choose new splitter locations. If we don't do this, autolayout will move the
			// splitters to minimum positions and they will be locked there. Note that we don't
			// just blindly unlock the splitters because if we did, then if you had adjusted the
			// splitters for a tiled layout, then switched to popups and back to tiled, you would
			// lose the adjustment.
			SlideLayout slideLayout = tourPage.ActiveSlideLayout;
			if (slideLayout.SplitterAreaWidth > slideLayout.InnerSize.Width - SlideLayoutAreas.MinSplitter ||
				slideLayout.SplitterAreaHeight > slideLayout.InnerSize.Height - SlideLayoutAreas.MinSplitter)
			{
				tourPage.UnlockSplitters();
			}
		}

		tourPage.LayoutManager.PerformAutoLayout(autoLayoutOptions);

		// Set popup options that are best suited for Ready Maps.
		if (tourPage.MapImage.IsReadyMap && tourPage.SlidesPopup)
			tourPage.PopupOptions.SetReadyMapDefaults();
	}
}
