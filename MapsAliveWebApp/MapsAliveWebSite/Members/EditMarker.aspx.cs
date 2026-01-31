// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using AvantLogic.MapsAlive.Engine;
using Telerik.Web.UI;

public partial class Members_EditMarker : MemberPage
{
	private bool photoCaptionTurnedOn;
	private bool getValuesFromForm;
	private bool getValuesFromMarker;
	private int markerId;
	private MarkerType markerType;
	private bool markerTypeChanged;
	private ShapeType shapeType;
	private bool shapeTypeChanged;
	private bool shapeTypeChangedBetweenPolygonLineOrHybrid;
	private int validAnchorLocationX;
	private int validAnchorLocationY;
	private int validCircleRadius;
	private int validPhotoHeight;
	private int validPhotoPadding;
	private int validPhotoWidth;
	private string validMarkerName;
	private string validNormalTextColor;
	private int validRectangleWidth;
	private int validRectangleHeight;
	private int validSymbolLocationX;
	private int validSymbolLocationY;
	private string validSelectedTextColor;
	private int validTextPadding;
	private string validTextString;

	protected void AddChangeDetection(CheckBox checkBox, string script)
	{
		checkBox.Attributes.Add("onclick", script);
	}

	private void AddShapeComboBoxItem(string text, ShapeType value, ShapeType selectedShapeType)
	{
		RadComboBoxItem item = new RadComboBoxItem();
		item.Text = text;
		item.Value = ((int)value).ToString();
		item.Selected = value == selectedShapeType;
		MarkerShapeComboBox.Items.Add(item);
	}

	private void CreateMarkerFromQueryStringId()
	{
		if (IsPostBack || IsReturnToTourBuilder)
		{
			markerId = account.LastResourceId(TourResourceType.Marker);
			if (markerId > 0)
			{
				marker = Account.GetCachedMarker(markerId);
				return;
			}
		}

		string id = Request.QueryString["id"];
		int.TryParse(id, out markerId);
		if (markerId != 0)
			marker = Account.GetCachedMarker(markerId);

		if (marker != null)
			markerId = marker.Id;

		if (marker == null || markerId == 0 || marker.AccountId != Utility.AccountId)
		{
			// There was no Id on the query string or it was not a valid Id.
			Server.Transfer(MemberPageAction.ActionPageTarget(MemberPageActionId.MarkerExplorer));
		}
		else
		{
			account.SetLastResourceId(TourResourceType.Marker, markerId);
		}
	}

	protected override void EmitJavaScript()
	{
		string loadingScript =
			AssignClientVar("selectedMarkerId", markerId) +
			AssignClientVar("isTextMarker", marker.MarkerType == MarkerType.Text);
		string loadedScript = string.Empty;
		EmitJavaScript(loadingScript, loadedScript);
	}

	protected override void InitControls(bool undo)
	{
		if (!pageValid)
		{
			// Don't init the controls if a validation error occurred. For most screens we don't have to do this, but
			// the autopost controls on this screen mean that the page can fail validation and still get to this code.
			return;
		}

		if (undo || getValuesFromMarker)
			InitControls();
		
		UpdateControls();
	}

	private void InitControls()
	{
		// This code is only executed the first time the page is loaded
		// or whenever the user changes the marker type. Changing the
		// marker type is equivalent to loading a new page for that type.

		MarkerNameTextBox.Text = marker.Name;
		AddChangeDetection(MarkerNameTextBox);

		MemberPage.InitShowUsageControl(ShowUsageControl, marker);

		MarkerTypeComboBox.SelectedValue = ((int)marker.MarkerType).ToString();
		if (marker.IsExclusive)
			MarkerTypeComboBox.Enabled = false;

		switch (markerType)
		{
			case MarkerType.Shape:
				InitShapeControls();
				break;

			case MarkerType.Symbol:
				InitSymbolControls();
				break;

			case MarkerType.SymbolAndShape:
				InitShapeControls();
				SymbolLocationComboBox.SelectedValue = marker.SymbolLocation.X == -1 && marker.SymbolLocation.Y == -1 ? "0" : "1";
				int x = marker.SymbolLocation.X;
				int y = marker.SymbolLocation.Y;
				SymbolXTextBox.Text = x == -1 ? "" : x.ToString();
				SymbolYTextBox.Text = y == -1 ? "" : y.ToString();
				AddChangeDetectionForPreview(SymbolXTextBox);
				AddChangeDetectionForPreview(SymbolYTextBox);
				break;

			case MarkerType.Text:
				InitShapeControls();
				TextAutoSizeCheckBox.Checked = marker.TextAutoSize;
				AddChangeDetectionForPreview(TextAutoSizeCheckBox);
				FontStyleComboBox.SelectedResourceId = marker.FontStyleResource.Id;
				AddChangeDetectionForPreview(TextMarkerStringTextBox);
				TextMarkerStringTextBox.Text = marker.TextString;
				AddChangeDetectionForPreview(TextPaddingTextBox);
				TextPaddingTextBox.Text = marker.TextPadding.ToString();
				break;

			case MarkerType.Photo:
				InitPhotoControls();
				if (marker.PhotoCaptionPosition != PhotoCaptionPositionType.None)
				{
					FontStyleComboBox.SelectedResourceId = marker.FontStyleResource.Id;
					AddChangeDetectionForPreview(TextMarkerStringTextBox);
					TextMarkerStringTextBox.Text = marker.TextString;
					AddChangeDetectionForPreview(TextPaddingTextBox);
					TextPaddingTextBox.Text = marker.TextPadding.ToString();
				}
				break;
		}

        // The Scale Shape to Map option is hidden because it does not seem to be implemented.
        // It doesn't do anything in MapsAlive V3, and although the boolean value gets saved
        // to the database, it does not get passed to the JavaScript runtime. The user documentation
        // doesn't describe it, and the V3 Flash code doesn't appear to use it. The purpose of the
        // option is to allow a marker shape to scale the same amount as a non-zoomable map image.
        // That way you could create a shape for a full size map image and use it with any scaled size
        // of the map. To enable this in the future, the database value for this flag needs to get
        // passed to the runtime via XML/XSLT e.g. as markerScales (like the markerZooms flag).
        // Logic needs to be added to the MapsAliveMap object to check the flag and do the scaling.
        ScaleShapePanel.Visible = false;
	}

