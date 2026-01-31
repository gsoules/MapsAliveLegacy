// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Web.UI.WebControls;
using AvantLogic.MapsAlive;

public partial class Admin_Reports : MemberPage
{
	protected override void PageLoad()
	{
		AdminReportType reportType;

		SetMasterPage(Master);

		// Since reports are only used by Admins, we allow use of the Back Button
		// by not preventing page caching.  Otherwise it's a pain to use reports.
		Master.AllowPageCaching();

		SetPageTitle(Resources.Text.AdminReportsTitle);
		SetPageReadOnly();
		SetActionId(MemberPageActionId.Reports);
		GetSelectedTourOrNone();

		if (!IsPostBack)
		{
			// Set default selections.
			int periodIndex = 2; // 1 day
			reportType = AdminReportType.None;

			// See if this is a request to login as a user.
			int accountId = 0;
			int.TryParse(Request.QueryString["login"], out accountId);
			if (accountId > 0)
			{
				LoginAsUser(accountId);
				return;
			}

			// See if a specific report was selected on the query string.
			accountId = 0;
			int.TryParse(Request.QueryString["accountId"], out accountId);
			if (accountId > 0)
			{
				reportType = AdminReportType.ToursForAccount;
				SearchTextBox.Text = accountId.ToString();
			}

			// Populate the dropdown lists.
			PopulateReportTimePeriodList(PeriodDropDownList, periodIndex);
			PopulateReportList(ReportDropDownList);
			ReportDropDownList.SelectedValue = reportType.ToString();
		}

		reportType = (AdminReportType)Enum.Parse(typeof(AdminReportType), ReportDropDownList.SelectedValue);
		AdminReport.Type = reportType;
		AdminReport.Days = int.Parse(PeriodDropDownList.SelectedValue);
		AdminReport.SearchText = SearchTextBox.Text;
		AdminReport.LoadData();
		SetPageMessage(AdminReport.StatusMessage);

		bool enableSearchField = reportType == AdminReportType.ToursForAccount;
		SearchTextBox.Enabled = enableSearchField;
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Account;
	}

	private void PopulateReportList(DropDownList list)
	{
		list.Items.Add(new ListItem("< Choose >", AdminReportType.None.ToString()));
		list.Items.Add(new ListItem("Account: Created", AdminReportType.AccountsCreated.ToString()));
		list.Items.Add(new ListItem("Account: Activity", AdminReportType.AccountsLastActive.ToString()));
		list.Items.Add(new ListItem("Tours: Created", AdminReportType.ToursCreated.ToString()));
		list.Items.Add(new ListItem("Tours: Built", AdminReportType.ToursBuilt.ToString()));
		list.Items.Add(new ListItem("Tours: Published", AdminReportType.ToursPublished.ToString()));
		list.Items.Add(new ListItem("Tours: Downloaded", AdminReportType.ToursDownloaded.ToString()));
		list.Items.Add(new ListItem("Tours: Archived", AdminReportType.ToursArchived.ToString()));
		list.Items.Add(new ListItem("Tours: Account #", AdminReportType.ToursForAccount.ToString()));

		// Reports that we don't ever use anymore.
		//list.Items.Add(new ListItem("Account: Account #", AdminReportType.AccountSummary.ToString()));
		//list.Items.Add(new ListItem("Account: User name", AdminReportType.AccountsLikeUserName.ToString()));
		//list.Items.Add(new ListItem("Account: Free", AdminReportType.FreeMemberships.ToString()));
		//list.Items.Add(new ListItem("Account: Paid", AdminReportType.PaidMemberships.ToString()));
		//list.Items.Add(new ListItem("Account: Elite", AdminReportType.EliteAccounts.ToString()));
	}

	private void PopulateReportTimePeriodList(DropDownList list, int selected)
	{
		list.Items.Add(new ListItem("All", "10000"));
		list.Items.Add(new ListItem("New", "1"));
		list.Items.Add(new ListItem("3 days", "3"));
		list.Items.Add(new ListItem("7 days", "7"));
		list.Items.Add(new ListItem("14 days", "14"));
		list.Items.Add(new ListItem("30 days", "30"));
		list.Items.Add(new ListItem("60 days", "60"));
		list.Items.Add(new ListItem("90 days", "90"));

		if (selected < 1 || selected > list.Items.Count)
			selected = 1;
		list.Items[selected - 1].Selected = true;
	}

	private void LoginAsUser(int accountId)
	{
		if (Utility.UserIsAdmin)
		{
			Account account = new Account(accountId);
			if (account != null)
			{
				Utility.ImitateUser(account.UserName);
				MapsAliveState.FlushSessionState();
				Response.Redirect(App.WebSitePathUrl("Members/" + MemberPageAction.ActionPageTarget(MemberPageActionId.TourExplorer)));
			}
		}
	}
}
