// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.Security;
using System.Web.UI.WebControls;

public partial class Controls_SignUp : System.Web.UI.UserControl
{
	private string buttonName;
	private string errorMessage;
	private const bool isNewAccount = true;
	private MembershipUser user;
	
	protected void Page_Load(object sender, EventArgs e)
	{
		if (IsPostBack)
		{
			Password.Attributes.Add("value", Password.Text);
			ConfirmPassword.Attributes.Add("value", ConfirmPassword.Text);
		}
		else
		{
			ContactName.Focus();
		}
	}

	public string ButtonName
	{
		get { return buttonName; }
		set { buttonName = value; }
	}

	public string ErrorMessage
	{
		get { return errorMessage; }
        set
        {
            errorMessage = value;
            ShowErrors(true);
        }
	}

	public Account NewAccount
	{
		get
		{
			Account account = null;
			if (user != null)
			{
				account = new Account((Guid)user.ProviderUserKey);
				MapsAliveState.Persist(MapsAliveObjectType.Account, account);
			}
			return account;
		}
	}

	public bool CreateAccount(AccountType accountType, AccountPlan accountPlan, int days, int hotspotLimit)
	{
		bool accountCreated = false;

		bool validAccountInfo = MapsAliveMembershipProvider.ValidAccountInfo(
			Email.Text,
			Password.Text,
			ConfirmPassword.Text,
			Email.Text,
			ContactName.Text,
			isNewAccount,
			out errorMessage);

		if (validAccountInfo)
		{
			user = MapsAliveMembershipProvider.CreateAccount(
				Email.Text.Trim(),
				Password.Text,
				ConfirmPassword.Text,
				ContactName.Text.Trim(),
				accountType,
				accountPlan,
				days,
				hotspotLimit,
				OkToSendEmail.Checked,
				out errorMessage);

			accountCreated = user != null;
		}

		ShowErrors(!accountCreated);

		return accountCreated;
	}

	public void DeleteAccount()
	{
		const bool deleteOrders = true;
		const bool deleteResources = true;
		const bool deleteUser = true;
		Account.PurgeAccount(Email.Text, deleteOrders, deleteResources, deleteUser);
	}

	public string GetEmail()
	{
		return Email.Text.Trim();
	}

	public void SendWelcomeEmail(AccountType accountType)
	{
		// The exiration date is 1 month or year from yesterday because today is the first day of membership.
		int days = (accountType == AccountType.Trial ? 30 : 365) - 1;
		DateTime expiryDate = DateTime.Now.AddDays(days);

		// Retrieve the newly created account directly from cache because the new user is
		// not logged in yet and therefore calling MapsAliveState.Account would return null.
		Account account = (Account)MapsAliveState.Retrieve(MapsAliveObjectType.Account);
		System.Diagnostics.Debug.Assert(account != null, "Newly created account is null");

		string emailMsg = string.Format(Utility.EmailPlainTextForRegistration,
			ContactName.Text, accountType.ToString(), expiryDate.ToLongDateString(), Email.Text, Password.Text, account.Id);
		Utility.SendEmailToCustomer(Email.Text, Resources.Text.NewRegistrationEmailSubject, emailMsg);
	}

	public bool ValidateAccountInfo()
	{
		bool valid = MapsAliveMembershipProvider.ValidAccountInfo(
			null,
			Password.Text,
			ConfirmPassword.Text,
			Email.Text,
			ContactName.Text,
			isNewAccount,
			out errorMessage);

		ShowErrors(!valid);
		
		return valid;
	}

	private void ShowErrors(bool show)
	{
		ErrorPanel.Visible = show;
		ErrorText.Text = show ? errorMessage : string.Empty;
	}
}
