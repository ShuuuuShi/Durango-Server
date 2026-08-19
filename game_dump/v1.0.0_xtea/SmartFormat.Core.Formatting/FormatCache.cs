using System.Collections.Generic;
using SmartFormat.Core.Parsing;

namespace SmartFormat.Core.Formatting;

public class FormatCache
{
	private Dictionary<string, object> cachedObjects;

	public Format Format { get; private set; }

	public Dictionary<string, object> CachedObjects => cachedObjects ?? (cachedObjects = new Dictionary<string, object>());

	public FormatCache(Format format)
	{
		Format = format;
	}
}
