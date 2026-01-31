<%@
	Page Language="C#"
	MasterPageFile="~/Masters/Secure.master"
	AutoEventWireup="true"
	CodeFile="Login.aspx.cs"
	Inherits="User_Login"
	Title="MapsAlive Login"
%>

<%@ MasterType VirtualPath="~/Masters/Secure.master" %>

<asp:Content ContentPlaceHolderID="ContentBody" Runat="Server">
	<script type="text/javascript">
	var allowEnterKeyToPost = true;
	function maOnLogin()
	{
		var e = document.getElementById('<%= StatusLine.ClientID %>');
		e.innerHTML = "Please wait while we access your account...";
		return true;
	}
	</script>
	
	<div class="securePageContent">
		<div class="pageDivider"></div>
		<div class="pageTitle">Account Login</div>
		<div class="pageDivider"></div>
			<div class="formTitleBar formTitleBarBlue"></div>
			<div class="formMainBox formMainBoxBlue">
				<table cellpadding="0" cellspacing="0" style="width: 100%;">
					<tr style="height:36px;">
						<td class="formLabelColumn formLabelColumnBlue">
							<asp:Label ID="LoginEmailLabel" runat="server" Text="<%$ Resources:Text, EmailLabel %>" />
						</td>
						<td class="formFieldColumn">      
							<asp:TextBox ID="LoginEmail" runat="server"></asp:TextBox>
						</td>
					</tr>
					<tr style="height:36px;">
						<td class="formLabelColumn formLabelColumnBlue">
							<asp:Label runat="server" Text="<%$ Resources:Text, PasswordLabel %>" />
						</td>
						<td class="formFieldColumn">
							<asp:TextBox ID="LoginPassword" runat="server" TextMode="Password"></asp:TextBox>
						</td>
					</tr>
					<tr>
						<td />
						<td>
							<asp:Label ID="RecoverPassword" runat="server" CssClass="finePrint" style="margin-left:8px;">
								<asp:LinkButton runat="server" PostBackUrl="~/User/Login.aspx?login=0" OnClick="OnRetrievePassword">I forgot my password</asp:LinkButton>
							</asp:Label>
						</td>
					</tr>
					<tr>
						<td class="formLabelColumn formLabelColumnBlue"></td>
						<td>
							<asp:Label ID="LoginFailed" runat="server" Visible="false" CssClass="loginPanelErrorMessage"/>
						</td>
					</tr>
					<tr style="height:30px;">
						<td class="formLabelColumnBlue"></td>
						<td style="padding:8px 0px 4px 0px;">
							<asp:CheckBox ID="RememberMe" CssClass="finePrint" runat="server" Checked="true" Text="<%$ Resources:Text, RememberMeLabel %>" />
						</td>
					</tr>
					<tr style="height:36px;">
						<td class="formLabelColumn formLabelColumnBlue"></td>
						<td>
							<asp:ImageButton ID="LoginButton" runat="server" ImageUrl="../Images/BtnLogin.gif" OnClientClick="return maOnLogin();" />
							<div style="margin-top:8px;font-weight:normal">
								<asp:LinkButton ID="GoHomeButton" runat="server" PostBackUrl="~/User/Login.aspx?login=0" OnClick="OnGoHome">Go back to MapsAlive home page</asp:LinkButton>
							</div>
						</td>
					</tr>
					<tr>
						<td class="formLabelColumn formLabelColumnBlue"></td>
						<td style="padding-top:4px;">
							<asp:Label ID="StatusLine" runat="server" style="color:#e69311;font-size:14px;font-weight:bold;" Text="&nbsp;"/>
						</td>
					</tr>
				</table>
			</div>
		</div>
	</div>

</asp:Content>
