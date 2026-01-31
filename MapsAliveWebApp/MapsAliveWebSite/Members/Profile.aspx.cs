// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;

public partial class Members_Profile : MemberPage
{
	private bool impersonatingUser;
	private int orderCount;
	private decimal validCredit;
	private int validDays;
	private int validDiscount;
	private int validHotspotLimit;
	private decimal validPayment;
	
	protected override void EmitJavaScript()
	{
		string loadingScript = string.Empty;
		string loadedScript = "maTogglePanelBar('AllToursListHeader', 'AllToursList');";
		EmitJavaScript(loadingScript, loadedScript);
	}

	private void GetSessionInfo()
	{
		MapsAliveDataRow row = MapsAliveDatabase.LoadDataRow("sp_Account_GetSessionInfo", "@AccountId", account.Id);
		string info = "No session info available";
		if (row != null)
		{
			string sessionIpAddress = row.StringValue("SessionIpAddress");
			if (sessionIpAddress != string.Empty)
			{
				DateTime sessionTime = row.DateTimeValue("SessionTime");
				TimeSpan timeSpan = DateTime.Now - sessionTime;
				string browser = row.StringValue("SessionBrowser");
				int minutes = timeSpan.Minutes;
				info = string.Format("Session started at {0} using {1} from {2}", sessionTime, browser, sessionIpAddress);
			}
		}
		SessionTextBox.Text = info;
	}

	private void HideRow(HtmlTableRow row)
	{
		row.Style.Add(HtmlTextWriterStyle.Display, "none");
	}

	protected override void InitControls(bool undo)
	{
		InitSummaryControls();
		InitProfileControls();

		if (!undo && IsPostBack)
			return;

		if (impersonatingUser)
			InitImpersonationControls();
	}

	private void InitSummaryControls()
	{
		AccountType.Text = account.MembershipDescription;
		Tours.Text = account.TourCount.ToString();
		AccountNumber.Text = account.Id.ToString();

		HideRow(NewsletterRow);
		
		if (account.CreditAmount > 0.0M)
		{
			CreditBalance.Text = account.CreditAmountString;
		}
		else
		{
			HideRow(CreditBalanceRow);
		}

		if (account.IsElite)
		{
			DaysRemaining.Text = "Unlimited";
			ExpiryDate.Text = string.Empty;
		}
		else
		{
			int daysRemaining = account.DaysActual - 1;
			DaysRemaining.Text = string.Format("{0:n0}", account.DaysRemaining);
			ExpiryDate.Text = string.Format(" ({0})", account.ExpiryDateString);
		}

		int hotspotsUsed = account.CountHotspotsInUse();
		int hotspotsBorrowed = account.CountHotspotsBorrowed();

		if (hotspotsBorrowed > 0)
		{
			SlidesBorrowed.Text = string.Format("{0:n0}", hotspotsBorrowed);
		}
		else
		{
			HideRow(HotspotsBorrowedRow);
		}

		HotspotLimit.Text = string.Format("{0:n0}", account.HotspotLimit);

		SlidesUsed.Text = string.Format("{0:n0}", hotspotsUsed);

		int hotspotsAvailable = account.HotspotLimit - hotspotsUsed;
		if (hotspotsAvailable < 0)
			hotspotsAvailable = 0;
		SlidesAvailable.Text = string.Format("{0:n0}", hotspotsAvailable);
	}

	private void InitProfileControls()
	{
        ContactNameTextBox.Text = account.ContactName;
		AddChangeDetection(ContactNameTextBox);

		EmailTextBox.Text = account.Email;
		if (impersonatingUser)
		{
			// Don't let a sys admin change a user's email while impersonating them.
			// Doing to puts the account into an inconsistent state with old and new email addresses.
			EmailTextBox.Enabled = false;
		}
		else
		{
			AddChangeDetection(EmailTextBox);
		}

		// Only put change detection on the new password. If they don't provide it,
		// we don't care about the old or confirm password.
		AddChangeDetection(NewPasswordTextBox);

		AddChangeDetection(NewsletterCheckbox);
		NewsletterCheckbox.Checked = account.SendNewsletter;
	}

