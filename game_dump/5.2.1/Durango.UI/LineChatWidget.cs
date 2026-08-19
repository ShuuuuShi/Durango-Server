using System;
using Durango.Logic.Social;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Chat;
using UnityEngine;

namespace Durango.UI;

public class LineChatWidget : MonoBehaviour
{
	public static bool On = true;

	[SerializeField]
	private GameObject _portraitBg;

	[SerializeField]
	private UIWidget _typeWidget;

	[SerializeField]
	private UILabel _typeLabel;

	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private UIWidget _textWidget;

	[SerializeField]
	private UILabel _chattingLine;

	[SerializeField]
	private Texture _portraitMask;

	[SerializeField]
	private float _visibleDuration;

	[SerializeField]
	private GameObject _chatBg;

	private int _channelPadding;

	private int _channelMinWidth;

	private int _textMargin;

	private int _textRightMargin;

	private ChatStruct _chatData;

	private Durango.Logic.Social.Conversation _conversation;

	private float _hideAt;

	private AnimationWidget _animWidget;

	private bool _isShowContextAction;

	private bool _isShowTodoList;

	private void Awake()
	{
		_channelMinWidth = _typeWidget.width;
		_channelPadding = _typeWidget.width - _typeLabel.width;
		_textMargin = _textWidget.leftAnchor.absolute - _typeWidget.rightAnchor.absolute;
		_textRightMargin = _textWidget.rightAnchor.absolute;
		_animWidget = GetComponent<AnimationWidget>();
		_portraitTexture.gameObject.SetActive(value: false);
		_chattingLine.text = string.Empty;
		_typeLabel.text = ChannelType.Region.GetName();
		UIManager.AddOnScreenResized(OnScreenResize);
	}

	private void Start()
	{
		UIEventListener.Get(base.gameObject).onClick = delegate
		{
			ChattingGroupBase chattingGroupBase = UIManager.FindScript<ChattingGroupBase>();
			if (_conversation == null)
			{
				chattingGroupBase.Open();
			}
			else
			{
				chattingGroupBase.Open(_conversation);
			}
		};
		Observable<bool> isShow = UIManager.FindScript<ContextActionGroupBase>().IsShow;
		isShow.Changed = (Action<bool>)Delegate.Combine(isShow.Changed, new Action<bool>(OnShowContextAction));
		UIManager.FindScript<ToDoListGroupBase>().AddWidthOnChanged(OnTodoWidgetChange);
		base.enabled = _hideAt > 0f;
	}

	private void Update()
	{
		if (_hideAt > 0f)
		{
			if (_hideAt < Time.time)
			{
				HideText(instant: false);
			}
		}
		else
		{
			base.enabled = false;
		}
	}

	private void OnShowContextAction(bool isShow)
	{
		bool isShowContextAction = _isShowContextAction;
		_isShowContextAction = !UIManager.IsPortraitScreen && isShow;
		if (isShowContextAction != _isShowContextAction)
		{
			RefreshTextRightOffset();
		}
	}

	private void OnTodoWidgetChange(float ratio)
	{
		bool isShowTodoList = _isShowTodoList;
		_isShowTodoList = ratio < 1f;
		if (isShowTodoList != _isShowTodoList)
		{
			RefreshTextRightOffset();
		}
	}

	private void RefreshTextRightOffset()
	{
		int num = (_isShowContextAction ? (-100) : 0);
		num -= (_isShowTodoList ? ToDoListGroupBase.Width : 0);
		SetTextRightOffset(num);
	}

	private void OnScreenResize()
	{
		bool isPortraitScreen = UIManager.IsPortraitScreen;
		_textWidget.rightAnchor.absolute = ((!isPortraitScreen) ? _textRightMargin : 0);
		_portraitBg.gameObject.SetActive(isPortraitScreen);
		_chatBg.SetActive(!isPortraitScreen);
		BoxCollider component = GetComponent<BoxCollider>();
		if (component != null)
		{
			component.enabled = isPortraitScreen;
		}
		UIUtility.UpdateAnchors(base.transform);
		if (isPortraitScreen)
		{
			ShowText(0f, instant: true);
		}
		else
		{
			HideText(instant: true);
		}
		OnShowContextAction(UIManager.FindScript<ContextActionGroupBase>().IsShow);
	}

	private void ShowText(float duration, bool instant)
	{
		_animWidget.SetAlpha(1f, !instant);
		_hideAt = ((!(duration > 0f)) ? 0f : (Time.time + duration));
		base.enabled = _hideAt > 0f;
	}

	private void HideText(bool instant)
	{
		base.enabled = false;
		_hideAt = 0f;
		_animWidget.SetAlpha(0f, !instant);
	}

	public void Add(ChatStruct chat, Durango.Logic.Social.Conversation conv = null)
	{
		if (!On || chat.Type == ChannelType.System || chat.IsEventMessage())
		{
			return;
		}
		if (conv != null)
		{
			if (GameSystem<SocialSystem>.Instance().ChannelInfo.IsHidden(conv))
			{
				return;
			}
		}
		else if (GameSystem<SocialSystem>.Instance().ChannelInfo.IsHidden(chat.Type))
		{
			return;
		}
		_chatData = chat;
		_conversation = conv;
		if (conv == null)
		{
			SetLineText(_chatData);
		}
		else
		{
			Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(_chatData.EntityId, OnResponsePlayerInfo);
		}
	}

	private void OnResponsePlayerInfo(Durango.Player.PlayerInfo playerInfo)
	{
		if (!(_chatData.EntityId != playerInfo.EntityId))
		{
			SetLineText(_chatData, playerInfo);
		}
	}

	private void SetLineText(ChatStruct chat, Durango.Player.PlayerInfo info = null)
	{
		if (_conversation != null && info != null && info.Valid)
		{
			_portraitTexture.gameObject.SetActive(value: true);
			PortraitBuilder.Argument portraitArgument = info.GetPortraitArgument();
			portraitArgument.Mask = _portraitMask;
			PortraitBuilder.Set(portraitArgument, _portraitTexture);
			_typeWidget.rightAnchor.absolute = _typeWidget.leftAnchor.absolute + _channelMinWidth;
			_typeLabel.gameObject.SetActive(value: false);
		}
		else
		{
			_typeLabel.gameObject.SetActive(value: true);
			_typeLabel.text = chat.Type.GetName();
			int num = Mathf.Max(_channelMinWidth, _typeLabel.width + _channelPadding);
			_typeWidget.rightAnchor.absolute = _typeWidget.leftAnchor.absolute + num;
			_portraitTexture.gameObject.SetActive(value: false);
		}
		_textWidget.leftAnchor.absolute = _typeWidget.rightAnchor.absolute + _textMargin;
		_chattingLine.text = ToLineText(chat);
		UIUtility.UpdateAnchors(base.transform);
		if (!UIManager.IsPortraitScreen)
		{
			ShowText(_visibleDuration, instant: false);
		}
	}

	private void SetTextRightOffset(int offset)
	{
		if (_chattingLine.rightAnchor.absolute != offset)
		{
			_chattingLine.rightAnchor.absolute = offset;
			_chattingLine.UpdateAnchors();
		}
	}

	private static string ToLineText(ChatStruct chat)
	{
		return string.Format((!(chat.Body is RadioDictation)) ? "[{3}]{0}[-] [{2}]{1}[-]" : "[{3}]{0}[-] [{2}][icon=icon_stt_s_stroke] {1}[-]", chat.Name, chat.FindText().Replace("\n", " "), NGUIText.EncodeColor(chat.GetMsgColor(Color.white)), NGUIText.EncodeColor(chat.GetNameColor()));
	}
}
