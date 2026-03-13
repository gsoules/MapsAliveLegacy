// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Diagnostics;
using System.Web;
using System.Web.Security;

public class MapsAliveMembershipProvider : SqlMembershipProvider
{
	public MapsAliveMembershipProvider()
    {
    }

	public static MembershipUser CreateAccount(
		string email, 
		string password, 
		string confirmPassword, 
		string contactName,
		AccountType accountType,
		AccountPlan accountPlan,
		int days,
		int hotspotLimit,
		bool sendNewsletter, 
		out string errorMessage)
	{
		const bool newAccount = true;

		if (!ValidAccountInfo(null, password, confirmPassword, email, contactName, newAccount, out errorMessage))
			return null;

		MembershipUser user = CreateNewAccount(
			email,
			password,
			email,
			contactName,
			accountType,
			accountPlan,
			days,
			hotspotLimit,
			sendNewsletter,
			ref errorMessage);

		if (user != null)
		{
			FormsAuthentication.SetAuthCookie(email, false);
		}

		return user;
	}

	private static MembershipUser CreateNewAccount(
		string userName,
		string password,
		string email,
		string contactName,
		AccountType accountType,
		AccountPlan accountPlan,
		int days,
		int hotspotLimit,
		bool sendNewsletter,
		ref string errorMessage)
	{
		MembershipCreateStatus status;
		MembershipUser newUser = MapsAliveMembershipProvider.CreateMapsAliveUser(
			userName,
			password,
			email,
			contactName,
			accountType,
			accountPlan,
			days,
			hotspotLimit,
			sendNewsletter,
			0,
			out status);

		switch (status)
		{
			case MembershipCreateStatus.Success:
				errorMessage = "Success";
				break;

			case MembershipCreateStatus.InvalidEmail:
				errorMessage = "The email address is not valid";
				break;

			case MembershipCreateStatus.InvalidPassword:
				errorMessage = "The password is not valid";
				break;

			case MembershipCreateStatus.InvalidUserName:
				errorMessage = "The Id is not valid";
				break;

			case MembershipCreateStatus.DuplicateUserName:
				errorMessage = "The email address is already in use";
				break;

			case MembershipCreateStatus.ProviderError:
				errorMessage = "We're sorry. A problem occurred while creating your account. Please try again. If the problem continues, please contact suppport@mapsalive.com.";
				bool deleted = Account.PurgeAccount(userName, true, true, true);
				Utility.ReportError("Unable to create account", string.Format("Email:{0}\nPW:{1}\nContact:{2}\nDeleted:{3}", userName, password, contactName, deleted.ToString()));
				newUser = null;
				break;

			default:
				errorMessage = status.ToString();
				break;
		}

		return newUser;
	}

	public static MembershipUser CreateMapsAliveUser(
		string userName,
		string password,
		string email,
		string contactName,
		AccountType accountType,
		AccountPlan accountPlan,
		int days,
		int hotspotLimit,
		bool sendNewsletter,
		int parentAccountId,
		out MembershipCreateStatus status)
	{
		status = MembershipCreateStatus.Success;
		MembershipUser newUser = null;

		try
		{
			newUser = Membership.CreateUser(userName, password, email);
			
			string refererr = MapsAliveState.Referrer;
			
			MapsAliveDatabase.ExecuteStoredProcedure("sp_Account_UpdateNewAccount",
				"@UserId", newUser.ProviderUserKey,
				"@ContactName", contactName.Trim(),
				"@Type", (int)accountType,
				"@PlanId", (int)accountPlan,
				"@Referrer", refererr == null ? string.Empty : refererr,
				"@Days", days,
				"@SlideLimit", hotspotLimit,
				"@SubAccountLimit", 0,
				"@SendNewsletter", sendNewsletter,
				"@ParentAccountId", parentAccountId);

			Account account = new Account((Guid)newUser.ProviderUserKey);
			MapsAliveState.Persist(MapsAliveObjectType.Account, account);

			account.RecordSessionInfo();
			
			bool success = TourResourceManager.CopyAllSystemResourcesToAccount(account, true);
			if (!success)
				status = MembershipCreateStatus.ProviderError;
		}
		catch (MembershipCreateUserException ex)
		{
			status = ex.StatusCode;
		}

		return newUser;
	}

