// Copyright (C) 2003-2010 AvantLogic Corporation
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

public enum ResourceImageFileAction
{
	CreateNewFile,
	UpdateExistingFile,
	CreateFileIfMissing
}

public enum ResourceDuplicateAction
{
	CreateNewResource,
	CopyExistingResource,
	ImportSystemResource
}

public abstract class TourResource
{
	public const string NoImageResourceImageId = "1";
	
	protected int accountId;
	protected MemberPageActionId explorerActionId;
	protected int resourceId;
	protected string resourceImageId;
	protected MapsAliveDataRow row;
	protected Byte[] _resourceImageBytes;

	public TourResource()
	{
		_resourceImageBytes = new Byte[0];
		resourceImageId = string.Empty;
	}

	public int AccountId
	{
		get { return accountId; }
		set { accountId = value; }
	}

	public int Id
	{
		get { return resourceId; }
		set { resourceId = value; }
	}

	public abstract void InitializeResourceFromDataRecord(MapsAliveDataRecord record);

	public string Name { get; set; }

	public string ResourceImageId
	{
		get	{ return resourceImageId; }
		set	{resourceImageId = value; }
	}

	public string Url
	{
		get { return ResourceImageUrl(ResourceType, resourceId, ResourceImageId); }
	}

	public virtual void AppearanceChanged()
	{
		Account.DeleteCachedResource(this);
	}

	public TourResource Clone()
	{
		return (TourResource)this.MemberwiseClone();
	}

	public static void CreateResourceImageFile(TourResourceType resourceType, int resourceId, string resourceImageId, ResourceImageFileAction action)
	{
		if (!TourResourceManager.HasResourceImageUrl(resourceType))
			return;

		string fileLocation;

		if (action == ResourceImageFileAction.CreateFileIfMissing)
		{
			fileLocation = ResourceImageFileLocation(resourceType, resourceImageId);
			if (FileManager.FileExists(fileLocation))
				return;
		}

		// Get the actual resource object so that we can access its image data.
		TourResource resource = Account.GetCachedResource(resourceType, resourceId);

		if (action == ResourceImageFileAction.CreateNewFile)
		{
			// Create a new image Id and write it to the database.
			resource.CreateResourceImageId();
			resource.UpdateResourceImageIdInDatabase();
		}
		else if (action == ResourceImageFileAction.UpdateExistingFile)
		{
			if (resourceImageId == NoImageResourceImageId)
			{
				// This resource does not need an image file.
				return;
			}

			string oldResourceImageId = resourceImageId;

			// Create a new image Id for the updated resource.
			resource.CreateResourceImageId();

			// If the image did not change, we don't need to update the file. This would be
			// the case, for example, if the resource's name changed, but not its appearance.
			if (oldResourceImageId == resource.ResourceImageId)
				return;

			// The image changed. Write the new Id to the database.
			resource.UpdateResourceImageIdInDatabase();

			// Delete the old image file, but only if no other resource still uses it.
			DeleteResourceImageFile(resourceType, resourceId, oldResourceImageId);
		}
			
		// Construct the file location for the new image.
		fileLocation = ResourceImageFileLocation(resourceType, resource.ResourceImageId);

		// Get the image data.
		Byte[] bytes = resource.ResourceImageBytes;
		
		// Create the image file.
		using (MemoryStream memoryStream = new MemoryStream(bytes))
		{
			Bitmap bitmap = (Bitmap)Bitmap.FromStream(memoryStream);
			bitmap.Save(fileLocation, ImageFormat.Png);
		}
	}

	public static void CreateResourceImageFileDiskCache(TourResourceType resourceType, Account account)
	{
		string folderLocation = ResourceImageFolderLocation(resourceType);
		if (!FileManager.FolderExists(folderLocation))
			FileManager.CreateFolder(folderLocation);

		string sp = string.Format("sp_{0}_Get{0}sOwnedByAccount", resourceType.ToString());
		DataTable dataTable = MapsAliveDatabase.LoadDataTable(sp, "@AccountId", account.Id);

		foreach (DataRow dataRow in dataTable.Rows)
		{
			MapsAliveDataRow row = new MapsAliveDataRow(dataRow);
			int resourceId = row.IntValue(string.Format("{0}Id", resourceType.ToString()));
			string resourceImageId = row.StringValue("ResourceImageId");

			if (resourceType == TourResourceType.Symbol && resourceImageId.Length == 0)
			{
				Utility.ReportError("CreateResourceImageFileDiskCache", "ResourceImageId is empty for resourceId " + resourceId);
			}
			
			// Check for the special "no symbol" symbol.
			bool isNoSymbolResource = resourceType == TourResourceType.Symbol && (resourceId == 0 || resourceImageId.Length == 0);
			
			if (resourceImageId == NoImageResourceImageId || isNoSymbolResource)
			{
				// This resource does not need an image file.
				continue;
			}

            try
            {
                // Create the image file if it does not already exist. The file won't exist if the disk cache
                // directory is emptied/deleted or if this is a pre version 3 resource that has no image Id yet.
                ResourceImageFileAction imageFileAction = resourceImageId == string.Empty ? ResourceImageFileAction.CreateNewFile : ResourceImageFileAction.CreateFileIfMissing;
			    CreateResourceImageFile(resourceType, resourceId, resourceImageId, imageFileAction);
            }
            catch
            {
                return;
            }
		}
	}

