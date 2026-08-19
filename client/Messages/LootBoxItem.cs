using MsgPack;

namespace Messages;

public struct LootBoxItem
{
	public const uint TypeCode = 29875325u;

	public string RewardId;

	public static void Pack(Packer packer, LootBoxItem val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(29875325u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.RewardId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RewardId);
		}
	}

	public static LootBoxItem Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		LootBoxItem result = default(LootBoxItem);
		result.RewardId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<LootBoxItem RewardId={RewardId}>";
	}
}
