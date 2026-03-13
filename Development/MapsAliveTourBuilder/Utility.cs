// Copyright (C) 2006 AvantLogic Corporation
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public class Utility
{
	public const string CrLf = "\r\n";

	public Utility()
	{
	}

	public static int AccountId
	{
		get
		{
			Account account = MapsAliveState.Account;
			if (account == null)
			{
				// This should never happen except during development in situations where testing
				// with different MapsAlive URLs and logins creates confusion with cookies in a
				// way that allows the app to authenticate a user when it should not.
				HttpContext.Current.Server.Transfer("~/User/Login.aspx");
			}
			return account.Id;
		}
	}

	public static string AccountIdentification
	{
		get	{ return MapsAliveState.Account.ContactName; }
	}

	public static string BitmapFormat(Bitmap bitmap)
	{
		System.Drawing.Imaging.ImageFormat format = bitmap.RawFormat;
		string formatName = "unknown";
		if (format.Equals(System.Drawing.Imaging.ImageFormat.Bmp)) formatName = "bmp";
		else if (format.Equals(System.Drawing.Imaging.ImageFormat.Emf)) formatName = "emf";
		else if (format.Equals(System.Drawing.Imaging.ImageFormat.Exif)) formatName = "exif";
		else if (format.Equals(System.Drawing.Imaging.ImageFormat.Gif)) formatName = "gif";
		else if (format.Equals(System.Drawing.Imaging.ImageFormat.Icon)) formatName = "icon";
		else if (format.Equals(System.Drawing.Imaging.ImageFormat.Jpeg)) formatName = "jpeg";
		else if (format.Equals(System.Drawing.Imaging.ImageFormat.MemoryBmp)) formatName = "memorybmp";
		else if (format.Equals(System.Drawing.Imaging.ImageFormat.Png)) formatName = "png";
		else if (format.Equals(System.Drawing.Imaging.ImageFormat.Tiff)) formatName = "tiff";
		else if (format.Equals(System.Drawing.Imaging.ImageFormat.Wmf)) formatName = "wmf";
		return formatName;
	}

	public static Bitmap BitmapFromBytes(Byte[] bytes)
	{
        return BitmapFromStream(new MemoryStream(bytes), out _, out _);
    }

	public static Bitmap BitmapFromBytes(Byte[] bytes, out string message)
	{
        return BitmapFromStream(new MemoryStream(bytes), out _, out message);
    }

	public static Bitmap BitmapFromStream(Stream fileStream, out ImageFormat imageFormat, out string message)
	{
		Bitmap bitmap = null;
		message = string.Empty;
		
		try
		{
			bitmap = new Bitmap(fileStream);
			imageFormat = bitmap.RawFormat;

			if (ImageIsCmykOrYcck(bitmap))
			{
				bitmap = null;
				message = "CMYK/YCCK color is not supported (convert to RGB)";
			}
		}
		catch
		{
			message = "Could not open file as an image";
			imageFormat = ImageFormat.Jpeg;
		}

		return bitmap;
	}

	public static Byte[] BytesFromUploadedBitmap(Bitmap bitmap, ImageFormat imageFormat, ref Size maxSize)
	{
		if (bitmap.Width > maxSize.Width || bitmap.Height > maxSize.Height)
		{
			Bitmap scaledBitmap = ScaledBitmap(bitmap, maxSize, true);
			bitmap = scaledBitmap;
		}

		// Return the actual size in case the image got scaled.
		maxSize = bitmap.Size;
		
		Byte[] imageBytes = ImageToByteArray(bitmap, imageFormat, false);
		
		// If we could not handle the bitmap's native format, convert it to jpg.
		if (imageBytes == null)
		{
			imageBytes = ImageToByteArray(bitmap, ImageFormat.Jpeg, true);
			if (imageBytes == null)
			{
                // There's something wrong with this image. Return a "bad image" image.
                // See OnTime task 791 for an example of tif image that triggers this problem.
                string badImageFileLocation = FileManager.WebAppFileLocationAbsolute("Images", "BadImage.jpg");
                imageBytes = Utility.ImageFileToByteArray(badImageFileLocation, out _);
			}
		}
		
		return imageBytes;
	}

	public static string ColorToHtml(Color color)
	{
		return "#" + color.ToArgb().ToString("x").Substring(2);
	}

	public static void CopyImageFileToTourFolder(string fileName, string fileExt, int tourId, string tourFileName)
	{
		string imageFileName = fileName + "." + fileExt;
		string source = FileManager.WebAppFileLocationAbsolute("Images", imageFileName);
		string target = FileManager.PreviewFolderLocationAbsolute(tourId, tourFileName);
		FileManager.CopyFile(source, target);
	}

	public static void CopyStream(Stream input, Stream output)
	{
		byte[] bytes = new byte[4096];
		while (true)
		{
			int read = input.Read(bytes, 0, bytes.Length);
			if (read <= 0)
			{
				output.Position = 0;
				return;
			}
			output.Write(bytes, 0, read);
		}
	}

	public static string CountryName(string countryCode)
	{
		string filterExp = "Code='" + countryCode + "'";
		return MapsAliveDatabase.SelectRowFromDataTable(
			MapsAliveState.DataTableForCountry(), filterExp).StringValue("Name");
	}

    public static string CreateErrorDump()
    {
        ActionQueue actionQueue = (ActionQueue)MapsAliveState.Retrieve(MapsAliveObjectType.ActionQueue);
        string actionQueueDump;
        if (actionQueue == null)
            actionQueueDump = "ActionQueue is null";
        else
            actionQueueDump = actionQueue.Dump();

        string dump = "";
        dump += "ACTION TRACE:\n" + actionQueueDump + "\n";
        dump += "STACK TRACE:\n" + DumpStackFrames();
        dump += "REQUEST:\n" + DumpRequestParameters();

        return dump;
    }

    public static string CreditCardName(int creditCardId)
	{
		string filterExp = "CardId=" + creditCardId;
		return MapsAliveDatabase.SelectRowFromDataTable(
			MapsAliveState.DataTableForCreditCard(), filterExp).StringValue("CardName");
	}

	private static MembershipUser CurrentUser
	{
		get
		{
			MembershipUser user = null;
			HttpContext context = HttpContext.Current;
			if (context != null)
			{
				try
				{
					string imitatedUserEmail = (string)context.Cache[ImitatedUserKey];

					if (imitatedUserEmail == null)
					{
						// No one is being imitated.  Get the actual user.
						return Membership.GetUser();
					}
					else
					{
						// Ad admin user is imitating another user.  Make sure the admin is still logged in.
						if (Membership.GetUser() == null)
							return null;
						else
							return Membership.GetUser(imitatedUserEmail);
					}
				}
				catch
				{
				}
			}
			return user;
		}
	}

	public static string DateShort(DateTime dateTime)
	{
		return dateTime.ToShortDateString() + " at " + dateTime.ToShortTimeString();
	}

	public static string DeactivatedPageHtml
	{
		get { return FileContents(MapsAliveObjectType.DeactivatedPageHtml, "DeactivatedPageHtml.htm"); }
	}

	public static string DeactivatedPageJavascript
	{
		get { return FileContents(MapsAliveObjectType.DeactivatedPageJavascript, "DeactivatedPageJavascript.js"); }
	}

	public static Byte[] DefaultImageBytes(Size requiredSize, string color)
	{
		return DefaultImageBytes(requiredSize, color, null);
	}

	public static Byte[] DefaultImageBytes(Size requiredSize, string color, string text)
	{
		try
		{
			Bitmap bitmap = new Bitmap(requiredSize.Width, requiredSize.Height);

			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.Clear(Utility.HexToColor(color));

				if (!String.IsNullOrEmpty(text))
				{
					Rectangle rect = new Rectangle(0, 0, requiredSize.Width, requiredSize.Height);
					graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
					graphics.DrawString(text, new Font("Arial", 10, FontStyle.Bold), Brushes.Green, rect);
				}
			}

			return ImageToByteArray(bitmap, ImageFormat.Jpeg);
		}
		catch
		{
			return null;
		}
	}

	private static string DumpRequestParameters()
	{
		string dump = string.Empty;
		HttpRequest request = HttpContext.Current.Request;

		if (request != null)
		{
			NameValueCollection param = request.Params;
			for (int i = 0; i < param.Count; i++)
			{
                string name = param.Keys[i];
                if (DumpRequestParametersExcludeParameter(name))
                    continue;
				string value = param[i];
				if (name != "ALL_RAW" && name != "ALL_HTTP" && value.Length > 0)
				{
					dump += string.Format("{0}[{1}]\n", name, value);
				}
			}
		}
		return dump;
	}

    private static bool DumpRequestParametersExcludeParameter(string name)
    {
        if (name.StartsWith(".ASPXAUTH"))
            return true;
        if (name.StartsWith("RadUrid"))
            return true;
        if (name.StartsWith("__"))
            return true;
        if (name.StartsWith("APPL_"))
            return true;
        if (name.StartsWith("AUTH_"))
            return true;
        if (name.StartsWith("REMOTE_USER"))
            return true;
        if (name.StartsWith("LOCAL_ADDR"))
            return true;
        if (name.StartsWith("PATH_"))
            return true;
        if (name.StartsWith("REMOTE_HOST"))
            return true;
        if (name.StartsWith("HTTPS"))
            return true;
        if (name.StartsWith("INSTANCE"))
            return true;
        if (name.StartsWith("CERT_"))
            return true;
        if (name.StartsWith("CONTENT_LENGTH"))
            return true;
        if (name.StartsWith("CONTENT_TYPE"))
            return true;
        if (name.StartsWith("GATEWAY_"))
            return true;
        if (name.StartsWith("SERVER_"))
            return true;
        if (name.StartsWith("HTTP_ACCEPT"))
            return true;
        if (name.StartsWith("HTTP_HOST"))
            return true;
        if (name.StartsWith("HTTP_COOKIE"))
            return true;
        if (name.StartsWith("HTTP_CACHE_CONTROL"))
            return true;
        if (name.StartsWith("HTTP_CONNECTION"))
            return true;
        if (name.StartsWith("HTTP_CONTENT"))
            return true;
        if (name.StartsWith("HTTP_SEC"))
            return true;
        if (name.StartsWith("HTTP_UPGRADE"))
            return true;
        if (name.StartsWith("HTTP_ORIGIN"))
            return true;
        if (name.StartsWith("REMOTE_PORT"))
            return true;
        if (name.StartsWith("REQUEST_METHOD"))
            return true;
        if (name.StartsWith("SCRIPT_NAME"))
            return true;
        return false;
    }

    private static string DumpStackFrames()
	{
		string dump = string.Empty;
		try
		{
			// Get the name of the method that reported this error.
			StackTrace stackTrace = new StackTrace(1, true);
			StackFrame[] stackFrames = stackTrace.GetFrames();
			foreach (StackFrame stackFrame in stackFrames)
			{
                string methodName = stackFrame.GetMethod().Name;
                if (DumpStackFramesExcludeMethodName(methodName))
                    continue;
				
                string fileName = stackFrame.GetFileName();

				if (fileName == null)
				{
					// A null file name means we are in system code.
					fileName = string.Empty;
				}
				else
				{
					// Extract just the file name from the full file location.
					fileName = new FileInfo(fileName).Name;
					fileName = " in " + fileName;
				}

				string line = stackFrame.GetFileLineNumber().ToString();
				if (line == "0")
					line = string.Empty;
				else
					line = " line " + line;

                dump += string.Format("{0}{1}{2}\n", methodName, fileName, line);
			}
		}
		catch
		{
			dump = "AN ERROR OCCURRED CREATING THE STACK FRAME DUMP<br/>" + dump;
		}

		return dump;
	}

    private static bool DumpStackFramesExcludeMethodName(string methodName)
    {
        if (methodName.StartsWith("SendEmail"))
            return true;
        if (methodName.StartsWith("ReportError"))
            return true;
        if (methodName.StartsWith("ReportException"))
            return true;
        if (methodName.StartsWith("CreateErrorDump"))
            return true;
        if (methodName.StartsWith("WriteErrorToLogFile"))
            return true;
        if (methodName.StartsWith("WriteToLogFile"))
            return true;
        if (methodName == "EventArgFunctionCaller")
            return true;
        if (methodName == "Callback")
            return true;
        if (methodName == "OnLoad")
            return true;
        if (methodName == "LoadRecursive")
            return true;
        if (methodName.Contains("IExecutionStep"))
            return true;
        if (methodName == "ExecuteStep")
            return true;
        if (methodName == "ResumeSteps")
            return true;
        if (methodName.Contains("ProcessRequest"))
            return true;
        return false;
    }

	public static string EmailForSupport
	{
		get { return "support@mapsalive.com"; }
	}

	public static string EmailPlainTextForLowTimeWarning
	{
		get { return FileContents(MapsAliveObjectType.EmailPlainTextForLowTimeWarning, "EmailPlainLowTimeWarning.txt"); }
	}

	public static string EmailPlainTextForNewOrder
	{
		get { return FileContents(MapsAliveObjectType.EmailPlainTextForNewOrder, "EmailPlainNewOrder.txt"); }
	}

	public static string EmailPlainTextForNoChargeOrder
	{
		get { return FileContents(MapsAliveObjectType.EmailPlainTextForNoChargeOrder, "EmailPlainNoChargeOrder.txt"); }
	}

	public static string EmailPlainTextForRegistration
	{
		get 
		{
			string fileName = "EmailPlainNewRegistration.txt";
			return FileContents(MapsAliveObjectType.EmailPlainTextForNewRegistration, fileName);
		}
	}

	public static string EmitAnalyticsScript()
	{
		return EmitAnalyticsScript(false);
	}

	public static string EmitAnalyticsScript(bool trackLoggedInUsers)
	{
		// Note that the call to this method must be wrapped in Telerik RadCodeBlock tags if the page
		// contains a Telerik control as does the Ready Maps page in the Learning Center (has a TreeView).
		// Without the wrapper you'll get an exception: "The Controls collection cannot be modified because
		// the control contains code blocks". The wrapper looks like this:
		//
		// <telerik:RadCodeBlock runat="server"><%= Utility.EmitAnalyticsScript() %></telerik:RadCodeBlock>
		//
		// Using the wrapper when there is no Telerik control on the page seems to have no effect so we use
		// it everywhere via LeftMenuMaster.

		if (App.DeveloperMode || UserIsMapsAlive)
			return string.Empty;

		try
		{
			bool isLoginPage = HttpContext.Current.Request.Url.AbsolutePath.ToLower().Contains("login.aspx");

			if (isLoginPage || (UserIsLoggedIn && !trackLoggedInUsers))
			{
				// Normally we are only interested in stats for people who have not signed up yet.
				// The exceptions are when a signup or purchase goal has been reached.
				return string.Empty;
			}

			return MapsAliveTourBuilder.Html.AnalyticsScript;
		}
		catch
		{
			return string.Empty;
		}
	}

	public static string EmitTrackerScript()
	{
		if (App.DeveloperMode || UserIsMapsAlive)
			return string.Empty;

		return MapsAliveTourBuilder.Html.TrackerScript;
	}

	public static string EmitTrackerScriptForTourBuilder()
	{
		if (AppContent.Topic("AppTourBuilderHitTracking") == "ON")
			return EmitTrackerScript();
		else
			return "";
	}
	
	public static Byte[] ExceptionImageBytes()
	{
		Byte[] bytes = new Byte[0];
		return bytes;
	}

	public static string ExceptionHtmlString(Exception ex)
	{
		return "<html><head/><body>" + ex.Message + "</body></html>";
	}

	public static Bitmap ExpandBitmap(Bitmap bitmap, Size expandedSize, Color fillColor, ImageExpansionType expansionType)
	{
		if (bitmap.Size.Width > expandedSize.Width || bitmap.Size.Height > expandedSize.Height)
		{
			Bitmap scaledBitmap = Utility.ScaledBitmap(bitmap, expandedSize, true);
			bitmap = scaledBitmap;
		}

		Bitmap expandedBitmap = new Bitmap(expandedSize.Width, expandedSize.Height, PixelFormat.Format24bppRgb);
		expandedBitmap.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

		int x = expansionType == ImageExpansionType.Center ? (expandedSize.Width - bitmap.Size.Width) / 2 : 0;
		int y = expansionType == ImageExpansionType.Center ? (expandedSize.Height - bitmap.Size.Height) / 2 : 0;

		using (Graphics graphics = Graphics.FromImage(expandedBitmap))
		{
			graphics.Clear(fillColor);

			if (expansionType == ImageExpansionType.Repeat)
			{
				while (y < expandedSize.Height)
				{
					x = 0;
					while (x < expandedSize.Width)
					{
						graphics.DrawImage(bitmap, x, y);
						x += bitmap.Width;
					}
					y += bitmap.Height;
				}
			}
			else
			{
				graphics.DrawImage(bitmap, x, y);
			}
		}

		return expandedBitmap;
	}

	private static string FileContents(MapsAliveObjectType type, string fileName)
	{
		string text = (string)MapsAliveState.Retrieve(type);
		if (text == null)
		{
			string fileLocation = FileManager.WebAppFileLocationAbsolute("User", fileName);
			text = FileManager.ReadFileContents(fileLocation);
			MapsAliveState.Persist(type, text);
		}
		return text;
	}

	public static bool FileExtensionIsValid(string ext, string[] list)
	{
		foreach (string s in list)
		{
			if (s.ToLower() == ext.ToLower())
				return true;
		}
		return false;
	}

	public static bool FileExtentionIsJpg(string fileExt)
	{
		string ext = fileExt.ToLower();
		return ext == "jpg" || ext == "jpeg";
	}

	public static string GetOrdinal(int number)
	{
		string ordinal = "th";

		if (number % 10 == 1 && number % 100 != 11)
			ordinal = "st";
		else if (number % 10 == 2 && number % 100 != 12)
			ordinal = "nd";
		else if (number % 10 == 3 && number % 100 != 13)
			ordinal = "rd";

		return ordinal;
	}

	public static Font GetFontForFamilyList(string familyList, float fontSizePx, FontStyle fontStyle)
	{
		Font font = null;
		string[] list = familyList.Split(',');
		foreach (string family in list)
		{
			try
			{
				string familyName = family;
				if (familyName.StartsWith("'"))
					familyName = familyName.Substring(1, familyName.Length - 2);
				font = new Font(familyName, fontSizePx, fontStyle, GraphicsUnit.Pixel);
				if (font.Name == familyName)
					break;
			}
			catch (Exception ex)
			{
				Utility.ReportException("GetFontForFamilyList", ex);
			}
		}
		return font;
	}

	private static bool HandlingDatabaseException
	{
		get
		{
			object o = HttpContext.Current.Session["HandlingDatabaseException"];
			return o != null && (bool)o;
		}
		set { HttpContext.Current.Session["HandlingDatabaseException"] = value; }
	}

	private static bool HandlingException
	{
		get
		{
			object o = HttpContext.Current.Session["HandlingException"];
			return o != null && (bool)o;
		}
		set { HttpContext.Current.Session["HandlingException"] = value; }
	}

	static public string Hash(byte[] data)
	{
		MD5 md5 = new MD5CryptoServiceProvider();
		byte[] result = md5.ComputeHash(data);

		// Build the final string by converting each byte
		// into hex and appending it to a StringBuilder
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < result.Length; i++)
			sb.Append(result[i].ToString("X2"));

		return sb.ToString();
	}

	public static bool HasWidthAndHeight(Size size)
	{
		// We use this method in place of Size.Empty to deal with cases where it is
		// possible for width to be 0 and height to be > 0 and vice versa. In those
		// cases the size will not compare equally to Size.Empty even though from a
		// practical standpoint there is no size when either dimension is 0.
		return size.Width > 0 && size.Height > 0;
	}

	public static Color HexToColor(string hexString)
	{
		//  Translates a html hexadecimal definition of a color into a .NET Framework Color.
		//  The input string must start with a '#' character and be followed by 6 hexadecimal
		//  digits. The digits A-F are not case sensitive. If the conversion was not successfull
		//  the color white will be returned.
		Color color;
		if ((hexString.StartsWith("#")) && (hexString.Length == 7))
		{
			int r = HexToInt(hexString.Substring(1, 2));
			int g = HexToInt(hexString.Substring(3, 2));
			int b = HexToInt(hexString.Substring(5, 2));
			color = Color.FromArgb(r, g, b);
		}
		else
		{
			color = Color.White;
		}
		return color;
	}

	public static int HexToInt(string hexString)
	{
		string hex = hexString;
		if (hex.StartsWith("#"))
			hex = hex.Substring(1);
		return int.Parse(hex, System.Globalization.NumberStyles.HexNumber, null);
	}

	public static int InternetExplorerMajorVersion(HttpRequest request)
	{
		System.Web.HttpBrowserCapabilities browser = request.Browser;
		if (browser != null && browser.Browser == "IE")
			return browser.MajorVersion;
		else
			return 0;
	}

	public static Byte[] ImageFileToByteArray(string fileLocation, out Size size)
	{
		try
		{
			Byte[] bytes;
			Bitmap bitmap = new Bitmap(fileLocation);

			// Get a copy of the bitmap so that the file won't stay locked.
			bitmap = SafeBitmap(bitmap);

			size = bitmap.Size;
			bytes = ImageToByteArray(bitmap, ImageFormat.Jpeg);
			return bytes;

		}
		catch (Exception ex)
		{
			Utility.ReportException("ImageFileToByteArray " + fileLocation, ex);
			size = Size.Empty;
			return new Byte[0];
		}
	}

	public static bool ImageIsCmykOrYcck(System.Drawing.Image image)
	{
		// This information derived from: http://www.maxostudio.com/Tut_CS_CMYK.cfm and http://msdn2.microsoft.com/en-us/library/system.drawing.imaging.imageflags.aspx
		ImageFlags imageFlags = (ImageFlags)Enum.Parse(typeof(ImageFlags), image.Flags.ToString());
		string flags = imageFlags.ToString();
		return flags.Contains("Ycck") || flags.Contains("Cmyk");
	}

	public static Byte[] ImageToByteArray(Bitmap bitmap, ImageFormat imageFormat)
	{
		return ImageToByteArray(bitmap, imageFormat, true);
	}
	
	public static Byte[] ImageToByteArray(Bitmap bitmap, ImageFormat imageFormat, bool reportException)
	{
		Byte[] bytes = new Byte[0];

		if (bitmap != null)
		{
			try
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					bitmap.Save(memoryStream, imageFormat);
					bytes = memoryStream.ToArray();
				}
			}
			catch (Exception ex)
			{
				try
				{
					// Normally we should never get here, but we have seen cases that certain images under
					// certain mysterious conditions cause the infamous "A generic error occurred in GDI+"
					// exception. One possible cause is some kind of file locking issue, though the problem
					// has occurred when no file (only memory streams) is involved. Rather than just bail
					// out, we try again but this time using a copy of the bitmap that is created per the
					// recommendations from Microsoft. For awhile we were just always using the safe bitmap
					// when a user uploaded an image, but SafeBitmap causes some image degradation, not to
					// mention it loses the EXIF data, and so that was not a good solution. Hopefully this
					// will work well all of the time and catch the oddball case where there's a problem.
					bitmap = SafeBitmap(bitmap);
					using (MemoryStream memoryStream = new MemoryStream())
					{
						bitmap.Save(memoryStream, imageFormat);
						bytes = memoryStream.ToArray();
					}
				}
				catch (Exception)
				{
					if (reportException)
						ReportException("ImageToByteArray", string.Format("{0}", bitmap.Size.ToString()), ex);
					bytes = null;
				}
			}
		}
		return bytes;
	}

	public static void ImitateUser(string email)
	{
		Cache cache = HttpContext.Current.Cache;
		if (email == null)
		{
			cache.Remove(ImitatedUserKey);
			MapsAliveState.FlushSessionState();
		}
		else
		{
			cache.Insert(ImitatedUserKey, email, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(15));
		}
	}

	private static string ImitatedUserKey
	{
		get
		{
			HttpSessionState session = HttpContext.Current.Session;
			return session == null ? string.Empty : "User" + session.SessionID;
		}
	}

	public static bool ImitatingUser()
	{
		HttpContext context = HttpContext.Current;
		return context.Cache[ImitatedUserKey] != null;
	}

	public static void InitImageButton(ImageButton button, string name)
	{
		InitImageButton(button, name, true);
	}

	public static void InitImageButton(ImageButton button, string name, bool enabled)
	{
		string buttonImageName = "Btn" + name;
		button.ImageUrl = string.Format("~/Images/{0}{1}.gif", buttonImageName, enabled ? "1" : "0");
		button.Attributes.Add("class", enabled ? "buttonEnabled" : "buttonDisabled");
		string script = "maOnRollover({0}, this, '" + buttonImageName + "');";
		button.Attributes.Add("onmouseover", string.Format(script, "true"));
		button.Attributes.Add("onmouseout", string.Format(script, "false"));
		button.Enabled = enabled;
	}

	public static void InitReadyMapsTree(RadTreeView treeView)
	{

		// See if the Ready Maps tree XML is already in memory.
		// Note that the Ready Maps XML is currently persisted in session memory so that
		// we don't have to keep restarting the app or flushing the cache during devlopment.
		// Eventually it should be moved to application cache so that it's only created
		// once for all users.
		string xml = (string)MapsAliveState.Retrieve(MapsAliveObjectType.ReadyMapsXml);

		if (xml == null)
		{
			// The XML is not in memory. Get it from the file.
			string fileLocation = Path.Combine(App.AppRuntimeFolderRoot, "ReadyMaps/ReadyMaps.xml");
			treeView.LoadContentFile(fileLocation);
			
			// Decorate the XML with image and file URLs.
			foreach (RadTreeNode node in treeView.GetAllNodes())
			{
				bool isShapesNode = node.Category == "shapes";
				bool isMapNode = node.Category == "map";

				if (isShapesNode)
					node.ImageUrl = "~/Images/ReadyMapsShapes.png";
				else if (isMapNode)
					node.ImageUrl = "~/Images/ReadyMapsMap.png";

				RadTreeNode parentNode = node.ParentNode;

				if (isMapNode || isShapesNode)
				{
					string newValue = string.Empty;
					while (parentNode != null)
					{
						string path = parentNode.Attributes["Path"];
						newValue = path + "\\" + newValue;
						parentNode = parentNode.ParentNode;
					}
					
					string filePath = newValue + node.Value;

					fileLocation = Path.Combine(FileManager.AppRuntimeFolderLocationAbsolute, filePath);
					node.Value = fileLocation;
					Debug.Assert(FileManager.FileExists(fileLocation), "Ready Map file not found: " + fileLocation);
				}
			}

			// Set the skin and the event handlers so that they get written to the XML too.
			treeView.Skin = "Vista";
			treeView.OnClientMouseOver = "maReadyMapsOnMouseOver";
			treeView.OnClientMouseOut = "maReadyMapsOnMouseOut";
			treeView.OnClientNodeClicking = "onNodeClicking";
			treeView.OnClientNodeExpanding =" maReadyMapsOnNodeExpanding";
			
			// Put the XML in cache.
			MapsAliveState.Persist(MapsAliveObjectType.ReadyMapsXml, treeView.GetXml());
		}
		else
		{
			// The XML is already in cache.
			treeView.LoadXmlString(xml);
		}
	}

	public static bool IsAlphaNumeric(string text)
	{
		Regex pattern = new Regex("[^a-zA-Z0-9]");
		return !pattern.IsMatch(text);
	}


	public static bool IsLegalXmlChar(int c)
	{
		// Prevent exceptions like "hexadecimal value 0x0B, is an invalid character" that will
		// be raised below by xmlWriter.WriteStartElement if the XML contains bad characters.
		// This can happen if the user copy/pastes into the hotspot editor from e.g. Word.
		// See this article for more info: http://seattlesoftware.wordpress.com/2008/09/11/hexadecimal-value-0-is-an-invalid-character

		return
		(
			 c >= 0x20 ||
			 c == 0x9 ||	// \t
			 c == 0xA ||	// \n
			 c == 0xD		// \r
		);
	}

	public static bool IsValidFileName(string id)
	{
		return id.IndexOfAny(new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' }) == -1;
	}

	public static string JavascriptDoubleQuotedString(string s)
	{
		// Escape double quotes.
		string text = s.Replace("\"", "\\\"");

		return text;
	}

	public static string JavascriptSingleQuotedString(string s)
	{
		// Escape single quotes.
		string text = s.Replace("'", @"\'");
		
		// Replace new lines with spaces. Note that new lines are not allowed in Javascript strings
		// which is the primary reason we eliminate them. Also, new lines in HTML are rendered as
		// a single space which is why we don't remove them all together but change to a space.
		text = text.Replace(CrLf, " ");
		
		return text;
	}

	public static MemberPageActionId LastAction
	{
		get
		{
			object action = MapsAliveState.Retrieve(MapsAliveObjectType.LastAction);
			if (action == null)
				return MemberPageActionId._NotSet;
			else
				return (MemberPageActionId)action;
		}
	}

	public static MemberPageActionId LastPageAction
	{
		get
		{
			object action = MapsAliveState.Retrieve(MapsAliveObjectType.LastPageAction);
			if (action == null)
				return MemberPageActionId.Map;
			else
				return (MemberPageActionId)action;
		}
		set
		{
			MapsAliveState.Persist(MapsAliveObjectType.LastPageAction, value);
			MapsAliveState.Persist(MapsAliveObjectType.LastAction, value);
		}
	}

	public static MemberPageActionId LastViweAction
	{
		get
		{
			object action = MapsAliveState.Retrieve(MapsAliveObjectType.LastViewAction);
			if (action == null)
				return MemberPageActionId.EditHotspotContent;
			else
				return (MemberPageActionId)action;
		}
		set
		{
			MapsAliveState.Persist(MapsAliveObjectType.LastViewAction, value);
			MapsAliveState.Persist(MapsAliveObjectType.LastAction, value);
		}
	}

	public static string MailToSupportLink(string subject)
	{
		return string.Format("<a href=\"mailto:support@mapsalive.com?subject={0}\">support@mapsalive.com</a>", subject);
	}

	public static Size MaxImageSizeForInfoPage
	{
		get { return new Size(2400, 2400); }
	}

	public static Size MaxImageSizeForMapPage
	{
		get { return new Size(2400, 2400); }
	}

	public static string MemberPageLink(string linkText, MemberPageActionId actionId)
	{
		string url = MemberPageAction.ActionPageTarget(actionId);
		return string.Format("<a href=\"{0}\">{1}</a>", url, linkText);
	}

	public static string NumberOrNo(int x)
	{
		return x == 0 ? "no" : x.ToString();
	}

	public static int PercentToPixel(double pct, int dimension)
	{
		// Doubles have so much precision that most of the time we get back a whole
		// number for a pixel value, but once in awhile the pixel value comes back
		// as something like 123.0000000003 or 122.999999999876.  To handle those
		// cases we round the result to the nearest whole number.
		return (int)(Math.Round(pct * dimension));
	}

	public static double PixelToPercent(int px, int dimension)
	{
		return (double)px / (double)dimension;
	}

	public static string Plural(int x)
	{
		return x == 0 || x > 1 ? "s" : string.Empty;
	}

	public static void PreventPageCaching(HttpResponse response)
	{
		// Prevent pages from being cached to make it impossible for a user to view
		// stale data by using their browser history or back button.  These settings
		// tell the browser to always hit the server to get the page.  They work with
		// IE and Firefox, but not with Safari and we don't know a Safari solution.
		response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
		response.Cache.SetCacheability(HttpCacheability.NoCache);
		response.Cache.SetNoStore();
	}

	public static string QuickHelpText(string topic)
	{
		return QuickHelpText(topic, string.Empty);
	}

	public static string QuickHelpText(string topic, string details)
	{
		return QuickHelpText(topic, details, false);
	}

	public static string QuickHelpText(string topic, string details, bool optional)
	{
		string quickHelpText;
		if (optional)
		{
			quickHelpText = AppContent.TopicOptional(topic);
			if (quickHelpText.Length == 0)
				return string.Empty;
		}
		else
		{
			quickHelpText = AppContent.Topic(topic) + details;
		}

		if (UserIsMapsAlive || App.DeveloperMode)
			quickHelpText += "<div class='quickHelpTopic'>" + topic + "</div>";
		quickHelpText = quickHelpText.Replace("'", "\\'");
		quickHelpText = quickHelpText.Replace(CrLf, "");
		return quickHelpText;
	}

	public static void RecordAction(MemberPageActionId actionId)
	{
		if (Utility.ImitatingUser())
			return;

		try
		{
			ActionQueue actionQueue = (ActionQueue)MapsAliveState.Retrieve(MapsAliveObjectType.ActionQueue);
			if (actionQueue == null)
			{
				actionQueue = new ActionQueue(128);
				MapsAliveState.Persist(MapsAliveObjectType.ActionQueue, actionQueue);
			}
			actionQueue.Add(actionId);

			if (actionId != MemberPageActionId.HomePage)
			{
				// Make sure that no one else has logged into this account.
				Account account = MapsAliveState.Account;
				MapsAliveDataRow row = MapsAliveDatabase.LoadDataRow("sp_Account_GetSessionId", "@AccountId", account.Id);
				if (row != null)
				{
					string sessionIdFromDatabase = row.StringValue("SessionId");
					if (sessionIdFromDatabase != string.Empty && sessionIdFromDatabase != HttpContext.Current.Session.SessionID)
					{
						account.Logout();
						HttpContext.Current.Response.Redirect(App.WebSitePathUrl(string.Format("User/ForcedLogout.aspx?id={0}", account.Id)));
					}
				}
			}
		}
		catch
		{
			// This will catch a thread-aborted exception if the user is redirected to the forced logout page.
		}
	}

	public static void RegisterClientJavaScript(Page page, RuntimeFile runtimeFile)
	{
		string script = "webSiteUrl = '" + App.WebSiteUrl + "';" + " var appVersion = '" + App.Version + "';";
        page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Utility.cs loading", script, true);

        // Emit includes for .js files. Use the AppRuntime folder root rather than the web site URL
		// because the root is a relative reference where is the URL is absolute. A relative reference
		// automatically "inherits" the security of the page which means the include works equally well
		// on either an http or https page. If instead  we use the http URL, we get one of those "this page
		// contains both secure and unsecure items" which causes the page to not be secure at all.
		page.ClientScript.RegisterClientScriptInclude(runtimeFile.ToString(),
			App.AppRuntimeFolderRoot + App.Version + "/" + Runtime.RuntimeFileName(runtimeFile));
	}

	public static void RegisterMapEditorJavaScript(Page page, string path, string fileName)
	{
		string script = "webSiteUrl = '" + App.WebSiteUrl + "';" + " var appVersion = '" + App.Version + "';";
		page.ClientScript.RegisterClientScriptBlock(page.GetType(), "Utility.cs loading", script, true);
		page.ClientScript.RegisterClientScriptInclude(fileName, path + "/" + fileName);
	}

	public static void RegisterMemberPageJavaScript(Page page)
	{
		RegisterClientJavaScript(page, RuntimeFile.PublicPageJs);
		RegisterClientJavaScript(page, RuntimeFile.MemberPageJs);
	}

	public static void RegisterPublicPageJavaScript(Page page)
	{
		RegisterClientJavaScript(page, RuntimeFile.PublicPageJs);
	}

	public static void RegisterColorChooserJavaScript(Page page)
	{
		RegisterClientJavaScript(page, RuntimeFile.ColorChooserJs);
	}

    public static void RegisterHtmlEditorJavaScript(Page page)
    {
        RegisterClientJavaScript(page, RuntimeFile.HtmlEditorJs);
    }

	public static void RegisterLayoutEditorJavaScript(Page page)
	{
		RegisterClientJavaScript(page, RuntimeFile.LayoutEditorJs);
	}

	public static void RegisterMapEditorJavaScript(Page page)
	{
		RegisterClientJavaScript(page, RuntimeFile.MapEditorJs);
	}

    public static void ReportError(string subject, string msg)
	{
		if (App.DeveloperMode)
		{
			Debugger.Break();
		}
		else
		{
			SendEmail(subject, msg, true);
		}
	}

	public static void ReportEvent(string subject, string msg)
	{
		if (App.DeveloperMode)
		{
			Debugger.Break();
		}
		else
		{
			SendEmail(subject, msg, false);
		}
	}

	public static void ReportDatabaseException(string subject, Exception ex)
	{
		if (HandlingDatabaseException)
		{
			// Exception occcurred while handling an exception.
			Debugger.Break();
			return;
		}

		HandlingDatabaseException = true;
		ReportException(subject, null, ex);
		HandlingDatabaseException = false;
	}

	public static void ReportException(string subject, Exception ex)
	{
		ReportException(subject, null, ex);
	}

	public static void ReportException(string subject, string info, Exception ex)
	{
		if (HandlingException)
		{
			// Exception occcurred while handling an exception.
			Debugger.Break();
			return;
		}

		if (ex is System.Threading.ThreadAbortException)
		{
			// The current page has been aborted and we are redirecting to another page so don't report
			// this exception. The most common case where this occurs is when a user is being forced to
			// logout because they or someone else logged into the same account from another browser.
			
            //ReportEvent("Handled ThreadAbortException", string.Format("A ThreadAbortException is being ignored.\nSubject:{0}\nInfo:{1}", subject, info));
			return;
		}

		HandlingException = true;

		string msg = subject + "\n\n";

		if (info != null)
		{
			msg += "INFO: " + info + "\n\n";
		}

		if (ex == null)
		{
			msg = string.Format("{0}: {1}", ex.GetType().Name, ex.Message);
		}
		else
		{
			msg += "EXCEPTION MESSAGE: " + (ex.Message.Length > 0 ? ex.Message : "<no message>") + "\n";

			if (ex.InnerException != null && ex.InnerException.Message != null && ex.InnerException.Message.Length != 0 && ex.InnerException.Message != ex.Message)
			{
				// Only show the inner exception message if exists and is different than the exception message.
				msg += "INNER EXCEPTION MESSAGE: " + ex.InnerException.Message + "\n";
			}

			msg += "\nEXCEPTION STACK TRACE:\n" + ex.StackTrace + "\n";
		}
		
		ReportError(subject, msg);
		
		HandlingException = false;
	}

	private static Bitmap SafeBitmap(Bitmap bitmapOriginal)
	{
		// This method addresses the problem of an image file getting locked.
		// See this article for details: http://support.microsoft.com/kb/814675

		Bitmap bitmapCopy;

		if (bitmapOriginal.PixelFormat == PixelFormat.Format8bppIndexed ||
			bitmapOriginal.PixelFormat == PixelFormat.Format4bppIndexed ||
			bitmapOriginal.PixelFormat == PixelFormat.Format1bppIndexed)
		{
			// For now return the original bitmap because we just can't get the code below to work
			// correctly. To debug I wrote both bitmaps to files and learned that the color table in the copied file
			// does not match the original and so the colors and transparency in the copy are not correct and the
			// copy is much larger. Also the ImageFormat of the copy is memory bitmap (b96b3caa) whereas the original
			// is gif (b96b3cb0).
			return bitmapOriginal;

			/*
			Rectangle rect = new Rectangle(0, 0, bitmapOriginal.Width, bitmapOriginal.Height);
			BitmapData bitmapData1 = bitmapOriginal.LockBits(rect, ImageLockMode.ReadOnly, bitmapOriginal.PixelFormat);
			int length = bitmapData1.Stride * bitmapOriginal.Height;

			bitmapCopy = new Bitmap(bitmapOriginal.Width, bitmapOriginal.Height, bitmapOriginal.PixelFormat);
			BitmapData bitmapData2 = bitmapCopy.LockBits(rect, ImageLockMode.WriteOnly, bitmapOriginal.PixelFormat);

			byte[] bytes = new byte[length];
			Marshal.Copy(bitmapData1.Scan0, bytes, 0, length);
			Marshal.Copy(bytes, 0, bitmapData2.Scan0, length);

			bitmapOriginal.UnlockBits(bitmapData1);
			bitmapCopy.UnlockBits(bitmapData2);

			//bitmapCopy.Save(@"C:\AvantLogic\MapsAliveWebApp\AppRuntime\ggg.gif", ImageFormat.Gif);
			*/
		}
		else
		{
			try
			{
				bitmapCopy = new Bitmap(bitmapOriginal.Width, bitmapOriginal.Height, bitmapOriginal.PixelFormat);
				bitmapCopy.SetResolution(bitmapOriginal.HorizontalResolution, bitmapOriginal.VerticalResolution);

				using (Graphics graphics = Graphics.FromImage(bitmapCopy))
				{
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
					graphics.SmoothingMode = SmoothingMode.AntiAlias;
					graphics.CompositingQuality = CompositingQuality.HighQuality;
					graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

					graphics.DrawImage(bitmapOriginal, 0, 0);
				}

				bitmapOriginal.Dispose();
				return bitmapCopy;

			}
			catch (Exception ex)
			{
				// This should never happen, but it has occurred, most recently when importing an archive with
                // a 4MB map image. However, it only happened on the development server and not on the production
                // server. Return the original bitmap and hope that it won't cause a locking problem.
				ReportException("SafeBitmap", ex);
                bitmapOriginal.Dispose();
                return bitmapOriginal;
			}
		}
	}

	public static string GraphicImageFileName(int sampleId)
	{
		return string.Format("graphic{0}.png", sampleId.ToString());
	}
	public static string SampleImageFileName(int sampleId)
	{
		string imageId = sampleId < 10 ? "0" + sampleId : sampleId.ToString();
		string fileName = string.Format("sample{0}.jpg", imageId);
		return fileName;
	}

	public static string SampleThumbFileName(int sampleId)
	{
		string imageId = sampleId < 10 ? "0" + sampleId : sampleId.ToString();
		string fileName = string.Format("thumb{0}.jpg", imageId);
		return fileName;
	}

	public static Bitmap ScaledBitmap(Bitmap bitmap, Size containerSize, bool disposeOriginalBitmap)
	{
		Size imageSize = bitmap.Size;
		
		// Determine the size of the scaled image.
		Size scaledSize = ScaledImageSize(imageSize, containerSize);

		// Don't scale if the image already fits within the container.
		if (scaledSize == imageSize)
		{
			if (disposeOriginalBitmap)
			{
				// Return the bitmap that was passed in instead of creating a new bitmap and then
				// disposing of the original. If we were to create a new bitmap, the "copy" would
				// not preserve the original's EXIF data and the copy's quality would be slightly
				// degraded as happens when you save a jpeg.
				return bitmap;
			}
			else
			{
				// The caller wants to keep using the original bitmap and they want a scaled version.
				// Return a copy of the original so that there are now two bitmaps, either of which can
				// be disposed when necessary without affecting the other.
				return new Bitmap(bitmap);
			}
		}
		
		int imageWidth = bitmap.Width;
		int imageHeight = bitmap.Height;
		int scaledWidth = scaledSize.Width;
		int scaledHeight = scaledSize.Height;

		// Create a new bitmap having the same resolution as the original.
		Bitmap scaledBitmap = new Bitmap(scaledWidth, scaledHeight, PixelFormat.Format24bppRgb);
		scaledBitmap.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

		// Scale the image.
		using (Graphics graphics = Graphics.FromImage(scaledBitmap))
		{
			graphics.Clear(Color.White);
			
			// Set interpolation for a high quality image and pixel offset to avoid
			// the problem where lighter color edge appears all around the image.
			graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
			
			graphics.DrawImage(bitmap,
				new Rectangle(0, 0, scaledWidth, scaledHeight),
				new Rectangle(0, 0, imageWidth, imageHeight),
				GraphicsUnit.Pixel);
		}

		if (disposeOriginalBitmap)
		{
			// The caller no longer has a need for the original bitmap so we 
			// dispose of it here so that every caller doesn't have to do it.
			bitmap.Dispose();
		}

		return scaledBitmap;
	}

	public static Byte[] ScaledImageBytes(ref Byte[] originalBytes, Size requiredSize)
	{
		Bitmap bitmap;

		// Construct the original image from its data.
		using (MemoryStream memoryStream = new MemoryStream(originalBytes))
		{
			bitmap = (Bitmap)Bitmap.FromStream(memoryStream);
		}

		// Don't scale if the image is already equal to or smaller than the required size.
		if (bitmap.Width <= requiredSize.Width && bitmap.Height <= requiredSize.Height )
			return originalBytes;

		// Create a properly scaled version of the original image.
		Bitmap scaledBitmap = ScaledBitmap(bitmap, requiredSize, true);
		bitmap = scaledBitmap;
		Byte[] scaledBytes = ImageToByteArray(bitmap, ImageFormat.Jpeg);

		return scaledBytes;
	}

	public static Size ScaledImageSize(Size imageSize, Size containerSize)
	{
		int containerWidth = containerSize.Width;
		int containerHeight = containerSize.Height;
		int imageWidth = imageSize.Width;
		int imageHeight = imageSize.Height;

		// Don't scale if the image already fits within the container.
		if (imageWidth <= containerWidth && imageHeight <= containerHeight)
			return imageSize;

		// Determine the scaling factor needed to reduce the image size
		// while still preserving its original aspect ratio.  The value we
		// use depends on which dimension (width or height) has to be reduced
		// the most in order to make the image fit within its container.
		double scalingFactor;
		double scalingFactorW = ((double)containerWidth / (double)imageWidth);
		double scalingFactorH = ((double)containerHeight / (double)imageHeight);
		if (scalingFactorH < scalingFactorW)
			scalingFactor = scalingFactorH;
		else
			scalingFactor = scalingFactorW;

		int scaledWidth;
		int scaledHeight;
		
		// Calculate the scaled image's dimensions.  If one of the dimensions will
		// be smaller than the container, we calculate it.  We could simply calculate
		// both dimensions, but rounding when converting from double to int can cause
		// the result to be off by a pixel.
		if (scalingFactorH == scalingFactorW)
		{
			scaledWidth = containerWidth;
			scaledHeight = containerHeight;
		}
		else if (scalingFactorH < scalingFactorW)
		{
			scaledWidth = (int)Math.Ceiling((imageWidth * scalingFactor));
			scaledHeight = containerHeight;
		}
		else
		{
			scaledWidth = containerWidth;
			scaledHeight = (int)Math.Ceiling((imageHeight * scalingFactor));
		}

		// Very wide or tall images that are scaled to a very small size
		// can lose a dimension. Make sure we always return a valid size.
		if (scaledWidth == 0)
			scaledWidth = 1;
		if (scaledHeight == 0)
			scaledHeight = 1;

		return new Size(scaledWidth, scaledHeight);
	}

	private static bool SendEmail(string subject, string msg, bool isErrorReport)
	{
		string userName = UserName;
		string tourName = null;
		string tourPageName = null;
		string tourViewTitle = null;

		if (userName != null)
		{
			try
			{
				// Get the tour from memory intead of calling SelectedTourOrNull to avoid
				// a call to the database and possibly triggering an error while sending mail.
				Tour tour = (Tour)MapsAliveState.Retrieve(MapsAliveObjectType.Tour);

				if (tour != null)
				{
					tourName = tour.Id + ", " + tour.Name;
					TourPage tourPage = tour.SelectedTourPage;
					if (tourPage != null)
					{
						tourPageName = tourPage.Id + ", " + tourPage.Name;
					}
					TourView tourView = tour.SelectedTourView;
					if (tourView != null)
					{
						tourViewTitle = tourView.Id + ", " + tourView.Title;
					}
				}
			}
			catch
			{
				tourName = null;
			}
		}

		string subjectText = string.Format("{0} ({1})", subject, App.WebSiteUrlShort);

		string messageBody =
			"Time: " + DateTime.Now + "\n";

		// Get the tour from memory intead of calling MapsAliveState.Account to avoid
		// a call to the database and possibly triggering an error while sending mail.
		Account account = (Account)MapsAliveState.Retrieve(MapsAliveObjectType.Account);
		
		if (account != null)
			messageBody += "Acct: " + account.Id + "\n";

		if (userName != null)
			messageBody += "User: " + userName + "\n";

		if (tourName != null)
			messageBody += "Tour: " + tourName + "\n";

		if (tourPageName != null)
			messageBody += "Page: " + tourPageName + "\n";

		if (tourViewTitle != null)
			messageBody += "View: " + tourViewTitle + "\n";

		messageBody += "\n" + msg;

		if (isErrorReport)
		{
			subjectText = "ERROR - " + subjectText;
			if (HandlingDatabaseException)
				subjectText = "HANDLED DATABASE " + subjectText;

            messageBody += "\n" + CreateErrorDump();
            WriteErrorToLogFile(messageBody);
		}

		return SendEmail(EmailForSupport, subjectText, messageBody, false);
	}

    public static SmtpClient CreateSmtpClient()
    {
        // Get the STMP settings from web.config.
        System.Net.Configuration.SmtpSection smtpSection = (System.Net.Configuration.SmtpSection)System.Configuration.ConfigurationManager.GetSection("system.net/mailSettings/smtp");

        SmtpClient smtpClient = new SmtpClient();
        smtpClient.UseDefaultCredentials = smtpSection.Network.DefaultCredentials;
        smtpClient.Port = smtpSection.Network.Port;
        smtpClient.EnableSsl = true;
        smtpClient.Credentials = new System.Net.NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);
        return smtpClient;
    }

    public static bool SendEmail(string to, string subject, string body, bool ccSupport)
	{
		if (App.MapsAliveConfig.SmtpDisabled)
			return true;

		MailMessage mailMessage = new MailMessage(EmailForSupport, to, subject, body);

        if (ccSupport)
        {
            mailMessage.Bcc.Add(new MailAddress(EmailForSupport));
        }

        SmtpClient smtpClient = CreateSmtpClient();

        try
        {
			smtpClient.Send(mailMessage);
            WriteToLogFile(string.Format("Email sent to {0}, \"{1}\"", to, subject));
			return true;
		}
		catch (Exception ex)
		{
            Utility.ReportException("SendEmail " + to, ex);
            return false;
		}
	}

	public static void SendEmailToAdmin(string subject, string msg)
	{
		SendEmail(subject, msg, false);
	}

	public static void SendEmailToCustomer(string email, string subject, string body)
	{
		SendEmailToCustomer(email, subject, body, true);
	}

	public static void SendEmailToCustomer(string email, string subject, string body, bool ccSupport)
	{
		if (App.DeveloperMode)
		{
			// Don't sent emails to customers while developing because most of the addresses we use are
			// bogus and so they bounce, and when we are working with actual customer addresses, we don't
			// want to be sending them test messages.
			return;
		}

		SendEmail(email, subject, body, ccSupport);
	}

	public static void SendEmailToSupport(string subject, string body)
	{
        // No longer send emails from the app to support. Send to admin instead.
        // This way the only emails in the support inbox will be from customers.
        SendEmailToAdmin(subject, body);
	}

	public static void SetDivText(HtmlGenericControl div, string text)
	{
		div.InnerHtml = text;
	}

	public static bool SharpenMapInsetImage(Bitmap bitmap)
	{
		// Only sharpen very small images.  Larger images get oversharpened by this code.
		if (bitmap.Width > 100 && bitmap.Height > 100)
			return false;

		// From http://aspalliance.com/cookbook/ViewChapter.aspx?Chapter=22
		int repetitions = 1;
		int red = 0;
		int green = 0;
		int blue = 0;

		for (int i = 0; i < repetitions; i++)
		{
			for (int x = 3; x < (bitmap.Width - 3); x++)
			{
				for (int y = 3; y < (bitmap.Height - 3); y++)
				{
					// red
					red += -1 * bitmap.GetPixel(x + 1, y + 1).R;
					red += -2 * bitmap.GetPixel(x, y - 1).R;
					red += -1 * bitmap.GetPixel(x + 1, y - 1).R;
					red += -2 * bitmap.GetPixel(x - 1, y).R;
					red += 16 * bitmap.GetPixel(x, y).R;
					red += -2 * bitmap.GetPixel(x + 1, y).R;
					red += -1 * bitmap.GetPixel(x - 1, y + 1).R;
					red += -2 * bitmap.GetPixel(x, y + 1).R;
					red += -1 * bitmap.GetPixel(x + 1, y + 1).R;
					red /= 4;
					if (red > 255) red = 255;
					if (red < 0) red = 0;

					// green
					green += -1 * bitmap.GetPixel(x + 1, y + 1).G;
					green += -2 * bitmap.GetPixel(x, y - 1).G;
					green += -1 * bitmap.GetPixel(x + 1, y - 1).G;
					green += -2 * bitmap.GetPixel(x - 1, y).G;
					green += 16 * bitmap.GetPixel(x, y).G;
					green += -2 * bitmap.GetPixel(x + 1, y).G;
					green += -1 * bitmap.GetPixel(x - 1, y + 1).G;
					green += -2 * bitmap.GetPixel(x, y + 1).G;
					green += -1 * bitmap.GetPixel(x + 1, y + 1).G;
					green /= 4;
					if (green > 255) green = 255;
					if (green < 0) green = 0;

					// blue
					blue += -1 * bitmap.GetPixel(x + 1, y + 1).B;
					blue += -2 * bitmap.GetPixel(x, y - 1).B;
					blue += -1 * bitmap.GetPixel(x + 1, y - 1).B;
					blue += -2 * bitmap.GetPixel(x - 1, y).B;
					blue += 16 * bitmap.GetPixel(x, y).B;
					blue += -2 * bitmap.GetPixel(x + 1, y).B;
					blue += -1 * bitmap.GetPixel(x - 1, y + 1).B;
					blue += -2 * bitmap.GetPixel(x, y + 1).B;
					blue += -1 * bitmap.GetPixel(x + 1, y + 1).B;
					blue /= 4;
					if (blue > 255) blue = 255;
					if (blue < 0) blue = 0;

					bitmap.SetPixel(x, y, Color.FromArgb(red, green, blue));

					red = 0;
					green = 0;
					blue = 0;
				}
			}
		}
		return true;
	}

	public static bool ThrowExceptionOnDatabaseError
	{
		get
		{
			object o = HttpContext.Current.Session["ThrowExceptionOnDatabaseError"];
			return o != null && (bool)o;
		}
		set { HttpContext.Current.Session["ThrowExceptionOnDatabaseError"] = value; }
	}

	public static void Trace(string msg)
	{
		Debug.WriteLine("Trace: " + msg);
	}

	public static void TransferToHomePage()
	{
		HttpContext.Current.Response.Redirect(App.WebSitePathUrl("Default.aspx"));
	}

	public static void TransferToConnectionDroppedPage(Exception ex)
	{
		if (ThrowExceptionOnDatabaseError)
			throw ex;
		else
			HttpContext.Current.Response.Redirect(App.WebSitePathUrl("User/ErrorConnectionDropped.aspx"));
	}

	public static void UpdateUserLastActivityDate()
	{
		string userName = UserName;
		if (userName != null)
			MapsAliveDatabase.ExecuteStoredProcedure("sp_Account_UpdateLastActivityDate", "@UserName", userName);
	}

	public static bool UrlFound(string url)
	{
		if (string.IsNullOrEmpty(url))
			return false;

		bool found = true;
		try
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

		}
		catch (WebException ex)
		{
			HttpWebResponse response = (HttpWebResponse)ex.Response;
            if (response != null)
            {
			    HttpStatusCode code = response.StatusCode;
			    found = code != HttpStatusCode.NotFound;
            }
            else
            {
                found = false;
            }
		}
		catch
		{
			found = false;
		}
		return found;
	}

	public static string UserEmail
	{
		get { return CurrentUser.Email; }
	}

	public static Guid UserId
	{
		get { return (Guid)CurrentUser.ProviderUserKey; }
	}

	private static bool UserHasRole(string roleName)
	{
		bool userHasRole = false;
		if (UserIsLoggedIn)
		{
			string[] roles = Roles.GetRolesForUser();
			foreach (string role in roles)
			{
				if (role == roleName)
				{
					userHasRole = true;
					break;
				}
			}
		}
		return userHasRole;
	}

	public static bool UserHasAdminRole(string userName)
	{
		if (UserIsLoggedIn)
		{
			string[] roles = Roles.GetRolesForUser(userName);
			foreach (string role in roles)
			{
				if (role == "administrator")
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool UserIsAdmin
	{
		get	{ return UserHasRole("administrator"); }
	}

	public static bool UserIsMapsAlive
	{
		get { return UserIsAdmin; }
	}

	public static bool UserIsLoggedIn
	{
		get { return CurrentUser != null; }
	}

	public static string UserName
	{
		get
		{
			MembershipUser user = CurrentUser;
			return user?.UserName;
		}
	}

    public static void WriteErrorToLogFile(string text)
    {
        WriteToLogFile(text, App.MapsAliveConfig.LogFile, true);
    }

    public static void WriteToLogFile(string text)
    {
        WriteToLogFile(text, App.MapsAliveConfig.LogFile, false);
    }

    public static void WriteToLogFile(string text, string logFileNamePattern, bool isErrorReport)
    {
        if (logFileNamePattern.Length == 0)
            return;

        try
        {
            // Insert the date into the log file name pattern which contains "{0}" where the date goes.
            string day = DateTime.Now.ToString("yyyy-MM-dd");
            string logFileName = string.Format(logFileNamePattern, day);

            // Prepend a timestamp to the text to be logged.
            text = string.Format("{0} : {1}\n", DateTime.Now.ToLongTimeString(), text);

            if (isErrorReport)
                text += CreateErrorDump();

            // Append the text to the end of the log file. If the file does not exist, it will get created.
            const bool appendToFile = true;
            StreamWriter streamWriter = new StreamWriter(logFileName, appendToFile);
            streamWriter.Write(text);
            streamWriter.Close();
        }
        catch
        {
        }
    }

    public static bool ValidEmailAddress(string email)
	{
		try
		{
			MailMessage mailMessage = new MailMessage("name@domain.com", email);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static bool ValidPassword(string password)
	{
		string regExPattern = "^.{5,16}$";
		Regex regEx = new Regex(regExPattern);
		return regEx.IsMatch(password);
	}
}
