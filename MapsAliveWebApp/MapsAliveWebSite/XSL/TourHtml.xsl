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
	<xsl:param name="customHtmlTop"/>
	<xsl:param name="customHtmlAbsolute"/>
	<xsl:param name="customHtmlBottom"/>

	<xsl:template match="/tour">
		<xsl:variable name="page" select="tourPages/tourPage[@pageId=$pageId]"/>
		<xsl:choose>
			<xsl:when test="$contentId='css-tour'">
				<xsl:call-template name="EmitCssForTour">
					<xsl:with-param name="page" select="$page"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$contentId='css-page'">
				<xsl:call-template name="EmitCssForPage">
					<xsl:with-param name="page" select="$page"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$contentId='js'">
				<xsl:call-template name="EmitJavascriptForPage">
					<xsl:with-param name="page" select="$page"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$contentId='html-tour'">
				<xsl:call-template name="EmitHtmlForTour">
					<xsl:with-param name="page" select="$page"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$contentId='html'">
				<xsl:call-template name="EmitHtml">
					<xsl:with-param name="page" select="$page"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:when test="$contentId='tour'">
				<xsl:call-template name="EmitJavascriptForTour">
				</xsl:call-template>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="EmitHtml">
		<xsl:param name="page"/>
		<xsl:text disable-output-escaping='yes'>&lt;!DOCTYPE html></xsl:text>
		<html lang="en">
			<head>
				<meta http-equiv="Cache-Control" content="no-cache"/>
				<meta name="description" content="Interactive map {@tourName} created with MapsAlive"/>
				<xsl:if test="@webAppCapable='True'">
				<meta name="apple-mobile-web-app-capable" content="yes" />
				<meta name="apple-mobile-web-app-status-bar-style" content="black" />
				</xsl:if>
				<meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <link rel="icon" href="data:;" /> <!-- Indicate no favicon so that browsers don't try to load it. -->
				<title><xsl:value-of select="@browserTitle" /></title>
				<xsl:if test="@accountId='300003'">
				<script async="" src='https://www.googletagmanager.com/gtag/js?id=G-9MYHNCT1Z5'></script>
				<script>window.dataLayer=window.dataLayer || [];function gtag() { dataLayer.push(arguments); }gtag('js', new Date());gtag('config', 'G-9MYHNCT1Z5');</script>
				</xsl:if>
				<style>body{background-color:<xsl:value-of select="@bodyBackgroundColor"/>;margin:<xsl:value-of select="@bodyMargin" />px;}}</style>
			</head>
			<body>
				<main style="max-width:{@tourWidth}px">
					<script type="module" id="ma-{@tourId}" src="{@fileTourLoaderJs}?v={@buildId}"></script>
					<div class="ma-{@tourId}"></div>
				</main>
			</body>
		</html>
	</xsl:template>

	<xsl:template name="EmitHtmlForTour">
		<xsl:param name="page"/>
