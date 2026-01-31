// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.UI.WebControls;

public partial class Members_EditColorScheme : EditStylePage
{
	private string validLayoutAreaBackgroundColor;
	private string validTitleTextColor;
	private string validTitleBackgroundColor;
	private string validStripeColor;
	private string validStripeBorderColor;
	private string validMenuBackgroundColor;
	private string validMenuNormalTextColor;
	private string validMenuNormalBackgroundColor;
	private string validMenuLineColor;
	private string validMenuSelectedTextColor;
	private string validMenuSelectedBackgroundColor;
	private string validMenuHoverTextColor;
	private string validMenuHoverBackgroundColor;
	private string validSlideTitleTextColor;
	private string validSlideTextColor;
	private string validSlideBackgroundColor;
	private string validFooterLinkTextColor;
    private bool useV3ColorScheme;

	protected override void InitControls(bool undo)
	{
		MemberPage.InitShowUsageControl(ShowUsageControl, colorScheme);
		
		SwitchColorSchemeControl.OnClickActionId = MemberPageActionId.EditColorScheme;
		SwitchColorSchemeControl.QueryString = string.Format("?id={0}&switch=1", colorScheme.Id);
		SwitchColorSchemeControl.Title = string.Format("Switch to {0}", colorScheme.Name);
		
		if (!undo && IsPostBack)
			return;

		ColorSchemeNameTextBox.Text = colorScheme.Name;
		AddChangeDetection(ColorSchemeNameTextBox);

        if (useV3ColorScheme)
        {
            MapAreaBackgroundColorSwatch.Visible = false;
            BannerBackgroundColorSwatch.Visible = false;
        }
        else
        {
            MapAreaBackgroundColorSwatch.ColorValue = colorScheme.MenuNormalBackgroundColor;
            BannerBackgroundColorSwatch.ColorValue = colorScheme.MenuSelectedBackgroundColor;
            
            MenuNormalTextColorSwatch.Label = "Menu Item Text";
            MenuHoverTextColorSwatch.Label = "Menu Item Hover Text";
            MenuSelectedTextColorSwatch.Label = "Selected Menu Item Text";
            
            MenuNormalBackgroundColorSwatch.Visible = false;
            MenuSelectedBackgroundColorSwatch.Visible = false;

            MenuHoverBackgroundColorSwatch.Visible = false;
            MenuBackgroundColorSwatch.Visible = false;
            MenuLineColorSwatch.Visible = false;
        }

        LayoutAreaBackgroundColorSwatch.ColorValue = colorScheme.LayoutAreaBackgroundColor;
		TitleTextColorSwatch.ColorValue = colorScheme.TitleTextColor;
		TitleBackgroundColorSwatch.ColorValue = colorScheme.TitleBackgroundColor;
		StripeColorSwatch.ColorValue = colorScheme.StripeColor;
		StripeBorderColorSwatch.ColorValue = colorScheme.StripeBorderColor;
		FooterLinkColorSwatch.ColorValue = colorScheme.FooterLinkTextColor;
		MenuBackgroundColorSwatch.ColorValue = colorScheme.MenuBackgroundColor;
		MenuNormalTextColorSwatch.ColorValue = colorScheme.MenuNormalTextColor;
		MenuNormalBackgroundColorSwatch.ColorValue = colorScheme.MenuNormalBackgroundColor;
		MenuLineColorSwatch.ColorValue = colorScheme.MenuLineColor;
		MenuSelectedTextColorSwatch.ColorValue = colorScheme.MenuSelectedTextColor;
		MenuSelectedBackgroundColorSwatch.ColorValue = colorScheme.MenuSelectedBackgroundColor;
		MenuHoverTextColorSwatch.ColorValue = colorScheme.MenuHoverTextColor;
		MenuHoverBackgroundColorSwatch.ColorValue = colorScheme.MenuHoverBackgroundColor;
		SlideTitleTextColorSwatch.ColorValue = colorScheme.SlideTitleTextColor;
		SlideTextColorSwatch.ColorValue = colorScheme.SlideTextColor;
		SlideBackgroundColorSwatch.ColorValue = colorScheme.SlideBackgroundColor;

		InitPreviewControls();
	}

	private void InitPreviewControls()
	{
		string previewMsg;
		if (tourPage == null)
		{
			LayoutPreviewImagePanel.Visible = false;
			return;
		}

        // Don't attempt to display a preview of the color scheme when there's no active tour.
        if (tour == null)
            return;

        if (tour.ColorScheme.Id == colorScheme.Id)
		{
			previewMsg = "Your tour is using this color scheme.";
			SwitchColorSchemeControl.Visible = false;
		}
		else
		{
			previewMsg = string.Format(AppContent.Topic("NotUsingColorSchemeMesssage"), tour.ColorScheme.Name);
		}
		
		PreviewMsg.Text = previewMsg;

		SlideLayout slideLayout = tourPage.ActiveSlideLayout;

		// Show the dimensions of each area in the slide.
		int drawDimensions = useV3ColorScheme ? 1 : 0;

        // Draw the image to fit the available width.
        int thumbnailDimension = Math.Min(tour.TourSize.Width, TourBuilderPageContentRightWidth);

        PagePreviewImage.ImageUrl = string.Format("PageRenderer.ashx?args={0},{1},{2},{3},{4}&tsid={5}&v={6}",
			(int)PreviewType.PagePreview,
			thumbnailDimension,
			tourPage.Id,
			drawDimensions,
			(int)slideLayout.Pattern,
			colorScheme.Id,
			DateTime.Now.Ticks
		);
	}

