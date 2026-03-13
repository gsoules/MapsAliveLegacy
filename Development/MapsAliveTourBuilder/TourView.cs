// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Xml.XPath;

public enum SlideMediaType
{
	Photo = 0,
	Embed = 1
}

// These values are known in the database -- don't change them.
public enum ShowContentEvent
{
	OnMouseover = 0,
	OnClick = 1,
	Never = 2
}

public enum MarkerZoomType
{
	Default = 0,
	DoesNotZoom = 1,
	DoesZoom = 2
}

public partial class TourView
{
	// These flags get written to the database as a single integer value.
	// We use this bit mask approach for tracking changes so that we can
	// add new flags without having to add new colums to the TourView table
	// When you add a new flag, DO NOT CHANGE the hex value of existing
	// flags.  If you do, you will change the meaning of the flags in all
	// existing map page views in the database.
	[Flags]
	private enum ChangeFlags
	{
		Image		= 0x0001,
		Marker		= 0x0002,
		Title		= 0x0004,
		Description = 0x0008,
		Tooltip		= 0x0010,
		EmbedText	= 0x0020,
		DirPreview	= 0x0040,
		LiveData	= 0x0080
	}

	private ChangeFlags changed;
	private DateTime dateCreated;
	private DateTime dateModified;
	private string descriptionHtml;
	private string dirPreviewImageUrl;
	private string dirPreviewText;
	private string embedText;
	private int embedHeight;
	private int embedWidth;
	private bool excludeFromDirectory;
	private string title;
	private int id;
	private TourViewImage image;
	private int imageId;
	private MarkerAction markerClickAction;
	private string markerClickActionTarget;
	private MarkerZoomType markerZoomType;
	private bool markerIsDisabled;
	private bool markerIsHidden;
	private bool markerIsLocked;
	private bool markerIsNotAnchored;
	private bool markerIsStatic;
	private bool markerIsRoute;
	private MarkerAction markerRolloverAction;
	private string markerRolloverActionTarget;
	private MarkerAction markerRolloutAction;
	private string markerRolloutActionTarget;
	private int markerRotation;
	private int markerId;
	private double markerPctX;
	private double markerPctY;
	private int markerZoomThreshold;
	private string messengerFunction;
	private string notes;
	private SlideMediaType mediaType;
	private ShowContentEvent showContentEvent;
	private int sequenceNumber;
	private string slideId;
	private int slideHeightOverride;
	private int slideWidthOverride;
	private string toolTip;
	private bool touchPerformsClickAction;
	private Tour tour;
	private TourPage tourPage;
	private bool usesLiveData;

	public TourView(Tour tour, TourPage tourPage)
	{
		this.tourPage = tourPage;
		this.tour = tour;
		slideId = string.Empty;
		markerPctX = -.5;
		markerPctY = -.5;
		markerId = tourPage.IsDataSheet ? 0 : MapsAliveState.Account.LastMarkerIdSelected;
		
		markerClickAction = MarkerAction.None;
		markerClickActionTarget = string.Empty;
		markerRolloverAction = MarkerAction.None;
		markerRolloverActionTarget = string.Empty;
		markerRolloutAction = MarkerAction.None;
		markerRolloutActionTarget = string.Empty;
		showContentEvent = ShowContentEvent.OnMouseover;

        touchPerformsClickAction = true;
		
		descriptionHtml = string.Empty;
		embedText = string.Empty;
		toolTip = string.Empty;

		messengerFunction = string.Empty;
		notes = string.Empty;
		dirPreviewImageUrl = string.Empty;
		dirPreviewText = string.Empty;
	
		dateCreated = DateTime.Now;

		// Determine what sequence number to use.
		int highestSequenceNumber = 0;
		foreach (TourView tourView in tourPage.TourViews)
		{
			if (tourView.sequenceNumber > highestSequenceNumber)
				highestSequenceNumber = tourView.sequenceNumber;
		}
		sequenceNumber = highestSequenceNumber + 1;

		markerZoomType = MarkerZoomType.Default;
	}

	public TourView(Tour tour, TourPage ownerTourPage, int id)
	{
		this.tour = tour;

		// Get the view having the specified Id.  If the Id is bad, no record will come back.
		MapsAliveDataRow row = ReadTourViewRowFromDatabase(tour, id);
		if (row == null)
			return;

		// Now that we know the Id is good, we can set it.
		this.id = id;

		// Point this view to the map page it belongs to.
		if (ownerTourPage == null)
		{
			int tourPageId = row.IntValue("TourPageId");
			tourPage = tour.SetSelectedTourPage(tourPageId);
		}
		else
		{
			tourPage = ownerTourPage;
		}

		InitializeTourViewFromDataRecord(row);
	}

	public void CloneTourView(int tourViewId)
	{
		MapsAliveDataRow row = ReadTourViewRowFromDatabase(tour, tourViewId);
		if (row == null)
			return;

		InitializeTourViewFromDataRecord(row);
	}

