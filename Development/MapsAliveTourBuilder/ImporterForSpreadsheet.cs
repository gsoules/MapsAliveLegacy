// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using DataStreams.Csv;
using DataStreams.Xls;
using DataStreams.Xlsx;

public class ImporterForSpreadsheet
{
	private enum Instruction
	{
		Comment,
		DefineTemplate,
		UseTemplate,
		Slide,
		End,
		Eof
	}

	private enum State
	{
		Header,
		Transfer,
		Definitions,
		Uses,
		Slides,
		Eof
	}

	private XmlNode dataGroupNode;
	private Instruction instruction;
	private string instructionLabel;
	private string instructionText;
	private bool eof;
	private int lineNumber;
	private bool spreadsheetHasInstructionsColumn;
	private ImportReport report;
	private ReaderForSpreadsheet readerForSpreadsheet;
	private ArrayList spreadsheetColumnNames;
	private State state;
	private XmlDocument tableXmlDoc;
	private Tour tour;

	private const string instructionsColumnName = "instructions";

	// Entries in this table must be all lowercase.
	private string[,] propertyNameConversionTable = {
		{"slideid",		"hotspotid"},
		{"pageid",		"mapid"},
		{"firstslide",	"firsthotspot"},
		{"newslideid",	"newhotspotid"}
	};

	private enum PropertyNameRequest
	{
		GetOldName,
		GetNewName
	}

	public ImporterForSpreadsheet(Tour tour, ImportReport report)
	{
		this.tour = tour;
		this.report = report;
	}
	
	public XmlDocument CompileSpreadsheetIntoTableXml()
	{
		// Create a new XML document and initialize it with the basic structure for Table XML.
		tableXmlDoc = new XmlDocument();
		tableXmlDoc.LoadXml("<table><header/><definitions/><data/></table>");

		report.Trace("READING SPREADSHEET ...");

		// Make sure the header record (row 1) has the required columns.
		if (!ValidateSpreadsheetHeader())
			return null;
		
		// Compile the spreadheet.
		state = State.Transfer;
		EnterTransferState();

		// Write the file to disk in case we want to see what it looks like.
		if (App.DeveloperMode)
		{
			string fileLocation = FileManager.PreviewFolderLocationAbsolute(tour.Id) + "\\_table.xml";
			tableXmlDoc.Save(fileLocation);
		}

		return tableXmlDoc;
	}

	private void CreateNewDataGroup()
	{
		// Create <table><data><group><templates>
		XmlNode tableXmlDataNode = tableXmlDoc.SelectSingleNode("//data");
		dataGroupNode = XmlUtility.CreateElement(tableXmlDoc, "group");
		tableXmlDataNode.AppendChild(dataGroupNode);
	}

	public bool CreateSpreadsheetReader(byte[] data)
	{
		readerForSpreadsheet = new ReaderForSpreadsheet(data);
		return readerForSpreadsheet.Opened;
	}

	private void CreateTableXmlColumnElements(XmlElement parentElement)
	{
		// Populate the passed-in element with column values from the current record.

		foreach (string columnName in spreadsheetColumnNames)
		{
			// Ignore the instructions column
			if (columnName.ToLower() == instructionsColumnName)
				continue;

			string columnValue = ReadSpreadsheetRecordColumn(columnName);
			if (columnValue.Length > 0)
			{
				// Create <column name="...">...</column>.
				XmlElement columnElement = XmlUtility.CreateElement(tableXmlDoc, "column", "name", columnName);
				columnElement.InnerText = columnValue;
				parentElement.AppendChild(columnElement);
			}
		}
	}

	private void EnterDefinitionsState()
	{
		StateRequiresDataGroup = false;

		// Get <table><definitions>.
		XmlNode definitionsNode = tableXmlDoc.SelectSingleNode("//definitions");

		while (state == State.Definitions)
		{
			TraceState();

			// Create <table><definitions><template>.
			XmlElement templateElement = XmlUtility.CreateElement(tableXmlDoc, "template", "name", instructionLabel);
			definitionsNode.AppendChild(templateElement);
			CreateTableXmlColumnElements(templateElement);
			report.Trace(string.Format("Read definition '{0}'", instructionText), lineNumber);

			ReadNextInstruction();

			switch (instruction)
			{
				case Instruction.UseTemplate:
					state = State.Uses;
					break;

				case Instruction.Slide:
					state = State.Slides;
					break;

				case Instruction.End:
				case Instruction.Eof:
					state = State.Transfer;
					break;
			}
		}
	}

