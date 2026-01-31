<%@
	Control
	Language="C#"
	AutoEventWireup="true"
	CodeFile="MarginsAndSpacing.ascx.cs"
	Inherits="Controls_MarginsAndSpacing"
%>

<div style="position:relative;">
	<AvantLogic:QuickHelpTitle ID="MarginsForCanvas" runat="server" Title="Margins" TopMargin="0px" />

	<div style="padding-left:12px;">
		<table>
			<tr>
				<td align="Right">
					<asp:Label runat="server" CssClass="controlLabelNested" Text="<%$ Resources:Text, LayoutMarginTopLabel %>"></asp:Label>
				</td>
				<td>
					<asp:TextBox ID="_MarginTop" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
					<asp:Label ID="_MarginTopError" runat="server" CssClass="textErrorMessage" />
				</td>
				
				<td align="Right" style="padding-left:16px;">
					<asp:Label runat="server" CssClass="controlLabelNested" Text="<%$ Resources:Text, LayoutMarginRightLabel %>"></asp:Label>
				</td>
				<td>
					<asp:TextBox ID="_MarginRight" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
					<asp:Label ID="_MarginRightError" runat="server" CssClass="textErrorMessage" />
				</td>
				
				<td align="Right" style="padding-left:16px;">
					<asp:Label runat="server" CssClass="controlLabelNested" Text="<%$ Resources:Text, LayoutMarginBottomLabel %>"></asp:Label>
				</td>
				<td>
					<asp:TextBox ID="_MarginBottom" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
					<asp:Label ID="_MarginBottomError" runat="server" CssClass="textErrorMessage" />
				</td>
				
				<td align="Right" style="padding-left:16px;">
					<asp:Label runat="server" CssClass="controlLabelNested" Text="<%$ Resources:Text, LayoutMarginLeftLabel %>"></asp:Label>
				</td>
				<td>
					<asp:TextBox ID="_MarginLeft" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
					<asp:Label ID="_MarginLeftError" runat="server" CssClass="textErrorMessage" />
				</td>
			</tr>
		</table>
	</div>

	<asp:Panel ID="_SpacingPanel" runat="server" CssClass="spacingPanelBelow">
		<AvantLogic:QuickHelpTitle ID="LayoutSpacing" runat="server" Title="Spacing" TopMargin="0px" />
		<div style="padding-left:12px;">
			<asp:Panel ID="_SpacingYesPanel" runat="server" Visible="true">
				<table cellpadding="1" cellpadding="1">
					<tr>
						<td ID="_SpacingCellH1" runat="server" align="Right">
							<asp:Label runat="server" CssClass="controlLabelNested" Text="<%$ Resources:Text, HorizontalSpacingLabel %>"></asp:Label>
						</td>
						<td ID="_SpacingCellH2" runat="server">
							<asp:TextBox ID="_SpacingH" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
							<asp:Label ID="_SpacingHError" runat="server" CssClass="textErrorMessage" />
						</td>
						<td ID="_SpacingCellV1" runat="server" align="Right" style="padding-left:16px;">
							<asp:Label runat="server" CssClass="controlLabelNested" Text="<%$ Resources:Text, VerticalSpacingLabel %>"></asp:Label>
						</td>
						<td ID="_SpacingCellV2" runat="server">
							<asp:TextBox ID="_SpacingV" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
							<asp:Label ID="_SpacingVError" runat="server" CssClass="textErrorMessage" />
						</td>
					</tr>
				</table>
			</asp:Panel>
			<asp:Panel ID="_SpacingNoPanel" runat="server" CssClass="finePrintHelp" Visible="false">
				The selected template has only one area. Spacing options only appear for templates with 2 or 3 areas.
			</asp:Panel>
		</div>
	</asp:Panel>
</div>
