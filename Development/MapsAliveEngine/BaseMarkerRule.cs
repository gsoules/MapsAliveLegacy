// Copyright (C) 2006 AvantLogic Corporation
using System;
using System.Collections.Generic;
using System.Text;

namespace AvantLogic.MapsAlive.Engine
{
	public class BaseMarkerRule
	{
		protected bool callJavascriptOnEvent;
		protected  MarkerEventType eventType;
		protected bool gotoPageOnEvent;
		protected string javascriptText;
		protected bool linkToUrlOnEvent;
		protected bool noDisplayActionOnEvent;
		protected bool showViewOnEvent;
		protected bool selectMarkerOnEvent;

		public BaseMarkerRule()
		{
		}

		public BaseMarkerRule(MarkerEventType eventType)
		{
			this.eventType = eventType;
			SetNoDisplayActionOnEvent();
		}

		#region ===== Properties ========================================================

		public bool CallJavascriptOnEvent
		{
			get { return callJavascriptOnEvent; }
			set { callJavascriptOnEvent = value; }
		}

		public bool GotoPageOnEvent
		{
			get { return gotoPageOnEvent; }
		}

		public bool LinkToUrlOnEvent
		{
			get { return linkToUrlOnEvent; }
		}

		public MarkerEventType EventType
		{
			get { return eventType; }
			set { eventType = value; }
		}

		public bool NoDisplayActionOnEvent
		{
			get { return noDisplayActionOnEvent; }
		}

		public bool SelectMarkerOnEvent
		{
			get { return selectMarkerOnEvent; }
			set { selectMarkerOnEvent = value; }
		}

		public bool ShowViewOnEvent
		{
			get	{ return showViewOnEvent;	}
		}
		#endregion

		#region ===== Public ============================================================

		public virtual string JavascriptText(int themeId)
		{
			return javascriptText;
		}

		public virtual void SetJavascriptText(int themeId, string text)
		{
			javascriptText = text;
		}

		public void SetNoDisplayActionOnEvent()
		{
			ClearEventActions();
			noDisplayActionOnEvent = true;
		}

		public void SetGotoPageOnEvent()
		{
			ClearEventActions();
			gotoPageOnEvent = true;
		}

		public void SetLinkToUrlOnEvent()
		{
			ClearEventActions();
			linkToUrlOnEvent = true;
		}

		public void SetShowViewOnEvent()
		{
			ClearEventActions();
			showViewOnEvent = true;
		}
		#endregion

		#region ===== Protected =========================================================
		#endregion

		#region ===== Private ===========================================================

		private void ClearEventActions()
		{
			noDisplayActionOnEvent = false;
			gotoPageOnEvent = false;
			linkToUrlOnEvent = false;
			showViewOnEvent = false;
		}
		#endregion
	}
}
