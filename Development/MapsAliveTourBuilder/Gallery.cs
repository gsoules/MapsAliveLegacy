// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Drawing;

// These values are known in the DB -- do not change.
public enum GalleryCellAlignH
{
	Center = 0,
	Left = 1,
	Right = 2
}

// These values are known in the DB -- do not change.
public enum GalleryCellAlignV
{
	Center = 0,
	Top = 1,
	Bottom = 2
}

public class GalleryOptions
{
	public GalleryOptions(
		bool isGallery,
		int spacingRow,
		int spacingColumn,
		bool autoSpacingRow,
		bool autoSpacingColumn,
		int marginTop,
		int marginLeft,
		GalleryCellAlignH cellAlignH,
		GalleryCellAlignV cellAlignV,
		bool useFixedRowHeight,
		bool useFixedColumnWidth,
		ImageExpansionType backgroundType)
	{
		IsGallery = isGallery;
		SpacingRow = spacingRow;
		SpacingColumn = spacingColumn;
		AutoSpacingRow = autoSpacingRow;
		AutoSpacingColumn = autoSpacingColumn;
		MarginTop = marginTop;
		MarginLeft = marginLeft;
		CellAlignH = cellAlignH;
		CellAlignV = cellAlignV;
		UseFixedRowHeight = useFixedRowHeight;
		UseFixedColumnWidth = useFixedColumnWidth;
		BackgroundType = backgroundType;
	}

	public GalleryCellAlignH CellAlignH { get; set; }
	public GalleryCellAlignV CellAlignV { get; set; }
	public int MarginLeft { get; set; }
	public int MarginTop { get; set; }
	public ImageExpansionType BackgroundType { get; set; }
	public bool IsGallery { get; set; }
	public int SpacingColumn { get; set; }
	public bool AutoSpacingRow { get; set; }
	public bool AutoSpacingColumn { get; set; }
	public int SpacingRow { get; set; }
	public bool UseFixedRowHeight { get; set; }
	public bool UseFixedColumnWidth { get; set; }
}

public class Gallery
{
	private ArrayList columnWidths;
	private Size fixedCellSize;
	private Size gallerySize;
	private MarkerDefinition markerDefinition;
	private Hashtable markerDefinitions;
	private int markerHeight;
	private int markerWidth;
	private int numberOfColumns;
	private int numberOfRows;
	private GalleryOptions options;
	private ArrayList rowHeights;
	private TourPage tourPage;

	public Gallery(TourPage tourPage, Hashtable markerDefinitions)
	{
		this.tourPage = tourPage;
		this.markerDefinitions = markerDefinitions;
		options = tourPage.GalleryOptions;
		
		// Determine how much area is available for markers.
		Size mapAreaSize = tourPage.MapAreaSize;
		gallerySize.Width = mapAreaSize.Width - options.MarginLeft;
		gallerySize.Height = mapAreaSize.Height - options.MarginTop;
	}

	public void ArrangeMarkers()
	{
		try
		{
			CalculateFixedCellSize();

			// Determine how many columns would be in the first row if we just
			// start placing markers left to right until we run out of room.
			// This tells us the maximum number of columns we'll need. 
			numberOfColumns = CalculateFirstRowColumns();
			if (numberOfColumns == 0)
				return;

			bool ok = false;

			while (!ok)
			{
				// Starting with the maximum column count, determine if the rest of the
				// rows will fit in this many columns. If any row's markers would run
				// out of room before using all the columns, subtract one and try again.
				while (!MarkersFitInColumns(numberOfColumns))
				{
					numberOfColumns--;
					if (numberOfColumns == 0)
						return;
				}

				// Determine the height of each row and the width of each column.
				CalculateCellSizes();

				// Place each marker in its cell. If the result comes back empty, our column
				// estimate was still too large. This can happen if for example the first row
				// contains a lot of narrow markers and the second row contains wide markers.
				// It's only when we place markers on the second row that we discover the problem.
				tourPage.GallerySize = PlaceMarkers();

				if (tourPage.GallerySize == Size.Empty)
				{
					// Try again with one less column.
					numberOfColumns--;
				}
				else
				{
					// Everything fits.
					ok = true;
				}
			}

			// Update the database.
			foreach (TourView tourView in tourPage.TourViewsBySequence)
			{
				if (tourView.MarkerChanged)
				{
					// This tour view's position moved.
					const bool notifyTourPage = false;
					tourView.UpdateDatabase(notifyTourPage);
				}
			}

			tourPage.TourViewChanged();
		}
		catch (Exception ex)
		{
			Utility.ReportException("ArrangeMarkers", ex);
		}
	}

