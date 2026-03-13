// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class BannerImage : TourImage
{
	private Tour tour;

	public BannerImage(Tour tour, int imageId) : base(tour.ThemeId)
	{
		UsesThumbnail = false;

		this.tour = tour;
		this.id = imageId;

		if (imageId != 0)
		{
			MapsAliveDataRow row = MapsAliveDatabase.LoadDataRow(
				"sp_BannerImage_GetImageMetaData", "@TourId", tour.Id, "@ImageId", imageId, "@ThemeId", themeId);
			InitializeFromDataRow(row);
		}
	}

	#region ===== Properties ========================================================

	public override string MissingImageText
	{
		get { return MapsAliveTourBuilder.Text.PreviewMissingBannerImageHelp; }
	}
	#endregion

	#region ===== Public ============================================================

	public override void DeleteImageInDatabase()
	{
		ExecuteStoredProcedureForDeleteImage("sp_BannerImage_DeleteImage", "@TourId", tour.Id);
	}

	public override void InsertImageIntoDatabase()
	{
		ExecuteStoredProcedureForCreateImage("sp_BannerImage_CreateImage", tour.Id, "@KeyId", tour.Id);
	}

	public override void UpdateImageInDatabase()
	{
		ExecuteStoredProcedureForUpdateImage("sp_BannerImage_UpdateImage", "@TourId", tour.Id);
	}

	public override void UpdateImageVersionInDatabase()
	{
		ExecuteStoredProcedureForUpdateImageVersion("sp_BannerImage_UpdateImageVersion", "@TourId", tour.Id);
	}

	#endregion

	#region ===== Protected =========================================================
	#endregion

	#region ===== Private ===========================================================
	#endregion
}
