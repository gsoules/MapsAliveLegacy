// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Diagnostics;
using System.IO;
using System.Web;
using Telerik.Web.UI;
using Telerik.Web.UI.Upload;

public class Importer
{
	protected bool importFailed;
	protected bool importTerminated;
	protected string message;
	protected ImportReport report;
	protected Stream stream;
	protected Tour tour;
	protected TourPage tourPage;

	public Importer(string errorMessage)
	{
		importFailed = true;
		message = errorMessage;
	}

	public Importer(Tour tour, TourPage tourPage, Stream stream, string reportTitle)
	{
		this.tour = tour;
		this.tourPage = tourPage;
		this.stream = stream;
		
		report = new ImportReport(reportTitle);
		message = string.Empty;
	}

	public string CreateReportHtml()
	{
		// Remember where we are in the report before actual report info is written.
		int emptyReportLength = report.Rows.Length;

		report.EmitSection(ImportReport.Topic.ImageFileRejected);
		report.EmitSection(ImportReport.Topic.AreaTagRejected);
		report.EmitSection(ImportReport.Topic.MarkerImported);
		report.EmitSection(ImportReport.Topic.MarkerUpdated);
		report.EmitSection(ImportReport.Topic.SlideDeleted);
		report.EmitSection(ImportReport.Topic.SlideImported);
		report.EmitSection(ImportReport.Topic.SlideUpdated);
		report.EmitSection(ImportReport.Topic.SlideUnchanged);
		report.EmitSection(ImportReport.Topic.MarkerUnchanged);
		report.EmitSection(ImportReport.Topic.RoutesImported);
		report.EmitSection(ImportReport.Topic.RoutesRejected);
		report.EmitSection(ImportReport.Topic.RoutesUnresolved);
		report.EmitSection(ImportReport.Topic.UserImported);
		report.EmitSection(ImportReport.Topic.UserRejected);
		report.EmitSection(ImportReport.Topic.UserUnchanged);
		report.EmitSection(ImportReport.Topic.UserUpdated);
		report.EmitSection(ImportReport.Topic.ResourceImported);

		if (importTerminated)
		{
			report.EmitRow(ImportReport.Topic.HotspotLimitReached, AppContent.Topic("HelpImportSlideLimitReached"));
			report.EmitSection(ImportReport.Topic.HotspotLimitReached);
		}

		string reportText = string.Empty;

		if (message.Length > 0)
			reportText = string.Format("<div class='textErrorMessage'>{0}</div>", message);

		if (report.Rows.Length == emptyReportLength)
			report.EmitSection(ImportReport.Topic.NothingImported);

		// The trace always appears last.
		report.EmitSection(ImportReport.Topic.Trace);

		reportText += report.Rows;

		return reportText;
	}

	public bool ImportFailed
	{
		get { return importFailed; }
	}

	public bool ImportTerminated
	{
		get { return importTerminated; }
	}

	protected bool OkToKeepImporting
	{
		get { return HttpContext.Current.Response.IsClientConnected; }
	}

	public ImportReport Report
	{
		get { return report; }
	}

	public string StatusMessage
	{
		get { return message; }
	}
}
