// Copyright (C) 2003-2005 AvantLogic Corporation
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;

namespace AvantLogic.MapsAlive.Runtime
{
	public class RTImageLibrary : RTElements
	{
		protected string absoluteFileLocation;
		protected string relativeFileLocation;
		
		#region ===== Constructors ====================================================

		public RTImageLibrary(string absoluteFileLocation, string relativeFileLocation)
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

		#endregion

		#region ===== Public methods ====================================================

		public RTImageLibraryItem GetRTImageLibraryItem(int pageId, int viewId, int themeId) 
		{
			return (RTImageLibraryItem)GetRTElement(pageId, viewId, themeId);
		}

		public SortedList GetRTImageLibraryItems() 
		{
			return GetRTElements();
		}

		public SortedList GetRTImageLibraryItemsForTheme(int themeId) 
		{
			return GetRTElementsForTheme(themeId);
		}

		public SortedList GetRTImageLibraryItemsForPage(int pageId) 
		{
			return GetRTElementsForPage(pageId);
		}

		public SortedList GetRTImageLibraryItemsForView(int viewId) 
		{
			return GetRTElementsForView(viewId);
		}
		
		public SortedList GetMatchingRTImageLibraryItems(SortedList imageInstanceList1, SortedList imageInstanceList2) 
		{
			return GetMatchingRTElements(imageInstanceList1, imageInstanceList2);
		}

		public void AddRTImageLibraryItem(RTImageLibraryItem rtImageLibraryItem)
		{
			this.AddRTElement(rtImageLibraryItem, rtImageLibraryItem.PageId, rtImageLibraryItem.ViewId, rtImageLibraryItem.ThemeId);
			
			// Update the size of this library to be as wide as the widest image and as tall as the tallest image.
			if (rtImageLibraryItem.FixedWidth > this.Width)
				this.Width = rtImageLibraryItem.FixedWidth;
			if (rtImageLibraryItem.FixedHeight > this.Height)
				this.Height = rtImageLibraryItem.FixedHeight;
		}
		
		#endregion

		#region ===== Protected methods ====================================================
		#endregion

	}
}
