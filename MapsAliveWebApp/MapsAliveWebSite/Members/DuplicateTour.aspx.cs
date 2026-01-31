// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.IO;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using Telerik.Web.UI.Upload;

public partial class Members_DuplicateTour : ImportPage
{
	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle("Duplicate Tour");
		SetPageReadOnly();
		SetActionId(MemberPageActionId.DuplicateTour);
		GetSelectedTourOrNone();

		if (!OkToImportTour())
		{
			DuplicateButton.Enabled = false;
			return;
		}

		SetPageMessage(string.Format("Press the Duplicate button below to duplicate {0}", tour.Name));

        if (IsPostBack)
        {
            string eventTarget = Request.Form["__EVENTTARGET"];
            if (eventTarget == "EventOnDuplicate")
                OnDuplicate();
        }
        else
        {
            ProgressMonitor.ShowProgress(ProgressArea, "Duplicating", "Duplicated");
        }
	}

	protected override Importer BeginImport(Stream stream, string reportTitle, string fileExt)
	{
		importer = new ImporterForArchiveXml(null, reportTitle);
		((ImporterForArchiveXml)importer).ImportArchiveFromTempFolder(account, true);
		return importer;
	}

	protected override void EndImport()
	{
		if (importer.ImportFailed)
			ShowReport(ImportReportTable.Panel, ImportReportTable.Title, ImportReportTable.Body);
		else
			Response.Redirect(MemberPageAction.ActionPageTarget(MemberPageActionId.TourManager) + "?duplicated=" + tour.Id);
	}

	protected override Label FileNameLabel
	{
		get { return null; }
	}

	protected void OnDuplicate()
	{
		DuplicateTour(tour);
	}

	private void DuplicateTour(Tour sourceTour)
	{
		ExporterForArchive exporter = new ExporterForArchive(sourceTour);
		string exporterStatusMessage = exporter.CreateArchiveTempFolder(ExcludeHotspotsCheckbox.Checked);

		if (exporterStatusMessage == string.Empty)
		{
			ImportFromLocalData("Duplication of tour " + sourceTour.Id);
		}
		else
		{
			SetPageError(exporterStatusMessage);
		}
	}
}
