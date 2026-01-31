<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet
	version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
>

	<xsl:output
		method="html"
		indent="yes"
		encoding="UTF-8"
	/>

	<xsl:param name="contentId"/>
	<xsl:param name="pageId"/>

	<xsl:template match="/tour">
		<xsl:variable name="page" select="tourPages/tourPage[@pageId=$pageId]"/>
		<xsl:choose>
			<xsl:when test="$contentId='css'">
				<xsl:call-template name="EmitCss">
					<xsl:with-param name="page" select="$page"/>
        </xsl:call-template>
			</xsl:when>
			<xsl:when test="$contentId='js'">
				<xsl:call-template name="EmitBody">
						<xsl:with-param name="page" select="$page"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$contentId='html' or $contentId='html_'">
				<xsl:call-template name="EmitHtml">
						<xsl:with-param name="page" select="$page"/>
				</xsl:call-template>
			</xsl:when>
		</xsl:choose>
  </xsl:template>

	<xsl:template name="EmitHtml">
		<xsl:param name="page"/>
		<xsl:text disable-output-escaping='yes'>&lt;!DOCTYPE html></xsl:text>
		<html lang="en">
			<!--
				When using IE on Microsoft Windows XP Service Pack 2, any Javascript in a web page causes a
				security message to appear in IE's yellow Information Bar.  The problem is avoid by our inserting
				a "Mark of the web" into the page.  The problem can also be avoided by changing IE's security
				settings.  This is done by choosing the	Advanced tab from Tools > Internet Options...  In the
				Security section of the options, check the box called	"Allow active content to run in files on My
				Computer". We force new line above and below so that the mark ends up on its own line.
			-->
