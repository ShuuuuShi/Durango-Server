using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct EstateGrids
{
	public const uint TypeCode = 2429u;

	public KeyValuePair<int, int>[] Chunks;

	public Dictionary<KeyValuePair<int, int>, EstateInfo> Estates;

	public static void Pack(Packer packer, EstateGrids val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2429u);
		}
		else
		{
			packer.PackArrayHeader(2);
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
				packer.Pack(val.Chunks[i].Key);
				packer.Pack(val.Chunks[i].Value);
			}
		}
		if (val.Estates == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Estates.Count);
		foreach (KeyValuePair<KeyValuePair<int, int>, EstateInfo> estate in val.Estates)
		{
			packer.PackArrayHeader(2);
			packer.Pack(estate.Key.Key);
			packer.Pack(estate.Key.Value);
			EstateInfo.Pack(packer, estate.Value);
		}
	}

	public static EstateGrids Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		EstateGrids result = default(EstateGrids);
		result.Chunks = new KeyValuePair<int, int>[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.Read();
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			int key = ((MessagePackObject)(ref lastReadData2)).AsInt32();
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			int value = ((MessagePackObject)(ref lastReadData3)).AsInt32();
			ref KeyValuePair<int, int> reference = ref result.Chunks[i];
			reference = new KeyValuePair<int, int>(key, value);
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		result.Estates = new Dictionary<KeyValuePair<int, int>, EstateInfo>(num2);
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			unpacker.Read();
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			int key2 = ((MessagePackObject)(ref lastReadData5)).AsInt32();
			unpacker.Read();
			MessagePackObject lastReadData6 = unpacker.LastReadData;
			int value2 = ((MessagePackObject)(ref lastReadData6)).AsInt32();
			KeyValuePair<int, int> key3 = new KeyValuePair<int, int>(key2, value2);
			unpacker.Read();
			EstateInfo value3 = EstateInfo.Unpack(unpacker);
			result.Estates.Add(key3, value3);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<EstateGrids Chunks={Chunks} Estates={Estates}>";
	}
}
