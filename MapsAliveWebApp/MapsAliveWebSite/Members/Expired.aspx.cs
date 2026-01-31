// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class Members_Expired : MemberPage
{
	protected override void InitControls(bool undo)
	{
		PageText.Text = AppContent.Topic(account.IsTrial ? "WarningTrialExpired" : "WarningPlanExpired");
		string message = string.Format("Time on your {0} has run out", MapsAliveState.Account.MembershipDescription);
		SetPageError(message);
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle("Account Expired");
		SetPageReadOnly();
		SetActionId(MemberPageActionId.Expired);
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Account;
	}
}
