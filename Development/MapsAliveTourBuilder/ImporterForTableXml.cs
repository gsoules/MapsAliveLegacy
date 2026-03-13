// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using DataStreams.Common;
using DataStreams.Csv;
using DataStreams.Xls;
using DataStreams.Xlsx;

public class ImporterForTableXml
{
	private XmlElement activeTemplateElement;
	private XmlNodeList dataGroupUseNodes;
	private Hashtable mergedSlideData;
	private ImportReport report;
	private string[] slidePropertyNames;
	private Stack substitutions;
	private string[] tableColumnNames;
	private XmlDocument tableXmlDoc;
	private XmlDocument tourXmlDoc;
	private Tour tour;

	public ImporterForTableXml(Tour tour, ImportReport report)
	{
		this.tour = tour;
		this.report = report;
	}

	private bool CompatiblePageIds(string pageId, string currentPageId, string tourSelectedPageId)
	{
		pageId = pageId.ToLower();
		currentPageId = currentPageId.ToLower();
		bool useForAnyPage = pageId == "**";
		bool useForCurrentPage = pageId == "*" && currentPageId == tourSelectedPageId.ToLower();
		bool useForThisPage = pageId == currentPageId;
		return useForAnyPage || useForCurrentPage || useForThisPage;
	}

	private bool CompatibleTourIds(string tourId, string currentTourId)
	{
		tourId = tourId.ToLower();
		currentTourId = currentTourId.ToLower();
		bool useForAnyTour = tourId == "*";
		bool useForThisTour = tourId == currentTourId;
		return useForAnyTour || useForThisTour;
	}

	public XmlDocument CompileTableXmlIntoTourXml(XmlDocument tableXmlDoc, out string errorMessage)
	{
		this.tableXmlDoc = tableXmlDoc;
		errorMessage = string.Empty;

		// Determine what hotspot properties are being imported.
		GetTableColumnNames();

		// Create a new XML document and initialize it with the basic structure for Tour XML.
		tourXmlDoc = new XmlDocument();
		tourXmlDoc.LoadXml("<tour><maps></maps></tour>");
		XmlElement tourXmlPagesElement = (XmlElement)tourXmlDoc.DocumentElement.SelectSingleNode("//maps");

		report.Trace("COMPILING TABLE XML ...");
		
		// Process each page in the tour.
		foreach (TourPage tourPage in tour.TourPages)
		{
			report.Trace(string.Format("Compiling <b>{0}</b> ({1}) ...", tourPage.Name, tourPage.PageId));

			// Create a <map> element and attach it to its parent <maps> element.
			XmlElement tourXmlPageElement = XmlUtility.CreateElement(tourXmlDoc, "map");
			tourXmlPagesElement.AppendChild(tourXmlPageElement);
			
			// Create a <map><mapId> element and attach it to its parent <map> element.
			XmlElement tourXmlPageIdElement = XmlUtility.CreateElement(tourXmlDoc, "mapId");
			tourXmlPageIdElement.InnerText = tourPage.PageId;
			tourXmlPageElement.AppendChild(tourXmlPageIdElement);

			// Create a <map><hotspots> element and attach it to its parent <map> element.
			XmlElement tourXmlSlidesElement = XmlUtility.CreateElement(tourXmlDoc, "hotspots");
			tourXmlPageElement.AppendChild(tourXmlSlidesElement);

			// Process all the data groups that can be used on this page.
			ProcessTableXmlDataGroups(tourPage, tour.SelectedTourPage.PageId, tourXmlSlidesElement);
		}

		// Write the file to disk in case we want to see what it looks like.
		if (App.DeveloperMode)
		{
			string fileLocation = FileManager.PreviewFolderLocationAbsolute(tour.Id) + "\\_tour.xml";
			tourXmlDoc.Save(fileLocation);
		}

		return tourXmlDoc;
	}

