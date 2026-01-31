// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class Members_DeleteHotspots : MemberPage
{
	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle("Delete Hotspots");
		SetActionIdForPageAction(MemberPageActionId.DeleteHotspots);
		GetSelectedTourPage();
		SetPageReadOnly();
		SetPageMessage("Check the hotspots you want to delete and then press the Delete button");

		if (IsPostBack)
		{
            string eventTarget = Request.Form["__EVENTTARGET"];
            if (eventTarget == "EventOnDelete")
            {
                OnDelete();
            }
            else
            {
                // Restore the items that were checked.
                DeleteList.SetChecked(DeletionList.Value);
			
			    // Remember the checked items so that they can be restored after returning from Tour Preview.
			    MapsAliveState.Persist(MapsAliveObjectType.CheckList, DeletionList.Value);
            }
		}
		else
		{
			if (IsReturnToTourBuilder)
			{
				// Restore the items that were checked before going to Tour Preview or the Home page.
				object list = MapsAliveState.Retrieve(MapsAliveObjectType.CheckList);
				if (list != null)
				{
					DeleteList.SetChecked((string)list);
				}
			}
			else
			{
				// This is a new post. Make sure there's no list leftover from an earlier visit to this screen.
				MapsAliveState.Flush(MapsAliveObjectType.CheckList);
			}

			// Create a list of hotspots that can be checked-off.
			int count = InitReplaceList();
			if (count == 0)
			{
				Utility.SetDivText(NoListPanel, string.Format("{0} does not have any hotspots.", tourPage.Name));
				ButtonDelete.Enabled = false;
				CheckAllControl.Visible = false;
				UncheckAllControl.Visible = false;
			}
		}

		// Initialize the check-all and uncheck-all controls.
		CheckAllControl.OnClickJavaScript = "maCheckAll(true);";
		UncheckAllControl.OnClickJavaScript = "maCheckAll(false);";

		// Add a handler to the Delete button to execute JavaScript that will examine the checkboxes.
		ButtonDelete.OnClientClick = "return maOnDelete();";
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}

	private int InitReplaceList()
	{
		// Create a checkbox list of non-exclusive markers on this tour page. We don't allow the
		// user to non-exclusive markers because it seems like it would be too error prone and
		// confusing as a bulk operation. They can change them individually on the Hotspot Content screen.

		int count = 0;
		foreach (TourView tourView in tourPage.TourViews)
		{
			if (tourView.MarkerIsRoute)
				continue;
			Marker marker = Account.GetCachedMarker(tourView.MarkerId);
			count++;
			DeleteList.AddListItem(tourView.Title, marker.Name, marker.Url, tourView.Id);
		}

		return count;
	}

	protected void OnDelete()
	{
		// The user clicked the Delete button.
		// Get the list of hotspots to be deleted. If it's empty, the user didn't check any.
		string value = DeletionList.Value;
		if (value.Length > 0)
		{
			// Split the comma-separated list into an array of marker Ids.
			string[] deletions = value.Split(',');

			// Update the each marker for each hotspot that the user checked.
			foreach (string checkedTourViewId in deletions)
			{
				TourView checkedTourView = tourPage.GetTourView(int.Parse(checkedTourViewId));
				if (checkedTourView == null)
				{
					// This can happen if the user refreshes the page after doing a delete.
					// We could try to detect that case, but this is safe.
					break;
				}
				checkedTourView.Delete();
			}
		}

		// Update the list so that it no longer shows the hotspots that were deleted.
		InitReplaceList();
		
		// Force the tour navigator to get refreshed so that it will no longer display the
		// hotspots that were deleted. Normally this happens as part of the normal page
		// loaded cycle, but this handler is called after the page load has completed.
		// Thus, once the delete is complete, we have to refresh the navigator.
		masterPage.InitNavigation(this);
	}
}
