using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class TransmissionQueueDetailItem : SelectableWidget
{
	[SerializeField]
	private UILabel _indexLabel;

	[SerializeField]
	private ItemIconTex _itemIcon;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _prototypeLabel;

	[SerializeField]
	private UILabel _timerLabel;

	private int _prevTimerCount;

	public ReceivingItem Data { get; private set; }

	public void Set(int index, ReceivingItem data)
	{
		Data = data;
		if (data.Item == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		_itemIcon.SetIcon(data.Item);
		_nameLabel.text = data.Item.Name;
		_prototypeLabel.text = T._("{0} {1:lv:}", data.Item.PrototypeName, data.Item.Level);
		_timerLabel.text = string.Empty;
		_prevTimerCount = 0;
		_indexLabel.text = (index + 1).ToString();
	}

	public void UpdateTimer(double now)
	{
		int num = ((!(Data.ReceivingAt < now)) ? ((int)(Data.ReceivingAt - now)) : 0);
		if (_prevTimerCount != num)
		{
			_prevTimerCount = num;
			_timerLabel.text = ((num <= 0) ? string.Empty : TimedeltaFormatter.Format(num));
		}
	}
}
