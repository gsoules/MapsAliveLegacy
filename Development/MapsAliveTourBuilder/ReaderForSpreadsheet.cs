// Copyright (C) 2003-2010 AvantLogic Corporation
using System.IO;
using System.Text;
using DataStreams.Common;
using DataStreams.Csv;
using DataStreams.Xls;
using DataStreams.Xlsx;

public enum SpreadsheetType
{
	csv,
	xls,
	xlsx
}

public class ReaderForSpreadsheet
{
	private bool opened;
	private ReaderBase reader;
	private SpreadsheetType readerType;

	public bool Opened
	{
		get { return opened; }
	}

	public ReaderBase Reader
	{
		get { return reader; }
	}

	public SpreadsheetType Type
	{
		get { return readerType; }
	}
	
	public ReaderForSpreadsheet(byte[] data)
	{
		MemoryStream stream;

		try
		{
			stream = new MemoryStream(data);
			readerType = SpreadsheetType.xls;
			XlsReader xlsReader = new XlsReader(stream);
			xlsReader.Settings.HasHeaders = true;
			xlsReader.Settings.CaseSensitive = false;
			reader = xlsReader;
			opened = true;
		}
		catch
		{
		}

		if (!opened)
		{
			try
			{
				stream = new MemoryStream(data);
				readerType = SpreadsheetType.xlsx;
				XlsxReader xlsReader = new XlsxReader(stream);
				xlsReader.Settings.HasHeaders = true;
				xlsReader.Settings.CaseSensitive = false;
				reader = xlsReader;
				opened = true;
			}
			catch
			{
			}
		}

		if (!opened)
		{
			try
			{
				stream = new MemoryStream(data);
				readerType = SpreadsheetType.csv;
				CsvReader csvReader = new CsvReader(stream, Encoding.Default);
				csvReader.Settings.CaseSensitive = false;
				csvReader.ReadHeaders();
				reader = csvReader;
				opened = true;
			}
			catch
			{
			}
		}
	}
}