	private void CreateTourXmlSlideElement(XmlElement parentElement, XmlElement tableXmlSlideElement)
	{
		// Create a <hotspot> element and attach it to its parent <hotspots> element.
		XmlElement tourXmlSlideElement = XmlUtility.CreateElement(tourXmlDoc, "hotspot");
		parentElement.AppendChild(tourXmlSlideElement);

		string slideId = GetMergedSlideDataForColumn("HotspotId");
		report.Trace(string.Format("{0} : compiled", slideId), XmlUtility.GetLineAttribtueValue(tableXmlSlideElement));

		// Loop over the columns in the active hotspot data and pick out the ones that correspond
		// to hotspot child elements in the tour XML, e.g. elements like "title" and "clickAction"
		// as opposed to user-defined data columns.
		foreach (string columnName in tableColumnNames)
		{
			int index = LookupNameInSlidePropertyNameTable(columnName);
			if (index == -1)
			{
				// This is a user-defined column so ignore it.
				continue;
			}

			// Don't add tourId or mapId values to a hotspot. We already know their values from the XML hierarchy.
			string elementName = slidePropertyNames[index];
			if (elementName == "tourId" || elementName == "mapId")
				continue;

			// Get the child element's value.
			string value = GetMergedSlideDataForColumn(columnName);
			if (value == null)
				continue;

			// Ignore any values that are protected. This means the user neither wants to provide a value
			// or inherit a template value. They just want to leave the hotspot's current value alone.
			if (value.Trim().ToLower() == "[protect]")
				continue;
			
			// Expand any inline substitutions. If there is no value, don't add it to the hotspot.
			string expandedValue = string.Empty;

			// Expand any inline column references that are in square brackets and get back the fully
			// expanded value. If the result is empty, then the user does not want to provide a value.
			// In that case return without creating a hotspot element. Since the element won't appear in 
			// the tour XML, it's corresponding property in the actual hotspot won't get changed. If the
			// users wants to explicitly set the property to empty, they can specify "[blank]". In that
			// case a hotspot element will get created and its value will be the empty string.
			if (value.Trim().ToLower() != "[blank]")
			{
				expandedValue = ExpandSlideValue(value);
				if (expandedValue.Length == 0)
					continue;
			}

			// Create a child element for the hotspot in the tour XML.
			XmlElement tourXmlSlideValueElement = XmlUtility.CreateElement(tourXmlDoc, elementName);
			tourXmlSlideValueElement.InnerText = expandedValue;
			tourXmlSlideElement.AppendChild(tourXmlSlideValueElement);
		}
	}

	private bool DataGroupUsesTemplates
	{
		get { return dataGroupUseNodes.Count > 0; }
	}

	private string ExpandSlideValue(string raw)
	{
		substitutions = new Stack();
		return ExpandSubstitutions(raw);
	}

	private string ExpandSubstitutions(string raw)
	{
		StringBuilder sb = new StringBuilder();

		int i = 0;
		while (i < raw.Length)
		{
			char c = raw[i];
			if (c == '[')
			{
				bool parsingSubstitution = true;

				int start = i;
				int length = 0;

				while (i < raw.Length - 1 && parsingSubstitution)
				{
					i++;
					length++;
					c = raw[i];
					if (c == ']')
					{
						length++;
						i++;
						break;
					}
					if (c == '[')
					{
						// We have encountered a nested '[' so bail out.
						parsingSubstitution = false;
					}
				}

				if (i >= raw.Length && c != ']')
				{
					// There's no closing bracket so just append the rest of the string.
					sb.Append(raw.Substring(i));
				}
				else
				{
					string name = string.Empty;
					string value = null;

					if (parsingSubstitution)
					{
						// We found a substitution variable. Now get its value;
						name = raw.Substring(start, length);
						name = name.ToLower();
						value = GetSubstitutionValue(name);
						if (value == null)
						{
							// No value. Treat the substitution as text.
							value = name;
						}
					}
					else
					{
						value = raw.Substring(start, length);
					}

					if (value != null)
					{
						sb.Append(value);
					}
					else
					{
						// No value. Treat the substitution as text.
						sb.Append(raw);
					}
				}
			}
			else
			{
				sb.Append(c);
				i++;
			}
		}

		return sb.ToString();
	}

