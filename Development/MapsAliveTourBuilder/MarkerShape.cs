// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Text;

public class MarkerShape
{
	private string altAttribute;
	private Rectangle containingRectangle;
	private string coords;
	private bool coordsAreValid;
	private string error;
	private string hrefAttribute;
	private int lineWidth;
	private Point location;
	private string rawCoords;
	private ShapeType shapeType;
	private string titleAttribute;

	public MarkerShape(ShapeType shapeType, int lineWidth)
	{
		this.shapeType = shapeType;
		this.lineWidth = lineWidth;
	}

	public MarkerShape(ShapeType shapeType, int lineWidth, string coords)
	{
		this.shapeType = shapeType;
		this.lineWidth = lineWidth;
		this.rawCoords = coords;
	}

	public MarkerShape(string alt, string title, string href, ShapeType shapeType, int lineWidth, string coords)
	{
		this.altAttribute = alt;
		this.titleAttribute = title;
		this.hrefAttribute = href;
		this.shapeType = shapeType;
		this.lineWidth = lineWidth;
		this.location = Point.Empty;
		this.rawCoords = coords;
	}

	public Point[] Points { get; set; }

	public string AltAttribute
	{
		get { return altAttribute; }
	}

	public Rectangle ContainingRectangle
	{
		get
		{
			if (containingRectangle == Rectangle.Empty)
			{
				containingRectangle = CalculateContainingRectangle();
				ShiftPointsToZeroZero();
				containingRectangle.X = 0;
				containingRectangle.Y = 0;
			}
			return containingRectangle;
		}
		set { containingRectangle = value; }
	}

	public string Coords
	{
		get { return coords != null ? coords : rawCoords; }
		set { coords = value; }
	}

	public string Error
	{
		get { return error; }
		set { error = value; }
	}

	public string HrefAttribute
	{
		get { return hrefAttribute; }
	}

	public bool IsValid
	{
		get { return coordsAreValid; }
	}

	public Point Location
	{
		get { return location; }
	}

	public ShapeType ShapeType
	{
		get { return shapeType; }
	}

	public string TitleAttribute
	{
		get { return titleAttribute; }
	}

	private Rectangle CalculateContainingRectangle()
	{
		int left = int.MaxValue;
		int right = 0;
		int top = int.MaxValue;
		int bottom = 0;


		// Find left, right, top, and bottom most points of the polygon.
		int index = 0;
		while (index < Points.Length)
		{
			Point point = Points[index];
			bool expandCircle = shapeType == ShapeType.Circle;

			if (point.X == -1)
			{
				// This "point" indicates the start of a new hybrid section.
				index++;

				expandCircle = point.Y == (int)ShapeType.Circle;
				if (expandCircle)
				{
					// Move past the section start. We'll do special processing for circles below.
					point = Points[index];
				}
				else
				{
					// Skip the section start since it's not used in the rectangle calculation.
					continue;
				}
			}

			int x = point.X;
			int y = point.Y;

			if (expandCircle)
			{
				// We need to virtually construct the rectangle that contains the circle so that
				// we can see how that rectangle affects the overall containing rectangle. If the
				// center point is near the edge of the hybrid shape, the circle can expand beyond
				// the container's edge and thus extend the bounds of the container.
				
				// Move past the circle's center point to get it's radius.
				index++;
				point = Points[index];
				int radius = point.X;

				// First test the upper left corner of the circle's rectangle...
				UpdateContainingBounds(ref left, ref right, ref top, ref bottom, x - radius, y - radius);
				
				// And then the lower right corner.
				UpdateContainingBounds(ref left, ref right, ref top, ref bottom, x + radius, y + radius);
			}
			else
			{
				UpdateContainingBounds(ref left, ref right, ref top, ref bottom, x, y);
			}

			index++;
		}

		// Determine the polygon's height and width.
		int width = right - left + 1;
		int height = bottom - top + 1;

		// Create a rectangle the encloses all of the polygon's points.
		return new Rectangle(left, top, width, height);
	}

	public void CombineWith(MarkerShape markerShape)
	{
		if (shapeType != ShapeType.Hybrid)
		{
			// A request has been made to combine another shape with the current shape. Since the
			// current shape is not already a hybrid, we know it's the first shape and that we need to  
			// prepend a section start indicator to the first shape.
			rawCoords = string.Format("-1,{0},{1}", (int)shapeType, rawCoords);
			
			// Convert the overall shape to hybrid.
			shapeType = ShapeType.Hybrid;
		}

		// Prepend a section start indicator to the shape just passed in.
		rawCoords += string.Format(",-1,{0},{1}", (int)markerShape.shapeType, markerShape.rawCoords);
	}

	public static Point[] CopyPoints(Point[] from)
	{
		Point[] copy = new Point[from.Length];
		for (int index = 0; index < from.Length; index++)
			copy[index] = from[index];
		return copy;
	}

