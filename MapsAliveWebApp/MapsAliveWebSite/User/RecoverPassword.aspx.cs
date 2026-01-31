// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.Security;

public partial class User_RecoverPassword : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
	{
		LoginEmail.Focus();

		if (IsPostBack)
			OnGetPassword();
	}

	private void OnGetPassword()
	{
		string userName = LoginEmail.Text.Trim();
		if (userName.Length == 0)
			return;

		string pw = string.Empty;

		MembershipUser user;
		try
		{
			user = Membership.GetUser(userName);
		}
		catch (Exception)
		{
			Message.Text = "That is not a valid email address.";
			return;
		}
		
		if (user == null)
		{
			pw = string.Empty;
		}
		else
		{
			try
			{
				pw = user.GetPassword();
			}
			catch
			{
				Message.Text = "Sorry. That account is temporarily locked.";
				return;
			}
		}
		
		if (pw == string.Empty)
		{
			Message.Text = Resources.Text.PasswordNotMailedText;
		}
		else
		{
			Message.Text = Resources.Text.PasswordSentText;
			RetrievePasswordText.Visible = false;
			GetPasswordButton.Visible = false;
			LoginEmail.Enabled = false;

			string body = string.Format("Here is the information you requested: {0}", pw);
			Utility.SendEmailToCustomer(userName, "MapsAlive", body);
		}
		
		return;
	}
}
