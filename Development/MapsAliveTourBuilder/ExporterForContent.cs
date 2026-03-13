// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Xml;
using DataStreams.Csv;
using DataStreams.Common;

public class ExporterForContent : Exporter
{
	// This class is used as an associative array. While not very efficient, it allows
	// us to add new export properties without having to define them -- we can just add
	// them to the array associatively by their SlideProperty name.
	private class HotspotProperties
	{
		Hashtable properties = new Hashtable();

		public void Export(SlideProperty property)
		{
			Set(property, true);
		}

		public string Get(SlideProperty property)
		{
			object value = properties[property];
			return value == null ? string.Empty : (string)value;
		}

		public bool OkToExport(SlideProperty property)
		{
			return Get(property) == "true";
		}

		public void Set(SlideProperty property, string value)
		{
			// If the property is already set, first remove it and then set it to avoid a duplicate
			// key error. This should only happen when marking a column to be exported.
			if (properties.Contains(property))
				properties.Remove(property);
			
			properties.Add(property, value);
		}

		public void Set(SlideProperty property, int value)
		{
			Set(property, value.ToString());
		}

		public void Set(SlideProperty property, bool value)
		{
			Set(property, value ? "true" : "false");
		}

		public void Set(SlideProperty property)
		{
			properties.Add(property, property.ToString());
		}
	}

	private HotspotProperties currentRow;
	private HotspotProperties exports;
	private ArrayList header;
	private ArrayList rows;
	private Tour tour;

	// The logic here has been designed to be data-driven. It lets us add new export
	// properties by only adding three lines of code for each one. It creates an
	// in-memory representation of a spreadsheet to be exported as CSV. The spreadsheet
	// contains a column each hotspot property that can be export. We then make a pass
	// over the columns to identify which ones should be exported and which should be
	// skipped. The columns marked for export can then be written out in CSV form. We
	// use the spreadsheet representation because it corresponds to the logic that allows
	// us to import hotspots from an actual CSV or Excel spreadsheet.

	public ExporterForContent(Tour tour)
	{
		this.tour = tour;
		header = new ArrayList();
		rows = new ArrayList();
	}

	public void CreateContentCsvFile(string fileLocation)
	{
		CreateRowsToBeExported();
		ExportRowsToCsvFile(fileLocation);
	}

	public void CreateContentXmlFile(string fileLocation)
	{
		CreateRowsToBeExported();
		ExportRowsToXmlFile(fileLocation);
	}

	// ------------------------------------------------------------------------
	// Begining of  conditional columns section.
	//
	// Edit the three methods below when you want to add or modify one of the
	// columns that is not always exported. You shouldn't have to edit elsewhere.
	// ------------------------------------------------------------------------

	private void CreateConditionalHeaderRowColumns()
	{
		// The order here is the order in which columns will appear left to right in the spreadsheet.
		SetHeaderRowColumn(SlideProperty.Tooltip);
		SetHeaderRowColumn(SlideProperty.Categories);
		SetHeaderRowColumn(SlideProperty.ShowContentWhen);
		SetHeaderRowColumn(SlideProperty.ClickAction);
		SetHeaderRowColumn(SlideProperty.ClickActionTarget);
		SetHeaderRowColumn(SlideProperty.OpenUrlInNewWindow);
		SetHeaderRowColumn(SlideProperty.MouseoverAction);
		SetHeaderRowColumn(SlideProperty.MouseoverActionTarget);
		SetHeaderRowColumn(SlideProperty.MouseoutAction);
		SetHeaderRowColumn(SlideProperty.MouseoutActionTarget);
		SetHeaderRowColumn(SlideProperty.WhenTouchExecuteClick);
		SetHeaderRowColumn(SlideProperty.ExcludeFromDirectory);
		SetHeaderRowColumn(SlideProperty.FirstHotspot);
		SetHeaderRowColumn(SlideProperty.HotspotOrder);
		SetHeaderRowColumn(SlideProperty.MarkerName);
		SetHeaderRowColumn(SlideProperty.MarkerPctX);
		SetHeaderRowColumn(SlideProperty.MarkerPctY);
		SetHeaderRowColumn(SlideProperty.MarkerStyle);
		SetHeaderRowColumn(SlideProperty.IsDisabled);
		SetHeaderRowColumn(SlideProperty.IsHidden);
		SetHeaderRowColumn(SlideProperty.IsLocked);
		SetHeaderRowColumn(SlideProperty.IsNotAnchored);
		SetHeaderRowColumn(SlideProperty.IsRoute);
		SetHeaderRowColumn(SlideProperty.IsStatic);
		SetHeaderRowColumn(SlideProperty.MarkerZooms);
		SetHeaderRowColumn(SlideProperty.ZoomVisibility);
		SetHeaderRowColumn(SlideProperty.MediaType);
		SetHeaderRowColumn(SlideProperty.Media);
		SetHeaderRowColumn(SlideProperty.PopupOverrideWidth);
		SetHeaderRowColumn(SlideProperty.PopupOverrideHeight);
		SetHeaderRowColumn(SlideProperty.DirPreviewImageUrl);
		SetHeaderRowColumn(SlideProperty.DirPreviewText);
		SetHeaderRowColumn(SlideProperty.UsesLiveData);
		SetHeaderRowColumn(SlideProperty.MessengerFunction);
		SetHeaderRowColumn(SlideProperty.Notes);
	}

