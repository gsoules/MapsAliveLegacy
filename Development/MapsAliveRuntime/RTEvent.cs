// Copyright (C) 2003-2005 AvantLogic Corporation
using System;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Diagnostics;

namespace AvantLogic.MapsAlive.Runtime
{
	using System;

	public class RTEvent
	{
		public enum Types { INIT = 0, CLICK = 1, MOUSEENTER = 3, MOUSEEXIT = 4 };
		public enum Actions { NOACTION=0, GOTOPAGE=1, LINKURL=2, SHOWVIEW=4 };
		

		protected int type;
		protected int action = 0;
		protected bool select = false;
		protected bool javascript = false;
		protected string pageurl = "";
		protected Hashtable javascripts = new Hashtable();
		protected Hashtable linkUrls = new Hashtable();

		#region ===== Constructors ====================================================

		public RTEvent():base()
		{
		}

		#endregion

		#region ===== Accessors ====================================================

		public int Type
		{
			get { return type; }
			set { type = value; }
		}		

		public int Action
		{
			get { return action; }
			set { action = value; }
		}		


		public bool Select
		{
			get { return select; }
			set { select = value; }
		}		

		public bool Javascript
		{
			get { return javascript; }
			set { javascript = value; }
		}		

		public string PageUrl
		{
			get { return pageurl; }
			set { pageurl = value; }
		}		

		public Hashtable Javascripts
		{
			get { return javascripts; }
		}		

		public Hashtable LinkUrls
		{
			get { return linkUrls; }
		}		

		#endregion
	
		#region ===== Private ====================================================
		#endregion

		#region ===== Protected ====================================================
		#endregion

		#region ===== Public ====================================================
		
		#endregion
	}

}