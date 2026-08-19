using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Logic.Mail;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI;

public class MailContentsView : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private RectLayoutComponent _layout;

	[SerializeField]
	private RectLayoutComponent _mainLayout;

	[SerializeField]
	private GameObject _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _timeLabel;

	[SerializeField]
	private SelectableButton _actionButton;

	[SerializeField]
	private UIWidget _acceptedPlayerWidget;

	[SerializeField]
	private UILabel _acceptedPlayerLabel;

	[SerializeField]
	private UIWidget _attachedListWidget;

	[SerializeField]
	private MailAttachedItemWidget _attachedNodeBase;

	[SerializeField]
	private GameObject _separator;

	[SerializeField]
	private UIWidget _textWidget;

	[SerializeField]
	private UILabel _textLabel;

	private ListObjectPool<MailAttachedItemWidget> _attachedNodes;

	private bool _isWaiting;

	private MailGroup _parent;

	public Durango.Logic.Mail.Mail Mail { get; private set; }

	void IUIInitializable.Init()
	{
		SelectableButton actionButton = _actionButton;
		actionButton.Clicked = (Action)Delegate.Combine(actionButton.Clicked, new Action(OnClickActionButton));
		_attachedNodes = new ListObjectPool<MailAttachedItemWidget>();
		_attachedNodes.BaseObject = _attachedNodeBase;
		_attachedNodes.UseBase = true;
		_attachedNodes.Clear();
		_parent = UIUtility.FindComponentInParent<MailGroup>(base.gameObject);
		UIEventListener uIEventListener = UIEventListener.Get(_titleWidget.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			_parent.Close();
		});
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		_isWaiting = false;
	}

	public void Set(Durango.Logic.Mail.Mail mail)
	{
		Mail = mail;
		mail.GetText(out var titleText, out var mainText);
		_titleLabel.text = titleText;
		if (string.IsNullOrEmpty(mainText))
		{
			_textWidget.gameObject.SetActive(value: false);
		}
		else
		{
			_textWidget.gameObject.SetActive(value: true);
			_textLabel.text = mainText;
		}
		_timeLabel.SetText(mail.GetExpiresText());
		if (string.IsNullOrEmpty(mail.AcceptedEntityId))
		{
			_acceptedPlayerWidget.gameObject.SetActive(value: false);
		}
		else
		{
			_acceptedPlayerWidget.gameObject.SetActive(value: true);
			_acceptedPlayerLabel.text = string.Empty;
			Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(mail.AcceptedEntityId, delegate(Durango.Player.PlayerInfo info)
			{
				_acceptedPlayerLabel.text = T._("{0} 받음", "[icon=icon_person] " + info.GetNameFreq(20, string.Empty));
			});
		}
		if (KUtility.GetSize(mail.AttachedItems) + KUtility.GetSize(mail.AttachedVouchers) + KUtility.GetSize(mail.Money) > 0)
		{
			_attachedListWidget.gameObject.SetActive(value: true);
			_attachedNodes.BeginLoad();
			if (mail.AttachedItems != null)
			{
				ItemData[] attachedItems = mail.AttachedItems;
				foreach (ItemData item in attachedItems)
				{
					_attachedNodes.GetNext().Set(item).SetAccepted(mail.Accepted);
				}
			}
			if (mail.AttachedVouchers != null)
			{
				VoucherInfo[] attachedVouchers = mail.AttachedVouchers;
				foreach (VoucherInfo voucher in attachedVouchers)
				{
					_attachedNodes.GetNext().Set(voucher).SetAccepted(mail.Accepted);
				}
			}
			if (mail.Money != null)
			{
				foreach (KeyValuePair<Currency, int> item2 in mail.Money)
				{
					_attachedNodes.GetNext().Set(new Money(item2.Value, item2.Key)).SetAccepted(mail.Accepted);
				}
			}
			_attachedNodes.EndLoad();
			_attachedListWidget.height = 80 + (int)UIUtility.WidgetsGridReposition(_attachedNodes, null, Vector2.down, new Vector3(70f, -40f), (float)_attachedListWidget.width - 140f, _attachedNodeBase.localSize, 10f, 10f).y;
		}
		else
		{
			_attachedListWidget.gameObject.SetActive(value: false);
		}
		if (mail.Accepted)
		{
			_actionButton.Text = T._("버리기");
			_actionButton.SetStyle(PresetButton.Style.Border);
		}
		else
		{
			_actionButton.Text = T._("받기");
			_actionButton.SetStyle(PresetButton.Style.Solid);
		}
		_separator.gameObject.SetActive(_attachedListWidget.gameObject.activeSelf && _textWidget.gameObject.activeSelf);
		_layout.UpdateLayout();
		_mainLayout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
		_scrollView.ResetPosition();
		GameSystem<MailSystem>.Instance().MarkMailsAsRead(mail);
	}

	private void OnClickActionButton()
	{
		if (_isWaiting)
		{
			return;
		}
		if (Mail.Accepted)
		{
			_isWaiting = true;
			GameSystem<MailSystem>.Instance().DeleteMails(new List<Durango.Logic.Mail.Mail> { Mail }, delegate
			{
				_isWaiting = false;
			});
		}
		else
		{
			_isWaiting = true;
			GameSystem<MailSystem>.Instance().AcceptMails(new List<Durango.Logic.Mail.Mail> { Mail }, delegate
			{
				_isWaiting = false;
			});
		}
	}
}
