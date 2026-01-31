// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web;
using System.Web.SessionState;

public class TourViewEditor : IHttpHandler, IRequiresSessionState
{
	private string targetAspxPage;
	private Tour tour;
	private TourView tourView;

	// This handler is invoked when the user clicks a hotspot name in the tour navigator.  
	public void ProcessRequest(HttpContext context)
	{
		GetTourView(context);
		ChooseTargetAspxPage(context);

		if (targetAspxPage == null)
			context.Response.Redirect(MemberPageAction.ActionPageTarget(MemberPageActionId.TourManager));
		else
			context.Server.Execute(targetAspxPage, false);
	}

	private void ChooseTargetAspxPage(HttpContext context)
	{
		if (tourView == null)
			return;

		// Determine which action to perform on this tour-view and get the ASPX page that does it.
		// We do this by converting the specified action Id number to an action Id enum. If the
		// convert fails, its a bogus action, probably because someone edited the query string.
		int aid = 0;
		int.TryParse(context.Request.QueryString["aid"], out aid);
		MemberPageActionId actionId;
		try
		{
			actionId = (MemberPageActionId)aid;

			// No exception occurred, so the action is good, but lets make sure its a slide action.
			if (MemberPageAction.IsHotspotAction(actionId))
			{
				if (actionId == MemberPageActionId.EditHotspotActions && tourView.TourPage.IsDataSheet)
				{
					// The action is for a non-info page's marker.  Since info pages don't have
					// markers, change the action to show the page's slide content.
					actionId = MemberPageActionId.EditHotspotContent;
				}
				else if (actionId == MemberPageActionId.HotspotLimitReached)
				{
					// If the user clicks a slide in the tour navigator while on the
					// HotspotLimitReached screen, take them to the edit content page.
					actionId = MemberPageActionId.EditHotspotContent;
				}
				
				targetAspxPage = MemberPageAction.ActionPageTarget(actionId);

				Utility.LastViweAction = actionId;
			}
		}
		catch
		{
		}
	}

	private string GetTourView(HttpContext context)
	{
		tour = MapsAliveState.SelectedTour;
		string targetAspxPage = null;

		int viewId = 0;
		int.TryParse(context.Request.QueryString["vid"], out viewId);

		// Make the clicked slide become the tour's selected view.  If null is
		// returned, viewId does not match a tour-view in the selected tour.
		tourView = tour.SetSelectedTourView(viewId);
		return targetAspxPage;
	}

	public bool IsReusable
	{
		get { return true; }
	}
}