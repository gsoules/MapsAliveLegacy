// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public abstract class UploadPage : MemberPage
{
	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}
}
