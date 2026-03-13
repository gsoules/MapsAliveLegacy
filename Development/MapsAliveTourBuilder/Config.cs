// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Configuration;
using System.IO;
using System.Net.Configuration;
using System.Net.Mail;
using System.Xml;

public class Config
{
	private string adminEmail;
	private readonly DateTime createDate;
	private readonly bool configNotFound;
	private bool installed;
    private string logFile;
	private string parseError;
	private string password;
    private double release;
	private string runtimeDir;
	private bool runtimeDirAccessible;
	private bool runtimeDirExists;
	private string runtimeDirUrl;
	private bool runtimeDirUrlOk;
	private string samplesDir;
	private bool samplesDirAccessible;
	private bool samplesDirExists;
	private string siteUrl;
	private bool siteUrlOk;
	private bool smtpOk;
	private string smtpError;
	private bool tourDirAccessible;
	private bool tourDirExists;
	private string tourDir;
	private string tourUrl;
	private bool tourUrlOk;
	private XmlDocument xmlDocument;
	private bool xmlOk;

	public Config()
	{
		createDate = DateTime.Now;
		smtpOk = false;

		string xmlLocation = FileManager.WebAppFileLocationAbsolute("", "MapsAlive.config");
		
		if (FileManager.FileExists(xmlLocation))
		{
			ParseConfigXml(xmlLocation);
		}
		else
		{
			configNotFound = true;
		}
	}

	public string AdminEmail
	{
		get { return adminEmail; }
	}

	public void ApproveInstallation()
	{
		// Create a file to be used as a flag indicating that the installation is okay.
		if (!Installed)
			FileManager.CreateTextFile(InstallFileLocation, DateTime.Now.ToString());
	}

	public DateTime CreateDate
	{
		get { return createDate; }
	}

	public bool DatabaseOk
	{
		get { return MapsAliveDatabase.NotConnected == false; }
	}

	private bool GetBoolValue(string xpath)
	{
		string value = GetStringValue(xpath).ToLower();
		return value == "true";
	}

	private string GetStringValue(string xpath)
	{
		string value = string.Empty;
		XmlElement element = (XmlElement)xmlDocument.SelectSingleNode(xpath);
		if (element != null)
		{
			XmlAttribute attribute = element.Attributes["value"];
			if (attribute != null)
				value = attribute.Value;
		}
		return value;
	}

	public static Config Initialize()
	{
		Config config = new Config();

		if (config.XmlOk)
		{
			config.ValidateDirs();
			
			// Persist the config so that we don't have to parse it again.
			MapsAliveState.Persist(MapsAliveObjectType.Config, config);

			config.CreateAppRuntime();
		}

		return config;
	}

	private void CreateAppRuntime()
	{
		if (runtimeDirExists)
		{
			try
			{
				// Copy the latest files to the AppRuntime folder. We do this each time
				// the configuration is read in case we have updated any of the files.
				TourBuilder tourBuilder = new TourBuilder();
				tourBuilder.CopyAppRuntimeFilesToAppRuntimeFolder();
			}
			catch
			{
			}
		}
	}

	private bool DirIsAccessible(string dir)
	{
		string fileLocation = Path.Combine(dir, "test");
		FileManager.CreateTextFile(fileLocation, DateTime.Now.ToString());
		if (!FileManager.FileExists(fileLocation))
			return false;
		FileManager.DeleteFile(fileLocation);
		return !FileManager.FileExists(fileLocation);
	}

	public bool Installed
	{
		get
		{
			if (runtimeDirExists)
			{
				if (!installed)
					installed = FileManager.FileExists(InstallFileLocation);
			}
			else
			{
				// The install file location is determined from the site directory location.
				// We can't test for the install file until that dir has been validated.
				installed = false;
			}
			
			return installed;
		}
	}

	public string InstallFileLocation
	{
		get
		{
			// Make sure we don't attempt to get a location that comes from MapsAlive.config before it has passed.
			System.Diagnostics.Debug.Assert(runtimeDirExists, "Attempt to access install file location before siteDirOk");
			
			return Path.Combine(FileManager.AppRuntimeFolderLocationAbsolute, "installed");
		}
	}

    public string LogFile
    {
        get { return logFile; }
    }

    public bool NotFound
	{
		get { return configNotFound; }
	}

