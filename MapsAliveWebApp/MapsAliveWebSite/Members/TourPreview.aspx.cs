// Copyright (C) 2003-2021 AvantLogic Corporation
using System;
using System.Net;
using System.Web;
using System.Web.UI;

public partial class Members_TourPreview : System.Web.UI.Page
{
	private enum BuildStatus
	{
		Built,
		NotBuilt,
		Published,
		BuildError
	}

	private enum Snip
	{
		MapsAliveJs = 1,
		MaClientPath = 2,
		PageJs = 3,
		SoundManager = 4,
		LiveDataJs = 5,
		CustomJs = 6,
		MapViewerJs = 7,
		Include = 8,
		TourPreviewFolder = 10,
		LoaderJs = 11
	}

	private Account account;
	private bool build;
	private BuildStatus buildStatus;
	private bool hidingAdvisor;
	private int pageNumber;
	private bool pageUsesLiveData;
	private string previewPath;
	private bool published;
	private bool publishError;
	private bool rebuild;
	private bool renumber;
	private MemberPageActionId tourBuilderActionId;
	private string snippet;
	private int adviceCount;
	private int level1AdviceCount;
	private int level2AdviceCount;
	private string pageName;
	private bool reportMode;
	private bool showingAdvisor;
	private bool showingSnippets;
	private Tour tour;
	private string tourPath;
	private string version;

	protected void Page_Load(object sender, EventArgs e)
	{
		account = MapsAliveState.Account;

		Utility.PreventPageCaching(Response);
		Utility.RecordAction(MemberPageActionId.TourPreview);

		System.Diagnostics.Debug.WriteLine("Enter TourPreview");

		// See if the tour's Id is on the query string.
		int tourId = 0;
		int.TryParse(Request.QueryString["tourId"], out tourId);
		if (tourId != 0)
		{
			int accountId = 0;
			int.TryParse(Request.QueryString["accountId"], out accountId);
			tour = new Tour(tourId, accountId);
			if (tour.Id == 0)
				tour = null;
			else
			{
				reportMode = true;
				HidePanels();
			}
		}

		if (!reportMode && MapsAliveState.Account.MembershipExpired)
			Server.Transfer(MemberPageAction.ActionPageTarget(MemberPageActionId.Expired));

		if (tour == null)
		{
			tour = MapsAliveState.SelectedTour;
		}

		if (tour == null || tour.TourPages.Count == 0)
		{
			HidePanels();
			StatusMessage.Text = tour == null ? "There is no tour" : string.Format("{0} has no pages", tour.Name);
			return;
		}

		if (tour.V3CompatibilityEnabled)
			PreviewPanelV3.Visible = true;
		else
			PreviewPanel.Visible = true;

		SoundManagerPanel.Visible = tour.UseSoundManager;

		if (tour.V3CompatibilityEnabled)
        {
			CustomHtmlJavaScriptPanel.Visible = tour.HasCustomHtmlJavaScript;
			CustomHtmlJavaScriptIncludePanel.Visible = tour.CustomHtmlJavaScriptIncludeSrc.Length > 0;
        }

		tourPath = tour.Url;
		previewPath = tourPath + "_";

		SetBrowserTitle();
		GetQueryStringArgs();
		GetLiveDataMessengerFunctions();
		
		if (!reportMode)
			BuildTour();
		
		SetTourName();
		SetStatusMessage();

		version = "?v=" + tour.BuildId;
		
		if (tour.V3CompatibilityEnabled)
        {
			string pattern = tour.V3CompatibilityEnabled ? TourBuilder.PatternForPageCssFileV3 : TourBuilder.PatternForTourCssJsFile;
			string fileName = string.Format(pattern,  pageNumber, tour.BuildId);
			LinkCss.Href = string.Format("{0}/{1}{2}", previewPath, fileName, version);
        }
		else
        {
			LinkCss.Visible = false;
        }

		if (tour.HasCustomHtmlCss && tour.V3CompatibilityEnabled)
			CustomHtmlCss.Href = previewPath + "/custom.css" + version;
		else
			CustomHtmlCss.Visible = false;

		if (reportMode)
        {
            PublishButton.Visible = false;
        }
        else
		{
			if (!account.DisableTourAdvisor)
				GetAdvice();
			
			CreateMenuBar();
			CreateCodeSnippets();
		}
	}