	private void InitPhotoControls()
	{
		AddChangeDetectionForPreview(PhotoWidthTextBox);
		AddChangeDetectionForPreview(PhotoHeightTextBox);
		AddChangeDetectionForPreview(PhotoPaddingTextBox);

		PhotoWidthTextBox.Text = marker.PhotoWidth.ToString();
		PhotoHeightTextBox.Text = marker.PhotoHeight.ToString();
		PhotoPaddingTextBox.Text = marker.PhotoPadding.ToString();
	}

	private void InitShapeControls()
	{
		MarkerStyleComboBox.SelectedResourceId = marker.MarkerStyle.Id;

		AddChangeDetectionForPreview(RectangleWidthTextBox);
		AddChangeDetectionForPreview(RectangleHeightTextBox);
		AddChangeDetectionForPreview(CircleRadiusTextBox);

		switch (shapeType)
		{
			case ShapeType.Circle:
				int radius = marker.CircleRadius;
				if (radius == 0)
				{
					// The user has never provided a radius so use the actual radius.
					radius = marker.ShapeCoordsCircleRadius;
				}
				CircleRadiusTextBox.Text = radius.ToString();
				break;

			case ShapeType.Polygon:
			case ShapeType.Line:
			case ShapeType.Hybrid:
				break;

			case ShapeType.Rectangle:
				Size size = new Size(marker.RectangleSize.Width, marker.RectangleSize.Height);
				if (size == Size.Empty)
				{
					// The user has never provided a size so use the actual size.
					size = marker.ShapeCoordsRectangleSize;
				}
				RectangleWidthTextBox.Text = size.Width.ToString();
				RectangleHeightTextBox.Text = size.Height.ToString();
				break;
		}

		AddChangeDetection(ScaleShapeCheckBox);
		ScaleShapeCheckBox.Checked = marker.ScaleShapeToMap;
	}

	private void InitShapeList(MarkerType markerType, ShapeType selectedShapeType)
	{
		// Add circle and rectangle since they are valid for all marker types except photo or hybrid).
        if (selectedShapeType != ShapeType.Hybrid)
        {
    		AddShapeComboBoxItem("Circle", ShapeType.Circle, selectedShapeType);
	    	AddShapeComboBoxItem("Rectangle", ShapeType.Rectangle, selectedShapeType);
        }

		// Add the other shapes if allowed.
		if (markerType != MarkerType.Text)
		{
			// Show the Hybrid option for hybrids and the Polygon and Line options for those
			// shapes, but don't allow the user to change a hybrid to a line or polygon and
			// don't allow them to change a line or polygon to a hybrid. Doing either would
			// allow the wrong kind of coordinates to get used.
			if (selectedShapeType == ShapeType.Hybrid)
			{
				AddShapeComboBoxItem("Hybrid", ShapeType.Hybrid, selectedShapeType);
			}
			else
			{
				AddShapeComboBoxItem("Polygon", ShapeType.Polygon, selectedShapeType);
				AddShapeComboBoxItem("Line", ShapeType.Line, selectedShapeType);
			}
		}

		MarkerShape.Topic = markerType == MarkerType.Text ? "MarkerShapeText" : "MarkerShape";
	}

	private void InitSymbolControls()
	{
		NormalSymbolComboBox.SelectedResourceId = marker.NormalSymbolId;
		SelectedSymbolComboBox.SelectedResourceId = marker.SelectedSymbolId;

		if (marker.NormalSymbolId > 0)
		{
			Symbol symbol = Account.GetCachedSymbol(marker.NormalSymbolId);
			AnchorLocationComboBox.SelectedValue = marker.AnchorLocation.X == -1 && marker.AnchorLocation.Y == -1 ? "0" : "1";
			int anchorX = marker.AnchorLocation.X;
			int anchorY = marker.AnchorLocation.Y;
			AnchorXTextBox.Text = anchorX == -1 ? ((int)(symbol.Size.Width / 2)).ToString() : anchorX.ToString();
			AnchorYTextBox.Text = anchorY == -1 ? ((int)(symbol.Size.Height / 2)).ToString() : anchorY.ToString();
			AddChangeDetectionForPreview(AnchorXTextBox);
			AddChangeDetectionForPreview(AnchorYTextBox);
		}
	}

	protected void OnOptionChanged(object sender, EventArgs e)
	{
		InitPageControls();
	}