	private void ParseConfigXml(string xmlLocation)
	{
		try
		{
			string xml = FileManager.ReadFileContents(xmlLocation);
			xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);

			tourUrl = GetStringValue("//tourUrl");
			siteUrl = GetStringValue("//siteUrl");
			samplesDir = GetStringValue("//samplesDir");
			runtimeDir = GetStringValue("//runtimeDir");
			runtimeDirUrl = siteUrl + "/AppRuntime";
			tourDir = GetStringValue("//tourDir");
			adminEmail = GetStringValue("//adminEmail");
			password = GetStringValue("//configPassword");
			logFile = GetStringValue("//logFile");

            if (!double.TryParse(GetStringValue("//release"), out release))
                release = 0.0;
			
			parseError = string.Empty;
			xmlOk = true;
		}
		catch (Exception ex)
		{
			parseError = ex.Message;
		}
	}

	public bool Passed
	{
		// The config is considered okay if the basic configuration settings are okay.
		// The status of the database and SMTP are not factored in. Note also that
		// siteUrlOk and tourUrlOk are not able to be tested when the config is initialized.
		// They are validated when the first session starts following an application restart.
		get
		{
			return
				xmlOk &&
				samplesDirExists &&
				runtimeDirExists &&
				runtimeDirAccessible &&
				tourDirExists &&
				tourDirAccessible;
		}
	}

	public string Password
	{
		get { return password; }
	}

	public string ParseError
	{
		get { return parseError == null ? string.Empty : parseError; }
	}

    public double Release
    {
        get { return release; }
    }

    public string RuntimeDir
	{
		get { return runtimeDir; }
	}

	public bool RuntimeDirAccessible
	{
		get { return runtimeDirAccessible; }
	}

	public bool RuntimeDirExists
	{
		get { return runtimeDirExists; }
	}

	public string RuntimeDirUrl
	{
		get { return runtimeDirUrl; }
	}

	public bool RuntimeDirUrlOk
	{
		get { return runtimeDirUrlOk; }
	}
	
	public string SamplesDir
	{
		get { return samplesDir; }
	}

	public bool SamplesDirAccessible
	{
		get { return samplesDirAccessible; }
	}

	public bool SamplesDirExists
	{
		get { return samplesDirExists; }
	}

	public string SiteUrl
	{
		get { return siteUrl; }
	}

	public bool SiteUrlOk
	{
		get { return siteUrlOk; }
	}

	public bool SmtpDisabled
	{
		get { return adminEmail == string.Empty; }
	}

	public string SmtpError
	{
		get { return smtpError == null ? string.Empty : smtpError; }
	}

	public bool SmtpOk
	{
		get { return smtpOk; }
	}

	public string TourDir
	{
		get { return tourDir; }
	}

	public bool TourDirAccessible
	{
		get { return tourDirAccessible; }
	}

	public bool TourDirExists
	{
		get { return tourDirExists; }
	}

	public string TourUrl
	{
		get { return tourUrl; }
	}

	public bool TourUrlOk
	{
		get { return tourUrlOk; }
	}

	private void ValidateDirs()
	{
		samplesDirExists = FileManager.FolderExists(samplesDir);
		if (samplesDirExists)
			samplesDirAccessible = DirIsAccessible(samplesDir);
		else
			samplesDirAccessible = false;
		
		runtimeDirExists = FileManager.FolderExists(runtimeDir);
		if (runtimeDirExists)
			runtimeDirAccessible = DirIsAccessible(runtimeDir);
		else
			runtimeDirAccessible = false;
		
		tourDirExists = FileManager.FolderExists(tourDir);
		if (tourDirExists)
			tourDirAccessible = DirIsAccessible(tourDir);
		else
			tourDirAccessible = false;
	}

	public void ValidateSmtp()
	{
		string subject = "MapsAlive SMTP Test V4 (ma-04)";
		string body = "This is a test message from MapsAlive V4 to validate SMTP settings.";

		MailMessage mailMessage = new MailMessage(Utility.EmailForSupport, Utility.EmailForSupport, subject, body);
        SmtpClient smtpClient = Utility.CreateSmtpClient();
        try
        {
            smtpClient.Send(mailMessage);
			smtpError = string.Empty;
			smtpOk = true;
		}
		catch (Exception ex)
		{
			// We test for a generic exception instead of an SmtpException because both can be raised.
			smtpError = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
			smtpOk = false;
		}
	}

	public void ValidateUrls()
	{
		tourUrlOk = Utility.UrlFound(tourUrl);
		
		// Verify that the site URL is good by looking for a folder on the site. We must
		// not just check the site URL by itself because doing so will cause Default.aspx
		// to load after first triggering a session start. Since we validate URLs at
		// session start, we would get into an infinite loop. We also can't test for special
		// folders like bin or any .config files since those will not be found. So we test
		// for the Images folder which will be found even though access is forbidden.
		siteUrlOk = Utility.UrlFound(siteUrl + "/Images");
		runtimeDirUrlOk = Utility.UrlFound(runtimeDirUrl);
	}

	public bool XmlOk
	{
		get { return xmlOk; }
	}
}
