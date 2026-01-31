// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;

public partial class Members_TourExplorer : MemberPage
{
	protected override void InitControls(bool undo)
	{
		string thumbList = string.Empty;

		DataTable dataTable = account.GetTours();
		DataView dataView = dataTable.DefaultView;

		if (dataTable.Rows.Count >= 2)
		{
			SortPanel.Visible = true;

			SortToursByIdControl.OnClickActionId = MemberPageActionId.TourExplorer;
			SortToursByIdControl.QueryString = "?sort=id";
			SortToursByDateControl.OnClickActionId = MemberPageActionId.TourExplorer;
			SortToursByDateControl.QueryString = "?sort=date";
			SortToursByNameControl.OnClickActionId = MemberPageActionId.TourExplorer;
			SortToursByNameControl.QueryString = "?sort=name";

			string sortOption = Request.QueryString["sort"];
			if (sortOption == null)
				sortOption = (string)MapsAliveState.Retrieve(MapsAliveObjectType.TourExplorerSort);
			else
				MapsAliveState.Persist(MapsAliveObjectType.TourExplorerSort, sortOption);

			if (sortOption != null)
			{
				if (sortOption == "id")
				{
					dataView.Sort = "TourId desc";
					SortToursByIdControl.AppearsEnabled = false;
				}
				else if (sortOption == "date")
				{
					dataView.Sort = "BuildDate desc";
					SortToursByDateControl.AppearsEnabled = false;
				}
				else
				{
					dataView.Sort = "Name asc";
					SortToursByNameControl.AppearsEnabled = false;
				}
			}
			else
			{
				dataView.Sort = "BuildDate desc";
				SortToursByDateControl.AppearsEnabled = false;
			}
		}

		if (dataTable.Rows.Count == 0)
		{
			Response.Redirect(MemberPageAction.ActionPageTarget(MemberPageActionId.Welcome));
		}
		else
		{
			foreach (DataRowView dataViweRow in dataView)
			{
				MapsAliveDataRow row = new MapsAliveDataRow(dataViweRow.Row);
				DateTime dateBuilt = row.DateTimeValue("BuildDate");
				DateTime datePublished = row.DateTimeValue("PublishDate");
				string tourName = row.StringValue("Name");
				string tourId = row.IntValue("TourId").ToString();
				int tourPageWidth = row.IntValue("PageWidth");
				int tourPageHeight = row.IntValue("PageHeight");
				int startPageId = row.IntValue("StartPageId");
				bool tourHasBeenBuilt = row.IntValue("BuildId") != 0;
				bool tourHasBeenPublished = datePublished != DateTime.MinValue;
				int tourPageCount = row.IntValue("PageCount");
				
				// This test is not adequate because it doesn't check if the tour has been changed since
				// last built (which is different than built since published.  Ideally we should call
				// tour.HasChangedSinceLastPublished, but we don't have the actual Tour object and we
				// don't want to incur the overhead of getting it.  For now this is a defect.
				bool tourHasChangedSinceLastPublished = tourHasBeenPublished && dateBuilt > datePublished;

				thumbList += string.Format("{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}{12}{1}",
					(char)0x01,
					(char)0x02,
					tourId,
					startPageId,
					tourName,
					tourPageWidth,
					tourPageHeight, 
					0, 
					0,
					tourHasBeenBuilt ? "1" : "0",
					tourHasBeenPublished ? "1" : "0",
					tourPageCount,
					tourHasChangedSinceLastPublished ? "1" : "0"
				);
			}

			// Remove the last 0x02 delimeter.
			if (thumbList.Length > 0)
				thumbList = thumbList.Substring(0, thumbList.Length - 1);

			PageThumbs.ThumbList = thumbList;
			PageThumbs.ShowTourThumbs = true;
		}
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle(Resources.Text.AccountPageTourExplorerTitle);

		int count = account.TourCount;

		SetPageReadOnly();
		SetActionId(MemberPageActionId.TourExplorer);
		GetSelectedTourOrNone();

		// Stop remembering the last page or slide action the user took.  When they
		// choose a different tour, start out with the default actions.
		MapsAliveState.Flush(MapsAliveObjectType.LastPageAction);
		MapsAliveState.Flush(MapsAliveObjectType.LastViewAction);
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Account;
	}
}
