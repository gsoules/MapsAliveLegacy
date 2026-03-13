// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Drawing;

public class Banner
{
	private BannerImage _image;
	private int imageId;
	private string urlTitle;
	private string url;
	private bool urlOpensWindow;
	private Tour tour;

	public Banner(Tour tour)
	{
	    this.tour = tour;
	}

	public Banner(Tour tour, MapsAliveDataRecord record)
	{
		this.tour = tour;
		imageId = record.IntValue("BannerImageId");
		url = record.StringValue("BannerUrl");
		urlTitle = record.StringValue("BannerUrlTitle");
		urlOpensWindow = record.BoolValue("BannerUrlOpensWindow");
	}

	public bool HasImage
	{
		get { return imageId > 0; }
	}

	public BannerImage Image
	{
		get
		{
			if (_image == null)
			{
				if (imageId == 0)
				{
					CreateBannerImage();
				}
				else
				{
					// We delay creating the image until something actually uses it.
					_image = new BannerImage(tour, imageId);
				}
			}
			return _image;
		}
		set { _image = value; }
	}

	public int ImageId
	{
		get { return imageId; }
		set { imageId = value; }
	}

	public Size Size
	{
		get { return new Size(tour.TourSize.Width, OptimalHeight()); }
	}

	public string Url
	{
		get { return url == null ? string.Empty : url; }
		set { url = value; }
	}

	public bool UrlOpensWindow
	{
		get { return urlOpensWindow; }
		set { urlOpensWindow = value; }
	}

	public string UrlTitle
	{
		get { return urlTitle == null ? string.Empty : urlTitle; }
		set { urlTitle = value; }
	}

	public void CreateBannerImage()
	{
		_image = new BannerImage(tour, 0);
		imageId = TourImage.GetNextIdForTour(tour.Id);
		_image.Id = imageId;
		tour.SetBannerImageChanged();
		_image.InsertImageIntoDatabase();
		
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_UpdateBannerImageId",
			"@TourId", tour.Id,
			"@ImageId", imageId);
	}

	public void ImageUploaded(string fileName, Size size, Byte[] bytes)
	{
		Image.Uploaded(tour.Id, fileName, size, bytes);
		tour.SetBannerImageChanged();
		Image.KeepUploadedFile(tour.Id);

		foreach (TourPage tourPage in tour.TourPages)
		{
			tourPage.InvalidateThumbnail();
			tourPage.SetBannerImageChanged();
		}
	}

	public int OptimalHeight()
	{
		return OptimalHeight(tour.TourSize.Width);
	}

	public int OptimalHeight(int width)
	{
		// Create a virtual area that is as wide as the tour and as high as the actual banner image.
		// Use it to figure out the proper height for the scaled image.
		Size bannerAreaSize = new Size(width, Image.Height);
		Size scaledImageSize = Utility.ScaledImageSize(Image.Size, bannerAreaSize);
		return scaledImageSize.Height;
	}

	public void SetOptions(bool runAutoLayout, int changeFlags, bool imageChanged, string url, string title, bool opensWindow)
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_UpdateBannerOptions",
			"@TourId", tour.Id,
			"@ThemeId", tour.ThemeId,
			"@HasBanner", tour.HasBanner,
			"@BannerUrl", url,
			"@BannerUrlTitle", title,
			"@BannerUrlOpensWindow", urlOpensWindow,
			"@ChangeFlags", changeFlags);

		if (imageChanged)
			tour.SetBannerImageChanged();

		foreach (TourPage tourPage in tour.TourPages)
		{
			if (imageChanged && runAutoLayout)
			{
				tourPage.LayoutManager.PerformAutoLayoutForBannerChange();
				tourPage.SetBannerImageChanged();
			}
			tourPage.SetBannerOptionsChanged();
		}
	}
}
