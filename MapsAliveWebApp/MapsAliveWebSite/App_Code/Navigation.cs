// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using AvantLogic.MapsAlive;
using Telerik.Web.UI;

// This class exists to allow the MemberPage class to access navigation controls
// that are defined in MemberPage.master.  MemberPage.master's Page_Init method
// sets controls in this class (MasterTopMenu, MasterTabs, etc.) which MemberPage
// can then access via this class's properties.  If we didn't do it this way,
// every subclass of MemberPage would have to explicity tell MemberPage about
// the navigation controls in MemberPage.master.

public class Navigation : System.Web.UI.MasterPage
{

	private Account account;
	private MemberPageActionId actionId;
	private bool enableColorSchemes;
	private string formId;
	private string formContentId;
	private RadMenu masterTopMenu;
	private MemberPage memberPage;
	private Panel masterAllToursPanel;
	private Panel masterCurrentTourPanel;
	private MemberPageHeader masterPageHeader;
	private HiddenField masterPostId;
	private Label masterSpecialMessage;
	private Panel masterSpecialMessagePanel;
	private ScriptManager masterScriptManager;
	private StatusBox masterStatusBox;
	private TourList masterTourList;
	private GroupNavigator masterTourNavigator;
	private Tour tour;
	private TourPage tourPage;
	private TourView tourView;
	private bool userIsAdmin;

	public Navigation()
	{
	}

	public MemberPageActionId ActionId
	{
		get { return actionId; }
		set { actionId = value; }
	}

	private void AddRecentResource(RadMenuItem recentGroup, TourResource resource, MemberPageActionId resourceActionId, int id)
	{
		if (actionId == resourceActionId)
		{
			// Don't add a recent resource if displaying that's resource's edit screen.
			return;
		}

		string name = resource.ResourceType == TourResourceType.Category ? ((Category)resource).Title : resource.Name;
		string itemText = RecentItemText(TourResourceManager.GetTitle(resource.ResourceType), name);
		RadMenuItem newItem = new RadMenuItem(itemText);
		recentGroup.Items.Add(newItem);
		string queryString = string.Format("id={0}", id);
		SetMenuItemTargetPage(newItem, resourceActionId, queryString);
	}
	
	private void DisableMenuGroup(string groupId)
	{
		RadMenuItem group = FindMenuGroup(groupId);
		if (group != null)
			group.Enabled = false;
	}

	private void DisableMenuItem(MemberPageActionId actionId)
	{
		RadMenuItem item = FindMenuItem(actionId);
		if (item != null)
			item.Enabled = false;
	}

	private RadMenuItem FindMenuGroup(string groupId)
	{
		foreach (RadMenuItem group in masterTopMenu.Items)
		{
			if (group.Attributes["GroupId"] == groupId)
				return group;
			
			foreach (RadMenuItem item in group.Items)
			{
				if (item.Attributes["GroupId"] == groupId)
					return item;
			}
		}
		return null;
	}

	private RadMenuItem FindMenuItem(MemberPageActionId actionId)
	{
		return FindMenuItem(actionId.ToString());
	}

	private RadMenuItem FindMenuItem(string itemValue)
	{
		RadMenuItem item = masterTopMenu.FindItemByValue(itemValue);
		System.Diagnostics.Debug.Assert(item != null, "Attempt to find nonexistant menu item: " + itemValue);
		return item;
	}

	public string FormId
	{
		get { return formId; }
		set { formId = value; }
	}

	public string FormContentId
	{
		get { return formContentId; }
		set { formContentId = value; }
	}

	private void GetFilterAndUsageFlags(ref bool showCategoryFilterOption, ref bool showMarkerFilterOption, ref bool showUsageOptions, ref TourResourceType tourResourceType)
	{
		switch (actionId)
		{
			case MemberPageActionId.CategoryExplorer:
				tourResourceType = TourResourceType.Category;
				showCategoryFilterOption = true;
				break;

			case MemberPageActionId.FontStyleExplorer:
				tourResourceType = TourResourceType.FontStyle;
				break;

			case MemberPageActionId.MarkerStyleExplorer:
				tourResourceType = TourResourceType.MarkerStyle;
				break;

			case MemberPageActionId.MarkerExplorer:
				tourResourceType = TourResourceType.Marker;
				showMarkerFilterOption = true;
				break;

			case MemberPageActionId.SymbolExplorer:
				tourResourceType = TourResourceType.Symbol;
				break;

			case MemberPageActionId.TooltipStyleExplorer:
				tourResourceType = TourResourceType.TooltipStyle;
				break;

			case MemberPageActionId.ColorSchemeExplorer:
				tourResourceType = TourResourceType.TourStyle;
				break;

			case MemberPageActionId.EditHotspotContent:
				showCategoryFilterOption = true;
				showMarkerFilterOption = true;
				break;

			case MemberPageActionId.HotspotOptionsAdvanced:
				showCategoryFilterOption = true;
				break;

			default:
				showUsageOptions = false;
				break;
		}

		if (tour == null)
		{
			showCategoryFilterOption = false;
			showMarkerFilterOption = false;
		}
	}

