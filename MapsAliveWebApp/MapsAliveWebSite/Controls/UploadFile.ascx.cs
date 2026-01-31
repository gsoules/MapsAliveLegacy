// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class Controls_UploadFile : System.Web.UI.UserControl
{
	private int extraStepCount;

	protected void Page_Load(object sender, EventArgs e)
	{
		SelectFileStepNumber.Text = string.Format("{0}", extraStepCount + 1);
		UploadStepNumber.Text = string.Format("{0}", extraStepCount + 2);
	}

	public void AddStep(string description)
	{
		extraStepCount++;
		if (extraStepCount == 1)
		{
			ExtraStep1.Visible = true;
			ExtraStep1Description.Text = description;
		}
		else if (extraStepCount == 2)
		{
			ExtraStep2.Visible = true;
			ExtraStep2Description.Text = description;
		}
		else
		{
			System.Diagnostics.Debug.Fail("Only 2 extra steps are supported");
		}
	}

	public void ClearSteps()
	{
		extraStepCount = 0;
	}

	public Label FileNameLabel
	{
		get { return FileName; }
	}

	public string QuickHelpTitle
	{
		set
		{
			if (value == string.Empty)
				QuickHelp.Visible = false;
			else
				QuickHelp.Title = value;
		}
	}

	public RadProgressArea ProgressArea
	{
		get { return UploadProgressArea; }
	}

	public string QuickHelpTopic
	{
		set { QuickHelp.Topic = value; }
	}

	public string QuickHelpTopMargin
	{
		set { QuickHelp.TopMargin = value; }
	}

	public string SelectedFileName
	{
		set { FileName.Text = value; }
	}

	public void Usage(string buttonName, string fileDescription, string extensions)
	{
		ButtonUpload.Text = buttonName;
		string extensionList = extensions == string.Empty ? string.Empty : string.Format(" ({0})", extensions);
		SelectFileStepDescription.Text = string.Format("Choose a file containing {0}{1}", fileDescription, extensionList);
		UploadStepDescription.Text = string.Format("Click the {0} button", buttonName);
	}
}