	public void CreateResourceImageId()
	{
		// Create an Id that uniquely identifies this image based on its content. Prior to version 3
		// we used the resource's Id and version number to constuct the name, but when we eliminated
		// shared system resources in favor of every user getting their own copies of system resources,
		// using the old scheme would have meant having thousands of identical files. By using the hash,
		// each user qets their own resources, but they share common image files.
		resourceImageId = Utility.Hash(ResourceImageBytes);
	}

	protected string CreateUniqueNameForNewResource(TourResourceType resourceType)
	{
		// Generate a name that is not already in use.
		int suffix = TourResource.GetCount(resourceType, accountId);
		string prefix = TourResourceManager.GetTitle(resourceType);
		string name;
		do
		{
			suffix++;
			name = string.Format("{0} {1}", prefix, suffix);
		} while (TourResource.NameInUse(resourceType, 0, name, accountId));

		return name;
	}

	public static string CreateUniqueResourceName(TourResourceType resourceType, string oldName)
	{
		const int maxNamePrefix = 56;

		if (oldName.Length > maxNamePrefix)
		{
			oldName = oldName.Substring(0, maxNamePrefix) + "...";
		}

		string newName = oldName + " (2)";

		if (NameInUse(resourceType, 0, newName, Utility.AccountId))
		{
			int copyNumber = 2;
			do
			{
				newName = string.Format("{0} ({1})", oldName, copyNumber);
				copyNumber++;
			} while (NameInUse(resourceType, 0, newName, Utility.AccountId));
		}

		return newName;
	}

	public static string CreateCopyOfResourceName(TourResourceType resourceType, string oldName)
	{
		string newName;

		if (oldName.ToLower().StartsWith("copy "))
		{
			newName = CreateUniqueResourceName(resourceType, oldName);
		}
		else
		{
			newName = "Copy of " + oldName;

			// Generate names like "Copy of Foo", "Copy 2 of Foo", "Copy 3 of Foo" ...
			if (NameInUse(resourceType, 0, newName, Utility.AccountId))
			{
				int index = newName.IndexOf(" of ");
				if (index > 0 && newName.Length > index + 5)
				{
					newName = newName.Substring(index + 4);
				}
			}

			if (NameInUse(resourceType, 0, newName, Utility.AccountId))
			{
				int copyNumber = 2;
				do
				{
					newName = string.Format("Copy {0} of {1}", copyNumber, oldName);
					copyNumber++;
				} while (Tour.TourNameInUse(newName));
			}
		}

		return newName;
	}

	public virtual void DeleteResource()
	{
		string sp = string.Format("sp_{0}_Delete", ResourceType.ToString());
		string idName = string.Format("@{0}Id", ResourceType.ToString());
		MapsAliveDatabase.ExecuteStoredProcedure(sp, idName, resourceId);
		Account.DeleteCachedResource(this);
		
		if (TourResourceManager.HasResourceImageUrl(ResourceType))
			DeleteResourceImageFile(ResourceType, resourceId, ResourceImageId);

		Account account = MapsAliveState.Account;
		if (account.LastResourceId(ResourceType) == resourceId)
			account.SetLastResourceId(ResourceType, 0);
	}

	public static void DeleteResourceImageFile(TourResourceType resourceType, int resourceId, string resourceImageId)
	{
		if (resourceImageId == string.Empty)
			return;

		string spName = string.Format("sp_{0}_CountUsingResourceImageId", resourceType.ToString());
		int usageCount = MapsAliveDatabase.GetCount(spName, "@ResourceImageId", resourceImageId);
		
		if (usageCount == 0)
		{
			string fileLocation = ResourceImageFileLocation(resourceType, resourceImageId);
			string tempFileLocation = fileLocation + "_";
			FileManager.RenameFile(fileLocation, tempFileLocation);

			// There's a slight possibility of a race condition whereby just as we "deleted" the file above, another user
			// created a resource with the same image Id. Test again for usage and restore the file if necessary.
			usageCount = MapsAliveDatabase.GetCount(spName, "@ResourceImageId", resourceImageId);
			if (usageCount > 0)
			{
				// Restore the file.
				FileManager.RenameFile(tempFileLocation, fileLocation);
			}
			else
			{
				// Now delete the file for real.
				FileManager.DeleteFile(tempFileLocation);
			}
		}
	}

