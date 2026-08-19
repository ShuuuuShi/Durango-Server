using MsgPack;

namespace Messages;

public struct UseItem
{
	public const uint TypeCode = 17u;

	public ulong ItemId;

	public static void Pack(Packer packer, UseItem val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(17u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.ItemId);
	}

	public static UseItem Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		UseItem result = default(UseItem);
		result.ItemId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<UseItem ItemId={ItemId}>";
	}
}
