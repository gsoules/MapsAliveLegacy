// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Diagnostics;
using Telerik.Web.UI;
using Telerik.Web.UI.Upload;

public class ProgressMonitor
{
	private static void InitProgressIndicators(RadProgressArea progressArea, string currentOperation, string secondaryOperation, bool uploadingFile)
	{
		ProgressIndicators indicators =
			ProgressIndicators.CurrentFileName |
			ProgressIndicators.TimeElapsed;

		if (uploadingFile)
		{
			// Show the primary progress bar.
			indicators =
				indicators |
				ProgressIndicators.RequestSize |
				ProgressIndicators.TotalProgressBar |
				ProgressIndicators.TotalProgress |
				ProgressIndicators.TotalProgressPercent;
		}

		if (secondaryOperation != null)
		{
			// Show the second progress bar.
			indicators =
				indicators |
				ProgressIndicators.FilesCountBar |
				ProgressIndicators.FilesCountPercent;
		}

		progressArea.ProgressIndicators = indicators;
		
		progressArea.Skin = "Windows7";
		progressArea.Localization.UploadedFiles = secondaryOperation + ": ";
		progressArea.Localization.CurrentFileName = currentOperation + ": ";
	}

	public static void ShowImportFileProgress(RadProgressArea progressArea)
	{
		InitProgressIndicators(progressArea, "Importing", "Imported", true);
	}

	public static void ShowProgress(RadProgressArea progressArea, string currentOperation, string secondaryOperation)
	{
		// Use this method when showing progress for an operation that does not upload a file.
		InitProgressIndicators(progressArea, currentOperation, secondaryOperation, false);
	}

	public static void ShowUploadFileProgress(RadProgressArea progressArea)
	{
		InitProgressIndicators(progressArea, "File", null, true);
	}
	
	public static void Update(string currentOperation, double totalOperations, double totalOperationsPerformed)
	{
		// This method can be called by any page in order to update the progress bar and % completed stats
		// on its progress panel. It's up to the page when to make the call, but the number of calls should
		// be approximately the same as the value of totalOperations so that progress gets to 100%. To use
		// progress monitoring, a page must have this code in its aspx:
		//
		//	<telerik:RadProgressManager id="RadProgressManager" runat="server" />
		//	<telerik:RadProgressArea id="ProgressArea" runat="server" />
		//
		
		Debug.Assert(totalOperations > 0.0, "UpdateProgress totalOperations is " + totalOperations);

		if (currentOperation.Contains(Utility.CrLf))
		{
			// We have seen at least one case where the href value in shape HTML from Adobe Illustrator
			// contained a carriage return. The Telerik progress control choked and displayed an error.
			// There may be other characters that cause the problem, but we prevent the known problem here.
			currentOperation = currentOperation.Replace(Utility.CrLf, " ");
		}

		if (totalOperationsPerformed > totalOperations)
			totalOperationsPerformed = totalOperations;
		double percentProgress = Math.Ceiling((totalOperationsPerformed / totalOperations) * 100);

		RadProgressContext progress = RadProgressContext.Current;
		progress.CurrentOperationText = currentOperation;
		progress.PrimaryPercent = 100;
		progress.SecondaryValue = totalOperationsPerformed;
		progress.SecondaryPercent = percentProgress;
		progress.SecondaryTotal = totalOperations;
	}
}
