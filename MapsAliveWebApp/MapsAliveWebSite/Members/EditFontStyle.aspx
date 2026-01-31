<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="EditFontStyle.aspx.cs"
	Inherits="Members_EditFontStyle"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
    <script type="text/javascript">
	var familyArray = [<%= FontFamilyArray %>];
	function maShowPreview(size, familyId, bold, italic, underline)
	{
		var preview = document.getElementById("SampleText");
		preview.innerHTML = document.getElementById("<%= FontStyleNameTextBox.ClientID %>").value;
		var style = preview.style;
		style.fontSize =  size + "px";
		style.lineHeight = style.fontSize;
		style.fontWeight = bold ? "bold" : "normal";
		style.fontStyle = italic ? "italic" : "normal";
		style.textDecoration = underline ? "underline" : "none";
		var family = familyArray[familyId - 1];
		style.fontFamily = family;
	}
	function maUpdatePreview(sender, eventArgs)
	{
		maChangeDetected();
		var fontSize = $find("<%= SliderFontSize.ClientID %>").get_value();
		document.getElementById("<%= FontSizeValue.ClientID %>").innerHTML = fontSize + " px";
		var fontFamilyId = parseInt($find("<%= FontFamilyComboBox.ClientID %>").get_value(), 10);
		var bold = document.getElementById("<%= BoldCheckBox.ClientID %>").checked;
		var italic = document.getElementById("<%= ItalicCheckBox.ClientID %>").checked;
		var underline = document.getElementById("<%= UnderlineCheckBox.ClientID %>").checked;
		maShowPreview(fontSize, fontFamilyId, bold, italic, underline);
	}
	</script>
	
	<AvantLogic:QuickHelpTitle ID="FontStyleName" runat="server" Title="<%$ Resources:Text, FontStyleNameLabel %>" TopMargin="0px" Span="true" />
	<AvantLogic:MemberPageActionButton ID="ShowUsageControl" runat="server" />
	<br />
	<asp:TextBox ID="FontStyleNameTextBox" runat="server" Width="200" />
	<asp:Label ID="FontStyleNameError" runat="server" CssClass="textErrorMessage" />
	
	<div class="optionsSectionTitle"">Options</div>

	<AvantLogic:QuickHelpTitle ID="FontStyleFontFamily" runat="server" Title="Font Family" />
	<telerik:RadComboBox 
		id="FontFamilyComboBox" 
		Runat="server"
		AutoPostBack="false"
		Skin="Default"
		OnClientSelectedIndexChanged="maUpdatePreview"
		>
		<ExpandAnimation Type="None" />
		<CollapseAnimation Type="None" />
	</telerik:RadComboBox>
	
	<AvantLogic:QuickHelpTitle ID="FontStyleFontSize" runat="server" Title="Font Size" />
	<table>
	    <tr>
	        <td>
	            <telerik:RadSlider
	                ID="SliderFontSize"
	                runat="server"
	                Width="200"
	                MinimumValue="8"
                    MaximumValue="144"
                    OnClientValueChanged="maUpdatePreview"
                    Skin="Default"
                    DragText=""
                  />
	        </td>
	        <td>
                <asp:Label ID="FontSizeValue" runat="server" CssClass="unit"/>
	        </td>
	    </tr>
	</table>
	
	<AvantLogic:QuickHelpTitle ID="FontStyleBoldItalic" runat="server" Title="Font Weight and Style" />
	<div style="margin-bottom:16px;">
		<asp:CheckBox ID="BoldCheckBox" runat="server" Text="Bold" CssClass="controlLabel" />
		&nbsp;&nbsp;&nbsp;
		<asp:CheckBox ID="ItalicCheckBox" runat="server" Text="Italic" CssClass="controlLabel" />
		&nbsp;&nbsp;&nbsp;
		<asp:CheckBox ID="UnderlineCheckBox" runat="server" Text="Underline" CssClass="controlLabel" />
	</div>

	<div id="PreviewMessage" class="optionsSectionTitle">Preview</div>
	<div id="SampleText"></div>
</asp:Content>
