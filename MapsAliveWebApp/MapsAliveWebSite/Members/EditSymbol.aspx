<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="EditSymbol.aspx.cs"
	Inherits="Members_EditSymbol"
	Trace="false"
	TraceMode="SortByCategory"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>
<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<AvantLogic:QuickHelpTitle ID="SymbolName" runat="server" Title="<%$ Resources:Text, SymbolNameLabel %>" TopMargin="0px" Span="true" />
	<AvantLogic:MemberPageActionButton ID="ShowUsageControl" runat="server" />
	<br />
	<asp:TextBox ID="SymbolNameTextBox" Width="200" runat="server"></asp:TextBox>
	<asp:Label ID="SymbolNameError" runat="server" CssClass="textErrorMessage" />

	<div class="optionsSectionTitle">Upload Image</div>

	<AvantLogic:QuickHelpTitle ID="SymbolImage" runat="server" Title="<%$ Resources:Text, SymbolImageLabel %>" />
							
	<input type="file" runat="server" id="FileInput" style="height:22px;" onkeypress="return maStopEvent(event);"/>
	<asp:Button runat="server" Enabled="true" Text="<%$ Resources:Text, ButtonUpload %>" OnClientClick="maOnEventUploadFile();return false;" CausesValidation="false" Height="22px" />

	<telerik:RadProgressManager id="RadProgressManager" runat="server" />
	<telerik:RadProgressArea id="ProgressArea" runat="server" />

	<div style="margin-top:4px; height:24px; overflow:hidden;">
		<asp:Label ID="FileName" runat="server" CssClass="textUploadFileName"></asp:Label>
	</div>
	
	<asp:Panel ID="ImageDiv" runat="server" CssClass="uploadedImage">
		<div class="optionsSectionTitle">Preview</div>
		<asp:Image ID="ImageElement" runat="server" />
	</asp:Panel>
</asp:Content>
