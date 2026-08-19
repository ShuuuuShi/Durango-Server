using MsgPack;
using Shared.Faction;

namespace Messages;

public struct Faction
{
	public FactionType Type;

	public int Point;

	public int Level;

	public double AvailableAt;

	public static void Pack(Packer packer, Faction val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		packer.Pack((int)val.Type);
		packer.Pack(val.Point);
		packer.Pack(val.Level);
		packer.Pack(val.AvailableAt);
	}

	public static Faction Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Faction result = default(Faction);
		if (num < 0 || 4 < num)
		{
			result.Type = FactionType.Invalid;
		}
		else
		{
			result.Type = (FactionType)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Point = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Level = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.AvailableAt = ((MessagePackObject)(ref lastReadData4)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<Faction Type={Type} Point={Point} Level={Level} AvailableAt={AvailableAt}>";
	}
}
