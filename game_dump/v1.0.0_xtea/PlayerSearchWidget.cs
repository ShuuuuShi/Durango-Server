using System;
using Player;
using UnityEngine;

public class PlayerSearchWidget : MonoBehaviour
{
	private Action<PlayerInfo> _playerSelected;

	[SerializeField]
	private UIInput _input;

	[SerializeField]
	private DefaultSelectableButton _confirmBtn;

	[SerializeField]
	private UISpriteLabel _commentLabel;

	[SerializeField]
	private PlayerSearchTextInput _playerSearchInput;

	[SerializeField]
	private UIWidget _textInputWidget;

	private AnimationWidget _animWidget;

	private Vector3 _basePosition;

	private string _comment = string.Empty;

	private string _defaultValue = string.Empty;

	private AnimationWidget AnimWidget
	{
		get
		{
			if ((Object)(object)_animWidget == (Object)null)
			{
				_animWidget = ((Component)this).GetComponent<AnimationWidget>();
			}
			return _animWidget;
		}
	}

	public void Init()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private void Start()
	{
		EventDelegate.Add(_input.onSubmit, OnSubmit);
		_confirmBtn.Clicked = OnSubmit;
	}

	private void Update()
	{
		if (Input.GetKey((KeyCode)27))
		{
			Hide();
		}
	}

	private void OnEnable()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		_basePosition = (float)UIManager.ScreenHeight / 2f * Vector3.up;
		_input.value = _defaultValue;
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnTouch));
		UIManager.KeyboardHeightUpdated += OnVisibleKeyboard;
	}

	private void OnDisable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnTouch));
		UIManager.KeyboardHeightUpdated -= OnVisibleKeyboard;
	}

	private void OnVisibleKeyboard(int height)
	{
		if (height <= 0)
		{
			OnSubmit();
		}
	}

	private void OnSubmit()
	{
		if (_playerSearchInput.IsCheckPlayer())
		{
			PlayerInfo player = _playerSearchInput.Player;
			if (player != null && player.Valid)
			{
				KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(player.EntityId, ResponseSelectedPlayerInfo);
				Hide();
			}
		}
		else
		{
			_playerSearchInput.CheckPlayer();
		}
	}

	private void ResponseSelectedPlayerInfo(PlayerInfo playerInfo)
	{
		if (_playerSelected != null)
		{
			_playerSelected(playerInfo);
		}
	}

	public void Show(Action<PlayerInfo> selected, string comment = null, string defaultValue = null)
	{
		_playerSelected = selected;
		_comment = ((comment == null) ? string.Empty : comment);
		_defaultValue = ((defaultValue == null) ? string.Empty : defaultValue);
		((Behaviour)_playerSearchInput).enabled = true;
		Show();
	}

	private void Show()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		bool flag = !((Component)this).gameObject.activeSelf;
		Vector3 localPosition = ((Component)_textInputWidget).transform.localPosition;
		if (string.IsNullOrEmpty(_comment))
		{
			((Component)_commentLabel).gameObject.SetActive(false);
			localPosition.y = 0f;
		}
		else
		{
			((Component)_commentLabel).gameObject.SetActive(true);
			_commentLabel.text = _comment;
			float y = _commentLabel.Label.printedSize.y;
			localPosition.y = -((int)y + 10);
		}
		((Component)_textInputWidget).transform.localPosition = localPosition;
		if (flag)
		{
			((Component)this).gameObject.SetActive(true);
			AnimWidget.Alpha = 1f;
			AnimWidget.SetPosition(_basePosition + Vector3.up * (0f - localPosition.y + (float)_textInputWidget.height), useTween: false);
			AnimWidget.SetPosition(_basePosition);
		}
		else
		{
			((Component)this).BroadcastMessage("OnEnable");
		}
		_input.isSelected = true;
	}

	public void Hide()
	{
		AnimWidget.Alpha = 0f;
	}

	private void OnTouch(GameObject touchObj, bool press)
	{
		if ((Object)(object)this == (Object)null || !press || !((Component)this).gameObject.activeSelf)
		{
			return;
		}
		Transform transform = ((Component)this).transform;
		Transform val = ((!((Object)(object)touchObj == (Object)null)) ? touchObj.transform : null);
		bool flag = false;
		while ((Object)(object)val != (Object)null)
		{
			if ((Object)(object)val == (Object)(object)transform)
			{
				flag = true;
				break;
			}
			val = val.parent;
		}
		if (!flag && (Object)(object)_playerSearchInput.PlayerSelector == (Object)null)
		{
			Hide();
		}
	}
}
