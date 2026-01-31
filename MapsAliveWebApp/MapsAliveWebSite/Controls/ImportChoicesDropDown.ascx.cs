// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.UI.WebControls;

public partial class Controls_ImportChoicesDropDown : System.Web.UI.UserControl
{
	public void SetSelectedValue(ImportType importSlidesType)
	{
		if (DropDownList.Items.Count == 0)
		{
			AddListItem("Photos", ImportType.SlideImages);
			AddListItem("Slide Content", ImportType.SlideContent);
			AddListItem("HTML Image Map", ImportType.Markers);
		}

		int id = (int)importSlidesType;
		DropDownList.SelectedValue = id.ToString();
	}

	private void AddListItem(string text, ImportType importSlidesType)
	{
		int id = (int)importSlidesType;
		ListItem item = new ListItem(text, id.ToString());
		DropDownList.Items.Add(item);
	}
}
