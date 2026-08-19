using System.Collections.Generic;

namespace Durango.Utils;

public class TimeSequenceItemComparer<T> : IComparer<T> where T : ITimeSequenceItem
{
	public int Compare(T x, T y)
	{
		float num = x?.At ?? 0f;
		float value = y?.At ?? 0f;
		return num.CompareTo(value);
	}
}
