using MsgPack;
using Shared.Faction;
using Shared.System;

namespace Messages;

public struct ExplorePOI
{
	public const uint TypeCode = 908u;

	public Point2 Tile;

	public Shared.System.PointOfInterest Type;

	public FactionType? Faction;

	public string Icon;

	public string Title;

	public static void Pack(Packer packer, ExplorePOI val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(908u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.Pack((int)val.Type);
		if (!val.Faction.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack((int)val.Faction.Value);
		}
		if (val.Icon == null)
		{
			packer.PackNull();
		}
		else if (val.Icon == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Icon);
		}
		if (val.Title == null)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackString(val.Title);
		}
	}

	public static ExplorePOI Unpack(Unpacker unpacker)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		ExplorePOI result = default(ExplorePOI);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData)).AsInt32();
		if (num2 < 0 || 4 < num2)
		{
			result.Type = Shared.System.PointOfInterest.Invalid;
		}
		else
		{
			result.Type = (Shared.System.PointOfInterest)num2;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData2)).IsNil)
		{
			result.Faction = null;
		}
		else
		{
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			int num3 = ((MessagePackObject)(ref lastReadData3)).AsInt32();
			FactionType value = ((num3 >= 0 && 4 >= num3) ? ((FactionType)num3) : FactionType.Invalid);
			result.Faction = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData4)).IsNil)
		{
			result.Icon = null;
		}
		else
		{
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			string icon = ((MessagePackObject)(ref lastReadData5)).AsString();
			result.Icon = icon;
		}
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData6)).IsNil)
		{
			result.Title = null;
		}
		else
		{
			string title = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.Title = title;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ExplorePOI Tile={Tile} Type={Type} Faction={Faction} Icon={Icon} Title={Title}>";
	}
}
