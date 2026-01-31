// Copyright (C) 2003-2019 AvantLogic Corporation
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;

public partial class Members_Map : MemberPage
{
    private string defaultCoords;
    private string defaultCoordsCircle;
    private string defaultCoordsRectangle;
    private string defaultCoordsPolygon;
    private string defaultCoordsLine;
    private bool noMap;
	private bool optionsChanged;
    private string markerData;
    private string markerStyleData;
    private string hotspotData;
    private int newHotspotViewId;
    private int newShapeSideLength;

    protected override void EmitJavaScript()
    {
        if (noMap)
        {
            base.EmitJavaScript();
            return;
        }

        string loadingScript =
            AssignClientVar("actionIdEditView", (int)MemberPageActionId.EditHotspotContent) +
            AssignClientVar("actionIdHotspotAdvanced", (int)MemberPageActionId.HotspotOptionsAdvanced) +
            AssignClientVar("actionIdMarkerActions", (int)MemberPageActionId.EditHotspotActions) +
            AssignClientVar("contactName", account.ContactName.Replace("'", "\\'"));

        // Warn the user if they have not saved the map after several minutes. We only do this on the Map screen
        // because all of the editing actions happen client-side in JavaScript. If the user is doing a lot of
        // work on the map and their session expires, they will lose their changes.
        const int minutes = 15;
        const int milliseconds = minutes * 60 * 1000;

        string loadedScript = string.Format("setTimeout(maSaveWarning,{0});", milliseconds);
        loadingScript += AssignClientVar("saveTimeout", milliseconds);

        if (tour.SelectedTourView != null && Request.QueryString["locate"] == "1")
        {
            loadingScript += AssignClientVar("tourViewIdToLocate", tour.SelectedTourView.Id);
        }

        EmitJavaScript(loadingScript, loadedScript);

        string previewPath = tour.Url + "_";

        string version = "?v=" + tour.BuildId;

        // Emit the JavaScript include files.
        var scriptTag = new HtmlGenericControl { TagName = "script" };
        scriptTag.Attributes.Add("type", "module");

        string mapJsFileName = string.Format(TourBuilder.PatternForMapJsFile, tour.SelectedTourPage.PageNumber, tour.BuildId);
        scriptTag.Attributes.Add("src", ResolveUrl(string.Format("{0}/{1}{2}", previewPath, mapJsFileName, version)));

        this.Page.Header.Controls.Add(scriptTag);
    }

    protected string AppRuntimeFolder
    {
        get { return App.AppRuntimeUrl; }
    }

    private void CalculateShapeSideLength()
    {
        int shapeSideLengthTotal = 0;
        int markerCount = 0;

        DataTable dataTable = MapsAliveDatabase.LoadDataTable("sp_TourView_GetTourViewIdsByTourPageId", "@TourPageId", this.tourPage.Id);
        foreach (DataRow dataRow in dataTable.Rows)
        {
            MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
            int tourViewId = row.IntValue("TourViewId");

            TourView tourView = new TourView(tour, this.tourPage, tourViewId);

            if (tourView.MarkerId == 0)
                continue;

            if (!tourView.MarkerHasBeenPlacedOnMap)
                continue;

            // Get the length of the perimiter (width + height) of this marker, but only if the marker is on the map.
            Marker marker = Account.GetCachedMarker(tourView.MarkerId);
            int shapeSideLength = GetShapeSideLength(marker);
            if (shapeSideLength > 0)
            {
                markerCount += 1;
                shapeSideLengthTotal += shapeSideLength;
            }
        }

        // Calculate the average length of the sides of the marker shapes that are on this map.
        // That value will get used to deterine what size shape to create for a new shape marker.
        newShapeSideLength = markerCount == 0 ? 0 : (int)(((double)shapeSideLengthTotal) / (double)markerCount);
    }