	protected override void PageLoad()
	{
		Utility.RegisterColorChooserJavaScript(this);

		SetMasterPage(Master);
		SetPageTitle(TourResource.GetTitleForEditPage(TourResourceType.Marker));
		SetActionId(MemberPageActionId.EditMarker);
		GetSelectedTourOrNone();
		CreateMarkerFromQueryStringId();

		// Save a copy of the unedited resource.
		resourceBeforeEdit = marker.Clone();

		if (!IsPostBack)
		{
			// See comments in Account for these properties.
			account.TempMarkerPolygonCoords = null;
			account.TempMarkerHybridCoords = null;
			account.LastShapeTypeSelected = ShapeType.None;
			account.LastTextShapeTypeSelected = ShapeType.None;
		}

		// IMPORTANT:
		// Set the handler that will be called when these controls are used. Note that TourResourceComboBox
		// controls share the same script. Even though we are setting the script for all, code is only
		// emitted for the first. The logic to determine what to do for which combo box is located in
		// maResourceChanged. Currently this is the only screen that suffers from this. We can improve
		// it later if more screens use multiple TourResourceComboBox and need different scripts.
		MarkerStyleComboBox.ResourceType = TourResourceType.MarkerStyle;
		MarkerStyleComboBox.OnClientSelectedIndexChangedScript = "maResourceChanged(sender);";
		FontStyleComboBox.ResourceType = TourResourceType.FontStyle;
		FontStyleComboBox.OnClientSelectedIndexChangedScript = "maResourceChanged(sender);";
		NormalSymbolComboBox.ResourceType = TourResourceType.Symbol;
		NormalSymbolComboBox.OnClientSelectedIndexChangedScript = "maResourceChanged(sender);";
		SelectedSymbolComboBox.ResourceType = TourResourceType.Symbol;
		SelectedSymbolComboBox.OnClientSelectedIndexChangedScript = "maResourceChanged(sender);";

		// Get the type of marker we are now working with.
		markerType = IsPostBack ? (MarkerType)int.Parse(MarkerTypeComboBox.SelectedValue) : marker.MarkerType;

		// Determine if the marker type changed.
		markerTypeChanged = markerType != marker.MarkerType;
		
		// If this is a post back and the user did not change the marker type, then
		// the form is being submitted to change other values. If the user changed the
		// marker type or this is the initial load, then we get all values from the
		// marker itself. The only exception is the marker name. You can change the 
		// marker name and the marker type, but when you change the type, any other
		// values on the form will be ignored.
		getValuesFromForm = IsPostBack && !markerTypeChanged;
		
		// Use a variable with the opposite value for purposed of clarity. If the values
		// are not coming from the form, then they come from the marker object.
		getValuesFromMarker = !getValuesFromForm;

		// Determine what the shape type should be.
		if (MarkerTypeHasShape(markerType))
			SetShapeType();

		// Set this visibility of the various panels based on the marker type. Note that
		// we must do this every time which is why this call is here instead of in InitControls().
		// InitControls() is called when the page first loads and when the user presses the
		// Undo button. It is also called from the OnOptionChanged handler; however, the work
		// done by ShowControlsForMarkerType needs to happen first because the visibility of the
		// heavy-weight combobox controls determines whether their view state is enabled and whether
		// their content gets loaded from the database. Because this page has so much going on, we
		// have to be very careful to keep the work and size of the view state on each post from
		// getting out of hand.
		ShowControlsForMarkerType();
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Resources;
	}

    protected override void PerformUpdate()
    {
        marker.UpdateResource(resourceBeforeEdit);
        if (tour == null)
            return;
        foreach (TourPage tourPage in tour.TourPages)
            tourPage.MapMarkerChanged();
    }

    protected override void ReadPageFields()
	{
		marker.Name = validMarkerName;

		if (markerTypeChanged)
			ChangeMarkerType();

		if (getValuesFromMarker)
			return;

		switch (markerType)
		{
			case MarkerType.Symbol:
				marker.ShapeType = 0;
				ReadSymbolFields();
				break;
			
			case MarkerType.Shape:
				ReadMarkerStyleFields();
				ReadShapeFields();
				break;
			
			case MarkerType.SymbolAndShape:
				ReadMarkerStyleFields();
				ReadSymbolFields();
				ReadShapeFields();
				marker.SymbolLocation = new Point(validSymbolLocationX, validSymbolLocationY);
				break;
			
			case MarkerType.Text:
				ReadMarkerStyleFields();
				ReadShapeFields();
				ReadTextFields();
				break;
			
			case MarkerType.Photo:
				ReadMarkerStyleFields();
				ReadShapeFields();
				ReadPhotoFields();
				if (marker.PhotoCaptionPosition != PhotoCaptionPositionType.None)
					ReadTextFields();
				break;
		}

		marker.AnchorLocation = new Point(validAnchorLocationX, validAnchorLocationY);
	}

	private void ReadPhotoFields()
	{
		marker.PhotoPadding = validPhotoPadding;
		marker.PhotoConstraint = (PhotoConstraintType)int.Parse(PhotoConstraintComboBox.SelectedValue);
		marker.PhotoCrop = (PhotoCropType)int.Parse(PhotoCropOptionsComboBox.SelectedValue);
		marker.PhotoCropFactor = int.Parse(PhotoCropFactorComboBox.SelectedValue);
		marker.PhotoScale = (int)SliderPhotoScale.Value;
		marker.PhotoNormalOpacity = (int)SliderNormalOpacity.Value;
		marker.PhotoSelectedOpacity = (int)SliderSelectedOpacity.Value;
		marker.PhotoNormalEffect = (PhotoEffect)int.Parse(PhotoEffectNormalComboBox.SelectedValue);
		marker.PhotoSelectedEffect = (PhotoEffect)int.Parse(PhotoEffectSelectedComboBox.SelectedValue);
		
		// Turning the photo caption on means that we have to show the text controls.
		PhotoCaptionPositionType newCaptionPosition = (PhotoCaptionPositionType)int.Parse(PhotoCaptionPositionComboBox.SelectedValue);
		photoCaptionTurnedOn = marker.PhotoCaptionPosition == PhotoCaptionPositionType.None && newCaptionPosition != PhotoCaptionPositionType.None;
		marker.PhotoCaptionPosition = newCaptionPosition;
	}

	private bool MarkerTypeHasFontStyle(MarkerType type)
	{
		return type == MarkerType.Text || type == MarkerType.Photo;
	}

	private bool MarkerTypeHasMarkerStyle(MarkerType type)
	{
		return type != MarkerType.Symbol;
	}

	private bool MarkerTypeHasSymbol(MarkerType type)
	{
		return type == MarkerType.Symbol || type == MarkerType.SymbolAndShape;
	}

	private bool MarkerTypeHasShape(MarkerType type)
	{
		return
			type == MarkerType.SymbolAndShape ||
			type == MarkerType.Shape ||
			type == MarkerType.Text ||
			type == MarkerType.Photo;
	}

