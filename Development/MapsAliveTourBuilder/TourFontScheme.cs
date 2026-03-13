// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;

public class TourFontScheme
{
	private int fontSchemeId;

	private string fontFamilyHeading;
	private string fontFamilyDescription;
	private string fontFamilyTitle;
	private string fontFamilyFooter;
    private string fontFamilyMenuItem;
    private string fontFamilyMenuSlideItem;
	private string fontSizeHeading;
	private string fontSizeDescription;
	private string fontSizeTitle;
	private string fontSizeFooter;
    private string fontSizeMenuItem;
    private string fontSizeMenuSlideItem;
	private string fontStyleHeading;
	private string fontStyleDescription;
	private string fontStyleTitle;
	private string fontStyleFooter;
    private string fontStyleMenuItem;
    private string fontStyleMenuSlideItem;
	private string fontWeightHeading;
	private string fontWeightDescription;
	private string fontWeightTitle;
	private string fontWeightFooter;
    private string fontWeightMenuItem;
    private string fontWeightMenuSlideItem;
	private bool preview;

	public TourFontScheme(int fontSchemeId, bool preview)
	{
		this.fontSchemeId = fontSchemeId;
		this.preview = preview;
		LoadFontSchemeFromDatabase();
	}

	public string FontFamilyHeading
	{
		get { return fontFamilyHeading; }
	}

	public string FontFamilyDescription
	{
		get { return fontFamilyDescription; }
	}

	public string FontFamilyTitle
	{
		get { return fontFamilyTitle; }
	}

	public string FontFamilyFooter
	{
		get { return fontFamilyFooter; }
	}

	public string FontFamilyMenuItem
	{
		get { return fontFamilyMenuItem; }
	}

	public string FontFamilyMenuSlideItem
	{
		get { return fontFamilyMenuSlideItem; }
	}

	public string FontSizeHeading
	{
		get { return fontSizeHeading; }
	}

	public string FontSizeDescription
	{
		get { return fontSizeDescription; }
	}

	public string FontSizeTitle
	{
		get { return fontSizeTitle; }
	}

	public string FontSizeFooter
	{
		get { return fontSizeFooter; }
	}

	public string FontSizeMenuItem
	{
		get { return fontSizeMenuItem; }
	}

	public string FontSizeMenuSlideItem
	{
		get { return fontSizeMenuSlideItem; }
	}

	public string FontStyleHeading
	{
		get { return TranslateFontStyle(fontStyleHeading); }
	}

	public string FontStyleDescription
	{
		get { return TranslateFontStyle(fontStyleDescription); }
	}

	public string FontStyleTitle
	{
		get { return TranslateFontStyle(fontStyleTitle); }
	}

	public string FontStyleFooter
	{
		get { return TranslateFontStyle(fontStyleFooter); }
	}

	public string FontStyleMenuItem
	{
		get { return TranslateFontStyle(fontStyleMenuItem); }
	}

	public string FontStyleMenuSlideItem
	{
		get { return TranslateFontStyle(fontStyleMenuSlideItem); }
	}

	public string FontWeightHeading
	{
		get { return TranslateFontStyle(fontWeightHeading); }
	}

	public string FontWeightDescription
	{
		get { return TranslateFontStyle(fontWeightDescription); }
	}

	public string FontWeightTitle
	{
		get { return TranslateFontStyle(fontWeightTitle); }
	}

	public string FontWeightFooter
	{
		get { return TranslateFontStyle(fontWeightFooter); }
	}

	public string FontWeightMenuItem
	{
		get { return TranslateFontStyle(fontWeightMenuItem); }
	}

	public string FontWeightMenuSlideItem
	{
		get { return TranslateFontStyle(fontWeightMenuSlideItem); }
	}

	private void LoadFontSchemeFromDatabase()
	{
		DataTable dataTable = Tour.OptionsForFontScheme;
		MapsAliveDataRow row = null;

		// Find the row for this font scheme.
		foreach (DataRow dataRow in dataTable.Rows)
		{
			row = new MapsAliveDataRow(dataRow);
			if (fontSchemeId == row.IntValue("TourFontSchemeId"))
				break;
		}
		Debug.Assert(row != null, "Expected to find font scheme row");

		fontFamilyHeading = row.StringValue("FamilyHeading");
        fontSizeHeading = row.StringValue("SizeHeading");
        fontStyleHeading = row.StringValue("StyleHeading");
        fontWeightHeading = row.StringValue("WeightHeading");

		fontFamilyDescription = row.StringValue("FamilyDescription");
        fontSizeDescription = row.StringValue("SizeDescription");
        fontStyleDescription = row.StringValue("StyleDescription");
        fontWeightDescription = row.StringValue("WeightDescription");

        fontFamilyTitle = row.StringValue("FamilyTitle");
        fontSizeTitle = row.StringValue("SizeTitle");
        fontStyleTitle = row.StringValue("StyleTitle");
        fontWeightTitle = row.StringValue("WeightTitle");

        fontFamilyFooter = row.StringValue("FamilyFooter");
        fontSizeFooter = row.StringValue("SizeFooter");
        fontStyleFooter = row.StringValue("StyleFooter");
        fontWeightFooter = row.StringValue("WeightFooter");

        fontFamilyMenuItem = row.StringValue("FamilyMenuItem");
        fontSizeMenuItem = row.StringValue("SizeMenuItem");
        fontStyleMenuItem = row.StringValue("StyleMenuItem");
        fontWeightMenuItem = row.StringValue("WeightMenuItem");

        fontFamilyMenuSlideItem = row.StringValue("FamilyMenuSlideItem");
        fontSizeMenuSlideItem = row.StringValue("SizeMenuSlideItem");
        fontStyleMenuSlideItem = row.StringValue("StyleMenuSlideItem");
        fontWeightMenuSlideItem = row.StringValue("WeightMenuSlideItem");

        // V4 overrides the default font scheme which uses smaller Verdana fonts. A future version
        // of MapsAlive should allow the user to create, edit, and choose a font scheme for the tour.
        Tour tour = MapsAliveState.SelectedTourOrNull;
		if (tour != null && tour.V4)
        {
            fontSizeHeading = "14";
            fontFamilyHeading = "Arial, Helvetica, Verdana, Sans-Serif";
            fontSizeDescription = "12";
            fontFamilyDescription = "Arial, Helvetica, Verdana, Sans-Serif";
        }
    }

	private string TranslateFontStyle(string styleName)
	{
		if (preview)
		{
			int styleValue = 0;

			switch (styleName.Trim())
			{
				case "bold": styleValue = (int)FontStyle.Bold; break;
				case "bolder": styleValue = (int)FontStyle.Bold; break;
				case "italic": styleValue = (int)FontStyle.Italic; break;
				case "normal": styleValue = (int)FontStyle.Regular; break;
			}

			return styleValue.ToString();
		}
		else
		{
			return styleName;
		}
	}
}
