using System;
using System.Collections.Generic;
using Durango.Logic.Social;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class ChattingRoomTabWidget : SelectableWidget
{
	[Serializable]
	private struct PortraitPositions
	{
		public PortraitPosition[] Positions;
	}

	[Serializable]
	private struct PortraitPosition
	{
		public Texture Mask;

		public Vector2 MaskTiling;

		public Vector2 MaskOffset;

		public Vector2 Pivot;

		public Vector2 Size;
	}

	[SerializeField]
	private UIWidget _portraitWidget;

	[SerializeField]
	private ListObjectPool _portraitTextures;

	[SerializeField]
	private UISprite _portraitBg;

	[SerializeField]
	private GameObject _unknownPlayerIcon;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _freqLabel;

	[SerializeField]
	private UISprite _pushStateSprite;

	[SerializeField]
	private UISprite _hideStateSprite;

	[SerializeField]
	private CountableNotificationLabel _notification;

	[SerializeField]
	private PortraitPositions[] _portraitPositions;

	private string[] _entityIds;

	private int _loadPlayerIndex;

	private Conversation _conversation;

	private readonly List<PortraitBuilder.Argument> _portraits = new List<PortraitBuilder.Argument>();

	public int MaxPlayerPortraitCount => _portraitPositions.Length;

	public Conversation Conversation
	{
		get
		{
			return _conversation;
		}
		private set
		{
			if (_conversation != value)
			{
				if (_conversation != null)
				{
					_conversation.Notification.Changed -= UpdateNotification;
				}
				if (value != null)
				{
					value.Notification.Changed += UpdateNotification;
					_portraitTextures.Clear();
					_portraits.Clear();
					_loadPlayerIndex = 0;
					bool flag = !value.PushEnabled;
					bool active = GameSystem<SocialSystem>.Instance().ChannelInfo.IsHidden(value);
					_pushStateSprite.gameObject.SetActive(flag);
					_hideStateSprite.gameObject.SetActive(active);
					Vector3 localPosition = _pushStateSprite.transform.localPosition;
					if (flag)
					{
						localPosition -= new Vector3(_hideStateSprite.width + 2, 0f);
					}
					_hideStateSprite.transform.localPosition = localPosition;
					_entityIds = value.GetEntityIds();
				}
			}
			_conversation = value;
		}
	}

	protected override void OnInit()
	{
		ClickSound = UISound.ClickType.ButtonMedium;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Conversation = null;
	}

	public void Set(Conversation conversation)
	{
		bool flag = Conversation != conversation;
		Conversation = conversation;
		if (flag)
		{
			if (Conversation.IsEmpty)
			{
				_portraitWidget.gameObject.SetActive(value: false);
				_unknownPlayerIcon.SetActive(value: false);
				_freqLabel.gameObject.SetActive(value: false);
				_titleLabel.height = _titleLabel.fontSize;
				_titleLabel.text = T._("빈 그룹");
				_freqLabel.text = string.Empty;
			}
			else if (conversation.IsIndividual)
			{
				_portraitWidget.gameObject.SetActive(value: false);
				_unknownPlayerIcon.SetActive(value: false);
				_freqLabel.gameObject.SetActive(value: true);
				_titleLabel.height = _titleLabel.fontSize;
				_titleLabel.text = string.Empty;
				_freqLabel.text = string.Empty;
			}
			else
			{
				_portraitWidget.gameObject.SetActive(value: true);
				_unknownPlayerIcon.SetActive(value: false);
				_freqLabel.gameObject.SetActive(value: false);
				_titleLabel.height = _titleLabel.fontSize * 2 + _titleLabel.spacingY;
				_titleLabel.text = T._("그룹 채팅");
			}
			RequestNextPlayer();
			if (!string.IsNullOrEmpty(conversation.CustomName))
			{
				_titleLabel.text = conversation.CustomName;
			}
		}
		UpdateNotification();
	}

	private bool RequestNextPlayer()
	{
		string text;
		do
		{
			if (_portraits.Count >= MaxPlayerPortraitCount)
			{
				return false;
			}
			if (_conversation == null || _loadPlayerIndex >= _entityIds.Length)
			{
				return false;
			}
			text = _entityIds[_loadPlayerIndex++];
		}
		while (string.IsNullOrEmpty(text) || text == GameManager.PlayerId);
		Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(text, OnResponsePlayerInfo);
		return true;
	}

	private void OnResponsePlayerInfo(PlayerInfo info)
	{
		if (_portraits.Count >= MaxPlayerPortraitCount)
		{
			return;
		}
		if (_portraits.Count == 0 && _conversation != null && _conversation.IsIndividual)
		{
			bool flag = !string.IsNullOrEmpty(_conversation.CustomName);
			if (!info.Valid)
			{
				_portraitWidget.gameObject.SetActive(value: false);
				_unknownPlayerIcon.SetActive(value: true);
				if (!flag)
				{
					_titleLabel.text = T._("알수없음");
				}
				_freqLabel.text = "????";
			}
			else
			{
				_portraitWidget.gameObject.SetActive(value: true);
				_unknownPlayerIcon.SetActive(value: false);
				if (!flag)
				{
					_titleLabel.text = info.Name;
				}
				_freqLabel.text = $"#{info.Freq:0000} [size=18]kHz[/size]";
			}
		}
		if (info.Valid)
		{
			_portraits.Add(info.GetPortraitArgument());
		}
		if (_conversation == null || !_conversation.IsGroup || !RequestNextPlayer())
		{
			UpdatePortrait();
		}
	}

	private void UpdateNotification()
	{
		if (Conversation != null)
		{
			int count = Conversation.Notification.Count;
			_notification.Set(count);
		}
	}

	private void UpdatePortrait()
	{
		int count = _portraits.Count;
		_portraitTextures.Set(count);
		if (count != 0)
		{
			PortraitPosition[] positions = _portraitPositions[count - 1].Positions;
			Vector3 vector = _portraitWidget.localCorners[0];
			Vector2 localSize = _portraitWidget.localSize;
			Vector4 vector2 = Vector4.zero;
			int i = 0;
			for (int count2 = _portraits.Count; i < count2; i++)
			{
				PortraitBuilder.Argument arg = _portraits[i];
				PortraitPosition portraitPosition = positions[i];
				UITexture component = _portraitTextures[i].GetComponent<UITexture>();
				arg.Mask = portraitPosition.Mask;
				arg.MaskScale = portraitPosition.MaskTiling;
				arg.MaskOffset = portraitPosition.MaskOffset;
				PortraitBuilder.Set(arg, component);
				vector2 += (Vector4)arg.BgColor;
				component.width = (int)(localSize.x * portraitPosition.Size.x);
				component.height = (int)(localSize.y * portraitPosition.Size.y);
				component.transform.localPosition = vector + Vector3.Scale(localSize, portraitPosition.Pivot);
			}
			if (_portraits.Count > 0)
			{
				vector2 /= (float)_portraits.Count;
			}
			else
			{
				vector2 = Color.white;
			}
			_portraitBg.color = vector2;
		}
	}
}
