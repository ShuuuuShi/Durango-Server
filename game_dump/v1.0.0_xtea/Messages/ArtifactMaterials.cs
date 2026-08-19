using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct ArtifactMaterials
{
	public const uint TypeCode = 2091u;

	public ulong EntityId;

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
		packer.Pack(val.EntityId);
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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ArtifactMaterials result = default(ArtifactMaterials);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.Materials = new Dictionary<string, Item[]>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData3)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			int num2 = ((MessagePackObject)(ref lastReadData4)).AsInt32();
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
