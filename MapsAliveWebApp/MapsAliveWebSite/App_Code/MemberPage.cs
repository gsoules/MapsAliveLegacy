// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Diagnostics;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using AvantLogic.MapsAlive;
using AvantLogic.MapsAlive.Engine;

public abstract class MemberPage : System.Web.UI.Page
{
	protected Account account;
	protected Category category;
	private bool cannotDeleteResource;
	protected bool categoryFilterChanged;
	private bool deleteSucceeded;
	protected bool fieldValid;
	protected bool isValidPost;
	protected Marker marker;
	protected MarkerStyle markerStyle;
	protected Navigation masterPage;
	protected PageUsage pageUsage;
	protected bool pageValid;
	protected Symbol symbol;
	protected TooltipStyle tooltipStyle;
	protected FontStyleResource fontStyleResource;
	protected Tour tour;
	protected TourResource resourceBeforeEdit;
	protected ColorScheme colorScheme;
	protected TourPage tourPage;
	protected TourView tourView;
	private bool okToTransferToAnotherPage;

	public MemberPage()
	{
		pageValid = true;
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		account = MapsAliveState.Account;
		SetResourceFiltering();
		SetPageUsage();
		PageLoad();
		ShowStatusBox();
		HandlePostEvent();
		EmitJavaScript();
		PageLoaded();
		
		masterPage.MasterPageHeader.PageIsValid = pageValid;
		masterPage.MasterPageHeader.Tour = tour;
		masterPage.MasterPageHeader.TourPage = tourPage;

		// Check if an HttpRequestValidationException was handled in Global.asax.
		// It can occur if a user types HTML characters like '<' in a text box.
		// The exception is designed to protect against script injection. Pages that
		// need to allow these characters have ValidateRequest="false" in the <%@ Page
		// section at the top of the .aspx file. Other pages ignore the attempt to save
		// the text and display the message below. Not a great solution, but at least
		// the user will know something is wrong instead of wondering why the save failed.
		if (MapsAliveState.Retrieve(MapsAliveObjectType.RequestValidationErrorHandled) != null)
		{
			MapsAliveState.Flush(MapsAliveObjectType.RequestValidationErrorHandled);
			SetPageError("Save failed because a field contains HTML characters such as '<' that are not allowed in that field.");
		}

		masterPage.InitNavigation(this);

		MaintainScrollPositionOnPostBack = true;

		okToTransferToAnotherPage = true;
	}

	protected void Page_PreRender(object sender, EventArgs e)
	{
		PagePreRender();
	}

	public bool BrowserIsIE6
	{
		get
		{
			HttpBrowserCapabilities browser = Request.Browser;
			return browser.Browser == "IE" && browser.MajorVersion < 7;
		}
	}

	public FontStyleResource FontStyleResource
	{
		get { return fontStyleResource; }
	}

	public Category Category
	{
		get { return category; }
	}

	public static void InitShowUsageControl(AvantLogic.MapsAlive.MemberPageActionButton control, TourResource resource)
	{
		control.OnClickActionId = MemberPageActionId.TourResourceDependencyReport;
		control.Title = "Usage";
		control.QueryString = string.Format("?rt={0}&id={1}", (int)resource.ResourceType, resource.Id);
	}

	protected bool IsReturnFromTourPreview
	{
		get { return Request.QueryString["tp"] == "1"; }
	}

	protected bool IsReturnToTourBuilder
	{
		get { return Request.QueryString["tb"] == "1"; }
	}

	public Marker Marker
	{
		get { return marker; }
	}

	public MarkerStyle MarkerStyle
	{
		get { return markerStyle; }
	}

	public PageUsage PageUsage
	{
		get { return pageUsage; }
	}

	public Symbol Symbol
	{
		get { return symbol; }
	}

	public TooltipStyle TooltipStyle
	{
		get { return tooltipStyle; }
	}

	public Tour Tour
	{
		get { return tour; }
	}

	public TourPage TourPage
	{
		get { return tourPage; }
	}

	public ColorScheme ColorScheme
	{
		get { return colorScheme; }
	}

	public TourView TourView
	{
		get { return tourView; }
	}

	public Label SpecialMessage
	{
		get { return masterPage.MasterSpecialMessage; }
	}

	public Panel SpecialMessagePanel
	{
		get { return masterPage.MasterSpecialMessagePanel; }
	}

	public StatusBox StatusBox
	{
		get { return masterPage.MasterStatusBox; }
	}

	protected virtual void AddChangeDetection(CheckBox checkBox)
	{
		checkBox.Attributes.Add("onclick", "maChangeDetected();");
	}

	protected virtual void AddChangeDetectionForPreview(CheckBox checkBox)
	{
		checkBox.Attributes.Add("onclick", "maChangeDetectedForPreview();");
	}

	protected virtual void AddChangeDetection(DropDownList dropDownList)
	{
		dropDownList.Attributes.Add("onchange", "maChangeDetected();");
	}

	protected virtual void AddChangeDetectionForPreview(DropDownList dropDownList)
	{
		dropDownList.Attributes.Add("onchange", "maChangeDetectedForPreview();");
	}

	protected virtual void AddChangeDetection(RadioButton radioButton)
	{
		// The event should really be onchange, but some browsers (IE 6) don't fire onchange.
		// Using onclick is safe, but means that a change is detected if the user clicks on,
		// but does not change the radio button selection.
		radioButton.Attributes.Add("onclick", "maChangeDetected();");
	}

