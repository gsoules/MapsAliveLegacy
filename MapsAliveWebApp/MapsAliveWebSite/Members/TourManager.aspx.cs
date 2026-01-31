// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class Members_TourManager : MemberPage
{
	private bool accountCanCopyToSamples;
    private bool isSamplesAccount;
	private const bool renumber = true;
	private bool tourChangedToPrivate;
    private bool switchedBetweenV3AndV4 = false;

	private void AddChangeDetection(CheckBox checkBox, string script)
	{
		checkBox.Attributes.Add("onclick", "maChangeDetected();" + script);
	}
	private void AddChangeToV4Detection(CheckBox checkBox)
	{
		checkBox.Attributes.Add("onclick", "maConvertToV4(this);");
	}

	protected override void InitControls(bool undo)
	{
		TourName.Text = tour.Name;
		TourId.Text = tour.Id.ToString();
		DateCreated.Text = tour.DateCreated.ToLongDateString() + ", " + tour.DateCreated.ToShortTimeString();
        
        if (tour.DateBuilt == DateTime.MinValue)
            Compiled.Text = "Never compiled";
        else
            Compiled.Text = string.Format("{0}, {1} (build #{2})", tour.DateBuilt.ToLongDateString(), tour.DateBuilt.ToShortTimeString(), tour.BuildId);

        if (tour.IsPrivate)
		{
			DatePublished.Text = "Private (cannot be published)";
		}
		else
		{
			if (tour.DatePublished == DateTime.MinValue)
				DatePublished.Text = "Never published";
			else
				DatePublished.Text = tour.DatePublished.ToLongDateString() + ", " + tour.DatePublished.ToShortTimeString();
		}

		if (tour.DateDownloadFileCreated == DateTime.MinValue)
			DateDownloaded.Text = "Never downloaded";
		else
			DateDownloaded.Text = tour.DateDownloadFileCreated.ToLongDateString() + ", " + tour.DateDownloadFileCreated.ToShortTimeString();

		if (tour.DateArchiveFileCreated == DateTime.MinValue)
			DateArchived.Text = "Never archived";
		else
			DateArchived.Text = tour.DateArchiveFileCreated.ToLongDateString() + ", " + tour.DateArchiveFileCreated.ToShortTimeString();

		HyperLinkTour.Target = "_blank";
		HyperLinkTour.Text = tour.Url;
		if (tour.IsPrivate)
		{
			HyperLinkTour.Text = "This tour is private and has no URL";
			HyperLinkTour.NavigateUrl = string.Empty;
		}
        else if (!tour.HasBeenPublished)
		{
			HyperLinkTour.Attributes.Add("onclick", "maAlert('This URL will be active after you publish this tour.\\nTo publish, click Tour Preview and then click Publish.');return false;");
		}
        else
        {
            string clickAction;
            if (tour.HasChangedSinceLastPublished)
                clickAction = Tour.HasChangedSinceLastPublishedConfirm(tour.Id);
            else
                clickAction = string.Format("window.open('{0}', '_blank');", tour.Url);
            HyperLinkTour.Attributes.Add("onclick", clickAction);
        }

		int mapCount = 0;
		int dataSheetCount = 0;
		int tourPageCount = tour.TourPageCount;

		if (tourPageCount == 0)
		{
			PagesPanel.Visible = false;
			RenumberPanel.Visible = false;
			RebuildControl.AppearsEnabled = false;
		}
		else
		{
			InitConfigurationControls(tourPageCount);

			RebuildControl.OnClickJavaScript = RebuildFunctionCall(!renumber);

			// Provide a list of this tour's page Id for the page thumbnails.
			string thumbList = string.Empty;
			foreach (TourPage tourPage in tour.TourPages)
			{
				if (tourPage.IsDataSheet)
					dataSheetCount++;
				else
					mapCount++;

				thumbList += string.Format("{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{1}",
					(char)0x01,
					(char)0x02,
					tour.Id,
					tourPage.Id,
					tourPage.Name,
					tour.TourSize.Width,
					tour.TourSize.Height,
					tourPage.IsDataSheet ? "1" : "0",
					tourPage.PageNumber,
					tourPage.HasBeenBuilt ? "1" : "0",
					tour.HasBeenPublished ? "1" : "0",
					tourPage.TourViews.Count,
					tour.HasChangedSinceLastPublished ? "1" : "0"
				);

				if (!IsPostBack)
				{
					// Populate the menu order list.
					RadListBoxItem listBoxItem = new RadListBoxItem(tourPage.Name, tourPage.Id.ToString());
					MenuOrderListBox.Items.Add(listBoxItem);

					// Populate the first page combo box.
					RadComboBoxItem comboBoxItem = new RadComboBoxItem(tourPage.Name, tourPage.Id.ToString());
					FirstPageComboBox.Items.Add(comboBoxItem);
					if (tourPage.Id == tour.FirstPage.Id)
						comboBoxItem.Selected = true;
				}
			}

			// Remove the last 0x02 delimeter.
			if (thumbList.Length > 0)
				thumbList = thumbList.Substring(0, thumbList.Length - 1);

			PageThumbs.ThumbList = thumbList;
		}

		InitAdvancedOptions();

		if (accountCanCopyToSamples)
		{
			// Display a link that makes it easy for us to copy a tour from the tour folder
			// to the samples folders. This is easier and faster than copying via FTP.
			CopyToSamplesPanel.Visible = true;
			CopyToSamplesControl.OnClickActionId = MemberPageActionId.TourManager;
			CopyToSamplesControl.QueryString = "?copy=1";
			CopyToSamplesControl.WarningMessage = "Copy this tour to the mapsalive.com/samples folder?";
			CopyToSamplesControl.AppearsEnabled = !tour.HasChangedSinceLastPublished;
		}
	}

	private void InitAdvancedOptions()
	{
		AllowUnbrandedCheckBox.Checked = tour.CanAppearUnbranded;
		if (account.IsPlusOrProPlan)
			AddChangeDetection(AllowUnbrandedCheckBox);
		else
			AllowUnbrandedCheckBox.Enabled = false;

		bool isFreeMembership = MapsAliveState.Account.IsTrial;

		string script = !tour.HasBeenPublished ? "" : "if (this.checked) maAlert(" +
        "'<p><b>Warning:</b> Checking the Private option will " +
		"cause the published version of this tour to be " +
		"deleted from the internet as soon as you save or " +
		"navigate away from this page. This happens " +
        "because private tours cannot be published and therefore the published version must be deleted.</p><p>If " +
		"you don\\'t want to delete the published tour, " +
		"click OK and uncheck the box now</p>.');";
		PrivateCheckBox.Checked = tour.IsPrivate;

		if (account.IsProPlan)
			AddChangeDetection(PrivateCheckBox, script);
		else
			PrivateCheckBox.Enabled = false;

		if (tour.V3CompatibilityEnabled)
        {
			UseSoundManagerCheckBox.Checked = tour.UseSoundManager;
			if (account.IsProPlan)
				AddChangeDetection(UseSoundManagerCheckBox);
			else
				UseSoundManagerCheckBox.Enabled = false;
        }

        EnableImagePreloadingCheckBox.Visible = true;
        EnableImagePreloading.Visible = true;

		// Only keeping these options for V3 compatibility.
        UseSoundManagerCheckBox.Visible = tour.V3CompatibilityEnabled;
		UseSoundManager.Visible = tour.V3CompatibilityEnabled;
        
        SelectsOnTouchStartCheckBox.Visible = tour.V3CompatibilityEnabled;
        SelectsOnTouchStart.Visible = tour.V3CompatibilityEnabled;
        
        EntirePopupVisibleCheckBox.Visible = tour.V3CompatibilityEnabled;
        EntirePopupVisible.Visible = tour.V3CompatibilityEnabled;
        
        UseTouchUiOnDesktopCheckBox.Visible = tour.V3CompatibilityEnabled;
        UseTouchScreenUi.Visible = tour.V3CompatibilityEnabled;

        ShowZoomControlCheckBox.Visible = tour.V3CompatibilityEnabled;
        ShowZoomControl.Visible = tour.V3CompatibilityEnabled;

        WebAppCapableCheckBox.Visible = tour.V3CompatibilityEnabled;
        WebAppCapable.Visible = tour.V3CompatibilityEnabled;

        // Not using these options anymore, but keeping the code in case we need them again.
        DisableSmoothPanningCheckBox.Visible = false;
        DisableSmoothPanning.Visible = false;
        DisableBlendEffectCheckBox.Visible = false;
        DisableBlendEffect.Visible = false;

        BrowserTitleText.Text = tour.BrowserTitle;
		AddChangeDetection(BrowserTitleText);

		if (!IsPostBack)
		{
            EnableV3CompatibilityCheckBox.Checked = tour.V3CompatibilityEnabled;
			SelectsOnTouchStartCheckBox.Checked = tour.MapSelectsOnTouchStart;
			UseTouchUiOnDesktopCheckBox.Checked = tour.UseTouchUiOnDesktop;
			EnlargeHitTestAreaCheckBox.Checked = tour.MapEnlargeHitTestArea;
			DisableBlendEffectCheckBox.Checked = tour.MapDisableBlendEffect;
			DisableSmoothPanningCheckBox.Checked = tour.MapDisableSmoothPanning;
			ShowZoomControlCheckBox.Checked = tour.MapShowZoomControlOnIOs;
			EntirePopupVisibleCheckBox.Checked = tour.MapEntirePopupVisible;
			EnableImagePreloadingCheckBox.Checked = tour.MapEnableImagePreloading;
			WebAppCapableCheckBox.Checked = tour.TourIsWebAppCapable;
            DisableKeyboardShortcutsCheckBox.Checked = tour.KeyboardShortcutsDisabled;
        }

        EnableV3CompatibilityCheckBox.Enabled = !tour.IsFlexMapTour;

        SelectsOnTouchStartCheckBox.Enabled = true;
		UseTouchUiOnDesktopCheckBox.Enabled = true;
		UseTouchUiOnDesktopCheckBox.Enabled = true;
		EnlargeHitTestAreaCheckBox.Enabled = true;
		DisableBlendEffectCheckBox.Enabled = true;
		DisableSmoothPanningCheckBox.Enabled = true;
		ShowZoomControlCheckBox.Enabled = true;
		EntirePopupVisibleCheckBox.Enabled = true;
		EnableImagePreloadingCheckBox.Enabled = true;
		WebAppCapableCheckBox.Enabled = true;

        AddChangeToV4Detection(EnableV3CompatibilityCheckBox);

		AddChangeDetection(DisableKeyboardShortcutsCheckBox);
		AddChangeDetection(SelectsOnTouchStartCheckBox);
		AddChangeDetection(UseTouchUiOnDesktopCheckBox);
		AddChangeDetection(EnlargeHitTestAreaCheckBox);
		AddChangeDetection(DisableBlendEffectCheckBox);
		AddChangeDetection(DisableSmoothPanningCheckBox);
		AddChangeDetection(EnableImagePreloadingCheckBox);
		AddChangeDetection(ShowZoomControlCheckBox);
		AddChangeDetection(EntirePopupVisibleCheckBox);
		AddChangeDetection(WebAppCapableCheckBox);
	}

	private void InitConfigurationControls(int tourPageCount)
	{
		if (tourPageCount == 1)
		{
			ConfigurationPanel.Visible = false;
			RenumberPanel.Visible = false;
		}
		else
		{
			MenuOrderListBox.ButtonSettings.RenderButtonText = false;
			MenuOrderListBox.ButtonSettings.Position = ListBoxButtonPosition.Right;
			MenuOrderListBox.AllowReorder = tourPageCount > 1;

			RenumberControl.OnClickJavaScript = RebuildFunctionCall(renumber);
			RenumberControl.WarningMessage = "<p>Renumber the HTML pages in this tour?</p><p>Only use this feature if you understand what it does.</p><p>Read the Explain box for more information.</p>";
		}
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle(Resources.Text.TourManagerTitle);
		SetActionId(MemberPageActionId.TourManager);
		
		// Note: This page posts back automatically if the user selects a new first page. We have to do this
		// so that the Tour Preview button will get reset with the new page. Otherwise, if the user
		// changes the first page, clicks Tour Preview, and doesn't see the new first page appear first,
		// it looks as though the change had not affect.

		GetSelectedTour();

        isSamplesAccount = account.UserName == "samples@mapsalive.com";
        accountCanCopyToSamples = isSamplesAccount || Utility.UserIsAdmin;

		if (Request.QueryString["imported"] == "1")
		{
			SetPageMessage("Import from archive file has completed. This is the imported tour.");
		}
		else if (accountCanCopyToSamples && Request.QueryString["copy"] == "1")
		{
			CopyTourToSamplesFolder();
		}
		else if (accountCanCopyToSamples && !isSamplesAccount && Request.QueryString["transfer"] == "1")
		{
			TransferTourToSamplesAccount();
		}
		else
		{
			string duplicatedTourId = Request.QueryString["duplicated"];
			if (duplicatedTourId != null)
			{
				SetPageMessage(string.Format("Duplication of tour #{1} has completed. This is the duplicated tour.", tour.Id, duplicatedTourId));
			}
		}

		if (tour.ExceedsSlideLimit)
		{
			SetPageSpecialNotice(account.HotspotLimitMessage(HotspotLimitWarningContext.TourOverLimit));
		}

		// Automatically set the tour's first page as the selected page.
		tourPage = tour.SelectedTourPage;
		if (tourPage == null && tour.TourPages.Count >= 1)
		{
			tour.SetSelectedTourPage(tour.FirstPageId);
			tourPage = tour.SelectedTourPage;
		}

		// Automatically set the tour's first page view as the selected view.
		if (tourPage != null && tourPage.TourViews.Count >= 1)
		{
			tour.SetSelectedTourView(tourPage.FirstTourViewId);
			tourView = tour.SelectedTourView;
		}

		// Cleanup the last download file that was created for this tour.
		string downloadFileLocation = tour.DownloadFileLocation;
		if (tour.HasBeenPublished && FileManager.FileExists(downloadFileLocation))
		{
			FileManager.DeleteFile(downloadFileLocation);
		}

		tour.ConvertToLatestVersion();
	}

	protected override void PerformUpdate()
	{
		int position = 0;
		bool orderChanged = false;

		tour.UpdateDatabase();

		foreach (RadListBoxItem item in MenuOrderListBox.Items)
		{
			TourPage tempTourPage;
			int tourPageId = int.Parse(item.Value);
			
			if (tourPageId == tourPage.Id)
				tempTourPage = tourPage;
			else
				tempTourPage = new TourPage(tour, tourPageId);
			
			position++;
			if (tempTourPage.MenuPosition != position)
			{
				tempTourPage.MenuPosition = position;
				tempTourPage.UpdateDatabase();
				orderChanged = true;
			}
		}

		if (orderChanged)
		{
			tour.RebuildTourTreeXml();
			tour.ReloadTourPages();
		}

		TourBuilder tourBuilder = new TourBuilder(tour);

		if (tourChangedToPrivate)
		{
			tourBuilder.DeletePublishedTour();
			tour.UnpublishCompleted();
		}

        tourBuilder.BuildTour();

        // When switching between V3 and V4, flush the tour from the cache to force it be read again
        // from the database which will in turn perform any fixups that are required when upgrading
        // to V4, or ensure that no V4 state is leftover when switching back to V3.
        if (switchedBetweenV3AndV4)
            MapsAliveState.Flush(MapsAliveObjectType.Tour);
	}
    protected override void ReadPageFields()
	{
		tour.CanAppearUnbranded = AllowUnbrandedCheckBox.Checked;
		tourChangedToPrivate = PrivateCheckBox.Checked && !tour.IsPrivate;
		tour.IsPrivate = PrivateCheckBox.Checked;
		tour.BrowserTitle = BrowserTitleText.Text;

		if (tour.V3CompatibilityEnabled)
			tour.UseSoundManager = UseSoundManagerCheckBox.Checked;

        // Determine if the user just unchecked the V3 compatibility option.
        // If so, uncheck the option to disable responsiveness.
        if (tour.V3CompatibilityEnabled && !EnableV3CompatibilityCheckBox.Checked || tour.V4 && EnableV3CompatibilityCheckBox.Checked)
            switchedBetweenV3AndV4 = true;

        // V3 flags
        setRuntimeTargetFlag(UseTouchUiOnDesktopCheckBox.Checked, Tour.MapViewerFlags.UseTouchUiOnDesktop);
        setRuntimeTargetFlag(SelectsOnTouchStartCheckBox.Checked, Tour.MapViewerFlags.SelectsOnTouchStart);
        setRuntimeTargetFlag(DisableBlendEffectCheckBox.Checked, Tour.MapViewerFlags.DisableBlendEffect);
        setRuntimeTargetFlag(ShowZoomControlCheckBox.Checked, Tour.MapViewerFlags.ShowZoomControlOnIOs);
        setRuntimeTargetFlag(EntirePopupVisibleCheckBox.Checked, Tour.MapViewerFlags.EntirePopupVisible);
        setRuntimeTargetFlag(DisableSmoothPanningCheckBox.Checked, Tour.MapViewerFlags.DisableSmoothPanning);
        setRuntimeTargetFlag(WebAppCapableCheckBox.Checked, Tour.MapViewerFlags.WebAppCapable);

        // V3 and V4 flags
        setRuntimeTargetFlag(EnlargeHitTestAreaCheckBox.Checked, Tour.MapViewerFlags.EnlargeHitTestArea);
        setRuntimeTargetFlag(EnableImagePreloadingCheckBox.Checked, Tour.MapViewerFlags.EnableImagePreloading);

        // V4 flags
        setRuntimeTargetFlag(EnableV3CompatibilityCheckBox.Checked, Tour.MapViewerFlags.EnableV3Compatibility);
        setRuntimeTargetFlag(DisableKeyboardShortcutsCheckBox.Checked, Tour.MapViewerFlags.DisableKeyboardShortcuts);
    }

    protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}

	private void CopyTourToSamplesFolder()
	{
		string publishedFolderLocationAbsolute = FileManager.PublishedFolderLocationAbsolute(tour.Id);
		string samplesFolderLocationAbsolute = string.Format("{0}\\{1}", App.SamplesFolderLocationAbsolute, tour.Id);

		if (FileManager.FolderExists(samplesFolderLocationAbsolute))
			FileManager.DeleteFolderContents(samplesFolderLocationAbsolute);

		FileManager.CopyFolder(publishedFolderLocationAbsolute, samplesFolderLocationAbsolute);
	}

    private void setRuntimeTargetFlag(bool enable, Tour.MapViewerFlags flag)
    {
        if (enable)
            tour.RuntimeTarget |= flag;
        else
            tour.RuntimeTarget &= ~flag;
    }

    private void TransferTourToSamplesAccount()
    {
        const int samplesAccountId = 300003;

        MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_UpdateAccountId",
            "@TourId", tour.Id,
            "@AccountId", samplesAccountId
        );

        Server.Transfer(MemberPageAction.ActionPageTarget(MemberPageActionId.TourExplorer), false);
    }

    protected void OnChangeFirstPage(object sender, EventArgs e)
	{
		// We automatically post back when the user changes the first page so that we can reload with
		// updated parameters on the Tour Preview button that will preview the new first page. Note that
		// this is a special case because normally Tour Preview shows the page selected in the Tour Navigator,
		// but prior to adding this logic, it was very confusing to change the first page and not see it be
		// first in Tour Preview.
		int newFirstPageId = int.Parse(FirstPageComboBox.SelectedItem.Value);
		tour.SetFirstPage(newFirstPageId);
		Response.Redirect(MemberPageAction.ActionPageTarget(MemberPageActionId.TourManager));
	}

	protected string RebuildFunctionCall(bool renumber)
	{
		int pageNumber = tour.FirstPage == null ? 0 : tour.FirstPage.PageNumber;
		return string.Format("maOnEventRebuild({0},{1},{2});", tour.Id, pageNumber, renumber ? 1 : 0);
	}

    protected override bool SetStatus()
    {
        if (tour.TourPages.Count >= 1)
            return false;
        
        StatusBox.Clear();

        StatusBox.LastAction = "This tour has no map";
        StatusBox.NextAction = "Add a map";
        StatusBox.SetStep(1, "Choose [New > Map] from the menu.");

        return true;
    }
}
