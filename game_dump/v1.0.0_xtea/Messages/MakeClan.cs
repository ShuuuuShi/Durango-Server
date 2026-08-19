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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		MakeClan result = default(MakeClan);
		result.ClanName = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		if (num < 0 || 1 < num)
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
