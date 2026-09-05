using MsgPack;

namespace Messages;

public struct GetInventoryInfos
{
	public const uint TypeCode = 108u;

	public PropKey? Target;

	public static void Pack(Packer packer, GetInventoryInfos val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(108u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (!val.Target.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			PropKey.Pack(packer, val.Target.Value);
		}
	}

	public static GetInventoryInfos Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetInventoryInfos result = default(GetInventoryInfos);
		if (unpacker.LastReadData.IsNil)
		{
			result.Target = null;
		}
		else
		{
			PropKey value = PropKey.Unpack(unpacker);
			result.Target = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<GetInventoryInfos Target={Target}>";
	}
}
