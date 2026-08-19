using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct ArtifactRepairCost
{
	public const uint TypeCode = 2054u;

	public KeyValuePair<int, int> CostRange;

	public static void Pack(Packer packer, ArtifactRepairCost val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2054u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.PackArrayHeader(2);
		packer.Pack(val.CostRange.Key);
		packer.Pack(val.CostRange.Value);
	}

	public static ArtifactRepairCost Unpack(Unpacker unpacker)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int key = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int value = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		ArtifactRepairCost result = default(ArtifactRepairCost);
		result.CostRange = new KeyValuePair<int, int>(key, value);
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactRepairCost CostRange={CostRange}>";
	}
}
