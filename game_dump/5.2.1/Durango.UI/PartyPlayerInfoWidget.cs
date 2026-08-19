using System;
using Durango.Logic;
using Durango.Logic.Party;
using Durango.Player;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class PartyPlayerInfoWidget : UIWidget
{
	public enum ActionMode
	{
		None,
		CancelInvitaion,
		ElectLeader
	}

	[SerializeField]
	private GameObject _info;

	[SerializeField]
	private GameObject _wait;

	[SerializeField]
	private GameObject _kickButton;

	[SerializeField]
	private UILabel _playerName;

	[SerializeField]
	private UILabel _levelAndFreq;

	[SerializeField]
	private GameObject _offline;

	[SerializeField]
	private UIWidget _gauge;

	[SerializeField]
	private UISprite _life;

	[SerializeField]
	private UISprite _stamina;

	[SerializeField]
	private GameObject _upper;

	[SerializeField]
	private UITexture _preview;

	[SerializeField]
	private GameObject _loadingRing;

	[SerializeField]
	private GameObject _empty;

	[SerializeField]
	private GameObject _contents;

	[SerializeField]
	private GameObject _action;

	[SerializeField]
	private SelectableButton _actionButton;

	[SerializeField]
	private GameObject _bottom;

	[SerializeField]
	private UILabel _clandAndRegion;

	[SerializeField]
	private GameObject _deadBg;

	private Member _member;

	private bool _electLeader;

	private ActionMode _mode;

	private UIModelRender _uiModelRender;

	private PlayerBehavior _previewModel;

	private bool _isAlive;

	public string EntityId
	{
		get
		{
			if (_member != null)
			{
				return _member.EntityId;
			}
			return string.Empty;
		}
	}

	public event Action<string> Kicked;

	public event Action<string, ActionMode> ButtonClicked;

	public event Action<PartyPlayerInfoWidget> Clicked;

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying && base.parent != null)
		{
			Vector3 vector = base.parent.localCorners[2] - base.parent.localCorners[0];
			base.width = (int)(vector.x * 0.2f);
			base.height = (int)vector.y;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (Application.isPlaying)
		{
			UIEventListener uIEventListener = UIEventListener.Get(_kickButton);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(KickButton_Clicked));
			UIEventListener uIEventListener2 = UIEventListener.Get(_upper);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(Upper_Clicked));
			UIEventListener uIEventListener3 = UIEventListener.Get(_preview.gameObject);
			uIEventListener3.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener3.onDrag, new UIEventListener.VectorDelegate(Preview_Drag));
			SelectableButton actionButton = _actionButton;
			actionButton.Clicked = (Action)Delegate.Combine(actionButton.Clicked, new Action(ActionButton_Clicked));
			_actionButton.CanClickWhenDisabled = true;
		}
	}

	protected override void OnUpdate()
	{
		base.OnStart();
		if (!Application.isPlaying || _member == null)
		{
			return;
		}
		_life.fillAmount = _member.Life;
		_stamina.fillAmount = _member.Stamina;
		UpdateOffline(_member.IsOffline);
		bool isAlive = _member.IsAlive;
		_deadBg.SetActive(!isAlive);
		if (_isAlive == isAlive)
		{
			return;
		}
		_isAlive = isAlive;
		if (_previewModel != null)
		{
			_previewModel.SetAlive(isAlive, fromInit: true);
			if (!isAlive)
			{
				_previewModel.transform.localRotation = Quaternion.Euler(0f, GetDeadAngle(_previewModel.IsMale), 0f);
			}
		}
	}

	private int GetDeadAngle(bool isMale)
	{
		if (!isMale)
		{
			return 0;
		}
		return 45;
	}

	private void KickButton_Clicked(GameObject go)
	{
		if (this.Kicked != null)
		{
			this.Kicked(EntityId);
		}
	}

	private void Upper_Clicked(GameObject go)
	{
		PlayerInfoPopup.RequestShow(EntityId);
	}

	private void Preview_Drag(GameObject go, Vector2 delta)
	{
		if (_previewModel != null && _isAlive)
		{
			Transform obj = _previewModel.transform;
			obj.Rotate(obj.up, 0f - delta.x, Space.World);
		}
	}

	private void ActionButton_Clicked()
	{
		if (_actionButton.Disabled)
		{
			UIManager.SystemMsg(T._("파티장만 이용가능한 기능입니다."));
		}
		else if (this.ButtonClicked != null)
		{
			this.ButtonClicked(EntityId, _mode);
		}
	}

	public void Set(Member member)
	{
		if (_member != null)
		{
			_member.PlayerInfoUpdated -= UpdatePlayerInfo;
		}
		_member = member;
		_isAlive = _member != null && _member.IsAlive;
		if (_member == null)
		{
			SetEmpty();
			return;
		}
		_member.PlayerInfoUpdated += UpdatePlayerInfo;
		_empty.SetActive(value: false);
		_contents.SetActive(value: true);
		UpdateOffline(_member.IsOffline);
		UpdateActivation();
		UpdatePlayerInfo(_member.PlayerInfo);
	}

	private void SetEmpty()
	{
		_empty.SetActive(GameSystem<PartySystem>.Instance().IsLeader);
		_contents.SetActive(value: false);
		_preview.mainTexture = null;
		UIModelRenderBuilder.Release(_uiModelRender);
		_uiModelRender = null;
		_previewModel = null;
		_deadBg.SetActive(value: false);
	}

	public void ToggleElectLeader(bool electLeader)
	{
		_electLeader = electLeader;
		UpdateActivation();
	}

	private void UpdateOffline(bool isOffline)
	{
		_offline.SetActive(isOffline);
		_gauge.alpha = ((!isOffline) ? 1f : 0.4f);
	}

	private void UpdateActivation()
	{
		if (_member != null)
		{
			bool isAccepted = _member.IsAccepted;
			bool isLeader = _member.IsLeader;
			bool isLeader2 = GameSystem<PartySystem>.Instance().IsLeader;
			_gauge.gameObject.SetActive(isAccepted);
			ActionMode mode = ChooseActionMode(isLeader, isAccepted);
			SetActionMode(isLeader2, mode);
			_wait.SetActive(!isAccepted);
			_kickButton.SetActive(isAccepted && !isLeader && isLeader2);
			_bottom.SetActive(isAccepted);
		}
	}

	private ActionMode ChooseActionMode(bool isLeader, bool isAccepted)
	{
		if (!isLeader)
		{
			if (!isAccepted)
			{
				return ActionMode.CancelInvitaion;
			}
			if (_electLeader)
			{
				return ActionMode.ElectLeader;
			}
		}
		return ActionMode.None;
	}

	private void SetActionMode(bool hasAuth, ActionMode mode)
	{
		_mode = mode;
		_action.SetActive(mode != ActionMode.None);
		_actionButton.Text = ((mode != ActionMode.ElectLeader) ? T._("초대 취소") : T._("파티장 위임"));
		_actionButton.SetStyle((mode != ActionMode.ElectLeader) ? PresetButton.Style.Border : PresetButton.Style.Solid);
		_actionButton.Disabled = !hasAuth;
	}

	private void UpdatePlayerInfo([CanBeNull] PlayerInfo info)
	{
		if (info == null || !info.Valid)
		{
			_loadingRing.SetActive(value: true);
			_info.SetActive(value: false);
			return;
		}
		_info.SetActive(value: true);
		_loadingRing.SetActive(value: false);
		string text = info.Name;
		if (_member.IsLeader)
		{
			text = "[icon=crown] " + text;
		}
		_playerName.text = text;
		_levelAndFreq.text = T._("{0:lv:} | #{1:0000} [size=18]kHz", info.Level, info.Freq);
		string text2 = T._("[icon=icon_popup_player_island] {0}", _member.RegionName);
		if (info.HasClan)
		{
			text2 = T._("[icon=icon_mainhud_guild] {0}\n", info.ClanName) + text2;
		}
		_clandAndRegion.text = text2;
		SetPreviewModel(info);
	}

	private void SetPreviewModel(PlayerInfo info)
	{
		if (_uiModelRender == null)
		{
			_uiModelRender = UIModelRenderBuilder.Make();
		}
		if (_previewModel == null || _previewModel.IsMale != info.IsMale)
		{
			PlayerManager playerManager = Singleton<PlayerManager>.Instance();
			bool isMale = info.IsMale;
			float yaw = ((!_isAlive) ? GetDeadAngle(info.IsMale) : 180);
			_previewModel = playerManager.MakePreview(isMale, null, yaw);
			_uiModelRender.SetModel(_previewModel.gameObject, 35f);
		}
		PlayerManager.SetDisplay(_previewModel, info.Display);
		_previewModel.SetAlive(_isAlive, fromInit: true);
		_uiModelRender.FillTexture(_preview);
	}

	[UsedImplicitly]
	private void OnClick()
	{
		if (this.Clicked != null)
		{
			this.Clicked(this);
		}
	}
}
