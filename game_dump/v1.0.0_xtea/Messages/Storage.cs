using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct Storage
{
	public const uint TypeCode = 2299u;

	public Dictionary<string, byte[]> Data;

	public static void Pack(Packer packer, Storage val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2299u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Data == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Data.Count);
		foreach (KeyValuePair<string, byte[]> datum in val.Data)
		{
			if (datum.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(datum.Key);
			}
			if (datum.Value == null)
			{
				packer.PackBinary(new byte[0]);
			}
			else
			{
				packer.PackBinary(datum.Value);
			}
		}
	}

	public static Storage Unpack(Unpacker unpacker)
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
		Storage result = default(Storage);
		result.Data = new Dictionary<string, byte[]>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData2)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			byte[] value = ((MessagePackObject)(ref lastReadData3)).AsBinary();
			result.Data.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Storage Data={Data}>";
	}
}
