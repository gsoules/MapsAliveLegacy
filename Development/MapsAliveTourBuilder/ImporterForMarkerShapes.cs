// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Web;
using AvantLogic;
using AvantLogic.MapsAlive.Engine;

public class ImporterForMarkerShapes : ImporterForHotspots
{
	private ArrayList markerShapes;
	private bool linkToUrl;
	private bool linkToUrlInNewWindow;
	private int markerStyleId;
	private bool treatPolygonsAsLines;
	private bool useTitleForTooltip;

	public ImporterForMarkerShapes(TourPage tourPage, Stream stream, string reportTitle)
		: base(tourPage.Tour, tourPage, stream, reportTitle)
	{
	}

	public void ImportAreaTags(
		int markerStyleId,
		bool treatPolygonsAsLines,
		bool useTitleForTooltip,
		bool linkToUrl,
		bool linkToUrlInNewWindow)
	{
		this.markerStyleId = markerStyleId;
		this.treatPolygonsAsLines = treatPolygonsAsLines;
		this.useTitleForTooltip = useTitleForTooltip;
		this.linkToUrl = linkToUrl;
		this.linkToUrlInNewWindow = linkToUrlInNewWindow;

		ExtractAreaTags(stream);

		// Enter the importing currentState so that markers will be positioned
		// and scaled correctly whether or not MapZoom is turned on.
		tourPage.ImportingMarkers = true;

		int count = 0;
		foreach (MarkerShape markerShape in markerShapes)
		{
			if (!markerShape.IsValid)
				continue;

			string alt = markerShape.AltAttribute;
			if (alt == string.Empty || !Utility.IsValidFileName(alt))
				alt = string.Format("Hotspot{0}", count + 1);
			string markerName = alt;
			string title = markerShape.TitleAttribute;
			if (title == string.Empty)
				title = alt;

			// Look for a tour view with an import key that matches the marker name.
			TourView tourView = GetTourViewBySlideId(tourPage, markerName, title);

			bool newMarker = false;
			bool newSlide = false;
			Marker marker = null;
			if (tourView != null)
			{
				// A match was found. Get the tour view's marker.
				marker = GetTourViewMarker(tourView);
				if (marker.Name.ToLower() != markerName.ToLower())
				{
					// The tour view's marker key does not match the key from the Area tag.
					// This can happen when slides are first imported from images and then
					// from area tags. The image import will have given each view the same
					// non-exclusive marker. Create a new marker for the tour view from the
					// information in the Area tag.
					marker = CreateNewMarkerForExistingTourView(markerShape, markerName, tourView, marker);
					newMarker = true;
				}
			}
			else
			{
				// No tour view on this page has a name that matches the marker name.
				// Create a new tour view and an exclusive marker to go with it.
				marker = CreateMarker(markerName, markerShape);
				tourView = CreateNewTourViewAndNewMarker(marker, markerName, markerShape, alt, title);
				newMarker = true;
				newSlide = true;
			}

			if (marker != null)
				UpdateMarkerAndTourViewFromAreaTag(marker, tourView, markerShape, newSlide, newMarker);

			ProgressMonitor.Update(alt, markerShapes.Count, ++count);
		}

		// Exit the importing currentState.
		tourPage.ImportingMarkers = false;
	}

	private Marker CreateNewMarkerForExistingTourView(MarkerShape markerShape, string markerName, TourView tourView, Marker oldMarker)
	{
		if (oldMarker.IsExclusive)
			tourView.DeleteExclusiveMarker();
		Marker marker = CreateMarker(markerName, markerShape);
		tourView.MarkerId = marker.Id;
		tourView.UpdateDatabase();
		marker.MakeExclusive(tourView);
		report.EmitRow(ImportReport.Topic.MarkerImported, tourView.Title);
		return marker;
	}

