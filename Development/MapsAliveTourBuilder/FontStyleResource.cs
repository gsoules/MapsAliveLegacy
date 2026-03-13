// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

// Note that this class is named FontStyleResource instead of just FontStyle
// so that it does not clash with System.Drawing.FontStyle.

public class FontStyleResource : TourResource
{
	private string _fontFamily;
	private int fontFamilyId;

	public FontStyleResource()
	{
	}

	public FontStyleResource(int fontStyleId)
	{
		if (LoadResourceRowFromDatabase(fontStyleId))
			InitializeResourceFromDataRecord(row);
	}

	public override void InitializeResourceFromDataRecord(MapsAliveDataRecord record)
	{
		Bold = record.BoolValue(Tag.bold);
		FontFamilyId = record.IntValue(Tag.fontFamilyId);
		FontSizePx = record.IntValue(Tag.fontSizePx);
		Italic = record.BoolValue(Tag.italic);
		Underline = record.BoolValue(Tag.underline);
	}

	public enum Tag
	{
		id,
		name,
		fontFamilyId,
		fontSizePx,
		bold,
		italic,
		underline
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
			
			case Tag.fontFamilyId:
				return FontFamilyId.ToString();
			
			case Tag.fontSizePx:
				return FontSizePx.ToString();
			
			case Tag.bold:
				return Bold.ToString();
			
			case Tag.italic:
				return Italic.ToString();
			
			case Tag.underline:
				return Underline.ToString();

			default:
				Debug.Fail("Unsupported FontStyle XML tag requested " + tag);
				return "???";
		}
	}

	public bool Bold { get; set; }
	public int FontSizePx { get; set; }
	public bool Italic { get; set; }
	public bool Underline { get; set; }

	public override bool HasSameAppearanceAs(TourResource resource)
	{
		FontStyleResource that = (FontStyleResource)resource;
		return
			this.Bold == that.Bold &&
			this.fontFamilyId == that.fontFamilyId &&
			this.FontSizePx == that.FontSizePx &&
			this.Italic == that.Italic &&
			this.Underline == that.Underline;
	}

	public string FontFamily
	{
		get
		{
			if (_fontFamily == null)
				_fontFamily = (string)MapsAliveDatabase.LoadScalar("sp_FontFamily_GetFontFamilyByFontFamilyId", "@FontFamilyId", FontFamilyId);
			return _fontFamily;
		}
	}

	public int FontFamilyId
	{
		get { return fontFamilyId; }
		set
		{
			if (value != fontFamilyId)
			{
				fontFamilyId = value;
				_fontFamily = null;
			}
		}
	}

	public FontStyle FontStyle
	{
		get
		{
			FontStyle fontStyle = Bold ? FontStyle.Bold : FontStyle.Regular;
			if (Italic)
				fontStyle |= FontStyle.Italic;
			if (Underline)
				fontStyle |= FontStyle.Underline;
			return fontStyle;
		}
	}

	public override TourResourceType ResourceType
	{
		get { return TourResourceType.FontStyle; }
	}

	private static int OpacityToAlpha(int opacity)
	{
		// Convert an opacity percentage to an alpha value.
		return (int)((opacity * 255) / 100);
	}

	protected override Byte[] GenerateResourceImageBytes()
	{
		const string sampleText = "Sample";
        int fontSize = Math.Min(FontSizePx, 48);
        int w = 200;
		int h = fontSize;
		Bitmap bitmap = new Bitmap(w, h);

		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			graphics.Clear(Color.White);

			SolidBrush textBrush = new SolidBrush(Color.DarkSlateGray);

			Font font = Utility.GetFontForFamilyList(FontFamily, fontSize, FontStyle);

			// Measure the text so that we can center it.
			SizeF sizeF = graphics.MeasureString(sampleText, font);
			int textWidth = (int)Math.Ceiling(sizeF.Width);
			int textOffsetX = (w - textWidth) / 2;
			int textHeight = (int)Math.Ceiling(sizeF.Height);
			int textOffsetY = (h - textHeight) / 2;

			graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
			graphics.DrawString(sampleText, font, textBrush, textOffsetX, textOffsetY);
		}

		return Utility.ImageToByteArray(bitmap, ImageFormat.Png);
	}

	public override void InsertIntoDatabase(int accountId)
	{
		resourceId = (int)MapsAliveDatabase.ReadScalar("sp_FontStyle_CreateFontStyle", "@AccountId", accountId);
		UpdateDatabase();
	}

	public override void UpdateDatabase()
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_FontStyle_UpdateFontStyle",
			"@FontStyleId", resourceId,
			"@Name", Name,
			"@FontFamilyId", FontFamilyId,
			"@FontSizePx", FontSizePx,
			"@Bold", Bold,
			"@Italic", Italic,
			"@Underline", Underline
		);
	}
}
