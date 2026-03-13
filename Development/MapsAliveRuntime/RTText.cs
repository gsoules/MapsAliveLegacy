// Copyright (C) 2003-2005 AvantLogic Corporation
using System;
using System.Drawing;
using System.Collections;

namespace AvantLogic.MapsAlive.Runtime
{
	public class RTText:RTElement
	{
		protected string text;

		#region ===== Constructors ====================================================

		public RTText(string text):base()
		{
			this.text = text;
		}

		#endregion

		#region ===== Accessors ====================================================

		public string Text
		{
			get { return text; }
			set { text = value; }
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