using MsgPack;

namespace Messages;

public struct LeaveClan
{
	public const uint TypeCode = 3652u;

	public string ClanId;

	public static void Pack(Packer packer, LeaveClan val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3652u);
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

	public static LeaveClan Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		LeaveClan result = default(LeaveClan);
		result.ClanId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<LeaveClan ClanId={ClanId}>";
	}
}
