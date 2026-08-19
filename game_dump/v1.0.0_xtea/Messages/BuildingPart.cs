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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		BuildingPart result = default(BuildingPart);
		result.Materials = new Dictionary<string, Item[]>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData2)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			int num2 = ((MessagePackObject)(ref lastReadData3)).AsInt32();
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
