using System.Collections.Generic;
using ChatData;
using L10N;
using UnityEngine;

public class BottomMenuWidget : MonoBehaviour
{
	private const float DrawTime = 10f;

	[SerializeField]
	private CommunicationButton _communicationButton;

	[SerializeField]
	private CommunicationButton _quickChatButton;

	[SerializeField]
	private CommunicationButton _keyboardButton;

	[SerializeField]
	private GameObject _speakButton;

	[SerializeField]
	private CommunicationList _communicationList;

	[SerializeField]
	private QuickChatList _quickChatList;

	[SerializeField]
	private UILabel _chatNewCountLabel;

	[SerializeField]
	private UISprite _rightLine;

	[SerializeField]
	private UIInput _hiddenInput;

	private float _quickChatEnableAt;

	private Vector3 _basePosition;

	private void Start()
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		_communicationButton.Initailize(OnSelectCommunication, _communicationList.Show);
		_quickChatButton.Initailize(delegate
		{
			UIManager.Open<ChattingGroup>();
		}, _quickChatList.Show);
		_keyboardButton.Initailize(OnClickKeyboardButton, null);
		_communicationList.CommunicationSelected += OnSelectCommunication;
		_quickChatList.QuickChatClicked += OnClickQuickChat;
		EventDelegate.Set(_hiddenInput.onSubmit, OnSubmitHiddenKeyboard);
		if (!_speakButton.activeSelf)
		{
			Vector3 localPosition = ((Component)_rightLine).transform.localPosition;
			localPosition.x -= 80f;
			((Component)_rightLine).transform.localPosition = localPosition;
		}
		EventDelegate.Add(GameSystem<SocialSystem>.Instance().ConversationsNewCount.OnChangeList, ConversationNewCountUpdate);
		GameSystem<CombatSystem>.Instance().ChangedCombatMode += OnChangeCombatMode;
		UpdateCommunicationIcon();
		ConversationNewCountUpdate();
	}

	private void OnPortraitMode(bool isPortrait)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_basePosition = Vector3.zero;
	}

	private void OnChangeCombatMode(bool isCombat)
	{
		MoveToCombatModePosition(isCombat, instant: false);
	}

	private void MoveToCombatModePosition(bool isCombat, bool instant)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		if (_basePosition == Vector3.zero)
		{
			_basePosition = ((Component)this).transform.localPosition;
		}
		Vector3 val = _basePosition;
		if (isCombat)
		{
			val += Vector3.left * 100f;
		}
		if (instant)
		{
			((Component)this).transform.localPosition = val;
		}
		else
		{
			TweenPosition.Begin(((Component)this).gameObject, 0.3f, val);
		}
	}

	private void ConversationNewCountUpdate()
	{
		int num = 0;
		Dictionary<ulong, Conversation> conversations = GameSystem<SocialSystem>.Instance().Conversations;
		Dictionary<ulong, Conversation>.Enumerator enumerator = conversations.GetEnumerator();
		while (enumerator.MoveNext())
		{
			num += enumerator.Current.Value.NewChecker.Count;
		}
		((Component)((Component)_chatNewCountLabel).transform.parent).gameObject.SetActive(num > 0);
		_chatNewCountLabel.text = num.ToString();
	}

	private void EnableDrawMode(bool isDrawMode)
	{
		KSingleton<PlayerController>.Instance().DrawMode = isDrawMode;
		if (isDrawMode)
		{
			_communicationButton.StartFillAmount(10f, () => KSingleton<PlayerController>.Instance().DrawMode, delegate
			{
				KSingleton<PlayerController>.Instance().DrawMode = false;
			});
			string title = T._("그림채팅 모드");
			string comment = T._("그림을 그리면 주위 사람들이 볼 수 있습니다.");
			SetButtonComment(((Component)_communicationButton).gameObject, title, comment, 10f);
		}
	}

	private void OnSelectCommunication()
	{
		int selectedEmotion = _communicationList.SelectedEmotion;
		switch (selectedEmotion)
		{
		case -1:
			if (!KSingleton<PlayerController>.Instance().IsInServerSideBattle && !PlayerBehavior.LocalPlayer.IsMoving)
			{
				KeyValuePair<string, string[]> selectedMotion = _communicationList.SelectedMotion;
				string[] value = selectedMotion.Value;
				if (value != null && value.Length > 0)
				{
					int num = Random.Range(0, value.Length);
					string motionState = value[num];
					KSingleton<PlayerController>.Instance().Motion(motionState, 0f, 1f, forceTransition: false, string.Empty);
					EnableDrawMode(isDrawMode: false);
				}
			}
			break;
		case 0:
		{
			bool isDrawMode = !KSingleton<PlayerController>.Instance().DrawMode;
			EnableDrawMode(isDrawMode);
			break;
		}
		default:
			if (CheckQuickChatEnabled())
			{
				KSingleton<UIManager>.Instance().Emoticon((uint)(selectedEmotion - 1), 1f);
				EnableDrawMode(isDrawMode: false);
			}
			break;
		}
		UpdateCommunicationIcon();
		_communicationList.Hide();
	}

	private void UpdateCommunicationIcon()
	{
		_communicationList.Init();
		int selectedEmotion = _communicationList.SelectedEmotion;
		string spriteName;
		switch (selectedEmotion)
		{
		case -1:
		{
			KeyValuePair<string, string[]> selectedMotion = _communicationList.SelectedMotion;
			spriteName = IconMap.Get($"#emotion_{selectedMotion.Key}");
			break;
		}
		case 0:
			spriteName = "button_hud_pen";
			break;
		default:
			spriteName = $"icon_emoticon_{selectedEmotion}";
			break;
		}
		_communicationButton.Set(spriteName);
	}

	private void OnClickQuickChat(string chat)
	{
		if (CheckQuickChatEnabled())
		{
			Point2 humanePosition = KSingleton<MapContext>.Instance().HumanePosition;
			string message = LocalizeSystem.Format(chat, MapPositionParser.ToString(humanePosition));
			GameSystem<SocialSystem>.Instance().Say(message);
		}
	}

	private bool CheckQuickChatEnabled()
	{
		float time = Time.time;
		if (time < _quickChatEnableAt)
		{
			return false;
		}
		_quickChatEnableAt = Time.time + 1f;
		return true;
	}

	private void OnClickKeyboardButton()
	{
		_hiddenInput.isSelected = true;
	}

	private void OnSubmitHiddenKeyboard()
	{
		string value = _hiddenInput.value;
		_hiddenInput.value = string.Empty;
		_hiddenInput.isSelected = false;
		if (!string.IsNullOrEmpty(value))
		{
			GameSystem<SocialSystem>.Instance().Say(value);
		}
	}

	private static void SetButtonComment(GameObject obj, string title, string comment, float visibleTime)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(comment) || !string.IsNullOrEmpty(title))
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Set(title, comment);
			widgetTooltipControl.Show(obj.transform, Vector2.up * 20f, visibleTime);
		}
	}
}
