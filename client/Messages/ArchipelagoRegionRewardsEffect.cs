using MsgPack;
using Shared.System;

namespace Messages;

public struct ArchipelagoRegionRewardsEffect
{
	public const uint TypeCode = 240003u;

	public Shared.System.RewardEffect Type;

	public string ArchipelagoId;

	public string RegionId;

	public static void Pack(Packer packer, ArchipelagoRegionRewardsEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(240003u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack((int)val.Type);
		if (val.ArchipelagoId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ArchipelagoId);
		}
		if (val.RegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RegionId);
		}
	}

	public static ArchipelagoRegionRewardsEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ArchipelagoRegionRewardsEffect result = default(ArchipelagoRegionRewardsEffect);
		if (num < 0 || 23 < num)
		{
			result.Type = Shared.System.RewardEffect.Invalid;
		}
		else
		{
			result.Type = (Shared.System.RewardEffect)num;
		}
		unpacker.Read();
		result.ArchipelagoId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.RegionId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<ArchipelagoRegionRewardsEffect Type={Type} ArchipelagoId={ArchipelagoId} RegionId={RegionId}>";
	}
}
