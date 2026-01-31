// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class Members_ReplaceMarkers : MemberPage
{
	private int replacementMarkerId;

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle("Replace Markers");
		SetActionIdForPageAction(MemberPageActionId.ReplaceMarkers);
		GetSelectedTourPage();
		SetPageReadOnly();

		if (IsPostBack)
		{
			// Save the Id of the marker the user selected before we initialize the control again.
			replacementMarkerId = MarkerComboBox.SelectedResourceId;
			account.SetLastResourceId(TourResourceType.Marker, replacementMarkerId);
			
			// Restore the items that were checked.
			ReplaceList.SetChecked(ReplacementList.Value);
			
			// Remember the checked items so that they can be restored after resturning from Tour Preview.
			MapsAliveState.Persist(MapsAliveObjectType.CheckList, ReplacementList.Value);
		}
		else
		{
			if (IsReturnToTourBuilder)
			{
				// Restore the items that were checked before going to Tour Preview or the Home page.
				object list = MapsAliveState.Retrieve(MapsAliveObjectType.CheckList);
				if (list != null)
				{
					ReplaceList.SetChecked((string)list);
				}

				replacementMarkerId = account.LastResourceId(TourResourceType.Marker);
				if (replacementMarkerId == 0)
				{
					// This can happen if the session timed out before returning to the Tour Builder
					// or if the user left this screen before doing a replace.
					replacementMarkerId = account.DefaultResourceId(TourResourceType.Marker);
				}
			}
			else
			{
				// This is a new post. Make sure there's no list leftover from an earlier visit to this screen.
				MapsAliveState.Flush(MapsAliveObjectType.CheckList);

				// Get the default marker style Id to show as selected in the dropdown list.
				replacementMarkerId = account.DefaultResourceId(TourResourceType.Marker);
			}

			// Create a list of hotspots that can be checked-off.
			int count = InitReplaceList();
			if (count == 0)
			{
				Utility.SetDivText(NoListPanel, string.Format("{0} does not have any markers that can be replaced.", tourPage.Name));
				ButtonReplace.Enabled = false;
				CheckAllControl.Visible = false;
				UncheckAllControl.Visible = false;
			}
		}

		// Create the dropdown list of marker styles that the user can choose from as the replacement style.
		MarkerComboBox.ResourceType = TourResourceType.Marker;
		MarkerComboBox.SelectedResourceId = replacementMarkerId;

		// Initialize the check-all and uncheck-all controls.
		CheckAllControl.OnClickJavaScript = "maCheckAll(true);";
		UncheckAllControl.OnClickJavaScript = "maCheckAll(false);";

		// Add a handler to the Replace button to execute JavaScript that will examine the checkboxes.
		ButtonReplace.OnClientClick = "maOnReplace();return true;";
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
			if (marker.IsExclusive)
				continue;

			count++;

			ReplaceList.AddListItem(tourView.Title, marker.Name, marker.Url, tourView.Id);
		}

		return count;
	}

	protected void OnReplace(object sender, EventArgs e)
	{
		// The user clicked the Replace button.
		// Get the list of hotspots to be updated. If it's empty, the user didn't check any.
		string value = ReplacementList.Value;
		if (value.Length > 0)
		{
			// Split the comma-separated list into an array of marker Ids.
			string[] replacements = value.Split(',');

			// Update the each marker for each hotspot that the user checked.
			foreach (string checkedTourViewId in replacements)
			{
				TourView checkedTourView = tourPage.GetTourView(int.Parse(checkedTourViewId));
				checkedTourView.MarkerId = replacementMarkerId;
				checkedTourView.UpdateDatabase();
			}

			tourPage.RebuildMap();
		}

		// Update the list so that it shows the replacements that were just made.
		InitReplaceList();
	}
}
