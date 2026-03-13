// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

// This enumeration primarily contains the names of properties that correspond
// directly to TourView properites. There are, however, some pseudo-properties
// like Delete, Instructions, PageId, and SlideId that are not slide properties
// per se, but can appear as columns in an import table.
//
// IMPORTANT: When you add new import items, you must also add them to ExporterForContent.
//
public enum SlideProperty
{
	Categories,
	ClickAction,
	ClickActionTarget,
	Delete,
	DirPreviewImageUrl,
	DirPreviewText,
	ExcludeFromDirectory,
	FirstHotspot,
	HotspotId,
	HotspotOrder,
	Instructions,
	IsDisabled,
	IsHidden,
	IsLocked,
	IsNotAnchored,
	IsRoute,
	IsStatic,
	MapId,
	MarkerName,
    MarkerPctX,
    MarkerPctY,
	MarkerStyle,
	MarkerZooms,
	Media,
	MediaType,
	MessengerFunction,
	MouseoverAction,
	MouseoverActionTarget,
	MouseoutAction,
	MouseoutActionTarget,
	NewHotspotId,
	Notes,
	OpenUrlInNewWindow,
	PopupOverrideHeight,
	PopupOverrideWidth,
	ShowContentWhen,
	Text,
	TourId,
	Title,
	Tooltip,
	UsesLiveData,
	WhenTouchExecuteClick,
	ZoomVisibility
}

public class ImporterForContentXml : ImporterForHotspots
{
	private bool ignoreHotspotForDataSheet;
    private bool importMarkerLocations;
	private string importNote;
	private int markerId;
	private string slidePropertyValue;
	private ReaderForImportedXml readerForImportedXml;
	private bool slideUpdated;
	private string title;
	private bool tourPageChanged;

	public ImporterForContentXml(Tour tour, Stream stream, string reportTitle)
		: base(tour, null, stream, reportTitle)
	{
	}

	private void UpdateTourViewFromContentRecord(TourView tourView, bool isNewSlide)
	{
		slideUpdated = false;
		tourPageChanged = false;

		ImportPropertyExcludeFromDirectory(tourView);
		ImportPropertyMedia(tourView);
		ImportNewHotspotId(tourView);
		ImportPropertyNotes(tourView);
		ImportPropertyText(tourView);
		ImportPropertyTitle(tourView);

		if (!tourPage.IsDataSheet)
		{
			ImportPropertyCategories(tourView);
			ImportPropertyClickAction(tourView);
			ImportPropertyDirPreviewImageUrl(tourView);
			ImportPropertyDirPreviewText(tourView);
			ImportPropertyFirstHotspot(tourView);
			ImportPropertyHotspotOrder(tourView);
			ImportPropertyIsDisabled(tourView);
			ImportPropertyIsHidden(tourView);
			ImportPropertyIsLocked(tourView);
			ImportPropertyIsNotAnchored(tourView);
			ImportPropertyIsRoute(tourView);
			ImportPropertyIsStatic(tourView);
			ImportPropertyMarkerName(tourView);
			ImportPropertyMarkerStyle(tourView);
			ImportPropertyMarkerZooms(tourView);
			ImportPropertyMouseoutAction(tourView);
			ImportPropertyMouseoverAction(tourView);
			ImportPropertyWhenTouchExecuteClick(tourView);
			ImportPropertyPopupOverrideHeight(tourView);
			ImportPropertyPopupOverrideWidth(tourView);
			ImportPropertyShowHotspotWhen(tourView);
			ImportPropertyTooltip(tourView);
			ImportPropertyUsesLiveData(tourView);
			ImportPropertyMessengerFunction(tourView);
			ImportPropertyZoomVisibility(tourView);
		}

		if (tourPageChanged)
			tourPage.UpdateDatabase();

		tourView.UpdateDatabase();

		string column1 = string.Format("{0} : {1}", tourView.TourPage.PageId, tourView.Title);

		if (isNewSlide)
			report.EmitRow(ImportReport.Topic.SlideImported, column1, importNote);
		else if (slideUpdated)
			report.EmitRow(ImportReport.Topic.SlideUpdated, column1, importNote);
		else
			report.EmitRow(ImportReport.Topic.SlideUnchanged, column1, importNote);

		importNote = null;
	}

	public void AddImportError(string note)
	{
		AddImportNote(string.Format("<span style='color:red;'>{0}</span>", note));
	}

	private void AddImportNote(string note)
	{
		if (importNote == null)
			importNote = note;
		else
			importNote += "; " + note;
	}

	private bool DeleteHotspot(TourView tourView)
	{
		if (!GetSlidePropertyValueLower(SlideProperty.Delete))
			return false;

		if (slidePropertyValue != "true")
			return false;

		if (!GetSlidePropertyValue(SlideProperty.HotspotId))
			return false;

		// Delete the slide if it exists. If not, return true so that we know not to import this slide.
		if (tourView == null)
		{
			report.Trace(string.Format("{0} : ignored. The hotspot is marked for delete, but does not exist ", slidePropertyValue));
		}
		else
		{
			tourView.Delete();
			report.Trace(string.Format("{0} : deleted", slidePropertyValue));
			report.EmitRow(ImportReport.Topic.SlideDeleted, tourView.SlideId);

			SlideWasUpdated(SlideProperty.Delete);
		}

		return true;
	}

