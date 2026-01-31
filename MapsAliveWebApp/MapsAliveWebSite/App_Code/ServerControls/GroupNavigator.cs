// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml;

namespace AvantLogic.MapsAlive
{
	public class GroupNavigator : WebControl
	{
		private int rootId;
		private string selectedGroupId;
		private string selectedItemId;
		private bool showGroupsExpanded;
		private string xml;
		private XmlDocument xmlDoc;

		public GroupNavigator()
		{
            // This method sometimes gets called right after an Application_Start has occurred and gets an
            // "Object reference not set to an instance of an object" exception on the line that assigns the
            // showGroupsExpanded flag. The exception then triggers an unexpected MapsAlive error. It's not
            // clear what's going on, but it could be a timing problem where the state is not yet set. Since
            // all this constructor does is create an empty XmlDocument and set a flag, hopefully catching
            // the exception will prevent the unexpected error.
            try
            {
                xmlDoc = new XmlDocument();
	    		showGroupsExpanded = MapsAliveState.Account.ShowTourNavigatorExpanded;
            }
            catch
            {
                showGroupsExpanded = false;
            }
		}

		public void SetSelectedGroup(string nodeId)
		{
			selectedGroupId = nodeId;
		}

		public void SetSelectedItem(string nodeId)
		{
			selectedItemId = nodeId;
		}

		public void SetRootId(int rootId)
		{
			this.rootId = rootId;
		}

		public string Xml
		{
			set
			{
				xml = value;
				xmlDoc.LoadXml(xml);
			}
		}

		private string CreateMouseOverScript(string itemId)
		{
			int x = 140;
			int y = -50;
			int imageWidth = 100;
			int imageHeight = 100;
			string viewId = itemId.Substring(1);
			string script = string.Format("maQuickImageShow(this,'{0}',{1},{2},{3},{4});",
				ImageSrc(viewId, 300), x, y, imageWidth, imageHeight);
			return script;
		}

		private static string GetAttributeValue(XmlElement element, string name)
		{
			string value = string.Empty;
			XmlAttribute attribute = element.Attributes[name];
			if (attribute != null)
				value = attribute.Value;
			return value;
		}

		private string GetText(XmlElement element)
		{
			return GetAttributeValue(element, "Text");
		}

		private string GetValue(XmlElement element)
		{
			return GetAttributeValue(element, "Value");
		}

		private string ImageSrc(string id, int dimension)
		{
			return string.Format("Thumbnail.ashx?id={0}&dim={1}&ma=1", id, dimension);
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			XmlNode rootNode = xmlDoc.SelectSingleNode("/Tree/Node");

			RenderRoot(writer, rootNode);

			if (rootNode == null)
				return;
			
			foreach (XmlElement groupElement in rootNode.ChildNodes)
			{
				RenderGroup(writer, groupElement);
			}
		}

		private void RenderGroup(HtmlTextWriter writer, XmlElement groupElement)
		{
			string groupName = GetText(groupElement);
			if (groupName == string.Empty)
				return;

			string groupId = GetValue(groupElement);
			bool isDataSheet = GetAttributeValue(groupElement, "MapImageId") == "0";

			// Render a div that will contain the expand/collapse graphic and the page name.
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "groupNavSection");
			writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "relative");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			if (isDataSheet || groupElement.ChildNodes.Count == 0)
			{
				// Render a non-expanding/collapsing graphic.
				writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "absolute");
				writer.AddAttribute(HtmlTextWriterAttribute.Class, isDataSheet ? "groupNavDataSheet" : "groupNavChildless");
				writer.RenderBeginTag(HtmlTextWriterTag.Div);
				writer.RenderEndTag();
			}
			else
			{
				// Render the expand/collapse graphic.
				writer.AddAttribute(HtmlTextWriterAttribute.Onclick, string.Format("maToggleGroupNavigator(this, '{0}');", groupId));
				writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "absolute");
				writer.AddAttribute(HtmlTextWriterAttribute.Class, showGroupsExpanded || groupId == selectedGroupId ? "groupNavExpanded" : "groupNavCollapsed");
				writer.RenderBeginTag(HtmlTextWriterTag.Div);
				writer.RenderEndTag();
			}
			
			// Render the group name.
			string className = "groupNavGroupName";
			if (groupId == selectedGroupId)
				className += "Selected";
			else if (isDataSheet)
				className += "DataSheet";
			writer.AddAttribute(HtmlTextWriterAttribute.Onclick, string.Format("maOnClickTourNavigator('{0}');", groupId));
			writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "absolute");
			writer.AddAttribute(HtmlTextWriterAttribute.Class, className);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.Write(groupName);
			writer.RenderEndTag();
			
			writer.RenderEndTag();

			// Render the group's items unless the group is for a data sheet. A data sheet page has 0 for its map image Id.
			if (!isDataSheet)
			{
				// Render a div that will contain the group's item names.
				writer.AddAttribute(HtmlTextWriterAttribute.Id, groupId);
				writer.AddStyleAttribute(HtmlTextWriterStyle.Display, showGroupsExpanded || groupId == selectedGroupId ? "block" : "none");
				writer.RenderBeginTag(HtmlTextWriterTag.Div);

				foreach (XmlElement itemElement in groupElement.ChildNodes)
				{
					RenderItem(writer, itemElement);
				}

				writer.RenderEndTag();
			}
		}

		private void RenderItem(HtmlTextWriter writer, XmlElement itemElement)
		{
			string itemName = GetText(itemElement);
			string itemId = GetValue(itemElement);
			string className = "groupNavItemName";
			if (itemId == selectedItemId)
				className += "Selected";

			//string onMouseOverScript = CreateMouseOverScript(itemId);
			//writer.AddAttribute("onmouseover", onMouseOverScript);
			//writer.AddAttribute("onmouseout", "javascript:maQuickPreviewHide();");

			writer.AddAttribute(HtmlTextWriterAttribute.Onclick, string.Format("maOnClickTourNavigator('{0}');", itemId));
			writer.AddAttribute(HtmlTextWriterAttribute.Class, className);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.Write(itemName);
			writer.RenderEndTag();
		}

		private void RenderRoot(HtmlTextWriter writer, XmlNode rootNode)
		{
			string rootName;

			if (rootNode == null)
			{
				rootName = "(There is no current tour)";
			}
			else
			{
				rootName = GetText((XmlElement)rootNode);
				string groupId = GetValue((XmlElement)rootNode);
				writer.AddAttribute(HtmlTextWriterAttribute.Onclick, string.Format("maOnClickTourNavigator('{0}');", groupId));
			}

			writer.AddAttribute(HtmlTextWriterAttribute.Class, rootName.Length <= 22 ? "groupNavParentName" : "groupNavParentNameSmall");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.Write(rootName);
			writer.RenderEndTag();
		}

		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Div; }
		}
	}
}