<xsl:text>
</xsl:text>
<xsl:comment> saved from url=(0013)about:internet </xsl:comment>
<xsl:text>
</xsl:text>
			<head>
				<meta http-equiv="Pragma" content="no-cache"/>
				<meta http-equiv="Expires" content="-1"/>
				<meta http-equiv="Content-Type" content="text/html;charset=utf-8" />
				<xsl:if test="@webAppCapable='True'">
				<meta name="apple-mobile-web-app-capable" content="yes" />
				<meta name="apple-mobile-web-app-status-bar-style" content="black" />
				</xsl:if>
				<xsl:if test="@viewPortIsDeviceWidth='True'">
				<meta name="viewport" content="width=device-width, maximum-scale=1.0" />
				</xsl:if>
				<link rel="stylesheet" type="text/css" href="page{$page/@pageNumber}.css?v={@buildId}" />
				<style>body{background-color:<xsl:value-of select="@bodyBackgroundColor"/>;margin:<xsl:value-of select="@bodyMargin" />px;}<xsl:if test="@centeredInBrowser='True'">
      #maTour{text-align:center;margin:auto;}<xsl:if test="@showMapsAliveLink='1' or @showCustomFooter='1'">
      #maFooterLink{text-align:center;margin:auto;}</xsl:if><xsl:if test="@hasCustomHtmlTop">
      #maCustomHtmlTop{margin:auto;}</xsl:if><xsl:if test="@hasCustomHtmlBottom">
      #maCustomHtmlBottom{margin:auto;}</xsl:if></xsl:if>
				</style>
				<xsl:if test="$contentId='html_'">
					<style>#maTour{height:<xsl:value-of select="$page/@pageHeight - @bannerHeight"/>px;}#maBanner{display:none;}</style>
				</xsl:if>
				<xsl:if test="@hasCustomHtmlCss">
					<link rel="stylesheet" type="text/css" href="custom.css?v={@buildId}" />
				</xsl:if>
				<title><xsl:value-of select="@browserTitle" /></title>
			</head>
			<body>
				<!-- Emit the JavaScript includes.  Note that the closing </script> tag must present.
				     If we use the shortcut syntax to close the tag, the second file won't be included.
				     There is a lot written about this, but I have not been able to find an official
						 explanation.  We force the closing tag by inserting a non-breaking space. -->
				<xsl:if test="@fileSoundManager">
					<script type="text/javascript" src="{@fileSoundManager}">&#160;</script>
					<script type="text/javascript">soundManager.url=".";</script>
				</xsl:if>
				<script type="text/javascript" src="{@fileMapsAliveJs}?v={@buildId}">&#160;</script>
				<xsl:if test="@fileMapViewerJs">
					<script type="text/javascript" src="{@fileMapViewerJs}?v={@buildId}">&#160;</script>
				</xsl:if>
				<script type="text/javascript">maClient.page=true;</script>
				<xsl:if test="$contentId='html_'">
					<script type="text/javascript">maClient.unbranded=true;</script>
				</xsl:if>
				<script type="text/javascript" src="page{$page/@pageNumber}.js?v={@buildId}">&#160;</script>
				<xsl:if test="$page/@usesLiveData">
					<script type="text/javascript" src="livedata.js?v={@buildId}">&#160;</script>
				</xsl:if>
				<xsl:if test="@hasCustomHtmlJavaScript">
					<script type="text/javascript" src="custom.js?v={@buildId}">&#160;</script>
					<xsl:if test="@javascriptIncludeSrc">
						<script type="text/javascript" src="{@javascriptIncludeSrc}?v={@buildId}">&#160;</script>
					</xsl:if>
				</xsl:if>
			</body>
			<!-- Repeat the head tag to prevent caching per recommendation of Microsoft (see http://support.microsoft.com/kb/222064).
			<head>
        <meta http-equiv="Pragma" content="no-cache"/>
        <meta http-equiv="Expires" content="-1"/>
      </head>
			-->
    </html>
	</xsl:template>

	<xsl:template name="EmitBody">
		<xsl:param name="page"/>
		<xsl:variable name="mapFileNamePrefix" select="substring($page/@mapFileName,1,12)"/>
		<script type="text/javascript">
			<xsl:call-template name="EmitJavascript">
				<xsl:with-param name="page" select="$page"/>
			</xsl:call-template>
		</script>
		<!-- Force a line break after the closing script tag. If we don't do this, the next tag
		starts on the same line as the script and that screws up the formatting on some browser (Safari).-->
		<xsl:text>
		</xsl:text>
		<xsl:if test="@fileMapViewerJs">
			<!-- Statically include the map tiles and symbols js to workaround an iOS 9 Safari bug
				where these files are taking 30+ seconds to download dynamically from mapviewer.js.
				The penalty for doing this is that tours the execute with Flash download these files
				that are only needed for HTML5. -->
			document.writeln('<script type="text/javascript" src="' + maClient.path + '{$mapFileNamePrefix}.js?v={@buildId}"></script>');
			document.writeln('<script type="text/javascript" src="' + maClient.path + 'symbols{$page/@pageNumber}.js?v={@buildId}"></script>');
		</xsl:if>
		<xsl:text>
		</xsl:text>
		<xsl:if test="@hasCustomHtmlTop">
		<customHtmlTop/>
		</xsl:if>
		<div id="maTour">
			<xsl:if test="$page/@testRouteId">
				<select style="position:absolute;z-index:10000" onchange="mapsalive.drawTestRoute('{$page/@testRouteId}',this.value);">
				<option value="0">- Hide Route -</option>
				<xsl:for-each select="routes/route">
					<option value="{@id}"><xsl:value-of select="@id"/></option>
				</xsl:for-each>
				</select>
			</xsl:if>

			<xsl:if test="@hasCustomHtmlAbsolute">
			<customHtmlAbsolute/>
			</xsl:if>

			<xsl:if test="@hasBanner='True'">
				<xsl:call-template name="EmitBanner">
					<xsl:with-param name="page" select="$page"/>
				</xsl:call-template>
			</xsl:if>

			<xsl:if test="@hasTitle='True'">
				<xsl:call-template name="EmitTitle">
					<xsl:with-param name="page" select="$page"/>
				</xsl:call-template>
			</xsl:if>

			<xsl:if test="@hasHeaderStripe='True'">
				<xsl:call-template name="EmitHeaderStripe" />
			</xsl:if>

			<xsl:call-template name="EmitMenuAndLayout">
				<xsl:with-param name="page" select="$page"/>
				<xsl:with-param name="canvasWidth" select="$page/@canvasWidth"/>
				<xsl:with-param name="canvasHeight" select="$page/@canvasHeight"/>
				<xsl:with-param name="pageWidth" select="$page/@pageWidth"/>
			</xsl:call-template>

			<xsl:if test="@hasFooterStripe='True'">
				<xsl:call-template name="EmitFooterStripe" />
			</xsl:if>

			<xsl:call-template name="EmitJavascriptDetection">
				<xsl:with-param name="page" select="$page"/>
			</xsl:call-template>
			<xsl:if test="@hasDirectory='True'">
				<xsl:call-template name="EmitDirectory">
					<xsl:with-param name="page" select="$page"/>
				</xsl:call-template>
			</xsl:if>
		</div>
		<xsl:if test="@hasCustomHtmlBottom">
		<customHtmlBottom/>
		</xsl:if>
		<xsl:call-template name="EmitFooterLinks"/>
	</xsl:template>

		<xsl:template name="EmitBanner">
    <xsl:param name="page"/>
		<xsl:choose>
			<xsl:when test="$page/@bannerUrl">
				<a href="{$page/@bannerUrl}">
					<xsl:if test="$page/@bannerUrlOpensWindow='True'">
						<xsl:attribute name="target">_blank</xsl:attribute>
					</xsl:if>
					<xsl:if test="$page/@bannerUrlTitle">
						<xsl:attribute name="title"><xsl:value-of select="$page/@bannerUrlTitle" /></xsl:attribute>
					</xsl:if>
					<div id="maBanner"></div>
				</a>
			</xsl:when>
			<xsl:otherwise>
				<div id="maBanner"></div>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="EmitDirectory">
    <xsl:param name="page"/>
			<div id="maDir" onmouseover="maClient.dirMouseOver();">
				<table cellpadding="0" cellspacing="0" style="padding-top:1px;height:22px;width:100%;">
					<tr id="maDirTitleBar">
					</tr>
				</table>
			</div>
			<div id="maDirBody" onmouseover="maClient.dirBodyMouseOver();">
				<div id="maDirStatusLine"></div>
				<div id="maDirContent"></div>
			</div>
	</xsl:template>

	<xsl:template name="EmitTitle">
    <xsl:param name="page"/>
    <xsl:choose>
      <xsl:when test="$page/@showDropdown='True'">
        <xsl:variable name="width" select="$page/@pageWidth - @titleOffsetLeft - $page/@layoutMarginRight"/>
				<xsl:variable name="height" select="$page/@pageTitleHeight - @titleOffsetTop - @titleOffsetBottom" />
					<div id="maPageTitle">
          <table width="{$width}" cellpadding="0" cellspacing="0">
            <tr>
              <td style="width:75%;">
								<div id="maPageTitleText"><xsl:value-of select="$page/@pageTitle" /></div>
              </td>
              <td style="width:25%; text-align:right">
                <xsl:call-template name="EmitDropdownList">
                  <xsl:with-param name="page" select="$page"/>
                </xsl:call-template>
              </td>
            </tr>
          </table>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <div id="maPageTitle">
					<div id="maPageTitleText"><xsl:value-of select="$page/@pageTitle" /></div>
        </div>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <xsl:template name="EmitDropdownList">
    <xsl:param name="page"/>
    <select id="maHotspotDropdown" onchange="maClient.showSlide(this[selectedIndex].value,true)" name="slides">
      <xsl:if test="$page/@popupSlides='True'">
        <option value="-1" selected="">
					<xsl:value-of select="$page/@slideListInstructions"/>
				</option>
      </xsl:if>
      <xsl:for-each select="$page/tourView">
				<xsl:if test="substring(@name,1,1)!='_'">
					<xsl:variable name="viewId" select="@viewId"/>
					<xsl:choose>
						<xsl:when test="$viewId=$page/@firstTourViewId and $page/@popupSlides!='True'">
							<option value="{@viewId}" selected="">
								<xsl:value-of select="@name"/>
							</option>
						</xsl:when>
						<xsl:otherwise>
							<option value="{@viewId}">
								<xsl:value-of select="@name"/>
							</option>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:if>
      </xsl:for-each>
    </select>
  </xsl:template>

  <xsl:template name="EmitFooterStripe">
    <div id="maFooterStripe"></div>
  </xsl:template>

  <xsl:template name="EmitFooterLinks">
		<!-- In this template we disable output escaping so that the user can use entities in their text. -->
		<xsl:if test="@showMapsAliveLink='1' or @showCustomFooter='1'">
			<div id="maFooterLink">
				<xsl:if test="@showCustomFooter='1'">
					<xsl:value-of select="@tourFooterText1" disable-output-escaping="yes"/>
					<xsl:if test="@tourFooterLinkText!=''">
						<a class="maFooterLink" target="_blank" href="{@tourFooterLinkUrl}">
							<xsl:value-of select="@tourFooterLinkText" disable-output-escaping="yes"/>
						</a>
					</xsl:if>
					<xsl:value-of select="@tourFooterText2" disable-output-escaping="yes"/>
				</xsl:if>
				<xsl:if test="@showMapsAliveLink='1'">
					<div style="font-size:11px;margin-top:2px;border:solid 1px #ddd;padding:4px;background-color:#eee;color:777;">
							Created with
							<span style="color:#5b79ab;font-weight:bold;">Maps</span>
							<span style="color:#548e42;font-weight:bold;">Alive</span>
							Free Trial
							<xsl:text disable-output-escaping="yes">&amp;nbsp;&amp;mdash;&amp;nbsp;</xsl:text>
							<a class="maFooterLink" target="_blank" href="https://www.mapsalive.com?ref={@accountId}">www.mapsalive.com</a>
					</div>
				</xsl:if>
			</div>
		</xsl:if>
  </xsl:template>
  
  <xsl:template name="EmitImage">
    <xsl:param name="marginBottom"/>
		<xsl:variable name="commonStyle" select="concat('margin-bottom:',$marginBottom,'px;')"/>
		<div id="maHotspotMediaArea">
			<xsl:attribute name="style"><xsl:value-of select="$commonStyle" /></xsl:attribute>
		</div>
	</xsl:template>

	<xsl:template name="EmitJavascriptDetection">
    <xsl:param name="page"/>
		<xsl:variable name="w" select="$page/@canvasWidth"/>
		<xsl:variable name="h" select="$page/@canvasHeight"/>
		<noscript>
		<div style="position:absolute;left:{$w div 4}px;top:{$h div 4}px;padding:8px;color:green;font-family:sans-serif;font-size:12px;background-color:#f3f3f3;border:solid 1px #777777;">
			This interactive map requires JavaScript
		</div>
		</noscript>
	</xsl:template>

	<xsl:template name="EmitJavascript">
		<xsl:param name="page"/>
		var maTour = {
		version:"<xsl:value-of select="@appVersion" />",
		buildId:"<xsl:value-of select="@buildId" />",
		pageNumber:<xsl:value-of select="$page/@pageNumber" />,
		pageName:"<xsl:value-of select="$page/@pageName" />",
		mapId:"<xsl:value-of select="$page/@mapId" />",
		blinkCount:<xsl:value-of select="$page/@blinkCount" />,
		visitedMarkerAlpha:<xsl:value-of select="$page/@visitedMarkerAlpha" />,
		editMode:false,
		stageW:<xsl:value-of select="$page/@mapAreaWidth" />,
		stageH:<xsl:value-of select="$page/@mapAreaHeight" />,
		mapWidth:<xsl:value-of select="$page/@mapWidth" />,
		mapHeight:<xsl:value-of select="$page/@mapHeight" />,
		mapAreaScale:<xsl:value-of select="$page/@mapAreaScale" />,
		runSlideShow:<xsl:choose><xsl:when test="$page/@runSlideShow='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		slideShowInterval:<xsl:value-of select="$page/@slideShowInterval" />,
		mapInsetLocation:<xsl:value-of select="$page/@mapInsetLocation" />,
		mapInsetSize:<xsl:value-of select="$page/@mapInsetSize" />,
		mapInsetColor:"<xsl:value-of select="$page/@mapInsetColor" />",
		mapZoomControlColor:"<xsl:value-of select="$page/@mapZoomControlColor" />",
		infoPage:<xsl:choose><xsl:when test="@infoPage='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		hasDirectory:<xsl:choose><xsl:when test="@hasDirectory='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		firstImageSrc:"<xsl:value-of select="$page/@imageSrc" />",
		marker:{
		viewId:0,
		x:0,
		y:0,
		w:0,
		h:0,
		absX:0,
		absY:0,
		rectTop:null,
		rectRight:null,
		rectBottom:null,
		rectLeft:null},
		mobileDeviceFeaturesEnabled:<xsl:choose><xsl:when test="@mobileDeviceFeaturesEnabled='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		desktopDefaultIsHtml5:<xsl:choose><xsl:when test="@desktopDefaultIsHtml5='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		mobileDefaultIsFlash:<xsl:choose><xsl:when test="@mobileDefaultIsFlash='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		selectsOnTouchStart:<xsl:choose><xsl:when test="@selectsOnTouchStart='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		enlargeHitTestArea:<xsl:choose><xsl:when test="@enlargeHitTestArea='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		disableBlendEffect:<xsl:choose><xsl:when test="@disableBlendEffect='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		disableSmoothPanning:<xsl:choose><xsl:when test="@disableSmoothPanning='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		showZoomControlOnIOs:<xsl:choose><xsl:when test="@showZoomControlOnIOs='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		entirePopupVisible:<xsl:choose><xsl:when test="@entirePopupVisible='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		enableImagePreloading:<xsl:choose><xsl:when test="@enableImagePreloading='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		useTouchUiOnDeskop:<xsl:choose><xsl:when test="@useTouchUiOnDeskop='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		markerInstanceTable:[<xsl:value-of select="markerInstanceTable" />],
		markerStyleTable:[<xsl:value-of select="markerStyleTable" />],
		pageId:"<xsl:value-of select="$page/@pageId" />",
		mapFileName:"<xsl:value-of select="$page/@mapFileName" />",
		mapInsetFileName:"<xsl:value-of select="$page/@mapInsetFileName" />",
		mapSwfName:"<xsl:value-of select="$page/@mapLibrary" />",
		mapZoomLevel:"<xsl:value-of select="$page/@mapZoomLevel" />",
		mapZoomMidLevel:<xsl:value-of select="$page/@mapZoomMidLevel" />,
		mapPanX:"<xsl:value-of select="$page/@mapPanX" />",
		mapPanY:"<xsl:value-of select="$page/@mapPanY" />",
		mapShowZoomControl:<xsl:choose><xsl:when test="$page/@mapShowZoomControl='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		stringTable:[<xsl:value-of select="stringTable" disable-output-escaping="yes" />],
		pageTable:[<xsl:value-of select="pageTable" />],
		slideTable:[<xsl:value-of select="slideTable" />],
		categoryTable:[<xsl:value-of select="categoryTable" />],
		<xsl:if test="@hasDirectory='True'">
		dir:{
			table:[<xsl:value-of select="@dirTable" />],
			location:<xsl:value-of select="@dirLocation"/>,
			locationX:<xsl:value-of select="@dirLocationX"/>,
			locationY:<xsl:value-of select="@dirLocationY"/>,
			contentWidth:<xsl:value-of select="@dirContentWidth"/>,
			alignContentRight:<xsl:choose><xsl:when test="@dirAlignContentRight='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
			autoCollapse:<xsl:choose><xsl:when test="@dirAutoCollapse='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
			backgroundColor:"<xsl:value-of select="@dirBackgroundColor"/>",
			maxHeight:<xsl:value-of select="@dirMaxHeight"/>,
			previewOnRight:<xsl:choose><xsl:when test="@dirPreviewOnRight='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
			previewImageWidth:<xsl:value-of select="@dirPreviewImageWidth"/>,
			previewWidth:<xsl:value-of select="@dirPreviewWidth"/>,
			showImagePreview:<xsl:choose><xsl:when test="@dirShowImagePreview='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
			showGroupSort:<xsl:choose><xsl:when test="@dirShowGroupSort='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
			showSearch:<xsl:choose><xsl:when test="@dirShowSearch='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
			showTextPreview:<xsl:choose><xsl:when test="@dirShowTextPreview='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
			staysOpen:<xsl:choose><xsl:when test="@dirStaysOpen='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
			openExpanded:<xsl:choose><xsl:when test="@dirOpenExpanded='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
			titleBarColor:"<xsl:value-of select="@dirTitleBarColor"/>",
			titleBarWidth:"<xsl:value-of select="@dirTitleBarWidth"/>",
			textAlphaSortTooltip:"<xsl:value-of select="@dirTextAlphaSortTooltip"/>",
			textClearButtonLabel:"<xsl:value-of select="@dirTextClearButtonLabel"/>",
			textGroupSortTooltip:"<xsl:value-of select="@dirTextGroupSortTooltip"/>",
			textNoSearchMessage:"<xsl:value-of select="@dirTextNoSearchMessage"/>",
			textSearchLabel:"<xsl:value-of select="@dirTextSearchLabel"/>",
			textSearchResultsMessage:"<xsl:value-of select="@dirTextSearchResultsMessage"/>",
			textTitle:"<xsl:value-of select="@dirTextTitle"/>"},
		</xsl:if>
		showInstructions:<xsl:choose><xsl:when test="$page/@showInstructions='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		<xsl:if test="$page/@showInstructions='True'">
			instructions:{
			title:"<xsl:value-of select="$page/@instructionsTitle" />",
			text:"<xsl:value-of select="$page/@instructionsText"/>",
			width:<xsl:value-of select="$page/@instructionsWidth"/>,
			bgColor:"<xsl:value-of select="$page/@instructionsBgColor"/>",
			color:"<xsl:value-of select="$page/@instructionsColor"/>"},
		</xsl:if>
		selectedViewId:<xsl:value-of select="$page/@firstTourViewId" />,
		accountId:<xsl:value-of select="@accountId"/>,
		layout:"<xsl:value-of select="$page/@layoutId"/>",
		canvasW:<xsl:value-of select="$page/@canvasWidth"/>,
		canvasH:<xsl:value-of select="$page/@canvasHeight"/>,
		mediaAreaW:<xsl:value-of select="$page/@imageAreaWidth"/>,
		mediaAreaH:<xsl:value-of select="$page/@imageAreaHeight"/>,
		textAreaW:<xsl:value-of select="$page/@textAreaWidth"/>,
		textAreaH:<xsl:value-of select="$page/@textAreaHeight"/>,
		layoutSpacingH:<xsl:value-of select="$page/@layoutSpacingH"/>,
		layoutSpacingV:<xsl:value-of select="$page/@layoutSpacingV"/>,
		layoutMarginTop:<xsl:value-of select="$page/@layoutMarginTop"/>,
		layoutMarginRight:<xsl:value-of select="$page/@layoutMarginRight"/>,
		layoutMarginBottom:<xsl:value-of select="$page/@layoutMarginBottom"/>,
		layoutMarginLeft:<xsl:value-of select="$page/@layoutMarginLeft"/>,
		showSlideTitle:<xsl:choose><xsl:when test="$page/@showSlideTitle='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		usesFixedPopup:<xsl:choose><xsl:when test="$page/@popupSlidesFixed='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		usesDynamicPopup:<xsl:choose><xsl:when test="$page/@popupSlidesDynamic='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		usesPopup:<xsl:choose><xsl:when test="$page/@popupSlides='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose><xsl:if test="$page/@popupSlides='True'">,
		width:<xsl:value-of select="$page/@pageWidth"/>,
		height:<xsl:value-of select="$page/@pageHeight"/>,
		popup:{
			location:<xsl:value-of select="$page/@popupLocation"/>,
			bestSideSequence:<xsl:value-of select="$page/@popupBestSideSequence"/>,
			allowMouseover:<xsl:choose><xsl:when test="$page/@popupAllowMouseover='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
			delayType:<xsl:value-of select="$page/@popupDelayType"/>,
			delay:<xsl:value-of select="$page/@popupDelay"/>,
			initialized:<xsl:choose><xsl:when test="$page/@popupInitialized='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
			pinOnClick:<xsl:choose><xsl:when test="$page/@popupPinOnClick='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
			pinMsg:"<xsl:value-of select="$page/@popupPinMsg"/>",
			arrowType:<xsl:value-of select="$page/@popupArrowType"/>,
			markerOffset:<xsl:value-of select="$page/@popupMarkerOffset"/>,
			cornerRadius:<xsl:value-of select="$page/@popupCornerRadius"/>,
			imageRadius:<xsl:value-of select="$page/@popupImageCornerRadius"/>,
			dropShadowDistance:<xsl:value-of select="$page/@popupDropShadowDistance"/>,
			maxW:<xsl:value-of select="$page/@popupWidth"/>,
			maxH:<xsl:value-of select="$page/@popupHeight"/>,
			minW:<xsl:value-of select="$page/@popupMinWidth"/>,
			minH:<xsl:value-of select="$page/@popupMinHeight"/>,
			textOnlyW:<xsl:value-of select="$page/@popupTextOnlyWidth"/>,
			actualW:0,
			actualH:0,
			borderWidth:<xsl:value-of select="$page/@popupBorderWidth"/>,
			backgroundColor:"<xsl:value-of select="$page/@colorPopupBackground"/>",
			borderColor:"<xsl:value-of select="$page/@colorPopupBorder"/>",
			borderBottom:"<xsl:value-of select="$page/@popupBorderWidth"/>px solid <xsl:value-of select="$page/@colorPopupBorder"/>",
			borderLeft:"<xsl:value-of select="$page/@popupBorderWidth"/>px solid <xsl:value-of select="$page/@colorPopupBorder"/>",
			borderRight:"<xsl:value-of select="$page/@popupBorderWidth"/>px solid <xsl:value-of select="$page/@colorPopupBorder"/>",
			borderTop:"<xsl:value-of select="$page/@popupBorderWidth"/>px solid <xsl:value-of select="$page/@colorPopupBorder"/>"<xsl:if test="$page/@popupSlidesFixed='True'">,
			left:<xsl:value-of select="$page/@popupLocationX"/>,
			top:<xsl:value-of select="$page/@popupLocationY"/></xsl:if>}</xsl:if>};

		<xsl:if test="routes">
			var routesTable = {
			<xsl:for-each select="routes/route">
				<xsl:if test="position()!=1">,</xsl:if>
				<xsl:value-of select="@id"/>:{route:'<xsl:value-of select="."/>'}
			</xsl:for-each>
			}
		</xsl:if>

		<xsl:value-of select="@uaDetectionJs" disable-output-escaping="yes"/>
	</xsl:template>

  <xsl:template name="EmitLayout">
    <xsl:param name="page"/>
    <xsl:param name="canvasWidth"/>

    <xsl:variable name="layoutId" select="$page/@layoutId"/>

		<!-- The layout div contains the page's hotspot content. -->
		<div id="maLayout">

      <xsl:choose>
        <xsl:when test="$page/@popupSlides='True'">
          <!-- This page has popup views, so we only want to emit the map on the page.
               The popup layout is emitted separately as a hidden div that is absolute positioned. -->
          <xsl:call-template name="EmitLayoutMapOnly">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='VIITT'">
          <xsl:call-template name="EmitLayoutVIITT">
            <xsl:with-param name="page" select="$page" />
            <xsl:with-param name="borderWidth" select="0"/>
          </xsl:call-template>
        </xsl:when>
        
        <xsl:when test="$layoutId='VTTII'">
          <xsl:call-template name="EmitLayoutVTTII">
            <xsl:with-param name="page" select="$page" />
            <xsl:with-param name="borderWidth" select="0"/>
          </xsl:call-template>
        </xsl:when>
        
        <xsl:when test="$layoutId='HIITT'">
          <xsl:call-template name="EmitLayoutHIITT">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>
        
        <xsl:when test="$layoutId='HTTII'">
          <xsl:call-template name="EmitLayoutHTTII">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>
        
        <xsl:when test="$layoutId='HII'">
          <xsl:call-template name="EmitLayoutHII">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>
        
        <xsl:when test="$layoutId='HTT'">
          <xsl:call-template name="EmitLayoutHTT">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <!-- The remaining layouts are regular layouts with map, image and text -->
        <xsl:when test="$layoutId='HMTII'">
          <xsl:call-template name="EmitLayoutImageBottom1">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='HTMII'">
          <xsl:call-template name="EmitLayoutImageBottom2">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='VIIMT'">
          <xsl:call-template name="EmitLayoutImageLeft1">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='VIITM'">
          <xsl:call-template name="EmitLayoutImageLeft2">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='VMTII'">
          <xsl:call-template name="EmitLayoutImageRight1">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='VTMII'">
          <xsl:call-template name="EmitLayoutImageRight2">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='HIIMT'">
          <xsl:call-template name="EmitLayoutImageTop1">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='HIITM'">
          <xsl:call-template name="EmitLayoutImageTop2">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='HITMM'">
          <xsl:call-template name="EmitLayoutMapBottom1">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='HTIMM'">
          <xsl:call-template name="EmitLayoutMapBottom2">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='HIIMM'">
          <xsl:call-template name="EmitLayoutMapBottom3">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='HTTMM'">
          <xsl:call-template name="EmitLayoutMapBottom4">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='VMMIT'">
          <xsl:call-template name="EmitLayoutMapLeft1">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='VMMTI'">
          <xsl:call-template name="EmitLayoutMapLeft2">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='VMMII'">
          <xsl:call-template name="EmitLayoutMapLeft3">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='VMMTT'">
          <xsl:call-template name="EmitLayoutMapLeft4">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='HMM'">
          <xsl:call-template name="EmitLayoutMapOnly">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='VITMM'">
          <xsl:call-template name="EmitLayoutMapRight1">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='VTIMM'">
          <xsl:call-template name="EmitLayoutMapRight2">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='VIIMM'">
          <xsl:call-template name="EmitLayoutMapRight3">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='VTTMM'">
          <xsl:call-template name="EmitLayoutMapRight4">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='HMMIT'">
          <xsl:call-template name="EmitLayoutMapTop1">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='HMMTI'">
          <xsl:call-template name="EmitLayoutMapTop2">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='HMMII'">
          <xsl:call-template name="EmitLayoutMapTop3">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='HMMTT'">
          <xsl:call-template name="EmitLayoutMapTop4">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='HMITT'">
          <xsl:call-template name="EmitLayoutTextBottom1">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='HIMTT'">
          <xsl:call-template name="EmitLayoutTextBottom2">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='VTTMI'">
          <xsl:call-template name="EmitLayoutTextLeft1">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='VTTIM'">
          <xsl:call-template name="EmitLayoutTextLeft2">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='VMITT'">
          <xsl:call-template name="EmitLayoutTextRight1">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='VIMTT'">
          <xsl:call-template name="EmitLayoutTextRight2">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='HTTMI'">
          <xsl:call-template name="EmitLayoutTextTop1">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="$layoutId='HTTIM'">
          <xsl:call-template name="EmitLayoutTextTop2">
            <xsl:with-param name="page" select="$page" />
          </xsl:call-template>
        </xsl:when>

      </xsl:choose>

    </div>
  </xsl:template>

  <!--  LAYOUT TEMPLATES -->

  <!-- LAYOUT ImageBottom1 - HMTII -->
  <xsl:template name="EmitLayoutImageBottom1">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
    </xsl:call-template>
    <xsl:call-template name="EmitImage" >
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosText">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@mapAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="width" select="$page/@textAreaWidth" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT ImageBottom2 - HTMII -->
  <xsl:template name="EmitLayoutImageBottom2">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginTop" select="0" />
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
      <xsl:with-param name="marginRight" select="$page/@layoutMarginRight" />
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitImage">
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosMap">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@textAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="width" select="$page/@mapAreaWidth" />
      <xsl:with-param name="height" select="$page/@mapAreaHeight" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT ImageLeft1 - VIIMT -->
  <xsl:template name="EmitLayoutImageLeft1">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitImage" >
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosMapText">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@imageAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="marginTop" select="$page/@layoutSpacingH"/>
      <xsl:with-param name="marginBottom" select="$page/@layoutMarginBottom"/>
      <xsl:with-param name="marginLeft" select="0"/>
      <xsl:with-param name="marginRight" select="$page/@layoutMarginRight"/>     
      <xsl:with-param name="width" select="$page/@mapAreaWidth" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT ImageLeft2 - VIITM -->
  <xsl:template name="EmitLayoutImageLeft2">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitImage" >
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosTextMap">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@imageAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="marginTop" select="0"/>
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH"/>
      <xsl:with-param name="marginLeft" select="0"/>
      <xsl:with-param name="marginRight" select="$page/@layoutMarginRight"/>     
      <xsl:with-param name="width" select="$page/@mapAreaWidth" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT ImageRight1 - VMTII -->
  <xsl:template name="EmitLayoutImageRight1">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
    </xsl:call-template>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
      <xsl:with-param name="marginTop" select="$page/@layoutSpacingH" />
      <xsl:with-param name="marginRight" select="$page/@layoutSpacingV" />
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosImage">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@mapAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="width" select="$page/@imageAreaWidth" />
      <xsl:with-param name="height" select="$page/@imageAreaHeight" />
      <xsl:with-param name="margin" select="0" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT ImageRight2 - VTMII -->
  <xsl:template name="EmitLayoutImageRight2">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
      <xsl:with-param name="marginTop" select="0" />
      <xsl:with-param name="marginRight" select="$page/@layoutSpacingV" />
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosImage">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@mapAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="width" select="$page/@imageAreaWidth" />
      <xsl:with-param name="height" select="$page/@imageAreaHeight" />
      <xsl:with-param name="margin" select="0" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT ImageTop1 - HIIMT -->
  <xsl:template name="EmitLayoutImageTop1">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitImage" >
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
    </xsl:call-template>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosText">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop + $page/@imageAreaHeight + $page/@layoutSpacingH" />
      <xsl:with-param name="left" select="$page/@mapAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
			<xsl:with-param name="marginTop" select="$page/@layoutMarginTop"/>
			<xsl:with-param name="marginBottom" select="$page/@layoutMarginBottom"/>
			<xsl:with-param name="marginLeft" select="$page/@layoutMarginLeft"/>
			<xsl:with-param name="marginRight" select="$page/@layoutMarginRight"/>
		</xsl:call-template>
  </xsl:template>

  <!-- LAYOUT ImageTop2 - HIITM -->
  <xsl:template name="EmitLayoutImageTop2">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitImage" >
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
    </xsl:call-template>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
      <xsl:with-param name="marginTop" select="$page/@layoutSpacingH" />
      <xsl:with-param name="marginRight" select="$page/@layoutSpacingV" />
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosMap">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop + $page/@imageAreaHeight + $page/@layoutSpacingH" />
      <xsl:with-param name="left" select="$page/@textAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="width" select="$page/@mapAreaWidth" />
      <xsl:with-param name="height" select="$page/@mapAreaHeight" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT MapBottom1 - HITMM -->
  <xsl:template name="EmitLayoutMapBottom1">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitImage" >
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
    </xsl:call-template>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosText">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@imageAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
			<xsl:with-param name="marginTop" select="$page/@layoutMarginTop"/>
			<xsl:with-param name="marginBottom" select="$page/@layoutMarginBottom"/>
			<xsl:with-param name="marginLeft" select="$page/@layoutMarginLeft"/>
			<xsl:with-param name="marginRight" select="$page/@layoutMarginRight"/>
		</xsl:call-template>
  </xsl:template>

  <!-- LAYOUT MapBottom2 - HTIMM -->
  <xsl:template name="EmitLayoutMapBottom2">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
      <xsl:with-param name="marginTop" select="$page/@layoutMarginTop" />
      <xsl:with-param name="marginRight" select="$page/@layoutSpacingV" />
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosImage">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@textAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="width" select="$page/@imageAreaWidth" />
      <xsl:with-param name="height" select="$page/@imageAreaHeight" />
      <xsl:with-param name="margin" select="$page/@layoutSpacingH" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT MapBottom3 - HIIMM -->
  <xsl:template name="EmitLayoutMapBottom3">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitImage" >
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
    </xsl:call-template>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>

  </xsl:template>

  <!-- LAYOUT MapBottom4 - HTTMM -->
  <xsl:template name="EmitLayoutMapBottom4">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
      <xsl:with-param name="marginTop" select="0" />
      <xsl:with-param name="marginRight" select="0" />
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT MapLeft1 - VMMIT -->
  <xsl:template name="EmitLayoutMapLeft1">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosImageText">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@mapAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="marginTop" select="$page/@layoutSpacingH"/>
      <xsl:with-param name="marginBottom" select="$page/@layoutMarginBottom"/>
      <xsl:with-param name="marginLeft" select="0"/>
      <xsl:with-param name="marginRight" select="$page/@layoutMarginRight"/>
      <xsl:with-param name="width" select="$page/@imageAreaWidth" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT MapLeft2 - VMMTI -->
  <xsl:template name="EmitLayoutMapLeft2">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosTextImage">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@mapAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="marginTop" select="0"/>
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH"/>
      <xsl:with-param name="marginLeft" select="0"/>
      <xsl:with-param name="marginRight" select="$page/@layoutMarginRight"/>
      <xsl:with-param name="width" select="$page/@imageAreaWidth" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT MapLeft3 - VMMII -->
  <xsl:template name="EmitLayoutMapLeft3">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosImage">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@mapAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="width" select="$page/@imageAreaWidth" />
      <xsl:with-param name="height" select="$page/@imageAreaHeight" />
      <xsl:with-param name="margin" select="0" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT MapLeft4 - VMMTT -->
  <xsl:template name="EmitLayoutMapLeft4">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosText">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@mapAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
			<xsl:with-param name="marginTop" select="$page/@layoutMarginTop"/>
			<xsl:with-param name="marginBottom" select="$page/@layoutMarginBottom"/>
			<xsl:with-param name="marginLeft" select="$page/@layoutMarginLeft"/>
			<xsl:with-param name="marginRight" select="$page/@layoutMarginRight"/>
		</xsl:call-template>
  </xsl:template>

  <!-- LAYOUT MapOnly - HMM -->
  <xsl:template name="EmitLayoutMapOnly">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT MapRight1 - VITMM -->
  <xsl:template name="EmitLayoutMapRight1">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitImage" >
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
    </xsl:call-template>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
      <xsl:with-param name="marginTop" select="$page/@layoutSpacingH" />
      <xsl:with-param name="marginRight" select="$page/@layoutSpacingV" />
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosMap">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@textAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="width" select="$page/@mapAreaWidth" />
      <xsl:with-param name="height" select="$page/@mapAreaHeight" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT MapRight2 - VTIMM -->
  <xsl:template name="EmitLayoutMapRight2">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
      <xsl:with-param name="marginTop" select="0" />
      <xsl:with-param name="marginRight" select="$page/@layoutSpacingV" />
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitImage" >
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosMap">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@textAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="width" select="$page/@mapAreaWidth" />
      <xsl:with-param name="height" select="$page/@mapAreaHeight" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT MapRight3 - VIIMM -->
  <xsl:template name="EmitLayoutMapRight3">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitImage" >
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosMap">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@imageAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="width" select="$page/@mapAreaWidth" />
      <xsl:with-param name="height" select="$page/@mapAreaHeight" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT MapRight4 - VTTMM -->
  <xsl:template name="EmitLayoutMapRight4">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
      <xsl:with-param name="marginTop" select="0" />
      <xsl:with-param name="marginRight" select="$page/@layoutSpacingV" />
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosMap">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@textAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="width" select="$page/@mapAreaWidth" />
      <xsl:with-param name="height" select="$page/@mapAreaHeight" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT MapTop1 - HMMIT -->
  <xsl:template name="EmitLayoutMapTop1">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
    </xsl:call-template>
    <xsl:call-template name="EmitImage" >
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosText">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@mapAreaHeight + $page/@layoutSpacingH + $page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@imageAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
			<xsl:with-param name="marginTop" select="$page/@layoutMarginTop"/>
			<xsl:with-param name="marginBottom" select="$page/@layoutMarginBottom"/>
			<xsl:with-param name="marginLeft" select="$page/@layoutMarginLeft"/>
			<xsl:with-param name="marginRight" select="$page/@layoutMarginRight"/>
		</xsl:call-template>
  </xsl:template>

  <!-- LAYOUT MapTop2 - HMMTI -->
  <xsl:template name="EmitLayoutMapTop2">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
    </xsl:call-template>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
      <xsl:with-param name="marginTop" select="$page/@layoutSpacingH" />
      <xsl:with-param name="marginRight" select="$page/@layoutSpacingV" />
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosImage">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@mapAreaHeight + $page/@layoutSpacingH + $page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@textAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="width" select="$page/@imageAreaWidth" />
      <xsl:with-param name="height" select="$page/@imageAreaHeight" />
      <xsl:with-param name="margin" select="0" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT MapTop3 - HMMII -->
  <xsl:template name="EmitLayoutMapTop3">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
    </xsl:call-template>
    <xsl:call-template name="EmitImage" >
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT MapTop4 - HMMTT -->
  <xsl:template name="EmitLayoutMapTop4">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
    </xsl:call-template>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
      <xsl:with-param name="marginTop" select="$page/@layoutSpacingH" />
      <xsl:with-param name="marginRight" select="0" />
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT TextBottom1 - HMITT -->
  <xsl:template name="EmitLayoutTextBottom1">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
    </xsl:call-template>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
      <xsl:with-param name="marginTop" select="$page/@layoutSpacingH" />
      <xsl:with-param name="marginRight" select="0" />
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosImage">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@mapAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="width" select="$page/@imageAreaWidth" />
      <xsl:with-param name="height" select="$page/@imageAreaHeight" />
      <xsl:with-param name="margin" select="$page/@layoutSpacingH" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT TextBottom2 - HIMTT -->
  <xsl:template name="EmitLayoutTextBottom2">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitImage" >
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
    </xsl:call-template>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
      <xsl:with-param name="marginTop" select="$page/@layoutSpacingH" />
      <xsl:with-param name="marginRight" select="0" />
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosMap">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@imageAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="width" select="$page/@mapAreaWidth" />
      <xsl:with-param name="height" select="$page/@mapAreaHeight" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT TextLeft1 - VTTMI -->
  <xsl:template name="EmitLayoutTextLeft1">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
      <xsl:with-param name="marginTop" select="0" />
      <xsl:with-param name="marginRight" select="$page/@layoutSpacingV" />
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosMapImage">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@textAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="width" select="$page/@imageAreaWidth" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT TextLeft2 - VTTIM -->
  <xsl:template name="EmitLayoutTextLeft2">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
      <xsl:with-param name="marginTop" select="0" />
      <xsl:with-param name="marginRight" select="$page/@layoutSpacingV" />
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosImageMap">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@textAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
      <xsl:with-param name="width" select="$page/@imageAreaWidth" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT TextRight1 - VMITT -->
  <xsl:template name="EmitLayoutTextRight1">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
    </xsl:call-template>
    <xsl:call-template name="EmitImage" >
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosText">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@mapAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
			<xsl:with-param name="marginTop" select="$page/@layoutMarginTop"/>
			<xsl:with-param name="marginBottom" select="$page/@layoutMarginBottom"/>
			<xsl:with-param name="marginLeft" select="$page/@layoutMarginLeft"/>
			<xsl:with-param name="marginRight" select="$page/@layoutMarginRight"/>
		</xsl:call-template>
  </xsl:template>

  <!-- LAYOUT TextRight2 - VIMTT -->
  <xsl:template name="EmitLayoutTextRight2">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitImage" >
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
    </xsl:call-template>
    <xsl:call-template name="EmitMap" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosText">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$page/@layoutMarginTop" />
      <xsl:with-param name="left" select="$page/@mapAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
			<xsl:with-param name="marginTop" select="$page/@layoutMarginTop"/>
			<xsl:with-param name="marginBottom" select="$page/@layoutMarginBottom"/>
			<xsl:with-param name="marginLeft" select="$page/@layoutMarginLeft"/>
			<xsl:with-param name="marginRight" select="$page/@layoutMarginRight"/>
		</xsl:call-template>
	</xsl:template>

	<!-- LAYOUT TextTop1 - HTTMI -->
	<xsl:template name="EmitLayoutTextTop1">
		<xsl:param name="page"/>
		<xsl:call-template name="EmitTextFixed" >
			<xsl:with-param name="page" select="$page" />
			<xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
			<xsl:with-param name="marginTop" select="0" />
			<xsl:with-param name="marginRight" select="0"/>
			<xsl:with-param name="marginLeft" select="0" />
		</xsl:call-template>
		<xsl:call-template name="EmitMap" >
			<xsl:with-param name="page" select="$page" />
			<xsl:with-param name="marginBottom" select="0" />
		</xsl:call-template>
		<xsl:call-template name="EmitAbsPosImage">
			<xsl:with-param name="page" select="$page"/>
			<xsl:with-param name="top" select="$page/@layoutMarginTop + $page/@textAreaHeight + $page/@layoutSpacingH"/>
			<xsl:with-param name="left" select="$page/@mapAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft"/>
			<xsl:with-param name="width" select="$page/@imageAreaWidth"/>
			<xsl:with-param name="height" select="$page/@imageAreaHeight"/>
			<xsl:with-param name="margin" select="0"/>
		</xsl:call-template>
	</xsl:template>

	<!-- LAYOUT TextTop2 - HTTIM -->
	<xsl:template name="EmitLayoutTextTop2">
		<xsl:param name="page"/>
		<xsl:call-template name="EmitTextFixed" >
			<xsl:with-param name="page" select="$page" />
			<xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
			<xsl:with-param name="marginTop" select="0" />
			<xsl:with-param name="marginRight" select="0"/>
			<xsl:with-param name="marginLeft" select="0" />
		</xsl:call-template>
		<xsl:call-template name="EmitImage" >
			<xsl:with-param name="marginBottom" select="0" />
		</xsl:call-template>
		<xsl:call-template name="EmitAbsPosMap">
			<xsl:with-param name="page" select="$page" />
			<xsl:with-param name="top" select="$page/@layoutMarginTop + $page/@textAreaHeight + $page/@layoutSpacingH" />
			<xsl:with-param name="left" select="$page/@imageAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft" />
			<xsl:with-param name="width" select="$page/@mapAreaWidth" />
			<xsl:with-param name="height" select="$page/@mapAreaHeight" />
		</xsl:call-template>
	</xsl:template>

	<!-- LAYOUT VIITT (No Map; Image and Text Only) -->
	<xsl:template name="EmitLayoutVIITT">
		<xsl:param name="page"/>
		<xsl:param name="borderWidth"/>
		<xsl:call-template name="EmitImage">
			<xsl:with-param name="marginBottom" select="0" />
		</xsl:call-template>
		<xsl:call-template name="EmitAbsPosText">
			<xsl:with-param name="page" select="$page" />
			<xsl:with-param name="top" select="$page/@layoutMarginTop + $borderWidth" />
			<xsl:with-param name="left" select="$page/@imageAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft + $borderWidth" />
			<xsl:with-param name="marginTop" select="$page/@layoutMarginTop"/>
			<xsl:with-param name="marginBottom" select="$page/@layoutMarginBottom"/>
			<xsl:with-param name="marginLeft" select="$page/@layoutMarginLeft"/>
			<xsl:with-param name="marginRight" select="$page/@layoutMarginRight"/>
		</xsl:call-template>
  </xsl:template>

  <!-- LAYOUT VTTII (No Map; Text and Image Only) -->
  <xsl:template name="EmitLayoutVTTII">
    <xsl:param name="page"/>
    <xsl:param name="borderWidth"/>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="$page/@layoutMarginBottom" />
      <xsl:with-param name="marginTop" select="0" />
      <xsl:with-param name="marginRight" select="$page/@layoutSpacingV"/>
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitAbsPosImage">
      <xsl:with-param name="page" select="$page"/>
      <xsl:with-param name="top" select="$page/@layoutMarginTop + $borderWidth"/>
      <xsl:with-param name="left" select="$page/@textAreaWidth + $page/@layoutSpacingV + $page/@layoutMarginLeft + $borderWidth"/>
      <xsl:with-param name="width" select="$page/@imageAreaWidth"/>
      <xsl:with-param name="height" select="$page/@imageAreaHeight"/>
      <xsl:with-param name="margin" select="0"/>
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT HIITT (No Map; Image and Text Only) -->
  <xsl:template name="EmitLayoutHIITT">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitImage" >
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
    </xsl:call-template>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="$page/@layoutMarginBottom" />
      <xsl:with-param name="marginTop" select="$page/@layoutSpacingH" />
      <xsl:with-param name="marginRight" select="0"/>
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT HTTII (No Map; Text and Image Only) -->
  <xsl:template name="EmitLayoutHTTII">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
      <xsl:with-param name="marginTop" select="0" />
      <xsl:with-param name="marginRight" select="0"/>
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
    <xsl:call-template name="EmitImage" >
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT HII (No Map; Image Only) -->
  <xsl:template name="EmitLayoutHII">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitImage" >
      <xsl:with-param name="marginBottom" select="0" />
    </xsl:call-template>
  </xsl:template>

  <!-- LAYOUT HTT (No Map; Text Only) -->
  <xsl:template name="EmitLayoutHTT">
    <xsl:param name="page"/>
    <xsl:call-template name="EmitTextFixed" >
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="marginBottom" select="0" />
      <xsl:with-param name="marginTop" select="0" />
      <xsl:with-param name="marginRight" select="0"/>
      <xsl:with-param name="marginLeft" select="0" />
    </xsl:call-template>
  </xsl:template>
  
  <xsl:template name="EmitLeftMenu">
    <xsl:param name="page"/>
    <xsl:param name="canvasWidth"/>
    <xsl:param name="canvasHeight"/>
    <xsl:variable name="tableWidth" select="@leftNavWidth + $canvasWidth"/>
    <table cellspacing="0" cellpadding="0" style="width:{$tableWidth}px;height:{$canvasHeight}px;">
      <tr>
        <!-- Left Menu-->
        <td id="maLeftMenuCell">
          <div id="maLeftMenu">
              <xsl:call-template name="EmitLeftMenuItems">
                <xsl:with-param name="page" select="$page" />
              </xsl:call-template>
          </div>
        </td>
        <td id="maLayoutCell">
          <xsl:call-template name="EmitLayout">
            <xsl:with-param name="page" select="$page"/>
            <xsl:with-param name="canvasWidth" select="$canvasWidth"/>
          </xsl:call-template>
        </td>
      </tr>
    </table>
  </xsl:template>

  <xsl:template name="EmitMap">
    <xsl:param name="page"/>
    <xsl:param name="marginBottom"/>
		<xsl:variable name="popups">
			<xsl:choose>
				<xsl:when test="$page/@popupSlides='True'">true</xsl:when>
				<xsl:otherwise>false</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<!-- Force a line break before the opening tag. -->
		<xsl:text>
		</xsl:text>
		<div id="maMap" style="position:relative;width:{$page/@mapAreaWidth}px;height:{$page/@mapAreaHeight}px;overflow:hidden;margin-bottom:{$marginBottom}px;">
			<script type="text/javascript">
				var mapViewer=new maClient.MapViewer();
			</script>
		</div>
		<!-- Force a line break after the closing tag. -->
		<xsl:text>
		</xsl:text>
  </xsl:template>

  <xsl:template name="EmitMenuAndLayout">
    <xsl:param name="page"/>
    <xsl:param name="canvasWidth"/>
    <xsl:param name="canvasHeight"/>
    <xsl:param name="pageWidth"/>
    <xsl:choose>
      <xsl:when test="@navigationId='1'">
        <xsl:call-template name="EmitNoMenu">
          <xsl:with-param name="page" select="$page"/>
          <xsl:with-param name="canvasWidth" select="$canvasWidth"/>
        </xsl:call-template>
      </xsl:when>

      <xsl:when test="@navigationId='2'">
        <xsl:call-template name="EmitLeftMenu">
          <xsl:with-param name="page" select="$page"/>
          <xsl:with-param name="canvasWidth" select="$canvasWidth"/>
          <xsl:with-param name="canvasHeight" select="$canvasHeight"/>
        </xsl:call-template>
      </xsl:when>

      <xsl:when test="@navigationId='3'">
        <xsl:call-template name="EmitTopMenu">
          <xsl:with-param name="page" select="$page"/>
          <xsl:with-param name="canvasWidth" select="$canvasWidth"/>
          <xsl:with-param name="pageWidth" select="$pageWidth"/>
        </xsl:call-template>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="EmitLeftMenuItems">
    <xsl:param name="page"/>
    <xsl:variable name="pages" select="/tour/tourPages/tourPage"/>
    <xsl:variable name="menuStyle" select="@menuStyleId"/>
    <xsl:for-each select="$pages">
			<xsl:if test="@exclude='False'">
				<div class="maLeftMenuPageName">
					<!-- For menu style 3 we need to set the current style for both the page name and for any 
             slide names that we show because they are together in a tab. -->
					<xsl:if test="$menuStyle='3' and @pageId=$page/@pageId">
						<xsl:attribute name="class">maLeftNavPageNameCurrent</xsl:attribute>
					</xsl:if>

					<a href="javascript:maClient.goToPage('page{@pageNumber}.htm', null, null);">
						<!-- If this is the current page set the class to current so its style is correct -->
						<xsl:if test="@pageId=$page/@pageId">
							<xsl:attribute name="class">current</xsl:attribute>
						</xsl:if>
						<xsl:value-of select="@pageName" />
					</a>

					<xsl:if test="@pageId=$page/@pageId and $page/@showSlideNamesInMenu='True'">
						<!-- Emit code to put out the slide names under this page name -->
						<div id="maHotspotNamesInMenu">
							<xsl:for-each select="$pages/tourView">
								<xsl:if test="substring(@name,1,1)!='_'">
									<a href="javascript:maClient.showSlide({@viewId},true);" id="ma{@viewId}" onmouseover="maClient.showMarker({@viewId},true);" onmouseout="maClient.showMarker({@viewId},false);">
										<xsl:value-of select="@name"/>
									</a>
									<br/>
								</xsl:if>
							</xsl:for-each>
						</div>
					</xsl:if>

				</div>
			</xsl:if>
    </xsl:for-each>
  </xsl:template>
  
  <xsl:template name="EmitTopMenuItems">
    <xsl:param name="page"/>
    <xsl:variable name="pages" select="/tour/tourPages/tourPage"/>
		<xsl:variable name="menuStyleId" select="@menuStyleId"/>
		<xsl:for-each select="$pages">
			<xsl:if test="@exclude='False'">
				<li>
					<a href="javascript:maClient.goToPage('page{@pageNumber}.htm', null, null)">
						<xsl:if test="$menuStyleId='2' and position()=last()">
							<xsl:attribute name="style">border-right-width:0px;</xsl:attribute>
						</xsl:if>
						<xsl:if test="@pageId=$page/@pageId">
							<xsl:attribute name="class">current</xsl:attribute>
						</xsl:if>
						<xsl:value-of select="@pageName" />
					</a>
				</li>
			</xsl:if>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="EmitNoMenu">
    <xsl:param name="page"/>
    <xsl:param name="canvasWidth"/>
    <xsl:call-template name="EmitLayout">
      <xsl:with-param name="page" select="$page"/>
      <xsl:with-param name="canvasWidth" select="$canvasWidth"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="EmitHeaderStripe">
    <div id="maHeaderStripe"></div>
  </xsl:template>

  <!-- Templates to emit absolute positioned blocks -->
 
  <xsl:template name="EmitAbsPosImage">
    <xsl:param name="page"/>
    <xsl:param name="top"/>
    <xsl:param name="left"/>
    <xsl:param name="width"/>
    <xsl:param name="height"/>
    <xsl:param name="margin"/>

    <xsl:choose>
      <xsl:when test="@infoPage='True'">
        <div id="maAbsBlock" style="top:{$top}px; left:{$left}px; width:{$width}px; height:{$height}px;">
          <xsl:call-template name="EmitImage" >
            <xsl:with-param name="marginBottom" select="$margin" />
          </xsl:call-template>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <div id="maAbsBlock" style="top:{$top}px; left:{$left}px; width:{$width}px; height:{$height}px;">
          <xsl:call-template name="EmitImage" >
            <xsl:with-param name="marginBottom" select="$margin" />
          </xsl:call-template>
        </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="EmitAbsPosImageMap">
    <xsl:param name="page"/>
    <xsl:param name="top"/>
    <xsl:param name="left"/>
    <xsl:param name="width"/>

    <div id="maAbsBlock" style="top:{$top}px; left:{$left}px; width:{$width}px;">
      <xsl:call-template name="EmitImage" >
        <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
      </xsl:call-template>
      <xsl:call-template name="EmitMap" >
        <xsl:with-param name="page" select="$page" />
        <xsl:with-param name="marginBottom" select="0" />
      </xsl:call-template>
    </div>
  </xsl:template>

  <xsl:template name="EmitAbsPosImageText">
    <xsl:param name="page"/>
    <xsl:param name="top"/>
    <xsl:param name="left"/>
    <xsl:param name="marginTop"/>
    <xsl:param name="marginBottom"/>
    <xsl:param name="marginRight"/>
    <xsl:param name="marginLeft"/>
    <xsl:param name="width"/>

    <div id="maAbsBlock" style="top:{$top}px; left:{$left}px; width:{$width}px;">
      <xsl:call-template name="EmitImage" >
        <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
      </xsl:call-template>
      <xsl:call-template name="EmitTextFixed" >
        <xsl:with-param name="page" select="$page" />
        <xsl:with-param name="marginBottom" select="$marginBottom" />
        <xsl:with-param name="marginTop" select="$marginTop" />
        <xsl:with-param name="marginRight" select="$marginRight"/>
        <xsl:with-param name="marginLeft" select="$marginLeft" />
      </xsl:call-template>
    </div>
  </xsl:template>


  <xsl:template name="EmitAbsPosMap">
    <xsl:param name="page"/>
    <xsl:param name="top"/>
    <xsl:param name="left"/>
    <xsl:param name="width"/>
    <xsl:param name="height"/>

    <div id="maAbsBlock" style="top:{$top}px; left:{$left}px; width:{$width}px; height:{$height}px;">
      <xsl:call-template name="EmitMap" >
        <xsl:with-param name="page" select="$page" />
        <xsl:with-param name="marginBottom" select="0" />
      </xsl:call-template>
    </div>
  </xsl:template>

  <xsl:template name="EmitAbsPosMapImage">
    <xsl:param name="page"/>
    <xsl:param name="top"/>
    <xsl:param name="left"/>
    <xsl:param name="width"/>

    <div id="maAbsBlock" style="top:{$top}px; left:{$left}px; width:{$width}px;">
      <xsl:call-template name="EmitMap" >
        <xsl:with-param name="page" select="$page" />
        <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
      </xsl:call-template>
      <xsl:call-template name="EmitImage" >
        <xsl:with-param name="marginBottom" select="0" />
      </xsl:call-template>
    </div>
  </xsl:template>

  <xsl:template name="EmitAbsPosMapText">
    <xsl:param name="page"/>
    <xsl:param name="top"/>
    <xsl:param name="left"/>
    <xsl:param name="marginTop"/>
    <xsl:param name="marginBottom"/>
    <xsl:param name="marginLeft"/>
    <xsl:param name="marginRight"/>
    <xsl:param name="width"/>

    <div id="maAbsBlock" style="top:{$top}px; left:{$left}px; width:{$width}px;">
      <xsl:call-template name="EmitMap" >
        <xsl:with-param name="page" select="$page" />
        <xsl:with-param name="marginBottom" select="$page/@layoutSpacingH" />
      </xsl:call-template>
      <xsl:call-template name="EmitTextFixed" >
        <xsl:with-param name="page" select="$page" />
        <xsl:with-param name="marginTop" select="$marginTop" />
        <xsl:with-param name="marginBottom" select="$marginBottom" />
        <xsl:with-param name="marginLeft" select="$marginLeft" />
        <xsl:with-param name="marginRight" select="$marginRight" />
      </xsl:call-template>
    </div>
  </xsl:template>

  <xsl:template name="EmitAbsPosText">
    <xsl:param name="page"/>
    <xsl:param name="top"/>
    <xsl:param name="left"/>
    <xsl:param name="marginTop"/>
    <xsl:param name="marginBottom"/>
    <xsl:param name="marginLeft"/>
    <xsl:param name="marginRight"/>

    <xsl:call-template name="EmitTextAbsolute">
      <xsl:with-param name="page" select="$page" />
      <xsl:with-param name="top" select="$top" />
      <xsl:with-param name="left" select="$left" />
      <xsl:with-param name="marginTop" select="0" />
      <xsl:with-param name="marginBottom" select="0" />
      <xsl:with-param name="marginLeft" select="0" />
      <xsl:with-param name="marginRight" select="0" />
    </xsl:call-template>
  </xsl:template>
  
  <xsl:template name="EmitAbsPosTextImage">
    <xsl:param name="page"/>
    <xsl:param name="top"/>
    <xsl:param name="left"/>
    <xsl:param name="marginTop"/>
    <xsl:param name="marginBottom"/>
    <xsl:param name="marginLeft"/>
    <xsl:param name="marginRight"/>
    <xsl:param name="width"/>

    <div id="maAbsBlock" style="top:{$top}px; left:{$left}px; width:{$width}px;">
      <xsl:call-template name="EmitTextFixed">
        <xsl:with-param name="page" select="$page" />
        <xsl:with-param name="marginTop" select="$marginTop" />
        <xsl:with-param name="marginBottom" select="$marginBottom" />
        <xsl:with-param name="marginLeft" select="$marginLeft" />
        <xsl:with-param name="marginRight" select="$marginRight" />
      </xsl:call-template>
      <xsl:call-template name="EmitImage" >
        <xsl:with-param name="marginBottom" select="0" />
      </xsl:call-template>
    </div>
  </xsl:template>

  <xsl:template name="EmitAbsPosTextMap">
    <xsl:param name="page"/>
    <xsl:param name="top"/>
    <xsl:param name="left"/>
    <xsl:param name="marginTop"/>
    <xsl:param name="marginBottom"/>
    <xsl:param name="marginLeft"/>
    <xsl:param name="marginRight"/>
    <xsl:param name="width"/>

    <div id="maAbsBlock" style="top:{$top}px; left:{$left}px; width:{$width}px;">
      <xsl:call-template name="EmitTextFixed">
        <xsl:with-param name="page" select="$page" />
        <xsl:with-param name="marginTop" select="$marginTop" />
        <xsl:with-param name="marginBottom" select="$marginBottom" />
        <xsl:with-param name="marginLeft" select="$marginLeft" />
        <xsl:with-param name="marginRight" select="$marginRight" />
      </xsl:call-template>
      <xsl:call-template name="EmitMap" >
        <xsl:with-param name="page" select="$page" />
        <xsl:with-param name="marginBottom" select="0" />
      </xsl:call-template>
    </div>

  </xsl:template>

  <xsl:template name="EmitTextAbsolute">
    <xsl:param name="page"/>
		<xsl:param name="top"/>
		<xsl:param name="left"/>
		<xsl:param name="marginTop"/>
    <xsl:param name="marginBottom"/>
    <xsl:param name="marginRight"/>
    <xsl:param name="marginLeft"/>

		<xsl:variable name="style" select="concat('position:absolute;top:',$top,'px;left:',$left,'px;margin:',$marginTop,'px ',$marginRight,'px ',$marginBottom,'px ',$marginLeft,'px;')"/>

		<xsl:call-template name="EmitTextArea">
        <xsl:with-param name="page" select="$page" />
        <xsl:with-param name="style" select="$style" />
		</xsl:call-template>
  </xsl:template>

	<xsl:template name="EmitTextFixed">
		<xsl:param name="page"/>
		<xsl:param name="marginTop"/>
		<xsl:param name="marginBottom"/>
		<xsl:param name="marginRight"/>
		<xsl:param name="marginLeft"/>

		<xsl:variable name="style" select="concat('margin:',$marginTop,'px ',$marginRight,'px ',$marginBottom,'px ',$marginLeft,'px;')"/>

		<xsl:call-template name="EmitTextArea">
			<xsl:with-param name="page" select="$page" />
			<xsl:with-param name="style" select="$style" />
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="EmitTextArea">
		<xsl:param name="page"/>
		<xsl:param name="style"/>

		<xsl:variable name="text2MarginTop">
			<xsl:choose>
				<xsl:when test="$page/@showSlideTitle='True'">4</xsl:when>
				<xsl:otherwise>0</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<div id="maTextArea" style="{$style}">
			<div id="maHotspotTitle">
				<xsl:value-of select="$page/@slideTitle"/>
			</div>
			<div id="maHotspotText" style="margin-top:{$text2MarginTop}px;">
				<xsl:value-of select="$page/@slideText" disable-output-escaping="yes"/>
			</div>
		</div>
	</xsl:template>

	<xsl:template name="EmitTopMenu">
    <xsl:param name="page"/>
    <xsl:param name="canvasWidth"/>
    <xsl:param name="pageWidth"/>
  
    <xsl:choose>
      <xsl:when test="@menuStyleId=3">
        <div id="maMenuBackground">
          <div id="maTopMenu">
            <ul id="maTopMenuList">
              <xsl:call-template name="EmitTopMenuItems">
                <xsl:with-param name="page" select="$page"/>
              </xsl:call-template>
            </ul>
          </div>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <div id="maTopMenu">
          <ul id="maTopMenuList">
            <xsl:call-template name="EmitTopMenuItems">
              <xsl:with-param name="page" select="$page"/>
            </xsl:call-template>
          </ul>
        </div>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:call-template name="EmitLayout">
      <xsl:with-param name="page" select="$page"/>
      <xsl:with-param name="canvasWidth" select="$canvasWidth"/>
    </xsl:call-template>
  </xsl:template>

<!-- Templates that emit all the CSS needed for the tour pages -->

	<xsl:template name="EmitCss">
		<xsl:param name="page"/>
		<xsl:variable name="pageWidth" select="$page/@pageWidth" />
		<xsl:variable name="pageHeight" select="$page/@pageHeight" />
		<xsl:variable name="canvasWidth" select="$page/@canvasWidth" />
		<xsl:variable name="canvasHeight" select="$page/@canvasHeight" />
		#maTour {
		<xsl:if test="@hasBackgroundColor='True'">
			background-color:<xsl:value-of select="@colorTourBackground"/>;
		</xsl:if>
			height:<xsl:value-of select="$pageHeight"/>px;
			position:relative;
			width:<xsl:value-of select="$pageWidth"/>px;
		}

		<xsl:if test="@hasCustomHtmlTop">
		#maCustomHtmlTop {
		width:<xsl:value-of select="$pageWidth"/>px;
		}
		</xsl:if>
		
		<xsl:if test="@hasCustomHtmlAbsolute">
		#maCustomHtmlAbsolute {
			position:absolute;
			text-align:left;
			z-index:100;
		}
		</xsl:if>
		
		<xsl:if test="@hasCustomHtmlBottom">
		#maCustomHtmlBottom {
			width:<xsl:value-of select="$pageWidth"/>px;
		}
		</xsl:if>
		
		<xsl:if test="$page/@popupSlides='True'">
		#maPopup {
			text-align:left;
		}
		.maPopup,
		.maPopupPinned {
			<xsl:if test="$page/@popupCornerRadius!=0">
			-moz-border-radius:<xsl:value-of select="$page/@popupCornerRadius"/>px;
			border-radius:<xsl:value-of select="$page/@popupCornerRadius"/>px;
			</xsl:if>
			<xsl:if test="$page/@popupDropShadowDistance!=0">
			-moz-box-shadow:<xsl:value-of select="$page/@popupDropShadowDistance"/>px <xsl:value-of select="$page/@popupDropShadowDistance"/>px 10px rgba(0,0,0,0.3);
			-webkit-box-shadow: <xsl:value-of select="$page/@popupDropShadowDistance"/>px <xsl:value-of select="$page/@popupDropShadowDistance"/>px 10px rgba(0,0,0,0.3);
			box-shadow: <xsl:value-of select="$page/@popupDropShadowDistance"/>px <xsl:value-of select="$page/@popupDropShadowDistance"/>px 10px rgba(0,0,0,0.3);
			</xsl:if>
		}
		<xsl:if test="$page/@popupCornerRadius!=0">
		.maPopupPinned {
			<xsl:if test="$page/@popupCornerRadius!=0">
			-moz-border-radius-topleft:0px;
			border-top-left-radius:0px;
			-moz-border-radius-topright:0px;
			border-top-right-radius:0px;
			</xsl:if>
		}
		</xsl:if>
		<xsl:if test="$page/@popupImageCornerRadius!=0">
		#maHotspotImage {
			-moz-border-radius:<xsl:value-of select="$page/@popupImageCornerRadius"/>px;
			border-radius:<xsl:value-of select="$page/@popupImageCornerRadius"/>px;
		}
		</xsl:if>
		</xsl:if>

			<!-- Emit maTooltip CSS unless this page is a data sheet in which case there will be no tooltip properties. -->
		<xsl:if test="@tooltipBorder">
		#maTooltip {
			z-index:10000;
			visibility:hidden;
			position:absolute;
			border:<xsl:value-of select="@tooltipBorder"/>;
			color:<xsl:value-of select="@tooltipTextColor"/>;
			background-color:<xsl:value-of select="@tooltipBackgroundColor"/>;
			font-size:<xsl:value-of select="@tooltipFontSize"/>px;
			font-family:<xsl:value-of select="@tooltipFontFamily"/>;
			padding:<xsl:value-of select="@tooltipPadding"/>px;
			font-weight:<xsl:value-of select="@tooltipFontWeight"/>;
			font-style:<xsl:value-of select="@tooltipFontStyle"/>;
			text-decoration:<xsl:value-of select="@tooltipUnderline"/>;
			<xsl:if test="@tooltipMaxWidth">
				max-width:<xsl:value-of select="@tooltipMaxWidth"/>px;
			</xsl:if>
			}
		</xsl:if>
		
		<xsl:call-template name="EmitLayoutCSS">
			<xsl:with-param name="canvasWidth" select="$canvasWidth"/>
			<xsl:with-param name="canvasHeight" select="$canvasHeight"/>
			<xsl:with-param name="page" select="$page"/>
		</xsl:call-template>

		<xsl:if test="@hasBanner='True'">
			<xsl:call-template name="EmitBannerCSS">
				<xsl:with-param name="page" select="$page"/>
				<xsl:with-param name="pageWidth" select="$pageWidth"/>
			</xsl:call-template>
		</xsl:if>

		<xsl:if test="@hasDirectory='True'">
			<xsl:call-template name="EmitDirectoryCSS"/>
		</xsl:if>

		<xsl:if test="@hasTitle='True'">
			<xsl:call-template name="EmitTitleCSS">
				<xsl:with-param name="pageWidth" select="$pageWidth"/>
			</xsl:call-template>
		</xsl:if>

		<xsl:if test="@hasHeaderStripe='True'">
			<xsl:call-template name="EmitHeaderStripeCSS">
				<xsl:with-param name="pageWidth" select="$pageWidth"/>
			</xsl:call-template>
		</xsl:if>

		<xsl:if test="@hasFooterStripe='True'">
			<xsl:call-template name="EmitFooterStripeCSS">
				<xsl:with-param name="pageWidth" select="$pageWidth"/>
			</xsl:call-template>
		</xsl:if>

		<xsl:if test="$page/@showDropdown='True'">
			<xsl:call-template name="EmitSlideDropdownCSS">
				<xsl:with-param name="page" select="$page"/>
			</xsl:call-template>
		</xsl:if>

		<xsl:call-template name="EmitTextAreaCSS">
			<xsl:with-param name="page" select="$page"/>
		</xsl:call-template>

		<xsl:call-template name="EmitImageCSS">
			<xsl:with-param name="page" select="$page"/>
		</xsl:call-template>

		<xsl:call-template name="EmitMapCSS">
			<xsl:with-param name="page" select="$page"/>
		</xsl:call-template>

		<xsl:choose>
			<xsl:when test="@navigationId='2'">
				<xsl:call-template name="EmitLeftMenuCSS">
					<xsl:with-param name="page" select="$page"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="@navigationId='3'">
				<xsl:call-template name="EmitTopMenuCSS">
					<xsl:with-param name="page" select="$page"/>
					<xsl:with-param name="pageWidth" select="$pageWidth" />
				</xsl:call-template>
			</xsl:when>
		</xsl:choose>

		<xsl:if test="@showMapsAliveLink='1' or @showCustomFooter='1'">
			<xsl:call-template name="EmitMapsAliveLinkCSS">
				<xsl:with-param name="pageWidth" select="$pageWidth"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<!-- Templates to emit CSS for page elements -->

	<xsl:template name="EmitBannerCSS">
		<xsl:param name="page"/>
		<xsl:param name="pageWidth"/>
		#maBanner {
		background-image:url(<xsl:value-of select="$page/@bannerImg"/>);
		height:<xsl:value-of select="@bannerHeight - @bannerPaddingTop"/>px;
		width:<xsl:value-of select="$pageWidth - @bannerPaddingLeft"/>px;
		<xsl:choose>
			<xsl:when test="bannerRepeat='X'">background-repeat:repeat-x;</xsl:when>
			<xsl:when test="bannerRepeat='Y'">background-repeat:repeat-y;</xsl:when>
			<xsl:when test="bannerRepeat='XY'">background-repeat:repeat;</xsl:when>
			<xsl:otherwise>background-repeat:no-repeat;</xsl:otherwise>
		</xsl:choose>
		<xsl:choose>
			<xsl:when test="bannerPosition='left'">background-position:left;</xsl:when>
			<xsl:when test="bannerPosition='right'">background-position:right;</xsl:when>
			<xsl:when test="bannerPosition='center'">background-position:center;</xsl:when>
		</xsl:choose>
		<xsl:if test="$page/@bannerUrl">
			cursor:pointer;
		</xsl:if>
		}
	</xsl:template>

	<xsl:template name="EmitDirectoryCSS">
		.maDir, .maDirTouch, .maDirBody, .maDirBodyTouch, .maDirStatusLine, .maDirStatusLineTouch {
		font-family:Verdana, Arial, Helvetica, Sans-Serif;
		font-size:10px;
		text-align:left;
		}
		.maDir, .maDirTouch {
		border:solid 1px <xsl:value-of select="@dirBorderColor"/>;
		background-color:<xsl:value-of select="@dirTitleBarColor"/>;
		cursor:pointer;
		height:22px;
		padding-left:4px;
		padding-right:2px;
		position:absolute;
		visibility:hidden;
		overflow:hidden;
		z-index:4000;
		}
		.maDirBody, .maDirBodyTouch {
		border:solid 1px <xsl:value-of select="@dirBorderColor"/>;
		display:none;
		position:absolute;
		width:<xsl:value-of select="@dirContentWidth"/>px;
		background-color:<xsl:value-of select="@dirBackgroundColor"/>;
		z-index:4000;
		}
		.maDirBodyTouch {
		font-size:18px;
		}
		.maDirStatusLine, .maDirStatusLineTouch {
		color:<xsl:value-of select="@dirStatusTextColor"/>;
		background-color:<xsl:value-of select="@dirStatusBackgroundColor"/>;
		border-bottom: solid 1px <xsl:value-of select="@dirBorderColor"/>;
		padding-top:1px;
		padding-left:4px;
		padding-right:4px;
		height:14px;
		overflow:hidden;
		}
		.maDirContent, .maDirContentTouch {
		padding:4px;
		max-height:<xsl:value-of select="@dirMaxHeight"/>px;
		overflow-y:scroll;
		overflow-x:hidden;
		}
		.maDirTitle, .maDirTitleTouch {
		color:<xsl:value-of select="@dirTitleTextColor"/>;
		font-weight:bold;
		padding-right:4px;
		white-space:nowrap;
		}
		.maDirTitle {
		font-size:11px;
		}
		.maDirTitleTouch {
		font-size:12px;
		}
		.maDirSearchBox, .maDirSearchBoxTouch {
		font-size:11px;
		width:60px;
		border:solid 1px #dddddd;
		height:14px;
		padding-top:1px;
		}
		.maDirSearchLabel, .maDirSearchLabelTouch {
		color:<xsl:value-of select="@dirTitleTextColor"/>;
		font-size:10px;
		font-weight:normal;
		text-align:right;
		width:100%;
		padding-right:1px;
		}
		.maDirLevel,
		.maDirLevelTouch {
		cursor:pointer;
		}
		.maDirLevel {
		font-size:11px;
		}
		.maDirLevelTouch {
		font-size:18px;
		}
		.maDirLevel1, .maDirLevelTouch1 {
		padding-left:0px;
		margin-top:4px;
		}
		.maDirLevel1 a, 
		.maDirLevelTouch1 a {
		color:<xsl:value-of select="@dirLevel1TextColor"/>;
		font-weight:bold;
		text-decoration:none;
		}
		.maDirLevel a:link,
		.maDirLevelTouch1 a:link,
		.maDirEntry a:visited,
		.maDirLevelTouch1 a:visited {
		text-decoration:none;
		}
		.maDirLevel1 a:hover,
		.maDirLevelTouch1 a:hover {
		text-decoration:underline;
		}
		.maDirLevel2, .maDirLevelTouch2 {
		margin-top:2px;
		}
		.maDirLevel2 {
		padding-left:8px;
		}
		.maDirLevelTouch2 {
		padding-left:16px;
		}
		.maDirLevel2 a,
		.maDirLevelTouch2 a {
		color:<xsl:value-of select="@dirLevel2TextColor"/>;
		text-decoration:none;
		}
		.maDirLevel2 a:link,
		.maDirLevelTouch2 a:link,
		.maDirEntry a:visited,
		.maDirLevelTouch2 a:visited {
		text-decoration:none;
		}
		.maDirLevel2 a:hover,
		.maDirLevelTouch2 a:hover {
		text-decoration:underline;
		}
		.maDirLevelCount, .maDirLevelCountTouch {
		color:<xsl:value-of select="@dirEntryCountColor"/>;
		font-weight:normal;
		}
		.maDirLevelCount {
		font-size:9px;
		}
		.maDirLevelCountTouch {
		font-size:12px;
		}
		.maDirEntry, .maDirEntryTouch {
		cursor:pointer;
		}
		.maDirEntry {
		font-size:10px;
		padding-left:8px;
		}
		.maDirEntryTouch {
		font-size:18px;
		padding-left:18px;
		}
		.maDirEntry a, .maDirEntryTouch a {
		color:<xsl:value-of select="@dirEntryTextColor"/>;
		font-weight:normal;
		}
		.maDirEntry a {
		line-height:14px;
		}
		.maDirEntryTouch a {
		line-height:32px;
		}
		.maDirEntry a:link,
		.maDirEntryTouch a:link,
		.maDirEntry a:visited,
		.maDirEntryTouch a:visited {
		text-decoration:none;
		}
		.maDirEntry a:hover,
		.maDirEntryTouch a:hover {
		color:<xsl:value-of select="@dirEntryTextHoverColor"/>;
		text-decoration:underline;
		}
		.maDirPreview {
		background-color:<xsl:value-of select="@dirPreviewBackgroundColor"/>;
		border:solid 1px <xsl:value-of select="@dirPreviewBorderColor"/>;
		color:<xsl:value-of select="@dirPreviewTextColor"/>;
		font-family:Verdana, Arial, Helvetica, Sans-Serif;
		font-size:9px;
		overflow:hidden;
		position:absolute;
		text-align:left;
		z-index:4001;
		}
		.maDirPreviewImage {
		float:right;
		border:solid 1px <xsl:value-of select="@dirPreviewImageBorderColor"/>;
		}
		.maDirEntrySearchResult, .maDirEntrySearchResultTouch {
		color:<xsl:value-of select="@dirSearchResultTextColor"/>;
		background-color:<xsl:value-of select="@dirSearchResultBackgroundColor"/>;
		}
	</xsl:template>

	<xsl:template name="EmitSlideDropdownCSS">
    <xsl:param name="page"/>
    #maHotspotDropdown {
    float:right;
    margin-right:<xsl:choose>
			<xsl:when test="@hasTitle='True'">0px;</xsl:when>
			<xsl:otherwise><xsl:value-of select="$page/@dropdownMarginRight"/>px;</xsl:otherwise>
    </xsl:choose>
  }
  </xsl:template>
 
  <xsl:template name="EmitFooterStripeCSS">
    <xsl:param name="pageWidth"/>
    #maFooterStripe {
    background-color:<xsl:value-of select="@colorFooterStripeBackground"/>;
    border-top:solid 1px <xsl:value-of select="@colorFooterStripeTopBorder"/>;
    border-bottom:solid 1px <xsl:value-of select="@colorFooterStripeBottomBorder"/>;
    height:<xsl:value-of select="@footerStripeHeight - @footerStripeBorderHeight"/>px;
    width:<xsl:value-of select="$pageWidth"/>px;
    }
  </xsl:template>

  <xsl:template name="EmitMapsAliveLinkCSS">
    <xsl:param name="pageWidth"/>
    #maFooterLink {
    color:<xsl:value-of select="@colorFooterLinkText"/>;
    font-family:<xsl:value-of select="@fontFamilyFooter"/>;
    font-size:<xsl:value-of select="@fontSizeFooter"/>px;
    font-style:<xsl:value-of select="@fontStyleFooter"/>;
    font-weight:<xsl:value-of select="@fontWeightFooter"/>;
    line-height:<xsl:value-of select="@footerStripeHeight - @footerStripeBorderHeight"/>px;
    text-align:center;
    width:<xsl:value-of select="$pageWidth"/>px;
    }
    .maFooterLink:link {
    color:<xsl:value-of select="@colorFooterLinkText"/>;
    text-decoration:underline;
    }

    .maFooterLink:visited {
    color:<xsl:value-of select="@colorFooterLinkText"/>;
    text-decoration:underline;
    }

    .maFooterLink:hover {
    color:<xsl:value-of select="@colorFooterLinkText"/>;
    text-decoration:underline;
    }
  </xsl:template>
  
  <xsl:template name="EmitTextAreaCSS">
    <xsl:param name="page"/>
		<xsl:variable name="id" select="$page/@layoutId" />
		<xsl:if test="$page/@showSlideTitle='True'">
			#maHotspotTitle {
			background-color:<xsl:choose>
				<xsl:when test="$page/@popupSlides='True'">
					<xsl:value-of select="$page/@colorPopupBackground"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="@colorSlideTitleBackground"/>
				</xsl:otherwise>
			</xsl:choose>;
			color:<xsl:choose>
				<xsl:when test="$page/@popupSlides='True'">
					<xsl:value-of select="$page/@colorPopupTitleText"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="@colorSlideTitleText"/>
				</xsl:otherwise>
			</xsl:choose>;
			font-family:<xsl:value-of select="@fontFamilyHeading"/>;
			font-size:<xsl:value-of select="@fontSizeHeading"/>px;
			font-style:<xsl:value-of select="@fontStyleHeading"/>;
			font-weight:<xsl:value-of select="@fontWeightHeading"/>;
			}
		</xsl:if>

		#maTextArea {
		background-color:<xsl:choose>
			<xsl:when test="$page/@popupSlides='True'">
				<xsl:value-of select="$page/@colorPopupBackground"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="@colorSlideBackground"/>
			</xsl:otherwise>
		</xsl:choose>;
		}
		#maHotspotText {
		background-color:<xsl:choose>
			<xsl:when test="$page/@popupSlides='True'">
				<xsl:value-of select="$page/@colorPopupBackground"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="@colorSlideBackground"/>
			</xsl:otherwise>
		</xsl:choose>;
		color:<xsl:choose>
			<xsl:when test="$page/@popupSlides='True'">
				<xsl:value-of select="$page/@colorPopupText"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="@colorSlideText"/>
			</xsl:otherwise>
		</xsl:choose>;
		font-family:<xsl:value-of select="@fontFamilyDescription"/>;
		font-size:<xsl:value-of select="@fontSizeDescription"/>px;
		font-style:<xsl:value-of select="@fontStyleDescription"/>;
		font-weight:<xsl:value-of select="@fontWeightDescription"/>;
		}
	</xsl:template>

	<xsl:template name="EmitImageCSS">
    <xsl:param name="page"/>
  </xsl:template>

	<xsl:template name="EmitLayoutCSS">
		<xsl:param name="page"/>
		<xsl:param name="canvasWidth"/>
		<xsl:param name="canvasHeight"/>
		#maLayout {
			text-align:left;
		<xsl:choose>
			<xsl:when test="$page/@popupSlides='True'">
				height:<xsl:value-of select="$canvasHeight - $page/@popupMapMarginTop - $page/@popupMapMarginBottom"/>px;
				padding-top:<xsl:value-of select="$page/@popupMapMarginTop"/>px;
				padding-right:<xsl:value-of select="$page/@popupMapMarginRight"/>px;
				padding-bottom:<xsl:value-of select="$page/@popupMapMarginBottom"/>px;
				padding-left:<xsl:value-of select="$page/@popupMapMarginLeft"/>px;
				width:<xsl:value-of select="$canvasWidth - $page/@popupMapMarginLeft - $page/@popupMapMarginRight"/>px;
			</xsl:when>
			<xsl:otherwise>
				height:<xsl:value-of select="$canvasHeight - $page/@layoutMarginTop - $page/@layoutMarginBottom"/>px;
				padding-top:<xsl:value-of select="$page/@layoutMarginTop"/>px;
				padding-right:<xsl:value-of select="$page/@layoutMarginRight"/>px;
				padding-bottom:<xsl:value-of select="$page/@layoutMarginBottom"/>px;
				padding-left:<xsl:value-of select="$page/@layoutMarginLeft"/>px;
				position:relative;
				width:<xsl:value-of select="$canvasWidth - $page/@layoutMarginLeft - $page/@layoutMarginRight"/>px;
			</xsl:otherwise>
		</xsl:choose>
		}
		<xsl:if test="@navigationId='2'">
			#maLayoutCell {
			vertical-align:top;
			}
		</xsl:if>
		#maAbsBlock {
		overflow:hidden;
		position:absolute;
		}
	</xsl:template>

	<xsl:template name="EmitMapCSS">
    <xsl:param name="page"/>
		#maMap {
		background-color:<xsl:value-of select="@colorTourBackground"/>;
		}
		.maInstructions, maInstructionsTitle	{
		font-family:Arial;
		font-size:12px;
		}
		.maInstructions	{
		padding:6px;
		border:solid 1px #777;
		}
		.maInstructionsTitle {
		font-weight:bold;
		margin-bottom:4px;
		}
		.maZoomControl	{
		z-index:4000;
		position:absolute;
		top:-4px;
		left:-4px;
		cursor:pointer;
		-webkit-tap-highlight-color:rgba(0,0,0,0);
		}
	</xsl:template>

  <xsl:template name="EmitTitleCSS">
    <xsl:param name="pageWidth"/>
    #maPageTitle {
    background-color:<xsl:value-of select="@colorTitleBackground"/>;
    height:<xsl:value-of select="@pageTitleHeight - @titleOffsetTop - @titleOffsetBottom" />px;
    padding:<xsl:value-of select="@titleOffsetTop"/>px 0px <xsl:value-of select="@titleOffsetBottom"/>px <xsl:value-of select="@titleOffsetLeft"/>px;
    text-align:left;
    width:<xsl:value-of select="$pageWidth - @titleOffsetLeft"/>px;
		}
		#maPageTitleText {
    color:<xsl:value-of select="@colorTitleText" />;
    font-family:<xsl:value-of select="@fontFamilyTitle"/>;
    font-size:<xsl:value-of select="@fontSizeTitle"/>px;
    font-style:<xsl:value-of select="@fontStyleTitle"/>;
    font-weight:<xsl:value-of select="@fontWeightTitle"/>;
    <!-- The height needs to be specified for both the title and text to make sure that a
		very long title won't cause the text height to grow when the text wraps. By setting the
		height and setting overflow to hidden, a long title will simply get clipped. -->
		height:<xsl:value-of select="@pageTitleHeight - @titleOffsetTop - @titleOffsetBottom" />px;
    line-height:<xsl:value-of select="@pageTitleHeight - @titleOffsetTop - @titleOffsetBottom" />px;
		overflow:hidden;
		}
	</xsl:template>

  <xsl:template name="EmitHeaderStripeCSS">
    <xsl:param name="pageWidth"/>
    #maHeaderStripe {
    background-color:<xsl:value-of select="@colorHeaderStripeBackground"/>;
	border-bottom:solid 1px <xsl:value-of select="@colorHeaderStripeBottomBorder"/>;
	border-top:solid 1px <xsl:value-of select="@colorHeaderStripeTopBorder"/>;
    font-size:5px; <!-- needed to force the height attribute to work in IE -->
    height:<xsl:value-of select="@headerStripeHeight - @headerStripeBorderHeight"/>px;
    width:<xsl:value-of select="$pageWidth"/>px;
    }
  </xsl:template>

  <!-- Templates to emit CSS for Left and Top Menu -->
  <!-- Style1 = Bar
	   Style2 = Simple
	   Style3 = Tab
  -->

  <xsl:template name="EmitLeftMenuCSS">
   <xsl:param name="page"/>
    #maLeftMenuCell {
    vertical-align:top;
    width:<xsl:value-of select="@leftNavWidth"/>px;
    }
    #maLeftMenu {
    background-color:<xsl:value-of select="@colorMenuBackground"/>;
    font-family:<xsl:value-of select="@fontFamilyMenuItem" />;
    font-size:<xsl:value-of select="@fontSizeMenuItem" />px;
    font-style:<xsl:value-of select ="@fontStyleMenuItem" />;
    font-weight:<xsl:value-of select="@fontWeightMenuItem" />;
    height:<xsl:value-of select="$page/@canvasHeight"/>px;
    overflow:<xsl:value-of select="@menuScrolls" />;
    text-align:left;
    white-space:nowrap;
    }
    <xsl:choose>
      <xsl:when test="@menuStyleId='1'">
        <xsl:call-template name="EmitLeftMenuCSS_Style1">
          <xsl:with-param name="page" select="$page"/>
        </xsl:call-template>
        </xsl:when>
      <xsl:when test="@menuStyleId='2'">
        <xsl:call-template name="EmitLeftMenuCSS_Style2">
          <xsl:with-param name="page" select="$page"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="@menuStyleId='3'">
        <xsl:call-template name="EmitLeftMenuCSS_Style3"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="EmitLeftMenuCSS_Style1"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="EmitLeftMenuCSS_Style1">
    <xsl:param name="page"/>
    #maLeftMenu {
    margin-bottom:0px;
    margin-top:0px;
    padding-left:8px;
    width:<xsl:value-of select="@leftNavWidth - 8"/>px;
    }
    #maLeftMenu a {
    line-height:<xsl:value-of select="@navHeight" />px;
    padding-bottom:0px;
    }
    #maLeftMenu a:link, #maLeftMenu a:visited {
    color:<xsl:value-of select="@colorMenuItemNormalText" />;
    margin:0px 10px 4px 0px;
    text-decoration:none;
    }
    #maLeftMenu a.current:link, #maLeftMenu a.current:visited {
    border-bottom:2px solid;
    border-bottom-color:<xsl:value-of select="@colorMenuItemLine" />;
    color:<xsl:value-of select="@colorMenuItemSelectedText" />;
    background:transparent;
    }
    #maLeftMenu a:hover {
    background:transparent;
    border-bottom:0px;
    border-bottom:2px solid;
    border-bottom-color:<xsl:value-of select="@colorMenuItemLine" />;
    color:<xsl:value-of select="@colorMenuItemHoverText" />;
    }
    #maHotspotNamesInMenu {
    cursor:pointer;
    font-family:<xsl:value-of select="@fontFamilyMenuSlideItem" />;
    font-size:<xsl:value-of select="@fontSizeMenuSlideItem" />px;
    font-style:<xsl:value-of select ="@fontStyleMenuSlideItem" />;
    font-weight:<xsl:value-of select="@fontWeightMenuSlideItem" />;
	margin:0px;
	padding:0px 0px 0px 8px;
	width:<xsl:value-of select="@leftNavWidth - 16"/>px;
	overflow:hidden;
	}
	#maHotspotNamesInMenu a:link, #maHotspotNamesInMenu a:visited {
	color:<xsl:value-of select="@colorMenuItemNormalText" />;
    height:<xsl:value-of select="@navHeight - 4" />px;
    line-height:<xsl:value-of select="@navHeight - 3" />px;
    }
    #maHotspotNamesInMenu a.currentSlide:link, #maHotspotNamesInMenu a.currentSlide:visited {
    border-bottom:2px solid <xsl:value-of select="@colorMenuItemLine" />;
    color:<xsl:value-of select="@colorMenuItemSelectedText" />;
    height:<xsl:value-of select="@navHeight - 4" />px;
    line-height:<xsl:value-of select="@navHeight - 3" />px;
    }
    #maHotspotNamesInMenu a:hover {  
    border-bottom:0px;
    color:<xsl:value-of select="@colorMenuItemHoverText" />;
    height:<xsl:value-of select="@navHeight - 4" />px;
    line-height:<xsl:value-of select="@navHeight - 3" />px;
    text-decoration:none;
    }
  </xsl:template>

  <xsl:template name="EmitLeftMenuCSS_Style2">
    <xsl:param name="page"/>
    #maLeftMenu {
    width:<xsl:value-of select="@leftNavWidth"/>px;
    }
    .maLeftMenuPageName {
    border-bottom:1px solid <xsl:value-of select ="@colorMenuItemLine"/>;
    padding:0.25em 0.25em 0.25em .75em;.
    width:<xsl:value-of select="@leftNavWidth"/>px;
    }
    #maLeftMenu a {
    height:<xsl:value-of select="@navHeight - 8" />px;
    text-align:left;
    }
    #maLeftMenu a:link, #maLeftMenu a:visited {
    color:<xsl:value-of select="@colorMenuItemNormalText" />;
    text-decoration:none;
    }
    #maLeftMenu a.current:link, #maLeftMenu a.current:visited {
    color:<xsl:value-of select="@colorMenuItemSelectedText" />;
    text-decoration:none;
    }
    #maLeftMenu a:hover {
    color:<xsl:value-of select="@colorMenuItemHoverText" />;
    text-decoration:none;
    }
    #maHotspotNamesInMenu {
    cursor:pointer;
    font-family:<xsl:value-of select="@fontFamilyMenuSlideItem" />;
    font-size:<xsl:value-of select="@fontSizeMenuSlideItem" />px;
    font-style:<xsl:value-of select ="@fontStyleMenuSlideItem" />;
    font-weight:<xsl:value-of select="@fontWeightMenuSlideItem" />;
    margin:0px;
    padding:0px 0px 0px 8px;
    }
    #maHotspotNamesInMenu a:link, #maHotspotNamesInMenu a:visited {
    color:<xsl:value-of select="@colorMenuItemNormalText" />;
    height:<xsl:value-of select="@navHeight - 4" />px;
    line-height:<xsl:value-of select="@navHeight - 4" />px;
    }
    #maHotspotNamesInMenu a.currentSlide:link, #maHotspotNamesInMenu a.currentSlide:visited {
    color:<xsl:value-of select="@colorMenuItemSelectedText" />;
    height:<xsl:value-of select="@navHeight - 4" />px;
    line-height:<xsl:value-of select="@navHeight - 4" />px;
    }
    #maHotspotNamesInMenu a:hover {
    border-bottom:0px;
    color:<xsl:value-of select="@colorMenuItemHoverText" />;
    height:<xsl:value-of select="@navHeight - 4" />px;
    line-height:<xsl:value-of select="@navHeight - 4" />px;
    text-decoration:none;
    }
  </xsl:template>

  <xsl:template name="EmitLeftMenuCSS_Style3">
    #maLeftMenuCell {
    background-color:<xsl:value-of select="@colorMenuItemNormalBackground"/>;
    padding:0px;
    }
    #maLeftMenu {
    margin:0px;
    padding:0px;
    vertical-align:top;
    width:<xsl:value-of select="@leftNavWidth"/>px;
    }
    .maLeftMenuPageName {
    background-color:<xsl:value-of select="@colorMenuItemNormalBackground"/>;
    border-bottom:1px solid <xsl:value-of select ="@colorMenuItemLine"/>;
    }
    .maLeftNavPageNameCurrent {
    background-color:<xsl:value-of select="@colorMenuItemSelectedBackground"/>;
    border-bottom:1px solid <xsl:value-of select ="@colorMenuItemLine"/>;
    color:<xsl:value-of select="@colorMenuItemSelectedText"/>;
    }
    .maLeftNavPageNameCurrent a:link, .maLeftNavPageNameCurrent a:visited {
    color:<xsl:value-of select="@colorMenuItemSelectedText"/>;
	}
    #maLeftMenu a {
    height:<xsl:value-of select="@navHeight" />px;
    line-height:<xsl:value-of select="@navHeight" />px;
    padding:0px 3px 0px 8px;
    text-decoration:none;
    width:<xsl:value-of select="@leftNavWidth"/>px;
    }
    #maLeftMenu a.current:link, #maLeftMenu a.current:visited {
    color:<xsl:value-of select="@colorMenuItemSelectedText"/>;
    }
    #maLeftMenu a:link, #maLeftMenu a:visited {
    color:<xsl:value-of select="@colorMenuItemNormalText"/>;
    }
    #maLeftMenu a:hover {
    color:<xsl:value-of select="@colorMenuItemHoverText"/>;
    text-decoration:none;
    }
    #maHotspotNamesInMenu {
    background-color:<xsl:value-of select="@colorMenuItemSelectedBackground"/>;
    cursor:pointer;
    font-family:<xsl:value-of select="@fontFamilyMenuSlideItem" />;
    font-size:<xsl:value-of select="@fontSizeMenuSlideItem" />px;
    font-style:<xsl:value-of select ="@fontStyleMenuSlideItem" />;
    font-weight:<xsl:value-of select="@fontWeightMenuSlideItem" />;
    margin:0px;
    padding:0px 0px 0px 8px;
    }
    #maHotspotNamesInMenu a:link, #maHotspotNamesInMenu a:visited {
    background-color:<xsl:value-of select="@colorMenuItemSelectedBackground"/>;
    color:<xsl:value-of select="@colorMenuItemNormalText" />;
    height:<xsl:value-of select="@navHeight - 4" />px;
    line-height:<xsl:value-of select="@navHeight - 4" />px;
    }
    #maHotspotNamesInMenu a.currentSlide:link, #maHotspotNamesInMenu a.currentSlide:visited {
    background-color:<xsl:value-of select="@colorMenuItemSelectedBackground"/>;
    color:<xsl:value-of select="@colorMenuItemSelectedText" />;
    height:<xsl:value-of select="@navHeight - 4" />px;
    line-height:<xsl:value-of select="@navHeight - 4" />px;
    }
    #maHotspotNamesInMenu a:hover {
    border-bottom:0px;
    color:<xsl:value-of select="@colorMenuItemHoverText" />;
	height:<xsl:value-of select="@navHeight - 4" />px;
    line-height:<xsl:value-of select="@navHeight - 4" />px;
    text-decoration:none;
    }
  </xsl:template>  

  <xsl:template name="EmitTopMenuCSS">
    <xsl:param name="page"/>
    <xsl:param name="pageWidth"/>
   <!-- CSS styles specific to each layout style -->
    <xsl:choose>
      <xsl:when test="@menuStyleId='1'">
        <xsl:call-template name="EmitTopNavCSS_Style1">
          <xsl:with-param name="pageWidth" select="$pageWidth"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="@menuStyleId='2'">
        <xsl:call-template name="EmitTopNavCSS_Style2">
          <xsl:with-param name="page" select="$page"/>
          <xsl:with-param name="pageWidth" select="$pageWidth"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="@menuStyleId='3'">
        <xsl:call-template name="EmitTopNavCSS_Style3">
          <xsl:with-param name="page" select="$page"/>
          <xsl:with-param name="pageWidth" select="$pageWidth"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="EmitTopNavCSS_Style1"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <xsl:template name="EmitTopNavCSS_Style1">
    <xsl:param name="pageWidth"/>
    #maTopMenu {
	background-color:<xsl:value-of select="@colorTourBackground"/>;
	height:<xsl:value-of select="@navHeight"/>px;
    <!--margin-left:10px;-->
    padding:4px 0px 4px 10px;
    overflow:hidden;
    white-space:nowrap;
    width:<xsl:value-of select="$pageWidth - 10"/>px;
    }
    #maTopMenuList {
    font-family:<xsl:value-of select="@fontFamilyMenuItem" />;
    font-size:<xsl:value-of select="@fontSizeMenuItem" />px;
    font-style:<xsl:value-of select ="@fontStyleMenuItem" />;
    font-weight:<xsl:value-of select="@fontWeightMenuItem" />;
    margin:0px;
    overflow:hidden;
    white-space:nowrap;
    padding:0px 0px 20px 0px;
    }
    #maTopMenuList li {
    display:inline;
    list-style-type:none;
    margin:0px;
    overflow:hidden;
    white-space:nowrap;
    padding:0px;
    }
    #maTopMenuList a:link, #maTopMenuList a:visited {
    color:<xsl:value-of select="@colorMenuItemNormalText"/>;
    float:left;
    line-height:16px;
    margin:0px 10px 4px 0px;
    text-decoration:none;
    }
    #maTopMenuList a.current:link, #maTopMenuList a.current:visited {
    border-bottom:2px solid;
    border-color:<xsl:value-of select="@colorMenuItemLine"/>;
    color:<xsl:value-of select="@colorMenuItemSelectedText"/>;
	padding-bottom:2px;
	}
	#maTopMenuList a:hover {
    border-bottom:2px solid;
    border-color:<xsl:value-of select="@colorMenuItemLine"/>;
    color:<xsl:value-of select="@colorMenuItemHoverText"/>;
	padding-bottom:2px;
	}
  </xsl:template>

  <xsl:template name="EmitTopNavCSS_Style2">
    <xsl:param name="page"/>
    <xsl:param name="pageWidth"/>
    #maTopMenu {
    background-color:<xsl:value-of select="@colorTourBackground"/>;
    height:<xsl:value-of select="@navHeight"/>px;
    overflow:hidden;
    padding:4px 0px 4px 0px;
    text-align:left;
    white-space:nowrap;
    width:<xsl:value-of select="$pageWidth"/>px;
    }
    #maTopMenuList {
    font-family:<xsl:value-of select="@fontFamilyMenuItem" />;
    font-size:<xsl:value-of select="@fontSizeMenuItem" />px;
    font-style:<xsl:value-of select ="@fontStyleMenuItem" />;
    font-weight:<xsl:value-of select="@fontWeightMenuItem" />;
    margin:0px;
    padding:0px;
    }
    #maTopMenuList li {
    display:inline;
    list-style-type:none;
    margin:0px;
    padding-bottom:2px;
    }
    #maTopMenuList li a {
    border-right:2px solid;
    padding:0.25em 0.75em 0.25em 0.5em;
    text-decoration:none;
    }
    .maTopMenu-first {
    border-left:2px solid <xsl:value-of select="@colorMenuItemLine "/>;
    }
    #maTopMenuList a:link, #maTopMenuList a:visited {
    color:<xsl:value-of select="@colorMenuItemNormalText"/>;
    border-color:<xsl:value-of select="@colorMenuItemLine"/>;
    border-right-color:<xsl:value-of select="@colorMenuItemLine"/>;
    }
    #maTopMenuList a.current:link, #maTopMenuList a.current:visited {
    border-right-color:<xsl:value-of select="@colorMenuItemLine"/>;
    color:<xsl:value-of select="@colorMenuItemSelectedText"/>;
	}
	#maTopMenuList a:hover {
    color:<xsl:value-of select="@colorMenuItemHoverText"/>;
	}
  </xsl:template>

  <xsl:template name="EmitTopNavCSS_Style3">
    <xsl:param name="page"/>
    <xsl:param name="pageWidth"/>
    #maMenuBackground{
    height:<xsl:value-of select="@navHeight + 8"/>px;
    width:<xsl:value-of select="$pageWidth"/>px;
    }
    #maTopMenu {
    background-color:<xsl:value-of select="@colorMenuBackground"/>;
    height:<xsl:value-of select="@navHeight + 8"/>px;
    margin:0px;
    margin-bottom:0px;
    overflow:hidden;
    text-align:left;
    width:<xsl:value-of select="$pageWidth"/>px;
    }
    #maTopMenuList {
    margin:0px;
    padding:0px;
    }
    #maTopMenuList li {
    display:inline;
    font-family:<xsl:value-of select="@fontFamilyMenuItem" />;
    font-size:<xsl:value-of select="@fontSizeMenuItem" />px;
    font-style:<xsl:value-of select ="@fontStyleMenuItem" />;
    font-weight:<xsl:value-of select="@fontWeightMenuItem" />;
    list-style-type:none;
    margin:0px;
    padding:0px;
    }
    .maTopMenu-first {
	  border-left:1px solid <xsl:value-of select="@colorMenuItemLine"/>;
    }
    #maTopMenuList a:link, #maTopMenuList a:visited {
    background-color:<xsl:value-of select="@colorMenuItemNormalBackground"/>;
  	border-right:1px solid <xsl:value-of select="@colorMenuItemLine"/>;
    color:<xsl:value-of select="@colorMenuItemNormalText"/>;
    float:left;
    line-height:<xsl:value-of select="@navHeight + 8"/>px;
    padding:0em 0.75em 0em 0.75em;
    text-decoration:none;
    }
    #maTopMenuList a.current:link, #maTopMenuList a.current:visited {
    background-color:<xsl:value-of select="@colorMenuItemSelectedBackground"/>;
    color:<xsl:value-of select="@colorMenuItemSelectedText"/>;
    }
    #maTopMenuList a:hover {
    background-color:<xsl:value-of select="@colorMenuItemHoverBackground"/>;
    color:<xsl:value-of select="@colorMenuItemHoverText"/>;
    }
 </xsl:template>

</xsl:stylesheet>