	private void CreateConditionalContentRowColumns(TourView tourView, HotspotProperties row)
	{
		if (tourView.TourPage.IsDataSheet)
		{
			row.Set(SlideProperty.MediaType, TourView.NameOfMediaType(tourView.MediaType));
			row.Set(SlideProperty.Media, tourView.EmbedText);
			
			row.Set(SlideProperty.Notes, tourView.Notes);
		}
		else
		{
			row.Set(SlideProperty.Tooltip, tourView.ToolTip);
			
			row.Set(SlideProperty.Categories, tourView.Tour.CategoryManager.GetCategoryList(tourView.Id));

			row.Set(SlideProperty.ShowContentWhen, TourView.NameOfShowContentEvent(tourView.ShowContentEvent));
				
			row.Set(SlideProperty.ClickAction, TourView.NameOfMarkerAction(tourView.MarkerClickAction));
			
			string clickActionTarget = tourView.MarkerClickActionTarget;
			if (tourView.MarkerClickAction == MarkerAction.GotoPage)
			{
				TourPage targetTourPage = tourView.TourPage.Tour.GetTourPage(int.Parse(clickActionTarget));
				clickActionTarget = targetTourPage.PageId;
			}
			row.Set(SlideProperty.ClickActionTarget, clickActionTarget);
			
			row.Set(SlideProperty.OpenUrlInNewWindow, tourView.MarkerClickAction == MarkerAction.LinkToUrlNewWindow);
			row.Set(SlideProperty.MouseoverAction, TourView.NameOfMarkerAction(tourView.MarkerRolloverAction));
			row.Set(SlideProperty.MouseoverActionTarget, tourView.MarkerRolloverActionTarget);
			row.Set(SlideProperty.MouseoutAction, TourView.NameOfMarkerAction(tourView.MarkerRolloutAction));
			row.Set(SlideProperty.MouseoutActionTarget, tourView.MarkerRolloutActionTarget);
			row.Set(SlideProperty.WhenTouchExecuteClick, tourView.TouchPerformsClickAction);
			
			row.Set(SlideProperty.ExcludeFromDirectory, TrueOrEmpty(tourView.ExcludeFromDirectory));
			row.Set(SlideProperty.FirstHotspot, TrueOrEmpty(tourView.TourPage.FirstTourViewId == tourView.Id));
			row.Set(SlideProperty.HotspotOrder, tourView.SequenceNumber);

			string markerName = string.Empty;
            string markerPctX = string.Empty;
            string markerPctY = string.Empty;
			string markerStyleName = string.Empty;
			if (!tourView.MarkerIsRoute)
			{
				Marker marker = Account.GetCachedMarker(tourView.MarkerId);
				markerName = marker.Name;
                markerPctX = tourView.MarkerPctX.ToString();
                markerPctY = tourView.MarkerPctY.ToString();
				markerStyleName = marker.MarkerStyle.Name;
			}
			row.Set(SlideProperty.MarkerName, markerName);
			row.Set(SlideProperty.MarkerPctX, markerPctX);
			row.Set(SlideProperty.MarkerPctY, markerPctY);
			row.Set(SlideProperty.MarkerStyle, markerStyleName);
			
			row.Set(SlideProperty.IsDisabled, tourView.MarkerIsDisabled);
			row.Set(SlideProperty.IsHidden, tourView.MarkerIsHidden);
			row.Set(SlideProperty.IsStatic, tourView.MarkerIsStatic);
			row.Set(SlideProperty.IsRoute, tourView.MarkerIsRoute);
			row.Set(SlideProperty.IsLocked, tourView.MarkerIsLocked);
			row.Set(SlideProperty.IsNotAnchored, tourView.MarkerIsNotAnchored);
			row.Set(SlideProperty.MarkerZooms, tourView.MarkerZooms);
			row.Set(SlideProperty.ZoomVisibility, tourView.MarkerZoomThreshold);
						
			row.Set(SlideProperty.MediaType, TourView.NameOfMediaType(tourView.MediaType));
			row.Set(SlideProperty.Media, tourView.EmbedText);
			
			row.Set(SlideProperty.PopupOverrideWidth, tourView.SlideWidthOverride);
			row.Set(SlideProperty.PopupOverrideHeight, tourView.SlideHeightOverride);
			
			row.Set(SlideProperty.DirPreviewImageUrl, tourView.DirPreviewImageUrl);
			row.Set(SlideProperty.DirPreviewText, tourView.DirPreviewText);
			
			row.Set(SlideProperty.UsesLiveData, tourView.UsesLiveData);
			row.Set(SlideProperty.MessengerFunction, tourView.MessengerFunction);
			
			row.Set(SlideProperty.Notes, tourView.Notes);
		}
	}

