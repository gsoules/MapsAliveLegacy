// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.SessionState;

public enum PreviewType
{
	SlideLayout = 1,
	SlideLayoutInPage = 2,
	PagePreview = 3,
	PagePreviewThumb = 4,
	PagePreviewQuick = 5
}

public class PageRenderer : IHttpHandler, IRequiresSessionState
{
	private Size bannerSize;
	private Bitmap bitmap;
	private Brush brushDimensions;
	private Brush brushHelp;
	private Size layoutAreaSize;
	private HttpContext context;
	private bool drawDimensions;
	private Font fontArea;
	private Font fontDimensions;
	private Font fontHelp;
	private Font fontMenuItem;
	private Font fontTitle;
	private Font fontViewTitle;
	private Font fontDescription;
	private int fontSchemeId;
	private Graphics graphics;
	private bool hasBanner;
	private bool hasFooterStripe;
	private bool hasHeaderStripe;
	private bool hasTitle;
	private int menuLocationId;
	private int menuStyleId;
	private StringFormat noWrap;
	private Size pageSize;
	private Pen pen;
	private Pen penSlideArea;
	private int previewHeight;
	private int previewWidth;
	private SlideLayoutPattern slideLayoutPattern;
	private SolidBrush solidBrush;
	private int splitterH;
	private int splitterV;
	private int scaledDimension;
	private bool showSlideContent;
	private Tour tour;
	private ColorScheme colorScheme;
	private int colorSchemeId;
	private TourFontScheme tourFontScheme;
	private TourPage tourPage;
	private TourView tourView;
	PreviewType previewType;
	private int x;
	private int y;

	private const int hideLayoutMargin = 8;


	public void ProcessRequest (HttpContext context)
	{
		this.context = context;

		// See if the tour Id was passed on the query string.
		int tourId = 0;
		int.TryParse(context.Request.QueryString["tid"], out tourId);
		if (tourId > 0)
		{
			tour = (Tour)MapsAliveState.Retrieve(MapsAliveObjectType.Tour);
			if (tour == null || tour.Id != tourId)
				tour = new Tour(tourId);
		}

		// See if the tour style Id was passed on the query string.
		// This is only done when rendering the preview on the EditColorScheme.aspx page.
		int.TryParse(context.Request.QueryString["tsid"], out colorSchemeId);
		
		if (tour == null)
		{
			// See if we are displaying the preview for the current tour.
			tour = (Tour)MapsAliveState.Retrieve(MapsAliveObjectType.Tour);
		}

		if (tour == null)
		{
			// Still no tour.  The session may have expired so try getting it from the cookie.
			tour = MapsAliveState.SelectedTourOrNull;
		}

		if (tour == null)
		{
			// Something is wrong, so render a text message as the preview image.
			DrawError("Session expired");
		}
		else
		{
			if (tour != null && tour.TourPages.Count == 0)
			{
				DrawError("Tour has no pages");
			}
			else if (ParseRequest())
			{
				if (RenderCachedPreview())
				    return;

				DrawPreview();
			}
			else
			{
				SetTourOptionFlags();
				DrawError("Request denied");
			}
		}
		
		GraphicsDispose();

		RenderPreview(previewWidth, previewHeight);
    }

	#region ===== Properties ========================================================

	private bool RenderingScaledImage
	{
		get { return scaledDimension > 0; }
	}

	private bool DrawingPopup
	{
		get { return previewType == PreviewType.SlideLayout && tourPage.SlidesPopup; }
	}
	
	#endregion

	#region ===== Public ============================================================

    public bool IsReusable
	{
        get	{ return false; }
    }
	#endregion

	#region ===== Protected =========================================================
	#endregion

	#region ===== Private ===========================================================

	private void DrawAreaPlaceholderColor(ref Rectangle rect, Color areaColor, Color textColor, string text)
	{
		SolidBrush solidBrush = new SolidBrush(areaColor);
		graphics.FillRectangle(solidBrush, rect);
		solidBrush = new SolidBrush(textColor);
		graphics.DrawString(text, fontArea, solidBrush, x + 10, y + 10);
	}

	private void DrawAreaPlaceholderPattern(ref Rectangle rect, Color areaColor, Color textColor, string text, HatchStyle hatchStyle)
	{
		HatchBrush hatchBrush = new HatchBrush(hatchStyle, Color.Black, Color.White);
		graphics.FillRectangle(hatchBrush, rect);
		solidBrush = new SolidBrush(textColor);
		graphics.DrawString(text, fontArea, solidBrush, x + 10, y + 10);
	}

