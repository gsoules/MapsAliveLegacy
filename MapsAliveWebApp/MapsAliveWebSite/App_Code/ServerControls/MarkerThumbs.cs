// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AvantLogic.MapsAlive
{
	public class MarkerThumbs : WebControl
	{
		private int columnsPerRow;
		private string thumbList;

		private const int thumbnailSize = 100;
		private const int quickPhotoSize = 300;

		public MarkerThumbs()
		{
			columnsPerRow = 5;
		}

		#region ===== Properties ========================================================

		public int ColumnsPerRow
		{
			get { return columnsPerRow; }
			set { columnsPerRow = value; }
		}

		public Size QuickPhotoSize
		{
			get { return new Size(quickPhotoSize, quickPhotoSize); }
		}

		public string ThumbList
		{
			get { return thumbList; }
			set { thumbList = value; }
		}
		#endregion

		#region ===== Public ============================================================
		#endregion

		#region ===== Protected =========================================================

		protected override void RenderContents(HtmlTextWriter writer)
		{
			// Render the thumb table.
			writer.RenderBeginTag(HtmlTextWriterTag.Table);			//	<table>
			RenderRows(writer);
			writer.RenderEndTag();									//	</table>
		}

		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Div; }
		}
		#endregion

		#region ===== Private ===========================================================

		private void RenderControls(HtmlTextWriter writer, string viewId, string imageId, int column, int imageWidth, int imageHeight, bool hasMarker, bool hasText, bool hasAction)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "thumbControls");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			
			writer.AddAttribute(HtmlTextWriterAttribute.Id, "controls" + viewId);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "thumbControlsUnselected");
			writer.RenderBeginTag(HtmlTextWriterTag.Span);

			string script = "maHandleActionExplicit(" + (int)MemberPageActionId.EditHotspotContent + ",'" + MemberPageAction.ActionPageTarget(MemberPageActionId.EditHotspot) + "?vid=" + viewId + "');";
			
			writer.AddAttribute(HtmlTextWriterAttribute.Onclick, script);
			writer.AddAttribute(HtmlTextWriterAttribute.Src, "../Images/EditSlideButton.gif");
			writer.AddAttribute(HtmlTextWriterAttribute.Title, "Click to edit slide content");
			writer.RenderBeginTag(HtmlTextWriterTag.Img);
			writer.RenderEndTag();
			writer.Write("&nbsp;");

			if (imageHeight > 0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Src, "../Images/Magnify.gif");
				int x = column <= 2 ? -4 : -(imageWidth + 24);
				int y = -(imageHeight + 20);
				script = string.Format("maQuickImageShow(this,'{0}',{1},{2},{3},{4});", ImageSrc(viewId, quickPhotoSize), x, y, imageWidth, imageHeight);
				writer.AddAttribute("onmouseover", script);
				writer.AddAttribute("onmouseout", "javascript:maQuickPreviewHide();");
				writer.RenderBeginTag(HtmlTextWriterTag.Img);
				writer.RenderEndTag();
				writer.Write("&nbsp;");
			}

			if (hasText)
			{
				const int textWidth = 200;
				const int textHeight = 160;
				int x = column <= 2 ? -4 : -(textWidth + 24);
				int y = -(textHeight + 20);
				writer.AddAttribute(HtmlTextWriterAttribute.Src, "../Images/SlideText.gif");
				script = string.Format("maQuickIframeShow(this,'{0}',{1},{2},{3},{4});", TextSrc(viewId), x, y, textWidth, textHeight);
				writer.AddAttribute("onmouseover", script);
				writer.AddAttribute("onmouseout", "javascript:maQuickPreviewHide();");
				writer.RenderBeginTag(HtmlTextWriterTag.Img);
				writer.RenderEndTag();
			}												
			writer.RenderEndTag();

			int srcSuffix = hasMarker ? 1 : 0;
			string idPrefix = hasMarker ? "hasMarker" : "noMarker";
			writer.AddAttribute(HtmlTextWriterAttribute.Src, string.Format("../Images/SlideMarker{0}.gif", srcSuffix));
			writer.AddAttribute(HtmlTextWriterAttribute.Id, idPrefix + viewId);
			writer.AddAttribute(HtmlTextWriterAttribute.Title, "A red ball means this slide's marker is not on the map yet");
			writer.RenderBeginTag(HtmlTextWriterTag.Img);
			writer.RenderEndTag();

			writer.RenderEndTag();
		}

		private void RenderRows(HtmlTextWriter writer)
		{
			if (thumbList.Length == 0)
				return;

			string[] thumb = thumbList.Split((char)0x02);
			int index = 0;
			int lastIndex = thumb.Length - 1;
			int thumbRecords = thumb.Length;
			int rowCount = (int)Math.Ceiling((float)thumbRecords / (float)columnsPerRow);

			for (int row = 1; row <= rowCount; row++)
			{
				// Emit a table row.
				writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
				writer.RenderBeginTag(HtmlTextWriterTag.Tr);

				// Emit the row's cells.
				for (int column = 1; column <= columnsPerRow; column++)
				{
					// Get the data for this cell.
					string[] thumbInfo = thumb[index].Split((char)0x01);
					string imageId = thumbInfo[0];
					string name = thumbInfo[1];
					string viewId = thumbInfo[2];
					int imageWidth = int.Parse(thumbInfo[3]);
					int imageHeight = int.Parse(thumbInfo[4]);
					bool hasMarker = thumbInfo[5] == "1";
					bool hasText = thumbInfo[6] == "1";
					bool hasAction = thumbInfo[7] == "1";

					// Emit the enclosing TD tag for this cell.
					writer.AddAttribute(HtmlTextWriterAttribute.Class, "markerThumbCell");
					writer.AddAttribute("onmouseover", OnShowControls(true, viewId));
					writer.AddAttribute("onmouseout", OnShowControls(false, viewId));

					// Set the column width plus right padding except on the last column.
					int width = thumbnailSize;
					if (column != columnsPerRow)
						width += 17;
					writer.AddStyleAttribute(HtmlTextWriterStyle.Width, width.ToString() + "px");
					writer.RenderBeginTag(HtmlTextWriterTag.Td);

					// Emit the cell contents.
					RenderThumbnail(writer, viewId, imageId, name, imageHeight);
					RenderControls(writer, viewId, imageId, column, imageWidth, imageHeight, hasMarker, hasText, hasAction);

					// Emit the closing TD tag.
					writer.RenderEndTag();

					// Determine if there are any more thumbs to emit.
					index++;
					if (index > lastIndex)
						break;
				}

				writer.RenderEndTag();
			}
		}

		private void RenderThumbnail(HtmlTextWriter writer, string viewId, string imageId, string name, int imageHeight)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "markerThumb");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			string imageSrc;
			if (imageHeight > 0)
				imageSrc = ImageSrc(viewId, thumbnailSize);
			else
				imageSrc = App.WebSitePathUrl("Images/MissingSlideImage.gif");

			writer.AddAttribute(HtmlTextWriterAttribute.Id, "thumb" + viewId);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "markerThumbUnselected");
			writer.AddAttribute(HtmlTextWriterAttribute.Src, imageSrc);
			writer.AddAttribute(HtmlTextWriterAttribute.Onclick, OnClickCall(viewId));
			writer.AddAttribute(HtmlTextWriterAttribute.Title, "Click to locate marker on map");
			writer.RenderBeginTag(HtmlTextWriterTag.Img);
			writer.RenderEndTag();
			
			writer.RenderEndTag();

			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, thumbnailSize + "px");
			writer.AddAttribute(HtmlTextWriterAttribute.Id, "name" + viewId);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "markerThumbNameUnselected");
			writer.AddAttribute(HtmlTextWriterAttribute.Onclick, OnClickCall(viewId));
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.Write(name);
			writer.RenderEndTag();
		}

		private string OnClickCall(string viewId)
		{
			return string.Format("maSelectMarkerThumb({0});", viewId);
		}

		private string OnShowControls(bool show, string viewId)
		{
			return string.Format("maShowThumbControls({0},{1});", show ? 1 : 0, viewId);
		}

		private string ImageSrc(string id, int dimension)
		{
			return string.Format("Thumbnail.ashx?id={0}&dim={1}&ma=1", id, dimension);
		}

		private string TextSrc(string viewId)
		{
			return string.Format("SlideText.ashx?id={0}", viewId);
		}
		#endregion
	}
}
