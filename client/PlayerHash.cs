using System.Runtime.CompilerServices;

[CompilerGenerated]
internal sealed class PlayerHash
{
	internal static uint ComputeStringHash(string s)
	{
		uint num = default(uint);
		if (s != null)
		{
			num = 2166136261u;
			for (int i = 0; i < s.Length; i++)
			{
				num = (s[i] ^ num) * 16777619;
			}
		}
		return num;
	}
}
