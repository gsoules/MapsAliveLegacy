// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Security;

public enum AccountPlan
{
	Starter = 4,
	Personal = 1,
	Plus = 2,
	Pro = 3,
}

public enum AccountType
{
	Elite = 5,
	Trial = 6,
	Paid = 7
}

public enum HotspotLimitWarningContext
{
	AddNewHotspotAccountAtLimit,
	AddNewHotspotAccountOverLimit,
	AddNewHotspotTourOverLimit,
	ExportTourOverLimit,
	ImportHotspotsAccountAtLimit,
	ImportHotspotsAccountOverLimit,
	ImportHotspotsTourOverLimit,
	ImportTourAccountOverLimit,
	ImportTourAccountAtLimit,
	PublishTourOverLimit,
	TourOverLimit
}

public enum HotspotLimitStatus
{
	Unknown,
	UnderLimit,
	AtLimit,
	OverLimit
}

[Flags]
public enum ResourceFilters
{
	Marker = 0x0001,
	Category = 0x0002
}

public class Account
{
	private int accountId;
	private int announcementId;
	private string contactName;
	private decimal creditAmount;
	private int _days;
	private bool diskCacheCreated;
	private int discountPercent;
	private string email;
	private HotspotLimitStatus _hotspotLimitStatus;
	private MemberPageActionId lastActionIdForImportSlides;
	private int lastMarkerIdSelected;
	private Account parentAccount;
	private int parentAccountId;
	private bool markersZoom;
	private decimal paymentAmount;
	private ResourceFilters resourceFilters;
	private bool sendNewsletter;
	private string siteName;
	private bool showSlideContentInLayoutPreview;
	private bool showTourNavigatorExpanded;
	private int _slideLimit;
	private int _tourCount;
	private AccountType type;
	private Guid userId;
	private string userName;

	private const string defaultSiteName = "MapsAlive Tour Builder";

	private Dictionary<MemberPageActionId, MemberPageActionId> lastActionIdDictionary;
	private Dictionary<TourResourceType, int> lastResourceIdDictionary;
	
	// These two properties are here to remember coordinate values while a user is editing a marker in the
	// marker editor. They make it possible for the user to experiment with different marker and shape types.
	// If we didn't have these, a user would blow away their polygon/line/hybrid coords if the changed the
	// marker or shape type and then put it back to polygon/line/hybrid. These properties are here in Account
	// instead of in the EditHotspotActions class, because the marker object gets flushed from the cache whenever the
	// marker or shape type changes in order to flush the preview image.
	public string TempMarkerPolygonCoords { get; set; }
	public string TempMarkerHybridCoords { get; set; }
	
	public ShapeType LastShapeTypeSelected { get; set; }
	public ShapeType LastTextShapeTypeSelected { get; set; }

	public int DefaultFontStyleId { get; set; }
	public int DefaultMarkerId { get; set; }
	public int DefaultMarkerStyleId { get; set; }
	public int DefaultSymbolId { get; set; }
	public int DefaultTooltipStyleId { get; set; }
	public int DefaultColorSchemeId { get; set; }
	
	public bool DisableTourAdvisor { get; set; }
	public bool HasLastReport { get; set; }
	public AccountPlan Plan { get; set; }

	public Account(Guid userId)
	{
		// Call this constructor to create the working account for a new user or a logged-in user.
		MapsAliveDataRow row = MapsAliveDatabase.LoadDataRow("sp_Account_GetAccountByUserId", "@UserId", userId);
		if (row == null)
		{
			Debug.Fail("No account row returned for user");
			return;
		}
		ConstructAccount(row);
	}

	public Account(int accountId)
	{
		MapsAliveDataRow row = MapsAliveDatabase.LoadDataRow("sp_Account_GetAccountByAccountId", "@AccountId", accountId);
		ConstructAccount(row);
	}

	public Account(MapsAliveDataRow row)
	{
	    ConstructAccount(row);
	}

	private void ConstructAccount(MapsAliveDataRow row)
	{
		accountId = row.IntValue("AccountId");
		userId = row.GuidValue("UserId");
		contactName = row.StringValue("ContactName");
		siteName = row.StringValue("SiteName");
		sendNewsletter = row.BoolValue("SendNewsletter");
		type = (AccountType)row.IntValue("Type");
		Plan = (AccountPlan)row.IntValue("PlanId");
		parentAccountId = row.IntValue("ParentAccountId");
		announcementId = row.IntValue("AnnouncementId");
		creditAmount = row.MoneyValue("CreditAmount");
		paymentAmount = row.MoneyValue("PaymentAmount");
		discountPercent = row.IntValue("DiscountPercent");
		_days = row.IntValue("Days");
		_slideLimit = row.IntValue("SlideLimit");
		showSlideContentInLayoutPreview = row.BoolValue("ShowSlideContentInLayoutPreview");
		showTourNavigatorExpanded = row.BoolValue("ShowTourNavigatorExpanded");
		markersZoom = row.BoolValue("MarkersZoom");

		resourceFilters = (ResourceFilters)row.IntValue("ResourceFilters");

		if (parentAccountId != 0)
			parentAccount = new Account(parentAccountId);

		lastActionIdDictionary = new Dictionary<MemberPageActionId, MemberPageActionId>();
		lastResourceIdDictionary = new Dictionary<TourResourceType, int>();

		SetLastActionIdForGroup(MemberPageActionId.LayoutProperties, MemberPageActionId.TemplateSplittersForLayoutArea);
		SetLastActionIdForGroup(MemberPageActionId.MapProperties, MemberPageActionId.Map);
		SetLastActionIdForGroup(MemberPageActionId.Resources, MemberPageActionId.Resources);
		SetLastActionIdForGroup(MemberPageActionId.HotspotProperties, MemberPageActionId.EditHotspotContent);
		SetLastActionIdForGroup(MemberPageActionId.TourProperties, MemberPageActionId.TourManager);
		
		lastActionIdForImportSlides = ActionIdIdForImportSlidesDbId(row.IntValue("LastImportSlidesType"));
		lastMarkerIdSelected = row.IntValue("LastMarkerIdSelected");
		
		LastShapeTypeSelected = ShapeType.None;
		LastTextShapeTypeSelected = ShapeType.None;

		DefaultFontStyleId = row.IntValue("DefaultFontStyleId");
		DefaultMarkerId = row.IntValue("DefaultMarkerId");
		DefaultMarkerStyleId = row.IntValue("DefaultMarkerStyleId");
		DefaultSymbolId = row.IntValue("DefaultSymbolId");
		DefaultTooltipStyleId = row.IntValue("DefaultTooltipStyleId");
		DefaultColorSchemeId = row.IntValue("DefaultTourStyleId");

		DisableTourAdvisor = row.BoolValue("DisableTourAdvisor");
		HasLastReport = row.StringValue("LastReport") != string.Empty;

		// Set the in-memory tour count to -1 to indicate that it has not been queried yet.
		_tourCount = -1;

		// Force the hotspot limit status to be calculated so that we know the correct initial state.
		_hotspotLimitStatus = HotspotLimitStatus.Unknown;
		_hotspotLimitStatus = HotspotLimitStatus;
	}

