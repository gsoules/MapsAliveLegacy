// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Web.Security;

public partial class User_SignUp : System.Web.UI.Page
{
	protected void Page_Load(object sender, EventArgs e)
    {
		if (!Request.IsSecureConnection && !App.DeveloperMode)
		{
			Response.Redirect("~/Default.aspx");
			return;
		}

        if (Utility.UserIsLoggedIn)
		{
			// Only anonymous users can sign up. Log out the existing user.
			// This removes their authentication cookie. We have to redirect back
			// to this page so that Membership can tell that the user is not logged in.
			MapsAliveState.Account.Logout();
			Response.Redirect("SignUp.aspx");
		}

		Utility.PreventPageCaching(Response);

		SignUpControl.ButtonName = "Sign Up";

		if (IsPostBack && Request.QueryString["login"] != "0")
			OnSignUp();
	}
	
	protected void OnGoHome(object sender, EventArgs e)
	{
		Response.Redirect("~/Default.aspx");
	}

	protected void OnPlans(object sender, EventArgs e)
	{
		Response.Redirect("~/public/pricing");
	}

	protected void OnSignUp()
	{
        bool valid = ReCaptchIsValid();

        if (!valid)
        {
            SignUpControl.ErrorMessage = "Unable to create account. Please contact support@mapsalive.com";
            return;
        }

        AccountPlan accountPlan = AccountPlan.Pro;
		AccountType accountType = AccountType.Trial;
		int hotspotLimit = accountType == AccountType.Trial ? 300 : 0;
		int days = accountType == AccountType.Trial ? 30 : 0;
		bool accountCreated = SignUpControl.CreateAccount(accountType, accountPlan, days, hotspotLimit);

		if (!accountCreated)
			return;

		SignUpControl.SendWelcomeEmail(AccountType.Trial);
		
		string targetPage = "../Members/" + MemberPageAction.ActionPageTarget(MemberPageActionId.Welcome) + "?signup=1";
		Response.Redirect(targetPage);
	}

    public bool ReCaptchIsValid()
    {
        return true;

        var valid = false;
        
        // Get the token that the client-side captcha JavaScript produced and posted in the form.
        var captchaResponse = Request.Form["g-recaptcha-response"];
        
        // Ask Google to verify the response.
        var secretKey = "6LeoE0geAAAAALNLz6pmMkOn-OGIQmjJE0aLbS-I";
        var apiUrl = "https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}";
        var requestUri = string.Format(apiUrl, secretKey, captchaResponse);

        /* Get the response back as JSON that looks like this:
        {
            "success": true | false,
            "challenge_ts": timestamp,
            "hostname": string,
            "error-codes": [...]
        }
        */
        var request = (HttpWebRequest)WebRequest.Create(requestUri);

        // Since we don't have a JSON parser, strip out white space and look for: "Success":true 
        using (WebResponse response = request.GetResponse())
        {
            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
            {
                var json = streamReader.ReadToEnd();
                json = json.Replace(" ", "");
                valid = json.Contains("\"success\":true");
            }
        }

        return valid;
    }
}