    private void ConvertHybrid()
    {
        if (HybridConvert.Value.Length == 0)
            return;

        int tourViewId = int.Parse(HybridConvert.Value);

        // Clear the hidden form field.
        HybridConvert.Value = "";

        TourView tourView = tour.GetTourView(tourViewId);
        Marker marker = Account.GetCachedMarker(tourView.MarkerId);
        
        if (marker.ShapeType == ShapeType.Hybrid)
        {
            string[] coords = marker.ShapeCoords.Split(',');

            // Get the type of the hybrid's first and only shape. It's the 2nd value after "-1".
            ShapeType shapeType = (ShapeType)int.Parse(coords[1]);
            marker.ShapeType = shapeType;

            // Convert a a hybrid marker to a shape marker.
            switch (shapeType)
            {
                case ShapeType.Circle:
                    int radius = int.Parse(coords[4]);
                    marker.ShapeCoordsCircleRadius = radius;
                    marker.CircleRadius = radius;
                    break;

                case ShapeType.Rectangle:
                    int x1 = int.Parse(coords[2]);
                    int y1 = int.Parse(coords[3]);
                    int x2 = int.Parse(coords[4]);
                    int y2 = int.Parse(coords[5]);
                    int width = x2 - x1 + 1;
                    int height = y2 - y1 + 1;
                    marker.RectangleSize = new Size(width, height);
                    marker.ShapeCoordsRectangleSize = marker.RectangleSize;
                    break;

                case ShapeType.Polygon:
                case ShapeType.Line:
                    marker.ShapeCoords = marker.ShapeCoords.Substring(5);
                    break;
            }
        }
        else
        {
            // Convert a shape marker to a hybrid marker.
            string shapeCoords = "";
            switch (marker.ShapeType)
            {
                case ShapeType.Circle:
                    shapeCoords = string.Format("{0},{0},{0},0", marker.CircleRadius);
                    break;

                case ShapeType.Rectangle:
                    shapeCoords = string.Format("0,0,{0},{1}", marker.RectangleSize.Width - 1, marker.RectangleSize.Height - 1);
                    break;

                case ShapeType.Polygon:
                case ShapeType.Line:
                    shapeCoords = marker.ShapeCoords;
                    break;
            }

            marker.ShapeCoords = string.Format("-1,{0},{1}", (int)marker.ShapeType, shapeCoords);

            // Convert the shape to a hybrid now that the original shape type has been inserted into it.
            marker.ShapeType = ShapeType.Hybrid;
        }

        marker.UpdateDatabase();
        tourPage.RebuildMap();

        // Make view Id available to the Map Editor so it knows to select the marker when the page reloads.
        newHotspotViewId = tourViewId;
    }

    private void CreateDefaultCoords(int shapeSize)
    {
        int width = shapeSize;
        int half = (int)Math.Round((double)width / 2);
        int third = (int)Math.Round((double)width / 3);
        int quarter = (int)Math.Round((double)width / 4);
        int threeQuarter = quarter * 3;

        defaultCoordsCircle = string.Format("0,0,{0},0", third);
        defaultCoordsRectangle = string.Format("0,0,{1},{0}", half, width);
        defaultCoordsPolygon = string.Format("0,{2},{0},0,{1},{1},{2},{0},{3},{2}", quarter, half, threeQuarter, width);
        defaultCoordsLine = string.Format("0,0,{0},0,{1},{1},{2},{1}", third, third * 2, width);
    }

    private void CreateNewHotspot()
    {
        if (NewHotspot.Value.Length == 0)
            return;

        string[] args = NewHotspot.Value.Split(';');

        int markerId;
        int.TryParse(args[0], out markerId);
        string hotspotId = args[1];
        string hotspotTitle = args[2];

        // Clear the hidden form field.
        NewHotspot.Value = "";

        CreateNewHotspotTourView(markerId, hotspotId, hotspotTitle);

        tourPage.RebuildMap();
    }

    private TourView CreateNewHotspotTourView(int markerId, string hotspotId, string hotspotTitle)
    {
        TourView tourView = tour.CreateNewTourView();

        string id = hotspotId;
        int count = 2;
        while (TourView.TourViewSlideIdInUse(tourPage, tourView, id))
            id = String.Format("{0} ({1})", hotspotId, count++);

        tourView.SlideId = id;
        tourView.Title = hotspotTitle;
        tourView.MarkerId = markerId;

        tour.AddTourView(tourView);
        
        account.SetLastResourceId(TourResourceType.Marker, markerId);

        // Make the new view's Id available to the Map Editor so it knows to drop the new
        // hotspot's marker onto the map.
        newHotspotViewId = tourView.Id;

        return tourView;
    }

