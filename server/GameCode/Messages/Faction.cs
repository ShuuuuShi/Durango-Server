using MsgPack;
using Shared.Faction;

namespace Messages;

public struct Faction
{
	public FactionType Type;

	public int Point;

	public int Level;

	public double AvailableAt;

	public int? PointBefore;

	public double StartsAt;

	public double EndsAt;

	public static void Pack(Packer packer, Faction val, bool hint = false)
	{
		packer.PackArrayHeader(7);
		packer.Pack((int)val.Type);
		packer.Pack(val.Point);
		packer.Pack(val.Level);
		packer.Pack(val.AvailableAt);
		if (!val.PointBefore.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.PointBefore.Value);
		}
		packer.Pack(val.StartsAt);
		packer.Pack(val.EndsAt);
	}

	public static Faction Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Faction result = default(Faction);
		if (num < 0 || 101 < num)
		{
			result.Type = FactionType.Invalid;
		}
		else
		{
			result.Type = (FactionType)num;
		}
		unpacker.Read();
		result.Point = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.AvailableAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PointBefore = null;
		}
		else
		{
			int value = unpacker.LastReadData.AsInt32();
			result.PointBefore = value;
		}
		unpacker.Read();
		result.StartsAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.EndsAt = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<Faction Type={Type} Point={Point} Level={Level} AvailableAt={AvailableAt} PointBefore={PointBefore} StartsAt={StartsAt} EndsAt={EndsAt}>";
	}
}
