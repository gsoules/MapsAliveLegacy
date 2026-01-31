// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class Public_CategoryExplorer : MemberPage
{
	private DataTable categoryDataTable;

	protected override void InitControls(bool undo)
	{
		if (account.ResourceIsFilteredBy(ResourceFilters.Category))
		{
			FilterWarningPanel.Visible = true;
			Utility.SetDivText(FilterWarningPanel, "Only showing categories used by this tour. Categories are sorted by directory position (first column).<br/>To see all categories, uncheck Filter Categories in the Library menu.");
		}
		
		CategorySelector.ResourceType = TourResourceType.Category;
		CategorySelector.IsFiltered = account.ResourceIsFilteredBy(ResourceFilters.Category);
		CategorySelector.DataTable = categoryDataTable;
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle(TourResource.GetTitleForExplorerPage(TourResourceType.Category));
		SetPageMessage(TourResource.GetExplorerMessage(TourResourceType.Category));
		SetPageReadOnly();
		SetActionId(MemberPageActionId.CategoryExplorer);
		GetSelectedTourOrNone();

		tour = MapsAliveState.SelectedTourOrNull;

		// Delete unused categories if requested.
		bool deleteUnused = Request.QueryString["delete"] == "1";
		if (deleteUnused)
		{
			DeleteUnusedResources(TourResourceType.Category);
			if (tour != null)
				tour.ReloadCategories();
		}

		categoryDataTable = Category.GetFilteredCategoryList(account.ResourceIsFilteredBy(ResourceFilters.Category), tour, Utility.AccountId);

		if (categoryFilterChanged)
		{
			// The user just toggled category filtering on or off. Flush the category table.
			tour.CategoryFilterChanged();
		}
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Resources;
	}
}