	protected virtual void AddChangeDetectionForPreview(RadioButton radioButton)
	{
		radioButton.Attributes.Add("onclick", "maChangeDetectedForPreview();");
	}

	protected virtual void AddChangeDetection(TextBox textBox)
	{
		textBox.Attributes.Add("onkeyup", "maTestForTextBoxChanges();");
		textBox.Attributes.Add("onfocus", "maStartMonitoringTextBoxChanges(this);");
	}

	protected virtual void AddChangeDetectionForPreview(TextBox textBox)
	{
		textBox.Attributes.Add("onkeyup", "maTestForTextBoxChangesForPreview();");
		textBox.Attributes.Add("onfocus", "maStartMonitoringTextBoxChangesForPreview(this);");
	}

	protected string AssignClientVar(string name, string value)
	{
		return name + "='" + value + "';";
	}

	protected string AssignClientVar(string name, int value)
	{
		return name + "=" + value + ";";
	}

	protected string AssignClientVar(string name, bool value)
	{
		return name + "=" + (value ? "true" : "false") + ";";
	}

	protected string AssignClientVarObject(string name, string value)
	{
		return name + "=" + value + ";";
	}

	protected virtual void ChangePageMode(string mode)
	{
	}

	protected virtual void ClearErrors(params Label[] errorLabels)
	{
		SetPageError(string.Empty);
		foreach (Label label in errorLabels)
			label.Text = string.Empty;
	}

	private void Delete(string target)
	{
		string[] args = target.Split(',');

		deleteSucceeded = false;
		string objectName = string.Empty;
		MemberPageActionId parentPageActionId = MemberPageActionId.Undefined;
		MemberPageActionId deleteActionId = MemberPageActionId.Undefined;

		string actionIdName = args[0];

		switch (actionIdName)
		{
			case "DeleteTour":
				objectName = "Tour";
				parentPageActionId = MemberPageActionId.TourExplorer;
				DeleteTour(args);
				deleteActionId = MemberPageActionId.DeleteTour;
				break;

			case "DeleteMap":
			case "DeleteDataSheet":
			case "DeleteGallery":
				objectName = actionIdName == "DeleteMap" ? "Map" : (actionIdName == "DeleteGallery" ? "Gallery" : "Data Sheet");
				parentPageActionId = MemberPageActionId.TourManager;
				DeleteMap(args);
				if (actionIdName == "DeleteGallery")
					deleteActionId = MemberPageActionId.DeleteGallery;
				else if (actionIdName == "DeleteDataSheet")
					deleteActionId = MemberPageActionId.DeleteDataSheet;
				else
					deleteActionId = MemberPageActionId.DeleteMap;
				break;

			case "DeleteHotspot":
				objectName = "Hotspot";
				if (masterPage.ActionId == MemberPageActionId.EditHotspotContent)
					parentPageActionId = MemberPageActionId.Map;
				else
					parentPageActionId = masterPage.ActionId;
				DeleteHotspot();
				deleteActionId = MemberPageActionId.DeleteHotspot;
				break;

			case "DeleteMarker":
				objectName = TourResourceManager.GetTitle(TourResourceType.Marker);
				parentPageActionId = MemberPageActionId.MarkerExplorer;
				DeleteMarker();
				deleteActionId = MemberPageActionId.DeleteMarker;
				break;

			case "DeleteSymbol":
				objectName = TourResourceManager.GetTitle(TourResourceType.Symbol);
				parentPageActionId = MemberPageActionId.SymbolExplorer;
				DeleteSymbol();
				deleteActionId = MemberPageActionId.DeleteSymbol;
				break;

			case "DeleteCategory":
				objectName = TourResourceManager.GetTitle(TourResourceType.Category);
				parentPageActionId = MemberPageActionId.CategoryExplorer;
				DeleteCategory();
				deleteActionId = MemberPageActionId.DeleteCategory;
				break;

			case "DeleteFontStyle":
				objectName = TourResourceManager.GetTitle(TourResourceType.FontStyle);
				parentPageActionId = MemberPageActionId.FontStyleExplorer;
				DeleteFontStyle();
				deleteActionId = MemberPageActionId.DeleteFontStyle;
				break;

			case "DeleteMarkerStyle":
				objectName = TourResourceManager.GetTitle(TourResourceType.MarkerStyle);
				parentPageActionId = MemberPageActionId.MarkerStyleExplorer;
				DeleteMarkerStyle();
				deleteActionId = MemberPageActionId.DeleteMarkerStyle;
				break;

			case "DeleteTooltipStyle":
				objectName = TourResourceManager.GetTitle(TourResourceType.TooltipStyle);
				parentPageActionId = MemberPageActionId.TooltipStyleExplorer;
				DeleteTooltipStyle();
				deleteActionId = MemberPageActionId.DeleteTooltipStyle;
				break;

			case "DeleteTourStyle":
				objectName = TourResourceManager.GetTitle(TourResourceType.TourStyle);
				parentPageActionId = MemberPageActionId.ColorSchemeExplorer;
				DeleteColorScheme();
				deleteActionId = MemberPageActionId.DeleteColorScheme;
				break;

			default:
				Debug.Fail("Unexpected delete event " + target);
				break;
		}

		// Record the delete action for debugging purposes. The logic to capture and record the delete
		// action Id could be removed once we are confident that no more delete releated anomalies exist.
		Utility.RecordAction(deleteActionId);

		if (deleteSucceeded || !isValidPost)
		{
			Server.Transfer(MemberPageAction.ActionPageTarget(parentPageActionId), false);
		}
		else
		{
			string message;
			if (cannotDeleteResource)
				message = string.Format("The default {0} cannot be deleted", objectName);
			else
				message = string.Format("The {0} could not be deleted because it is in use", objectName);
			SetPageError(message);
		}
	}

