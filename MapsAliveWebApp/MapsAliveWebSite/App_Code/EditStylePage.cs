// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.UI.WebControls;

public abstract class EditStylePage : MemberPage
{
	protected int resourceId;
	protected TourResource resource;
	protected string validName;

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Resources;
	}

	protected override void PerformUpdate()
	{
		resource.UpdateResource(resourceBeforeEdit);
	}

	protected void CreateNewResource(TourResourceType resourceType)
	{
		resource = TourResourceManager.CreateNewResource(resourceType, resourceId);
	}

	protected TourResource CreateResourceFromQueryStringId(TourResourceType resourceType)
	{
		if (IsPostBack || IsReturnToTourBuilder)
		{
			resourceId = MapsAliveState.Account.LastResourceId(resourceType);

			if (resourceId == 0)
			{
				// This can happen if the user came back the style editor screen (e.g. from TourPreview)
				// after a very long time such that their session expired.
				Response.Redirect(MemberPageAction.ActionPageTarget(MemberPageActionId.TourExplorer));
			}
			
			CreateNewResource(resourceType);
		}
		else
		{
			string id = Request.QueryString["id"];
			int.TryParse(id, out resourceId);

			// Special case handling for tour style when we want to make sure that that the style for
			// the current tour is to be edited. This logic is used when the user clicks the Edit link
			// next to the Color Scheme scheme list. See comments in that code for more info.
			if (resourceType == TourResourceType.TourStyle && resourceId == -1 && tour != null)
				resourceId = tour.ColorScheme.Id;

			if (resourceId != 0)
				CreateNewResource(resourceType);

			if (resource == null)
			{
				// This can happen if a user deletes a resource and then uses the Back button to
				// return to a screen that was referencing that resource via a query string Id.
				return null;
			}

			MapsAliveState.Account.SetLastResourceId(resourceType, resource.Id);
		}

		// Copy the unedited resource so that it can be compared later against the edited resource.
		resourceBeforeEdit = resource.Clone();
		
		return resource;
	}

	protected virtual void ClearErrors()
	{
	}

	protected virtual void ValidateResourceName(TextBox nameTextBox, Label nameTextBoxError)
	{
		ClearErrors();

		string resourceName = TourResourceManager.GetTitle(resource.ResourceType).ToLower();

		validName = nameTextBox.Text.Trim();
		ValidateFieldNotBlank(validName, nameTextBoxError, string.Format("A {0} name is required", resourceName));
		if (!fieldValid)
			return;

		// Report an error it the user changed the title and the new title is already in use.
		bool nameInUse = FontStyleResource.NameInUse(resource.ResourceType, resourceId, validName, account.Id);
		ValidateFieldCondition(!nameInUse, nameTextBoxError, string.Format("Another {0} has that name", resourceName));
	}

	protected override void Undo()
	{
		ClearErrors();
	}
}
