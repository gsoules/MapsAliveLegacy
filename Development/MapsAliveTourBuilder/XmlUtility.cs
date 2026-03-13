// Copyright (C) 2003-2010 AvantLogic Corporation
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;

public class XmlUtility
{
	public static void CreateAttribute(XmlDocument xmlDoc, XmlElement element, string name, string value)
	{
		XmlAttribute attribute = xmlDoc.CreateAttribute(name);
		attribute.Value = value;
		element.Attributes.Append(attribute);
	}

	public static XmlElement CreateElement(XmlDocument xmlDoc, string elementName)
	{
		return CreateElement(xmlDoc, elementName, null, null);
	}

	public static XmlElement CreateElement(XmlDocument xmlDoc, string elementName, string attributeName, string attributeValue)
	{
		// Create an element and add an attribute to it.
		XmlElement element = xmlDoc.CreateElement(elementName);

		if (attributeName != null)
			CreateAttribute(xmlDoc, element, attributeName, attributeValue);

		return element;
	}

	public static int GetLineAttribtueValue(XmlElement element)
	{
		int line = 0;
		
		XmlAttribute attribute = element.Attributes["line"];
		
		if (attribute != null)
			int.TryParse(attribute.Value, out line);
	
		return line;
	}

	public static string GetNameAttribtueValue(XmlElement element)
	{
		XmlAttribute attribute = element.Attributes["name"];
		return attribute == null ? string.Empty : attribute.Value;
	}

	public static XPathNodeIterator XPathNavigatorForData(string xmlFileName, string xPath)
	{
		XPathDocument xPathDocument = new XPathDocument(FileManager.WebAppFileLocationAbsolute("App_Data", xmlFileName));
		XPathNavigator navigator = xPathDocument.CreateNavigator();
		XPathNodeIterator resourceNodes = navigator.Select(xPath);
		return resourceNodes;
	}
}