	#region ===== Properties ========================================================

	public int AnnouncementId
	{
		get { return announcementId; }
	}

	public string ContactName
	{
		get { return contactName; }
	}

	public decimal CreditAmount
	{
		get { return creditAmount; }
	}

	public string CreditAmountString
	{
		get { return string.Format("{0:c2}", creditAmount); }
	}

	public string CreditExpiryDateString
	{
		get
		{
			const int daysCreditValidAfterAccountExpires = 30;
			int daysRemaining = DaysActual - 1;
			int daysUntilCreditExpires = daysRemaining + daysCreditValidAfterAccountExpires;
			return string.Format("{0:n0}", DateTime.Now.AddDays(daysUntilCreditExpires).ToShortDateString());
		}
	}

	public int DaysActual
	{
		// If days are < 0, this property will return a negative number.
		get	{ return _days;	}
	}

	public int DaysRemaining
	{
		// If days are < 0, this property will return 0.
		get { return Math.Max(0, DaysActual); }
	}

	public int DiscountPercent
	{
		get { return discountPercent; }
	}

	public string Email
	{
		get
		{
			if (email == null)
				email = (string)MapsAliveDatabase.LoadScalar("sp_Account_GetEmail", "@AccountId", accountId);
			return email;
		}
	}

	public string ExpiryDateString
	{
		get
		{
			int daysRemaining = DaysActual - 1;
			string daysDescription = daysRemaining >= 0 ? "expires" : "expired";
			return string.Format("{0} {1:n0}", daysDescription, DateTime.Now.AddDays(daysRemaining).ToShortDateString());
		}
	}

	public int HotspotLimit
	{
		get
		{
			return _slideLimit;
		}
		set
		{
			if (_slideLimit == value)
				return;

			// See if this account currently exceeds its slide limit.
			bool wasOverLimit = HotspotLimitStatus == HotspotLimitStatus.OverLimit;

			// Update the limit and set the status to unknown so that it will be updated the next time it is requested.
			_slideLimit = value;
			_hotspotLimitStatus = HotspotLimitStatus.Unknown;

			EvaluateHotspotLimitStatus(null, wasOverLimit);
		}
	}

	public bool HotspotLimitReached
	{
		get
		{
			if (HotspotLimit == 0)
				return false;
			else
				return CountHotspotsInUse() >= HotspotLimit;
		}
	}

	public int Id
	{
		get { return accountId; }
	}

	public bool IsElite
	{
		get { return type == AccountType.Elite; }
	}

	public bool IsPersonalPlan
	{
		get { return Plan == AccountPlan.Starter || Plan == AccountPlan.Personal; }
	}

	public bool IsTrial
	{
		get { return type == AccountType.Trial; }
	}

	public bool IsPaid
	{
		get { return type == AccountType.Paid; }
	}

	public bool IsPlusPlan
	{
		get { return Plan == AccountPlan.Plus; }
	}

	public bool IsPlusOrProPlan
	{
		get { return IsPlusPlan || IsProPlan; }
	}

	public bool IsProPlan
	{
		get { return Plan == AccountPlan.Pro; }
	}

	public int LastMarkerIdSelected
	{
		get
		{
			if (lastMarkerIdSelected == 0)
				lastMarkerIdSelected = DefaultMarkerId;
			return lastMarkerIdSelected;
		}
		set
		{
			if (lastMarkerIdSelected != value)
			{
				lastMarkerIdSelected = value;
				MapsAliveDatabase.ExecuteStoredProcedure("sp_Account_UpdateLastMarkerIdSelected",
					"@AccountId", Utility.AccountId,
					"@MarkerId", lastMarkerIdSelected);
			}
		}
	}

	// V4 uses the markersZoom flag for the show step-by-step help preference.
    public bool ShowStepByStepHelp
	{
		get { return markersZoom; }
		set { markersZoom = value; }
	}

	public string MembershipDescription
	{
		get
		{
			string typeDescription;

			switch (type)
			{
				case AccountType.Trial:
					typeDescription = "Trial ";
					break;

				case AccountType.Paid:
					typeDescription = "";
					break;

				case AccountType.Elite:
					typeDescription = "Elite ";
					break;

				default:
					typeDescription = type.ToString();
					break;
			}

			string planDescription = PlanDescription(Plan);

			return string.Format("{0}{1}", typeDescription, planDescription);
		}
	}

	public decimal PaymentAmount
	{
		get { return paymentAmount; }
	}

