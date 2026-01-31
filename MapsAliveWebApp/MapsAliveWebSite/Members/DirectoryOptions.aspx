<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="DirectoryOptions.aspx.cs"
	Inherits="Members_DirectoryOptions"
	Trace="false"
	TraceMode="SortByCategory"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
    <script type="text/javascript">
		function maOnColorChanged(swatchId, color)
		{
			if (
			swatchId == "<%= TitleTextColorSwatch.ClientID %>_Swatch" ||
			swatchId == "<%= TitleBarColorSwatch.ClientID %>_Swatch" ||
			swatchId == "<%= BorderColorSwatch.ClientID %>_Swatch" ||
			swatchId == "<%= PreviewBorderColorSwatch.ClientID %>_Swatch" ||
			swatchId == "<%= PreviewTextColorSwatch.ClientID %>_Swatch" ||
			swatchId == "<%= PreviewBackgroundColorSwatch.ClientID %>_Swatch" ||
			swatchId == "<%= StatusTextColorSwatch.ClientID %>_Swatch" ||
			swatchId == "<%= StatusBackgroundColorSwatch.ClientID %>_Swatch"
			)
			{
                var e = document.getElementById("<%= UseColorSchemeColorsCheckBox.ClientID %>");
                if (e)
				    e.checked = false;
			}
		}
		function maOnShowColorWarning(checkbox)
		{
			maChangeDetectedForPreview();
			var e = document.getElementById("ColorWarning");
			e.className = checkbox.checked ? "previewMessage" : "optionsSectionTitle";
			e.innerHTML = checkbox.checked ? "Click save to see tour colors" : "Colors";
		}
   </script>
    <asp:Panel ID="BasicOptionsPanel" runat="server" Visible="false">
		<div>
            <asp:CheckBox ID="ShowDirectoryBasicCheckBox" runat="server" />
		    <AvantLogic:QuickHelpTitle ID="DirectoryBasicShow" runat="server" Title="<%$ Resources:Text, ShowDirectoryLabel %>" Span="true" />
		</div>
        <div style="margin-top:6px;">
		    <asp:CheckBox ID="ShowMenuBasicCheckBox" runat="server" />
		    <AvantLogic:QuickHelpTitle ID="ShowMenuBasic" runat="server" Title="Show Menu" Span="true" />
        </div>
    </asp:Panel>

    <asp:Panel ID="AllOptionsPanel" runat="server" Visible="false">
	    <div>
		    <asp:CheckBox ID="ShowDirectoryCheckBox" runat="server" />
		    <AvantLogic:QuickHelpTitle ID="DirectoryShow" runat="server" Title="<%$ Resources:Text, ShowDirectoryLabel %>" Span="true" />
	    </div>
        <div style="margin-top:6px;">
		    <asp:CheckBox ID="ShowMenuCheckBox" runat="server" />
		    <AvantLogic:QuickHelpTitle ID="ShowMenu" runat="server" Title="Show Menu" Span="true" />
        </div>
	
	    <div class="optionsSectionTitle">General Options</div>
	    <asp:Panel ID="CustomLocationPanel" runat="server">
		    <table>
			    <tr>
				    <td valign="top">
					    <AvantLogic:QuickHelpTitle ID="DirectoryLocation" runat="server" Title="Location" TopMargin="0px" />
					    <asp:DropDownList ID="LocationDropDownList" runat="server" style="margin-top:4px;">
						    <asp:ListItem Text="Map top left" Value="2" />
						    <asp:ListItem Text="Map top right" Value="3" />
						    <asp:ListItem Text="Title bar" Value="4" />
						    <asp:ListItem Text="Top menu bar" Value="5" />
						    <asp:ListItem Text="Custom" Value="1" />
					    </asp:DropDownList>
				    </td>
				    <td valign="top" style="padding-left:32px;">
					    <AvantLogic:QuickHelpTitle ID="DirectoryLocationCustom" runat="server" Title="Offset" TopMargin="0px" />
					    <table style="padding:0px;">
						    <tr>
							    <td>
								    <asp:Label runat="server" CssClass="controlLabel" Text="X" />
							    </td>
							    <td>
								    <asp:TextBox ID="CustomLocationX" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
								    <asp:Label ID="CustomLocationXError" runat="server" CssClass="textErrorMessage" />
							    </td>
							    <td style="padding-left:8px;">
								    <asp:Label runat="server" CssClass="controlLabel" Text="Y" />
							    </td>
							    <td>
								    <asp:TextBox ID="CustomLocationY" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
								    <asp:Label ID="CustomLocationYError" runat="server" CssClass="textErrorMessage" />
							    </td>
						    </tr>
					    </table>
				    </td>
			    </tr>
		    </table>
	    </asp:Panel>
	    <table class="optionsTable">
		    <tr>
			    <td>
				    <AvantLogic:QuickHelpTitle ID="DirectoryGrouping" runat="server" Title="Grouping" TopMargin="0px" />
				    <asp:DropDownList ID="GroupingDropDownList" runat="server" style="margin-top:4px;">
					    <asp:ListItem Text="None (alphabetic only)" Value="0" />
					    <asp:ListItem Text="By category" Value="1" />
					    <asp:ListItem Text="By category, then page" Value="2" />
					    <asp:ListItem Text="By page" Value="3" />
					    <asp:ListItem Text="By page, then category" Value="4" />
				    </asp:DropDownList>
			    </td>
		    </tr>
		    <tr>
			    <td>
				    <div style="margin-top:12px;">
					    <asp:CheckBox ID="ShowAllPagesCheckBox" runat="server" />
					    <AvantLogic:QuickHelpTitle ID="DirectoryShowAllPages" runat="server" Span="true" Title="Show Hotspots for All Pages" />
				    </div>
				    <div style="margin-top:4px;">
					    <asp:CheckBox ID="ShowSearchCheckBox" runat="server" />
					    <AvantLogic:QuickHelpTitle ID="DirectoryShowSearch" runat="server" Span="true" Title="Show Search Box" />
				    </div>
                    <asp:Panel ID="AlignContentRightPanel" runat="server">
				        <div style="margin-top:4px;">
					        <asp:CheckBox ID="AlignContentRightCheckBox" runat="server" />
					        <AvantLogic:QuickHelpTitle ID="DirectoryAlignContentRight" runat="server" Span="true" Title="Dropdown On Right" TopMargin="0px" />
				        </div>
                    </asp:Panel>
                    <asp:Panel ID="PreviewOnRightPanel" runat="server">
			            <div style="margin-top:4px;">
				            <asp:CheckBox ID="PreviewOnRightCheckBox" runat="server" />
				            <AvantLogic:QuickHelpTitle ID="DirectoryPreviewOnRight" runat="server" Span="true" Title="Preview On Right" TopMargin="0px" />
			            </div>
                    </asp:Panel>
				    <div style="margin-top:4px;">
					    <asp:CheckBox ID="ShowImagePreviewCheckBox" runat="server" />
					    <AvantLogic:QuickHelpTitle ID="DirectoryShowImagePreview" runat="server" Span="true" Title="Show Image Preview" />
				    </div>
				    <div style="margin-top:4px;">
					    <asp:CheckBox ID="ShowTextPreviewCheckBox" runat="server" />
					    <AvantLogic:QuickHelpTitle ID="DirectoryShowTextPreview" runat="server" Span="true" Title="Show Text Preview" />
				    </div>
				    <div style="margin-top:4px;">
					    <asp:CheckBox ID="StaysOpenCheckBox" runat="server" />
					    <AvantLogic:QuickHelpTitle ID="DirectoryStaysOpen" runat="server" Span="true" Title="Stay Open" />
				    </div>
				    <div style="margin-top:4px;">
					    <asp:CheckBox ID="OpenExpandedCheckBox" runat="server" />
					    <AvantLogic:QuickHelpTitle ID="DirectoryOpenExpanded" runat="server" Span="true" Title="Expand Groups when Tour Loads" />
				    </div>
				    <div style="margin-top:4px;">
					    <asp:CheckBox ID="AutoCollapseCheckBox" runat="server" />
					    <AvantLogic:QuickHelpTitle ID="DirectoryAutoCollapse" runat="server" Span="true" Title="Auto Collapse Groups" TopMargin="0px" />
				    </div>
			    </td>
		    </tr>
	    </table>
	
	    <div class="optionsSectionTitle">Size Options</div>
	    <table class="optionsTable">
		    <tr>
			    <td colspan = "2" valign="top">
				    <table>
					    <tr ID="TitleBarWidthOption" runat="server">
						    <td align="Right">
							    <asp:Label runat="server" CssClass="controlLabel" Text="Title Bar Width"></asp:Label>
						    </td>
						    <td>
							    <asp:TextBox ID="TitleBarWidthTextBox" runat="server" Width="30px" /><span class="unit">px</span>
							    <asp:Label ID="TitleBarWidthError" runat="server" CssClass="textErrorMessage" />
							    <AvantLogic:QuickHelpTitle ID="DirectoryTitleBarWidth" runat="server" Span="true" />
						    </td>
					    </tr>
					    <tr>
						    <td align="Right">
							    <asp:Label runat="server" CssClass="controlLabel" Text="Navigation Panel Width"></asp:Label>
						    </td>
						    <td>
							    <asp:TextBox ID="ContentWidthTextBox" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
							    <asp:Label ID="ContentWidthError" runat="server" CssClass="textErrorMessage" />
							    <AvantLogic:QuickHelpTitle ID="DirectoryContentWidth" runat="server" Span="true" />
						    </td>
					    </tr>
					    <tr>
						    <td align="Right">
							    <asp:Label runat="server" CssClass="controlLabel" Text="Navigation Panel Max Height"></asp:Label>
						    </td>
						    <td>
							    <asp:TextBox ID="ContentHeightTextBox" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
							    <asp:Label ID="ContentHeightError" runat="server" CssClass="textErrorMessage" />
							    <AvantLogic:QuickHelpTitle ID="DirectoryContentHeight" runat="server" Span="true" />
						    </td>
					    </tr>
					    <tr ID="PreviewWidthOption" runat="server">
						    <td align="Right">
							    <asp:Label runat="server" CssClass="controlLabel" Text="Preview Width"></asp:Label>
						    </td>
						    <td>
							    <asp:TextBox ID="PreviewWidthTextBox" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
							    <asp:Label ID="PreviewWidthError" runat="server" CssClass="textErrorMessage" />
							    <AvantLogic:QuickHelpTitle ID="DirectoryPreviewWidth" runat="server" Span="true" />
						    </td>
					    </tr>
					    <tr>
						    <td align="Right">
							    <asp:Label runat="server" CssClass="controlLabel" Text="Preview Image Width"></asp:Label>
						    </td>
						    <td>
							    <asp:TextBox ID="PreviewImageWidthTextBox" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
							    <asp:Label ID="PreviewImageWidthError" runat="server" CssClass="textErrorMessage" />
							    <AvantLogic:QuickHelpTitle ID="DirectoryPreviewImageWidth" runat="server" Span="true" />
						    </td>
					    </tr>
				    </table>
			    </td>
		    </tr>
	    </table>
		
	    <div class="optionsSectionTitle">Text Options</div>
	    <table class="optionsTable">
		    <tr>
			    <td>
				    <table>
					    <tr ID="TitleOption" runat="server">
						    <td align="Right">
							    <asp:Label runat="server" CssClass="controlLabel" Text="Directory Title"></asp:Label>
						    </td>
						    <td>
							    <asp:TextBox ID="TitleTextBox" runat="server" Width="120px"></asp:TextBox>
							    <AvantLogic:QuickHelpTitle ID="DirectoryTitle" runat="server" Span="true" />
						    </td>
					    </tr>
					    <tr>
						    <td align="Right">
							    <asp:Label runat="server" CssClass="controlLabel" Text="Alpha Sort"></asp:Label>
						    </td>
						    <td>
							    <asp:TextBox ID="AlphaSortTooltipTextBox" runat="server" Width="120px"></asp:TextBox>
							    <AvantLogic:QuickHelpTitle ID="DirectoryAlphaSortTooltip" runat="server" Span="true" />
						    </td>
					    </tr>
					    <tr>
						    <td align="Right">
							    <asp:Label runat="server" CssClass="controlLabel" Text="Group Sort"></asp:Label>
						    </td>
						    <td>
							    <asp:TextBox ID="GroupSortTooltipTextBox" runat="server" Width="120px"></asp:TextBox>
							    <AvantLogic:QuickHelpTitle ID="DirectoryGroupSortTooltip" runat="server" Span="true" />
						    </td>
					    </tr>
					    <tr>
						    <td align="Right">
							    <asp:Label runat="server" CssClass="controlLabel" Text="Search Box"></asp:Label>
						    </td>
						    <td>
							    <asp:TextBox ID="SearchBoxLabelTextBox" runat="server" Width="120px"></asp:TextBox>
							    <AvantLogic:QuickHelpTitle ID="DirectorySearchBoxLabel" runat="server" Span="true" />
						    </td>
					    </tr>
					    <tr ID="SearchResultsMessageOption" runat="server">
						    <td align="Right">
							    <asp:Label runat="server" CssClass="controlLabel" Text="Search Results Message"></asp:Label>
						    </td>
						    <td>
							    <asp:TextBox ID="SearchResultsMessageTextBox" runat="server" Width="120px"></asp:TextBox>
							    <AvantLogic:QuickHelpTitle ID="DirectorySearchResultsMessage" runat="server" Span="true" OffsetX="20" OffsetY="-54" />
						    </td>
					    </tr>
					    <tr ID="ItemsMessageOption" runat="server">
						    <td align="Right">
							    <asp:Label runat="server" CssClass="controlLabel" Text="Items Message"></asp:Label>
						    </td>
						    <td>
							    <asp:TextBox ID="ItemsMessageTextBox" runat="server" Width="120px"></asp:TextBox>
							    <AvantLogic:QuickHelpTitle ID="DirectoryItemsMessage" runat="server" Span="true" OffsetX="20" OffsetY="-66" />
						    </td>
					    </tr>
					    <tr ID="ClearButtonOption" runat="server">
						    <td align="Right">
							    <asp:Label ID="ClearButtonLabel" runat="server" CssClass="controlLabel" Text="Clear Button Label"></asp:Label>
						    </td>
						    <td>
							    <asp:TextBox ID="ClearButtonLabelTextBox" runat="server" Width="120px"></asp:TextBox>
							    <AvantLogic:QuickHelpTitle ID="MenuTitle" runat="server" Span="true" OffsetX="20" OffsetY="-54" />
						    </td>
					    </tr>
				    </table>
			    </td>
		    </tr>
	    </table>
	
	    <div id="ColorWarning" class="optionsSectionTitle">Colors</div>
        <table>
            <tr ID="UseColorSchemeOption" runat="server">
                <td colspan="2" style="padding-left: 60px">
                    <asp:CheckBox ID="UseColorSchemeColorsCheckBox" runat="server" />
                    <AvantLogic:QuickHelpTitle ID="UseTourStyleColorsForDirectory" runat="server" Span="true" Title="Use Tour Colors" />
                </td>
            </tr>
            <tr>
                <AvantLogic:ColorSwatch ID="TitleBarColorSwatch" Label="Title Background" QuickHelpTitle="DirectoryTitleBarColor" runat="server" Col="true" />
            </tr>
            <tr>
                <AvantLogic:ColorSwatch ID="BorderColorSwatch" Label="Directory Border" QuickHelpTitle="DirectoryBorderColor" runat="server" Col="true" />
            </tr>
            <tr>
                <AvantLogic:ColorSwatch ID="PreviewBorderColorSwatch" Label="Preview Border" QuickHelpTitle="DirectoryPreviewBorderColor" runat="server" Col="true" />
            </tr>
            <tr>
                <AvantLogic:ColorSwatch ID="PreviewTextColorSwatch" Label="Preview Text" QuickHelpTitle="DirectoryPreviewTextColor" runat="server" Col="true" />
            </tr>
            <tr>
                <AvantLogic:ColorSwatch ID="PreviewBackgroundColorSwatch" Label="Preview Background" QuickHelpTitle="DirectoryPreviewBackgroundColor" runat="server" Col="true" />
            </tr>
            <tr>
                <AvantLogic:ColorSwatch ID="StatusTextColorSwatch" Label="Status Text" QuickHelpTitle="DirectoryStatusTextColor" runat="server" Col="true" />
            </tr>
            <tr>
                <AvantLogic:ColorSwatch ID="StatusBackgroundColorSwatch" Label="Status Background" QuickHelpTitle="DirectoryStatusBackgroundColor" runat="server" Col="true" />
            </tr>
            <tr>
                <AvantLogic:ColorSwatch ID="TitleTextColorSwatch" Label="Title Text" QuickHelpTitle="DirectoryTitleTextColor" runat="server" Col="true" />
            </tr>

            <!-- Rows above used to be in the left column. They correspond to tour color scheme colors. Rows below used to be in the right column. -->

            <tr>
                <AvantLogic:ColorSwatch ID="BackgroundColorSwatch" Label="Dropdown Background" QuickHelpTitle="DirectoryBackgroundColor" runat="server" Col="true" />
            </tr>
            <tr>
                <AvantLogic:ColorSwatch ID="Level1TextColorSwatch" Label="Level 1 Text" QuickHelpTitle="DirectoryLevel1Color" runat="server" Col="true" />
            </tr>
            <tr>
                <AvantLogic:ColorSwatch ID="Level2TextColorSwatch" Label="Level 2 Text" QuickHelpTitle="DirectoryLevel2Color" runat="server" Col="true" />
            </tr>
            <tr>
                <AvantLogic:ColorSwatch ID="EntryTextColorSwatch" Label="Entry Text" QuickHelpTitle="DirectoryEntryTextColor" runat="server" Col="true" />
            </tr>
            <tr>
                <AvantLogic:ColorSwatch ID="EntryCountColorSwatch" Label="Entry Count" QuickHelpTitle="DirectoryEntryCountColor" runat="server" Col="true" />
            </tr>
            <tr>
                <AvantLogic:ColorSwatch ID="EntryTextHoverColorSwatch" Label="Entry Text Hover" QuickHelpTitle="DirectoryEntryTextHoverColor" runat="server" Col="true" />
            </tr>
            <tr>
                <AvantLogic:ColorSwatch ID="PreviewImageBorderColorSwatch" Label="Preview Image Border" QuickHelpTitle="DirectoryPreviewImageBorderColor" runat="server" Col="true" />
            </tr>
            <tr>
                <AvantLogic:ColorSwatch ID="SearchResultsTextColorSwatch" Label="Search Results Text" QuickHelpTitle="DirectorySearchResultsTextColor" runat="server" Col="true" />
            </tr>
            <tr>
                <AvantLogic:ColorSwatch ID="SearchResultsBackgroundColorSwatch" Label="Search Results Highlight" QuickHelpTitle="DirectorySearchResultsBackgroundColor" runat="server" Col="true" OffsetX="20" OffsetY="-54" />
            </tr>
        </table>
    </asp:Panel>

</asp:Content>