	public static TourResource DuplicateResource(int accountId, TourResourceType resourceType, int resourceId, ResourceDuplicateAction duplicateAction)
	{
		// Get the resource that we are going to duplicate.
		TourResource oldResource = Account.GetCachedResource(resourceType, resourceId);
		
		string oldName = oldResource.Name;
		string newName = oldName;

		if (duplicateAction == ResourceDuplicateAction.ImportSystemResource)
		{
			// Strip off the leading asterisk from the default system resource.
			if (newName.StartsWith("*"))
				newName = newName.Substring(1);

			// If the account already has a resource with this name, don't make another copy.
			// We want to prevent the user from ending up with multiple copies if they ask to
			// import MapsAlive resources in the future. Of course if they rename the
			// resource, this logic won't work, but it's not intended to be a foolproof scheme.
			if (NameInUse(resourceType, 0, newName, accountId))
				return null;
		}

		// Clone the original resource. Explicity request the image bytes to force them to get generated on-the-fly.
		TourResource newResource = (TourResource)oldResource.MemberwiseClone();
		newResource.ResourceImageBytes = oldResource.ResourceImageBytes;
		newResource.AccountId = accountId;

		// Give non-system resources a unique name.
		if (duplicateAction == ResourceDuplicateAction.CopyExistingResource)
		{
			newName = CreateCopyOfResourceName(resourceType, oldName);
		}
		else if (duplicateAction == ResourceDuplicateAction.CreateNewResource)
		{
			newName = newResource.CreateUniqueNameForNewResource(resourceType);
		}

		newResource.Name = newName;
		return newResource;
	}

	public static TourResource DuplicateResourceInDatabase(int accountId, TourResourceType resourceType, int resourceId, ResourceDuplicateAction duplicateAction)
	{
		TourResource newResource = DuplicateResource(accountId, resourceType, resourceId, duplicateAction);
		
		if (newResource != null)
		{
			newResource.InsertIntoDatabase(accountId);
			newResource.UpdateDatabase();

			// UpdateDatabase does not set the resource image Id so we have to do it explicitly.
			newResource.UpdateResourceImageIdInDatabase();
		}
		
		return newResource;
	}

	protected virtual Byte[] GenerateResourceImageBytes()
	{
		// This method should only be called if overidden in a subclass. It is virtual instead
		// of abstract because not all TourResource subclasses have preview images.
		System.Diagnostics.Debug.Fail("GenerateResourceImageBytes was called in base class");
		return new Byte[0];
	}

	public static int GetCount(TourResourceType resourceType, int accountId)
	{
		string spName = string.Format("sp_{0}_Get{0}CountByAccountId", resourceType.ToString());
		return MapsAliveDatabase.GetCount(spName, "@AccountId", accountId);
	}

	public static string GetExplorerMessage(TourResourceType resourceType)
	{
		Debug.Assert(resourceType != TourResourceType.Undefined, "Resource type is undefined");
		int count = MapsAliveDatabase.GetCount(string.Format("sp_{0}_Get{0}CountByAccountId", resourceType.ToString()), "@AccountId", Utility.AccountId);
		string message = count == 0 ? "To create a {0}, click New > Resource > {0}" : "";
		return string.Format(message, TourResourceManager.GetTitle(resourceType));
	}

	public static string GetTitleForAddPage(TourResourceType resourceType)
	{
		return string.Format("Add {0}", TourResourceManager.GetTitle(resourceType));
	}

	public static string GetTitleForEditPage(TourResourceType resourceType)
	{
		return string.Format("Edit {0}", TourResourceManager.GetTitle(resourceType));
	}

	public static string GetTitleForExplorerPage(TourResourceType resourceType)
	{
		return string.Format("{0} Library", TourResourceManager.GetTitle(resourceType));
	}

	public virtual string GetTagValue(int tagId)
	{
		Debug.Fail("GetTagValue base class was called");
		return null;
	}

	public abstract bool HasSameAppearanceAs(TourResource resource);

	public abstract void InsertIntoDatabase(int accountId);

	private void InvalidateDependentsOfResource()
	{
		TourResourceDependencyWalker walker = new TourResourceDependencyWalker();
		walker.InvalidateDependentsOfResource(this);
	}

	protected bool LoadResourceRowFromDatabase(int id)
	{
		// Construct the stored procedure name that loads this resource from the database.
		string sp = string.Format("sp_{0}_Get{0}", ResourceType.ToString());
		string idName = string.Format("{0}Id", ResourceType.ToString());

		// Read the record.
		row = MapsAliveDatabase.LoadDataRow(sp, idName, id);
		if (row == null)
		{
			// This can happen if a user deletes a resource and then uses the Back button to
			// return to a screen that was referencing that resource via a query string Id.
			return false;
		}

		// Update this object with information from the record.
		this.resourceId = id;
		this.accountId = row.IntValue("AccountId");
		
		// Some resources (Category) don't have images.
		if (row.HasColumn("ResourceImageId"))
			this.resourceImageId = row.StringValue("ResourceImageId");
	
		// Some resources have a Title instead of a name, so we check before reading the name.
		if (row.HasColumn("Name"))
			this.Name = row.StringValue("Name");

		return true;
	}