    private void CreateShapeMarker()
    {
        if (NewShapeMarker.Value.Length == 0)
            return;

        // Get the values passed  via the hidden form field.
        string[] args = NewShapeMarker.Value.Split(';');
        string markerAction = args[0];
        string hotspotAction = args[1];
        string markerName = args[2];
        string hotspotData = args[3];
        string shape = args[4];
        int markerId;
        int.TryParse(args[5], out markerId);
        int markerStyleId;
        int.TryParse(args[6], out markerStyleId);

        if (args.Length >= 8)
            int.TryParse(args[7], out newShapeSideLength);
        else
            newShapeSideLength = DefaultShapeSizeLength;

        if (newShapeSideLength <= 0)
            newShapeSideLength = DefaultShapeSizeLength;

        // Clear the form field so that it will be empty when the page loads again.
        NewShapeMarker.Value = "";
        
        // Remember the last shape chosen and whether last hotspot action was new or existing.
        LastShape.Value = shape;
        LastHotspotChoice.Value = hotspotAction;

        // Make sure the marker name is unique.
        string name = markerName;
        int count = 2;
        while (TourResource.NameInUse(TourResourceType.Marker, 0, name, account.Id))
            name = String.Format("{0} ({1})", markerName, count++);
        markerName = name;

        // Create an entirely new marker or a duplicate of an existing marker.
        Marker newMarker;
        if (markerAction == "new")
            newMarker = CreateShapeMarkerNew(shape, markerName, markerStyleId, newShapeSideLength);
        else
            newMarker = CreateShapeMarkerDuplicate(markerId, markerName);

        account.SetLastResourceId(TourResourceType.MarkerStyle, markerStyleId);

        // Assign the new marker to a hotspot.
        TourView tourView;
        if (hotspotAction == "new")
        {
            string[] data = hotspotData.Split(',');
            string hotspotId = data[0];
            string hotspotTitle = data[1];
            tourView = CreateNewHotspotTourView(newMarker.Id, hotspotId, hotspotTitle);
        }
        else
        {
            // Assign the marker to an existing hotspot.
            int tourViewId;
            int.TryParse(hotspotData, out tourViewId);
            tour.SetSelectedTourView(tourViewId);
            tourView = tour.SelectedTourView;

            // Remove the tour view's old marker from the database if it was exclusive to this tour view.
            MapsAliveDatabase.ExecuteStoredProcedure("sp_Marker_DeleteExclusive", "@TourViewId", tourViewId, "@MarkerId", tourView.MarkerId);
            
            tourView.MarkerId = newMarker.Id;
            tourView.UpdateDatabase();
        }

        // Make the new view's Id available to the Map Editor so it knows to drop a new
        // hotspot's marker onto the map or select the new marker for an existing hotspot.
        newHotspotViewId = tourView.Id;

        tourPage.RebuildMap();
    }

    private Marker CreateShapeMarkerDuplicate(int markerId, string markerName)
    {
        Marker newMarker = (Marker)TourResource.DuplicateResourceInDatabase(account.Id, TourResourceType.Marker, markerId, ResourceDuplicateAction.CopyExistingResource);
        newMarker.Name = markerName;

        // Make sure that the new marker gets a new resource image it it needs one. This logic is here to address
        // the case where someone duplicates an exclusive marker e.g. a marker from a ReadyMap or other map that
        // uses imported marker shapes. Imported markers use 1 for the resource image Id so that no resource image
        // gets generated for them. See comment in ImporterForMarkerShapes::CreateMarker for an explanation.
        newMarker.CreateResourceImageId();
        newMarker.UpdateResourceImageIdInDatabase();
        TourResource.CreateResourceImageFile(TourResourceType.Marker, newMarker.Id, newMarker.ResourceImageId, ResourceImageFileAction.CreateFileIfMissing);

        newMarker.UpdateDatabase();
        return newMarker;
    }

