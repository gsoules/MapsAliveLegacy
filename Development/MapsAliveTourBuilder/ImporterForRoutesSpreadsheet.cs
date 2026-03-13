// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using DataStreams.Csv;
using DataStreams.Xls;
using DataStreams.Xlsx;

public class ImporterForRoutesSpreadsheet : Importer
{
	private class RouteTable
	{
		ArrayList routes;

		public RouteTable()
		{
			routes = new ArrayList();
		}

		public void AddRoute(Route route)
		{
			routes.Add(route);
		}

		public int Count
		{
			get { return routes.Count; }
		}

		public Route Lookup(string routeId)
		{
			foreach (Route route in routes)
			{
				if (route.RouteId == routeId)
					return route;
			}
			return null;
		}

		public ArrayList Routes
		{
			get { return routes; }
		}
	}

	private class Route
	{
		private bool beingResolved;
		private ImportReport report;
		private ArrayList sections;
		private TourPage tourPage;

		public Route(string routeId, bool import, string rawRoute, int lineNumber, TourPage tourPage, ImportReport report)
		{
			RouteId = routeId.Trim();
			Import = import;
			LineNumber = lineNumber;
			Resolved = false;

			this.tourPage = tourPage;
			this.report = report;

			// Create a Section from each semicolon-delimited part of the route.
			sections = new ArrayList();
			string[] rawSections = rawRoute.Split(';');
			foreach (string rawSection in rawSections)
			{
				sections.Add(new RouteSection(rawSection, this));
			}
		}

	
		public bool Import { get; set; }
		public string RouteId { get; set; }
		public int LineNumber { get; set; }
		public bool Resolved { get; set; }
		public bool Rejected { get; set; }

		public string Coords()
		{
			string list = "";

			foreach (RouteSection routeSection in sections)
			{
				if (list.Length > 0)
					list += ";";
				list += routeSection.Coords();
			}

			return list;
		}

		public ImportReport Report
		{
			get { return report; }
		}

		public void ResolveRoute(RouteTable routeTable)
		{
			if (beingResolved)
			{
				report.Warning(string.Format("{0} contains a circular reference", RouteId), LineNumber);
				Rejected = true;
				return;
			}

			beingResolved = true;
			int unresolvedSectionsCount = 0;
			
			foreach (RouteSection routeSection in sections)
			{
				if (routeSection.Resolved)
					continue;

				routeSection.ResolveSection(routeTable);

				if (Rejected)
					break;

				if (!routeSection.Resolved)
					unresolvedSectionsCount++;
			}
				
			// This route is resolved when all of its section are resolved.
			Resolved = unresolvedSectionsCount == 0;
			beingResolved = false;
		}

		public TourPage TourPage
		{
			get { return tourPage; }
		}
	}

	private class RouteSection
	{
		private Route route;
		private ArrayList segments;
		
		public RouteSection(string rawSection, Route route)
		{
			string section = rawSection.Trim();
			this.route = route;
			
			// Create a Segment from each comma-delimted part of the section.
			segments = new ArrayList();
			string[] rawSegments = section.Split(',');

			if (rawSegments.Length >= 2 && section.StartsWith("(") && section.EndsWith(")"))
			{
				route.Report.Warning(string.Format("{0} contains a section enclosed in parentheses (parens can only appear around individual segment Ids", route.RouteId), route.LineNumber);
				route.Rejected = true;
			}
			
			foreach (string rawSegment in rawSegments)
			{
				segments.Add(new RouteSegment(rawSegment, route));
			}
		}

		public bool Resolved { get; set; }

		public string Coords()
		{
			string list = "";

			foreach (RouteSegment routeSegment in segments)
			{
				if (list.Length > 0)
					list += ",";
				list += routeSegment.Coords();
			}

			return list;
		}

		public void ResolveSection(RouteTable routeTable)
		{
			int unresolvedSegmentCount = 0;
			foreach (RouteSegment routeSegment in segments)
			{
				if (routeSegment.Resolved)
					continue;

				routeSegment.ResolveSegment(routeTable);

				if (route.Rejected)
					break;

				if (!routeSegment.Resolved)
					unresolvedSegmentCount++;
			}
			
			// This section is resolved when all of its segments are resolved.
			Resolved = unresolvedSegmentCount == 0;
		}
	}