    public override MembershipUser CreateUser(
		string username,
		string password,
		string email,
		string passwordQuestion,
		string passwordAnswer,
		bool isApproved,
		object providerUserKey,
		out MembershipCreateStatus status)
    {
		MembershipUser membershipUser = base.CreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status);

		if (membershipUser != null)
		{
			try
			{
				string ipAddress = HttpContext.Current.Request.UserHostAddress;

				MapsAliveDatabase.ExecuteStoredProcedure("sp_Account_CreateAccount",
					"@UserId", membershipUser.ProviderUserKey,
					"@Type", (int)AccountType.Trial,
					"@IpAddress", ipAddress,
					"@AnnouncementId", App.AnnouncementId,
					"@Days", 0,
					"@SlideLimit", 0);
			}
			catch
			{
				status = MembershipCreateStatus.ProviderError;	
				return null;
			}

			// By default, new accounts get a member role, but the first account becomes an administrator.
			int members = MapsAliveDatabase.GetCount("sp_Account_GetCount");
			Roles.AddUserToRole(username, members == 1 ? "administrator" : "member");
		}

		return membershipUser;
	}

	public static bool DeleteAccount(string email)
	{
		bool userDeleted = Membership.DeleteUser(email);
		Debug.Assert(userDeleted, "Failed to delete user " + email);
		return userDeleted;
	}

	public static bool UpdateAccount(
		Account account,
		string userName,
		bool oldPasswordRequired,
		string oldPassword,
		string newPassword,
		string confirmPassword,
		string email,
		string contactName,
		bool sendNewsletter,
		out string
		errorMessage)
	{
		MembershipUser user = Membership.GetUser(account.UserId);
		System.Diagnostics.Debug.Assert(user != null, "No user found");

		errorMessage = string.Empty;
		bool updated = false;

		if (!ValidAccountInfo(userName, newPassword, confirmPassword, email, contactName, false, out errorMessage))
			return false;

		UpdateEmail(user, account, email, ref updated);

		UpdatePassword(user, oldPasswordRequired, oldPassword, newPassword, confirmPassword, ref errorMessage, ref updated);
		
		if (!updated && errorMessage != string.Empty)
			return false;

		UpdateContactName(account, contactName, ref updated);

		UpdateSendNewsletter(account, sendNewsletter, ref updated);

		UpdateUserName(user, account, userName, ref errorMessage, ref updated);
		if (!updated && errorMessage != string.Empty)
			return false;

		return updated;
	}

	private static void UpdateContactName(Account account, string contactName, ref bool updated)
	{
		if (string.IsNullOrEmpty(contactName))
			return;

		if (account.ContactName == contactName)
			return;
		
		account.UpdateContactNameInDatabase(contactName);
		updated = true;
	}

	private static void UpdateSendNewsletter(Account account, bool sendNewsletter, ref bool updated)
	{
		if (account.SendNewsletter == sendNewsletter)
			return;

		account.UpdateSendNewsletterInDatabase(sendNewsletter);
		updated = true;
	}

	private static void UpdateEmail(MembershipUser user, Account account, string email, ref bool updated)
	{
		if (string.IsNullOrEmpty(email))
			return;

		if (user.Email.ToLower() == email.ToLower())
			return;
		
		user.Email = email;
		Membership.UpdateUser(user);
		
		updated = true;
	}

	private static void UpdatePassword(
		MembershipUser user,
		bool oldPasswordRequired,
		string oldPassword,
		string newPassword,
		string confirmPassword,
		ref string errorMessage,
		ref bool updated)
	{
		if (string.IsNullOrEmpty(newPassword))
			return;

		string currentPassword = user.GetPassword();

		if (string.IsNullOrEmpty(oldPassword))
		{
			if (oldPasswordRequired)
			{
				errorMessage = "Please provide your old password";
				return;
			}
		}
		else
		{
			if (currentPassword != oldPassword)
			{
				errorMessage = "The old password is incorrect";
				return;
			}
		}

		if (newPassword != confirmPassword)
		{
			errorMessage = "The new and confirm passwords don't match";
			return;
		}

		if (currentPassword == newPassword)
			return;

		user.ChangePassword(user.GetPassword(), newPassword);
		Membership.UpdateUser(user);
		updated = true;
	}

	private static void UpdateUserName(MembershipUser user, Account account, string userName, ref string errorMessage, ref bool updated)
	{
		if (string.IsNullOrEmpty(userName))
			return;

		if (user.UserName.ToLower() == userName.ToLower())
			return;

		// See if the new name is available.
		MembershipUser existingUser = Membership.GetUser(userName);
		if (existingUser != null)
		{
			errorMessage = "The new email address is already in use";
			return;
		}

		// See if we are changing the user name for the current user.
		bool nameChangeIsForCurrentUser = MapsAliveState.Account.UserId == (Guid)user.ProviderUserKey;

		// The Membership class does not permit setting the user name so we have to do it ourselves.
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Account_UpdateUserName",
			"@UserId", account.UserId,
			"NewUserName", userName);

		if (nameChangeIsForCurrentUser)
		{
			// Create an authentication ticket and assign it to a cookie. We do this because
			// the cookie contains the user name (encrypted) and since we changed the name,
			// the old cookie is no good anymore. For more information, see the section called
			// Forms Authentication in this article: http://progtutorials.tripod.com/Authen.htm.
			FormsAuthentication.SetAuthCookie(userName, false);
		}

		// The email and user names are the same.
		user = Membership.GetUser(userName);
		user.Email = userName;
		Membership.UpdateUser(user);

		account.EmailChanged();

		updated = true;
	}

	public static bool ValidAccountInfo(
		string userName,
		string password,
		string confirmPassword,
		string email,
		string contactName,
		bool newAccount,
		out string errorMessage)
	{
		errorMessage = string.Empty;

		Account account = MapsAliveState.Account;

		if (!string.IsNullOrEmpty(userName) && !Utility.ValidEmailAddress(userName))
		{
			// The userName parameter is not a valid email address.
			errorMessage = "The email address is not valid";
			return false;
		}

		if (newAccount && string.IsNullOrEmpty(contactName))
		{
			errorMessage = "A contact name is required";
			return false;
		}

		if (newAccount && string.IsNullOrEmpty(email))
		{
			errorMessage = "An email address is required";
			return false;
		}

		if (!string.IsNullOrEmpty(email) && !Utility.ValidEmailAddress(email))
		{
			// The email parameter is not a valid email address.
			errorMessage = "The email address is not valid";
			return false;
		}

		if (newAccount && string.IsNullOrEmpty(password))
		{
			errorMessage = "A password is required";
			return false;
		}

		if (!string.IsNullOrEmpty(password) && !Utility.ValidPassword(password))
		{
			errorMessage = "Passwords must contain 5 to 16 characters";
			return false;
		}

		if (newAccount && string.IsNullOrEmpty(confirmPassword))
		{
			errorMessage = "A confirm password is required";
			return false;
		}

		if (password != confirmPassword)
		{
			errorMessage = "The new and confirm passwords don't match";
			return false;
		}

		if (newAccount && !string.IsNullOrEmpty(email))
		{
			if (Membership.GetUser(email) != null)
			{
				errorMessage = "The email address is already in use";
				return false;
			}
		}

		return true;
	}
}
