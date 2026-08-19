using MsgPack;

namespace Messages;

public struct RefuseSuggestion
{
	public const uint TypeCode = 9138750u;

	public string ClanId;

	public static void Pack(Packer packer, RefuseSuggestion val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(9138750u);
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

	public static RefuseSuggestion Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RefuseSuggestion result = default(RefuseSuggestion);
		result.ClanId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<RefuseSuggestion ClanId=" + ClanId + ">";
	}
}
