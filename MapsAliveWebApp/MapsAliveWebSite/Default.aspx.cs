using System;
using System.Configuration;
using System.IO;
using System.Net.Configuration;
using System.Web;
using System.Web.UI;

public partial class Default : System.Web.UI.Page
{
	private bool badDatabaseVersion;
	private	Config config;
	private bool fatalError;
	private const string statusPass = "PASS";
	private const string statusFail = "FAIL";
	private const string statusNotTested = "Not Tested";
	
	protected void Page_Load(object sender, EventArgs e)
    {
		config = App.MapsAliveConfig;
		
		if (IsPostBack)
			return;

		if (ShowConfigSettings())
			DisplayConfiguration();
		else
			GoToHomePage();
	}

	private void DisplayConfiguration()
	{
		Utility.PreventPageCaching(Response);
		StatusPanel.Visible = true;

		DisplayOverviewSection();

		if (fatalError)
			return;

		DisplayLocationsSection();
		DisplayDatabaseSection();
		DisplaySmtpSection();
	}

	private void DisplayDatabaseSection()
	{
		ConnectionStringSettings dbSettings = ConfigurationManager.ConnectionStrings["MapsAliveSqlServer"];
		if (dbSettings == null)
		{
			DatabaseStatus.Text = ErrorValue("Web.Config does not contain a connection string for MapsAliveSqlServer");
		}
		else
		{
			try
			{
				System.Data.Common.DbConnectionStringBuilder sb = new System.Data.Common.DbConnectionStringBuilder();
				sb.ConnectionString = dbSettings.ConnectionString;
				DatabaseServer.Text = (string)sb["data source"];
				Database.Text = (string)sb["initial catalog"];
				DatabaseServerLogin.Text = (string)sb["user id"];
			}
			catch (Exception ex)
			{
				DatabaseServer.Text = "Connection string is bad: " + ex.Message;
			}

			if (config.DatabaseOk)
			{
				UserName.Text = Utility.UserIsLoggedIn ? Utility.UserName : "Not logged in";
				DatabaseStatus.Text = PassValue();
				
				int dbVersion = MapsAliveDatabase.DatabaseVersion;
				if (dbVersion == App.DatabaseVersion)
				{
					DatabaseVersion.Text = dbVersion.ToString();
				}
				else
				{
					badDatabaseVersion = true;
					DatabaseVersion.Text = ErrorValue(string.Format("Required database version is {0}, but connected version is {1}", App.DatabaseVersion, dbVersion));
					ConfigStatus.Text = ErrorStatus();
					DatabaseStatus.Text = ErrorValue(statusFail);
				}
			}
			else
			{
				// The database connection did not pass.
				UserName.Text = UnknownValue();
				string dbStatus = MapsAliveDatabase.DatabaseStatus;
				if (dbStatus == string.Empty)
					dbStatus = UnknownValue();
				else
					dbStatus = ErrorValue(dbStatus.Replace(Utility.CrLf, "<br/>"));

				DatabaseStatus.Text = dbStatus;
				ConfigStatus.Text = ErrorStatus();
			}
		}
	}
	private void DisplayLocationsSection()
	{
		bool locationsOk =
			config.SamplesDirExists &&
			config.SamplesDirAccessible &&
			config.RuntimeDirExists &&
			config.RuntimeDirUrlOk &&
			config.RuntimeDirAccessible &&
			config.SiteUrlOk &&
			config.TourDirExists &&
			config.TourDirAccessible &&
			config.TourUrlOk;
		LocationsStatus.Text = locationsOk ? PassValue() : ErrorValue(statusFail);
		
		SamplesDir.Text = LocationValue(config.SamplesDirExists, config.SamplesDir);
		if (config.SamplesDirExists)
			SamplesDir.Text = AccessibleValue(config.SamplesDirAccessible, config.SamplesDir);
		
		RuntimeDir.Text = LocationValue(config.RuntimeDirExists, config.RuntimeDir);
		if (config.RuntimeDirExists)
			RuntimeDir.Text = AccessibleValue(config.RuntimeDirAccessible, config.RuntimeDir);

		RuntimeUrl.Text = LocationValue(config.RuntimeDirUrlOk, config.RuntimeDirUrl);

		SiteUrl.Text = LocationValue(config.SiteUrlOk, config.SiteUrl);
		
		TourDir.Text = LocationValue(config.TourDirExists, config.TourDir);
		if (config.TourDirExists)
			TourDir.Text = AccessibleValue(config.TourDirAccessible, config.TourDir);
		
		TourUrl.Text = LocationValue(config.TourUrlOk, config.TourUrl);
	}