	private void CalculateCellSizes()
	{
		// Create and initialize arrays that know height of each row and width of each column.
		rowHeights = new ArrayList(numberOfRows);
		columnWidths = new ArrayList(numberOfColumns);
		
		for (int i = 0; i < numberOfRows; i++)
			rowHeights.Add(options.UseFixedRowHeight ? fixedCellSize.Height : 0);
		
		for (int i = 0; i < numberOfColumns; i++)
			columnWidths.Add(options.UseFixedColumnWidth ? fixedCellSize.Width : 0);
		

		// Find the tallest marker in each row and the widest marker in each column.
		int column = 1;
		int row = 1;
		
		foreach (TourView tourView in tourPage.TourViewsBySequence)
		{
			GetMarkerDefinition(tourView);

			// Determine if this markers needs a taller row or wider column.
			if (markerHeight > (int)rowHeights[row - 1])
				rowHeights[row - 1] = markerHeight;
			if (markerWidth > (int)columnWidths[column - 1])
				columnWidths[column - 1] = markerWidth;
			
			// Move to the next column.
			column++;
			if (column > numberOfColumns)
			{
				// Move to the next row.
				column = 1;
				row++;
			}

			// Ignore rows that don't fit in the gallery.
			if (row > numberOfRows)
				break;
		}

		PerformAutoSpacing();
	}

	private int CalculateFirstRowColumns()
	{
		// Lay down the first row of markers to determine the max number of columns we'll need.
		// It's the max because no matter what other markers come along for subsequent rows,
		// they can't use more columns than the first row.

		int numberOfColumnsInFirstRow = 0;
		int widthLeft = gallerySize.Width;

		foreach (TourView tourView in tourPage.TourViewsBySequence)
		{
			GetMarkerDefinition(tourView);
			widthLeft -= markerWidth;
			if (widthLeft < 0)
				break;
			
			numberOfColumnsInFirstRow++;
			widthLeft -= options.SpacingColumn;
		}

		return numberOfColumnsInFirstRow;
	}

	private void CalculateFixedCellSize()
	{
		// Create a definition for each marker. This is very expensive so we only do it once.
		foreach (TourView tourView in tourPage.TourViewsBySequence)
		{
			//Marker marker = Account.GetCachedMarker(tourView.MarkerId);
			//markerDefinition = marker.CreateMarkerDefinition(1, tourView);
			//markerDefinitions.Add(tourView.Id, markerDefinition);
			
			// Keep track of the width of the widest marker and the height of the tallest
			// marker. When we're done, will know what dimensions to use for fixed rows
			// and columns if the user requests them.
			GetMarkerDefinition(tourView);
			Size size = markerDefinition.SizeWithBorder;
			if (size.Width > fixedCellSize.Width)
				fixedCellSize.Width = size.Width;
			if (size.Height > fixedCellSize.Height)
				fixedCellSize.Height = size.Height;
		}
	}

	private void GetMarkerDefinition(TourView tourView)
	{
		string id;
		Marker marker = Account.GetCachedMarker(tourView.MarkerId);
		if (marker.MarkerType == MarkerType.Photo || marker.MarkerType == MarkerType.Text)
			id = string.Format("{0}_{1}", marker.Id, tourView.Id);
		else
			id = marker.Id.ToString();

		markerDefinition = (MarkerDefinition)markerDefinitions[id];
		markerWidth = markerDefinition.SizeWithBorder.Width;
		markerHeight = markerDefinition.SizeWithBorder.Height;
	}