	private void GetActiveTemplateForTableXmlDataGroup(XmlNode tableXmlDataGroupNode, TourPage tourPage, string currentPageId)
	{
		activeTemplateElement = null;
		int firstLine = -1;

		// Identify the active template for this group by looping over all of its Use templates.
		// If more than one template is valid for this page, the last one in the group is chosen.

		dataGroupUseNodes = tableXmlDataGroupNode.SelectNodes("templates/use");
		foreach (XmlElement useElement in dataGroupUseNodes)
		{
			int line = XmlUtility.GetLineAttribtueValue(useElement);
			if (firstLine == -1)
				firstLine = line;

			// Get the template's name so we can get its definition. If it has no name, it's used as-is.
			string templateName = XmlUtility.GetNameAttribtueValue(useElement);

			if (templateName != string.Empty)
			{
				// Get the definition for this template. If none exists, report an error.
				XmlNode tableXmlDefinitionNode = GetTemplateDefinitionFromTableXml(templateName);
				if (tableXmlDefinitionNode == null)
				{
					report.Warning(templateName + " has not been defined", line);
					continue;
				}

				// Merge the definition's values with values that the template overrides.
				MergeTableXmlTemplateUseWithDefinition(useElement, tableXmlDefinitionNode);
			}

			// See if the template has a TourId and/or PageId values.
			string templateTourId = string.Empty;
			string templatePageId = string.Empty;

			// Get all the columns from the use element.
			XmlNodeList columnNodes = useElement.SelectNodes("column");
			foreach (XmlElement columnElement in columnNodes)
			{
				string name = XmlUtility.GetNameAttribtueValue(columnElement).ToLower();
				string value = columnElement.InnerText;
				if (name == "tourid")
					templateTourId = value;
				else if (name == "mapid")
					templatePageId = value;
			}

			// Evaluate the template's tour Id value if it has one.
			if (templateTourId != string.Empty && !CompatibleTourIds(templateTourId, tour.Id.ToString()))
			{
				report.Trace(string.Format("Not using template '{0}' because its TourId {1} is not for this tour {2}", templateName, templateTourId, tour.Id), line);
				continue;
			}

			// Evaluate the template's page Id value if it has one.
			if (templatePageId != string.Empty && !CompatiblePageIds(templatePageId, tourPage.PageId, tour.SelectedTourPage.PageId))
			{
				string explanation;
				if (templatePageId == "*")
					explanation = "'*' means import into the current map";
				else
					explanation = string.Format("'{0}' is not for '{1}'", templatePageId, tourPage.Name);

				string name = templateName.Length == 0 ? "unnamed template" : string.Format("template '{0}'", templateName); 
				report.Trace(string.Format("Not using {0} because its MapId {1}", name, explanation), line);
				continue;
			}

			activeTemplateElement = useElement;
		}

		if (activeTemplateElement != null)
		{
			string templateName = XmlUtility.GetNameAttribtueValue(activeTemplateElement);
			if (templateName.Length == 0)
				templateName = "unnamed template";
			else
				templateName = string.Format("template '{0}'", templateName);
			report.Trace(string.Format("Using {0} for the following hotspots on '{1}'", templateName, tourPage.Name), XmlUtility.GetLineAttribtueValue(activeTemplateElement));
		}
	}

	private string GetMergedSlideDataForColumn(string columnName)
	{
		return (string)mergedSlideData[MergedSlideDataColumnName(columnName)];
	}

	private void GetTableColumnNames()
	{
		XmlNodeList tableXmlColumnNames = tableXmlDoc.SelectNodes("//table/header/column");
		tableColumnNames = new string[tableXmlColumnNames.Count];
		int index = 0;
		foreach (XmlElement tableColumnElement in tableXmlColumnNames)
		{
			string columnName = XmlUtility.GetNameAttribtueValue(tableColumnElement);
			tableColumnNames[index] = columnName;
			index++;
		}
	}

	private string GetSubstitutionValue(string columnName)
	{
		columnName = columnName.ToLower();

		// There is no "[blank]" column and normally "[blank]" is used to indicate that a
		// column's value is the empty string. However, during substitution processing, one
		// column may reference another column that contains "[blank]" as its value and so
		// here is this code we have to literaly translate "[blank]" to an empty string.
		if (columnName.ToLower() == "[blank]")
			return string.Empty;

		if (substitutions.Contains(columnName))
			return string.Format("*** CIRCULAR REFERENCE TO {0} ***", columnName);

		// Get the value for the column name.
		string result = (string)mergedSlideData[columnName.ToLower()];

		if (result == null)
			return null;

		// Since the expanded value could contain references, we have to expand it via an
		// indirect recursive call to ExpandSubstitutions. We protect against circular
		// references by keeping track of what we have attempted to substitute so far.
		// If result contains nothing to expand, this GetSubstitutionValue method will
		// terminate because ExpandSubstitutions won't call it again.
		substitutions.Push(columnName);
		result = ExpandSubstitutions(result);
		substitutions.Pop();
		return result;
	}

