// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class MapImage : TourImage
{
	private TourPage tourPage;

	public MapImage(TourPage tourPage, int imageId) : base(tourPage.Tour.ThemeId)
	{
		this.tourPage = tourPage;
		this.id = imageId;

		if (imageId != 0)
		{
			MapsAliveDataRow row = MapsAliveDatabase.LoadDataRow(
				"sp_TourPageImage_GetImageMetaData", "@TourPageId", tourPage.Id, "@ImageId", imageId, "@ThemeId", themeId);
			InitializeFromDataRow(row);
		}
	}

	public override string MissingImageText
	{
		get { return MapsAliveTourBuilder.Text.PreviewMissingMapImageHelp; }
	}

	public override void DeleteImageInDatabase()
	{
		ExecuteStoredProcedureForDeleteImage("sp_TourPageImage_DeleteImage", "@TourPageId", tourPage.Id);
	}

	public override void InsertImageIntoDatabase()
	{
		ExecuteStoredProcedureForCreateImage("sp_TourPageImage_CreateImage", tourPage.Tour.Id, "@TourPageId", tourPage.Id);
	}

	public override void UpdateImageInDatabase()
	{
		ExecuteStoredProcedureForUpdateImage("sp_TourPageImage_UpdateImage", "@TourPageId", tourPage.Id);
	}

	public override void UpdateImageVersionInDatabase()
	{
		ExecuteStoredProcedureForUpdateImageVersion("sp_TourPageImage_UpdateImageVersion", "@TourPageId", tourPage.Id);
	}
}
