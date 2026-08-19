using System;
using System.Collections.Generic;
using Durango.UI.Control;
using L10N;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI.Popup;

public class PaidCurrencyInfoPopup : TooltipBase
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private PaidCurrencyInfoWidget _infoBase;

	[SerializeField]
	private UILabel _captionLabel;

	[SerializeField]
	private SelectableButton _confirmButton;

	[SerializeField]
	private RectLayout _layout;

	private readonly List<Currency> _types = new List<Currency>();

	private string _caption;

	private ListObjectPool<PaidCurrencyInfoWidget> _infos;

	protected override void Start()
	{
		base.Start();
		SelectableButton confirmButton = _confirmButton;
		confirmButton.Clicked = (Action)Delegate.Combine(confirmButton.Clicked, new Action(Hide));
		_titleLabel.text = T._("소지재화 확인");
		_confirmButton.Text = T._("확인");
		_infos = new ListObjectPool<PaidCurrencyInfoWidget>();
		_infos.BaseObject = _infoBase;
		_infos.UseBase = true;
	}

	public PaidCurrencyInfoPopup DefaultSetting()
	{
		SetCurrency(Currency.Coin);
		SetCurrency(Currency.Gem);
		SetCaption(T._("유료분부터 먼저 소비됩니다."));
		return this;
	}

	public void SetCurrency(Currency type)
	{
		_types.Add(type);
	}

	public void SetCaption(string caption)
	{
		_caption = caption;
	}

	protected override void FillData()
	{
		_infos.Set(_types.Count);
		if (_types.Count > 0)
		{
			int num = (base.Widget.width - (_types.Count + 1) * 20) / _types.Count;
			float num2 = (float)(base.Widget.width - _types.Count * num) / ((float)_types.Count + 1f);
			for (int i = 0; i < _infos.Count; i++)
			{
				_infos[i].width = num;
				_infos[i].Set(_types[i]);
			}
			Vector3[] localCorners = _infoBase.transform.parent.GetComponent<UIWidget>().localCorners;
			UIUtility.WidgetsReposition(_infos, Vector3.right, Vector3.Lerp(localCorners[0], localCorners[1], 0.5f) + Vector3.right * num2, num2);
		}
		if (string.IsNullOrEmpty(_caption))
		{
			_captionLabel.transform.parent.gameObject.SetActive(value: false);
			return;
		}
		_captionLabel.text = _caption;
		_captionLabel.transform.parent.gameObject.SetActive(value: true);
	}

	protected override void UpdateLayout()
	{
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
		base.Widget.SetPosition(Vector3.zero, 0.5f, 0.5f);
	}

	protected override void OnHide()
	{
		base.OnHide();
		_types.Clear();
		_caption = null;
	}

	protected override void OnTryConfirmOnModal()
	{
		Hide();
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _confirmButton;
	}
}
