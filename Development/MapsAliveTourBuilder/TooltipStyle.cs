// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

public class TooltipStyle : TourResource
{
	private FontStyleResource _fontStyleResource;
	private int fontStyleId;

	public TooltipStyle()
	{
	}

	public TooltipStyle(int tooltipStyleId)
	{
		if (LoadResourceRowFromDatabase(tooltipStyleId))
			InitializeResourceFromDataRecord(row);
	}

	public override void InitializeResourceFromDataRecord(MapsAliveDataRecord record)
	{
		fontStyleId = record.IntValue(Tag.fontStyleId);
		BackgroundColor = record.StringValue(Tag.backgroundColor);
		BackgroundColorOpacity = record.IntValue(Tag.backgroundColorOpacity);
		LineColor = record.StringValue(Tag.lineColor);
		LineWidth = record.IntValue("LineWidth", Tag.lineThickness);
		MaxWidth = record.IntValue(Tag.maxWidth);
		Padding = record.IntValue(Tag.padding);
		TextColor = record.StringValue(Tag.textColor);
	}

	public enum Tag
	{
		id,
		name,
		fontStyleId,
		textColor,
		lineColor,
		backgroundColor,
		backgroundColorOpacity,
		lineThickness,
		padding,
		maxWidth
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
			
			case Tag.fontStyleId:
				return fontStyleId.ToString();

			case Tag.textColor:
				return TextColor;
			
			case Tag.lineColor:
				return LineColor;
			
			case Tag.backgroundColor:
				return BackgroundColor;
			
			case Tag.backgroundColorOpacity:
				return BackgroundColorOpacity.ToString();
			
			case Tag.lineThickness:
				return LineWidth.ToString();
			
			case Tag.padding:
				return Padding.ToString();
			
			case Tag.maxWidth:
				return MaxWidth.ToString();

			default:
				Debug.Fail("Unsupported Tooltip XML tag requested " + tag);
				return "???";
		}
	}

	public string BackgroundColor { get; set; }
	private int BackgroundColorOpacity { get; set; }
	public string LineColor { get; set; }
	public int LineWidth { get; set; }
	public int MaxWidth { get; set; }
	public int Padding { get; set; }
	public string TextColor { get; set; }
		
	public override bool HasSameAppearanceAs(TourResource resource)
	{
		TooltipStyle that = (TooltipStyle)resource;
		return
			this.BackgroundColor == that.BackgroundColor &&
			this.BackgroundColorOpacity == that.BackgroundColorOpacity &&
			this.fontStyleId == that.fontStyleId &&
			this.LineColor == that.LineColor &&
			this.LineWidth == that.LineWidth &&
			this.MaxWidth == that.MaxWidth &&
			this.Padding == that.Padding &&
			this.TextColor == that.TextColor;
	}

	public bool BackgroundIsTransparent
	{
		get { return BackgroundColorOpacity == 0; }
		set { BackgroundColorOpacity = value ? 0 : 100; }
	}

	public override TourResourceType ResourceType
	{
		get { return TourResourceType.TooltipStyle; }
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
			fontStyleId = _fontStyleResource.Id;
		}
	}

	public int FontStyleResourceId
	{
		get { return fontStyleId; }
		set { fontStyleId = value; }
	}

	protected override Byte[] GenerateResourceImageBytes()
	{
		const string sampleText = "Sample";
		const int w = 160;
		int h = FontStyleResource.FontSizePx + (Padding * 2);
		Bitmap bitmap = new Bitmap(w, h);

		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			// Clear the background with cyan so that we'll notice if the code below is not fully
			// drawing the tooltip fill color and border. The cyan should not show anywhere.
			graphics.Clear(Color.Cyan);
			
			SolidBrush textBrush = new SolidBrush(Utility.HexToColor(TextColor));
			SolidBrush backgroundBrush = new SolidBrush(Utility.HexToColor(BackgroundIsTransparent ? "#ffffff" : BackgroundColor));
			Pen pen = new Pen(Utility.HexToColor(LineColor));
		
			Font font = Utility.GetFontForFamilyList(FontStyleResource.FontFamily, FontStyleResource.FontSizePx, FontStyleResource.FontStyle);
			
			// Measure the text so that we can center it.
			SizeF sizeF = graphics.MeasureString(sampleText, font);
			int textWidth = (int)Math.Ceiling(sizeF.Width);
			int textOffsetX = (w - textWidth) / 2;
			int textHeight = (int)Math.Ceiling(sizeF.Height);
			int textOffsetY = (h - textHeight) / 2;

			graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

			Rectangle rect = new Rectangle(0, 0, w - 1, h - 1);
			graphics.FillRectangle(backgroundBrush, rect);
			graphics.DrawString(sampleText, font, textBrush, textOffsetX, textOffsetY);
			graphics.DrawRectangle(pen, rect);
		}

		return Utility.ImageToByteArray(bitmap, ImageFormat.Png);
	}

	public override void InsertIntoDatabase(int accountId)
	{
		resourceId = (int)MapsAliveDatabase.ReadScalar("sp_TooltipStyle_CreateTooltipStyle", "@AccountId", accountId);
		UpdateDatabase();
	}

	public static void InvalidateMapsThatDependOnTooltipStyle(int tooltipStyleId)
	{
		DataTable dataTable = MapsAliveDatabase.LoadDataTable("sp_TourPage_GetTourPagesThatUseTooltipStyle",
			"@AccountId", Utility.AccountId,
			"@TooltipStyleId", tooltipStyleId);

		foreach (DataRow dataRow in dataTable.Rows)
		{
		    MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
		    int tourId = row.IntValue("TourId");
		    int tourPageId = row.IntValue("TourPageId");
		    Tour tour = Tour.GetSelectedTourOrCreateFromDatabase(tourId);
		    TourPage tourPage = tour.GetInMemoryTourPageOrCreateFromDatabase(tour, tourPageId);
		    tourPage.SetTooltipStyleChanged();
		    tourPage.UpdateDatabaseTooltipStyle();
			Utility.Trace(string.Format("InvalidateMapsThatDependOnTooltipStyle {0} : {1}", tour.Name, tourPage.Name));
		}
	}

	public override void UpdateDatabase()
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TooltipStyle_UpdateTooltipStyle",
			"@TooltipStyleId", resourceId,
			"@Name", Name,
			"@FontStyleId", fontStyleId,
			"@TextColor", TextColor,
			"@Padding", Padding,
			"@BackgroundColor", BackgroundColor,
			"@BackgroundColorOpacity", BackgroundColorOpacity,
			"@LineWidth", LineWidth,
			"@LineColor", LineColor,
			"@MaxWidth", MaxWidth
		);
	}
}
