// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.SessionState;
using System.Xml;

// This HTTP handler acts as a traffic cop to determine which screen should appear
// in situations where the choice of the next screen depends on the current currentState
// of the app. Most logic in this handler excutes inbetween the origin page (the
// one that invoked this handler) and the target page. The exception is Tour Preview
// actions that update the database and stay on the Tour Preview page.
//
// Without this  ashx handler, we would need to use aspx pages that contained
// code-behind logic, but no user interface which is not a nice model. From an
// architecture perspective it would be nice to have multiple handlers containing
// limited logic -- rather than this kitchen sink handler -- but using a single
// handler limits how many handler entries have to be managed in web.config and 
// prevents multiple implemenations of the same general model that is coded here.

public class PerformAction : IHttpHandler, IRequiresSessionState
{
	private MemberPageActionId actionId;
	private HttpContext context;
	private string queryArgs;
	private NameValueCollection queryString;
	private Tour tour;
	private TourPage tourPage;
	private TourView tourView;
	private MemberPageActionId targetPageActionId;

	public void ProcessRequest(HttpContext context)
	{
		try
		{
			this.context = context;
			tour = MapsAliveState.SelectedTourOrNull;
			GetAction();

			if (IsTourPreviewAction)
			{
				PerformTourPreviewAction();

				// Reload the Tour Preview screen in order to refresh the Tour Advisor and other messages.
				targetPageActionId = MemberPageActionId.TourPreview;
			}
			else if (IsExportAction)
			{
				if (PerformExportAction())
				{
					// Return to the ExportTourOverLimit screen without posting. We can't postback because doing
					// so would interrupt the download.
					return;
				}
				else
				{
					// Fall through to transfer to the ExportTourOverLimit screen and display a message.
					targetPageActionId = MemberPageActionId.Export;
				}
			}
			else
			{
				PerformTransferToAnotherPageAction();
				targetPageActionId = actionId;
			}

			TransferToNextPage();

		}
		catch (Exception ex)
		{
			Utility.ReportException("PerformAction: " + targetPageActionId, ex);
		}
	}

	public bool IsReusable
	{
		get { return true; }
	}

	private void TransferToNextPage()
	{
		if (targetPageActionId == MemberPageActionId._NotSet)
		{
			// This should never happen unless you type PerformAction.ashx on the query string,
			// but we have seen it occur so we trap it here and go to the home page.
			Debug.Fail("PerformAction called when action Id is not set");
			targetPageActionId = MemberPageActionId.TourManager;
		}

		string targetAspxPage = MemberPageAction.ActionPageTarget(targetPageActionId) + queryArgs;
		Debug.Assert(targetAspxPage != null, "No page found for " + actionId);

		// See: http://blogs.msdn.com/tmarq/archive/2009/06/25/correct-use-of-system-web-httpresponse-redirect.aspx.
		// If we do a vanilla redirect without adding the false parameter and without calling CompleteRequest, a
		// ThreadAbort exception occurs which apparently has a tremendous amount of overhead associated with it.
		context.Response.Redirect("~/Members/" + targetAspxPage, false);
		context.ApplicationInstance.CompleteRequest();
	}

	private void GetAction()
	{
		queryString = context.Request.QueryString;

		// Get the action to be performed.
		int aid = 0;
		int.TryParse(queryString["aid"], out aid);
		actionId = (MemberPageActionId)aid;
		if (!Enum.IsDefined(typeof(MemberPageActionId), actionId))
			actionId = MemberPageActionId.TourExplorer;
		
		// Get the tour page and/or view that is relevant to this action.
		if (tour != null)
		{
			int pid = 0;
			int.TryParse(queryString["pid"], out pid);
			if (pid != 0)
			{
				tourPage = tour.GetTourPage(pid);
				if (tourPage == null)
					actionId = MemberPageActionId.TourManager;

				int vid = 0;
				int.TryParse(queryString["vid"], out vid);
				if (vid != 0)
				{
					tourView = tourPage.GetTourView(vid);
					if (tourView == null)
						actionId = MemberPageActionId.TourManager;
				}
			}
		}
	}

