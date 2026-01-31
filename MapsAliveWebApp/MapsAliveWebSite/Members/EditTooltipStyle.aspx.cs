// Copyright (C) 2003-2010 AvantLogic Corporation
using System.Data;
using System.Web.UI.WebControls;

public partial class Members_EditTooltipStyle : EditStylePage
{
	private string fontStyleTable;
	private string validTextColor;
	private string validBackgroundColor;
	private string validLineColor;

	protected override void EmitJavaScript()
	{
		string loadingScript = string.Empty;

		string loadedScript = string.Format("maInitTooltipStylePreview({0},'{1}','{2}','{3}',{4},{5},{6});",
			tooltipStyle.LineWidth,
			tooltipStyle.LineColor,
			tooltipStyle.BackgroundIsTransparent ? "transparent" : tooltipStyle.BackgroundColor,
			tooltipStyle.TextColor,
			tooltipStyle.Padding,
			tooltipStyle.FontStyleResource.Id,
			tooltipStyle.MaxWidth
			);

		EmitJavaScript(loadingScript, loadedScript);
	}

	protected void AddChangeDetection(CheckBox checkBox, string script)
	{
		checkBox.Attributes.Add("onclick", script);
	}

	protected override void InitControls(bool undo)
	{
		LineWidthValue.Text = tooltipStyle.LineWidth.ToString() + " px";
		PaddingValue.Text = tooltipStyle.Padding.ToString() + " px";
		MaxWidthValue.Text = tooltipStyle.MaxWidth.ToString() + " px";

		FontStyleComboBox.ResourceType = TourResourceType.FontStyle;
		FontStyleComboBox.SelectedResourceId = tooltipStyle.FontStyleResource.Id;
		FontStyleComboBox.OnClientSelectedIndexChangedScript = "maFontStyleChanged(sender, eventArgs);";

		EditFontStyleControl.OnClickActionId = MemberPageActionId.EditFontStyle;
		EditFontStyleControl.Title = "Edit";
		EditFontStyleControl.AppearsEnabled = tooltipStyle.FontStyleResource.AccountId != 0;
		EditFontStyleControl.QueryString = "?id=" + tooltipStyle.FontStyleResource.Id;

		MemberPage.InitShowUsageControl(ShowUsageControl, tooltipStyle);
		InitFontStyleTable();
		
		if (!undo && IsPostBack)
			return;

		TooltipStyleNameTextBox.Text = tooltipStyle.Name;
		AddChangeDetectionForPreview(TooltipStyleNameTextBox);

		TextColorSwatch.ColorValue = tooltipStyle.TextColor;
		BackgroundColorSwatch.ColorValue = tooltipStyle.BackgroundColor;
		LineColorSwatch.ColorValue = tooltipStyle.LineColor;

		TransparentBackgroundCheckBox.Checked = tooltipStyle.BackgroundIsTransparent;
		AddChangeDetection(TransparentBackgroundCheckBox, "maBackgroundTransparencyChanged(this);");

		SliderLineWidth.Value = tooltipStyle.LineWidth;
		SliderPadding.Value = tooltipStyle.Padding;
		SliderMaxWidth.Value = tooltipStyle.MaxWidth;
	}

	protected override void PageLoad()
	{
		Utility.RegisterColorChooserJavaScript(this);

		SetMasterPage(Master);
		SetPageTitle(TourResource.GetTitleForEditPage(TourResourceType.TooltipStyle));
		SetActionId(MemberPageActionId.EditTooltipStyle);
		GetSelectedTourOrNone();

		tooltipStyle = (TooltipStyle)CreateResourceFromQueryStringId(TourResourceType.TooltipStyle);

		if (tooltipStyle == null)
		{
			// This can happen if a user deletes a tooltip style and then uses the Back button to return to this screen.
			Response.Redirect(MemberPageAction.ActionPageTarget(MemberPageActionId.TooltipStyleExplorer));
		}
	}

	protected override void ReadPageFields()
	{
		tooltipStyle.Name = validName;
		tooltipStyle.TextColor = TextColorSwatch.ColorValue;
		tooltipStyle.BackgroundColor = BackgroundColorSwatch.ColorValue;
		tooltipStyle.LineColor = LineColorSwatch.ColorValue;
		tooltipStyle.BackgroundIsTransparent = TransparentBackgroundCheckBox.Checked;
		tooltipStyle.LineWidth = (int)SliderLineWidth.Value;
		tooltipStyle.Padding = (int)SliderPadding.Value;
		tooltipStyle.MaxWidth = (int)SliderMaxWidth.Value;
		tooltipStyle.FontStyleResource = Account.GetCachedFontStyle(FontStyleComboBox.SelectedResourceId);
	}

	protected override void ValidatePage()
	{
		ValidateResourceName(TooltipStyleNameTextBox, TooltipStyleNameError);
		if (!fieldValid)
			return;

		validTextColor = ValidateColorSwatch(TextColorSwatch);
		validBackgroundColor = ValidateColorSwatch(BackgroundColorSwatch);
		validLineColor = ValidateColorSwatch(LineColorSwatch);

		if (!pageValid)
			return;
	}

	protected override void ClearErrors()
	{
		ClearErrors(TooltipStyleNameError);
	}

	protected string FontStyleTable
	{
		get { return fontStyleTable; }
	}

	private void InitFontStyleTable()
	{
		bool firstRow = true;
		
		DataTable dataTable = MapsAliveDatabase.LoadDataTable("sp_FontStyle_GetAllFontsAvailableToAccount", "@AccountId", Utility.AccountId);
		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);

			if (!firstRow)
				fontStyleTable += ",";
			firstRow = false;
			
			fontStyleTable += string.Format("s{0}:", row.IntValue("FontStyleId"));
			fontStyleTable += "[";
			fontStyleTable += string.Format("\"{0}\",", row.StringValue("Family"));
			fontStyleTable += string.Format("{0},", row.IntValue("FontSizePx"));
			fontStyleTable += string.Format("{0},", row.BoolValue("Bold") ? 1 : 0);
			fontStyleTable += string.Format("{0},", row.BoolValue("Italic") ? 1 : 0);
			fontStyleTable += string.Format("{0}", row.BoolValue("Underline") ? 1 : 0);
			fontStyleTable += "]";
		}
	}
}