	public string PaymentAmountString
	{
		get { return string.Format("{0:c2}", paymentAmount); }
	}

	public static string PlanDescription(AccountPlan plan)
	{
		switch (plan)
		{
			case AccountPlan.Starter:
			case AccountPlan.Personal:
				return "Basic Plan";

			case AccountPlan.Plus:
				return "Plus Plan";

			case AccountPlan.Pro:
				return "Pro Plan";

			default:
				return plan.ToString();
		}
	}

	public bool MembershipExpired
	{
		get
		{
			if (IsElite)
				return false;

			return DaysRemaining == 0;
		}
	}

	public string NagMessage
	{
		get
		{
			if (IsElite)
				return string.Empty;

			string linkText = string.Empty;
			string messageText = string.Empty;
			OrderKind orderKind = OrderKind.NotSet;

			int nagDays = 30;

			if (DaysRemaining <= nagDays)
			{
				if (type == AccountType.Trial)
				{
					linkText = "Buy a MapsAlive Plan";
					if (DaysRemaining == 0)
					{
						messageText = AppContent.Topic("NagFreeExpired");
					}
					else
					{
						messageText = string.Format(AppContent.Topic("NagFreeExpiring"), DaysRemainingMessage);
					}
					orderKind = OrderKind.BuyPlan;
				}
				else if (type == AccountType.Paid)
				{
					linkText = "Renew your plan";
					if (DaysRemaining == 0)
					{
						messageText = AppContent.Topic("NagPaidExpired");
					}
					else
					{
						messageText = string.Format(AppContent.Topic("NagPaidExpiring"), DaysRemainingMessage);
					}
					orderKind = OrderKind.RenewPlan;
				}
			}

			if (messageText != string.Empty)
			{
				string linkTag = string.Format("<a href='../Buy/ShoppingCart.aspx?order={0}'>{1}</a>", (int)orderKind, linkText);
				messageText = string.Format("{0}<br/>{1}", messageText, linkTag);
			}

			return messageText;
		}
	}

	public static int OrderCount(int accountId)
	{
		return MapsAliveDatabase.GetCount("sp_Order_GetCountByAccountId", "@AccountId", accountId);
	}

	public Account ParentAccount
	{
		get { return parentAccount; }
	}

	public int ParentAccountId
	{
		get { return parentAccountId; }
	}

	public static string RequiresPlanMessage(string feature, AccountPlan plan)
	{
		string message = "<p>The {0} feature requires the {1} plan.</p><p>To enable this feature, you can upgrade to the {1} plan by choosing <b>Account > Upgrade</b> from the menu.</p>";
		string whichPlan = plan == AccountPlan.Plus ? "Plus or Pro" : "Pro";
		return string.Format(message, feature, whichPlan);
	}
	
	public bool ResourceIsFilteredBy(ResourceFilters flags)
	{
		return (resourceFilters & flags) != 0;
	}

	public ResourceFilters ResourceFilters
	{
		get { return resourceFilters; }
		set
		{
			if (resourceFilters != value)
			{
				resourceFilters = value;
				MapsAliveDatabase.ExecuteStoredProcedure("sp_Account_UpdateResourceFilters",
					"@AccountId", accountId,
					"@ResourceFilters", (int)resourceFilters);
			}
		}
	}

	public bool SendNewsletter
	{
		get { return sendNewsletter; }
	}

	public void ShowAnnouncements(bool show)
	{
		this.announcementId = show ? 0 : App.AnnouncementId;

		MapsAliveDatabase.ExecuteStoredProcedure("sp_Account_UpdateAnnouncementId",
			"@UserId", Utility.UserId,
			"@Id", announcementId);
	}

	public bool ShowSlideContentInLayoutPreview
	{
		get { return showSlideContentInLayoutPreview; }
		set { showSlideContentInLayoutPreview = value; }
	}

	public bool ShowTourNavigatorExpanded
	{
		get { return showTourNavigatorExpanded; }
		set { showTourNavigatorExpanded = value; }
	}

	public string SiteName
	{
		get
		{
			if (siteName == string.Empty)
				return defaultSiteName;
			else
				return siteName;
		}
	}

	public int TourCount
	{
		get
		{
			// Only query the database for the tour count if we have not already done so. This is an
			// optimization that is necessary because the tour count is not maintained in the database
			// and we don't want to calculate it every time we need to know which is every member page load.
			if (_tourCount == -1)
				_tourCount = MapsAliveDatabase.GetCount("sp_Tour_GetTourCountByAccountId", "@AccountId", accountId);
			return _tourCount;
		}
	}

	public void TourCountChanged()
	{
		// See comment in the TourCount property.
		_tourCount = -1;
	}
	
	public AccountType Type
	{
		get	{ return type; }
	}

	public Guid UserId
	{
		get { return userId; }
	}

	public string UserName
	{
		get
		{
			if (userName == null)
				userName = (string)MapsAliveDatabase.LoadScalar("sp_Account_GetUserName", "@AccountId", accountId);
			return userName;
		}
	}

	#endregion
	
	private MemberPageActionId ActionIdIdForImportSlidesDbId(int id)
	{
		if (id == 1)
			return MemberPageActionId.ImportHotspotContent;
		else if (id == 2)
			return MemberPageActionId.ImportMarkerShapes;
		else if (id == 3 || id == 0)
			return MemberPageActionId.ImportHotspotPhotos;
		else if (id == 4)
			return MemberPageActionId.ImportRoutes;
		else
		{
			Debug.Fail("Import action has not been added to ActionIdIdForImportSlidesDbId");
			return MemberPageActionId.ImportHotspotPhotos;
		}
	}

	private int CalculateHotspotsAvailable()
	{
		int hotspotsAvailable;

		if (HotspotLimit == 0 && Type == AccountType.Elite)
		{
			hotspotsAvailable = int.MaxValue;
		}
		else
		{
			hotspotsAvailable = HotspotLimit - CountHotspotsInUse();
		}

		return hotspotsAvailable;
	}