	private void DeleteCategory()
	{
		if (isValidPost && Account.NumberOfResourceDependents(TourResourceType.Category, category.Id) == 0)
		{
			category.Delete();
			deleteSucceeded = true;
		}
	}

	private void DeleteFontStyle()
	{
		if (isValidPost && Account.NumberOfResourceDependents(TourResourceType.FontStyle, fontStyleResource.Id) == 0)
		{
			if (fontStyleResource.Id == account.DefaultResourceId(TourResourceType.FontStyle))
			{
				cannotDeleteResource = true;
			}
			else
			{
				fontStyleResource.DeleteResource();
				deleteSucceeded = true;
			}
		}
	}

	private void DeleteMarker()
	{
		if (isValidPost && Account.NumberOfResourceDependents(TourResourceType.Marker, marker.Id) == 0)
		{
			if (marker.Id == account.DefaultResourceId(TourResourceType.Marker))
			{
				cannotDeleteResource = true;
			}
			else
			{
				if (account.LastMarkerIdSelected == marker.Id)
					account.LastMarkerIdSelected = 0;
				marker.DeleteResource();
				deleteSucceeded = true;
			}
		}
	}
	
	private void DeleteMarkerStyle()
	{
		if (isValidPost && Account.NumberOfResourceDependents(TourResourceType.MarkerStyle, markerStyle.Id) == 0)
		{
			if (markerStyle.Id == account.DefaultResourceId(TourResourceType.MarkerStyle))
			{
				cannotDeleteResource = true;
			}
			else
			{
				markerStyle.DeleteResource();
				deleteSucceeded = true;
			}
		}
	}

	private void DeleteMap(string[] args)
	{
		if (isValidPost)
		{
			int pageId;
			int.TryParse(args[1], out pageId);
			TourPage tourPageToDelete = tour.GetTourPage(pageId);
			if (tourPageToDelete != null)
			{
				tourPageToDelete.Delete();
				deleteSucceeded = true;
			}
		}
	}

	private void DeleteHotspot()
	{
		if (isValidPost)
		{
			tourView.Delete();
			deleteSucceeded = true;
		}
	}

	private void DeleteSymbol()
	{
		if (isValidPost && Account.NumberOfResourceDependents(TourResourceType.Symbol, symbol.Id) == 0)
		{
			if (symbol.Id == account.DefaultResourceId(TourResourceType.Symbol))
			{
				cannotDeleteResource = true;
			}
			else
			{
				symbol.DeleteResource();
				deleteSucceeded = true;
			}
		}
	}

	private void DeleteTooltipStyle()
	{
		if (isValidPost && Account.NumberOfResourceDependents(TourResourceType.TooltipStyle, tooltipStyle.Id) == 0)
		{
			if (tooltipStyle.Id == account.DefaultResourceId(TourResourceType.TooltipStyle))
			{
				cannotDeleteResource = true;
			}
			else
			{
				tooltipStyle.DeleteResource();
				deleteSucceeded = true;
			}
		}
	}

	private void DeleteTour(string[] args)
	{
		int tourId;
		int.TryParse(args[1], out tourId);
		Tour tourToDelete = new Tour(tourId);
		if (tourToDelete.Id != 0)
		{
			tourToDelete.Delete();
			deleteSucceeded = true;
		}
	}

	private void DeleteColorScheme()
	{
		if (isValidPost && Account.NumberOfResourceDependents(TourResourceType.TourStyle, colorScheme.Id) == 0)
		{
			if (colorScheme.Id == account.DefaultResourceId(TourResourceType.TourStyle))
			{
				cannotDeleteResource = true;
			}
			else
			{
				colorScheme.DeleteResource();
				deleteSucceeded = true;
			}
		}
	}

	protected void DeleteUnusedResources(TourResourceType resourceType)
	{
		bool deleteUnused = Request.QueryString["delete"] == "1";
		if (deleteUnused)
		{
			TourResourceManager.DeleteUnusedResources(resourceType);
		}
	}

	protected virtual void EmitJavaScript()
	{
		EmitJavaScript("", "");
	}

	protected void EmitJavaScript(string loadingScript, string loadedScript)
	{
		loadingScript +=
			AssignClientVar("formId", masterPage.FormId) +
			AssignClientVar("formContentId", masterPage.FormContentId) +
			AssignClientVar("actionId", (int)masterPage.ActionId) +
			AssignClientVar("actionIdLastPageAction", (int)Utility.LastPageAction) +
			AssignClientVar("actionIdLastViewAction", (int)Utility.LastViweAction) +
			AssignClientVar("actionIdIsMapAction", MemberPageAction.IsMapAction(masterPage.ActionId)) +
			AssignClientVar("saveButtonSrc", "../Images/" + "BtnSave1.gif") +
			AssignClientVar("undoButtonSrc", "../Images/" + "BtnUndo1.gif") +
			AssignClientVar("actionIdIsViewAction", MemberPageAction.IsHotspotAction(masterPage.ActionId));

		if (loadingScript.Length > 0)
			ClientScript.RegisterClientScriptBlock(this.GetType(), "MemberPage.cs loading", loadingScript, true);
		
		if (loadedScript.Length > 0)
			ClientScript.RegisterStartupScript(this.GetType(), "MemberPage.cs loaded", loadedScript, true);
	}