	private class RouteSegment
	{
		private string coords;
		private bool hasBeenLookedUpAsWaypoint;
		private ArrayList points;
		private bool resolved;
		private bool reverse;
		private Route route;
		private string rawValue;
		
		public RouteSegment(string rawSegment, Route route)
		{
			rawValue = rawSegment.Trim();
			this.route = route;

			if (rawValue.StartsWith("(") && rawValue.EndsWith(")"))
			{
				// The the order of points in a segment enclosed in parens must be reversed.
				reverse = true;

				// Discard the parens.
				rawValue = rawValue.Substring(1, rawValue.Length - 2);
			}

			if (rawValue.ToLower() == route.RouteId.ToLower())
			{
				//Circular reference detected.
				route.Report.Warning(string.Format("{0} references itself", route.RouteId), route.LineNumber);
				route.Rejected = true;
				resolved = true;
			}
			else
			{
				points = new ArrayList();
			}
		}

		public string Coords()
		{
			return coords;
		}
		
		public bool Resolved
		{
			get { return hasBeenLookedUpAsWaypoint && resolved; }
		}

		public void ResolveSegment(RouteTable routeTable)
		{
			if (!hasBeenLookedUpAsWaypoint)
			{
				// If a segment is a waypoint, then it is automatically resolved.
				TourView tourView = route.TourPage.GetTourViewBySlideId(rawValue);
				if (tourView != null)
				{
					resolved = true;
					int x = tourView.MarkerX;
					int y = tourView.MarkerY;
					if (x < 0 || y < 0)
					{
						route.Report.Warning(string.Format("{0} uses off-map waypoint {1}", route.RouteId, rawValue), route.LineNumber);
						route.Rejected = true;
					}
					else
					{
						SetCoords(tourView.Id.ToString());
						route.Report.Trace(string.Format("Resolved {0} as a waypoint at {1}", rawValue, coords), route.LineNumber);
					}
				}
				hasBeenLookedUpAsWaypoint = true;
			}

			if (!resolved)
			{
			    Route referencedRoute = routeTable.Lookup(rawValue);
			    if (referencedRoute == null)
			    {
					route.Report.Warning(string.Format("{0} references non-existent route {1}", route.RouteId, rawValue), route.LineNumber);
					route.Rejected = true;
					resolved = true;
			    }
			    else
			    {
					if (referencedRoute.Rejected)
					{
						route.Report.Warning(string.Format("{0} references unresolvable route {1}", route.RouteId, referencedRoute.RouteId), route.LineNumber);
						route.Rejected = true;
						resolved = true;
					}
					else
					{
						if (!referencedRoute.Resolved)
						{
							// Recursively resolve the referenced route.
							//	route.Report.Trace(route.RouteId + " ...");
							referencedRoute.ResolveRoute(routeTable);
						}

						if (referencedRoute.Resolved)
						{
							resolved = true;
							SetCoords(referencedRoute.Coords());
						}
					}
			    }
			}
		}

		private static string ReverseCoords(string coords)
		{
			string reversedCoords = string.Empty;
			
			// Reverse the semicolon-separated sections within the coords.
			string reversedSections = ReverseList(coords, ';');

			// Loop over each section and reverse its comma-separated points.
			string[] sectionList = reversedSections.Split(';');
			foreach (string section in sectionList)
			{
				string reversedSection = ReverseList(section, ',');
				if (reversedCoords.Length > 0)
					reversedCoords += ";";
				reversedCoords += reversedSection;
			}

			if (coords.Contains(";"))
			{
				System.Diagnostics.Debug.WriteLine(coords + " == " + reversedCoords);
			}

			return reversedCoords;
		}

		private static string ReverseList(string list, char separator)
		{
			string[] listItems = list.Split(separator);
			string reversedList = "";
			
			int index = listItems.Length - 1;
			while (index >= 0)
			{
				if (reversedList.Length > 0)
					reversedList += separator;

				reversedList += listItems[index];
				index--;
			}
			return reversedList;
		}

