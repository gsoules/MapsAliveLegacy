// Copyright (C) 2003-2020 AvantLogic Corporation
using System;
using System.Drawing;
using System.Diagnostics;

public enum MapShape
{
	Horizontal,
	Vertical,
	Square
}

public class LayoutManager
{
	private SlideLayout layoutAreaSlideLayout;
	private bool creatingTemplateChoices;
	private Size imageAreaSize;
	private Size maxLayoutAreaInnerSize;
	private Size mapAreaSize;
	private Size mapImageSize;
	private Size oldImageAreaSize;
	private Size oldMapAreaSize;
	private Size oldScaledMapImageSize;
	private AutoLayoutOptions options;
	private SlideLayout popupSlideLayout;
	private Size scaledMapImageSize;
	private Size scaledImageSize;
	private Size textAreaSize;
	private Tour tour;
	private TourPage tourPage;
	
	// These values are known in the database -- do not change.
	private enum SlideLayoutAreaType
	{
		Map = 1,
		Image = 2,
		Text = 3
	}

	public LayoutManager(TourPage tourPage, ref SlideLayout slideLayoutForChoices)
	{
		// This constructor is called when creating layout choices.
		// Only the layout for the choices to be shown is passed in.
		this.tourPage = tourPage;
		this.tour = tourPage.Tour;
		this.layoutAreaSlideLayout = SlidesPopup ? null : slideLayoutForChoices;
		this.popupSlideLayout = SlidesPopup ? slideLayoutForChoices : null;
		creatingTemplateChoices = true;
	}

	public LayoutManager(TourPage tourPage, ref SlideLayout layoutAreaSlideLayout, ref SlideLayout popupSlideLayout)
	{
		this.tourPage = tourPage;
		this.tour = tourPage.Tour;
		this.layoutAreaSlideLayout = layoutAreaSlideLayout;
		this.popupSlideLayout = popupSlideLayout;

		SaveAreaSizes();
	}

	#region ===== Properties ========================================================

	public SlideLayout ActiveSlideLayout
	{
		get { return SlidesPopup ? popupSlideLayout : layoutAreaSlideLayout; }
	}

	private Size FirstSlideImageSize
	{
		get
		{
			Size size = Size.Empty;
			TourView firstTourView = tourPage.FirstTourView;
			if (firstTourView != null && firstTourView.HasMedia)
				size = firstTourView.GetConstrainedImageSize();

			if (size == Size.Empty)
				size = new Size(400, 300);

			return size;
		}
	}
	
	private bool ImageWidthIsLocked
	{
		get { return SplitterIsLockedV; }
	}

	private bool ImageHeightIsLocked
	{
		get { return SplitterIsLockedH; }
	}

	private bool MapWidthIsLocked
	{
		get { return SplitterIsLockedV; }
	}

	private bool MapHeightIsLocked
	{
		get { return SplitterIsLockedH; }
	}

	private int MinNonMapWidth
	{
		get { return tourPage.LayoutMinNonMapWidth; }
	}

	private int MinNonMapHeight
	{
		get { return tourPage.LayoutMinNonMapHeight; }
	}

	public Size ImageAreaMaxSize
	{
		get
		{
			SlideLayout slideLayout = ActiveSlideLayout;
			
			SlideLayoutSplitters splitters = slideLayout.Splitters;
			SplitterEdgeH splitterEdgeH = slideLayout.SplitterEdgeH;
			SplitterEdgeV splitterEdgeV = slideLayout.SplitterEdgeV;

			int width = slideLayout.ImageArea.Width;
			int height = slideLayout.ImageArea.Height;

			if (splitterEdgeH == SplitterEdgeH.ImageTop || splitterEdgeH == SplitterEdgeH.ImageBottom)
			{
				if (ImageHeightIsLocked)
				{
					height = ActiveSlideLayout.SplitterAreaHeight;
				}
				else
				{
					// The layout has image over text or text over image. The image area can grow vertically into the text area.
					height = slideLayout.InnerSize.Height - slideLayout.Spacing.H - MinNonMapHeight;

					if (height < MinNonMapHeight)
						height = ActiveSlideLayout.InnerSize.Height / 2;
				}
			}

			if (splitterEdgeV == SplitterEdgeV.ImageLeft || splitterEdgeV == SplitterEdgeV.ImageRight)
			{
				if (ImageWidthIsLocked)
				{
					width = ActiveSlideLayout.SplitterAreaWidth;
				}
				else
				{
					// The layout has image side-by-side with text.
					width = ActiveSlideLayout.InnerSize.Width - slideLayout.Spacing.V - MinNonMapWidth;

					if (width < MinNonMapWidth)
						width = ActiveSlideLayout.InnerSize.Width / 2;
				}
			}

			return new Size(width, height);
		}
	}

	static public Size MinAllowedSize
	{
		get { return new Size(8, 8); }
	}

	private bool SlidesPopup
	{
		get { return tourPage.SlidesPopup; }
	}

	private int SpacingH
	{
		get { return layoutAreaSlideLayout.HasHorizontalSplitter ? layoutAreaSlideLayout.Spacing.H : 0; }
	}

	private int SpacingV
	{
		get { return layoutAreaSlideLayout.HasVerticalSplitter ? layoutAreaSlideLayout.Spacing.V : 0; }
	}

	private bool SplitterIsLockedH
	{
		get { return ActiveSlideLayout.Splitters.LockedH && !creatingTemplateChoices; }
	}

	private bool SplitterIsLockedV
	{
		get { return ActiveSlideLayout.Splitters.LockedV && !creatingTemplateChoices; }
	}

	#endregion

	#region ===== Methods ============================================================

	public static Size CalculateMaxLayoutAreaSize(Tour tour)
	{
		// Get the width of the tour options.
		int tourOptionsW = TourLayout.CalculateWidthOfTourOptions(tour);

		// Determine the max tour width.
		int tourW = tour.WidthType == TourSizeType.Exact ? tour.TourSize.Width : tour.MaxTourSize.Width;

		// Get the height of the tour options based on the tour width. Note that the tour width
		// affects the height of the banner which is why we have to get the tour width first.
		int tourOptionsH = TourLayout.CalculateHeightOfTourOptions(tour, tourW);

		// Determine the max tour height.
		int tourH = tour.HeightType == TourSizeType.Exact ? tour.TourSize.Height : tour.MaxTourSize.Height;

		// Determine the layout area size by subtracting the tour options from the tour size.
		// If a dimension is set to Max Layout Area, use it instead.
		int layoutAreaW = tour.WidthType == TourSizeType.LayoutArea ? tour.MaxTourSize.Width : tourW - tourOptionsW;
		int layoutAreaH = tour.HeightType == TourSizeType.LayoutArea ? tour.MaxTourSize.Height : tourH - tourOptionsH;

		return new Size(layoutAreaW, layoutAreaH);
	}

