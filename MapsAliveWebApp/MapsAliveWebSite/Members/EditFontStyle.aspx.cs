// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class Members_EditFontStyle : EditStylePage
{
	private string fontFamilyArray;
	
	protected override void EmitJavaScript()
	{
		string loadingScript = string.Empty;

		string loadedScript = string.Format("maShowPreview({0},{1},{2},{3},{4});",
			fontStyleResource.FontSizePx,
			fontStyleResource.FontFamilyId,
			fontStyleResource.Bold ? "true" : "false",
			fontStyleResource.Italic ? "true" : "false",
			fontStyleResource.Underline ? "true" : "false"
			);

		EmitJavaScript(loadingScript, loadedScript);
	}

	protected void AddChangeDetection(CheckBox checkBox, string script)
	{
		checkBox.Attributes.Add("onclick", script);
	}

	protected override void InitControls(bool undo)
	{
		FontSizeValue.Text = fontStyleResource.FontSizePx.ToString() + " px";
		
		InitFontFamilyList();
		MemberPage.InitShowUsageControl(ShowUsageControl, fontStyleResource);

		if (!undo && IsPostBack)
			return;

		FontStyleNameTextBox.Text = fontStyleResource.Name;
		AddChangeDetectionForPreview(FontStyleNameTextBox);

		FontSizeValue.Text = fontStyleResource.FontSizePx.ToString() + " px";
		SliderFontSize.Value = fontStyleResource.FontSizePx;

		ItalicCheckBox.Checked = fontStyleResource.Italic;
		AddChangeDetection(ItalicCheckBox, "maUpdatePreview(true);");
		
		BoldCheckBox.Checked = fontStyleResource.Bold;
		AddChangeDetection(BoldCheckBox, "maUpdatePreview(true);");

		UnderlineCheckBox.Checked = fontStyleResource.Underline;
		AddChangeDetection(UnderlineCheckBox, "maUpdatePreview(true);");
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle(TourResource.GetTitleForEditPage(TourResourceType.FontStyle));
		SetActionId(MemberPageActionId.EditFontStyle);
		GetSelectedTourOrNone();

		fontStyleResource = (FontStyleResource)CreateResourceFromQueryStringId(TourResourceType.FontStyle);

		if (fontStyleResource == null)
		{
			// This can happen if a user deletes a font style and then uses the Back button to return to this screen.
			Response.Redirect(MemberPageAction.ActionPageTarget(MemberPageActionId.FontStyleExplorer));
		}
	}

	protected override void ReadPageFields()
	{
		fontStyleResource.Name = validName;
		fontStyleResource.FontFamilyId = int.Parse(FontFamilyComboBox.SelectedValue);
		fontStyleResource.Bold = BoldCheckBox.Checked;
		fontStyleResource.Italic = ItalicCheckBox.Checked;
		fontStyleResource.Underline = UnderlineCheckBox.Checked;
		fontStyleResource.FontSizePx = (int)SliderFontSize.Value;
	}

	protected override void ValidatePage()
	{
		ValidateResourceName(FontStyleNameTextBox, FontStyleNameError);
		if (!fieldValid)
			return;
	}

	protected override void ClearErrors()
	{
		ClearErrors(FontStyleNameError);
	}

	private void AddFontFamilyToList(string name, int id)
	{
		RadComboBoxItem item = new RadComboBoxItem();
		item.Text = name;
		item.Value = id.ToString();
		if (fontStyleResource.FontFamilyId == id)
			item.Selected = true;
		FontFamilyComboBox.Items.Add(item);
	}

	protected string FontFamilyArray
	{
		get { return fontFamilyArray; }
	}

	private void InitFontFamilyList()
	{
		// Create the client-side family array on every page render since it's not preserved
		// in the view state. Only initialize the combo box once since it is preserved.

		DataTable dataTable = MapsAliveState.DataTableForFontFamily();
		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			
			if (!IsPostBack)
				AddFontFamilyToList(row.StringValue("Name"), row.IntValue("FontFamilyId"));
			
			// Put the font's family list in an array. The family list contains comma separated alternatives.
			if (fontFamilyArray != null)
				fontFamilyArray += ",";
			fontFamilyArray += string.Format("\"{0}\"", row.StringValue("Family"));
		}
	}
}
