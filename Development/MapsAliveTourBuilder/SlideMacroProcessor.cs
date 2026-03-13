// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Text.RegularExpressions;
using AvantLogic;

public class SlideMacroProcessor
{
	private string macroName;
	private string macroError;
	private Hashtable regexTable;
	private Tour tour;
	
	public SlideMacroProcessor(Tour tour)
	{
		this.tour = tour;
		regexTable = new Hashtable();
	}

	public void ExpandMacros(TourView tourView, ref string slideText)
	{
		ArrayList macros;

		macros = ExpandMacro("image", ref slideText);
		foreach (Hashtable macro in macros)
		{
			ExpandImageMacro(macro, ref slideText, tourView);
		}

		macros = ExpandMacro("iframe", ref slideText);
		foreach (Hashtable macro in macros)
		{
			ExpandIframeMacro(macro, ref slideText);
		}

		macros = ExpandMacro("tour", ref slideText);
		foreach (Hashtable macro in macros)
		{
			ExpandTourMacro(macro, ref slideText);
		}

		macros = ExpandMacro("page", ref slideText);
		foreach (Hashtable macro in macros)
		{
			ExpandPageMacro(macro, ref slideText, tourView);
		}

		// This is the same as the "page" macro to preserve backward compatibility.
		macros = ExpandMacro("linkto", ref slideText);
		foreach (Hashtable macro in macros)
		{
			ExpandPageMacro(macro, ref slideText, tourView);
		}

		// Someday we might want to have a text macro that would allow a user to insert text from
		// another slide. Keep in mind that the inserted text would have to be pre-processed as is
		// done in PreProcessSlideText and macros within the inserted text would have to either be
		// ignored or also expaned. In the latter case we'd have to check for circularity. It seems
		// like there are enough trouble spots to leave this one off for now.
	}

	private void AddMacroError(string error)
	{
		macroError += "<br/>" + error;
	}

	private void AddMacroError(string pattern, string value)
	{
		macroError += string.Format("<br/>" + pattern, value);
	}

	private void AddRequiredParameterError(string name)
	{
		AddMacroError(string.Format("A {0} parameter is required.", name));
	}

	private static string CreateIframeTag(string src, string width, string height)
	{
		return string.Format("<iframe src=\"{0}\" width=\"{1}\" height=\"{2}\" frameBorder=\"0\" scrolling=\"no\" ></iframe>", src, width, height);
	}

	private ArrayList ExpandMacro(string name, ref string slideText)
	{
		macroName = name;

		Regex regex = (Regex)regexTable[name];

		if (regex == null)
		{
			// This pattern looks for "[name:" followed by 1 or more of anything except "]" followed by "]".
			// The part of the pattern in parens is a group that should contain a comma-separated list of args.
			string pattern = string.Format(@"\[{0}-macro(\s|&nbsp;)(?<id>[^\]]*)]", macroName);

			// Construct the regular expression processor for the pattern. It's an expensive operation
			// and this code gets called for each slide, so cache the processor so that we can reuse it.
			regex = new Regex(pattern, RegexOptions.IgnoreCase);
			regexTable.Add(name, regex);
		}
		
		MatchCollection matches = regex.Matches(slideText);

		ArrayList macros = new ArrayList();

		foreach (Match match in matches)
		{
			Capture capture = match.Groups["id"];

			// Create an ArrayList where the first element will be the match
			// and subsequent elements will be the args.
			Hashtable macro = new Hashtable();
			macro.Add("macro", match.Value);

			// Extract the args portion of the macro. The text has to be decoded to
			// convert encoded HTML entities like &amp; &nbsp; and &lt; to their
			// character values.
			string captureValue = System.Web.HttpUtility.HtmlDecode(capture.Value.Trim());
			
			// Remove any line breaks.
			captureValue = captureValue.Replace("<br>", " ");
			captureValue = captureValue.Replace("<br/>", " ");
			
			// Split the args into name/value pairs.
			string[] args = captureValue.Split(';');

			// Trim off trailing or leading whitespace.
			for (int i = 0; i < args.Length; i++)
			{
				string s = args[i].Trim();
				string[] arg = s.Split(':');
				
				if (arg.Length >= 2)
				{
					string key = arg[0].ToLower();

					// Because an arg might be like this "src:http://www..." where the value
					// contains a colon, we have to use substring to get the entire value.
					int colonIndex = s.IndexOf(':');
					string value = s.Substring(colonIndex);
					if (value.Length > 1)
						value = value.Substring(1);
					else
						value = string.Empty;

					try
					{
						macro.Add(key, value);
					}
					catch
					{
						// This can happen if the user codes the same argument more than once.
						continue;
					}
				}
				else
				{
					continue;
				}
			}

			macros.Add(macro);
		}

		return macros;
	}

