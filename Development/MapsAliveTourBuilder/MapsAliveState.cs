// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Diagnostics;
using System.Collections;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml;

public enum MapsAliveObjectType
{
	Account,
	ActionQueue,
	AnalyticsBuy,
	AnalyticsSignup,
	AppContentFilter,
	BuyOptions,
	CalculatorState,
	CachedResourcesTable,
	CheckList,
	Config,
	CustomHtmlEditorState,
	DatabaseStatus,
	DataTableForCreditCard,
	DataTableForCreditCardMonth,
	DataTableForCreditCardYear,
	DataTableForCountry,
	DataTableForFontFamily,
	DataTableForStateProvince,
	DeactivatedPageHtml,
	DeactivatedPageJavascript,
	EmailPlainTextForNewOrder,
	EmailPlainTextForNewRegistration,
	EmailPlainTextForNoChargeOrder,
	EmailPlainTextForLowTimeWarning,
	ExportTrace,
	FeatureSortAlpha,
	FindUsersFilter,
	HomeTourId,
	HtmlEditor,
	LastAction,
	LastPageAction,
	LastViewAction,
	MenuXmlAccount,
	MenuXmlProject,
	MenuXmlResources,
	Order,
	PostId,
	PhotoMarkerPlaceholderImage,
	ReadyMapsXml,
	RequestValidationErrorHandled,
	SampleFilterMap,
	SampleFilterFeatures,
	SiteContent,
	TallEditor,
	TallTextBox,
	Tour,
	TourExplorerSort,
	TourList,
	TourBuilderActionId,
	TourOptions,
	UserOrigin,
	WebSiteLinksXml,
	Xsl,
	XslV3
}

public class MapsAliveState
{
	private enum Action
	{
		Persist,
		Retrieve,
		Flush
	}

	public MapsAliveState()
	{
	}

	#region ===== Properties ========================================================

	public static bool FeatureSortAlpha
	{
		get
		{
			object value = Retrieve(MapsAliveObjectType.FeatureSortAlpha);
			if (value == null)
				return false;
			else
				return (bool)value;
		}
		set
		{
			Persist(MapsAliveObjectType.FeatureSortAlpha, value);
		}
	}

	public static bool HtmlEditor
	{
		get
		{
			object value = Retrieve(MapsAliveObjectType.HtmlEditor);
			if (value == null)
				return false;
			else
				return (bool)value;
		}
		set
		{
			Persist(MapsAliveObjectType.HtmlEditor, value);
		}
	}

	public static Order Order
	{
		get
		{
			return (Order)Retrieve(MapsAliveObjectType.Order);
		}
		set
		{
			if (value == null)
				Flush(MapsAliveObjectType.Order);
			else
				Persist(MapsAliveObjectType.Order, value);
		}
	}

	public static string Referrer
	{
		get { return GetStateCookie("ref"); }
		set { SetStateCookie("ref", value); }
	}

	public static Tour SelectedTour
	{
		get
		{
			Tour tour = SelectedTourOrNull;

			if (tour == null)
				Tour.DenyTourAccess();
			
			return tour;
		}
	}

	public static Tour SelectedTourOrNull
	{
		get
		{
			Tour tour = (Tour)Retrieve(MapsAliveObjectType.Tour);
			
			if (tour == null)
				tour = RecoverTourFromCookie();

			return tour;
		}
	}

	public static bool TallEditor
	{
		get
		{
			object value = Retrieve(MapsAliveObjectType.TallEditor);
			if (value == null)
				return false;
			else
				return (bool)value;
		}
		set
		{
			Persist(MapsAliveObjectType.TallEditor, value);
		}
	}

	public static bool TallTextBox
	{
		get
		{
			object value = Retrieve(MapsAliveObjectType.TallTextBox);
			if (value == null)
				return false;
			else
				return (bool)value;
		}
		set
		{
			Persist(MapsAliveObjectType.TallTextBox, value);
		}
	}
	#endregion

	#region ===== Public ============================================================