	public void ConvertCoordsToPoints(ShapeType coordsShapeType, string coords)
	{
		Debug.WriteLine("ConvertStringCoordsToPoints " + coords);
		int[] coordsArray;

		// This method expects that coords are well-formed. That means that all values are
		// integers and the pairs conform to shape rules. It throws an exception otherwise.
		
		// Create an integer array of the coord values.
		string[] stringArray = coords.Split(',');
		coordsArray = new int[stringArray.Length];
		for (int index = 0; index < stringArray.Length; index++)
		{
			string stringValue = stringArray[index].Trim();
			if (stringValue.Length == 0)
			{
				coordsAreValid = false;
				Error = string.Format("Coordinate {0} is blank", index + 1);
				return;
			}
			
			int intValue = 0;
			if (int.TryParse(stringValue, out intValue))
			{
				coordsArray[index] = intValue;
			}
			else
			{
				coordsAreValid = false;
				Error = string.Format("Coordinate {0} '{1}' is not an integer", index + 1, stringValue);
				return;
			}
		}

		// Create a list that we can add Point objects to one at a time.
		ArrayList list = new ArrayList();
		int listIndex = 0;
		int coordIndex = 0;

		while (coordIndex < coordsArray.Length)
		{
			int x;
			int y;

			x = coordsArray[coordIndex];
			coordIndex++;

			if (coordsShapeType == ShapeType.Circle && coordIndex == 3)
			{
				y = 0;
				// Circle coords from MapsAlive 2.6 only had 3 values: x, y, and radius.
				// In MapsAlive 3.0 we added a 4th dummy y value so that we always have
				// pairs of values. When the 4th value is present, skip past it.
				if (coordsArray.Length >= 4)
				{
					coordIndex++;
				}
			}
			else if (coordsShapeType == ShapeType.Hybrid && x == -1)
			{
				// This is the start of a new section. X is -1 and Y is the ShapeType.
				y = coordsArray[coordIndex];
				coordIndex++;

				// Normally we just add the section start to the list as a "point", but if
				// it's a circle, we have to parse it manually to deal with the fact that it
				// will only have 3 values if created with an older version of MapsAlive.
				if ((ShapeType)y == ShapeType.Circle)
				{
					// Add the section start "point" to the list.
					list.Add(new Point(x, y));

					// Add the circle's center point to the list.
					x = coordsArray[coordIndex];
					coordIndex++;
					y = coordsArray[coordIndex];
					coordIndex++;
					list.Add(new Point(x, y));

					// Create a new "point" for the radius.
					x = coordsArray[coordIndex];
					y = 0;
					coordIndex++;

					if (coordIndex < coordsArray.Length && coordsArray[coordIndex] == 0)
					{
						// Skip past 4th circle value (see comment above for Circle shape).
						coordIndex++;
					}
				}
			}
			else
			{
				// X and Y are a vanilla point. No special parsing is required.
				y = coordsArray[coordIndex];
				coordIndex++;
			}

			// Create a new Point and add it to the list.
			list.Add(new Point(x, y));
			listIndex++;
		}

		// Create an array of points from the list of points.
		Points = new Point[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			Points[i] = (Point)list[i];
		}
	}

	private void ConvertPointsToCoords()
	{
		// Create a string approximately long enough to hold all of the coordinates.
		// Figure that each point has 2 values (X and Y) that each could be 4 digits.
		// The digits are separated by 1 comma.  Thus, 2 * 4 + 1 = 9;
		StringBuilder sb = new StringBuilder(Points.Length * 9);

		// Create an X/Y pair followed by a comma for each point.
		for (int index = 0; index < Points.Length; index++)
		{
			sb.Append(Points[index].X.ToString() + "," + Points[index].Y.ToString());
			if (index != Points.Length - 1)
				sb.Append(',');
		}

		Coords = sb.ToString();
	}

	private void LocateXY()
	{
		// This simple calculation is critical to the precise positioning of shapes on the map.
		// If you make any changes here, be sure to test by importing shapes to verify that they
		// line up perfectly with the corresponding Area tags.

		int w = containingRectangle.Width;
		int h = containingRectangle.Height;

		if (shapeType != ShapeType.Rectangle)
		{
			// For location purposes we need to subtract 1 from the dimensions of the containing rectangle
			// of non-rectangle shapes in order to get them to be positioned properly on the map. This was
			// determined by trial and error. All see comments in ParseCoordinates below where we special
			// case rectangles that are imported from area tags.
			w -= 1;
			h -= 1;
		}

		location.X = containingRectangle.X + (w / 2);
		location.Y = containingRectangle.Y + (h / 2);
	}