	private XmlNode GetTemplateDefinitionFromTableXml(string templateName)
	{
		string xPath = string.Format("//table/definitions/template[@name='{0}']", templateName);
		XmlNode tableXmlDefinitionNode = tableXmlDoc.SelectSingleNode(xPath);
		return tableXmlDefinitionNode;
	}

	private void InitSlidePropertyNameTable()
	{
		// Create an array of names corresponding to the SlideProperty enum.
		// We need this so that we can make case-insensitive comparisons.
		
		slidePropertyNames = Enum.GetNames(typeof(SlideProperty));
		
		for (int i = 0; i < slidePropertyNames.Length; i++)
		{
			// Make the first letter of the name lower case to match XML naming convention.
			string name = slidePropertyNames[i];
			name = name.Substring(0, 1).ToLower() + name.Substring(1);
			slidePropertyNames[i] = name;
		}
	}

	private int LookupNameInSlidePropertyNameTable(string text)
	{
		if (slidePropertyNames == null)
			InitSlidePropertyNameTable();

		for (int index = 0; index < slidePropertyNames.Length; index++)
		{
			if (text.ToLower() == slidePropertyNames[index].ToLower())
				return index;
		}
		return -1;
	}

	private string MergedSlideDataColumnName(string columnName)
	{
		string name = columnName.ToLower();

		if (LookupNameInSlidePropertyNameTable(columnName) != -1)
		{
			// This is a hotspot value column. Put it into the data table as though
			// it were user defined so that we can easily match references like "[SlideId]".
			name = string.Format("[{0}]", name);
		}

		return name;
	}

	private void MergeTableXmlSlideWithActiveTemplate(XmlElement tableXmlSlideElement)
	{
		// Create a place where we can put the data from the active template and merge
		// it with the data from the hotspot currently being processed (the active hotspot).
		mergedSlideData = new Hashtable();

		// Get a value for each column. 
		foreach (string columnName in tableColumnNames)
		{
			string value = string.Empty;

			// Get the value from the import hotspot column.
			string xPath = string.Format("column[@name='{0}']", columnName);
			XmlElement tableXmlSlideValueElement = (XmlElement)tableXmlSlideElement.SelectSingleNode(xPath);
			if (tableXmlSlideValueElement == null)
			{
				// The hotspot has no value. Try the template if there is one.
				if (DataGroupUsesTemplates)
				{
					tableXmlSlideValueElement = (XmlElement)activeTemplateElement.SelectSingleNode(xPath);
					if (tableXmlSlideValueElement != null)
					{
						// Use the template's value.
						value = tableXmlSlideValueElement.InnerText;
					}
				}
			}
			else
			{
				// Use the hotspot's value that overrides the template.
				value = tableXmlSlideValueElement.InnerText;
			}

			// Add the value to the table.
			string name = MergedSlideDataColumnName(columnName);
			if (mergedSlideData.Contains(name))
				report.Error("Duplicate column name detected: " + name);
			else
				mergedSlideData.Add(name, value);
		}
	}

	private void MergeTableXmlTemplateUseWithDefinition(XmlElement useElement, XmlNode tableXmlDefinitionNode)
	{
		// Get the definition's values and copy them to this use except for values that this use overrides.
		XmlNodeList tableXmlDefinitionColumnNodes = tableXmlDefinitionNode.SelectNodes("column");
		foreach (XmlElement columnElement in tableXmlDefinitionColumnNodes)
		{
			// Get the definition value. If it has none, ignore the column.
			string columnName = XmlUtility.GetNameAttribtueValue(columnElement);
			string definitionValue = columnElement.InnerText;
			if (definitionValue == string.Empty)
				continue;

			// Get the corresponding column from the use element. If the use element
			// column has a value, keep it as an override of the corresponding definition column.
			string xPath = string.Format("column[@name='{0}']", columnName);
			XmlElement useColumnElement = (XmlElement)useElement.SelectSingleNode(xPath);
			if (useColumnElement != null)
				continue;

			// Add the definition's column to the use element.
			useColumnElement = XmlUtility.CreateElement(tableXmlDoc, "column", "name", columnName);
			useColumnElement.InnerText = definitionValue;
			useElement.AppendChild(useColumnElement);
		}
	}