	private void DrawBanner()
	{
		if (hasBanner)
		{
            int bannerHeight = tour.Banner.OptimalHeight(pageSize.Width);
			Byte[] bytes = tour.Banner.Image.Bytes;
			using (MemoryStream memoryStream = new MemoryStream(bytes))
			{
				Bitmap bitmap = (Bitmap)Bitmap.FromStream(memoryStream);
				Bitmap scaledBitmap = Utility.ScaledBitmap(bitmap, new Size(pageSize.Width, bannerHeight), true);
				bitmap = scaledBitmap;

                // V4 uses the V3 menu selected background color for the map area background color.
                string backgroundColor = tour.V3CompatibilityEnabled ? colorScheme.LayoutAreaBackgroundColor : colorScheme.MenuSelectedBackgroundColor;
                SolidBrush solidBrush = new SolidBrush(Utility.HexToColor(backgroundColor));
                Rectangle rect = new Rectangle(0, 0, tour.TourSize.Width, bitmap.Height + 1);
                graphics.FillRectangle(solidBrush, rect);

				rect = new Rectangle(x, y, bitmap.Width, bitmap.Height);
				graphics.DrawImage(bitmap, rect);
				
				if (drawDimensions)
					DrawDimensions(rect.Location, bitmap.Width, bitmap.Height, false);
			}
			y += bannerHeight;
		}
	}

	private void DrawDimensions(Point point, int width, int height, bool textArea)
	{
		if (!drawDimensions)
			return;

        string dimensionsText;
        dimensionsText = string.Format("{0}px x {1}px", width, height);

        SizeF sizeF = graphics.MeasureString(dimensionsText, fontDimensions);
		int x = point.X + (width / 2) - (int)(sizeF.Width / 2);
		int y = point.Y + (height / 2) - 8;
		int w = (int)sizeF.Width + 4;
		int h = (int)sizeF.Height + 3;
		Rectangle rect = new Rectangle(x, y, w, h);
		if (w > width || h > height)
			return;
		graphics.FillRectangle(textArea ? Brushes.White : brushDimensions, rect);
		graphics.DrawString(dimensionsText, fontDimensions, Brushes.Red, x + 2, y + 2);
		graphics.DrawRectangle(pen, rect);
	}

	private void DrawError(string msg)
	{
		DrawError(200, 24, msg);
	}

	private void DrawError(int width, int height, string msg)
	{
		if (graphics == null)
		{
			bitmap = new Bitmap(width, height);
			GraphicsInit(bitmap);
		}
		Rectangle rect = new Rectangle(0, 0, width, height);
		graphics.DrawString(msg, fontHelp, brushHelp, rect);

		previewWidth = width;
		previewHeight = height;
	}

	private void DrawFooterStripe()
	{
		if (hasFooterStripe)
		{
			int h = TourLayout.FooterStripeHeight;
			
			const int outlineWidth = 1;
			int x = outlineWidth;
			int y = previewHeight - outlineWidth - h;

			Pen p = new Pen(Utility.HexToColor(colorScheme.StripeBorderColor));
			graphics.DrawLine(p, x, y, pageSize.Width, y);

			Rectangle rect = new Rectangle(x, y + 1, pageSize.Width, h - 2);
			solidBrush.Color = Utility.HexToColor(colorScheme.StripeColor);
			graphics.FillRectangle(solidBrush, rect);
			
			p.Color = Utility.HexToColor(colorScheme.StripeBorderColor);
			graphics.DrawLine(p, x, y + h - 1, pageSize.Width, y + h - 1);
		}
	}

	private void DrawHeaderStripe()
	{
		if (hasHeaderStripe)
		{
			int h = TourLayout.HeaderStripeHeight;

			Pen p = new Pen(Utility.HexToColor(colorScheme.StripeBorderColor));
			graphics.DrawLine(p, x, y, pageSize.Width, y);

			Rectangle rect = new Rectangle(x, y + 1, pageSize.Width, h - 2);
			solidBrush.Color = Utility.HexToColor(colorScheme.StripeColor);
			graphics.FillRectangle(solidBrush, rect);
			
			p.Color = Utility.HexToColor(colorScheme.StripeBorderColor);
			graphics.DrawLine(p, x, y + h - 1, pageSize.Width, y + h - 1);

			y += TourLayout.HeaderStripeHeight;
		}
	}

