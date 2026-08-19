using MsgPack;

namespace Messages;

public struct GetDiscoverDistances
{
	public const uint TypeCode = 2310u;

	public string RegionId;

	public static void Pack(Packer packer, GetDiscoverDistances val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2310u);
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

	public static GetDiscoverDistances Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetDiscoverDistances result = default(GetDiscoverDistances);
		result.RegionId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<GetDiscoverDistances RegionId={RegionId}>";
	}
}
