// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public class TourLayout
{
    private const int bannerPaddingLeft = 0;
    private const int bannerPaddingTop = 0;
 	private const int customfooterHeight = 16;
    private const bool hasBackgroundColor = true;
	private const int footerStripeHeight = 18;
    private const int footerStripeBorderHeight = 2;
    private const int headerStripeBorderHeight = 2;
    private const int headerStripeHeight = 8;
	private const int spacing = 8;
    private const int titleHeight = 32;
    private const int titleOffsetBottom = 4;
    private const int titleOffsetLeft = 4;
    private const int titleOffsetTop = 4;
    private const int topMenuMargin = spacing;

	public TourLayout()
	{
	}

	#region ===== Properties ========================================================

	public static int BannerPaddingTop
	{
        get { return bannerPaddingTop; }
	}

	public static int BannerPaddingLeft
	{
        get { return bannerPaddingLeft; }
	}

	public static bool HasBackgroundColor
	{
        get { return hasBackgroundColor; }
	}

	public static int FooterHeight
	{
        get { return customfooterHeight; }
	}

	public static int FooterStripeHeight
	{
		get { return footerStripeHeight; }
	}

	public static int FooterStripeBorderHeight
	{
		get { return footerStripeBorderHeight; }
	}

	public static int HeaderStripeBorderHeight
	{
        get { return headerStripeBorderHeight; }
	}

	public static int HeaderStripeHeight
	{
		get { return headerStripeHeight; }
	}

	public static int TitleHeight
	{
		get { return titleHeight; }
	}

	public static int TitleOffsetBottom
	{
        get { return titleOffsetBottom; }
	}

	public static int TitleOffsetLeft
	{
        get { return titleOffsetLeft; }
	}

	public static int TitleOffsetTop
	{
        get { return titleOffsetTop; }
	}
	#endregion

	#region ===== Public ============================================================

	public static Size CalculateLayoutAreaSizeFromTourSize(Tour tour, Size tourSize)
	{
		// Determine the available space for the layout area by starting with the tour
		// size and subtracting off the areas needed for tour options like the menu,
		// title, and banner. Whatever is leftover will be used for the layout area.
		int layoutAreaWidth = tourSize.Width - CalculateWidthOfTourOptions(tour);
		int layoutAreaHeight = tourSize.Height - CalculateHeightOfTourOptions(tour, tourSize.Width);
		Size layoutAreaSize = new Size(layoutAreaWidth, layoutAreaHeight);
		return layoutAreaSize;
	}

	public static int CalculateHeightOfTourOptions(Tour tour, int tourWidth)
	{
		int heightUpper = CalculateHeightOfTourOptionsUpper(tour, tourWidth);
		int heightLower = CalculateHeightOfTourOptionsLower(tour, tourWidth);
		return heightUpper + heightLower;
	}

	public static int CalculateHeightOfTourOptionsLeft(Tour tour)
	{
		return tour.MenuLocationIdEffective == (int)Tour.MenuLocation.Left ? tour.MenuWidth : 0;
	}

	public static int CalculateHeightOfTourOptionsLower(Tour tour, int tourWidth)
	{
		int footerHeight = tour.HasFooterStripe ? footerStripeHeight : 0;
        if (tour.HasCustomFooter)
            footerHeight += customfooterHeight;
        return footerHeight;
	}

	public static int CalculateHeightOfTourOptionsUpper(Tour tour, int tourWidth)
	{
		int topMenuAreaHeight;

		if (tour.MenuLocationIdEffective == (int)Tour.MenuLocation.Top)
			topMenuAreaHeight = tour.MenuHeight + topMenuMargin;
		else
			topMenuAreaHeight = 0;

		int titleH = tour.HasTitle ? titleHeight : 0;
		if (tour.HasHeaderStripe)
			titleH += HeaderStripeHeight;
		
		int bannerHeight = tour.HasBanner ? tour.Banner.OptimalHeight(tourWidth) : 0;

		return bannerHeight + titleH + topMenuAreaHeight;
	}

	public static int CalculateWidthOfTourOptions(Tour tour)
	{
		int menuWidth = tour.MenuLocationIdEffective == (int)Tour.MenuLocation.Left ? tour.MenuWidth : 0;
		return menuWidth;
	}
	#endregion


	#region ===== Private ============================================================
	#endregion
}