	private static MapsAliveDataRow ReadTourViewRowFromDatabase(Tour tour, int id)
	{
		MapsAliveDataRow row = MapsAliveDatabase.LoadDataRow(
			"sp_TourView_GetTourViewByTourViewId", "@TourId", tour.Id, "@TourViewId", id, "@ThemeId", tour.ThemeId);
		return row;
	}

	public void InitializeTourViewFromDataRecord(MapsAliveDataRecord record)
	{
		bool isRow = record is MapsAliveDataRow;

		markerPctX = record.DoubleValue(Tag.markerPctX);
		markerPctY = record.DoubleValue(Tag.markerPctY);
		markerRotation = record.IntValue(Tag.markerRotation);
		markerClickAction = (MarkerAction)record.IntValue("MarkerClickAction", Tag.clickAction);
		markerClickActionTarget = record.StringValue("MarkerClickActionTarget", Tag.clickActionTarget);
		markerRolloverAction = (MarkerAction)record.IntValue("MarkerRolloverAction", Tag.mouseoverAction);
		markerRolloverActionTarget = record.StringValue("MarkerRolloverActionTarget", Tag.mouseoverActionTarget);
		markerRolloutAction = (MarkerAction)record.IntValue("MarkerRolloutAction", Tag.mouseoutAction);
		markerRolloutActionTarget = record.StringValue("MarkerRolloutActionTarget", Tag.mouseoutActionTarget);
		showContentEvent = (ShowContentEvent)record.IntValue("SelectSlideEvent", Tag.showContentWhen);
		markerIsHidden = record.BoolValue("MarkerIsInvisible", Tag.markerIsHidden);
		markerIsDisabled = record.BoolValue(Tag.markerIsDisabled);
		markerIsLocked = record.BoolValue(Tag.markerIsLocked);
		markerIsNotAnchored = record.BoolValue(Tag.markerIsNotAnchored);
		markerIsRoute = record.BoolValue(Tag.markerIsRoute);
		markerIsStatic = record.BoolValue(Tag.markerIsStatic);
		markerZoomThreshold = record.IntValue(Tag.markerZoomThreshold);
		markerZoomType = (MarkerZoomType)record.IntValue(Tag.markerZoomType);
		title = record.StringValue(Tag.title);
		slideId = record.StringValue("SlideId", Tag.hotspotId);
		descriptionHtml = record.StringValue("Description", Tag.text);
		toolTip = record.StringValue("ToolTip", Tag.toolTip);
		excludeFromDirectory = record.BoolValue(Tag.excludeFromDirectory);
		mediaType = (SlideMediaType)record.IntValue(Tag.mediaType);
		embedText = record.StringValue("EmbedText", Tag.media);
		embedWidth = record.IntValue("EmbedWidth", Tag.mediaWidth);
		embedHeight = record.IntValue("EmbedHeight", Tag.mediaHeight);
		slideWidthOverride = record.IntValue("SlideWidthOverride", Tag.popupOverrideWidth);
		slideHeightOverride = record.IntValue("SlideHeightOverride", Tag.popupOverrideHeight);

		markerId = record.IntValue(Tag.markerId);
		if (markerId == 0 && !tourPage.IsDataSheet && !markerIsRoute)
		{
			// This should never happen, but we have seen cases where another error has left the
			// database in an inconsistent state. To protect against further errors we give the
			// tour view a good marker Id.
			Debug.Fail(string.Format("Marker Id for TourView {0} in tour {1} is 0", this.id, tour.Id));
			markerId = MapsAliveState.Account.DefaultMarkerId;
		}

		if (isRow)
		{
			dateCreated = record.DateTimeValue("CreateDate");
			dateModified = record.DateTimeValue("ModifyDate");
			changed = (ChangeFlags)record.LongValue("ChangeFlags");

			// Initialize the view image from the database.
			imageId = record.IntValue("imageId");
			if (imageId > 0)
			{
				image = new TourViewImage(this, imageId);
				if (image.Id == 0)
				{
					// This logic should only execute for tours that were created prior to
					// version 2.0. We used to create an image placeholder for every slide
					// and then replaced it after the user uploaded an image. This logic
					// removes those old placeholder images.
					RemoveImageFromDatabase();
				}
			}
		}

		usesLiveData = record.BoolValue(Tag.usesLiveData);
		messengerFunction = record.StringValue(Tag.messengerFunction);
		dirPreviewText = record.StringValue(Tag.dirPreviewText);
		dirPreviewImageUrl = record.StringValue(Tag.dirPreviewImageUrl);
		notes = record.StringValue(Tag.notes);
		sequenceNumber = record.IntValue("SequenceNumber", Tag.hotspotOrder);
		touchPerformsClickAction = record.BoolValue(Tag.touchPerformsClickAction);
	}

