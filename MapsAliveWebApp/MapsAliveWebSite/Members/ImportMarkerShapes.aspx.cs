// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.IO;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class Members_ImportMarkerShapes : ImportPage
{
	private string readyMapShapesFileLocation;
	
	protected override void InitControls(bool undo)
	{
		// Set ignore as the default so that a user will not accidentally re-import slides after
		// having set the click action on them, only to have the import change the click action
		// to link to the href.
		IgnoreHref.Checked = true;
		
		int lastMarkerStyleId = account.LastResourceId(TourResourceType.MarkerStyle);
		if (lastMarkerStyleId == 0)
			lastMarkerStyleId = account.DefaultResourceId(TourResourceType.MarkerStyle);
		MarkerStyleSelector.SelectedResourceId = lastMarkerStyleId;
	}

	private void InitUploadFileControl()
	{
		if (tourPage.MapImage.IsReadyMap)
		{
			UploadFileControl.QuickHelpTitle = string.Empty;
		}
		else
		{
			UploadFileControl.QuickHelpTitle = Resources.Text.ImportMarkerShapesFileLabel;
			UploadFileControl.QuickHelpTopic = "ImportMarkersFile";
		}
		UploadFileControl.Usage("Import", "an HTML image map", ValidFileExtensionsString);
		UploadFileControl.ClearSteps();
		UploadFileControl.AddStep("Choose import options (below, optional)");
		UploadFileControl.AddStep("Choose a style for new markers (below, optional)");

		ProgressMonitor.ShowImportFileProgress(UploadFileControl.ProgressArea);
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle(Resources.Text.ImportMarkerShapesFileLabel);
		SetPageReadOnly();
		SetActionIdForPageAction(MemberPageActionId.ImportMarkerShapes);
		GetSelectedTourPage();

		if (!IsPostBack && Request.QueryString["rm"] == "1")
		{
			// The user just chose and loaded a Ready Map. The UploadMap screen transfered here.
			ChoicesPanel.Visible = false;
			MarkerStyleSectionTitlePanel.Visible = false;
			SetPageMessage("Your Ready Map has been loaded. You can start using the map now or import shapes for it.");
		}

		MarkerStyleSelector.ResourceType = TourResourceType.MarkerStyle;
	
		if (!OkToImportHotspots())
		{
			AllPanels.Visible = false;
			return;
		}

		ValidFileExtensionsArray = new string[2] { ".htm", ".html" };

		InitImportChoices();
	}

	private void InitImportChoices()
	{
		// Determine whether or not to present the user with import choices. If the map is a Ready Map,
		// the user can choose to import Ready Map shapes or import a file. Otherwise, they only get
		// to import from a file. The presentation of the import file control is slightly different in
		// each case. There is a section bar above the import control when Ready Maps can be imported,
		// and a QuickHelp explain label otherwise.

		bool showReadyMaps = tourPage.MapImage.IsReadyMap;
		if (showReadyMaps)
		{
			// Initialize the tree and make sure that is has shapes for this map.
			// If there are no shapes, don't show the tree for this Ready Map.
			showReadyMaps = ShowReadyMaps();
		}

		if (showReadyMaps)
		{
			bool useReadyMap;
			if (IsPostBack)
			{
				useReadyMap = RadioButtonImportReadyMap.Checked;
			}
			else
			{
				useReadyMap = tourPage.MapImage.IsReadyMap || Request.QueryString["readymap"] == "1";
				RadioButtonImportReadyMap.Checked = useReadyMap;
				RadioButtonImportFile.Checked = !useReadyMap;
			}

			ReadyMapsPanel.Visible = useReadyMap;
			ImportFilePanel.Visible = !useReadyMap;
			FileImportOptionsPanel.Visible = !useReadyMap;

			if (useReadyMap)
			{
				// Show the progress panel without the file upload progress bar.
				ProgressMonitor.ShowProgress(ProgressArea, "Importing", "Imported");
			}
			else
			{
				InitUploadFileControl();
			}
		}
		else
		{
			ChoicesPanel.Visible = false;
			ReadyMapsPanel.Visible = false;
			ImportFilePanelSectionTitle.Visible = false;
			InitUploadFileControl();
		}
	}

	private void HideReadyMaps()
	{
	}

	private bool ShowReadyMaps()
	{
		bool treeNotInitializedYet = ReadyMapsTree.Nodes.Count == 0;

		if (treeNotInitializedYet && tourPage.MapImage.IsReadyMap)
		{
			// The map image is a Ready Map.
			Utility.InitReadyMapsTree(ReadyMapsTree);

			// Initially hide all the nodes in the tree.
			foreach (RadTreeNode node in ReadyMapsTree.GetAllNodes())
			{
				node.Visible = false;
			}
			
			// Find the node for this map's package and then show just that part of the tree.
			foreach (RadTreeNode node in ReadyMapsTree.GetAllNodes())
			{
				string packageId = tourPage.MapImage.ReadyMapPackageId.ToString();
				if (node.Attributes["PackageId"] == packageId)
				{
					int shapesCount = 0;

					// Make all the children of the package visible.
					foreach (RadTreeNode childNode in node.Nodes)
					{
						int mapGroupId = tourPage.ReadyMapGroupId;
						bool isShapesNode = childNode.Category == "shapes";
						bool showShape = false;

						if (isShapesNode)
						{
							// Make sure the shape's group Id matches the map group Id.
							// Neither has a group Id, both will be zero and thus match.
							int shapeGroupId = tourPage.ReadyMapGroupId;
							int.TryParse(childNode.Attributes["GroupId"], out shapeGroupId);
							showShape = shapeGroupId == mapGroupId;
						}

						childNode.Visible = showShape;
						if (showShape)
						{
							childNode.BackColor = Utility.HexToColor("#fafac6");
							shapesCount++;
						}
					}

					if (shapesCount == 0)
					{
						// There are no shapes for this map.
						return false;
					}
					
					// Make the package and all of its ancestors visible. Disable all of them except
					// the root so that the entire tree can be collapsed, but not the intermediate levels.
					node.Expanded = true;
					node.Visible = true;
					node.Enabled = false;
					RadTreeNode parentNode = node.ParentNode;
					while (parentNode != null)
					{
						parentNode.Expanded = true;
						parentNode.Visible = true;
						parentNode.Enabled = parentNode.Level == 0;
						parentNode = parentNode.ParentNode;
					}

					// Use the file name minus its extension for the map description.
					string description =  tourPage.MapImage.FileNameOriginal;
					description = description.Substring(0, description.Length - 4);
					MapDescription.Text = description;
				}
			}
		}
		return true;
	}

	protected override Importer BeginImport(Stream stream, string reportTitle, string fileExt)
	{
		if (stream == null)
		{
			// This is a Ready Map.
			stream = new FileStream(readyMapShapesFileLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		}

		importer = new ImporterForMarkerShapes(tourPage, stream, reportTitle);

		((ImporterForMarkerShapes)importer).ImportAreaTags(
			MarkerStyleSelector.SelectedResourceId,
			TreatPolygonsAsLinesCheckBox.Checked,
			UseTitleForTooltipCheckBox.Checked,
			LinkToUrl.Checked,
			LinkToUrlInNewWindow.Checked
			);
		
		return importer;
	}
	
	protected override void EndImport()
	{
		account.SetLastResourceId(TourResourceType.MarkerStyle, MarkerStyleSelector.SelectedResourceId);

		ShowReport(ImportReportTable.Panel, ImportReportTable.Title, ImportReportTable.Body);

		if (tourPage != null)
			tourPage.TourViewChanged();
		tour.RebuildTourTreeXml();

		// Force the tour navigator to get refreshed so that it will display the hotspots for
		// the shapes that were just imported. Normally this happens as part of the normal page
		// loaded cycle, but this handler is called after the page load has completed. Thus,
		// once the import is complete, we have to refresh the navigator.
		masterPage.InitNavigation(this);

		if (account.HotspotLimitStatus == HotspotLimitStatus.OverLimit)
			SetPageSpecialNotice(account.HotspotLimitMessage(HotspotLimitWarningContext.TourOverLimit));

		// Collapse the tree in order to bring the report higher up on the page.
		if (RadioButtonImportReadyMap.Checked)
			ReadyMapsTree.Nodes[0].Expanded = false;
	}

	protected override Label FileNameLabel
	{
		get { return UploadFileControl.FileNameLabel; }
	}

	protected void OnUploadReadyMapShapes(object sender, RadTreeNodeEventArgs e)
	{
		string fileName = e.Node.Value;
		readyMapShapesFileLocation = FileManager.WebAppFileLocationAbsolute("App_Data", fileName);

		ImportFromLocalData(e.Node.Text);
		
		// Explicitly update the step-by-step instructions since ShowStatusBox
		// won't get called automatically after this handler returns.
		ShowStatusBox();
	}
}
