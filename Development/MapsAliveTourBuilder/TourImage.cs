// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

// These values are known in the DB -- don't change.
public enum ImageExpansionType
{
	None = -1,
	Center = 0,
	UpperLeft = 1,
	Repeat = 2
}

public abstract class TourImage
{
	protected string fileExt;
	protected int height;
	protected int id;
	protected Byte[] imageBytes;
	protected bool isMapImage;
	protected int length;
	protected string originalFileName;
	protected int readyMapPackageId;
	protected int themeId;
	protected Byte[] thumbnailBytes;
	protected Size thumbnailSize;
	protected int tourImageId;
	protected bool usesThumbnail;
	protected int version;
	protected int versionBuilt;
	protected bool wasUploaded;
	protected int width;

	private const string tempFilePrefix = "~img";
	private const int thumbnailDimension = 100;

	public TourImage(int themeId)
	{
		this.themeId = themeId;
		originalFileName = string.Empty;
		fileExt = "jpg";
		imageBytes = new Byte[0];
		thumbnailBytes = new Byte[0];
		usesThumbnail = true;
	}

	#region ===== Properties ========================================================

	public float AspectRatio
	{
		get { return HasFile ? (float)width / (float)height : 0.0f; }
	}

	public Byte[] Bytes
	{
		get
		{
			Byte[] bytes = new Byte[0];

			if (imageBytes.Length > 0)
			{
				// The image's bytes are already here because the image was just uploaded.
				bytes = imageBytes;
			}
			else if (length > 0)
			{
				// Fetch the image from the database.
//				System.Diagnostics.Debug.WriteLine(string.Format(">>> Read {0} bytes from DB for {1}", length, originalFileName));
				bytes = (Byte[])MapsAliveDatabase.LoadScalar("sp_TourImage_GetImage", "@TourImageId", tourImageId);
			}

			return bytes;
		}
	}

	public string FileNameInternal
	{
		get	{ return DeriveFileNameInternal(id, themeId, version, "jpg");	}
	}

	public string FileNameOriginal
	{
		get { return originalFileName; }
		set { originalFileName = value; }
	}

	public bool HasFile
	{
		get { return originalFileName.Length >= 1; }
	}

	public int Height
	{
		get { return height; }
		set	{ height = value; }
	}

	public int Id
	{
		get { return id; }
		set { id = value; }
	}

	public bool IsReadyMap
	{
		get { return readyMapPackageId > 0; }
	}

	public int Length
	{
		get { return length; }
	}

	public abstract string MissingImageText
	{
		get;
	}

	public int ReadyMapPackageId
	{
		get { return readyMapPackageId; }
		set { readyMapPackageId = value; }
	}

	public Size Size
	{
		get { return new Size(width, height); }
		set
		{
			width = value.Width;
			height = value.Height;
		}
	}

	public static string TempFileWildcard
	{
		get { return tempFilePrefix + "*.*"; }
	}

	public Size ThumbnailSize
	{
		get { return thumbnailSize; }
	}

	public int TourImageId
	{
		get { return tourImageId; }
	}

	protected bool UsesThumbnail
	{
		set { usesThumbnail = value; }
	}

	public int Version
	{
		get { return version; }
	}

	public int VersionBuilt	
	{
		get { return versionBuilt; }
	}

	public bool VersionChanged
	{
		get { return version != versionBuilt; }
	}

	public bool WasUploaded
	{
		get { return wasUploaded; }
	}

	public int Width
	{
		get { return width; }
		set	{ width = value; }
	}
	#endregion

	#region ===== Public ============================================================

	public void BumpVersionAndUpdateDatabase()
	{
		version++;
		UpdateImageVersionInDatabase();
	}

	public void Built()
	{
		versionBuilt = version;
	}

	public void CreateFile(int tourId, Size containerSize, bool isMapImage)
	{
		CreateFile(tourId, containerSize, isMapImage, false, "#ffffff");
	}

