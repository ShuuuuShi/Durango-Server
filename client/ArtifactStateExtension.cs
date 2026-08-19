using Durango.Network;
using Messages;

public static class ArtifactStateExtension
{
	public static bool IsRepairing(this ArtifactState msg)
	{
		float duration;
		float ratio;
		return msg.TryGetRepairInfo(out duration, out ratio);
	}

	public static bool TryGetRepairInfo(this ArtifactState msg, out float duration, out float ratio)
	{
		duration = 0f;
		ratio = 0f;
		Pair<double, double>? repairement = msg.Repairement;
		if (!repairement.HasValue)
		{
			return false;
		}
		double num = msg.Repairement.Value.Item1;
		double item = msg.Repairement.Value.Item2;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (num <= 0.0)
		{
			num = predictedServerTime;
		}
		if (item <= predictedServerTime || num >= item)
		{
			return false;
		}
		duration = (float)(item - num);
		ratio = (float)(predictedServerTime - num) / duration;
		return true;
	}
}
