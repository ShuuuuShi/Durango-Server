using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct BuildingPart
{
	public Dictionary<string, Item[]> Materials;

	public Gauge Progress;

	public static void Pack(Packer packer, BuildingPart val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		if (val.Materials == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
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
		Gauge.PackTo(val.Progress, packer);
	}

	public static BuildingPart Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		BuildingPart result = default(BuildingPart);
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
		unpacker.Read();
		result.Progress = Gauge.UnpackFrom(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<BuildingPart Materials={Materials} Progress={Progress}>";
	}
}
