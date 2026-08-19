using System.Collections.Generic;
using MsgPack;
using Shared.Faction;

namespace Messages;

public struct MissionInfos
{
	public const uint TypeCode = 3622u;

	public Mission[] Missions;

	public Dictionary<FactionType, double> MissionActivatesAt;

	public Dictionary<FactionType, string> RecommendFailReasons;

	public byte ShuffleCount;

	public double? ShuffleAt;

	public static void Pack(Packer packer, MissionInfos val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(3622u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		if (val.Missions == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Missions.Length);
			for (int i = 0; i < val.Missions.Length; i++)
			{
				Mission.Pack(packer, val.Missions[i]);
			}
		}
		if (val.MissionActivatesAt == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.MissionActivatesAt.Count);
			foreach (KeyValuePair<FactionType, double> item in val.MissionActivatesAt)
			{
				packer.Pack((int)item.Key);
				packer.Pack(item.Value);
			}
		}
		if (val.RecommendFailReasons == null)
		{
			packer.PackNull();
		}
		else if (val.RecommendFailReasons == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.RecommendFailReasons.Count);
			foreach (KeyValuePair<FactionType, string> recommendFailReason in val.RecommendFailReasons)
			{
				packer.Pack((int)recommendFailReason.Key);
				packer.PackString(recommendFailReason.Value);
			}
		}
		packer.Pack(val.ShuffleCount);
		if (!val.ShuffleAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.ShuffleAt.Value);
		}
	}

	public static MissionInfos Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		MissionInfos result = default(MissionInfos);
		result.Missions = new Mission[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Mission reference = ref result.Missions[i];
			reference = Mission.Unpack(unpacker);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.MissionActivatesAt = new Dictionary<FactionType, double>(num2, default(FactionTypeComparer));
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			int num3 = unpacker.LastReadData.AsInt32();
			FactionType key = ((num3 >= 0 && 101 >= num3) ? ((FactionType)num3) : FactionType.Invalid);
			unpacker.Read();
			double value = unpacker.LastReadData.AsDouble();
			result.MissionActivatesAt.Add(key, value);
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RecommendFailReasons = null;
		}
		else
		{
			int num4 = unpacker.LastReadData.AsInt32();
			Dictionary<FactionType, string> dictionary = new Dictionary<FactionType, string>(num4, default(FactionTypeComparer));
			for (int k = 0; k < num4; k++)
			{
				unpacker.Read();
				int num5 = unpacker.LastReadData.AsInt32();
				FactionType key2 = ((num5 >= 0 && 101 >= num5) ? ((FactionType)num5) : FactionType.Invalid);
				unpacker.Read();
				string value2 = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
				dictionary.Add(key2, value2);
			}
			result.RecommendFailReasons = dictionary;
		}
		unpacker.Read();
		result.ShuffleCount = unpacker.LastReadData.AsByte();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ShuffleAt = null;
		}
		else
		{
			double value3 = unpacker.LastReadData.AsDouble();
			result.ShuffleAt = value3;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<MissionInfos Missions={Missions} MissionActivatesAt={MissionActivatesAt} RecommendFailReasons={RecommendFailReasons} ShuffleCount={ShuffleCount} ShuffleAt={ShuffleAt}>";
	}
}
