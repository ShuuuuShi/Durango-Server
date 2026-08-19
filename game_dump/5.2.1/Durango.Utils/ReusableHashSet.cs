using System.Collections.Generic;

namespace Durango.Utils;

public class ReusableHashSet<T> : Reusable<HashSet<T>>
{
	public static Reusable<HashSet<T>> Pop()
	{
		Reusable<HashSet<T>> reusable = Reusable<HashSet<T>>.DoPop();
		reusable.Value.Clear();
		return reusable;
	}
}