	private void ChangeMarkerType()
	{
		MarkerType oldType = marker.MarkerType;
		MarkerType newType = markerType;

		if (MarkerTypeHasSymbol(oldType) && !MarkerTypeHasSymbol(newType))
		{
			// The marker changed from a type that had a symbol to one that does not. Remove the marker's symbols.
			marker.NormalSymbolId = 0;
			marker.SelectedSymbolId = 0;
		}
		else if (MarkerTypeHasSymbol(newType) && !MarkerTypeHasSymbol(oldType))
		{
			// The marker changed from a type that had no symbol to one that does. Set default symbols.
			marker.NormalSymbolId = account.DefaultResourceId(TourResourceType.Symbol);
			marker.SelectedSymbolId = account.DefaultResourceId(TourResourceType.Symbol);
		}

		if (MarkerTypeHasSymbol(newType) && MarkerTypeHasShape(newType))
		{
			SymbolLocationComboBox.SelectedValue = "0";
			marker.SymbolLocation = new Point(-1, -1);
		}

		if (MarkerTypeHasMarkerStyle(oldType) && !MarkerTypeHasMarkerStyle(newType))
		{
			// The marker changed from a type that had a marker style to one that does not. Remove the marker's marker style.
			marker.MarkerStyle = null;
		}
		else if (MarkerTypeHasMarkerStyle(newType) && !MarkerTypeHasMarkerStyle(oldType))
		{
			// The marker changed from a type that had no marker style to one that does. Set default marker style.
			marker.MarkerStyle = new MarkerStyle(account.DefaultResourceId(TourResourceType.MarkerStyle));
		}

		// The marker changed from a circle to a rectangle or vice versa. Adjust the coordinates.
		if (MarkerTypeHasShape(oldType) && !MarkerTypeHasShape(newType))
		{
			// The marker changed from a type that had a shape to one that does not. Remove the marker's shape.
			marker.ShapeType = ShapeType.None;
		}
		else if (MarkerTypeHasShape(newType) && !MarkerTypeHasShape(oldType))
		{
			// The marker changed from a type that does not have a shape to one that does.
			if (shapeType == ShapeType.None)
				shapeType = ShapeType.Circle;
			ReadShapeFields();
		}

		if (MarkerTypeHasFontStyle(oldType) && !MarkerTypeHasFontStyle(newType))
		{
			// The marker changed from a type that had a font style to one that does not. Remove the marker's font style.
			marker.FontStyleResource = null;
		}
		else if (MarkerTypeHasFontStyle(newType) && (!MarkerTypeHasFontStyle(oldType) || marker.FontStyleResourceId == 0))
		{
			// The marker changed from a type that had no font style to one that does. Set default font style.
			// Note that this can happen when changing to a Text marker from a Photo Marker that had no caption.
			marker.FontStyleResource = new FontStyleResource(account.DefaultResourceId(TourResourceType.FontStyle));
		}

		if (newType == MarkerType.Text || newType == MarkerType.Photo)
		{
			marker.TextString = string.Empty;
			SetDefaultTextColors();
			marker.SymbolLocation = new Point(-1, -1);

			if (newType == MarkerType.Text)
			{
				marker.TextPadding = 4;
				marker.ShapeType = shapeType;
				marker.TextAutoSize = true;
				marker.TextAlignH = TextAlignType.Center;
				marker.TextAlignV = TextAlignType.Center;
			}
			else if (newType == MarkerType.Photo)
			{
				marker.PhotoNormalOpacity = 100;
				marker.PhotoSelectedOpacity = 100;
				marker.PhotoConstraint = PhotoConstraintType.WidthAndHeight;
				marker.PhotoScale = 20;
				marker.PhotoWidth = 80;
				marker.PhotoHeight = 80;
				marker.PhotoCrop = PhotoCropType.ScaleAndTrim;
				marker.PhotoCropFactor = 0;
				marker.PhotoCaptionPosition = PhotoCaptionPositionType.None;
				marker.PhotoPadding = 4;
			}
		}
		
		marker.MarkerType = markerType;
	}

	private void ReadTextFields()
	{
		if (shapeTypeChanged || markerTypeChanged)
			return;

		marker.TextAlignV = (TextAlignType)int.Parse(TextAlignmentVComboBox.SelectedValue);
		marker.TextAlignH = (TextAlignType)int.Parse(TextAlignmentHComboBox.SelectedValue);
		marker.TextPadding = validTextPadding;

		marker.TextAutoSize = TextAutoSizeCheckBox.Checked;
		marker.TextString = TextMarkerStringTextBox.Text;

		if (photoCaptionTurnedOn)
		{
			marker.FontStyleResource = Account.GetCachedFontStyle(account.DefaultFontStyleId);
			SetDefaultTextColors();
			AddChangeDetection(TextMarkerStringTextBox);
		}
		else
		{
			// Read the text appearance, but only if text fields were already showing. If the user
			// just turned the caption on, keep the text values that the marker already has.
			marker.FontStyleResource = Account.GetCachedFontStyle(FontStyleComboBox.SelectedResourceId);
			marker.NormalTextColor = validNormalTextColor;
			marker.SelectedTextColor = validSelectedTextColor;
		}

		if (shapeType == ShapeType.Circle)
		{
			marker.CircleRadius = validCircleRadius;
		}
		else
		{
			Size validRectangleSize = new Size(validRectangleWidth, validRectangleHeight);
			marker.RectangleSize = validRectangleSize;
		}
	}

	private void ReadMarkerStyleFields()
	{
		marker.MarkerStyle = Account.GetCachedMarkerStyle(MarkerStyleComboBox.SelectedResourceId);
	}

	private bool IsPolygonLineOrHybrid(ShapeType shapeType)
	{
		return shapeType == ShapeType.Polygon || shapeType == ShapeType.Line || shapeType == ShapeType.Hybrid;
	}