	public static Account Account
	{
		get
		{
			Account account = null;
			if (Utility.UserIsLoggedIn)
			{
				account = (Account)Retrieve(MapsAliveObjectType.Account);
				if (account == null)
				{
					account = new Account(Utility.UserId);
					if (account.Id == 0)
					{
						// This should never happen except during development when working on account logic.
						return null;
					}

					Persist(MapsAliveObjectType.Account, account);
					
					// Make sure that the reource image files for this account exist on disk.
					// Normally this call does nothing because the files are already there,
					// but because we check for the cache each time an Account object is created,
					// we can manually delete the cache folder and have it automatically reconstructed.
					// This is occasionally very useful for maintenance and development purposes.
					account.CreateResourceImageFileDiskCache();

					account.RecordSessionInfo();
				}
			}
			return account;
		}
	}

	public static DataTable DataTableForCountry()
	{
		return DataTableFromXmlFor(MapsAliveObjectType.DataTableForCountry, "Country");
	}

	public static DataTable DataTableForCreditCard()
	{
		return DataTableFromXmlFor(MapsAliveObjectType.DataTableForCreditCard, "CreditCard");
	}

	public static DataTable DataTableForCreditCardMonth()
	{
		return DataTableFromXmlFor(MapsAliveObjectType.DataTableForCreditCardMonth, "CreditCardMonth");
	}

	public static DataTable DataTableForCreditCardYear()
	{
		return DataTableFromXmlFor(MapsAliveObjectType.DataTableForCreditCardYear, "CreditCardYear");
	}

	public static DataTable DataTableForFontFamily()
	{
		return DataTableFor(MapsAliveObjectType.DataTableForFontFamily, "sp_FontFamily_GetAll");
	}

	public static DataTable DataTableForStateProvince()
	{
		return DataTableFromXmlFor(MapsAliveObjectType.DataTableForStateProvince, "StateProvince");
	}

	public static void Flush(MapsAliveObjectType mapsAliveObjectType)
	{
		PerformAction(Action.Flush, mapsAliveObjectType, null);
	}

	public static void FlushSessionState()
	{
		Flush(MapsAliveObjectType.Account);
		Flush(MapsAliveObjectType.ActionQueue);
		Flush(MapsAliveObjectType.AnalyticsBuy);
		Flush(MapsAliveObjectType.AnalyticsSignup);
		Flush(MapsAliveObjectType.CachedResourcesTable);
		Flush(MapsAliveObjectType.CalculatorState);
		Flush(MapsAliveObjectType.CheckList);
		Flush(MapsAliveObjectType.CustomHtmlEditorState);
		Flush(MapsAliveObjectType.FeatureSortAlpha);
		Flush(MapsAliveObjectType.ExportTrace);
		Flush(MapsAliveObjectType.HtmlEditor);
		Flush(MapsAliveObjectType.LastAction);
		Flush(MapsAliveObjectType.LastPageAction);
		Flush(MapsAliveObjectType.LastViewAction);
		Flush(MapsAliveObjectType.Order);
		Flush(MapsAliveObjectType.ReadyMapsXml);
		Flush(MapsAliveObjectType.RequestValidationErrorHandled);
		Flush(MapsAliveObjectType.SampleFilterMap);
		Flush(MapsAliveObjectType.SampleFilterFeatures);
		Flush(MapsAliveObjectType.TallEditor);
		Flush(MapsAliveObjectType.TallTextBox);
		Flush(MapsAliveObjectType.Tour);
		Flush(MapsAliveObjectType.TourBuilderActionId);
		Flush(MapsAliveObjectType.TourExplorerSort);
		Flush(MapsAliveObjectType.TourList);

		// Note that we don't flush the PostId because that would cause post validation to fail.
	}

	public static int GenerateNextPostId()
	{
		int postId = PostId;
		postId++;
		Persist(MapsAliveObjectType.PostId, postId);
		return postId;
	}

	public static int PostId
	{
		get
		{
			object postId = Retrieve(MapsAliveObjectType.PostId);
			return postId == null ? 0 : (int)postId;
		}
	}

	public static bool PostIsValid(HttpContext context)
	{
		return PostId.ToString() == context.Request.QueryString["post"];
	}

	public static void Persist(MapsAliveObjectType mapsAliveObjectType, object toCache)
	{
		if (toCache == null)
		{
			System.Diagnostics.Debug.Fail(string.Format("Attempt to Persist null object in MapsAliveState: {0}", mapsAliveObjectType));
			return;
		}

		PerformAction(Action.Persist, mapsAliveObjectType, toCache);
	}

