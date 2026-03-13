// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Xml;

public class ReaderForImportedXml
{
	private int currentSlideNumber;
	private const string ns = "";
	private ImportReport report;
	private int slideCount;
	private int slidesRead;
	private Tour tour;
	private XmlDocument tourXmlDoc;
	private XmlNode xmlNodeForCurrentSlide;
	private XmlNodeList xmlSlideNodeList;
	private XmlElement xmlRoot;

	public ReaderForImportedXml(Tour tour, ImportReport report)
	{
		this.tour = tour;
		this.report = report;
	}

	private XmlDocument CreateXmlFromStream(Stream stream, out string errorMessage)
	{
		// Read the data into a memory stream and save the stream's buffer. The code below will
		// attempt to open the stream as XML, but if that fails, it will pass the buffer to the
		// spreadsheet importer to see if it can read the data. The idea is to get the data into
		// memory just once and then be able to create streams from it as necessary. Note that we
		// do it this way because sreams get closed after an exception such as the normals ones
		// that occur as we try opening the data first as XML, then Excel, etc.
		MemoryStream memoryStream = new MemoryStream();
		Utility.CopyStream(stream, memoryStream);
		byte[] data = memoryStream.GetBuffer();

		errorMessage = string.Empty;
		XmlDocument xmlDoc = null;
		
		try
		{
			// See if the data is already XML.
			xmlDoc = new XmlDocument();
			xmlDoc.Load(memoryStream);
		}
		catch
		{
			// See if the data is a spreadsheet. In this code, "spreadsheet" means either Excel or CSV.
			ImporterForSpreadsheet importer = new ImporterForSpreadsheet(tour, report);

			if (importer.CreateSpreadsheetReader(data))
			{
				// The data is a valid spreadsheet. Compile it into table XML.
				xmlDoc = importer.CompileSpreadsheetIntoTableXml();

				if (xmlDoc == null)
					errorMessage = "Required columns are missing (see trace)"; 
			}
			else
			{
				errorMessage = "Unable to read data as XML, Excel, or CSV";
			}
		}

		return xmlDoc;
	}

	public int CurrentSlideNumber
	{
		get { return currentSlideNumber; }
	}

	private bool IsCurrentVersion(XmlDocument xmlDoc, string root)
	{
		string xPath = string.Empty;

		if (root == "table")
		{
			xPath = "//table/data/group/hotspots";
		}
		else if (root == "tour")
		{
			xPath = "//tour/maps";
		}
		else
		{
			Debug.Fail("Unexpected root " + root);
			return false;
		}
		
		// The xPath is new for version 3. If not found, the XML is not the current version.
		XmlNode node = xmlDoc.SelectSingleNode(xPath);
		return node != null;
	}

	public bool LoadTourXml(Stream stream, out string errorMessage)
	{
		// Attempt to read the stream.
		XmlDocument xmlDoc = CreateXmlFromStream(stream, out errorMessage);
		if (xmlDoc == null)
			return false;

		// We have XML. Determine what kind it is.
		xmlRoot = xmlDoc.DocumentElement;
		if (xmlRoot.Name == "tour")
		{
			// The XML is in tour format.
			if (IsCurrentVersion(xmlDoc, "tour"))
			{
				// The XML can be used as-is.
				tourXmlDoc = xmlDoc;
			}
			else
			{
				// The XML needs to converted to the current format.
				tourXmlDoc = ConvertToCurrentTourXml(xmlDoc);
			}
		}
		else if (xmlRoot.Name == "table")
		{
			// The XML is in table format.
			if (IsCurrentVersion(xmlDoc, "table"))
			{
				// The XML can be used as-is.
				tourXmlDoc = xmlDoc;
			}
			else
			{
				// The XML needs to converted to the current format.
				tourXmlDoc = ConvertToCurrentTableXml(xmlDoc);
			}
			
			// Compile the table XML into tour format. Note that a user can import
			// XML in table or tour format. This XML might have been imported directly,
			// or it might have been compiled into XML from a spreadsheet.
			ImporterForTableXml importer = new ImporterForTableXml(tour, report);
			tourXmlDoc = importer.CompileTableXmlIntoTourXml(tourXmlDoc, out errorMessage);
		}
		else
		{
			errorMessage = "Imported XML must be in Tour or Import format.";
			return false;
		}

		xmlRoot = tourXmlDoc.DocumentElement;

		// Set all the mapId values to lower case so that we can do case-insensitive compares.
		XmlNodeList mapNodeList = xmlRoot.SelectNodes("/tour/maps/map/mapId");
		foreach (XmlNode mapNode in mapNodeList)
			mapNode.InnerText = mapNode.InnerText.ToLower();
		
		// Determine how many hotspots there are in the entire stream. We need
		// the count so that the progress bar can report percentage-complete.
		xmlSlideNodeList = xmlRoot.SelectNodes("//hotspot");
		if (xmlSlideNodeList != null)
			slideCount = xmlSlideNodeList.Count;

		return true;
	}