	private bool GetSlidePropertyValueDouble(SlideProperty property)
	{
		if (!GetSlidePropertyValue(property))
			return false;

		double value;
		if (double.TryParse(slidePropertyValue, out value))
		{
			slidePropertyValue = value.ToString();
			return true;
		}
		else
		{
			AddImportError(string.Format("{0} : ignored (a floating point value is required)", slidePropertyValue));
			return false;
		}
	}

	private bool GetSlidePropertyValueInteger(SlideProperty property)
	{
		if (!GetSlidePropertyValue(property))
			return false;

		int value;
		if (int.TryParse(slidePropertyValue, out value))
		{
			slidePropertyValue = value.ToString();
			return true;
		}
		else
		{
			AddImportError(string.Format("{0} : ignored (an integer value is required)", slidePropertyValue));
			return false;
		}
	}

	private bool GetSlidePropertyValue(SlideProperty property)
	{
		// Make the first letter of the property name lower case to match XML naming convention.
		string propertyName = property.ToString();
		propertyName = propertyName.Substring(0, 1).ToLower() + propertyName.Substring(1);
		
		// Get the property's value from the tour XML. Note that even if the value is empty, we use it.
		// If you don't want to change a property value, leave that property out of the slide XML.
		slidePropertyValue = readerForImportedXml.ReadSlidePropertyValue(propertyName);

		return slidePropertyValue != null;
	}

	private bool GetSlidePropertyValueLower(SlideProperty property)
	{
		if (!GetSlidePropertyValue(property))
			return false;

		slidePropertyValue = slidePropertyValue.ToLower();
		return true;
	}

	private void ImportPropertyCategories(TourView tourView)
	{
		if (!GetSlidePropertyValue(SlideProperty.Categories))
			return;

		CategoryManager categoryManager = tourView.Tour.CategoryManager;

		// Note: the code below makes extensive use of comma separated lists and arrays.
		// Since a user can make mal-formed lists, e.g. "foo,,bar," (has extra commas), and
		// because we use split to convert lists to arrays, there is the possibility that
		// the arrays will contain empty elements. That's why there are so many checks
		// comparing array elements to empty strings.

		// Create an array of the category codes in the tour XML after first removing any spaces in the list.
		string newCodesList = Regex.Replace(slidePropertyValue, @"\s+", "");
		string[] newCodes;
		if (newCodesList.Length == 0)
			newCodes = new string[0];
		else
			newCodes = newCodesList.ToLower().Split(',');

		// Create an array of the categores that the slide currently has.
		string[] oldCodes;
		string oldCodesList = categoryManager.GetCategoryList(tourView.Id);
		if (oldCodesList.Length == 0)
			oldCodes = new string[0];
		else
			oldCodes = oldCodesList.ToLower().Split(',');

		// Create an array of all the categories in the user's account.
		string[] accountCodes;
		string accountCodesList = categoryManager.GetCategoryList();
		if (accountCodesList.Length == 0)
			accountCodes = new string[0];
		else
			accountCodes = accountCodesList.ToLower().Split(',');

		// Identify any categories in the new list that are not in the account.
		ArrayList badCodes = new ArrayList();
		for (int i = 0; i < newCodes.Length; i++)
		{
			bool newCodeExists = false;
			string newCode = newCodes[i];
			
			if (newCode.Length == 0)
				continue;
			
			foreach (string accountCode in accountCodes)
			{
				if (newCode == accountCode.ToLower())
				{
					newCodeExists = true;
					break;
				}
			}
			if (!newCodeExists)
			{
				badCodes.Add(newCodes[i]);
				newCodes[i] = string.Empty;
			}
		}

		// Create a list of categories to be removed from the slide. These are the ones
		// that are in the old codes list, but not in the new codes list.
		ArrayList removeCodes = new ArrayList();
		foreach (string oldCode in oldCodes)
		{
			bool remove = true;
			foreach (string newCode in newCodes)
			{
				if (newCode.Length == 0)
					continue;
				
				if (oldCode == newCode)
				{
					remove = false;
					break;
				}
			}
			if (remove)
				removeCodes.Add(oldCode);
		}

		// Create a list of categories to be added to the slide. These are the ones
		// that are in the new codes list, but not in the old codes list.
		ArrayList addCodes = new ArrayList();
		foreach (string newCode in newCodes)
		{
			if (newCode == string.Empty)
				continue;

			bool add = true;
			foreach (string oldCode in oldCodes)
			{
				if (oldCode.Length == 0)
					continue;
				
				if (newCode == oldCode)
				{
					add = false;
					break;
				}
			}
			if (add)
				addCodes.Add(newCode);
		}

		// Report the bad codes.
		string badCodesList = string.Empty;
		if (badCodes.Count > 0)
		{
			foreach (string badCode in badCodes)
			{
				if (badCodesList != string.Empty)
					badCodesList += ",";
				badCodesList += badCode;
			}
		}

		// Remove old codes that are no longer to be used for this slide.
		string removeCodesList = string.Empty;
		if (removeCodes.Count > 0)
		{
			foreach (string code in removeCodes)
			{
				categoryManager.RemoveCategory(tourView.Id, categoryManager.GetCategory(code).Id);
				
				if (removeCodesList != string.Empty)
					removeCodesList += ",";
				removeCodesList += code;
			}
		}

		// Add new codes to be used for this slide.
		string addCodesList = string.Empty;
		if (addCodes.Count > 0)
		{
			foreach (string code in addCodes)
			{
				TourViewCategory tourViewCategory = categoryManager.AddTourViewCategory(tourView, code);
				if (tourViewCategory == null)
					continue;

				// Tell the tour view when an image-override category has been added
				// since the category will probably cause the image size to change.
				if (tourViewCategory.Category.Type == CategoryType.ImageAreaOverride)
					tourView.SetImageChanged();
				
				if (addCodesList != string.Empty)
					addCodesList += ",";
				addCodesList += code;

			}
		}
		
		// Indicate if any categories were added, removed, or ignored. Note that we report
		// bad codes with either a changed slide or an unchanged slide.
		if (addCodesList.Length > 0 || removeCodesList.Length > 0)
		{
			SlideWasUpdated(SlideProperty.Categories);
			if (addCodesList.Length > 0)
				AddImportNote(string.Format("Added: {0}", addCodesList));
			if (removeCodesList.Length > 0)
				AddImportNote(string.Format("Removed: {0}", removeCodesList));
			if (badCodesList.Length > 0)
				AddImportError(string.Format("Unknown: {0}", badCodesList));
		}
		else if (badCodesList.Length > 0)
		{
			AddImportError(string.Format("Unknown categories: {0}", badCodesList));
		}
	}