	private void ReadShapeFields()
	{
		bool useMarkerValues = shapeTypeChanged || markerTypeChanged;

		if (!shapeTypeChangedBetweenPolygonLineOrHybrid)
		{
			// Handle the case where the user needs to switch between related shape types.
			// Set a flag that will preserve the coordinages. The flag is a
			// class variable because this method gets called twice following the switch from
			// one kind of shape to another; however, after we set the flag the first time,
			// information about the old and new shape has changed, so if the flag is already
			// true, we just leave it alone. If it's false, it will be false both times.
			// Clearly this is not the most elegant logic, but it's what works for now.
			shapeTypeChangedBetweenPolygonLineOrHybrid = IsPolygonLineOrHybrid(shapeType) && IsPolygonLineOrHybrid(marker.ShapeType);
		}

		marker.ShapeType = shapeType;

		switch (shapeType)
		{
			case ShapeType.Circle:
				{
					if (useMarkerValues)
					{
						validCircleRadius = marker.CircleRadius;
					}
					marker.ShapeCoordsCircleRadius = validCircleRadius;
					marker.CircleRadius = validCircleRadius;
					CircleRadiusTextBox.Text = validCircleRadius.ToString();
					break;
				}

			case ShapeType.Rectangle:
				{
					if (markerType == MarkerType.Photo)
					{
						if (useMarkerValues)
						{
							validPhotoWidth = marker.PhotoWidth;
							validPhotoHeight = marker.PhotoHeight;
						}

						if (marker.PhotoConstraint == PhotoConstraintType.Width || marker.PhotoConstraint == PhotoConstraintType.WidthAndHeight)
							marker.PhotoWidth = validPhotoWidth;
						if (marker.PhotoConstraint == PhotoConstraintType.Height || marker.PhotoConstraint == PhotoConstraintType.WidthAndHeight)
							marker.PhotoHeight = validPhotoHeight;
					}
					else
					{
						if (useMarkerValues)
						{
							validRectangleWidth = marker.RectangleSize.Width;
							validRectangleHeight = marker.RectangleSize.Height;
						}
						Size validRectangleSize = new Size(validRectangleWidth, validRectangleHeight);
						marker.ShapeCoordsRectangleSize = validRectangleSize;
						marker.RectangleSize = validRectangleSize;
						RectangleWidthTextBox.Text = validRectangleWidth.ToString();
						RectangleHeightTextBox.Text = validRectangleHeight.ToString();
					}
					break;
				}

			case ShapeType.Polygon:
			case ShapeType.Line:
			case ShapeType.Hybrid:
				{
					string coordinates = string.Empty;
					if (useMarkerValues && !shapeTypeChangedBetweenPolygonLineOrHybrid)
					{
						coordinates = shapeType == ShapeType.Hybrid ? account.TempMarkerHybridCoords : account.TempMarkerPolygonCoords;
						if (coordinates == null)
						{
							// Create Polygon that looks like mountains or a Line that looks like a graph.
							coordinates = "0,55,46,14,55,33,84,0,110,45,125,28,139,54";
							if (shapeType == ShapeType.Hybrid)
							{
								// Make the shape a hybrid and add a circle and a rectangle.
								coordinates = "-1,3," + coordinates + ",-1,1,15,11,10,0,-1,2,110,15,140,23";
							}
						}
					}

					if (shapeTypeChangedBetweenPolygonLineOrHybrid)
						coordinates = marker.ShapeCoords;
					else
						marker.ShapeCoords = coordinates;

					// Temporarily remember these coords in case the user changes the marker type or shape, but
					// then returns to polygon/line or hybrid. The temp values are only good while editing this marker.
					if (shapeType == ShapeType.Hybrid)
						account.TempMarkerHybridCoords = coordinates;
					else
						account.TempMarkerPolygonCoords = coordinates;
					break;
				}
		}

		marker.ScaleShapeToMap = ScaleShapeCheckBox.Checked;
	}

	private void ReadSymbolFields()
	{
		int normalSymbolId = NormalSymbolComboBox.SelectedResourceId;
		marker.NormalSymbolId = normalSymbolId;

		int selectedSymbolId = SelectedSymbolComboBox.SelectedResourceId;
		marker.SelectedSymbolId = selectedSymbolId;
	}

	private void SetDefaultTextColors()
	{
		marker.NormalTextColor = "#777777";
		marker.SelectedTextColor = "#000000";
	}

	private void SetShapeType()
	{
		if (markerType == MarkerType.Photo)
		{
			shapeType = ShapeType.Rectangle;
		}
		else
		{
			if (getValuesFromForm)
			{
				try
				{
					shapeType = (ShapeType)int.Parse(MarkerShapeComboBox.SelectedValue);
				}
				catch (Exception)
				{
					// This should never happen, but it has so we trap it until we figure out the cause.
					getValuesFromForm = false;
					Utility.ReportError("MarkerShapeComboBox.SelectedValue is not an integer", string.Format("MarkerType:{0} SelectedValue:'{1}'", markerType, MarkerShapeComboBox.SelectedValue));
				}
			}

			switch (markerType)
			{
				case MarkerType.Shape:
				case MarkerType.SymbolAndShape:
					if (!getValuesFromForm)
						shapeType = account.LastShapeTypeSelected == ShapeType.None ? marker.ShapeType : account.LastShapeTypeSelected;
					
					account.LastShapeTypeSelected = shapeType;
					break;
				
				case MarkerType.Text:
					if (!getValuesFromForm)
					{
						shapeType = account.LastTextShapeTypeSelected == ShapeType.None ? marker.ShapeType : account.LastTextShapeTypeSelected;
						if (shapeType != ShapeType.Rectangle && shapeType != ShapeType.Circle)
						{
							// The marker was previously a shape that's not compatible for Text markers.
							shapeType = ShapeType.Rectangle;
						}
					}
					account.LastTextShapeTypeSelected = shapeType;
					break;

				default:
					Debug.Fail("Unexpected marker type " + markerType);
					break;
			}

			if (markerType != MarkerType.Photo)
			{
				// Make the shape selector visible and clear its items. We do this each time because the
				// set of items listed depends on the marker type. Some types show all shapes whereas others
				// like Text, only list circle and rectangle.
				MarkerShapeComboBox.Items.Clear();
				InitShapeList(markerType, shapeType);
			}
		}

		// Determine if the shape changed.
		shapeTypeChanged = shapeType != marker.ShapeType;

		// When the shape or marker changes, we need to "read" the data for the new marker and shape.
		if (shapeTypeChanged || markerTypeChanged)
			ReadShapeFields();
	}