	protected void GetSelectedTour()
	{
		tour = MapsAliveState.SelectedTour;
		tourPage = tour.SelectedTourPage;
		tourView = tour.SelectedTourView;
		
		masterPage.MasterPageHeader.PageObjectType = PageObjectType.Tour;
	}

	protected void GetSelectedTourOrNone()
	{
		tour = MapsAliveState.SelectedTourOrNull;
		if (tour != null)
		{
			tourPage = tour.SelectedTourPage;
			tourView = tour.SelectedTourView;

			masterPage.MasterPageHeader.PageObjectType = PageObjectType.Tour;
		}
	}

	protected void GetSelectedTourPage()
	{
		tour = MapsAliveState.SelectedTour;

		tourPage = tour.SelectedTourPage;
		if (tourPage == null)
		{
			string targetPath = MemberPageAction.ActionPageTargetPath(MemberPageActionId.TourManager);
			Response.Redirect(targetPath);
		}

		tourView = tour.SelectedTourView;
	}

	protected void GetSelectedTourView()
	{
		tour = MapsAliveState.SelectedTour;

		tourPage = tour.SelectedTourPage;

		tourView = tour.SelectedTourView;
		if (tourView == null)
		{
			string targetPath = MemberPageAction.ActionPageTargetPath(MemberPageActionId.TourManager);
			Response.Redirect(targetPath);
		}

		masterPage.MasterPageHeader.PageObjectType = PageObjectType.TourView;
		Utility.LastViweAction = masterPage.ActionId;
	}

	protected void HandlePostEvent()
	{
		string eventTarget = string.Empty;
		string eventArgument = string.Empty;
		bool isDeleteEvent = false;
		bool isUndoEvent = false;
		bool isDelayedSaveEvent;
		int nextPostId;

		if (IsPostBack)
		{
			eventArgument = Page.Request.Params["__EVENTARGUMENT"];
			eventTarget = Page.Request.Params["__EVENTTARGET"];
			isDeleteEvent = eventTarget == "EventDelete";
			isUndoEvent = eventTarget == "EventUndo";
			
			// Verify that the post is valid. Note that if the session expires between the time
			// the page loads and the time it posts back, MapsAliveState.PostId will reset to 0
			// and not compare equally to masterPage.MasterPostId.Value.
			isValidPost = masterPage.MasterPostId.Value == MapsAliveState.PostId.ToString();
		}
		else
		{
			isValidPost = true;
		}

		// Set the page's next post Id.
		nextPostId = MapsAliveState.GenerateNextPostId();
		masterPage.MasterPostId.Value = nextPostId.ToString();

		// Determine if this is delayed save event.  They are special because their handlers
		// do the upload and then save the page.  Thus for them we don't do a save here.
		isDelayedSaveEvent =
			eventTarget == "EventUploadFile" ||
			eventTarget == "EventRemoteArchiveImport" ||
			eventTarget == "EventRemoteContentImport" ||
			eventTarget == "EventUploadSampleImage" ||
			eventTarget == "EventRemoveImage" ||
			eventTarget == "EventChangePageMode";

//		Debug.WriteLine("HandlePostEvent: " + Page.ToString() + " " + eventTarget + " " + eventArgument);
		string viewState = Page.Request.Params["__VIEWSTATE"];
//		Debug.WriteLine("VIEWSTATE SIZE = " + (viewState == null ? 0 : viewState.Length));

		if (!IsPostBack || isUndoEvent)
		{
			// This page is loading for the first time or we are reloading it
			// to undo the user's changes.  Tell the page it is being loaded.
			InitPageControls(isUndoEvent);
			return;
		}
		else
		{
			// Set the action Id. It's normally set by InitPageControls.
			masterPage.MasterPageHeader.PageActionId = masterPage.ActionId;
		}

		// Delete the requested object and then transfer to its parent page.
		if (isDeleteEvent)
		{
			Delete(eventArgument);
			
			// We only get here if the delete could not be performed.
			InitPageControls(false);
			return;
		}

		if (!isDelayedSaveEvent)
		{
			// Attempt to save the page.  If there are validation errors, display
			// the page again regardless of the event type.
			if (!Save())
			{
				InitPageControls();
				return;
			}
		}

		// The page has been saved.  Now honor the event.
		switch (eventTarget)
		{
			case "EventSave":
				{
					// Load the page again because we are returning to it.
					InitPageControls();
					break;
				}

			case "EventSaveAndBuild":
			case "EventSaveAndTransfer":
				{
					if (isValidPost && eventArgument.Contains("post="))
					{
						// Bump the post Id in the query string from the client to match the new Id.
						// We need to do this because the page we are transferring to is going to validate
						// against the new Id which is 1 greater than the Id coming from the client.
						// Note that the post Id validation logic is simple, but very easy to break.
						// Be extemely careful making changes and test thoroughly.
						int clientPostId = nextPostId - 1;
						eventArgument = eventArgument.Replace("post=" + clientPostId, "post=" + nextPostId);
					}
					
					if (eventArgument.Length > 1)
					{
						// We redirect instead of Server.Transfer because if the new page is an HttpHandler,
						// transfer will cause the dreaded "Error executing child request" error.
						Response.Redirect(App.WebSitePathUrl(eventArgument.Substring(1)));
					}
					else
					{
						// We don't know what circumstances can cause control to come here, but we have seen
						// it happen a few times and so we trap it to avoid getting an index-out-of-range
						// error when we use Substring on the eventArgument. This code causes the page to save,
						// but not transfer.
						InitPageControls();
					}
					break;
				}

			case "EventUploadSampleImage":
				{
					if (isValidPost)
					{
						int sampleId = int.Parse(eventArgument);
						UploadSampleImage(sampleId);
					}
					InitPageControls();
					break;
				}

			case "EventUploadFile":
				{
					if (isValidPost)
					{
						ImportFromUploadedFile();
					}
					InitPageControls();
					break;
				}

			case "EventRemoteImport":
				{
					if (isValidPost)
					{
						ImportFromRemoteUrl(eventArgument);
					}
					InitPageControls();
					break;
				}

			case "EventRemoveImage":
				{
					if (isValidPost)
					{
						bool contentChanged = eventArgument == "1";
						RemoveImage();
					}
					InitPageControls();
					break;
				}

			case "EventChangePageMode":
				{
					if (isValidPost)
					{
						ChangePageMode(eventArgument);
					}
					InitPageControls();
					break;
				}

			default:
				{
					if (!isValidPost && eventTarget == string.Empty)
					{
						// We can get here if the session expired between the time the user first came
						// to the page and the time they clicked a button that caused it to post back

						MemberPageActionId actionId = masterPage.ActionId;
						
						if (actionId == MemberPageActionId.AppContent ||
							actionId == MemberPageActionId.UploadMap
							)
						{
							// We know it's safe to proceed with the above pages after a timeout.
							break;
						}

						// The current page is not listed above, and there is no case for the eventTarget.
						// To be safe, we assume that it's not safe to proceed. For example, if a user
						// is on the Tour Setup page to create a new tour, but does not click the Continue
						// button until after their session expires, the page will post back, but because
						// isValidPost is false, the usual Save logic, which validates and reads the page
						// fields, will not execute because the Save() method returns immediately if it see
						// that the post is invalid. If we, allowed OnCreateTour handler to get called, it
						// would attempt to create a tour with uninitialized values (because they had not
						// been read or validated).
						//
						// In many cases it is safe to proceed, but until we identify them all, we play it
						// safe and transfer to a session expired page. Better for the user to see that
						// than an unexpected MapsAlive error.

						Response.Redirect("~/Members/" + MemberPageAction.ActionPageTarget(MemberPageActionId.SessionExpired));
					}
					break;
				}
		}
	}

