using System;
using System.Web.UI.WebControls;

public partial class Controls_TemplateSpacing : System.Web.UI.UserControl
{
	protected void Page_Load(object sender, EventArgs e)
	{
	}

	public TextBox Bottom
	{
		get { return MarginBottom; }
	}

	public Label BottomError
	{
		get { return MarginBottomError; }
	}

	public string Help
	{
		set { MarginsHelp.Text = value; }
	}

	public TextBox Left
	{
		get { return MarginLeft; }
	}

	public Label LeftError
	{
		get { return MarginLeftError; }
	}

	public TextBox Right
	{
		get { return MarginRight; }
	}

	public Label RightError
	{
		get { return MarginRightError; }
	}

	public string Title
	{
		set { MarginsTitle.Text = value; }
	}

	public TextBox Top
	{
		get { return MarginTop; }
	}

	public Label TopError
	{
		get { return MarginTopError; }
	}
}
