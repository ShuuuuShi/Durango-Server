using System.Collections.Generic;
using Durango.Logic.Social;
using Durango.Player;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Shared.Chat;
using UnityEngine;

namespace Durango.UI;

public class ChattingChannelOption : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private UISprite _emptyBar;

	[SerializeField]
	private UISprite _optionBar;

	[SerializeField]
	private ChatRoomOption _chatRoomOption;

	[SerializeField]
	private UIWidget _chatLineList;

	[SerializeField]
	private UILabel _subscriptionCount;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _freqLabel;

	[SerializeField]
	private UISprite _hideButton;

	[SerializeField]
	private UISprite _optionButton;

	[SerializeField]
	private SpriteData _chatUnhideSprite;

	[SerializeField]
	private SpriteData _chatHideSprite;

	[SerializeField]
	private SpriteData _pushEnableSprite;

	[SerializeField]
	private SpriteData _pushDisableSprite;

	[SerializeField]
	[Tooltip("버튼의 우측으로부터의 정렬 순서")]
	private UISprite[] _buttonOrderFromRight;

	[SerializeField]
	[Tooltip("옵션 버튼 간 간격")]
	private int _buttonSpacing;

	[SerializeField]
	[Tooltip("숨김 툴팁 보이기까지 호버링 후 대기시간")]
	private float _showHideTooltipThreshold;

	private IList<KeyValuePair<ChatFilterType, uint>> _mainChannelInfos;

	private float _hideButtonHoveredTime;

	private bool _showHideTooltip;

	public ChatRoomOption ChatRoomOptionBox => _chatRoomOption;

	public Conversation CurrentConv { get; private set; }

	public ChatFilterType FilterType { get; private set; }

	void IUIInitializable.Init()
	{
		SelectableWidget component = _hideButton.GetComponent<SelectableWidget>();
		component.Clicked = OnClickHide;
		component.OnHovered = OnHoverHide;
		component = _optionButton.GetComponent<SelectableWidget>();
		component.Clicked = OnClickOption;
		RefreshBar();
	}

	public void Set(IList<KeyValuePair<ChatFilterType, uint>> mainChannels)
	{
		_mainChannelInfos = mainChannels;
	}

	public void Select(ChatFilterType filterType)
	{
		if (FilterType != filterType || CurrentConv != null)
		{
			FilterType = filterType;
			CurrentConv = null;
			RefreshBar();
		}
	}

	public void Select(Conversation conversation)
	{
		if (CurrentConv != conversation)
		{
			CurrentConv = conversation;
			RefreshBar();
		}
	}

	public void HidePopup()
	{
		if (_chatRoomOption.IsVisible)
		{
			_chatRoomOption.Hide();
		}
	}

	private void OnClickHide()
	{
		((!((CurrentConv == null) ? GameSystem<SocialSystem>.Instance().ChannelInfo.ToggleHide(FilterType) : GameSystem<SocialSystem>.Instance().ChannelInfo.ToggleHide(CurrentConv))) ? _chatUnhideSprite : _chatHideSprite).Set(_hideButton);
		UIManager.Popup.Tooltip<WidgetTooltipControl>().Hide();
	}

	private void OnClickOption()
	{
		if (_chatRoomOption.IsVisible)
		{
			_chatRoomOption.Hide();
		}
		else if (CurrentConv != null)
		{
			_chatRoomOption.Set(CurrentConv, (int)GetComponent<UIWidget>().parent.GetHeight());
			_chatRoomOption.Show();
		}
	}

	private void OnHoverHide(bool isHover)
	{
		if (isHover)
		{
			_hideButtonHoveredTime = Time.time;
			_showHideTooltip = true;
		}
		else
		{
			_showHideTooltip = false;
			UIManager.Popup.Tooltip<WidgetTooltipControl>().Hide();
		}
	}

	private void RefreshBar()
	{
		bool flag = CurrentConv == null;
		bool flag2 = flag && (FilterType == ChatFilterType.All || FilterType == ChatFilterType.System);
		_optionBar.gameObject.SetActive(!flag2);
		if (flag2)
		{
			Reposition();
			return;
		}
		bool flag3 = !flag && CurrentConv.IsIndividual;
		if (flag3)
		{
			_subscriptionCount.gameObject.SetActive(value: false);
		}
		else
		{
			string text;
			if (flag)
			{
				uint num = 0u;
				foreach (KeyValuePair<ChatFilterType, uint> mainChannelInfo in _mainChannelInfos)
				{
					if (mainChannelInfo.Key == FilterType)
					{
						num = mainChannelInfo.Value;
						break;
					}
				}
				text = ((num != 0) ? $"[icon=icon_chat_member_pc:1.46] {num}" : string.Empty);
			}
			else
			{
				int entityCount = CurrentConv.EntityCount;
				text = ((entityCount != 0) ? T._("그룹 채팅 {0}명", entityCount) : string.Empty);
			}
			_subscriptionCount.text = text;
			_subscriptionCount.gameObject.SetActive(value: true);
		}
		if (flag3)
		{
			ChattingGroup_PC.RequestPartnerName(CurrentConv, OnResponsePartnerInfo);
		}
		else
		{
			_nameLabel.gameObject.SetActive(value: false);
			_freqLabel.gameObject.SetActive(value: false);
		}
		_optionButton.gameObject.SetActive(!flag);
		ChannelType channelType = SocialSystem.ConvertToChannelType(FilterType);
		if (flag && !ChatChannelInfo.IsHideable(channelType))
		{
			_hideButton.gameObject.SetActive(value: false);
		}
		else
		{
			((!((!flag) ? GameSystem<SocialSystem>.Instance().ChannelInfo.IsHidden(CurrentConv) : GameSystem<SocialSystem>.Instance().ChannelInfo.IsHidden(channelType))) ? _chatUnhideSprite : _chatHideSprite).Set(_hideButton);
			_hideButton.gameObject.SetActive(value: true);
		}
		Reposition();
	}

	public void Reposition()
	{
		int num = _buttonSpacing;
		for (int i = 0; i < _buttonOrderFromRight.Length; i++)
		{
			UISprite uISprite = _buttonOrderFromRight[i];
			if (uISprite.gameObject.activeInHierarchy)
			{
				int width = uISprite.width;
				uISprite.leftAnchor.absolute = -(num + width);
				uISprite.rightAnchor.absolute = -num;
				uISprite.UpdateAnchors();
				num += width + _buttonSpacing;
			}
		}
		UIUtility.UpdateAnchors(base.transform);
		if (_optionBar.gameObject.activeInHierarchy)
		{
			_chatLineList.topAnchor.absolute = _optionBar.bottomAnchor.absolute;
		}
		else
		{
			_chatLineList.topAnchor.absolute = _emptyBar.bottomAnchor.absolute;
		}
		_chatLineList.UpdateAnchors();
	}

	private void OnResponsePartnerInfo(PlayerInfo info)
	{
		if (CurrentConv != null && CurrentConv.IsIndividual)
		{
			_nameLabel.gameObject.SetActive(value: true);
			_freqLabel.gameObject.SetActive(value: true);
			if (info.Valid)
			{
				_nameLabel.text = info.Name;
				_freqLabel.text = $"#{info.Freq:0000} kHz";
			}
			else
			{
				_nameLabel.text = T._("알수없음");
				_freqLabel.text = "#???? kHz";
			}
			UIUtility.UpdateAnchors(_freqLabel.transform);
		}
	}

	private void LateUpdate()
	{
		if (_showHideTooltip && Time.time > _hideButtonHoveredTime + _showHideTooltipThreshold)
		{
			string text = ((!((CurrentConv != null) ? GameSystem<SocialSystem>.Instance().ChannelInfo.IsHidden(CurrentConv) : GameSystem<SocialSystem>.Instance().ChannelInfo.IsHidden(FilterType))) ? T._("채널의 대화 내용을 전체 채팅에 표시합니다.") : T._("채널의 대화 내용을 전체 채팅에 표시하지 않습니다."));
			ChattingGroupBase.ShowToggleButtonTooltip(text, _hideButton, Vector3.zero);
			_showHideTooltip = false;
		}
	}
}