	protected void InitPageControls()
	{
		InitPageControls(false);
	}

	private void InitPageControls(bool undo)
	{
		masterPage.MasterPageHeader.PageActionId = masterPage.ActionId;
		
		InitMasterPageButtons();
		
		if (undo)
			Undo();
		
		InitControls(undo);
		
        // The V4 Tour Builder does not use the All Tours panel. In V3, it was visible when there were >= 1 tours.
        masterPage.MasterAllToursPanel.Visible = false;
		
		// Hide the Current Tour Navigator if there is no current tour.
		masterPage.MasterCurrentTourPanel.Visible = tour != null;
	}

	protected void InitMasterPageButtons()
	{
		MemberPageHeader pageHeader = masterPage.MasterPageHeader;
	}

	protected virtual void InitControls(bool undo)
	{
	}

	protected abstract void PageLoad();

	protected virtual void PageLoaded()
	{
	}

	protected virtual void PagePreRender()
	{
		HttpBrowserCapabilities browser = Request.Browser;
		if (browser.Browser == "IE" && browser.MajorVersion >= 10)
		{
            // Force IE 10 into IE 9 compatibility to circumvents a problem that causes the Telerik menus to not work.
            HtmlMeta meta = new HtmlMeta
            {
                HttpEquiv = "X-UA-Compatible",
                Content = "IE=EmulateIE9"
            };
            Page.Header.Controls.AddAt(0, meta);
		}
	}

	protected virtual void PerformUpdate()
	{
	}

	protected virtual void ReadPageFields()
	{
	}

	protected abstract PageUsage PageUsageType();

	protected bool Save()
	{
		if (!isValidPost)
			return true;

		pageValid = true;
		ValidatePage();

		// If the page is valid, we disable the save and undo buttons, but
		// if it's invalid, we keep them enabled so the user can either fix
		// the problem and press Save, or press Undo.
		bool enable = !pageValid;
		
		if (pageValid)
		{
			ReadPageFields();

			try
			{
				PerformUpdate();
				return true;
			}
			catch (Exception ex)
			{
				if (ex is System.Threading.ThreadAbortException && this.masterPage.ActionId == MemberPageActionId.Profile)
				{
					// This exception should only occur if the user changed their email address
					// on the Profile page. See the comments there in PerformUpdate().
					return true;
				}
				Utility.ReportException("Save", ex);
				return false;
			}
		}
		
		return false;
	}

