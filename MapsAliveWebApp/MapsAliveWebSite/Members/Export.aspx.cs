// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class Members_Export : MemberPage
{

	protected override void InitControls(bool undo)
	{
		// Export Published Tour
		string help = AppContent.TopicHtml("ExportPublishedTourHelp");
		if (tour.IsPrivate)
		{
			ExportPublishedTourControl.Title = "Download Private Tour";
			help = help.Replace("ublished", "rivate");
		}
		Utility.SetDivText(ExportPublishedTourHelp, help);
		ExportPublishedTourControl.OnClickJavaScript = OnClickScript(MemberPageActionId.ExportPublishedTour);
		string kind = tour.IsPrivate ? "private" : "published";
		if (tour.ExceedsSlideLimit)
			ExportPublishedTourControl.AppearsEnabled = false;
		else if (account.IsTrial && !Utility.UserIsAdmin)
			ExportPublishedTourControl.ErrorMessage = AppContent.TopicHtml("ExportPublishedDenyNotPaid");
		else if (!tour.HasBeenPublished && !tour.IsPrivate)
			ExportPublishedTourControl.ErrorMessage = AppContent.TopicHtml("ExportPublishedDenyNotPublished");
		else if (tour.HasChangedSinceLastPublished && !tour.IsPrivate)
			ExportPublishedTourControl.WarningMessage = string.Format(AppContent.TopicHtml("ExportPublishedTourChangedConfirm"), kind);
		else
			ExportPublishedTourControl.WarningMessage = string.Format(AppContent.TopicHtml("ExportPublishedTourConfirm"), kind);
		
		// Export Archive
		Utility.SetDivText(ExportArchiveHelp, AppContent.TopicHtml("ExportArchiveHelp"));
		ExportArchiveControl.OnClickJavaScript = OnClickScript(MemberPageActionId.ExportArchiveFullSize);
		if (tour.ExceedsSlideLimit)
			ExportArchiveControl.AppearsEnabled = false;
		else if (account.IsTrial && !Utility.UserIsAdmin)
			ExportArchiveControl.ErrorMessage = AppContent.TopicHtml("ExportArchiveNotPaid");
		else if (!account.IsProPlan && !Utility.UserIsAdmin)
			ExportArchiveControl.ErrorMessage = Account.RequiresPlanMessage("Arhive Tour", AccountPlan.Pro);
		else
			ExportArchiveControl.WarningMessage = AppContent.TopicHtml("ExportArchiveConfirm");

		// Export Content CSV
		Utility.SetDivText(ExportContentCsvHelp, AppContent.TopicHtml("ExportContentCsvHelp"));
		if (ExportContentCsvControl.Enabled)
		{
			if (account.IsPlusOrProPlan)
			{
				ExportContentCsvControl.OnClickJavaScript = OnClickScript(MemberPageActionId.ExportContentCsv);
				ExportContentCsvControl.WarningMessage = AppContent.TopicHtml("ExportContentCsvConfirm");
			}
			else
			{
				ExportContentCsvControl.ErrorMessage = Account.RequiresPlanMessage("Export Tour Content to CSV", AccountPlan.Plus);
			}
		}

		// Export Content XML
		Utility.SetDivText(ExportContentXmlHelp, AppContent.TopicHtml("ExportContentXmlHelp"));
		if (account.IsPlusOrProPlan)
		{
			ExportContentXmlControl.OnClickJavaScript = OnClickScript(MemberPageActionId.ExportContentXml);
			ExportContentXmlControl.WarningMessage = AppContent.TopicHtml("ExportContentXmlConfirm");
		}
		else
		{
			ExportContentXmlControl.ErrorMessage = Account.RequiresPlanMessage("Export Tour Content to XML", AccountPlan.Plus);
		}

		// Export Resources
		Utility.SetDivText(ExportResourcesHelp, AppContent.TopicHtml("ExportResourcesHelp"));
		if (account.IsProPlan)
		{
			ExportResourcesControl.OnClickJavaScript = OnClickScript(MemberPageActionId.ExportResources);
			ExportResourcesControl.WarningMessage = AppContent.TopicHtml("ExportResourcesConfirm");
		}
		else
		{
			ExportResourcesControl.ErrorMessage = Account.RequiresPlanMessage("Export Tour Resources", AccountPlan.Pro);
		}

		// Export All Resources
		if (account.IsProPlan)
		{
			ExportAllResourcesControl.OnClickJavaScript = OnClickScript(MemberPageActionId.ExportResourcesAll);
			ExportAllResourcesControl.WarningMessage = AppContent.TopicHtml("ExportResourcesAllConfirm");
		}
		else
		{
			ExportAllResourcesControl.ErrorMessage = Account.RequiresPlanMessage("Export Account Resources", AccountPlan.Pro);
		}

		// Export Content Images
		if (Utility.UserIsAdmin)
		{
			Utility.SetDivText(ExportImagesHelp, AppContent.TopicHtml("ExportImagesHelp"));
			ExportImagesControl.OnClickJavaScript = OnClickScript(MemberPageActionId.ExportImages);
			ExportImagesControl.WarningMessage = AppContent.TopicHtml("ExportImagesConfirm");
		}
		else
		{
			ExportImagesPanel.Visible = false;
		}
	}

	private string OnClickScript(MemberPageActionId actionId)
	{
		return "maHandleExportEvent(" + (int)actionId + ");";
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle("Export");
		SetPageReadOnly();
		SetActionId(MemberPageActionId.Export);
		GetSelectedTour();

		if (Request.QueryString["failed"] == "1")
		{
			string message = (string)MapsAliveState.Retrieve(MapsAliveObjectType.ExportTrace);
			MapsAliveState.Flush(MapsAliveObjectType.ExportTrace);
			if (Utility.UserIsMapsAlive)
			{
				SetPageError(message);
			}
			else
			{
				SetPageError("Export failed. Please report this problem to support@mapsalive.com");
			}
		}
		else if (Request.QueryString["duplicated"] == "1")
		{
			SetPageMessage("Tour was duplicated");
		}

		if (tour.ExceedsSlideLimit)
		{
			SetPageSpecialWarning(account.HotspotLimitMessage(HotspotLimitWarningContext.ExportTourOverLimit));
		}
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}
}
