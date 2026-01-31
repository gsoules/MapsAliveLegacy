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
using AvantLogic.MapsAlive.Engine;

public class MarkerRenderer : IHttpHandler, IRequiresSessionState
{
	public void ProcessRequest(HttpContext context)
	{
		context.Response.ContentType = "image/jpg";

		int markerId = 0;
		int state = 0;
		bool actualSize = context.Request.QueryString["actual"] == "1";
		
		int.TryParse(context.Request.QueryString["id"], out markerId);
		int.TryParse(context.Request.QueryString["state"], out state);

		try
		{
			Marker marker = Account.GetCachedMarker(markerId);

			if (marker == null)
			{
				Utility.ReportError("MarkerRenderer", "No marker found for Id " + markerId);
				return;
			}
			
			// Get the bytes for the marker's preview image;
			Byte[] bytes;
			if (state == (int)MarkerState.Combo)
			{
				bytes = marker.ResourceImageBytes;
			}
			else
			{
				if (actualSize)
				{
					bytes = state == (int)MarkerState.Normal ? marker.NormalActualImageBytes : marker.SelectedActualImageBytes;
				}
				else
				{
					bytes = state == (int)MarkerState.Normal ? marker.NormalScaledImageBytes : marker.SelectedScaledImageBytes;
				}
			}

			if (bytes.Length == 0)
			{
				string missingImageFileLocation = FileManager.WebAppFileLocationAbsolute("Images", "MissingSlideImage.gif");
				Size size;
				bytes = Utility.ImageFileToByteArray(missingImageFileLocation, out size);
			}

			// Stream the image's bytes down to the browser.
			context.Response.BinaryWrite(bytes);
		}
		catch (Exception ex)
		{
			Utility.ReportException("MarkerRenderer", ex);
		}
    }

    public bool IsReusable
	{
        get	{ return false; }
    }
}