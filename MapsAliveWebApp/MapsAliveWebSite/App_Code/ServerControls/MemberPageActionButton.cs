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

namespace AvantLogic.MapsAlive
{
	public class MemberPageActionButton : WebControl
	{
		private bool appearsEnabled;
		private string errorMessage;
		private bool important;
		private MemberPageActionId onClickActionId;
		private string onClickJavaScript;
		private string queryString;
		private bool saveWithoutTransferring;
		private bool subtle;
		private string title;
		private bool transferWithoutSaving;
		private bool veryImportant;
		private string warningMessage;

		public MemberPageActionButton()
		{
			queryString = string.Empty;
			appearsEnabled = true;
			warningMessage = string.Empty;
		}

		public bool AppearsEnabled
		{
			// Note that this property is called AppearsEnabled instead of Enabled to avoid
			// confusion/conflict with the WebControl Enabled property. Also, when this property
			// is set to false, the actual control is still enabled, but appears disabled.
			get	{ return appearsEnabled; }
			set	{ appearsEnabled = value; }
		}

		public string ErrorMessage
		{
			set	{ errorMessage = value.Replace("'", "\\'");	}
		}

		public bool Important
		{
			set { important = value; }
		}

		public bool Subtle
		{
			set { subtle = value; }
		}

		public MemberPageActionId OnClickActionId
		{
			set { onClickActionId = value; }
		}

		public string OnClickJavaScript
		{
			set { onClickJavaScript = value; }
		}

		public string QueryString
		{
			set { queryString = value; }
		}

		public bool SaveWithoutTransferring
		{
			set { saveWithoutTransferring = value; }
		}

		public string Title
		{
			set { title = value; }
		}

		public bool TransferWithoutSaving
		{
			set { transferWithoutSaving = value; }
		}

		public bool VeryImportant
		{
			set { veryImportant = value; }
		}

		public string WarningMessage
		{
			set { warningMessage = value.Replace("'", "\\'"); }
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			string fcn;
			
			if (saveWithoutTransferring)
				fcn = "maOnEventSave";
			else if (transferWithoutSaving)
				fcn = "maSafeTransfer";
			else
				fcn = "maOnEventSaveAndTransfer";

			string onClickScript = string.Empty;

			if (errorMessage != null)
			{
				// The error message is set so only show the alert and not let the action execute.
				onClickScript = "maAlert('" + errorMessage + "');";
			}
			else if (onClickJavaScript != null)
			{
				// If a JavaScript click hander is provided, use it as the action.
				onClickScript = onClickJavaScript;
			}
			else
			{
				// Create a click hander using the action id provided.
				if (onClickActionId != MemberPageActionId._NotSet)
				{
					string targetPage = MemberPageAction.ActionPageTarget(onClickActionId) + queryString;
					onClickScript = fcn + "('/Members/" + targetPage + "');";
				}
			}

			if (warningMessage != string.Empty)
			{
				// A warning message is set, so wrap the script to be executed within a confirm.
				onClickScript = string.Format("maConfirmAndExecuteScript(`{0}`,`{1}`);", warningMessage, onClickScript);
			}
			
			string className;
			if (appearsEnabled)
			{
				if (important)
					className = "pageActionControl2";
				else if (veryImportant)
					className = "pageActionControl3";
				else if (subtle)
					className = "pageActionControl4";
				else
					className = "pageActionControl";
			}
			else
			{
				className = "pageActionControlDisabled";
			}
			
			writer.AddAttribute(HtmlTextWriterAttribute.Class, className);
			
			if (appearsEnabled)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Onclick, onClickScript);
				writer.AddAttribute("onmouseover", string.Format("this.className='{0}Over';", className));
				writer.AddAttribute("onmouseout", string.Format("this.className='{0}';", className));
			}
			
			writer.RenderBeginTag(HtmlTextWriterTag.Span);
			writer.Write(title);
			writer.RenderEndTag();
		}

		protected override HtmlTextWriterTag TagKey
		{
			get { return HtmlTextWriterTag.Span; }
		}
	}
}
