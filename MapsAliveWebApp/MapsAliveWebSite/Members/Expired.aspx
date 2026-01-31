<%@ Page 
    Language="C#" 
    MasterPageFile="~/Masters/MemberPage.master" 
    AutoEventWireup="true" 
    CodeFile="Expired.aspx.cs" 
    Inherits="Members_Expired" 
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentBody" Runat="Server">
	<div class="textNormal">
		<asp:Label ID="PageText" runat="server" />
    </div>
</asp:Content>

