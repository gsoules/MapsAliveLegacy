// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Web.UI.WebControls;

public partial class Admin_Users : MemberPage
{
	protected override void PageLoad()
	{
		SetPageTitle(Resources.Text.UserAccountsTitle);
		SetPageReadOnly();
		SetActionId(MemberPageActionId.UserAccounts);
		GetSelectedTourOrNone();

		string deletedUser = Request.QueryString["deleted"];
		if (deletedUser != null)
			SetPageMessage("User was deleted: " + deletedUser);

		if (Utility.ImitatingUser())
		{
			SetPageSpecialNotice("Impersonating " + Utility.AccountIdentification);
			StopImpersonationButton.Text = "Stop Impersonating " + account.ContactName;
		}
		else
		{
			StopImpersonationPanel.Visible = false;
		}

		Filter.Focus();

		Filter.Attributes.Add("onfocus", "SetSelected();");

		if (!IsPostBack || Request.Form["__EVENTARGUMENT"] == string.Empty)
		{
			// The user did not click the Select link in a row. They either pressed Enter, or
			// clicked the Find button, and so we need to update the grid.
			// When they click a row, we don't update the grid because we redirect to the Profile screen.
			UpdateGridView();
		}
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		// In order to make the Enter key work on this page, we had to disable partial rendering,
		// otherwise we got a JavaScript error: "'this._postBackSettings.async' is null or not an object".
		// Note that EnablePartialRendering can only be set here in Page_Init which executes before Page_Load.
		SetMasterPage(Master);
		masterPage.MasterScriptManager.EnablePartialRendering = false;
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Account;
	}

	private void UpdateGridView()
	{
		string filterText = Filter.Text;

		if (Filter.Text == string.Empty)
		{
			filterText = Request.QueryString["filter"];

			if (filterText == null)
			{
				// When this page is hit from Highrise, the arg is id instead of filter.
				filterText = Request.QueryString["id"];
			}
	
			if (filterText == null)
			{
				filterText = (string)MapsAliveState.Retrieve(MapsAliveObjectType.FindUsersFilter);
			}

			Filter.Text = filterText;
		}

		if (string.IsNullOrEmpty(filterText))
			return;
		
		MapsAliveState.Persist(MapsAliveObjectType.FindUsersFilter, Filter.Text.Trim());
		
		DataTable table;

		if (filterText == "*")
			table = MapsAliveDatabase.LoadDataTable("sp_Account_GetUsers");
		else if (filterText.ToLower().StartsWith("#"))
			table = MapsAliveDatabase.LoadDataTable("sp_Account_GetUserByTour", "@TourId", filterText.Substring(1).Trim());
		else
			table = MapsAliveDatabase.LoadDataTable("sp_Account_GetUsersByFilter", "@Filter", filterText);

		GridView.DataSource = table;
		GridView.DataBind();
		
		Results.Text = string.Format("{0} records found", table.Rows.Count);
	}

	protected void OnSelectRow(object sender, EventArgs e)
	{
		string email = GridView.SelectedRow.Cells[2].Text;

		if (Utility.UserHasAdminRole(email))
		{
			SetPageError("You cannot impersonate an administrator");
			return;
		}

		// It's okay to imitate this user because they are not ad administrator.
		Utility.ImitateUser(email);
		MapsAliveState.FlushSessionState();

		string targetPage = MemberPageAction.ActionPageTarget(MemberPageActionId.Profile);
		Response.Redirect("~/Members/" + targetPage);
	}

	protected void OnStopImpersonation(object sender, EventArgs e)
	{
		Utility.ImitateUser(null);
		SetPageSpecialWarning(string.Empty);
		StopImpersonationPanel.Visible = false;
	}
}
