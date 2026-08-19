using MsgPack;

namespace Messages;

public struct GetInventory
{
	public const uint TypeCode = 2010u;

	public PropKey? Target;

	public static void Pack(Packer packer, GetInventory val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2010u);
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

	public static GetInventory Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetInventory result = default(GetInventory);
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
		return $"<GetInventory Target={Target}>";
	}
}
