<%@ Page 
    Language="C#" 
    MasterPageFile="~/Masters/MemberPage.master" 
    AutoEventWireup="true" 
    CodeFile="SessionExpired.aspx.cs" 
    Inherits="Members_SessionExpired" 
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<div class="textNormal">
		Your MapsAlive session expired.
		<br /><br />
		You can resume working by choosing from the menu or clicking in the Tour Navigator.
	</div>
</asp:Content>

