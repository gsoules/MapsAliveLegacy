// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using AvantLogic.MapsAlive.Engine;

// All of these enumberation values are known in the DB -- don't change the numbers.
public enum MarkerAction
{
	None = 0,
	GotoPage = 1,
	LinkToUrl = 2,
	CallJavascript = 3,
	LinkToUrlNewWindow = 5
}

public enum MarkerState
{
	Normal = 0,
	Selected = 1,
	Combo = 2
}

public enum MarkerType
{
	Symbol = 1,
	Shape = 2,
	SymbolAndShape = 3,
	Text = 4,
	Photo = 5
}

public enum MarkerFilter
{
	Tour,
	Exclusive,
	Account,
	Gallery
}

public enum PhotoConstraintType
{
	Scale = 0,
	Width = 1,
	Height = 2,
	WidthAndHeight = 3
}

public enum PhotoCropType
{
	ScaleAndTrim = 0,
	AlignLeftOrTop = 1,
	AlignCenter = 2,
	AlignRightOrBottom = 3,
	CropCenter = 4,
	CropN = 5,
	CropNE = 6,
	CropE = 7,
	CropSE = 8,
	CropS = 9,
	CropSW = 10,
	CropW = 11,
	CropNW = 12
}

public enum PhotoCaptionPositionType
{
	None = 0,
	Top = 1,
	Bottom = 2
}

public enum PhotoEffect
{
	Color = 0,
	GrayScale = 1,
	Sepia = 2,
	ColorNegative = 3,
	GrayScaleNegative = 4
}

public enum ShapeType
{
	None = 0,
	Circle = 1,
	Rectangle = 2,
	Polygon = 3,
	Line = 4,
	Hybrid = 5,
}

public enum TextAlignType
{
	LeftOrTop = 0,
	Center = 1,
	RightOrBottom = 2
}

public partial class Marker : TourResource
{
	private int anchorLocationX;
	private int anchorLocationY;
	private int fontStyleId;
	private FontStyleResource _fontStyleResource;
	private bool isExclusive;
	private MarkerStyle _markerStyle;
	private int markerStyleId;
	private MarkerDefinition markerDefinition;
	private Byte[] _normalActualImageBytes;
	private Byte[] _normalScaledImageBytes;
	private Byte[] normalSymbolImageBytes;
	private Byte[] _selectedActualImageBytes;
	private Byte[] _selectedScaledImageBytes;
	private Byte[] selectedSymbolImageBytes;
	private string shapeCoords;
	private Point[] shapePoints;
	private Rectangle shapeRectangle;
	private int symbolLocationX;
	private int symbolLocationY;
	private int tourViewId;

	public int CircleRadius { get; set; }
	public PhotoCaptionPositionType PhotoCaptionPosition { get; set; }
	public PhotoConstraintType PhotoConstraint { get; set; }
	public PhotoCropType PhotoCrop { get; set; }
	public int PhotoCropFactor { get; set; }
	public int PhotoHeight { get; set; }
	public PhotoEffect PhotoNormalEffect { get; set; }
	public int PhotoNormalOpacity { get; set; }
	public int PhotoScale { get; set; }
	public PhotoEffect PhotoSelectedEffect { get; set; }
	public int PhotoSelectedOpacity { get; set; }
	public int PhotoWidth { get; set; }
	public MarkerType MarkerType { get; set; }
	public int NormalSymbolId { get; set; }
	public string NormalTextColor { get; set; }
	public int PhotoPadding { get; set; }
	public Size RectangleSize { get; set; }
	public bool ScaleShapeToMap { get; set; }
	public int SelectedSymbolId { get; set; }
	public string SelectedTextColor { get; set; }
	public ShapeType ShapeType { get; set; }
	public TextAlignType TextAlignH { get; set; }
	public TextAlignType TextAlignV { get; set; }
	public bool TextAutoSize { get; set; }
	public int TextPadding { get; set; }
	public string TextString { get; set; }

	private const int defaultCircleRadius = 40;
	private Size defaultRectangleSize = new Size(150, 40);
	private const int defaultPadding = 8;
	private const int maxPreviewDimension = 200;
	private const int defaultPhotoScale = 20;
	private const int defaultPhotoHeight = 100;
	private const int defaultPhotoWidth = 100;

	public Marker()
	{
		ConstructMarker();
	}

	public Marker(MarkerStyle markerStyle, int accountId)
	{
		this.MarkerType = MarkerType.Shape;
		this.accountId = accountId;
		MarkerStyle = markerStyle;
		ConstructMarker();
	}

	public Marker(int markerId)
	{
		if (LoadResourceRowFromDatabase(markerId))
			InitializeResourceFromDataRecord(row);
	}

	private void ConstructMarker()
	{
		// These properties must be non-null when inserted into the DB.
		TextString = string.Empty;
		NormalTextColor = string.Empty;
		SelectedTextColor = string.Empty;
		ShapeCoords = string.Empty;

		// There arrays must be non-null;
		FlushMarkerImageBytes();
		normalSymbolImageBytes = new Byte[0];
		selectedSymbolImageBytes = new Byte[0];
	}

	public override void InitializeResourceFromDataRecord(MapsAliveDataRecord record)
	{
		bool isRow = record is MapsAliveDataRow;
		
		// Properties that determine appearance.
		int circleRadius = record.IntValue(Tag.circleRadius);
		fontStyleId = record.IntValue(Tag.fontStyleId);
		PhotoConstraint = (PhotoConstraintType)record.IntValue(Tag.photoConstraintId);
		PhotoCaptionPosition = (PhotoCaptionPositionType)record.IntValue(Tag.photoCaptionPositionId);
		PhotoCrop = (PhotoCropType)record.IntValue(Tag.photoCropId);
		PhotoCropFactor = record.IntValue(Tag.photoCropFactor);
		PhotoHeight = record.IntValue(Tag.photoHeight);
		PhotoWidth = record.IntValue(Tag.photoWidth);
		PhotoNormalEffect = (PhotoEffect)record.IntValue(Tag.photoNormalEffect);
		PhotoNormalOpacity = record.IntValue(Tag.photoNormalOpacity);
		PhotoScale = record.IntValue(Tag.photoScale);
		PhotoSelectedEffect = (PhotoEffect)record.IntValue(Tag.photoSelectedEffect);
		PhotoSelectedOpacity = record.IntValue(Tag.photoSelectedOpacity);
		markerStyleId = record.IntValue(Tag.markerStyleId);
		MarkerType = (MarkerType)record.IntValue(Tag.markerType);
		NormalSymbolId = record.IntValue(Tag.normalSymbolId);
		NormalTextColor = record.StringValue(Tag.normalTextColor);
		PhotoPadding = record.IntValue(Tag.padding);
		int rectangleWidth = record.IntValue(Tag.rectangleWidth);
		int rectangleHeight = record.IntValue(Tag.rectangleHeight);
		ScaleShapeToMap = record.BoolValue(Tag.scaleShapeToMap);
		SelectedSymbolId = record.IntValue(Tag.selectedSymbolId);
		SelectedTextColor = record.StringValue(Tag.selectedTextColor);
		
		// Must get the shape type before setting ShapeCoords.
		ShapeType = (ShapeType)record.IntValue(Tag.shapeType);
		ShapeCoords = record.StringValue(Tag.shapeCoords);

		anchorLocationX = record.IntValue(Tag.anchorLocationX);
		anchorLocationY = record.IntValue(Tag.anchorLocationY);
		symbolLocationX = record.IntValue(Tag.symbolLocationX);
		symbolLocationY = record.IntValue(Tag.symbolLocationY);
		TextAlignH = (TextAlignType)record.IntValue(Tag.textAlignH);
		TextAlignV = (TextAlignType)record.IntValue(Tag.textAlignV);
		TextAutoSize = record.BoolValue(Tag.textAutoSize);
		TextPadding = record.IntValue(Tag.textPadding);
		TextString = record.StringValue(Tag.textString);

		// Set the flag that indicates if this marker is used exclusively by one tour view.
		// There is no such flag in the database so we see if the tour view Id is set. There
		// is no tour view Id in the XML, but there is a flag in the XML.
		if (isRow)
		{
			tourViewId = record.IntValue("TourViewId");
			isExclusive = tourViewId != 0;
		}
		else
		{
			isExclusive = record.BoolValue(Tag.isExclusive);
		}

		// Temporary test to make sure that exlusive markers have the correct resource image Id.
		Debug.Assert(!(tourViewId > 0 && ResourceImageId != TourResource.NoImageResourceImageId), "Exclusive marker ResourceImageId does not = " + TourResource.NoImageResourceImageId);

		// Properties that are constructed on-the-fly by the query.
		if (isRow)
		{
			// Important: the row used here is not the raw row corresponding to a EditHotspotActions table record.
			// It must contain the dynamically constructed NormalSymbolImage and SelectedSymbolImage
			// columns that are returned by sp_Marker_GetMarker. Those columns don't exist in the EditHotspotActions table.
			normalSymbolImageBytes = record.ByteArrayValue("NormalSymbolImage");
			selectedSymbolImageBytes = record.ByteArrayValue("SelectedSymbolImage");
		}

		// Properties that can only be constructed after the database record has been read.
		FlushMarkerImageBytes();
		SetShapeSizes(circleRadius, rectangleWidth, rectangleHeight);
	}

	public enum Tag
	{
		id,
		name,
		markerType,
		isExclusive,
		fontStyleId,
		markerStyleId,
		normalSymbolId,
		selectedSymbolId,
		photoConstraintId,
		photoCaptionPositionId,
		photoCropId,
		photoCropFactor,
		photoWidth,
		photoHeight,
		photoScale,
		photoNormalEffect,
		photoNormalOpacity,
		photoSelectedEffect,
		photoSelectedOpacity,
		normalTextColor,
		selectedTextColor,
		padding,
		rectangleWidth,
		rectangleHeight,
		circleRadius,
		shapeType,
		scaleShapeToMap,
		shapeCoords,
		anchorLocationX,
		anchorLocationY,
		symbolLocationX,
		symbolLocationY,
		textAlignH,
		textAlignV,
		textAutoSize,
		textPadding,
		textString
	}

