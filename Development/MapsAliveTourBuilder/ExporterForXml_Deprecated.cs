// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Xml;

// This class is preserved to provide compatibility with users of version 2 who
// are relying on the tour.xml being generated in the published tour zip file.
// The most notable customer is Resort Maps -- they have an automated system that
// reads this file and excutes web site logic based on its contents.
//
// Do not make any modifications to this class.

public class ExporterForXml_Deprecated
{
	private Tour tour;
	private XmlWriter xmlWriter;
	private ArrayList inUsecategoryList;

	public ExporterForXml_Deprecated(Tour tour)
	{
		this.tour = tour;
		inUsecategoryList = new ArrayList();
	}

	public void CreateXmlFile(string fileLocation)
	{
		XmlWriterSettings settings = new XmlWriterSettings();
		settings.Indent = true;
		settings.IndentChars = ("\t");

		MemoryStream xmlMemoryStream = new MemoryStream();

		xmlWriter = XmlWriter.Create(xmlMemoryStream, settings);
		xmlWriter.WriteStartDocument();
		CreateTourXml(xmlWriter, tour);
		xmlWriter.WriteEndDocument();
		xmlWriter.Flush();

		xmlMemoryStream.Position = 0;

		using (System.IO.FileStream fileStream = new System.IO.FileStream(fileLocation, System.IO.FileMode.Create))
		{
			xmlMemoryStream.WriteTo(fileStream);
			fileStream.Flush();
			fileStream.Close();
		}

		xmlMemoryStream.Close();
	}

	private void CreateTourXml(XmlWriter xmlWriter, Tour tour)
	{
		xmlWriter.WriteStartElement("tour");
		EmitTourAttributes();


		EmitElement("name", tour.Name);
		EmitElement("browserTitle", tour.BrowserTitle);

		xmlWriter.WriteStartElement("pages");
		foreach (TourPage tourPage in tour.TourPages)
		{
			xmlWriter.WriteStartElement("page");
			
			EmitTourPageAttributes(tourPage);

			EmitElement("mapId", tourPage.PageId);
			EmitElement("pageNumber", tourPage.PageNumber.ToString());
			EmitElement("name", tourPage.Name);
			EmitElement("title", tourPage.Title);

			xmlWriter.WriteStartElement("slides");
			foreach (TourView tourView in tourPage.TourViews)
			{
				EmitSlide(tourView);
			}
			xmlWriter.WriteEndElement(); // slides
			xmlWriter.WriteEndElement(); // page
		}
		
		xmlWriter.WriteEndElement(); // pages
		
		EmitEnums();
		
		EmitTourCategories();
		
		xmlWriter.WriteEndElement(); // tour
	}

	private void EmitEnums()
	{
		xmlWriter.WriteStartElement("types");
		xmlWriter.WriteComment("Type names are subject to change. Rely only on type values.");
		
		EmitEnumSlideMediaType();
		EmitEnumCategoryType();

		xmlWriter.WriteEndElement(); // types
	}

	private void EmitEnumCategoryType()
	{
		xmlWriter.WriteStartElement("category");
		CategoryType[] types = (CategoryType[])Enum.GetValues(typeof(CategoryType));
		foreach (CategoryType type in types)
		{
			xmlWriter.WriteStartElement("type");
			xmlWriter.WriteAttributeString("name", type.ToString());
			xmlWriter.WriteAttributeString("value", ((int)type).ToString());
			xmlWriter.WriteEndElement(); // type
		}
		xmlWriter.WriteEndElement(); // category
	}

	private void EmitEnumSlideMediaType()
	{
		xmlWriter.WriteStartElement("slideMedia");
		SlideMediaType[] types = (SlideMediaType[]) Enum.GetValues(typeof(SlideMediaType));
		foreach (SlideMediaType type in types)
		{
			xmlWriter.WriteStartElement("type");
			xmlWriter.WriteAttributeString("name", type.ToString());
			xmlWriter.WriteAttributeString("value", ((int)type).ToString());
			xmlWriter.WriteEndElement(); // type
		}
		xmlWriter.WriteEndElement(); // slideMedia
	}

	private void EmitTourAttributes()
	{
		xmlWriter.WriteAttributeString("id", tour.Id.ToString());
		xmlWriter.WriteAttributeString("url", tour.Url);

		// Use the current time as the publish date since the tour's publish date
		// is from the last time the tour was published. The new date won't get set
		// until after this export completes. Ideally, the date should be passed here
		// from the tour builder so that the same date is used here in the tour itself.
		DateTime now = DateTime.Now;
		string published = now.ToShortDateString() + " " + now.ToShortTimeString();
		xmlWriter.WriteAttributeString("published", published);
	}

