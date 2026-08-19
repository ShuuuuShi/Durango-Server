using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct TechSupportEstimates
{
	public const uint TypeCode = 59139u;

	public Dictionary<string, Dictionary<int, TechSupportEstimateInfo>> Estimates;

	public static void Pack(Packer packer, TechSupportEstimates val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(59139u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Estimates == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Estimates.Count);
		foreach (KeyValuePair<string, Dictionary<int, TechSupportEstimateInfo>> estimate in val.Estimates)
		{
			if (estimate.Key == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(estimate.Key);
			}
			if (estimate.Value == null)
			{
				packer.PackMapHeader(0);
				continue;
			}
			packer.PackMapHeader(estimate.Value.Count);
			foreach (KeyValuePair<int, TechSupportEstimateInfo> item in estimate.Value)
			{
				packer.Pack(item.Key);
				TechSupportEstimateInfo.Pack(packer, item.Value);
			}
		}
	}

	public static TechSupportEstimates Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		TechSupportEstimates result = default(TechSupportEstimates);
		result.Estimates = new Dictionary<string, Dictionary<int, TechSupportEstimateInfo>>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			Dictionary<int, TechSupportEstimateInfo> dictionary = new Dictionary<int, TechSupportEstimateInfo>(num2);
			for (int j = 0; j < num2; j++)
			{
				unpacker.Read();
				int key2 = unpacker.LastReadData.AsInt32();
				unpacker.Read();
				TechSupportEstimateInfo value = TechSupportEstimateInfo.Unpack(unpacker);
				dictionary.Add(key2, value);
			}
			result.Estimates.Add(key, dictionary);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<TechSupportEstimates Estimates={Estimates}>";
	}
}
