using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Item;
using Durango.Logic.Mail;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Shared.Economy;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class MailNodeWidget : MonoBehaviour
{
	public Action<MailNodeWidget> Clicked;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _timeLabel;

	[SerializeField]
	private UILabel _acceptedLabel;

	[SerializeField]
	private UIWidget _attachedAreaWidget;

	[SerializeField]
	private UIWidget _attachedItemWidget;

	[SerializeField]
	private UIWidget _emptyAttachedWidget;

	[SerializeField]
	private UISprite _spriteIcon;

	[SerializeField]
	private ItemIconTex _itemIcon;

	[SerializeField]
	private UILabel _firstCountLabel;

	[SerializeField]
	private UIWidget _attachedCountAreaWidget;

	[SerializeField]
	private UILabel _attachedCountLabel;

	[SerializeField]
	private SelectableButton _actionButton;

	[SerializeField]
	private GameObject _newLabel;

	private bool _isWaiting;

	private RectLayoutComponent _layout;

	public Mail Data { get; private set; }

	private void OnDisable()
	{
		_isWaiting = false;
	}

	public void Init()
	{
		SelectableButton actionButton = _actionButton;
		actionButton.Clicked = (Action)Delegate.Combine(actionButton.Clicked, new Action(OnClick_AcceptButton));
		_layout = GetComponent<RectLayoutComponent>();
	}

	private void OnClick_AcceptButton()
	{
		if (_isWaiting)
		{
			return;
		}
		if (Data.IsGm())
		{
			OnClick();
		}
		else if (Data.Accepted)
		{
			_isWaiting = true;
			GameSystem<MailSystem>.Instance().DeleteMails(new List<Mail> { Data }, delegate
			{
				_isWaiting = false;
			});
		}
		else if (Data.Highlighted && !Data.IsRead)
		{
			OnClick();
		}
		else
		{
			_isWaiting = true;
			GameSystem<MailSystem>.Instance().AcceptMails(new List<Mail> { Data }, delegate
			{
				_isWaiting = false;
			});
		}
	}

	private void OnClick()
	{
		if (Clicked != null)
		{
			Clicked(this);
		}
	}

	public void Set(Mail data)
	{
		Data = data;
		UpdateContentWidget();
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	private void UpdateContentWidget()
	{
		Mail data = Data;
		data.GetText(out var titleText, out var _);
		_titleLabel.text = titleText;
		_newLabel.gameObject.SetActive(!data.IsRead);
		if (string.IsNullOrEmpty(data.AcceptedEntityId))
		{
			_acceptedLabel.gameObject.SetActive(value: false);
		}
		else
		{
			_acceptedLabel.gameObject.SetActive(value: true);
			_acceptedLabel.text = string.Empty;
			Durango.Utils.Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(data.AcceptedEntityId, delegate(PlayerInfo info)
			{
				_acceptedLabel.text = T._("{0} 받음", "[icon=icon_person] " + info.GetNameFreq(20, string.Empty));
			});
		}
		int num = KUtility.GetSize(data.AttachedItems) + KUtility.GetSize(data.AttachedVouchers) + KUtility.GetSize(data.Money);
		if (num > 0)
		{
			_attachedItemWidget.gameObject.SetActive(value: true);
			_emptyAttachedWidget.gameObject.SetActive(value: false);
			_attachedAreaWidget.alpha = ((!data.Accepted) ? 1f : 0.3f);
		}
		else
		{
			_attachedItemWidget.gameObject.SetActive(value: false);
			_emptyAttachedWidget.gameObject.SetActive(value: true);
			_attachedAreaWidget.alpha = 1f;
		}
		if (num > 1)
		{
			_attachedCountAreaWidget.gameObject.SetActive(value: true);
			_attachedCountLabel.text = $"+{num}";
			_attachedCountAreaWidget.width = _attachedCountLabel.width + 10;
			UIUtility.UpdateAnchors(_attachedCountAreaWidget.transform);
		}
		else
		{
			_attachedCountAreaWidget.gameObject.SetActive(value: false);
		}
		if (KUtility.GetSize(data.AttachedItems) > 0)
		{
			_spriteIcon.gameObject.SetActive(value: false);
			_itemIcon.gameObject.SetActive(value: true);
			_firstCountLabel.gameObject.SetActive(value: false);
			_itemIcon.SetIcon(data.AttachedItems[0]);
		}
		else if (KUtility.GetSize(data.AttachedVouchers) > 0)
		{
			_spriteIcon.gameObject.SetActive(value: true);
			_itemIcon.gameObject.SetActive(value: false);
			Voucher voucher = SingletonDict<string, Voucher>.Get(data.AttachedVouchers[0].VoucherId);
			_spriteIcon.spriteName = voucher.Icon;
			_spriteIcon.color = NGUIText.ParseColor24(voucher.GetHexColor());
			UIUtility.ResizeToSquare(_spriteIcon);
			if (data.AttachedVouchers[0].Count > 1)
			{
				_firstCountLabel.gameObject.SetActive(value: true);
				_firstCountLabel.text = data.AttachedVouchers[0].Count.ToString();
			}
			else
			{
				_firstCountLabel.gameObject.SetActive(value: false);
			}
		}
		else if (KUtility.GetSize(data.Money) > 0)
		{
			_spriteIcon.gameObject.SetActive(value: true);
			_itemIcon.gameObject.SetActive(value: false);
			KeyValuePair<Currency, int> keyValuePair = data.Money.First();
			_spriteIcon.spriteName = Inventory.GetIcon(keyValuePair.Key);
			_spriteIcon.color = Color.white;
			UIUtility.ResizeToSquare(_spriteIcon);
			_firstCountLabel.gameObject.SetActive(value: true);
			_firstCountLabel.text = keyValuePair.Value.ToString();
		}
		if (data.Accepted)
		{
			if (data.IsGm())
			{
				_actionButton.Text = T._("읽기");
				_actionButton.SetStyle(PresetButton.Style.Border);
			}
			else
			{
				_actionButton.Text = T._("버리기");
				_actionButton.SetStyle(PresetButton.Style.Border);
			}
		}
		else if (Data.Highlighted && !Data.IsRead)
		{
			_actionButton.Text = T._("읽기");
			_actionButton.SetStyle(PresetButton.Style.Solid);
		}
		else
		{
			_actionButton.Text = T._("받기");
			_actionButton.SetStyle(PresetButton.Style.Solid);
		}
		_timeLabel.SetText(data.GetExpiresText());
	}
}
