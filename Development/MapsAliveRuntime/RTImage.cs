// Copyright (C) 2003-2005 AvantLogic Corporation
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;

namespace AvantLogic.MapsAlive.Runtime
{
	public class RTImage:RTElement 
	{
		protected Hashtable rtImageLibraryItems = new Hashtable();
		protected int siteImageId;
		protected Bitmap bitmap;
		protected Uri uri;
		protected ImageFormat format;
		protected string absoluteFileLocation;
		protected string relativeFileLocation;
		protected bool renderAsJpg;

		public RTImage(Bitmap bitmap)
		{
			this.Bitmap = bitmap;
			this.Width = bitmap.Width;
			this.Height = bitmap.Height;
		}

		public RTImage(string name, string absoluteFileLocation, string relativeFileLocation)
		{
			this.absoluteFileLocation = absoluteFileLocation;
			this.relativeFileLocation = relativeFileLocation;
		}

		public string AbsoluteFileLocation
		{
			get { return absoluteFileLocation; }
		}
	
		public string RelativeFileLocation
		{
			get { return relativeFileLocation; }
		}

		public Hashtable RTImageLibraryItems 
		{
			get { return rtImageLibraryItems; }
		}

		public int SiteImageId
		{
			get { return siteImageId; }
			set { siteImageId = value; }
		}

		public Uri Uri 
		{
			get { return uri; }
			set { uri = value; }
		}

		public ImageFormat Format 
		{
			get
			{ 
				LoadBitmap();
				return format == null ? bitmap.RawFormat : format; 
			}
		}

		public string AbsolutePath 
		{
			get { return uri.AbsolutePath; }
		}

		public Bitmap Bitmap 
		{
			// Bitmaps are loaded on demand
			get	{ return LoadBitmap();	}
			set { bitmap = value; }
		}

		public bool RenderAsJpg
		{
			get { return renderAsJpg; }
			set { renderAsJpg = value; }
		}
						
		protected Bitmap LoadBitmap() 
		{
			if (bitmap == null)
			{
				try 
				{
					// Open the bitmap as a file.  This action locks the file so we 
					// need to make an in-memory copy and then release it.
					using (Bitmap fileBitmap = (Bitmap)Bitmap.FromFile(uri.LocalPath)) 
					{
						// The copied bitmap gives us access to the bitmap data
						bitmap = new Bitmap(fileBitmap);

						// But ... has the side effect of forgetting the bitmap's
						// original format.  We remember it here for future use.
						// CAVEAT:  The original format can still be checked,
						// but will always return type "memoryBmp".  To determine
						// the correct raw format, the caller must use the RTImage
						// instance's "Format" accessor.
						format = fileBitmap.RawFormat;

						// If this RTImage did not know its size, set it now.
						if (this.width == 0 || this.height == 0)
						{
							this.width = bitmap.Width;
							this.height = bitmap.Height;
						}
					}
				} 
				catch (Exception ex) 
				{
					throw new RuntimeException(string.Format("Unable to obtain a bitmap for {0}.", this.uri.LocalPath), ex);
				}

			}
			return bitmap;
		}
		
	}
}