// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class Members_HotspotLimit : MemberPage
{
	private HotspotLimitStatus status;

	protected override void InitControls(bool undo)
	{
		if (IsPostBack)
			return;

		string message = string.Empty;
		string suggestion = string.Empty;

		if (status == HotspotLimitStatus.OverLimit && !tour.ExceedsSlideLimit)
		{
			AddHotspotAnywayPanel.Visible = false;
			SetPageSpecialWarning(account.HotspotLimitMessage(HotspotLimitWarningContext.AddNewHotspotAccountOverLimit));
		}
		else
		{
			string script = string.Format("maOnEventSaveAndTransfer('/Members/{0}?ok=1');", MemberPageAction.ActionPageTarget(MemberPageActionId.AddHotspot));
			AddHotspotAtLimitControl.OnClickJavaScript = script;
			AddHotspotAtLimitControl.Important = true;
			SetPageSpecialNotice(account.HotspotLimitMessage(tour.ExceedsSlideLimit ? HotspotLimitWarningContext.AddNewHotspotTourOverLimit : HotspotLimitWarningContext.AddNewHotspotAccountAtLimit));
		}
		
		if (account.CreditAmount > 0.0M)
		{
			suggestion = string.Format(
				"<div style='width:400px;margin-top:16px;margin-left:60px;font-weight:bold;color:#555555;padding:12px;border:solid 1px green;background-color:#ffffd5;'>" +
				"Your account has a credit balance of {0} that you can use to purchase more hotspots. The credit will be applied automatically to your next purchase.<br/>",
				account.CreditAmountString);
			suggestion += "</div>";
		}

		Suggestion.Text = string.Format("{0}<br/>{1}", suggestion, AppContent.Topic("MakeHotspotsAvailableSuggestions"));
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageReadOnly();
		SetActionId(MemberPageActionId.HotspotLimitReached);
		GetSelectedTour();

		status = account.HotspotLimitStatus;
		SetPageTitle(status == HotspotLimitStatus.OverLimit ? "Hotspot Limit Exceeded" : "Hotspot Limit Reached");
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Account;
	}
}
