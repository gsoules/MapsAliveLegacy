// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;

public class MapsAliveCrm
{
	private const string crmAuthToken = "61714a73673cbccac65ab8d3b738d53015b5b838";
	
	public MapsAliveCrm()
	{
	}

	public static bool AddTagToContact(string crmId, string tag)
	{
		// Create the command that tells Highrise to add a tag to a contact
		string command = string.Format("http://avantlogic.highrisehq.com/people/{0}/tags.xml", crmId);

		// Construct the XML that Highrise require to hold the tag text.
		string postData = string.Format("<name>{0}</name>", tag);

		// Make the request.
		string errorMsg;
		XmlDocument xml;
		bool success = MapsAliveHttp.MakePostXmlRequest(out xml, out errorMsg, command, crmAuthToken, postData, false);
		return success;
	}

	private static string BackgroundLink(int accountId)
	{
		string url = string.Format("https://www.mapsalive.com/account?id={0}", accountId);
		string link = string.Format("&lt;a href=\"{0}\" target=\"_blank\"&gt;MapsAlive account {1}&lt;/a&gt;", url, accountId);
		return link;
	}

	public static bool ContactHasCorrespondence(string crmId)
	{
		XmlDocument emailsXml;
		XmlDocument notesXml;

		// Get the current contact info.
		emailsXml = GetCorrespondenceXml(crmId, "emails");

		if (emailsXml == null)
		{
			// When nothing comes back at all, we know that the CRM Id is no good.
			return false;
		}

		notesXml = GetCorrespondenceXml(crmId, "notes");

		bool hasCorrespondence =
			emailsXml.InnerText != string.Empty ||
			notesXml.InnerText != string.Empty;

		return hasCorrespondence;
	}

	public static string CreateCrmContact(Account account)
	{
		return CreateCrmContact(account.Id, account.Email, account.ContactName);
	}

	public static string CreateCrmContact(int accountId, string email, string contactName)
	{
		// Convert the contact name into first and last name;
		string firstName;
		string lastName;
		CreateFirstLastName(out firstName, out lastName, contactName);

		return CreateCrmContact(accountId, email, firstName, lastName);
	}

	public static string CreateCrmContact(int accountId, string email, string firstName, string lastName)
	{
		string crmId = null;

		// Create the command that tells Highrise to create a new contact.
		string command = "http://avantlogic.highrisehq.com/people.xml";

		// Construct the XML that Highrise requires to create a new contact.
		string postData = string.Format(
			"<person>" +
			"<first-name>{0}</first-name><last-name>{1}</last-name>" +
			"<background>{2}</background>" +
			"<contact-data>" +
			"<email-addresses><email-address><address>{3}</address><location>Work</location></email-address></email-addresses>" +
			"</contact-data>" +
			"</person>",
			firstName, lastName, BackgroundLink(accountId), email);

		// Make the request.
		string errorMsg;
		XmlDocument xml;
		bool success = MapsAliveHttp.MakePostXmlRequest(out xml, out errorMsg, command, crmAuthToken, postData, false);

		// A succesful request returns the XML for the new contact.
		if (success && xml != null)
		{
			// Extract the Id of the new contact.
			XmlElement root = xml.DocumentElement;
			XmlNode idNode = root.SelectSingleNode("id");
			if (idNode != null)
			{
				crmId = idNode.InnerText;
			}
		}

		if (crmId == null)
		{
			Utility.ReportError("Failed to create CRM contact for account " + accountId, errorMsg);
			return string.Empty;
		}
		else
		{
			return crmId;
		}
	}

	private static void CreateFirstLastName(out string first, out string last, string contactName)
	{
		int index = contactName.LastIndexOf(' ');
		if (index == -1)
		{
			first = contactName;
			last = string.Empty;
		}
		else
		{
			first = contactName.Substring(0, index);
			last = contactName.Substring(index + 1);
		}
	}

	private static XmlDocument GetCorrespondenceXml(string crmId, string kind)
	{
		XmlDocument xml;
		string errorMsg;
		string command = string.Format("http://avantlogic.highrisehq.com/people/{0}/{1}.xml", crmId, kind);
		bool success = MapsAliveHttp.MakeGetRequest(out xml, out errorMsg, command, crmAuthToken, false);
		return success ? xml : null;
	}

	public static string GetCrmId(string command)
	{
		XmlDocument xml;
		string errorMsg;
		string crmId = string.Empty;

		bool success = MapsAliveHttp.MakeGetRequest(out xml, out errorMsg, command, crmAuthToken, false);

		if (success)
			crmId = GetCrmIdFromPersonXml(xml);
		
		crmId = GetCrmIdFromPersonXml(xml);

		return crmId;
	}

	public static string GetCrmIdByAccountId(int accountId)
	{
		string command = string.Format("http://avantlogic.highrisehq.com/people/search.xml?criteria[background]={0}", accountId);
		return GetCrmId(command);
	}

	public static string GetCrmIdByEmail(string email)
	{
		string command = string.Format("http://avantlogic.highrisehq.com/people/search.xml?criteria[email]={0}", email);
		return GetCrmId(command);
	}

