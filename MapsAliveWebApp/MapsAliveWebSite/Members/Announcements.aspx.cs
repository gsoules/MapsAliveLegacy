// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class Members_Announcements : MemberPage
{
	protected override void InitControls(bool undo)
	{
		Announcement.Text = AppContent.Topic("AnnouncementGeneralText");

		if (IsReturnToTourBuilder)
		{
			DontShowCheckBox.Checked = account.AnnouncementId >= App.AnnouncementId;
			AddChangeDetection(DontShowCheckBox);
		}
		else
		{
			OptionsPanel.Visible = true;
		}
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle("Announcements");
		SetPageReadOnly();
		SetActionId(MemberPageActionId.Announcements);
		GetSelectedTourOrNone();
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Account;
	}

	protected void OnDontShowChanged(object sender, EventArgs e)
	{
		account.ShowAnnouncements(!DontShowCheckBox.Checked);
	}
}
