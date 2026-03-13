// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;

public enum OrderKind
{
	NotSet,
	BuyPlan,
	BuyHotspots,
	UpgradePlan,
	RenewPlan,
	Payment
}

public class Order
{
	private decimal credit;
	private int days;
	private decimal discount;
	private bool invalid;
	private ArrayList items;
	private decimal payment;
	private SpecialOfferId specialOfferId;
	private decimal subTotal;
	private decimal total;
	private OrderKind kind;
	
	public Order()
	{
		items = new ArrayList();
		specialOfferId = SpecialOfferId.None;
	}

	public Order(OrderKind kind)
	{
		this.kind = kind;
		items = new ArrayList();
		specialOfferId = SpecialOfferId.None;
	}

	public decimal Credit
	{
		get { return credit; }
	}

	public string CreditString
	{
		get { return invalid ? string.Empty : PriceString(credit); }
	}

	public string CreditStringNegative
	{
		get { return invalid || credit == 0.0M ? string.Empty : string.Format("-{0}", PriceString(credit)); }
	}

	public int Days
	{
		get { return days; }
		set
		{
			days = value;
			UpdateTotal();
		}
	}

	public decimal Discount
	{
		get { return discount; }
	}

	public string DiscountString
	{
		get { return invalid ? string.Empty : PriceString(discount); }
	}

	public string DiscountStringNegative
	{
		get { return invalid || discount == 0.0M ? string.Empty : string.Format("-{0}", PriceString(discount)); }
	}

	public OrderItemHotspots ItemHotspots
	{
		get { return (OrderItemHotspots)GetItem(OrderItemKind.Hotspots); }
	}

	public OrderItemPlan ItemPlan
	{
		get { return (OrderItemPlan)GetItem(OrderItemKind.Plan); }
	}

	public ArrayList Items
	{
		get { return items; }
	}

	public OrderKind Kind
	{
		get	{ return kind;	}
	}

	public decimal Payment
	{
		get { return payment; }
	}

	public string PaymentString
	{
		get { return invalid ? string.Empty : PriceString(payment); }
	}

	public SpecialOfferId SpecialOfferId
	{
		get { return specialOfferId; }
		set { specialOfferId = value; }
	}

	public decimal SubTotal
	{
		get { return subTotal; }
	}

	public string SubTotalString
	{
		get { return invalid ? string.Empty : PriceString(subTotal); }
	}

	public decimal Total
	{
		get	{ return total;	}
	}

	public string TotalString
	{
		get	{ return invalid ? string.Empty : PriceString(total); }
	}

	public void AddItem(OrderItem item)
	{
		items.Add(item);
		UpdateTotal();
	}

	public static void AddHotspotsToOrder(Order order, int hotspotsQty)
	{
		// If there is already a hotspot item in the order, remove it.
		order.RemoveItem(OrderItemKind.Hotspots);

		if (hotspotsQty > 0)
		{
			int units = hotspotsQty / 100;
			OrderItemHotspots orderItemHotspots = new OrderItemHotspots(order, units, false);
			order.AddItem(orderItemHotspots);
		}
	}

	public static Order CreateBuyHotspotsOrder(int hotspotsQty)
	{
		Order order = new Order(OrderKind.BuyHotspots);
		AddHotspotsToOrder(order, hotspotsQty);
		return order;
	}

	public static Order CreatePaymentOrder(decimal paymentAmount)
	{
		Order order = new Order(OrderKind.Payment);
		OrderItemPayment orderItemPayment = new OrderItemPayment(order, 1);
		orderItemPayment.UnitCost = paymentAmount;
		order.AddItem(orderItemPayment);
		return order;
	}

	public static Order CreatePlanOrder(OrderKind orderKind, AccountPlan plan)
	{
		Order order = new Order(orderKind);
		OrderItemPlan itemPlan = new OrderItemPlan(order, plan, OrderItemPlan.PlanHotspotsQty(plan));
		itemPlan.UnitCost = OrderItemPlan.CalculatePlanUnitCost(plan);
		if (orderKind == OrderKind.UpgradePlan)
			itemPlan.IsFixedPrice = false;
		order.AddItem(itemPlan);

		return order;
	}

	public static Order CreatePlanUpgradeOrder(AccountPlan upgradedPlan, AccountPlan currentPlan)
	{
		Order order = CreatePlanOrder(OrderKind.UpgradePlan, upgradedPlan);
		order.ItemPlan.CalculateUpgradeUnitCost(upgradedPlan, currentPlan);
		return order;
	}

	public OrderItem GetItem(OrderItemKind kind)
	{
		foreach (OrderItem item in items)
		{
			if (item.Kind == kind)
				return item;
		}
		return null;
	}

	public bool HasItemKind(OrderItemKind kind)
	{
		OrderItem item = GetItem(kind);
		return item != null;
	}

	public void InsertFirstItem(OrderItem item)
	{
		items.Insert(0, item);
		UpdateTotal();
	}

	public static string PriceString(decimal price)
	{
		string priceString = string.Format("{0:c2}", price);
		if (price < 0)
		{
			// Convert from format with parens like ($100.00) to -$100.00.
			priceString = string.Format("-{0}", priceString.Substring(1, priceString.Length - 2));
		}
		return priceString;
	}

	public void RemoveItem(OrderItemKind kind)
	{
		OrderItem item = GetItem(kind);
		if (item != null)
			items.Remove(item);
	}

	public void UpdateTotal()
	{
		invalid = false;
		OrderItem creditItem = null;
		OrderItemDiscount discountItem = null;
		total = 0.0M;
		foreach (OrderItem item in items)
		{
			if (item.Invalid)
			{
				invalid = true;
				total = -1;
				return;
			}
			
			if (item is OrderItemCredit)
			{
				creditItem = item;
				continue;
			}

			if (item is OrderItemDiscount)
			{
				discountItem = (OrderItemDiscount)item;
				continue;
			}

			if (item is OrderItemPayment)
			{
				payment = item.OrderPrice;
			}
			
			total += item.OrderPrice;
		}

		subTotal = total;

		if (creditItem != null)
		{
			credit = Math.Min(Math.Abs(creditItem.OrderPrice), total);
			total -= credit;
			creditItem.UnitCost = -credit;
		}

		if (discountItem != null)
		{
			discount = total * ((decimal)discountItem.Percent / 100.0M);
			total -= discount;
			discountItem.UnitCost = -discount;
		}
	}
}
