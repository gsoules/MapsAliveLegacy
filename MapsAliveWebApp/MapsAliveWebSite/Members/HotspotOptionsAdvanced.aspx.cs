// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public class CategoryComparer : IComparer
{
    int IComparer.Compare(Object o1, Object o2)
    {
        return ((new CaseInsensitiveComparer()).Compare(((Category)o1).Title, ((Category)o2).Title));
    }
}

public partial class Members_HotspotOptionsAdvanced : MemberPage
{
	private bool overrideWidthChanged;
	private bool overrideHeightChanged;
	private bool showSlideDimensions;
	private int validOverrideHeight;
	private int validOverrideWidth;
	private int validZoomThreshold;

	protected void AddChangeDetection(CheckBox checkBox, string script)
	{
		checkBox.Attributes.Add("onclick", script);
	}

	protected override void EmitJavaScript()
	{
		// string loadingScript = AssignClientVar("selectedMarkerId", tourView.MarkerId);
		string loadingScript = string.Empty;
		string loadedScript = string.Empty;
		EmitJavaScript(loadingScript, loadedScript);
	}

	protected override void InitControls(bool undo)
	{
		// Show/Hide panels
		DimensionsPanel.Visible = showSlideDimensions;

		if (tourPage.IsDataSheet)
		{
			// Use alternative Quick Help Ids so that the explain text for
			// these controls is different for a data sheet versus a hotspot.
			ExcludeFromDirectory.Topic = "ExcludeFromDirectoryDataSheet";
			SlideContentCodeSnippets.Topic = "SlideContentCodeSnippetsDataSheet";
		}

		MessengerFunctionPanel.Style.Add(HtmlTextWriterStyle.Display, tourView.UsesLiveData || LiveDataCheckBox.Checked ? "block" : "none");

		if (!undo && IsPostBack)
			return;

        HotspotTitle.Text = tourView.Title;

        if (tourPage.IsDataSheet)
		{
			// Change labels that are different for a data sheet.
			ExcludeFromDirectory.Title = "Exclude From Navigation";

			// Hide controls that are not used by a data sheet.
			LiveDataSectionPanel.Visible = false;
			LiveDataPanel.Visible = false;
			CategoryPanel.Visible = false;
		}
		else
		{
			// Get the filter to be used to restrict which markers are shown. If the filter
			// is for unsued or account markers, use the all markers filter instead.
			ResourceFilters resourceFilters = account.ResourceFilters;
				
			if (account.ResourceIsFilteredBy(ResourceFilters.Category))
			{
				CategoryFilterWarningPanel.Visible = true;
				Utility.SetDivText(CategoryFilterWarningPanel, "Only showing categories used by this tour. To see all, uncheck Filter Categories on the Library menu.");
			}
		}

		ExcludeCheckBox.Checked = tourView.ExcludeFromDirectory;
		AddChangeDetection(ExcludeCheckBox);

		// Popup size override.
		if (showSlideDimensions)
		{
			OverrideWidthTextBox.Text = tourView.SlideWidthOverride == 0 ? string.Empty : tourView.SlideWidthOverride.ToString();
			OverrideHeightTextBox.Text = tourView.SlideHeightOverride == 0 ? string.Empty : tourView.SlideHeightOverride.ToString();
			AddChangeDetection(OverrideWidthTextBox);
			AddChangeDetection(OverrideHeightTextBox);
		}

		if (tourPage.IsDataSheet)
		{
			MarkerAttirbutesPanel.Visible = false;
		}
		else
		{
			// Marker Changes Size When Map Zooms option.
			ListItem item;
			MarkerZoomTypeDropDownList.Items.Clear();

			if (tourView.MarkerIsBound)
			{
				// Bound markers always zoom. Show Yes with the control disabled.
				item = new ListItem("Yes", ((int)MarkerZoomType.DoesZoom).ToString());
				item.Selected = true;
				MarkerZoomTypeDropDownList.Items.Add(item);
				MarkerZoomTypeDropDownList.Enabled = false;
			}
			else
			{
				string defaultMarkerZoomType = string.Format("{0}", tourPage.MarkersZoom ? "Yes" : "No");
				item = new ListItem(defaultMarkerZoomType, ((int)MarkerZoomType.Default).ToString());
				if (tourView.MarkerZoomType == MarkerZoomType.Default)
					item.Selected = true;
				MarkerZoomTypeDropDownList.Items.Add(item);
				item = new ListItem("Yes (for this hotspot only)", ((int)MarkerZoomType.DoesZoom).ToString());
				if (tourView.MarkerZoomType == MarkerZoomType.DoesZoom)
					item.Selected = true;
				MarkerZoomTypeDropDownList.Items.Add(item);
				item = new ListItem("No (for this hotspot only)", ((int)MarkerZoomType.DoesNotZoom).ToString());
				if (tourView.MarkerZoomType == MarkerZoomType.DoesNotZoom)
					item.Selected = true;
				MarkerZoomTypeDropDownList.Items.Add(item);
				AddChangeDetection(MarkerZoomTypeDropDownList);
			}

			// Not Anchored
			MarkerNotAnchoredCheckBox.Checked = tourView.MarkerIsNotAnchored;
			AddChangeDetection(MarkerNotAnchoredCheckBox);

			// Locked.
            if (tour.V3CompatibilityEnabled)
            {
			    MarkerLockedCheckBox.Checked = tourView.MarkerIsLocked;
			    AddChangeDetection(MarkerLockedCheckBox);
            }
            else
            {
                MarkerLocked.Visible = false;
                MarkerLockedCheckBox.Visible = false;
            }

			// Disabled.
			MarkerDisabledCheckBox.Checked = tourView.MarkerIsDisabled;
			if (account.IsPlusOrProPlan)
				AddChangeDetection(MarkerDisabledCheckBox);
			else
				MarkerDisabledCheckBox.Enabled = false;

			// Invisible.
			MarkerHiddenCheckBox.Checked = tourView.MarkerIsHidden;
			if (account.IsPlusOrProPlan)
				AddChangeDetection(MarkerHiddenCheckBox);
			else
				MarkerHiddenCheckBox.Enabled = false;

			// Static.
			MarkerStaticCheckBox.Checked = tourView.MarkerIsStatic;
			if (account.IsPlusOrProPlan)
				AddChangeDetection(MarkerStaticCheckBox);
			else
				MarkerStaticCheckBox.Enabled = false;

			// Route
			if (tourPage.IsGallery)
			{
				MarkerRouteCheckBox.Enabled = false;
			}
			else
			{
				MarkerRouteCheckBox.Checked = tourView.MarkerIsRoute;
				if (account.IsPlusOrProPlan)
					AddChangeDetection(MarkerRouteCheckBox);
				else
					MarkerRouteCheckBox.Enabled = false;
			}

			// Zoom Visibility Threshold
			MarkerZoomThresholdTextBox.Text = tourView.MarkerZoomThreshold.ToString();
			AddChangeDetection(MarkerZoomThresholdTextBox);
		}

		// Live Data
		if (account.IsPlusOrProPlan)
		{
			LiveDataCheckBox.Checked = tourView.UsesLiveData;
			LiveDataTextBox.Text = tourView.MessengerFunction;
			AddChangeDetection(LiveDataCheckBox, "maOnUseLiveDataClicked();");
			AddChangeDetection(LiveDataTextBox);
		}
		else
		{
			LiveDataCheckBox.Enabled = false;
		}

		// Show categories for this slide.
		InitCategories();

		//Show the hotspot URL.
		ShowCodeSnippets();
	}