	private TourResource GetResourceForAction()
	{
		TourResource resource = null;

		switch (actionId)
		{
			case MemberPageActionId.EditCategory:
				resource = memberPage.Category;
				break;

			case MemberPageActionId.EditMarker:
				resource = memberPage.Marker;
				break;

			case MemberPageActionId.EditSymbol:
				resource = memberPage.Symbol;
				break;

			case MemberPageActionId.EditFontStyle:
				resource = memberPage.FontStyleResource;
				break;

			case MemberPageActionId.EditMarkerStyle:
				resource = memberPage.MarkerStyle;
				break;

			case MemberPageActionId.EditTooltipStyle:
				resource = memberPage.TooltipStyle;
				break;

			case MemberPageActionId.EditColorScheme:
				resource = memberPage.ColorScheme;
				break;

			default:
				break;
		}
		return resource;
	}

	private void HideMenuGroup(string groupId)
	{
		RadMenuItem group = FindMenuGroup(groupId);
		if (group != null)
			group.Visible = false;
	}

	private void HideMenuItem(MemberPageActionId actionId)
	{
		HideMenuItem(actionId.ToString());
	}

	private void HideMenuItem(string itemValue)
	{
		RadMenuItem item = FindMenuItem(itemValue);
		if (item != null)
			item.Visible = false;
	}

	private void HideSeparator(int separatorNumber)
	{
		HideMenuItem("Separator" + separatorNumber);
	}

	private void InitAccountItems()
	{
		if (account.IsTrial)
		{
			HideMenuItem(MemberPageActionId.UpgradeMembership);
			HideMenuItem(MemberPageActionId.RenewMembership);
			HideMenuItem(MemberPageActionId.OrderHistory);
			ShowMenuItem(MemberPageActionId.BuyPlan);
		}
		else
		{
			HideMenuItem(MemberPageActionId.BuyPlan);

			if (account.DaysRemaining == 0)
			{
				HideMenuItem(MemberPageActionId.UpgradeMembership);
			}
			else if (account.Plan == AccountPlan.Starter)
			{
				SetMenuItemText(MemberPageActionId.UpgradeMembership, "Upgrade Your Plan");
			}
			else if (account.Plan == AccountPlan.Personal)
			{
                HideMenuItem(MemberPageActionId.UpgradeMembership);
                // SetMenuItemText(MemberPageActionId.UpgradeMembership, "Upgrade to the Plus or Pro Plan");
            }
            else if (account.IsPlusPlan)
			{
                HideMenuItem(MemberPageActionId.UpgradeMembership);
                // SetMenuItemText(MemberPageActionId.UpgradeMembership, "Upgrade to the Pro Plan");
            }
            else if (account.IsProPlan)
			{
				HideMenuItem(MemberPageActionId.UpgradeMembership);
				ShowMenuItem(MemberPageActionId.BuyHotspots);
                DisableMenuItem(MemberPageActionId.BuyHotspots);
            }

			if (account.PaymentAmount > 0)
			{
				ShowMenuItem(MemberPageActionId.BuyCustomServices);
			}

			if (account.DaysRemaining > 60)
			{
				HideMenuItem(MemberPageActionId.RenewMembership);
			}
		}

		if (!userIsAdmin)
		{
			HideSeparator(8);
			HideMenuGroup("Admin");
		}

		if (Utility.UserIsMapsAlive)
		{
			ShowMenuItem(MemberPageActionId.AppContent);
			ShowMenuItem(MemberPageActionId.RecoverPassword);
			ShowMenuItem(MemberPageActionId.UserAccounts);
			ShowMenuItem(MemberPageActionId.Reports);
			ShowMenuItem(MemberPageActionId.ReportAccountStatistics);
		}
		else
		{
			HideSeparator(9);
			HideSeparator(10);
			HideSeparator(11);
		}
	}

	private void InitCurrentTourNavigator()
	{
		if (masterTourNavigator == null)
			return;

		if (tour != null)
		{
			masterTourNavigator.Xml = tour.TourTreeXml;
			masterTourNavigator.SetRootId(tour.Id);
		}

		if (tourPage != null)
		{
			string nodeValue = "p" + tourPage.Id;
			masterTourNavigator.SetSelectedGroup(nodeValue);
		}

		if (tourView != null)
		{
			string nodeValue = "s" + tourView.Id;
			masterTourNavigator.SetSelectedItem(nodeValue);
		}
	}

	private ResourceFilters InitFilterMenuItem(bool showAll, string itemValue, ResourceFilters filter)
	{
		ResourceFilters resourceFilters;
		resourceFilters = account.ResourceFilters;
		if (showAll)
		{
			resourceFilters = account.SetResourceIsFiltered(resourceFilters, filter, true);
			ShowMenuItemAsChecked(false, itemValue);
		}
		else
		{
			resourceFilters = account.SetResourceIsFiltered(resourceFilters, filter, false);
			ShowMenuItemAsChecked(true, itemValue);
		}
		SetMenuItemTargetPage(itemValue, actionId, string.Format("filter={0}", (int)resourceFilters));
		return resourceFilters;
	}

