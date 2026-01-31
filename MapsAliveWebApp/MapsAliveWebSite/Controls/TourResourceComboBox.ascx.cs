// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Diagnostics;
using System.Data;
using System.Web.UI;
using Telerik.Web.UI;

public partial class Controls_TourResourceComboBox : System.Web.UI.UserControl
{
	private string onClientSelectedIndexChangedScript;
	private DataTable itemsDataTable;
	private bool resourceTypeHasImage;
	private TourResourceType resourceType = TourResourceType.Undefined;

	protected void Page_Load(object sender, EventArgs e)
	{
		// Don't emit the view state (which can be quite large if the list is long and the
		// items have images) if this control is not visible.
		EnableViewState = Visible;

		if (!Visible)
			return;

		EmitClientSideIndexChangedHandler();

		LoadItems();
	}

	public string OnClientSelectedIndexChangedScript
	{
		set { onClientSelectedIndexChangedScript = value; } 
	}

	private void EmitClientSideIndexChangedHandler()
	{
		// Only handle client side events if this control does not post back.
		if (TourResourceComboBox.AutoPostBack)
			return;
		
		TourResourceComboBox.OnClientSelectedIndexChanged = "maOnTourResourceComboBoxIndexChanged";

		// The function below is called when the user makes a selection from the combo box.
		// We emit it here dynamically instead of putting it in the aspx page so that there
		// is only ever one instance on pages that use multiple TourResourceComboBox controls.
		// The function locates the <img> tag that appears next to the combo box and sets its
		// src to be the URL of the selected item.
		string script =
			"function maOnTourResourceComboBoxIndexChanged(sender, eventArgs)" +
			"{" +
			"	var e = eventArgs.get_item().get_comboBox().get_element().parentNode.children[1];" +
			"	if (e)" +
			"		e.src = eventArgs.get_item().get_imageUrl();";

		if (onClientSelectedIndexChangedScript != null)
			script += onClientSelectedIndexChangedScript;
			
		script += "}";

		const string key = "Controls_TourResourceComboBox";
		Type type = this.GetType();
		ClientScriptManager cs = Page.ClientScript;
		if (!cs.IsClientScriptBlockRegistered(type, key))
			cs.RegisterClientScriptBlock(type, key, script, true);
	}

	public RadComboBox ComboBox
	{
		get { return TourResourceComboBox; }
	}

	public DataTable ItemsDataTable
	{
		set { itemsDataTable = value; }
	}

	private bool Loaded
	{
		get { return TourResourceComboBox.Items.Count > 0; }
	}

	public TourResourceType ResourceType
	{
		set
		{
			resourceType = value;
			resourceTypeHasImage = TourResourceManager.HasResourceImageUrl(resourceType);
			TourResourceComboBoxImage.Visible = resourceTypeHasImage;
			if (value == TourResourceType.MarkerStyle)
			{
				// Make the control wide enough so that long names are not truncated,
				// but not so long that it wont fit on the Edit Marker screen.
				TourResourceComboBox.Width = 300;
			}
		}
	}

	public void LoadItems()
	{
		Debug.Assert(resourceType != TourResourceType.Undefined, "TourResourceComboBox.ResourceType has not been set");

		if (!Loaded)
		{
			if (itemsDataTable == null)
			{
				string sp = string.Format("sp_{0}_Get{0}sOwnedByAccount", resourceType.ToString());
				itemsDataTable = MapsAliveDatabase.LoadDataTable(sp, "AccountId", Utility.AccountId);
			}

			foreach (DataRow dataRow in itemsDataTable.Rows)
			{
				MapsAliveDataRow row = new MapsAliveDataRow(dataRow);

				string idName = string.Format("{0}Id", resourceType.ToString());
				int id = row.IntValue(idName);
				string name = row.StringValue("Name");

				RadComboBoxItem item = new RadComboBoxItem();
				item.Text = name;
				item.Value = id.ToString();

				if (resourceTypeHasImage)
				{
					string resourceImageId = row.StringValue("ResourceImageId");
					if (id == 0)
						item.ImageUrl = App.WebSitePathUrl("Images/Blank.gif");
					else
						item.ImageUrl = TourResource.ResourceImageUrl(resourceType, id, resourceImageId);
				}

				TourResourceComboBox.Items.Add(item);
			}
		}

		TourResourceComboBoxImage.Src = TourResourceComboBox.SelectedItem.ImageUrl;
	}

	public int SelectedResourceId
	{
		get { return int.Parse(TourResourceComboBox.SelectedValue); }
		set
		{
			if (!Loaded)
				LoadItems();

			TourResourceComboBox.SelectedValue = value.ToString();
			if (resourceTypeHasImage)
				TourResourceComboBoxImage.Src = TourResourceComboBox.SelectedItem.ImageUrl;
		}
	}

	public void SetSelectedIndexChangedHandler(EventHandler handler)
	{
		TourResourceComboBox.SelectedIndexChanged += new RadComboBoxSelectedIndexChangedEventHandler(handler);
		TourResourceComboBox.AutoPostBack = true;
	}
}
