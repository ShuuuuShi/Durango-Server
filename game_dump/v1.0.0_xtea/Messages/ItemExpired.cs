using MsgPack;

namespace Messages;

public struct ItemExpired
{
	public const uint TypeCode = 3714u;

	public ulong ItemId;

	public string Text;

	public static void Pack(Packer packer, ItemExpired val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3714u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.ItemId);
		packer.PackString(val.Text);
	}

	public static ItemExpired Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ItemExpired result = default(ItemExpired);
		result.ItemId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		result.Text = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<ItemExpired ItemId={ItemId} Text={Text}>";
	}
}
