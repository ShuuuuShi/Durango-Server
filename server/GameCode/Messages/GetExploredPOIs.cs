using MsgPack;

namespace Messages;

public struct GetExploredPOIs
{
	public const uint TypeCode = 902u;

	public string RegionId;

	public static void Pack(Packer packer, GetExploredPOIs val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(902u);
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

	public static GetExploredPOIs Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetExploredPOIs result = default(GetExploredPOIs);
		result.RegionId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<GetExploredPOIs RegionId={RegionId}>";
	}
}
