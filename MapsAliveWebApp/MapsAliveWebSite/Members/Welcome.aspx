<%@ Page 
    Language="C#" 
    MasterPageFile="~/Masters/MemberPage.master" 
    AutoEventWireup="true" 
    CodeFile="Welcome.aspx.cs" 
    Inherits="Members_Welcome" 
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<asp:Panel ID="MainPanel" runat="server" class="textNormal" style="width:400px;">
		<asp:Label ID="Instructions" runat="server" CssClass="textLarge" />
	</asp:Panel>

	<div style="margin-top:4px;">
        <img src="../Images/welcome.jpg" />
	</div>
</asp:Content>

