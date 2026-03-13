// Copyright (C) 2006-2009 AvantLogic Corporation

namespace AvantLogic.MapsAlive.Engine
{
	public class BaseMarkerDefinition
	{
		protected int id;
		protected BaseMarkerRuleSet markerRuleSet;
		protected string name;
		protected BaseMarkerAppearance normalAppearance;
		protected BaseMarkerAppearance rolloverAppearance;
		protected BaseMarkerAppearance selectedAppearance;

		public BaseMarkerDefinition(int id, string name, BaseMarkerRuleSet markerRuleSet)
		{
			this.id = id;
			this.name = name;
			this.markerRuleSet = markerRuleSet;
			selectedAppearance = new BaseMarkerAppearance();
			normalAppearance = new BaseMarkerAppearance();
			rolloverAppearance = new BaseMarkerAppearance();
		}

		public virtual BaseMarkerRule ClickRule
		{
			get { return BaseMarkerRuleSet.BaseClickRule; }
		}

		public BaseMarkerRuleSet BaseMarkerRuleSet
		{
			get { return markerRuleSet; }
			set { markerRuleSet = value; }
		}

		public BaseMarkerAppearance BaseNormalAppearance
		{
			get { return normalAppearance; }
			set { normalAppearance = value; }
		}

		public BaseMarkerAppearance BaseSelectedAppearance
		{
			get { return selectedAppearance; }
			set { selectedAppearance = value; }
		}

		public int Id
		{
			get { return id; }
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public BaseMarkerAppearance NormalAppearance
		{
			get { return normalAppearance; }
		}

		public BaseMarkerAppearance RolloverAppearance
		{
		    get { return rolloverAppearance; }
		}

		public BaseMarkerAppearance SelectedAppearance
		{
			get { return selectedAppearance; }
		}

		public virtual bool ShowViewOnClickEvent
		{
			get { return BaseMarkerRuleSet.BaseClickRule.ShowViewOnEvent; }
		}

		public virtual bool ShowViewOnMouseEnterEvent
		{
			get { return BaseMarkerRuleSet.BaseMouseEnterRule.ShowViewOnEvent; }
		}

		public BaseShape HotspotShape()
		{
			if (normalAppearance.BaseShape != null)
				return normalAppearance.BaseShape;

			if (normalAppearance.BaseSymbol != null)
				return new BaseShape(normalAppearance.BaseSymbol);
			else if (selectedAppearance.BaseSymbol != null)
				return new BaseShape(selectedAppearance.BaseSymbol);

			System.Diagnostics.Debug.Fail("No normal shape or symbol was defined");
			return null;
		}
	}
}
