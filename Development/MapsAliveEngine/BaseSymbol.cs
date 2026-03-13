// Copyright (C) 2006 AvantLogic Corporation
using System;
using System.Diagnostics;
using System.Drawing;

namespace AvantLogic.MapsAlive.Engine
{
	public class BaseSymbol
	{
		protected BitmapInfo bitmapInfo;
		protected BaseFileDescriptor fileDescriptor;
		protected int id;
		protected string name;
		protected bool renderAsJpg;

		public BaseSymbol()
		{
		}

		public BaseSymbol(int id, Bitmap bitmap)
		{
			this.name = string.Empty;
			this.id = id;
			long bytes = 0;
			bitmapInfo = new BitmapInfo(bitmap, bitmap.Size, bytes, BitmapInfo.BitmapFormat(bitmap), 0);
		}

		public BaseSymbol(string name, int id, BaseFileDescriptor fileDescriptor)
		{
			this.name = name;
			this.id = id;
			this.fileDescriptor = fileDescriptor;
			Construct();
		}

		protected void Construct()
		{
			bitmapInfo = new BitmapInfo(null, Size.Empty, 0, null, 0);
		}

		public Bitmap Bitmap
		{
			get
			{
				if (bitmapInfo.Bitmap == null)
				{
					if (!LoadSymbolImage())
						LoadDefaultSymbolImage();
				}
				Debug.Assert(bitmapInfo != null);
				return bitmapInfo.Bitmap;
			}
		}

		public BaseFileDescriptor BaseFileDescriptor
		{
			get { return fileDescriptor; }
			set { fileDescriptor = value; }
		}

		public int Id
		{
			get { return id; }
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public bool RenderAsJpg
		{
			get { return renderAsJpg; }
			set { renderAsJpg = value; }
		}

		public bool LoadSymbolImage()
		{
			BitmapInfo newBitmapInfo = BitmapInfo.BitmapInfoFromFile(fileDescriptor.FileLocation);

			if (newBitmapInfo == null)
				return false;
			else
				bitmapInfo = newBitmapInfo;

			return true;
		}

		protected virtual void LoadDefaultSymbolImage()
		{
			bitmapInfo = null;
		}
	}
}
