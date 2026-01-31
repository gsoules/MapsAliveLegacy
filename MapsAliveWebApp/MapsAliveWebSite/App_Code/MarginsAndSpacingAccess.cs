// Copyright (C) 2003-2010 AvantLogic Corporation
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public class MarginsAndSpacingAccess : System.Web.UI.UserControl
{
	// The virtual methods below serve as a public interface to provide access to the controls in the
	// MarginsAndSpacing.ascx user control. It exists because of a .NET deficiency that prevents
	// an App_Code class from accessing a user control due to an issue having to due with compilcation
	// order and circular dependencies. By providing this level of indirection, we are able to share
	// the MarginsAndSpacing user control.

	// The motivation for this class was to give the LayoutPage class access to the controls on in the
	// MarginsAndSpacing user control. The LayoutPage class is a subclass of MemberPage. It contains
	// layout logic that is shared by PopupAppearance, LayoutAreaMarginsAndSpacing, and MapsMargins,
	// each of which is a subclass of LayoutPage. Originally the controls and code-behind logic for
	// these three screens was contained in LayoutSizes.aspx which dynamically presented controls based
	// on whether the current tourPage used popups. LayoutSizes also contained other layout logic, but
	// it was a complex page and the user interface was confusing so in 12/2009 we broke into multiple
	// screens. However, each of the new screens still needed the controls and logic from LayoutSizes
	// that let the user set margins, spacing, and autolayout options. That logic is non-trivial
	// because it has to be wired up to the MemberPage logic for InitControls, ReadPageFields, Validate,
	// etc. Rather than duplicate the controls and logic in each of the three new screens, we factored 
	// it out by putting the controls in MarginsAndSpacing and the logic in LayoutPage. By providing
	// this MarginsAndSpacingAccess class, LayoutPage can access those controls.

	public virtual TextBox MarginBottom
	{
		get { return null; }
	}

	public virtual Label MarginBottomError
	{
		get { return null; }
	}

	public virtual TextBox MarginLeft
	{
		get { return null; }
	}

	public virtual Label MarginLeftError
	{
		get { return null; }
	}

	public virtual TextBox MarginRight
	{
		get { return null; }
	}

	public virtual Label MarginRightError
	{
		get { return null; }
	}

	public virtual TextBox MarginTop
	{
		get { return null; }
	}

	public virtual Label MarginTopError
	{
		get { return null; }
	}

	public virtual Label SpacingHelp
	{
		get { return null; }
	}

	public virtual Label SpacingTitle
	{
		get { return null; }
	}

	public virtual TextBox SpacingH
	{
		get { return null; }
	}

	public virtual Label SpacingHError
	{
		get { return null; }
	}

	public virtual HtmlTableCell SpacingCellH1
	{
		get { return null; }
	}

	public virtual HtmlTableCell SpacingCellH2
	{
		get { return null; }
	}

	public virtual HtmlTableCell SpacingCellV1
	{
		get { return null; }
	}

	public virtual HtmlTableCell SpacingCellV2
	{
		get { return null; }
	}

	public virtual TextBox SpacingV
	{
		get { return null; }
	}

	public virtual Label SpacingVError
	{
		get { return null; }
	}

	public virtual Panel SpacingPanel
	{
		get { return null; }
	}

	public virtual Panel SpacingYesPanel
	{
		get { return null; }
	}
	
	public virtual Panel SpacingNoPanel
	{
		get { return null; }
	}
}