	private void EnterSlidesState()
	{
		StateRequiresDataGroup = true;

		// Create <table><data><group><hotspots>
		XmlElement slidesElement = XmlUtility.CreateElement(tableXmlDoc, "hotspots");
		dataGroupNode.AppendChild(slidesElement);

		// Loop over hotspot records until a template use record or eof is encountered.
		while (state == State.Slides)
		{
			TraceState();

			// Create <table><data><group><hotspots><hotspot>
			XmlElement slideElement = XmlUtility.CreateElement(tableXmlDoc, "hotspot", "line", lineNumber.ToString());
			slidesElement.AppendChild(slideElement);
			CreateTableXmlColumnElements(slideElement);
			
			string slideId = ReadSpreadsheetRecordColumn("HotspotId");
			report.Trace(string.Format("{0} : Read", slideId), lineNumber);
			
			ReadNextInstruction();

			switch (instruction)
			{
				case Instruction.DefineTemplate:
					state = State.Definitions;
					break;
				
				case Instruction.UseTemplate:
					state = State.Uses;
					break;
				
				case Instruction.End:
				case Instruction.Eof:
					StateRequiresDataGroup = false;
					state = State.Transfer;
					break;
			}
		}
	}

	private void EnterTransferState()
	{
		while (!eof)
		{
			if (state == State.Transfer)
			{
				TraceState();
				ReadNextInstruction();
			}
			
			switch (instruction)
			{
				case Instruction.DefineTemplate:
					state = State.Definitions;
					EnterDefinitionsState();
					break;

				case Instruction.UseTemplate:
					state = State.Uses;
					EnterUsesState();
					break;

				case Instruction.Slide:
					state = State.Slides;
					EnterSlidesState();
					break;

				case Instruction.End:
					report.Warning("Ignored extra End instruction", lineNumber);
					break;
			}
		}
	}

	private void EnterUsesState()
	{
		CreateNewDataGroup();
		
		XmlElement tableXmlTemplatesElement = XmlUtility.CreateElement(tableXmlDoc, "templates");
		dataGroupNode.AppendChild(tableXmlTemplatesElement);

		while (state == State.Uses)
		{
			TraceState();

			// Create <table><data><group><templates><use>
			XmlElement tableXmlUseElement = XmlUtility.CreateElement(tableXmlDoc, "use", "name", instructionLabel);
			XmlUtility.CreateAttribute(tableXmlDoc, tableXmlUseElement, "line", lineNumber.ToString());
			tableXmlTemplatesElement.AppendChild(tableXmlUseElement);
			CreateTableXmlColumnElements(tableXmlUseElement);
			report.Trace(string.Format("Read use '{0}'", instructionText), lineNumber);

			ReadNextInstruction();
			
			switch (instruction)
			{
				case Instruction.DefineTemplate:
					report.Warning("Unexpected Define instruction. Previous Use instruction has no effect.");
					state = State.Definitions;
					break;

				case Instruction.Slide:
					state = State.Slides;
					break;

				case Instruction.End:
				case Instruction.Eof:
					state = State.Transfer;
					break;
			}
		}
	}

	private string GetConvertedPropertyName(string requestedName, PropertyNameRequest request)
	{
		requestedName = requestedName.ToLower();
		
		int lookupIndex = request == PropertyNameRequest.GetOldName ? 1 : 0;
		int returnIndex = request == PropertyNameRequest.GetOldName ? 0 : 1;

		// Each row in the table has two elements (old name and new name) so we halve the length.
		int tableLength = propertyNameConversionTable.Length / 2;

		for (int row = 0; row < tableLength; row++)
		{
			string name = propertyNameConversionTable[row, lookupIndex];
			if (name == requestedName)
				return propertyNameConversionTable[row, returnIndex];
		}
		return null;
	}

	private Instruction GetInstruction(string text)
	{
		text = text.Trim().ToLower();

		if (text.Length == 0 || text.StartsWith("\0"))
		{
			// The text can start with \0 if the last record of a .csv file is empty.
			return Instruction.Comment;
		}

		if (ParseInstruction("#", text))
			return Instruction.Comment;

		if (ParseInstruction("use template", text))
			return Instruction.UseTemplate;

		if (ParseInstruction("define template", text))
			return Instruction.DefineTemplate;

		if (ParseInstruction("end", text))
			return Instruction.End;

		report.Warning(string.Format("Ignored unrecognized instruction '{0}'", text), lineNumber);
		return Instruction.Comment;
	}

	private ArrayList GetSpreadsheetColumnNames()
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

		// Ignore columns that are not a hotspot property name like "SlideId" and "Title"
		// and are not a user-defined data column name enclosed in square brackets.
		ArrayList allowedColumnNames = new ArrayList();
		string[] slidePropertyNames = Enum.GetNames(typeof(SlideProperty));
		string nameLower = string.Empty;