	protected override void PageLoad()
	{
		Utility.RegisterColorChooserJavaScript(this);

		SetMasterPage(Master);
		SetPageTitle(TourResource.GetTitleForEditPage(TourResourceType.TourStyle));
		SetActionId(MemberPageActionId.EditColorScheme);
		GetSelectedTourOrNone();

        // Determine whether the V3 or V4 color scheme should be edited. If there is no current tour,/ use V4.
        useV3ColorScheme = tour != null && tour.V3CompatibilityEnabled;

        colorScheme = (ColorScheme)CreateResourceFromQueryStringId(TourResourceType.TourStyle);

		if (colorScheme == null)
		{
			// This can happen if a user deletes a color scheme and then uses the Back button to return to this screen.
			Response.Redirect(MemberPageAction.ActionPageTarget(MemberPageActionId.ColorSchemeExplorer));
		}

		if (tour != null && Request.QueryString["switch"] == "1")
		{
			tour.ColorScheme = colorScheme;
			tour.SwitchColorScheme();
		}
	}
	protected override void ReadPageFields()
	{
		colorScheme.Name = validName;

		colorScheme.LayoutAreaBackgroundColor = validLayoutAreaBackgroundColor;
		colorScheme.TitleTextColor = validTitleTextColor;
		colorScheme.TitleBackgroundColor = validTitleBackgroundColor;
		colorScheme.StripeColor = validStripeColor;
		colorScheme.StripeBorderColor = validStripeBorderColor;
		colorScheme.FooterLinkTextColor = validFooterLinkTextColor;
		colorScheme.MenuNormalTextColor = validMenuNormalTextColor;
		colorScheme.MenuSelectedTextColor = validMenuSelectedTextColor;
		colorScheme.MenuHoverTextColor = validMenuHoverTextColor;
		colorScheme.SlideTitleTextColor = validSlideTitleTextColor;
		colorScheme.SlideTextColor = validSlideTextColor;
		colorScheme.SlideBackgroundColor = validSlideBackgroundColor;

		colorScheme.MenuNormalBackgroundColor = validMenuNormalBackgroundColor;
	    colorScheme.MenuSelectedBackgroundColor = validMenuSelectedBackgroundColor;

        if (useV3ColorScheme)
        {
		    colorScheme.MenuBackgroundColor = validMenuBackgroundColor;
		    colorScheme.MenuHoverBackgroundColor = validMenuHoverBackgroundColor;
		    colorScheme.MenuLineColor = validMenuLineColor;
        }
        else
        {
            colorScheme.MenuBackgroundColor = colorScheme.MenuHoverBackgroundColor;
            colorScheme.MenuHoverBackgroundColor = colorScheme.MenuHoverBackgroundColor;
            colorScheme.MenuLineColor = colorScheme.MenuLineColor;
        }

        if (tour == null)
            return;

        foreach (TourPage tourPage in tour.TourPages)
            tourPage.InvalidateThumbnail();
    }

    protected override void ValidatePage()
	{
		ValidateResourceName(ColorSchemeNameTextBox, ColorSchemeNameError);
		if (!fieldValid)
			return;

		validLayoutAreaBackgroundColor = ValidateColorSwatch(LayoutAreaBackgroundColorSwatch);
		validTitleTextColor = ValidateColorSwatch(TitleTextColorSwatch);
		validTitleBackgroundColor = ValidateColorSwatch(TitleBackgroundColorSwatch);
		validStripeColor = ValidateColorSwatch(StripeColorSwatch);
		validStripeBorderColor = ValidateColorSwatch(StripeBorderColorSwatch);
		validMenuNormalTextColor = ValidateColorSwatch(MenuNormalTextColorSwatch);
		validMenuSelectedTextColor = ValidateColorSwatch(MenuSelectedTextColorSwatch);
		validMenuHoverTextColor = ValidateColorSwatch(MenuHoverTextColorSwatch);
		validSlideTitleTextColor = ValidateColorSwatch(SlideTitleTextColorSwatch);
		validSlideTextColor = ValidateColorSwatch(SlideTextColorSwatch);
		validSlideBackgroundColor = ValidateColorSwatch(SlideBackgroundColorSwatch);
		validFooterLinkTextColor = ValidateColorSwatch(FooterLinkColorSwatch);
        
        if (useV3ColorScheme)
        {
		    validMenuNormalBackgroundColor = ValidateColorSwatch(MenuNormalBackgroundColorSwatch);
    		validMenuSelectedBackgroundColor = ValidateColorSwatch(MenuSelectedBackgroundColorSwatch);
		    validMenuHoverBackgroundColor = ValidateColorSwatch(MenuHoverBackgroundColorSwatch);
		    validMenuBackgroundColor = ValidateColorSwatch(MenuBackgroundColorSwatch);
		    validMenuLineColor = ValidateColorSwatch(MenuLineColorSwatch);
        }
        else
        {
            validMenuNormalBackgroundColor = ValidateColorSwatch(MapAreaBackgroundColorSwatch);
            validMenuSelectedBackgroundColor = ValidateColorSwatch(BannerBackgroundColorSwatch);

            validMenuHoverBackgroundColor = colorScheme.MenuHoverBackgroundColor;
            validMenuBackgroundColor = colorScheme.MenuBackgroundColor;
            validMenuLineColor = colorScheme.MenuLineColor;
        }
    }

	protected override void ClearErrors()
	{
		ClearErrors(ColorSchemeNameError);
	}
}
