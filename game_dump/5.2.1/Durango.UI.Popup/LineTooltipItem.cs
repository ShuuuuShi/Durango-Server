using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.Popup;

public class LineTooltipItem : MonoBehaviour
{
	[SerializeField]
	private UISpriteLabel _keyLabel;

	[SerializeField]
	private UISpriteLabel _valueLabel;

	[SerializeField]
	private GameObject _splitLine;

	private UIWidget _widget;

	public UISpriteLabel KeyLabel => _keyLabel;

	public UISpriteLabel ValueLabel => _valueLabel;

	public string Key
	{
		get
		{
			return _keyLabel.text;
		}
		set
		{
			_keyLabel.text = value;
		}
	}

	public string Value
	{
		get
		{
			return _valueLabel.text;
		}
		set
		{
			_valueLabel.text = value;
		}
	}

	public int Index { get; set; }

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				_widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public void LineActive(bool active)
	{
		_splitLine.gameObject.SetActive(active);
	}

	public void UpdateLayout(float padding)
	{
		int width = Widget.width;
		Vector3 localPosition = KeyLabel.transform.localPosition;
		Vector3 localPosition2 = ValueLabel.transform.localPosition;
		localPosition.x = ((float)width - padding) * -0.5f;
		localPosition2.x = ((float)width - padding) * 0.5f;
		KeyLabel.transform.localPosition = localPosition;
		ValueLabel.transform.localPosition = localPosition2;
	}
}
