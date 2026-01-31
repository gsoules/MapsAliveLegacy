<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ErrorUnexpected.aspx.cs" Inherits="User_ErrorUnexpected" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>MapsAlive - Unexpected Error</title>
	<link href="../Styles/StyleSecure.css" rel="stylesheet" type="text/css" />
	<link rel="shortcut icon" type="image/x-icon" href="~/favicon.ico" />
</head>
<body class="secureErrorPageContent">
	<div class="securePageContent">
		<div class="secureBannerImage">
			<asp:Image ID="BannerImage" ImageUrl="../Images/MapsAlive-Logo-160px.jpg" runat="server" onclick="maTransferToPage('/Default.aspx');" style="cursor:pointer;" />
		</div>
		
		<div class="pageDivider"></div>
		<div class="pageTitle">Unexpected Error</div>
		<div class="pageDivider"></div>

		<div runat="server" style="margin-top:50px;">
			<div style="font-weight:bold;">
				<asp:Label ID="Title" runat="server" Text="Label">MapsAlive has encountered an unexpected problem</asp:Label>
			</div>
			
			<asp:Panel ID="PanelReturnToHomePage" runat="server" style="margin-top:16px;">
				<asp:HyperLink runat="server" NavigateUrl="~/Default.aspx"><b>Return to MapsAlive home page</b></asp:HyperLink><br />
			</asp:Panel>
			
			<br />
			<i>Please</i> report this problem <asp:HyperLink ID="EmailLink" runat="server">support@mapsalive.com</asp:HyperLink> to help us prevent it from happening again.
			<br />
			<br />
			<i>We apologize for this inconvenience.</i>
			<br />
			<br />
			<br />
			<asp:Label ID="ErrorId" runat="server" style="font-size:9px;" />
		</div>
	</div>
</body>
</html>