	private void DrawImage(TourImage tourImage, Size area, Size constrainedSize, bool isMapImage)
	{
		Rectangle rect;
		bool imageDrawn = false;

		if (tourImage.Id > 0)
		{
			try
			{
				// Determine how big of an image we need.
				Size areaSize = new Size(area.Width, area.Height);
				Size requiredSize = Utility.ScaledImageSize(tourImage.Size, constrainedSize);

				// Get the image's bytes.
				Byte[] bytes;
				// Debug.Write(string.Format("IMAGE {0} : Old thumbnail {1}x{2} : ", tourImage.Id, tourImage.ThumbnailSize.Width, tourImage.ThumbnailSize.Height));
				if (tourImage.ThumbnailSize.Width >= requiredSize.Width && tourImage.ThumbnailSize.Height >= requiredSize.Height)
				{
					// A large-enough cached version of the image's thumbnail is already in memory.
					// Debug.WriteLine(string.Format("RESUSE {0}x{1}", requiredSize.Width, requiredSize.Height));
					bytes = tourImage.Thumbnail(tourImage.ThumbnailSize);
				}
				else
				{
					// There is either no cached thumbnail or its too small.  See if we need the full sized
					// image or if a larger thumbnail will do.  If a thumbnail works, we ask for it so that
					// it will get cached and possibly be usable during a future call to this method.  Using
					// cached thumbnails is especially important when previewing page layouts because the same
					// map and view images are needed over and over again for different layouts.
					if (requiredSize.Width < tourImage.Size.Width && requiredSize.Height < tourImage.Size.Height)
					{
						// A thumbnail will do.
						// Debug.WriteLine(string.Format("NEW {0}x{1}", requiredSize.Width, requiredSize.Height));
						bytes = tourImage.Thumbnail(requiredSize);
					}
					else
					{
						// We need the entire image.
						// Debug.WriteLine(string.Format("FULL SIZE {0}x{1}", requiredSize.Width, requiredSize.Height));
						bytes = tourImage.Bytes;
					}
				}

				// Create a bitmap from the image and draw it.  Since the image we got might be
				// bigger than we need (because it came from a large thumbnail that was already
				// in cache) we'll scale the image down if necessary.
				using (MemoryStream memoryStream = new MemoryStream(bytes))
				{
					Bitmap bitmap = (Bitmap)Bitmap.FromStream(memoryStream);
					Bitmap scaledBitmap = Utility.ScaledBitmap(bitmap, areaSize, true);
					bitmap = scaledBitmap;
					rect = new Rectangle(x, y, bitmap.Width, bitmap.Height);
					graphics.DrawImage(bitmap, rect);
				}

				imageDrawn = true;
			}
			catch
			{
			}
		}

		if (!imageDrawn)
		{
			rect = new Rectangle(x, y, area.Width, area.Height);
			string msg = isMapImage ? Resources.Text.PreviewNoMapImage : Resources.Text.PreviewNoViewImage;
			graphics.DrawString(msg, fontHelp, brushHelp, rect);
		}
	}

	private void DrawImageArea(Size imageAreaSize)
	{
		if (!Utility.HasWidthAndHeight(imageAreaSize))
			return;

		int imageAreaWidth = imageAreaSize.Width;
		int imageAreaHeight = imageAreaSize.Height;

        if (slideLayoutPattern == SlideLayoutPattern.HMIT || slideLayoutPattern == SlideLayoutPattern.HMTI)
        {
            imageAreaHeight /= 2;
            if (slideLayoutPattern == SlideLayoutPattern.HMTI)
                y += imageAreaHeight + tourPage.LayoutAreaSlideLayout.Spacing.H;
        }

        Rectangle rect = new Rectangle(x, y, imageAreaWidth, imageAreaHeight);

		solidBrush = new SolidBrush(Utility.HexToColor(DrawingPopup ? tourPage.PopupOptions.BackgroundColor : colorScheme.LayoutAreaBackgroundColor));
		graphics.FillRectangle(solidBrush, rect);

		if (showSlideContent && tourView != null && tourView.Image != null)
		{
			DrawImage(tourView.Image, new Size(imageAreaWidth, imageAreaHeight), tourView.GetConstrainedImageSize(), false);
		}
		else
		{
			DrawAreaPlaceholderColor(ref rect, Color.DarkGray, Color.Gainsboro, "Image Area");
		}

		if (drawDimensions)
			DrawDimensions(rect.Location, imageAreaWidth, imageAreaHeight, false);
	}

	private void DrawMapArea(Size mapAreaSize)
	{
		if (!Utility.HasWidthAndHeight(mapAreaSize))
			return;

		int mapAreaWidth = mapAreaSize.Width;
		int mapAreaHeight = mapAreaSize.Height;

		Rectangle rect = new Rectangle(x, y, mapAreaWidth, mapAreaHeight);

		if (tourPage.IsGallery)
		{
			string fileLocation;
			string backgroundColor = tourPage.MapPlaceholderColor;
			fileLocation = FileManager.WebAppFileLocationAbsolute("Images", "Gallery.png");

			SolidBrush solidBrush = new SolidBrush(Utility.HexToColor(backgroundColor));
			graphics.FillRectangle(solidBrush, rect);
			
			Bitmap bitmap = new Bitmap(fileLocation);
			Bitmap scaledBitmap = Utility.ScaledBitmap(bitmap, rect.Size, true);
			bitmap = scaledBitmap;
			int x_ = x + ((rect.Width - bitmap.Width) / 2);
			int y_ = y + ((rect.Height - bitmap.Height) / 2);
			graphics.DrawImage(bitmap, x_, y_, bitmap.Width, bitmap.Height);
		}
		else if (tourPage.MapImage.Length == 0)
		{
			DrawAreaPlaceholderColor(ref rect, Utility.HexToColor(tourPage.MapPlaceholderColor), Color.Gray, "Map Area");
		}
		else
		{
            // V4 uses the V3 menu normal background color for the map area background color. On the Splitters page,
            // use a cyan color as the background to help the user see when the map is not filling the map area.
            string backgroundColor;
            if (previewType == PreviewType.SlideLayout)
                backgroundColor = "#9AFEFF";
            else
                backgroundColor = tour.V3CompatibilityEnabled ? colorScheme.LayoutAreaBackgroundColor : colorScheme.MenuNormalBackgroundColor;

            SolidBrush solidBrush = new SolidBrush(Utility.HexToColor(backgroundColor));
			graphics.FillRectangle(solidBrush, rect);

			MapImage mapImage = tourPage.MapImage;
			DrawImage(mapImage, mapAreaSize, mapAreaSize, true);
		}

		if (drawDimensions)
			DrawDimensions(rect.Location, mapAreaWidth, mapAreaHeight, false);
	}

