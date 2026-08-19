using MsgPack;

namespace Messages;

public struct GetPOICount
{
	public const uint TypeCode = 900u;

	public string RegionId;

	public static void Pack(Packer packer, GetPOICount val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(900u);
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

	public static GetPOICount Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetPOICount result = default(GetPOICount);
		result.RegionId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<GetPOICount RegionId={RegionId}>";
	}
}
