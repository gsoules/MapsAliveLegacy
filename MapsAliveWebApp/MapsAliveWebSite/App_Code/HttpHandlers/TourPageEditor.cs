// Copyright (C) 2003-2010 AvantLogic Corporation
using System.Web;
using System.Web.SessionState;

public class TourPageEditor : IHttpHandler, IRequiresSessionState
{
	private string targetAspxPage;
	private Tour tour;
	private TourPage tourPage;
	
	// This handler is invoked when the user clicks a map or data sheet name in the tour navigator.  
	public void ProcessRequest(HttpContext context)
	{
		GetTourPage(context);
		ChooseTargetAspxPage(context);
		
		if (targetAspxPage == null)
			Utility.TransferToHomePage();
		else
			context.Server.Execute(targetAspxPage, false);
	}

	private void GetTourPage(HttpContext context)
	{
		int tourId = 0;
		int.TryParse(context.Request.QueryString["tid"], out tourId);
		if (tourId > 0)
		{
			tour = new Tour(tourId);
			MapsAliveState.SetSelectedTour(tour);
		}

		tour = MapsAliveState.SelectedTour;

		int pageId = 0;
		int.TryParse(context.Request.QueryString["pid"], out pageId);
		
		// Make the clicked tour-page become the tour's selected page.  If null is
		// returned, pageId does not match a tour-page in the selected tour.
		tourPage = tour.SetSelectedTourPage(pageId);
	}

	private void ChooseTargetAspxPage(HttpContext context)
	{
		if (tourPage == null)
			return;

		if (tourPage.IsDataSheet)
		{
			// Make the data sheet's sole slide become the selected view.
			tour.SetSelectedTourView(tourPage.FirstTourViewId);
		}

		// Get the integer action Id for the target page.
		MemberPageActionId actionId;
		int aid = 0;
		int.TryParse(context.Request.QueryString["aid"], out aid);

		try
		{
			// Determine which action to perform on this tour-page and get the ASPX page that does it.
			// We do this by converting the specified action Id number to an action Id enum. If the
			// convert fails, its a bogus action, probably because someone edited the query string.
			actionId = (MemberPageActionId)aid;

			if (tourPage.IsDataSheet)
			{
				// When the user clicks a data sheet in the tour navigator, we need to choose the best
				// action based on recent events. We either want to go to the Data Sheet Content screen
				// or stay on a page screen that applies to a Data Sheet. For example, if the user is
				// on the Banner Image screen and wants to see what each map and data sheet looks like
				// by clicking in the tour navigator, they can do that because the banner image screen is
				// a data sheet action. But if they have been editing hot spots and then click on a data
				// sheet, we assume that they want to edit the data sheet. This special logic is needed
				// because a data sheet is treated like a page sometimes and like a slide at others.
				if (Utility.LastAction == MemberPageActionId.HotspotOptionsAdvanced)
					actionId = MemberPageActionId.HotspotOptionsAdvanced;
				else if (!MemberPageAction.IsDataSheetAction(actionId) || Utility.LastAction == MemberPageActionId.EditHotspotContent)
					actionId = MemberPageActionId.EditHotspotContent;
			}
			else
			{
				if (!tourPage.SlidesPopup && MemberPageAction.IsPopupAction(actionId))
				{
					actionId = MemberPageActionId.Map;
				}
				else
				{
					if (actionId == MemberPageActionId.MapSetupNew)
						actionId = MemberPageActionId.MapSetup;
					else if (actionId == MemberPageActionId.GallerySetupNew)
						actionId = MemberPageActionId.GallerySetup;
					else if (actionId == MemberPageActionId.EditHotspotContent || actionId == MemberPageActionId.LastReport)
						actionId = MemberPageActionId.Map;
				}
			}

			if (tourPage.IsGallery)
			{
				if (actionId == MemberPageActionId.Map)
					actionId = MemberPageActionId.Gallery;
				else if (actionId == MemberPageActionId.MapSetup)
					actionId = MemberPageActionId.GallerySetup;
			}
			else
			{
				if (actionId == MemberPageActionId.Gallery)
					actionId = MemberPageActionId.Map;
				if (actionId == MemberPageActionId.GallerySetup)
					actionId = MemberPageActionId.MapSetup;
			}

			targetAspxPage = MemberPageAction.ActionPageTarget(actionId);
			Utility.LastPageAction = actionId;
		}
		catch
		{
		}
	}

	public bool IsReusable
	{
		get { return true; }
	}
}