    private Marker CreateShapeMarkerNew(string shape, string markerName, int markerStyleId, int shapeSizeLength)
    {
        // Create a new marker resource by duplicating a default resource and inserting it into the database.
        int resourceId = account.DefaultResourceId(TourResourceType.Marker);
        Marker newMarker = (Marker)TourResource.DuplicateResourceInDatabase(account.Id, TourResourceType.Marker, resourceId, ResourceDuplicateAction.CopyExistingResource);

        // Make a copy of the resource so that the resource's image can get updated later.
        TourResource resourceBeforeEdit = newMarker.Clone();

        // Update the new marker to be a shape marker with the name and marker style chosen by the user.
        newMarker.Name = markerName;
        newMarker.MarkerStyle = new MarkerStyle(markerStyleId);
        newMarker.NormalSymbolId = 0;
        newMarker.SelectedSymbolId = 0;
        newMarker.MarkerType = MarkerType.Shape;
        newMarker.ScaleShapeToMap = true;

        if (shapeSizeLength == 0)
            shapeSizeLength = DefaultShapeSizeLength;

        int width = shapeSizeLength;
        int halfWidth = (int)Math.Round((double)width / 2);
        int thirdWidth = (int)Math.Round((double)width / 3);
        CreateDefaultCoords(shapeSizeLength);

        // Create a default shape for a new marker.
        switch (shape)
        {
            case "circle":
                newMarker.ShapeType = ShapeType.Circle;
                newMarker.ShapeCoordsCircleRadius = halfWidth;
                newMarker.CircleRadius = halfWidth;
                break;

            case "rectangle":
                newMarker.ShapeType = ShapeType.Rectangle;
                Size rectangleSize = new Size(width, halfWidth);
                newMarker.ShapeCoordsRectangleSize = rectangleSize;
                newMarker.RectangleSize = rectangleSize;
                break;

            case "line":
                newMarker.ShapeType = ShapeType.Line;
                newMarker.ShapeCoords = defaultCoordsLine;
                break;

            case "polygon":
                newMarker.ShapeType = ShapeType.Polygon;
                newMarker.ShapeCoords = defaultCoordsPolygon;
                break;

            default:
                break;
        }

        // Save the changes to the marker.
        newMarker.UpdateDatabase();

        // Update the resource's image that is cached in the Markers folder in AppRuntime.
        newMarker.UpdateResource(resourceBeforeEdit);

        return newMarker;
    }

	private void CreateThumbList()
	{
		// Fetch all of the view images for this page.
		DataTable dataTable = MapsAliveDatabase.LoadDataTable("sp_TourPage_GetAllTourViewImages", "@TourPageId", tourPage.Id, "@ThemeId", tour.ThemeId);

		string thumbList = string.Empty;
		tourPage.TourViews.Sort(new TourViewComparer());
		foreach (TourView tourView in tourPage.TourViews)
		{
			// Create a list of information that we can split later on.
			int imageId = tourView.HasImage ? tourView.Image.TourImageId : 0;
			string title = tourView.Title;
			Size imageSize = tourView.HasImage ? ImageThumbSize(MarkerThumbs.QuickPhotoSize, tourView.Image) : Size.Empty;
			string hasMarker = tourView.MarkerHasBeenPlacedOnMap ? "1" : "0";
			string hasText = tourView.DescriptionHtml.Trim().Length > 0 ? "1" : "0";
			string hasAction = ((tourView.MarkerClickAction != MarkerAction.None) || (tourView.ToolTip.Trim().Length != 0)) ? "1" : "0";
			thumbList += string.Format("{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{1}", (char)0x01, (char)0x02, imageId, title, tourView.Id, imageSize.Width, imageSize.Height, hasMarker, hasText, hasAction);

			// Attach this view's thumbnail image to the view's TourImage object.
			foreach (DataRow dataRow in dataTable.Rows)
			{
				MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
				if (tourView.Id == row.IntValue("TourViewId") && tourView.HasImage)
				{
					tourView.Image.SetThumbnail(row.ByteArrayValue("Thumbnail"), Tour.ThumbnailSize);
					break;
				}
			}
		}

		// Remove the last 0x02 delimeter.
		if (thumbList.Length > 0)
			thumbList = thumbList.Substring(0, thumbList.Length - 1);

		MarkerThumbs.ThumbList = thumbList;
        int thumbAreaWidth = Math.Max(TourBuilderPageContentRightWidth, tourPage.MapAreaSize.Width);
        MarkerThumbs.ColumnsPerRow = (int)Math.Round((float)thumbAreaWidth / TourBuilderMapPageSlideThumbWidth);
	}

    protected string DefaultMarkerImage
    {
        get { return "../Images/DefaultMarkerImage.png"; }
    }

    protected string DefaultMarkerId
    {
        get
        {
            int markerId = account.LastResourceId(TourResourceType.Marker);
            if (markerId == 0)
                markerId = account.DefaultResourceId(TourResourceType.Marker);
            return markerId.ToString();
        }
    }

    protected string DefaultMarkerStyleId
    {
        get
        {
            int markerStyleId = account.LastResourceId(TourResourceType.MarkerStyle);
            if (markerStyleId == 0)
                markerStyleId = account.DefaultResourceId(TourResourceType.MarkerStyle);
            return markerStyleId.ToString();
        }
    }

