using UnityEngine;

public class LatencyDisplay : MonoBehaviour
{
	[SerializeField]
	private UILabel _latencyLabel;

	private int _prevLatency = -1;

	private void Update()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.RoundToInt(Connections.Frontend.Latency * 1000f);
		if (_prevLatency != num)
		{
			_latencyLabel.text = $"Latency: {num}";
			_prevLatency = num;
		}
		if (num <= 500)
		{
			_latencyLabel.color = Color.green;
		}
		else if (num <= 1500)
		{
			_latencyLabel.color = Color.yellow;
		}
		else
		{
			_latencyLabel.color = Color.red;
		}
	}
}