	private void ShowControlsForMarkerType()
	{
		// First hide all the controls.
		ShowMarkerStyleControls(false);
		ShowSymbolControls(false);
		ShowShapeControls(false);
		ShowTextControls(false);
		ShowPhotoControls(false);
		ShowSymbolLocationControls(false);
		
		// Then just show the controls that apply to the marker type.
		switch (markerType)
		{
			case MarkerType.Symbol:
				ShowSymbolControls(true);
				break;
			
			case MarkerType.Shape:
				ShowMarkerStyleControls(true);
				ShowShapeControls(true);
				break;
			
			case MarkerType.SymbolAndShape:
				ShowMarkerStyleControls(true);
				ShowSymbolControls(true);
				ShowShapeControls(true);
				ShowSymbolLocationControls(true);
				break;
			
			case MarkerType.Text:
				ShowMarkerStyleControls(true);
				ShowShapeControls(true);
				ShowTextControls(true);
				break;
			
			case MarkerType.Photo:
				ShowMarkerStyleControls(true);
				ShowPhotoControls(true);
				break;
			
			default:
				Debug.Fail("Unsupported MarkerType" + markerType);
				break;
		} 
	}

	private void ShowPhotoControls(bool show)
	{
		PhotoPanel.Visible = show;
		PhotoCaptionPanel.Visible = show;
	}

	private void ShowMarkerStyleControls(bool show)
	{
		MarkerStylePanel.Visible = show;
	}

	private void ShowShapeControls(bool show)
	{
		MarkerShapePanel.Visible = show;
		ShapePanels.Visible = show;
	}

	private void ShowSymbolControls(bool show)
	{
		SymbolsPanel.Visible = show;
		
		// Only show the anchor location panel for symbol markers, not for symbol + shape.
		AnchorLocationPanel.Visible = show && markerType == MarkerType.Symbol;
	}

	private void ShowSymbolLocationControls(bool show)
	{
		SymbolLocationPanel.Visible = show;
	}

	private void ShowTextControls(bool show)
	{
		TextOptionsPanel1.Visible = show;
		TextOptionsPanel2.Visible = show;
		TextOptions_Colors.Visible = show;
		TextPaddingPanel.Visible = show;
		TextAlignmentVOptionsPanel.Visible = show;
	}

	private void UpdateControls()
	{
		// This code is executed the first time the page loads and 
		// whenever the user changes the value of an autopostback
		// control which requires other controls on the page to be updated.

		UpdatePreviewControl();

		switch (markerType)
		{
			case MarkerType.Shape:
				UpdateMarkerStyleControls();
				UpdateShapeControls();
				break;

			case MarkerType.Symbol:
				UpdateSymbolControls();
				break;

			case MarkerType.SymbolAndShape:
				UpdateMarkerStyleControls();
				UpdateSymbolControls();
				UpdateShapeControls();
				break;

			case MarkerType.Text:
				UpdateMarkerStyleControls();
				UpdateTextControls();
				UpdateShapeControls();
				UpdateFontControls();
				break;

			case MarkerType.Photo:
				UpdateMarkerStyleControls();
				UpdatePhotoControls();
				if (marker.PhotoCaptionPosition != PhotoCaptionPositionType.None)
				{
					UpdateTextControls();
					UpdateFontControls();
				}
				break;
		}

		bool anchorIsCentered = getValuesFromForm ? AnchorLocationComboBox.SelectedValue == "0" : marker.AnchorLocation == new Point(-1, -1);
		AnchorLocationXYPanel.Visible = !anchorIsCentered;
	}

	private void UpdateFontControls()
	{
		FontStyleResource fontStyle;
		if (getValuesFromForm)
			fontStyle = Account.GetCachedFontStyle(FontStyleComboBox.SelectedResourceId);
		else
			fontStyle = marker.FontStyleResource;
		
		EditFontStyleControl.OnClickActionId = MemberPageActionId.EditFontStyle;
		EditFontStyleControl.Title = "Edit";
		EditFontStyleControl.AppearsEnabled = fontStyle.AccountId != 0;
		EditFontStyleControl.QueryString = "?id=" + fontStyle.Id;
	}

	private void UpdatePhotoControls()
	{
		PhotoPaddingTextBox.Text = marker.PhotoPadding.ToString();
		
		PhotoConstraintComboBox.SelectedValue = ((int)marker.PhotoConstraint).ToString();
		PhotoCropOptionsComboBox.SelectedValue = ((int)marker.PhotoCrop).ToString();
		PhotoCropFactorComboBox.SelectedValue = ((int)marker.PhotoCropFactor).ToString();
		PhotoCaptionPositionComboBox.SelectedValue = ((int)marker.PhotoCaptionPosition).ToString();

		PhotoScalePanel.Visible = marker.PhotoConstraint == PhotoConstraintType.Scale;
		PhotoSizePanel.Visible = marker.PhotoConstraint != PhotoConstraintType.Scale;
		CropOptionsPanel.Visible = marker.PhotoConstraint == PhotoConstraintType.WidthAndHeight;

		if (marker.PhotoCaptionPosition != PhotoCaptionPositionType.None)
		{
			TextOptions_Colors.Visible = true;
			TextOptionsPanel1.Visible = true;
			TextPaddingPanel.Visible = true;
			TextAlignmentVOptionsPanel.Visible = false;
		}

		PhotoWidthRow.Style.Add(HtmlTextWriterStyle.Display, marker.PhotoConstraint == PhotoConstraintType.Height ? "none" : "block");
		PhotoHeightRow.Style.Add(HtmlTextWriterStyle.Display, marker.PhotoConstraint == PhotoConstraintType.Width ? "none" : "block");
		
		SliderPhotoScale.Value = marker.PhotoScale;
		PhotoScaleValue.Text = marker.PhotoScale.ToString() + "%";

		SliderNormalOpacity.Value = marker.PhotoNormalOpacity;
		NormalOpacityValue.Text = marker.PhotoNormalOpacity.ToString() + "%";

		SliderSelectedOpacity.Value = marker.PhotoSelectedOpacity;
		SelectedOpacityValue.Text = marker.PhotoSelectedOpacity.ToString() + "%";

		PhotoEffectNormalComboBox.SelectedValue = ((int)marker.PhotoNormalEffect).ToString();
		PhotoEffectSelectedComboBox.SelectedValue = ((int)marker.PhotoSelectedEffect).ToString();
	}

