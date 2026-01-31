// Copyright (C) 2003-2010 AvantLogic Corporation
using System.Web;

public partial class Members_CustomHtml : MemberPage
{
	private const int shorter = 10;
	private const int taller = 50;

	private enum editorType
	{
		Css = 1,
		JavaScript = 2,
		Top = 3,
		Absolute = 4,
		Bottom = 5
	}
    protected override void EmitJavaScript()
    {
        string loadingScript =
           AssignClientVar("contactName", account.ContactName.Replace("'", "\\'"));

        // Warn the user if they have not saved their edits after several minutes.
        // If their session expires, they will lose their changes.
        const int minutes = 10;
        const int milliseconds = minutes * 60 * 1000;

        string loadedScript = string.Format("setTimeout(maSaveWarning,{0});", milliseconds);
        loadingScript += AssignClientVar("saveTimeout", milliseconds);

        EmitJavaScript(loadingScript, loadedScript);
    }

    protected override void InitControls(bool undo)
	{
		if (!undo && IsPostBack)
			return;

        HtmlEditorCss.Text = ConvertPlainTextToEncodedHtml(tour.CustomHtmlCss);
        HtmlEditorJs.Text = ConvertPlainTextToEncodedHtml(tour.CustomHtmlJavaScript);

        HtmlEditorHtmlAbsolute.Text = ConvertPlainTextToEncodedHtml(tour.CustomHtmlAbsolute);

        if (tour.IsFlexMapTour)
        {
            HtmlEditorHtmlTop.Visible = false;
            CustomHtmlTop.Visible = false;
            HtmlEditorHtmlBottom.Visible = false;
            CustomHtmlBottom.Visible = false;
        }
        else
        {
            HtmlEditorHtmlTop.Text = ConvertPlainTextToEncodedHtml(tour.CustomHtmlTop);
            HtmlEditorHtmlBottom.Text = ConvertPlainTextToEncodedHtml(tour.CustomHtmlBottom);
        }
	}

	protected override void PageLoad()
	{
        Utility.RegisterHtmlEditorJavaScript(this);
        
        SetMasterPage(Master);
		SetPageTitle("Custom HTML");
		SetActionIdForPageAction(MemberPageActionId.CustomHtml);
		GetSelectedTour();
	}

	protected override void PerformUpdate()
	{
		tour.UpdateDatabase();
		tour.CreateCustomHtmlFiles();
	}

    protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}

	protected override void ReadPageFields()
	{
        string css = ConvertEncodedHtmlToPlainText(HtmlEditorCss.Text);
        tour.CustomHtmlCss = StripPTags(css);

        string js = ConvertEncodedHtmlToPlainText(HtmlEditorJs.Text);
        tour.CustomHtmlJavaScript = StripPTags(js);

		tour.CustomHtmlTop = ConvertEncodedHtmlToPlainText(HtmlEditorHtmlTop.Text);
		tour.CustomHtmlAbsolute = ConvertEncodedHtmlToPlainText(HtmlEditorHtmlAbsolute.Text);
		tour.CustomHtmlBottom = ConvertEncodedHtmlToPlainText(HtmlEditorHtmlBottom.Text);
    }

    private string StripPTags(string text)
    {
        // Remove enclosing <p> tags that are sometimes inserted by the
        // Tiny MCE editor even though it's configured not to do that.
        if (text.StartsWith("<p>"))
            text = text.Substring(3);
        if (text.EndsWith("</p>"))
            text = text.Substring(0, text.Length - 4);
        return text;
    }

    private string ConvertPlainTextToEncodedHtml(string text)
    {
        if (text.Length == 0)
            return text;

        // Convert the plain text for CSS, JavaScript, and HTML into HTML entities so that the
        // plain text will actually look like plain text when displayed in the Tiny MCE editor.
        // If this isn't done, the editor will interpret HTML as HTML and display the content
        // without showing the HTML tags.
        string s = text;
        s = HttpUtility.HtmlEncode(s);

        // Replace the plain text CRs with break tags.
        s = s.Replace("\r\n", "<br>");
        s = s.Replace("\n", "<br>");

        // Replace spaces with non-breaking spaces to preserve spacing, especially indenting
        // at the beginning of lines, when going from plain text to HTML. Because these &nbsp;
        // characters are only used for displaying the plain text as HTML while in the editor,
        // and because they are converted back to actual spaces when the text is saved, they
        // have no effect on the HTML itself.
        s = s.Replace(" ", "&nbsp;");
        
        return s;
    }

    private string ConvertEncodedHtmlToPlainText(string text)
    {
        if (text.Length == 0)
            return text;
        
        // Convert the encoded HTML that comes from the Tiny MCE editor into the plain text
        // that is stored in the database. This will be the text that the runtime uses to
        // insert into the DOM for custom JavaScript, CSS, and HTML.

        // Replace non-breaking spaces with spaces before decoding, otherwise &nbsp; will get decoded
        // to the hidden character &#160 (xA0) which takes up one visible space but is not the same as a
        // space. Its inclusion in HTML prevents the HTML from loading properly when inserted into the DOM.
        string s = text;
        s = s.Replace("&nbsp;", " ");

        // Convert HTML entities back to plain text.
        s = HttpUtility.HtmlDecode(s);
        
        // Convert HTML break tags of any flavor into CRs for use with plain text.
        s = s.Replace("<br />", "\r\n");
        s = s.Replace("<br/>", "\r\n");
        s = s.Replace("<br>", "\r\n");
        
        return s;
    }
}
