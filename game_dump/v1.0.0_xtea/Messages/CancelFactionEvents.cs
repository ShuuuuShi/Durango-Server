using MsgPack;
using Shared.Faction;

namespace Messages;

public struct CancelFactionEvents
{
	public const uint TypeCode = 3623u;

	public FactionType Faction;

	public static void Pack(Packer packer, CancelFactionEvents val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3623u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.Faction);
	}

	public static CancelFactionEvents Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		CancelFactionEvents result = default(CancelFactionEvents);
		if (num < 0 || 4 < num)
		{
			result.Faction = FactionType.Invalid;
		}
		else
		{
			result.Faction = (FactionType)num;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<CancelFactionEvents Faction={Faction}>";
	}
}