	private void ProcessTableXmlDataGroups(TourPage tourPage, string currentPageId, XmlElement tourXmlSlidesElement)
	{
		XmlNodeList groupNodes = tableXmlDoc.SelectNodes("//data/group");

		// Process each group for the tour page.
		foreach (XmlNode tableXmlDataGroupNode in groupNodes)
		{
			// Determine which use template to activate for the hotspots in this group.
			GetActiveTemplateForTableXmlDataGroup(tableXmlDataGroupNode, tourPage, currentPageId);

			// Apply the active template to the hotspots in this group.
			ProcessTableXmlSlides(tourPage, tableXmlDataGroupNode, tourXmlSlidesElement);
		}
	}

	private void ProcessTableXmlSlides(TourPage tourPage, XmlNode tableXmlGroupNode, XmlElement tourXmlSlidesElement)
	{
		// Get all the hotspots for this group.
		XmlNodeList tableXmlSlideNodes = tableXmlGroupNode.SelectNodes("hotspots/hotspot");
		if (tableXmlSlideNodes.Count == 0)
			return;

		if (DataGroupUsesTemplates && activeTemplateElement == null)
		{
			// No active template was found for this data group. That can happen if the
			// hotspots are not supposed to be used on the current page or tour. Tell the
			// user about it in case they didn't intend for these hotspots to be skipped.
			ReportSkippedSlidesWarning(tourPage, tableXmlSlideNodes);
			return;
		}

		// Loop over the hotspots and create a <hotspot> element for each.
		foreach (XmlElement tableXmlSlideElement in tableXmlSlideNodes)
		{
			MergeTableXmlSlideWithActiveTemplate(tableXmlSlideElement);

			if (!SlideHasTourIdAndPageId(tableXmlSlideElement, tourPage))
				continue;

			CreateTourXmlSlideElement(tourXmlSlidesElement, tableXmlSlideElement);
		}
	}

	private void ReportSkippedSlidesWarning(TourPage tourPage, XmlNodeList tableXmlSlideNodes)
	{
		// Report a warning that all the hotspots in the current group are being skipped.
		int firstSlideLine = XmlUtility.GetLineAttribtueValue((XmlElement)tableXmlSlideNodes.Item(0));
		int lastSlideLine = XmlUtility.GetLineAttribtueValue((XmlElement)tableXmlSlideNodes.Item(tableXmlSlideNodes.Count - 1));
		string message;
		if (firstSlideLine > 0 && lastSlideLine > 0)
			message = string.Format("Skipped all hotspots on rows {0} - {1} for '{2}'", firstSlideLine, lastSlideLine, tourPage.Name);
		else
			message = string.Format("Skipped all hotspots in group for {0}", tourPage.Name);
		message += " : no template applies to them";
		report.Warning(message);
		return;
	}

	private bool SlideHasTourIdAndPageId(XmlElement tableXmlSlideElement, TourPage tourPage)
	{
		string currentTourId = tour.Id.ToString();
		string currentPageId = tourPage.PageId.ToLower();
		bool hasTourId = true;
		bool hasPageId = true;
		string slideTourId = string.Empty;
		string slidePageId = string.Empty;

		// Make sure this hotspot can be used with this tour.
		slideTourId = GetMergedSlideDataForColumn("TourId");
		if (!CompatibleTourIds(slideTourId, currentTourId))
			hasTourId = false;

		// Make sure this hotspot can be used on this map.
		slidePageId = GetMergedSlideDataForColumn("MapId");
		if (!CompatiblePageIds(slidePageId, currentPageId, tour.SelectedTourPage.PageId))
			hasPageId = false;

		if (hasTourId && hasPageId)
		{
			return true;
		}
		else
		{
			string slideId = GetMergedSlideDataForColumn("HotspotId");
			int line = XmlUtility.GetLineAttribtueValue(tableXmlSlideElement);
			string message;
			if (!hasTourId)
			{
				message = string.Format("Skip hotspot '{0}' because its TourId ", slideId);
				if (slideTourId.Length == 0)
					message += "is blank and no template is providing it.";
				else
					message += string.Format("'{0}' is not for tour {1}", slideTourId, tour.Id);
				report.Trace(message, line);
			}
			if (!hasPageId)
			{
				message = string.Format("Skip hotspot '{0}' because its MapId ", slideId);
				if (slidePageId.Length == 0)
				{
					message += "is blank and no template is providing it.";
				}
				else
				{
					if (slidePageId == "*")
						message += "'*' means import into the current map";
					else
						message += string.Format("'{0}' is not for {1}", slidePageId, currentPageId);
				}
				report.Trace(message, line);
			}
			return false;
		}
	}
}
