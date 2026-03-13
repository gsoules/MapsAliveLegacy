// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Diagnostics;
using System.Drawing;

public partial class TourViewImage : TourImage
{
	private TourView tourView;

	public Bitmap CachedBitmap { get; set; }
	public Size CachedBitmapSize { get; set; }

	public TourViewImage(TourView tourView, int imageId) : base(tourView.Tour.ThemeId)
	{
		this.tourView = tourView;
		this.id = imageId;

		if (imageId != 0)
		{
			MapsAliveDataRow row = MapsAliveDatabase.LoadDataRow(
				"sp_TourViewImage_GetImageMetaData", "@TourViewId", tourView.Id, "@ImageId", imageId, "@ThemeId", themeId);

			if (row == null)
			{
				// This should never happen, but seems to for some older tours. To
				// avoid problems, we simply handle the error by setting the image
				// id to 0. See comment below re originalFileName length for more info.
				id = 0;
			}
			else
			{
				InitializeFromDataRow(row);

				if (originalFileName.Length == 0)
				{
					// This logic should only execute for tours that were created prior to
					// version 2.0. We used to create an image placeholder for every slide
					// and then replaced it after the user uploaded an image. We no longer
					// need or want those placeholders so we set the image id to 0 to tell
					// the view it has no image and that the placeholder image record
					// should be removed from the database.
					id = 0;
				}
			}
		}
	}

	public override string MissingImageText
	{
		get { return MapsAliveTourBuilder.Text.PreviewMissingViewImageHelp; }
	}

	public override void DeleteImageInDatabase()
	{
		ExecuteStoredProcedureForDeleteImage("sp_TourViewImage_DeleteImage", "@TourViewId", tourView.Id);
		CachedBitmap = null;
	}

	public override void InsertImageIntoDatabase()
	{
		ExecuteStoredProcedureForCreateImage("sp_TourViewImage_CreateImage", tourView.Tour.Id, "@TourViewId", tourView.Id);
		CachedBitmap = null;
	}

	public override void UpdateImageInDatabase()
	{
		ExecuteStoredProcedureForUpdateImage("sp_TourViewImage_UpdateImage", "@TourViewId", tourView.Id);
		CachedBitmap = null;
	}

	public override void UpdateImageVersionInDatabase()
	{
		ExecuteStoredProcedureForUpdateImageVersion("sp_TourViewImage_UpdateImageVersion", "@TourViewId", tourView.Id);
		CachedBitmap = null;
	}
}
