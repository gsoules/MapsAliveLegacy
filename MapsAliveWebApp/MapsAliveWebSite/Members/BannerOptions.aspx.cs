// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Drawing;
using Telerik.Web.UI;

public partial class Members_BannerOptions : ImageUploadPage
{
	private bool bannerRemoved;
	private bool bannerUploaded;
	private bool imageChanged;
	private bool optionsChanged;
	private string validBannerUrl;
	private string validBannerUrlTitle;
	private bool validBannerUrlOpensWindow;

	protected override void InitControls(bool undo)
	{
		if (tour.HasBanner && !badFileName)
		    FileName.Text = tour.Banner.Image.FileNameOriginal;

		if (tour.HasBanner)
		{
			Size scaledImageSize = Utility.ScaledImageSize(tour.Banner.Image.Size, tour.Banner.Size);
			if (scaledImageSize.Width < tour.Banner.Size.Width || scaledImageSize.Height < tour.Banner.Size.Height)
				scaledImageSize = tour.Banner.Size;
			ImageSizeActual.Text = string.Format("{0} x {1}", tour.Banner.Image.Size.Width, tour.Banner.Image.Size.Height);
			ImageSizeScaled.Text = string.Format("{0} x {1}", scaledImageSize.Width, scaledImageSize.Height);

			ImageElement.Visible = true;
			ImageElement.ImageUrl = string.Format("ImageRenderer.ashx?type=banner&v={0}", DateTime.Now.Ticks);

			ImageElement.Width = Math.Min(150, scaledImageSize.Width);
		}

		RemoveBannerPanel.Visible = tour.HasBanner;

        bool navButtonIsInBanner = tour.V4 && tour.HasBanner && (
            tour.Directory.Location == TourDirectoryLocation.BannerLeft ||
            tour.Directory.Location == TourDirectoryLocation.BannerCenter ||
            tour.Directory.Location == TourDirectoryLocation.BannerRight);

        if (navButtonIsInBanner)
        {
            NavButtonInBannerMessage.Visible = true;
            ButtonRemove.Visible = false;
        }

        OptionsPanel.Visible = tour.HasBanner;
		FileInfoPanel.Visible = tour.HasBanner;

		if (tour.TourPageCount > 0)
		{
			InitPreviewControls();
		}
		else
		{
			LayoutPreviewImagePanel.Visible = false;
		}
	
		if (!undo && IsPostBack)
			return;

		if (tour.HasBanner)
		{
			BannerUrlTextBox.Text = tour.Banner.Url;
			AddChangeDetection(BannerUrlTextBox);

			BannerUrlTitleTextBox.Text = tour.Banner.UrlTitle;
			AddChangeDetection(BannerUrlTitleTextBox);

			BannerUrlOpensWindowCheckBox.Checked = tour.Banner.UrlOpensWindow;
			AddChangeDetection(BannerUrlOpensWindowCheckBox);
		}
	}

	private void InitPreviewControls()
	{
		// Show the dimensions of each area in the slide.
		int drawDimensions = tour.V3CompatibilityEnabled ? 1 : 0;

        // Draw the image to fit the available width.
        int thumbnailDimension = Math.Min(tour.TourSize.Width, TourBuilderPageContentRightWidth);

        LayoutPreviewImage.ImageUrl = string.Format("PageRenderer.ashx?args={0},{1},{2},{3},{4}&v={5}",
			(int)PreviewType.PagePreview,
			thumbnailDimension,
			tourPage.Id,
			drawDimensions,
			(int)tourPage.ActiveSlideLayout.Pattern,
			DateTime.Now.Ticks
		);
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle(Resources.Text.BannerOptionsTitle);
		SetActionIdForPageAction(MemberPageActionId.BannerOptions);
		GetSelectedTourOrNone();

		if (!IsPostBack)
			ProgressMonitor.ShowUploadFileProgress(ProgressArea);
	}

	protected override void PagePreRender()
	{
		base.PagePreRender();

		if (badFileName)
			SetPageError(badFileNameMessage);
	}

	protected override void PerformUpdate()
	{
		if (bannerUploaded || bannerRemoved)
		{
			if (bannerRemoved)
				tour.HasBanner = false;
			if (bannerUploaded)
				tour.HasBanner = true;
			imageChanged = true;
			optionsChanged = true;
		}

		if (optionsChanged)
		{
			const bool runAutoLayout = true;
			tour.SetBannerOptions(
				runAutoLayout,
				imageChanged,
				validBannerUrl,
				validBannerUrlTitle,
				validBannerUrlOpensWindow);
		}

		if (tour.HasBanner && tour.WidthType != TourSizeType.Exact)
		{
			tour.WidthType = TourSizeType.Exact;
			tour.UpdateDatabase();
		}
	}

	protected override void RemoveImage()
	{
		// Delete the banner image file from the preview folder.
		string fileName = tour.Banner.Image.FileNameInternal;
		string fileLocation = FileManager.PreviewFolderLocationAbsolute(tour.Id, fileName);
		FileManager.DeleteFile(fileLocation);
		
		// Delete the image's data, but don't delete it's placeholder record from the database.
		// We need to keep the record so we can use it if the user uploads another banner image.
		tour.Banner.Image.EraseBytes();
		tour.Banner.Image.UpdateImageInDatabase();

		bannerRemoved = true;
		validBannerUrl = tour.Banner.Url;
		validBannerUrlTitle = tour.Banner.UrlTitle;
		validBannerUrlOpensWindow = tour.Banner.UrlOpensWindow;
		
		PerformUpdate();
	}

	protected override void ReadPageFields()
	{
		// We test every option to see if it changed to avoid updating the banner unless
		// necessary.  Since the banner appears on every page, a change to it requres that
		// every page be rebuilt.

		if (tour.Banner.Url != validBannerUrl)
		{
			optionsChanged = true;
			tour.Banner.Url = BannerUrlTextBox.Text;
		}

		if (tour.Banner.UrlTitle != validBannerUrlTitle)
		{
			optionsChanged = true;
			tour.Banner.UrlTitle = BannerUrlTitleTextBox.Text;
		}

		if (tour.Banner.UrlOpensWindow != validBannerUrlOpensWindow)
		{
			optionsChanged = true;
			tour.Banner.UrlOpensWindow = BannerUrlOpensWindowCheckBox.Checked;
		}
	}

	protected override void ImageUploaded(string fileName, Size size, Byte[] bytes)
	{
		tour.Banner.ImageUploaded(fileName, size, bytes);
		
		optionsChanged = true;
		imageChanged = true;

		foreach (TourPage tourPage in tour.TourPages)
		{
			tourPage.LayoutManager.PerformAutoLayoutForBannerChange();
			tourPage.InvalidateThumbnail();
		}

		bannerUploaded = true;
		tour.HasBanner = true;
		InitPageControls();
	}

	protected override void ImportFromUploadedFile()
	{
		ImportImageFromUploadedFile(new Size(4096, 800), true);

		if (!badFileName)
			Save();
	}

	protected override void ValidatePage()
	{
		ClearErrors();
	
		validBannerUrl = BannerUrlTextBox.Text;
		validBannerUrlTitle = BannerUrlTitleTextBox.Text;
		validBannerUrlOpensWindow = BannerUrlOpensWindowCheckBox.Checked;
	}
}