		private void SetCoords(string coords)
		{
			// This method does not actually set coordinates, but rather the Id of a TourView.
			// The x,y coordinates can be derived at a later time when needed. We used to emit
			// coords and thus the name SetCoords, but now we wait until building a tour to
			// get x,y values so that they reflect the hotspot's current location, even if it
			// moves or the map size changes after this import occurs.
			this.coords = reverse ? ReverseCoords(coords) : coords;
		}
	}

	private int lineNumber;
	private bool importedXml;
	private int recordCount;
	private RouteTable routeTable;
	private ReaderForSpreadsheet readerForSpreadsheet;

	public ImporterForRoutesSpreadsheet(TourPage tourPage, Stream stream, string reportTitle)
		: base(tourPage.Tour, tourPage, stream, reportTitle)
	{
		lineNumber = 1;
	}

	private void CreateRouteXml()
	{
		XmlWriterSettings settings = new XmlWriterSettings();
		settings.OmitXmlDeclaration = true;
		
		StringBuilder sb = new StringBuilder();

		using (XmlWriter xmlWriter = XmlWriter.Create(sb, settings))
		{
			xmlWriter.WriteStartDocument();
			CreateXml(xmlWriter);
			xmlWriter.WriteEndDocument();
			xmlWriter.Flush();
		}

		SaveRoutesXml(sb.ToString());
	}

	private void CreateXml(XmlWriter xmlWriter)
	{
		xmlWriter.WriteStartElement("routes");

		foreach (Route route in routeTable.Routes)
		{
			if (route.Rejected)
			{
				report.EmitRow(ImportReport.Topic.RoutesRejected, route.RouteId);
				continue;
			}
			
			if (!route.Resolved)
			{
				report.EmitRow(ImportReport.Topic.RoutesUnresolved, route.RouteId);
				continue;
			}

			if (!route.Import)
			{
				continue;
			}

			report.EmitRow(ImportReport.Topic.RoutesImported, route.RouteId, route.Coords());

			xmlWriter.WriteStartElement("route");
			xmlWriter.WriteAttributeString("id", route.RouteId);
			xmlWriter.WriteString(route.Coords());
			
			xmlWriter.WriteEndElement(); // route
		}

		xmlWriter.WriteEndElement(); // routes
	}

	private bool HasColumn(string[] names, string columnName)
	{
		foreach (string name in names)
		{
			if (name.Trim().ToLower() == columnName.Trim().ToLower())
				return true;
		}

		report.Warning(string.Format("\"{0}\" column is missing", columnName));
		return false;
	}

