using System;
using System.Collections.Generic;
using Durango.System;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class InfoTooltip : TooltipBase
{
	private struct KeyValuePair
	{
		public SyncString Key;

		public KeyLabelBase.IContent Value;

		public KeyValuePair(SyncString key, KeyLabelBase.IContent value)
		{
			Key = key;
			Value = value;
		}
	}

	[SerializeField]
	private KeyGaugeLabel _gaugeLabelBase;

	[SerializeField]
	private KeyValueLabel _textLabelBase;

	[SerializeField]
	private int _minWidth;

	[SerializeField]
	private int _maxWidth;

	[SerializeField]
	private int _widthWithMapsButton;

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UIWidget _noticeWidget;

	[SerializeField]
	private int _interTitleSubtitleLength = 10;

	[SerializeField]
	private UIWidget _infoWidget;

	[SerializeField]
	private UIWidget _buttonWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _subTitleLabel;

	[SerializeField]
	private UILabel _noticeLabel;

	[SerializeField]
	private GameObject _titleInfoSeparator;

	[SerializeField]
	private UIWidget _infoPanel;

	[SerializeField]
	private UISprite _infoSeparatorBase;

	[SerializeField]
	private SelectableButton _mapsButton;

	[SerializeField]
	private SelectableButton _button;

	[SerializeField]
	private RectLayout _layout;

	private List<KeyLabelBase> _infoLabels = new List<KeyLabelBase>();

	private ListObjectPool<UISprite> _infoSeparators = new ListObjectPool<UISprite>();

	private SyncString _title;

	private SyncString _subTitle;

	private List<KeyValuePair> _infoList;

	private string _buttonText;

	private Action _onClickButton;

	private string _regionId;

	public SelectableButton Button => _button;

	public void SetTitle(SyncString text)
	{
		_title = text;
		MarkAsChanged();
	}

	public void SetSubtitle(SyncString text)
	{
		_subTitle = text;
		MarkAsChanged();
	}

	public void SetInfo<T>(int index, SyncString key, T value) where T : KeyLabelBase.IContent
	{
		if (index >= 0 && index <= _infoList.Count)
		{
			if (index == _infoList.Count)
			{
				_infoList.Add(new KeyValuePair(key, value));
			}
			else
			{
				_infoList[index] = new KeyValuePair(key, value);
			}
			MarkAsChanged();
		}
	}

	public void SetNotice([CanBeNull] string text)
	{
		bool active = !string.IsNullOrEmpty(text);
		_noticeWidget.gameObject.SetActive(active);
		_noticeLabel.text = text;
		MarkAsChanged();
	}

	public void SetButton(string text, string regionId, Action onClick)
	{
		if (!(_buttonText == text) || !(onClick == _onClickButton) || !(regionId == _regionId))
		{
			bool active = !string.IsNullOrEmpty(regionId);
			_mapsButton.gameObject.SetActive(active);
			_regionId = regionId;
			_buttonText = text;
			_onClickButton = onClick;
			MarkAsChanged();
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		_infoList = new List<KeyValuePair>();
		_infoSeparators.BaseObject = _infoSeparatorBase;
		_infoSeparators.UseBase = true;
		SelectableButton button = _button;
		button.Clicked = (Action)Delegate.Combine(button.Clicked, new Action(OnClickButton));
		SelectableButton mapsButton = _mapsButton;
		mapsButton.Clicked = (Action)Delegate.Combine(mapsButton.Clicked, new Action(OnClickMapsButton));
		_mapsButton.Text = T._("듀랑고 맵스");
		_gaugeLabelBase.gameObject.SetActive(value: false);
		_textLabelBase.gameObject.SetActive(value: false);
	}

	protected override void OnHide()
	{
		base.OnHide();
		_title = null;
		_subTitle = null;
		_infoList.Clear();
		_buttonText = null;
		_onClickButton = null;
	}

	protected override void FillData()
	{
		_titleLabel.SetText(_title);
		_subTitleLabel.SetText(_subTitle);
		UIUtility.DoPoolAsMethod<KeyLabelBase, KeyValuePair>(ref _infoLabels, _infoList, _infoWidget.transform, delegate(KeyValuePair k)
		{
			if (k.Value is SyncString)
			{
				return _textLabelBase;
			}
			return (k.Value is KeyGaugeLabel.Gauge) ? _gaugeLabelBase : null;
		}, delegate(KeyLabelBase labelObj, KeyValuePair data, int idx)
		{
			labelObj.Set(data.Key, data.Value);
		});
		_button.Text = _buttonText;
	}

	protected override void UpdateLayout()
	{
		int num = _titleLabel.height + (int)Mathf.Abs(_titleLabel.GetPosition(0f, 1f).y) * 2;
		if (!_subTitle.HasText())
		{
			_subTitleLabel.gameObject.SetActive(value: false);
		}
		else
		{
			_subTitleLabel.gameObject.SetActive(value: true);
			num += _subTitleLabel.height + _interTitleSubtitleLength;
		}
		_titleWidget.height = num;
		float num2 = Mathf.Max((float)_titleLabel.width + _titleLabel.GetPosition(0f, 0f).x * 2f, _minWidth);
		for (int i = 0; i < _infoLabels.Count; i++)
		{
			KeyLabelBase keyLabelBase = _infoLabels[i];
			if (keyLabelBase.gameObject.activeSelf)
			{
				num2 = Mathf.Max(num2, keyLabelBase.GetPreferredSize(_maxWidth).x);
			}
		}
		bool flag = !string.IsNullOrEmpty(_buttonText);
		if (flag)
		{
			num2 = Mathf.Max(_button.GetPreferredSize().x + _button.Widget.leftAnchor.absolute + -_button.Widget.rightAnchor.absolute, num2);
		}
		if (!string.IsNullOrEmpty(_regionId))
		{
			num2 = _widthWithMapsButton;
		}
		Vector3 position = _infoPanel.transform.position;
		int num3 = 0;
		int num4 = Mathf.CeilToInt(num2);
		int count = 0;
		for (int j = 0; j < _infoLabels.Count; j++)
		{
			if (_infoLabels[j].gameObject.activeSelf)
			{
				KeyLabelBase keyLabelBase2 = _infoLabels[j];
				keyLabelBase2.UpdateLayout(num4);
				keyLabelBase2.transform.localPosition = position + Vector3.down * num3;
				num3 += keyLabelBase2.Widget.height;
				if (j < _infoLabels.Count - 1)
				{
					UISprite orAdd = _infoSeparators.GetOrAdd(count++);
					orAdd.transform.localPosition = position + Vector3.down * num3;
					orAdd.width = num4;
				}
			}
		}
		_infoSeparators.Set(count);
		if (num3 == 0)
		{
			_infoWidget.gameObject.SetActive(value: false);
		}
		else
		{
			_infoWidget.gameObject.SetActive(value: true);
			_infoWidget.height = num3;
		}
		_buttonWidget.gameObject.SetActive(flag);
		_titleInfoSeparator.gameObject.SetActive(_infoWidget.gameObject.activeSelf || _buttonWidget.gameObject.activeSelf);
		_layout.UpdateLayout(num4, 0f);
		UIUtility.UpdateAnchors(base.transform);
	}

	private void OnClickButton()
	{
		if (_onClickButton != null)
		{
			_onClickButton();
		}
	}

	private void OnClickMapsButton()
	{
		string arg = ((!Debug.isDebugBuild) ? _regionId : string.Empty);
		Platform.Instance.ShowWeb(T._("듀랑고 맵스"), $"http://maps.durango.nexon.com/{arg}");
	}
}
