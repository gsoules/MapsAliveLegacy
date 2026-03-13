// Copyright (C) 2003-2010 AvantLogic Corporation
using System.Collections;

// Do not change these numbers because they are hard-coded in mapsalive.js and
// mapsalive-tour.js. Changing them will break V3 compatibility mode.
public enum RuntimeFile
{
	Deprecated1 = 0,
	Deprecated2 = 1,
	MapViewerJs = 9,
	MapsAliveJs = 10,
	MemberPageJs = 11,
	PublicPageJs = 12,
	ColorChooserJs = 13,
	LayoutEditorJs = 14,
	LiveDataJs = 15,
	SoundManagerJs = 16,
	Deprecated3 = 17,
	Deprecated4 = 20,
	LoadingLiveData = 21,
    Deprecated5 = 22,
	Blank = 23,
	CloseX = 24,
	Pin1 = 25,
	Pin2 = 26,
	Pin1Animated = 27,
	CloseTouchX = 28,
	CloseHelpX = 29,
	ArrowLeft1 = 30,
	ArrowLeft2 = 31,
	ArrowRight1 = 32,
	ArrowRight2 = 33,
	ArrowUp1 = 34,
	ArrowUp2 = 35,
	ArrowDown1 = 36,
	ArrowDown2 = 37,
	DownloadReadMe = 41,
	DirSortAZ = 50,
	DirGroup = 51,
	HammerJs = 60,
    HtmlEditorJs = 61,
    MapEditorJs = 70,
    MapsAliveLoaderJs = 71,
    MapsAliveApiJs = 73,
    MapsAliveTourJs = 74,
    MapsAlivePageJs = 75,
    MapsAliveMapJs = 76,
    MapsAliveLayoutJs = 77,
    MapsAliveMarkerJs = 78,
    MapsAliveDirectoryJs = 79,
    MapsAlivePopupJs = 80,
    MapsAliveViewJs = 81,
    PopupCloseX = 82,
    MapsAliveRuntimeJs = 83,
    MapsAliveLiveDataJs = 84,
    MapsAliveMediaJs = 85,
    MapsAliveGraphicsJs = 86,
    ContentExpand = 87,
    ContentContract = 88,
    NavButton = 89,
    MobileCloseX = 90,
    DirContract = 91,
    DirExpand = 92,
    DirSearch = 93,
    CurrentPage = 94,
    ZoomIn = 95,
    ZoomOut = 96,
    Offline = 97,
    HelpButton = 98
}

public class Runtime
{
	public static string ProjectFileName(RuntimeFile runtimeFile)
	{
		switch (runtimeFile)
		{
			case RuntimeFile.MapsAliveJs: return "mapsalive.js";
			case RuntimeFile.MapViewerJs: return "mapviewer.js";
			case RuntimeFile.MapsAliveLoaderJs: return TourBuilder.PatternForTourLoaderJsFile;
			case RuntimeFile.MapsAliveApiJs: return "mapsalive-api.js";
			case RuntimeFile.MapsAliveRuntimeJs: return "mapsalive-runtime.js";
			case RuntimeFile.MapsAliveTourJs: return "mapsalive-tour.js";
			case RuntimeFile.MapsAlivePageJs: return "mapsalive-page.js";
			case RuntimeFile.MapsAliveMapJs: return "mapsalive-map.js";
			case RuntimeFile.MapsAliveLayoutJs: return "mapsalive-layout.js";
			case RuntimeFile.MapsAliveMarkerJs: return "mapsalive-marker.js";
			case RuntimeFile.MapsAliveDirectoryJs: return "mapsalive-directory.js";
			case RuntimeFile.MapsAlivePopupJs: return "mapsalive-popup.js";
			case RuntimeFile.MapsAliveViewJs: return "mapsalive-view.js";
			case RuntimeFile.MapsAliveLiveDataJs: return "mapsalive-livedata.js";
			case RuntimeFile.MapsAliveMediaJs: return "mapsalive-media.js";
			case RuntimeFile.MapsAliveGraphicsJs: return "mapsalive-graphics.js";
			case RuntimeFile.LiveDataJs: return "livedata.js";
			case RuntimeFile.SoundManagerJs: return "soundmanager2-nodebug-jsmin.js";
			case RuntimeFile.MemberPageJs: return "MemberPage.js";
			case RuntimeFile.PublicPageJs: return "PublicPage.js";
			case RuntimeFile.ColorChooserJs: return "ColorChooser.js";
			case RuntimeFile.HtmlEditorJs: return "HtmlEditor/tinymce.min.js";
			case RuntimeFile.LayoutEditorJs: return "LayoutEditor.js";
			case RuntimeFile.MapEditorJs: return "MapEditor.js";
			case RuntimeFile.HammerJs: return "hammer.min.js";
			case RuntimeFile.Blank: return "blank.gif";
			case RuntimeFile.LoadingLiveData: return "loading.gif";
			case RuntimeFile.CloseX: return "closeX.gif";
			case RuntimeFile.CloseTouchX: return "closeTouchX.png";
			case RuntimeFile.PopupCloseX: return "popupCloseX.png";
			case RuntimeFile.ContentContract: return "contentContract.svg";
			case RuntimeFile.ContentExpand: return "contentExpand.svg";
			case RuntimeFile.NavButton: return "navButton.svg";
			case RuntimeFile.HelpButton: return "helpButton.svg";
			case RuntimeFile.MobileCloseX: return "mobileCloseX.svg";
			case RuntimeFile.CloseHelpX: return "closeHelpX.png";
			case RuntimeFile.Pin1: return "pin1.gif";
			case RuntimeFile.Pin2: return "pin2.gif";
			case RuntimeFile.Pin1Animated: return "pin1Animated.gif";
			case RuntimeFile.ArrowLeft1: return "arrowLeft1.gif";
			case RuntimeFile.ArrowLeft2: return "arrowLeft2.gif";
			case RuntimeFile.ArrowRight1: return "arrowRight1.gif";
			case RuntimeFile.ArrowRight2: return "arrowRight2.gif";
			case RuntimeFile.ArrowUp1: return "arrowUp1.gif";
			case RuntimeFile.ArrowUp2: return "arrowUp2.gif";
			case RuntimeFile.ArrowDown1: return "arrowDown1.gif";
			case RuntimeFile.ArrowDown2: return "arrowDown2.gif";
			case RuntimeFile.DownloadReadMe: return "ReadMe.pdf";
			case RuntimeFile.DirGroup: return "dirGroup.png";
			case RuntimeFile.DirSortAZ: return "dirSortAZ.png";
			case RuntimeFile.DirContract: return "dirContract.svg";
			case RuntimeFile.DirExpand: return "dirExpand.svg";
			case RuntimeFile.DirSearch: return "dirSearch.svg";
			case RuntimeFile.CurrentPage: return "currentPage.svg";
			case RuntimeFile.ZoomIn: return "zoomIn.svg";
			case RuntimeFile.ZoomOut: return "zoomOut.svg";
			case RuntimeFile.Offline: return "offline.svg";
			default:
				System.Diagnostics.Debug.Fail("Unsupported RuntimeFile type " + runtimeFile.ToString());
				return null;
		}
	}

