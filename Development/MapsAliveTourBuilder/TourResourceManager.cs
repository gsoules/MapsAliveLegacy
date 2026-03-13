// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Diagnostics;
using System.IO;

public class TourResourceManager
{
	// The purpose of this class is to provide static knowledge of TourResource subclasses.
	// These methods are called to get information about a subclass as opposed to an instance
	// of a subclass.

	public static string ClickScript(TourResourceType resourceType, int id)
	{
		string resourceTypeName = resourceType == TourResourceType.TourStyle ? "ColorScheme" : resourceType.ToString();
		return string.Format("maSafeTransfer('/Members/Edit{0}.aspx?id={1}');", resourceTypeName, id);
	}

	public static string ClickScript(TourResourceType resourceType, int id, int accountId)
	{
		string script;
		if (accountId == 0)
			script = string.Format("maSafeTransfer('/Members/DuplicateResource.aspx?rt={0}');", (int)resourceType);
		else
			script = string.Format("maSafeTransfer('/Members/Edit{0}.aspx?id={1}');", resourceType, id);
		return script;
	}

	public static TourResource CreateNewResource(TourResourceType resourceType)
	{
		switch (resourceType)
		{
			case TourResourceType.Category:
				return new Category();

			case TourResourceType.FontStyle:
			    return new FontStyleResource();

			case TourResourceType.Marker:
				return new Marker();

			case TourResourceType.MarkerStyle:
				return new MarkerStyle();

			case TourResourceType.TourStyle:
				return new ColorScheme();

			case TourResourceType.TooltipStyle:
				return new TooltipStyle();

			case TourResourceType.Symbol:
				return new Symbol();

			default:
				System.Diagnostics.Debug.Fail("CreateNewResource is not supported for " + resourceType);
				return null;
		}
	}

