// Copyright (C) 2006 AvantLogic Corporation
using System;
using System.Drawing;
using System.IO;
using System.Net;
using AvantLogic.MapsAlive.Engine;

namespace AvantLogic.MapsAlive.Engine
{
	public class BitmapInfo
	{
		private Bitmap bitmap;
		private long bytes;
		private string fileLocation;
		private string hash;
		private string imageFormat;
		private long lastWriteTimeInTicks;
		private Size size;

		public BitmapInfo(Bitmap bitmap, Size size, long bytes, string imageFormat, long lastWriteTimeInTicks)
		{
			this.bitmap = bitmap;
			this.size = size;
			this.bytes = bytes;
			this.imageFormat = imageFormat;
			this.lastWriteTimeInTicks = lastWriteTimeInTicks;
			this.fileLocation = null;
			ComputeHash();
		}

		#region ===== Properties ========================================================

		public Bitmap Bitmap
		{
			get { return bitmap; }
		}

		public long Bytes
		{
			get { return bytes; }
			set { bytes = value; }
		}

		public string FileLocation
		{
			get { return fileLocation; }
			set { fileLocation = value; }
		}

		public string Hash
		{
			get { return hash; }
			set { hash = value; }
		}

		public string ImageFormat
		{
			get { return imageFormat; }
		}

		public string LastWriteTime
		{
			get
			{
				DateTime dt = new DateTime(lastWriteTimeInTicks);
				return dt.ToString();
			}
		}

		public long LastWriteTimeInTicks
		{
			get { return lastWriteTimeInTicks; }
		}

		public Size Size
		{
			get { return size; }
			set { size = value; }
		}
		#endregion

		#region ===== Public ============================================================

		public static string BitmapFormat(Bitmap bitmap)
		{
			System.Drawing.Imaging.ImageFormat format = bitmap.RawFormat;
			string formatName = "unknown";
			if (format.Equals(System.Drawing.Imaging.ImageFormat.Bmp)) formatName = "bmp";
			else if (format.Equals(System.Drawing.Imaging.ImageFormat.Emf)) formatName = "emf";
			else if (format.Equals(System.Drawing.Imaging.ImageFormat.Exif)) formatName = "exif";
			else if (format.Equals(System.Drawing.Imaging.ImageFormat.Gif)) formatName = "gif";
			else if (format.Equals(System.Drawing.Imaging.ImageFormat.Icon)) formatName = "icon";
			else if (format.Equals(System.Drawing.Imaging.ImageFormat.Jpeg)) formatName = "jpeg";
			else if (format.Equals(System.Drawing.Imaging.ImageFormat.MemoryBmp)) formatName = "memorybmp";
			else if (format.Equals(System.Drawing.Imaging.ImageFormat.Png)) formatName = "png";
			else if (format.Equals(System.Drawing.Imaging.ImageFormat.Tiff)) formatName = "tiff";
			else if (format.Equals(System.Drawing.Imaging.ImageFormat.Wmf)) formatName = "wmf";
			return formatName;
		}

		public static BitmapInfo BitmapInfoFromFile(string fileLocation)
		{
			BitmapInfo bitmapInfo;
			try
			{
				// Create a temporary bitmap from the file and return a copy of it.  Dispose of
				// the temporary copy so that the file won't remain locked as the live backing
				// for the bitmap which would prevent the user from modifying the file.  This
				// way the bitmap data is kept in memory instead of in the file.  We have to get
				// the format from the temp bitmap because it contains the file's format -- the
				// copied bitmap always has a format of MemoryBmp.
				using (Bitmap fileBitmap = new Bitmap(fileLocation))
				{
					Bitmap memoryBitmap = new Bitmap(fileBitmap);
					long bytes = BaseFileManager.FileBytes(fileLocation);
					long ticks = BaseFileManager.LastWriteTimeInTicks(fileLocation);
					bitmapInfo = new BitmapInfo(memoryBitmap, fileBitmap.Size, bytes, BitmapFormat(fileBitmap), ticks);
				}
			}
			catch
			{
				bitmapInfo = null;
			}
			return bitmapInfo;
		}

		public static BitmapInfo BitmapInfoFromUri(string uri)
		{
			BitmapInfo bitmapInfo = null;
			try
			{
				Stream ImageStream = new WebClient().OpenRead(uri);
				Bitmap bitmap = (Bitmap)Image.FromStream(ImageStream);
				long bytes = 0;
				bitmapInfo = new BitmapInfo(bitmap, bitmap.Size, bytes, BitmapInfo.BitmapFormat(bitmap), 0);
			}
			catch
			{
			}
			return bitmapInfo;
		}
		#endregion

		#region ===== Protected =========================================================
		#endregion

		#region ===== Private ===========================================================

		private void ComputeHash()
		{
			ImageConverter ic = new ImageConverter();
			byte[] byteArray = new byte[1];
			hash = BaseUtility.Hash((byte[])ic.ConvertTo(bitmap, byteArray.GetType()));
		}
		#endregion
	}
}