	protected void SetActionId(MemberPageActionId actionId)
	{
		this.masterPage.ActionId = actionId;

		MapsAliveState.Persist(MapsAliveObjectType.TourBuilderActionId, actionId);

		Utility.RecordAction(actionId);

		// Remember the group associated with the action Id so that when the user switches
		// from one main tab to another, or returns from Tour Preview, we know which group
		// or page to return to.

		switch (actionId)
		{
			case MemberPageActionId.EditHotspotContent:
			case MemberPageActionId.EditHotspotActions:
			case MemberPageActionId.HotspotOptionsAdvanced:
				account.SetLastActionIdForGroup(MemberPageActionId.HotspotProperties, actionId);
				break;

			case MemberPageActionId.ImportRoutes:
			case MemberPageActionId.ImportHotspots:
			case MemberPageActionId.ImportHotspotContent:
			case MemberPageActionId.ImportHotspotPhotos:
			case MemberPageActionId.ImportMarkerShapes:
				account.LastActionIdForImportSlides = actionId;
				break;

			case MemberPageActionId.LayoutAreaMarginsAndSpacing:
			case MemberPageActionId.PopupAppearance:
			case MemberPageActionId.TemplateChoicesForLayoutArea:
			case MemberPageActionId.TemplateChoicesForPopup:
			case MemberPageActionId.TemplateSplittersForLayoutArea:
			case MemberPageActionId.TemplateSplittersForPopup:
				account.SetLastActionIdForGroup(MemberPageActionId.LayoutProperties, actionId);
				break;

			case MemberPageActionId.BannerOptions:
			case MemberPageActionId.DirectoryOptions:
			case MemberPageActionId.TourManager:
			case MemberPageActionId.TourOptions:
				account.SetLastActionIdForGroup(MemberPageActionId.TourProperties, actionId);
				break;

			case MemberPageActionId.Map:
			case MemberPageActionId.MapOptionsAdvanced:
			case MemberPageActionId.MapSetup:
			case MemberPageActionId.MapSetupNew:
				account.SetLastActionIdForGroup(MemberPageActionId.MapProperties, actionId);
				break;
			
			case MemberPageActionId.UploadMap:
				// Special case so that the user never goes to the Upload Map page when choosing Map from
				// the main menu. While it might be useful to stay on the Upload Map page sometimes, you
				// usually want to go to the Map page and it's confusing when you end up on Upload Map.
				account.SetLastActionIdForGroup(MemberPageActionId.MapProperties, MemberPageActionId.Map);
				break;

			case MemberPageActionId.CategoryExplorer:
			case MemberPageActionId.EditCategory:
				account.SetLastActionIdForGroup(MemberPageActionId.Resources, MemberPageActionId.CategoryExplorer);
				break;

			case MemberPageActionId.MarkerExplorer:
			case MemberPageActionId.EditMarker:
				account.SetLastActionIdForGroup(MemberPageActionId.Resources, MemberPageActionId.MarkerExplorer);
				break;

			case MemberPageActionId.SymbolExplorer:
			case MemberPageActionId.EditSymbol:
				account.SetLastActionIdForGroup(MemberPageActionId.Resources, MemberPageActionId.SymbolExplorer);
				break;
			
			case MemberPageActionId.ColorSchemeExplorer:
			case MemberPageActionId.EditColorScheme:
				account.SetLastActionIdForGroup(MemberPageActionId.Resources, MemberPageActionId.ColorSchemeExplorer);
				break;

			case MemberPageActionId.FontStyleExplorer:
			case MemberPageActionId.EditFontStyle:
				account.SetLastActionIdForGroup(MemberPageActionId.Resources, MemberPageActionId.FontStyleExplorer);
				break;

			case MemberPageActionId.MarkerStyleExplorer:
			case MemberPageActionId.EditMarkerStyle:
				account.SetLastActionIdForGroup(MemberPageActionId.Resources, MemberPageActionId.MarkerStyleExplorer);
				break;
			
			case MemberPageActionId.TooltipStyleExplorer:
			case MemberPageActionId.EditTooltipStyle:
				account.SetLastActionIdForGroup(MemberPageActionId.Resources, MemberPageActionId.TooltipStyleExplorer);
				break;

			default:
				break;
		}
	}

	protected void SetActionIdForPageAction(MemberPageActionId actionId)
	{
		SetActionId(actionId);
		Utility.LastPageAction = actionId;
		masterPage.MasterPageHeader.PageObjectType = PageObjectType.TourPage;
	}

	protected void SetMasterPage(Navigation masterPage)
	{
		this.masterPage = masterPage;
	}

	protected void SetPageError(string message)
	{
		string text = message == string.Empty ? message : string.Format("<span style='color:#cc0000;font-weight:bold;'>{0}</span>", message);
		SetPageSpecialWarning(text);
	}

	protected void SetPageMessage(string message)
	{
		SetPageSpecialNotice(message);
	}

	protected void SetPageWarning(string message)
	{
		SetPageSpecialWarning(message);
	}

	protected void SetPageReadOnly()
	{
		masterPage.MasterPageHeader.ReadOnlyPage = true;
	}

	protected void SetPageTitle(string title)
	{
		masterPage.MasterPageHeader.Title = title;
		Page.Title = string.Format(Resources.Text.MemberPageBrowserTitle, title, Utility.AccountId);
	}

	protected void SetPageUsage()
	{
		pageUsage = PageUsageType();
	}