	public static bool CategoryCodeInUse(int categoryId, string code)
	{
		return MapsAliveDatabase.GetCount("sp_Category_GetCategoryExistsByCategoryCode", "@AccountId", Utility.AccountId, "@CategoryId", categoryId, "@Code", code) != 0;
	}

	public void ClearResourceFilters()
	{
		resourceFilters = 0;
	}

	public int CountHotspotsBorrowed()
	{
		int borrowed = CountHotspotsInUse() - HotspotLimit;
		if (borrowed < 0)
			borrowed = 0;
		return borrowed;
	}

	public int CountHotspotsInUse()
	{
		// Return the number of slides in this account and it all of its sub accounts.
		object value = MapsAliveDatabase.LoadScalar("sp_Account_CountSlides", "@AccountId", accountId);
		if (value is DBNull)
			return 0;
		else
			return (int)value;
	}

	public static int CountSlidesInTour(Tour tour)
	{
		int count = 0;
		foreach (TourPage tourPage in tour.TourPages)
		{
			foreach (TourView tourView in tourPage.TourViews)
			{
				count++;
			}
		}
		return count;
	}

	public int CountToursInAccount()
	{
		// Return the number of tours in this account and it all of its sub accounts.
		object value = MapsAliveDatabase.LoadScalar("sp_Account_CountTours", "@AccountId", accountId);
		if (value is DBNull)
			return 0;
		else
			return (int)value;
	}

	private static TourResource CreateNewTourResource(TourResourceType resourceType, int resourceId)
	{
		TourResource resource = null;

		switch (resourceType)
		{
			case TourResourceType.Category:
				resource = new Category(resourceId);
				break;

			case TourResourceType.Marker:
				resource = new Marker(resourceId);
				break;

			case TourResourceType.MarkerStyle:
				resource = new MarkerStyle(resourceId);
				break;

			case TourResourceType.TooltipStyle:
				resource = new TooltipStyle(resourceId);
				break;

			case TourResourceType.FontStyle:
				resource = new FontStyleResource(resourceId);
				break;

			case TourResourceType.TourStyle:
				resource = new ColorScheme(resourceId);
				break;

			case TourResourceType.Symbol:
				resource = new Symbol(resourceId);
				break;

			default:
				Debug.Fail("Unsupported ResourceType " + resourceType);
				break;
		}

		if (resource != null && resource.Id == 0)
		{
			// This can happen if a user deletes a resource and then uses the Back button to
			// return to a screen that was referencing that resource via a query string Id.
			resource = null;
		}
		
		return resource;
	}

	public void CreateResourceImageFileDiskCache()
	{
		if (diskCacheCreated)
			return;

		diskCacheCreated = true;

		TourResource.CreateResourceImageFileDiskCache(TourResourceType.MarkerStyle, this);
		TourResource.CreateResourceImageFileDiskCache(TourResourceType.TourStyle, this);
		TourResource.CreateResourceImageFileDiskCache(TourResourceType.TooltipStyle, this);
		TourResource.CreateResourceImageFileDiskCache(TourResourceType.FontStyle, this);
		TourResource.CreateResourceImageFileDiskCache(TourResourceType.Marker, this);
		TourResource.CreateResourceImageFileDiskCache(TourResourceType.Symbol, this);
	}

	private string DaysRemainingMessage
	{
		get
		{
			int d = DaysRemaining;
			return string.Format("{0} day{1}", d, Utility.Plural(d));
		}
	}

	private int DbIdForImportSlidesActionId(MemberPageActionId id)
	{
		// This is a temporary solution so that we can remember the last type of slide import
		// the user performed. We have to map action Ids to numbers that won't ever change in
		// the database. The MemberPageActionId enums do not have hard-coded values and can change.

		if (id == MemberPageActionId.ImportHotspotContent)
			return 1;
		else if (id == MemberPageActionId.ImportMarkerShapes)
			return 2;
		else if (id == MemberPageActionId.ImportHotspotPhotos)
			return 3;
		else if (id == MemberPageActionId.ImportRoutes)
			return 4;
		else
		{
			Debug.Fail("Import action has not been added to DbIdForImportSlidesActionId");
			return 3;
		}
	}

	public int DefaultResourceId(TourResourceType resourceType)
	{
		switch (resourceType)
		{
			case TourResourceType.FontStyle:
				return DefaultFontStyleId;
			
			case TourResourceType.Marker:
				return DefaultMarkerId;
			
			case TourResourceType.MarkerStyle:
				return DefaultMarkerStyleId;
			
			case TourResourceType.Symbol:
				return DefaultSymbolId;
			
			case TourResourceType.TooltipStyle:
				return DefaultTooltipStyleId;
						
			case TourResourceType.TourStyle:
				return DefaultColorSchemeId;
			
			default:
				return 0;
		}
	}
	
	public static void DeleteCachedResource(TourResource tourResource)
	{
		DeleteCachedResource(tourResource.ResourceType, tourResource.Id);
	}

	public static void DeleteCachedResource(TourResourceType resourceType, int resourceId)
	{
		//Utility.Trace(string.Format("DeleteCachedResource {0} {1}", resourceType, resourceId));
		Hashtable resourceTable = GetCachedReourceTable(resourceType);
		resourceTable.Remove(resourceId);
	}

	public void EmailChanged()
	{
		// Erase the email value so that the next time the Email property is
		// called, we'll fetch the new value from the database.
		email = null;
	}

