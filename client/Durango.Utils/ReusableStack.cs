using System.Collections.Generic;

namespace Durango.Utils;

public class ReusableStack<T> : Reusable<Stack<T>>
{
	public static Reusable<Stack<T>> Pop()
	{
		Reusable<Stack<T>> reusable = Reusable<Stack<T>>.DoPop();
		reusable.Value.Clear();
		return reusable;
	}
}
