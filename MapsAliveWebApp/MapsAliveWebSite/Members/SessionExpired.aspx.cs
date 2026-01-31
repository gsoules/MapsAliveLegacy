// Copyright (C) 2003-2010 AvantLogic Corporation

public partial class Members_SessionExpired : MemberPage
{
	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle("Session Expired");
		SetPageReadOnly();
		SetActionId(MemberPageActionId.SessionExpired);
		GetSelectedTourOrNone();
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Account;
	}
}