	public void CreateFile(int tourId, Size containerSize, bool isMapImage, bool isMapInsetImage, string mapPlaceholderColor)
	{
		CreateFile(tourId, null, containerSize, isMapImage, isMapInsetImage, mapPlaceholderColor, ImageExpansionType.None);
	}

	public void CreateFile(int tourId, TourPage tourPage, Size containerSize, bool isMapImage, bool isMapInsetImage, string mapPlaceholderColor, ImageExpansionType expansionType)
	{
		if (containerSize.Width == 0 || containerSize.Height == 0)
			return;

		this.isMapImage = isMapImage;

		string mapImageFileLocation = FileManager.PreviewFolderLocationAbsolute(tourId, FileNameInternal);

		FileInfo fileInfo = new FileInfo(mapImageFileLocation);
		string fileName = fileInfo.Name;

		if (isMapInsetImage)
			mapImageFileLocation = mapImageFileLocation.Replace(fileName, "_" + fileName);

        if (FileManager.FileExists(mapImageFileLocation))
            return;

        Tour tour = MapsAliveState.SelectedTour;
        if (isMapImage && tour.V3CompatibilityEnabled)
		{
			// This code gets executed when the map size has changed. Delete any previous versions of the file
			// that are in the tour folder. By doing a wildcard delete, we guarantee that any file with a 
			// matching prefix will get cleaned up including all tile images if the map was sliced.
            // V4 doesn't use this logic because it completely cleans out the tour folder before rebuilding.
			DeleteMapImagesFromPreviewFolder(tourId, isMapInsetImage);
		}

		// Create the new file.
		Byte[] bytes = HasFile ? Bytes : Utility.DefaultImageBytes(containerSize, mapPlaceholderColor);
		if (bytes == null)
		{
			// This would normally never happen, but it can if the map has a negative height because the tour
			// height is fixed and has a too-tall banner. The problem should be caught in the banner logic,
			// but for now we just keep an exception from occurring.
			return;
		}

		using (MemoryStream memoryStream = new MemoryStream(bytes))
		{
            Bitmap bitmap = null;
            try
            {
                bitmap = (Bitmap)Bitmap.FromStream(memoryStream);
            }
            catch (Exception ex)
            {
                // This is only known to occur on very old tours that have a swf file for their map image
                // and no alternate jpg file. Catching the exception here allows the Tour Builder to open
                // the tour, but other exceptions will occur if you try to work with it. You can however
                // go to the Choose Map Image page (where you'll see the name of the swf file) and click
                // the Remove Map link. Then you can choose a new jpg map or build the tour without a map. 
                Utility.ReportException("TourImage.CreateFile", "Unable to open swf file. On the Choose Map Image screen, remove the swf and replace with a jpg.", ex);
                return;
            }

            // Scale-down the image if necessary.  Correctly sized images will be left as-is.
            Size originalSize = bitmap.Size;
					
			Size scaledImageSize = Utility.ScaledImageSize(originalSize, containerSize);
			if (expansionType != ImageExpansionType.None && (scaledImageSize.Width < containerSize.Width || scaledImageSize.Height < containerSize.Height))
			{
				Bitmap expandedBitmap = Utility.ExpandBitmap(bitmap, containerSize, Utility.HexToColor(mapPlaceholderColor), expansionType);
				bitmap.Dispose();
				bitmap = expandedBitmap;
			}
			else
			{
				Bitmap scaledBitmap = Utility.ScaledBitmap(bitmap, containerSize, true);
				bitmap = scaledBitmap;
			}

			// The map inset is so small that it gets kind of soft so we sharpen it.
			if (isMapInsetImage)
				Utility.SharpenMapInsetImage(bitmap);

			// We always create files in jpg format even though we attempt to preserve their original
			// format when we upload them. A file we create will only be used as either a map image
			// or a slide image. Gif files don't scale down well, so we have to convert those to jpg
			// unless they are used at their full size. So rather than complicate the logic and attempt
			// to deal with lots of formats, we simply make every file a jpg.
			try
			{
				bitmap.Save(mapImageFileLocation, ImageFormat.Jpeg);

				// Create the V3 map tiles file.
                if (tourPage != null && isMapImage && !isMapInsetImage && tourPage.Tour.V3CompatibilityEnabled)
				{
					StringBuilder mapTilesDataZoomedIn = new StringBuilder();
					StringBuilder mapTilesDataZoomedOut = new StringBuilder();

                    string fileLocation;
                    string fileContent;

                    // Create the map tiles data for both V3 and V4 for the zoomed-in image, or the map image if not zoomable.
                    CreateMapTiles(bitmap, mapTilesDataZoomedIn);

                    // Create the map tiles file in the form needed for V3.
                    if (tourPage.MapCanZoom)
                    {
                        // Create the V3 zoomed-out map tiles data.
                        double scale = tourPage.CalculateMapAreaScale();
                        Size size = new Size((int)((double)bitmap.Width * scale), (int)(((double)bitmap.Height * scale)));
                        Bitmap scaledBitmap = Utility.ScaledBitmap(bitmap, size, false);
                        CreateMapTiles(scaledBitmap, mapTilesDataZoomedOut);
                        scaledBitmap.Dispose();
                    }

                    // Create the V3 map tiles file which is a JavaScript array of one or two elements, one for each zoom level.
                    // If the map zooms, the first array element contains an array of the zoomed-out tiles and the second contains
                    // the tiles for the map at 100%. If the map does not zoom, the array contains only one element which is an
                    // array of the the tiles for the map image scaled to fit the tour's map area.
                    fileLocation = mapImageFileLocation.Substring(0, mapImageFileLocation.Length - 4);
                    string zoomedOutData = tourPage.MapCanZoom ? string.Format("{0},\n", mapTilesDataZoomedOut) : "";
                    fileContent = string.Format("maClient.Html5.prototype.mapTiles=function(){{return [\n{0}{1}\n];}};\n", zoomedOutData, mapTilesDataZoomedIn);
                    FileManager.CreateTextFile(string.Format(TourBuilder.PatternForMapTilesFileV3, fileLocation), fileContent);
				}
						
				bitmap.Dispose();
			}
			catch (Exception ex)
			{
				Utility.ReportException("TourImage.CreateFile", string.Format("{0} : {1}", mapImageFileLocation, bitmap.Size.ToString()), ex);

				// Every so often someone uploads an image that triggers a GDI error when it gets written to
				// a file (see OnTime task 791). Handle the error by substituting a good "bad image" image.
				string badImageFileLocation = FileManager.WebAppFileLocationAbsolute("Images", "BadImage.jpg");
				FileManager.CopyFile(badImageFileLocation, mapImageFileLocation);
			}
		}
	}

