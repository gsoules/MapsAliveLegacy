// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class Members_EditHotspotContent : ImageUploadPage
{
	private string chooseFileButtonText;
	private bool changingPageMode;
	private bool layoutHasMediaArea;
	private bool layoutHasTextArea;
	private bool newMarkerSelected;
	private int oldMarkerId;
	private bool tooltipAllowed;
	private string validEmbedCode;
	private string validDataSheetName;
	private Size validEmbedCodeSize;
	private string validSlideId;
	private string validSlideTitle;

	bool viewTitleChanged;
	bool dataSheetNameChanged;
    bool highlightTourNavigator;

    protected override void EmitJavaScript()
	{
		string loadingScript = string.Empty;
        string loadedScript = string.Format("maHighlightTourNavigator({0});", highlightTourNavigator ? "true" : "false");
        EmitJavaScript(loadingScript, loadedScript);
	}

	protected override void InitControls(bool undo)
	{
		// Show/Hide panels
		PhotoPanel.Visible = layoutHasMediaArea && tourView.MediaType == SlideMediaType.Photo;
		EmbedPanel.Visible = layoutHasMediaArea && tourView.MediaType != SlideMediaType.Photo;
		EmbedPreviewPanel.Visible = false;
		EmbedPreviewPanelError.Visible = !pageValid;
		MediaSelectorPanel.Visible = layoutHasMediaArea;
		TextPanel.Visible = layoutHasTextArea;
		MapPanel.Visible = !tourPage.IsDataSheet;
		MarkerPanel.Visible = !tourPage.IsDataSheet && !tourView.MarkerIsRoute;
		TooltipPanel.Visible = !tourPage.IsDataSheet;
		NoTooltipPanel.Visible = !tooltipAllowed;

		if (tourPage.IsDataSheet)
		{
			// Change "Hotspot Id" to "Data Sheet Id" using non-breaking spaces
			// to keep the table cell from wrapping the words.
			TourViewId.Title = "Data&nbsp;Sheet&nbsp;Id";
			TourViewId.Topic = "TourViewIdDataSheet";
		}
		else
		{
			InitMapPreview();

			if (!tourView.MarkerIsRoute)
			{
				MarkerComboBox.ResourceType = TourResourceType.Marker;
				MarkerComboBox.OnClientSelectedIndexChangedScript = "maMarkerChanged();";

				if (!undo && IsPostBack)
				{
					// When posting back we only need to set the resource Id.
					// On the first post, we also load the combo box data table.
					MarkerComboBox.SelectedResourceId = tourView.MarkerId;
				}

				Marker marker = Account.GetCachedMarker(tourView.MarkerId);
				EditMarkerControl.OnClickActionId = MemberPageActionId.EditMarker;
				EditMarkerControl.Title = "Edit";
				EditMarkerControl.QueryString = "?id=" + tourView.MarkerId;
				
				NewMarkerControl.OnClickActionId = MemberPageActionId.CreateMarker;
				NewMarkerControl.Title = "New";
				NewMarkerControl.QueryString = string.Format("&id={0}&vid={1}", tourView.MarkerId, tourView.Id);
				NewMarkerControl.WarningMessage = string.Format("Click OK to create a duplicate of [@{0}@] and assign it to this hotspot. You will then be taken to the Edit Marker screen so that you can edit your new marker.", marker.Name);

                if (!tourPage.IsDataSheet)
                {
                    LocateHotspotOnMapControl.Visible = true;
                    LocateHotspotOnMapControl.OnClickActionId = MemberPageActionId.Map;
                    LocateHotspotOnMapControl.Title = (tourView.MarkerHasBeenPlacedOnMap ? "Locate" : "Place") + " this hotspot on the map";
                    LocateHotspotOnMapControl.QueryString = "?locate=1";
                    LocateHotspotOnMap.Visible = true;
                }
            }
        }

		// Media area title
		if (layoutHasMediaArea)
		{
			if (tourView.MediaType == SlideMediaType.Photo)
			{
				MediaLabel.Text = "Photo";
				TourViewPhoto.Topic = "TourViewPhoto";
			}
			else if (tourView.MediaType == SlideMediaType.Embed)
			{
				MediaLabel.Text = "Multimedia";
				TourViewPhoto.Topic = "TourViewMedia";
			}
		}
		else
		{
			MediaLabel.Text = "Media";
		}

		// Photo upload 
		InitPhotoUploadPanel();

		// Embed code
		if (tourView.MediaType == SlideMediaType.Embed)
		{
			EmbedTextBox.Rows = 20;
			AddChangeDetection(EmbedTextBox);
		}

		// Show or hide no-text message.
		NoTextMessage.Style.Add(HtmlTextWriterStyle.Display, layoutHasTextArea ? "none" : "block");

		if (tourPage.IsDataSheet)
		{
			// Use alternative Quick Help Ids so that the explain text for
			// these controls is different for a data sheet versus a hotspot.
			TourViewTitle.Topic = "TourViewTitleDataSheet";
			TourViewPhoto.Topic = "TourViewPhotoDataSheet";
		}

		if (!undo && IsPostBack)
			return;

		//------------------------------

		if (tourPage.IsDataSheet)
		{
			// Show the data sheet Name field.
			DataSheetNamePanel.Visible = true;
			DataSheetNameTextBox.Text = tourPage.Name;
			AddChangeDetection(DataSheetNameTextBox);
			
			// Set the hotspot title field to show the data sheet's page title.
			TourViewTitleTextBox.Text = tourPage.Title;

			// Change labels that are different for a data sheet.
			TourViewTitle.Title = "Data Sheet Title";
			
			// Hide controls that are not used by a data sheet.
			TooltipSectionPanel.Visible = false;
		}
		else
		{
			TourViewTitleTextBox.Text = tourView.Title;
		}
		AddChangeDetection(TourViewTitleTextBox);

		// Slide Id.
		SlideIdTextBox.Text = tourPage.IsDataSheet ? tourPage.PageId : tourView.SlideId;
		AddChangeDetection(SlideIdTextBox);
		
		// EditHotspotActions selector.
		if (!tourPage.IsDataSheet)
		{
			// Get the filter to be used to restrict which markers are shown. If the filter
			// is for unsued or account markers, use the all markers filter instead.
			Account account = MapsAliveState.Account;
			ResourceFilters resourceFilters = account.ResourceFilters;

			if (account.ResourceIsFilteredBy(ResourceFilters.Marker))
			{
				MarkerFilterWarningPanel.Visible = true;
				Utility.SetDivText(MarkerFilterWarningPanel, "The list above only contains markers used by this tour.<br/>To see all of your markers, uncheck Filter Markers on the Library menu.");
			}

			if (!tourView.MarkerIsRoute)
			{
				// Get a table of filtered markers and create the combo box items from it.
				MarkerFilter markerFilter = account.ResourceIsFilteredBy(ResourceFilters.Marker) ? MarkerFilter.Tour : MarkerFilter.Account;
				DataTable markerDataTable = Marker.GetFilteredMarkerList(markerFilter, tour, tourPage, tourView, account.Id);
				MarkerComboBox.ItemsDataTable = markerDataTable;
				MarkerComboBox.SelectedResourceId = tourView.MarkerId;
			}
		}

		// Embed code 
		if (tourView.MediaType == SlideMediaType.Embed)
		{
			EmbedTextBox.Text = tourView.EmbedText;
			EmbedPanelInstructions.Text = "Paste your HTML code into the box below";
			EmbedPreviewPanel.Width = tourView.EmbedWidth;
			EmbedPreviewPanel.Height = tourView.EmbedHeight;
			SetPreviewHtml(tourView.EmbedText);
		}

        // Tiny MCE Editor.
        if (layoutHasTextArea)
            HtmlEditor.Text = tourView.DescriptionHtml;

		// Tool Tip
		TourViewToolTipTextBox.Text = System.Web.HttpUtility.HtmlDecode(tourView.ToolTip);
		AddChangeDetection(TourViewToolTipTextBox);

		if (layoutHasMediaArea)
		{
			MediaSelectorComboBox.SelectedValue = ((int)tourView.MediaType).ToString();
		}
	}

	private void InitPhotoUploadPanel()
	{
		bool showPhotoUploadPanel = layoutHasMediaArea && tourView.MediaType == SlideMediaType.Photo;
		bool showMediaMessagePanel = !layoutHasMediaArea || (showPhotoUploadPanel && !tourView.HasImage);
		
		PhotoUploadPanel.Visible = showPhotoUploadPanel;
		PhotoImagePanel.Visible = showPhotoUploadPanel && tourView.HasImage;
		MediaMessagePanel.Visible = showMediaMessagePanel;
		
		if (showPhotoUploadPanel && tourView.HasImage)
		{
			// Append a timestamp to the image src to prevent the browser from using a cached image. If we don't do this
			// the browser will occasionally show the previous image when a user has just uploaed a new image.
			ImageElement.ImageUrl = string.Format("Thumbnail.ashx?dim=100&id={0}&v={1}", tourView.Id, DateTime.Now.Ticks);
			Size thumbnailSize = Utility.ScaledImageSize(tourView.Image.Size, new Size(100, 100));
			PhotoImagePanel.Style.Add(HtmlTextWriterStyle.Height, Math.Max(80, thumbnailSize.Height) + "px");

			Size scaledImageSize = Utility.ScaledImageSize(tourView.GetConstrainedImageSize(), tourView.GetImageContainerSize());
			
			if (tourView.TourPage.ActiveSlideLayout.HasImageArea)
			{
				// Even though we are showing the photo panel, we need to make sure that there is an image to show.
				// If the hotspot uses a photo marker, we show media area even if the layout has no image.
				string script = string.Format("maQuickImageShow(this,'{0}',{1},{2},{3},{4});", "ImageRenderer.ashx?type=photo&width=" + scaledImageSize.Width + "&height=" + scaledImageSize.Height, thumbnailSize.Width + 4, -100, scaledImageSize.Width, scaledImageSize.Height);
				ImageElement.Attributes.Add("onmouseover", script);
				ImageElement.Attributes.Add("onmouseout", "javascript:maQuickPreviewHide();");
			}

			ImageSizeActual.Text = string.Format("{0} x {1}", tourView.Image.Size.Width, tourView.Image.Size.Height);
			ImageSizeScaled.Text = string.Format("{0} x {1}", scaledImageSize.Width, scaledImageSize.Height);

			if (!badFileName)
				FileName.Text = tourView.Image.FileNameOriginal;
		}
		
		if (showMediaMessagePanel)
		{
			string message;
			if (layoutHasMediaArea && tourView.MediaType == SlideMediaType.Photo && !tourView.HasImage)
			{
				string pageUsage = tourPage.IsDataSheet ? "data sheet" : "hotspot";
				message = string.Format("This {1} has no photo. To upload one, click <b>{0}</b><br />and choose a photo from your computer. Then click <b>Load</b>.", chooseFileButtonText, pageUsage);
			}
			else
			{
				message = "This layout does not display media such as photo, video, etc.";
			}
			MediaMessage.Text = message;
		}
	}

	private void InitMapPreview()
	{
		if (tourPage.MapImage.HasFile)
		{
			MapImageElement.ImageUrl = "Thumbnail.ashx?type=map&width=60&height=50";
			Size containerSize = tourPage.MapAreaSize;
			if (containerSize.Width > 560)
				containerSize.Width = 560;
			Size scaledSize = Utility.ScaledImageSize(tourPage.MapImage.Size, containerSize);
			string script = string.Format("maQuickImageShow(this,'{0}',{1},{2},{3},{4});", "ImageRenderer.ashx?type=map&width=" + scaledSize.Width + "&height=" + scaledSize.Height, -(scaledSize.Width + 20), -16, scaledSize.Width, scaledSize.Height);
			MapImageElement.Attributes.Add("onmouseover", script);
			MapImageElement.Attributes.Add("onmouseout", "javascript:maQuickPreviewHide();");
		}
		else
		{
			MapPanel.Visible = false;
		}
	}

	protected override void PageLoad()
	{
        Utility.RegisterHtmlEditorJavaScript(this);
        
        SetMasterPage(Master);
		SetActionId(MemberPageActionId.EditHotspotContent);
		GetSelectedTourView();
		SetPageTitle(string.Format("Edit {0} Content", tourPage.IsDataSheet ? "Data Sheet" : "Hotspot"));

		if (tourView.MarkerIsRoute)
		{
			SetPageMessage("This is a route hotspot");
		}
		else if (Request.QueryString["new"] == "1")
		{
			SetPageMessage(string.Format("This is a new {0}", tourPage.IsDataSheet ? "data sheet" : "hotspot"));
		}

		SlideLayout slideLayout = tourPage.ActiveSlideLayout;
		layoutHasMediaArea = slideLayout.HasImageArea;
		if (!layoutHasMediaArea && !tourPage.IsDataSheet)
		{
			// If the hotspot uses a photo marker, show the media area so the user can upload
			// a photo for the marker even though the photo won't appear in the layout.
			Marker marker = Account.GetCachedMarker(tourView.MarkerId);
			layoutHasMediaArea = marker.MarkerType == MarkerType.Photo;
		}
		layoutHasTextArea = slideLayout.HasTextArea;

		tooltipAllowed = !tourPage.IsDataSheet && (!tourPage.SlidesPopup || tourPage.PopupOptions.LocationIsFixed || tourView.ShowContentEvent == ShowContentEvent.Never);
		if (!tooltipAllowed)
		{
			NoTooltipMessage.Text = "Because this map uses popups, a tooltip will be displayed only if this hotspot's Show action is set to \"Never\" or \"Marker is clicked\",<br/>or if this hotspot has no content and the Show Tooltip option is checked on the Popup Behavior screen.";
		}

		chooseFileButtonText = "Choose File";

		if (!IsPostBack)
			ProgressMonitor.ShowUploadFileProgress(ProgressArea);
	}

	protected override void PagePreRender()
	{
		HttpBrowserCapabilities browser = Request.Browser;
		if (browser.Browser == "IE" && browser.MajorVersion >= 9)
		{
			// Force IE 9 and IE 10 into compatibility mode just on this page. This circumvents a bug in
			// IE that causes the Telerik editor to insert extra <br/> tags in between lines.
			// We don't want to always run in compatibility mode, because doing so would
			// prevent HTML5 and CSS3 from working.
			// We originally set IE9 to emulate IE8 to fix the extra <br/> tags, but with the introduction
			// of IE 10 the Telerik editor controls did not display. So, now we emulate IE 7 in both IE 9 and IE 10 
			// which seems to fix both the extra <br/> tags and the display of the editor controls.
			HtmlMeta meta = new HtmlMeta();
			meta.HttpEquiv = "X-UA-Compatible";
			meta.Content = "IE=EmulateIE7";
			Page.Header.Controls.AddAt(0, meta);
		}

		if (badFileName)
			SetPageError(badFileNameMessage);
	}

	protected override void PerformUpdate()
	{
		if (newMarkerSelected)
		{
			Marker marker = Account.GetCachedMarker(oldMarkerId);
			if (marker.IsExclusive)
			{
				Debug.Assert(marker.TourViewId == tourView.Id, "Unexpected tour view Id for exclusive marker");
			}
		}

		if (tourPage.IsDataSheet)
			tourPage.UpdateDatabase();
		
		tourView.UpdateDatabase();

		if (viewTitleChanged || dataSheetNameChanged)
			tour.RebuildTourTreeXml();

		if (tourPage.IsDataSheet)
		{
			if (tourView.SlideId != tourPage.PageId)
			{
				tourPage.PageId = tourView.SlideId;
				tourPage.UpdateDatabase();
			}
		}
	}
	
	protected override void ReadPageFields()
	{
		if (tourPage.IsDataSheet)
		{
			dataSheetNameChanged = tourPage.Name != validDataSheetName;
			if (dataSheetNameChanged)
			{
				// Keep the data sheet page and view names in sync.
				tourPage.Name = validDataSheetName;
				tourView.Title = validDataSheetName;
			}
			
			// Use the "slide title" as the data sheet's page title so that the
			// user can show it in the title bar like they can with a map page.
			tourPage.Title = validSlideTitle;
		}
		else
		{
			viewTitleChanged = tourView.Title != validSlideTitle;
			if (viewTitleChanged)
			{
				tourView.Title = validSlideTitle;
				if (!tourView.MarkerIsRoute)
				{
					Marker marker = Account.GetCachedMarker(tourView.MarkerId);
					if (marker.MarkerType == MarkerType.Text || (marker.MarkerType == MarkerType.Photo && marker.PhotoCaptionPosition != PhotoCaptionPositionType.None))
					{
						// Regenerate the map to update the text for this hotspot's marker.
						tourPage.RebuildMap();
					}
				}
			}
		}

		tourView.SlideId = validSlideId;

		tourView.MediaType = (SlideMediaType)int.Parse(MediaSelectorComboBox.SelectedValue);

		if (!tourPage.IsDataSheet && !tourView.MarkerIsRoute)
		{
			oldMarkerId = tourView.MarkerId;
			int newMarkerId = MarkerComboBox.SelectedResourceId;
			if (oldMarkerId != newMarkerId)
			{
				// Make the new marker the last selected marker unless it's exclusive. We don't remember
				// exclusive markers because they are only good with the slide they are exclusive to.
				Marker marker = Account.GetCachedMarker(newMarkerId);
				
				if (!marker.IsExclusive)
				{
					tourView.MarkerId = newMarkerId;
					account.LastMarkerIdSelected = newMarkerId;
					newMarkerSelected = true;
				}
			}
		}

		if (tourView.MediaType == SlideMediaType.Embed)
		{
			Size size = validEmbedCodeSize;
			tourView.EmbedText = validEmbedCode;
			
			// The parser might have modified the code, so update the text box in case that happens.
			EmbedTextBox.Text = validEmbedCode;

			bool hasHtml = validEmbedCode.Trim().Length > 0;

			// If no width or height was provided, use defaults.
			Size mediaAreaSize = tourView.GetImageContainerSize();
			if (size.Width == 0)
				size.Width = hasHtml ? mediaAreaSize.Width : 0;
			if (size.Height == 0)
				size.Height = hasHtml ? mediaAreaSize.Height : 0;

			tourView.EmbedWidth = size.Width;
			tourView.EmbedHeight = size.Height;

			// Update the preview.
			EmbedPreviewPanel.Width = size.Width;
			EmbedPreviewPanel.Height = size.Height;
			SetPreviewHtml(validEmbedCode);
		}

		// Remove any instances of escaped single quotes since this text will end up in the MapsAlive.js
		// string table which uses single quotes to delimit entries. It's unlikely that someone will put
		// '\ in their text, but it has happened and it caused a JavaScript error on the string table.
        string s = HtmlEditor.Text;
        s = s.Replace("\\'", "'");
        tourView.DescriptionHtml = s;

		tourView.ToolTip = System.Web.HttpUtility.HtmlEncode(TourViewToolTipTextBox.Text);
	}

	protected override bool SetStatus()
	{
        StatusBox.Clear();

        highlightTourNavigator = false;

        if (tourPage.IsDataSheet)
            return false;

        if (tourPage.MarkersOnMap >= 2)
            return false;

        int tourViewCount = tourPage.TourViews.Count;

        if (tourViewCount == 1)
        {

            if (tourView.Image == null)
            {
                StatusBox.LastAction = "The map has 1 hotspot";
                highlightTourNavigator = true;
                ShowUploadImageInstructions();
            }
            else
            {
                StatusBox.LastAction = "The map has 1 hotspot with photo";
                StatusBox.NextAction = "Add a 2nd hotspot";
                StatusBox.ShowGraphicHotspotIcon = true;
                StatusBox.SetStep(1, AppContent.Topic("StatusNewHotspot"));
            }
            return true;
        }

        if (tourViewCount == 2 && tourView.Image == null)
        {
            StatusBox.LastAction = "The map has 2 hotspots";
            highlightTourNavigator = true;
            ShowUploadImageInstructions();
            return true;
        }

        return ShowSmartStatusBox("map");
	}

    private void ShowUploadImageInstructions()
    {
        bool usesMultimedia = tourView.MediaType != SlideMediaType.Photo;

        StatusBox.NextAction = "Edit this hotspot";
        StatusBox.SetStep(1, "[Type a title] for this hotspot.");
        StatusBox.SetStep(2, "[Choose a marker] from the marker list.");

        if (usesMultimedia)
        {
            StatusBox.SetStep(3, "[Paste HTML code] into the <b>Multimedia</b> field.");
        }
        else
        {
            StatusBox.SetStep(3, "[Upload an image]:<br>- Press the Choose File button.<br>- Choose a photo from your computer.<br>- Press the Load button.");
        }

        string altStep = "If you don't want to add an image, click the<br>";
        if (tourPage.TourViews.Count == 1)
        {
            StatusBox.ShowGraphicHotspotIcon = true;
            altStep += "<b>Add New Hotspot</b> icon to create another hotspot.";
        }
        else
        {
            StatusBox.ShowGraphicMapEditorIcon = true;
            altStep += "<b>Map Editor</b> icon to place markers on the map.";
        }
        StatusBox.SetAlternateStep(altStep);
    }

    protected override void ChangePageMode(string mode)
	{
		SlideMediaType usageType = (SlideMediaType)int.Parse(mode);

		pageValid = true;

		changingPageMode = true;
		ValidatePage();
		changingPageMode = false;
		
		if (pageValid)
		{
			tourView.MediaType = usageType;
			MediaSelectorComboBox.SelectedValue = ((int)tourView.MediaType).ToString();
		}
		else
		{
			return;
		}

		SetPreviewHtml(string.Empty);
		EmbedTextBox.Text = string.Empty;

		if (tourView.MediaType == SlideMediaType.Embed && tourView.HasImage)
		{
			// Remove the image unless using a photo marker.
			Marker marker = Account.GetCachedMarker(tourView.MarkerId);
			if (marker.MarkerType != MarkerType.Photo)
			{
				RemoveImage();
			}
		}
		
		Save();

		// Explicitly update the step-by-step instructions since ShowStatusBox
		// won't get called automatically after this handler returns.
		ShowStatusBox();
	}
	
	protected override void ImportFromUploadedFile()
	{
		Size maxSize = tourPage.IsDataSheet ? Utility.MaxImageSizeForInfoPage : Utility.MaxImageSizeForMapPage;
		
		ImportImageFromUploadedFile(maxSize, true);
	
		if (!badFileName)
			Save();
	}

	protected override void RemoveImage()
	{
		tourView.RemoveImage();

		if (tourView.TourPage.FirstTourViewId == tourView.Id)
		{
			tourView.TourPage.InvalidateThumbnail();
		}
		
		// Always save because RemoveImage changes the tour view's image Id.
		Save();
	}

	protected override void UploadSampleImage(int sampleId)
	{
		string fileName = Utility.SampleImageFileName(sampleId);
		string fileLocation = FileManager.WebAppFileLocationAbsolute("images\\samples", fileName);
		Size size;
		Byte[] imageBytes = Utility.ImageFileToByteArray(fileLocation, out size);
		ImageUploaded(fileName, size, imageBytes, sampleId);
		Save();
	}

	protected override void Undo()
	{
		ClearErrors();
	}
	
	private void ClearErrors()
	{
		ClearErrors(
			SlideIdError,
			DataSheetNameError,
			TourViewTitleError);
	}

	protected override void ValidatePage()
	{
		ClearErrors();

		validSlideTitle = TourViewTitleTextBox.Text.Trim();

		if (tourPage.IsDataSheet)
		{
			validDataSheetName = DataSheetNameTextBox.Text.Trim();
			ValidateFieldNotBlank(validDataSheetName, DataSheetNameError, Resources.Text.DataSheetNameRequired);
			if (!fieldValid)
				return;

			if (tourPage.Name != validDataSheetName)
			{
				bool nameInUse = TourPage.TourPageNameInUse(tour, tourPage.Id, validDataSheetName);
				ValidateFieldCondition(!nameInUse, DataSheetNameError, Resources.Text.ErrorTourPageNameInUse);
			}
			if (!pageValid)
				return;
		}
		else
		{
			// Validate the hotspot title. No validation is necessary for a data sheet title.
			ValidateFieldNotBlank(validSlideTitle, TourViewTitleError, Resources.Text.ErrorViewTitleRequired);
			if (!fieldValid)
				return;
		}

		if (!fieldValid)
			return;

		validSlideId = SlideIdTextBox.Text.Trim();
		ValidateFieldNotBlank(validSlideId, SlideIdError, Resources.Text.ErrorSlideIdRequired);
		if (!fieldValid)
			return;

		// Make sure the Id does not contain any disallowed characters.
		ValidateFieldIsValidFileName(validSlideId, SlideIdError, Resources.Text.ErrorIdContainsInvalidCharacters);
		if (!fieldValid)
			return;

		// Report an error it the user changed the Id and the new Id is already in use.
		if (tourPage.IsDataSheet)
		{
			if (fieldValid && tourPage.PageId != validSlideId)
			{
				bool idInUse = TourPage.TourPagePageIdInUse(tour, tourPage.Id, validSlideId);
				ValidateFieldCondition(!idInUse, SlideIdError, Resources.Text.ErrorPageIdInUse);
			}
		}
		else
		{
			if (fieldValid && tourView.SlideId != validSlideId)
			{
				bool idInUse = TourView.TourViewSlideIdInUse(tourPage, tourView, validSlideId);
				ValidateFieldCondition(!idInUse, SlideIdError, Resources.Text.ErrorSlideIdInUse);
			}
		}

		if (!fieldValid)
			return;

		if (tourView.MediaType == SlideMediaType.Embed)
		{
			if (!pageValid)
				return;

			validEmbedCode = EmbedTextBox.Text;

			if (!changingPageMode)
			{
				// Parse the embed code to get width and height. The parser might
				// modify the code, so update the text box in case that happens.
				string errorMessage;
				pageValid = MediaParser.ParseEmbedCode(ref validEmbedCode, out validEmbedCodeSize, out errorMessage);

				if (!pageValid)
				{
					SetPreviewError(errorMessage);
					return;
				}
			}
		}
	}

	protected override void ImageUploaded(string fileName, Size size, Byte[] bytes)
	{
		ImageUploaded(fileName, size, bytes, 0);
	}

	private void ImageUploaded(string fileName, Size size, Byte[] bytes, int sampleImageId)
	{
		tourView.ImageUploaded(fileName, size, bytes, sampleImageId);

		// Explicitly update the step-by-step instructions since ShowStatusBox
		// won't get called automatically after this call returns.
		ShowStatusBox();
	}

	private void SetPreviewHtml(string html)
	{
		LiteralControl panelText = EmbedPreviewPanel.Controls[0] as LiteralControl;
		panelText.Text = System.Web.HttpUtility.HtmlDecode(html);
	}

	private void SetPreviewError(string message)
	{
		LiteralControl panelText = EmbedPreviewPanelError.Controls[0] as LiteralControl;
		panelText.Text = message;
	}
}
