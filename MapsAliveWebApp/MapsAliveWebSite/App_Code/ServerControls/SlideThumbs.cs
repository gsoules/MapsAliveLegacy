// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Diagnostics;
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
	public class SlideThumbs : WebControl
	{
		private int columnsPerRow;
		private bool tourWidthLocked;
		private bool tourHeightLocked;
		private PreviewType previewType;
		private SlideLayout selectedSlideLayout;
		private bool showNonMapLayouts;
		private bool slidesPopup;
		private TourPage tourPage;
		private string warning;
		
		private SlideLayout layoutForFamilyMapH;
		private SlideLayout layoutForFamilyMapHI;
		private SlideLayout layoutForFamilyMapHT;
		private SlideLayout layoutForFamilyMapV;
		private SlideLayout layoutForFamilyMapVI;
		private SlideLayout layoutForFamilyMapVT;
		private SlideLayout layoutForFamilyImageH;
		private SlideLayout layoutForFamilyImageV;
		private SlideLayout layoutForFamilyNoMapImageH;
		private SlideLayout layoutForFamilyNoMapImageV;
		private SlideLayout layoutForFamilyTextH;
		private SlideLayout layoutForFamilyTextV;
		private SlideLayout layoutForFamilyMapOnly;
		private SlideLayout layoutForFamilyImageOnly;
		private SlideLayout layoutForFamilyTextOnly;

		private const int thumbnailSize = 160;
		private const string LayoutIdKey = "LayoutSelector";

		public SlideThumbs()
		{
			columnsPerRow = 4;
			previewType = PreviewType.SlideLayout;
			warning = string.Empty;
		}

		#region ===== Properties ========================================================

		public int ColumnsPerRow
		{
			get { return columnsPerRow; }
			set { columnsPerRow = value; }
		}

		public bool TourWidthLocked
		{
			set { tourWidthLocked = value; }
		}

		public bool TourHeightLocked
		{
			set { tourHeightLocked = value; }
		}

		public PreviewType PreviewType
		{
			set { previewType = value; }
		}

		public bool ShowNonMapLayouts
		{
			set { showNonMapLayouts = value; }
		}

		public bool SlidesPopup
		{
			set { slidesPopup = value; }
		}

		public TourPage TourPage
		{
			set
			{
				tourPage = value;
				selectedSlideLayout = tourPage.ActiveSlideLayout;
			}
		}

		public string Warning
		{
			set { warning = value; }
		}
		#endregion

		#region ===== Public ============================================================

		public bool LoadPostData(string postDataKey, System.Collections.Specialized.NameValueCollection postCollection)
		{
			ViewState[LayoutIdKey] = int.Parse(postCollection[postDataKey]);
			return false;
		}

		public void RaisePostDataChangedEvent()
		{
		}
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

		private SlideLayout CreateFamilyLayout(ref SlideLayout familySlideLayout, SlideLayoutPattern pattern)
		{
			// This method creates a layout for a specific layout family.  If the layout already exists,
			// it does nothing.  The first time a layout is created for a family, it is saved (by this method's
			// caller).  All subsequent attempts to translate splitters for a layout in the same family, use
			// the family layout as the "old" layout.  This is done so that all layouts in the same family
			// will be translated from a layout that has the same splitter usage (e.g. vertical is for the
			// map and horizontal is for the image).  Before we introduced families, all layouts were translated
			// from the selected layout; however, any splitters that did not have the same usage could not be
			// translated and were just left in their original position (they couldn't translate because, for
			// example, the selected layout's vertical splitter was used for the map, the another layout used
			// the vertical splitter for the image).

			if (familySlideLayout != null)
			{
				// We already have a layout for the current thumb's family.
				return familySlideLayout;
			}
			
			// Make a copy of the selected layout.
			SlideLayout slideLayoutCopy = new SlideLayout(selectedSlideLayout);
			
			// Change the copied layout's pattern to that of the current thumb's layout so that subsequent
			// translations of layouts in this family will use the current thumb's layout as the "old" layout.
			slideLayoutCopy.Pattern = pattern;

			// Create a temporary layout manager.
			LayoutManager layoutManager = new LayoutManager(tourPage, ref slideLayoutCopy);
			
			// Run auto layout for the family.
			layoutManager.PerformAutoLayoutForLayoutFamily(tourWidthLocked, tourHeightLocked);

			// Save the family layout so we won't have to create it again for the next layout in the family.
			familySlideLayout = layoutManager.ActiveSlideLayout;

			return familySlideLayout;
		}

		private void GetLayoutValues(int patternId, out int splitterH, out int splitterV, out Size layoutAreaSize)
		{
			SlideLayout slideLayout;
			SlideLayoutPattern pattern = (SlideLayoutPattern)patternId;

			SlideLayoutFamily selectedLayoutFamily = SlideLayout.GetFamily(tourPage.ActiveSlideLayout.Pattern);
			SlideLayoutFamily thumbLayoutFamily = SlideLayout.GetFamily(pattern);

			if (selectedLayoutFamily == thumbLayoutFamily)
			{
				// The current thumb is in the same family as the selected layout.  The thumb
				// layout can be translated directly from the selected layout.
				slideLayout = selectedSlideLayout;
			}
			else
			{
				// The current thumb belongs to a different family than the selected layout.
				// Get a layout for the family and translate this thumb's layout from it.
				switch (thumbLayoutFamily)
				{
					case SlideLayoutFamily.MapH:
						slideLayout = CreateFamilyLayout(ref layoutForFamilyMapH, pattern);
						break;

					case SlideLayoutFamily.MapHI:
						slideLayout = CreateFamilyLayout(ref layoutForFamilyMapHI, pattern);
						break;

					case SlideLayoutFamily.MapHT:
						slideLayout = CreateFamilyLayout(ref layoutForFamilyMapHT, pattern);
						break;

					case SlideLayoutFamily.MapV:
						slideLayout = CreateFamilyLayout(ref layoutForFamilyMapV, pattern);
						break;

					case SlideLayoutFamily.MapVI:
						slideLayout = CreateFamilyLayout(ref layoutForFamilyMapVI, pattern);
						break;

					case SlideLayoutFamily.MapVT:
						slideLayout = CreateFamilyLayout(ref layoutForFamilyMapVT, pattern);
						break;

					case SlideLayoutFamily.ImageH:
						slideLayout = CreateFamilyLayout(ref layoutForFamilyImageH, pattern);
						break;

					case SlideLayoutFamily.ImageV:
						slideLayout = CreateFamilyLayout(ref layoutForFamilyImageV, pattern);
						break;

					case SlideLayoutFamily.NoMapImageH:
						slideLayout = CreateFamilyLayout(ref layoutForFamilyNoMapImageH, pattern);
						break;

					case SlideLayoutFamily.NoMapImageV:
						slideLayout = CreateFamilyLayout(ref layoutForFamilyNoMapImageV, pattern);
						break;

					case SlideLayoutFamily.TextH:
						slideLayout = CreateFamilyLayout(ref layoutForFamilyTextH, pattern);
						break;

					case SlideLayoutFamily.TextV:
						slideLayout = CreateFamilyLayout(ref layoutForFamilyTextV, pattern);
						break;

					case SlideLayoutFamily.MapOnly:
						slideLayout = CreateFamilyLayout(ref layoutForFamilyMapOnly, pattern);
						break;

					case SlideLayoutFamily.ImageOnly:
						slideLayout = CreateFamilyLayout(ref layoutForFamilyImageOnly, pattern);
						break;

					case SlideLayoutFamily.TextOnly:
						slideLayout = CreateFamilyLayout(ref layoutForFamilyTextOnly, pattern);
						break;

					default:
						Debug.Fail("Unexpected family " + thumbLayoutFamily.ToString());
						slideLayout = null;
						break;
				}
			}

			// Get splitter values for this thumb's layout.
			SlideLayout.TranslateSplitters(tourPage.Tour, slideLayout, pattern, out splitterH, out splitterV);

			// Get the page size for this thumb's layout.
			layoutAreaSize = slideLayout.OuterSize;
		}

		private void RenderThumbnail(HtmlTextWriter writer, int patternId)
		{
			int splitterH;
			int splitterV;
			Size layoutAreaSize;

            SlideLayoutPattern pattern = selectedSlideLayout.Pattern;
            
            // Convert deprecated popup layouts to a supported layout.
            if (tourPage.Tour.V4 && tourPage.SlidesPopup && (pattern == SlideLayoutPattern.VIITT || pattern == SlideLayoutPattern.VTTII))
                pattern = SlideLayoutPattern.HIITT;

			GetLayoutValues(patternId, out splitterH, out splitterV, out layoutAreaSize);

			Size tourSize = LayoutManager.CalculateTourSizeForLayoutAreaOuterSize(tourPage.Tour, layoutAreaSize);

			writer.RenderBeginTag(HtmlTextWriterTag.Div);	//				<div>
			writer.AddAttribute(HtmlTextWriterAttribute.Id, "thumb" + patternId);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, patternId == (int)pattern ? "slideThumbSelected" : "slideThumbUnselected");
			writer.AddAttribute(HtmlTextWriterAttribute.Src, ImageSrc(patternId, thumbnailSize, splitterH, splitterV, layoutAreaSize.Width, layoutAreaSize.Height));
			
			// Set up a click handler for the new layout unless this is the current layout.
			if ((int)pattern != patternId)
				writer.AddAttribute(HtmlTextWriterAttribute.Onclick, OnClickCall(warning, patternId, splitterH, splitterV));
			
			writer.RenderBeginTag(HtmlTextWriterTag.Img);	//					<img>
			writer.RenderEndTag();							//					</img>
			writer.RenderEndTag();							//				</div>

			writer.AddAttribute(HtmlTextWriterAttribute.Id, "name" + patternId);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "slideThumbNameUnselected");
			writer.AddAttribute(HtmlTextWriterAttribute.Onclick, OnClickCall(warning, patternId, splitterH, splitterV));
			writer.RenderBeginTag(HtmlTextWriterTag.Div);	//				<div>

			string info = string.Format("<b>#{0}</b>", patternId);
			if (!tourPage.SlidesPopup)
				info += string.Format("&nbsp;&nbsp;&nbsp;({0} x {1})", tourSize.Width, tourSize.Height);
			writer.Write(info);
			
			writer.RenderEndTag();							//				</div>
		}
	
		private void RenderRows(HtmlTextWriter writer)
		{
			Array types = Enum.GetValues(typeof(SlideLayoutPattern));
			int indexOfFirstPopupPattern = 0;
			
			for (int i = 0; i < types.Length; i++)
			{
				SlideLayoutPattern pattern = (SlideLayoutPattern)(i + 1);
				if (!SlideLayout.GetHasMapArea(pattern) || pattern == SlideLayoutPattern.HMM)
				{
					indexOfFirstPopupPattern = i;
					break;
				}
			}

			int index;
			int lastIndex;

			if (slidesPopup)
			{
				index = indexOfFirstPopupPattern;
				lastIndex = types.Length - 1;
			}
			else
			{
				index = 0;
				lastIndex = types.Length - 1;
			}

			int layoutCount = lastIndex - index + 1;
			int rowCount = (int)Math.Ceiling((double)layoutCount / (double)columnsPerRow);

            bool done = false;

			for (int row = 1; row <= rowCount; row++)
			{
                if (done)
                    break;

                writer.AddAttribute(HtmlTextWriterAttribute.Valign, "top");
				writer.RenderBeginTag(HtmlTextWriterTag.Tr);

				for (int column = 1; column <= columnsPerRow;)
				{
					int patternId = index + 1;

                    if (index > lastIndex)
                    {
                        done = true;
                        break;
                    }

                    index++;

                    SlideLayoutPattern slideLayoutPattern = (SlideLayoutPattern)patternId;
					bool layoutHasMapArea = SlideLayout.GetHasMapArea(slideLayoutPattern);

                    if (SlideLayout.IsDeprecatedLayout(slideLayoutPattern, tourPage) && !tourPage.Tour.V3CompatibilityEnabled)
                    {
                        // V4 no longer supports layouts where the image and text areas are not together in the
                        // same column or row with with both having either the same width or the same height.
                        continue;
                    }

                    if (SlideLayout.IsStackedResponsiveLayout(slideLayoutPattern))
                    {
                        // The user cannot chooose a stacked responsive layout. They are
                        // only used by the runtime when the browser is very narrow.
                        continue;
                    }

                    if (!layoutHasMapArea && !tourPage.IsDataSheet && !slidesPopup && !tourPage.Tour.V3CompatibilityEnabled)
                    {
                        // V4 no longer allows a non-map layout to be chosen for a page that has a map.
                        continue;
                    }

                    if (slidesPopup && layoutHasMapArea)
					{
						// Don't show map layouts for popup slides.
						continue;
					}

                    column++;

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "slideThumbCell");
					writer.AddStyleAttribute(HtmlTextWriterStyle.Width, ((int)(thumbnailSize + 8)).ToString());
					writer.RenderBeginTag(HtmlTextWriterTag.Td);

					RenderThumbnail(writer, patternId);

					writer.RenderEndTag();
				}

				writer.RenderEndTag();
			}
		}

		private string OnClickCall(string warning, int patternId, int splitterH, int splitterV)
		{
			int hasHorizontalSplitter = SlideLayout.GetHasHorizontalSplitter((SlideLayoutPattern)patternId) ? 1 : 0;
			int hasVerticalSplitter = SlideLayout.GetHasVerticalSplitter((SlideLayoutPattern)patternId) ? 1 : 0;
			int adjust = !tourWidthLocked || !tourHeightLocked ? 1 : 0;
			return string.Format("maOnSlideLayoutClicked('{0}',{1});", warning, patternId);
		}

		private string ImageSrc(int patternId, int dimension, int splitterH, int splitterV, int slideWidth, int slideHeight)
		{
			int drawDimensions = 0;

			return string.Format("PageRenderer.ashx?args={0},{1},{2},{3},{4},{5},{6},{7},{8}&v={9}",
				(int)previewType,
				dimension,
				tourPage.Id,
				drawDimensions,
				patternId.ToString(),
				splitterH,
				splitterV,
				slideWidth,
				slideHeight,
				DateTime.Now.Ticks
			);
		}
		#endregion
	}
}
