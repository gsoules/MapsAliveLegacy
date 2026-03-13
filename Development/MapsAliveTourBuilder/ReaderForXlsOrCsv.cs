// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.IO;
using System.Text;
using DataStreams.Xls;
using DataStreams.Csv;
using DataStreams.Common;

public class ReaderForXlsOrCsv
{
	private bool isOpen;
	private ReaderBase reader;

	public ReaderForXlsOrCsv(string fileExtension, Stream stream)
	{
		try
		{
			if (fileExtension == ".xls")
			{
				XlsReader xlsReader = new XlsReader(stream);
				xlsReader.Settings.HasHeaders = true;
				xlsReader.Settings.CaseSensitive = false;
				reader = xlsReader;
			}
			else
			{
				CsvReader csvReader = new CsvReader(stream, Encoding.Default);
				csvReader.Settings.CaseSensitive = false;
				csvReader.ReadHeaders();
				reader = csvReader;
			}
			isOpen = true;
		}
		catch
		{
			isOpen = false;
		}
	}

	public void Close()
	{
		if (reader != null)
			reader.Close();
	}

	public ulong CurrentRecordNumber
	{
		get { return reader.CurrentRecord; }
	}

	public bool HasRequiredHeader(string headerName)
	{
		if (reader is XlsReader)
			return (reader as XlsReader).GetIndex(headerName) > -1;
		else
			return (reader as CsvReader).GetIndex(headerName) > -1;
	}

	public bool IsOpen
	{
		get { return isOpen; }
	}

	public string ReadColumn(string headerName)
	{
		string text;
		
		if (reader is XlsReader)
			text = ((XlsReader)reader)[headerName];
		else
			text = ((CsvReader)reader)[headerName];
		
		return text;
	}

	public string ReadColumnTrim(string headerName)
	{
		return ReadColumn(headerName).Trim();
	}

	public bool ReadRecord()
	{
		try
		{
			return reader.ReadRecord();
		}
		catch (Exception)
		{
			return false;
		}
	}
}
