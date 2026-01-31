// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Members_TemplateSplitters : MemberPage
{
	protected bool autolayoutChanged;
	private SplitterEdgeH splitterEdgeH;
	private SplitterEdgeV splitterEdgeV;
	private int validAutoWidth;
	private int validAutoHeight;

	protected void AutoSaveOnChange(CheckBox checkBox)
	{
		MemberPageActionId actionId = tourPage.SlidesPopup ? MemberPageActionId.TemplateSplittersForPopup : MemberPageActionId.TemplateSplittersForLayoutArea;
		string script = string.Format("maChangeDetected();maOnEventSaveAndTransfer('/Members/{0}?adjust=1');", MemberPageAction.ActionPageTarget(actionId));
		checkBox.Attributes.Add("onclick", script);
	}

	protected override void EmitJavaScript()
	{
		SlideLayout slideLayout = tourPage.ActiveSlideLayout;

		string loadingScript =
			AssignClientVar("viewWidth", tourPage.SlidesPopup ? tourPage.PopupSlideLayout.OuterSize.Width : tour.LayoutAreaSize.Width) +
			AssignClientVar("viewHeight", tourPage.SlidesPopup ? tourPage.PopupSlideLayout.OuterSize.Height : tour.LayoutAreaSize.Height) +
			AssignClientVar("fullWidth", tourPage.SlidesPopup ? tourPage.PopupSlideLayout.OuterSize.Width : tour.TourSize.Width) +
			AssignClientVar("fullHeight", tourPage.SlidesPopup ? tourPage.PopupSlideLayout.OuterSize.Height : tour.TourSize.Height) +
			AssignClientVar("meaningH", slideLayout.SplitterMeaning(splitterEdgeH)) +
			AssignClientVar("meaningV", slideLayout.SplitterMeaning(splitterEdgeV)) +
			AssignClientVar("invertH", slideLayout.IsInvertedSplitterH ? 1 : 0) +
			AssignClientVar("invertV", slideLayout.IsInvertedSplitterV ? 1 : 0) +
			AssignClientVar("viewSpacingH", slideLayout.Spacing.H) +
			AssignClientVar("viewSpacingV", slideLayout.Spacing.V) +
			AssignClientVar("viewMarginsH", slideLayout.Margin.Top + slideLayout.Margin.Bottom) +
			AssignClientVar("viewMarginsV", slideLayout.Margin.Left + slideLayout.Margin.Right);

		string loadedScript = "initSlider();";
		
		EmitJavaScript(loadingScript, loadedScript);
	}

	protected override void InitControls(bool undo)
	{
		bool needsMap = !tourPage.IsDataSheet && !tourPage.MapImage.HasFile;
		SlideLayout slideLayout = tourPage.ActiveSlideLayout;

		splitterEdgeH = slideLayout.SplitterEdgeH;
		splitterEdgeV = slideLayout.SplitterEdgeV;
		
		RenderPageControls();
		InitSplitterLocks(slideLayout);
		InitSliderValues(slideLayout);
		InitAutoLayoutControls();
		RenderPreviewImage(slideLayout);
		RenderLayoutEditor(slideLayout);

        if (tour.V3CompatibilityEnabled)
            OptionsPanel.Visible = true;
        else
            InfoPanel.Visible = true;
	}

	private void InitAutoLayoutControls()
	{
		SlideLayout slideLayout = tourPage.ActiveSlideLayout;

		string minAutoWidthMeaning = slideLayout.MinNonMapWidthMeaning;
		string minAutoHeightMeaning = slideLayout.MinNonMapHeightMeaning;
		if (minAutoWidthMeaning == string.Empty && minAutoHeightMeaning == string.Empty)
		{
			AutoNoPanel.Visible = true;
		}
		else
		{
			AutoYesPanel.Visible = true;

			if (minAutoWidthMeaning != string.Empty)
			{
				AutoWidthMeaning.Text = minAutoWidthMeaning;
				AutoWidth.Text = tourPage.LayoutMinNonMapWidth.ToString();
				AddChangeDetection(AutoWidth);
			}
			else
			{
				AutoWidthRow.Style.Add(HtmlTextWriterStyle.Display, "none");
			}

			if (minAutoHeightMeaning != string.Empty)
			{
				AutoHeightMeaning.Text = minAutoHeightMeaning;
				AutoHeight.Text = tourPage.LayoutMinNonMapHeight.ToString();
				AddChangeDetection(AutoHeight);
			}
			else
			{
				AutoHeightRow.Style.Add(HtmlTextWriterStyle.Display, "none");
			}
		}
	}

	protected override void PageLoad()
	{
		Utility.RegisterLayoutEditorJavaScript(this);

		SetMasterPage(Master);
		GetSelectedTourPage();

		if (!IsPostBack && !IsReturnToTourBuilder)
		{
			AutoLayout autoLayout = new AutoLayout(tourPage);
			autoLayout.HandleQueryOptions(Request.QueryString);

			// Remember the original layout to compare against on post back to see if Undo is allowed.
			// If we got here following an adjustment, the layout was just modified and is not the original.
			if (Request.QueryString["adjust"] == null)
				tourPage.AcceptLayoutChanges();
		}

		SetPageTitle(tourPage.SlidesPopup ? Resources.Text.PreviewPopupLayoutPageTitle : Resources.Text.PreviewFixedPageLayoutPageTitle);
		SetActionIdForPageAction(tourPage.SlidesPopup ? MemberPageActionId.TemplateSplittersForPopup : MemberPageActionId.TemplateSplittersForLayoutArea);
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}

	protected override void PerformUpdate()
	{
		tourPage.UpdateDatabase();

		if (autolayoutChanged)
			tourPage.LayoutManager.PerformAutoLayoutForCurrentPage();
    
        tourPage.RebuildMap();
    }

    protected override void ReadPageFields()
	{
		int splitterH = 0;
		int.TryParse(TopHeight.Value, out splitterH);
		
		int splitterV = 0;
		int.TryParse(LeftWidth.Value, out splitterV);

        // V4 treats the splitters as always being locked so that auto layout won't move them.
        bool lockedH = tour.V3CompatibilityEnabled ? SplitterLockedCheckBoxH.Checked : true;
        bool lockedV = tour.V3CompatibilityEnabled ? SplitterLockedCheckBoxV.Checked : true;
        SlideLayoutSplitters splitters = new SlideLayoutSplitters(splitterH, splitterV, lockedH, lockedV);
		tourPage.LayoutManager.SplittersChanged(splitters);

		ReadAutoFields();
	}

	protected void ReadAutoFields()
	{
		if (validAutoWidth != tourPage.LayoutMinNonMapWidth || validAutoHeight != tourPage.LayoutMinNonMapHeight)
		{
			autolayoutChanged = true;
			tourPage.LayoutMinNonMapWidth = validAutoWidth;
			tourPage.LayoutMinNonMapHeight = validAutoHeight;
		}
	}

	protected override void ChangePageMode(string mode)
	{
		// This method gets called when the user moves one of the layout editor sliders.
		Save();
		tourPage.LayoutManager.PerformAutoLayoutForCurrentPage();
	}

	private void CreateInstructionsMessage(SlideLayout slideLayout)
	{
		string instructions = string.Empty;
		if (splitterEdgeH != SplitterEdgeH.None)
			instructions += "<img src='../Images/ColorSwatchYellow.gif'/> <b>{0}</b> can be changed by dragging the yellow slider up/down.&nbsp;&nbsp;<br/>";

		if (splitterEdgeV != SplitterEdgeV.None)
			instructions += "<img src='../Images/ColorSwatchOrange.gif'/> <b>{1}</b> can be changed by dragging the orange slider left/right.&nbsp;&nbsp;";

		if (instructions == string.Empty)
			LayoutPreviewInstructionsPanel.Visible = false;
		else
			LayoutPreviewInstructions.Text = string.Format(instructions, slideLayout.SplitterMeaning(splitterEdgeH), slideLayout.SplitterMeaning(splitterEdgeV));
	}

	private void InitSliderValues(SlideLayout slideLayout)
	{
		LeftWidth.Value = slideLayout.Splitters.V.ToString();
		TopHeight.Value = slideLayout.Splitters.H.ToString();
		SlideWidth.Value = slideLayout.OuterSize.Width.ToString();
		SlideHeight.Value = slideLayout.OuterSize.Height.ToString();
	}

	private void InitSplitterLocks(SlideLayout slideLayout)
	{
		if (splitterEdgeH == SplitterEdgeH.None && splitterEdgeV == SplitterEdgeV.None)
		{
			SplitterLocksPanel.Visible = false;
		}
		else
		{
			if (splitterEdgeH == SplitterEdgeH.None)
			{
				SplitterLockedCheckBoxH.Visible = false;
				SplitterLockedLabelH.Visible = false;
			}
			else
			{
				if (tour.V3CompatibilityEnabled)
                {
				    if (tour.AutoLayoutEnabled)
					    AutoSaveOnChange(SplitterLockedCheckBoxH);
				    else
					    SplitterLockedCheckBoxH.Enabled = false;

                    string colorStyle = slideLayout.Splitters.LockedH ? "color:red;" : string.Empty;
				    SplitterLockedCheckBoxH.Checked = slideLayout.Splitters.LockedH;
				    string h = string.Format(" to <span style='{0};font-weight:normal;'>{1}px ({2}%)</span>", colorStyle, slideLayout.SplitterAreaHeight, slideLayout.SplitterAreaHeightPercent);
				    SplitterLockedLabelH.Text = string.Format("Set {0}{1}", slideLayout.SplitterMeaning(splitterEdgeH), h);
                }
                else
                {
                    string h = string.Format(" is set to <span style='font-weight:bold;'>{0}%</span>", slideLayout.SplitterAreaHeightPercent);
                    SplitterInfoH.Text = string.Format("{0}{1}", slideLayout.SplitterMeaning(splitterEdgeH), h);
                }
			}

			if (splitterEdgeV == SplitterEdgeV.None)
			{
				SplitterLockedCheckBoxV.Visible = false;
				SplitterLockedLabelV.Visible = false;
			}
			else
			{
                if (tour.V3CompatibilityEnabled)
                {
                    if (tour.AutoLayoutEnabled)
                        AutoSaveOnChange(SplitterLockedCheckBoxV);
                    else
                        SplitterLockedCheckBoxV.Enabled = false;

                    string colorStyle = slideLayout.Splitters.LockedV ? "color:red;" : string.Empty;
                    SplitterLockedCheckBoxV.Checked = slideLayout.Splitters.LockedV;
                    string v = string.Format(" to <span style='{0}font-weight:normal;;'>{1}px ({2}%)</span>", colorStyle, slideLayout.SplitterAreaWidth, slideLayout.SplitterAreaWidthPercent);
                    SplitterLockedLabelV.Text = string.Format("Set {0}{1}", slideLayout.SplitterMeaning(splitterEdgeV), v);
                }
                else
                {
                    double percent = slideLayout.SplitterAreaWidthPercent;
                    if (slideLayout.Pattern == SlideLayoutPattern.HMMTI || slideLayout.Pattern == SlideLayoutPattern.VTTII)
                        percent = 100 - percent;
                    string v = string.Format(" is set to <span style='font-weight:bold;'>{0}%</span>", percent);
                    SplitterInfoV.Text = string.Format("{0}{1}", slideLayout.SplitterMeaning(splitterEdgeV), v);
                }
            }
		}
	}

	private void RenderLayoutEditor(SlideLayout slideLayout)
	{
		bool showVerticalSlider = slideLayout.HasHorizontalSplitter;
		bool showHorizontalSlider = slideLayout.HasVerticalSplitter;
		bool showNone = !(showHorizontalSlider || showVerticalSlider);

		int left = showNone ? 3 : 24;
		int top = showNone ? 4 : 22;

		int pageOptionsOffsetLeft = 0;
		int pageOptionsOffsetTop = 0;
		if (!tourPage.SlidesPopup)
		{
			// Add 1 for the border that goes around the preview image.
			pageOptionsOffsetLeft = TourLayout.CalculateHeightOfTourOptionsLeft(tour) + 1;
			pageOptionsOffsetTop = TourLayout.CalculateHeightOfTourOptionsUpper(tour, tour.TourSize.Width) + 1;
		}

		LayoutPreviewImagePanel.Style.Add(HtmlTextWriterStyle.Left, left + "px");
		LayoutPreviewImagePanel.Style.Add(HtmlTextWriterStyle.Top, top + "px");
		
		LayoutPreviewPanel.Visible = true;

		CreateInstructionsMessage(slideLayout);

		string displayMode;

		// Set the height of the overall control (sliders and the preview image they operate on).
		int w = tourPage.SlidesPopup ? slideLayout.OuterSize.Width : tour.TourSize.Width; 
		if (showHorizontalSlider)
			w += 26;
		int h = tourPage.SlidesPopup ? slideLayout.OuterSize.Height : tour.TourSize.Height;
		if (showVerticalSlider || showHorizontalSlider)
			h += 26;

		SliderControl.Style.Add(HtmlTextWriterStyle.Width, w + "px");
		SliderControl.Style.Add(HtmlTextWriterStyle.Height, h + "px");

		// Determine how big of a layout area we are going to show.
		int innerWidth = slideLayout.InnerSize.Width;
		int	innerHeight = slideLayout.InnerSize.Height;
		
		int leftOffset = slideLayout.Margin.Left + left + pageOptionsOffsetLeft;
		int topOffset = slideLayout.Margin.Top + top + pageOptionsOffsetTop;
		
		// The user moves the vertical splitter with the horizontal slider.
		// Set the width  and left offset of the horizontal slider track.
		SliderHorizontalTrack.Style.Add(HtmlTextWriterStyle.Width, innerWidth + "px");
		SliderHorizontalTrack.Style.Add(HtmlTextWriterStyle.Left, leftOffset + "px");

		// Position the left and right nudger arrows.
		SliderHorizontalNudgeLeft.Style.Add(HtmlTextWriterStyle.Left, 14 + slideLayout.Margin.Left + pageOptionsOffsetLeft + "px");
		SliderHorizontalNudgeRight.Style.Add(HtmlTextWriterStyle.Left, leftOffset + innerWidth + 0 + "px");

		// Position the horizontal slider at the location of the vertical splitter.
		SliderHorizontal.Style.Add(HtmlTextWriterStyle.Left, slideLayout.Splitters.V + "px");
		int sliderWidth = slideLayout.Spacing.V >= 6 ? slideLayout.Spacing.V - 2 : 4;
		SliderHorizontal.Style.Add(HtmlTextWriterStyle.Width, sliderWidth + "px");

		// Hide or show the horizontal slider and nudgers.
		if (showHorizontalSlider)
			SliderControl.Style.Add(HtmlTextWriterStyle.MarginTop, "12px");
		displayMode = showHorizontalSlider ? "block" : "none";
		SliderHorizontalTrack.Style.Add(HtmlTextWriterStyle.Display, displayMode);
		SliderHorizontalNudgeLeft.Style.Add(HtmlTextWriterStyle.Display, displayMode);
		SliderHorizontalNudgeRight.Style.Add(HtmlTextWriterStyle.Display, displayMode);

		// The user moves the horizontal splitter with the vertical slider.
		// Set the height and top offset of the vertical slider track.
		SliderVerticalTrack.Style.Add(HtmlTextWriterStyle.Height, innerHeight + "px");
		SliderVerticalTrack.Style.Add(HtmlTextWriterStyle.Top, topOffset + "px");

		// Position the up and down nudger arrows.
		SliderVerticalNudgeUp.Style.Add(HtmlTextWriterStyle.Top, 12 + slideLayout.Margin.Top + pageOptionsOffsetTop + "px");
		SliderVerticalNudgeDown.Style.Add(HtmlTextWriterStyle.Top, topOffset + innerHeight + 0 + "px");

		// Position the vertical slider at the location of the horizontal splitter.
		SliderVertical.Style.Add(HtmlTextWriterStyle.Top, slideLayout.Splitters.H + "px");
		int sliderHeight = slideLayout.Spacing.H >= 6 ? slideLayout.Spacing.H - 2 : 4;
		SliderVertical.Style.Add(HtmlTextWriterStyle.Height, sliderHeight + "px");

		// Hide the vertical slider and nudgers.
		displayMode = showVerticalSlider ? "block" : "none";
		SliderVerticalTrack.Style.Add(HtmlTextWriterStyle.Display, displayMode);
		SliderVerticalNudgeUp.Style.Add(HtmlTextWriterStyle.Display, displayMode);
		SliderVerticalNudgeDown.Style.Add(HtmlTextWriterStyle.Display, displayMode);
	}

	private void RenderPageControls()
	{
		MemberPageActionId layoutPreviewActionId = tourPage.SlidesPopup ? MemberPageActionId.TemplateSplittersForPopup : MemberPageActionId.TemplateSplittersForLayoutArea;

		RestoreLayoutControl.Title = "Undo Layout Changes";
		RestoreLayoutControl.OnClickActionId = layoutPreviewActionId;
		RestoreLayoutControl.QueryString = "?restore=1";
		bool layoutChanged = tourPage.LayoutChanged;
		RestoreLayoutControl.AppearsEnabled = layoutChanged;
		RestoreLayoutControl.VeryImportant = layoutChanged;
		RestoreLayoutControl.WarningMessage = "Undo changes?";
	}

	private void RenderPreviewImage(SlideLayout slideLayout)
	{
        // Show the dimensions of each area in the slide.
        int drawDimensions = tour.V3CompatibilityEnabled ? 1 : 0;

        // Draw the image at actual size.
        int thumbnailDimension = 0;
		
		LayoutPreviewImage.ImageUrl = string.Format("PageRenderer.ashx?args={0},{1},{2},{3},{4},{5},{6},{7},{8}&v={9}",
			(int)PreviewType.SlideLayout,
			thumbnailDimension,
			tourPage.Id,
			drawDimensions,
			(int)slideLayout.Pattern,
			slideLayout.Splitters.H,
			slideLayout.Splitters.V,
			slideLayout.OuterSize.Width,
			slideLayout.OuterSize.Height,
			DateTime.Now.Ticks
		);
	}

    protected string StyleDefinitions
    {
        get
        {
            // Create styles that will be inserted into the Splitters pages after the <script> tags in order to make the page width
            // accommodate tour image preview width plus the additional space on the left and right for the splitter controls.

            int maxWidth = Math.Max(TourBuilderPageWidth, tour.TourSize.Width + TourBuilderPageContentLeftWidth);
            maxWidth += 32;
            
            string definitions = "<style type=\"text/css\">";

            // Note that doubled up curly braces below are to escape a single curly brace ub lines that use string.Format().
            definitions += string.Format("body{{max-width:{0}px;}}", maxWidth);
            definitions += string.Format(".memberPageControlsHeader{{width:{0}px;}}", maxWidth);
            definitions += string.Format(".memberPageControlsMenuBackground{{width:{0}px;}}", maxWidth);
            definitions += string.Format(".memberPageContentRight{{width:{0}px;}}", Math.Max(maxWidth - TourBuilderPageContentLeftWidth, TourBuilderPageContentRightWidth));

            definitions += "</style>";

            return definitions;
        }
    }

    protected override void Undo()
	{
		ClearErrors();
	}

	protected override void ValidatePage()
	{
		ClearErrors();

		if (!ValidateAuto())
			return;
	}

	protected bool ValidateAuto()
	{
		int maxW = 1024;

		if (tourPage.ActiveSlideLayout.MinNonMapWidthMeaning != string.Empty)
			validAutoWidth = ValidateFieldInRange(AutoWidth, 36, maxW, AutoWidthError);
		else
			validAutoWidth = tourPage.LayoutMinNonMapWidth;

		if (tourPage.ActiveSlideLayout.MinNonMapHeightMeaning != string.Empty)
			validAutoHeight = ValidateFieldInRange(AutoHeight, 36, maxW, AutoHeightError);
		else
			validAutoHeight = tourPage.LayoutMinNonMapHeight;

		return pageValid;
	}

	private void ClearErrors()
	{
		ClearErrors(
			AutoWidthError,
			AutoHeightError);
	}
}