	public override string GetTagValue(int tagId)
	{
		Tag tag = (Tag)tagId;

		bool hasSymbol = this.MarkerType == MarkerType.Symbol || this.MarkerType == MarkerType.SymbolAndShape;
		bool hasShape = this.MarkerType != MarkerType.Symbol;
		bool isPhotoMarker = this.MarkerType == MarkerType.Photo;
		bool isPhotoMarkerWithText = isPhotoMarker && PhotoCaptionPosition != PhotoCaptionPositionType.None;
		bool isTextMarker = this.MarkerType == MarkerType.Text;

		switch (tag)
		{
			case Tag.id:
				return Id.ToString();
			
			case Tag.name:
				return Name;
			
			case Tag.markerType:
				return ((int)MarkerType).ToString();

			case Tag.isExclusive:
				return IsExclusive.ToString();
			
			case Tag.fontStyleId:
				return isTextMarker || isPhotoMarkerWithText ? fontStyleId.ToString() : string.Empty;
			
			case Tag.markerStyleId:
				return hasShape ? markerStyleId.ToString() : string.Empty;
			
			case Tag.normalSymbolId:
				return hasSymbol ? NormalSymbolId.ToString() : string.Empty;
			
			case Tag.selectedSymbolId:
				return hasSymbol ? SelectedSymbolId.ToString() : string.Empty;
			
			case Tag.padding:
				return isPhotoMarker ? PhotoPadding.ToString() : string.Empty;

			case Tag.photoConstraintId:
				return isPhotoMarker ? ((int)PhotoConstraint).ToString() : string.Empty;

			case Tag.photoCaptionPositionId:
				return isPhotoMarker ? ((int)PhotoCaptionPosition).ToString() : string.Empty;
			
			case Tag.photoCropId:
				return isPhotoMarker ? ((int)PhotoCrop).ToString() : string.Empty;
			
			case Tag.photoCropFactor:
				return isPhotoMarker ? ((int)PhotoCropFactor).ToString() : string.Empty;
			
			case Tag.photoWidth:
				return isPhotoMarker ?  PhotoWidth.ToString() : string.Empty;
			
			case Tag.photoHeight:
				return isPhotoMarker ?  PhotoHeight.ToString() : string.Empty;
			
			case Tag.photoScale:
				return isPhotoMarker ?  PhotoScale.ToString() : string.Empty;
			
			case Tag.photoNormalEffect:
				return isPhotoMarker ? ((int)PhotoNormalEffect).ToString() : string.Empty;
			
			case Tag.photoNormalOpacity:
				return isPhotoMarker ?  PhotoNormalOpacity.ToString() : string.Empty;
			
			case Tag.photoSelectedEffect:
				return isPhotoMarker ? ((int)PhotoSelectedEffect).ToString() : string.Empty;
			
			case Tag.photoSelectedOpacity:
				return isPhotoMarker ? PhotoSelectedOpacity.ToString() : string.Empty;
			
			case Tag.rectangleWidth:
				return hasShape ? RectangleSize.Width.ToString() : string.Empty;
			
			case Tag.rectangleHeight:
				return hasShape ? RectangleSize.Height.ToString() : string.Empty;
			
			case Tag.circleRadius:
				return hasShape && ShapeType == ShapeType.Circle ? CircleRadius.ToString() : string.Empty;
			
			case Tag.shapeType:
				return hasShape ? ((int)ShapeType).ToString() : string.Empty;
			
			case Tag.scaleShapeToMap:
				return hasShape ? ScaleShapeToMap.ToString() : string.Empty;
			
			case Tag.shapeCoords:
				return hasShape ? ShapeCoords : string.Empty;

			case Tag.anchorLocationX:
				return anchorLocationX.ToString();

			case Tag.anchorLocationY:
				return anchorLocationY.ToString();
			
			case Tag.symbolLocationX:
				return (hasSymbol && hasShape) || isPhotoMarker || isTextMarker ? symbolLocationX.ToString() : string.Empty;
			
			case Tag.symbolLocationY:
				return (hasSymbol && hasShape) || isPhotoMarker || isTextMarker ? symbolLocationY.ToString() : string.Empty;
			
			case Tag.normalTextColor:
				return isTextMarker || isPhotoMarkerWithText ? NormalTextColor : string.Empty;
			
			case Tag.selectedTextColor:
				return isTextMarker || isPhotoMarkerWithText ? SelectedTextColor : string.Empty;
			
			case Tag.textAlignH:
				return isTextMarker || isPhotoMarkerWithText ? ((int)TextAlignH).ToString() : string.Empty;
			
			case Tag.textAlignV:
				return isTextMarker ? ((int)TextAlignV).ToString() : string.Empty;
			
			case Tag.textAutoSize:
				return isTextMarker ? TextAutoSize.ToString() : string.Empty;
			
			case Tag.textPadding:
				return isTextMarker || isPhotoMarkerWithText ? TextPadding.ToString() : string.Empty;
			
			case Tag.textString:
				return isTextMarker || isPhotoMarkerWithText ? TextString : string.Empty;
			
			default:
				Debug.Fail("Unsupported Marker XML tag requested " + tag);
				return "???";
		}
	}

	public override bool HasSameAppearanceAs(TourResource resource)
	{
		Marker that = (Marker)resource;

		if (this.MarkerType != that.MarkerType)
			return false;

		if (this.AnchorLocation != that.AnchorLocation)
			return false;

		bool hasSymbol = this.MarkerType == MarkerType.Symbol || this.MarkerType == MarkerType.SymbolAndShape;
		bool hasShape = this.MarkerType != MarkerType.Symbol;
		bool isPhotoMarker = this.MarkerType == MarkerType.Photo;
		bool isTextMarker = this.MarkerType == MarkerType.Text;
		bool same;

		bool hasSameShapeCoords = !hasShape || this.ShapeCoords == that.ShapeCoords;

		if (this.MarkerType == MarkerType.Text)
		{
			// Compare shape coords unless this is a text marker with auto-size enabled.
			// In that case, the coords are calculated dynamically based on the text and
			// can't be compared to the original coords from the database.

			if (this.TextAutoSize != that.TextAutoSize)
			{
				// The auto-size option changed, so don't bother to continue comparing. 
				return false;
			}

			if (this.TextAutoSize)
			{
				// Since auto-size is set, we don't care if the shape coords match.
				hasSameShapeCoords = true;
			}
		}

		if (!hasSameShapeCoords)
			return false;

		if (isPhotoMarker)
		{
			same =
				this.PhotoPadding == that.PhotoPadding &&
				this.PhotoConstraint == that.PhotoConstraint &&
				this.PhotoCaptionPosition == that.PhotoCaptionPosition &&
				this.PhotoCrop == that.PhotoCrop &&
				this.PhotoCropFactor == that.PhotoCropFactor &&
				this.PhotoHeight == that.PhotoHeight &&
				this.PhotoNormalEffect == that.PhotoNormalEffect &&
				this.PhotoNormalOpacity == that.PhotoNormalOpacity &&
				this.PhotoScale == that.PhotoScale &&
				this.PhotoSelectedEffect == that.PhotoSelectedEffect &&
				this.PhotoSelectedOpacity == that.PhotoSelectedOpacity &&
				this.PhotoWidth == that.PhotoWidth;
			if (!same)
				return false;
		}

		if (hasSymbol)
		{
			if (this.NormalSymbolId != that.NormalSymbolId || this.SelectedSymbolId != that.SelectedSymbolId)
				return false;

			if (hasShape && this.SymbolLocation != that.SymbolLocation)
				return false;
		}

		if (hasShape)
		{
			same =
				this.ShapeType == that.ShapeType &&
				this.markerStyleId == that.markerStyleId &&
				this.RectangleSize == that.RectangleSize &&
				this.ScaleShapeToMap == that.ScaleShapeToMap;
			if (!same)
				return false;

			if (ShapeType == ShapeType.Circle && this.CircleRadius != that.CircleRadius)
				return false;
		}

		if (isTextMarker || (isPhotoMarker && this.PhotoCaptionPosition != PhotoCaptionPositionType.None))
		{
			same =
				this.fontStyleId == that.fontStyleId &&
				this.NormalTextColor == that.NormalTextColor &&
				this.TextPadding == that.TextPadding &&
				this.SelectedTextColor == that.SelectedTextColor &&
				this.TextAlignH == that.TextAlignH &&
				this.TextAlignV == that.TextAlignV &&
				this.TextAutoSize == that.TextAutoSize &&
				this.TextPadding == that.TextPadding &&
				this.TextString == that.TextString;
			if (!same)
				return false;
		}

		return true;
	}

	private void DeriveMarkerShape()
	{
		////// SVG
        if (shapeCoords.StartsWith("<svg"))
        {
            shapeRectangle = new Rectangle(0, 0, 64, 64);
            return;
        }

        MarkerShape markerShape = new MarkerShape(ShapeType, MarkerStyle.LineWidth);
		markerShape.ConvertCoordsToPoints(ShapeType, shapeCoords);
        
        if (markerShape.Points == null)
        {
            // This should never happen, but if it does, manufacture valid coords to avoid an Unexpected
            // MapsAlive error. It handles a case that occurred on 2022-04-25 where after editing a hybrid
            // marker, the runtime created coords "1,3,NaN,NaN,NaN,NaN,NaN,NaN,NaN...". This code replaces
            // the bad coords with a small triangle to serve as a clue if/when the problem happens again.
            shapeCoords = "-1,3,0,0,0,32,32,32";
            markerShape.ConvertCoordsToPoints(ShapeType, shapeCoords);
            UpdateDatabase();
        }

		shapePoints = MarkerShape.CopyPoints(markerShape.Points);
		shapeRectangle = markerShape.ContainingRectangle;
	}

	public Point[] ShapePoints
	{
		get
		{
			if (shapePoints == null)
				DeriveMarkerShape();
			return shapePoints;
		}
		set { shapePoints = value; }
	}

	public Rectangle ShapeRectangle
	{
		get
		{
			if (shapeRectangle == Rectangle.Empty)
				DeriveMarkerShape();
			return shapeRectangle;
		}
		set
		{
			shapeRectangle = value;
		}
	}

	private void SetShapeSizes(int circleRadius, int rectangleWidth, int rectangleHeight)
	{
		// Set the shape's rectangle size.
		if (rectangleWidth == 0 || rectangleHeight == 0)
		{
			// This marker was created with an older version of MapsAlive when we did
			// not keep the rectangle size in the DB. We only had the shape coords
			if (ShapeType == ShapeType.Rectangle)
			{
				Size rectangleSize = ShapeCoordsRectangleSize;
				rectangleWidth = rectangleSize.Width;
				rectangleHeight = rectangleSize.Height;
			}
			else
			{
				// Set the default in case the user changes this shape to a rectangle.
				rectangleWidth = defaultRectangleSize.Width;
				rectangleHeight = defaultRectangleSize.Height;
			}
		}
		RectangleSize = new Size(rectangleWidth, rectangleHeight);

		// Set the shape's circle radius.
		if (circleRadius == 0)
		{
			// This marker was created with an older version of MapsAlive when we did
			// not keep the circle radius in the DB. We only had the shape coords.
			if (ShapeType == ShapeType.Circle)
			{
				circleRadius = ShapeCoordsCircleRadius;
			}
			else
			{
				// Set the default in case the user changes this shape to a circle.
				circleRadius = defaultCircleRadius;
			}
		}
		CircleRadius = circleRadius;
	}

	public override TourResourceType ResourceType
	{
		get { return TourResourceType.Marker; }
	}

	public Point AnchorLocation
	{
		get { return new Point(anchorLocationX, anchorLocationY); }
		set
		{
			anchorLocationX = value.X;
			anchorLocationY = value.Y;
		}
	}

	public bool IsExclusive
	{
		get { return isExclusive; }
		set { isExclusive = value; }
	}