	public static object Retrieve(MapsAliveObjectType mapsAliveObjectType)
	{
		return PerformAction(Action.Retrieve, mapsAliveObjectType, null);
	}

	public static void SetSelectedTour(Tour tour)
	{
		if (tour == null)
			Flush(MapsAliveObjectType.Tour);
		else
			Persist(MapsAliveObjectType.Tour, tour);
		string id = tour == null ? "0" : tour.Id.ToString();
		SetStateCookie("tour", id);
	}

	public static void SetSelectedTourPage(TourPage tourPage)
	{
		string id = tourPage == null ? "0" : tourPage.Id.ToString();
		SetStateCookie("page", id);
	}

	public static void SetSelectedTourView(TourView tourView)
	{
		string id = tourView == null ? "0" : tourView.Id.ToString();
		SetStateCookie("view", id);
	}

	#endregion

	#region ===== Private ===========================================================

	private static DataTable DataTableFor(MapsAliveObjectType type, string storedProcedure)
	{
		DataTable dataTable = (DataTable)MapsAliveState.Retrieve(type);
		if (dataTable == null)
		{
			dataTable = MapsAliveDatabase.LoadDataTable(storedProcedure);
			MapsAliveState.Persist(type, dataTable);
		}
		return dataTable;
	}

	private static DataTable DataTableFromXmlFor(MapsAliveObjectType type, string fileNamePrefix)
	{
		DataTable dataTable = (DataTable)MapsAliveState.Retrieve(type);
		if (dataTable == null)
		{
			string fileLocation = FileManager.WebAppFileLocationAbsolute("App_Data", fileNamePrefix + ".xml");
			DataSet dataSet = new DataSet();
			dataSet.ReadXml(fileLocation);
			dataTable = dataSet.Tables[0];
			MapsAliveState.Persist(type, dataTable);
		}
		return dataTable;
	}

	private static string Key(MapsAliveObjectType mapsAliveObjectType)
	{
		return "CACHE_" + mapsAliveObjectType.ToString();
	}

	private static object PerformAction(Action action, MapsAliveObjectType mapsAliveObjectType, object toCache)
	{
		Debug.Assert(!(action == Action.Persist && toCache == null), "Attempt to persist null object.  Use Flush instead");

		object fromCache = null;
		string key = Key(mapsAliveObjectType);

