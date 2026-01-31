<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ErrorConnectionDropped.aspx.cs" Inherits="User_ErrorConnectionDropped" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <title>MapsAlive - Connection Dropped</title>
	<link href="../Styles/StyleSecure.css" rel="stylesheet" type="text/css" />
	<link rel="shortcut icon" type="image/x-icon" href="~/favicon.ico" />
</head>
<body class="secureErrorPageContent">
	<div class="securePageContent">
		<div class="secureBannerImage">
			<asp:Image ID="BannerImage" ImageUrl="../Images/MapsAlive-Logo-160px.jpg" runat="server" onclick="maTransferToPage('/Default.aspx');" style="cursor:pointer;" />
		</div>
		
		<div class="pageDivider"></div>
		<div class="pageTitle">Connection Dropped</div>
		<div class="pageDivider"></div>

		<div runat="server" style="margin-top:50px;">
			<div style="font-weight:bold;">
				<asp:Label ID="Title" runat="server" Text="Label">Your connection to the MapsAlive server seems to have dropped.</asp:Label>
			</div>
			
			<div style="margin-top:12px;color:#555;line-height:18px;">
				This can happen if there is an interruption in the internet somewhere between your browser
				and our server. Please return to what you were doing and try again.
			</div>
			
			<div style="margin-top:24px;">
				<asp:HyperLink ID="TourBuilderLink" runat="server" Visible="false" ><b>Return to Tour Builder</b></asp:HyperLink>
			</div>
			<div style="margin-top:8px;margin-bottom:24px;">
				<asp:HyperLink runat="server" NavigateUrl="~/Default.aspx"><b>Return to MapsAlive home page</b></asp:HyperLink><br />
			</div>
			
			<div style="margin-top:12px;color:#555;line-height:18px;">
				If the problem persists, please contact <asp:HyperLink ID="EmailLink" runat="server">support@mapsalive.com</asp:HyperLink>.
			</div>
		</div>
	</div>
</body>
</html>