	public static Size CalculateTourSizeForLayoutAreaOuterSize(Tour tour, Size layoutAreaSize)
	{
		Size tourSize = new Size();
		tourSize.Width = layoutAreaSize.Width + TourLayout.CalculateWidthOfTourOptions(tour);
		tourSize.Height = layoutAreaSize.Height + TourLayout.CalculateHeightOfTourOptions(tour, tourSize.Width);
		return tourSize;
	}

	private bool ChangePattern(SlideLayoutPattern newPattern)
	{
		// We'll change to the new pattern unless it's in the same family as the current pattern.
		// We make this check to deal with the case where the user selected a different layout
		// than the one MapsAlive chose, but their selection was in the same family. Later, 
		// they upload a new image that has the same shape as the old image -- we don't want
		// the layout to change since the current layout is fine for the image's shape.
		bool sameFamily = SlideLayout.GetFamily(newPattern) == SlideLayout.GetFamily(layoutAreaSlideLayout.Pattern);

		// We'll change to the new pattern unless the user locked one or both splitters on the old
		// pattern. We take this as an indication that the user has an investment in the current layout.
		bool splittersLocked = ActiveSlideLayout.Splitters.LockedH || ActiveSlideLayout.Splitters.LockedV;

		if (sameFamily || splittersLocked)
		{
			return false;
		}
		else
		{
			ActiveSlideLayout.Pattern = newPattern;
			return true;
		}
	}

	private void ChooseSlideLayoutPatternForNonMapLayout()
	{
		// When a new first slide image (or the only image for an Info page) is upload
		// (or the first image is uploaded) choose the best non-map layout based on
		// the image's aspect ratio.

		if (!options.OkToChooseDifferentLayout)
			return;
		
		// If the current pattern is all image or all text, don't change it.
		if (layoutAreaSlideLayout.Pattern == SlideLayoutPattern.HII || layoutAreaSlideLayout.Pattern == SlideLayoutPattern.HTT)
			return;

		SlideLayoutPattern newPattern;
		
		Size imageSize = FirstSlideImageSize;
		
		if (imageSize.Width > imageSize.Height)
			newPattern = SlideLayoutPattern.HIITT;
		else
			newPattern = SlideLayoutPattern.VIITT;

		ChangePattern(newPattern);
	}

	private void ChooseSlideLayoutPatternForMapLayout()
	{
		// When a new map image is uploaded (or the first map image is uploaded)
		// choose the best layout based on the map's aspect ratio.

		if (!options.OkToChooseDifferentLayout)
			return;

		// If the user had explicitly selected the map-only layout, don't change it.
		if (layoutAreaSlideLayout.Pattern == SlideLayoutPattern.HMM)
			return;

		// Don't change the layout for a page that has slides. It can be very disconcerting
		// if you change the map and as a result the layout and/or size of the slide image
		// and text changes.
		if (tourPage.TourViews.Count > 0)
			return;

		SlideLayoutPattern newPattern;
		MapShape mapShape = GetMapShape(mapImageSize);

		if (mapShape == MapShape.Horizontal)
		{
			// This map is horizontal.  Choose a horizontal layout.
			newPattern = SlideLayoutPattern.HMMIT;
		}
		else
		{
			// This map is square or vertical.  Choose a vertical layout.
			newPattern = SlideLayoutPattern.VMMIT;
		}

		// Change to the new pattern.
		bool patternChanged = ChangePattern(newPattern);

		if (tour.HasMoreThanOnePage)
			return;

		if (patternChanged && tour.WidthType == TourSizeType.LayoutArea && tour.HeightType == TourSizeType.LayoutArea)
		{
			SetMaxLayoutAreaInnerSize();
		}
	}

	private Size GetLayoutAreaInnerSize(Size outerSize)
	{
		SlideLayoutMargin margin = layoutAreaSlideLayout.Margin;
		Size size = Size.Empty;
		size.Width = outerSize.Width - margin.Left - margin.Right;
		size.Height = outerSize.Height - margin.Top - margin.Bottom;
		return size;
	}

	private Size GetLayoutAreaOuterSize(Size innerSize)
	{
		SlideLayoutMargin margin = layoutAreaSlideLayout.Margin;
		Size size = Size.Empty;
		size.Width = innerSize.Width + margin.Left + margin.Right;
		size.Height = innerSize.Height + margin.Top + margin.Bottom;
		return size;
	}

	private MapShape GetMapShape(Size mapImageSize)
	{
		Debug.Assert(mapImageSize != Size.Empty, "Map image size is empty");

		MapShape shape;

		// Determine the map's aspect ratio.  A ratio of 1.0 is perfectly square.
		float ratio = (float)mapImageSize.Width / (float)mapImageSize.Height;

		if (ratio >= .9 && ratio <= 1.10)
			shape = MapShape.Square;
		else if (ratio > 1.0)
			shape = MapShape.Horizontal;
		else
			shape = MapShape.Vertical;

		return shape;
	}

	private int GetLockedWidth(int targetWidth)
	{
		int maxWidth = tourPage.SlidesPopup ? popupSlideLayout.InnerSize.Width : maxLayoutAreaInnerSize.Width;
		if (targetWidth + SpacingV + SlideLayoutAreas.MinSplitter <= maxWidth)
			return targetWidth;
		else
			return maxWidth - SpacingV - SlideLayoutAreas.MinSplitter;
	}

	private int GetLockedHeight(int targetHeight)
	{
		int maxHeight = tourPage.SlidesPopup ? popupSlideLayout.InnerSize.Height : maxLayoutAreaInnerSize.Height;
		if (targetHeight + SpacingH + SlideLayoutAreas.MinSplitter <= maxHeight)
			return targetHeight;
		else
			return maxHeight - SpacingH - SlideLayoutAreas.MinSplitter;
	}

	public void LayoutAreaSlideLayoutSizeChanged(Size newOuterSize)
	{
		// Move the splitters in the new sized layout so they are in the same relative
		// positions as they were in the old layout.  For instance, if the vertical splitter
		// at the old size is 25% in from the left size, it will be 25% in at the new size.
		SlideLayoutSplitters newSplitters = MoveSplittersRelativeToOldLayoutAreaSize(newOuterSize);

		layoutAreaSlideLayout.SetNewOuterSize(newOuterSize);
		layoutAreaSlideLayout.SetNewSplitters(newSplitters);

		tourPage.SetLayoutChanged();
	}

