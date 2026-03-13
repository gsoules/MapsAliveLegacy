// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;

public class MapsAliveDataRow : MapsAliveDataRecord
{
	DataRow row;

	public MapsAliveDataRow(DataRow row)
	{
		this.row = row;
	}

	public bool HasColumn(string columnName)
	{
		return row.Table.Columns.Contains(columnName);
	}

	public override bool BoolValue<T>(T tag)
	{
		return BoolValue(tag.ToString());
	}

	public override bool BoolValue(string columnName)
	{
		object value = Row(columnName);
		if (value is DBNull)
			return false;
		else
			return (bool)value;
	}

	public override Byte[] ByteArrayValue(string columnName)
	{
		object value = Row(columnName);
		if (value is DBNull)
			return new Byte[0];
		else
			return (Byte[])value;
	}

	public override string ColorValue<T>(T tag)
	{
		return ColorValue(tag.ToString());
	}

	public override string ColorValue(string columnName)
	{
		object value = Row(columnName);
		if (value is DBNull)
			return "#ffffff";
		else
			return (string)value;
	}

	public override DateTime DateTimeValue(string columnName)
	{
		object value = Row(columnName);
		if (value is DBNull)
			return DateTime.MinValue;
		else
			return (DateTime)value;
	}

	public string DateTimeValueString(string columnName)
	{
		object value = Row(columnName);
		if (value is DBNull)
			return string.Empty;
		else
		{
			DateTime date = (DateTime)value;
			return string.Format("{0} at {1}", date.ToShortDateString(), date.ToShortTimeString());
		}
	}

	public decimal DecimalValue(string columnName)
	{
		object value = Row(columnName);
		if (value is DBNull)
			return 0;
		else
			return (decimal)value;
	}

	public override double DoubleValue<T>(T tag)
	{
		return DoubleValue(tag.ToString());
	}

	public double DoubleValue(string columnName)
	{
		object value = Row(columnName);
		if (value is DBNull)
			return 0;
		else
			return Convert.ToDouble(value);
	}

	public Guid GuidValue(string columnName)
	{
		object value = Row(columnName);
		if (value is DBNull)
			return Guid.Empty;
		else
			return (Guid)value;
	}

	public override int IntValue(string columnName)
	{
		object value = Row(columnName);
		if (value is DBNull)
			return 0;
		else
			return (int)value;
	}

	public override int IntValue<T>(T tag)
	{
		return IntValue(tag.ToString());
	}

	public override bool IsNull(string columnName)
	{
		return Row(columnName) is DBNull;
	}

	public override long LongValue(string columnName)
	{
		object value = Row(columnName);
		if (value is DBNull)
			return 0;
		else
			return (long)value;
	}

	public decimal MoneyValue(string columnName)
	{
		object value = Row(columnName);
		if (value is DBNull)
			return 0;
		else
			return (decimal)value;
	}

	private object Row(string columnName)
	{
		try
		{
			return row[columnName];
		}
		catch (ArgumentException ex)
		{
			Utility.ReportException("Row " + columnName, ex);
			return null;
		}
	}

	public override string StringValue(string columnName)
	{
		object value = Row(columnName);
		if (value is DBNull)
			return string.Empty;
		else
			return (string)value;
	}

	public override string StringValue<T>(T tag)
	{
		return StringValue(tag.ToString());
	}
}