	public void CreateFilesForMapImage(int tourId, Size containerSize, string mapPlaceholderColor)
	{
 		string mapImageFileLocation = FileManager.PreviewFolderLocationAbsolute(tourId, FileNameInternal);

		FileInfo fileInfo = new FileInfo(mapImageFileLocation);
		string fileName = fileInfo.Name;

        string fileLocation100 = mapImageFileLocation.Replace(fileName, fileName.Replace(".jpg", "_100.jpg"));
        string fileLocation50 = mapImageFileLocation.Replace(fileName, fileName.Replace(".jpg", "_50.jpg"));
        string fileLocation25 = mapImageFileLocation.Replace(fileName, fileName.Replace(".jpg", "_25.jpg"));

        Size size50 = new Size(containerSize.Width / 2, containerSize.Height / 2);
        Size size25 = new Size(containerSize.Width / 4, containerSize.Height / 4);

		// Create an array of bytes containing the data for the full size image.
		Byte[] bytes = HasFile ? Bytes : Utility.DefaultImageBytes(containerSize, mapPlaceholderColor);
		if (bytes == null)
		{
			// This would normally never happen, but it can if the map has a negative height because the tour
			// height is fixed and has a too-tall banner. The problem should be caught in the banner logic,
			// but for now we just keep an exception from occurring.
			return;
		}

        Bitmap bitmap100;
        Bitmap bitmap50;
        Bitmap bitmap25;

        using (MemoryStream memoryStream = new MemoryStream(bytes))
        {
            // Create the 100% size map image bitmap from the image data that's stored in the database.
            bitmap100 = (Bitmap)Bitmap.FromStream(memoryStream);

            // Create the 50% and 25% bitmaps. Creating them from existing bitmaps is much faster than creating
            // them from scratch using the image data from the database. Experiments show no noticable difference
            // in the resulting images created this way versus creating them from the full size image data.
            bitmap50 = Utility.ScaledBitmap(bitmap100, size50, false);
            bitmap25 = Utility.ScaledBitmap(bitmap50, size25, false);

		    // Write the bitmaps to files.
            try
		    {
			    bitmap100.Save(fileLocation100, ImageFormat.Jpeg);
			    bitmap100.Dispose();

			    bitmap50.Save(fileLocation50, ImageFormat.Jpeg);
			    bitmap50.Dispose();

			    bitmap25.Save(fileLocation25, ImageFormat.Jpeg);
			    bitmap25.Dispose();
		    }
		    catch (Exception ex)
		    {
			    Utility.ReportException("TourImage.CreateFilesForMapImage", string.Format("{0} : {1}", mapImageFileLocation, bitmap100.Size.ToString()), ex);

			    // Every so often someone uploads an image that triggers a GDI error when it gets
			    // written to a file. Handle the error by substituting a good "bad image" image.
			    string badImageFileLocation = FileManager.WebAppFileLocationAbsolute("Images", "BadImage.jpg");
			    FileManager.CopyFile(badImageFileLocation, mapImageFileLocation);
		    }
        }
	}