export let html =
`<xsl:if test="@hasCustomHtmlTop">
<div id="maCustomHtmlTop" class="maCustomHtmlTop"><xsl:value-of select="$customHtmlTop" disable-output-escaping="yes"/></div>
			</xsl:if>
		
			<xsl:if test="$page/@testRouteId">
				<select style="position:absolute;z-index:10000" onchange="mapsalive.drawTestRoute('{$page/@testRouteId}',this.value);">
				<option value="0">- Hide Route -</option>
				<xsl:for-each select="routes/route">
					<option value="{@id}"><xsl:value-of select="@id"/></option>
				</xsl:for-each>
				</select>
			</xsl:if>

			<xsl:if test="@hasCustomHtmlAbsolute">
  		<div id="maCustomHtmlAbsolute" class="maCustomHtmlAbsolute"><xsl:value-of select="$customHtmlAbsolute" disable-output-escaping="yes"/></div>
			</xsl:if>

			<xsl:if test="@hasBanner='True'">
				<xsl:call-template name="EmitBanner">
					<xsl:with-param name="page" select="$page"/>
				</xsl:call-template>
			</xsl:if>

			<xsl:if test="@hasTitle='True'">
				<xsl:call-template name="EmitTitleBar" />
			</xsl:if>

			<xsl:if test="@hasHeaderStripe='True'">
				<xsl:call-template name="EmitHeaderStripe" />
			</xsl:if>

			<xsl:call-template name="EmitLayout">
			</xsl:call-template>

			<xsl:if test="@hasFooterStripe='True'">
				<xsl:call-template name="EmitFooterStripe" />
			</xsl:if>

		<xsl:if test="@hasCustomHtmlBottom">
		<div id="maCustomHtmlBottom" class="maCustomHtmlBottom"><xsl:value-of select="$customHtmlBottom" disable-output-escaping="yes"/></div>
		</xsl:if>
		<xsl:call-template name="EmitFooter"/>
`;
	</xsl:template>

	<xsl:template name="EmitBanner">
    <xsl:param name="page"/>
		<div id="maBanner">
		<xsl:choose>
			<xsl:when test="@bannerUrl">
				<a href="{@bannerUrl}">
					<xsl:if test="@bannerUrlOpensWindow='True'">
						<xsl:attribute name="target">_blank</xsl:attribute>
					</xsl:if>
					<xsl:if test="@bannerUrlTitle">
						<xsl:attribute name="title"><xsl:value-of select="@bannerUrlTitle" /></xsl:attribute>
					</xsl:if>
					<img></img>
				</a>
			</xsl:when>
			<xsl:otherwise>
				<img></img>
			</xsl:otherwise>
		</xsl:choose>
		</div>
	</xsl:template>

	<xsl:template name="EmitTitleBar">
    <div id="maTitleBar" class="maTitleBar">
			<div id="maTitleText" class="maTitleText"></div>
    </div>
  </xsl:template>

  <xsl:template name="EmitFooterStripe">
    <div id="maFooterStripe" class="maFooterStripe"></div>
  </xsl:template>

  <xsl:template name="EmitFooter">
		<!-- In this template we disable output escaping so that the user can use entities in their text. -->
		<xsl:if test="@showMapsAliveLink='1' or @showCustomFooter='1'">
			<div id="maFooter" class="maFooter maHidden">
				<div>
					<xsl:if test="@showCustomFooter='1'">
						<xsl:value-of select="@tourFooterText1" disable-output-escaping="yes"/>
						<xsl:if test="@tourFooterLinkText!=''">
							<a class="maFooter" target="_blank" href="{@tourFooterLinkUrl}">
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
								<a class="maFooter" target="_blank" href="https://www.mapsalive.com?ref={@accountId}">www.mapsalive.com</a>
						</div>
					</xsl:if>
				</div>
			</div>
		</xsl:if>
  </xsl:template>

	<xsl:template name="EmitJavascriptForTour">
export let tourProperties = {
	tourId:<xsl:value-of select="@tourId" />,
	plan:"<xsl:value-of select="@plan" />",
	name:"<xsl:value-of select="@tourName" />",
	tourFolderUrl:"<xsl:value-of select="@tourFolderUrl" />",
	version:"<xsl:value-of select="@appVersion" />",
	buildId:<xsl:value-of select="@buildId" />,
	firstPageNumber:<xsl:value-of select="@firstPageNumber" />,
	enableV3Compatibility:<xsl:choose><xsl:when test="@enableV3Compatibility='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	disableKeyboardShortcuts:<xsl:choose><xsl:when test="@disableKeyboardShortcuts='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	selectsOnTouchStart:<xsl:choose><xsl:when test="@disableKeyboardShortcuts='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	enlargeHitTestArea:<xsl:choose><xsl:when test="@enlargeHitTestArea='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	disableBlendEffect:<xsl:choose><xsl:when test="@disableBlendEffect='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	disableSmoothPanning:<xsl:choose><xsl:when test="@disableSmoothPanning='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	entirePopupVisible:<xsl:choose><xsl:when test="@entirePopupVisible='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	preloadImagesOnMobile:<xsl:choose><xsl:when test="@enableImagePreloading='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	isFlexMapTour:<xsl:choose><xsl:when test="@isFlexMapTour='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	hasDirectory:<xsl:choose><xsl:when test="@hasDirectory='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	hasBanner:<xsl:choose><xsl:when test="@hasBanner='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	hasDataSheet:<xsl:choose><xsl:when test="@hasDataSheet='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	hasTitleBar:<xsl:choose><xsl:when test="@hasTitle='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	bannerHeight:<xsl:value-of select="@bannerHeight - @bannerPaddingTop"/>,
	bannerImageSrc:"<xsl:value-of select="@bannerImg"/>",
	canAppearUnbranded:<xsl:choose><xsl:when test="@canAppearUnbranded='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	accountId:<xsl:value-of select="@accountId"/>,
	bodyMargin:<xsl:value-of select="@bodyMargin" />,
	centeredInBrowser:<xsl:choose><xsl:when test="@centeredInBrowser='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	width:<xsl:value-of select="@tourWidth"/>,
	height:<xsl:value-of select="@tourHeight"/>,
	layoutAreaW:<xsl:value-of select="@layoutAreaWidth"/>,
	layoutAreaH:<xsl:value-of select="@layoutAreaHeight"/>,
	mapAreaBackgroundColor:"<xsl:value-of select="@colorMenuItemNormalBackground"/>",
	hotspotTitleColor:"<xsl:value-of select="@colorSlideTitleText"/>",
	hotspotTitleBackgroundColor:"<xsl:value-of select="@colorSlideTitleBackground"/>",
	hotspotTextColor:"<xsl:value-of select="@colorSlideText"/>",
	hotspotTextBackgroundColor:"<xsl:value-of select="@colorSlideBackground"/>",
	hideMenu:<xsl:choose><xsl:when test="@hideMenu='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	navButtonHidden:<xsl:choose><xsl:when test="@navigationId='1'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	navButtonLocation:<xsl:value-of select="@dirLocation"/>,
	navButtonLocationX:<xsl:value-of select="@dirLocationX"/>,
	navButtonLocationY:<xsl:value-of select="@dirLocationY"/>
};

