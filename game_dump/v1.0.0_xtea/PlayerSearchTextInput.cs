using System;
using System.Collections.Generic;
using L10N;
using Player;
using UnityEngine;

[RequireComponent(typeof(UIInput))]
public class PlayerSearchTextInput : MonoBehaviour
{
	public Action<PlayerInfo> SelectPlayerChanged;

	[SerializeField]
	private UIInput _input;

	[SerializeField]
	private UISprite _validSprite;

	private bool _hasFocus;

	private float _searchLockTime;

	private PlayerSelectControl _playerSelector;

	public UIInput Input => _input;

	public PlayerInfo Player { get; private set; }

	public PlayerSelectControl PlayerSelector
	{
		get
		{
			return _playerSelector;
		}
		private set
		{
			_playerSelector = value;
		}
	}

	private void Awake()
	{
		((Component)_validSprite).gameObject.SetActive(false);
		EventDelegate.Set(_input.onChange, InputValueChanged);
		UIEventListener.Get(((Component)_input).gameObject).onSelect = OnSelectInputLabel;
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		((Component)_validSprite).gameObject.SetActive(false);
	}

	private void OnSelectInputLabel(GameObject go, bool select)
	{
		if (select)
		{
			_hasFocus = true;
		}
		else if (_hasFocus)
		{
			CheckPlayer();
		}
	}

	private void InputValueChanged()
	{
		PlayerSelect(null, refreshValidSprite: false);
	}

	public bool IsCheckPlayer()
	{
		return ((Component)_validSprite).gameObject.activeSelf;
	}

	public void CheckPlayer()
	{
		_hasFocus = false;
		float time = Time.time;
		if (!(time < _searchLockTime))
		{
			_searchLockTime = time + 0.5f;
			string text = _input.value.Trim();
			if (!string.IsNullOrEmpty(text))
			{
				_input.value = text;
				KSingleton<PlayerInfoManager>.Instance().SearchPlayerInfos(text, ResponsePlayerInfos);
			}
		}
	}

	private void ResponsePlayerInfos(PlayerInfo[] playerInfos)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		PlayerInfo player = null;
		ulong playerId = GameManager.PlayerId;
		int num = ((playerInfos != null) ? playerInfos.Length : 0);
		if (num > 0)
		{
			List<PlayerInfo> list = new List<PlayerInfo>(playerInfos);
			for (int num2 = num - 1; num2 >= 0; num2--)
			{
				if (list[num2].EntityId == playerId)
				{
					list.RemoveAt(num2);
				}
			}
			if (list.Count > 0)
			{
				player = list[0];
				PlayerSelectControl playerSelectControl = UIManager.Popup.Tooltip<PlayerSelectControl>();
				playerSelectControl.Set(list, OnSelectPlayer);
				UILabel label = _input.label;
				Vector3 val = Vector3.left * (float)label.width * 0.5f;
				playerSelectControl.Show((UIWidget)_input.label, Vector2.op_Implicit(val), 3600f);
				playerSelectControl.AddOnFinished(PlayerSelectorFinished);
				PlayerSelector = playerSelectControl;
			}
		}
		PlayerSelect(player);
	}

	private void PlayerSelectorFinished()
	{
		PlayerSelector = null;
	}

	private void OnSelectPlayer(PlayerInfo playerInfo)
	{
		PlayerSelect(playerInfo);
	}

	public void SetPlayer(PlayerInfo player)
	{
		_input.value = string.Empty;
		PlayerSelect(player);
	}

	private void PlayerSelect(PlayerInfo player, bool refreshValidSprite = true)
	{
		PlayerInfo player2 = Player;
		if (player != null && player.Valid)
		{
			_input.value = player.Name;
		}
		Player = player;
		if (player2 != player && SelectPlayerChanged != null)
		{
			SelectPlayerChanged(Player);
		}
		if (refreshValidSprite)
		{
			RefreshValidSprite();
		}
		else
		{
			((Component)_validSprite).gameObject.SetActive(false);
		}
	}

	private void RefreshValidSprite()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		string spriteName;
		Color color;
		if (Player == null || !Player.Valid)
		{
			spriteName = "icon_check_cancel";
			color = UIManager.UIRed;
		}
		else
		{
			spriteName = "icon_check_ok";
			color = UIManager.UIGreen;
		}
		((Component)_validSprite).gameObject.SetActive(!string.IsNullOrEmpty(_input.value));
		_validSprite.spriteName = spriteName;
		UIUtility.ResizeToSquare(_validSprite, _input.label.fontSize);
		_validSprite.color = color;
		Vector3 localPosition = ((Component)_validSprite).transform.localPosition;
		localPosition.x = _input.label.printedSize.x + 15f;
		((Component)_validSprite).transform.localPosition = localPosition;
		if (((Component)_validSprite).gameObject.activeSelf && Player == null)
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(string.Empty, T._("존재하지 않는 플레이어 입니다"));
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Sign = 1;
			widgetTooltipControl.Show((UIWidget)_input.label, Vector2.zero, 5f);
		}
	}
}
