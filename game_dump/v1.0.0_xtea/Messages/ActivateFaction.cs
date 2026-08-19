using MsgPack;
using Shared.Faction;

namespace Messages;

public struct ActivateFaction
{
	public const uint TypeCode = 3610u;

	public FactionType Faction;

	public static void Pack(Packer packer, ActivateFaction val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3610u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack((int)val.Faction);
	}

	public static ActivateFaction Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		ActivateFaction result = default(ActivateFaction);
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
		return $"<ActivateFaction Faction={Faction}>";
	}
}
