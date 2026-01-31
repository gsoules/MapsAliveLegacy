<%@
	Page Language="C#"
	AutoEventWireup="true"
	CodeFile="TourPreview.aspx.cs"
	Inherits="Members_TourPreview"
%>

<!DOCTYPE html>
<html lang="en">
	<head runat="server">
		<title></title>
		<link rel="shortcut icon" type="image/x-icon" href="~/favicon.ico" />
        <script src="../Runtime/Vex/vex.combined.min.js"></script>
        <script>vex.defaultOptions.className = 'vex-theme-os'</script>
        <link rel="stylesheet" href="../Runtime/Vex/vex.css" />
        <link rel="stylesheet" href="../Runtime/Vex/vex-theme-os.css" />
		<link href="../Styles/Style.css" rel="stylesheet" type="text/css" />
		<link href="../Styles/TourPreview.css" rel="stylesheet" type="text/css" />
        <link runat="server" id="LinkCss" rel="stylesheet" type="text/css" />
	    <link runat="server" id="CustomHtmlCss" rel="stylesheet" type="text/css" />
		<script type="text/javascript" src="../Runtime/TourPreview.js"></script>
		<% =StyleDefinitions %>
	</head>
	<body>
		<div class="tourPreviewTop">
			<asp:Panel ID="HeaderPanel" runat="server" CssClass="maPreviewMain">
				<asp:Panel ID="PublishPanel" runat="server" CssClass="tourPreviewerMenuItemPublishPanel">
					<div style="margin-bottom:24px;">
						<asp:Label ID="RunTourLabel" runat="server" Visible="false">Run this tour: </asp:Label>
						<asp:HyperLink ID="HyperLinkTour" runat="server" CssClass="tourPreviewUrl" ToolTip="Open this tour in it's own window"></asp:HyperLink>
					</div>
					<asp:Panel ID="PublishButton" runat="server" CssClass="tourPreviewPagePublishButton">
						<asp:HyperLink ID="HyperLinkPublish" CssClass="previewPageBannerOvalButton" runat="server" ToolTip="Publish this tour">
							<asp:Label runat="server" style="margin-right:8px;">Publish</asp:Label>			
						</asp:HyperLink>
					</asp:Panel>
				</asp:Panel>
				<div class="tourPreviewerPageName">Tour Preview</div>
				<asp:Panel ID="PreviewPageTourName" runat="server">
					<asp:Label ID="TourPreviewerTourName" CssClass="memberPageControlsTourName" runat="server"></asp:Label>
				</asp:Panel>
				<asp:Panel ID="MenuPanel" runat="server" CssClass="tourPreviewerTourLink">
					<asp:HyperLink ID="HyperLinkReturn" runat="server" CssClass="tourPreviewerMenuItem" ToolTip="Go back to the Tour Builder">
						<span>Return to Tour Builder</span>
					</asp:HyperLink>		
					<asp:HyperLink ID="HyperLinkAdvisor" runat="server" CssClass="tourPreviewerMenuItem" Visible="false" ToolTip="Show suggestions for improving this tour"><asp:Label ID="ShowAdvisorOption" runat="server" Text="Label"/></asp:HyperLink>					
					<asp:HyperLink ID="HyperLinkSnippets" runat="server" CssClass="tourPreviewerMenuItem" ToolTip="Show me how to integrate this tour with my website"><asp:Label ID="ShowSnippetsOption" runat="server" Text="Label"/></asp:HyperLink>					
				</asp:Panel>
				<hr />
				<div style="margin:0 24px 6px 0;text-align:right;">
					<asp:Label ID="StatusMessage" runat="server" CssClass="maPreviewStatusMessage" />
				</div>
				<asp:Panel ID="WarningPanel" runat="server" style="display:none;"></asp:Panel>
			</asp:Panel>
					
		    <asp:Panel ID="TasksPanel" runat="server" CssClass="maPreviewAdvisor">
			    <asp:Panel ID="TasksPanelContent" runat="server" CssClass="maPreviewAdvisorPanel">
				    <div class="maPreviewSubPanelTitle1">Tour <span class="maPreviewSubPanelTitle2">Advisor</span><span class="maTasksHide">(To turn off the Tour Advisor, go to <a href="../Members/Preferences.aspx">Account Preferences</a>)</span></div>
				    <div id="AdviceHeader">Below are some suggestions for improving your tour:</div>
				    <asp:Label ID="Tasks" runat="server"/>
			    </asp:Panel>
		    </asp:Panel>

			<asp:Panel ID="SnippetsPanel" runat="server" CssClass="maPreviewSnippets">
				<div class="maPreviewSnippetsPanel">
					<div class="maPreviewSubPanelTitle1">Code <span class="maPreviewSubPanelTitle2">Snippets</span></div>
					<div class="maSnippetsPanelContent">
						Shown below are ways to integrate your published tour with your web site.<br />
						You can copy/paste the HTML code shown below directly into your own web pages.<br />
						<hr />
						1. Type the tour's URL directly into your browser's address field.  The URL for this tour is:<br />
						<asp:Label ID="UseUrl" runat="server" CssClass="tourPreviewerSnippet"></asp:Label>
						<hr />
						2. Link to the tour from a web page so that the tour opens in a separate window:<br />
						<asp:Label ID="UseOpenWindow" runat="server" CssClass="tourPreviewerSnippet"></asp:Label>
						<div style="padding-left: 12px; margin-top: 6px;">
							Try it by clicking here: <asp:Label ID="UseOpenWindowTryIt" runat="server"></asp:Label> (opens a new window).
						</div>
						<hr />
						3. Link to the tour from a web page so that the tour replaces the current window:<br />
						<asp:Label ID="UseReplaceWindow" runat="server" CssClass="tourPreviewerSnippet"></asp:Label>
						<div style="padding-left:12px;margin-top:6px;">
							Try it by clicking here: <asp:Label ID="UseReplaceWindowTryIt" runat="server" ></asp:Label> (when done, press the Back button to return to this page).
						</div>
						<hr />
						4. Embed the tour into a web page using an iframe tag:<br />
						<asp:Label ID="UseIframe" runat="server" CssClass="tourPreviewerSnippet"></asp:Label>
						<div id="IframeDiv" style="padding-left: 12px; margin-top: 6px;">
							<div>
								<asp:HyperLink ID="HyperLinkInsertIframe" runat="server"><span id="ShowIframeOption">Show me</span></asp:HyperLink>
							</div>
						</div>
						<hr />
						5. Embed this tour directly inside your web page HTML. <span class="maPreviewMoreInfo">For help see <a href="https://mapsalive.com/docs/use-embed/" target="_blank">Using Interactive Maps</a> in the MapsAlive User Guide</span>.<br />
						<div style="margin-left:12px;margin-top:4px;"><asp:Label ID="UseFramelessHelpText1" runat="server"></asp:Label></div>
						<asp:Label ID="UseFrameless" runat="server" CssClass="tourPreviewerSnippet"></asp:Label>
						<div style="margin-left: 12px; margin-top: 8px;"><asp:Label ID="UseFramelessHelpText2" runat="server"></asp:Label></div>
					</div>
				</div>
			</asp:Panel>
			
			<asp:Panel ID="SoundManagerPanel" runat="server" Visible="false">
				<script type="text/javascript" src="<% =PathSoundManagerJs %>"></script>
				<script type="text/javascript">soundManager.url='<% =PathRuntimeFolder %>';</script>
			</asp:Panel>

			<asp:Panel ID="SpecialMessagePanel" runat="server" CssClass="specialMessage" Visible="false">
				<div style="width:715px;">
					<asp:Label ID="SpecialMessage" runat="server" />
				</div>
			</asp:Panel>
		</div>
		
		<div class="tourPreviewBottom">
			<asp:Panel ID="PreviewPanel" runat="server" Visible="false" >
				<script type="text/javascript">window.maTourPreviewPath="<% =PathTourPreviewFolder %>/";</script>
				<script type="module" id="<% =LoaderId %>" src="<% =PathLoaderJs %>"></script>
				<div class="<% =LoaderId %>"></div>
			</asp:Panel>

			<asp:Panel ID="PreviewPanelV3" runat="server" Visible="false">
				<script type="text/javascript" src="<% =PathMapsAliveJs %>"></script>
				<script type="text/javascript"><% =PathMaClient %>maClient.preview = true;</script>
				<asp:Panel ID="MapViewerPanel" runat="server">
					<script type="text/javascript" src="<% =PathMapViewerJs %>"></script>
				</asp:Panel>
				<script type="text/javascript" src="<% =PathPageJs %>"></script>
			</asp:Panel>

			<asp:Panel ID="CustomHtmlJavaScriptIncludePanel" runat="server" Visible="false">
				<script type="text/javascript" src="<% =PathCustomHtmlJs %>"></script>
			</asp:Panel>
			
			<asp:Panel ID="CustomHtmlJavaScriptPanel" runat="server" Visible="false">
				<script type="text/javascript" src="<% =PathCustomJs %>"></script>
			</asp:Panel>
		</div>
	</body>
</html>
