using System.Collections.Generic;

namespace Durango.Utils;

public class ReusableList<T> : Reusable<List<T>>
{
	public static Reusable<List<T>> Pop()
	{
		Reusable<List<T>> reusable = Reusable<List<T>>.DoPop();
		reusable.Value.Clear();
		return reusable;
	}
}
