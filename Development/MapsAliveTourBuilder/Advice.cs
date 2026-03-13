// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;

public enum PageSolution
{
	AddSlide,
	DisableMapZoom,
	EnableMapZoom,
	GoToMap,
	RenameMap,
	RenameGallery,
	UploadMapImage
}

public enum SlideSolution
{
	GoToMap,
	RenameSlide,
	RenameDataSheet,
	UploadSlidePhoto
}

public abstract class Advice
{
	protected bool advise;
	private int level;
	private string additionalInfo;
	protected ArrayList affectedSlides;
	protected AdviceSet adviceSet;
	protected ArrayList pageSolutions;
	protected ArrayList slideSolutions;
	private string titleSingle;
	private string titlePlural;
	protected string html;
	protected int tourBuilderActionId;

	public Advice(AdviceSet adviceSet)
	{
		this.adviceSet = adviceSet;
		affectedSlides = new ArrayList();
		pageSolutions = new ArrayList();
		slideSolutions = new ArrayList();
	}

	public void Add()
	{
		advise = true;
		adviceSet.AdviceAdded(level);
	}

	public void AddAffectedSlide(TourView tourView)
	{
		if (affectedSlides.Count >= adviceSet.MaxMessagesPerAdviceSet)
		{
			adviceSet.AdviceNotAdded();
			return;
		}
		adviceSet.AdviceAdded(level);
		affectedSlides.Add(tourView);
		advise = true;
	}

	public void AddSolution(PageSolution solution)
	{
		pageSolutions.Add(solution);
	}

	public void AddSolution(SlideSolution solution)
	{
		slideSolutions.Add(solution);
	}

	public ArrayList AffectedSlides
	{
		get { return affectedSlides; }
	}

	public string AdditionalInfo
	{
		get
		{
			if (String.IsNullOrEmpty(additionalInfo))
				return string.Empty;
			return string.Format("<span class='maAdditionalInfo'> ({0})</span>", additionalInfo);
		}
	}

	public string Title
	{
		get { return affectedSlides.Count <= 1 ? titleSingle : titlePlural; }
	}

	protected void EmitSolutionLinks(StringBuilder html, TourPage tourPage)
	{
		foreach (PageSolution solution in pageSolutions)
			EmitPageSolution(html, solution, tourPage);
	}

	protected void EmitSolutionLinks(StringBuilder html, TourView tourView)
	{
		foreach (SlideSolution solution in slideSolutions)
			EmitSlideSolution(html, solution, tourView);
	}

	public void EmitAdviceAndSolutions(StringBuilder html, int tourBuilderActionId)
	{
		if (!advise)
			return;

		bool hasAffectedSlides = affectedSlides.Count > 0;

		this.tourBuilderActionId = tourBuilderActionId;

		html.Append("<tr>");

		if (hasAffectedSlides)
		{
			html.Append(string.Format("<td colspan='2' class='maAdviceLevel{0}'>{1}{2}:</td>", level, Title, AdditionalInfo));
		}
		else
		{
			html.Append(string.Format("<td colspan='2' class='maAdviceLevel{0}'>{1}{2}", level, Title, AdditionalInfo));
			EmitSolutionLinks(html, adviceSet.TourPage);
			html.Append("</td>");
		}

		html.Append("</tr>");

		if (hasAffectedSlides)
		{
			html.Append("<tr><td colspan='2'><table>");
			foreach (TourView tourView in affectedSlides)
			{
				html.Append("<tr>");
				html.Append("<td class='maSlideName'>");
				html.Append(tourView.Title);
				html.Append("</td>");
				html.Append("<td>");
				EmitSolutionLinks(html, tourView);
				html.Append("</td>");
				html.Append("</tr>");
			}
			html.Append("</table></td></tr>");
		}
	}

	public void SetAdditionalInfo(string info)
	{
		this.additionalInfo = info;
	}

	public void SetSeverity(int level)
	{
		this.level = level;
	}

	public void SetTitle(string title)
	{
		titleSingle = title;
	}

	public void SetTitle(string single, string plural)
	{
		titleSingle = single;
		titlePlural = plural;
	}
	
	protected void EmitSolution(StringBuilder html, string text)
	{
		html.Append(string.Format("<span class='maPreviewAction'>{0}</span>", text));
	}

	protected void EmitPageSolution(StringBuilder html, PageSolution solution, TourPage tourPage)
	{
		string text = string.Empty;

		switch (solution)
		{
			case PageSolution.AddSlide:
				text = EmitSolutionLink("Add Hotspot", MemberPageActionId.AddHotspot, tourPage);
				break;

			case PageSolution.DisableMapZoom:
				text = EmitSolutionLink("Disable MapZoom", MemberPageActionId.DisableMapZoom, tourPage);
				break;

			case PageSolution.EnableMapZoom:
				text = EmitSolutionLink("Enable MapZoom", MemberPageActionId.EnableMapZoom, tourPage);
				break;
			
			case PageSolution.GoToMap:
				text = EmitSolutionLink("Go To Map", MemberPageActionId.Map, tourPage);
				break;

			case PageSolution.RenameMap:
				text = EmitSolutionLink("Rename", MemberPageActionId.MapSetup, tourPage);
				break;

			case PageSolution.RenameGallery:
				text = EmitSolutionLink("Rename", MemberPageActionId.GallerySetup, tourPage);
				break;
			
			case PageSolution.UploadMapImage:
				text = EmitSolutionLink("Choose Map Image", MemberPageActionId.UploadMap, tourPage);
				break;

			default:
				Debug.Fail("No page solution implemented for " + solution.ToString());
				break;
		}

		EmitSolution(html, text);
	}

	protected void EmitSlideSolution(StringBuilder html, SlideSolution solution, TourView tourView)
	{
		string text = string.Empty;

		switch (solution)
		{
			case SlideSolution.GoToMap:
				text = EmitSolutionLink("Go To Map", MemberPageActionId.Map, tourView.TourPage);
				break;

			case SlideSolution.RenameSlide:
				text = EmitSolutionLink("Rename", MemberPageActionId.EditHotspotContent, tourView);
				break;

			case SlideSolution.RenameDataSheet:
				text = EmitSolutionLink("Rename", MemberPageActionId.EditHotspotContent, tourView);
				break;

			case SlideSolution.UploadSlidePhoto:
				text = EmitSolutionLink("Upload Photo", MemberPageActionId.EditHotspotContent, tourView);
				break;
			
			default:
				Debug.Fail("No slide solution implemented for " + solution.ToString());
				break;
		}

		EmitSolution(html, text);
	}

	protected string EmitSolutionLink(string name, MemberPageActionId actionId, TourPage tourPage)
	{
		return EmitSolutionLink(name, actionId, tourPage.Id, 0);
	}

	protected string EmitSolutionLink(string name, MemberPageActionId actionId, TourView tourView)
	{
		return EmitSolutionLink(name, actionId, tourView.TourPage.Id, tourView.Id);
	}

	protected string EmitSolutionLink(string name, MemberPageActionId actionId, int tourPageId, int tourViewId)
	{
		return string.Format("<a href=\"PerformAction.ashx?aid={0}&pid={1}&vid={2}&tbaid={3}\">{4}</a>",
			(int)actionId,
			tourPageId,
			tourViewId,
			tourBuilderActionId,
			name);
	}
}
