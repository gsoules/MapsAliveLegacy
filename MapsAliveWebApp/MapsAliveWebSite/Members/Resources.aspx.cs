// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class Public_Resources : MemberPage
{
	protected override void PageLoad()
	{
		Account account = MapsAliveState.Account;

		MemberPageActionId lastActionIdForResources = account.LastActionIdForGroup(MemberPageActionId.Resources);
		
		int lastResourceId = 0;

		switch (lastActionIdForResources)
		{
			case MemberPageActionId.CategoryExplorer:
				lastResourceId = account.LastResourceId(TourResourceType.Category);
				if (lastResourceId != 0)
					lastActionIdForResources = MemberPageActionId.EditCategory;
				break;

			case MemberPageActionId.MarkerExplorer:
				lastResourceId = account.LastResourceId(TourResourceType.Marker);
				if (lastResourceId != 0)
					lastActionIdForResources = MemberPageActionId.EditMarker;
				break;

			case MemberPageActionId.SymbolExplorer:
				lastResourceId = account.LastResourceId(TourResourceType.Symbol);
				if (lastResourceId != 0)
					lastActionIdForResources = MemberPageActionId.EditSymbol;
				break;

			case MemberPageActionId.MarkerStyleExplorer:
				lastResourceId = account.LastResourceId(TourResourceType.MarkerStyle);
				if (lastResourceId != 0)
					lastActionIdForResources = MemberPageActionId.EditMarkerStyle;
				break;

			case MemberPageActionId.FontStyleExplorer:
				lastResourceId = account.LastResourceId(TourResourceType.FontStyle);
				if (lastResourceId != 0)
					lastActionIdForResources = MemberPageActionId.EditFontStyle;
				break;
			
			case MemberPageActionId.TooltipStyleExplorer:
				lastResourceId = account.LastResourceId(TourResourceType.TooltipStyle);
				if (lastResourceId != 0)
					lastActionIdForResources = MemberPageActionId.EditTooltipStyle;
				break;
			
			case MemberPageActionId.ColorSchemeExplorer:
				lastResourceId = account.LastResourceId(TourResourceType.TourStyle);
				if (lastResourceId != 0)
					lastActionIdForResources = MemberPageActionId.EditColorScheme;
				break;
		}

		string queryString = lastResourceId > 0 ? "?id=" + lastResourceId : String.Empty;

		if (lastResourceId == 0)
		{
			SetMasterPage(Master);
			SetPageTitle("");
			SetPageMessage("Choose Edit from the menu to choose the kind of resource you want to work with");
			SetPageReadOnly();
			SetActionId(MemberPageActionId.Resources);
			// ResourceLandingPageContent.Text = AppContent.Topic("ResourcesLandingPage");
		}
		else
		{
			Response.Redirect(MemberPageAction.ActionPageTarget(lastActionIdForResources) + queryString);
		}
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Resources;
	}
}
