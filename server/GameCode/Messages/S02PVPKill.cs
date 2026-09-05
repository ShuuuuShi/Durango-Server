using MsgPack;

namespace Messages;

public struct S02PVPKill
{
	public const uint TypeCode = 222210u;

	public string VictimName;

	public static void Pack(Packer packer, S02PVPKill val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(222210u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.VictimName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.VictimName);
		}
	}

	public static S02PVPKill Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		S02PVPKill result = default(S02PVPKill);
		result.VictimName = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<S02PVPKill VictimName={VictimName}>";
	}
}
