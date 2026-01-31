// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class Members_MapProperties : MemberPage
{
	protected override void PageLoad()
	{
		Server.Transfer(MemberPageAction.ActionPageTarget(account.LastActionIdForGroup(MemberPageActionId.MapProperties)));
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}
}
