using MsgPack;
using Shared.Economy;

namespace Messages;

public struct MakeClan
{
	public const uint TypeCode = 3651u;

	public string ClanName;

	public Currency Currency;

	public static void Pack(Packer packer, MakeClan val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3651u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.ClanName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ClanName);
		}
		packer.Pack((int)val.Currency);
	}

	public static MakeClan Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		MakeClan result = default(MakeClan);
		result.ClanName = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 7 < num)
		{
			result.Currency = Currency.Invalid;
		}
		else
		{
			result.Currency = (Currency)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<MakeClan ClanName={ClanName} Currency={Currency}>";
	}
}
