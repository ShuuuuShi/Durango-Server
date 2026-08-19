using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct Performance
{
	public string Id;

	public string Name;

	public string Icon;

	public Dictionary<string, float> Nums;

	public Dictionary<string, string> Strs;

	public static void Pack(Packer packer, Performance val, bool hint = false)
	{
		packer.PackArrayHeader(5);
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		packer.PackString(val.Name);
		if (val.Icon == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Icon);
		}
		if (val.Nums == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Nums.Count);
			foreach (KeyValuePair<string, float> num in val.Nums)
			{
				if (num.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(num.Key);
				}
				packer.Pack(num.Value);
			}
		}
		if (val.Strs == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Strs.Count);
		foreach (KeyValuePair<string, string> str in val.Strs)
		{
			if (str.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(str.Key);
			}
			if (str.Value == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(str.Value);
			}
		}
	}

	public static Performance Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Performance result = default(Performance);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Name = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.Icon = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Nums = new Dictionary<string, float>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			float value = unpacker.LastReadData.AsSingle();
			result.Nums.Add(key, value);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Strs = new Dictionary<string, string>(num2);
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			string key2 = unpacker.LastReadData.AsString();
			unpacker.Read();
			string value2 = unpacker.LastReadData.AsString();
			result.Strs.Add(key2, value2);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Performance Id={Id} Name={Name} Icon={Icon} Nums={Nums} Strs={Strs}>";
	}
}
