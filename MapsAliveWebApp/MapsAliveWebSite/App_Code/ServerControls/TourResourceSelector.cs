// Copyright (C) 2003-2010 AvantLogic Corporation
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AvantLogic.MapsAlive
{
	public class TourResourceSelector : WebControl
	{
		private Account account;
		private DataTable dataTable;
		private bool isFiltered;
		private int previewImageWidth;
		private TourResourceType resourceType;
		private HtmlTextWriter writer;

		public TourResourceSelector()
		{
			previewImageWidth = 50;
			account = MapsAliveState.Account;
		}

		public DataTable DataTable
		{
			set { dataTable = value; }
		}

		public bool IsFiltered
		{
			set { isFiltered = value; }
		}

		public int PreviewImageWidth
		{
			set { previewImageWidth = value; }
		}

		public TourResourceType ResourceType
		{
			set { resourceType = value; }
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			this.writer = writer;

			if (dataTable == null)
			{
				string sp = string.Format("sp_{0}_Get{0}sOwnedByAccount", resourceType);
				dataTable = MapsAliveDatabase.LoadDataTable(sp, "AccountId", Utility.AccountId);
			}

			if (dataTable.Rows.Count > 0)
			{
				RenderRows();
			}
			else
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "finePrintHelp");
				writer.RenderBeginTag(HtmlTextWriterTag.Div);
				
				string text = "&nbsp;";
				string description = TourResourceManager.GetTitlePlural(resourceType);
				if (isFiltered)
					text += string.Format("This tour is not using any {0}", description);
				else
					text += string.Format("You have not added any {0} to your account yet", description);

				writer.Write(text);
				writer.RenderEndTag();
			}
		}

		private void RenderRows()
		{
           string classes = "resourceCollection";
            if (resourceType == TourResourceType.Category)
            {
                classes += " resourceDetails";
            }
            else
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "resourceViewToggle");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                string href = "javascript:maToggleResourceView();";
                writer.AddAttribute(HtmlTextWriterAttribute.Id, "ResourceViewToggle", false);
                writer.AddAttribute(HtmlTextWriterAttribute.Href, href, false);
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write("Show Details View");
                writer.RenderEndTag();
                writer.RenderEndTag();

                classes += " resourceGrid";
            }

            if (resourceType == TourResourceType.Marker ||
                resourceType == TourResourceType.MarkerStyle ||
                resourceType == TourResourceType.TooltipStyle ||
                resourceType == TourResourceType.FontStyle)
                classes += " resourceGridThreeColumns";

            writer.AddAttribute(HtmlTextWriterAttribute.Class, classes);
            writer.AddAttribute(HtmlTextWriterAttribute.Id, "Resources", false);
			
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            foreach (DataRow dataRow in dataTable.Rows)
			{
				MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
				RenderRow(row);
			}

			writer.RenderEndTag();
		}
		
		private void RenderRow(MapsAliveDataRow row)
        {
            int id = ResourceId(row);

            if (resourceType == TourResourceType.Symbol && id == 0)
            {
                // Don't show the "No Symbol" symbol.
                return;
            }

            // See if this is the default resource (but don't show a default symbol since the default is not used for anything).
            bool isDefault = id == account.DefaultResourceId(resourceType) && resourceType != TourResourceType.Symbol;

            if (TourResourceManager.HasResourceCode(resourceType))
            {
                AddClassNameAndMouseOverAttributes("resourceItem");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                RenderUsage(id);

                CategoryType categoryType = (CategoryType)row.IntValue("Type");
                int directoryPosition = row.IntValue("DirectoryPosition");
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "resourceItemOrder");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(categoryType == CategoryType.DirectoryGroup ? directoryPosition.ToString() : "");
                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, TourResourceManager.ClickScript(resourceType, id));
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "resourceItemNameCategory");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(row.StringValue("Title"));
                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "resourceItemCode");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(row.StringValue("Code"));
                writer.RenderEndTag();
            }
            else
            {
                AddClassNameAndMouseOverAttributes("resourceItem");

                writer.AddAttribute(HtmlTextWriterAttribute.Onclick, TourResourceManager.ClickScript(resourceType, id));
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                RenderUsage(id);

                if (TourResourceManager.HasResourceImageUrl(resourceType))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "resourceItemImage");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    string imageUrl = TourResource.ResourceImageUrl(resourceType, id, row.StringValue("ResourceImageId"));

                    writer.AddAttribute(HtmlTextWriterAttribute.Src, imageUrl);
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }

                string className = isDefault ? "resourceItemName resourceItemDefault" : "resourceItemName";
                writer.AddAttribute(HtmlTextWriterAttribute.Class, className);
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(row.StringValue("Name"));
                writer.RenderEndTag();
            }

            writer.RenderEndTag();
        }

        private void RenderUsage(int id)
        {

            // Show the usage count link.
            int usageCount = Account.NumberOfResourceDependents(resourceType, id);
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "resourceItemUsage");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            if (usageCount > 0)
            {
                string targetPage = MemberPageAction.ActionPageTarget(MemberPageActionId.TourResourceDependencyReport);
                string queryString = string.Format("?rt={0}&id={1}", (int)resourceType, id);
                writer.AddAttribute(HtmlTextWriterAttribute.Href, targetPage + queryString);
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write(usageCount.ToString() + " use" + (usageCount == 1 ? "" : "s"));
                writer.RenderEndTag();
            }
            else
            {
                writer.Write("Not used");
            }
            writer.RenderEndTag();
        }

        private void AddClassNameAndMouseOverAttributes(string className)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Class, className);
			writer.AddAttribute("onmouseover", string.Format("this.className='{0}Hover'", className));
			writer.AddAttribute("onmouseout", string.Format("this.className='{0}'", className));
		}

		private int ResourceId(MapsAliveDataRow row)
		{
			return row.IntValue(string.Format("{0}Id", resourceType));
		}
	}
}