	private void SetResourceFiltering()
	{
		string filterString = Request.QueryString["filter"];
		if (filterString != null)
		{
			bool filteringCategories = account.ResourceIsFilteredBy(ResourceFilters.Category);
			
			int resourceFilters;
			int.TryParse(filterString, out resourceFilters);
			account.ResourceFilters = (ResourceFilters)resourceFilters;

			if (filteringCategories != account.ResourceIsFilteredBy(ResourceFilters.Category))
				categoryFilterChanged = true;

		}
	}

	protected void SetPageSpecialWarning(string message)
	{
		bool showSpecialMessage = !string.IsNullOrEmpty(message);
		SpecialMessagePanel.Visible = showSpecialMessage;
		SpecialMessagePanel.CssClass = "memberPageSpecialWarning";
		if (showSpecialMessage)
			SpecialMessage.Text = message;
	}

	protected void SetPageSpecialNotice(string message)
	{
		SetPageSpecialWarning(message);
		SpecialMessagePanel.CssClass = "memberPageSpecialNotice";
	}

	protected virtual bool SetStatus()
	{
		// If we control comes here, the current page does not override this method.
		return false;
	}

	protected virtual void RemoveImage()
	{
	}

    public int TourBuilderMapPageSlideThumbWidth
    {
        get { return 119; }
    }

    public string MapPageStyleDefinitions
    {
        get
        {
            // Create styles that will be inserted into the Map and Gallery editor pages after
            // the <script> tags in order to make the page width accommodate the map area width.
            // The code is here so that both pages can share it.

            int maxWidth = Math.Max(TourBuilderPageWidth, tourPage.MapAreaSize.Width + TourBuilderPageContentLeftWidth);
            int contentRightWidth = Math.Max(tourPage.MapAreaSize.Width, TourBuilderPageContentRightWidth);
            string definitions = "<style type=\"text/css\">";

            // Note that doubled up curly braces below are to escape a single curly brace ub lines that use string.Format().
            definitions += string.Format("body{{max-width:{0}px;}}", maxWidth);
            definitions += string.Format(".memberPageControlsHeader{{width:{0}px;}}", maxWidth);
            definitions += string.Format(".memberPageControlsMenuBackground{{width:{0}px;}}", maxWidth);
            definitions += string.Format(".memberPageContentRight{{width:{0}px;}}", contentRightWidth);

            int mapControlsPanelPadding = 6;
            definitions += string.Format("#mapControlsPanel{{width:{0}px;}}", contentRightWidth - mapControlsPanelPadding);

            definitions += "</style>";
           
            return definitions;
        }
    }

    public int TourBuilderPageWidth
    {
        get { return 965; }
    }

    public int TourBuilderPageContentLeftWidth
    {
        get { return 245; }
    }

    public int TourBuilderPageContentRightWidth
    {
        get { return 720; }
    }

    protected virtual void UploadSampleImage(int sampleId)
	{
	}

	protected virtual void ImportFromUploadedFile()
	{
	}

	protected virtual void ImportFromRemoteUrl(string url)
	{
	}

	protected void SetErrorMessage(Label errorLabel, string errorMsg)
	{
		string msg = "<span style='color:red'>*</span> " + errorMsg;
		SetPageError(msg);
		if (errorLabel != null)
			errorLabel.Text = "*";
	}

	protected virtual void SetFieldError(Label errorLabel)
	{
		if (fieldValid)
			errorLabel.Text = string.Empty;
		else
			pageValid = false;
	}

	protected void ShowStatusBox()
	{
		// Make the step-by-step instructions visible on pages that choose to show them.
        StatusBox.Visible = account.ShowStepByStepHelp && SetStatus();
	}

    protected bool ShowSmartStatusBox(string kind)
    {
        if (tourPage == null)
        {
            StatusBox.LastAction = "This tour has no map";
            StatusBox.NextAction = "Add a map to the tour";
            StatusBox.SetStep(1, "Choose [New > Map] from the menu.");
            return true;
        }

        if (tour.TourPages.Count >= 1 && !tourPage.MapImage.HasFile)
        {
            if (this.masterPage.ActionId == MemberPageActionId.UploadMap)
            {
                ShowUploadMapImageStatusBox();
                return true;
            }
            return false;
        }

        int tourViewCount = tourPage.TourViews.Count;
        if (tourViewCount == 0)
        {
            StatusBox.LastAction = string.Format("This {0} does not<br>have any hotspots yet", kind);
            StatusBox.NextAction = "Add the 1st hotspot";
            StatusBox.ShowGraphicHotspotIcon = true;
            StatusBox.SetStep(1, AppContent.Topic("StatusNewHotspot"));
            return true;
        }

        if (tourViewCount == 1)
        {
            StatusBox.LastAction = string.Format("This {0} has only 1 hotspot", kind);
            StatusBox.NextAction = "Add a 2nd hotspot";
            StatusBox.ShowGraphicHotspotIcon = true;
            StatusBox.SetStep(1, AppContent.Topic("StatusNewHotspot"));
            return true;
        }

        if (tourPage.MarkersOnMap >= 2)
            return false;

        if (masterPage.ActionId != MemberPageActionId.Map)
        {
            StatusBox.LastAction = string.Format("This {0} has {1} hotspots. You can<br>place their markers on the map.", kind, tourViewCount);
            StatusBox.NextAction = "Go to the Map Editor";
            StatusBox.ShowGraphicMapEditorIcon = true;
            StatusBox.SetStep(1, AppContent.Topic("StatusMapEditor"));
            return true;
        }

        return false;
    }

