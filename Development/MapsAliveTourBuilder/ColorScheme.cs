// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

public class ColorScheme : TourResource
{
	public ColorScheme()
	{
	}

	public ColorScheme(int colorSchemeId)
	{
		if (LoadResourceRowFromDatabase(colorSchemeId))
			InitializeResourceFromDataRecord(row);
	}

	public override void InitializeResourceFromDataRecord(MapsAliveDataRecord record)
	{
		LayoutAreaBackgroundColor = record.StringValue("TourBackground", Tag.layoutAreaBackgroundColor);
		TitleTextColor = record.StringValue("TitleText", Tag.titleTextColor);
		TitleBackgroundColor = record.StringValue("TitleBackground", Tag.titleBackgroundColor);
		StripeColor = record.StringValue("HeaderBarBackground", Tag.stripeColor);
		StripeBorderColor = record.StringValue("HeaderBarTopBorder", Tag.stripeBorderColor);
		FooterLinkTextColor = record.StringValue("FooterLinkText", Tag.footerLinkTextColor);
		MenuBackgroundColor = record.StringValue("MenuAreaBackground", Tag.menuBackgroundColor);
		MenuNormalTextColor = record.StringValue("MenuText", Tag.menuNormalTextColor);
		MenuNormalBackgroundColor = record.StringValue("MenuBackground", Tag.menuNormalBackgroundColor);
		MenuLineColor = record.StringValue("MenuBorder", Tag.menuLineColor);
		MenuSelectedTextColor = record.StringValue("MenuCurrentText", Tag.menuSelectedTextColor);
		MenuSelectedBackgroundColor = record.StringValue("MenuCurrentBackground", Tag.menuSelectedBackgroundColor);
		MenuHoverTextColor = record.StringValue("MenuHoverText", Tag.menuHoverTextColor);
		MenuHoverBackgroundColor = record.StringValue("MenuHoverBackground", Tag.menuHoverBackgroundColor);
		SlideTitleTextColor = record.StringValue("SlideTitleText", Tag.hotspotTitleTextColor);
		SlideTextColor = record.StringValue("SlideText", Tag.hotspotTextColor);
		SlideBackgroundColor = record.StringValue("SlideBackground", Tag.hotspotBackgroundColor);
	}

	public enum Tag
	{
		id,
		name,
		layoutAreaBackgroundColor,
		titleTextColor,
		titleBackgroundColor,
		stripeColor,
		stripeBorderColor,
		footerLinkTextColor,
		menuBackgroundColor,
		menuNormalTextColor,
		menuNormalBackgroundColor,
		menuLineColor,
		menuSelectedTextColor,
		menuSelectedBackgroundColor,
		menuHoverTextColor,
		menuHoverBackgroundColor,
		hotspotTitleTextColor,
		hotspotTextColor,
		hotspotBackgroundColor
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

			case Tag.layoutAreaBackgroundColor:
				return LayoutAreaBackgroundColor;
			
			case Tag.titleTextColor:
				return TitleTextColor;
			
			case Tag.titleBackgroundColor:
				return TitleBackgroundColor;

			case Tag.stripeColor:
				return StripeColor;

			case Tag.stripeBorderColor:
				return StripeBorderColor;

			case Tag.footerLinkTextColor:
				return FooterLinkTextColor;

			case Tag.menuBackgroundColor:
				return MenuBackgroundColor;

			case Tag.menuNormalTextColor:
				return MenuNormalTextColor;

			case Tag.menuNormalBackgroundColor:
				return MenuNormalBackgroundColor;

			case Tag.menuLineColor:
				return MenuLineColor;

			case Tag.menuSelectedTextColor:
				return MenuSelectedTextColor;

			case Tag.menuSelectedBackgroundColor:
				return MenuSelectedBackgroundColor;

			case Tag.menuHoverTextColor:
				return MenuHoverTextColor;

			case Tag.menuHoverBackgroundColor:
				return MenuHoverBackgroundColor;

			case Tag.hotspotTitleTextColor:
				return SlideTitleTextColor;

