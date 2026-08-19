using MsgPack;

namespace Messages;

public struct SuggestBreak
{
	public const uint TypeCode = 9138748u;

	public string ClanId;

	public static void Pack(Packer packer, SuggestBreak val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(9138748u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.ClanId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ClanId);
		}
	}

	public static SuggestBreak Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SuggestBreak result = default(SuggestBreak);
		result.ClanId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<SuggestBreak ClanId={ClanId}>";
	}
}