	private void InitMenu()
	{
		userIsAdmin = Utility.UserIsAdmin;

		//enableColorSchemes = Utility.UserIsMapsAlive;
		enableColorSchemes = true;

		// Load the menu items.
		masterTopMenu.LoadXml(MenuXml());

		// Set the target page for each menu item based on the item's value which is an action Id.
		foreach (RadMenuItem group in masterTopMenu.Items)
		{
			foreach (RadMenuItem item in group.Items)
			{
				if (item.Value.StartsWith("Title_"))
				{
					SetMenuTitle(item);
				}
				else if (item.Value.StartsWith("_"))
				{
					// The _ prefix is used in placeholders for dynamically set actions.
					continue;
				}
				else
				{
					SetMenuItemTargetPage(item);
					if (item.Value == actionId.ToString())
					{
						ShowMenuGroupAsSelected(group);
						ShowMenuItemAsSelected(item);
					}

					foreach (RadMenuItem subItem in item.Items)
					{
						if (subItem.Value.StartsWith("Title_"))
						{
							SetMenuTitle(subItem);
						}
						else
						{
							SetMenuItemTargetPage(subItem);
							if (subItem.Value == actionId.ToString())
							{
								ShowMenuGroupAsSelected(group);
								ShowMenuGroupAsSelected(item);
								ShowMenuItemAsSelected(subItem);
							}
						}
					}
				}
			}
		}

		InitResourceLibrary();
		InitAccountItems();
		InitTourBuilderItems();
		InitRecentItems();

		if (tour == null)
		{
			HideMenuGroup("Tour");
			ShowMenuGroup("TourRestore");
			HideMenuGroup("Map");
			HideMenuGroup("Gallery");
			HideMenuGroup("DataSheet");
			HideMenuGroup("Hotspot");
			HideMenuGroup("Layout");
			HideMenuGroup("Popup");
			HideMenuItem(MemberPageActionId.AddHotspot);
			HideSeparator(13);
			HideMenuItem(MemberPageActionId.MapSetupNew);
			HideMenuItem(MemberPageActionId.GallerySetupNew);
			HideMenuItem(MemberPageActionId.AddDataSheet);
			HideSeparator(6);
		}
        else if (tour.IsFlexMapTour)
        {
            HideMenuItem(MemberPageActionId.GallerySetupNew);
            HideMenuItem(MemberPageActionId.AddDataSheet);
        }
    }

	public void InitNavigation(MemberPage memberPage)
	{
		this.memberPage = memberPage;
		account = MapsAliveState.Account;

		if (account == null)
		{
			tour = null;
		}
		else
		{
			tour = memberPage.Tour;
			if (tour != null)
			{
				tourPage = tour.SelectedTourPage;
				tourView = tour.SelectedTourView;
			}
		}

		InitMenu();
		InitCurrentTourNavigator();
	}

	private void InitRecentItems()
	{
		RadMenuItem recentGroup = FindMenuGroup("Recent");

		if (tourPage != null)
		{
			string name = tourPage.Name;
			string description;
			MemberPageActionId actionId;
			if (tourPage.IsDataSheet)
			{
				description = "Data Sheet";
				actionId = MemberPageActionId.EditHotspotContent;
			}
			else if (tourPage.IsGallery)
			{
				description = "Gallery";
				actionId = MemberPageActionId.Gallery;
			}
			else
			{
				description = "Map";
				actionId = MemberPageActionId.Map;
			}

			string itemText = RecentItemText(description, name);
			RadMenuItem newItem = new RadMenuItem(itemText);
			recentGroup.Items.Add(newItem);
			SetMenuItemTargetPage(newItem, actionId);
		}

		if (tourView != null && !tourPage.IsDataSheet)
		{
			string name = tourView.Title;
			string itemText = RecentItemText("Hotspot", name);
			RadMenuItem newItem = new RadMenuItem(itemText);
			recentGroup.Items.Add(newItem);
			string queryString = string.Format("vid={0}", tourView.Id);
			SetMenuItemTargetPage(newItem, MemberPageActionId.EditHotspot, queryString);
		}

		InitRecentResources(recentGroup);

		if (recentGroup.Items.Count == 0)
			recentGroup.Enabled = false;
	}

