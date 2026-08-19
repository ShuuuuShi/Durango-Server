using System;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class ArtifactInventoryAccessWidget : UIWidget
{
	public Action Closed;

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private Selectable _prevButton;

	[SerializeField]
	private Selectable _nextButton;

	[SerializeField]
	private UILabel _valueLabel;

	[SerializeField]
	private UILabel _helpLabel;

	[SerializeField]
	private RectLayout _layout;

	private int[] _values = new int[5] { -1, 0, 1, 10, 30 };

	private string _text;

	private int _value;

	private Action<int> _onChanged;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_titleLabel.text = T._("꺼내기 개수 설정");
			UIEventListener uIEventListener = UIEventListener.Get(_titleWidget.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnBack));
			Selectable prevButton = _prevButton;
			prevButton.Clicked = (Action)Delegate.Combine(prevButton.Clicked, new Action(OnPrev));
			Selectable nextButton = _nextButton;
			nextButton.Clicked = (Action)Delegate.Combine(nextButton.Clicked, new Action(OnNext));
			_layout.UpdateLayout();
		}
	}

	public void Set(string text, int accessCount, Action<int> onChanged)
	{
		Init();
		_text = text;
		_onChanged = onChanged;
		SetValue(accessCount);
	}

	public void InvokeChanged()
	{
		if (_onChanged != null)
		{
			_onChanged(_value);
		}
		_text = null;
		_onChanged = null;
	}

	private void SetValue(int value)
	{
		_value = value;
		switch (value)
		{
		case -1:
			_valueLabel.text = T._("무제한");
			_valueLabel.color = PresetColor.UIYellow;
			break;
		case 0:
			_valueLabel.text = value.ToString();
			_valueLabel.color = PresetColor.UILightGray;
			break;
		default:
			_valueLabel.text = value.ToString();
			_valueLabel.color = PresetColor.UIYellow;
			break;
		}
		UpdateHelpText();
	}

	private void OnPrev()
	{
		int value = _value;
		int num = -1;
		for (int i = 0; i < _values.Length; i++)
		{
			if (value <= _values[i])
			{
				num = i - 1;
				break;
			}
		}
		if (num < 0)
		{
			num = _values.Length - 1;
		}
		SetValue(_values[num]);
	}

	private void OnNext()
	{
		int value = _value;
		int num = _values.Length;
		for (int num2 = _values.Length - 1; num2 >= 0; num2--)
		{
			if (value >= _values[num2])
			{
				num = num2 + 1;
				break;
			}
		}
		if (num >= _values.Length)
		{
			num = 0;
		}
		SetValue(_values[num]);
	}

	private void UpdateHelpText()
	{
		switch (_value)
		{
		case -1:
			_helpLabel.text = T._("<em>{0}</em>{0:-은} <em>무제한</em>의 물건을 꺼낼 수 있습니다.", _text);
			break;
		case 0:
			_helpLabel.text = T._("<em>{0}</em>{0:-은} 물건을 꺼낼 수 없습니다.", _text);
			break;
		default:
			_helpLabel.text = T._("<em>{0}</em>{0:-은} {1}마다 <em>{2}개</em>의 물건을 꺼낼 수 있습니다.", _text, TimedeltaFormatter.Format(OptionSystem.GetInventoryAccessRefreshPeriod() * 60 * 60, 1, "hour"), _value);
			break;
		}
	}

	private void OnBack(GameObject obj)
	{
		if (Closed != null)
		{
			Closed();
		}
	}
}
