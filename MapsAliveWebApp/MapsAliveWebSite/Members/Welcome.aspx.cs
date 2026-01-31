// Copyright (C) 2003-2010 AvantLogic Corporation

public partial class Members_Welcome : MemberPage
{
	private bool firstTour;

	protected override void InitControls(bool undo)
	{
		Instructions.Text = AppContent.Topic(firstTour ? "Welcome1" : "Welcome2");
	}

	protected override void PageLoad()
	{
		firstTour = account.TourCount == 0;
		
		SetMasterPage(Master);
		SetPageTitle("Welcome to MapsAlive");
		SetPageReadOnly();
		SetActionId(MemberPageActionId.Welcome);
		GetSelectedTourOrNone();

        EnableStepByStepHelp();
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Account;
	}

    protected override bool SetStatus()
    {
        StatusBox.Clear();
        StatusBox.NextAction = "Create a new tour";
        StatusBox.SetStep(1, "Click [New > Tour] from the menu above.");
        return true;
    }

    private void EnableStepByStepHelp()
    {
        if (account.ShowStepByStepHelp)
            return;

        // Enable step-by-step help whenever the user comes to the Welcome screen.
        // This is also how the help gets enabled for a new account.
        account.ShowStepByStepHelp = true;

        account.UpdateAccountPreferences(
            account.SiteName,
            account.ShowSlideContentInLayoutPreview,
            account.ShowTourNavigatorExpanded,
            account.DisableTourAdvisor,
            account.ShowStepByStepHelp);
    }
}