		foreach (string name in names)
		{
			nameLower = name.ToLower();
			bool addName = false;

			if (name.StartsWith("[") && name.EndsWith("]"))
			{
				if (!Utility.IsAlphaNumeric(name.Substring(1, name.Length - 2)))
				{
					report.Warning(string.Format("Ignored column '{0}' (only letters and digits allowed)", name), 1);
					continue;
				}

				// The column is for a user-defined data value. The column name cannot contain spaces.
				addName = true;
			}
			else if (nameLower == "tourid" || nameLower == "mapid")
			{
				// We have to special case these because they are not hotspot properties, but are required columns.
				addName = true;
			}
			else
			{
				// Make a case-insensitive comparison to hotspot property names.
				foreach (string propertyName in slidePropertyNames)
				{
					if (nameLower == propertyName.ToLower())
					{
						// The column is for a hotspot property.
						addName = true;
						break;
					}
				}
			}

			if (addName)
				allowedColumnNames.Add(nameLower);
			else
			{
				if (name.Length > 0)
					report.Warning(string.Format("Ignored column '{0}' (not a hotspot property)", name), 1);
			}
		}

		return allowedColumnNames;
	}

	private bool ParseInstruction(string command, string text)
	{
		instructionLabel = string.Empty;
		
		if (command == "#" && text.StartsWith("#"))
			return true;

		// Remove extraneous spaces so that "Define template foo" is same as "Define   template   foo".
		text = Regex.Replace(text, @"\s+", " ");

		if (text == command)
		{
			// The text is just the command with no label.
			return true;
		}

		if (text.StartsWith(command.ToLower() + " "))
		{
			// Extract the label from the text following the command up to the next space.
			instructionLabel = text.Substring(command.Length + 1);
			int spaceIndex = instructionLabel.IndexOf(' ');
			if (spaceIndex != -1)
				instructionLabel = instructionLabel.Substring(0, spaceIndex);
			return true;
		}

		return false;
	}

	private void ReadNextInstruction()
	{
		do
		{
			try
			{
				eof = !readerForSpreadsheet.Reader.ReadRecord();
				lineNumber++;
			}
			catch (Exception ex)
			{
                Utility.Trace(string.Format("ReadNextInstruction: {0} {1}", lineNumber, ex.Message));
                report.Trace(string.Format("Unexpected exception in ReadNextInstruction: {0}", ex.Message), lineNumber);
                eof = true;
			}

			if (eof)
			{
				instruction = Instruction.Eof;
			}
			else
			{
				instructionText = string.Empty;
				if (spreadsheetHasInstructionsColumn)
					instructionText = ReadSpreadsheetRecordColumn(instructionsColumnName).Trim();

				if (instructionText.Length == 0)
				{
					if (SpreadsheetRowContainsSlide())
						instruction = Instruction.Slide;
					else
						instruction = Instruction.Comment;
				}
				else
				{
					instruction = GetInstruction(instructionText);
				}
			}
		}
		while (instruction == Instruction.Comment);
	}

	public string ReadSpreadsheetRecordColumn(string columnName)
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

	private bool SpreadsheetHasRequiredColumns()
	{
		bool hasHotspotId = false;
		bool hasTitle = false;
		bool hasTourId = false;
		bool hasMapId = false;

		foreach (string columnName in spreadsheetColumnNames)
		{
			switch (columnName.ToLower())
			{
				case "hotspotid":
					hasHotspotId = true;
					break;

				case "title":
					hasTitle = true;
					break;

				case "tourid":
					hasTourId = true;
					break;

				case "mapid":
					hasMapId = true;
					break;

				default:
					break;

			}
		}

		if (hasHotspotId && hasTitle && hasTourId && hasMapId)
		{
			return true;
		}
		else
		{
			string column = string.Empty;

			if (!hasHotspotId)
				column = "HotspotId";
			else if (!hasTitle)
				column = "Title";
			else if (!hasTourId)
				column = "TourId";
			else if (!hasMapId)
				column = "MapId";

			report.Error(string.Format("Required column '{0}' is missing", column), lineNumber);
			return false;
		}
	}

	public bool SpreadsheetRowContainsSlide()
	{
		string slideIdColumnValue = ReadSpreadsheetRecordColumn("HotspotId");
		return slideIdColumnValue != string.Empty;
	}

	private bool StateRequiresDataGroup
	{
		set
		{
			if (value == true)
			{
				if (dataGroupNode == null)
					CreateNewDataGroup();
			}
			else
			{
				dataGroupNode = null;
			}
		}
	}

	private void TraceState()
	{
		Debug.WriteLine(string.Format("{0} {1}", state, lineNumber));
	}

	private bool ValidateSpreadsheetHeader()
	{
		// Get <table><header>.
		XmlNode headerNode = tableXmlDoc.SelectSingleNode("//header");

		spreadsheetColumnNames = GetSpreadsheetColumnNames();

		if (!SpreadsheetHasRequiredColumns())
			return false;

		foreach (string columnName in spreadsheetColumnNames)
		{
			// Add a column element for each column in the header record. If the header contains
			// an "instructions" column, we know we need to process template definitions and uses.
			headerNode.AppendChild(XmlUtility.CreateElement(tableXmlDoc, "column", "name", columnName));
			if (columnName.ToLower() == instructionsColumnName)
				spreadsheetHasInstructionsColumn = true;
		}

		lineNumber = 1;
		return true;
	}
}
