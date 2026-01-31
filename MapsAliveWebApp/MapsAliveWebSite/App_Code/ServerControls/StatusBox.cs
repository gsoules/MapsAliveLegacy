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
	public class StatusBox : WebControl
	{
		private string alternateStep;
		private bool hideCloseX;
		private string lastAction;
        private string lastActionNote;
		private string nextAction;
		private string note;
		private string[] step;
		private bool[] unnumbered;
        private bool showGraphicHotspotIcon;
        private bool showGraphicMapEditorIcon;
		private bool showSampleImages;
		private bool showSampleMaps;
		private bool showSampleZip;
        private int statusBoxTop = 2;
        private int statusBoxLeft = 428;

		const int maxStep = 5;
		
		public StatusBox()
		{
			step = new string[maxStep];
			unnumbered = new bool[maxStep];
		}

		#region ===== Properties ========================================================

		public bool HideCloseX
		{
			set { hideCloseX = value; }
		}
		
		public string LastAction
		{
			set { lastAction = FormatText(value); }
		}

		
		public string NextAction
		{
			set { nextAction = FormatText(value); }
		}

		public string Note
		{
			set { note = FormatText(value); }
		}

		public bool ShowGraphicHotspotIcon
		{
			set { showGraphicHotspotIcon = value; }
		}

		public bool ShowGraphicMapEditorIcon
		{
			set { showGraphicMapEditorIcon = value; }
		}

		public bool ShowSampleImages
		{
			set { showSampleImages = value; }
		}

		public bool ShowSampleMaps
		{
			set { showSampleMaps = value; }
		}

		public bool ShowSampleZip
		{
			set { showSampleZip = value; }
		}

		public string LastActionNote
		{
			set { lastActionNote = value; }
		}

		#endregion
		
		#region ===== Public ========================================================

		public void Clear()
		{
			alternateStep = null;
			for (int index = 0; index < maxStep; index++)
				step[index] = null;
			
			showSampleMaps = false;
			showSampleImages = false;
			showSampleZip = false;
            showGraphicHotspotIcon = false;
            showGraphicMapEditorIcon = false;
            lastActionNote = "";
        }

		public void SetAlternateStep(string value)
		{
			alternateStep = value;
		}

        public void SetStatusBoxTopLeft(int top, int left)
        {
            statusBoxTop = top;
            statusBoxLeft = left;
        }

        public void SetStep(int stepNumber, string value)
		{
			System.Diagnostics.Debug.Assert(stepNumber > 0 && stepNumber <= maxStep, "Invalid step " + stepNumber);
			step[stepNumber - 1] = FormatText(value);
		}

		public void SetUnnumberedStep(int stepNumber, string value)
		{
			SetStep(stepNumber, value);
			unnumbered[stepNumber - 1] = true;
		}

		#endregion

		#region ===== Protected =========================================================

		protected override void RenderContents(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Id, "StatusBox");
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "statusBox");
            writer.AddStyleAttribute(HtmlTextWriterStyle.Top, statusBoxTop + "px");

            // Set the default left offset though it will be updated dynamically by maOnResize
            // in MemberPage.js so that box's right edge sticks to the right edge of the Tour
            // Builder and will thus automatically adjust for the wider Map Editor page.
            writer.AddStyleAttribute(HtmlTextWriterStyle.Left, statusBoxLeft + "px");

			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			EmitCloseX(writer);
			EmitContent(writer);
			writer.RenderEndTag();
		}

		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Div; }
		}
		#endregion

		#region ===== Private ===========================================================

        private string FormatText(string text)
        {
            string s = text.Replace("[", "<span class='statusBoxAction'>");
            s = s.Replace("]", "</span>");
            s = s.Replace("<a'", "<a title='See this topic in the User Guide' target='_blank' href='https://www.mapsalive.com/docs/");
            return s;
        }

		private void EmitAlternateStep(HtmlTextWriter writer)
		{
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);

			writer.AddAttribute(HtmlTextWriterAttribute.Class, "statusBoxStepNumberAlternate");
			writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.Write("OR");
			writer.RenderEndTag();
			
			writer.RenderEndTag();

			writer.RenderBeginTag(HtmlTextWriterTag.Tr);

			writer.AddAttribute(HtmlTextWriterAttribute.Class, "statusBoxStepTextAlternate");
			writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.Write(alternateStep);
			writer.RenderEndTag();

			writer.RenderEndTag();
		}

		private void EmitCloseX(HtmlTextWriter writer)
		{
			if (hideCloseX)
				return;
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "statusBoxCloseX");
			writer.AddAttribute(HtmlTextWriterAttribute.Onclick, "maShowStatusBox(false);");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.RenderEndTag();
		}

		private void EmitContent(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "statusBoxTitle");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.Write("Step-by-step Instructions");
			writer.RenderEndTag();
			writer.RenderBeginTag(HtmlTextWriterTag.Hr);
			writer.RenderEndTag();
			
			if (note != null)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "statusBoxNote");
				writer.RenderBeginTag(HtmlTextWriterTag.Div);
				writer.Write(note);
				writer.RenderEndTag();
			}

			if (lastAction != null)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "statusBoxLastAction");
				writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(lastAction);
                writer.RenderEndTag();

                if (lastActionNote != null && lastActionNote.Length > 1)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, "statusBoxLastActionNote");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    writer.Write(lastActionNote);
                    writer.RenderEndTag();
                }
				
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Id, "statusBoxInstructions");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            if (lastAction != null)
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                writer.RenderEndTag();
            }
            
            if (nextAction != null)
			{
                if (lastAction != null)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "statusBoxStatus");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    writer.Write("Next Step:");
                    writer.RenderEndTag();
                }

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "statusBoxNextAction");
				writer.RenderBeginTag(HtmlTextWriterTag.Div);
				writer.Write(nextAction);
				writer.RenderEndTag();
			}

			writer.AddAttribute(HtmlTextWriterAttribute.Class, "statusBoxSteps");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			EmitSteps(writer);
			writer.RenderEndTag();

            writer.AddAttribute(HtmlTextWriterAttribute.Class, "statusBoxStopMessage");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.Write("To stop seeing these instructions, uncheck the <b>Show Instructions</b> option on the <a href='../Members/Preferences.aspx'>Account Preferences</a> screen.");
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        private void EmitGraphicHotspotIcon(HtmlTextWriter writer)
        {
            EmitGraphic(writer, 201);
        }

        private void EmitGraphicMapEditorIcon(HtmlTextWriter writer)
        {
            EmitGraphic(writer, 202);
        }

        private void EmitGraphic(HtmlTextWriter writer, int imageId)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "statusBoxGraphic");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            string fileLocation = App.WebSitePathUrl(string.Format("images/samples/{0}", Utility.GraphicImageFileName(imageId)));
            writer.AddAttribute(HtmlTextWriterAttribute.Src, fileLocation);
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();

            writer.RenderEndTag();
        }

        private void EmitSampleImages(HtmlTextWriter writer)
		{
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "statusBoxSampleImages");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            EmitSampleImagesRow(writer, 0, 0);
			EmitSampleImagesRow(writer, 1, 0);
			EmitSampleImagesRow(writer, 2, 0);

            writer.RenderEndTag();
        }

        private void EmitSampleImagesRow(HtmlTextWriter writer, int rowNumber, int startId)
		{
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			EmitSampleImage(writer, rowNumber * 3 + 1 + startId);
			writer.RenderEndTag();
			
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			EmitSampleImage(writer, (rowNumber * 3) + 2 + startId);
			writer.RenderEndTag();
			
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			EmitSampleImage(writer, (rowNumber * 3) + 3 + startId);
			writer.RenderEndTag();
		}

		private void EmitSampleImage(HtmlTextWriter writer, int imageId)
		{
			string onClick = string.Format("maOnEventUploadSampleImage('{0}');", imageId);
			writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClick);

			string fileLocation = App.WebSitePathUrl(string.Format("images/samples/{0}", Utility.SampleThumbFileName(imageId)));
			writer.AddAttribute(HtmlTextWriterAttribute.Id, "maSampleImage");
			writer.AddAttribute(HtmlTextWriterAttribute.Src, fileLocation);

			writer.RenderBeginTag(HtmlTextWriterTag.Img);
			writer.RenderEndTag();
		}

		private void EmitSampleMaps(HtmlTextWriter writer)
		{
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "statusBoxMapImages");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
			EmitSampleImagesRow(writer, 0, 100);
			writer.RenderEndTag();
		}

		private void EmitSampleZip(HtmlTextWriter writer)
		{
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "statusBoxSampleZip");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            
            string onClick = "maOnEventUploadSampleImage(0);";
			writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClick);
			string fileLocation = App.WebSitePathUrl("images/samples/thumbzip.jpg");
			writer.AddAttribute(HtmlTextWriterAttribute.Id, "maSampleImage");
			writer.AddAttribute(HtmlTextWriterAttribute.Src, fileLocation);
			writer.AddStyleAttribute(HtmlTextWriterStyle.Cursor, "pointer");
			writer.RenderBeginTag(HtmlTextWriterTag.Img);
			writer.RenderEndTag();

			writer.RenderEndTag();
		}

        private void EmitStep(HtmlTextWriter writer, int stepNumber)
		{
			int stepIndex = stepNumber - 1;
			if (step[stepIndex] == null)
				return;

			bool isUnnumberedStep = unnumbered[stepIndex];

			writer.RenderBeginTag(HtmlTextWriterTag.Tr);

			writer.AddAttribute(HtmlTextWriterAttribute.Class, "statusBoxStepNumber");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			if (nextAction == null)
				writer.Write("&bull;");
			else if (isUnnumberedStep)
				writer.Write("");
			else
				writer.Write(stepNumber + ".");
			writer.RenderEndTag();
			
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "statusBoxStepText");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.Write(step[stepIndex]);
			writer.RenderEndTag();

			writer.RenderEndTag();
		}

		private void EmitSteps(HtmlTextWriter writer)
		{
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "statusBoxStepsTable");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);
			
            for (int stepNumber = 1; stepNumber <= maxStep; stepNumber++)
				EmitStep(writer, stepNumber);

			if (alternateStep != null)
				EmitAlternateStep(writer);
			
			writer.RenderEndTag();

			if (showSampleImages)
				EmitSampleImages(writer);
			else if (showSampleMaps)
				EmitSampleMaps(writer);
			else if (showSampleZip)
				EmitSampleZip(writer);
			else if (showGraphicHotspotIcon)
				EmitGraphicHotspotIcon(writer);
			else if (showGraphicMapEditorIcon)
				EmitGraphicMapEditorIcon(writer);
		}
		#endregion
	}
}
