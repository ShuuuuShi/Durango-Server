using MsgPack;

namespace Messages;

public struct CustomQuestEvent
{
	public const uint TypeCode = 312798u;

	public string Keyword;

	public static void Pack(Packer packer, CustomQuestEvent val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(312798u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Keyword == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Keyword);
		}
	}

	public static CustomQuestEvent Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CustomQuestEvent result = default(CustomQuestEvent);
		result.Keyword = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<CustomQuestEvent Keyword={Keyword}>";
	}
}
