// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace AvantLogic.MapsAlive
{
	public class QuickHelpTitle : WebControl
	{
		private int offsetX;
		private int offsetY;
		private HtmlTextWriterTag outerTag;
		private string title;
		private string topic;
		private string topMargin;

		public QuickHelpTitle()
		{
			offsetX = 0;
			offsetY = 0;
			title = string.Empty;
			topMargin = "12px";
			outerTag = HtmlTextWriterTag.Div;
		}

		#region ===== Properties ========================================================

		public int OffsetX
		{
			set { offsetX = value; }
		}

		public int OffsetY
		{
			set { offsetY = value; }
		}

		public bool Span
		{
			set	{ outerTag = value ? HtmlTextWriterTag.Span : HtmlTextWriterTag.Div; }
		}

		public string Topic
		{
			set { topic = value; }
		}

		public string Title
		{
			set { title = value; }
		}

		public string TopMargin
		{
			set { topMargin = value; }
		}

		#endregion

		#region ===== Public ============================================================
		#endregion

		#region ===== Protected =========================================================

		protected override void RenderContents(HtmlTextWriter writer)
		{
			writer.AddStyleAttribute(HtmlTextWriterStyle.MarginTop, topMargin);
			writer.AddStyleAttribute(HtmlTextWriterStyle.MarginBottom, "2px");

			writer.RenderBeginTag(outerTag);

			if (title.Length > 0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "quickHelpTitle");
				writer.RenderBeginTag(HtmlTextWriterTag.Span);	//		<span>
				writer.Write(title + "&nbsp;");
				writer.RenderEndTag();							//		</span>
			}

			string topicId = "QuickHelp" + (topic == null ? this.ID : topic);
			string script = string.Format("javascript:maQuickHelpShow(this,'{0}',{1},{2});", Utility.QuickHelpText(topicId), offsetX, offsetY);
			writer.AddAttribute(HtmlTextWriterAttribute.Src, App.WebSitePathUrl("Images/QuickHelp.png"));
			writer.AddAttribute("onmouseover", script);
			writer.AddAttribute("onmouseout", "javascript:maQuickHelpHide();");
			writer.RenderBeginTag(HtmlTextWriterTag.Img);	//		<img>
			writer.RenderEndTag();							//		</img>

			writer.RenderEndTag(); // outer tag
		}


		protected override HtmlTextWriterTag TagKey
		{
			get { return outerTag; }
		}
		#endregion

		#region ===== Private ===========================================================
		#endregion
	}
}
