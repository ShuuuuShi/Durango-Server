using System.Collections.Generic;
using System.Linq;

namespace Durango.Utils.Extensions;

public static class HashSetExtension
{
	public static int AddRange<T>(this HashSet<T> source, IEnumerable<T> items)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (items == null)
		{
			throw Error.ArgumentNull("predicate");
		}
		return items.Count(source.Add);
	}
}
