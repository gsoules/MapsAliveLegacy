// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.IO;
using System.Web.UI.WebControls;

public partial class Members_ImportArchive : ImportPage
{
	protected override void InitControls(bool undo)
	{
		string script = string.Format("maOnEventRemoteImport(document.getElementById('{0}').value);return false;", RemoteImportTextBox.ClientID);
		RemoteImportButton.Attributes.Add("onclick", script);
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle("Import Tour From Archive File");
		SetPageReadOnly();
		SetActionId(MemberPageActionId.ImportArchive);
		GetSelectedTourOrNone();
		
		InitUploadChoices();

		if (!OkToImportTour())
		{
			AllPanels.Visible = false;
			return;
		}

		ValidFileExtensionsArray = new string[1] { ".zip" };

		if (!IsPostBack)
			ProgressMonitor.ShowImportFileProgress(ProgressArea);
	}

	protected override Importer BeginImport(Stream stream, string reportTitle, string fileExt)
	{
		importer = new ImporterForArchiveXml(stream, reportTitle);
		((ImporterForArchiveXml)importer).ImportArchiveFromStream(MapsAliveState.Account);
		return importer;
	}

	protected override void EndImport()
	{
		bool failed = importer.ImportFailed;
		if (!failed)
			failed = !((ImporterForArchiveXml)importer).ImportedNewTour;

		if (failed)
			ShowReport(ImportReportTable.Panel, ImportReportTable.Title, ImportReportTable.Body);
		else
			Response.Redirect(MemberPageAction.ActionPageTarget(MemberPageActionId.TourManager) + "?imported=1");
	}

	protected override Label FileNameLabel
	{
		get { return FileName; }
	}

	private void InitUploadChoices()
	{
		bool uploadFile = RadioButtonUploadFile.Checked;
		bool remoteUrl = RadioButtonRemoteUrl.Checked;

		UploadFilePanel.Visible = uploadFile;
		RemoteUrlPanel.Visible = remoteUrl;
	}
}