	private void DrawMenu()
	{
		// Don't draw a menu for V4 since it uses a menu icon, or for V3 when there is no menu.
        if (menuLocationId == (int)Tour.MenuLocation.None || tour.V4)
			return;

		solidBrush.Color = Utility.HexToColor(colorScheme.MenuBackgroundColor);

		Color normalMenuTextColor = Utility.HexToColor(colorScheme.MenuNormalTextColor);
		Color selectedMenuTextColor = Utility.HexToColor(colorScheme.MenuSelectedTextColor);

		Rectangle rect;

		if (menuLocationId == (int)Tour.MenuLocation.Left)
		{
            int bannerHeight = hasBanner ? tour.Banner.OptimalHeight() : 0;
            int pageTitleHeight = hasTitle ? (TourLayout.TitleHeight) : 0;
			if (hasHeaderStripe)
				pageTitleHeight += TourLayout.HeaderStripeHeight;
			int footerStripeHeight = hasFooterStripe ? TourLayout.FooterStripeHeight : 0;
			rect = new Rectangle(x, y, tour.MenuWidth, pageSize.Height - bannerHeight - pageTitleHeight - footerStripeHeight);
			graphics.FillRectangle(solidBrush, rect);
			
			rect.X += 1;
			rect.Y += 8;
			rect.Width -= 8;
			foreach (TourPage tourPage in tour.TourPages)
			{
				if (ExcludeFromMenu(tourPage))
					continue;

				SizeF sizeF = DrawMenuItemName(ref normalMenuTextColor, ref selectedMenuTextColor, ref rect, tourPage);
				rect.Y += (int)(sizeF.Height * 1.25f);
			}

			x += tour.MenuWidth;
		}

		if (menuLocationId == (int)Tour.MenuLocation.Top)
		{
			int menuHeight = tour.MenuHeight + 8;
			rect = new Rectangle(x, y, pageSize.Width, menuHeight);
			solidBrush.Color = Utility.HexToColor(colorScheme.MenuBackgroundColor);
			graphics.FillRectangle(solidBrush, rect);
			
			rect.Y += 4;
			foreach (TourPage tourPage in tour.TourPages)
			{
				if (ExcludeFromMenu(tourPage))
					continue;

				SizeF sizeF = DrawMenuItemName(ref normalMenuTextColor, ref selectedMenuTextColor, ref rect, tourPage);
				int itemWidth = (int)sizeF.Width + 8;
				rect.X += itemWidth;
				rect.Width -= itemWidth;
			}

			y += menuHeight;
		}
	}

	private SizeF DrawMenuItemName(ref Color normalMenuTextColor, ref Color selectedMenuTextColor, ref Rectangle rect, TourPage tourPage)
	{
		string itemName = tourPage.Name;
		if (tour.SelectedTourPage != null && tourPage.Id == tour.SelectedTourPage.Id)
			solidBrush.Color = selectedMenuTextColor;
		else
			solidBrush.Color = normalMenuTextColor;
		graphics.DrawString(itemName, fontMenuItem, solidBrush, rect, noWrap);
		SizeF sizeF = graphics.MeasureString(itemName, fontMenuItem);
		return sizeF;
	}

	private void DrawPreview()
	{
		showSlideContent = (
			(previewType != PreviewType.SlideLayoutInPage || tour.V4) &&
			(MapsAliveState.Account.ShowSlideContentInLayoutPreview || previewType == PreviewType.PagePreviewThumb || previewType == PreviewType.PagePreviewQuick));

		if (DrawingPopup)
		{
			Size slideSize = tourPage.SlidesPopup ? tourPage.PopupSlideLayout.OuterSize : layoutAreaSize;
			pageSize = LayoutManager.CalculateTourSizeForLayoutAreaOuterSize(tour, slideSize);
			previewWidth = slideSize.Width;
			previewHeight = slideSize.Height;
		}
		else if (previewType == PreviewType.SlideLayoutInPage)
		{
			pageSize = LayoutManager.CalculateTourSizeForLayoutAreaOuterSize(tour, layoutAreaSize);

			if (hasBanner)
			{
				// The banner height has to be dynamically adjusted for each layout.
				int adjustedBannerHeight = tour.Banner.OptimalHeight(pageSize.Width);
				bannerSize = new Size(pageSize.Width, adjustedBannerHeight);
			}

			previewWidth = pageSize.Width;
			previewHeight = pageSize.Height;
		}
		else
		{
			pageSize = tour.TourSize;
			previewWidth = pageSize.Width;
			previewHeight = pageSize.Height;
		}

		if (DrawingPopup)
		{
			// Add the border thickness.
			int border = tourPage.PopupOptions.BorderWidth * 2;
			previewWidth += border;
			previewHeight += border;
		}
		else
		{
			// Add a pixel all around for an outline.
			previewWidth += 2;
			previewHeight += 2;
		}

        if (tour.HasCustomFooter)
            previewHeight -= TourLayout.FooterHeight;

		Size previewSize = Size.Empty;
		bool drawTourBackground = previewType == PreviewType.SlideLayout && !tourPage.SlidesPopup && tourPage.ShowLayoutAreaInLayoutEditor;
		if (drawTourBackground)
		{
			Size maxLayoutAreaSize = LayoutManager.CalculateMaxLayoutAreaSize(tour);
			Size maxTourSize = LayoutManager.CalculateTourSizeForLayoutAreaOuterSize(tour, maxLayoutAreaSize);
			previewSize = new Size(maxTourSize.Width + 2, maxTourSize.Height + 2);
		}
		else
		{
			previewSize = new Size(previewWidth, previewHeight);
		}

		
		// Create the bitmap for the preview image.
		bitmap = new Bitmap(previewSize.Width, previewSize.Height);
		GraphicsInit(bitmap);

		try
		{
			if (DrawingPopup)
			{
				DrawSlideLayout();
			}
			else
			{
				if (drawTourBackground)
					DrawTourBackground(previewSize.Width, previewSize.Height);
				DrawPreviewOutline(previewWidth, previewHeight);
				DrawBanner();
				DrawTitleBar();
				DrawHeaderStripe();
				DrawMenu();
				DrawSlideLayout();
				DrawFooterStripe();
			}
		}
		catch (Exception ex)
		{
			DrawError(previewWidth, previewHeight, ex.Message);
			Utility.ReportException("TourPreview", ex);
		}
	}

