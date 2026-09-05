using MsgPack;

namespace Messages;

public struct GetInventoryItems
{
	public const uint TypeCode = 106u;

	public PropKey? Target;

	public static void Pack(Packer packer, GetInventoryItems val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(106u);
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

	public static GetInventoryItems Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetInventoryItems result = default(GetInventoryItems);
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
		return $"<GetInventoryItems Target={Target}>";
	}
}
