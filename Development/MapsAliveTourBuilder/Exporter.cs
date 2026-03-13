// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.IO;
using System.Text;
using System.Xml;

public class Exporter
{
	protected byte[] xmlBytes;
	protected MemoryStream xmlMemoryStream;
	protected XmlWriter xmlWriter;
	protected XmlWriterSettings xmlWriterSettings;

	protected void CopyXmlMemoryStreamToBytes()
	{
		// Copy the XML stream to an array of bytes. Note that we use the stream length
		// instead of GetBuffer() length because the internal buffer is a fixed size
		// that can be larger than what's in the stream. Unfilled bytes at the end of
		// the buffer get padded with zeros which we don't want to export.
		xmlMemoryStream.Position = 0;
		xmlBytes = new byte[(int)xmlMemoryStream.Length];
		Array.Copy(xmlMemoryStream.GetBuffer(), xmlBytes, xmlBytes.Length);
	}

	protected void CreateFile(string fileLocation, byte[] bytes)
	{
		using (FileStream fileStream = new FileStream(fileLocation, FileMode.Create, FileAccess.Write))
		{
			fileStream.Write(bytes, 0, bytes.Length);
		}
	}

	protected void CreateXmlMemoryStreamAndSettings()
	{
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Indent = true;
		xmlWriterSettings.IndentChars = ("\t");
		
		xmlMemoryStream = new MemoryStream();
	}

	protected string CreateTimeStampGmt()
	{
		DateTime nowGmt = DateTime.Now.ToUniversalTime();
		string stamp = string.Format("Created on {0:r} by MapsAlive {1}", nowGmt, App.VersionStamp);
		return stamp;
	}
	
	protected void EmitElement(string name, int value)
	{
		EmitElement(name, value.ToString());
	}

	protected void EmitElement(string name, bool value)
	{
		EmitElement(name, value ? "true" : "false");
	}

	protected void EmitElement(SlideProperty property, string value)
	{
		string propertyName = property.ToString();
		
		// Make the first letter of the property name lower case.
		propertyName = propertyName.Substring(0, 1).ToLower() + propertyName.Substring(1);
		
		EmitElement(propertyName, value);
	}

	protected void EmitElement(string name, string value)
	{
		if (string.IsNullOrEmpty(value))
			return;

		StringBuilder sb = new StringBuilder();

		foreach (char c in value)
		{
			if (Utility.IsLegalXmlChar(c))
			{
				sb.Append(c);
			}
			else
			{
				sb.Append("?");
			}
		}

		xmlWriter.WriteStartElement(name);
		xmlWriter.WriteString(sb.ToString());
		xmlWriter.WriteEndElement();
	}
}
