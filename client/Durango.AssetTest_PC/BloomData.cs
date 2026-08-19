using System;

namespace Durango.AssetTest_PC;

[Serializable]
public struct BloomData
{
	public SimpleBloom Default;

	public SimpleBloom Modified;

	public float StartModifyingTime;

	public float ModifyingTimeLength;
}
