// Copyright (C) 2003-2005 AvantLogic Corporation
using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Diagnostics;

namespace AvantLogic.MapsAlive.Runtime
{
	public abstract class RTElement
	{
		protected string name = "";
		protected int id;
		protected int x = 0;
		protected int y = 0;
		protected int width = 0;
		protected int height = 0;

		#region ===== Constructors ====================================================

		public RTElement()
		{
		}

		#endregion

		#region ===== Accessors ====================================================
	
		public virtual string Name
		{
			get { return name; }
			set { name = value; }
		}		

		public int Id
		{
			get { return id; }
			set { id = value; }
		}		

		public int X 
		{
			get { return x; }
			set { x = value; }
		}

		public int Y 
		{
			get { return y; }
			set { y = value; }
		}

		public int Width
		{
			get { return width; }
			set { width = value; }
		}		

		public int Height
		{
			get { return height; }
			set { height = value; }
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
