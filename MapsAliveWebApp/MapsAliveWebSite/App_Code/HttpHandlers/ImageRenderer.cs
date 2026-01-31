// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Web;
using System.Web.SessionState;

public class ImageRenderer : IHttpHandler, IRequiresSessionState
{
	public void ProcessRequest(HttpContext context)
	{
		context.Response.ContentType = "image/jpg";
		string imageType = string.Empty;

		try
		{
			Tour tour = MapsAliveState.SelectedTourOrNull;
			if (tour == null)
				return;

			TourImage tourImage = null;
			Size requiredSize = Size.Empty;

			// Get the image to be rendered.
			imageType = context.Request.QueryString["type"];
			if (imageType == "photo")
			{
				TourView tourView = tour.SelectedTourView;
				if (tourView == null)
				{
					Debug.Fail("ImageRenderer expected selectedTourView");
				}
				else
				{
					tourImage = tour.SelectedTourView.Image;
					requiredSize = tour.SelectedTourView.GetImageContainerSize();
				}
			}
			else if (imageType == "banner")
			{
				tourImage = tour.Banner.Image;
				requiredSize = tour.Banner.Size;
			}
			else if (imageType == "map" || imageType == "mapInset")
			{
				int width = 0;
				int height = 0;
				int.TryParse(context.Request.QueryString["width"], out width);
				int.TryParse(context.Request.QueryString["height"], out height);

				TourPage tourPage = tour.SelectedTourPage;
				if (tourPage == null)
				{
					Debug.Fail("ImageRenderer expected SelectedTourPage");
				}
				else
				{
					tourImage = tourPage.MapImage;

					if (width == 0 || height == 0)
					{
						if (tourPage.MapCanZoom && tourPage.MapImage.HasFile)
						{
							width = tourPage.MapImage.Width;
							height = tourPage.MapImage.Height;
						}
						else
						{
							width = tourPage.MapAreaSize.Width;
							height = tourPage.MapAreaSize.Height;
						}
					}
					requiredSize = new Size(width, height);
				}
			}
			else
			{
				Debug.Fail("Unexpected image type " + imageType);
			}

			// Get the data for the original image.
			if (tourImage == null)
			{
				// This can happen if the user adds a new slide and quickly (inadvertently) mouses
				// over the image preview before the page has reloaded.
				return;
			}

			if (!tourImage.HasFile)
				tourImage.Size = requiredSize;
			Byte[] bytes = tourImage.Bytes;
			Debug.Assert(bytes.Length > 0, "Image has zero bytes");
			
			// If the original image's dimensions are larger than the container, scale
			// the image down to fit the container.  We never up-scale images.
			if (tourImage.Width > requiredSize.Width || tourImage.Height > requiredSize.Height)
				bytes = Utility.ScaledImageBytes(ref bytes, requiredSize);

			if (imageType == "mapInset")
			{
				Bitmap bitmap = Utility.BitmapFromBytes(bytes);
				if (Utility.SharpenMapInsetImage(bitmap))
					bytes = Utility.ImageToByteArray(bitmap, ImageFormat.Jpeg);
			}

			// Stream the image's bytes down to the browser.
			context.Response.BinaryWrite(bytes);
		}
		catch (Exception ex)
		{
			Utility.ReportException("ImageRenderer: " + imageType, ex);
		}
    }

    public bool IsReusable
	{
        get	{ return false; }
    }
}