	private void BuildTour()
	{
		if (rebuild || tour.HasChangedSinceLastBuilt())
		{
			try
			{
				if (rebuild && renumber)
					tour.RenumberPages = true;
				TourBuilder tourBuilder = new TourBuilder(tour);
				bool ok = tourBuilder.BuildTour();
				buildStatus = ok ? BuildStatus.Built : BuildStatus.BuildError;
			}
			catch (Exception ex)
			{
				Utility.ReportException("BuildTour", ex);
				buildStatus = BuildStatus.BuildError;
			}
		}
		else if (published)
		{
			buildStatus = BuildStatus.Published;
		}
		else if (publishError)
		{
			buildStatus = BuildStatus.BuildError;
		}
		else
		{
			buildStatus = BuildStatus.NotBuilt;
		}
	}

	private void HidePanels()
	{
		MenuPanel.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
		TasksPanel.Visible = false;
		SnippetsPanel.Visible = false;
	}

	private void SetBrowserTitle()
	{
		string browserTitle = tour.BrowserTitle.Trim().Length == 0 ? tour.Name : tour.BrowserTitle;
		Page.Title = string.Format(Resources.Text.PreviewPageBrowserTitle, browserTitle);
	}

	private void GetQueryStringArgs()
	{
		// Get the action Id of the Tour Builder page to return to when the user clicks
		// return to Tour Builder.  If the Id is not valid, default to the Tour Manager.
		int aid = 0;
		int.TryParse(Request.QueryString["aid"], out aid);
		if (Enum.IsDefined(typeof(MemberPageActionId), aid))
		{
			tourBuilderActionId = (MemberPageActionId)aid;
		}
		else
		{
			tourBuilderActionId = MemberPageActionId.TourManager;
		}

        if (tour.V3CompatibilityEnabled)
        {
            // Get the page number for the page to preview and make sure it's valid. Tour Preview of tour in
            // V3 compatibility mode can only display one page.  When the user clicks in the tour's page menu,
            // the Tour Preview gets reloaded with the new page.
            pageNumber = 0;
		    int.TryParse(Request.QueryString["page"], out pageNumber);
		    bool pageNumberOk = false;
		    foreach (TourPage tourPage in tour.TourPages)
		    {
			    if (tourPage.PageNumber == pageNumber)
			    {
				    pageNumberOk = true;
				    pageName = tourPage.Name;
				    break;
			    }
		    }

		    // If the pageNumber was no good, show the tour's first page.
		    if (!pageNumberOk)
			    pageNumber = tour.GetTourPage(tour.FirstPageId).PageNumber;
        }

		// Get the build arg.
		build = Request.QueryString["build"] == "1";

		// Get the rebuild arg, but if the tour has never been built or
		// was last built with an out-dated runtme, force a rebuild now.
		if (tour.BuildId == 0 || tour.RequiresRebuild)
			rebuild = true;
		else
			rebuild = Request.QueryString["rebuild"] == "1";

		// Get the renumber arg that indicates that the tour pages should be renumbered on a rebuild.
		if (rebuild)
			renumber = Request.QueryString["renumber"] == "1";

		// Get the published arg.
		published = Request.QueryString["pub"] == "1";
		publishError = Request.QueryString["pub"] == "2";

		// Get the show/hide advisor arg.  Both of these flags can be false,
		// but only one can be true.
		hidingAdvisor = Request.QueryString["sa"] == "0";
		showingAdvisor = Request.QueryString["sa"] == "1";

		// Get the show snippets arg.
		showingSnippets = Request.QueryString["ss"] == "1";
	}

