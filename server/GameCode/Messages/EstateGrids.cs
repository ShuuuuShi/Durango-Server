using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct EstateGrids
{
	public const uint TypeCode = 2429u;

	public Point2[] Chunks;

	public Dictionary<Point2, string> Cells;

	public EstateLicense[] EstateLicenses;

	public static void Pack(Packer packer, EstateGrids val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2429u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.Chunks == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Chunks.Length);
			for (int i = 0; i < val.Chunks.Length; i++)
			{
				packer.PackArrayHeader(2);
				packer.Pack((byte)val.Chunks[i].x);
				packer.Pack((byte)val.Chunks[i].y);
			}
		}
		if (val.Cells == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Cells.Count);
			foreach (KeyValuePair<Point2, string> cell in val.Cells)
			{
				packer.PackArrayHeader(2);
				packer.Pack((byte)cell.Key.x);
				packer.Pack((byte)cell.Key.y);
				if (cell.Value == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(cell.Value);
				}
			}
		}
		if (val.EstateLicenses == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.EstateLicenses.Length);
		for (int j = 0; j < val.EstateLicenses.Length; j++)
		{
			EstateLicense.Pack(packer, val.EstateLicenses[j]);
		}
	}

	public static EstateGrids Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		EstateGrids result = default(EstateGrids);
		result.Chunks = new Point2[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.ReadByte(out var result2);
			result.Chunks[i].x = result2;
			unpacker.ReadByte(out result2);
			result.Chunks[i].y = result2;
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Cells = new Dictionary<Point2, string>(num2);
		Point2 key = default(Point2);
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			unpacker.ReadByte(out var result3);
			key.x = result3;
			unpacker.ReadByte(out result3);
			key.y = result3;
			unpacker.Read();
			string value = unpacker.LastReadData.AsString();
			result.Cells.Add(key, value);
		}
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		result.EstateLicenses = new EstateLicense[num3];
		for (int k = 0; k < num3; k++)
		{
			unpacker.Read();
			ref EstateLicense reference = ref result.EstateLicenses[k];
			reference = EstateLicense.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<EstateGrids Chunks={Chunks} Cells={Cells} EstateLicenses={EstateLicenses}>";
	}
}
