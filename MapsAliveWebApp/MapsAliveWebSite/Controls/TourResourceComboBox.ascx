<%@ Control
	Language="C#"
	AutoEventWireup="true"
	CodeFile="TourResourceComboBox.ascx.cs"
	Inherits="Controls_TourResourceComboBox"
	ClassName="TourResourceComboBox"
%>

<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<div style="margin-top:2px;display:flex;align-items:center;">
	<telerik:RadComboBox 
		id="TourResourceComboBox" 
		Runat="server"
		Width="200px" 
		Height="400px" 
		DropDownWidth="480px" 
		AutoPostBack="False"
		Skin="Default"
		Style="margin-right:6px;"
		> 
		<ExpandAnimation Type="None" />
		<CollapseAnimation Type="None" />
	</telerik:RadComboBox>

	<img id="TourResourceComboBoxImage" runat="server" />
	
	<!--
	Logic in EmitClientSideIndexChangedHandler generates JavaScript that assumes that the
	img tag above is a sibling of the combo box (both nested inside a div). If you change
	this HTML, change that code too and test to make sure that when you select an image
	from the combox	box, the img gets updated immediately.
	-->
</div>
