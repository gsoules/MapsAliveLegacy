<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="EditTooltipStyle.aspx.cs"
	Inherits="Members_EditTooltipStyle"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
    <script type="text/javascript">
        var lineThickness;
        var lineColor;
        var maxWidth;
        var textColor;
        var backgroundColor;
        var padding;
        var fontStyle;
		var fontStyleTable = {<%= FontStyleTable %>};
		function maChangeLineThickness(sender, eventArgs)
		{
			maChangeDetected();
			var e = document.getElementById("<%= LineWidthValue.ClientID %>");
			var value = sender.get_value();
			e.innerHTML = value + " px";
			lineThickness = value;
			maUpdatePreview();
		}
		function maChangePadding(sender, eventArgs)
		{
			maChangeDetected();
			var e = document.getElementById("<%= PaddingValue.ClientID %>");
			var value = sender.get_value();
			e.innerHTML = value + " px";
			padding = value;
			maUpdatePreview();
		}
		function maChangeMaxWidth(sender, eventArgs)
		{
			maChangeDetected();
			var e = document.getElementById("<%= MaxWidthValue.ClientID %>");
			var value = sender.get_value();
			e.innerHTML = value + " px";
			maxWidth = value;
			maUpdatePreview();
		}
		function maOnColorChanged(swatchId, color)
		{
			switch(swatchId)
			{
				case "<%= LineColorSwatch.ClientID %>_Swatch":
					lineColor = color;
					break;
				
				case "<%= BackgroundColorSwatch.ClientID %>_Swatch":
					backgroundColor = color;
					break;
				
				case "<%= TextColorSwatch.ClientID %>_Swatch":
					textColor = color;
					break;
			}
			maUpdatePreview();
		}
		function maBackgroundTransparencyChanged(e)
		{
			maChangeDetected();
			backgroundColor = e.checked ? "transparent" : document.getElementById("<%= BackgroundColorSwatch.ClientID %>_Swatch").style.backgroundColor;
			maUpdatePreview();
		}
		function maFontStyleChanged(sender, eventArgs)
		{
			maChangeDetected();
			maTourResourceSelectionChanged("<%= FontStyleComboBox.ClientID %>", "<%= EditFontStyleControl.ClientID %>", "EditFontStyle.aspx");
			var id = sender.get_value();
			fontStyle = fontStyleTable["s" + id];
			maUpdatePreview();
		}
		function maInitTooltipStylePreview(lt, lc, bc, tc, p, fs, mw)
		{
			lineThickness = lt;
			lineColor = lc;
			backgroundColor = bc;
			textColor = tc;
			padding = p;
			fontStyle = fontStyleTable["s" + fs];
			maxWidth = mw;
			maUpdatePreview();
		}
		function maUpdatePreview()
		{
			try
			{
				var preview = document.getElementById("preview");
				preview.innerHTML = document.getElementById("<%= TooltipStyleNameTextBox.ClientID %>").value;
				var style = preview.style;
				style.border = "solid " + lineThickness + "px " + lineColor;
				style.backgroundColor = backgroundColor;
				style.color = textColor;
				style.padding = padding + "px";
				style.fontFamily = fontStyle[0];
				style.fontSize = fontStyle[1] + "px";
				style.fontWeight = fontStyle[2] ? "bold" : "normal";
				style.fontStyle = fontStyle[3] ? "italic" : "normal";
				style.textDecoration = fontStyle[4] ? "underline" : "none";
                style.width = maxWidth + "px";
                style.lineHeight = fontStyle[1] + "px";
			}
			catch (error)
			{
				// Ignore attempts to assign invalid colors.
			}
		}
    </script>
	
	<AvantLogic:QuickHelpTitle ID="TooltipStyleName" runat="server" Title="Tooltip Style Name" TopMargin="0px" Span="true" />
	<AvantLogic:MemberPageActionButton ID="ShowUsageControl" runat="server" />
	<br />
	<asp:TextBox ID="TooltipStyleNameTextBox" runat="server" Width="200" />
	<asp:Label ID="TooltipStyleNameError" runat="server" CssClass="textErrorMessage" />

	<div class="optionsSectionTitle">Colors</div>
	<table>
		<AvantLogic:ColorSwatch Id="TextColorSwatch" Label="Text Color" QuickHelpTitle="TooltipStyleTextColor" Row="true" runat="server" />
		<AvantLogic:ColorSwatch Id="LineColorSwatch" Label="Line Color" QuickHelpTitle="TooltipStyleLineColor" Row="true" runat="server" />
		<tr>
			<td style="text-align:right;">
				<asp:Label runat="server" CssClass="controlLabel" Text="Background Color"/>
			</td>
			<td>
				<AvantLogic:ColorSwatch Id="BackgroundColorSwatch" runat="server" />
				<AvantLogic:QuickHelpTitle ID="TooltipStyleBackgroundColor" runat="server" Span="true" />
				&nbsp;&nbsp;
				<asp:CheckBox ID="TransparentBackgroundCheckBox" runat="server" />
				<AvantLogic:QuickHelpTitle ID="TooltipTransparentBackground" runat="server" Title="Make Background Transparent" Span="true" />
			</td>
		</tr>
	</table>
	
	<div class="optionsSectionTitle">Options</div>
		
	<div style="margin-top:12px;margin-bottom:2px;">
		<AvantLogic:QuickHelpTitle ID="TooltipFont" runat="server" Title="Font" Span="true" />
	</div>
	<table cellpadding="0" cellspacing="0">
		<tr>
			<td>
				<uc:TourResourceComboBox Id="FontStyleComboBox" runat="server" />
			</td>
			<td style="padding-left:4px;">
				<AvantLogic:MemberPageActionButton Subtle="true" ID="EditFontStyleControl" runat="server" />
			</td>
		</tr>
	</table>

	<AvantLogic:QuickHelpTitle ID="TooltipLineWidth" runat="server" Title="Line Thickness" />
	<table>
	    <tr>
	        <td>
	            <telerik:RadSlider
	                ID="SliderLineWidth"
	                runat="server"
	                Width="100"
	                MinimumValue="0"
                    MaximumValue="16"
                    Value="1"
                    OnClientValueChanged="maChangeLineThickness"
                    Skin="Default"
                    DragText=""
                  />
	        </td>
	        <td>
                <asp:Label ID="LineWidthValue" runat="server" CssClass="unit"/>
	        </td>
	    </tr>
	</table>
	
	<AvantLogic:QuickHelpTitle ID="TooltipPadding" runat="server" Title="Padding" />
	<table>
	    <tr>
	        <td>
	            <telerik:RadSlider
	                ID="SliderPadding"
	                runat="server"
	                Width="100"
	                MinimumValue="0"
                    MaximumValue="16"
                    Value="1"
                    OnClientValueChanged="maChangePadding"
                    Skin="Default"
                    DragText=""
                  />
	        </td>
	        <td>
                <asp:Label ID="PaddingValue" runat="server" CssClass="unit"/>
	        </td>
	    </tr>
	</table>
		
	<AvantLogic:QuickHelpTitle ID="TooltipMaxWidth" runat="server" Title="Max Width" />
	<table>
	    <tr>
	        <td>
	            <telerik:RadSlider
	                ID="SliderMaxWidth"
	                runat="server"
	                Width="400"
	                MinimumValue="100"
                    MaximumValue="800"
                    Value="1"
                    OnClientValueChanged="maChangeMaxWidth"
                    Skin="Default"
                    DragText=""
                  />
	        </td>
	        <td>
                <asp:Label ID="MaxWidthValue" runat="server" CssClass="unit"/>
	        </td>
	    </tr>
	</table>

	<div id="PreviewMessage" class="optionsSectionTitle">Preview</div>
	<div style="position:relative;">
		<div id="preview" style="position:absolute;">Preview</div>
	</div>
</asp:Content>
