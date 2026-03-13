// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Text;


public class ImportReport
{
	public enum Topic
	{
		Trace,
		MapImported,
		SlideDeleted,
		SlideImported,
		SlideUnchanged,
		SlideUpdated,
		MarkerImported,
		MarkerUnchanged,
		MarkerUpdated,
		AreaTagRejected,
		AreaTagAccepted,
		ImageFileRejected,
		ResourceImported,
		RoutesImported,
		RoutesRejected,
		RoutesUnresolved,
		HotspotLimitReached,
		UserImported,
		UserRejected,
		UserUnchanged,
		UserUpdated,
		NothingImported
	}
	
	private class TopicReport
	{
		private int count;
		private string report;

		public TopicReport()
		{
		}

		public int Count
		{
			get { return count; }
		}

		public string Report
		{
			get	{ return report;	}
		}

		public void Append(string text)
		{
			report += text;
			count++;
		}
	}

	private int errorCount;
	private StringBuilder report;
	private string title;
	private Hashtable topicReports;

	public ImportReport(string title)
	{
		this.title = title;
		topicReports = new Hashtable();
		report = new StringBuilder();
	}

	public void Error(string message)
	{
		Error(message, 0);
	}

	public void EmitRow(Topic topic, string column1)
	{
		EmitRow(topic, column1, null);
	}

	public void EmitRow(Topic topic, string column1, string column2)
	{
		TopicReport topicReport = (TopicReport)topicReports[topic];
		if (topicReport == null)
		{
			topicReport = new TopicReport();
			topicReports.Add(topic, topicReport);
		}

		string text;
		string classAttribute = topic == Topic.Trace ? " class='reportTrace'" : string.Empty;
		if (column2 == null)
			text = string.Format("<tr{1}><td colspan='2' style='padding-left:16px;padding-right:16px;'>{0}</td></tr>", column1, classAttribute);
		else
			text = string.Format("<tr{2}><td style='padding-left:16px;padding-right:16px;'>{0}</td><td><div style='font-style:italic;overflow:hidden;'>{1}</div></td></tr>", column1, column2, classAttribute);

		topicReport.Append(text);
	}

	public void EmitSection(Topic topic)
	{
		TopicReport topicReport = (TopicReport)topicReports[topic];
		
		// Don't emit the section if it has no topics.
		if (topicReport == null && topic != Topic.NothingImported)
			return;

		string title;
		switch (topic)
		{
			case Topic.Trace:
				title = "Trace <span style='font-weight:normal;'>(row # appears in [])</span>";
				break;

			case Topic.MapImported:
				title = "Maps imported";
				break;
			
			case Topic.ResourceImported:
				title = "Resources imported";
				break;

			case Topic.SlideDeleted:
				title = "Hotspots deleted";
				break;

			case Topic.SlideImported:
				title = "Hotspots imported";
				break;
			
			case Topic.SlideUnchanged:
				title = "Hotspots unchanged";
				break;
			
			case Topic.SlideUpdated:
				title = "Hotspots updated";
				break;
			
			case Topic.MarkerUnchanged:
				title = "Markers unchanged";
				break;
			
			case Topic.MarkerImported:
				title = "Markers imported";
				break;
			
			case Topic.MarkerUpdated:
				title = "Markers updated";
				break;

			case Topic.AreaTagRejected:
				title = "Invalid area tags";
				break;

			case Topic.ImageFileRejected:
				title = "Files skipped because they are not images";
				break;

			case Topic.AreaTagAccepted:
				title = "Area tags that can be used as markers";
				break;

			case Topic.RoutesImported:
				title = "Routes imported";
				break;

			case Topic.RoutesRejected:
				title = "Routes rejected";
				break;

			case Topic.RoutesUnresolved:
				title = "Routes that could not be resolved";
				break;

			case Topic.HotspotLimitReached:
				title = "Hotspot limit reached";
				break;

			case Topic.UserImported:
				title = "Users imported";
				break;

			case Topic.UserRejected:
				title = "Users rejected";
				break;

			case Topic.UserUnchanged:
				title = "Users unchanged";
				break;

			case Topic.UserUpdated:
				title = "Users updated";
				break;
			
			case Topic.NothingImported:
				title = "Nothing was imported";
				break;
			
			default:
				title = "Unsupported topic " + topic;
				break;
		}

		int topMargin = report.Length == 0 ? 0 : 8;
		bool dontShowCount = topic == Topic.HotspotLimitReached || topic == Topic.NothingImported;
		string countText = dontShowCount ? "" : ": " + topicReport.Count;
		string topicText = topicReport != null ? topicReport.Report : string.Empty;

		string classAttribute = topic == Topic.Trace ? " class='reportTrace'" : string.Empty;
		report.Append(string.Format("<tr{3}><td colspan='2'><div style='margin-top:{2}px;'><b>{0}</b></div></td></tr>{1}", title, topicText, topMargin, classAttribute));
	}

	public void Error(string message, int recordNumber)
	{
		errorCount++;
		Trace("<b>Error:</b> " + message, recordNumber);
	}

	public string Rows
	{
		get
		{
			string table = string.Empty;
			if (report.Length > 0)
				table = string.Format("<table style='table-layout:fixed;'>{0}</table>", report.ToString());
			return table;
		}
	}

	public string Title
	{
		get { return title; }
	}

	public void Trace(string message)
	{
		Trace(message, 0);
	}

	public void Trace(string message, int recordNumber)
	{
		if (recordNumber > 0)
			message = string.Format("{0} [{1}]", message, recordNumber);
		message = string.Format("- {0}", message);
		EmitRow(Topic.Trace, message, null);
	}

	public void Warning(string message)
	{
		Warning(message, 0);
	}

	public void Warning(string message, int recordNumber)
	{
		string prefix = "<b>Warning:</b> ";
		errorCount++;
		Trace(prefix + message, recordNumber);
	}

	public void Warning(string message, string hotspotId)
	{
		errorCount++;
		message = string.Format("{0} : {1}", hotspotId, message);
		Trace(message);
	}
}
