using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Social;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Chat;
using UnityEngine;

namespace Durango.UI;

public class ChattingLineBase : MonoBehaviour
{
	public Action<string> NameLabelClicked;

	public Action<ChatStruct> LinkClicked;

	public Action HeightChanged;

	[SerializeField]
	protected UILabel NameLabel;

	[SerializeField]
	protected UILabel TextLabel;

	[SerializeField]
	private SelectableWidget _actionButton;

	[SerializeField]
	private UISprite _actionButtonIcon;

	[SerializeField]
	private SelectableWidget _translationButton;

	[SerializeField]
	private UILabel _srcLang;

	[SerializeField]
	private GameObject _canTranslateIcon;

	[SerializeField]
	private UILabel _eventMessageLabel;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private GameObject _sttIcon;

	[SerializeField]
	private float _nameColliderWidthPadding;

	[SerializeField]
	private int _verticalPadding;

	[SerializeField]
	private int _appendTextSize;

	[SerializeField]
	private Color _appendTextColor;

	private Color _defaultTextColor = Color.white;

	private UIWidget _widget;

	private string _appendTextColorString;

	private int _height;

	private readonly List<ChatStruct> _chatItems = new List<ChatStruct>();

	[CanBeNull]
	private ChatStruct ChatData => _chatItems.FirstOrDefault();

	public string EntityId => (ChatData == null) ? string.Empty : ChatData.EntityId;

	public string Name => (ChatData == null) ? string.Empty : ChatData.Name;

	public ChannelType Type => (ChatData != null) ? ChatData.Type : ChannelType.Region;

	public ChatStruct.ChatMsgType MsgType => (ChatData != null) ? ChatData.MsgType : ChatStruct.ChatMsgType.Talk;

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				_widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public int VerticalPadding => _verticalPadding;

	public Vector3 Position
	{
		get
		{
			return base.transform.localPosition;
		}
		set
		{
			base.transform.localPosition = value;
		}
	}

	protected virtual void Awake()
	{
		_appendTextColorString = _appendTextColor.ToHex();
	}

	private void Start()
	{
		UIEventListener.Get(NameLabel.gameObject).onClick = OnClickNameLabel;
		SelectableWidget actionButton = _actionButton;
		actionButton.Clicked = (Action)Delegate.Combine(actionButton.Clicked, new Action(OnClickLink));
		_actionButton.SetClickSound(UISound.ClickType.ButtonDefault);
		SelectableWidget translationButton = _translationButton;
		translationButton.Clicked = (Action)Delegate.Combine(translationButton.Clicked, (Action)delegate
		{
			bool translationOn = !IsAllTranslated();
			bool flag = true;
			foreach (ChatStruct chatItem in _chatItems)
			{
				chatItem.TranslationOn = translationOn;
				if (flag)
				{
					SetText(chatItem);
					flag = false;
				}
				else
				{
					AppendText(chatItem);
				}
			}
			UpdateButtons();
			UpdateWidgetHeight();
		});
	}

	private void OnClickNameLabel(GameObject obj)
	{
		UISound.PlayClick(UISound.ClickType.ButtonDefault);
		if (NameLabelClicked != null)
		{
			NameLabelClicked(EntityId);
		}
	}

	private void OnClickLink()
	{
		if (LinkClicked != null)
		{
			LinkClicked(ChatData);
		}
	}