	private void InitCategories()
	{
		if (categoryFilterChanged)
		{
			// The user just toggled category filtering on or off. Flush the category table.
			tour.CategoryFilterChanged();
		}

		CategoryManager categoryManager = tour.CategoryManager;
		ArrayList categoryTable = categoryManager.CategoryTable;
        categoryTable.Sort(new CategoryComparer());
		ListItem categoryItem;
		CheckBoxList.Items.Clear();

		foreach (Category category in categoryTable)
		{
			if (category.Id == 0)
				continue;
			categoryItem = new ListItem(string.Format("{0}&nbsp;&nbsp;&nbsp;", category.Title), category.Code);
			categoryItem.Selected = tourView.HasCategory(category);
			CheckBoxList.Items.Add(categoryItem);
		}

		bool hasCategories = CheckBoxList.Items.Count > 0;
		CategoriesPanel.Visible = hasCategories;
		NoCategoriesPanel.Visible = !hasCategories;
		if (!hasCategories)
		{
			if (!account.IsPlusOrProPlan)
				NoCategoriesMessage.Text = Account.RequiresPlanMessage("Categories", AccountPlan.Plus);
			else if (account.ResourceIsFilteredBy(ResourceFilters.Category))
				NoCategoriesMessage.Text = "This tour is not using any categories.";
			else
				NoCategoriesMessage.Text = "You don't have any categories yet. To add a category, click New > Resource > Catgegory from the menu.";
		}

		// Add change detection. Note that this will detect a click anywhere in the table of
		// checkboxes, even if no checkbox is actually clicked, but for now, it's good enough.
		CheckBoxList.Attributes.Add("onclick", "maChangeDetected();");
	}


	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetActionId(MemberPageActionId.HotspotOptionsAdvanced);
		GetSelectedTourView();
		SetPageTitle(string.Format("Advanced {0} Options", tourPage.IsDataSheet ? "Data Sheet" : "Hotspot"));

