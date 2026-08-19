using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class TransmissionQueueItem : SelectableWidget
{
	[SerializeField]
	private ItemIconTex _itemIcon;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _indexLabel;

	[SerializeField]
	private UILabel _timeLabel;

	private int _prevTimerCount;

	public ReceivingItem Data { get; private set; }

	public void Set(int index, ReceivingItem data)
	{
		Data = data;
		if (data.Item == null)
		{
			_itemIcon.gameObject.SetActive(value: false);
			_levelLabel.gameObject.SetActive(value: false);
			_timeLabel.gameObject.SetActive(value: false);
		}
		else
		{
			_itemIcon.SetIcon(data.Item);
			_levelLabel.text = T._("{0:lv:}", data.Item.Level);
			_timeLabel.text = string.Empty;
			_prevTimerCount = 0;
			_itemIcon.gameObject.SetActive(value: true);
			_levelLabel.gameObject.SetActive(value: true);
			_timeLabel.gameObject.SetActive(value: true);
		}
		_indexLabel.text = (index + 1).ToString();
		base.Disabled = data.Item == null;
	}

	public void UpdateTimer(double now)
	{
		int num = ((!(Data.ReceivingAt < now)) ? ((int)(Data.ReceivingAt - now)) : 0);
		if (_prevTimerCount != num)
		{
			_prevTimerCount = num;
			_timeLabel.text = ((num <= 0) ? T._("받는 중") : TimedeltaFormatter.Format(num));
		}
	}
}
