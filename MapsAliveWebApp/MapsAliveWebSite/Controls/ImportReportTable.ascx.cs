// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.UI.WebControls;

public partial class Controls_ImportReportTable : System.Web.UI.UserControl
{
	public Label Body
	{
		get { return ReportBody; }
	}

	public Panel Panel
	{
		get { return ReportPanel; }
	}
	
	public Label Title
	{
		get { return ReportTitle; }
	}
}
