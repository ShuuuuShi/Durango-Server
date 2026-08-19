using System.Collections.Generic;
using MsgPack;
using Shared.Item;

namespace Messages;

public struct Bleach
{
	public const uint TypeCode = 3669u;

	public ColorChannel Channel;

	public Dictionary<string, string[]> Materials;

	public string ToolItemId;

	public PropKey? Workbench;

	public static void Pack(Packer packer, Bleach val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(3669u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack((int)val.Channel);
		if (val.Materials == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
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
		if (val.ToolItemId == null)
		{
			packer.PackNull();
		}
		else if (val.ToolItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ToolItemId);
		}
		if (!val.Workbench.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			PropKey.Pack(packer, val.Workbench.Value);
		}
	}

	public static Bleach Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Bleach result = default(Bleach);
		if (num < 0 || 2 < num)
		{
			result.Channel = ColorChannel.Invalid;
		}
		else
		{
			result.Channel = (ColorChannel)num;
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Materials = new Dictionary<string, string[]>(num2);
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			int num3 = unpacker.LastReadData.AsInt32();
			string[] array = new string[num3];
			for (int j = 0; j < num3; j++)
			{
				unpacker.Read();
				array[j] = unpacker.LastReadData.AsString();
			}
			result.Materials.Add(key, array);
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ToolItemId = null;
		}
		else
		{
			string toolItemId = unpacker.LastReadData.AsString();
			result.ToolItemId = toolItemId;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Workbench = null;
		}
		else
		{
			PropKey value = PropKey.Unpack(unpacker);
			result.Workbench = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Bleach Channel={Channel} Materials={Materials} ToolItemId={ToolItemId} Workbench={Workbench}>";
	}
}
