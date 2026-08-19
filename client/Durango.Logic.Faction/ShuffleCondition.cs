using System;
using Durango.Network;
using Yaml;
using Yaml.Util;

namespace Durango.Logic.Faction;

public class ShuffleCondition
{
	private int _baseRemainCount;

	private double _shuffleAt;

	public int RemainCount
	{
		get
		{
			if (_shuffleAt > 0.0)
			{
				double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
				double num = Math.Max(0.0, predictedServerTime - _shuffleAt);
				double rechargeCooltime = Singleton<Constants>.Instance.Faction.Mission.Shuffle.RechargeCooltime;
				return Math.Min(MaxCount, _baseRemainCount + (int)(num / rechargeCooltime));
			}
			return _baseRemainCount;
		}
	}

	public int MaxCount => Singleton<Constants>.Instance.Faction.Mission.Shuffle.MaxCount;

	public double RechargeEndsAt
	{
		get
		{
			if (_shuffleAt > 0.0)
			{
				FactionInfo.MissionData.ShuffleData shuffle = Singleton<Constants>.Instance.Faction.Mission.Shuffle;
				return _shuffleAt + shuffle.RechargeCooltime * (double)(MaxCount - _baseRemainCount);
			}
			return 0.0;
		}
	}

	public void Set(int count, double shuffleAt)
	{
		_baseRemainCount = count;
		_shuffleAt = shuffleAt;
	}
}
