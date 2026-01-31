// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.UI.WebControls;

public partial class Controls_InLineHelp : System.Web.UI.UserControl
{
	public void SetTopic(string topic, string content)
	{
		string title = "Tell me about " + topic;
		HyperLink.NavigateUrl = string.Format("javascript:maShowInLineHelp('{0}','{1}');", title, TopicTitle.ClientID);
		TopicContent.Text = content;
		TopicTitle.Text = title;
	}
}