	private void ImportPropertyClickAction(TourView tourView)
	{
		string clickAction = GetSlidePropertyValueLower(SlideProperty.ClickAction) ? slidePropertyValue : null;
		if (clickAction == null)
			return;

		string clickActionTarget = GetSlidePropertyValue(SlideProperty.ClickActionTarget) ? slidePropertyValue : null;

		MarkerAction markerAction = MarkerAction.None;
		bool setAction = false;

		if (clickAction == TourView.NameOfMarkerAction(MarkerAction.None))
		{
			markerAction = MarkerAction.None;
			setAction = true;
		}
		else
		{
			if (clickActionTarget == null || clickActionTarget.Length == 0)
			{
				report.Warning(string.Format("ClickActionTarget for {0} is missing or empty", clickAction), tourView.SlideId);
			}
			else
			{
				if (clickAction == TourView.NameOfMarkerAction(MarkerAction.LinkToUrl))
				{
					MarkerAction urlAction;
					GetSlidePropertyValueLower(SlideProperty.OpenUrlInNewWindow);
					if (slidePropertyValue == "true")
						urlAction = MarkerAction.LinkToUrlNewWindow;
					else
						urlAction = MarkerAction.LinkToUrl;
					markerAction = urlAction;
					setAction = true;
				}
				else if (clickAction == TourView.NameOfMarkerAction(MarkerAction.CallJavascript))
				{
					markerAction = MarkerAction.CallJavascript;
					setAction = true;
				}
				else if (clickAction == TourView.NameOfMarkerAction(MarkerAction.GotoPage))
				{
					TourPage targetPage = tourView.Tour.GetTourPageByPageId(clickActionTarget);
					if (targetPage == null)
					{
						AddImportError(string.Format("Click action is Go to Page '{0}', but this tour does not have a map or data sheet with that Id.", clickActionTarget));
					}
					else
					{
						markerAction = MarkerAction.GotoPage;

						// Change the target from the user-defined page Id to the internal page Id.
						clickActionTarget = targetPage.Id.ToString();
						setAction = true;
					}
				}
				else
				{
					report.Warning(string.Format("Ignored unrecognized ClickAction '{0}'", clickAction));
				}
			}
		}

		if (setAction)
		{
			if (tourView.MarkerClickAction != markerAction)
			{
				tourView.MarkerClickAction = markerAction;
				SlideWasUpdated(SlideProperty.ClickAction);
			}
			if (tourView.MarkerClickAction != MarkerAction.None && tourView.MarkerClickActionTarget != clickActionTarget)
			{
				tourView.MarkerClickActionTarget = clickActionTarget;
				SlideWasUpdated(SlideProperty.ClickActionTarget);
			}
		}
	}

	private void ImportPropertyDirPreviewImageUrl(TourView tourView)
	{
		if (!GetSlidePropertyValue(SlideProperty.DirPreviewImageUrl))
			return;

		if (slidePropertyValue != tourView.DirPreviewImageUrl)
		{
			tourView.DirPreviewImageUrl = slidePropertyValue;
			SlideWasUpdated(SlideProperty.DirPreviewImageUrl);
		}
	}

