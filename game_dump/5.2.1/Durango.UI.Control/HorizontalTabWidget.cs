using Durango.Logic.Notification;
using UnityEngine;

namespace Durango.UI.Control;

public class HorizontalTabWidget : SelectableWidget
{
	[SerializeField]
	private KeyValueLabel _keyValueLabel;

	[SerializeField]
	private UILabel _singleText;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UISprite _notification;

	public void SetText(SyncString text)
	{
		_singleText.SetText(text);
		_keyValueLabel.Set(null, null);
		_singleText.gameObject.SetActive(value: true);
		_iconSprite.gameObject.SetActive(value: false);
	}

	public void SetText(SyncString key, SyncString value)
	{
		_keyValueLabel.Set(key, value);
		_keyValueLabel.gameObject.SetActive(value: true);
		_singleText.gameObject.SetActive(value: false);
		_iconSprite.gameObject.SetActive(value: false);
	}

	public void SetIcon(string icon)
	{
		_iconSprite.spriteName = icon;
		_keyValueLabel.Set(null, null);
		_singleText.gameObject.SetActive(value: false);
		_iconSprite.gameObject.SetActive(value: true);
	}

	public void SetValue(SyncString value)
	{
		if (_keyValueLabel.gameObject.activeSelf)
		{
			_keyValueLabel.SetValue(value);
		}
	}

	public int GetPreferredSize(int limitSize = 0)
	{
		int num = 0;
		if (_singleText.gameObject.activeSelf)
		{
			num = (int)_singleText.printedSize.x + 40;
		}
		else
		{
			if (!_iconSprite.gameObject.activeSelf)
			{
				return (int)_keyValueLabel.GetPreferredSize(limitSize).x;
			}
			num = _iconSprite.width + 40;
		}
		if (limitSize > 0)
		{
			return Mathf.Min(num, limitSize);
		}
		return num;
	}

	public void UpdateLayout(int size = 0)
	{
		if (_singleText.gameObject.activeSelf || _iconSprite.gameObject.activeSelf)
		{
			base.Widget.width = ((size <= 0) ? GetPreferredSize() : size);
		}
		else
		{
			_keyValueLabel.UpdateLayout(size);
		}
	}

	public void NotificationOn(bool on, Type type)
	{
		if (on)
		{
			_notification.gameObject.SetActive(value: true);
			_notification.color = Notification.GetTypeColor(type);
		}
		else
		{
			_notification.gameObject.SetActive(value: false);
		}
	}
}