    private int DefaultShapeSizeLength
    {
        get
        {
            // Use 10% of the map width as the default shape width.
            Size mapSize = tourPage.MapImage.Size;
            int shapeWidth = (int)Math.Round((double)mapSize.Width * .1);

            // Handle the case where no map was uploaded.
            if (shapeWidth == 0)
                shapeWidth = (int)Math.Round((double)tourPage.MapAreaSize.Width * .1);
            
            return shapeWidth;
        }
    }

    private void DeleteHotspots()
    {
        if (DeletedHotspots.Value.Length == 0)
            return;

        string[] viewIds = DeletedHotspots.Value.Split(',');

        // Clear the hidden form field.
        DeletedHotspots.Value = "";

        foreach (string tourViewId in viewIds)
        {
            TourView tourView = tourPage.GetTourView(int.Parse(tourViewId));
            Debug.Assert(tourView != null, "View Id not found for hotspot to delete");
            if (tourView == null)
                continue;
            tourView.Delete();
        }

        tourPage.RebuildMap();
    }

    private string EscapeSingleQuotes(string text)
    {
        return text.Replace("'", "\\'");
    }

    private void EmitDefaultCoords()
    {
        CalculateShapeSideLength();
        CreateDefaultCoords(NewShapeSideLength);
        defaultCoords = string.Format("{{circle:'{0}',rectangle:'{1}',polygon:'{2}',line:'{3}'}}",
            defaultCoordsCircle, defaultCoordsRectangle, defaultCoordsPolygon, defaultCoordsLine);
    }

    private void EmitHotspotData()
    {
        hotspotData = "[";

        foreach (TourView tourView in tourPage.TourViews)
        {
            if (hotspotData.Length > 1)
                hotspotData += ",";
            string title = EscapeSingleQuotes(tourView.Title);
            string hotspotId = tourView.SlideId.Replace("'", "\\'");
            hotspotData += string.Format("{{viewId:{0}, title:'{1}', id:'{2}', markerId:{3}}}", tourView.Id, title, hotspotId, tourView.MarkerId);
        }

        hotspotData += "]";
    }

    private void EmitMarkerData()
    {
        TourResourceType resourceType = TourResourceType.Marker;
        string sp = string.Format("sp_{0}_Get{0}sOwnedByAccount", resourceType.ToString());
        DataTable itemsDataTable = MapsAliveDatabase.LoadDataTable(sp, "AccountId", Utility.AccountId);

        markerData = "[";

        foreach (DataRow dataRow in itemsDataTable.Rows)
        {
            MapsAliveDataRow row = new MapsAliveDataRow(dataRow);

            string idName = string.Format("{0}Id", resourceType.ToString());
            int markerId = row.IntValue(idName);
            if (markerId == 0)
                continue;

            string name = EscapeSingleQuotes(row.StringValue("Name"));

            string resourceImageId = row.StringValue("ResourceImageId");

            if (markerData.Length > 1)
                markerData += ",";
            markerData += string.Format("{{id:{0}, name:'{1}', image:'{2}'}}", markerId, name, resourceImageId);
        }

        markerData += "]";
    }

    private void EmitMarkerStyleData()
    {
        TourResourceType resourceType = TourResourceType.MarkerStyle;
        string sp = string.Format("sp_{0}_Get{0}sOwnedByAccount", resourceType.ToString());
        DataTable itemsDataTable = MapsAliveDatabase.LoadDataTable(sp, "AccountId", Utility.AccountId);

        markerStyleData = "[";

        foreach (DataRow dataRow in itemsDataTable.Rows)
        {
            MapsAliveDataRow row = new MapsAliveDataRow(dataRow);

            string idName = string.Format("{0}Id", resourceType.ToString());
            int id = row.IntValue(idName);
            if (id == 0)
                continue;
            string name = EscapeSingleQuotes(row.StringValue("Name"));

            string resourceImageId = row.StringValue("ResourceImageId");

            if (markerStyleData.Length > 1)
                markerStyleData += ",";
            markerStyleData += string.Format("{{id:{0}, name:'{1}', image:'{2}'}}", id, name, resourceImageId);
        }

        markerStyleData += "]";
    }

