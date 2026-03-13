// Copyright (C) 2003-2008 AvantLogic Corporation
using System;
using Majestic12;

namespace AvantLogic
{
	public class Tag
	{
		private HTMLchunk chunk;
		private const string CrLf = "\r\n";

		public Tag(HTMLchunk chunk)
		{
			this.chunk = chunk;
		}

		public string AttributeValue(string name)
		{
			string value = chunk.GetParamValue(name);
			if (value.Contains(CrLf))
			{
				// It's unusual for an attribute to contain a new line, but sometimes
				// Adobe Illustrator emits them instead of a space.
				value = value.Replace(CrLf, " ");
			}
			return value;
		}

		public bool HasAttributes
		{
			get { return chunk.iParams > 0; }
		}

		public string Html
		{
			get { return chunk.oHTML; }
		}

		public bool IsOpenTag
		{
			get { return chunk.oType == HTMLchunkType.OpenTag; }
		}

		public string Name
		{
			get { return chunk.sTag; }
		}

		public bool NameIs(string name)
		{
			return Name.ToLower() == name.ToLower();
		}

		public int Length
		{
			get { return chunk.iChunkLength; }
		}

		public int Offset
		{
			get { return chunk.iChunkOffset; }
		}
	}
}
