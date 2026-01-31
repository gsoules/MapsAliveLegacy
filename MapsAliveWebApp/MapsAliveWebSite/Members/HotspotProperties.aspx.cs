// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class Members_HotspotProperties : MemberPage
{
	protected override void PageLoad()
	{
		Server.Transfer(MemberPageAction.ActionPageTarget(MapsAliveState.Account.LastActionIdForGroup(MemberPageActionId.HotspotProperties)));
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}
}