    public static ArrayList AppRuntimeJavaScriptFiles()
    {
        ArrayList files = new ArrayList();
        files.Add(RuntimeFile.MemberPageJs);
        files.Add(RuntimeFile.PublicPageJs);
        files.Add(RuntimeFile.ColorChooserJs);
        files.Add(RuntimeFile.LayoutEditorJs);
        files.Add(RuntimeFile.MapEditorJs);

        return files;
    }

	public static ArrayList RuntimeJavaScriptFilesV3()
    {
		ArrayList files = new ArrayList();
		files.Add(RuntimeFile.MapsAliveJs);
		files.Add(RuntimeFile.MapViewerJs);
		files.Add(RuntimeFile.LiveDataJs);
		files.Add(RuntimeFile.SoundManagerJs);
		return files;
	}

    public static ArrayList RuntimeJavaScriptFilesV4()
    {
		ArrayList files = new ArrayList();
		files.Add(RuntimeFile.MapsAliveApiJs);
		files.Add(RuntimeFile.MapsAliveRuntimeJs);
		files.Add(RuntimeFile.MapsAliveTourJs);
		files.Add(RuntimeFile.MapsAlivePageJs);
		files.Add(RuntimeFile.MapsAliveMapJs);
		files.Add(RuntimeFile.MapsAliveLayoutJs);
		files.Add(RuntimeFile.MapsAliveMarkerJs);
		files.Add(RuntimeFile.MapsAliveDirectoryJs);
		files.Add(RuntimeFile.MapsAlivePopupJs);
		files.Add(RuntimeFile.MapsAliveViewJs);
		files.Add(RuntimeFile.MapsAliveLiveDataJs);
		files.Add(RuntimeFile.MapsAliveMediaJs);
		files.Add(RuntimeFile.MapsAliveGraphicsJs);
		return files;
	}

    public static ArrayList RuntimeJavaScriptFiles3rdParty()
    {
		ArrayList files = new ArrayList();
		files.Add(RuntimeFile.HammerJs);
		return files;
	}

	public static string RuntimeFileName(RuntimeFile runtimeFile)
	{
		string name;

		if (RuntimeJavaScriptFilesV3().Contains(runtimeFile) ||
			RuntimeJavaScriptFilesV4().Contains(runtimeFile) ||
			RuntimeJavaScriptFiles3rdParty().Contains(runtimeFile) ||
            runtimeFile == RuntimeFile.HtmlEditorJs ||
			runtimeFile == RuntimeFile.MapsAliveLoaderJs)
		{
			// These files keep their natural name.
			name = ProjectFileName(runtimeFile).ToLower();
		}
		else if (runtimeFile == RuntimeFile.DownloadReadMe)
		{
			name = ProjectFileName(runtimeFile);
		}
		else
		{
            // All other files get names that contain the app version so that when the app version changes,
            // the browser won't use cached versions having the old names.
            string fileName = ProjectFileName(runtimeFile);
            string pattern = "{0:0###}_{1}";
            string suffix;

            if (fileName.EndsWith(".js"))
            {
                // Convert a JavaScript file name like SomeName.js to the form 0001_01_1234_SomeName.js.
                pattern += "_{2}";
                suffix = fileName;
            }
            else
            {
               // Convert an image file name like SomeName.gif to the form 0001_01_1234.gif.
               // The client-side runtime JavaScript depends on this format so don't change it here
               // without changing the corresponding logic there.
               pattern += ".{2}";
               suffix = fileName.Split('.')[1];
            }

            int fileNumber = (int)runtimeFile;
			name = string.Format(pattern, fileNumber, App.Version, suffix);
		}
		return name;
	}
}