	public FontStyleResource FontStyleResource
	{
		get
		{
			if (_fontStyleResource == null)
				_fontStyleResource = Account.GetCachedFontStyle(fontStyleId);
			return _fontStyleResource;
		}
		set
		{
			_fontStyleResource = value;
			fontStyleId = value == null ? 0 : _fontStyleResource.Id;
		}
	}

	public int FontStyleResourceId
	{
		get { return fontStyleId; }
		set { fontStyleId = value; }
	}

	public int MarkerStyleId
	{
		get { return markerStyleId; }
		set
		{
			_markerStyle = null;
			markerStyleId = value;
		}
	}

	public MarkerStyle MarkerStyle
	{
		get
		{
			if (_markerStyle == null)
				_markerStyle = Account.GetCachedMarkerStyle(markerStyleId);
			return _markerStyle;
		}
		set
		{
			_markerStyle = value;
			markerStyleId = value == null ? 0 : _markerStyle.Id;
		}
	}

	// The normal and selected preview size image bytes are obtained from the actual size image
	// bytes. They are cached during a session, but are not persisted in the database since they
	// can quickly be recreated by scaling the actual size image bytes.
	public Byte[] NormalScaledImageBytes
	{
		get
		{
			if (_normalScaledImageBytes.Length == 0)
				_normalScaledImageBytes = ScaledImageBytes(NormalActualImageBytes);
			return _normalScaledImageBytes;
		}
		set { _normalScaledImageBytes = value; }
	}

	public Byte[] SelectedScaledImageBytes
	{
		get
		{
			if (_selectedScaledImageBytes.Length == 0)
				_selectedScaledImageBytes = ScaledImageBytes(SelectedActualImageBytes);
			return _selectedScaledImageBytes;
		}
		set { _selectedScaledImageBytes = value; }
	}

	// The normal and selected actual size image bytes are generated on-the-fly whenever the
	// marker changes and then cached in session memory. They are also kept in the database
	// so that the graphics processing to create them is only ever performed once. 
	public Byte[] NormalActualImageBytes
	{
		get
		{
			if (_normalActualImageBytes.Length == 0)
				_normalActualImageBytes = GenerateMarkerStateImageBytes(MarkerState.Normal);
			return _normalActualImageBytes;
		}
		set { _normalActualImageBytes = value; }
	}

	public Byte[] SelectedActualImageBytes
	{
		get
		{
			if (_selectedActualImageBytes.Length == 0)
				_selectedActualImageBytes = GenerateMarkerStateImageBytes(MarkerState.Selected);
			return _selectedActualImageBytes;
		}
		set { _selectedActualImageBytes = value; }
	}

	private byte[] ScaledImageBytes(Byte[] bytes)
	{
		Bitmap bitmap = Utility.BitmapFromBytes(bytes);
		if (bitmap.Width > maxPreviewDimension || bitmap.Height > maxPreviewDimension)
		{
			// The image is too big for use as a preview.
			Bitmap scaledBitmap = Utility.ScaledBitmap(bitmap, new Size(maxPreviewDimension, maxPreviewDimension), true);
			bitmap = scaledBitmap;
			bytes = Utility.ImageToByteArray(bitmap, ImageFormat.Png);
		}
		return bytes;
	}

	public string ShapeCoords
	{
		get { return shapeCoords; }
		set	{ shapeCoords = value; }
	}
	
	public Point SymbolLocation
	{
		get { return new Point(symbolLocationX, symbolLocationY); }
		set
		{	
			symbolLocationX = value.X;
			symbolLocationY = value.Y;
		}
	}

	public int TourViewId
	{
		get { return tourViewId; }
	}
	
	public override void AppearanceChanged()
	{
		base.AppearanceChanged();
		markerDefinition = null;
		FlushMarkerImageBytes();
	}

	public static string ConvertEffects(string rawEffects)
	{
		string effects = rawEffects.Trim().ToLower();
		effects = effects.Replace("blend", "-1,1");
		effects = effects.Replace("innerglow", "-1,2");
		effects = effects.Replace("glow", "-1,3");
		effects = effects.Replace("shadow", "-1,4");
		
		// Allow a semicolon to be used to separate effects.
		effects = effects.Replace(';', ',');
		
		return effects;
	}

	private static BaseMarkerRuleSet CreateActionRuleSet()
	{
		BaseMarkerRule clickRule = new BaseMarkerRule(MarkerEventType.Click);
		BaseMarkerRule mouseEnterRule = new BaseMarkerRule(MarkerEventType.MouseEnter);
		BaseMarkerRule mouseExitRule = new BaseMarkerRule(MarkerEventType.MouseExit);
		return new BaseMarkerRuleSet(clickRule, mouseEnterRule, mouseExitRule);
	}

	private Size CalculatePhotoImageAreaSize(Size imageSize)
	{
		Size imageAreaSize = Size.Empty;

		if (PhotoConstraint == PhotoConstraintType.Scale)
		{
			// Scale the image by a user-supplied percentage.
			float scalingFactor = (float)PhotoScale / 100;
			imageAreaSize.Width = (int)(scalingFactor * imageSize.Width);
			imageAreaSize.Height = (int)(scalingFactor * imageSize.Height);
		}
		else
		{
			// Scale the image to fit within the size constraints.
			switch (PhotoConstraint)
			{
				case PhotoConstraintType.Width:
					imageAreaSize.Width = PhotoWidth;
					imageAreaSize.Height = imageSize.Height;
					break;

				case PhotoConstraintType.Height:
					imageAreaSize.Width = imageSize.Width;
					imageAreaSize.Height = PhotoHeight;
					break;

				case PhotoConstraintType.WidthAndHeight:
					imageAreaSize = CalculatePhotoImageContainerSize(imageSize);
					break;
			}

			imageAreaSize = Utility.ScaledImageSize(imageSize, imageAreaSize);
		}

		return imageAreaSize;
	}

	private Size CalculatePhotoImageContainerSize(Size imageSize)
	{
		Size containerSize = Size.Empty;
		
		bool cropImage = PhotoConstraint == PhotoConstraintType.WidthAndHeight && PhotoCrop >= PhotoCropType.CropCenter;

		if (cropImage)
		{
			// A crop factor setting of 0 means 1x, 1 means 2x, etc.
			int cropFactor = PhotoCropFactor + 1;

			// Make one dimension of the container the same as the narrow end of the image
			// so that the image will exactly fill the container area in the narrow dimension
			// and grow beyond the constrained area in the other dimension. Later we can crop
			// the part that grows out of the constrained area.
			if (imageSize.Width >= imageSize.Height)
			{
				// Square or wide image. Height is narrow end.
				containerSize.Width = int.MaxValue;
				containerSize.Height = PhotoHeight * cropFactor;
			}
			else
			{
				// Tall image. Width is narrow end.
				containerSize.Width = PhotoWidth * cropFactor;
				containerSize.Height = int.MaxValue;
			}
		}
		else
		{
			containerSize = new Size(PhotoWidth, PhotoHeight);
		}

		return containerSize;
	}

	private static BaseShape CreateInvisibleShape(int shapeId, Size size)
	{
		BaseShape shape = new BaseShape((AvantLogic.MapsAlive.Engine.ShapeType)((int)ShapeType.Rectangle));
		shape.Id = shapeId;
		shape.FillColorOpacity = 0;
		shape.LineColorOpacity = 0;
		shape.LineWidth = 0;
		shape.SymbolLocation = new Point(size.Width / 2, size.Height / 2);
		shape.Effects = string.Empty;
		shape.Coords = string.Format("0,0,{0},{1}", size.Width, size.Height);
		shape.Rectangle = new Rectangle(0, 0, size.Width, size.Height);
		return shape;
	}

	private static BaseMarkerRuleSet CreateJavascriptRuleSet()
	{
		BaseMarkerRule javascriptClickRule = new BaseMarkerRule(MarkerEventType.Click);
		BaseMarkerRule javascriptMouseEnterRule = new BaseMarkerRule(MarkerEventType.MouseEnter);
		BaseMarkerRule javascriptMouseExitRule = new BaseMarkerRule(MarkerEventType.MouseExit);
		return new BaseMarkerRuleSet(javascriptClickRule, javascriptMouseEnterRule, javascriptMouseExitRule);
	}