	private void InitImpersonationControls()
	{
		StopImpersonationPanel.Visible = true;
		StopImpersonationButton.Text = "Stop Impersonating " + account.ContactName;
		
		AdminPanel.Visible = true;

		SetPageSpecialNotice("Impersonating " + Utility.AccountIdentification);

		MembershipUser user = Membership.GetUser(Utility.UserId);
        try
        {
            LabelUserPw.Text = user.GetPassword();
        }
        catch
        {
			LabelUserPw.Text = "ACCOUNT WAS LOCKED<br/>Unlocking now (refesh this page to verify)";
			user.UnlockUser();
		}
		DaysTextBox.Text = string.Format("{0}", account.DaysRemaining);
		AddChangeDetection(DaysTextBox);

		HotspotsTextBox.Text = account.HotspotLimit.ToString();
		AddChangeDetection(HotspotsTextBox);

		CreditTextBox.Text = account.CreditAmountString;
		AddChangeDetection(CreditTextBox);

		PaymentTextBox.Text = account.PaymentAmountString;
		AddChangeDetection(PaymentTextBox);

		DiscountTextBox.Text = account.DiscountPercent.ToString();
		AddChangeDetection(DiscountTextBox);

		AccountTypeList.SelectedValue = ((int)account.Type).ToString();
		AddChangeDetection(AccountTypeList);

		AccountPlanList.SelectedValue = ((int)account.Plan).ToString();
		AddChangeDetection(AccountPlanList);


		if (account.TourCount == 0)
		{
            ActivateToursRow.Visible = false;
			DeactivateToursRow.Visible = false;
			PurgeAccountRow.Visible = false;
		}
		
		if (orderCount > 0)
		{
			string orderCountText = string.Format("{0} order{1}", orderCount, Utility.Plural(orderCount));
            AccountOrders.Text = string.Format("This account has {0}", orderCountText);
		}

		GetSessionInfo();
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle(Resources.Text.AccountPageStatusTitle);
		SetActionId(MemberPageActionId.Profile);
		GetSelectedTourOrNone();

		impersonatingUser = Utility.ImitatingUser();
		orderCount = Account.OrderCount(account.Id);
        
        if (IsPostBack)
        {
            string eventTarget = Request.Form["__EVENTTARGET"];
            switch (eventTarget)
            {
                case "OnViewReports":
                    OnViewReports();
                    break;
                case "OnReturnToUsers":
                    OnReturnToUsers();
                    break;
                case "OnActivateTours":
                    OnActivateTours();
                    break;
                case "OnDeactivateTours":
                    OnDeactivateTours();
                    break;
                case "OnDeleteAccount":
                    OnDeleteAccount();
                    break;
                case "OnPurgeAccount":
                    OnPurgeAccount();
                    break;
            }
        }
        else if (Request.QueryString["newemail"] == "1")
        {
			SetPageMessage("Your email address has been updated");
        }
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Account;
	}

	protected override void Undo()
	{
		ClearErrors();
	}

	protected override void PerformUpdate()
	{
		AccountType accountType = (AccountType)(int.Parse(AccountTypeList.SelectedValue));
		AccountPlan accountPlan = (AccountPlan)(int.Parse(AccountPlanList.SelectedValue));
		bool accountChanged = false;
		
		if (impersonatingUser)
		{
			accountChanged = false;
			accountChanged = accountChanged || validDays != account.DaysActual;
			accountChanged = accountChanged || validHotspotLimit != account.HotspotLimit;
			accountChanged = accountChanged || validCredit != account.CreditAmount;
			accountChanged = accountChanged || validPayment != account.PaymentAmount;
			accountChanged = accountChanged || validDiscount != account.DiscountPercent;
			accountChanged = accountChanged || accountType != account.Type;
			accountChanged = accountChanged || accountPlan != account.Plan;

			if (accountChanged)
			{
				account.UpdateAccountLimits(
					validDays,
					validHotspotLimit,
					validCredit,
					validPayment,
					validDiscount,
					accountType,
					accountPlan);

				// Update the member page account object by reading it back from the database.
				MapsAliveState.Flush(MapsAliveObjectType.Account);
				account = MapsAliveState.Account;
			}

            MapsAliveState.Account.UpdateHotspotStatus();
        }

        string errorMessage;
		const bool oldPasswordRequired = true;

		bool emailChanged = EmailTextBox.Text != account.Email;

		bool profileChanged = emailChanged;
		profileChanged = profileChanged || OldPasswordTextBox.Text != string.Empty;
		profileChanged = profileChanged || NewPasswordTextBox.Text != string.Empty;
		profileChanged = profileChanged || ConfirmPasswordTextBox.Text != string.Empty;
		profileChanged = profileChanged || ContactNameTextBox.Text != account.ContactName;
		profileChanged = profileChanged || NewsletterCheckbox.Checked != account.SendNewsletter;

		if (profileChanged)
		{
			bool updated = MapsAliveMembershipProvider.UpdateAccount(
				account,
				EmailTextBox.Text.Trim(),
				oldPasswordRequired,
				OldPasswordTextBox.Text,
				NewPasswordTextBox.Text,
				ConfirmPasswordTextBox.Text,
				null,
				ContactNameTextBox.Text.Trim(),
				NewsletterCheckbox.Checked,
				out errorMessage
			);

			if (errorMessage != string.Empty)
			{
				SetPageError(errorMessage);
			}
			else
			{
				if (emailChanged && !impersonatingUser)
				{
					// Redirect back to this page because UpdateAccount created a new cookie for the authetication
					// ticket for the new user name. If we don't redirect, the new cookie won't get read and the
					// user will no longer be logged in. Note that the redirect triggers a ThreadAbortException
					// which is gracefully handled in MemberPage.Save(). We tried using TransferToMemberPage to
					// avoid the exception, but for some reason when we do that we end up logged out.
					Response.Redirect("Profile.aspx?newemail=1");
				}
				else
				{
					if (accountChanged)
						SetPageMessage("Account and Profile have been updated");
					else
						SetPageMessage("Your profile has been updated");
				}
			}
		}
		else
		{
			SetPageMessage(accountChanged ? "Account has been updated" : "No changes were made to your profile");
		}
	}

