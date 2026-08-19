using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct Craft
{
	public const uint TypeCode = 6u;

	public string RecipeId;

	public Dictionary<string, string[]> Materials;

	public string ToolItemId;

	public PropKey? Workbench;

	public int? ReformSlotIndex;

	public static void Pack(Packer packer, Craft val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(6u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		if (val.RecipeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RecipeId);
		}
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
		if (!val.ReformSlotIndex.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.ReformSlotIndex.Value);
		}
	}

	public static Craft Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Craft result = default(Craft);
		result.RecipeId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Materials = new Dictionary<string, string[]>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			string[] array = new string[num2];
			for (int j = 0; j < num2; j++)
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
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ReformSlotIndex = null;
		}
		else
		{
			int value2 = unpacker.LastReadData.AsInt32();
			result.ReformSlotIndex = value2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Craft RecipeId={RecipeId} Materials={Materials} ToolItemId={ToolItemId} Workbench={Workbench} ReformSlotIndex={ReformSlotIndex}>";
	}
}
