// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web;
using System.Web.SessionState;

public class AddTourView : IHttpHandler, IRequiresSessionState
{
	public void ProcessRequest(HttpContext context)
	{
		Tour tour = MapsAliveState.SelectedTour;

		int selectedTourViewId = 0;
		bool duplicate = context.Request.QueryString["dup"] == "1";
		if (duplicate && tour != null)
		{
			// Remember which hotspot is currently selected so we know which one to duplicate.
			selectedTourViewId = tour.SelectedTourView.Id;
		}
		
		bool added = false;
		bool isDataSheet = context.Request.QueryString["ds"] == "1";
        bool isNewTour = context.Request.QueryString["newtour"] == "1";

        // Determine if it's okay to add a new hotspot. It's always okay when the account hotspot
        // limit has not been reached, but also okay to add to a tour that already exceeds the limit.
        // When the account is at or over its limit, we transfer to the HotspotLimitReached screen
        // and let the user decide if they want to go over the limit. If they do, that screen
        // sends control back here passing ok=1 on the query string.
        bool okToAdd = false;
		HotspotLimitStatus status = MapsAliveState.Account.HotspotLimitStatus;

		if (isDataSheet)
		{
			okToAdd = true;
		}
		else
		{
			if (status == HotspotLimitStatus.AtLimit || (status == HotspotLimitStatus.OverLimit && tour.ExceedsSlideLimit))
			{
				if (context.Request.QueryString["ok"] == "1")
					okToAdd = true;
			}
			else
			{
				okToAdd = status == HotspotLimitStatus.UnderLimit;
			}
		}

		if (okToAdd && (MapsAliveState.PostIsValid(context) || isNewTour))
		{
			TourView tourView;

			if (isDataSheet)
			{
				bool isGallery = false;
				TourPage tourPage = tour.CreateNewTourPage(isGallery, isDataSheet, null, false);
				tour.AddTourPage(tourPage, true);
				tourView = tour.CreateNewTourViewForDataSheet(tourPage);
			}
			else
			{
				tourView = tour.CreateNewTourView();
			}
			
			tour.AddTourView(tourView);
			
			added = true;
			
			if (duplicate)
			{
				// Remember some of the data this is going to get clobbered by cloning.
				string hotspotId = tourView.SlideId;
				string title = tourView.Title;
				double pctX = tourView.MarkerPctX;
				double pctY = tourView.MarkerPctY;

				// Read from the database the data for the hotspot being cloned, but assign it to the new hotspot.
				tourView.CloneTourView(selectedTourViewId);
				
				// Restore the fields of this new hotspot that are unique to it.
				tourView.SlideId = hotspotId;
				tourView.Title = title;
				tourView.MarkerPctX = pctX;
				tourView.MarkerPctY = pctY;

				tourView.UpdateDatabase();
			}
		}

		string targetPage;
		if (okToAdd)
		{
			targetPage = string.Format(MemberPageAction.ActionPageTarget(MemberPageActionId.EditHotspotContent) + "?new={0}", added ? "1" : "0");
		}
		else
		{
			targetPage = MemberPageAction.ActionPageTarget(MemberPageActionId.HotspotLimitReached);
		}
		
		context.Server.Execute(targetPage, false);
	}

	public bool IsReusable
	{
		get { return true; }
	}
}