	private TourView CreateNewTourViewAndNewMarker(Marker marker, string markerName, MarkerShape markerShape, string alt, string title)
	{
		// Create a slide and add the marker to it.
		TourView tourView = tourPage.Tour.CreateNewTourView(title != string.Empty ? title : alt);
		tourView.SlideId = alt;
		tourView.MarkerId = marker.Id;
		
		// Set the is-locked flag on imported shapes with the idea that they are bound to the
		// map and the user won't want to accidentally move them while in the map editor.
		tourView.MarkerIsLocked = true;
		
		const bool importingSlides = true;
		tourPage.Tour.AddTourView(tourView, importingSlides);
		marker.MakeExclusive(tourView);
		importedSlideCount++;
		report.EmitRow(ImportReport.Topic.SlideImported, marker.Name);
		return tourView;
	}

	private Marker CreateMarker(string markerName, MarkerShape markerShape)
	{
		int accountId = MapsAliveState.Account.Id;
		Marker marker = new Marker(Account.GetCachedMarkerStyle(markerStyleId), accountId);
		
		marker.Name = markerName;
		marker.ShapeType = markerShape.ShapeType;
		marker.ShapeCoords = markerShape.Coords;
		marker.ShapePoints = MarkerShape.CopyPoints(markerShape.Points);
		marker.ScaleShapeToMap = true;
		
		marker.InsertIntoDatabase(accountId);
		
		// Now that the marker is in the database it has an Id and we can set its resource image
		// Id to a value that indicates that this exclusive marker has no image. Exclusive markers
		// don't get an image a) because of the overhead and b) because an exclusive marker is not
		// general purpose -- it's tied to a single tour view, and c) the image is of limited value
		// since it's a tiny polygon that can look very similar to all the other imported polygons.
		// We used to give them images, but the resource image file folder ended up holding thousands
		// of image files that, as just explained, had too little value to justify the cost of file
		// creation, deletion, and bookkeeping.
		marker.ResourceImageId = TourResource.NoImageResourceImageId;
		marker.UpdateResourceImageIdInDatabase();
		
		return marker;
	}

	private string EliminateDuplicateCoords(string coords)
	{
		// Eliminate adjancent coordinates that are identical. We compare points
		// as strings rather than integer tuples in case the input is bad.
		StringBuilder sb = new StringBuilder();
		string[] points = coords.Split(',');
		
		if (points.Length < 4)
			return coords;
		
		string lastPoint = string.Empty;
		int end = points.Length - 1;
		int dups = 0;
		string firstPoint = string.Empty;
		
		for (int index = 0; index < end; index += 2)
		{
			string currentPoint = string.Format("{0},{1}", points[index].Trim(), points[index + 1].Trim());
			if (index == 0)
				firstPoint = currentPoint;
			if (currentPoint != lastPoint)
			{
				sb.Append("," + currentPoint);
			}
			else
			{
				dups++;
			}
			lastPoint = currentPoint;
		}

		// Strip off the last point if the same as the first. We do this because for some reason Adobe Illustrator
		// emits polygon and line shapes with the same first and last point. If the use wants to treat the shape
		// as a line instead of a polygon, the extra point causes the polygon to get closed when drawing a line.
		if (firstPoint == lastPoint)
			sb.Length -= lastPoint.Length + 1;
				
		Debug.WriteLine(">>> Eliminated " + dups + " coordinates");
		
		// Return the filtered coords string minus the leading comma.
		return sb.ToString().Substring(1);
	}