	private void ImportPropertyDirPreviewText(TourView tourView)
	{
		if (!GetSlidePropertyValue(SlideProperty.DirPreviewText))
			return;

		if (slidePropertyValue != tourView.DirPreviewText)
		{
			tourView.DirPreviewText = slidePropertyValue;
			SlideWasUpdated(SlideProperty.DirPreviewText);
		}
	}

	private void ImportPropertyExcludeFromDirectory(TourView tourView)
	{
		if (!GetSlidePropertyValueLower(SlideProperty.ExcludeFromDirectory))
			return;

		bool exclude = slidePropertyValue == "true";

		if (exclude != tourView.ExcludeFromDirectory)
		{
			tourView.ExcludeFromDirectory = exclude;
			SlideWasUpdated(SlideProperty.ExcludeFromDirectory);
		}
	}

	private void ImportPropertyFirstHotspot(TourView tourView)
	{
		if (!GetSlidePropertyValueLower(SlideProperty.FirstHotspot))
			return;

		// You can only set a slide as first -- you can't unset it.
		if (slidePropertyValue != "true")
		{
			AddImportError(string.Format("Ignored: '{0}' (only 'True' is allowed)", slidePropertyValue));
			return;
		}

		if (tourView.Id != tourView.TourPage.FirstTourViewId)
		{
			tourView.TourPage.SetFirstTourView(tourView.Id);
			SlideWasUpdated(SlideProperty.FirstHotspot);
		}
	}

	private void ImportPropertyHotspotOrder(TourView tourView)
	{
		if (!GetSlidePropertyValueInteger(SlideProperty.HotspotOrder))
			return;

		if (slidePropertyValue != tourView.SequenceNumber.ToString())
		{
			tourView.SequenceNumber = int.Parse(slidePropertyValue);
			SlideWasUpdated(SlideProperty.HotspotOrder);
		}
	}

	private void ImportPropertyIsDisabled(TourView tourView)
	{
		if (!GetSlidePropertyValueLower(SlideProperty.IsDisabled))
			return;

		bool isDisabled = slidePropertyValue == "true";

		if (isDisabled != tourView.MarkerIsDisabled)
		{
			tourView.MarkerIsDisabled = isDisabled;
			SlideWasUpdated(SlideProperty.IsDisabled);
		}
	}

	private void ImportPropertyIsHidden(TourView tourView)
	{
		if (!GetSlidePropertyValueLower(SlideProperty.IsHidden))
			return;

		bool isHidden = slidePropertyValue == "true";

		if (isHidden != tourView.MarkerIsHidden)
		{
			tourView.MarkerIsHidden = isHidden;
			SlideWasUpdated(SlideProperty.IsHidden);
		}
	}

	private void ImportPropertyIsLocked(TourView tourView)
	{
		if (!GetSlidePropertyValueLower(SlideProperty.IsLocked))
			return;

		bool isLocked = slidePropertyValue == "true";

		if (isLocked != tourView.MarkerIsLocked)
		{
			tourView.MarkerIsLocked = isLocked;
			SlideWasUpdated(SlideProperty.IsLocked);
		}
	}

	private void ImportPropertyIsNotAnchored(TourView tourView)
	{
		if (!GetSlidePropertyValueLower(SlideProperty.IsNotAnchored))
			return;

		bool isNotAnchored = slidePropertyValue == "true";

		if (isNotAnchored != tourView.MarkerIsNotAnchored)
		{
			tourView.MarkerIsNotAnchored = isNotAnchored;
			SlideWasUpdated(SlideProperty.IsNotAnchored);
		}
	}

	private void ImportPropertyIsRoute(TourView tourView)
	{
		if (!GetSlidePropertyValueLower(SlideProperty.IsRoute))
			return;

		bool isRoute = slidePropertyValue == "true";

		if (isRoute != tourView.MarkerIsRoute)
		{
			tourView.MarkerIsRoute = isRoute;
			SlideWasUpdated(SlideProperty.IsRoute);
		}
	}

	private void ImportPropertyIsStatic(TourView tourView)
	{
		if (!GetSlidePropertyValueLower(SlideProperty.IsStatic))
			return;

		bool isStatic = slidePropertyValue == "true";

		if (isStatic != tourView.MarkerIsStatic)
		{
			tourView.MarkerIsStatic = isStatic;
			SlideWasUpdated(SlideProperty.IsStatic);
		}
	}

