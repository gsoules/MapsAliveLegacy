// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public class CreditCard
{
	private string address;
	private string cardNumber;
	private string city;
	private string country;
	private string cvv;
	private string expMonth;
	private string expYear;
	private string firstName;
	private bool hasMissingData;
	private string lastName;
	private string state;
	private string zip;

	public CreditCard()
	{
		// All of the string values in this class are initialized here 
		// so that an empty string is returned if the value is null. We do this for
		// the case when an order is no-charge (total is $0.0) and there is no credit
		// card data, but we are still using a card as part of the order process.
		
		 address = string.Empty;
		 cardNumber = string.Empty;
		 city = string.Empty;
		 country = "US";
		 cvv = string.Empty;
		 expMonth = string.Empty;
		 expYear = string.Empty;
		 firstName = string.Empty;
		 lastName = string.Empty;
		 state = "? ";
		 zip = string.Empty;
	}


	public string Address
	{
		get { return address; }
	}

	public void ReadAddress(string value, Label errorLabel)
	{
		address = value;
		Validate(value, errorLabel);
	}

	public string CardNumber
	{
		get { return cardNumber; }
	}

	public void ReadCardNumber(string value, Label errorLabel)
	{
		cardNumber = value;
		Validate(value, errorLabel);
	}

	public string City
	{
		get { return city; }
	}

	public void ReadCity(string value, Label errorLabel)
	{
		city = value;
		Validate(value, errorLabel);
	}

	public string Country
	{
		get { return country; }
	}

	public void ReadCountry(string value)
	{
		country = value;
	}

	public string Cvv
	{
		get { return cvv; }
	}

	public void ReadCvv(string value, Label errorLabel)
	{
		cvv = value;
		Validate(value, errorLabel);
	}

	public string ExpMonth
	{
		get { return expMonth; }
	}

	public void ReadExpMonthAndYear(string month, string year, Label errorLabel)
	{
		expMonth = month;
		expYear = year;
		FieldHasMissingData(expMonth == "? " || expYear == "? ", errorLabel);
	}

	public string ExpYear
	{
		get { return expYear; }
	}

	public string FirstName
	{
		get { return firstName; }
	}

	public void ReadFirstName(string value, Label errorLabel)
	{
		firstName = value;
		Validate(value, errorLabel);
	}

	public string Last4Digits
	{
		get
		{
			if (cardNumber.Length >= 4)
				return cardNumber.Substring(cardNumber.Length - 4, 4);
			else
				return cardNumber;
		}
	}

	public string LastName
	{
		get { return lastName; }
	}

	public void ReadLastName(string value, Label errorLabel)
	{
		lastName = value;
		Validate(value, errorLabel);
	}

	public string State
	{
		get { return state; }
	}

	public void ReadState(string value, Label errorLabel)
	{
		state = value;
		FieldHasMissingData((state == "? " || state == "XX") && CountryIsUsOrCa, errorLabel);
	}

	public string Zip
	{
		get { return zip; }
	}

	public void ReadZip(string value, Label errorLabel)
	{
		zip = value;
		FieldHasMissingData(value.Trim().Length == 0 && CountryIsUsOrCa, errorLabel);
	}

	public bool HasMissingFields
	{
		get { return hasMissingData; }
	}

	private bool CountryIsUsOrCa
	{
		get { return country == "US" || country == "CA"; }
	}

	private void FieldHasMissingData(bool missing, Label errorLabel)
	{
		if (missing)
		{
			hasMissingData = true;
			errorLabel.Text = "*";
		}
		else
		{
			errorLabel.Text = string.Empty;
		}
	}

	private void Validate(string value, Label errorLabel)
	{
		FieldHasMissingData(value.Trim().Length == 0, errorLabel);
	}
}