	public MarkerDefinition CreateMarkerDefinition(int idSeed, Tour tour, TourView tourView)
	{
		// Create the Symbol components of this marker.
		BaseSymbol normalBaseSymbol = null;
		BaseSymbol selectedBaseSymbol = null;
		Point anchorDelta = new Point(0, 0);

		int normalId = idSeed * 2;
		int selectedId = normalId + 1;

		if (NormalSymbolId > 0)
		{
			Symbol normalSymbol = Account.GetCachedSymbol(NormalSymbolId);
			Byte[] normalSymbolImageBytes = normalSymbol.Bytes;
			if (normalSymbolImageBytes != null)
			{
				Bitmap normalSymbolBitmap = Utility.BitmapFromBytes(normalSymbolImageBytes);
				normalBaseSymbol = new BaseSymbol(normalId, normalSymbolBitmap);

				// Translate the 0,0 based anchor location to a delta from the marker's center.
				Size symbolSize = normalSymbolBitmap.Size;
				int deltaX = anchorLocationX > -1 ? anchorLocationX - (symbolSize.Width / 2) : 0;
				int deltaY = anchorLocationY > -1 ? anchorLocationY - (symbolSize.Height / 2) : 0;
				anchorDelta = new Point(deltaX, deltaY);
			}
		}
		else if (MarkerType == MarkerType.Text)
		{
			normalBaseSymbol = CreateTextSymbol(MarkerState.Normal, normalId, tourView);
		}
		else if (MarkerType == MarkerType.Photo)
		{
			normalBaseSymbol = CreatePhotoSymbol(MarkerState.Normal, normalId, tourView);
		}

		if (SelectedSymbolId > 0)
		{
			Symbol selectedSymbol = Account.GetCachedSymbol(SelectedSymbolId);
			Byte[] selectedSymbolImageBytes = selectedSymbol.Bytes;
			if (selectedSymbolImageBytes != null)
			{
				Bitmap selectedSymbolBitmap = Utility.BitmapFromBytes(selectedSymbolImageBytes);
				selectedBaseSymbol = new BaseSymbol(selectedId, selectedSymbolBitmap);
			}

		}
		else if (MarkerType == MarkerType.Text)
		{
			selectedBaseSymbol = CreateTextSymbol(MarkerState.Selected, selectedId, tourView);
		}
		else if (MarkerType == MarkerType.Photo)
		{
			selectedBaseSymbol = CreatePhotoSymbol(MarkerState.Selected, selectedId, tourView);
		}

		// Create the Shape components of this marker.
		BaseShape normalShape = null;
		BaseShape selectedShape = null;
		Point symbolLocation = SymbolLocation;

		if (MarkerType == MarkerType.Photo || (MarkerType == MarkerType.Text && ShapeType == ShapeType.Rectangle))
		{
		    Size adjustedSize = normalBaseSymbol.Bitmap.Size;

			int border = MarkerStyle.LineWidth / 2;
			adjustedSize.Width += border * 2;
			adjustedSize.Height += border * 2;

		    // Dynamically set the shape size for the slide that uses this photo marker.
		    ShapeCoordsRectangleSize = adjustedSize;

		    // Force the shape's rectangle to get recalculated.
		    shapeRectangle = Rectangle.Empty;
		}

		if (ShapeType != ShapeType.None)
		{
			if (ShapeType == ShapeType.Circle)
			{
				int radius = ShapeCoordsCircleRadius;
				if (MarkerType == MarkerType.Text)
					radius += ((MarkerStyle.LineWidth + 1) / 2);
				ShapeCoords = string.Format("{0},{0},{1}", ((MarkerStyle.LineWidth + 1) / 2) + radius - 1, radius);
			}

			string shapeCoords = this.ShapeCoords;

			AvantLogic.MapsAlive.Engine.ShapeType baseShapeType = (AvantLogic.MapsAlive.Engine.ShapeType)((int)ShapeType);

			// Create the Normal shape.
			normalShape = new BaseShape(baseShapeType);
			normalShape.Id = 1;
			normalShape.FillColor = Utility.HexToColor(MarkerStyle.NormalFillColor);
			normalShape.FillColorOpacity = MarkerStyle.NormalFillColorOpacity;
			normalShape.LineColor = Utility.HexToColor(MarkerStyle.NormalLineColor);
			normalShape.LineColorOpacity = MarkerStyle.NormalLineColorOpacity;
			normalShape.LineWidth = MarkerStyle.LineWidth;
			normalShape.SymbolLocation = symbolLocation;
			normalShape.Effects = ConvertEffects(MarkerStyle.NormalShapeEffects);
			normalShape.Coords = shapeCoords;
			normalShape.Rectangle = ShapeRectangle;

			// Create the Selected shape.
			selectedShape = new BaseShape(baseShapeType);
			selectedShape.Id = 2;
			selectedShape.FillColor = Utility.HexToColor(MarkerStyle.SelectedFillColor);
			selectedShape.FillColorOpacity = MarkerStyle.SelectedFillColorOpacity;
			selectedShape.LineColor = Utility.HexToColor(MarkerStyle.SelectedLineColor);
			selectedShape.LineColorOpacity = MarkerStyle.SelectedLineColorOpacity;
			selectedShape.LineWidth = MarkerStyle.LineWidth;
			selectedShape.SymbolLocation = symbolLocation;
			selectedShape.Effects = ConvertEffects(MarkerStyle.SelectedShapeEffects);
			selectedShape.Coords = shapeCoords;
			selectedShape.Rectangle = ShapeRectangle;
		}
		else if (MarkerType == MarkerType.Symbol && SelectedSymbolId == 0)
		{
			// When there is no selected symbol, show the normal symbol in its place.
			selectedBaseSymbol = normalBaseSymbol;
		}
		else if (MarkerType == MarkerType.Symbol && NormalSymbolId == 0)
		{
			// This is a symbol marker that has no normal symbol (the other is "no symbol"). Create an
			// invisible rectangle shape for the normal symbol that is the size of the selected symbol.
			// If we don't do this, nothing will happen when the mouse moves over the hotspot. Note that
			// we also create the shape for the selected symbol. If we don't, when the mouse is just at
			// the edge of a normal symbol that has transparency, the marker will flicker rapidly as it
			// toggles between the symbol and the invisble shape.
			Size size = selectedBaseSymbol.Bitmap.Size;
			normalShape = CreateInvisibleShape(1, size);
			selectedShape = CreateInvisibleShape(2, size);
		}

		// Create the marker's definition from the symbol and shape appearances plus the rules.
		// Note that the MapsAlive Engine supports three distinct appearances: normal, rollover, and selected.
		// In the web app we don't support rollover because it overly complicates the user interface and
		// provides little value.  Instead, we use the selected appearance for rollover.
		MarkerDefinition markerDefinition = new MarkerDefinition(idSeed, string.Empty, null);
		markerDefinition.Base.BaseNormalAppearance = new BaseMarkerAppearance(normalBaseSymbol, normalShape);
		markerDefinition.Base.BaseSelectedAppearance = new BaseMarkerAppearance(selectedBaseSymbol, selectedShape);
		markerDefinition.AnchorDelta = anchorDelta;

		return markerDefinition;
	}

	public static BaseMarker CreateMarkerInstance(MarkerDefinition markerDefinition, TourView tourView, BaseLayer layer, int zIndex)
	{
		int markerInstanceId = tourView.Id;
		
		// Create and initialize a base marker for the marker's definition.
		BaseMarker baseMarker = new BaseMarker(markerInstanceId, layer.Id);
		baseMarker.BaseMarkerDefinition = markerDefinition.Base;
		baseMarker.Name = string.Empty;
		baseMarker.Location = new Point(tourView.MarkerX, tourView.MarkerY);

		// If the marker is located to the right or below the map, give it a negative location.
		TourPage tourPage = tourView.TourPage;
		Size mapSize = tourPage.MapCanZoom && tourPage.MapImage.HasFile ? tourPage.MapImage.Size : tourPage.ScaledMapSize;
		if (baseMarker.Location.X > mapSize.Width || baseMarker.Location.Y > mapSize.Height)
			baseMarker.Location = new Point(-mapSize.Width, -mapSize.Height);

		baseMarker.AnchorDelta = markerDefinition.AnchorDelta;
		baseMarker.PctX = tourView.MarkerPctX;
		baseMarker.PctY = tourView.MarkerPctY;
		baseMarker.Rotation = tourView.MarkerRotation;
		baseMarker.ZIndex = zIndex;
		baseMarker.BaseLayer = layer;
		baseMarker.ZoomThreshold = tourView.MarkerZoomThreshold;
		
		// Create rule sets
		BaseMarkerRuleSet actionRuleSet = CreateActionRuleSet();
		BaseMarkerRuleSet javascriptRuleSet = CreateJavascriptRuleSet();
		
		// Create a marker instance for this specific view.
		baseMarker.AddBaseMarkerInstance(actionRuleSet, javascriptRuleSet);
		
		// Set the view's tool tip if it has one.
		DefineToolTip(baseMarker.MarkerInstance, tourView);

        bool isBound = tourView.MarkerIsBound;
        if (!isBound && !tourView.Tour.V3CompatibilityEnabled)
        {
            // Treat a zoomable marker on a non-zoomable map the same as a bound marker. This allows a marker
            // that is sized for an original size map, to scale with the map if the map is used without zooming.
            // Just like a bound marker, the shape will scale up or down to match the map scale the same as
            // Ready May shapes for states and counties.
            isBound = tourView.MarkerZooms && !tourPage.MapCanZoom;
        }

        // Set the marker's attributes.
        baseMarker.MarkerInstance.IsBound = isBound;
        baseMarker.MarkerInstance.IsDisabled = tourView.MarkerIsDisabled;
		baseMarker.MarkerInstance.IsNotAnchored = tourView.MarkerIsNotAnchored;
		baseMarker.MarkerInstance.IsHidden = tourView.MarkerIsHidden;
		baseMarker.MarkerInstance.IsLocked = tourView.MarkerIsLocked;
		baseMarker.MarkerInstance.IsShapeOnly = markerDefinition.Base.BaseNormalAppearance.BaseSymbol == null && markerDefinition.Base.BaseSelectedAppearance.BaseSymbol == null;
		baseMarker.MarkerInstance.IsRoute = tourView.MarkerIsRoute;
		baseMarker.MarkerInstance.IsStatic = tourView.MarkerIsStatic;
		baseMarker.MarkerInstance.MarkerZooms = (tourView.MarkerZooms && tourPage.MapCanZoom) || baseMarker.MarkerInstance.IsBound;

		// Set the marker's click and mouse over/out actions.
		SetClickAction(tourView, baseMarker);
		SetRolloverAction(tourView, baseMarker);
		SetRolloutAction(tourView, baseMarker);

		return baseMarker;
	}

	private Bitmap CreatePhotoImage(MarkerState state, TourView tourView)
	{
		Bitmap placeholderBitmap = null;
		Bitmap photoImageBitmap = null;
		
		bool createPlaceholderImage = tourView == null || !tourView.HasImage;

		if (createPlaceholderImage)
		{
			placeholderBitmap = (Bitmap)MapsAliveState.Retrieve(MapsAliveObjectType.PhotoMarkerPlaceholderImage);
			if (placeholderBitmap == null)
			{
				string fileLocation = FileManager.WebAppFileLocationAbsolute("Images", "PhotoMarkerPlaceholder.jpg");
				placeholderBitmap = new Bitmap(fileLocation);
				MapsAliveState.Persist(MapsAliveObjectType.PhotoMarkerPlaceholderImage, placeholderBitmap);
			}
		}

		// Get the actual size of the image to be used for this photo marker.
		Size imageSize = createPlaceholderImage ? placeholderBitmap.Size : tourView.Image.Size;

		// Determine the size of the area that the image will occupy. The area includes any
		// fill needed to align the image within the marker. It does not include the matte.
		Size imageAreaSize = CalculatePhotoImageAreaSize(imageSize);
		Debug.Assert(imageAreaSize != Size.Empty, "Photo marker image size is empty");

		if (createPlaceholderImage)
		{
			photoImageBitmap = Utility.ScaledBitmap(placeholderBitmap, imageAreaSize, false);
		}
		else
		{
			// Create a bitmap from the view's image and then scale it down to the size of the photo marker.
			// We do a special optimization here for photo marker symbols because it's expensive to create the
			// bitmap and scale it from the image bytes. We cache the bitmap in memory only for the current
			// session which means the first time here there is no savings; however, if the user is working
			// on a gallery, fussing with layout, this should improve performance.
			TourViewImage tourViewImage = (TourViewImage)tourView.Image;
			photoImageBitmap = tourViewImage.CachedBitmap;
			if (photoImageBitmap == null || tourViewImage.CachedBitmapSize != imageAreaSize)
			{
				photoImageBitmap = Utility.BitmapFromBytes(tourView.Image.Bytes);
				Bitmap scaledBitmap = Utility.ScaledBitmap(photoImageBitmap, imageAreaSize, true);
				photoImageBitmap = scaledBitmap;
				tourViewImage.CachedBitmap = photoImageBitmap;
				tourViewImage.CachedBitmapSize = imageAreaSize;
			}
		}

		return photoImageBitmap;
	}

