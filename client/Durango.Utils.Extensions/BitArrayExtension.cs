using System.Collections;
using System.Text;

namespace Durango.Utils.Extensions;

public static class BitArrayExtension
{
	public static string AsString(this BitArray bits)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < bits.Count; i++)
		{
			char value = ((!bits[i]) ? '0' : '1');
			stringBuilder.Append(value);
		}
		return stringBuilder.ToString();
	}
}
