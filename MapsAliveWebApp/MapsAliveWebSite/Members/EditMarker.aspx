<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="EditMarker.aspx.cs"
	Inherits="Members_EditMarker"
	Trace="false"
	TraceMode="SortByCategory"
%>

<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<script type="text/javascript" language="javascript">                
	var isTextMarker;
	function maOnPreviewRollover(over)
	{
		var img = document.getElementById(formContentId + "PreviewImage");
		img.src = "MarkerRenderer.ashx?actual=0&state=" + (over ? "1" : "0") + "&id=" + selectedMarkerId;
	}
	function maChangeNormalOpacity(sender, eventArgs)
	{
		maChangeOpacity(sender, "<%= NormalOpacityValue.ClientID %>", true);
	}
	function maChangeSelectedOpacity(sender, eventArgs)
	{
		maChangeOpacity(sender, "<%= SelectedOpacityValue.ClientID %>", false);
	}
	function maChangeOpacity(sender, clientId, normal)
	{
		maChangeDetected();
		var e = document.getElementById(clientId);
		var value = sender.get_value();
		e.innerHTML = value + " px";
	}
	function maChangePhotoScale(sender, eventArgs)
	{
		maChangeDetected();
		var e = document.getElementById("<%= PhotoScaleValue.ClientID %>");
		var value = sender.get_value();
		e.innerHTML = value + "%";
	}
	function maResourceChanged(sender)
	{
		var comboBoxId = sender.get_element().id;
		if (comboBoxId == "<%= MarkerStyleComboBox.ClientID %>" + "_TourResourceComboBox")
			maMarkerStyleChanged();
		else if (comboBoxId == "<%= FontStyleComboBox.ClientID %>" + "_TourResourceComboBox")
			maFontStyleChanged();
		else if (comboBoxId == "<%= NormalSymbolComboBox.ClientID %>" + "_TourResourceComboBox")
			maChangeDetectedForPreview();
		else if (comboBoxId == "<%= SelectedSymbolComboBox.ClientID %>" + "_TourResourceComboBox")
			maChangeDetectedForPreview();
	}
	function maMarkerStyleChanged()
	{
		maChangeDetectedForPreview();
		maTourResourceSelectionChanged("<%= MarkerStyleComboBox.ClientID %>", "<%= EditMarkerStyleControl.ClientID %>", "EditMarkerStyle.aspx");
	}
	function maFontStyleChanged()
	{
		maChangeDetectedForPreview();
		maTourResourceSelectionChanged("<%= FontStyleComboBox.ClientID %>", "<%= EditFontStyleControl.ClientID %>", "EditFontStyle.aspx");
	}
	</script> 

	<div style="position:relative;">
		<div style="position:absolute;left:500px;top:0px;width:214px;">
			<div style="padding-left:4px;">
				<AvantLogic:QuickHelpTitle ID="MarkerPreview" Title="Marker Preview" runat="server" TopMargin="0px" Span="true" />
			</div>
			<asp:Panel ID="PreviewPanel" runat="server">
				<img id="PreviewImage" runat="server" style="margin-top:4px;margin-left:4px;" onmouseover="maOnPreviewRollover(true)" onmouseout="maOnPreviewRollover(false)" />
				<div id="PreviewMessage"></div>
			</asp:Panel>
		</div>

		<AvantLogic:QuickHelpTitle ID="MarkerName" runat="server" Title="Marker Name" TopMargin="0px" Span="true" />
		<AvantLogic:MemberPageActionButton ID="ShowUsageControl" runat="server" />
		<br />
		<asp:TextBox ID="MarkerNameTextBox" runat="server" Width="400"></asp:TextBox>
		<asp:Label ID="MarkerNameError" runat="server" CssClass="textErrorMessage" />
		
		<AvantLogic:QuickHelpTitle ID="MarkerTypeChoice" runat="server" Title="Marker Type" />
		<telerik:RadComboBox 
			id="MarkerTypeComboBox" 
			Runat="server"
			Width="150"
			AutoPostBack="True"
			Skin="Default"
			OnSelectedIndexChanged="OnOptionChanged"
			>
			<ExpandAnimation Type="None" />
			<CollapseAnimation Type="None" />
			<Items>
				<telerik:RadComboBoxItem runat="server" Text="Symbol" Value="1" />
				<telerik:RadComboBoxItem runat="server" Text="Shape" Value="2" />
				<telerik:RadComboBoxItem runat="server" Text="Symbol + Shape" Value="3" />	
				<telerik:RadComboBoxItem runat="server" Text="Text" Value="4" />
				<telerik:RadComboBoxItem runat="server" Text="Photo" Value="5" />
			</Items>
		</telerik:RadComboBox>
					
		<asp:Panel ID="MarkerShapePanel" runat="server" style="margin-top:16px;">
			<AvantLogic:QuickHelpTitle ID="MarkerShape" runat="server" Title="<%$ Resources:Text, MarkerBackgroundShapeLabel %>" TopMargin="0px" />
			<telerik:RadComboBox 
				id="MarkerShapeComboBox" 
				Runat="server"
				AutoPostBack="True"
				Skin="Default"
				OnSelectedIndexChanged="OnOptionChanged"
				>
				<ExpandAnimation Type="None" />
				<CollapseAnimation Type="None" />
			</telerik:RadComboBox>
		</asp:Panel>

		<asp:Panel ID="MarkerStylePanel" runat="server" style="margin-top:16px;">
			<div>
				<AvantLogic:QuickHelpTitle ID="MarkerStyleChoice" runat="server" Title="Marker Style" Span="true" TopMargin="0px" />
			</div>
			<table cellpadding="0" cellspacing="0">
				<tr>
					<td>
						<uc:TourResourceComboBox Id="MarkerStyleComboBox" runat="server" />
					</td>
					<td style="padding-left:4px;">
						<AvantLogic:MemberPageActionButton Subtle="true" ID="EditMarkerStyleControl" runat="server" />
					</td>
					<td style="padding-left:4px;">
						<AvantLogic:MemberPageActionButton Subtle="true" ID="NewMarkerStyleControl" runat="server" />
					</td>
				</tr>
			</table>
			<asp:Panel ID="PhotoCaptionPanel" runat="server" style="margin-top:16px;">
				<AvantLogic:QuickHelpTitle ID="PhotoCaptionPosition" runat="server" Title="Photo Caption Text Location" />
				<telerik:RadComboBox 
					id="PhotoCaptionPositionComboBox" 
					Runat="server"
					Width="150"
					Skin="Default"
					AutoPostBack="True"
					OnSelectedIndexChanged="OnOptionChanged"
					>
					<ExpandAnimation Type="None" />
					<CollapseAnimation Type="None" />
					<Items>
						<telerik:RadComboBoxItem runat="server" Text="None" Value="0" Selected="True" />
						<telerik:RadComboBoxItem runat="server" Text="Top" Value="1" />
						<telerik:RadComboBoxItem runat="server" Text="Bottom" Value="2" />
					</Items>
				</telerik:RadComboBox>
			</asp:Panel>
			
			<asp:Panel ID="TextOptions_Colors" runat="server" style="margin-bottom:16px;">
				<div class="optionsSectionTitle">Text Colors</div>
				<table cellpadding="0" cellspacing="0">
					<tr>
						<td style="padding-right:24px;">
							<AvantLogic:QuickHelpTitle ID="TextColorNormal" runat="server" Title="Normal" TopMargin="0px" />
							<AvantLogic:ColorSwatch Id="NormalTextColorSwatch" runat="server" ForPreview="true" />
						</td>
						<td style="padding-right:24px;">
							<AvantLogic:QuickHelpTitle ID="TextColorSelected" runat="server" Title="Selected" TopMargin="0px" />
							<AvantLogic:ColorSwatch Id="SelectedTextColorSwatch" runat="server" ForPreview="true" />
						</td>
					</tr>
				</table>
			</asp:Panel>
		</asp:Panel>
		
		<asp:Panel ID="TextOptionsPanel1" runat="server">
			<div class="optionsSectionTitle">Text Options</div>
			<AvantLogic:QuickHelpTitle ID="TextMarkerString" runat="server" Title="Text" TopMargin="0px" />
			<asp:TextBox ID="TextMarkerStringTextBox" runat="server" Width="290" />
			<asp:Label ID="TextMarkerStringError" runat="server" CssClass="textErrorMessage" />
		
			<div style="margin-top:12px;">
				<AvantLogic:QuickHelpTitle ID="MarkerFontStyle" runat="server" Title="Font" Span="true" />
			</div>
			<table cellpadding="0" cellspacing="0">
				<tr>
					<td>
						<uc:TourResourceComboBox Id="FontStyleComboBox" runat="server" />
					</td>
					<td style="padding-left:4px;">
						<AvantLogic:MemberPageActionButton Subtle="true" ID="EditFontStyleControl" runat="server" />
					</td>
				</tr>
			</table>
				
			<asp:Panel ID="TextAlignmentHOptionsPanel" runat="server">
				<AvantLogic:QuickHelpTitle ID="MarkerTextAlignmentH" runat="server" Title="Horizontal Alignment" />
				<telerik:RadComboBox 
					id="TextAlignmentHComboBox" 
					Runat="server"
					Width="150"
					Skin="Default"
					OnClientSelectedIndexChanged="maChangeDetectedForPreview"
					>
					<ExpandAnimation Type="None" />
					<CollapseAnimation Type="None" />
					<Items>
						<telerik:RadComboBoxItem runat="server" Text="Left" Value="0" Selected="True" />
						<telerik:RadComboBoxItem runat="server" Text="Center" Value="1" />
						<telerik:RadComboBoxItem runat="server" Text="Right" Value="2" />
					</Items>
				</telerik:RadComboBox>
			</asp:Panel>
		</asp:Panel>
			
		<asp:Panel ID="TextAlignmentVOptionsPanel" runat="server">
			<AvantLogic:QuickHelpTitle ID="MarkerTextAlignmentV" runat="server" Title="Vertical Alignment" />
			<telerik:RadComboBox 
				id="TextAlignmentVComboBox" 
				Runat="server"
				Width="150"
				Skin="Default"
				OnClientSelectedIndexChanged="maChangeDetectedForPreview"
				>
				<ExpandAnimation Type="None" />
				<CollapseAnimation Type="None" />
				<Items>
					<telerik:RadComboBoxItem runat="server" Text="Top" Value="0" Selected="True" />
					<telerik:RadComboBoxItem runat="server" Text="Center" Value="1" />
					<telerik:RadComboBoxItem runat="server" Text="Bottom" Value="2" />
				</Items>
			</telerik:RadComboBox>
		</asp:Panel>
			
		<asp:Panel ID="TextPaddingPanel" runat="server">
			<AvantLogic:QuickHelpTitle ID="MarkerTextPadding" runat="server" Title="Text Padding" />
			<asp:TextBox ID="TextPaddingTextBox" runat="server" Width="30px" /><span class="unit">px</span>
			<asp:Label ID="TextPaddingError" runat="server" CssClass="textErrorMessage" />
		</asp:Panel>
		
		<asp:Panel ID="SymbolsPanel" runat="server">
			<div class="optionsSectionTitle">Symbol Options</div>
			<div style="margin-right:16px;">
				<AvantLogic:QuickHelpTitle ID="MarkerNormalSymbol" runat="server" Title="<%$ Resources:Text, MarkerNormalSymbolLabel %>" Span="true" />
				<table cellpadding="0" cellspacing="0">
					<tr>
						<td>
							<uc:TourResourceComboBox Id="NormalSymbolComboBox" runat="server" />
						</td>
						<td style="padding-left:4px;">
							<AvantLogic:MemberPageActionButton Subtle="true" ID="EditNormalSymbolControl" runat="server" />
						</td>
					</tr>
				</table>
			</div>
			<div style="margin-top:16px;">
				<AvantLogic:QuickHelpTitle ID="MarkerSelectedSymbol" runat="server" Title="<%$ Resources:Text, MArkerSelectedSymbolLabel %>" Span="true" />
				<table cellpadding="0" cellspacing="0">
					<tr>
						<td>
							<uc:TourResourceComboBox Id="SelectedSymbolComboBox" runat="server" />
						</td>
						<td style="padding-left:4px;">
							<AvantLogic:MemberPageActionButton Subtle="true" ID="EditSelectedSymbolControl" runat="server" />
						</td>
					</tr>
				</table>
			</div>
		</asp:Panel>

		<asp:Panel ID="ShapePanels" runat="server">
			<div class="optionsSectionTitle">Shape Options</div>
			
			<asp:Panel ID="TextOptionsPanel2" runat="server">
				<asp:CheckBox ID="TextAutoSizeCheckBox" runat="server" />
				<AvantLogic:QuickHelpTitle ID="TextMarkerAutoSize" runat="server" Title="Auto Size" Span="true" />
			</asp:Panel>

			<asp:Panel ID="CirclePanel" runat="server">
				<AvantLogic:QuickHelpTitle ID="CircleDiameter" runat="server" Title="Circle Radius" />
				<asp:TextBox ID="CircleRadiusTextBox" runat="server" Width="30"></asp:TextBox><span class="unit">px</span>
				<asp:Label ID="CircleRadiusError" runat="server" CssClass="textErrorMessage" />
			</asp:Panel>
					
			<asp:Panel ID="RectanglePanel" runat="server">
				<AvantLogic:QuickHelpTitle ID="MarkerRectangleSize" runat="server" Title="<%$ Resources:Text, RectangleSizeLabel %>" />
				<table>
					<tr>
						<td align="Right">
							<asp:Label runat="server" CssClass="controlLabel" Text="<%$ Resources:Text, RectangleWidthLabel %>"></asp:Label>
						</td>
						<td>
							<asp:TextBox ID="RectangleWidthTextBox" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
							<asp:Label ID="RectangleWidthError" runat="server" CssClass="textErrorMessage" />
						</td>
					</tr>
					<tr>
						<td align="Right">
							<asp:Label runat="server" CssClass="controlLabel" Text="<%$ Resources:Text, RectangleHeightLabel %>"></asp:Label>
						</td>
						<td>
							<asp:TextBox ID="RectangleHeightTextBox" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
							<asp:Label ID="RectangleHeightError" runat="server" CssClass="textErrorMessage" />
						</td>
					</tr>
				</table>
			</asp:Panel>
						
			<asp:Panel ID="PolygonHelpPanel" runat="server" Visible="false" CssClass="memberPageSpecialNotice" style="font-weight:normal;">
				<asp:Label ID="PolygonHelp" runat="server" />
			</asp:Panel>

			<asp:Panel ID="SymbolLocationPanel" runat="server" style="margin-top:8px;">
				<AvantLogic:QuickHelpTitle ID="SymbolLocationOption" runat="server" Title="<%$ Resources:Text, SymbolLocationOptionLabel %>" />
				<telerik:RadComboBox 
					id="SymbolLocationComboBox" 
					Runat="server"
					AutoPostBack="True"
					Skin="Default"
					OnSelectedIndexChanged="OnOptionChanged"
					>
					<ExpandAnimation Type="None" />
					<CollapseAnimation Type="None" />
					<Items>
						<telerik:RadComboBoxItem runat="server" Text="Centered in shape" Value="0" Selected="True" />
						<telerik:RadComboBoxItem runat="server" Text="Positioned at X,Y" Value="1" />
					</Items>
				</telerik:RadComboBox>
				
				<asp:Panel ID="SymbolLocationXYPanel" runat="server" >
					<table style="margin-top:8px;">
						<tr>
							<td align="Right">
								<asp:Label ID="Label1" runat="server" CssClass="controlLabel" Text="<%$ Resources:Text, SymbolLocationXLabel %>"></asp:Label>
							</td>
							<td>
								<asp:TextBox ID="SymbolXTextBox" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
								<asp:Label ID="SymbolXError" runat="server" CssClass="textErrorMessage" />
							</td>
							<td align="Right" style="padding-left:16px;">
								<asp:Label ID="Label2" runat="server" CssClass="controlLabel" Text="<%$ Resources:Text, SymbolLocationYLabel %>"></asp:Label>
							</td>
							<td>
								<asp:TextBox ID="SymbolYTextBox" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
								<asp:Label ID="SymbolYError" runat="server" CssClass="textErrorMessage" />
							</td>
						</tr>
					</table>
				</asp:Panel>
			</asp:Panel>
			
			<asp:Panel ID="ScaleShapePanel" runat="server" style="margin-top:16px">
				<asp:CheckBox ID="ScaleShapeCheckBox" runat="server" />
				<AvantLogic:QuickHelpTitle ID="MarkerScaleShape" runat="server" Title="Scale Shape to Map" Span="true" />
			</asp:Panel>
		</asp:Panel>

		<asp:Panel ID="PhotoPanel" runat="server">
			<div class="optionsSectionTitle">Marker Size Options</div>
			<AvantLogic:QuickHelpTitle ID="PhotoConstraint" runat="server" Title="Image Size Constraint" TopMargin="0px" />
			<telerik:RadComboBox 
				id="PhotoConstraintComboBox" 
				Runat="server"
				Width="150"
				AutoPostBack="True"
				Skin="Default"
				OnSelectedIndexChanged="OnOptionChanged"
				>
				<ExpandAnimation Type="None" />
				<CollapseAnimation Type="None" />
				<Items>
					<telerik:RadComboBoxItem runat="server" Text="Scale Percent" Value="0" Selected="True" />
					<telerik:RadComboBoxItem runat="server" Text="Fixed Width" Value="1" />
					<telerik:RadComboBoxItem runat="server" Text="Fixed Height" Value="2" />
					<telerik:RadComboBoxItem runat="server" Text="Fixed Width & Height" Value="3" />
				</Items>
			</telerik:RadComboBox>
			
			<asp:Panel ID="PhotoScalePanel" runat="server">
				<table>
					<tr>
						<td colspan="2">
							<AvantLogic:QuickHelpTitle ID="PhotoMarkerScale" runat="server" Title="Scale Percent" />
							<table>
								<tr>
									<td>
										<telerik:RadSlider
											ID="SliderPhotoScale"
											runat="server"
											Width="140"
											MinimumValue="1"
											MaximumValue="100"
											Value="100"
											OnClientValueChanged="maChangePhotoScale"
											Skin="Default"
											DragText=""
										  />
									</td>
									<td>
										<asp:Label ID="PhotoScaleValue" runat="server" CssClass="unit"/>
									</td>
								</tr>
							</table>
						</td>
					</tr>
				</table>
			</asp:Panel>
				
			<asp:Panel ID="PhotoSizePanel" runat="server">
				<AvantLogic:QuickHelpTitle ID="PhotoMarkerSize" runat="server" Title="Image Area Size" />
				<table>
					<tr ID="PhotoWidthRow" runat="server">
						<td align="Right">
							<asp:Label runat="server" CssClass="controlLabel" Text="<%$ Resources:Text, RectangleWidthLabel %>"></asp:Label>
						</td>
						<td>
							<asp:TextBox ID="PhotoWidthTextBox" runat="server" Width="30px" /><span class="unit">px</span>
							<asp:Label runat="server" CssClass="textErrorMessage" />
						</td>
					</tr>
					<tr ID="PhotoHeightRow" runat="server">
						<td align="Right">
							<asp:Label runat="server" CssClass="controlLabel" Text="<%$ Resources:Text, RectangleHeightLabel %>"></asp:Label>
						</td>
						<td>
							<asp:TextBox ID="PhotoHeightTextBox" runat="server" Width="30px" /><span class="unit">px</span>
							<asp:Label runat="server" CssClass="textErrorMessage" />
						</td>
					</tr>
				</table>
				
				<asp:Panel ID="CropOptionsPanel" runat="server">
					<table cellpadding = "0" cellspacing = "0">
						<tr>
							<td>
								<AvantLogic:QuickHelpTitle ID="PhotoCropOptions" runat="server" Title="Cropping / Alignment" />
								<telerik:RadComboBox 
									id="PhotoCropOptionsComboBox" 
									Runat="server"
									Width="150"
									Skin="Default"
									OnClientSelectedIndexChanged="maChangeDetectedForPreview"
									>
									<ExpandAnimation Type="None" />
									<CollapseAnimation Type="None" />
									<Items>
										<telerik:RadComboBoxItem runat="server" Text="Scale and Trim" Value="0" Selected="True" />
										<telerik:RadComboBoxItem runat="server" Text="Align Left or Top" Value="1" />
										<telerik:RadComboBoxItem runat="server" Text="Align Center" Value="2" />
										<telerik:RadComboBoxItem runat="server" Text="Align Right or Bottom" Value="3" />
										<telerik:RadComboBoxItem runat="server" Text="Crop Center" Value="4" />
										<telerik:RadComboBoxItem runat="server" Text="Crop N" Value="5" />
										<telerik:RadComboBoxItem runat="server" Text="Crop NE" Value="6" />
										<telerik:RadComboBoxItem runat="server" Text="Crop E" Value="7" />
										<telerik:RadComboBoxItem runat="server" Text="Crop SE" Value="8" />
										<telerik:RadComboBoxItem runat="server" Text="Crop S" Value="9" />
										<telerik:RadComboBoxItem runat="server" Text="Crop SW" Value="10" />
										<telerik:RadComboBoxItem runat="server" Text="Crop W" Value="11" />
										<telerik:RadComboBoxItem runat="server" Text="Crop NW" Value="12" />
									</Items>
								</telerik:RadComboBox>
							</td>
							<td style="padding-left:24px;">
								<AvantLogic:QuickHelpTitle ID="PhotoCropFactor" runat="server" Title="Crop Factor" />
								<telerik:RadComboBox 
									id="PhotoCropFactorComboBox" 
									Runat="server"
									Width="40"
									Skin="Default"
									OnClientSelectedIndexChanged="maChangeDetectedForPreview"
									>
									<ExpandAnimation Type="None" />
									<CollapseAnimation Type="None" />
									<Items>
										<telerik:RadComboBoxItem runat="server" Text="1x" Value="0" Selected="True" />
										<telerik:RadComboBoxItem runat="server" Text="2x" Value="1" />
										<telerik:RadComboBoxItem runat="server" Text="3x" Value="2" />
										<telerik:RadComboBoxItem runat="server" Text="4x" Value="3" />
										<telerik:RadComboBoxItem runat="server" Text="5x" Value="4" />
										<telerik:RadComboBoxItem runat="server" Text="6x" Value="5" />
										<telerik:RadComboBoxItem runat="server" Text="7x" Value="6" />
										<telerik:RadComboBoxItem runat="server" Text="8x" Value="7" />
										<telerik:RadComboBoxItem runat="server" Text="9x" Value="8" />
										<telerik:RadComboBoxItem runat="server" Text="10x" Value="9" />
									</Items>
								</telerik:RadComboBox>
							</td>
						</tr>
					</table>
				</asp:Panel>
			</asp:Panel>
						
			<asp:Panel ID="PhotoPaddingPanel" runat="server">
				<AvantLogic:QuickHelpTitle ID="PhotoMarkerPadding" runat="server" Title="Matte" />
				<asp:TextBox ID="PhotoPaddingTextBox" runat="server" Width="30px" /><span class="unit">px</span>
				<asp:Label ID="PhotoPaddingError" runat="server" CssClass="textErrorMessage" />
			</asp:Panel>
		
			<div class="optionsSectionTitle">Image Special Effects</div>
			
			<table cellspacing="0" cellpadding="0">
				<tr>
					<td width="200px">
						<AvantLogic:QuickHelpTitle ID="PhotoMarkerNormalEffect" runat="server" Title="Normal Image" TopMargin="0px" OffsetY="-80" />
						<telerik:RadComboBox 
							id="PhotoEffectNormalComboBox" 
							Runat="server"
							Width="150"
							Skin="Default"
							OnClientSelectedIndexChanged="maChangeDetectedForPreview"
							>
							<ExpandAnimation Type="None" />
							<CollapseAnimation Type="None" />
							<Items>
								<telerik:RadComboBoxItem runat="server" Text="Color (no special effect)" Value="0" Selected="True" />
								<telerik:RadComboBoxItem runat="server" Text="Black & White" Value="1" />
								<telerik:RadComboBoxItem runat="server" Text="Sepia" Value="2" />
								<telerik:RadComboBoxItem runat="server" Text="Color Negative" Value="3" />
								<telerik:RadComboBoxItem runat="server" Text="Black & White Negative" Value="4" />
							</Items>
						</telerik:RadComboBox>
					</td>
					<td>
						<AvantLogic:QuickHelpTitle ID="PhotoMarkerSelectedEffect" runat="server" Title="Selected Image" TopMargin="0px" OffsetY="-80" />
						<telerik:RadComboBox 
							id="PhotoEffectSelectedComboBox" 
							Runat="server"
							Width="150"
							Skin="Default"
							OnClientSelectedIndexChanged="maChangeDetectedForPreview"
							>
							<ExpandAnimation Type="None" />
							<CollapseAnimation Type="None" />
							<Items>
								<telerik:RadComboBoxItem runat="server" Text="Color (no special effect)" Value="0" Selected="True" />
								<telerik:RadComboBoxItem runat="server" Text="Black & White" Value="1" />
								<telerik:RadComboBoxItem runat="server" Text="Sepia" Value="2" />
								<telerik:RadComboBoxItem runat="server" Text="Color Negative" Value="3" />
								<telerik:RadComboBoxItem runat="server" Text="Black & White Negative" Value="4" />
							</Items>
						</telerik:RadComboBox>
					</td>
				</tr>
				<tr>
					<td>
						<AvantLogic:QuickHelpTitle ID="PhotoMarkerNormalOpacity" runat="server" Title="Normal Opacity" OffsetY="-100" />
						<table>
							<tr>
								<td>
									<telerik:RadSlider
										ID="SliderNormalOpacity"
										runat="server"
										Width="110"
										MinimumValue="0"
										MaximumValue="100"
										Value="100"
										OnClientValueChanged="maChangeNormalOpacity"
										Skin="Default"
										DragText=""
									  />
								</td>
								<td>
									<asp:Label ID="NormalOpacityValue" runat="server" CssClass="unit"/>
								</td>
							</tr>		
						</table>
					</td>
					<td>
						<AvantLogic:QuickHelpTitle ID="PhotoMarkerSelectedOpacity" runat="server" Title="Selected Opacity" OffsetY="-100" />
						<table>
							<tr>
								<td>
									<telerik:RadSlider
										ID="SliderSelectedOpacity"
										runat="server"
										Width="110"
										MinimumValue="0"
										MaximumValue="100"
										Value="100"
										OnClientValueChanged="maChangeSelectedOpacity"
										Skin="Default"
										DragText=""
									  />
								</td>
								<td>
									<asp:Label ID="SelectedOpacityValue" runat="server" CssClass="unit"/>
								</td>
							</tr>
						</table>	
					</td>		
				</tr>
			</table>
		</asp:Panel>
			
		<asp:Panel ID="AnchorLocationPanel" runat="server" style="margin-top:8px;">
			<AvantLogic:QuickHelpTitle ID="AnchorLocation" runat="server" Title="Anchor Location" OffsetY="-100" OffsetX="20" />
			<telerik:RadComboBox 
				id="AnchorLocationComboBox" 
				Runat="server"
				AutoPostBack="True"
				Skin="Default"
				OnSelectedIndexChanged="OnOptionChanged"
				>
				<ExpandAnimation Type="None" />
				<CollapseAnimation Type="None" />
				<Items>
					<telerik:RadComboBoxItem runat="server" Text="Center" Value="0" Selected="True" />
					<telerik:RadComboBoxItem runat="server" Text="Off Center" Value="1" />
				</Items>
			</telerik:RadComboBox>
			
			<asp:Panel ID="AnchorLocationXYPanel" runat="server" >
				<table style="margin-top:8px;">
					<tr>
						<td align="Right">
							<asp:Label runat="server" CssClass="controlLabel" Text="X" />
						</td>
						<td>
							<asp:TextBox ID="AnchorXTextBox" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
							<asp:Label ID="AnchorXError" runat="server" CssClass="textErrorMessage" />
						</td>
						<td align="Right" style="padding-left:16px;">
							<asp:Label runat="server" CssClass="controlLabel" Text="Y" />
						</td>
						<td>
							<asp:TextBox ID="AnchorYTextBox" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
							<asp:Label ID="AnchorYError" runat="server" CssClass="textErrorMessage" />
						</td>
					</tr>
				</table>
				<div>
					<asp:Label ID="SymbolDimensions" runat="server" CssClass="finePrint" style="color:Gray;" />
				</div>
			</asp:Panel>
		</asp:Panel>
	</div>
</asp:Content>
