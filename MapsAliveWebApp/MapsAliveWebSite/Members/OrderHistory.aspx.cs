// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;

public partial class Members_OrderHistory : MemberPage
{
	protected override void PageLoad()
	{
		SetMasterPage(Master);
		SetPageTitle(Resources.Text.AccountPagePurchaseHistoryTitle);
		SetPageReadOnly();
		SetActionId(MemberPageActionId.OrderHistory);
		GetSelectedTourOrNone();

		DataTable orderTable = MapsAliveDatabase.LoadDataTable("sp_Order_GetSummaryByAccountId", "@AccountId", account.Id);

		int orderCount = 0;

		if (orderTable.Rows.Count == 0)
		{
			OrderTable.Visible = false;
		}
		else
		{
			orderCount += orderTable.Rows.Count;
			OrderTable.DataSource = orderTable;
			OrderTable.DataBind();
		}

		SetPageMessage(orderCount == 0 ? "You have not made any purchases yet" : "Your purchases are shown below");
	}

	protected void OnSelectRow(object sender, EventArgs e)
	{
		string orderId = OrderTable.SelectedRow.Cells[0].Text;
		Response.Redirect(string.Format("BuyReceipt.aspx?id={0}&review=1", orderId));
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.Account;
	}
}
