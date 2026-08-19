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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		TerrainDebug result = default(TerrainDebug);
		result.TileLabels = new Dictionary<Point2, string>(num);
		ushort num2 = default(ushort);
		Point2 key = default(Point2);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.ReadUInt16(ref num2);
			key.x = num2;
			unpacker.ReadUInt16(ref num2);
			key.y = num2;
			unpacker.Read();
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			string value = ((MessagePackObject)(ref lastReadData2)).AsString();
			result.TileLabels.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<TerrainDebug TileLabels={TileLabels}>";
	}
}
