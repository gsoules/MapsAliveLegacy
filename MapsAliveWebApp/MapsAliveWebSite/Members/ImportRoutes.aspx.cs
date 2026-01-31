// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.IO;
using System.Web.UI.WebControls;
using System.Xml;

public partial class Members_ImportRoutes : ImportPage
{
	protected override void InitControls(bool undo)
	{
		UploadFileControl.QuickHelpTitle = Resources.Text.ImportRoutesFileLabel;
		UploadFileControl.QuickHelpTopic = "ImportRoutesFile";
		UploadFileControl.Usage("Import", "routes", ValidFileExtensionsString);
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle(Resources.Text.ImportRoutesFileLabel);
		SetPageReadOnly();
		SetActionIdForPageAction(MemberPageActionId.ImportRoutes);
		GetSelectedTourPage();

		ValidFileExtensionsArray = new string[4] { ".csv", ".xls", ".xlsx", ".xml"};

		if (!IsPostBack && Request.QueryString["delete"] == "1")
		{
			tourPage.RoutesXml = string.Empty;
			tourPage.ShowRouteList = false;
			tourPage.UpdateDatabase();
		}

		string list = string.Empty;
		
		if (tourPage.RoutesXml.Length > 0)
		{
			Routes routes = new Routes(tourPage.RoutesXml);
			XmlNodeList routeNodes = routes.RouteNodes;
			int count = routeNodes.Count;
			if (count > 0)
			{
				list = string.Format("This map has {0} route{1}:<br/>", count, Utility.Plural(count));
				
				foreach (XmlNode routeNode in routeNodes)
				{
					list += string.Format("&nbsp;&nbsp;{0}<br/>", routeNode.Attributes["id"].Value);
				}

				EnabledDeleteRoutes();
			}
		}

		if (list.Length == 0)
		{
			list = "This map has no routes";
			DeleteRoutesPanel.Visible = false;
		}

		RoutesList.Text = list;

		if (!IsPostBack)
			ProgressMonitor.ShowImportFileProgress(UploadFileControl.ProgressArea);
	}

	protected override Importer BeginImport(Stream stream, string reportTitle, string fileExt)
	{
		importer = new ImporterForRoutesSpreadsheet(tourPage, stream, reportTitle);
		((ImporterForRoutesSpreadsheet)importer).ImportRoutes(fileExt);
		return importer;
	}

	protected override void EndImport()
	{
		ShowReport(ImportReportTable.Panel, ImportReportTable.Title, ImportReportTable.Body);
		RoutesListPanel.Visible = false;
		EnabledDeleteRoutes();
	}

	protected override Label FileNameLabel
	{
		get { return UploadFileControl.FileNameLabel; }
	}

	private void EnabledDeleteRoutes()
	{
		DeleteRoutesPanel.Visible = true;
		DeleteRoutesControl.OnClickActionId = MemberPageActionId.ImportRoutes;
		DeleteRoutesControl.QueryString = "?delete=1";
		DeleteRoutesControl.WarningMessage = string.Format("Delete routes for {0}?", tourPage.Name);
	}
}