	private void ImportPropertyMarkerName(TourView tourView)
	{
		if (!GetSlidePropertyValue(SlideProperty.MarkerName))
			return;

		if (tourView.MarkerIsRoute)
		{
			AddImportError("Cannot set MarkerName for a route hotspot");
			return;
		}

		// Get the tour view's marker and see if it's exclusive.
		Marker marker = Account.GetCachedMarker(tourView.MarkerId);
		if (marker.IsExclusive)
		{
			if (marker.Name != slidePropertyValue)
                AddImportError("Cannot change the name of a bound marker");
			return;
		}

		string markerName = slidePropertyValue;

        string markerPctX = GetSlidePropertyValueDouble(SlideProperty.MarkerPctX) ? slidePropertyValue : "";
        string markerPctY = GetSlidePropertyValueDouble(SlideProperty.MarkerPctY) ? slidePropertyValue : "";

        // This code should be calling Account.GetCachedMarker, but only after that method is
        // changed to use the stored procedure used here. The old sp_Marker_GetMarkerByName
        // does not pass the account Id and therefore can return multiple markers! For now we
        // are making the DB call here so that we can release version 2.6. Fix this in version 3.
        MapsAliveDataRow row = MapsAliveDatabase.LoadDataRow("sp_Marker_GetMarkerIdByMarkerName", "@Name", markerName, "@AccountId", Utility.AccountId);
		if (row == null)
		{
			AddImportError(string.Format("Unknown MarkerName: '{0}'", markerName));
			return;
		}
		else
		{
			int markerId = row.IntValue("MarkerId");
			if (tourView.MarkerId != markerId)
			{
				tourView.MarkerId = markerId;
				SlideWasUpdated(SlideProperty.MarkerName);
			}

            if (importMarkerLocations && markerPctX != string.Empty && markerPctY != string.Empty)
            {
                double pctX = double.Parse(markerPctX);
                double pctY = double.Parse(markerPctY);
                if (tourView.MarkerPctX != pctX)
                {
                    tourView.MarkerPctX = pctX;
                    SlideWasUpdated(SlideProperty.MarkerPctX);
                }
                if (tourView.MarkerPctY != pctY)
                {
                    tourView.MarkerPctY = pctY;
                    SlideWasUpdated(SlideProperty.MarkerPctY);
                }
            }
        }
	}

	private void ImportPropertyMarkerStyle(TourView tourView)
	{
		if (!GetSlidePropertyValueLower(SlideProperty.MarkerStyle))
			return;

		Marker marker = Account.GetCachedMarker(tourView.MarkerId);

		string markerStyleName = slidePropertyValue;
		string sp = "sp_MarkerStyle_GetMarkerStyleByName";
		int markerStyleId = MapsAliveDatabase.ReadInt(sp, "@AccountId", Utility.AccountId, "@Name", markerStyleName);

		if (markerStyleId == 0)
		{
			AddImportError(string.Format("Unknown MarkerStyle: '{0}'", markerStyleName));
			return;
		}

		if (marker.MarkerStyle.Id != markerStyleId)
		{
		    if (!marker.IsExclusive)
		    {
			    // Defer this error until now so that it won't appear if the style is not going to change.

                // Don't report the error because it's very confusing, and it's really only a notification, not an error.
                // AddImportError("Cannot change the style of a marker that is not bound to a hotspot");
			    return;
		    }

			// Get the marker style the user wants to switch to.
			MarkerStyle replacementMarkerStyle = Account.GetCachedMarkerStyle(markerStyleId);
			marker.MarkerStyle = replacementMarkerStyle;
			marker.UpdateResourceAndDependents();
			SlideWasUpdated(SlideProperty.MarkerStyle);
		}
	}

	private void ImportPropertyMarkerZooms(TourView tourView)
	{
		if (!GetSlidePropertyValueLower(SlideProperty.MarkerZooms))
			return;

		MarkerZoomType markerZoomType;
		if (slidePropertyValue == "true")
			markerZoomType = MarkerZoomType.DoesZoom;
		else if (slidePropertyValue == "false")
			markerZoomType = MarkerZoomType.DoesNotZoom;
		else
			markerZoomType = MarkerZoomType.Default;

		if (markerZoomType != tourView.MarkerZoomType)
		{
			tourView.MarkerZoomType = markerZoomType;
			SlideWasUpdated(SlideProperty.MarkerZooms);
		}
	}

	private void ImportPropertyMedia(TourView tourView)
	{
		// Both the media type and value must be set together.

		if (!GetSlidePropertyValueLower(SlideProperty.MediaType))
			return;

		string mediaTypeValue = slidePropertyValue;

		if (!GetSlidePropertyValue(SlideProperty.Media))
			return;

		string mediaValue = slidePropertyValue;

		if (mediaTypeValue == TourView.NameOfMediaType(SlideMediaType.Photo))
		{
			AddImportError("Photo media cannot be imported");
			return;
		}

		if (mediaTypeValue == TourView.NameOfMediaType(SlideMediaType.Embed))
		{
			if (tourView.EmbedText != mediaValue)
			{
				Size embedSize;
				string errorMessage;
				bool valid = MediaParser.ParseEmbedCode(ref mediaValue, out embedSize, out errorMessage);
				if (valid)
				{
					tourView.EmbedText = mediaValue;
					tourView.EmbedWidth = embedSize.Width;
					tourView.EmbedHeight = embedSize.Height;
				}
				else
				{
					AddImportError("Media: " + errorMessage);
					return;
				}
			}
		}

		if (mediaValue != tourView.EmbedText)
		{
			SlideWasUpdated(SlideProperty.Media);
		}
	}

	private void ImportPropertyMessengerFunction(TourView tourView)
	{
		if (!GetSlidePropertyValue(SlideProperty.MessengerFunction))
			return;

		if (!MapsAliveState.Account.IsProPlan)
		{
			AddImportNote("Request function import requires a Pro Plan");
			return;
		}

		if (slidePropertyValue != tourView.MessengerFunction)
		{
			tourView.MessengerFunction = slidePropertyValue;
			SlideWasUpdated(SlideProperty.MessengerFunction);
		}
	}