export let directoryProperties = {
	table:[<xsl:value-of select="@dirTable" />],
	contentWidth:<xsl:value-of select="@dirContentWidth"/>,
	autoCollapse:<xsl:choose><xsl:when test="@dirAutoCollapse='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	backgroundColor:"<xsl:value-of select="@dirBackgroundColor"/>",
	maxHeight:<xsl:value-of select="@dirMaxHeight"/>,
	previewOnRight:<xsl:choose><xsl:when test="@dirPreviewOnRight='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	previewImageWidth:<xsl:value-of select="@dirPreviewImageWidth"/>,
	previewWidth:<xsl:value-of select="@dirPreviewWidth"/>,
	showImagePreview:<xsl:choose><xsl:when test="@dirShowImagePreview='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	showAllPages:<xsl:choose><xsl:when test="@dirShowAllPages='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	showGroupSort:<xsl:choose><xsl:when test="@dirShowGroupSort='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	showSearch:<xsl:choose><xsl:when test="@dirShowSearch='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	showTextPreview:<xsl:choose><xsl:when test="@dirShowTextPreview='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	staysOpen:<xsl:choose><xsl:when test="@dirStaysOpen='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	openExpanded:<xsl:choose><xsl:when test="@dirOpenExpanded='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	textAlphaSort:"<xsl:value-of select="@dirTextAlphaSortTooltip"/>",
	textGroupSort:"<xsl:value-of select="@dirTextGroupSortTooltip"/>",
	textSearchLabel:"<xsl:value-of select="@dirTextSearchLabel"/>",
	textMenuTitle:"<xsl:value-of select="@dirTextClearButtonLabel"/>",
	textDirectoryTitle:"<xsl:value-of select="@dirTextTitle"/>"
};
		<xsl:value-of select="@uaDetectionJs" disable-output-escaping="yes"/>
	</xsl:template>

	<xsl:template name="EmitJavascriptForPage">
		<xsl:param name="page"/>
