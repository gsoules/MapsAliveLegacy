<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="PopupAppearance.aspx.cs"
	Inherits="Members_PopupAppearance"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
    <script type="text/javascript">
		function maOnColorChanged(swatchId, color)
		{
			if (
				swatchId == "<%= PopupSlideBackgroundColorSwatch.ClientID %>_Swatch" ||
				swatchId == "<%= PopupSlideTitleTextColorSwatch.ClientID %>_Swatch" ||
				swatchId == "<%= PopupSlideTextColorSwatch.ClientID %>_Swatch")
			{
				var e = document.getElementById("<%= UseColorSchemeColorsCheckBox.ClientID %>");
				e.checked = false;
			}
		}
   </script>
	
	<div style="position:relative;">
		<table cellpadding="0" cellspacing="0" style="width:715px;">
			<tr>
				<td valign="top" style="width:240px;">
					<div class="optionsSectionTitleFirst">Sizes</div>
					<table cellpadding="0" cellspacing="0">
						<tr runat="server">
							<td>
								<AvantLogic:QuickHelpTitle ID="PopupMaxSize" runat="server" Title="Max Size" TopMargin="0px" />
								<table>
									<tr>
										<td align="Right" style="padding-left:8px;">
											<asp:Label runat="server" CssClass="controlLabelNested" Text="Width"/>
										</td>
										<td>
											<asp:TextBox ID="PopupSlideWidth" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
											<asp:Label ID="PopupSlideWidthError" runat="server" CssClass="textErrorMessage" />
										</td>
										<td align="Right" style="padding-left:16px;">
											<asp:Label runat="server" CssClass="controlLabelNested" Text="Height"/>
										</td>
										<td>
											<asp:TextBox ID="PopupSlideHeight" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
											<asp:Label ID="PopupSlideHeightError" runat="server" CssClass="textErrorMessage" />
										</td>
									</tr>
								</table>
							</td>
						</tr>
						<tr>
							<td style="padding-top:8px;">
								<AvantLogic:QuickHelpTitle ID="PopupMinSize" runat="server" Title="Min Size" TopMargin="0px" />
								<table>
									<tr>
										<td align="Right" style="padding-left:8px;">
											<asp:Label runat="server" CssClass="controlLabelNested" Text="Width"/>
										</td>
										<td>
											<asp:TextBox ID="PopupSlideMinWidth" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
											<asp:Label ID="PopupSlideMinWidthError" runat="server" CssClass="textErrorMessage" />
										</td>
										<td align="Right" style="padding-left:16px;">
											<asp:Label runat="server" CssClass="controlLabelNested" Text="Height"/>
										</td>
										<td>
											<asp:TextBox ID="PopupSlideMinHeight" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
											<asp:Label ID="PopupSlideMinHeightError" runat="server" CssClass="textErrorMessage" />
										</td>
									</tr>
								</table>
							</td>
						</tr>
						<tr>
							<td colspan="4" style="padding-top:8px;">
                                <asp:Panel ID="PopupTextOnlyPanel" runat="server">
								    <AvantLogic:QuickHelpTitle ID="PopupTextOnly" runat="server" Title="Text-Only Width" TopMargin="0px" />
								    <asp:TextBox ID="PopupTextOnlyWidthTextBox" runat="server" Width="30px" /><span class="unit">px</span>
								    <asp:Label ID="PopupTextOnlyWidthError" runat="server" CssClass="textErrorMessage" />
                                </asp:Panel>
							</td>
						</tr>
					</table>
				</td>
				<td valign="top" style="padding-left:16px;">
					<div class="optionsSectionTitleFirst">Options</div>
					
					<table cellpadding="0" cellspacing="0">
						<tr>
							<td style="padding-left:4px;">
								<AvantLogic:QuickHelpTitle ID="PopupBorderThickness" runat="server" Title="Border Thickness" TopMargin="0px" />
								<asp:TextBox ID="PopupSlideBorderWidth" runat="server" Width="30px" /><span class="unit">px</span>
								<asp:Label ID="PopupSlideBorderWidthError" runat="server" CssClass="textErrorMessage" />
							</td>
							<td style="padding-left:40px;">
								<AvantLogic:QuickHelpTitle ID="PopupBorderColor" runat="server" Title="Border Color" TopMargin="0px" />
								<AvantLogic:ColorSwatch Id="PopupSlideBorderColorSwatch" runat="server" ForPreview="true" />
							<td style="padding-left:24px;">
								<AvantLogic:QuickHelpTitle ID="PopupTitleTextColor" runat="server" Title="Title Text Color"  TopMargin="0px"/>
								<AvantLogic:ColorSwatch Id="PopupSlideTitleTextColorSwatch" runat="server" ForPreview="true" />
							</td>
							</td>
						</tr>
						<tr>
							<td style="padding-left:4px;">
								<AvantLogic:QuickHelpTitle ID="PopupBackgroundColor" runat="server" Title="Background Color" />
								<AvantLogic:ColorSwatch Id="PopupSlideBackgroundColorSwatch" runat="server" ForPreview="true" />
							</td>
							<td style="padding-left:40px;">
								<AvantLogic:QuickHelpTitle ID="PopupTextColor" runat="server" Title="Text Color" />
								<AvantLogic:ColorSwatch Id="PopupSlideTextColorSwatch" runat="server" ForPreview="true" />
							</td>
						</tr>
						<tr>
							<td></td>
							<td style="padding-left:40px;padding-top:16px;">
								<asp:CheckBox ID="UseColorSchemeColorsCheckBox" runat="server" />
								<AvantLogic:QuickHelpTitle ID="UseTourStyleColorsForPopup" runat="server" Title="Use Tour Colors" Span="true" />
							</td>
						</tr>
						<tr>
							<td style="padding-left:4px;padding-top:16px;">
								<AvantLogic:QuickHelpTitle ID="PopupCornerRadiusPx" runat="server" Title="Popup Corner Radius" TopMargin="0px" />
								<asp:TextBox ID="PopupCornerRadius" runat="server" Width="30px" /><span class="unit">px</span>
								<asp:Label ID="PopupCornerRadiusError" runat="server" CssClass="textErrorMessage" />
							</td>
							<td style="padding-left:40px;padding-top:16px;">
								<AvantLogic:QuickHelpTitle ID="ImageCornerRadiusPx" runat="server" Title="Image Corner Radius" TopMargin="0px" />
								<asp:TextBox ID="ImageCornerRadius" runat="server" Width="30px" /><span class="unit">px</span>
								<asp:Label ID="ImageCornerRadiusError" runat="server" CssClass="textErrorMessage" />
							</td>
							<td style="padding-left:28px;padding-top:16px;">
								<AvantLogic:QuickHelpTitle ID="DropShadowDistancePx" runat="server" Title="Drop Shadow" TopMargin="0px" />
								<asp:TextBox ID="DropShadowDistance" runat="server" Width="30px" /><span class="unit">px</span>
								<asp:Label ID="DropShadowDistanceError" runat="server" CssClass="textErrorMessage" />
							</td>
						</tr>
					</table>
				</td>
			</tr>
		</table>
		
		<div class="optionsSectionTitle">Margins and Spacing</div>
		<uc:MarginsAndSpacing ID="MarginsAndSpacingControl" runat="server" />

		<div id="PreviewMessage" class="optionsSectionTitle">Preview</div>

		<asp:Panel ID="LayoutPreviewImagePanel" runat="server" style="margin-top:8px;">
			<div class="finePrintHelp" style="margin-bottom:12px;">
			This preview is an approximation. For more accuracy, use Tour Preview.
			</div>
			<asp:Image ID="TemplatePreviewImage" runat="server" />
		</asp:Panel>
	</div>
</asp:Content>

