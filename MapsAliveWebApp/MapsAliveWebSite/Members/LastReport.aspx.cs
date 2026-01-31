// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.IO;
using System.Web.UI.WebControls;

public partial class Members_LastReport : MemberPage
{
	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle("Last Report");
		SetPageReadOnly();
		SetActionIdForPageAction(MemberPageActionId.LastReport);
		GetSelectedTourOrNone();

		HideTracePanel.Visible = !ShowTraceCheckBox.Checked;

		string report = (string)MapsAliveDatabase.ReadColumn("LastReport", "sp_Account_GetLastReport", "@AccountId", account.Id);
		if (report == null)
			report = "There is no last report";
		Utility.SetDivText(Report, report);
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Account;
	}
}