	public static bool NameInUse(TourResourceType resourceType, int id, string name, int accountId)
	{
		string spName = string.Format("sp_{0}_Get{0}ExistsByName", resourceType.ToString());
		string idName = string.Format("{0}Id", resourceType.ToString());
		return MapsAliveDatabase.GetCount(spName, "@AccountId", accountId, idName, id, "@Name", name) != 0;
	}

	public Byte[] ResourceImageBytes
	{
		get
		{
			if (_resourceImageBytes.Length == 0)
			{
				_resourceImageBytes = GenerateResourceImageBytes();
			}
			if (_resourceImageBytes.Length == 0)
			{
				string missingImageFileLocation = FileManager.WebAppFileLocationAbsolute("Images", "MissingSlideImage.gif");
				Size size;
				_resourceImageBytes = Utility.ImageFileToByteArray(missingImageFileLocation, out size);
			}
			return _resourceImageBytes;
		}
		set { _resourceImageBytes = value; }
	}

	public static string ResourceImageFileLocation(TourResourceType resourceType, string resourceImageId)
	{
		Debug.Assert(resourceImageId != string.Empty && resourceImageId != NoImageResourceImageId, "Unexpected resourceImageId");
		string fileName = resourceImageId;
		return string.Format("{0}\\{1}.png", ResourceImageFolderLocation(resourceType), fileName);
	}

	public static string ResourceImageFolderLocation(TourResourceType resourceType)
	{
		string folderName = string.Format("\\{0}s", resourceType.ToString());
		return FileManager.AppRuntimeFolderLocationAbsolute + folderName;
	}

	public static string ResourceImageUrl(TourResourceType resourceType, int id, string resourceImageId)
	{
		string url;
		
		if (resourceImageId == string.Empty)
			url = App.WebSitePathUrl("Images/Blank.gif");
		else if (resourceImageId == NoImageResourceImageId)
			url = App.WebSitePathUrl("Images/Shape.gif");
		else
			url = string.Format("{0}{1}s/{2}.png", App.AppRuntimeUrl, resourceType.ToString(), resourceImageId);

		return url;
	}

	public abstract TourResourceType ResourceType { get; }

	public virtual void SetTagValue(string tagName, string value)
	{
		Debug.Fail("SetTagValue base class was called");
	}

	public abstract void UpdateDatabase();

	public void UpdateResource(TourResource resourceBeforeEdit)
	{
		if (resourceBeforeEdit == null)
		{
			Debug.Fail("UpdateResource was called with null resource");
			return;
		}

		// This method gets called when a user saves changes to a resource. When the changes alter the
		// resource's appearance, other objects (resources, maps, tours) that use the changed resource
		// have to be updated as well. For example, if a font style changes, any markers or tooltip
		// style that use it have to be updated. In turn the hotspots and maps that use the markers and
		// tooltips have to be updated. The resource image files for the font style and markers have to
		// be updated too. In short, making even a minor change to a single resource can have a ripple
		// effect that is expensive to excute right now to perform all the updates, and later when the
		// user works with any tours having maps that depend on the resource because those maps will get
		// rebuilt on-the-fly the next time the user works with the map or goes to Tour Preview.
		//
		// The purpose of this method, and the HasSameAppearanceAs methods that each resource provides,
		// is to avoid the ripple if the user only changes the resource's name or other property that
		// does not change the resource's appearance. If only the name changed, we only update the
		// resource in the database and do not trigger the logic to update dependents.

		if (this.HasSameAppearanceAs(resourceBeforeEdit))
		{
			UpdateDatabase();
			Account.DeleteCachedResource(this);
		}
		else
		{
			UpdateResourceAndDependents();
		}
	}

	public void UpdateResourceAndDependents()
	{
		AppearanceChanged();
		UpdateDatabase();
		TourResource.CreateResourceImageFile(ResourceType, resourceId, ResourceImageId, ResourceImageFileAction.UpdateExistingFile);
		InvalidateDependentsOfResource();
	}

	public void UpdateResourceImageIdInDatabase()
	{
		string spName = string.Format("sp_{0}_UpdateResourceImageId", ResourceType);
		string idName = string.Format("{0}Id", ResourceType.ToString());
		MapsAliveDatabase.ExecuteStoredProcedure(spName, idName, resourceId, "@ResourceImageId", ResourceImageId);
	}
}
