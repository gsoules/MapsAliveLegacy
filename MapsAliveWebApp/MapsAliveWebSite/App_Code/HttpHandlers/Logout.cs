// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web;
using System.Web.SessionState;

public class Logout : IHttpHandler, IRequiresSessionState
{
	public void ProcessRequest(HttpContext context)
	{
		Account account = MapsAliveState.Account;

		if (account != null)
		{
			// The user is still loggged in so log them out. If account is null,
			// the user clicked the logout link after their session had expired
			// (or someone typed the URL for logout.ashx directly in the browser).
			account.Logout();
		}
		
		// Redirect rather than transfer to the login page to force
		// the page to reload and redraw the banner so that the logged
		// out user's name will no longer appear.
		context.Response.Redirect("~/User/Login.aspx");
	}

	public bool IsReusable
	{
		get { return true; }
	}
}