	private void InitRecentResources(RadMenuItem recentGroup)
	{
		int id;
		TourResource resource;

		id = account.LastResourceId(TourResourceType.Category);
		if (id != 0)
		{
			resource = Account.GetCachedCategory(id);
			AddRecentResource(recentGroup, resource, MemberPageActionId.EditCategory, id);
		}

		id = account.LastResourceId(TourResourceType.Marker);
		if (id != 0)
		{
			resource = Account.GetCachedMarker(id);
			AddRecentResource(recentGroup, resource, MemberPageActionId.EditMarker, id);
		}

		id = account.LastResourceId(TourResourceType.Symbol);
		if (id != 0)
		{
			resource = Account.GetCachedSymbol(id);
			AddRecentResource(recentGroup, resource, MemberPageActionId.EditSymbol, id);
		}

		id = account.LastResourceId(TourResourceType.MarkerStyle);
		if (id != 0)
		{
			resource = Account.GetCachedMarkerStyle(id);
			AddRecentResource(recentGroup, resource, MemberPageActionId.EditMarkerStyle, id);
		}

		id = account.LastResourceId(TourResourceType.FontStyle);
		if (id != 0)
		{
			resource = Account.GetCachedFontStyle(id);
			AddRecentResource(recentGroup, resource, MemberPageActionId.EditFontStyle, id);
		}

		id = account.LastResourceId(TourResourceType.TooltipStyle);
		if (id != 0)
		{
			resource = Account.GetCachedTooltipStyle(id);
			AddRecentResource(recentGroup, resource, MemberPageActionId.EditTooltipStyle, id);
		}

		id = account.LastResourceId(TourResourceType.TourStyle);
		if (id != 0)
		{
			resource = Account.GetCachedColorScheme(id);
			AddRecentResource(recentGroup, resource, MemberPageActionId.EditColorScheme, id);
		}
	}

	private void InitResourceFilterMenuItems(out bool showUsageOptions, out TourResourceType tourResourceType)
	{
		bool showCategoryFilterOption = false;
		bool showMarkerFilterOption = false;
		showUsageOptions = true;
		tourResourceType = TourResourceType.Undefined;

		GetFilterAndUsageFlags(ref showCategoryFilterOption, ref showMarkerFilterOption, ref showUsageOptions, ref tourResourceType);

		bool showAllCategories = !account.ResourceIsFilteredBy(ResourceFilters.Category);
		bool showAllMarkers = !account.ResourceIsFilteredBy(ResourceFilters.Marker);
		ResourceFilters resourceFilters;

		if (showCategoryFilterOption)
		{
			SetMenuItemTextSubstitution("_ShowOnlyTourCategories", tour.Name);
			resourceFilters = InitFilterMenuItem(showAllCategories, "_ShowOnlyTourCategories", ResourceFilters.Category);
		}
		else
		{
			HideMenuItem("_ShowOnlyTourCategories");
		}

		if (showMarkerFilterOption)
		{
			SetMenuItemTextSubstitution("_ShowOnlyTourMarkers", tour.Name);
			resourceFilters = InitFilterMenuItem(showAllMarkers, "_ShowOnlyTourMarkers", ResourceFilters.Marker);
		}
		else
		{
			HideMenuItem("_ShowOnlyTourMarkers");
		}

		if (!(showCategoryFilterOption || showMarkerFilterOption))
			HideSeparator(2);
	}

	private void InitResourceGroup()
	{
		if (!enableColorSchemes)
			HideMenuItem(MemberPageActionId.ColorSchemeExplorer);

		TourResource resource = GetResourceForAction();
		bool hideResourceItems = false;

		if (actionId == MemberPageActionId.Resources)
		{
			hideResourceItems = true;
		}
		else
		{
			if (resource == null || resource.Id == 0)
			{
				hideResourceItems = true;
			}
			else
			{
				InitResourceMenuEditGroup(resource);
			}
		}

		if (hideResourceItems)
		{
			HideSeparator(4);
			HideSeparator(5);
			HideMenuItem(MemberPageActionId.DuplicateResource);
			HideMenuItem("_DeleteResource");
		}
		else if (resource.ResourceType == TourResourceType.Category)
		{
			HideMenuItem(MemberPageActionId.DuplicateResource);
			HideSeparator(5);
		}

		bool showUsageOptions;
		TourResourceType tourResourceType;
		InitResourceFilterMenuItems(out showUsageOptions, out tourResourceType);
		InitShowUsageMenuItem(showUsageOptions, tourResourceType);

		if (!account.IsPlusOrProPlan)
		{
			SetMenuItemDenyMessage(MemberPageActionId.CategoryExplorer, Account.RequiresPlanMessage("Categories", AccountPlan.Plus));
			SetMenuItemDenyMessage(MemberPageActionId.CreateCategory, Account.RequiresPlanMessage("Category", AccountPlan.Plus));
		}
	}