	private bool IsExportAction
	{
		get
		{
			return
				actionId == MemberPageActionId.ExportArchive ||
				actionId == MemberPageActionId.ExportArchiveFullSize ||
				actionId == MemberPageActionId.ExportContentCsv ||
				actionId == MemberPageActionId.ExportContentXml ||
				actionId == MemberPageActionId.ExportImages ||
				actionId == MemberPageActionId.ExportPublishedTour ||
				actionId == MemberPageActionId.ExportResources ||
				actionId == MemberPageActionId.ExportResourcesAll;
		}
	}

	private bool IsTourPreviewAction
	{
		get
		{
			return
				actionId == MemberPageActionId.DeleteMap ||
				actionId == MemberPageActionId.DeleteHotspot ||
				actionId == MemberPageActionId.DisableMapZoom ||
				actionId == MemberPageActionId.EnableMapZoom ||
				actionId == MemberPageActionId.PublishTour;
		}
	}

	private void PerformTransferToAnotherPageAction()
	{
		switch (actionId)
		{
			case MemberPageActionId.TourBuilder:
				SetTourBuilderAction();
				break;

			case MemberPageActionId.EditHotspotContent:
				SetSelectedView();
				break;

			case MemberPageActionId.AddHotspot:
				queryArgs = string.Format("?post={0}", MapsAliveState.PostId);
				SetSelectedPage();
				break;

			case MemberPageActionId.GallerySetup:
			case MemberPageActionId.GallerySetupNew:
			case MemberPageActionId.Map:
			case MemberPageActionId.MapOptionsAdvanced:
			case MemberPageActionId.MapSetup:
			case MemberPageActionId.MapSetupNew:
			case MemberPageActionId.UploadMap:
				SetSelectedPage();
				break;

			case MemberPageActionId.CreateFontStyle:
			case MemberPageActionId.CreateMarker:
			case MemberPageActionId.CreateMarkerStyle:
			case MemberPageActionId.CreateSymbol:
			case MemberPageActionId.CreateTooltipStyle:
			case MemberPageActionId.CreateColorScheme:
				CreateNewResource();
				break;

			default:
				break;
		}
	}

