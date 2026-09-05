using MsgPack;

namespace Messages;

public struct RequestFullCountPOIsReward
{
	public const uint TypeCode = 9031u;

	public string RegionId;

	public static void Pack(Packer packer, RequestFullCountPOIsReward val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(9031u);
		}
		else
		{
			packer.PackArrayHeader(1);
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

	public static RequestFullCountPOIsReward Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RequestFullCountPOIsReward result = default(RequestFullCountPOIsReward);
		result.RegionId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<RequestFullCountPOIsReward RegionId={RegionId}>";
	}
}
