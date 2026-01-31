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

public class SymbolRenderer : IHttpHandler, IRequiresSessionState
{
	public void ProcessRequest(HttpContext context)
	{
		context.Response.ContentType = "image/jpg";

		int symbolId = 0;
		int width = 0;

		int.TryParse(context.Request.QueryString["id"], out symbolId);
		int.TryParse(context.Request.QueryString["width"], out width);

		try
		{
			// Get the bytes for the symbol's image;
			Symbol symbol = Account.GetCachedSymbol(symbolId);
			Byte[] bytes = symbol.Bytes;

			// Scale the symbol if a size was passed.
			if (width > 0)
				bytes = Utility.ScaledImageBytes(ref bytes, new Size(width, width));

			// Stream the image's bytes down to the browser.
			context.Response.BinaryWrite(bytes);
		}
		catch (Exception ex)
		{
			Utility.ReportException("SymbolRenderer", ex);
		}
	}

	public bool IsReusable
	{
		get { return false; }
	}
}