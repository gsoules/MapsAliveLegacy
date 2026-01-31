// Copyright (C) 2003-2010 AvantLogic Corporation
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AvantLogic.MapsAlive
{
	public class CheckList : WebControl
	{
		public struct ListItem
		{
			public string Title;
			public string ResourceName;
			public string ResourceUrl;
			public int Id;

			public ListItem(string title, string resourceName, string resourceUrl, int id)
			{
				Title = title;
				ResourceName = resourceName;
				ResourceUrl = resourceUrl;
				Id = id;
			}
		}
		
		private string checkedList;
		private	string[] showChecked;
		private ArrayList listItems;

		public CheckList()
		{
			showChecked = new string[0];
			checkedList = string.Empty;
			listItems = new ArrayList();
		}

		public void AddListItem(string title, string resourceName, string resourceUrl, int id)
		{
			ListItem item = new ListItem(title, resourceName, resourceUrl, id);
			listItems.Add(item);
		}

		public void SetChecked(string checkedList)
		{
			this.checkedList = checkedList;
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "textNormal");
			writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
			writer.RenderBeginTag(HtmlTextWriterTag.Table);

			// Get the list of items to be shown as checked.
			if (checkedList.Length > 0)
				showChecked = checkedList.Split(',');

			foreach (ListItem item in listItems)
			{
				writer.RenderBeginTag(HtmlTextWriterTag.Tr);

				writer.RenderBeginTag(HtmlTextWriterTag.Td);
				writer.AddAttribute(HtmlTextWriterAttribute.Value, item.Id.ToString());
				writer.AddAttribute(HtmlTextWriterAttribute.Type, "checkbox");

				if (ItemIsChecked(item.Id.ToString()))
					writer.AddAttribute(HtmlTextWriterAttribute.Checked, "true");
				writer.RenderBeginTag(HtmlTextWriterTag.Input);
				writer.RenderEndTag();
				writer.RenderEndTag();

				writer.RenderBeginTag(HtmlTextWriterTag.Td);
				writer.Write(item.Title);
				writer.RenderEndTag();

				writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingLeft, "12px");
				writer.RenderBeginTag(HtmlTextWriterTag.Td);
				writer.Write(item.ResourceName);
				writer.RenderEndTag();

				writer.AddStyleAttribute(HtmlTextWriterStyle.PaddingLeft, "12px");
				writer.RenderBeginTag(HtmlTextWriterTag.Td);
				writer.AddAttribute(HtmlTextWriterAttribute.Src, item.ResourceUrl);
				writer.RenderBeginTag(HtmlTextWriterTag.Img);
				writer.RenderEndTag();
				writer.RenderEndTag();
				
				writer.RenderEndTag();
			}

			writer.RenderEndTag();
		}

		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Div; }
		}

		private bool ItemIsChecked(string id)
		{
			if (showChecked.Length == 0)
				return false;

			foreach (string s in showChecked)
			{
				if (id == s)
					return true;
			}

			return false;
		}
	}
}
