<%@ Page Language="C#" 
    AutoEventWireup="true" 
    CodeFile="Default.aspx.cs"
    Inherits="Default" 
    EnableViewState="false"
%>
<!DOCTYPE html>
<html lang="en" >
	<head runat="server">
		<title>MapsAlive Configuration Status</title>
		<link href="Styles/Style.css" rel="stylesheet" type="text/css" />
   </head>
	<body>
		<form runat="server">
			<div class="bannerTitleConfigStatus">MapsAlive Configuration Status</div>
			
			<asp:Panel ID="StatusPanel" runat="server" Visible="false">
				<table style="border-spacing:6px; width:760px;">
					<tr>
						<td class="config">
							<asp:Label ID="HomeLink" runat="server">
								<asp:LinkButton runat="server" OnClick="OnHome">Home</asp:LinkButton>&nbsp;
							</asp:Label>
						</td>
						<td>
							<asp:LinkButton runat="server" OnClick="OnRestart">Restart</asp:LinkButton>&nbsp;
							<asp:Label ID="RestartError" runat="server" />
						</td>
					</tr>
					<tr>
						<td class="configSpacer" colspan="2" />
					</tr>
					<tr>
						<td class="configSection">Status:</td>
						<td><asp:Label ID="ConfigStatus" runat="server" /></td>
					</tr>
					<tr>
						<td class="config">Time Stamp:</td>
						<td><asp:Label ID="TimeStamp" runat="server" /></td>
					</tr>
					<tr>
						<td class="config">Current User:</td>
						<td><asp:Label ID="UserName" runat="server" /></td>
					</tr>
					<tr>
						<td class="config">MapsAlive:</td>
						<td><asp:Label ID="Version" runat="server" /></td>
					</tr>
					<tr>
						<td class="config">Developer Mode:</td>
						<td><asp:Label ID="DeveloperMode" runat="server" /></td>
					</tr>
					<tr>
						<td class="configSpacer" colspan="2" />
					</tr>
					<tr>
						<td class="configSection">Locations:</td>
						<td><asp:Label ID="LocationsStatus" runat="server" /></td>
					</tr>
					<tr>
						<td class="config">tourDir:</td>
						<td><asp:Label ID="TourDir" runat="server" /></td>
					</tr>
					<tr>
						<td class="config">samplesDir:</td>
						<td><asp:Label ID="SamplesDir" runat="server" /></td>
					</tr>
					<tr>
						<td class="config">runtimeDir:</td>
						<td><asp:Label ID="RuntimeDir" runat="server" /></td>
					</tr>
					<tr>
						<td class="config">runtimeUrl:</td>
						<td><asp:Label ID="RuntimeUrl" runat="server" /></td>
					</tr>
					<tr>
						<td class="config">siteUrl:</td>
						<td><asp:Label ID="SiteUrl" runat="server" /></td>
					</tr>
					<tr>
						<td class="config">tourUrl:</td>
						<td><asp:Label ID="TourUrl" runat="server" /></td>
					</tr>
					<tr>
						<td class="configSpacer" colspan="2" />
					</tr>
					<tr>
						<td class="configSection">DB Connection:</td>
						<td><asp:Label ID="DatabaseStatus" runat="server" /></td>
					</tr>
					<tr>
						<td class="config">Server:</td>
						<td><asp:Label ID="DatabaseServer" runat="server" /></td>
					</tr>
					<tr>
						<td class="config">Login:</td>
						<td><asp:Label ID="DatabaseServerLogin" runat="server" /></td>
					</tr>
					<tr>
						<td class="config">Database:</td>
						<td><asp:Label ID="Database" runat="server" /></td>
					</tr>
					<tr>
						<td class="config">Version:</td>
						<td><asp:Label ID="DatabaseVersion" runat="server" /></td>
					</tr>
					<tr>
						<td class="configSpacer" colspan="2" />
					</tr>
					<tr>
						<td class="config"></td>
						<td>
							<asp:LinkButton runat="server" ID="TestSmtp" OnClick="OnTestSmtp">Send test email to support</asp:LinkButton>&nbsp;
						</td>
					</tr>
					<tr>
						<td class="configSection">SMTP:</td>
						<td><asp:Label ID="SmtpStatus" runat="server" /></td>
					</tr>
					<tr>
						<td class="config">Support Email:</td>
						<td><asp:Label ID="SysAdminEmail" runat="server" /></td>
					</tr>
					<tr>
						<td class="config">Host:</td>
						<td><asp:Label ID="SmtpHost" runat="server" /></td>
					</tr>
					<tr>
						<td class="config">Port:</td>
						<td><asp:Label ID="SmtpPort" runat="server" /></td>
					</tr>
					<tr>
						<td class="config">SMTP User:</td>
						<td><asp:Label ID="SmtpUserName" runat="server" /></td>
					</tr>
					<tr>
						<td class="configSpacer" colspan="2" />
					</tr>
				</table>
				<div style="color:gray;margin-left:6px;margin-top:8px;font-size:10px;">
					<asp:Label ID="StatusFlags" runat="server" />
				</div>
			</asp:Panel>
		</form>
	</body>
</html>
