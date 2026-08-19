using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct AdvisorTargets
{
	public const uint TypeCode = 3709u;

	public Dictionary<string, float> Titles;

	public string[] RemainingRewards;

	public static void Pack(Packer packer, AdvisorTargets val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3709u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Titles == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Titles.Count);
			foreach (KeyValuePair<string, float> title in val.Titles)
			{
				if (title.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(title.Key);
				}
				packer.Pack(title.Value);
			}
		}
		if (val.RemainingRewards == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.RemainingRewards.Length);
		for (int i = 0; i < val.RemainingRewards.Length; i++)
		{
			if (val.RemainingRewards[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.RemainingRewards[i]);
			}
		}
	}

	public static AdvisorTargets Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		AdvisorTargets result = default(AdvisorTargets);
		result.Titles = new Dictionary<string, float>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			float value = unpacker.LastReadData.AsSingle();
			result.Titles.Add(key, value);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.RemainingRewards = new string[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			result.RemainingRewards[j] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AdvisorTargets Titles={Titles} RemainingRewards={RemainingRewards}>";
	}
}