	private void MarkConditionalColumnsToBeExported()
	{
		ExportIfNotEmpty(SlideProperty.Tooltip);
		
		ExportIfNotEmpty(SlideProperty.Categories);

		ExportIfNotDefault(SlideProperty.ShowContentWhen, TourView.NameOfShowContentEvent(ShowContentEvent.OnMouseover));

		ExportIfNotDefault(SlideProperty.ClickAction, TourView.NameOfMarkerAction(MarkerAction.None), SlideProperty.ClickActionTarget);
		ExportIfTrue(SlideProperty.OpenUrlInNewWindow);
		ExportIfNotDefault(SlideProperty.MouseoverAction, TourView.NameOfMarkerAction(MarkerAction.None), SlideProperty.MouseoverActionTarget);
		ExportIfNotDefault(SlideProperty.MouseoutAction, TourView.NameOfMarkerAction(MarkerAction.None), SlideProperty.MouseoutActionTarget);
		ExportIfFalse(SlideProperty.WhenTouchExecuteClick);

		ExportIfTrue(SlideProperty.ExcludeFromDirectory);
		ExportIfTrue(SlideProperty.FirstHotspot);
		ExportIfNotZero(SlideProperty.HotspotOrder);

		ExportIfNotEmpty(SlideProperty.MarkerName);
		ExportIfNotEmpty(SlideProperty.MarkerPctX);
		ExportIfNotEmpty(SlideProperty.MarkerPctY);
		ExportIfNotEmpty(SlideProperty.MarkerStyle);
		ExportIfTrue(SlideProperty.IsDisabled);
		ExportIfTrue(SlideProperty.IsHidden);
		ExportIfTrue(SlideProperty.IsStatic);
		ExportIfTrue(SlideProperty.IsRoute);
		ExportIfTrue(SlideProperty.IsLocked);
		ExportIfTrue(SlideProperty.IsNotAnchored);
		ExportIfNotZero(SlideProperty.ZoomVisibility);

        // The default for MarkerZooms was an account preference in V3 set to false.
        // There is no account preference in V4 and the default is true.
		ExportIfNotDefault(SlideProperty.MarkerZooms, tour.V4 ? true : false);

		ExportIfNotDefault(SlideProperty.MediaType, TourView.NameOfMediaType(SlideMediaType.Photo), SlideProperty.Media);

		ExportIfNotZero(SlideProperty.PopupOverrideWidth);
		ExportIfNotZero(SlideProperty.PopupOverrideHeight);

		ExportIfNotEmpty(SlideProperty.DirPreviewImageUrl);
		ExportIfNotEmpty(SlideProperty.DirPreviewText);

		ExportIfNotDefault(SlideProperty.UsesLiveData, "false", SlideProperty.MessengerFunction);

		ExportIfNotEmpty(SlideProperty.Notes);
	}