	private SlideLayoutSplitters MoveSplittersRelativeToOldLayoutAreaSize(Size newLayoutAreaSize)
	{
		int newH = -1;
		int newV = -1;

		Size oldInnerSize = ActiveSlideLayout.InnerSize;

		SlideLayoutMargin margin = ActiveSlideLayout.Margin;
		SlideLayoutSplitters splitters = ActiveSlideLayout.Splitters;
		
		int width = newLayoutAreaSize.Width - margin.Left - margin.Right;
		int height = newLayoutAreaSize.Height - margin.Top - margin.Bottom;
		Size newInnerSize = new Size(width, height);

		// Determine the percentage location of each splitter.
		if (ActiveSlideLayout.HasHorizontalSplitter)
		{
			if (splitters.LockedH)
			{
				newH = splitters.H;
			}
			else
			{
				double pctH = Utility.PixelToPercent(splitters.H, oldInnerSize.Height);
				newH = Utility.PercentToPixel(pctH, newInnerSize.Height);
			}
		}
		if (ActiveSlideLayout.HasVerticalSplitter)
		{
			if (splitters.LockedV)
			{
				newV = splitters.V;
			}
			else
			{
				double pctV = Utility.PixelToPercent(splitters.V, oldInnerSize.Width);
				newV = Utility.PercentToPixel(pctV, newInnerSize.Width);
			}
		}

		return new SlideLayoutSplitters(newH, newV, splitters.LockedH, splitters.LockedV);
	}

	public void PerformAutoLayout(AutoLayoutOptions options)
	{
        if (!tour.AutoLayoutEnabled && !options.OkToAdjustSlideLayout)
			return;

        // Limit the V4 auto layout options to only adjusting the tour height if an option
        // changes such as adding or removing the banner, top menu, or tour title.
        if (tour.V4)
        {
            options.WidthType = TourSizeType.Exact;
            options.HeightType = TourSizeType.Exact;
            options.OkToChooseDifferentLayout = false;
        }

        // Get the options that control how the auto layout will behave.
        this.options = options;

		// Save the current sizes of area and initialize variables that all layout methods use.
		InitLayoutSizes();

		if (options.OkToAdjustSlideLayout)
		{
			// Determine how big the max tour size should be based on the map shape
			// and whether or not its a user map, sample map, or Ready Map.
			MapShape mapShape = GetMapShape(mapImageSize);
			
			if (SlidesPopup)
			{
				// Popup slides have two layouts: one for the popup and one for the map.
				if (!creatingTemplateChoices)
				{
					// Lay out the map for the pop up slide. We don't do this when creating
					// layout choices because the choices only show the popup, not the map.
					SetMaxLayoutAreaInnerSize();
					RunAutoLayoutForMapOnlyLayout();
				}

				// Lay out the pop up slide's image and text areas.
				ChooseSlideLayoutPatternForNonMapLayout();
				RunAutoLayoutForImageAndTextLayout(popupSlideLayout, popupSlideLayout.InnerSize);
			}
			else
			{
				// Start with a layout area size that is as large as the user allows.
				SetMaxLayoutAreaInnerSize();

				if (layoutAreaSlideLayout.HasMapArea)
				{
					// Lay out the fixed slide's map, image, and text areas.
					ChooseSlideLayoutPatternForMapLayout();
					RunAutoLayoutForFixedSlideLayout();
				}
				else
				{
					// Lay out the non-map fixed slide's image and text areas.
					ChooseSlideLayoutPatternForNonMapLayout();
					RunAutoLayoutForImageAndTextLayout(layoutAreaSlideLayout, maxLayoutAreaInnerSize);
				}
			}
		}

		// Determine what sizes changed and tell the tour and tour page about the changes.
		UpdateTourPage();
	}

	public void PerformAutoLayoutForBannerChange()
	{
		if (!tour.AutoLayoutEnabled)
			return;

		AutoLayoutOptions options = new AutoLayoutOptions();
		
		// Never change the tour width when the banner changes.
		options.WidthType = TourSizeType.Exact;

		if (tour.HeightType == TourSizeType.LayoutArea)
		{
			options.HeightType = TourSizeType.LayoutArea;
			options.OkToAdjustSlideLayout = false;
		}
		else
		{
			options.HeightType = tour.HasMoreThanOnePage ? TourSizeType.Exact : tour.HeightType;
		}
		
		PerformAutoLayout(options);
	}

	public void PerformAutoLayoutForCurrentPage()
	{
        AutoLayoutOptions options = new AutoLayoutOptions();
		options.WidthType = tour.HasMoreThanOnePage ? TourSizeType.Exact : tour.WidthType;
		options.HeightType = tour.HasMoreThanOnePage ? TourSizeType.Exact : tour.HeightType;
        options.OkToAdjustSlideLayout = false;
		PerformAutoLayout(options);
		if (!tourPage.SlidesPopup)
			tourPage.InvalidateThumbnail();
	}

	public void PerformAutoLayoutForLayoutFamily(bool tourWidthLocked, bool tourHeightLocked)
	{
		AutoLayoutOptions options = new AutoLayoutOptions();
		options.WidthType = tourWidthLocked ? TourSizeType.Exact : TourSizeType.LayoutArea;
		options.HeightType = tourHeightLocked ? TourSizeType.Exact : TourSizeType.LayoutArea;
		PerformAutoLayout(options);
	}

	public void PerformAutoLayoutForNewMapImage()
	{
		AutoLayoutOptions options = new AutoLayoutOptions();
		options.WidthType = tour.HasMoreThanOnePage ? TourSizeType.Exact : tour.WidthType;
		options.HeightType = tour.HasMoreThanOnePage ? TourSizeType.Exact : tour.HeightType;
		
		// Only choose a new layout for fixed layouts. If it's a popup layout and the user
		// uploads a new map, we don't want to change the popup's layout.
		options.OkToChooseDifferentLayout = tourPage.Tour.AutoLayoutEnabled && !tourPage.SlidesPopup;

        if (tourPage.SlidesPopup)
            options.OkToAdjustSlideLayout = false;
		
		PerformAutoLayout(options);
	}

	public void PerformAutoLayoutForNewSlideImage()
	{
		AutoLayoutOptions options = new AutoLayoutOptions();
		options.WidthType = tour.HasMoreThanOnePage ? TourSizeType.Exact : tour.WidthType;
		options.HeightType = tour.HasMoreThanOnePage ? TourSizeType.Exact : tour.HeightType;

		// If the layout is image and text with no map, choose the best layout
		// based on whether the image is tall or wide.
		options.OkToChooseDifferentLayout =
			!SlidesPopup &&
			(ActiveSlideLayout.Family == SlideLayoutFamily.NoMapImageV ||
			ActiveSlideLayout.Family == SlideLayoutFamily.NoMapImageH);

		tourPage.LayoutManager.PerformAutoLayout(options);
	}

	public void PerformAutoLayoutOnOtherPages(TourPage tourPageToIgnore)
	{
		foreach (TourPage otherPage in tour.TourPages)
		{
			if (otherPage.Id == tourPageToIgnore.Id)
				continue;

			AutoLayoutOptions options = new AutoLayoutOptions();
			options.WidthType = TourSizeType.Exact;
			options.HeightType = TourSizeType.Exact;
			otherPage.LayoutManager.PerformAutoLayout(options);

			otherPage.InvalidateThumbnail();
		}
	}

