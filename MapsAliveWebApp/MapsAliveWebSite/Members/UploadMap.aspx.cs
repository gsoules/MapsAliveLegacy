// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class Members_UploadMap : ImageUploadPage
{
    bool highlightTourNavigator;

    protected override void EmitJavaScript()
	{
        string loadedScript = string.Format("maHighlightTourNavigator({0});", highlightTourNavigator ? "true" : "false");
        EmitJavaScript(string.Empty, loadedScript);
	}

	protected void AddChangeDetection(CheckBox checkBox, string script)
	{
		checkBox.Attributes.Add("onclick", script);
	}

	protected void AddChangeDetection(TextBox textBox, string script)
	{
		base.AddChangeDetection(textBox);
		textBox.Attributes.Add("onchange", script);
	}

	protected override void InitControls(bool undo)
	{
		Size mapAreaSize = tourPage.MapAreaSize;

		bool showMapImage = tourPage.MapImage.HasFile;

		ImagePreviewPanel.Visible = showMapImage;
		RemoveMapPanel.Visible = showMapImage;

		if (showMapImage)
		{
			Size imageSize = tourPage.ScaledMapSize;
			ImageSizeActual.Text = string.Format("{0} x {1}", tourPage.MapImage.Size.Width, tourPage.MapImage.Size.Height);

			if (tourPage.MapCanZoom)
			{
				// Hide the scaled size message since it's not meaningful with map zoom or with SVG maps.
				ScaleSizeMessage.Style.Add(HtmlTextWriterStyle.Display, "none");
			}
			else
			{
				ImageSizeScaled.Text = string.Format("{0} x {1}", imageSize.Width, imageSize.Height);
			}

			if (!badFileName)
			{
				FileName.Text = tourPage.MapImage.FileNameOriginal;
				MapPreviewImage.ImageUrl = string.Format("ImageRenderer.ashx?type=map&width=715&height=715");
			}
		}
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetActionIdForPageAction(MemberPageActionId.UploadMap);
		SetPageReadOnly();
		GetSelectedTourPage();
		
		SetPageTitle(tourPage.IsGallery ? "Choose Background Image" : Resources.Text.UploadMapPageTitle);
		ReadyMapsMessagePanel.Visible = false;

		InitUploadChoices();

		if (!IsPostBack)
		{
			if (Request.QueryString["rm"] == "1")
			{
				if (tourPage.TourViews.Count == 0)
				{
					// The user just chose and loaded a Ready Map and they have not previously
					// imported or added any hotspots. Send them to the import shapes screen.
					string targetPage = MemberPageAction.ActionPageTarget(MemberPageActionId.ImportMarkerShapes);
					Response.Redirect(targetPage + "?rm=1");
				}
				else
				{
					// Show them a message saying hot they can import shapes.
					HelpMessage.Text = "You can import shapes for this map by choosing Tour > Import > Marker Shapes from the menu.";
				}
			}

			ProgressMonitor.ShowUploadFileProgress(ProgressArea);
		}
	}

	protected override void PagePreRender()
	{
		base.PagePreRender();

		if (badFileName)
			SetPageError(badFileNameMessage);
	}

	protected override void ImageUploaded(string fileName, Size size, Byte[] bytes)
	{
		// If this is the first time a map has been uploaded, change the last map action Id to
		// be the Map page instead of this Upload Map page. We do this so that the next time
		// the user clicks Map in the Page Menu, they won't come back here.
		if (!tourPage.MapImage.HasFile)
			account.SetLastActionIdForGroup(MemberPageActionId.MapProperties, MemberPageActionId.Map);

		tourPage.ImageUploaded(fileName, size, bytes, false);

		InitPageControls();

		tourPage.HasNeverHadMap = false;
        tourPage.MapFocusPercent = 0;
        tourPage.MapFocus = 0;
	}

	protected override void ImportFromUploadedFile()
	{
		// Limit the size to 4096 which is up from the original Flash limitation of 3840.
        // There's no functional reason for not allowing a still larger size, but large
        // files take longer to download and use up more disk space on the server.
		Size maxSize = new Size(4096, 4096);

		ImportImageFromUploadedFile(maxSize, true);
		
		if (!badFileName)
		{
			// A readyMapPackageId of zero means this is not a sample image or a ReadyMap.
			tourPage.MapImage.ReadyMapPackageId = 0;
			
			Save();
		}

		// Explicitly update the step-by-step instructions since ShowStatusBox
		// won't get called automatically after this handler returns.
		ShowStatusBox();
	}

	private void InitUploadChoices()
	{
		if (tour.V4 && !tourPage.MapImage.IsReadyMap)
        {
            ChoicesPanel.Visible = false;
            UploadFilePanelSectionTitle.CssClass = "optionsSectionTitlePlain";
            UploadFilePanelSectionTitle.Style.Add(HtmlTextWriterStyle.Margin, "0");
        }
        else if (!IsPostBack && tourPage.IsGallery)
		{
			ChoicesPanel.Visible = false;
			SelectUploadFileOption(true);
			SelectReadyMapsOption(false);
			PreviewLabel.Text = "Background Image Preview";
			ButtonRemove.Text = "Remove Image";
		}

        // Support for Ready Maps is allowed in V4 but only in V3 Compatibility mode.
		bool isReadyMap = RadioButtonReadyMap.Checked;
		bool isFileMap = RadioButtonUploadFile.Checked;
		if (!isReadyMap && !isFileMap)
		{
			isReadyMap = tourPage.MapImage.IsReadyMap;
			if (!isReadyMap)
				isFileMap = true;
		}
		SelectUploadFileOption(isFileMap);
		SelectReadyMapsOption(isReadyMap);
		if (isReadyMap)
			ShowReadyMaps();
	}

	protected void OnUploadReadyMap(object sender, RadTreeNodeEventArgs e)
	{
		Debug.Assert(e != null, "OnUploadReadyMap: event arg is null");
		Debug.Assert(e.Node != null, "OnUploadReadyMap: clicked node is null");
		Debug.Assert(e.Node.Value != null, "OnUploadReadyMap: clicked node value is null");

		try
		{
			// Get the node that was clicked.
			string value = e.Node.Value;
			RadTreeNode parentNode = e.Node.ParentNode;
			
			// Get its package Id and save it with the map.
			int packageId;
			int.TryParse(parentNode.Attributes["PackageId"], out packageId);
			tourPage.MapImage.ReadyMapPackageId = packageId;
			
			// Get the map's group Id if it has one.
			int groupId;
			int.TryParse(e.Node.Attributes["GroupId"], out groupId);
			tourPage.SetReadyMapGroupId(groupId);
			
			// Create a file name that we'll use to describe this map.
			// We don't use the actual file name since it reveals our internal file structure.
			string fileName = string.Format("{0}/{1}.jpg", e.Node.ParentNode.Text, e.Node.Text);

			// Determine the location of the file associated with the node.
			string fileLocation = FileManager.WebAppFileLocationAbsolute("App_Data", value);

			// Set popup options that are best suited for Ready Maps.
			if (tourPage.HasNeverHadMap && tourPage.SlidesPopup)
			{
				// We only do this the first time a map is loaded into a TourPage. We don't do it again
				// if the user chooses a different Ready Map, or if they remove the map and then choose
				// a Ready Map, because we don't know if they changed the popup options along the way.
				// Note that the HasNeverHadMap flag will get cleared any map image gets loaded.
				tourPage.PopupOptions.SetReadyMapDefaults();
			}
			
			// Load the map image.
			UploadReadyMapFromFile(fileName, fileLocation);

			// Redirect back to this page so that when it loads it will know the map area dimensions
			// for the newly chosen Ready Map. These are needed when EmitJavaScript is called to set
			// the size of the Flash map editor stage. If we don't redirect back, the stage will be for
			// the previous map's size since EmitJavaScript is called before OnUploadReadyMap is called.
			TransferToMemberPage(MemberPageAction.ActionPageTarget(MemberPageActionId.UploadMap) + "?rm=1");
		}
		catch (Exception ex)
		{
			Debug.Fail("OnUploadReadyMap: " + ex.Message); // TEMP until we figure out why mail is not sent for this error.
			SetPageSpecialWarning("An unexpected error occurred while trying to upload a Ready Map. It has been reported to MapsAlive technical support.");
			string nodeText = e != null && e.Node != null && e.Node.Text != null ? e.Node.Text : "NO NODE TEXT";
			Utility.ReportError("OnUploadReadyMap: " + nodeText, ex.Message);
		}
	}

	protected override void RemoveImage()
	{
		// Delete the map image files from the preview folder.
		tourPage.MapImage.DeleteMapImagesFromPreviewFolder(tour.Id);

		// Delete the image's data, but don't delete it's placeholder record from the database.
		// We need to keep the record so we can use it if the user uploads another map image.
		tourPage.MapImage.EraseBytes();
		tourPage.MapImage.ReadyMapPackageId = 0;
		tourPage.MapImage.UpdateImageInDatabase();
		
		tourPage.SetMapImageChanged();
		tourPage.RebuildMap();
		tourPage.UpdateDatabase();

        // Explicitly update the step-by-step instructions since ShowStatusBox
        // won't get called automatically after this handler returns.
        ShowStatusBox();
    }

    private void SelectReadyMapsOption(bool select)
	{
		RadioButtonReadyMap.Checked = select;
		ReadyMapsPanel.Visible = select;
	}

	private void SelectUploadFileOption(bool select)
	{
		RadioButtonUploadFile.Checked = select;
		UploadFilePanel.Visible = select;
	}

	protected override bool SetStatus()
	{
        highlightTourNavigator = false;

        if (tourPage.IsGallery)
            return false;

        StatusBox.Clear();

        bool isNewMap = Request.QueryString["newmap"] == "1" && !tourPage.MapImage.HasFile;
        
        if (isNewMap)
        {
            highlightTourNavigator = true;
            ShowUploadMapImageStatusBox();
            return true;
        }

        int tourViewCount = tourPage.TourViews.Count;

        if (tourPage.MapImage.HasFile && tourViewCount == 0)
        {
            StatusBox.LastAction = "Your map image<br>has been uploaded";
            StatusBox.NextAction = "Add a hotspot to this map";
            StatusBox.ShowGraphicHotspotIcon = true;
            StatusBox.SetStep(1, AppContent.Topic("StatusNewHotspot"));
            return true;
        }

        return ShowSmartStatusBox("map");
	}

	private void ShowImportShapesMessage()
	{
		ReadyMapsMessage.Text = string.Format(AppContent.Topic("ReadyMapHasShapes"), Utility.MemberPageLink("Import Shapes", MemberPageActionId.ImportMarkerShapes));
		ReadyMapsMessagePanel.Visible = true;

		// Collapse the tree in order to bring the map image higher up on the page.
		ReadyMapsTree.Nodes[0].Expanded = false;
	}

	private void ShowReadyMaps()
	{
		if (ReadyMapsTree.Nodes.Count == 0)
		{
			// The tree has not been initialized yet.

			Utility.InitReadyMapsTree(ReadyMapsTree);

			foreach (RadTreeNode node in ReadyMapsTree.GetAllNodes())
			{
				int packageId;
				int.TryParse(node.Attributes["PackageId"], out packageId);

				// Expand the samples if this map does not have a file yet.
				if (packageId == 5000 && !tourPage.MapImage.HasFile)
					node.Expanded = true;

				node.Visible = node.Category != "shapes";
			}
		}
	}

	private void UploadReadyMapFromFile(string fileName, string fileLocation)
	{
		Byte[] bytesFile;
		Size sizeFile;

		// Get the jpg version of the Ready Map file.
		bytesFile = Utility.ImageFileToByteArray(fileLocation, out sizeFile);

		ImageUploaded(fileName, sizeFile, bytesFile);
		
		Save();
	}

	protected override void UploadSampleImage(int sampleId)
	{
		string fileName = Utility.SampleImageFileName(sampleId);
		string fileLocation = FileManager.WebAppFileLocationAbsolute("images\\samples", fileName);
		Size size;
		Byte[] imageBytes = Utility.ImageFileToByteArray(fileLocation, out size);
		tourPage.MapImage.ReadyMapPackageId = 0;
		ImageUploaded(fileName, size, imageBytes);
		Save();

		// Force a reset of the status to make the sample map images go away.
		SetStatus();
	}
}