	private Bitmap CreatePhotoImageContrained(Bitmap bitmap)
	{
		bool notCropOrAlign = 
			PhotoConstraint == PhotoConstraintType.Scale ||
			PhotoConstraint == PhotoConstraintType.Width ||
			PhotoConstraint == PhotoConstraintType.Height ||
			(PhotoConstraint == PhotoConstraintType.WidthAndHeight && PhotoCrop == PhotoCropType.ScaleAndTrim);

		if (notCropOrAlign && MarkerStyle.LineWidth < 2)
		{
			// The image is already the size needed.
			return bitmap;
		}

		// Determine the constrained size.
		int w = notCropOrAlign ? bitmap.Width : PhotoWidth;
		int h = notCropOrAlign ? bitmap.Height : PhotoHeight;
		Size constrainedSize = new Size(w, h);
		
		if (MarkerStyle.LineWidth >= 2)
		{
			// Cropping is needed due to a thick border. Note that we do this after we have already
			// sized the image because we must crop, not scale so that we preserve the aspect ratio.
			// The crop reduce the image all around by half the thickness of even borders so that the
			// so that the edges of the image won't get drawn on top of the inside of the border.
			// After croppinge, the inside edge of the border fits against the outside edge of the
			// image. If we didn't do this, the border would overlap the image. Note that we initially
			// tried enlarging the shape so that the border fit around the image without cropping it,
			// but that made photo marker rectangles larger than other rectangles that were specified
			// to be the same size.
			int extra = MarkerStyle.LineWidth / 2;
			constrainedSize.Width -= extra * 2;
			constrainedSize.Height -= extra * 2;
		}

		// Determine how we are constraining the image. If the image is not supposed to be cropped
		// or aligned, we set the contraint to crop-center to reduce the image for border thickness.
		PhotoCropType constraintType = notCropOrAlign ? PhotoCropType.CropCenter : PhotoCrop;

		// Get the negative difference between the image size and the constrained size.
		int deltaX = constrainedSize.Width - bitmap.Width;
		int deltaY = constrainedSize.Height - bitmap.Height;
		int midX = deltaX / 2;
		int midY = deltaY / 2;

		// Contrain the image by how it is placed within the contrained area. When aligning, 100% of the
		// image fits, but it has to be positioned either centered or toward one edge. When cropping,
		// part of the image does not fit so we position such that the desired portion is visible.
		int x = 0;
		int y = 0;
		switch (constraintType)
		{
			case PhotoCropType.CropCenter:
			case PhotoCropType.AlignCenter:
				{
					x = midX;
					y = midY;
					break;
				}

			case PhotoCropType.CropN:
				{
					x = midX;
					y = 0;
					break;
				}

			case PhotoCropType.CropNE:
				{
					x = deltaX;
					y = 0;
					break;
				}

			case PhotoCropType.CropE:
				{
					x = deltaX;
					y = midY;
					break;
				}

			case PhotoCropType.CropSE:
			case PhotoCropType.AlignRightOrBottom:
				{
					x = deltaX;
					y = deltaY;
					break;
				}

			case PhotoCropType.CropS:
				{
					x = midX;
					y = deltaY;
					break;
				}

			case PhotoCropType.CropSW:
				{
					x = 0;
					y = deltaY;
					break;
				}

			case PhotoCropType.CropW:
				{
					x = 0;
					y = midY;
					break;
				}

			case PhotoCropType.CropNW:
			case PhotoCropType.AlignLeftOrTop:
			default:
				{
					x = 0;
					y = 0;
					break;
				}
		}

		Bitmap bitmapContrained = new Bitmap(constrainedSize.Width, constrainedSize.Height);
		using (Graphics graphics = Graphics.FromImage(bitmapContrained))
		{
			Color transparentColor = Color.FromArgb(0, 0, 0, 0);
			graphics.Clear(transparentColor);
			graphics.DrawImage(bitmap, x, y);
		}

		return bitmapContrained;
	}

	private Bitmap CreatePhotoImageMatted(Bitmap bitmap)
	{
		// Add a matte around the photo image.
		int pad = PhotoPadding * 2;
		Bitmap bitmapWithPadding = new Bitmap(bitmap.Width + pad, bitmap.Height + pad);
		bitmapWithPadding.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

		using (Graphics graphics = Graphics.FromImage(bitmapWithPadding))
		{
			Color transparentColor = Color.FromArgb(0, 0, 0, 0);
			graphics.Clear(transparentColor);

			// Draw the image within the padding
			graphics.DrawImage(bitmap, PhotoPadding, PhotoPadding);
		}

		bitmap = bitmapWithPadding;
		return bitmap;
	}

	private Bitmap CreatePhotoImageWithEffects(Bitmap oldBitmap, PhotoEffect effect, int opacity)
	{
		// Apply effects and transparency. Note that the bitmap returned is Format32bppArgb
		// in order to support transparency. By default the images we create are Format24bppRgb.

		// Create a blank bitmap the same size as original.
		Bitmap newBitmap = new Bitmap(oldBitmap.Width, oldBitmap.Height);

		using (Graphics graphics = Graphics.FromImage(newBitmap))
		{
			float alpha = (float)opacity / 100;
			ColorMatrix colorMatrix = null;

			switch (effect)
			{
				case PhotoEffect.Color:
					colorMatrix = new ColorMatrix(new float[][]
					{
						new float[] {1, 0, 0, 0, 0},
						new float[] {0, 1, 0, 0, 0},
						new float[] {0, 0, 1, 0, 0},
						new float[] {0, 0, 0, alpha, 0},
						new float[] {0, 0, 0, 0, 1}
					});
					break;

				
				case PhotoEffect.GrayScale:
				case PhotoEffect.GrayScaleNegative:
					colorMatrix = new ColorMatrix(new float[][]
					{
						new float[] {.3f, .3f, .3f, 0, 0},
						new float[] {.59f, .59f, .59f, 0, 0},
						new float[] {.11f, .11f, .11f, 0, 0},
						new float[] {0, 0, 0, alpha, 0},
						new float[] {0, 0, 0, 0, 1}
					});
					break;

				case PhotoEffect.Sepia:
					colorMatrix = new ColorMatrix(new float[][]
					{
						new float[] {.393f, .349f, .272f, 0, 0},
						new float[] {.769f, .686f, .534f, 0, 0},
						new float[] {.189f, .168f, .131f, 0, 0},
						new float[] {0, 0, 0, alpha, 0},
						new float[] {0, 0, 0, 0, 1}
					});
					break;

				case PhotoEffect.ColorNegative:
					colorMatrix = new ColorMatrix(new float[][]
					{
						new float[] {-1, 0, 0, 0, 0},
						new float[] {0, -1, 0, 0, 0},
						new float[] {0, 0, -1, 0, 0},
						new float[] {0, 0, 0, alpha, 0},
						new float[] {1, 1, 1, 0, 1}
					});
					break;
			}

			ImageAttributes attributes = new ImageAttributes();
			attributes.SetColorMatrix(colorMatrix);

			graphics.DrawImage(oldBitmap, new Rectangle(0, 0, oldBitmap.Width, oldBitmap.Height),
			   0, 0, oldBitmap.Width, oldBitmap.Height, GraphicsUnit.Pixel, attributes);
		}

		if (effect == PhotoEffect.GrayScaleNegative)
		{
			// We just turned the image to gray scale. Now recurse to make it negative.
			return CreatePhotoImageWithEffects(newBitmap, PhotoEffect.ColorNegative, opacity);
		}

		return newBitmap;
	}

	private BaseSymbol CreatePhotoSymbol(MarkerState state, int id, TourView tourView)
	{
		// This method creates the symbol image for a photo marker.
		Bitmap bitmap;

		// Create a bitmap of image. If the user requested cropping,
		// the image that comes back is large enough to be cropped.
		bitmap = CreatePhotoImage(state, tourView);

		// Apply effects and transparency.
		int opacity = state == MarkerState.Normal ? PhotoNormalOpacity : PhotoSelectedOpacity;
		PhotoEffect effect = state == MarkerState.Normal ? PhotoNormalEffect : PhotoSelectedEffect;
		bitmap = CreatePhotoImageWithEffects(bitmap, effect, opacity);

		// Add any any fill area around the image to obey the user's request for alignment within
		// the marker's rectangle. The fill area does not include matting which is added next.
		// If the user requested scale to fit, or cropping, the image comes back accordingly.
		bitmap = CreatePhotoImageContrained(bitmap);

		// Add a matte around the image.
		if (PhotoPadding > 0)
		{
			bitmap = CreatePhotoImageMatted(bitmap);
		}

		// Add a text caption above or below the image.
		if (PhotoCaptionPosition != PhotoCaptionPositionType.None)
		{
			bitmap = CreatePhotoSymbolText(state, tourView, bitmap);
		}

		return new BaseSymbol(id, bitmap);
	}

	private Bitmap CreatePhotoSymbolText(MarkerState state, TourView tourView, Bitmap bitmap)
	{
		// Get the text.
		string text = TextString;
		if (text.Length == 0)
			text = tourView == null ? "Caption" : tourView.Title;
		
		// Get the symbol size. This is the image size plus photo padding.
		Size symbolSize = bitmap.Size;

		// Determine how big a rectangle the text needs.
		Size outerTextSize = new Size(symbolSize.Width, int.MaxValue);
		int padding = TextPadding * 2;
		Size innerTextSize = new Size(outerTextSize.Width - padding, outerTextSize.Height - padding);
		
		// Measure the actual text and trim the inner size to just contain the text.
		innerTextSize = MeasureText(text, innerTextSize);

		// If there is also photo padding, remove it where the text and image meet.
		int textPhotoBoundaryPadding = PhotoPadding;
		
		// Change the outer size to conform to the trimmed inner size.
		outerTextSize.Width = innerTextSize.Width + padding;
		outerTextSize.Height = innerTextSize.Height + padding - textPhotoBoundaryPadding;

		// The overall width cannot be narrower than the symbol width.
		if (outerTextSize.Width < symbolSize.Width)
		{
			outerTextSize.Width = symbolSize.Width;
			innerTextSize.Width = outerTextSize.Width - padding;
		}

		// Increate the size of the marker by the extra height needed for the text.
		Bitmap bitmapWithText = new Bitmap(symbolSize.Width, symbolSize.Height + outerTextSize.Height);
		bitmapWithText.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);
		
		// Determine where the image will be placed.
		int imageX = symbolSize.Width >= outerTextSize.Width ? 0 : (outerTextSize.Width - symbolSize.Width) / 2;
		int imageY = PhotoCaptionPosition == PhotoCaptionPositionType.Bottom ? 0 : outerTextSize.Height;
		
		// Determine where the text will be placed.
		int textX = outerTextSize.Width >= symbolSize.Width ? 0 : ((outerTextSize.Width - outerTextSize.Width) / 2);
		textX += TextPadding;
		int textY = TextPadding;
		textY += PhotoCaptionPosition == PhotoCaptionPositionType.Top ? 0 : symbolSize.Height - textPhotoBoundaryPadding;

		using (Graphics graphics = Graphics.FromImage(bitmapWithText))
		{
			Color transparentColor = Color.FromArgb(0, 0, 0, 0);
			graphics.Clear(transparentColor);

			// Draw the image.
			graphics.DrawImage(bitmap, imageX, imageY);

			// Draw the text.
			string textColor = state == MarkerState.Normal ? NormalTextColor : SelectedTextColor;
			Rectangle textRect = new Rectangle(textX, textY, innerTextSize.Width, innerTextSize.Height);
			TextAlignV = TextAlignType.RightOrBottom;
			DrawText(text, textColor, textRect, graphics);
		}

