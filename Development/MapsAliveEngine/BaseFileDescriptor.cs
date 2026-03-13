// Copyright (C) 2006 AvantLogic Corporation
using System;
using System.Diagnostics;
using System.Drawing;

namespace AvantLogic.MapsAlive.Engine
{
	public class BaseFileDescriptor : ICloneable
	{
		protected long bytes;
		protected string fileLocation;
		protected int id;
		protected string name;
		protected BaseFileDescriptor originalFileDescriptor;
		protected string rootPath;
		protected Size size;
		protected long timeStamp;

		public BaseFileDescriptor()
		{
		}

		public BaseFileDescriptor(int id, string fileLocation)
		{
			this.id = id;
			this.fileLocation = fileLocation;
		}

		public virtual void Release()
		{
		}

		#region ===== Properties ========================================================

		public long Bytes
		{
			get { return bytes; }
			set { bytes = value; }
		}

		public virtual string FileLocation
		{
			get { return fileLocation; }
		}

		public virtual string FileLocationRelative
		{
			get
			{
				if (rootPath == null)
					return new System.IO.FileInfo(FileLocation).Name;
				else
					return FileLocation.Substring(rootPath.Length + 1).Replace("\\", "/");
			}
		}

		public int Id
		{
			get { return id; }
			set { id = value; }
		}

		public string Kb
		{
			get
			{
				if (bytes < 1024)
				{
					return string.Format("{0} bytes", bytes);
				}
				else
				{
					double kb = (double)bytes / 1024;
					return string.Format("{0:f1} KB", kb);
				}
			}
		}

		public string LastModifiedDate
		{
			get
			{
				DateTime dt = new DateTime(timeStamp);
				return dt.ToString();
			}
		}

		public string Name
		{
			get { return name; }
		}

		public virtual BaseFileDescriptor OriginalBaseFileDescriptor
		{
			get	{ return originalFileDescriptor; }
		}

		public string RootPath
		{
			set { rootPath = value; }
		}

		public Size Size
		{
			get { return size; }
			set
			{
				Debug.Assert(value.Width != 0 && value.Height != 0, "Attempt to give a file a 0 width or height");
				size = value;
			}
		}
		#endregion

		#region ===== Public ============================================================

		public object Clone()
		{
			return MemberwiseClone();
		}
		#endregion

		#region ===== Protected =========================================================
		#endregion

		#region ===== Private ===========================================================
		#endregion

	}
}
