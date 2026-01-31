<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="TemplateSplitters.aspx.cs"
	Inherits="Members_TemplateSplitters"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>
<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>
<%@ Register Assembly="App_Code\ServerControls\SlideThumbs.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<% =StyleDefinitions %>
	<asp:HiddenField ID="LeftWidth" runat="server" />
	<asp:HiddenField ID="TopHeight" runat="server" />
	<asp:HiddenField ID="SlideWidth" runat="server" />
	<asp:HiddenField ID="SlideHeight" runat="server" />

    <asp:Panel ID="InfoPanel" runat="server" Visible="false" style="margin-bottom:12px;">
		<div><asp:Label ID="SplitterInfoH" runat="server" CssClass="controlLabel"/></div>
		<div><asp:Label ID="SplitterInfoV" runat="server" CssClass="controlLabel"/></div>
    </asp:Panel>
	
	<asp:Panel ID="LayoutPreviewInstructionsPanel" runat="server" class="finePrint" style="margin-left:4px;color:#555555;">
		<asp:Label ID="LayoutPreviewInstructions" runat="server" style="line-height:14px;"/>
	</asp:Panel>

	<asp:Panel ID="LayoutPreviewPanel" runat="server" Visible="false">
		<div runat="server" id="SliderControl" style="position:relative;margin-bottom:12px;">
			<div runat="server" id="SliderVerticalTrack" class="sliderVerticalTrack">
				<div runat="server" id="SliderVertical" class="sliderVertical"></div>
			</div>

			<asp:Panel ID="LayoutPreviewImagePanel" runat="server" style="position:absolute;">
				<asp:Image ID="LayoutPreviewImage" runat="server" />
			</asp:Panel>
			
			<div runat="server" id= "SliderHorizontalTrack" class="sliderHorizontalTrack">
				<div runat="server" id="SliderHorizontal" class="sliderHorizontal"></div>
			</div>
			<div runat="server" id="SliderHorizontalNudgeLeft" class="sliderNudgeLeft" onclick="maNudgeSliderHorizontal(-1);"></div>
			<div runat="server" id="SliderHorizontalNudgeRight" class="sliderNudgeRight" onclick="maNudgeSliderHorizontal(1);"></div>
			<div runat="server" id="SliderVerticalNudgeUp" class="sliderNudgeUp" onclick="maNudgeSliderVertical(-1);"></div>
			<div runat="server" id="SliderVerticalNudgeDown" class="sliderNudgeDown" onclick="maNudgeSliderVertical(1);"></div>
		</div>

    	<asp:Panel ID="OptionsPanel" runat="server" Visible="false">
		    <div class="optionsSectionTitle" style="margin-top:24px;">Options</div>
		    <table>
			    <tr>
				    <td valign="top" width="270">
					    <div ID="SplitterLocksPanel" runat="server">
						    <table cellpadding="1" cellspacing="1" >
							    <tr>
								    <td colspan="2">
									    <AvantLogic:QuickHelpTitle ID="SplitterOptions" runat="server" Title="Splitter Options" TopMargin="0px" Span="true" OffsetX="20" OffsetY="-300" />
								    </td>
							    </tr>
							    <tr>
								    <td style="padding-left:8px;padding-top:6px;">
									    <asp:CheckBox ID="SplitterLockedCheckBoxH" runat="server" />
									    <asp:Label ID="SplitterLockedLabelH" runat="server" CssClass="controlLabel"/>
								    </td>
							    </tr>
							    <tr>
								    <td style="padding-left:8px;padding-top:6px;">
									    <asp:CheckBox ID="SplitterLockedCheckBoxV" runat="server" />
									    <asp:Label ID="SplitterLockedLabelV" runat="server" CssClass="controlLabel"/>
								    </td>
							    </tr>
						    </table>
					    </div>
				    </td>
				    <td valign="top" colspan="2">
					    <asp:Panel ID="AutoYesPanel" runat="server" Visible="false">
						    <table cellpadding="1" cellpadding="1">
							    <tr>
								    <td colspan="2">
									    <AvantLogic:QuickHelpTitle ID="MinNonMapSize" runat="server" Title="Auto Layout Minimums" TopMargin="0px" OffsetX="20" OffsetY="-80" />
								    </td>
							    </tr>
							    <tr ID="AutoWidthRow" runat="server">
								    <td style="padding-left:8px;" align="Right">
									    <asp:Label ID="AutoWidthMeaning" runat="server" CssClass="controlLabel" />
								    </td>
								    <td>
									    <asp:TextBox ID="AutoWidth" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
									    <asp:Label ID="AutoWidthError" runat="server" CssClass="textErrorMessage" />
								    </td>
							    </tr>
							    <tr ID="AutoHeightRow" runat="server">
								    <td style="padding-left:8px;" align="Right">
									    <asp:Label ID="AutoHeightMeaning" runat="server" CssClass="controlLabel" />
								    </td>
								    <td>
									    <asp:TextBox ID="AutoHeight" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
									    <asp:Label ID="AutoHeightError" runat="server" CssClass="textErrorMessage" />
								    </td>
							    </tr>
						    </table>
					    </asp:Panel>
					    <asp:Panel ID="AutoNoPanel" runat="server" CssClass="finePrintHelp" Visible="false">
						    The selected layout has only one area. Auto Layout Minimums only appear for layouts with 2 or 3 areas.
					    </asp:Panel>
				    </td>
			    </tr>
		    </table>		
		
		    <div>
			    <AvantLogic:MemberPageActionButton ID="RestoreLayoutControl" runat="server"/>
			    <AvantLogic:QuickHelpTitle ID="RestoreLayoutCanvasSplitters" runat="server" Span="true" OffsetX="20" OffsetY="-145" />
		    </div>
	    </asp:Panel>
	</asp:Panel>
	
	<div id="SliderCoord" class="splitterReadout">
		<span id="SliderCoordMeaning"></span>
		<span id="SliderCoordValue" style="color:red;"></span>
	</div>
	
	<div id="SplitterBar" style="position:absolute;background-color:#ffffff;border:solid 1px #ff0000;visibility:hidden;height:20px;font-size:8px;"></div>
</asp:Content>

