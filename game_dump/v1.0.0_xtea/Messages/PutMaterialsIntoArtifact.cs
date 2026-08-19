using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct PutMaterialsIntoArtifact
{
	public const uint TypeCode = 2092u;

	public ulong EntityId;

	public Point2 Tile;

	public Dictionary<string, ulong[]> Materials;

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
		packer.Pack(val.EntityId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (val.Materials == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Materials.Count);
		foreach (KeyValuePair<string, ulong[]> material in val.Materials)
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
				packer.Pack(material.Value[i]);
			}
		}
	}

	public static PutMaterialsIntoArtifact Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		PutMaterialsIntoArtifact result = default(PutMaterialsIntoArtifact);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.Materials = new Dictionary<string, ulong[]>(num2);
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData3)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			int num3 = ((MessagePackObject)(ref lastReadData4)).AsInt32();
			ulong[] array = new ulong[num3];
			for (int j = 0; j < num3; j++)
			{
				unpacker.Read();
				int num4 = j;
				MessagePackObject lastReadData5 = unpacker.LastReadData;
				array[num4] = ((MessagePackObject)(ref lastReadData5)).AsUInt64();
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
