// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

public class MarkerStyle : TourResource
{
	public MarkerStyle()
	{
		NormalFillColor = string.Empty;
		NormalLineColor = string.Empty;
		NormalShapeEffects = string.Empty;
		SelectedFillColor = string.Empty;
		SelectedLineColor = string.Empty;
		SelectedShapeEffects = string.Empty;
	}

	public MarkerStyle(int accountId, string name)
	{
		this.accountId = accountId;
		if (name == null)
			this.Name = CreateUniqueNameForNewResource(TourResourceType.MarkerStyle);
		else
			this.Name = name;
	}

	public MarkerStyle(int markerStyleId)
	{
		if (LoadResourceRowFromDatabase(markerStyleId))
			InitializeResourceFromDataRecord(row);
	}

	public override void InitializeResourceFromDataRecord(MapsAliveDataRecord record)
	{
		LineWidth = record.IntValue("LineWidth", Tag.lineThickness);
		NormalFillColor = record.ColorValue(Tag.normalFillColor);
		NormalFillColorOpacity = record.IntValue(Tag.normalFillColorOpacity);
		NormalLineColor = record.ColorValue(Tag.normalLineColor);
		NormalLineColorOpacity = record.IntValue(Tag.normalLineColorOpacity);
		SelectedFillColor = record.ColorValue(Tag.selectedFillColor);
		SelectedFillColorOpacity = record.IntValue(Tag.selectedFillColorOpacity);
		SelectedLineColor = record.ColorValue(Tag.selectedLineColor);
		SelectedLineColorOpacity = record.IntValue(Tag.selectedLineColorOpacity);
		NormalShapeEffects = record.StringValue("NormalShapeEffects", Tag.normalEffects);
		SelectedShapeEffects = record.StringValue("SelectedShapeEffects", Tag.selectedEffects);
	}

	public enum Tag
	{
		id,
		name,
		lineThickness,
		normalFillColor,
		normalFillColorOpacity,
		normalLineColor,
		normalLineColorOpacity,
		normalEffects,
		selectedFillColor,
		selectedFillColorOpacity,
		selectedLineColor,
		selectedLineColorOpacity,
		selectedEffects
	}

	public override string GetTagValue(int tagId)
	{
		Tag tag = (Tag)tagId;

		switch (tag)
		{
			case Tag.id:
				return Id.ToString();
			
			case Tag.name:
				return Name;
			
			case Tag.lineThickness:
				return LineWidth.ToString();
			
			case Tag.normalFillColor:
				return NormalFillColor;
			
			case Tag.normalFillColorOpacity:
				return NormalFillColorOpacity.ToString();
			
			case Tag.normalLineColor:
				return NormalLineColor;
			
			case Tag.normalLineColorOpacity:
				return NormalLineColorOpacity.ToString();
			
			case Tag.normalEffects:
				return NormalShapeEffects;
			
			case Tag.selectedFillColor:
				return SelectedFillColor;
			
			case Tag.selectedFillColorOpacity:
				return SelectedFillColorOpacity.ToString();
			
			case Tag.selectedLineColor:
				return SelectedLineColor;
			
			case Tag.selectedLineColorOpacity:
				return SelectedLineColorOpacity.ToString();
			
			case Tag.selectedEffects:
				return SelectedShapeEffects;
			
			default:
				Debug.Fail("Unsupported MarkerStyle XML tag requested " + tag);
				return "???";
		}
	}

	public int LineWidth { get; set; }
	public string NormalFillColor { get; set; }
	public int NormalFillColorOpacity { get; set; }
	public string NormalLineColor { get; set; }
	public int NormalLineColorOpacity { get; set; }
	public string NormalShapeEffects { get; set; }
	public string SelectedFillColor { get; set; }
	public int SelectedFillColorOpacity { get; set; }
	public string SelectedLineColor { get; set; }
	public int SelectedLineColorOpacity { get; set; }
	public string SelectedShapeEffects { get; set; }
		
	public override bool HasSameAppearanceAs(TourResource resource)
	{
		MarkerStyle that = (MarkerStyle)resource;
		return
			this.LineWidth == that.LineWidth &&
			this.NormalFillColor == that.NormalFillColor &&
			this.NormalFillColorOpacity == that.NormalFillColorOpacity &&
			this.NormalLineColor == that.NormalLineColor &&
			this.NormalLineColorOpacity == that.NormalLineColorOpacity &&
			this.NormalShapeEffects == that.NormalShapeEffects &&
			this.SelectedFillColor == that.SelectedFillColor &&
			this.SelectedFillColorOpacity == that.SelectedFillColorOpacity &&
			this.SelectedLineColor == that.SelectedLineColor &&
			this.SelectedLineColorOpacity == that.SelectedLineColorOpacity &&
			this.SelectedShapeEffects == that.SelectedShapeEffects;
	}

	protected override Byte[] GenerateResourceImageBytes()
	{
		// Create a temporary marker having this marker style.
		Marker marker = new Marker(this, accountId);

		// Give the marker a rectangle shape for the style's color swatch.
		marker.ShapeType = ShapeType.Rectangle;
		marker.ShapeCoords = string.Format("0,0,48,48");

		// Use the combo image bytes created for the temporary marker as the styles bytes.
		return marker.ResourceImageBytes;
	}

	public override void InsertIntoDatabase(int accountId)
	{
		if (accountId == 0)
			resourceId = (int)MapsAliveDatabase.ReadScalar("sp_MarkerStyle_CreateSystemMarkerStyle", "@Name", Name);
		else
			resourceId = (int)MapsAliveDatabase.ReadScalar("sp_MarkerStyle_CreateMarkerStyle", "@AccountId", accountId);

		UpdateDatabase();
	}

	public override TourResourceType ResourceType
	{
		get { return TourResourceType.MarkerStyle; }
	}

	public override void UpdateDatabase()
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_MarkerStyle_UpdateMarkerStyle",
			"@MarkerStyleId", resourceId,
			"@Name", Name,
			"@LineWidth", LineWidth,
			"@NormalFillColor", NormalFillColor,
			"@NormalFillColorOpacity", NormalFillColorOpacity,
			"@NormalLineColor", NormalLineColor,
			"@NormalLineColorOpacity", NormalLineColorOpacity,
			"@SelectedFillColor", SelectedFillColor,
			"@SelectedFillColorOpacity", SelectedFillColorOpacity,
			"@SelectedLineColor", SelectedLineColor,
			"@SelectedLineColorOpacity", SelectedLineColorOpacity,
			"@NormalShapeEffects", NormalShapeEffects,
			"@SelectedShapeEffects", SelectedShapeEffects
		);
	}
}
