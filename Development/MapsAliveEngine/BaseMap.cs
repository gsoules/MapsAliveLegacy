// Copyright (C) 2006 AvantLogic Corporation
using System;
using System.Collections;
using System.Drawing;

namespace AvantLogic.MapsAlive.Engine
{
	public class BaseMap
	{
		protected bool usesNativeImages;
		protected int id;
		protected Size size;

		public BaseMap()
		{
		}

		public BaseMap(int id, Size size)
		{
			this.id = id;
			this.size = size;
		}

		#region ===== Properties ========================================================

		public virtual int Id
		{
			get { return id; }
			set { id = value; }
		}

		public virtual Size Size
		{
			get { return size; }
			set	{ size = value;	}
		}

		public virtual bool UsesNativeImages
		{
			get { return usesNativeImages; }
			set { usesNativeImages = value; }
		}
		#endregion

		#region ===== Public ============================================================

		public virtual bool HasImage(int themeId)
		{
			return true;
		}
		#endregion

		#region ===== Protected =========================================================
		#endregion

		#region ===== Private ===========================================================
		#endregion
	}
}
