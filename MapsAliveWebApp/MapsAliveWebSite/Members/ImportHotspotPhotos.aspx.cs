// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Web.UI.WebControls;

public partial class Members_ImportHotspotPhotos : ImportPage
{
	protected override void InitControls(bool undo)
	{
		UploadFileControl.QuickHelpTitle = Resources.Text.ImportImagesFileLabel;
		UploadFileControl.QuickHelpTopic = "ImportFile";
		UploadFileControl.Usage("Import", "photos", ValidFileExtensionsString);
		UploadFileControl.AddStep("Choose a marker for new hotspots (below, optional)");

		if (tourPage.IsGallery)
		{
			// When importing photos for a gallery, restrict the marker list to only
			// show photo markers. If there are none, then show all markers.
			DataTable markerDataTable = Marker.GetFilteredMarkerList(MarkerFilter.Gallery, tour, tourPage, tourView, account.Id);
			if (markerDataTable.Rows.Count > 0)
				MarkerComboBox.ItemsDataTable = markerDataTable;
		}
		MarkerComboBox.SelectedResourceId = account.LastMarkerIdSelected;
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle(Resources.Text.ImportImagesFileLabel);
		SetPageReadOnly();
		SetActionIdForPageAction(MemberPageActionId.ImportHotspotPhotos);
		GetSelectedTourPage();

		MarkerComboBox.ResourceType = TourResourceType.Marker;

		if (!OkToImportHotspots())
		{
			AllPanels.Visible = false;
			return;
		}

		SetPageSpecialNotice(string.Format("Photos will be imported into <b>{0}</b>", tourPage.Name));

		ValidFileExtensionsArray = new string[1] { ".zip" };
		
		if (!IsPostBack)
			ProgressMonitor.ShowImportFileProgress(UploadFileControl.ProgressArea);
	}

	protected override Importer BeginImport(Stream stream, string reportTitle, string fileExt)
	{
		ImporterForHotspotPhotos importer = new ImporterForHotspotPhotos(tourPage, stream, reportTitle);
		importer.ImportImageFiles(MarkerComboBox.SelectedResourceId);
		return importer;
	}

	protected override void EndImport()
	{
		account.LastMarkerIdSelected = MarkerComboBox.SelectedResourceId;
		ShowReport(ImportReportTable.Panel, ImportReportTable.Title, ImportReportTable.Body);

		if (!importer.ImportFailed)
		{
			tour.RebuildTourTreeXml();

			if (tourPage.IsGallery)
			{
				// The import succeeded so take the user to the Gallery screen.
				string targetPage = MemberPageAction.ActionPageTarget(MemberPageActionId.Gallery);
				Response.Redirect(targetPage);
			}
		}
	}

	protected override Label FileNameLabel
	{
		get { return UploadFileControl.FileNameLabel; }
	}

	protected override void UploadSampleImage(int sampleId)
	{
		string fileLocation = FileManager.WebAppFileLocationAbsolute("images\\samples", "samples.zip");
		ImportFromLocalFile(fileLocation, "Import Sample Gallery Photos");
	}
}
