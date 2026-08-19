using System;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class ChattingInputControl_PC : MonoBehaviour, IUIInitializable, IUICursorChangable
{
	public Action<string> Submitted;

	[SerializeField]
	private UIInput _inputLabel;

	[SerializeField]
	private UILabel _channelLabel;

	[SerializeField]
	private UILabel _defaultLabel;

	[SerializeField]
	private bool _deleteTextOnFocus;

	private string _channelName;

	private int _inputLabelDefaultLeftMargin;

	private bool _isConnected;

	private bool _isEnabled;

	private bool _isAllChatChannel;

	private bool _needUpdateLayout;

	public UIWidget Widget => GetComponent<UIWidget>();

	public UIWidget InputLabelWidget => _inputLabel.GetComponent<UIWidget>();

	public bool IsAvailable => _isConnected && _isEnabled;

	public bool IsFocused => _inputLabel.isSelected;

	public bool IsAllChatChannel
	{
		get
		{
			return _isAllChatChannel;
		}
		set
		{
			_isAllChatChannel = value;
			_channelLabel.gameObject.SetActive(value);
			RefreshInternal();
		}
	}

	bool IUICursorChangable.IsCursorChangable()
	{
		return true;
	}

	bool IUICursorChangable.IsCursorSpecified(ref GameCursorType cursorType)
	{
		cursorType = GameCursorType.Chatting;
		return true;
	}

	void IUIInitializable.Init()
	{
		_inputLabelDefaultLeftMargin = InputLabelWidget.leftAnchor.absolute;
		GameSystem<InputSystem>.Instance().On(InputCommand.SelectAllChatInput, delegate
		{
			if (IsFocused)
			{
				_inputLabel.selectionStart = 0;
			}
		});
		EventDelegate.Add(_inputLabel.onSubmit, delegate
		{
			if (Submitted != null)
			{
				Submitted(_inputLabel.value);
			}
			_inputLabel.value = string.Empty;
		});
		EventDelegate.Add(_inputLabel.onChange, delegate
		{
			string text = _inputLabel.label.text;
			_defaultLabel.enabled = string.IsNullOrEmpty(text);
		});
		UIEventListener.Get(base.gameObject).onClick = delegate
		{
			SetFocus(isSelected: true, isClearText: false);
		};
	}

	public void SetEnabled(bool isEnabled)
	{
		if (_isEnabled != isEnabled)
		{
			_isEnabled = isEnabled;
			Refresh();
		}
	}

	public void SetChannelName(string channelName)
	{
		_channelName = $"[{channelName}]";
		RefreshInternal();
	}

	public void SetFocus(bool isSelected, bool isClearText = true)
	{
		RefreshInternal();
		SetFocusInternal(isSelected && IsAvailable, isClearText);
	}

	private void SetFocusInternal(bool isFocus, bool isClearText = true)
	{
		_inputLabel.isSelected = isFocus;
		if (_deleteTextOnFocus && isClearText)
		{
			_inputLabel.value = string.Empty;
		}
	}

	public void Refresh()
	{
		RefreshInternal();
		if (!IsAvailable && IsFocused)
		{
			SetFocusInternal(isFocus: false);
		}
	}

	private void RefreshInternal()
	{
		_isConnected = GameSystem<SocialSystem>.Instance().CanSay();
		_needUpdateLayout = true;
	}

	private void UpdateLayout()
	{
		_channelLabel.GetComponent<SelectableWidget>().Selected = IsFocused;
		_inputLabel.gameObject.SetActive(IsAvailable);
		if (IsAvailable)
		{
			_channelLabel.text = _channelName;
		}
		else
		{
			_channelLabel.text = ((!_isEnabled) ? T._("채팅 불가") : T._("재접속 중..."));
		}
		_channelLabel.ProcessText();
		InputLabelWidget.leftAnchor.absolute = _inputLabelDefaultLeftMargin;
		if (IsAllChatChannel)
		{
			int num = _inputLabelDefaultLeftMargin + _channelLabel.width;
			InputLabelWidget.leftAnchor.absolute += num;
		}
		InputLabelWidget.ResetAndUpdateAnchors();
		UIUtility.ResetAndUpdateAnchors(base.transform);
	}

	private void LateUpdate()
	{
		if (_needUpdateLayout)
		{
			_needUpdateLayout = false;
			UpdateLayout();
		}
	}
}