	private bool MarkersFitInColumns(int targetNumberOfColumns)
	{
		// This method determines if the markers will be in the target number of colums.
		// It does this by attempting to layout all of the markers in the columns. If
		// any marker won't fit, we know that at least one of the columns is too narrow
		// and return false. The caller will try again passing a smaller target number.
		
		int heightLeft = gallerySize.Height;
		int widthLeft = gallerySize.Width;
		int maxMarkerHeightInRow = options.UseFixedRowHeight ? fixedCellSize.Height : 0;
		
		int column = 1;
		numberOfRows = 1;

		// Set initial widths for each column. As we layout the markers column by column and 
		// row by row, we'll widen columns as neccessary to accommodate the markers placed 
		// in them. Each time we widen a column, less space becomes available for subsequent
		// columns. If a column becomes too narrow for a marker, we know that the target
		// passed to this method was too large.
		columnWidths = new ArrayList(numberOfColumns);
		for (int i = 0; i < numberOfColumns; i++)
		{
			columnWidths.Add(options.UseFixedColumnWidth ? fixedCellSize.Width : 0);
		}	

		foreach (TourView tourView in tourPage.TourViewsBySequence)
		{
			GetMarkerDefinition(tourView);

			// Determine how much gallery width this marker occupies. If the
			// marker is narrower than its column, we use the column widht.
			int widthUsed;
			if (options.UseFixedColumnWidth)
			{
				widthUsed = fixedCellSize.Width;
			}
			else
			{
				widthUsed = Math.Max(markerWidth, (int)columnWidths[column - 1]);
				if (widthUsed > (int)columnWidths[column - 1])
					columnWidths[column - 1] = widthUsed;
			}

			// Subtract the width used from the amount of room left.
			widthLeft -= widthUsed;

			bool startNewRow = false;
			if (widthLeft < 0)
			{
				// The marker won't fit on the current row.
				if (column <= targetNumberOfColumns)
				{
					// We ran out of room before hitting the last column.
					// Therefore, the target number of columns is too large.
					return false;
				}
				
				startNewRow = true;
				widthUsed = markerWidth + options.SpacingColumn;
			}
			else
			{
				// This marker fits.
				if (markerHeight > maxMarkerHeightInRow)
					maxMarkerHeightInRow = markerHeight;

				if (maxMarkerHeightInRow > heightLeft)
				{
					// The marker is too tall for this row. Ignore the rest of the markers.
					if (numberOfRows > 1)
						numberOfRows--;
					return true;
				}
				
				// Move to the next column.
				column++;
				if (column > numberOfColumns)
				{
					startNewRow = true;
					widthUsed = 0;
				}
				else
				{
					widthLeft -= options.SpacingColumn;
				}
			}

			if (startNewRow)
			{
				// Determine if there is room for another row.
				heightLeft -= maxMarkerHeightInRow;
				if (heightLeft < 0)
				{
					// We ran out of vertical space. Ignore the rest of the markers so
					// that their sizes don't throw off the calculations we just made.
					// The markers that we examined all fit in the target number of columns.
					numberOfRows--;
					return true;
				}

				// Start a new row and column.
				heightLeft -= options.SpacingRow;
				maxMarkerHeightInRow = options.UseFixedRowHeight ? fixedCellSize.Height : 0;
				numberOfRows++;
				column = 1;
				
				// Reset the available width. If we started a new row because the current marker would not
				// fit on the previous row, the marker will get placed on the new row so we subtract its width.
				widthLeft = gallerySize.Width - widthUsed;
			}
		}
		
		// If we get here, every marker fit within the target number of columns.
		return true;
	}

