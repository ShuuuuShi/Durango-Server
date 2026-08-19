using MsgPack;

namespace Messages;

public struct ItemUsed
{
	public const uint TypeCode = 18u;

	public string Motion;

	public float Time;

	public string Msg;

	public static void Pack(Packer packer, ItemUsed val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(18u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.Motion == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Motion);
		}
		packer.Pack(val.Time);
		packer.PackString(val.Msg);
	}

	public static ItemUsed Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ItemUsed result = default(ItemUsed);
		result.Motion = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Time = ((MessagePackObject)(ref lastReadData2)).AsSingle();
		unpacker.Read();
		result.Msg = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<ItemUsed Motion={Motion} Time={Time} Msg={Msg}>";
	}
}
