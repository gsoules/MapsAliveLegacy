<%@ Control
	Language="C#"
	AutoEventWireup="true"
	CodeFile="InLineHelp.ascx.cs"
	Inherits="Controls_InLineHelp"
%>

<script language="javascript">                
function maShowInLineHelp(topic, titleId)
{
	var s = document.getElementById('TopicDiv').style;
	var show = s.display == "none";
	s.display = show ? "block" : "none";
	var e = document.getElementById(titleId);
	e.innerHTML = show ? "Hide explanation" : (topic);
}
</script> 

<asp:HyperLink ID="HyperLink" runat="server">
	<asp:Label ID="TopicTitle" runat="server" Text="Label" />
</asp:HyperLink>&nbsp;
<div id="TopicDiv" style="display:none;">
	<asp:Label ID="TopicContent" runat="server" />
</div>

