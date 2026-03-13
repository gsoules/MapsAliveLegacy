// Copyright (C) 2006 AvantLogic Corporation
using System;
using System.Collections.Generic;
using System.Text;

namespace AvantLogic.MapsAlive.Engine
{
	public class BasePage
	{
		protected int id;

		protected BasePage()
		{
		}

		public BasePage(int id)
		{
			this.id = id;
		}

		#region ===== Properties ========================================================
		
		public int Id
		{
			get { return id; }
		}

		public string ImageTagId
		{
			get { return "image1"; }
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