	private void DrawPreviewOutline(int width, int height)
	{
		x = 0;
		y = 0;

		Rectangle rect;
		rect = new Rectangle(x, y, width - 1, height - 1);
		Color color = Utility.HexToColor(colorScheme.LayoutAreaBackgroundColor);
		SolidBrush solidBrush = new SolidBrush(color);
		graphics.FillRectangle(solidBrush, rect);
		Pen p = new Pen(Utility.HexToColor("#efefef"));
		graphics.DrawRectangle(p, rect);
		solidBrush.Dispose();

		// Move to inside the outline.
		x = 1;
		y = 1;
	}

	private void DrawTourBackground(int width, int height)
	{
		Rectangle rect;
		rect = new Rectangle(x, y, width - 1, height - 1);
		Color color = Utility.HexToColor("#d5e1ec");
		HatchBrush hatchBrush = new HatchBrush(HatchStyle.Trellis, Color.White, color);
		graphics.FillRectangle(hatchBrush, rect);
		graphics.DrawRectangle(Pens.RoyalBlue, rect);
		hatchBrush.Dispose();

		int unusedHeight = height - 2 - tour.TourSize.Height;
		int unusedWidth = width - 2 - tour.TourSize.Width;

		string msg;

		if (unusedWidth > 36)
		{
			if (unusedHeight == 0)
				msg = string.Format("{0} x {1}", unusedWidth, height - 2);
			else
				msg = unusedWidth.ToString();
			SizeF sizeF = graphics.MeasureString(msg, fontDimensions);
			x = width - (unusedWidth / 2 + 8);
			y = height / 2 - (int)(sizeF.Height / 2);
			DrawMessage(ref rect, msg, ref sizeF);
		}

		if (unusedHeight > 24)
		{
			if (unusedWidth == 0)
				msg = string.Format("{0} x {1}", width - 2, unusedHeight);
			else
				msg = unusedHeight.ToString();
			SizeF sizeF = graphics.MeasureString(msg, fontDimensions);
			x = width / 2 - (int)(sizeF.Width / 2);
			y = height - (unusedHeight / 2 + 8);
			DrawMessage(ref rect, msg, ref sizeF);
		}
	}

	private void DrawMessage(ref Rectangle rect, string msg, ref SizeF sizeF)
	{
		int w = (int)sizeF.Width + 4;
		int h = (int)sizeF.Height + 3;
		rect = new Rectangle(x, y, w, h);
		graphics.FillRectangle(Brushes.White, rect);
		graphics.DrawString(msg, fontDimensions, Brushes.RoyalBlue, x + 2, y + 2);
		graphics.DrawRectangle(pen, rect);
	}