    private int GetShapeSideLength(Marker marker)
    {
        // Get the marker's perimeter length (width + height).
        int length;
        switch (marker.ShapeType)
        {
            case ShapeType.Circle:
                length = marker.CircleRadius * 4;
                break;

            case ShapeType.Rectangle:
                length = marker.RectangleSize.Width + marker.RectangleSize.Height;
                break;

            case ShapeType.Line:
            case ShapeType.Polygon:
            case ShapeType.Hybrid:
                Rectangle shapeRectangle = marker.ShapeRectangle;
                length = shapeRectangle.Width + shapeRectangle.Height;
                break;

            default:
                length = 0;
                break;
        }

        // Return the avarage of the shape's width plus its height.
        return (int)((double)length / 2);
    }

    protected string HotspotData
    {
        get { return hotspotData; }
    }

    protected string HotspotLimitReached
    {
        get
        {
            HotspotLimitStatus status = MapsAliveState.Account.HotspotLimitStatus;
            bool atLimit = status == HotspotLimitStatus.AtLimit || (status == HotspotLimitStatus.OverLimit && tour.ExceedsSlideLimit);
            return atLimit ? "true" : "false";
        }
    }

	protected override void InitControls(bool undo)
	{
		if (noMap)
		{
			MapPanel.Visible = false;
			NoMapPanel.Visible = true;
			return;
		}

		CreateThumbList();

		InitHotspotList();
		InitRoutesList();
		InitMapAreaSize();

        EmitHotspotData();
        EmitMarkerData();
        EmitMarkerStyleData();
        EmitDefaultCoords();
    }

	private Size ImageThumbSize(Size containerSize, TourImage tourImage)
	{
		Size imageSize = new Size(tourImage.Width, tourImage.Height);
		return Utility.ScaledImageSize(imageSize, containerSize);
	}

    private void InitHotspotList()
	{
		// Populate the slide name drop down list.
		SlideNameDropDownList.Items.Clear();
		ListItem item = new ListItem("- Choose a hotspot -", "0");
		item.Selected = true;
		SlideNameDropDownList.Items.Add(item);
		foreach (TourView tourView in tourPage.TourViews)
		{
			item = new ListItem(tourView.Title, tourView.Id.ToString());
			SlideNameDropDownList.Items.Add(item);
		}
		SlideNameDropDownList.Attributes.Add("onchange", "maSelectMarkerThumb(parseInt(this.value, 10));");
	}

	private void InitMapAreaSize()
	{
        MapArea.Width = tourPage.MapAreaSize.Width;
        MapArea.Height = tourPage.MapAreaSize.Height;
		
        // V4 tours are always zoomable.
        mapControls.MapCanZoom = tourPage.MapCanZoom || tour.V4;
        mapControls.DisplayShowMapFocusControl = tour.V4;
	}

	private void InitRoutesList()
	{
		string routesXml = tourPage.RoutesXml;
		if (routesXml.Length == 0)
			return;

		RoutesListExplain.Visible = true;
		RoutesDropDownList.Visible = true;
		RoutesDropDownList.Items.Clear();
		ListItem item = new ListItem("- Hide Routes -", "0");
		item.Selected = true;
		RoutesDropDownList.Items.Add(item);

		Routes routes = new Routes(tourPage.RoutesXml);
		XmlNodeList routeNodes = routes.RouteNodes;
		foreach (XmlNode routeNode in routeNodes)
		{
            string routeId = routeNode.Attributes["id"].Value;
            item = new ListItem(routeId, routeId);
			RoutesDropDownList.Items.Add(item);
		}
		string script = "maDrawRoute(this.value);";
		RoutesDropDownList.Attributes.Add("onchange", script);
	}

    protected string IsPlusOrProPlan
    {
        get { return account.IsPlusOrProPlan ? "true" : "false"; }
    }

    protected string MarkerCount
    {
        get
        {
            int count = TourResource.GetCount(TourResourceType.Marker, account.Id);
            count += 1;
            return count.ToString();
        }
    }

    protected string DefaultCoords
    {
        get { return defaultCoords; }
    }

    protected string MarkerData
    {
        get { return markerData; }
    }

    protected string MarkerStyleData
    {
        get { return markerStyleData; }
    }

    protected int NewHotspotViewId
    {
        get { return newHotspotViewId; }
    }

    protected int NewShapeSideLength
    {
        get { return newShapeSideLength; }
    }

