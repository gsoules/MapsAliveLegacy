// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Xml.XPath;

class MapsAliveDataRecordXml : MapsAliveDataRecord
{
	XPathNavigator navigator;
	XPathNavigator node;

	public MapsAliveDataRecordXml(XPathNavigator navigator)
	{
		this.navigator = navigator;
	}

	private string GetV3NameForNode(string v4Name)
    {
		switch(v4Name)
        {
			//	  V4 Name							    V3 Name
			case "layoutAreaBackgroundColor":	return "canvasBackgroundColor";
			case "layoutAreaHeight":			return "canvasHeight";
			case "layoutAreaWidth":				return "canvasWidth";
			case "layoutAreaSplitterH":			return "canvasSplitterH";
			case "layoutAreaSplitterV":			return "canvasSplitterV";
			case "layoutAreaSplitterLockedH":	return "canvasSplitterLockedH";
			case "layoutAreaSplitterLockedV":	return "canvasSplitterLockedV";
			case "layoutAreaMarginTop":			return "canvasMarginTop";
			case "layoutAreaMarginRight":		return "canvasMarginRight";
			case "layoutAreaMarginBottom":		return "canvasMarginBottom";
			case "layoutAreaMarginLeft":		return "canvasMarginLeft";
			case "layoutAreaSpacingH":			return "canvasSpacingH";
			case "layoutAreaSpacingV":			return "canvasSpacingV";
			case "layoutAreaTemplateId":		return "canvasTemplateId";
		}

		return "";
    }

	private void GetTagNode<T>(T tag)
	{
		node = navigator.SelectSingleNode(tag.ToString());
		
		if (node == null)
        {
			string v3Name = GetV3NameForNode(tag.ToString());
			if (v3Name.Length > 0)
            {
				// The node had a different name in V3 and this XML contains a node with the V3 name.
				// Therefore, this must be V3 XML being imported into V4. Fetch the value of the V3 node
				// and return it for the value of the V4 name.
				node = navigator.SelectSingleNode(v3Name);
            }
		}
	}

	public override bool BoolValue<T>(T tag)
	{
		GetTagNode(tag);
		if (node == null)
			return false;
		else
			return node.Value.ToLower() == "true";
	}

	public override string ColorValue<T>(T tag)
	{
		GetTagNode(tag);
		if (node == null)
			return "#ffffff";
		else
			return node.Value;
	}

	public override double DoubleValue<T>(T tag)
	{
		GetTagNode(tag);
		if (node == null)
			return 0;
		else
			return double.Parse(node.Value);
	}

	public override int IntValue<T>(T tag)
	{
		GetTagNode(tag);
		if (node == null)
			return 0;
		else
			return int.Parse(node.Value);
	}

	public override string StringValue<T>(T tag)
	{
		GetTagNode(tag);
		if (node == null)
			return string.Empty;
		else
			return node.Value;
	}
}
