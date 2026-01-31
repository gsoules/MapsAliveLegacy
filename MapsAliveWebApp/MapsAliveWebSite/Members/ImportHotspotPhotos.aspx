<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="ImportHotspotPhotos.aspx.cs"
	Inherits="Members_ImportHotspotPhotos"
	Title=""
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<asp:Panel ID="AllPanels" runat="server" class="textNormal">
		<table cellpadding="0" cellspacing="0">
			<tr>
				<td style="width:400px;">
					<uc:UploadFile Id="UploadFileControl" runat="server" />
				</td>
			</tr>
		</table>

		<div class="optionsSectionTitle">Marker For New Hotspots</div>
		
		<AvantLogic:QuickHelpTitle ID="ImportedPhotoMarker" runat="server" Title="Marker" TopMargin="4px" />
		<uc:TourResourceComboBox Id="MarkerComboBox" runat="server" />
		
		<uc:ImportReportTable Id="ImportReportTable" runat="server" />
	</asp:Panel>
</asp:Content>

