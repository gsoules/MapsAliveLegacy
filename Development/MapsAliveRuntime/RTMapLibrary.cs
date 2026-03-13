// Copyright (C) 2003-2005 AvantLogic Corporation
using System;
using System.IO;
using System.Collections;
using System.Diagnostics;

namespace AvantLogic.MapsAlive.Runtime
{
	public class RTMapLibrary : RTElement
	{
		protected int designId;
		protected string tagId;
		protected Hashtable rtLayers = new Hashtable();
		protected Hashtable backgroundImages = new Hashtable();
		protected Hashtable managedImages = new Hashtable();
		protected string absoluteFileLocation;
		protected string relativeFileLocation;

		#region ===== Constructors ====================================================

		public RTMapLibrary(string absoluteFileLocation, string relativeFileLocation)
		{
			this.absoluteFileLocation = absoluteFileLocation;
			this.relativeFileLocation = relativeFileLocation;
		}
		
		#endregion

		#region ===== Accessors ====================================================
	
		public string AbsoluteFileLocation
		{
			get { return absoluteFileLocation; }
		}
	
		public string RelativeFileLocation
		{
			get { return relativeFileLocation; }
		}

		public Hashtable RTLayers
		{
			get { return rtLayers; }
		}		

		public Hashtable BackgroundImages
		{
			get { return backgroundImages; }
		}

		public Hashtable ManagedImages
		{
			get { return managedImages; }
		}		

		#endregion
	
		#region ===== Public ====================================================

		public void AddManagedImage(string pageTagId, int pageId) 
		{
			if (! managedImages.ContainsKey(pageId)) 
			{
				managedImages.Add(pageId, new ArrayList());
			}
			ArrayList imageList = (ArrayList)managedImages[pageId];
			imageList.Add(pageTagId);
		}

		#endregion
	}
}
