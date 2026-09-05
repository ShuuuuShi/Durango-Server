using MsgPack;

namespace Messages;

public struct SharedSheet
{
	public const uint TypeCode = 47852650u;

	public string SheetId;

	public static void Pack(Packer packer, SharedSheet val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(47852650u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.SheetId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SheetId);
		}
	}

	public static SharedSheet Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SharedSheet result = default(SharedSheet);
		result.SheetId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<SharedSheet SheetId={SheetId}>";
	}
}
