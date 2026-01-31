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

public class Thumbnail : IHttpHandler, IRequiresSessionState
{
	public void ProcessRequest(HttpContext context)
	{
		context.Response.ContentType = "image/jpg";

		try
		{
			string imageType = context.Request.QueryString["type"];
			if (imageType == "map")
			{
				int width = 0;
				int height = 0;
				int.TryParse(context.Request.QueryString["width"], out width);
				int.TryParse(context.Request.QueryString["height"], out height);
				Size requiredSize = new Size(width, height);
				
				Tour tour = MapsAliveState.SelectedTour;
				if (tour == null)
					return;
				
				TourPage tourPage = tour.SelectedTourPage;
				if (tourPage == null)
					return;
			
				TourImage tourImage = tourPage.MapImage;
				Byte[] bytes = tourImage.Thumbnail(requiredSize);
				context.Response.BinaryWrite(bytes);
			}
			else if (imageType == "file")
			{
				string fileName = context.Request.QueryString["file"];
				string fileLocation = FileManager.WebAppFileLocationAbsolute("App_Data", fileName);
				Byte[] bytes = null;
				if (FileManager.FileExists(fileLocation))
					bytes = FileManager.ReadFileBytes(fileLocation);
				
				if (bytes == null)
				{
					// This should never happen, but let's display a nice image anyway.
					fileLocation = FileManager.WebAppFileLocationAbsolute("Images", "MissingSlideImage.gif");
					bytes = FileManager.ReadFileBytes(fileLocation);
				}
				if (bytes == null)
				{
					return;
				}

				Size requiredSize = GetRequiredSize(context, 300);
				bytes = Utility.ScaledImageBytes(ref bytes, requiredSize);
				context.Response.BinaryWrite(bytes);
			}
			else
			{
				// Get the view Id.
				int viewId = 0;
				int.TryParse(context.Request.QueryString["id"], out viewId);

				Size requiredSize = GetRequiredSize(context, 100);

				Tour tour = MapsAliveState.SelectedTour;
				if (tour == null)
					return;

				TourPage tourPage = tour.SelectedTourPage;
				if (tourPage == null)
					return;

				TourView tourView = tourPage.GetTourView(viewId);
				if (tourView == null)
					return;

				Byte[] bytes;
				if (tourView.ShowContentEvent == ShowContentEvent.Never && context.Request.QueryString["ma"] == "1")
				{
					// When there is no marker action and the caller has passed ma=1,
					// show the marker image instead of the tour view image.
					Marker marker = Account.GetCachedMarker(tourView.MarkerId);
					bytes = marker.NormalScaledImageBytes;
				}
				else
				{
					TourImage tourImage = tourView.Image;
					if (tourImage == null)
					{
						// This will normally never happen, but can if someone attempts to access this HTTP handler directly.
						return;
					}
					bytes = tourView.Image.Thumbnail(requiredSize);
				}

				context.Response.BinaryWrite(bytes);
			}
		}
		catch (Exception ex)
		{
			Utility.ReportException("Thumbnail", ex);
		}
    }

	private static Size GetRequiredSize(HttpContext context, int defaultDimension)
	{
		// Get the size of the thumbnail.
		int dimension = 0;
		int.TryParse(context.Request.QueryString["dim"], out dimension);

		if (dimension == 0)
			dimension = defaultDimension;
		Size requiredSize = new Size(dimension, dimension);
		return requiredSize;
	}

    public bool IsReusable
	{
        get	{ return true; }
    }
}