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

public partial class Public_MarkerExplorer : MemberPage
{
	private DataTable accountMarkerDataTable;

	protected override void InitControls(bool undo)
	{
		if (account.ResourceIsFilteredBy(ResourceFilters.Marker))
		{
			FilterWarningPanel.Visible = true;
			Utility.SetDivText(FilterWarningPanel, "Only showing markers used by this tour. To see all, uncheck Filter Markers in the Library menu.");
		}

		AccountMarkerSelector.ResourceType = TourResourceType.Marker;
		AccountMarkerSelector.IsFiltered = account.ResourceIsFilteredBy(ResourceFilters.Marker);

		string tourName = tour != null ? tour.Name : string.Empty;
		string pageName = tourPage != null ? tourPage.Name : string.Empty;

		// Get the list of non-exclusive markers.
		AccountMarkerSelector.DataTable = accountMarkerDataTable;
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle(TourResource.GetTitleForExplorerPage(TourResourceType.Marker));
		SetPageMessage(TourResource.GetExplorerMessage(TourResourceType.Marker));
		SetPageReadOnly();
		SetActionId(MemberPageActionId.MarkerExplorer);
		GetSelectedTourOrNone();

		DeleteUnusedResources(TourResourceType.Marker);

		if (tour != null && account.ResourceIsFilteredBy(ResourceFilters.Marker))
			accountMarkerDataTable = Marker.GetFilteredMarkerList(MarkerFilter.Exclusive, tour, tourPage, null, Utility.AccountId);
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Resources;
	}
}
