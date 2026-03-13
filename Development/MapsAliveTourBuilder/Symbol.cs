// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Diagnostics;
using System.Drawing;

public class Symbol : TourResource
{
	private Byte[] bytes;
	private int height;
	private string originalFileName;
	private int length;
	private int width;

	public Symbol()
	{
	}

	public Symbol(Account account)
	{
		accountId = account.Id;
		Name = CreateUniqueNameForNewResource(TourResourceType.Symbol); 
		bytes = new Byte[0];
		width = 0;
		height = 0;
		length = 0;
	}

	public Symbol(int symbolId)
	{
		if (LoadResourceRowFromDatabase(symbolId))
			InitializeResourceFromDataRecord(row);
	}

	public override void InitializeResourceFromDataRecord(MapsAliveDataRecord record)
	{
		bool isRow = record is MapsAliveDataRow;

		if (isRow)
		{
			bytes = record.ByteArrayValue("Image");
			length = record.IntValue("Length");
			originalFileName = record.StringValue("OriginalFileName");
		}
		
		height = record.IntValue(Tag.height);
		width = record.IntValue(Tag.width);
	}

	public enum Tag
	{
		id,
		name,
		key,
		width,
		height
	}

	public override string GetTagValue(int tagId)
	{
		Tag tag = (Tag)tagId;

		switch (tag)
		{
			case Tag.id:
				return Id.ToString();

			case Tag.name:
				return Name;

			case Tag.key:
				return Utility.Hash(Bytes);

			case Tag.width:
				return Size.Width.ToString();

			case Tag.height:
				return Size.Height.ToString();

			default:
				Debug.Fail("Unsupported Symbol XML tag requested " + tag);
				return "???";
		}
	}

	public override void SetTagValue(string tagName, string value)
	{
		Tag tag = (Tag)Enum.Parse(typeof(Tag), tagName);

		switch (tag)
		{
			case Tag.id:
				resourceId = int.Parse(value);
				break;

			case Tag.name:
				Name = value;
				break;

			case Tag.key:
				ResourceImageId = value;
				break;
			
			case Tag.width:
				Size = new Size(int.Parse(value), Size.Height);
				break;

			case Tag.height:
				Size = new Size(Size.Width, int.Parse(value));
				break;

			default:
				Debug.Fail("Unsupported Symbol XML tag requested " + tag);
				break;
		}
	}

	public override bool HasSameAppearanceAs(TourResource resource)
	{
		Symbol that = (Symbol)resource;

		if (this.bytes.GetHashCode() == that.bytes.GetHashCode())
			return true;

		if (this.bytes.Length != that.bytes.Length)
			return false;

		for (int i = 0; i < this.bytes.Length; i++)
		{
			if (this.bytes[i] != that.bytes[i])
				return false;
		}
		return true;
	}

	public override TourResourceType ResourceType
	{
		get { return TourResourceType.Symbol; }
	}

	public Byte[] Bytes
	{
		get	{ return bytes; }
	}

	public string FileNameOriginal
	{
		get { return originalFileName; }
	}

	public int Length
	{
		get { return length; }
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

	protected override Byte[] GenerateResourceImageBytes()
	{
		if (resourceId == 0)
		{
			string blankImageFileLocation = FileManager.WebAppFileLocationAbsolute("Images", "Blank.gif");
			Size size;
			return Utility.ImageFileToByteArray(blankImageFileLocation, out size);
		}
		else
		{
			const int maxDimension = 72;
			
			int w = width;
			int h = height;
			int x = 0;
			int y = 0;
			if (w > maxDimension || h > maxDimension)
			{
				// The original image is larger than the max size. Draw it to fit within the max size.
				w = maxDimension;
				h = maxDimension;
			}
			
			Bitmap opaqueBitmap = new Bitmap(width, height);
			Bitmap originalBitmap = Utility.BitmapFromBytes(Bytes);
			
			using (Graphics graphics = Graphics.FromImage(opaqueBitmap))
			{
				graphics.Clear(Color.White);
				Rectangle rect = new Rectangle(x, y, width, height);
				graphics.DrawImage(originalBitmap, rect);
			}

			Byte[] bytes = Utility.ImageToByteArray(opaqueBitmap, originalBitmap.RawFormat);

			return Utility.ScaledImageBytes(ref bytes, new Size(w, h));
		}
	}

	public void ImageUploaded(string fileName, Size size, Byte[] bytes)
	{
		this.Size = size;
		this.bytes = bytes;
		this.length = bytes.Length;
		this.originalFileName = fileName;
	}

	public override void InsertIntoDatabase(int accountId)
	{
		resourceId = (int)MapsAliveDatabase.ReadScalar("sp_Symbol_CreateSymbol", "@AccountId", accountId);
		UpdateDatabase();

		TourResource.CreateResourceImageFile(TourResourceType.Symbol, resourceId, string.Empty, ResourceImageFileAction.CreateNewFile);
	}

	public void LoadImageFromFile(string fileLocation)
	{
		Size size;
		bytes = Utility.ImageFileToByteArray(fileLocation, out size);
		height = size.Height;
		width = size.Width;
		length = bytes.Length;
	}

	public override void UpdateDatabase()
	{
		MapsAliveDatabase.ExecuteStoredProcedure("sp_Symbol_UpdateSymbol",
			"@SymbolId", resourceId,
			"@Name", Name,
			"@Image", bytes,
			"@Width", width,
			"@Height", height,
			"@Length", bytes.Length,
			"@OriginalFileName", originalFileName
		);
	}
}
