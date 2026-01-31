// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AvantLogic.MapsAlive
{
	public class ColorSwatch : WebControl, IPostBackDataHandler
	{
		private string colorValue;
		private string colorValueControlClientId;
		private string errorMessage;
		private bool forPreview;
		private string swatchControlClientId;

		public ColorSwatch()
		{
			colorValue = string.Empty;
			errorMessage = string.Empty;
		}

		protected override void OnInit(EventArgs e)
		{
			string clientId = ClientID + ClientIDSeparator;
			colorValueControlClientId = clientId + "Value";
			swatchControlClientId = clientId + "Swatch";
			
			Page.RegisterRequiresPostBack(this);
			base.OnInit(e);
		}

		public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
		{
			colorValue = postCollection[colorValueControlClientId];
			return false;
		}

		public void RaisePostDataChangedEvent()
		{
		}

		public string ColorValue
		{
			get	{ return colorValue.Trim();	}
			set	{ colorValue = value; }
		}

		public string ErrorMessage
		{
			set { errorMessage = value; }
		}

		public bool ForPreview
		{
			set { forPreview = value; }
		}

		public bool Col { get; set; }
		public bool Row { get; set; }
		public string Label { get; set; }
		public int LabelWidth { get; set; }
		public string QuickHelpTitle { get; set; }

		protected override void RenderContents(HtmlTextWriter writer)
		{
			if (Row || Col)
			{
				if (Row)
					writer.RenderBeginTag(HtmlTextWriterTag.Tr);

				if (LabelWidth != 0)
					writer.AddStyleAttribute(HtmlTextWriterStyle.Width, LabelWidth + "px;");
				
				if (Col)
					writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "150px");
				
				writer.AddStyleAttribute(HtmlTextWriterStyle.TextAlign, "right");
				writer.RenderBeginTag(HtmlTextWriterTag.Td);

				writer.AddAttribute(HtmlTextWriterAttribute.Class, "controlLabel");
				writer.RenderBeginTag(HtmlTextWriterTag.Span);
				writer.Write(this.Label);
				writer.RenderEndTag();
				
				writer.RenderEndTag();
				
				if (Col)
					writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "130px");
				writer.RenderBeginTag(HtmlTextWriterTag.Td);
				RenderSwatch(writer);

				writer.Write("&nbsp;");

				QuickHelpTitle quickHelpTitle = new QuickHelpTitle();
				quickHelpTitle.Span = true;
				quickHelpTitle.ID = QuickHelpTitle;
				quickHelpTitle.RenderControl(writer);

				writer.RenderEndTag();
				
				if (Row)
					writer.RenderEndTag();
			}
			else
			{
				RenderSwatch(writer);
			}
		}

		private void RenderSwatch(HtmlTextWriter writer)
		{
			string forPreviewValue = forPreview ? "true" : "false";

			TextBox textBox = new TextBox();
			textBox.ID = colorValueControlClientId;
			textBox.Width = 60;
			textBox.Text = colorValue;
			writer.AddAttribute("onkeyup", string.Format("maOnEditColor('{0}',this,{1});", swatchControlClientId, forPreviewValue));
			textBox.RenderControl(writer);

			Label label = new Label();
			label.CssClass = "textErrorMessage";
			label.Text = errorMessage;
			label.RenderControl(writer);

			writer.AddAttribute(HtmlTextWriterAttribute.Id, swatchControlClientId);
			writer.AddAttribute("onclick", string.Format("maChooseColor('{0}','{1}',{2});", swatchControlClientId, colorValueControlClientId, forPreviewValue));
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "colorSwatch");
			writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, colorValue);
			writer.RenderBeginTag(HtmlTextWriterTag.Span);
			writer.Write("&nbsp;&nbsp;&nbsp;");
			writer.RenderEndTag();
		}

		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Span; }
		}
	}
}