	public void ImportRoutes(string fileExt)
	{
		try
		{
			OpenSpreadsheet(fileExt);

			if (importedXml)
				return;
			
			if (readerForSpreadsheet.Opened)
			{
				if (IsRoutesSpreadsheet())
				{
					report.Trace("IMPORTING ROUTES FROM SPREADSHEET");
					ReadRoutes();
					ResolveRoutes();
				}
				else
				{
					message = "One or more required columns were not found";
					importFailed = true;
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
	}
	
	public static bool IsValidRouteId(string s)
	{
		// A valid route Id is alphanumeric and can also contain '_'. It cannot start with a digit.
		// This code is faster than a regular expression for short strings.
		int position = 0;
		foreach (char c in s)
		{
			if (position == 0 && Char.IsDigit(c))
				return false;

			if (!Char.IsLetterOrDigit(c) && c != '_')
				return false;

			position++;
		}
		return true;
	}

	private bool IsRoutesSpreadsheet()
	{
		// Get the names of the columns in the first row of the spreadsheet or CSV data.
		string[] names = new string[0];

		switch (readerForSpreadsheet.Type)
		{
			case SpreadsheetType.csv:
				names = ((DataStreams.Csv.CsvReader)readerForSpreadsheet.Reader).Headers;
				break;

			case SpreadsheetType.xls:
			case SpreadsheetType.xlsx:
				names = ((DataStreams.Common.SpreadsheetReader)readerForSpreadsheet.Reader).Headers;
				break;
		}

		// Check each column separately so that an error will get reported for all that are missing.
		bool hasId = HasColumn(names, "Id");
		bool hasRoute = HasColumn(names, "Route");
		bool hasImport = HasColumn(names, "Import");

		return hasId && hasRoute && hasImport;
	}

	private void OpenSpreadsheet(string fileExt)
	{
		MemoryStream memoryStream = new MemoryStream();
		Utility.CopyStream(stream, memoryStream);

		// Copy the stream to a byte array, trimming off any non-data at the end of the stream's buffer.
		// We make the copy so that we can read the data multiple times if necessary (the stream can only be read once).
		byte[] data = new byte[(int)memoryStream.Length];
		Array.Copy(memoryStream.GetBuffer(), data, data.Length);

		if (fileExt == ".xml")
			ReadRoutesFromXmlFile(data);
		else
			readerForSpreadsheet = new ReaderForSpreadsheet(data);
	}

	private bool ReadNextRoute()
	{
		bool eof;

		try
		{
			eof = !readerForSpreadsheet.Reader.ReadRecord();
			lineNumber++;
		}
		catch (Exception)
		{
			eof = false;
		}

		return !eof;
	}

	private void ReadRoutes()
	{
		routeTable = new RouteTable();

		while (ReadNextRoute())
		{
			string routeId = ReadSpreadsheetRecordColumn("Id");
			string definition = ReadSpreadsheetRecordColumn("Route");
			bool import = ReadSpreadsheetRecordColumn("Import").ToLower() == "true";

			if (routeId.Length > 0)
			{
				if (!IsValidRouteId(routeId))
				{
					report.Warning(string.Format("'{0}' is not a valid route Id (only letters, digits, and underscore allowed; first letter cannot be a digit)", routeId), lineNumber);
				}
				else if (definition.Length == 0)
				{
					report.Warning(string.Format("Route {0} has no definition.", routeId), lineNumber);
				}
				else
				{
					Route existingRoute = routeTable.Lookup(routeId);
					if (existingRoute == null)
					{
						Route route = new Route(routeId, import, definition, lineNumber, tourPage, report);
						routeTable.AddRoute(route);
					}
					else
					{
						report.Warning(string.Format("Skipping route {0} because it was already defined on line {1}.", routeId, existingRoute.LineNumber), lineNumber);
					}
				}
			}
		}
	}

	private void ReadRoutesFromXmlFile(byte[] data)
	{
		importedXml = true;

		string xmlString = System.Text.ASCIIEncoding.ASCII.GetString(data);
		Routes routes = new Routes(xmlString);
		
		if (routes.IsValid)
		{
			XmlNodeList routeNodes = routes.RouteNodes;

			if (routeNodes.Count == 0)
			{
				message = "File does not contain any routes";
				importFailed = true;
			}
			else
			{
				report.Trace("IMPORTING ROUTES FROM XML FILE");
				foreach (XmlNode routeNode in routeNodes)
				{
					report.EmitRow(ImportReport.Topic.RoutesImported, routeNode.Attributes["id"].Value);
				}
				SaveRoutesXml(routes.OuterXml);
			}
		}
		else
		{
			message = "File does not contain valid routes XML";
			importFailed = true;
		}
	}

	private string ReadSpreadsheetRecordColumn(string columnName)
	{
		string value = null;

		switch (readerForSpreadsheet.Type)
		{
			case SpreadsheetType.csv:
				value = ((CsvReader)readerForSpreadsheet.Reader)[columnName];
				break;

			case SpreadsheetType.xls:
				value = ((XlsReader)readerForSpreadsheet.Reader)[columnName];
				break;

			case SpreadsheetType.xlsx:
				value = ((XlsxReader)readerForSpreadsheet.Reader)[columnName];
				break;
		}

		if (value != null)
			value = value.Trim();

		return value;
	}

	private void ResolveRoutes()
	{
		recordCount = routeTable.Count;

		int recordNumber = 0;

		foreach (Route route in routeTable.Routes)
		{
			recordNumber++;
			
			string status = string.Format("record {0}", recordNumber);
			ProgressMonitor.Update(status, recordCount, recordNumber);

			route.ResolveRoute(routeTable);

			if (route.Resolved && !route.Rejected)
				report.Trace(string.Format("Resolved route {0} : {1}", route.RouteId, route.Coords()));
		}

		CreateRouteXml();
	}

	private void SaveRoutesXml(string xmlString)
	{
		tourPage.RoutesXml = xmlString;
		tourPage.UpdateDatabase();
	}
}
