using System;
using System.Collections;
using Majestic12;

namespace AvantLogic
{
	public class HtmlParser
	{
		private HTMLparser parser;

		public HtmlParser(string html)
		{
			parser = new HTMLparser(html);
			parser.SetChunkHashMode(true);
			parser.bKeepRawHTML = true;
			parser.bAutoKeepComments = false;
			parser.bAutoKeepScripts = false;
			parser.bCompressWhiteSpaceBeforeTag = true;
			parser.bAutoMarkClosedTagsWithParamsAsOpen = false;
		}

		public Tag Parse()
		{
			HTMLchunk chunk;
			chunk = parser.ParseNext();
			if (chunk == null)
				return null;

			Tag tag = new Tag(chunk);
			return tag;
		}			
	}
}