	public enum Tag
	{
		id,
		clickAction,
		clickActionTarget,
		dirPreviewImageUrl,
		dirPreviewText,
		excludeFromDirectory,
		hotspotId,
		hotspotOrder,
		markerId,
		markerIsDisabled,
		markerIsHidden,
		markerIsLocked,
		markerIsNotAnchored,
		markerIsRoute,
		markerIsStatic,
		markerPctX,
		markerPctY,
		markerRotation,
		markerZoomThreshold,
		markerZoomType,
		mediaType,
		media,
		mediaHeight,
		mediaWidth,
		messengerFunction,
		mouseoverAction,
		mouseoverActionTarget,
		mouseoutAction,
		mouseoutActionTarget,
		notes,
		popupOverrideWidth,
		popupOverrideHeight,
		showContentWhen,
		text,
		title,
		toolTip,
		touchPerformsClickAction,
		usesLiveData
	}

	public string GetTagValue(int tagId)
	{
		Tag tag = (Tag)tagId;

		switch (tag)
		{
			case Tag.id:
				return Id.ToString();
			
			case Tag.clickAction:
				return ((int)MarkerClickAction).ToString();
			
			case Tag.clickActionTarget:
				return MarkerClickActionTarget;
			
			case Tag.dirPreviewImageUrl:
				return DirPreviewImageUrl;
			
			case Tag.dirPreviewText:
				return DirPreviewText;
			
			case Tag.excludeFromDirectory:
				return ExcludeFromDirectory.ToString();
			
			case Tag.hotspotId:
				return SlideId;

			case Tag.hotspotOrder:
				return SequenceNumber.ToString();
			
			case Tag.markerIsDisabled:
				return MarkerIsDisabled.ToString();
			
			case Tag.markerIsHidden:
				return MarkerIsHidden.ToString();

			case Tag.markerIsLocked:
				return MarkerIsLocked.ToString();

			case Tag.markerIsNotAnchored:
				return MarkerIsNotAnchored.ToString();
			
			case Tag.markerIsRoute:
				return MarkerIsRoute.ToString();
			
			case Tag.markerIsStatic:
				return MarkerIsStatic.ToString();
			
			case Tag.markerId:
				return MarkerId.ToString();
			
			case Tag.markerPctX:
				return markerPctX.ToString();
			
			case Tag.markerPctY:
				return markerPctY.ToString();
			
			case Tag.markerRotation:
				return MarkerRotation.ToString();

			case Tag.markerZoomThreshold:
				return MarkerZoomThreshold.ToString();

			case Tag.markerZoomType:
				return ((int)MarkerZoomType).ToString();
			
			case Tag.mediaType:
				return ((int)MediaType).ToString();
			
			case Tag.media:
				return EmbedText;
			
			case Tag.mediaHeight:
				return EmbedHeight.ToString();
			
			case Tag.mediaWidth:
				return EmbedWidth.ToString();
			
			case Tag.messengerFunction:
				return MessengerFunction;
			
			case Tag.mouseoverAction:
				return ((int)MarkerRolloverAction).ToString();
			
			case Tag.mouseoverActionTarget:
				return MarkerRolloverActionTarget;
			
			case Tag.mouseoutAction:
				return ((int)MarkerRolloutAction).ToString();
			
			case Tag.mouseoutActionTarget:
				return MarkerRolloutActionTarget;
			
			case Tag.notes:
				return Notes;
			
			case Tag.popupOverrideWidth:
				return SlideWidthOverride.ToString();
			
			case Tag.popupOverrideHeight:
				return SlideHeightOverride.ToString();
			
			case Tag.showContentWhen:
				return ((int)ShowContentEvent).ToString();
			
			case Tag.text:
				return DescriptionHtml;
			
			case Tag.title:
				return Title;

			case Tag.touchPerformsClickAction:
				return TouchPerformsClickAction.ToString();

			case Tag.toolTip:
				return ToolTip;
			
			case Tag.usesLiveData:
				return UsesLiveData.ToString();

			default:
				Debug.Fail("Unsupported TourDirectory XML tag requested " + tag);
				return "???";
		}
	}
	#region ===== Properties ========================================================