	public static string GetCrmIdByName(string name)
	{
		string command = string.Format("http://avantlogic.highrisehq.com/people/search.xml?term={0}", name);
		return GetCrmId(command);
	}

	private static string GetCrmIdFromPersonXml(XmlDocument xml)
	{
		if (xml != null)
		{
			XmlElement root = xml.DocumentElement;
			XmlNode idNode = root.SelectSingleNode("/people/person/id");
			if (idNode != null)
			{
				return idNode.InnerText;
			}
		}
		return string.Empty;
	}

	private static string GetCrmBackgroundFromPersonXml(XmlDocument xml)
	{
		if (xml != null)
		{
			XmlElement root = xml.DocumentElement;
			XmlNode idNode = root.SelectSingleNode("/people/person/background");
			if (idNode != null)
			{
				return idNode.InnerText;
			}
		}
		return string.Empty;
	}

	private static string GetEmailIdForContact(string crmId)
	{
		string errorMsg;
		XmlDocument xml;
		string emailId = null;

		// Get the current contact info.
		string command = string.Format("http://avantlogic.highrisehq.com/people/{0}.xml", crmId);
		bool success = MapsAliveHttp.MakeGetRequest(out xml, out errorMsg, command, crmAuthToken, true);
		if (!success)
			return null;

		if (xml == null)
			return null;

		// Get a list of all the email addresses in the contact.
		XmlElement root = xml.DocumentElement;
		XmlNodeList nodeList = root.SelectNodes("contact-data/email-addresses/email-address");
		if (nodeList == null)
			return null;

		// Find the address for the "Work" location (as opposed to "Home" or "Other").
		// That's where we put the user's email when we created their account.
		foreach (XmlNode addressNode in nodeList)
		{
			XmlNode node = addressNode.SelectSingleNode("location");
			if (node.InnerText == "Work")
			{
				// Get the CRM Id for the work location. If the user has multiple work emails
				// in the CRM, we might update the wrong one, but we can live with that for now.
				// At least we'll have the email that they just updated for their MapsAlive account.
				node = addressNode.SelectSingleNode("id");
				emailId = node.InnerText;
				break;
			}
		}
		return emailId;
	}

	public static bool IsValidCrmId(string crmId)
	{
		string command = string.Format("http://avantlogic.highrisehq.com/people/{0}.xml", crmId);
		string errorMsg;
		XmlDocument xml;
		bool success = MapsAliveHttp.MakeGetRequest(out xml, out errorMsg, command, crmAuthToken, false);
		return success;
	}

	public static bool UpdateCrmBackground(string email)
	{
		XmlDocument xml;
		string errorMsg;

		string command = string.Format("http://avantlogic.highrisehq.com/people/search.xml?criteria[email]={0}", email);
		bool success = MapsAliveHttp.MakeGetRequest(out xml, out errorMsg, command, crmAuthToken, false);

		if (!success)
			return false;

		string crmId = GetCrmIdFromPersonXml(xml);
		string background = GetCrmBackgroundFromPersonXml(xml);
		string accountIdString = background.Substring(background.Length - 10);
		accountIdString = accountIdString.Substring(0, 6);
		int accountId = int.Parse(accountIdString);

		// Create the command that tells Highrise to create a new contact.
		command = string.Format("http://avantlogic.highrisehq.com/people/{0}.xml", crmId);

		// Construct the XML that Highrise requires to create a new contact.
		string postData = string.Format(
			"<person>" +
			"<background>{0}</background>" +
			"</person>",
			BackgroundLink(accountId));

		// Make the request.
		success = MapsAliveHttp.MakePutXmlRequest(out xml, out errorMsg, command, crmAuthToken, postData, false);
		return success;
	}

	public static void UpdateCrmContact(Account account)
	{
		string crmId = GetCrmIdByAccountId(account.Id);
		if (crmId == string.Empty)
		{
			// This account does not have a CRM record that links back to this account.
			return;
		}

		// Get the CRM email Id for this contact.
		string emailId = GetEmailIdForContact(crmId);
		if (emailId == null)
		{
			Utility.ReportError(string.Format("Failed to get CRM contact {0} for account {1}", crmId, account.Id), string.Empty);
			return;
		}

		string firstName;
		string lastName;
		string errorMsg;
		XmlDocument xml;

		// Convert the contact name into first and last name;
		CreateFirstLastName(out firstName, out lastName, account.ContactName);

		// Create the command that tells Highrise to create a new contact.
		string command = string.Format("http://avantlogic.highrisehq.com/people/{0}.xml", crmId);

		// Construct the XML that Highrise requires to create a new contact.
		string postData = string.Format(
			"<person>" +
			"<first-name>{0}</first-name><last-name>{1}</last-name>" +
			"<contact-data>" +
			"<email-addresses><email-address><id type=\"integer\">{3}</id><address>{2}</address><location>Work</location></email-address></email-addresses>" +
			"</contact-data>" +
			"</person>",
			firstName, lastName, account.Email, emailId);

		// Make the request.
		bool success = MapsAliveHttp.MakePutXmlRequest(out xml, out errorMsg, command, crmAuthToken, postData, false);

		if (!success)
		{
			Utility.ReportError(string.Format("Failed to update CRM contact for account {0}", account.Id), errorMsg);
		}
	}
}
