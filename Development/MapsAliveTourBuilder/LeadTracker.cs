// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.IO;
using System.Web;

public class LeadTracker
{
	public LeadTracker()
	{
	}

	public static void TrackFirstPageVisited()
	{
		TrackFirstPageVisited(string.Empty);
	}

	public static void TrackFirstPageVisited(string pageVersion)
	{
		string referrer = MapsAliveState.Referrer;
		if (referrer != null)
			return;

		try
		{
			HttpRequest request = HttpContext.Current.Request;

			string referrerUrl = request.UrlReferrer == null ? string.Empty : request.UrlReferrer.Host;

			string referrerId = string.Empty;
			string refId = request.QueryString["ref"];
			string tourId = request.QueryString["tour"];
			string gclid = request.QueryString["gclid"];

			if (refId != null)
				referrerId = refId;
			else if (tourId != null)
				referrerId = "ma_" + tourId;
			else if (gclid != null)
				referrerId = "gc_" + gclid;

			string page = Path.GetFileNameWithoutExtension(request.FilePath);
			if (pageVersion.Length > 0)
				page += ":" + pageVersion;

			MapsAliveState.Referrer = string.Format("{0:yyyyMMdd},{1},{2},{3}", DateTime.Now, page, referrerUrl, referrerId);

		}
		catch
		{
			// Make sure this code can't cause an unexpected error. We've seen it happen
			// once when someone hit the site with a referrer URL that contained HTML. The
			// exception was "Invalid URI: The URI scheme is not valid."
		}
	}
}
