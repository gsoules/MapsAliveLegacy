// Copyright (C) 2003-2010 AvantLogic Corporation
using System;

public class TourViewCategory
{
	private TourView tourView;
	private Category category;

	public TourViewCategory(TourView tourView, Category category)
	{
		this.tourView = tourView;
		this.category = category;
	}

	public TourView TourView
	{
		get { return tourView; }
	}

	public Category Category
	{
		get { return category; }
	}
}