	private void ImportPropertyMouseoutAction(TourView tourView)
	{
		if (!GetSlidePropertyValueLower(SlideProperty.MouseoutAction))
			return;

		MarkerAction markerAction = MarkerAction.None;
		bool setAction = false;

		if (slidePropertyValue == TourView.NameOfMarkerAction(MarkerAction.CallJavascript))
		{
			markerAction = MarkerAction.CallJavascript;
			setAction = true;
		}
		else if (slidePropertyValue == TourView.NameOfMarkerAction(MarkerAction.None))
		{
			markerAction = MarkerAction.None;
			setAction = true;
		}
		else
		{
			report.Warning(string.Format("Ignored unrecognized MouseoutAction '{0}'", slidePropertyValue, tourView.SlideId));
		}

		if (setAction && tourView.MarkerRolloutAction != markerAction)
		{
			tourView.MarkerRolloutAction = markerAction;
			SlideWasUpdated(SlideProperty.MouseoutAction);
		}

		if (tourView.MarkerRolloutAction == MarkerAction.CallJavascript)
		{
			string mouseoutActionTarget = GetSlidePropertyValue(SlideProperty.MouseoutActionTarget) ? slidePropertyValue : null;
			if (mouseoutActionTarget == null || mouseoutActionTarget.Length == 0)
			{
				report.Warning("MouseoutActionTarget JavaScript is missing or empty", tourView.SlideId);
			}
			else if (tourView.MarkerRolloutActionTarget != mouseoutActionTarget)
			{
				tourView.MarkerRolloutActionTarget = mouseoutActionTarget;
				SlideWasUpdated(SlideProperty.MouseoutActionTarget);
			}
		}
	}

	private void ImportPropertyMouseoverAction(TourView tourView)
	{
		if (!GetSlidePropertyValueLower(SlideProperty.MouseoverAction))
			return;

		MarkerAction markerAction = MarkerAction.None;
		bool setAction = false;

		if (slidePropertyValue == TourView.NameOfMarkerAction(MarkerAction.CallJavascript))
		{
			markerAction = MarkerAction.CallJavascript;
			setAction = true;
		}
		else if (slidePropertyValue == TourView.NameOfMarkerAction(MarkerAction.None))
		{
			markerAction = MarkerAction.None;
			setAction = true;
		}
		else
		{
			report.Warning(string.Format("Ignored unrecognized MouseoverAction '{0}'", slidePropertyValue, tourView.SlideId));
		}

		if (setAction && tourView.MarkerRolloverAction != markerAction)
		{
			tourView.MarkerRolloverAction = markerAction;
			SlideWasUpdated(SlideProperty.MouseoverAction);
		}

		if (tourView.MarkerRolloverAction == MarkerAction.CallJavascript)
		{
			string mouseoverActionTarget = GetSlidePropertyValue(SlideProperty.MouseoverActionTarget) ? slidePropertyValue : null;
			if (mouseoverActionTarget == null || mouseoverActionTarget.Length == 0)
			{
				report.Warning("MouseoverActionTarget JavaScript is missing or empty", tourView.SlideId);
			}
			else if (tourView.MarkerRolloverActionTarget != mouseoverActionTarget)
			{
				tourView.MarkerRolloverActionTarget = mouseoverActionTarget;
				SlideWasUpdated(SlideProperty.MouseoverActionTarget);
			}
		}
	}

	private void ImportNewHotspotId(TourView tourView)
	{
		if (!GetSlidePropertyValue(SlideProperty.NewHotspotId))
			return;

		if (!Utility.IsValidFileName(slidePropertyValue))
		{
			slideUpdated = false;
			AddImportError(string.Format("NewSlideId '{0}' invalid", slidePropertyValue));
			return;
		}

		if (tourPage.IsDataSheet)
		{
			if (slidePropertyValue != tourPage.PageId)
			{
				if (tourPage.Tour.GetTourPageByPageId(slidePropertyValue) == null)
				{
					tourPage.PageId = slidePropertyValue;
					tourView.SlideId = slidePropertyValue;
					tourPageChanged = true;
				}
				else
				{
					// A map or data sheet with the new slide Id already exists.
					AddImportError(string.Format("NewHotspotId '{0}' already in use", slidePropertyValue));
					return;
				}

				SlideWasUpdated(SlideProperty.NewHotspotId);
			}
		}
		else
		{
			if (slidePropertyValue != tourView.SlideId)
			{
				if (tourPage.GetTourViewBySlideId(slidePropertyValue) == null)
				{
					tourView.SlideId = slidePropertyValue;
				}
				else
				{
					// A tour view with the new slide Id already exists.
					AddImportError(string.Format("NewHotspotId '{0}' already in use", slidePropertyValue));
					return;
				}

				SlideWasUpdated(SlideProperty.NewHotspotId);
			}
		}
	}

