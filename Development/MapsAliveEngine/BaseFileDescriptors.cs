// Copyright (C) 2003-2005 AvantLogic Corporation
using System;
using System.Collections;

namespace AvantLogic.MapsAlive.Engine
{
	public class FileDescriptorComparer : IComparer  
	{
		int IComparer.Compare(Object o1, Object o2)  
		{
			int id1 = ((BaseFileDescriptor)o1).Id;
			int id2 = ((BaseFileDescriptor)o2).Id;
			return id1 - id2;
		}
	}

	public class BaseFileDescriptors : IEnumerable
	{
		private ArrayList collection = new ArrayList();

		public BaseFileDescriptors()
		{
		}

		public void Release()
		{
			for (int index = 0; index < collection.Count; index++)
			{
				((BaseFileDescriptor)collection[index]).Release();
				collection[index] = null;
			}
		}

		#region ===== Properties ========================================================
		
		public int Count
		{
			get { return collection.Count; }
		}
		#endregion

		#region ===== Public methods ====================================================
		
		// This method provides array-like access to the collection.
		public BaseFileDescriptor this[int index]
		{
			get	{ return (BaseFileDescriptor)collection[index]; }
		}

		public void Add(BaseFileDescriptor fileDescriptor)
		{
			collection.Add(fileDescriptor);
		}

		public BaseFileDescriptor GetFileDescriptorById(int id)
		{
			foreach (BaseFileDescriptor fileDescriptor in collection)
			{
				if (fileDescriptor.Id == id)
					return fileDescriptor;
			}
			return null;
		}

		public BaseFileDescriptor GetFileDescriptorByFileLocation(string fileLocation)
		{
			foreach (BaseFileDescriptor fileDescriptor in collection)
			{
				if (BaseUtility.SameString(fileDescriptor.FileLocation, fileLocation))
					return fileDescriptor;
			}
			return null;
		}

		public int NewId()
		{
			// Start looking for an available Id starting with 1 so that we keep reusing lower numbers instead
			// of having the Ids get bigger and bigger.  Note that not all file descriptors get written to the
			// project XML file (e.g. the descriptor for the project file) and therefore some numbers appear to
			// not be used, but they are while the app is running.
			int id = 1;
			while (GetFileDescriptorById(id) != null)
				id++;
			return id;
		}

		public void Remove(BaseFileDescriptor fileDescriptor)
		{
			collection.Remove(fileDescriptor);
		}

		public void Sort()
		{
			collection.Sort(new FileDescriptorComparer());
		}
		#endregion

		#region ===== Classes and methods required to support enumeration ===============

		public IEnumerator GetEnumerator()
		{
			return new FileDescriptorEnumerator(this);
		}

		private class FileDescriptorEnumerator : IEnumerator
		{
			private int position = -1;
			private BaseFileDescriptors fileDescriptors;

			public FileDescriptorEnumerator(BaseFileDescriptors fileDescriptors)
			{
				this.fileDescriptors = fileDescriptors;
			}

			public bool MoveNext()
			{
				if (position < fileDescriptors.collection.Count - 1)
				{
					position++;
					return true;
				}
				else
				{
					return false;
				}
			}

			public void Reset()
			{
				position = -1;
			}

			public object Current
			{
				get
				{
					return fileDescriptors.collection[position];
				}
			}
		}
		#endregion
	}
}
