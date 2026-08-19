using Durango.Network;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class ClanAllyLockWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _timerLabel;

	[SerializeField]
	private RectLayout _layout;

	private AllySlot _slot;

	private void Start()
	{
		_layout.UpdateOnSizeChange();
	}

	public void Set(AllySlot slot)
	{
		_slot = slot;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		double at = ((!_slot.StateExpiresAt.HasValue) ? 0.0 : _slot.StateExpiresAt.Value);
		if (at > predictedServerTime)
		{
			_timerLabel.SetText(new SyncString(delegate(out string text, out float period)
			{
				double num = at - Connections.Frontend.GetPredictedServerTime();
				if (num > 0.0)
				{
					text = string.Format("[icon=icon_skill_time] {0}", TimedeltaFormatter.Format(num, 1, "min"));
					period = (float)(num % 60.0);
				}
				else
				{
					text = string.Empty;
					period = 0f;
				}
			}));
		}
		else
		{
			_timerLabel.text = string.Empty;
		}
	}
}
