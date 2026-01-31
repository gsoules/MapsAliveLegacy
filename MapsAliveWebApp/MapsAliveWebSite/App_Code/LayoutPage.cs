// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.UI;

public abstract class LayoutPage : MemberPage
{
	private MarginsAndSpacingAccess controls;
	protected bool marginsAndSpacingChanged;
	private bool showMarginAdjustmentsOnly;
	private SlideLayout slideLayout;
	protected int validMarginTop;
	protected int validMarginRight;
	protected int validMarginBottom;
	protected int validMarginLeft;
	private int validSpacingH;
	private int validSpacingV;

	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}

	protected void InitMarginsAndSpacingForLayoutArea(MarginsAndSpacingAccess controls)
	{
		this.controls = controls;
		slideLayout = tourPage.LayoutAreaSlideLayout;
	}

	protected void InitMarginsAndSpacingForPopup(MarginsAndSpacingAccess controls)
	{
		this.controls = controls;
		slideLayout = tourPage.PopupSlideLayout;
	}

	protected void InitMargins(MarginsAndSpacingAccess controls)
	{
		showMarginAdjustmentsOnly = true;
		InitMarginsAndSpacingForLayoutArea(controls);
		controls.SpacingYesPanel.Visible = false;
	}

	protected void InitMarginsAndSpacingControls()
	{
		InitMarginControls();

		if (showMarginAdjustmentsOnly)
		{
			controls.SpacingPanel.Visible = false;
		}
		else
		{
			InitSpacingControls();
		}
	}

	private void InitMarginControls()
	{
		controls.MarginTop.Text = slideLayout.Margin.Top.ToString();
		AddChangeDetectionForPreview(controls.MarginTop);

		controls.MarginRight.Text = slideLayout.Margin.Right.ToString();
		AddChangeDetectionForPreview(controls.MarginRight);

		controls.MarginBottom.Text = slideLayout.Margin.Bottom.ToString();
		AddChangeDetectionForPreview(controls.MarginBottom);

		controls.MarginLeft.Text = slideLayout.Margin.Left.ToString();
		AddChangeDetectionForPreview(controls.MarginLeft);
	}

	private void InitSpacingControls()
	{
		int spacingH = slideLayout.Spacing.H;
		int spacingV = slideLayout.Spacing.V;
		bool hasSpacing = false;

		if (slideLayout.HasHorizontalSplitter)
		{
			controls.SpacingH.Text = slideLayout.Spacing.H.ToString();
			AddChangeDetectionForPreview(controls.SpacingH);
			hasSpacing = true;
		}
		else
		{
			controls.SpacingCellH1.Style.Add(HtmlTextWriterStyle.Display, "none");
			controls.SpacingCellH2.Style.Add(HtmlTextWriterStyle.Display, "none");
		}

		if (slideLayout.HasVerticalSplitter)
		{
			controls.SpacingV.Text = slideLayout.Spacing.V.ToString();
			AddChangeDetectionForPreview(controls.SpacingV);
			hasSpacing = true;
		}
		else
		{
			controls.SpacingCellV1.Style.Add(HtmlTextWriterStyle.Display, "none");
			controls.SpacingCellV2.Style.Add(HtmlTextWriterStyle.Display, "none");
		}

		controls.SpacingYesPanel.Visible = hasSpacing;
		controls.SpacingNoPanel.Visible = !hasSpacing;
	}

	protected void ReadMarginFields()
	{
		SlideLayoutMargin newMargin = new SlideLayoutMargin(validMarginTop, validMarginRight, validMarginBottom, validMarginLeft);
		if (slideLayout.Margin != newMargin)
		{
			slideLayout.SetNewMargin(newMargin);
			marginsAndSpacingChanged = true;
		}
	}

	protected void ReadSpacingFields()
	{
		SlideLayoutSpacing spacing = new SlideLayoutSpacing(validSpacingH, validSpacingV);
		if (slideLayout.Spacing != spacing)
		{
			slideLayout.SetNewSpacing(spacing);
			marginsAndSpacingChanged = true;
		}
	}

	protected void ShowSpacingPanelOnRight()
	{
		controls.SpacingPanel.CssClass = "spacingPanelRight";
	}

	protected bool ValidateMargins()
	{
		int min = 0;
		int max = 512;

		validMarginTop = ValidateFieldInRange(controls.MarginTop, min, max, controls.MarginTopError);
		validMarginBottom = ValidateFieldInRange(controls.MarginBottom, min, max, controls.MarginBottomError);
		validMarginRight = ValidateFieldInRange(controls.MarginRight, min, max, controls.MarginRightError);
		validMarginLeft = ValidateFieldInRange(controls.MarginLeft, min, max, controls.MarginLeftError);
		return pageValid;
	}

	protected bool ValidateSpacing()
	{
		// The max value is arbitrarily set to 1/3 the page size. We used to base it on the slide's inner size
		// but that does not get updated until after this page posts, and thus the calculation would not take
		// into account new size values the user just typed.
		int min = 0;
		int max = 512;

		if (tourPage.ActiveSlideLayout.HasHorizontalSplitter)
			validSpacingH = ValidateFieldInRange(controls.SpacingH, min, max, controls.SpacingHError);
		else
			validSpacingH = tourPage.ActiveSlideLayout.Spacing.H;

		if (!pageValid)
			return false;

		if (tourPage.ActiveSlideLayout.HasVerticalSplitter)
			validSpacingV = ValidateFieldInRange(controls.SpacingV, min, max, controls.SpacingVError);
		else
			validSpacingV = tourPage.ActiveSlideLayout.Spacing.V;

		return pageValid;
	}

	protected void ClearMarginsAndSpacingErrors()
	{
		ClearErrors(
			controls.MarginTopError,
			controls.MarginRightError,
			controls.MarginBottomError,
			controls.MarginLeftError,
			controls.SpacingHError,
			controls.SpacingVError);
	}

}
