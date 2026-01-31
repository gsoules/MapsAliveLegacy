// Copyright (C) 2003-2010 AvantLogic Corporation

public partial class Members_TemplateChoices : MemberPage
{
	bool multiPageTour;
	bool restrictPageWidth;
	bool restrictPageHeight;
	bool warnAboutTourSizeChanging;

	protected override void InitControls(bool undo)
	{
		// Show only the popup templates (photo and/or text, but no map) if the page
		// uses popups or if it is an info page (since info pages have no map).
		bool slidesPopup = tourPage.SlidesPopup || tourPage.IsDataSheet;
		SlideThumbs.TourPage = tourPage;
		SlideThumbs.SlidesPopup = slidesPopup;

		// Show the non-map templates when the Slide Names in Menu option or the Slide Drop
		// Down List options are enabled so that a user can create a page that has slides,
		// but has no map.  With these options a map can be used, but is not necessary because
		// you can see the slide by clicking it's name in the menu or choosing it from the
		// list.  Also, since a user could have enabled on of these option and chosen a non-map
		// template and then disabled the option, we need to make sure they can still see the
		// non-map template when they come to this templates page.
		SlideThumbs.ShowNonMapLayouts =
			tourPage.ShowSlideNamesInMenu ||
			tourPage.ShowSlideList ||
			!tourPage.ActiveSlideLayout.HasMapArea;

		SlideThumbs.PreviewType = tourPage.SlidesPopup ? PreviewType.SlideLayout : PreviewType.SlideLayoutInPage;

		ShowChoiceOptions();

		SlideThumbs.TourWidthLocked = restrictPageWidth;
		SlideThumbs.TourHeightLocked = restrictPageHeight;
	}

	protected override void PageLoad()
	{
		Utility.RegisterLayoutEditorJavaScript(this);

		SetMasterPage(Master);
		GetSelectedTourPage();
		SetPageReadOnly();
		SetActionIdForPageAction(tourPage.SlidesPopup ? MemberPageActionId.TemplateChoicesForPopup : MemberPageActionId.TemplateChoicesForLayoutArea);
		SetPageTitle(string.Format("{0} Template Choices", tourPage.SlidesPopup ? "Popup" : ""));

		multiPageTour = tour.TourPages.Count > 1;

		if (IsPostBack)
        {
            GetPatternIdOfSelectedTemplate();
            return;
        }

        HandleQueryOptions();
	}

    private void GetPatternIdOfSelectedTemplate()
    {
        // Retrieve the pattern Id from the hidden PatternId field.
        int patternId;
        int.TryParse(PatternId.Value, out patternId);

        // Switch to the new template and reset both splitters to 50%
        if (patternId > 0)
        {
            int splitterH;
            int splitterV;
            SlideLayout.TranslateSplitters(tourPage.Tour, tourPage.ActiveSlideLayout, (SlideLayoutPattern)patternId, out splitterH, out splitterV);
            tourPage.ActiveSlideLayout.Pattern = (SlideLayoutPattern)patternId;
            SlideLayoutSplitters newSplitters = new SlideLayoutSplitters(splitterH, splitterV, false, false);
            tourPage.LayoutManager.SplittersChanged(newSplitters);
            tourPage.SetLayoutChanged();
            tourPage.UpdateDatabase();
            tourPage.RebuildMap();
        }
    }

    private void HandleQueryOptions()
	{
		if (Request.QueryString["restrict"] == "0")
		{
			restrictPageWidth = false;
			restrictPageHeight = false;
			warnAboutTourSizeChanging = true;
		}
		else
		{
			restrictPageWidth = multiPageTour || tour.TourWidthLocked;
			restrictPageHeight = multiPageTour || tour.TourHeightLocked;
		}

		if (tour.HasBanner)
			restrictPageWidth = true;
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}

    private void ShowChoiceOptions()
	{
		if (tourPage.SlidesPopup)
			return;

		bool bothDimensionsLocked = multiPageTour || (restrictPageWidth && restrictPageHeight);
		bool oneDimensionLocked = !bothDimensionsLocked && (restrictPageWidth || restrictPageHeight);

		if (oneDimensionLocked || bothDimensionsLocked || warnAboutTourSizeChanging)
		{
			bool restrictTourSize = restrictPageWidth || restrictPageHeight;

			OptionsPanel.Visible = true;

			if (tour.HasBanner || tour.V4)
			{
				// Hide the option to show other sizes since Auto Layout requires the width be fixed when there is a banner.
				// This rules exists because the logic to dynamically size both the banner and the map and image areas is
				// too complex to support for now. This option is no longer supported in V4.
				ToggleTemplateChoices.Visible = false;
			}
			else
			{
				ToggleTemplateChoices.OnClickActionId = MemberPageActionId.TemplateChoicesForLayoutArea;

				// Set the restrict option to the opposite of what it is now.
				ToggleTemplateChoices.QueryString = string.Format("?restrict={0}", restrictTourSize ? "0" : "1");

				// Show the message for the opposite slideLayout we are in now.
				string title;
				if (restrictTourSize)
				{
					title = "Show templates at other sizes that might work well for this tour";
				}
				else
				{
					title = "Only show templates for the current tour's web page ";
					if (multiPageTour || (tour.TourWidthLocked && tour.TourHeightLocked))
						title += "size.";
					else if (tour.TourWidthLocked)
						title += "width.";
					else
						title += "height.";
				}
				ToggleTemplateChoices.Title = title;
			}

			string text = string.Empty;
			int w = tour.TourSize.Width;
			int h = tour.TourSize.Height;

			if (warnAboutTourSizeChanging)
			{
				string warning = "Change template?\\n\\nThis tour will get the new template\\'s size.";
				SlideThumbs.Warning = warning;
				text = string.Format("The size of some templates below is different than the current tour's size of {0} x {1}.", w, h);
				if (multiPageTour)
					text += "<span style='color:red'><br/>Choosing a different sized template will change this tour's size.</span>";
			}
			else if (bothDimensionsLocked)
			{
				if (tour.TourPages.Count == 1)
					text = "This tour's";
				else
					text = string.Format("This tour has {0} maps and its", tour.TourPages.Count);

				text += string.Format(" tour size is {0} x {1}. Every template below is this size.", w, h, tour.TourPages.Count);
			}
			else if (oneDimensionLocked)
			{
				if (restrictPageWidth)
					text = string.Format("This tour has its width set to {0}. Every template below has this width.", w);
				else
					text = string.Format("This tour has its height set to {0}. Every template below has this height.", h);
			}
			OptionsPanelText.Text = text;
		}
	}
}
