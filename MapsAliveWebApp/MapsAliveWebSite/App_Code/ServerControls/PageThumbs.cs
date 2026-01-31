// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Drawing;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace AvantLogic.MapsAlive
{
	public class PageThumbs : WebControl
	{
		private int columnsPerRow;
		private int previewDimension;
		private bool showTourThumbs;
		private int scaledDimension;
		private string thumbList;

		public PageThumbs()
		{
			columnsPerRow = 4;
			scaledDimension = 116;
			previewDimension = 300;
			thumbList = string.Empty;
		}

		#region ===== Properties ========================================================

		public int ColumnsPerRow
		{
			set { columnsPerRow = value; }
		}

		public bool ShowTourThumbs
		{
			set { showTourThumbs = value; }
		}

		public string ThumbList
		{
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

		private string ClickScript(int tourId, int pageId)
		{
			string onClickScript;
			if (showTourThumbs)
				onClickScript = "maSafeTransfer('/Members/ManageTour.ashx?id=" + tourId + "');";
			else
				onClickScript = "maHandleAction('" + MemberPageAction.ActionPageTarget(MemberPageActionId.EditPage) + "?pid=" + pageId.ToString() + "');";
			return onClickScript;
		}

		private string EditAltText
		{
			get { return string.Format("Click to edit{0}", showTourThumbs ? " this tour" : ""); }
		}

		private void RenderControls(HtmlTextWriter writer, int tourId, int pageId, int pageNumber, string name, int column, Size previewSize, Size thumbnailSize, bool isDataSheet, bool built, bool published, bool changedSincePublished)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "thumbControls");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			writer.AddAttribute(HtmlTextWriterAttribute.Id, "controls" + (showTourThumbs ? tourId : pageId));
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "thumbControlsUnselected");
			writer.RenderBeginTag(HtmlTextWriterTag.Span);
			int dimension = Math.Max(previewSize.Width, previewSize.Height);
			int drawDimensions = 0;
			string popupSrc = string.Format("PageRenderer.ashx?args={0},{1},{2},{3}&v={4}",
				(int)PreviewType.PagePreviewQuick, dimension, pageId, drawDimensions, DateTime.Now.Ticks);
			if (tourId > 0)
				popupSrc += "&tid=" + tourId;
			int x = column <= 2 ? 0 : -(previewSize.Width + 24);
			int y = -(previewSize.Height / 2);

			string onMouseOverScript = string.Format("maQuickImageShow(this,'{0}',{1},{2},{3},{4});", popupSrc, x, y, previewSize.Width, previewSize.Height);
			writer.AddAttribute(HtmlTextWriterAttribute.Src, "../Images/EditSlideButton.gif");
			writer.AddAttribute(HtmlTextWriterAttribute.Onclick, ClickScript(tourId, pageId));
			writer.AddAttribute(HtmlTextWriterAttribute.Title, EditAltText);
			writer.RenderBeginTag(HtmlTextWriterTag.Img);
			writer.RenderEndTag();
			writer.Write("&nbsp;");

			if (pageId > 0 && built)
			{
				if (published)
				{
					// Show the take tour icon. 
					string url = App.TourUrl(tourId);
					string altText;
					if (showTourThumbs)
					{
						altText = "Click to take this tour";
					}
					else
					{
						altText = "Click to take this tour starting at " + name;
						url += "/Page" + pageNumber + ".htm";
					}
					writer.AddAttribute("href", url);
					writer.AddAttribute("target", "_blank");
					writer.RenderBeginTag(HtmlTextWriterTag.A);
					writer.AddAttribute("border", "0");
					writer.AddAttribute(HtmlTextWriterAttribute.Src, "../Images/TakeTour.gif");
					writer.AddAttribute(HtmlTextWriterAttribute.Title, altText);
					if (changedSincePublished)
						writer.AddAttribute(HtmlTextWriterAttribute.Onclick, Tour.HasChangedSinceLastPublishedDeny);
					writer.RenderBeginTag(HtmlTextWriterTag.Img);
					writer.RenderEndTag();
					writer.RenderEndTag();
					writer.Write("&nbsp;");
				}
				
				// Show the preview tour page image icon.
				writer.AddAttribute("onmouseover", onMouseOverScript);
				writer.AddAttribute("onmouseout", "maQuickPreviewHide();");
				writer.AddAttribute(HtmlTextWriterAttribute.Src, "../Images/Magnify.gif");
				writer.RenderBeginTag(HtmlTextWriterTag.Img);
				writer.RenderEndTag();
				writer.Write("&nbsp;");
			}

			string pageKind = isDataSheet ? "DataSheet" : "Map";
			string pageDescription = isDataSheet ? "data sheet" : "map";
			string deleteScript = string.Format("maHandleDelete{0}({1},'{2}');", showTourThumbs ? "Tour" : pageKind, showTourThumbs ? tourId : pageId, name.Replace("'", "\\'"));
			writer.AddAttribute(HtmlTextWriterAttribute.Src, "../Images/Delete.gif");
			writer.AddAttribute(HtmlTextWriterAttribute.Onclick, deleteScript);
			writer.AddAttribute(HtmlTextWriterAttribute.Title, "Delete this " + (showTourThumbs ? "tour" : pageDescription));
			writer.RenderBeginTag(HtmlTextWriterTag.Img);
			writer.RenderEndTag();
			writer.Write("&nbsp;");

			writer.RenderEndTag();
			writer.RenderEndTag();
		}
	
		private void RenderRows(HtmlTextWriter writer)
		{
			try
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
					writer.RenderBeginTag(HtmlTextWriterTag.Tr);		//		<tr>

					// Emit the row's cells.
					for (int column = 1; column <= columnsPerRow; column++)
					{
						// Get the data for this cell.
						string[] thumbInfo = thumb[index].Split((char)0x01);
						int tourId = int.Parse(thumbInfo[0]);
						int pageId = int.Parse(thumbInfo[1]);
						string name = thumbInfo[2];
						int width = int.Parse(thumbInfo[3]);
						int height = int.Parse(thumbInfo[4]);
						bool isDataSheet = thumbInfo[5] == "1";
						int pageNumber = int.Parse(thumbInfo[6]);
						bool built = thumbInfo[7] == "1";
						bool published = thumbInfo[8] == "1";
						int count = int.Parse(thumbInfo[9]);
						bool changedSincePublished = thumbInfo[10] == "1";

						Size tourPageSize = new Size(width, height);
						Size previewContainerSize = new Size(previewDimension, previewDimension);
						Size scaledPreviewSize = Utility.ScaledImageSize(tourPageSize, previewContainerSize);
						Size scaledContainerSize = new Size(scaledDimension, scaledDimension);
						Size scaledThumbnailSize = Utility.ScaledImageSize(tourPageSize, scaledContainerSize);

						// Emit the enclosing TD tag for this cell.
						int controlId = showTourThumbs ? tourId : pageId;
						writer.AddAttribute("onmouseover", OnShowControls(true, controlId));
						writer.AddAttribute("onmouseout", OnShowControls(false, controlId));
						writer.RenderBeginTag(HtmlTextWriterTag.Td);

						// Emit the cell contents.
						RenderThumbnail(writer, tourId, pageId, pageNumber, name, column, scaledPreviewSize, scaledThumbnailSize, isDataSheet, count);
						RenderControls(writer, tourId, pageId, pageNumber, name, column, scaledPreviewSize, scaledThumbnailSize, isDataSheet, built, published, changedSincePublished);

						// Emit the closing TD tag.
						writer.RenderEndTag();

						// Determine if there are any more thumbs to emit.
						index++;
						if (index > lastIndex)
							break;
					}

					writer.RenderEndTag();								//		</tr>
				}

			}
			catch (Exception ex)
			{
				string info = thumbList == null ? "Thumblist is null" : "[" + thumbList + "]";
				Utility.ReportException("RenderRows", info, ex);
			}
		}

		private void RenderThumbnail(HtmlTextWriter writer, int tourId, int pageId, int pageNumber, string name, int column, Size previewSize, Size thumbnailSize, bool isDataSheet, int count)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "pageThumb");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			if (count > 0 || !showTourThumbs)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Id, "thumb" + pageId);
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "pageThumbImage");
				writer.AddAttribute(HtmlTextWriterAttribute.Onclick, ClickScript(tourId, pageId));
				writer.AddAttribute(HtmlTextWriterAttribute.Title, EditAltText);
				writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "pointer");
				writer.AddAttribute(HtmlTextWriterAttribute.Src, ThumbnailImageSrc(pageId, Math.Max(thumbnailSize.Width, thumbnailSize.Height), tourId));
				writer.RenderBeginTag(HtmlTextWriterTag.Img);
				writer.RenderEndTag();
			}

			string textWidth = Math.Max(thumbnailSize.Width, thumbnailSize.Height) + "px";

			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, textWidth);
			writer.AddAttribute(HtmlTextWriterAttribute.Id, "name" + pageId);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, isDataSheet ? "pageThumbNameInfo" : "pageThumbName");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.Write(name);
			writer.RenderEndTag();

			writer.AddStyleAttribute(HtmlTextWriterStyle.Width, textWidth);
			writer.AddAttribute(HtmlTextWriterAttribute.Id, "name" + pageId);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "pageThumbStats");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			string stats = string.Empty;
			if (showTourThumbs)
				stats += string.Format("Tour #{0}<br/>", tourId);
			else
				stats += string.Format("page{0}.htm<br/>", pageNumber);
			string plural = count == 1 ? "" : "s";

			if (!isDataSheet)
			{
				string what = showTourThumbs ? " page{0}" : " hotspot{0}";
				stats += count + string.Format(what, plural);
			}
			
			writer.Write(stats);
			writer.RenderEndTag();

			writer.RenderEndTag();
		}

		private string OnShowControls(bool show, int id)
		{
			return string.Format("maShowThumbControls({0},{1});", show ? 1 : 0, id);
		}

		private string ThumbnailImageSrc(int tourPageId, int dimension, int tourId)
		{
			int drawDimensions = 0;
			string src = string.Format("PageRenderer.ashx?args={0},{1},{2},{3}&v={4}",
				(int)PreviewType.PagePreviewThumb,
				dimension,
				tourPageId,
				drawDimensions,
				DateTime.Now.Ticks
			);
			if (tourId > 0)
				src += "&tid=" + tourId;

			return src;
		}
		#endregion
	}
}
