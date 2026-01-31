// Copyright (C) 2003-2010 AvantLogic Corporation
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AvantLogic.MapsAlive
{
    public enum PageObjectType
	{
		Tour,
		TourPage,
		TourView
	}
	
	public class MemberPageHeader : WebControl
	{
        private bool isReadOnlyPage;
		private MemberPageActionId pageActionId;
		private bool pageIsValid;
		private PageObjectType pageObjectType;
		private string title;
		private Tour tour;
		private TourPage tourPage;
		private HtmlTextWriter writer;

		public MemberPageHeader()
		{
			title = "Title Goes Here";
		}

		#region ===== Properties ========================================================

		public MemberPageActionId PageActionId
		{
			set { pageActionId = value; }
		}

		public bool PageIsValid
		{
			set { pageIsValid = value; }
		}

		public PageObjectType PageObjectType
		{
			get { return pageObjectType; }
			set { pageObjectType = value; }
		}

		public bool ReadOnlyPage
		{
			set { isReadOnlyPage = value; }
		}

		public string Title
		{
			set { title = value; }
		}

		public Tour Tour
		{
			set { tour = value; }
		}

		public TourPage TourPage
		{
			set { tourPage = value; }
		}
        #endregion

        #region ===== Protected =========================================================

        protected override void RenderContents(HtmlTextWriter writer)
		{
			this.writer = writer;

			RenderBranding();
			RenderTourName();
			RenderHeaderControlsRight();
			RenderSubHeader();
		}

		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Div; }
		}
		#endregion

		#region ===== Private ===========================================================

		private void RenderHeaderControlsRight()
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Id, "MemberPageControlsHeaderRight");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			RenderLinks();
			RenderTourPreviewIconButton();
			writer.RenderEndTag();
		}
		
		private void RenderImageButton(string name, string tooltip, string onClickScript, bool enabled)
		{
			RenderImageButton(name, tooltip, "", "", onClickScript, enabled);
		}

		private void RenderImageButton(string name, string tooltip, string onMouseOverScript, string onMouseOutScript, string onClickScript, bool enabled)
		{
			string buttonImageName = "Btn" + name;

			writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "pointer");

			string src = App.WebSitePathUrl(string.Format("Images/{0}{1}.gif", buttonImageName, enabled ? "1" : "0"));
			writer.AddAttribute(HtmlTextWriterAttribute.Src, src);

			if (onClickScript.Length > 0)
				writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClickScript);

			// Enable or disable the button by giving it a special class name.  We can't use the 'disabled'
			// attribute since not all browsers support it on IMG elements (Safari, and FF on the Mac).
			writer.AddAttribute(HtmlTextWriterAttribute.Class, enabled ? "buttonEnabled" : "buttonDisabled");
			
			string mouseActionScript = "maOnRollover({0}, this, '" + buttonImageName + "');";
			writer.AddAttribute("onmouseover", string.Format(mouseActionScript, "true") + onMouseOverScript);
			writer.AddAttribute("onmouseout", string.Format(mouseActionScript, "false") + onMouseOutScript);

			writer.AddAttribute(HtmlTextWriterAttribute.Title, tooltip);
			writer.AddAttribute(HtmlTextWriterAttribute.Id, name + "Button");
			
			writer.RenderBeginTag(HtmlTextWriterTag.Img);
			writer.RenderEndTag();
		} 

		private void RenderPreviewButton()
		{
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "memberPageTourPreviewButton");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			
			string script = tour.Advisor.PreviewScript((int)pageActionId, tourPage.PageNumber);
			writer.AddAttribute(HtmlTextWriterAttribute.Href, string.Format("javascript:{0}", script));
            writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "pointer");

			writer.AddAttribute(HtmlTextWriterAttribute.Title, "Preview, publish, or run this tour");
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "memberPageBannerOvalButton");
            writer.AddAttribute(HtmlTextWriterAttribute.Id, "PreviewIconButton");
			writer.RenderBeginTag(HtmlTextWriterTag.A);
            
            writer.RenderBeginTag(HtmlTextWriterTag.Span);
			writer.Write("Tour Preview");
			
            writer.RenderEndTag();
			writer.RenderEndTag();
			writer.RenderEndTag();
		}

		private void RenderTextButton(string name, string tooltip, string onClickScript, string buttonText, string buttonClass)
		{
			writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "pointer");

			if (onClickScript.Length > 0)
				writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClickScript);

			writer.AddAttribute(HtmlTextWriterAttribute.Title, tooltip);
			writer.AddAttribute(HtmlTextWriterAttribute.Id, name + "Button");
			writer.AddAttribute(HtmlTextWriterAttribute.Class, buttonClass);
			writer.RenderBeginTag(HtmlTextWriterTag.Span);
			writer.Write(buttonText);
			writer.RenderEndTag();
		}

        private void RenderLinks()
        {
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "memberPageControlsLinks");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            RenderTextButton("UserGuideIcon", "View the MapsAlive User Guide", "window.open('https://www.mapsalive.com/docs', '_blank');", "User Guide", "memberPageControlsLinkHome");
            RenderTextButton("HomeIcon", "MapsAlive Home Page", "maOnEventSaveAndTransfer('');", "Home", "memberPageControlsLinkHome");
            RenderTextButton("LogoutIcon", "Logout of your account", "maOnEventSaveAndTransfer('/Logout.ashx');", "Logout", "memberPageControlsLinkLogout");
			writer.RenderEndTag();
        }

		private void RenderBranding()
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "memberPageControlsSiteName");

			if (Utility.UserIsAdmin)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Onclick, "maOnEventSaveAndTransfer('/Admin/Users.aspx');");
				writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "pointer");
			}
			
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.Write(MapsAliveState.Account.SiteName);
			RenderUserName();
			writer.RenderEndTag();
		}

		private void RenderTourPreviewIconButton()
		{
			if (tour == null)
				tour = (Tour)MapsAliveState.Retrieve(MapsAliveObjectType.Tour);

			if (tour == null)
				return;

			if (pageActionId == MemberPageActionId.Welcome || pageActionId == MemberPageActionId.MapSetupNew || pageActionId == MemberPageActionId.GallerySetupNew)
			{
				// Don't show the Tour Preview button when the user is setting up a new tour, map, or gallery.
				return;
			}

			if (pageActionId == MemberPageActionId.TourResourceDependencyReport)
			{
				// Don't show the Tour Preview button when on the dependency report page because we don't
				// have the right logic to return back to that page from Tour Preview and show the correct
				// dependency report. When returning from Tour Preview the dependency report page would need
				// to know which type of resource was being reported on before Tour Preview was clicked. If
				// the report page knew that, then it could do like e.g. EditMarker.aspx.cs when it checks
				// the IsReturnToTourBuilder flag and gets the resource from the account cache. For now lets
				// just not let the user go to TourPreview from the report page.
				return;
			}

			// Determine which page to show first in the tour preview. Normally its the page that's
			// selected in the Tour Navigator, but when on the Tour Manager screen, show the tour's
			// first page first. It's inconsistent, but since one of the main reasons for going to the
			// Tour Manager is to change the first page, it's very confusing if you change it and then
			// click Tour Preview and that page does not appear first.
			if (tourPage == null)
				return;

			RenderPreviewButton();
		}

		private void RenderTourLink()
		{
			
		}
		
		private void RenderTourName()
		{
			Tour tour = tourPage == null ? null : tourPage.Tour;
			if (tour != null)
			{
				string tourName;
				bool isTourPageScreen = PageObjectType == PageObjectType.TourPage || PageObjectType == PageObjectType.TourView;
				if (isTourPageScreen && tourPage != null)
				{
					// Combine the tour name and page names with a placeholder separator that the user
					// cannot type. That way we can replace it without affecting the names.
					tourName = tour.Name + Utility.CrLf + tourPage.Name;
				}
				else
				{
					tourName = tour.Name;
				}

				// Make sure the text is not too long. If it is, truncate it.
				const int maxNameLength = 40;
				if (tourName.Length > maxNameLength)
					tourName = tourName.Substring(0, maxNameLength) + "...";

				if (isTourPageScreen && tourPage != null)
				{
					// Replace the placeholder separator with a friendly separator. We do the replacement
					// after truncation so that the friendly separator does not get truncated.
					tourName = tourName.Replace(Utility.CrLf, "&nbsp;&nbsp;&mdash;&nbsp;&nbsp;");
				}

                string prefix;
                if (tour.V3CompatibilityEnabled)
                    prefix = "V3 Tour";
                else
                    prefix = tour.IsFlexMapTour ? "Flex Map" : "Classic Tour";
                tourName = string.Format("{0} #{1} : {2}", prefix, tour.Id, tourName);

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "memberPageControlsTourName");
				writer.RenderBeginTag(HtmlTextWriterTag.Div);
				writer.Write(tourName);
				writer.RenderEndTag();
			}			
		}
		
		private void RenderSubHeader()
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "memberPageControlsMenuBackground");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.RenderEndTag();

			writer.AddAttribute(HtmlTextWriterAttribute.Class, "memberPageControlsSaveUndo");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			RenderSaveButtons();
			writer.RenderEndTag();
			
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "memberPageControlsTitle");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.Write(title);

            RenderTitleHelp();
            RenderTitleUserGuide();

            writer.RenderEndTag();

			writer.AddAttribute(HtmlTextWriterAttribute.Class, "memberPageControlsStatusMessage");
			writer.AddAttribute(HtmlTextWriterAttribute.Id, "MemberPageStatusMsg");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.RenderEndTag();
		}

		private void RenderSaveButtons()
		{
			if (isReadOnlyPage)
				return;

			string topic = "QuickHelp" + this.ID;
			string script = string.Format("javascript:maQuickHelpShow(this,'{0}',{1},{2});", Utility.QuickHelpText(topic), 0, 0);
			writer.AddAttribute(HtmlTextWriterAttribute.Src, App.WebSitePathUrl("Images/QuickHelp.png"));
			writer.AddAttribute("onmouseover", script);
			writer.AddAttribute("onmouseout", "javascript:maQuickHelpHide();");
			writer.RenderBeginTag(HtmlTextWriterTag.Img);	//		<img>
			writer.RenderEndTag();							//		</img>
			writer.Write("&nbsp");
			RenderImageButton("Save", "Save changes to this page", "maOnEventSaveButton(this);", !pageIsValid);
			writer.Write("&nbsp");
			RenderImageButton("Undo", "Undo changes to this page", "maOnEventUndoButton(this);", !pageIsValid);
		}
		
		private void RenderTitleHelp()
		{
			// This method emits the explain icon that follows the screen title.
			
			string actionIdName = pageActionId.ToString();
			
			// Handle the special case of Slide Content and Data Sheet Content sharing
			// the same page. If the page is a data sheet, create a pseudo action Id so 
			//that we can display different help text for hotspots and data sheets.
			if (pageActionId == MemberPageActionId.EditHotspotContent && tourPage.IsDataSheet)
				actionIdName = "DataSheetContent";
            else if (pageActionId == MemberPageActionId.HotspotOptionsAdvanced && tourPage.IsDataSheet)
                actionIdName = "DataSheetOptionsAdvanced";

            string topicId = string.Format("ScreenHelp{0}", actionIdName);
			string topicText = Utility.QuickHelpText(topicId, string.Empty, true);
			if (topicText != string.Empty)
			{
				writer.Write("&nbsp;");
				topicText = string.Format("<b>{0}</b><br/></br>{1}", title, topicText);
				string script = string.Format("javascript:maQuickHelpShow(this,'{0}',{1},{2});", topicText, 0, 0);
				writer.AddAttribute(HtmlTextWriterAttribute.Src, App.WebSitePathUrl("Images/QuickHelp.png"));
				writer.AddAttribute("onmouseover", script);
				writer.AddAttribute("onmouseout", "javascript:maQuickHelpHide();");
				writer.RenderBeginTag(HtmlTextWriterTag.Img);
				writer.RenderEndTag();
			}
		}
		
		private void RenderTitleUserGuide()
		{
            // This method emits the user guide icon that follows the help icon.

            string pageActionIdString;
            if (pageActionId == MemberPageActionId.EditHotspotContent && tourPage.IsDataSheet)
                pageActionIdString = "DataSheetContent";
            else if (pageActionId == MemberPageActionId.HotspotOptionsAdvanced && tourPage.IsDataSheet)
                pageActionIdString = "DataSheetOptionsAdvanced";
            else
                pageActionIdString = pageActionId.ToString();

            string topicId = string.Format("UserGuide{0}", pageActionIdString);
			string topicText = AppContent.TopicOptional(topicId);

            if (topicText != string.Empty)
			{
				writer.Write("&nbsp;");
				string href = string.Format("https://www.mapsalive.com/docs/{0}", topicText);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "memberPageControlsUserGuide");
                writer.AddAttribute(HtmlTextWriterAttribute.Href, href, false);
                writer.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
                writer.AddAttribute(HtmlTextWriterAttribute.Title, string.Format("Learn about {0} in the User Guide", title));
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.AddAttribute(HtmlTextWriterAttribute.Src, App.WebSitePathUrl("Images/UserGuide.png"));
                writer.RenderBeginTag(HtmlTextWriterTag.Img);
				writer.RenderEndTag();
                writer.RenderEndTag();
			}
		}

		private void RenderUserName()
		{
			string className = "memberPageControlsUserName";
			string name = " - " + Utility.AccountIdentification;
			if (Utility.ImitatingUser())
			{
				className += "Impersonating";
				name = string.Format("{0}", MapsAliveState.Account.Email);
			}
			writer.AddAttribute(HtmlTextWriterAttribute.Class, className);
			writer.AddAttribute(HtmlTextWriterAttribute.Onclick, "maOnEventSaveAndTransfer('/Members/Profile.aspx');");
			writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "pointer");
			writer.RenderBeginTag(HtmlTextWriterTag.Span);
			writer.Write(name);
			writer.RenderEndTag();
		}
		#endregion
	}
}
