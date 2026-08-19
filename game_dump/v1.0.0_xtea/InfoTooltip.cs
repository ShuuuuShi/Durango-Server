using System;
using System.Collections.Generic;
using UnityEngine;

public class InfoTooltip : TooltipBase
{
	[SerializeField]
	private int _minWidth;

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UIWidget _infoWidget;

	[SerializeField]
	private UIWidget _buttonWidget;

	[SerializeField]
	private UISpriteLabel _titleLabel;

	[SerializeField]
	private UISpriteLabel _subTitleLabel;

	[SerializeField]
	private ListObjectPool _infos;

	[SerializeField]
	private DefaultSelectableButton _button;

	private int _titleOnlyHeight;

	private int _titleAndSubtitleHeight;

	private int _minimunButtonHeight;

	private Vector2 _baseButtonPos;

	private string _title;

	private string _subTitle;

	private List<KeyValuePair<string, string>> _infoList = new List<KeyValuePair<string, string>>();

	private string _buttonText;

	private Action _onClickButton;

	public void SetTitle(string text)
	{
		if (!(_title == text))
		{
			_title = text;
			MarkAsChange();
		}
	}

	public void SetSubtitle(string text)
	{
		if (!(_subTitle == text))
		{
			_subTitle = text;
			MarkAsChange();
		}
	}

	public void ClearInfo()
	{
		_infoList.Clear();
		MarkAsChange();
	}

	public void AddInfo(string key, string value)
	{
		_infoList.Add(new KeyValuePair<string, string>(key, value));
		MarkAsChange();
	}

	public void SetInfo(int index, string key, string value)
	{
		if (index >= 0 && index <= _infoList.Count)
		{
			if (index == _infoList.Count)
			{
				AddInfo(key, value);
			}
			else if (!(_infoList[index].Key == key) || !(_infoList[index].Value == value))
			{
				_infoList[index] = new KeyValuePair<string, string>(key, value);
				MarkAsChange();
			}
		}
	}

	public void SetButton(string text, Action onClick)
	{
		if (!(_buttonText == text) || !(onClick == _onClickButton))
		{
			_buttonText = text;
			_onClickButton = onClick;
			MarkAsChange();
		}
	}

	protected override void OnAwake()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		base.OnAwake();
		_titleOnlyHeight = _titleLabel.Label.height + (int)Mathf.Abs(_titleLabel.Label.GetPosition(0f, 1f).y) * 2;
		_titleAndSubtitleHeight = (int)Mathf.Abs(_subTitleLabel.Label.GetPosition(0f, 0f).y);
		_minimunButtonHeight = _button.Widget.height;
		_baseButtonPos = Vector2.op_Implicit(_button.Widget.GetPosition(0f, 1f));
		DefaultSelectableButton button = _button;
		button.Clicked = (Action)Delegate.Combine(button.Clicked, new Action(OnClickButotn));
	}

	protected override void OnFinish()
	{
		base.OnFinish();
		_title = null;
		_subTitle = null;
		_infoList.Clear();
		_buttonText = null;
		_onClickButton = null;
	}

	protected override void FillData()
	{
		_titleLabel.text = _title;
		_subTitleLabel.text = _subTitle;
		_infos.Set(_infoList.Count);
		for (int i = 0; i < _infos.Count; i++)
		{
			KeyValueLabel component = _infos[i].GetComponent<KeyValueLabel>();
			component.Set(_infoList[i].Key, _infoList[i].Value);
		}
		_button.Text = _buttonText;
	}

	protected override void UpdateLayout()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		int height;
		if (string.IsNullOrEmpty(_subTitle))
		{
			((Component)_subTitleLabel).gameObject.SetActive(false);
			height = _titleOnlyHeight;
		}
		else
		{
			((Component)_subTitleLabel).gameObject.SetActive(true);
			height = _titleAndSubtitleHeight;
		}
		_titleWidget.height = height;
		float num = Mathf.Max((float)_titleLabel.Label.width + _titleLabel.Label.GetPosition(0f, 0f).x * 2f, (float)_minWidth);
		for (int i = 0; i < _infos.Count; i++)
		{
			KeyValueLabel component = _infos[i].GetComponent<KeyValueLabel>();
			num = Mathf.Max(num, component.GetPredictSize().x);
		}
		if (!string.IsNullOrEmpty(_buttonText))
		{
			Vector2 printedSize = _button.TextLabel.printedSize;
			_button.Widget.height = Mathf.Max(_minimunButtonHeight, (int)printedSize.y + 20);
			num = Mathf.Max(printedSize.x + 80f, num);
		}
		Vector3 localPosition = _infos.BaseObject.transform.localPosition;
		int num2 = 0;
		int num3 = Mathf.CeilToInt(num);
		for (int j = 0; j < _infos.Count; j++)
		{
			KeyValueLabel component2 = _infos[j].GetComponent<KeyValueLabel>();
			component2.UpdateLayout(num3);
			((Component)component2).transform.localPosition = localPosition + Vector3.down * (float)num2;
			num2 += component2.Widget.height;
		}
		if (num2 == 0)
		{
			((Component)_infoWidget).gameObject.SetActive(false);
		}
		else
		{
			((Component)_infoWidget).gameObject.SetActive(true);
			_infoWidget.height = num2;
		}
		if (string.IsNullOrEmpty(_buttonText))
		{
			((Component)_buttonWidget).gameObject.SetActive(false);
		}
		else
		{
			((Component)_buttonWidget).gameObject.SetActive(true);
			_button.Widget.width = num3 - 40;
			_button.Widget.SetPosition(Vector2.op_Implicit(_baseButtonPos), 0f, 1f);
			_buttonWidget.height = _button.Widget.height + 20;
		}
		base.Widget.width = num3;
		base.Widget.height = _titleWidget.height + ((num2 > 0) ? _infoWidget.height : 0) + ((!string.IsNullOrEmpty(_buttonText)) ? _buttonWidget.height : 0);
		_titleWidget.width = num3;
		if (num2 > 0)
		{
			_infoWidget.width = num3;
			((Component)_infoWidget).transform.localPosition = ((Component)_titleWidget).transform.localPosition + Vector3.down * (float)_titleWidget.height;
		}
		if (!string.IsNullOrEmpty(_buttonText))
		{
			_buttonWidget.width = num3;
			((Component)_buttonWidget).transform.localPosition = ((Component)_titleWidget).transform.localPosition + Vector3.down * (float)(_titleWidget.height + num2);
		}
		UIUtility.UpdateAnchors(((Component)this).transform);
		NGUITools.UpdateWidgetCollider(((Component)this).gameObject);
	}

	private void OnClickButotn()
	{
		if (_onClickButton != null)
		{
			_onClickButton();
		}
	}
}
