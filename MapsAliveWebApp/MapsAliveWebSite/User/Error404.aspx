<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/Secure.master"
	AutoEventWireup="true"
	CodeFile="Error404.aspx.cs"
	Inherits="User_Error404"
	Title="MapsAlive - Error 404"
%>
    
<%@ MasterType VirtualPath="~/Masters/Secure.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentBody" Runat="Server">
	<div class="securePageContent">
		
		<div class="pageDivider"></div>

		<div class="textNormal" style="margin-left:0px;margin-top:50px;">
			The requested URL <b><asp:Label ID="UrlName" runat="server" /></b> was not found on this server.
			<br />
			<br />
			If your URL is correct, please contact <asp:HyperLink ID="EmailLink" runat="server">support@mapsalive.com</asp:HyperLink>.
		</div>
	</div>
</asp:Content>