	private void ImportPropertyPopupOverrideHeight(TourView tourView)
	{
		if (!GetSlidePropertyValueInteger(SlideProperty.PopupOverrideHeight))
			return;

		if (slidePropertyValue != tourView.SlideHeightOverride.ToString())
		{
			tourView.SlideHeightOverride = int.Parse(slidePropertyValue);
			SlideWasUpdated(SlideProperty.PopupOverrideHeight);
		}
	}

	private void ImportPropertyPopupOverrideWidth(TourView tourView)
	{
		if (!GetSlidePropertyValueInteger(SlideProperty.PopupOverrideWidth))
			return;

		if (slidePropertyValue != tourView.SlideWidthOverride.ToString())
		{
			tourView.SlideWidthOverride = int.Parse(slidePropertyValue);
			SlideWasUpdated(SlideProperty.PopupOverrideWidth);
		}
	}

	private void ImportPropertyNotes(TourView tourView)
	{
		if (!GetSlidePropertyValue(SlideProperty.Notes))
			return;

		if (slidePropertyValue != tourView.Notes)
		{
			tourView.Notes = slidePropertyValue;
			SlideWasUpdated(SlideProperty.Notes);
		}
	}

	private void ImportPropertyShowHotspotWhen(TourView tourView)
	{
		if (!GetSlidePropertyValueLower(SlideProperty.ShowContentWhen))
			return;

		ShowContentEvent showContentEvent = ShowContentEvent.OnMouseover;
		bool setAction = false;

		if (slidePropertyValue == TourView.NameOfShowContentEvent(ShowContentEvent.OnMouseover))
		{
			showContentEvent = ShowContentEvent.OnMouseover;
			setAction = true;
		}
		else if (slidePropertyValue == TourView.NameOfShowContentEvent(ShowContentEvent.OnClick))
		{
			showContentEvent = ShowContentEvent.OnClick;
			setAction = true;
		}
		else if (slidePropertyValue == TourView.NameOfShowContentEvent(ShowContentEvent.Never))
		{
			showContentEvent = ShowContentEvent.Never;
			setAction = true;
		}
		else
		{
			report.Warning(string.Format("Ignored unrecognized ShowContentWhen option '{0}'", slidePropertyValue));
		}

		if (setAction && tourView.ShowContentEvent != showContentEvent)
		{
			tourView.ShowContentEvent = showContentEvent;
			SlideWasUpdated(SlideProperty.ShowContentWhen);
		}
	}

	private void ImportPropertyText(TourView tourView)
	{
		if (!GetSlidePropertyValue(SlideProperty.Text))
			return;

		if (slidePropertyValue != tourView.DescriptionHtml)
		{
			tourView.DescriptionHtml = slidePropertyValue;
			SlideWasUpdated(SlideProperty.Text);
		}
	}

	private void ImportPropertyTitle(TourView tourView)
	{
		if (!GetSlidePropertyValue(SlideProperty.Title))
			return;

		if (tourPage.IsDataSheet)
		{
			if (slidePropertyValue != tourPage.Name)
			{
				tourPage.Name = slidePropertyValue;
				tourPageChanged = true;
				SlideWasUpdated(SlideProperty.Title);
			}
		}
		else
		{
			if (slidePropertyValue != tourView.Title)
			{
				tourView.Title = slidePropertyValue;
				SlideWasUpdated(SlideProperty.Title);
			}
		}
	}

	private void ImportPropertyTooltip(TourView tourView)
	{
		if (!GetSlidePropertyValue(SlideProperty.Tooltip))
			return;

		if (slidePropertyValue != tourView.ToolTip)
		{
			tourView.ToolTip = slidePropertyValue;
			SlideWasUpdated(SlideProperty.Tooltip);
		}
	}

	private void ImportPropertyUsesLiveData(TourView tourView)
	{
		if (!GetSlidePropertyValueLower(SlideProperty.UsesLiveData))
			return;

		if (!MapsAliveState.Account.IsPlusOrProPlan)
		{
			AddImportNote("Live Data import requires a Plus or Pro Plan");
			return;
		}

		bool usesLiveData = slidePropertyValue == "true";

		if (usesLiveData != tourView.UsesLiveData)
		{
			tourView.UsesLiveData = usesLiveData;
			SlideWasUpdated(SlideProperty.UsesLiveData);
		}
	}

	private void ImportPropertyWhenTouchExecuteClick(TourView tourView)
	{
		if (!GetSlidePropertyValueLower(SlideProperty.WhenTouchExecuteClick))
			return;

		bool executeClick = slidePropertyValue == "true";

		if (executeClick != tourView.TouchPerformsClickAction)
		{
			tourView.TouchPerformsClickAction = executeClick;
			SlideWasUpdated(SlideProperty.WhenTouchExecuteClick);
		}
	}

	private void ImportPropertyZoomVisibility(TourView tourView)
	{
		if (!GetSlidePropertyValueInteger(SlideProperty.ZoomVisibility))
			return;

		int value = int.Parse(slidePropertyValue);
		if (value < -100 || value > 100)
		{
			AddImportError(string.Format("{0} is not in the range -100 to 100", slidePropertyValue));
			return;
		}

		if (slidePropertyValue != tourView.MarkerZoomThreshold.ToString())
		{
			tourView.MarkerZoomThreshold = value;
			SlideWasUpdated(SlideProperty.ZoomVisibility);
		}
	}