	private bool PerformExportAction()
	{
		try
		{
			string downloadFileName = string.Empty;
			string statusMessage = string.Empty;
			string fileLocation = string.Empty;
			string contentType = string.Empty;

			queryArgs = string.Empty;

			switch (actionId)
			{
				case MemberPageActionId.ExportArchive:
				case MemberPageActionId.ExportArchiveFullSize:
					{
						contentType = "application/x-zip-compressed";
						ExporterForArchive exporter = new ExporterForArchive(tour);
						downloadFileName = string.Format("MapsAliveArchiveForTour{0}.zip", tour.Id);
						fileLocation = ExportFileLocation(downloadFileName);
						bool exportOriginalImagesSizes = actionId == MemberPageActionId.ExportArchiveFullSize;
						exporter.CreateArchiveZipFile(fileLocation, exportOriginalImagesSizes);
						tour.ArchiveFileCreated();
						break;
					}

				case MemberPageActionId.ExportContentCsv:
					{
						contentType = "application/octet-stream";
						ExporterForContent exporter = new ExporterForContent(tour);
						downloadFileName = string.Format("MapsAliveContentForTour{0}.csv", tour.Id);
						fileLocation = ExportFileLocation(downloadFileName);
						exporter.CreateContentCsvFile(fileLocation);
						break;
					}

				case MemberPageActionId.ExportContentXml:
					{
						contentType = "text/xml";
						ExporterForContent exporter = new ExporterForContent(tour);
						downloadFileName = string.Format("MapsAliveContentForTour{0}.xml", tour.Id);
						fileLocation = ExportFileLocation(downloadFileName);
						exporter.CreateContentXmlFile(fileLocation);
						break;
					}

				case MemberPageActionId.ExportImages:
					{
						contentType = "application/x-zip-compressed";
						ExporterForArchive exporter = new ExporterForArchive(tour);
						downloadFileName = string.Format("MapsAliveImagesForTour{0}.zip", tour.Id);
						fileLocation = ExportFileLocation(downloadFileName);
						statusMessage = exporter.CreateImagesZipFile(fileLocation);
						break;
					}

				case MemberPageActionId.ExportPublishedTour:
					{
						contentType = "application/x-zip-compressed";
						TourBuilder tourBuilder = new TourBuilder(tour);
						tourBuilder.CreateDownloadFile();
						downloadFileName = tour.DownloadFileName;
						break;
					}

				case MemberPageActionId.ExportResources:
				case MemberPageActionId.ExportResourcesAll:
					{
						contentType = "application/x-zip-compressed";
						ExporterForArchive exporter = new ExporterForArchive(tour);
						if (actionId == MemberPageActionId.ExportResourcesAll)
							downloadFileName = string.Format("MapsAliveResourcesForAccount{0}.zip", Utility.AccountId);
						else
							downloadFileName = string.Format("MapsAliveResourcesForTour{0}.zip", tour.Id);
						fileLocation = ExportFileLocation(downloadFileName);
						statusMessage = exporter.CreateResourcesZipFile(fileLocation, actionId == MemberPageActionId.ExportResourcesAll);
						break;
					}

				default:
					Debug.Fail("No export handler for export action " + actionId);
					break;
			}

			if (statusMessage != string.Empty)
			{
				if (queryArgs == string.Empty)
					queryArgs = "?failed=1";
				MapsAliveState.Persist(MapsAliveObjectType.ExportTrace, statusMessage);
				return false;
			}
			else
			{
				fileLocation = string.Format("{0}\\{1}", FileManager.PreviewFolderLocationAbsolute(tour.Id), downloadFileName);
				context.Response.ContentType = contentType;
				context.Response.AppendHeader("content-disposition", string.Format("attachment; filename={0}", downloadFileName));
				context.Response.TransmitFile(fileLocation);
				MapsAliveState.Persist(MapsAliveObjectType.ExportTrace, actionId.ToString());
			}
		}
		catch (Exception ex)
		{
			Utility.ReportException("PerformExportAction " + actionId, ex);
			return false;
		}

		return true;
	}

	private string ExportFileLocation(string fileName)
	{
		return Path.Combine(FileManager.PreviewFolderLocationAbsolute(tour.Id), fileName);
	}

	private void PerformTourPreviewAction()
	{
		// Get the action Id of the tour builder page that the Tour Preview is suppossed
		// to return to when the user clicks Return to Tour Builder.  We have to capture
		//it here so we can pass it back to the preview page.
		int tbaid = 0;
		int.TryParse(queryString["tbaid"], out tbaid);
		queryArgs = string.Format("?aid={0}&page={1}", tbaid, tour.SelectedTourPage.PageNumber);

		switch (actionId)
		{
			case MemberPageActionId.DeleteMap:
				if (tourPage != null)
					tourPage.Delete();
				break;

			case MemberPageActionId.DeleteHotspot:
				if (tourView != null)
					tourView.Delete();
				break;

			case MemberPageActionId.DisableMapZoom:
			case MemberPageActionId.EnableMapZoom:
				tourPage.MapCanZoom = actionId == MemberPageActionId.EnableMapZoom;
				tourPage.UpdateDatabase();
				break;

			case MemberPageActionId.PublishTour:
				int publishCode;
				try
				{
					TourBuilder tourBuilder = new TourBuilder(tour);
					publishCode = tourBuilder.PublishTour() ? 1 : 2;
				}
				catch (Exception ex)
				{
					publishCode = 2;
					Utility.ReportException("PublishTour", ex);
				}
				queryArgs += string.Format("&pub={0}&rev={1}", publishCode, App.Revision);
				break;


			default:
				Debug.Fail("No tour preview action handler for action Id " + actionId);
				break;
		}
	}