	private void EvaluateHotspotLimitStatus(Tour tour, bool accountWasOverLimit)
	{
		// This method is called when the number of available hotspots increases or decreases.
		// If determines if the change puts the account over or under its hotspot limit and
		// sets or clears tour flags accordingly.

		if (accountWasOverLimit)
		{
			if (HotspotLimitStatus != HotspotLimitStatus.OverLimit)
			{
				// The account was over its limit, but is not anymore. Clear the exceeds-limit flag in any offending tours.
				MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_UpdateNoneExceedsSlideLimit", "@AccountId", accountId);
				Tour selectedTour = MapsAliveState.SelectedTourOrNull;
				if (selectedTour != null)
					selectedTour.ExceedsSlideLimit = false;
			}
		}
		else
		{
			if (HotspotLimitStatus == HotspotLimitStatus.OverLimit && tour != null)
			{
				// The account was not over its limit, but is now. Mark which tour caused the offense.
				tour.ExceedsSlideLimit = true;
			}
		}
	}

	public static Category GetCachedCategory(int categoryId)
	{
		return (Category)GetCachedResource(TourResourceType.Category, categoryId);
	}

	public static ColorScheme GetCachedColorScheme(int colorSchemeId)
	{
		if (colorSchemeId == 0)
		{
		//	Debug.Fail("Attempt to GetCachedColorScheme of 0 -- using account default");
			colorSchemeId = MapsAliveState.Account.DefaultColorSchemeId;
		}
		return (ColorScheme)GetCachedResource(TourResourceType.TourStyle, colorSchemeId);
	}

	public static FontStyleResource GetCachedFontStyle(int fontStyleId)
	{
		if (fontStyleId == 0)
		{
		//	Debug.Fail("Attempt to GetCachedFontStyle of 0 -- using account default");
			fontStyleId = MapsAliveState.Account.DefaultFontStyleId;
		}
		return (FontStyleResource)GetCachedResource(TourResourceType.FontStyle, fontStyleId);
	}

	public static Marker GetCachedMarker(int markerId)
	{
		if (markerId == 0)
		{
		//	Debug.Fail("Attempt to GetCachedMarker of 0 -- using account default");
			markerId = MapsAliveState.Account.DefaultMarkerId;
		}
		return (Marker)GetCachedResource(TourResourceType.Marker, markerId);
	}

	public static MarkerStyle GetCachedMarkerStyle(int markerStyleId)
	{
		if (markerStyleId == 0)
		{
		//	Debug.Fail("Attempt to GetCachedMarkerStyle of 0 -- using account default");
			markerStyleId = MapsAliveState.Account.DefaultMarkerStyleId;
		}
		return (MarkerStyle)GetCachedResource(TourResourceType.MarkerStyle, markerStyleId);
	}

	public static TourResource GetCachedResource(TourResourceType resourceType, int resourceId)
	{
		//Utility.Trace(string.Format("GetCachedResource {0} {1}", resourceType, resourceId));
		
		// Get the table of cached resources of the specified TourResourceType.
		Hashtable resourceTable = GetCachedReourceTable(resourceType);

		// See if the table contains a resource with the specified Id.
		TourResource tourResource = (TourResource)resourceTable[resourceId];

		if (tourResource == null)
		{
			// The requested resource is not in the table. Create a new/ resource from its database values
			// and add it to the table. If the resource comes back null, the user probably used the Back
			// button after deleting a resource.
			tourResource = CreateNewTourResource(resourceType, resourceId);
			
			if (tourResource != null)
				resourceTable.Add(resourceId, tourResource);
		}
		
		return tourResource;
	}
	
	private static Hashtable GetCachedReourceTable(TourResourceType resourceType)
	{
		Hashtable table = (Hashtable)MapsAliveState.Retrieve(MapsAliveObjectType.CachedResourcesTable);
		if (table == null)
		{
			// The cache is empty.  Create a new table and cache it. This only happens
			// once after a session starts and a resource is requested for the first time.
			table = new Hashtable();

			table.Add(TourResourceType.Category, new Hashtable());
			table.Add(TourResourceType.Marker, new Hashtable());
			table.Add(TourResourceType.MarkerStyle, new Hashtable());
			table.Add(TourResourceType.TooltipStyle, new Hashtable());
			table.Add(TourResourceType.TourStyle, new Hashtable());
			table.Add(TourResourceType.Symbol, new Hashtable());
			table.Add(TourResourceType.FontStyle, new Hashtable());

			MapsAliveState.Persist(MapsAliveObjectType.CachedResourcesTable, table);
		}

		return (Hashtable)table[resourceType];
	}

	public static Symbol GetCachedSymbol(int symbolId)
	{
		if (symbolId == 0)
		{
		//	Debug.Fail("Attempt to GetCachedSymbol of 0 -- using account default");
			symbolId = MapsAliveState.Account.DefaultSymbolId;
		}
		return (Symbol)GetCachedResource(TourResourceType.Symbol, symbolId);
	}

	public static TooltipStyle GetCachedTooltipStyle(int tooltipStyleId)
	{
		if (tooltipStyleId == 0)
		{
		//	Debug.Fail("Attempt to GetCachedTooltipStyle of 0 -- using account default");
			tooltipStyleId = MapsAliveState.Account.DefaultTooltipStyleId;
		}
		return (TooltipStyle)GetCachedResource(TourResourceType.TooltipStyle, tooltipStyleId);
	}

	public static Marker GetMarkerOrNull(int markerId)
	{
		Hashtable markerTable = GetCachedReourceTable(TourResourceType.Marker);
		return (Marker)markerTable[markerId];
	}

	public DataTable GetTours()
	{
		return MapsAliveDatabase.LoadDataTable("sp_Tour_GetToursByAccountId", "@AccountId", accountId);
	}

