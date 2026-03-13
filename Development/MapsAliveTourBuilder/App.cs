// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Configuration;
using System.Diagnostics;
using System.Web;

public class App
{
	private const int databaseVersion = 61;
	
	private const int majorVersion = 4;
	private const int revVersion = 2;
	private const int minorVersion = 27;

	public static int AnnouncementId
	{
		get
		{
			try
			{
				return (int)MapsAliveDatabase.LoadScalar("sp_App_GetAnnouncementVersion");
			}
			catch
			{
				return 0;
			}
		}
	}

	public static string AppRuntimeFolderLocationAbsolute
	{
		get { return MapsAliveConfig.RuntimeDir; }
	}

	public static string AppRuntimeUrl
	{
		get { return WebSiteUrl + "AppRuntime/"; }
	}

	public static string AppRuntimeFolderRoot
	{
		get { return WebSiteRoot + "AppRuntime/"; }
	}

	public static string ArchiveVersion
	{
		get { return "1.0"; }
	}

	public static Config MapsAliveConfig
	{
		// Use this property when it's okay to initialize the config if it does not already exist.
		// Note that it does not exist when the app first starts, but it also does not exist when
		// it has been previously initialized and found to be invalid. It only gets persisted in
		// memory when it is valid. That way you don't have to restart the app to correct errors
		// and test the config again.
		get
		{
			Config config = (Config)MapsAliveState.Retrieve(MapsAliveObjectType.Config);
			if (config == null)
			{
				config = Config.Initialize();
			}
			return config;
		}
	}

    public static string ConfigPage
	{
		get { return "~/Default.aspx"; }
	}

	public static int DatabaseVersion
	{
		get { return databaseVersion; }
	}

	public static bool DeveloperMode
	{
		get { return ConfigurationManager.AppSettings["DeveloperMode"] == "1"; }
	}

	public static bool Installed
	{
		get
		{
			// Get the config from memory, but do not initialize it if it does not exist because
			// doing so could cause an infinite loop to occur from callers of this property.
			Config config = (Config)MapsAliveState.Retrieve(MapsAliveObjectType.Config);
			return config != null && config.Installed;
		}
	}

    public static string LogFile
    {
        get { return MapsAliveConfig.LogFile; }
    }

	public static int MajorVersion
	{
		get { return majorVersion; }
	}

	public static int MinorVersion
	{
		get { return minorVersion; }
	}

    public static string Revision
    {
        get { return String.Format("{0}{1}{2}", majorVersion, revVersion, minorVersion); }
    }

    public static string SamplesFolderLocationAbsolute
	{
		get { return MapsAliveConfig.SamplesDir; }
	}

	public static string TourFolderLocationAbsolute
	{
		get { return MapsAliveConfig.TourDir; }
	}

	public static string TourFolderUrl
	{
		get { return MapsAliveConfig.TourUrl + "/"; }
	}

	public static string TourUrl(int tourId)
	{
		return TourFolderUrl + tourId.ToString();
	}

	public static string Version
	{
		get { return string.Format("{0:0#}_{1:0###}", App.MajorVersion, App.MinorVersion); }
	}

    public static string VersionStamp
	{
		get { return string.Format("V{0:#}.{1:#}.{2:0#}", App.MajorVersion, revVersion, App.MinorVersion); }
	}

	public static string WebSitePathUrl(string relativePath)
	{
		Debug.Assert(relativePath.Substring(0, 1) != "/", "relativePath should not begin with '/'");
		return WebSiteUrl + relativePath;
	}

	public static string WebSitePathUrlSecure(string relativePath)
	{
		if (DeveloperMode)
		{
			return HttpRuntime.AppDomainAppVirtualPath + '/' + relativePath;
		}
		else
		{
			return WebSitePathUrl(relativePath).Replace("http", "https");
		}
	}

	public static string WebSiteRoot
	{
		get
		{
			// Return the path -- ending with '/' -- to the root of the web site.
			// We need this property because the path on the local host is different
			// than the path on the actual web server.  On local host the root is
			// '/MapsAlive' whereas on the server it's just '/'.
			string path = HttpRuntime.AppDomainAppVirtualPath;
			if (path.Length > 1)
				path += "/";
			return path;
		}
	}

	public static string WebSiteUrl
	{
		get { return MapsAliveConfig.SiteUrl + "/"; }
	}

	public static string WebSiteUrlShort
	{
		get
		{
			string url = MapsAliveConfig.SiteUrl;
			if (url.StartsWith("https://"))
				url = url.Substring(8);
			return url;
		}
	}
}