	private string GetArg(Hashtable macro, string key)
	{
		object o = macro[key];
		if (o == null)
			return null;
		else
			return ((string)o).Trim();
	}

	private void ExpandImageMacro(Hashtable macro, ref string slideText, TourView tourView)
	{
		string matchValue = GetArg(macro, "macro");
		macroError = string.Empty;
		
		// Get the required slide Id.
		string slideId = GetArg(macro, "slide-id");
		if (slideId == null)
			slideId = GetArg(macro, "hotspot-id");
		if (slideId == null)
			AddRequiredParameterError("hotspot-id");

		// Get the optional page Id.
		TourPage tourPage = null;
		string pageId = GetArg(macro, "page-id");
		if (pageId == null)
			pageId = GetArg(macro, "map-id");
		if (pageId != null)
		{
			tourPage = tour.GetTourPageByPageId(pageId);
			if (tourPage == null)
				AddMacroError("'{0}' is not the Id of a page in this tour.", pageId);
		}
		if (tourPage == null)
			tourPage = tourView.TourPage;

		// Get the slide's tour  view.
		TourView slide = null;
		if (slideId != null)
		{
			slide = tourPage.GetTourViewBySlideId(slideId);
			if (slide == null)
				AddMacroError("'{0}' is not the Id of a slide on page " + tourPage.Name + ".", slideId);
			else if (!slide.HasImage)
				AddMacroError("Slide '{0}' has no image.", slideId);
			else if (!tourView.TourPage.ActiveSlideLayout.HasImageArea)
				AddMacroError("The image for slide '{0}' is not available because its page layout does not display images.", slideId);
		}

		// Get the optional dimensions.
		string width = ValidatePixelValue(GetArg(macro, "width"), "width");
		string height = ValidatePixelValue(GetArg(macro, "height"), "height");

		if (macroError != string.Empty)
		{
			ReportMacroError(ref slideText, matchValue);
		}
		else
		{
			// Construct an <img> tag. Prefix the src with a ':' that will be
			// replaced at runtime in MapsAlive.js by the correct path to the file.
			string w = width == null ? string.Empty : string.Format(" width='{0}px'", width);
			string h = height == null ? string.Empty : string.Format(" height='{0}px'", height);
			string src = slide.Image.FileNameInternal;
			string value = string.Format("<img{0}{1} src=\":{2}\"/>", w, h, src);
			slideText = slideText.Replace(matchValue, value);
		}
	}

	private void ExpandIframeMacro(Hashtable macro, ref string slideText)
	{
		string matchValue = GetArg(macro, "macro");
		macroError = string.Empty;

		string src = null;
		string rawSrc = GetArg(macro, "src");
		if (rawSrc == null)
			AddRequiredParameterError("src");
		else
			src = rawSrc;

		string width;
		string height;
		GetRequiredWidthAndHeight(macro, out width, out height);

		if (macroError != string.Empty)
		{
			ReportMacroError(ref slideText, matchValue);
		}
		else
		{
			// Construct an <iframe> tag.
			string value = CreateIframeTag(src, width, height);
			slideText = slideText.Replace(matchValue, value);
		}
	}

	private void ExpandTourMacro(Hashtable macro, ref string slideText)
	{
		Tour innerTour = null;
		string matchValue = GetArg(macro, "macro");
		macroError = string.Empty;

		string tourId = GetArg(macro, "tour");
		if (tourId == null)
		{
			AddRequiredParameterError("tour");
		}
		else
		{
			int id = 0;
			int.TryParse(tourId, out id);
			if (id == 0)
			{
				AddMacroError("'{0}' is not a valid tour number.", tourId);
			}
			else
			{
				innerTour = new Tour(id);
				if (innerTour.Id == 0)
				{
					AddMacroError("Tour {0} does not belong to your account.", id.ToString());
				}
				else if (!innerTour.HasBeenPublished)
				{
					AddMacroError("Tour {0} has not been published.", id.ToString());
				}
			}
		}

		if (macroError != string.Empty)
		{
			ReportMacroError(ref slideText, matchValue);
		}
		else
		{
			// See if the user provided a src parameter. This allows someone to reference a tour in their
			// account that is hosted somewhere else (not at tour.mapsalive.com). The iframe width and
			// height will be fetched from the account but the iframe src will be the one provided.
			string src = GetArg(macro, "src");
			if (src == null)
				src = innerTour.Url;
			
			// Construct an <iframe> tag.
			string value = CreateIframeTag(src, innerTour.TourSize.Width.ToString(), innerTour.TourSize.Height.ToString());
			slideText = slideText.Replace(matchValue, value);
		}
	}

