using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct RemodelArtifact
{
	public const uint TypeCode = 2098u;

	public string EntityId;

	public Point2 Tile;

	public string SlotId;

	public Dictionary<string, string[]> Materials;

	public string ToolItemId;

	public static void Pack(Packer packer, RemodelArtifact val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(2098u);
		}
		else
		{
			packer.PackArrayHeader(5);
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
		if (val.SlotId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SlotId);
		}
		if (val.Materials == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Materials.Count);
			foreach (KeyValuePair<string, string[]> material in val.Materials)
			{
				if (material.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(material.Key);
				}
				if (material.Value == null)
				{
					packer.PackArrayHeader(0);
					continue;
				}
				packer.PackArrayHeader(material.Value.Length);
				for (int i = 0; i < material.Value.Length; i++)
				{
					if (material.Value[i] == null)
					{
						packer.PackString(string.Empty);
					}
					else
					{
						packer.PackString(material.Value[i]);
					}
				}
			}
		}
		if (val.ToolItemId == null)
		{
			packer.PackNull();
		}
		else if (val.ToolItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ToolItemId);
		}
	}

	public static RemodelArtifact Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RemodelArtifact result = default(RemodelArtifact);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.SlotId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Materials = new Dictionary<string, string[]>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			string[] array = new string[num2];
			for (int j = 0; j < num2; j++)
			{
				unpacker.Read();
				array[j] = unpacker.LastReadData.AsString();
			}
			result.Materials.Add(key, array);
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ToolItemId = null;
		}
		else
		{
			string toolItemId = unpacker.LastReadData.AsString();
			result.ToolItemId = toolItemId;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RemodelArtifact EntityId={EntityId} Tile={Tile} SlotId={SlotId} Materials={Materials} ToolItemId={ToolItemId}>";
	}
}
