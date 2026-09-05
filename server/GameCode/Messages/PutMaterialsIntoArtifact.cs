using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct PutMaterialsIntoArtifact
{
	public const uint TypeCode = 2092u;

	public string EntityId;

	public Point2 Tile;

	public Dictionary<string, string[]> Materials;

	public static void Pack(Packer packer, PutMaterialsIntoArtifact val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2092u);
		}
		else
		{
			packer.PackArrayHeader(3);
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
		if (val.Materials == null)
		{
			packer.PackMapHeader(0);
			return;
		}
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

	public static PutMaterialsIntoArtifact Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PutMaterialsIntoArtifact result = default(PutMaterialsIntoArtifact);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
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
		return result;
	}

	public override string ToString()
	{
		return $"<PutMaterialsIntoArtifact EntityId={EntityId} Tile={Tile} Materials={Materials}>";
	}
}
