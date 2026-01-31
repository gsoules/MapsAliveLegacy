// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace AvantLogic.MapsAlive
{
	public class GetStartedLink : WebControl
	{
		private string src;
		private string href;
		private string title;
		private bool newWindow;

		public GetStartedLink()
		{
		}

		public void SetLink(string src, string href, string title, bool newWindow)
		{
			this.src = src;
			this.href = href;
			this.title = title;
			this.newWindow = newWindow;
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "getStartedIcon");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.AddAttribute(HtmlTextWriterAttribute.Src, src);
			writer.RenderBeginTag(HtmlTextWriterTag.Img);
			writer.RenderEndTag();
			writer.RenderEndTag();
			writer.RenderEndTag();
			
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.AddAttribute(HtmlTextWriterAttribute.Href, href);
			if (newWindow)
				writer.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
			writer.RenderBeginTag(HtmlTextWriterTag.A);
			writer.Write(title);
			writer.RenderEndTag();
			writer.RenderEndTag();
		}

		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Tr; }
		}
	}
}