	private void EmitTourCategories()
	{
		xmlWriter.WriteStartElement("categories");

		//string value = string.Empty;

		foreach (Category category in inUsecategoryList)
		{
			xmlWriter.WriteStartElement("category");
			xmlWriter.WriteAttributeString("id", category.Id.ToString());
			xmlWriter.WriteAttributeString("type", ((int)category.Type).ToString());
			xmlWriter.WriteAttributeString("code", category.Code);
			xmlWriter.WriteAttributeString("title", category.Title);
			xmlWriter.WriteEndElement(); // category
		}

		xmlWriter.WriteEndElement(); // categories
	}

	private void EmitTourPageAttributes(TourPage tourPage)
	{
		xmlWriter.WriteAttributeString("id", tourPage.Id.ToString());
	}

	private void EmitSlide(TourView tourView)
	{
		xmlWriter.WriteStartElement("slide");
		EmitSlideAttributes(tourView);

		EmitElement("slideId", tourView.SlideId);
		EmitElement("title", tourView.Title);

		if (tourView.MarkerClickAction == MarkerAction.LinkToUrl || tourView.MarkerClickAction == MarkerAction.LinkToUrlNewWindow)
		{
			EmitElement("clickAction", tourView.MarkerClickAction.ToString());
			EmitElement("clickActionTarget", tourView.MarkerClickActionTarget);
		}

		if (tourView.TourPage.ActiveSlideLayout.HasImageArea && tourView.HasImage)
			EmitElement("imageFileName", tourView.Image.FileNameInternal);

		// This is an experiment for writing binary data to the XML file.
		//if (tourView.HasImageArea)
		//{
		//    TourImage image = tourView.Image;
		//    Size imageSize = Utility.ScaledImageSize(image.Size, tourPage.ActiveSlideLayout.ImageArea.Size);
		//    byte[] bytes = Utility.ScaledImageBytes(image.Bytes, imageSize);
		//    xmlWriter.WriteStartElement("image");
		//    xmlWriter.WriteBase64(bytes, 0, bytes.Length);
		//    xmlWriter.WriteEndElement();
		//}

		EmitSlideMedia(tourView);
		EmitSlideText(tourView);
		EmitSlideCategories(tourView);
		EmitElement("notes", tourView.Notes);
		xmlWriter.WriteEndElement(); // slide
	}

	private void EmitElement(string nodeName, string value)
	{
		if (value.Length == 0)
			return;
		xmlWriter.WriteStartElement(nodeName);
		xmlWriter.WriteString(value);
		xmlWriter.WriteEndElement();
	}

	private void EmitSlideAttributes(TourView tourView)
	{
		xmlWriter.WriteAttributeString("id", tourView.Id.ToString());
	}

	private void EmitSlideCategories(TourView tourView)
	{
		ArrayList list = tour.CategoryManager.GetCategories(tourView.Id);
		if (list.Count == 0)
			return;
		
		string value = string.Empty;

		foreach (Category category in list)
		{
			if (value.Length > 0)
				value += ",";
			value += category.Code;

			if (!inUsecategoryList.Contains(category))
				inUsecategoryList.Add(category);
		}
			
		EmitElement("categories", value);
	}

	private void EmitSlideMedia(TourView tourView)
	{
		xmlWriter.WriteStartElement("media");

		Size scaledImageSize = Utility.ScaledImageSize(tourView.GetConstrainedImageSize(), tourView.GetImageContainerSize());

		SlideMediaType slideMediaType = tourView.MediaType;
		xmlWriter.WriteAttributeString("type", ((int)slideMediaType).ToString());

		if (slideMediaType == SlideMediaType.Photo)
		{
			xmlWriter.WriteAttributeString("width", scaledImageSize.Width.ToString());
			xmlWriter.WriteAttributeString("height", scaledImageSize.Height.ToString());
		}
		else
		{
			if (slideMediaType == SlideMediaType.Embed)
				xmlWriter.WriteAttributeString("width", tourView.EmbedWidth.ToString());

			string media = tourView.EmbedText;
			if (media.Trim().Length > 0)
				xmlWriter.WriteString(media);
		}
	
		xmlWriter.WriteEndElement(); // media
	}

	private void EmitSlideText(TourView tourView)
	{
		xmlWriter.WriteStartElement("text");
		
		string text = tourView.DescriptionText;
		if (text.Trim().Length > 0)
			xmlWriter.WriteString(tourView.DescriptionHtml);
		
		xmlWriter.WriteEndElement(); // text
	}
}
