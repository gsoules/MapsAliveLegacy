// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web;
using System.Web.SessionState;

public class ManageTour : IHttpHandler, IRequiresSessionState
{
	public void ProcessRequest(HttpContext context)
	{
		Tour tour = null;
		
		int tourId = 0;
		int.TryParse(context.Request.QueryString["id"], out tourId);

		if (tourId != 0)
		{
			// Get the tour for the Id.  If the tour comes back with an Id of 0, something
			// is wrong, for example, someone monkeyed with the URL and set the Id for a tour
			// that the logged in user does not own, or the tour was deleted in another browser.
			tour = new Tour(tourId);
			if (tour.Id == 0)
				tour = null;
		}
		
		// If there's no tour, go back to the Tour Explorer.
		if (tour == null)
			context.Response.Redirect("~/Members/TourExplorer.aspx");

		// Cache the reference to the tour so that other pages can access it.
		MapsAliveState.SetSelectedTour(tour);

		// Automatically set the tour's first page and first view.
		if (tour.FirstPageId != 0)
		{
			TourPage firstPage = tour.SetSelectedTourPage(tour.FirstPageId);
			if (firstPage != null && firstPage.FirstTourViewId != 0)
			{
				tour.SetSelectedTourView(firstPage.FirstTourViewId);
			}
		}
		
		// Go to the appropriate starting page for the tour.
		string targetPath = MemberPageAction.ActionPageTargetPath(tour.Advisor.DefaultActionId);
		context.Server.Execute(targetPath);
	}

	public bool IsReusable
	{
		get { return true; }
	}
}