	private void InitResourceLibrary()
	{
		// This method causes the Resource group to be selected and the appropriate Explorer
		// menu item to be highlighted when any resource editor or dependency report screen
		// is the current action. We do this to make it easier for the user to find the
		// Explorer menu item for the resource being edited or having its usage shown.

		if (actionId == MemberPageActionId.EditCategory ||
			actionId == MemberPageActionId.EditFontStyle ||
			actionId == MemberPageActionId.EditMarker ||
			actionId == MemberPageActionId.EditMarkerStyle ||
			actionId == MemberPageActionId.EditSymbol ||
			actionId == MemberPageActionId.EditTooltipStyle ||
			actionId == MemberPageActionId.EditColorScheme ||
			actionId == MemberPageActionId.TourResourceDependencyReport)
		{
			ShowMenuGroupAsSelected("Resources");

			if (actionId == MemberPageActionId.EditCategory)
				ShowMenuItemAsActive(MemberPageActionId.CategoryExplorer);
			else if (actionId == MemberPageActionId.EditFontStyle)
				ShowMenuItemAsActive(MemberPageActionId.FontStyleExplorer);
			else if (actionId == MemberPageActionId.EditMarker)
				ShowMenuItemAsActive(MemberPageActionId.MarkerExplorer);
			else if (actionId == MemberPageActionId.EditMarkerStyle)
				ShowMenuItemAsActive(MemberPageActionId.MarkerStyleExplorer);
			else if (actionId == MemberPageActionId.EditSymbol)
				ShowMenuItemAsActive(MemberPageActionId.SymbolExplorer);
			else if (actionId == MemberPageActionId.EditTooltipStyle)
				ShowMenuItemAsActive(MemberPageActionId.TooltipStyleExplorer);
			else if (actionId == MemberPageActionId.EditColorScheme)
				ShowMenuItemAsActive(MemberPageActionId.ColorSchemeExplorer);
			else if (actionId == MemberPageActionId.TourResourceDependencyReport)
			{
				int rt;
				int.TryParse(Request.QueryString["rt"], out rt);
				TourResourceType resourceType = (TourResourceType)rt;
				ShowMenuItemAsActive(string.Format("{0}Explorer", resourceType == TourResourceType.TourStyle ? "ColorScheme" : resourceType.ToString()));
			}
		}
	}

	private void InitResourceMenuEditGroup(TourResource resource)
	{
		string resourceTypeName;

		resourceTypeName = TourResourceManager.GetTitle(resource.ResourceType);

		string name;
		if (resource is Category)
			name = ((Category)resource).Title;
		else
			name = resource.Name;

		// Fixup the generic duplicate resource item. Note that duplication is initiated by PerformAction.asxh.
		MemberPageActionId duplicateActionId = MemberPageActionId._NotSet;
		switch (resource.ResourceType)
		{
			case TourResourceType.FontStyle:
				duplicateActionId = MemberPageActionId.CreateFontStyle;
				break;
			
			case TourResourceType.Marker:
				duplicateActionId = MemberPageActionId.CreateMarker;
				break;
			
			case TourResourceType.MarkerStyle:
				duplicateActionId = MemberPageActionId.CreateMarkerStyle;
				break;
			
			case TourResourceType.Symbol:
				duplicateActionId = MemberPageActionId.CreateSymbol;
				break;
			
			case TourResourceType.TooltipStyle:
				duplicateActionId = MemberPageActionId.CreateTooltipStyle;
				break;
			
			case TourResourceType.TourStyle:
				duplicateActionId = MemberPageActionId.CreateColorScheme;
				break;
		}

		SetMenuItemTextSubstitution(MemberPageActionId.DuplicateResource, name);
		SetMenuItemQueryString(MemberPageActionId.DuplicateResource, string.Format("aid={0}&rt={1}&id={2}", (int)duplicateActionId, (int)resource.ResourceType, resource.Id));

		// Fixup the generic delete resource item. Note that delete events don't need a target page.
		SetMenuItemTextSubstitution("_DeleteResource", name);
		RadMenuItem item = FindMenuItem("_DeleteResource");
		ShowMenuItemAsDelete(item);
		string action = string.Format("Delete{0}", resource.ResourceType.ToString());
		item.Value = action;
		item.Attributes["TargetId"] = resource.Id.ToString();
		item.Attributes["TargetType"] = resourceTypeName;
		item.Attributes["TargetName"] = name;
	}

	private void InitShowUsageMenuItem(bool showUsageOptions, TourResourceType tourResourceType)
	{
		if (showUsageOptions)
		{
			string resourceTitle = TourResourceManager.GetTitlePlural(tourResourceType);
			RadMenuItem item = FindMenuItem("_DeleteUnused");
			SetMenuItemTextSubstitution("_DeleteUnused", resourceTitle);
			SetMenuItemTargetPage(item, actionId, "delete=1");
			SetMenuItemConfirmMessage("_DeleteUnused", string.Format("[Delete unused [@{0}@] from this account?][*This action cannot be undone.*]", resourceTitle));
		}
		else
		{
			HideSeparator(3);
			HideMenuItem("_DeleteUnused");
		}
	}