	private void CreateCodeSnippets()
	{
		SnippetsPanel.Style.Add(HtmlTextWriterStyle.Display, showingSnippets ? "block" : "none");

		const int indent12 = 12;
		const int indent24 = 24;
		const string gray = "#999999";
		snippet = string.Empty;

		snippet = string.Empty;
		AddSnippet(tourPath);
		UseUrl.Text = snippet;

		snippet = string.Empty;
		string html = string.Format("<a href=\"{0}\">{1}</a>", tourPath, tour.Name);
		AddSnippet(html);
		UseReplaceWindow.Text = snippet;
		UseReplaceWindowTryIt.Text = html;

		snippet = string.Empty;
		html = string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>", tourPath, tour.Name);
		AddSnippet(html);
		UseOpenWindow.Text = snippet;
		UseOpenWindowTryIt.Text = html;

		int pageWidth = tour.TourSize.Width + (tour.BodyMargin * 2);
		int pageHeight = tour.TourSize.Height + (tour.BodyMargin * 2);

		int createdWithMapsAliveHeight = account.IsTrial ? 26 : 0;
		snippet = string.Empty;
		AddSnippet(string.Format("<iframe src=\"{0}\" width=\"{1}\" height=\"{2}\" frameBorder=\"0\" scrolling=\"no\"></iframe>", tourPath, pageWidth, pageHeight + createdWithMapsAliveHeight));
		UseIframe.Text = snippet;
		HyperLinkInsertIframe.NavigateUrl = string.Format("javascript:maShowIframe('{0}', {1}, {2});", tourPath, pageWidth, pageHeight + createdWithMapsAliveHeight);

		snippet = string.Empty;
		AddSnippet("<!DOCTYPE html>", gray, indent12);
		AddSnippet("<html lang=\"en\">", gray, indent12);
		AddSnippet("<head>", gray, indent12);
		AddSnippet("<meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\" />", gray, indent24);
		AddSnippet("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">", gray, indent24);
		AddSnippet("</head>", gray, indent12);
		AddSnippet("<body>", gray, indent12);

		if (tour.V3CompatibilityEnabled)
        {
			AddSnippet(string.Format("<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\" />", tourPath + "/page" + pageNumber + ".css"), indent24);

			if (tour.HasCustomHtmlCss)
			{
				AddSnippet(string.Format("<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}\" />", tourPath + "/custom.css"), indent24);
			}

			if (tour.UseSoundManager)
			{
				AddSnippet(string.Format("<script type=\"text/javascript\" src=\"{0}\"></script>", Path(Snip.SoundManager, tourPath, false)), indent24);
				AddSnippet(string.Format("<script type='text/javascript'>soundManager.url=\"{0}\";</script>", tourPath), indent24);
			}

			AddSnippet(string.Format("<script type=\"text/javascript\" src=\"{0}\"></script>", Path(Snip.MapsAliveJs, tourPath, false)), indent24);
			AddSnippet(string.Format("<script type=\"text/javascript\" src=\"{0}\"></script>", Path(Snip.MapViewerJs, tourPath, false)), indent24);
        }

		if (pageUsesLiveData)
		{
			if (tour.V3CompatibilityEnabled)
				AddSnippet(string.Format("<script type=\"text/javascript\" src=\"{0}\"></script>", Path(Snip.LiveDataJs, tourPath, false)), indent24);
		}

		if (tour.V4)
        {
			string maxHeight = tour.IsFlexMapTour ? string.Format(" data-flex-height=\"{0}\"", tour.TourSize.Height) : "";
            AddSnippet(string.Format("<script type=\"module\" id=\"ma-{1}\" src=\"{0}\"></script>", Path(Snip.LoaderJs, tourPath, false), tour.Id), indent24);
            AddSnippet(string.Format("<div class=\"ma-{0}\"{1}></div>", tour.Id, maxHeight), indent24);
        }

		if (tour.V3CompatibilityEnabled)
		{
			AddSnippet(string.Format("<script type=\"text/javascript\">{0}</script>", Path(Snip.MaClientPath, tourPath, false)), indent24);

			bool usesGoToPage = tour.MenuLocationIdEffective != (int)Tour.MenuLocation.None || tour.HasDirectory;
			if (tour.TourPages.Count >= 2 && usesGoToPage)
			{
				string script = string.Format("maClient.hostPageUrl=\"See User Guide\";");
				AddSnippet(string.Format("<script type=\"text/javascript\">{0}</script>", script), indent24);
			}

			AddSnippet(string.Format("<script type=\"text/javascript\" src=\"{0}\"></script>", Path(Snip.PageJs, tourPath, false)), indent24);

			if (tour.HasCustomHtmlJavaScript)
			{
				string src = tour.CustomHtmlJavaScriptIncludeSrc;
				if (src.Length > 0)
				{
					AddSnippet(string.Format("<script type=\"text/javascript\" src=\"{0}\"></script>", src), indent24);
				}
				AddSnippet(string.Format("<script type=\"text/javascript\" src=\"{0}\"></script>", Path(Snip.CustomJs, tourPath, false)), indent24);
			}
		}
		
		AddSnippet("</body>", gray, indent12);
		AddSnippet("</html>", gray, indent12);
		UseFrameless.Text = snippet;
		UseFramelessHelpText1.Text = AppContent.Topic("HelpFrameless1");
		
		if (tour.V3CompatibilityEnabled && tour.TourPages.Count >= 2)
			UseFramelessHelpText2.Text = string.Format(AppContent.Topic("HelpFrameless2"), pageName, tour.TourPages.Count);
        else if (tour.IsFlexMapTour)
            UseFramelessHelpText2.Text = AppContent.Topic("HelpFrameless3");
    }

    private void CreateMenuBar()
	{
		// Create the URL for the Return to TourBuilder button. The tb arg tells the returned-to
		// page that the Tour Builder is being is returned to from outside the Tour Builder and
		// that it needs to restore its Tour Builder state. The tp arg says returning from Tour Preview.
		string returnPage = MemberPageAction.ActionPageTarget((MemberPageActionId)tourBuilderActionId);
		string targetPath = App.WebSitePathUrl("Members/" + returnPage);
		targetPath += returnPage.Contains("?") ? "&" : "?";
		targetPath += string.Format("tb=1&tp=1&rev={0}", App.Revision);
		HyperLinkReturn.NavigateUrl = "javascript:maReturnToTourBuilder('" + targetPath + "');";

		if (account.DisableTourAdvisor)
		{
			TasksPanel.Visible = false;
		}
		else
		{
			bool okToShowAdvice = level2AdviceCount > 0;
			bool showAdvice = (okToShowAdvice && !hidingAdvisor) || showingAdvisor;
			TasksPanel.Style.Add(HtmlTextWriterStyle.Display, "none");
		}

        ShowSnippetsOption.Text = SnippetsMenuItemTitle(true);
		HyperLinkSnippets.NavigateUrl = string.Format("javascript:maShowSnippets({0},'{1}','{2}', {3});", tour.HasBeenPublished ? 1 : 0, SnippetsMenuItemTitle(true), SnippetsMenuItemTitle(false), tour.Id);
        
        if (!account.DisableTourAdvisor)
        {
            ShowAdvisorOption.Text = "Show Tour Advisor";
            HyperLinkAdvisor.Visible = true;
            HyperLinkAdvisor.NavigateUrl = string.Format("javascript:maShowTasks({0},'{1}','{2}');", adviceCount, AdvisorMenuItemTitle(true), AdvisorMenuItemTitle(false));
        }

		int aid = (int)MemberPageActionId.PublishTour;
		int tbaid = (int)tourBuilderActionId;
        bool publishDisabled = false;
		
		string publishUrl = "javascript:";
		if (tour.ExceedsSlideLimit)
		{
			SpecialMessagePanel.Visible = true;
			SpecialMessage.Text = account.HotspotLimitMessage(HotspotLimitWarningContext.PublishTourOverLimit);
			HyperLinkPublish.Enabled = false;
		}
		else if (tour.IsPrivate)
		{
			publishUrl += string.Format("maAlert({0}, 'This tour is private and cannot be published.<p>The Private option can be changed on the Tour Manager screen.</p>')", tour.Id);
            HyperLinkPublish.NavigateUrl = publishUrl;
            publishDisabled = true;
        }
		else if (tour.HasBeenPublished && !tour.HasChangedSinceLastPublished)
		{
			publishUrl += string.Format("maAlert({0}, 'This tour has not changed since it was last published.')",tour.Id);
            HyperLinkPublish.NavigateUrl = publishUrl;
            publishDisabled = true;
        }
        else
		{
			string clickAction = string.Format("maConfirmPublish({0}, '{1}', '{2}', {3}, {4});", tour.Id, Utility.JavascriptSingleQuotedString(tour.Name), tour.Url, aid, tbaid);
            HyperLinkPublish.Attributes.Add("onclick", clickAction);
        }

        if (publishDisabled)
            HyperLinkPublish.CssClass += " publishDisabled";

        int pageCount = tour.TourPages.Count;

		if (tour.HasBeenPublished)
		{
			HyperLinkTour.Target = "_blank";
			HyperLinkTour.Text = tour.Url;
			HyperLinkTour.Style.Add(HtmlTextWriterStyle.FontWeight, "600");
            string clickAction;
			if (tour.HasChangedSinceLastPublished)
                clickAction = Tour.HasChangedSinceLastPublishedConfirm(tour.Id);
            else
                clickAction = string.Format("window.open('{0}', '_blank');", tour.Url);
            HyperLinkTour.Attributes.Add("onclick", clickAction);
            RunTourLabel.Visible = true;
        }
        else
		{
			PublishButton.Style.Add(HtmlTextWriterStyle.PaddingTop, "14px");
			HyperLinkTour.Visible = false;
			RunTourLabel.Visible = false;
		}
	}

	private void SetStatusMessage()
	{
		string message = string.Empty;

		switch (buildStatus)
		{
			case BuildStatus.Built:
			case BuildStatus.NotBuilt:
				if (tour.HasChangedSinceLastPublished)
					message = string.Format("Tour changed since published on {0}", Utility.DateShort(tour.DatePublished));
				break;

			case BuildStatus.Published:
				// message = string.Format("{0} has been published and is ready to use at &nbsp;<a href='{1}' target='_blank'>{2}</a>", tour.Name, tour.Url, tour.UrlPlain);
				message = string.Format("Tour was published on {0} and is ready to use.", Utility.DateShort(tour.DatePublished));
				break;
			
			case BuildStatus.BuildError:
				message = "<span style='color:red;font-weight:bold;'>We're sorry, but an unexpected error occurred while updating or publishing this tour.</span><br/><br/><span style='color:#000;font-weight:bold;'>Please try again.</span><br/><br/>If the problem persists, contact <a href='mailto:support@mapsalive.com?subject=Tour Preview Error'>support@mapsalive.com</a>.";
				break;
		}

		StatusMessage.Text = message;
	}

	private void SetTourName()
	{
        string prefix;
        if (tour.V3CompatibilityEnabled)
            prefix = "V3 Tour";
        else
            prefix = tour.IsFlexMapTour ? "Flex Map" : "Classic Tour";
        string tourName = string.Format("{0} #{1} : {2}", prefix, tour.Id, tour.Name);
        TourPreviewerTourName.Text = tourName;
	}
	
	private string AdvisorMenuItemTitle(bool show)
	{
		return (show ? "Show" : "Hide") + " Tour Advisor";
	}
	
	private string SnippetsMenuItemTitle(bool show)
	{
		return (show ? "Show" : "Hide") + " Code Snippets";
	}

	private string TaskMenuItemTitle(bool advisorHidden)
	{
		if (advisorHidden)
		{
			string itemCount = string.Empty;
			if (adviceCount > 0)
				itemCount = string.Format(" ({0} item{1})", adviceCount, adviceCount != 1 ? "s" : "");
			return string.Format("Show Tour Advisor{0}", itemCount);
		}
		else
			return "Hide Tour Advisor";
	}

	private void GetAdvice()
	{
		const int maxMessagesPerAdviceSet = 3;
		string tasks = tour.Advisor.EmitAdviceAndSolutionsHtml((int)tourBuilderActionId, maxMessagesPerAdviceSet);
		level1AdviceCount = tour.Advisor.Level1AdviceCount;
		level2AdviceCount = tour.Advisor.Level2AdviceCount;
		adviceCount = level1AdviceCount + level2AdviceCount;

		Tasks.Text = adviceCount > 0 ? tasks : "<div>This tour is in good shape.  The advisor has no suggestions at this time.</div>";

		if (adviceCount > 0)
		{
			TasksPanel.Style.Add(HtmlTextWriterStyle.Height, "auto");
			TasksPanelContent.Style.Add(HtmlTextWriterStyle.Height, "auto");
		}
	}

	private void AddSnippet(string text)
	{
		AddSnippet(text, "#000000", 12);
	}

	private void AddSnippet(string text, int indent)
	{
		AddSnippet(text, "#000000", indent);
	}

	private void AddSnippet(string text, string color, int indent)
	{
		int marginTop = snippet.Length == 0 ? 6 : 0;
		snippet += string.Format("<div style='margin-top:{3}px;color:{1};padding-left:{2}px;'>{0}</div>", HttpUtility.HtmlEncode(text), color, indent, marginTop);
	}
	
	protected string LoaderId
	{
		get { return string.Format("ma-{0}", tour.Id); }
	}

	protected string PathCustomJs
	{
		get { return Path(Snip.CustomJs, previewPath, true); }
	}

	protected string PathCustomHtmlJs
	{
		get { return Path(Snip.Include, previewPath, true); }
	}

	protected string PathLoaderJs
	{
		get { return Path(Snip.LoaderJs, previewPath, true); }
	}

	protected string PathMaClient
	{
		get { return Path(Snip.MaClientPath, previewPath, true); }
	}

	protected string PathMapsAliveJs
	{
		get { return Path(Snip.MapsAliveJs, previewPath, true); }
	}

	protected string PathMapViewerJs
	{
		get { return Path(Snip.MapViewerJs, previewPath, true); }
	}

	protected string PathPageJs
	{
		get { return Path(Snip.PageJs, previewPath, true); }
	}

	protected string PathRuntimeFolder
	{
		// The SoundManager swf has to come from the runtime folder instead of from the tour's
		// preview folder in order to avoid a Flash cross-domain security error that manifests
		// itself as JavaScript error "NP object error".
		get { return "../Runtime"; }
	}


	protected string PathSoundManagerJs
	{
		get { return Path(Snip.SoundManager, "../Runtime", false); }
	}

	protected string PathTourPreviewFolder
	{
		get { return Path(Snip.TourPreviewFolder, previewPath, true); }
	}

	private string Path(Snip id, string path, bool includeVersion)
	{
		string v = includeVersion ? version : string.Empty;
		switch (id)
		{
			case Snip.MapsAliveJs:
				return path + "/mapsalive.js" + v;
					
			case Snip.MapViewerJs:
				return path + "/mapviewer.js" + v;
					
			case Snip.LoaderJs:
				return path + "/mapsalive-module.js" + v;
					
			case Snip.TourPreviewFolder:
				return path + "/";

			case Snip.LiveDataJs:
				return path + "/livedata.js" + v;
			
			case Snip.MaClientPath:
				return "maClient.path=\"" + path + "/\";";
			
			case Snip.PageJs:
				// This path is only used when previewing a V3 tour.
				return path + "/" + String.Format(TourBuilder.PatternForPageJsFileV3, pageNumber) + v;

			case Snip.SoundManager:
				return path + "/soundmanager2-nodebug-jsmin.js";
			
			case Snip.CustomJs:
				return path + "/custom.js" + v;
			
			case Snip.Include:
				return tour.CustomHtmlJavaScriptIncludeSrc + v;

			default:
				return string.Empty;
		}
	}

	protected string StyleDefinitions
	{
		get
		{
            // Create styles that will be inserted into the Tour Preview page after any <style> tags that define other CSS.
            // Note that doubled up curly braces below are to escape them only for lines that use string.Format().
            int maxWidth = Math.Max(965, tour.TourSize.Width + (tour.BodyMargin * 2));
            string definitions = "<style type=\"text/css\">";
            definitions += string.Format("body{{background-color:{0};}}", tour.BodyBackgroundColor);
			definitions += string.Format("body{{max-width:{0}px;}}", maxWidth);
			definitions += ".maTour{margin:auto;}";
			definitions += "#maTour{margin:auto;}";
            definitions += "</style>";
            return definitions;
		}
	}

	private void GetLiveDataMessengerFunctions()
	{
		foreach (TourPage tourPage in tour.TourPages)
		{
			if (tourPage.UsesLiveData)
			{
                pageUsesLiveData = true;
                return;
			}
		}

		pageUsesLiveData = false;
	}
}
