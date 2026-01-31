<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="TourExplorer.aspx.cs"
	Inherits="Members_TourExplorer"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<%@ Register Assembly="App_Code\ServerControls\TourSelector.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentBody" Runat="Server">
	<asp:Panel ID="SortPanel" runat="server" Visible="false" style="margin-bottom:8px;">
		Sort by: 
		<AvantLogic:MemberPageActionButton ID="SortToursByNameControl" Title="Name" runat="server" />
		&nbsp;
		<AvantLogic:MemberPageActionButton ID="SortToursByIdControl" Title="Tour #" runat="server" />
		&nbsp;
		<AvantLogic:MemberPageActionButton ID="SortToursByDateControl" Title="Date Built" runat="server" />
		<AvantLogic:QuickHelpTitle ID="TourExplorerSort" runat="server" Span="true" />
	</asp:Panel>
	
	<asp:Panel ID="ToursPanel" runat="server">
		<AvantLogic:PageThumbs ID="PageThumbs" runat="server" ColumnsPerRow="5" Dimension="124" />
	</asp:Panel>
</asp:Content>