			case Tag.hotspotTextColor:
				return SlideTextColor;

			case Tag.hotspotBackgroundColor:
				return SlideBackgroundColor;
			
			default:
				Debug.Fail("Unsupported ColorScheme XML tag requested " + tag);
				return "???";
		}
	}

	public override TourResourceType ResourceType
	{
		get { return TourResourceType.TourStyle; }
	}

    public string FooterLinkTextColor { get; set; }
	public string MenuBackgroundColor { get; set; }
	public string MenuHoverBackgroundColor { get; set; }
	public string MenuHoverTextColor { get; set; }
	public string MenuLineColor { get; set; }
	public string MenuNormalBackgroundColor { get; set; }
	public string MenuNormalTextColor { get; set; }
	public string MenuSelectedBackgroundColor { get; set; }
	public string MenuSelectedTextColor { get; set; }
    public string SlideBackgroundColor { get; set; }
    public string SlideTextColor { get; set; }
	public string SlideTitleTextColor { get; set; }
 	public string StripeBorderColor { get; set; }
	public string StripeColor { get; set; }
	public string TitleBackgroundColor { get; set; }
	public string TitleTextColor { get; set; }
	public string LayoutAreaBackgroundColor { get; set; }
		
	public override bool HasSameAppearanceAs(TourResource resource)
	{
		ColorScheme that = (ColorScheme)resource;
		return
			this.FooterLinkTextColor == that.FooterLinkTextColor &&
			this.MenuBackgroundColor == that.MenuBackgroundColor &&
			this.MenuHoverBackgroundColor == that.MenuHoverBackgroundColor &&
			this.MenuHoverTextColor == that.MenuHoverTextColor &&
			this.MenuLineColor == that.MenuLineColor &&
			this.MenuNormalBackgroundColor == that.MenuNormalBackgroundColor &&
			this.MenuNormalTextColor == that.MenuNormalTextColor &&
			this.MenuSelectedBackgroundColor == that.MenuSelectedBackgroundColor &&
			this.MenuSelectedTextColor == that.MenuSelectedTextColor &&
			this.SlideBackgroundColor == that.SlideBackgroundColor &&
			this.SlideTextColor == that.SlideTextColor &&
			this.SlideTitleTextColor == that.SlideTitleTextColor &&
			this.StripeBorderColor == that.StripeBorderColor &&
			this.StripeColor == that.StripeColor &&
			this.TitleBackgroundColor == that.TitleBackgroundColor &&
			this.TitleTextColor == that.TitleTextColor &&
			this.LayoutAreaBackgroundColor == that.LayoutAreaBackgroundColor;
	}

	protected override Byte[] GenerateResourceImageBytes()
	{
		const int w = 48 + 2;
		const int h = 32 + 2;
		Bitmap bitmap = new Bitmap(w, h);

		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			Rectangle rect = new Rectangle(0, 0, w, h);
			SolidBrush solidBrush = new SolidBrush(Color.LightGray);
			graphics.FillRectangle(solidBrush, rect);

			rect = new Rectangle(1, 1, 16, 16);
			solidBrush = new SolidBrush(Utility.HexToColor(MenuNormalTextColor));
			graphics.FillRectangle(solidBrush, rect);
			
			rect = new Rectangle(17, 1, 16, 16);
			solidBrush = new SolidBrush(Utility.HexToColor(LayoutAreaBackgroundColor));
			graphics.FillRectangle(solidBrush, rect);
			
			rect = new Rectangle(33, 1, 16, 16);
			solidBrush = new SolidBrush(Utility.HexToColor(MenuNormalBackgroundColor));
			graphics.FillRectangle(solidBrush, rect);
			
			rect = new Rectangle(1, 17, 16, 16);
			solidBrush = new SolidBrush(Utility.HexToColor(SlideBackgroundColor));
			graphics.FillRectangle(solidBrush, rect);
			
			rect = new Rectangle(17, 17, 16, 16);
			solidBrush = new SolidBrush(Utility.HexToColor(MenuSelectedTextColor));
			graphics.FillRectangle(solidBrush, rect);
			
			rect = new Rectangle(33, 17, 16, 16);
			solidBrush = new SolidBrush(Utility.HexToColor(TitleBackgroundColor));
			graphics.FillRectangle(solidBrush, rect);
		}

		return Utility.ImageToByteArray(bitmap, ImageFormat.Png);
	}

	public override void InsertIntoDatabase(int accountId)
	{
		resourceId = (int)MapsAliveDatabase.ReadScalar("sp_TourStyle_CreateTourStyle", "@AccountId", accountId);
		UpdateDatabase();
	}

	public static void InvalidateToursThatDependOnColorScheme(int colorSchemeId)
	{
		DataTable dataTable = MapsAliveDatabase.LoadDataTable("sp_Tour_GetToursThatUseTourStyle",
			"@AccountId", Utility.AccountId,
			"@TourStyleId", colorSchemeId);

		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			Tour tour = Tour.GetSelectedTourOrCreateFromDatabase(row.IntValue("TourId"));
			tour.SwitchColorScheme();
			Utility.Trace(string.Format("InvalidateToursThatDependOnTourStyle {0}", tour.Name));
		}
	}

	public static void SynchronizeColorsForDirectory(Tour tour)
	{
		TourDirectory directory = tour.Directory;
		ColorScheme colorScheme = tour.ColorScheme;

		if (directory.UseColorSchemeColors)
		{
			directory.TitleTextColor = colorScheme.MenuSelectedTextColor;
			directory.TitleBarColor = colorScheme.MenuSelectedBackgroundColor;
			directory.BorderColor = colorScheme.MenuLineColor;
			directory.PreviewBorderColor = colorScheme.MenuLineColor;
			directory.PreviewTextColor = colorScheme.SlideTextColor;
			directory.PreviewBackgroundColor = colorScheme.SlideBackgroundColor;
			directory.StatusTextColor = colorScheme.MenuSelectedTextColor;
			directory.StatusBackgroundColor = colorScheme.MenuSelectedBackgroundColor;
			directory.UpdateDatabase();
		}
	}

	public static void SynchronizeColorsForPopup(Tour tour)
	{
		ColorScheme colorScheme = tour.ColorScheme;

		foreach (TourPage tourPage in tour.TourPages)
		{
			if (tourPage.IsDataSheet || !tourPage.PopupOptions.UseColorSchemeColors)
				continue;

			SynchronizeColorsForPopup(tourPage, colorScheme);
		}
	}

	public static void SynchronizeColorsForPopup(TourPage tourPage, ColorScheme colorScheme)
	{
		tourPage.PopupOptions.BackgroundColor = colorScheme.SlideBackgroundColor;
		tourPage.PopupOptions.TitleTextColor = colorScheme.SlideTitleTextColor;
		tourPage.PopupOptions.TextColor = colorScheme.SlideTextColor;
		tourPage.UpdateDatabase();
	}

	public override void UpdateDatabase()
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourStyle_UpdateTourStyle",
			"@TourStyleId", resourceId,
			"@Name", Name,
			"TourBackground", LayoutAreaBackgroundColor,
			"TitleText", TitleTextColor,
			"TitleBackground", TitleBackgroundColor,
			"HeaderBarBackground", StripeColor,
			"HeaderBarTopBorder", StripeBorderColor,
			"FooterLinkText", FooterLinkTextColor,
			"MenuAreaBackground", MenuBackgroundColor,
			"MenuText", MenuNormalTextColor,
			"MenuBackground", MenuNormalBackgroundColor,
			"MenuBorder", MenuLineColor,
			"MenuCurrentText", MenuSelectedTextColor,
			"MenuCurrentBackground", MenuSelectedBackgroundColor,
			"MenuHoverText", MenuHoverTextColor,
			"MenuHoverBackground", MenuHoverBackgroundColor,
			"SlideTitleText", SlideTitleTextColor,
			"SlideText", SlideTextColor,
			"SlideBackground", SlideBackgroundColor
		);
	}
}