	public void Built()
	{
		bool imageVersionChanged = HasImage ? image.VersionChanged : false;

		// Determine if anything changed.  If not, we don't need to update the database.
		if (changed == 0 && !imageVersionChanged)
			return;

		// Clear this view's change flags.
		changed = 0;
		
		if (HasImage)
			image.Built();

		// Update the database to clear the change flags and/or update the image version.
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourView_Built",
			"@BuildId", tour.BuildId,
			"@TourViewId", id,
			"@ImageId", imageId,
			"@ThemeId", tour.ThemeId,
			"@ImageVersionBuilt", HasImage ? image.VersionBuilt : 0
		);
	}

	public string DateCreatedShort
	{
		get { return Utility.DateShort(dateCreated); }
	}

	public string DescriptionHtml
	{
		get { return descriptionHtml; }
		set
		{
			FlagAsChangedIf(descriptionHtml != value, ChangeFlags.Description);
			descriptionHtml = value;
		}
	}

	public string DescriptionText
	{
		get
		{
			// Use a temporary  RadEditor control as an Html to plain text converter.
			Telerik.Web.UI.RadEditor radEditor = new Telerik.Web.UI.RadEditor();
			radEditor.Content = DescriptionHtml;
			return radEditor.Text;
		}
	}

	public string DirPreviewImageUrl
	{
		get { return dirPreviewImageUrl; }
		set
		{
			FlagAsChangedIf(dirPreviewImageUrl != value, ChangeFlags.DirPreview);
			dirPreviewImageUrl = value;
		}
	}

	public string DirPreviewText
	{
		get { return dirPreviewText; }
		set
		{
			FlagAsChangedIf(dirPreviewText != value, ChangeFlags.DirPreview);
			dirPreviewText = value;
		}
	}

	public string EmbedText
	{
		get { return embedText; }
		set
		{
			FlagAsChangedIf(embedText != value, ChangeFlags.EmbedText);
			embedText = value;
		}
	}

	public int EmbedHeight
	{
		get { return embedHeight; }
		set { embedHeight = value; }
	}

	public int EmbedWidth
	{
		get { return embedWidth; }
		set { embedWidth = value; }
	}

	public bool ExcludeFromDirectory
	{
		get { return excludeFromDirectory; }
		set { excludeFromDirectory = value; }
	}

	public bool HasNoContent
	{
		get
		{
			bool hasContent =
				DescriptionHtml.Length > 0 ||
				HasImage ||
				(mediaType == SlideMediaType.Embed && EmbedText.Length > 0) ||
				UsesLiveData;
			return !hasContent;
		}
	}

	public bool HasImage
	{
		get { return image != null; }
	}

	public bool HasMedia
	{
		get { return HasImage || mediaType == SlideMediaType.Embed; }
	}

	public int Id
	{
		get { return id; }
	}

	public TourImage Image
	{
		get { return this.image; }
	}

	public bool ImageChanged
	{
		get { return Changed(ChangeFlags.Image); }
	}

	public bool MarkerChanged
	{
		get	{ return Changed(ChangeFlags.Marker); }
	}

	public MarkerAction MarkerClickAction
	{
		get { return markerClickAction; }
		set
		{
			FlagMarkerChangedIf(markerClickAction != value);
			markerClickAction = value;
		}
	}

	public string MarkerClickActionTarget
	{
		get { return markerClickActionTarget; }
		set
		{
			FlagMarkerChangedIf(markerClickActionTarget != value);
			markerClickActionTarget = value;
		}
	}

	public bool MarkerHasBeenPlacedOnMap
	{
		get { return markerPctX > 0 && markerPctY > 0; }
	}

	public string MarkerRolloverActionTarget
	{
		get { return markerRolloverActionTarget; }
		set
		{
			FlagMarkerChangedIf(markerRolloverActionTarget != value);
			markerRolloverActionTarget = value;
		}
	}

	public string MarkerRolloutActionTarget
	{
		get { return markerRolloutActionTarget; }
		set
		{
			FlagMarkerChangedIf(markerRolloutActionTarget != value);
			markerRolloutActionTarget = value;
		}
	}

	public int MarkerId
	{
		get { return markerId; }
		set
		{
			FlagMarkerChangedIf(markerId != value);
			markerId = value;
		}
	}

	public bool MarkerIsBound
	{
		get
		{
			if (MarkerIsRoute)
				return false;

			return Account.GetCachedMarker(markerId).IsExclusive;
		}
	}

	public bool MarkerIsDisabled
	{
		get { return markerIsDisabled; }
		set
		{
			FlagMarkerChangedIf(markerIsDisabled != value);
			markerIsDisabled = value;
		}
	}

	public bool MarkerIsHidden
	{
		get { return markerIsHidden; }
		set
		{
			FlagMarkerChangedIf(markerIsHidden != value);
			markerIsHidden = value;
		}
	}

	public bool MarkerIsLocked
	{
		get { return markerIsLocked; }
		set
		{
			FlagMarkerChangedIf(markerIsLocked != value);
			markerIsLocked = value;
		}
	}

	public bool MarkerIsNotAnchored
	{
		get { return markerIsNotAnchored; }
		set
		{
			FlagMarkerChangedIf(markerIsNotAnchored != value);
			markerIsNotAnchored = value;
		}
	}

	public bool MarkerIsRoute
	{
		get { return markerIsRoute; }
		set
		{
			FlagMarkerChangedIf(markerIsRoute != value);
			markerIsRoute = value;
		}
	}

	public bool MarkerIsStatic
	{
		get { return markerIsStatic; }
		set
		{
			FlagMarkerChangedIf(markerIsStatic != value);
			markerIsStatic = value;
		}
	}

	public double MarkerPctX
	{
		get { return markerPctX; }
		set
		{
			if (value != markerPctX)
			{
				markerPctX = value;
				FlagMarkerChanged();
			}
		}
	}

	public double MarkerPctY
	{
		get { return markerPctY; }
		set
		{
			if (value != markerPctY)
			{
				markerPctY = value;
				FlagMarkerChanged();
			}
		}
	}

	public MarkerAction MarkerRolloverAction
	{
		get { return markerRolloverAction; }
		set
		{
			FlagMarkerChangedIf(markerRolloverAction != value);
			markerRolloverAction = value;
		}
	}

	public MarkerAction MarkerRolloutAction
	{
		get { return markerRolloutAction; }
		set
		{
			FlagMarkerChangedIf(markerRolloutAction != value);
			markerRolloutAction = value;
		}
	}

	public int MarkerRotation
	{
		get { return markerRotation; }
		set
		{
			FlagMarkerChangedIf(markerRotation != value);
			markerRotation = value;
		}
	}

	public bool MarkerZooms
	{
		get
		{
			if (MarkerZoomType == MarkerZoomType.Default)
				return tourPage.MarkersZoom;
			else
				return MarkerZoomType == MarkerZoomType.DoesZoom ? true : false;
		}
	}

	public int MarkerZoomThreshold
	{
		get { return markerZoomThreshold; }
		set
		{
			FlagMarkerChangedIf(markerZoomThreshold != value);
			markerZoomThreshold = value;
		}
	}

	public MarkerZoomType MarkerZoomType
	{
		get { return markerZoomType; }
		set
		{
			FlagMarkerChangedIf(markerZoomType != value);
			markerZoomType = value;
		}
	}

	public int MarkerX
	{
		get
		{
			int width = tourPage.MapCanZoom && tourPage.MapImage.HasFile ? tourPage.MapImage.Width : tourPage.ScaledMapSize.Width;
			return Utility.PercentToPixel(markerPctX, width);
		}
		set
		{
			bool fullSize = tourPage.MapCanZoom || tourPage.ImportingMarkers;
			int width = fullSize && tourPage.MapImage.HasFile ? tourPage.MapImage.Width : tourPage.ScaledMapSize.Width;

			if (value > width)
				value = -width;
			
			double pct = Utility.PixelToPercent(value, width);
			if (pct != markerPctX)
			{
				markerPctX = pct;
				FlagMarkerChanged();
			}
		}
	}

	public int MarkerY
	{
		get
		{
			int height = tourPage.MapCanZoom && tourPage.MapImage.HasFile ? tourPage.MapImage.Height : tourPage.ScaledMapSize.Height;
			return Utility.PercentToPixel(markerPctY, height);
		}
		set 
		{
			bool fullSize = tourPage.MapCanZoom || tourPage.ImportingMarkers;
			int height = fullSize && tourPage.MapImage.HasFile ? tourPage.MapImage.Height : tourPage.ScaledMapSize.Height;

			if (value > height)
				value = -height;

			double pct = Utility.PixelToPercent(value, height);
			if (pct != markerPctY)
			{
				markerPctY = pct;
				FlagMarkerChanged();
			}
		}
	}

	public Size MediaSize
	{
		get
		{
			if (HasMedia)
				return HasImage ? Image.Size : new Size(EmbedWidth, EmbedHeight);
			else
				return Size.Empty;
		}
	}

	public SlideMediaType MediaType
	{
		get { return mediaType; }
		set { mediaType = value; }
	}

	public string MessengerFunction
	{
		get { return messengerFunction; }
		set
		{
			FlagAsChangedIf(messengerFunction != value, ChangeFlags.LiveData);
			messengerFunction = value;
		}
	}

	public string Notes
	{
		get { return notes; }
		set { notes = value; }
	}

	public ShowContentEvent ShowContentEvent
	{
		get { return showContentEvent; }
		set
		{
			FlagMarkerChangedIf(showContentEvent != value);
			showContentEvent = value;
		}
	}

	public int SequenceNumber
	{
		get { return sequenceNumber; }
		set { sequenceNumber = value; }
	}

	public string SlideId
	{
		get { return slideId == string.Empty ? id.ToString() : slideId; }
		set { slideId = value.Trim(); }
	}

	public int SlideHeightOverride
	{
		get { return slideHeightOverride; }
		set
		{
			FlagAsChangedIf(slideHeightOverride != value, ChangeFlags.Image);
			slideHeightOverride = value;
		}
	}

	public int SlideWidthOverride
	{
		get { return slideWidthOverride; }
		set
		{
			FlagAsChangedIf(slideWidthOverride != value, ChangeFlags.Image);
			slideWidthOverride = value;
		}
	}

	public string Title
	{
		get { return title; }
		set
		{
			FlagAsChangedIf(title != value, ChangeFlags.Title);
			title = value;
		}
	}

	public string TitleOrSlideId
	{
		get { return title.Length > 0 ? title : slideId; }
	}

	public string ToolTip
	{
		get { return toolTip; }
		set
		{
			FlagAsChangedIf(toolTip != value, ChangeFlags.Tooltip);
			toolTip = value;
		}
	}

	public bool TouchPerformsClickAction
	{
		get { return touchPerformsClickAction; }
		set
		{
			FlagMarkerChangedIf(touchPerformsClickAction != value);
			touchPerformsClickAction = value;
		}
	}

	public Tour Tour
	{
		get { return tour; }
	}

	public TourPage TourPage
	{
		get { return tourPage; }
	}

	public bool TourViewImageChanged
	{
		get { return TourViewImageChanged; }
	}

	public bool TourViewMarkerChanged
	{
		get { return TourViewMarkerChanged; }
	}

	public bool TourViewTextChanged
	{
		get { return TourViewTextChanged; }
	}

	public bool UsesLiveData
	{
		get { return usesLiveData; }
		set
		{
			FlagAsChangedIf(usesLiveData != value, ChangeFlags.LiveData);
			usesLiveData = value;
		}
	}
	#endregion

	#region ===== Public ============================================================

	public void Delete()
	{
		Tour tour = tourPage.Tour;

		RemoveImage();

		DeleteExclusiveMarker();
		
		if (tour.HasDirectory)
			tour.CategoryManager.TourViewDeleted(id);
		
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourView_Delete", "@TourViewId", id);
		
		tourPage.RemoveTourView(this);
		tour.RebuildTourTreeXml();
		tour.SetNoTourViewSelected();
		tourPage.TourViewChanged();

		MapsAliveState.Account.HotspotDeleted(tour);
	}

	public void DeleteExclusiveMarker()
	{
		// Remove the marker from the database.
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Marker_DeleteExclusive", "@TourViewId", id, "@MarkerId", markerId);
		
		Marker marker = Account.GetMarkerOrNull(markerId);
		if (marker != null && marker.IsExclusive)
		{
			// Remove any references to the marker.
			Account account = MapsAliveState.Account;
			if (marker.Id == account.LastResourceId(TourResourceType.Marker))
			{
				account.SetLastResourceId(TourResourceType.Marker, 0);
			}

			// Remove the marker from the cache.
			Account.DeleteCachedResource(marker);
		}
	}

	public Size GetConstrainedImageSize()
	{
		if (!HasImage)
			return Size.Empty;

		Size actualSize = Image.Size;
		Size constrainedSize = actualSize;
		
		Category category = tour.CategoryManager.GetImageOverrideCategory(id);
		if (category != null)
		{
			bool categoryOverridesWidth = category.Width > 0;
			bool categoryOverridesHeight = category.Height > 0;

			if (categoryOverridesWidth)
			{
				constrainedSize.Width = category.Width;
			}
			if (categoryOverridesHeight)
			{
				constrainedSize.Height = category.Height;
			}

			if (!(categoryOverridesWidth && categoryOverridesHeight))
				constrainedSize = Utility.ScaledImageSize(actualSize, constrainedSize);
		}

		return constrainedSize;
	}

	public Size GetImageContainerSize()
	{
		LayoutManager layoutManager = tourPage.LayoutManager;
		SlideLayout slideLayout = tourPage.ActiveSlideLayout;

		int width = slideLayout.ImageArea.Width;
		int height = slideLayout.ImageArea.Height;

		bool widthSetByCategory = false;
		bool heightSetByCategory = false;
		
		Size maxSize = layoutManager.ImageAreaMaxSize;

		// Determine if this slide's media area size is overridden by a category.
		Category category = tour.CategoryManager.GetImageOverrideCategory(id);
		if (category != null)
		{
			if (category.Width > 0)
			{
				widthSetByCategory = true;
				if (category.Width <= maxSize.Width)
				{
					// Prevent the image from extending beyond the width of its container.
					width = category.Width;
				}
			}

			if (category.Height > 0)
			{
				heightSetByCategory = true;
				if (category.Height <= maxSize.Height)
				{
					// Prevent the image from extending beyond the height of its container.
					height = category.Height;
				}
			}
		}

		// Determine if the media area can grow into the text area.
		if (!widthSetByCategory)
		{
			// The media area can grow into the text except for layouts where the media and
			// text are side by side. In those layouts, the text will move toward the image
			// if the image is narrow, but a wide image will never make the text narrower.
			if (!SlideLayout.HasSideBySideTextAndImage(slideLayout.Pattern))
			{
				width = maxSize.Width;
			}
		}

		if (!heightSetByCategory)
		{
			height = maxSize.Height;
		}

		// Determine if this slide's overall size is overridden. A positive delta means this slide is
		// bigger than others. Negative means it's smaller. Apply the delta to the image area (the
		// override allows the user to grow or shrink the image while leaving the text area size
		// unaffected). If the delta is large and negative, the image area can be eliminated, but make
		// sure it does not go below zero.
		if (slideWidthOverride > 0)
		{
			int deltaW = slideWidthOverride - slideLayout.OuterSize.Width;
			width += deltaW;
			if (width < 0)
				width = slideWidthOverride;
		}
		if (slideHeightOverride > 0)
		{
			int deltaH = slideHeightOverride - slideLayout.OuterSize.Height;
			height += deltaH;
			if (height < 0)
				height = slideHeightOverride;
		}

		// Check for negative dimensions caused by overly large margins.
		if (width <= 0 || height < 0)
			return Size.Empty;

        // Enlarge the container to the minimum size for V4 view images.
        if (tour.V4)
        {
            int minW = 600;
            int minH = 900;
            if (width < minW)
                width = minW;
            if (height < minH)
                height = minH;
        }

		return new Size(width, height);
	}

	public bool HasCategory(Category category)
	{
		return category != null && tour.CategoryManager.TourViewHasCategory(id, category.Id);
	}

	public bool HasCategory(string categoryCode)
	{
		return tour.CategoryManager.TourViewHasCategory(id, categoryCode);
	}

	public bool HasChangedSinceLastBuilt()
	{
		return changed != 0;
	}

	public void ImageUploaded(string fileName, Size size, Byte[] bytes)
	{
		ImageUploaded(fileName, size, bytes, 0);
	}

	public void ImageUploaded(string fileName, Size size, Byte[] bytes, int sampleImageId)
	{
		bool firstImage = image == null;

		if (firstImage)
			CreateImage();

		image.ReadyMapPackageId = sampleImageId;
		image.Uploaded(tour.Id, fileName, size, bytes);

		if (tourPage.FirstTourViewId == this.id)
			tourPage.InvalidateThumbnail();

		SetImageChanged();

		image.KeepUploadedFile(tour.Id);
	}

	public static TourView ImportSlide(string slideId, string title, int markerId, TourPage tourPage)
	{
		TourView tourView = tourPage.Tour.CreateNewTourView(title != string.Empty ? title : slideId, tourPage);
		tourView.SlideId = slideId != string.Empty ? slideId : title;
		tourView.MarkerId = markerId;
		const bool importingSlides = true;
		tourPage.Tour.AddTourView(tourView, importingSlides);
		return tourView;
	}

	public void InsertTourViewIntoDatabase()
	{
		id = (int)MapsAliveDatabase.ReadScalar("sp_TourView_CreateTourView",
			"@TourPageId", tourPage.Id,
			"@ThemeId", tour.ThemeId,
			"@MarkerId", markerId
		);

		UpdateDatabase(false);
	}
	
	public static string NameOfMarkerAction(MarkerAction markerAction)
	{
		switch (markerAction)
		{
			case MarkerAction.None:
				return "none";

			case MarkerAction.GotoPage:
				return "gotopage";

			case MarkerAction.LinkToUrl:
			case MarkerAction.LinkToUrlNewWindow:
				return "url";

			case MarkerAction.CallJavascript:
				return "javascript";

			default:
				Debug.Fail("Unexpected MarkerAction " + markerAction);
				return string.Empty;
		}
	}

	public static string NameOfMediaType(SlideMediaType mediaType)
	{
		switch (mediaType)
		{
			case SlideMediaType.Photo:
				return "photo";

			case SlideMediaType.Embed:
				return "multimedia";
			
			default:
				Debug.Fail("Unexpected SlideMediaType " + mediaType);
				return string.Empty;
		}
	}

	public static string NameOfShowContentEvent(ShowContentEvent showContentEvent)
	{
		switch (showContentEvent)
		{
			case ShowContentEvent.OnMouseover:
				return "mouseover";
			
			case ShowContentEvent.OnClick:
				return "click";
			
			case ShowContentEvent.Never:
				return "never";
			
			default:
				Debug.Fail("Unexpected ShowContentEvent " + showContentEvent);
				return string.Empty;
		}
	}

	public void RemoveImage()
	{
		if (HasImage)
		{
			// Remove the hotspot's image file from the preview folder.
			string fileLocation = FileManager.PreviewFolderLocationAbsolute(tour.Id, image.FileNameInternal);
			FileManager.DeleteFile(fileLocation);

			// Remove the hotspot's image from the database.
			RemoveImageFromDatabase();
		}
	}

	private void RemoveImageFromDatabase()
	{
		// Remove the image from the database.
		image.Remove();
		image = null;
		imageId = 0;
	}

	public void SetImageChanged()
	{
		FlagAsChanged(ChangeFlags.Image);
	}

	public void SetSequenceNumber(int sequenceNumber)
	{
		this.sequenceNumber = sequenceNumber;
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourView_UpdateSequenceNumber", "@TourViewId", id, "@SequenceNumber", sequenceNumber);
	}

	public static bool TourViewSlideIdInUse(TourPage tourPage, string slideId)
	{
		return TourViewSlideIdInUse(tourPage, null, slideId);
	}

	public static bool TourViewSlideIdInUse(TourPage tourPage, TourView tourView, string slideId)
	{
		// This method passes a slide Id to avoid comparing its own slide Id.
		return MapsAliveDatabase.GetCount("sp_TourView_GetTourViewExistsBySlideId",
			"@TourPageId", tourPage.Id,
			"@TourViewId", tourView == null ? 0 : tourView.id,
			"@SlideId", slideId
		) != 0;
	}

	public static bool TourViewTitleInUse(TourPage tourPage, string title)
	{
		foreach (TourView tourView in tourPage.TourViews)
		{
			if (title.ToLower() == tourView.Title.ToLower())
				return true;
		}
		return false;
	}

	public void UpdateDatabase()
	{
		const bool notifyTourPage = true;
		UpdateDatabase(notifyTourPage);
	}

	public void UpdateDatabase(bool notifyTourPage)
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_TourView_UpdateTourView",
			"@TourViewId", id,
			"@SlideId", SlideId,
			"@ThemeId", tour.ThemeId,
			"@Title", title,
			"@Description", descriptionHtml,
			"@ToolTip", toolTip,
			"@ImageId", imageId,
			"@MarkerId", MarkerIsRoute ? 0 : markerId,
			"@MarkerPctX", markerPctX,
			"@MarkerPctY", markerPctY,
			"@MarkerRotation", markerRotation,
			"@MarkerClickAction", (int)markerClickAction,
			"@MarkerClickActionTarget", markerClickActionTarget,
			"@MarkerRolloverAction", (int)markerRolloverAction,
			"@MarkerRolloverActionTarget", markerRolloverActionTarget,
			"@MarkerRolloutAction", (int)markerRolloutAction,
			"@MarkerRolloutActionTarget", markerRolloutActionTarget,
			"@MarkerIsInvisible", markerIsHidden,
			"@MarkerIsDisabled", markerIsDisabled,
			"@MarkerIsLocked", markerIsLocked,
			"@MarkerIsNotAnchored", markerIsNotAnchored,
			"@MarkerIsRoute", markerIsRoute,
			"@MarkerIsStatic", markerIsStatic,
			"@MarkerZoomThreshold", markerZoomThreshold,
			"@MarkerZoomType", markerZoomType,
			"@SelectSlideEvent", (int)showContentEvent,
			"@ExcludeFromDirectory", excludeFromDirectory,
			"@MediaType", (int)mediaType,
			"@EmbedText", embedText,
			"@EmbedWidth", embedWidth,
			"@EmbedHeight", embedHeight,
			"@SlideWidthOverride", slideWidthOverride,
			"@SlideHeightOverride", slideHeightOverride,
			"@UsesLiveData", usesLiveData,
			"@MessengerFunction", messengerFunction,
			"@DirPreviewImageUrl", dirPreviewImageUrl,
			"@DirPreviewText", dirPreviewText,
			"@SequenceNumber", sequenceNumber,
			"@TouchPerformsClickAction", touchPerformsClickAction,
			"@Notes", notes,
			"@ChangeFlags", (int)changed
		);

		if (notifyTourPage)
		{
			bool rebuildMap = Changed(ChangeFlags.Marker | ChangeFlags.Tooltip);
			if (rebuildMap)
			{
				FlagAsUnchanged(ChangeFlags.Marker);
				FlagAsUnchanged(ChangeFlags.Tooltip);
			}

			tourPage.TourViewChanged(rebuildMap);
		}
	}
	#endregion

	#region ===== Private ===========================================================

	private bool Changed(ChangeFlags flags)
	{
		return (changed & flags) != 0;
	}

	private void CreateImage()
	{
		image = new TourViewImage(this, 0);
		imageId = TourImage.GetNextIdForTour(tour.Id);
		image.Id = imageId;
		image.InsertImageIntoDatabase();
	}

	private void FlagAsChanged(ChangeFlags flag)
	{
		FlagAsChangedIf(true, flag);
	}

	private void FlagAsChangedIf(bool condition, ChangeFlags flag)
	{
		if (condition)
			changed |= flag;
	}

	private void FlagAsUnchanged(ChangeFlags flag)
	{
		changed &= ~flag;
	}

	private void FlagMarkerChanged()
	{
		FlagAsChangedIf(true, ChangeFlags.Marker);
	}

	private void FlagMarkerChangedIf(bool condition)
	{
		FlagAsChangedIf(condition, ChangeFlags.Marker);
	}
	#endregion
}

public class TourViewComparer : IComparer
{
	int IComparer.Compare(Object o1, Object o2)
	{
		string title1 = ((TourView)o1).Title;
		string title2 = ((TourView)o2).Title;
		return string.Compare(title1, title2, true);
	}
}

public class TourViewSequenceNumberComparer : IComparer
{
	int IComparer.Compare(Object o1, Object o2)
	{
		int seq1 = ((TourView)o1).SequenceNumber;
		int seq2 = ((TourView)o2).SequenceNumber;
		return seq1 == seq2 ? 0 : (seq1 > seq2 ? 1 : -1);
	}
}