	private bool DisplayOverviewSection()
	{
		string configStatus;

		if (config.Passed)
		{
			configStatus = PassValue();
		}
		else
		{
			configStatus = ErrorStatus();

			if (config.NotFound)
			{
				configStatus += "<br/><br/>" + ErrorValue("MapsAlive.config file was not found");
				fatalError = true;
			}

			if (config.ParseError != string.Empty)
			{
				configStatus += "<br/><br/>" + ErrorValue("Could not parse MapsAlive.config file.<br/>") + config.ParseError;
				fatalError = true;
			}
		}

		ConfigStatus.Text = configStatus;
		TimeStamp.Text = Utility.DateShort(config.CreateDate);

		Version.Text = App.VersionStamp;
        DeveloperMode.Text = App.DeveloperMode.ToString();

		SysAdminEmail.Text = config.AdminEmail == string.Empty ? WarningValue("Not provided. MapsAlive will not send email.") : config.AdminEmail;
		return fatalError;
	}

	private void DisplaySmtpSection()
	{
		SmtpSection smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
		if (smtpSection == null)
		{
			SmtpStatus.Text = ErrorValue("Web.Config does not contain a section for system.net/mailSettings/smtp");
		}
		else
		{
			SmtpHost.Text = smtpSection.Network.Host;
			SmtpPort.Text = smtpSection.Network.Port.ToString();
			SmtpUserName.Text = smtpSection.Network.UserName;

			string smtpStatus;

			if (config.SmtpError == string.Empty)
			{
				if (config.SmtpOk)
				{
					smtpStatus = PassValue();
				}
				else
				{
					if (SmtpPasses)
					{
						smtpStatus = WarningValue(statusNotTested);
						TestSmtp.Enabled = config.AdminEmail != string.Empty;
					}
					else
					{
						// A non-empty email that has not been tested is an error.
						smtpStatus = ErrorValue("Not tested. Click the send test link above.");
					}
				}
			}
			else
			{
				smtpStatus = ErrorValue(config.SmtpError);
			}

			if (config.Passed && config.DatabaseOk && !config.SmtpOk && !badDatabaseVersion)
				ConfigStatus.Text = SmtpPasses ? PassValue() : ErrorStatus();

			SmtpStatus.Text = smtpStatus;
		}
	}

	private string ErrorStatus()
	{
		return WarningValue("There are errors (shown in red). Correct them and restart.");
	}

	private string ErrorValue(string value)
	{
		return string.Format("<span class='configError'>{0}</span>", value);
	}

	private void GoToHomePage()
	{
		if (!App.Installed)
		{
			StatusPanel.Visible = true;
			ConfigStatus.Text = ErrorValue(string.Format("MapsAlive is not installed properly.<div style='color:black;font-weight:normal;'>Verify that {0} exists.</div>", config.InstallFileLocation));
			return;
		}

        Response.Redirect("~/Public");
	}

	private string AccessibleValue(bool ok, string value)
	{
		if (ok)
			return value;
		else
			return string.Format("Insufficient IIS permision on {0}", ErrorValue(value));
	}

	private string LocationValue(bool ok, string value)
	{
		if (ok)
			return value;
		else
			return string.Format("{0} was not found", ErrorValue(value));
	}

	protected void OnHome(object sender, EventArgs e)
	{
		GoToHomePage();
	}

	protected void OnRestart(object sender, EventArgs e)
	{
		try
		{
			HttpRuntime.UnloadAppDomain();
			Response.Redirect(App.ConfigPage);
		}
		catch (Exception ex)
		{
			RestartError.Text = ErrorValue(ex.Message);
		}
	}

	protected void OnTestSmtp(object sender, EventArgs e)
	{
		config.ValidateSmtp();
		Response.Redirect(string.Format("{0}?config=1&pw={1}", App.ConfigPage, config.Password));
	}

