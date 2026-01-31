// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml;

namespace AvantLogic.MapsAlive
{
	public class TourList : WebControl
	{
		public TourList()
		{
		}

		private static string CreateMouseOverScript(int tourId, int startPageId, ref Size tourSize)
		{
			Size previewSize = Utility.ScaledImageSize(tourSize, new Size(300, 300));
			int dimension = Math.Max(previewSize.Width, previewSize.Height);
			int drawDimensions = 0;
			int x = 140;
			int y = -100;
			string src = string.Format("PageRenderer.ashx?args={0},{1},{2},{3}&tid={4}&v={5}",
				(int)PreviewType.PagePreviewQuick, dimension, startPageId, drawDimensions, tourId, DateTime.Now.Ticks);
			string script = string.Format("maQuickImageShow(this,'{0}',{1},{2},{3},{4});",
				src, x, y, previewSize.Width, previewSize.Height);
			return script;
		}

		private static DataTable TourTable()
		{
			DataTable dataTable = (DataTable)MapsAliveState.Retrieve(MapsAliveObjectType.TourList);
			if (dataTable == null)
			{
				dataTable = MapsAliveState.Account.GetTours();
				MapsAliveState.Persist(MapsAliveObjectType.TourList, dataTable);
			}
			return dataTable;
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			DataTable dataTable = TourTable();
			if (dataTable.Rows.Count == 0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "tourListName");
				writer.RenderBeginTag(HtmlTextWriterTag.Div);
				writer.Write("(No tours yet)");
				writer.RenderEndTag();
			}
			else
			{
				foreach (DataRow dataRow in dataTable.Rows)
				{
					MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
					string tourName = row.StringValue("Name");
					int tourId = row.IntValue("TourId");
					int startPageId = row.IntValue("StartPageId");
					Size tourSize = new Size(row.IntValue("PageWidth"), row.IntValue("PageHeight"));
					bool exceedsSlideLimit = row.BoolValue("ExceedsSlideLimit");
					RenderTourName(writer, tourName, tourId, startPageId, tourSize, exceedsSlideLimit);
				}
			}
		}

		private void RenderTourName(HtmlTextWriter writer, string tourName, int tourId, int startPageId, Size tourSize, bool exceedsSlideLimit)
		{
			string onClickScript = string.Format("maOnEventSaveAndTransfer('/Members/ManageTour.ashx?id={0}');", tourId);
			writer.AddAttribute("onclick", onClickScript);
			
			//string onMouseOverScript = CreateMouseOverScript(tourId, startPageId, ref tourSize);
			//writer.AddAttribute("onmouseover", onMouseOverScript);
			//writer.AddAttribute("onmouseout", "maQuickPreviewHide();");

			string className = "tourListName";
			if (exceedsSlideLimit)
				className += "ExceedsLimit";
			writer.AddAttribute(HtmlTextWriterAttribute.Class, className);
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.Write(tourName);
			writer.RenderEndTag();
		}

		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Div; }
		}
	}
}