	private XmlDocument ConvertToCurrentTableXml(XmlDocument xmlDoc)
	{
		// This brute force conversion is not very elegant, but it really does not need
		// to do very much and so an XSL transform seems like it would be overkill.
		// This is much simpler and almost certainly faster.
		string xmlText = xmlDoc.OuterXml;
		xmlText = xmlText.Replace("<column name=\"pageid\"", "<column name=\"mapid\"");
		xmlText = xmlText.Replace("<column name=\"slideid\"", "<column name=\"hotspotid\"");
		xmlText = xmlText.Replace("<column name=\"firstslide\"", "<column name=\"firsthotspot\"");
		xmlText = xmlText.Replace("<column name=\"newslideid\"", "<column name=\"newhotspotid\"");
		xmlText = xmlText.Replace("<slide", "<hotspot");
		xmlText = xmlText.Replace("</slide", "</hotspot");
		xmlDoc.LoadXml(xmlText);
		return xmlDoc;
	}

	private XmlDocument ConvertToCurrentTourXml(XmlDocument xmlDoc)
	{
		// See comment for ConvertToCurrentTableXml.
		string xmlText = xmlDoc.OuterXml;
		xmlText = xmlText.Replace("<page", "<map");
		xmlText = xmlText.Replace("</page", "</map");
		xmlText = xmlText.Replace("<slide", "<hotspot");
		xmlText = xmlText.Replace("</slide", "</hotspot");
		xmlText = xmlText.Replace("<firstSlide", "<firstHotspot");
		xmlText = xmlText.Replace("</firstSlide", "</firstHotspot");
		xmlText = xmlText.Replace("<newSlideId", "<newHotspotId");
		xmlText = xmlText.Replace("</newSlideId", "</newHotspotId");
		xmlDoc.LoadXml(xmlText);
		return xmlDoc;
	}

	public int PositionToTourPage(string pageId)
	{
		string xPath = string.Format("//map[mapId='{0}']/hotspots/hotspot", pageId.ToLower());
		xmlSlideNodeList = xmlRoot.SelectNodes(xPath);
		int slideCount = xmlSlideNodeList.Count;
		report.Trace(string.Format("XPath \"{0}\" count={1}", xPath, slideCount));
		currentSlideNumber = 0;
		return slideCount;
	}

	public bool ReadSlide()
	{
		xmlNodeForCurrentSlide = xmlSlideNodeList.Item(currentSlideNumber);
		currentSlideNumber++;
		slidesRead++;
		return xmlNodeForCurrentSlide != null;
	}

	public string ReadPropertyValue(XmlNode xmlNode, string propertyName)
	{
		if (xmlNode == null)
			return null;

		XmlNode nodeForProperty = xmlNode.SelectSingleNode(propertyName);
		if (nodeForProperty != null)
			return nodeForProperty.InnerText;
		else
			return null;

	}

	public string ReadSlidePropertyValue(string propertyName)
	{
		return ReadPropertyValue(xmlNodeForCurrentSlide, propertyName);
	}

	public int SlideCount
	{
		get { return slideCount; }
	}

	public int SlidesRead
	{
		get { return slidesRead; }
	}

	public XmlDocument XmlDocument
	{
		get { return tourXmlDoc; }
	}
}
