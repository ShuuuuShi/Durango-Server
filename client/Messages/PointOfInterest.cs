using MsgPack;
using Shared.System;

namespace Messages;

public struct PointOfInterest
{
	public Point2 Tile;

	public Shared.System.PointOfInterest Type;

	public string Icon;

	public string Title;

	public ushort? EntityType;

	public bool IsExplored;

	public static void Pack(Packer packer, PointOfInterest val, bool hint = false)
	{
		packer.PackArrayHeader(6);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.Pack((int)val.Type);
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
		if (!val.EntityType.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.EntityType.Value);
		}
		packer.Pack(val.IsExplored);
	}

	public static PointOfInterest Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		unpacker.ReadUInt16(out var result);
		PointOfInterest result2 = default(PointOfInterest);
		result2.Tile.x = result;
		unpacker.ReadUInt16(out result);
		result2.Tile.y = result;
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 6 < num)
		{
			result2.Type = Shared.System.PointOfInterest.Invalid;
		}
		else
		{
			result2.Type = (Shared.System.PointOfInterest)num;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result2.Icon = null;
		}
		else
		{
			string icon = unpacker.LastReadData.AsString();
			result2.Icon = icon;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result2.Title = null;
		}
		else
		{
			string title = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result2.Title = title;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result2.EntityType = null;
		}
		else
		{
			ushort value = unpacker.LastReadData.AsUInt16();
			result2.EntityType = value;
		}
		unpacker.Read();
		result2.IsExplored = unpacker.LastReadData.AsBoolean();
		return result2;
	}

	public override string ToString()
	{
		return $"<PointOfInterest Tile={Tile} Type={Type} Icon={Icon} Title={Title} EntityType={EntityType} IsExplored={IsExplored}>";
	}
}
