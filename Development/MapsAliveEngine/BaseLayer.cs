// Copyright (C) 2006 AvantLogic Corporation
using System;
using System.Collections.Generic;
using System.Text;

namespace AvantLogic.MapsAlive.Engine
{
	public class BaseLayer
	{
		int id;
		BaseFileDescriptor imageFileDescriptor;
		BaseMap map;
		protected string name;
		protected int zIndex;
			
		public BaseLayer()
		{
		}

		public BaseLayer(BaseMap map, string name, int id, BaseFileDescriptor imageFileDescriptor)
		{
			this.map = map;
			this.name = name;
			this.id = id;
			this.imageFileDescriptor = imageFileDescriptor;
		}

		#region ===== Properties ========================================================

		public virtual int Id
		{
			get { return id; }
			set { id = value; }
		}

		public virtual string Name
		{
			get { return name; }
			set { name = value; }
		}

		public virtual int ZIndex
		{
			get { return zIndex; }
			set { zIndex = value; }
		}
		#endregion

		#region ===== Public ============================================================

		public virtual bool HasImage(int themeId)
		{
			return false;
		}

		public virtual BaseFileDescriptor ImageFileDescriptor(int themeId)
		{
			return imageFileDescriptor;
		}
		#endregion

		#region ===== Protected =========================================================
		#endregion

		#region ===== Private ===========================================================
		#endregion
	}
}
