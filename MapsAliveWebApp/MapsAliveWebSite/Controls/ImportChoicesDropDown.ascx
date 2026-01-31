<%@ Control
	Language="C#"
	AutoEventWireup="true"
	CodeFile="ImportChoicesDropDown.ascx.cs"
	Inherits="Controls_ImportChoicesDropDown"
%>

<div style="margin-bottom:12px;">
	<AvantLogic:QuickHelpTitle ID="ImportChoices" runat="server" Title="<%$ Resources:Text, ImportChoicesLabel %>" OffsetX="180" OffsetY="-8" TopMargin="0px" />
	<asp:DropDownList ID="DropDownList" runat="server" onchange="maOnEventImportSlides(this);" />
</div>

