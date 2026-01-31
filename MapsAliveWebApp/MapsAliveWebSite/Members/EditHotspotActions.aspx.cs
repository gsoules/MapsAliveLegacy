// Copyright (C) 2003-2010 AvantLogic Corporation
using System.Diagnostics;
using System.Web.UI.WebControls;

public partial class Members_EditHotspotActions : MemberPage
{
	protected override void EmitJavaScript()
	{
		string loadingScript = AssignClientVar("selectedMarkerId", tourView.MarkerId);
		string loadedScript =
			"maShowClickActionTarget(" + (int)tourView.MarkerClickAction + ");" +
			"maShowRolloverActionTarget('" + (int)tourView.MarkerRolloverAction + "');" +
			"maShowRolloutActionTarget(" + (int)tourView.MarkerRolloutAction + ");";

		if (!tourView.MarkerIsRoute)
			loadedScript += "maOnPreviewRollover(false);";

		EmitJavaScript(loadingScript, loadedScript);
	}

	protected override void InitControls(bool undo)
	{
		if (!undo && IsPostBack)
			return;

		if (tourView.MarkerIsRoute)
			MarkerPreviewPanel.Visible = false;

		int pageCount = tour.TourPages.Count;

		HotspotId.Text = tourView.SlideId;

		// Initialize the radio buttons
		AddChangeDetection(RadioButtonOnClick);
		AddChangeDetection(RadioButtonOnMouseover);
		AddChangeDetection(RadioButtonNever);
		
		if (tourView.ShowContentEvent == ShowContentEvent.OnClick)
			RadioButtonOnClick.Checked = true;
		else if (tourView.ShowContentEvent == ShowContentEvent.OnMouseover)
			RadioButtonOnMouseover.Checked = true;
		else
			RadioButtonNever.Checked = true;

		// Populate the click action dropdown list.
		ClickActionDropDownList.Items.Clear();
		ClickActionDropDownList.Items.Add(new ListItem("None", ((int)MarkerAction.None).ToString(), true));

		ListItem listItem;

		if (pageCount > 1)
		{
			listItem = new ListItem("Go to Page", ((int)MarkerAction.GotoPage).ToString());
			ClickActionDropDownList.Items.Add(listItem);
			
			// Populate the page drop down list.
			PageDropDownList.Items.Clear();
			foreach (TourPage page in tour.TourPages)
			{
				if (page.Id == tourView.TourPage.Id)
					continue;
				ListItem item = new ListItem(page.Name, page.Id.ToString());
				if (tourView.MarkerClickAction == MarkerAction.GotoPage)
					item.Selected = tourView.MarkerClickActionTarget == page.Id.ToString();
				PageDropDownList.Items.Add(item);
			}

			if (tourView.MarkerClickAction == MarkerAction.GotoPage)
			{
				AddChangeDetection(PageDropDownList);
			}
		}

		listItem = new ListItem("Link to URL", ((int)MarkerAction.LinkToUrl).ToString());
		ClickActionDropDownList.Items.Add(listItem);

		listItem = new ListItem("JavaScript", ((int)MarkerAction.CallJavascript).ToString());
		ClickActionDropDownList.Items.Add(listItem);

		if (tourView.MarkerClickAction == MarkerAction.LinkToUrl ||
			tourView.MarkerClickAction == MarkerAction.LinkToUrlNewWindow)
		{
			UrlTextBox.Text = tourView.MarkerClickActionTarget;
			PopupCheckBox.Checked = tourView.MarkerClickAction == MarkerAction.LinkToUrlNewWindow;
			AddChangeDetection(UrlTextBox);
			AddChangeDetection(PopupCheckBox);
		}

		// Since we don't show LinkToUrlNewWindow as a separate action, map it to LinkToUrl.
		// When we read the page, we use the popup checkbox to tell which link flavor was chosen.
		MarkerAction selectedAction;
		if (tourView.MarkerClickAction == MarkerAction.LinkToUrlNewWindow)
			selectedAction = MarkerAction.LinkToUrl;
		else
			selectedAction = tourView.MarkerClickAction;

		JS1TextBox.Enabled = account.IsPlusOrProPlan;
		if (tourView.MarkerClickAction == MarkerAction.CallJavascript)
		{
			JS1TextBox.Text = tourView.MarkerClickActionTarget;
			if (account.IsPlusOrProPlan)
				AddChangeDetection(JS1TextBox);
		}
		
		ClickActionDropDownList.SelectedValue = ((int)selectedAction).ToString();

		// Set the mouseover options.
		RolloverActionDropDownList.SelectedValue = ((int)tourView.MarkerRolloverAction).ToString();
		JS2TextBox.Enabled = account.IsPlusOrProPlan;
		if (tourView.MarkerRolloverAction == MarkerAction.CallJavascript)
		{
			JS2TextBox.Text = tourView.MarkerRolloverActionTarget;
			if (account.IsPlusOrProPlan)
				AddChangeDetection(JS2TextBox);
		}
		
		// Set the mouseout options.
		RolloutActionDropDownList.SelectedValue = ((int)tourView.MarkerRolloutAction).ToString();
		JS3TextBox.Enabled = account.IsPlusOrProPlan;
		if (tourView.MarkerRolloutAction == MarkerAction.CallJavascript)
		{
			JS3TextBox.Text = tourView.MarkerRolloutActionTarget;
			if (account.IsPlusOrProPlan)
				AddChangeDetection(JS3TextBox);
		}

		TouchOptionsPanel.Visible = true;
		AddChangeDetection(RadioButtonTouchMouseOver);
		AddChangeDetection(RadioButtonTouchClick);

		if (tourView.TouchPerformsClickAction)
			RadioButtonTouchClick.Checked = true;
		else
			RadioButtonTouchMouseOver.Checked = true;
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle(Resources.Text.MarkerPageTitle);
		SetActionId(MemberPageActionId.EditHotspotActions);
		GetSelectedTourView();
	}

