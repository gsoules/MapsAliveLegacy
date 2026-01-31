// Copyright (C) 2020 AvantLogic Corporation
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AvantLogic.MapsAlive
{
	public class MapControls : WebControl
	{
		private bool isGallery = false;
		private bool mapCanZoom = false;
        private bool displayShowMapFocusControl = false;

		public MapControls()
		{
		}

		#region ===== Properties ========================================================
		public bool IsGallery
		{
			set { isGallery = value; }
		}

        public bool IsMapEditor
        {
            get { return !isGallery; }
        }

		public bool MapCanZoom
		{
			set { mapCanZoom = value; }
		}

		public bool DisplayShowMapFocusControl
        {
			set { displayShowMapFocusControl = value; }
		}
		#endregion

		#region ===== Public ============================================================
		#endregion

		#region ===== Protected =========================================================

		protected override void RenderContents(HtmlTextWriter writer)
		{
			// Render the thumb table.
			RenderControlPanel(writer);
		}

		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Div; }
		}
		#endregion

		#region ===== Private ===========================================================

		private void AddMapControl(HtmlTextWriter writer, string name, string imageFileName, string tooltip)
        {
			writer.AddAttribute(HtmlTextWriterAttribute.Id, "mapControl" + name);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "mapControl mapControlHidden");
			writer.AddAttribute(HtmlTextWriterAttribute.Src, "../Images/" + imageFileName);
			writer.AddAttribute(HtmlTextWriterAttribute.Alt, tooltip);
			writer.RenderBeginTag(HtmlTextWriterTag.Img);
			writer.RenderEndTag();
		}

        private void AddSpacer(HtmlTextWriter writer)
        {
            writer.Write("&nbsp;&nbsp;&nbsp;&nbsp;");
        }

        private void AddSpacerSmall(HtmlTextWriter writer)
        {
            writer.Write("&nbsp;");
        }

        private void RenderControlPanel(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "mapControls");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

            if (IsMapEditor)
            {
                QuickHelpTitle quickHelpTitle = new QuickHelpTitle();
                quickHelpTitle.Span = true;
                quickHelpTitle.ID = "MapControlsHelp";
                quickHelpTitle.OffsetY = -512;
                quickHelpTitle.OffsetX = 20;
                quickHelpTitle.RenderControl(writer);
                AddSpacer(writer);

                AddMapControl(writer, "ToggleSoftControlKey", "SoftControlKey.png", "Toggle the soft control key on or off");
                AddSpacerSmall(writer);

                AddMapControl(writer, "SelectAllMarkers", "SelectAllMarkers.png", "Select all markers");
                AddSpacerSmall(writer);

                AddMapControl(writer, "ChangeStackOrder", "ChangeStackOrder.png", "Move the selected above or below other markers");
                AddSpacerSmall(writer);

                AddMapControl(writer, "AllowOverZoom", "AllowOverZoom.png", "Enable over-zoom past 100%");
                AddSpacer(writer);
            }

            AddMapControl(writer, "RotateLeft", "RotateLeft.png", "Rotate selected marker(s) left 5&deg; (for 1&deg; press Ctrl or Command)");
			AddMapControl(writer, "RotateRight", "RotateRight.png", "Rotate selected marker(s) right 5&deg; (for 1&deg; press Ctrl or Command)");
            AddSpacerSmall(writer);

            if (IsMapEditor)
            {
                AddMapControl(writer, "NudgeLeft", "NudgeLeft.png", "Nudge selected marker(s) left by 5 pixels (for 1 pixel press Ctrl or Command)");
                AddMapControl(writer, "NudgeUp", "NudgeUp.png", "Nudge selected marker(s) up by 5 pixels (for 1 pixel, press Ctrl or Command)");
                AddMapControl(writer, "NudgeDown", "NudgeDown.png", "Nudge selected marker(s) down by 5 pixels (for 1 pixel press Ctrl or Command)");
                AddMapControl(writer, "NudgeRight", "NudgeRight.png", "Nudge selected marker(s) right by 5 pixels (for 1 pixel press Ctrl or Command)");
                AddMapControl(writer, "AlignColumn", "AlignColumn.png", "Align selected markers in a column");
                AddMapControl(writer, "AlignRow", "AlignRow.png", "Align selected markers in a row");
            }

            AddSpacer(writer);
            AddMapControl(writer, "ToggleAppearance", "ToggleAppearance.png", "Toggle selected and normal marker appearance");

           if (IsMapEditor)
           {
                AddMapControl(writer, "ToggleTransparency", "ToggleTransparency.png", "Toggle transparency of selected marker(s)");
                AddMapControl(writer, "ToggleLockMarker", "MarkerUnlock.png", "Lock or unlock selected marker(s)");
			}

            AddMapControl(writer, "GoToEditScreen", "GoToEditScreen.png", "Go to a Tour Builder screen for the selected hotspot");

            if (IsMapEditor)
            {
			    AddMapControl(writer, "Remove", "Remove.png", "Remove the selected marker(s) from map (does not delete hotspots). To delete hotspots press Ctrl or Command.");

                if (mapCanZoom)
                {
                    AddSpacer(writer);
                    AddMapControl(writer, "SetMapFocus", "SetMapFocus.png", "Set the map focus using the default focus point. To choose the focus point press Ctrl or Command.");
                     
                    if (displayShowMapFocusControl)
                        AddMapControl(writer, "ShowMapFocus", "ShowMapFocus.png", "Show how the map is focused");
                }

                AddSpacer(writer);
                AddMapControl(writer, "ReplaceMarker", "ReplaceMarker.png", "Replace the selected marker(s)");
                AddMapControl(writer, "ReplaceMarkerStyle", "ReplaceMarkerStyle.png", "Replace the marker style of the selected shape marker(s)");

                AddSpacer(writer);
                AddMapControl(writer, "AddNewHotspot", "AddNewHotspot.png", "Add a new hotspot and place its marker on the map");
                AddMapControl(writer, "CreateNewMarker", "CreateNewMarker.png", "Create a new circle, rectangle, line, or polygon shape marker");
                AddMapControl(writer, "DuplicateMarker", "DuplicateMarker.png", "Duplicate the selected shape marker");
                AddMapControl(writer, "EditMarker", "EditMarker.png", "Edit the selected shape marker. Or double-click the marker to edit its shape.");
                AddMapControl(writer, "ConvertHybrid", "ConvertHybrid.png", "");

                AddSpacer(writer);
                AddMapControl(writer, "FinishEditing", "FinishEditing.png", "Accept edits to the selected marker");
                AddMapControl(writer, "UndoEdit", "UndoEdit.png", "Undo last edit");
                AddMapControl(writer, "RedoEdit", "RedoEdit.png", "Redo last edit");

                AddSpacerSmall(writer);
                AddMapControl(writer, "CancelEditing", "CancelEditing.png", "Cancel edits to the selected marker");
            }

            writer.RenderEndTag();
        }

        #endregion
    }
}