	private void ExtractAreaTags(Stream stream)
	{
		markerShapes = new ArrayList();
		byte[] buffer = new byte[stream.Length];
		stream.Read(buffer, 0, buffer.Length);
		string html = System.Text.ASCIIEncoding.ASCII.GetString(buffer);

		HtmlParser parser = new HtmlParser(html);

		string tagText = string.Empty;
		int count = 0;
		
		MarkerStyle markerStyle = Account.GetCachedMarkerStyle(markerStyleId);

		while (true)
		{
			Tag tag = parser.Parse();
			
			// When a null tag comes back, we are done.
			if (tag == null)
				break;

			// We are only interested in Area tags with attributes (e.g. not interested in </area>).
			if (!(tag.NameIs("AREA") && tag.HasAttributes))
				continue;

			count++;
			bool skipped = false;
			
			string alt = tag.AttributeValue("alt");
			string title = tag.AttributeValue("title");
			string href = tag.AttributeValue("href");
			if (alt == string.Empty && title == string.Empty)
			{
				// If there is no alt or title attribute, use the href.
				// And if there's no href, create a name for alt.
				alt = href != string.Empty ? href : "Area" + count;
			}
			
			string outerHtml = HttpUtility.HtmlEncode(tag.Html);
			if (outerHtml.Length > 64)
			    outerHtml = outerHtml.Substring(0, 64) + "...";

			string shape = tag.AttributeValue("shape");
			ShapeType shapeType = ShapeType.Polygon;

			MarkerShape hybridAreaTag = null;
			if (!skipped)
			{
				hybridAreaTag = GetDuplicatedAreaTag(alt);
				switch (shape.ToLower())
				{
					case "circ":
					case "circle":
						shapeType = ShapeType.Circle;
						break;

					case "rect":
					case "rectangle":
						shapeType = ShapeType.Rectangle;
						break;

					case "poly":
					case "polygon":
						if (treatPolygonsAsLines)
							shapeType = ShapeType.Line;
						else
							shapeType = ShapeType.Polygon;
						break;

					case "line":
						shapeType = ShapeType.Line;
						break;

					default:
						skipped = true;
						if (shape != "default")
						{
							// Note that we don't report an error on "default" because it's defined in HTML
							// to let you specify what happens if you click in an image map, but not on a shape.
							report.EmitRow(ImportReport.Topic.AreaTagRejected, "Unsupported shape: " + shape, outerHtml);
						}
						break;
				}
			}

			// Show the extracted tag even if its coordinates might not be valid.
			tagText += tag.Html + Utility.CrLf;

			if (!skipped)
			{
				string coords = tag.AttributeValue("coords");
				if (shapeType == ShapeType.Circle)
				{
					// Append a dummy y value after the radius so that coords are always in pairs.
					coords += ",0";
				}

				// Get rid of any adjacent points that are the same. This is especially common when
				// importing ReadyMaps that are not the same size as the original Adobe Illustrator file.
				coords = EliminateDuplicateCoords(coords);

				MarkerShape markerShape = new MarkerShape(alt, title, href, shapeType, markerStyle.LineWidth, coords);

				if (hybridAreaTag == null)
					markerShapes.Add(markerShape);
				else
					hybridAreaTag.CombineWith(markerShape);
			}
		}

		// Now that we have all the tags, validate each. We wait until the end because
		// the coordinates for hybrid tags have to be validated as a set. If we did them
		// one tag at a time, the logic to shift points toward 0,0 would shift each area
		// individually without regard for the relative location to the other areas in the hybrid.
		foreach (MarkerShape markerShape in markerShapes)
		{
			markerShape.ParseCoordinates();
			
			if (markerShape.IsValid)
				report.EmitRow(ImportReport.Topic.AreaTagAccepted, markerShape.AltAttribute);
			else
			{
				int start = 0;
				string badCoords = HttpUtility.HtmlEncode(markerShape.Coords);
				if (badCoords.StartsWith("-1"))
				{
					// Skip pass the -1 and the shape type that follows it.
					start = badCoords.Substring(3).IndexOf(',') + 4;
				}
				if (badCoords.Length > 32)
					badCoords = badCoords.Substring(start, 32) + "...";
				report.EmitRow(ImportReport.Topic.AreaTagRejected, markerShape.Error, badCoords);
			}
		}
	}

	private MarkerShape GetDuplicatedAreaTag(string alt)
	{
		foreach (MarkerShape markerShape in markerShapes)
		{
			if (markerShape.AltAttribute.ToLower() == alt.ToLower())
			{
				return markerShape;
			}
		}
		return null;
	}

	private Marker GetTourViewMarker(TourView tourView)
	{
		Marker marker = Account.GetCachedMarker(tourView.MarkerId);
		Debug.Assert(marker != null, "Tour view marker not found");
		return marker;
	}

