using Durango.Network;
using UnityEngine;

namespace Durango.Development;

public class LatencyDisplay : MonoBehaviour
{
	[SerializeField]
	private UILabel _latencyLabel;

	private int _prevLatency = -1;

	private void Update()
	{
		int num = Mathf.RoundToInt(Connections.Frontend.Ping * 1000f);
		if (_prevLatency != num)
		{
			_latencyLabel.text = $"Ping: {num}";
			_prevLatency = num;
		}
		if (num <= 200)
		{
			_latencyLabel.color = Color.green;
		}
		else if (num <= 500)
		{
			_latencyLabel.color = Color.yellow;
		}
		else
		{
			_latencyLabel.color = Color.red;
		}
	}
}