	public void PerformAutoLayoutForTourOptionChanges()
	{
		if (!tour.AutoLayoutEnabled)
			return;

		AutoLayoutOptions options = new AutoLayoutOptions();
		if (tour.WidthType == TourSizeType.LayoutArea && tour.HeightType == TourSizeType.LayoutArea)
		{
			// When both dimension types are layout area, we only want to change the tour size and not the layout.
			options.WidthType = TourSizeType.LayoutArea;
			options.HeightType = TourSizeType.LayoutArea;
			options.OkToAdjustSlideLayout = false;
		}
		else
		{
			// When either dimension is not layout area, we allow the layout to change as necessary.
			options.WidthType = tour.HasMoreThanOnePage ? TourSizeType.Exact : tour.WidthType;
			options.HeightType = tour.HasMoreThanOnePage ? TourSizeType.Exact : tour.HeightType;
		}

		PerformAutoLayout(options);
	}

	public void PopupSizeChanged(Size newOuterSize)
	{
		popupSlideLayout.SetNewOuterSize(newOuterSize);
		tourPage.SetLayoutChanged();
	}
	
	private void ReduceMaxLayoutAreaInnerSize(int newWidth, int newHeight)
	{
		// This method will make the max layout area size smaller, but not allow it to get larger.
		if (!options.TourWidthLocked && newWidth < maxLayoutAreaInnerSize.Width)
			maxLayoutAreaInnerSize.Width = newWidth;
		
		if (!options.TourHeightLocked && newHeight < maxLayoutAreaInnerSize.Height)
			maxLayoutAreaInnerSize.Height = newHeight;
	}

	private void RunAutoLayoutForFixedSlideLayout()
	{
		switch (ActiveSlideLayout.Family)
		{
			case SlideLayoutFamily.MapV:
			case SlideLayoutFamily.MapVI:
			case SlideLayoutFamily.MapVT:
				RunAutoLayoutForFamilyMapV();
				break;

			case SlideLayoutFamily.MapH:
			case SlideLayoutFamily.MapHI:
			case SlideLayoutFamily.MapHT:
				RunAutoLayoutForFamilyMapH();
				break;

			case SlideLayoutFamily.ImageV:
			case SlideLayoutFamily.TextH:
				RunAutoLayoutForFamilyImageVOrTextH();
				break;

			case SlideLayoutFamily.ImageH:
			case SlideLayoutFamily.TextV:
				RunAutoLayoutForFamilyImageHOrTextV();
				break;

			case SlideLayoutFamily.MapOnly:
				RunAutoLayoutForMapOnlyLayout();
				break;

			default:
				Debug.Fail("Unexpected family " + ActiveSlideLayout.Family);
				return;
		}
	}

	private void RunAutoLayoutForFamilyImageHOrTextV()
	{
		// ImageH is a map-in-corner layout where the map and text are in the top or bottom row.
		// Opposite the map/text is a row containing the image.

		// TextV is a map-in-corner layout where the map and image are in the left or right column.
		// Opposite the map/image is a column containing the text.

		// What these two families have in common is that the tour width is the map width plus
		// min text width, and the tour height is the map height plus image height. Also, the
		// horizontal splitter controls the image and the vertical splitter controls the map.

		// Determine the width of the map area:
		if (MapWidthIsLocked)
		{
			mapAreaSize.Width = GetLockedWidth(layoutAreaSlideLayout.MapArea.Width);
		}
		else
		{
			// Start by setting the map area to its max possible width.
			mapAreaSize.Width = maxLayoutAreaInnerSize.Width - SpacingH - MinNonMapWidth;
		}

		// Determine the height of the map area:
		if (ImageHeightIsLocked)
		{
			// The map height is locked which means the map area height is the height not used by the image.
			mapAreaSize.Height = GetLockedHeight(maxLayoutAreaInnerSize.Height - SpacingH - layoutAreaSlideLayout.ImageArea.Height);
		}
		else
		{
			// Start by setting the map area to its max possible height.
			mapAreaSize.Height = maxLayoutAreaInnerSize.Height - SpacingH - MinNonMapHeight;
		}

		// Scale the map image to fit the map area.
		scaledMapImageSize = Utility.ScaledImageSize(mapImageSize, mapAreaSize);

		// If the scaled map image does not use the full width of the map area, reduce the map area width.
		if (!MapWidthIsLocked && scaledMapImageSize.Width < mapAreaSize.Width)
			mapAreaSize.Width = scaledMapImageSize.Width;

		// If the scaled map image does not use the full height of the map area, reduce the map area height.
		if (scaledMapImageSize.Height < mapAreaSize.Height && !tourPage.IsGallery)
			mapAreaSize.Height = scaledMapImageSize.Height;

		// We now have our map area size. Determine the size of the non-map (image and/or text) areas.

		// Determine the image area width.
		if (layoutAreaSlideLayout.Family == SlideLayoutFamily.ImageH)
		{
			// The image area width for family ImageH is the layout area width.
			imageAreaSize.Width = maxLayoutAreaInnerSize.Width;
		}
		else
		{
			// The image area width for family TextV is the map area width.
			imageAreaSize.Width = mapAreaSize.Width;
		}

		// Determine the image area height.
		if (ImageHeightIsLocked)
		{
			imageAreaSize.Height = GetLockedHeight(layoutAreaSlideLayout.ImageArea.Height);
		}
		else
		{
			imageAreaSize.Height = maxLayoutAreaInnerSize.Height - SpacingH - mapAreaSize.Height;
		}

		// Scale the image to fit within the image area.
		scaledImageSize = Utility.ScaledImageSize(FirstSlideImageSize, imageAreaSize);

		// If the image is not tall enough, reduce the height of the image area.
		if (!ImageHeightIsLocked && scaledImageSize.Height < imageAreaSize.Height)
		{
			if (scaledImageSize.Height >= MinNonMapHeight)
				imageAreaSize.Height = scaledImageSize.Height;
			else
				imageAreaSize.Height = MinNonMapHeight;
		}

		// Determine the size of the text area.
		textAreaSize.Width = MinNonMapWidth;

		// Now we have the size of the map, image, and text areas. Determine the layout area size.
		int mapAndTextWidth = mapAreaSize.Width + SpacingV + textAreaSize.Width;
		int newLayoutAreaWidth = layoutAreaSlideLayout.Family == SlideLayoutFamily.ImageH ?
			Math.Max(mapAndTextWidth, imageAreaSize.Width) : mapAndTextWidth;
		SetAutoLayoutLayoutAreaInnerSize(newLayoutAreaWidth, mapAreaSize.Height + SpacingH + imageAreaSize.Height);

		// Calculate the splitter values for the map and image areas.
		SetAutoLayoutSplitters(imageAreaSize.Height, mapAreaSize.Width);
	}

