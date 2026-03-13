// Copyright (C) 2006 AvantLogic Corporation
using System;
using System.Collections.Generic;
using System.Text;

namespace AvantLogic.MapsAlive.Engine
{
	public class BaseMarkerRuleSet
	{
		protected int id;
		protected BaseMarkerRule clickRule;
		protected BaseMarkerRule mouseEnterRule;
		protected BaseMarkerRule mouseExitRule;

		public BaseMarkerRuleSet()
		{
		}

		public BaseMarkerRuleSet(BaseMarkerRule clickRule, BaseMarkerRule mouseEnterRule, BaseMarkerRule mouseExitRule)
		{
			this.clickRule = clickRule;
			this.mouseEnterRule = mouseEnterRule;
			this.mouseExitRule = mouseExitRule;
		}

		#region ===== Properties ========================================================

		public virtual BaseMarkerRule BaseClickRule
		{
			get { return clickRule; }
		}

		public virtual BaseMarkerRule BaseMouseEnterRule
		{
			get { return mouseEnterRule; }
		}

		public virtual BaseMarkerRule BaseMouseExitRule
		{
			get { return mouseExitRule; }
		}

		public int Id
		{
			get { return id; }
		}
		#endregion

		#region ===== Public ============================================================
		#endregion

		#region ===== Protected =========================================================
		#endregion

		#region ===== Private ===========================================================
		#endregion
	}
}
