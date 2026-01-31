// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.SessionState;

public class SlideText : IHttpHandler, IRequiresSessionState
{
	public void ProcessRequest(HttpContext context)
	{
		context.Response.ContentType = "text/html";

		try
		{
			// Get the view Id.
			int viewId = 0;
			int.TryParse(context.Request.QueryString["id"], out viewId);

			// Get the view.
			Tour tour = MapsAliveState.SelectedTour;
			TourPage tourPage = tour.SelectedTourPage;
			TourView tourView = tourPage.GetTourView(viewId);

			// Get the view's description text.
			string text = tourView == null ? string.Empty : tourView.DescriptionHtml;

			if (text.Trim().Length == 0)
				text = "[ This slide has no description ]";

			// Stream the text down to the browser.
			string html = string.Format("<html><head/><body style=\"margin:0px;font-size:11px;font-family:Arial, Helvetica, Verdana, Sans-Serif;\">{0}</body></html>", text);
			context.Response.Write(html);
		}
		catch (Exception ex)
		{
			Utility.ReportException("SlideText", ex);
			context.Response.Write(string.Empty);
		}
    }

    public bool IsReusable
	{
        get	{ return false; }
    }
}