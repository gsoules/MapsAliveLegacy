// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class Public_StyleExplorer : MemberPage
{
	// This page serves as the common Style Explorer page for all style-type resources.

	private TourResourceType resourceType;
	private string resourceTitle;

	protected override void InitControls(bool undo)
	{
		StyleSelector.ResourceType = resourceType;
	}

	protected override void PageLoad()
	{
		int rt = 0;
		int.TryParse(Request.QueryString["rt"], out rt);
		resourceType = (TourResourceType)rt;

		if (resourceType == TourResourceType.Undefined)
		{
			// This should never happen unless someone edited the URL.
			Response.Redirect(MemberPageAction.ActionPageTarget(MemberPageActionId.Resources));
		}

		resourceTitle = TourResourceManager.GetTitle(resourceType);

		SetMasterPage(Master);
		SetPageTitle(string.Format("{0} Library", resourceTitle));
		SetPageMessage(TourResource.GetExplorerMessage(resourceType));
		SetPageReadOnly();
		SetActionId(TourResourceManager.GetExplorerActionId(resourceType));
		GetSelectedTourOrNone();

		DeleteUnusedResources(resourceType);
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Resources;
	}
}
