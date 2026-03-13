// Copyright (C) 2003-2005 AvantLogic Corporation
using System;
using System.IO;
using System.Collections;
using System.Diagnostics;

namespace AvantLogic.MapsAlive.Runtime
{
	public class RTTheme
	{
		protected string name = "";
		protected int id;

		#region ===== Constructors ====================================================

		public RTTheme()
		{
		}

		#endregion

		#region ===== Accessors ====================================================

		public string Name
		{
			get { return name; }
			set { name = value.Replace(" ","").ToLower(); }
		}		

		public int Id
		{
			get { return id; }
			set { id = value; }
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
