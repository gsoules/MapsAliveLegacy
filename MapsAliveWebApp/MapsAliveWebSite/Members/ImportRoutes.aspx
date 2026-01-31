<%@
	Page Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="ImportRoutes.aspx.cs"
	Inherits="Members_ImportRoutes"
	EnableViewState="false"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<div class="textNormal">
		<uc:UploadFile Id="UploadFileControl" runat="server" />
		<uc:ImportReportTable Id="ImportReportTable" runat="server" />
		
		<asp:Panel ID="RoutesListPanel" runat="server" style="margin-top:16px;">
			<asp:Label ID="RoutesList" runat="server" />
		</asp:Panel>
		
		<asp:Panel ID="DeleteRoutesPanel" runat="server" style="margin-top:16px;">
			<AvantLogic:MemberPageActionButton ID="DeleteRoutesControl" Title="DeleteRoutes" runat="server" />
			<AvantLogic:QuickHelpTitle ID="DeleteRoutes" runat="server" Span="true" OffsetY="-176" />
		</asp:Panel>
	</div>
</asp:Content>