	private void RunAutoLayoutForFamilyImageVOrTextH()
	{
		// ImageV is a map-in-corner layout where the map and text are in the left or right column.
		// Opposite the map/text is a column containing the image.

		// TextH is a map-in-corner layout where the map and image are in the top or bottom row.
		// Opposite the map/image is a row containing the text.

		// What these two families have in common is that the tour height is the map height plus
		// min text height, and the tour width is the map width plus image width. Also, the
		// vertical splitter controls the image and the horizontal splitter controls the map.

		// Determine the width of the map area:
		if (ImageWidthIsLocked)
		{
			// The image width is locked which means the map area width is the width not used by the image.
			mapAreaSize.Width = GetLockedWidth(maxLayoutAreaInnerSize.Width - SpacingV - layoutAreaSlideLayout.ImageArea.Width);
		}
		else
		{
			// Start by setting the map area to its max possible width.
			mapAreaSize.Width = maxLayoutAreaInnerSize.Width - SpacingV - MinNonMapWidth;
		}

		// Determine the height of the map area:
		if (MapHeightIsLocked)
		{
			// The map area height is locked.
			mapAreaSize.Height = GetLockedHeight(layoutAreaSlideLayout.MapArea.Height);
		}
		else
		{
			// Start by setting the map area to its max possible height.
			mapAreaSize.Height = maxLayoutAreaInnerSize.Height - SpacingH - MinNonMapHeight;
		}

		// Scale the map image to fit the map area.
		scaledMapImageSize = Utility.ScaledImageSize(mapImageSize, mapAreaSize);

		// If the scaled map image does not use the full width of the map area, reduce the map area width.
		if (scaledMapImageSize.Width < mapAreaSize.Width)
			mapAreaSize.Width = scaledMapImageSize.Width;

		// If the scaled map image does not use the full height of the map area, reduce the map area height.
		if (!MapHeightIsLocked && scaledMapImageSize.Height < mapAreaSize.Height)
			mapAreaSize.Height = scaledMapImageSize.Height;

		// We now have our map area size. Determine the size of the non-map (image and/or text) areas.

		// Start by setting the image area to its max possible width.
		if (ImageWidthIsLocked)
		{
			imageAreaSize.Width = layoutAreaSlideLayout.ImageArea.Width;
		}
		else
		{
			imageAreaSize.Width = maxLayoutAreaInnerSize.Width - SpacingV - mapAreaSize.Width;
		}

		// Determine the image area height.
		if (layoutAreaSlideLayout.Family == SlideLayoutFamily.ImageV)
		{
			// The image area height for family ImageV is the layout area height.
			imageAreaSize.Height = maxLayoutAreaInnerSize.Height;
		}
		else
		{
			// The image area height for family TextH is the map area height.
			imageAreaSize.Height = mapAreaSize.Height;
		}

		// Scale the image to fit within the image area.
		scaledImageSize = Utility.ScaledImageSize(FirstSlideImageSize, imageAreaSize);

		// If the image is not wide enough, reduce the width of the image area.
		if (!ImageWidthIsLocked && scaledImageSize.Width < imageAreaSize.Width)
		{
			if (scaledImageSize.Width >= MinNonMapWidth)
				imageAreaSize.Width = scaledImageSize.Width;
			else
				imageAreaSize.Width = MinNonMapWidth;
		}

		// Determine the height of the image area for family ImageV.
		if (layoutAreaSlideLayout.Family == SlideLayoutFamily.ImageV)
		{
			int minImageHeight = mapAreaSize.Height + SpacingH + MinNonMapHeight;
			if (scaledImageSize.Height < minImageHeight)
				imageAreaSize.Height = MinNonMapHeight;
			else if (scaledImageSize.Height < maxLayoutAreaInnerSize.Height)
				imageAreaSize.Height = scaledImageSize.Height;
		}

		// Determine the size of the text area.
		textAreaSize.Height = MinNonMapHeight;

		// Now we have the size of the map, image, and text areas. Determine the layout area size.
		int mapAndTextHeight = mapAreaSize.Height + SpacingH + textAreaSize.Height;
		int newLayoutHeight = layoutAreaSlideLayout.Family == SlideLayoutFamily.ImageV ?
			Math.Max(mapAndTextHeight, imageAreaSize.Height) : mapAndTextHeight;
		SetAutoLayoutLayoutAreaInnerSize(mapAreaSize.Width + SpacingV + imageAreaSize.Width, newLayoutHeight);

		// Calculate the splitter values for the map and image areas.
		SetAutoLayoutSplitters(mapAreaSize.Height, imageAreaSize.Width);
	}

	private void RunAutoLayoutForImageAndTextLayout(SlideLayout slideLayout, Size maxInnerSize)
	{
		// Determine the size of the image area.
		switch (slideLayout.Family)
		{
			case SlideLayoutFamily.NoMapImageH:
				// The layout for this family is image over text, or text over image.
				imageAreaSize.Width = maxInnerSize.Width;

				if (ImageHeightIsLocked)
				{
					imageAreaSize.Height = GetLockedHeight(slideLayout.SplitterAreaHeight);
				}
				else
				{
					imageAreaSize.Height = maxInnerSize.Height - slideLayout.Spacing.H - MinNonMapHeight;
					if (imageAreaSize.Height < MinNonMapHeight)
						imageAreaSize.Height = maxInnerSize.Height / 2;
				}
				break;

			case SlideLayoutFamily.NoMapImageV:
				// The layout for this family is side-by-side image and text, or text and image.
				imageAreaSize.Height = maxInnerSize.Height;

				if (ImageWidthIsLocked)
				{
					imageAreaSize.Width = GetLockedWidth(slideLayout.SplitterAreaWidth);
				}
				else
				{
					imageAreaSize.Width = maxInnerSize.Width - slideLayout.Spacing.V - MinNonMapWidth;
					if (imageAreaSize.Width < MinNonMapWidth)
						imageAreaSize.Width = maxInnerSize.Width / 2;
				}

				break;

			case SlideLayoutFamily.ImageOnly:
			case SlideLayoutFamily.TextOnly:
				// The layout is either all image or all text.
				if (slideLayout.Pattern == SlideLayoutPattern.HTT)
				{
					imageAreaSize = Size.Empty;
				}
				else if (slideLayout.Pattern == SlideLayoutPattern.HII)
				{
					imageAreaSize = tourPage.SlidesPopup ? popupSlideLayout.InnerSize : layoutAreaSlideLayout.InnerSize;
					scaledImageSize = Utility.ScaledImageSize(FirstSlideImageSize, imageAreaSize);
				}
				else
				{
					Debug.Fail("Unexpected layout " + slideLayout.Pattern);
				}
				break;

			default:
				Debug.Fail("Unexpected family " + slideLayout.Family);
				break;
		}

		// Now we know the size of the image area. Scale the image to fit and then adjust
		// the image area to elimnate any space to the right or below the image.
		if (slideLayout.HasImageArea)
		{
			scaledImageSize = Utility.ScaledImageSize(FirstSlideImageSize, imageAreaSize);

			if (!ImageWidthIsLocked && scaledImageSize.Width < imageAreaSize.Width)
				imageAreaSize.Width = scaledImageSize.Width;

			if (!ImageHeightIsLocked && scaledImageSize.Height < imageAreaSize.Height)
				imageAreaSize.Height = scaledImageSize.Height;
		}

		if (!tourPage.SlidesPopup)
		{
			// The layout is either for an Info page, or a Map page that has no map.
			// Treat it the same as a fixed layout page to determine the layout area size.
			int layoutAreaHeight;
			int layoutAreaWidth;

			if (slideLayout.HasImageArea)
			{
				if (slideLayout.Family == SlideLayoutFamily.NoMapImageH)
				{
					// The image is the full width of the layout area.
					layoutAreaWidth = Math.Max(scaledImageSize.Width, SlideLayoutAreas.MinSplitter);
					layoutAreaHeight = scaledImageSize.Height + SpacingH + MinNonMapHeight;
				}
				else if (slideLayout.Family == SlideLayoutFamily.NoMapImageV)
				{
					// The image is the full height of the layout area.
					layoutAreaWidth = scaledImageSize.Width + SpacingV + MinNonMapWidth;
					layoutAreaHeight = Math.Max(scaledImageSize.Height, SlideLayoutAreas.MinSplitter);
				}
				else
				{
					// The image is the entire layout area.
					layoutAreaWidth = Math.Max(scaledImageSize.Width, SlideLayoutAreas.MinSplitter);
					layoutAreaHeight = Math.Max(scaledImageSize.Height, SlideLayoutAreas.MinSplitter);
				}
			}
			else
			{
				// The text is the entire layout area.
				layoutAreaWidth = maxInnerSize.Width;
				layoutAreaHeight = maxInnerSize.Height;
			}

			SetAutoLayoutLayoutAreaInnerSize(layoutAreaWidth, layoutAreaHeight);
		}

		SetAutoLayoutSplitters(slideLayout.HasHorizontalSplitter ? imageAreaSize.Height : 0, slideLayout.HasVerticalSplitter ? imageAreaSize.Width : 0);
	}