	public void ParseCoordinates()
	{
		// This method is used to validate and parse coordinates that came from the
		// user via Area tags or entered manually in the marker editor. First verify
		// that the comma-separated list of values represents a valid shape.
		ValidateCoords();
		if (!coordsAreValid)
			return;

		// Create an array of integer Points that is easier to work with than a string of numbers.
		ConvertCoordsToPoints(ShapeType, Coords);
		if (!coordsAreValid)
			return;

		if (shapeType == ShapeType.Rectangle)
		{
			// Adjust for the fact that in Flash rectangles are rendered using vector drawing, not
			// bit map drawing. Thus a rectangle with points 0,0 and 8,10 is 8x10 not 9x11. We
			// make this adjustment only for rectangles that are imported from area tags.
			Points[1].X -= 1;
			Points[1].Y -= 1;
		}

		// Analyze the coordinates to create a rectangle that contains all of them.
		ContainingRectangle = CalculateContainingRectangle();

		// Depending on where the coordinates originated from and how they needed to be adjusted 
		// to accommodate the rectangle that bounds a circle, the containing rectangle's origin
		// might not be at (0,0) which we need for drawing purposes. Since we've just recorded
		// the shape's location with respect to (0,0), we can shift it's origin to (0,0).
		ShiftPointsToZeroZero();

		// Determine the shape's location on the map, that is, it's offset from (0,0).
		LocateXY();

		// Convert the points back to a string of comma separated numbers so that we can
		// store them in the database that way.
		ConvertPointsToCoords();
	}

	private static int ScaleCoord(int coord, int actualAxisLength, int scaledAxisLength)
	{
		// Determine the percentage distance of this coordinate from zero on its axis.
		double pct = Utility.PixelToPercent(coord, actualAxisLength);

		// Using the scaled axis, get the pixel corresponding to the percentage just calculated.
		return Utility.PercentToPixel(pct, scaledAxisLength);
	}

	private static Point ScalePoint(Point point, Size actualSize, Size scaledSize)
	{
		int x = ScaleCoord(point.X, actualSize.Width, scaledSize.Width);
		int y = ScaleCoord(point.Y, actualSize.Height, scaledSize.Height);
		return new Point(x, y);
	}

	private void ShiftPointsToZeroZero()
	{
		// This method shifts all the points such that the rectangle that contains them
		// has its origin at (0,0). We do this so that we can draw the shape in the marker
		// editor without knowing where the marker is located on the map

		int left = containingRectangle.X;
		int top = containingRectangle.Y;

		if (left == 0 && top == 0)
			return;

		// Adjust each point one at a time.
		int index = 0;
		while (index < Points.Length)
		{
			bool isCircle = false;
			Point point = Points[index];
			
			if (point.X == -1)
			{
				// This is a hybrid marker section start "point". Move past it.
				isCircle = point.Y == (int)ShapeType.Circle;
				index++;
			}
			else
			{
				isCircle = shapeType == ShapeType.Circle;
			}

			// Adjust the point. If this is a circle, we are adjusting its center point.
			Points[index].X -= left;
			Points[index].Y -= top;

			if (isCircle)
			{
				// Skip past the radius.
				index++;
			}

			index++;
		}
	}

	private static void UpdateContainingBounds(ref int left, ref int right, ref int top, ref int bottom, int x, int y)
	{
		if (x < left)
			left = x;
		if (x > right)
			right = x;
		if (y < top)
			top = y;
		if (y > bottom)
			bottom = y;
	}

	private void ValidateCoords()
	{
		string[] rawPairs = rawCoords.Trim().Split(',');
		int rawCoordsCount = rawPairs.Length;

		switch (shapeType)
		{
			case ShapeType.Circle:
				Error = MapsAliveTourBuilder.Text.CirclenNeedsFourValues;
				coordsAreValid = rawCoordsCount == 4;
				break;
			
			case ShapeType.Hybrid:
				coordsAreValid = true;
				break;
			
			case ShapeType.Line:
				Error = MapsAliveTourBuilder.Text.LineNeedsFourValues;
				coordsAreValid = rawCoordsCount >= 4;
				break;
			
			case ShapeType.Polygon:
				Error = MapsAliveTourBuilder.Text.PolygonNeedsSixValues;
				coordsAreValid = rawCoordsCount >= 6;
				break;
			
			case ShapeType.Rectangle:
				Error = MapsAliveTourBuilder.Text.RectangleNeedsFourValues;
				coordsAreValid = rawCoordsCount == 4;
				break;
			
			default:
				System.Diagnostics.Debug.Fail("Unsupported shape type " + shapeType);
				break;
		}

		if (coordsAreValid)
		{
			const int maxCoordsLength = 130000;
			if (Coords.Length > maxCoordsLength)
			{
				// This limit was imposed to prevent an exception in Flash. It may no longer be necessary,
				// but no one has ever complained about it and so we keep it to avoid overly complex shapes.
				coordsAreValid = false;
				Error = "Shape has too many points";
			}
		}

		if (!coordsAreValid)
		{
			string name = altAttribute;
			if (name.Length == 0)
				name = titleAttribute;
			if (name.Length == 0)
				name = hrefAttribute;
			if (name.Length > 0)
				Error = name + ": " + Error;
		}
	}
}