	protected override void PerformUpdate()
	{
		tourView.UpdateDatabase();
	}
	
	protected override void ReadPageFields()
	{
		if (RadioButtonOnMouseover.Checked)
			tourView.ShowContentEvent = ShowContentEvent.OnMouseover;
		else if (RadioButtonOnClick.Checked)
			tourView.ShowContentEvent = ShowContentEvent.OnClick;
		else
			tourView.ShowContentEvent = ShowContentEvent.Never;
		
		MarkerAction clickAction = (MarkerAction)int.Parse(ClickActionDropDownList.SelectedValue);
		switch (clickAction)
		{
			case MarkerAction.None:
				tourView.MarkerClickAction = MarkerAction.None;
				tourView.MarkerClickActionTarget = string.Empty;
				break;

			case MarkerAction.GotoPage:
				tourView.MarkerClickAction = MarkerAction.GotoPage;
				tourView.MarkerClickActionTarget = PageDropDownList.SelectedValue;
				break;

			case MarkerAction.LinkToUrl:
				if (PopupCheckBox.Checked)
					tourView.MarkerClickAction = MarkerAction.LinkToUrlNewWindow;
				else
					tourView.MarkerClickAction = MarkerAction.LinkToUrl;
				tourView.MarkerClickActionTarget = UrlTextBox.Text;
				break;

			case MarkerAction.CallJavascript:
				tourView.MarkerClickAction = MarkerAction.CallJavascript;
				tourView.MarkerClickActionTarget = JS1TextBox.Text;
				break;

			default:
				Debug.Fail("Unexpected click action " + clickAction);
				break;
		}

		MarkerAction rolloverAction = (MarkerAction)int.Parse(RolloverActionDropDownList.SelectedValue);
		switch (rolloverAction)
		{
			case MarkerAction.CallJavascript:
				tourView.MarkerRolloverAction = MarkerAction.CallJavascript;
				tourView.MarkerRolloverActionTarget = JS2TextBox.Text;
				break;

			case MarkerAction.None:
				tourView.MarkerRolloverAction = MarkerAction.None;
				tourView.MarkerRolloverActionTarget = string.Empty;
				break;

			default:
				Debug.Fail("Unexpected rollover action " + rolloverAction);
				break;
		}

		MarkerAction rolloutAction = (MarkerAction)int.Parse(RolloutActionDropDownList.SelectedValue);
		switch (rolloutAction)
		{
			case MarkerAction.CallJavascript:
				tourView.MarkerRolloutAction = MarkerAction.CallJavascript;
				tourView.MarkerRolloutActionTarget = JS3TextBox.Text;
				break;

			case MarkerAction.None:
				tourView.MarkerRolloutAction = MarkerAction.None;
				tourView.MarkerRolloutActionTarget = string.Empty;
				break;

			default:
				Debug.Fail("Unexpected rollout action " + rolloutAction);
				break;
		}

		tourView.TouchPerformsClickAction = RadioButtonTouchClick.Checked;
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}
}