    protected void ShowUploadMapImageStatusBox()
    {
        StatusBox.NextAction = "Upload a map image";
        StatusBox.SetStep(1, "Press the [Choose File] button.");
        StatusBox.SetStep(2, "Choose a <a'ref-maps/#map-image'>map image</a> file from your computer.");
        StatusBox.SetStep(3, "Press the [Load] button to upload the file.");
    }

    protected void TransferToMemberPage(string target)
	{
		// Make sure that this method is only called from a post back handler that is called 
		// after this MemberPage class's Page_Load is done executing. In all other cases,
		// the transfer has to be done via a direct call to Response.Redirect. If you call
		// this method instead, the page being transferred away from executes to completion
		// before the transfer occurs.
		Debug.Assert(okToTransferToAnotherPage, "Cannot use TransferToMemberPage from this location");

		// This method returns to its caller even though it does a transfer. Therefore the caller must
		// itself return after calling this method. To understand why we use this, see "Correct use of
		// System.Web.HttpResponse.Redirect" in the AvantLogic knowledge base document.
		Response.Redirect(target, false);
		Context.ApplicationInstance.CompleteRequest();
	}

	protected virtual void ValidateFieldCondition(bool condition, Label errorLabel, string errorMsg)
	{
		fieldValid = true;

		if (!condition)
		{
			SetErrorMessage(errorLabel, errorMsg);
			fieldValid = false;
		}

		SetFieldError(errorLabel);
	}

	protected virtual int ValidateFieldInRange(TextBox field, int min, int max, Label errorLabel)
	{
		return ValidateFieldInRange(field, min, max, errorLabel, false);
	}

	protected virtual int ValidateFieldInRange(TextBox field, int min, int max, Label errorLabel, bool blankAllowed)
	{
		if (blankAllowed && field.Text.Trim() == string.Empty)
			return 0;

		fieldValid = true;

		int n;

		if (!int.TryParse(field.Text, out n))
			fieldValid = false;
		else
			fieldValid = n >= min && n <= max;

		if (!fieldValid)
		{
			string msg = blankAllowed ? Resources.Text.NumberInRangeOrBlankRequired : Resources.Text.NumberInRangeRequired;
			SetErrorMessage(errorLabel, string.Format(msg, min, max));
		}

		SetFieldError(errorLabel);

		return n;
	}

	protected virtual string ValidateColorSwatch(ColorSwatch colorSwatch)
	{
		fieldValid = false;

		string colorString = colorSwatch.ColorValue;
		
		if (!String.IsNullOrEmpty(colorString))
		{
			Color color = Color.Empty;
			try
			{
				color = ColorTranslator.FromHtml(colorString);
				fieldValid = true;
			}
			catch
			{
				// The string was not recognized as a color name. Try it as a hex value.
				try
				{
					color = ColorTranslator.FromHtml("#" + colorString);
					fieldValid = true;
				}
				catch
				{
				}
			}
			if (fieldValid)
			{
				colorSwatch.ColorValue = string.Format("#{0:x2}", color.R) + string.Format("{0:x2}", color.G) + string.Format("{0:x2}", color.B);
			}
		}

		if (fieldValid)
		{
			colorSwatch.ErrorMessage = string.Empty;
		}
		else
		{
			SetPageError(Resources.Text.HexColorRequired);
			colorSwatch.ErrorMessage = "*";
		}

		pageValid = pageValid && fieldValid;

		return colorSwatch.ColorValue;
	}

	protected virtual void ValidateFieldNotBlank(string fieldText, Label errorLabel, string errorMsg)
	{
		fieldValid = true;

		if (fieldText.Trim().Length == 0)
		{
			SetErrorMessage(errorLabel, errorMsg);
			fieldValid = false;
		}

		SetFieldError(errorLabel);
	}

	protected virtual void ValidateFieldIsValidFileName(string fieldText, Label errorLabel, string errorMsg)
	{
		fieldValid = true;

		if (!Utility.IsValidFileName(fieldText))
		{
			SetErrorMessage(errorLabel, errorMsg);
			fieldValid = false;
		}

		SetFieldError(errorLabel);
	}

	protected virtual void ValidateFieldIsValidCategoryCode(string fieldText, Label errorLabel, string errorMsg)
	{
		fieldValid = true;

		if (!Utility.IsValidFileName(fieldText) || fieldText.Contains(","))
		{
			SetErrorMessage(errorLabel, errorMsg);
			fieldValid = false;
		}

		SetFieldError(errorLabel);
	}

	protected virtual decimal ValidateMoney(TextBox field, bool allowNegativeValue, Label errorLabel)
	{
		fieldValid = true;

		decimal value = 0.0M;

		try
		{
			string money = field.Text;
			if (money.StartsWith("$"))
				money = money.Substring(1);
			value = decimal.Parse(money);
			if (!allowNegativeValue && value < 0)
			{
				SetErrorMessage(errorLabel, "Amount must not be negative");
				fieldValid = false;
			}
		}
		catch
		{
			SetErrorMessage(errorLabel, "A decimal value is required");
			fieldValid = false;
		}

		SetFieldError(errorLabel);

		return value;
	}

	protected virtual void Undo()
	{
	}

	protected virtual void ValidatePage()
	{
	}
}