	private void CreateNewResource()
	{
		// Create a new resource or duplicate an existing resource.
		// We don't verify that the resource type and Id are valid because the caller is trusted.
		// Calls initiate via menu items that provide a query string to PerformAction.ashx.

		// Get the resource type.
		int rt = 0;
		int.TryParse(queryString["rt"], out rt);
		TourResourceType resourceType = (TourResourceType)rt;

		// Get the resource Id. Zero means create a new reource. Non-zero means duplicate an existing resource.
		int resourceId = 0;
		int.TryParse(queryString["id"], out resourceId);

		// Determine what kind of duplicate action to use to create this resource.
		ResourceDuplicateAction duplicateAction = resourceId == 0 ? ResourceDuplicateAction.CreateNewResource : ResourceDuplicateAction.CopyExistingResource;

		Account account = MapsAliveState.Account;

		if (resourceId == 0)
		{
			// Create a new resource by duplicating a default resource.
			resourceId = account.DefaultResourceId(resourceType);
		}

		// Duplicate the selected resource and insert it into the database.
		TourResource newResource = TourResource.DuplicateResourceInDatabase(account.Id, resourceType, resourceId, duplicateAction);
		Marker marker;

		// Set the action and query string to transfer to the resource's editor page.
		queryArgs = string.Format("?id={0}", newResource.Id);
		switch (resourceType)
		{
			case TourResourceType.FontStyle:
				actionId = MemberPageActionId.EditFontStyle;
				break;

			case TourResourceType.Marker:
				actionId = MemberPageActionId.EditMarker;
				marker = (Marker)newResource;
				if (marker.IsExclusive)
				{
					// A copy of an exclusive marker must be non-exclusive. Otherwise the
					// cloned marker's tour view would have two markers associated with it.
					marker.MakeNonExclusive();
					TourResource.CreateResourceImageFile(TourResourceType.Marker, marker.Id, string.Empty, ResourceImageFileAction.CreateNewFile);
				}
				int tourViewId = 0;
				int.TryParse(queryString["vid"], out tourViewId);
				if (tourViewId != 0)
				{
					TourView tourView = tour.SelectedTourView;
					tourView.MarkerId = newResource.Id;
					tourView.UpdateDatabase();
				}
				break;

			case TourResourceType.MarkerStyle:
				actionId = MemberPageActionId.EditMarkerStyle;
				int markerId = 0;
				int.TryParse(queryString["mid"], out markerId);
				if (markerId != 0)
				{
					marker = Account.GetCachedMarker(markerId);
					marker.MarkerStyleId = newResource.Id;
					marker.UpdateDatabase();
				}
				break;

			case TourResourceType.Symbol:
				actionId = MemberPageActionId.EditSymbol;
				break;

			case TourResourceType.TooltipStyle:
				actionId = MemberPageActionId.EditTooltipStyle;
				break;

			case TourResourceType.TourStyle:
				actionId = MemberPageActionId.EditColorScheme;
				break;
		}
	}

	private void SetSelectedPage()
	{
		if (tourPage != null)
			tourPage = tour.SetSelectedTourPage(tourPage.Id);
	}

	private void SetSelectedView()
	{
		if (tourView != null)
			tourView = tour.SetSelectedTourView(tourView.Id);
	}

	private void SetTourBuilderAction()
	{
		Account account = MapsAliveState.Account;
		if (account == null)
		{
			// This does not normally happen, but it can if for example the browser crashes and then
			// restores itself on a tour builder page, but without the user still being logged in.
			actionId = MemberPageActionId.HomePage;
			return;
		}

		bool showAnnouncements = account.AnnouncementId < App.AnnouncementId;

		if (showAnnouncements)
		{
			actionId = MemberPageActionId.Announcements;
		}
		else
		{
			bool hasRecentTour = MapsAliveState.SelectedTourOrNull != null;

			if (hasRecentTour)
			{
				object id = MapsAliveState.Retrieve(MapsAliveObjectType.TourBuilderActionId);
				actionId = id == null || (MemberPageActionId)id == MemberPageActionId.Announcements ? MemberPageActionId.TourManager : (MemberPageActionId)id;
			}
			else
			{
				actionId = MemberPageActionId.TourExplorer;
			}
		}

		// Indicate that this is a return to the Tour Builder. The tb arg tells the returned-to
		// page that the Tour Builder is being is returned to from outside the Tour Builder and
		// that it needs to restore its Tour Builder currentState.
		queryArgs = "?tb=1";
	}
}