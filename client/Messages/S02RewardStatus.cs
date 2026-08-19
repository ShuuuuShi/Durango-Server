using MsgPack;

namespace Messages;

public struct S02RewardStatus
{
	public int Level;

	public int Count;

	public int RewardedLevel;

	public static void Pack(Packer packer, S02RewardStatus val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.Pack(val.Level);
		packer.Pack(val.Count);
		packer.Pack(val.RewardedLevel);
	}

	public static S02RewardStatus Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		S02RewardStatus result = default(S02RewardStatus);
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Count = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.RewardedLevel = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<S02RewardStatus Level={Level} Count={Count} RewardedLevel={RewardedLevel}>";
	}
}
