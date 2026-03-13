// Copyright (C) 2003-2005 AvantLogic Corporation
using System;
using System.Collections;
using System.Diagnostics;

namespace AvantLogic.MapsAlive.Runtime
{
	public abstract class RTElements
	{
		protected SortedList elementList = new SortedList();
		protected SortedList pageList = new SortedList();
		protected SortedList viewList = new SortedList();
		protected SortedList themeList = new SortedList();
		protected int designId;
		protected string tagId;
		protected int x = 0;
		protected int y = 0;
		protected int width = 0;
		protected int height = 0;
		
		public RTElements()
		{
		}

		#region ===== Properties ========================================================
		
		public int DesignId
		{
			get { return designId; }
			set { designId = value; }
		}	

		public string TagId
		{
			get { return tagId; }
			set { tagId = value; }
		}

		public int X 
		{
			get { return x; }
			set { x = value; }
		}

		public int Y 
		{
			get { return y; }
			set { y = value; }
		}

		public int Width
		{
			get { return width; }
			set { width = value; }
		}		

		public int Height
		{
			get { return height; }
			set { height = value; }
		}

		public ICollection PageIds
		{
			get { return pageList.Keys; }
		}
		
		public ICollection ViewIds
		{
			get { return viewList.Keys; }
		}

		public ICollection ThemeIds
		{
			get { return themeList.Keys; }
		}

		#endregion

		#region ===== Public methods ====================================================
		#endregion

		#region ===== Private methods =================================================

		private string CreateElementKey(int pageId, int viewId, int themeId) 
		{
			return string.Format("{0}_{1}_{2}", pageId, viewId, themeId);
		}
		
		private int CreatePageThemeId(int pageId, int themeId) 
		{
			return string.Format("{0}_{1}", pageId, themeId).GetHashCode();
		}

		private void AddRTElement(RTElement rtElement, SortedList sortedList, int id, string elementKey)
		{
			if (! sortedList.ContainsKey(id)) 
			{
				sortedList.Add(id, new SortedList());
			}
			SortedList elementList = (SortedList)sortedList[id];
			if (! elementList.Contains(elementKey)) 
			{
				elementList.Add(elementKey, rtElement);
			}
		}

		#endregion

		#region ===== Protected methods =================================================
		
		protected object GetRTElement(int pageId, int viewId, int themeId) 
		{
			return elementList[CreateElementKey(pageId, viewId, themeId)];
		}

		protected SortedList GetRTElements() 
		{
			return elementList;
		}

		protected SortedList GetRTElementsForPage(int pageId) 
		{
			return (SortedList)pageList[pageId];
		}

		protected SortedList GetRTElementsForView(int viewId) 
		{
			return (SortedList)viewList[viewId];
		}

		protected SortedList GetRTElementsForTheme(int themeId) 
		{
			return (SortedList)themeList[themeId];
		}

		protected SortedList GetMatchingRTElements(SortedList elementList1, SortedList elementList2) 
		{
			SortedList matchingElementList = new SortedList();
			foreach(string elementKey in elementList1.Keys) 
			{
				if (elementList2.ContainsKey(elementKey)) 
				{
					matchingElementList.Add(elementKey, elementList2[elementKey]);
				}
			}
			return matchingElementList;
		}

		protected void AddRTElement(RTElement rtElement, int pageId, int viewId, int themeId)
		{
			string elementKey = CreateElementKey(pageId, viewId, themeId);
			if (! elementList.ContainsKey(elementKey)) 
			{
				elementList.Add(elementKey, rtElement);
			}

			AddRTElement(rtElement, pageList, pageId, elementKey);
			AddRTElement(rtElement, viewList, viewId, elementKey);
			AddRTElement(rtElement, themeList, themeId, elementKey);
		}

		#endregion

		#region ===== Protected methods =================================================
		
		#endregion

	}
}