	private string PassValue()
	{
		return string.Format("<span class='configError' style='color:green;'>{0}</span>", statusPass);
	}

	private bool ShowConfigSettings()
	{
		bool showConfig = false;

		if (config.Installed)
		{
			// We don't normally show config settings when installed. See if they have been requested.
			bool requestConfig = Request.QueryString["config"] == "1";
			if (requestConfig)
			{
				// The user has requested to see the config settings. Show them if they have
				// put the password on the query string or they are logged is as the administrator.
				bool configPassworkOk = Request.QueryString["pw"] == config.Password;
				showConfig = requestConfig && (configPassworkOk || Utility.UserIsAdmin);
			}
		}
		else
		{
			// The app is not installed. Show config settings so the admin can see any errors.
			showConfig = true;
			ValidateUrls();

			if (config.Passed && config.SiteUrlOk && config.TourUrlOk && config.DatabaseOk)
			{
				// All critical tests have passed. See if the SMTP settings are okay.
				if (config.SmtpOk || SmtpPasses)
				{
					// The configuration is 100% approved.
					// Create an install file that serves as a flag to indicate that the configuration
					// passed all tests during installation. Unless the file is manually deleted by a
					// system administrator, the app ins considered installed unless MapsAlive.conig
					// is edited and then does not pass basic testing following an application restart.
					// Even then, the install file remains in place so that even a database error will
					// not cause the config settings to be displayed again automatically. This is by
					// design so that a user will never see the config settings due to a temporary
					// problem with the database connection or SMTP.
					config.ApproveInstallation();
					if (!config.Installed)
					{
						StatusFlags.Text = ErrorValue("1: Could not create install file");
						showConfig = true;
					}
				}
				else
				{
					StatusFlags.Text = string.Format("2: {0}{1}", StatusFlag(config.SmtpOk), StatusFlag(SmtpPasses));
					showConfig = true;
				}
			}
			else
			{
				// A critical test has not yet passed so don't let the user go to the home page.
				HomeLink.Enabled = false;
				
				// Set flags that will help us debug the problem if the page info is not enough.
				StatusFlags.Text = string.Format("3: {0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}",
					StatusFlag(config.Passed),
					StatusFlag(config.SiteUrlOk),
					StatusFlag(config.TourUrlOk),
					StatusFlag(config.DatabaseOk),
					StatusFlag(config.SamplesDirExists),
					StatusFlag(config.SamplesDirAccessible),
					StatusFlag(config.RuntimeDirExists),
					StatusFlag(config.RuntimeDirAccessible),
					StatusFlag(config.TourDirExists),
					StatusFlag(config.TourDirAccessible),
					StatusFlag(config.XmlOk),
					StatusFlag(config.RuntimeDirUrlOk)
					);
			}
		}

		// If not showing the config and the DB is okay, make sure the app is using the right DB version.
		if (!showConfig && (config.DatabaseOk && App.DatabaseVersion != MapsAliveDatabase.DatabaseVersion))
		{
			// Force the status screen to show the version mismatch error.
			showConfig = true;
			StatusFlags.Text = string.Format("4: {0}{1}", StatusFlag(config.DatabaseOk), StatusFlag(App.DatabaseVersion != MapsAliveDatabase.DatabaseVersion));
		}

		if (showConfig)
		{
			// To keep from showing spurious URL errors, validate the URL values now even if they were validated earlier.
			ValidateUrls();
		}
		
		return showConfig;
	}

	private void ValidateUrls()
	{
		if (!config.SiteUrlOk || !config.TourUrlOk)
		{
			// Test if the URLs are okay. Ideally this would be done by the Config object, but
			// that object is created at application start and we found that having the app
			// ping itself during during start-up could cause the app to hang.
			config.ValidateUrls();
		}
	}

	private bool SmtpPasses
	{
		get { return config.AdminEmail == string.Empty || config.Installed; }
	}

	private int StatusFlag(bool condition)
	{
		return condition ? 1 : 0;
	}

	private string UnknownValue()
	{
		return string.Format("<span style='color:gray;'>{0}</span>", "Not evaluated until all errors are corrected");
	}

	private string WarningValue(string value)
	{
		return string.Format("<span class='configError' style='color:#ff6600;'>{0}</span>", value);
	}
}

