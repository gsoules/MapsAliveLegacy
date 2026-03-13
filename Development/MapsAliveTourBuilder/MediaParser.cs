// Copyright (C) 2003-2021 AvantLogic Corporation
using System.Drawing;
using AvantLogic;

public class MediaParser
{
	public MediaParser()
	{
	}

	public static bool ParseEmbedCode(ref string html, out Size size, out string errorMessage)
	{
		errorMessage = string.Empty;
		size = Size.Empty;

		// Leave now if there's nothing to parse.
		if (html.Trim().Length == 0)
			return true;

		// Determine if any text appears before the opening tag.
		int firstTagChar = html.IndexOf("<");
		if (firstTagChar < 0)
		{
			errorMessage = "No &lt;video&gt; or &lt;iframe&gt; tag was found.";
			return false;
		}
		else if (firstTagChar > 0)
		{
			// Ignore any text that appears before the opening tag.
			html = html.Substring(firstTagChar);
		}

		// Start parsing the HTML.
		HtmlParser parser = new HtmlParser(html);
		Tag tag = parser.Parse();
		if (tag == null)
			return true;

		// Remember the opening tag so that we can match its closing tag later.
		string openTagName = tag.Name;

		if (tag.NameIs("video") || tag.NameIs("iframe"))
		{
			// Get the size attributes right from the tag.
			int width = GetDimensionAttribute(tag, "width");
			int height = GetDimensionAttribute(tag, "height");

            if (width == 0 || height == 0)
            {
                errorMessage = string.Format("Width and height must both be specified for the &lt;{0}&gt; tag.", openTagName);
                return false;
            }

            size = new Size(width, height);
		}
		else
		{
			// We didn't find anything we recognize. Note that one of the main reasons we don't
			// support other tags e.g. Div, is because we really need to know the dimensions of
			// whatever we embed. If we allowed any HTML, we would not know how big is which
			// would make dealing with slide layouts a lot more difficult. If a user needs to
			// use their own HTML, they can do it in the text part of the slide with the text editor.
			errorMessage = string.Format("Found a &lt;{0}&gt; tag, but only &lt;video&gt; and &lt;iframe&gt; tags are allowed.", tag.Name);
			return false;
		}

		// Now we start over by parsing the html which has passed the basic sanity test.
		parser = new HtmlParser(html);
		tag = parser.Parse();

		// Search for the closing tag. It will have the same name as the opening
		// tag and will be marked as not being an open tag.
		bool foundClosingTag = false;
		while (tag != null && !foundClosingTag)
		{
			tag = parser.Parse();
			foundClosingTag = tag != null && !tag.IsOpenTag && tag.NameIs(openTagName);
		}

		if (foundClosingTag)
		{
			// Truncate any text that follows the closing tag.
			html = html.Substring(0, tag.Offset + tag.Length);
		}

		return true;
	}

	private static int GetDimensionAttribute(Tag tag, string name)
	{
        // Convert a dimension (width or height) string to an integer. Ignore "px" if included in the string.
        string value = tag.AttributeValue(name);
        value = value.ToLower().Replace("px", "");
        int dim;
        int.TryParse(value, out dim);
        return dim;
	}
}
