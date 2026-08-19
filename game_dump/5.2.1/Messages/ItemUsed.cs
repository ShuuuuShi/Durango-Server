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
		unpacker.Read();
		ItemUsed result = default(ItemUsed);
		result.Motion = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Time = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.Msg = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<ItemUsed Motion={Motion} Time={Time} Msg={Msg}>";
	}
}
