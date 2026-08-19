using MsgPack;

namespace Messages;

public struct MissionBonusReward
{
	public RewardInfo Rewards;

	public int LeftCount;

	public int MaxCount;

	public double ValidUntil;

	public static void Pack(Packer packer, MissionBonusReward val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		RewardInfo.Pack(packer, val.Rewards);
		packer.Pack(val.LeftCount);
		packer.Pack(val.MaxCount);
		packer.Pack(val.ValidUntil);
	}

	public static MissionBonusReward Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		MissionBonusReward result = default(MissionBonusReward);
		result.Rewards = RewardInfo.Unpack(unpacker);
		unpacker.Read();
		result.LeftCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.MaxCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.ValidUntil = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<MissionBonusReward Rewards={Rewards} LeftCount={LeftCount} MaxCount={MaxCount} ValidUntil={ValidUntil}>";
	}
}