	private void UpdateMarkerStyleControls()
	{
		MarkerStyle markerStyle;
		if (getValuesFromForm)
			markerStyle = Account.GetCachedMarkerStyle(MarkerStyleComboBox.SelectedResourceId);
		else
			markerStyle = marker.MarkerStyle;

		MarkerStyleComboBox.SelectedResourceId = markerStyle.Id;

		EditMarkerStyleControl.OnClickActionId = MemberPageActionId.EditMarkerStyle;
		EditMarkerStyleControl.Title = "Edit";
		EditMarkerStyleControl.QueryString = "?id=" + marker.MarkerStyle.Id;

		NewMarkerStyleControl.OnClickActionId = MemberPageActionId.CreateMarkerStyle;
		NewMarkerStyleControl.Title = "New";
		NewMarkerStyleControl.QueryString = string.Format("&id={0}&mid={1}", marker.MarkerStyle.Id, marker.Id);
		NewMarkerStyleControl.WarningMessage = string.Format("Click OK to create a duplicate of [@{0}@] and assign it to this marker. You will then be taken to the Edit Marker Style screen so that you can edit your new marker style.", marker.MarkerStyle.Name);
	}

	private void UpdatePreviewControl()
	{
		PreviewImage.Src = "MarkerRenderer.ashx?actual=0&state=0&id=" + markerId;
	}

	private void UpdateShapeControls()
	{
		ShapeType selectedShapeType;

		if (getValuesFromForm)
			selectedShapeType = (ShapeType)int.Parse(MarkerShapeComboBox.SelectedValue);
		else
			selectedShapeType = marker.ShapeType;
		
		bool symbolIsCentered = getValuesFromForm ? SymbolLocationComboBox.SelectedValue == "0" : marker.SymbolLocation == new Point(-1, -1);
		SymbolLocationXYPanel.Visible = !symbolIsCentered;

		// Make visible just the one shape panel corresponding to the selected shape.
		RectanglePanel.Visible = selectedShapeType == ShapeType.Rectangle;
		CirclePanel.Visible = selectedShapeType == ShapeType.Circle;
		
		bool showPolygonHelp = 
			selectedShapeType == ShapeType.Hybrid ||
			selectedShapeType == ShapeType.Line ||
			selectedShapeType == ShapeType.Polygon;
		
		if (showPolygonHelp)
			PolygonHelp.Text = AppContent.Topic("PolygonHelp");

		PolygonHelpPanel.Visible = showPolygonHelp;
	}

	private void UpdateSymbolControls()
	{
		int normalSymbolId = getValuesFromForm ? NormalSymbolComboBox.SelectedResourceId : marker.NormalSymbolId;
		int selectedSymbolId = getValuesFromForm ? SelectedSymbolComboBox.SelectedResourceId : marker.SelectedSymbolId;
		
		NormalSymbolComboBox.SelectedResourceId = normalSymbolId;
		SelectedSymbolComboBox.SelectedResourceId = selectedSymbolId;

		if (marker.NormalSymbolId != 0)
		{
			EditNormalSymbolControl.OnClickActionId = MemberPageActionId.EditSymbol;
			EditNormalSymbolControl.Title = "Edit";
			EditNormalSymbolControl.AppearsEnabled = Account.GetCachedSymbol(marker.NormalSymbolId).AccountId != 0;
			EditNormalSymbolControl.QueryString = "?id=" + marker.NormalSymbolId;

			Symbol normalSymbol = Account.GetCachedSymbol(marker.NormalSymbolId);
			Size size = normalSymbol.Size;
			SymbolDimensions.Text = string.Format("Symbol is {0} x {1}. Upper left is 0,0. Lower right is {2},{3}.", size.Width, size.Height, size.Width - 1, size.Height - 1);
		}
		else
		{
			AnchorLocationPanel.Visible = false;
		}

		if (marker.SelectedSymbolId != 0)
		{
			EditSelectedSymbolControl.OnClickActionId = MemberPageActionId.EditSymbol;
			EditSelectedSymbolControl.Title = "Edit";
			EditSelectedSymbolControl.AppearsEnabled = Account.GetCachedSymbol(marker.SelectedSymbolId).AccountId != 0;
			EditSelectedSymbolControl.QueryString = "?id=" + marker.SelectedSymbolId;
		}
	}

	private void UpdateTextControls()
	{
		NormalTextColorSwatch.ColorValue = marker.NormalTextColor;
		NormalTextColorSwatch.ForPreview = marker.MarkerType == MarkerType.Text;

		SelectedTextColorSwatch.ColorValue = marker.SelectedTextColor;
		SelectedTextColorSwatch.ForPreview = marker.MarkerType == MarkerType.Text;
	
		TextAlignmentVComboBox.SelectedValue = ((int)marker.TextAlignV).ToString();
		TextAlignmentHComboBox.SelectedValue = ((int)marker.TextAlignH).ToString();
		TextPaddingTextBox.Text = marker.TextPadding.ToString();
		FontStyleComboBox.SelectedResourceId = marker.FontStyleResource.Id;
	}

