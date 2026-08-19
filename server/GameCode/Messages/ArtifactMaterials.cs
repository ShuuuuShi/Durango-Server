using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct ArtifactMaterials
{
	public const uint TypeCode = 2091u;

	public string EntityId;

	public Dictionary<string, Item[]> Materials;

	public static void Pack(Packer packer, ArtifactMaterials val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2091u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.Materials == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Materials.Count);
		foreach (KeyValuePair<string, Item[]> material in val.Materials)
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
				Item.Pack(packer, material.Value[i]);
			}
		}
	}

	public static ArtifactMaterials Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ArtifactMaterials result = default(ArtifactMaterials);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Materials = new Dictionary<string, Item[]>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			Item[] array = new Item[num2];
			for (int j = 0; j < num2; j++)
			{
				unpacker.Read();
				ref Item reference = ref array[j];
				reference = Item.Unpack(unpacker);
			}
			result.Materials.Add(key, array);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactMaterials EntityId={EntityId} Materials={Materials}>";
	}
}
