using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Logic.Mail;
using Durango.UI.Control;
using L10N;
using Messages;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI.Popup;

public class SunsetMailPopup : TooltipBase
{
	[SerializeField]
	private GameObject[] _closeButtons;

	[SerializeField]
	private RectLayoutComponent _viewerWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private KWidgetScrollView _mainScrollView;

	[SerializeField]
	private UILabel _mainLabel;

	[SerializeField]
	private UIWidget _attachedListWidget;

	[SerializeField]
	private MailAttachedItemWidget _attachedNodeBase;

	[SerializeField]
	private SelectableButton _replyButton;

	[SerializeField]
	private UILabel _endLabel;

	[SerializeField]
	private RectLayoutComponent _replyWidget;

	[SerializeField]
	private UILabel _replyTitleLabel;

	[SerializeField]
	private UIModelViewer _playerModelViewer;

	[SerializeField]
	private KWidgetScrollView _replyScrollView;

	[SerializeField]
	private UIInput _replyTextInput;

	[SerializeField]
	private UILabel _replayEndLabel;

	[SerializeField]
	private SelectableButton _sendButton;

	private Durango.Logic.Mail.Mail _mail;

	private bool _isSending;

	private ListObjectPool<MailAttachedItemWidget> _attachedNodes;

	protected override void OnAwake()
	{
		base.OnAwake();
		GameObject[] closeButtons = _closeButtons;
		for (int i = 0; i < closeButtons.Length; i++)
		{
			UIEventListener uIEventListener = UIEventListener.Get(closeButtons[i]);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
			{
				Hide();
			});
		}
		_replyButton.Text = T._("답장하기");
		_sendButton.Text = T._("보내기");
		_replyTitleLabel.text = T._("답장 쓰기");
		_replyTextInput.defaultText = T._("편지 내용을 입력해주세요.");
		SelectableButton replyButton = _replyButton;
		replyButton.Clicked = (Action)Delegate.Combine(replyButton.Clicked, new Action(ShowReplyPage));
		SelectableButton sendButton = _sendButton;
		sendButton.Clicked = (Action)Delegate.Combine(sendButton.Clicked, new Action(SendReply));
		_attachedNodes = new ListObjectPool<MailAttachedItemWidget>();
		_attachedNodes.BaseObject = _attachedNodeBase;
		_attachedNodes.UseBase = true;
		_attachedNodes.Clear();
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (_mail != null && !_mail.Accepted)
		{
			GameSystem<MailSystem>.Instance().AcceptMails(new List<Durango.Logic.Mail.Mail> { _mail }, null);
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		_viewerWidget.gameObject.SetActive(value: true);
		_replyWidget.gameObject.SetActive(value: false);
	}

	public void Set(Durango.Logic.Mail.Mail mail)
	{
		_mail = mail;
	}

	protected override void FillData()
	{
		base.FillData();
		Durango.Logic.Mail.Mail mail = _mail;
		mail.GetText(out var titleText, out var mainText);
		_titleLabel.text = titleText;
		string arg;
		string text;
		if (mainText == null)
		{
			arg = string.Empty;
			text = string.Empty;
		}
		else
		{
			mainText = mainText.Trim();
			int num = mainText.LastIndexOf('\n');
			if (num == -1)
			{
				arg = mainText;
				text = string.Empty;
			}
			else
			{
				arg = mainText.Substring(0, num).Trim();
				text = mainText.Substring(num).Trim();
			}
		}
		string text2 = PlayerBehavior.LocalPlayer.GetName();
		_mainLabel.text = string.Format("<em>{0}</em>\n\n{1}", T._("{0} 님께,", text2), arg);
		_endLabel.text = text;
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
			_attachedListWidget.height = 80 + (int)UIUtility.WidgetsGridReposition(_attachedNodes, null, Vector2.down, new Vector3(20f, -40f), (float)_attachedListWidget.width - 40f, _attachedNodeBase.localSize, 10f, 10f).y;
		}
		else
		{
			_attachedListWidget.gameObject.SetActive(value: false);
		}
		_replayEndLabel.text = T._("사랑을 담아, <em>{0}</em>", text2);
	}

	protected override void UpdateLayout()
	{
		base.UpdateLayout();
		base.Widget.SetAnchor(base.transform.parent.gameObject, 0, 0, 0, 0);
		UIUtility.UpdateAnchors(base.transform);
		_viewerWidget.UpdateLayout();
		_replyWidget.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
		_mainScrollView.ResetPosition();
		_replyScrollView.ResetPosition();
	}

	private void ShowReplyPage()
	{
		_viewerWidget.gameObject.SetActive(value: false);
		_replyWidget.gameObject.SetActive(value: true);
		_replyTextInput.value = string.Empty;
		_isSending = false;
		PlayerDisplay display = PlayerBehavior.LocalPlayer.Display;
		display.Equip = null;
		_playerModelViewer.SetPlayerModel(PlayerBehavior.LocalPlayer.IsMale, display, new UIModelViewer.Arguments
		{
			CameraAngle = 35f,
			Rotation = 140f,
			Loaded = delegate(GameObject obj)
			{
				PlayerBehavior component = obj.GetComponent<PlayerBehavior>();
				if (!(component == null) && !(component.Anim == null))
				{
					component.PlayMotionForcely("Emotion_Heart", 1f, immediately: true);
				}
			}
		});
	}

	private void SendReply()
	{
		string value = _replyTextInput.value;
		if (string.IsNullOrEmpty(value) || _isSending)
		{
			return;
		}
		_isSending = true;
		GameSystem<SendReportSystem>.Instance().SendServerStatus(value, "선셋 편지", delegate(bool result)
		{
			if (!result)
			{
				UIManager.SystemMsg(T._("전송에 실패했습니다."));
			}
			else
			{
				_isSending = false;
				Hide();
				UIManager.SystemMsg(T._("편지가 안전하게 전달되었습니다."));
			}
		});
	}
}
