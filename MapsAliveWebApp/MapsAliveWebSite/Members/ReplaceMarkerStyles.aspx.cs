// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class Members_ReplaceMarkerStyles : MemberPage
{
	private int replacementMarkerStyleId;

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle("Replace Marker Styles");
		SetActionIdForPageAction(MemberPageActionId.ReplaceMarkerStyles);
		GetSelectedTourPage();
		SetPageReadOnly();

		if (IsPostBack)
		{
			// Save the Id of the marker style the user selected before we initialize the control again.
			replacementMarkerStyleId = MarkerStyleComboBox.SelectedResourceId;
			account.SetLastResourceId(TourResourceType.MarkerStyle, replacementMarkerStyleId);
			
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

				replacementMarkerStyleId = account.LastResourceId(TourResourceType.MarkerStyle);
				if (replacementMarkerStyleId == 0)
				{
					// This can happen if the session timed out before returning to the Tour Builder
					// or if the user left this screen before doing a replace.
					replacementMarkerStyleId = account.DefaultResourceId(TourResourceType.MarkerStyle);
				}
			}
			else
			{
				// This is a new post. Make sure there's no list leftover from an earlier visit to this screen.
				MapsAliveState.Flush(MapsAliveObjectType.CheckList);

				// Get the default marker style Id to show as selected in the dropdown list.
				replacementMarkerStyleId = account.DefaultResourceId(TourResourceType.MarkerStyle);
			}
			
			// Create a list of hotspots that can be checked-off.
			int count = InitReplaceList();
			if (count == 0)
			{
				Utility.SetDivText(NoListPanel, string.Format("{0} has no markers that use marker styles.", tourPage.Name));
				ButtonReplace.Enabled = false;
				CheckAllControl.Visible = false;
				UncheckAllControl.Visible = false;
			}
		}

		// Create the dropdown list of marker styles that the user can choose from as the replacement style.
		MarkerStyleComboBox.ResourceType = TourResourceType.MarkerStyle;
		MarkerStyleComboBox.SelectedResourceId = replacementMarkerStyleId;

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
		// Create a checkbox list of markers on this map that use marker styles.
		int count = 0;
		foreach (TourView tourView in tourPage.TourViews)
		{
			if (tourView.MarkerIsRoute)
				continue;

			Marker marker = Account.GetCachedMarker(tourView.MarkerId);
			if (marker.MarkerType == MarkerType.Symbol)
				continue;

			count++;

			ReplaceList.AddListItem(tourView.Title, marker.MarkerStyle.Name, marker.MarkerStyle.Url, marker.Id);
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

			// Get the marker style the user wants to switch to.
			MarkerStyle replacementMarkerStyle = Account.GetCachedMarkerStyle(replacementMarkerStyleId);

			// Update each marker that the user checked.
			foreach (string markerId in replacements)
			{
				Marker marker = Account.GetCachedMarker(int.Parse(markerId));
				marker.MarkerStyle = replacementMarkerStyle;
				marker.UpdateResourceAndDependents();
			}
		}

		// Update the list so that it shows the replacements that were just made.
		InitReplaceList();

        tourPage.RebuildMap();
    }
}
