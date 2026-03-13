// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Diagnostics;

// These values are known in the DB -- do not change.
public enum CategoryType
{
	DirectoryGroup = 1,
	ImageAreaOverride = 2
}

public class Category : TourResource
{
	public Category()
	{
		// This constructor is used to create a new category.
		accountId = MapsAliveState.Account.Id;
		ConstructNewCategory(this);
	}

	public Category(CategoryType categoryType)
	{
		// This constructor is used to create the default category.
		Type = categoryType;
		Code = "Other";
		Title = "Other";
		DirectoryPosition = 0;
	}

	public Category(int categoryId)
	{
		// This constructor is used to create a category from the database..
		if (LoadResourceRowFromDatabase(categoryId))
			InitializeResourceFromDataRecord(row);
	}

	private static void ConstructNewCategory(Category category)
	{
		// Generate a default code that is not already in use.
		int suffix = MapsAliveDatabase.GetCount("sp_Category_GetCategoryCountByAccountId", "@AccountId", Utility.AccountId);
		string code;
		string title;
		do
		{
			suffix++;
			code = string.Format("{0}{1}", MapsAliveTourBuilder.Text.CategoryKindName, suffix);
			title = string.Format("{0} {1}", MapsAliveTourBuilder.Text.CategoryKindTitle, suffix);
		} while (Account.CategoryCodeInUse(0, code));

		category.Code = code;
		category.Title = title;
		category.Type = CategoryType.DirectoryGroup;
		category.DirectoryPosition = 1;
	}

	public override void InitializeResourceFromDataRecord(MapsAliveDataRecord record)
	{
		bool isRow = record is MapsAliveDataRow;
		
		Type = (CategoryType)record.IntValue("Type", Tag.categoryType);
		Code = record.StringValue(Tag.code);
		Title = record.StringValue(Tag.title);
		DirectoryPosition = record.IntValue(Tag.directoryPosition);
		Width = record.IntValue(Tag.width);
		Height = record.IntValue(Tag.height);
	}

	public enum Tag
	{
		id,
		name,
		categoryType,
		code,
		title,
		directoryPosition,
		width,
		height
	}

	public override string GetTagValue(int tagId)
	{
		Tag tag = (Tag)tagId;

		switch (tag)
		{
			case Tag.id:
				return Id.ToString();

			case Tag.name:
			    return string.Empty;

			case Tag.categoryType:
				return ((int)Type).ToString();

			case Tag.code:
				return Code;

			case Tag.title:
				return Title;

			case Tag.directoryPosition:
				return DirectoryPosition.ToString();

			case Tag.width:
				return Width.ToString();

			case Tag.height:
				return Height.ToString();

			default:
				Debug.Fail("Unsupported Symbol XML tag requested " + tag);
				return "???";
		}
	}

	public override bool HasSameAppearanceAs(TourResource resource)
	{
		Category that = (Category)resource;
		if (this.Type != that.Type || this.Title != that.Title)
			return false;

		if (this.Type == CategoryType.DirectoryGroup)
			return this.DirectoryPosition == that.DirectoryPosition;
		else if (this.Type == CategoryType.ImageAreaOverride)
			return this.Width == that.Width && this.Height == that.Height;

		return false;
	}

	public string Code { get; set; }

	public int DirectoryPosition { get; set; }

	public int Height { get; set; }

	public override TourResourceType ResourceType
	{
		get { return TourResourceType.Category; }
	}

	public string Title { get; set; }

	public CategoryType Type { get; set; }

	public int Width { get; set; }

	public void Delete()
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Category_Delete", "@CategoryId", resourceId);
		Tour tour = MapsAliveState.SelectedTour;
		if (tour != null)
			tour.ReloadCategories();
		
		MapsAliveState.Account.SetLastResourceId(TourResourceType.Category, 0);
	}

	public static DataTable GetFilteredCategoryList(bool filterForTour, Tour tour, int accountId)
	{
		DataTable dataTable = null;

		if (filterForTour && tour != null)
		{
			dataTable = MapsAliveDatabase.LoadDataTable("sp_Category_GetCategorysUsedByTour",
				"@AccountId", accountId, "@TourId", tour.Id);
		}
		else
		{
			dataTable = MapsAliveDatabase.LoadDataTable("sp_Category_GetCategorysOwnedByAccount",
				"@AccountId", accountId);
		}

		return dataTable;
	}

	public static void InvalidateToursThatDependOnCategory(int categoryId)
	{
		DataTable dataTable = MapsAliveDatabase.LoadDataTable("sp_Tour_GetToursThatUseCategory",
			"@AccountId", Utility.AccountId,
			"@CategoryId", categoryId);

		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			Tour tour = Tour.GetSelectedTourOrCreateFromDatabase(row.IntValue("TourId"));
			tour.ReloadCategories();
			tour.UpdateDatabase();
			Utility.Trace(string.Format("InvalidateToursThatDependOnCategory {0}", tour.Name));
		}
	}

	public override void InsertIntoDatabase(int accountId)
	{
		resourceId = (int)MapsAliveDatabase.ReadScalar("sp_Category_CreateCategory",
			"@AccountId", accountId,
			"@Type", (int)Type,
			"@Code", Code,
			"@Title", Title,
			"@DirectoryPosition", DirectoryPosition,
			"@Width", Width,
			"@Height", Height
		);
	}

	public override void UpdateDatabase()
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Category_UpdateCategory",
			"@CategoryId", resourceId,
			"@Type", Type,
			"@Code", Code,
			"@Title", Title,
			"@DirectoryPosition", DirectoryPosition,
			"@Width", Width,
			"@Height", Height
		);
	}
}
