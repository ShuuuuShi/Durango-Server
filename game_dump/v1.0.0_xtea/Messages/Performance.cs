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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Performance result = default(Performance);
		result.Id = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		result.Name = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Icon = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.Nums = new Dictionary<string, float>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData4)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			float value = ((MessagePackObject)(ref lastReadData5)).AsSingle();
			result.Nums.Add(key, value);
		}
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData6)).AsInt32();
		result.Strs = new Dictionary<string, string>(num2);
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			MessagePackObject lastReadData7 = unpacker.LastReadData;
			string key2 = ((MessagePackObject)(ref lastReadData7)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData8 = unpacker.LastReadData;
			string value2 = ((MessagePackObject)(ref lastReadData8)).AsString();
			result.Strs.Add(key2, value2);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Performance Id={Id} Name={Name} Icon={Icon} Nums={Nums} Strs={Strs}>";
	}
}