	protected override void Undo()
	{
		ClearErrors();
	}

	protected override void ValidatePage()
	{
		ClearErrors();

		if (!ValidateMarkerName())
			return;

		if (!ValidateAnchorLocation())
			return;

		if (getValuesFromMarker)
			return;
		
		if (marker.MarkerType == MarkerType.Photo)
		{
			ValidatePhotoMarkerSize();
			if (pageValid && marker.PhotoCaptionPosition != PhotoCaptionPositionType.None)
				ValidateText();
			validPhotoPadding = ValidateFieldInRange(PhotoPaddingTextBox, 0, 64, PhotoPaddingError);
			return;
		}

		if (markerType == MarkerType.Symbol || markerType == MarkerType.SymbolAndShape)
		{
			if (!ValidateSymbolPair())
				return;
		}

		if (!MarkerShapeComboBox.Visible)
			return;

		if (!ValididateShapeSize())
			return;

		if (markerType == MarkerType.SymbolAndShape)
		{
			if (!ValidateSymbolLocation())
				return;
		}

		if (marker.MarkerType == MarkerType.Text)
		{
			if (!ValidateText())
				return;
		}
	}

	private bool ValidatePhotoMarkerSize()
	{
		if (marker.PhotoConstraint == PhotoConstraintType.Width || marker.PhotoConstraint == PhotoConstraintType.WidthAndHeight)
			validPhotoWidth = ValidateFieldInRange(PhotoWidthTextBox, 4, 1600, RectangleHeightError);

		if (pageValid && marker.PhotoConstraint == PhotoConstraintType.Height || marker.PhotoConstraint == PhotoConstraintType.WidthAndHeight)
			validPhotoHeight = ValidateFieldInRange(PhotoHeightTextBox, 4, 1600, RectangleHeightError);

		return pageValid;
	}

	private bool ValidateMarkerName()
	{
		validMarkerName = MarkerNameTextBox.Text.Trim();
		ValidateFieldNotBlank(validMarkerName, MarkerNameError, Resources.Text.ErrorMarkerNameRequired);

		if (fieldValid && marker.Name != validMarkerName)
		{
			bool nameInUse = Marker.NameInUse(TourResourceType.Marker, marker.Id, validMarkerName, account.Id);
			ValidateFieldCondition(!nameInUse, MarkerNameError, Resources.Text.ErrorMarkerNameInUse);
		}

		return pageValid;
	}

	private bool ValididateShapeSize()
	{
		if (shapeTypeChanged)
			return true;

		switch (shapeType)
		{
			case ShapeType.Circle:
				if (CircleRadiusTextBox.Visible)
					validCircleRadius = ValidateFieldInRange(CircleRadiusTextBox, 2, 400, CircleRadiusError);
				break;

			case ShapeType.Rectangle:
				if (RectangleWidthTextBox.Visible)
					validRectangleWidth = ValidateFieldInRange(RectangleWidthTextBox, 4, 1600, RectangleWidthError);
				if (RectangleHeightTextBox.Visible)
					validRectangleHeight = ValidateFieldInRange(RectangleHeightTextBox, 4, 1600, RectangleHeightError);
				break;
		}

		return pageValid;
	}

	private bool ValidateAnchorLocation()
	{
		validAnchorLocationX = -1;
		validAnchorLocationY = -1;

		if (AnchorLocationPanel.Visible && AnchorLocationComboBox.SelectedValue == "1")
		{
			int normalSymbolId = getValuesFromForm ? NormalSymbolComboBox.SelectedResourceId : marker.NormalSymbolId;

			if (normalSymbolId > 0)
			{
				Symbol symbol = Account.GetCachedSymbol(normalSymbolId);
				validAnchorLocationX = ValidateFieldInRange(AnchorXTextBox, 0, symbol.Size.Width - 1, AnchorXError);
				validAnchorLocationY = ValidateFieldInRange(AnchorYTextBox, 0, symbol.Size.Height - 1, AnchorYError);
			}
		}

		return pageValid;
	}

	private bool ValidateSymbolPair()
	{
		if (NormalSymbolComboBox.SelectedResourceId == 0 && SelectedSymbolComboBox.SelectedResourceId == 0)
		{
			SetErrorMessage(null, "You must select an image for Normal Symbol and/or Selected Symbol (they can't both be 'No Symbol')");
			pageValid = false;
			return false;
		}
		return true;
	}

	private bool ValidateSymbolLocation()
	{
		validSymbolLocationX = -1;
		validSymbolLocationY = -1;
		
		if (SymbolLocationPanel.Visible && SymbolLocationComboBox.SelectedValue == "1")
		{
			// Treat a blank field as though it were -1 (meaning centered).
			if (SymbolXTextBox.Text.Trim() != string.Empty)
				validSymbolLocationX = ValidateFieldInRange(SymbolXTextBox, 0, 1000, SymbolXError);
			
			if (SymbolYTextBox.Text.Trim() != string.Empty)
				validSymbolLocationY = ValidateFieldInRange(SymbolYTextBox, 0, 1000, SymbolYError);
		}

		return pageValid;
	}

	private bool ValidateText()
	{
		validNormalTextColor = ValidateColorSwatch(NormalTextColorSwatch);
		validSelectedTextColor = ValidateColorSwatch(SelectedTextColorSwatch);
		
		if (!pageValid)
			return false;

		validTextString = TextMarkerStringTextBox.Text.Trim();
		
		if (!pageValid)
			return false;
		
		validTextPadding = ValidateFieldInRange(TextPaddingTextBox, 0, 64, TextPaddingError);

		return pageValid;
	}

	private void ClearErrors()
	{
		ClearErrors(
			MarkerNameError,
			CircleRadiusError,
			PhotoPaddingError,
			RectangleWidthError,
			RectangleHeightError,
			SymbolXError,
			SymbolYError,
			TextMarkerStringError,
			TextPaddingError);
	}
}
