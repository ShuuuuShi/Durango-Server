using MsgPack;

namespace Messages;

public struct AcceptedSupportRewards
{
	public const uint TypeCode = 2509347u;

	public SupportRewards Rewards;

	public SupportRewards RandomRewards;

	public SupportRequestUpdated UpdatedInfo;

	public static void Pack(Packer packer, AcceptedSupportRewards val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2509347u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		SupportRewards.Pack(packer, val.Rewards);
		SupportRewards.Pack(packer, val.RandomRewards);
		SupportRequestUpdated.Pack(packer, val.UpdatedInfo);
	}

	public static AcceptedSupportRewards Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AcceptedSupportRewards result = default(AcceptedSupportRewards);
		result.Rewards = SupportRewards.Unpack(unpacker);
		unpacker.Read();
		result.RandomRewards = SupportRewards.Unpack(unpacker);
		unpacker.Read();
		result.UpdatedInfo = SupportRequestUpdated.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<AcceptedSupportRewards Rewards={Rewards} RandomRewards={RandomRewards} UpdatedInfo={UpdatedInfo}>";
	}
}
