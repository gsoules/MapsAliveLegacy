// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public abstract class MapsAliveDataRecord
{
	public abstract bool BoolValue<T>(T tag);

	public virtual bool BoolValue(string columnName)
	{
		return false;
	}

	public bool BoolValue<T>(string columnName, T tag)
	{
		// This method and others like it that refer to the MapsAliveDataRow subclass of this MapsAliveDataRecord
		// class, violate basic rules of object oriented design. We do it, however, to keep the code that uses
		// these methods as simple, readable, and maintainable as possible. The purpose of these methods that
		// take both a columnName and a tag as parameters is to allow us to use newer more desirable tag names
		// that will appear in exported XML while still being able to pass old column names to the database. For
		// example, the database column "SlideLayoutFixedMarginTop" is replaced with the XML tag layoutAreaMarginTop.
		// If "this" is a MapsAliveDataRow, we pass the column name string to the database BoolValue method, but
		// if "this" is a MapsAliveDataRecordXml, we pass the tag to the XML BoolValue method.
		if (this is MapsAliveDataRow)
			return BoolValue(columnName);
		else
			return BoolValue(tag);
	}

	public virtual Byte[] ByteArrayValue(string columnName)
	{
		return new Byte[0];
	}

	public abstract string ColorValue<T>(T tag);

	public virtual string ColorValue(string columnName)
	{
		return "#ffffff";
	}

	public string ColorValue<T>(string columnName, T tag)
	{
		if (this is MapsAliveDataRow)
			return ColorValue(columnName);
		else
			return ColorValue(tag);
	}

	public virtual DateTime DateTimeValue(string columnName)
	{
		return DateTime.MinValue;
	}

	public abstract double DoubleValue<T>(T tag);

	public abstract int IntValue<T>(T tag);

	public virtual int IntValue(string columnName)
	{
		return 0;
	}

	public virtual long LongValue(string columnName)
	{
		return 0;
	}

	public int IntValue<T>(string columnName, T tag)
	{
		if (this is MapsAliveDataRow)
			return IntValue(columnName);
		else
			return IntValue(tag);
	}

	public virtual bool IsNull(string columnName)
	{
		return true;
	}

	public abstract string StringValue<T>(T tag);

	public virtual string StringValue(string columnName)
	{
		return string.Empty;
	}

	public string StringValue<T>(string columnName, T tag)
	{
		if (this is MapsAliveDataRow)
			return StringValue(columnName);
		else
			return StringValue(tag);
	}
}