	private void UpdateMarkerAndTourViewFromAreaTag(Marker marker, TourView tourView, MarkerShape markerShape, bool newSlide, bool newMarker)
	{
		bool markerChanged = false;
		bool tourViewChanged = false;

		markerChanged = UpdateMarkerShape(marker, markerShape, markerChanged);
		tourViewChanged = UpdateMarkerLocation(marker, tourView, markerShape, tourViewChanged);
		tourViewChanged = UpdateTooltip(tourView, markerShape, tourViewChanged);
		tourViewChanged = UpdateMarkerClickAction(tourView, markerShape, tourViewChanged);

		if (markerChanged || tourViewChanged)
		{
			if (tourViewChanged)
			{
				if (!newSlide)
					report.EmitRow(ImportReport.Topic.SlideUpdated, tourView.Title);
				tourView.UpdateDatabase();
			}

			if (markerChanged)
			{
				if (!newMarker)
					report.EmitRow(ImportReport.Topic.MarkerUpdated, marker.Name);
				marker.UpdateDatabase();
			}
		}

		if (!markerChanged && !newMarker)
			report.EmitRow(ImportReport.Topic.MarkerUnchanged, marker.Name);

		if (!tourViewChanged)
			report.EmitRow(ImportReport.Topic.SlideUnchanged, tourView.Title);
	}

	private bool UpdateMarkerClickAction(TourView tourView, MarkerShape markerShape, bool tourViewChanged)
	{
		if (linkToUrl || linkToUrlInNewWindow)
		{
			if (linkToUrl && tourView.MarkerClickAction != MarkerAction.LinkToUrl)
			{
				tourView.MarkerClickAction = MarkerAction.LinkToUrl;
				tourViewChanged = true;
			}

			if (linkToUrlInNewWindow && tourView.MarkerClickAction != MarkerAction.LinkToUrlNewWindow)
			{
				tourView.MarkerClickAction = MarkerAction.LinkToUrlNewWindow;
				tourViewChanged = true;
			}

			if (tourView.MarkerClickActionTarget != markerShape.HrefAttribute)
			{
				tourView.MarkerClickActionTarget = markerShape.HrefAttribute;
				tourViewChanged = true;
			}
		}
		return tourViewChanged;
	}

	private bool UpdateTooltip(TourView tourView, MarkerShape markerShape, bool tourViewChanged)
	{
		string title = markerShape.TitleAttribute;
		if (useTitleForTooltip & title != string.Empty)
		{
			if (tourView.ToolTip != title)
			{
				tourView.ToolTip = title;
				tourViewChanged = true;
			}
		}
		return tourViewChanged;
	}

	private static bool UpdateMarkerLocation(Marker marker, TourView tourView, MarkerShape markerShape, bool tourViewChanged)
	{
		if (tourView.MarkerX != markerShape.Location.X || tourView.MarkerY != markerShape.Location.Y)
		{
			// The X or Y location is different, but these are unscaled values.
			// Save off the current scaled values.
			int oldX = tourView.MarkerX;
			int oldY = tourView.MarkerX;
			
			// Update the location.
			int offset = 0;
			tourView.MarkerX = markerShape.Location.X + offset;
			tourView.MarkerY = markerShape.Location.Y + offset;
			
			// See if the updated results are different than the original.
			tourViewChanged = oldX != tourView.MarkerX || oldY != tourView.MarkerX;
		}
		return tourViewChanged;
	}

	private static bool UpdateMarkerShape(Marker marker, MarkerShape markerShape, bool markerChanged)
	{
		// Determine if the imported marker's shape or location has changed.
		if (marker.ShapeType != markerShape.ShapeType)
		{
			marker.ShapeType = markerShape.ShapeType;
			markerChanged = true;
		}

		if (marker.ShapeCoords != markerShape.Coords)
		{
			marker.ShapeCoords = markerShape.Coords;
			marker.ShapeRectangle = markerShape.ContainingRectangle;
			markerChanged = true;
		}
		return markerChanged;
	}
}