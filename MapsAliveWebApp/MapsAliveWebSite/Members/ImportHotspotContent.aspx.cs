// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.IO;
using System.Web.UI.WebControls;

public partial class Members_ImportHotspotContent : ImportPage
{
	protected override void InitControls(bool undo)
	{
		MarkerComboBox.ResourceType = TourResourceType.Marker;
		MarkerComboBox.SelectedResourceId = account.LastMarkerIdSelected;

		RemoteImportTextBox.Text = tour.RemoteImportUrl; ;

		string script = string.Format("maOnEventRemoteImport(document.getElementById('{0}').value);return false;", RemoteImportTextBox.ClientID);
		RemoteImportButton.Attributes.Add("onclick", script);
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle(Resources.Text.ImportContentFileLabel);
		SetPageReadOnly();
		SetActionIdForPageAction(MemberPageActionId.ImportHotspotContent);
		GetSelectedTour();

		if (!OkToImportHotspots())
		{
			AllPanels.Visible = false;
			return;
		}

		InitUploadChoices();

		MarkerComboBox.ResourceType = TourResourceType.Marker;

		ValidFileExtensionsArray = new string[4] { ".csv", ".xls", ".xlsx", ".xml" };

		if (!IsPostBack)
			ProgressMonitor.ShowImportFileProgress(ProgressArea);
	}

	protected override Importer BeginImport(Stream stream, string reportTitle, string fileExt)
	{
		ImporterForContentXml importer = new ImporterForContentXml(tour, stream, reportTitle);
		importer.ImportContentXml(MarkerComboBox.SelectedResourceId, ImportMarkerLocations.Checked);
		return importer;
	}

	protected override void EndImport()
	{
		account.LastMarkerIdSelected = MarkerComboBox.SelectedResourceId;

		ShowReport(ImportReportTable.Panel, ImportReportTable.Title, ImportReportTable.Body);

		if (importer.ImportFailed)
			return;

		if (tourPage != null)
			tourPage.TourViewChanged();
		tour.RebuildTourTreeXml();
	}

	protected override Label FileNameLabel
	{
		get { return FileName; }
	}

	protected override void ImportFromRemoteUrl(string url)
	{
		if (url.Length > 0 && url != tour.RemoteImportUrl)
		{
			// Save the URL so it will be there the next time the user comes to the Import page.
			tour.RemoteImportUrl = url;
			tour.UpdateDatabase();
		}

		base.ImportFromRemoteUrl(url);
	}

	private void InitUploadChoices()
	{
		if (!IsPostBack)
		{
			bool useUrl = tour.RemoteImportUrl != string.Empty;
			RadioButtonUploadFile.Checked = !useUrl;
			RadioButtonRemoteUrl.Checked = useUrl;
		}
		
		bool uploadFile = RadioButtonUploadFile.Checked;
		bool remoteUrl = RadioButtonRemoteUrl.Checked;

		UploadFilePanel.Visible = uploadFile;
		RemoteUrlPanel.Visible = remoteUrl;
	}
}