	protected override void ReadPageFields()
	{
		CreditTextBox.Text = string.Format("{0:c2}", validCredit);
		PaymentTextBox.Text = string.Format("{0:c2}", validPayment);
		DiscountTextBox.Text = validDiscount.ToString();
	}

	protected void OnActivateTours()
	{
		account.SetTourState(TourState.Active);
		ShowActivateStatus("activated");
	}

	protected void OnDeactivateTours()
	{
		account.SetTourState(TourState.Expired);
		ShowActivateStatus("deactivated");
	}

	protected void OnDeleteAccount()
	{
		const bool deleteOrders = true;
		const bool deleteResources = true;
		const bool deleteUser = true;
		string userName = Utility.UserName;

		if (Utility.UserHasAdminRole(userName))
		{
			SetPageError("Cannot delete an administrator");
			return;
		}

		bool deleted = Account.PurgeAccount(Utility.UserName, deleteOrders, deleteResources, deleteUser);
		
		if (deleted)
		{
			Utility.ImitateUser(null);
			string queryString = string.Format("?deleted={0}", userName);
			ReturnToUsersScreen(queryString);
		}
		else
		{
			SetPageError("Delete failed");
		}
	}

	protected void OnPurgeAccount()
	{
		const bool deleteOrders = false;
		const bool deleteResources = false;
		const bool deleteUser = false;
		bool purged = Account.PurgeAccount(Utility.UserName, deleteOrders, deleteResources, deleteUser);
		MapsAliveState.FlushSessionState();
		InitSummaryControls();
		SetPageMessage(purged ? "Account has been purged" : "Purge failed");
	}

	protected void OnReturnToUsers()
	{
		ReturnToUsersScreen(string.Empty);
	}

	protected void OnViewReports()
	{
		string targetPage = MemberPageAction.ActionPageTarget(MemberPageActionId.Reports);
		string queryString = string.Format("?accountId={0}&tours=1", account.Id);
		Response.Redirect(targetPage + queryString);
	}

	private void ReturnToUsersScreen(string queryString)
	{
		string targetPage = MemberPageAction.ActionPageTarget(MemberPageActionId.UserAccounts);
		Response.Redirect(targetPage + queryString);
	}

	private void ShowActivateStatus(string action)
	{
		SetPageMessage(string.Format("Tours for {0} have been {1}", account.ContactName, action));
	}

	protected override void ValidatePage()
	{
		if (!impersonatingUser)
			return;

		ClearErrors();
		
		validDays = ValidateFieldInRange(DaysTextBox, -2000, 2000, DaysError);
		if (!pageValid)
			return;
		
		validHotspotLimit = ValidateFieldInRange(HotspotsTextBox, 0, 100000, HotspotsError);
		if (!pageValid)
			return;

		const bool allowNegativeAmount = false;
		validCredit = ValidateMoney(CreditTextBox, allowNegativeAmount, CreditError);
		if (!pageValid)
			return;
		
		validPayment = ValidateMoney(PaymentTextBox, allowNegativeAmount, PaymentError);
		if (!pageValid)
			return;

		validDiscount = ValidateFieldInRange(DiscountTextBox, 0, 50, DiscountError);
	}

	private void ClearErrors()
	{
		ClearErrors(
			DaysError,
			CreditError,
			HotspotsError);
	}

	protected void OnStopImpersonation(object sender, EventArgs e)
	{
		Utility.ImitateUser(null);
		SetPageSpecialWarning(string.Empty);
		StopImpersonationPanel.Visible = false;
		ReturnToUsersScreen(string.Empty);
	}
}
