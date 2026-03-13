// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Diagnostics;

public enum OrderItemKind
{
	Renewal,
	Plan,
	Hotspots,
	Credit,
	Discount,
	Payment
}

public abstract class OrderItem
{
	protected string details;
	protected bool highlightQty;
	protected bool isFixedPrice;
	protected OrderItemKind kind;
	protected Order order;
	protected string productTitle;
	protected string quickInfoId;
	protected int units;
	protected string unitsText;
	protected decimal unitCost;
	
	public OrderItem(Order order, int units)
	{
		Debug.Assert(order != null, "An OrderItem requires an order");
		this.order = order;
		this.units = units;
		this.unitsText = units.ToString();
	}

	public decimal AnnualPrice
	{
		get { return unitCost * units; }
	}

	public virtual string AnnualPriceString
	{
		get { return Order.PriceString(AnnualPrice); }
	}

	public string Details
	{
		get { return details; }
		set { details = value; }
	}
	
	public bool HighlightQty
	{
		get { return highlightQty; }
		set { highlightQty = value; }
	}

	public bool Invalid
	{
		get { return units < 0; }
	}
	
	public bool IsFixedPrice
	{
		get { return isFixedPrice; }
		set	{ isFixedPrice = value;	}
	}

	public OrderItemKind Kind
	{
		get { return kind; }
	}

	public decimal OrderPrice
	{
		get { return isFixedPrice ? AnnualPrice : ProratedPrice(AnnualPrice, order.Days); }
	}

	public string OrderPriceString
	{
		get { return Order.PriceString(OrderPrice); }
	}

	public string ProductTitle
	{
		get { return productTitle; }
	}

	public string ProductTitlePlural()
	{
		return ProductTitlePlural(units);
	}

	public string ProductTitlePlural(int units)
	{
		// Remove the trailing "s" if only 1.
		if (units != 1 || !productTitle.EndsWith("s"))
			return productTitle;
		else
			return productTitle.Substring(0, productTitle.Length - 1);
	}

	public string QuickInfoId
	{
		get { return quickInfoId; }
	}

	public static decimal ProratedPrice(decimal price, int days)
	{
		decimal dayCost = price / 365;
		return Math.Round(dayCost * days, 2);
	}

	public decimal UnitCost
	{
		get { return unitCost; }
		set { unitCost = value; }
	}

	public string UnitCostString
	{
		get { return Order.PriceString(unitCost); }
	}

	public virtual int Units
	{
		get { return units; }
		set { units = value; }
	}

	public string UnitsText
	{
		// The purpose unitsText is to record the text version of units that a user
		// typed. It allows the shopping cart to display an error showing an
		// invalid units, i.e. text that cannot be converted to an integer. 
		get { return Invalid ? unitsText : string.Format("{0:n0}", Units); }
		set { unitsText = value; }
	}
}

public class OrderItemCredit : OrderItem
{
	public OrderItemCredit(Order order, int units) : base(order, units)
	{
		kind = OrderItemKind.Credit;
		productTitle = "Credit";
		isFixedPrice = true;
	}
}

public class OrderItemPayment : OrderItem
{
	public OrderItemPayment(Order order, int units)
		: base(order, units)
	{
		kind = OrderItemKind.Payment;
		productTitle = "Payment for Custom Services";
		isFixedPrice = true;
	}
}

public class OrderItemDiscount : OrderItem
{
	private int percent;

	public OrderItemDiscount(Order order, int units, int percent) : base(order, units)
	{
		kind = OrderItemKind.Discount;
		productTitle = "Discount";
		isFixedPrice = true;
		this.percent = percent;
	}

	public int Percent
	{
		get { return percent; }
	}
}

public class OrderItemPlan : OrderItem
{
	private AccountPlan plan;
	private int hotspotsQty;

	public OrderItemPlan(Order order, AccountPlan plan, int hotspotsQty) : base(order, 1)
	{
		this.plan = plan;
		this.hotspotsQty = hotspotsQty;
		kind = OrderItemKind.Plan;
		
		int displayedQty = hotspotsQty;
		if (plan == AccountPlan.Pro && displayedQty > OrderItemPlan.PlanHotspotsQty(AccountPlan.Pro))
			displayedQty = OrderItemPlan.PlanHotspotsQty(AccountPlan.Pro);
		
		string prefix = string.Empty;
		if (order.Kind == OrderKind.RenewPlan)
			prefix = "Renew ";
		else if (order.Kind == OrderKind.UpgradePlan)
			prefix = "Upgrade to ";
		
		productTitle = string.Format("{0}{1} with {2} Hotspots", prefix, Account.PlanDescription(plan), displayedQty);
		
		isFixedPrice = true;
		
		if (order.Kind == OrderKind.UpgradePlan)
			unitCost = CalculatePlanUnitCost(plan);
	}

	public override string AnnualPriceString
	{
		get
		{
			if (order.Kind == OrderKind.UpgradePlan)
			{
				// The unit price for an upgrade plan is the difference between the annual cost of the
				// upgraded plan minus the annual cost of the current plan. This is not what we want to
				// show the user for the 1 year cost of the plan so we return actual yearly cost instead.
				return Order.PriceString(CalculatePlanUnitCost(plan));
			}
			else
			{
				return base.AnnualPriceString;
			}
		}
	}

	public AccountPlan Plan
	{
		get { return plan; }
	}

	public int HotspotsQty
	{
		get { return hotspotsQty; }
	}

	public static decimal CalculatePlanUnitCost(AccountPlan plan)
	{
		switch (plan)
		{
			case AccountPlan.Starter:
				return 29.0M;
			
			case AccountPlan.Personal:
				return 49.0M;
			
			case AccountPlan.Plus:
				return 99.0M;
			
			case AccountPlan.Pro:
				return 199.0M;

			default:
				Debug.Fail(string.Format("Unexpected  plan: {0}", plan.ToString()));
				return 0.0M;
		}
	}

	public static decimal CalculatePlanUnitCost(AccountPlan upgradePlan, AccountPlan currentPlan)
	{
		return CalculatePlanUnitCost(upgradePlan) - CalculatePlanUnitCost(currentPlan);
	}

	public void CalculateUpgradeUnitCost(AccountPlan upgradedPlan, AccountPlan currentPlan)
	{
		unitCost = CalculatePlanUnitCost(upgradedPlan, currentPlan);
	}

	public static int PlanHotspotsQty(AccountPlan plan)
	{
		switch (plan)
		{
			case AccountPlan.Starter:
				return 20;

			case AccountPlan.Personal:
				return 50;

			case AccountPlan.Plus:
				return 125;

			case AccountPlan.Pro:
				return 300;

			default:
				Debug.Fail("Unsupported plan");
				return 0;
		}
	}
}

public class OrderItemHotspots : OrderItem
{
	public OrderItemHotspots(Order order, int units, bool firstPurchase) : base(order, units)
	{
		kind = OrderItemKind.Hotspots;
		productTitle = string.Format("{0:n0} Additional Hotspots", Quantity);
		unitCost = 20.0M;
		isFixedPrice = firstPurchase || order.Kind == OrderKind.RenewPlan;
	}

	public static int HotspotsPerUnit
	{
		get { return 100; }
	}

	public int Quantity
	{
		// 1 hotspot item unit = 100 hotspots.
		get { return QuantityFromUnits(units); }
	}

	public static int UnitsFromQuantity(int quantity)
	{
		return quantity / HotspotsPerUnit;
	}

	public static int QuantityFromUnits(int units)
	{
		return units * HotspotsPerUnit;
	}
}