	public void HotspotAdded(Tour tour)
	{
		bool wasOverLimit = HotspotLimitStatus == HotspotLimitStatus.OverLimit;
		
		switch (HotspotLimitStatus)
		{
			case HotspotLimitStatus.UnderLimit:
				// We don't know if the new hotspot will change the status so set it
				// to unknown to force it to be recalculated the next time requested.
				_hotspotLimitStatus = HotspotLimitStatus.Unknown;
				break;
			
			case HotspotLimitStatus.AtLimit:
				// The new hotspot puts the account over its limit.
				_hotspotLimitStatus = HotspotLimitStatus.OverLimit;
				break;
			
			case HotspotLimitStatus.OverLimit:
				// The account is already over its limit. Adding another hotspot won't change the status.
				break;
			
			default:
				Debug.Fail("HotspotAdded: Unexpected HotspotLimitStatus: " + _hotspotLimitStatus);
				break;
		}

		EvaluateHotspotLimitStatus(tour, wasOverLimit);
	}

	public void HotspotDeleted(Tour tour)
	{
		bool wasOverLimit = HotspotLimitStatus == HotspotLimitStatus.OverLimit;

		switch (HotspotLimitStatus)
		{
			case HotspotLimitStatus.UnderLimit:
				// The account is already under its limit. deleting a hotspot won't change the status.
				break;

			case HotspotLimitStatus.AtLimit:
				// The deleted hotspot puts the account under its limit.
				_hotspotLimitStatus = HotspotLimitStatus.UnderLimit;
				break;

			case HotspotLimitStatus.OverLimit:
				// We don't know if the deletion will change the status so set it
				// to unknown to force it to be recalculated the next time requested.
				_hotspotLimitStatus = HotspotLimitStatus.Unknown;
				break;

			default:
				Debug.Fail("HotspotDeleted: Unexpected HotspotLimitStatus: " + _hotspotLimitStatus);
				break;
		}

		EvaluateHotspotLimitStatus(tour, wasOverLimit);
	}

	public string HotspotLimitMessage(HotspotLimitWarningContext warningContext)
	{
		string message = AppContent.Topic("Special" + warningContext);
		int hotspotsAvailable = CalculateHotspotsAvailable();
		int hotspotsExceeded = hotspotsAvailable * -1;
		string tourCountString = TourCount == 1 ? "" : (TourCount.ToString() + " ");
		string statistics;
		
		switch (warningContext)
		{
			case HotspotLimitWarningContext.AddNewHotspotAccountAtLimit:
			case HotspotLimitWarningContext.ImportTourAccountAtLimit:
			case HotspotLimitWarningContext.ImportHotspotsAccountAtLimit:
				statistics = string.Format(AppContent.Topic("SpecialStatisticsAccountAtLimit"), tourCountString, Utility.Plural(TourCount), HotspotLimit);
				message = statistics + message;
				break;
			
			case HotspotLimitWarningContext.AddNewHotspotAccountOverLimit:
			case HotspotLimitWarningContext.AddNewHotspotTourOverLimit:
			case HotspotLimitWarningContext.ImportTourAccountOverLimit:
			case HotspotLimitWarningContext.ImportHotspotsAccountOverLimit:
			case HotspotLimitWarningContext.ImportHotspotsTourOverLimit:
				statistics = string.Format(AppContent.Topic("SpecialStatisticsAccountOverLimit"), tourCountString, Utility.Plural(TourCount), HotspotLimit, hotspotsExceeded, Utility.Plural(hotspotsExceeded));
				message = statistics + message;
				break;

			case HotspotLimitWarningContext.ExportTourOverLimit:
			case HotspotLimitWarningContext.TourOverLimit:
			case HotspotLimitWarningContext.PublishTourOverLimit:
				statistics = string.Format(AppContent.Topic("SpecialStatisticsTourOverLimit"), hotspotsExceeded, Utility.Plural(hotspotsExceeded), HotspotLimit);
				message = statistics + message;
				break;
			
			default:
				break;
		}

        // Add link to the user guide at the end of the warning message.
        string extra = string.Format(" <a id='LearnMoreLink' href='https://mapsalive.com/docs/about-v3/#borrowing-hotspots' target='_blank'>Learn about borrowing hotspots</a>.");

        return message + extra;
    }

	public HotspotLimitStatus HotspotLimitStatus
	{
		get
		{
			if (_hotspotLimitStatus == HotspotLimitStatus.Unknown)
			{
				int hotspotsAvailable = CalculateHotspotsAvailable();

				if (hotspotsAvailable > 0)
				{
					_hotspotLimitStatus = HotspotLimitStatus.UnderLimit;
				}
				else if (hotspotsAvailable == 0)
				{
					_hotspotLimitStatus = HotspotLimitStatus.AtLimit;
				}
				else
				{
					_hotspotLimitStatus = HotspotLimitStatus.OverLimit;
				}

			}
			return _hotspotLimitStatus;
		}
	}

	public static int HotspotsRequiredForReadyMaps
	{
		get { return 50; }
	}

	public MemberPageActionId LastActionIdForGroup(MemberPageActionId actionIdGroup)
	{
		if (lastActionIdDictionary.ContainsKey(actionIdGroup))
			return lastActionIdDictionary[actionIdGroup];
		else
			return MemberPageActionId.Undefined;
	}
	
	public MemberPageActionId LastActionIdForImportSlides
	{
		// Currently we only remember the last tab for import slides because we think this is one that
		// will be annoying or error-prone if we don't, especially for users who tend to always import
		// the same kind of file every time. Other tabs like tour properties are not remembered between sessions.
		get	{ return lastActionIdForImportSlides == MemberPageActionId.Undefined ? MemberPageActionId.ImportHotspotPhotos : lastActionIdForImportSlides; }
		set
		{
			if (lastActionIdForImportSlides != value)
			{
				lastActionIdForImportSlides = value;
				MapsAliveDatabase.ExecuteStoredProcedure("sp_Account_UpdateLastImportSlidesType",
				    "@AccountId", accountId,
					"@Type", DbIdForImportSlidesActionId(lastActionIdForImportSlides));
			}
		}
	}

