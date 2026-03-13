// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Web;

public class MapsAliveDatabaseException : ApplicationException
{
	public MapsAliveDatabaseException(string message) : base(message) { }
}

public class MapsAliveDatabase
{
	private const string NoDatabase = "UNKNOWN:NOT CONNECTED";

	public MapsAliveDatabase()
	{
	}

	public static string DatabaseId
	{
		get
		{
			string id = NoDatabase;
			string status = string.Empty;

			try
			{
				MapsAliveDataRow row = MapsAliveDatabase.LoadDataRow("sp_Database_Id");
				if (row == null)
				{
					status = "No row returned from sp_Database_Id";
				}
				else
				{
					string hostName = row.StringValue("hostname").Trim();
					string dbName = row.StringValue("dbname").Trim();
					id = string.Format("{0}:{1}", hostName, dbName);
				}
			}
			catch (SqlException ex)
			{
				if (ex.Number == 2812)
				{
					// The error is "Could not find stored procedure" which means
					// we connected to the database, but it's not the right database.
					status = "Connected to database, but the schema is not for MapsAlive";
				}
				else
				{
					status = string.Format("{0} : {1}", ex.Number, ex.Message);
				}
			}
			catch (Exception ex)
			{
				status = ex.Message;
			}

			// Save the status in memory so that we don't get into recursion by trying
			// to access the database while trying to report that we can't access it.
			MapsAliveState.Persist(MapsAliveObjectType.DatabaseStatus, status);
			
			return id;
		}
	}

	public static string DatabaseStatus
	{
		get
		{
			string status = (string)MapsAliveState.Retrieve(MapsAliveObjectType.DatabaseStatus);
			if (status == null)
				status = string.Empty;
			return status;
		}
	}

	public static int DatabaseVersion
	{
		get
		{
			try
			{
				return (int)MapsAliveDatabase.LoadScalar("sp_App_GetDatabaseVersion");
			}
			catch
			{
				return 0;
			}
		}
	}

	public static bool NotConnected
	{
		get { return DatabaseId == NoDatabase; }
	}

	public static string ConnectionString
	{
		get 
		{
			ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["MapsAliveSqlServer"];
			if (settings == null)
				return string.Empty;
			else
				return settings.ConnectionString;
		}
	}

	public static void ExecuteStoredProcedure(string storedProcedureName, params object[] parameters)
	{
		try
		{
			using (SqlConnection connection = new SqlConnection(ConnectionString))
			{
				connection.Open();

				using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
				{
                    int timeout = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MapsAliveSqlTimeOut"]);
                    if (timeout > 0)
                        command.CommandTimeout = timeout;

                    command.CommandType = CommandType.StoredProcedure;

                    AddParameters(command, parameters);
					command.ExecuteNonQuery();
				}
			}
		}
		catch (Exception ex)
		{
			Utility.ReportDatabaseException("ExecuteStoredProcedure " + storedProcedureName, ex);
			Utility.TransferToConnectionDroppedPage(ex);
		}
	}

	public static int GetCount(string storedProcedureName, params object[] parameters)
	{
		return (int)ReadScalar(storedProcedureName, parameters);
	}

	public static bool IsTrue(string storedProcedureName, params object[] parameters)
	{
		return (int)ReadScalar(storedProcedureName, parameters) == 1;
	}

	public static MapsAliveDataRow LoadDataRow(string storedProcedureName, params object[] parameters)
	{
		MapsAliveDataRow dataRow = null;

		try
		{
			DataTable dataTable = LoadDataTable(storedProcedureName, parameters);
			int rowsCount = dataTable.Rows.Count;
			Debug.Assert(rowsCount <= 1, string.Format("{0} returned {1} rows instead of 1 or 0", storedProcedureName, rowsCount));
			DataRow row = rowsCount == 1 ? dataTable.Rows[0] : null;
			if (row != null)
				dataRow = new MapsAliveDataRow(row);
		}
		catch (Exception ex)
		{
			if (ex is System.Threading.ThreadAbortException)
			{
				// The call to LoadDataTable got and handled a database exception don't report this one.
				return null;
			}
			Utility.ReportDatabaseException("LoadDataRow " + storedProcedureName, ex);
			Utility.TransferToConnectionDroppedPage(ex);
		}

		return dataRow;
	}

	public static DataTable LoadDataTable(string storedProcedureName, params object[] parameters)
	{
		DataTable dataTable = null;

		try
		{
			using (SqlConnection connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
				{
                    // Override the default time out of 30 seconds by using the value in web.config.
                    int timeout = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MapsAliveSqlTimeOut"]);
                    if (timeout > 0)
                        command.CommandTimeout = timeout;
                    
                    command.CommandType = CommandType.StoredProcedure;
					AddParameters(command, parameters);
					SqlDataReader dataReader = command.ExecuteReader();
					dataTable = new DataTable();
					dataTable.Load(dataReader);
					dataReader.Close();
				}
			}
		}
		catch (Exception ex)
		{
			Utility.ReportDatabaseException("LoadDataTable " + storedProcedureName, ex);
			Utility.TransferToConnectionDroppedPage(ex);
		}
		
		return dataTable;
	}

	public static object LoadScalar(string storedProcedureName, params object[] parameters)
	{
		object scalar = null;

		try
		{
			using (SqlConnection connection = new SqlConnection(ConnectionString))
			{
				connection.Open();
				using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					AddParameters(command, parameters);
					scalar = command.ExecuteScalar();
				}
			}
		}
		catch (SqlException ex)
		{
			Utility.ReportDatabaseException("LoadScalar " + storedProcedureName, ex);
			Utility.TransferToConnectionDroppedPage(ex);
		}

		return scalar;
	}

	public static object ReadColumn(int columnIndex, string storedProcedureName, params object[] parameters)
	{
		DataTable dataTable = LoadDataTable(storedProcedureName, parameters);
		ReportDatabaseErrorIf(dataTable.Rows.Count != 1, "Expected exactly 1 row from " + storedProcedureName);
		return dataTable.Rows[0][columnIndex];
	}

	public static object ReadColumn(string columnName, string storedProcedureName, params object[] parameters)
	{
		DataTable dataTable = LoadDataTable(storedProcedureName, parameters);
		ReportDatabaseErrorIf(dataTable.Rows.Count != 1, "Expected exactly 1 row from " + storedProcedureName);
		object value = dataTable.Rows[0][columnName];
		if (value is DBNull)
			return null;
		else
			return value;
	}

	public static int ReadInt(string storedProcedureName, params object[] parameters)
	{
		object value = LoadScalar(storedProcedureName, parameters);
		if (value == null || value is DBNull)
			return 0;
		else
			return (int)value;
	}

	public static object ReadScalar(string storedProcedureName, params object[] parameters)
	{
		return LoadScalar(storedProcedureName, parameters);
	}

	public static void ReportDatabaseErrorIf(bool condition, string message)
	{
		if (condition)
			Utility.ReportError("Database Error", message);
	}

	public static MapsAliveDataRow SelectRowFromDataTable(DataTable dataTable, string filterExp)
	{
		return new MapsAliveDataRow(dataTable.Select(filterExp)[0]);
	}

	public static void AddParameters(SqlCommand command, object[] parameters)
	{
		int lastParameter = parameters.Length;
		Debug.Assert(lastParameter % 2 == 0, "An even number of parameters is required");
		for (int i = 0; i < lastParameter; i += 2)
		{
			command.Parameters.AddWithValue((string)parameters[i], parameters[i + 1]);
		}
	}
}
