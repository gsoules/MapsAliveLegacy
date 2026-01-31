// Copyright (C) 2003-2010 AvantLogic Corporation
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Members_EditCategory : MemberPage
{
	private int categoryId;
	private bool optionsChanged;
	private string validCode;
	private string validTitle;
	private CategoryType validCategoryType;
	private int validPosition;
	private int validWidth;
	private int validHeight;

	protected void AddChangeDetection(DropDownList dropDownList, string script)
	{
		dropDownList.Attributes.Add("onchange", script);
	}

	protected override void InitControls(bool undo)
	{
		MemberPage.InitShowUsageControl(ShowUsageControl, category);

		CategoryType categoryType = IsPostBack ? (CategoryType)int.Parse(CategoryTypeDropDownList.SelectedValue) : category.Type;
		PositionPanel.Style.Add(HtmlTextWriterStyle.Display, categoryType == CategoryType.DirectoryGroup ? "block" : "none");
		DimensionsPanel.Style.Add(HtmlTextWriterStyle.Display, categoryType == CategoryType.ImageAreaOverride ? "block" : "none");
		
		if (!undo && IsPostBack)
			return;

		CategoryCodeTextBox.Text = category.Code;
		AddChangeDetection(CategoryCodeTextBox);
		
		CategoryTitleTextBox.Text = category.Title;
		AddChangeDetection(CategoryTitleTextBox);

		CategoryTypeDropDownList.SelectedValue = ((int)category.Type).ToString();
		AddChangeDetection(CategoryTypeDropDownList, "maShowDimensions(this);");

		PositionTextBox.Text = category.DirectoryPosition.ToString();
		AddChangeDetection(PositionTextBox);

		WidthTextBox.Text = category.Width == 0 ? string.Empty : category.Width.ToString();
		HeightTextBox.Text = category.Height == 0 ? string.Empty : category.Height.ToString();
		AddChangeDetection(HeightTextBox);
		AddChangeDetection(WidthTextBox);
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetActionId(MemberPageActionId.EditCategory);
		SetPageTitle(TourResource.GetTitleForEditPage(TourResourceType.Category));
		ParseQueryString();
		GetSelectedTourOrNone();
	}

	private void ParseQueryString()
	{
		if (IsPostBack || IsReturnToTourBuilder)
		{
			categoryId = account.LastResourceId(TourResourceType.Category);
			if (categoryId > 0)
			{
				category = new Category(categoryId);
				return;
			}
		}

		string id = Request.QueryString["id"];
		
		if (id == "new")
		{
			category = new Category();
			category.InsertIntoDatabase(account.Id);
			Tour tour = MapsAliveState.SelectedTourOrNull;
			if (tour != null)
				tour.ReloadCategories();
		}
		else
		{
			int.TryParse(id, out categoryId);
			if (categoryId != 0)
				category = new Category(categoryId);
		}

		if (category != null)
			categoryId = category.Id;
		
		if (categoryId == 0 || category == null || category.AccountId != Utility.AccountId)
		{
			// There was no Id on the query string or it was not a valid Id.
			Server.Transfer(MemberPageAction.ActionPageTarget(MemberPageActionId.CategoryExplorer));
		}
		else
		{
			account.SetLastResourceId(TourResourceType.Category, categoryId);
		}
	}

	protected override void PerformUpdate()
	{
		if (optionsChanged)
			category.UpdateResourceAndDependents();
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Resources;
	}

	protected override void ReadPageFields()
	{
		optionsChanged = false;

		if (category.Type != validCategoryType)
		{
			category.Type = validCategoryType;
			optionsChanged = true;
		}

		if (category.Code != validCode)
		{
			category.Code = validCode;
			optionsChanged = true;
		}

		if (category.Title != validTitle)
		{
			category.Title = validTitle;
			optionsChanged = true;
		}
		
		if (validCategoryType == CategoryType.DirectoryGroup)
		{
			if (category.DirectoryPosition != validPosition)
			{
				category.DirectoryPosition = validPosition;
				optionsChanged = true;
			}
		}
		else if (validCategoryType == CategoryType.ImageAreaOverride)
		{
			if (validWidth == 0)
				WidthTextBox.Text = string.Empty;
			if (category.Width != validWidth)
			{
				category.Width = validWidth;
				optionsChanged = true;
			}

			if (validHeight == 0)
				HeightTextBox.Text = string.Empty;
			if (category.Height != validHeight)
			{
				category.Height = validHeight;
				optionsChanged = true;
			}
		}
	}

	protected override void Undo()
	{
		ClearErrors(CategoryCodeError, CategoryTitleError, WidthError, HeightError);
	}

	protected override void ValidatePage()
	{
		ClearErrors();

		validCode = CategoryCodeTextBox.Text.Trim();
		ValidateFieldNotBlank(validCode, CategoryCodeError, Resources.Text.ErrorCategoryCodeRequired);

		// Make sure the code does not contain any disallowed characters.
		ValidateFieldIsValidCategoryCode(validCode, CategoryCodeError, Resources.Text.ErrorIdContainsInvalidCharacters);
		if (!fieldValid)
			return;

		if (fieldValid)
		{
			bool nameInUse = Account.CategoryCodeInUse(categoryId, validCode);
			ValidateFieldCondition(!nameInUse, CategoryCodeError, Resources.Text.ErrorCategoryCodeInUse);
		}

		validTitle = CategoryTitleTextBox.Text.Trim();
		ValidateFieldNotBlank(validTitle, CategoryTitleError, Resources.Text.ErrorCategoryTitleRequired);
		if (!pageValid)
			return;

		validCategoryType = (CategoryType)int.Parse(CategoryTypeDropDownList.SelectedValue);

		if (validCategoryType == CategoryType.DirectoryGroup)
		{
			validPosition = ValidateFieldInRange(PositionTextBox, 1, 10000, PositionError);
			if (!pageValid)
				return;
		}
		else if (validCategoryType == CategoryType.ImageAreaOverride)
		{
			validWidth = ValidateFieldInRange(WidthTextBox, 0, 1600, WidthError, true);
			validHeight = ValidateFieldInRange(HeightTextBox, 0, 1600, HeightError, true);
			if (!pageValid)
				return;
		}
	}
}
