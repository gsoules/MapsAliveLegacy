// Copyright (C) 2003-2005 AvantLogic Corporation
using System;
using System.Drawing;
using System.Collections;

namespace AvantLogic.MapsAlive.Runtime
{
	public class RTSymbol:RTElement
	{
		protected RTImage rtImage;
		
		#region ===== Constructors ====================================================

		public RTSymbol():base()
		{
			this.Name = "empty_symbol";
		}

		#endregion

		#region ===== Accessors ====================================================
		
		public RTImage RTImage
		{
			get { return rtImage; }
			set { rtImage = value; }
		}

		public int SiteImageId
		{
			get { return rtImage.SiteImageId; }
		}

		public new int Width
		{
			get { return rtImage.Width; }
		}		

		public new int Height
		{
			get { return rtImage.Height; }
		}		

		public Uri Uri 
		{
			get { return rtImage.Uri; }
		}

		public string AbsolutePath 
		{
			get { return rtImage.AbsolutePath; }
		}

		public Bitmap Bitmap 
		{
			get { return rtImage.Bitmap; }
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