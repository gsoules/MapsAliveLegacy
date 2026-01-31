// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Telerik.Web.UI;
using Telerik.Web.UI.Upload;
public abstract class ImageUploadPage : MemberPage
{
	protected bool badFileName;
	protected string badFileNameMessage;
	
	protected override PageUsage PageUsageType()
	{
		return PageUsage.TourBuilder;
	}

	protected void ImportImageFromUploadedFile(Size maxSize, bool convertToJpg)
	{
		SetPageSpecialWarning(string.Empty);
		
		RadUploadContext radUploadContext = RadUploadContext.Current;

		if (radUploadContext.UploadedFiles.Count == 1)
		{
			UploadedFile uploadedFile = radUploadContext.UploadedFiles[0];

			ImageFormat imageFormat;	
			Bitmap bitmap = Utility.BitmapFromStream(uploadedFile.InputStream, out imageFormat, out badFileNameMessage);
				
			if (bitmap == null)
			{
				SetPageSpecialWarning(Resources.Text.ImageUploadError);
				badFileName = true;
			}
			else
			{
				// Convert the image to bytes, scaling it down to the max size if necessary.
				// Note that if the image is scaled, maxSize will be updated with the scaled
				// size which it why we pass it, not bitmap.size, to ImageUploaded.
				Byte[] imageBytes = Utility.BytesFromUploadedBitmap(bitmap, convertToJpg ? ImageFormat.Jpeg : imageFormat, ref maxSize);

				ImageUploaded(uploadedFile.GetName(), maxSize, imageBytes);
			}
		}
		else
		{
			badFileNameMessage = "No file selected";
			badFileName = true;
		}

		if (badFileName)
			InitPageControls();
	}

	protected abstract void ImageUploaded(string fileName, Size size, Byte[] bytes);
}
