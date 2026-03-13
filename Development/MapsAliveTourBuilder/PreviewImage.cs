// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Drawing;

public class PreviewImage
{
	private bool isFirstPage;
	private bool isInitialized;
	private Byte[] mapImageBytes;
	private int pageId;
	private string pageName;
	private string pageTitle;
	private Byte[] bytes;
	private string viewTitle;
	private string viewDescription;
	private Byte[] viewImageBytes;
	
	public PreviewImage(int pageId, string pageName, string pageTitle, bool isFirstPage)
	{
		this.pageId = pageId;
		this.pageName = pageName;
		this.pageTitle = pageTitle;
		this.isFirstPage = isFirstPage;
	}

	public bool IsFirstPage
	{
		get { return isFirstPage; }
	}

	public bool IsInitialized
	{
		get { return isInitialized; }
		set { isInitialized = value; }
	}

	public Byte[] MapImageBytes
	{
		get { return mapImageBytes; }
		set { mapImageBytes = value; }
	}

	public int PageId
	{
		get { return pageId; }
	}

	public string PageName
	{
		get { return pageName; }
	}

	public string PageTitle
	{
		get { return pageTitle; }
	}

	public Byte[] Bytes
	{
		get { return bytes; }
		set { bytes = value; }
	}

	public string ViewDescription
	{
		get { return viewDescription; }
		set { viewDescription = value; }
	}

	public string ViewTitle
	{
		get { return viewTitle; }
		set { viewTitle = value; }
	}

	public Byte[] ViewImageBytes
	{
		get { return viewImageBytes; }
		set { viewImageBytes = value; }
	}
}
