// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Diagnostics;

public class AppContent
{
    const string noRecord = "Help text for this feature is coming soon.";

    public AppContent()
	{
	}

	public static int AddTopic(string newTopic, out string statusMessage)
	{
		string topic = newTopic.Trim();
		
		if (topic.Length == 0)
		{
			statusMessage = "The topic cannot be blank";
			return 0;
		}

		if (TopicOptional(topic) != string.Empty)
		{
			statusMessage = "That topic already exists";
			return 0;
		}

		try
		{
			int id = (int)MapsAliveDatabase.ReadScalar("sp_AppContent_CreateTopic", "@Topic", topic);
			{
				statusMessage = "Topic was added";
				MapsAliveState.Flush(MapsAliveObjectType.SiteContent);
				return id;
			}
		}
		catch (Exception ex)
		{
			statusMessage = ex.Message;
			return 0;
		}
	}

	public static void UpdateTopic(int id, string topic, string text)
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_AppContent_Update", "@Id", id, "@Topic", topic, "@Text", text);
		MapsAliveState.Flush(MapsAliveObjectType.SiteContent);
	}

	public static string Topic(string topic)
	{
		// Get topic text by its topic name.
		string filterExp = string.Format("Topic = '{0}'", topic);
		return TopicData(filterExp);
	}

	public static string Topic(int contentId)
	{
		// Get topic text by its content Id.
		string filterExp = string.Format("Id = {0}", contentId);
		return TopicData(filterExp);
	}

    // This method is needed during the transition from V3 to V4 in order to convert the newlines that
    // used in V3 alert and confirm message into HTML line breaks, and eventually additional styling,
    // used by the Vex alert and confirm dialogs. Once V4 has been deployed, AppContent using
    // \n for the dialogs should be replaced with HTML, and this method can be replaced by direct
    // calls to Topic.
    public static string TopicHtml(string topic)
    {
        string text = Topic(topic);
        return text.Replace("\\n", "<br />");
    }

    public static string TopicOptional(string topic)
	{
		string value = Topic(topic);
		if (value == noRecord)
			return string.Empty;
		else
			return value;
	}


	// ----- Private methods -----

	private static DataTable ContentDataTable()
	{
		// Get the content data table from cache.
		DataTable dataTable = (DataTable)MapsAliveState.Retrieve(MapsAliveObjectType.SiteContent);
		if (dataTable == null)
		{
			// The cache is empty.  Create a new table from the database and cache it.
			dataTable = MapsAliveDatabase.LoadDataTable("sp_AppContent_GetAll");
			MapsAliveState.Persist(MapsAliveObjectType.SiteContent, dataTable);
		}

		return dataTable;
	}

	private static string TopicData(string filterExp)
	{
		// Get the text for a single content item.
		string data;
		DataTable dataTable = ContentDataTable();

		if (dataTable == null)
		{
			Debug.Fail(string.Format("TopicData request for \"{0}\" returned null data table", filterExp));
			data = "";
		}
		else
		{
			DataRow[] rows = dataTable.Select(filterExp);
			if (rows.Length == 1)
			{
				object value = rows[0]["Text"];
				if (value is DBNull)
					data = string.Empty;
				else
					data = (string)value;
			}
			else
			{
				// Note that this string is tested in TopicOptional above.  If you
				// change this text here, make sure you change the test there.
				data = noRecord;
			}
		}
		return data;
	}
}
