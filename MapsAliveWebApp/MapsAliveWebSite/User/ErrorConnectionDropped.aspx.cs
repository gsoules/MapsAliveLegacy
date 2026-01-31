// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class User_ErrorConnectionDropped : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
		try
		{
			string mailTo = "mailto:support@mapsalive.com?subject=MapsAlive connection problem";
			EmailLink.NavigateUrl = mailTo;

			if (Utility.UserIsLoggedIn)
			{
				TourBuilderLink.Visible = true;
				TourBuilderLink.NavigateUrl = MemberPageAction.ActionPageTarget(MemberPageActionId.TourBuilder);
			}
		}
		catch
		{
		}
	}
}
