// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Diagnostics;
using System.Text;

public partial class Admin_ReportAccountStatistics : MemberPage
{
	private StringBuilder report;
	private string reportElite;
	private string reportFree;
	private string reportPaid;
	private string reportExpired;
	private string reportSummary;

	private int eliteAccounts;
	private int paidAccounts;
	private int freeAccounts;
	private int paidAccountsExpired;
	private int freeAccountsExpired;
	private int eliteTours;
	private int paidTours;
	private int freeTours;
	private int paidToursExpired;
	private int freeToursExpired;
	private int eliteSlides;
	private int paidSlides;
	private int freeSlides;
	private int paidSlidesExpired;
	private int freeSlidesExpired;

	private int errorCount;

	private int totalTours;
	private int totalSlides;
	private int totalAccounts;

	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle("Account Statistics");
		SetPageReadOnly();
		SetActionId(MemberPageActionId.ReportAccountStatistics);
		GetSelectedTourOrNone();

		report = new StringBuilder();
		RunReport();
		EmitReport();
		report = report.Replace(Utility.CrLf, "<br/>");
		Statistics.Text = report.ToString();
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Account;
	}

	private int RunReport()
	{
		report.Append(string.Format("Account Statistics : {0}<br/><br/>", Utility.DateShort(DateTime.Now)));

		DataTable dataTable = MapsAliveDatabase.LoadDataTable("sp_Account_GetAll");
		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			Account account = new Account(row);
			ProcessAccount(account);
		}
		return dataTable.Rows.Count;
	}

	private void ProcessAccount(Account account)
	{
		try
		{
			if (account == null)
			{
				report.Append(" [Account is null]");
				return;
			}

			UpdateStatistics(account);
		}
		catch (Exception ex)
		{
			ReportError(string.Format("Exception in ProcessAccount {0} : {1}", account.Id, ex.Message));
		}
	}

	private string ReportRowForAccount(Account account)
	{
		string days = account.IsElite ? "*" : account.DaysActual.ToString();
		string rowText = string.Format("<br/>{0} {1} [{2}]", account.Id.ToString(), account.ContactName, days);
		if (account.CreditAmount != 0.0M)
			rowText += string.Format(" {0:c2}", account.CreditAmount);
		return rowText;
	}

	private void ReportError(string msg)
	{
		report.Append(string.Format(">>> ERROR: {0}{1<br/>}", msg));
		errorCount += 1;
	}

	private void ReportInfo(string msg)
	{
		report.Append(string.Format("* {0}{1}<br/>", msg));
	}

	private void UpdateStatistics(Account account)
	{
		int tourCount = account.CountToursInAccount();
		int slideCount = account.CountHotspotsInUse();

		totalAccounts++;
		totalTours += tourCount;
		totalSlides += slideCount;

		string reportRow = ReportRowForAccount(account);

		switch (account.Type)
		{
			case AccountType.Elite:
				reportElite += reportRow;
				eliteAccounts++;
				eliteTours += tourCount;
				eliteSlides += slideCount;
				break;

			case AccountType.Trial:
				if (account.MembershipExpired)
				{
					reportExpired += reportRow;
					freeAccountsExpired++;
					freeToursExpired += tourCount;
					freeSlidesExpired += slideCount;
				}
				else
				{
					reportFree += reportRow;
					freeAccounts++;
					freeTours += tourCount;
					freeSlides += slideCount;
				}
				break;

			case AccountType.Paid:
				if (account.MembershipExpired)
				{
					reportExpired += reportRow;
					paidAccountsExpired++;
					paidToursExpired += tourCount;
					paidSlidesExpired += slideCount;
				}
				else
				{
					reportPaid += reportRow;
					paidAccounts++;
					paidTours += tourCount;
					paidSlides += slideCount;
				}
				break;

			default:
				Debug.Fail("Unexpected account type " + account.Type);
				break;
		}
	}

	private void EmitReport()
	{
		EmitSummary();

		report.Append(
			string.Format("SUMMARY{0}<br/><br/>", reportSummary) +
			string.Format("ELITE ACCOUNTS : {0}{1}<br/><br/>", eliteAccounts, reportElite) +
			string.Format("PAID ACCOUNTS : {0}{1}<br/><br/>", paidAccounts, reportPaid) +
			string.Format("FREE ACCOUNTS : {0}{1}<br/><br/>", freeAccounts, reportFree) +
			string.Format("EXPIRED ACCOUNTS : {0}{1}<br/>", paidAccountsExpired + freeAccountsExpired, reportExpired));
	}

	private void EmitSummary()
	{
		EmitSummary(eliteAccounts, "Elite Accounts");
		EmitSummary(paidAccounts, "Paid Accounts");
		EmitSummary(freeAccounts, "Free Accounts");
		EmitSummary(paidAccountsExpired, "Paid Accounts Expired");
		EmitSummary(freeAccountsExpired, "Free Accounts Expired");
		EmitSummary(totalAccounts, "TOTAL Accounts");
		EmitBlankLine();

		EmitSummary(eliteTours, "Elite Tours");
		EmitSummary(paidTours, "Paid Tours");
		EmitSummary(freeTours, "Free Tours");
		EmitSummary(freeToursExpired, "Free Tours Expired");
		EmitSummary(paidToursExpired, "Paid Tours Expired");
		EmitSummary(totalTours, "TOTAL Tours");
		EmitBlankLine();

		EmitSummary(eliteSlides, "Elite Hotspots");
		EmitSummary(paidSlides, "Paid Hotspots");
		EmitSummary(freeSlides, "Free Hotspots");
		EmitSummary(paidSlidesExpired, "Paid Hotspots Expired");
		EmitSummary(freeSlidesExpired, "Free Hotspots Expired");
		EmitSummary(totalSlides, "TOTAL Hotspots");
	}

	private void EmitSummary(int value, string name)
	{
		reportSummary += string.Format("{2}{0} : {1}", name, value, Utility.CrLf);
	}

	private void EmitBlankLine()
	{
		reportSummary += Utility.CrLf;
	}
}
