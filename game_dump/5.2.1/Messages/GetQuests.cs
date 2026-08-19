using MsgPack;

namespace Messages;

public struct GetQuests
{
	public const uint TypeCode = 237918u;

	public string Category;

	public static void Pack(Packer packer, GetQuests val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(237918u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Category == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Category);
		}
	}

	public static GetQuests Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetQuests result = default(GetQuests);
		result.Category = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<GetQuests Category=" + Category + ">";
	}
}
