<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/Secure.master"
	AutoEventWireup="true"
	CodeFile="ForcedLogout.aspx.cs"
	Inherits="User_ForcedLogout"
	Title="Logged Out"
%>
    
<%@ MasterType VirtualPath="~/Masters/Secure.master" %>

<asp:Content ContentPlaceHolderID="ContentBody" Runat="Server">
<div class="securePageContent">
	<div class="pageDivider"></div>
	<div class="pageTitle">Logged Out</div>
	<div class="pageDivider"></div>

	<div style="margin-top: 50px;">
		<asp:Label ID="Message" class="textNormal" runat="server" />
	</div>
	<div class="textNormal" style="margin-top:12px;">
		<span><a href="Login.aspx">Return to Login Page</a></span>
	</div>
</div>
</asp:Content>
