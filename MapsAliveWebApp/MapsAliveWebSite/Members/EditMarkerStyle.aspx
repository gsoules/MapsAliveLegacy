<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="EditMarkerStyle.aspx.cs"
	Inherits="Members_EditMarkerStyle"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
    <script type="text/javascript">
        var normalFill;
        var selectedFill;
        var normalLine;
        var selectedLine;
		function maGetPreview()
		{
			normalFill = document.getElementById("previewNormalFill").style;
			selectedFill = document.getElementById("previewSelectedFill").style;
			normalLine = document.getElementById("previewNormalLine").style;
			selectedLine = document.getElementById("previewSelectedLine").style;
		}
		function maChangeLineThickness(sender, eventArgs)
		{
			maChangeDetected();
			maGetPreview();
			var e = document.getElementById("<%= LineWidthValue.ClientID %>");
			var value = sender.get_value();
			e.innerHTML = value + " px";
			maSetLineThickness(normalLine, normalFill, value);
			maSetLineThickness(selectedLine, selectedFill, value);
		}
		function maSetLineThickness(line, fill, value)
		{
			line.borderWidth = value + "px " + value + "px " + value + "px " + value + "px";
			line.width = (80 - (value * 2)) + "px";
			line.height = (80 - (value * 2)) + "px";
			line.top = "8px";
			line.left = "20px";

			fill.top = ((value / 2) + 8) + "px";
			fill.left = ((value / 2) + 20) + "px";
			fill.width = (80 - value) + "px";
			fill.height = (80 - value) + "px";
		}
		function maOnColorChanged(swatchId, color)
		{
			maGetPreview();
			
			try
			{
				switch(swatchId)
				{
					case "<%= NormalLineColorSwatch.ClientID %>_Swatch":
						normalLine.borderColor = color;
						break;
					
					case "<%= SelectedLineColorSwatch.ClientID %>_Swatch":
						selectedLine.borderColor = color;
						break;
					
					case "<%= NormalFillColorSwatch.ClientID %>_Swatch":
						normalFill.backgroundColor = color;
						break;
					
					case "<%= SelectedFillColorSwatch.ClientID %>_Swatch":
						selectedFill.backgroundColor = color;
						break;
				}
			}
			catch (error)
			{
				// Ignore attempts to assign invalid colors.
			}
		}
		function maChangeOpacity(sender, eventArgs)
		{
			maChangeDetected();
			maGetPreview();
		   
			var e;
			var id = sender.get_id();
			var value = sender.get_value();
			var valuePercent = value / 100;
			
			switch(id)
			{
				case "<%= SliderNormalFillColorOpacity.ClientID %>":
					e = document.getElementById("<%= NormalFillColorOpacityValue.ClientID %>");
					maSetOpacity(normalFill, value);
					break;
				
				case "<%= SliderSelectedFillColorOpacity.ClientID %>":
					e = document.getElementById("<%= SelectedFillColorOpacityValue.ClientID %>");
					maSetOpacity(selectedFill, value);
					break;
					
				case "<%= SliderNormalLineColorOpacity.ClientID %>":
					e = document.getElementById("<%= NormalLineColorOpacityValue.ClientID %>");
					maSetOpacity(normalLine, value);
					break;
					
				case "<%= SliderSelectedLineColorOpacity.ClientID %>":
					e = document.getElementById("<%= SelectedLineColorOpacityValue.ClientID %>");
					maSetOpacity(selectedLine, value);
					break;
			}
			e.innerHTML = value + "%";
		}
		function maSetOpacity(style, value)
		{
			var valuePercent = value / 100;
			style.filter = "alpha(opacity=" + value + ")";
			style.MozOpacity = valuePercent;
			style.opacity = valuePercent;
		}
		function maInitMarkerStylePreview(line, nfc, nfo, nlc, nlo, sfc, sfo, slc, slo)
		{
			maGetPreview();
			maSetLineThickness(normalLine, normalFill, line);
			maSetLineThickness(selectedLine, selectedFill, line);
			maSetOpacity(normalFill, nfo);
			maSetOpacity(selectedFill, sfo);
			maSetOpacity(normalLine, nlo);
			maSetOpacity(selectedLine, slo);
			maOnColorChanged("<%= NormalFillColorSwatch.ClientID %>_Swatch", nfc);
 			maOnColorChanged("<%= SelectedFillColorSwatch.ClientID %>_Swatch", sfc);
			maOnColorChanged("<%= NormalLineColorSwatch.ClientID %>_Swatch", nlc);
			maOnColorChanged("<%= SelectedLineColorSwatch.ClientID %>_Swatch", slc);
		}
   </script>

	<AvantLogic:QuickHelpTitle ID="MarkerStyleName" runat="server" Title="Marker Style Name" TopMargin="0px" Span="true" />
	<AvantLogic:MemberPageActionButton ID="ShowUsageControl" runat="server" />
   	<AvantLogic:MemberPageActionButton ID="NewMarkerStyleControl" runat="server" />
	<br />
	<asp:TextBox ID="MarkerStyleNameTextBox" runat="server" Width="200" style="margin-top:4px;" />
	<asp:Label ID="MarkerStyleNameError" runat="server" CssClass="textErrorMessage" />
	<br />
   	<AvantLogic:QuickHelpTitle ID="MarkerStyleId" runat="server" Title="Marker Style Id" />
	<asp:Label ID="StyleId" runat="server" CssClass="textNormal" />

	<div class="optionsSectionTitle">Options</div>
	<AvantLogic:QuickHelpTitle ID="MarkerLineWidth" runat="server" Title="<%$ Resources:Text, MarkerLineWidthLabel %>" TopMargin="0px" />
	<table>
	    <tr>
	        <td>
	            <telerik:RadSlider
	                ID="SliderLineWidth"
	                runat="server"
	                Width="100"
	                MinimumValue="0"
                    MaximumValue="32"
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
		
	<div class="optionsSectionTitle">Normal Appearance</div>
	<div class="finePrintHelp" style="padding:0px 0px 12px 0px;">
		The Normal Appearance is what shows when a marker is not selected and the mouse is not over it.
	</div>

	<table cellpadding="0" cellspacing="0">
		<tr>
			<td width="200px">
				<AvantLogic:QuickHelpTitle ID="NormalFillColor" runat="server" Title="<%$ Resources:Text, NormalFillColorLabel %>" TopMargin="0px" />
				<AvantLogic:ColorSwatch Id="NormalFillColorSwatch" runat="server" />
			</td>
			<td width="240px">
				<AvantLogic:QuickHelpTitle ID="NormalFillColorOpacity" runat="server" Title="<%$ Resources:Text, NormalFillColorOpacityLabel %>" TopMargin="0px" />
				<table>
				    <tr>
				        <td>
				            <telerik:RadSlider
				                ID="SliderNormalFillColorOpacity"
				                runat="server"
				                Width="140"
				                MinimumValue="0"
                                MaximumValue="100"
                                Value="100"
                                OnClientValueChanged="maChangeOpacity"
                                Skin="Default"
                                DragText=""
                              />
				        </td>
				        <td>
							<asp:Label ID="NormalFillColorOpacityValue" runat="server" CssClass="unit" />
				        </td>
				    </tr>
				</table>
			</td>
			<td rowspan="2" valign="top">
				<AvantLogic:QuickHelpTitle ID="MarkerStylePreviewNormal" runat="server" Title="Preview" TopMargin="0px" />
				<div style="position:relative;width:120px;height:80px;background-image:url('../Images/MarkerStyleBackground.gif');background-repeat:no-repeat;background-position:center 28px;">
					<div id="previewNormalLine" style="position:absolute;z-index:2;border:solid 1px white;"></div>
					<div id="previewNormalFill" style="position:absolute;z-index:1;">&nbsp;</div>
				</div>
			</td>
		</tr>
		<tr>
			<td width="200px">
				<AvantLogic:QuickHelpTitle ID="NormalLineColor" runat="server" Title="<%$ Resources:Text, NormalLineColorLabel %>" />
				<AvantLogic:ColorSwatch Id="NormalLineColorSwatch" runat="server" />
			</td>
			<td width="240px">
				<AvantLogic:QuickHelpTitle ID="NormalLineColorOpacity" runat="server" Title="<%$ Resources:Text, NormalLineColorOpacityLabel %>" />
				<table>
				    <tr>
				        <td>
				            <telerik:RadSlider
				                ID="SliderNormalLineColorOpacity"
				                runat="server"
				                Width="140"
				                MinimumValue="0"
                                MaximumValue="100"
                                Value="100"
                                OnClientValueChanged="maChangeOpacity"
                                Skin="Default"
                                DragText=""
                              />
				        </td>
				        <td>
							<asp:Label ID="NormalLineColorOpacityValue" runat="server" CssClass="unit" />
				        </td>
				    </tr>
				</table>
			</td>
		</tr>
		<tr>
			<td colspan="3">
				<AvantLogic:QuickHelpTitle ID="NormalShapeEffects" runat="server" Title="Normal Effects" />
				<asp:TextBox ID="NormalEffectsTextBox" runat="server" Width="400" />
			</td>
		</tr>
	</table>
	
	<div class="optionsSectionTitle">Selected Appearance</div>
	<div class="finePrintHelp" style="padding:0px 0px 12px 0px;">
		The Selected Appearance is what shows when a marker is selected or the mouse is over it.
	</div>
	
	<table cellpadding="0" cellspacing="0">
		<tr>
			<td width="200px">
				<AvantLogic:QuickHelpTitle ID="SelectedFillColor" runat="server" Title="<%$ Resources:Text, SelectedFillColorLabel %>" TopMargin="0px" />
				<AvantLogic:ColorSwatch Id="SelectedFillColorSwatch" runat="server" />
			</td>
			<td width="240px">
				<AvantLogic:QuickHelpTitle ID="SelectedFillColorOpacity" runat="server" Title="<%$ Resources:Text, SelectedFillColorOpacityLabel %>" TopMargin="0px" />
				<table>
				    <tr>
				        <td>
				            <telerik:RadSlider
				                ID="SliderSelectedFillColorOpacity"
				                runat="server"
				                Width="140"
				                MinimumValue="0"
                                MaximumValue="100"
                                Value="100"
                                OnClientValueChanged="maChangeOpacity"
                                Skin="Default"
                                DragText=""
                              />
				        </td>
				        <td>
							<asp:Label ID="SelectedFillColorOpacityValue" runat="server" CssClass="unit" />
				        </td>
				    </tr>
				</table>
			</td>
			<td rowspan="2">
				<AvantLogic:QuickHelpTitle ID="MarkerStylePreviewSelected" runat="server" Title="Preview" TopMargin="0px" />
				<div style="position:relative;width:120px;height:80px;background-image:url('../Images/MarkerStyleBackground.gif');background-repeat:no-repeat;background-position:center 28px;">
					<div id="previewSelectedLine" style="position:absolute;z-index:2;border:solid 1px white;"></div>
					<div id="previewSelectedFill" style="position:absolute;z-index:1;">&nbsp;</div>
				</div>
			</td>
		</tr>
		<tr>
			<td width="200px">
				<AvantLogic:QuickHelpTitle ID="SelectedLineColor" runat="server" Title="<%$ Resources:Text, SelectedLineColorLabel %>" />
				<AvantLogic:ColorSwatch Id="SelectedLineColorSwatch" runat="server" />
			</td>
			<td width="240px">
				<AvantLogic:QuickHelpTitle ID="SelectedLineColorOpacity" runat="server" Title="<%$ Resources:Text, SelectedLineColorOpacityLabel %>" />
				<table>
				    <tr>
				        <td>
				            <telerik:RadSlider
				                ID="SliderSelectedLineColorOpacity"
				                runat="server"
				                Width="140"
				                MinimumValue="0"
                                MaximumValue="100"
                                Value="100"
                                OnClientValueChanged="maChangeOpacity"
                                Skin="Default"
                                DragText=""
                              />
				        </td>
				        <td>
							<asp:Label ID="SelectedLineColorOpacityValue" runat="server" CssClass="unit" />
				        </td>
				    </tr>
				</table>
			</td>
		</tr>
		<tr>
			<td colspan="3">
				<AvantLogic:QuickHelpTitle ID="SelectedShapeEffects" runat="server" Title="Selected Effects" />
				<asp:TextBox ID="SelectedEffectsTextBox" runat="server" Width="400" />
			</td>
		</tr>
	</table>
		
</asp:Content>