	// ------------------------------------------------------------------------
	// End of conditional columns section.
	// ------------------------------------------------------------------------

	private void CreateContentRow(TourPage tourPage, TourView tourView)
	{
		HotspotProperties row = new HotspotProperties();

		// These columns are always emitted. Note that we don't emit the TourId and MapId
		// columns because they are set in the Use Template row that is emitted for each tour page.
		row.Set(SlideProperty.HotspotId, tourView.SlideId);
		row.Set(SlideProperty.Title, tourView.Title);
		row.Set(SlideProperty.Text, tourView.DescriptionHtml);

		CreateConditionalContentRowColumns(tourView, row);
		
		rows.Add(row);
	}

	private void CreateHeaderRow()
	{
		currentRow = new HotspotProperties();
		
		
		// These columns will always be exported.
		// The order here is the order in which columns will appear left to right in the spreadsheet.
		SetHeaderRowColumn(SlideProperty.Instructions);
		SetHeaderRowColumn(SlideProperty.TourId);
		SetHeaderRowColumn(SlideProperty.MapId);
		SetHeaderRowColumn(SlideProperty.HotspotId);
		SetHeaderRowColumn(SlideProperty.Text);
		SetHeaderRowColumn(SlideProperty.Title);
		
		// These columns will only be exported if their content gets marked for export.
		CreateConditionalHeaderRowColumns();

		rows.Add(currentRow);
	}

	private void CreateRowsToBeExported()
	{
		// Create the top row which contains the column names.
		CreateHeaderRow();

		foreach (TourPage tourPage in tour.TourPages)
		{
			// Create a "Use Template" row for each page.
			CreateUseTemplateRow(tourPage);

			// Create the content rows for the current page.
			foreach (TourView tourView in tourPage.TourViews)
			{
				CreateContentRow(tourPage, tourView);
			}
		}

		// Determine which columns contain data that should be exported.
		MarkColumnsToBeExported();
	}

	private void CreateUseTemplateRow(TourPage tourPage)
	{
		// This row is written once for each page in the tour.
		HotspotProperties row = new HotspotProperties();
		row.Set(SlideProperty.TourId, tour.Id);
		row.Set(SlideProperty.MapId, tourPage.PageId);
		row.Set(SlideProperty.Instructions, "Use Template");
		rows.Add(row);
	}

	private void EmitHotspotXml(HotspotProperties row)
	{
		xmlWriter.WriteStartElement("hotspot");

		foreach (SlideProperty property in header)
		{
			if (!OkToExportXmlElement(row, property))
				continue;

			EmitElement(property, row.Get(property));
		}

		xmlWriter.WriteEndElement(); // hotspot
	}

	private void Export(SlideProperty property)
	{
		// Set this property for export, but check first to see if it's already
		// set to avoid getting a duplicate key error on the hash table.
		if (!OkToExport(property))
			exports.Export(property);
	}

	private void ExportIfNotDefault(SlideProperty property, bool value)
	{
		string text = currentRow.Get(property);
		if (!string.IsNullOrEmpty(text) && text != (value ? "true" : "false"))
		{
			Export(property);
		}
	}

	private void ExportIfNotDefault(SlideProperty property, string value)
	{
		string text = currentRow.Get(property);
		if (!string.IsNullOrEmpty(text) && text != value)
		{
			Export(property);
		}
	}

	private void ExportIfNotDefault(SlideProperty property, string value, SlideProperty dependentProperty)
	{
		string text = currentRow.Get(property);
		if (!string.IsNullOrEmpty(text) && text != value)
		{
			Export(property);
			Export(dependentProperty);
		}
	}

	private void ExportIfNotZero(SlideProperty property)
	{
		string text = currentRow.Get(property);
		if (!string.IsNullOrEmpty(text) && text != "0")
			Export(property);
	}

	private void ExportIfNotEmpty(SlideProperty property)
	{
		if (!string.IsNullOrEmpty(currentRow.Get(property)))
			Export(property);
	}

	private void ExportIfTrue(SlideProperty property)
	{
		if (currentRow.Get(property) == "true")
			Export(property);
	}

	private void ExportIfFalse(SlideProperty property)
	{
		if (currentRow.Get(property) == "false")
			Export(property);
	}

	private void ExportRowsToCsvFile(string fileLocation)
	{
		using (CsvWriter csv = new CsvWriter(fileLocation))
		{
			foreach (HotspotProperties row in rows)
			{
				currentRow = row;
				WriteRow(csv);
			}
			csv.Close();
		}
	}

