// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web;
using System.Web.SessionState;

public class ExpiredTour : IHttpHandler, IRequiresSessionState
{
	public void ProcessRequest(HttpContext context)
	{
		context.Response.ContentType = "text/html";

		try
		{
			int accountId = 0;
			string title = string.Empty;
			int tourId = 0;

			// Get the query args. Note that we use longer names beginning with "ma" to minimize
			// the chance that our names will conflict with an arg in the referring URL. For example
			// it's less likely that the referrer will reference .maTourId' versus just 'id'.
			int.TryParse(context.Request.QueryString["maTourId"], out tourId);
			string referrerUrl = context.Request.QueryString["maRef"];

			if (tourId != 0)
			{
				accountId = Tour.GetAccountId(tourId);

				if (accountId != 0)
				{
					// Get the tour.
					Tour tour = new Tour(tourId, accountId);
					if (tour != null)
					{
						title = tour.BrowserTitle.Trim().Length == 0 ? tour.Name : tour.BrowserTitle;
					}
				}
			}

			string msg = string.Format(AppContent.Topic("ExpiredTour"), title, tourId == 0 ? "" : tourId.ToString());

			string html = string.Format(
				"<html>" +
				"<head/>" +
				"<body style=\"" +
				"margin:0px;" +
				"background-color:#c5daef;" +
				"\">" +
				"<div style=\"" +
				"color: #464646;" +
				"font-size:12px;" +
				"font-family:Verdana,Arial,Helvetica,Sans-Serif;" +
				"margin-top:32px;" +
				"text-align:center;" +
				"\">" +
				"{0}" +
				"</div>" +
				"</body>" +
				"</html>",
				msg);
			
			// Stream the text down to the browser where it will get loaded as the src of an iframe.
			context.Response.Write(html);

		//	msg = string.Format("Tour: {0}\nTitle: {1}\nAccount: {2}\nReferrer: {3}", tourId, title, accountId, referrerUrl);
		//	Utility.SendEmailToSupport(string.Format("Deactivated tour {0} visited", tourId), msg, false);
		}
		catch (Exception ex)
		{
			Utility.ReportException("ExpiredTour", ex);
			context.Response.Write(Utility.ExceptionHtmlString(ex));
		}
    }

    public bool IsReusable
	{
        get	{ return false; }
    }
}