    protected override void PageLoad()
	{
		// Ideally the Map Editor would be imported as type <script type="module" instead of
        // "text/javascript" but we haven't figured out how to do that in ASP.NET. 
        Utility.RegisterClientJavaScript(this, RuntimeFile.MapEditorJs);

		SetMasterPage(Master);
		SetPageTitle(Resources.Text.MapPageTitle);
		SetActionIdForPageAction(MemberPageActionId.Map);
		GetSelectedTourPage();

		if (tourPage.IsGallery)
		{
			// This should only happen if the user manually edited the URL, but
			// let make sure you can't try to work with a gallery as a map.
			Response.Redirect(MemberPageAction.ActionPageTarget(MemberPageActionId.Gallery));
		}

		noMap = !tourPage.SlidesPopup && !tourPage.LayoutAreaSlideLayout.HasMapArea;

		if (noMap)
			SetPageReadOnly();
		
		// For development and debugging.
		bool forceRebuild = Request.QueryString["build"] == "1";
		
		if (tourPage.HasChangedSinceLastBuilt() || tour.BuildId == 0 || forceRebuild)
		{
			TourBuilder tourBuilder = new TourBuilder(tour);
			tourBuilder.BuildTour();
		}
	}

	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}

    protected string PathTourFolder
    {
        get { return tour.Url + "_"; }
    }

    protected override void PerformUpdate()
	{
		tourPage.UpdateMarkerCoords(MarkerCoords.Value);

        // Clear the hidden form field.
        MarkerCoords.Value = "";

        string state = ZoomState.Value;

		if (state.Length != 0)
		{
			string[] stateValue = state.Split(',');

			// Update the Map Editor's current zoom and pan state. The next time the users
            // goes to the Map Editor, the map will be zoomed and panned using these value.
            // These values are not used by a tour.
            tourPage.MapZoomLevel = float.Parse(stateValue[0]);
			tourPage.MapZoomX = int.Parse(stateValue[1]);
			tourPage.MapZoomY = int.Parse(stateValue[2]);
            
            // Pack the focus point X and Y values into a single integer.
            short focusStateX = short.Parse(stateValue[3]);
            short focusStateY = short.Parse(stateValue[4]);
            int mapFocus = (focusStateX << 16) | (focusStateY & 0xffff);

            // Update the zoom percent and focus position that the user choose in the Map Editor by
            // clicking the map focus button and then clicking a focus point. These values are used to
            // zoom and position a V4 map when it first displays in a tour. V3 tours use the values
            // above for that purpose These values are created in, but not used by the Map Editor.
            tourPage.MapFocus = mapFocus; 
            tourPage.MapFocusPercent = int.Parse(stateValue[5]);
            
            optionsChanged = true;
		}

		if (optionsChanged)
			tourPage.UpdateDatabase();

        DeleteHotspots();
        ReplaceMarkers();
        ReplaceMarkerStyles();
        UpdateEditedMarkers();
        CreateNewHotspot();
        CreateShapeMarker();
        ConvertHybrid();

		if (tourPage.MapChanged)
		{
			TourBuilder tourBuilder = new TourBuilder(tour);
			tourBuilder.BuildTour();
		}
	}

    protected override void ReadPageFields()
	{
	}

    private void ReplaceMarkers()
    {
        if (ReplacedMarkers.Value.Length == 0)
            return;

        string[] args = ReplacedMarkers.Value.Split(';');
        
        int replacementMarkerId = int.Parse(args[0]);
        
        string[] replacements = args[1].Split(',');

        // Clear the hidden form field.
        ReplacedMarkers.Value = "";

        foreach (string tourViewId in replacements)
        {
            TourView tourView = tourPage.GetTourView(int.Parse(tourViewId));

            // Remove the old marker from the database if it was exclusive to this tour view.
            MapsAliveDatabase.ExecuteStoredProcedure("sp_Marker_DeleteExclusive", "@TourViewId", tourViewId, "@MarkerId", tourView.MarkerId);

            tourView.MarkerId = replacementMarkerId;
            tourView.UpdateDatabase();
        }

        tourPage.RebuildMap();

        account.SetLastResourceId(TourResourceType.Marker, replacementMarkerId);
    }

    private void ReplaceMarkerStyles()
    {
        if (ReplacedMarkerStyles.Value.Length == 0)
            return;

        string[] args = ReplacedMarkerStyles.Value.Split(';');

        int replacementMarkerStyleId = int.Parse(args[0]);
        MarkerStyle replacementMarkerStyle = Account.GetCachedMarkerStyle(replacementMarkerStyleId);

        string[] replacements = args[1].Split(',');

        // Clear the hidden form field.
        ReplacedMarkerStyles.Value = "";

        int count = 0;

        foreach (string tourViewId in replacements)
        {
            // Get the marker.
            TourView tourView = tourPage.GetTourView(int.Parse(tourViewId));
            int markerId = tourView.MarkerId;
            Marker marker = Account.GetCachedMarker(markerId);
            if (marker.MarkerType == MarkerType.Symbol)
                continue;

            // Skip this marker if its style has already been replaced. 
            if (marker.MarkerStyle == replacementMarkerStyle)
                continue;

            marker.MarkerStyle = replacementMarkerStyle;
            marker.UpdateResourceAndDependents();
            count += 1;
        }

        if (count > 0)
            tourPage.RebuildMap();

        account.SetLastResourceId(TourResourceType.MarkerStyle, replacementMarkerStyleId);
    }

	protected override bool SetStatus()
	{
		StatusBox.Clear();
        StatusBox.SetStatusBoxTopLeft(34, 0);

        int tourViewCount = tourPage.TourViews.Count;

        if (ShowSmartStatusBox("map"))
            return true;

		tourPage.UpdateMarkerCoords(MarkerCoords.Value);
        if (tourPage.MarkersOnMap >= 2)
            return false;

        StatusBox.Note = "Learn about the <a'start-map-editor'>Map Editor</a>";

        string lastAction;
        if (tourPage.MarkersOnMap == 0)
        {
            lastAction = string.Format("The map has {0} hotspot{1} but", tourViewCount, tourViewCount > 1 ? "s" : "");
            lastAction += "<br>no markers are on the map yet";
        }
        else if (tourPage.MarkersOnMap == 1)
        {
            lastAction = "There is 1 marker on the map";
        }
        else
        {
            lastAction = "You are almost done!";
        }

        StatusBox.LastAction = lastAction;
        //StatusBox.LastActionNote = "When done in Tour Preview,<br>click the Return to Tour Builder link";
        StatusBox.NextAction = tourPage.MarkersOnMap == 0 ? "Place markers on the map" : "Place another marker on the map";
		StatusBox.SetStep(1, "[Choose a hotspot] from the list above the map.");
		StatusBox.SetStep(2, "The <a'ref-markers/#working-with-markers'>marker</a> will appear centered on the map.");
        StatusBox.SetStep(3, "[Drag the marker] anywhere on the map.");
		StatusBox.SetStep(4, "[Repeat] for each hotspot.");

		return true;
	}

    protected string StyleDefinitions
    {
        get { return MapPageStyleDefinitions;  }
    }
    private void UpdateEditedMarker(int markerId, ShapeType shapeType, int shapeW, int shapeH, string coords)
    {
        Marker marker = Account.GetCachedMarker(markerId);

        // Make a copy of the resource so that the resource's image can get updated later.
        TourResource resourceBeforeEdit = marker.Clone();

        if (shapeType == ShapeType.Rectangle)
        {
            Size size = new Size(shapeW, shapeH);
            marker.RectangleSize = size;
            marker.ShapeCoordsRectangleSize = size;
        }
        else if (shapeType == ShapeType.Circle)
        {
            int radius = (int)Math.Round((double)shapeW / 2);
            marker.CircleRadius = radius;
            marker.ShapeCoordsCircleRadius = radius;
        }
        else
        {
            marker.ShapeCoords = coords;
        }

        marker.UpdateDatabase();

        marker.UpdateResource(resourceBeforeEdit);
    }

    private void UpdateEditedMarkers()
    {
        if (EditedMarkers.Value.Length == 0)
            return;

        string[] edits = EditedMarkers.Value.Split('|');

        foreach (string edit in edits)
        {
            string[] args = edit.Split(';');

            int markerId;
            int.TryParse(args[0], out markerId);
            int shapeType;
            int.TryParse(args[1], out shapeType);
            int shapeW;
            int.TryParse(args[2], out shapeW);
            int shapeH;
            int.TryParse(args[3], out shapeH);
            string coords = args[4];

            UpdateEditedMarker(markerId, (ShapeType)shapeType, shapeW, shapeH, coords);
        }

        // Clear the hidden form field.
        EditedMarkers.Value = "";

        foreach (TourPage tourPage in tour.TourPages)
        {
            tourPage.MapMarkerChanged();
            tourPage.RebuildMap();
        }
    }
}
