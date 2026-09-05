using MsgPack;

namespace Messages;

public struct ReceiveAdvisorReward
{
	public const uint TypeCode = 3908u;

	public string TitleId;

	public static void Pack(Packer packer, ReceiveAdvisorReward val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3908u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.TitleId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TitleId);
		}
	}

	public static ReceiveAdvisorReward Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ReceiveAdvisorReward result = default(ReceiveAdvisorReward);
		result.TitleId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<ReceiveAdvisorReward TitleId={TitleId}>";
	}
}