	private void RunAutoLayoutForFamilyMapH()
	{
		// This layout has the map going left to right on either the top or bottom row.
		// Opposite the map is a row containing side-by-side image/text, or text/image.

		// In this layout, the map runs horizontally, so the width of the map area is the width of the layout area.
		mapAreaSize.Width = maxLayoutAreaInnerSize.Width;

		// Determine the height of the map area:
		if (MapHeightIsLocked)
		{
			// The map area height is locked.
			mapAreaSize.Height = GetLockedHeight(layoutAreaSlideLayout.MapArea.Height);

			// Scale the map image to fit the map area.
			scaledMapImageSize = Utility.ScaledImageSize(mapImageSize, mapAreaSize);
		}
		else
		{
			// The height of the map area is the layout area height minus the height of the image area.
			int imageAreaHeight;

			if (ImageWidthIsLocked)
			{
				// The image width is locked so we have to compute the image height based on that width.
				Size tempImageAreaSize = Size.Empty;
				tempImageAreaSize.Width = layoutAreaSlideLayout.SplitterAreaWidth;
				tempImageAreaSize.Height = maxLayoutAreaInnerSize.Height - SpacingH - SlideLayoutAreas.MinSplitter;
				imageAreaHeight = Utility.ScaledImageSize(FirstSlideImageSize, tempImageAreaSize).Height;
			}
			else
			{
				imageAreaHeight = MinNonMapHeight;
			}

			// Now that we know the image area height we can set the map area height.
			mapAreaSize.Height = maxLayoutAreaInnerSize.Height - SpacingH - imageAreaHeight;

			// Scale the map image to fit the map area.
			scaledMapImageSize = Utility.ScaledImageSize(mapImageSize, mapAreaSize);

			// If the scaled image does not use the full width of the layout area, reduce the layout area width.
			if (scaledMapImageSize.Width < maxLayoutAreaInnerSize.Width)
			{
				int minImageWidth = ImageWidthIsLocked ? layoutAreaSlideLayout.SplitterAreaWidth : SlideLayoutAreas.MinSplitter;
				int newWidth = Math.Max(scaledMapImageSize.Width, minImageWidth + SpacingV + MinNonMapWidth);
				ReduceMaxLayoutAreaInnerSize(newWidth, maxLayoutAreaInnerSize.Height);
			}
		}

		// If the map scaled image does not use the full height of the map area, reduce the map area height.
		if (scaledMapImageSize.Height < mapAreaSize.Height)
			mapAreaSize.Height = scaledMapImageSize.Height;

		// Set the map area width to match the map image width. We do this for algorithmic purposes
		// even though in reality the map area width for this layout is the layout area width.
		mapAreaSize.Width = scaledMapImageSize.Width;

		// We now have our map area size. Determine the size of the non-map (image and/or text) areas.
		int nonMapAreaHeight = maxLayoutAreaInnerSize.Height - SpacingH - mapAreaSize.Height;

		if (layoutAreaSlideLayout.HasImageArea)
		{
			// Start by setting the image area to its max possible height.
			imageAreaSize.Height = nonMapAreaHeight;

			// Determine the image width.
			if (ImageWidthIsLocked)
			{
				imageAreaSize.Width = GetLockedWidth(layoutAreaSlideLayout.SplitterAreaWidth);
			}
			else
			{
				if (layoutAreaSlideLayout.HasTextArea)
					imageAreaSize.Width = maxLayoutAreaInnerSize.Width - SpacingV - MinNonMapWidth;
				else
					imageAreaSize.Width = mapAreaSize.Width;
			}

			// Scale the image to fit within the image area.
			scaledImageSize = Utility.ScaledImageSize(FirstSlideImageSize, imageAreaSize);

			// If the image is not tall enough, reduce the height of the image area.
			if (scaledImageSize.Height < nonMapAreaHeight)
			{
				if (scaledImageSize.Height >= MinNonMapHeight)
					nonMapAreaHeight = scaledImageSize.Height;
				else
				{
					nonMapAreaHeight = MinNonMapHeight;

					// Make sure there is enough room for the non map height;
					if (nonMapAreaHeight > maxLayoutAreaInnerSize.Height - mapAreaSize.Height - SpacingH)
						nonMapAreaHeight = maxLayoutAreaInnerSize.Height - mapAreaSize.Height - SpacingH;
				}

				imageAreaSize.Height = nonMapAreaHeight;
			}

			// Determine the size of the text area.
			if (layoutAreaSlideLayout.HasTextArea)
			{
				// Start by setting the text area to the layout area width minus the image area width.
				textAreaSize.Width = maxLayoutAreaInnerSize.Width - SpacingV - imageAreaSize.Width;
				textAreaSize.Height = nonMapAreaHeight;

				// If the image is not wide enough, make the image area narrow and the text area wider.
				if (!ImageWidthIsLocked)
				{
					int extraImageAreaWidth = imageAreaSize.Width - scaledImageSize.Width;

					if (extraImageAreaWidth > 0)
					{
						imageAreaSize.Width -= extraImageAreaWidth;
						textAreaSize.Width += extraImageAreaWidth;
					}
				}

				// Narrow the text area width to the minimum required.
				int extraTextAreaWidth = maxLayoutAreaInnerSize.Width - SpacingV - imageAreaSize.Width - MinNonMapWidth;
				if (extraTextAreaWidth > 0)
					textAreaSize.Width -= extraTextAreaWidth;
			}
		}
		else
		{
			// There is no image area so use all the non-map area for text.
			textAreaSize.Height = nonMapAreaHeight;
			textAreaSize.Width = mapAreaSize.Width;
		}

		// Now we have the size of the map, image, and text areas. Determine the layout area size.
		SetAutoLayoutLayoutAreaInnerSize(
			Math.Max(mapAreaSize.Width, imageAreaSize.Width + SpacingV + textAreaSize.Width),
			mapAreaSize.Height + SpacingH + nonMapAreaHeight);

		// Calculate the splitter values for the map and image areas.
		SetAutoLayoutSplitters(mapAreaSize.Height, imageAreaSize.Width);
	}

