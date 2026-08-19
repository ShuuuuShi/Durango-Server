using System;
using System.Collections.Generic;
using ChatData;
using JetBrains.Annotations;
using L10N;
using Player;
using UnityEngine;

public class ChattingRoomTabWidget : MonoBehaviour
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
	private SpriteData _pushEnableSprite;

	[SerializeField]
	private SpriteData _pushDisableSprite;

	[SerializeField]
	private UILabel _newCountLabel;

	[SerializeField]
	private PressColorChange _pressColorChange;

	[SerializeField]
	private PortraitPositions[] _portraitPositions;

	private ulong[] _entityIds;

	private int _loadPlayerIndex;

	private bool _isSelect;

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
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			if (_conversation != value)
			{
				if (_conversation != null)
				{
					EventDelegate.Remove(_conversation.NewChecker.OnChangeList, RefreshConversationNewCount);
				}
				if (value != null)
				{
					EventDelegate.Add(value.NewChecker.OnChangeList, RefreshConversationNewCount);
					_portraitTextures.Clear();
					_portraits.Clear();
					_loadPlayerIndex = 0;
					SpriteData spriteData = ((!value.PushEnabled) ? _pushDisableSprite : _pushEnableSprite);
					spriteData.Set(_pushStateSprite);
					_pressColorChange.SetColor(_pushStateSprite, spriteData.color);
					_entityIds = value.GetEntityIds();
				}
			}
			_conversation = value;
		}
	}

	private void OnDisable()
	{
		Conversation = null;
	}

	public void Set(Conversation conversation)
	{
		bool flag = Conversation != conversation;
		Conversation = conversation;
		Select(_isSelect);
		if (flag)
		{
			if (Conversation.IsEmpty)
			{
				((Component)_portraitWidget).gameObject.SetActive(false);
				_unknownPlayerIcon.SetActive(false);
				((Component)_freqLabel).gameObject.SetActive(false);
				_titleLabel.height = _titleLabel.fontSize + _titleLabel.spacingY;
				_titleLabel.text = T._("빈 그룹");
				_freqLabel.text = string.Empty;
			}
			else if (conversation.IsIndividual)
			{
				((Component)_portraitWidget).gameObject.SetActive(false);
				_unknownPlayerIcon.SetActive(false);
				((Component)_freqLabel).gameObject.SetActive(true);
				_titleLabel.height = _titleLabel.fontSize + _titleLabel.spacingY;
				_titleLabel.text = string.Empty;
				_freqLabel.text = string.Empty;
			}
			else
			{
				((Component)_portraitWidget).gameObject.SetActive(true);
				_unknownPlayerIcon.SetActive(false);
				((Component)_freqLabel).gameObject.SetActive(false);
				_titleLabel.height = (_titleLabel.fontSize + _titleLabel.spacingY) * 2;
				_titleLabel.text = T._("그룹 채팅");
			}
			RequestNextPlayer();
			if (!string.IsNullOrEmpty(conversation.CustomName))
			{
				_titleLabel.text = conversation.CustomName;
			}
		}
		RefreshConversationNewCount();
	}

	private bool RequestNextPlayer()
	{
		ulong num;
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
			num = _entityIds[_loadPlayerIndex++];
		}
		while (num == 0L || num == GameManager.PlayerId);
		KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(num, OnResponsePlayerInfo, useOldCache: true);
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
				((Component)_portraitWidget).gameObject.SetActive(false);
				_unknownPlayerIcon.SetActive(true);
				if (!flag)
				{
					_titleLabel.text = T._("알수없음");
				}
				_freqLabel.text = "????";
			}
			else
			{
				((Component)_portraitWidget).gameObject.SetActive(true);
				_unknownPlayerIcon.SetActive(false);
				if (!flag)
				{
					_titleLabel.text = info.Name;
				}
				_freqLabel.text = $"{info.Freq:0000}";
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

	private void RefreshConversationNewCount()
	{
		if (Conversation != null)
		{
			int count = Conversation.NewChecker.Count;
			((Component)((Component)_newCountLabel).transform.parent).gameObject.SetActive(count > 0);
			_newCountLabel.text = count.ToString();
		}
	}

	public void Select(bool select)
	{
		if (select && _conversation != null)
		{
			_conversation.NewChecker.Count = 0;
			_conversation.ReadAt = KUtility.GetTimestamp();
			GameSystem<SocialSystem>.Instance().SaveConversations();
		}
		_isSelect = select;
		Selected(select);
	}

	private void Selected(bool select)
	{
		_pressColorChange.Press(select);
	}

	[UsedImplicitly]
	private void OnPress(bool press)
	{
		Selected(press || _isSelect);
	}

	private void UpdatePortrait()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		int count = _portraits.Count;
		_portraitTextures.Set(count);
		if (count != 0)
		{
			PortraitPosition[] positions = _portraitPositions[count - 1].Positions;
			Vector3 val = _portraitWidget.localCorners[0];
			Vector2 localSize = _portraitWidget.localSize;
			Vector4 val2 = Vector4.zero;
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
				val2 += Color.op_Implicit(arg.BgColor);
				component.width = (int)(localSize.x * portraitPosition.Size.x);
				component.height = (int)(localSize.y * portraitPosition.Size.y);
				((Component)component).transform.localPosition = val + Vector3.Scale(Vector2.op_Implicit(localSize), Vector2.op_Implicit(portraitPosition.Pivot));
			}
			val2 = ((_portraits.Count <= 0) ? Color.op_Implicit(Color.white) : (val2 / (float)_portraits.Count));
			_portraitBg.color = Color.op_Implicit(val2);
		}
	}
}