	private void DrawSlideLayout()
	{
		SlideLayoutSplitters splitters = new SlideLayoutSplitters(splitterH, splitterV, true, true);

		SlideLayout slideLayout;

		if (tourPage.SlidesPopup)
		{
			slideLayout = new SlideLayout(
				slideLayoutPattern,
				layoutAreaSize, 
				splitters,
				tourPage.PopupSlideLayout.Margin,
				tourPage.PopupSlideLayout.Spacing);
		}
		else
		{
			slideLayout = new SlideLayout(
				slideLayoutPattern,
				layoutAreaSize, 
				splitters,
				tourPage.LayoutAreaSlideLayout.Margin,
				tourPage.LayoutAreaSlideLayout.Spacing);
		}

		int _x = x;
		int _y = y;

		if (DrawingPopup)
		{
			int border = tourPage.PopupOptions.BorderWidth;

			// Fill the layout area with the border color.
			Rectangle rect = new Rectangle(x, y, previewWidth, previewHeight);
			solidBrush.Color = Utility.HexToColor(tourPage.PopupOptions.BorderColor);
			graphics.FillRectangle(solidBrush, rect);

			_x += border;
			_y += border;
			
			// Fill the area inside the border with the background color.
			rect = new Rectangle(_x, _y, slideLayout.OuterSize.Width, slideLayout.OuterSize.Height);

			string backgroundColor;
			backgroundColor = tourPage.PopupOptions.BackgroundColor;

			solidBrush.Color = Utility.HexToColor(backgroundColor);
			graphics.FillRectangle(solidBrush, rect);
		}
		
		Rectangle area;

		bool drawMap = !tourPage.SlidesPopup || previewType != PreviewType.SlideLayout;
		if (drawMap)
		{
			if (tourPage.SlidesPopup)
			{
				area = new Rectangle(new Point(0, 0), tourPage.LayoutAreaSlideLayout.InnerSize);
				x += tourPage.LayoutAreaSlideLayout.Margin.Left;
				y += tourPage.LayoutAreaSlideLayout.Margin.Top;
			}
			else
			{
				area = slideLayout.MapArea;
				x = _x + area.X;
				y = _y + area.Y;
			}
			
			DrawMapArea(area.Size);
		}

		bool drawImageAndTextAreas = !tourPage.SlidesPopup || previewType == PreviewType.SlideLayout;
		if (drawImageAndTextAreas)
		{
			area = slideLayout.ImageArea;
			x = _x + area.X;
			y = _y + area.Y;
			DrawImageArea(area.Size);

			area = slideLayout.TextArea;
			x = _x + area.X;
			y = _y + area.Y;
			DrawTextArea(area.Size);
		}
	}

	private void DrawTextArea(Size textAreaSize)
	{
		if (!Utility.HasWidthAndHeight(textAreaSize))
			return;

		int textAreaWidth = textAreaSize.Width;
        int textAreaHeight = textAreaSize.Height;

        if (slideLayoutPattern == SlideLayoutPattern.HMIT || slideLayoutPattern == SlideLayoutPattern.HMTI)
        {
            textAreaHeight /= 2;
            if (slideLayoutPattern == SlideLayoutPattern.HMIT)
                y += textAreaHeight + tourPage.LayoutAreaSlideLayout.Spacing.H;
        }

        Rectangle rect;

		if (showSlideContent)
		{
			string backgroundColor = colorScheme.SlideBackgroundColor;
			Color color = Utility.HexToColor(backgroundColor);
			SolidBrush solidBrush = new SolidBrush(color);
			
			int titleHeight = tourPage.ShowSlideTitle ? 20 : 0;

			if (tourPage.ShowSlideTitle)
			{
				rect = new Rectangle(x, y, textAreaWidth, Math.Min(titleHeight, textAreaHeight));
				graphics.FillRectangle(solidBrush, rect);

				string titleText = tourView == null ? Resources.Text.PreviewViewTitleHelp : tourView.Title;
				string titleTextColor = DrawingPopup ? tourPage.PopupOptions.TitleTextColor : colorScheme.SlideTitleTextColor;
				solidBrush = new SolidBrush(Utility.HexToColor(titleTextColor));
				graphics.DrawString(titleText, fontViewTitle, solidBrush, rect, noWrap);
			}

			rect = new Rectangle(x, y + titleHeight, textAreaWidth, textAreaHeight - titleHeight);
			if (rect.Height <= 0)
				return;

			solidBrush = new SolidBrush(color);
			graphics.FillRectangle(solidBrush, rect);
			
			string descriptionText = tourView == null ? Resources.Text.PreviewViewDescriptionHelp : tourView.DescriptionText;
            if (descriptionText == string.Empty)
                descriptionText = Resources.Text.PreviewViewDescriptionHelp;
            string descriptionTextColor = DrawingPopup ? tourPage.PopupOptions.TextColor : colorScheme.SlideTitleTextColor;
			solidBrush = new SolidBrush(Utility.HexToColor(descriptionTextColor));
			graphics.DrawString(descriptionText, fontDescription, solidBrush, rect);
		}
		else
		{
			rect = new Rectangle(x, y, textAreaWidth, textAreaHeight);
			DrawAreaPlaceholderPattern(ref rect, Utility.HexToColor("#eaf1ea"), Color.Black, "Text Area", HatchStyle.Horizontal);
		}

		if (drawDimensions)
			DrawDimensions(new Point(x, y), textAreaWidth, textAreaHeight, true);
	}

	private void DrawTitleBar()
	{
		if (hasTitle)
		{
			solidBrush.Color = Utility.HexToColor(colorScheme.TitleBackgroundColor);
			Rectangle rect = new Rectangle(x, y, pageSize.Width, TourLayout.TitleHeight);

			graphics.FillRectangle(solidBrush, rect);

			rect.X += 1;
			rect.Y += 2;
			rect.Width -= 8;

			solidBrush.Color = Utility.HexToColor(colorScheme.TitleTextColor);
			graphics.DrawString(tourPage.TitleOrName, fontTitle, solidBrush, rect, noWrap);

			y += TourLayout.TitleHeight;
		}
	}

