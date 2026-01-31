// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class Members_BuyReceipt : MemberPage
{
	protected override void InitControls(bool undo)
	{
		Account account = MapsAliveState.Account;

		int orderId = 0;
		int.TryParse(Request.QueryString["id"], out orderId);
		bool isReview = Request.QueryString["review"] == "1";

		MapsAliveDataRow row = null;

		if (orderId != 0)
		{
			row = MapsAliveDatabase.LoadDataRow("sp_Order_GetOrderById",
				"@AccountId", account.Id,
				"@OrderId", orderId);
		}

		if (orderId == 0 || row == null)
		{
			if (Request.QueryString["tb"] == "1")
			{
				// Handle the case where the user was on the Buy Receipt page, then went to the
				// Home page and then back to the Tour Builder. The order Id is no longer on the
				// query string so just take them to the order history page.
				Response.Redirect("~/Members/" + MemberPageAction.ActionPageTarget(MemberPageActionId.OrderHistory));
			}
			else
			{
				Utility.TransferToHomePage();
			}
		}

		string mailTo = "mailto:support@mapsalive.com";
		EmailLink.NavigateUrl = mailTo;

		AccountNumber.Text = account.Id.ToString();
		OrderNumber.Text = orderId.ToString();
		OrderDate.Text = row.DateTimeValue("PurchaseDate").ToString();
		Purchase.Text = row.StringValue("PlanDescription");
		decimal purchasePrice = row.MoneyValue("PurchasePrice");
		bool noCharge = purchasePrice == 0.0M;
		
		string priceText;
		decimal creditAmount = row.MoneyValue("CreditAmount");
		if (noCharge)
		{
			priceText = string.Format("{0:c2}", creditAmount);
		}
		else
		{
			priceText = string.Format("{0:c2}", purchasePrice);
			if (creditAmount > 0.0M)
				priceText += string.Format(" (after {0:c2} credit applied)", creditAmount);
		}

		Price.Text = priceText;

		if (noCharge)
		{
			PaymentPanel.Visible = false;
			PaymentMessagePanel.Visible = false;
			CreditBalancePanel.Visible = true;
			CreditBalanceMessage.Text = string.Format("This order was paid from your membership credit balance.<br/>Your new credit balance is <b>{0}</b>.", account.CreditAmountString);
		}

		PaymentMethod.Text = string.Format("Credit card ending in {0}", row.StringValue("CreditCardLast4Digits"));

		Who.Text = string.Format("{0} {1}", row.StringValue("FirstName"), row.StringValue("LastName"));

		Address1.Text = row.StringValue("Address1");

		string countryCode = row.StringValue("CountryCode");
		string country = Utility.CountryName(countryCode);
		string state = string.Empty;
		if (countryCode == "US" || countryCode == "CA")
			state = string.Format(", {0}", row.StringValue("StateProvinceCode"));
		CityStateZip.Text = string.Format("{0}{1} {2}", row.StringValue("City"), state, row.StringValue("ZipPostalCode"));
		Country.Text = country;
		Email.Text = Utility.UserEmail;

		Charge.Text = string.Format("{0:c2}", purchasePrice);

		if (account.TourCount == 0 && !isReview)
			CreateFirstTourPanel.Visible = true;

		SpecialOfferId specialOfferId = (SpecialOfferId)row.IntValue("SpecialOfferId");
		if (specialOfferId != SpecialOfferId.None && !isReview)
		{
			SpecialOfferPanel.Visible = true;
			SpecialOfferMessage.Text = SpecialOffer.Instructions(specialOfferId);
		}

		string statusMessage;
		if (isReview)
			statusMessage = "This is the receipt for a previous order.";
		else
			statusMessage = "Your receipt is below. It has also been emailed to you.";
		SetPageMessage(statusMessage);
	}

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle(Resources.Text.BuyReceiptHeader);
		SetPageReadOnly();
		SetActionId(MemberPageActionId.BuyReceipt);
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Account;
	}
}
