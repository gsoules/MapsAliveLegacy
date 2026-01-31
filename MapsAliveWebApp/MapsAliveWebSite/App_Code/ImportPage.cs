// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using Telerik.Web.UI.Upload;

public abstract class ImportPage : MemberPage
{
	private string errorMessage = string.Empty;
	protected Importer importer;
	private string reportHtml;
	private UploadedFile uploadedFile;
	private string uploadedFileExt;
	private string uploadedFileName = string.Empty;
	private string[] validFileExtensionsArray;
	private string validFileExtensionsString;

	protected override void PagePreRender()
	{
		base.PagePreRender();

		if (importer == null)
		{
			// There's no importer yet, so ignore this call;
			return;
		}

		Label fileNameLabel = FileNameLabel;
		if (fileNameLabel == null)
		{
			// The subclass does not display a file name.
			return;
		}

		// Display the file name in red if the file could not be imported.
		fileNameLabel.Style.Add(HtmlTextWriterStyle.Color, importer.ImportFailed ? "red" : "black");
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}

	protected abstract Importer BeginImport(Stream stream, string reportTitle, string fileExt);

	private void CreateReport()
	{
		if (importer.Report == null)
			return;

		reportHtml = importer.CreateReportHtml();

		// Save the report so that it can be viewed from the Account > Last Report menu.
		string savedReportHtml = string.Format("<b>&nbsp;{0}</b> ({1:r})<br/><hr/>{2}", importer.Report.Title, DateTime.Now.ToUniversalTime(), reportHtml);
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Account_UpdateLastReport", "@AccountId", account.Id, "@LastReport", savedReportHtml);
		account.HasLastReport = true;
	}
	protected abstract void EndImport();

	private void ImportFinished()
	{
		if (importer == null)
		{
			// We could not create an importer for the data to be imported. This can happen when
			// a very basic error occurs such as file not found or invalid file extension. Create a
			// placeholder importer that subclasses can use to determine that the import failed.
			importer = new Importer(errorMessage);
		}
		else
		{
			// Determine if the import created new hotspots and caused the account limit to be exceeded.
			if (account.HotspotLimitStatus == HotspotLimitStatus.OverLimit)
			{
				// Get the current tour and mark it as over the limit. Note that we get the selected tour
				// instead of using our reference to tour, because in the case of a duplicate or import
				// from archive operation, its the newly created tour that we want to mark. By getting the
				// selected tour, we get the right one.
				Tour offendingTour = MapsAliveState.SelectedTour;
				offendingTour.ExceedsSlideLimit = true;
			}
		}
		
		// Call the subclass to let it know that the import is done.
		EndImport();
	}

	protected abstract Label FileNameLabel { get; }

	protected virtual void ImportFromLocalData(string reportTitle)
	{
		importer = BeginImport(null, reportTitle, string.Empty);
		CreateReport();
		ImportFinished();
	}

	protected virtual void ImportFromLocalFile(string fileLocation, string reportTitle)
	{
		FileStream fileStream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		importer = BeginImport(fileStream, reportTitle, string.Empty);
		CreateReport();
		ImportFinished();
	}

	protected override void ImportFromRemoteUrl(string url)
	{
		RadUploadContext radUploadContext = RadUploadContext.Current;

		if (url.Length > 0)
		{
			try
			{
				// Read data from the remote source.
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();
				Stream responseStream = response.GetResponseStream();

				// Pass the data to the subclass so that it can begin importing.
				importer = BeginImport(responseStream, "Import " + url, string.Empty);
				response.Close();

				CreateReport();
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
			}
		}
		else
		{
			errorMessage = "No URL was provided for the remote data source";
		}

		ImportFinished();
	}

	protected override void ImportFromUploadedFile()
	{
		RadUploadContext radUploadContext = RadUploadContext.Current;

		if (radUploadContext.UploadedFiles.Count == 1)
		{
			uploadedFile = radUploadContext.UploadedFiles[0];
			uploadedFileName = uploadedFile.GetName();
			uploadedFileExt = uploadedFile.GetExtension();
			FileNameLabel.Text = uploadedFileName;

			if (Utility.FileExtensionIsValid(uploadedFileExt, validFileExtensionsArray))
			{
				// Pass the data to the subclass so that it can begin importing.
				importer = BeginImport(uploadedFile.InputStream, "Import " + uploadedFileName, uploadedFileExt.ToLower());

				CreateReport();
			}
			else
			{
				errorMessage = "Only these file types can be imported: " + validFileExtensionsString;
			}
		}
		else
		{
			errorMessage = "No file selected";
			FileNameLabel.Text = string.Empty;
		}

		ImportFinished();
	}

	protected bool OkToImportHotspots()
	{
		bool ok = true;
		string message = string.Empty;

		if (account.HotspotLimitStatus == HotspotLimitStatus.AtLimit)
		{
			// Warn that this import will cause the hotspot limit to be exceeded.
			SetPageSpecialNotice(account.HotspotLimitMessage(HotspotLimitWarningContext.ImportHotspotsAccountAtLimit));
		}
		else if (account.HotspotLimitStatus == HotspotLimitStatus.OverLimit)
		{
			HotspotLimitWarningContext warningContext;
			if (tour.ExceedsSlideLimit)
			{
				// Warn that this tour already exceeds the hotspot limit.
				warningContext = HotspotLimitWarningContext.ImportHotspotsTourOverLimit;
				SetPageSpecialNotice(account.HotspotLimitMessage(warningContext));
			}
			else
			{
				// Warn that another tour exceeds the hotspot limit and this tour cannot be imported into.
				warningContext = HotspotLimitWarningContext.ImportHotspotsAccountOverLimit;
				SetPageSpecialWarning(account.HotspotLimitMessage(warningContext));
				ok = false;
			}
		}
		
		return ok;
	}

	protected bool OkToImportTour()
	{
		bool ok = true;
		string message = string.Empty;

		if (account.HotspotLimitStatus == HotspotLimitStatus.AtLimit)
		{
			SetPageSpecialNotice(account.HotspotLimitMessage(HotspotLimitWarningContext.ImportTourAccountAtLimit));
		}
		else if (account.HotspotLimitStatus == HotspotLimitStatus.OverLimit)
		{
			SetPageSpecialWarning(account.HotspotLimitMessage(HotspotLimitWarningContext.ImportTourAccountOverLimit));
			ok = false;
		}

		return ok;
	}

	protected void ShowReport(Panel reportPanel, Label reportTitleLabel, Label reportAreaLabel)
	{
		if (importer.ImportFailed)
		{
			// An early error (e.g. upload button pressed with no file
			// selected) prevented an importer from being constucted.
			SetPageError(importer.StatusMessage);
		}
		else
		{
			Debug.Assert(importer.Report != null, "Report is null");
			SetPageMessage("Finished (see report at end of page)");
			reportTitleLabel.Text = "Report: " + importer.Report.Title;
			reportAreaLabel.Text = reportHtml;
			reportPanel.Visible = true;
		}
	}

	protected string UploadedFileExt
	{
		get { return uploadedFileExt; }
	}

	protected string[] ValidFileExtensionsArray
	{
		get { return validFileExtensionsArray; }
		set
		{
			validFileExtensionsArray = value;
			validFileExtensionsString = string.Empty;
			foreach (string ext in validFileExtensionsArray)
			{
				if (validFileExtensionsString != string.Empty)
					validFileExtensionsString += ", ";
				validFileExtensionsString += ext;
			}
		}
	}

	protected string ValidFileExtensionsString
	{
		get { return validFileExtensionsString; }
	}
}
