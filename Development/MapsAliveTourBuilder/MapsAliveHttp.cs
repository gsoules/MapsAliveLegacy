// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;

public class MapsAliveHttp
{
	public static bool MakeGetRequest(out XmlDocument xml, out string errorMsg, string command, string authToken, bool reportError)
	{
		return MakeHttpRequest(out xml, out errorMsg, "GET", authToken, command, null, null, reportError);
	}

	public static bool MakePostXmlRequest(out XmlDocument xml, out string errorMsg, string command, string authToken, string xmlData, bool reportError)
	{
		return MakeHttpRequest(out xml, out errorMsg, "POST", authToken, command, xmlData, "application/xml", reportError);
	}

	public static bool MakePutXmlRequest(out XmlDocument xml, out string errorMsg, string command, string authToken, string xmlData, bool reportError)
	{
		return MakeHttpRequest(out xml, out errorMsg, "PUT", authToken, command, xmlData, "application/xml", reportError);
	}

	private static bool MakeHttpRequest(out XmlDocument xml, out string errorMsg, string method, string authToken, string command, string content, string contentType, bool reportError)
	{
		xml = null;
		errorMsg = string.Empty;

		method = method.ToUpper();
		if (method != "GET" && method != "POST" && method != "PUT")
		{
			errorMsg = string.Format("'{0}' is not a supported method", method);
			return false;
		}

		try
		{
			ASCIIEncoding encoding = new ASCIIEncoding();
			
			// Create the request.
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(command);
			request.Method = method;

			// Set basic authorization if an authentication token was passed.
			if (authToken != null)
			{
				string token = Convert.ToBase64String(Encoding.ASCII.GetBytes(authToken));
				request.Headers.Add("Authorization", "Basic " + token);
			}

			// Set the data to be sent with the request. Note that the ContentLength will be set
			// automatically by the stream writer. If we attempt to set it based on content.Length
			// and if the text contains multi-byte characters e.g. a letter with an accent mark,
			// we can end up setting it too short and cause an exception.
			if (method == "POST" || method == "PUT")
			{
				request.ContentType = contentType;
				using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
				{
					streamWriter.Write(content);
					streamWriter.Close();
				}
			}

			// Send the request.
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();

			// Create an XML document from the response.
			if (method == "POST" || method == "GET")
			{
				using (Stream stream = response.GetResponseStream())
				{
					using (StreamReader streamReader = new StreamReader(stream))
					{
						xml = new XmlDocument();
						xml.Load(streamReader.BaseStream);
						streamReader.Close();
					}

					stream.Close();
				}
			}
			
			return true;
		}
		catch (Exception ex)
		{
			if (reportError)
			{
				errorMsg = ex.Message;
				Utility.ReportError("MakeHttpRequest failed", ex.Message);
			}
			return false;
		}
	}
}

