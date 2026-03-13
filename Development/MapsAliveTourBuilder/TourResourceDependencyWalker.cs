// Copyright (C) 2003-2010 AvantLogic Corporation
using System.Data;
using System.Diagnostics;
using System.Text;

public class TourResourceDependencyWalker
{
	private StringBuilder stringBuilder;

	public TourResourceDependencyWalker()
	{
	}

	public void AppendToReport(string text)
	{
		if (stringBuilder == null)
			stringBuilder = new StringBuilder();
		stringBuilder.Append(text);
	}

	private DataTable GetDependentsOfResource(TourResource resource, string dependentName)
	{
		string sp = string.Format("sp_{0}_Get{0}sThatUse{1}", dependentName, resource.ResourceType);

		string idParameterName = string.Format("@{0}Id", resource.ResourceType);

		DataTable dataTable = MapsAliveDatabase.LoadDataTable(sp,
			"@AccountId", Utility.AccountId,
			idParameterName, resource.Id);

		return dataTable;
	}

	public void InvalidateDependentsOfResource(TourResource resource)
	{
		switch (resource.ResourceType)
		{
			case TourResourceType.Category:
				Category.InvalidateToursThatDependOnCategory(resource.Id);
				break;

			case TourResourceType.FontStyle:
				InvalidateTooltipsThatDependOnResource(resource);
				InvalidateMarkersThatDependOnResource(resource);
				break;

			case TourResourceType.Marker:
				InvalidateMapsThatDependOnResource(resource);
				break;

			case TourResourceType.MarkerStyle:
				InvalidateMarkersThatDependOnResource(resource);
				break;

			case TourResourceType.Symbol:
				InvalidateMarkersThatDependOnResource(resource);
				break;

			case TourResourceType.TourStyle:
				ColorScheme.InvalidateToursThatDependOnColorScheme(resource.Id);
				break;

			case TourResourceType.TooltipStyle:
				TooltipStyle.InvalidateMapsThatDependOnTooltipStyle(resource.Id);
				break;

			default:
				AppendToReport("Unsupported resource type " + resource.ResourceType);
				break;
		}
	}

	private void InvalidateMarkersThatDependOnResource(TourResource resource)
	{
		string sp = string.Format("sp_Marker_GetMarkerIdsBy{0}Id", resource.ResourceType);
		string idParameterName = string.Format("@{0}Id", resource.ResourceType);

		// Find every marker that depends on the parent resource.
		DataTable dataTable = MapsAliveDatabase.LoadDataTable(sp, idParameterName, resource.Id);
		foreach (DataRow dataRow in dataTable.Rows)
		{
			// Delete the marker from the cache.
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			int markerId = row.IntValue("MarkerId");
			Account.DeleteCachedResource(TourResourceType.Marker, markerId);

			// Update the marker's resource image.
			bool isExclusiveMarker = row.IntValue("TourViewId") != 0;
			if (!isExclusiveMarker)
			{
				string resourceImageId = row.StringValue("ResourceImageId");
				TourResource.CreateResourceImageFile(TourResourceType.Marker, markerId, resourceImageId, ResourceImageFileAction.UpdateExistingFile);
			}

			Utility.Trace(string.Format("InvalidateMarkersThatDependOnResource {0} : {1}", resource.ResourceType, markerId));
		}
	
		// Rebuild the pages that use the markers that have just been invalidated.
		InvalidateMapsThatDependOnResource(resource);
	}

	private void InvalidateMapsThatDependOnResource(TourResource resource)
	{
		// Find every map that uses the resource and mark it for rebuilding.
		string sp = string.Format("sp_TourPage_GetTourPageIdsBy{0}Id", resource.ResourceType);
		string idParameterName = string.Format("@{0}Id", resource.ResourceType);
		DataTable dataTable = MapsAliveDatabase.LoadDataTable(sp, idParameterName, resource.Id);
		TourPage.RebuildMap(dataTable);
	}

	private void InvalidateTooltipsThatDependOnResource(TourResource resource)
	{
		DataTable dataTable = GetDependentsOfResource(resource, TourResourceType.TooltipStyle.ToString());
		foreach (DataRow dataRow in dataTable.Rows)
		{
			// Delete the tooltip from the cache.
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			int tooltipStyleId = row.IntValue("TooltipStyleId");
			Account.DeleteCachedResource(TourResourceType.TooltipStyle, tooltipStyleId);

			// Update the tooltip's resource image.
			string resourceImageId = row.StringValue("ResourceImageId");
			TourResource.CreateResourceImageFile(TourResourceType.TooltipStyle, tooltipStyleId, resourceImageId, ResourceImageFileAction.UpdateExistingFile);

			// Update the maps that use the toolip.
			TooltipStyle.InvalidateMapsThatDependOnTooltipStyle(tooltipStyleId);

			Utility.Trace(string.Format("InvalidateTooltipsThatDependOnResource {0} : {1}", resource.ResourceType, tooltipStyleId));
		}
	}

	public string Report
	{
		get	{ return stringBuilder == null ? string.Empty : stringBuilder.ToString(); }
	}

