<%@ Control
	Language="C#"
	AutoEventWireup="true"
	CodeFile="UploadFile.ascx.cs"
	Inherits="Controls_UploadFile"
%>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>

<AvantLogic:QuickHelpTitle ID="QuickHelp" runat="server" />

<style type="text/css">
.importStep 
{ 		
	font-size: 10px; 
	font-family:Arial, Helvetica, Verdana, Sans-Serif;
	margin-bottom:8px;
}
</style>
<div style="margin-top:4px;margin-bottom:8px;padding:8px;border:solid 1px darkgray;width:358px;overflow:hidden;">
	<asp:Panel ID="ExtraStep1" CssClass="importStep" runat="server" Visible="false">
		<b>1. </b>
		<asp:Label ID="ExtraStep1Description" runat="server" />
	</asp:Panel>
	<asp:Panel ID="ExtraStep2" CssClass="importStep" runat="server" Visible="false">
		<b>2. </b>
		<asp:Label ID="ExtraStep2Description" runat="server" />
	</asp:Panel>
	<div class="importStep">
		<b><asp:Label ID="SelectFileStepNumber" runat="server" />. </b>
		<asp:Label ID="SelectFileStepDescription" runat="server" />
	</div>
	<input type="file" runat="server" id="FileInput" style="height:22px;" onkeypress="return maStopEvent(event);"/>
	<div style="margin-bottom:8px;">
		<asp:Label ID="FileName" runat="server" style="color:Green;font-size:9px;" />
	</div>
	<div class="importStep">
		<b><asp:Label ID="UploadStepNumber" runat="server" />. </b>
		<asp:Label ID="UploadStepDescription" runat="server" />
	</div>
	<asp:Button ID="ButtonUpload" runat="server" Enabled="true" OnClientClick="maOnEventUploadFile();return false;" CausesValidation="false" Height="22px" />
</div>

<telerik:RadProgressManager id="RadProgressManager" runat="server" />
<telerik:RadProgressArea id="UploadProgressArea" runat="server" />

