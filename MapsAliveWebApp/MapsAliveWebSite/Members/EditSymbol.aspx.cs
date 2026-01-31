// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Drawing;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class Members_EditSymbol : ImageUploadPage
{
	private bool addNew;
	private int symbolId;
	string validSymbolName;

	protected override void InitControls(bool undo)
	{
		if (badFileName)
			return;

		if (addNew || symbol == null)
		{
			ImageDiv.Visible = false;
			SetPageTitle(TourResource.GetTitleForAddPage(TourResourceType.Symbol));
			if (symbol == null)
				return;
		}
		else
		{
			AddChangeDetection(SymbolNameTextBox);
			
			ImageDiv.Visible = true;
			SetPageTitle(TourResource.GetTitleForEditPage(TourResourceType.Symbol));
			
			int width = symbol.Size.Width;
			int height = symbol.Size.Height;
			int dimension = Math.Max(symbol.Size.Width, symbol.Size.Height);

			if (BrowserIsIE6)
			{
				// Deal with IE 6's inability to display the transparent portion of PNG images.
				ImageElement.ImageUrl = "../Images/Blank.gif";

				ImageElement.Style.Value =
					string.Format("width:{0}px;height:{1}px;filter:progid:DXImageTransform.Microsoft.AlphaImageLoader(src='SymbolRenderer.ashx?id={2}&width={3}', sizingMethod='scale')",
					width,
					height,
					symbol.Id,
					dimension);
			}
			else
			{
				ImageElement.ImageUrl = string.Format("SymbolRenderer.ashx?id={0}&width={1}", symbol.Id, dimension);
			}
		}

		FileName.Text = string.Format("{0} ({1:n0} bytes)", symbol.FileNameOriginal, symbol.Length);
		
		MemberPage.InitShowUsageControl(ShowUsageControl, symbol);

		if (!undo && IsPostBack)
			return;

		SymbolNameTextBox.Text = symbol.Name;
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetActionId(MemberPageActionId.EditSymbol);
		GetSelectedTourOrNone();
		ParseQueryString();

		if (symbol != null)
		{
			// Save a copy of the unedited symbol.
			resourceBeforeEdit = symbol.Clone();
		}

		if (!IsPostBack)
			ProgressMonitor.ShowUploadFileProgress(ProgressArea);
	}

	private void ParseQueryString()
	{
		string id = Request.QueryString["id"];

		if (IsPostBack || IsReturnToTourBuilder)
		{
			symbolId = account.LastResourceId(TourResourceType.Symbol);
			
			//Determine what the page needs to do.
			if (symbolId == 0)
			{
				if (IsPostBack)
				{
					// The user just uploaded a new symbol.
					return;
				}
				else
				{
					// The user is returning to the new-symbol page after leaving the Tour Builder.
					id = "new";
				}
			}
			else
			{
				// The user is returning to the edit-symbol page after leaving the Tour Builder.
				symbol = new Symbol(symbolId);
				return;
			}
		}

		if (id == "new")
		{
			addNew = true;
			symbol = new Symbol(MapsAliveState.Account);
			account.SetLastResourceId(TourResourceType.Symbol, 0);
		}
		else
		{
			// The user wants to edit an existing symbol.
			int.TryParse(id, out symbolId);
			if (symbolId != 0)
				symbol = new Symbol(symbolId);
		}

		if (symbol != null)
			symbolId = symbol.Id;

		if ((symbolId == 0 && !addNew) || (symbol != null && symbol.AccountId != Utility.AccountId))
		{
			// There was no Id on the query string or it was not a valid Id.
			Server.Transfer(MemberPageAction.ActionPageTarget(MemberPageActionId.SymbolExplorer));
		}
		else
		{
			account.SetLastResourceId(TourResourceType.Symbol, symbolId);
		}
	}

	protected override void PagePreRender()
	{
		base.PagePreRender();

		if (badFileName)
			SetPageError(badFileNameMessage);
	}

	protected override void PerformUpdate()
	{
		if (symbol == null)
			symbol = new Symbol(symbolId);

		symbol.Name = validSymbolName;
		symbol.UpdateResource(resourceBeforeEdit);

        if (tour != null)
        {
            foreach (TourPage tourPage in tour.TourPages)
                tourPage.MapMarkerChanged();
        }
    }

    protected void AddChangeDetection(TextBox textBox, string script)
	{
		base.AddChangeDetection(textBox);
		textBox.Attributes.Add("onchange", script);
	}

	protected override void ImageUploaded(string fileName, Size size, Byte[] bytes)
	{
		symbolId = account.LastResourceId(TourResourceType.Symbol);

		if (symbolId == 0)
		{
			// This is a new symbol.
			symbol = new Symbol(MapsAliveState.Account);
			symbol.Name = validSymbolName;
			symbol.ImageUploaded(fileName, size, bytes);
			symbol.InsertIntoDatabase(account.Id);
			account.SetLastResourceId(TourResourceType.Symbol, symbol.Id);
		}
		else
		{
			// The symbol image is being updated.
			symbol = Account.GetCachedSymbol(symbolId);
			symbol.ImageUploaded(fileName, size, bytes);
			PerformUpdate();
		}

		symbolId = symbol.Id;
	}

	protected override void ImportFromUploadedFile()
	{
		ValidatePage();
		if (!pageValid)
			return;

		ImportImageFromUploadedFile(new Size(1200, 1200), false);
	}

	protected override void Undo()
	{
		ClearErrors(SymbolNameError);
	}

	protected override void ValidatePage()
	{
		ClearErrors();

		validSymbolName = SymbolNameTextBox.Text.Trim();
		ValidateFieldNotBlank(validSymbolName, SymbolNameError, Resources.Text.ErrorSymbolNameRequired);

		if (fieldValid)
		{
			bool nameInUse = Symbol.NameInUse(TourResourceType.Symbol, symbolId, validSymbolName, account.Id);
			ValidateFieldCondition(!nameInUse, SymbolNameError, Resources.Text.ErrorSymbolNameInUse);
		}
	}
}
