using System.Linq;
using Durango.Logic.Social;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class BottomMenuWidgetBase : MonoBehaviour
{
	protected const float DrawTime = 10f;

	[SerializeField]
	protected CommunicationButtonBase _communicationButton;

	[SerializeField]
	protected EmotionSelector _emotionSelector;

	[SerializeField]
	protected UISprite _emotionNewSprite;

	[SerializeField]
	protected Emoticon _playedLastEmoticon;

	[CanBeNull]
	[SerializeField]
	protected CommunicationButtonBase _quickChatButton;

	[CanBeNull]
	[SerializeField]
	protected QuickChatSelector _quickChatSelector;

	[CanBeNull]
	[SerializeField]
	protected CountableNotificationLabel _chatNewCountLabel;

	public bool IsEmotionSelectorVisible => _emotionSelector.IsVisible;

	public CommunicationButtonBase CommunicationButton => _communicationButton;

	public EmoticonWidget FindEmoticonWidget(string key)
	{
		return _emotionSelector.FindNode(key);
	}

	public virtual void RefreshCommunicationButton()
	{
	}

	protected virtual void Start()
	{
		GameSystem<SocialSystem>.Instance().Emotional.EmoticonNotification.Changed += UpdateEmotionNotifiaction;
		GameSystem<SocialSystem>.Instance().Emotional.MotionNotification.Changed += UpdateEmotionNotifiaction;
		UpdateEmotionNotifiaction();
		_communicationButton.Initailize(OnClickCommunicationButton, OnLongpressComuunicationButton);
		SetEmoticonButton(null, playEmoticon: false);
		_emotionSelector.PenClicked += delegate
		{
			bool isDrawMode = !GameSystem<InputSystem>.Instance().DrawMode;
			EnableDrawMode(isDrawMode);
			_emotionSelector.Hide();
		};
		_emotionSelector.EmoticonClicked += delegate(Emoticon emoticon)
		{
			SetEmoticonButton(emoticon, playEmoticon: false);
		};
		GameSystem<SocialSystem>.Instance().ConversationsNewCount.Changed += ConversationNewCount_Changed;
		ConversationNewCount_Changed();
	}

	private void UpdateEmotionNotifiaction()
	{
		bool hasNewNotifiaction = GameSystem<SocialSystem>.Instance().Emotional.HasNewNotifiaction;
		_emotionNewSprite.gameObject.SetActive(hasNewNotifiaction);
	}

	protected virtual void OnClickCommunicationButton()
	{
		if (GameSystem<InputSystem>.Instance().DrawMode)
		{
			EnableDrawMode(isDrawMode: false);
		}
		else
		{
			_emotionSelector.Show();
		}
	}

	private void OnLongpressComuunicationButton()
	{
		if (GameSystem<InputSystem>.Instance().DrawMode)
		{
			EnableDrawMode(isDrawMode: false);
		}
		else if (_playedLastEmoticon != null)
		{
			GameSystem<SocialSystem>.Instance().PlayEmoticon(_playedLastEmoticon);
		}
		else
		{
			SetEmoticonButton(null, playEmoticon: true);
		}
	}

	private void SetEmoticonButton([CanBeNull] Emoticon data, bool playEmoticon)
	{
		Emoticon emoticon = data;
		if (emoticon == null)
		{
			emoticon = GameSystem<SocialSystem>.Instance().Emotional.Emoticons.FirstOrDefault((Emoticon elem) => elem.Available);
			if (emoticon == null)
			{
				return;
			}
		}
		_playedLastEmoticon = emoticon;
		if (playEmoticon)
		{
			GameSystem<SocialSystem>.Instance().PlayEmoticon(_playedLastEmoticon);
		}
		_communicationButton.Set(_playedLastEmoticon.UIIcon);
	}

	public void SetCommunicationButtonActive(bool active)
	{
		_communicationButton.gameObject.SetActive(active);
	}

	protected void EnableDrawMode(bool isDrawMode)
	{
		GameSystem<InputSystem>.Instance().DrawMode = isDrawMode;
		if (isDrawMode)
		{
			_communicationButton.Set("button_hud_pen");
			_communicationButton.StartFillAmount(10f, () => GameSystem<InputSystem>.Instance().DrawMode, delegate
			{
				EnableDrawMode(isDrawMode: false);
			});
			string title = T._("그림채팅 모드");
			string comment = T._("그림을 그리면 주위 사람들이 볼 수 있습니다.");
			SetButtonComment(_communicationButton, title, comment, 10f);
		}
		else
		{
			SetEmoticonButton(null, playEmoticon: false);
		}
	}

	protected virtual void OnClickQuickChat(string chat)
	{
		Point2 humanePosition = Singleton<MapContext>.Instance().HumanePosition;
		string message = T._(LocalizeSystem.Get(chat), MapPositionParser.ToString(humanePosition));
		GameSystem<SocialSystem>.Instance().QuickSay(message);
	}

	private void ConversationNewCount_Changed()
	{
		if (!(_chatNewCountLabel == null))
		{
			int count = GameSystem<SocialSystem>.Instance().ConversationsNewCount.Count;
			_chatNewCountLabel.Set(count);
		}
	}

	private static void SetButtonComment(UIWidget buttonWidget, string title, string comment, float visibleTime)
	{
		if (!string.IsNullOrEmpty(comment) || !string.IsNullOrEmpty(title))
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Set(title, comment);
			widgetTooltipControl.Show(buttonWidget.transform, Vector2.up * 20f, visibleTime);
			Vector3 position = buttonWidget.localCorners[1];
			position = buttonWidget.transform.TransformPoint(position);
			position = widgetTooltipControl.transform.parent.InverseTransformPoint(position);
			position.y += (float)widgetTooltipControl.Widget.height + 10f;
			widgetTooltipControl.Widget.SetPosition(position, 0f, 1f);
		}
	}
}