	private void ImportDataSheet(string slideId, TourView tourView)
	{
		if (tourView == null)
		{
			report.Trace(string.Format("{0} : ignored because it does not match '{1}'", slideId, tourPage.PageId));
			return;
		}

		if (ignoreHotspotForDataSheet)
		{
			report.Trace(string.Format("{0} : ignored because only one update is allowed per data sheet", slideId));
			return;
		}

		UpdateTourViewFromContentRecord(tourView, false);
		
		if (slideUpdated)
			report.Trace(string.Format("{0} : Updated", slideId));
		else
			report.Trace(string.Format("{0} : Unchanged", slideId));

		ignoreHotspotForDataSheet = true;
	}

	private void ImportHotspot(string slideId, TourView tourView)
	{
		// If the import data contains a Delete column and the value is "true", delete the slide and quit.
		if (DeleteHotspot(tourView))
			return;

		bool isNewSlide = false;
		string newSlideId = null;
		if (tourView == null)
		{
			GetSlidePropertyValue(SlideProperty.NewHotspotId);
			newSlideId = slidePropertyValue;
			if (newSlideId != null && newSlideId != string.Empty)
			{
				// We didn't find a view for the SlideId, but this same record has a NewSlideId.
				// Be safe and assume that this data is being imported again after all old ids were
				// changed to new ids on a previous import. In that case, we must not create new
				// slides for the old ids.
				tourView = GetTourViewBySlideId(tourPage, newSlideId, title);

				// We can't match this record with a view so ignore it.
				if (tourView == null)
				{
					report.EmitRow(ImportReport.Topic.SlideUnchanged, slideId, "No hotspot found");
					return;
				}
				else
				{
					// It's a reimport.
					report.Trace(string.Format("Ignored hotspot '{0}' which was previously changed to '{1}'", slideId, newSlideId));
				}
			}

			// No tour view on this page has an import key that matches the content key or title.
			if (tourView == null)
			{
				// Create a new tour view for this hotspot.
				GetSlidePropertyValue(SlideProperty.Title);
				title = slidePropertyValue;
				if (title == null || title == string.Empty)
					title = slideId;
				tourView = CreateSlide(slideId, title, markerId, tourPage);
				isNewSlide = true;
				report.Trace(string.Format("{0} : New", slideId));
			}
		}
		
		if (tourView != null)
			UpdateTourViewFromContentRecord(tourView, isNewSlide);

		if (!isNewSlide)
		{
			if (slideUpdated)
				report.Trace(string.Format("{0} : Updated", slideId));
			else
				report.Trace(string.Format("{0} : Unchanged", slideId));
		}
	}

	private void ImportSlide()
	{
		GetSlidePropertyValue(SlideProperty.HotspotId);
		string slideId = slidePropertyValue;

		if (slideId == null || slideId == string.Empty)
		{
			report.Trace(string.Format("Hotspot element {0} has no hotspotId", readerForImportedXml.CurrentSlideNumber + 2));
		}
		else
		{
			TourView tourView = GetTourViewBySlideId(tourPage, slideId, title);
			if (tourPage.IsDataSheet)
				ImportDataSheet(slideId, tourView);
			else
				ImportHotspot(slideId, tourView);
		}
	}

	public void ImportContentXml(int markerId, bool importMarkerLocations)
	{
		this.markerId = markerId;
        this.importMarkerLocations = importMarkerLocations;

		readerForImportedXml = new ReaderForImportedXml(tour, report);

		try
		{
			// Create a reader to let us read the data as tour XML.
			bool loaded = readerForImportedXml.LoadTourXml(stream, out message);

			if (loaded)
			{
				report.Trace("IMPORTING TOUR XML");
				
				foreach (TourPage page in tour.TourPages)
				{
					// Note: Import used to be for 1 page and this.tourPage was it. Now we import for the
					// entire tour, but we current use this.tourPage to keep track of the "active" tour page.
					this.tourPage = page;

					int hotspotsOnPage = readerForImportedXml.PositionToTourPage(tourPage.PageId);

					if (hotspotsOnPage > 0)
					{
						report.Trace(string.Format("Importing into <b>{0}</b> ({1})", tourPage.Name, tourPage.PageId));

						while (readerForImportedXml.ReadSlide())
						{
							ImportSlide();

							string status = string.Format("record {0}", readerForImportedXml.CurrentSlideNumber);
							ProgressMonitor.Update(status, readerForImportedXml.SlideCount, readerForImportedXml.SlidesRead);

							if (importFailed || !OkToKeepImporting)
								break;
						}
					}
				}
			}
			else
			{
				importFailed = true;
			}
		}
		catch (Exception ex)
		{
			message = ex.Message;
			importFailed = true;
		}

		tour.ReloadCategories();
	}

	private void SlideWasUpdated(SlideProperty property)
	{
		slideUpdated = true;
		AddImportNote(property.ToString());
	}
}
