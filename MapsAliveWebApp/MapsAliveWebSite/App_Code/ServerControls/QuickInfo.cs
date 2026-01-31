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
	public class QuickInfo : WebControl
	{
		private int offsetX;
		private int offsetY;
		private string topic;
		private string url;

		public QuickInfo()
		{
			offsetX = 24;
			offsetY = -4;
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

		public string Topic
		{
			set { topic = value; }
		}

		public string Url
		{
			set { url = value; }
		}

		#endregion

		#region ===== Public ============================================================
		#endregion

		#region ===== Protected =========================================================

		protected override void RenderContents(HtmlTextWriter writer)
		{
			string details = string.Empty;
			bool showDetailsLink = !string.IsNullOrEmpty(url);
			if (showDetailsLink)
			{
				string instructions = "For detailed information, click the Info icon now.";
				details = string.Format("<div class='finePrint' style='padding-top:4px;margin-top:6px;border-top:dashed 1px white;'><b>{0}</b></div>", instructions);
			}
			string topicId = "QuickInfo" + (topic == null ? this.ID : topic);
			string text = Utility.QuickHelpText(topicId, details);
			string script = string.Format("javascript:maQuickHelpShow(this,'{0}',{1},{2},true);", text, offsetX, offsetY);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "quickHelpImg");
			writer.AddAttribute(HtmlTextWriterAttribute.Src, App.WebSitePathUrl("Images/QuickInfo.png"));
			writer.AddAttribute("onmouseover", script);
			writer.AddAttribute("onmouseout", "javascript:maQuickHelpHide(true);");
			if (showDetailsLink)
				writer.AddAttribute("onclick", string.Format("maTransferToPage('{0}');", url));
			writer.RenderBeginTag(HtmlTextWriterTag.Img);
			writer.RenderEndTag();
		}


		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Span; }
		}
		#endregion

		#region ===== Private ===========================================================
		#endregion
	}
}
