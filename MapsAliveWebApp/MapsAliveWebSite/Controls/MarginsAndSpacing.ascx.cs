// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class Controls_MarginsAndSpacing : MarginsAndSpacingAccess
{
	// See comments in MarginsAndSpacingAccess.cs for an explanation of this class subclass.
	// Note that we prefix the control IDs with an underscore so that the property
	// names reflect what the IDs would normally be called.
	//
	// IMPORTANT: Whenever you add/remove/change a control here, you must also change
	// the corresponding virtual property in MarginsAndSpacingAccess.cs.

	public override TextBox MarginBottom
	{
		get { return _MarginBottom; }
	}

	public override Label MarginBottomError
	{
		get { return _MarginBottomError; }
	}

	public override TextBox MarginLeft
	{
		get { return _MarginLeft; }
	}

	public override Label MarginLeftError
	{
		get { return _MarginLeftError; }
	}

	public override TextBox MarginRight
	{
		get { return _MarginRight; }
	}

	public override Label MarginRightError
	{
		get { return _MarginRightError; }
	}

	public override TextBox MarginTop
	{
		get { return _MarginTop; }
	}

	public override Label MarginTopError
	{
		get { return _MarginTopError; }
	}

	public override TextBox SpacingH
	{
		get { return _SpacingH; }
	}

	public override Label SpacingHError
	{
		get { return _SpacingHError; }
	}

	public override HtmlTableCell SpacingCellH1
	{
		get { return _SpacingCellH1; }
	}

	public override HtmlTableCell SpacingCellH2
	{
		get { return _SpacingCellH2; }
	}

	public override HtmlTableCell SpacingCellV1
	{
		get { return _SpacingCellV1; }
	}

	public override HtmlTableCell SpacingCellV2
	{
		get { return _SpacingCellV2; }
	}

	public override TextBox SpacingV
	{
		get { return _SpacingV; }
	}

	public override Label SpacingVError
	{
		get { return _SpacingVError; }
	}

	public override Panel SpacingPanel
	{
		get { return _SpacingPanel; }
	}

	public override Panel SpacingYesPanel
	{
		get { return _SpacingYesPanel; }
	}

	public override Panel SpacingNoPanel
	{
		get { return _SpacingNoPanel; }
	}
}
