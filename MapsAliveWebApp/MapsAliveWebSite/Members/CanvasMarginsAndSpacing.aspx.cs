// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.UI.WebControls;

public partial class Members_CanvasMarginsAndSpacing : LayoutPage
{
	protected override void InitControls(bool undo)
	{
		if (!undo && IsPostBack)
			return;

		InitMarginsAndSpacingControls();
		InitPreviewControls();
	}

	private void InitPreviewControls()
	{
		// Show the dimensions of each area in the slide.
		int drawDimensions = tour.V3CompatibilityEnabled ? 1 : 0;

        // Draw the image to fit the available width.
        int thumbnailDimension = Math.Min(tour.TourSize.Width, TourBuilderPageContentRightWidth);

		LayoutPreviewImage.ImageUrl = string.Format("PageRenderer.ashx?args={0},{1},{2},{3},{4}&v={5}",
			(int)PreviewType.PagePreview,
			thumbnailDimension,
			tourPage.Id,
			drawDimensions,
			(int)tourPage.ActiveSlideLayout.Pattern,
			DateTime.Now.Ticks
		);
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetActionIdForPageAction(MemberPageActionId.LayoutAreaMarginsAndSpacing);
		GetSelectedTourPage();

		SetPageTitle(tourPage.SlidesPopup ? "Map Margins" : "Margins and Spacing");
		
		if (tourPage.SlidesPopup)
			InitMargins(MarginsAndSpacingControl);
		else
			InitMarginsAndSpacingForLayoutArea(MarginsAndSpacingControl);
	}

	protected override void PerformUpdate()
	{
		tourPage.UpdateDatabase();

		if (marginsAndSpacingChanged)
		{
			tourPage.LayoutManager.PerformAutoLayoutForCurrentPage();
			tourPage.SetLayoutChanged();
		}
	}

	protected override void ReadPageFields()
	{
		ReadMarginFields();
		
		if (!tourPage.SlidesPopup)
			ReadSpacingFields();
	}

	protected override void SetFieldError(Label errorLabel)
	{
		if (!fieldValid)
			errorLabel.Text = "*";

		base.SetFieldError(errorLabel);
	}

	protected override void Undo()
	{
		ClearErrors();
	}

	protected override void ValidatePage()
	{
		ClearErrors();

		if (!ValidateMargins())
			return;

		if (!tourPage.SlidesPopup)
		{
			if (!ValidateSpacing())
				return;
		}
	}

	private void ClearErrors()
	{
		ClearMarginsAndSpacingErrors();
	}
}