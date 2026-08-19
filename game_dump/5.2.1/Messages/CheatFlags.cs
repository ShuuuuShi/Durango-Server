using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct CheatFlags
{
	public const uint TypeCode = 2089u;

	public Dictionary<string, bool> Flags;

	public static void Pack(Packer packer, CheatFlags val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2089u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Flags == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Flags.Count);
		foreach (KeyValuePair<string, bool> flag in val.Flags)
		{
			if (flag.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(flag.Key);
			}
			packer.Pack(flag.Value);
		}
	}

	public static CheatFlags Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		CheatFlags result = default(CheatFlags);
		result.Flags = new Dictionary<string, bool>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			bool value = unpacker.LastReadData.AsBoolean();
			result.Flags.Add(key, value);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<CheatFlags Flags={Flags}>";
	}
}