export let page<xsl:value-of select="$page/@pageNumber" />Properties = {
	pageNumber:<xsl:value-of select="$page/@pageNumber" />,
	pageName:"<xsl:value-of select="$page/@pageName" />",
	mapId:"<xsl:value-of select="$page/@mapId" />",
	pageTitle:"<xsl:value-of select="$page/@pageTitle" />",
	isGallery:<xsl:choose><xsl:when test="$page/@isGallery='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	isDataSheet:<xsl:choose><xsl:when  test="$page/@infoPage='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	firstViewId:<xsl:value-of select="$page/@firstTourViewId"/>,
	layoutId:"<xsl:value-of select="$page/@layoutId"/>",
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
	hasPopup:<xsl:choose><xsl:when test="$page/@popupSlides='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	usesFixedPopup:<xsl:choose><xsl:when test="$page/@popupSlidesFixed='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	usesDynamicPopup:<xsl:choose><xsl:when test="$page/@popupSlidesDynamic='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	showViewTitle:<xsl:choose><xsl:when test="$page/@showSlideTitle='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	excludeFromNavigation:<xsl:choose><xsl:when test="$page/@excludeFromNavigation='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
	hotspotDropdownInstructions:"<xsl:value-of select="$page/@slideListInstructions" />",
	showHelp:<xsl:choose><xsl:when test="$page/@showInstructions='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise></xsl:choose>,
		<xsl:if test="$page/@showInstructions='True'">
	help:{
		title:"<xsl:value-of select="$page/@instructionsTitle"/>",
		text:"<xsl:value-of select="$page/@instructionsText"/>",
		width:<xsl:value-of select="$page/@instructionsWidth"/>,
		bgColor:"<xsl:value-of select="$page/@instructionsBgColor"/>",
		color:"<xsl:value-of select="$page/@instructionsColor"/>"},
		</xsl:if>
	tooltip:{
		border:"<xsl:value-of select="$page/@tooltipBorder"/>",
		color:"<xsl:value-of select="$page/@tooltipTextColor"/>",
		bgColor:"<xsl:value-of select="$page/@tooltipBackgroundColor"/>",
		fontSize:"<xsl:value-of select="$page/@tooltipFontSize"/>px",
		fontFamily:"<xsl:value-of select="$page/@tooltipFontFamily"/>",
		padding:"<xsl:value-of select="$page/@tooltipPadding"/>px",
		fontWeight:"<xsl:value-of select="$page/@tooltipFontWeight"/>",
		fontStyle:"<xsl:value-of select="$page/@tooltipFontStyle"/>",
		textDecoration:"<xsl:value-of select="$page/@tooltipUnderline"/>",
		maxWidth:"<xsl:value-of select="@tooltipMaxWidth"/>px"},
	runSlideShow:<xsl:choose>
		<xsl:when test="$page/@runSlideShow='True'">true</xsl:when>
		<xsl:otherwise>false</xsl:otherwise>
	</xsl:choose>,
	slideShowInterval:<xsl:value-of select="$page/@slideShowInterval" />,
	categoryTable:[<xsl:value-of select="categoryTable" />],
		<xsl:if test="routes">
	routesTable:{
			<xsl:for-each select="routes/route">
				<xsl:if test="position()!=1">,</xsl:if>
				<xsl:value-of select="@id"/>:{route:'<xsl:value-of select="."/>'}
			</xsl:for-each>
	},
		</xsl:if>
	stringTable:[<xsl:value-of select="stringTable" disable-output-escaping="yes" />],
	viewTable:[<xsl:value-of select="slideTable" />],
	markerStyleTable:[<xsl:value-of select="markerStyleTable" />],
	markerInstanceTable:[<xsl:value-of select="markerInstanceTable" />]
};
		
export let page<xsl:value-of select="$page/@pageNumber" />MapProperties = {
	mapAreaW:<xsl:value-of select="$page/@mapAreaWidth" />,
	mapAreaH:<xsl:value-of select="$page/@mapAreaHeight" />,
	mapWidth:<xsl:value-of select="$page/@mapWidth" />,
	mapHeight:<xsl:value-of select="$page/@mapHeight" />,
	<xsl:if test="$page/@popupSlides='True'">
	mapMarginTop:<xsl:value-of select="$page/@popupMapMarginTop"/>,
	mapMarginRight:<xsl:value-of select="$page/@popupMapMarginRight"/>,
	mapMarginBottom:<xsl:value-of select="$page/@popupMapMarginBottom"/>,
	mapMarginLeft:<xsl:value-of select="$page/@popupMapMarginLeft"/>,
	</xsl:if>
	mapFileName:"<xsl:value-of select="$page/@mapFileName" />",
	mapInsetLocation:<xsl:value-of select="$page/@mapInsetLocation" />,
	mapInsetSize:<xsl:value-of select="$page/@mapInsetSize" />,
	mapInsetColor:"<xsl:value-of select="$page/@mapInsetColor" />",
	mapImageSharpening:<xsl:value-of select="$page/@mapImageSharpening" />,
	mapZoomEnabled:<xsl:choose><xsl:when test="$page/@mapShowZoomControl='True'">true</xsl:when><xsl:otherwise>false</xsl:otherwise>		</xsl:choose>,
	zoomInOutControlColor:"<xsl:value-of select="$page/@mapZoomControlColor" />",
	mapZoomLevelV3:<xsl:value-of select="$page/@mapZoomLevel" />,
	mapZoomMidLevelV3:<xsl:value-of select="$page/@mapZoomMidLevel" />,
	mapEditorPanX:<xsl:value-of select="$page/@mapPanX" />,
	mapEditorPanY:<xsl:value-of select="$page/@mapPanY" />,
	mapEditorPercent:<xsl:value-of select="$page/@mapZoomPercent" />,
	mapFocusX:<xsl:value-of select="$page/@mapFocusX" />,
	mapFocusY:<xsl:value-of select="$page/@mapFocusY" />,
	mapFocusPercent:<xsl:value-of select="$page/@mapFocusPercent" />,
	blinkCount:<xsl:value-of select="$page/@blinkCount" />,
	visitedMarkerAlpha:<xsl:value-of select="$page/@visitedMarkerAlpha" />
};