	private void ExportRowsToXmlFile(string fileLocation)
	{
		CreateXmlMemoryStreamAndSettings();
		
		using (xmlWriter = XmlWriter.Create(xmlMemoryStream, xmlWriterSettings))
		{
			xmlWriter.WriteStartDocument();
			xmlWriter.WriteComment(string.Format("{0} for tour {1}.", CreateTimeStampGmt(), tour.Id));

			xmlWriter.WriteStartElement("tour");
			xmlWriter.WriteStartElement("maps");

			int mapCount = 0;

			foreach (HotspotProperties row in rows)
			{
				// Skip the header row.
				if (row == rows[0])
					continue;

				// Determine if this row is the start of a map.
				string mapId = row.Get(SlideProperty.MapId);
				
				if (mapId != string.Empty)
				{
					if (mapCount > 0)
					{
						// Emit the previous map's closing tags.
						xmlWriter.WriteEndElement(); // hotspots
						xmlWriter.WriteEndElement(); // map
					}

					// Emit this map's opening tags.
					xmlWriter.WriteStartElement("map");
					EmitElement("mapId", mapId);
					xmlWriter.WriteStartElement("hotspots");

					mapCount++;
					
					// Move to the next row which is for the first hotspot for this map.
					continue;
				}

				EmitHotspotXml(row);
			}

			if (mapCount > 0)
			{
				// Emit the last map's closing tags.
				xmlWriter.WriteEndElement(); // hotspots
				xmlWriter.WriteEndElement(); // map
			}
			
			// Emit the rest of the closing tags.
			xmlWriter.WriteEndElement(); // maps
			xmlWriter.WriteEndElement(); // tour

			xmlWriter.WriteEndDocument();
			xmlWriter.Flush();

			// Create the XML file.
			CopyXmlMemoryStreamToBytes();
			CreateFile(fileLocation, xmlBytes);
			xmlMemoryStream.Close();
		}
	}

	private void MarkColumnsToBeExported()
	{
		exports = new HotspotProperties();

		// Initially set all columns as not being exported.
		foreach (SlideProperty property in header)
		{
			exports.Set(property, false);
		}
		
		// Always export these columns.
		Export(SlideProperty.Instructions);
		Export(SlideProperty.TourId);
		Export(SlideProperty.MapId);
		Export(SlideProperty.HotspotId);
		Export(SlideProperty.Title);
		Export(SlideProperty.Text);

        foreach (HotspotProperties row in rows)
		{
			// Ignore the header row.
			if (row == rows[0])
				continue;

			currentRow = row;

			// ExportTourOverLimit the remaining columns only if they meet certain conditions.
			// The idea is that if every cell in a column contains either no data
			// or contains default data, that it does not get written out. Otherwise
			// the exported spreadsheet would be unweildy. For example, if the tour
			// does not use any mouseout handlers, we don't export the mouseout action
			// and mouseout action target columns.
			MarkConditionalColumnsToBeExported();
		}
	}

	private bool OkToExport(SlideProperty property)
	{
		return exports.OkToExport(property);
	}

	private bool OkToExportXmlElement(HotspotProperties row, SlideProperty property)
	{
		if (property == SlideProperty.Instructions || property == SlideProperty.TourId || property == SlideProperty.MapId)
			return false;

		if (!OkToExport(property))
			return false;

		if (row.Get(property) == string.Empty)
			return false;

		return true;
	}

	private void SetHeaderRowColumn(SlideProperty property)
	{
		// The header ArrayList keeps track of which columns are in the row header.
		header.Add(property);
		
		currentRow.Set(property);
	}

	private string TrueOrEmpty(bool condition)
	{
		return condition ? "true" : string.Empty;
	}

	private void WriteColumn(CsvWriter csv, SlideProperty property)
	{
		if (OkToExport(property))
		{
			string value = currentRow.Get(property);
			//Debug.Write(value + ",");
			csv.Write(value);
		}
	}

	private void WriteRow(CsvWriter csv)
	{
		//Debug.Write("[");

		foreach (SlideProperty property in header)
			WriteColumn(csv, property);

		//Debug.WriteLine("]");
		//Debug.Flush();

		csv.EndRecord();
	}
}
