// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class Members_LayoutProperties : MemberPage
{
	protected override void PageLoad()
	{
		Server.Transfer(MemberPageAction.ActionPageTarget(MapsAliveState.Account.LastActionIdForGroup(MemberPageActionId.LayoutProperties)));
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}
}