	private void CreateMapTiles(Bitmap bitmap, StringBuilder mapTilesData)
	{
		// Note that a slight degradation of the map image quality can occur when creating the map tiles
		// because of the problem that jpegs lose quality every time they are saved. 

		Size tileSize = new Size(256, 256);
		
		int lastColumn = bitmap.Width / tileSize.Width;
		int lastColumnWidth = bitmap.Width - (lastColumn * tileSize.Width);
		if (lastColumnWidth > 0)
			lastColumn++;
		else
			lastColumnWidth = tileSize.Width;

		int lastRow = bitmap.Height / tileSize.Height;
		int lastRowHeight = bitmap.Height - (lastRow * tileSize.Height);
		if (lastRowHeight > 0)
			lastRow++;
		else
			lastRowHeight = tileSize.Height;

		ArrayList tiles = new ArrayList();

		for (int row = 1; row <= lastRow; row++)
		{
			for (int column = 1; column <= lastColumn; column++)
			{
				int width = column == lastColumn ? lastColumnWidth : tileSize.Width;
				int height = row == lastRow ? lastRowHeight : tileSize.Height;
				CreateMapTile(bitmap, tileSize, row, column, width, height, tiles);
			}
		}

		// Create a JavaScript array of the images as Base64 strings.
		bool firstTile = true;
		mapTilesData.Append("[");
		for (int index = 0; index < tiles.Count; index++)
		{
			if (firstTile)
				firstTile = false;
			else
				mapTilesData.Append(",\n");
			mapTilesData.Append(string.Format("'{0}'", (string)tiles[index]));
		}
		mapTilesData.Append("]");
	}

	private void CreateMapTile(Bitmap bitmap, Size tileSize, int row, int column, int width, int height, ArrayList tiles)
	{
		int rowOffset = row - 1;
		int columnOffset = column - 1;

		int x = columnOffset * tileSize.Width;
		int y = rowOffset * tileSize.Height;

		Rectangle rect = new Rectangle(x, y, width, height);
		Bitmap tile = bitmap.Clone(rect, bitmap.PixelFormat);

		if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed ||
			bitmap.PixelFormat == PixelFormat.Format4bppIndexed ||
			bitmap.PixelFormat == PixelFormat.Format1bppIndexed ||
			bitmap.PixelFormat == PixelFormat.Undefined ||
			bitmap.PixelFormat == PixelFormat.DontCare ||
			bitmap.PixelFormat == PixelFormat.Format16bppGrayScale ||
			bitmap.PixelFormat == PixelFormat.Format16bppArgb1555)
		{
			// Any of the above formats will trigger "a graphics object cannot be created from an
			// image that has an indexed pixel format" error.

			tile = new Bitmap(tile);
		}

