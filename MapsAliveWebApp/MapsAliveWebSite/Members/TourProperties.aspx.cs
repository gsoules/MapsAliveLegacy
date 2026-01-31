// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class Members_TourProperties : MemberPage
{
	protected override void PageLoad()
	{
		Server.Transfer(MemberPageAction.ActionPageTarget(account.LastActionIdForGroup(MemberPageActionId.TourProperties)));
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}
}
