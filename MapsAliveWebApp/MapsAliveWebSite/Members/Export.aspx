<%@ Page 
    Language="C#" 
    MasterPageFile="~/Masters/MemberPage.master" 
    AutoEventWireup="true" 
    CodeFile="Export.aspx.cs" 
    Inherits="Members_Export" 
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<div style="padding-bottom:8px;">
		<div class="optionsSectionTitle">Download Tour</div>
		<div id="ExportPublishedTourHelp" runat="server" class="textHelp"/>
		<AvantLogic:MemberPageActionButton ID="ExportPublishedTourControl" runat="server" Title="Download Published Tour" />
		
		<div class="optionsSectionTitle">Archive Tour to Zip file</div>
		<div id="ExportArchiveHelp" runat="server" class="textHelp"/>
		<AvantLogic:MemberPageActionButton ID="ExportArchiveControl" runat="server" Title="Archive Tour" />
		
		<div class="optionsSectionTitle">Export Tour Content to CSV (Excel) file</div>
		<div id="ExportContentCsvHelp" runat="server" class="textHelp"/>
		<AvantLogic:MemberPageActionButton ID="ExportContentCsvControl" runat="server" Title="Export Tour Content to CSV" />
		
		<div class="optionsSectionTitle">Export Tour Content to XML file</div>
		<div id="ExportContentXmlHelp" runat="server" class="textHelp"/>
		<AvantLogic:MemberPageActionButton ID="ExportContentXmlControl" runat="server" Title="Export Tour Content to XML" />
		
		<div class="optionsSectionTitle">Export Resources to Zip file</div>
		<div id="ExportResourcesHelp" runat="server" class="textHelp"/>
		<AvantLogic:MemberPageActionButton ID="ExportResourcesControl" runat="server" Title="Export Tour Resources" />
		<AvantLogic:MemberPageActionButton ID="ExportAllResourcesControl" runat="server" Title="Export Account Resources" />
		
		<asp:Panel ID="ExportImagesPanel" runat="server" style="background-color:#e7f0f6;padding-bottom:8px;">
			<div class="optionsSectionTitle">Export Tour Images to Zip file <span class="finePrint" style="font-weight:normal;">[only available to system administrator]</span></div>
			<div id="ExportImagesHelp" runat="server" class="textHelp"/>
			<AvantLogic:MemberPageActionButton ID="ExportImagesControl" runat="server" Title="Export Tour Images" />
		</asp:Panel>
	</div>
</asp:Content>