	private void InitTourBuilderItems()
	{
		InitResourceGroup();

		if (tour == null)
		{
			if (account.TourCount == 0)
				DisableMenuItem(MemberPageActionId.TourExplorer);
			return;
		}

		string tourName = tour.Name;

		SetMenuItemDeleteTarget(MemberPageActionId.DeleteTour, tour.Id, tourName);
		SetMenuItemTextSubstitution(MemberPageActionId.DeleteTour, tourName);
		SetMenuItemTextSubstitution(MemberPageActionId.AddDataSheet, tourName);
		SetMenuItemTextSubstitution(MemberPageActionId.MapSetupNew, tourName);
		SetMenuItemTextSubstitution(MemberPageActionId.GallerySetupNew, tourName);

		if (!account.IsPlusOrProPlan)
		{
			if (tour.V3CompatibilityEnabled)
                SetMenuItemDenyMessage(MemberPageActionId.DirectoryOptions, Account.RequiresPlanMessage("Directory Options", AccountPlan.Plus));
			SetMenuItemDenyMessage(MemberPageActionId.BannerOptions, Account.RequiresPlanMessage("Banner Image", AccountPlan.Plus));
		}

        // Allow V3 plus and personal plan users to duplicate their tours so they can test with V4.
		if (!account.IsProPlan && !tour.V3CompatibilityEnabled)
		{
			SetMenuItemDenyMessage(MemberPageActionId.DuplicateTour, Account.RequiresPlanMessage("Duplicate Tour", AccountPlan.Pro));
		}

        if (MapsAliveState.Account.MembershipExpired)
            DisableMenuItem(MemberPageActionId.Export);

        if (tourPage == null)
		{
			DisableMenuGroup("Page");
			DisableMenuGroup("Popup");
			DisableMenuGroup("Map");
			DisableMenuGroup("Gallery");
			
			DisableMenuItem(MemberPageActionId.MapMargins);
			DisableMenuItem(MemberPageActionId.LayoutAreaMarginsAndSpacing);
			DisableMenuItem(MemberPageActionId.TemplateSplittersForLayoutArea);
			DisableMenuItem(MemberPageActionId.TemplateChoicesForLayoutArea);
			
			HideMenuItem(MemberPageActionId.AddHotspot);

			DisableMenuItem(MemberPageActionId.ImportHotspotContent);
			DisableMenuItem(MemberPageActionId.ImportHotspotPhotos);
			DisableMenuItem(MemberPageActionId.ImportMarkerShapes);
			DisableMenuItem(MemberPageActionId.ImportRoutes);

			DisableMenuItem(MemberPageActionId.MapMargins);
			DisableMenuItem(MemberPageActionId.LayoutAreaMarginsAndSpacing);
			DisableMenuItem(MemberPageActionId.TemplateSplittersForLayoutArea);
			DisableMenuItem(MemberPageActionId.TemplateChoicesForLayoutArea);
		}
		else
		{
			string tourPageName = tourPage.Name;

			if (tour.IsFlexMapTour)
            {
                DisableMenuItem(MemberPageActionId.MapMargins);
                DisableMenuItem(MemberPageActionId.BannerOptions);
                HideMenuGroup("Gallery");
                HideMenuGroup("DataSheet");
                SetMenuItemDeleteTarget(MemberPageActionId.DeleteMap, tourPage.Id, tourPageName);
                SetMenuItemTextSubstitution(MemberPageActionId.DeleteMap, tourPageName);
                SetMenuItemTextSubstitution(MemberPageActionId.AddHotspot, tourPageName);
            }
            else if (tourPage.IsDataSheet)
			{
				HideMenuGroup("Map"); ;
				HideMenuGroup("Gallery");
				HideMenuGroup("Hotspot");
				HideMenuGroup("Popup");
				HideMenuItem(MemberPageActionId.AddHotspot);

				DisableMenuItem(MemberPageActionId.ImportHotspotPhotos);
				DisableMenuItem(MemberPageActionId.ImportMarkerShapes);
				DisableMenuItem(MemberPageActionId.ImportRoutes);
				
				SetMenuItemDeleteTarget(MemberPageActionId.DeleteDataSheet, tourPage.Id, tourPageName);
				SetMenuItemTextSubstitution(MemberPageActionId.DeleteDataSheet, tourPageName);
			}
			else
			{
				if (tourPage.IsGallery)
				{
					HideMenuGroup("Map");
					SetMenuItemDeleteTarget(MemberPageActionId.DeleteGallery, tourPage.Id, tourPageName);
					SetMenuItemTextSubstitution(MemberPageActionId.DeleteGallery, tourPageName);

					DisableMenuItem(MemberPageActionId.ImportMarkerShapes);
					DisableMenuItem(MemberPageActionId.ImportRoutes);
				}
				else
				{
					HideMenuGroup("Gallery");
					SetMenuItemDeleteTarget(MemberPageActionId.DeleteMap, tourPage.Id, tourPageName);
					SetMenuItemTextSubstitution(MemberPageActionId.DeleteMap, tourPageName);
				}

				HideMenuGroup("DataSheet");
				
				SetMenuItemTextSubstitution(MemberPageActionId.AddHotspot, tourPageName);

				if (!account.HasLastReport)
				{
					HideSeparator(12);
					HideMenuItem(MemberPageActionId.LastReport);
				}
			}

			if (!account.IsPlusOrProPlan)
			{
				SetMenuItemDenyMessage(MemberPageActionId.ImportHotspotContent, Account.RequiresPlanMessage("Import Content", AccountPlan.Plus));
				SetMenuItemDenyMessage(MemberPageActionId.ImportMarkerShapes, Account.RequiresPlanMessage("Import Marker Shapes", AccountPlan.Plus));
				SetMenuItemDenyMessage(MemberPageActionId.ReplaceMarkers, Account.RequiresPlanMessage("Replace Markers", AccountPlan.Plus));
				SetMenuItemDenyMessage(MemberPageActionId.ReplaceMarkerStyles, Account.RequiresPlanMessage("Replace Marker Styles", AccountPlan.Plus));
			}
			if (!account.IsProPlan)
			{
				SetMenuItemDenyMessage(MemberPageActionId.ImportRoutes, Account.RequiresPlanMessage("Import Routes", AccountPlan.Pro));
			}

			if (tourPage.SlidesPopup)
			{
				HideMenuItem(MemberPageActionId.TemplateChoicesForLayoutArea);
				HideMenuItem(MemberPageActionId.TemplateSplittersForLayoutArea);
				HideMenuItem(MemberPageActionId.LayoutAreaMarginsAndSpacing);
			}
			else
			{
				DisableMenuGroup("Popup");
				HideMenuItem(MemberPageActionId.MapMargins);
			}
		}

        if (tourView == null)
		{
			DisableMenuGroup("Hotspot");
			HideMenuGroup("DataSheet");
		}
		else
		{
			SetMenuItemDeleteTarget(MemberPageActionId.DeleteHotspot, tourView.Id, tourView.Title);
			SetMenuItemTextSubstitution(MemberPageActionId.DeleteHotspot, tourView.Title);
			SetMenuItemTextSubstitution(MemberPageActionId.DuplicateHotspot, tourView.Title);
		}
	}

