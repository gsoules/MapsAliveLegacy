<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/Secure.master"
	AutoEventWireup="true"
	CodeFile="RecoverPassword.aspx.cs"
	Inherits="User_RecoverPassword"
	Title="Retrieve MapsAlive Password"
%>

<%@ MasterType VirtualPath="~/Masters/Secure.master" %>

<asp:Content ContentPlaceHolderID="ContentBody" Runat="Server">
	<div class="securePageContent">
		<div class="pageDivider"></div>
		<div class="pageTitle">Recover Password</div>
		<div class="pageDivider"></div>

		<div class="textNormal formTable" style="margin-top:50px;">
			<div class="formTitleBar formTitleBarBlue"></div>
			<div class="formMainBox formMainBoxBlue">					
				<table cellpadding="0" cellspacing="0">
					<tr>
						<td colspan="2" class="textNormal" style="padding-bottom:12px;">
							<asp:Label ID="RetrievePasswordText" runat="server" Text="<%$ Resources:Text, RetrievePasswordText %>"/>
						</td>
					</tr>
					<tr>
						<td class="formLabelColumn formLabelColumnBlue">
							<asp:Label runat="server" Text="<%$ Resources:Text, EmailLabel %>" />
						</td>
						<td class="formFieldColumn">      
							<asp:TextBox ID="LoginEmail" runat="server"></asp:TextBox>
						</td>
					</tr>
					<tr>
						<td></td>
						<td style="padding-top:12px;padding-bottom:6px;">
							<asp:ImageButton ID="GetPasswordButton" runat="server" ImageUrl="../Images/BtnRecoverPW.gif" />
						</td>
					</tr>
				</table>
			</div>
			<div style="margin:10px 0px;" class="loginPanelErrorMessage">
				<asp:Label ID="Message" runat="server" />
			</div>
			<div style="textNormal">
				<span style="font-weight:normal;"><a href="Login.aspx">Return to Login Page</a></span>
			</div>
		</div>
	</div>

</asp:Content>
