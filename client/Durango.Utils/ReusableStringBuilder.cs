using System.Text;

namespace Durango.Utils;

public class ReusableStringBuilder : Reusable<StringBuilder>
{
	public static Reusable<StringBuilder> Pop()
	{
		Reusable<StringBuilder> reusable = Reusable<StringBuilder>.DoPop();
		reusable.Value.Length = 0;
		return reusable;
	}
}
