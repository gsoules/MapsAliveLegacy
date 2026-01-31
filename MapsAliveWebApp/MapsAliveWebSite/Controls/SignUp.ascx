<%@ Control
	Language="C#"
	AutoEventWireup="true"
	CodeFile="SignUp.ascx.cs"
	Inherits="Controls_SignUp"
%>

	<table class="textNormal formTable" cellpadding="0" cellspacing="0">
	<tr>
		<td>
			<div>
				<div class="formTitleBar formTitleBarGreen">
					<asp:Label ID="TitleBar" runat="server" style="line-height:16px;" Text="Account Information"/>
				</div>
				
				<%-- Error Panel --%>
				<asp:Panel ID="ErrorPanel" runat="server" Visible="false" style="border:solid 1px green; padding:8px 8px 2px 8px;background-color:White;" >
					<div style="margin-top:2px;margin-bottom:8px;color:Red;"><asp:Label ID="ErrorText" runat="server"/></div>
				</asp:Panel>
			</div>
		</td>
	</tr>
	<tr>
		<td>
			<div class="formMainBox formMainBoxGreen">
				<table cellpadding="0" cellspacing="0" style="width: 100%;">
					<%-- Contact Name --%>
					<tr class="formRow">
						<td class="formLabelColumn formLabelColumnGreen">
							<asp:Label runat="server" Text="<%$ Resources:Text, ContactName %>"/></td>
						<td class="formFieldColumn">
							<asp:TextBox ID="ContactName" runat="server"></asp:TextBox><asp:Label ID="ContactNameError" CssClass="textErrorMessage" runat="server"/>
						</td>
					</tr>
					<%-- Email --%>
					<tr class="formRow">
						<td class="formLabelColumn formLabelColumnGreen">
							<asp:Label ID="EmailLabel" runat="server" Text="<%$ Resources:Text, EmailLabel %>"/></td>
						<td class="formFieldColumn">
							<asp:TextBox ID="Email" runat="server"></asp:TextBox><asp:Label ID="EmailError" CssClass="textErrorMessage" runat="server"/>
						</td>
					</tr>
					<%-- Password --%>
					<tr class="formRow">
						<td class="formLabelColumn formLabelColumnGreen">
							<asp:Label  runat="server" Text="<%$ Resources:Text, PasswordLabel %>"/></td>
						<td class="formFieldColumn">
							<asp:TextBox ID="Password" runat="server" autocomplete="off" TextMode="Password" MaxLength="16"></asp:TextBox><asp:Label ID="PasswordError" CssClass="textErrorMessage" runat="server"/>
						</td>
					</tr>
					<%-- Confirm Password --%>
					<tr class="formRow">
						<td class="formLabelColumn formLabelColumnGreen">
							<asp:Label runat="server" Text="<%$ Resources:Text, ConfirmPasswordLabel %>"/></td>
						<td class="formFieldColumn">
							<asp:TextBox ID="ConfirmPassword" runat="server" autocomplete="off" TextMode="Password" MaxLength="16"></asp:TextBox><asp:Label ID="ConfirmPasswordError" CssClass="textErrorMessage" runat="server"/>
						</td>
					</tr>
					<tr>
						<td class="finePrint" colspan="2" style="text-align:center;padding-bottom:8px;">
							<asp:Panel ID="NewsLetterPanel" runat="server" Visible="false">
								<asp:CheckBox ID="OkToSendEmail" runat="server" Checked="false" />
								<asp:Label runat="server" Text="<%$ Resources:Text, NewsletterOptIn %>"/>
							</asp:Panel>
							<asp:Panel ID="TermsPanel" runat="server">
								By clicking <%= ButtonName %> you agree to the
								<asp:HyperLink ID="TermsHyperLink" runat="server" NavigateUrl="javascript:maTransferToPage('/public/terms-of-service');">
									<asp:Label ID="TermsOfService" runat="server" Text="<%$ Resources:Text, TermsOfServiceLabel %>"/>
								</asp:HyperLink>.
							</asp:Panel>
						</td>
					</tr>
				</table>
			</div>
		</td>
	</tr>
</table>
