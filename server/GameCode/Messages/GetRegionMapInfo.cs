using MsgPack;

namespace Messages;

public struct GetRegionMapInfo
{
	public const uint TypeCode = 205u;

	public string RegionId;

	public static void Pack(Packer packer, GetRegionMapInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(205u);
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

	public static GetRegionMapInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetRegionMapInfo result = default(GetRegionMapInfo);
		result.RegionId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<GetRegionMapInfo RegionId={RegionId}>";
	}
}