export let page<xsl:value-of select="$page/@pageNumber" />PopupProperties = {
		<xsl:choose>
			<xsl:when test="$page/@popupSlides='True'">
	location:<xsl:value-of select="$page/@popupLocation"/>,
	bestSideSequence:<xsl:value-of select="$page/@popupBestSideSequence"/>,
	allowMouseOverPopup:<xsl:choose>
					<xsl:when test="$page/@popupAllowMouseover='True'">true</xsl:when>
					<xsl:otherwise>false</xsl:otherwise>
				</xsl:choose>,
	delayType:<xsl:value-of select="$page/@popupDelayType"/>,
	delay:<xsl:value-of select="$page/@popupDelay"/>,
	pinOnClick:<xsl:choose>
					<xsl:when test="$page/@popupPinOnClick='True'">true</xsl:when>
					<xsl:otherwise>false</xsl:otherwise>
				</xsl:choose>,
	arrowType:<xsl:value-of select="$page/@popupArrowType"/>,
	markerOffset:<xsl:value-of select="$page/@popupMarkerOffset"/>,
	maxW:<xsl:value-of select="$page/@popupWidth"/>,
	maxH:<xsl:value-of select="$page/@popupHeight"/>,
	minW:<xsl:value-of select="$page/@popupMinWidth"/>,
	minH:<xsl:value-of select="$page/@popupMinHeight"/>,
	textOnlyW:<xsl:value-of select="$page/@popupTextOnlyWidth"/>,
	actualW:0,
	actualH:0,
	left:<xsl:value-of select="$page/@popupLocationX"/>,
	top:<xsl:value-of select="$page/@popupLocationY"/>,
	imageCornerRadius:<xsl:value-of select="$page/@popupImageCornerRadius"/>,
	borderRadius:<xsl:value-of select="$page/@popupCornerRadius"/>,
	borderWidth:<xsl:value-of select="$page/@popupBorderWidth"/>,
	backgroundColor:"<xsl:value-of select="$page/@colorPopupBackground"/>",
	titleColor:"<xsl:value-of select="$page/@colorPopupTitleText"/>",
	textColor:"<xsl:value-of select="$page/@colorPopupText"/>",
	boxShadow:<xsl:value-of select="$page/@popupDropShadowDistance"/>,
	borderColor:"<xsl:value-of select="$page/@colorPopupBorder"/>"
};
			</xsl:when>
			<xsl:otherwise>
};
			</xsl:otherwise>
		</xsl:choose>
		<xsl:value-of select="@uaDetectionJs" disable-output-escaping="yes"/>
	</xsl:template>

  <xsl:template name="EmitLayout">
		<div id="maLayout" class="maLayout">
		</div>
  </xsl:template>
	
  <xsl:template name="EmitHeaderStripe">
    <div id="maHeaderStripe" class="maHeaderStripe"></div>
  </xsl:template>