		// Create a string containing the image data in Base64 format for use in the Data URI scheme.
		Byte[] bytes = Utility.ImageToByteArray(tile, ImageFormat.Jpeg);
		tiles.Add(Convert.ToBase64String(bytes));
	}

	public virtual void DeleteImageInDatabase()
	{
	}
	
	public void DeleteMapImagesFromPreviewFolder(int tourId)
	{
		DeleteMapImagesFromPreviewFolder(tourId, false);
		DeleteMapImagesFromPreviewFolder(tourId, true);
	}

	private void DeleteMapImagesFromPreviewFolder(int tourId, bool isMapInsetImage)
	{
		string oldFileNameWildcard = DeriveFileNameInternal(id, themeId, -1, "*");
		if (isMapInsetImage)
			oldFileNameWildcard = "_" + oldFileNameWildcard;
		string[] fileLocations = FileManager.FolderEntries(FileManager.PreviewFolderLocationAbsolute(tourId), oldFileNameWildcard);
		foreach (string fileLocation in fileLocations)
		{
			FileManager.DeleteFile(fileLocation);
		}
	}

	public static int GetNextIdForTour(int tourId)
	{
		int id;
		if (tourId == 0)
			id = 1;
		else
			id = (int)MapsAliveDatabase.ReadScalar("sp_Tour_GetNextImageId", "@TourId", tourId);
		return id;
	}

	public abstract void InsertImageIntoDatabase();

	public void KeepUploadedFile(int tourId)
	{
		wasUploaded = false;
		UpdateImageInDatabase();
	}

	public void Remove()
	{
		EraseBytes();
		DeleteImageInDatabase();
	}

	public void EraseBytes()
	{
		originalFileName = string.Empty;
		imageBytes = new Byte[0];
		length = 0;
		width = 0;
		height = 0;
		thumbnailBytes = new Byte[0];
		thumbnailSize = Size.Empty;
	}

	public void SetThumbnail(Byte[] bytes, Size size)
	{
		if (bytes.Length > 0)
		{
			thumbnailSize = size;
			thumbnailBytes = Utility.ScaledImageBytes(ref bytes, thumbnailSize);
		}
	}

	public Byte[] Thumbnail(Size requiredSize)
	{
		if (thumbnailSize != requiredSize)
		{
			SetThumbnail(Bytes, requiredSize);
		}
		return thumbnailBytes;
	}

	public abstract void UpdateImageInDatabase();

	public abstract void UpdateImageVersionInDatabase();

	public void Uploaded(int tourId, string fileName, Size size, Byte[] imageBytes)
	{
		wasUploaded = true;
		
		this.width = size.Width;
		this.height = size.Height;
		this.imageBytes = imageBytes;
		this.length = imageBytes.Length;
		this.thumbnailBytes = new Byte[0];
		this.thumbnailSize = Size.Empty;
		this.originalFileName = fileName;
		string ext = new FileInfo(fileName).Extension;
		this.fileExt = ext.Length > 1 ? ext.Substring(1) : ext;
	}
	#endregion

	#region ===== Protected =========================================================

	protected void InitializeFromDataRow(MapsAliveDataRow row)
	{
		version = row.IntValue("Version");
		versionBuilt = row.IntValue("VersionBuilt");
		tourImageId = row.IntValue("TourImageId");
		originalFileName = row.StringValue("OriginalFileName");
		fileExt = row.StringValue("FileExt");
		width = row.IntValue("Width");
		height = row.IntValue("Height");
		length = row.IntValue("Length");
		readyMapPackageId = row.IntValue("SampleId");
	}

	// These general methods allows us to call a specific stored procedure to insert a specific
	// kind of image into the database.  The database schema uses separate TourViewImage and
	// TourPageImage tables instead of a single generic table.  This compromise allows us
	// to use a single primary key (instead of a composite key) that links an image record to
	// its owner record (e.g. a TourViewImage record is linked to a TourView record and a TourPageImage
	// record is linked to Page).  This makes it much easier to enforce referential integrity
	// and allows us to use the database's cascading delete feature to automatically
	// clean up image records when their owner record is deleted.  These benefits might still
	// be achieved with a generic table and composite keys, but this scheme is simple and
	// efficient.  It does however require us to keep the various TourViewImage and TourPageImage tables
	// and their corresponding stored procedures in sync, but fortunately they won't change often.

	protected void ExecuteStoredProcedureForCreateImage(string storedProcedureName, int tourId, string keyName, int keyId)
	{
		Byte[] thumbnailBytes = new Byte[0];
		if (imageBytes.Length > 0 && usesThumbnail)
			thumbnailBytes = Utility.ScaledImageBytes(ref imageBytes, new Size(thumbnailDimension, thumbnailDimension));

		tourImageId = (int)MapsAliveDatabase.ReadScalar(storedProcedureName,
			"@TourId", tourId,
			keyName, keyId,
			"@ImageId", id,
			"@ThemeId", themeId,
			"@Version", version,
			"@OriginalFileName", originalFileName,
			"@FileExt",fileExt,
			"@Width", width,
			"@Height", height,
			"@Length", imageBytes.Length,
			"@Image", imageBytes,
			"@Thumbnail", thumbnailBytes
		);
	}

	protected void ExecuteStoredProcedureForDeleteImage(string storedProcedureName, string keyName, int keyId)
	{
		MapsAliveDatabase.ExecuteStoredProcedure(storedProcedureName,
			keyName, keyId,
			"@ImageId", id,
			"@ThemeId", themeId,
			"@TourImageId", tourImageId
		);

		// Now that the image is in the database, we can free its memory.
		imageBytes = new Byte[0];
	}

	protected void ExecuteStoredProcedureForUpdateImage(string storedProcedureName, string keyName, int keyId)
	{
		Byte[] thumbnailBytes = new Byte[0];
		
		if (usesThumbnail && imageBytes.Length > 0)
			thumbnailBytes = Utility.ScaledImageBytes(ref imageBytes, new Size(thumbnailDimension, thumbnailDimension));
		
		MapsAliveDatabase.ExecuteStoredProcedure(storedProcedureName,
			keyName, keyId,
			"@ImageId", id,
			"@ThemeId", themeId,
			"@Version", version,
			"@TourImageId", tourImageId,
			"@OriginalFileName", originalFileName,
			"@FileExt",fileExt,
			"@Width", width,
			"@Height", height,
			"@Length", imageBytes.Length,
			"@Image", imageBytes,
			"@Thumbnail", thumbnailBytes,
			"@SampleId", readyMapPackageId
		);

		// Now that the image is in the database, we can free its memory.
		imageBytes = new Byte[0];
	}

	protected void ExecuteStoredProcedureForUpdateImageVersion(string storedProcedureName, string keyName, int keyId)
	{
		MapsAliveDatabase.ExecuteStoredProcedure(storedProcedureName,
			keyName, keyId,
			"@ImageId", id,
			"@ThemeId", themeId,
			"@Version", version
		);
	}
	#endregion

	#region ===== Private ===========================================================

	private static string DeriveFileNameInternal(int imageId, int themeId, int version, string fileExt)
	{
		// Names of user-chosen images start at 0100 so that we can tell them apart from
		// our own system images which start at 0000.  We use leading zeros so that the
		// names have a consistent appearance and so that they sort numerically.  While
		// none of this matters functionally, it makes development and debugging easier.
		// If version is -1, we treat it as a wildcard.
		string ver = version == -1 ? "*" : string.Format("{0:0###}", version);
		return string.Format("{0:0###}_{1:0#}_{2:0###}.{3}", 100 + imageId, themeId, ver, fileExt);
	}
	#endregion
}
