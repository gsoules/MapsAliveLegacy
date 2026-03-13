// Copyright (C) 2003-2010 AvantLogic Corporation
using System.Diagnostics;

// This code was derived from an article: http://www.eggheadcafe.com/articles/20030402.asp
// We use it to add our own listener so that we'll trap a failed assertion. If we don't do
// this, the assert message is simply written to the Output window since message boxes don't
// display on servers.		

public class AssertHandler : DefaultTraceListener
{
	public override void Fail(string assertDescription)
	{
		Utility.WriteErrorToLogFile("Assert Failed: " + assertDescription);
	}
}
