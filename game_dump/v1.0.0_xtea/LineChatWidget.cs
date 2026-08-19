using System.Collections;
using ChatData;
using JetBrains.Annotations;
using L10N;
using Messages;
using Player;
using Shared.Chat;
using UnityEngine;

public class LineChatWidget : MonoBehaviour
{
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
	private UISpriteLabel _chattingLine;

	[SerializeField]
	private Texture _portraitMask;

	[SerializeField]
	private float _visibleDuration;

	private UIWidget _widget;

	private int _leftAnchor;

	private int _topAnchor;

	private int _channelPadding;

	private int _channelMinWidth;

	private int _textMargin;

	private int _textRightMargin;

	private ChatStruct _chatData;

	private ChatData.Conversation _conversation;

	private bool _isTimerActive;

	private float _hideAt;

	private AnimationWidget _animWidget;

	private bool _isShowContextAction;

	private bool _isShowTodoList;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	private void Awake()
	{
		_channelMinWidth = _typeWidget.width;
		_channelPadding = _typeWidget.width - _typeLabel.width;
		_textMargin = _textWidget.leftAnchor.absolute - _typeWidget.rightAnchor.absolute;
		_textRightMargin = _textWidget.rightAnchor.absolute;
		_leftAnchor = Widget.leftAnchor.absolute;
		_topAnchor = Widget.topAnchor.absolute;
		_animWidget = ((Component)this).GetComponent<AnimationWidget>();
	}

	private void Start()
	{
		UIEventListener.Get(((Component)this).gameObject).onClick = delegate
		{
			ChattingGroup chattingGroup = UIManager.FindScript<ChattingGroup>();
			if (_conversation == null)
			{
				chattingGroup.Open();
			}
			else
			{
				chattingGroup.Open(_conversation);
			}
		};
		((Component)_portraitTexture).gameObject.SetActive(false);
		UIManager.FindScript<ContextActionGroup>().OnShowContextAction += OnShowContextAction;
		UIManager.FindScript<ToDoListGroup>().WidthRatioChanged += OnTodoWidgetChange;
	}

	private void OnShowContextAction(bool isShow)
	{
		bool isShowContextAction = _isShowContextAction;
		_isShowContextAction = !UIManager.IsPortraitMode && isShow;
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
		num -= (_isShowTodoList ? 100 : 0);
		SetTextRightOffset(num);
	}

	private IEnumerator CoHideTimer()
	{
		while (_isTimerActive && _hideAt > 0f)
		{
			if (_hideAt < Time.time)
			{
				HideText(instant: false);
				break;
			}
			float remain = _hideAt - Time.time;
			if (remain <= 0f)
			{
				break;
			}
			yield return null;
		}
		_isTimerActive = false;
	}

	[UsedImplicitly]
	private void OnPortraitMode(bool isPortrait)
	{
		if (isPortrait)
		{
			Widget.leftAnchor.absolute = 0;
			Widget.topAnchor.absolute = 80;
			_textWidget.rightAnchor.absolute = 0;
		}
		else
		{
			Widget.leftAnchor.absolute = _leftAnchor;
			Widget.topAnchor.absolute = _topAnchor;
			_textWidget.rightAnchor.absolute = _textRightMargin;
		}
		_portraitBg.gameObject.SetActive(isPortrait);
		BoxCollider component = ((Component)this).GetComponent<BoxCollider>();
		if ((Object)(object)component != (Object)null)
		{
			((Collider)component).enabled = isPortrait;
		}
		UIUtility.UpdateAnchors(((Component)this).transform);
		if (isPortrait)
		{
			ShowText(0f, instant: true);
		}
		else
		{
			HideText(instant: true);
		}
		OnShowContextAction(UIManager.FindScript<ContextActionGroup>().IsShow);
	}

	private void ShowText(float duration, bool instant)
	{
		_animWidget.SetAlpha(1f, !instant);
		_hideAt = ((!(duration > 0f)) ? 0f : (Time.time + duration));
		if (_hideAt > 0f && !_isTimerActive)
		{
			((MonoBehaviour)this).StartCoroutine(CoHideTimer());
		}
	}

	private void HideText(bool instant)
	{
		_animWidget.SetAlpha(0f, !instant);
	}

	public void Add(ChatStruct chat, ChatData.Conversation conv = null)
	{
		if (chat.Type != ChannelType.System && !(chat.Body is RadioEntered) && !(chat.Body is RadioLeft))
		{
			_chatData = chat;
			_conversation = conv;
			KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(_chatData.EntityId, OnResponsePlayerInfo);
		}
	}

	private void OnResponsePlayerInfo(Player.PlayerInfo playerInfo)
	{
		if (_chatData.EntityId == playerInfo.EntityId)
		{
			if (_conversation != null && playerInfo.Valid)
			{
				((Component)_portraitTexture).gameObject.SetActive(true);
				PortraitBuilder.Argument portraitArgument = playerInfo.GetPortraitArgument();
				portraitArgument.Mask = _portraitMask;
				PortraitBuilder.Set(portraitArgument, _portraitTexture);
				_typeWidget.rightAnchor.absolute = _typeWidget.leftAnchor.absolute + _channelMinWidth;
				((Component)_typeLabel).gameObject.SetActive(false);
			}
			else
			{
				((Component)_typeLabel).gameObject.SetActive(true);
				_typeLabel.text = _chatData.Type.GetName();
				int num = Mathf.Max(_channelMinWidth, _typeLabel.width + _channelPadding);
				_typeWidget.rightAnchor.absolute = _typeWidget.leftAnchor.absolute + num;
				((Component)_portraitTexture).gameObject.SetActive(false);
			}
			_textWidget.leftAnchor.absolute = _typeWidget.rightAnchor.absolute + _textMargin;
			_chattingLine.text = ToLineText(_chatData, playerInfo, _chatData.Body is RadioDictation);
			UIUtility.UpdateAnchors(((Component)this).transform);
			if (!UIManager.IsPortraitMode)
			{
				ShowText(_visibleDuration, instant: false);
			}
		}
	}

	private void SetTextRightOffset(int offset)
	{
		int absolute = _chattingLine.Label.rightAnchor.absolute;
		if (absolute != offset)
		{
			_chattingLine.Label.rightAnchor.absolute = offset;
			_chattingLine.Label.UpdateAnchors();
		}
	}

	private static string ToLineText(ChatStruct chat, Player.PlayerInfo info, bool isStt)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		string format = ((!isStt) ? "{3}{0}[-] {2}{1}[-]" : "{3}{0}[-] {2}[icon=icon_stt_s_stroke] {1}[-]");
		return string.Format(format, (!info.Valid) ? chat.Name : info.Name, chat.FindText().Replace("\n", " "), UIManager.ColorBBCode(chat.GetMsgColor(Color.white)), UIManager.ColorBBCode(chat.GetNameColor()));
	}
}
