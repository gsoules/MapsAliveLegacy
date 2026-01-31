// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Configuration;
using System.Data;
using System.Web.Security;

public partial class User_Login : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
    {
        if (!Request.IsSecureConnection && !App.DeveloperMode)
        {
			Response.Redirect("~/Default.aspx");
            return;
        }

		if (Utility.UserIsLoggedIn)
		{
			MapsAliveState.Account.Logout();
		}

        LoginEmail.Focus();
		
		// Make sure that nothing somehow got leftover from the previous session.  In theory
		// we should not have to do this, but there is at least one case where if the user
		// clicks logout from My Account while a page is still loading (e.g. My Tours) the
		// user's Account object can get restablished.  When that happens, a newly logged in
		// user gets the previous users account.  Until we figure out how to prevent that and
		// until we are certain there are no similar cases, we do this to be safe.
		MapsAliveState.FlushSessionState();

		if (IsPostBack && Request.QueryString["login"] != "0")
			OnLogin();
	}

	protected void OnGoHome(object sender, EventArgs e)
	{
		Response.Redirect("~/Default.aspx");
	}

	protected void OnRetrievePassword(object sender, EventArgs e)
	{
		Response.Redirect("~/User/RecoverPassword.aspx");
	}

	protected void OnSignup(object sender, EventArgs e)
	{
		Response.Redirect("~/public/pricing");
	}

	protected void OnLogin()
	{
		string userName = LoginEmail.Text;
		
		MembershipUser user = null;
		try
		{
			user = Membership.GetUser(userName);
		}
		catch
		{
			// An exception can occur if the user name contains a comma. There are probably other cases.
			LoginFailed.Text = "The email address you provided is not valid";
			LoginFailed.Visible = true;
			return;
		}
	
		if (user != null && user.IsLockedOut)
		{
			// A user will get locked out after maxInvalidPasswordAttempts (in web.config).
			TimeSpan timeSpan = DateTime.Now - user.LastLockoutDate;
			if (timeSpan.Minutes > 10)
				user.UnlockUser();
		}
				
		if (Membership.ValidateUser(userName, LoginPassword.Text))
		{
			FormsAuthentication.RedirectFromLoginPage(userName, RememberMe.Checked);
			string targetUrl = MemberPageAction.ActionPageTarget(MemberPageActionId.TourBuilder);
			Response.Redirect(App.WebSitePathUrl(targetUrl));
		}
		else
		{
			string msg = string.Empty;
			if (user == null)
			{
				// V2 migration used to be supported here, but was removed as of 6/8/2015.
				msg = "The email address you provided does not match an account in our records.";
			}
			else if (user.IsLockedOut)
			{
				msg = "This account has been locked because of too many failed login attempts. Please wait 10 minutes and try again with a valid email and password.";
			}
			else
			{
				msg = "Login was not sucessful. Note that passwords are case-sensitive. To get your password, click 'I forgot my password'.";
			}

			LoginFailed.Text = msg;
			LoginFailed.Visible = true;
		}
	}
}