<!-- Templates that emit all the CSS needed for the tour pages -->

	<xsl:template name="EmitCssForTour">
    <xsl:if test="@centeredInBrowser='True'">
      <xsl:if test="@showMapsAliveLink='1' or @showCustomFooter='1'">
          #maFooter{text-align:center;margin:auto;}
      </xsl:if>
      <xsl:if test="@hasCustomHtmlTop">
          #maCustomHtmlTop{margin:auto;}
      </xsl:if>
      <xsl:if test="@hasCustomHtmlBottom">
          #maCustomHtmlBottom{margin:auto;}
      </xsl:if>
    </xsl:if>

		#maTour {
		<xsl:if test="@hasBackgroundColor='True'">
		width:<xsl:value-of select="@tourWidth"/>px;
		background-color:<xsl:value-of select="@colorTourBackground"/>;
		</xsl:if>
		position:relative;
		font-size:1em;
		}
		#maWaitIndicator {
		z-index:10000;
		visibility:hidden;
		position:absolute;
		display:flex;
		justify-content:center;
		align-items:center;
		padding:0;
		border:none;
		background-color:transparent;
		width:32px;
		height:32px;
		}
		#maWaitIndicator img {
		width:32px;
		height:32px;
		}
		#maLayout {
		box-sizing:border-box;
		position:relative;
		}
		#maLayout.maStacked {
		overflow:auto;
		}
		#maNavPanel {
		position: absolute;
		width: 0px;
		opacity: 0.0;
		background-color:#fff;
		overflow-x: hidden;
		overflow-y: auto;
		transition: all .25s ease-in;
		box-sizing: border-box;
		font-family:Arial, Helvetica, Verdana, Sans-Serif;
		}
		#maNavPanel.show {
		opacity:1;
		box-shadow:#ccccccbc 0px 0px 8px 0px;
		padding: 4px 4px;
		border: solid 1px <xsl:value-of select="@dirBorderColor"/>;
		z-index:1;
		}
		#maNavPanel .maMenuPanel
		{
		margin: 0 0 8px 4px;
		padding-top:4px;
		}
		#maNavPanel .maMenuItemRow {
		width:100%;
		padding-right:4px;
		display:flex;
		}
		#maNavPanel .maHelpButton {
		width:16px;
		height:16px;
		margin-right:4px;
		cursor:pointer;
		}
		#maNavPanel .maMenuItem {
		color:<xsl:value-of select="@colorMenuItemNormalText"/>;
		margin-bottom:4px;
		display:flex;
		cursor:pointer;
		}
		#maNavPanel .maMenuItem.maSelected {
		color:<xsl:value-of select="@colorMenuItemSelectedText"/>;
		font-weight:bold;
		cursor:default;
		}
		#maNavPanel .maMenuItem:hover {
		color:<xsl:value-of select="@colorMenuItemHoverText"/>;
		}
		#maNavPanel .maMenuItem.maSelected:hover {
		color:<xsl:value-of select="@colorMenuItemSelectedText"/>;
		}

		<xsl:if test="@hasCustomHtmlTop">
		#maCustomHtmlTop {
		width:<xsl:value-of select="@tourWidth"/>px;
		}
		</xsl:if>
		
		<xsl:if test="@hasCustomHtmlAbsolute">
		#maCustomHtmlAbsolute {
			position:absolute;
			text-align:left;
			z-index:1;
		}
		</xsl:if>
		
		<xsl:if test="@hasCustomHtmlBottom">
		#maCustomHtmlBottom {
			width:<xsl:value-of select="@tourWidth"/>px;
		}
		</xsl:if>
		
		<xsl:if test="@hasBanner='True'">
			<xsl:call-template name="EmitBannerCSS">
			</xsl:call-template>
		</xsl:if>

		<xsl:call-template name="EmitDirectoryCSS"/>

		<xsl:if test="@hasTitle='True'">
			<xsl:call-template name="EmitTitleBarCSS">
			</xsl:call-template>
		</xsl:if>

		<xsl:if test="@hasHeaderStripe='True'">
			<xsl:call-template name="EmitHeaderStripeCSS">
			</xsl:call-template>
		</xsl:if>

		<xsl:if test="@hasFooterStripe='True'">
			<xsl:call-template name="EmitFooterStripeCSS">
			</xsl:call-template>
		</xsl:if>

		<xsl:if test="@showMapsAliveLink='1' or @showCustomFooter='1'">
			<xsl:call-template name="EmitFooterCSS">
			</xsl:call-template>
		</xsl:if>

		<xsl:call-template name="EmitMapCSS">
		</xsl:call-template>
	</xsl:template>

	<xsl:template name="EmitCssForPage">
		<xsl:param name="page"/>
		<xsl:call-template name="EmitTextAreaCSS">
			<xsl:with-param name="page" select="$page"/>
		</xsl:call-template>
	</xsl:template>

	<!-- Templates to emit CSS for page elements -->

	<xsl:template name="EmitBannerCSS">
		#maBanner {
		justify-content:center;
		<!-- Use the V3 menu selected color as the V4 banner background color. -->
		background-color:<xsl:value-of select="@colorMenuItemSelectedBackground"/>;
		<xsl:if test="@bannerUrl">
			cursor:pointer;
		</xsl:if>
		}
		#maBanner img {
		vertical-align:middle;
		}
	</xsl:template>

	<xsl:template name="EmitDirectoryCSS">
		#maNavPanel .maDirHeader {
		padding:8px 8px 0 8px;
		border-top:solid 6px #ccc;
		}
		#maNavPanel .maDirHeaderTitle {
		color:<xsl:value-of select="@colorMenuItemSelectedText"/>;
		font-weight:bold;
		font-size:16px;
		padding-right:4px;
		white-space:nowrap;
		text-align:center;
		margin-bottom:8px;
		}
		#maNavPanel .maDirToggle {
		font-size:12px;
		font-weight:bold;
		margin-top:4px;
		cursor:pointer;
		background-color:#666;
		color:#fff;
		border-radius:12px;
		padding:3px 6px;
		margin-bottom:3px;
		}
		#maNavPanel .maDirSearchContainer {
		padding:0 4px;
		border-radius:12px;
		border:solid 1px #ccc;
		display:flex;
		align-items:center;
		}
		#maNavPanel .maDirSearchIcon {
		width:20px;
		height:20px;
		}
		#maNavPanel .maDirSearchBox {
		<!-- 16px is min size to prevent iOS from zooming the screen when typing in the search box. -->
		font-size:16px;
		outline:none;
		padding:1px 4px;
		border:none;
		width:80px;
		height:20px;
		}
		#maNavPanel .maDirSearchBox::placeholder {
		font-size:14px;
		}
		#maNavPanel .maDirSearchX {
		width:20px;
		height:20px;
		}
		#maDirPanel {
		font-family:Arial, Helvetica, Verdana, Sans-Serif;
		font-size:16px;
		text-align:left;
		margin-top:4px;
		padding:4px;
		overflow:hidden;
		overflow-y:auto;
		overflow-x:hidden;
		}
		#maDirPanel.maNoDirHeader {
		border-top: solid 3px #ccc;
		}
		#maDirPanel .maDirGroupHeader {
		cursor:pointer;
		display:flex;
		align-items:center;
		}
		#maDirPanel .maDirGroup {
		display:inline;
		user-select:none;
		}
		#maDirPanel .maDirExpander {
		width:16px;
		height:16px;
		margin-right:2px;
		vertical-align:top;
		}
		#maDirPanel .maDirLevel1 {
		color:<xsl:value-of select="@dirLevel1TextColor"/>;
		padding-bottom:4px;
		padding-left:0px;
		margin-top:4px;
		font-weight:bold;
		text-decoration:none;
		}
		#maDirPanel .maDirLevel2 {
		color:<xsl:value-of select="@dirLevel2TextColor"/>;
		margin-top:2px;
		padding-left:18px;
		}
		#maDirPanel .maDirLevelCount {
		color:<xsl:value-of select="@dirEntryCountColor"/>;
		font-weight:normal;
		padding-left:4px;
		}
		#maDirPanel .maDirEntry {
		cursor:pointer;
		color:<xsl:value-of select="@dirEntryTextColor"/>;
		font-weight:normal;
		line-height:20px;
		overflow:hidden;
		text-overflow:ellipsis;
		white-space:nowrap;
		}
		#maDirPanel .maDirEntry:hover {
		color:<xsl:value-of select="@dirEntryTextHoverColor"/>;
		}
		#maDirPreview {
		background-color:<xsl:value-of select="@dirPreviewBackgroundColor"/>;
		border:solid 1px <xsl:value-of select="@dirPreviewBorderColor"/>;
		color:<xsl:value-of select="@dirPreviewTextColor"/>;
		font-family:Arial, Helvetica, Verdana, Sans-Serif;
		font-size:11px;
		overflow:hidden;
		position:absolute;
		text-align:left;
		visibility:hidden;
		z-index:4001;
		}
		#maDirPreview .maDirPreviewImage {
		vertical-align:middle;
		}
		#maDirPreview .maDirPreviewText {
		padding:6px;;
		}
		#maDirPreview .maDirPreviewText div {
		overflow:hidden;
		}
		#maDirPreview .maDirSearchHitHighlight {
		color:<xsl:value-of select="@dirSearchResultTextColor"/>;
		background-color:<xsl:value-of select="@dirSearchResultBackgroundColor"/>;
		}
	</xsl:template>
 
  <xsl:template name="EmitFooterStripeCSS">
    #maFooterStripe {
    background-color:<xsl:value-of select="@colorFooterStripeBackground"/>;
    border-top:solid 1px <xsl:value-of select="@colorFooterStripeTopBorder"/>;
    border-bottom:solid 1px <xsl:value-of select="@colorFooterStripeBottomBorder"/>;
    height:<xsl:value-of select="@footerStripeHeight - @footerStripeBorderHeight"/>px;
		width:100%;
		box-sizing:border-box;
		display:none;
		}
	</xsl:template>

  <xsl:template name="EmitFooterCSS">
		#maFooter {
		flex-direction:column;
		justify-content:center;
		color:<xsl:value-of select="@colorFooterLinkText"/>;
    font-family:<xsl:value-of select="@fontFamilyFooter"/>;
    font-size:<xsl:value-of select="@fontSizeFooter"/>px;
    font-style:<xsl:value-of select="@fontStyleFooter"/>;
    font-weight:<xsl:value-of select="@fontWeightFooter"/>;
    height:<xsl:value-of select="@footerHeight"/>px;
		text-align:center;
		width:100%;
		box-sizing:border-box;
		display:none;
		}
		#maFooter:link {
		color:<xsl:value-of select="@colorFooterLinkText"/>;
    text-decoration:underline;
    }

    #maFooter:visited {
    color:<xsl:value-of select="@colorFooterLinkText"/>;
    text-decoration:underline;
    }

    #maFooter:hover {
    color:<xsl:value-of select="@colorFooterLinkText"/>;
    text-decoration:underline;
    }
  </xsl:template>
  
  <xsl:template name="EmitTextAreaCSS">
		#maContentArea {
		line-height:normal;
		}
		
		#maHotspotTitle, #maPopupTitle {
		font-family:<xsl:value-of select="@fontFamilyHeading"/>;
		font-size:<xsl:value-of select="@fontSizeHeading"/>px;
		font-style:<xsl:value-of select="@fontStyleHeading"/>;
		font-weight:<xsl:value-of select="@fontWeightHeading"/>;
		}

		#maPopupTitle {
	  margin-bottom:6px;
		box-sizing:border-box;
		}
		
		#maHotspotText, #maPopupText {
		font-family:<xsl:value-of select="@fontFamilyDescription"/>;
		font-size:<xsl:value-of select="@fontSizeDescription"/>px;
		font-style:<xsl:value-of select="@fontStyleDescription"/>;
		font-weight:<xsl:value-of select="@fontWeightDescription"/>;
		background-color:<xsl:value-of select="@colorSlideBackground"/>;
		}

		#maPopupText {
			overflow-x:hidden;
			overflow-y:auto;
		}
	</xsl:template>

	<xsl:template name="EmitMapCSS">
		<!-- Use the V3 menu background color as the V4 map canvas background color. -->
		#maMap {
		background-color:<xsl:value-of select="@colorMenuItemNormalBackground"/>;
		font-family: Arial, Helvetica, sans-serif;
		font-size: 18px;
		color: #ccc;
		position: relative;
		}
		#maHelp, #maHelpTitle, #maHelpContent	{
		font-family: Arial, Helvetica, sans-serif;
		}
		#maHelpPanel	{
		top:14px;
		padding:8px;
		border:solid 1px #ccc;
		box-shadow:#ccccccbc 0px 0px 8px 0px;
		transition:left .25s ease-in-out;
		z-index:2;
		}
		#maHelpTitle {
		font-weight:bold;
		font-size:16px;
		margin-bottom:6px;
		}
		#maHelpContent {
		font-size:12px;
		}
		#maGrabber {
		position:absolute;
		opacity:0.5;
		border:solid 1px #fff;
		border-radius:4px 4px;
		background-color:#ccc;
		transition:background-color 1.0s ease-out;
		}
		#maGrabber.maSelected {
		background-color:#000;
		}
	</xsl:template>

  <xsl:template name="EmitTitleBarCSS">
    #maTitleBar {
		position:relative;
    display:flex;
		flex-direction:row;
		justify-content:space-between;
		align-items:center;
		background-color:<xsl:value-of select="@colorTitleBackground"/>;
		height:32px;
		padding:0 <xsl:value-of select="@titleOffsetLeft"/>px 0 <xsl:value-of select="@titleOffsetLeft"/>px;
    width:100%;
		box-sizing:border-box;
		}
		#maTitleText {
    color:<xsl:value-of select="@colorTitleText" />;
		font-family:Arial, Helvetica, Verdana, sans-serif;
		font-size:<xsl:value-of select="@fontSizeTitle"/>px;
    font-style:<xsl:value-of select="@fontStyleTitle"/>;
    font-weight:<xsl:value-of select="@fontWeightTitle"/>;
		overflow:hidden;
		}
	</xsl:template>

  <xsl:template name="EmitHeaderStripeCSS">
    #maHeaderStripe {
    background-color:<xsl:value-of select="@colorHeaderStripeBackground"/>;
		border-bottom:solid 1px <xsl:value-of select="@colorHeaderStripeBottomBorder"/>;
		border-top:solid 1px <xsl:value-of select="@colorHeaderStripeTopBorder"/>;
    font-size:5px; <!-- needed to force the height attribute to work in IE -->
    height:<xsl:value-of select="@headerStripeHeight - @headerStripeBorderHeight"/>px;
		width:100%;
		box-sizing:border-box;
		}
	</xsl:template>

</xsl:stylesheet>


