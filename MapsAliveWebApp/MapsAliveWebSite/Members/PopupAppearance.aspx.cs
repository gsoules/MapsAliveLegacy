// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Members_PopupAppearance : LayoutPage
{
	private Size newPopupSize;
	private bool popupMaxSizeChanged;
	private string validPopupSlideBackgroundColor;
	private string validPopupSlideBorderColor;
	private int validPopupSlideWidth;
	private int validPopupSlideHeight;
	private int validPopupSlideMinWidth;
	private int validPopupSlideMinHeight;
	private string validPopupSlideTextColor;
	private int validPopupTextOnlyWidth;
	private string validPopupSlideTitleTextColor;
	private int validPopupSlideBorderWidth;
	private int validPopupCornerRadius;
	private int validImageCornerRadius;
	private int validDropShadowDistance;

	protected override void InitControls(bool undo)
	{
		InitPreviewControls();
		
		if (!undo && IsPostBack)
			return;

		InitMarginsAndSpacingControls();
		InitPopupSizeControls();
		InitTextOnlyField();
		InitAppearanceControls();
	}

	private void InitTextOnlyField()
	{
		if (tour.V3CompatibilityEnabled)
        {
            PopupTextOnlyWidthTextBox.Text = tourPage.PopupOptions.TextOnlyWidth.ToString();
		    AddChangeDetection(PopupTextOnlyWidthTextBox);
        }
        else
        {
            PopupTextOnlyPanel.Visible = false;
        }
	}

	private void InitAppearanceControls()
	{
		PopupSlideBackgroundColorSwatch.ColorValue = tourPage.PopupOptions.BackgroundColor;
		PopupSlideBorderColorSwatch.ColorValue = tourPage.PopupOptions.BorderColor;
		PopupSlideTitleTextColorSwatch.ColorValue = tourPage.PopupOptions.TitleTextColor;
		PopupSlideTextColorSwatch.ColorValue = tourPage.PopupOptions.TextColor;

		UseColorSchemeColorsCheckBox.Checked = tourPage.PopupOptions.UseColorSchemeColors;
		AddChangeDetectionForPreview(UseColorSchemeColorsCheckBox);

		PopupSlideBorderWidth.Text = tourPage.PopupOptions.BorderWidth.ToString();
		AddChangeDetectionForPreview(PopupSlideBorderWidth);

		PopupCornerRadius.Text = tourPage.PopupOptions.PopupCornerRadius.ToString();
		AddChangeDetectionForPreview(PopupCornerRadius);

		ImageCornerRadius.Text = tourPage.PopupOptions.ImageCornerRadius.ToString();
		AddChangeDetectionForPreview(ImageCornerRadius);

		DropShadowDistance.Text = tourPage.PopupOptions.DropShadowDistance.ToString();
		AddChangeDetectionForPreview(DropShadowDistance);
	}

	private void InitPreviewControls()
	{
		SlideLayout slideLayout = tourPage.ActiveSlideLayout;

        // Show the dimensions of each area in the slide.
        int drawDimensions = tour.V3CompatibilityEnabled ? 1 : 0;

        // Draw the image to fit the available width.
        int thumbnailDimension = Math.Min(tour.TourSize.Width, TourBuilderPageContentRightWidth);

        TemplatePreviewImage.ImageUrl = string.Format("PageRenderer.ashx?args={0},{1},{2},{3},{4},{5},{6},{7},{8}&v={9}",
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

	private void InitPopupSizeControls()
	{
		PopupSlideWidth.Text = tourPage.PopupSlideLayout.OuterSize.Width.ToString();
		AddChangeDetectionForPreview(PopupSlideWidth);

		PopupSlideHeight.Text = tourPage.PopupSlideLayout.OuterSize.Height.ToString();
		AddChangeDetectionForPreview(PopupSlideHeight);

		PopupSlideMinWidth.Text = tourPage.PopupOptions.MinSize.Width.ToString();
		AddChangeDetectionForPreview(PopupSlideMinWidth);

		PopupSlideMinHeight.Text = tourPage.PopupOptions.MinSize.Height.ToString();
		AddChangeDetectionForPreview(PopupSlideMinHeight);
	}

	protected override void PageLoad()
	{
		Utility.RegisterColorChooserJavaScript(this);

		SetMasterPage(Master);
		SetPageTitle("Popup Size and Appearance");
		SetActionIdForPageAction(MemberPageActionId.PopupAppearance);
		GetSelectedTourPage();
		
		InitMarginsAndSpacingForPopup(MarginsAndSpacingControl);
		ShowSpacingPanelOnRight();
	}

	protected override void PerformUpdate()
	{
		tourPage.UpdateDatabase();

		if (popupMaxSizeChanged)
		{
			tourPage.LayoutManager.PopupSizeChanged(newPopupSize);
			tourPage.LayoutManager.PerformAutoLayoutForCurrentPage();
		}

		tourPage.RebuildMap();
	}

	protected override void ReadPageFields()
	{
		ReadAppearanceFields();
		ReadPopupSizeFields();
		ReadTextOnlyField();
		ReadMarginFields();
		ReadSpacingFields();
	}

	private void ReadTextOnlyField()
	{
        if (tour.V4)
            return;

        PopupOptions popupOptions = tourPage.PopupOptions;
		
		if (validPopupTextOnlyWidth != popupOptions.TextOnlyWidth)
			popupOptions.TextOnlyWidth = validPopupTextOnlyWidth;
	}

	private void ReadPopupSizeFields()
	{
		newPopupSize = new Size(validPopupSlideWidth, validPopupSlideHeight);
		popupMaxSizeChanged = newPopupSize != tourPage.PopupSlideLayout.OuterSize;

		if (popupMaxSizeChanged)
        {
            tourPage.PopupSlideLayout.SetNewOuterSize(newPopupSize);
            
            // Reset the splitters to be in the middle to ensure they are in valid positions for the new popup size.
            int splitterH;
            int splitterV;
            SlideLayout.TranslateSplitters(tourPage.Tour, tourPage.PopupSlideLayout, tourPage.PopupSlideLayout.Pattern, out splitterH, out splitterV);
            SlideLayoutSplitters newSplitters = new SlideLayoutSplitters(splitterH, splitterV, false, false);
            tourPage.LayoutManager.SplittersChanged(newSplitters);
            tourPage.SetLayoutChanged();
        }

        PopupOptions popupOptions = tourPage.PopupOptions;
		Size newPopupMinSize = new Size(validPopupSlideMinWidth, validPopupSlideMinHeight);
		if (newPopupMinSize != popupOptions.MinSize)
			popupOptions.MinSize = newPopupMinSize;
	}

	private void ReadAppearanceFields()
	{
		PopupOptions popupOptions = tourPage.PopupOptions;

		if (popupOptions.BorderWidth != validPopupSlideBorderWidth)
			popupOptions.BorderWidth = validPopupSlideBorderWidth;

		if (popupOptions.PopupCornerRadius != validPopupCornerRadius)
			popupOptions.PopupCornerRadius = validPopupCornerRadius;

		if (popupOptions.ImageCornerRadius != validImageCornerRadius)
			popupOptions.ImageCornerRadius = validImageCornerRadius;

		if (popupOptions.DropShadowDistance != validDropShadowDistance)
			popupOptions.DropShadowDistance = validDropShadowDistance;

		if (popupOptions.BorderColor != validPopupSlideBorderColor)
			popupOptions.BorderColor = validPopupSlideBorderColor;

		if (popupOptions.TitleTextColor != validPopupSlideTitleTextColor)
			popupOptions.TitleTextColor = validPopupSlideTitleTextColor;

		if (popupOptions.TextColor != validPopupSlideTextColor)
			popupOptions.TextColor = validPopupSlideTextColor;

		if (popupOptions.UseColorSchemeColors != UseColorSchemeColorsCheckBox.Checked)
			popupOptions.UseColorSchemeColors = UseColorSchemeColorsCheckBox.Checked;

		if (popupOptions.BackgroundColor != validPopupSlideBackgroundColor)
			popupOptions.BackgroundColor = validPopupSlideBackgroundColor;
	}

	protected override void Undo()
	{
		ClearErrors();
	}

	protected override void ValidatePage()
	{
		ClearErrors();

		if (!ValidateAppearanceFields())
			return;

		if (!ValidatePopupSizeFields())
			return;

		if (!ValidateMargins())
			return;

		if (!ValidateSpacing())
			return;
	}

	private bool ValidatePopupSizeFields()
	{
		int minAllowedSlideWidth = LayoutManager.MinAllowedSize.Width;
		int minAllowedSlideHeight = LayoutManager.MinAllowedSize.Height;
		const int maxAllowedSlideWidth = 1200;
		validPopupSlideWidth = ValidateFieldInRange(PopupSlideWidth, minAllowedSlideWidth, maxAllowedSlideWidth, PopupSlideWidthError);
		validPopupSlideHeight = ValidateFieldInRange(PopupSlideHeight, minAllowedSlideHeight, maxAllowedSlideWidth, PopupSlideHeightError);

		if (!pageValid)
			return false;

		validPopupSlideMinWidth = ValidateFieldInRange(PopupSlideMinWidth, minAllowedSlideWidth, validPopupSlideWidth, PopupSlideMinWidthError);
		validPopupSlideMinHeight = ValidateFieldInRange(PopupSlideMinHeight, minAllowedSlideHeight, validPopupSlideHeight, PopupSlideMinHeightError);

		if (!pageValid)
			return false;

		if (tour.V3CompatibilityEnabled)
            validPopupTextOnlyWidth = ValidateFieldInRange(PopupTextOnlyWidthTextBox, validPopupSlideMinWidth, validPopupSlideWidth, PopupTextOnlyWidthError);

		return pageValid;
	}

	private bool ValidateAppearanceFields()
	{
		validPopupSlideBorderWidth = ValidateFieldInRange(PopupSlideBorderWidth, 0, 8, PopupSlideBorderWidthError);
		if (!pageValid)
			return false;

		validPopupCornerRadius = ValidateFieldInRange(PopupCornerRadius, 0, 1000, PopupCornerRadiusError);
		if (!pageValid)
			return false;

		validImageCornerRadius = ValidateFieldInRange(ImageCornerRadius, 0, 1000, ImageCornerRadiusError);
		if (!pageValid)
			return false;

		validDropShadowDistance = ValidateFieldInRange(DropShadowDistance, 0, 100, DropShadowDistanceError);
		if (!pageValid)
			return false;

		if (UseColorSchemeColorsCheckBox.Checked)
		{
			PopupSlideBackgroundColorSwatch.ColorValue = tour.ColorScheme.SlideBackgroundColor;
			PopupSlideTitleTextColorSwatch.ColorValue = tour.ColorScheme.SlideTitleTextColor;
			PopupSlideTextColorSwatch.ColorValue = tour.ColorScheme.SlideTextColor;
		}

		validPopupSlideBackgroundColor = ValidateColorSwatch(PopupSlideBackgroundColorSwatch);
		validPopupSlideTitleTextColor = ValidateColorSwatch(PopupSlideTitleTextColorSwatch);
		validPopupSlideTextColor = ValidateColorSwatch(PopupSlideTextColorSwatch);
		
		validPopupSlideBorderColor = ValidateColorSwatch(PopupSlideBorderColorSwatch);
		if (!pageValid)
			return false;

		return pageValid;
	}

	private void ClearErrors()
	{
		ClearErrors(
			PopupSlideBorderWidthError,
			PopupSlideWidthError,
			PopupSlideHeightError,
			PopupSlideMinWidthError,
			PopupSlideMinHeightError,
			PopupTextOnlyWidthError,
			PopupCornerRadiusError,
			ImageCornerRadiusError,
			DropShadowDistanceError
			);
		
		ClearMarginsAndSpacingErrors();
	}
}
