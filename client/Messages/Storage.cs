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
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Storage result = default(Storage);
		result.Data = new Dictionary<string, byte[]>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			byte[] value = unpacker.LastReadData.AsBinary();
			result.Data.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Storage Data={Data}>";
	}
}