	private static bool ExcludeFromMenu(TourPage tourPage)
	{
		bool exclude = tourPage.ExcludeFromNavigation || (tourPage.IsDataSheet && tourPage.FirstTourView.ExcludeFromDirectory);
		return exclude;
	}

	private FontStyle FontSchemeStyle(string fontStyleEnumerationNumber)
	{
		// Convert a string containing the enumeration number for a
		// font style back into a FontStyle enumeration value.
		return (FontStyle)int.Parse(fontStyleEnumerationNumber);
	}

	private void GetFirstTourView()
	{
		// Get the page's first tour view and tell the page that we only need this one
		// view temporarily.  That way the page won't load and cache all of its views.
		// We do this to optimize the case where we are previewing all of a tour's pages
		// and we only need the first view for each page.  Note that if the page already
		// has the first view cached, it will simply return it.
		tourView = tourPage.GetTourViewTemp(tourPage.FirstTourViewId);
	}

	private void GraphicsDispose()
	{
		fontArea.Dispose();
		fontHelp.Dispose();
		fontDimensions.Dispose();
		fontMenuItem.Dispose();
		fontTitle.Dispose();
		fontViewTitle.Dispose();
		fontDescription.Dispose();
		solidBrush.Dispose();
		graphics.Dispose();
	}

	private void GraphicsInit(Bitmap bitmap)
	{
		graphics = Graphics.FromImage(bitmap);
		graphics.Clear(Color.White);
		
		noWrap = new StringFormat(StringFormatFlags.NoWrap);

		brushDimensions = new SolidBrush(Color.FromArgb(100, Color.White));
		brushHelp = Brushes.Green;
		penSlideArea = Pens.WhiteSmoke;
		solidBrush = new SolidBrush(Color.White);

		pen = Pens.LightGray;

		if (colorSchemeId == 0)
		{
			// This shouldn't happen, but we have seen it occur, so this code
			// is here until we figure out what causes it and if it needs to be fixed.
			// It only seems to happen when drawing an error graphic, in which
			// case this colorSchemeId simply might not be initialized.
			colorSchemeId = MapsAliveState.Account.DefaultColorSchemeId;
		}
		colorScheme = new ColorScheme(colorSchemeId);

		tourFontScheme = new TourFontScheme(fontSchemeId, true);

        // Adjust the size of the fonts to be larger when the preview is scaled down.
        double previewScale = drawDimensions && RenderingScaledImage ? ((double)scaledDimension / (double)tour.TourSize.Width) : 1;
        int fontAreaPx = (int)Math.Round(20 / previewScale);
        int fontPx = (int)Math.Round(12 / previewScale);
        int helpFontPx = (int)Math.Round(16 / previewScale);

        fontArea = new Font("Arial", fontAreaPx, FontStyle.Regular);
		fontHelp = new Font("Arial", helpFontPx, FontStyle.Regular);
		fontDimensions = new Font("Courier", fontPx, FontStyle.Regular);

		fontMenuItem = InitFont(
			tourFontScheme.FontFamilyMenuItem,
			PixelsToPoints(tourFontScheme.FontSizeMenuItem),
			tourFontScheme.FontStyleMenuItem,
			tourFontScheme.FontWeightMenuItem);

		fontTitle = InitFont(
			tourFontScheme.FontFamilyTitle,
			PixelsToPoints(tourFontScheme.FontSizeTitle),
			tourFontScheme.FontStyleTitle,
			tourFontScheme.FontWeightTitle);

		fontViewTitle = InitFont(
			tourFontScheme.FontFamilyHeading,
			PixelsToPoints(tourFontScheme.FontSizeHeading),
			tourFontScheme.FontStyleHeading,
			tourFontScheme.FontWeightHeading);

		fontDescription = InitFont(
			tourFontScheme.FontFamilyDescription,
			PixelsToPoints(tourFontScheme.FontSizeDescription),
			tourFontScheme.FontStyleDescription,
			tourFontScheme.FontWeightDescription);
	}

	private Font InitFont(string family, string size, string style, string weight)
	{
		FontStyle fontStyle = FontSchemeStyle(style);
		FontStyle fontWeight = FontSchemeStyle(weight);
		int fontSize = int.Parse(size);
		return new Font(family, fontSize, fontStyle | fontWeight);
	}

	private bool ParsePagePreviewRequest(string[] arg)
	{
		GetFirstTourView();

		SlideLayout slideLayout = tourPage.ActiveSlideLayout;
		slideLayoutPattern = slideLayout.Pattern;
		splitterH = slideLayout.Splitters.H;
		splitterV = slideLayout.Splitters.V;

		SetTourOptionFlags();

		if (hasBanner)
			bannerSize = tour.Banner.Size;

		layoutAreaSize = tour.LayoutAreaSize;	
		
		return true;
	}

