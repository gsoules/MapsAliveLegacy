// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web;

public partial class User_ForcedLogout : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
	{
		string message = "<b>You have been logged out</b> because another browser started a session with this account. This could happen because you opened another browser window and logged into MapsAlive. It could also happen if another person logged into your account from another computer.";

		int accountId = 0;
		string accountIdString = Request.QueryString["id"];
		if (accountIdString != null)
		{
			int.TryParse(accountIdString, out accountId);
		}

		if (accountId > 0)
		{
			MapsAliveDataRow row = MapsAliveDatabase.LoadDataRow("sp_Account_GetSessionInfo", "@AccountId", accountId);
			if (row != null)
			{
				string sessionIpAddress = row.StringValue("SessionIpAddress");
				DateTime sessionTime = row.DateTimeValue("SessionTime");
				TimeSpan timeSpan = DateTime.Now - sessionTime;
				string browser = row.StringValue("SessionBrowser");
				int minutes = timeSpan.Minutes;
				if (minutes <= 0)
					minutes = 1;

				message += string.Format("<br/><br/>The new session started about <b>{0} minute{1} ago</b> using <b>{2}</b> as the browser.", minutes, Utility.Plural(minutes), browser);
				message += string.Format("<br/>The login occurred from IP address <b>{0}</b>", sessionIpAddress);

				string yourIpAddress = HttpContext.Current.Request.UserHostAddress;
				if (sessionIpAddress == yourIpAddress)
					message += " which is the same as your IP address.";
				else
					message += string.Format(" which is different than your IP address <b>{0}</b>.", yourIpAddress);
			}
		}

		message += "<br/><br/>To protect your data, only one browser session at a time is allowed. You can log back in, but if someone else is using your account, they will get logged out.<br/><br/>Note that you can change your password by choosing <b>Account > Profile</b> from the Tour Builder menu.";

		Message.Text = message;
	}
}
