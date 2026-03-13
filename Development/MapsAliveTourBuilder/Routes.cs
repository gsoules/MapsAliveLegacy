// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Xml;

public class Routes
{
	private bool isValid;
	private bool pointsAreCoordinates;
	private XmlDocument xmlDocument;

	public Routes(string routesXml)
	{
		try
		{
			xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(routesXml);

			XmlNode rootNode = xmlDocument.SelectSingleNode("/routes");
			XmlAttribute attribute = rootNode.Attributes["pointsAreCoordinates"];
			pointsAreCoordinates = attribute != null && attribute.Value == "true";

			isValid = true;
		}
		catch
		{
			isValid = false;
		}
	}
		
	public delegate string FixupValue(int tourViewId);

	public bool IsValid
	{
		get { return isValid; }
	}

	public static XmlDocument FixupRoutesXml(string RoutesXml, FixupValue fixupValue)
	{
		// Create an XML document containing the routes that were imported for this map.
		// Fixup each TourView Id in the imported route to be the corresponding Id for this map.
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(RoutesXml);

		XmlNodeList routeNodes = xmlDocument.SelectNodes("/routes/route");

		// Process each route one at a time.
		foreach (XmlNode routeNode in routeNodes)
		{
			// Convert the TourView Ids in this route to TourView Ids for the current map.
			string[] sections = routeNode.InnerText.Split(';');
			for (int section = 0; section < sections.Length; section++)
			{
				string[] tourViewIds = sections[section].Split(',');
				for (int index = 0; index < tourViewIds.Length; index++)
				{
					int tourViewId;
					int.TryParse(tourViewIds[index], out tourViewId);
					
					// Call the passed-in function to get the fixup Id.
					string fixedUpId = fixupValue(tourViewId);
					if (fixedUpId == null)
					{
						// This can happen in this scenario. Routes are imported into a map and stored there.
						// The user later deletes a hotspot that is in a route. That route now contains a
						// non-existant hotspot. If the tour is then duplicated or archived/restored, the
						// hotspot's Id can't be fixed up because that hotspot doesn't exist. So we ignore it.
						// The Id can also come back null if the hotspot is itself a route.
						tourViewIds[index] = ".";
					}
					else
					{
						// Replace the TourView Id with the hotspot's coordinate values.
						tourViewIds[index] = fixedUpId;
					}
				}
				// Combine the tourViewIds back into a single comma-separated string of TourView Ids.
				sections[section] = String.Join(",", tourViewIds);
			}

			// Combine the sections back into a single semicolon-separated string of sections.
			string s = String.Join(";", sections);

			// Eliminate slots for hotspots that no longer exists.
			s = s.Replace(",.", "");

			// Update the DOM with the processed route.
			routeNode.InnerText = s;
		}

		return xmlDocument;
	}

	public XmlNodeList RouteNodes
	{
		get { return xmlDocument.SelectNodes("/routes/route"); }
	}

	public string OuterXml
	{
		get { return xmlDocument.OuterXml; }
	}
}