		switch (mapsAliveObjectType)
		{
			// Session object cache.
			// IMPORTANT: Keep this list in sync with FlushSessionState().
			case MapsAliveObjectType.Account:
			case MapsAliveObjectType.ActionQueue:
			case MapsAliveObjectType.AnalyticsBuy:
			case MapsAliveObjectType.AnalyticsSignup:
			case MapsAliveObjectType.AppContentFilter:
			case MapsAliveObjectType.CachedResourcesTable:
			case MapsAliveObjectType.CalculatorState:
			case MapsAliveObjectType.CheckList:
			case MapsAliveObjectType.CustomHtmlEditorState:
			case MapsAliveObjectType.ExportTrace:
			case MapsAliveObjectType.FeatureSortAlpha:
			case MapsAliveObjectType.FindUsersFilter:
			case MapsAliveObjectType.HtmlEditor:
			case MapsAliveObjectType.LastAction:
			case MapsAliveObjectType.LastPageAction:
			case MapsAliveObjectType.LastViewAction:
			case MapsAliveObjectType.Order:
			case MapsAliveObjectType.PostId:
			case MapsAliveObjectType.ReadyMapsXml:
			case MapsAliveObjectType.RequestValidationErrorHandled:
			case MapsAliveObjectType.SampleFilterMap:
			case MapsAliveObjectType.SampleFilterFeatures:
			case MapsAliveObjectType.TallEditor:
			case MapsAliveObjectType.TallTextBox:
			case MapsAliveObjectType.Tour:
			case MapsAliveObjectType.TourBuilderActionId:
			case MapsAliveObjectType.TourExplorerSort:
			case MapsAliveObjectType.TourList:
			{
				System.Web.SessionState.HttpSessionState session = HttpContext.Current.Session;
				if (session == null)
					return null;
				
				switch (action)
				{
					case Action.Persist:
						session[key] = toCache;
						break;

					case Action.Retrieve:
						fromCache = session[key];
						break;

					case Action.Flush:
						session.Remove(key);
						break;
				}
				break;
			}

			// Web application cache.
			case MapsAliveObjectType.BuyOptions:
			case MapsAliveObjectType.Config:
			case MapsAliveObjectType.DatabaseStatus:
			case MapsAliveObjectType.DataTableForCountry:
			case MapsAliveObjectType.DataTableForCreditCard:
			case MapsAliveObjectType.DataTableForCreditCardMonth:
			case MapsAliveObjectType.DataTableForCreditCardYear:
			case MapsAliveObjectType.DataTableForFontFamily:
			case MapsAliveObjectType.DataTableForStateProvince:
			case MapsAliveObjectType.DeactivatedPageHtml:
			case MapsAliveObjectType.DeactivatedPageJavascript:
			case MapsAliveObjectType.EmailPlainTextForNewOrder:
			case MapsAliveObjectType.EmailPlainTextForNewRegistration:
			case MapsAliveObjectType.EmailPlainTextForNoChargeOrder:
			case MapsAliveObjectType.HomeTourId:
			case MapsAliveObjectType.MenuXmlAccount:
			case MapsAliveObjectType.MenuXmlProject:
			case MapsAliveObjectType.MenuXmlResources:
			case MapsAliveObjectType.PhotoMarkerPlaceholderImage:
			case MapsAliveObjectType.SiteContent:
			case MapsAliveObjectType.TourOptions:
			case MapsAliveObjectType.UserOrigin:
			case MapsAliveObjectType.WebSiteLinksXml:
			case MapsAliveObjectType.Xsl:
			case MapsAliveObjectType.XslV3:
			{
				if (mapsAliveObjectType == MapsAliveObjectType.UserOrigin)
					key = HttpContext.Current.Session.SessionID;

				System.Web.Caching.Cache cache = HttpContext.Current.Cache;

				switch (action)
				{
					case Action.Persist:
						cache[key] = toCache;
						break;

					case Action.Retrieve:
						fromCache = cache[key];
						break;

					case Action.Flush:
						cache.Remove(key);
						break;
				}
				break;
			}
		}

		return fromCache;
	}

	private static Tour RecoverTourFromCookie()
	{
		if (!Utility.UserIsLoggedIn)
			return null;
		
		Tour tour = null;

		string id = GetStateCookie("tour");
		if (id != null && id != "0")
		{
			int tourId;
			if (int.TryParse(id, out tourId))
			{
				tour = new Tour(tourId);
				
				if (tour.Id == 0)
					tour = null;
				
				if (tour != null)
				{
					SetSelectedTour(tour);
					id = GetStateCookie("page");
					if (id != null && id != "0")
					{
						int tourPageId;
						if (int.TryParse(id, out tourPageId))
							tour.SetSelectedTourPage(tourPageId);

						id = GetStateCookie("view");
						if (id != null && id != "0")
						{
							int tourViewId;
							if (int.TryParse(id, out tourViewId))
								tour.SetSelectedTourView(tourViewId);
						}
					}
				}
			}
		}
		return tour;
	}

	private static string GetStateCookie(string cookieName)
	{
		try
		{
			string value = null;
			HttpContext context = HttpContext.Current;
			if (context != null)
			{
				HttpCookie cookie = context.Request.Cookies[cookieName];
				if (cookie != null)
				{
					value = cookie.Value;
				//	Debug.WriteLine(string.Format("GetStateCookie {0}:'{1}'", cookieName, value));
				}
			}
			return value;
		}
		catch
		{
			return null;
		}
	}

	private static void SetStateCookie(string cookieName, string value)
	{
		HttpContext context = HttpContext.Current;
		if (context != null)
		{
			context.Response.Cookies[cookieName].Value = value;
			context.Response.Cookies[cookieName].Expires = DateTime.Now.AddMonths(12);
		//	Debug.WriteLine(string.Format("SetStateCookie {0}:'{1}'", cookieName, value));
		}
	}
	#endregion
}