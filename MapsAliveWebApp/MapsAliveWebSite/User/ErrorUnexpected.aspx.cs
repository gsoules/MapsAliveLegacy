// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class User_ErrorUnexpected : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
		try
		{
			string id = Request.QueryString["id"];
			string dbStatus = MapsAliveDatabase.DatabaseStatus;
			bool dbError = dbStatus != string.Empty;

			if (dbError)
			{
				PanelReturnToHomePage.Visible = false;
				Title.Text = "MapsAlive cannot access its database";
				
				// Extract the error number from the status if there is one.
				string[] status = dbStatus.Split(':');
				if (status.Length > 0)
					id = status[0];
			}
			else if (id == null)
			{
				// When Id is not null, we detected and reported and unexpected problem.
				// When it's null, the exception was unhandled. Use a fake Id to indicate that case.
				id = "500";
			}

			ErrorId.Text = "Error ID = " + id;
			DateTime now = DateTime.Now;
			string message = "MapsAlive error " + id + " occurred on " + now.ToLongDateString() + " at " + now.ToLongTimeString();

			if (dbError)
				message += " " + dbStatus;
			
			string mailTo = "mailto:support@mapsalive.com?subject=Error Report ID=" + id + "&body=" + message;
			EmailLink.NavigateUrl = mailTo;

			try
			{
				string subject = string.Format("UNEXPECTED ERROR {0}", dbError ? "Database" : id);
				Utility.ReportError(subject, string.Empty);
			}
			catch (Exception)
			{
			}
		}
		catch
		{
		}
	}
}
