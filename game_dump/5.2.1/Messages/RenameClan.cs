using MsgPack;

namespace Messages;

public struct RenameClan
{
	public const uint TypeCode = 36510u;

	public string ClanName;

	public static void Pack(Packer packer, RenameClan val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(36510u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.ClanName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ClanName);
		}
	}

	public static RenameClan Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RenameClan result = default(RenameClan);
		result.ClanName = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<RenameClan ClanName=" + ClanName + ">";
	}
}