		return bitmapWithText;
	}

	private BaseSymbol CreateTextSymbol(MarkerState state, int id, TourView tourView)
	{
		string text = TextString;
        Size outerTextSize;
        Size innerTextSize;

        string sampleText;
        // Determine the outer size of the Text marker's shape.
        if (ShapeType == ShapeType.Circle)
        {
            int w = CircleRadius * 2;
            outerTextSize = new Size(w, w);
            sampleText = w < 50 ? "T" : "Text";
        }
        else
        {
            outerTextSize = RectangleSize;
            sampleText = outerTextSize.Width < 50 ? "T" : "Text";
        }

        if (text.Length == 0)
			text = tourView == null ? sampleText : tourView.Title;

		// Create an inner size that is smaller than the outer size by the padding amount.
		int padding = TextPadding * 2;
		innerTextSize = new Size(outerTextSize.Width - padding, outerTextSize.Height - padding);

		if (TextAutoSize)
		{
			// Measure the actual text and trim the inner size to just contain the text.
			innerTextSize = MeasureText(text, innerTextSize);
			
			// Change the outer size to conform to the trimmed inner size.
			outerTextSize.Width = innerTextSize.Width + padding;
			outerTextSize.Height = innerTextSize.Height + padding;
		}

		// Set the internal shape coords to shape's size.
		if (ShapeType == ShapeType.Circle)
			ShapeCoordsCircleRadius = outerTextSize.Width / 2;
		else
			ShapeCoordsRectangleSize = outerTextSize;

		// Determine if the background will be transparent. If the marker style fill color is 100%, we'll set
		// the transparent flag to false. Whether or not the flag is set has a significant impact on how the
		// text symbol image gets drawn and later rendered. For a brief time we had a checkbox
		// on the Edit Marker page to let the user choose transparent, but that complicated the user interface
		// and made things confusing with respect to the marker style background opacity. The current approach
		// lets us render the symbol one way or another by using the opacity as a flag.
		MarkerStyle markerStyle = Account.GetCachedMarkerStyle(markerStyleId);
		int markerFillColorOpacity = state == MarkerState.Normal ? markerStyle.NormalFillColorOpacity : markerStyle.SelectedFillColorOpacity;
		bool textBackgroundIsTransparent = markerFillColorOpacity < 100;
		
		// Create a bitmap to draw on.
		Bitmap bitmap = new Bitmap(outerTextSize.Width, outerTextSize.Height);
		
		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			if (textBackgroundIsTransparent || ShapeType == ShapeType.Circle)
			{
				if (ShapeType == ShapeType.Circle)
					graphics.SmoothingMode = SmoothingMode.AntiAlias;

				// Make the bitmap transparent. If the shape is a cirle, we have to do this even if the
				// background is not transparent so that the outside of the circle will be transparent.
				graphics.Clear(Color.FromArgb(0, 0, 0, 0));
			}

			if (!textBackgroundIsTransparent)
			{
				// Get the background color from the marker style.
				Color backgroundColor2 = Utility.HexToColor(state == MarkerState.Normal ? markerStyle.NormalFillColor : markerStyle.SelectedFillColor);
				
				// Create a nearly identitical color from it by changing just one bit in the blue channel.
				int r = backgroundColor2.R;
				int g = backgroundColor2.G;
				int b = backgroundColor2.B;
				if (b == 255)
					b = 254;
				else
					b += 1;
				Color backgroundColor1 = Color.FromArgb(r, g, b);
				
				// Create brushes from the colors.
				SolidBrush backgroundBrush1 = new SolidBrush(backgroundColor1);
				SolidBrush backgroundBrush2 = new SolidBrush(backgroundColor2);

				// The logic addressesa bug where sometimes a text marker's symbol image got corrupted by Flash
				// and appeared as invisible on the map. The problem seemed to happen when most of the pixels in
				// the image are the same. To lessen the chances of the problem occurring, we draw the background
				// color as concentric rectangles/circles so that going top to bottom, or left to right, every
				// other pixel is a different color. See if this logic can now be eliminated.
				
				// Alternate pixels for half of the rectangle's shortest side.
				int limit = Math.Min(outerTextSize.Width, outerTextSize.Height) / 2;
				limit -= 1;
				int w = outerTextSize.Width;
				int h = outerTextSize.Height;
				
				for (int i = 0; i < limit; i++)
				{
					// Adjust the x/y offset and rectangle size so that we draw a samaller and smaller
					// rectangle each time through the loop.
					int offset = i * 2;
					Rectangle outerRect = new Rectangle(i, i, w - offset, h - offset);
					
					// Switch to the other color each time through the loop.
					SolidBrush backgroundBrush = i % 2 == 0 ? backgroundBrush1 : backgroundBrush2;
					
					if (ShapeType == ShapeType.Circle)
					{
						graphics.FillEllipse(backgroundBrush, outerRect);
					}
					else
					{
						graphics.FillRectangle(backgroundBrush, outerRect);
					}
				}
			}
			
			// Draw the text.
			Rectangle innerRect = new Rectangle(TextPadding, TextPadding, innerTextSize.Width, innerTextSize.Height);
			string textColor = state == MarkerState.Normal ? NormalTextColor : SelectedTextColor;
			graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
			DrawText(text, textColor, innerRect, graphics);
		}

		BaseSymbol baseSymbol = new BaseSymbol(id, bitmap);

		// Render transparent text markers as jpg images to avoid the problem described in OnTime #869. If the
		// flag is false, we don't render as a jpg because doing so creates artifacts in the solid fill color.
		baseSymbol.RenderAsJpg = textBackgroundIsTransparent;

		return baseSymbol;
	}

	private static void DefineToolTip(BaseMarkerInstance markerInstance, TourView tourView)
	{
		bool toolTipAllowed = ToolTipAllowed(tourView);
		string tooltipText = tourView.ToolTip;

		if (toolTipAllowed && tooltipText.Length == 0 && tourView.HasNoContent)
			tooltipText = tourView.Title;
		
		if (!toolTipAllowed || tooltipText.Length == 0)
			return;

		markerInstance.SetTooltip(tourView.Tour.ThemeId, tooltipText);
	}

	private void DrawCircle(BaseShape shape, Rectangle bounds, Graphics graphics, SolidBrush brush, Pen pen)
	{
		int lineWidth = shape.LineWidth;
		if (MarkerType == MarkerType.Text)
			lineWidth += 1;
		int halfBorder = lineWidth / 2;
		Rectangle rect = new Rectangle(halfBorder, halfBorder, shape.Rectangle.Width, shape.Rectangle.Height);
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		graphics.FillEllipse(brush, rect);
		if (shape.LineWidth != 0)
			graphics.DrawEllipse(pen, rect);
	}

	private void DrawHybrid(BaseShape shape, Graphics graphics, SolidBrush brush, Pen pen)
	{
		int index = 0;

		while (index < ShapePoints.Length)
		{
			// Get the "point" that starts the section.
			Point point = ShapePoints[index];
			Debug.Assert(point.X == -1, "Expected -1");
			ShapeType shapeType = (ShapeType)point.Y;

			// Get the range of points for the section.
			index++;
			int start = index;
			int end = 0;
			while (index < ShapePoints.Length)
			{
				end = index;
				if (ShapePoints[index].X == -1)
				{
					// We have found the start of the next section.
					end--;
					break;
				}
				index++;
			}

			// Create an array of points for the section and copy the section points to it.
			int sectionLength = end - start + 1;
			Point[] sectionPoints = new Point[sectionLength];
			for (int i = 0; i < sectionLength; i++)
			{
				sectionPoints[i] = ShapePoints[start + i];
			}

			// Draw the section
			int x;
			int y;
			int w;
			int h;
			Rectangle rect;
			
			switch (shapeType)
			{
				case ShapeType.Circle:
					x = sectionPoints[0].X;
					y = sectionPoints[0].Y;
					int radius = sectionPoints[1].X;
					w = radius * 2;
					rect = new Rectangle(x - radius, y - radius, w, w);
					graphics.FillEllipse(brush, rect);
					if (shape.LineWidth != 0)
						graphics.DrawEllipse(pen, rect);
					break;
				
				case ShapeType.Rectangle:
					x = sectionPoints[0].X;
					y = sectionPoints[0].Y;
					w = sectionPoints[1].X - x + 1;
					h = sectionPoints[1].Y - y + 1;
					rect = new Rectangle(x, y, w, h);
					graphics.FillRectangle(brush, rect);
					if (shape.LineWidth != 0)
						graphics.DrawRectangle(pen, rect);
					break;
				
				case ShapeType.Polygon:
					graphics.FillPolygon(brush, sectionPoints);
					if (shape.LineWidth > 0)
						graphics.DrawPolygon(pen, sectionPoints);
					break;
				
				case ShapeType.Line:
					if (shape.LineWidth > 0)
						graphics.DrawLines(pen, sectionPoints);
					break;
			}
		}
	}

	private void DrawMarkerAppearance(BaseMarkerAppearance appearance, Rectangle bounds, Color backgroundColor, Graphics graphics)
	{
		// This method is used to create the preview images that are used in MapsAlive. Previews images
		// appear in the EditHotspotActions Editor and as the small combo icons that appear in marker lists.

		if (!appearance.Defined)
			return;

		Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
		
		using (Graphics localGraphics = Graphics.FromImage(bitmap))
		{
			localGraphics.Clear(backgroundColor);

			// Draw the symbol on top of the shape with both images at their full size.
			bool hasShape = appearance.BaseShape != null;
			Point symbolLocation = hasShape ? appearance.BaseShape.SymbolLocation : new Point(-1, -1);
			if (hasShape)
			{
				ShapeType shapeType = (ShapeType)((int)appearance.BaseShape.ShapeType);
				DrawShape(shapeType, appearance.BaseShape, bounds, localGraphics);
			}
			DrawSymbol(appearance.BaseSymbol, bounds, symbolLocation, backgroundColor, localGraphics);
		}

		// Draw the composite image onto the drawing shape.
		graphics.DrawImage(bitmap, 0, 0);
	}

	private void DrawPolygon(ShapeType shapeType, BaseShape shape, Graphics graphics, SolidBrush brush, Pen pen)
	{
        ////// SVG
        if (shapeCoords.StartsWith("<svg"))
            return;

        // Draw the polygon.
		graphics.SmoothingMode = SmoothingMode.AntiAlias;
		
		if (shapeType == ShapeType.Polygon)
		{
			graphics.FillPolygon(brush, ShapePoints);
			if (shape.LineWidth > 0)
				graphics.DrawPolygon(pen, ShapePoints);
		}
		else if (shapeType == ShapeType.Hybrid)
		{
			DrawHybrid(shape, graphics, brush, pen);
		}
		else
		{
			if (shape.LineWidth > 0)
				graphics.DrawLines(pen, ShapePoints);
		}
	}

	private void DrawRectangle(BaseShape shape, Rectangle bounds, Graphics graphics, SolidBrush brush, Pen pen)
	{
		int offset = shape.LineWidth / 2;
		Rectangle rect = new Rectangle(offset, offset, bounds.Width - shape.LineWidth, bounds.Height - shape.LineWidth);
		graphics.FillRectangle(brush, rect);

		if (shape.LineWidth != 0)
			graphics.DrawRectangle(pen, rect);
	}

	private void DrawShape(ShapeType shapeType, BaseShape shape, Rectangle bounds, Graphics graphics)
	{
		if (shape == null)
			return;

		Pen pen = null;
		if (shape.LineWidth > 0)
		{
			pen = new Pen(shape.LineColor);
			pen.Width = shape.LineWidth;
			pen.DashStyle = DashStyle.Solid;
		}
		SolidBrush brush = new SolidBrush(shape.FillColor);

		// Draw the shape's shape.
		switch (shapeType)
		{
			case ShapeType.Rectangle:
				DrawRectangle(shape, bounds, graphics, brush, pen);
				break;

			case ShapeType.Circle:
				DrawCircle(shape, bounds, graphics, brush, pen);
				break;

			case ShapeType.Polygon:
			case ShapeType.Line:
			case ShapeType.Hybrid:
				DrawPolygon(shapeType, shape, graphics, brush, pen);
				break;
		}

		brush.Dispose();
		if (pen != null)
			pen.Dispose();
	}

	private void DrawSymbol(BaseSymbol symbol, Rectangle bounds, Point locationInShape, Color backgroundColor, Graphics graphics)
	{
		if (symbol == null)
			return;

		// Get the symbol image.
		Bitmap bitmap = symbol.Bitmap;

		// Draw the symbol centered within its portions of the item shape.
		Size size = new Size(bitmap.Width, bitmap.Height);
		Point location = new Point(bounds.X + ((bounds.Width + 1) / 2), bounds.Y + ((bounds.Height + 1) / 2));
		location.X -= ((size.Width + 1) / 2);
		location.Y -= ((size.Height + 1) / 2);
		
		if (locationInShape.X != -1)
		{
			location.X = locationInShape.X - size.Width / 2;
		}
		if (locationInShape.Y != -1)
		{
			location.Y = locationInShape.Y - size.Height / 2;
		}
		
		Rectangle symbolBounds = new Rectangle(location, size);
		graphics.DrawImage(bitmap, symbolBounds);
	}

	private void DrawText(string text, string textColor, Rectangle textRect, Graphics graphics)
	{
		// Set up to draw the text;
		SolidBrush textBrush = new SolidBrush(Utility.HexToColor(textColor));
		Font font = Utility.GetFontForFamilyList(FontStyleResource.FontFamily, FontStyleResource.FontSizePx, FontStyleResource.FontStyle);

		// Align the text vertically and horizontally.
		StringFormat drawFormat = new StringFormat();
		drawFormat.LineAlignment = TextAlignV == TextAlignType.LeftOrTop ? StringAlignment.Near : (TextAlignV == TextAlignType.RightOrBottom ? StringAlignment.Far : StringAlignment.Center);
		drawFormat.Alignment = TextAlignH == TextAlignType.LeftOrTop ? StringAlignment.Near : (TextAlignH == TextAlignType.RightOrBottom ? StringAlignment.Far : StringAlignment.Center);

		// Draw the inner rectangle onto the outer rectangle offset by the padding amount.
		graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
		graphics.DrawString(text, font, textBrush, textRect, drawFormat);
	}

	private void FlushMarkerImageBytes()
	{
		NormalActualImageBytes = new Byte[0];
		SelectedActualImageBytes = new Byte[0];
		NormalScaledImageBytes = new Byte[0];
		SelectedScaledImageBytes = new Byte[0];
		ResourceImageBytes = new Byte[0];
	}

	protected override Byte[] GenerateResourceImageBytes()
	{
		Bitmap normalBitmap = Utility.BitmapFromBytes(NormalActualImageBytes);
		Bitmap selectedBitmap = Utility.BitmapFromBytes(SelectedActualImageBytes);

        if (normalBitmap == null || selectedBitmap == null)
        {
            // This should never happen, but it has occurred during development.
            Debug.WriteLine("GenerateResourceImageBytes bit maps are null");
            return new Byte[0];
        }

        // Determine the dimensions of the bitmap needed to draw the marker's normal and selected
        // images side by side with spacing between them. Because the marker has fixed dimensions,
        // the normal and selected dimensions will be the same even if the the normal and selected
        // images are different sizes.
        int spacing = 16;
        int w = (normalBitmap.Width * 2) + spacing;
        int h = normalBitmap.Height;

        Bitmap bitmap = new Bitmap(w, h);

		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
            // Draw on a transparent background, though for some reason the transparency of the
            // normal and selected bitmaps does not come through (their backgrounds are white).
            Color transparentColor = Color.FromArgb(0, 0, 0, 0);
            graphics.Clear(transparentColor);

			graphics.DrawImage(normalBitmap, 0, 0);
			graphics.DrawImage(selectedBitmap, normalBitmap.Width + spacing, 0);
		}

        // Scale the resulting image to fit in a rectangle that is no taller than the max height.
        const int max = 72;
        Bitmap scaledBitmap = Utility.ScaledBitmap(bitmap, new Size(max + spacing + max, max), true);
		bitmap = scaledBitmap;

		return Utility.ImageToByteArray(bitmap, ImageFormat.Png);
	}

	private Byte[] GenerateMarkerStateImageBytes(MarkerState state)
	{
		// Debug.WriteLine(string.Format("GenerateMarkerStateImageBytes {0} {1}", currentState.ToString(), this.Name));

		BaseMarkerAppearance markerAppearanceNormal = null;
		BaseMarkerAppearance markerAppearanceSelected = null;

		if (markerDefinition == null)
			markerDefinition = CreateMarkerDefinition(1, null, null);

		if (state == MarkerState.Normal)
		{
			markerAppearanceNormal = markerDefinition.Base.BaseNormalAppearance;
			
			// Determine if this marker's normal shape is invisible. If so, show a thin cyan line in
			// the preview so the user sees something. If the marker Id is 0, this is not an actual
			// marker, but rather a temp marker used to create a preview image for a marker style.
			if (markerAppearanceNormal.BaseSymbol == null && Id > 0)
			{
				BaseShape shape = markerAppearanceNormal.BaseShape;
				if (shape != null)
				{
					bool lineIsInvisible = shape.LineWidth == 0 || (shape.LineColor == Color.White || shape.LineColorOpacity == 0);
					bool fillIsInvisible = shape.FillColor == Color.White || shape.FillColorOpacity == 0;
					if (lineIsInvisible && fillIsInvisible)
					{
						// Show the invisible shape's outline in cyan  in the preview.
						markerAppearanceNormal.BaseShape.LineWidth = 1;
						markerAppearanceNormal.BaseShape.LineColor = Color.Cyan;
						markerAppearanceNormal.BaseShape.LineColorOpacity = 25;
					}
				}
			}
		}

		if (state == MarkerState.Selected)
			markerAppearanceSelected = markerDefinition.Base.BaseSelectedAppearance;

		// Get the rectangle the fully encloses the marker's normal and selected shape and symbols.
		Rectangle bounds = markerDefinition.Bounds;

		// Check for an "empty" rectangle. We check both width and height rather than bounds.Empty
		// because one could be zero and not the other. That can happen when the marker shape
		// is imported via Area tags where the bounding rectangle has a zero height or width.
		if (bounds.Width == 0 || bounds.Height == 0)
		{
			return new Byte[0];
		}

		if (ShapeType != ShapeType.None)
		{
			bounds.Width += MarkerStyle.LineWidth;
			bounds.Height += MarkerStyle.LineWidth;
		}

		if (MarkerType == MarkerType.Photo || (MarkerType == MarkerType.Text && ShapeType == ShapeType.Rectangle))
		{
			// Add an extra pixel for borders with an odd thickness.
			bounds.Width += MarkerStyle.LineWidth % 2;
			bounds.Height += MarkerStyle.LineWidth % 2;
		}

		Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);

		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			// Use a white background. Note that if it were not for IE 6's inability
			// to handle transparent png images, we would draw on a transparent background.
			Color backgroundColor = Color.White;
			graphics.Clear(backgroundColor);

			BaseMarkerAppearance appearance = state == MarkerState.Normal ? markerAppearanceNormal : markerAppearanceSelected;
			DrawMarkerAppearance(appearance, bounds, backgroundColor, graphics);
		}
		
		return Utility.ImageToByteArray(bitmap, ImageFormat.Png);
	}

	public static DataTable GetFilteredMarkerList(MarkerFilter filter, Tour tour, TourPage tourPage, TourView tourView, int accountId)
	{
		DataTable dataTable = null;

		int tourViewId = tourView == null ? 0 : tourView.Id;

		switch (filter)
		{
			case MarkerFilter.Account:
				// This SP returns the markers owned by the account AND the exclusive marker for the tour
				// view if it has one. As such, we can't use sp_Marker_GetMarkersOwnedByAccount here.
				dataTable = MapsAliveDatabase.LoadDataTable("sp_Marker_GetMarkersOwnedByAccountOrTourView",
					"@AccountId", accountId, "@TourViewId", tourViewId);
				break;

			case MarkerFilter.Tour:
				dataTable = MapsAliveDatabase.LoadDataTable("sp_Marker_GetMarkersUsedByTour",
					"@AccountId", accountId, "@TourId", tour.Id, "@TourViewId", tourViewId);
				break;

			case MarkerFilter.Exclusive:
				dataTable = MapsAliveDatabase.LoadDataTable("sp_Marker_GetMarkersUsedByTourOrTourView",
					"@AccountId", accountId, "@TourId", tour.Id);
				break;

			case MarkerFilter.Gallery:
				dataTable = MapsAliveDatabase.LoadDataTable("sp_Marker_GetMarkersForTypeOwnedByAccount",
					"@AccountId", accountId, "@MarkerType", (int)MarkerType.Photo);
				break;

			default:
				break;
		}
		
		return dataTable;
	}

	public override void InsertIntoDatabase(int accountId)
	{
		resourceId = (int)MapsAliveDatabase.ReadScalar("sp_Marker_CreateMarker", "@AccountId", accountId);
		UpdateDatabase();
	}

	public void MakeExclusive(TourView tourView)
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Marker_SetExclusiveToTourView",
			"@MarkerId", resourceId,
			"@TourViewId", tourView.Id
		);
	}

	public void MakeNonExclusive()
	{
		tourViewId = 0;
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Marker_SetNotExclusiveToTourView",
			"@MarkerId", resourceId
		);
	}
	
	private Size MeasureText(string text, Size area)
	{
		// Add a little length to the text to account for the fact that MeasureString sometimes
		// returns a too-small measurement. This adjustments won't work all the time, and it can
		// instroduce an unnecessary line break, but it seems to help in general.
		text = "*" + text + "*";

		Bitmap bitmap = new Bitmap(1, 1);
		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
			Font font = Utility.GetFontForFamilyList(FontStyleResource.FontFamily, FontStyleResource.FontSizePx, FontStyleResource.FontStyle);
			SizeF sizeF = graphics.MeasureString(text, font, area, StringFormat.GenericTypographic);
			return new Size((int)Math.Ceiling(sizeF.Width), (int)Math.Ceiling(sizeF.Height));
		}
	}

	private static void SetClickAction(TourView tourView, BaseMarker baseMarker)
	{
		// Set the click rules for this marker instance.
		MarkerAction clickAction = tourView.MarkerClickAction;
		string clickActionTarget = tourView.MarkerClickActionTarget.Trim();
		if (clickAction != MarkerAction.None && clickActionTarget != string.Empty)
		{
			int themeId = tourView.Tour.ThemeId;

			switch (clickAction)
			{
				case MarkerAction.GotoPage:
					baseMarker.MarkerInstance.BaseActionMarkerRuleSet.BaseClickRule.SetGotoPageOnEvent();
					int targetPageId;
					int.TryParse(clickActionTarget, out targetPageId);
					TourPage targetPage = targetPageId == 0 ? null : tourView.Tour.GetTourPage(targetPageId);
					if (targetPage != null)
					{
						baseMarker.MarkerInstance.TargetPageId = targetPage.Id;
						baseMarker.MarkerInstance.TargetPageName = "Page" + targetPage.PageNumber;
					}
					break;

				case MarkerAction.LinkToUrl:
				case MarkerAction.LinkToUrlNewWindow:
					baseMarker.MarkerInstance.BaseActionMarkerRuleSet.BaseClickRule.SetLinkToUrlOnEvent();
					break;

				case MarkerAction.CallJavascript:
					baseMarker.MarkerInstance.BaseJavascriptMarkerRuleSet.BaseClickRule.CallJavascriptOnEvent = true;
					baseMarker.MarkerInstance.BaseJavascriptMarkerRuleSet.BaseClickRule.SetJavascriptText(themeId, clickActionTarget);
					break;
			}
		}

		if (tourView.ShowContentEvent == ShowContentEvent.OnClick)
		{
			baseMarker.MarkerInstance.TargetViewId = tourView.Id;
			baseMarker.MarkerInstance.BaseActionMarkerRuleSet.BaseClickRule.SetShowViewOnEvent();
		}
	}

	private static void SetRolloverAction(TourView tourView, BaseMarker baseMarker)
	{
		// Set the rollover rules for this marker instance.
		MarkerAction rolloverAction = tourView.MarkerRolloverAction;
		string rolloverActionTarget = tourView.MarkerRolloverActionTarget.Trim();
		if (rolloverAction == MarkerAction.CallJavascript && rolloverActionTarget != string.Empty)
		{
			int themeId = tourView.Tour.ThemeId;
			baseMarker.MarkerInstance.BaseJavascriptMarkerRuleSet.BaseMouseEnterRule.CallJavascriptOnEvent = true;
			baseMarker.MarkerInstance.BaseJavascriptMarkerRuleSet.BaseMouseEnterRule.SetJavascriptText(themeId, rolloverActionTarget);
		}

		// Detect the case where there is no hotspot content and the user wants to display a tooltip instead.
		TourPage tourPage = tourView.TourPage;
		bool dontShowContent = tourPage.SlidesPopup && tourView.HasNoContent && tourPage.PopupOptions.ShowTooltipWhenNoContent && tourPage.PopupOptions.Location != PopupLocation.FixedAlwaysVisible;
		baseMarker.MarkerInstance.TargetViewId = tourView.Id;

		if (tourView.ShowContentEvent == ShowContentEvent.OnMouseover && !dontShowContent)
		{
			baseMarker.MarkerInstance.BaseActionMarkerRuleSet.BaseMouseEnterRule.SetShowViewOnEvent();
		}
		else if (tourView.ShowContentEvent == ShowContentEvent.Never || dontShowContent)
		{
			baseMarker.MarkerInstance.DoesNotShowContent = true;
			baseMarker.MarkerInstance.BaseActionMarkerRuleSet.BaseMouseEnterRule.SetNoDisplayActionOnEvent();
		}
	}

	private static void SetRolloutAction(TourView tourView, BaseMarker baseMarker)
	{
		// Set the rollout rules for this marker instance.
		MarkerAction rolloutAction = tourView.MarkerRolloutAction;
		string rolloutActionTarget = tourView.MarkerRolloutActionTarget.Trim();
		if (rolloutAction == MarkerAction.CallJavascript && rolloutActionTarget != string.Empty)
		{
			int themeId = tourView.Tour.ThemeId;
			baseMarker.MarkerInstance.BaseJavascriptMarkerRuleSet.BaseMouseExitRule.CallJavascriptOnEvent = true;
			baseMarker.MarkerInstance.BaseJavascriptMarkerRuleSet.BaseMouseExitRule.SetJavascriptText(themeId, rolloutActionTarget);
		}
	}

	public int ShapeCoordsCircleRadius
	{
		// See comments for ShapeCoordsRectangleSize.
		get
		{
			string[] coordinates = ShapeCoords.Split(',');
			int radius = int.Parse(coordinates[2]);
			return radius;
		}
		set
		{
			int radius = value;
			ShapeCoords = string.Format("0,0,{0}", radius);
		}
	}

	public Size ShapeCoordsRectangleSize
	{
		// The ShapeCoords string is used to maintain a shape's actual size regardless of whether
		// it's a circle, rectangle, or polygon. Whenever we need to pass shape size information
		// between the web app and the MapsAlive Engine, we use shape coord. This property converts
		// Size to shape coords and vice versa. Circles are handled by ShapeCoordsCircleRadius.
		//
		// Note that rectangle coords are x1,y1,x2,y2. They are NOT x,y,w,h like a rectangle.
		
		get
		{
			string[] coordinates = ShapeCoords.Split(',');
			int x1 = int.Parse(coordinates[0]);
			int y1 = int.Parse(coordinates[1]);
			int x2 = int.Parse(coordinates[2]);
			int y2 = int.Parse(coordinates[3]);
			return new Size(x2 - x1 + 1, y2 - y1 + 1);
		}
		set
		{
			int x1 = 0;
			int y1 = 0;
			int x2 = value.Width - 1;
			int y2 = value.Height - 1;

			ShapeCoords = string.Format("{0},{1},{2},{3}", x1, y1, x2, y2);
		}
	}

	public Size SymbolSize()
	{
		Size normalSize = Size.Empty;
		Size selectedSize = Size.Empty;

		if (NormalSymbolId > 0)
		{
			Symbol normalSymbol = Account.GetCachedSymbol(NormalSymbolId);
			normalSize = normalSymbol.Size;
		}

		if (SelectedSymbolId > 0)
		{
			Symbol selectedSymbol = Account.GetCachedSymbol(SelectedSymbolId);
			selectedSize = selectedSymbol.Size;
		}

		Size size = new Size(Math.Max(normalSize.Width, selectedSize.Width), Math.Max(normalSize.Height, selectedSize.Height));

		if (size == Size.Empty)
			size = new Size(4, 4);

		return size;
	}

	public static bool ToolTipAllowed(TourView tourView)
	{
		TourPage tourPage = tourView.TourPage;

		bool toolTipAllowed = false;
		string tooltipText = tourView.ToolTip;

		if (tourPage.SlidesPopup)
		{
			if (tourPage.PopupOptions.Location == PopupLocation.FixedAlwaysVisible)
			{
				toolTipAllowed = true;
			}
			else if ((tourView.ShowContentEvent == ShowContentEvent.Never || tourView.ShowContentEvent == ShowContentEvent.OnClick) && tooltipText.Length != 0)
			{
				toolTipAllowed = true;
			}
			else if (tourView.HasNoContent && tourPage.PopupOptions.ShowTooltipWhenNoContent)
			{
				toolTipAllowed = true;
			}
		}
		else
		{
			// Tooltips are always allowed for tiled layouts, but only if there is tooltip text.
			toolTipAllowed = tooltipText.Length > 0;
		}

		return toolTipAllowed;
	}

	public override void UpdateDatabase()
	{
		Utility.Trace("Marker.UpdateDatabase " + this.Name);
		
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Marker_UpdateMarker",
			"@MarkerId", resourceId,
			"@Name", Name,
			"@MarkerStyleId", markerStyleId,
			"@ShapeType", (int)ShapeType,
			"@ShapeCoords", ShapeCoords,
			"@AnchorLocationX", anchorLocationX,
			"@AnchorLocationY", anchorLocationY,
			"@SymbolLocationX", symbolLocationX,
			"@SymbolLocationY", symbolLocationY,
			"@NormalSymbolId", NormalSymbolId,
			"@SelectedSymbolId", SelectedSymbolId,
			"@MarkerType", (int)MarkerType,
			"@FontStyleId", fontStyleId,
			"@RectangleWidth", RectangleSize.Width,
			"@RectangleHeight", RectangleSize.Height,
			"@CircleRadius", CircleRadius,
			"@TextAutoSize", TextAutoSize,
			"@TextPadding", TextPadding,
			"@TextString", TextString,
			"@TextAlignH", TextAlignH,
			"@TextAlignV", TextAlignV,
			"@NormalTextColor", NormalTextColor,
			"@SelectedTextColor", SelectedTextColor,
			"@Padding", PhotoPadding,
			"@PhotoConstraintId", (int)PhotoConstraint,
			"@PhotoScale", PhotoScale,
			"@PhotoWidth", PhotoWidth,
			"@PhotoHeight", PhotoHeight,
			"@PhotoNormalEffect", (int)PhotoNormalEffect,
			"@PhotoSelectedEffect", (int)PhotoSelectedEffect,
			"@PhotoNormalOpacity", PhotoNormalOpacity,
			"@PhotoSelectedOpacity", PhotoSelectedOpacity,
			"@PhotoCropId", PhotoCrop,
			"@PhotoCropFactor", PhotoCropFactor,
			"@PhotoCaptionPositionId", (int)PhotoCaptionPosition,
			"@ScaleShapeToMap", ScaleShapeToMap
		);
	}

	// Used only if needed for debugging.
	private void WriteSymbolImageToFile(MarkerState state, Tour tour, int symbolId, Bitmap bitmap)
	{
		if (tour == null)
			return;

		try
		{
			string previewFolderLocationAbsolute = FileManager.PreviewFolderLocationAbsolute(tour.Id);

			string id = symbolId.ToString() + (state == MarkerState.Normal ? "N" : "S");
			string fileLocation = previewFolderLocationAbsolute + "\\S" + id + tour.BuildId + ".png";
			bitmap.Save(fileLocation, ImageFormat.Png);
		}
		catch (Exception ex)
		{
			Debug.Fail("WriteSymbolImageToFile: " + ex.Message);
		}
	}

	// Used only if needed for debugging.
	private void WriteHotspotImageToFile(MarkerState state, TourView tourView, Bitmap bitmap)
	{
		if (tourView == null)
			return;

		try
		{
			int tourId = tourView.Tour.Id;
			string previewFolderLocationAbsolute = FileManager.PreviewFolderLocationAbsolute(tourId);

			string id = (tourView == null ? "" : tourView.Id.ToString()) + (state == MarkerState.Normal ? "N" : "S");
			string fileLocation = previewFolderLocationAbsolute + "\\H" + id + tourView.Tour.BuildId + ".png";
			bitmap.Save(fileLocation, ImageFormat.Png);
		}
		catch (Exception ex)
		{
			Debug.Fail("WriteSymbolImageToFile: " + ex.Message);
		}
	}
}
