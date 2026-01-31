<%@
	Control Language="C#"
	AutoEventWireup="true"
	CodeFile="TemplateSpacing.ascx.cs"
	Inherits="Controls_TemplateSpacing"
%>

<table class="optionsTable">
	<tr>
		<td class="optionsTableTitle" colspan="3">
			<asp:Label ID="MarginsTitle" runat="server" />
		</td>
	</tr>
	<tr>
		<td colspan="2" class="finePrintHelp" style="padding-left:3px;padding-bottom:4px;">
			<asp:Label ID="MarginsHelp" runat="server" />
		</td>
	</tr>
	<tr>
		<td valign="top">
			<table>
				<tr>
					<td align="Right">
						<asp:Label runat="server" CssClass="controlLabel" Text="<%$ Resources:Text, LayoutMarginTopLabel %>"></asp:Label>
					</td>
					<td>
						<asp:TextBox ID="MarginTop" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
						<asp:Label ID="MarginTopError" runat="server" CssClass="textErrorMessage" />
					</td>
					
					<td align="Right" style="padding-left:16px;">
						<asp:Label runat="server" CssClass="controlLabel" Text="<%$ Resources:Text, LayoutMarginRightLabel %>"></asp:Label>
					</td>
					<td>
						<asp:TextBox ID="MarginRight" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
						<asp:Label ID="MarginRightError" runat="server" CssClass="textErrorMessage" />
					</td>
					
					<td align="Right" style="padding-left:16px;">
						<asp:Label runat="server" CssClass="controlLabel" Text="<%$ Resources:Text, LayoutMarginBottomLabel %>"></asp:Label>
					</td>
					<td>
						<asp:TextBox ID="MarginBottom" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
						<asp:Label ID="MarginBottomError" runat="server" CssClass="textErrorMessage" />
					</td>
					
					<td align="Right" style="padding-left:16px;">
						<asp:Label runat="server" CssClass="controlLabel" Text="<%$ Resources:Text, LayoutMarginLeftLabel %>"></asp:Label>
					</td>
					<td>
						<asp:TextBox ID="MarginLeft" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
						<asp:Label ID="MarginLeftError" runat="server" CssClass="textErrorMessage" />
					</td>
				</tr>
			</table>
		</td>
	</tr>
</table>