	public Panel MasterAllToursPanel
	{
		get { return masterAllToursPanel; }
		set { masterAllToursPanel = value; }
	}

	public Panel MasterCurrentTourPanel
	{
		get { return masterCurrentTourPanel; }
		set { masterCurrentTourPanel = value; }
	}

	public MemberPageHeader MasterPageHeader
	{
		get { return masterPageHeader; }
		set { masterPageHeader = value; }
	}

	public HiddenField MasterPostId
	{
		get { return masterPostId; }
		set { masterPostId = value; }
	}

	public Label MasterSpecialMessage
	{
		get { return masterSpecialMessage; }
		set { masterSpecialMessage = value; }
	}

	public Panel MasterSpecialMessagePanel
	{
		get { return masterSpecialMessagePanel; }
		set { masterSpecialMessagePanel = value; }
	}

	public ScriptManager MasterScriptManager
	{
		get { return masterScriptManager; }
		set {  masterScriptManager = value; }
	}

	public StatusBox MasterStatusBox
	{
		get { return masterStatusBox; }
		set { masterStatusBox = value; }
	}

	public RadMenu MasterTopMenu
	{
		get { return masterTopMenu; }
		set { masterTopMenu = value; }
	}

	public TourList MasterTourList
	{
		get { return masterTourList; }
		set { masterTourList = value; }
	}

	public GroupNavigator MasterTourNavigator
	{
		get { return masterTourNavigator; }
		set { masterTourNavigator = value; }
	}

	private string MenuXml()
	{
		string fileName = "ProjectMenu.xml";
		MapsAliveObjectType menuXmlType = MapsAliveObjectType.MenuXmlProject;

		string text = (string)MapsAliveState.Retrieve(menuXmlType);
		if (text == null)
		{
			string fileLocation = FileManager.WebAppFileLocationAbsolute("App_Data", fileName);
			text = FileManager.ReadFileContents(fileLocation);
			MapsAliveState.Persist(menuXmlType, text);
		}
		return text;
	}

	private string RecentItemText(string item, string name)
	{
		return string.Format("{0}: <span class='radMenuTextSubstitution'>{1}</span>", item, name);
	}

	private void SetMenuItemConfirmMessage(string itemValue, string msg)
	{
		RadMenuItem item = FindMenuItem(itemValue);
		if (item != null)
		{
			item.Attributes["ConfirmMsg"] = msg;
		}
	}

	private void SetMenuItemDeleteTarget(MemberPageActionId actionId, int id, string name)
	{
		RadMenuItem item = FindMenuItem(actionId);
		if (item != null)
		{
			item.Attributes["TargetId"] = id.ToString();
			item.Attributes["TargetName"] = name;
			ShowMenuItemAsDelete(item);
		}
	}

	private void SetMenuItemDenyMessage(MemberPageActionId actionId, string msg)
	{
		RadMenuItem item = FindMenuItem(actionId);
		if (item != null)
		{
			item.Attributes["AlertMsg"] = msg;
		}
	}

