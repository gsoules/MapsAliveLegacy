// Copyright (C) 2003-2005 AvantLogic Corporation
using System;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Diagnostics;

namespace AvantLogic.MapsAlive.Runtime
{
	public class RTImageLibraryItem:RTElement
	{
		// Ids are sequential
		private static int itemId = 1;

		protected int pageId;
		protected int viewId;
		protected int themeId;
		protected Point offset;
		protected Size fixedSize;
		protected RTImage rtImage;

		#region ===== Constructors ====================================================

		public RTImageLibraryItem(RTImage rtImage, int pageId, int viewId, int themeId)
		{
			this.id = itemId++;
			this.rtImage = rtImage;
			this.pageId = pageId;
			this.viewId = viewId;
			this.ThemeId = themeId;
			this.offset = new Point(0,0);
			this.fixedSize = new Size(0,0);
			this.x = rtImage.X;
			this.y = rtImage.Y;
			this.width = rtImage.Width;
			this.height = rtImage.Height;
		}

		#endregion

		#region ===== Accessors ====================================================
	
		public RTImage RTImage 
		{
			get { return rtImage; }
			set { rtImage = value; }
		}

		public int PageId 
		{
			get { return pageId; }
			set { pageId = value; }
		}

		public int ViewId 
		{
			get { return viewId; }
			set { viewId = value; }
		}

		public int ThemeId 
		{
			get { return themeId; }
			set { themeId = value; }
		}

		public Point Offset
		{
			get { return offset; }
			set { offset = value; }
		}
		
		public int OffsetX
		{
			get { return offset.X; }
			set { offset.X = value; }
		}

		public int OffsetY
		{
			get { return offset.Y; }
			set { offset.Y = value; }
		}

		public int FixedWidth 
		{
			get { return fixedSize.Width; }
			set { fixedSize.Width = value; }
		}
		
		public int FixedHeight 
		{
			get { return fixedSize.Height; }
			set { fixedSize.Height = value; }
		}

		public Size FixedSize 
		{
			get { return fixedSize; }
			set { fixedSize = value; }
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