	public void ReportDependentsOfResource(TourResource resource)
	{
		switch (resource.ResourceType)
		{
			case TourResourceType.Category:
				ReportTourViewsDependentOnResource(resource);
				break;

			case TourResourceType.FontStyle:
				ReportDependentOfResources(resource, TourResourceType.TooltipStyle, TourResourceType.TooltipStyle.ToString());
				ReportDependentOfResources(resource, TourResourceType.Marker, TourResourceType.Marker.ToString());
				break;

			case TourResourceType.Marker:
				ReportTourViewsDependentOnResource(resource);
				break;

			case TourResourceType.MarkerStyle:
				ReportDependentOfResources(resource, TourResourceType.Marker, "NonExclusiveMarker");
				ReportTourViewsDependentOnResource(resource);
				break;

			case TourResourceType.Symbol:
				ReportDependentOfResources(resource, TourResourceType.Marker, TourResourceType.Marker.ToString());
				break;

			case TourResourceType.TourStyle:
				ReportToursDependentOnResource(resource);
				break;

			case TourResourceType.TooltipStyle:
				ReportTourPagesDependentOnResource(resource);
				break;

			default:
				AppendToReport("Unsupported resource type " + resource.ResourceType);
				break;
		}
	}

	private void ReportDependentOfResources(TourResource resource, TourResourceType dependentResourceType, string dependentName)
	{
		DataTable dataTable = GetDependentsOfResource(resource, dependentName);

		if (dataTable.Rows.Count > 0)
		{
			if (resource.ResourceType == TourResourceType.FontStyle || resource.ResourceType == TourResourceType.MarkerStyle)
			{
				// Font and marker styles have two dependents so emit a section title for each.
				WriteReportSection(TourResourceManager.GetTitlePlural(dependentResourceType));
			}

			AppendToReport("<table class='dependencyReportTable'>");
		}

		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			TourResource dependent = TourResourceManager.CreateNewResource(dependentResourceType, row.IntValue(dependentResourceType + "Id"));
			AppendToReport(string.Format("<tr onclick=\"{0}\">", TourResourceManager.ClickScript(dependent.ResourceType, dependent.Id)));
			WriteReportItem(dependent);
			AppendToReport("</tr>");
		}

		if (dataTable.Rows.Count > 0)
		{
			AppendToReport("</table>");
		}
	}

	private void ReportTourPagesDependentOnResource(TourResource resource)
	{
		DataTable dataTable = GetDependentsOfResource(resource, "TourPage");

		string currentTourName = string.Empty;
		string currentTourPageName = string.Empty;

		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			string tourPageName = row.StringValue("TourPageName");
			string tourName = row.StringValue("TourName");

			if (tourName != currentTourName)
			{
				if (currentTourName != string.Empty)
					AppendToReport(string.Format("<div class='{0}'>&nbsp;</div>", "tourDependentLevel1Spacer"));
				currentTourName = tourName;
				AppendToReport(string.Format("<div class='{0}'>{1}</div>", "tourDependentLevel1", tourName));
				currentTourPageName = string.Empty;
			}

			if (tourPageName != currentTourPageName)
			{
				currentTourPageName = tourPageName;
				AppendToReport(string.Format("<div class='{0}'>{1}</div>", "tourDependentLevel2Page", tourPageName));
			}
		}
	}

	private void ReportToursDependentOnResource(TourResource resource)
	{
		DataTable dataTable = GetDependentsOfResource(resource, "Tour");
		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			Tour tour = Tour.GetSelectedTourOrCreateFromDatabase(row.IntValue("TourId"));
			AppendToReport(string.Format("<div class='{0}'>{1}</div>", "tourDependentTour", tour.Name));
		}
	}

	private void ReportTourViewsDependentOnResource(TourResource resource)
	{
		DataTable dataTable = GetDependentsOfResource(resource, "TourView");

		if (resource.ResourceType == TourResourceType.MarkerStyle && dataTable.Rows.Count > 0)
		{
			// EditHotspotActions styles have two dependents (non-exclusive and exclusive markers) so emit a section title for each.
			WriteReportSection("Hotspots that use exclusive markers");
		}

		string currentTourName = string.Empty;
		string currentTourPageName = string.Empty;

		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			int tourViewId =  row.IntValue("TourViewId");
			string tourViewTitle = row.StringValue("TourViewTitle");
			string tourPageName = row.StringValue("TourPageName");
			string tourName = row.StringValue("TourName");

			if (tourName != currentTourName)
			{
				if (currentTourName != string.Empty)
					AppendToReport(string.Format("<div class='{0}'>&nbsp;</div>", "tourDependentLevel1Spacer"));
				currentTourName = tourName;
				AppendToReport(string.Format("<div class='{0}'>{1}</div>", "tourDependentLevel1", tourName));
				currentTourPageName = string.Empty;
			}

			if (tourPageName != currentTourPageName)
			{
				currentTourPageName = tourPageName;
				AppendToReport(string.Format("<div class='{0}'>{1}</div>", "tourDependentLevel2", tourPageName));
			}
			
			AppendToReport(string.Format("<div class='{0}'>{1}</div>", "tourDependentLevel3", tourViewTitle));
		}
	}

	private void WriteReportItem(TourResource resource)
	{
		string className = "resourceUsageReportItem";
		string mouseOverScript = string.Format("this.className='{0}Hover'", className);
		string mouseOutScript = string.Format("this.className='{0}'", className);
		AppendToReport(string.Format("<td><img class=\"{0}\"/ src=\"{1}\"/></td>", "resourceUsageReportItemImage", resource.Url));

		AppendToReport(string.Format("<td class=\"{0}\" onmouseover=\"{2}\" onmouseout=\"{3}\">{1}</div>",
			className,
			resource.Name,
			mouseOverScript,
			mouseOutScript));
	}

	private void WriteReportSection(string text)
	{
		string className = "optionsSectionTitle";
		if (Report.Length == 0)
			className += "First";
		AppendToReport(string.Format("<div class='{0}'>{1}</div>", className, text));
	}
}
