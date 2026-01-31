<%@ Page
	Language="C#"
	MasterPageFile="~/Masters/MemberPage.master"
	AutoEventWireup="true"
	CodeFile="EditCategory.aspx.cs"
	Inherits="Members_EditCategory"
%>

<%@ MasterType VirtualPath="~/Masters/MemberPage.master" %>

<%@ Register Assembly="App_Code\ServerControls\QuickHelpTitle.cs" TagPrefix="AvantLogic" Namespace="AvantLogic.MapsAlive" %>

<asp:Content ID="Content" ContentPlaceHolderID="ContentBody" Runat="Server">
	<script language="javascript">                
	function maShowDimensions(categoryTypeDropDownList)
	{
		maChangeDetected();
		document.getElementById(formContentId + "PositionPanel").style.display = categoryTypeDropDownList.value == "1" ? "block" : "none";	
		document.getElementById(formContentId + "DimensionsPanel").style.display = categoryTypeDropDownList.value == "2" ? "block" : "none";	
	}
	</script>
	
	<AvantLogic:QuickHelpTitle ID="CategoryTitle" runat="server" Title="<%$ Resources:Text, CategoryTitleLabel %>" TopMargin="0px" Span="true" />
		<AvantLogic:MemberPageActionButton ID="ShowUsageControl" runat="server" />
		<br />
	<asp:TextBox ID="CategoryTitleTextBox" runat="server" Width="140"></asp:TextBox>
	<asp:Label ID="CategoryTitleError" runat="server" CssClass="textErrorMessage" />

	<AvantLogic:QuickHelpTitle ID="CategoryCode" runat="server" Title="<%$ Resources:Text, CategoryCodeLabel %>" />
	<asp:TextBox ID="CategoryCodeTextBox" Width="140" runat="server"></asp:TextBox>
	<asp:Label ID="CategoryCodeError" runat="server" CssClass="textErrorMessage" />

	<AvantLogic:QuickHelpTitle ID="CategoryKind" runat="server" Title="Type" />
	<asp:DropDownList ID="CategoryTypeDropDownList" runat="server" style="margin-top:4px;">
		<asp:ListItem Text="Directory Group" Value="1" />
		<asp:ListItem Text="Hotspot Image Size Override" Value="2" />
	</asp:DropDownList>
	
	<asp:Panel ID="PositionPanel" runat="server">
		<AvantLogic:QuickHelpTitle ID="CategoryPosition" runat="server" Title="Directory Position" />
		<asp:TextBox ID="PositionTextBox" runat="server" Width="30px" />
		<asp:Label ID="PositionError" runat="server" CssClass="textErrorMessage" />
	</asp:Panel>

	<asp:Panel ID="DimensionsPanel" runat="server">
		<AvantLogic:QuickHelpTitle ID="CategoryDimensions" runat="server" Title="Dimensions" />
		<table style="padding:0px;">
			<tr>
				<td>
					<asp:Label runat="server" CssClass="controlLabel" Text="Width" />
				</td>
				<td>
					<asp:TextBox ID="WidthTextBox" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
					<asp:Label ID="WidthError" runat="server" CssClass="textErrorMessage" />
				</td>
				<td style="padding-left:8px;">
					<asp:Label runat="server" CssClass="controlLabel" Text="Height" />
				</td>
				<td>
					<asp:TextBox ID="HeightTextBox" runat="server" Width="30px"></asp:TextBox><span class="unit">px</span>
					<asp:Label ID="HeightError" runat="server" CssClass="textErrorMessage" />
				</td>
			</tr>
		</table>
	</asp:Panel>
</asp:Content>