	private void PerformAutoSpacing()
	{
		// Increase row heights and/or column widths so markers are spaced to fill the gallery area.
		// The algorthim determines how much width and/or height is leftover after all of the markers
		// have been placed and then distributes it evenly to all rows and columns.s
		
		if (options.AutoSpacingColumn)
		{
			int widthUsed = 0;
			for (int i = 0; i < numberOfColumns; i++)
			{
				widthUsed += (int)columnWidths[i];
				if (i > 0)
					widthUsed += options.SpacingColumn;
			}
			int marginRight = options.MarginLeft;
			int delta = gallerySize.Width - widthUsed - marginRight;
			int padding = delta / numberOfColumns;
			if (padding > 0)
			{
				for (int i = 0; i < numberOfColumns; i++)
				{
					columnWidths[i] = (int)columnWidths[i] + padding;
				}
			}
		}

		if (options.AutoSpacingRow)
		{
			int heightUsed = 0;
			for (int i = 0; i < numberOfRows; i++)
			{
				heightUsed += (int)rowHeights[i];
				if (i > 0)
					heightUsed += options.SpacingRow;
			}
			int marginBottom = options.MarginTop;
			int delta = gallerySize.Height - heightUsed - marginBottom;
			int padding = delta / numberOfRows;
			if (padding > 0)
			{
				for (int i = 0; i < numberOfRows; i++)
				{
					rowHeights[i] = (int)rowHeights[i] + padding;
				}
			}
		}
	}

	private Size PlaceMarkers()
	{
		int maxW = 0;
		int maxH = 0;
		int x = 0;
		int y = 0;
		int row = 1;
		int column = 1;

		foreach (TourView tourView in tourPage.TourViewsBySequence)
		{
			if (row > numberOfRows)
			{
				// Place the remaining markers off-screen.
				tourView.MarkerX = -1000;
				tourView.MarkerY = -1000;
				continue;
			}

			GetMarkerDefinition(tourView);

			if (column == 1)
			{
				x = options.MarginLeft;
				if (row == 1)
					y = options.MarginTop;
			}

			// Horizontally align the marker.
			int deltaX = ((int)columnWidths[column - 1] - markerWidth) / 2;
			if (options.CellAlignH == GalleryCellAlignH.Left)
				deltaX = 0;
			else if (options.CellAlignH == GalleryCellAlignH.Right)
				deltaX *= 2;
			
			// Vertically align the marker.
			int deltaY = ((int)rowHeights[row - 1] - markerHeight) / 2;
			if (options.CellAlignV == GalleryCellAlignV.Top)
				deltaY = 0;
			else if (options.CellAlignV == GalleryCellAlignV.Bottom)
				deltaY *= 2;

			// Place the marker in its correct location within the gallery. Note that we shift
			// the marker right and down by half of its border thickness so that the entire border is
			// visible. It was done this way because the old Flash drawShape method shifted the location left
			// and up to account for the border. Normally that's the correct behavior, but not here.
			int halfBorder = markerDefinition.LineWidth / 2;
			tourView.MarkerX = x + (markerWidth / 2) + deltaX + halfBorder;
			tourView.MarkerY = y + (markerHeight / 2) + deltaY + halfBorder;

			// Move right to the next column.
			x += (int)columnWidths[column - 1] + options.SpacingColumn;

			if (x - options.SpacingColumn > gallerySize.Width)
			{
				// The pattern layed out so far does not fit in the estimated number of columns.
				return Size.Empty;
			}

			column++;

			if (column > numberOfColumns)
			{
				// Move to the next row.
				column = 1;
				y += (int)rowHeights[row - 1] + options.SpacingRow;
				row++;

				// Track the width and height occupied by the placed markers.
				int w = x - options.SpacingColumn + options.MarginLeft;
				if (w > maxW)
					maxW = w;

				int h = y - options.SpacingRow + options.MarginTop;
				if (h > maxH)
					maxH = h;
			}
		}

		return new Size(maxW, maxH);
	}
}