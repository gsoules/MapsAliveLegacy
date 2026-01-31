<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="CustomHtml.aspx.cs"
	Inherits="Members_CustomHtml"
	ValidateRequest="false"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<script language="javascript">
    // Don't use the visualchars plugin because it will cause the nonbreaking_wrap option to act as if set to true.
    // The nonbreaking_wrap must be set to false to prevent insertion of <span class="mce-nbsp-wrap"></span> around
    // sets of three &nbsp; entities that get added by the nonbreaking plugin when the Tab key is pressed.
    tinymce.init({
        selector: `.HtmlEditor`,
        content_style: 'body { font-family: monospace; font-size: 12px; margin:4px 4px;}',
        min_height: 120,
        max_width:1600,
        resize: 'both',
        plugins: 'code paste nonbreaking autoresize',
        paste_as_text: true,
        forced_root_block: false,
        nonbreaking_force_tab: true,
        nonbreaking_wrap: false,
        branding: false,
        autoresize_bottom_margin: 8,
        toolbar: 'undo redo customCopy',
        menubar: false,
        removed_menuitems: 'visualaid',
        setup: function (editor)
        {
            editor.on('Change', function (e)
            {
                maChangeDetected();
            });

            editor.ui.registry.addButton('customCopy', {
                icon: 'copy',
                tooltip: 'Copy to clipboard',
                onAction: function (_)
                {
                    let content = editor.getContent();
                    content = content.replace(/<br \/>/g, '\r\n');
                    content = content.replace(/&nbsp;/g, ' ');
                    const e = document.createElement('textarea');
                    e.innerHTML = content;
                    document.body.appendChild(e);
                    e.select();
                    document.execCommand('copy');
                    document.body.removeChild(e);
                }
            });
        }
    });
    function maSaveWarning()
    {
        let msg = "<p>Hello " + contactName + ".</p><p>You have not saved your edits for a while and your MapsAlive session may expire soon.</p><p>To avoid losing any work, press SAVE to save changes and extend your browser session.</p><p>To continue without saving, press CANCEL.";
        if (contentChanged)
        {
            let dialogShowing = document.getElementsByClassName('vex-dialog-form');
            if (dialogShowing.length == 0)
                maConfirmAndExecuteScript(msg, "maOnEventSave();", "SAVE");
        }
        setTimeout(maSaveWarning, saveTimeout);
    }
    </script>
	
    <AvantLogic:QuickHelpTitle ID="CustomHtmlCss" runat="server" Title="CSS" Span="true" />
	<asp:TextBox ID="HtmlEditorCss" runat="server" CssClass="HtmlEditor" />
	<br />
	<AvantLogic:QuickHelpTitle ID="CustomHtmlJavaScript" runat="server" Title="JavaScript" Span="true" />
	<asp:TextBox ID="HtmlEditorJs" runat="server" CssClass="HtmlEditor" />
	<br />
	<AvantLogic:QuickHelpTitle ID="CustomHtmlAbsolute" runat="server" Title="HTML Absolute" Span="true" />
	<asp:TextBox ID="HtmlEditorHtmlAbsolute" runat="server" CssClass="HtmlEditor" />
	<br />
	<AvantLogic:QuickHelpTitle ID="CustomHtmlTop" runat="server" Title="HTML Top" Span="true" />
	<asp:TextBox ID="HtmlEditorHtmlTop" runat="server" CssClass="HtmlEditor" />
	<br />
	<AvantLogic:QuickHelpTitle ID="CustomHtmlBottom" runat="server" Title="HTML Bottom" Span="true" />
	<asp:TextBox ID="HtmlEditorHtmlBottom" runat="server" CssClass="HtmlEditor" />
</asp:Content>

