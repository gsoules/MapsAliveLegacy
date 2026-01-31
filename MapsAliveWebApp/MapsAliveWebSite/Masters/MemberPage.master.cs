// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using Telerik.Web.UI;

public partial class Masters_MemberPage : Navigation
{
	private bool allowPageCaching;

	protected void Page_Init(object sender, EventArgs e)
	{
		FormId = this.ClientID + ClientIDSeparator;
		FormContentId = this.ContentBody.ClientID + ClientIDSeparator;
		MasterTopMenu = this.TopMenu;
		MasterPageHeader = this.PageHeaderControl;
		MasterPostId = this.PostId;
		MasterStatusBox = this.StatusBox;
		MasterSpecialMessage = this.SpecialMessage;
		MasterSpecialMessagePanel = this.SpecialMessagePanel;
		MasterTourNavigator = this.TourNavigator;
		MasterAllToursPanel = this.AllToursPanel;
		MasterCurrentTourPanel = this.CurrentTourPanel;
		MasterScriptManager = this.ScriptManager;
	}

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!allowPageCaching)
            Utility.PreventPageCaching(Response);

        Utility.RegisterMemberPageJavaScript(Page);

        string nagMessage = MapsAliveState.Account.NagMessage;
        bool nagableAction = ActionId == MemberPageActionId.TourManager || ActionId == MemberPageActionId.TourExplorer || ActionId == MemberPageActionId.Profile;

        if (nagMessage != string.Empty && nagableAction)
        {
            NagPanel.Visible = true;
            Nag.Message = nagMessage;
        }

        // Show the controls to edit/add a hotspot and go to the Map Editor.
        Tour tour = MapsAliveState.SelectedTourOrNull;
        if (tour != null && tour.TourPageCount >= 1)
        {
            TourPage tourPage = tour.SelectedTourPage;
            if (tourPage != null)
            {
                TourPanelIcons.Visible = !tourPage.IsGallery && !tourPage.IsDataSheet;
                TourView tourView = tour.SelectedTourView;
                EditHotspotIcon.Visible = tourView != null;
            }
        }
    }

    public void AllowPageCaching()
	{
		allowPageCaching = true;
	}

	public string EmitAnalyticsScriptForTourBuilder()
	{
		bool emitAnalytics = false;

		if (ActionId == MemberPageActionId.BuyReceipt)
		{
			bool viewPurchaseHistory = Request.QueryString["review"] == "1";
			
			if (!viewPurchaseHistory)
			{
				Account account = MapsAliveState.Account;
				DataTable orderTable = MapsAliveDatabase.LoadDataTable("sp_Order_GetSummaryByAccountId", "@AccountId", account.Id);
				if (orderTable.Rows.Count == 1)
				{
					if (MapsAliveState.Retrieve(MapsAliveObjectType.AnalyticsBuy) == null)
					{
						emitAnalytics = true;
						MapsAliveState.Persist(MapsAliveObjectType.AnalyticsBuy, true);
					}
				}
			}
		}
		else if (ActionId == MemberPageActionId.Welcome)
		{
			if (Request.QueryString["signup"] == "1")
			{
				if (MapsAliveState.Retrieve(MapsAliveObjectType.AnalyticsSignup) == null)
				{
					emitAnalytics = true;
					MapsAliveState.Persist(MapsAliveObjectType.AnalyticsSignup, true);
				}
			}
		}

		if (emitAnalytics)
		{
			return Utility.EmitAnalyticsScript(true);
		}
		else
		{
			return string.Empty;
		}
	}
}
