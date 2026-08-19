using System;
using Durango.Network;
using L10N;
using Newtonsoft.Json;

namespace Durango.Player;

public struct PlayerConnected : IComparable<PlayerConnected>
{
	[JsonProperty(PropertyName = "disconnected_at")]
	public double? DisconnectedAt;

	[JsonProperty(PropertyName = "online")]
	public bool Online;

	public string GetConnectedString()
	{
		if (Online)
		{
			return T._("<em>접속 중</em>");
		}
		if (DisconnectedAt.HasValue)
		{
			double value = DisconnectedAt.Value;
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			double num = predictedServerTime - value;
			if (2592000.0 < num)
			{
				return T._("오래 전");
			}
			return T._("{0} 전", TimedeltaFormatter.Format(predictedServerTime - value, 2, "min"));
		}
		return string.Empty;
	}

	public int CompareTo(PlayerConnected other)
	{
		int num = Online.CompareTo(other.Online);
		if (num != 0)
		{
			return -num;
		}
		return Nullable.Compare(DisconnectedAt, other.DisconnectedAt);
	}
}