	private void SetMenuItemQueryString(MemberPageActionId actionId, string queryString)
	{
		RadMenuItem item = FindMenuItem(actionId);
		if (item != null)
		{
			item.Attributes["TargetPage"] = string.Format("{0}&{1}", item.Attributes["TargetPage"], queryString);
		}
	}

	private void SetMenuItemTargetPage(RadMenuItem item)
	{
		SetMenuItemTargetPage(item, item.Value);
	}
	
	private void SetMenuItemTargetPage(RadMenuItem item, string action)
	{
		if (action != string.Empty && !item.IsSeparator)
		{
			MemberPageActionId actionId = (MemberPageActionId)Enum.Parse(typeof(MemberPageActionId), action);
			SetMenuItemTargetPage(item, actionId);
		}
	}

	private void SetMenuItemTargetPage(RadMenuItem item, MemberPageActionId actionId)
	{
		SetMenuItemTargetPage(item, actionId, string.Empty);
	}

	private void SetMenuItemTargetPage(string itemValue, MemberPageActionId actionId, string queryString)
	{
		RadMenuItem item = FindMenuItem(itemValue);
		if (item != null)
		{
			SetMenuItemTargetPage(item, actionId, queryString);
		}
	}

	private void SetMenuItemTargetPage(RadMenuItem item, MemberPageActionId actionId, string queryString)
	{
		string targetPage = MemberPageAction.ActionPageTarget(actionId);
        string separator;
		if (queryString.Length > 0)
		{
			separator = targetPage.Contains("?") ? "&" : "?";
			targetPage += separator + queryString;
		}

        separator = targetPage.Contains("?") ? "&" : "?";
        targetPage += string.Format("{0}rev={1}", separator, App.Revision);


        item.Attributes["TargetPage"] = targetPage;
	}

	private void SetMenuItemText(MemberPageActionId actionId, string text)
	{
		RadMenuItem item = FindMenuItem(actionId.ToString());
		if (item != null)
			item.Text = text;
	}

	private void SetMenuItemTextSubstitution(MemberPageActionId actionId, string substitution)
	{
		SetMenuItemTextSubstitution(actionId.ToString(), substitution);
	}

	private void SetMenuItemTextSubstitution(string itemValue, string substitution)
	{
		RadMenuItem item = FindMenuItem(itemValue);
		SetMenuItemTextSubstitution(item, substitution);
	}

	private void SetMenuItemTextSubstitution(RadMenuItem item, string substitution)
	{
		item.Text = string.Format(item.Text, "<span class='radMenuTextSubstitution'>" + substitution + "</span>");
	}

	private void SetMenuTitle(RadMenuItem item)
	{
		if (tour != null)
		{
			if (item.Value == "Title_Tour")
			{
				SetMenuTitleProperties(item, tour.Name);
			}
			else
			{
				if (item.Value == "Title_Map")
				{
					SetMenuTitleProperties(item, tourPage == null ? "This tour has no maps or data sheets" : tourPage.Name);
				}
				else if (item.Value == "Title_Hotspot")
				{
					if (tourView != null)
					{
						SetMenuTitleProperties(item, tourView.Title);
					}
				}
			}
		}
	}

	private static void SetMenuTitleProperties(RadMenuItem item, string title)
	{
		item.Text = title;
		item.CssClass = "radMenuTitle";
		item.Enabled = false;
	}

	private static void ShowMenuGroupAsSelected(RadMenuItem item)
	{
		item.CssClass = "radMenuGroupSelected";
	}

	private void ShowMenuGroupAsSelected(string itemValue)
	{
		RadMenuItem group = FindMenuGroup(itemValue);
		if (group != null)
			ShowMenuGroupAsSelected(group);
	}

	private void ShowMenuGroup(string itemValue)
	{
		RadMenuItem group = FindMenuGroup(itemValue);
		if (group != null)
			group.Visible = true;
	}

	private void ShowMenuItem(MemberPageActionId actionId)
	{
		RadMenuItem item = FindMenuItem(actionId);
		if (item != null)
			item.Visible = true;
	}

	private void ShowMenuItemAsActive(MemberPageActionId actionId)
	{
		ShowMenuItemAsActive(actionId.ToString());
	}

	private void ShowMenuItemAsActive(string itemValue)
	{
		RadMenuItem item = FindMenuItem(itemValue);
		if (item != null)
			item.ImageUrl = "../Images/MenuResourceExplorer.png";
	}

	private void ShowMenuItemAsChecked(bool showChecked, string itemValue)
	{
		RadMenuItem item = FindMenuItem(itemValue);
		if (item != null)
			item.ImageUrl = showChecked ? "../Images/MenuCheck.png" : string.Empty;
	}

	private void ShowMenuItemAsDelete(RadMenuItem item)
	{
		item.ImageUrl = "../Images/DeleteX.png";
	}

	private static void ShowMenuItemAsSelected(RadMenuItem item)
	{
		item.CssClass = "radMenuItemSelected";
		item.Enabled = false;
	}
}
