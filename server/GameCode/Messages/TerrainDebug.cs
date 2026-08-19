using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct TerrainDebug
{
	public const uint TypeCode = 2050u;

	public Dictionary<Point2, string> TileLabels;

	public static void Pack(Packer packer, TerrainDebug val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2050u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.TileLabels == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.TileLabels.Count);
		foreach (KeyValuePair<Point2, string> tileLabel in val.TileLabels)
		{
			packer.PackArrayHeader(2);
			packer.Pack((ushort)tileLabel.Key.x);
			packer.Pack((ushort)tileLabel.Key.y);
			if (tileLabel.Value == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(tileLabel.Value);
			}
		}
	}

	public static TerrainDebug Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		TerrainDebug result = default(TerrainDebug);
		result.TileLabels = new Dictionary<Point2, string>(num);
		Point2 key = default(Point2);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.ReadUInt16(out var result2);
			key.x = result2;
			unpacker.ReadUInt16(out result2);
			key.y = result2;
			unpacker.Read();
			string value = unpacker.LastReadData.AsString();
			result.TileLabels.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<TerrainDebug TileLabels={TileLabels}>";
	}
}
