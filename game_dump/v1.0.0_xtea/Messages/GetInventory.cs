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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		GetInventory result = default(GetInventory);
		if (((MessagePackObject)(ref lastReadData)).IsNil)
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
