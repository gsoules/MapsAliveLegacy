// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class Members_ImportHotspots : MemberPage
{
	protected override void PageLoad()
	{
		Server.Transfer(MemberPageAction.ActionPageTarget(MapsAliveState.Account.LastActionIdForImportSlides));
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}
}