	private void RunAutoLayoutForFamilyMapV()
	{
		// This layout has the map going up and down on either the left or right column.
		// Opposite the map is a column containing image over text, or text over image.

		// In this layout, the map runs vertically, so the height of the map area is the height of the layout area.
		mapAreaSize.Height = maxLayoutAreaInnerSize.Height;
		
		// Determine the width of the map area:
		if (MapWidthIsLocked)
		{
			// The map area width is locked.
			mapAreaSize.Width = GetLockedWidth(layoutAreaSlideLayout.MapArea.Width);
			
			// Scale the map image to fit the map area.
			scaledMapImageSize = Utility.ScaledImageSize(mapImageSize, mapAreaSize);
		}
		else
		{
			// The width of the map area is the layout area width minus the width of the image area.
			int imageAreaWidth;
			
			if (ImageHeightIsLocked)
			{
				// The image height is locked so we have to compute the image width based on that height.
				Size tempImageAreaSize = Size.Empty;
				tempImageAreaSize.Width = maxLayoutAreaInnerSize.Width - SpacingV - SlideLayoutAreas.MinSplitter;
				tempImageAreaSize.Height = layoutAreaSlideLayout.SplitterAreaHeight;
				imageAreaWidth = Utility.ScaledImageSize(FirstSlideImageSize, tempImageAreaSize).Width;
			}
			else
			{
				imageAreaWidth = MinNonMapWidth;
			}

			// Now that we know the image area width we can set the map area width.
			if (tourPage.IsGallery)
			{
				// Make the gallery area wide enough for columns of markers that are 100px wide
				// with 8px margins. The hope is to get a good default result using the Classic marker.
				const int columnWidth = 108;
				int columns = 1;
				if (tour.TourSize.Width >= 1000 )
					columns = 4;
				else if (tour.TourSize.Width >= 800)
					columns = 3;
				else if (tour.TourSize.Width >= 500)
					columns = 2;

				mapAreaSize.Width = (columns * columnWidth) + 16;
			}
			else
			{
				mapAreaSize.Width = maxLayoutAreaInnerSize.Width - SpacingV - imageAreaWidth;
			}

			// Scale the map image to fit the map area.
			scaledMapImageSize = Utility.ScaledImageSize(mapImageSize, mapAreaSize);

			// If the scaled map image does not use the full height of the layout area, reduce the layout area height.
			if (scaledMapImageSize.Height < maxLayoutAreaInnerSize.Height)
			{
				int minImageHeight = ImageHeightIsLocked ? layoutAreaSlideLayout.SplitterAreaHeight : SlideLayoutAreas.MinSplitter;
				int newHeight = Math.Max(scaledMapImageSize.Height, minImageHeight + SpacingH + MinNonMapHeight);
				ReduceMaxLayoutAreaInnerSize(maxLayoutAreaInnerSize.Width, newHeight);
			}
		}

		// If the scaled map image does not use the full width of the map area, reduce the map area width.
		if (scaledMapImageSize.Width < mapAreaSize.Width && !tourPage.IsGallery)
			mapAreaSize.Width = scaledMapImageSize.Width;

		// Set the map area height to match the map image height. We do this for algorithmic purposes
		// even though in reality the map area height for this layout is the layout area height.
		mapAreaSize.Height = scaledMapImageSize.Height;

		// We now have our map area size. Determine the size of the non-map (image and/or text) areas.
		int nonMapAreaWidth = maxLayoutAreaInnerSize.Width - SpacingV - mapAreaSize.Width;
		
		if (layoutAreaSlideLayout.HasImageArea)
		{
			// Start by setting the image area to its max possible width.
			imageAreaSize.Width = nonMapAreaWidth;

			// Determine the image height.
			if (ImageHeightIsLocked)
			{
				imageAreaSize.Height = GetLockedHeight(layoutAreaSlideLayout.SplitterAreaHeight);
			}
			else
			{
				if (layoutAreaSlideLayout.HasTextArea)
					imageAreaSize.Height = maxLayoutAreaInnerSize.Height - SpacingH - MinNonMapHeight;
				else
					imageAreaSize.Height = maxLayoutAreaInnerSize.Height;

				// Scale the image to fit within the image area.
				scaledImageSize = Utility.ScaledImageSize(FirstSlideImageSize, imageAreaSize);

				// If the image is not wide enough, narrow the image area.
				if (scaledImageSize.Width < nonMapAreaWidth)
				{
					if (scaledImageSize.Width >= MinNonMapWidth)
						nonMapAreaWidth = scaledImageSize.Width;
					else
					{
						nonMapAreaWidth = MinNonMapWidth;

						// Make sure there is enough room for the non map width;
						if (nonMapAreaWidth > maxLayoutAreaInnerSize.Width - mapAreaSize.Width - SpacingV)
							nonMapAreaWidth = maxLayoutAreaInnerSize.Width - mapAreaSize.Width - SpacingV;
					}

					imageAreaSize.Width = nonMapAreaWidth;
				}

				// If the image is not tall enough, shorten the image area.
				if (scaledImageSize.Height < imageAreaSize.Height)
				{
					imageAreaSize.Height = scaledImageSize.Height;
				}
			}

			// Determine the size of the text area.
			if (layoutAreaSlideLayout.HasTextArea)
			{
				// Start by setting the text area to the layout area height minus the image area height.
				textAreaSize.Width = nonMapAreaWidth;
				textAreaSize.Height = maxLayoutAreaInnerSize.Height - SpacingH - imageAreaSize.Height;

				// If the image is not tall enough, make the image area shorter and the text area taller.
				if (!ImageHeightIsLocked)
				{
					int extraImageAreaHeight = imageAreaSize.Height - scaledImageSize.Height;

					if (extraImageAreaHeight > 0)
					{
						imageAreaSize.Height -= extraImageAreaHeight;
						textAreaSize.Height += extraImageAreaHeight;
					}
				}

				// Shorten the text area height to the minimum required.
				int extraTextAreaHeight = maxLayoutAreaInnerSize.Height - SpacingH - imageAreaSize.Height - MinNonMapHeight;
				if (extraTextAreaHeight > 0)
					textAreaSize.Height -= extraTextAreaHeight;
			}
		}
		else
		{
			// There is no image area so use all the non-map area for text.
			textAreaSize.Width = nonMapAreaWidth;
			textAreaSize.Height = mapAreaSize.Height;
		}

		// Now we have the size of the map, image, and text areas. Determine the layout area size.
		SetAutoLayoutLayoutAreaInnerSize(
			mapAreaSize.Width + SpacingV + nonMapAreaWidth,
			Math.Max(mapAreaSize.Height, imageAreaSize.Height + SpacingH + textAreaSize.Height));
		
		// Calculate the splitter values for the map and image areas.
		SetAutoLayoutSplitters(imageAreaSize.Height, mapAreaSize.Width);
	}

