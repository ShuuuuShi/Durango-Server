using System.Collections.Generic;

namespace Durango.Utils;

public class ReusableDictionary<T, V> : Reusable<Dictionary<T, V>>
{
	public static Reusable<Dictionary<T, V>> Pop()
	{
		Reusable<Dictionary<T, V>> reusable = Reusable<Dictionary<T, V>>.DoPop();
		reusable.Value.Clear();
		return reusable;
	}
}