	public static TourResource CreateNewResource(TourResourceType resourceType, int resourceId)
	{
		TourResource resource = null;

		switch (resourceType)
		{
			case TourResourceType.Category:
				resource = new Category(resourceId);
				break;
			
			case TourResourceType.FontStyle:
				resource = new FontStyleResource(resourceId);
				break;
			
			case TourResourceType.Marker:
				resource = new Marker(resourceId);
				break;
			
			case TourResourceType.MarkerStyle:
				resource = new MarkerStyle(resourceId);
				break;
			
			case TourResourceType.TourStyle:
				resource = new ColorScheme(resourceId);
				break;
			
			case TourResourceType.TooltipStyle:
				resource = new TooltipStyle(resourceId);
				break;
			
			case TourResourceType.Symbol:
				resource = new Symbol(resourceId);
				break;

			default:
				System.Diagnostics.Debug.Fail("CreateNewResource is not supported for " + resourceType);
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

	public static void DeleteUnusedResources(TourResourceType resourceType)
	{
		Account account = MapsAliveState.Account;

		string sp = string.Format("sp_{0}_Get{0}sOwnedByAccount", resourceType);
		DataTable dataTable = MapsAliveDatabase.LoadDataTable(sp, "AccountId", Utility.AccountId);
		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			int resourceId = row.IntValue(string.Format("{0}Id", resourceType));
			TourResource tourResource = Account.GetCachedResource(resourceType, resourceId);
			int dependentsCount = Account.NumberOfResourceDependents(resourceType, resourceId);
			if (resourceId == account.DefaultResourceId(resourceType))
				continue;
			if (dependentsCount == 0)
			{
				tourResource.DeleteResource();
				if (resourceType == TourResourceType.Marker && account.LastMarkerIdSelected == resourceId)
				{
					account.LastMarkerIdSelected = 0;
				}
			}
		}
	}

	public static MemberPageActionId GetExplorerActionId(TourResourceType resourceType)
	{
		switch (resourceType)
		{
			case TourResourceType.Category:
				return MemberPageActionId.CategoryExplorer;

			case TourResourceType.FontStyle:
				return MemberPageActionId.FontStyleExplorer;

			case TourResourceType.Marker:
				return MemberPageActionId.MarkerExplorer;

			case TourResourceType.MarkerStyle:
				return MemberPageActionId.MarkerStyleExplorer;

			case TourResourceType.Symbol:
				return MemberPageActionId.SymbolExplorer;

			case TourResourceType.TourStyle:
				return MemberPageActionId.ColorSchemeExplorer;

			case TourResourceType.TooltipStyle:
				return MemberPageActionId.TooltipStyleExplorer;

			default:
				return MemberPageActionId.Undefined;
		}
	}

	public static string GetTitle(TourResourceType resourceType)
	{
		switch (resourceType)
		{
			case TourResourceType.Category:
				return "Category";

			case TourResourceType.FontStyle:
				return "Font Style";

			case TourResourceType.Marker:
				return "Marker";

			case TourResourceType.MarkerStyle:
				return "Marker Style";

			case TourResourceType.Symbol:
				return "Symbol";

			case TourResourceType.TourStyle:
				return "Color Scheme";

			case TourResourceType.TooltipStyle:
				return "Tooltip Style";

			default:
				return "Unsupported Resource Type";
		}
	}

	public static string GetTitlePlural(TourResourceType resourceType)
	{
		if (resourceType == TourResourceType.Category)
			return "Categories";
		else
			return GetTitle(resourceType) + "s";
	}

	public static bool HasResourceCode(TourResourceType resourceType)
	{
		return resourceType == TourResourceType.Category;
	}

	public static bool HasResourceImageUrl(TourResourceType resourceType)
	{
		return resourceType != TourResourceType.Category;
	}

	public static bool CopyAllSystemResourcesToAccount(Account account, bool creatingNewAccount)
	{
		try
		{
			account.CreateResourceImageFileDiskCache();

			string masterResourcesFileLocation = FileManager.WebAppFileLocationAbsolute("App_Data", "MasterResources.zip");
			FileStream fileStream = new FileStream(masterResourcesFileLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			ImporterForArchiveXml importer = new ImporterForArchiveXml(fileStream, "Import Resources");
			importer.ImportingResourcesForNewAccount = creatingNewAccount;
			importer.ImportArchiveFromStream(account);

			if (importer.ImportFailed)
			{
				Utility.ReportError("CopyAllSystemResourcesToAccount", importer.StatusMessage);
				return false;
			}

			return true;

		}
		catch (Exception ex)
		{
			Utility.ReportException("CopyAllSystemResourcesToAccount " + account.Id, ex);
			return false;
		}
	}

	// TEMPORARY FOR CONVERTING 2.6 to 3.0
	public static void CopyAndConvertAllSystemResourcesToAccount(Account account)
	{
		account.CreateResourceImageFileDiskCache();

		int accountId = account.Id;

		// The calls below are in dependency order. Higher resources are dependent on lower.
		// DO NOT CHANGE THIS SEQUENCE.
		//
		int defaultMarkerId = CopySystemResourcesToAccount(TourResourceType.Marker, accountId);
		int defaultSymbolId = CopySystemResourcesToAccount(TourResourceType.Symbol, accountId);
		int defaultMarkerStyleId = CopySystemResourcesToAccount(TourResourceType.MarkerStyle, accountId);
		int defaultTooltipStyleId = CopySystemResourcesToAccount(TourResourceType.TooltipStyle, accountId);
		int defaultFontStyleId = CopySystemResourcesToAccount(TourResourceType.FontStyle, accountId);
		int defaultColorSchemeId = CopySystemResourcesToAccount(TourResourceType.TourStyle, accountId);

		Account.UpdateAccountResourceSettings(
			accountId,
			defaultFontStyleId,
			defaultMarkerId,
			defaultMarkerStyleId,
			defaultSymbolId,
			defaultTooltipStyleId,
			defaultColorSchemeId);
	}

	// TEMPORARY FOR CONVERTING 2.6 to 3.0
	public static int CopySystemResourcesToAccount(TourResourceType resourceType, int accountId)
	{
		int defaultId = 0;

		string spName = string.Format("sp_{0}_Get{0}sOwnedByMapsAlive", resourceType.ToString());
		DataTable dataTable = MapsAliveDatabase.LoadDataTable(spName);

		foreach (DataRow dataRow in dataTable.Rows)
		{
			// Get the Id of the system resource.
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			string idName = string.Format("{0}Id", resourceType.ToString());
			int oldId = row.IntValue(idName);
			string systemResourceName = row.StringValue("Name");
			
			// Only the "No Symbol" resource should have an Id of 0.
			if (oldId == 0)
				continue;

			// Copy the system resource.
			TourResource newResource = TourResource.DuplicateResourceInDatabase(accountId, resourceType, oldId, ResourceDuplicateAction.ImportSystemResource);

			// If the resource comes back null, the account already has one with the same name.
			if (newResource != null)
			{
				int newId = newResource.Id;
				FixupResourceReferences(resourceType, oldId, newId, accountId);
			}
			
			// Use the first system resource as the default resource. The OwnedByMapsAlive stored procedures
			// return records in name order. The default resource is prefixed with "*" so that it sorts first.
			// If no name has that prefix, the first name alphabetically gets used.
			if (defaultId == 0)
			{
				if (newResource == null)
				{
					//TEMPORARY WHILE CONVERTING THE DEVELOPMENT DATABASE.
					if (systemResourceName.StartsWith("*"))
						systemResourceName = systemResourceName.Substring(1); // Strip off the leading *
					string sp = string.Format("sp_{0}_GetByName", resourceType.ToString());
					defaultId = MapsAliveDatabase.ReadInt(sp, "@AccountId", accountId, "@Name", systemResourceName);
					// END TEMPORARY
				}
				else
					defaultId = newResource.Id;
			}
		}

		return defaultId;
	}

	private static void FixupResourceReferences(TourResourceType resourceType, int oldId, int newId, int accountId)
	{
		switch (resourceType)
		{
			case TourResourceType.FontStyle:
				UpdateMarkersThatUseFontStyle(oldId, newId, accountId);
				UpdateTooltipsThatUseFontStyle(oldId, newId, accountId);
				break;

			case TourResourceType.Marker:
				UpdateTourViewsThatUseMarker(oldId, newId, accountId);
				break;

			case TourResourceType.MarkerStyle:
				UpdateMarkersThatUseMarkerStyle(oldId, newId, accountId);
				break;

			case TourResourceType.Symbol:
				UpdateMarkersThatUseSymbol(oldId, newId, accountId);
				break;

			case TourResourceType.TourStyle:
				UpdateToursThatUseColorScheme(oldId, newId, accountId);
				break;

			case TourResourceType.TooltipStyle:
				UpdateTourPagesThatUseTooltipStyle(oldId, newId, accountId);
				break;
		}
	}

	private static void UpdateResourceDependent(string dependent, TourResourceType resourceType, int oldId, int newId, int accountId)
	{
		string spGetDependents = string.Format("sp_{0}_Get{0}sThatUse{1}", dependent, resourceType.ToString());
		string spUpdateDependent = string.Format("sp_{0}_Update{1}Id", dependent, resourceType.ToString());
		string resourceIdColumn = string.Format("{0}Id", resourceType.ToString());
		string dependentIdColumn = string.Format("{0}Id", dependent);

		// Get all the dependents in this account that reference the old resource.
		DataTable dataTable = MapsAliveDatabase.LoadDataTable(spGetDependents,
			"@AccountId", accountId, "@" + resourceIdColumn, oldId);

		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			int dependentId = row.IntValue(dependentIdColumn);

			// Update the dependent to reference the new resource.
			MapsAliveDatabase.ExecuteStoredProcedure(spUpdateDependent,
				"@" + dependentIdColumn, dependentId, "@" + resourceIdColumn, newId);
		}
	}

	private static void UpdateMarkersThatUseMarkerStyle(int oldId, int newId, int accountId)
	{
		UpdateResourceDependent("Marker", TourResourceType.MarkerStyle, oldId, newId, accountId);
	}

	private static void UpdateMarkersThatUseFontStyle(int oldId, int newId, int accountId)
	{
		UpdateResourceDependent("Marker", TourResourceType.FontStyle, oldId, newId, accountId);
	}

	private static void UpdateTooltipsThatUseFontStyle(int oldId, int newId, int accountId)
	{
		UpdateResourceDependent("TooltipStyle", TourResourceType.FontStyle, oldId, newId, accountId);
	}

	private static void UpdateTourPagesThatUseTooltipStyle(int oldId, int newId, int accountId)
	{
		UpdateResourceDependent("TourPage", TourResourceType.TooltipStyle, oldId, newId, accountId);
	}

	private static void UpdateToursThatUseColorScheme(int oldId, int newId, int accountId)
	{
		UpdateResourceDependent("Tour", TourResourceType.TourStyle, oldId, newId, accountId);
	}

	private static void UpdateTourViewsThatUseMarker(int oldId, int newId, int accountId)
	{
		UpdateResourceDependent("TourView", TourResourceType.Marker, oldId, newId, accountId);
	}

	private static void UpdateMarkersThatUseSymbol(int oldId, int newId, int accountId)
	{
		DataTable dataTable = MapsAliveDatabase.LoadDataTable("sp_Marker_GetMarkersThatUseSymbol",
			"@AccountId", accountId, "@SymbolId", oldId);

		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			int dependentId = row.IntValue("MarkerId");

			int normalSymbolId = row.IntValue("NormalSymbolId");
			int selectedSymbolId = row.IntValue("SelectedSymbolId");

			if (normalSymbolId == oldId)
				normalSymbolId = newId;

			if (selectedSymbolId == oldId)
				selectedSymbolId = newId;

			MapsAliveDatabase.ExecuteStoredProcedure("sp_Marker_UpdateSymbolIds",
				"@MarkerId", dependentId, "@NormalSymbolId", normalSymbolId, "@SelectedSymbolId", selectedSymbolId);
		}
	}
}
