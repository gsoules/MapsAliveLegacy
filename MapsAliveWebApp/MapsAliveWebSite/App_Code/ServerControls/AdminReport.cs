// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace AvantLogic.MapsAlive
{
	public enum AdminReportType
	{
		None,
		
		AccountReports,
		AccountSummary,
		AccountsCreated,
		AccountsLastActive,
		AccountsLikeUserName,
		FreeMemberships,
		PaidMemberships,
		EliteAccounts,
		
		TourReports,
		ToursForAccount,
		ToursCreated,
		ToursBuilt,
		ToursPublished,
		ToursArchived,
		ToursDownloaded,
		
		OtherReports
	}

	public class AdminReport : WebControl
	{
		private DataTable dataTable;
		private int days;
		private AdminReportType reportType;
		private MapsAliveDataRow row;
		private string searchText;
		private string statusMessage;
		private HtmlTextWriter writer;

		public AdminReport()
		{
		}

		#region ===== Properties ========================================================

		public int Days
		{
			set { days = value; }
		}

		public string SearchText
		{
			set { searchText = value; }
		}

		public string StatusMessage
		{
			get { return statusMessage; }
		}

		public AdminReportType Type
		{
			set { reportType = value; }
		}

		#endregion

		#region ===== Public ============================================================

		public void LoadData()
		{
			try
			{
				LoadDataTable();

				if (dataTable == null)
				{
					if (reportType == AdminReportType.None)
						statusMessage = "Choose a report";
					else
						statusMessage = string.Format("{0} report is not implemented yet", reportType.ToString());
				}
				else
				{
					int count = dataTable.Rows.Count;
					statusMessage = string.Format("{0} row{1} found", count, count > 1 ? "s" : "");
				}
			}
			catch
			{
				statusMessage = "Invalid search term";
			}
		}

		#endregion

		#region ===== Protected =========================================================

		protected override void RenderContents(HtmlTextWriter writer)
		{
			this.writer = writer;

			try
			{
				if (dataTable != null && dataTable.Rows.Count > 0)
				{
					writer.RenderBeginTag(HtmlTextWriterTag.Table);
					EmitReport();
					writer.RenderEndTag();
				}
			}
			catch (Exception ex)
			{
				writer.Write("An exception occurred while generating the report: " + ex.Message);
			}
		}

		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Div; }
		}
		#endregion

		#region ===== Private ===========================================================

		private void AccountsReportDetailRows()
		{
			int accountId = row.IntValue("AccountId");
			EmitToursRow(accountId, row.IntValue("TourCount"));
			AccountType type = (AccountType)row.IntValue("Type");
			bool isTrial = type == AccountType.Trial;
			EmitRow("Type", type.ToString());
			EmitRow("Hotspots", row.IntValue("SlideLimit"));
			EmitRow("Days Left", row.IntValue("Days"));
			EmitRow("Activity", string.Format("{0} days since last use", row.IntValue("Activity")));
			EmitRow("IP Address", row.StringValue("IpAddress"));
			EmitRow("Referrer", row.StringValue("Referrer"));
			EmitRow("Created", string.Format("{0} GMT", row.DateTimeValueString("CreateDate")));

			int discount = row.IntValue("DiscountPercent");
			if (discount != 0)
				EmitRow("Discount", discount.ToString() + "%");

			if (Utility.UserIsAdmin)
				EmitLoginRow(accountId, row.StringValue("UserName"));
		}

		private int AccountsReportMasterColumns()
		{
			EmitColumn(row.IntValue("AccountId"));
			EmitColumn(row.StringValue("UserName"));
			EmitColumn(row.IntValue("TourCount"));

			string date;
			if (reportType == AdminReportType.AccountsLastActive)
				date = row.DateTimeValueString("ReportYear");
			else
				date = row.StringValue("ReportYear");
			
			// Make the last column wide so that the details table won't cause the master columns to change size.
			int width = 200;
			EmitColumn(date, width);

			EmitColumn(row.StringValue("Referrer"));

			return 4;
		}

		private DataTable DataTableForAccountTypeReport(AccountType type)
		{
			return MapsAliveDatabase.LoadDataTable("sp_Account_ReportByType", "@Days", days, "@Type", (int)type);
		}

		private void EmitColumn(int value)
		{
			EmitColumn(value.ToString(), 0);
		}

		private void EmitColumn(string value)
		{
			EmitColumn(value, 0);
		}

		private void EmitColumn(int value, int width)
		{
			EmitColumn(value.ToString(), width);
		}

		private void EmitColumn(string value, int width)
		{
			if (width == 0)
				value += "&nbsp;&nbsp;";
			else
				writer.AddStyleAttribute(HtmlTextWriterStyle.Width, string.Format("{0}px", width));
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.Write(value);
			writer.RenderEndTag();
		}

		private void EmitColumn(string value, string linkUrl, bool openLinkInNewWindow)
		{
			writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "right");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			if (linkUrl == null)
			{
				writer.Write(value);
			}
			else
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Href, linkUrl, false);
				if (openLinkInNewWindow)
					writer.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
				writer.RenderBeginTag(HtmlTextWriterTag.A);
				writer.Write(value);
				writer.RenderEndTag();
			}
			writer.RenderEndTag();
		}

		private void EmitImage(string iconId, string src)
		{
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.AddAttribute(HtmlTextWriterAttribute.Id, iconId);
			writer.AddAttribute(HtmlTextWriterAttribute.Src, src);
			writer.RenderBeginTag(HtmlTextWriterTag.Img);
			writer.RenderEndTag();
			writer.RenderEndTag();
		}

		private void EmitReport()
		{
			string detailId;
			string iconId;
			int masterColumns = 1;
			int rowNumber = 0;

			foreach (DataRow dataRow in dataTable.Rows)
			{
				bool reportIsAccountSummary = reportType == AdminReportType.AccountSummary;

				row = new MapsAliveDataRow(dataRow);
				rowNumber++;
				detailId = "detail" + rowNumber;
				iconId = "icon" + rowNumber;

				// Emit the master row.
				writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "pointer");
				writer.AddAttribute(HtmlTextWriterAttribute.Onclick, string.Format("maToggleReportDetail('{0}','{1}');", detailId, iconId));
				writer.RenderBeginTag(HtmlTextWriterTag.Tr);
				EmitImage(iconId, reportIsAccountSummary ? "../Images/ReportZoomOut.gif" : "../Images/ReportZoomIn.gif");

				if (IsAccountsReport)
					masterColumns = AccountsReportMasterColumns();
				else if (IsToursReport)
					masterColumns = ToursReportMasterColumns();

				writer.RenderEndTag();

				// Emit the detail rows as a table inside a single row within a tbody.
				// We use the tbody to expand/collapse the entire detail section.
				writer.AddAttribute(HtmlTextWriterAttribute.Id, detailId);
				writer.AddStyleAttribute(HtmlTextWriterStyle.Display, reportIsAccountSummary ? "block" : "none");
				writer.RenderBeginTag(HtmlTextWriterTag.Tbody);

				writer.RenderBeginTag(HtmlTextWriterTag.Tr);

				// Span the icon column plus the master columns.
				int colspan = 1 + masterColumns;
				writer.AddAttribute(HtmlTextWriterAttribute.Colspan, colspan.ToString());

				writer.RenderBeginTag(HtmlTextWriterTag.Td);

				writer.AddStyleAttribute(HtmlTextWriterStyle.MarginLeft, "16px;");
				writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
				writer.RenderBeginTag(HtmlTextWriterTag.Table);

				if (IsAccountsReport)
					AccountsReportDetailRows();
				else if (IsToursReport)
					ToursReportDetailRows();

				writer.RenderEndTag();
				writer.RenderEndTag();
				writer.RenderEndTag();
				writer.RenderEndTag();
			}
		}

		private void EmitRow(string title, int value)
		{
			EmitRow(title, value.ToString(), null, false);
		}

		private void EmitRow(string title, decimal value)
		{
			EmitRow(title, value.ToString(), null, false);
		}

		private void EmitRow(string title, string value)
		{
			EmitRow(title, value, null, false);
		}

		private void EmitRow(string title, string value, string linkUrl, bool openLinkInNewWindow)
		{
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);
			writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "right");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.Write(title + ":");
			writer.RenderEndTag();
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			if (linkUrl == null)
			{
				writer.Write(value);
			}
			else
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Href, linkUrl, false);
				if (openLinkInNewWindow)
					writer.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
				writer.RenderBeginTag(HtmlTextWriterTag.A);
				writer.Write(value);
				writer.RenderEndTag();
			}
			writer.RenderEndTag();
			writer.RenderEndTag();
		}

		private void EmitAccountRow(int accountId)
		{
			string linkUrl = string.Format("Reports.aspx?AccountId={0}", accountId);
			EmitRow("Account", accountId.ToString(), linkUrl, false);
		}

		private void EmitBuiltTourPreviewRow(int tourId, string buildDate, int accountId)
		{
			string linkUrl = string.Format("{0}Members/{1}?tourId={2}&accountId={3}", App.WebSiteUrl, MemberPageAction.ActionPageTarget(MemberPageActionId.TourPreview), tourId, accountId);
			EmitRow("Built", buildDate, linkUrl, true);
		}

		private void EmitLoginRow(int accountId, string userEmail)
		{
			string linkUrl = string.Format("Reports.aspx?login={0}", accountId);
			EmitRow("Login as", userEmail, linkUrl, false);
		}

		private void EmitPublishedTourRow(int tourId, string publishDate)
		{
			string linkUrl = App.TourUrl(tourId);
			EmitRow("Published", publishDate, linkUrl, true);
		}

		private void EmitToursRow(int accountId, int count)
		{
			string linkUrl = count == 0 ? null : string.Format("Reports.aspx?accountId={0}&tours=1", accountId);
			EmitRow("Tours", row.IntValue("TourCount").ToString(), linkUrl, false);
		}

		private bool IsAccountsReport
		{
			get { return reportType > AdminReportType.AccountReports && reportType < AdminReportType.TourReports; }
		}

		private bool IsToursReport
		{
			get { return reportType > AdminReportType.TourReports && reportType < AdminReportType.OtherReports; }
		}

		private void LoadDataTable()
		{
			const int allDays = 100000;

			switch (reportType)
			{
				case AdminReportType.None:
					break;

				case AdminReportType.AccountSummary:
					dataTable = MapsAliveDatabase.LoadDataTable("sp_Account_ReportByAccountId", "@Days", allDays, "@AccountId", searchText);
					break;

				case AdminReportType.AccountsCreated:
					dataTable = MapsAliveDatabase.LoadDataTable("sp_Account_ReportCreated", "@Days", days);
					break;

				case AdminReportType.AccountsLastActive:
					dataTable = MapsAliveDatabase.LoadDataTable("sp_Account_LastActive", "@Days", days);
					break;

				case AdminReportType.AccountsLikeUserName:
					dataTable = MapsAliveDatabase.LoadDataTable("sp_Account_ReportByLikeUserName", "@Days", allDays, "@UserName", searchText);
					break;

				case AdminReportType.EliteAccounts:
					dataTable = DataTableForAccountTypeReport(AccountType.Elite);
					break;

				case AdminReportType.FreeMemberships:
					dataTable = DataTableForAccountTypeReport(AccountType.Trial);
					break;

				case AdminReportType.PaidMemberships:
					dataTable = DataTableForAccountTypeReport(AccountType.Paid);
					break;

				case AdminReportType.ToursCreated:
					dataTable = MapsAliveDatabase.LoadDataTable("sp_Tour_ReportCreated", "@Days", days);
					break;

				case AdminReportType.ToursBuilt:
					dataTable = MapsAliveDatabase.LoadDataTable("sp_Tour_ReportToursBuilt", "@Days", days);
					break;

				case AdminReportType.ToursPublished:
					dataTable = MapsAliveDatabase.LoadDataTable("sp_Tour_ReportToursPublished", "@Days", days);
					break;

				case AdminReportType.ToursDownloaded:
					dataTable = MapsAliveDatabase.LoadDataTable("sp_Tour_ReportToursDownloaded", "@Days", days);
					break;

				case AdminReportType.ToursArchived:
					dataTable = MapsAliveDatabase.LoadDataTable("sp_Tour_ReportToursArchived", "@Days", days);
					break;
				
				case AdminReportType.ToursForAccount:
					dataTable = MapsAliveDatabase.LoadDataTable("sp_Tour_ReportByAccountId", "@Days", allDays, "@AccountId", searchText);
					break;

				default:
					break;
			}
		}

		private void ToursReportDetailRows()
		{
			int tourId = row.IntValue("TourId");
			
			EmitRow("Tour Id", tourId.ToString());
			
			
			string buildDate = row.DateTimeValueString("BuildDate");
			string publishDate = row.DateTimeValueString("PublishDate");
			
			if (buildDate.Length == 0)
				EmitRow("Built", "Not built");
			else
				EmitBuiltTourPreviewRow(tourId, buildDate, row.IntValue("AccountId"));
			if (publishDate.Length == 0)
				EmitRow("Published", "Not published");
			else
				EmitPublishedTourRow(tourId, publishDate);
			
			EmitRow("Created", row.DateTimeValueString("CreateDate"));
			EmitRow("Pages", row.IntValue("PageCount"));
			EmitRow("Hotspots", row.IntValue("ViewCount"));
			
			EmitAccountRow(row.IntValue("AccountId"));
		}

		private int ToursReportMasterColumns()
		{
			EmitColumn(row.StringValue("Name"));
			EmitColumn(row.StringValue("Account"));

			int slides = row.IntValue("ViewCount");
			EmitColumn(slides);

			int tourId = row.IntValue("TourId");
			string buildDate = row.DateTimeValueString("BuildDate");
			if (buildDate.Length == 0)
			{
				writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "right");
				EmitColumn(tourId.ToString(), 40);
			}
			else
			{
				int accountId = row.IntValue("AccountId");
				string linkUrl = string.Format("{0}Members/{1}?tourId={2}&accountId={3}", App.WebSiteUrl, MemberPageAction.ActionPageTarget(MemberPageActionId.TourPreview), tourId, accountId);
				EmitColumn(tourId.ToString(), linkUrl, true);
			}
			EmitColumn("&nbsp;" + row.StringValue("ReportYear"));

			return 3;
		}
	#endregion
	}
}
