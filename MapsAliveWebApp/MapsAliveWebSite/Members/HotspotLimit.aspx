<%@ Page 
    Language="C#" 
    MasterPageFile="~/Masters/MemberPage.master" 
    AutoEventWireup="true" 
    CodeFile="HotspotLimit.aspx.cs" 
    Inherits="Members_HotspotLimit" 
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ContentPlaceHolderID="ContentBody" Runat="Server">
	<div class="textNormal">
		<asp:Panel ID="AddHotspotAnywayPanel" runat="server" style="margin-top:16px;">
			<AvantLogic:MemberPageActionButton ID="AddHotspotAtLimitControl" runat="server" Title="I Understand. Add New Hotspot Anyway." />
		</asp:Panel>
		
		<div style="margin-top:4px;">
			<asp:Label ID="Suggestion" runat="server" />
		</div>
    </div>
</asp:Content>

