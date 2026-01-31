// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public partial class Controls_Nag : System.Web.UI.UserControl
{
	public string Message
	{
		set { NagMessage.Text = value; }
	}
}
