// Copyright (C) 2008 AvantLogic Corporation
using System;

public partial class Public_TourResourceDependencyReport : MemberPage
{
	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageReadOnly();
		SetActionId(MemberPageActionId.TourResourceDependencyReport);
		GetSelectedTourOrNone();

		int rt = 0;
		int.TryParse(Request.QueryString["rt"], out rt);
		TourResourceType resourceType = (TourResourceType)rt;

		int id = 0;
		int.TryParse(Request.QueryString["id"], out id);

		TourResource resource = null;

		string usedBy = string.Empty;

		switch (resourceType)
		{
			case TourResourceType.Category:
				resource = Account.GetCachedCategory(id);
				usedBy = "hotspots";
				break;
			
			case TourResourceType.FontStyle:
				resource = Account.GetCachedFontStyle(id);
				usedBy = "tooltip styles and markers";
				break;
			
			case TourResourceType.Marker:
				resource = Account.GetCachedMarker(id);
				usedBy = "hotspots";
				break;
			
			case TourResourceType.MarkerStyle:
				usedBy = "markers";
				resource = Account.GetCachedMarkerStyle(id);
				break;
			
			case TourResourceType.Symbol:
				usedBy = "markers";
				resource = Account.GetCachedSymbol(id);
				break;
			
			case TourResourceType.TourStyle:
				usedBy = "tours";
				resource = Account.GetCachedColorScheme(id);
				break;
			
			case TourResourceType.TooltipStyle:
				usedBy = "maps";
				resource = Account.GetCachedTooltipStyle(id);
				break;
			
			case TourResourceType.Undefined:
				default:
				break;
		}


		string report = string.Empty;
		if (resource == null)
		{
			SetPageTitle("Usage Report : not available");
		}
		else
		{
			TourResourceDependencyWalker walker = new TourResourceDependencyWalker();
			walker.ReportDependentsOfResource(resource);

			report = walker.Report;

			string name = resourceType == TourResourceType.Category ? ((Category)resource).Title : resource.Name;
			string resourceTitle =  TourResourceManager.GetTitle(resource.ResourceType);
			SetPageTitle(string.Format("{0} Usage Report", resourceTitle));

			string nameLink = string.Format("<a href=\"javascript:{1}\">{0}</a>", name, TourResourceManager.ClickScript(resourceType, resource.Id));

			if (report.Length == 0)
				ReportTitle.Text = nameLink + " is not being used anywhere";
			else
				ReportTitle.Text = string.Format("{0} {1} is being used by the {2} listed below", resourceTitle, nameLink, usedBy);
		}

		ReportData.Text = report;
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Resources;
	}
}
