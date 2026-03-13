// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Data;
using System.Diagnostics;

public class CategoryManager
{
	private ArrayList categoryTable;
	private ArrayList tourViewCategoryTable;

	public CategoryManager(Tour tour)
	{
		LoadCategoryTable(tour);
		LoadTourViewCategoryTable(tour);
	}

	public ArrayList CategoryTable
	{
		get { return categoryTable; }
	}

	public bool HasDirectoryEntries
	{
		get { return tourViewCategoryTable.Count > 0; }
	}

	public TourViewCategory AddTourViewCategory(TourView tourView, string categoryCode)
	{
		Category category = GetCategory(categoryCode);
		if (category == null)
			return null;

		TourViewCategory tourViewCategory = GetTourViewCategory(tourView.Id, category.Id);
		if (tourViewCategory != null)
			return null;

		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourViewCategory_Create",
			"@TourViewId", tourView.Id,
			"@CategoryId", category.Id);

		tourViewCategory = AddTourViewCategory(tourView, category);

		return tourViewCategory;
	}

	public TourViewCategory AddTourViewCategory(TourView tourView, Category category)
	{
		TourViewCategory tourViewCategory = new TourViewCategory(tourView, category);
		tourViewCategoryTable.Add(tourViewCategory);

		return tourViewCategory;
	}

	public Category GetCategory(int categoryId)
	{
		foreach (Category category in categoryTable)
		{
			if (category.Id == categoryId)
				return category;
		}

		return null;
	}

	public Category GetCategory(string categoryCode)
	{
		foreach (Category category in categoryTable)
		{
			if (category.Code.ToLower() == categoryCode.ToLower())
				return category;
		}

		return null;
	}

	public Category GetImageOverrideCategory(int tourViewId)
	{
		foreach (TourViewCategory tourViewCategory in tourViewCategoryTable)
		{
			if (tourViewCategory.TourView.Id == tourViewId && tourViewCategory.Category.Type == CategoryType.ImageAreaOverride)
				return tourViewCategory.Category;
		}

		return null;
	}

	public ArrayList GetCategories(int tourViewId)
	{
		ArrayList list = new ArrayList();
		foreach (TourViewCategory tourViewCategory in tourViewCategoryTable)
		{
			if (tourViewCategory.TourView.Id == tourViewId)
			{
				Category category = tourViewCategory.Category;
				if (category.Id > 0)
					list.Add(category);
			}
		}
		return list;
	}

	public string GetCategoryList()
	{
		// Return a comma separated list of cagegory codes for the account.
		string list = string.Empty;
		foreach (Category category in categoryTable)
		{
			if (list != string.Empty)
				list += ",";
			list += category.Code;
		}
		return list;
	}

	public string GetCategoryList(int tourViewId)
	{
		// Return a comma separated list of cagegory codes for the tour view.
		string list = string.Empty;
		foreach (TourViewCategory tourViewCategory in tourViewCategoryTable)
		{
			if (tourViewCategory.TourView.Id == tourViewId)
			{
				Category category = tourViewCategory.Category;
				if (category.Id > 0)
				{
					if (list != string.Empty)
						list += ",";
					list += tourViewCategory.Category.Code;
				}
			}
		}
		return list;
	}

	public ArrayList GetTourViews(int categoryId, int tourPageId)
	{
		ArrayList tourViews = new ArrayList();
		foreach (TourViewCategory tourViewCategory in tourViewCategoryTable)
		{
			Category category = tourViewCategory.Category;
			TourPage tourPage = tourViewCategory.TourView.TourPage;

			if (category.Type != CategoryType.DirectoryGroup)
				continue;

			bool match = false;

			if (categoryId == -1 && tourPageId != -1)
				match = tourPage.Id == tourPageId;
			else if (categoryId != -1 && tourPageId == -1)
				match = category.Id == categoryId;
			else if (categoryId != -1 && tourPageId != -1)
				match = category.Id == categoryId && tourPage.Id == tourPageId;

			if (match)
				tourViews.Add(tourViewCategory.TourView);
		}
		return tourViews;
	}

	public void RemoveCategory(int tourViewId, int categoryId)
	{
		TourViewCategory tourViewCategory = GetTourViewCategory(tourViewId, categoryId);
		Debug.Assert(tourViewCategory != null, "No TourViewCategory found for " + tourViewId + ":" + categoryId);

		RemoveCategory(tourViewCategory);
	}

	private void RemoveCategory(TourViewCategory tourViewCategory)
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourViewCategory_Remove",
			"@TourViewId", tourViewCategory.TourView.Id,
			"@CategoryId", tourViewCategory.Category.Id);

		tourViewCategoryTable.Remove(tourViewCategory);
	}

	public void TourViewDeleted(int tourViewId)
	{
		foreach (TourViewCategory tourViewCategory in tourViewCategoryTable)
		{
			if (tourViewCategory.TourView.Id == tourViewId)
			{
				RemoveCategory(tourViewCategory);
				return;
			}
		}
	}

	public string TourViewNames(int tourViewId)
	{
		string names = string.Empty;
		foreach (TourViewCategory tourViewCategory in tourViewCategoryTable)
		{
			if (tourViewCategory.TourView.Id == tourViewId)
			{
				if (names != string.Empty)
					names += ", ";
				names += tourViewCategory.Category.Code;
			}
		}
		return names;
	}

	private TourViewCategory GetTourViewCategory(int tourViewId, int categoryId)
	{
		foreach (TourViewCategory tourViewCategory in tourViewCategoryTable)
		{
			if (tourViewCategory.TourView.Id == tourViewId && tourViewCategory.Category.Id == categoryId)
				return tourViewCategory;
		}

		return null;
	}

	private void LoadCategoryTable(Tour tour)
	{
		categoryTable = new ArrayList();

		DataTable dataTable;

		Account account = MapsAliveState.Account;
		bool filterByTour = account.ResourceIsFilteredBy(ResourceFilters.Category);

		dataTable = Category.GetFilteredCategoryList(filterByTour, tour, account.Id);
		
		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			int categoryId = row.IntValue("CategoryId");
			Category category = new Category(categoryId);
			Debug.Assert(category != null, "Category is null " + categoryId);
			categoryTable.Add(category);
		}

		// Add the default category.
		categoryTable.Add(new Category(CategoryType.DirectoryGroup));
	}

	private void LoadTourViewCategoryTable(Tour tour)
	{
		tourViewCategoryTable = new ArrayList();
		
		DataTable dataTable = MapsAliveDatabase.LoadDataTable("sp_TourViewCategory_GetByTourId", "@TourId", tour.Id);
		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			int tourViewId = row.IntValue("TourViewId");
			int categoryId = row.IntValue("CategoryId");

			TourView tourView = tour.GetTourView(tourViewId);
			Debug.Assert(tourView != null, "No TourView found for id " + tourViewId);

			Category category = GetCategory(categoryId);
			Debug.Assert(category != null, string.Format("No Category found for categoryId:{0} tourId:{1} tourViewId:{2}", categoryId, tour.Id, tourViewId));

			AddTourViewCategory(tourView, category);
		}
	}

	public bool TourViewHasCategory(int tourViewId, int categoryId)
	{
		foreach (TourViewCategory tourViewCategory in tourViewCategoryTable)
		{
			if (tourViewCategory.TourView.Id == tourViewId && tourViewCategory.Category.Id == categoryId)
				return true;
		}

		return false;
	}

	public bool TourViewHasCategory(int tourViewId, string categoryCode)
	{
		foreach (TourViewCategory tourViewCategory in tourViewCategoryTable)
		{
			if (tourViewCategory.TourView.Id == tourViewId && tourViewCategory.Category.Code == categoryCode)
				return true;
		}

		return false;
	}
}