		showSlideDimensions = tourPage.SlidesPopup;
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}

	protected override void PerformUpdate()
	{
		if (tourPage.IsDataSheet)
			tourPage.UpdateDatabase();
		
		tourView.UpdateDatabase();

		// Determine if the override width or height changed.
		bool imageAreaOverrideChanged = overrideWidthChanged || overrideHeightChanged;
		
		// Update any categories that were added to or removed from this slide.
		CategoryManager categoryManager = tour.CategoryManager;
		foreach (ListItem categoryItem in CheckBoxList.Items)
		{
			Category category = categoryManager.GetCategory(categoryItem.Value);
			if (categoryItem.Selected != tourView.HasCategory(category))
			{
				if (categoryItem.Selected)
				{
					categoryManager.AddTourViewCategory(tourView, category.Code);
				}
				else
				{
					categoryManager.RemoveCategory(tourView.Id, category.Id);
				}

				if (category.Type == CategoryType.ImageAreaOverride)
					imageAreaOverrideChanged = true;
			}
		}

		if (imageAreaOverrideChanged)
		{
			tourView.SetImageChanged();
			tourView.UpdateDatabase();
		}

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
        tourView.ExcludeFromDirectory = ExcludeCheckBox.Checked;

        if (showSlideDimensions)
		{
			overrideWidthChanged = tourView.SlideWidthOverride != validOverrideWidth;
			tourView.SlideWidthOverride = validOverrideWidth;
			if (validOverrideWidth == 0)
				OverrideWidthTextBox.Text = string.Empty;
			
			overrideHeightChanged = tourView.SlideHeightOverride != validOverrideHeight;
			tourView.SlideHeightOverride = validOverrideHeight;
			if (validOverrideHeight == 0)
				OverrideHeightTextBox.Text = string.Empty;
		}

		if (!tourPage.IsDataSheet)
		{
			tourView.UsesLiveData = LiveDataCheckBox.Checked;
			tourView.MessengerFunction = LiveDataTextBox.Text;

			tourView.MarkerIsDisabled = MarkerDisabledCheckBox.Checked;
			tourView.MarkerIsHidden = MarkerHiddenCheckBox.Checked;

			if (tour.V3CompatibilityEnabled)
                tourView.MarkerIsLocked = MarkerLockedCheckBox.Checked;

			tourView.MarkerIsNotAnchored = MarkerNotAnchoredCheckBox.Checked;
			tourView.MarkerIsStatic = MarkerStaticCheckBox.Checked;
			tourView.MarkerZoomType = (MarkerZoomType)(int.Parse(MarkerZoomTypeDropDownList.SelectedValue));

			if (tourView.MarkerIsRoute != MarkerRouteCheckBox.Checked)
			{
				// The user changed the Is Marker Route option.
				tourView.MarkerIsRoute = MarkerRouteCheckBox.Checked;

				if (tourView.MarkerIsRoute)
				{
					// A route hotspot must not reference a marker so as not to create a false dependency.
					tourView.MarkerId = 0;
				}
				else
				{
					// The hotspot is no longer a route, so give it a marker.
					tourView.MarkerId = account.DefaultMarkerId;
				}
			}

			tourView.MarkerZoomThreshold = validZoomThreshold;
		}
	}

	protected override void Undo()
	{
		ClearErrors();
	}
	
	private void ClearErrors()
	{
		ClearErrors(
			OverrideWidthError,
			OverrideHeightError,
			MarkerZoomThresholdError);
	}

	protected override void ValidatePage()
	{
		ClearErrors();
		
		if (showSlideDimensions)
		{
			Size maxSize = Utility.MaxImageSizeForMapPage;
			validOverrideWidth = ValidateFieldInRange(OverrideWidthTextBox, 0, maxSize.Width, OverrideWidthError, true);
			validOverrideHeight = ValidateFieldInRange(OverrideHeightTextBox, 0, maxSize.Height, OverrideHeightError, true);
			
			if (!pageValid)
				return;
		}

		if (!tourPage.IsDataSheet)
		{
			validZoomThreshold = ValidateFieldInRange(MarkerZoomThresholdTextBox, -100, 100, MarkerZoomThresholdError);
			if (!pageValid)
				return;
		}
	}

	private void ShowCodeSnippets()
	{
		string url = string.Format("{0}?page={1}", tour.Url, tourPage.PageNumber);
		
		if (!tourPage.IsDataSheet)
			url += string.Format("&hotspot={0}", tourView.SlideId);

		if (tour.HasBeenPublished && !tour.HasChangedSinceLastPublished)
			url = string.Format("<a href='{0}' target='_blank'>{0}</a>", url);
		
		CodeSnippets.Text = url;
	}
}