	private void RunAutoLayoutForMapOnlyLayout()
	{
		// Set the new scaled map image size and map area sizes. They will be compared later to the old sizes.
		scaledMapImageSize = Utility.ScaledImageSize(mapImageSize, maxLayoutAreaInnerSize);
		mapAreaSize = scaledMapImageSize;

		// Set the size of the layout area to match the size of the map.
		SetAutoLayoutLayoutAreaInnerSize(mapAreaSize.Width, mapAreaSize.Height);
	}

	private void InitLayoutSizes()
	{
		// Put the map image size into a convenience variable that we can use everywhere.
		if (tourPage.MapImage.HasFile)
		{
			mapImageSize = tourPage.MapImage.Size;
		}
		else
		{
			mapImageSize = new Size(400, 400);
		}

		if (creatingTemplateChoices)
			return;

		// Clear variables that all auto layout methods use.
		mapAreaSize = Size.Empty;
		imageAreaSize = Size.Empty;
		textAreaSize = Size.Empty;
		scaledMapImageSize = Size.Empty;
		scaledImageSize = Size.Empty;
	}

	private void SaveAreaSizes()
	{
		oldMapAreaSize = SlidesPopup ? layoutAreaSlideLayout.InnerSize : layoutAreaSlideLayout.MapArea.Size;
		oldScaledMapImageSize = tourPage.MapImage != null && tourPage.MapImage.HasFile ? Utility.ScaledImageSize(tourPage.MapImage.Size, oldMapAreaSize) : Size.Empty;
		oldImageAreaSize = ActiveSlideLayout.ImageArea.Size;
	}

	private void SetAutoLayoutLayoutAreaInnerSize(int width, int height)
	{
		int newWidth = options.TourWidthLocked ? maxLayoutAreaInnerSize.Width : width;
		int newHeight = options.TourHeightLocked ? maxLayoutAreaInnerSize.Height : height;
		Size newInnerSize = new Size(newWidth, newHeight);
		SetLayoutAreaInnerSize(newInnerSize);
	}

	private void SetAutoLayoutSplitters(int h, int v)
	{
		int newH = ActiveSlideLayout.IsInvertedSplitterH ? ActiveSlideLayout.InnerSize.Height - h - SpacingH : h;
		int newV = ActiveSlideLayout.IsInvertedSplitterV ? ActiveSlideLayout.InnerSize.Width - v - SpacingV : v;
		SlideLayoutSplitters newSplitters = new SlideLayoutSplitters(newH, newV, SplitterIsLockedH, SplitterIsLockedV);
		SplittersChanged(newSplitters);
	}

	public void SetLayoutAreaInnerSize(Size innerSize)
	{
		SetLayoutAreaOuterSize(GetLayoutAreaOuterSize(innerSize));
	}

	public void SetLayoutAreaOuterSize(Size outerSize)
	{
		layoutAreaSlideLayout.SetNewOuterSize(outerSize);
	}

	private void SetMaxLayoutAreaInnerSize()
	{
		int tourW;
		int tourH;

		// Get the width of the tour options.
		int tourOptionsW = TourLayout.CalculateWidthOfTourOptions(tour);

		// Determine the max tour width.
		if (options.WidthType == TourSizeType.LayoutArea)
			tourW = tour.MaxTourSize.Width + tourOptionsW;
		else
			tourW = options.TourWidthLocked ? tour.TourSize.Width : tour.MaxTourSize.Width;

		// Get the height of the tour options based on the tour width. Note that the tour width
		// affects the height of the banner which is why we have to get the tour width first.
		int tourOptionsH = TourLayout.CalculateHeightOfTourOptions(tour, tourW);

		// Determine the max tour size.
		if (options.HeightType == TourSizeType.LayoutArea)
			tourH = tour.MaxTourSize.Height + tourOptionsH;
		else
			tourH = options.TourHeightLocked ? tour.TourSize.Height : tour.MaxTourSize.Height;

		// Determine the layout area size by subtracting the tour options from the tour size.
		int layoutAreaW = tourW - tourOptionsW;
		int layoutAreaH = tourH - tourOptionsH;
		Size maxLayoutAreaOuterSize = new Size(layoutAreaW, layoutAreaH);
		maxLayoutAreaInnerSize = GetLayoutAreaInnerSize(maxLayoutAreaOuterSize);
	}

	public void SplittersChanged(SlideLayoutSplitters newSplitters)
	{
		// Note that we always call SetNewSplitters even if the splitter values have not changed
		// because the layout's size might have changed which would cause the sizes of the layout
		// areas to change even if the splitter values remained the same.
		ActiveSlideLayout.SetNewSplitters(newSplitters);
	}

	private void UpdateTourPage()
	{
		if (creatingTemplateChoices)
			return;

		if (layoutAreaSlideLayout.HasMapArea)
		{
			if (mapAreaSize != oldMapAreaSize && tourPage.IsGallery)
				tourPage.RebuildMap();

			if (scaledMapImageSize != oldScaledMapImageSize)
				tourPage.SetMapImageSizeChanged();
		}

		if (imageAreaSize != oldImageAreaSize)
			tourPage.SetImageAreaSizeChanged();

		Size newTourSize = CalculateTourSizeForLayoutAreaOuterSize(tour, layoutAreaSlideLayout.OuterSize);
		if (newTourSize != tour.TourSize || layoutAreaSlideLayout.OuterSize != tour.LayoutAreaSize)
		{
			if (tour.V3CompatibilityEnabled)
            {
                Debug.Assert(!options.TourWidthLocked || newTourSize.Width == tour.TourSize.Width, "Changing locked tour width");
			    Debug.Assert(!options.TourHeightLocked || newTourSize.Height == tour.TourSize.Height, "Changing locked tour height");
            }
			tour.SetTourAndLayoutAreaSizes(newTourSize, layoutAreaSlideLayout.OuterSize);
		}

		if (tour.Banner.HasImage && tour.Banner.Image.HasFile)
			tour.AdjustBannerToFitLayout();

		tourPage.UpdateDatabase();

		// Save the current size's so we'll have them to compare to after the next auto layout is run.
		SaveAreaSizes();
	}

	#endregion
}
