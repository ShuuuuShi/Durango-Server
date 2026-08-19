using MsgPack;

namespace Messages;

public struct ExploredPOIs
{
	public const uint TypeCode = 903u;

	public PointOfInterest[] POIs;

	public bool FullCountRewarded;

	public Cost? RewardCost;

	public bool IsOpenedMap;

	public static void Pack(Packer packer, ExploredPOIs val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(903u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.POIs == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.POIs.Length);
			for (int i = 0; i < val.POIs.Length; i++)
			{
				PointOfInterest.Pack(packer, val.POIs[i]);
			}
		}
		packer.Pack(val.FullCountRewarded);
		if (!val.RewardCost.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Cost.Pack(packer, val.RewardCost.Value);
		}
		packer.Pack(val.IsOpenedMap);
	}

	public static ExploredPOIs Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ExploredPOIs result = default(ExploredPOIs);
		result.POIs = new PointOfInterest[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref PointOfInterest reference = ref result.POIs[i];
			reference = PointOfInterest.Unpack(unpacker);
		}
		unpacker.Read();
		result.FullCountRewarded = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RewardCost = null;
		}
		else
		{
			Cost value = Cost.Unpack(unpacker);
			result.RewardCost = value;
		}
		unpacker.Read();
		result.IsOpenedMap = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<ExploredPOIs POIs={POIs} FullCountRewarded={FullCountRewarded} RewardCost={RewardCost} IsOpenedMap={IsOpenedMap}>";
	}
}
