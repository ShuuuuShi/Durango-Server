using MsgPack;

namespace Messages;

public struct GetSharedMusic
{
	public const uint TypeCode = 47852457u;

	public string SheetId;

	public static void Pack(Packer packer, GetSharedMusic val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(47852457u);
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

	public static GetSharedMusic Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetSharedMusic result = default(GetSharedMusic);
		result.SheetId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<GetSharedMusic SheetId=" + SheetId + ">";
	}
}