	public int LastResourceId(TourResourceType resourceType)
	{
		if (lastResourceIdDictionary.ContainsKey(resourceType))
			return lastResourceIdDictionary[resourceType];
		else
			return 0;
	}

	public void Logout()
	{
		Roles.DeleteCookie();
		FormsAuthentication.SignOut();
		Utility.ImitateUser(null);
	}

	public static int NumberOfResourceDependents(TourResourceType resourceType, int resourceId)
	{
		switch (resourceType)
		{
			case TourResourceType.Category:
				return MapsAliveDatabase.GetCount("sp_TourViewCategory_CountOfSlidesUsingCategory", "@CategoryId", resourceId);
			
			case TourResourceType.Marker:
				return MapsAliveDatabase.GetCount("sp_Marker_CountOfSlidesUsingMarker", "@MarkerId", resourceId);

			case TourResourceType.MarkerStyle:
				return MapsAliveDatabase.GetCount("sp_MarkerStyle_CountOfMarkersUsingMarkerStyle", "@MarkerStyleId", resourceId);

			case TourResourceType.TooltipStyle:
				return MapsAliveDatabase.GetCount("sp_TooltipStyle_CountOfPagesUsingTooltipStyle", "@TooltipStyleId", resourceId);

			case TourResourceType.FontStyle:
				{
					int count = MapsAliveDatabase.GetCount("sp_FontStyle_CountOfMarkersUsingFontStyle", "@FontStyleId", resourceId);
					count += MapsAliveDatabase.GetCount("sp_FontStyle_CountOfTooltipsUsingFontStyle", "@FontStyleId", resourceId);
					return count;
				}
			
			case TourResourceType.TourStyle:
				return MapsAliveDatabase.GetCount("sp_TourStyle_CountOfToursUsingTourStyle", "@TourStyleId", resourceId);
			
			case TourResourceType.Symbol:
				return MapsAliveDatabase.GetCount("sp_Symbol_CountOfMarkersUsingSymbol", "@SymbolId", resourceId);
			
			default:
				Debug.Fail("Unsupported TourResourceType " + resourceType);
				return 0;
		}
	}

	public static bool PurgeAccount(string userName, bool deleteOrders, bool deleteResources, bool deleteUser)
	{
		int accountId = 0;

		try
		{
			MapsAliveDataRow accountRow = MapsAliveDatabase.LoadDataRow("sp_GetAccount_ByUserName", "@UserName", userName);
			if (accountRow == null)
				return false;

			accountId = accountRow.IntValue("AccountId");

			DataTable dataTable = MapsAliveDatabase.LoadDataTable("sp_Tour_GetToursByAccountId", "@AccountId", accountId);
			foreach (DataRow dataRow in dataTable.Rows)
			{
				MapsAliveDataRow tourRow = new MapsAliveDataRow(dataRow);
				int tourId = tourRow.IntValue("TourId");
				Tour tour = new Tour(tourId, accountId);
				tour.Delete();
			}

			if (deleteUser || deleteResources)
			{
				// Note that we only delete resources when explicityly requested or when deleting the user because
				// otherwise the account would be left with no default resources which would cause an error on the
				// Preferences pages which lists the defaults.
				MapsAliveDatabase.ExecuteStoredProcedure("sp_Account_Delete_Resources", "@AccountId", accountId);
			}

			if (deleteOrders)
			{
				// WARNING: Do not delete orders unless they were only for test purposes.
				MapsAliveDatabase.ExecuteStoredProcedure("sp_Order_DeleteByAccount", "@AccountId", accountId);
			}

			if (deleteUser)
			{
				Membership.DeleteUser(userName, true);
			}

			return true;

		}
		catch (Exception ex)
		{
			Utility.ReportException("PurgeAccount " + accountId, ex);
			return false;
		}
	}

