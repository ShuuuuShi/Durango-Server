using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct PlaceAddOns
{
	public const uint TypeCode = 2436u;

	public string EntityId;

	public Point2 Tile;

	public Dictionary<int, string> AddOnPlacements;

	public Dictionary<int, string> PrevAddOnPlacements;

	public static void Pack(Packer packer, PlaceAddOns val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(2436u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (val.AddOnPlacements == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.AddOnPlacements.Count);
			foreach (KeyValuePair<int, string> addOnPlacement in val.AddOnPlacements)
			{
				packer.Pack(addOnPlacement.Key);
				if (addOnPlacement.Value == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(addOnPlacement.Value);
				}
			}
		}
		if (val.PrevAddOnPlacements == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.PrevAddOnPlacements.Count);
		foreach (KeyValuePair<int, string> prevAddOnPlacement in val.PrevAddOnPlacements)
		{
			packer.Pack(prevAddOnPlacement.Key);
			if (prevAddOnPlacement.Value == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(prevAddOnPlacement.Value);
			}
		}
	}

	public static PlaceAddOns Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PlaceAddOns result = default(PlaceAddOns);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.AddOnPlacements = new Dictionary<int, string>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int key = unpacker.LastReadData.AsInt32();
			unpacker.Read();
			string value = unpacker.LastReadData.AsString();
			result.AddOnPlacements.Add(key, value);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.PrevAddOnPlacements = new Dictionary<int, string>(num2);
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			int key2 = unpacker.LastReadData.AsInt32();
			unpacker.Read();
			string value2 = unpacker.LastReadData.AsString();
			result.PrevAddOnPlacements.Add(key2, value2);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PlaceAddOns EntityId={EntityId} Tile={Tile} AddOnPlacements={AddOnPlacements} PrevAddOnPlacements={PrevAddOnPlacements}>";
	}
}