	private bool ParseRequest()
	{
		// This method recognizes different requests.  Arg 0 is always an integer
		// indicating the type of request.  The remaining args vary depending on
		// the request type.  Since an inquisitive or malicious user could attempt
		// to call this handler via their own web page (by coding the call as the
		// src value for an IMG tag), we are very careful to validate parameters
		// to ensure that this code won't crash because of an invalid argument.

		string args = context.Request.QueryString["args"];

		if (args == null)
			return false;

		// The query string has a single parameter consisting of comma delimeted values.
		string[] arg = args.Split(',');

		// The first value should be the type of preview being requested.
		int requestId;
		if (!int.TryParse(arg[0], out requestId))
			return false;
		if (Enum.IsDefined(typeof(PreviewType), requestId))
			previewType = (PreviewType)requestId;
		else
			return false;
		
		// Get the scaling dimensions.
		if (!int.TryParse(arg[1], out scaledDimension))
			return false;
		
		// Get the tour page Id. It is required for page preview, but optional for slide preview.
		int tourPageId;
		if (!int.TryParse(arg[2], out tourPageId))
			return false;
		if (tourPageId > 0)
		{
			tourPage = tour.GetTourPage(tourPageId);
			if (tourPage == null)
				return false;
		}
		else
			return false;

		// Get the draw-dimensions option.
        // 0 - Don't draw dimensions
        // 1 - Draw dimensions
		drawDimensions = arg[3] == "1";

		// Get the rest parameters for the request.
		switch (previewType)
		{
			case PreviewType.SlideLayout:
			case PreviewType.SlideLayoutInPage:
				return ParseSlideLayoutRequest(arg);
			
			case PreviewType.PagePreview:
			case PreviewType.PagePreviewThumb:
			case PreviewType.PagePreviewQuick:
				return ParsePagePreviewRequest(arg);
			
			default:
				return false;
		}
	}

	private bool ParseSlideLayoutRequest(string[] arg)
	{
		SetTourOptionFlags();
		
		tourPage = tour.SelectedTourPage;
		if (tourPage == null)
			return false;

		// Get the page's first tour view; however, instead of calling our local GetFirstTourView
		// method like we do for the other preview requests, call the tour page's GetTourView
		// method directly.  This is an optimization to force the page to load and cache its
		// views.  We do this because we know that this preview code is going to get called over
		// and over again while the various layouts are rendered and if we use GetFirstTourView
		// we could end up fetching the entire view from the database each time.  Although a page's
		// views are normally cached aleady anyway when this code is called, it is possible to for
		// a user to switch between layouts for one page and then another by being on the layouts
		// screen and then clicking different pages in the tree.  This optimization deals with that case.
		tourView = tourPage.GetTourView(tourPage.FirstTourViewId);

		// Get the type of slide layout to preview.
		int slideLayoutId;
		if (!int.TryParse(arg[4], out slideLayoutId))
			return false;
		if (Enum.IsDefined(typeof(SlideLayoutPattern), slideLayoutId))
			slideLayoutPattern = (SlideLayoutPattern)slideLayoutId;
		else
			return false;

		// Get the slide's splitter values.
		if (!int.TryParse(arg[5], out splitterH))
			return false;
		if (!int.TryParse(arg[6], out splitterV))
			return false;

		// Get the slide's layout area dimensions.
		int w;
		int h;
		if (!int.TryParse(arg[7], out w))
			return false;
		if (!int.TryParse(arg[8], out h))
			return false;

		layoutAreaSize = new Size(w, h);

		return true;
	}

	private string PixelsToPoints(string pixels)
	{
		// For now we just cut down the pixels a bit and that seems to work well enough.
		// Eventually we need to determine the screen resolution (e.g. 72dpi) and do the correct math.
		return (int.Parse(pixels) - 2).ToString();
	}

	private void SetTourOptionFlags()
	{
		hasTitle = tour.HasTitle;
		hasHeaderStripe = tour.HasHeaderStripe;
		hasFooterStripe = tour.HasFooterStripe;
		hasBanner = tour.HasBanner;
		menuLocationId = tour.MenuLocationIdEffective;

		if (colorSchemeId == 0)
		{
			// The tour style Id will be non-zero if it was passed on the query string.
			colorSchemeId = tour.ColorScheme.Id;
		}
		
		fontSchemeId = tour.FontSchemeId;
		menuStyleId = tour.MenuStyleId;
	}

	private bool RenderCachedPreview()
	{
		if (previewType == PreviewType.PagePreviewThumb)
		{
			Byte[] bytes = tourPage.ThumbnailBytes;
			if (bytes != null)
			{
				RenderPreview(bytes);
				return true;
			}
		}
		return false;
	}

	private void RenderPreview(Byte[] bytes)
	{
		context.Response.ContentType = "image/jpg";
		context.Response.BinaryWrite(bytes);
	}

	private void RenderPreview(int width, int height)
	{
		if (RenderingScaledImage)
		{
			Bitmap scaledBitmap = Utility.ScaledBitmap(bitmap, new Size(scaledDimension, scaledDimension), true);
			bitmap = scaledBitmap;
		}

		Byte[] bytes = Utility.ImageToByteArray(bitmap, ImageFormat.Jpeg);

		RenderPreview(bytes);

		if (previewType == PreviewType.PagePreviewThumb && tourPage != null)
		{
			// Save the work we just did to create a thumbnail so we won't
			// have to do it again unless the image changes.
			tourPage.ThumbnailBytes = bytes;
		}
	}
	#endregion

}