	public void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
	}

	private void SetEventState(bool isEvent)
	{
		NameLabel.gameObject.SetActive(!isEvent);
		TextLabel.gameObject.SetActive(!isEvent);
		_eventMessageLabel.gameObject.SetActive(isEvent);
		_sttIcon.SetActive(value: false);
	}

	private void SetEventMessage(ChatStruct chat, IList<Durango.Player.PlayerInfo> playerInfos)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < playerInfos.Count; i++)
		{
			Durango.Player.PlayerInfo playerInfo = playerInfos[i];
			list.Add(playerInfo.Valid ? playerInfo.Name : T._("알수없음"));
		}
		string text = string.Format("[a9a278]{0}[-]", T._("{0:l:{}|, }", list));
		string arg = string.Empty;
		if (chat.Body is RadioEntered)
		{
			arg = T._("{0} 님이 입장했습니다.", text);
		}
		else if (chat.Body is RadioLeft)
		{
			arg = T._("{0} 님이 퇴장했습니다.", text);
		}
		if (!string.IsNullOrEmpty(chat.Name) && chat.Name.StartsWith("#"))
		{
			string text2 = T._(LocalizeSystem.Get(chat.Name), text);
			if (!string.IsNullOrEmpty(text2))
			{
				arg = text2;
			}
		}
		string text3 = $"[7e7e64][icon=icon_person] {arg}[-]";
		_eventMessageLabel.text = text3;
		UpdateWidgetHeight();
	}

	public virtual void SetChat(ChatStruct chat, bool isAllChat = false)
	{
		_chatItems.Clear();
		_chatItems.Add(chat);
		if (chat.IsEventMessage())
		{
			SetEventState(isEvent: true);
			_actionButton.gameObject.SetActive(value: false);
			string[] entityIds = null;
			if (chat.Body is RadioEntered)
			{
				entityIds = ((RadioEntered)chat.Body).EntityIds;
			}
			else if (chat.Body is RadioLeft)
			{
				entityIds = ((RadioLeft)chat.Body).EntityIds;
			}
			Singleton<PlayerInfoManager>.Instance().RequestPlayerInfos(entityIds, delegate(Durango.Player.PlayerInfo[] infos)
			{
				SetEventMessage(chat, infos);
			});
			return;
		}
		SetEventState(isEvent: false);
		if (chat.IsNoticeMessage())
		{
			SetName("[icon=icon_speaker]");
			TextLabel.text = chat.FindText();
			NameLabel.color = chat.GetMsgColor(_defaultTextColor);
		}
		else
		{
			SetName((chat.Type != ChannelType.System || !string.IsNullOrEmpty(chat.Name)) ? chat.Name : T._("시스템"));
			SetText(chat);
			NameLabel.color = chat.GetNameColor();
		}
		TextLabel.color = chat.GetMsgColor(_defaultTextColor);
		UpdateButtons();
		UpdateWidgetHeight();
	}

	public virtual void AppendChat(ChatStruct chat)
	{
		_chatItems.Add(chat);
		AppendText(chat);
		UpdateButtons();
		UpdateWidgetHeight();
	}

	protected virtual void SetText(ChatStruct chat)
	{
		string format = $"{{0}} [c][{_appendTextColorString}][size={_appendTextSize}]{{1}}[/size][-][/c]";
		TextLabel.text = string.Format(format, chat.FindText(), Times.Timeago(chat.Time));
	}

	private void AppendText(ChatStruct chat)
	{
		string format = $"{{0}}\n{{1}} [c][{_appendTextColorString}][size={_appendTextSize}]{{2}}[/size][-][/c]";
		TextLabel.text = string.Format(format, TextLabel.text, chat.FindText(), Times.Timeago(chat.Time));
	}

	private void UpdateButtons()
	{
		bool active = false;
		if (_chatItems.Any((ChatStruct x) => x.Translatable))
		{
			_translationButton.gameObject.SetActive(value: true);
			_actionButton.gameObject.SetActive(value: false);
			bool flag = IsAllTranslated();
			_canTranslateIcon.SetActive(!flag);
			_srcLang.gameObject.SetActive(flag);
			if (flag)
			{
				ChatStruct chatStruct = _chatItems.FirstOrDefault((ChatStruct x) => x.Translatable && !string.IsNullOrEmpty(x.SourceLang));
				string text = ((chatStruct == null) ? "EN" : chatStruct.SourceLang.ToUpperInvariant());
				if (text.Length > 2)
				{
					text = text.Substring(0, 2);
				}
				_srcLang.text = text;
			}
		}
		else
		{
			ChatStruct.ChatMsgType msgType = MsgType;
			if (msgType == ChatStruct.ChatMsgType.Link)
			{
				_actionButton.gameObject.SetActive(value: true);
				_sttIcon.SetActive(value: false);
				_canTranslateIcon.SetActive(value: false);
				_srcLang.gameObject.SetActive(value: false);
				ChatStruct chatData = ChatData;
				string spriteName = string.Empty;
				Color color = Color.white;
				if (chatData != null)
				{
					if (chatData.Body is RadioPin || chatData.Body is RadioPinWithText)
					{
						spriteName = "icon_map_pinpoint";
						color = new Color32(239, 180, 79, byte.MaxValue);
					}
					else if (chatData.Body is RadioLink)
					{
						ParamsDictionary dict = ParamsDictionary.MakeParams(((RadioLink)chatData.Body).Link);
						spriteName = dict.Get("icon");
						color = dict.Get("color").ToColor(Color.white);
					}
				}
				_actionButtonIcon.spriteName = spriteName;
				_actionButtonIcon.color = color;
			}
			else
			{
				active = msgType == ChatStruct.ChatMsgType.Dictation;
				_actionButton.gameObject.SetActive(value: false);
				_sttIcon.SetActive(value: false);
				_canTranslateIcon.SetActive(value: false);
				_srcLang.gameObject.SetActive(value: false);
			}
		}
		_sttIcon.SetActive(active);
		OnUpdateButtons();
	}

	protected virtual void OnUpdateButtons()
	{
	}

	protected int GetRightButtonMargin()
	{
		if (_actionButton.gameObject.activeSelf)
		{
			return _actionButton.Widget.width;
		}
		if (_translationButton.gameObject.activeSelf)
		{
			return _translationButton.Widget.width;
		}
		return 0;
	}

	private bool IsAllTranslated()
	{
		return _chatItems.All((ChatStruct x) => !x.Translatable || x.Translated);
	}

	private void SetHeight(int value)
	{
		if (value != _height)
		{
			_height = value;
			Widget.height = value;
			if (HeightChanged != null)
			{
				HeightChanged();
			}
		}
	}

	public int GetHeight()
	{
		return _height;
	}

	protected virtual void SetName(string playerName)
	{
		NameLabel.text = playerName;
		BoxCollider component = NameLabel.GetComponent<BoxCollider>();
		Vector3 size = component.size;
		size.y = NameLabel.height + _verticalPadding;
		size.x = (float)NameLabel.width + _nameColliderWidthPadding;
		component.size = size;
	}

	private void UpdateWidgetHeight()
	{
		int height = ((!TextLabel.gameObject.activeSelf) ? (_eventMessageLabel.height + _verticalPadding) : (TextLabel.height + _verticalPadding));
		SetHeight(height);
		if (_background != null)
		{
			_background.UpdateAnchors();
		}
	}

	public void SetBgColor(Color color)
	{
		if (_background != null)
		{
			_background.color = color;
		}
	}

	public void SetTextColor(Color color)
	{
		_defaultTextColor = color;
	}

	public UILabel GetTextLabel()
	{
		return TextLabel;
	}
}
