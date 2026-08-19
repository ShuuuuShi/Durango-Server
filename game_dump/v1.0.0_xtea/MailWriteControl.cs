using System;
using ItemSystem;
using L10N;
using Player;
using UnityEngine;

public class MailWriteControl : MonoBehaviour
{
	public Action<ulong, string, ulong> RequestSendMail;

	[SerializeField]
	private UILabel _receiverLabel;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private PlayerSearchTextInput _receiverInput;

	[SerializeField]
	private UIInput _titleInput;

	[SerializeField]
	private UIInput _commentInput;

	[SerializeField]
	private DefaultSelectableButton _sendButton;

	[SerializeField]
	private DefaultSelectableButton _appendItemButton;

	private ItemData _selectedItem;

	private UIWidget _widget;

	private PlayerInfo _receiver;

	public UIWidget Widget => (!((Object)(object)_widget != (Object)null)) ? (_widget = ((Component)this).GetComponent<UIWidget>()) : _widget;

	public void Init()
	{
		OnLocalize();
		_sendButton.Clicked = OnClick_SendButton;
		_appendItemButton.Clicked = OnClick_AppendItemButton;
		_receiverInput.SelectPlayerChanged = SelectPlayerChanged;
		EventDelegate.Set(_titleInput.onChange, SendReadyCheck);
		EventDelegate.Set(_commentInput.onChange, SendReadyCheck);
	}

	private void SelectPlayerChanged(PlayerInfo playerInfo)
	{
		_receiver = playerInfo;
		SendReadyCheck();
	}

	private void SendReadyCheck()
	{
		if (_receiver == null || string.IsNullOrEmpty(_titleInput.value.Trim()))
		{
			_sendButton.Disable = true;
		}
		else
		{
			_sendButton.Disable = false;
		}
	}

	private void ClearInputField()
	{
		_titleInput.value = string.Empty;
		_commentInput.value = string.Empty;
	}

	private void OnClick_SendButton()
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		string text = _titleInput.value.Trim();
		string arg = _commentInput.value.Trim();
		string text2 = $"{text}\n{arg}".Trim();
		ulong arg2 = ((_selectedItem != null) ? _selectedItem.Id : 0);
		if (_receiver != null && !string.IsNullOrEmpty(text2))
		{
			ClearInputField();
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(T._("우편이 전송 되었습니다"), LocalizeSystem.Format("#send_mail_tooltip_comment", _receiver.Name, text), 400);
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Show(_sendButton.Widget, Vector2.op_Implicit(Vector3.up * 10f), 5f);
			if (RequestSendMail != null)
			{
				RequestSendMail(_receiver.EntityId, text2, arg2);
			}
		}
	}

	private void OnClick_AppendItemButton()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		PopupItemSelector popupItemSelector = UIManager.Popup.Tooltip<PopupItemSelector>();
		popupItemSelector.Direction = TooltipBase.TooltipDirection.Vertical;
		popupItemSelector.Set(null, 1, null, displayTooltip: true, OnSelectItem);
		popupItemSelector.Show(_appendItemButton.Widget, Vector2.zero, 3600f);
	}

	private void OnSelectItem(ItemData item)
	{
		_selectedItem = item;
	}

	public void Show(PlayerInfo receiver = null)
	{
		((Component)this).gameObject.SetActive(true);
		Widget.alpha = 0f;
		TweenAlpha.Begin(((Component)this).gameObject, 0.5f, 1f);
		ClearInputField();
		_receiverInput.SetPlayer(receiver);
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void OnLocalize()
	{
		_receiverLabel.text = T._("받는 사람");
		_titleLabel.text = T._("제목");
		CalcLabelBackground();
		_receiverInput.Input.defaultText = T._("아이디를 입력하세요");
		_titleInput.defaultText = T._("제목을 입력하세요");
	}

	private void CalcLabelBackground()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Max(_receiverLabel.printedSize.x, _titleLabel.printedSize.x);
		int width = (int)(num + ((Component)_receiverLabel).transform.localPosition.x * 2f);
		((Component)((Component)_receiverLabel).transform.parent).GetComponent<UIWidget>().width = width;
		((Component)((Component)_titleLabel).transform.parent).GetComponent<UIWidget>().width = width;
	}
}
