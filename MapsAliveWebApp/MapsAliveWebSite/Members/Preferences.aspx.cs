// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;

public partial class Members_Preferences : MemberPage
{
	protected override void InitControls(bool undo)
	{
		SiteNameTextBox.Text = account.SiteName;
		AddChangeDetection(SiteNameTextBox);

		PreviewSlideContentCheckBox.Checked = account.ShowSlideContentInLayoutPreview;
		AddChangeDetection(PreviewSlideContentCheckBox);

		ShowTourNavigatorExpandedCheckBox.Checked = account.ShowTourNavigatorExpanded;
		AddChangeDetection(ShowTourNavigatorExpandedCheckBox);

		DisableTourAdvisorCheckBox.Checked = account.DisableTourAdvisor;
		AddChangeDetection(DisableTourAdvisorCheckBox);

	    ShowStepByStepHelpCheckBox.Checked = account.ShowStepByStepHelp;
	    AddChangeDetection(ShowStepByStepHelpCheckBox);

		InitResourceControls();
	}

	protected void InitResourceControls()
	{
		FontStyleComboBox.ResourceType = TourResourceType.FontStyle;
		FontStyleComboBox.SelectedResourceId = account.DefaultResourceId(TourResourceType.FontStyle);
		FontStyleComboBox.OnClientSelectedIndexChangedScript = "maChangeDetected();";

		MarkerComboBox.ResourceType = TourResourceType.Marker;
		MarkerComboBox.SelectedResourceId = account.DefaultResourceId(TourResourceType.Marker);
		MarkerComboBox.OnClientSelectedIndexChangedScript = "maChangeDetected();";

		MarkerStyleComboBox.ResourceType = TourResourceType.MarkerStyle;
		MarkerStyleComboBox.SelectedResourceId = account.DefaultResourceId(TourResourceType.MarkerStyle);
		MarkerStyleComboBox.OnClientSelectedIndexChangedScript = "maChangeDetected();";

		TooltipStyleComboBox.ResourceType = TourResourceType.TooltipStyle;
		TooltipStyleComboBox.SelectedResourceId = account.DefaultResourceId(TourResourceType.TooltipStyle);
		TooltipStyleComboBox.OnClientSelectedIndexChangedScript = "maChangeDetected();";

		ColorSchemeComboBox.ResourceType = TourResourceType.TourStyle;
		ColorSchemeComboBox.SelectedResourceId = account.DefaultResourceId(TourResourceType.TourStyle);
		ColorSchemeComboBox.OnClientSelectedIndexChangedScript = "maChangeDetected();";

		SymbolComboBox.ResourceType = TourResourceType.Symbol;
		SymbolComboBox.SelectedResourceId = account.DefaultResourceId(TourResourceType.Symbol);
		SymbolComboBox.OnClientSelectedIndexChangedScript = "maChangeDetected();";

		ImportSystemResourceControl.OnClickActionId = MemberPageActionId.Preferences;
		ImportSystemResourceControl.QueryString = "?sys=1";
		ImportSystemResourceControl.WarningMessage = "<p>Import Mapsalive resources?</p><p>Import will add Font Styles, Markers, Marker Styles, Symbols, Tooltip Styles, and Color Schemes to your account.</p>";
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle("Preferences");
		SetActionId(MemberPageActionId.Preferences);
		GetSelectedTourOrNone();

		if (!IsPostBack)
		{
			// Only test for import actions when the page first posts. Otherwise we'll end
			// up reading the query string when the Save button is pressed.
			if (Request.QueryString["sys"] == "1")
			{
				bool copied = TourResourceManager.CopyAllSystemResourcesToAccount(account, false);
				if (copied)
					SetPageMessage("MapsAlive resources have been imported");
				else
					SetPageError("An error occurred during import. MapsAlive support has been notified.");
			}
		}
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Account;
	}

	protected override void PerformUpdate()
	{
		account.UpdateAccountPreferences(
			SiteNameTextBox.Text,
			PreviewSlideContentCheckBox.Checked,
			ShowTourNavigatorExpandedCheckBox.Checked,
			DisableTourAdvisorCheckBox.Checked,
			ShowStepByStepHelpCheckBox.Checked);

		account.DefaultFontStyleId = FontStyleComboBox.SelectedResourceId;
		account.DefaultMarkerId = MarkerComboBox.SelectedResourceId;
		account.DefaultMarkerStyleId = MarkerStyleComboBox.SelectedResourceId;
		account.DefaultTooltipStyleId = TooltipStyleComboBox.SelectedResourceId;
		account.DefaultColorSchemeId = ColorSchemeComboBox.SelectedResourceId;
		account.DefaultSymbolId = SymbolComboBox.SelectedResourceId;

		account.UpdateAccountResourceSettings();
	}
}
