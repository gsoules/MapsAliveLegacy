// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class User_Error404 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
		string url = Request.QueryString["aspxerrorpath"];
		
		if (url == null)
		{
			if (Request.QueryString.Count >= 1)
			{
				// aspxerrorpath will be missing if the error was caught by IIS before it got to ASP.NET
				// e.g. mapsalive.com/nosuchfolder as opposed to mapsalive.com/nosuchfile.aspx. In that case
				// the query string will look like this: 404;http://beta.mapsalive.com:80/nosuchfolder
				// and so we clean it up to get rid of parts we don't want the user to see.
				url = Request.QueryString[0];
				url = url.Replace("404;", "");
				
                // Filter out the port number (80 on the beta server, 443 on the production server).
                url = url.Replace(":80", "");
				url = url.Replace(":443", "");
			}
		}
		else if (url.StartsWith("/"))
		{
			url = url.Substring(1);
		}
		
		string ok = Request.QueryString["ok"];


        // Redirect to the Home page instead of displaying this 404 Error page.
        Utility.WriteToLogFile(String.Format("404 : {0}", url));
        Utility.TransferToHomePage();
        return;

        /*
        if (ok != "1")
		{
			// Circumvent the problem where this error page can't access it's CSS if the error path is such that
			// the relative reference to style.css does not work. We do this by redirecting to an absolute page URL
			// and marking the redirect as ok.
			Response.Redirect(App.WebSitePathUrl(string.Format("User/Error404.aspx?aspxerrorpath={0}&ok=1", url)));
		}
		
		// Return a 404 error code so that anything pinging the site like a crawler or PCI scan
		// knows that the page does not exist. If we don't do this, a 200 code is returned.
		// This is necessary to pass our TrustKeeper PCI tests because they look for folders
		// that contain things that have vulnerabilities like Front Page Extensions. If they get
		// back a 200 code instead of 404, they just assume the vulnerability exists and fail us.
		Response.StatusCode = 404;

		UrlName.Text = url;

		string mailTo = "mailto:support@mapsalive.com?subject=MapsAlive page not found (404 error)";
		EmailLink.NavigateUrl = mailTo;
        */
	}
}
