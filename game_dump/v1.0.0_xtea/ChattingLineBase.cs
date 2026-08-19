using System;
using System.Collections.Generic;
using System.Text;
using ChatData;
using L10N;
using Messages;
using Player;
using Shared.Chat;
using UnityEngine;

public class ChattingLineBase : MonoBehaviour
{
	public Action<ulong> NameLabelClicked;

	public Action<ChatStruct> SharedPointButtonClicked;

	public Action HeightChanged;

	[SerializeField]
	private UISpriteLabel _nameLabel;

	[SerializeField]
	private UISpriteLabel _textLabel;

	[SerializeField]
	private UISpriteLabel _eventMessageLabel;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private GameObject _sttIcon;

	[SerializeField]
	private GameObject _buttonSharedPoint;

	[SerializeField]
	private int _verticalPadding;

	[SerializeField]
	private int _textLeftMargin;

	[SerializeField]
	private int _textRightMargin;

	private Color _defaultTextColor = Color.white;

	private ChatStruct _chatData;

	private UIWidget _widget;

	private int _height;

	public ulong EntityId => _chatData.EntityId;

	public ChannelType Type => _chatData.Type;

	public ChatStruct.ChatMsgType MsgType => _chatData.MsgType;

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

	public Vector3 Position
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.localPosition;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((Component)this).transform.localPosition = value;
		}
	}

	private void Start()
	{
		UIEventListener.Get(((Component)_nameLabel).gameObject).onClick = OnClickNameLabel;
		UIEventListener.Get(_buttonSharedPoint).onClick = OnClickPin;
	}

	private void OnClickNameLabel(GameObject obj)
	{
		if (NameLabelClicked != null)
		{
			NameLabelClicked(EntityId);
		}
	}

	private void OnClickPin(GameObject obj)
	{
		if (SharedPointButtonClicked != null)
		{
			SharedPointButtonClicked(_chatData);
		}
	}

	public void SetActive(bool active)
	{
		((Component)this).gameObject.SetActive(active);
	}

	private void SetEventState(bool active)
	{
		((Component)_nameLabel).gameObject.SetActive(!active);
		((Component)_textLabel).gameObject.SetActive(!active);
		((Component)_eventMessageLabel).gameObject.SetActive(active);
		_sttIcon.SetActive(false);
		_buttonSharedPoint.SetActive(false);
	}

	private void SetEventMessage(ChatStruct chat, IList<Player.PlayerInfo> playerInfos)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < playerInfos.Count; i++)
		{
			if (i != 0)
			{
				stringBuilder.Append(", ");
			}
			Player.PlayerInfo playerInfo = playerInfos[i];
			stringBuilder.Append(playerInfo.Valid ? playerInfo.Name : T._("알수없음"));
		}
		string text = $"[a9a278]{stringBuilder}[-]";
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
			string text2 = LocalizeSystem.Format(chat.Name, text);
			if (!string.IsNullOrEmpty(text2))
			{
				arg = text2;
			}
		}
		string text3 = $"[7e7e64][icon_person] {arg}[-]";
		_eventMessageLabel.text = text3;
		UpdateWidgetHeight();
	}

	private bool IsEventMessage(ChatStruct data)
	{
		return data.Body is RadioEntered || data.Body is RadioLeft;
	}

	public void SetText(ChatStruct chat)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		_chatData = chat;
		if (IsEventMessage(chat))
		{
			SetEventState(active: true);
			SetBgColor(new Color(0.19f, 0.19f, 0.15f));
			ulong[] entityIds = null;
			if (chat.Body is RadioEntered)
			{
				entityIds = ((RadioEntered)chat.Body).EntityIds;
			}
			else if (chat.Body is RadioLeft)
			{
				entityIds = ((RadioLeft)chat.Body).EntityIds;
			}
			KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfos(entityIds, delegate(Player.PlayerInfo[] infos)
			{
				SetEventMessage(chat, infos);
			}, useOldCache: true);
			return;
		}
		SetEventState(active: false);
		_textLabel.text = $"{chat.FindText()} [c][888888][size=16]{TimerSystem.Timeago(chat.Time)}[/size][-][/c]";
		if (chat.Type == ChannelType.System)
		{
			SetName(chat.Name);
		}
		else
		{
			Widget.alpha = 0f;
			KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(chat.EntityId, ResponsePlayerInfo, useOldCache: true);
		}
		_nameLabel.Label.color = chat.GetNameColor();
		_textLabel.Label.color = chat.GetMsgColor(_defaultTextColor);
		_sttIcon.SetActive(chat.Body is RadioDictation);
		_buttonSharedPoint.SetActive(chat.Body is RadioPin);
		UpdateWidgetHeight();
	}

	public void AppendText(ChatStruct chat)
	{
		_textLabel.text = $"{_textLabel.text}\n{chat.FindText()} [c][888888][size=16]{TimerSystem.Timeago(chat.Time)}[/size][-][/c]";
		UpdateWidgetHeight();
	}

	private void SetHeight(int value)
	{
		if (value != _height)
		{
			_height = value;
			Widget.height = value;
			_background.UpdateAnchors();
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

	private void ResponsePlayerInfo(Player.PlayerInfo info)
	{
		if (info.Valid)
		{
			if (EntityId != info.EntityId)
			{
				return;
			}
			SetName(info.Name);
		}
		else
		{
			SetName(T._("(알 수 없는 사람)"));
		}
		UpdateWidgetHeight();
		Widget.alpha = 1f;
	}

	private void SetName(string playerName)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		_nameLabel.text = playerName;
		Vector3 localPosition = ((Component)_nameLabel).transform.localPosition;
		if (!string.IsNullOrEmpty(playerName))
		{
			localPosition = ((Component)_nameLabel).transform.localPosition + Vector3.right * (float)(_nameLabel.Label.width + _textLeftMargin);
		}
		((Component)_textLabel).transform.localPosition = localPosition;
		_textLabel.Label.width = (int)((float)Widget.width - localPosition.x - (float)_textRightMargin);
		_textLabel.Label.ProcessText();
	}

	private void UpdateWidgetHeight()
	{
		int height = ((!((Component)_textLabel).gameObject.activeSelf) ? (_eventMessageLabel.Label.height - _eventMessageLabel.Label.spacingY + _verticalPadding * 2) : (_textLabel.Label.height - _textLabel.Label.spacingY + _verticalPadding * 2));
		SetHeight(height);
	}

	public void SetBgColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_background.color = color;
	}

	public void SetTextColor(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		_defaultTextColor = color;
	}
}