	private void GetRequiredWidthAndHeight(Hashtable macro, out string width, out string height)
	{
		width = GetArg(macro, "width");
		if (width == null)
			AddRequiredParameterError("width");
		else
			width = ValidatePixelValue(width, "width");

		height = GetArg(macro, "height");
		if (height == null)
			AddRequiredParameterError("height");
		else
			height = ValidatePixelValue(height, "height");
	}

	private void ExpandPageMacro(Hashtable macro, ref string slideText, TourView tourView)
	{
		string matchValue = GetArg(macro, "macro");
		macroError = string.Empty;

		TourPage tourPage = null;
		string markerId = string.Empty;
		
		string linkText = GetArg(macro, "link-text");
		string slideId = GetArg(macro, "slide-id");
		if (slideId == null)
			slideId = GetArg(macro, "hotspot-id");
		string pageId = GetArg(macro, "page-id");
		if (pageId == null)
			pageId = GetArg(macro, "map-id");
		string tooltip = GetArg(macro, "tooltip");
		
		if (pageId == null)
		{
			AddRequiredParameterError("map-id");
		}
		else
		{
			tourPage = tour.GetTourPageByPageId(pageId);
			if (tourPage == null)
			{
				AddMacroError("'{0}' is not the Id of a page in this tour", pageId);
			}
			else
			{
				if (slideId != null)
				{
					TourView slide = tourPage.GetTourViewBySlideId(slideId);
					if (slide == null)
						AddMacroError("'{0}' is not the Id of a slide in this tour", slideId);
					else
						markerId = string.Format("{0}", slide.Id);
				}
			}
		}

		if (macroError != string.Empty)
		{
			ReportMacroError(ref slideText, matchValue);
		}
		else
		{
			// Construct an <a> tag.
			if (linkText == null)
				linkText = tourPage.Name;
			tooltip = tooltip == null ? string.Empty : string.Format(" title=\"{0}\"", tooltip);

			string value;
			if (tourPage.Id == tourView.TourPage.Id && markerId != string.Empty)
			{
				// The link is to this same page in order show a different slide.
				value = string.Format("<a href=\"javascript:maClient.showSlide({0},true);\"{1}>{2}</a>", markerId, tooltip, linkText);
			}
			else
			{
				if (markerId != string.Empty)
					markerId = "," + markerId;
                if (tour.V3CompatibilityEnabled)
				    value = string.Format("<a href=\"javascript:maClient.goToPage('{0}'{1},null);\"{2}>{3}</a>", tourPage.NameForPageHtmlPublishedFile, markerId, tooltip, linkText);
                else
                    value = string.Format("<a href=\"javascript:window.MapsAlive.getTour('1-{4}').goToPageView({0}{1},null);\"{2}>{3}</a>", tourPage.PageNumber, markerId, tooltip, linkText, tour.Id);
            }

            slideText = slideText.Replace(matchValue, value);
		}
	}

	private void ReportMacroError(ref string slideText, string matchValue)
	{
		string error = string.Format("{0}<div style='color:red;'>=== Error in {1}-macro ==={2}</div></br/>", matchValue, macroName, macroError);
		slideText = slideText.Replace(matchValue, error);
	}

	private string ValidatePixelValue(string value, string name)
	{
		if (value == null)
			return null;

		int v = 0;

		if (value.ToLower().EndsWith("px"))
			value = value.Substring(0, value.Length - 2);

		int.TryParse(value, out v);
		if (v <= 0)
		{
			macroError += string.Format("<br/>'{0}' is not a valid {1}.", value, name);
			return null;
		}
		else
		{
			return v.ToString();
		}
	}
}