	private void RebuildAllTours()
	{
		DataTable dataTable = GetTours();
		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			int tourId = row.IntValue("TourId");
			Tour tour = new Tour(tourId, accountId);
			tour.RequireRebuild();
			tour.UpdateDatabase();
		}
	}

	public void RecordSessionInfo()
	{
		if (Utility.ImitatingUser())
			return;

		HttpContext context = HttpContext.Current;
		HttpRequest request = context.Request;
		HttpBrowserCapabilities browser = request.Browser;

		string sessionId = context.Session.SessionID;
		string ipAddress = request.UserHostAddress;
		string browserName = browser.Browser;
		string browserVersion = browser.Version;

		MapsAliveDatabase.ExecuteStoredProcedure("sp_Account_UpdateSessionInfo",
			"@AccountId", Id,
			"@SessionId", sessionId,
			"@SessionIpAddress", ipAddress,
			"@SessionBrowser", string.Format("{0} {1}", browserName, browserVersion)
		);
	}

	public void SetLastActionIdForGroup(MemberPageActionId actionIdGroup, MemberPageActionId actionId)
	{
		if (lastActionIdDictionary.ContainsKey(actionIdGroup))
			lastActionIdDictionary.Remove(actionIdGroup);
		lastActionIdDictionary.Add(actionIdGroup, actionId);
	}
	
	public void SetLastResourceId(TourResourceType resourceType, int resourceId)
	{
		if (lastResourceIdDictionary.ContainsKey(resourceType))
			lastResourceIdDictionary.Remove(resourceType);
		lastResourceIdDictionary.Add(resourceType, resourceId);
	}

	public ResourceFilters SetResourceIsFiltered(ResourceFilters filters, ResourceFilters flag, bool set)
	{
		if (set)
			filters |= flag;
		else
			filters &= ~flag;

		return filters;
	}

	public void SetTourState(TourState tourState)
	{
		DataTable dataTable = GetTours();
		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			int tourId = row.IntValue("TourId");
			Tour tour = new Tour(tourId, accountId);
			tour.SetState(tourState);
		}
	}

	public void UpdateAccountLimits(
		int days,
		int hotspotLimit,
		decimal creditAmount,
		decimal paymentAmount,
		int discountPercent,
		AccountType accountType,
		AccountPlan plan
		)
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Account_UpdateLimits",
			"@AccountId", accountId,
			"@Days", days,
			"@SlideLimit", hotspotLimit,
			"@CreditAmount", creditAmount,
			"@PaymentAmount", paymentAmount,
			"@DiscountPercent", discountPercent,
			"@AccountType", accountType,
			"@PlanId", (int)plan);

		UpdateHotspotStatus();
	}

	public void UpdateAccountPreferences(
		string siteName,
		bool showSlideContentInLayoutPreview,
		bool showTourNavigatorExpanded,
		bool disableTourAdvisor,
		bool markersZoom)
	{
		this.siteName = siteName;
		this.showSlideContentInLayoutPreview = showSlideContentInLayoutPreview;
		this.showTourNavigatorExpanded = showTourNavigatorExpanded;
		this.DisableTourAdvisor = disableTourAdvisor;
		this.ShowStepByStepHelp = markersZoom;
		
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Account_UpdatePreferences",
			"@AccountId", accountId,
			"@SiteName", siteName,
			"@ShowSlideContentInLayoutPreview", showSlideContentInLayoutPreview,
			"@ShowTourNavigatorExpanded", showTourNavigatorExpanded,
			"@DisableTourAdvisor", disableTourAdvisor,
			"@MarkersZoom", markersZoom);
	}

	public void UpdateAfterPurchase(
		bool isNewAccount,
		bool isRenewal,
		decimal creditAmountApplied,
		decimal paymentAmountApplied,
		int daysPurchased,
		int hotspotsDelta,
		AccountPlan plan)
	{
		if (!isNewAccount && MembershipExpired && daysPurchased > 0)
		{
			SetTourState(TourState.Active);
			Utility.SendEmailToAdmin("Plan Activated", string.Format("Account #{0} has been reactivated", accountId));
		}

		// Note that hotspotsDelta is a value relative to how many hotspots are already in the
		// account. It is not the number of hotspots being purchased except in the case of a 
		// first time purchase. A positive number means that additional hotspots are being
		// purchased. A negative number means a downgrade. A zero delta occurs when only days
		// are being purchased.

		// Apply the delta to the current hotspot limit to determine the new limit.
		if (type == AccountType.Trial)
		{
			type = AccountType.Paid;
			_days = 365;
			HotspotLimit = hotspotsDelta;

			// Force a rebuild of all tours to get rid of the "Created with MapsAlive Trial" message.
			RebuildAllTours();
		}
		else
		{
			_days = DaysRemaining + daysPurchased;
			HotspotLimit += hotspotsDelta;
		}

		Plan = plan;

		UpdateHotspotStatus();

		creditAmount -= creditAmountApplied;
		paymentAmount -= paymentAmountApplied;

		MapsAliveDatabase.ExecuteStoredProcedure("sp_Account_UpdateAfterPurchase",
			"@AccountId", accountId,
			"@Type", (int)type,
			"@PlanId", (int)Plan,
			"@CreditAmount", creditAmount,
			"@PaymentAmount", paymentAmount,
			"@Days", _days,
			"@SlideLimit", HotspotLimit,
			"@SubAccountLimit", 0
		);
	}

	public void UpdateAccountResourceSettings()
	{
		Account.UpdateAccountResourceSettings(
			this.Id,
			DefaultFontStyleId,
			DefaultMarkerId,
			DefaultMarkerStyleId,
			DefaultSymbolId,
			DefaultTooltipStyleId,
			DefaultColorSchemeId);
	}

	public static void UpdateAccountResourceSettings(
		int accountId,
		int defaultFontStyleId,
		int defaultMarkerId,
		int defaultMarkerStyleId,
		int defaultSymbolId,
		int defaultTooltipStyleId,
		int defaultColorSchemeId
	)
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Account_UpdateDefaultResourceIds",
		"@AccountId", accountId,
		"@DefaultFontStyleId", defaultFontStyleId,
		"@DefaultMarkerId", defaultMarkerId,
		"@DefaultMarkerStyleId", defaultMarkerStyleId,
		"@DefaultSymbolId", defaultSymbolId,
		"@DefaultTooltipStyleId", defaultTooltipStyleId,
		"@DefaultTourStyleId", defaultColorSchemeId);
	}

	public void UpdateContactNameInDatabase(string contactName)
	{
		this.contactName = contactName;
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Account_UpdateContactName",
			"@AccountId", accountId,
			"@ContactName", contactName);
	}

	public void UpdateHotspotStatus()
	{
		// This method is called when a tour or map is deleted which usually results in
		// hotspots being deleted as well. If the account was at or over its limit prior
		// to this call. we need to update the hotspot status in case the deletion
		// caused it to change. This method is also called when the limit changes because
		// the user bought more hotspots or the sys admin changed their limit. In those
		// two cases, there may still be a tour that is marked as exceeding its limit.

		if (HotspotLimitStatus == HotspotLimitStatus.UnderLimit)
		{
			// Make sure no tour is still marked as exceeding its limit.
			MapsAliveDatabase.ExecuteStoredProcedure("sp_Tour_UpdateNoneExceedsSlideLimit", "@AccountId", accountId);
			return;
		}

		bool wasOverLimit = HotspotLimitStatus == HotspotLimitStatus.OverLimit;
		_hotspotLimitStatus = HotspotLimitStatus.Unknown;
		EvaluateHotspotLimitStatus(null, wasOverLimit);
	}

	public void UpdateSendNewsletterInDatabase(bool sendNewsletter)
	{
		this.sendNewsletter = sendNewsletter;
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Account_UpdateSendNewsletter",
			"@AccountId", accountId,
			"@SendNewsletter", sendNewsletter);
	}
}
