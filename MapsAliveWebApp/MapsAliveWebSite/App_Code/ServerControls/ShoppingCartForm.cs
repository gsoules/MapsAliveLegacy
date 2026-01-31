// Copyright (C) 2003-2010 AvantLogic Corporation
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AvantLogic.MapsAlive
{
	public class ShoppingCartForm : WebControl
	{
		private string footerMessage;
		private Order order;
		private bool showProratedPrice;
		private bool subTotalEmitted;
		private HtmlTextWriter writer;

		public ShoppingCartForm()
		{
		}

		public string FooterMessage
		{
			set { footerMessage = value; }
		}

		public Order Order
		{
			set { order = value; }
		}

		public bool ShowProratedPrice
		{
			set { showProratedPrice = value; }
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			this.writer = writer;
			EmitForm();
		}

		private void EmitForm()
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "0");
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "cartGeneralTable");
			writer.RenderBeginTag(HtmlTextWriterTag.Table);

			EmitRowHeader();
			
			foreach (OrderItem item in order.Items)
			{
				if (item is OrderItemCredit)
				{
					EmitCredit(item);
					continue;
				}
				
				if (item is OrderItemDiscount)
				{
					EmitDiscount(item);
					continue;
				}

				writer.AddAttribute(HtmlTextWriterAttribute.Class, "cartRow");
				writer.RenderBeginTag(HtmlTextWriterTag.Tr);

				EmitColumnDescription(item);
				EmitColumnAnnualPrice(item);
				
				if (showProratedPrice)
					EmitColumnOrderPrice(item);

				writer.RenderEndTag();
			}

			EmitRowFooter();

			writer.RenderEndTag();
		}

		private void EmitColumnDescription(OrderItem item)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "cartRowItem");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.Write(item.ProductTitle);

			if (!string.IsNullOrEmpty(item.QuickInfoId))
			{
				writer.Write("&nbsp;");
				QuickInfo quickInfo = new QuickInfo();
				quickInfo.ID = item.QuickInfoId;
				quickInfo.RenderControl(writer);
			}

			if (!string.IsNullOrEmpty(item.Details))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "cartRowItemDetail");
				writer.RenderBeginTag(HtmlTextWriterTag.Div);
				writer.Write(item.Details);
				writer.RenderEndTag();
			}

		    writer.RenderEndTag();
		}

		private void EmitColumnAnnualPrice(OrderItem item)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Id, string.Format("{0}AnnualPrice", item.Kind.ToString()));
			writer.AddStyleAttribute(HtmlTextWriterStyle.FontWeight, "normal");
			writer.RenderBeginTag(HtmlTextWriterTag.Td);

			// bool hidePrice = item.Invalid || (item.AnnualPrice < 0 && showProratedPrice);
			// writer.Write(hidePrice ? "&nbsp;" : item.AnnualPriceString);
			writer.RenderEndTag();
		}

		private void EmitColumnOrderPrice(OrderItem item)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Id, string.Format("{0}PriceTotal", item.Kind.ToString()));
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.Write(item.Invalid ? "&nbsp;" : item.OrderPriceString);
			writer.RenderEndTag();
		}

		private void EmitCredit(OrderItem creditItem)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "cartRowSubTotal");
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);

			if (showProratedPrice)
				EmitTD("&nbsp");

			EmitTD("Sub Total:");
			EmitTD(order.SubTotalString);
			subTotalEmitted = true;

			writer.RenderEndTag();

			writer.AddAttribute(HtmlTextWriterAttribute.Class, "cartRow");
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);

			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "cartRowItemDetail");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.Write(creditItem.Details);
			writer.RenderEndTag();
			writer.RenderEndTag();
			
			if (showProratedPrice)
				EmitTD("Credit:");
			EmitTD(order.CreditStringNegative);

			writer.RenderEndTag();
		}

		private void EmitDiscount(OrderItem discountItem)
		{
			if (!subTotalEmitted)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "cartRowSubTotal");
				writer.RenderBeginTag(HtmlTextWriterTag.Tr);

				if (showProratedPrice)
					EmitTD("&nbsp");

				EmitTD("Sub Total:");
				EmitTD(order.SubTotalString);

				writer.RenderEndTag();
			}

			writer.AddAttribute(HtmlTextWriterAttribute.Class, "cartRow");
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);

			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "cartRowItemDetail");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);
			writer.Write(discountItem.Details);
			writer.RenderEndTag();
			writer.RenderEndTag();
			
			if (showProratedPrice)
				EmitTD("Discount:");
			EmitTD(order.DiscountStringNegative);

			writer.RenderEndTag();
		}

		private void EmitRowFooter()
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "cartRowBottom");
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);

			if (showProratedPrice)
			{
				writer.RenderBeginTag(HtmlTextWriterTag.Td);
				writer.RenderEndTag();
			}
			
			EmitTD("Order Total:");
			
			writer.AddAttribute(HtmlTextWriterAttribute.Id, "OrderTotal");
			EmitTD(order.TotalString);
			
			writer.RenderEndTag();
		}

		private void EmitRowHeader()
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "cartRowTop");
			writer.RenderBeginTag(HtmlTextWriterTag.Tr);
			
			EmitTD("Item");
			EmitTD("", 100);

			if (showProratedPrice)
				EmitTD("Price", 100);
			
			writer.RenderEndTag();
		}

		private void EmitTD(string text)
		{
			EmitTD(text, 0);
		}

		private void EmitTD(string text, int width)
		{
			// If there's no text emit a hard space to keep table cell formatting looking ok.
			if (text == string.Empty)
				text = "&nbsp;";
			
			if (width != 0)
			{
				writer.AddStyleAttribute(HtmlTextWriterStyle.Width, string.Format("{0}px", width));
			}
			writer.RenderBeginTag(HtmlTextWriterTag.Td);
			writer.Write(text);
			writer.RenderEndTag();
		}
	}
}
