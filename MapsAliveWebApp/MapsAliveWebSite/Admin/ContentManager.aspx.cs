// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Web.UI.WebControls;

public partial class Admin_ContentManager : MemberPage
{
	protected override void PageLoad()
	{
        Utility.RegisterHtmlEditorJavaScript(this);

        SetMasterPage(Master);
		SetPageTitle(Resources.Text.AppContentTitle);
		SetPageReadOnly();
		SetActionId(MemberPageActionId.AppContent);
		GetSelectedTourOrNone();

		if (IsPostBack)
        {
            string eventTarget = Request.Form["__EVENTTARGET"];
            if (eventTarget == "EventOnDelete")
                OnDelete();
        }
        else
        {
			UpdateGridView();
        }
	}

	private void ShowEditorControls(bool show)
	{
        HtmlEditorPanel.Visible = show;
		CancelButton.Visible = show;
		SaveButton.Visible = show;
		EditorControlsPanel.Visible = show;
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Account;
	}

	protected void OnAdd(object sender, EventArgs e)
	{
		string statusMessage;
		string name = NewTopic.Text;
		int id = AppContent.AddTopic(name, out statusMessage);
		if (id == 0)
		{
			NewTopicMessage.Text = statusMessage;
		}
		else
		{
			NewTopic.Text = string.Empty;
			NewTopicMessage.Text = string.Empty;
			UpdateGridView();
			LoadTopicIntoEditor(id, name);
		}
	}

	protected void OnCancel(object sender, EventArgs e)
	{
		// Refresh the grid to show the original text.
		UpdateGridView();
		LoadHtmlEditor(Id);
	}

	protected void OnDelete()
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_AppContent_Delete", "@Id", Id);
		UpdateGridView();
		ShowEditorControls(false);
	}

	protected void OnFilter(object sender, EventArgs e)
	{
		UpdateGridView();
	}

	protected void OnFilterClear(object sender, EventArgs e)
	{
		Filter.Text = string.Empty;
		UpdateGridView();
	}

	protected void OnFlushCache(object sender, EventArgs e)
	{
		MapsAliveState.Flush(MapsAliveObjectType.SiteContent);
	}

	protected void OnSelectRow(object sender, EventArgs e)
	{
		LoadTopicIntoEditor(SelectedRowId, SelectedRowTopic);
	}

	protected void OnSave(object sender, EventArgs e)
	{
		AppContent.UpdateTopic(Id, TopicName.Text, HtmlEditor.Text);
		UpdateGridView();
	}

	private int Id
	{
		get { return int.Parse(TopicId.Value); }
		set { TopicId.Value = value.ToString(); }
	}

	private void LoadHtmlEditor(int id)
	{
		string html = AppContent.Topic(id);
        HtmlEditor.Text = html;
	}

	private void LoadTopicIntoEditor(int id, string name)
	{
		ShowEditorControls(true);
		TopicName.Text = name;
		LoadHtmlEditor(id);
		Id = id;
	}

	private int SelectedRowId
	{
		get { return int.Parse(GridView.SelectedRow.Cells[1].Text);  }
	}

	private string SelectedRowTopic
	{
		get { return GridView.SelectedRow.Cells[2].Text; }
	}

	private void UpdateGridView()
	{
		DataTable table;

		string filterText;

		if (IsPostBack)
		{
			filterText = Filter.Text.Trim();
			MapsAliveState.Persist(MapsAliveObjectType.AppContentFilter, filterText);
		}
		else
		{
			filterText = (string)MapsAliveState.Retrieve(MapsAliveObjectType.AppContentFilter);
			if (filterText == null)
				filterText = string.Empty;
			Filter.Text = filterText;
		}

		if (filterText.Length == 0)
			table = MapsAliveDatabase.LoadDataTable("sp_AppContent_GetAll");
		else if (FilterType.Text == "Topic")
			table = GetTable("sp_AppContent_FilterByTopic");
		else if (FilterType.Text == "Text")
			table = GetTable("sp_AppContent_FilterByText");
		else
			table = GetTable("sp_AppContent_Filter");

		GridView.DataSource = table;
		GridView.DataBind();
		Results.Text = string.Format("{0} records found", table.Rows.Count);
	}

	private DataTable GetTable(string sp)
	{
		return MapsAliveDatabase.LoadDataTable(sp, "@Filter", Filter.Text.Trim());
	}
}

